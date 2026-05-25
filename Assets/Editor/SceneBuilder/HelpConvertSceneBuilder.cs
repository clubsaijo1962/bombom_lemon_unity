using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class HelpConvertSceneBuilder
    {
        const string CP   = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor     = new(0.98f, 0.92f, 0.62f);
        static readonly Color BtnPrimary  = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnSecondary= new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted   = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color CardColor   = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color LifeChip    = new(0.38f, 0.55f, 0.92f);

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

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont     = FindJapaneseTMPFont();
            var uiSprite   = GetBuiltinUISprite();
            var pillSprite = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png");

            var cardSprite = FindSprite("card");
            if (cardSprite == null)
            {
                var ct = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (ct != null) cardSprite = Sprite.Create(ct, new Rect(0,0,ct.width,ct.height), new Vector2(0.5f,0.5f));
            }

            // レモン透かし
            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // Panel
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // HUD 右上
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(340f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);
            TextMeshProUGUI lifeCountLabel, helpCardCountLabel;
            MakeHUDGroup(hudGO.transform, "LifeGroup", 0f,    0.47f, lemonSprite, jpFont, out lifeCountLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup", 0.53f, 1f,    cardSprite,  jpFont, out helpCardCountLabel);

            // HOME ボタン
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton", "HOME",
                new Vector2(0f,1f), new Vector2(0f,1f),
                new Vector2(24f,-104f), new Vector2(200f,80f),
                BtnSecondary, TextMuted, 34f, jpFont, pillSprite);

            // ラウンド表示（上部中央）
            var roundLabelGO = new GameObject("RoundLabel", typeof(RectTransform));
            roundLabelGO.transform.SetParent(panelGO.transform, false);
            {
                var rlR = roundLabelGO.GetComponent<RectTransform>();
                rlR.anchorMin = rlR.anchorMax = new Vector2(0.5f, 1f);
                rlR.pivot = new Vector2(0.5f, 0.5f);
                rlR.sizeDelta = new Vector2(300f, 60f);
                rlR.anchoredPosition = new Vector2(-65f, -144f);
            }
            var roundLabelTmp = roundLabelGO.AddComponent<TextMeshProUGUI>();
            roundLabelTmp.text = "−/−ラウンド";
            roundLabelTmp.fontStyle = FontStyles.Bold;
            roundLabelTmp.alignment = TextAlignmentOptions.Center;
            roundLabelTmp.color = TextPrimary;
            roundLabelTmp.enableAutoSizing = true;
            roundLabelTmp.fontSizeMin = 32f; roundLabelTmp.fontSizeMax = 36f;
            roundLabelTmp.enableWordWrapping = false;
            roundLabelTmp.raycastTarget = false;
            if (jpFont != null) roundLabelTmp.font = jpFont;

            // タイトル
            MakeLabel(panelGO.transform, "Title", "次が最終ラウンド\nヘルプカードは使えません",
                new Vector2(0.5f,0.5f), new Vector2(0f, 690f), new Vector2(880f,140f),
                44f, TextPrimary, FontStyles.Bold, jpFont);

            // コンテンツカード
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = CardColor; cardImg.raycastTarget = false;
            var sh = cardGO.AddComponent<Shadow>();
            sh.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            sh.effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(960f, 900f);
            cardR.anchoredPosition = new Vector2(0f, 20f);

            // 説明文
            MakeLabel(cardGO.transform, "Desc",
                "残りのヘルプカードを\nライフに変換します",
                new Vector2(0.5f,0.5f), new Vector2(0f, 300f), new Vector2(860f, 140f),
                40f, TextPrimary, FontStyles.Normal, jpFont);

            TextMeshProUGUI cardCountLbl = null, gainLbl = null;

            // ヘルプカードアイコン＋枚数ラベル（横並び）
            {
                var rowGO = new GameObject("CardRow", typeof(RectTransform));
                rowGO.transform.SetParent(cardGO.transform, false);
                var rr = rowGO.GetComponent<RectTransform>();
                rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
                rr.pivot = new Vector2(0.5f, 0.5f);
                rr.sizeDelta = new Vector2(300f, 110f);
                rr.anchoredPosition = new Vector2(0f, 130f);

                if (cardSprite != null)
                {
                    var ciGO = new GameObject("CardIcon", typeof(RectTransform));
                    ciGO.transform.SetParent(rowGO.transform, false);
                    var ciR = ciGO.GetComponent<RectTransform>();
                    ciR.anchorMin = ciR.anchorMax = new Vector2(0f, 0.5f);
                    ciR.pivot = new Vector2(0f, 0.5f);
                    ciR.sizeDelta = new Vector2(90f, 90f);
                    ciR.anchoredPosition = Vector2.zero;
                    var ciImg = ciGO.AddComponent<Image>();
                    ciImg.sprite = cardSprite; ciImg.preserveAspect = true; ciImg.raycastTarget = false;
                }

                var cntGO = new GameObject("CountLabel", typeof(RectTransform));
                cntGO.transform.SetParent(rowGO.transform, false);
                var cntR = cntGO.GetComponent<RectTransform>();
                cntR.anchorMin = new Vector2(0f, 0f); cntR.anchorMax = new Vector2(1f, 1f);
                cntR.offsetMin = new Vector2(100f, 0f); cntR.offsetMax = Vector2.zero;
                var cntTmp = cntGO.AddComponent<TextMeshProUGUI>();
                cntTmp.text = "× 2枚"; cntTmp.fontStyle = FontStyles.Bold;
                cntTmp.alignment = TextAlignmentOptions.MidlineLeft;
                cntTmp.enableAutoSizing = true; cntTmp.fontSizeMin = 48f; cntTmp.fontSizeMax = 72f;
                cntTmp.color = LifeChip; cntTmp.enableWordWrapping = false; cntTmp.raycastTarget = false;
                if (jpFont != null) cntTmp.font = jpFont;
                cardCountLbl = cntTmp;
            }

            // レモンアイコン＋ゲインラベル（横並び）
            {
                var rowGO = new GameObject("GainRow", typeof(RectTransform));
                rowGO.transform.SetParent(cardGO.transform, false);
                var rr = rowGO.GetComponent<RectTransform>();
                rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
                rr.pivot = new Vector2(0.5f, 0.5f);
                rr.sizeDelta = new Vector2(300f, 90f);
                rr.anchoredPosition = new Vector2(0f, -20f);

                if (lemonSprite != null)
                {
                    var glGO = new GameObject("LemonIcon", typeof(RectTransform));
                    glGO.transform.SetParent(rowGO.transform, false);
                    var glR = glGO.GetComponent<RectTransform>();
                    glR.anchorMin = glR.anchorMax = new Vector2(0f, 0.5f);
                    glR.pivot = new Vector2(0f, 0.5f);
                    glR.sizeDelta = new Vector2(72f, 72f);
                    glR.anchoredPosition = Vector2.zero;
                    var glImg = glGO.AddComponent<Image>();
                    glImg.sprite = lemonSprite; glImg.preserveAspect = true; glImg.raycastTarget = false;
                }

                var glblGO = new GameObject("GainLabel", typeof(RectTransform));
                glblGO.transform.SetParent(rowGO.transform, false);
                var glblR = glblGO.GetComponent<RectTransform>();
                glblR.anchorMin = new Vector2(0f, 0f); glblR.anchorMax = new Vector2(1f, 1f);
                glblR.offsetMin = new Vector2(82f, 0f); glblR.offsetMax = Vector2.zero;
                var gainTmp = glblGO.AddComponent<TextMeshProUGUI>();
                gainTmp.text = "ライフ +2"; gainTmp.fontStyle = FontStyles.Bold;
                gainTmp.alignment = TextAlignmentOptions.MidlineLeft;
                gainTmp.enableAutoSizing = true; gainTmp.fontSizeMin = 44f; gainTmp.fontSizeMax = 56f;
                gainTmp.color = new Color(0.18f, 0.52f, 0.18f);
                gainTmp.enableWordWrapping = false; gainTmp.raycastTarget = false;
                if (jpFont != null) gainTmp.font = jpFont;
                gainLbl = gainTmp;
            }

            // 残りライフ
            var lifeAfterLbl = MakeLabel(cardGO.transform, "LifeAfter", "残りライフ: 10",
                new Vector2(0.5f,0.5f), new Vector2(0f, -160f), new Vector2(860f, 80f),
                48f, TextPrimary, FontStyles.Bold, jpFont);

            // 次へボタン（初期非表示）
            var nextBtnGO = MakeButton(panelGO.transform, "ContinueButton", "最終ラウンドへ ▶",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(0f,-790f), new Vector2(900f,118f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // Controller
            var ctrlGO = new GameObject("HelpConvertController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<HelpConvertController>();
            var so   = new SerializedObject(ctrl);
            so.FindProperty("cardCountLabel").objectReferenceValue    = cardCountLbl;
            so.FindProperty("gainLabel").objectReferenceValue         = gainLbl;
            so.FindProperty("lifeAfterLabel").objectReferenceValue    = lifeAfterLbl;
            so.FindProperty("lifeCountLabel").objectReferenceValue    = lifeCountLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue= helpCardCountLabel;
            so.FindProperty("roundLabel").objectReferenceValue        = roundLabelTmp;
            so.FindProperty("continueButton").objectReferenceValue    = nextBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue        = homeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue        = panelCG;

            // HellModeColorApplier（地獄モード時の配色変更）
            var hellGO = new GameObject("HellModeColorApplier");
            hellGO.transform.SetParent(canvasGO.transform, false);
            var hellApplier = hellGO.AddComponent<HellModeColorApplier>();
            var hellSO = new SerializedObject(hellApplier);
            hellSO.FindProperty("lemonPatternRoot").objectReferenceValue = canvasGO.transform.Find("LemonPattern");
            var limeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/lime.png");
            if (limeSprite != null) hellSO.FindProperty("limeSprite").objectReferenceValue = limeSprite;
            hellSO.ApplyModifiedProperties();

            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/HelpConvert.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/HelpConvert.unity");
            Debug.Log("[HelpConvertSceneBuilder] HelpConvert シーンを作成しました");
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

        static void MakeHUDGroup(Transform parent, string name, float xMin, float xMax,
            Sprite icon, TMP_FontAsset font, out TextMeshProUGUI countLabel)
        {
            var grp = new GameObject(name, typeof(RectTransform));
            grp.transform.SetParent(parent, false);
            var r = grp.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(xMin, 0f); r.anchorMax = new Vector2(xMax, 1f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(grp.transform, false);
            var ir = iconGO.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0f, 0.5f); ir.anchorMax = new Vector2(0f, 0.5f);
            ir.pivot = new Vector2(0f, 0.5f);
            ir.sizeDelta = new Vector2(52f, 52f); ir.anchoredPosition = Vector2.zero;
            if (icon != null) { var img = iconGO.AddComponent<Image>(); img.sprite = icon; img.preserveAspect = true; img.raycastTarget = false; }
            var lbl = new GameObject("Count", typeof(RectTransform));
            lbl.transform.SetParent(grp.transform, false);
            var lr = lbl.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(58f, 0f); lr.offsetMax = Vector2.zero;
            countLabel = lbl.AddComponent<TextMeshProUGUI>();
            countLabel.fontSize = 42f; countLabel.fontStyle = FontStyles.Bold;
            countLabel.alignment = TextAlignmentOptions.MidlineLeft;
            countLabel.enableWordWrapping = false;
            countLabel.enableAutoSizing = true; countLabel.fontSizeMin = 32f; countLabel.fontSizeMax = 42f;
            countLabel.color = new Color(0.20f, 0.10f, 0.02f); countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

        static void StretchFull(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static Sprite GetBuiltinUISprite() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, Color color, FontStyles style,
            TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center; tmp.color = color; tmp.raycastTarget = false;
            tmp.lineSpacing = 4f;
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
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;
            var bg = go.AddComponent<Image>();
            bg.sprite = btnSprite ?? GetBuiltinUISprite(); bg.type = Image.Type.Sliced; bg.color = bgColor;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white; cols.highlightedColor = new Color(1f,1f,0.85f,1f);
            cols.pressedColor = new Color(0.75f,0.75f,0.75f,1f); cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;
            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f,0f); tr.offsetMax = new Vector2(-8f,0f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false; tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center; tmp.color = textColor; tmp.raycastTarget = false;
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
            return null;
        }

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[HelpConvertBuilder] Not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            { ti.spriteImportMode = SpriteImportMode.Single; ti.spriteBorder = border; ti.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
