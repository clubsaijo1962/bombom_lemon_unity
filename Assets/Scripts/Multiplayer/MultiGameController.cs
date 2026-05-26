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
    /// 協力モード：ゲーム進行画面。
    /// 回答者・最終決定者・予想者の3役に分かれてリアルタイムで進行する。
    /// </summary>
    public class MultiGameController : MonoBehaviour
    {
        public enum PlayerRole { Answerer, Decider, Guesser }

        [Header("お題・役割表示")]
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI answererNameLabel;
        [SerializeField] TextMeshProUGUI deciderNameLabel;
        [SerializeField] TextMeshProUGUI guideLabel;
        [SerializeField] TextMeshProUGUI myRoleLabel;

        [Header("回答表示")]
        [SerializeField] TextMeshProUGUI answerDisplayLabel;

        [Header("回答者入力パネル（自分が回答者のときのみ表示）")]
        [SerializeField] GameObject      answererPanel;
        [SerializeField] TMP_InputField  answerInputField;
        [SerializeField] Button          submitAnswerBtn;
        [SerializeField] TextMeshProUGUI submitAnswerBtnLabel;

        [Header("予想リスト")]
        [SerializeField] RectTransform   guessListContent;
        [SerializeField] TMP_FontAsset   listFont;
        [SerializeField] TextMeshProUGUI guessSectionHeader;

        [Header("予想者入力パネル（自分が予想者のときのみ表示）")]
        [SerializeField] GameObject      guesserPanel;
        [SerializeField] TMP_InputField  guessInputField;
        [SerializeField] Button          submitGuessBtn;
        [SerializeField] TextMeshProUGUI submitGuessBtnLabel;

        [Header("最終決定者パネル（自分が最終決定者のときのみ表示）")]
        [SerializeField] GameObject      deciderPanel;
        [SerializeField] TMP_InputField  secretInputField;
        [SerializeField] Button          finalDecideBtn;
        [SerializeField] TextMeshProUGUI finalDecideBtnLabel;

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

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            // VLG / ContentSizeFitter を無効化（手動配置）
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

            // 最終決定者: 自分の秘密の数字をプリセット
            if (_myRole == PlayerRole.Decider && secretInputField != null)
                secretInputField.text = RoomConfig.MySecretNumber.ToString();

            // 初期表示
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
            _myRole = myIdx == RoomConfig.AnswererIndex ? PlayerRole.Answerer
                    : myIdx == RoomConfig.DeciderIndex  ? PlayerRole.Decider
                    : PlayerRole.Guesser;
        }

        // ── 多言語・役割別UI適用 ───────────────────────────────────────────────
        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;

            // 役割パネル表示切替
            if (answererPanel) answererPanel.SetActive(_myRole == PlayerRole.Answerer);
            if (guesserPanel)  guesserPanel.SetActive(_myRole == PlayerRole.Guesser);
            if (deciderPanel)  deciderPanel.SetActive(_myRole == PlayerRole.Decider);

            // ボタンラベル
            if (submitAnswerBtnLabel) submitAnswerBtnLabel.text = en ? "Share ▶"     : "回答を送る ▶";
            if (submitGuessBtnLabel)  submitGuessBtnLabel.text  = en ? "Guess ▶"     : "予想を送る ▶";
            if (finalDecideBtnLabel)  finalDecideBtnLabel.text  = en ? "Finalize ✓" : "最終決定 ✓";
            if (guessSectionHeader)   guessSectionHeader.text   = en ? "Everyone's Guesses" : "みんなの予想";

            // ガイドテキスト（役割別）
            string guide = _myRole switch
            {
                PlayerRole.Answerer => en
                    ? "You're the Answerer! Share your honest feeling about the topic."
                    : "あなたが回答者！お題に対するあなたの正直な感覚を答えてください。",
                PlayerRole.Decider => en
                    ? "You're the Final Decider! Enter your secret number to reveal it."
                    : "あなたが最終決定者！秘密の数字を入力して確定してください。",
                _ => en
                    ? "Guess the Final Decider's secret number based on the Answerer's reply!"
                    : "回答を参考に、最終決定者の秘密の数字を予想しよう！",
            };
            if (guideLabel) guideLabel.text = guide;

            // 自分の役割バッジ
            string roleBadge = _myRole switch
            {
                PlayerRole.Answerer => en ? "🎤 Answerer"      : "🎤 あなたは回答者",
                PlayerRole.Decider  => en ? "🔐 Final Decider" : "🔐 あなたは最終決定者",
                _                   => en ? "🎯 Guesser"       : "🎯 あなたは予想者",
            };
            if (myRoleLabel) myRoleLabel.text = roleBadge;

            // InputField プレースホルダー
            SetPlaceholder(answerInputField, en ? "Enter your answer..." : "回答を入力...");
            SetPlaceholder(guessInputField,  en ? "Guess (1-99)"        : "予想の数字 (1〜99)");
            SetPlaceholder(secretInputField, en ? "Your secret number"  : "秘密の数字");
        }

        // ── 役割発表表示 ─────────────────────────────────────────────────────
        void ShowAnnouncement()
        {
            var players = LobbyManager.Instance.CurrentLobby?.Players;
            bool en = LanguageSettings.IsEnglish;

            string answererName = GetPlayerName(players, RoomConfig.AnswererIndex);
            string deciderName  = GetPlayerName(players, RoomConfig.DeciderIndex);

            if (topicLabel)        topicLabel.text        = RoomConfig.GameTopic;
            if (answererNameLabel) answererNameLabel.text = en
                ? $"Answerer: {answererName}"
                : $"回答者：{answererName}";
            if (deciderNameLabel)  deciderNameLabel.text  = en
                ? $"Final Decider: {deciderName}"
                : $"最終決定者：{deciderName}";
        }

        void ShowMyCard()
        {
            bool en = LanguageSettings.IsEnglish;
            string myTopic = TopicDatabase.GetTopic(RoomConfig.GameSeed, RoomConfig.PlayerIndex);
            if (myTopicLabel)  myTopicLabel.text  = myTopic;
            if (mySecretLabel) mySecretLabel.text = RoomConfig.MySecretNumber.ToString();
            if (roundLabel)
                roundLabel.text = en
                    ? $"Round {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}"
                    : $"ラウンド {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}";
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
            bool en = LanguageSettings.IsEnglish;

            // 回答者の最新回答を表示
            if (RoomConfig.AnswererIndex >= 0 && RoomConfig.AnswererIndex < players.Count)
            {
                var ap = players[RoomConfig.AnswererIndex];
                string answer = "";
                if (ap.Data != null && ap.Data.TryGetValue("Answer", out var ad))
                    answer = ad.Value;
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
            StartCoroutine(LoadWithFade("MultiResult"));
        }

        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── 予想リスト（手動配置）──────────────────────────────────────────────
        void RebuildGuessList(List<LobbyPlayer> players)
        {
            if (guessListContent == null) return;
            for (int i = guessListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(guessListContent.GetChild(i).gameObject);

            bool en = LanguageSettings.IsEnglish;
            int n = players.Count;
            float totalH = n > 0 ? n * SlotHeight + (n - 1) * SlotGap : 0f;
            guessListContent.sizeDelta = new Vector2(0f, totalH);

            for (int i = 0; i < n; i++)
            {
                var p = players[i];
                string name = "?";
                if (p.Data != null && p.Data.TryGetValue("Name", out var nd)) name = nd.Value;
                string guess = "---";
                if (p.Data != null && p.Data.TryGetValue("Guess", out var gd) &&
                    !string.IsNullOrEmpty(gd.Value))
                    guess = gd.Value;

                string badge = i == RoomConfig.AnswererIndex ? (en ? "[A]" : "[回]")
                             : i == RoomConfig.DeciderIndex  ? (en ? "[D]" : "[決]")
                             : "";

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
            r.anchorMin        = new Vector2(0f, 1f);
            r.anchorMax        = new Vector2(1f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(0f, SlotHeight);
            r.anchoredPosition = new Vector2(0f, yTop);

            var bg = go.AddComponent<Image>();
            bg.color = isMe
                ? new Color(1f, 0.97f, 0.85f, 0.92f)
                : new Color(0.96f, 0.96f, 0.96f, 0.55f);
            bg.raycastTarget = false;

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
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            if (listFont != null) tmp.font = listFont;
        }

        // ── ボタンハンドラ ────────────────────────────────────────────────────
        void OnSubmitAnswer()
        {
            string answer = answerInputField?.text ?? "";
            _ = SubmitAnswerAsync(answer);
        }

        async System.Threading.Tasks.Task SubmitAnswerAsync(string answer)
        {
            if (submitAnswerBtn) submitAnswerBtn.interactable = false;
            try
            {
                await LobbyManager.Instance.UpdatePlayerAnswerAsync(answer);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiGame] SubmitAnswer: {e.Message}");
            }
            finally
            {
                if (submitAnswerBtn) submitAnswerBtn.interactable = true;
            }
        }

        void OnSubmitGuess()
        {
            string guess = guessInputField?.text ?? "";
            _ = SubmitGuessAsync(guess);
        }

        async System.Threading.Tasks.Task SubmitGuessAsync(string guess)
        {
            if (submitGuessBtn) submitGuessBtn.interactable = false;
            try
            {
                await LobbyManager.Instance.UpdatePlayerGuessAsync(guess);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiGame] SubmitGuess: {e.Message}");
            }
            finally
            {
                if (submitGuessBtn) submitGuessBtn.interactable = true;
            }
        }

        void OnFinalDecide()
        {
            string input = secretInputField?.text ?? "";
            int num;
            if (!int.TryParse(input, out num) || num < 1 || num > 99)
                num = RoomConfig.MySecretNumber;
            num = Mathf.Clamp(num, 1, 99);
            _ = FinalizeAsync(num);
        }

        async System.Threading.Tasks.Task FinalizeAsync(int number)
        {
            if (finalDecideBtn) finalDecideBtn.interactable = false;
            try
            {
                await LobbyManager.Instance.FinalizeGameAsync(number);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiGame] Finalize: {e.Message}");
                if (finalDecideBtn) finalDecideBtn.interactable = true;
            }
        }

        // ── ユーティリティ ────────────────────────────────────────────────────
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
