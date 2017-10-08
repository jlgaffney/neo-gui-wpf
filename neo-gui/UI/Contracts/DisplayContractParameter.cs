using Neo.SmartContract;

namespace Neo.UI.Contracts
{
    public class DisplayContractParameter
    {
        private readonly int index;
        private readonly ContractParameter parameter;

        public DisplayContractParameter(int index, ContractParameter parameter)
        {
            this.index = index;
            this.parameter = parameter;
        }

        public string IndexStr => "[" + this.index + "]";

        public string Type => this.parameter?.Type.ToString();

        public string Value => this.parameter?.ToString();

        public ContractParameter Parameter => this.parameter;

        public int Index => this.index;
    }
}