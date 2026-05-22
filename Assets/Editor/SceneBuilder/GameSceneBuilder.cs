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

        // ── カラーパレット ──────────────────────────
        static readonly Color BgColor     = new(0.98f, 0.92f, 0.62f);        // レモンイエロー背景
        static readonly Color CardColor   = new(1f,    0.99f, 0.95f, 0.95f); // ウォームホワイトカード
        static readonly Color LemonYellow = new(0.97f, 0.83f, 0.18f);        // レモンイエロー（アクセント）
        static readonly Color BtnPrimary  = new(0.97f, 0.82f, 0.10f);        // ゴールデンイエロー（メインCTA）
        static readonly Color BtnSecondary= new(0.99f, 0.95f, 0.72f);        // ペールレモン（サブボタン）
        static readonly Color TextPrimary = new(0.20f, 0.10f, 0.02f);        // ダークブラウン
        static readonly Color TextMuted   = new(0.45f, 0.28f, 0.08f, 0.72f); // ミディアムブラウン
        static readonly Color ChipAlt     = new(0.99f, 0.95f, 0.72f);        // ペールレモン
        static readonly Color SepColor    = new(0.86f, 0.76f, 0.48f, 0.65f); // ゴールデンライン

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
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();

            // スプライト
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite = FindSprite("card");
            if (cardSprite == null)
            {
                var cardTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (cardTex != null)
                    cardSprite = Sprite.Create(cardTex, new Rect(0, 0, cardTex.width, cardTex.height), new Vector2(0.5f, 0.5f));
            }
            if (cardSprite == null) Debug.LogWarning("[GameBuilder] card sprite not found");

            // レモン透かし（クリーム背景に合わせ控えめに）
            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // TopicRuntimeDatabase
            new GameObject("TopicRuntimeDatabase").AddComponent<TopicRuntimeDatabase>();

            // Panel CanvasGroup（フェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── コンテンツカード ──
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

            // ── HUD 右上 ──
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(340f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);

            MakeHUDGroup(hudGO.transform, "LifeGroup",  0f,    0.47f, lemonSprite, jpFont, out var lifeLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup",  0.53f, 1f,    cardSprite,  jpFont, out var helpLabel);
            helpLabel.text = "×0";

            // ── お題ヘッダー（ゴールド）──
            MakeLabel(panelGO.transform, "TopicHeader",
                LanguageSettings.IsEnglish ? "TOPIC" : "お題",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 660f), new Vector2(900f, 56f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            // ── お題テキスト（大・白）──
            var topicGO = new GameObject("TopicText", typeof(RectTransform));
            topicGO.transform.SetParent(panelGO.transform, false);
            var topicR = topicGO.GetComponent<RectTransform>();
            topicR.anchorMin = topicR.anchorMax = new Vector2(0.5f, 0.5f);
            topicR.pivot = new Vector2(0.5f, 0.5f);
            topicR.sizeDelta = new Vector2(900f, 260f);
            topicR.anchoredPosition = new Vector2(0f, 476f);
            var topicTmp = topicGO.AddComponent<TextMeshProUGUI>();
            topicTmp.text = "お題テキスト";
            topicTmp.fontStyle = FontStyles.Bold;
            topicTmp.alignment = TextAlignmentOptions.Center;
            topicTmp.color = TextPrimary;
            topicTmp.enableAutoSizing = true;
            topicTmp.fontSizeMin = 36f; topicTmp.fontSizeMax = 64f;
            topicTmp.enableWordWrapping = false;
            topicTmp.overflowMode = TextOverflowModes.Overflow;
            topicTmp.raycastTarget = false;
            if (jpFont != null) topicTmp.font = jpFont;

            // ── Low / High ラベル ──
            var lowTmp = MakeLabel(panelGO.transform, "LowLabel", "1 = ○○",
                new Vector2(0.5f, 0.5f), new Vector2(-220f, 290f), new Vector2(420f, 56f),
                32f, TextMuted, FontStyles.Normal, jpFont);
            lowTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var highTmp = MakeLabel(panelGO.transform, "HighLabel", "99 = ○○",
                new Vector2(0.5f, 0.5f), new Vector2(220f, 290f), new Vector2(420f, 56f),
                32f, TextMuted, FontStyles.Normal, jpFont);
            highTmp.alignment = TextAlignmentOptions.MidlineRight;

            // ── グラデーションバー ──
            MakeGradientArrow(panelGO.transform, new Vector2(0f, 222f), jpFont);

            // ── セパレーター ──
            var divGO = new GameObject("Divider", typeof(RectTransform));
            divGO.transform.SetParent(panelGO.transform, false);
            divGO.AddComponent<Image>().color = SepColor;
            var divR = divGO.GetComponent<RectTransform>();
            divR.anchorMin = divR.anchorMax = new Vector2(0.5f, 0.5f);
            divR.pivot = new Vector2(0.5f, 0.5f);
            divR.sizeDelta = new Vector2(880f, 1f);
            divR.anchoredPosition = new Vector2(0f, 182f);

            // ── 回答プレイヤー ──
            MakeLabel(panelGO.transform, "AnswerHeader",
                LanguageSettings.IsEnglish ? "ANSWERER" : "回答プレイヤー",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            var answerNameLabel = MakePlayerChip(panelGO.transform, "AnswerChip",
                new Vector2(0f, -68f), ChipAlt, TextPrimary, jpFont);

            // ── お題変更ボタン ──
            var topicChangeBtnGO = MakeButton(panelGO.transform, "TopicChangeButton",
                LanguageSettings.IsEnglish ? "Change Topic" : "お題を変更",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -280f), new Vector2(700f, 90f),
                BtnSecondary, TextMuted, 38f, jpFont, btnYellow);

            // ── 数字確認ボタン ──
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton",
                LanguageSettings.IsEnglish ? "Confirm Number ▶" : "数字確認 ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -790f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 46f, jpFont, btnYellow);

            // ── Home ボタン（左上固定）──
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton",
                "HOME",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(200f, 80f),
                BtnSecondary, TextMuted, 34f, jpFont, btnYellow);

            // ── コントローラー ──
            var ctrlGO = new GameObject("GameTopicController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<GameTopicController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("topicLabel").objectReferenceValue      = topicTmp;
            so.FindProperty("topicLowLabel").objectReferenceValue   = lowTmp;
            so.FindProperty("topicHighLabel").objectReferenceValue  = highTmp;
            so.FindProperty("answerNameLabel").objectReferenceValue = answerNameLabel;
            so.FindProperty("lifeCountLabel").objectReferenceValue  = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("confirmButton").objectReferenceValue      = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("topicChangeButton").objectReferenceValue  = topicChangeBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue         = homeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue      = panelCG;

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

        // ── グラデーション矢印 ───────────────────────────────────────────

        static void MakeGradientArrow(Transform parent, Vector2 pos, TMP_FontAsset _)
        {
            const string texPath = "Assets/Sprites/UI/GradientBarTex.png";
            System.IO.Directory.CreateDirectory("Assets/Sprites/UI");
            int w = 128, h = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var leftCol  = new Color(0.82f, 0.88f, 0.96f, 0.80f);
            var rightCol = new Color(0.97f, 0.72f, 0.08f, 1.00f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, Color.Lerp(leftCol, rightCol, (float)x / (w - 1)));
            tex.Apply();
            System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath);
            var gradTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

            var go = new GameObject("GradientBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(880f, 20f);
            r.anchoredPosition = pos;

            if (gradTex != null)
            {
                var ri = go.AddComponent<RawImage>();
                ri.texture = gradTex;
                ri.raycastTarget = false;
            }
        }

        // ── レモン透かし ─────────────────────────────────────────────

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

        // ── プレイヤー名チップ ───────────────────────────────────────

        static TextMeshProUGUI MakePlayerChip(Transform parent, string name,
            Vector2 pos, Color chipColor, Color textColor, TMP_FontAsset font)
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
            shadow.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.22f);
            shadow.effectDistance = new Vector2(0f, -8f);

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
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (a is Sprite sp) return sp;
            }
            var allGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.ToLower().Contains(keyword.ToLower())) continue;
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (a is Sprite sp) return sp;
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
