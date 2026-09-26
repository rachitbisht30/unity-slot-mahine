using System;
using System.Collections;
using UnityEngine;

namespace SlotMachine.Core
{
    /// <summary>
    /// Mini-Game Bonus Feature: Fortune Wheel mini-game triggered by bonus landing.
    /// Simulates wheel spinning and award multiplier calculation.
    /// </summary>
    public class BonusGameManager : MonoBehaviour
    {
        [Header("Bonus Wheel Configuration")]
        [SerializeField] private float[] _wheelMultipliers = new float[] { 5f, 10f, 15f, 25f, 50f, 100f };
        [SerializeField] private Transform _wheelTransform;

        public bool IsBonusActive { get; private set; } = false;

        public void SpinWheel(Action<float> onRewardAwarded)
        {
            if (IsBonusActive) return;
            StartCoroutine(SpinWheelRoutine(onRewardAwarded));
        }

        private IEnumerator SpinWheelRoutine(Action<float> onRewardAwarded)
        {
            IsBonusActive = true;
            int randomIndex = UnityEngine.Random.Range(0, _wheelMultipliers.Length);
            float targetMultiplier = _wheelMultipliers[randomIndex];

            float duration = 3.0f;
            float elapsed = 0f;
            float totalRotation = 360f * 4 + (randomIndex * (360f / _wheelMultipliers.Length));

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentAngle = Mathf.Lerp(0, totalRotation, EasingEaseOutCubic(t));
                if (_wheelTransform != null)
                {
                    _wheelTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
                }
                yield return null;
            }

            IsBonusActive = false;
            onRewardAwarded?.Invoke(targetMultiplier);
        }

        private float EasingEaseOutCubic(float x)
        {
            return 1f - Mathf.Pow(1f - x, 3f);
        }
    }
}
