using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BomBomLemon.Splash
{
    /// <summary>
    /// Splash screen: フェードイン → ロゴ表示 → フェードアウト → タイトルシーンへ遷移
    /// </summary>
    public class SplashScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private Image backgroundImage;

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1.0f;
        [SerializeField] private float holdDuration = 1.8f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = Color.black;

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "Title";

        void Start()
        {
            if (backgroundImage) backgroundImage.color = backgroundColor;
            if (logoCanvasGroup) logoCanvasGroup.alpha = 0f;
            StartCoroutine(PlaySplash());
        }

        IEnumerator PlaySplash()
        {
            // フェードイン
            yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

            // ロゴ表示維持
            yield return new WaitForSeconds(holdDuration);

            // フェードアウト
            yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

            // タイトルシーンへ
            SceneManager.LoadScene(titleSceneName);
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (logoCanvasGroup) logoCanvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            if (logoCanvasGroup) logoCanvasGroup.alpha = to;
        }

        // タップ/クリックでスキップ
        void Update()
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                StopAllCoroutines();
                SceneManager.LoadScene(titleSceneName);
            }
        }
    }
}
