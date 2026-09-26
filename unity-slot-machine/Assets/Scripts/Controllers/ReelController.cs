using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMachine.Core;

namespace SlotMachine.Controllers
{
    public enum ReelState
    {
        Idle,
        Spinning,
        Slowing,
        Stopping
    }

    /// <summary>
    /// Controls a single slot reel column animation, position looping, and smooth stopping easing.
    /// </summary>
    public class ReelController : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [SerializeField] private int _reelIndex = 0;
        [SerializeField] private float _spinSpeed = 15f;
        [SerializeField] private float _symbolHeight = 2.0f;
        [SerializeField] private Transform _symbolContainer;

        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve _stopBounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public ReelState State { get; private set; } = ReelState.Idle;
        public int[] FinalSymbolIds { get; private set; }

        private List<GameObject> _symbolInstances = new List<GameObject>();
        private float _currentSpeed = 0f;
        private Action _onSpinCompleted;

        public void Initialize(List<SymbolConfig> symbolConfigs, int symbolCount = 5)
        {
            State = ReelState.Idle;
            // Clear existing if re-initializing
            foreach (Transform child in _symbolContainer != null ? _symbolContainer : transform)
            {
                Destroy(child.gameObject);
            }
            _symbolInstances.Clear();
        }

        public void StartSpin()
        {
            State = ReelState.Spinning;
            _currentSpeed = _spinSpeed;
        }

        public void StopSpin(int[] targetSymbolIds, float delaySeconds, Action onCompleted)
        {
            FinalSymbolIds = targetSymbolIds;
            _onSpinCompleted = onCompleted;
            StartCoroutine(StopRoutine(delaySeconds));
        }

        private IEnumerator StopRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            State = ReelState.Slowing;

            // Decelerate speed smoothly
            float duration = 0.6f;
            float elapsed = 0f;
            float initialSpeed = _currentSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentSpeed = Mathf.Lerp(initialSpeed, 2f, elapsed / duration);
                yield return null;
            }

            State = ReelState.Stopping;
            
            // Perform bounce alignment to target symbols
            yield return StartCoroutine(BounceToFinalPosition());

            State = ReelState.Idle;
            _onSpinCompleted?.Invoke();
        }

        private IEnumerator BounceToFinalPosition()
        {
            Vector3 startPos = transform.localPosition;
            Vector3 targetPos = new Vector3(startPos.x, Mathf.Round(startPos.y / _symbolHeight) * _symbolHeight, startPos.z);

            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = _stopBounceCurve.Evaluate(t);
                transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, curveValue);
                yield return null;
            }

            transform.localPosition = targetPos;
        }

        private void Update()
        {
            if (State == ReelState.Spinning || State == ReelState.Slowing)
            {
                // Translate reel downward continuously
                transform.Translate(Vector3.down * _currentSpeed * Time.deltaTime);

                // Check vertical loop wrap
                if (transform.localPosition.y <= -_symbolHeight * 3f)
                {
                    Vector3 pos = transform.localPosition;
                    pos.y += _symbolHeight * 3f;
                    transform.localPosition = pos;
                }
            }
        }
    }
}
