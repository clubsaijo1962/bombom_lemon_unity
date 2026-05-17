using UnityEngine;
using BomBomLemon.Network;

namespace BomBomLemon.UI
{
    /// <summary>
    /// Handles button interactions on the Main Menu screen.
    /// Wire up button OnClick events to these methods in the Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        // ------------------------------------------------------------------ button handlers
        /// <summary>Transitions to the Player Setup screen for a local (pass-and-play) game.</summary>
        public void OnLocalPlayButton()
        {
            Debug.Log("[MainMenuUI] Local Play selected.");
            NetworkSessionManager.Instance?.StartLocalSession(2); // default 2; PlayerSetup will override
            UIManager.Instance?.ShowPlayerSetup();
        }

        /// <summary>Transitions to the online multiplayer lobby flow.</summary>
        public void OnOnlinePlayButton()
        {
            Debug.Log("[MainMenuUI] Online Play selected.");
            // TODO: Show online lobby screen (create / join room UI).
            // For now, navigate to PlayerSetup in online mode placeholder.
            UIManager.Instance?.ShowPlayerSetup();
        }

        /// <summary>Opens the Settings panel.</summary>
        public void OnSettingsButton()
        {
            Debug.Log("[MainMenuUI] Settings selected.");
            // TODO: UIManager.Instance?.ShowPanel("Settings");
        }

        /// <summary>Quits the application.</summary>
        public void OnQuitButton()
        {
            Debug.Log("[MainMenuUI] Quit.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
