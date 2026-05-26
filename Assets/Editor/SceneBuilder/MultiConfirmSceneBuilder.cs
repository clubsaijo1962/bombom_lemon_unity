using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Multiplayer;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// 協力モード：秘密の数字確認画面のシーンビルダー
    /// </summary>
    public static class MultiConfirmSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnDisabled  = new(0.80f, 0.80f, 0.80f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color TextWarning  = new(0.75f, 0.20f, 0.10f, 0.90f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color NumberBg     = new(0.97f, 0.82f, 0.10f);    // 黄色背景
        static readonly Color TopicBg      = new(0.98f, 0.96f, 0.85f, 0.90f); // 薄黄色背景

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = BgColor;
                camera.clearFlags      = CameraClearFlags.SolidColor;
                camera.orthographic    = true;
                camera.allowMSAA       = false;
            }

            var canvasGO = new GameObject("Canvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode    = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera   = camera;
            canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont      = FindJapaneseTMPFont();
            var btnYellow   = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite    = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.06f);

            // Panel (fade)
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── タイトル ──
            var titleTmp = MakeLabel(panelGO.transform, "Title", "秘密の数字を確認してください",
                new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(940f, 72f),
                52f, TextPrimary, FontStyles.Bold, jpFont);

            // ── コンテンツカード: 1020×1100, center y=-10 ──
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = CardColor; cardImg.raycastTarget = false;
            cardGO.AddComponent<Shadow>().effectColor    = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            cardGO.GetComponent<Shadow>().effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot     = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(1020f, 1100f);
            cardR.anchoredPosition = new Vector2(0f, -10f);
            // card local: top=550, bottom=-550

            // ── プレイヤー名 ──
            var playerNameTmp = MakeLabel(cardGO.transform, "PlayerName", "プレイヤー名",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 496f), new Vector2(860f, 48f),
                36f, TextMuted, FontStyles.Bold, jpFont);

            MakeSeparator(cardGO.transform, 457f);

            // ── お題セクション ──
            var topicHeaderTmp = MakeLabel(cardGO.transform, "TopicHeader", "お題",
                new Vector2(0.5f, 0.5f), new Vector2(-300f, 418f), new Vector2(200f, 44f),
                36f, TextMuted, FontStyles.Bold, jpFont);
            topicHeaderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // お題テキスト（薄黄背景カード）
            var topicBgGO = new GameObject("TopicBg", typeof(RectTransform));
            topicBgGO.transform.SetParent(cardGO.transform, false);
            var topicBgImg = topicBgGO.AddComponent<Image>();
            topicBgImg.sprite = uiSprite; topicBgImg.type = Image.Type.Sliced;
            topicBgImg.color = TopicBg; topicBgImg.raycastTarget = false;
            var topicBgR = topicBgGO.GetComponent<RectTransform>();
            topicBgR.anchorMin = topicBgR.anchorMax = new Vector2(0.5f, 0.5f);
            topicBgR.pivot     = new Vector2(0.5f, 0.5f);
            topicBgR.sizeDelta = new Vector2(900f, 130f);
            topicBgR.anchoredPosition = new Vector2(0f, 315f);

            var topicLblGO = new GameObject("TopicLabel", typeof(RectTransform));
            topicLblGO.transform.SetParent(topicBgGO.transform, false);
            var tlr = topicLblGO.GetComponent<RectTransform>();
            tlr.anchorMin = Vector2.zero; tlr.anchorMax = Vector2.one;
            tlr.offsetMin = new Vector2(20f, 8f); tlr.offsetMax = new Vector2(-20f, -8f);
            var topicTmp = topicLblGO.AddComponent<TextMeshProUGUI>();
            topicTmp.text = "今日の気分を点数で表すなら";
            topicTmp.fontSize = 44f; topicTmp.fontStyle = FontStyles.Bold;
            topicTmp.alignment = TextAlignmentOptions.Midline;
            topicTmp.color = TextPrimary; topicTmp.raycastTarget = false;
            topicTmp.enableWordWrapping = true;
            topicTmp.enableAutoSizing   = true;
            topicTmp.fontSizeMin = 32f; topicTmp.fontSizeMax = 44f;
            if (jpFont != null) topicTmp.font = jpFont;

            MakeSeparator(cardGO.transform, 237f);

            // ── 秘密の数字セクション ──
            var secretHeaderTmp = MakeLabel(cardGO.transform, "SecretHeader", "あなたの秘密の数字",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 196f), new Vector2(860f, 44f),
                36f, TextMuted, FontStyles.Bold, jpFont);

            // 数字表示（大きな黄色ボックス）
            var numBoxGO = new GameObject("NumberBox", typeof(RectTransform));
            numBoxGO.transform.SetParent(cardGO.transform, false);
            var numBoxImg = numBoxGO.AddComponent<Image>();
            numBoxImg.sprite = btnYellow; numBoxImg.type = Image.Type.Sliced;
            numBoxImg.color = NumberBg; numBoxImg.raycastTarget = false;
            numBoxGO.AddComponent<Shadow>().effectColor    = new Color(0.50f, 0.35f, 0f, 0.28f);
            numBoxGO.GetComponent<Shadow>().effectDistance = new Vector2(0f, -8f);
            var numBoxR = numBoxGO.GetComponent<RectTransform>();
            numBoxR.anchorMin = numBoxR.anchorMax = new Vector2(0.5f, 0.5f);
            numBoxR.pivot     = new Vector2(0.5f, 0.5f);
            numBoxR.sizeDelta = new Vector2(300f, 200f);
            numBoxR.anchoredPosition = new Vector2(0f, 40f);

            var numLblGO = new GameObject("NumberLabel", typeof(RectTransform));
            numLblGO.transform.SetParent(numBoxGO.transform, false);
            var nlr = numLblGO.GetComponent<RectTransform>();
            nlr.anchorMin = Vector2.zero; nlr.anchorMax = Vector2.one;
            nlr.offsetMin = nlr.offsetMax = Vector2.zero;
            var numTmp = numLblGO.AddComponent<TextMeshProUGUI>();
            numTmp.text = "47"; numTmp.fontSize = 120f; numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = TextPrimary; numTmp.raycastTarget = false;
            if (jpFont != null) numTmp.font = jpFont;

            // 警告テキスト
            var warningTmp = MakeLabel(cardGO.transform, "Warning",
                "⚠ 他のプレイヤーには見せないで！",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(860f, 64f),
                32f, TextWarning, FontStyles.Bold, jpFont);

            // 待機ラベル（確認後に表示）
            var waitingTmp = MakeLabel(cardGO.transform, "WaitingLabel",
                "他のプレイヤーを待っています...",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(860f, 44f),
                36f, TextMuted, FontStyles.Normal, jpFont);
            waitingTmp.gameObject.SetActive(false);

            // 確認済み人数ラベル（確認後に表示）
            var readyCountTmp = MakeLabel(cardGO.transform, "ReadyCount",
                "0 / ? 人 確認済み",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(860f, 48f),
                40f, TextPrimary, FontStyles.Bold, jpFont);
            readyCountTmp.gameObject.SetActive(false);

            // ── 確認ボタン（カード外・下部）──
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton", "確認しました ✓",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -670f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 48f, jpFont, btnYellow);

            // ── コントローラー ──
            var ctrlGO = new GameObject("MultiConfirmController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<MultiConfirmController>();
            var so   = new SerializedObject(ctrl);

            so.FindProperty("secretNumberLabel").objectReferenceValue = numTmp;
            so.FindProperty("topicLabel").objectReferenceValue        = topicTmp;
            so.FindProperty("playerNameLabel").objectReferenceValue   = playerNameTmp;
            so.FindProperty("titleLabel").objectReferenceValue        = titleTmp;
            so.FindProperty("topicHeaderLabel").objectReferenceValue  = topicHeaderTmp;
            so.FindProperty("secretHeaderLabel").objectReferenceValue = secretHeaderTmp;
            so.FindProperty("warningLabel").objectReferenceValue      = warningTmp;
            so.FindProperty("confirmButton").objectReferenceValue     = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("confirmBtnLabel").objectReferenceValue   =
                confirmBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("waitingLabel").objectReferenceValue      = waitingTmp;
            so.FindProperty("readyCountLabel").objectReferenceValue   = readyCountTmp;
            so.FindProperty("panelGroup").objectReferenceValue        = panelCG;

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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MultiConfirm.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/MultiConfirm.unity");
            Debug.Log("[MultiConfirmSceneBuilder] MultiConfirm シーンを作成しました");
        }

        // ── ユーティリティ ────────────────────────────────────────────

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

        static void BuildLemonPattern(Transform parent, Sprite lemon, float alpha)
        {
            if (lemon == null) return;
            var p = new GameObject("LemonPattern", typeof(RectTransform));
            p.transform.SetParent(parent, false);
            StretchFull(p.GetComponent<RectTransform>());
            for (int row = 0; row < 10; row++)
            {
                float y = 960f - row * 220f;
                float xs = (row % 2 == 0) ? 0f : 125f;
                for (int col = 0; col < 6; col++)
                {
                    var go = new GameObject($"L{row}_{col}", typeof(RectTransform));
                    go.transform.SetParent(p.transform, false);
                    var r = go.GetComponent<RectTransform>();
                    r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.sizeDelta = new Vector2(110f, 110f);
                    r.anchoredPosition = new Vector2(-625f + col * 250f + xs, y);
                    r.localRotation = Quaternion.Euler(0f, 0f, -22f);
                    var img = go.AddComponent<Image>();
                    img.sprite = lemon; img.preserveAspect = true; img.raycastTarget = false;
                    img.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        static void StretchFull(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

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
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot;
            r.sizeDelta = size; r.anchoredPosition = pos;
            var bg = go.AddComponent<Image>();
            bg.sprite = btnSprite ?? GetBuiltinUISprite(); bg.type = Image.Type.Sliced; bg.color = bgColor;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor     = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor    = new Color(0.75f, 0.75f, 0.75f, 1f);
            cols.disabledColor   = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;
            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 0f); tr.offsetMax = new Vector2(-8f, 0f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false; tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return go;
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[MultiConfirmBuilder] Not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            { ti.spriteImportMode = SpriteImportMode.Single; ti.spriteBorder = border; ti.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            return null;
        }

        static TMP_FontAsset FindJapaneseTMPFont()
        {
            string[] c = { "NotoSansJP", "NotoSans", "Noto", "Meiryo", "YuGothic", "Japanese", "JP" };
            foreach (var kw in c)
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
