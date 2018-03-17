using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Globalization.Resources;
using Neo.UI.Core.Internal.Services.Interfaces;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Wallet.Data;
using Neo.UI.Core.Wallet.Initialization;
using Neo.UI.Core.Wallet.Messages;
using Neo.VM;
using NeoModules.JsonRpc.Client;
using NeoModules.RPC.DTOs;
using NeoModules.RPC.Services;
using BaseWallet = Neo.Wallets.Wallet;
using Timer = System.Timers.Timer;
using Transaction = Neo.Core.Transaction;

namespace Neo.UI.Core.Wallet.Implementations
{
    internal class LightWalletController : BaseWalletController, IWalletController
    {
        private readonly IMessagePublisher messagePublisher;
        private readonly INotificationService notificationService;

        private readonly object walletRefreshLock = new object();

        private bool initialized;
        private bool disposed;

        private RpcClient rpcClient;

        private Timer refreshTimer;

        public LightWalletController(
            ICertificateQueryService certificateQueryService,
            IMessagePublisher messagePublisher,
            INotificationService notificationService,
            ITransactionBuilderFactory transactionBuilderFactory)
            : base(certificateQueryService, messagePublisher, notificationService, transactionBuilderFactory)
        {
            this.messagePublisher = messagePublisher;
            this.notificationService = notificationService;
        }

        public bool LightMode => true;

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

            if (!(parameters is LightWalletInitializationParameters))
            {
                throw new ArgumentException();
            }

            var initializationParameters = (LightWalletInitializationParameters)parameters;

            // TODO Determine an appropriate RPC seed node
            var seedNodeUrl = initializationParameters.RpcSeedList.First();
            
            this.rpcClient = new RpcClient(new Uri(seedNodeUrl));

            // Setup automatic refresh timer
            this.refreshTimer = new Timer
            {
                Interval = 10000,
                AutoReset = true
            };

            this.refreshTimer.Elapsed += (sender, e) => this.RefreshWallet();

            this.refreshTimer.Enabled = true;

            this.initialized = true;
        }

        protected override void RefreshWallet()
        {
            var lockAcquired = Monitor.TryEnter(this.walletRefreshLock);

            if (!lockAcquired) return;

            var refreshTask = Task.Run(async () =>
            {
                var blockService = new NeoApiBlockService(this.rpcClient);
                var nodeService = new NeoApiNodeService(this.rpcClient);

                var blockHeight = (uint)await blockService.GetBlockCount.SendRequestAsync();
                var nodeCount = await nodeService.GetConnectionCount.SendRequestAsync();

                this.messagePublisher.Publish(new WalletStatusMessage(this.WalletIsOpen ? this.currentWallet.WalletHeight : 0, new BlockchainStatus(blockHeight, blockHeight, true, 0.0, TimeSpan.Zero, nodeCount)));

                if (!this.WalletIsOpen) return;

                await this.UpdateWalletBalances();

                // TODO Refresh transaction list
            });

            refreshTask.Wait();

            Monitor.Exit(this.walletRefreshLock);
        }

        protected override void SetCurrentWallet(BaseWallet wallet, IDisposable walletLocker)
        {
            base.SetCurrentWallet(wallet, walletLocker);
            
            // Setup wallet if required
            if (this.WalletIsOpen)
            {
                // Load accounts
                foreach (var account in this.GetAccounts())
                {
                    this.AddAccountItem(account);
                }

                // TODO Load transactions
                /*var walletTransactionHashes = this.currentWallet.GetTransactions();

                // Get transaction information from transaction hashes
                var walletTransactions = new List<TransactionItem>();
                foreach (var transactionHash in walletTransactionHashes)
                {
                    var transaction = this.blockchainService.GetTransaction(transactionHash, out var height);

                    if (transaction == null) continue;

                    var transactionTime = this.blockchainService.GetTimeOfBlock((uint)height);

                    walletTransactions.Add(new TransactionItem(transactionHash, transaction.Type, (uint)height, transactionTime));
                }

                // Add transactions to wallet info, ordered by time
                var orderedTransactions = walletTransactions.OrderBy(item => item.Time);

                foreach (var transactionItem in orderedTransactions)
                {
                    this.AddTransaction(transactionItem);
                }*/

                this.Refresh();
            }
        }

