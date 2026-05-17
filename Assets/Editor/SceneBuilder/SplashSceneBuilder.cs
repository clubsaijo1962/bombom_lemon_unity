using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using BomBomLemon.Splash;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class SplashSceneBuilder
    {
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.04f, 0.04f, 0.18f);
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

            // 背景：深い夜空色
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.04f, 0.04f, 0.18f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 下グラデーション風オーバーレイ（紫みを足す）
            var overlayGO = new GameObject("BgOverlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.12f, 0.02f, 0.18f, 0.55f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = new Vector2(0f, 0f);
            overlayRect.anchorMax = new Vector2(1f, 0.5f);
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // LogoGroup（フェード用 CanvasGroup）
            var logoGroupGO = new GameObject("LogoGroup", typeof(RectTransform));
            logoGroupGO.transform.SetParent(canvasGO.transform, false);
            var cg = logoGroupGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            var lgRect = logoGroupGO.GetComponent<RectTransform>();
            lgRect.anchorMin = new Vector2(0.5f, 0.5f);
            lgRect.anchorMax = new Vector2(0.5f, 0.5f);
            lgRect.sizeDelta = new Vector2(750f, 750f);
            lgRect.anchoredPosition = new Vector2(0f, 40f);

            // グロー（ロゴの後ろで光る）
            var glowGO = new GameObject("LogoGlow", typeof(RectTransform));
            glowGO.transform.SetParent(logoGroupGO.transform, false);
            var glowImage = glowGO.AddComponent<Image>();
            glowImage.color = new Color(1f, 0.92f, 0.55f, 0f);
            var glowRect = glowGO.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(860f, 860f);
            glowRect.anchoredPosition = Vector2.zero;

            // ロゴ Image（グローの後に追加してグローより前面に来る）
            var logoGO = new GameObject("Logo");
            logoGO.transform.SetParent(logoGroupGO.transform, false);
            var logoImage = logoGO.AddComponent<Image>();
            logoImage.preserveAspect = true;

            var sprite = FindLogoSprite();
            if (sprite != null)
            {
                logoImage.sprite = sprite;
                Debug.Log($"[SplashSceneBuilder] ロゴ読み込み成功: {AssetDatabase.GetAssetPath(sprite)}");
            }
            else
            {
                logoImage.color = new Color(1f, 0.9f, 0.4f);
                Debug.LogWarning("[SplashSceneBuilder] ロゴ画像が見つかりません。Assets/Sprites/UI/ に PNG を配置して再実行してください。");
            }

            var logoRect = logoGO.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.sizeDelta = new Vector2(700f, 700f);
            logoRect.anchoredPosition = Vector2.zero;

            // SplashController + AudioSource
            var ctrlGO = new GameObject("SplashController");
            var ctrl = ctrlGO.AddComponent<SplashScreenController>();
            var audioSrc = ctrlGO.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.volume = 1f;

            var clip = FindAudioClip();
            if (clip != null)
            {
                audioSrc.clip = clip;
                Debug.Log($"[SplashSceneBuilder] 音源読み込み成功: {AssetDatabase.GetAssetPath(clip)}");
            }
            else
            {
                Debug.LogWarning("[SplashSceneBuilder] 音源が見つかりません。Assets/Audio/piyo.mp3 を配置して再実行してください。");
            }

            var so = new SerializedObject(ctrl);
            so.FindProperty("logoCanvasGroup").objectReferenceValue = cg;
            so.FindProperty("logoRect").objectReferenceValue = logoRect;
            so.FindProperty("glowImage").objectReferenceValue = glowImage;
            so.FindProperty("audioSource").objectReferenceValue = audioSrc;
            so.FindProperty("audioDuration").floatValue = 2.5f;
            so.FindProperty("audioFadeOutTime").floatValue = 0.4f;
            so.FindProperty("fadeInDuration").floatValue = 1.0f;
            so.FindProperty("holdDuration").floatValue = 2.2f;
            so.FindProperty("fadeOutDuration").floatValue = 0.7f;
            so.FindProperty("bounceDelay").floatValue = 0.5f;
            so.FindProperty("titleSceneName").stringValue = "Title";
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Splash.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Splash.unity", 0);

            Debug.Log("[SplashSceneBuilder] Splash シーンを作成しました → Assets/Scenes/Splash.unity");
        }

        static Sprite FindLogoSprite()
        {
            string[] candidates =
            {
                "Assets/Sprites/UI/remodori.png",
                "Assets/Sprites/UI/club_saijo_logo.png",
                "Assets/Sprites/UI/ClubSaijoLogo.png",
                "Assets/Sprites/UI/logo.png",
                "Assets/Sprites/UI/Logo.png",
            };

            foreach (var path in candidates)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
            }

            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                if (name.Contains("logo") || name.Contains("saijo") || name.Contains("club") || name.Contains("remodori"))
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            var fallbackGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/UI" });
            if (fallbackGuids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(fallbackGuids[0]));

            return null;
        }

        static AudioClip FindAudioClip()
        {
            string[] candidates =
            {
                "Assets/Audio/piyo.mp3",
                "Assets/Audio/piyo.wav",
                "Assets/Audio/piyo.ogg",
            };

            foreach (var path in candidates)
            {
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c != null) return c;
            }

            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                if (name.Contains("piyo") || name.Contains("bird") || name.Contains("tori"))
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }

            return null;
        }
    }
}
