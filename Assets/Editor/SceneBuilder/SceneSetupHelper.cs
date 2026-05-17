using UnityEditor;

namespace BomBomLemon.Editor.SceneBuilder
{
    public static class SceneSetupHelper
    {
        /// <summary>
        /// シーンを Build Settings の指定インデックスに追加する（重複チェックあり）
        /// </summary>
        public static void AddSceneToBuildSettings(string scenePath, int insertIndex = -1)
        {
            var current = EditorBuildSettings.scenes;

            foreach (var s in current)
                if (s.path == scenePath) return;

            var next = new EditorBuildSettingsScene[current.Length + 1];

            if (insertIndex < 0 || insertIndex >= current.Length)
            {
                // 末尾に追加
                for (int i = 0; i < current.Length; i++) next[i] = current[i];
                next[current.Length] = new EditorBuildSettingsScene(scenePath, true);
            }
            else
            {
                // 指定位置に挿入
                for (int i = 0; i < insertIndex; i++) next[i] = current[i];
                next[insertIndex] = new EditorBuildSettingsScene(scenePath, true);
                for (int i = insertIndex; i < current.Length; i++) next[i + 1] = current[i];
            }

            EditorBuildSettings.scenes = next;
        }
    }
}
