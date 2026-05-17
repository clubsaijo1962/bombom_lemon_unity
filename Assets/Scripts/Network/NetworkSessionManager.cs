using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace BomBomLemon.Network
{
    /// <summary>
    /// Manages multiplayer sessions for both local (pass-and-play) and online (UGS Relay + Lobby) modes.
    /// Online methods are async and integrate with Unity Gaming Services.
    /// </summary>
    public class NetworkSessionManager : MonoBehaviour
    {
        // ------------------------------------------------------------------ singleton
        public static NetworkSessionManager Instance { get; private set; }

        // ------------------------------------------------------------------ state
        /// <summary>The currently active multiplayer mode.</summary>
        public MultiplayerMode CurrentMode { get; private set; } = MultiplayerMode.Local;

        /// <summary>Number of players in the current local session.</summary>
        public int LocalPlayerCount { get; private set; }

        /// <summary>Join code for the current online room (empty if not in an online session).</summary>
        public string OnlineRoomCode { get; private set; } = string.Empty;

        // ------------------------------------------------------------------ events
        [Header("Session Events")]
        public UnityEvent OnSessionStarted  = new UnityEvent();
        public UnityEvent<string> OnPlayerJoined  = new UnityEvent<string>();
        public UnityEvent<string> OnPlayerLeft    = new UnityEvent<string>();
        public UnityEvent OnSessionEnded    = new UnityEvent();

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
        }

        // ------------------------------------------------------------------ local
        /// <summary>
        /// Initialises a pass-and-play session on a single device.
        /// </summary>
        /// <param name="playerCount">Number of players (2-20).</param>
        public async Task StartLocalSession(int playerCount)
        {
            CurrentMode      = MultiplayerMode.Local;
            LocalPlayerCount = Mathf.Clamp(playerCount, 2, 20);
            OnlineRoomCode   = string.Empty;

            Debug.Log($"[NetworkSessionManager] Local session started with {LocalPlayerCount} player(s).");
            OnSessionStarted.Invoke();

            await Task.CompletedTask; // placeholder for potential async init work
        }

        // ------------------------------------------------------------------ online
        /// <summary>
        /// Creates an online room via Unity Relay + Lobby.
        /// Requires Unity Gaming Services to be initialised and the player to be authenticated.
        /// </summary>
        /// <param name="roomName">Human-readable lobby name.</param>
        /// <param name="maxPlayers">Maximum number of players allowed in this room (2-20).</param>
        public async Task CreateOnlineRoom(string roomName, int maxPlayers)
        {
            CurrentMode  = MultiplayerMode.Online;
            maxPlayers   = Mathf.Clamp(maxPlayers, 2, 20);

            Debug.Log($"[NetworkSessionManager] Creating online room '{roomName}' (max {maxPlayers} players)...");

            // TODO: Replace with actual UGS Relay allocation + Lobby creation calls.
            // Example flow:
            //   var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            //   OnlineRoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //   var lobbyOptions = new CreateLobbyOptions { ... };
            //   await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, lobbyOptions);

            OnlineRoomCode = "DEMO01"; // placeholder
            Debug.Log($"[NetworkSessionManager] Online room created. Join code: {OnlineRoomCode}");
            OnSessionStarted.Invoke();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Joins an existing online room using its join code.
        /// </summary>
        /// <param name="roomCode">The relay join code obtained from the host.</param>
        public async Task JoinOnlineRoom(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                Debug.LogError("[NetworkSessionManager] Room code cannot be empty.");
                return;
            }

            CurrentMode    = MultiplayerMode.Online;
            OnlineRoomCode = roomCode.Trim().ToUpperInvariant();

            Debug.Log($"[NetworkSessionManager] Joining online room with code '{OnlineRoomCode}'...");

            // TODO: Replace with actual UGS Relay join + Lobby join calls.
            // Example flow:
            //   var joinAllocation = await RelayService.Instance.JoinAllocationAsync(OnlineRoomCode);
            //   NetworkManager.Singleton.GetComponent<UnityTransport>()
            //       .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            //   await LobbyService.Instance.JoinLobbyByCodeAsync(OnlineRoomCode);
            //   NetworkManager.Singleton.StartClient();

            OnSessionStarted.Invoke();
            OnPlayerJoined.Invoke("LocalPlayer"); // placeholder

            await Task.CompletedTask;
        }

        // ------------------------------------------------------------------ shared
        /// <summary>Leaves the current session and cleans up network resources.</summary>
        public async Task LeaveSession()
        {
            Debug.Log("[NetworkSessionManager] Leaving session...");

            // TODO: Disconnect NetworkManager, leave Lobby, etc.
            // NetworkManager.Singleton.Shutdown();

            OnlineRoomCode   = string.Empty;
            LocalPlayerCount = 0;
            OnSessionEnded.Invoke();

            await Task.CompletedTask;
        }
    }
}
