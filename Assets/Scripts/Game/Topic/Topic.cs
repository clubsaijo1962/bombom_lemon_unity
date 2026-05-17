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

        public Topic(string text, string lowLabel, string highLabel)
        {
            Text = text;
            LowLabel = lowLabel;
            HighLabel = highLabel;
        }
    }
}
