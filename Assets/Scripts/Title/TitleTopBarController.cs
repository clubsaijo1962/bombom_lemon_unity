using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BomBomLemon.Title
{
    public class TitleTopBarController : MonoBehaviour
    {
        [SerializeField] private Button               rulesButton;
        [SerializeField] private Button               topicsButton;
        [SerializeField] private Button               hellModeButton;
        [SerializeField] private Button               languageButton;
        [SerializeField] private RulesPanel            rulesPanel;
        [SerializeField] private TopicCustomizePanel   topicCustomizePanel;
        [SerializeField] private Image           hellButtonBg;
        [SerializeField] private TextMeshProUGUI hellLabelTmp;
        [SerializeField] private TextMeshProUGUI languageBtnLabel;
        [SerializeField] private TextMeshProUGUI rulesBtnLabel;
        [SerializeField] private TextMeshProUGUI topicsBtnLabel;
        [SerializeField] private Image           backgroundImage;
        [SerializeField] private LemonRainEffect lemonRain;
        [SerializeField] private RawImage        startButtonImage;
        [SerializeField] private RawImage        titleLemonImage;
        [SerializeField] private TextMeshProUGUI subtitleJP;
        [SerializeField] private TextMeshProUGUI subtitleEN;
        [SerializeField] private TextMeshProUGUI hellDescJPTmp;
        [SerializeField] private TextMeshProUGUI hellDescENTmp;
        [SerializeField] private CanvasGroup      hellDescGroup;
        [SerializeField] private Texture2D       lemonTexture;
        [SerializeField] private Texture2D       limeTexture;
        [SerializeField] private Texture2D       startNormalTexture;
        [SerializeField] private Texture2D       startLimeTexture;

        public bool IsHellMode { get; private set; } = false;

        static readonly Color BtnLemon    = new Color(0.98f, 0.90f, 0.42f, 0.85f);
        static readonly Color BgNormal    = new Color(0.98f, 0.90f, 0.55f, 1f);
        static readonly Color SubJPNormal = new Color(0.38f, 0.18f, 0.04f, 0.92f);
        static readonly Color SubENNormal = new Color(0.48f, 0.28f, 0.10f, 0.85f);

        static readonly Color BtnLime   = new Color(0.38f, 0.70f, 0.25f, 0.90f);
        static readonly Color BgHell    = new Color(0.52f, 0.76f, 0.32f, 1f);
        static readonly Color SubJPHell = new Color(0.20f, 0.55f, 0.22f, 1f);
        static readonly Color SubENHell = new Color(0.28f, 0.50f, 0.22f, 0.85f);

        void Start()
        {
            rulesButton?.onClick.AddListener(OnRules);
            topicsButton?.onClick.AddListener(OnTopics);
            hellModeButton?.onClick.AddListener(OnHellModeToggle);
            languageButton?.onClick.AddListener(OnLanguageToggle);
            ApplyInstant();
        }

        void OnRules()  => rulesPanel?.Show();
        void OnTopics() => topicCustomizePanel?.Show();

        void OnLanguageToggle()
        {
            LanguageSettings.Toggle();
            UpdateLanguage();
            StartCoroutine(ScalePunch(languageButton?.transform));
        }

        void OnHellModeToggle()
        {
            IsHellMode = !IsHellMode;

            if (lemonRain)       lemonRain.SetTexture(IsHellMode ? limeTexture : lemonTexture);
            if (titleLemonImage) titleLemonImage.texture = IsHellMode ? limeTexture : lemonTexture;
            if (startButtonImage && startNormalTexture && startLimeTexture)
                startButtonImage.texture = IsHellMode ? startLimeTexture : startNormalTexture;

            UpdateHellLabel();

            StopAllCoroutines();
            StartCoroutine(AnimateColors());
            StartCoroutine(AnimateHellDesc());
            StartCoroutine(ScalePunch(hellButtonBg?.transform));
        }

        void ApplyInstant()
        {
            UpdateHellLabel();
            UpdateLanguage();
            if (hellButtonBg)    hellButtonBg.color    = BtnLemon;
            if (backgroundImage) backgroundImage.color = BgNormal;
            if (subtitleJP)      subtitleJP.color      = SubJPNormal;
            if (subtitleEN)      subtitleEN.color      = SubENNormal;
        }

        void UpdateHellLabel()
        {
            if (!hellLabelTmp) return;
            bool en = LanguageSettings.IsEnglish;
            hellLabelTmp.text = IsHellMode
                ? (en ? "Hell Mode ON"  : "地獄モード ON")
                : (en ? "Hell Mode OFF" : "地獄モード OFF");
        }

        void UpdateLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (languageBtnLabel) languageBtnLabel.text = en ? "English On"  : "English Off";
            if (rulesBtnLabel)    rulesBtnLabel.text    = en ? "Rules"       : "ルール";
            if (topicsBtnLabel)   topicsBtnLabel.text   = en ? "Topics"      : "お題";
            UpdateHellLabel();
            if (subtitleJP)    subtitleJP.gameObject.SetActive(!en);
            if (subtitleEN)    subtitleEN.gameObject.SetActive(en);
            if (hellDescJPTmp) hellDescJPTmp.gameObject.SetActive(!en);
            if (hellDescENTmp) hellDescENTmp.gameObject.SetActive(en);
        }

        IEnumerator AnimateColors()
        {
            Color fromBg    = backgroundImage ? backgroundImage.color : BgNormal;
            Color fromBtn   = hellButtonBg    ? hellButtonBg.color    : BtnLemon;
            Color fromSubJP = subtitleJP      ? subtitleJP.color      : SubJPNormal;
            Color fromSubEN = subtitleEN      ? subtitleEN.color      : SubENNormal;

            Color toBg    = IsHellMode ? BgHell    : BgNormal;
            Color toBtn   = IsHellMode ? BtnLime   : BtnLemon;
            Color toSubJP = IsHellMode ? SubJPHell : SubJPNormal;
            Color toSubEN = IsHellMode ? SubENHell : SubENNormal;

            float duration = 0.35f, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (backgroundImage) backgroundImage.color = Color.Lerp(fromBg,    toBg,    t);
                if (hellButtonBg)    hellButtonBg.color    = Color.Lerp(fromBtn,   toBtn,   t);
                if (subtitleJP)      subtitleJP.color      = Color.Lerp(fromSubJP, toSubJP, t);
                if (subtitleEN)      subtitleEN.color      = Color.Lerp(fromSubEN, toSubEN, t);
                yield return null;
            }
            if (backgroundImage) backgroundImage.color = toBg;
            if (hellButtonBg)    hellButtonBg.color    = toBtn;
            if (subtitleJP)      subtitleJP.color      = toSubJP;
            if (subtitleEN)      subtitleEN.color      = toSubEN;
        }

        IEnumerator AnimateHellDesc()
        {
            if (hellDescGroup == null) yield break;
            float target = IsHellMode ? 1f : 0f;
            float from   = hellDescGroup.alpha;
            float duration = 0.40f, elapsed = 0f;
            hellDescGroup.blocksRaycasts = false;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                hellDescGroup.alpha = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                yield return null;
            }
            hellDescGroup.alpha = target;
        }

        IEnumerator ScalePunch(Transform t)
        {
            if (t == null) yield break;
            Vector3 original = t.localScale;
            float duration = 0.18f, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s = 1f + 0.10f * Mathf.Sin(Mathf.PI * elapsed / duration);
                t.localScale = original * s;
                yield return null;
            }
            t.localScale = original;
        }
    }
}
