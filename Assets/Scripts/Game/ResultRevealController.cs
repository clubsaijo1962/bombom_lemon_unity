using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    public class ResultRevealController : MonoBehaviour
    {
        [Header("予想数字カード（左）")]
        [SerializeField] CanvasGroup guessedGroup;
        [SerializeField] TextMeshProUGUI guessedNumberLabel;

        [Header("秘密数字カード（右）")]
        [SerializeField] CanvasGroup secretGroup;
        [SerializeField] TextMeshProUGUI secretNumberLabel;

        [Header("差・ライフ変化")]
        [SerializeField] CanvasGroup diffGroup;
        [SerializeField] TextMeshProUGUI diffLabel;
        [SerializeField] TextMeshProUGUI lifeChangeLabel;
        [SerializeField] TextMeshProUGUI lifeCountLabel;

        [Header("爆発エフェクト（bomb.png使用）")]
        [SerializeField] RectTransform explosionSmall;
        [SerializeField] RectTransform explosionLarge;

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

            if (guessedGroup) { guessedGroup.alpha = 0f; guessedGroup.blocksRaycasts = false; }
            if (secretGroup)  { secretGroup.alpha  = 0f; secretGroup.blocksRaycasts  = false; }
            if (diffGroup)    { diffGroup.alpha     = 0f; diffGroup.blocksRaycasts    = false; }

            if (explosionSmall) explosionSmall.gameObject.SetActive(false);
            if (explosionLarge) explosionLarge.gameObject.SetActive(false);

            if (lifeCountLabel)
                lifeCountLabel.text = $"×{SinglePlayConfig.CurrentLife}";

            nextButton?.onClick.AddListener(OnNext);
            homeButton?.onClick.AddListener(OnHome);
            if (nextButton) nextButton.gameObject.SetActive(false);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
            StartCoroutine(RevealSequence());
        }

        IEnumerator RevealSequence()
        {
            yield return new WaitForSeconds(0.5f);

            // 1. 予想数字（左）表示
            if (guessedNumberLabel)
                guessedNumberLabel.text = SinglePlayConfig.GuessedNumber.ToString();
            yield return StartCoroutine(FadeGroup(guessedGroup, 0f, 1f, 0.35f));
            if (guessedGroup) guessedGroup.blocksRaycasts = true;

            // 2. 2秒後に秘密数字（右）表示
            yield return new WaitForSeconds(2f);
            if (secretNumberLabel)
                secretNumberLabel.text = SinglePlayConfig.SecretNumber.ToString();
            yield return StartCoroutine(FadeGroup(secretGroup, 0f, 1f, 0.35f));
            if (secretGroup) secretGroup.blocksRaycasts = true;

            // 3. 1秒後に差・ライフ変化表示
            yield return new WaitForSeconds(1f);

            int diff      = Mathf.Abs(SinglePlayConfig.GuessedNumber - SinglePlayConfig.SecretNumber);
            int lifeLoss  = diff;
            bool en       = LanguageSettings.IsEnglish;

            if (diffLabel)
                diffLabel.text = en ? $"Difference: {diff}" : $"差: {diff}";
            if (lifeChangeLabel)
                lifeChangeLabel.text = en ? $"−{lifeLoss} life points" : $"ライフ −{lifeLoss}";

            yield return StartCoroutine(FadeGroup(diffGroup, 0f, 1f, 0.30f));
            if (diffGroup) diffGroup.blocksRaycasts = true;

            yield return StartCoroutine(ExplodeAndReduceLife(diff, lifeLoss));

            if (nextButton) nextButton.gameObject.SetActive(true);
        }

        IEnumerator ExplodeAndReduceLife(int diff, int lifeLoss)
        {
            bool big      = diff >= 5;
            var explosion = big ? explosionLarge : explosionSmall;

            if (explosion != null)
            {
                explosion.gameObject.SetActive(true);
                yield return StartCoroutine(AnimateExplosion(explosion, big));
                explosion.gameObject.SetActive(false);
            }

            int startLife  = SinglePlayConfig.CurrentLife;
            int targetLife = Mathf.Max(0, startLife - lifeLoss);
            float dur      = Mathf.Max(0.5f, lifeLoss * 0.12f);
            float elapsed  = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                int current = Mathf.RoundToInt(Mathf.Lerp(startLife, targetLife, elapsed / dur));
                if (lifeCountLabel) lifeCountLabel.text = $"×{current}";
                yield return null;
            }

            SinglePlayConfig.CurrentLife = targetLife;
            if (lifeCountLabel) lifeCountLabel.text = $"×{targetLife}";
        }

        IEnumerator AnimateExplosion(RectTransform explosion, bool big)
        {
            float dur      = big ? 0.8f : 0.5f;
            float maxScale = big ? 2.2f : 1.6f;
            var img        = explosion.GetComponent<Image>();
            float elapsed  = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t     = elapsed / dur;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * maxScale;
                explosion.localScale = Vector3.one * scale;
                if (img) img.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }

            explosion.localScale = Vector3.one;
            if (img) img.color = Color.white;
        }

        IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
        {
            if (cg == null) yield break;
            float t = 0f;
            cg.alpha = from;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            cg.alpha = to;
        }

        void OnNext() => StartCoroutine(LoadWithFade("Game"));
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
