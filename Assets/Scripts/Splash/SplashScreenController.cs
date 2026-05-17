using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace BomBomLemon.Splash
{
    public class SplashScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private RectTransform logoRect;
        [SerializeField] private Image glowImage;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float audioDuration = 2.5f;
        [SerializeField] private float audioFadeOutTime = 0.4f;

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1.0f;
        [SerializeField] private float holdDuration = 2.2f;
        [SerializeField] private float fadeOutDuration = 0.7f;
        [SerializeField] private float bounceDelay = 0.5f;

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "Title";

        private bool _skipped;

        void Start()
        {
            if (logoCanvasGroup) logoCanvasGroup.alpha = 0f;
            if (logoRect) logoRect.localScale = Vector3.one;
            SetGlowAlpha(0f);
            StartCoroutine(PlaySplash());
        }

        IEnumerator PlaySplash()
        {
            yield return StartCoroutine(FadeIn(fadeInDuration));

            if (audioSource && audioSource.clip)
            {
                audioSource.Play();
                StartCoroutine(FadeOutAudio(audioDuration, audioFadeOutTime));
            }

            yield return new WaitForSeconds(bounceDelay);
            if (!_skipped) yield return StartCoroutine(BounceLogo());

            float remainHold = holdDuration - bounceDelay - 0.4f;
            if (remainHold > 0f && !_skipped)
                yield return new WaitForSeconds(remainHold);

            if (!_skipped) yield return StartCoroutine(FadeOut(fadeOutDuration));

            GoToTitle();
        }

        IEnumerator FadeIn(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !_skipped)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (logoCanvasGroup) logoCanvasGroup.alpha = t;
                SetGlowAlpha(t * 0.22f);
                yield return null;
            }
            if (logoCanvasGroup) logoCanvasGroup.alpha = 1f;
            SetGlowAlpha(0.22f);
        }

        IEnumerator BounceLogo()
        {
            if (logoRect == null) yield break;

            const float growTime = 0.13f;
            const float shrinkTime = 0.27f;
            const float peak = 1.11f;

            float t = 0f;
            while (t < growTime)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, peak, Mathf.SmoothStep(0f, 1f, t / growTime));
                logoRect.localScale = Vector3.one * s;
                SetGlowAlpha(0.22f + (s - 1f) * 2f);
                yield return null;
            }

            t = 0f;
            while (t < shrinkTime)
            {
                t += Time.deltaTime;
                float p = t / shrinkTime;
                float s = 1f + (peak - 1f) * Mathf.Cos(p * Mathf.PI * 1.6f) * (1f - p);
                logoRect.localScale = Vector3.one * Mathf.Max(0.96f, s);
                SetGlowAlpha(Mathf.Lerp(0.35f, 0.22f, p));
                yield return null;
            }

            logoRect.localScale = Vector3.one;
            SetGlowAlpha(0.18f);
        }

        IEnumerator FadeOut(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !_skipped)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (logoCanvasGroup) logoCanvasGroup.alpha = 1f - t;
                SetGlowAlpha(0.18f * (1f - t));
                yield return null;
            }
            if (logoCanvasGroup) logoCanvasGroup.alpha = 0f;
            SetGlowAlpha(0f);
        }

        IEnumerator FadeOutAudio(float startDelay, float fadeTime)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, startDelay - fadeTime));
            float start = audioSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeTime && audioSource)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(start, 0f, elapsed / fadeTime);
                yield return null;
            }
            if (audioSource) audioSource.Stop();
        }

        void SetGlowAlpha(float a)
        {
            if (!glowImage) return;
            var c = glowImage.color;
            c.a = a;
            glowImage.color = c;
        }

        void Skip()
        {
            if (_skipped) return;
            _skipped = true;
            StopAllCoroutines();
            if (audioSource) audioSource.Stop();
            GoToTitle();
        }

        void GoToTitle() => SceneManager.LoadScene(titleSceneName);

        void Update()
        {
            if (_skipped) return;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) Skip();
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) Skip();
        }
    }
}
