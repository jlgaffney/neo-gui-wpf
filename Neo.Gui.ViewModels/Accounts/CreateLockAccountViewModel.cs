using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Accounts
{
    public class CreateLockAccountViewModel : ViewModelBase, IDialogViewModel<CreateLockAccountDialogResult>
    {
        private const int HoursInDay = 24;
        private const int MinutesInHour = 60;

        private readonly IDialogManager dialogManager;
        private readonly IMessagePublisher messagePublisher;

        private KeyPair selectedKeyPair;
        private DateTime unlockDate;
        private int unlockHour;
        private int unlockMinute;

        public CreateLockAccountViewModel(
            IDialogManager dialogManager,
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.dialogManager = dialogManager;
            this.messagePublisher = messagePublisher;

            this.KeyPairs = new ObservableCollection<KeyPair>(
                walletController.GetStandardAccounts()
                    .Select(p => p.GetKey()).ToArray());

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

        public ObservableCollection<KeyPair> KeyPairs { get; }

        public KeyPair SelectedKeyPair
        {
            get => this.selectedKeyPair;
            set
            {
                if (Equals(this.selectedKeyPair, value)) return;

                this.selectedKeyPair = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.CreateEnabled));
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

        public bool CreateEnabled => this.SelectedKeyPair != null;

        public ICommand CreateCommand => new RelayCommand(this.Create);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<CreateLockAccountDialogResult> SetDialogResultAndClose;

        public CreateLockAccountDialogResult DialogResult { get; private set; }
        #endregion

        private void Create()
        {
            var contract = this.GenerateContract();

            if (contract == null) return;

            this.messagePublisher.Publish(new AddContractMessage(contract));

            this.Close(this, EventArgs.Empty);
        }

        private Contract GenerateContract()
        {
            if (this.SelectedKeyPair == null) return null;

            var publicKey = this.SelectedKeyPair.PublicKey;

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
                    return Contract.Create(new[] {ContractParameterType.Signature}, sb.ToArray());
                }
                catch
                {
                    this.dialogManager.ShowMessageDialog(string.Empty, Strings.AddContractFailedMessage);
                    return null;
                }
            }
        }
    }
}