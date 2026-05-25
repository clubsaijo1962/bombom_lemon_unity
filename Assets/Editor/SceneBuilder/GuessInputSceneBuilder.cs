using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class GuessInputSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);        // レモンイエロー背景
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f); // ウォームホワイトカード
        static readonly Color LemonYellow  = new(0.97f, 0.83f, 0.18f);        // レモンイエロー（アクセント）
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);        // ゴールデンイエロー（メインCTA）
        static readonly Color BtnSecondary = new(0.99f, 0.95f, 0.72f);        // ペールレモン（サブボタン）
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);        // ダークブラウン
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f); // ミディアムブラウン
        static readonly Color ChipAlt      = new(0.99f, 0.95f, 0.72f);        // ペールレモン
        static readonly Color ChipGuesser  = new(0.99f, 0.95f, 0.72f);        // ペールレモン（統一）
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f); // ゴールデンライン

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

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();

            var btnYellow   = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite    = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite  = FindSprite("card");
            if (cardSprite == null)
            {
                var cardTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/card.svg");
                if (cardTex != null)
                    cardSprite = Sprite.Create(cardTex, new Rect(0, 0, cardTex.width, cardTex.height), new Vector2(0.5f, 0.5f));
            }
            if (cardSprite == null) Debug.LogWarning("[GuessInputBuilder] card sprite not found");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

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

            // HUD 右上
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

            // HOME ボタン（左上）
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton",
                "HOME",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(200f, 80f),
                BtnSecondary, TextMuted, 34f, jpFont, btnYellow);

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

            // ── レイアウト（均等スペーシング）──
            // 上セクション: card top(720)〜Sep1(342)=378px、要素244px、4gap×33px
            //   TopicHeader(664) TopicText(535) LowHighRow(403) Sep1(342)
            // 下セクション: Sep1(342)〜card bottom(-540)=882px、要素566px、7gap×45px
            //   HelpBtn(260) Header(149) Chip(4) Sep2(-112) InputLabel(-184) InputField(-375)

            // ─ ルール説明（カード枠の外・黄色背景上）─
            MakeLabel(panelGO.transform, "RuleMessage",
                "回答者：数字を言わず、お題に合う回答を！\nみんな：秘密の数字を予想しよう！",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 785f), new Vector2(940f, 80f),
                32f, TextMuted, FontStyles.Normal, jpFont,
                autoSizeMin: 32f, autoSizeMax: 36f);

            // ─ お題セクション ─
            MakeLabel(panelGO.transform, "TopicHeader",
                "お題",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 640f), new Vector2(900f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            var topicLabelTmp = MakeLabel(panelGO.transform, "TopicLabel",
                "お題テキスト",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 511f), new Vector2(900f, 100f),
                40f, TextPrimary, FontStyles.Bold, jpFont,
                autoSizeMin: 32f, autoSizeMax: 48f);
            topicLabelTmp.enableWordWrapping = false;
            topicLabelTmp.overflowMode = TextOverflowModes.Overflow;

            // Low / High ラベル（左右）
            TextMeshProUGUI topicLowTmp, topicHighTmp;
            {
                var rowGO = new GameObject("LowHighRow", typeof(RectTransform));
                rowGO.transform.SetParent(panelGO.transform, false);
                var rr = rowGO.GetComponent<RectTransform>();
                rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
                rr.pivot = new Vector2(0.5f, 0.5f);
                rr.sizeDelta = new Vector2(900f, 52f);
                rr.anchoredPosition = new Vector2(0f, 379f);

                topicLowTmp = MakeLabelInParent(rowGO.transform, "LowLabel", "1 = 低い",
                    new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero,
                    32f, TextMuted, FontStyles.Normal, jpFont, TextAlignmentOptions.MidlineLeft);

                topicHighTmp = MakeLabelInParent(rowGO.transform, "HighLabel", "99 = 高い",
                    new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero,
                    32f, TextMuted, FontStyles.Normal, jpFont, TextAlignmentOptions.MidlineRight);
            }

            MakeSeparator(panelGO.transform, 312f);

            var helpBtnGO = MakeButton(panelGO.transform, "HelpButton",
                "? ヒントを見る",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 230f), new Vector2(320f, 72f),
                BtnSecondary, TextMuted, 32f, jpFont, btnYellow);

            MakeLabel(panelGO.transform, "FinalGuesserHeader",
                "予想の最終決定者",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 119f), new Vector2(900f, 60f),
                36f, TextPrimary, FontStyles.Bold, jpFont);

            var finalGuesserTmp = MakeNameChip(panelGO.transform, "FinalGuesserChip",
                new Vector2(0f, -26f), ChipGuesser, TextPrimary, jpFont, uiSprite, h: 140f);

            MakeSeparator(panelGO.transform, -142f);

            MakeLabel(panelGO.transform, "InputHeader",
                "予想する数字（1〜99）",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -214f), new Vector2(860f, 52f),
                32f, TextMuted, FontStyles.Bold, jpFont);

            var inputField = MakeNumberInputField(panelGO.transform, "NumberInputField",
                new Vector2(0f, -365f), new Vector2(560f, 240f), jpFont, uiSprite, btnYellow);

            // 確定ボタン：カード外・NumberConfirmと同位置 y=-790
            var confirmBtnGO = MakeButton(panelGO.transform, "ConfirmButton",
                "確定 ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -790f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 46f, jpFont, btnYellow);

            // ── ヒントパネル（モーダル、初期非表示）──
            var examplesPanelGO = new GameObject("ExamplesPanel", typeof(RectTransform));
            examplesPanelGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(examplesPanelGO.GetComponent<RectTransform>());
            var examplesCG = examplesPanelGO.AddComponent<CanvasGroup>();
            examplesCG.alpha = 0f; examplesCG.blocksRaycasts = false;

            // 暗幕オーバーレイ
            var overlayGO = new GameObject("Overlay", typeof(RectTransform));
            overlayGO.transform.SetParent(examplesPanelGO.transform, false);
            StretchFull(overlayGO.GetComponent<RectTransform>());
            overlayGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            // モーダルカード
            var modalGO = new GameObject("ModalCard", typeof(RectTransform));
            modalGO.transform.SetParent(examplesPanelGO.transform, false);
            var modalImg = modalGO.AddComponent<Image>();
            modalImg.sprite = uiSprite; modalImg.type = Image.Type.Sliced;
            modalImg.color = new Color(1f, 0.98f, 0.94f, 1f);
            var modalSh = modalGO.AddComponent<Shadow>();
            modalSh.effectColor = new Color(0.15f, 0.08f, 0f, 0.40f);
            modalSh.effectDistance = new Vector2(0f, -16f);
            var mr = modalGO.GetComponent<RectTransform>();
            mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 0.5f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(920f, 600f);
            mr.anchoredPosition = Vector2.zero;

            // タイトル "具体例"
            MakeLabel(modalGO.transform, "ModalTitle",
                "具体例",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(880f, 60f),
                40f, TextPrimary, FontStyles.Bold, jpFont);

            MakeSeparator(modalGO.transform, 196f);

            // 低い数字の例
            MakeLabel(modalGO.transform, "LowHeader",
                "低い数字の例",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 148f), new Vector2(820f, 44f),
                32f, TextMuted, FontStyles.Bold, jpFont, align: TextAlignmentOptions.Left);

            var hintLowTmp = MakeLabel(modalGO.transform, "HintLowLabel",
                "（具体例）",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 72f), new Vector2(820f, 90f),
                36f, TextPrimary, FontStyles.Normal, jpFont,
                autoSizeMin: 32f, autoSizeMax: 40f);

            MakeSeparator(modalGO.transform, 18f);

            // 高い数字の例
            MakeLabel(modalGO.transform, "HighHeader",
                "高い数字の例",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(820f, 44f),
                32f, TextMuted, FontStyles.Bold, jpFont, align: TextAlignmentOptions.Left);

            var hintHighTmp = MakeLabel(modalGO.transform, "HintHighLabel",
                "（具体例）",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -106f), new Vector2(820f, 90f),
                36f, TextPrimary, FontStyles.Normal, jpFont,
                autoSizeMin: 32f, autoSizeMax: 40f);

            // 閉じるボタン
            var closeBtnGO = MakeButton(modalGO.transform, "CloseButton",
                "× 閉じる",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -240f), new Vector2(340f, 80f),
                BtnSecondary, TextMuted, 36f, jpFont, btnYellow);

            // コントローラー
            var ctrlGO = new GameObject("GuessInputController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<GuessInputController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("topicLabel").objectReferenceValue         = topicLabelTmp;
            so.FindProperty("topicLowLabel").objectReferenceValue      = topicLowTmp;
            so.FindProperty("topicHighLabel").objectReferenceValue     = topicHighTmp;
            so.FindProperty("roundLabel").objectReferenceValue         = roundLabelTmp;
            so.FindProperty("finalGuesserLabel").objectReferenceValue  = finalGuesserTmp;
            so.FindProperty("lifeCountLabel").objectReferenceValue     = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("numberInputField").objectReferenceValue   = inputField;
            so.FindProperty("confirmButton").objectReferenceValue      = confirmBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue         = homeBtnGO.GetComponent<Button>();
            so.FindProperty("helpButton").objectReferenceValue         = helpBtnGO.GetComponent<Button>();
            so.FindProperty("examplesPanel").objectReferenceValue      = examplesCG;
            so.FindProperty("hintLowLabel").objectReferenceValue       = hintLowTmp;
            so.FindProperty("hintHighLabel").objectReferenceValue      = hintHighTmp;
            so.FindProperty("closeButton").objectReferenceValue        = closeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue         = panelCG;

            // HellModeColorApplier（地獄モード時の配色変更）
            var hellGO = new GameObject("HellModeColorApplier");
            hellGO.transform.SetParent(canvasGO.transform, false);
            var hellApplier = hellGO.AddComponent<HellModeColorApplier>();
            var hellSO = new SerializedObject(hellApplier);
            hellSO.FindProperty("lemonPatternRoot").objectReferenceValue = canvasGO.transform.Find("LemonPattern");
            var limeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/lime.png");
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
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GuessInput.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/GuessInput.unity");

            Debug.Log("[GuessInputSceneBuilder] GuessInput シーンを作成しました");
        }

        // ── TMP_InputField（数字入力）────────────────────────────────

        static TMP_InputField MakeNumberInputField(Transform parent, string name,
            Vector2 pos, Vector2 size, TMP_FontAsset font, Sprite uiSpr, Sprite btnSpr)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (btnSpr != null) { bg.sprite = btnSpr; bg.type = Image.Type.Sliced; }
            else { bg.sprite = uiSpr; bg.type = Image.Type.Sliced; }
            bg.color = new Color(1f, 0.96f, 0.88f);

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.40f);
            shadow.effectDistance = new Vector2(0f, -10f);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic = bg;
            inputField.characterLimit = 2;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(16f, 6f); taR.offsetMax = new Vector2(-16f, -6f);

            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(taGO.transform, false);
            var phR = phGO.GetComponent<RectTransform>();
            phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
            phR.offsetMin = phR.offsetMax = Vector2.zero;
            var phTmp = phGO.AddComponent<TextMeshProUGUI>();
            phTmp.text = "??";
            phTmp.fontSize = 80f;
            phTmp.fontStyle = FontStyles.Bold;
            phTmp.color = new Color(0.20f, 0.10f, 0.02f, 0.30f);
            phTmp.alignment = TextAlignmentOptions.Center;
            phTmp.raycastTarget = false;
            if (font != null) phTmp.font = font;

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 80f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.color = TextPrimary;
            txtTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) txtTmp.font = font;

            inputField.textViewport  = taR;
            inputField.textComponent = txtTmp;
            inputField.placeholder   = phTmp;

            return inputField;
        }

        // ── 名前チップ ──────────────────────────────────────────────

        static TextMeshProUGUI MakeNameChip(Transform parent, string name,
            Vector2 pos, Color chipColor, Color textColor, TMP_FontAsset font, Sprite uiSpr, float h = 110f)
        {
            var chipGO = new GameObject(name, typeof(RectTransform));
            chipGO.transform.SetParent(parent, false);
            var cr = chipGO.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(860f, h);
            cr.anchoredPosition = pos;

            var bg = chipGO.AddComponent<Image>();
            bg.sprite = uiSpr; bg.type = Image.Type.Sliced;
            bg.color = chipColor; bg.raycastTarget = false;

            var sh = chipGO.AddComponent<Shadow>();
            sh.effectColor = new Color(0.22f, 0.14f, 0.02f, 0.22f);
            sh.effectDistance = new Vector2(0f, -8f);

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

        // ── セパレーター ────────────────────────────────────────────

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

        // ── レモン透かし ────────────────────────────────────────────

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
            if (ti == null) { Debug.LogWarning($"[GuessInputBuilder] Not found: {path}"); return null; }
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
            if (ti == null) { Debug.LogWarning($"[GuessInputBuilder] Sprite not found: {path}"); return null; }
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
            tmp.alignment = align;
            tmp.color = color; tmp.raycastTarget = false;
            if (autoSizeMin > 0f && autoSizeMax > 0f)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = autoSizeMin; tmp.fontSizeMax = autoSizeMax;
            }
            if (font != null) tmp.font = font;
            return tmp;
        }

        static TextMeshProUGUI MakeLabelInParent(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin; r.anchorMax = anchorMax;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.offsetMin = offsetMin; r.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = alignment;
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
