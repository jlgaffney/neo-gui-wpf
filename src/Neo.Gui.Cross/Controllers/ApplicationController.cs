using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using Akka.Actor;
using Neo.Gui.Cross.Certificates;
using Neo.Gui.Cross.IO.Actors;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Services;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using VMArray = Neo.VM.Types.Array;

namespace Neo.Gui.Cross.Controllers
{
    // TODO Implement IDisposable
    public class ApplicationController : IApplicationController
    {
        private const int P2PPort = 20333;
        private const int P2PWsPort = 20334;

        private const int RefreshIntervalMilliseconds = 1000; // 500;

        private readonly IAccountBalanceService accountBalanceService;
        private readonly IAccountService accountService;
        private readonly IBlockchainService blockchainService;
        private readonly ICertificateQueryService certificateQueryService;
        private readonly IMessageAggregator messageAggregator;
        private readonly NeoSystem neoSystem;
        private readonly ISettings settings;
        private readonly IWalletService walletService;

        private IActorRef actor;

        private DateTime blockPersistenceTime;

        private DateTime lastAccountBalancesChangedMessagePublishTime;
        private DateTime lastBlockchainHeightChangedMessagePublishTime;

        private Timer refreshTimer;

        public ApplicationController(
            IAccountBalanceService accountBalanceService,
            IAccountService accountService,
            IBlockchainService blockchainService,
            ICertificateQueryService certificateQueryService,
            IMessageAggregator messageAggregator,
            NeoSystem neoSystem,
            ISettings settings,
            IWalletService walletService)
        {
            this.accountBalanceService = accountBalanceService;
            this.accountService = accountService;
            this.blockchainService = blockchainService;
            this.certificateQueryService = certificateQueryService;
            this.messageAggregator = messageAggregator;
            this.neoSystem = neoSystem;
            this.settings = settings;
            this.walletService = walletService;
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            actor = neoSystem.ActorSystem.ActorOf(EventWrapper<Blockchain.PersistCompleted>.Props(Blockchain_PersistCompleted));
            neoSystem.Blockchain.Tell(new Blockchain.Register(), actor);
            neoSystem.StartNode(P2PPort, P2PWsPort);

            // TODO Start refresh timer
            StartRefreshTimer();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            // TODO Stop refresh timer
            StopRefreshTimer();
            
            if (actor != null)
            {
                neoSystem.ActorSystem.Stop(actor);
                actor = null;
            }

            if (walletService.WalletIsOpen)
            {
                walletService.CloseWallet();
            }

            IsRunning = false;
        }

        private void StartRefreshTimer()
        {
            refreshTimer = new Timer(RefreshIntervalMilliseconds);
            refreshTimer.Elapsed += OnRefresh;
            refreshTimer.AutoReset = true;
            refreshTimer.Start();
        }

        private void StopRefreshTimer()
        {
            refreshTimer.Elapsed -= OnRefresh;
            refreshTimer.Stop();
            refreshTimer.AutoReset = false; // TODO Is this line necessary?
            refreshTimer.Dispose();
            refreshTimer = null;
        }

        private void Blockchain_PersistCompleted(Blockchain.PersistCompleted e)
        {
            // if (IsDisposed) return;
            
            blockPersistenceTime = DateTime.UtcNow;
            if (walletService.WalletIsOpen)
            {
                accountBalanceService.NEP5TokenBalanceChanged = true;
                if (walletService.CurrentWallet.GetCoins().Any(p =>
                    !p.State.HasFlag(CoinState.Spent) &&
                    p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)))
                {
                    // Claimable utility token balance increased

                    accountBalanceService.GlobalAssetBalanceChanged = true;
                }
            }

            if (DateTime.UtcNow - lastBlockchainHeightChangedMessagePublishTime > TimeSpan.FromSeconds(1.5))
            {
                messageAggregator.Publish(new BlockchainHeightChangedMessage(e.Block.Index));
                lastBlockchainHeightChangedMessagePublishTime = DateTime.UtcNow;
            }
        }

