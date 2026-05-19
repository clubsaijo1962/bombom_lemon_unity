using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    public class SingleSettingsController : MonoBehaviour
    {
        [SerializeField] TMP_InputField  countField;
        [SerializeField] Button          decreaseBtn;
        [SerializeField] Button          increaseBtn;
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;
        [SerializeField] TextMeshProUGUI infoLabel;
        [SerializeField] TextMeshProUGUI helpInfoLabel;
        [SerializeField] RectTransform   listContent;
        [SerializeField] TMP_FontAsset   font;
        [SerializeField] Button          startButton;
        [SerializeField] Button          backButton;
        [SerializeField] CanvasGroup     screenFade;
        [SerializeField] CanvasGroup     panelGroup;
        [SerializeField] TextMeshProUGUI headerLabel;
        [SerializeField] TextMeshProUGUI startBtnLabel;
        [SerializeField] TextMeshProUGUI backBtnLabel;
        [SerializeField] TextMeshProUGUI playerNamesHeader;
        [SerializeField] TextMeshProUGUI countSectionLabel;
        [SerializeField] string          gameSceneName = "Game";
        [SerializeField] string          backSceneName = "PlayerSetup";

        const int   MinPlayers = 2;
        const int   MaxPlayers = 24;
        const float RowH  = 90f;
        const float RowGap = 6f;
        const float PadV   = 8f;

        int _count = 2;
        readonly List<GameObject>    _rows   = new();
        readonly List<TMP_InputField> _fields = new();

        static readonly Color[] RowBg =
        {
            new(1f,   0.97f, 0.90f, 1f),
            new(0.96f,0.90f, 0.78f, 0.60f),
        };

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            decreaseBtn?.onClick.AddListener(OnDecrease);
            increaseBtn?.onClick.AddListener(OnIncrease);
            countField?.onEndEdit.AddListener(OnCountEdited);
            startButton?.onClick.AddListener(OnStart);
            backButton?.onClick.AddListener(OnBack);

            LanguageSettings.OnLanguageChanged += ApplyLanguage;
            ApplyLanguage();
            SetCount(2);
            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void OnDestroy() => LanguageSettings.OnLanguageChanged -= ApplyLanguage;

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (headerLabel)       headerLabel.text       = en ? "Player Setup"     : "人数を決めよう";
            if (countSectionLabel) countSectionLabel.text = en ? "Players"           : "人数";
            if (playerNamesHeader) playerNamesHeader.text = en ? "Player Names"      : "プレイヤー名";
            if (startBtnLabel)     startBtnLabel.text     = en ? "Start Game ▶"     : "ゲームスタート ▶";
            if (backBtnLabel)      backBtnLabel.text      = en ? "← Back"            : "← 戻る";
            RefreshInfoLabel();
            RefreshPlaceholders();
        }

        void RefreshInfoLabel()
        {
            bool en = LanguageSettings.IsEnglish;
            int life = _count * 4;
            int help = CalcHelp(_count);
            if (infoLabel)     infoLabel.text     = en ? $"Lives: {life}"      : $"ライフ: {life}個";
            if (helpInfoLabel) helpInfoLabel.text = en ? $"Help cards: {help}" : $"ヘルプカード: {help}枚";
        }

        void RefreshHUD()
        {
            if (lifeCountLabel)     lifeCountLabel.text     = $"×{_count * 4}";
            if (helpCardCountLabel) helpCardCountLabel.text = $"×{CalcHelp(_count)}";
        }

        void RefreshPlaceholders()
        {
            bool en = LanguageSettings.IsEnglish;
            for (int i = 0; i < _fields.Count; i++)
            {
                if (_fields[i].placeholder is TextMeshProUGUI ph)
                    ph.text = en ? $"Player {i + 1}" : $"プレイヤー{i + 1}";
            }
        }

        void SetCount(int n)
        {
            n = Mathf.Clamp(n, MinPlayers, MaxPlayers);
            while (_rows.Count < n)  AddRow(_rows.Count);
            while (_rows.Count > n)  RemoveLastRow();
            _count = n;
            if (countField) countField.text = n.ToString();
            ResizeContent();
            RefreshInfoLabel();
            RefreshHUD();
        }

        void OnDecrease() => SetCount(_count - 1);
        void OnIncrease() => SetCount(_count + 1);

        void OnCountEdited(string s)
        {
            if (int.TryParse(s, out int n)) SetCount(n);
            else if (countField) countField.text = _count.ToString();
        }

        // ── 動的行 ────────────────────────────────────────────────

        void AddRow(int idx)
        {
            bool en = LanguageSettings.IsEnglish;

            var rowGO = new GameObject($"Row{idx}", typeof(RectTransform));
            rowGO.transform.SetParent(listContent, false);
            var r = rowGO.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot     = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0f, RowH);
            r.anchoredPosition = new Vector2(0f, -(PadV + idx * (RowH + RowGap)));

            rowGO.AddComponent<Image>().color = RowBg[idx % 2];

            // バッジ
            var badgeGO = new GameObject("Badge", typeof(RectTransform));
            badgeGO.transform.SetParent(rowGO.transform, false);
            var br = badgeGO.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0f, 0.5f); br.anchorMax = new Vector2(0f, 0.5f);
            br.pivot = new Vector2(0f, 0.5f);
            br.sizeDelta = new Vector2(52f, 52f);
            br.anchoredPosition = new Vector2(14f, 0f);
            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = BadgeColor(idx);
            badgeImg.raycastTarget = false;

            var numGO = new GameObject("N", typeof(RectTransform));
            numGO.transform.SetParent(badgeGO.transform, false);
            var nr = numGO.GetComponent<RectTransform>();
            nr.anchorMin = Vector2.zero; nr.anchorMax = Vector2.one;
            nr.offsetMin = Vector2.zero; nr.offsetMax = Vector2.zero;
            var numTmp = numGO.AddComponent<TextMeshProUGUI>();
            numTmp.text = (idx + 1).ToString();
            numTmp.fontSize = 30f;
            numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = Color.white;
            numTmp.raycastTarget = false;
            if (font) numTmp.font = font;

            // 名前 InputField
            var fieldGO = new GameObject("NameField", typeof(RectTransform));
            fieldGO.transform.SetParent(rowGO.transform, false);
            var fr = fieldGO.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0f, 0f);
            fr.anchorMax = new Vector2(1f, 1f);
            fr.offsetMin = new Vector2(76f, 4f);
            fr.offsetMax = new Vector2(-12f, -4f);

            var fieldBg = fieldGO.AddComponent<Image>();
            fieldBg.color = Color.clear;
            var inputField = fieldGO.AddComponent<TMP_InputField>();
            inputField.targetGraphic = fieldBg;
            inputField.characterLimit = 20;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(fieldGO.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(4f, 2f); taR.offsetMax = new Vector2(-4f, -2f);

            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(taGO.transform, false);
            var phR = phGO.GetComponent<RectTransform>();
            phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
            phR.offsetMin = Vector2.zero; phR.offsetMax = Vector2.zero;
            var phTmp = phGO.AddComponent<TextMeshProUGUI>();
            phTmp.text = en ? $"Player {idx + 1}" : $"プレイヤー{idx + 1}";
            phTmp.fontSize = 36f;
            phTmp.color = new Color(0.50f, 0.38f, 0.18f, 0.55f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;
            phTmp.raycastTarget = false;
            if (font) phTmp.font = font;

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = Vector2.zero; txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 36f;
            txtTmp.color = new Color(0.18f, 0.08f, 0.01f, 1f);
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
            if (font) txtTmp.font = font;

            inputField.textViewport  = taR;
            inputField.textComponent = txtTmp;
            inputField.placeholder   = phTmp;
            inputField.text          = "";

            rowGO.AddComponent<ScrollDragForwarder>();

            _rows.Add(rowGO);
            _fields.Add(inputField);
        }

        void RemoveLastRow()
        {
            if (_rows.Count == 0) return;
            int last = _rows.Count - 1;
            Destroy(_rows[last]);
            _rows.RemoveAt(last);
            _fields.RemoveAt(last);
        }

        void ResizeContent()
        {
            if (!listContent) return;
            float h = PadV * 2 + _rows.Count * (RowH + RowGap) - RowGap;
            listContent.sizeDelta = new Vector2(0f, Mathf.Max(h, 0f));
        }

        // ── ゲーム開始 ────────────────────────────────────────────

        void OnStart()
        {
            bool en = LanguageSettings.IsEnglish;
            var names = new string[_count];
            for (int i = 0; i < _count; i++)
            {
                string t = i < _fields.Count ? _fields[i].text.Trim() : "";
                names[i] = string.IsNullOrEmpty(t)
                    ? (en ? $"Player {i + 1}" : $"プレイヤー{i + 1}")
                    : t;
            }
            SinglePlayConfig.Set(_count, names);
            StartCoroutine(LoadWithFade(gameSceneName));
        }

        void OnBack() => StartCoroutine(LoadWithFade(backSceneName));

        // ── コルーチン ────────────────────────────────────────────

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

        // ── ヘルパー ──────────────────────────────────────────────

        static int CalcHelp(int count)
        {
            if (count <= 2)  return 0;
            if (count <= 4)  return 1;
            if (count <= 7)  return 2;
            if (count <= 10) return 3;
            return 3 + (count - 8) / 3;
        }

        static Color BadgeColor(int idx)
        {
            Color[] p =
            {
                new(0.98f,0.78f,0.18f), new(0.98f,0.60f,0.22f),
                new(0.88f,0.38f,0.28f), new(0.72f,0.34f,0.62f),
                new(0.36f,0.60f,0.88f), new(0.30f,0.75f,0.58f),
                new(0.48f,0.78f,0.32f), new(0.85f,0.68f,0.26f),
            };
            return p[idx % p.Length];
        }
    }
}
