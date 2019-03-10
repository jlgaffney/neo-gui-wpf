using System;
using Neo.Network.P2P.Payloads;

namespace Neo.Gui.Cross.Models
{
    public class TransactionStateDetails
    {
        public Transaction Transaction { get; set; }

        public uint BlockIndex { get; set; }

        public DateTime Time { get; set; }
    }
}
