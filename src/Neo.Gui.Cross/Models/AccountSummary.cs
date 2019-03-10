

namespace Neo.Gui.Cross.Models
{
    public class AccountSummary
    {
        public string Label { get; set; }

        public string Address { get; set; }

        public AccountType Type { get; set; }

        public uint NeoBalance { get; set; }

        public double GasBalance { get; set; }
    }
}
