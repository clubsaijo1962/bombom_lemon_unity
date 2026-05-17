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

            // カメラを黒背景に設定
            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = Color.black;
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

            // EventSystem（UI操作に必要）
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景（黒・全画面）
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.black;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // LogoGroup（CanvasGroup でフェードを制御）
            var logoGroupGO = new GameObject("LogoGroup", typeof(RectTransform));
            logoGroupGO.transform.SetParent(canvasGO.transform, false);
            var cg = logoGroupGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            var lgRect = logoGroupGO.GetComponent<RectTransform>();
            lgRect.anchorMin = new Vector2(0.5f, 0.5f);
            lgRect.anchorMax = new Vector2(0.5f, 0.5f);
            lgRect.sizeDelta = new Vector2(700f, 700f);
            lgRect.anchoredPosition = Vector2.zero;

            // ロゴ Image
            var logoGO = new GameObject("Logo");
            logoGO.transform.SetParent(logoGroupGO.transform, false);
            var logoImage = logoGO.AddComponent<Image>();
            logoImage.preserveAspect = true;

            // ロゴスプライトを検索（ファイル名が多少違っても対応）
            var sprite = FindLogoSprite();
            if (sprite != null)
            {
                logoImage.sprite = sprite;
                logoImage.SetNativeSize();
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

            // SplashController
            var ctrlGO = new GameObject("SplashController");
            var ctrl = ctrlGO.AddComponent<SplashScreenController>();

            // SerializedObject 経由でプライベートフィールドに代入
            var so = new SerializedObject(ctrl);
            so.FindProperty("logoCanvasGroup").objectReferenceValue = cg;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("fadeInDuration").floatValue = 1.0f;
            so.FindProperty("holdDuration").floatValue = 1.8f;
            so.FindProperty("fadeOutDuration").floatValue = 0.8f;
            so.FindProperty("titleSceneName").stringValue = "Title";
            so.ApplyModifiedProperties();

            // シーンを保存
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Splash.unity");

            // Build Settings に追加
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Splash.unity", 0);

            Debug.Log("[SplashSceneBuilder] Splash シーンを作成しました → Assets/Scenes/Splash.unity");
        }

        static Sprite FindLogoSprite()
        {
            string[] candidates =
            {
                "Assets/Sprites/UI/club_saijo_logo.png",
                "Assets/Sprites/UI/ClubSaijoLogo.png",
                "Assets/Sprites/UI/logo.png",
                "Assets/Sprites/UI/Logo.png",
                "Assets/Sprites/UI/remodori.png",
            };

            foreach (var path in candidates)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
            }

            // キーワード検索
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                if (name.Contains("logo") || name.Contains("saijo") || name.Contains("club") || name.Contains("remodori"))
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            // それでも見つからなければ Assets/Sprites/UI の最初のスプライトを使う
            var fallbackGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/UI" });
            if (fallbackGuids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(fallbackGuids[0]));

            return null;
        }
    }
}
