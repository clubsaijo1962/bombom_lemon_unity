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

        // ── ラウンド進行管理 ───────────────────────────────────────────────────

        /// <summary>現在のラウンドインデックス（0始まり）</summary>
        public static int CurrentRound { get; set; } = 0;

        /// <summary>総ラウンド数（プレイヤー人数分）</summary>
        public static int TotalRounds { get; set; } = 0;

        /// <summary>最終決定者が回る順序（インデックス配列）</summary>
        public static int[] DeciderOrder { get; set; } = System.Array.Empty<int>();

        /// <summary>共有ライフ数</summary>
        public static int Lives { get; set; } = 5;

        /// <summary>残りヘルプカード枚数</summary>
        public static int HelpCards { get; set; } = 0;

        /// <summary>このラウンドでヘルプカードが使用されたか</summary>
        public static bool HelpCardUsedThisRound { get; set; } = false;

        /// <summary>最終決定者が確定した秘密の数字</summary>
        public static int FinalConfirmedNumber { get; set; } = 0;

        // ── チームバトル ───────────────────────────────────────────────────────

        /// <summary>チームAのプレイヤーインデックス配列</summary>
        public static int[]  TeamA               { get; set; } = System.Array.Empty<int>();

        /// <summary>チームBのプレイヤーインデックス配列</summary>
        public static int[]  TeamB               { get; set; } = System.Array.Empty<int>();

        /// <summary>チームAの累積差スコア（小さい方が勝ち）</summary>
        public static int    TeamAScore          { get; set; } = 0;

        /// <summary>チームBの累積差スコア</summary>
        public static int    TeamBScore          { get; set; } = 0;

        /// <summary>このラウンドのアクティブチーム ("A" or "B")</summary>
        public static string ActiveTeam          { get; set; } = "A";

        /// <summary>奇数人数時に2回プレイするプレイヤーのインデックス（偶数時は -1）</summary>
        public static int    DoubledPlayerIndex  { get; set; } = -1;

        /// <summary>ダブルプレイヤーの2つ目のお題（TeamBattle 用）</summary>
        public static string MySecondTopic       { get; set; } = "";

        /// <summary>ダブルプレイヤーの2つ目の秘密の数字（TeamBattle 用）</summary>
        public static int    MySecondSecretNumber { get; set; } = 0;
    }
}
