using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Multiplayer;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// チームバトルモード：ラウンド結果画面のシーンビルダー。
    /// チームスコア・ラウンド結果・予想一覧を表示し、次ラウンド進行を担う。
    /// </summary>
    public static class TeamResultSceneBuilder
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
        static readonly Color NumberBg     = new(0.97f, 0.82f, 0.10f);

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

            // ── 自分の情報バー（上部固定）──
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
            var roundBarTmp = MakeLabel(myInfoBarGO.transform, "RoundLabel", "ラウンド 1/1",
                new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(300f, 64f),
                34f, Color.white, FontStyles.Bold, jpFont);
            roundBarTmp.alignment = TextAlignmentOptions.MidlineLeft;

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

            // ── コンテンツカード: 1020×1600, center y=-30 ──
            // card local: top=+800, bottom=-800
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
            cardR.sizeDelta = new Vector2(1020f, 1600f);
            cardR.anchoredPosition = new Vector2(0f, -30f);

            // ── ラウンドヘッダー ──
            var roundHeaderTmp = MakeLabel(cardGO.transform, "RoundHeader", "ラウンド結果",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 760f), new Vector2(960f, 60f),
                48f, TextPrimary, FontStyles.Bold, jpFont);

            // ── アクティブチームバッジ ──
            var activeTeamBadgeTmp = MakeLabel(cardGO.transform, "ActiveTeamBadge", "チームA の番",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 700f), new Vector2(700f, 52f),
                40f, TextPrimary, FontStyles.Bold, jpFont);

            MakeSeparator(cardGO.transform, 664f);

            // ── お題 ──
            var topicHeaderTmp = MakeLabel(cardGO.transform, "TopicHeader", "お題",
                new Vector2(0.5f, 0.5f), new Vector2(-390f, 620f), new Vector2(130f, 44f),
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
            topicBgR.sizeDelta = new Vector2(940f, 104f);
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

            MakeSeparator(cardGO.transform, 492f);

            // ── 最終決定者名 ──
            var deciderNameTmp = MakeLabel(cardGO.transform, "DeciderName", "最終決定者：○○",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 448f), new Vector2(920f, 52f),
                36f, TextMuted, FontStyles.Normal, jpFont);

            // ── 実際の数字ボックス（黄色・大） ──
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
            numBoxR.anchoredPosition = new Vector2(0f, 340f);
            var numLblGO = new GameObject("NumberLabel", typeof(RectTransform));
            numLblGO.transform.SetParent(numBoxGO.transform, false);
            var nlr = numLblGO.GetComponent<RectTransform>();
            nlr.anchorMin = Vector2.zero; nlr.anchorMax = Vector2.one;
            nlr.offsetMin = nlr.offsetMax = Vector2.zero;
            var numTmp = numLblGO.AddComponent<TextMeshProUGUI>();
            numTmp.text = "47"; numTmp.fontSize = 72f; numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = TextPrimary; numTmp.raycastTarget = false;
            if (jpFont != null) numTmp.font = jpFont;

            // ── ラウンド差分ラベル ──
            var roundDiffTmp = MakeLabel(cardGO.transform, "RoundDiffLabel", "差：+2",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 248f), new Vector2(920f, 56f),
                38f, TextPrimary, FontStyles.Bold, jpFont);

            MakeSeparator(cardGO.transform, 204f);

            // ── スコア行（チームA 左・チームB 右）──
            var teamAScoreTmp = MakeLabel(cardGO.transform, "TeamAScoreLabel", "チームA：0pt",
                new Vector2(0.5f, 0.5f), new Vector2(-230f, 152f), new Vector2(400f, 72f),
                40f, new Color(0.15f, 0.35f, 0.75f), FontStyles.Bold, jpFont);

            var teamBScoreTmp = MakeLabel(cardGO.transform, "TeamBScoreLabel", "チームB：0pt",
                new Vector2(0.5f, 0.5f), new Vector2(230f, 152f), new Vector2(400f, 72f),
                40f, new Color(0.75f, 0.20f, 0.15f), FontStyles.Bold, jpFont);

            MakeSeparator(cardGO.transform, 104f);

            // ── 予想リストヘッダー ──
            var listHeaderTmp = MakeLabel(cardGO.transform, "GuessSectionHeader", "みんなの予想",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(940f, 52f),
                40f, TextPrimary, FontStyles.Bold, jpFont);

            // ── ScrollRect（予想リスト）──
            var scrollGO = new GameObject("GuessListScroll", typeof(RectTransform));
            scrollGO.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin        = new Vector2(0f, 0.5f);
            scrollR.anchorMax        = new Vector2(1f, 0.5f);
            scrollR.pivot            = new Vector2(0.5f, 1f);
            scrollR.sizeDelta        = new Vector2(-40f, 400f);
            scrollR.anchoredPosition = new Vector2(0f, -20f);
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

            // ── ホストパネル ──
            var hostPanelGO = new GameObject("HostPanel", typeof(RectTransform));
            hostPanelGO.transform.SetParent(cardGO.transform, false);
            StretchFull(hostPanelGO.GetComponent<RectTransform>());
            hostPanelGO.SetActive(false);

            var nextRoundBtnGO = MakeButton(hostPanelGO.transform, "NextRoundBtn",
                "次のラウンドへ ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -680f), new Vector2(400f, 80f),
                BtnPrimary, TextPrimary, 40f, jpFont, btnYellow);

            // ── ScreenFade ──
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;

            // ── TeamResultController ──
            var ctrlGO = new GameObject("TeamResultController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<TeamResultController>();
            var so   = new SerializedObject(ctrl);

            // ラウンド情報
            so.FindProperty("roundHeaderLabel").objectReferenceValue   = roundHeaderTmp;
            so.FindProperty("activeTeamBadge").objectReferenceValue    = activeTeamBadgeTmp;
            so.FindProperty("topicLabel").objectReferenceValue         = topicTmp;
            so.FindProperty("actualNumberLabel").objectReferenceValue  = numTmp;
            so.FindProperty("deciderNameLabel").objectReferenceValue   = deciderNameTmp;

            // チームスコア
            so.FindProperty("teamAScoreLabel").objectReferenceValue    = teamAScoreTmp;
            so.FindProperty("teamBScoreLabel").objectReferenceValue    = teamBScoreTmp;
            so.FindProperty("roundDiffLabel").objectReferenceValue     = roundDiffTmp;

            // 自分の情報バー
            so.FindProperty("myTopicLabel").objectReferenceValue       = myTopicLabelTmp;
            so.FindProperty("mySecretLabel").objectReferenceValue      = mySecretLabelTmp;

            // 予想リスト
            so.FindProperty("guessListContent").objectReferenceValue   = contentR;
            so.FindProperty("listFont").objectReferenceValue           = jpFont;

            // ホストパネル
            so.FindProperty("hostPanel").objectReferenceValue          = hostPanelGO;
            so.FindProperty("nextRoundBtn").objectReferenceValue       =
                nextRoundBtnGO.GetComponent<Button>();
            so.FindProperty("nextRoundBtnLabel").objectReferenceValue  =
                nextRoundBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            // フェード
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.FindProperty("panelGroup").objectReferenceValue = panelCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                "Assets/Scenes/TeamResult.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/TeamResult.unity");
            Debug.Log("[TeamResultSceneBuilder] TeamResult シーンを作成しました");
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
            if (ti == null) { Debug.LogWarning($"[TeamResultSceneBuilder] Not found: {path}"); return null; }
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
