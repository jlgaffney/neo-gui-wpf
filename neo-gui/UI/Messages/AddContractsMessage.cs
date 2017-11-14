using System.Collections.Generic;
using Neo.Wallets;

namespace Neo.UI.Messages
{
    public class AddContractsMessage
    {
        #region Public Properties 
        public IEnumerable<VerificationContract> Contracts { get; }
        #endregion

        #region Constructor 
        public AddContractsMessage(IEnumerable<VerificationContract> contracts)
        {
            this.Contracts = contracts;
        }
        #endregion
    }
}
