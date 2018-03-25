using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Globalization.Resources;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Internal.Services.Interfaces;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet.Data;
using Neo.UI.Core.Wallet.Initialization;
using Neo.UI.Core.Wallet.Messages;
using Neo.VM;
using Neo.Wallets;
using BaseWallet = Neo.Wallets.Wallet;
using Timer = System.Timers.Timer;

namespace Neo.UI.Core.Wallet.Implementations
{
    internal class FullWalletController : BaseWalletController, IWalletController
    {
        #region Private Fields 
        private readonly IBlockchainService blockchainService;
        private readonly IMessagePublisher messagePublisher;
        private readonly INotificationService notificationService;

        private readonly object walletRefreshLock = new object();

        private bool initialized;
        private bool disposed;

        private Timer refreshTimer;

        private bool balanceChanged;
        private bool checkNep5Balance;

        #endregion

        #region Constructor 
        public FullWalletController(
            IBlockchainService blockchainService,
            ICertificateQueryService certificateQueryService,
            IMessagePublisher messagePublisher,
            INotificationService notificationService,
            ITransactionBuilderFactory transactionBuilderFactory)
            : base(certificateQueryService, messagePublisher, notificationService, transactionBuilderFactory)
        {
            this.blockchainService = blockchainService;
            this.messagePublisher = messagePublisher;
            this.notificationService = notificationService;

            StateReader.Default.Notify += Default_Notify;
            StateReader.Default.Log += Default_Log;
        }

        private void Default_Log(object sender, LogEventArgs e)
        {
        }

        private void Default_Notify(object sender, NotifyEventArgs e)
        {
        }
        #endregion

        #region IWalletController implementation

        public bool LightMode => false;

