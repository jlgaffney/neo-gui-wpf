using System.IO;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Gui.Base.Collections;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Wpf.MVVM;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Home
{
    public class AssetsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<ClearAssetsMessage>,
        IMessageHandler<AssetAddedMessage>
    {
        #region Private Fields 
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IProcessHelper processHelper;
        private readonly IWalletController walletController;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IMessagePublisher messagePublisher;
        private AssetItem selectedAsset;
        #endregion

        #region Properties
        public ConcurrentObservableCollection<AssetItem> Assets { get; }

        public AssetItem SelectedAsset
        {
            get => this.selectedAsset;
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ViewCertificateEnabled));
                NotifyPropertyChanged(nameof(this.DeleteAssetEnabled));
            }
        }

        public bool ViewCertificateEnabled
        {
            get
            {
                if (this.SelectedAsset == null) return false;

                if (this.SelectedAsset.State == null) return false;

                return this.walletController.CanViewCertificate(this.SelectedAsset);
            }
        }

        public bool DeleteAssetEnabled => this.SelectedAsset != null &&
                                          (this.SelectedAsset.State == null ||
                                           (this.SelectedAsset.State.AssetType != AssetType.GoverningToken &&
                                            this.SelectedAsset.State.AssetType != AssetType.UtilityToken));
        #endregion Properties

        #region Commands
        public ICommand ViewCertificateCommand => new RelayCommand(this.ViewCertificate);
        public ICommand DeleteAssetCommand => new RelayCommand(this.DeleteAsset);

        public ICommand ViewSelectedAssetDetailsCommand => new RelayCommand(this.ViewSelectedAssetDetails);
        #endregion Commands

        #region Constructor 
        public AssetsViewModel(
            IWalletController walletController,
            IProcessHelper processHelper,
            IMessageSubscriber messageSubscriber,
            IMessagePublisher messagePublisher)
        {
            this.walletController = walletController;
            this.processHelper = processHelper;
            this.messageSubscriber = messageSubscriber;
            this.messagePublisher = messagePublisher;

            this.Assets = new ConcurrentObservableCollection<AssetItem>();
        }
        #endregion
        
        #region Menu Command Methods
        private void ViewCertificate()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var hash = Contract.CreateSignatureRedeemScript(this.SelectedAsset.State.Owner).ToScriptHash();
            var address = Wallet.ToAddress(hash);
            var path = Path.Combine(Properties.Settings.Default.CertCachePath, $"{address}.cer");

            this.processHelper.OpenInExternalBrowser(path);
        }

        private void DeleteAsset()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var value = this.walletController.GetAvailable(this.SelectedAsset.State.AssetId);

            if (MessageBox.Show($"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.State.GetName()}:{value}")}",
                    Strings.DeleteConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var transaction = this.walletController.MakeTransaction(new ContractTransaction
            {
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = this.SelectedAsset.State.AssetId,
                        Value = value,
                        ScriptHash = RecycleScriptHash
                    }
                }
            }, fee: Fixed8.Zero);

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
        }
        #endregion Menu Command Methods

        #region Private Methods 
        private void ViewSelectedAssetDetails()
        {
            if (this.SelectedAsset == null) return;

            var url = string.Format(Properties.Settings.Default.Urls.AssetUrl, this.SelectedAsset.Name.Substring(2));

            this.processHelper.OpenInExternalBrowser(url);
        }
        #endregion

        #region ILoadable implementation
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region IUnloadable implementation
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        #region IMessageHandler implementation

        public void HandleMessage(ClearAssetsMessage message)
        {
            this.Assets.Clear();
        }

        public void HandleMessage(AssetAddedMessage message)
        {
            this.Assets.Add(message.Asset);
        }
        #endregion
    }
}
