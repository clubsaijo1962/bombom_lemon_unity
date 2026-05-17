using UnityEngine;

namespace BomBomLemon.Player
{
    [System.Serializable]
    public class PlayerData
    {
        public int PlayerIndex;
        public string PlayerName = "Player";
        public Color PlayerColor = Color.white;
        public int CurrentTileIndex;
        public int Score;
        public bool IsLocalPlayer = true;
        public bool IsBot;
    }
}
