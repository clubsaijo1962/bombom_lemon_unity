using System.Collections.Generic;
using UnityEngine;
using BomBomLemon.Player;

namespace BomBomLemon.Board
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [SerializeField] private List<Tile> tiles = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public Tile GetTile(int index)
        {
            if (index < 0 || index >= tiles.Count) return null;
            return tiles[index];
        }

        public Tile GetStartTile()
        {
            return tiles.Find(t => t.Type == TileType.Start) ?? tiles[0];
        }

        public void RegisterPlayer(PlayerData player)
        {
            player.CurrentTileIndex = GetStartTile().TileIndex;
        }
    }
}
