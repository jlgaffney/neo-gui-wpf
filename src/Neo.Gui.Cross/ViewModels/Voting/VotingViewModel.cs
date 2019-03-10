using System;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using Neo.Wallets;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Voting
{
    public class VotingViewModel :
        ViewModelBase,
        ILoadable<UInt160>
    {
        private readonly IBlockchainService blockchainService;

        private string voterAddress;
        private string votes;

        
        public VotingViewModel(
            IBlockchainService blockchainService)
        {
            this.blockchainService = blockchainService;
        }
        

        public string VoterAddress
        {
            get => voterAddress;
            set
            {
                if (Equals(voterAddress, value))
                {
                    return;
                }

                voterAddress = value;

                this.RaisePropertyChanged();
            }
        }

        public string Votes
        {
            get => votes;
            set
            {
                if (Equals(votes, value))
                {
                    return;
                }

                votes = value;

                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand SubmitCommand => ReactiveCommand.Create(Submit);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);


        public void Load(UInt160 voterScriptHash)
        {
            if (voterScriptHash == null)
            {
                return;
            }

            SetVoterScriptHash(voterScriptHash);
        }

        private void SetVoterScriptHash(UInt160 scriptHash)
        {
            VoterAddress = scriptHash.ToAddress();

            var accountState = blockchainService.GetAccountState(scriptHash);
            
            // Concatenate votes into multi-line string
            Votes = accountState.Votes.Select(x => x.ToString()).ToArray().ToMultiLineString();
        }

        private async void Submit()
        {
            var voterScriptHash = VoterAddress.ToScriptHash().ToString();

            // TODO Build transaction, sign it, and relay transaction to the network

            /*var transactionParameters = new VotingTransactionParameters(voterScriptHash, Votes.ToLines());

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);*/

            OnClose();
        }
    }
}
