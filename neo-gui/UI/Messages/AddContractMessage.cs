using Neo.Wallets;

namespace Neo.UI.Messages
{
    public class AddContractMessage
    {
        #region Public Properties 
        public VerificationContract Contract { get; }
        #endregion

        #region Constructor 
        public AddContractMessage(VerificationContract contract)
        {
            this.Contract = contract;
        }
        #endregion
    }
}