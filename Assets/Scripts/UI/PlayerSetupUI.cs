using System.Collections.Generic;
using UnityEngine;
using BomBomLemon.Core;
using BomBomLemon.Player;

namespace BomBomLemon.UI
{
    public class PlayerSetupUI : MonoBehaviour
    {
        [SerializeField] private GameSettings settings;

        public int SelectedPlayerCount { get; private set; } = 2;
        public List<PlayerData> ConfiguredPlayers { get; } = new();

        static readonly Color[] DefaultColors =
        {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), Color.white,
        };

        void Start()
        {
            for (int i = 0; i < SelectedPlayerCount; i++)
                AddPlayer();
        }

        public void AddPlayer()
        {
            int max = settings ? settings.MaxPlayers : 20;
            if (ConfiguredPlayers.Count >= max) return;

            int index = ConfiguredPlayers.Count;
            ConfiguredPlayers.Add(new PlayerData
            {
                PlayerIndex = index,
                PlayerName = $"Player {index + 1}",
                PlayerColor = DefaultColors[index % DefaultColors.Length],
                IsLocalPlayer = true,
            });
            SelectedPlayerCount = ConfiguredPlayers.Count;
        }

        public void RemovePlayer()
        {
            int min = settings ? settings.MinPlayers : 2;
            if (ConfiguredPlayers.Count <= min) return;

            ConfiguredPlayers.RemoveAt(ConfiguredPlayers.Count - 1);
            SelectedPlayerCount = ConfiguredPlayers.Count;
        }

        public void SetPlayerName(int index, string name)
        {
            if (index < 0 || index >= ConfiguredPlayers.Count) return;
            ConfiguredPlayers[index].PlayerName = name;
        }

        public void SetPlayerColor(int index, Color color)
        {
            if (index < 0 || index >= ConfiguredPlayers.Count) return;
            ConfiguredPlayers[index].PlayerColor = color;
        }

        public void OnStartGameButton()
        {
            GameManager.Instance.StartGame(ConfiguredPlayers);
            UIManager.Instance.ShowGameHUD();
        }
    }
}
