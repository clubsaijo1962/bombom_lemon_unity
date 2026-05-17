using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using BomBomLemon.Dice;

namespace BomBomLemon.Core
{
    public enum TurnPhase
    {
        WaitingForRoll,
        Rolling,
        Moving,
        EventTriggering,
        Done,
    }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private DiceRoller diceRoller;

        public TurnPhase CurrentPhase { get; private set; } = TurnPhase.WaitingForRoll;
        public int LastDiceResult { get; private set; }

        public UnityEvent<TurnPhase> OnPhaseChanged = new();

        void SetPhase(TurnPhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged.Invoke(phase);
        }

        public void StartTurn()
        {
            SetPhase(TurnPhase.WaitingForRoll);
        }

        public void RollDice()
        {
            if (CurrentPhase != TurnPhase.WaitingForRoll) return;
            SetPhase(TurnPhase.Rolling);
            StartCoroutine(diceRoller.RollAnimation(OnDiceResult));
        }

        void OnDiceResult(int result)
        {
            LastDiceResult = result;
            SetPhase(TurnPhase.Moving);
        }

        public void OnMoveComplete()
        {
            SetPhase(TurnPhase.EventTriggering);
        }

        public void EndTurn()
        {
            SetPhase(TurnPhase.Done);
            GameManager.Instance.NextTurn();
        }
    }
}
