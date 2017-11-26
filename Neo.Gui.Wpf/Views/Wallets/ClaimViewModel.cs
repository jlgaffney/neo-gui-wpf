using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class ClaimViewModel :
        ViewModelBase,
        ILoadable
    {
        private readonly IBlockChainController blockChainController;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private Fixed8 availableGas = Fixed8.Zero;
        private Fixed8 unavailableGas = Fixed8.Zero;

        private bool claimEnabled;

        public ClaimViewModel(
            IBlockChainController blockChainController,
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.blockChainController = blockChainController;
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
        }

        #region Public Properties

        public Fixed8 AvailableGas
        {
            get => this.availableGas;
            set
            {
                if (this.availableGas == value) return;

                this.availableGas = value;

                NotifyPropertyChanged();
            }
        }

        public Fixed8 UnavailableGas
        {
            get => this.unavailableGas;
            set
            {
                if (this.unavailableGas == value) return;

                this.unavailableGas = value;

                NotifyPropertyChanged();
            }
        }

        public bool ClaimEnabled
        {
            get => this.claimEnabled;
            set
            {
                if (this.claimEnabled == value) return;

                this.claimEnabled = value;

                NotifyPropertyChanged();
            }
        }

        #endregion Public Properties

        public ICommand ClaimCommand => new RelayCommand(this.Claim);

        public ICommand ClosingCommand => new RelayCommand(this.OnClosing);

        #region ILoadable implementation

        public void OnLoad()
        {
            // Calculate bonus GAS
            this.CalculateBonusAvailable();
            
            this.CalculateBonusUnavailable(this.blockChainController.BlockHeight + 1);

            this.blockChainController.AddPersistCompletedEventHandler(this.BlockchainPersistCompleted);
        }

        #endregion

        private void OnClosing()
        {
            this.blockChainController.RemovePersistCompletedEventHandler(this.BlockchainPersistCompleted);
        }

        private void BlockchainPersistCompleted(object sender, Block block)
        {
            CalculateBonusUnavailable(block.Index + 1);
        }

        private void CalculateBonusAvailable()
        {
            var bonusAvailable = this.walletController.CalculateBonus();
            this.AvailableGas = bonusAvailable;

            if (bonusAvailable == Fixed8.Zero)
            {
                this.ClaimEnabled = false;
            }
        }

        private void CalculateBonusUnavailable(uint height)
        {
            var unspent = this.walletController.FindUnspentCoins()
                .Where(p => p.Output.AssetId.Equals(this.blockChainController.GoverningToken.Hash))
                .Select(p => p.Reference);

            var references = new HashSet<CoinReference>();

            foreach (var group in unspent.GroupBy(p => p.PrevHash))
            {
                int heightStart;
                var transaction = this.blockChainController.GetTransaction(group.Key, out heightStart);

                if (transaction == null) continue; // not enough of the chain available

                foreach (var reference in group)
                {
                    references.Add(reference);
                }
            }

            this.UnavailableGas = this.walletController.CalculateBonus(references, height);
        }        

        private void Claim()
        {
            var claims = this.walletController.GetUnclaimedCoins().Select(p => p.Reference).ToArray();

            if (claims.Length == 0) return;

            var transaction = new ClaimTransaction
            {
                Claims = claims,
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = this.blockChainController.UtilityToken.Hash,
                        Value = this.walletController.CalculateBonus(claims),
                        ScriptHash = this.walletController.GetChangeAddress()
                    }
                }
            };

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
            this.TryClose();
        }
    }
}