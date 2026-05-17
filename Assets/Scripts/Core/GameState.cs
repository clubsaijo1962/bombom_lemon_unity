namespace BomBomLemon.Core
{
    /// <summary>
    /// Represents the high-level state of the game flow.
    /// </summary>
    public enum GameState
    {
        /// <summary>The main menu is displayed.</summary>
        MainMenu,

        /// <summary>Players are being configured (name, color, count).</summary>
        PlayerSetup,

        /// <summary>The game is actively being played.</summary>
        Playing,

        /// <summary>Transition between player turns (animation, pause).</summary>
        TurnTransition,

        /// <summary>A tile event is being resolved.</summary>
        EventResolution,

        /// <summary>The game has ended and results are shown.</summary>
        GameOver,
    }
}
