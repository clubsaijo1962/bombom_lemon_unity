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
        /// <summary>ゲーム状態が変化したとき（"Waiting" → "Confirming" など）</summary>
        public event Action<string> OnGameStateChanged;
        /// <summary>全員が Ready="1" になったとき</summary>
        public event Action OnAllPlayersReady;
        /// <summary>最終決定者が秘密の数字を確定したとき（数字を引数で渡す）</summary>
        public event Action<int> OnGameFinalized;
        /// <summary>全ラウンド終了・ライフ残存でゲームクリア</summary>
        public event Action OnGameClear;
        /// <summary>ライフが尽きてゲームオーバー</summary>
        public event Action OnGameOver;

        // ── プライベート ──────────────────────────────────────────────────────
        Coroutine _heartbeatRoutine;
        Coroutine _pollRoutine;
        List<LobbyPlayer> _lastPlayers = new List<LobbyPlayer>();
        string _lastGameState = "";

        const float HeartbeatInterval = 15f;
        const float PollInterval = 1.0f;   // 1秒ごとにポーリング（2.5→1s で同期遅延を解消）

        // Lobby カスタムデータキー
        const string KeyPin       = "Pin";
        const string KeyMode      = "Mode";
        const string KeyHell      = "Hell";
        const string KeyGameState = "State";  // "Waiting" | "Confirming"
        const string KeyGameSeed  = "Seed";   // ランダムシード（整数文字列）

        // Player カスタムデータキー
        const string KeyReady        = "Ready";    // "0" | "1"
        const string KeyPlayerAnswer = "Answer";   // 回答者の回答テキスト
        const string KeyPlayerGuess  = "Guess";    // 各プレイヤーの予想数字（文字列）

        // ゲームフェーズ用ロビーキー
        const string KeyAnswererIdx  = "AnswIdx";  // 回答者インデックス
        const string KeyDeciderIdx   = "DecIdx";   // 最終決定者インデックス
        const string KeyGameTopic    = "Topic";    // このラウンドのお題
        const string KeyFinalNumber  = "FinalNum"; // 最終決定した秘密の数字

        // ラウンド管理キー
        const string KeyLives        = "Lives";    // 共有ライフ
        const string KeyHelpCards    = "Helps";    // 残りヘルプカード
        const string KeyRound        = "Round";    // 現在のラウンドインデックス
        const string KeyDeciderOrder = "Order";    // 最終決定者順序（カンマ区切り）
        const string KeyHelpUsed     = "HelpUsed"; // このラウンドでヘルプカード使用済み "0"|"1"

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

            RoomConfig.LobbyId         = CurrentLobby.Id;
            RoomConfig.LocalPlayerId   = AuthenticationService.Instance.PlayerId;
            RoomConfig.IsHost          = true;
            RoomConfig.HostName        = hostName;
            RoomConfig.LocalPlayerName = hostName;

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

            RoomConfig.LobbyId         = CurrentLobby.Id;
            RoomConfig.LocalPlayerId   = AuthenticationService.Instance.PlayerId;
            RoomConfig.IsHost          = false;
            RoomConfig.LocalPlayerName = playerName;

            StartPoll();

            Debug.Log($"[LobbyManager] 入室成功 LobbyId={CurrentLobby.Id}");
            return CurrentLobby;
        }

        // ── 確認フェーズ開始（ホスト専用）──────────────────────────────────────
        /// <summary>
        /// ランダムシードを生成して Lobby に書き込み、全員に "Confirming" 状態を通知する。
        /// ホストが ゲームスタート ボタンを押したときに呼ぶ。
        /// </summary>
        public async Task StartConfirmPhaseAsync()
        {
            int seed = UnityEngine.Random.Range(10000, 99999);
            RoomConfig.GameSeed    = seed;
            RoomConfig.PlayerIndex = 0;   // ホストは常に index=0

            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState] = new DataObject(DataObject.VisibilityOptions.Member,
                                            "Confirming"),
                        [KeyGameSeed]  = new DataObject(DataObject.VisibilityOptions.Member,
                                            seed.ToString()),
                    }
                });

            Debug.Log($"[LobbyManager] 確認フェーズ開始 Seed={seed}");
        }

        // ── プレイヤー確認完了（全員）──────────────────────────────────────────
        /// <summary>プレイヤーが秘密の数字を確認したときに呼ぶ。</summary>
        public async Task SetPlayerReadyAsync()
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                RoomConfig.LobbyId,
                RoomConfig.LocalPlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        [KeyReady] = new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, "1")
                    }
                });

            Debug.Log("[LobbyManager] プレイヤー Ready 送信");
        }

        // ── ゲームフェーズ管理 ────────────────────────────────────────────────
        /// <summary>
        /// ホストがゲームフェーズを開始する。
        /// 回答者・最終決定者インデックスとお題をロビーに書き込み、全員に通知する。
        /// </summary>
        public async Task StartGamePhaseAsync(int answererIdx, int deciderIdx, string topic)
        {
            RoomConfig.AnswererIndex = answererIdx;
            RoomConfig.DeciderIndex  = deciderIdx;
            RoomConfig.GameTopic     = topic;

            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState]  = new DataObject(DataObject.VisibilityOptions.Member, "Playing"),
                        [KeyAnswererIdx]= new DataObject(DataObject.VisibilityOptions.Member, answererIdx.ToString()),
                        [KeyDeciderIdx] = new DataObject(DataObject.VisibilityOptions.Member, deciderIdx.ToString()),
                        [KeyGameTopic]  = new DataObject(DataObject.VisibilityOptions.Member, topic),
                    }
                });

            Debug.Log($"[LobbyManager] ゲームフェーズ開始 Answerer={answererIdx} Decider={deciderIdx} Topic={topic}");
        }

        /// <summary>回答者が回答テキストを送信する。</summary>
        public async Task UpdatePlayerAnswerAsync(string answer)
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                RoomConfig.LobbyId,
                RoomConfig.LocalPlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        [KeyPlayerAnswer] = new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, answer)
                    }
                });
            Debug.Log("[LobbyManager] 回答送信");
        }

        /// <summary>予想者が予想数字を送信する。</summary>
        public async Task UpdatePlayerGuessAsync(string guess)
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                RoomConfig.LobbyId,
                RoomConfig.LocalPlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        [KeyPlayerGuess] = new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, guess)
                    }
                });
            Debug.Log($"[LobbyManager] 予想送信 Guess={guess}");
        }

        /// <summary>最終決定者が秘密の数字を確定する。</summary>
        public async Task FinalizeGameAsync(int secretNumber)
        {
            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState]  = new DataObject(DataObject.VisibilityOptions.Member, "Result"),
                        [KeyFinalNumber]= new DataObject(DataObject.VisibilityOptions.Member, secretNumber.ToString()),
                    }
                });
            Debug.Log($"[LobbyManager] ゲーム確定 FinalNumber={secretNumber}");
        }

        // ── ラウンド管理（ホスト専用）─────────────────────────────────────────
        /// <summary>
        /// ラウンド制ゲームを開始する（ホスト専用）。
        /// MultiConfirmController の全員Ready後に呼ぶ。
        /// </summary>
        public async Task StartGameWithRoundsAsync(
            int answererIdx, int deciderIdx, string topic,
            int[] deciderOrder, int lives, int helpCards)
        {
            string orderStr = string.Join(",", deciderOrder);
            RoomConfig.AnswererIndex         = answererIdx;
            RoomConfig.DeciderIndex          = deciderIdx;
            RoomConfig.GameTopic             = topic;
            RoomConfig.DeciderOrder          = deciderOrder;
            RoomConfig.TotalRounds           = deciderOrder.Length;
            RoomConfig.Lives                 = lives;
            RoomConfig.HelpCards             = helpCards;
            RoomConfig.CurrentRound          = 0;
            RoomConfig.HelpCardUsedThisRound = false;

            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState]   = new DataObject(DataObject.VisibilityOptions.Member, "Playing"),
                        [KeyAnswererIdx] = new DataObject(DataObject.VisibilityOptions.Member, answererIdx.ToString()),
                        [KeyDeciderIdx]  = new DataObject(DataObject.VisibilityOptions.Member, deciderIdx.ToString()),
                        [KeyGameTopic]   = new DataObject(DataObject.VisibilityOptions.Member, topic),
                        [KeyRound]       = new DataObject(DataObject.VisibilityOptions.Member, "0"),
                        [KeyLives]       = new DataObject(DataObject.VisibilityOptions.Member, lives.ToString()),
                        [KeyHelpCards]   = new DataObject(DataObject.VisibilityOptions.Member, helpCards.ToString()),
                        [KeyDeciderOrder]= new DataObject(DataObject.VisibilityOptions.Member, orderStr),
                        [KeyHelpUsed]    = new DataObject(DataObject.VisibilityOptions.Member, "0"),
                    }
                });

            Debug.Log($"[LobbyManager] ラウンドゲーム開始 Round=0/{deciderOrder.Length} Lives={lives} Helps={helpCards}");
        }

        /// <summary>次のラウンドに進める（ホスト専用）。</summary>
        public async Task AdvanceToNextRoundAsync(
            int newLives, int newHelps, int nextRound,
            int answererIdx, int deciderIdx, string topic)
        {
            RoomConfig.AnswererIndex         = answererIdx;
            RoomConfig.DeciderIndex          = deciderIdx;
            RoomConfig.GameTopic             = topic;
            RoomConfig.CurrentRound          = nextRound;
            RoomConfig.Lives                 = newLives;
            RoomConfig.HelpCards             = newHelps;
            RoomConfig.HelpCardUsedThisRound = false;

            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState]   = new DataObject(DataObject.VisibilityOptions.Member, "Playing"),
                        [KeyAnswererIdx] = new DataObject(DataObject.VisibilityOptions.Member, answererIdx.ToString()),
                        [KeyDeciderIdx]  = new DataObject(DataObject.VisibilityOptions.Member, deciderIdx.ToString()),
                        [KeyGameTopic]   = new DataObject(DataObject.VisibilityOptions.Member, topic),
                        [KeyRound]       = new DataObject(DataObject.VisibilityOptions.Member, nextRound.ToString()),
                        [KeyLives]       = new DataObject(DataObject.VisibilityOptions.Member, newLives.ToString()),
                        [KeyHelpCards]   = new DataObject(DataObject.VisibilityOptions.Member, newHelps.ToString()),
                        [KeyHelpUsed]    = new DataObject(DataObject.VisibilityOptions.Member, "0"),
                    }
                });

            Debug.Log($"[LobbyManager] ラウンド進行 Round={nextRound} Lives={newLives} Helps={newHelps}");
        }

        /// <summary>ヘルプカードを使用する（最終決定者専用）。</summary>
        public async Task UseHelpCardAsync()
        {
            int newHelps = Mathf.Max(0, RoomConfig.HelpCards - 1);
            RoomConfig.HelpCards             = newHelps;
            RoomConfig.HelpCardUsedThisRound = true;

            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyHelpUsed]  = new DataObject(DataObject.VisibilityOptions.Member, "1"),
                        [KeyHelpCards] = new DataObject(DataObject.VisibilityOptions.Member, newHelps.ToString()),
                    }
                });
            Debug.Log($"[LobbyManager] ヘルプカード使用 残り={newHelps}");
        }

        /// <summary>ゲームを終了させる（ホスト専用）。isWin=true でGameClear、false でGameOver。</summary>
        public async Task EndGameAsync(bool isWin)
        {
            string endState = isWin ? "GameClear" : "GameOver";
            await LobbyService.Instance.UpdateLobbyAsync(
                RoomConfig.LobbyId,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [KeyGameState] = new DataObject(DataObject.VisibilityOptions.Member, endState),
                    }
                });
            Debug.Log($"[LobbyManager] ゲーム終了 State={endState}");
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

                // プレイヤーリストを毎回発火
                _lastPlayers = new List<LobbyPlayer>(players);
                OnPlayersUpdated?.Invoke(players);

                // ── ライフ・ヘルプカードを常時同期（Result状態でも更新）──────
                if (CurrentLobby.Data != null)
                {
                    if (CurrentLobby.Data.TryGetValue(KeyLives, out var ld) &&
                        int.TryParse(ld.Value, out int lv)) RoomConfig.Lives = lv;
                    if (CurrentLobby.Data.TryGetValue(KeyHelpCards, out var hd) &&
                        int.TryParse(hd.Value, out int hc)) RoomConfig.HelpCards = hc;
                    if (CurrentLobby.Data.TryGetValue(KeyHelpUsed, out var hu))
                        RoomConfig.HelpCardUsedThisRound = hu.Value == "1";
                }

                // ── ゲーム状態の変化を検知 ──────────────────────────────────
                string newState = "";
                if (CurrentLobby.Data != null &&
                    CurrentLobby.Data.TryGetValue(KeyGameState, out var stateData))
                    newState = stateData.Value;

                if (newState != _lastGameState)
                {
                    _lastGameState = newState;

                    if (newState == "Confirming" &&
                        CurrentLobby.Data != null &&
                        CurrentLobby.Data.TryGetValue(KeyGameSeed, out var seedData) &&
                        int.TryParse(seedData.Value, out int seed))
                    {
                        // Confirming 遷移時はシードとプレイヤーインデックスを確定
                        RoomConfig.GameSeed = seed;
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (players[i].Id == RoomConfig.LocalPlayerId)
                            {
                                RoomConfig.PlayerIndex = i;
                                break;
                            }
                        }
                    }
                    else if (newState == "Playing" && CurrentLobby.Data != null)
                    {
                        // Playing 遷移時: ラウンドデータ一式を更新
                        if (CurrentLobby.Data.TryGetValue(KeyAnswererIdx, out var aIdx) &&
                            int.TryParse(aIdx.Value, out int ai))
                            RoomConfig.AnswererIndex = ai;
                        if (CurrentLobby.Data.TryGetValue(KeyDeciderIdx, out var dIdx) &&
                            int.TryParse(dIdx.Value, out int di))
                            RoomConfig.DeciderIndex = di;
                        if (CurrentLobby.Data.TryGetValue(KeyGameTopic, out var topicData))
                            RoomConfig.GameTopic = topicData.Value;
                        if (CurrentLobby.Data.TryGetValue(KeyRound, out var rData) &&
                            int.TryParse(rData.Value, out int round))
                            RoomConfig.CurrentRound = round;
                        if (CurrentLobby.Data.TryGetValue(KeyLives, out var lData) &&
                            int.TryParse(lData.Value, out int lives))
                            RoomConfig.Lives = lives;
                        if (CurrentLobby.Data.TryGetValue(KeyHelpCards, out var hData) &&
                            int.TryParse(hData.Value, out int helps))
                            RoomConfig.HelpCards = helps;
                        if (CurrentLobby.Data.TryGetValue(KeyDeciderOrder, out var oData))
                        {
                            var parts = oData.Value.Split(',');
                            var order = new int[parts.Length];
                            for (int i2 = 0; i2 < parts.Length; i2++)
                                int.TryParse(parts[i2].Trim(), out order[i2]);
                            RoomConfig.DeciderOrder = order;
                            RoomConfig.TotalRounds  = order.Length;
                        }
                        RoomConfig.HelpCardUsedThisRound = false;
                    }
                    else if (newState == "Result" && CurrentLobby.Data != null)
                    {
                        // Result 遷移時: 最終決定された秘密の数字を通知
                        if (CurrentLobby.Data.TryGetValue(KeyFinalNumber, out var finalNum) &&
                            int.TryParse(finalNum.Value, out int finalNumber))
                        {
                            RoomConfig.FinalConfirmedNumber = finalNumber;
                            OnGameFinalized?.Invoke(finalNumber);
                        }
                    }
                    else if (newState == "GameClear") { OnGameClear?.Invoke(); }
                    else if (newState == "GameOver")  { OnGameOver?.Invoke(); }
                    OnGameStateChanged?.Invoke(newState);
                }

                // ── 全員 Ready チェック ────────────────────────────────────
                if (newState == "Confirming" && players.Count > 0)
                {
                    bool allReady = true;
                    foreach (var p in players)
                    {
                        if (p.Data == null ||
                            !p.Data.TryGetValue(KeyReady, out var r) ||
                            r.Value != "1")
                        { allReady = false; break; }
                    }
                    if (allReady) OnAllPlayersReady?.Invoke();
                }
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
