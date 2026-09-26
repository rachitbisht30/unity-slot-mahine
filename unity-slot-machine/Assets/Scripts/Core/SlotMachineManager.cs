using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMachine.Controllers;
using SlotMachine.Services;

namespace SlotMachine.Core
{
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        FreeSpins,
        BonusGame,
        ShowingWin
    }

    /// <summary>
    /// Core Game Orchestrator managing state, balance, bets, RNG service, reel execution, and payouts.
    /// </summary>
    public class SlotMachineManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private float _startingCredits = 1000f;
        [SerializeField] private float _betStep = 10f;
        [SerializeField] private float _minBet = 10f;
        [SerializeField] private float _maxBet = 500f;

        [Header("Symbol & Reel Settings")]
        [SerializeField] private List<SymbolConfig> _symbolConfigs;
        [SerializeField] private List<ReelController> _reels;

        [Header("Controllers & Handlers")]
        [SerializeField] private UIController _uiController;
        [SerializeField] private AudioController _audioController;

        // State & Services
        public GameState State { get; private set; } = GameState.Idle;
        public float Balance { get; private set; }
        public float CurrentBet { get; private set; }
        public int FreeSpinsRemaining { get; private set; } = 0;
        public float FreeSpinMultiplier { get; private set; } = 2.0f;
        public bool IsAutoSpinActive { get; private set; } = false;

        private ISlotRNGService _rngService;
        private WinCalculator _winCalculator;

        public event Action<float> OnBalanceChanged;
        public event Action<float> OnBetChanged;
        public event Action<SpinResult> OnSpinCompleted;
        public event Action<int> OnFreeSpinsUpdated;

        private void Awake()
        {
            Balance = _startingCredits;
            CurrentBet = _minBet;
            _rngService = new StandardRNGService();
            _winCalculator = new WinCalculator(_symbolConfigs);
        }

        private void Start()
        {
            OnBalanceChanged?.Invoke(Balance);
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void ChangeBet(float amount)
        {
            if (State != GameState.Idle) return;

            CurrentBet = Mathf.Clamp(CurrentBet + amount, _minBet, _maxBet);
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void SetMaxBet()
        {
            if (State != GameState.Idle) return;

            CurrentBet = _maxBet;
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void ToggleAutoSpin()
        {
            IsAutoSpinActive = !IsAutoSpinActive;
            if (IsAutoSpinActive && State == GameState.Idle)
            {
                TriggerSpin();
            }
        }

        public void TriggerSpin()
        {
            if (State != GameState.Idle && State != GameState.FreeSpins) return;

            // Check balance for normal spins
            if (FreeSpinsRemaining <= 0)
            {
                if (Balance < CurrentBet)
                {
                    Debug.LogWarning("Insufficient balance!");
                    IsAutoSpinActive = false;
                    return;
                }

                Balance -= CurrentBet;
                OnBalanceChanged?.Invoke(Balance);
            }
            else
            {
                FreeSpinsRemaining--;
                OnFreeSpinsUpdated?.Invoke(FreeSpinsRemaining);
            }

            StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            State = GameState.Spinning;
            _audioController?.PlaySpinSound();

            // 1. Generate outcome grid via RNG service
            List<int> availableIds = new List<int>();
            Dictionary<int, int> weights = new Dictionary<int, int>();

            foreach (var cfg in _symbolConfigs)
            {
                availableIds.Add(cfg.id);
                weights[cfg.id] = cfg.weight;
            }

            int[,] gridOutcome = _rngService.GenerateSpinGrid(3, 3, availableIds, weights);

            // 2. Trigger spinning state on all reels
            foreach (var reel in _reels)
            {
                reel.StartSpin();
            }

            yield return new WaitForSeconds(0.8f);

            // 3. Staggered stopping of reels (0.4s delay per reel)
            int completedCount = 0;
            for (int r = 0; r < _reels.Count; r++)
            {
                int reelIdx = r;
                int[] reelSymbols = new int[3] { gridOutcome[r, 0], gridOutcome[r, 1], gridOutcome[r, 2] };

                _reels[r].StopSpin(reelSymbols, r * 0.4f, () =>
                {
                    _audioController?.PlayReelStopSound();
                    completedCount++;
                });
            }

            // Wait for all reels to halt
            while (completedCount < _reels.Count)
            {
                yield return null;
            }

            // 4. Evaluate Grid Wins
            State = GameState.Evaluating;
            float activeMultiplier = (FreeSpinsRemaining > 0) ? FreeSpinMultiplier : 1.0f;
            SpinResult result = _winCalculator.EvaluateGrid(gridOutcome, CurrentBet, activeMultiplier);

            // Award Win Payout
            if (result.totalPayout > 0)
            {
                Balance += result.totalPayout;
                OnBalanceChanged?.Invoke(Balance);

                if (result.isJackpot)
                {
                    _audioController?.PlayJackpotSound();
                }
                else
                {
                    _audioController?.PlayWinSound();
                }
            }

            // Process Free Spin Triggers
            if (result.isFreeSpinTriggered)
            {
                FreeSpinsRemaining += 10;
                OnFreeSpinsUpdated?.Invoke(FreeSpinsRemaining);
                _audioController?.PlayBonusTriggerSound();
            }

            OnSpinCompleted?.Invoke(result);

            yield return new WaitForSeconds(1.2f);

            // 5. Handle Free Spins continuation or Auto-Spin loop
            if (FreeSpinsRemaining > 0)
            {
                TriggerSpin();
            }
            else if (IsAutoSpinActive && Balance >= CurrentBet)
            {
                TriggerSpin();
            }
            else
            {
                State = GameState.Idle;
            }
        }
    }
}
