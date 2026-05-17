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

            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.1f, 0.05f, 0.2f);
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

            // TitleGroup（フェードイン用 CanvasGroup）
            var titleGroupGO = new GameObject("TitleGroup");
            titleGroupGO.transform.SetParent(canvasGO.transform, false);
            var titleCG = titleGroupGO.AddComponent<CanvasGroup>();
            titleCG.alpha = 0f;
            SetFullStretch(titleGroupGO.GetComponent<RectTransform>());

            // ロゴ
            var logoGO = new GameObject("TitleLogo");
            logoGO.transform.SetParent(titleGroupGO.transform, false);
            var logoImage = logoGO.AddComponent<Image>();
            logoImage.color = new Color(1f, 0.9f, 0.3f);
            var logoRect = logoGO.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.7f);
            logoRect.anchorMax = new Vector2(0.5f, 0.7f);
            logoRect.sizeDelta = new Vector2(700f, 200f);
            logoRect.anchoredPosition = Vector2.zero;

            // MainPanel
            var mainPanelGO = new GameObject("MainPanel");
            mainPanelGO.transform.SetParent(titleGroupGO.transform, false);
            SetFullStretch(mainPanelGO.GetComponent<RectTransform>());

            // Play Button
            var playBtn = CreateButton(mainPanelGO.transform, "PlayButton", "プレイ",
                new Vector2(0.5f, 0.45f), new Vector2(400f, 80f));

            // Credits Button
            var creditsBtn = CreateButton(mainPanelGO.transform, "CreditsButton", "クレジット",
                new Vector2(0.5f, 0.32f), new Vector2(400f, 80f));

            // ModeSelectPanel
            var modePanelGO = new GameObject("ModeSelectPanel");
            modePanelGO.transform.SetParent(titleGroupGO.transform, false);
            modePanelGO.SetActive(false);
            SetFullStretch(modePanelGO.GetComponent<RectTransform>());

            var localBtn = CreateButton(modePanelGO.transform, "LocalModeButton", "ローカル対戦",
                new Vector2(0.5f, 0.55f), new Vector2(400f, 80f));

            var onlineBtn = CreateButton(modePanelGO.transform, "OnlineModeButton", "オンライン対戦",
                new Vector2(0.5f, 0.42f), new Vector2(400f, 80f));

            var backBtn = CreateButton(modePanelGO.transform, "BackButton", "もどる",
                new Vector2(0.5f, 0.29f), new Vector2(400f, 80f));

            // TitleScreenController
            var ctrlGO = new GameObject("TitleScreenController");
            var ctrl = ctrlGO.AddComponent<TitleScreenController>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("titleGroup").objectReferenceValue = titleCG;
            so.FindProperty("mainPanel").objectReferenceValue = mainPanelGO;
            so.FindProperty("modeSelectPanel").objectReferenceValue = modePanelGO;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("creditsButton").objectReferenceValue = creditsBtn;
            so.FindProperty("localModeButton").objectReferenceValue = localBtn;
            so.FindProperty("onlineModeButton").objectReferenceValue = onlineBtn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("titleLogoRect").objectReferenceValue = logoRect;
            so.FindProperty("playerSetupSceneName").stringValue = "PlayerSetup";
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Title.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Title.unity", 1);

            Debug.Log("[TitleSceneBuilder] Title シーンを作成しました → Assets/Scenes/Title.unity");
        }

        static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchorCenter, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.15f, 0.4f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.25f, 0.6f);
            colors.pressedColor = new Color(0.1f, 0.08f, 0.25f);
            btn.colors = colors;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 40;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
