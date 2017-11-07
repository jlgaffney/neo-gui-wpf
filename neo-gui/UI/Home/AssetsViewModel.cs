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
using Neo.UI.Base.Helpers;
using Neo.UI.Base.MVVM;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AssetsViewModel : ViewModelBase
    {
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private AssetItem selectedAsset;

        public AssetsViewModel()
        {
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

            TransactionHelper.SignAndShowInformation(transaction);
        }

        #endregion Menu Command Methods

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
    }
}