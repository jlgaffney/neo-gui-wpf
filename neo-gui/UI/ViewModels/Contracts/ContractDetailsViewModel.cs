using System.Windows.Input;
using Neo.UI.MVVM;
using Neo.Wallets;

namespace Neo.UI.ViewModels.Contracts
{
    public class ContractDetailsViewModel : ViewModelBase
    {
        public string Address { get; private set; }

        public string ScriptHash { get; private set; }

        public string RedeemScript { get; private set; }

        public ICommand CloseCommand => new RelayCommand(this.TryClose);

        public void SetContract(VerificationContract contract)
        {
            if (contract == null)
            {
                this.Address = null;
                this.ScriptHash = null;
                this.RedeemScript = null;
                return;
            }
            
            this.Address = Wallet.ToAddress(contract.ScriptHash);
            this.ScriptHash = contract.ScriptHash.ToString();
            this.RedeemScript = contract.Script.ToHexString();
        }
    }
}