        private async Task UpdateWalletBalances()
        {
            var accountService = new NeoApiAccountService(this.rpcClient);
            var assetService = new NeoApiAssetService(this.rpcClient);
            
            var assetsInWallet = new Dictionary<UInt256, Dictionary<UInt160, AssetBalance>>();
            var nep5TokensInWallet = new Dictionary<UInt160, Dictionary<UInt160, BigDecimal>>();

            var nep5TokenDecimalsCache = new Dictionary<UInt160, byte>();

            var accountScriptHashes = this.currentWallet.GetAccounts().Select(account => account.ScriptHash).ToList();
            
            foreach (var accountScriptHash in accountScriptHashes)
            {
                var address = BaseWallet.ToAddress(accountScriptHash);

                var accountState = await accountService.GetAccountState.SendRequestAsync(address);

                var balanceNeo = accountState?.Balance?.FirstOrDefault(balance => balance.Asset == GoverningTokenAssetId);
                var balanceGas = accountState?.Balance?.FirstOrDefault(balance => balance.Asset == UtilityTokenAssetId);

                var neo = balanceNeo != null ? Fixed8.Parse(balanceNeo.Value) : Fixed8.Zero;
                var gas = balanceGas != null ? Fixed8.Parse(balanceGas.Value) : Fixed8.Zero;

                this.messagePublisher.Publish(new AccountBalanceRefreshedMessage(accountScriptHash.ToString(), (int) ((decimal) neo), (decimal) gas));

                // Add asset balances to a dictionary to be summed later
                if (accountState?.Balance != null)
                {
                    foreach (var assetBalance in accountState.Balance)
                    {
                        var assetId = UInt256.Parse(assetBalance.Asset);
                        var balance = Fixed8.Parse(assetBalance.Value);

                        if (!assetsInWallet.ContainsKey(assetId))
                        {
                            assetsInWallet[assetId] = new Dictionary<UInt160, AssetBalance>();
                        }

                        var balanceInfo = new AssetBalance
                        {
                            Balance = balance
                        };

                        if (assetBalance.Asset == UtilityTokenAssetId)
                        {
                            // TODO Get bonus gas amount
                        }

                        Debug.Assert(!assetsInWallet[assetId].ContainsKey(accountScriptHash));

                        assetsInWallet[assetId][accountScriptHash] = balanceInfo;
                    }
                }

                // Update NEP5 token balances
                foreach (var nep5ScriptHash in this.nep5WatchScriptHashes)
                {
                    var nep5Service = new NeoNep5Service(this.rpcClient, nep5ScriptHash.ToString());

                    if (!nep5TokenDecimalsCache.TryGetValue(nep5ScriptHash, out var decimals))
                    {
                        var decimalsStr = await nep5Service.GetDecimals();
                        decimals = byte.Parse(decimalsStr);
                    }

                    var balance = await nep5Service.GetBalance(accountScriptHash.ToString(), decimals.ToString());

                    var balanceBigInt = BigInteger.Parse(balance);

                    var tokenBalance = new BigDecimal(balanceBigInt, decimals);

                    // Add account's asset balance to dictionary for summing total later
                    if (!nep5TokensInWallet.ContainsKey(nep5ScriptHash))
                    {
                        nep5TokensInWallet.Add(nep5ScriptHash, new Dictionary<UInt160, BigDecimal>());
                    }

                    var nep5TokenAccountBalances = nep5TokensInWallet[nep5ScriptHash];

                    Debug.Assert(!nep5TokenAccountBalances.ContainsKey(accountScriptHash));

                    nep5TokenAccountBalances[accountScriptHash] = tokenBalance;
                }
            }

            // Remove assets with zero balances
            foreach (var assetId in this.currentWalletInfo.GetAssetsInWallet())
            {
                if (assetsInWallet.ContainsKey(assetId)) continue;

                // Balance is zero
                this.messagePublisher.Publish(new AssetTotalBalanceSummaryRemovedMessage(assetId.ToString()));

                this.currentWalletInfo.RemoveAssetFromList(assetId);
            }

            // Get total wallet balances for each asset
            foreach (var assetId in assetsInWallet.Keys)
            {
                var accountBalances = assetsInWallet[assetId];

                var totalBalance = accountBalances.Values.Aggregate(Fixed8.Zero, (x, y) => x + y.Balance);
                var totalBonus = accountBalances.Values.Aggregate(Fixed8.Zero, (x, y) => x + y.Bonus);

                if (!this.currentWalletInfo.WalletContainsAsset(assetId))
                {
                    var assetState = await assetService.GetAssetState.SendRequestAsync(assetId.ToString());

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
                        assetName = GetLocalizedAssetName(assetState.Name);
                        
                        isSystemAsset = false;
                    }

                    // TODO Query for asset owner certificate

                    this.currentWalletInfo.AddAssetToList(assetId, totalBalance);

                    this.messagePublisher.Publish(new AssetTotalBalanceSummaryAddedMessage(assetId.ToString(),
                        assetName, $"{Strings.UnknownIssuer}[{assetState.Owner}]", assetState.Type, isSystemAsset,
                        (decimal) totalBalance, (decimal) totalBonus));
                }
                else
                {
                    this.currentWalletInfo.UpdateAssetTotalBalance(assetId, totalBalance);

                    this.messagePublisher.Publish(new AssetTotalBalanceChangedMessage(assetId.ToString(),
                        (decimal) totalBalance, (decimal) totalBonus));
                }
            }

