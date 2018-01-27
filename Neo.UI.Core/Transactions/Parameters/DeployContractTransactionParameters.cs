namespace Neo.UI.Core.Transactions.Parameters
{
    public class DeployContractTransactionParameters
    {
        public string ContractSourceCode { get; }

        public string ParameterList { get; }

        public string ReturnType { get; }

        public bool NeedsStorage { get; }

        public string Name { get; }

        public string Version { get; }

        public string Author { get; }

        public string Email { get; }

        public string Description { get; }

        public DeployContractTransactionParameters(
            string contractSourceCode, 
            string parameterList, 
            string returnType, 
            bool needsStorage, 
            string name,
            string version, 
            string author, 
            string email, 
            string description)
        {
            this.ContractSourceCode = contractSourceCode;
            this.ParameterList = parameterList;
            this.ReturnType = returnType;
            this.NeedsStorage = needsStorage;
            this.Name = name;
            this.Version = version;
            this.Author = author;
            this.Email = email;
            this.Description = description;
        }
    }
}
