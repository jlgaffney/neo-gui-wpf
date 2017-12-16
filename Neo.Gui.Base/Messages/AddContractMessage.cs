using Neo.SmartContract;

namespace Neo.Gui.Base.Messages
{
    public class AddContractMessage
    {
        #region Public Properties 
        public Contract Contract { get; }
        #endregion

        #region Constructor 
        public AddContractMessage(Contract contract)
        {
            this.Contract = contract;
        }
        #endregion
    }
}