using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.MVVM;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public class CreateLockAccountViewModel : ViewModelBase
    {
        private const int HoursInDay = 24;
        private const int MinutesInHour = 60;

        private ECPoint selectedAccount;
        private DateTime unlockDate;
        private int unlockHour;
        private int unlockMinute;

        private VerificationContract contract;

        public CreateLockAccountViewModel()
        {
            this.Accounts = new ObservableCollection<ECPoint>(App.CurrentWallet.GetContracts().Where(p => p.IsStandard).Select(p => App.CurrentWallet.GetKey(p.PublicKeyHash).PublicKey).ToArray());

            this.Hours = new List<int>();

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

        public ObservableCollection<ECPoint> Accounts { get; }

        public ECPoint SelectedAccount
        {
            get => this.selectedAccount;
            set
            {
                if (Equals(this.selectedAccount, value)) return;

                this.selectedAccount = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.CreateEnabled));
            }
        }

        public List<int> Hours { get; }

        public List<int> Minutes { get; }

        public DateTime MinimumDate { get; }

        public DateTime UnlockDate
        {
            get => this.unlockDate;
            set
            {
                if (this.unlockDate == value) return;

                this.unlockDate = value;

                NotifyPropertyChanged();
            }
        }

        public int UnlockHour
        {
            get => this.unlockHour;
            set
            {
                if (this.unlockHour == value) return;

                this.unlockHour = value;

                NotifyPropertyChanged();
            }
        }

        public int UnlockMinute
        {
            get => this.unlockMinute;
            set
            {
                if (this.unlockMinute == value) return;

                this.unlockMinute = value;

                NotifyPropertyChanged();
            }
        }

        public bool CreateEnabled => this.SelectedAccount != null;

        public ICommand CreateCommand => new RelayCommand(this.Create);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);


        private void Create()
        {
            this.contract = this.GenerateContract();

            if (contract == null) return;

            this.TryClose();
        }

        public VerificationContract GetContract()
        {
            return this.contract;
        }

        private VerificationContract GenerateContract()
        {
            if (this.SelectedAccount == null) return null;

            var publicKey = this.SelectedAccount;

            // Combine unlock date and time
            var unlockDateTime = this.UnlockDate.Date
                .AddHours(this.UnlockHour)
                .AddMinutes(this.UnlockMinute);

            var timestamp = unlockDateTime.ToTimestamp();

            using (var sb = new ScriptBuilder())
            {
                sb.EmitPush(publicKey);
                sb.EmitPush(timestamp);
                // Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                sb.EmitAppCall(UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4"));

                try
                {
                    return VerificationContract.Create(publicKey.EncodePoint(true).ToScriptHash(),
                        new[] {ContractParameterType.Signature}, sb.ToArray());
                }
                catch
                {
                    MessageBox.Show(Strings.AddContractFailedMessage);
                    return null;
                }
            }
        }
    }
}