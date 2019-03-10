using System.Linq;
using Neo.Gui.Cross.Services;
using Neo.SmartContract;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class ImportCustomContractViewModel : ViewModelBase
    {
        private readonly IAccountService accountService;

        private string parameterList;
        private string script;

        public ImportCustomContractViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public string ParameterList
        {
            get => parameterList;
            set
            {
                if (Equals(parameterList, value))
                {
                    return;
                }

                parameterList = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(ImportEnabled));
            }
        }

        public string Script
        {
            get => script;
            set
            {
                if (Equals(script, value))
                {
                    return;
                }

                script = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(ImportEnabled));
            }
        }

        public bool ImportEnabled =>
            !string.IsNullOrEmpty(ParameterList) &&
            !string.IsNullOrEmpty(Script);

        public ReactiveCommand ImportCommand => ReactiveCommand.Create(Import);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);
        
        private void Import()
        {
            if (!ImportEnabled)
            {
                return;
            }

            var parameters = ParameterList.HexToBytes().Select(p => (ContractParameterType)p).ToArray();

            var account = accountService.CreateContractAccount(Script.HexToBytes(), parameters);

            if (account == null)
            {
                // TODO Inform user

                return;
            }

            OnClose();
        }
    }
}
