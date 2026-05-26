using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using BomBomLemon.PlayerSetup;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

namespace BomBomLemon.Network
{
    /// <summary>
    /// Unity Gaming Services Lobby のラッパー。シーンをまたいで生存するシングルトン。
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────
        static LobbyManager _instance;
        public static LobbyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[LobbyManager]");
                    _instance = go.AddComponent<LobbyManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── 状態 ─────────────────────────────────────────────────────────────
        public bool IsInitialized { get; private set; }
        public Lobby CurrentLobby { get; private set; }

        // ── イベント ──────────────────────────────────────────────────────────
        /// <summary>プレイヤーリストが更新されたとき</summary>
        public event Action<List<LobbyPlayer>> OnPlayersUpdated;
        /// <summary>ロビーが削除された（ホストが解散）とき</summary>
        public event Action OnLobbyDeleted;

        // ── プライベート ──────────────────────────────────────────────────────
        Coroutine _heartbeatRoutine;
        Coroutine _pollRoutine;
        List<LobbyPlayer> _lastPlayers = new List<LobbyPlayer>();

        const float HeartbeatInterval = 15f;
        const float PollInterval = 1.0f;   // 1秒ごとにポーリング（2.5→1s で同期遅延を解消）

        // Lobby カスタムデータキー
        const string KeyPin  = "Pin";
        const string KeyMode = "Mode";
        const string KeyHell = "Hell";

        // ── 初期化 ────────────────────────────────────────────────────────────
        /// <summary>UGS 初期化と匿名サインイン。複数回呼んでも安全。</summary>
        public async Task InitializeAsync()
        {
            if (IsInitialized) return;
            var initOptions = new InitializationOptions();

            // ParrelSync クローンエディタは同じ匿名アカウントになるため
            // 別プロファイルを設定して異なるプレイヤーIDを取得する
#if UNITY_EDITOR
            if (ParrelSync.ClonesManager.IsClone())
                initOptions.SetProfile("ParrelSyncClone");
#endif

            await UnityServices.InitializeAsync(initOptions);
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            IsInitialized = true;
            Debug.Log($"[LobbyManager] 初期化完了 PlayerID={AuthenticationService.Instance.PlayerId}");
        }

