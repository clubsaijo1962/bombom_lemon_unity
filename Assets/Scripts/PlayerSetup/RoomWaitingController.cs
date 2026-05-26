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
    /// プレイヤースロットは手動配置（最大24人）。
    /// </summary>
    public class RoomWaitingController : MonoBehaviour
    {
        const int   MaxPlayers   = 24;
        const float SlotHeight   = 72f;
        const float SlotGap      = 8f;
        const float SlotPadTop   = 4f;
        const float SlotPadBot   = 4f;

        [Header("ルーム情報")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI pinValueLabel;
        [SerializeField] TextMeshProUGUI modeLabel;

        [Header("地獄モード")]
        [SerializeField] GameObject hellModeBadge;

        [Header("参加プレイヤーリスト（動的生成）")]
        [SerializeField] RectTransform   playerListContent;
        [SerializeField] Sprite          playerSlotSprite;
        [SerializeField] TMP_FontAsset   playerSlotFont;
        [SerializeField] TextMeshProUGUI playerCountLabel;

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

        static readonly Color SlotEmpty   = new(0.95f, 0.95f, 0.95f, 0.55f);
        static readonly Color SlotFilled  = new(1.00f, 0.97f, 0.90f, 0.92f);
        static readonly Color TextPrimary = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted   = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color StatusColor = new(0.45f, 0.28f, 0.08f, 0.60f);

        bool _isLeaving;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            // VLG / ContentSizeFitter は手動配置のため無効化
            if (playerListContent != null)
            {
                var csf = playerListContent.GetComponent<ContentSizeFitter>();
                if (csf != null) csf.enabled = false;
                var vlg = playerListContent.GetComponent<VerticalLayoutGroup>();
                if (vlg != null) vlg.enabled = false;
            }

            ApplyLanguage();
            ShowRoomInfo();

            LobbyManager.Instance.OnPlayersUpdated  += HandlePlayersUpdated;
            LobbyManager.Instance.OnLobbyDeleted    += HandleLobbyDeleted;
            LobbyManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            // 購読直後に現在のロビー状態を即反映（ポーリング待ち不要）
            var initPlayers = LobbyManager.Instance.CurrentLobby?.Players
                              ?? new List<LobbyPlayer>();
            RebuildPlayerList(initPlayers);

            startButton?.onClick.AddListener(OnStart);
            backButton?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayersUpdated   -= HandlePlayersUpdated;
                LobbyManager.Instance.OnLobbyDeleted     -= HandleLobbyDeleted;
                LobbyManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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

        // ── UGS イベントハンドラ ──────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players) => RebuildPlayerList(players);

        void HandleLobbyDeleted()
        {
            if (_isLeaving) return;
            _isLeaving = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── プレイヤーリスト（手動配置）────────────────────────────────────────
        void RebuildPlayerList(List<LobbyPlayer> players)
        {
            if (playerListContent == null) return;

            // 既存スロットを即座に削除
            for (int i = playerListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(playerListContent.GetChild(i).gameObject);

            if (playerCountLabel != null)
                playerCountLabel.text = $"{players.Count} / {MaxPlayers}";

            bool en = LanguageSettings.IsEnglish;

            // 表示アイテムリスト（実プレイヤー + 空枠最大6）
            var items = new List<(string name, bool filled)>();
            foreach (var p in players)
            {
                string name = "?";
                if (p.Data != null && p.Data.TryGetValue("Name", out var d)) name = d.Value;
                items.Add((name, true));
            }
            int empty = Mathf.Min(MaxPlayers - players.Count, 6);
            for (int i = 0; i < empty; i++) items.Add((null, false));

            // Content の高さを手動設定（VLG/CSF は無効化済み）
            int n = items.Count;
            float totalH = n > 0
                ? SlotPadTop + n * SlotHeight + (n - 1) * SlotGap + SlotPadBot
                : SlotPadTop + SlotPadBot;
            playerListContent.sizeDelta = new Vector2(0f, totalH);

            // スロットを上から順に配置
            for (int i = 0; i < n; i++)
            {
                float yTop = -(SlotPadTop + i * (SlotHeight + SlotGap));
                AddSlot(items[i].name, items[i].filled, en, yTop);
            }
        }

        void AddSlot(string playerName, bool filled, bool en, float yTop)
        {
            var go = new GameObject("Slot", typeof(RectTransform));
            go.transform.SetParent(playerListContent, false);

            // 手動配置：Content 上端アンカー、全幅、高さ SlotHeight
            var r = go.GetComponent<RectTransform>();
            r.anchorMin       = new Vector2(0f, 1f);
            r.anchorMax       = new Vector2(1f, 1f);
            r.pivot           = new Vector2(0.5f, 1f);
            r.sizeDelta       = new Vector2(0f, SlotHeight);
            r.anchoredPosition = new Vector2(0f, yTop);

            var bg = go.AddComponent<Image>();
            if (playerSlotSprite != null) { bg.sprite = playerSlotSprite; bg.type = Image.Type.Sliced; }
            bg.color = filled ? SlotFilled : SlotEmpty;
            bg.raycastTarget = false;

            // 名前ラベル（左65%）
            var nGO = new GameObject("Name", typeof(RectTransform));
            nGO.transform.SetParent(go.transform, false);
            var nr = nGO.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f); nr.anchorMax = new Vector2(0.65f, 1f);
            nr.offsetMin = new Vector2(24f, 4f); nr.offsetMax = new Vector2(-4f, -4f);
            var nameTmp = nGO.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = playerName ?? "---";
            nameTmp.fontSize  = 36f;
            nameTmp.fontStyle = filled ? FontStyles.Bold : FontStyles.Normal;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.color     = filled ? TextPrimary : TextMuted;
            nameTmp.raycastTarget = false;
            if (playerSlotFont != null) nameTmp.font = playerSlotFont;

            // ステータスラベル（右35%）
            var sGO = new GameObject("Status", typeof(RectTransform));
            sGO.transform.SetParent(go.transform, false);
            var sr = sGO.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.65f, 0f); sr.anchorMax = new Vector2(1f, 1f);
            sr.offsetMin = new Vector2(4f, 4f); sr.offsetMax = new Vector2(-24f, -4f);
            var statusTmp = sGO.AddComponent<TextMeshProUGUI>();
            statusTmp.text      = filled ? (en ? "Waiting" : "待機中") : "";
            statusTmp.fontSize  = 32f;
            statusTmp.alignment = TextAlignmentOptions.MidlineRight;
            statusTmp.color     = StatusColor;
            statusTmp.raycastTarget = false;
            if (playerSlotFont != null) statusTmp.font = playerSlotFont;
        }

        // ── UGS ゲーム状態ハンドラ ───────────────────────────────────────────
        void HandleGameStateChanged(string state)
        {
            if (_isLeaving) return;
            // ゲストはポーリング経由で "Confirming" を検知してシーン遷移
            if (state == "Confirming" && !RoomConfig.IsHost)
            {
                _isLeaving = true;
                StartCoroutine(LoadWithFade("MultiConfirm"));
            }
        }

        // ── ボタン ────────────────────────────────────────────────────────────
        void OnStart()
        {
            if (!RoomConfig.IsHost) return;
            _ = StartGameAsync();
        }

        async System.Threading.Tasks.Task StartGameAsync()
        {
            if (startButton) startButton.interactable = false;
            if (startBtnLabel) startBtnLabel.text = "…";
            try
            {
                await LobbyManager.Instance.StartConfirmPhaseAsync();
                _isLeaving = true;
                StartCoroutine(LoadWithFade("MultiConfirm"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[RoomWaiting] StartGameAsync: {e.Message}");
                if (startButton) startButton.interactable = true;
                if (startBtnLabel) startBtnLabel.text =
                    LanguageSettings.IsEnglish ? "Start Game ▶" : "ゲームスタート ▶";
            }
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
