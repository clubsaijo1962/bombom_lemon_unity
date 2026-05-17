using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BomBomLemon.Game.Card;
using BomBomLemon.Game.Topics;
using BomBomLemon.Game.Score;
using BomBomLemon.Player;

namespace BomBomLemon.Game
{
    public enum ITOGamePhase
    {
        Setup,          // ゲーム準備中
        TopicReveal,    // お題発表
        HintPhase,      // ヒントを出す（数字を言わずに）
        GuessingPhase,  // 他プレイヤーの数字を予想
        RevealPhase,    // 正解発表
        RoundEnd,       // ラウンド終了・スコア確定
        GameEnd,        // ゲーム終了
    }

    /// <summary>
    /// ITOベースのゲーム進行を管理する。
    /// 各ラウンド: カード配布 → お題発表 → ヒント → 予想 → 正解発表 → スコア計算
    /// </summary>
    public class ITOGameManager : MonoBehaviour
    {
        public static ITOGameManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private TopicDatabase topicDatabase;
        [SerializeField] private int totalRounds = 3;

        // --- state ---
        public ITOGamePhase CurrentPhase { get; private set; } = ITOGamePhase.Setup;
        public List<PlayerData> Players { get; } = new();
        public Topic CurrentTopic { get; private set; }
        public List<NumberCard> DealtCards { get; } = new();

        // guesses[guesserIndex][targetIndex] = guessed value
        public Dictionary<int, Dictionary<int, int>> Guesses { get; } = new();
        public Dictionary<int, int> RoundScores { get; } = new();

        public int CurrentRound { get; private set; } = 1;

        // --- events ---
        public UnityEvent<ITOGamePhase> OnPhaseChanged = new();
        public UnityEvent<Topic> OnTopicRevealed = new();
        public UnityEvent<List<NumberCard>> OnCardsDealt = new();
        public UnityEvent<Dictionary<int, int>> OnRoundScoresCalculated = new();

        private readonly CardDeck _deck = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void SetPhase(ITOGamePhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged.Invoke(phase);
        }

        // ── ゲーム開始 ──────────────────────────────────────────
        public void StartGame(List<PlayerData> players)
        {
            Players.Clear();
            Players.AddRange(players);
            CurrentRound = 1;
            foreach (var p in Players) p.Score = 0;
            SetPhase(ITOGamePhase.Setup);
            StartRound();
        }

        // ── ラウンド開始 ─────────────────────────────────────────
        public void StartRound()
        {
            _deck.Reset();
            DealtCards.Clear();
            Guesses.Clear();
            RoundScores.Clear();

            // カード配布
            var cards = _deck.DealCards(Players.Count);
            DealtCards.AddRange(cards);
            OnCardsDealt.Invoke(DealtCards);

            // お題決定
            CurrentTopic = topicDatabase != null ? topicDatabase.GetRandom() : FallbackTopic();
            SetPhase(ITOGamePhase.TopicReveal);
            OnTopicRevealed.Invoke(CurrentTopic);
        }

        // ── ヒットフェーズへ ─────────────────────────────────────
        public void BeginHintPhase()
        {
            SetPhase(ITOGamePhase.HintPhase);
        }

        // ── 予想フェーズへ ───────────────────────────────────────
        public void BeginGuessingPhase()
        {
            SetPhase(ITOGamePhase.GuessingPhase);
        }

        /// <summary>
        /// プレイヤー guesserIndex が、targetIndex のカードを value だと予想する
        /// </summary>
        public void SubmitGuess(int guesserIndex, int targetIndex, int value)
        {
            if (!Guesses.ContainsKey(guesserIndex))
                Guesses[guesserIndex] = new Dictionary<int, int>();
            Guesses[guesserIndex][targetIndex] = Mathf.Clamp(value, 1, 99);

            Debug.Log($"[ITOGame] Player{guesserIndex} guessed Player{targetIndex}'s card as {value}");
        }

        /// <summary>全員の予想が揃ったら正解発表フェーズへ進む</summary>
        public bool AllGuessesSubmitted()
        {
            // 各プレイヤーが自分以外の全員分を予想しているか確認
            for (int guesser = 0; guesser < Players.Count; guesser++)
            {
                if (!Guesses.ContainsKey(guesser)) return false;
                for (int target = 0; target < Players.Count; target++)
                {
                    if (target == guesser) continue;
                    if (!Guesses[guesser].ContainsKey(target)) return false;
                }
            }
            return true;
        }

        // ── 正解発表 ─────────────────────────────────────────────
        public void BeginRevealPhase()
        {
            SetPhase(ITOGamePhase.RevealPhase);
            foreach (var card in DealtCards) card.IsRevealed = true;
            CalculateRoundScores();
        }

        void CalculateRoundScores()
        {
            // 各プレイヤーが「正確に当てられた回数」でスコアを得る
            // + 自分が正確に当てた回数でもスコアを得る（双方向）
            foreach (var p in Players) RoundScores[p.PlayerIndex] = 0;

            for (int guesser = 0; guesser < Players.Count; guesser++)
            {
                if (!Guesses.ContainsKey(guesser)) continue;
                foreach (var kvp in Guesses[guesser])
                {
                    int targetIdx = kvp.Key;
                    int guessedValue = kvp.Value;
                    int actualValue = DealtCards[targetIdx].Value;
                    int pts = ScoreCalculator.Calculate(actualValue, guessedValue);

                    // 当てた人と当てられた人の両方にポイント
                    RoundScores[guesser] += pts / 2;
                    RoundScores[targetIdx] += pts / 2;
                }
            }

            // 累計スコアに加算
            foreach (var p in Players)
            {
                if (RoundScores.TryGetValue(p.PlayerIndex, out int pts))
                    p.Score += pts;
            }

            OnRoundScoresCalculated.Invoke(RoundScores);
            SetPhase(ITOGamePhase.RoundEnd);
        }

        // ── 次のラウンドへ ───────────────────────────────────────
        public void NextRound()
        {
            CurrentRound++;
            if (CurrentRound > totalRounds)
            {
                SetPhase(ITOGamePhase.GameEnd);
            }
            else
            {
                StartRound();
            }
        }

        static Topic FallbackTopic() => new("好きなもの", "あまり好きじゃない", "大好き！");
    }
}
