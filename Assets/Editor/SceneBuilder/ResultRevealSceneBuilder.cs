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

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnSecondary = new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color GuessedTint  = new(0.55f, 0.82f, 0.48f);  // グリーン（予想）
        static readonly Color SecretTint   = new(0.97f, 0.62f, 0.22f);  // オレンジ（秘密）
        static readonly Color DiffTint     = new(0.90f, 0.25f, 0.18f);  // レッド（差）

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
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont    = FindJapaneseTMPFont();
            var uiSprite  = GetBuiltinUISprite();
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);

            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png");
            var cardSprite  = FindSprite("card");
            if (cardSprite == null)
            {
                var cardTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (cardTex != null)
                    cardSprite = Sprite.Create(cardTex, new Rect(0, 0, cardTex.width, cardTex.height), new Vector2(0.5f, 0.5f));
            }

            // bomb.pngの読み込み（ユーザー指定のパスが存在しない場合は警告のみ）
            var bombTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/bomb.png");
            Sprite bombSprite = null;
            if (bombTex != null)
                bombSprite = Sprite.Create(bombTex, new Rect(0, 0, bombTex.width, bombTex.height), new Vector2(0.5f, 0.5f));
            else
                Debug.LogWarning("[ResultRevealBuilder] bomb.png が見つかりません: Assets/Sprites/UI/bomb.png → 爆発画像なしでビルドします");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // Panel CanvasGroup（フェード用）
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
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(200f, 80f),
                BtnSecondary, TextMuted, 34f, jpFont, btnYellow);

            // ── タイトルラベル ──
            MakeLabel(panelGO.transform, "Title",
                "結果発表",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 740f), new Vector2(800f, 80f),
                52f, TextPrimary, FontStyles.Bold, jpFont);

            // ── 左カード：予想数字 ──
            var guessedGroupGO = new GameObject("GuessedGroup", typeof(RectTransform));
            guessedGroupGO.transform.SetParent(panelGO.transform, false);
            var guessedCG = guessedGroupGO.AddComponent<CanvasGroup>();
            guessedCG.alpha = 0f;
            {
                var r = guessedGroupGO.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(460f, 560f);
                r.anchoredPosition = new Vector2(-265f, 200f);
            }
            BuildNumberCard(guessedGroupGO.transform, uiSprite, CardColor, GuessedTint,
                "予想", "Guess", jpFont, out var guessedNumLabel);

            // ── 右カード：秘密数字 ──
            var secretGroupGO = new GameObject("SecretGroup", typeof(RectTransform));
            secretGroupGO.transform.SetParent(panelGO.transform, false);
            var secretCG = secretGroupGO.AddComponent<CanvasGroup>();
            secretCG.alpha = 0f;
            {
                var r = secretGroupGO.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(460f, 560f);
                r.anchoredPosition = new Vector2(265f, 200f);
            }
            BuildNumberCard(secretGroupGO.transform, uiSprite, CardColor, SecretTint,
                "秘密", "Secret", jpFont, out var secretNumLabel);

            // VS ラベル（中央）
            MakeLabel(panelGO.transform, "VS",
                "vs",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(120f, 80f),
                44f, TextMuted, FontStyles.Bold, jpFont);

            // セパレーター
            MakeSeparator(panelGO.transform, -170f);

            // ── 差・ライフ変化グループ ──
            var diffGroupGO = new GameObject("DiffGroup", typeof(RectTransform));
            diffGroupGO.transform.SetParent(panelGO.transform, false);
            var diffCG = diffGroupGO.AddComponent<CanvasGroup>();
            diffCG.alpha = 0f;
            {
                var r = diffGroupGO.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(900f, 260f);
                r.anchoredPosition = new Vector2(0f, -370f);
            }

            var diffCardBg = diffGroupGO.AddComponent<Image>();
            diffCardBg.sprite = uiSprite; diffCardBg.type = Image.Type.Sliced;
            diffCardBg.color = new Color(1f, 0.94f, 0.92f, 0.9f);

            var diffLabelTmp = MakeLabel(diffGroupGO.transform, "DiffLabel",
                "差: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 56f), new Vector2(860f, 80f),
                46f, DiffTint, FontStyles.Bold, jpFont);

            var lifeChangeTmp = MakeLabel(diffGroupGO.transform, "LifeChangeLabel",
                "ライフ −0",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(860f, 60f),
                38f, TextMuted, FontStyles.Bold, jpFont);

            // ── 次へボタン ──
            var nextBtnGO = MakeButton(panelGO.transform, "NextButton",
                "次の番へ ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -720f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 46f, jpFont, btnYellow);

            // ── 爆発エフェクト（bomb.png、中央オーバーレイ）──
            RectTransform explosionSmall = null, explosionLarge = null;

            explosionSmall = BuildExplosion(canvasGO.transform, "ExplosionSmall", bombSprite, 280f);
            explosionLarge = BuildExplosion(canvasGO.transform, "ExplosionLarge", bombSprite, 520f);

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
            so.FindProperty("explosionSmall").objectReferenceValue     = explosionSmall;
            so.FindProperty("explosionLarge").objectReferenceValue     = explosionLarge;
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

        static RectTransform BuildExplosion(Transform parent, string name, Sprite sprite, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(size, size);
            r.anchoredPosition = new Vector2(0f, 200f);

            if (sprite != null)
            {
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
            }
            return r;
        }

        static void BuildNumberCard(Transform parent, Sprite uiSprite, Color cardColor, Color headerColor,
            string labelJP, string labelEN, TMP_FontAsset font, out TextMeshProUGUI numberLabel)
        {
            // カード背景
            var cardGO = new GameObject("Card", typeof(RectTransform));
            cardGO.transform.SetParent(parent, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = cardColor; cardImg.raycastTarget = false;
            var cr = cardGO.GetComponent<RectTransform>();
            cr.anchorMin = Vector2.zero; cr.anchorMax = Vector2.one;
            cr.offsetMin = cr.offsetMax = Vector2.zero;

            // ヘッダーバー
            var hdrGO = new GameObject("Header", typeof(RectTransform));
            hdrGO.transform.SetParent(cardGO.transform, false);
            var hdrImg = hdrGO.AddComponent<Image>();
            hdrImg.sprite = uiSprite; hdrImg.type = Image.Type.Sliced;
            hdrImg.color = headerColor; hdrImg.raycastTarget = false;
            var hr = hdrGO.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0f, 1f); hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.sizeDelta = new Vector2(0f, 110f); hr.anchoredPosition = Vector2.zero;

            var hdrLabel = MakeLabel(hdrGO.transform, "HeaderLabel",
                labelJP,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 80f),
                38f, Color.white, FontStyles.Bold, font);

            // 数字ラベル（大きく中央に）
            numberLabel = MakeLabel(cardGO.transform, "NumberLabel",
                "??",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(400f, 260f),
                160f, new Color(0.18f, 0.08f, 0.01f), FontStyles.Bold, font,
                autoSizeMin: 80f, autoSizeMax: 180f);
        }

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
            countLabel.alignment = TextAlignmentOptions.MidlineLeft;
            countLabel.enableWordWrapping = false;
            countLabel.enableAutoSizing = true; countLabel.fontSizeMin = 32f; countLabel.fontSizeMax = 42f;
            countLabel.color = new Color(0.20f, 0.10f, 0.02f); countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

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

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[ResultRevealBuilder] Sprite not found: {path}"); return null; }
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

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font,
            float autoSizeMin = 0f, float autoSizeMax = 0f,
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
            if (autoSizeMin > 0f && autoSizeMax > 0f)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = autoSizeMin; tmp.fontSizeMax = autoSizeMax;
            }
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
            cols.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
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
