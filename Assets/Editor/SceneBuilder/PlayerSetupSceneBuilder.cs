using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class PlayerSetupSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnSecondary = new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 1f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);

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

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = BgColor;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var jpFont = FindJapaneseTMPFont();
            var lemonTex    = FindTexture("Title_Lemon");
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png");
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            BuildLemonPattern(canvasGO.transform, lemonSprite);

            // Panel CanvasGroup（フェードイン用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // ── 戻るボタン（左上）──
            var backBtnGO = MakeButton(panelGO.transform, "BackButton", "← 戻る",
                new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(54f, -152f), new Vector2(220f, 88f),
                BtnSecondary, TextMuted, 34f, jpFont, btnYellow);

            // ── ヘッダー ──
            var header = MakeLabel(panelGO.transform, "Header", "どうやって遊ぶ？",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 620f), new Vector2(900f, 90f),
                56f, new Color(0.26f, 0.10f, 0.01f, 1f), FontStyles.Bold, jpFont);

            // ── レモン装飾（ヘッダー両脇）──
            if (lemonTex != null)
            {
                float lh = 110f;
                float lr = (float)lemonTex.width / lemonTex.height;
                AddDecoration(panelGO.transform, "LemonL", lemonTex, lh * lr, lh, new Vector2(-390f, 620f), -18f);
                AddDecoration(panelGO.transform, "LemonR", lemonTex, lh * lr, lh, new Vector2(390f, 620f), 18f);
            }

            // ── このスマホ１台で遊ぶ（大ボタン）──
            var localBtnGO = MakeButton(panelGO.transform, "LocalPlayButton", "このスマホ１台で遊ぶ",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 200f), new Vector2(900f, 150f),
                BtnPrimary, TextPrimary, 46f, jpFont, btnYellow);

            // ── または ──
            var orLabel = MakeLabel(panelGO.transform, "OrLabel", "― または ―",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(600f, 60f),
                34f, new Color(0.40f, 0.22f, 0.06f, 0.70f), FontStyles.Normal, jpFont);

            // ── 部屋ボタン2つ ──
            var createBtnGO = MakeButton(panelGO.transform, "CreateRoomButton", "部屋を立てる",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-230f, -160f), new Vector2(430f, 120f),
                BtnSecondary, TextMuted, 40f, jpFont, btnYellow);

            var joinBtnGO = MakeButton(panelGO.transform, "JoinRoomButton", "部屋に入る",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(230f, -160f), new Vector2(430f, 120f),
                BtnSecondary, TextMuted, 40f, jpFont, btnYellow);

            // ── 区切り線 ──
            var divGO = new GameObject("Divider", typeof(RectTransform));
            divGO.transform.SetParent(panelGO.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = SepColor;
            divImg.raycastTarget = false;
            var divRect = divGO.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(860f, 2f);
            divRect.anchoredPosition = new Vector2(0f, -320f);

            // ── ガイドテキスト ──
            var guide = MakeLabel(panelGO.transform, "GuideLabel",
                "部屋に集まって遊ぶ場合は\nそれぞれのスマホにボムボムレモンが\nインストールされている必要があります",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -510f), new Vector2(900f, 220f),
                32f, new Color(0.38f, 0.20f, 0.05f, 0.85f), FontStyles.Normal, jpFont);
            guide.alignment = TextAlignmentOptions.Center;
            guide.lineSpacing = 8f;

            // ── レモン装飾（下部）──
            if (lemonTex != null)
            {
                float lh = 180f;
                float lr = (float)lemonTex.width / lemonTex.height;
                AddDecoration(panelGO.transform, "LemonBL", lemonTex, lh * lr, lh, new Vector2(-380f, -760f), -25f);
                AddDecoration(panelGO.transform, "LemonBR", lemonTex, lh * lr, lh, new Vector2(380f, -760f), 25f);
            }

            // ── GameModeSelectController ──
            var ctrlGO = new GameObject("GameModeSelectController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<GameModeSelectController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("localPlayButton").objectReferenceValue  = localBtnGO.GetComponent<Button>();
            so.FindProperty("createRoomButton").objectReferenceValue = createBtnGO.GetComponent<Button>();
            so.FindProperty("joinRoomButton").objectReferenceValue   = joinBtnGO.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue       = backBtnGO.GetComponent<Button>();
            so.FindProperty("headerLabel").objectReferenceValue      = header;
            so.FindProperty("localPlayLabel").objectReferenceValue   = localBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("createRoomLabel").objectReferenceValue  = createBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("joinRoomLabel").objectReferenceValue    = joinBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("orLabel").objectReferenceValue          = orLabel;
            so.FindProperty("guideLabel").objectReferenceValue       = guide;
            so.FindProperty("backLabel").objectReferenceValue        = backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("panelGroup").objectReferenceValue       = panelCG;
            so.FindProperty("localSceneName").stringValue            = "SingleSettings";
            so.FindProperty("titleSceneName").stringValue            = "Title";
            so.ApplyModifiedProperties();

            // HellModeColorApplier（地獄モード時の配色変更）
            var hellGO = new GameObject("HellModeColorApplier");
            hellGO.transform.SetParent(canvasGO.transform, false);
            var hellApplier = hellGO.AddComponent<HellModeColorApplier>();
            var hellSO = new SerializedObject(hellApplier);
            hellSO.FindProperty("lemonPatternRoot").objectReferenceValue = canvasGO.transform.Find("LemonPattern");
            var limeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/lime.png");
            if (limeSprite != null) hellSO.FindProperty("limeSprite").objectReferenceValue = limeSprite;
            hellSO.ApplyModifiedProperties();

            // 全画面フェードオーバーレイ（最前面）
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            var sfRect2 = sfGO.GetComponent<RectTransform>();
            sfRect2.anchorMin = Vector2.zero; sfRect2.anchorMax = Vector2.one;
            sfRect2.offsetMin = Vector2.zero; sfRect2.offsetMax = Vector2.zero;
            var sfImg = sfGO.AddComponent<Image>();
            sfImg.color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f;
            sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/PlayerSetup.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/PlayerSetup.unity", 2);

            Debug.Log("[PlayerSetupSceneBuilder] PlayerSetup シーンを作成しました → Assets/Scenes/PlayerSetup.unity");
        }

        // ── ボタン作成ヘルパー ──
        static GameObject MakeButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 pivot,
            Vector2 pos, Vector2 size,
            Color bgColor, Color textColor,
            float fontSize, TMP_FontAsset font, Sprite pill)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMin;
            r.pivot = pivot;
            r.sizeDelta = size;
            r.anchoredPosition = pos;

            // シャドウ
            var shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(go.transform, false);
            var sr = shadowGO.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(2f, -8f); sr.offsetMax = new Vector2(-2f, -1f);
            var si = shadowGO.AddComponent<Image>();
            si.sprite = pill; si.type = Image.Type.Sliced;
            si.color = new Color(0.40f, 0.22f, 0.04f, 0.20f);
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
            btn.colors = cols;
            btn.targetGraphic = bg;

            var txtGO = new GameObject("Label", typeof(RectTransform));
            txtGO.transform.SetParent(go.transform, false);
            var tr = txtGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(12f, 0f); tr.offsetMax = new Vector2(-12f, 0f);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.characterSpacing = 2f;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            if (font != null) tmp.font = font;

            return go;
        }

        // ── ラベル作成ヘルパー ──
        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.characterSpacing = 2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        // ── レモン透かしパターン ──
        static void BuildLemonPattern(Transform parent, Sprite lemonSprite)
        {
            if (lemonSprite == null) return;
            var patternGO = new GameObject("LemonPattern", typeof(RectTransform));
            patternGO.transform.SetParent(parent, false);
            var pr = patternGO.GetComponent<RectTransform>();
            pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
            pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;
            const float iconSize = 120f, colStep = 250f, rowStep = 220f, angle = -22f, alpha = 0.07f;
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

        // ── レモン装飾 ──
        static void AddDecoration(Transform parent, string name, Texture2D tex,
            float w, float h, Vector2 pos, float rotateDeg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(w, h);
            r.anchoredPosition = pos;
            r.localRotation = Quaternion.Euler(0f, 0f, rotateDeg);
            var raw = go.AddComponent<RawImage>();
            raw.texture = tex;
            raw.raycastTarget = false;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0.55f;
            cg.blocksRaycasts = false;
        }

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[PlayerSetupBuilder] Sprite not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spriteBorder = border;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

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
    }
}
