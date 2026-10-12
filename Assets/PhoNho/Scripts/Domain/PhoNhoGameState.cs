using System;
using System.Collections.Generic;
using PhoNho.Domain.Economy;
using UnityEngine;

namespace PhoNho.Domain
{
    /// <summary>
    /// Persistent across scene loads to preserve wallet balances, ledger history,
    /// inventory ingredients, and prepared dishes during seamless scene transitions.
    /// Does not require DontDestroyOnLoad game objects.
    /// </summary>
    public static class PhoNhoGameState
    {
        private static bool _hasInitializedSession = false;
        private static int _persistedScoin = 200;
        private static int _persistedGem = 10;
        private static int _persistedTcoin = 0;
        private static readonly List<LedgerEntry> _persistedLedger = new List<LedgerEntry>();
        private static readonly HashSet<string> _persistedIdempotencyKeys = new HashSet<string>();
        private static readonly Dictionary<string, int> _persistedIngredients = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> _persistedProducts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public static bool HasInitializedSession => _hasInitializedSession;
        public static int Scoin => _persistedScoin;
        public static int Gem => _persistedGem;
        public static int Tcoin => _persistedTcoin;
        public static IReadOnlyList<LedgerEntry> Ledger => _persistedLedger;
        public static IReadOnlyDictionary<string, int> Ingredients => _persistedIngredients;
        public static IReadOnlyDictionary<string, int> Products => _persistedProducts;

        public static void InitializeSession(int scoin, int gem, int tcoin)
        {
            if (_hasInitializedSession) return;
            _persistedScoin = scoin;
            _persistedGem = gem;
            _persistedTcoin = tcoin;
            _hasInitializedSession = true;
        }

        public static void SaveWalletState(int scoin, int gem, int tcoin, IEnumerable<LedgerEntry> ledgerEntries)
        {
            _hasInitializedSession = true;
            _persistedScoin = scoin;
            _persistedGem = gem;
            _persistedTcoin = tcoin;

            if (ledgerEntries != null)
            {
                _persistedLedger.Clear();
                _persistedIdempotencyKeys.Clear();
                foreach (var entry in ledgerEntries)
                {
                    _persistedLedger.Add(entry);
                    if (!string.IsNullOrEmpty(entry.idempotencyKey))
                    {
                        _persistedIdempotencyKeys.Add(entry.idempotencyKey);
                    }
                }
            }
        }

        public static void RecordTransaction(CurrencyType currency, int delta, string reason, string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey) || _persistedIdempotencyKeys.Contains(idempotencyKey))
            {
                return;
            }

            int current = currency == CurrencyType.Scoin ? _persistedScoin : (currency == CurrencyType.Gem ? _persistedGem : _persistedTcoin);
            int next = current + delta;
            if (currency == CurrencyType.Scoin) _persistedScoin = next;
            else if (currency == CurrencyType.Gem) _persistedGem = next;
            else if (currency == CurrencyType.Tcoin) _persistedTcoin = next;

            var entry = new LedgerEntry(Guid.NewGuid().ToString("N"), idempotencyKey, currency, delta, next, reason);
            _persistedLedger.Add(entry);
            _persistedIdempotencyKeys.Add(idempotencyKey);
            _hasInitializedSession = true;
        }

        public static void SaveInventoryState(IDictionary<string, int> ingredients, IDictionary<string, int> products)
        {
            if (ingredients != null)
            {
                _persistedIngredients.Clear();
                foreach (var kvp in ingredients)
                {
                    _persistedIngredients[kvp.Key] = kvp.Value;
                }
            }

            if (products != null)
            {
                _persistedProducts.Clear();
                foreach (var kvp in products)
                {
                    _persistedProducts[kvp.Key] = kvp.Value;
                }
            }
            _hasInitializedSession = true;
        }

        public static void AddProduct(string recipeId, int amount = 1)
        {
            if (string.IsNullOrEmpty(recipeId) || amount <= 0) return;
            int current = _persistedProducts.TryGetValue(recipeId, out int c) ? c : 0;
            _persistedProducts[recipeId] = current + amount;
            _hasInitializedSession = true;
        }

        public static void ResetForTesting(int initialScoin = 200, int initialGem = 10, int initialTcoin = 0)
        {
            _persistedScoin = initialScoin;
            _persistedGem = initialGem;
            _persistedTcoin = initialTcoin;
            _persistedLedger.Clear();
            _persistedIdempotencyKeys.Clear();
            _persistedIngredients.Clear();
            _persistedProducts.Clear();

            string initKey = "init_test_" + Guid.NewGuid().ToString("N");
            var entry = new LedgerEntry(Guid.NewGuid().ToString("N"), initKey, CurrencyType.Scoin, initialScoin, initialScoin, "Khởi tạo vốn ban đầu");
            _persistedLedger.Add(entry);
            _persistedIdempotencyKeys.Add(initKey);

            _hasInitializedSession = true;
        }
    }
}
