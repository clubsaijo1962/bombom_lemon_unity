using UnityEngine;

namespace BomBomLemon.Core
{
    /// <summary>
    /// ScriptableObject holding global game configuration.
    /// Create via Assets > Create > BomBom Lemon > Game Settings.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "BomBom Lemon/Game Settings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Player Limits")]
        [Min(2)]
        public int MinPlayers = 2;

        [Min(2)]
        public int MaxPlayers = 20;

        [Header("Board Configuration")]
        [Tooltip("Total number of tiles on the board.")]
        [Min(1)]
        public int BoardSize = 40;

        [Header("Dice Configuration")]
        [Min(1)]
        public int DiceCount = 1;

        [Min(2)]
        public int DiceFaces = 6;

        [Header("Movement")]
        [Tooltip("World units per second when a token moves between tiles.")]
        [Min(0.1f)]
        public float TokenMoveSpeed = 5f;

        [Header("Multiplayer")]
        public bool AllowOnlinePlay = true;
    }
}
