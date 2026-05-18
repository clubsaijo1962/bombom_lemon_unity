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
            topBarSO.ApplyModifiedProperties();

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
    }
}
