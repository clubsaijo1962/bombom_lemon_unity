using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class RoomSetupSceneBuilder
    {
        const string CP   = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor       = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor     = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary    = new(0.97f, 0.82f, 0.10f);
        static readonly Color TextPrimary   = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted     = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color ErrorColor    = new(0.80f, 0.10f, 0.10f);
        static readonly Color SepColor      = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color ModeBgDefault = new(0.93f, 0.93f, 0.95f);
        static readonly Color ModeTextDefault = new(0.35f, 0.35f, 0.40f, 0.85f);

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

            var jpFont    = FindJapaneseTMPFont();
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite  = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // Panel (fade)
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── 戻るボタン（左上）──
            var backBtnGO = MakeButton(panelGO.transform, "BackButton", "← 戻る",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(220f, 80f),
                new Color(0.99f, 0.95f, 0.72f), TextMuted, 34f, jpFont, btnYellow);

            // ── タイトル ──
            var titleTmp = MakeLabel(panelGO.transform, "Title", "部屋を立てる",
                new Vector2(0.5f, 1f), new Vector2(0f, -197f), new Vector2(800f, 80f),
                56f, TextPrimary, FontStyles.Bold, jpFont);

            // ── コンテンツカード ──
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = CardColor; cardImg.raycastTarget = false;
            var cardSh = cardGO.AddComponent<Shadow>();
            cardSh.effectColor    = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            cardSh.effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot     = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(1020f, 1160f);
            cardR.anchoredPosition = new Vector2(0f, -15f);

            // ── PIN セクション ──
            var pinHeaderTmp = MakeLabel(cardGO.transform, "PinHeader", "暗証番号（6桁）",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 367f), new Vector2(860f, 52f),
                36f, TextMuted, FontStyles.Bold, jpFont);

            var pinField = MakePinInputField(cardGO.transform, "PinInputField",
                new Vector2(0f, 257f), new Vector2(640f, 130f), jpFont, btnYellow);

            // PINエラーラベル
            var pinErrorTmp = MakeLabel(cardGO.transform, "PinErrorLabel", "6桁の数字を入力してください",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 175f), new Vector2(780f, 44f),
                32f, ErrorColor, FontStyles.Normal, jpFont);

            // セパレーター
            MakeSeparator(cardGO.transform, 117f);

            // ── ゲームモード セクション ──
            var modeHeaderTmp = MakeLabel(cardGO.transform, "ModeHeader", "ゲームモード",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 57f), new Vector2(860f, 52f),
                36f, TextMuted, FontStyles.Bold, jpFont);

            // CoopLife ボタン（デフォルト: 薄いグレー。Start()でSelectMode(CoopLife)が黄色にする）
            TextMeshProUGUI coopLifeLbl, coopLifeDescLbl;
            var coopLifeBtnGO = MakeModeButton(cardGO.transform, "CoopLifeButton",
                "協力モード", "チームみんなでライフを守る協力ゲーム",
                new Vector2(0f, -73f), new Vector2(960f, 180f),
                ModeBgDefault, ModeTextDefault, jpFont, uiSprite, btnYellow,
                out coopLifeLbl, out coopLifeDescLbl);

            // TeamBattle ボタン（デフォルト: 薄いグレー）
            TextMeshProUGUI teamBattleLbl, teamBattleDescLbl;
            var teamBattleBtnGO = MakeModeButton(cardGO.transform, "TeamBattleButton",
                "チームバトル", "2チームに分かれて差の合計が少ない方が勝ち",
                new Vector2(0f, -303f), new Vector2(960f, 180f),
                ModeBgDefault, ModeTextDefault, jpFont, uiSprite, btnYellow,
                out teamBattleLbl, out teamBattleDescLbl);

            // ── 確定ボタン（カード外・下部）──
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton", "確定する ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -690f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 48f, jpFont, btnYellow);

            // ── コントローラー ──
            var ctrlGO = new GameObject("RoomSetupController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<RoomSetupController>();
            var so   = new SerializedObject(ctrl);

            so.FindProperty("pinInputField").objectReferenceValue      = pinField;
            so.FindProperty("pinErrorLabel").objectReferenceValue      = pinErrorTmp;
            so.FindProperty("coopLifeButton").objectReferenceValue     = coopLifeBtnGO.GetComponent<Button>();
            so.FindProperty("teamBattleButton").objectReferenceValue   = teamBattleBtnGO.GetComponent<Button>();
            so.FindProperty("coopLifeBg").objectReferenceValue         = coopLifeBtnGO.GetComponent<Image>();
            so.FindProperty("teamBattleBg").objectReferenceValue       = teamBattleBtnGO.GetComponent<Image>();
            so.FindProperty("coopLifeLabel").objectReferenceValue      = coopLifeLbl;
            so.FindProperty("coopLifeDescLabel").objectReferenceValue  = coopLifeDescLbl;
            so.FindProperty("teamBattleLabel").objectReferenceValue    = teamBattleLbl;
            so.FindProperty("teamBattleDescLabel").objectReferenceValue= teamBattleDescLbl;
            so.FindProperty("titleLabel").objectReferenceValue         = titleTmp;
            so.FindProperty("pinHeaderLabel").objectReferenceValue     = pinHeaderTmp;
            so.FindProperty("modeHeaderLabel").objectReferenceValue    = modeHeaderTmp;
            so.FindProperty("confirmBtnLabel").objectReferenceValue    = confirmBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("backBtnLabel").objectReferenceValue       = backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("confirmButton").objectReferenceValue      = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue         = backBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue         = panelCG;

            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/RoomSetup.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/RoomSetup.unity");
            Debug.Log("[RoomSetupSceneBuilder] RoomSetup シーンを作成しました");
        }

        // ── PIN 入力フィールド ──────────────────────────────────────────

        static TMP_InputField MakePinInputField(Transform parent, string name, Vector2 pos, Vector2 size,
            TMP_FontAsset font, Sprite btnSpr)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            bg.sprite = btnSpr ?? GetBuiltinUISprite(); bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 0.96f, 0.88f);
            go.AddComponent<Shadow>().effectColor = new Color(0.22f, 0.14f, 0.02f, 0.30f);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic  = bg;
            inputField.characterLimit = 6;
            inputField.contentType    = TMP_InputField.ContentType.IntegerNumber;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(16f, 4f); taR.offsetMax = new Vector2(-16f, -4f);

            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(taGO.transform, false);
            var phR = phGO.GetComponent<RectTransform>();
            phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
            phR.offsetMin = phR.offsetMax = Vector2.zero;
            var phTmp = phGO.AddComponent<TextMeshProUGUI>();
            phTmp.text = "000000"; phTmp.fontSize = 56f;
            phTmp.fontStyle = FontStyles.Bold;
            phTmp.color = new Color(0.20f, 0.10f, 0.02f, 0.25f);
            phTmp.alignment = TextAlignmentOptions.Center;
            phTmp.raycastTarget = false;
            phTmp.enableWordWrapping = false;
            if (font != null) phTmp.font = font;

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 64f; txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.color = new Color(0.20f, 0.10f, 0.02f);
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.enableWordWrapping = false;
            if (font != null) txtTmp.font = font;

            inputField.textViewport  = taR;
            inputField.textComponent = txtTmp;
            inputField.placeholder   = phTmp;

            return inputField;
        }

        // ── ゲームモードボタン（大きめ・2行テキスト）──────────────────

        static GameObject MakeModeButton(Transform parent, string name,
            string mainText, string descText, Vector2 pos, Vector2 size,
            Color bgColor, Color textColor, TMP_FontAsset font, Sprite uiSpr, Sprite btnSpr,
            out TextMeshProUGUI mainLabel, out TextMeshProUGUI descLabel)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            bg.sprite = btnSpr ?? uiSpr; bg.type = Image.Type.Sliced; bg.color = bgColor;
            var sh = go.AddComponent<Shadow>();
            sh.effectColor    = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            sh.effectDistance = new Vector2(0f, -6f);

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor      = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor     = new Color(0.78f, 0.78f, 0.78f, 1f);
            cols.colorMultiplier  = 1f;
            btn.colors = cols; btn.targetGraphic = bg;

            // メインラベル（上寄り）
            var mainGO = new GameObject("Label", typeof(RectTransform));
            mainGO.transform.SetParent(go.transform, false);
            var mr = mainGO.GetComponent<RectTransform>();
            mr.anchorMin = new Vector2(0f, 0.5f); mr.anchorMax = new Vector2(1f, 1f);
            mr.offsetMin = new Vector2(20f, 0f); mr.offsetMax = new Vector2(-20f, -8f);
            mainLabel = mainGO.AddComponent<TextMeshProUGUI>();
            mainLabel.text = mainText; mainLabel.fontSize = 40f;
            mainLabel.fontStyle = FontStyles.Bold;
            mainLabel.alignment = TextAlignmentOptions.Center;
            mainLabel.color = textColor; mainLabel.raycastTarget = false;
            mainLabel.enableWordWrapping = false;
            if (font != null) mainLabel.font = font;

            // 説明ラベル（下寄り）
            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(go.transform, false);
            var dr = descGO.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0f, 0f); dr.anchorMax = new Vector2(1f, 0.5f);
            dr.offsetMin = new Vector2(20f, 8f); dr.offsetMax = new Vector2(-20f, 0f);
            descLabel = descGO.AddComponent<TextMeshProUGUI>();
            descLabel.text = descText; descLabel.fontSize = 32f;
            descLabel.fontStyle = FontStyles.Normal;
            descLabel.alignment = TextAlignmentOptions.Center;
            descLabel.color = textColor; descLabel.raycastTarget = false;
            descLabel.enableWordWrapping = true;
            if (font != null) descLabel.font = font;

            return go;
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
            cols.normalColor = Color.white; cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f); cols.colorMultiplier = 1f;
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
            if (ti == null) { Debug.LogWarning($"[RoomSetupBuilder] Not found: {path}"); return null; }
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
