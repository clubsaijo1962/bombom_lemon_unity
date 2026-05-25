using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// 部屋を立てる画面。暗証番号（6桁）とゲームモードを設定して確定する。
    /// </summary>
    public class RoomSetupController : MonoBehaviour
    {
        [Header("PIN入力")]
        [SerializeField] TMP_InputField  pinInputField;
        [SerializeField] TextMeshProUGUI pinErrorLabel;

        [Header("ゲームモード選択")]
        [SerializeField] Button coopLifeButton;
        [SerializeField] Button teamBattleButton;
        [SerializeField] Image  coopLifeBg;
        [SerializeField] Image  teamBattleBg;
        [SerializeField] TextMeshProUGUI coopLifeLabel;
        [SerializeField] TextMeshProUGUI coopLifeDescLabel;
        [SerializeField] TextMeshProUGUI teamBattleLabel;
        [SerializeField] TextMeshProUGUI teamBattleDescLabel;

        [Header("多言語ラベル")]
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI pinHeaderLabel;
        [SerializeField] TextMeshProUGUI modeHeaderLabel;
        [SerializeField] TextMeshProUGUI confirmBtnLabel;
        [SerializeField] TextMeshProUGUI backBtnLabel;

        [Header("ボタン")]
        [SerializeField] Button confirmButton;
        [SerializeField] Button backButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        static readonly Color SelectedBg     = new Color(0.97f, 0.82f, 0.10f);
        static readonly Color UnselectedBg   = new Color(0.99f, 0.95f, 0.72f);
        static readonly Color SelectedText   = new Color(0.20f, 0.10f, 0.02f);
        static readonly Color UnselectedText = new Color(0.45f, 0.28f, 0.08f, 0.72f);

        RoomConfig.GameMode _selectedMode = RoomConfig.GameMode.CoopLife;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;
            if (pinErrorLabel) pinErrorLabel.gameObject.SetActive(false);

            ApplyLanguage();
            SelectMode(RoomConfig.GameMode.CoopLife);

            coopLifeButton?.onClick.AddListener(() => SelectMode(RoomConfig.GameMode.CoopLife));
            teamBattleButton?.onClick.AddListener(() => SelectMode(RoomConfig.GameMode.TeamBattle));
            confirmButton?.onClick.AddListener(OnConfirm);
            backButton?.onClick.AddListener(OnBack);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (titleLabel)         titleLabel.text         = en ? "Create Room"              : "部屋を立てる";
            if (pinHeaderLabel)     pinHeaderLabel.text     = en ? "Room PIN  (6 digits)"     : "暗証番号（6桁）";
            if (modeHeaderLabel)    modeHeaderLabel.text    = en ? "Game Mode"                : "ゲームモード";
            if (coopLifeLabel)      coopLifeLabel.text      = en ? "Protect Life Together"   : "全員でライフを守る";
            if (coopLifeDescLabel)  coopLifeDescLabel.text  = en ? "Cooperate to keep your lives up!"
                                                                 : "チームみんなでライフを守る協力ゲーム";
            if (teamBattleLabel)    teamBattleLabel.text    = en ? "Team Battle"              : "チームバトル";
            if (teamBattleDescLabel)teamBattleDescLabel.text= en ? "Split into 2 teams — fewest total diff wins!"
                                                                 : "2チームに分かれて差の合計が少ない方が勝ち";
            if (confirmBtnLabel)    confirmBtnLabel.text    = en ? "Confirm ▶"               : "確定する ▶";
            if (backBtnLabel)       backBtnLabel.text       = en ? "← Back"                  : "← 戻る";
        }

        void SelectMode(RoomConfig.GameMode mode)
        {
            _selectedMode = mode;

            bool coop = mode == RoomConfig.GameMode.CoopLife;
            if (coopLifeBg)    coopLifeBg.color    = coop  ? SelectedBg   : UnselectedBg;
            if (teamBattleBg)  teamBattleBg.color  = !coop ? SelectedBg   : UnselectedBg;
            if (coopLifeLabel) coopLifeLabel.color  = coop  ? SelectedText : UnselectedText;
            if (coopLifeDescLabel) coopLifeDescLabel.color = coop ? SelectedText : UnselectedText;
            if (teamBattleLabel) teamBattleLabel.color  = !coop ? SelectedText : UnselectedText;
            if (teamBattleDescLabel) teamBattleDescLabel.color = !coop ? SelectedText : UnselectedText;
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

            RoomConfig.Pin  = pin;
            RoomConfig.Mode = _selectedMode;

            // TODO: ネットワーク実装時にここで部屋を作成する
            Debug.Log($"[RoomSetup] PIN={pin} Mode={_selectedMode}");
            StartCoroutine(LoadWithFade("RoomWaiting"));
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
