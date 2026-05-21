using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    public class GuessInputController : MonoBehaviour
    {
        [Header("お題")]
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI topicLowLabel;
        [SerializeField] TextMeshProUGUI topicHighLabel;

        [Header("予想の最終決定者")]
        [SerializeField] TextMeshProUGUI finalGuesserLabel;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;

        [Header("入力")]
        [SerializeField] TMP_InputField numberInputField;

        [Header("ボタン")]
        [SerializeField] Button confirmButton;
        [SerializeField] Button homeButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyData();

            confirmButton?.onClick.AddListener(OnConfirm);
            homeButton?.onClick.AddListener(OnHome);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyData()
        {
            bool en = LanguageSettings.IsEnglish;

            if (topicLabel)
                topicLabel.text = en ? SinglePlayConfig.TopicTextEN : SinglePlayConfig.TopicTextJP;
            if (topicLowLabel)
                topicLowLabel.text = en ? SinglePlayConfig.TopicLowEN : SinglePlayConfig.TopicLowJP;
            if (topicHighLabel)
                topicHighLabel.text = en ? SinglePlayConfig.TopicHighEN : SinglePlayConfig.TopicHighJP;

            if (finalGuesserLabel)
                finalGuesserLabel.text = SinglePlayConfig.GetNextFinalGuesserName();

            if (lifeCountLabel)
            {
                lifeCountLabel.text = $"×{SinglePlayConfig.LifeCount}";
                lifeCountLabel.enableWordWrapping = false;
                lifeCountLabel.enableAutoSizing = true;
                lifeCountLabel.fontSizeMin = 32f;
                lifeCountLabel.fontSizeMax = 42f;
            }
            if (helpCardCountLabel)
            {
                helpCardCountLabel.text = $"×{SinglePlayConfig.HelpCardCount}";
                helpCardCountLabel.enableWordWrapping = false;
            }
        }

        void OnConfirm()
        {
            string raw = numberInputField != null ? numberInputField.text.Trim() : "";
            if (!int.TryParse(raw, out int guess) || guess < 1 || guess > 99)
                return;

            StartCoroutine(LoadWithFade("Game"));
        }

        void OnHome() => StartCoroutine(LoadWithFade("SingleSettings"));

        IEnumerator FadeOverlayOut()
        {
            if (screenFade == null) yield break;
            float dur = 0.35f, t = 0f;
            while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(1f, 0f, t / dur); yield return null; }
            screenFade.alpha = 0f;
            screenFade.blocksRaycasts = false;
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
