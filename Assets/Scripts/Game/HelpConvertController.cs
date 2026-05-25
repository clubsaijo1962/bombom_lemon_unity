using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;
using BomBomLemon.Audio;

namespace BomBomLemon.Game
{
    public class HelpConvertController : MonoBehaviour
    {
        [Header("表示")]
        [SerializeField] TextMeshProUGUI cardCountLabel;
        [SerializeField] TextMeshProUGUI gainLabel;
        [SerializeField] TextMeshProUGUI lifeAfterLabel;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;
        [SerializeField] TextMeshProUGUI roundLabel;

        [Header("ボタン")]
        [SerializeField] Button continueButton;
        [SerializeField] Button homeButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;
            if (continueButton) continueButton.gameObject.SetActive(false);

            continueButton?.onClick.AddListener(OnContinue);
            homeButton?.onClick.AddListener(OnHome);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
            StartCoroutine(ConvertSequence());
        }

        IEnumerator ConvertSequence()
        {
            bool en = LanguageSettings.IsEnglish;
            int cards    = SinglePlayConfig.CurrentHelpCards;
            int lifeFrom = SinglePlayConfig.CurrentLife;
            int lifeTo   = lifeFrom + cards;

            if (lifeCountLabel)     lifeCountLabel.text     = $"×{lifeFrom}";
            if (helpCardCountLabel) helpCardCountLabel.text  = $"×{cards}";
            if (roundLabel)         roundLabel.text          = $"{SinglePlayConfig.CurrentRound}/{SinglePlayConfig.TotalRounds}ラウンド目";
            if (cardCountLabel)     cardCountLabel.text      = en ? $"× {cards}" : $"× {cards}枚";
            if (gainLabel)          gainLabel.text           = en ? $"Life +{cards}" : $"ライフ +{cards}";
            if (lifeAfterLabel)     lifeAfterLabel.gameObject.SetActive(false);

            yield return new WaitForSeconds(1.2f);

            // ライフアニメーション
            float dur = Mathf.Max(0.6f, cards * 0.12f);
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                int cur = Mathf.RoundToInt(Mathf.Lerp(lifeFrom, lifeTo, elapsed / dur));
                if (lifeCountLabel) lifeCountLabel.text = $"×{cur}";
                yield return null;
            }
            if (lifeCountLabel) lifeCountLabel.text = $"×{lifeTo}";
            AudioManager.Instance?.PlayLemonGet();

            // 状態更新
            SinglePlayConfig.CurrentLife      = lifeTo;
            SinglePlayConfig.CurrentHelpCards = 0;
            if (helpCardCountLabel) helpCardCountLabel.text = "×0";

            if (lifeAfterLabel)
            {
                lifeAfterLabel.text = en ? $"Life: {lifeTo}" : $"残りライフ: {lifeTo}";
                lifeAfterLabel.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.4f);
            if (continueButton) continueButton.gameObject.SetActive(true);
        }

        void OnContinue() => StartCoroutine(LoadWithFade("Game"));
        void OnHome()     => StartCoroutine(LoadWithFade("SingleSettings"));

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
