namespace Neo.UI.Core.Data.TransactionParameters
{
    public class DeployContractTransactionParameters
    {
        public string ContractSourceCode { get; private set; }

        public string ParameterList { get; private set; }

        public string ReturnType { get; private set; }

        public bool NeedsStorage { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Author { get; private set; }

        public string Email { get; private set; }

        public string Description { get; private set; }

        public DeployContractTransactionParameters(
            string contactSourceCode, 
            string parameterList, 
            string returnType, 
            bool needsStorage, 
            string name,
            string version, 
            string author, 
            string email, 
            string description)
        {
            this.ContractSourceCode = ContractSourceCode;
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
