using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Wpf.MVVM;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ViewContractViewModel : ViewModelBase
    {
        public string Address { get; private set; }
        public string ScriptHash { get; private set; }
        public string ParameterList { get; private set; }

        public string RedeemScriptHex { get; private set; }

        public ICommand CloseCommand => new RelayCommand(this.TryClose);

        public void SetContract(VerificationContract contract)
        {
            this.Address = contract.Address;
            this.ScriptHash = contract.ScriptHash.ToString();
            this.ParameterList = contract.ParameterList.Cast<byte>().ToArray().ToHexString();

            this.RedeemScriptHex = contract.Script.ToHexString();

            // Update properties
            NotifyPropertyChanged(nameof(this.Address));
            NotifyPropertyChanged(nameof(this.ScriptHash));
            NotifyPropertyChanged(nameof(this.ParameterList));
            NotifyPropertyChanged(nameof(this.RedeemScriptHex));
        }
    }
}