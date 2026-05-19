namespace BomBomLemon.PlayerSetup
{
    public static class SinglePlayConfig
    {
        static int      _playerCount = 2;
        static string[] _playerNames = { "プレイヤー1", "プレイヤー2" };

        public static int      PlayerCount => _playerCount;
        public static string[] PlayerNames => _playerNames;
        public static int      LifeCount   => _playerCount * 4;

        public static int HelpCardCount
        {
            get
            {
                if (_playerCount <= 2)  return 0;
                if (_playerCount <= 4)  return 1;
                if (_playerCount <= 7)  return 2;
                if (_playerCount <= 10) return 3;
                return 3 + (_playerCount - 8) / 3;
            }
        }

        public static void Set(int count, string[] names)
        {
            _playerCount = count;
            _playerNames = (string[])names.Clone();
        }
    }
}
