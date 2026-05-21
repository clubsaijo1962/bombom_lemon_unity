using System.Collections.Generic;
using UnityEngine;

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

        // ── NumberConfirm 用 ─────────────────────────────────────────
        static string _currentAnswerName = "";
        static int    _secretNumber      = 0;

        public static string CurrentAnswerName
        {
            get => _currentAnswerName;
            set => _currentAnswerName = value;
        }

        public static int SecretNumber
        {
            get => _secretNumber;
            set => _secretNumber = value;
        }

        // ── GuessInput 用 ─────────────────────────────────────────────
        static string _currentGuideName  = "";
        static string _topicTextJP       = "";
        static string _topicTextEN       = "";
        static string _topicLowJP        = "";
        static string _topicLowEN        = "";
        static string _topicHighJP       = "";
        static string _topicHighEN       = "";
        static string _topicHintLowJP    = "";
        static string _topicHintLowEN    = "";
        static string _topicHintHighJP   = "";
        static string _topicHintHighEN   = "";
        static readonly HashSet<int> _usedFinalGuesserIndices = new();

        // ── 現在のライフ（ゲーム中に変動）───────────────────────────
        static int _currentLife = 0;
        public static int CurrentLife
        {
            get => _currentLife;
            set => _currentLife = value;
        }

        // ── GuessInput → ResultReveal 用 ──────────────────────────────
        static int _guessedNumber = 0;
        public static int GuessedNumber
        {
            get => _guessedNumber;
            set => _guessedNumber = value;
        }

        public static string CurrentGuideName  => _currentGuideName;
        public static string TopicTextJP       => _topicTextJP;
        public static string TopicTextEN       => _topicTextEN;
        public static string TopicLowJP        => _topicLowJP;
        public static string TopicLowEN        => _topicLowEN;
        public static string TopicHighJP       => _topicHighJP;
        public static string TopicHighEN       => _topicHighEN;
        public static string TopicHintLowJP    => _topicHintLowJP;
        public static string TopicHintLowEN    => _topicHintLowEN;
        public static string TopicHintHighJP   => _topicHintHighJP;
        public static string TopicHintHighEN   => _topicHintHighEN;

        public static void SetTopicInfo(string guideName,
            string topicJP,    string topicEN,
            string lowJP,      string lowEN,
            string highJP,     string highEN,
            string hintLowJP,  string hintLowEN,
            string hintHighJP, string hintHighEN)
        {
            _currentGuideName  = guideName;
            _topicTextJP       = topicJP;     _topicTextEN      = topicEN;
            _topicLowJP        = lowJP;        _topicLowEN       = lowEN;
            _topicHighJP       = highJP;       _topicHighEN      = highEN;
            _topicHintLowJP    = hintLowJP;    _topicHintLowEN   = hintLowEN;
            _topicHintHighJP   = hintHighJP;   _topicHintHighEN  = hintHighEN;
        }

        public static string GetNextFinalGuesserName()
        {
            if (_playerNames == null || _playerCount == 0) return "";

            var unused = new List<int>();
            for (int i = 0; i < _playerCount; i++)
                if (!_usedFinalGuesserIndices.Contains(i)) unused.Add(i);

            if (unused.Count == 0)
            {
                _usedFinalGuesserIndices.Clear();
                for (int i = 0; i < _playerCount; i++) unused.Add(i);
            }

            int idx = unused[Random.Range(0, unused.Count)];
            _usedFinalGuesserIndices.Add(idx);
            return _playerNames[idx];
        }

        public static void Set(int count, string[] names)
        {
            _playerCount = count;
            _playerNames = (string[])names.Clone();
            _usedFinalGuesserIndices.Clear();
            _currentLife = count * 4;
        }

        public static void SetRound(string answerName, int secretNumber)
        {
            _currentAnswerName = answerName;
            _secretNumber      = secretNumber;
        }
    }
}
