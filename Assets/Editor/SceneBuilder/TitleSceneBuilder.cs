using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Title;

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
                new Vector2(0.5f, 0.5f), new Vector2(920f, 72f), 44);  // 46 * 0.95
            subJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -740f);
            subJP.color = new Color(1f, 0.62f, 0.18f, 1f);   // 明るく可愛いオレンジ
            subJP.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) subJP.font = jpFont;
            ApplySharpMaterial(subJP);

            var subEN = CreateLabel(titleGroupGO.transform, "SubtitleEN",
                "Party game for 2 to 24 players", new Vector2(0.5f, 0.5f), new Vector2(920f, 52f), 28);
            subEN.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -812f);
            subEN.color = new Color(0.68f, 0.52f, 0.32f, 0.85f); // パステルブラウン（英語）
            if (jpFont != null) subEN.font = jpFont;
            ApplySharpMaterial(subEN);

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
            hellDescJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -882f);
            hellDescJP.color = SubJPHellColor();
            hellDescJP.fontStyle = TMPro.FontStyles.Bold;
            if (jpFont != null) hellDescJP.font = jpFont;
            ApplySharpMaterial(hellDescJP);

            var hellDescEN = CreateLabel(hellDescGO.transform, "HellDescEN",
                "Life 1/2  No Help Cards", new Vector2(0.5f, 0.5f), new Vector2(920f, 38f), 22);
            hellDescEN.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -940f);
            hellDescEN.color = SubENHellColor();
            if (jpFont != null) hellDescEN.font = jpFont;
            ApplySharpMaterial(hellDescEN);

            // ─── 上部ボタンバー ───
            var rulesBtn  = CreateTopBarButton(titleGroupGO.transform, "RulesButton",  "ルール", new Vector2(0f,1f), new Vector2( 54f,-191f), new Vector2(152f,54f), jpFont);
            var topicsBtn = CreateTopBarButton(titleGroupGO.transform, "TopicsButton", "お題",   new Vector2(0f,1f), new Vector2(222f,-191f), new Vector2(120f,54f), jpFont);
            var hellBtnGO = CreateTopBarButtonGO(titleGroupGO.transform, "HellModeButton", "地獄モード OFF", new Vector2(1f,1f), new Vector2(-54f,-191f), new Vector2(260f,54f), jpFont);

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
            topBarSO.ApplyModifiedProperties();

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
                tmp.fontSize         = 28;
                tmp.characterSpacing = 2f;
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
            hTJ.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 10f);
            hTJ.color = new Color(0.22f, 0.10f, 0.02f);
            hTJ.fontStyle = FontStyles.Bold;
            if (font != null) hTJ.font = font;
            ApplySharpMaterial(hTJ);

            var hTE = CreateLabel(hdrGO.transform, "TitleEN", "How to Play",
                new Vector2(0.5f, 0.5f), new Vector2(500f, 34f), 22);
            hTE.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, -30f);
            hTE.color = new Color(0.40f, 0.22f, 0.06f);
            if (font != null) hTE.font = font;
            ApplySharpMaterial(hTE);

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
            cbXTmp.fontSize = 30f;
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
            var viewImg = viewGO.AddComponent<Image>();
            viewImg.color = new Color(1f, 1f, 1f, 0f);
            viewGO.AddComponent<Mask>().showMaskGraphic = false;

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
            };

            const float itemH   = 96f;
            const float itemGap = 4f;
            const float padH    = 20f;
            float yOff = padH;

            for (int i = 0; i < rules.Length; i++)
            {
                CreateRuleItemAt(contGO.transform, i + 1, rules[i].jp, rules[i].en, font, yOff, itemH);
                yOff += itemH + itemGap;
            }
            yOff -= itemGap;

            // セパレーター
            yOff += 14f;
            var sepGO = new GameObject("Separator", typeof(RectTransform));
            sepGO.transform.SetParent(contGO.transform, false);
            var sepR = sepGO.GetComponent<RectTransform>();
            sepR.anchorMin = new Vector2(0f, 1f); sepR.anchorMax = new Vector2(1f, 1f);
            sepR.pivot = new Vector2(0.5f, 1f);
            sepR.sizeDelta = new Vector2(-40f, 2f);
            sepR.anchoredPosition = new Vector2(0f, -yOff);
            sepGO.AddComponent<Image>().color = new Color(0.88f, 0.76f, 0.44f, 0.7f);
            yOff += 2f;

            // ヘルプカードセクション
            yOff += 10f;
            const float helpH = 148f;
            var helpTex = FindTexture("card") ?? FindTexture("helpcard") ?? FindTexture("help_card") ?? FindTexture("help");
            CreateHelpCardSectionAt(contGO.transform, helpTex, font, yOff, helpH);
            yOff += helpH + 24f;  // 下padding

            // コンテンツ高さを確定
            contR.sizeDelta = new Vector2(0f, yOff);

            // ── RulesPanel コンポーネント ──
            var panel = overlayGO.AddComponent<RulesPanel>();
            var pSO = new SerializedObject(panel);
            pSO.FindProperty("overlay").objectReferenceValue     = overlayCG;
            pSO.FindProperty("card").objectReferenceValue        = cardRect;
            pSO.FindProperty("closeButton").objectReferenceValue = closeBtn;
            pSO.FindProperty("backdrop").objectReferenceValue    = backdropBtn;
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
            numTmp.fontSize      = 20f;
            numTmp.fontStyle     = FontStyles.Bold;
            numTmp.alignment     = TextAlignmentOptions.Center;
            numTmp.color         = Color.white;
            numTmp.raycastTarget = false;
            if (font != null) numTmp.font = font;
            ApplySharpMaterial(numTmp);

            var jpGO = new GameObject("JP", typeof(RectTransform));
            jpGO.transform.SetParent(go.transform, false);
            var jpR = jpGO.GetComponent<RectTransform>();
            jpR.anchorMin        = new Vector2(0f, 0.5f);
            jpR.anchorMax        = new Vector2(1f, 0.5f);
            jpR.pivot            = new Vector2(0.5f, 0.5f);
            jpR.sizeDelta        = new Vector2(-78f, 36f);
            jpR.anchoredPosition = new Vector2(24f, 18f);
            var jpTmp = jpGO.AddComponent<TextMeshProUGUI>();
            jpTmp.text               = jp;
            jpTmp.fontSize           = 27f;
            jpTmp.fontStyle          = FontStyles.Bold;
            jpTmp.alignment          = TextAlignmentOptions.MidlineLeft;
            jpTmp.enableWordWrapping = false;
            jpTmp.overflowMode       = TextOverflowModes.Ellipsis;
            jpTmp.characterSpacing   = 1f;
            jpTmp.color              = new Color(0.22f, 0.10f, 0.02f);
            jpTmp.raycastTarget      = false;
            if (font != null) jpTmp.font = font;
            ApplySharpMaterial(jpTmp);

            var enGO = new GameObject("EN", typeof(RectTransform));
            enGO.transform.SetParent(go.transform, false);
            var enR = enGO.GetComponent<RectTransform>();
            enR.anchorMin        = new Vector2(0f, 0.5f);
            enR.anchorMax        = new Vector2(1f, 0.5f);
            enR.pivot            = new Vector2(0.5f, 0.5f);
            enR.sizeDelta        = new Vector2(-78f, 28f);
            enR.anchoredPosition = new Vector2(24f, -20f);
            var enTmp = enGO.AddComponent<TextMeshProUGUI>();
            enTmp.text               = en;
            enTmp.fontSize           = 19f;
            enTmp.alignment          = TextAlignmentOptions.MidlineLeft;
            enTmp.enableWordWrapping = false;
            enTmp.overflowMode       = TextOverflowModes.Ellipsis;
            enTmp.characterSpacing   = 1f;
            enTmp.color              = new Color(0.44f, 0.30f, 0.14f);
            enTmp.raycastTarget      = false;
            if (font != null) enTmp.font = font;
            ApplySharpMaterial(enTmp);
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
            numTmp2.fontSize      = 20f;
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
            jpTmp2.fontSize           = 27f;
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
            enTmp2.fontSize           = 19f;
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
            hcJPTmp.fontSize           = 26f;
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
            hcENTmp.fontSize           = 20f;
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
            hcJPTmp.fontSize           = 26f;
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
            hcENTmp.fontSize           = 20f;
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
