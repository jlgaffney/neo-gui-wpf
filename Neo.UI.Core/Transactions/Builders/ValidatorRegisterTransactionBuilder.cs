using System;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    public class ValidatorRegisterTransactionBuilder : ITransactionBuilder<ElectionTransactionParameters>
    {
        private const string ValidatorRegisterApi = "Neo.Validator.Register";

        public Transaction Build(ElectionTransactionParameters parameters)
        {
            var validatorPublicKeyECPoint = ECPoint.Parse(parameters.ValidatorPublicKey, ECCurve.Secp256k1);
            
            var stateTransaction = new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Validator,
                        Key = validatorPublicKeyECPoint.ToArray(),
                        Field = "Registered",
                        Value = BitConverter.GetBytes(true)
                    }
                }
            };

            return stateTransaction;
        }
    }
}
