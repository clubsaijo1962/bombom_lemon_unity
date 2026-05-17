using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BomBomLemon.Player;

namespace BomBomLemon.Board
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private int tileIndex;
        [SerializeField] private TileType type = TileType.Normal;
        [SerializeField] private List<int> nextTileIndices = new();
        [SerializeField] private int warpDestination = -1;

        public int TileIndex => tileIndex;
        public TileType Type => type;
        public IReadOnlyList<int> NextTileIndices => nextTileIndices;
        public int WarpDestination => warpDestination;

        public UnityEvent<PlayerData> OnPlayerLanded = new();

        public void PlayerLanded(PlayerData player)
        {
            OnPlayerLanded.Invoke(player);
        }
    }
}
