using System.Collections.Generic;
using UnityEngine;
using BomBomLemon.Core;
using BomBomLemon.Player;

namespace BomBomLemon.UI
{
    /// <summary>
    /// Manages the player configuration screen where users set player count, names, and colours.
    /// </summary>
    public class PlayerSetupUI : MonoBehaviour
    {
        // ------------------------------------------------------------------ constants
        private const int AbsoluteMinPlayers = 2;
        private const int AbsoluteMaxPlayers = 20;

        // ------------------------------------------------------------------ inspector
        [Header("Configuration")]
        [SerializeField] private int _selectedPlayerCount = 2;

        // ------------------------------------------------------------------ state
        /// <summary>Number of players selected for this session (clamped 2-20).</summary>
        public int SelectedPlayerCount
        {
            get => _selectedPlayerCount;
            private set => _selectedPlayerCount = Mathf.Clamp(value, AbsoluteMinPlayers, AbsoluteMaxPlayers);
        }

        /// <summary>Player configurations built up via the UI.</summary>
        public List<PlayerData> ConfiguredPlayers { get; private set; } = new List<PlayerData>();

        // ------------------------------------------------------------------ player management
        /// <summary>Adds a new default player entry (up to MaxPlayers).</summary>
        public void AddPlayer()
        {
            if (ConfiguredPlayers.Count >= AbsoluteMaxPlayers)
            {
                Debug.LogWarning("[PlayerSetupUI] Maximum player count reached.");
                return;
            }

            var player = new PlayerData
            {
                PlayerIndex   = ConfiguredPlayers.Count,
                PlayerName    = $"Player {ConfiguredPlayers.Count + 1}",
                PlayerColor   = GetDefaultColor(ConfiguredPlayers.Count),
                IsLocalPlayer = true,
                IsBot         = false,
            };
            ConfiguredPlayers.Add(player);
            SelectedPlayerCount = ConfiguredPlayers.Count;

            Debug.Log($"[PlayerSetupUI] Added {player.PlayerName}. Total: {ConfiguredPlayers.Count}");
        }

        /// <summary>Removes the last player entry (keeps at least MinPlayers).</summary>
        public void RemovePlayer()
        {
            if (ConfiguredPlayers.Count <= AbsoluteMinPlayers)
            {
                Debug.LogWarning("[PlayerSetupUI] Minimum player count reached.");
                return;
            }

            ConfiguredPlayers.RemoveAt(ConfiguredPlayers.Count - 1);
            SelectedPlayerCount = ConfiguredPlayers.Count;

            Debug.Log($"[PlayerSetupUI] Removed last player. Total: {ConfiguredPlayers.Count}");
        }

        /// <summary>Sets the name for the player at the given index.</summary>
        public void SetPlayerName(int index, string name)
        {
            if (!IsValidIndex(index)) return;
            ConfiguredPlayers[index].PlayerName = string.IsNullOrWhiteSpace(name)
                ? $"Player {index + 1}"
                : name.Trim();
        }

        /// <summary>Sets the colour for the player at the given index.</summary>
        public void SetPlayerColor(int index, Color color)
        {
            if (!IsValidIndex(index)) return;
            ConfiguredPlayers[index].PlayerColor = color;
        }

        /// <summary>Validates the configuration and launches the game.</summary>
        public void OnStartGameButton()
        {
            if (ConfiguredPlayers.Count < AbsoluteMinPlayers)
            {
                Debug.LogWarning("[PlayerSetupUI] Not enough players configured.");
                return;
            }

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[PlayerSetupUI] GameManager not found.");
                return;
            }

            gm.ClearPlayers();
            foreach (var player in ConfiguredPlayers)
                gm.AddPlayer(player);

            gm.StartGame();
            UIManager.Instance?.ShowGameHUD();
        }

        // ------------------------------------------------------------------ unity
        private void OnEnable()
        {
            // Initialise with the minimum number of default players.
            if (ConfiguredPlayers.Count == 0)
            {
                for (int i = 0; i < AbsoluteMinPlayers; i++)
                    AddPlayer();
            }
        }

        // ------------------------------------------------------------------ helpers
        private bool IsValidIndex(int index)
        {
            if (index >= 0 && index < ConfiguredPlayers.Count) return true;
            Debug.LogWarning($"[PlayerSetupUI] Index {index} out of range (0-{ConfiguredPlayers.Count - 1}).");
            return false;
        }

        private static Color GetDefaultColor(int index)
        {
            Color[] palette =
            {
                Color.red, Color.blue, Color.green, Color.yellow,
                Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f),
                Color.white, Color.gray,
                new Color(0f, 0.5f, 0f), new Color(0.5f, 0.25f, 0f),
                new Color(1f, 0.75f, 0.8f), new Color(0.6f, 0.8f, 0.2f),
                new Color(0f, 0.75f, 1f), new Color(1f, 0.5f, 0.5f),
                new Color(0.5f, 1f, 0.5f), new Color(0.5f, 0.5f, 1f),
                new Color(1f, 1f, 0.5f), new Color(0.75f, 0.5f, 0.75f),
            };
            return palette[index % palette.Length];
        }
    }
}
