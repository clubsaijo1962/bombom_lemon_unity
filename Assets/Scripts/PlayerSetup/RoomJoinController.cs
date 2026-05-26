using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// 部屋に入る画面。プレイヤー名と暗証番号（6桁）を入力して入室する。
    /// </summary>
    public class RoomJoinController : MonoBehaviour
    {
        [Header("名前入力")]
        [SerializeField] TMP_InputField  playerNameInputField;
        [SerializeField] TextMeshProUGUI nameHeaderLabel;

        [Header("PIN入力")]
        [SerializeField] TMP_InputField  pinInputField;
        [SerializeField] TextMeshProUGUI pinHeaderLabel;
        [SerializeField] TextMeshProUGUI pinErrorLabel;

        [Header("多言語ラベル")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI confirmBtnLabel;
        [SerializeField] TextMeshProUGUI backBtnLabel;

        [Header("ボタン")]
        [SerializeField] Button confirmButton;
        [SerializeField] Button backButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;
            if (pinErrorLabel) pinErrorLabel.gameObject.SetActive(false);

            ApplyLanguage();

            confirmButton?.onClick.AddListener(OnConfirm);
            backButton?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (titleLabel)      titleLabel.text      = en ? "Join Room"            : "部屋に入る";
            if (nameHeaderLabel) nameHeaderLabel.text = en ? "Your Name"            : "あなたの名前";
            if (pinHeaderLabel)  pinHeaderLabel.text  = en ? "Room PIN  (6 digits)" : "暗証番号（6桁）";
            if (confirmBtnLabel) confirmBtnLabel.text = en ? "Join ▶"              : "入室する ▶";
            if (backBtnLabel)    backBtnLabel.text    = en ? "← Back"              : "← 戻る";
        }

        void OnConfirm()
        {
            string pin = pinInputField != null ? pinInputField.text.Trim() : "";
            if (pin.Length != 6)
            {
                if (pinErrorLabel)
                {
                    bool en = LanguageSettings.IsEnglish;
                    pinErrorLabel.text = en ? "Please enter a 6-digit PIN." : "6桁の数字を入力してください";
                    pinErrorLabel.gameObject.SetActive(true);
                }
                return;
            }
            if (pinErrorLabel) pinErrorLabel.gameObject.SetActive(false);

            // ローカルチェック：このセッションで立てた部屋と照合（ネットワーク実装前の仮実装）
            if (RoomConfig.Pin.Length > 0 && pin != RoomConfig.Pin)
            {
                ShowRoomNotFound();
                return;
            }

            string name = playerNameInputField != null ? playerNameInputField.text.Trim() : "";
            if (name.Length == 0) name = LanguageSettings.IsEnglish ? "Player" : "プレイヤー";

            RoomConfig.Pin      = pin;
            RoomConfig.HostName = name;

            // TODO: ネットワーク実装時 - PINで部屋を検索して入室
            Debug.Log($"[RoomJoin] Name={name} PIN={pin}");
            StartCoroutine(LoadWithFade("RoomWaiting"));
        }

        /// <summary>指定したPINの部屋が見つからない場合にエラーを表示する（ネットワーク実装時に呼び出す）</summary>
        public void ShowRoomNotFound()
        {
            bool en = LanguageSettings.IsEnglish;
            if (pinErrorLabel)
            {
                pinErrorLabel.text = en
                    ? "Room not found. Please check the PIN."
                    : "その部屋は存在しません。\n暗証番号をご確認ください。";
                pinErrorLabel.gameObject.SetActive(true);
            }
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
