using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BomBomLemon.Game.Topics;

namespace BomBomLemon.Title
{
    public class TopicCustomizePanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup    overlay;
        [SerializeField] RectTransform  card;
        [SerializeField] Button         closeButton;
        [SerializeField] Button         backdrop;
        [SerializeField] RectTransform  listContent;
        [SerializeField] TopicEditDialog editDialog;
        [SerializeField] Button         addButton;
        [SerializeField] Button         resetButton;
        [SerializeField] TMP_FontAsset  font;

        const float RowH  = 172f;
        const float RowGap = 3f;
        const float PadV   = 16f;

        bool _busy;
        readonly List<GameObject> _rows = new();

        void Start()
        {
            closeButton?.onClick.AddListener(Hide);
            backdrop?.onClick.AddListener(Hide);
            addButton?.onClick.AddListener(OnAdd);
            resetButton?.onClick.AddListener(OnReset);
        }

        public void Show()
        {
            if (_busy) return;
            Refresh();
            gameObject.SetActive(true);
            StartCoroutine(Animate(true));
        }

        public void Hide()
        {
            if (_busy) return;
            StartCoroutine(Animate(false));
        }

        // ─── list ───────────────────────────────────────────

        void Refresh()
        {
            foreach (var r in _rows) if (r) Destroy(r);
            _rows.Clear();

            var db = TopicRuntimeDatabase.Instance;
            if (db == null || listContent == null) return;

            float y = PadV;
            for (int i = 0; i < db.Topics.Count; i++)
            {
                int idx = i;
                var row = BuildRow(listContent, i, db.Topics[i], y,
                    () => OnEdit(idx), () => OnDelete(idx));
                _rows.Add(row);
                y += RowH + RowGap;
            }
            y += PadV;
            listContent.sizeDelta = new Vector2(0f, y);
        }

