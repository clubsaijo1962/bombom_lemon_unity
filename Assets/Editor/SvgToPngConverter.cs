using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

namespace BomBomLemon.Editor
{
    public static class SvgToPngConverter
    {
        [MenuItem("BomBomLemon/Convert SVG to PNG")]
        public static void ConvertAll()
        {
            var svgFiles = Directory.GetFiles(
                Path.Combine(Application.dataPath, "Sprites/UI"),
                "*.svg", SearchOption.TopDirectoryOnly);

            int count = 0;
            foreach (var svg in svgFiles)
            {
                string png = Path.ChangeExtension(svg, ".png");
                if (Convert(svg, png)) count++;
            }

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[SvgToPng] {count} ファイルを変換しました。");
        }

        public static bool Convert(string svgPath, string pngPath)
        {
            try
            {
                var psi = new ProcessStartInfo("python3", $"-c \"import cairosvg; cairosvg.svg2png(url='{svgPath}', write_to='{pngPath}', output_width=256, output_height=256)\"")
                {
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                };
                using var proc = Process.Start(psi);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    UnityEngine.Debug.LogWarning($"[SvgToPng] 変換失敗: {svgPath}\n{proc.StandardError.ReadToEnd()}");
                    return false;
                }
                UnityEngine.Debug.Log($"[SvgToPng] 変換完了: {pngPath}");
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SvgToPng] エラー: {e.Message}");
                return false;
            }
        }
    }
}
