using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    public class NumberConfirmController : MonoBehaviour
    {
        [Header("プレイヤー")]
        [SerializeField] TextMeshProUGUI playerNameLabel;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;
        [SerializeField] TextMeshProUGUI roundLabel;

        [Header("秘密の数字")]
        [SerializeField] CanvasGroup questionGroup;
        [SerializeField] CanvasGroup numberGroup;
        [SerializeField] Button      revealButton;
        [SerializeField] TextMeshProUGUI secretNumberLabel;

        [Header("ボタン")]
        [SerializeField] Button nextButton;
        [SerializeField] Button homeButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;
            if (questionGroup) { questionGroup.alpha = 1f; questionGroup.blocksRaycasts = true; }
            if (numberGroup)   { numberGroup.alpha   = 0f; numberGroup.blocksRaycasts  = false; }
            if (nextButton)    nextButton.gameObject.SetActive(false);

            ApplyData();

            revealButton?.onClick.AddListener(OnReveal);
            nextButton?.onClick.AddListener(OnNext);
            homeButton?.onClick.AddListener(OnHome);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyData()
        {
            if (playerNameLabel)
                playerNameLabel.text = SinglePlayConfig.CurrentAnswerName;
            if (lifeCountLabel)
            {
                lifeCountLabel.text = $"×{SinglePlayConfig.CurrentLife}";
                lifeCountLabel.enableWordWrapping = false;
                lifeCountLabel.enableAutoSizing = true;
                lifeCountLabel.fontSizeMin = 32f;
                lifeCountLabel.fontSizeMax = 42f;
            }
            if (helpCardCountLabel)
            {
                helpCardCountLabel.text = $"×{SinglePlayConfig.CurrentHelpCards}";
                helpCardCountLabel.enableWordWrapping = false;
            }
            if (roundLabel)
                roundLabel.text = $"{SinglePlayConfig.CurrentRound}/{SinglePlayConfig.TotalRounds}ラウンド目";
            if (secretNumberLabel)
            {
                if (SinglePlayConfig.SecretNumber == 0)
                    SinglePlayConfig.SetRound(SinglePlayConfig.CurrentAnswerName, Random.Range(1, 100));
                secretNumberLabel.text = SinglePlayConfig.SecretNumber.ToString();
            }
        }

        void OnReveal() => StartCoroutine(RevealSequence());

        IEnumerator RevealSequence()
        {
            if (questionGroup != null)
            {
                float dur = 0.18f, t = 0f;
                while (t < dur) { t += Time.deltaTime; questionGroup.alpha = Mathf.SmoothStep(1f, 0f, t / dur); yield return null; }
                questionGroup.alpha = 0f;
                questionGroup.blocksRaycasts = false;
            }
            if (numberGroup != null)
            {
                float dur = 0.28f, t = 0f;
                while (t < dur) { t += Time.deltaTime; numberGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
                numberGroup.alpha = 1f;
                numberGroup.blocksRaycasts = true;
            }
            if (nextButton) nextButton.gameObject.SetActive(true);
        }

        void OnNext() => StartCoroutine(LoadWithFade("GuessInput"));
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
