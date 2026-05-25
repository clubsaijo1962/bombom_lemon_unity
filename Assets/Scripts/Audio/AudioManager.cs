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
        [SerializeField] AudioClip badClip;
        [SerializeField] AudioClip goodClip;
        [SerializeField] AudioClip lemonGetClip;
        [SerializeField] AudioClip showClip;

        AudioSource _sfxSource;
        AudioSource _fireSfxSource; // 専用ソース（途中停止可能）

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _sfxSource = GetComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _fireSfxSource = gameObject.AddComponent<AudioSource>();
            _fireSfxSource.playOnAwake = false;
        }

        void Start()
        {
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

        // ── 再生メソッド ──────────────────────────────────────────────

        public void PlayClick()     => PlayOneShot(clickClip);
        public void PlayGameClear() => PlayOneShot(gameClearClip);
        public void PlayGameOver()  => PlayOneShot(gameOverClip);
        public void PlayBad()       => PlayOneShot(badClip);
        public void PlayGood()      => PlayOneShot(goodClip);
        public void PlayLemonGet()  => PlayOneShot(lemonGetClip);
        public void PlayShow()      => PlayOneShot(showClip);

        /// <summary>爆発音：音源の最初の1.5秒のみ、音量65%で再生</summary>
        public void PlayFireMusic()
        {
            if (fireMusicClip == null || _fireSfxSource == null) return;
            StopCoroutine(nameof(FireMusicCo));
            StartCoroutine(nameof(FireMusicCo));
        }

        IEnumerator FireMusicCo()
        {
            _fireSfxSource.Stop();
            _fireSfxSource.clip   = fireMusicClip;
            _fireSfxSource.volume = 0.65f;
            _fireSfxSource.Play();
            yield return new WaitForSeconds(1.5f);
            _fireSfxSource.Stop();
        }

        void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip);
        }
    }
}
