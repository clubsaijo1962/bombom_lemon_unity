using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class SingleSettingsSceneBuilder
    {
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.98f, 0.90f, 0.55f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographic = true;
                camera.allowMSAA = false;
            }

            // Canvas
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = new Color(0.98f, 0.90f, 0.55f);
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();
            var pill   = GetBuiltinUISprite();

            // Panel CanvasGroup（コンテンツフェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ────────────── HUD 右上 ──────────────
            var lemonTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Title_Lemon.png")
                           ?? FindTexture("Lemon");
            var starTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(
                               "Assets/Sprites/UI/kenney_ui-pack/PNG/Yellow/Default/star.png")
                           ?? FindTexture("star");

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = new Vector2(1f, 1f);
            hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot     = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(460f, 88f);
            hudR.anchoredPosition = new Vector2(-14f, -110f);

            var hudBg = hudGO.AddComponent<Image>();
            if (pill != null) { hudBg.sprite = pill; hudBg.type = Image.Type.Sliced; }
            hudBg.color = new Color(0.18f, 0.08f, 0.01f, 0.72f);

            // ライフ pill（左半分）レモン色
            var lifePillGO = new GameObject("LifePill", typeof(RectTransform));
            lifePillGO.transform.SetParent(hudGO.transform, false);
            var lpR = lifePillGO.GetComponent<RectTransform>();
            lpR.anchorMin = new Vector2(0f, 0f);    lpR.anchorMax = new Vector2(0.47f, 1f);
            lpR.offsetMin = new Vector2(4f, 4f);    lpR.offsetMax = new Vector2(-4f, -4f);
            var lpBg = lifePillGO.AddComponent<Image>();
            if (pill != null) { lpBg.sprite = pill; lpBg.type = Image.Type.Sliced; }
            lpBg.color = new Color(0.88f, 0.76f, 0.12f, 0.90f);

            TextMeshProUGUI lifeLabel;
            if (lemonTex != null)
            {
                var liGO = new GameObject("LemonIcon", typeof(RectTransform));
                liGO.transform.SetParent(lifePillGO.transform, false);
                var liR = liGO.GetComponent<RectTransform>();
                liR.anchorMin = new Vector2(0f, 0f);    liR.anchorMax = new Vector2(0.42f, 1f);
                liR.offsetMin = new Vector2(4f, 3f);    liR.offsetMax = new Vector2(0f, -3f);
                var liRaw = liGO.AddComponent<RawImage>();
                liRaw.texture = lemonTex; liRaw.raycastTarget = false;

                var lcGO = new GameObject("LifeLabel", typeof(RectTransform));
                lcGO.transform.SetParent(lifePillGO.transform, false);
                var lcR = lcGO.GetComponent<RectTransform>();
                lcR.anchorMin = new Vector2(0.42f, 0f); lcR.anchorMax = new Vector2(1f, 1f);
                lcR.offsetMin = new Vector2(0f, 0f);    lcR.offsetMax = new Vector2(-4f, 0f);
                var lcTmp = lcGO.AddComponent<TextMeshProUGUI>();
                lcTmp.text = "×8"; lcTmp.fontSize = 34f; lcTmp.fontStyle = FontStyles.Bold;
                lcTmp.alignment = TextAlignmentOptions.Center;
                lcTmp.color = new Color(0.18f, 0.08f, 0.01f, 1f); lcTmp.raycastTarget = false;
                if (jpFont != null) lcTmp.font = jpFont;
                lifeLabel = lcTmp;
            }
            else
            {
                lifeLabel = MakeFillLabel(lifePillGO.transform, "LifeLabel", "♥ ×8",
                    36f, new Color(0.18f, 0.08f, 0.01f, 1f), FontStyles.Bold, jpFont);
            }

            // ヘルプ pill（右半分）スター画像
            var helpPillGO = new GameObject("HelpPill", typeof(RectTransform));
            helpPillGO.transform.SetParent(hudGO.transform, false);
            var hpR = helpPillGO.GetComponent<RectTransform>();
            hpR.anchorMin = new Vector2(0.53f, 0f); hpR.anchorMax = new Vector2(1f, 1f);
            hpR.offsetMin = new Vector2(4f, 4f);     hpR.offsetMax = new Vector2(-4f, -4f);
            var hpBg = helpPillGO.AddComponent<Image>();
            if (pill != null) { hpBg.sprite = pill; hpBg.type = Image.Type.Sliced; }
            hpBg.color = new Color(0.22f, 0.48f, 0.78f, 0.70f);

            TextMeshProUGUI helpLabel;
            if (starTex != null)
            {
                var ciGO = new GameObject("StarIcon", typeof(RectTransform));
                ciGO.transform.SetParent(helpPillGO.transform, false);
                var ciR = ciGO.GetComponent<RectTransform>();
                ciR.anchorMin = new Vector2(0f, 0f);    ciR.anchorMax = new Vector2(0.40f, 1f);
                ciR.offsetMin = new Vector2(4f, 4f);    ciR.offsetMax = new Vector2(0f, -4f);
                var ciRaw = ciGO.AddComponent<RawImage>();
                ciRaw.texture = starTex; ciRaw.raycastTarget = false;

                var cnGO = new GameObject("HelpLabel", typeof(RectTransform));
                cnGO.transform.SetParent(helpPillGO.transform, false);
                var cnR = cnGO.GetComponent<RectTransform>();
                cnR.anchorMin = new Vector2(0.40f, 0f); cnR.anchorMax = new Vector2(1f, 1f);
                cnR.offsetMin = new Vector2(0f, 0f);    cnR.offsetMax = new Vector2(-4f, 0f);
                var cnTmp = cnGO.AddComponent<TextMeshProUGUI>();
                cnTmp.text = "×0"; cnTmp.fontSize = 34f; cnTmp.fontStyle = FontStyles.Bold;
                cnTmp.alignment = TextAlignmentOptions.Center;
                cnTmp.color = Color.white; cnTmp.raycastTarget = false;
                if (jpFont != null) cnTmp.font = jpFont;
                helpLabel = cnTmp;
            }
            else
            {
                helpLabel = MakeFillLabel(helpPillGO.transform, "HelpLabel", "★ ×0",
                    36f, Color.white, FontStyles.Bold, jpFont);
            }

            // ────────────── 戻るボタン（左上）──────────────
            var backBtnGO = MakeButton(panelGO.transform, "BackButton", "← 戻る",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -110f), new Vector2(200f, 60f),
                new Color(0.78f, 0.62f, 0.20f, 0.80f),
                new Color(0.22f, 0.10f, 0.02f, 1f), 28f, jpFont, pill);

            // ────────────── ヘッダー ──────────────
            var headerLabel = MakeLabel(panelGO.transform, "Header", "人数を決めよう",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 650f), new Vector2(900f, 72f),
                52f, new Color(0.26f, 0.10f, 0.01f, 1f), FontStyles.Bold, jpFont);

            // ────────────── 人数コントロール ──────────────
            var countSectionLabel = MakeLabel(panelGO.transform, "CountSectionLabel", "人数",
                new Vector2(0.5f, 0.5f), new Vector2(-295f, 500f), new Vector2(130f, 60f),
                36f, new Color(0.35f, 0.15f, 0.03f, 0.90f), FontStyles.Bold, jpFont);
            countSectionLabel.alignment = TextAlignmentOptions.MidlineRight;

            var decBtnGO = MakeButton(panelGO.transform, "DecreaseBtn", "－",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-130f, 500f), new Vector2(92f, 92f),
                new Color(0.78f, 0.28f, 0.18f, 0.88f), Color.white, 52f, jpFont, pill);

            var countFieldGO = MakeCountInputField(panelGO.transform, "CountField", "2",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 500f), new Vector2(130f, 82f),
                50f, jpFont, pill);

            var incBtnGO = MakeButton(panelGO.transform, "IncreaseBtn", "＋",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(130f, 500f), new Vector2(92f, 92f),
                new Color(0.28f, 0.68f, 0.30f, 0.90f), Color.white, 52f, jpFont, pill);

            var unitLabel = MakeLabel(panelGO.transform, "UnitLabel", "人",
                new Vector2(0.5f, 0.5f), new Vector2(232f, 500f), new Vector2(60f, 60f),
                36f, new Color(0.35f, 0.15f, 0.03f, 0.80f), FontStyles.Normal, jpFont);
            unitLabel.alignment = TextAlignmentOptions.MidlineLeft;

            // ────────────── 情報テキスト ──────────────
            var infoLabel = MakeLabel(panelGO.transform, "InfoLabel",
                "ライフ: 8個   ／   ヘルプカード: 0枚",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 372f), new Vector2(900f, 52f),
                33f, new Color(0.38f, 0.18f, 0.05f, 0.80f), FontStyles.Normal, jpFont);

            // 区切り線
            var divGO = new GameObject("Divider", typeof(RectTransform));
            divGO.transform.SetParent(panelGO.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.60f, 0.40f, 0.12f, 0.25f);
            divImg.raycastTarget = false;
            var divR = divGO.GetComponent<RectTransform>();
            divR.anchorMin = new Vector2(0.5f, 0.5f); divR.anchorMax = new Vector2(0.5f, 0.5f);
            divR.pivot = new Vector2(0.5f, 0.5f);
            divR.sizeDelta = new Vector2(900f, 2f);
            divR.anchoredPosition = new Vector2(0f, 296f);

            // ────────────── プレイヤー名ヘッダー ──────────────
            var playerNamesHeader = MakeLabel(panelGO.transform, "PlayerNamesHeader", "プレイヤー名",
                new Vector2(0.5f, 0.5f), new Vector2(-180f, 226f), new Vector2(520f, 52f),
                38f, new Color(0.35f, 0.15f, 0.03f, 0.90f), FontStyles.Bold, jpFont);
            playerNamesHeader.alignment = TextAlignmentOptions.MidlineLeft;

            // ────────────── スクロールビュー ──────────────
            var scrollGO = new GameObject("NameScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(panelGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin = new Vector2(0.5f, 0.5f);
            scrollR.anchorMax = new Vector2(0.5f, 0.5f);
            scrollR.pivot = new Vector2(0.5f, 0.5f);
            scrollR.sizeDelta = new Vector2(980f, 720f);
            scrollR.anchoredPosition = new Vector2(0f, -174f);

            // タッチ判定用（空白エリアでもスクロール受け付ける）
            var scrollBg = scrollGO.AddComponent<Image>();
            scrollBg.color = Color.clear;

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 24f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.decelerationRate = 0.14f;

            var viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            viewportGO.AddComponent<RectMask2D>();
            var vpR = viewportGO.GetComponent<RectTransform>();
            vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one;
            vpR.offsetMin = Vector2.zero; vpR.offsetMax = Vector2.zero;
            scrollRect.viewport = vpR;

            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentR = contentGO.GetComponent<RectTransform>();
            contentR.anchorMin = new Vector2(0f, 1f);
            contentR.anchorMax = new Vector2(1f, 1f);
            contentR.pivot = new Vector2(0.5f, 1f);
            contentR.offsetMin = Vector2.zero;
            contentR.offsetMax = Vector2.zero;
            contentR.sizeDelta = new Vector2(0f, 200f);
            scrollRect.content = contentR;

            // ────────────── スタートボタン ──────────────
            var startBtnGO = MakeButton(panelGO.transform, "StartButton", "ゲームスタート ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -806f), new Vector2(900f, 120f),
                new Color(0.98f, 0.75f, 0.15f, 0.96f),
                new Color(0.18f, 0.07f, 0.01f, 1f), 44f, jpFont, pill);

            // ────────────── SingleSettingsController ──────────────
            var ctrlGO = new GameObject("SingleSettingsController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<SingleSettingsController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("countField").objectReferenceValue        = countFieldGO.GetComponent<TMP_InputField>();
            so.FindProperty("decreaseBtn").objectReferenceValue       = decBtnGO.GetComponent<Button>();
            so.FindProperty("increaseBtn").objectReferenceValue       = incBtnGO.GetComponent<Button>();
            so.FindProperty("lifeCountLabel").objectReferenceValue    = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("infoLabel").objectReferenceValue         = infoLabel;
            so.FindProperty("listContent").objectReferenceValue       = contentR;
            so.FindProperty("font").objectReferenceValue              = jpFont;
            so.FindProperty("startButton").objectReferenceValue       = startBtnGO.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue        = backBtnGO.GetComponent<Button>();
            so.FindProperty("headerLabel").objectReferenceValue       = headerLabel;
            so.FindProperty("startBtnLabel").objectReferenceValue     = startBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("backBtnLabel").objectReferenceValue      = backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("playerNamesHeader").objectReferenceValue = playerNamesHeader;
            so.FindProperty("countSectionLabel").objectReferenceValue = countSectionLabel;
            so.FindProperty("panelGroup").objectReferenceValue        = panelCG;
            so.FindProperty("gameSceneName").stringValue              = "Game";
            so.FindProperty("backSceneName").stringValue              = "PlayerSetup";

            // ScreenFade（最前面）
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SingleSettings.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/SingleSettings.unity", 3);

            Debug.Log("[SingleSettingsSceneBuilder] SingleSettings シーンを作成しました → Assets/Scenes/SingleSettings.unity");
        }

        // ─────────────────────────────────────────────────────────────────
        // ヘルパー
        // ─────────────────────────────────────────────────────────────────

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static TextMeshProUGUI MakeFillLabel(Transform parent, string name, string text,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(4f, 0f); r.offsetMax = new Vector2(-4f, 0f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        static GameObject MakeButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            Color bgColor, Color textColor, float fontSize, TMP_FontAsset font, Sprite pill)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;

            var shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(go.transform, false);
            var sr = shadowGO.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(2f, -7f); sr.offsetMax = new Vector2(-2f, -1f);
            var si = shadowGO.AddComponent<Image>();
            if (pill != null) { si.sprite = pill; si.type = Image.Type.Sliced; }
            si.color = new Color(0.40f, 0.22f, 0.04f, 0.18f);
            si.raycastTarget = false;

            var bg = go.AddComponent<Image>();
            if (pill != null) { bg.sprite = pill; bg.type = Image.Type.Sliced; }
            bg.color = bgColor;

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.90f, 1f);
            cols.pressedColor = new Color(0.80f, 0.76f, 0.62f, 1f);
            cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;

            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(12f, 0f); tr.offsetMax = new Vector2(-12f, 0f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return go;
        }

        static GameObject MakeCountInputField(Transform parent, string name, string defaultText,
            Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, TMP_FontAsset font, Sprite pill)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (pill != null) { bg.sprite = pill; bg.type = Image.Type.Sliced; }
            bg.color = new Color(1f, 0.97f, 0.88f, 0.95f);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic = bg;
            inputField.characterLimit = 2;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(6f, 4f); taR.offsetMax = new Vector2(-6f, -4f);

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = Vector2.zero; txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.text = defaultText; txtTmp.fontSize = fontSize;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.color = new Color(0.18f, 0.08f, 0.01f, 1f);
            txtTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) txtTmp.font = font;

            inputField.textComponent = txtTmp;
            inputField.text = defaultText;
            return go;
        }

        static Texture2D FindTexture(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Texture2D {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                if (tex != null) return tex;
            }
            var allGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains(keyword.ToLower()))
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }

        static TMP_FontAsset FindJapaneseTMPFont()
        {
            string[] candidates = { "NotoSansJP", "NotoSans", "Noto", "Meiryo", "YuGothic", "Japanese", "JP" };
            foreach (var kw in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {kw}");
                if (guids.Length > 0)
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            var all = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (all.Length > 0)
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(all[0]));
            return null;
        }
    }
}
