using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BomBomLemon.Title
{
    public class RulesPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup    overlay;
        [SerializeField] private RectTransform  card;
        [SerializeField] private Button         closeButton;
        [SerializeField] private Button         backdrop;
        [SerializeField] private TextMeshProUGUI headerJP;
        [SerializeField] private TextMeshProUGUI headerEN;
        [SerializeField] private Transform       rulesContent;

        bool _busy;

        void Start()
        {
            closeButton?.onClick.AddListener(Hide);
            backdrop?.onClick.AddListener(Hide);
            LanguageSettings.OnLanguageChanged += ApplyLanguage;
            ApplyLanguage();
        }

        void OnDestroy() => LanguageSettings.OnLanguageChanged -= ApplyLanguage;

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (headerJP) headerJP.gameObject.SetActive(!en);
            if (headerEN) headerEN.gameObject.SetActive(en);
            if (rulesContent == null) return;
            foreach (Transform row in rulesContent)
            {
                var j = row.Find("JP"); if (j) j.gameObject.SetActive(!en);
                var e = row.Find("EN"); if (e) e.gameObject.SetActive(en);
            }
        }

        public void Show()
        {
            if (_busy) return;
            gameObject.SetActive(true);
            StartCoroutine(Animate(true));
        }

        public void Hide()
        {
            if (_busy) return;
            StartCoroutine(Animate(false));
        }

        IEnumerator Animate(bool show)
        {
            _busy = true;
            overlay.blocksRaycasts = show;
            overlay.interactable   = show;

            float alphaFrom  = show ? 0f : 1f;
            float alphaTo    = show ? 1f : 0f;
            var   scaleFrom  = show ? Vector3.one * 0.88f : Vector3.one;
            var   scaleTo    = show ? Vector3.one          : Vector3.one * 0.88f;

            overlay.alpha = alphaFrom;
            if (card) card.localScale = scaleFrom;

            float dur = 0.26f, elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                overlay.alpha = Mathf.Lerp(alphaFrom, alphaTo, t);
                if (card) card.localScale = Vector3.Lerp(scaleFrom, scaleTo, t);
                yield return null;
            }
            overlay.alpha = alphaTo;
            if (card) card.localScale = scaleTo;
            if (!show) gameObject.SetActive(false);
            _busy = false;
        }
    }
}
