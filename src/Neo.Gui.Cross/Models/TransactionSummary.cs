using System;

namespace Neo.Gui.Cross.Models
{
    public class TransactionSummary
    {
        public string Id { get; set; }

        public TransactionType Type { get; set; }

        public DateTime Time { get; set; }

        public uint BlockIndex { get; set; }

        public string Confirmations { get; set; }
    }
}
