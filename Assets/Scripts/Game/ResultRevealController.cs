using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;
using BomBomLemon.Audio;

namespace BomBomLemon.Game
{
    public class ResultRevealController : MonoBehaviour
    {
        [Header("ターン情報")]
        [SerializeField] TextMeshProUGUI roundLabel;
        [SerializeField] TextMeshProUGUI remainingTurnsLabel;

        [Header("予想数字カード（左）")]
        [SerializeField] CanvasGroup guessedGroup;
        [SerializeField] TextMeshProUGUI guessedNumberLabel;

        [Header("秘密数字カード（右）")]
        [SerializeField] CanvasGroup secretGroup;
        [SerializeField] TextMeshProUGUI secretNumberLabel;

        [Header("差")]
        [SerializeField] CanvasGroup diffGroup;
        [SerializeField] TextMeshProUGUI diffLabel;

        [Header("ライフ変化表示（差解決後）")]
        [SerializeField] CanvasGroup lifeTransitionGroup;
        [SerializeField] TextMeshProUGUI lifeTransitionLabel;

        [Header("ヘルプカード使用表示")]
        [SerializeField] CanvasGroup helpUsedGroup;
        [SerializeField] TextMeshProUGUI helpUsedLabel;

        [Header("キャラクター（差と同時に表示）")]
        [SerializeField] CanvasGroup characterGroup;
        [SerializeField] Image painlemoImage;
        [SerializeField] Image lemonImage;

        [Header("爆発（bomb.png）")]
        [SerializeField] RectTransform explosionSmall;
        [SerializeField] RectTransform explosionLarge;

        [Header("レモンシャワー（ピッタリ）")]
        [SerializeField] RectTransform lemonShowerParent;
        [SerializeField] Texture2D lemonTex;

        [Header("ヘルプカードダイアログ")]
        [SerializeField] CanvasGroup helpDialogGroup;
        [SerializeField] TextMeshProUGUI helpDialogBodyLabel;
        [SerializeField] Button useHelpButton;
        [SerializeField] Button dontUseButton;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;

        [Header("ゲームオーバー")]
        [SerializeField] CanvasGroup gameOverGroup;
        [SerializeField] TextMeshProUGUI gameOverDetailLabel;
        [SerializeField] Button gameOverHomeButton;
        [SerializeField] RectTransform gameOverPainlemo;

        [Header("ゲームクリア")]
        [SerializeField] CanvasGroup gameClearGroup;
        [SerializeField] TextMeshProUGUI gameClearDetailLabel;
        [SerializeField] Button gameClearHomeButton;
        [SerializeField] RectTransform gameClearLemon;

        [Header("ボタン")]
        [SerializeField] Button nextButton;
        [SerializeField] Button homeButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        int _diff;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            if (guessedGroup)        { guessedGroup.alpha        = 0f; guessedGroup.blocksRaycasts        = false; }
            if (secretGroup)         { secretGroup.alpha         = 0f; secretGroup.blocksRaycasts         = false; }
            if (diffGroup)           { diffGroup.alpha           = 0f; diffGroup.blocksRaycasts           = false; }
            if (characterGroup)      { characterGroup.alpha      = 0f; characterGroup.blocksRaycasts      = false; }
            if (lifeTransitionGroup) { lifeTransitionGroup.alpha = 0f; lifeTransitionGroup.blocksRaycasts = false; }
            if (helpUsedGroup)       { helpUsedGroup.alpha       = 0f; helpUsedGroup.blocksRaycasts       = false; }
            if (helpDialogGroup)     { helpDialogGroup.alpha     = 0f; helpDialogGroup.blocksRaycasts     = false; }
            if (gameOverGroup)       { gameOverGroup.alpha       = 0f; gameOverGroup.blocksRaycasts       = false; }
            if (gameClearGroup)      { gameClearGroup.alpha      = 0f; gameClearGroup.blocksRaycasts      = false; }

            if (explosionSmall) explosionSmall.gameObject.SetActive(false);
            if (explosionLarge) explosionLarge.gameObject.SetActive(false);
            if (lemonShowerParent) lemonShowerParent.gameObject.SetActive(false);

            UpdateHUD();

