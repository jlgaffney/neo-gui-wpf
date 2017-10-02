using Neo.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Neo.UI.Controls;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels
{
    public class ClaimViewModel : ViewModelBase
    {
        private NeoWindow view;

        private Fixed8 availableGAS = Fixed8.Zero;
        private Fixed8 unavailableGAS = Fixed8.Zero;

        private bool claimEnabled;

        #region Public Properties

        public Fixed8 AvailableGAS
        {
            get => this.availableGAS;
            set
            {
                if (this.availableGAS == value) return;

                this.availableGAS = value;

                NotifyPropertyChanged();
            }
        }

        public Fixed8 UnavailableGAS
        {
            get => this.unavailableGAS;
            set
            {
                if (this.unavailableGAS == value) return;

                this.unavailableGAS = value;

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

        public override void OnViewAttached(object view)
        {
            this.view = view as NeoWindow;

            // Calculate bonus GAS
            this.CalculateBonusAvailable();
            
            this.CalculateBonusUnavailable(Blockchain.Default.Height + 1);

            Blockchain.PersistCompleted += Blockchain_PersistCompleted;

            this.view.Closing += (sender, e) =>
            { Blockchain.PersistCompleted -= Blockchain_PersistCompleted; };
        }

        private void Blockchain_PersistCompleted(object sender, Block block)
        {
            CalculateBonusUnavailable(block.Index + 1);
        }    

        private void CalculateBonusAvailable()
        {
            var bonusAvailable = Blockchain.CalculateBonus(App.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference));
            this.AvailableGAS = bonusAvailable;

            if (bonusAvailable == Fixed8.Zero)
            {
                this.ClaimEnabled = false;
            }
        }

        private void CalculateBonusUnavailable(uint height)
        {
            var unspent = App.CurrentWallet.FindUnspentCoins()
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

            this.UnavailableGAS = Blockchain.CalculateBonus(references, height);
        }        

        private void Claim()
        {
            var claims = App.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference).ToArray();
            if (claims.Length == 0) return;
            Helper.SignAndShowInformation(new ClaimTransaction
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
                        ScriptHash = App.CurrentWallet.GetChangeAddress()
                    }
                }
            });

            this.view?.Close();
        }
    }
}