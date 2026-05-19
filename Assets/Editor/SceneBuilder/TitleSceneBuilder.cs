using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Title;
using BomBomLemon.Game.Topics;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class TitleSceneBuilder
    {
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.98f, 0.90f, 0.55f);
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

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景（濃い黄色）
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.98f, 0.90f, 0.55f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // TitleGroup（フェードイン用）
            var titleGroupGO = new GameObject("TitleGroup", typeof(RectTransform));
            titleGroupGO.transform.SetParent(canvasGO.transform, false);
            var titleCG = titleGroupGO.AddComponent<CanvasGroup>();
            titleCG.alpha = 0f;
            var tgRect = titleGroupGO.GetComponent<RectTransform>();
            tgRect.anchorMin = Vector2.zero;
            tgRect.anchorMax = Vector2.one;
            tgRect.offsetMin = Vector2.zero;
            tgRect.offsetMax = Vector2.zero;

            // レモン雨エフェクト（TitleGroupの最初の子→ロゴの背面に描画）
            var lemonTex    = FindTexture("Title_Lemon");
            var limeTex     = FindTexture("lime");
            var startLimeTex = FindTexture("startlime");
            var rainGO = new GameObject("LemonRain", typeof(RectTransform));
            rainGO.transform.SetParent(titleGroupGO.transform, false);
            var rainRect = rainGO.GetComponent<RectTransform>();
            rainRect.anchorMin = Vector2.zero;
            rainRect.anchorMax = Vector2.one;
            rainRect.offsetMin = Vector2.zero;
            rainRect.offsetMax = Vector2.zero;
            var lemonRain = rainGO.AddComponent<LemonRainEffect>();
            var rainSO = new SerializedObject(lemonRain);
            rainSO.FindProperty("lemonTexture").objectReferenceValue = lemonTex;
            rainSO.FindProperty("particleCount").intValue = 8;
            rainSO.ApplyModifiedProperties();

            // タイトルロゴ（3枚重ね）
            var bubbleTex = FindTexture("Title_Bubble");
            var wordTex   = FindTexture("Title_Word");

            var logoGroupGO = new GameObject("TitleLogo", typeof(RectTransform));
            logoGroupGO.transform.SetParent(titleGroupGO.transform, false);
            var logoGroupRect = logoGroupGO.GetComponent<RectTransform>();
            logoGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoGroupRect.pivot = new Vector2(0.5f, 0.5f);
            logoGroupRect.sizeDelta = new Vector2(1200f, 1200f);
            logoGroupRect.anchoredPosition = new Vector2(0f, 220f);

            // Bubble: 7%上（134px）: y=-268+134=-134
            var bubbleRect = AddRawImageLayerSizedByWidth(logoGroupGO.transform, "Layer_Bubble", bubbleTex, 938f, new Vector2(0f, -134f));

            // Lemon: 416*1.1=458px、10%大きく
            var lemonRect = AddRawImageLayerSized(logoGroupGO.transform, "Layer_Lemon", lemonTex, 527f, new Vector2(0f, -18f));

            // Word: 幅860px
            var wordRect = AddRawImageLayerSizedByWidth(logoGroupGO.transform, "Layer_Word", wordTex, 860f, new Vector2(0f, -270f));

            // STARTボタン
            var startBtnGO = new GameObject("StartButton", typeof(RectTransform));
            startBtnGO.transform.SetParent(titleGroupGO.transform, false);
            var startRect = startBtnGO.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.5f, 0.5f);
            startRect.anchorMax = new Vector2(0.5f, 0.5f);
            startRect.pivot = new Vector2(0.5f, 0.5f);
            startRect.anchoredPosition = new Vector2(0f, -576f);

            var hitImg = startBtnGO.AddComponent<Image>();
            hitImg.color = Color.clear;
            var playBtn = startBtnGO.AddComponent<Button>();
            var btnColors = playBtn.colors;
            btnColors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            btnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            playBtn.colors = btnColors;

            var startTex = FindTexture("start");
            RawImage startBtnRawImg = null;
            if (startTex != null)
            {
                float ratio = (float)startTex.width / startTex.height;
                float h = 290f;   // 276 * 1.05
                startRect.sizeDelta = new Vector2(h * ratio, h);

                var startImgGO = new GameObject("StartImage", typeof(RectTransform));
                startImgGO.transform.SetParent(startBtnGO.transform, false);
                startBtnRawImg = startImgGO.AddComponent<RawImage>();
                startBtnRawImg.texture = startTex;
                startBtnRawImg.raycastTarget = false;
                var imgRect = startImgGO.GetComponent<RectTransform>();
                imgRect.anchorMin = new Vector2(0.5f, 0.5f);
                imgRect.anchorMax = new Vector2(0.5f, 0.5f);
                imgRect.pivot = new Vector2(0.5f, 0.5f);
                imgRect.sizeDelta = new Vector2(h * ratio, h);
                imgRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                startRect.sizeDelta = new Vector2(500f, 130f);
                hitImg.color = new Color(0.2f, 0.15f, 0.4f);
            }

            // サブタイトル（日本語フォントを検索して適用）
            var jpFont = FindJapaneseTMPFont();
            var subJP = CreateLabel(titleGroupGO.transform, "SubtitleJP",
                "2～24人用のパーティーゲーム",
                new Vector2(0.5f, 0.5f), new Vector2(920f, 72f), 41);
            subJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -759f);
            subJP.color = new Color(0.38f, 0.18f, 0.04f, 0.92f);
            subJP.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) subJP.font = jpFont;
            ApplySharpMaterial(subJP);

            var subEN = CreateLabel(titleGroupGO.transform, "SubtitleEN",
                "Party game for 2 to 24 players", new Vector2(0.5f, 0.5f), new Vector2(920f, 72f), 41);
            subEN.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -759f);
            subEN.color = new Color(0.48f, 0.28f, 0.10f, 0.85f);
            subEN.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) subEN.font = jpFont;
            ApplySharpMaterial(subEN);
            subEN.gameObject.SetActive(false);

            // 地獄モード説明（初期は非表示）
            // SubtitleJP: y=-740 h=72  SubtitleEN: y=-812 h=52 → 下端 y=-838
            // HellDescJP: y=-882（44px余白）  HellDescEN: y=-940
            var hellDescGO = new GameObject("HellDesc", typeof(RectTransform));
            hellDescGO.transform.SetParent(titleGroupGO.transform, false);
            var hellDescCG = hellDescGO.AddComponent<CanvasGroup>();
            hellDescCG.alpha = 0f;
            hellDescCG.blocksRaycasts = false;
            var hellDescRect = hellDescGO.GetComponent<RectTransform>();
            hellDescRect.anchorMin = Vector2.zero;
            hellDescRect.anchorMax = Vector2.one;
            hellDescRect.offsetMin = Vector2.zero;
            hellDescRect.offsetMax = Vector2.zero;

            var hellDescJP = CreateLabel(hellDescGO.transform, "HellDescJP",
                "ライフ1/2  ヘルプカード無し", new Vector2(0.5f, 0.5f), new Vector2(920f, 52f), 34);
            hellDescJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -820f);
            hellDescJP.color = SubJPHellColor();
            hellDescJP.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) hellDescJP.font = jpFont;
            ApplySharpMaterial(hellDescJP);

            var hellDescEN = CreateLabel(hellDescGO.transform, "HellDescEN",
                "Life 1/2  No Help Cards", new Vector2(0.5f, 0.5f), new Vector2(920f, 52f), 34);
            hellDescEN.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -820f);
            hellDescEN.color = SubENHellColor();
            hellDescEN.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) hellDescEN.font = jpFont;
            ApplySharpMaterial(hellDescEN);
            hellDescEN.gameObject.SetActive(false);

            // ─── 上部ボタンバー ───
            var rulesBtn  = CreateTopBarButton(titleGroupGO.transform, "RulesButton",  "ルール", new Vector2(0f,1f), new Vector2( 54f,-191f), new Vector2(152f,54f), jpFont);
            var topicsBtn = CreateTopBarButton(titleGroupGO.transform, "TopicsButton", "お題",   new Vector2(0f,1f), new Vector2(222f,-191f), new Vector2(120f,54f), jpFont);
            var hellBtnGO = CreateTopBarButtonGO(titleGroupGO.transform, "HellModeButton", "地獄モード OFF", new Vector2(1f,1f), new Vector2(-54f,-191f), new Vector2(260f,54f), jpFont);
            // お題右端342px、地獄モード左端766px → 中点554px、キャンバス中心540px → オフセット+14px
            var langBtnGO = CreateTopBarButtonGO(titleGroupGO.transform, "LanguageButton", "English Off",
                new Vector2(0.5f, 1f), new Vector2(14f, -191f), new Vector2(220f, 54f), jpFont);

            // TitleTopBarController
            var topBarGO = new GameObject("TitleTopBarController");
            var topBar = topBarGO.AddComponent<TitleTopBarController>();
            var topBarSO = new SerializedObject(topBar);
            topBarSO.FindProperty("rulesButton").objectReferenceValue        = rulesBtn;
            topBarSO.FindProperty("topicsButton").objectReferenceValue       = topicsBtn;
            topBarSO.FindProperty("hellModeButton").objectReferenceValue     = hellBtnGO.GetComponent<Button>();
            topBarSO.FindProperty("hellButtonBg").objectReferenceValue       = hellBtnGO.GetComponent<Image>();
            topBarSO.FindProperty("hellLabelTmp").objectReferenceValue       = hellBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            topBarSO.FindProperty("backgroundImage").objectReferenceValue    = bgImage;
            topBarSO.FindProperty("lemonRain").objectReferenceValue          = lemonRain;
            topBarSO.FindProperty("titleLemonImage").objectReferenceValue    = lemonRect.GetComponent<RawImage>();
            topBarSO.FindProperty("subtitleJP").objectReferenceValue         = subJP;
            topBarSO.FindProperty("subtitleEN").objectReferenceValue         = subEN;
            if (startBtnRawImg != null)
                topBarSO.FindProperty("startButtonImage").objectReferenceValue = startBtnRawImg;
            topBarSO.FindProperty("lemonTexture").objectReferenceValue       = lemonTex;
            topBarSO.FindProperty("limeTexture").objectReferenceValue        = limeTex;
            topBarSO.FindProperty("startNormalTexture").objectReferenceValue = startTex;
            if (startLimeTex != null)
                topBarSO.FindProperty("startLimeTexture").objectReferenceValue = startLimeTex;
            topBarSO.FindProperty("hellDescGroup").objectReferenceValue = hellDescCG;
            topBarSO.FindProperty("languageButton").objectReferenceValue    = langBtnGO.GetComponent<Button>();
            topBarSO.FindProperty("languageBtnLabel").objectReferenceValue  = langBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            topBarSO.FindProperty("rulesBtnLabel").objectReferenceValue     = rulesBtn.gameObject.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            topBarSO.FindProperty("topicsBtnLabel").objectReferenceValue    = topicsBtn.gameObject.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            topBarSO.FindProperty("hellDescJPTmp").objectReferenceValue     = hellDescJP;
            topBarSO.FindProperty("hellDescENTmp").objectReferenceValue     = hellDescEN;
            // rulesPanel は BuildRulesPanel 後に設定

            // TitleScreenController + BGM AudioSource
            var ctrlGO = new GameObject("TitleScreenController");
            var ctrl = ctrlGO.AddComponent<TitleScreenController>();
            var bgmSrc = ctrlGO.AddComponent<AudioSource>();
            bgmSrc.playOnAwake = false;
            bgmSrc.loop = true;
            bgmSrc.volume = 0.8f;
            var bgmClip = FindAudioClip("title_music", "title", "bgm");
            if (bgmClip != null)
            {
                bgmSrc.clip = bgmClip;
                Debug.Log($"[TitleSceneBuilder] BGM読み込み成功: {AssetDatabase.GetAssetPath(bgmClip)}");
            }
            else
            {
                Debug.LogWarning("[TitleSceneBuilder] BGMが見つかりません。Assets/Audio/title_music.mp3 を配置して再実行してください。");
            }

            var so = new SerializedObject(ctrl);
            so.FindProperty("titleGroup").objectReferenceValue = titleCG;
            so.FindProperty("mainPanel").objectReferenceValue = titleGroupGO;
            so.FindProperty("modeSelectPanel").objectReferenceValue = titleGroupGO;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("titleLogoRect").objectReferenceValue = logoGroupRect;
            so.FindProperty("bgmSource").objectReferenceValue = bgmSrc;
            so.FindProperty("playerSetupSceneName").stringValue = "PlayerSetup";
            so.ApplyModifiedProperties();

            // TitleLogoAnimator
            var animGO = new GameObject("TitleLogoAnimator");
            var anim = animGO.AddComponent<TitleLogoAnimator>();
            var animSO = new SerializedObject(anim);
            animSO.FindProperty("layerBubble").objectReferenceValue = bubbleRect;
            animSO.FindProperty("layerLemon").objectReferenceValue  = lemonRect;
            animSO.FindProperty("layerWord").objectReferenceValue   = wordRect;
            animSO.FindProperty("startButton").objectReferenceValue = startBtnGO.GetComponent<RectTransform>();
            animSO.ApplyModifiedProperties();

            // ルールパネルをCanvas直下に追加（TitleGroupの外＝常に最前面）
            var rulesPanel = BuildRulesPanel(canvasGO.transform, jpFont);
            topBarSO.FindProperty("rulesPanel").objectReferenceValue = rulesPanel;

            // お題カスタマイズパネル
            var (editDialog, topicPanel) = BuildTopicPanel(canvasGO.transform, jpFont);
            topBarSO.FindProperty("topicCustomizePanel").objectReferenceValue = topicPanel;
            topBarSO.ApplyModifiedProperties();

            // TopicRuntimeDatabase（永続シングルトン）
            var rdbGO = new GameObject("TopicRuntimeDatabase");
            var rdb   = rdbGO.AddComponent<TopicRuntimeDatabase>();
            var rdbSO = new SerializedObject(rdb);
            {
                TopicDatabase dbAsset = null;
                var topicDbGuids = AssetDatabase.FindAssets("t:TopicDatabase");
                if (topicDbGuids.Length > 0)
                {
                    dbAsset = AssetDatabase.LoadAssetAtPath<TopicDatabase>(
                        AssetDatabase.GUIDToAssetPath(topicDbGuids[0]));
                }
                else
                {
                    // アセットが存在しない場合は新規作成
                    System.IO.Directory.CreateDirectory("Assets/Resources");
                    dbAsset = ScriptableObject.CreateInstance<TopicDatabase>();
                    AssetDatabase.CreateAsset(dbAsset, "Assets/Resources/TopicDatabase.asset");
                    Debug.Log("[TitleSceneBuilder] TopicDatabase.asset を新規作成しました");
                }
                // デフォルトお題を確実に書き込む
                if (dbAsset.Topics.Count == 0)
                {
                    dbAsset.LoadDefaultTopics();
                    EditorUtility.SetDirty(dbAsset);
                    AssetDatabase.SaveAssets();
                }
                Debug.Log($"[TitleSceneBuilder] TopicDatabase: {dbAsset.Topics.Count} topics loaded");
                rdbSO.FindProperty("defaultDatabase").objectReferenceValue = dbAsset;
                rdbSO.ApplyModifiedProperties();
            }

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Title.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Title.unity", 1);

            Debug.Log("[TitleSceneBuilder] Title シーンを作成しました → Assets/Scenes/Title.unity");
        }

        static TextMeshProUGUI CreateLabel(Transform parent, string name, string text,
            Vector2 anchorCenter, Vector2 size, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text             = text;
            tmp.fontSize         = fontSize;
            tmp.characterSpacing = 2f;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.color            = Color.white;
            ApplySharpMaterial(tmp);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return tmp;
        }

        static void ApplySharpMaterial(TextMeshProUGUI tmp)
        {
            var mat = tmp.fontSharedMaterial;
            if (mat == null) return;
            mat.SetFloat(TMPro.ShaderUtilities.ID_FaceDilate, 0.12f);
            mat.SetFloat(TMPro.ShaderUtilities.ID_OutlineSoftness, 0f);
        }

        // Unity内蔵UISprite（丸角矩形・9-slice済）を取得
        static Sprite GetBuiltinUISprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        // 縦グラデーションテクスチャを生成（上:topColor → 下:botColor）
        static Sprite MakeGradientSprite(Color topColor, Color botColor)
        {
            const int w = 4, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode   = TextureWrapMode.Clamp;
            for (int y = 0; y < h; y++)
            {
                var c = Color.Lerp(botColor, topColor, y / (float)(h - 1));
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*0.5f, 1f, 0,
                SpriteMeshType.FullRect, new Vector4(1,1,1,1));
        }

        static Sprite _pillSprite;
        static Sprite GetPillSprite()
        {
            if (_pillSprite != null) return _pillSprite;
            const int sz = 128;
            const float r = sz * 0.5f;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode   = TextureWrapMode.Clamp;
            var px = new Color32[sz * sz];
            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 1.2f);
                    px[y * sz + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            _pillSprite = Sprite.Create(tex, new Rect(0,0,sz,sz), Vector2.one*0.5f, 1f, 0,
                SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            return _pillSprite;
        }

        // Kenney スプライトを 9-slice で読み込む
        static Sprite LoadKenneySprite(string color, string filename, Vector4 border)
        {
            string path = $"Assets/Sprites/UI/kenney_ui-pack/PNG/{color}/Default/{filename}";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning($"[TitleSceneBuilder] Kenney sprite not found: {path}");
                return null;
            }
            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, border);
        }

        // 上部バー: Buttonを返す
        static Button CreateTopBarButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pos, Vector2 size, TMP_FontAsset font)
        {
            return CreateTopBarButtonGO(parent, name, label, anchor, pos, size, font).GetComponent<Button>();
        }

        // 上部バー: GameObjectを返す
        static GameObject CreateTopBarButtonGO(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pos, Vector2 size, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.pivot            = new Vector2(anchor.x, 0.5f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = pos;

            bool isHell = name == "HellModeButton";

            var pill    = GetBuiltinUISprite() ?? GetPillSprite();

            // ── シャドウ（ぼんやりした暖色ドロップシャドウ）──
            var shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(go.transform, false);
            var shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(2f, -7f);
            shadowRect.offsetMax = new Vector2(-2f, -1f);
            var shadowImg = shadowGO.AddComponent<Image>();
            shadowImg.sprite        = pill;
            shadowImg.type          = Image.Type.Sliced;
            shadowImg.color         = new Color(0.55f, 0.30f, 0.05f, 0.22f);
            shadowImg.raycastTarget = false;

            // ── メインボタン（半透明ウォームクリーム / 地獄は暖かいオレンジ）──
            var bg    = go.AddComponent<Image>();
            bg.sprite = pill;
            bg.type   = Image.Type.Sliced;
            bg.color  = isHell
                ? new Color(0.98f, 0.90f, 0.42f, 0.85f)   // レモン黄（地獄OFFデフォルト）
                : new Color(1f,   0.98f, 0.88f, 0.78f);    // ウォームクリーム（通常）

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor      = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.90f, 1f);
            cols.pressedColor     = new Color(0.85f, 0.78f, 0.65f, 1f);
            cols.colorMultiplier  = 1f;
            btn.colors        = cols;
            btn.targetGraphic = bg;

            {
                var txtGO = new GameObject("Label", typeof(RectTransform));
                txtGO.transform.SetParent(go.transform, false);
                var txtRect = txtGO.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = new Vector2(12f, 0f);
                txtRect.offsetMax = new Vector2(-12f, 0f);
                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.text             = label;
                tmp.fontSize         = 32;
                tmp.characterSpacing = 2f;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.alignment        = TextAlignmentOptions.Center;
                tmp.color            = new Color(0.35f, 0.12f, 0.02f, 1f);
                if (font != null) tmp.font = font;
                ApplySharpMaterial(tmp);
            }

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
            Debug.LogWarning("[TitleSceneBuilder] 日本語TMP_FontAssetが見つかりません。Window > TextMeshPro > Import TMP Essential Resources 後に日本語フォントをインポートしてください。");
            return null;
        }

        static RectTransform AddRawImageLayer(Transform parent, string name, Texture2D tex)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            if (tex != null)
            {
                var raw = go.AddComponent<RawImage>();
                raw.texture = tex;
                raw.raycastTarget = false;
            }
            return rect;
        }

        static RectTransform AddRawImageLayerSized(Transform parent, string name, Texture2D tex, float targetHeight, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            if (tex != null)
            {
                float ratio = (float)tex.width / tex.height;
                rect.sizeDelta = new Vector2(targetHeight * ratio, targetHeight);
                var raw = go.AddComponent<RawImage>();
                raw.texture = tex;
                raw.raycastTarget = false;
            }
            else
            {
                rect.sizeDelta = new Vector2(targetHeight, targetHeight);
            }
            return rect;
        }

        static RectTransform AddRawImageLayerSizedByWidth(Transform parent, string name, Texture2D tex, float targetWidth, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            if (tex != null)
            {
                float ratio = (float)tex.width / tex.height;
                rect.sizeDelta = new Vector2(targetWidth, targetWidth / ratio);
                var raw = go.AddComponent<RawImage>();
                raw.texture = tex;
                raw.raycastTarget = false;
            }
            else
            {
                rect.sizeDelta = new Vector2(targetWidth, targetWidth * 0.3f);
            }
            return rect;
        }

        static Texture2D FindTexture(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Texture2D {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null) return tex;
            }
            var allGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains(keyword.ToLower()))
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            Debug.LogWarning($"[TitleSceneBuilder] テクスチャが見つかりません: {keyword}");
            return null;
        }

        static AudioClip FindAudioClip(params string[] keywords)
        {
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                foreach (var kw in keywords)
                    if (name.Contains(kw.ToLower())) return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }

        static Color SubJPHellColor() => new Color(0.20f, 0.55f, 0.22f, 1f);
        static Color SubENHellColor() => new Color(0.28f, 0.50f, 0.22f, 0.85f);

        // ─────────────────────────────────────────────────────
        // お題カスタマイズパネル
        // ─────────────────────────────────────────────────────
        static (TopicEditDialog, TopicCustomizePanel) BuildTopicPanel(Transform canvasParent, TMP_FontAsset font)
        {
            // ── オーバーレイ ──
            var overlayGO = new GameObject("TopicCustomizeOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvasParent, false);
            overlayGO.SetActive(false);
            var overlayCG = overlayGO.AddComponent<CanvasGroup>();
            overlayCG.alpha = 0f; overlayCG.blocksRaycasts = false; overlayCG.interactable = false;
            var overlayR = overlayGO.GetComponent<RectTransform>();
            overlayR.anchorMin = Vector2.zero; overlayR.anchorMax = Vector2.one;
            overlayR.offsetMin = Vector2.zero; overlayR.offsetMax = Vector2.zero;

            // バックドロップ
            var bdGO = new GameObject("Backdrop", typeof(RectTransform));
            bdGO.transform.SetParent(overlayGO.transform, false);
            StretchFull(bdGO.GetComponent<RectTransform>());
            bdGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            var backdropBtn = bdGO.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;

            // カード
            var cardGO = new GameObject("Card", typeof(RectTransform));
            cardGO.transform.SetParent(overlayGO.transform, false);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = new Vector2(0.5f, 0.5f); cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(980f, 1560f);
            cardR.anchoredPosition = Vector2.zero;
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = GetBuiltinUISprite(); cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(1f, 0.98f, 0.93f);

            // ヘッダー
            var hdrGO = new GameObject("Header", typeof(RectTransform));
            hdrGO.transform.SetParent(cardGO.transform, false);
            var hdrR = hdrGO.GetComponent<RectTransform>();
            hdrR.anchorMin = new Vector2(0f, 1f); hdrR.anchorMax = new Vector2(1f, 1f);
            hdrR.pivot = new Vector2(0.5f, 1f);
            hdrR.sizeDelta = new Vector2(0f, 118f); hdrR.anchoredPosition = Vector2.zero;
            var hdrImg = hdrGO.AddComponent<Image>();
            hdrImg.sprite = GetBuiltinUISprite(); hdrImg.type = Image.Type.Sliced;
            hdrImg.color = new Color(0.98f, 0.88f, 0.38f);

            var hJP = CreateLabel(hdrGO.transform, "TitleJP", "お題",
                new Vector2(0.5f, 0.5f), new Vector2(500f, 52f), 44);
            hJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, -4f);
            hJP.color = new Color(0.22f, 0.10f, 0.02f); hJP.fontStyle = FontStyles.Bold;
            if (font) hJP.font = font; ApplySharpMaterial(hJP);

            // 閉じるボタン
            var closeBtn = MakeCloseButton(hdrGO.transform, font);

            // ツールバー（追加ボタン・リセットボタン）
            var tbGO = new GameObject("Toolbar", typeof(RectTransform));
            tbGO.transform.SetParent(cardGO.transform, false);
            var tbR = tbGO.GetComponent<RectTransform>();
            tbR.anchorMin = new Vector2(0f, 1f); tbR.anchorMax = new Vector2(1f, 1f);
            tbR.pivot = new Vector2(0.5f, 1f);
            tbR.sizeDelta = new Vector2(0f, 68f); tbR.anchoredPosition = new Vector2(0f, -118f);
            var tbImg = tbGO.AddComponent<Image>();
            tbImg.color = new Color(0.96f, 0.92f, 0.82f, 1f); tbImg.raycastTarget = false;

            var addBtn   = MakeTbButton(tbGO.transform, "AddBtn",   "＋ お題を追加  Add",
                new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(320f, 48f),
                new Color(0.28f, 0.62f, 0.28f, 0.95f), font);
            var resetBtn = MakeTbButton(tbGO.transform, "ResetBtn", "初期化  Reset",
                new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(220f, 40f),
                new Color(0.62f, 0.40f, 0.16f, 0.88f), font);

            // スクロールビュー
            var scrollGO = new GameObject("ScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin = new Vector2(0f, 0f); scrollR.anchorMax = new Vector2(1f, 1f);
            scrollR.offsetMin = new Vector2(0f, 0f); scrollR.offsetMax = new Vector2(0f, -186f);

            var viewGO = new GameObject("Viewport", typeof(RectTransform));
            viewGO.transform.SetParent(scrollGO.transform, false);
            StretchFull(viewGO.GetComponent<RectTransform>());
            viewGO.AddComponent<RectMask2D>();

            var contGO = new GameObject("Content", typeof(RectTransform));
            contGO.transform.SetParent(viewGO.transform, false);
            var contR = contGO.GetComponent<RectTransform>();
            contR.anchorMin = new Vector2(0f, 1f); contR.anchorMax = new Vector2(1f, 1f);
            contR.pivot = new Vector2(0.5f, 1f);
            contR.offsetMin = Vector2.zero; contR.offsetMax = Vector2.zero;
            contR.sizeDelta = new Vector2(0f, 100f);

            var scrollComp = scrollGO.AddComponent<ScrollRect>();
            scrollComp.horizontal = false; scrollComp.vertical = true;
            scrollComp.content = contR; scrollComp.viewport = viewGO.GetComponent<RectTransform>();
            scrollComp.scrollSensitivity = 50f;
            scrollComp.movementType = ScrollRect.MovementType.Clamped;

            // EditDialog を先に作る（TopicCustomizePanelが参照するため）
            var editDialog = BuildTopicEditDialog(canvasParent, font);

            // TopicCustomizePanel コンポーネント
            var panel = overlayGO.AddComponent<TopicCustomizePanel>();
            var pSO = new SerializedObject(panel);
            pSO.FindProperty("overlay").objectReferenceValue     = overlayCG;
            pSO.FindProperty("card").objectReferenceValue        = cardR;
            pSO.FindProperty("closeButton").objectReferenceValue = closeBtn;
            pSO.FindProperty("backdrop").objectReferenceValue    = backdropBtn;
            pSO.FindProperty("listContent").objectReferenceValue = contR;
            pSO.FindProperty("editDialog").objectReferenceValue  = editDialog;
            pSO.FindProperty("addButton").objectReferenceValue   = addBtn;
            pSO.FindProperty("resetButton").objectReferenceValue = resetBtn;
            if (font) pSO.FindProperty("font").objectReferenceValue = font;
            pSO.FindProperty("headerLabel").objectReferenceValue   = hJP;
            pSO.FindProperty("addBtnLabel").objectReferenceValue   = addBtn?.GetComponentInChildren<TextMeshProUGUI>();
            pSO.FindProperty("resetBtnLabel").objectReferenceValue = resetBtn?.GetComponentInChildren<TextMeshProUGUI>();
            pSO.ApplyModifiedProperties();

            return (editDialog, panel);
        }

        static TopicEditDialog BuildTopicEditDialog(Transform canvasParent, TMP_FontAsset font)
        {
            var overlayGO = new GameObject("TopicEditDialogOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvasParent, false);
            overlayGO.SetActive(false);
            var overlayCG = overlayGO.AddComponent<CanvasGroup>();
            overlayCG.alpha = 0f; overlayCG.blocksRaycasts = false; overlayCG.interactable = false;
            StretchFull(overlayGO.GetComponent<RectTransform>());

            var bdGO = new GameObject("Backdrop", typeof(RectTransform));
            bdGO.transform.SetParent(overlayGO.transform, false);
            StretchFull(bdGO.GetComponent<RectTransform>());
            bdGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            var bdBtn = bdGO.AddComponent<Button>();
            bdBtn.transition = Selectable.Transition.None;

            var cardGO = new GameObject("Card", typeof(RectTransform));
            cardGO.transform.SetParent(overlayGO.transform, false);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = new Vector2(0.5f, 0.5f); cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(980f, 1480f); cardR.anchoredPosition = Vector2.zero;
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = GetBuiltinUISprite(); cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(1f, 0.98f, 0.93f);

            // ヘッダー
            var hdrGO = new GameObject("Header", typeof(RectTransform));
            hdrGO.transform.SetParent(cardGO.transform, false);
            var hdrR = hdrGO.GetComponent<RectTransform>();
            hdrR.anchorMin = new Vector2(0f, 1f); hdrR.anchorMax = new Vector2(1f, 1f);
            hdrR.pivot = new Vector2(0.5f, 1f);
            hdrR.sizeDelta = new Vector2(0f, 110f); hdrR.anchoredPosition = Vector2.zero;
            var hdrImg = hdrGO.AddComponent<Image>();
            hdrImg.sprite = GetBuiltinUISprite(); hdrImg.type = Image.Type.Sliced;
            hdrImg.color = new Color(0.35f, 0.60f, 0.90f);

            var titleLbl = CreateLabel(hdrGO.transform, "Title", "お題を追加\nAdd Topic",
                new Vector2(0.5f, 0.5f), new Vector2(700f, 90f), 30);
            titleLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 0f);
            titleLbl.color = Color.white; titleLbl.fontStyle = FontStyles.Bold;
            if (font) titleLbl.font = font; ApplySharpMaterial(titleLbl);

            var cancelBtn = MakeCloseButton(hdrGO.transform, font);

            // フィールドエリア（スクロール）
            var scrollGO = new GameObject("ScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin = new Vector2(0f, 0f); scrollR.anchorMax = new Vector2(1f, 1f);
            scrollR.offsetMin = new Vector2(0f, 130f); scrollR.offsetMax = new Vector2(0f, -110f);

            var viewGO = new GameObject("Viewport", typeof(RectTransform));
            viewGO.transform.SetParent(scrollGO.transform, false);
            StretchFull(viewGO.GetComponent<RectTransform>());
            viewGO.AddComponent<RectMask2D>();

            var contGO = new GameObject("Content", typeof(RectTransform));
            contGO.transform.SetParent(viewGO.transform, false);
            var contR = contGO.GetComponent<RectTransform>();
            contR.anchorMin = new Vector2(0f, 1f); contR.anchorMax = new Vector2(1f, 1f);
            contR.pivot = new Vector2(0.5f, 1f);
            contR.offsetMin = Vector2.zero; contR.offsetMax = Vector2.zero;

            var sc = scrollGO.AddComponent<ScrollRect>();
            sc.horizontal = false; sc.vertical = true;
            sc.content = contR; sc.viewport = viewGO.GetComponent<RectTransform>();
            sc.scrollSensitivity = 50f; sc.movementType = ScrollRect.MovementType.Clamped;

            // 5行 × 2列のフィールドを生成
            var fieldLabels = new[]
            {
                ("お題テキスト", "Topic Text"),
                ("低い指標",     "Low Label"),
                ("高い指標",     "High Label"),
                ("低い数字の例", "Low Example"),
                ("高い数字の例", "High Example"),
            };

            const float rowH = 220f, rowGap = 6f, padTop = 16f;
            var inputFields = new TMP_InputField[10]; // JP0..4, EN5..9
            for (int i = 0; i < 5; i++)
            {
                float y = padTop + i * (rowH + rowGap);
                inputFields[i]     = BuildFieldRow(contR, fieldLabels[i].Item1, fieldLabels[i].Item2,
                                                   true,  y, rowH, font);
                inputFields[i + 5] = BuildFieldRow(contR, fieldLabels[i].Item1, fieldLabels[i].Item2,
                                                   false, y, rowH, font);
            }
            contR.sizeDelta = new Vector2(0f, padTop + 5 * (rowH + rowGap));

            // ガイドラベル（保存ボタン上）
            var guideGO = new GameObject("GuideLabel", typeof(RectTransform));
            guideGO.transform.SetParent(cardGO.transform, false);
            var guideR = guideGO.GetComponent<RectTransform>();
            guideR.anchorMin = new Vector2(0f, 0f); guideR.anchorMax = new Vector2(1f, 0f);
            guideR.pivot = new Vector2(0.5f, 0f);
            guideR.sizeDelta = new Vector2(-48f, 44f);
            guideR.anchoredPosition = new Vector2(0f, 86f);
            var guideTmp = guideGO.AddComponent<TextMeshProUGUI>();
            guideTmp.text = "JP・ENどちらか一方のみでもOK";
            guideTmp.fontSize = 32f;
            guideTmp.alignment = TextAlignmentOptions.Center;
            guideTmp.color = new Color(0.38f, 0.20f, 0.06f, 0.75f);
            guideTmp.enableWordWrapping = false;
            if (font) guideTmp.font = font;
            ApplySharpMaterial(guideTmp);

            // 保存ボタン
            var saveBtnGO = new GameObject("SaveButton", typeof(RectTransform));
            saveBtnGO.transform.SetParent(cardGO.transform, false);
            var saveBtnR = saveBtnGO.GetComponent<RectTransform>();
            saveBtnR.anchorMin = new Vector2(0f, 0f); saveBtnR.anchorMax = new Vector2(1f, 0f);
            saveBtnR.pivot = new Vector2(0.5f, 0f);
            saveBtnR.sizeDelta = new Vector2(-48f, 72f); saveBtnR.anchoredPosition = new Vector2(0f, 8f);
            var saveImg = saveBtnGO.AddComponent<Image>();
            saveImg.sprite = GetBuiltinUISprite(); saveImg.type = Image.Type.Sliced;
            saveImg.color = new Color(0.28f, 0.62f, 0.28f);
            var saveBtn = saveBtnGO.AddComponent<Button>();
            saveBtn.targetGraphic = saveImg;
            var saveLbl = CreateLabel(saveBtnGO.transform, "L", "保存",
                new Vector2(0.5f, 0.5f), new Vector2(400f, 52f), 32);
            saveLbl.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            saveLbl.color = Color.white; saveLbl.fontStyle = FontStyles.Bold;
            if (font) saveLbl.font = font; ApplySharpMaterial(saveLbl);

            // TopicEditDialog コンポーネント
            var dialog = overlayGO.AddComponent<TopicEditDialog>();
            var dSO = new SerializedObject(dialog);
            dSO.FindProperty("overlay").objectReferenceValue      = overlayCG;
            dSO.FindProperty("card").objectReferenceValue         = cardR;
            dSO.FindProperty("saveButton").objectReferenceValue   = saveBtn;
            dSO.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
            dSO.FindProperty("backdrop").objectReferenceValue     = bdBtn;
            dSO.FindProperty("titleLabel").objectReferenceValue   = titleLbl;
            if (font) dSO.FindProperty("font").objectReferenceValue = font;
            // JP fields
            dSO.FindProperty("fTextJP").objectReferenceValue     = inputFields[0];
            dSO.FindProperty("fLowJP").objectReferenceValue      = inputFields[1];
            dSO.FindProperty("fHighJP").objectReferenceValue     = inputFields[2];
            dSO.FindProperty("fHintLowJP").objectReferenceValue  = inputFields[3];
            dSO.FindProperty("fHintHighJP").objectReferenceValue = inputFields[4];
            // EN fields
            dSO.FindProperty("fTextEN").objectReferenceValue     = inputFields[5];
            dSO.FindProperty("fLowEN").objectReferenceValue      = inputFields[6];
            dSO.FindProperty("fHighEN").objectReferenceValue     = inputFields[7];
            dSO.FindProperty("fHintLowEN").objectReferenceValue  = inputFields[8];
            dSO.FindProperty("fHintHighEN").objectReferenceValue = inputFields[9];
            dSO.FindProperty("saveBtnLabel").objectReferenceValue  = saveLbl;
            dSO.FindProperty("guideLabel").objectReferenceValue   = guideTmp;
            dSO.ApplyModifiedProperties();

            return dialog;
        }

        // JP列(isJP=true)またはEN列のTMP_InputFieldを1フィールド分作成して返す
        static TMP_InputField BuildFieldRow(RectTransform parent, string labelJP, string labelEN,
                                            bool isJP, float yTop, float totalH, TMP_FontAsset font)
        {
            float colW = 0.5f;
            float xMin = isJP ? 0f : 0.5f;
            float xMax = isJP ? 0.5f : 1f;
            string lang = isJP ? "JP" : "EN";
            string header = isJP ? labelJP : labelEN;

            var go = new GameObject($"{lang}_{labelJP}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(xMin, 1f); r.anchorMax = new Vector2(xMax, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.sizeDelta = new Vector2(0f, totalH);
            r.anchoredPosition = new Vector2(0f, -yTop);

            // ラベル
            var lgo = new GameObject("Label", typeof(RectTransform));
            lgo.transform.SetParent(go.transform, false);
            var lr = lgo.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 1f); lr.anchorMax = new Vector2(1f, 1f);
            lr.pivot = new Vector2(0f, 1f);
            lr.sizeDelta = new Vector2(-16f, 38f); lr.anchoredPosition = new Vector2(10f, -6f);
            var ltmp = lgo.AddComponent<TextMeshProUGUI>();
            ltmp.text = header; ltmp.fontSize = 32f;
            ltmp.color = new Color(0.30f, 0.18f, 0.06f);
            ltmp.fontStyle = FontStyles.Bold;
            ltmp.alignment = TextAlignmentOptions.MidlineLeft;
            ltmp.raycastTarget = false;
            if (font) ltmp.font = font;

            // 言語バッジ
            var bgo = new GameObject("LangBadge", typeof(RectTransform));
            bgo.transform.SetParent(go.transform, false);
            var br = bgo.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(1f, 1f); br.anchorMax = new Vector2(1f, 1f);
            br.pivot = new Vector2(1f, 1f);
            br.sizeDelta = new Vector2(80f, 44f); br.anchoredPosition = new Vector2(-14f, -4f);
            var bimg = bgo.AddComponent<Image>();
            bimg.sprite = GetBuiltinUISprite(); bimg.type = Image.Type.Sliced;
            bimg.color = isJP ? new Color(0.88f, 0.40f, 0.10f, 0.85f) : new Color(0.22f, 0.46f, 0.78f, 0.85f);
            bimg.raycastTarget = false;
            var btmp_go = new GameObject("T", typeof(RectTransform));
            btmp_go.transform.SetParent(bgo.transform, false);
            var btr = btmp_go.GetComponent<RectTransform>();
            btr.anchorMin = Vector2.zero; btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero; btr.offsetMax = Vector2.zero;
            var btmp = btmp_go.AddComponent<TextMeshProUGUI>();
            btmp.text = lang; btmp.fontSize = 32f; btmp.fontStyle = FontStyles.Bold;
            btmp.alignment = TextAlignmentOptions.Center; btmp.color = Color.white;
            btmp.raycastTarget = false; if (font) btmp.font = font;

            // TMP_InputField
            const float fieldH = 148f;
            var fgo = new GameObject("Field", typeof(RectTransform));
            fgo.transform.SetParent(go.transform, false);
            var fr = fgo.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0f, 0f); fr.anchorMax = new Vector2(1f, 0f);
            fr.pivot = new Vector2(0.5f, 0f);
            fr.sizeDelta = new Vector2(-16f, fieldH); fr.anchoredPosition = new Vector2(0f, 8f);
            var fieldBg = fgo.AddComponent<Image>();
            fieldBg.sprite = GetBuiltinUISprite(); fieldBg.type = Image.Type.Sliced;
            fieldBg.color = new Color(0.96f, 0.94f, 0.88f);

            var textArea = new GameObject("TextArea", typeof(RectTransform));
            textArea.transform.SetParent(fgo.transform, false);
            var taR = textArea.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(8f, 4f); taR.offsetMax = new Vector2(-8f, -4f);
            textArea.AddComponent<RectMask2D>();

            var placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(textArea.transform, false);
            StretchFull(placeholder.GetComponent<RectTransform>());
            var phTmp = placeholder.AddComponent<TextMeshProUGUI>();
            phTmp.text = isJP ? "日本語を入力…" : "Enter in English…";
            phTmp.fontSize = 32f; phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = new Color(0.60f, 0.50f, 0.38f, 0.6f);
            phTmp.alignment = TextAlignmentOptions.TopLeft;
            phTmp.enableWordWrapping = true;
            if (font) phTmp.font = font;

            var inputText = new GameObject("Text", typeof(RectTransform));
            inputText.transform.SetParent(textArea.transform, false);
            StretchFull(inputText.GetComponent<RectTransform>());
            var inTmp = inputText.AddComponent<TextMeshProUGUI>();
            inTmp.text = ""; inTmp.fontSize = 32f;
            inTmp.color = new Color(0.18f, 0.10f, 0.02f);
            inTmp.alignment = TextAlignmentOptions.TopLeft;
            inTmp.enableWordWrapping = true;
            if (font) inTmp.font = font;

            var inputField = fgo.AddComponent<TMP_InputField>();
            inputField.textViewport = taR;
            inputField.textComponent = inTmp;
            inputField.placeholder = phTmp;
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            inputField.characterLimit = 80;
            inputField.targetGraphic = fieldBg;

            return inputField;
        }

        static Button MakeTbButton(Transform parent, string name, string label,
                                   Vector2 anchor, Vector2 pos, Vector2 size, Color color, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(anchor.x, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.sprite = GetBuiltinUISprite(); img.type = Image.Type.Sliced; img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            var lgo = new GameObject("L", typeof(RectTransform));
            lgo.transform.SetParent(go.transform, false);
            StretchFull(lgo.GetComponent<RectTransform>());
            var tmp = lgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 32f; tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
            tmp.raycastTarget = false; if (font) tmp.font = font; ApplySharpMaterial(tmp);
            return btn;
        }

        static Button MakeCloseButton(Transform hdrParent, TMP_FontAsset font)
        {
            var go = new GameObject("CloseButton", typeof(RectTransform));
            go.transform.SetParent(hdrParent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 0.5f); r.anchorMax = new Vector2(1f, 0.5f);
            r.pivot = new Vector2(1f, 0.5f);
            r.sizeDelta = new Vector2(58f, 58f); r.anchoredPosition = new Vector2(-22f, 0f);
            var img = go.AddComponent<Image>();
            img.sprite = GetPillSprite(); img.type = Image.Type.Sliced;
            img.color = new Color(0.58f, 0.32f, 0.08f, 0.88f);
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            var xgo = new GameObject("X", typeof(RectTransform));
            xgo.transform.SetParent(go.transform, false);
            StretchFull(xgo.GetComponent<RectTransform>());
            var xtmp = xgo.AddComponent<TextMeshProUGUI>();
            xtmp.text = "×"; xtmp.fontSize = 32f;
            xtmp.alignment = TextAlignmentOptions.Center; xtmp.color = Color.white;
            xtmp.raycastTarget = false; if (font) xtmp.font = font; ApplySharpMaterial(xtmp);
            return btn;
        }

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        // ─────────────────────────────────────────
        // ルールパネル
        // ─────────────────────────────────────────
        static RulesPanel BuildRulesPanel(Transform canvasParent, TMP_FontAsset font)
        {
            // ── オーバーレイ（初期非表示）──
            var overlayGO = new GameObject("RulesOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvasParent, false);
            overlayGO.SetActive(false);
            var overlayCG = overlayGO.AddComponent<CanvasGroup>();
            overlayCG.alpha = 0f;
            overlayCG.blocksRaycasts = false;
            overlayCG.interactable   = false;
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // ── バックドロップ（タップで閉じる）──
            var bdGO = new GameObject("Backdrop", typeof(RectTransform));
            bdGO.transform.SetParent(overlayGO.transform, false);
            var bdRect = bdGO.GetComponent<RectTransform>();
            bdRect.anchorMin = Vector2.zero;
            bdRect.anchorMax = Vector2.one;
            bdRect.offsetMin = Vector2.zero;
            bdRect.offsetMax = Vector2.zero;
            bdGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            var backdropBtn = bdGO.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;

            // ── カードシャドウ ──
            var shadowGO = new GameObject("CardShadow", typeof(RectTransform));
            shadowGO.transform.SetParent(overlayGO.transform, false);
            var shadowR = shadowGO.GetComponent<RectTransform>();
            shadowR.anchorMin = new Vector2(0.5f, 0.5f);
            shadowR.anchorMax = new Vector2(0.5f, 0.5f);
            shadowR.pivot     = new Vector2(0.5f, 0.5f);
            shadowR.sizeDelta = new Vector2(1000f, 1256f);
            shadowR.anchoredPosition = new Vector2(10f, -16f);
            var shadowImg2 = shadowGO.AddComponent<Image>();
            shadowImg2.sprite = GetBuiltinUISprite();
            shadowImg2.type   = Image.Type.Sliced;
            shadowImg2.color  = new Color(0.12f, 0.06f, 0.01f, 0.50f);
            shadowImg2.raycastTarget = false;

            // ── カード本体 ──
            var cardGO = new GameObject("Card", typeof(RectTransform));
            cardGO.transform.SetParent(overlayGO.transform, false);
            var cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot     = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(980f, 1240f);
            cardRect.anchoredPosition = Vector2.zero;
            var cardImg2 = cardGO.AddComponent<Image>();
            cardImg2.sprite = GetBuiltinUISprite();
            cardImg2.type   = Image.Type.Sliced;
            cardImg2.color  = new Color(1f, 0.98f, 0.93f);

            // ── ヘッダー ──
            var hdrGO = new GameObject("Header", typeof(RectTransform));
            hdrGO.transform.SetParent(cardGO.transform, false);
            var hdrRect = hdrGO.GetComponent<RectTransform>();
            hdrRect.anchorMin = new Vector2(0f, 1f);
            hdrRect.anchorMax = new Vector2(1f, 1f);
            hdrRect.pivot     = new Vector2(0.5f, 1f);
            hdrRect.sizeDelta = new Vector2(0f, 118f);
            hdrRect.anchoredPosition = Vector2.zero;
            var hdrImg = hdrGO.AddComponent<Image>();
            hdrImg.sprite = GetBuiltinUISprite();
            hdrImg.type   = Image.Type.Sliced;
            hdrImg.color  = new Color(0.98f, 0.88f, 0.38f);

            // ヘッダータイトル
            var hTJ = CreateLabel(hdrGO.transform, "TitleJP", "ルール",
                new Vector2(0.5f, 0.5f), new Vector2(500f, 58f), 46);
            hTJ.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 0f);
            hTJ.color = new Color(0.22f, 0.10f, 0.02f);
            hTJ.fontStyle = FontStyles.Bold;
            if (font != null) hTJ.font = font;
            ApplySharpMaterial(hTJ);

            var hTE = CreateLabel(hdrGO.transform, "TitleEN", "How to Play",
                new Vector2(0.5f, 0.5f), new Vector2(500f, 58f), 46);
            hTE.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 0f);
            hTE.color = new Color(0.22f, 0.10f, 0.02f);
            hTE.fontStyle = FontStyles.Bold;
            if (font != null) hTE.font = font;
            ApplySharpMaterial(hTE);
            hTE.gameObject.SetActive(false);

            // 閉じるボタン
            var closeBtnGO = new GameObject("CloseButton", typeof(RectTransform));
            closeBtnGO.transform.SetParent(hdrGO.transform, false);
            var cbRect = closeBtnGO.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(1f, 0.5f);
            cbRect.anchorMax = new Vector2(1f, 0.5f);
            cbRect.pivot     = new Vector2(1f, 0.5f);
            cbRect.sizeDelta = new Vector2(58f, 58f);
            cbRect.anchoredPosition = new Vector2(-22f, 0f);
            var cbImg = closeBtnGO.AddComponent<Image>();
            cbImg.sprite = GetPillSprite();
            cbImg.type   = Image.Type.Sliced;
            cbImg.color  = new Color(0.58f, 0.32f, 0.08f, 0.88f);
            var closeBtn = closeBtnGO.AddComponent<Button>();
            var cbCols = closeBtn.colors;
            cbCols.highlightedColor = new Color(0.78f, 0.48f, 0.14f, 1f);
            cbCols.pressedColor     = new Color(0.38f, 0.18f, 0.04f, 1f);
            closeBtn.colors = cbCols;
            closeBtn.targetGraphic = cbImg;
            var cbXGO = new GameObject("X", typeof(RectTransform));
            cbXGO.transform.SetParent(closeBtnGO.transform, false);
            var cbXRect = cbXGO.GetComponent<RectTransform>();
            cbXRect.anchorMin = Vector2.zero;
            cbXRect.anchorMax = Vector2.one;
            cbXRect.offsetMin = Vector2.zero;
            cbXRect.offsetMax = Vector2.zero;
            var cbXTmp = cbXGO.AddComponent<TextMeshProUGUI>();
            cbXTmp.text = "×";
            cbXTmp.fontSize = 32f;
            cbXTmp.alignment = TextAlignmentOptions.Center;
            cbXTmp.color = Color.white;
            cbXTmp.raycastTarget = false;
            if (font != null) cbXTmp.font = font;
            ApplySharpMaterial(cbXTmp);

            // ── スクロールビュー ──
            var scrollGO2 = new GameObject("ScrollView", typeof(RectTransform));
            scrollGO2.transform.SetParent(cardGO.transform, false);
            var scrollR = scrollGO2.GetComponent<RectTransform>();
            scrollR.anchorMin = new Vector2(0f, 0f);
            scrollR.anchorMax = new Vector2(1f, 1f);
            scrollR.offsetMin = new Vector2(0f, 0f);
            scrollR.offsetMax = new Vector2(0f, -118f);

            var viewGO = new GameObject("Viewport", typeof(RectTransform));
            viewGO.transform.SetParent(scrollGO2.transform, false);
            var viewR = viewGO.GetComponent<RectTransform>();
            viewR.anchorMin = Vector2.zero;
            viewR.anchorMax = Vector2.one;
            viewR.offsetMin = Vector2.zero;
            viewR.offsetMax = Vector2.zero;
            viewGO.AddComponent<RectMask2D>();

            // Content: 手動レイアウト（VLG+ContentSizeFitterはEditor生成シーンでは不安定）
            var contGO = new GameObject("Content", typeof(RectTransform));
            contGO.transform.SetParent(viewGO.transform, false);
            var contR = contGO.GetComponent<RectTransform>();
            contR.anchorMin = new Vector2(0f, 1f);
            contR.anchorMax = new Vector2(1f, 1f);
            contR.pivot     = new Vector2(0.5f, 1f);
            contR.offsetMin = Vector2.zero;
            contR.offsetMax = Vector2.zero;

            var scrollComp = scrollGO2.AddComponent<ScrollRect>();
            scrollComp.horizontal        = false;
            scrollComp.vertical          = true;
            scrollComp.content           = contR;
            scrollComp.viewport          = viewR;
            scrollComp.scrollSensitivity = 40f;
            scrollComp.movementType      = ScrollRect.MovementType.Clamped;

            // ── ルール項目（手動Y座標） ──
            var rules = new (string jp, string en)[]
            {
                ("お題と秘密の数字が配られる",              "A topic and secret number are dealt to each player"),
                ("配られた数字を直接伝えてはいけない",        "Never directly reveal your number to others"),
                ("お題に対し、数字のレベルに合う答えを言う",   "Give an answer matching the level of your number for the topic"),
                ("全員またはチームで相談して数字を当てる",     "Everyone or teams discuss and guess the number"),
                ("予想と実際の差がマイナス点になる",          "The gap between guess and actual number becomes minus points"),
                ("ピッタリ当てたら参加人数分のライフが増える",  "A perfect guess earns lives equal to the player count"),
                ("協力モード：ライフ０にならず全ターンが終われば勝利", "Coop: Win if all turns finish before lives reach zero"),
                ("チームモード：マイナス合計が少ないチームが勝ち",   "Team: The team with fewer total minus points wins"),
                ("ヘルプカードを使うとマイナスが４に固定される",       "Using a help card fixes your minus points at 4"),
            };

            const float itemH   = 90f;
            const float itemGap = 4f;
            const float padH    = 20f;
            float yOff = padH;

            for (int i = 0; i < rules.Length; i++)
            {
                CreateRuleItemAt(contGO.transform, i + 1, rules[i].jp, rules[i].en, font, yOff, itemH);
                yOff += itemH + itemGap;
            }
            yOff += 16f;  // 下padding

            // コンテンツ高さを確定
            contR.sizeDelta = new Vector2(0f, yOff);

            // ── RulesPanel コンポーネント ──
            var panel = overlayGO.AddComponent<RulesPanel>();
            var pSO = new SerializedObject(panel);
            pSO.FindProperty("overlay").objectReferenceValue      = overlayCG;
            pSO.FindProperty("card").objectReferenceValue         = cardRect;
            pSO.FindProperty("closeButton").objectReferenceValue  = closeBtn;
            pSO.FindProperty("backdrop").objectReferenceValue     = backdropBtn;
            pSO.FindProperty("headerJP").objectReferenceValue     = hTJ;
            pSO.FindProperty("headerEN").objectReferenceValue     = hTE;
            pSO.FindProperty("rulesContent").objectReferenceValue = contGO.transform;
            pSO.ApplyModifiedProperties();

            return panel;
        }

        static void CreateRuleItemAt(Transform parent, int number, string jp, string en, TMP_FontAsset font, float yTop, float height)
        {
            var go = new GameObject($"Rule{number}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 1f);
            r.anchorMax        = new Vector2(1f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(0f, height);
            r.anchoredPosition = new Vector2(0f, -yTop);

            if (number % 2 == 0)
            {
                var rowImg = go.AddComponent<Image>();
                rowImg.color = new Color(0.96f, 0.92f, 0.78f, 0.40f);
                rowImg.raycastTarget = false;
            }

            var badgeGO = new GameObject("Badge", typeof(RectTransform));
            badgeGO.transform.SetParent(go.transform, false);
            var badgeR = badgeGO.GetComponent<RectTransform>();
            badgeR.anchorMin = new Vector2(0f, 0.5f);
            badgeR.anchorMax = new Vector2(0f, 0.5f);
            badgeR.pivot     = new Vector2(0f, 0.5f);
            badgeR.sizeDelta = new Vector2(40f, 40f);
            badgeR.anchoredPosition = new Vector2(10f, 0f);
            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.sprite = GetPillSprite();
            badgeImg.type   = Image.Type.Sliced;
            badgeImg.color  = RuleBulletColor(number);
            badgeImg.raycastTarget = false;
            var numGO = new GameObject("Num", typeof(RectTransform));
            numGO.transform.SetParent(badgeGO.transform, false);
            var numR = numGO.GetComponent<RectTransform>();
            numR.anchorMin = Vector2.zero;
            numR.anchorMax = Vector2.one;
            numR.offsetMin = Vector2.zero;
            numR.offsetMax = Vector2.zero;
            var numTmp = numGO.AddComponent<TextMeshProUGUI>();
            numTmp.text          = number.ToString();
            numTmp.fontSize      = 32f;
            numTmp.fontStyle     = FontStyles.Bold;
            numTmp.alignment     = TextAlignmentOptions.Center;
            numTmp.color         = Color.white;
            numTmp.raycastTarget = false;
            ApplySharpMaterial(numTmp);
            if (font != null) numTmp.font = font;

            var jpGO = new GameObject("JP", typeof(RectTransform));
            jpGO.transform.SetParent(go.transform, false);
            var jpR = jpGO.GetComponent<RectTransform>();
            jpR.anchorMin        = new Vector2(0f, 0.5f);
            jpR.anchorMax        = new Vector2(1f, 0.5f);
            jpR.pivot            = new Vector2(0.5f, 0.5f);
            jpR.sizeDelta        = new Vector2(-80f, 78f);
            jpR.anchoredPosition = new Vector2(28f, 0f);
            var jpTmp = jpGO.AddComponent<TextMeshProUGUI>();
            jpTmp.text               = jp;
            jpTmp.fontSize           = 32f;
            jpTmp.fontStyle          = FontStyles.Bold;
            jpTmp.alignment          = TextAlignmentOptions.MidlineLeft;
            jpTmp.enableWordWrapping = true;
            jpTmp.overflowMode       = TextOverflowModes.Overflow;
            jpTmp.characterSpacing   = 1f;
            jpTmp.color              = new Color(0.22f, 0.10f, 0.02f);
            jpTmp.raycastTarget      = false;
            ApplySharpMaterial(jpTmp);
            if (font != null) jpTmp.font = font;

            var enGO = new GameObject("EN", typeof(RectTransform));
            enGO.transform.SetParent(go.transform, false);
            var enR = enGO.GetComponent<RectTransform>();
            enR.anchorMin        = new Vector2(0f, 0.5f);
            enR.anchorMax        = new Vector2(1f, 0.5f);
            enR.pivot            = new Vector2(0.5f, 0.5f);
            enR.sizeDelta        = new Vector2(-80f, 78f);
            enR.anchoredPosition = new Vector2(28f, 0f);
            var enTmp = enGO.AddComponent<TextMeshProUGUI>();
            enTmp.text               = en;
            enTmp.fontSize           = 32f;
            enTmp.fontStyle          = FontStyles.Bold;
            enTmp.alignment          = TextAlignmentOptions.MidlineLeft;
            enTmp.enableWordWrapping = true;
            enTmp.overflowMode       = TextOverflowModes.Overflow;
            enTmp.characterSpacing   = 1f;
            enTmp.color              = new Color(0.22f, 0.10f, 0.02f);
            enTmp.raycastTarget      = false;
            ApplySharpMaterial(enTmp);
            if (font != null) enTmp.font = font;
            enGO.SetActive(false);
        }

        static void CreateRuleItem(Transform parent, int number, string jp, string en, TMP_FontAsset font)
        {
            var go = new GameObject($"Rule{number}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(0f, 94f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 94f;
            le.flexibleWidth   = 1f;

            // 偶数行に薄い背景
            if (number % 2 == 0)
            {
                var rowImg = go.AddComponent<Image>();
                rowImg.color = new Color(0.96f, 0.92f, 0.78f, 0.40f);
                rowImg.raycastTarget = false;
            }

            // ナンバーバッジ
            var badgeGO = new GameObject("Badge", typeof(RectTransform));
            badgeGO.transform.SetParent(go.transform, false);
            var badgeR = badgeGO.GetComponent<RectTransform>();
            badgeR.anchorMin = new Vector2(0f, 0.5f);
            badgeR.anchorMax = new Vector2(0f, 0.5f);
            badgeR.pivot     = new Vector2(0f, 0.5f);
            badgeR.sizeDelta = new Vector2(40f, 40f);
            badgeR.anchoredPosition = new Vector2(10f, 0f);
            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.sprite = GetPillSprite();
            badgeImg.type   = Image.Type.Sliced;
            badgeImg.color  = RuleBulletColor(number);
            badgeImg.raycastTarget = false;
            var numGO2 = new GameObject("Num", typeof(RectTransform));
            numGO2.transform.SetParent(badgeGO.transform, false);
            var numR = numGO2.GetComponent<RectTransform>();
            numR.anchorMin = Vector2.zero;
            numR.anchorMax = Vector2.one;
            numR.offsetMin = Vector2.zero;
            numR.offsetMax = Vector2.zero;
            var numTmp2 = numGO2.AddComponent<TextMeshProUGUI>();
            numTmp2.text          = number.ToString();
            numTmp2.fontSize      = 32f;
            numTmp2.fontStyle     = FontStyles.Bold;
            numTmp2.alignment     = TextAlignmentOptions.Center;
            numTmp2.color         = Color.white;
            numTmp2.raycastTarget = false;
            if (font != null) numTmp2.font = font;
            ApplySharpMaterial(numTmp2);

            // JP テキスト（上段）
            var jpGO2 = new GameObject("JP", typeof(RectTransform));
            jpGO2.transform.SetParent(go.transform, false);
            var jpR = jpGO2.GetComponent<RectTransform>();
            jpR.anchorMin        = new Vector2(0f, 0.5f);
            jpR.anchorMax        = new Vector2(1f, 0.5f);
            jpR.pivot            = new Vector2(0.5f, 0.5f);
            jpR.sizeDelta        = new Vector2(-78f, 36f);
            jpR.anchoredPosition = new Vector2(24f, 18f);
            var jpTmp2 = jpGO2.AddComponent<TextMeshProUGUI>();
            jpTmp2.text               = jp;
            jpTmp2.fontSize           = 32f;
            jpTmp2.fontStyle          = FontStyles.Bold;
            jpTmp2.alignment          = TextAlignmentOptions.MidlineLeft;
            jpTmp2.enableWordWrapping = false;
            jpTmp2.overflowMode       = TextOverflowModes.Ellipsis;
            jpTmp2.characterSpacing   = 1f;
            jpTmp2.color              = new Color(0.22f, 0.10f, 0.02f);
            jpTmp2.raycastTarget      = false;
            if (font != null) jpTmp2.font = font;
            ApplySharpMaterial(jpTmp2);

            // EN テキスト（下段）
            var enGO2 = new GameObject("EN", typeof(RectTransform));
            enGO2.transform.SetParent(go.transform, false);
            var enR = enGO2.GetComponent<RectTransform>();
            enR.anchorMin        = new Vector2(0f, 0.5f);
            enR.anchorMax        = new Vector2(1f, 0.5f);
            enR.pivot            = new Vector2(0.5f, 0.5f);
            enR.sizeDelta        = new Vector2(-78f, 28f);
            enR.anchoredPosition = new Vector2(24f, -20f);
            var enTmp2 = enGO2.AddComponent<TextMeshProUGUI>();
            enTmp2.text               = en;
            enTmp2.fontSize           = 32f;
            enTmp2.alignment          = TextAlignmentOptions.MidlineLeft;
            enTmp2.enableWordWrapping = false;
            enTmp2.overflowMode       = TextOverflowModes.Ellipsis;
            enTmp2.characterSpacing   = 1f;
            enTmp2.color              = new Color(0.44f, 0.30f, 0.14f);
            enTmp2.raycastTarget      = false;
            if (font != null) enTmp2.font = font;
            ApplySharpMaterial(enTmp2);
        }

        static Color RuleBulletColor(int n)
        {
            var p = new Color[]
            {
                new Color(0.98f, 0.78f, 0.18f),
                new Color(0.98f, 0.60f, 0.22f),
                new Color(0.88f, 0.38f, 0.28f),
                new Color(0.72f, 0.34f, 0.62f),
                new Color(0.36f, 0.60f, 0.88f),
                new Color(0.30f, 0.75f, 0.58f),
                new Color(0.48f, 0.78f, 0.32f),
                new Color(0.85f, 0.68f, 0.26f),
            };
            return p[(n - 1) % p.Length];
        }

        static void CreateHelpCardSectionAt(Transform parent, Texture2D iconTex, TMP_FontAsset font, float yTop, float height)
        {
            const string jpText = "ヘルプカードを使うとマイナスが４に固定される";
            const string enText = "Using a help card fixes your minus points at 4";

            var go = new GameObject("HelpCardSection", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 1f);
            r.anchorMax        = new Vector2(1f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(0f, height);
            r.anchoredPosition = new Vector2(0f, -yTop);

            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = GetBuiltinUISprite();
            bgImg.type   = Image.Type.Sliced;
            bgImg.color  = new Color(0.98f, 0.90f, 0.55f, 0.90f);
            bgImg.raycastTarget = false;

            float iconW = 0f;
            if (iconTex != null)
            {
                iconW = 80f;
                float ratio = (float)iconTex.width / iconTex.height;
                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(go.transform, false);
                var iconR = iconGO.GetComponent<RectTransform>();
                iconR.anchorMin        = new Vector2(0f, 0.5f);
                iconR.anchorMax        = new Vector2(0f, 0.5f);
                iconR.pivot            = new Vector2(0f, 0.5f);
                iconR.sizeDelta        = new Vector2(iconW, iconW / ratio);
                iconR.anchoredPosition = new Vector2(18f, 0f);
                var iconRaw = iconGO.AddComponent<RawImage>();
                iconRaw.texture      = iconTex;
                iconRaw.raycastTarget = false;
            }

            float xOff = iconW > 0f ? (iconW + 30f) : 0f;

            var hcJP = new GameObject("JP", typeof(RectTransform));
            hcJP.transform.SetParent(go.transform, false);
            var hcJPR = hcJP.GetComponent<RectTransform>();
            hcJPR.anchorMin        = new Vector2(0f, 0.5f);
            hcJPR.anchorMax        = new Vector2(1f, 0.5f);
            hcJPR.pivot            = new Vector2(0.5f, 0.5f);
            hcJPR.sizeDelta        = new Vector2(-(xOff + 20f), 38f);
            hcJPR.anchoredPosition = new Vector2(xOff * 0.5f, 16f);
            var hcJPTmp = hcJP.AddComponent<TextMeshProUGUI>();
            hcJPTmp.text               = jpText;
            hcJPTmp.fontSize           = 32f;
            hcJPTmp.fontStyle          = FontStyles.Bold;
            hcJPTmp.alignment          = iconTex != null ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
            hcJPTmp.enableWordWrapping = true;
            hcJPTmp.characterSpacing   = 1f;
            hcJPTmp.color              = new Color(0.35f, 0.18f, 0.02f);
            hcJPTmp.raycastTarget      = false;
            if (font != null) hcJPTmp.font = font;
            ApplySharpMaterial(hcJPTmp);

            var hcEN = new GameObject("EN", typeof(RectTransform));
            hcEN.transform.SetParent(go.transform, false);
            var hcENR = hcEN.GetComponent<RectTransform>();
            hcENR.anchorMin        = new Vector2(0f, 0.5f);
            hcENR.anchorMax        = new Vector2(1f, 0.5f);
            hcENR.pivot            = new Vector2(0.5f, 0.5f);
            hcENR.sizeDelta        = new Vector2(-(xOff + 20f), 28f);
            hcENR.anchoredPosition = new Vector2(xOff * 0.5f, -20f);
            var hcENTmp = hcEN.AddComponent<TextMeshProUGUI>();
            hcENTmp.text               = enText;
            hcENTmp.fontSize           = 32f;
            hcENTmp.alignment          = iconTex != null ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
            hcENTmp.enableWordWrapping = true;
            hcENTmp.characterSpacing   = 1f;
            hcENTmp.color              = new Color(0.50f, 0.32f, 0.10f);
            hcENTmp.raycastTarget      = false;
            if (font != null) hcENTmp.font = font;
            ApplySharpMaterial(hcENTmp);
        }

        static void CreateHelpCardSection(Transform parent, Texture2D iconTex, TMP_FontAsset font)
        {
            const string jpText = "ヘルプカードを使うとマイナスが４に固定される";
            const string enText = "Using a help card fixes your minus points at 4";

            var go = new GameObject("HelpCardSection", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(0f, 140f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 140f;
            le.flexibleWidth   = 1f;

            var bgImg2 = go.AddComponent<Image>();
            bgImg2.sprite = GetBuiltinUISprite();
            bgImg2.type   = Image.Type.Sliced;
            bgImg2.color  = new Color(0.98f, 0.90f, 0.55f, 0.90f);
            bgImg2.raycastTarget = false;

            float iconW = 0f;
            if (iconTex != null)
            {
                iconW = 80f;
                float ratio = (float)iconTex.width / iconTex.height;
                var iconGO2 = new GameObject("Icon", typeof(RectTransform));
                iconGO2.transform.SetParent(go.transform, false);
                var iconR = iconGO2.GetComponent<RectTransform>();
                iconR.anchorMin        = new Vector2(0f, 0.5f);
                iconR.anchorMax        = new Vector2(0f, 0.5f);
                iconR.pivot            = new Vector2(0f, 0.5f);
                iconR.sizeDelta        = new Vector2(iconW, iconW / ratio);
                iconR.anchoredPosition = new Vector2(18f, 0f);
                var iconRawImg = iconGO2.AddComponent<RawImage>();
                iconRawImg.texture      = iconTex;
                iconRawImg.raycastTarget = false;
            }

            float xOff = iconW > 0f ? (iconW + 30f) : 0f;

            var hcJP = new GameObject("JP", typeof(RectTransform));
            hcJP.transform.SetParent(go.transform, false);
            var hcJPR = hcJP.GetComponent<RectTransform>();
            hcJPR.anchorMin        = new Vector2(0f, 0.5f);
            hcJPR.anchorMax        = new Vector2(1f, 0.5f);
            hcJPR.pivot            = new Vector2(0.5f, 0.5f);
            hcJPR.sizeDelta        = new Vector2(-(xOff + 20f), 38f);
            hcJPR.anchoredPosition = new Vector2(xOff * 0.5f, 16f);
            var hcJPTmp = hcJP.AddComponent<TextMeshProUGUI>();
            hcJPTmp.text               = jpText;
            hcJPTmp.fontSize           = 32f;
            hcJPTmp.fontStyle          = FontStyles.Bold;
            hcJPTmp.alignment          = iconTex != null ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
            hcJPTmp.enableWordWrapping = true;
            hcJPTmp.characterSpacing   = 1f;
            hcJPTmp.color              = new Color(0.35f, 0.18f, 0.02f);
            hcJPTmp.raycastTarget      = false;
            if (font != null) hcJPTmp.font = font;
            ApplySharpMaterial(hcJPTmp);

            var hcEN = new GameObject("EN", typeof(RectTransform));
            hcEN.transform.SetParent(go.transform, false);
            var hcENR = hcEN.GetComponent<RectTransform>();
            hcENR.anchorMin        = new Vector2(0f, 0.5f);
            hcENR.anchorMax        = new Vector2(1f, 0.5f);
            hcENR.pivot            = new Vector2(0.5f, 0.5f);
            hcENR.sizeDelta        = new Vector2(-(xOff + 20f), 28f);
            hcENR.anchoredPosition = new Vector2(xOff * 0.5f, -20f);
            var hcENTmp = hcEN.AddComponent<TextMeshProUGUI>();
            hcENTmp.text               = enText;
            hcENTmp.fontSize           = 32f;
            hcENTmp.alignment          = iconTex != null ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
            hcENTmp.enableWordWrapping = true;
            hcENTmp.characterSpacing   = 1f;
            hcENTmp.color              = new Color(0.50f, 0.32f, 0.10f);
            hcENTmp.raycastTarget      = false;
            if (font != null) hcENTmp.font = font;
            ApplySharpMaterial(hcENTmp);
        }
    }
}
