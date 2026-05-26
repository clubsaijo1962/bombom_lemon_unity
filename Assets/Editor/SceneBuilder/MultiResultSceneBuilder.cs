using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Multiplayer;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// 協力モード：ラウンド結果画面のシーンビルダー。
    /// 全員の予想・差・実際の数字を表示し、ヘルプカード使用・次ラウンド進行を担う。
    /// </summary>
    public static class MultiResultSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnGreen     = new(0.25f, 0.72f, 0.35f);
        static readonly Color BtnRed       = new(0.85f, 0.22f, 0.15f);
        static readonly Color BtnGray      = new(0.55f, 0.55f, 0.55f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color TopicBg      = new(0.98f, 0.96f, 0.85f, 0.90f);
        static readonly Color NumberBg     = new(0.97f, 0.82f, 0.10f);
        static readonly Color MyCardBg     = new(1.00f, 0.96f, 0.80f, 0.92f);
        static readonly Color OverlayBg    = new(0.10f, 0.08f, 0.02f, 0.85f);
        static readonly Color DialogBg     = new(1.00f, 0.99f, 0.95f, 0.98f);

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

            // Panel
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── 自分の秘密バー（上部固定） ──
            // card local top-anchored bar
            var myBarGO = new GameObject("MySecretBar", typeof(RectTransform));
            myBarGO.transform.SetParent(panelGO.transform, false);
            var myBarR = myBarGO.GetComponent<RectTransform>();
            myBarR.anchorMin = new Vector2(0f, 1f);
            myBarR.anchorMax = new Vector2(1f, 1f);
            myBarR.pivot     = new Vector2(0.5f, 1f);
            myBarR.sizeDelta = new Vector2(0f, 88f);
            myBarR.anchoredPosition = Vector2.zero;
            var myBarImg = myBarGO.AddComponent<Image>();
            myBarImg.color = MyCardBg;
            myBarImg.raycastTarget = false;

            // My Secret Bar: RoundLabel (left), Topic (center), Secret (right)
            var roundBarTmp = MakeLabel(myBarGO.transform, "RoundLabel", "ラウンド 1 / 4",
                new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(250f, 60f),
                28f, TextMuted, FontStyles.Bold, jpFont);
            roundBarTmp.rectTransform.anchorMin = new Vector2(0f, 0f);
            roundBarTmp.rectTransform.anchorMax = new Vector2(0f, 1f);
            roundBarTmp.rectTransform.offsetMin = new Vector2(14f, 4f);
            roundBarTmp.rectTransform.offsetMax = new Vector2(200f, -4f);
            roundBarTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var myTopicBarTmp = MakeLabel(myBarGO.transform, "MyTopicBar", "お題：今日の気分...",
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 60f),
                30f, TextPrimary, FontStyles.Normal, jpFont);
            myTopicBarTmp.enableWordWrapping = false;
            myTopicBarTmp.overflowMode = TextOverflowModes.Ellipsis;

            var mySecretBarTmp = MakeLabel(myBarGO.transform, "MySecretBar_Secret", "47",
                new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(160f, 60f),
                44f, TextPrimary, FontStyles.Bold, jpFont);
            mySecretBarTmp.rectTransform.anchorMin = new Vector2(1f, 0f);
            mySecretBarTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            mySecretBarTmp.rectTransform.offsetMin = new Vector2(-180f, 4f);
            mySecretBarTmp.rectTransform.offsetMax = new Vector2(-14f, -4f);
            mySecretBarTmp.alignment = TextAlignmentOptions.MidlineRight;

            // ── コンテンツカード: 1020×1580, center y=-50 ──
            // card local: top=+790, bottom=-790
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
            cardR.sizeDelta = new Vector2(1020f, 1580f);
            cardR.anchoredPosition = new Vector2(0f, -50f);

            // ── ラウンドヘッダー ──
            var roundHeaderTmp = MakeLabel(cardGO.transform, "RoundHeader", "ラウンド 1 / 4",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 750f), new Vector2(940f, 56f),
                44f, TextPrimary, FontStyles.Bold, jpFont);

            // ── 最終決定者名 ──
            var deciderNameTmp = MakeLabel(cardGO.transform, "DeciderName", "最終決定者：○○",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 694f), new Vector2(940f, 44f),
                34f, TextMuted, FontStyles.Normal, jpFont);

            MakeSeparator(cardGO.transform, 662f);

            // ── お題 ──
            var topicHeaderTmp = MakeLabel(cardGO.transform, "TopicHeader", "お題",
                new Vector2(0.5f, 0.5f), new Vector2(-390f, 622f), new Vector2(130f, 44f),
                34f, TextMuted, FontStyles.Bold, jpFont);
            topicHeaderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var topicBgGO = new GameObject("TopicBox", typeof(RectTransform));
            topicBgGO.transform.SetParent(cardGO.transform, false);
            var topicBgImg = topicBgGO.AddComponent<Image>();
            topicBgImg.sprite = uiSprite; topicBgImg.type = Image.Type.Sliced;
            topicBgImg.color = TopicBg; topicBgImg.raycastTarget = false;
            var topicBgR = topicBgGO.GetComponent<RectTransform>();
            topicBgR.anchorMin = topicBgR.anchorMax = new Vector2(0.5f, 0.5f);
            topicBgR.pivot     = new Vector2(0.5f, 0.5f);
            topicBgR.sizeDelta = new Vector2(940f, 110f);
            topicBgR.anchoredPosition = new Vector2(0f, 552f);
            var topicLblGO = new GameObject("TopicLabel", typeof(RectTransform));
            topicLblGO.transform.SetParent(topicBgGO.transform, false);
            var tlr = topicLblGO.GetComponent<RectTransform>();
            tlr.anchorMin = Vector2.zero; tlr.anchorMax = Vector2.one;
            tlr.offsetMin = new Vector2(20f, 8f); tlr.offsetMax = new Vector2(-20f, -8f);
            var topicTmp = topicLblGO.AddComponent<TextMeshProUGUI>();
            topicTmp.text = "今日の気分を点数で表すなら"; topicTmp.fontSize = 40f;
            topicTmp.fontStyle = FontStyles.Bold; topicTmp.alignment = TextAlignmentOptions.Midline;
            topicTmp.color = TextPrimary; topicTmp.raycastTarget = false;
            topicTmp.enableWordWrapping = true; topicTmp.enableAutoSizing = true;
            topicTmp.fontSizeMin = 30f; topicTmp.fontSizeMax = 40f;
            if (jpFont != null) topicTmp.font = jpFont;

            MakeSeparator(cardGO.transform, 488f);

            // ── 実際の数字 ──
            var actualHeaderTmp = MakeLabel(cardGO.transform, "ActualHeader", "実際の秘密の数字",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 444f), new Vector2(940f, 44f),
                34f, TextMuted, FontStyles.Bold, jpFont);

            // 数字表示ボックス
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
            numBoxR.sizeDelta = new Vector2(280f, 180f);
            numBoxR.anchoredPosition = new Vector2(0f, 360f);
            var numLblGO = new GameObject("NumberLabel", typeof(RectTransform));
            numLblGO.transform.SetParent(numBoxGO.transform, false);
            var nlr = numLblGO.GetComponent<RectTransform>();
            nlr.anchorMin = Vector2.zero; nlr.anchorMax = Vector2.one;
            nlr.offsetMin = nlr.offsetMax = Vector2.zero;
            var numTmp = numLblGO.AddComponent<TextMeshProUGUI>();
            numTmp.text = "47"; numTmp.fontSize = 110f; numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = TextPrimary; numTmp.raycastTarget = false;
            if (jpFont != null) numTmp.font = jpFont;

            MakeSeparator(cardGO.transform, 258f);

            // ── ライフ・ヘルプカード表示 ──
            var livesTmp = MakeLabel(cardGO.transform, "LivesLabel", "❤ ライフ：5",
                new Vector2(0.5f, 0.5f), new Vector2(-220f, 214f), new Vector2(400f, 52f),
                40f, new Color(0.85f, 0.12f, 0.12f), FontStyles.Bold, jpFont);

            var helpsTmp = MakeLabel(cardGO.transform, "HelpCardsLabel", "🃏 ヘルプカード：4",
                new Vector2(0.5f, 0.5f), new Vector2(220f, 214f), new Vector2(440f, 52f),
                36f, new Color(0.20f, 0.45f, 0.20f), FontStyles.Bold, jpFont);

            // ── 予想リストヘッダー ──
            var listHeaderTmp = MakeLabel(cardGO.transform, "ListHeader", "みんなの予想",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(940f, 52f),
                42f, TextPrimary, FontStyles.Bold, jpFont);

            // ── ScrollRect（予想リスト）──
            var scrollGO = new GameObject("GuessListScroll", typeof(RectTransform));
            scrollGO.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin        = new Vector2(0f, 0.5f);
            scrollR.anchorMax        = new Vector2(1f, 0.5f);
            scrollR.pivot            = new Vector2(0.5f, 1f);
            scrollR.sizeDelta        = new Vector2(-40f, 440f);
            scrollR.anchoredPosition = new Vector2(0f, 128f);
            var sr = scrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false;
            var vpGO = new GameObject("Viewport", typeof(RectTransform));
            vpGO.transform.SetParent(scrollGO.transform, false);
            StretchFull(vpGO.GetComponent<RectTransform>());
            vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            vpGO.AddComponent<Mask>().showMaskGraphic = false;
            sr.viewport = vpGO.GetComponent<RectTransform>();
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(vpGO.transform, false);
            var contentR = contentGO.GetComponent<RectTransform>();
            contentR.anchorMin        = new Vector2(0f, 1f);
            contentR.anchorMax        = new Vector2(1f, 1f);
            contentR.pivot            = new Vector2(0.5f, 1f);
            contentR.sizeDelta        = new Vector2(0f, 0f);
            contentR.anchoredPosition = Vector2.zero;
            sr.content = contentR;

            // ── ヘルプカードパネル（最終決定者のみ、コントローラーが表示制御）──
            var helpCardPanelGO = new GameObject("HelpCardPanel", typeof(RectTransform));
            helpCardPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(helpCardPanelGO.GetComponent<RectTransform>());
            helpCardPanelGO.SetActive(false);

            var useHelpCardBtnGO = MakeButton(helpCardPanelGO.transform, "UseHelpCardBtn",
                "ヘルプカード使用 🃏",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220f, -640f), new Vector2(480f, 88f),
                BtnGreen, Color.white, 38f, jpFont, btnYellow);

            var helpCardUsedLblTmp = MakeLabel(helpCardPanelGO.transform, "HelpCardUsedLabel",
                "✓ ヘルプカード使用済み！",
                new Vector2(0.5f, 0.5f), new Vector2(220f, -640f), new Vector2(440f, 88f),
                36f, BtnGreen, FontStyles.Bold, jpFont);
            helpCardUsedLblTmp.gameObject.SetActive(false);

            // ── ホストパネル（ホストのみ）──
            var hostPanelGO = new GameObject("HostPanel", typeof(RectTransform));
            hostPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(hostPanelGO.GetComponent<RectTransform>());
            hostPanelGO.SetActive(false);

            var nextRoundBtnGO = MakeButton(hostPanelGO.transform, "NextRoundBtn",
                "次のラウンドへ ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(220f, -640f), new Vector2(460f, 88f),
                BtnPrimary, TextPrimary, 40f, jpFont, btnYellow);

            // ── ヘルプカード変換ダイアログ（overlay + dialog card）──
            var helpConvertPanelGO = new GameObject("HelpConvertPanel", typeof(RectTransform));
            helpConvertPanelGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(helpConvertPanelGO.GetComponent<RectTransform>());
            helpConvertPanelGO.SetActive(false);

            // Overlay dim
            var dimGO = new GameObject("Dim", typeof(RectTransform));
            dimGO.transform.SetParent(helpConvertPanelGO.transform, false);
            StretchFull(dimGO.GetComponent<RectTransform>());
            dimGO.AddComponent<Image>().color = OverlayBg;

            // Dialog card
            var dialogGO = new GameObject("DialogCard", typeof(RectTransform));
            dialogGO.transform.SetParent(helpConvertPanelGO.transform, false);
            var dialogImg = dialogGO.AddComponent<Image>();
            dialogImg.sprite = uiSprite; dialogImg.type = Image.Type.Sliced;
            dialogImg.color = DialogBg;
            var dialogR = dialogGO.GetComponent<RectTransform>();
            dialogR.anchorMin = dialogR.anchorMax = new Vector2(0.5f, 0.5f);
            dialogR.pivot = new Vector2(0.5f, 0.5f);
            dialogR.sizeDelta = new Vector2(900f, 500f);
            dialogR.anchoredPosition = Vector2.zero;

            var helpConvertLblTmp = MakeLabel(dialogGO.transform, "HelpConvertLabel",
                "最終ラウンド！\nヘルプカードをライフに変換しますか？",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(860f, 200f),
                36f, TextPrimary, FontStyles.Bold, jpFont);
            helpConvertLblTmp.enableWordWrapping = true;

            var convertBtnGO = MakeButton(dialogGO.transform, "ConvertBtn",
                "変換する ✓",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-190f, -120f), new Vector2(340f, 80f),
                BtnGreen, Color.white, 40f, jpFont, btnYellow);

            var skipConvertBtnGO = MakeButton(dialogGO.transform, "SkipConvertBtn",
                "変換しない",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(190f, -120f), new Vector2(340f, 80f),
                BtnGray, Color.white, 40f, jpFont, btnYellow);

            // ── ゲームクリア オーバーレイ ──
            var gameClearOverlayGO = new GameObject("GameClearOverlay", typeof(RectTransform));
            gameClearOverlayGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(gameClearOverlayGO.GetComponent<RectTransform>());
            gameClearOverlayGO.SetActive(false);
            var gcDimGO = new GameObject("Dim", typeof(RectTransform));
            gcDimGO.transform.SetParent(gameClearOverlayGO.transform, false);
            StretchFull(gcDimGO.GetComponent<RectTransform>());
            gcDimGO.AddComponent<Image>().color = new Color(0.05f, 0.35f, 0.05f, 0.90f);
            var gameClearLblTmp = MakeLabel(gameClearOverlayGO.transform, "GameClearLabel",
                "🎉 ゲームクリア！ 🎉",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(980f, 200f),
                72f, Color.white, FontStyles.Bold, jpFont);
            var gameClearBackBtnGO = MakeButton(gameClearOverlayGO.transform, "GameClearBackBtn",
                "ロビーへ戻る",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -100f), new Vector2(500f, 100f),
                BtnPrimary, TextPrimary, 44f, jpFont, btnYellow);

            // ── ゲームオーバー オーバーレイ ──
            var gameOverOverlayGO = new GameObject("GameOverOverlay", typeof(RectTransform));
            gameOverOverlayGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(gameOverOverlayGO.GetComponent<RectTransform>());
            gameOverOverlayGO.SetActive(false);
            var goDimGO = new GameObject("Dim", typeof(RectTransform));
            goDimGO.transform.SetParent(gameOverOverlayGO.transform, false);
            StretchFull(goDimGO.GetComponent<RectTransform>());
            goDimGO.AddComponent<Image>().color = new Color(0.35f, 0.05f, 0.05f, 0.90f);
            var gameOverLblTmp = MakeLabel(gameOverOverlayGO.transform, "GameOverLabel",
                "もう少しだった...",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(980f, 160f),
                60f, Color.white, FontStyles.Bold, jpFont);
            var gameOverBackBtnGO = MakeButton(gameOverOverlayGO.transform, "GameOverBackBtn",
                "ロビーへ戻る",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -100f), new Vector2(500f, 100f),
                BtnGray, Color.white, 44f, jpFont, btnYellow);

            // ── ScreenFade ──
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;

            // ── MultiResultController ──
            var ctrlGO = new GameObject("MultiResultController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<MultiResultController>();
            var so   = new SerializedObject(ctrl);

            // ラウンド情報
            so.FindProperty("roundHeaderLabel").objectReferenceValue = roundHeaderTmp;
            so.FindProperty("topicLabel").objectReferenceValue       = topicTmp;
            so.FindProperty("actualNumberLabel").objectReferenceValue= numTmp;
            so.FindProperty("deciderNameLabel").objectReferenceValue  = deciderNameTmp;

            // 自分の秘密
            so.FindProperty("myTopicLabel").objectReferenceValue  = myTopicBarTmp;
            so.FindProperty("mySecretLabel").objectReferenceValue = mySecretBarTmp;

            // 予想リスト
            so.FindProperty("guessListContent").objectReferenceValue = contentR;
            so.FindProperty("listFont").objectReferenceValue         = jpFont;

            // ライフ・ヘルプ表示
            so.FindProperty("livesLabel").objectReferenceValue    = livesTmp;
            so.FindProperty("helpCardsLabel").objectReferenceValue = helpsTmp;

            // ヘルプカードパネル
            so.FindProperty("helpCardPanel").objectReferenceValue =
                helpCardPanelGO;
            so.FindProperty("useHelpCardBtn").objectReferenceValue =
                useHelpCardBtnGO.GetComponent<Button>();
            so.FindProperty("useHelpCardBtnLabel").objectReferenceValue =
                useHelpCardBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("helpCardUsedLabel").objectReferenceValue = helpCardUsedLblTmp;

            // ホストパネル
            so.FindProperty("hostPanel").objectReferenceValue =
                hostPanelGO;
            so.FindProperty("nextRoundBtn").objectReferenceValue =
                nextRoundBtnGO.GetComponent<Button>();
            so.FindProperty("nextRoundBtnLabel").objectReferenceValue =
                nextRoundBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // ヘルプ変換ダイアログ
            so.FindProperty("helpConvertPanel").objectReferenceValue  = helpConvertPanelGO;
            so.FindProperty("helpConvertLabel").objectReferenceValue  = helpConvertLblTmp;
            so.FindProperty("convertBtn").objectReferenceValue        =
                convertBtnGO.GetComponent<Button>();
            so.FindProperty("skipConvertBtn").objectReferenceValue    =
                skipConvertBtnGO.GetComponent<Button>();

            // ゲームクリア
            so.FindProperty("gameClearOverlay").objectReferenceValue  = gameClearOverlayGO;
            so.FindProperty("gameClearLabel").objectReferenceValue    = gameClearLblTmp;
            so.FindProperty("gameClearBackBtn").objectReferenceValue  =
                gameClearBackBtnGO.GetComponent<Button>();

            // ゲームオーバー
            so.FindProperty("gameOverOverlay").objectReferenceValue   = gameOverOverlayGO;
            so.FindProperty("gameOverLabel").objectReferenceValue     = gameOverLblTmp;
            so.FindProperty("gameOverBackBtn").objectReferenceValue   =
                gameOverBackBtnGO.GetComponent<Button>();

            // フェード
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.FindProperty("panelGroup").objectReferenceValue = panelCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                "Assets/Scenes/MultiResult.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/MultiResult.unity");
            Debug.Log("[MultiResultSceneBuilder] MultiResult シーンを作成しました");
        }

        // ── ユーティリティ（MultiConfirmSceneBuilder と共通）─────────────────

        static void MakeSeparator(Transform parent, float y)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = new Color(0.86f, 0.76f, 0.48f, 0.65f);
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
                    r.localRotation = Quaternion.Euler(0f, 0f, -22f);
                    var img = go.AddComponent<Image>();
                    img.sprite = lemon; img.preserveAspect = true; img.raycastTarget = false;
                    img.color = new Color(1f, 1f, 1f, alpha);
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
            if (ti == null) { Debug.LogWarning($"[MultiResultBuilder] Not found: {path}"); return null; }
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