        private void OnRefresh(object sender, ElapsedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            var persistenceSpan = DateTime.UtcNow - blockPersistenceTime;
            if (persistenceSpan < TimeSpan.Zero)
            {
                persistenceSpan = TimeSpan.Zero;
            }
            /*if (persistenceSpan > Blockchain.TimePerBlock)
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                toolStripProgressBar1.Value = persistenceSpan.Seconds;
                toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            }*/


            if (!walletService.WalletIsOpen)
            {
                return;
            }

            var globalAssetBalancesChanged = false;
            var nep5TokenBalancesChanged = false;


            if (walletService.CurrentWallet.WalletHeight <= blockchainService.Height + 1)
            {
                if (accountBalanceService.GlobalAssetBalanceChanged)
                {
                    using (var snapshot = blockchainService.GetSnapshot())
                    {
                        var coins = walletService.CurrentWallet.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                        var bonusAvailable = snapshot.CalculateBonus(walletService.CurrentWallet
                            .GetUnclaimedCoins().Select(p => p.Reference));
                        var bonusUnavailable = snapshot.CalculateBonus(
                            coins.Where(p =>
                                p.State.HasFlag(CoinState.Confirmed) &&
                                p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).Select(p => p.Reference),
                            snapshot.Height + 1);
                        var bonus = bonusAvailable + bonusUnavailable;
                        var assets = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                        {
                            Asset = snapshot.Assets.TryGet(k),
                            Value = g.Sum(p => p.Output.Value),
                            Claim = k.Equals(Blockchain.UtilityToken.Hash) ? bonus : Fixed8.Zero
                        }).ToDictionary(p => p.Asset.AssetId);
                        if (bonus != Fixed8.Zero && !assets.ContainsKey(Blockchain.UtilityToken.Hash))
                        {
                            assets[Blockchain.UtilityToken.Hash] = new
                            {
                                Asset = snapshot.Assets.TryGet(Blockchain.UtilityToken.Hash),
                                Value = Fixed8.Zero,
                                Claim = bonus
                            };
                        }
                        var governingTokenAccountBalances = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash))
                            .GroupBy(p => p.Output.ScriptHash)
                            .ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                        var utilityTokenAccountBalances = coins.Where(p => p.Output.AssetId.Equals(Blockchain.UtilityToken.Hash))
                            .GroupBy(p => p.Output.ScriptHash)
                            .ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                        foreach (var account in accountService.GetAllAccounts())
                        {
                            var governingTokenAccountBalance = governingTokenAccountBalances.ContainsKey(account.ScriptHash)
                                ? governingTokenAccountBalances[account.ScriptHash]
                                : Fixed8.Zero;
                            var utilityTokenAccountBalance = utilityTokenAccountBalances.ContainsKey(account.ScriptHash)
                                ? utilityTokenAccountBalances[account.ScriptHash]
                                : Fixed8.Zero;
                            
                            accountBalanceService.UpdateGlobalAssetBalance(account.ScriptHash, Blockchain.GoverningToken.Hash, governingTokenAccountBalance);
                            accountBalanceService.UpdateGlobalAssetBalance(account.ScriptHash, Blockchain.UtilityToken.Hash, utilityTokenAccountBalance);


                            var globalAssetBalances = accountBalanceService.GetGlobalAssetBalances(account.ScriptHash);

                            foreach (var assetId in globalAssetBalances.Keys)
                            {
                                // TODO Skip governing and utility tokens

                                Fixed8 balance;
                                if (!assets.ContainsKey(assetId))
                                {
                                    balance = Fixed8.Zero;
                                }
                                else
                                {
                                    balance = globalAssetBalances[assetId];
                                }

                                accountBalanceService.UpdateGlobalAssetBalance(account.ScriptHash, assetId, balance);
                            }
                        }

                        /*foreach (var asset in assets.Values)
                            {
                                string value_text = asset.Value.ToString() +
                                                    (asset.Asset.AssetId.Equals(Blockchain.UtilityToken.Hash)
                                                        ? $"+({asset.Claim})"
                                                        : "");
                                if (listView2.Items.ContainsKey(asset.Asset.AssetId.ToString()))
                                {
                                    listView2.Items[asset.Asset.AssetId.ToString()].SubItems["value"].Text = value_text;
                                }
                                else
                                {
                                    string asset_name = asset.Asset.AssetType == AssetType.GoverningToken
                                        ? "NEO"
                                        : asset.Asset.AssetType == AssetType.UtilityToken
                                            ? "NeoGas"
                                            : asset.Asset.GetName();
                                    listView2.Items.Add(new ListViewItem(new[]
                                    {
                                        new ListViewItem.ListViewSubItem
                                        {
                                            Name = "name",
                                            Text = asset_name
                                        },
                                        new ListViewItem.ListViewSubItem
                                        {
                                            Name = "type",
                                            Text = asset.Asset.AssetType.ToString()
                                        },
                                        new ListViewItem.ListViewSubItem
                                        {
                                            Name = "value",
                                            Text = value_text
                                        },
                                        new ListViewItem.ListViewSubItem
                                        {
                                            ForeColor = Color.Gray,
                                            Name = "issuer",
                                            Text = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]"
                                        }
                                    }, -1, listView2.Groups["unchecked"])
                                    {
                                        Name = asset.Asset.AssetId.ToString(),
                                        Tag = asset.Asset,
                                        UseItemStyleForSubItems = false
                                    });
                                }
                            }*/

                            // TODO Update view models



                        accountBalanceService.GlobalAssetBalanceChanged = false;

                        globalAssetBalancesChanged = true;
                    }
                }
                    /*foreach (ListViewItem item in listView2.Groups["unchecked"].Items.OfType<ListViewItem>().ToArray())
                    {
                        ListViewItem.ListViewSubItem subitem = item.SubItems["issuer"];
                        AssetState asset = (AssetState)item.Tag;
                        CertificateQueryResult result;
                        if (asset.AssetType == AssetType.GoverningToken || asset.AssetType == AssetType.UtilityToken)
                        {
                            result = new CertificateQueryResult { Type = CertificateQueryResultType.System };
                        }
                        else
                        {
                            result = certificateQueryService.Query(asset.Owner);
                        }
                        using (result)
                        {
                            subitem.Tag = result.Type;
                            switch (result.Type)
                            {
                                case CertificateQueryResultType.Querying:
                                case CertificateQueryResultType.QueryFailed:
                                    break;
                                case CertificateQueryResultType.System:
                                    subitem.ForeColor = Color.Green;
                                    subitem.Text = Strings.SystemIssuer;
                                    break;
                                case CertificateQueryResultType.Invalid:
                                    subitem.ForeColor = Color.Red;
                                    subitem.Text = $"[{Strings.InvalidCertificate}][{asset.Owner}]";
                                    break;
                                case CertificateQueryResultType.Expired:
                                    subitem.ForeColor = Color.Yellow;
                                    subitem.Text = $"[{Strings.ExpiredCertificate}]{result.Certificate.Subject}[{asset.Owner}]";
                                    break;
                                case CertificateQueryResultType.Good:
                                    subitem.ForeColor = Color.Black;
                                    subitem.Text = $"{result.Certificate.Subject}[{asset.Owner}]";
                                    break;
                            }
                            switch (result.Type)
                            {
                                case CertificateQueryResultType.System:
                                case CertificateQueryResultType.Missing:
                                case CertificateQueryResultType.Invalid:
                                case CertificateQueryResultType.Expired:
                                case CertificateQueryResultType.Good:
                                    item.Group = listView2.Groups["checked"];
                                    break;
                            }
                        }
                    }*/
            }

            if (accountBalanceService.NEP5TokenBalanceChanged && persistenceSpan > TimeSpan.FromSeconds(2))
            {
                var addresses = accountService.GetAllAccounts().Select(p => p.ScriptHash).ToArray();
                foreach (var scriptHash in settings.Contracts.NEP5)
                {
                    byte[] script;
                    using (var sb = new ScriptBuilder())
                    {
                        foreach (var address in addresses)
                        {
                            sb.EmitAppCall(scriptHash, "balanceOf", address);
                        }
                        sb.Emit(OpCode.DEPTH, OpCode.PACK);
                        sb.EmitAppCall(scriptHash, "decimals");
                        script = sb.ToArray();
                    }
                    var engine = ApplicationEngine.Run(script);
                    if (engine.State.HasFlag(VMState.FAULT))
                    {
                        continue;
                    }
                        
                    byte decimals = (byte)engine.ResultStack.Pop().GetBigInteger();

                    var amountArray = (VMArray) engine.ResultStack.Pop();
                    amountArray.Reverse();

                    for (int i = 0; i < amountArray.Count; i++)
                    {
                        var address = addresses[i];
                        var amount = amountArray[i].GetBigInteger();
                            
                        accountBalanceService.UpdateNEP5TokenBalance(address, scriptHash, new BigDecimal(amount, decimals));
                    }
                }

                accountBalanceService.NEP5TokenBalanceChanged = false;

                nep5TokenBalancesChanged = true;
            }


            if ((globalAssetBalancesChanged || nep5TokenBalancesChanged) &&
                DateTime.UtcNow - lastAccountBalancesChangedMessagePublishTime > TimeSpan.FromSeconds(1.5))
            {
                messageAggregator.Publish(new AccountBalancesChangedMessage());
                lastAccountBalancesChangedMessagePublishTime = DateTime.UtcNow;
            }
        }
    }
}
