using System;

namespace PhoNho.Domain.Economy
{
    [Serializable]
    public class LedgerEntry
    {
        public string id;
        public string timestamp;
        public string idempotencyKey;
        public CurrencyType currency;
        public int amount;
        public int balanceAfter;
        public string reason;

        public LedgerEntry() { }

        public LedgerEntry(string id, string idempotencyKey, CurrencyType currency, int amount, int balanceAfter, string reason)
        {
            this.id = id;
            this.timestamp = DateTime.UtcNow.ToString("o");
            this.idempotencyKey = idempotencyKey;
            this.currency = currency;
            this.amount = amount;
            this.balanceAfter = balanceAfter;
            this.reason = reason;
        }
    }
}
