using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BomBomLemon.Multiplayer;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// チームバトル：最終結果画面のシーンビルダー
    /// </summary>
    public static class TeamFinalSceneBuilder
    {
        const string CP = "Assets/Sprites/UI/Casual Game UI Pack - Buttons, Icons & Elements/PNG Files/";
        const int PillL = 66, PillB = 20, PillR = 66, PillT = 8;

        static readonly Color BgColor     = new(0.98f, 0.92f, 0.62f);
        static readonly Color CardColor   = new(1f,    0.99f, 0.95f, 0.95f);
        static readonly Color BtnPrimary  = new(0.97f, 0.82f, 0.10f);
        static readonly Color TextPrimary = new(0.20f, 0.10f, 0.02f);
        static readonly Color TextMuted   = new(0.45f, 0.28f, 0.08f, 0.72f);
        static readonly Color SepColor    = new(0.86f, 0.76f, 0.48f, 0.65f);
        static readonly Color ColTeamA    = new(0.15f, 0.35f, 0.75f);
        static readonly Color ColTeamB    = new(0.75f, 0.20f, 0.15f);
        static readonly Color ColGold     = new(0.85f, 0.55f, 0.00f);
        static readonly Color TeamABg     = new(0.88f, 0.92f, 1.00f, 0.85f);
        static readonly Color TeamBBg     = new(1.00f, 0.88f, 0.88f, 0.85f);

        public static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

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

            // 背景
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.AddComponent<Image>().color = BgColor;
            StretchFull(bgGO.GetComponent<RectTransform>());

            var jpFont    = FindJapaneseTMPFont();
            var btnYellow = LoadSliced(CP + "mini_btn_yellow.png", PillL, PillB, PillR, PillT);
            var uiSprite  = GetBuiltinUISprite();
            var lemonSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Lemon.png")
                            ?? FindSprite("Lemon");
            BuildLemonPattern(canvasGO.transform, lemonSpr, 0.06f);

            // Panel
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelCG = panelGO.AddComponent<CanvasGroup>();
            panelCG.alpha = 0f;
            StretchFull(panelGO.GetComponent<RectTransform>());

            // ── カード: 1020×1440, center y=0 ──
            var cardGO = new GameObject("ContentCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = uiSprite; cardImg.type = Image.Type.Sliced;
            cardImg.color = CardColor; cardImg.raycastTarget = false;
            cardGO.AddComponent<Shadow>().effectColor = new Color(0.22f, 0.14f, 0.02f, 0.18f);
            cardGO.GetComponent<Shadow>().effectDistance = new Vector2(0f, -12f);
            var cardR = cardGO.GetComponent<RectTransform>();
            cardR.anchorMin = cardR.anchorMax = new Vector2(0.5f, 0.5f);
            cardR.pivot     = new Vector2(0.5f, 0.5f);
            cardR.sizeDelta = new Vector2(1020f, 1440f);
            cardR.anchoredPosition = new Vector2(0f, 0f);

            // タイトル
            MakeLabel(cardGO.transform, "Title", "チームバトル 最終結果",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 680f), new Vector2(940f, 64f),
                48f, TextPrimary, FontStyles.Bold, jpFont);

            // 勝者ラベル（大きく）
            var winnerTmp = MakeLabel(cardGO.transform, "WinnerLabel", "🏆 チームA の勝利！",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 580f), new Vector2(940f, 88f),
                64f, ColGold, FontStyles.Bold, jpFont);
            winnerTmp.enableAutoSizing = true;
            winnerTmp.fontSizeMin = 44f; winnerTmp.fontSizeMax = 64f;

            // 勝者詳細
            var winnerDetailTmp = MakeLabel(cardGO.transform, "WinnerDetailLabel",
                "差の合計がより小さかった！",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 490f), new Vector2(880f, 56f),
                36f, TextMuted, FontStyles.Normal, jpFont);

            MakeSeparator(cardGO.transform, 440f);

            // ── チームA エリア（左）──
            var teamABgGO = new GameObject("TeamABg", typeof(RectTransform));
            teamABgGO.transform.SetParent(cardGO.transform, false);
            teamABgGO.AddComponent<Image>().color = TeamABg;
            var taR = teamABgGO.GetComponent<RectTransform>();
            taR.anchorMin = new Vector2(0.02f, 0.5f);
            taR.anchorMax = new Vector2(0.48f, 0.5f);
            taR.pivot     = new Vector2(0.5f, 0.5f);
            taR.sizeDelta = new Vector2(0f, 320f);
            taR.anchoredPosition = new Vector2(0f, 240f);

            var teamAHeaderTmp = MakeLabel(teamABgGO.transform, "TeamAHeader", "チームA",
                new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(380f, 56f),
                44f, ColTeamA, FontStyles.Bold, jpFont);

            var teamAScoreTmp = MakeLabel(teamABgGO.transform, "TeamAScore", "差の合計：0",
                new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(380f, 64f),
                48f, TextPrimary, FontStyles.Bold, jpFont);
            teamAScoreTmp.enableAutoSizing = true;
            teamAScoreTmp.fontSizeMin = 36f; teamAScoreTmp.fontSizeMax = 56f;

            var teamAMembersTmp = MakeLabel(teamABgGO.transform, "TeamAMembers", "---",
                new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(380f, 100f),
                32f, TextPrimary, FontStyles.Normal, jpFont);
            teamAMembersTmp.enableWordWrapping = true;
            teamAMembersTmp.enableAutoSizing   = true;
            teamAMembersTmp.fontSizeMin = 28f; teamAMembersTmp.fontSizeMax = 32f;

            // ── チームB エリア（右）──
            var teamBBgGO = new GameObject("TeamBBg", typeof(RectTransform));
            teamBBgGO.transform.SetParent(cardGO.transform, false);
            teamBBgGO.AddComponent<Image>().color = TeamBBg;
            var tbR2 = teamBBgGO.GetComponent<RectTransform>();
            tbR2.anchorMin = new Vector2(0.52f, 0.5f);
            tbR2.anchorMax = new Vector2(0.98f, 0.5f);
            tbR2.pivot     = new Vector2(0.5f, 0.5f);
            tbR2.sizeDelta = new Vector2(0f, 320f);
            tbR2.anchoredPosition = new Vector2(0f, 240f);

            var teamBHeaderTmp = MakeLabel(teamBBgGO.transform, "TeamBHeader", "チームB",
                new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(380f, 56f),
                44f, ColTeamB, FontStyles.Bold, jpFont);

            var teamBScoreTmp = MakeLabel(teamBBgGO.transform, "TeamBScore", "差の合計：0",
                new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(380f, 64f),
                48f, TextPrimary, FontStyles.Bold, jpFont);
            teamBScoreTmp.enableAutoSizing = true;
            teamBScoreTmp.fontSizeMin = 36f; teamBScoreTmp.fontSizeMax = 56f;

            var teamBMembersTmp = MakeLabel(teamBBgGO.transform, "TeamBMembers", "---",
                new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(380f, 100f),
                32f, TextPrimary, FontStyles.Normal, jpFont);
            teamBMembersTmp.enableWordWrapping = true;
            teamBMembersTmp.enableAutoSizing   = true;
            teamBMembersTmp.fontSizeMin = 28f; teamBMembersTmp.fontSizeMax = 32f;

            MakeSeparator(cardGO.transform, 60f);

            // ── 戻るボタン ──
            var backBtnGO = MakeButton(cardGO.transform, "BackBtn", "ロビーに戻る",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -24f), new Vector2(440f, 80f),
                BtnPrimary, TextPrimary, 40f, jpFont, btnYellow);

            // ── ScreenFade ──
            var sfGO = new GameObject("ScreenFade", typeof(RectTransform));
            sfGO.transform.SetParent(canvasGO.transform, false);
            StretchFull(sfGO.GetComponent<RectTransform>());
            sfGO.AddComponent<Image>().color = Color.black;
            var sfCG = sfGO.AddComponent<CanvasGroup>();
            sfCG.alpha = 1f; sfCG.blocksRaycasts = true;

            // ── TeamFinalController ──
            var ctrlGO = new GameObject("TeamFinalController");
            ctrlGO.transform.SetParent(canvasGO.transform, false);
            var ctrl = ctrlGO.AddComponent<TeamFinalController>();
            var so   = new SerializedObject(ctrl);

            so.FindProperty("winnerLabel").objectReferenceValue       = winnerTmp;
            so.FindProperty("winnerDetailLabel").objectReferenceValue = winnerDetailTmp;
            so.FindProperty("teamAHeaderLabel").objectReferenceValue  = teamAHeaderTmp;
            so.FindProperty("teamAScoreLabel").objectReferenceValue   = teamAScoreTmp;
            so.FindProperty("teamAMembersLabel").objectReferenceValue = teamAMembersTmp;
            so.FindProperty("teamBHeaderLabel").objectReferenceValue  = teamBHeaderTmp;
            so.FindProperty("teamBScoreLabel").objectReferenceValue   = teamBScoreTmp;
            so.FindProperty("teamBMembersLabel").objectReferenceValue = teamBMembersTmp;
            so.FindProperty("backBtn").objectReferenceValue =
                backBtnGO.GetComponent<Button>();
            so.FindProperty("backBtnLabel").objectReferenceValue =
                backBtnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("screenFade").objectReferenceValue = sfCG;
            so.FindProperty("panelGroup").objectReferenceValue = panelCG;
            so.ApplyModifiedProperties();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                "Assets/Scenes/TeamFinal.unity");
            SceneSetupHelper.AddSceneToBuildSettings("Assets/Scenes/TeamFinal.unity");
            Debug.Log("[TeamFinalSceneBuilder] TeamFinal シーンを作成しました");
        }

        // ── ユーティリティ ─────────────────────────────────────────────────────

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
            cols.normalColor      = Color.white;
            cols.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            cols.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
            cols.disabledColor    = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            cols.colorMultiplier  = 1f;
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
            if (ti == null) { Debug.LogWarning($"[TeamFinalBuilder] Not found: {path}"); return null; }
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
