using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class CreateLockAccountViewModel : ViewModelBase, IDialogViewModel<CreateLockAccountLoadParameters>, ILoadable
    {
        #region Private Fields 
        private const int HoursInDay = 24;
        private const int MinutesInHour = 60;
        
        private readonly IWalletController walletController;

        private string selectedPublicKey;
        private DateTime unlockDate;
        private int unlockHour;
        private int unlockMinute;
        #endregion

        #region Public Properties 
        public ObservableCollection<string> PublicKeys { get; }      // TODO: this is not KeyPairs anymore but a list of PublicKeys. Need to be checked with jlgaffney

        public string SelectedPublicKey
        {
            get => this.selectedPublicKey;
            set
            {
                if (Equals(this.selectedPublicKey, value)) return;

                this.selectedPublicKey = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.CreateEnabled));
            }
        }

        public List<int> Hours { get; private set; }

        public List<int> Minutes { get; private set; }

        public DateTime MinimumDate { get; private set; }

        public DateTime UnlockDate
        {
            get => this.unlockDate;
            set
            {
                if (this.unlockDate == value) return;

                this.unlockDate = value;

                RaisePropertyChanged();
            }
        }

        public int UnlockHour
        {
            get => this.unlockHour;
            set
            {
                if (this.unlockHour == value) return;

                this.unlockHour = value;

                RaisePropertyChanged();
            }
        }

        public int UnlockMinute
        {
            get => this.unlockMinute;
            set
            {
                if (this.unlockMinute == value) return;

                this.unlockMinute = value;

                RaisePropertyChanged();
            }
        }

        public bool CreateEnabled => this.SelectedPublicKey != null;

        public RelayCommand CreateCommand => new RelayCommand(this.HandleCreateAccount);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public CreateLockAccountViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.PublicKeys = new ObservableCollection<string>();

            this.Hours = new List<int>();
            this.Minutes = new List<int>();
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(CreateLockAccountLoadParameters parameters)
        {
        }
        #endregion

        #region ILoadableImplementation
        public void OnLoad()
        {
            var accountPublicKeys = walletController.GetPublicKeysFromStandardAccounts();

            this.PublicKeys.Clear();
            foreach(var publicKey in accountPublicKeys)
            {
                this.PublicKeys.Add(publicKey);
            }

            this.Hours = Enumerable.Range(0, HoursInDay).ToList();
            this.Minutes = Enumerable.Range(0, MinutesInHour).ToList();

            var now = DateTime.UtcNow;

            this.MinimumDate = now.Date;
            this.UnlockDate = now;

            // Set unlock time
            var time = now.TimeOfDay;

            this.UnlockHour = time.Hours;
            this.UnlockMinute = time.Minutes;
        }
        #endregion

        #region Private Methods
        private void HandleCreateAccount()
        {
            if (this.SelectedPublicKey == null) return;

            var unlockDateTime = this.UnlockDate.Date
                .AddHours(this.UnlockHour)
                .AddMinutes(this.UnlockMinute)
                .ToTimestamp();

            this.walletController.AddLockContractAccount(this.SelectedPublicKey, unlockDateTime);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}