        GameObject BuildRow(RectTransform parent, int index, Topic t, float yTop,
                            System.Action onEdit, System.Action onDelete)
        {
            bool even = index % 2 == 0;
            var go = new GameObject($"Row{index}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0f, RowH);
            r.anchoredPosition = new Vector2(0f, -yTop);

            var bg = go.AddComponent<Image>();
            bg.color = even
                ? new Color(1f, 0.98f, 0.92f, 1f)
                : new Color(0.96f, 0.92f, 0.80f, 0.60f);
            bg.raycastTarget = false;

            // Number badge
            MakeBadge(go.transform, index + 1);

            // Text block (left of buttons)
            float btnW = 88f;
            float textX = 68f;
            float textW = -(textX + btnW + 16f);

            // Topic JP
            AddTmp(go.transform, "TopicJP",
                string.IsNullOrEmpty(t.Text) ? t.TextEN : t.Text,
                30f, FontStyles.Bold, new Color(0.18f, 0.08f, 0.01f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(textX, -28f), new Vector2(textW, 36f),
                true, TextAlignmentOptions.MidlineLeft);

            // Topic EN
            AddTmp(go.transform, "TopicEN",
                string.IsNullOrEmpty(t.TextEN) ? "" : t.TextEN,
                19f, FontStyles.Normal, new Color(0.40f, 0.26f, 0.10f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(textX, 4f), new Vector2(textW, 26f),
                true, TextAlignmentOptions.MidlineLeft);

            // Labels row
            string lo = string.IsNullOrEmpty(t.LowLabel) ? t.LowLabelEN : t.LowLabel;
            string hi = string.IsNullOrEmpty(t.HighLabel) ? t.HighLabelEN : t.HighLabel;
            AddTmp(go.transform, "Labels",
                $"低 {lo}  →  高 {hi}",
                21f, FontStyles.Normal, new Color(0.28f, 0.52f, 0.22f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(textX, 34f), new Vector2(textW, 28f),
                true, TextAlignmentOptions.MidlineLeft);

            // Hint low
            string hl = string.IsNullOrEmpty(t.HintLow) ? t.HintLowEN : t.HintLow;
            AddTmp(go.transform, "HintLow",
                $"低い数字の例：{hl}",
                19f, FontStyles.Normal, new Color(0.38f, 0.28f, 0.14f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(textX, 64f), new Vector2(textW, 26f),
                true, TextAlignmentOptions.MidlineLeft);

            // Hint high
            string hh = string.IsNullOrEmpty(t.HintHigh) ? t.HintHighEN : t.HintHigh;
            AddTmp(go.transform, "HintHigh",
                $"高い数字の例：{hh}",
                19f, FontStyles.Normal, new Color(0.38f, 0.28f, 0.14f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(textX, 86f), new Vector2(textW, 26f),
                true, TextAlignmentOptions.MidlineLeft);

            // Edit button
            MakeRowButton(go.transform, "EditBtn", "編集\nEdit",
                new Color(0.30f, 0.55f, 0.90f, 0.90f),
                new Vector2(1f, 0.5f), new Vector2(-12f, -24f), new Vector2(76f, 56f),
                onEdit);

            // Delete button
            MakeRowButton(go.transform, "DelBtn", "削除\nDel",
                new Color(0.85f, 0.28f, 0.22f, 0.88f),
                new Vector2(1f, 0.5f), new Vector2(-12f, 40f), new Vector2(76f, 44f),
                onDelete);

            return go;
        }

        void MakeBadge(Transform parent, int number)
        {
            var go = new GameObject("Badge", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0.5f);
            r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.sizeDelta = new Vector2(44f, 44f);
            r.anchoredPosition = new Vector2(10f, 0f);
            var img = go.AddComponent<Image>();
            img.color = BadgeColor(number);
            img.raycastTarget = false;

            var ngo = new GameObject("N", typeof(RectTransform));
            ngo.transform.SetParent(go.transform, false);
            var nr = ngo.GetComponent<RectTransform>();
            nr.anchorMin = Vector2.zero; nr.anchorMax = Vector2.one;
            nr.offsetMin = Vector2.zero; nr.offsetMax = Vector2.zero;
            var tmp = ngo.AddComponent<TextMeshProUGUI>();
            tmp.text = number.ToString(); tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white; tmp.raycastTarget = false;
            if (font) tmp.font = font;
        }

        void MakeRowButton(Transform parent, string name, string label, Color color,
                           Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(1f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.highlightedColor = Color.white * 1.15f;
            cols.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = cols; btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());

            var tgo = new GameObject("L", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 16f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white; tmp.raycastTarget = false;
            if (font) tmp.font = font;
        }

        TextMeshProUGUI AddTmp(Transform parent, string name, string text, float size,
                               FontStyles style, Color color,
                               Vector2 anchorMin, Vector2 anchorMax,
                               Vector2 pos, Vector2 delta, bool wrap,
                               TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin; r.anchorMax = anchorMax;
            r.pivot = new Vector2(0f, 0.5f);
            r.sizeDelta = delta; r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
            tmp.color = color; tmp.alignment = align;
            tmp.enableWordWrapping = wrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            if (font) tmp.font = font;
            return tmp;
        }

        // ─── actions ────────────────────────────────────────

        void OnAdd()
        {
            if (editDialog == null) return;
            editDialog.ShowForAdd(t =>
            {
                TopicRuntimeDatabase.Instance?.Topics.Add(t);
                TopicRuntimeDatabase.Instance?.Save();
                Refresh();
            });
        }

        void OnEdit(int idx)
        {
            var db = TopicRuntimeDatabase.Instance;
            if (db == null || idx >= db.Topics.Count || editDialog == null) return;
            editDialog.ShowForEdit(db.Topics[idx], t =>
            {
                db.Topics[idx] = t;
                db.Save();
                Refresh();
            });
        }

        void OnDelete(int idx)
        {
            var db = TopicRuntimeDatabase.Instance;
            if (db == null || idx >= db.Topics.Count) return;
            db.Topics.RemoveAt(idx);
            db.Save();
            Refresh();
        }

        void OnReset()
        {
            TopicRuntimeDatabase.Instance?.ResetToDefaults();
            Refresh();
        }

        // ─── animation ──────────────────────────────────────

        IEnumerator Animate(bool show)
        {
            _busy = true;
            overlay.blocksRaycasts = show; overlay.interactable = show;
            float from = show ? 0f : 1f, to = show ? 1f : 0f;
            var sf = show ? Vector3.one * 0.88f : Vector3.one;
            var st = show ? Vector3.one : Vector3.one * 0.88f;
            overlay.alpha = from;
            if (card) card.localScale = sf;
            float dur = 0.26f, e = 0f;
            while (e < dur)
            {
                e += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, e / dur);
                overlay.alpha = Mathf.Lerp(from, to, t);
                if (card) card.localScale = Vector3.Lerp(sf, st, t);
                yield return null;
            }
            overlay.alpha = to;
            if (card) card.localScale = st;
            if (!show) gameObject.SetActive(false);
            _busy = false;
        }

        static Color BadgeColor(int n)
        {
            Color[] p =
            {
                new(0.98f,0.78f,0.18f), new(0.98f,0.60f,0.22f),
                new(0.88f,0.38f,0.28f), new(0.72f,0.34f,0.62f),
                new(0.36f,0.60f,0.88f), new(0.30f,0.75f,0.58f),
                new(0.48f,0.78f,0.32f), new(0.85f,0.68f,0.26f),
            };
            return p[(n - 1) % p.Length];
        }
    }
}
