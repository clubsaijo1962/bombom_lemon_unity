using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class NumberConfirmSceneBuilder
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
            var uiSprite  = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite = FindSprite("card");
            if (cardSprite == null)
            {
                var cardTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (cardTex != null)
                    cardSprite = Sprite.Create(cardTex, new Rect(0, 0, cardTex.width, cardTex.height), new Vector2(0.5f, 0.5f));
            }
            if (cardSprite == null) Debug.LogWarning("[NumberConfirmBuilder] card sprite not found");

            // レモン透かし
            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

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

            MakeHUDGroup(hudGO.transform, "LifeGroup", 0f,    0.47f, lemonSprite, jpFont, out var lifeLabel);
            MakeHUDGroup(hudGO.transform, "HelpGroup", 0.53f, 1f,    cardSprite,  jpFont, out var helpLabel);
            helpLabel.text = "×0";

            // ── HOME ボタン（左上固定）──
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton",
                "HOME",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(200f, 80f),
                BtnSecondary, TextMuted, 34f, jpFont, btnYellow);

            // ── ラウンド表示（上部中央）──
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

            // ── 回答プレイヤー ヘッダー ──
            var answerHeaderTmp = MakeLabel(panelGO.transform, "AnswerHeader",
                "回答プレイヤー",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 618f), new Vector2(900f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            // ── プレイヤー名チップ ──
            var playerNameTmp = MakePlayerNameChip(panelGO.transform, "PlayerNameChip",
                new Vector2(0f, 504f), ChipAlt, TextPrimary, jpFont);

            // ── セパレーター ──
            {
                var divGO = new GameObject("Divider", typeof(RectTransform));
                divGO.transform.SetParent(panelGO.transform, false);
                divGO.AddComponent<Image>().color = SepColor;
                var divR = divGO.GetComponent<RectTransform>();
                divR.anchorMin = divR.anchorMax = new Vector2(0.5f, 0.5f);
                divR.pivot = new Vector2(0.5f, 0.5f);
                divR.sizeDelta = new Vector2(880f, 1f);
                divR.anchoredPosition = new Vector2(0f, 408f);
            }

            // ── QuestionGroup ──
            var questionGroupGO = new GameObject("QuestionGroup", typeof(RectTransform));
            questionGroupGO.transform.SetParent(panelGO.transform, false);
            StretchFull(questionGroupGO.GetComponent<RectTransform>());
            var questionCG = questionGroupGO.AddComponent<CanvasGroup>();
            questionCG.alpha = 1f;
            questionCG.blocksRaycasts = true;

            // 均等配置: divider(408)〜card bottom(-540)=948px、instruction(110)+badge(300)+next(118)、4gap=105px
            // Instruction y=248, Lock badge y=-62, Next y=-376

            var instructionLabelTmp = MakeLabel(questionGroupGO.transform, "InstructionLabel",
                "この人だけが秘密の数字を確認してください",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 248f), new Vector2(860f, 110f),
                32f, TextPrimary, FontStyles.Normal, jpFont,
                autoSizeMin: 32f, autoSizeMax: 40f);

            // ロックバッジボタン（UIパック element_lock.png）
            var lockSprite = EnsureSprite(CP + "element_lock.png");
            var revealBtnGO = new GameObject("RevealButton", typeof(RectTransform));
            revealBtnGO.transform.SetParent(questionGroupGO.transform, false);
            {
                var rr = revealBtnGO.GetComponent<RectTransform>();
                rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
                rr.pivot = new Vector2(0.5f, 0.5f);
                rr.sizeDelta = new Vector2(300f, 300f);
                rr.anchoredPosition = new Vector2(0f, -62f);

                var circleSprite = GetOrCreateCircleSprite();
                var bgImg = revealBtnGO.AddComponent<Image>();
                bgImg.sprite = circleSprite ?? uiSprite;
                bgImg.type = circleSprite != null ? Image.Type.Simple : Image.Type.Sliced;
                bgImg.color = LemonYellow;
                var sh = revealBtnGO.AddComponent<Shadow>();
                sh.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.35f);
                sh.effectDistance = new Vector2(0f, -12f);

                var btn = revealBtnGO.AddComponent<Button>();
                var cols = btn.colors;
                cols.normalColor      = Color.white;
                cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
                cols.pressedColor     = new Color(0.78f, 0.78f, 0.78f, 1f);
                cols.colorMultiplier  = 1f;
                btn.colors = cols; btn.targetGraphic = bgImg;

                if (lockSprite != null)
                {
                    var iconGO = new GameObject("LockIcon", typeof(RectTransform));
                    iconGO.transform.SetParent(revealBtnGO.transform, false);
                    var ir = iconGO.GetComponent<RectTransform>();
                    ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.5f);
                    ir.pivot = new Vector2(0.5f, 0.5f);
                    ir.sizeDelta = new Vector2(168f, 168f);
                    ir.anchoredPosition = Vector2.zero;
                    var iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = lockSprite;
                    iconImg.preserveAspect = true;
                    iconImg.color = Color.white;
                    iconImg.raycastTarget = false;
                }
            }

            // ── NumberGroup ──
            var numberGroupGO = new GameObject("NumberGroup", typeof(RectTransform));
            numberGroupGO.transform.SetParent(panelGO.transform, false);
            StretchFull(numberGroupGO.GetComponent<RectTransform>());
            var numberCG = numberGroupGO.AddComponent<CanvasGroup>();
            numberCG.alpha = 0f;
            numberCG.blocksRaycasts = false;

            var secretLabelTmp = MakeLabel(numberGroupGO.transform, "SecretLabel",
                "秘密の数字",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 248f), new Vector2(700f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            var numberLabelGO = new GameObject("NumberLabel", typeof(RectTransform));
            numberLabelGO.transform.SetParent(numberGroupGO.transform, false);
            var nlR = numberLabelGO.GetComponent<RectTransform>();
            nlR.anchorMin = nlR.anchorMax = new Vector2(0.5f, 0.5f);
            nlR.pivot = new Vector2(0.5f, 0.5f);
            nlR.sizeDelta = new Vector2(700f, 300f);
            nlR.anchoredPosition = new Vector2(0f, -62f);
            var numberLabelTmp = numberLabelGO.AddComponent<TextMeshProUGUI>();
            numberLabelTmp.text = "?";
            numberLabelTmp.fontStyle = FontStyles.Bold;
            numberLabelTmp.alignment = TextAlignmentOptions.Center;
            numberLabelTmp.color = new Color(0.94f, 0.52f, 0.06f); // オレンジ寄り
            numberLabelTmp.enableAutoSizing = true;
            numberLabelTmp.fontSizeMin = 80f;
            numberLabelTmp.fontSizeMax = 200f;
            numberLabelTmp.raycastTarget = false;
            if (jpFont != null) numberLabelTmp.font = jpFont;

            // ── 次へボタン（初期非表示）──
            var nextBtnGO = MakeButton(panelGO.transform, "NextButton",
                "確認したら次へ ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -790f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 46f, jpFont, btnYellow);
            nextBtnGO.SetActive(false);

            // ── コントローラー ──
            var ctrlGO = new GameObject("NumberConfirmController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<NumberConfirmController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("playerNameLabel").objectReferenceValue    = playerNameTmp;
            so.FindProperty("lifeCountLabel").objectReferenceValue     = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("roundLabel").objectReferenceValue         = roundLabelTmp;
            so.FindProperty("questionGroup").objectReferenceValue      = questionCG;
            so.FindProperty("numberGroup").objectReferenceValue        = numberCG;
            so.FindProperty("revealButton").objectReferenceValue       = revealBtnGO.GetComponent<Button>();
            so.FindProperty("secretNumberLabel").objectReferenceValue  = numberLabelTmp;
            so.FindProperty("nextButton").objectReferenceValue         = nextBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue         = homeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue         = panelCG;
            so.FindProperty("answerHeaderLabel").objectReferenceValue  = answerHeaderTmp;
            so.FindProperty("instructionLabel").objectReferenceValue   = instructionLabelTmp;
            so.FindProperty("secretLabel").objectReferenceValue        = secretLabelTmp;
            so.FindProperty("nextBtnLabel").objectReferenceValue       = nextBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // HellModeColorApplier（地獄モード時の配色変更）
            var hellGO = new GameObject("HellModeColorApplier");
            hellGO.transform.SetParent(canvasGO.transform, false);
            var hellApplier = hellGO.AddComponent<HellModeColorApplier>();
            var hellSO = new SerializedObject(hellApplier);
            hellSO.FindProperty("mainCamera").objectReferenceValue = camera;
            hellSO.FindProperty("backgroundImage").objectReferenceValue = bgGO.GetComponent<Image>();
            hellSO.FindProperty("lemonPatternRoot").objectReferenceValue = canvasGO.transform.Find("LemonPattern");
            Sprite limeSprite = null; { var _la = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/UI/lime.png"); foreach (var _a in _la) if (_a is Sprite _s) { limeSprite = _s; break; } }
            if (limeSprite != null) hellSO.FindProperty("limeSprite").objectReferenceValue = limeSprite;
            hellSO.ApplyModifiedProperties();

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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/NumberConfirm.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/NumberConfirm.unity");

            Debug.Log("[NumberConfirmSceneBuilder] NumberConfirm シーンを作成しました");
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

        // ── プレイヤー名チップ（表示専用）────────────────────────────

        static TextMeshProUGUI MakePlayerNameChip(Transform parent, string name,
            Vector2 pos, Color chipColor, Color textColor, TMP_FontAsset font)
        {
            var chipGO = new GameObject(name, typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var cr = chipGO.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(860f, 110f);
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

        static Sprite EnsureSprite(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[NumberConfirmBuilder] Not found: {path}"); return null; }
            if (ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[NumberConfirmBuilder] Sprite not found: {path}"); return null; }
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
                tmp.fontSizeMin = autoSizeMin;
                tmp.fontSizeMax = autoSizeMax;
            }
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

        static Sprite GetOrCreateCircleSprite()
        {
            const string path = "Assets/Sprites/UI/circle_white.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.5f;
            var center = new Vector2(r - 0.5f, r - 0.5f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 1f - Mathf.Clamp01(dist - r + 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            tex.Apply();

            System.IO.Directory.CreateDirectory("Assets/Sprites/UI");
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
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
