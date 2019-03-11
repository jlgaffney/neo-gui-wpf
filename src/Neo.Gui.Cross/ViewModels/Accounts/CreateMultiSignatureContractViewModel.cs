using System;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class CreateMultiSignatureContractViewModel : ViewModelBase
    {
        private readonly IAccountService accountService;

        private int minimumSignatureNumber;
        private int minimumSignatureNumberMaxValue;

        private string selectedPublicKey;

        private string newPublicKey;

        public CreateMultiSignatureContractViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;

            PublicKeys = new ObservableCollection<string>();
        }
        
        public int MinimumSignatureNumber
        {
            get => minimumSignatureNumber;
            set
            {
                if (Equals(minimumSignatureNumber, value))
                {
                    return;
                }

                minimumSignatureNumber = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(CreateEnabled));
            }
        }

        public int MinimumSignatureNumberMaxValue
        {
            get => minimumSignatureNumberMaxValue;
            set
            {
                if (Equals(minimumSignatureNumberMaxValue, value))
                {
                    return;
                }

                minimumSignatureNumberMaxValue = value;

                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> PublicKeys { get; }

        public string SelectedPublicKey
        {
            get => selectedPublicKey;
            set
            {
                if (Equals(selectedPublicKey, value))
                {
                    return;
                }

                selectedPublicKey = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RemoveSelectedPublicKeyEnabled));
            }
        }

        public string NewPublicKey
        {
            get => newPublicKey;
            set
            {
                if (Equals(newPublicKey, value))
                {
                    return;
                }

                newPublicKey = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(AddPublicKeyEnabled));
            }
        }

        public bool AddPublicKeyEnabled => !string.IsNullOrEmpty(NewPublicKey);

        public bool RemoveSelectedPublicKeyEnabled => SelectedPublicKey != null;

        public bool CreateEnabled => MinimumSignatureNumber > 0;

        public ReactiveCommand AddPublicKeyCommand => ReactiveCommand.Create(AddPublicKey);

        public ReactiveCommand RemoveSelectedPublicKeyCommand => ReactiveCommand.Create(RemoveSelectedPublicKey);

        public ReactiveCommand CreateCommand => ReactiveCommand.Create(Create);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);
        
        private void Create()
        {
            if (!CreateEnabled)
            {
                return;
            }

            var account = accountService.CreateMultiSignatureContractAccount(minimumSignatureNumber, PublicKeys);

            if (account == null)
            {
                // TODO Inform user

                return;
            }

            OnClose();
        }

        private void AddPublicKey()
        {
            if (!AddPublicKeyEnabled)
            {
                return;
            }

            // Check if public key has already been added
            if (PublicKeys.Any(publicKey => publicKey.Equals(NewPublicKey, StringComparison.InvariantCultureIgnoreCase)))
            {
                // TODO Notify user
                
                return;
            }

            PublicKeys.Add(NewPublicKey);

            NewPublicKey = string.Empty;
            MinimumSignatureNumberMaxValue = PublicKeys.Count;
        }

        private void RemoveSelectedPublicKey()
        {
            if (!RemoveSelectedPublicKeyEnabled)
            {
                return;
            }

            PublicKeys.Remove(SelectedPublicKey);

            MinimumSignatureNumberMaxValue = PublicKeys.Count;
        }
    }
}
