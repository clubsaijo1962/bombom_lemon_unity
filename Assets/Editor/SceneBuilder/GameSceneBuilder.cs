using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Game;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class GameSceneBuilder
    {
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.08f, 0.08f, 0.15f);
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

            // --- Phase / Round labels ---
            var phaseLabel = CreateLabel(canvasGO.transform, "PhaseLabel",
                "フェーズ", new Vector2(0.5f, 0.92f), new Vector2(600f, 60f), 36);

            var roundLabel = CreateLabel(canvasGO.transform, "RoundLabel",
                "ラウンド 1", new Vector2(0.5f, 0.86f), new Vector2(400f, 50f), 28);

            // --- Topic area ---
            var topicText = CreateLabel(canvasGO.transform, "TopicText",
                "お題テキスト", new Vector2(0.5f, 0.75f), new Vector2(800f, 70f), 42);

            var topicLowLabel = CreateLabel(canvasGO.transform, "TopicLowLabel",
                "1 = 低い側", new Vector2(0.15f, 0.68f), new Vector2(350f, 50f), 28);

            var topicHighLabel = CreateLabel(canvasGO.transform, "TopicHighLabel",
                "99 = 高い側", new Vector2(0.85f, 0.68f), new Vector2(350f, 50f), 28);

            // --- My Card Panel ---
            var myCardPanelGO = new GameObject("MyCardPanel");
            myCardPanelGO.transform.SetParent(canvasGO.transform, false);
            myCardPanelGO.SetActive(false);
            var myCardPanelRect = myCardPanelGO.GetComponent<RectTransform>();
            myCardPanelRect.anchorMin = new Vector2(0.5f, 0.52f);
            myCardPanelRect.anchorMax = new Vector2(0.5f, 0.52f);
            myCardPanelRect.sizeDelta = new Vector2(300f, 300f);
            myCardPanelRect.anchoredPosition = Vector2.zero;

            var cardBg = myCardPanelGO.AddComponent<Image>();
            cardBg.color = new Color(0.9f, 0.85f, 0.6f);

            var myCardNumberText = CreateLabel(myCardPanelGO.transform, "MyCardNumberText",
                "?", new Vector2(0.5f, 0.5f), new Vector2(260f, 200f), 120);

            // --- Guess Panel ---
            var guessPanelGO = new GameObject("GuessPanel");
            guessPanelGO.transform.SetParent(canvasGO.transform, false);
            guessPanelGO.SetActive(false);

            var guessPanelBg = guessPanelGO.AddComponent<Image>();
            guessPanelBg.color = new Color(0.15f, 0.12f, 0.3f, 0.95f);
            var guessPanelRect = guessPanelGO.GetComponent<RectTransform>();
            guessPanelRect.anchorMin = new Vector2(0.1f, 0.25f);
            guessPanelRect.anchorMax = new Vector2(0.9f, 0.65f);
            guessPanelRect.offsetMin = Vector2.zero;
            guessPanelRect.offsetMax = Vector2.zero;

            var guessingTargetLabel = CreateLabel(guessPanelGO.transform, "GuessingTargetLabel",
                "プレイヤーの数字は？", new Vector2(0.5f, 0.82f), new Vector2(700f, 60f), 32);

            var guessValueText = CreateLabel(guessPanelGO.transform, "GuessValueText",
                "50", new Vector2(0.5f, 0.6f), new Vector2(200f, 60f), 48);

            // Slider
            var sliderGO = new GameObject("GuessSlider");
            sliderGO.transform.SetParent(guessPanelGO.transform, false);
            var sliderRect = sliderGO.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.35f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.35f);
            sliderRect.sizeDelta = new Vector2(700f, 60f);
            sliderRect.anchoredPosition = Vector2.zero;
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 1;
            slider.maxValue = 99;
            slider.wholeNumbers = true;
            slider.value = 50;

            var submitBtn = CreateButtonSimple(guessPanelGO.transform, "SubmitGuessButton",
                "決定！", new Vector2(0.5f, 0.1f), new Vector2(400f, 80f));

            // --- Result Panel ---
            var resultPanelGO = new GameObject("ResultPanel");
            resultPanelGO.transform.SetParent(canvasGO.transform, false);
            resultPanelGO.SetActive(false);

            var resultPanelBg = resultPanelGO.AddComponent<Image>();
            resultPanelBg.color = new Color(0.05f, 0.1f, 0.2f, 0.97f);
            var resultPanelRect = resultPanelGO.GetComponent<RectTransform>();
            resultPanelRect.anchorMin = new Vector2(0.05f, 0.1f);
            resultPanelRect.anchorMax = new Vector2(0.95f, 0.9f);
            resultPanelRect.offsetMin = Vector2.zero;
            resultPanelRect.offsetMax = Vector2.zero;

            var resultText = CreateLabel(resultPanelGO.transform, "ResultText",
                "結果", new Vector2(0.5f, 0.6f), new Vector2(800f, 500f), 32);
            resultText.alignment = TextAlignmentOptions.TopLeft;

            var nextRoundBtn = CreateButtonSimple(resultPanelGO.transform, "NextRoundButton",
                "次のラウンドへ", new Vector2(0.5f, 0.1f), new Vector2(500f, 80f));

            // --- ITOGameManager ---
            var gmGO = new GameObject("ITOGameManager");
            gmGO.AddComponent<ITOGameManager>();

            // --- GameSceneUI ---
            var uiGO = new GameObject("GameSceneUI");
            var ui = uiGO.AddComponent<GameSceneUI>();

            var so = new SerializedObject(ui);
            so.FindProperty("topicText").objectReferenceValue = topicText;
            so.FindProperty("topicLowLabel").objectReferenceValue = topicLowLabel;
            so.FindProperty("topicHighLabel").objectReferenceValue = topicHighLabel;
            so.FindProperty("myCardNumberText").objectReferenceValue = myCardNumberText;
            so.FindProperty("myCardPanel").objectReferenceValue = myCardPanelGO;
            so.FindProperty("guessPanel").objectReferenceValue = guessPanelGO;
            so.FindProperty("guessingTargetLabel").objectReferenceValue = guessingTargetLabel;
            so.FindProperty("guessSlider").objectReferenceValue = slider;
            so.FindProperty("guessValueText").objectReferenceValue = guessValueText;
            so.FindProperty("submitGuessButton").objectReferenceValue = submitBtn;
            so.FindProperty("resultPanel").objectReferenceValue = resultPanelGO;
            so.FindProperty("resultText").objectReferenceValue = resultText;
            so.FindProperty("nextRoundButton").objectReferenceValue = nextRoundBtn;
            so.FindProperty("phaseLabel").objectReferenceValue = phaseLabel;
            so.FindProperty("roundLabel").objectReferenceValue = roundLabel;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/Game.unity");

            Debug.Log("[GameSceneBuilder] Game シーンを作成しました → Assets/Scenes/Game.unity");
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

        static Button CreateButtonSimple(Transform parent, string name, string label,
            Vector2 anchorCenter, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.5f, 0.8f);

            var btn = go.AddComponent<Button>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }
    }
}
