using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BomBomLemon.Player;

namespace BomBomLemon.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameSettings settings;

        public GameSettings Settings => settings;
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public List<PlayerData> Players { get; } = new();
        public int CurrentPlayerIndex { get; private set; }
        public PlayerData CurrentPlayer => Players.Count > 0 ? Players[CurrentPlayerIndex] : null;

        public UnityEvent<GameState> OnStateChanged = new();
        public UnityEvent<PlayerData> OnTurnChanged = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged.Invoke(newState);
        }

        public void ClearPlayers() => Players.Clear();

        public void AddPlayer(PlayerData player) => Players.Add(player);

        public void StartGame(List<PlayerData> players)
        {
            Players.Clear();
            Players.AddRange(players);
            StartGame();
        }

        public void StartGame()
        {
            CurrentPlayerIndex = 0;
            ChangeState(GameState.Playing);
            OnTurnChanged.Invoke(CurrentPlayer);
        }

        public void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            ChangeState(GameState.TurnTransition);
            OnTurnChanged.Invoke(CurrentPlayer);
        }

        public void EndGame()
        {
            ChangeState(GameState.GameOver);
        }
    }
}
