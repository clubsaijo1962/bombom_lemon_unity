using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.Network;
using BomBomLemon.PlayerSetup;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

namespace BomBomLemon.Multiplayer
{
    /// <summary>
    /// 協力モード：ラウンド結果画面。
    /// 全員の予想と実際の数字・差を表示し、ヘルプカード使用の決定、次ラウンドへの進行を担う。
    /// </summary>
    public class MultiResultController : MonoBehaviour
    {
        [Header("ラウンド情報")]
        [SerializeField] TextMeshProUGUI roundHeaderLabel;
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI actualNumberLabel;
        [SerializeField] TextMeshProUGUI deciderNameLabel;

        [Header("自分の秘密（常時表示）")]
        [SerializeField] TextMeshProUGUI myTopicLabel;
        [SerializeField] TextMeshProUGUI mySecretLabel;

        [Header("予想リスト")]
        [SerializeField] RectTransform   guessListContent;
        [SerializeField] TMP_FontAsset   listFont;

        [Header("ライフ・ヘルプ表示")]
        [SerializeField] TextMeshProUGUI livesLabel;
        [SerializeField] TextMeshProUGUI helpCardsLabel;

        [Header("ヘルプカードボタン（最終決定者のみ）")]
        [SerializeField] GameObject      helpCardPanel;
        [SerializeField] Button          useHelpCardBtn;
        [SerializeField] TextMeshProUGUI useHelpCardBtnLabel;
        [SerializeField] TextMeshProUGUI helpCardUsedLabel;

        [Header("次ラウンドボタン（ホストのみ）")]
        [SerializeField] GameObject      hostPanel;
        [SerializeField] Button          nextRoundBtn;
        [SerializeField] TextMeshProUGUI nextRoundBtnLabel;

        [Header("ヘルプカード変換ダイアログ（ホストのみ）")]
        [SerializeField] GameObject      helpConvertPanel;
        [SerializeField] TextMeshProUGUI helpConvertLabel;
        [SerializeField] Button          convertBtn;
        [SerializeField] Button          skipConvertBtn;

        [Header("ゲームクリア オーバーレイ")]
        [SerializeField] GameObject      gameClearOverlay;
        [SerializeField] TextMeshProUGUI gameClearLabel;
        [SerializeField] Button          gameClearBackBtn;

        [Header("ゲームオーバー オーバーレイ")]
        [SerializeField] GameObject      gameOverOverlay;
        [SerializeField] TextMeshProUGUI gameOverLabel;
        [SerializeField] Button          gameOverBackBtn;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        bool _loadingNext;
        bool _helpCardDecided;

        // HelpConvert ダイアログ用の一時保存
        int _pendingNewLives;
        int _pendingNewHelps;
        int _pendingNextRound;

        const float SlotHeight = 88f;
        const float SlotGap    =  8f;

        static readonly Color ColPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color ColMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color ColGreen    = new(0.10f, 0.42f, 0.12f);
        static readonly Color ColRed      = new(0.72f, 0.12f, 0.10f);
        static readonly Color ColGold     = new(0.85f, 0.55f, 0.00f);

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            // VLG / CSF 無効化
            if (guessListContent != null)
            {
                var csf = guessListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (csf) csf.enabled = false;
                var vlg = guessListContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (vlg) vlg.enabled = false;
            }

            ApplyLanguage();
            ShowResult();
            SetupRoleUI();

            // オーバーレイを最初は非表示
            if (gameClearOverlay) gameClearOverlay.SetActive(false);
            if (gameOverOverlay)  gameOverOverlay.SetActive(false);
            if (helpConvertPanel) helpConvertPanel.SetActive(false);

            LobbyManager.Instance.OnPlayersUpdated  += HandlePlayersUpdated;
            LobbyManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            LobbyManager.Instance.OnGameClear        += HandleGameClear;
            LobbyManager.Instance.OnGameOver         += HandleGameOver;
            LobbyManager.Instance.OnLobbyDeleted     += HandleLobbyDeleted;

