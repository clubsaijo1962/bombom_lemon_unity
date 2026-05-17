using System.Collections.Generic;
using UnityEngine;

namespace BomBomLemon.Game.Topics
{
    /// <summary>
    /// お題リストを管理する ScriptableObject。
    /// Create via: Assets > Create > BomBom Lemon > Topic Database
    /// </summary>
    [CreateAssetMenu(fileName = "TopicDatabase", menuName = "BomBom Lemon/Topic Database", order = 1)]
    public class TopicDatabase : ScriptableObject
    {
        [SerializeField] private List<Topic> topics = new();

        public IReadOnlyList<Topic> Topics => topics;

        /// ランダムにお題を1つ取得する
        public Topic GetRandom()
        {
            if (topics.Count == 0) return FallbackTopic();
            return topics[Random.Range(0, topics.Count)];
        }

        /// ゲーム起動直後でも動くデフォルトお題（エディタ未設定時用）
        static Topic FallbackTopic() =>
            new("好きな食べ物の甘さ", "甘くない", "めちゃ甘い");

        /// エディタ用: デフォルトのお題セットをリセット
        [ContextMenu("Load Default Japanese Topics")]
        void LoadDefaultTopics()
        {
            topics = new List<Topic>
            {
                new("かわいいもの", "かわいくない", "激かわいい"),
                new("大きいもの", "とても小さい", "とても大きい"),
                new("高価なもの", "安い", "超高い"),
                new("速いもの", "ゆっくり", "超速い"),
                new("辛い食べ物", "全然辛くない", "激辛"),
                new("怖いもの", "全然怖くない", "めちゃ怖い"),
                new("難しいこと", "超簡単", "超難しい"),
                new("人気のもの", "誰も知らない", "超有名"),
                new("重いもの", "軽い", "重い"),
                new("暑い場所", "涼しい", "灼熱"),
                new("懐かしいもの", "最近のこと", "大昔のこと"),
                new("眠くなること", "目が覚める", "すぐ寝れる"),
                new("笑えること", "全然面白くない", "爆笑する"),
                new("感動すること", "無感動", "号泣する"),
                new("甘い食べ物", "甘くない", "めちゃ甘い"),
            };
        }
    }
}
