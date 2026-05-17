using UnityEngine;

namespace BomBomLemon.Game.Card
{
    /// <summary>
    /// プレイヤーに配られる 1〜99 の数字カード
    /// </summary>
    [System.Serializable]
    public class NumberCard
    {
        public int Value;           // 1〜99
        public int OwnerIndex;      // 保持プレイヤーのインデックス
        public bool IsRevealed;     // 公開済みかどうか

        public NumberCard(int value, int ownerIndex)
        {
            Value = value;
            OwnerIndex = ownerIndex;
            IsRevealed = false;
        }
    }
}
