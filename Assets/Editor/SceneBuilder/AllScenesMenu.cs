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

        // 今後ここに追加していく
        // [MenuItem("BomBom Lemon/シーン作成/02 - Title シーン作成", priority = 2)]
        // [MenuItem("BomBom Lemon/シーン作成/03 - Game シーン作成", priority = 3)]

        [MenuItem("BomBom Lemon/シーン作成/全シーンをまとめて作成", priority = 100)]
        public static void BuildAll()
        {
            SplashSceneBuilder.Build();
            // TitleSceneBuilder.Build();
            // GameSceneBuilder.Build();
            Debug.Log("[AllScenesMenu] 全シーンの作成が完了しました。");
            EditorUtility.DisplayDialog("完了", "全シーンの作成が完了しました！", "OK");
        }
    }
}
