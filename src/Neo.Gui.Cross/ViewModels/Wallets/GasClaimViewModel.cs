using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Wallets
{
    public class GasClaimViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<BlockchainHeightChangedMessage>
    {
        private readonly IGasClaimCalculationService gasClaimCalculationService;
        private readonly IMessageAggregator messageAggregator;
        private readonly ITransactionService transactionService;

        private decimal availableGas = decimal.Zero;
        private decimal unavailableGas = decimal.Zero;

        private bool claimEnabled;

        public GasClaimViewModel() { }
        public GasClaimViewModel(
            IGasClaimCalculationService gasClaimCalculationService,
            IMessageAggregator messageAggregator,
            ITransactionService transactionService)
        {
            this.gasClaimCalculationService = gasClaimCalculationService;
            this.messageAggregator = messageAggregator;
            this.transactionService = transactionService;
        }

        public decimal AvailableGas
        {
            get => availableGas;
            set
            {
                if (availableGas == value) return;

                availableGas = value;

                this.RaisePropertyChanged();
            }
        }

        public decimal UnavailableGas
        {
            get => unavailableGas;
            set
            {
                if (unavailableGas == value) return;

                unavailableGas = value;

                this.RaisePropertyChanged();
            }
        }

        public bool ClaimEnabled
        {
            get => this.claimEnabled;
            set
            {
                if (this.claimEnabled == value) return;

                this.claimEnabled = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand ClaimCommand => ReactiveCommand.Create(Claim);
         
        public void Load()
        {
            messageAggregator.Subscribe(this);

            this.CalculateBonusAvailable();
        }

        public void Unload()
        {
            messageAggregator.Unsubscribe(this);
        }

        public void HandleMessage(BlockchainHeightChangedMessage message)
        {
            CalculateBonusUnavailable(message.Height + 1);
        }
        
        private void CalculateBonusAvailable()
        {
            var bonusAvailable = (decimal) gasClaimCalculationService.CalculateAvailableBonusGas();

            AvailableGas = bonusAvailable;

            if (AvailableGas == decimal.Zero)
            {
                ClaimEnabled = false;
            }
            else
            {
                ClaimEnabled = true;
            }
        }

        private void CalculateBonusUnavailable(uint height)
        {
            UnavailableGas = (decimal) gasClaimCalculationService.CalculateUnavailableBonusGas();
        }

        private async void Claim()
        {
            var claimTransaction = transactionService.CreateClaimTransaction();

            // TODO Sign transaction and relay it to the network
            // await this.walletController.BuildSignAndRelayTransaction(transactionParameters);

            OnClose();
        }
    }
}
