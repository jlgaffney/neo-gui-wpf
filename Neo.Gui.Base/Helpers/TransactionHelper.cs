using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.VM;

namespace Neo.Gui.Base.Helpers
{
    public static class TransactionHelper
    {
        public static InvocationTransaction MakeValidatorRegistrationTransaction(ECPoint publicKey)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Validator.Register", publicKey);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }

        public static InvocationTransaction MakeAssetCreationTransaction(
            AssetType? assetType,
            string assetName,
            Fixed8 amount,
            byte precision,
            ECPoint assetOwner,
            UInt160 assetAdmin,
            UInt160 assetIssuer)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Asset.Create", assetType, assetName, amount, precision, assetOwner, assetAdmin, assetIssuer);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(assetOwner).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }

        public static InvocationTransaction MakeContractCreationTransaction(
            byte[] script,
            byte[] parameterList,
            ContractParameterType returnType,
            bool needsStorage,
            string name,
            string version,
            string author,
            string email,
            string description)
        {
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Contract.Create", script, parameterList, returnType, needsStorage, name, version, author, email, description);
                return new InvocationTransaction
                {
                    Script = builder.ToArray()
                };
            }
        }
    }
}
