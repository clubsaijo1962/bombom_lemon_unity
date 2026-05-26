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
    /// プレイヤースロットは動的生成（最大24人）。
    /// </summary>
    public class RoomWaitingController : MonoBehaviour
    {
        const int MaxPlayers = 24;

        [Header("ルーム情報")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI pinValueLabel;
        [SerializeField] TextMeshProUGUI modeLabel;

        [Header("地獄モード")]
        [SerializeField] GameObject hellModeBadge;

        [Header("参加プレイヤーリスト（動的生成）")]
        [SerializeField] RectTransform   playerListContent;  // ScrollRect の Content
        [SerializeField] Sprite          playerSlotSprite;   // 角丸スプライト（SceneBuilder がワイヤー）
        [SerializeField] TMP_FontAsset   playerSlotFont;     // フォント（SceneBuilder がワイヤー）
        [SerializeField] TextMeshProUGUI playerCountLabel;   // "N / 24" 表示

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

        // スロット色
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

            ApplyLanguage();
            ShowRoomInfo();

            // UGS イベント購読
            LobbyManager.Instance.OnPlayersUpdated += HandlePlayersUpdated;
            LobbyManager.Instance.OnLobbyDeleted   += HandleLobbyDeleted;

            // 購読直後に現在のロビー状態をすぐ反映（ポーリング待ち不要）
            if (LobbyManager.Instance.CurrentLobby != null)
                HandlePlayersUpdated(LobbyManager.Instance.CurrentLobby.Players);
            else
                RebuildPlayerList(new List<LobbyPlayer>());

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

        // ── UGS イベントハンドラ ──────────────────────────────────────────────
        void HandlePlayersUpdated(List<LobbyPlayer> players)
        {
            RebuildPlayerList(players);
        }

        void HandleLobbyDeleted()
        {
            // ゲスト側：ホストが解散 → 強制退出
            if (_isLeaving) return;
            _isLeaving = true;
            StartCoroutine(LoadWithFade("PlayerSetup"));
        }

        // ── プレイヤーリスト動的生成 ──────────────────────────────────────────
        void RebuildPlayerList(List<LobbyPlayer> players)
        {
            if (playerListContent == null) return;

            // Destroy() は次フレームまで遅延するため DestroyImmediate で即座に削除
            for (int i = playerListContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(playerListContent.GetChild(i).gameObject);

            // カウントラベル更新
            if (playerCountLabel != null)
                playerCountLabel.text = $"{players.Count} / {MaxPlayers}";

            bool en = LanguageSettings.IsEnglish;

            // 入室プレイヤー分のスロットを生成
            foreach (var p in players)
            {
                string name = "?";
                if (p.Data != null && p.Data.TryGetValue("Name", out var d))
                    name = d.Value;
                AddSlot(name, en);
            }

            // 視認性のため残り枠を空スロットで表示（最大6枠）
            int emptyCount = Mathf.Min(MaxPlayers - players.Count, 6);
            for (int i = 0; i < emptyCount; i++)
                AddSlot(null, en);

            // ContentSizeFitter / VerticalLayoutGroup を同フレーム内で即座に再計算
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerListContent);
        }

        void AddSlot(string playerName, bool en)
        {
            var go = new GameObject("Slot", typeof(RectTransform));
            go.transform.SetParent(playerListContent, false);

            // LayoutElement で高さを指定
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 72f;
            le.minHeight       = 72f;

            // 背景
            var bg = go.AddComponent<Image>();
            if (playerSlotSprite != null) { bg.sprite = playerSlotSprite; bg.type = Image.Type.Sliced; }
            bg.color = (playerName != null) ? SlotFilled : SlotEmpty;
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
            nameTmp.fontStyle = (playerName != null) ? FontStyles.Bold : FontStyles.Normal;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.color     = (playerName != null) ? TextPrimary : TextMuted;
            nameTmp.raycastTarget = false;
            if (playerSlotFont != null) nameTmp.font = playerSlotFont;

            // ステータスラベル（右35%）
            var sGO = new GameObject("Status", typeof(RectTransform));
            sGO.transform.SetParent(go.transform, false);
            var sr = sGO.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.65f, 0f); sr.anchorMax = new Vector2(1f, 1f);
            sr.offsetMin = new Vector2(4f, 4f); sr.offsetMax = new Vector2(-24f, -4f);
            var statusTmp = sGO.AddComponent<TextMeshProUGUI>();
            statusTmp.text      = (playerName != null) ? (en ? "Waiting" : "待機中") : "";
            statusTmp.fontSize  = 32f;
            statusTmp.alignment = TextAlignmentOptions.MidlineRight;
            statusTmp.color     = StatusColor;
            statusTmp.raycastTarget = false;
            if (playerSlotFont != null) statusTmp.font = playerSlotFont;
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
