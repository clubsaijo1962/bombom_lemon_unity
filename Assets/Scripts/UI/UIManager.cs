using System.Collections.Generic;
using UnityEngine;

namespace BomBomLemon.UI
{
    /// <summary>
    /// Central UI singleton. Controls panel visibility and routes top-level screen transitions.
    /// Panels are registered by name so they can be shown/hidden without hard references.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ------------------------------------------------------------------ singleton
        public static UIManager Instance { get; private set; }

        // ------------------------------------------------------------------ inspector
        [Header("Panels (assign in Inspector)")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject playerSetupPanel;
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private GameObject gameOverPanel;

        // ------------------------------------------------------------------ internal registry
        private readonly Dictionary<string, GameObject> _panelRegistry = new Dictionary<string, GameObject>();

        // ------------------------------------------------------------------ unity
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterDefaultPanels();
        }

        private void RegisterDefaultPanels()
        {
            RegisterPanel("MainMenu",    mainMenuPanel);
            RegisterPanel("PlayerSetup", playerSetupPanel);
            RegisterPanel("GameHUD",     gameHUDPanel);
            RegisterPanel("GameOver",    gameOverPanel);
        }

        // ------------------------------------------------------------------ public API
        /// <summary>Registers a panel under the given name so it can be shown/hidden by name.</summary>
        public void RegisterPanel(string panelName, GameObject panel)
        {
            if (panel == null) return;
            _panelRegistry[panelName] = panel;
            panel.SetActive(false);
        }

        /// <summary>Shows the panel with the given name and hides all others.</summary>
        public void ShowPanel(string panelName)
        {
            foreach (var kvp in _panelRegistry)
                kvp.Value.SetActive(kvp.Key == panelName);
        }

        /// <summary>Shows a panel without hiding other panels.</summary>
        public void ShowPanelAdditive(string panelName)
        {
            if (_panelRegistry.TryGetValue(panelName, out var panel))
                panel.SetActive(true);
        }

        /// <summary>Hides the panel with the given name.</summary>
        public void HidePanel(string panelName)
        {
            if (_panelRegistry.TryGetValue(panelName, out var panel))
                panel.SetActive(false);
        }

        // ------------------------------------------------------------------ convenience methods
        public void ShowMainMenu()    => ShowPanel("MainMenu");
        public void ShowPlayerSetup() => ShowPanel("PlayerSetup");
        public void ShowGameHUD()     => ShowPanel("GameHUD");
        public void ShowGameOver()    => ShowPanel("GameOver");
    }
}
