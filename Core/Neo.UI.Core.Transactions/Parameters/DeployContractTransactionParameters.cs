namespace Neo.UI.Core.Transactions.Parameters
{
    public class DeployContractTransactionParameters : TransactionParameters
    {
        public string Name { get; }

        public string Version { get; }

        public string Author { get; }

        public string Email { get; }

        public string Description { get; }

        public string ScriptHex { get; }

        public string ParameterList { get; }

        public string ReturnType { get; }

        public bool NeedsStorage { get; }

        public bool NeedsDynamicCall { get; }

        public DeployContractTransactionParameters(
            string name,
            string version, 
            string author, 
            string email, 
            string description,
            string scriptHex,
            string parameterList,
            string returnType,
            bool needsStorage,
            bool needsDynamicCall)
        {
            this.Name = name;
            this.Version = version;
            this.Author = author;
            this.Email = email;
            this.Description = description;

            this.ScriptHex = scriptHex;
            this.ParameterList = parameterList;
            this.ReturnType = returnType;
            this.NeedsStorage = needsStorage;
            this.NeedsDynamicCall = needsDynamicCall;
        }
    }
}
