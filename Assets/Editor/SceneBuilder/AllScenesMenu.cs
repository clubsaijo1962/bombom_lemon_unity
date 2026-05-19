using UnityEditor;
using UnityEngine;

namespace BomBomLemon.Editor.SceneBuilder
{
    /// <summary>
    /// Unity メニュー「BomBom Lemon」からシーンをビルドするエントリポイント
    /// </summary>
    public static class AllScenesMenu
    {
        [MenuItem("BomBom Lemon/シーン作成/01 - Splash シーン作成", priority = 1)]
        public static void BuildSplash()
        {
            SplashSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Splash シーンを作成しました！\nAssets/Scenes/Splash.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/02 - Title シーン作成", priority = 2)]
        public static void BuildTitle()
        {
            TitleSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Title シーンを作成しました！\nAssets/Scenes/Title.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/03 - PlayerSetup シーン作成", priority = 3)]
        public static void BuildPlayerSetup()
        {
            PlayerSetupSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "PlayerSetup シーンを作成しました！\nAssets/Scenes/PlayerSetup.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/04 - Game シーン作成", priority = 4)]
        public static void BuildGame()
        {
            GameSceneBuilder.Build();
            EditorUtility.DisplayDialog("完了", "Game シーンを作成しました！\nAssets/Scenes/Game.unity", "OK");
        }

        [MenuItem("BomBom Lemon/シーン作成/全シーンをまとめて作成", priority = 100)]
        public static void BuildAll()
        {
            SplashSceneBuilder.Build();
            TitleSceneBuilder.Build();
            PlayerSetupSceneBuilder.Build();
            GameSceneBuilder.Build();
            Debug.Log("[AllScenesMenu] 全シーンの作成が完了しました。");
            EditorUtility.DisplayDialog("完了", "全シーンの作成が完了しました！", "OK");
        }
    }
}
