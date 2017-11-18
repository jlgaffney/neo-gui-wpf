using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Collections;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AssetsViewModel : ViewModelBase
    {
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IMessagePublisher messagePublisher;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private AssetItem selectedAsset;

        public AssetsViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();

            this.Assets = new ConcurrentObservableCollection<AssetItem>();
        }

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

                var queryResult = GetCertificateQueryResult(this.SelectedAsset.State);

                if (queryResult == null) return false;
                
                return queryResult.Type == CertificateQueryResultType.Good ||
                       queryResult.Type == CertificateQueryResultType.Expired ||
                       queryResult.Type == CertificateQueryResultType.Invalid;
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

        #region Menu Command Methods

        private void ViewCertificate()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var hash = Contract.CreateSignatureRedeemScript(this.SelectedAsset.State.Owner).ToScriptHash();
            var address = Wallet.ToAddress(hash);
            var path = Path.Combine(Settings.Default.CertCachePath, $"{address}.cer");
            Process.Start(path);
        }

        private void DeleteAsset()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var value = ApplicationContext.Instance.CurrentWallet.GetAvailable(this.SelectedAsset.State.AssetId);

            if (MessageBox.Show($"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.State.GetName()}:{value}")}",
                    Strings.DeleteConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var transaction = ApplicationContext.Instance.CurrentWallet.MakeTransaction(new ContractTransaction
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

        private void ViewSelectedAssetDetails()
        {
            if (this.SelectedAsset == null) return;

            var url = string.Format(Settings.Default.Urls.AssetUrl, this.SelectedAsset.Name.Substring(2));

            Process.Start(url);
        }

        internal CertificateQueryResult GetCertificateQueryResult(AssetState asset)
        {
            CertificateQueryResult result;
            if (asset.AssetType == AssetType.GoverningToken || asset.AssetType == AssetType.UtilityToken)
            {
                result = new CertificateQueryResult { Type = CertificateQueryResultType.System };
            }
            else
            {
                if (!this.certificateQueryResultCache.ContainsKey(asset.Owner))
                {
                    result = CertificateQueryService.Query(asset.Owner);

                    if (result == null) return null;

                    // Cache query result
                    this.certificateQueryResultCache.Add(asset.Owner, result);
                }

                result = this.certificateQueryResultCache[asset.Owner];
            }

            return result;
        }

        public AssetItem GetAsset(UInt160 scriptHashNEP5)
        {
            if (scriptHashNEP5 == null) return null;

            return this.Assets.FirstOrDefault(a => a.ScriptHashNEP5 != null && a.ScriptHashNEP5.Equals(scriptHashNEP5));
        }

        public AssetItem GetAsset(UInt256 assetId)
        {
            if (assetId == null) return null;

            return this.Assets.FirstOrDefault(a => a.State != null && a.State.AssetId != null && a.State.AssetId.Equals(assetId));
        }
    }
}