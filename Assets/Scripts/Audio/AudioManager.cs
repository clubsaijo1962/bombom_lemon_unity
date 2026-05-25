using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BomBomLemon.Audio
{
    /// <summary>
    /// SE管理シングルトン。DontDestroyOnLoad でシーン跨ぎで常駐。
    /// シーンロード時に全Buttonへクリック音を自動バインド。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] AudioClip clickClip;
        [SerializeField] AudioClip fireMusicClip;
        [SerializeField] AudioClip gameClearClip;
        [SerializeField] AudioClip gameOverClip;

        AudioSource _sfxSource;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _sfxSource = GetComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
        }

        void Start()
        {
            // 初期シーンのボタンをバインド
            StartCoroutine(BindButtonsNextFrame());
        }

        void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(BindButtonsNextFrame());
        }

        IEnumerator BindButtonsNextFrame()
        {
            yield return null;
            foreach (var btn in FindObjectsOfType<Button>(true))
                btn.onClick.AddListener(PlayClick);
        }

        public void PlayClick()     => PlayOneShot(clickClip);
        public void PlayFireMusic() => PlayOneShot(fireMusicClip);
        public void PlayGameClear() => PlayOneShot(gameClearClip);
        public void PlayGameOver()  => PlayOneShot(gameOverClip);

        void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip);
        }
    }
}
