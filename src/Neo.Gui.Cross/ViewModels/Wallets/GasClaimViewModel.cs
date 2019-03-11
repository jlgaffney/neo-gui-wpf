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
        private readonly ILocalNodeService localNodeService;
        private readonly IMessageAggregator messageAggregator;
        private readonly ITransactionService transactionService;
        private readonly IWalletService walletService;

        private decimal availableGas = decimal.Zero;
        private decimal unavailableGas = decimal.Zero;

        private bool claimEnabled;

        public GasClaimViewModel() { }
        public GasClaimViewModel(
            IGasClaimCalculationService gasClaimCalculationService,
            ILocalNodeService localNodeService,
            IMessageAggregator messageAggregator,
            ITransactionService transactionService,
            IWalletService walletService)
        {
            this.gasClaimCalculationService = gasClaimCalculationService;
            this.localNodeService = localNodeService;
            this.messageAggregator = messageAggregator;
            this.transactionService = transactionService;
            this.walletService = walletService;
        }

        public decimal AvailableGas
        {
            get => availableGas;
            set
            {
                if (Equals(availableGas, value))
                {
                    return;
                }

                availableGas = value;

                this.RaisePropertyChanged();
            }
        }

        public decimal UnavailableGas
        {
            get => unavailableGas;
            set
            {
                if (Equals(unavailableGas, value))
                {
                    return;
                }

                unavailableGas = value;

                this.RaisePropertyChanged();
            }
        }

        public bool ClaimEnabled
        {
            get => claimEnabled;
            set
            {
                if (Equals(claimEnabled, value))
                {
                    return;
                }

                claimEnabled = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand ClaimCommand => ReactiveCommand.Create(Claim);
         
        public void Load()
        {
            messageAggregator.Subscribe(this);

            CalculateBonusAvailable();
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

        private void Claim()
        {
            if (!ClaimEnabled)
            {
                return;
            }

            var claimTransaction = transactionService.CreateClaimTransaction();

            if (walletService.SignTransaction(claimTransaction))
            {
                localNodeService.RelayTransaction(claimTransaction);
            }
            else
            {
                // TODO Notify user
            }

            OnClose();
        }
    }
}
