using System.Collections.Generic;
using Neo.Wallets;

namespace Neo.UI.Messages
{
    public class RestoreContractsMessage
    {
        #region Public Properties 
        public IEnumerable<VerificationContract> Contracts { get; private set; }
        #endregion

        #region Constructor 
        public RestoreContractsMessage(IEnumerable<VerificationContract> contracts)
        {
            this.Contracts = contracts;
        }
        #endregion
    }
}