        // ── 部屋作成（ホスト）────────────────────────────────────────────────
        public async Task<Lobby> CreateLobbyAsync(
            string pin, string hostName,
            RoomConfig.GameMode mode, bool isHellMode,
            int maxPlayers = 24)
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    [KeyPin]  = new DataObject(DataObject.VisibilityOptions.Public, pin,
                                    DataObject.IndexOptions.S1),
                    [KeyMode] = new DataObject(DataObject.VisibilityOptions.Public,
                                    ((int)mode).ToString()),
                    [KeyHell] = new DataObject(DataObject.VisibilityOptions.Public,
                                    isHellMode ? "1" : "0"),
                },
                Player = BuildLocalPlayer(hostName)
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{hostName}の部屋", maxPlayers, options);

            RoomConfig.LobbyId       = CurrentLobby.Id;
            RoomConfig.LocalPlayerId = AuthenticationService.Instance.PlayerId;
            RoomConfig.IsHost        = true;
            RoomConfig.HostName      = hostName;   // ホスト自身も名前を保持

            StartHeartbeat();
            StartPoll();

            Debug.Log($"[LobbyManager] 部屋作成 LobbyId={CurrentLobby.Id}");
            return CurrentLobby;
        }

        // ── PINで部屋を検索して入室（ゲスト）──────────────────────────────────
        /// <summary>PINに一致する部屋が見つからない場合は LobbyNotFoundException をスロー。</summary>
        public async Task<Lobby> JoinLobbyByPinAsync(string pin, string playerName)
        {
            var queryResult = await LobbyService.Instance.QueryLobbiesAsync(
                new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(
                            QueryFilter.FieldOptions.S1, pin, QueryFilter.OpOptions.EQ)
                    },
                    Count = 1
                });

            if (queryResult.Results.Count == 0)
                throw new LobbyNotFoundException($"PIN={pin} の部屋が見つかりません");

            var target = queryResult.Results[0];

            // Lobby のデータを RoomConfig に反映
            if (target.Data != null)
            {
                if (target.Data.TryGetValue(KeyMode, out var modeData))
                    RoomConfig.Mode = (RoomConfig.GameMode)int.Parse(modeData.Value);
                if (target.Data.TryGetValue(KeyHell, out var hellData))
                    RoomConfig.IsHellMode = hellData.Value == "1";
                if (target.Data.TryGetValue(KeyPin, out var pinData))
                    RoomConfig.Pin = pinData.Value;
            }

            var hostPlayer = target.Players.Count > 0 ? target.Players[0] : null;
            if (hostPlayer != null &&
                hostPlayer.Data != null &&
                hostPlayer.Data.TryGetValue("Name", out var hostNameData))
                RoomConfig.HostName = hostNameData.Value;

            // 同じ名前のプレイヤーが既に入室していないか確認
            foreach (var p in target.Players)
            {
                if (p.Data != null &&
                    p.Data.TryGetValue("Name", out var existingName) &&
                    string.Equals(existingName.Value, playerName,
                        System.StringComparison.OrdinalIgnoreCase))
                    throw new DuplicatePlayerNameException(playerName);
            }

            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(
                target.Id,
                new JoinLobbyByIdOptions { Player = BuildLocalPlayer(playerName) });

            RoomConfig.LobbyId       = CurrentLobby.Id;
            RoomConfig.LocalPlayerId = AuthenticationService.Instance.PlayerId;
            RoomConfig.IsHost        = false;

            StartPoll();

            Debug.Log($"[LobbyManager] 入室成功 LobbyId={CurrentLobby.Id}");
            return CurrentLobby;
        }

        // ── ゲスト退出 ────────────────────────────────────────────────────────
        public async Task LeaveAsync()
        {
            StopRoutines();
            if (!string.IsNullOrEmpty(RoomConfig.LobbyId) &&
                !string.IsNullOrEmpty(RoomConfig.LocalPlayerId))
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(
                        RoomConfig.LobbyId, RoomConfig.LocalPlayerId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LobbyManager] LeaveAsync: {e.Message}");
                }
            }
            ClearRoomConfig();
        }

        // ── ホスト解散 ────────────────────────────────────────────────────────
        public async Task DissolveAsync()
        {
            StopRoutines();
            if (!string.IsNullOrEmpty(RoomConfig.LobbyId))
            {
                try
                {
                    await LobbyService.Instance.DeleteLobbyAsync(RoomConfig.LobbyId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LobbyManager] DissolveAsync: {e.Message}");
                }
            }
            ClearRoomConfig();
        }

        // ── ハートビート（ホスト専用）────────────────────────────────────────
        void StartHeartbeat()
        {
            if (_heartbeatRoutine != null) StopCoroutine(_heartbeatRoutine);
            _heartbeatRoutine = StartCoroutine(HeartbeatCoroutine());
        }

        IEnumerator HeartbeatCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(HeartbeatInterval);
                if (string.IsNullOrEmpty(RoomConfig.LobbyId)) yield break;
                // fire-and-forget（失敗しても続行）
                LobbyService.Instance.SendHeartbeatPingAsync(RoomConfig.LobbyId)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            Debug.LogWarning("[LobbyManager] Heartbeat: " + t.Exception?.Message);
                    });
            }
        }

        // ── ポーリング ────────────────────────────────────────────────────────
        void StartPoll()
        {
            if (_pollRoutine != null) StopCoroutine(_pollRoutine);
            _pollRoutine = StartCoroutine(PollCoroutine());
        }

        IEnumerator PollCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(PollInterval);
                if (string.IsNullOrEmpty(RoomConfig.LobbyId)) yield break;

                var task = LobbyService.Instance.GetLobbyAsync(RoomConfig.LobbyId);
                yield return new WaitUntil(() => task.IsCompleted);

                if (task.IsFaulted)
                {
                    // ロビーが存在しない → ホストが解散したと判断
                    Debug.LogWarning("[LobbyManager] Poll failed: " + task.Exception?.Message);
                    OnLobbyDeleted?.Invoke();
                    yield break;
                }

                CurrentLobby = task.Result;
                var players = CurrentLobby.Players;

                // 変化検知を行わず毎回発火（検知ロジックの取りこぼしを防ぐ）
                _lastPlayers = new List<LobbyPlayer>(players);
                OnPlayersUpdated?.Invoke(players);
            }
        }

        // ── ユーティリティ ────────────────────────────────────────────────────
        static LobbyPlayer BuildLocalPlayer(string name) => new LobbyPlayer
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                ["Name"] = new PlayerDataObject(
                    PlayerDataObject.VisibilityOptions.Public, name)
            }
        };

        void StopRoutines()
        {
            if (_heartbeatRoutine != null)
            {
                StopCoroutine(_heartbeatRoutine); _heartbeatRoutine = null;
            }
            if (_pollRoutine != null)
            {
                StopCoroutine(_pollRoutine); _pollRoutine = null;
            }
        }

        static void ClearRoomConfig()
        {
            RoomConfig.LobbyId       = "";
            RoomConfig.LocalPlayerId = "";
            RoomConfig.IsHost        = false;
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
            StopRoutines();
        }
    }

    /// <summary>PINに一致する Lobby が見つからなかった場合にスロー</summary>
    public class LobbyNotFoundException : Exception
    {
        public LobbyNotFoundException(string msg) : base(msg) { }
    }

    /// <summary>同じ名前のプレイヤーが既に入室している場合にスロー</summary>
    public class DuplicatePlayerNameException : Exception
    {
        public string PlayerName { get; }
        public DuplicatePlayerNameException(string name)
            : base($"プレイヤー名 '{name}' は既に使用されています") { PlayerName = name; }
    }
}
