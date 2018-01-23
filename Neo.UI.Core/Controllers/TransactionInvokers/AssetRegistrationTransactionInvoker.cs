using System;

using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    internal class AssetRegistrationTransactionInvoker : ITransactionInvoker
    {
        #region Private Fields
        private readonly AssetRegistrationTransactionConfiguration configuration;
        #endregion

        #region Constructor 
        public AssetRegistrationTransactionInvoker(AssetRegistrationTransactionConfiguration configuration)
        {
            this.configuration = configuration;
        }
        #endregion

        #region ITransactionInvoker Implementation 
        public string GetTransactionScript()
        {
            throw new NotImplementedException();
        }

        public void Invoke()
        {
            throw new NotImplementedException();
        }

        public string TestForGasUsage()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
