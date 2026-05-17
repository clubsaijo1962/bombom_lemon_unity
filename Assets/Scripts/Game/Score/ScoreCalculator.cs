using UnityEngine;

namespace BomBomLemon.Game.Score
{
    /// <summary>
    /// 予想と実際の数字の乖離からポイントを計算する
    /// </summary>
    public static class ScoreCalculator
    {
        /// <summary>
        /// 乖離からポイント変動を計算する。
        /// 完全一致: +100 / 1差: +90 / 5差: +50 / 10差: 0 / 20差以上: マイナス
        /// </summary>
        public static int Calculate(int actual, int guessed)
        {
            int diff = Mathf.Abs(actual - guessed);

            return diff switch
            {
                0       => 100,
                1       => 90,
                2       => 80,
                3       => 70,
                4       => 60,
                <= 5    => 50,
                <= 8    => 30,
                <= 10   => 10,
                <= 15   => 0,
                <= 20   => -10,
                <= 30   => -20,
                _       => -30,
            };
        }

        /// <summary>
        /// 乖離を「完全一致」「惜しい」「ほぼ正解」「外れ」「大外れ」で評価する
        /// </summary>
        public static string GetEvaluation(int actual, int guessed)
        {
            int diff = Mathf.Abs(actual - guessed);
            return diff switch
            {
                0       => "完全一致！",
                <= 2    => "惜しい！",
                <= 5    => "ほぼ正解！",
                <= 10   => "まあまあ",
                <= 20   => "外れ",
                _       => "大外れ...",
            };
        }
    }
}
