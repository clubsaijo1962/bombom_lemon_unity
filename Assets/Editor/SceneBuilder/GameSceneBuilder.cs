using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;
using BomBomLemon.Game.Topics;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class GameSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.98f, 0.91f, 0.58f);
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
            bgGO.AddComponent<Image>().color = new Color(0.98f, 0.91f, 0.58f);
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();

            // スプライト
            var btnCyan   = LoadSliced(CP + "mini_btn_cyan.png",   PillL, PillB, PillR, PillT);
            var uiSprite  = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite  = FindSprite("card");
            if (cardSprite == null)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (tex != null)
                    cardSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            // レモン透かし
            BuildLemonPattern(canvasGO.transform, lemonSprite);

            // TopicRuntimeDatabase（DontDestroyOnLoadで引き継がれるがシーン単独起動用に配置）
            // Canvasの子にしない（RectTransform不要のサービスオブジェクト）
            new GameObject("TopicRuntimeDatabase").AddComponent<TopicRuntimeDatabase>();

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
                img.color = new Color(1f, 0.99f, 0.95f, 0.92f);
                img.raycastTarget = false;
                var sh = cardGO.AddComponent<Shadow>();
                sh.effectColor = new Color(0.20f, 0.10f, 0f, 0.18f);
                sh.effectDistance = new Vector2(0f, -10f);
                var cr = cardGO.GetComponent<RectTransform>();
                cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
                cr.pivot     = new Vector2(0.5f, 0.5f);
                cr.sizeDelta = new Vector2(1020f, 1260f);
                cr.anchoredPosition = new Vector2(0f, 90f);
            }

            // ── HUD 右上 ──
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot     = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(340f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);

            MakeHUDGroup(hudGO.transform, "LifeGroup", 0f, 0.47f, lemonSprite, jpFont,
                out var lifeLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup", 0.53f, 1f, cardSprite, jpFont,
                out var helpLabel);
            helpLabel.text = "×0";

            // ── お題セクションヘッダー ──
            MakeLabel(panelGO.transform, "TopicHeader",
                LanguageSettings.IsEnglish ? "TOPIC" : "お題",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 656f), new Vector2(900f, 50f),
                30f, new Color(0.45f, 0.20f, 0.04f, 0.70f), FontStyles.Bold, jpFont);

            // ── お題テキスト ──
            var go = new GameObject("TopicText", typeof(RectTransform));
            go.transform.SetParent(panelGO.transform, false);
            var topicR = go.GetComponent<RectTransform>();
            topicR.anchorMin = topicR.anchorMax = new Vector2(0.5f, 0.5f);
            topicR.pivot = new Vector2(0.5f, 0.5f);
            topicR.sizeDelta = new Vector2(900f, 260f);
            topicR.anchoredPosition = new Vector2(0f, 476f);
            var topicTmp = go.AddComponent<TextMeshProUGUI>();
            topicTmp.text = "お題テキスト";
            topicTmp.fontStyle = FontStyles.Bold;
            topicTmp.alignment = TextAlignmentOptions.Center;
            topicTmp.color = new Color(0.18f, 0.06f, 0.01f, 1f);
            topicTmp.enableAutoSizing = true;
            topicTmp.fontSizeMin = 36f; topicTmp.fontSizeMax = 64f;
            topicTmp.raycastTarget = false;
            if (jpFont != null) topicTmp.font = jpFont;

            // LowLabel / HighLabel
            var lowTmp = MakeLabel(panelGO.transform, "LowLabel", "1 = ○○",
                new Vector2(0.5f, 0.5f), new Vector2(-250f, 294f), new Vector2(400f, 50f),
                26f, new Color(0.35f, 0.16f, 0.04f, 0.80f), FontStyles.Normal, jpFont);
            lowTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var highTmp = MakeLabel(panelGO.transform, "HighLabel", "99 = ○○",
                new Vector2(0.5f, 0.5f), new Vector2(250f, 294f), new Vector2(400f, 50f),
                26f, new Color(0.35f, 0.16f, 0.04f, 0.80f), FontStyles.Normal, jpFont);
            highTmp.alignment = TextAlignmentOptions.MidlineRight;

            // セパレーター
            var divGO = new GameObject("Divider", typeof(RectTransform));
            divGO.transform.SetParent(panelGO.transform, false);
            divGO.AddComponent<Image>().color = new Color(0.60f, 0.38f, 0.08f, 0.22f);
            var divR = divGO.GetComponent<RectTransform>();
            divR.anchorMin = divR.anchorMax = new Vector2(0.5f, 0.5f);
            divR.pivot = new Vector2(0.5f, 0.5f);
            divR.sizeDelta = new Vector2(900f, 2f);
            divR.anchoredPosition = new Vector2(0f, 224f);

            // ── ガイド ──
            MakeLabel(panelGO.transform, "GuideHeader",
                LanguageSettings.IsEnglish ? "GUIDE" : "ガイド",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 164f), new Vector2(900f, 44f),
                28f, new Color(0.40f, 0.18f, 0.04f, 0.72f), FontStyles.Bold, jpFont);

            var guideNameLabel = MakePlayerChip(panelGO.transform, "GuideChip",
                new Vector2(0f, 52f), new Color(0.98f, 0.78f, 0.22f, 1f), jpFont);

            // ── 回答プレイヤー ──
            MakeLabel(panelGO.transform, "AnswerHeader",
                LanguageSettings.IsEnglish ? "ANSWERER" : "回答プレイヤー",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(900f, 44f),
                28f, new Color(0.40f, 0.18f, 0.04f, 0.72f), FontStyles.Bold, jpFont);

            var answerNameLabel = MakePlayerChip(panelGO.transform, "AnswerChip",
                new Vector2(0f, -212f), new Color(0.36f, 0.62f, 0.88f, 1f), jpFont);

            // ── 数字確認ボタン ──
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton",
                LanguageSettings.IsEnglish ? "Confirm Number ▶" : "数字確認 ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -790f), new Vector2(900f, 118f),
                Color.white, new Color(0.06f, 0.28f, 0.32f, 1f), 46f, jpFont, btnCyan);

            // ── コントローラー ──
            var ctrlGO = new GameObject("GameTopicController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<GameTopicController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("topicLabel").objectReferenceValue      = topicTmp;
            so.FindProperty("topicLowLabel").objectReferenceValue   = lowTmp;
            so.FindProperty("topicHighLabel").objectReferenceValue  = highTmp;
            so.FindProperty("guideNameLabel").objectReferenceValue  = guideNameLabel;
            so.FindProperty("answerNameLabel").objectReferenceValue = answerNameLabel;
            so.FindProperty("lifeCountLabel").objectReferenceValue  = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("confirmButton").objectReferenceValue   = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue      = panelCG;

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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Game.unity");

            Debug.Log("[GameSceneBuilder] Game シーンを作成しました");
        }

        // ── レモン透かし ─────────────────────────────────────────────

        static void BuildLemonPattern(Transform parent, Sprite lemonSprite)
        {
            if (lemonSprite == null) return;
            var patternGO = new GameObject("LemonPattern", typeof(RectTransform));
            patternGO.transform.SetParent(parent, false);
            StretchFull(patternGO.GetComponent<RectTransform>());
            const float iconSize = 120f, colStep = 250f, rowStep = 220f, angle = -22f, alpha = 0.12f;
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

        // ── プレイヤー名チップ ───────────────────────────────────────

        static TextMeshProUGUI MakePlayerChip(Transform parent, string name,
            Vector2 pos, Color chipColor, TMP_FontAsset font)
        {
            var chipGO = new GameObject(name, typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var cr = chipGO.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(800f, 110f);
            cr.anchoredPosition = pos;

            var bg = chipGO.AddComponent<Image>();
            bg.sprite = GetBuiltinUISprite(); bg.type = Image.Type.Sliced;
            bg.color = chipColor; bg.raycastTarget = false;

            var shadow = chipGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.14f);
            shadow.effectDistance = new Vector2(0f, -6f);

            var textGO = new GameObject("Name", typeof(RectTransform));
            textGO.transform.SetParent(chipGO.transform, false);
            var tr = textGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(20f, 6f); tr.offsetMax = new Vector2(-20f, -6f);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "プレイヤー名";
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true; tmp.fontSizeMin = 28f; tmp.fontSizeMax = 48f;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
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
            countLabel.enableAutoSizing = true; countLabel.fontSizeMin = 24f; countLabel.fontSizeMax = 42f;
            countLabel.color = new Color(0.20f, 0.09f, 0.01f, 1f); countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

        // ── ヘルパー ──────────────────────────────────────────────────

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[GameBuilder] Sprite not found: {path}"); return null; }
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
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
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
            if (font != null) tmp.font = font;
            return tmp;
        }

        static GameObject MakeButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            Color bgColor, Color textColor, float fontSize, TMP_FontAsset font, Sprite btnSprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (btnSprite != null) { bg.sprite = btnSprite; bg.type = Image.Type.Sliced; }
            else { bg.sprite = GetBuiltinUISprite(); bg.type = Image.Type.Sliced; }
            bg.color = bgColor;

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor = new Color(0.80f, 0.80f, 0.80f, 1f);
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
