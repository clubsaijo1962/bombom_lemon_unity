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
    /// チームバトル：ゲーム進行画面。
    /// 回答者・最終決定者（アクティブチーム）と観戦者（相手チーム）に分かれる。
    /// 自分のお題・秘密の数字は常時表示。ダブルプレイヤーは2組表示。
    /// </summary>
    public class TeamGameController : MonoBehaviour
    {
        public enum PlayerRole { Answerer, Decider, ActiveGuesser, Observer }

        [Header("お題・役割表示")]
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI answererNameLabel;
        [SerializeField] TextMeshProUGUI deciderNameLabel;
        [SerializeField] TextMeshProUGUI guideLabel;
        [SerializeField] TextMeshProUGUI myRoleLabel;

        [Header("チーム情報")]
        [SerializeField] TextMeshProUGUI teamALabel;
        [SerializeField] TextMeshProUGUI teamBLabel;
        [SerializeField] TextMeshProUGUI activeTeamBadge;

        [Header("回答表示")]
        [SerializeField] TextMeshProUGUI answerDisplayLabel;

        [Header("予想リスト")]
        [SerializeField] RectTransform   guessListContent;
        [SerializeField] TMP_FontAsset   listFont;
        [SerializeField] TextMeshProUGUI guessSectionHeader;

        [Header("回答者パネル（自分が回答者のときのみ表示）")]
        [SerializeField] GameObject      answererPanel;
        [SerializeField] TMP_InputField  answerInputField;
        [SerializeField] Button          submitAnswerBtn;
        [SerializeField] TextMeshProUGUI submitAnswerBtnLabel;

        [Header("予想者パネル（アクティブチームの非回答者・非決定者）")]
        [SerializeField] GameObject      guesserPanel;
        [SerializeField] TMP_InputField  guessInputField;
        [SerializeField] Button          submitGuessBtn;
        [SerializeField] TextMeshProUGUI submitGuessBtnLabel;

        [Header("最終決定者パネル")]
        [SerializeField] GameObject      deciderPanel;
        [SerializeField] TMP_InputField  secretInputField;
        [SerializeField] Button          finalDecideBtn;
        [SerializeField] TextMeshProUGUI finalDecideBtnLabel;

        [Header("観戦パネル（相手チームは入力不可）")]
        [SerializeField] GameObject      observerPanel;
        [SerializeField] TextMeshProUGUI observerStatusLabel;

        [Header("自分の秘密（常時表示）")]
        [SerializeField] TextMeshProUGUI myTopicLabel;
        [SerializeField] TextMeshProUGUI mySecretLabel;
        [SerializeField] TextMeshProUGUI roundLabel;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        PlayerRole _myRole;
        bool       _loadingNext;

        const float SlotHeight = 80f;
        const float SlotGap    =  8f;

        static readonly Color ColPrimary = new(0.20f, 0.10f, 0.02f);
        static readonly Color ColMuted   = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color ColGreen   = new(0.10f, 0.42f, 0.12f);
        static readonly Color ColTeamA   = new(0.15f, 0.35f, 0.75f);
        static readonly Color ColTeamB   = new(0.75f, 0.20f, 0.15f);

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            // VLG / CSF を無効化（手動配置）
            if (guessListContent != null)
            {
                var csf = guessListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (csf) csf.enabled = false;
                var vlg = guessListContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (vlg) vlg.enabled = false;
            }

            DetermineRole();
            ApplyLanguage();
            ShowAnnouncement();
            ShowMyCard();

            LobbyManager.Instance.OnPlayersUpdated += HandlePlayersUpdated;
            LobbyManager.Instance.OnGameFinalized  += HandleGameFinalized;
            LobbyManager.Instance.OnLobbyDeleted   += HandleLobbyDeleted;

            submitAnswerBtn?.onClick.AddListener(OnSubmitAnswer);
            submitGuessBtn?.onClick.AddListener(OnSubmitGuess);
            finalDecideBtn?.onClick.AddListener(OnFinalDecide);

            // 最終決定者パネル: 秘密の数字をプリセット
            if (_myRole == PlayerRole.Decider && secretInputField != null)
            {
                bool isExtraRound = IsDoubledPlayerExtraRound();
                secretInputField.text = isExtraRound
                    ? RoomConfig.MySecondSecretNumber.ToString()
                    : RoomConfig.MySecretNumber.ToString();
            }

            var players = LobbyManager.Instance.CurrentLobby?.Players ?? new List<LobbyPlayer>();
            HandlePlayersUpdated(players);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayersUpdated -= HandlePlayersUpdated;
                LobbyManager.Instance.OnGameFinalized  -= HandleGameFinalized;
                LobbyManager.Instance.OnLobbyDeleted   -= HandleLobbyDeleted;
            }
        }

        // ── 役割決定 ──────────────────────────────────────────────────────────
        void DetermineRole()
        {
            int myIdx = RoomConfig.PlayerIndex;
            bool isActive = RoomConfig.ActiveTeam == "A"
                ? System.Array.IndexOf(RoomConfig.TeamA, myIdx) >= 0
                : System.Array.IndexOf(RoomConfig.TeamB, myIdx) >= 0;

            if (!isActive)
                _myRole = PlayerRole.Observer;
            else if (myIdx == RoomConfig.AnswererIndex)
                _myRole = PlayerRole.Answerer;
            else if (myIdx == RoomConfig.DeciderIndex)
                _myRole = PlayerRole.Decider;
            else
                _myRole = PlayerRole.ActiveGuesser;
        }

        bool IsDoubledPlayerExtraRound()
        {
            return RoomConfig.DoubledPlayerIndex == RoomConfig.PlayerIndex
                && RoomConfig.DoubledPlayerIndex >= 0
                && RoomConfig.CurrentRound == RoomConfig.TotalRounds - 1;
        }

        // ── 多言語・役割別UI ──────────────────────────────────────────────────
        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;

            // 役割パネル
            if (answererPanel) answererPanel.SetActive(_myRole == PlayerRole.Answerer);
            if (guesserPanel)  guesserPanel.SetActive(_myRole == PlayerRole.ActiveGuesser);
            if (deciderPanel)  deciderPanel.SetActive(_myRole == PlayerRole.Decider);
            if (observerPanel) observerPanel.SetActive(_myRole == PlayerRole.Observer);

            // ボタンラベル
            if (submitAnswerBtnLabel) submitAnswerBtnLabel.text = en ? "Share ▶"      : "回答を送る ▶";
            if (submitGuessBtnLabel)  submitGuessBtnLabel.text  = en ? "Guess ▶"      : "予想を送る ▶";
            if (finalDecideBtnLabel)  finalDecideBtnLabel.text  = en ? "Finalize ✓"  : "最終決定 ✓";
            if (guessSectionHeader)   guessSectionHeader.text   = en ? "Team Guesses" : "チームの予想";

            // ガイドテキスト（役割別）
            string guide = _myRole switch
            {
                PlayerRole.Answerer       => en ? "You're the Answerer! Share your honest feeling." : "あなたが回答者！お題に対する正直な感覚を答えてください。",
                PlayerRole.Decider        => en ? "You're the Final Decider! Enter your secret number." : "あなたが最終決定者！秘密の数字を入力して確定してください。",
                PlayerRole.ActiveGuesser  => en ? "Guess the Decider's secret number!" : "最終決定者の秘密の数字を予想しよう！",
                _                         => en ? "👀 Observing the other team..." : "👀 相手チームを観戦中...",
            };
            if (guideLabel) guideLabel.text = guide;

            // 役割バッジ
            string badge = _myRole switch
            {
                PlayerRole.Answerer      => en ? "🎤 Answerer"      : "🎤 あなたは回答者",
                PlayerRole.Decider       => en ? "🔐 Final Decider" : "🔐 あなたは最終決定者",
                PlayerRole.ActiveGuesser => en ? "🎯 Guesser"       : "🎯 あなたは予想者",
                _                        => en ? "👀 Observer"       : "👀 あなたは観戦中",
            };
            if (myRoleLabel) myRoleLabel.text = badge;

            // 観戦者ステータス
            if (observerStatusLabel)
                observerStatusLabel.text = en
                    ? "Your team rests this round.\nWatch and learn!"
                    : "あなたのチームは今ラウンドお休みです。\n相手の戦略を観察しよう！";

            // InputField プレースホルダー
            SetPlaceholder(answerInputField, en ? "Enter your answer..." : "回答を入力...");
            SetPlaceholder(guessInputField,  en ? "Guess (1-99)"         : "予想の数字 (1〜99)");
            SetPlaceholder(secretInputField, en ? "Your secret number"   : "秘密の数字");
        }

        // ── 役割発表 ─────────────────────────────────────────────────────────
        void ShowAnnouncement()
        {
            var players = LobbyManager.Instance.CurrentLobby?.Players;
            bool en = LanguageSettings.IsEnglish;

            if (topicLabel)        topicLabel.text        = RoomConfig.GameTopic;
            if (answererNameLabel) answererNameLabel.text  = en
                ? $"Answerer: {GetPlayerName(players, RoomConfig.AnswererIndex)}"
                : $"回答者：{GetPlayerName(players, RoomConfig.AnswererIndex)}";
            if (deciderNameLabel)  deciderNameLabel.text   = en
                ? $"Final Decider: {GetPlayerName(players, RoomConfig.DeciderIndex)}"
                : $"最終決定者：{GetPlayerName(players, RoomConfig.DeciderIndex)}";

            // チームラベル
            UpdateTeamLabels(players);
        }

        void UpdateTeamLabels(List<LobbyPlayer> players)
        {
            bool en = LanguageSettings.IsEnglish;
            string aNames = BuildTeamNameString(players, RoomConfig.TeamA);
            string bNames = BuildTeamNameString(players, RoomConfig.TeamB);
            if (teamALabel) { teamALabel.text  = en ? $"Team A: {aNames}" : $"チームA：{aNames}"; teamALabel.color = ColTeamA; }
            if (teamBLabel) { teamBLabel.text  = en ? $"Team B: {bNames}" : $"チームB：{bNames}"; teamBLabel.color = ColTeamB; }

            bool aActive = RoomConfig.ActiveTeam == "A";
            if (activeTeamBadge)
            {
                activeTeamBadge.text  = en ? $"▶ Team {RoomConfig.ActiveTeam} is playing" : $"▶ チーム{RoomConfig.ActiveTeam} のターン";
                activeTeamBadge.color = aActive ? ColTeamA : ColTeamB;
            }
        }

        string BuildTeamNameString(List<LobbyPlayer> players, int[] team)
        {
            if (players == null || team == null) return "";
            var sb = new System.Text.StringBuilder();
            foreach (int idx in team)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(GetPlayerName(players, idx));
            }
            return sb.ToString();
        }

        void ShowMyCard()
        {
            bool en = LanguageSettings.IsEnglish;
            bool isDoubled = RoomConfig.DoubledPlayerIndex == RoomConfig.PlayerIndex
                          && RoomConfig.DoubledPlayerIndex >= 0;

            string myTopic  = TopicDatabase.GetTopic(RoomConfig.GameSeed, RoomConfig.PlayerIndex);
            string mySecret = RoomConfig.MySecretNumber.ToString();

            if (isDoubled)
            {
                // 2組表示
                string t2 = RoomConfig.MySecondTopic;
                string s2 = RoomConfig.MySecondSecretNumber.ToString();
                if (myTopicLabel)  myTopicLabel.text  = $"{myTopic}  /  {t2}";
                if (mySecretLabel) mySecretLabel.text = $"🔒{mySecret}  /  🔒{s2}";
            }
            else
            {
                if (myTopicLabel)  myTopicLabel.text  = myTopic;
                if (mySecretLabel) mySecretLabel.text = mySecret;
            }

            if (roundLabel)
                roundLabel.text = en
                    ? $"Round {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}"
                    : $"ラウンド {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}";
        }

        // ── UGS イベント ─────────────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players)
        {
            bool en = LanguageSettings.IsEnglish;

            // 回答者の最新回答を表示
            if (RoomConfig.AnswererIndex >= 0 && RoomConfig.AnswererIndex < players.Count)
            {
                var ap = players[RoomConfig.AnswererIndex];
                string answer = "";
                if (ap.Data != null && ap.Data.TryGetValue("Answer", out var ad)) answer = ad.Value;
                if (answerDisplayLabel)
                    answerDisplayLabel.text = string.IsNullOrEmpty(answer)
                        ? (en ? "(No answer yet)" : "(まだ回答がありません)")
                        : answer;
            }

            RebuildGuessList(players);
        }

        void HandleGameFinalized(int finalNumber)
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("TeamResult"));
        }

        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── 予想リスト ────────────────────────────────────────────────────────
        void RebuildGuessList(List<LobbyPlayer> players)
        {
            if (guessListContent == null) return;
            for (int i = guessListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(guessListContent.GetChild(i).gameObject);

            bool en = LanguageSettings.IsEnglish;
            // アクティブチームのみ表示
            int[] activeTeam = RoomConfig.ActiveTeam == "A" ? RoomConfig.TeamA : RoomConfig.TeamB;
            int n = activeTeam?.Length ?? 0;
            float totalH = n > 0 ? n * SlotHeight + (n - 1) * SlotGap : 0f;
            guessListContent.sizeDelta = new Vector2(0f, totalH);

            for (int i = 0; i < n; i++)
            {
                int pIdx = activeTeam[i];
                if (pIdx < 0 || pIdx >= players.Count) continue;
                var p = players[pIdx];
                string name = GetPlayerName(players, pIdx);
                string guess = "---";
                if (p.Data != null && p.Data.TryGetValue("Guess", out var gd) &&
                    !string.IsNullOrEmpty(gd.Value))
                    guess = gd.Value;

                string badge = pIdx == RoomConfig.AnswererIndex ? (en ? "[A]" : "[回]")
                             : pIdx == RoomConfig.DeciderIndex  ? (en ? "[D]" : "[決]") : "";
                float yTop = -(i * (SlotHeight + SlotGap));
                bool isMe  = p.Id == RoomConfig.LocalPlayerId;
                AddGuessSlot($"{badge}{name}", guess, yTop, isMe);
            }
        }

        void AddGuessSlot(string name, string guess, float yTop, bool isMe)
        {
            var go = new GameObject("GuessSlot", typeof(RectTransform));
            go.transform.SetParent(guessListContent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0f, SlotHeight);
            r.anchoredPosition = new Vector2(0f, yTop);

            go.AddComponent<Image>().color = isMe
                ? new Color(1f, 0.97f, 0.85f, 0.92f)
                : new Color(0.96f, 0.96f, 0.96f, 0.55f);

            AddSlotLabel(go.transform, "Name", name,
                new Vector2(0f, 0f), new Vector2(0.58f, 1f),
                new Vector2(18f, 4f), new Vector2(-4f, -4f),
                36f, TextAlignmentOptions.MidlineLeft, ColPrimary);
            bool hasGuess = guess != "---" && !string.IsNullOrEmpty(guess);
            AddSlotLabel(go.transform, "Guess", guess,
                new Vector2(0.58f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-18f, -4f),
                46f, TextAlignmentOptions.MidlineRight,
                hasGuess ? ColGreen : ColMuted);
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

        // ── ボタンコールバック ────────────────────────────────────────────────
        void OnSubmitAnswer()
        {
            if (answerInputField == null || string.IsNullOrWhiteSpace(answerInputField.text)) return;
            if (submitAnswerBtn) submitAnswerBtn.interactable = false;
            _ = SendAnswerAsync(answerInputField.text.Trim());
        }

        async System.Threading.Tasks.Task SendAnswerAsync(string answer)
        {
            try   { await LobbyManager.Instance.UpdatePlayerAnswerAsync(answer); }
            catch (Exception e) { Debug.LogWarning($"[TeamGame] SendAnswer: {e.Message}"); }
            if (submitAnswerBtn) submitAnswerBtn.interactable = true;
        }

        void OnSubmitGuess()
        {
            if (guessInputField == null || string.IsNullOrWhiteSpace(guessInputField.text)) return;
            if (!int.TryParse(guessInputField.text, out int val) || val < 1 || val > 99) return;
            if (submitGuessBtn) submitGuessBtn.interactable = false;
            _ = SendGuessAsync(val.ToString());
        }

        async System.Threading.Tasks.Task SendGuessAsync(string guess)
        {
            try   { await LobbyManager.Instance.UpdatePlayerGuessAsync(guess); }
            catch (Exception e) { Debug.LogWarning($"[TeamGame] SendGuess: {e.Message}"); }
            if (submitGuessBtn) submitGuessBtn.interactable = true;
        }

        void OnFinalDecide()
        {
            if (secretInputField == null || string.IsNullOrWhiteSpace(secretInputField.text)) return;
            if (!int.TryParse(secretInputField.text, out int val) || val < 1 || val > 99) return;
            if (finalDecideBtn) finalDecideBtn.interactable = false;
            _ = FinalizeAsync(val);
        }

        async System.Threading.Tasks.Task FinalizeAsync(int secret)
        {
            try   { await LobbyManager.Instance.FinalizeTeamRoundAsync(secret); }
            catch (Exception e)
            {
                Debug.LogWarning($"[TeamGame] Finalize: {e.Message}");
                if (finalDecideBtn) finalDecideBtn.interactable = true;
            }
        }

        // ── ユーティリティ ────────────────────────────────────────────────────
        string GetPlayerName(List<LobbyPlayer> players, int index)
        {
            if (players == null || index < 0 || index >= players.Count) return "?";
            var p = players[index];
            if (p.Data != null && p.Data.TryGetValue("Name", out var d)) return d.Value;
            return "?";
        }

        static void SetPlaceholder(TMP_InputField field, string text)
        {
            if (field == null) return;
            var ph = field.placeholder as TextMeshProUGUI;
            if (ph != null) ph.text = text;
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
