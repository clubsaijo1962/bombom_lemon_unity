using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    public class GameModeSelectController : MonoBehaviour
    {
        [SerializeField] Button          localPlayButton;
        [SerializeField] Button          createRoomButton;
        [SerializeField] Button          joinRoomButton;
        [SerializeField] Button          backButton;
        [SerializeField] Button          languageButton;
        [SerializeField] Image           languageButtonBg;
        [SerializeField] TextMeshProUGUI languageBtnLabel;
        [SerializeField] CanvasGroup     screenFade;
        [SerializeField] TextMeshProUGUI headerLabel;
        [SerializeField] TextMeshProUGUI localPlayLabel;
        [SerializeField] TextMeshProUGUI createRoomLabel;
        [SerializeField] TextMeshProUGUI joinRoomLabel;
        [SerializeField] TextMeshProUGUI orLabel;
        [SerializeField] TextMeshProUGUI guideLabel;
        [SerializeField] TextMeshProUGUI backLabel;
        [SerializeField] CanvasGroup     panelGroup;

        [SerializeField] string localSceneName  = "Game";
        [SerializeField] string titleSceneName  = "Title";

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            localPlayButton?.onClick.AddListener(OnLocalPlay);
            createRoomButton?.onClick.AddListener(OnCreateRoom);
            joinRoomButton?.onClick.AddListener(OnJoinRoom);
            backButton?.onClick.AddListener(OnBack);
            languageButton?.onClick.AddListener(OnLanguageToggle);
            LanguageSettings.OnLanguageChanged += ApplyLanguage;
            ApplyLanguage();
            StartCoroutine(FadeContentIn());
            StartCoroutine(FadeOverlayOut());
        }

        void OnDestroy() => LanguageSettings.OnLanguageChanged -= ApplyLanguage;

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (headerLabel)      headerLabel.text      = en ? "How to Play?"                  : "どうやって遊ぶ？";
            if (localPlayLabel)   localPlayLabel.text   = en ? "Play on this device"           : "このスマホ１台で遊ぶ";
            if (createRoomLabel)  createRoomLabel.text  = en ? "Create Room"                   : "部屋を立てる";
            if (joinRoomLabel)    joinRoomLabel.text    = en ? "Join Room"                     : "部屋に入る";
            if (orLabel)          orLabel.text          = en ? "— or —"                        : "― または ―";
            if (guideLabel)       guideLabel.text       = en
                ? "For multi-device play, all devices need BomBom Lemon installed."
                : "部屋に集まって遊ぶ場合は\nそれぞれのスマホにボムボムレモンが\nインストールされている必要があります";
            if (backLabel)        backLabel.text        = en ? "← Back"                        : "← 戻る";
            if (languageBtnLabel) languageBtnLabel.text = en ? "English On" : "English Off";
            if (languageButtonBg) languageButtonBg.color = en
                ? new Color(0.28f, 0.65f, 0.90f, 0.88f)
                : new Color(1f, 0.98f, 0.88f, 0.78f);
        }

        void OnLanguageToggle()
        {
            LanguageSettings.Toggle();
        }

        void OnLocalPlay() => StartCoroutine(LoadWithFade(localSceneName));
        void OnBack()      => StartCoroutine(LoadWithFade(titleSceneName));

        void OnCreateRoom() => StartCoroutine(LoadWithFade("RoomSetup"));

        void OnJoinRoom()
        {
            Debug.Log("[GameModeSelectController] Join Room (not implemented yet)");
        }

        IEnumerator FadeContentIn()
        {
            if (panelGroup) panelGroup.alpha = 0f;
            float dur = 0.30f, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (panelGroup) panelGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur);
                yield return null;
            }
            if (panelGroup) panelGroup.alpha = 1f;
        }

        IEnumerator LoadWithFade(string sceneName)
        {
            if (screenFade != null)
            {
                screenFade.blocksRaycasts = true;
                float dur = 0.30f, t = 0f;
                while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
                screenFade.alpha = 1f;
            }
            SceneManager.LoadScene(sceneName);
        }

        IEnumerator FadeOverlayOut()
        {
            if (screenFade == null) yield break;
            float dur = 0.35f, t = 0f;
            while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(1f, 0f, t / dur); yield return null; }
            screenFade.alpha = 0f;
            screenFade.blocksRaycasts = false;
        }
    }
}
