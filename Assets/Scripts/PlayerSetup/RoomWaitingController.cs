using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.Network;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// 部屋待機画面。UGS Lobby をポーリングしてプレイヤーリストをリアルタイム更新する。
    /// </summary>
    public class RoomWaitingController : MonoBehaviour
    {
        const int MaxPlayers = 6;

        [Header("ルーム情報")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI pinValueLabel;
        [SerializeField] TextMeshProUGUI modeLabel;

        [Header("地獄モード")]
        [SerializeField] GameObject hellModeBadge;

        [Header("参加プレイヤー")]
        [SerializeField] GameObject[]      playerSlotObjects;
        [SerializeField] TextMeshProUGUI[] playerNameLabels;
        [SerializeField] TextMeshProUGUI[] playerStatusLabels;

        [Header("多言語ラベル")]
        [SerializeField] TextMeshProUGUI playersHeaderLabel;
        [SerializeField] TextMeshProUGUI startBtnLabel;
        [SerializeField] TextMeshProUGUI backBtnLabel;

        [Header("ボタン")]
        [SerializeField] Button startButton;
        [SerializeField] Button backButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        static readonly Color SlotEmpty  = new Color(0.95f, 0.95f, 0.95f, 0.55f);
        static readonly Color SlotFilled = new Color(1.00f, 0.97f, 0.90f, 0.92f);

        bool _isLeaving;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyLanguage();
            ShowRoomInfo();
            InitSlots();

            // UGS イベント購読
            LobbyManager.Instance.OnPlayersUpdated += HandlePlayersUpdated;
            LobbyManager.Instance.OnLobbyDeleted   += HandleLobbyDeleted;

            startButton?.onClick.AddListener(OnStart);
            backButton?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayersUpdated -= HandlePlayersUpdated;
                LobbyManager.Instance.OnLobbyDeleted   -= HandleLobbyDeleted;
            }
        }

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (playersHeaderLabel) playersHeaderLabel.text = en ? "Players"       : "参加プレイヤー";
            if (startBtnLabel)      startBtnLabel.text      = en ? "Start Game ▶" : "ゲームスタート ▶";
            if (backBtnLabel)       backBtnLabel.text       = RoomConfig.IsHost
                ? (en ? "← Dissolve" : "← 解散")
                : (en ? "← Leave"    : "← 退出");
        }

        void ShowRoomInfo()
        {
            bool en = LanguageSettings.IsEnglish;
            string host = RoomConfig.HostName.Length > 0 ? RoomConfig.HostName : (en ? "Host" : "ホスト");
            string mode = RoomConfig.Mode == RoomConfig.GameMode.CoopLife
                ? (en ? "Coop Mode"   : "協力モード")
                : (en ? "Team Battle" : "チームバトル");

            if (titleLabel)    titleLabel.text    = en ? $"{host}'s Room" : $"{host}の部屋";
            if (pinValueLabel) pinValueLabel.text = RoomConfig.Pin.Length > 0 ? RoomConfig.Pin : "------";
            if (modeLabel)     modeLabel.text     = mode;

            if (hellModeBadge) hellModeBadge.SetActive(RoomConfig.IsHellMode);
        }

        void InitSlots()
        {
            bool en = LanguageSettings.IsEnglish;
            string waiting = en ? "Waiting" : "待機中";
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (i < playerNameLabels.Length   && playerNameLabels[i])
                    playerNameLabels[i].text = "---";
                if (i < playerStatusLabels.Length && playerStatusLabels[i])
                    playerStatusLabels[i].text = waiting;
                if (i < playerSlotObjects.Length  && playerSlotObjects[i])
                {
                    var img = playerSlotObjects[i].GetComponent<UnityEngine.UI.Image>();
                    if (img) img.color = SlotEmpty;
                }
            }
        }

        // ── UGS イベントハンドラ ──────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players)
        {
            InitSlots();
            for (int i = 0; i < players.Count && i < MaxPlayers; i++)
            {
                string name = "?";
                if (players[i].Data != null &&
                    players[i].Data.TryGetValue("Name", out var data))
                    name = data.Value;
                OnPlayerJoined(i, name);
            }
        }

        void HandleLobbyDeleted()
        {
            // ゲスト側：ホストが解散 → 強制退出
            if (_isLeaving) return;
            _isLeaving = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── プレイヤースロット操作 ────────────────────────────────────────────
        public void OnPlayerJoined(int slotIndex, string playerName)
        {
            if (slotIndex < 0 || slotIndex >= MaxPlayers) return;
            bool en = LanguageSettings.IsEnglish;
            if (slotIndex < playerNameLabels.Length   && playerNameLabels[slotIndex])
                playerNameLabels[slotIndex].text   = playerName;
            if (slotIndex < playerStatusLabels.Length && playerStatusLabels[slotIndex])
                playerStatusLabels[slotIndex].text = en ? "Waiting" : "待機中";
            if (slotIndex < playerSlotObjects.Length  && playerSlotObjects[slotIndex])
            {
                var img = playerSlotObjects[slotIndex].GetComponent<UnityEngine.UI.Image>();
                if (img) img.color = SlotFilled;
            }
        }

        public void OnPlayerLeft(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxPlayers) return;
            bool en = LanguageSettings.IsEnglish;
            if (slotIndex < playerNameLabels.Length   && playerNameLabels[slotIndex])
                playerNameLabels[slotIndex].text   = "---";
            if (slotIndex < playerStatusLabels.Length && playerStatusLabels[slotIndex])
                playerStatusLabels[slotIndex].text = en ? "Waiting" : "待機中";
            if (slotIndex < playerSlotObjects.Length  && playerSlotObjects[slotIndex])
            {
                var img = playerSlotObjects[slotIndex].GetComponent<UnityEngine.UI.Image>();
                if (img) img.color = SlotEmpty;
            }
        }

        // ── ボタン ────────────────────────────────────────────────────────────
        void OnStart()
        {
            // TODO: ネットワーク実装時に全プレイヤーへゲーム開始シグナルを送信
            Debug.Log("[RoomWaiting] Game Start requested");
        }

        void OnBack()
        {
            if (_isLeaving) return;
            _isLeaving = true;
            _ = BackAsync();
        }

        async System.Threading.Tasks.Task BackAsync()
        {
            if (backButton) backButton.interactable = false;
            try
            {
                if (RoomConfig.IsHost)
                    await LobbyManager.Instance.DissolveAsync();
                else
                    await LobbyManager.Instance.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RoomWaiting] BackAsync: {e.Message}");
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
