using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// 部屋待機画面。入室プレイヤーをリアルタイム表示し、ホストがゲームスタートを行う。
    /// </summary>
    public class RoomWaitingController : MonoBehaviour
    {
        const int MaxPlayers = 6;

        [Header("ルーム情報")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI pinValueLabel;
        [SerializeField] TextMeshProUGUI modeLabel;

        [Header("参加プレイヤー")]
        [SerializeField] GameObject[]      playerSlotObjects;
        [SerializeField] TextMeshProUGUI[] playerNameLabels;
        [SerializeField] TextMeshProUGUI[] playerStatusLabels;

        [Header("地獄モード")]
        [SerializeField] GameObject hellModeBadge;

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

        // スロットのベース背景色
        static readonly Color SlotEmpty  = new Color(0.95f, 0.95f, 0.95f, 0.55f);
        static readonly Color SlotFilled = new Color(1.00f, 0.97f, 0.90f, 0.92f);

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyLanguage();
            ShowRoomInfo();
            InitSlots();

            startButton?.onClick.AddListener(OnStart);
            backButton?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            // titleLabel はShowRoomInfo()でHostName込みで設定するためここでは設定しない
            if (playersHeaderLabel) playersHeaderLabel.text = en ? "Players"       : "参加プレイヤー";
            if (startBtnLabel)      startBtnLabel.text      = en ? "Start Game ▶" : "ゲームスタート ▶";
            if (backBtnLabel)       backBtnLabel.text       = en ? "← Dissolve"   : "← 解散";
        }

        void ShowRoomInfo()
        {
            bool en = LanguageSettings.IsEnglish;
            string host = RoomConfig.HostName.Length > 0 ? RoomConfig.HostName : (en ? "Host" : "ホスト");
            string pin  = RoomConfig.Pin;
            string mode = RoomConfig.Mode == RoomConfig.GameMode.CoopLife
                ? (en ? "Coop Mode"   : "協力モード")
                : (en ? "Team Battle" : "チームバトル");

            if (titleLabel)    titleLabel.text    = en ? $"{host}'s Room" : $"{host}の部屋";
            if (pinValueLabel) pinValueLabel.text = pin.Length > 0 ? pin : "------";
            if (modeLabel)     modeLabel.text     = mode;

            // 地獄モードバッジ表示（協力モード時のみ有効）
            if (hellModeBadge) hellModeBadge.SetActive(RoomConfig.IsHellMode);
        }

        void InitSlots()
        {
            bool en = LanguageSettings.IsEnglish;
            string waiting = en ? "Waiting" : "待機中";

            for (int i = 0; i < MaxPlayers; i++)
            {
                if (i < playerNameLabels.Length   && playerNameLabels[i])
                    playerNameLabels[i].text   = "---";
                if (i < playerStatusLabels.Length && playerStatusLabels[i])
                    playerStatusLabels[i].text = waiting;

                // 空スロットは薄く表示
                if (i < playerSlotObjects.Length  && playerSlotObjects[i])
                {
                    var img = playerSlotObjects[i].GetComponent<UnityEngine.UI.Image>();
                    if (img) img.color = SlotEmpty;
                }
            }
        }

        /// <summary>プレイヤーが入室したとき呼び出す（ネットワーク実装時）</summary>
        public void OnPlayerJoined(int slotIndex, string playerName)
        {
            if (slotIndex < 0 || slotIndex >= MaxPlayers) return;
            bool en = LanguageSettings.IsEnglish;

            if (slotIndex < playerNameLabels.Length   && playerNameLabels[slotIndex])
                playerNameLabels[slotIndex].text   = playerName;
            if (slotIndex < playerStatusLabels.Length && playerStatusLabels[slotIndex])
                playerStatusLabels[slotIndex].text = en ? "Waiting" : "待機中";

            // 入室済みスロットは背景をウォームに
            if (slotIndex < playerSlotObjects.Length  && playerSlotObjects[slotIndex])
            {
                var img = playerSlotObjects[slotIndex].GetComponent<UnityEngine.UI.Image>();
                if (img) img.color = SlotFilled;
            }
        }

        /// <summary>プレイヤーが退室したとき呼び出す（ネットワーク実装時）</summary>
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

        void OnStart()
        {
            // TODO: ネットワーク実装時に全プレイヤーへゲーム開始シグナルを送信
            Debug.Log("[RoomWaiting] Game Start requested");
        }

        void OnBack() => StartCoroutine(LoadWithFade("PlayerSetup"));

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
