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
                _ = HostStartGameAsync();
            // ゲスト: HandleGameStateChanged("Playing") で遷移
        }

        async System.Threading.Tasks.Task HostStartGameAsync()
        {
            try
            {
                var players = LobbyManager.Instance.CurrentLobby?.Players;
                int count   = players?.Count ?? 1;

                // シードから決定論的に回答者・最終決定者を選出（全クライアントで同一結果）
                var rng = new System.Random(RoomConfig.GameSeed * 53 + 3);
                int answererIdx = rng.Next(0, count);
                int deciderIdx;
                if (count > 1)
                    do { deciderIdx = rng.Next(0, count); } while (deciderIdx == answererIdx);
                else
                    deciderIdx = 0;

                // 最終決定者のお題を全体のゲームお題とする
                string topic = TopicDatabase.GetTopic(RoomConfig.GameSeed, deciderIdx);

                await LobbyManager.Instance.StartGamePhaseAsync(answererIdx, deciderIdx, topic);
                StartCoroutine(LoadWithFade("MultiGame"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiConfirm] HostStartGameAsync: {e.Message}");
                _loadingNext = false;
            }
        }

        void HandleGameStateChanged(string state)
        {
            // ゲストは "Playing" 検知で MultiGame に遷移
            if (state == "Playing" && !_loadingNext)
            {
                _loadingNext = true;
                StartCoroutine(LoadWithFade("MultiGame"));
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
