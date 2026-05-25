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
        static readonly Color DiffNum      = new(0.55f, 0.05f, 0.02f);
        static readonly Color DialogBg     = new(1f,    0.97f, 0.90f);
        static readonly Color GameOverBg   = new(0.20f, 0.05f, 0.05f, 0.95f);
        static readonly Color GameClearBg  = new(0.96f, 0.85f, 0.04f, 0.90f);
        static readonly Color LifeChip     = new(0.38f, 0.55f, 0.92f);
        static readonly Color LifeNum      = new(0.05f, 0.15f, 0.58f);

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

            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
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

            // ── HUD 右上 ──
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

            // ── HOME ボタン（左上）──
            var homeBtnGO = MakeButton(panelGO.transform, "HomeButton", "HOME",
                new Vector2(0f,1f), new Vector2(0f,1f),
                new Vector2(24f,-104f), new Vector2(200f,80f),
                BtnSecondary, TextMuted, 34f, jpFont, pillSprite);

            // ── タイトル ──
            MakeLabel(panelGO.transform, "Title", "結果発表",
                new Vector2(0.5f,0.5f), new Vector2(0f, 760f), new Vector2(800f,90f),
                56f, TextPrimary, FontStyles.Bold, jpFont);

            // ── ターン情報 ──
            var roundLabelTmp = MakeLabel(panelGO.transform, "RoundLabel", "1/5人目のチャレンジ",
                new Vector2(0.5f,0.5f), new Vector2(0f, 664f), new Vector2(800f,64f),
                44f, TextPrimary, FontStyles.Bold, jpFont);

            var remainingLabelTmp = MakeLabel(panelGO.transform, "RemainingTurnsLabel", "残り4ターン",
                new Vector2(0.5f,0.5f), new Vector2(0f, 596f), new Vector2(800f,50f),
                36f, TextMuted, FontStyles.Normal, jpFont);

            // ── 数字カード 2枚（幅420、余白90px）──
            var guessedGroupGO = new GameObject("GuessedGroup", typeof(RectTransform));
            guessedGroupGO.transform.SetParent(panelGO.transform, false);
            var guessedCG = guessedGroupGO.AddComponent<CanvasGroup>();
            guessedCG.alpha = 0f;
            SetAnchoredRect(guessedGroupGO, new Vector2(420f, 400f), new Vector2(-255f, 310f));
            TextMeshProUGUI guessedNumLabel;
            BuildNumberPill(guessedGroupGO.transform, pillSprite, uiSprite, jpFont,
                GuessedBg, GuessedNum, "予　想", out guessedNumLabel);

            var secretGroupGO = new GameObject("SecretGroup", typeof(RectTransform));
            secretGroupGO.transform.SetParent(panelGO.transform, false);
            var secretCG = secretGroupGO.AddComponent<CanvasGroup>();
            secretCG.alpha = 0f;
            SetAnchoredRect(secretGroupGO, new Vector2(420f, 400f), new Vector2(255f, 310f));
            TextMeshProUGUI secretNumLabel;
            BuildNumberPill(secretGroupGO.transform, pillSprite, uiSprite, jpFont,
                SecretBg, SecretNum, "秘密の数字", out secretNumLabel);

            MakeLabel(panelGO.transform, "VS", "vs",
                new Vector2(0.5f,0.5f), new Vector2(0f, 310f), new Vector2(80f, 72f),
                40f, TextMuted, FontStyles.Bold, jpFont);

            // ── 差カード（予想・秘密数字と同スタイル）──
            var diffGroupGO = new GameObject("DiffGroup", typeof(RectTransform));
            diffGroupGO.transform.SetParent(panelGO.transform, false);
            var diffCG = diffGroupGO.AddComponent<CanvasGroup>();
            diffCG.alpha = 0f;
            SetAnchoredRect(diffGroupGO, new Vector2(480f, 200f), new Vector2(0f, -30f));
            TextMeshProUGUI diffLabelTmp;
            BuildNumberPill(diffGroupGO.transform, pillSprite, uiSprite, jpFont,
                DiffBg, DiffNum, "差", out diffLabelTmp);
            diffLabelTmp.fontSizeMin = 48f;
            diffLabelTmp.fontSizeMax = 100f;

            // ── キャラクターグループ ──
            var charGroupGO = new GameObject("CharacterGroup", typeof(RectTransform));
            charGroupGO.transform.SetParent(panelGO.transform, false);
            var charCG = charGroupGO.AddComponent<CanvasGroup>();
            charCG.alpha = 0f;
            SetAnchoredRect(charGroupGO, new Vector2(270f, 260f), new Vector2(0f, -300f));

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
            lmGO.SetActive(false);

            // TitleLemon（ゲームクリア確定時に表示するフルスクリーン画像）
            var tlGO = new GameObject("TitleLemon", typeof(RectTransform));
            tlGO.transform.SetParent(charGroupGO.transform, false);
            StretchFull(tlGO.GetComponent<RectTransform>());
            Image titleLemonImg = null;
            if (lemonSprite != null)
            {
                titleLemonImg = tlGO.AddComponent<Image>();
                titleLemonImg.sprite = lemonSprite;
                titleLemonImg.preserveAspect = true;
                titleLemonImg.raycastTarget = false;
            }
            tlGO.SetActive(false);

            var ssGO = new GameObject("Sosolemon", typeof(RectTransform));
            ssGO.transform.SetParent(charGroupGO.transform, false);
            StretchFull(ssGO.GetComponent<RectTransform>());
            Image sosolemonImg = null;
            var sosolemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/sosolemon.png");
            if (sosolemonSprite != null)
            {
                sosolemonImg = ssGO.AddComponent<Image>();
                sosolemonImg.sprite = sosolemonSprite;
                sosolemonImg.preserveAspect = true;
                sosolemonImg.raycastTarget = false;
            }
            else Debug.LogWarning("[ResultRevealBuilder] sosolemon.png not found");
            ssGO.SetActive(false);

            // ── 爆発エフェクト ──
            var expSmallRT = BuildExplosion(panelGO.transform, "ExplosionSmall", bombSprite, 300f, new Vector2(0f, -300f));
            var expLargeRT = BuildExplosion(panelGO.transform, "ExplosionLarge", bombSprite, 560f, new Vector2(0f, -300f));

            // ── ライフ変化表示（差解決後に出現・背景なし）──
            var ltGroupGO = new GameObject("LifeTransitionGroup", typeof(RectTransform));
            ltGroupGO.transform.SetParent(panelGO.transform, false);
            var ltCG = ltGroupGO.AddComponent<CanvasGroup>();
            ltCG.alpha = 0f; ltCG.blocksRaycasts = false;
            SetAnchoredRect(ltGroupGO, new Vector2(480f, 200f), new Vector2(0f, -510f));

            // 「ライフ」テキスト（背景なし）
            var ltHeaderGO = new GameObject("Header", typeof(RectTransform));
            ltHeaderGO.transform.SetParent(ltGroupGO.transform, false);
            var ltHR = ltHeaderGO.GetComponent<RectTransform>();
            ltHR.anchorMin = ltHR.anchorMax = new Vector2(0.5f, 1f);
            ltHR.pivot = new Vector2(0.5f, 1f);
            ltHR.sizeDelta = new Vector2(480f, 52f);
            ltHR.anchoredPosition = Vector2.zero;
            var ltHeaderTmp = ltHeaderGO.AddComponent<TextMeshProUGUI>();
            ltHeaderTmp.text = "ライフ"; ltHeaderTmp.fontSize = 36f;
            ltHeaderTmp.fontStyle = FontStyles.Bold; ltHeaderTmp.alignment = TextAlignmentOptions.Center;
            ltHeaderTmp.color = LifeChip; ltHeaderTmp.raycastTarget = false;
            if (jpFont != null) ltHeaderTmp.font = jpFont;

            // 数字テキスト「8→6」（背景なし）
            var ltNumGO = new GameObject("Number", typeof(RectTransform));
            ltNumGO.transform.SetParent(ltGroupGO.transform, false);
            var ltNR = ltNumGO.GetComponent<RectTransform>();
            ltNR.anchorMin = new Vector2(0f, 0f); ltNR.anchorMax = new Vector2(1f, 1f);
            ltNR.offsetMin = new Vector2(0f, 0f); ltNR.offsetMax = new Vector2(0f, -56f);
            TextMeshProUGUI ltLabel = ltNumGO.AddComponent<TextMeshProUGUI>();
            ltLabel.text = "8→6";
            ltLabel.fontStyle = FontStyles.Bold;
            ltLabel.alignment = TextAlignmentOptions.Center;
            ltLabel.color = LifeNum;
            ltLabel.enableAutoSizing = true;
            ltLabel.fontSizeMin = 80f;
            ltLabel.fontSizeMax = 140f;
            ltLabel.enableWordWrapping = false;
            ltLabel.raycastTarget = false;
            if (jpFont != null) ltLabel.font = jpFont;

            // ── ヘルプカード使用表示 ──
            var huGroupGO = new GameObject("HelpUsedGroup", typeof(RectTransform));
            huGroupGO.transform.SetParent(panelGO.transform, false);
            var huCG = huGroupGO.AddComponent<CanvasGroup>();
            huCG.alpha = 0f; huCG.blocksRaycasts = false;
            SetAnchoredRect(huGroupGO, new Vector2(720f, 64f), new Vector2(0f, -700f));

            var huLabelGO = new GameObject("HelpUsedLabel", typeof(RectTransform));
            huLabelGO.transform.SetParent(huGroupGO.transform, false);
            StretchFull(huLabelGO.GetComponent<RectTransform>());
            TextMeshProUGUI huLabel = huLabelGO.AddComponent<TextMeshProUGUI>();
            huLabel.text      = "ヘルプカード使用 −1";
            huLabel.fontSize  = 40f;
            huLabel.fontStyle = FontStyles.Bold;
            huLabel.alignment = TextAlignmentOptions.Center;
            huLabel.color     = new Color(0.60f, 0.20f, 0.60f, 1f);
            huLabel.raycastTarget = false;
            if (jpFont != null) huLabel.font = jpFont;

            // ── ヘルプカードダイアログ（diff>=5かつ残枚数ありの時表示）──
            var helpDialogGroupGO = new GameObject("HelpCardDialog", typeof(RectTransform));
            helpDialogGroupGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(helpDialogGroupGO.GetComponent<RectTransform>());
            var helpDialogCG = helpDialogGroupGO.AddComponent<CanvasGroup>();
            helpDialogCG.alpha = 0f; helpDialogCG.blocksRaycasts = false;

            var hdBack = new GameObject("Backdrop", typeof(RectTransform));
            hdBack.transform.SetParent(helpDialogGroupGO.transform, false);
            StretchFull(hdBack.GetComponent<RectTransform>());
            hdBack.AddComponent<Image>().color = new Color(0f,0f,0f,0.55f);

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
                new Vector2(0f,-803f), new Vector2(900f,118f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // ── レモンシャワー用親 ──
            var showerGO = new GameObject("LemonShowerParent", typeof(RectTransform));
            showerGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(showerGO.GetComponent<RectTransform>());
            showerGO.SetActive(false);

            // ── ゲームオーバーオーバーレイ ──
            var gameOverGO = new GameObject("GameOverOverlay", typeof(RectTransform));
            gameOverGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(gameOverGO.GetComponent<RectTransform>());
            var gameOverCG = gameOverGO.AddComponent<CanvasGroup>();
            gameOverCG.alpha = 0f; gameOverCG.blocksRaycasts = false;

            var goBd = new GameObject("Backdrop", typeof(RectTransform));
            goBd.transform.SetParent(gameOverGO.transform, false);
            StretchFull(goBd.GetComponent<RectTransform>());
            goBd.AddComponent<Image>().color = GameOverBg;

            var goCard = new GameObject("Card", typeof(RectTransform));
            goCard.transform.SetParent(gameOverGO.transform, false);
            var goCardImg = goCard.AddComponent<Image>();
            goCardImg.sprite = uiSprite; goCardImg.type = Image.Type.Sliced;
            goCardImg.color = new Color(0.98f, 0.92f, 0.90f, 1f);
            var goCardR = goCard.GetComponent<RectTransform>();
            goCardR.anchorMin = goCardR.anchorMax = new Vector2(0.5f, 0.5f);
            goCardR.pivot = new Vector2(0.5f, 0.5f);
            goCardR.sizeDelta = new Vector2(920f, 620f);
            goCardR.anchoredPosition = Vector2.zero;

            MakeLabel(goCard.transform, "Title", "GAME OVER",
                new Vector2(0.5f,0.5f), new Vector2(0f, 160f), new Vector2(860f, 110f),
                72f, new Color(0.78f, 0.10f, 0.10f), FontStyles.Bold, jpFont);

            var gameOverDetailLbl = MakeLabel(goCard.transform, "Detail", "あとX人でクリアでした！",
                new Vector2(0.5f,0.5f), new Vector2(0f, 50f), new Vector2(860f, 100f),
                44f, TextPrimary, FontStyles.Normal, jpFont);

            var gameOverHomeBtnGO = MakeButton(goCard.transform, "HomeButton", "ホームへ",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(0f, -130f), new Vector2(600f, 110f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // painlemo（カード枠の上中央にぷるぷる表示・2倍サイズ）
            RectTransform gameOverPainlemoRT = null;
            if (painlemoSprite != null)
            {
                var goPainGO = new GameObject("GameOverPainlemo", typeof(RectTransform));
                goPainGO.transform.SetParent(gameOverGO.transform, false);
                var gpR = goPainGO.GetComponent<RectTransform>();
                gpR.anchorMin = gpR.anchorMax = new Vector2(0.5f, 0.5f);
                gpR.pivot = new Vector2(0.5f, 0.5f);
                gpR.sizeDelta = new Vector2(380f, 380f);
                // goCard は中央y=0・高さ620 → 上端y=310。painlemo中心をその上に配置
                gpR.anchoredPosition = new Vector2(0f, 500f);
                var gpImg = goPainGO.AddComponent<Image>();
                gpImg.sprite = painlemoSprite;
                gpImg.preserveAspect = true;
                gpImg.raycastTarget = false;
                gameOverPainlemoRT = gpR;
            }

            // ── ゲームクリアオーバーレイ ──
            var gameClearGO = new GameObject("GameClearOverlay", typeof(RectTransform));
            gameClearGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(gameClearGO.GetComponent<RectTransform>());
            var gameClearCG = gameClearGO.AddComponent<CanvasGroup>();
            gameClearCG.alpha = 0f; gameClearCG.blocksRaycasts = false;

            var gcBd = new GameObject("Backdrop", typeof(RectTransform));
            gcBd.transform.SetParent(gameClearGO.transform, false);
            StretchFull(gcBd.GetComponent<RectTransform>());
            gcBd.AddComponent<Image>().color = GameClearBg;

            var gcCard = new GameObject("Card", typeof(RectTransform));
            gcCard.transform.SetParent(gameClearGO.transform, false);
            var gcCardImg = gcCard.AddComponent<Image>();
            gcCardImg.sprite = uiSprite; gcCardImg.type = Image.Type.Sliced;
            gcCardImg.color = new Color(1.00f, 0.98f, 0.82f, 1f);
            var gcCardR = gcCard.GetComponent<RectTransform>();
            gcCardR.anchorMin = gcCardR.anchorMax = new Vector2(0.5f, 0.5f);
            gcCardR.pivot = new Vector2(0.5f, 0.5f);
            gcCardR.sizeDelta = new Vector2(920f, 720f);
            gcCardR.anchoredPosition = Vector2.zero;

            MakeLabel(gcCard.transform, "Title", "ゲームクリア！",
                new Vector2(0.5f,0.5f), new Vector2(0f, 210f), new Vector2(860f, 120f),
                80f, new Color(0.72f, 0.40f, 0.02f), FontStyles.Bold, jpFont);

            var gameClearDetailLbl = MakeLabel(gcCard.transform, "Detail", "全員のチャレンジクリア！",
                new Vector2(0.5f,0.5f), new Vector2(0f, 60f), new Vector2(860f, 110f),
                44f, TextPrimary, FontStyles.Normal, jpFont);
            gameClearDetailLbl.lineSpacing = 6f;

            var gameClearHomeBtnGO = MakeButton(gcCard.transform, "HomeButton", "ホームへ",
                new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
                new Vector2(0f, -155f), new Vector2(600f, 110f),
                BtnPrimary, TextPrimary, 46f, jpFont, pillSprite);

            // ゲームクリアレモン（カード枠上中央にぷにぷに）
            RectTransform gameClearLemonRT = null;
            if (lemonSprite != null)
            {
                var gcLemonGO = new GameObject("GameClearLemon", typeof(RectTransform));
                gcLemonGO.transform.SetParent(gameClearGO.transform, false);
                var gclR = gcLemonGO.GetComponent<RectTransform>();
                gclR.anchorMin = gclR.anchorMax = new Vector2(0.5f, 0.5f);
                gclR.pivot = new Vector2(0.5f, 0.5f);
                gclR.sizeDelta = new Vector2(380f, 380f);
                gclR.anchoredPosition = new Vector2(0f, 500f);
                var gclImg = gcLemonGO.AddComponent<Image>();
                gclImg.sprite = lemonSprite;
                gclImg.preserveAspect = true;
                gclImg.raycastTarget = false;
                gameClearLemonRT = gclR;
            }

            // ── ResultRevealController ──
            var ctrlGO = new GameObject("ResultRevealController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<ResultRevealController>();
            var so   = new SerializedObject(ctrl);

            so.FindProperty("roundLabel").objectReferenceValue           = roundLabelTmp;
            so.FindProperty("remainingTurnsLabel").objectReferenceValue  = remainingLabelTmp;
            so.FindProperty("guessedGroup").objectReferenceValue         = guessedCG;
            so.FindProperty("guessedNumberLabel").objectReferenceValue   = guessedNumLabel;
            so.FindProperty("secretGroup").objectReferenceValue          = secretCG;
            so.FindProperty("secretNumberLabel").objectReferenceValue    = secretNumLabel;
            so.FindProperty("diffGroup").objectReferenceValue            = diffCG;
            so.FindProperty("diffLabel").objectReferenceValue            = diffLabelTmp;
            so.FindProperty("lifeTransitionGroup").objectReferenceValue  = ltCG;
            so.FindProperty("lifeTransitionLabel").objectReferenceValue  = ltLabel;
            so.FindProperty("helpUsedGroup").objectReferenceValue        = huCG;
            so.FindProperty("helpUsedLabel").objectReferenceValue        = huLabel;
            so.FindProperty("characterGroup").objectReferenceValue       = charCG;
            so.FindProperty("painlemoImage").objectReferenceValue        = painlemoImg;
            so.FindProperty("sosolemonImage").objectReferenceValue      = sosolemonImg;
            so.FindProperty("lemonImage").objectReferenceValue           = lemonImg;
            so.FindProperty("titleLemonImage").objectReferenceValue      = titleLemonImg;
            so.FindProperty("explosionSmall").objectReferenceValue       = expSmallRT;
            so.FindProperty("explosionLarge").objectReferenceValue       = expLargeRT;
            so.FindProperty("lemonShowerParent").objectReferenceValue    = showerGO.GetComponent<RectTransform>();
            so.FindProperty("lemonTex").objectReferenceValue             = lemonTex;
            so.FindProperty("helpDialogGroup").objectReferenceValue      = helpDialogCG;
            so.FindProperty("helpDialogBodyLabel").objectReferenceValue  = hdBody;
            so.FindProperty("useHelpButton").objectReferenceValue        = hdUseBtn.GetComponent<Button>();
            so.FindProperty("dontUseButton").objectReferenceValue        = hdNoBtn.GetComponent<Button>();
            so.FindProperty("lifeCountLabel").objectReferenceValue       = lifeCountLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue   = helpCardCountLabel;
            so.FindProperty("gameOverGroup").objectReferenceValue        = gameOverCG;
            so.FindProperty("gameOverDetailLabel").objectReferenceValue  = gameOverDetailLbl;
            so.FindProperty("gameOverHomeButton").objectReferenceValue   = gameOverHomeBtnGO.GetComponent<Button>();
            so.FindProperty("gameOverPainlemo").objectReferenceValue     = gameOverPainlemoRT;
            so.FindProperty("gameClearGroup").objectReferenceValue       = gameClearCG;
            so.FindProperty("gameClearDetailLabel").objectReferenceValue = gameClearDetailLbl;
            so.FindProperty("gameClearHomeButton").objectReferenceValue  = gameClearHomeBtnGO.GetComponent<Button>();
            so.FindProperty("gameClearLemon").objectReferenceValue       = gameClearLemonRT;
            so.FindProperty("nextButton").objectReferenceValue           = nextBtnGO.GetComponent<Button>();
            so.FindProperty("homeButton").objectReferenceValue           = homeBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue           = panelCG;

            // HellModeColorApplier（地獄モード時はレモンパターンをライムに差し替え）
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
            numberLabel.text                = "?";
            numberLabel.fontStyle           = FontStyles.Bold;
            numberLabel.alignment           = TextAlignmentOptions.Center;
            numberLabel.color               = numColor;
            numberLabel.enableAutoSizing    = true;
            numberLabel.fontSizeMin         = 80f;
            numberLabel.fontSizeMax         = 180f;
            numberLabel.enableWordWrapping  = false;
            numberLabel.raycastTarget       = false;
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
