using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BomBomLemon.Game.Card;
using BomBomLemon.Game.Topics;
using BomBomLemon.Player;

namespace BomBomLemon.Game
{
    /// <summary>
    /// ゲーム画面 (Game シーン) の UI を制御する。
    /// ITOGameManager のイベントを購読して画面を更新する。
    /// </summary>
    public class GameSceneUI : MonoBehaviour
    {
        [Header("お題表示")]
        [SerializeField] private TextMeshProUGUI topicText;
        [SerializeField] private TextMeshProUGUI topicLowLabel;
        [SerializeField] private TextMeshProUGUI topicHighLabel;

        [Header("自分のカード")]
        [SerializeField] private TextMeshProUGUI myCardNumberText;
        [SerializeField] private GameObject myCardPanel;

        [Header("予想入力")]
        [SerializeField] private GameObject guessPanel;
        [SerializeField] private TextMeshProUGUI guessingTargetLabel;
        [SerializeField] private Slider guessSlider;
        [SerializeField] private TextMeshProUGUI guessValueText;
        [SerializeField] private Button submitGuessButton;

        [Header("結果表示")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button nextRoundButton;

        [Header("フェーズ表示")]
        [SerializeField] private TextMeshProUGUI phaseLabel;
        [SerializeField] private TextMeshProUGUI roundLabel;

        private int _currentGuessingTarget = -1;
        private int _localPlayerIndex = 0;

        void Start()
        {
            var gm = ITOGameManager.Instance;
            if (gm == null) return;

            gm.OnPhaseChanged.AddListener(OnPhaseChanged);
            gm.OnTopicRevealed.AddListener(OnTopicRevealed);
            gm.OnCardsDealt.AddListener(OnCardsDealt);
            gm.OnRoundScoresCalculated.AddListener(OnRoundScoresCalculated);

            if (guessSlider)
            {
                guessSlider.minValue = 1;
                guessSlider.maxValue = 99;
                guessSlider.wholeNumbers = true;
                guessSlider.onValueChanged.AddListener(v => {
                    if (guessValueText) guessValueText.text = ((int)v).ToString();
                });
            }

            submitGuessButton?.onClick.AddListener(OnSubmitGuess);
            nextRoundButton?.onClick.AddListener(OnNextRound);

            SetAllPanelsHidden();
            if (roundLabel) roundLabel.text = $"ラウンド {gm.CurrentRound}";
        }

        void SetAllPanelsHidden()
        {
            if (myCardPanel) myCardPanel.SetActive(false);
            if (guessPanel) guessPanel.SetActive(false);
            if (resultPanel) resultPanel.SetActive(false);
        }

        void OnPhaseChanged(ITOGamePhase phase)
        {
            SetAllPanelsHidden();

            if (phaseLabel) phaseLabel.text = PhaseToJapanese(phase);

            switch (phase)
            {
                case ITOGamePhase.TopicReveal:
                    // お題表示は OnTopicRevealed で行う
                    break;

                case ITOGamePhase.HintPhase:
                    if (myCardPanel) myCardPanel.SetActive(true);
                    break;

                case ITOGamePhase.GuessingPhase:
                    StartGuessing();
                    break;

                case ITOGamePhase.RevealPhase:
                    break;

                case ITOGamePhase.RoundEnd:
                    if (resultPanel) resultPanel.SetActive(true);
                    break;
            }
        }

        void OnTopicRevealed(Topic topic)
        {
            if (topicText) topicText.text = topic.Text;
            if (topicLowLabel) topicLowLabel.text = $"1 = {topic.LowLabel}";
            if (topicHighLabel) topicHighLabel.text = $"99 = {topic.HighLabel}";
        }

        void OnCardsDealt(List<NumberCard> cards)
        {
            var myCard = cards.Find(c => c.OwnerIndex == _localPlayerIndex);
            if (myCard != null && myCardNumberText)
                myCardNumberText.text = myCard.Value.ToString();
        }

        void StartGuessing()
        {
            var gm = ITOGameManager.Instance;
            if (gm == null) return;

            // 自分以外で最初の未予想プレイヤーを対象にする
            _currentGuessingTarget = GetNextGuessTarget();
            if (_currentGuessingTarget < 0) return;

            ShowGuessPanel(_currentGuessingTarget);
        }

        void ShowGuessPanel(int targetIndex)
        {
            var gm = ITOGameManager.Instance;
            if (gm == null || guessPanel == null) return;

            string targetName = gm.Players[targetIndex].PlayerName;
            if (guessingTargetLabel) guessingTargetLabel.text = $"{targetName} の数字は？";
            if (guessSlider) guessSlider.value = 50;
            if (guessValueText) guessValueText.text = "50";
            guessPanel.SetActive(true);
        }

        void OnSubmitGuess()
        {
            var gm = ITOGameManager.Instance;
            if (gm == null || _currentGuessingTarget < 0) return;

            int guessedValue = guessSlider ? (int)guessSlider.value : 50;
            gm.SubmitGuess(_localPlayerIndex, _currentGuessingTarget, guessedValue);

            _currentGuessingTarget = GetNextGuessTarget();
            if (_currentGuessingTarget >= 0)
            {
                ShowGuessPanel(_currentGuessingTarget);
            }
            else
            {
                if (guessPanel) guessPanel.SetActive(false);
                // 全員分の予想が揃っているか確認
                if (gm.AllGuessesSubmitted())
                    gm.BeginRevealPhase();
            }
        }

        int GetNextGuessTarget()
        {
            var gm = ITOGameManager.Instance;
            if (gm == null) return -1;

            for (int i = 0; i < gm.Players.Count; i++)
            {
                if (i == _localPlayerIndex) continue;
                if (!gm.Guesses.ContainsKey(_localPlayerIndex) ||
                    !gm.Guesses[_localPlayerIndex].ContainsKey(i))
                    return i;
            }
            return -1;
        }

        void OnRoundScoresCalculated(Dictionary<int, int> scores)
        {
            var gm = ITOGameManager.Instance;
            if (gm == null || resultText == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"ラウンド {gm.CurrentRound} 結果\n");
            foreach (var p in gm.Players)
            {
                int roundPts = scores.TryGetValue(p.PlayerIndex, out int v) ? v : 0;
                sb.AppendLine($"{p.PlayerName}: +{roundPts}pt  (累計: {p.Score}pt)");
            }
            resultText.text = sb.ToString();
        }

        void OnNextRound()
        {
            ITOGameManager.Instance?.NextRound();
            var gm = ITOGameManager.Instance;
            if (gm != null && roundLabel)
                roundLabel.text = $"ラウンド {gm.CurrentRound}";
        }

        static string PhaseToJapanese(ITOGamePhase phase) => phase switch
        {
            ITOGamePhase.TopicReveal    => "お題発表！",
            ITOGamePhase.HintPhase      => "ヒントを出そう",
            ITOGamePhase.GuessingPhase  => "数字を予想しよう",
            ITOGamePhase.RevealPhase    => "正解発表！",
            ITOGamePhase.RoundEnd       => "ラウンド終了",
            ITOGamePhase.GameEnd        => "ゲーム終了！",
            _                           => "",
        };
    }
}
