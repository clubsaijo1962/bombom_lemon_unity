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
    }
}