            // Remove NEP5 tokens that are no longer being watched
            foreach (var scriptHash in this.currentWalletInfo.GetNEP5TokensInWallet())
            {
                if (this.nep5WatchScriptHashes.Contains(scriptHash)) continue;

                // Token is no longer being watched
                this.messagePublisher.Publish(new NEP5TokenTotalBalanceSummaryRemovedMessage(scriptHash.ToString()));

                this.currentWalletInfo.RemoveNEP5TokenFromList(scriptHash);
            }

            // Get total wallet balances for each NEP5 token
            foreach (var scriptHash in this.nep5WatchScriptHashes)
            {
                var accountBalances = nep5TokensInWallet[scriptHash];

                var totalBalance = new BigDecimal(BigInteger.Zero, 0);

                if (accountBalances.Any())
                {
                    var decimals = accountBalances.First().Value.Decimals;

                    Debug.Assert(accountBalances.All(balance => balance.Value.Decimals == decimals));

                    var totalBalanceBigInt = accountBalances.Values.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);

                    totalBalance = new BigDecimal(totalBalanceBigInt, decimals);
                }

                if (!this.currentWalletInfo.WalletContainsNEP5Token(scriptHash))
                {
                    var nep5Service = new NeoNep5Service(this.rpcClient, scriptHash.ToString());

                    var name = await nep5Service.GetName();

                    this.currentWalletInfo.AddNEP5TokenToList(scriptHash, totalBalance.Value, totalBalance.Decimals);

                    this.messagePublisher.Publish(new NEP5TokenTotalBalanceSummaryAddedMessage(scriptHash.ToString(), name, totalBalance.ToString()));
                }
                else
                {
                    this.currentWalletInfo.UpdateNEP5TokenTotalBalance(scriptHash, totalBalance.Value);

                    this.messagePublisher.Publish(new NEP5TokenTotalBalanceChangedMessage(scriptHash.ToString(), totalBalance.ToString()));
                }
            }
        }

        private static string GetLocalizedAssetName(IReadOnlyCollection<Name> names)
        {
            var localizedAssetName = names.FirstOrDefault(name => name.Lang == CultureInfo.CurrentUICulture.Name);

            return localizedAssetName.AssetName ?? names.First().AssetName;
        }

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
                        // Stop automatic refresh timer
                        this.refreshTimer?.Stop();
                        this.refreshTimer = null;

                        // Save and dispose of wallet if required
                        if (this.WalletIsOpen)
                        {
                            this.TrySaveWallet();

                            this.WalletDispose();
                        }

                        this.rpcClient = null;
                    }

                    this.disposed = true;
                }
            }
        }
        #endregion
















        public override bool DeleteAccount(string accountScriptHash)
        {
            var success = base.DeleteAccount(accountScriptHash);

            if (!success) return false;

            this.Refresh();

            return true;
        }

        public async Task<InvokeResult> InvokeScript(byte[] script)
        {
            var contractService = new NeoApiContractService(this.rpcClient);

            var result = await contractService.InvokeScript.SendRequestAsync(script.ToHexString());

            var engineState = (VMState) Enum.Parse(typeof(VMState), result.State);

            var gasConsumed = Fixed8.Parse(result.GasConsumed);

            var stackJson = new List<string>();

            foreach (var stack in result.Stack)
            {
                var stackJObject = new JObject
                {
                    ["type"] = (JObject) stack.Type,
                    ["value"] = (JObject) stack.Value
                };

                stackJson.Add(stackJObject.ToString());
            }

            return GetInvokeResult(engineState, gasConsumed, stackJson);
        }

        protected override Transaction BuildTransaction<TParameters>(TParameters parameters)
        {
            var transaction = base.BuildTransaction(parameters);

            throw new NotImplementedException();

            return transaction;
        }

        public override async Task<bool> Relay(IInventory inventory)
        {
            var transactionService = new NeoApiTransactionService(this.rpcClient);

            return await transactionService.SendRawTransaction.SendRequestAsync(inventory.ToArray().ToHexString());
        }

        public Task<IEnumerable<AssetDto>> GetWalletAssets()
        {
            throw new NotImplementedException();
        }

        public async Task<Transaction> GetTransaction(UInt256 hash)
        {
            var transactionService = new NeoApiTransactionService(this.rpcClient);

            var transactionHex = await transactionService.GetRawTransactionSerialized.SendRequestAsync(hash.ToString());
            
            var transactionBytes = transactionHex.HexToBytes();

            return Transaction.DeserializeFrom(transactionBytes);
        }

        public async Task<IEnumerable<string>> GetVotes(string voterScriptHashStr)
        {
            if (!UInt160.TryParse(voterScriptHashStr, out var voterScriptHash))
            {
                return Enumerable.Empty<string>();
            }

            var accountService = new NeoApiAccountService(this.rpcClient);

            var voterAddress = BaseWallet.ToAddress(voterScriptHash);

            var accountState = await accountService.GetAccountState.SendRequestAsync(voterAddress);

            return accountState.Votes.Select(vote => (string) vote);
        }

        public async Task<ContractStateDto> GetContractState(string scriptHash)
        {
            if (!UInt160.TryParse(scriptHash, out var _)) return null;

            var contractService = new NeoApiContractService(this.rpcClient);

            var contractState = await contractService.GetContractState.SendRequestAsync(scriptHash);

            return new ContractStateDto(contractState.Hash, contractState.Script.HexToBytes(), contractState.Parameters.Select(parameter => (ContractParameterType) Enum.Parse(typeof(ContractParameterType), parameter)), (ContractParameterType) Enum.Parse(typeof(ContractParameterType), contractState.ReturnType), contractState.Storage, contractState.Name, contractState.CodeVersion, contractState.Author, contractState.Email, contractState.Description);
        }

        public async Task<AssetStateDto> GetAssetState(string assetId)
        {
            if (!UInt256.TryParse(assetId, out var _)) return null;

            var assetService = new NeoApiAssetService(this.rpcClient);

            var assetState = await assetService.GetAssetState.SendRequestAsync(assetId);

            return new AssetStateDto(assetState.Id, assetState.Owner, assetState.Admin, assetState.Amount, assetState.Available, assetState.Precision, GetLocalizedAssetName(assetState.Name));
        }

        public decimal CalculateBonus()
        {
            throw new NotImplementedException();
        }

        public decimal CalculateUnavailableBonusGas(uint height)
        {
            throw new NotImplementedException();
        }

        public Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            throw new NotImplementedException();
        }

        public Task DeleteFirstClassAsset(string assetId)
        {
            throw new NotImplementedException();
        }

        public Task ClaimUtilityTokenAsset()
        {
            throw new NotImplementedException();
        }
    }
}