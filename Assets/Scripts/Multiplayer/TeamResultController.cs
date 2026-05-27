using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.Audio;
using BomBomLemon.Network;
using BomBomLemon.PlayerSetup;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

namespace BomBomLemon.Multiplayer
{
    /// <summary>
    /// チームバトル：ラウンド結果画面。
    /// アクティブチームの予想・差・スコアを表示し、次ラウンドへ進行する（ホスト操作）。
    /// </summary>
    public class TeamResultController : MonoBehaviour
    {
        [Header("ラウンド情報")]
        [SerializeField] TextMeshProUGUI roundHeaderLabel;
        [SerializeField] TextMeshProUGUI activeTeamBadge;
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI actualNumberLabel;
        [SerializeField] TextMeshProUGUI deciderNameLabel;

        [Header("スコア表示")]
        [SerializeField] TextMeshProUGUI teamAScoreLabel;
        [SerializeField] TextMeshProUGUI teamBScoreLabel;
        [SerializeField] TextMeshProUGUI roundDiffLabel;

        [Header("自分の秘密（常時表示）")]
        [SerializeField] TextMeshProUGUI myTopicLabel;
        [SerializeField] TextMeshProUGUI mySecretLabel;

        [Header("予想リスト")]
        [SerializeField] RectTransform   guessListContent;
        [SerializeField] TMP_FontAsset   listFont;

        [Header("次ラウンドボタン（ホストのみ）")]
        [SerializeField] GameObject      hostPanel;
        [SerializeField] Button          nextRoundBtn;
        [SerializeField] TextMeshProUGUI nextRoundBtnLabel;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        bool _loadingNext;

        const float SlotHeight = 88f;
        const float SlotGap    =  8f;

        static readonly Color ColPrimary = new(0.20f, 0.10f, 0.02f);
        static readonly Color ColMuted   = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color ColGreen   = new(0.10f, 0.42f, 0.12f);
        static readonly Color ColRed     = new(0.72f, 0.12f, 0.10f);
        static readonly Color ColGold    = new(0.85f, 0.55f, 0.00f);
        static readonly Color ColTeamA   = new(0.15f, 0.35f, 0.75f);
        static readonly Color ColTeamB   = new(0.75f, 0.20f, 0.15f);

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            if (guessListContent != null)
            {
                var csf = guessListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (csf) csf.enabled = false;
                var vlg = guessListContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (vlg) vlg.enabled = false;
            }

            ApplyLanguage();
            ShowResult();

            if (hostPanel) hostPanel.SetActive(RoomConfig.IsHost);

            LobbyManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            LobbyManager.Instance.OnTeamFinal        += HandleTeamFinal;
            LobbyManager.Instance.OnLobbyDeleted     += HandleLobbyDeleted;

