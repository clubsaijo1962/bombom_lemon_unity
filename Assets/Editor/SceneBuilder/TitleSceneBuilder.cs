using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
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
                camera.backgroundColor = new Color(0.99f, 0.96f, 0.82f);
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

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.99f, 0.96f, 0.82f);
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

            // タイトルロゴ（3枚重ね）
            var bubbleTex = FindTexture("Title_Bubble");
            var lemonTex  = FindTexture("Title_Lemon");
            var wordTex   = FindTexture("Title_Word");

            var logoGroupGO = new GameObject("TitleLogo", typeof(RectTransform));
            logoGroupGO.transform.SetParent(titleGroupGO.transform, false);
            var logoGroupRect = logoGroupGO.GetComponent<RectTransform>();
            logoGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoGroupRect.pivot = new Vector2(0.5f, 0.5f);
            logoGroupRect.sizeDelta = new Vector2(1200f, 1200f);
            logoGroupRect.anchoredPosition = new Vector2(0f, 220f);

            // Bubble: 画面幅に収まる（1080px基準で左右ほぼいっぱい）
            var bubbleRect = AddRawImageLayerSizedByWidth(logoGroupGO.transform, "Layer_Bubble", bubbleTex, 1040f, new Vector2(0f, 20f));

            // Lemon: 高さ520px・泡の中心に配置
            var lemonRect = AddRawImageLayerSized(logoGroupGO.transform, "Layer_Lemon", lemonTex, 520f, new Vector2(0f, 70f));

            // Word: 幅960px・左右40px余白で収まるサイズ
            var wordRect = AddRawImageLayerSizedByWidth(logoGroupGO.transform, "Layer_Word", wordTex, 960f, new Vector2(0f, -250f));

            // STARTボタン
            var startBtnGO = new GameObject("StartButton", typeof(RectTransform));
            startBtnGO.transform.SetParent(titleGroupGO.transform, false);
            var startRect = startBtnGO.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.5f, 0.5f);
            startRect.anchorMax = new Vector2(0.5f, 0.5f);
            startRect.pivot = new Vector2(0.5f, 0.5f);
            // 画面下から20%の位置: 中央(0) から -1920*0.30 = -576
            startRect.anchoredPosition = new Vector2(0f, -576f);

            // Button の当たり判定用 Image（透明）
            var hitImg = startBtnGO.AddComponent<Image>();
            hitImg.color = Color.clear;
            var playBtn = startBtnGO.AddComponent<Button>();
            var btnColors = playBtn.colors;
            btnColors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            btnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            playBtn.colors = btnColors;

            // START画像をRawImageで表示
            var startTex = FindTexture("start");
            if (startTex != null)
            {
                var startImgGO = new GameObject("StartImage", typeof(RectTransform));
                startImgGO.transform.SetParent(startBtnGO.transform, false);
                var rawImg = startImgGO.AddComponent<RawImage>();
                rawImg.texture = startTex;
                rawImg.raycastTarget = false;
                float ratio = (float)startTex.width / startTex.height;
                float h = 240f;
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
            so.FindProperty("modeSelectPanel").objectReferenceValue = titleGroupGO; // 暫定
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("titleLogoRect").objectReferenceValue = logoGroupRect;
            so.FindProperty("bgmSource").objectReferenceValue = bgmSrc;
            so.FindProperty("playerSetupSceneName").stringValue = "PlayerSetup";
            so.ApplyModifiedProperties();

            // TitleLogoAnimator（各レイヤー独立アニメーション）
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

        // フルストレッチ（Bubble背景用）
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

        // 高さ上限指定・アスペクト比保持（Lemon用）
        static RectTransform AddRawImageLayerSized(Transform parent, string name, Texture2D tex, float maxHeight, Vector2 pos)
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
                // Mathf.Minは使わない: テクスチャが小さくてもmaxHeightまで拡大する
                float ratio = (float)tex.width / tex.height;
                rect.sizeDelta = new Vector2(maxHeight * ratio, maxHeight);
                var raw = go.AddComponent<RawImage>();
                raw.texture = tex;
                raw.raycastTarget = false;
            }
            else
            {
                rect.sizeDelta = new Vector2(maxHeight, maxHeight);
            }
            return rect;
        }

        // 幅指定・アスペクト比保持（Word/Bubble用）
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
                // Mathf.Minは使わない: テクスチャが小さくてもtargetWidthまで拡大する
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
            // フォールバック：全体検索
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
