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
            var lemonTex = FindTexture("Title_Lemon");
            var rainGO = new GameObject("LemonRain", typeof(RectTransform));
            rainGO.transform.SetParent(titleGroupGO.transform, false);
            var rainRect = rainGO.GetComponent<RectTransform>();
            rainRect.anchorMin = Vector2.zero;
            rainRect.anchorMax = Vector2.one;
            rainRect.offsetMin = Vector2.zero;
            rainRect.offsetMax = Vector2.zero;
            var rain = rainGO.AddComponent<LemonRainEffect>();
            var rainSO = new SerializedObject(rain);
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
            if (startTex != null)
            {
                var startImgGO = new GameObject("StartImage", typeof(RectTransform));
                startImgGO.transform.SetParent(startBtnGO.transform, false);
                var rawImg = startImgGO.AddComponent<RawImage>();
                rawImg.texture = startTex;
                rawImg.raycastTarget = false;
                float ratio = (float)startTex.width / startTex.height;
                float h = 276f;
                var imgRect = startImgGO.GetComponent<RectTransform>();
                imgRect.anchorMin = new Vector2(0.5f, 0.5f);
                imgRect.anchorMax = new Vector2(0.5f, 0.5f);
                imgRect.pivot = new Vector2(0.5f, 0.5f);
                imgRect.sizeDelta = new Vector2(h * ratio, h);
                imgRect.anchoredPosition = Vector2.zero;
                startRect.sizeDelta = new Vector2(h * ratio, h);
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
                new Vector2(0.5f, 0.5f), new Vector2(820f, 58f), 34);
            subJP.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -740f);
            subJP.color = new Color(0.22f, 0.12f, 0.02f);
            if (jpFont != null) subJP.font = jpFont;

            var subEN = CreateLabel(titleGroupGO.transform, "SubtitleEN",
                "Party game for 2 to 24 players", new Vector2(0.5f, 0.5f), new Vector2(820f, 42f), 23);
            subEN.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -802f);
            subEN.color = new Color(0.30f, 0.20f, 0.05f, 0.88f);
            if (jpFont != null) subEN.font = jpFont;

            // ─── 上部ボタンバー ───
            // 左上: ルール・お題ボタン
            var rulesBtn   = CreateTopBarButton(titleGroupGO.transform, "RulesButton",   "ルール",   new Vector2(0f, 1f), new Vector2( 75f, -191f), new Vector2(148f, 52f), jpFont);
            var topicsBtn  = CreateTopBarButton(titleGroupGO.transform, "TopicsButton",  "お題",     new Vector2(0f, 1f), new Vector2(243f, -191f), new Vector2(120f, 52f), jpFont);
            // 右上: 地獄モードトグル
            var hellBtnGO  = CreateTopBarButtonGO(titleGroupGO.transform, "HellModeButton", "地獄モード", new Vector2(1f, 1f), new Vector2(-90f, -191f), new Vector2(178f, 52f), jpFont);
            var hellIndImg = hellBtnGO.transform.Find("Indicator")?.GetComponent<Image>();

            // TitleTopBarController
            var topBarGO = new GameObject("TitleTopBarController");
            var topBar = topBarGO.AddComponent<TitleTopBarController>();
            var topBarSO = new SerializedObject(topBar);
            topBarSO.FindProperty("rulesButton").objectReferenceValue  = rulesBtn;
            topBarSO.FindProperty("topicsButton").objectReferenceValue = topicsBtn;
            topBarSO.FindProperty("hellModeButton").objectReferenceValue = hellBtnGO.GetComponent<Button>();
            if (hellIndImg != null)
                topBarSO.FindProperty("hellModeIndicator").objectReferenceValue = hellIndImg;
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
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return tmp;
        }

        // 上部バー: Buttonを返す
        static Button CreateTopBarButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pos, Vector2 size, TMP_FontAsset font)
        {
            var go = CreateTopBarButtonGO(parent, name, label, anchor, pos, size, font);
            return go.GetComponent<Button>();
        }

        // 上部バー: GameObjectを返す（地獄モード用インジケーター付き）
        static GameObject CreateTopBarButtonGO(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pos, Vector2 size, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot     = new Vector2(anchor.x, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            // 白い角丸風の背景
            var bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.88f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 0.85f);
            colors.pressedColor     = new Color(0.85f, 0.85f, 0.85f);
            btn.colors = colors;

            // テキスト
            var textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 26;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color     = new Color(0.15f, 0.10f, 0.05f);
            if (font != null) tmp.font = font;

            // 地獄モードボタンのみインジケータードット
            if (name == "HellModeButton")
            {
                tmp.text      = "地獄モード";
                tmp.alignment = TextAlignmentOptions.MidlineRight;
                if (font != null) tmp.font = font;

                var dotGO = new GameObject("Indicator", typeof(RectTransform));
                dotGO.transform.SetParent(go.transform, false);
                var dotRect = dotGO.GetComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0f, 0.5f);
                dotRect.anchorMax = new Vector2(0f, 0.5f);
                dotRect.pivot     = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(22f, 22f);
                dotRect.anchoredPosition = new Vector2(22f, 0f);
                var dot = dotGO.AddComponent<Image>();
                dot.color = new Color(0.55f, 0.55f, 0.55f); // OFF = グレー
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
