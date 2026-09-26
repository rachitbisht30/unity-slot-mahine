using System;
using UnityEngine;
using UnityEngine.UI;
using SlotMachine.Core;

namespace SlotMachine.Controllers
{
    /// <summary>
    /// UI Controller handling screen updates, button inputs, paytable modals, and balance counters.
    /// Uses global::UnityEngine.UI explicit qualifiers to prevent namespace collision with local UI folders.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SlotMachineManager _slotManager;

        [Header("Text Displays")]
        [SerializeField] private global::UnityEngine.UI.Text _balanceText;
        [SerializeField] private global::UnityEngine.UI.Text _betText;
        [SerializeField] private global::UnityEngine.UI.Text _winText;
        [SerializeField] private global::UnityEngine.UI.Text _freeSpinsText;

        [Header("Buttons")]
        [SerializeField] private global::UnityEngine.UI.Button _spinButton;
        [SerializeField] private global::UnityEngine.UI.Button _maxBetButton;
        [SerializeField] private global::UnityEngine.UI.Button _betPlusButton;
        [SerializeField] private global::UnityEngine.UI.Button _betMinusButton;
        [SerializeField] private global::UnityEngine.UI.Button _autoSpinButton;
        [SerializeField] private global::UnityEngine.UI.Button _paytableButton;

        [Header("Modals & Banners")]
        [SerializeField] private GameObject _paytableModal;
        [SerializeField] private GameObject _freeSpinsBanner;
        [SerializeField] private GameObject _winBanner;

        private void OnEnable()
        {
            if (_slotManager != null)
            {
                _slotManager.OnBalanceChanged += UpdateBalance;
                _slotManager.OnBetChanged += UpdateBet;
                _slotManager.OnSpinCompleted += HandleSpinResult;
                _slotManager.OnFreeSpinsUpdated += UpdateFreeSpins;
            }
        }

        private void OnDisable()
        {
            if (_slotManager != null)
            {
                _slotManager.OnBalanceChanged -= UpdateBalance;
                _slotManager.OnBetChanged -= UpdateBet;
                _slotManager.OnSpinCompleted -= HandleSpinResult;
                _slotManager.OnFreeSpinsUpdated -= UpdateFreeSpins;
            }
        }

        private void Start()
        {
            if (_slotManager != null)
            {
                UpdateBalance(_slotManager.Balance);
                UpdateBet(_slotManager.CurrentBet);
            }

            _spinButton?.onClick.AddListener(OnSpinClicked);
            _maxBetButton?.onClick.AddListener(OnMaxBetClicked);
            _betPlusButton?.onClick.AddListener(() => _slotManager.ChangeBet(10f));
            _betMinusButton?.onClick.AddListener(() => _slotManager.ChangeBet(-10f));
            _autoSpinButton?.onClick.AddListener(OnAutoSpinClicked);
            _paytableButton?.onClick.AddListener(TogglePaytable);
        }

        public void OnSpinClicked()
        {
            _slotManager?.TriggerSpin();
        }

        public void OnMaxBetClicked()
        {
            _slotManager?.SetMaxBet();
        }

        public void OnAutoSpinClicked()
        {
            _slotManager?.ToggleAutoSpin();
        }

        public void TogglePaytable()
        {
            if (_paytableModal != null)
            {
                _paytableModal.SetActive(!_paytableModal.activeSelf);
            }
        }

        private void UpdateBalance(float newBalance)
        {
            if (_balanceText != null)
                _balanceText.text = $"${newBalance:F2}";
        }

        private void UpdateBet(float newBet)
        {
            if (_betText != null)
                _betText.text = $"${newBet:F2}";
        }

        private void UpdateFreeSpins(int count)
        {
            if (_freeSpinsBanner != null)
                _freeSpinsBanner.SetActive(count > 0);

            if (_freeSpinsText != null)
                _freeSpinsText.text = $"Free Spins: {count}";
        }

        private void HandleSpinResult(SpinResult result)
        {
            if (_winText != null)
            {
                if (result.totalPayout > 0)
                {
                    _winText.text = $"WIN: ${result.totalPayout:F2}!";
                    if (_winBanner != null) _winBanner.SetActive(true);
                }
                else
                {
                    _winText.text = "";
                    if (_winBanner != null) _winBanner.SetActive(false);
                }
            }
        }
    }
}