            useHelpCardBtn?.onClick.AddListener(OnUseHelpCard);
            nextRoundBtn?.onClick.AddListener(OnNextRound);
            convertBtn?.onClick.AddListener(OnHelpConvert);
            skipConvertBtn?.onClick.AddListener(OnSkipHelpConvert);
            gameClearBackBtn?.onClick.AddListener(OnBackToLobby);
            gameOverBackBtn?.onClick.AddListener(OnBackToLobby);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayersUpdated   -= HandlePlayersUpdated;
                LobbyManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                LobbyManager.Instance.OnGameClear        -= HandleGameClear;
                LobbyManager.Instance.OnGameOver         -= HandleGameOver;
                LobbyManager.Instance.OnLobbyDeleted     -= HandleLobbyDeleted;
            }
        }

        // ── 多言語適用 ────────────────────────────────────────────────────────
        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (useHelpCardBtnLabel) useHelpCardBtnLabel.text = en ? "Use Help Card 🃏" : "ヘルプカード使用 🃏";
            if (nextRoundBtnLabel)   nextRoundBtnLabel.text   = en ? "Next Round ▶"    : "次のラウンドへ ▶";
            if (gameClearLabel)      gameClearLabel.text      = en ? "🎉 Game Clear! 🎉" : "🎉 ゲームクリア！ 🎉";
            if (gameOverLabel)       gameOverLabel.text       = en ? "Game Over..." : "もう少しだった...";
        }

        // ── 結果表示 ──────────────────────────────────────────────────────────
        void ShowResult()
        {
            bool en = LanguageSettings.IsEnglish;
            int round  = RoomConfig.CurrentRound;
            int total  = RoomConfig.TotalRounds;
            int actual = RoomConfig.FinalConfirmedNumber;

            var players = LobbyManager.Instance.CurrentLobby?.Players;

            // ラウンドヘッダー
            if (roundHeaderLabel)
                roundHeaderLabel.text = en
                    ? $"Round {round + 1} / {total}"
                    : $"ラウンド {round + 1} / {total}";

            // お題
            if (topicLabel) topicLabel.text = RoomConfig.GameTopic;

            // 実際の数字
            if (actualNumberLabel) actualNumberLabel.text = actual.ToString();

            // 最終決定者名
            string deciderName = GetPlayerName(players, RoomConfig.DeciderIndex);
            if (deciderNameLabel)
                deciderNameLabel.text = en
                    ? $"Final Decider: {deciderName}"
                    : $"最終決定者：{deciderName}";

            // 自分の秘密
            string myTopic = TopicDatabase.GetTopic(RoomConfig.GameSeed, RoomConfig.PlayerIndex);
            if (myTopicLabel)  myTopicLabel.text  = myTopic;
            if (mySecretLabel) mySecretLabel.text = RoomConfig.MySecretNumber.ToString();

            // ライフ・ヘルプ更新
            UpdateStatusLabels();

            // 予想リスト構築
            if (players != null) RebuildGuessList(players, actual);
        }

        void UpdateStatusLabels()
        {
            bool en = LanguageSettings.IsEnglish;
            if (livesLabel)
                livesLabel.text = en
                    ? $"❤ Lives: {RoomConfig.Lives}"
                    : $"❤ ライフ：{RoomConfig.Lives}";
            if (helpCardsLabel)
                helpCardsLabel.text = en
                    ? $"🃏 Help Cards: {RoomConfig.HelpCards}"
                    : $"🃏 ヘルプカード：{RoomConfig.HelpCards}";
        }

        // ── 役割別UIセットアップ ─────────────────────────────────────────────
        void SetupRoleUI()
        {
            bool isDecider = RoomConfig.PlayerIndex == RoomConfig.DeciderIndex;
            bool isHost    = RoomConfig.IsHost;

            // ヘルプカードパネル（最終決定者のみ）
            if (helpCardPanel)
            {
                helpCardPanel.SetActive(isDecider);
                UpdateHelpCardUI();
            }

            // ホストパネル
            if (hostPanel) hostPanel.SetActive(isHost);
        }

        void UpdateHelpCardUI()
        {
            bool en = LanguageSettings.IsEnglish;
            bool used = RoomConfig.HelpCardUsedThisRound;
            bool canUse = RoomConfig.HelpCards > 0 && !used;

            if (useHelpCardBtn)  useHelpCardBtn.interactable = canUse;
            if (helpCardUsedLabel)
            {
                helpCardUsedLabel.gameObject.SetActive(used);
                helpCardUsedLabel.text = en ? "✓ Help Card Used!" : "✓ ヘルプカード使用済み！";
            }
        }

        // ── 予想リスト構築 ────────────────────────────────────────────────────
        void RebuildGuessList(List<LobbyPlayer> players, int actualNumber)
        {
            if (guessListContent == null) return;
            for (int i = guessListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(guessListContent.GetChild(i).gameObject);

            int n = players.Count;
            guessListContent.sizeDelta = new Vector2(0f, n > 0 ? n * SlotHeight + (n - 1) * SlotGap : 0f);

            for (int i = 0; i < n; i++)
            {
                var p = players[i];
                string name = GetPlayerName(players, i);
                bool hasGuess = p.Data != null && p.Data.TryGetValue("Guess", out var gd) &&
                                !string.IsNullOrEmpty(gd.Value) && int.TryParse(gd.Value, out _);
                int guessVal  = 0;
                string guess  = "---";
                if (hasGuess && p.Data.TryGetValue("Guess", out var gd2))
                {
                    int.TryParse(gd2.Value, out guessVal);
                    guess = gd2.Value;
                }

                int diff = hasGuess ? Mathf.Abs(guessVal - actualNumber) : -1;
                string badge = i == RoomConfig.AnswererIndex ? "[回]"
                             : i == RoomConfig.DeciderIndex  ? "[決]" : "";
                bool isMe = p.Id == RoomConfig.LocalPlayerId;

                float yTop = -(i * (SlotHeight + SlotGap));
                AddResultSlot(name, badge, guess, diff, yTop, isMe, hasGuess);
            }
        }

        void AddResultSlot(string name, string badge, string guess, int diff, float yTop, bool isMe, bool hasGuess)
        {
            var go = new GameObject("ResultSlot", typeof(RectTransform));
            go.transform.SetParent(guessListContent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0f, SlotHeight);
            r.anchoredPosition = new Vector2(0f, yTop);

            var bg = go.AddComponent<Image>();
            bg.color = isMe ? new Color(1f, 0.97f, 0.85f, 0.92f) : new Color(0.96f, 0.96f, 0.96f, 0.55f);
            bg.raycastTarget = false;

            // 名前（左30%）
            AddSlotLabel(go.transform, "Name", $"{badge}{name}",
                new Vector2(0f, 0f), new Vector2(0.35f, 1f),
                new Vector2(14f, 4f), new Vector2(-4f, -4f),
                32f, TextAlignmentOptions.MidlineLeft, ColPrimary);

            // 予想数字（中35%）
            AddSlotLabel(go.transform, "Guess", guess,
                new Vector2(0.35f, 0f), new Vector2(0.65f, 1f),
                new Vector2(4f, 4f), new Vector2(-4f, -4f),
                44f, TextAlignmentOptions.Center,
                hasGuess ? ColPrimary : ColMuted);

            // 差（右35%）
            string diffText = diff >= 0 ? $"差：{diff}" : "---";
            Color diffColor = diff < 0 ? ColMuted
                            : diff == 0 ? ColGold
                            : diff <= 10 ? ColGreen
                            : diff <= 25 ? ColPrimary
                            : ColRed;
            AddSlotLabel(go.transform, "Diff", diffText,
                new Vector2(0.65f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-14f, -4f),
                36f, TextAlignmentOptions.MidlineRight, diffColor);
        }

        void AddSlotLabel(Transform parent, string goName, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            float fontSize, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(goName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin; r.anchorMax = anchorMax;
            r.offsetMin = offsetMin; r.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize;
            tmp.alignment = align; tmp.color = color;
            tmp.enableWordWrapping = false; tmp.raycastTarget = false;
            if (listFont != null) tmp.font = listFont;
        }

        string GetPlayerName(List<LobbyPlayer> players, int index)
        {
            if (players == null || index < 0 || index >= players.Count) return "?";
            var p = players[index];
            if (p.Data != null && p.Data.TryGetValue("Name", out var d)) return d.Value;
            return "?";
        }

        // ── UGS イベントハンドラ ──────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players)
        {
            // ライフ・ヘルプカード更新（ポーリングで最新値がRoomConfigに反映済み）
            UpdateStatusLabels();
            UpdateHelpCardUI();

            // リスト再構築（ゲスト用: Guess はリスト目的で残す必要なし）
        }

        void HandleGameStateChanged(string state)
        {
            if (_loadingNext) return;
            if (state == "Playing")
            {
                // ホスト以外: 次ラウンドへ（ホストは自分で遷移）
                if (!RoomConfig.IsHost)
                {
                    _loadingNext = true;
                    StartCoroutine(LoadWithFade("MultiGame"));
                }
            }
        }

        void HandleGameClear()
        {
            if (gameClearOverlay) gameClearOverlay.SetActive(true);
            bool en = LanguageSettings.IsEnglish;
            if (gameClearLabel)
                gameClearLabel.text = en
                    ? $"🎉 Game Clear! 🎉\nLives Remaining: {RoomConfig.Lives}"
                    : $"🎉 ゲームクリア！ 🎉\n残りライフ：{RoomConfig.Lives}";
        }

        void HandleGameOver()
        {
            if (gameOverOverlay) gameOverOverlay.SetActive(true);
        }

        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── ヘルプカードボタン（最終決定者）────────────────────────────────────
        void OnUseHelpCard()
        {
            if (RoomConfig.HelpCardUsedThisRound) return;
            _ = UseHelpCardAsync();
        }

        async System.Threading.Tasks.Task UseHelpCardAsync()
        {
            if (useHelpCardBtn) useHelpCardBtn.interactable = false;
            try
            {
                await LobbyManager.Instance.UseHelpCardAsync();
                UpdateHelpCardUI();
                UpdateStatusLabels();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiResult] UseHelpCard: {e.Message}");
                if (useHelpCardBtn) useHelpCardBtn.interactable = true;
            }
        }

        // ── 次のラウンドへ（ホスト）──────────────────────────────────────────
        void OnNextRound()
        {
            if (!RoomConfig.IsHost || _loadingNext) return;
            if (nextRoundBtn) nextRoundBtn.interactable = false;
            _ = NextRoundAsync();
        }

        async System.Threading.Tasks.Task NextRoundAsync()
        {
            bool helpUsed = RoomConfig.HelpCardUsedThisRound;
            int newLives  = RoomConfig.Lives - (helpUsed ? 0 : 1);
            int newHelps  = RoomConfig.HelpCards;
            int nextRound = RoomConfig.CurrentRound + 1;
            int total     = RoomConfig.TotalRounds;

            // ゲーム終了判定
            if (nextRound >= total)
            {
                // 全ラウンド終了
                _loadingNext = true;
                try
                {
                    await LobbyManager.Instance.EndGameAsync(newLives > 0);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[MultiResult] EndGameAsync: {e.Message}");
                    StartCoroutine(LoadWithFade("PlayerSetup"));
                }
                if (newLives > 0) HandleGameClear(); else HandleGameOver();
                return;
            }

            // ライフ切れ
            if (newLives <= 0)
            {
                _loadingNext = true;
                try { await LobbyManager.Instance.EndGameAsync(false); }
                catch { }
                HandleGameOver();
                return;
            }

            // 最終ラウンド直前 かつ ヘルプカード残あり → HelpConvert ダイアログ
            if (nextRound == total - 1 && newHelps > 0)
            {
                _pendingNewLives  = newLives;
                _pendingNewHelps  = newHelps;
                _pendingNextRound = nextRound;
                ShowHelpConvertPanel(newHelps, newLives);
                if (nextRoundBtn) nextRoundBtn.interactable = true;
                return;
            }

            // 通常の次ラウンド
            try { await AdvanceRoundAsync(newLives, newHelps, nextRound); }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiResult] AdvanceRound: {e.Message}");
                if (nextRoundBtn) nextRoundBtn.interactable = true;
            }
        }

        async System.Threading.Tasks.Task AdvanceRoundAsync(int newLives, int newHelps, int nextRound)
        {
            var order      = RoomConfig.DeciderOrder;
            int count      = order.Length;
            int deciderIdx = order[nextRound % count];
            int answererIdx= order[(nextRound + 1) % count];
            string topic   = TopicDatabase.GetTopic(RoomConfig.GameSeed, deciderIdx);

            await LobbyManager.Instance.AdvanceToNextRoundAsync(
                newLives, newHelps, nextRound, answererIdx, deciderIdx, topic);

            _loadingNext = true;
            StartCoroutine(LoadWithFade("MultiGame"));
        }

        // ── ヘルプカード変換ダイアログ ─────────────────────────────────────────
        void ShowHelpConvertPanel(int helpCount, int currentLives)
        {
            if (helpConvertPanel == null) return;
            bool en = LanguageSettings.IsEnglish;
            if (helpConvertLabel)
                helpConvertLabel.text = en
                    ? $"Last Round! Convert {helpCount} Help Card(s) to {helpCount} Life(s)?"
                    : $"最終ラウンド！\nヘルプカード × {helpCount} 枚 → ライフ × {helpCount} 個に変換しますか？\n（現在のライフ：{currentLives}）";
            helpConvertPanel.SetActive(true);
        }

        void OnHelpConvert()
        {
            if (helpConvertPanel) helpConvertPanel.SetActive(false);
            int newLives = _pendingNewLives + _pendingNewHelps;
            int newHelps = 0;
            _ = AdvanceRoundAsync(newLives, newHelps, _pendingNextRound);
        }

        void OnSkipHelpConvert()
        {
            if (helpConvertPanel) helpConvertPanel.SetActive(false);
            _ = AdvanceRoundAsync(_pendingNewLives, _pendingNewHelps, _pendingNextRound);
        }

        // ── ロビーに戻る ──────────────────────────────────────────────────────
        void OnBackToLobby()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            _ = BackToLobbyAsync();
        }

        async System.Threading.Tasks.Task BackToLobbyAsync()
        {
            try
            {
                if (RoomConfig.IsHost)
                    await LobbyManager.Instance.DissolveAsync();
                else
                    await LobbyManager.Instance.LeaveAsync();
            }
            catch { }
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── フェード ──────────────────────────────────────────────────────────
        IEnumerator FadeOverlayOut()
        {
            if (screenFade == null) yield break;
            float dur = 0.35f, t = 0f;
            while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(1f, 0f, t / dur); yield return null; }
            screenFade.alpha = 0f; screenFade.blocksRaycasts = false;
        }

        IEnumerator FadeContentIn()
        {
            if (!panelGroup) yield break;
            float dur = 0.30f, t = 0f;
            while (t < dur) { t += Time.deltaTime; panelGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
            panelGroup.alpha = 1f;
        }

        IEnumerator LoadWithFade(string sceneName)
        {
            if (screenFade != null)
            {
                screenFade.blocksRaycasts = true;
                float dur = 0.28f, t = 0f;
                while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
                screenFade.alpha = 1f;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