        public void Initialize(IWalletInitializationParameters parameters)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IWalletController));
            }

            if (this.initialized)
            {
                // TODO Add exception message to string resources
                throw new Exception(nameof(IWalletController) + " has already been initialized!");
            }

            if (!(parameters is FullWalletInitializationParameters))
            {
                throw new ArgumentException();
            }

            var initializationParameters = (FullWalletInitializationParameters) parameters;

            base.Initialize(initializationParameters);

            this.blockchainService.Initialize(
                initializationParameters.LocalNodePort,
                initializationParameters.LocalWSPort,
                initializationParameters.BlockchainDataDirectoryPath);

            this.blockchainService.BlockAdded += this.OnBlockAdded;

            // Setup automatic refresh timer
            this.refreshTimer = new Timer
            {
                Interval = 2000,
                AutoReset = true
            };

            this.refreshTimer.Elapsed += (sender, e) => this.RefreshWallet();

            this.refreshTimer.Enabled = true;

            this.initialized = true;
        }

        public Task<InvokeResult> InvokeScript(byte[] script)
        {
            var engine = ApplicationEngine.Run(script);

            var result = GetInvokeResult(engine.State, engine.GasConsumed,
                engine.EvaluationStack.Select(p => p.ToParameter().ToJson().ToString()));
            
            var tcs = new TaskCompletionSource<InvokeResult>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public override Task<bool> Relay(IInventory inventory)
        {
            Guard.ArgumentIsNotNull(inventory, nameof(inventory));
            
            var success = this.blockchainService.Relay(inventory);

            if (success && inventory is Transaction transaction)
            {
                this.currentWallet?.ApplyTransaction(transaction);
            }

            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(success);
            return tcs.Task;
        }

        public Task<IEnumerable<AssetDto>> GetWalletAssets()
        {
            var walletAssets = new List<AssetDto>();

            // Add First-Class assets
            var unspendAssetId = this.FindUnspentAssetIds();

            foreach(var assetId in unspendAssetId)
            {
                var assetState = this.blockchainService.GetAssetState(assetId);

                walletAssets.Add(new AssetDto
                {
                    Id = assetId.ToString(),
                    Name = assetState.ToString(),
                    Decimals = assetState.Precision,
                    TokenType = TokenType.FirstClassToken
                });
            }

            // Add NEP-5 tokens
            var nep5Tokens = this.currentWalletInfo.GetNEP5TokensInWallet();
            foreach(var nep5Token in nep5Tokens)
            {
                var querySuccessful = this.blockchainService.GetNEP5TokenNameAndDecimals(nep5Token, out var tokenName, out var decimals);

                if (!querySuccessful) continue;

                walletAssets.Add(new AssetDto
                {
                    Id = nep5Token.ToString(),
                    Name = tokenName,
                    Decimals = decimals,
                    TokenType = TokenType.NEP5Token
                });
            }

            var tcs = new TaskCompletionSource<IEnumerable<AssetDto>>();
            tcs.SetResult(walletAssets);
            return tcs.Task;
        }

        public Task<Transaction> GetTransaction(UInt256 hash)
        {
            var transaction = this.blockchainService.GetTransaction(hash);

            var tcs = new TaskCompletionSource<Transaction>();
            tcs.SetResult(transaction);
            return tcs.Task;
        }

        public Task<IEnumerable<string>> GetVotes(string voterScriptHash)
        {
            var tcs = new TaskCompletionSource<IEnumerable<string>>();

            var accountState = this.blockchainService.GetAccountState(UInt160.Parse(voterScriptHash));

            if (accountState == null)
            {
                tcs.SetResult(new string[]{});
            }
            else
            {
                tcs.SetResult(accountState.Votes.Select(x => x.ToString()).ToArray());
            }

            return tcs.Task;
        }

        public Task<ContractStateDto> GetContractState(string scriptHashStr)
        {
            var tcs = new TaskCompletionSource<ContractStateDto>();

            if (!UInt160.TryParse(scriptHashStr, out var scriptHash))
            {
                tcs.SetResult(null);
            }
            else
            {
                var contractState = this.blockchainService.GetContractState(scriptHash);

                var contractStateDto = new ContractStateDto(contractState.ScriptHash.ToString(),
                    contractState.Script, contractState.ParameterList, contractState.ReturnType,
                    contractState.HasStorage, contractState.Name, contractState.CodeVersion,
                    contractState.Author, contractState.Email, contractState.Description);

                tcs.SetResult(contractStateDto);
            }

            return tcs.Task;
        }

        public Task<AssetStateDto> GetAssetState(string assetIdStr)
        {
            if (!UInt256.TryParse(assetIdStr, out var assetId)) return null;

            var assetState = this.blockchainService.GetAssetState(assetId);

            var assetStateDto = new AssetStateDto(
                assetState.AssetId.ToString(),
                assetState.Owner.ToString(),
                assetState.Admin.ToString(),
                assetState.Amount.ToString(),
                assetState.Available.ToString(),
                assetState.Precision, assetState.Name);

            var tcs = new TaskCompletionSource<AssetStateDto>();
            tcs.SetResult(assetStateDto);

            return tcs.Task;
        }

        public decimal CalculateBonus()
        {
            if (!this.WalletIsOpen) return decimal.Zero;

            return (decimal) this.CalculateBonus(this.currentWallet.GetUnclaimedCoins().Select(p => p.Reference));
        }

        public decimal CalculateUnavailableBonusGas(uint height)
        {
            if (!this.WalletIsOpen) return decimal.Zero;

            var unspent = this.currentWallet.FindUnspentCoins().Where(p =>p.Output.AssetId.ToString()
                .Equals(GoverningTokenAssetId)).Select(p => p.Reference);
            
            var references = new HashSet<CoinReference>();

            foreach (var group in unspent.GroupBy(p => p.PrevHash))
            {
                var transaction = this.GetTransaction(group.Key);

                if (transaction == null) continue; // not enough of the chain available

                foreach (var reference in group)
                {
                    references.Add(reference);
                }
            }

            return (decimal)this.blockchainService.CalculateBonus(references, height);
        }

        public Transaction MakeTransaction(
            Transaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, change_address: changeAddress, fee: fee);
        }

        public async Task DeleteFirstClassAsset(string assetIdStr)
        {
            this.ThrowIfWalletIsNotOpen();

            if (!UInt256.TryParse(assetIdStr, out var assetId)) return;

            var amountToDelete = this.currentWallet.GetAvailable(assetId);
            
            var transactionParameters = new DeleteFirstClassAssetTransactionParameters(assetId, amountToDelete);

            await this.BuildSignAndRelayTransaction(transactionParameters);
        }

        public async Task ClaimUtilityTokenAsset()
        {
            this.ThrowIfWalletIsNotOpen();

            var claims = this.currentWallet.GetUnclaimedCoins()
                .Select(p => p.Reference).ToArray();

            if (claims.Length == 0) return;

            var claimingAssetId = UInt256.Parse(UtilityTokenAssetId);
            var claimAmount = this.CalculateBonus(claims);
            var changeAddress = this.GetChangeAddress();

            var transactionParameters = new ClaimTransactionParameters(claims, claimingAssetId, claimAmount, changeAddress);

            await this.BuildSignAndRelayTransaction(transactionParameters);
        }

        public override bool DeleteAccount(string accountScriptHash)
        {
            var success = base.DeleteAccount(accountScriptHash);

            if (!success) return false;

            this.SetWalletBalanceChangedFlag();
            this.checkNep5Balance = true;

            return true;
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.initialized)
                    {
                        this.blockchainService.BlockAdded -= this.OnBlockAdded;

                        // Stop automatic refresh timer
                        this.refreshTimer?.Stop();
                        this.refreshTimer = null;

                        // Save and dispose of wallet if required
                        if (this.WalletIsOpen)
                        {
                            this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;

                            this.TrySaveWallet();

                            this.WalletDispose();
                        }
                    }

                    // Dispose of blockchain controller
                    this.blockchainService.Dispose();

                    this.disposed = true;
                }
            }
        }
        #endregion

        #region Private Methods

        private void OnBlockAdded(object sender, EventArgs e)
        {
            if (!this.WalletIsOpen) return;

            this.checkNep5Balance = true;

            var coins = this.currentWallet.GetCoins();

            // Check if bonus GAS value changed
            if (coins.Any(coin => !coin.State.HasFlag(CoinState.Spent) &&
                coin.Output.AssetId.ToString().Equals(GoverningTokenAssetId)))
            {
                this.SetWalletBalanceChangedFlag();
            }
        }

        private IEnumerable<UInt256> FindUnspentAssetIds()
        {
            this.ThrowIfWalletIsNotOpen();

            var distinctUnspentedCoinsAssetId = this.currentWallet
                .FindUnspentCoins()
                .Select(x => x.Output.AssetId)
                .Distinct();

            return distinctUnspentedCoinsAssetId;
        }

        protected override void RefreshWallet()
        {
            var lockAcquired = Monitor.TryEnter(this.walletRefreshLock);

            if (!lockAcquired) return;

            try
            {
                var blockchainStatus = this.blockchainService.GetStatus();

                this.messagePublisher.Publish(new WalletStatusMessage(this.WalletIsOpen ? this.currentWallet.WalletHeight : 0, blockchainStatus));

                if (!this.WalletIsOpen) return;

                this.UpdateWalletBalances(blockchainStatus.TimeSinceLastBlock);
            }
            finally
            {
                Monitor.Exit(this.walletRefreshLock);
            }
        }

        private void UpdateWalletBalances(TimeSpan timeSinceLastBlock)
        {
            if (this.currentWallet.WalletHeight <= this.blockchainService.BlockHeight + 1)
            {
                if (this.GetWalletBalanceChangedFlag())
                {
                    var governingTokenAssetId = UInt256.Parse(GoverningTokenAssetId);
                    var utilityTokenAssetId = UInt256.Parse(UtilityTokenAssetId);

                    var coins = this.currentWallet.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                    var bonusAvailable = this.blockchainService.CalculateBonus(this.currentWallet.GetUnclaimedCoins().Select(p => p.Reference));
                    var bonusUnavailable = this.blockchainService.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(governingTokenAssetId)).Select(p => p.Reference), this.blockchainService.BlockHeight + 1);
                    var bonus = bonusAvailable + bonusUnavailable;

                    var assets = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                    {
                        Asset = this.blockchainService.GetAssetState(k),
                        Value = g.Sum(p => p.Output.Value),
                        Claim = k.Equals(utilityTokenAssetId) ? bonus : Fixed8.Zero
                    }).ToDictionary(p => p.Asset.AssetId);

                    if (bonus != Fixed8.Zero && !assets.ContainsKey(utilityTokenAssetId))
                    {
                        assets[utilityTokenAssetId] = new
                        {
                            Asset = this.blockchainService.GetAssetState(utilityTokenAssetId),
                            Value = Fixed8.Zero,
                            Claim = bonus
                        };
                    }

                    var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(governingTokenAssetId)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                    var balanceGas = coins.Where(p => p.Output.AssetId.Equals(utilityTokenAssetId)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));

                    // Update account balances
                    foreach (var account in this.currentWallet.GetAccounts())
                    {
                        var neo = balanceNeo.ContainsKey(account.ScriptHash) ? balanceNeo[account.ScriptHash] : Fixed8.Zero;
                        var gas = balanceGas.ContainsKey(account.ScriptHash) ? balanceGas[account.ScriptHash] : Fixed8.Zero;

                        this.messagePublisher.Publish(new AccountBalanceRefreshedMessage(account.ScriptHash.ToString(), (int)((decimal)neo), (decimal)gas));
                    }


                    // Remove assets with zero balances
                    foreach (var assetId in this.currentWalletInfo.GetAssetsInWallet())
                    {
                        if (assets.ContainsKey(assetId)) continue;

                        // Balance is zero
                        this.messagePublisher.Publish(new AssetTotalBalanceSummaryRemovedMessage(assetId.ToString()));

                        this.currentWalletInfo.RemoveAssetFromList(assetId);
                    }

                    // Update asset total balance summaries
                    foreach (var assetId in assets.Keys)
                    {
                        var assetTotalBalance = assets[assetId];

                        if (!this.currentWalletInfo.WalletContainsAsset(assetId))
                        {
                            string assetName;
                            bool isSystemAsset;
                            if (assetId.ToString() == GoverningTokenAssetId)
                            {
                                assetName = "NEO";
                                isSystemAsset = true;
                            }
                            else if (assetId.ToString() == UtilityTokenAssetId)
                            {
                                assetName = "GAS";
                                isSystemAsset = true;
                            }
                            else
                            {
                                assetName = assetTotalBalance.Asset.Name;
                                isSystemAsset = false;
                            }

                            var assetInfo = new AssetInfo(assetId, isSystemAsset ? null : assetTotalBalance.Asset.Owner, assetName);

                            this.assetInfoCache.AddAssetInfo(assetInfo);

                            this.currentWalletInfo.AddAssetToList(assetId, assetTotalBalance.Value);

                            var issuer = isSystemAsset ? Strings.SystemIssuer : $"{Strings.UnknownIssuer}[{assetTotalBalance.Asset.Owner}]";

                            this.messagePublisher.Publish(new AssetTotalBalanceSummaryAddedMessage(assetId.ToString(),
                                assetName, issuer, assetTotalBalance.Asset.AssetType.ToString(), isSystemAsset,
                                    (decimal) assetTotalBalance.Value, (decimal) assetTotalBalance.Claim));
                        }
                        else
                        {
                            this.currentWalletInfo.UpdateAssetTotalBalance(assetId, assetTotalBalance.Value);

                            this.messagePublisher.Publish(new AssetTotalBalanceChangedMessage(assetId.ToString(),
                                (decimal) assetTotalBalance.Value, (decimal) assetTotalBalance.Claim));
                        }
                    }

                    this.ClearWalletBalanceChangedFlag();
                }
            }

            this.UpdateNEP5TokenTotalBalances(timeSinceLastBlock);

            this.CheckAssetIssuerCertificates();
        }

        private void UpdateNEP5TokenTotalBalances(TimeSpan timeSinceLastBlock)
        {
            if (timeSinceLastBlock <= TimeSpan.FromSeconds(2)) return;

            // Remove NEP5 tokens that are no longer being watched
            foreach (var scriptHash in this.currentWalletInfo.GetNEP5TokensInWallet())
            {
                if (this.nep5WatchScriptHashes.Contains(scriptHash)) continue;

                // Token is no longer being watched
                this.messagePublisher.Publish(new NEP5TokenTotalBalanceSummaryRemovedMessage(scriptHash.ToString()));

                this.currentWalletInfo.RemoveNEP5TokenFromList(scriptHash);
            }

            if (!checkNep5Balance) return;

            // Update balances
            var accountScriptHashes = this.currentWallet
                .GetAccounts()
                .Select(account => account.ScriptHash)
                .ToList();

            foreach (var nep5ScriptHash in this.nep5WatchScriptHashes)
            {
                var balances = this.blockchainService.GetNEP5Balances(nep5ScriptHash, accountScriptHashes, out var decimals);

                if (balances == null) continue;

                var totalBalanceBigInt = balances.Values.Aggregate(BigInteger.Zero, (x, y) => x + y);

                var totalBalance = new BigDecimal(totalBalanceBigInt, decimals);

                if (!this.currentWalletInfo.WalletContainsNEP5Token(nep5ScriptHash))
                {
                    var name = this.blockchainService.GetNEP5TokenName(nep5ScriptHash);
                    
                    this.currentWalletInfo.AddNEP5TokenToList(nep5ScriptHash, totalBalanceBigInt, decimals);

                    this.messagePublisher.Publish(new NEP5TokenTotalBalanceSummaryAddedMessage(nep5ScriptHash.ToString(), name, totalBalance.ToString()));
                }
                else
                {
                    this.currentWalletInfo.UpdateNEP5TokenTotalBalance(nep5ScriptHash, totalBalance.Value);

                    this.messagePublisher.Publish(new NEP5TokenTotalBalanceChangedMessage(nep5ScriptHash.ToString(), totalBalance.ToString()));
                }
            }

            checkNep5Balance = false;
        }

        protected override void SetCurrentWallet(BaseWallet wallet, IDisposable walletLocker)
        {
            if (this.WalletIsOpen)
            {
                // Dispose current wallet
                this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;
            }

            base.SetCurrentWallet(wallet, walletLocker);

            // Setup wallet if required
            if (this.WalletIsOpen)
            {
                // Load accounts
                foreach (var account in this.GetAccounts())
                {
                    this.AddAccountItem(account);
                }

                // Load transactions
                var walletTransactionHashes = this.currentWallet.GetTransactions();

                // Get transaction information from transaction hashes
                var walletTransactions = new List<TransactionMetadata>();
                foreach (var transactionHash in walletTransactionHashes)
                {
                    var transaction = this.blockchainService.GetTransaction(transactionHash, out var height);

                    if (transaction == null) continue;

                    var transactionTime = this.blockchainService.GetTimeOfBlock((uint) height);

                    walletTransactions.Add(new TransactionMetadata(transactionHash, transaction.Type, (uint) height, transactionTime));
                }

                // Add transactions to wallet info, ordered by time
                var orderedTransactions = walletTransactions.OrderBy(item => item.Time);

                foreach (var transactionItem in orderedTransactions)
                {
                    this.AddTransaction(transactionItem);
                }

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;

                this.SetWalletBalanceChangedFlag();
                this.checkNep5Balance = true;

                this.Refresh();
            }
        }

        private void CurrentWalletBalanceChanged(object sender, BalanceEventArgs e)
        {
            var transaction = e.Transaction;

            var transactionItem = new TransactionMetadata(transaction.Hash, transaction.Type, e.Height, TimeHelper.UnixTimestampToDateTime(e.Time));
            
            this.AddTransaction(transactionItem);

            this.SetWalletBalanceChangedFlag();
        }

        private Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return this.blockchainService.CalculateBonus(inputs, ignoreClaimed);
        }

        private bool GetWalletBalanceChangedFlag()
        {
            return this.balanceChanged;
        }

        private void SetWalletBalanceChangedFlag()
        {
            this.balanceChanged = true;
        }

        private void ClearWalletBalanceChangedFlag()
        {
            this.balanceChanged = false;
        }

        protected override Transaction BuildTransaction<TParameters>(TParameters parameters)
        {
            var transaction = base.BuildTransaction(parameters);

            if (transaction is InvocationTransaction invocationTransaction)
            {
                var invocationParameters = parameters as InvokeContractTransactionParameters;

                Debug.Assert(invocationParameters != null);

                var transactionFee = invocationTransaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : Fixed8.Zero;
                
                UInt160 changeAddress = null;
                if (!string.IsNullOrEmpty(invocationParameters.ChangeAddress))
                {
                    changeAddress = BaseWallet.ToScriptHash(invocationParameters.ChangeAddress);
                }

                transaction = this.MakeTransaction(new InvocationTransaction
                {
                    Version = transaction.Version,
                    Script = invocationTransaction.Script,
                    Gas = invocationTransaction.Gas,
                    Attributes = transaction.Attributes,
                    Inputs = transaction.Inputs,
                    Outputs = transaction.Outputs
                }, fee: transactionFee);

                if (transaction == null)
                {
                    throw new Exception("Transaction could not be created!");
                }
            }
            else if (transaction is IssueTransaction issueTransaction)
            {
                // Set asset distribution transaction fee
                transaction = this.currentWallet.MakeTransaction(issueTransaction, fee: Fixed8.One);
            }
            else if (parameters is DeleteFirstClassAssetTransactionParameters)
            {
                // Set transaction fee to zero
                transaction = this.currentWallet.MakeTransaction(transaction, fee: Fixed8.Zero);
            }
            else
            {
                var transferParameters = parameters as AssetTransferTransactionParameters;

                if (transferParameters != null && transaction is ContractTransaction contractTransaction)
                {
                    // A first-class asset transfer transaction is being built
                    // Add fee and change address info to the transaction
                    var fee = Fixed8.Zero;
                    if (!string.IsNullOrEmpty(transferParameters.TransferFee))
                    {
                        fee = Fixed8.Parse(transferParameters.TransferFee);
                    }

                    UInt160 changeAddress = null;
                    if (!string.IsNullOrEmpty(transferParameters.TransferChangeAddress))
                    {
                        changeAddress = BaseWallet.ToScriptHash(transferParameters.TransferChangeAddress);
                    }

                    transaction = this.MakeTransaction(contractTransaction, changeAddress, fee);
                }
            }

            return transaction;
        }
        #endregion
    }
}
