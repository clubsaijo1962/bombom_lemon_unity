using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class ResultRevealSceneBuilder
    {
        const string CP   = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor       = new(0.98f, 0.92f, 0.62f);
        static readonly Color BtnPrimary    = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnSecondary  = new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary   = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted     = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor      = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color GuessedBg     = new(0.48f, 0.82f, 0.42f);   // グリーン
        static readonly Color GuessedNum    = new(0.22f, 0.52f, 0.18f);   // ダークグリーン
        static readonly Color SecretBg      = new(0.96f, 0.58f, 0.20f);   // オレンジ
        static readonly Color SecretNum     = new(0.60f, 0.22f, 0.04f);   // ダークオレンジ
        static readonly Color DiffBg        = new(0.98f, 0.36f, 0.22f);   // レッド
        static readonly Color PerfectBg     = new(0.42f, 0.76f, 0.38f);   // 成功グリーン

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

            var jpFont     = FindJapaneseTMPFont();
            var uiSprite   = GetBuiltinUISprite();
            var pillSprite = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);

            var lemonTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Title_Lemon.png");
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png");

            var bombTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/bomb.png");
            Sprite bombSprite = null;
            if (bombTex != null)
                bombSprite = Sprite.Create(bombTex, new Rect(0,0,bombTex.width,bombTex.height), new Vector2(0.5f,0.5f));
            else
                Debug.LogWarning("[ResultRevealBuilder] bomb.png not found → Assets/Sprites/UI/bomb.png");

            var painlemoTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/painlemo.png");
            Sprite painlemoSprite = null;
            if (painlemoTex != null)
                painlemoSprite = Sprite.Create(painlemoTex, new Rect(0,0,painlemoTex.width,painlemoTex.height), new Vector2(0.5f,0.5f));
            else
                Debug.LogWarning("[ResultRevealBuilder] painlemo.png not found → Assets/Sprites/UI/painlemo.png");

            // レモン透かしパターン
            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.06f);

            // Panel（フェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── HUD 右上（ライフ）──
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot     = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(200f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);
            TextMeshProUGUI lifeCountLabel;
            MakeHUDGroup(hudGO.transform, lemonSprite, jpFont, out lifeCountLabel);

            // ── HOME ボタン（左上）──
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton", "HOME",
                new Vector2(0f,1f), new Vector2(0f,1f),
                new Vector2(24f,-104f), new Vector2(200f,80f),
                BtnSecondary, TextMuted, 34f, jpFont, pillSprite);

            // ── タイトル ──
            MakeLabel(panelGO.transform, "Title", "結果発表",
                new Vector2(0.5f,0.5f), new Vector2(0f, 760f), new Vector2(800f,90f),
                56f, TextPrimary, FontStyles.Bold, jpFont);

            // ── 数字カード2枚（左:予想 / 右:秘密）──

            // 予想（左）
            var guessedGroupGO = new GameObject("GuessedGroup", typeof(RectTransform));
            guessedGroupGO.transform.SetParent(panelGO.transform, false);
            var guessedCG = guessedGroupGO.AddComponent<CanvasGroup>();
            guessedCG.alpha = 0f;
            SetRect(guessedGroupGO, new Vector2(0.5f,0.5f), new Vector2(460f,480f), new Vector2(-272f, 300f));
            TextMeshProUGUI guessedNumLabel;
            BuildNumberPill(guessedGroupGO.transform, pillSprite, uiSprite, jpFont,
                GuessedBg, GuessedNum, "予想", "Guess", out guessedNumLabel);

            // 秘密（右）
            var secretGroupGO = new GameObject("SecretGroup", typeof(RectTransform));
            secretGroupGO.transform.SetParent(panelGO.transform, false);
            var secretCG = secretGroupGO.AddComponent<CanvasGroup>();
            secretCG.alpha = 0f;
            SetRect(secretGroupGO, new Vector2(0.5f,0.5f), new Vector2(460f,480f), new Vector2(272f, 300f));
            TextMeshProUGUI secretNumLabel;
            BuildNumberPill(secretGroupGO.transform, pillSprite, uiSprite, jpFont,
                SecretBg, SecretNum, "秘密", "Secret", out secretNumLabel);

            // VS
            MakeLabel(panelGO.transform, "VS", "vs",
                new Vector2(0.5f,0.5f), new Vector2(0f, 300f), new Vector2(96f,80f),
                44f, TextMuted, FontStyles.Bold, jpFont);

            // ── セパレーター ──
            MakeSeparator(panelGO.transform, -60f);

            // ── 差・ライフ変化グループ ──
            var diffGroupGO = new GameObject("DiffGroup", typeof(RectTransform));
            diffGroupGO.transform.SetParent(panelGO.transform, false);
            var diffCG = diffGroupGO.AddComponent<CanvasGroup>();
            diffCG.alpha = 0f;
            SetRect(diffGroupGO, new Vector2(0.5f,0.5f), new Vector2(920f, 220f), new Vector2(0f, -200f));
            var diffBg = diffGroupGO.AddComponent<Image>();
            diffBg.sprite = uiSprite; diffBg.type = Image.Type.Sliced;
            diffBg.color = new Color(1f, 0.97f, 0.95f, 0.92f);

            var diffLabelTmp = MakeLabel(diffGroupGO.transform, "DiffLabel", "差: 0",
                new Vector2(0.5f,0.5f), new Vector2(0f, 50f), new Vector2(860f, 80f),
                52f, DiffBg, FontStyles.Bold, jpFont);

            var lifeChangeTmp = MakeLabel(diffGroupGO.transform, "LifeChangeLabel", "ライフ −0",
                new Vector2(0.5f,0.5f), new Vector2(0f, -46f), new Vector2(860f, 60f),
                38f, TextMuted, FontStyles.Bold, jpFont);

            // ── painlemo キャラクター（差カードの下）──
            var painlemoGO = new GameObject("PainlemoImage", typeof(RectTransform));
            painlemoGO.transform.SetParent(panelGO.transform, false);
            SetRect(painlemoGO, new Vector2(0.5f,0.5f), new Vector2(280f, 280f), new Vector2(0f, -530f));
            if (painlemoSprite != null)
            {
                var pImg = painlemoGO.AddComponent<Image>();
                pImg.sprite = painlemoSprite;
                pImg.preserveAspect = true;
                pImg.raycastTarget = false;
            }
            var painlemoRT = painlemoGO.GetComponent<RectTransform>();

            // ── 爆発エフェクト（painlemoの上に重ねる）──
            var expSmallRT = BuildExplosion(panelGO.transform, "ExplosionSmall", bombSprite, 320f, new Vector2(0f, -530f));
            var expLargeRT = BuildExplosion(panelGO.transform, "ExplosionLarge", bombSprite, 560f, new Vector2(0f, -530f));

            // ── 次へボタン ──
            var nextBtnGO = MakeButton(panelGO.transform, "NextButton", "次の番へ ▶",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(0f,-790f), new Vector2(900f,118f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // ── レモンシャワー（差が０の演出）──
            var showerParentGO = new GameObject("LemonShowerParent", typeof(RectTransform));
            showerParentGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(showerParentGO.GetComponent<RectTransform>());
            showerParentGO.SetActive(false);

            // ── ResultRevealController ──
            var ctrlGO = new GameObject("ResultRevealController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<ResultRevealController>();
            var so   = new SerializedObject(ctrl);
            so.FindProperty("guessedGroup").objectReferenceValue       = guessedCG;
            so.FindProperty("guessedNumberLabel").objectReferenceValue = guessedNumLabel;
            so.FindProperty("secretGroup").objectReferenceValue        = secretCG;
            so.FindProperty("secretNumberLabel").objectReferenceValue  = secretNumLabel;
            so.FindProperty("diffGroup").objectReferenceValue          = diffCG;
            so.FindProperty("diffLabel").objectReferenceValue          = diffLabelTmp;
            so.FindProperty("lifeChangeLabel").objectReferenceValue    = lifeChangeTmp;
            so.FindProperty("lifeCountLabel").objectReferenceValue     = lifeCountLabel;
            so.FindProperty("painlemoImage").objectReferenceValue      = painlemoRT;
            so.FindProperty("explosionSmall").objectReferenceValue     = expSmallRT;
            so.FindProperty("explosionLarge").objectReferenceValue     = expLargeRT;
            so.FindProperty("lemonShowerParent").objectReferenceValue  = showerParentGO.GetComponent<RectTransform>();
            so.FindProperty("lemonTex").objectReferenceValue           = lemonTex;
            so.FindProperty("nextButton").objectReferenceValue         = nextBtnGO.GetComponent<Button>();
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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/ResultReveal.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/ResultReveal.unity");

            Debug.Log("[ResultRevealSceneBuilder] ResultReveal シーンを作成しました");
        }

        // ── 数字ピル（丸いカード）───────────────────────────────────────

        static void BuildNumberPill(Transform parent, Sprite pillSprite, Sprite uiSprite, TMP_FontAsset font,
            Color bgColor, Color numColor, string labelJP, string labelEN, out TextMeshProUGUI numberLabel)
        {
            // 上部ラベルチップ
            var chipGO = new GameObject("LabelChip", typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var chipImg = chipGO.AddComponent<Image>();
            chipImg.sprite = pillSprite ?? uiSprite;
            chipImg.type   = Image.Type.Sliced;
            chipImg.color  = bgColor;
            chipImg.raycastTarget = false;
            var chipR = chipGO.GetComponent<RectTransform>();
            chipR.anchorMin = chipR.anchorMax = new Vector2(0.5f, 1f);
            chipR.pivot     = new Vector2(0.5f, 1f);
            chipR.sizeDelta = new Vector2(240f, 70f);
            chipR.anchoredPosition = Vector2.zero;

            var chipLabel = chipGO.AddComponent<TextMeshProUGUI>();
            // TextMeshPro must be child
            Object.DestroyImmediate(chipLabel);

            var chipTxtGO = new GameObject("ChipLabel", typeof(RectTransform));
            chipTxtGO.transform.SetParent(chipGO.transform, false);
            StretchFull(chipTxtGO.GetComponent<RectTransform>());
            var chipTmp = chipTxtGO.AddComponent<TextMeshProUGUI>();
            chipTmp.text      = labelJP;
            chipTmp.fontSize  = 34f;
            chipTmp.fontStyle = FontStyles.Bold;
            chipTmp.alignment = TextAlignmentOptions.Center;
            chipTmp.color     = Color.white;
            chipTmp.raycastTarget = false;
            if (font != null) chipTmp.font = font;

            // 数字ピル本体（大きな丸角）
            var pillGO = new GameObject("NumberPill", typeof(RectTransform));
            pillGO.transform.SetParent(parent, false);
            var pillImg = pillGO.AddComponent<Image>();
            pillImg.sprite = pillSprite ?? uiSprite;
            pillImg.type   = Image.Type.Sliced;
            pillImg.color  = new Color(bgColor.r * 1.10f, bgColor.g * 1.10f, bgColor.b * 1.10f, 0.18f);
            pillImg.raycastTarget = false;
            var pillR = pillGO.GetComponent<RectTransform>();
            pillR.anchorMin = new Vector2(0f, 0f);
            pillR.anchorMax = new Vector2(1f, 1f);
            pillR.offsetMin = new Vector2(0f, 0f);
            pillR.offsetMax = new Vector2(0f, -76f);

            // 数字ラベル
            var numGO = new GameObject("Number", typeof(RectTransform));
            numGO.transform.SetParent(pillGO.transform, false);
            StretchFull(numGO.GetComponent<RectTransform>());
            numberLabel = numGO.AddComponent<TextMeshProUGUI>();
            numberLabel.text      = "?";
            numberLabel.fontStyle = FontStyles.Bold;
            numberLabel.alignment = TextAlignmentOptions.Center;
            numberLabel.color     = numColor;
            numberLabel.enableAutoSizing = true;
            numberLabel.fontSizeMin = 80f;
            numberLabel.fontSizeMax = 180f;
            numberLabel.enableWordWrapping = false;
            numberLabel.raycastTarget = false;
            if (font != null) numberLabel.font = font;
        }

        // ── 爆発オブジェクト ─────────────────────────────────────────────

        static RectTransform BuildExplosion(Transform parent, string name, Sprite sprite, float size, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(size, size);
            r.anchoredPosition = pos;
            r.localScale = Vector3.zero;

            if (sprite != null)
            {
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget  = false;
            }
            return r;
        }

        // ── HUD グループ ─────────────────────────────────────────────────

        static void MakeHUDGroup(Transform parent, Sprite icon, TMP_FontAsset font, out TextMeshProUGUI countLabel)
        {
            var grpGO = new GameObject("LifeGroup", typeof(RectTransform));
            grpGO.transform.SetParent(parent, false);
            StretchFull(grpGO.GetComponent<RectTransform>());

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
            countLabel.alignment       = TextAlignmentOptions.MidlineLeft;
            countLabel.enableWordWrapping  = false;
            countLabel.enableAutoSizing = true; countLabel.fontSizeMin = 32f; countLabel.fontSizeMax = 42f;
            countLabel.color = new Color(0.20f, 0.10f, 0.02f); countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

        // ── レモン透かし ─────────────────────────────────────────────────

        static void BuildLemonPattern(Transform parent, Sprite lemonSprite, float alpha)
        {
            if (lemonSprite == null) return;
            var patternGO = new GameObject("LemonPattern", typeof(RectTransform));
            patternGO.transform.SetParent(parent, false);
            StretchFull(patternGO.GetComponent<RectTransform>());
            const float iconSize = 110f, colStep = 250f, rowStep = 220f, angle = -22f;
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

        static void MakeSeparator(Transform parent, float y)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = SepColor;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(880f, 2f);
            r.anchoredPosition = new Vector2(0f, y);
        }

        static void SetRect(GameObject go, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
        }

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[ResultRevealBuilder] Not found: {path}"); return null; }
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

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = align; tmp.color = color; tmp.raycastTarget = false;
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
            bg.sprite = btnSprite ?? GetBuiltinUISprite();
            bg.type   = Image.Type.Sliced;
            bg.color  = bgColor;

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor      = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
            cols.colorMultiplier  = 1f;
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
