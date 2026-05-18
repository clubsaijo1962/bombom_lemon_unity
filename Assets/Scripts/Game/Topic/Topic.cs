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

        [Tooltip("Topic text in English")]
        public string TextEN;

        [Tooltip("Low-end guide label in English")]
        public string LowLabelEN;

        [Tooltip("High-end guide label in English")]
        public string HighLabelEN;

        [Tooltip("Low hint example in English")]
        public string HintLowEN;

        [Tooltip("High hint example in English")]
        public string HintHighEN;

        public Topic(string text, string lowLabel, string highLabel,
                     string hintLow, string hintHigh,
                     string textEN, string lowLabelEN, string highLabelEN,
                     string hintLowEN, string hintHighEN)
        {
            Text       = text;
            LowLabel   = lowLabel;
            HighLabel  = highLabel;
            HintLow    = hintLow;
            HintHigh   = hintHigh;
            TextEN      = textEN;
            LowLabelEN  = lowLabelEN;
            HighLabelEN = highLabelEN;
            HintLowEN   = hintLowEN;
            HintHighEN  = hintHighEN;
        }
    }
}
