using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoNho.Domain.Economy
{
    /// <summary>
    /// Manages player wallet balances for Scoin, Gem, and Tcoin.
    /// All balance changes MUST pass through ledger transactions with an idempotency key.
    /// Direct mutation of balances is strictly prohibited according to AGENTS.md rules.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Initial Balances [CHỐT]")]
        [SerializeField] private int _initialScoin = 200; // Vốn khởi đầu 200 Scoin theo GAME_PROMPT.md
        [SerializeField] private int _initialGem = 10;
        [SerializeField] private int _initialTcoin = 0;

        [Header("Current Balances (Read Only)")]
        [SerializeField] private int _scoin;
        [SerializeField] private int _gem;
        [SerializeField] private int _tcoin;

        private readonly List<LedgerEntry> _ledger = new List<LedgerEntry>();
        private readonly HashSet<string> _processedIdempotencyKeys = new HashSet<string>();

        public event Action<CurrencyType, int, int> OnBalanceChanged; // currency, newBalance, delta

        public int Scoin
        {
            get
            {
                EnsureInitialized();
                return _scoin;
            }
        }

        public int Gem
        {
            get
            {
                EnsureInitialized();
                return _gem;
            }
        }

        public int Tcoin
        {
            get
            {
                EnsureInitialized();
                return _tcoin;
            }
        }

        public IReadOnlyList<LedgerEntry> Ledger
        {
            get
            {
                EnsureInitialized();
                return _ledger;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_ledger.Count == 0)
            {
                InitializeWallet();
            }
        }

        public void InitializeWallet()
        {
            if (_ledger.Count == 0)
            {
                _scoin = 0;
                _gem = 0;
                _tcoin = 0;

                ApplyTransaction(CurrencyType.Scoin, _initialScoin, "Khởi tạo vốn ban đầu", "init_scoin_" + Guid.NewGuid().ToString("N"), out _);
                ApplyTransaction(CurrencyType.Gem, _initialGem, "Khởi tạo Gem ban đầu", "init_gem_" + Guid.NewGuid().ToString("N"), out _);
                if (_initialTcoin > 0)
                {
                    ApplyTransaction(CurrencyType.Tcoin, _initialTcoin, "Khởi tạo Tcoin ban đầu", "init_tcoin_" + Guid.NewGuid().ToString("N"), out _);
                }
            }
        }

        public int GetBalance(CurrencyType currency)
        {
            switch (currency)
            {
                case CurrencyType.Scoin: return _scoin;
                case CurrencyType.Gem: return _gem;
                case CurrencyType.Tcoin: return _tcoin;
                default: return 0;
            }
        }

        public bool CanAfford(CurrencyType currency, int amount)
        {
            if (amount <= 0) return true;
            return GetBalance(currency) >= amount;
        }

        /// <summary>
        /// Applies an atomic transaction to the ledger with idempotency check.
        /// </summary>
        public bool ApplyTransaction(CurrencyType currency, int delta, string reason, string idempotencyKey, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                error = "Idempotency key is required for all wallet transactions.";
                return false;
            }

            if (_processedIdempotencyKeys.Contains(idempotencyKey))
            {
                // Idempotent retry: return success without applying twice
                return true;
            }

            int currentBalance = GetBalance(currency);
            if (delta < 0 && currentBalance + delta < 0)
            {
                error = $"Không đủ {currency} (Hiện có: {currentBalance}, Cần: {-delta}).";
                return false;
            }

            int newBalance = currentBalance + delta;
            switch (currency)
            {
                case CurrencyType.Scoin:
                    _scoin = newBalance;
                    break;
                case CurrencyType.Gem:
                    _gem = newBalance;
                    break;
                case CurrencyType.Tcoin:
                    _tcoin = newBalance;
                    break;
            }

            _processedIdempotencyKeys.Add(idempotencyKey);
            var entry = new LedgerEntry(
                Guid.NewGuid().ToString("N"),
                idempotencyKey,
                currency,
                delta,
                newBalance,
                reason
            );
            _ledger.Add(entry);

            OnBalanceChanged?.Invoke(currency, newBalance, delta);
            return true;
        }
    }
}
