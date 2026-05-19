using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.Title
{
    /// <summary>
    /// タイトル画面のUI操作とシーン遷移を管理する
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject modeSelectPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button localModeButton;
        [SerializeField] private Button onlineModeButton;
        [SerializeField] private Button backButton;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.6f;
        [SerializeField] private RectTransform titleLogoRect;
        [SerializeField] private float logoBounceMagnitude = 12f;
        [SerializeField] private float logoBounceSpeed = 1.2f;

        [Header("Fade")]
        [SerializeField] private CanvasGroup screenFade;

        [Header("BGM")]
        [SerializeField] private AudioSource bgmSource;

        [Header("Scenes")]
        [SerializeField] private string playerSetupSceneName = "PlayerSetup";

        Vector3 _logoBasePosition;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (titleGroup) titleGroup.alpha = 0f;
            if (modeSelectPanel) modeSelectPanel.SetActive(false);
            if (mainPanel) mainPanel.SetActive(true);

            if (titleLogoRect) _logoBasePosition = titleLogoRect.anchoredPosition3D;

            playButton?.onClick.AddListener(OnPlayButton);
            creditsButton?.onClick.AddListener(OnCreditsButton);
            localModeButton?.onClick.AddListener(OnLocalMode);
            onlineModeButton?.onClick.AddListener(OnOnlineMode);
            backButton?.onClick.AddListener(OnBack);

            if (bgmSource) bgmSource.Play();
            StartCoroutine(FadeIn());
        }

        void Update()
        {
            if (titleLogoRect == null) return;
            float y = Mathf.Sin(Time.time * logoBounceSpeed) * logoBounceMagnitude;
            titleLogoRect.anchoredPosition3D = _logoBasePosition + new Vector3(0f, y, 0f);
        }

        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
                if (titleGroup) titleGroup.alpha = t;
                if (screenFade) screenFade.alpha = 1f - t;
                yield return null;
            }
            if (titleGroup) titleGroup.alpha = 1f;
            if (screenFade) { screenFade.alpha = 0f; screenFade.blocksRaycasts = false; }
        }

        void OnPlayButton()
        {
            StartCoroutine(LoadSceneWithFade(playerSetupSceneName));
        }

        IEnumerator LoadSceneWithFade(string sceneName)
        {
            if (screenFade != null)
            {
                screenFade.blocksRaycasts = true;
                float dur = 0.30f, elapsed = 0f;
                while (elapsed < dur)
                {
                    elapsed += Time.deltaTime;
                    screenFade.alpha = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                    yield return null;
                }
                screenFade.alpha = 1f;
            }
            SceneManager.LoadScene(sceneName);
        }

        void OnLocalMode()
        {
            SceneManager.LoadScene(playerSetupSceneName);
        }

        void OnOnlineMode()
        {
            // TODO: オンラインロビー画面へ遷移
            Debug.Log("[TitleScreen] Online mode: coming soon.");
        }

        void OnCreditsButton()
        {
            // TODO: クレジット画面
            Debug.Log("[TitleScreen] Credits: coming soon.");
        }

        void OnBack()
        {
            if (modeSelectPanel) modeSelectPanel.SetActive(false);
            if (mainPanel) mainPanel.SetActive(true);
        }
    }
}