            useHelpButton?.onClick.AddListener(OnUseHelp);
            dontUseButton?.onClick.AddListener(OnDontUseHelp);
            nextButton?.onClick.AddListener(OnNext);
            homeButton?.onClick.AddListener(OnHome);
            gameOverHomeButton?.onClick.AddListener(OnHome);
            gameClearHomeButton?.onClick.AddListener(OnHome);

            if (nextButton) nextButton.gameObject.SetActive(false);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
            StartCoroutine(RevealSequence());
        }

        void UpdateHUD()
        {
            if (lifeCountLabel)
                lifeCountLabel.text = $"×{SinglePlayConfig.CurrentLife}";
            if (helpCardCountLabel)
                helpCardCountLabel.text = $"×{SinglePlayConfig.CurrentHelpCards}";
        }

        IEnumerator RevealSequence()
        {
            bool en      = LanguageSettings.IsEnglish;
            int curRound = SinglePlayConfig.CurrentRound;
            int total    = SinglePlayConfig.TotalRounds;
            int rem      = total - curRound;

            if (roundLabel)
                roundLabel.text = en ? $"Challenge {curRound}/{total}" : $"{curRound}/{total}人目のチャレンジ";
            if (remainingTurnsLabel)
                remainingTurnsLabel.text = rem == 0
                    ? (en ? "Final challenge!" : "最終チャレンジ！")
                    : (en ? $"{rem} turns remaining" : $"残り{rem}ターン");

            yield return new WaitForSeconds(0.5f);

            // 1. 予想数字（左からスライドイン）
            if (guessedNumberLabel)
                guessedNumberLabel.text = SinglePlayConfig.GuessedNumber.ToString();
            yield return StartCoroutine(SlideIn(guessedGroup, -120f));

            // 2. 2秒後に秘密数字（右からスライドイン）
            yield return new WaitForSeconds(2f);
            if (secretNumberLabel)
                secretNumberLabel.text = SinglePlayConfig.SecretNumber.ToString();
            yield return StartCoroutine(SlideIn(secretGroup, 120f));

            // 3. 1秒後に差＋キャラクター同時表示
            yield return new WaitForSeconds(1f);

            _diff = Mathf.Abs(SinglePlayConfig.GuessedNumber - SinglePlayConfig.SecretNumber);

            if (_diff == 0)
            {
                if (diffLabel) diffLabel.text = en ? "Perfect match!" : "ピッタリ！";
                if (painlemoImage) painlemoImage.gameObject.SetActive(false);
                if (lemonImage)    lemonImage.gameObject.SetActive(true);
            }
            else
            {
                if (diffLabel) diffLabel.text = $"{_diff}";
                if (painlemoImage) painlemoImage.gameObject.SetActive(true);
                if (lemonImage)    lemonImage.gameObject.SetActive(false);
            }

            StartCoroutine(FadeGroup(diffGroup, 0f, 1f, 0.30f));
            yield return StartCoroutine(FadeGroup(characterGroup, 0f, 1f, 0.30f));
            if (diffGroup)      diffGroup.blocksRaycasts      = true;
            if (characterGroup) characterGroup.blocksRaycasts = true;

            // 差が出てからライフ処理まで1秒待機
            yield return new WaitForSeconds(1f);

            if (_diff == 0)
            {
                int gain     = SinglePlayConfig.PlayerCount;
                int fromLife = SinglePlayConfig.CurrentLife;
                int toLife   = fromLife + gain;

                // ライフ変化表示（X→Y形式）
                if (lifeTransitionLabel) lifeTransitionLabel.text = $"{fromLife}→{toLife}";
                yield return StartCoroutine(FadeGroup(lifeTransitionGroup, 0f, 1f, 0.25f));
                if (lifeTransitionGroup) lifeTransitionGroup.blocksRaycasts = true;

                StartCoroutine(LemonShower());
                yield return StartCoroutine(ChangeLifeAnimated(fromLife, toLife));
                SinglePlayConfig.CurrentLife = toLife;
                UpdateHUD();
            }
            else
            {
                if (characterGroup != null)
                    yield return StartCoroutine(Shake(characterGroup.GetComponent<RectTransform>(), 0.35f, 26f));

                int effectiveDamage = _diff;
                bool usedHelp = false;

                // ヘルプカードダイアログ（diff>=5かつ残枚数あり、かつ最終ラウンドでない）
                if (_diff >= 5 && SinglePlayConfig.CurrentHelpCards > 0 && curRound < total)
                {
                    _usedHelp = false;
                    yield return StartCoroutine(ShowHelpDialog());
                    usedHelp = _usedHelp;
                    effectiveDamage = usedHelp ? 4 : _diff;
                    // 選択後1秒待機してからライフ処理
                    yield return new WaitForSeconds(1f);
                }

                // ライフ変化表示（ダイアログ解決後、爆発前に出す）
                int fromLife = SinglePlayConfig.CurrentLife;
                int toLife   = Mathf.Max(0, fromLife - effectiveDamage);
                if (lifeTransitionLabel) lifeTransitionLabel.text = $"{fromLife}→{toLife}";
                yield return StartCoroutine(FadeGroup(lifeTransitionGroup, 0f, 1f, 0.25f));
                if (lifeTransitionGroup) lifeTransitionGroup.blocksRaycasts = true;

                // ヘルプカード使用済み表示
                if (usedHelp)
                {
                    if (helpUsedLabel)
                        helpUsedLabel.text = en ? "Help card used  −1" : "ヘルプカード使用 −1";
                    yield return StartCoroutine(FadeGroup(helpUsedGroup, 0f, 1f, 0.20f));
                    if (helpUsedGroup) helpUsedGroup.blocksRaycasts = true;
                }

                yield return StartCoroutine(ExplodeAndReduceLife(effectiveDamage));

                if (SinglePlayConfig.CurrentLife <= 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(ShowGameOver(curRound, total));
                    yield break;
                }
            }

            // ゲームクリアチェック（最終ラウンド終了かつライフ残り）
            if (curRound >= total)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ShowGameClear(total));
                yield break;
            }

            if (nextButton) nextButton.gameObject.SetActive(true);
        }

        // ── ヘルプカードダイアログ ───────────────────────────────────────

        bool _helpDialogAnswered;
        bool _usedHelp;

        IEnumerator ShowHelpDialog()
        {
            bool en = LanguageSettings.IsEnglish;
            if (helpDialogBodyLabel)
                helpDialogBodyLabel.text = en
                    ? $"Use a help card to\nfix the minus at −4\n(instead of −{_diff})?"
                    : $"ヘルプカードを使って\nマイナスを4にしますか？\n（本来 −{_diff}）";

            _helpDialogAnswered = false;
            yield return StartCoroutine(FadeGroup(helpDialogGroup, 0f, 1f, 0.25f));
            helpDialogGroup.blocksRaycasts = true;

            yield return new WaitUntil(() => _helpDialogAnswered);

            yield return StartCoroutine(FadeGroup(helpDialogGroup, 1f, 0f, 0.20f));
            helpDialogGroup.blocksRaycasts = false;

            if (_usedHelp)
            {
                SinglePlayConfig.CurrentHelpCards--;
                UpdateHUD();
            }
        }

        void OnUseHelp()     { _usedHelp = true;  _helpDialogAnswered = true; }
        void OnDontUseHelp() { _usedHelp = false; _helpDialogAnswered = true; }

        // ── 爆発 + ライフ減少 ─────────────────────────────────────────

        IEnumerator ExplodeAndReduceLife(int amount)
        {
            AudioManager.Instance?.PlayFireMusic();
            bool big      = amount >= 5;
            var explosion = big ? explosionLarge : explosionSmall;

            if (explosion != null)
            {
                explosion.gameObject.SetActive(true);
                yield return StartCoroutine(AnimateExplosion(explosion, big));
                explosion.gameObject.SetActive(false);
            }

            int target = Mathf.Max(0, SinglePlayConfig.CurrentLife - amount);
            yield return StartCoroutine(ChangeLifeAnimated(SinglePlayConfig.CurrentLife, target));
            SinglePlayConfig.CurrentLife = target;
            UpdateHUD();
        }

        IEnumerator ChangeLifeAnimated(int from, int to)
        {
            float dur = Mathf.Max(0.5f, Mathf.Abs(to - from) * 0.10f);
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                int cur = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / dur));
                if (lifeCountLabel) lifeCountLabel.text = $"×{cur}";
                yield return null;
            }
            if (lifeCountLabel) lifeCountLabel.text = $"×{to}";
        }

        IEnumerator AnimateExplosion(RectTransform explosion, bool big)
        {
            var img = explosion.GetComponent<Image>();
            explosion.localScale = Vector3.zero;

            if (!big)
            {
                float dur = 0.50f, elapsed = 0f;
                while (elapsed < dur)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / dur;
                    explosion.localScale = Vector3.one * Mathf.Max(0f, Mathf.Sin(t * Mathf.PI) * 1.7f);
                    if (img) img.color = new Color(1f, 1f, 1f, 1f - Mathf.Pow(t, 1.5f));
                    yield return null;
                }
            }
            else
            {
                // 第1波：超大爆発
                StartCoroutine(ScreenFlash(new Color(1f, 0.65f, 0.10f, 0.85f), 0.40f));
                float e = 0f;
                while (e < 0.55f)
                {
                    e += Time.deltaTime;
                    float t = e / 0.55f;
                    explosion.localScale = Vector3.one * Mathf.Max(0f, Mathf.Sin(t * Mathf.PI) * 4.2f);
                    if (img) img.color = new Color(1f, Mathf.Lerp(1f, 0.5f, t), 0f, 1f - Mathf.Pow(t, 2.5f) * 0.5f);
                    yield return null;
                }

                // 画面シェイク
                if (panelGroup != null)
                    StartCoroutine(Shake(panelGroup.GetComponent<RectTransform>(), 0.55f, 42f));

                // 第2波：余韻
                StartCoroutine(ScreenFlash(new Color(1f, 0.90f, 0.30f, 0.50f), 0.30f));
                e = 0f;
                while (e < 0.50f)
                {
                    e += Time.deltaTime;
                    float t = e / 0.50f;
                    explosion.localScale = Vector3.one * Mathf.Max(0f, Mathf.Sin(t * Mathf.PI) * 2.8f);
                    if (img) img.color = new Color(1f, 0.80f, 0.30f, 0.75f - Mathf.Pow(t, 1.5f) * 0.75f);
                    yield return null;
                }
            }

            explosion.localScale = Vector3.zero;
            if (img) img.color = Color.white;
        }

        IEnumerator ScreenFlash(Color color, float dur)
        {
            Transform root = transform;
            while (root.parent != null) root = root.parent;
            var go = new GameObject("Flash", typeof(RectTransform));
            go.transform.SetParent(root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var flashImg = go.AddComponent<Image>();
            flashImg.color = color; flashImg.raycastTarget = false;
            var cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            float t = 0f;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(1f, 0f, t / dur); yield return null; }
            if (go) Object.Destroy(go);
        }

        // ── ゲームオーバー ────────────────────────────────────────────

        IEnumerator ShowGameOver(int curRound, int total)
        {
            AudioManager.Instance?.PlayGameOver();
            bool en       = LanguageSettings.IsEnglish;
            int remaining = total - curRound;
            if (gameOverDetailLabel)
            {
                gameOverDetailLabel.text = remaining == 0
                    ? (en ? "Life ran out on the final challenge!" : "最終チャレンジでライフが尽きました！")
                    : (en ? $"You were {remaining} challenge{(remaining == 1 ? "" : "s")} away from clearing!"
                           : $"あと{remaining}人でクリアでした！");
            }
            yield return StartCoroutine(FadeGroup(gameOverGroup, 0f, 1f, 0.40f));
            if (gameOverGroup) gameOverGroup.blocksRaycasts = true;
            StartCoroutine(TremblingLoop(gameOverPainlemo));
        }

        IEnumerator TremblingLoop(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 origin = rt.anchoredPosition;
            while (true)
            {
                float t = Time.time;
                rt.anchoredPosition = origin + new Vector2(
                    Mathf.Sin(t * 24f) * 8f,
                    Mathf.Cos(t * 17f) * 5f);
                yield return null;
            }
        }

        IEnumerator BounceLoop(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 origin = rt.anchoredPosition;
            float phase = 0f;
            while (true)
            {
                phase += Time.deltaTime * 2.2f;
                float bounce  = Mathf.Abs(Mathf.Sin(phase)) * 28f;
                float squeeze = 1f + Mathf.Abs(Mathf.Sin(phase)) * 0.09f;
                rt.anchoredPosition = new Vector2(origin.x, origin.y + bounce);
                rt.localScale = new Vector3(1f / squeeze, squeeze, 1f);
                yield return null;
            }
        }

        // ── ゲームクリア ──────────────────────────────────────────────

        IEnumerator ShowGameClear(int total)
        {
            AudioManager.Instance?.PlayGameClear();
            bool en = LanguageSettings.IsEnglish;
            if (gameClearDetailLabel)
                gameClearDetailLabel.text = en
                    ? $"All {total} challenges cleared!\nRemaining life: {SinglePlayConfig.CurrentLife}"
                    : $"全{total}人のチャレンジクリア！\n残りライフ: {SinglePlayConfig.CurrentLife}";
            yield return StartCoroutine(FadeGroup(gameClearGroup, 0f, 1f, 0.40f));
            if (gameClearGroup) gameClearGroup.blocksRaycasts = true;

            StartCoroutine(BounceLoop(gameClearLemon));
            StartCoroutine(GameClearLemonShower());
            StartCoroutine(SpawnSparkles());
        }

        IEnumerator GameClearLemonShower()
        {
            if (lemonShowerParent == null || lemonTex == null) yield break;
            lemonShowerParent.gameObject.SetActive(true);

            float totalDur = 6.0f, elapsed = 0f;
            while (elapsed < totalDur)
            {
                StartCoroutine(SpawnShowerLemon());
                float wait = Random.Range(0.04f, 0.09f);
                yield return new WaitForSeconds(wait);
                elapsed += wait;
            }

            yield return new WaitForSeconds(2.0f);
            for (int i = lemonShowerParent.childCount - 1; i >= 0; i--)
                Object.Destroy(lemonShowerParent.GetChild(i).gameObject);
            lemonShowerParent.gameObject.SetActive(false);
        }

        IEnumerator SpawnSparkles()
        {
            Transform root = transform;
            while (root.parent != null) root = root.parent;

            float totalDur = 7.0f, elapsed = 0f;
            while (elapsed < totalDur)
            {
                StartCoroutine(SpawnSparkle(root));
                float wait = Random.Range(0.03f, 0.08f);
                yield return new WaitForSeconds(wait);
                elapsed += wait;
            }
        }

        IEnumerator SpawnSparkle(Transform parent)
        {
            var go = new GameObject("Sp", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            // Spawn only in margins outside the 920×720 game clear card
            float zone = Random.value;
            float spX, spY;
            if (zone < 0.45f)      { spX = Random.Range(-520f, 520f); spY = Random.Range(390f, 850f); }
            else if (zone < 0.90f) { spX = Random.Range(-520f, 520f); spY = Random.Range(-850f, -390f); }
            else if (zone < 0.95f) { spX = Random.Range(-540f, -480f); spY = Random.Range(-370f, 370f); }
            else                   { spX = Random.Range( 480f,  540f); spY = Random.Range(-370f, 370f); }
            rt.anchoredPosition = new Vector2(spX, spY);

            // 4アームの星型スパークル
            float sz = Random.Range(14f, 48f);
            float wid = sz * 0.16f;
            Color[] palette = {
                new(1f, 0.95f, 0.05f, 1f),
                new(1f, 0.80f, 0.20f, 1f),
                new(1f, 1.00f, 0.80f, 1f),
                new(1f, 1.00f, 1.00f, 1f),
            };
            Color col = palette[Random.Range(0, palette.Length)];

            for (int i = 0; i < 4; i++)
            {
                var arm = new GameObject($"a{i}", typeof(RectTransform));
                arm.transform.SetParent(go.transform, false);
                var ar = arm.GetComponent<RectTransform>();
                ar.anchorMin = ar.anchorMax = new Vector2(0.5f, 0.5f);
                ar.pivot = new Vector2(0.5f, 0.5f);
                ar.sizeDelta = new Vector2(wid, sz);
                ar.anchoredPosition = Vector2.zero;
                ar.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
                var armImg = arm.AddComponent<Image>();
                armImg.color = col; armImg.raycastTarget = false;
            }

            float dur = Random.Range(0.22f, 0.52f), t = 0f;
            float spin = Random.Range(-180f, 180f);
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                float scale = Mathf.Sin(p * Mathf.PI) * 1.3f;
                rt.localScale = Vector3.one * Mathf.Max(0f, scale);
                rt.Rotate(0f, 0f, spin * Time.deltaTime);
                yield return null;
            }
            if (go) Object.Destroy(go);
        }

        // ── レモンシャワー ────────────────────────────────────────────

        IEnumerator LemonShower()
        {
            if (lemonShowerParent == null || lemonTex == null) yield break;
            lemonShowerParent.gameObject.SetActive(true);

            const int count = 26;
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(SpawnShowerLemon());
                yield return new WaitForSeconds(0.055f);
            }

            yield return new WaitForSeconds(2.5f);

            for (int i = lemonShowerParent.childCount - 1; i >= 0; i--)
                Object.Destroy(lemonShowerParent.GetChild(i).gameObject);
            lemonShowerParent.gameObject.SetActive(false);
        }

        IEnumerator SpawnShowerLemon()
        {
            var go  = new GameObject("SL", typeof(RectTransform));
            go.transform.SetParent(lemonShowerParent, false);
            var raw = go.AddComponent<RawImage>();
            raw.texture = lemonTex; raw.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            float sz = Random.Range(80f, 180f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(sz, sz);

            float sx    = Random.Range(-520f, 520f);
            float speed = Random.Range(900f, 1700f);
            float drift = Random.Range(-160f, 160f);
            float rot0  = Random.Range(-360f, 360f);
            float spin  = Random.Range(-220f, 220f);
            float dur   = 2.0f;
            float t     = 0f;
            rt.anchoredPosition = new Vector2(sx, -980f);

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                rt.anchoredPosition = new Vector2(sx + drift * p, -980f + speed * p);
                rt.localRotation    = Quaternion.Euler(0f, 0f, rot0 + spin * t);
                float a = p > 0.65f ? Mathf.Lerp(1f, 0f, (p - 0.65f) / 0.35f) : 1f;
                raw.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            if (go) Object.Destroy(go);
        }

        // ── ユーティリティ ────────────────────────────────────────────

        IEnumerator SlideIn(CanvasGroup cg, float xOffset)
        {
            if (cg == null) yield break;
            var rt = cg.GetComponent<RectTransform>();
            if (rt == null) { cg.alpha = 1f; yield break; }
            Vector2 origin  = rt.anchoredPosition;
            float dur = 0.38f, elapsed = 0f;
            cg.alpha = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                rt.anchoredPosition = new Vector2(Mathf.Lerp(origin.x + xOffset, origin.x, t), origin.y);
                cg.alpha = t;
                yield return null;
            }
            rt.anchoredPosition = origin;
            cg.alpha = 1f; cg.blocksRaycasts = true;
        }

        IEnumerator Shake(RectTransform rt, float dur, float magnitude)
        {
            if (rt == null) yield break;
            Vector2 origin = rt.anchoredPosition;
            float elapsed  = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                rt.anchoredPosition = origin + new Vector2(
                    Mathf.Sin(t * Mathf.PI * 18f) * magnitude * (1f - t),
                    Mathf.Cos(t * Mathf.PI * 11f) * magnitude * 0.35f * (1f - t));
                yield return null;
            }
            rt.anchoredPosition = origin;
        }

        IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
        {
            if (cg == null) yield break;
            float t = 0f; cg.alpha = from;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            cg.alpha = to;
        }

        void OnNext()
        {
            SinglePlayConfig.CurrentRound++;
            bool isFinalRound = SinglePlayConfig.CurrentRound == SinglePlayConfig.TotalRounds;
            string nextScene = (isFinalRound && SinglePlayConfig.CurrentHelpCards > 0) ? "HelpConvert" : "Game";
            StartCoroutine(LoadWithFade(nextScene));
        }
        void OnHome()  => StartCoroutine(LoadWithFade("SingleSettings"));

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
