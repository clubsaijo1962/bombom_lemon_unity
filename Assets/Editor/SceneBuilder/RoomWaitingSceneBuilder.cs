using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class RoomWaitingSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;
        const int MaxPlayers = 6;

        static readonly Color BgColor      = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor    = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary   = new(0.97f, 0.82f, 0.10f);
        static readonly Color BtnBack      = new(0.99f, 0.95f, 0.72f);
        static readonly Color TextPrimary  = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted    = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor     = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color SlotBg       = new(0.95f, 0.95f, 0.95f, 0.55f);
        static readonly Color StatusColor  = new(0.45f, 0.28f, 0.08f, 0.60f);
        static readonly Color HellBadgeBg  = new(0.90f, 0.32f, 0.08f, 0.92f);

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = BgColor;
                camera.clearFlags      = CameraClearFlags.SolidColor;
                camera.orthographic    = true;
                camera.allowMSAA       = false;
            }

            var canvasGO = new GameObject("Canvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode    = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera   = camera;
            canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont      = FindJapaneseTMPFont();
            var btnYellow   = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite    = GetBuiltinUISprite();
            var lemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                              ?? FindSprite("Lemon");

            BuildLemonPattern(canvasGO.transform, lemonSprite, 0.07f);

            // Panel (fade)
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── 解散ボタン（左上）──
            var backBtnGO = MakeButton(panelGO.transform, "BackButton", "← 解散",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -104f), new Vector2(220f, 80f),
                BtnBack, TextMuted, 34f, jpFont, btnYellow);

            // ── タイトル（カード上スペース中央）──
            var titleTmp = MakeLabel(panelGO.transform, "Title", "部屋を立てています",
                new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(860f, 80f),
                56f, TextPrimary, FontStyles.Bold, jpFont);

            // ── コンテンツカード ──
            // カード: 1020×1160, center y=-15  上下パディング107px対称
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = CardColor; cardImg.raycastTarget = false;
            var cardSh = cardGO.AddComponent<Shadow>();
            cardSh.effectColor    = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            cardSh.effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot     = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(1020f, 1160f);
            cardR.anchoredPosition = new Vector2(0f, -15f);

            // ── ルーム情報セクション ──
            MakeLabel(cardGO.transform, "PinHeader", "暗証番号",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 453f), new Vector2(600f, 40f),
                34f, TextMuted, FontStyles.Bold, jpFont);

            var pinValueTmp = MakeLabel(cardGO.transform, "PinValue", "------",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 381f), new Vector2(600f, 88f),
                64f, TextPrimary, FontStyles.Bold, jpFont);

            // ゲームモードバッジ（黄色チップ）
            var modeTagGO = new GameObject("ModeTag", typeof(RectTransform));
            modeTagGO.transform.SetParent(cardGO.transform, false);
            var mtr = modeTagGO.GetComponent<RectTransform>();
            mtr.anchorMin = mtr.anchorMax = new Vector2(0.5f, 0.5f);
            mtr.pivot = new Vector2(0.5f, 0.5f);
            mtr.sizeDelta = new Vector2(360f, 60f);
            mtr.anchoredPosition = new Vector2(0f, 283f);
            var modeBgImg = modeTagGO.AddComponent<Image>();
            modeBgImg.sprite = btnYellow; modeBgImg.type = Image.Type.Sliced;
            modeBgImg.color = BtnPrimary; modeBgImg.raycastTarget = false;
            var modeLblGO = new GameObject("Label", typeof(RectTransform));
            modeLblGO.transform.SetParent(modeTagGO.transform, false);
            var modeLblR = modeLblGO.GetComponent<RectTransform>();
            modeLblR.anchorMin = Vector2.zero; modeLblR.anchorMax = Vector2.one;
            modeLblR.offsetMin = modeLblR.offsetMax = Vector2.zero;
            var modeTmp = modeLblGO.AddComponent<TextMeshProUGUI>();
            modeTmp.text = "協力モード"; modeTmp.fontSize = 36f; modeTmp.fontStyle = FontStyles.Bold;
            modeTmp.alignment = TextAlignmentOptions.Center;
            modeTmp.color = TextPrimary; modeTmp.raycastTarget = false;
            if (jpFont != null) modeTmp.font = jpFont;

            // ── 地獄モードバッジ（協力モード × 地獄モード時に RoomWaitingController が SetActive(true) する）──
            // セパレーター(y=225)の下、プレイヤーヘッダーの上に配置。通常時は非表示。
            // バッジ: y=183, 高さ76 → top=221, bottom=145 (separator=225 の下, PlayersHeader top=195 の上で重ならない)
            var hellBadgeGO = new GameObject("HellModeBadge", typeof(RectTransform));
            hellBadgeGO.transform.SetParent(cardGO.transform, false);
            var hellR = hellBadgeGO.GetComponent<RectTransform>();
            hellR.anchorMin = hellR.anchorMax = new Vector2(0.5f, 0.5f);
            hellR.pivot     = new Vector2(0.5f, 0.5f);
            hellR.sizeDelta = new Vector2(880f, 76f);
            hellR.anchoredPosition = new Vector2(0f, 183f);
            var hellBgImg = hellBadgeGO.AddComponent<Image>();
            hellBgImg.sprite = btnYellow; hellBgImg.type = Image.Type.Sliced;
            hellBgImg.color  = HellBadgeBg; hellBgImg.raycastTarget = false;
            var hellLblGO = new GameObject("Label", typeof(RectTransform));
            hellLblGO.transform.SetParent(hellBadgeGO.transform, false);
            var hellLblR = hellLblGO.GetComponent<RectTransform>();
            hellLblR.anchorMin = Vector2.zero; hellLblR.anchorMax = Vector2.one;
            hellLblR.offsetMin = new Vector2(16f, 4f); hellLblR.offsetMax = new Vector2(-16f, -4f);
            var hellTmp = hellLblGO.AddComponent<TextMeshProUGUI>();
            hellTmp.text      = "地獄モード：ライフ半分・ヘルプカード無し";
            hellTmp.fontSize  = 34f; hellTmp.fontStyle = FontStyles.Bold;
            hellTmp.alignment = TextAlignmentOptions.Center;
            hellTmp.color     = Color.white; hellTmp.raycastTarget = false;
            hellTmp.enableWordWrapping = false;
            hellTmp.overflowMode = TextOverflowModes.Overflow;
            if (jpFont != null) hellTmp.font = jpFont;
            hellBadgeGO.SetActive(false);   // 地獄モード時のみ RoomWaitingController が表示する

            // セパレーター
            MakeSeparator(cardGO.transform, 225f);

            // ── プレイヤーセクション ──
            var playersHeaderTmp = MakeLabel(cardGO.transform, "PlayersHeader", "参加プレイヤー",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 169f), new Vector2(860f, 52f),
                36f, TextMuted, FontStyles.Bold, jpFont);

            // ── プレイヤースロット × 6 ──
            // 各スロット: 960×90, 間隔=102px (90+12gap)
            // Y: 82, -20, -122, -224, -326, -428  (上下パディング107px対称)
            var slotGOs      = new GameObject[MaxPlayers];
            var nameLabels   = new TextMeshProUGUI[MaxPlayers];
            var statusLabels = new TextMeshProUGUI[MaxPlayers];
            float[] slotYs   = { 82f, -20f, -122f, -224f, -326f, -428f };

            for (int i = 0; i < MaxPlayers; i++)
            {
                slotGOs[i] = MakePlayerSlot(
                    cardGO.transform, $"PlayerSlot{i + 1}",
                    new Vector2(0f, slotYs[i]), new Vector2(960f, 90f),
                    jpFont, btnYellow,
                    out nameLabels[i], out statusLabels[i]);
            }

            // ── ゲームスタートボタン（カード外・下部）──
            var startBtnGO = MakeButton(panelGO.transform, "StartButton", "ゲームスタート ▶",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -690f), new Vector2(900f, 118f),
                BtnPrimary, TextPrimary, 48f, jpFont, btnYellow);

            // ── コントローラー ──
            var ctrlGO = new GameObject("RoomWaitingController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<RoomWaitingController>();
            var so   = new SerializedObject(ctrl);

            so.FindProperty("titleLabel").objectReferenceValue        = titleTmp;
            so.FindProperty("pinValueLabel").objectReferenceValue     = pinValueTmp;
            so.FindProperty("modeLabel").objectReferenceValue         = modeTmp;
            so.FindProperty("hellModeBadge").objectReferenceValue     = hellBadgeGO;
            so.FindProperty("playersHeaderLabel").objectReferenceValue = playersHeaderTmp;
            so.FindProperty("startBtnLabel").objectReferenceValue     = startBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("backBtnLabel").objectReferenceValue      = backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("startButton").objectReferenceValue       = startBtnGO.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue        = backBtnGO.GetComponent<Button>();
            so.FindProperty("panelGroup").objectReferenceValue        = panelCG;

            // プレイヤースロット配列をワイヤリング
            var slotsProp   = so.FindProperty("playerSlotObjects");
            var namesProp   = so.FindProperty("playerNameLabels");
            var statusProp  = so.FindProperty("playerStatusLabels");
            slotsProp.arraySize  = MaxPlayers;
            namesProp.arraySize  = MaxPlayers;
            statusProp.arraySize = MaxPlayers;
            for (int i = 0; i < MaxPlayers; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue  = slotGOs[i];
                namesProp.GetArrayElementAtIndex(i).objectReferenceValue  = nameLabels[i];
                statusProp.GetArrayElementAtIndex(i).objectReferenceValue = statusLabels[i];
            }

            // ScreenFade
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/RoomWaiting.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/RoomWaiting.unity");
            Debug.Log("[RoomWaitingSceneBuilder] RoomWaiting シーンを作成しました");
        }

        // ── プレイヤースロット ──────────────────────────────────────────

        static GameObject MakePlayerSlot(Transform parent, string name, Vector2 pos, Vector2 size,
            TMP_FontAsset font, Sprite spr,
            out TextMeshProUGUI nameLabel, out TextMeshProUGUI statusLabel)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size; r.anchoredPosition = pos;

            var bg = go.AddComponent<Image>();
            bg.sprite = spr; bg.type = Image.Type.Sliced;
            bg.color = SlotBg; bg.raycastTarget = false;

            // 名前ラベル（左65%）
            var nGO = new GameObject("NameLabel", typeof(RectTransform));
            nGO.transform.SetParent(go.transform, false);
            var nr = nGO.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f); nr.anchorMax = new Vector2(0.65f, 1f);
            nr.offsetMin = new Vector2(28f, 4f); nr.offsetMax = new Vector2(-4f, -4f);
            nameLabel = nGO.AddComponent<TextMeshProUGUI>();
            nameLabel.text = "---"; nameLabel.fontSize = 40f; nameLabel.fontStyle = FontStyles.Bold;
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            nameLabel.color = TextPrimary; nameLabel.raycastTarget = false;
            if (font != null) nameLabel.font = font;

            // ステータスラベル（右35%）
            var sGO = new GameObject("StatusLabel", typeof(RectTransform));
            sGO.transform.SetParent(go.transform, false);
            var sr = sGO.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.65f, 0f); sr.anchorMax = new Vector2(1f, 1f);
            sr.offsetMin = new Vector2(4f, 4f); sr.offsetMax = new Vector2(-28f, -4f);
            statusLabel = sGO.AddComponent<TextMeshProUGUI>();
            statusLabel.text = "待機中"; statusLabel.fontSize = 34f; statusLabel.fontStyle = FontStyles.Normal;
            statusLabel.alignment = TextAlignmentOptions.MidlineRight;
            statusLabel.color = StatusColor; statusLabel.raycastTarget = false;
            if (font != null) statusLabel.font = font;

            return go;
        }

        // ── ユーティリティ ────────────────────────────────────────────

        static void MakeSeparator(Transform parent, float y)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = SepColor;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(880f, 2f);
            r.anchoredPosition = new Vector2(0f, y);
        }

        static void BuildLemonPattern(Transform parent, Sprite lemon, float alpha)
        {
            if (lemon == null) return;
            var p = new GameObject("LemonPattern", typeof(RectTransform));
            p.transform.SetParent(parent, false);
            StretchFull(p.GetComponent<RectTransform>());
            for (int row = 0; row < 10; row++)
            {
                float y = 960f - row * 220f;
                float xs = (row % 2 == 0) ? 0f : 125f;
                for (int col = 0; col < 6; col++)
                {
                    var go = new GameObject($"L{row}_{col}", typeof(RectTransform));
                    go.transform.SetParent(p.transform, false);
                    var r = go.GetComponent<RectTransform>();
                    r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.sizeDelta = new Vector2(110f, 110f);
                    r.anchoredPosition = new Vector2(-625f + col * 250f + xs, y);
                    r.localRotation = Quaternion.Euler(0f, 0f, -22f);
                    var img = go.AddComponent<Image>();
                    img.sprite = lemon; img.preserveAspect = true; img.raycastTarget = false;
                    img.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        static void StretchFull(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pos, Vector2 size,
            float fontSize, Color color, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0.5f, 0.5f);
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
            Color bgColor, Color textColor, float fontSize, TMP_FontAsset font, Sprite btnSprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot;
            r.sizeDelta = size; r.anchoredPosition = pos;
            var bg = go.AddComponent<Image>();
            bg.sprite = btnSprite ?? GetBuiltinUISprite(); bg.type = Image.Type.Sliced; bg.color = bgColor;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white; cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f); cols.colorMultiplier = 1f;
            btn.colors = cols; btn.targetGraphic = bg;
            var tgo = new GameObject("Label", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var tr = tgo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 0f); tr.offsetMax = new Vector2(-8f, 0f);
            var tmp = tgo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false; tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor; tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return go;
        }

        static Sprite GetBuiltinUISprite()
            => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static Sprite LoadSliced(string path, int left, int bottom, int right, int top)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[RoomWaitingBuilder] Not found: {path}"); return null; }
            var border = new Vector4(left, bottom, right, top);
            if (ti.spriteBorder != border || ti.spriteImportMode != SpriteImportMode.Single)
            { ti.spriteImportMode = SpriteImportMode.Single; ti.spriteBorder = border; ti.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite FindSprite(string keyword)
        {
            var guids = AssetDatabase.FindAssets($"t:Sprite {keyword}", new[] { "Assets/Sprites" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (a is Sprite sp) return sp;
            }
            return null;
        }

        static TMP_FontAsset FindJapaneseTMPFont()
        {
            string[] c = { "NotoSansJP", "NotoSans", "Noto", "Meiryo", "YuGothic", "Japanese", "JP" };
            foreach (var kw in c)
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
