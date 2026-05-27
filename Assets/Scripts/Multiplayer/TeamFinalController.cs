using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.Audio;
using BomBomLemon.Network;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Multiplayer
{
    /// <summary>
    /// チームバトル：最終結果画面。
    /// 勝者チームとスコアを全員に表示する。
    /// </summary>
    public class TeamFinalController : MonoBehaviour
    {
        [Header("勝者表示")]
        [SerializeField] TextMeshProUGUI winnerLabel;
        [SerializeField] TextMeshProUGUI winnerDetailLabel;

        [Header("チームAスコア")]
        [SerializeField] TextMeshProUGUI teamAHeaderLabel;
        [SerializeField] TextMeshProUGUI teamAScoreLabel;
        [SerializeField] TextMeshProUGUI teamAMembersLabel;

        [Header("チームBスコア")]
        [SerializeField] TextMeshProUGUI teamBHeaderLabel;
        [SerializeField] TextMeshProUGUI teamBScoreLabel;
        [SerializeField] TextMeshProUGUI teamBMembersLabel;

        [Header("ボタン")]
        [SerializeField] Button          backBtn;
        [SerializeField] TextMeshProUGUI backBtnLabel;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        bool _loadingNext;

        static readonly Color ColTeamA  = new(0.15f, 0.35f, 0.75f);
        static readonly Color ColTeamB  = new(0.75f, 0.20f, 0.15f);
        static readonly Color ColDraw   = new(0.35f, 0.35f, 0.35f);
        static readonly Color ColGold   = new(0.85f, 0.55f, 0.00f);
        static readonly Color ColPrimary= new(0.20f, 0.10f, 0.02f);

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ShowFinalResult();

            LobbyManager.Instance.OnLobbyDeleted += HandleLobbyDeleted;
            backBtn?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
                LobbyManager.Instance.OnLobbyDeleted -= HandleLobbyDeleted;
        }

        // ── 最終結果表示 ──────────────────────────────────────────────────────
        void ShowFinalResult()
        {
            bool en = LanguageSettings.IsEnglish;
            var players = LobbyManager.Instance.CurrentLobby?.Players;

            // 勝者判定（TeamAScore/TeamBScore は OnTeamFinal で更新済み）
            int aScore = RoomConfig.TeamAScore;
            int bScore = RoomConfig.TeamBScore;
            string winner = aScore < bScore ? "A" : bScore < aScore ? "B" : "Draw";

            // 勝者ラベル
            Color winColor = winner == "A" ? ColTeamA : winner == "B" ? ColTeamB : ColDraw;
            if (winnerLabel)
            {
                winnerLabel.color = winColor;
                winnerLabel.text  = winner == "Draw"
                    ? (en ? "🤝 Draw!" : "🤝 引き分け！")
                    : en ? $"🏆 Team {winner} Wins!" : $"🏆 チーム{winner} の勝利！";
            }
            if (winnerDetailLabel)
                winnerDetailLabel.text = winner == "Draw"
                    ? (en ? "Both teams scored equally." : "両チーム同スコアでした。")
                    : en
                        ? $"Team {winner} had the smaller total difference!"
                        : $"チーム{winner} の差の合計がより小さかった！";

            // チームAヘッダー・スコア・メンバー
            if (teamAHeaderLabel) { teamAHeaderLabel.text = en ? "Team A" : "チームA"; teamAHeaderLabel.color = ColTeamA; }
            if (teamAScoreLabel)
            {
                teamAScoreLabel.text  = en ? $"Total Diff: {aScore}" : $"差の合計：{aScore}";
                teamAScoreLabel.color = winner == "A" ? ColGold : ColPrimary;
            }
            if (teamAMembersLabel) teamAMembersLabel.text = BuildMemberString(players, RoomConfig.TeamA);

            // チームBヘッダー・スコア・メンバー
            if (teamBHeaderLabel) { teamBHeaderLabel.text = en ? "Team B" : "チームB"; teamBHeaderLabel.color = ColTeamB; }
            if (teamBScoreLabel)
            {
                teamBScoreLabel.text  = en ? $"Total Diff: {bScore}" : $"差の合計：{bScore}";
                teamBScoreLabel.color = winner == "B" ? ColGold : ColPrimary;
            }
            if (teamBMembersLabel) teamBMembersLabel.text = BuildMemberString(players, RoomConfig.TeamB);

            if (backBtnLabel) backBtnLabel.text = en ? "Back to Lobby" : "ロビーに戻る";

            // 勝敗SE：自チームが勝利→GameClear、敗北→GameOver、引き分け→GameClear
            bool myTeamWon = (winner == "A" && System.Array.IndexOf(RoomConfig.TeamA, RoomConfig.PlayerIndex) >= 0)
                          || (winner == "B" && System.Array.IndexOf(RoomConfig.TeamB, RoomConfig.PlayerIndex) >= 0);
            if (winner == "Draw" || myTeamWon) AudioManager.Instance?.PlayGameClear();
            else                               AudioManager.Instance?.PlayGameOver();
        }

        string BuildMemberString(System.Collections.Generic.List<Unity.Services.Lobbies.Models.Player> players, int[] team)
        {
            if (players == null || team == null) return "";
            var sb = new System.Text.StringBuilder();
            foreach (int idx in team)
            {
                if (idx < 0 || idx >= players.Count) continue;
                var p = players[idx];
                if (sb.Length > 0) sb.Append("  ");
                string name = "?";
                if (p.Data != null && p.Data.TryGetValue("Name", out var d)) name = d.Value;
                sb.Append(name);
            }
            return sb.ToString();
        }

        // ── UGS イベント ─────────────────────────────────────────────────────
        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── ボタン ────────────────────────────────────────────────────────────
        void OnBack()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            _ = BackAsync();
        }

        async System.Threading.Tasks.Task BackAsync()
        {
            try
            {
                if (RoomConfig.IsHost) await LobbyManager.Instance.DissolveAsync();
                else                   await LobbyManager.Instance.LeaveAsync();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[TeamFinal] BackAsync: {e.Message}");
            }
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
