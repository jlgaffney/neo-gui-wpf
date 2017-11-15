using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Collections;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AssetsViewModel : 
        ViewModelBase, 
        IMessageHandler<UpdateAssetsBalanceMessage>,
        IMessageHandler<ClearAssetsMessage>,
        IMessageHandler<AddAssetMessage>
    {
        #region Private Fields 
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IApplicationContext applicationContext;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatcher dispatcher;
        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private AssetItem selectedAsset;
        #endregion

        public AssetsViewModel(
            IApplicationContext applicationContext,
            IMessagePublisher messagePublisher)
        {
            this.applicationContext = applicationContext;
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
        #endregion Commands

        #region Constructor 
        public AssetsViewModel(IMessagePublisher messagePublisher, IDispatcher dispatcher)
        {
            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();

            this.Assets = new ConcurrentObservableCollection<AssetItem>();
            this.messagePublisher = messagePublisher;
            this.dispatcher = dispatcher;
        }
        #endregion
        
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

            var value = this.applicationContext.CurrentWallet.GetAvailable(this.SelectedAsset.State.AssetId);

            if (MessageBox.Show($"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.State.GetName()}:{value}")}",
                    Strings.DeleteConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var transaction = this.applicationContext.CurrentWallet.MakeTransaction(new ContractTransaction
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
        private CertificateQueryResult GetCertificateQueryResult(AssetState asset)
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
        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(UpdateAssetsBalanceMessage message)
        {
            var assetList = this.Assets.ConvertToList();
            if (message.BalanceChanged)
            {
                var coins = this.applicationContext.CurrentWallet?.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                var bonusAvailable = Blockchain.CalculateBonus(this.applicationContext.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference));
                var bonusUnavailable = Blockchain.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).Select(p => p.Reference), Blockchain.Default.Height + 1);
                var bonus = bonusAvailable + bonusUnavailable;

                var assets = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                {
                    Asset = Blockchain.Default.GetAssetState(k),
                    Value = g.Sum(p => p.Output.Value),
                    Claim = k.Equals(Blockchain.UtilityToken.Hash) ? bonus : Fixed8.Zero
                }).ToDictionary(p => p.Asset.AssetId);

                if (bonus != Fixed8.Zero && !assets.ContainsKey(Blockchain.UtilityToken.Hash))
                {
                    assets[Blockchain.UtilityToken.Hash] = new
                    {
                        Asset = Blockchain.Default.GetAssetState(Blockchain.UtilityToken.Hash),
                        Value = Fixed8.Zero,
                        Claim = bonus
                    };
                }

                foreach (var asset in assetList.Where(item => item.State != null))
                {
                    if (assets.ContainsKey(asset.State.AssetId)) continue;

                    this.dispatcher.InvokeOnMainUIThread(() => this.Assets.Remove(asset));
                }

                foreach (var asset in assets.Values)
                {
                    if (asset.Asset == null || asset.Asset.AssetId == null) continue;

                    var valueText = asset.Value + (asset.Asset.AssetId.Equals(Blockchain.UtilityToken.Hash) ? $"+({asset.Claim})" : "");

                    var item = this.Assets.FirstOrDefault(a => a.State != null && a.State.AssetId.Equals(asset.Asset.AssetId));

                    if (item != null)
                    {
                        // Asset item already exists
                        item.Value = valueText;
                    }
                    else
                    {
                        // Add new asset item
                        string assetName;
                        switch (asset.Asset.AssetType)
                        {
                            case AssetType.GoverningToken:
                                assetName = "NEO";
                                break;

                            case AssetType.UtilityToken:
                                assetName = "NeoGas";
                                break;

                            default:
                                assetName = asset.Asset.GetName();
                                break;
                        }

                        var assetItem = new AssetItem
                        {
                            Name = assetName,
                            Type = asset.Asset.AssetType.ToString(),
                            Issuer = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]",
                            Value = valueText,
                            State = asset.Asset
                        };

                        /*this.Assets.Groups["unchecked"]
                        {
                            Name = asset.Asset.AssetId.ToString(),
                            Tag = asset.Asset,
                            UseItemStyleForSubItems = false
                        };*/

                        this.dispatcher.InvokeOnMainUIThread(() =>
                        {
                            this.Assets.Add(assetItem);
                        });
                    }
                }

                this.messagePublisher.Publish(new WalletBalanceChangedMessage(true));
            }


            foreach (var item in assetList)//.Groups["unchecked"].Items)
            {
                if (item.State == null) continue;

                var asset = item.State;

                var queryResult = this.GetCertificateQueryResult(asset);

                if (queryResult == null) continue;

                using (queryResult)
                {
                    switch (queryResult.Type)
                    {
                        case CertificateQueryResultType.Querying:
                        case CertificateQueryResultType.QueryFailed:
                            break;
                        case CertificateQueryResultType.System:
                            //subitem.ForeColor = Color.Green;
                            item.Issuer = Strings.SystemIssuer;
                            break;
                        case CertificateQueryResultType.Invalid:
                            //subitem.ForeColor = Color.Red;
                            item.Issuer = $"[{Strings.InvalidCertificate}][{asset.Owner}]";
                            break;
                        case CertificateQueryResultType.Expired:
                            //subitem.ForeColor = Color.Yellow;
                            item.Issuer = $"[{Strings.ExpiredCertificate}]{queryResult.Certificate.Subject}[{asset.Owner}]";
                            break;
                        case CertificateQueryResultType.Good:
                            //subitem.ForeColor = Color.Black;
                            item.Issuer = $"{queryResult.Certificate.Subject}[{asset.Owner}]";
                            break;
                    }
                    switch (queryResult.Type)
                    {
                        case CertificateQueryResultType.System:
                        case CertificateQueryResultType.Missing:
                        case CertificateQueryResultType.Invalid:
                        case CertificateQueryResultType.Expired:
                        case CertificateQueryResultType.Good:
                            //item.Group = listView2.Groups["checked"];
                            break;
                    }
                }
            }
        }

        public void HandleMessage(ClearAssetsMessage message)
        {
            this.Assets.Clear();
        }

        public void HandleMessage(AddAssetMessage message)
        {
            this.Assets.Add(message.AssetItem);
        }
        #endregion
    }
}