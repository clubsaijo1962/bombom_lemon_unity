using UnityEditor;
using UnityEngine;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// Unity メニュー「BomBom Lemon」からシーンをビルドするエントリポイント
    /// </summary>
    public static class AllScenesMenu
    {
        /// <summary>Play モード中は実行不可。ダイアログを出して false を返す。</summary>
        static bool GuardEditMode()
        {
            if (!EditorApplication.isPlaying) return true;
            EditorUtility.DisplayDialog(
                "実行できません",
                "シーン作成は Play モードを停止してから実行してください。\n\n▶ Stop ボタンを押してから再度お試しください。",
                "OK");
            return false;
        }

        [MenuItem("BomBom Lemon/シーン作成/01 - Splash シーン作成", priority = 1)]
        public static void BuildSplash()
        {
            if (!GuardEditMode()) return;
            SplashSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Splash シーンを作成しました！\nAssets/Scenes/Splash.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/02 - Title シーン作成", priority = 2)]
        public static void BuildTitle()
        {
            if (!GuardEditMode()) return;
            TitleSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Title シーンを作成しました！\nAssets/Scenes/Title.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/03 - PlayerSetup シーン作成", priority = 3)]
        public static void BuildPlayerSetup()
        {
            if (!GuardEditMode()) return;
            PlayerSetupSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "PlayerSetup シーンを作成しました！\nAssets/Scenes/PlayerSetup.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/04 - SingleSettings シーン作成", priority = 4)]
        public static void BuildSingleSettings()
        {
            if (!GuardEditMode()) return;
            SingleSettingsSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "SingleSettings シーンを作成しました！\nAssets/Scenes/SingleSettings.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/05 - Game シーン作成", priority = 5)]
        public static void BuildGame()
        {
            if (!GuardEditMode()) return;
            GameSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Game シーンを作成しました！\nAssets/Scenes/Game.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/06 - NumberConfirm シーン作成", priority = 6)]
        public static void BuildNumberConfirm()
        {
            if (!GuardEditMode()) return;
            NumberConfirmSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "NumberConfirm シーンを作成しました！\nAssets/Scenes/NumberConfirm.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/07 - GuessInput シーン作成", priority = 7)]
        public static void BuildGuessInput()
        {
            if (!GuardEditMode()) return;
            GuessInputSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "GuessInput シーンを作成しました！\nAssets/Scenes/GuessInput.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/08 - ResultReveal シーン作成", priority = 8)]
        public static void BuildResultReveal()
        {
            if (!GuardEditMode()) return;
            ResultRevealSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "ResultReveal シーンを作成しました！\nAssets/Scenes/ResultReveal.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/09 - HelpConvert シーン作成", priority = 9)]
        public static void BuildHelpConvert()
        {
            if (!GuardEditMode()) return;
            HelpConvertSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "HelpConvert シーンを作成しました！\nAssets/Scenes/HelpConvert.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/10 - RoomSetup シーン作成", priority = 10)]
        public static void BuildRoomSetup()
        {
            if (!GuardEditMode()) return;
            RoomSetupSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "RoomSetup シーンを作成しました！\nAssets/Scenes/RoomSetup.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/11 - RoomWaiting シーン作成", priority = 11)]
        public static void BuildRoomWaiting()
        {
            if (!GuardEditMode()) return;
            RoomWaitingSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "RoomWaiting シーンを作成しました！\nAssets/Scenes/RoomWaiting.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/12 - RoomJoin シーン作成", priority = 12)]
        public static void BuildRoomJoin()
        {
            if (!GuardEditMode()) return;
            RoomJoinSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "RoomJoin シーンを作成しました！\nAssets/Scenes/RoomJoin.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/全シーンをまとめて作成", priority = 100)]
        public static void BuildAll()
        {
            if (!GuardEditMode()) return;
            SplashSceneBuilder.Build();
            TitleSceneBuilder.Build();
            PlayerSetupSceneBuilder.Build();
            SingleSettingsSceneBuilder.Build();
            GameSceneBuilder.Build();
            NumberConfirmSceneBuilder.Build();
            GuessInputSceneBuilder.Build();
            ResultRevealSceneBuilder.Build();
            HelpConvertSceneBuilder.Build();
            RoomSetupSceneBuilder.Build();
            RoomWaitingSceneBuilder.Build();
            RoomJoinSceneBuilder.Build();
            SetTitleAsPlayModeStartScene();
            Debug.Log("[AllScenesMenu] 全シーンの作成が完了しました。");
            EditorUtility.DisplayDialog("完了", "全シーンの作成が完了しました！\n▶ 再生ボタンは Title シーンから起動します。", "OK");
        }

        /// <summary>▶ 再生ボタンを押したとき Title シーンから起動するよう設定する</summary>
        [MenuItem("BomBom Lemon/▶ 再生開始シーンを Title に設定", priority = 200)]
        public static void SetTitleAsPlayModeStartScene()
        {
            var scene = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>("Assets/Scenes/Title.unity");
            if (scene == null)
            {
                EditorUtility.DisplayDialog("エラー",
                    "Title.unity が見つかりません。\n先に「02 - Title シーン作成」を実行してください。", "OK");
                return;
            }
            UnityEditor.EditorSettings.playModeStartScene = scene;
            Debug.Log("[AllScenesMenu] 再生開始シーンを Title.unity に設定しました");
        }
    }
}
