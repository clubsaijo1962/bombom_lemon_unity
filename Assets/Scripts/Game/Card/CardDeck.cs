using System.Collections.Generic;
using UnityEngine;

namespace BomBomLemon.Game.Card
{
    /// <summary>
    /// 1〜99 のカードデッキ。シャッフルしてプレイヤーに配る。
    /// </summary>
    public class CardDeck
    {
        private readonly List<int> _remaining = new();

        public CardDeck()
        {
            Reset();
        }

        /// デッキを 1〜99 でリセットしてシャッフル
        public void Reset()
        {
            _remaining.Clear();
            for (int i = 1; i <= 99; i++)
                _remaining.Add(i);
            Shuffle();
        }

        /// 1枚引く。デッキが空なら -1 を返す。
        public int Draw()
        {
            if (_remaining.Count == 0) return -1;
            int index = _remaining.Count - 1;
            int value = _remaining[index];
            _remaining.RemoveAt(index);
            return value;
        }

        /// playerCount 人分のカードをまとめて配る
        public List<NumberCard> DealCards(int playerCount)
        {
            var cards = new List<NumberCard>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                int value = Draw();
                cards.Add(new NumberCard(value, i));
            }
            return cards;
        }

        public int Remaining => _remaining.Count;

        void Shuffle()
        {
            for (int i = _remaining.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_remaining[i], _remaining[j]) = (_remaining[j], _remaining[i]);
            }
        }
    }
}
