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

        [Header("ぺインレモキャラ＋爆発")]
        [SerializeField] RectTransform painlemoImage;
        [SerializeField] RectTransform explosionSmall;
        [SerializeField] RectTransform explosionLarge;

        [Header("レモンシャワー（差が０の時）")]
        [SerializeField] RectTransform lemonShowerParent;
        [SerializeField] Texture2D lemonTex;

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
            if (lemonShowerParent) lemonShowerParent.gameObject.SetActive(false);

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

            // 1. 予想数字（左）スライドイン
            if (guessedNumberLabel)
                guessedNumberLabel.text = SinglePlayConfig.GuessedNumber.ToString();
            yield return StartCoroutine(SlideIn(guessedGroup, -120f));

            // 2. 2秒後に秘密数字（右）スライドイン
            yield return new WaitForSeconds(2f);
            if (secretNumberLabel)
                secretNumberLabel.text = SinglePlayConfig.SecretNumber.ToString();
            yield return StartCoroutine(SlideIn(secretGroup, 120f));

            // 3. 1秒後に差・結果
            yield return new WaitForSeconds(1f);

            int diff = Mathf.Abs(SinglePlayConfig.GuessedNumber - SinglePlayConfig.SecretNumber);
            bool en  = LanguageSettings.IsEnglish;

            if (diff == 0)
            {
                // ピッタリ！ライフ増加
                int gain = SinglePlayConfig.PlayerCount;
                if (diffLabel)
                    diffLabel.text = en ? "Perfect match!!" : "ピッタリ！！";
                if (lifeChangeLabel)
                    lifeChangeLabel.text = en ? $"+{gain} life points!!" : $"ライフ +{gain}!!";

                yield return StartCoroutine(FadeGroup(diffGroup, 0f, 1f, 0.30f));

                // レモンシャワー + ライフ増加
                StartCoroutine(LemonShower());
                yield return StartCoroutine(AddLife(gain));
            }
            else
            {
                if (diffLabel)
                    diffLabel.text = en ? $"Difference: {diff}" : $"差: {diff}";
                if (lifeChangeLabel)
                    lifeChangeLabel.text = en ? $"−{diff} life points" : $"ライフ −{diff}";

                yield return StartCoroutine(FadeGroup(diffGroup, 0f, 1f, 0.30f));

                yield return StartCoroutine(ExplodeAndReduceLife(diff));
            }

            if (nextButton) nextButton.gameObject.SetActive(true);
        }

        // ── ライフ増加（ピッタリ）──────────────────────────────────────

        IEnumerator AddLife(int gain)
        {
            int startLife  = SinglePlayConfig.CurrentLife;
            int targetLife = startLife + gain;
            float dur      = Mathf.Max(0.8f, gain * 0.10f);
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

        // ── レモンシャワー ────────────────────────────────────────────

        IEnumerator LemonShower()
        {
            if (lemonShowerParent == null || lemonTex == null) yield break;
            lemonShowerParent.gameObject.SetActive(true);

            const int count = 24;
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(SpawnShowerLemon());
                yield return new WaitForSeconds(0.06f);
            }

            yield return new WaitForSeconds(2.5f);

            // 子オブジェクトを全削除
            for (int i = lemonShowerParent.childCount - 1; i >= 0; i--)
                Object.Destroy(lemonShowerParent.GetChild(i).gameObject);

            lemonShowerParent.gameObject.SetActive(false);
        }

        IEnumerator SpawnShowerLemon()
        {
            var go = new GameObject("ShowerLemon", typeof(RectTransform));
            go.transform.SetParent(lemonShowerParent, false);

            var raw = go.AddComponent<RawImage>();
            raw.texture = lemonTex;
            raw.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            float size = Random.Range(80f, 180f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);

            float startX = Random.Range(-520f, 520f);
            float startY = -1000f;
            rt.anchoredPosition = new Vector2(startX, startY);

            float speed = Random.Range(900f, 1600f);
            float drift = Random.Range(-150f, 150f);
            float startRot = Random.Range(-360f, 360f);
            float spinSpeed = Random.Range(-200f, 200f);
            float dur = 2.0f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / dur;
                rt.anchoredPosition = new Vector2(startX + drift * p, startY + speed * p);
                rt.localRotation = Quaternion.Euler(0f, 0f, startRot + spinSpeed * elapsed);
                float alpha = p > 0.65f ? Mathf.Lerp(1f, 0f, (p - 0.65f) / 0.35f) : 1f;
                raw.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            if (go != null) Object.Destroy(go);
        }

        // ── 爆発 + ライフ減少 ─────────────────────────────────────────

        IEnumerator ExplodeAndReduceLife(int diff)
        {
            // painlemoをシェイク
            if (painlemoImage != null)
                yield return StartCoroutine(Shake(painlemoImage, 0.35f, 28f));

            // 爆発
            bool big      = diff >= 5;
            var explosion = big ? explosionLarge : explosionSmall;

            if (explosion != null)
            {
                explosion.gameObject.SetActive(true);
                yield return StartCoroutine(AnimateExplosion(explosion, big));
                explosion.gameObject.SetActive(false);
            }

            // ライフ減少
            int startLife  = SinglePlayConfig.CurrentLife;
            int targetLife = Mathf.Max(0, startLife - diff);
            float dur      = Mathf.Max(0.5f, diff * 0.12f);
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

        IEnumerator Shake(RectTransform rt, float dur, float magnitude)
        {
            Vector2 origin = rt.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t      = elapsed / dur;
                float decay  = 1f - t;
                rt.anchoredPosition = origin + new Vector2(
                    Mathf.Sin(t * Mathf.PI * 16f) * magnitude * decay,
                    Mathf.Cos(t * Mathf.PI * 10f) * magnitude * 0.4f * decay);
                yield return null;
            }

            rt.anchoredPosition = origin;
        }

        IEnumerator AnimateExplosion(RectTransform explosion, bool big)
        {
            float dur      = big ? 0.75f : 0.50f;
            float peakScale = big ? 2.4f : 1.7f;
            var img = explosion.GetComponent<Image>();
            float elapsed = 0f;

            explosion.localScale = Vector3.zero;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t     = elapsed / dur;
                float scale = Mathf.Sin(t * Mathf.PI) * peakScale;
                explosion.localScale = Vector3.one * Mathf.Max(0f, scale);
                if (img) img.color = new Color(1f, 1f, 1f, 1f - Mathf.Pow(t, 1.5f));
                yield return null;
            }

            explosion.localScale = Vector3.zero;
            if (img) img.color = Color.white;
        }

        // ── ユーティリティ ────────────────────────────────────────────

        IEnumerator SlideIn(CanvasGroup cg, float xFrom)
        {
            if (cg == null) yield break;
            var rt = cg.GetComponent<RectTransform>();
            if (rt == null) { cg.alpha = 1f; yield break; }

            Vector2 origin = rt.anchoredPosition;
            float dur = 0.38f, elapsed = 0f;
            cg.alpha = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                rt.anchoredPosition = new Vector2(Mathf.Lerp(origin.x + xFrom, origin.x, t), origin.y);
                cg.alpha = t;
                yield return null;
            }

            rt.anchoredPosition = origin;
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
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
