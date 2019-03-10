using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Models;
using Neo.Gui.Cross.Services;
using Neo.Ledger;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class AssetsViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletOpenedMessage>,
        IMessageHandler<WalletClosedMessage>,
        IMessageHandler<AccountBalancesChangedMessage>
    {
        private readonly IAccountBalanceService accountBalanceService;
        private readonly IAccountService accountService;
        private readonly IBlockchainService blockchainService;
        private readonly IMessageAggregator messageAggregator;
        private readonly INEP5TokenService nep5TokenService;
        private readonly IWalletService walletService;

        private AssetSummary selectedAsset;

        public AssetsViewModel() { }
        public AssetsViewModel(
            IAccountBalanceService accountBalanceService,
            IAccountService accountService,
            IBlockchainService blockchainService,
            IMessageAggregator messageAggregator,
            INEP5TokenService nep5TokenService,
            IWalletService walletService)
        {
            this.accountBalanceService = accountBalanceService;
            this.accountService = accountService;
            this.blockchainService = blockchainService;
            this.walletService = walletService;
            this.nep5TokenService = nep5TokenService;
            this.messageAggregator = messageAggregator;

            Assets = new ObservableCollection<AssetSummary>();
        }

        public ObservableCollection<AssetSummary> Assets { get; }
        
        public AssetSummary SelectedAsset
        {
            get => selectedAsset;
            set
            {
                if (selectedAsset == value)
                {
                    return;
                }

                selectedAsset = value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanViewSelectedAssetCertificate));
                this.RaisePropertyChanged(nameof(CanDeleteSelectedAsset));
            }
        }

        public bool CanViewSelectedAssetCertificate =>
            SelectedAsset != null &&
            SelectedAsset.Type == AssetType.GlobalAsset &&
            false;//assetCertificateService.CanViewCertificate(SelectedAsset.Id);

        public bool CanDeleteSelectedAsset =>
            SelectedAsset != null &&
            SelectedAsset.Type == AssetType.GlobalAsset;

        public ReactiveCommand ViewCertificateCommand => ReactiveCommand.Create(ViewCertificate);

        public void Load()
        {
            LoadAssets();

            messageAggregator.Subscribe(this);
        }

        public void Unload()
        {
            messageAggregator.Unsubscribe(this);
        }

        public void HandleMessage(WalletOpenedMessage message)
        {
            LoadAssets();
        }

        public void HandleMessage(WalletClosedMessage message)
        {
            Assets.Clear();
        }

        public void HandleMessage(AccountBalancesChangedMessage message)
        {
            LoadAssets();
        }

        private void LoadAssets()
        {
            Assets.Clear();

            if (!walletService.WalletIsOpen)
            {
                return;
            }

            var globalAssetTotalBalances = new Dictionary<UInt256, Fixed8>();
            var nep5TokenTotalBalances = new Dictionary<UInt160, BigDecimal>();

            foreach (var account in accountService.GetAllAccounts())
            {
                var globalAssetBalances = accountBalanceService.GetGlobalAssetBalances(account.ScriptHash);
                var nep5TokenBalances = accountBalanceService.GetNEP5TokenBalances(account.ScriptHash);

                foreach (var assetId in globalAssetBalances.Keys)
                {
                    var accountBalance = globalAssetBalances[assetId];

                    if (!globalAssetTotalBalances.ContainsKey(assetId))
                    {
                        globalAssetTotalBalances.Add(assetId, Fixed8.Zero);
                    }

                    var totalBalance = globalAssetTotalBalances[assetId];

                    globalAssetTotalBalances[assetId] = totalBalance + accountBalance;
                }

                foreach (var nep5ScriptHash in nep5TokenBalances.Keys)
                {
                    var accountBalance = nep5TokenBalances[nep5ScriptHash];

                    if (!nep5TokenTotalBalances.ContainsKey(nep5ScriptHash))
                    {
                        nep5TokenTotalBalances.Add(nep5ScriptHash, new BigDecimal(BigInteger.Zero, accountBalance.Decimals));
                    }

                    var totalBalance = nep5TokenTotalBalances[nep5ScriptHash];

                    nep5TokenTotalBalances[nep5ScriptHash] = new BigDecimal(totalBalance.Value + accountBalance.Value, accountBalance.Decimals);
                }
            }

            foreach (var assetId in globalAssetTotalBalances.Keys)
            {
                var asset = blockchainService.GetAssetState(assetId);

                bool isSystemAsset;
                string assetName;
                if (assetId == Blockchain.GoverningToken.Hash)
                {
                    isSystemAsset = true;
                    assetName = "NEO";
                }
                else if (assetId == Blockchain.UtilityToken.Hash)
                {
                    isSystemAsset = true;
                    assetName = "GAS";
                }
                else
                {
                    isSystemAsset = false;
                    assetName = asset.Name;
                }

                Assets.Add(new AssetSummary
                {
                    Id = assetId.ToString(),
                    Name = assetName,
                    Type = isSystemAsset ? AssetType.SystemAsset : AssetType.GlobalAsset,
                    IssuerAddress = asset.Issuer.ToString(),
                    Balance = globalAssetTotalBalances[assetId].ToString()
                });
            }

            foreach (var nep5ScriptHash in nep5TokenTotalBalances.Keys)
            {
                var nep5TokenDetails = nep5TokenService.GetTokenDetails(nep5ScriptHash);

                Assets.Add(new AssetSummary
                {
                    Id = nep5ScriptHash.ToString(),
                    Name = nep5TokenDetails.Name,
                    Type = AssetType.NEP5Token,
                    IssuerAddress = $"ScriptHash:{nep5ScriptHash}",
                    Balance = nep5TokenTotalBalances[nep5ScriptHash].ToString()
                });
            }
        }

        private void ViewCertificate()
        {
            if (!CanViewSelectedAssetCertificate)
            {
                return;
            }

            /*var certificatePath = assetCertificateService.GetAssetCertificateFilePath(SelectedAsset.Id);

            if (string.IsNullOrEmpty(certificatePath))
            {
                // TODO Show error message
            }
            else
            {
                processManager.Run(certificatePath);
            }*/
        }
    }
}
