using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.IO;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;
using Neo.VM;

namespace Neo.Controllers
{
    public class BlockChainController : IBlockChainController
    {
        #region Private Fields
        private readonly IWalletController walletController;
        private readonly IApplicationContext applicationContext;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatcher dispatcher;

        private DateTime persistenceTime = DateTime.MinValue;
        private Timer uiUpdateTimer;
        private bool balanceChanged;
        private bool checkNep5Balance;
        #endregion

        #region Constructor 
        public BlockChainController(
            IWalletController walletController,
            IApplicationContext applicationContext, 
            IMessagePublisher messagePublisher, 
            IDispatcher dispatcher)
        {
            this.walletController = walletController;
            this.applicationContext = applicationContext;
            this.messagePublisher = messagePublisher;
            this.dispatcher = dispatcher;

            Task.Run(() =>
            {
                CheckForNewerVersion();

                ImportBlocksIfRequired();

                Blockchain.PersistCompleted += this.BlockchainPersistCompleted;

                // Start node
                this.applicationContext.LocalNode.Start(Settings.Default.NodePort, Settings.Default.WsPort);
            });

            
            if (this.uiUpdateTimer != null)
            {
                // Stop previous timer
                this.uiUpdateTimer.Stop();

                this.uiUpdateTimer.Elapsed -= this.UpdateWallet;

                this.uiUpdateTimer.Dispose();

                this.uiUpdateTimer = null;
            }

            var timer = new Timer
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += this.UpdateWallet;
            this.uiUpdateTimer = timer;
        }
        #endregion

        #region Private Methods 
        private void UpdateWallet(object sender, ElapsedEventArgs e)
        {
            var persistenceSpan = DateTime.UtcNow - this.persistenceTime;

            this.UpdateBlockProgress(persistenceSpan);

            this.UpdateBalances(persistenceSpan);
        }

        private void UpdateBlockProgress(TimeSpan persistenceSpan)
        {
            var blockProgressIndeterminate = false;
            var blockProgress = 0;

            if (persistenceSpan < TimeSpan.Zero)
            {
                persistenceSpan = TimeSpan.Zero;
            }

            if (persistenceSpan > Blockchain.TimePerBlock)
            {
                blockProgressIndeterminate = true;
            }
            else
            {
                blockProgressIndeterminate = true;
                blockProgress = persistenceSpan.Seconds;
            }

            var blockHeight = $"{GetWalletHeight()}/{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            var nodeCount = this.applicationContext.LocalNode.RemoteNodeCount;
            var blockStatus = $"{Strings.WaitingForNextBlock}:";

            var blockProgressMessage = new BlockProgressMessage(
                blockProgressIndeterminate, 
                blockProgress, 
                blockHeight, 
                nodeCount, 
                blockStatus);
            this.messagePublisher.Publish(blockProgressMessage);
        }

        private void UpdateBalances(TimeSpan persistenceSpan)
        {
            if (!this.walletController.IsWalletOpen) return;

            this.UpdateAssetBalances();

            this.UpdateNEP5TokenBalances(persistenceSpan);
        }

        private void UpdateAssetBalances()
        {
            if (this.walletController.WalletWeight > Blockchain.Default.Height + 1) return;

            this.messagePublisher.Publish(new AccountBalancesChangedMessage());

            this.messagePublisher.Publish(new UpdateAssetsBalanceMessage(this.balanceChanged));
        }

        private async void UpdateNEP5TokenBalances(TimeSpan persistenceSpan)
        {
            if (!checkNep5Balance) return;

            if (persistenceSpan <= TimeSpan.FromSeconds(2)) return;

            // Update balances
            var addresses = this.walletController.GetAddresses();

            foreach (var s in Settings.Default.NEP5Watched)
            {
                var scriptHash = UInt160.Parse(s);
                byte[] script;
                using (var builder = new ScriptBuilder())
                {
                    foreach (var address in addresses)
                    {
                        builder.EmitAppCall(scriptHash, "balanceOf", address);
                    }
                    builder.Emit(OpCode.DEPTH, OpCode.PACK);
                    builder.EmitAppCall(scriptHash, "decimals");
                    builder.EmitAppCall(scriptHash, "name");
                    script = builder.ToArray();
                }

                var engine = ApplicationEngine.Run(script);
                if (engine.State.HasFlag(VMState.FAULT)) continue;

                var name = engine.EvaluationStack.Pop().GetString();
                var decimals = (byte)engine.EvaluationStack.Pop().GetBigInteger();
                var amount = engine.EvaluationStack.Pop().GetArray().Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());
                if (amount == 0) continue;
                var balance = new BigDecimal(amount, decimals);
                var valueText = balance.ToString();

                await this.dispatcher.InvokeOnMainUIThread(() =>
                {
                    var item = (AssetItem)null; //this.GetAsset(scriptHash);

                    if (item != null)
                    {
                        item.Value = valueText;
                    }
                    else
                    {
                        var assetItem = new AssetItem
                        {
                            Name = name,
                            Type = "NEP-5",
                            Issuer = $"ScriptHash:{scriptHash}",
                            Value = valueText,
                        };

                        this.messagePublisher.Publish(new AddAssetMessage(assetItem));
                    }
                });
            }
            checkNep5Balance = false;
        }

        private uint GetWalletHeight()
        {
            uint walletHeight = 0;

            if (this.walletController.IsWalletOpen &&
                this.walletController.WalletWeight > 0)
            {
                // Set wallet height
                walletHeight = this.walletController.WalletWeight - 1;
            }

            return walletHeight;
        }

        private void CheckForNewerVersion()
        {
            var latestVersion = VersionHelper.LatestVersion;
            var currentVersion = VersionHelper.CurrentVersion;

            if (latestVersion == null || latestVersion <= currentVersion) return;

            this.messagePublisher.Publish(new NewVersionAvailableMessage($"{Strings.DownloadNewVersion}: {latestVersion}"));
        }

        private void ImportBlocksIfRequired()
        {
            const string acc_path = "chain.acc";
            const string acc_zip_path = acc_path + ".zip";

            // Check if blocks need importing
            if (File.Exists(acc_path))
            {
                // Import blocks
                using (var fileStream = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ImportBlocks(fileStream);
                }
                File.Delete(acc_path);
            }
            else if (File.Exists(acc_zip_path))
            {
                using (var fileStream = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(acc_path).Open())
                {
                    ImportBlocks(zipStream);
                }
                File.Delete(acc_zip_path);
            }
        }

        private void ImportBlocks(Stream stream)
        {
            var blockchain = (LevelDBBlockchain)Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (var reader = new BinaryReader(stream))
            {
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= Blockchain.Default.Height) continue;

                    var block = array.AsSerializable<Block>();
                    Blockchain.Default.AddBlock(block);
                }
            }
            blockchain.VerifyBlocks = true;
        }

        private void BlockchainPersistCompleted(object sender, Block block)
        {
            this.persistenceTime = DateTime.UtcNow;
            if (this.walletController.IsWalletOpen)
            {
                this.checkNep5Balance = true;

                var coins = this.walletController.GetCoins();

                if (coins.Any(
                    coin => !coin.State.HasFlag(CoinState.Spent) &&
                    coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)))
                {
                    balanceChanged = true;
                }
            }

            this.messagePublisher.Publish(new UpdateTransactionsMessage(Enumerable.Empty<TransactionInfo>()));
        }
        #endregion
    }
}
