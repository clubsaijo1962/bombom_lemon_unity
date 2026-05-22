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
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnSecondary = new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color GuessedBg    = new(0.42f, 0.78f, 0.36f);
        static readonly Color GuessedNum   = new(0.15f, 0.45f, 0.10f);
        static readonly Color SecretBg     = new(0.95f, 0.55f, 0.18f);
        static readonly Color SecretNum    = new(0.55f, 0.18f, 0.02f);
        static readonly Color DiffBg       = new(0.96f, 0.32f, 0.18f);
        static readonly Color DialogBg     = new(1f,    0.97f, 0.90f);

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
            var lemonTex    = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Title_Lemon.png");

            var cardSprite = FindSprite("card");
            if (cardSprite == null)
            {
                var ct = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (ct != null) cardSprite = Sprite.Create(ct, new Rect(0,0,ct.width,ct.height), new Vector2(0.5f,0.5f));
            }

            Sprite bombSprite = null;
            var bombTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/bomb.png");
            if (bombTex != null) bombSprite = Sprite.Create(bombTex, new Rect(0,0,bombTex.width,bombTex.height), new Vector2(0.5f,0.5f));
            else Debug.LogWarning("[ResultRevealBuilder] bomb.png not found");

            Sprite painlemoSprite = null;
            var painlemoTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/painlemo.png");
            if (painlemoTex != null) painlemoSprite = Sprite.Create(painlemoTex, new Rect(0,0,painlemoTex.width,painlemoTex.height), new Vector2(0.5f,0.5f));
            else Debug.LogWarning("[ResultRevealBuilder] painlemo.png not found");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.06f);

            // Panel（フェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── HUD 右上（ライフ＋ヘルプカード）──
            // HUD幅を広げてヘルプカードも常に表示
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot     = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(340f, 68f);
            hudR.anchoredPosition = new Vector2(-14f, -114f);

            TextMeshProUGUI lifeCountLabel, helpCardCountLabel;
            MakeHUDGroup(hudGO.transform, "LifeGroup",  0f,    0.47f, lemonSprite, jpFont, out lifeCountLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup",  0.53f, 1f,    cardSprite,  jpFont, out helpCardCountLabel);

            // ── HOME ボタン ──
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton", "HOME",
                new Vector2(0f,1f), new Vector2(0f,1f),
                new Vector2(24f,-104f), new Vector2(200f,80f),
                BtnSecondary, TextMuted, 34f, jpFont, pillSprite);

            // ── タイトル ──
            MakeLabel(panelGO.transform, "Title", "結果発表",
                new Vector2(0.5f,0.5f), new Vector2(0f, 760f), new Vector2(800f,90f),
                56f, TextPrimary, FontStyles.Bold, jpFont);

            // ── 数字カード 2枚（幅420、余白90px、中心±255）──
            // 左カード: 予想
            var guessedGroupGO = new GameObject("GuessedGroup", typeof(RectTransform));
            guessedGroupGO.transform.SetParent(panelGO.transform, false);
            var guessedCG = guessedGroupGO.AddComponent<CanvasGroup>();
            guessedCG.alpha = 0f;
            SetAnchoredRect(guessedGroupGO, new Vector2(420f, 460f), new Vector2(-255f, 310f));
            TextMeshProUGUI guessedNumLabel;
            BuildNumberPill(guessedGroupGO.transform, pillSprite, uiSprite, jpFont,
                GuessedBg, GuessedNum, "予　想", out guessedNumLabel);

            // 右カード: 秘密
            var secretGroupGO = new GameObject("SecretGroup", typeof(RectTransform));
            secretGroupGO.transform.SetParent(panelGO.transform, false);
            var secretCG = secretGroupGO.AddComponent<CanvasGroup>();
            secretCG.alpha = 0f;
            SetAnchoredRect(secretGroupGO, new Vector2(420f, 460f), new Vector2(255f, 310f));
            TextMeshProUGUI secretNumLabel;
            BuildNumberPill(secretGroupGO.transform, pillSprite, uiSprite, jpFont,
                SecretBg, SecretNum, "秘密の数字", out secretNumLabel);

            // VS
            MakeLabel(panelGO.transform, "VS", "vs",
                new Vector2(0.5f,0.5f), new Vector2(0f, 310f), new Vector2(80f, 72f),
                40f, TextMuted, FontStyles.Bold, jpFont);

            // ── セパレーター ──
            MakeSeparator(panelGO.transform, -50f);

            // ── 差カード ──
            var diffGroupGO = new GameObject("DiffGroup", typeof(RectTransform));
            diffGroupGO.transform.SetParent(panelGO.transform, false);
            var diffCG = diffGroupGO.AddComponent<CanvasGroup>();
            diffCG.alpha = 0f;
            SetAnchoredRect(diffGroupGO, new Vector2(880f, 200f), new Vector2(0f, -175f));
            var diffBg = diffGroupGO.AddComponent<Image>();
            diffBg.sprite = uiSprite; diffBg.type = Image.Type.Sliced;
            diffBg.color = new Color(1f, 0.97f, 0.94f, 0.92f);
            var diffLabelTmp = MakeLabel(diffGroupGO.transform, "DiffLabel", "差: 0",
                new Vector2(0.5f,0.5f), new Vector2(0f, 44f), new Vector2(840f, 80f),
                52f, DiffBg, FontStyles.Bold, jpFont);
            var lifeChangeTmp = MakeLabel(diffGroupGO.transform, "LifeChangeLabel", "ライフ −0",
                new Vector2(0.5f,0.5f), new Vector2(0f, -44f), new Vector2(840f, 60f),
                38f, TextMuted, FontStyles.Bold, jpFont);

            // ── キャラクターグループ（差カード直下、差と同時表示）──
            var charGroupGO = new GameObject("CharacterGroup", typeof(RectTransform));
            charGroupGO.transform.SetParent(panelGO.transform, false);
            var charCG = charGroupGO.AddComponent<CanvasGroup>();
            charCG.alpha = 0f;
            SetAnchoredRect(charGroupGO, new Vector2(300f, 300f), new Vector2(0f, -510f));

            // painlemo（diff>0）
            var plGO = new GameObject("Painlemo", typeof(RectTransform));
            plGO.transform.SetParent(charGroupGO.transform, false);
            StretchFull(plGO.GetComponent<RectTransform>());
            Image painlemoImg = null;
            if (painlemoSprite != null)
            {
                painlemoImg = plGO.AddComponent<Image>();
                painlemoImg.sprite = painlemoSprite;
                painlemoImg.preserveAspect = true;
                painlemoImg.raycastTarget = false;
            }

            // lemon（diff==0）
            var lmGO = new GameObject("Lemon", typeof(RectTransform));
            lmGO.transform.SetParent(charGroupGO.transform, false);
            StretchFull(lmGO.GetComponent<RectTransform>());
            Image lemonImg = null;
            if (lemonSprite != null)
            {
                lemonImg = lmGO.AddComponent<Image>();
                lemonImg.sprite = lemonSprite;
                lemonImg.preserveAspect = true;
                lemonImg.raycastTarget = false;
            }
            lmGO.SetActive(false); // 初期は非表示（差が0の時だけ使う）

            // ── 爆発エフェクト（キャラクターの上に重ねる）──
            var expSmallRT = BuildExplosion(panelGO.transform, "ExplosionSmall", bombSprite, 300f, new Vector2(0f, -510f));
            var expLargeRT = BuildExplosion(panelGO.transform, "ExplosionLarge", bombSprite, 560f, new Vector2(0f, -510f));

            // ── ヘルプカードダイアログ（diff>=5かつ残枚数ありの時表示）──
            var helpDialogGroupGO = new GameObject("HelpCardDialog", typeof(RectTransform));
            helpDialogGroupGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(helpDialogGroupGO.GetComponent<RectTransform>());
            var helpDialogCG = helpDialogGroupGO.AddComponent<CanvasGroup>();
            helpDialogCG.alpha = 0f; helpDialogCG.blocksRaycasts = false;

            // 暗幕
            var hdBack = new GameObject("Backdrop", typeof(RectTransform));
            hdBack.transform.SetParent(helpDialogGroupGO.transform, false);
            StretchFull(hdBack.GetComponent<RectTransform>());
            hdBack.AddComponent<Image>().color = new Color(0f,0f,0f,0.55f);

            // カード
            var hdCard = new GameObject("Card", typeof(RectTransform));
            hdCard.transform.SetParent(helpDialogGroupGO.transform, false);
            var hdCardImg = hdCard.AddComponent<Image>();
            hdCardImg.sprite = uiSprite; hdCardImg.type = Image.Type.Sliced;
            hdCardImg.color = DialogBg;
            var hdR = hdCard.GetComponent<RectTransform>();
            hdR.anchorMin = hdR.anchorMax = new Vector2(0.5f, 0.5f);
            hdR.pivot = new Vector2(0.5f, 0.5f);
            hdR.sizeDelta = new Vector2(900f, 520f);
            hdR.anchoredPosition = Vector2.zero;

            var hdBody = MakeLabel(hdCard.transform, "Body",
                "ヘルプカードを使って\nマイナスを4にしますか？",
                new Vector2(0.5f,0.5f), new Vector2(0f, 90f), new Vector2(820f, 200f),
                40f, TextPrimary, FontStyles.Bold, jpFont);
            hdBody.lineSpacing = 6f;

            var hdUseBtn = MakeButton(hdCard.transform, "UseHelpButton", "使う",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(-220f, -110f), new Vector2(360f,100f),
                new Color(0.42f,0.78f,0.36f), Color.white, 44f, jpFont, pillSprite);
            var hdNoBtn = MakeButton(hdCard.transform, "DontUseButton", "使わない",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(220f, -110f), new Vector2(360f,100f),
                BtnSecondary, TextMuted, 40f, jpFont, pillSprite);

            // ── 次へボタン ──
            var nextBtnGO = MakeButton(panelGO.transform, "NextButton", "次の番へ ▶",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(0f,-790f), new Vector2(900f,118f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // ── レモンシャワー用親 ──
            var showerGO = new GameObject("LemonShowerParent", typeof(RectTransform));
            showerGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(showerGO.GetComponent<RectTransform>());
            showerGO.SetActive(false);

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
            so.FindProperty("characterGroup").objectReferenceValue     = charCG;
            so.FindProperty("painlemoImage").objectReferenceValue      = painlemoImg;
            so.FindProperty("lemonImage").objectReferenceValue         = lemonImg;
            so.FindProperty("explosionSmall").objectReferenceValue     = expSmallRT;
            so.FindProperty("explosionLarge").objectReferenceValue     = expLargeRT;
            so.FindProperty("lemonShowerParent").objectReferenceValue  = showerGO.GetComponent<RectTransform>();
            so.FindProperty("lemonTex").objectReferenceValue           = lemonTex;
            so.FindProperty("helpDialogGroup").objectReferenceValue    = helpDialogCG;
            so.FindProperty("helpDialogBodyLabel").objectReferenceValue= hdBody;
            so.FindProperty("useHelpButton").objectReferenceValue      = hdUseBtn.GetComponent<Button>();
            so.FindProperty("dontUseButton").objectReferenceValue      = hdNoBtn.GetComponent<Button>();
            so.FindProperty("lifeCountLabel").objectReferenceValue     = lifeCountLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpCardCountLabel;
            so.FindProperty("nextButton").objectReferenceValue         = nextBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue         = homeBtnGO.GetComponent<Button>();
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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/ResultReveal.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/ResultReveal.unity");
            Debug.Log("[ResultRevealSceneBuilder] ResultReveal シーンを作成しました");
        }

        // ── 数字ピル ──────────────────────────────────────────────────

        static void BuildNumberPill(Transform parent, Sprite pill, Sprite ui, TMP_FontAsset font,
            Color headerColor, Color numColor, string labelText, out TextMeshProUGUI numberLabel)
        {
            // ヘッダーチップ（上）
            var chipGO = new GameObject("LabelChip", typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var chipImg = chipGO.AddComponent<Image>();
            chipImg.sprite = pill ?? ui; chipImg.type = Image.Type.Sliced;
            chipImg.color = headerColor; chipImg.raycastTarget = false;
            var cr = chipGO.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 1f);
            cr.pivot     = new Vector2(0.5f, 1f);
            cr.sizeDelta = new Vector2(320f, 80f);
            cr.anchoredPosition = Vector2.zero;

            var chipTxtGO = new GameObject("T", typeof(RectTransform));
            chipTxtGO.transform.SetParent(chipGO.transform, false);
            StretchFull(chipTxtGO.GetComponent<RectTransform>());
            var chipTmp = chipTxtGO.AddComponent<TextMeshProUGUI>();
            chipTmp.text      = labelText;
            chipTmp.fontSize  = 40f;
            chipTmp.fontStyle = FontStyles.Bold;
            chipTmp.alignment = TextAlignmentOptions.Center;
            chipTmp.color     = Color.white;
            chipTmp.raycastTarget = false;
            if (font != null) chipTmp.font = font;

            // 数字エリア（ピル全体の残り部分）
            var pillGO = new GameObject("NumberPill", typeof(RectTransform));
            pillGO.transform.SetParent(parent, false);
            var pillImg = pillGO.AddComponent<Image>();
            pillImg.sprite = pill ?? ui; pillImg.type = Image.Type.Sliced;
            pillImg.color  = new Color(headerColor.r, headerColor.g, headerColor.b, 0.14f);
            pillImg.raycastTarget = false;
            var pr = pillGO.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0f, 0f); pr.anchorMax = new Vector2(1f, 1f);
            pr.offsetMin = new Vector2(0f, 0f); pr.offsetMax = new Vector2(0f, -84f);

            var sh = pillGO.AddComponent<Shadow>();
            sh.effectColor    = new Color(headerColor.r * 0.5f, headerColor.g * 0.5f, headerColor.b * 0.5f, 0.30f);
            sh.effectDistance = new Vector2(0f, -8f);

            var numGO = new GameObject("Number", typeof(RectTransform));
            numGO.transform.SetParent(pillGO.transform, false);
            StretchFull(numGO.GetComponent<RectTransform>());
            numberLabel = numGO.AddComponent<TextMeshProUGUI>();
            numberLabel.text           = "?";
            numberLabel.fontStyle      = FontStyles.Bold;
            numberLabel.alignment      = TextAlignmentOptions.Center;
            numberLabel.color          = numColor;
            numberLabel.enableAutoSizing    = true;
            numberLabel.fontSizeMin    = 80f;
            numberLabel.fontSizeMax    = 180f;
            numberLabel.enableWordWrapping  = false;
            numberLabel.raycastTarget  = false;
            if (font != null) numberLabel.font = font;
        }

        // ── 爆発 ──────────────────────────────────────────────────────

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
                img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
            }
            return r;
        }

        // ── HUD グループ ──────────────────────────────────────────────

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
            countLabel.color = TextPrimary; countLabel.raycastTarget = false;
            if (font != null) countLabel.font = font;
        }

        // ── レモン透かし ──────────────────────────────────────────────

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

        static void SetAnchoredRect(GameObject go, Vector2 size, Vector2 pos)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
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
            if (ti == null) { Debug.LogWarning($"[ResultRevealBuilder] Not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            { ti.spriteImportMode = SpriteImportMode.Single; ti.spriteBorder = border; ti.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite GetBuiltinUISprite() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static void StretchFull(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, Color color, FontStyles style,
            TMP_FontAsset font, TextAlignmentOptions align = TextAlignmentOptions.Center)
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
