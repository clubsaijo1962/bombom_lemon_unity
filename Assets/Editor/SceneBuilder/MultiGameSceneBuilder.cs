using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Multiplayer;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// 協力モード：ゲーム進行画面のシーンビルダー
    /// 回答者・最終決定者・予想者の3役に対応した UI を構築する。
    /// </summary>
    public static class MultiGameSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnGreen     = new(0.25f, 0.72f, 0.35f);
        static readonly Color BtnRed       = new(0.85f, 0.22f, 0.15f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color TopicBg      = new(0.98f, 0.96f, 0.85f, 0.90f);
        static readonly Color AnswerBg     = new(0.94f, 0.97f, 1.00f, 0.90f);
        static readonly Color InputBg      = new(1.00f, 1.00f, 1.00f, 0.90f);
        static readonly Color SlotMe       = new(1.00f, 0.97f, 0.85f, 0.92f);
        static readonly Color SlotOther    = new(0.96f, 0.96f, 0.96f, 0.55f);

        public static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

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

            var jpFont    = FindJapaneseTMPFont();
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite  = GetBuiltinUISprite();
            var lemonSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                            ?? FindSprite("Lemon");

            BuildLemonPattern(canvasGO.transform, lemonSpr, 0.06f);

            // Panel (fade コンテナ)
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── カード: 1020×1760, center y=20 ──
            // card local: top=+880, bottom=-880
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color  = CardColor; cardImg.raycastTarget = false;
            var shadow = cardGO.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot     = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(1020f, 1760f);
            cardR.anchoredPosition = new Vector2(0f, 20f);

            // ── タイトル行 ──
            MakeLabel(cardGO.transform, "Title", "協力モード ゲーム",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 840f), new Vector2(960f, 64f),
                52f, TextPrimary, FontStyles.Bold, jpFont);

            // ── 自分の役割バッジ ──
            var myRoleTmp = MakeLabel(cardGO.transform, "MyRole", "🎯 あなたは予想者",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 782f), new Vector2(860f, 48f),
                38f, TextMuted, FontStyles.Bold, jpFont);

            MakeSeparator(cardGO.transform, 750f);

            // ── 役割発表 ──
            var answererNameTmp = MakeLabel(cardGO.transform, "AnswererName", "回答者：〇〇",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 712f), new Vector2(920f, 46f),
                36f, TextPrimary, FontStyles.Normal, jpFont);

            var deciderNameTmp = MakeLabel(cardGO.transform, "DeciderName", "最終決定者：△△",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 662f), new Vector2(920f, 46f),
                36f, TextPrimary, FontStyles.Normal, jpFont);

            // ── ガイドテキスト ──
            var guideTmp = MakeLabel(cardGO.transform, "Guide",
                "回答を参考に、最終決定者の秘密の数字を予想しよう！",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 606f), new Vector2(940f, 72f),
                32f, TextMuted, FontStyles.Normal, jpFont);
            guideTmp.enableWordWrapping = true;
            guideTmp.enableAutoSizing   = true;
            guideTmp.fontSizeMin = 28f; guideTmp.fontSizeMax = 32f;

            MakeSeparator(cardGO.transform, 560f);

            // ── お題セクション ──
            var topicHeaderTmp = MakeLabel(cardGO.transform, "TopicHeader", "お題",
                new Vector2(0.5f, 0.5f), new Vector2(-380f, 520f), new Vector2(160f, 44f),
                36f, TextMuted, FontStyles.Bold, jpFont);
            topicHeaderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // お題テキストボックス
            var topicBgGO = MakeBox(cardGO.transform, "TopicBox", TopicBg, uiSprite,
                new Vector2(0f, 452f), new Vector2(940f, 104f));
            var topicTmp = MakeBoxText(topicBgGO.transform, "TopicLabel",
                "今日の気分を点数で表すなら", jpFont,
                36f, TextPrimary, 32f, 44f);

            MakeSeparator(cardGO.transform, 392f);

            // ── 回答セクション ──
            var answerHeaderTmp = MakeLabel(cardGO.transform, "AnswerHeader", "回答",
                new Vector2(0.5f, 0.5f), new Vector2(-380f, 352f), new Vector2(160f, 44f),
                36f, TextMuted, FontStyles.Bold, jpFont);
            answerHeaderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // 回答表示ボックス（ポーリングで更新）
            var answerBgGO = MakeBox(cardGO.transform, "AnswerDisplayBox", AnswerBg, uiSprite,
                new Vector2(0f, 278f), new Vector2(940f, 120f));
            var answerDisplayTmp = MakeBoxText(answerBgGO.transform, "AnswerDisplay",
                "(まだ回答がありません)", jpFont,
                36f, TextMuted, 28f, 40f);

            // ── 回答者入力パネル（AnswererPanel: stretch over card） ──
            var answererPanelGO = new GameObject("AnswererPanel", typeof(RectTransform));
            answererPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(answererPanelGO.GetComponent<RectTransform>());
            answererPanelGO.SetActive(false); // コントローラーが役割に応じて表示

            var answerInputField = MakeInputField(answererPanelGO.transform, "AnswerInput",
                "回答を入力...",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(940f, 80f),
                36f, jpFont, uiSprite, TMP_InputField.ContentType.Standard);
            answerInputField.lineType = TMP_InputField.LineType.MultiLineSubmit;

            var submitAnswerBtnGO = MakeButton(answererPanelGO.transform, "SubmitAnswerBtn",
                "回答を送る ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(360f, 80f),
                BtnGreen, Color.white, 40f, jpFont, btnYellow);

            MakeSeparator(cardGO.transform, 14f);

            // ── 予想セクション ──
            var predHeaderTmp = MakeLabel(cardGO.transform, "GuessSectionHeader",
                "みんなの予想",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(940f, 52f),
                44f, TextPrimary, FontStyles.Bold, jpFont);

            // ── ScrollRect（予想リスト）──
            var scrollGO = new GameObject("GuessListScroll", typeof(RectTransform));
            scrollGO.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin        = new Vector2(0f, 0.5f);
            scrollR.anchorMax        = new Vector2(1f, 0.5f);
            scrollR.pivot            = new Vector2(0.5f, 1f);
            scrollR.sizeDelta        = new Vector2(-40f, 520f);
            scrollR.anchoredPosition = new Vector2(0f, -66f);
            var sr = scrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false;

            // Viewport
            var vpGO = new GameObject("Viewport", typeof(RectTransform));
            vpGO.transform.SetParent(scrollGO.transform, false);
            StretchFull(vpGO.GetComponent<RectTransform>());
            vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            vpGO.AddComponent<Mask>().showMaskGraphic = false;
            sr.viewport = vpGO.GetComponent<RectTransform>();

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(vpGO.transform, false);
            var contentR = contentGO.GetComponent<RectTransform>();
            contentR.anchorMin        = new Vector2(0f, 1f);
            contentR.anchorMax        = new Vector2(1f, 1f);
            contentR.pivot            = new Vector2(0.5f, 1f);
            contentR.sizeDelta        = new Vector2(0f, 0f);
            contentR.anchoredPosition = Vector2.zero;
            sr.content = contentR;

            // ── 予想者入力パネル（GuesserPanel: stretch over card）──
            var guesserPanelGO = new GameObject("GuesserPanel", typeof(RectTransform));
            guesserPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(guesserPanelGO.GetComponent<RectTransform>());
            guesserPanelGO.SetActive(false);

            var guessInputField = MakeInputField(guesserPanelGO.transform, "GuessInput",
                "予想の数字 (1〜99)",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -630f), new Vector2(560f, 80f),
                40f, jpFont, uiSprite, TMP_InputField.ContentType.IntegerNumber);

            var submitGuessBtnGO = MakeButton(guesserPanelGO.transform, "SubmitGuessBtn",
                "予想を送る ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -724f), new Vector2(360f, 80f),
                BtnPrimary, TextPrimary, 40f, jpFont, btnYellow);

            // ── 最終決定者パネル（DeciderPanel: stretch over card）──
            var deciderPanelGO = new GameObject("DeciderPanel", typeof(RectTransform));
            deciderPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(deciderPanelGO.GetComponent<RectTransform>());
            deciderPanelGO.SetActive(false);

            var secretInputField = MakeInputField(deciderPanelGO.transform, "SecretInput",
                "秘密の数字",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -630f), new Vector2(560f, 80f),
                40f, jpFont, uiSprite, TMP_InputField.ContentType.IntegerNumber);

            var finalDecideBtnGO = MakeButton(deciderPanelGO.transform, "FinalDecideBtn",
                "最終決定 ✓",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -724f), new Vector2(440f, 88f),
                BtnRed, Color.white, 44f, jpFont, btnYellow);

            // ── 自分の情報バー（常時表示・上部固定）──
            var myInfoBarGO = new GameObject("MyInfoBar", typeof(RectTransform));
            myInfoBarGO.transform.SetParent(panelGO.transform, false);
            var barImg = myInfoBarGO.AddComponent<Image>();
            barImg.color = new Color(0.20f, 0.10f, 0.02f, 0.82f);
            barImg.raycastTarget = false;
            var barR = myInfoBarGO.GetComponent<RectTransform>();
            barR.anchorMin = new Vector2(0f, 1f);
            barR.anchorMax = new Vector2(1f, 1f);
            barR.pivot     = new Vector2(0.5f, 1f);
            barR.sizeDelta = new Vector2(0f, 96f);
            barR.anchoredPosition = Vector2.zero;

            // ラウンド表示（左）
            var roundLabelTmp = MakeLabel(myInfoBarGO.transform, "RoundLabel", "ラウンド 1/1",
                new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(300f, 64f),
                34f, Color.white, FontStyles.Bold, jpFont);
            roundLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // 自分のお題（中央）
            var myTopicLabelTmp = MakeLabel(myInfoBarGO.transform, "MyTopicLabel", "お題：---",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(440f, 64f),
                32f, new Color(1f, 0.94f, 0.70f), FontStyles.Normal, jpFont);
            myTopicLabelTmp.alignment = TextAlignmentOptions.Midline;
            myTopicLabelTmp.enableWordWrapping = false;
            myTopicLabelTmp.overflowMode = TextOverflowModes.Ellipsis;

            // 自分の秘密の数字（右）
            var mySecretLabelTmp = MakeLabel(myInfoBarGO.transform, "MySecretLabel", "🔒 --",
                new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(200f, 64f),
                36f, new Color(1f, 0.88f, 0.30f), FontStyles.Bold, jpFont);
            mySecretLabelTmp.alignment = TextAlignmentOptions.MidlineRight;

            // ── ScreenFade ──
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;

            // ── MultiGameController ──
            var ctrlGO = new GameObject("MultiGameController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<MultiGameController>();
            var so   = new SerializedObject(ctrl);

            // お題・役割表示
            so.FindProperty("topicLabel").objectReferenceValue        = topicTmp;
            so.FindProperty("answererNameLabel").objectReferenceValue = answererNameTmp;
            so.FindProperty("deciderNameLabel").objectReferenceValue  = deciderNameTmp;
            so.FindProperty("guideLabel").objectReferenceValue        = guideTmp;
            so.FindProperty("myRoleLabel").objectReferenceValue       = myRoleTmp;

            // 回答表示
            so.FindProperty("answerDisplayLabel").objectReferenceValue = answerDisplayTmp;

            // 回答者パネル
            so.FindProperty("answererPanel").objectReferenceValue =
                answererPanelGO;
            so.FindProperty("answerInputField").objectReferenceValue  = answerInputField;
            so.FindProperty("submitAnswerBtn").objectReferenceValue   =
                submitAnswerBtnGO.GetComponent<Button>();
            so.FindProperty("submitAnswerBtnLabel").objectReferenceValue =
                submitAnswerBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // 予想リスト
            so.FindProperty("guessListContent").objectReferenceValue  = contentR;
            so.FindProperty("listFont").objectReferenceValue          = jpFont;
            so.FindProperty("guessSectionHeader").objectReferenceValue = predHeaderTmp;

            // 予想者パネル
            so.FindProperty("guesserPanel").objectReferenceValue =
                guesserPanelGO;
            so.FindProperty("guessInputField").objectReferenceValue   = guessInputField;
            so.FindProperty("submitGuessBtn").objectReferenceValue    =
                submitGuessBtnGO.GetComponent<Button>();
            so.FindProperty("submitGuessBtnLabel").objectReferenceValue =
                submitGuessBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // 最終決定者パネル
            so.FindProperty("deciderPanel").objectReferenceValue =
                deciderPanelGO;
            so.FindProperty("secretInputField").objectReferenceValue  = secretInputField;
            so.FindProperty("finalDecideBtn").objectReferenceValue    =
                finalDecideBtnGO.GetComponent<Button>();
            so.FindProperty("finalDecideBtnLabel").objectReferenceValue =
                finalDecideBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // 自分の情報バー
            so.FindProperty("roundLabel").objectReferenceValue    = roundLabelTmp;
            so.FindProperty("myTopicLabel").objectReferenceValue  = myTopicLabelTmp;
            so.FindProperty("mySecretLabel").objectReferenceValue = mySecretLabelTmp;

            // フェード
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.FindProperty("panelGroup").objectReferenceValue = panelCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                "Assets/Scenes/MultiGame.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/MultiGame.unity");
            Debug.Log("[MultiGameSceneBuilder] MultiGame シーンを作成しました");
        }

        // ── ユーティリティ ────────────────────────────────────────────────────

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

        static GameObject MakeBox(Transform parent, string name, Color color, Sprite sprite,
            Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite; img.type = Image.Type.Sliced;
            img.color = color; img.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            return go;
        }

        static TextMeshProUGUI MakeBoxText(Transform parent, string name, string text,
            TMP_FontAsset font, float fontSize, Color color, float minSize, float maxSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(20f, 8f); r.offsetMax = new Vector2(-20f, -8f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Midline;
            tmp.color = color; tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            tmp.enableAutoSizing   = true;
            tmp.fontSizeMin = minSize; tmp.fontSizeMax = maxSize;
            if (font != null) tmp.font = font;
            return tmp;
        }

        static TMP_InputField MakeInputField(Transform parent, string name, string placeholder,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, TMP_FontAsset font, Sprite bgSprite,
            TMP_InputField.ContentType contentType)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            bg.sprite = bgSprite ?? GetBuiltinUISprite();
            bg.type   = Image.Type.Sliced;
            bg.color  = InputBg;

            var field = go.AddComponent<TMP_InputField>();

            // TextArea
            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(14f, 4f); taR.offsetMax = new Vector2(-14f, -4f);
            taGO.AddComponent<RectMask2D>();
            field.textViewport = taR;

            // Placeholder
            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(taGO.transform, false);
            var phR = phGO.GetComponent<RectTransform>();
            phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
            phR.offsetMin = phR.offsetMax = Vector2.zero;
            var phTmp = phGO.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder; phTmp.fontSize = fontSize;
            phTmp.color = new Color(0.45f, 0.28f, 0.08f, 0.45f);
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;
            phTmp.enableWordWrapping = false; phTmp.raycastTarget = false;
            if (font != null) phTmp.font = font;
            field.placeholder = phTmp;

            // Text
            var tGO = new GameObject("Text", typeof(RectTransform));
            tGO.transform.SetParent(taGO.transform, false);
            var tR = tGO.GetComponent<RectTransform>();
            tR.anchorMin = Vector2.zero; tR.anchorMax = Vector2.one;
            tR.offsetMin = tR.offsetMax = Vector2.zero;
            var tTmp = tGO.AddComponent<TextMeshProUGUI>();
            tTmp.fontSize = fontSize;
            tTmp.color    = TextPrimary;
            tTmp.alignment = TextAlignmentOptions.MidlineLeft;
            tTmp.enableWordWrapping = false; tTmp.raycastTarget = false;
            if (font != null) tTmp.font = font;
            field.textComponent = tTmp;

            field.contentType    = contentType;
            field.characterLimit = contentType == TMP_InputField.ContentType.IntegerNumber ? 2 : 120;
            field.lineType       = TMP_InputField.LineType.SingleLine;

            return field;
        }

        static void BuildLemonPattern(Transform parent, Sprite lemon, float alpha)
        {
            if (lemon == null) return;
            var p = new GameObject("LemonPattern", typeof(RectTransform));
            p.transform.SetParent(parent, false);
            StretchFull(p.GetComponent<RectTransform>());
            for (int row = 0; row < 10; row++)
            {
                float y  = 960f - row * 220f;
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
                    r.localRotation    = Quaternion.Euler(0f, 0f, -22f);
                    var img = go.AddComponent<Image>();
                    img.sprite = lemon; img.preserveAspect = true; img.raycastTarget = false;
                    img.color  = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

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
            bg.sprite = btnSprite ?? GetBuiltinUISprite();
            bg.type   = Image.Type.Sliced; bg.color = bgColor;
            var btn  = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor      = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
            cols.disabledColor    = new Color(0.7f, 0.7f, 0.7f, 0.5f);
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

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[MultiGameBuilder] Not found: {path}"); return null; }
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
                var s    = AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            var all = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (all.Length > 0)
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    AssetDatabase.GUIDToAssetPath(all[0]));
            return null;
        }
    }
}
