using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class SingleSettingsSceneBuilder
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

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = new Color(0.98f, 0.90f, 0.55f);
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont = FindJapaneseTMPFont();
            var pill   = GetBuiltinUISprite();

            // Panel CanvasGroup（コンテンツフェード用）
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ────────────── HUD 右上（枠なし・アイコン＋数字のみ）──────────────
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");
            var cardSprite  = FindSprite("card");
            if (cardSprite == null) Debug.LogWarning("[SSBuilder] card sprite not found. Export card.svg as card.png and place it in Assets/Sprites/UI/");
            Debug.Log($"[SSBuilder] lemon={lemonSprite?.name ?? "NULL"}, card={cardSprite?.name ?? "NULL"}");

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(panelGO.transform, false);
            var hudR = hudGO.GetComponent<RectTransform>();
            hudR.anchorMin = new Vector2(1f, 1f);
            hudR.anchorMax = new Vector2(1f, 1f);
            hudR.pivot     = new Vector2(1f, 1f);
            hudR.sizeDelta = new Vector2(400f, 64f);
            hudR.anchoredPosition = new Vector2(-14f, -116f);

            // ── ライフグループ [LemonIcon][×N] ──
            var lifeGrpGO = new GameObject("LifeGroup", typeof(RectTransform));
            lifeGrpGO.transform.SetParent(hudGO.transform, false);
            var lgR = lifeGrpGO.GetComponent<RectTransform>();
            lgR.anchorMin = new Vector2(0f, 0f); lgR.anchorMax = new Vector2(0.46f, 1f);
            lgR.offsetMin = Vector2.zero;         lgR.offsetMax = Vector2.zero;

            // レモンアイコン（固定 56×56、中央アンカー → 歪みなし）
            var lemonHudGO = new GameObject("LemonIcon", typeof(RectTransform));
            lemonHudGO.transform.SetParent(lifeGrpGO.transform, false);
            var lhR = lemonHudGO.GetComponent<RectTransform>();
            lhR.anchorMin = new Vector2(0f, 0.5f); lhR.anchorMax = new Vector2(0f, 0.5f);
            lhR.pivot = new Vector2(0f, 0.5f);
            lhR.sizeDelta = new Vector2(56f, 56f);
            lhR.anchoredPosition = new Vector2(0f, 0f);
            if (lemonSprite != null)
            {
                var li = lemonHudGO.AddComponent<Image>();
                li.sprite = lemonSprite; li.preserveAspect = true; li.raycastTarget = false;
            }

            // ライフ数ラベル
            var lifeLblGO = new GameObject("LifeLabel", typeof(RectTransform));
            lifeLblGO.transform.SetParent(lifeGrpGO.transform, false);
            var llR = lifeLblGO.GetComponent<RectTransform>();
            llR.anchorMin = Vector2.zero; llR.anchorMax = Vector2.one;
            llR.offsetMin = new Vector2(60f, 0f); llR.offsetMax = Vector2.zero;
            var lifeLabel = lifeLblGO.AddComponent<TextMeshProUGUI>();
            lifeLabel.text = "×8"; lifeLabel.fontSize = 40f; lifeLabel.fontStyle = FontStyles.Bold;
            lifeLabel.alignment = TextAlignmentOptions.MidlineLeft;
            lifeLabel.enableWordWrapping = false; lifeLabel.overflowMode = TextOverflowModes.Overflow;
            lifeLabel.color = new Color(0.18f, 0.08f, 0.01f, 1f); lifeLabel.raycastTarget = false;
            if (jpFont != null) lifeLabel.font = jpFont;

            // ── ヘルプグループ [CardIcon][×N] ──
            var helpGrpGO = new GameObject("HelpGroup", typeof(RectTransform));
            helpGrpGO.transform.SetParent(hudGO.transform, false);
            var hgR = helpGrpGO.GetComponent<RectTransform>();
            hgR.anchorMin = new Vector2(0.54f, 0f); hgR.anchorMax = new Vector2(1f, 1f);
            hgR.offsetMin = Vector2.zero;            hgR.offsetMax = Vector2.zero;

            // カードアイコン（固定 56×56）
            var cardHudGO = new GameObject("CardIcon", typeof(RectTransform));
            cardHudGO.transform.SetParent(helpGrpGO.transform, false);
            var chR = cardHudGO.GetComponent<RectTransform>();
            chR.anchorMin = new Vector2(0f, 0.5f); chR.anchorMax = new Vector2(0f, 0.5f);
            chR.pivot = new Vector2(0f, 0.5f);
            chR.sizeDelta = new Vector2(56f, 56f);
            chR.anchoredPosition = new Vector2(0f, 0f);
            if (cardSprite != null)
            {
                var ci = cardHudGO.AddComponent<Image>();
                ci.sprite = cardSprite; ci.preserveAspect = true; ci.raycastTarget = false;
            }

            // ヘルプ数ラベル
            var helpLblGO = new GameObject("HelpLabel", typeof(RectTransform));
            helpLblGO.transform.SetParent(helpGrpGO.transform, false);
            var hlR2 = helpLblGO.GetComponent<RectTransform>();
            hlR2.anchorMin = Vector2.zero; hlR2.anchorMax = Vector2.one;
            hlR2.offsetMin = new Vector2(60f, 0f); hlR2.offsetMax = Vector2.zero;
            var helpLabel = helpLblGO.AddComponent<TextMeshProUGUI>();
            helpLabel.text = "×0"; helpLabel.fontSize = 40f; helpLabel.fontStyle = FontStyles.Bold;
            helpLabel.alignment = TextAlignmentOptions.MidlineLeft;
            helpLabel.enableWordWrapping = false; helpLabel.overflowMode = TextOverflowModes.Overflow;
            helpLabel.color = new Color(0.18f, 0.08f, 0.01f, 1f); helpLabel.raycastTarget = false;
            if (jpFont != null) helpLabel.font = jpFont;

            // ────────────── 戻るボタン（左上）──────────────
            var backBtnGO = MakeButton(panelGO.transform, "BackButton", "← 戻る",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -110f), new Vector2(200f, 60f),
                new Color(0.78f, 0.62f, 0.20f, 0.80f),
                new Color(0.22f, 0.10f, 0.02f, 1f), 28f, jpFont, pill);

            // ────────────── ヘッダー ──────────────
            var headerLabel = MakeLabel(panelGO.transform, "Header", "人数を決めよう",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 650f), new Vector2(900f, 72f),
                52f, new Color(0.26f, 0.10f, 0.01f, 1f), FontStyles.Bold, jpFont);

            // ────────────── 人数コントロール ──────────────
            var countSectionLabel = MakeLabel(panelGO.transform, "CountSectionLabel", "人数",
                new Vector2(0.5f, 0.5f), new Vector2(-295f, 500f), new Vector2(130f, 60f),
                36f, new Color(0.35f, 0.15f, 0.03f, 0.90f), FontStyles.Bold, jpFont);
            countSectionLabel.alignment = TextAlignmentOptions.MidlineRight;

            var decBtnGO = MakeButton(panelGO.transform, "DecreaseBtn", "－",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-130f, 500f), new Vector2(92f, 92f),
                new Color(0.78f, 0.28f, 0.18f, 0.88f), Color.white, 52f, jpFont, pill);

            var countFieldGO = MakeCountInputField(panelGO.transform, "CountField", "2",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 500f), new Vector2(130f, 82f),
                50f, jpFont, pill);

            var incBtnGO = MakeButton(panelGO.transform, "IncreaseBtn", "＋",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(130f, 500f), new Vector2(92f, 92f),
                new Color(0.28f, 0.68f, 0.30f, 0.90f), Color.white, 52f, jpFont, pill);

            var unitLabel = MakeLabel(panelGO.transform, "UnitLabel", "人",
                new Vector2(0.5f, 0.5f), new Vector2(232f, 500f), new Vector2(60f, 60f),
                36f, new Color(0.35f, 0.15f, 0.03f, 0.80f), FontStyles.Normal, jpFont);
            unitLabel.alignment = TextAlignmentOptions.MidlineLeft;

            // ────────────── 情報テキスト（アイコン付き）──────────────
            var infoGroupGO = new GameObject("InfoGroup", typeof(RectTransform));
            infoGroupGO.transform.SetParent(panelGO.transform, false);
            var igR = infoGroupGO.GetComponent<RectTransform>();
            igR.anchorMin = new Vector2(0.5f, 0.5f); igR.anchorMax = new Vector2(0.5f, 0.5f);
            igR.pivot = new Vector2(0.5f, 0.5f);
            igR.sizeDelta = new Vector2(900f, 56f);
            igR.anchoredPosition = new Vector2(0f, 372f);

            // レモンアイコン（固定 48×48・中央アンカー → 歪みなし）
            var liInfoIconGO = new GameObject("LemonIcon", typeof(RectTransform));
            liInfoIconGO.transform.SetParent(infoGroupGO.transform, false);
            var liIR = liInfoIconGO.GetComponent<RectTransform>();
            liIR.anchorMin = new Vector2(0f, 0.5f); liIR.anchorMax = new Vector2(0f, 0.5f);
            liIR.pivot = new Vector2(0f, 0.5f);
            liIR.sizeDelta = new Vector2(48f, 48f);
            liIR.anchoredPosition = new Vector2(80f, 0f);
            if (lemonSprite != null) { var ri = liInfoIconGO.AddComponent<Image>(); ri.sprite = lemonSprite; ri.preserveAspect = true; ri.raycastTarget = false; }
            else { var ti = liInfoIconGO.AddComponent<TextMeshProUGUI>(); ti.text = "♥"; ti.fontSize = 36f; ti.color = new Color(0.88f,0.76f,0.12f,1f); ti.alignment = TextAlignmentOptions.Center; ti.raycastTarget = false; if (jpFont != null) ti.font = jpFont; }

            // ライフテキスト
            var liInfoLabelGO = new GameObject("LifeInfoLabel", typeof(RectTransform));
            liInfoLabelGO.transform.SetParent(infoGroupGO.transform, false);
            var lilR = liInfoLabelGO.GetComponent<RectTransform>();
            lilR.anchorMin = new Vector2(0f, 0f); lilR.anchorMax = new Vector2(0.46f, 1f);
            lilR.offsetMin = new Vector2(136f, 0f); lilR.offsetMax = new Vector2(-4f, 0f);
            var infoLabel = liInfoLabelGO.AddComponent<TextMeshProUGUI>();
            infoLabel.text = "ライフ: 8個"; infoLabel.fontSize = 33f; infoLabel.fontStyle = FontStyles.Bold;
            infoLabel.color = new Color(0.38f, 0.18f, 0.05f, 0.90f);
            infoLabel.alignment = TextAlignmentOptions.MidlineLeft; infoLabel.raycastTarget = false;
            if (jpFont != null) infoLabel.font = jpFont;

            // 区切り
            var sepInfoGO = new GameObject("Sep", typeof(RectTransform));
            sepInfoGO.transform.SetParent(infoGroupGO.transform, false);
            var siR = sepInfoGO.GetComponent<RectTransform>();
            siR.anchorMin = new Vector2(0.46f, 0f); siR.anchorMax = new Vector2(0.54f, 1f);
            siR.offsetMin = Vector2.zero; siR.offsetMax = Vector2.zero;
            var sepTmpInfo = sepInfoGO.AddComponent<TextMeshProUGUI>();
            sepTmpInfo.text = "／"; sepTmpInfo.fontSize = 30f;
            sepTmpInfo.color = new Color(0.38f, 0.18f, 0.05f, 0.45f);
            sepTmpInfo.alignment = TextAlignmentOptions.Center; sepTmpInfo.raycastTarget = false;
            if (jpFont != null) sepTmpInfo.font = jpFont;

            // カードアイコン（固定 48×48・中央アンカー → 歪みなし）
            var hiInfoIconGO = new GameObject("CardIcon", typeof(RectTransform));
            hiInfoIconGO.transform.SetParent(infoGroupGO.transform, false);
            var hiIR = hiInfoIconGO.GetComponent<RectTransform>();
            hiIR.anchorMin = new Vector2(0.54f, 0.5f); hiIR.anchorMax = new Vector2(0.54f, 0.5f);
            hiIR.pivot = new Vector2(0f, 0.5f);
            hiIR.sizeDelta = new Vector2(48f, 48f);
            hiIR.anchoredPosition = new Vector2(6f, 0f);
            if (cardSprite != null) { var ri = hiInfoIconGO.AddComponent<Image>(); ri.sprite = cardSprite; ri.preserveAspect = true; ri.raycastTarget = false; }

            // ヘルプカードテキスト
            var hiInfoLabelGO = new GameObject("HelpInfoLabel", typeof(RectTransform));
            hiInfoLabelGO.transform.SetParent(infoGroupGO.transform, false);
            var hilR = hiInfoLabelGO.GetComponent<RectTransform>();
            hilR.anchorMin = new Vector2(0.54f, 0f); hilR.anchorMax = new Vector2(1f, 1f);
            hilR.offsetMin = new Vector2(62f, 0f); hilR.offsetMax = new Vector2(-8f, 0f);
            var helpInfoLabel = hiInfoLabelGO.AddComponent<TextMeshProUGUI>();
            helpInfoLabel.text = "ヘルプカード: 0枚"; helpInfoLabel.fontSize = 33f; helpInfoLabel.fontStyle = FontStyles.Bold;
            helpInfoLabel.color = new Color(0.38f, 0.18f, 0.05f, 0.90f);
            helpInfoLabel.alignment = TextAlignmentOptions.MidlineLeft; helpInfoLabel.raycastTarget = false;
            if (jpFont != null) helpInfoLabel.font = jpFont;

            // 区切り線
            var divGO = new GameObject("Divider", typeof(RectTransform));
            divGO.transform.SetParent(panelGO.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.60f, 0.40f, 0.12f, 0.25f);
            divImg.raycastTarget = false;
            var divR = divGO.GetComponent<RectTransform>();
            divR.anchorMin = new Vector2(0.5f, 0.5f); divR.anchorMax = new Vector2(0.5f, 0.5f);
            divR.pivot = new Vector2(0.5f, 0.5f);
            divR.sizeDelta = new Vector2(900f, 2f);
            divR.anchoredPosition = new Vector2(0f, 296f);

            // ────────────── プレイヤー名ヘッダー ──────────────
            var playerNamesHeader = MakeLabel(panelGO.transform, "PlayerNamesHeader", "プレイヤー名",
                new Vector2(0.5f, 0.5f), new Vector2(-180f, 226f), new Vector2(520f, 52f),
                38f, new Color(0.35f, 0.15f, 0.03f, 0.90f), FontStyles.Bold, jpFont);
            playerNamesHeader.alignment = TextAlignmentOptions.MidlineLeft;

            // ────────────── スクロールビュー ──────────────
            var scrollGO = new GameObject("NameScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(panelGO.transform, false);
            var scrollR = scrollGO.GetComponent<RectTransform>();
            scrollR.anchorMin = new Vector2(0.5f, 0.5f);
            scrollR.anchorMax = new Vector2(0.5f, 0.5f);
            scrollR.pivot = new Vector2(0.5f, 0.5f);
            scrollR.sizeDelta = new Vector2(980f, 720f);
            scrollR.anchoredPosition = new Vector2(0f, -174f);

            // タッチ判定用（空白エリアでもスクロール受け付ける）
            var scrollBg = scrollGO.AddComponent<Image>();
            scrollBg.color = Color.clear;

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 24f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.decelerationRate = 0.14f;

            var viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            viewportGO.AddComponent<RectMask2D>();
            var vpR = viewportGO.GetComponent<RectTransform>();
            vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one;
            vpR.offsetMin = Vector2.zero; vpR.offsetMax = Vector2.zero;
            scrollRect.viewport = vpR;

            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentR = contentGO.GetComponent<RectTransform>();
            contentR.anchorMin = new Vector2(0f, 1f);
            contentR.anchorMax = new Vector2(1f, 1f);
            contentR.pivot = new Vector2(0.5f, 1f);
            contentR.offsetMin = Vector2.zero;
            contentR.offsetMax = Vector2.zero;
            contentR.sizeDelta = new Vector2(0f, 200f);
            scrollRect.content = contentR;

            // ────────────── スタートボタン ──────────────
            var startBtnGO = MakeButton(panelGO.transform, "StartButton", "ゲームスタート ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -806f), new Vector2(900f, 120f),
                new Color(0.98f, 0.75f, 0.15f, 0.96f),
                new Color(0.18f, 0.07f, 0.01f, 1f), 44f, jpFont, pill);

            // ────────────── SingleSettingsController ──────────────
            var ctrlGO = new GameObject("SingleSettingsController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<SingleSettingsController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("countField").objectReferenceValue        = countFieldGO.GetComponent<TMP_InputField>();
            so.FindProperty("decreaseBtn").objectReferenceValue       = decBtnGO.GetComponent<Button>();
            so.FindProperty("increaseBtn").objectReferenceValue       = incBtnGO.GetComponent<Button>();
            so.FindProperty("lifeCountLabel").objectReferenceValue    = lifeLabel;
            so.FindProperty("helpCardCountLabel").objectReferenceValue = helpLabel;
            so.FindProperty("infoLabel").objectReferenceValue         = infoLabel;
            so.FindProperty("helpInfoLabel").objectReferenceValue    = helpInfoLabel;
            so.FindProperty("listContent").objectReferenceValue       = contentR;
            so.FindProperty("font").objectReferenceValue              = jpFont;
            so.FindProperty("startButton").objectReferenceValue       = startBtnGO.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue        = backBtnGO.GetComponent<Button>();
            so.FindProperty("headerLabel").objectReferenceValue       = headerLabel;
            so.FindProperty("startBtnLabel").objectReferenceValue     = startBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("backBtnLabel").objectReferenceValue      = backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("playerNamesHeader").objectReferenceValue = playerNamesHeader;
            so.FindProperty("countSectionLabel").objectReferenceValue = countSectionLabel;
            so.FindProperty("panelGroup").objectReferenceValue        = panelCG;
            so.FindProperty("gameSceneName").stringValue              = "Game";
            so.FindProperty("backSceneName").stringValue              = "PlayerSetup";

            // ScreenFade（最前面）
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SingleSettings.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/SingleSettings.unity", 3);

            Debug.Log("[SingleSettingsSceneBuilder] SingleSettings シーンを作成しました → Assets/Scenes/SingleSettings.unity");
        }

        // ─────────────────────────────────────────────────────────────────
        // ヘルパー
        // ─────────────────────────────────────────────────────────────────

        static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static TextMeshProUGUI MakeFillLabel(Transform parent, string name, string text,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(4f, 0f); r.offsetMax = new Vector2(-4f, 0f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
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
            Color bgColor, Color textColor, float fontSize, TMP_FontAsset font, Sprite pill)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;

            var shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(go.transform, false);
            var sr = shadowGO.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(2f, -7f); sr.offsetMax = new Vector2(-2f, -1f);
            var si = shadowGO.AddComponent<Image>();
            if (pill != null) { si.sprite = pill; si.type = Image.Type.Sliced; }
            si.color = new Color(0.40f, 0.22f, 0.04f, 0.18f);
            si.raycastTarget = false;

            var bg = go.AddComponent<Image>();
            if (pill != null) { bg.sprite = pill; bg.type = Image.Type.Sliced; }
            bg.color = bgColor;

            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.90f, 1f);
            cols.pressedColor = new Color(0.80f, 0.76f, 0.62f, 1f);
            cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;

            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(12f, 0f); tr.offsetMax = new Vector2(-12f, 0f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return go;
        }

        static GameObject MakeCountInputField(Transform parent, string name, string defaultText,
            Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, TMP_FontAsset font, Sprite pill)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            if (pill != null) { bg.sprite = pill; bg.type = Image.Type.Sliced; }
            bg.color = new Color(1f, 0.97f, 0.88f, 0.95f);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic = bg;
            inputField.characterLimit = 2;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            var taGO = new GameObject("Text Area", typeof(RectTransform));
            taGO.transform.SetParent(go.transform, false);
            taGO.AddComponent<RectMask2D>();
            var taR = taGO.GetComponent<RectTransform>();
            taR.anchorMin = Vector2.zero; taR.anchorMax = Vector2.one;
            taR.offsetMin = new Vector2(6f, 4f); taR.offsetMax = new Vector2(-6f, -4f);

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(taGO.transform, false);
            var txtR = txtGO.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero; txtR.anchorMax = Vector2.one;
            txtR.offsetMin = Vector2.zero; txtR.offsetMax = Vector2.zero;
            var txtTmp = txtGO.AddComponent<TextMeshProUGUI>();
            txtTmp.text = defaultText; txtTmp.fontSize = fontSize;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.color = new Color(0.18f, 0.08f, 0.01f, 1f);
            txtTmp.alignment = TextAlignmentOptions.Center;
            if (font != null) txtTmp.font = font;

            inputField.textViewport  = taR;
            inputField.textComponent = txtTmp;
            inputField.text = defaultText;
            return go;
        }

        static Sprite FindSprite(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Sprite {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
                if (s != null) return s;
            }
            var allGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains(keyword.ToLower()))
                {
                    var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null) return s;
                }
            }
            return null;
        }

        static Texture2D FindTexture(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Texture2D {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                if (tex != null) return tex;
            }
            var allGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains(keyword.ToLower()))
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
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
            return null;
        }
    }
}
