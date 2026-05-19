using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BomBomLemon.Game.Topics;

namespace BomBomLemon.Title
{
    public class TopicEditDialog : MonoBehaviour
    {
        [SerializeField] CanvasGroup    overlay;
        [SerializeField] RectTransform  card;
        [SerializeField] Button         saveButton;
        [SerializeField] Button         cancelButton;
        [SerializeField] Button         backdrop;
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TMP_FontAsset  font;

        // Input fields — assigned by scene builder or BuildFields()
        [SerializeField] TMP_InputField  fTextJP,    fLowJP,    fHighJP,    fHintLowJP,    fHintHighJP;
        [SerializeField] TMP_InputField  fTextEN,    fLowEN,    fHighEN,    fHintLowEN,    fHintHighEN;
        [SerializeField] TextMeshProUGUI saveBtnLabel;

        Action<Topic> _onSave;
        bool _busy;

        void Start()
        {
            saveButton?.onClick.AddListener(OnSave);
            cancelButton?.onClick.AddListener(Hide);
            backdrop?.onClick.AddListener(Hide);
        }

        public void ShowForAdd(Action<Topic> onSave)
        {
            _onSave = onSave;
            bool en = LanguageSettings.IsEnglish;
            if (titleLabel)  titleLabel.text  = en ? "Add Topic" : "お題を追加";
            if (saveBtnLabel) saveBtnLabel.text = en ? "Save" : "保存";
            SetFields("", "", "", "", "", "", "", "", "", "");
            Show();
        }

        public void ShowForEdit(Topic t, Action<Topic> onSave)
        {
            _onSave = onSave;
            bool en = LanguageSettings.IsEnglish;
            if (titleLabel)  titleLabel.text  = en ? "Edit Topic" : "お題を編集";
            if (saveBtnLabel) saveBtnLabel.text = en ? "Save" : "保存";
            SetFields(t.Text, t.LowLabel, t.HighLabel, t.HintLow, t.HintHigh,
                      t.TextEN, t.LowLabelEN, t.HighLabelEN, t.HintLowEN, t.HintHighEN);
            Show();
        }

        void SetFields(string jp, string lo, string hi, string hl, string hh,
                       string en, string loEN, string hiEN, string hlEN, string hhEN)
        {
            Set(fTextJP,    jp);   Set(fLowJP,    lo);   Set(fHighJP,    hi);
            Set(fHintLowJP, hl);   Set(fHintHighJP, hh);
            Set(fTextEN,    en);   Set(fLowEN,    loEN);  Set(fHighEN,    hiEN);
            Set(fHintLowEN, hlEN); Set(fHintHighEN, hhEN);
        }

        static void Set(TMP_InputField f, string v) { if (f) f.text = v; }
        static string Get(TMP_InputField f) => f ? f.text.Trim() : "";

        void OnSave()
        {
            if (string.IsNullOrEmpty(Get(fTextJP)) && string.IsNullOrEmpty(Get(fTextEN))) return;
            var t = new Topic(Get(fTextJP), Get(fLowJP), Get(fHighJP), Get(fHintLowJP), Get(fHintHighJP),
                              Get(fTextEN), Get(fLowEN), Get(fHighEN), Get(fHintLowEN), Get(fHintHighEN));
            Hide();
            _onSave?.Invoke(t);
        }

        void Show()
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
            if (overlay) { overlay.blocksRaycasts = show; overlay.interactable = show; }
            float from = show ? 0f : 1f, to = show ? 1f : 0f;
            var sf = show ? Vector3.one * 0.90f : Vector3.one;
            var st = show ? Vector3.one : Vector3.one * 0.90f;
            if (overlay) overlay.alpha = from;
            if (card)    card.localScale = sf;
            float dur = 0.20f, elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                if (overlay) overlay.alpha = Mathf.Lerp(from, to, t);
                if (card)    card.localScale = Vector3.Lerp(sf, st, t);
                yield return null;
            }
            if (overlay) overlay.alpha = to;
            if (card)    card.localScale = st;
            if (!show) gameObject.SetActive(false);
            _busy = false;
        }
    }
}
