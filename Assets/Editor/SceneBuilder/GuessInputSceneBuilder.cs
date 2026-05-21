using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class GuessInputSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.96f, 0.90f, 0.78f);
        static readonly Color CardColor    = new(1f,    1f,    1f,    0.97f);
        static readonly Color LemonYellow  = new(0.97f, 0.83f, 0.18f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.32f, 0.12f, 0.72f);
        static readonly Color ChipAlt      = new(0.99f, 0.95f, 0.72f);
        static readonly Color ChipGuesser  = new(0.97f, 0.88f, 0.54f);
        static readonly Color SepColor     = new(0.88f, 0.82f, 0.62f);

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = BgColor;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographic = true;
                camera.allowMSAA = false;
            }

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

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();

            var btnYellow  = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite   = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite  = FindSprite("card");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // Panel CanvasGroup（フェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // コンテンツカード
            {
                var cardGO = new GameObject("ContentCard", typeof(RectTransform));
                cardGO.transform.SetParent(panelGO.transform, false);
                var img = cardGO.AddComponent<Image>();
                img.sprite = uiSprite; img.type = Image.Type.Sliced;
                img.color = CardColor; img.raycastTarget = false;
                var sh = cardGO.AddComponent<Shadow>();
                sh.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.18f);
                sh.effectDistance = new Vector2(0f, -12f);
                var cr = cardGO.GetComponent<RectTransform>();
                cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
                cr.pivot = new Vector2(0.5f, 0.5f);
                cr.sizeDelta = new Vector2(1020f, 1260f);
                cr.anchoredPosition = new Vector2(0f, 90f);
            }

            // HUD 右上
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(340f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);

            MakeHUDGroup(hudGO.transform, "LifeGroup", 0f,    0.47f, lemonSprite, jpFont, out var lifeLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup", 0.53f, 1f,    cardSprite,  jpFont, out var helpLabel);
            helpLabel.text = "×0";

            // HOME ボタン（左上）
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton",
                "HOME",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(160f, 80f),
                Color.white, TextPrimary, 32f, jpFont, btnYellow, ChipAlt);

            // ── レイアウト（ガイドなし） ──
            // card top=720, bottom=-540, height=1260
            // お題(640/525/415) → sep(355) → 最終決定者(280/144) → sep(60)
            // → 入力ラベル(-4) → 入力欄(-145) → 確定(-420)

            MakeLabel(panelGO.transform, "TopicHeader",
                "お題",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 640f), new Vector2(900f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            var topicLabelTmp = MakeLabel(panelGO.transform, "TopicLabel",
                "お題テキスト",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 525f), new Vector2(900f, 130f),
                40f, TextPrimary, FontStyles.Bold, jpFont,
                autoSizeMin: 32f, autoSizeMax: 48f);

            // Low / High ラベル（左右）
            TextMeshProUGUI topicLowTmp, topicHighTmp;
            {
                var rowGO = new GameObject("LowHighRow", typeof(RectTransform));
                rowGO.transform.SetParent(panelGO.transform, false);
                var rr = rowGO.GetComponent<RectTransform>();
                rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
                rr.pivot = new Vector2(0.5f, 0.5f);
                rr.sizeDelta = new Vector2(900f, 52f);
                rr.anchoredPosition = new Vector2(0f, 415f);

                topicLowTmp = MakeLabelInParent(rowGO.transform, "LowLabel", "1 = 低い",
                    new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero,
                    32f, TextMuted, FontStyles.Normal, jpFont, TextAlignmentOptions.MidlineLeft);

                topicHighTmp = MakeLabelInParent(rowGO.transform, "HighLabel", "99 = 高い",
                    new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero,
                    32f, TextMuted, FontStyles.Normal, jpFont, TextAlignmentOptions.MidlineRight);
            }

            MakeSeparator(panelGO.transform, 355f);

            // ── 予想の最終決定者セクション ──
            MakeLabel(panelGO.transform, "FinalGuesserHeader",
                "予想の最終決定者",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), new Vector2(900f, 60f),
                36f, TextPrimary, FontStyles.Bold, jpFont);

            var finalGuesserTmp = MakeNameChip(panelGO.transform, "FinalGuesserChip",
                new Vector2(0f, 144f), ChipGuesser, TextPrimary, jpFont, uiSprite, h: 140f);

            MakeSeparator(panelGO.transform, 60f);

            // ── 数字入力セクション ──
            MakeLabel(panelGO.transform, "InputHeader",
                "予想する数字（1〜99）",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(860f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            // 入力欄：大型・LemonYellow背景で視認性UP
            var inputField = MakeNumberInputField(panelGO.transform, "NumberInputField",
                new Vector2(0f, -145f), new Vector2(560f, 160f), jpFont, uiSprite, btnYellow);

            // 確定ボタン：カード白背景の下スペース（y=-420、下端-479、カード下端-540）
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton",
                "確定 ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -420f), new Vector2(900f, 118f),
                Color.white, TextPrimary, 46f, jpFont, btnYellow, Color.white);

            // コントローラー
            var ctrlGO = new GameObject("GuessInputController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<GuessInputController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("topicLabel").objectReferenceValue         = topicLabelTmp;
            so.FindProperty("topicLowLabel").objectReferenceValue      = topicLowTmp;
            so.FindProperty("topicHighLabel").objectReferenceValue     = topicHighTmp;
            so.FindProperty("finalGuesserLabel").objectReferenceValue  = finalGuesserTmp;
            so.FindProperty("lifeCountLabel").objectReferenceValue     = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("numberInputField").objectReferenceValue   = inputField;
            so.FindProperty("confirmButton").objectReferenceValue      = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue         = homeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue         = panelCG;

            // ScreenFade
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GuessInput.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/GuessInput.unity");

            Debug.Log("[GuessInputSceneBuilder] GuessInput シーンを作成しました");
        }

        // ── TMP_InputField（数字入力）────────────────────────────────

        static TMP_InputField MakeNumberInputField(Transform parent, string name,
            Vector2 pos, Vector2 size, TMP_FontAsset font, Sprite uiSpr, Sprite btnSpr)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (btnSpr != null) { bg.sprite = btnSpr; bg.type = Image.Type.Sliced; }
            else { bg.sprite = uiSpr; bg.type = Image.Type.Sliced; }
            bg.color = LemonYellow;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.40f);
            shadow.effectDistance = new Vector2(0f, -10f);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic = bg;
            inputField.characterLimit = 2;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(16f, 6f); taR.offsetMax = new Vector2(-16f, -6f);

            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(taGO.transform, false);
            var phR = phGO.GetComponent<RectTransform>();
            phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
            phR.offsetMin = phR.offsetMax = Vector2.zero;
            var phTmp = phGO.AddComponent<TextMeshProUGUI>();
            phTmp.text = "??";
            phTmp.fontSize = 64f;
            phTmp.fontStyle = FontStyles.Bold;
            phTmp.color = new Color(0.20f, 0.10f, 0.02f, 0.30f);
            phTmp.alignment = TextAlignmentOptions.Center;
            phTmp.raycastTarget = false;
            if (font != null) phTmp.font = font;

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 64f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.color = TextPrimary;
            txtTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) txtTmp.font = font;

            inputField.textViewport  = taR;
            inputField.textComponent = txtTmp;
            inputField.placeholder   = phTmp;

            return inputField;
        }

        // ── 名前チップ ──────────────────────────────────────────────

        static TextMeshProUGUI MakeNameChip(Transform parent, string name,
            Vector2 pos, Color chipColor, Color textColor, TMP_FontAsset font, Sprite uiSpr, float h = 110f)
        {
            var chipGO = new GameObject(name, typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var cr = chipGO.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(860f, h);
            cr.anchoredPosition = pos;

            var bg = chipGO.AddComponent<Image>();
            bg.sprite = uiSpr; bg.type = Image.Type.Sliced;
            bg.color = chipColor; bg.raycastTarget = false;

            var sh = chipGO.AddComponent<Shadow>();
            sh.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.22f);
            sh.effectDistance = new Vector2(0f, -8f);

            var textGO = new GameObject("Name", typeof(RectTransform));
            textGO.transform.SetParent(chipGO.transform, false);
            var tr = textGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(20f, 6f); tr.offsetMax = new Vector2(-20f, -6f);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "プレイヤー名";
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.enableAutoSizing = true; tmp.fontSizeMin = 32f; tmp.fontSizeMax = 52f;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        // ── セパレーター ────────────────────────────────────────────

        static void MakeSeparator(Transform parent, float y)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = SepColor;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(880f, 2f);
            r.anchoredPosition = new Vector2(0f, y);
        }

        // ── レモン透かし ────────────────────────────────────────────

        static void BuildLemonPattern(Transform parent, Sprite lemonSprite, float alpha)
        {
            if (lemonSprite == null) return;
            var patternGO = new GameObject("LemonPattern", typeof(RectTransform));
            patternGO.transform.SetParent(parent, false);
            StretchFull(patternGO.GetComponent<RectTransform>());
            const float iconSize = 120f, colStep = 250f, rowStep = 220f, angle = -22f;
            for (int row = 0; row < 10; row++)
            {
                float y = 960f - row * rowStep;
                float xShift = (row % 2 == 0) ? 0f : colStep * 0.5f;
                for (int col = 0; col < 6; col++)
                {
                    float x = -625f + col * colStep + xShift;
                    var go = new GameObject($"L{row}_{col}", typeof(RectTransform));
                    go.transform.SetParent(patternGO.transform, false);
                    var r = go.GetComponent<RectTransform>();
                    r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.sizeDelta = new Vector2(iconSize, iconSize);
                    r.anchoredPosition = new Vector2(x, y);
                    r.localRotation = Quaternion.Euler(0f, 0f, angle);
                    var img = go.AddComponent<Image>();
                    img.sprite = lemonSprite; img.preserveAspect = true;
                    img.raycastTarget = false;
                    img.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        // ── HUD グループ ─────────────────────────────────────────────

        static void MakeHUDGroup(Transform parent, string name,
            float anchorMinX, float anchorMaxX, Sprite icon, TMP_FontAsset font,
            out TextMeshProUGUI countLabel)
        {
            var grpGO = new GameObject(name, typeof(RectTransform));
            grpGO.transform.SetParent(parent, false);
            var r = grpGO.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(anchorMinX, 0f); r.anchorMax = new Vector2(anchorMaxX, 1f);
            r.offsetMin = r.offsetMax = Vector2.zero;

            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(grpGO.transform, false);
            var ir = iconGO.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0f, 0.5f); ir.anchorMax = new Vector2(0f, 0.5f);
            ir.pivot = new Vector2(0f, 0.5f);
            ir.sizeDelta = new Vector2(56f, 56f); ir.anchoredPosition = Vector2.zero;
            if (icon != null)
            {
                var img = iconGO.AddComponent<Image>();
                img.sprite = icon; img.preserveAspect = true; img.raycastTarget = false;
            }

            var lblGO = new GameObject("Count", typeof(RectTransform));
            lblGO.transform.SetParent(grpGO.transform, false);
            var lr = lblGO.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(62f, 0f); lr.offsetMax = Vector2.zero;
            countLabel = lblGO.AddComponent<TextMeshProUGUI>();
            countLabel.text = "×8"; countLabel.fontSize = 42f; countLabel.fontStyle = FontStyles.Bold;
            countLabel.alignment = TextAlignmentOptions.MidlineLeft;
            countLabel.enableWordWrapping = false;
            countLabel.enableAutoSizing = true; countLabel.fontSizeMin = 32f; countLabel.fontSizeMax = 42f;
            countLabel.color = TextPrimary; countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

        // ── ヘルパー ──────────────────────────────────────────────────

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[GuessInputBuilder] Sprite not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spriteBorder = border;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font,
            float autoSizeMin = 0f, float autoSizeMax = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color; tmp.raycastTarget = false;
            if (autoSizeMin > 0f && autoSizeMax > 0f)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = autoSizeMin; tmp.fontSizeMax = autoSizeMax;
            }
            if (font != null) tmp.font = font;
            return tmp;
        }

        static TextMeshProUGUI MakeLabelInParent(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin; r.anchorMax = anchorMax;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.offsetMin = offsetMin; r.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = color; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        static GameObject MakeButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            Color bgColor, Color textColor, float fontSize, TMP_FontAsset font,
            Sprite btnSprite, Color? tint = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (btnSprite != null) { bg.sprite = btnSprite; bg.type = Image.Type.Sliced; }
            else { bg.sprite = GetBuiltinUISprite(); bg.type = Image.Type.Sliced; }
            bg.color = tint ?? bgColor;

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;

            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 16f); tr.offsetMax = new Vector2(-8f, -4f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false; tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return go;
        }

        static Sprite FindSprite(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Sprite {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
                if (s != null) return s;
            }
            var allGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains(keyword.ToLower()))
                {
                    var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null) return s;
                }
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
