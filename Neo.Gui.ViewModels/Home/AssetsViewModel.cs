using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.UI.Core.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.UI.Core.Data;
using Neo.UI.Core.Messaging.Interfaces;
using System.Linq;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.ViewModels.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Messages;

namespace Neo.Gui.ViewModels.Home
{
    public class AssetsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<AssetTotalBalanceSummaryAddedMessage>,
        IMessageHandler<AssetTotalBalanceSummaryRemovedMessage>,
        IMessageHandler<AssetTotalBalanceChangedMessage>,
        IMessageHandler<NEP5TokenTotalBalanceSummaryAddedMessage>,
        IMessageHandler<NEP5TokenTotalBalanceSummaryRemovedMessage>,
        IMessageHandler<NEP5TokenTotalBalanceChangedMessage>
    {
        #region Private Fields 
        private readonly IDialogManager dialogManager;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;
        private readonly IWalletController walletController;

        private UiAssetSummary selectedAsset;
        #endregion

        #region Public Properties
        public ObservableCollection<UiAssetSummary> Assets { get; }

        public UiAssetSummary SelectedAsset
        {
            get => this.selectedAsset;
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.ViewCertificateEnabled));
                RaisePropertyChanged(nameof(this.DeleteAssetEnabled));
            }
        }

        public bool ViewCertificateEnabled
        {
            get
            {
                if (this.SelectedAsset == null) return false;

                if (this.SelectedAsset.TokenType != TokenType.FirstClassToken) return false;

                return this.walletController.CanViewCertificate(this.SelectedAsset.AssetId);
            }
        }

        // TODO Should this also check if the user issued the asset?
        public bool DeleteAssetEnabled => 
            this.SelectedAsset != null &&
            this.SelectedAsset.TokenType == TokenType.FirstClassToken &&
            !this.SelectedAsset.IsSystemAsset;

        public RelayCommand ViewCertificateCommand => new RelayCommand(this.ViewCertificate);

        public RelayCommand DeleteAssetCommand => new RelayCommand(this.DeleteAsset);

        public RelayCommand ViewSelectedAssetDetailsCommand => new RelayCommand(this.ViewSelectedAssetDetails);
        #endregion Properties

        #region Constructor 
        public AssetsViewModel(
            IDialogManager dialogManager,
            IMessageSubscriber messageSubscriber,
            IProcessManager processManager,
            ISettingsManager settingsManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.messageSubscriber = messageSubscriber;
            this.processManager = processManager;
            this.settingsManager = settingsManager;
            this.walletController = walletController;

            this.Assets = new ObservableCollection<UiAssetSummary>();
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
        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.Assets.Clear();
        }

        public void HandleMessage(AssetTotalBalanceSummaryAddedMessage message)
        {
            Debug.Assert(!this.Assets.Any(a => a.TokenType == TokenType.FirstClassToken && a.AssetId == message.AssetId));

            var newAsset = new UiAssetSummary(message.AssetId, message.AssetName, message.AssetIssuer, message.AssetType, TokenType.FirstClassToken, message.IsSystemAsset);

            newAsset.SetAssetBalance(message.TotalBalance, message.TotalBonus);

            this.Assets.Add(newAsset);
        }

        public void HandleMessage(AssetTotalBalanceSummaryRemovedMessage message)
        {
            var asset = this.Assets.Single(a => a.TokenType == TokenType.FirstClassToken && a.AssetId == message.AssetId);

            if (asset == null) return;

            this.Assets.Remove(asset);
        }

        public void HandleMessage(AssetTotalBalanceChangedMessage message)
        {
            var asset = this.Assets.Single(a => a.TokenType == TokenType.FirstClassToken && a.AssetId == message.AssetId);

            asset.SetAssetBalance(message.TotalBalance, message.TotalBonus);
        }

        public void HandleMessage(NEP5TokenTotalBalanceSummaryAddedMessage message)
        {
            Debug.Assert(!this.Assets.Any(a => a.TokenType == TokenType.NEP5Token && a.AssetId == message.ScriptHash));

            var newAsset = new UiAssetSummary(message.ScriptHash, message.TokenName,
                $"ScriptHash:{message.ScriptHash}", "NEP-5", TokenType.NEP5Token, false)
            {
                TotalBalance = message.TotalBalance
            };

            this.Assets.Add(newAsset);
        }

        public void HandleMessage(NEP5TokenTotalBalanceSummaryRemovedMessage message)
        {
            var asset = this.Assets.Single(a => a.TokenType == TokenType.NEP5Token && a.AssetId == message.ScriptHash);

            if (asset == null) return;

            this.Assets.Remove(asset);
        }

        public void HandleMessage(NEP5TokenTotalBalanceChangedMessage message)
        {
            var asset = this.Assets.Single(a => a.TokenType == TokenType.NEP5Token && a.AssetId == message.ScriptHash);

            asset.TotalBalance = message.TotalBalance;
        }
        #endregion

        #region Private Methods 
        private void ViewSelectedAssetDetails()
        {
            if (this.SelectedAsset == null) return;

            var assetId = this.SelectedAsset.AssetId;
            if (assetId.StartsWith("0x"))
            {
                assetId = assetId.Substring(2);
            }

            var url = string.Format(this.settingsManager.AssetURLFormat, assetId);

            this.processManager.OpenInExternalBrowser(url);
        }

        private void ViewCertificate()
        {
            if (!this.ViewCertificateEnabled) return;
            
            var certificatePath = this.walletController.GetAssetCertificateFilePath(this.SelectedAsset.AssetId);

            if (string.IsNullOrEmpty(certificatePath))
            {
                // TODO Show error message
            }
            else
            {
                this.processManager.Run(certificatePath);
            }
        }

        private async void DeleteAsset()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.TokenType != TokenType.FirstClassToken) return;

            var result = this.dialogManager.ShowMessageDialog(
                Strings.DeleteConfirmation,
                $"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.Name}:{this.SelectedAsset.TotalBalance}")}",
                MessageDialogType.YesNo,
                MessageDialogResult.No);

            if (result != MessageDialogResult.Yes) return;

            await this.walletController.DeleteFirstClassAsset(this.SelectedAsset.AssetId);
        }
        #endregion
    }
}
