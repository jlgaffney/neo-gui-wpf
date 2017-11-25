using Neo.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Neo.Controllers;
using Neo.UI.Base.Controls;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;

namespace Neo.UI.Wallets
{
    public class ClaimViewModel : ViewModelBase
    {
        private readonly IApplicationContext applicationContext;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;

        private Fixed8 availableGas = Fixed8.Zero;
        private Fixed8 unavailableGas = Fixed8.Zero;

        private bool claimEnabled;

        public ClaimViewModel(
            IApplicationContext applicationContext,
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.applicationContext = applicationContext;
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

        public override void OnWindowAttached(NeoWindow window)
        {
            base.OnWindowAttached(window);

            // Calculate bonus GAS
            this.CalculateBonusAvailable();
            
            this.CalculateBonusUnavailable(Blockchain.Default.Height + 1);

            Blockchain.PersistCompleted += Blockchain_PersistCompleted;

            window.Closing += (sender, e) =>
            { Blockchain.PersistCompleted -= Blockchain_PersistCompleted; };
        }

        private void Blockchain_PersistCompleted(object sender, Block block)
        {
            CalculateBonusUnavailable(block.Index + 1);
        }

        private void CalculateBonusAvailable()
        {
            var bonusAvailable = Blockchain.CalculateBonus(this.walletController.GetUnclaimedCoins().Select(p => p.Reference));
            this.AvailableGas = bonusAvailable;

            if (bonusAvailable == Fixed8.Zero)
            {
                this.ClaimEnabled = false;
            }
        }

        private void CalculateBonusUnavailable(uint height)
        {
            var unspent = this.walletController.FindUnspentCoins()
                .Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash))
                .Select(p => p.Reference);

            var references = new HashSet<CoinReference>();

            foreach (var group in unspent.GroupBy(p => p.PrevHash))
            {
                int heightStart;
                var transaction = Blockchain.Default.GetTransaction(group.Key, out heightStart);

                if (transaction == null) continue; // not enough of the chain available

                foreach (var reference in group)
                {
                    references.Add(reference);
                }
            }

            this.UnavailableGas = Blockchain.CalculateBonus(references, height);
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
                        AssetId = Blockchain.UtilityToken.Hash,
                        Value = Blockchain.CalculateBonus(claims),
                        ScriptHash = this.walletController.GetChangeAddress()
                    }
                }
            };

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
            this.TryClose();
        }
    }
}