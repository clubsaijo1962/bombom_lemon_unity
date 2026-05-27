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
    /// 協力モード：秘密の数字確認画面。
    /// ホストが StartConfirmPhaseAsync を呼んだ後、全員がこのシーンに遷移してくる。
    /// 各プレイヤーはシードとインデックスから自分のお題と秘密の数字を確認し、
    /// 「確認しました」ボタンを押すと Ready 状態になる。
    /// 全員 Ready になったら次のシーンへ遷移する。
    /// </summary>
    public class MultiConfirmController : MonoBehaviour
    {
        [Header("数字表示")]
        [SerializeField] TextMeshProUGUI secretNumberLabel;
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI playerNameLabel;

        [Header("多言語ラベル")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI topicHeaderLabel;
        [SerializeField] TextMeshProUGUI secretHeaderLabel;
        [SerializeField] TextMeshProUGUI warningLabel;

        [Header("確認ボタン")]
        [SerializeField] Button          confirmButton;
        [SerializeField] TextMeshProUGUI confirmBtnLabel;

        [Header("待機ステータス")]
        [SerializeField] TextMeshProUGUI waitingLabel;
        [SerializeField] TextMeshProUGUI readyCountLabel;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        bool _confirmed      = false;
        bool _loadingNext    = false;

        // ── 初期化 ────────────────────────────────────────────────────────────
        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyLanguage();
            ShowAssignment();

            // UGS イベント購読
            LobbyManager.Instance.OnPlayersUpdated   += HandlePlayersUpdated;
            LobbyManager.Instance.OnAllPlayersReady  += HandleAllPlayersReady;
            LobbyManager.Instance.OnLobbyDeleted     += HandleLobbyDeleted;
            LobbyManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            confirmButton?.onClick.AddListener(OnConfirm);

            // 待機ラベルは確認後に表示
            if (waitingLabel)    waitingLabel.gameObject.SetActive(false);
            if (readyCountLabel) readyCountLabel.gameObject.SetActive(false);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayersUpdated   -= HandlePlayersUpdated;
                LobbyManager.Instance.OnAllPlayersReady  -= HandleAllPlayersReady;
                LobbyManager.Instance.OnLobbyDeleted     -= HandleLobbyDeleted;
                LobbyManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        // ── 多言語適用 ────────────────────────────────────────────────────────
        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (titleLabel)       titleLabel.text       = en ? "Your Secret Number"           : "秘密の数字を確認してください";
            if (topicHeaderLabel) topicHeaderLabel.text = en ? "Theme"                        : "お題";
            if (secretHeaderLabel)secretHeaderLabel.text= en ? "Your Secret Number"           : "あなたの秘密の数字";
            if (warningLabel)     warningLabel.text     = en ? "⚠ Don't show others!"         : "⚠ 他のプレイヤーには見せないで！";
            if (confirmBtnLabel)  confirmBtnLabel.text  = en ? "Got it ✓"                     : "確認しました ✓";
            if (waitingLabel)     waitingLabel.text     = en ? "Waiting for others..."        : "他のプレイヤーを待っています...";
        }

        // ── 割り当て表示 ──────────────────────────────────────────────────────
        void ShowAssignment()
        {
            int seed   = RoomConfig.GameSeed;
            int index  = RoomConfig.PlayerIndex >= 0 ? RoomConfig.PlayerIndex : 0;

            int    number = TopicDatabase.GetSecretNumber(seed, index);
            string topic  = TopicDatabase.GetTopic(seed, index);

            RoomConfig.MySecretNumber = number;

            if (secretNumberLabel) secretNumberLabel.text = number.ToString();
            if (topicLabel)        topicLabel.text        = topic;
            if (playerNameLabel)   playerNameLabel.text   = RoomConfig.LocalPlayerName;
        }

        // ── UGS イベントハンドラ ──────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players)
        {
            if (!_confirmed) return; // 確認前はカウント非表示

            int ready = 0;
            foreach (var p in players)
            {
                if (p.Data != null && p.Data.TryGetValue("Ready", out var r) && r.Value == "1")
                    ready++;
            }

            bool en = LanguageSettings.IsEnglish;
            if (readyCountLabel)
                readyCountLabel.text = en
                    ? $"{ready} / {players.Count} confirmed"
                    : $"{ready} / {players.Count} 人 確認済み";
        }

        void HandleAllPlayersReady()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            if (RoomConfig.IsHost)
            {
                if (RoomConfig.Mode == RoomConfig.GameMode.TeamBattle)
                    _ = HostStartTeamBattleAsync();
                else
                    _ = HostStartGameAsync();
            }
            // ゲスト: HandleGameStateChanged で遷移
        }

        async System.Threading.Tasks.Task HostStartGameAsync()
        {
            try
            {
                var players = LobbyManager.Instance.CurrentLobby?.Players;
                int count   = players?.Count ?? 1;

                // Fisher-Yates シャッフルで最終決定者の順序を決定（シードから再現可能）
                var rng   = new System.Random(RoomConfig.GameSeed * 53 + 3);
                int[] order = new int[count];
                for (int i = 0; i < count; i++) order[i] = i;
                for (int i = count - 1; i > 0; i--)
                {
                    int j = rng.Next(0, i + 1);
                    (order[i], order[j]) = (order[j], order[i]);
                }

                // Round 0: 最初の最終決定者と回答者
                int deciderIdx  = order[0];
                int answererIdx = order[count > 1 ? 1 : 0];
                string topic    = TopicDatabase.GetTopic(RoomConfig.GameSeed, deciderIdx);

                // 初期ライフ・ヘルプカード
                int startLives = RoomConfig.IsHellMode ? 3 : 5;
                int startHelps = count; // 1枚/プレイヤー

                await LobbyManager.Instance.StartGameWithRoundsAsync(
                    answererIdx, deciderIdx, topic,
                    order, startLives, startHelps);

                StartCoroutine(LoadWithFade("MultiGame"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiConfirm] HostStartGameAsync: {e.Message}");
                _loadingNext = false;
            }
        }

        async System.Threading.Tasks.Task HostStartTeamBattleAsync()
        {
            try
            {
                var players = LobbyManager.Instance.CurrentLobby?.Players;
                int count   = players?.Count ?? 1;

                // Fisher-Yates シャッフルでチーム分け（シード固定・再現可能）
                var rng      = new System.Random(RoomConfig.GameSeed * 71 + 7);
                int[] shuf   = new int[count];
                for (int i = 0; i < count; i++) shuf[i] = i;
                for (int i = count - 1; i > 0; i--)
                {
                    int j = rng.Next(0, i + 1);
                    (shuf[i], shuf[j]) = (shuf[j], shuf[i]);
                }

                int sizeA = (count + 1) / 2;
                int sizeB = count / 2;
                var teamA = new int[sizeA];
                var teamB = new int[sizeB];
                System.Array.Copy(shuf, 0, teamA, 0, sizeA);
                System.Array.Copy(shuf, sizeA, teamB, 0, sizeB);

                // 奇数時: チームB からランダム1名がダブルプレイヤー
                int doubledIdx = -1;
                if (count % 2 == 1 && sizeB > 0)
                    doubledIdx = teamB[rng.Next(0, sizeB)];

                // 各チーム内でシャッフル（回答順序を決定）
                var aOrder = ShuffleArr(teamA, rng);
                var bOrder = ShuffleArr(teamB, rng);

                // ラウンドペア列 [a0,d0, a1,d1, ...] を構築
                var roundPairs = BuildTeamRoundPairs(aOrder, bOrder, teamA, teamB, doubledIdx, rng,
                                                     RoomConfig.GameSeed, count);

                // ラウンド0の情報
                int firstAnswerer = roundPairs[0];
                int firstDecider  = roundPairs[1];
                bool isATeam      = System.Array.IndexOf(teamA, firstAnswerer) >= 0;
                string firstTeam  = isATeam ? "A" : "B";
                string firstTopic = TopicDatabase.GetTopic(RoomConfig.GameSeed, firstDecider);

                await LobbyManager.Instance.StartTeamBattleAsync(
                    firstAnswerer, firstDecider, firstTopic,
                    teamA, teamB, doubledIdx, roundPairs, 0, 0, firstTeam);

                // ホスト自身がダブルプレイヤーの場合、ポーリングを経由しないため
                // MySecondTopic / MySecondSecretNumber をここで直接設定する
                if (doubledIdx >= 0 && doubledIdx == RoomConfig.PlayerIndex)
                {
                    int numActual = count; // TotalRounds - 1 (奇数時の追加お題インデックス)
                    RoomConfig.MySecondTopic        = TopicDatabase.GetTopic(RoomConfig.GameSeed, numActual);
                    RoomConfig.MySecondSecretNumber = TopicDatabase.GetSecretNumber(RoomConfig.GameSeed, numActual);
                }

                StartCoroutine(LoadWithFade("TeamGame"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiConfirm] HostStartTeamBattleAsync: {e.Message}");
                _loadingNext = false;
            }
        }

        // ── チームバトル ユーティリティ ──────────────────────────────────────────

        static int[] ShuffleArr(int[] src, System.Random rng)
        {
            var arr = (int[])src.Clone();
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }

        /// <summary>
        /// [answerer0, decider0, answerer1, decider1, ...] のペア配列を構築する。
        /// - TeamA と TeamB を交互にラウンドに割り当てる
        /// - 奇数時はダブルプレイヤーの追加ラウンドを末尾に追加
        /// - 最終決定者は回答者と同じチームの別メンバーから選出（毎回変える）
        /// </summary>
        static int[] BuildTeamRoundPairs(
            int[] aOrder, int[] bOrder,
            int[] teamA,  int[] teamB,
            int doubledIdx, System.Random rng,
            int seed, int numActualPlayers)
        {
            var pairs = new System.Collections.Generic.List<int>();
            int maxLen = System.Math.Max(aOrder.Length, bOrder.Length);

            // 各チームの「次の最終決定者候補インデックス」を管理
            var aDeciderQueue = new System.Collections.Generic.List<int>();
            var bDeciderQueue = new System.Collections.Generic.List<int>();
            aDeciderQueue.AddRange(ShuffleArr(teamA, rng));
            bDeciderQueue.AddRange(ShuffleArr(teamB, rng));

            for (int i = 0; i < maxLen; i++)
            {
                // TeamA ラウンド
                if (i < aOrder.Length)
                {
                    int answerer = aOrder[i];
                    int decider  = PickDecider(answerer, teamA, aDeciderQueue);
                    pairs.Add(answerer);
                    pairs.Add(decider);
                }
                // TeamB ラウンド
                if (i < bOrder.Length)
                {
                    int answerer = bOrder[i];
                    int decider  = PickDecider(answerer, teamB, bDeciderQueue);
                    pairs.Add(answerer);
                    pairs.Add(decider);
                }
            }

            // ダブルプレイヤーの追加ラウンド（チームBから選出）
            if (doubledIdx >= 0)
            {
                int decider = PickDecider(doubledIdx, teamB, bDeciderQueue);
                // 追加ラウンドのお題インデックスは numActualPlayers（= count）を使用
                // decider はダブルプレイヤー自身（自分の2つ目の秘密を使う）
                pairs.Add(doubledIdx); // answerer
                pairs.Add(doubledIdx); // decider（自分の2つ目の秘密を使う）
                _ = decider;           // suppress warning
            }

            return pairs.ToArray();
        }

        /// <summary>チーム内から回答者以外の最終決定者を選出（キューをローテーション）。</summary>
        static int PickDecider(int answerer, int[] team,
            System.Collections.Generic.List<int> queue)
        {
            // キューが空ならチームメンバーで再充填
            if (queue.Count == 0)
                foreach (int m in team) queue.Add(m);

            // キューから answerer 以外の最初の候補を選ぶ
            for (int i = 0; i < queue.Count; i++)
            {
                if (queue[i] != answerer)
                {
                    int d = queue[i];
                    queue.RemoveAt(i);
                    return d;
                }
            }
            // 全員 answerer（1人チーム）→ 自分自身
            return answerer;
        }

        void HandleGameStateChanged(string state)
        {
            if (_loadingNext) return;
            // CoopLife ゲスト
            if (state == "Playing")
            {
                _loadingNext = true;
                StartCoroutine(LoadWithFade("MultiGame"));
            }
            // TeamBattle ゲスト
            else if (state == "TeamPlaying")
            {
                _loadingNext = true;
                StartCoroutine(LoadWithFade("TeamGame"));
            }
        }

        void HandleLobbyDeleted()
        {
            if (_loadingNext) return;
            _loadingNext = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── 確認ボタン ────────────────────────────────────────────────────────
        void OnConfirm()
        {
            if (_confirmed) return;
            _confirmed = true;

            bool en = LanguageSettings.IsEnglish;

            if (confirmButton)   confirmButton.interactable = false;
            if (confirmBtnLabel) confirmBtnLabel.text = en ? "✓ Confirmed" : "✓ 確認済み";
            if (waitingLabel)    waitingLabel.gameObject.SetActive(true);
            if (readyCountLabel) readyCountLabel.gameObject.SetActive(true);

            _ = SetReadyAsync();
        }

        async System.Threading.Tasks.Task SetReadyAsync()
        {
            try
            {
                await LobbyManager.Instance.SetPlayerReadyAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiConfirm] SetReadyAsync: {e.Message}");
            }
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
