using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class CreateLockAccountViewModel :
        ViewModelBase,
        ILoadable
    {
        private const int HoursInDay = 24;
        private const int MinutesInHour = 60;

        private readonly IAccountService accountService;

        private string selectedPublicKey;
        private DateTime unlockDate;
        private int unlockHour;
        private int unlockMinute;

        public CreateLockAccountViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;

            PublicKeys = new ObservableCollection<string>();

            Hours = Enumerable.Range(0, HoursInDay).ToList();
            Minutes = Enumerable.Range(0, MinutesInHour).ToList();
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

                // Update dependent property
                this.RaisePropertyChanged(nameof(CreateEnabled));
            }
        }

        public IReadOnlyList<int> Hours { get; }

        public IReadOnlyList<int> Minutes { get; }

        public DateTime MinimumDate { get; private set; }

        public DateTime UnlockDate
        {
            get => unlockDate;
            set
            {
                if (unlockDate == value)
                {
                    return;
                }

                unlockDate = value;

                this.RaisePropertyChanged();
            }
        }

        public int UnlockHour
        {
            get => unlockHour;
            set
            {
                if (unlockHour == value)
                {
                    return;
                }

                unlockHour = value;

                this.RaisePropertyChanged();
            }
        }

        public int UnlockMinute
        {
            get => unlockMinute;
            set
            {
                if (unlockMinute == value)
                {
                    return;
                }

                unlockMinute = value;

                this.RaisePropertyChanged();
            }
        }

        public bool CreateEnabled => SelectedPublicKey != null;

        public ReactiveCommand CreateCommand => ReactiveCommand.Create(CreateAccount);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);
        
        public void Load()
        {
            PublicKeys.Clear();
            PublicKeys.AddRange(accountService.GetStandardAccounts().Select(account => account.GetKey().PublicKey.ToString()));
            
            var now = DateTime.UtcNow;

            MinimumDate = now.Date;
            UnlockDate = now;

            // Set unlock time
            var time = now.TimeOfDay;

            UnlockHour = time.Hours;
            UnlockMinute = time.Minutes;
        }
        
        private void CreateAccount()
        {
            if (!CreateEnabled)
            {
                return;
            }

            var unlockDateTime = UnlockDate.Date
                .AddHours(UnlockHour)
                .AddMinutes(UnlockMinute);

            var account = accountService.CreateLockContractAccount(SelectedPublicKey, unlockDateTime);

            if (account == null)
            {
                // TODO Inform user

                return;
            }

            OnClose();
        }
    }
}
