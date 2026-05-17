using UnityEngine;
using TMPro;
using BomBomLemon.Player;

namespace BomBomLemon.UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentPlayerLabel;
        [SerializeField] private TextMeshProUGUI diceResultLabel;
        [SerializeField] private TextMeshProUGUI eventMessageLabel;

        public void UpdateCurrentPlayer(PlayerData player)
        {
            if (currentPlayerLabel)
                currentPlayerLabel.text = $"{player.PlayerName} のターン";
        }

        public void UpdateScore(PlayerData player, int newScore)
        {
            player.Score = newScore;
        }

        public void ShowDiceResult(int result)
        {
            if (diceResultLabel)
                diceResultLabel.text = result.ToString();
        }

        public void ShowEventMessage(string message)
        {
            if (eventMessageLabel)
            {
                eventMessageLabel.text = message;
                eventMessageLabel.gameObject.SetActive(true);
            }
        }

        public void HideEventMessage()
        {
            if (eventMessageLabel)
                eventMessageLabel.gameObject.SetActive(false);
        }
    }
}
