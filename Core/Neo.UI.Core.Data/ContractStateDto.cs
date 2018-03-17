using System.Collections.Generic;
using System.Linq;
using Neo.SmartContract;

namespace Neo.UI.Core.Data
{
    public class ContractStateDto
    {
        public string ScriptHash { get; set; }

        public byte[] Script { get; set; }

        public ContractParameterType[] Parameters { get; set; }

        public ContractParameterType ReturnType { get; set; }

        public bool HasStorage { get; set; }

        public string Name { get; set; }

        public string CodeVersion { get; set; }

        public string Author { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public ContractStateDto(string scriptHash, byte[] script, IEnumerable<ContractParameterType> parameters, ContractParameterType returnType, bool hasStorage, string name, string codeVersion, string author, string email, string description)
        {
            this.ScriptHash = scriptHash;
            this.Script = script;
            this.Parameters = parameters.ToArray();
            this.ReturnType = returnType;
            this.HasStorage = hasStorage;
            this.Name = name;
            this.CodeVersion = codeVersion;
            this.Author = author;
            this.Email = email;
            this.Description = description;
        }
    }
}