            nextRoundBtn?.onClick.AddListener(OnNextRound);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                LobbyManager.Instance.OnTeamFinal        -= HandleTeamFinal;
                LobbyManager.Instance.OnLobbyDeleted     -= HandleLobbyDeleted;
            }
        }

        // ── 多言語適用 ────────────────────────────────────────────────────────
        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (nextRoundBtnLabel) nextRoundBtnLabel.text = en ? "Next Round ▶" : "次のラウンドへ ▶";
        }

        // ── 結果表示 ──────────────────────────────────────────────────────────
        void ShowResult()
        {
            bool en = LanguageSettings.IsEnglish;
            int actual = RoomConfig.FinalConfirmedNumber;
            var players = LobbyManager.Instance.CurrentLobby?.Players;

            if (roundHeaderLabel)
                roundHeaderLabel.text = en
                    ? $"Round {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}"
                    : $"ラウンド {RoomConfig.CurrentRound + 1} / {RoomConfig.TotalRounds}";

            bool aActive = RoomConfig.ActiveTeam == "A";
            if (activeTeamBadge)
            {
                activeTeamBadge.text  = en
                    ? $"Team {RoomConfig.ActiveTeam} played this round"
                    : $"チーム{RoomConfig.ActiveTeam} のラウンド";
                activeTeamBadge.color = aActive ? ColTeamA : ColTeamB;
            }

            if (topicLabel)        topicLabel.text        = RoomConfig.GameTopic;
            if (actualNumberLabel) actualNumberLabel.text = actual.ToString();
            if (deciderNameLabel)
                deciderNameLabel.text = en
                    ? $"Final Decider: {GetPlayerName(players, RoomConfig.DeciderIndex)}"
                    : $"最終決定者：{GetPlayerName(players, RoomConfig.DeciderIndex)}";

            // 自分の秘密
            string myTopic  = TopicDatabase.GetTopic(RoomConfig.GameSeed, RoomConfig.PlayerIndex);
            bool isDoubled = RoomConfig.DoubledPlayerIndex == RoomConfig.PlayerIndex && RoomConfig.DoubledPlayerIndex >= 0;
            if (myTopicLabel)  myTopicLabel.text  = isDoubled ? $"{myTopic}  /  {RoomConfig.MySecondTopic}" : myTopic;
            if (mySecretLabel) mySecretLabel.text = isDoubled
                ? $"🔒{RoomConfig.MySecretNumber}  /  🔒{RoomConfig.MySecondSecretNumber}"
                : RoomConfig.MySecretNumber.ToString();

            // このラウンドの差を計算して表示
            int[] activeTeamArr = aActive ? RoomConfig.TeamA : RoomConfig.TeamB;
            int roundDiff = ComputeRoundDiff(players, actual, activeTeamArr,
                                              RoomConfig.AnswererIndex, RoomConfig.DeciderIndex);

            if (roundDiffLabel)
                roundDiffLabel.text = en
                    ? $"This round diff: {roundDiff}"
                    : $"このラウンドの差計：{roundDiff}";

            UpdateScoreLabels();

            if (players != null) RebuildGuessList(players, actual, activeTeamArr);

            // 数字開示SE
            if (roundDiff == 0) AudioManager.Instance?.PlayPerfect();
            else                AudioManager.Instance?.PlayShow();
        }

        int ComputeRoundDiff(List<LobbyPlayer> players, int actualNumber,
            int[] activeTeam, int answererIdx, int deciderIdx)
        {
            if (players == null || activeTeam == null) return 0;
            int total = 0;
            foreach (int idx in activeTeam)
            {
                if (idx == answererIdx || idx == deciderIdx) continue;
                if (idx < 0 || idx >= players.Count) continue;
                var p = players[idx];
                if (p.Data != null && p.Data.TryGetValue("Guess", out var gd) &&
                    int.TryParse(gd.Value, out int g))
                    total += Mathf.Abs(g - actualNumber);
            }
            return total;
        }

        void UpdateScoreLabels()
        {
            bool en = LanguageSettings.IsEnglish;
            if (teamAScoreLabel)
                teamAScoreLabel.text = en
                    ? $"Team A: {RoomConfig.TeamAScore} pts"
                    : $"チームA：{RoomConfig.TeamAScore} pt";
            if (teamBScoreLabel)
                teamBScoreLabel.text = en
                    ? $"Team B: {RoomConfig.TeamBScore} pts"
                    : $"チームB：{RoomConfig.TeamBScore} pt";
        }

        // ── 予想リスト ────────────────────────────────────────────────────────
        void RebuildGuessList(List<LobbyPlayer> players, int actualNumber, int[] activeTeam)
        {
            if (guessListContent == null) return;
            for (int i = guessListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(guessListContent.GetChild(i).gameObject);

            bool en = LanguageSettings.IsEnglish;
            int n = activeTeam?.Length ?? 0;
            guessListContent.sizeDelta = new Vector2(0f, n > 0 ? n * SlotHeight + (n - 1) * SlotGap : 0f);

            for (int i = 0; i < n; i++)
            {
                int pIdx = activeTeam[i];
                if (pIdx < 0 || pIdx >= players.Count) continue;
                var p = players[pIdx];
                string name = GetPlayerName(players, pIdx);
                bool hasGuess = p.Data != null && p.Data.TryGetValue("Guess", out var gd) &&
                                !string.IsNullOrEmpty(gd.Value) && int.TryParse(gd.Value, out _);
                int guessVal = 0; string guess = "---";
                if (hasGuess && p.Data.TryGetValue("Guess", out var gd2))
                { int.TryParse(gd2.Value, out guessVal); guess = gd2.Value; }

                int diff = hasGuess ? Mathf.Abs(guessVal - actualNumber) : -1;
                string badge = pIdx == RoomConfig.AnswererIndex ? (en ? "[A]" : "[回]")
                             : pIdx == RoomConfig.DeciderIndex  ? (en ? "[D]" : "[決]") : "";
                bool isMe = p.Id == RoomConfig.LocalPlayerId;
                AddResultSlot($"{badge}{name}", guess, diff, -(i * (SlotHeight + SlotGap)), isMe, hasGuess);
            }
        }

        void AddResultSlot(string name, string guess, int diff, float yTop, bool isMe, bool hasGuess)
        {
            var go = new GameObject("ResultSlot", typeof(RectTransform));
            go.transform.SetParent(guessListContent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0f, SlotHeight);
            r.anchoredPosition = new Vector2(0f, yTop);
            go.AddComponent<Image>().color = isMe ? new Color(1f, 0.97f, 0.85f, 0.92f) : new Color(0.96f, 0.96f, 0.96f, 0.55f);
            go.GetComponent<Image>().raycastTarget = false;

            AddSlotLabel(go.transform, "Name", name,
                new Vector2(0f, 0f), new Vector2(0.35f, 1f),
                new Vector2(14f, 4f), new Vector2(-4f, -4f), 32f, TextAlignmentOptions.MidlineLeft, ColPrimary);
            AddSlotLabel(go.transform, "Guess", guess,
                new Vector2(0.35f, 0f), new Vector2(0.65f, 1f),
                new Vector2(4f, 4f), new Vector2(-4f, -4f), 44f, TextAlignmentOptions.Center,
                hasGuess ? ColPrimary : ColMuted);
            bool enDiff = LanguageSettings.IsEnglish;
            string diffText = diff >= 0 ? (enDiff ? $"Diff: {diff}" : $"差：{diff}") : "---";
            Color diffColor = diff < 0 ? ColMuted : diff == 0 ? ColGold : diff <= 10 ? ColGreen : diff <= 25 ? ColPrimary : ColRed;
            AddSlotLabel(go.transform, "Diff", diffText,
                new Vector2(0.65f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-14f, -4f), 36f, TextAlignmentOptions.MidlineRight, diffColor);
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

        // ── UGS イベント ─────────────────────────────────────────────────────
        void HandleGameStateChanged(string state)
        {
            if (_loadingNext) return;
            if (state == "TeamPlaying" && !RoomConfig.IsHost)
            {
                _loadingNext = true;
                StartCoroutine(LoadWithFade("TeamGame"));
            }
        }

        void HandleTeamFinal(string winner)
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("TeamFinal"));
        }

        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── 次ラウンドボタン（ホスト専用）──────────────────────────────────────
        void OnNextRound()
        {
            if (!RoomConfig.IsHost || _loadingNext) return;
            if (nextRoundBtn) nextRoundBtn.interactable = false;
            _ = NextRoundAsync();
        }

        async System.Threading.Tasks.Task NextRoundAsync()
        {
            var players    = LobbyManager.Instance.CurrentLobby?.Players;
            int actual     = RoomConfig.FinalConfirmedNumber;
            bool aActive   = RoomConfig.ActiveTeam == "A";
            int[] activeT  = aActive ? RoomConfig.TeamA : RoomConfig.TeamB;

            int roundDiff  = ComputeRoundDiff(players, actual, activeT,
                                               RoomConfig.AnswererIndex, RoomConfig.DeciderIndex);
            int newAScore  = RoomConfig.TeamAScore + (aActive  ? roundDiff : 0);
            int newBScore  = RoomConfig.TeamBScore + (!aActive ? roundDiff : 0);
            int nextRound  = RoomConfig.CurrentRound + 1;

            if (nextRound >= RoomConfig.TotalRounds)
            {
                // 全ラウンド終了
                _loadingNext = true;
                string winner = newAScore < newBScore ? "A" : newBScore < newAScore ? "B" : "Draw";
                try { await LobbyManager.Instance.EndTeamGameAsync(winner, newAScore, newBScore); }
                catch (Exception e) { Debug.LogWarning($"[TeamResult] EndTeamGame: {e.Message}"); }
                StartCoroutine(LoadWithFade("TeamFinal"));
                return;
            }

            // 次ラウンドのデータを計算
            var order       = RoomConfig.DeciderOrder; // ペア列 [a0,d0,a1,d1,...]
            int nextAnswerer = order[nextRound * 2];
            int nextDecider  = order[nextRound * 2 + 1];
            bool nextIsA    = System.Array.IndexOf(RoomConfig.TeamA, nextAnswerer) >= 0;
            string nextTeam = nextIsA ? "A" : "B";

            // ダブルプレイヤーの追加ラウンドはお題インデックス = numActualPlayers
            bool isDoubledExtra = RoomConfig.DoubledPlayerIndex >= 0
                               && nextRound == RoomConfig.TotalRounds - 1
                               && nextAnswerer == RoomConfig.DoubledPlayerIndex;
            int numActual = RoomConfig.DoubledPlayerIndex >= 0
                ? RoomConfig.TotalRounds - 1 : RoomConfig.TotalRounds;
            string nextTopic = isDoubledExtra
                ? TopicDatabase.GetTopic(RoomConfig.GameSeed, numActual)
                : TopicDatabase.GetTopic(RoomConfig.GameSeed, nextDecider);

            try
            {
                await LobbyManager.Instance.AdvanceTeamRoundAsync(
                    nextAnswerer, nextDecider, nextTopic,
                    nextRound, newAScore, newBScore, nextTeam);
                _loadingNext = true;
                StartCoroutine(LoadWithFade("TeamGame"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TeamResult] AdvanceTeamRound: {e.Message}");
                if (nextRoundBtn) nextRoundBtn.interactable = true;
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
