using System.Linq;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class ViewContractViewModel :
        ViewModelBase,
        ILoadable<UInt160>
    {
        private readonly IAccountService accountService;

        private string address;
        private string scriptHash;
        private string parameterList;
        private string redeemScriptHex;

        public ViewContractViewModel() { }
        public ViewContractViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public string Address
        {
            get => address;
            private set
            {
                if (Equals(address, value))
                {
                    return;
                }

                address = value;

                this.RaisePropertyChanged();
            }
        }

        public string ScriptHash
        {
            get => scriptHash;
            private set
            {
                if (Equals(scriptHash, value))
                {
                    return;
                }

                scriptHash = value;

                this.RaisePropertyChanged();
            }
        }

        public string ParameterList
        {
            get => parameterList;
            private set
            {
                if (Equals(parameterList, value))
                {
                    return;
                }

                parameterList = value;

                this.RaisePropertyChanged();
            }
        }

        public string RedeemScriptHex
        {
            get => redeemScriptHex;
            private set
            {
                if (Equals(redeemScriptHex, value))
                {
                    return;
                }

                redeemScriptHex = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand CloseCommand => ReactiveCommand.Create(OnClose);
        
        public void Load(UInt160 contractScriptHash)
        {
            if (contractScriptHash == null)
            {
                return;
            }
            
            var account = accountService.GetAccount(contractScriptHash);

            if (account == null)
            {
                // TODO Inform user somehow
                return;
            }

            Address = account.Contract.Address;
            ScriptHash = account.Contract.ScriptHash.ToString();
            ParameterList = account.Contract.ParameterList.Cast<byte>().ToArray().ToHexString();
            RedeemScriptHex = account.Contract.Script.ToHexString();
        }
    }
}
