using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BomBomLemon.Player;

namespace BomBomLemon.UI
{
    /// <summary>
    /// Controls the in-game heads-up display: current player indicator, scores, dice results, and event messages.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        // ------------------------------------------------------------------ inspector
        [Header("Current Player")]
        [SerializeField] private TextMeshProUGUI currentPlayerNameText;
        [SerializeField] private Image           currentPlayerColorImage;

        [Header("Dice")]
        [SerializeField] private TextMeshProUGUI diceResultText;
        [SerializeField] private GameObject      diceResultPanel;

        [Header("Event Message")]
        [SerializeField] private TextMeshProUGUI eventMessageText;
        [SerializeField] private GameObject      eventMessagePanel;

        [Header("Score (optional scrolling list)")]
        [SerializeField] private TextMeshProUGUI scoreListText;

        // ------------------------------------------------------------------ public API
        /// <summary>Updates the displayed current-player information.</summary>
        public void UpdateCurrentPlayer(PlayerData player)
        {
            if (player == null) return;

            if (currentPlayerNameText  != null) currentPlayerNameText.text  = player.PlayerName;
            if (currentPlayerColorImage != null) currentPlayerColorImage.color = player.PlayerColor;
        }

        /// <summary>Updates the score display for the given player.</summary>
        public void UpdateScore(PlayerData player, int newScore)
        {
            if (player == null) return;
            player.Score = newScore;

            // Refresh the score list if present.
            RefreshScoreList();
        }

        /// <summary>Shows the dice result for a brief moment.</summary>
        public void ShowDiceResult(int result)
        {
            if (diceResultText  != null) diceResultText.text  = result.ToString();
            if (diceResultPanel != null) diceResultPanel.SetActive(true);
        }

        /// <summary>Hides the dice result panel.</summary>
        public void HideDiceResult()
        {
            if (diceResultPanel != null) diceResultPanel.SetActive(false);
        }

        /// <summary>Shows a temporary event message on the HUD.</summary>
        public void ShowEventMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (eventMessageText  != null) eventMessageText.text  = message;
            if (eventMessagePanel != null) eventMessagePanel.SetActive(true);

            // Auto-hide after 3 seconds.
            CancelInvoke(nameof(HideEventMessage));
            Invoke(nameof(HideEventMessage), 3f);
        }

        /// <summary>Hides the event message panel.</summary>
        public void HideEventMessage()
        {
            if (eventMessagePanel != null) eventMessagePanel.SetActive(false);
        }

        // ------------------------------------------------------------------ helpers
        private void RefreshScoreList()
        {
            if (scoreListText == null) return;

            var gm = Core.GameManager.Instance;
            if (gm == null || gm.Players == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var p in gm.Players)
                sb.AppendLine($"{p.PlayerName}: {p.Score} pt");

            scoreListText.text = sb.ToString();
        }
    }
}
