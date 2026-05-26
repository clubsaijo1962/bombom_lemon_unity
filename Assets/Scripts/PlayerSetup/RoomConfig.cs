namespace BomBomLemon.PlayerSetup
{
    /// <summary>
    /// マルチプレイ部屋設定（静的ストア）。
    /// ネットワーク実装時はここからバックエンドへ渡す。
    /// </summary>
    public static class RoomConfig
    {
        public enum GameMode { CoopLife, TeamBattle }

        /// <summary>暗証番号（6桁数字文字列）</summary>
        public static string Pin { get; set; } = "";

        /// <summary>部屋を立てたプレイヤーの名前</summary>
        public static string HostName { get; set; } = "";

        /// <summary>選択ゲームモード</summary>
        public static GameMode Mode { get; set; } = GameMode.CoopLife;

        /// <summary>地獄モードフラグ（協力モード時：ライフ半分・ヘルプカード無し）</summary>
        public static bool IsHellMode { get; set; } = false;

        /// <summary>UGS Lobby の ID（入退室時に使用）</summary>
        public static string LobbyId { get; set; } = "";

        /// <summary>このデバイスの UGS プレイヤー ID</summary>
        public static string LocalPlayerId { get; set; } = "";

        /// <summary>このデバイスがホストかどうか</summary>
        public static bool IsHost { get; set; } = false;

        /// <summary>このデバイスのプレイヤー名</summary>
        public static string LocalPlayerName { get; set; } = "";

        // ── ゲームセッション ──────────────────────────────────────────────────

        /// <summary>ホストが生成した乱数シード（お題・秘密の数字の決定に使用）</summary>
        public static int GameSeed { get; set; } = 0;

        /// <summary>このデバイスのプレイヤーインデックス（ロビー参加順、0始まり）</summary>
        public static int PlayerIndex { get; set; } = -1;

        /// <summary>このプレイヤーに割り当てられた秘密の数字</summary>
        public static int MySecretNumber { get; set; } = 0;

        // ── ゲーム進行フェーズ ─────────────────────────────────────────────────

        /// <summary>回答者のプレイヤーインデックス（ゲームフェーズで設定）</summary>
        public static int AnswererIndex { get; set; } = -1;

        /// <summary>最終決定者のプレイヤーインデックス（ゲームフェーズで設定）</summary>
        public static int DeciderIndex { get; set; } = -1;

        /// <summary>このラウンドのお題（最終決定者のお題）</summary>
        public static string GameTopic { get; set; } = "";
    }
}
