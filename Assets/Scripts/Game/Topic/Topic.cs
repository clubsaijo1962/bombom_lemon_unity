using UnityEngine;

namespace BomBomLemon.Game.Topics
{
    /// <summary>
    /// お題データ。小さい数字 = 少ない/弱い、大きい数字 = 多い/強い という方向で統一する。
    /// </summary>
    [System.Serializable]
    public class Topic
    {
        [Tooltip("お題テキスト（例: かわいいもの順）")]
        public string Text;

        [Tooltip("小さい側の説明（例: かわいくない）")]
        public string LowLabel;

        [Tooltip("大きい側の説明（例: めちゃかわいい）")]
        public string HighLabel;

        [Tooltip("1側のヒント具体例（ヒントボタンで表示）")]
        public string HintLow;

        [Tooltip("99側のヒント具体例（ヒントボタンで表示）")]
        public string HintHigh;

        public Topic(string text, string lowLabel, string highLabel, string hintLow = "", string hintHigh = "")
        {
            Text = text;
            LowLabel = lowLabel;
            HighLabel = highLabel;
            HintLow = hintLow;
            HintHigh = hintHigh;
        }
    }
}
