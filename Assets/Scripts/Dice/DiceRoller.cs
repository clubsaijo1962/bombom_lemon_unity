using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BomBomLemon.Dice
{
    public class DiceRoller : MonoBehaviour
    {
        [SerializeField] private float animationDuration = 0.8f;
        [SerializeField] private float rollInterval = 0.08f;

        public UnityEvent<int> OnRollComplete = new();

        public int Roll(int faces = 6)
        {
            return UnityEngine.Random.Range(1, faces + 1);
        }

        public IEnumerator RollAnimation(Action<int> onComplete, int faces = 6)
        {
            float elapsed = 0f;
            int displayValue = 1;

            while (elapsed < animationDuration)
            {
                displayValue = Roll(faces);
                OnRollComplete.Invoke(displayValue);
                elapsed += rollInterval;
                yield return new WaitForSeconds(rollInterval);
            }

            int finalResult = Roll(faces);
            OnRollComplete.Invoke(finalResult);
            onComplete?.Invoke(finalResult);
        }
    }
}
