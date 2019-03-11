using System.Collections.Generic;
using System.Linq;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;

namespace Neo.Gui.Cross.Services
{
    public class GasClaimCalculationService : IGasClaimCalculationService
    {
        private readonly IBlockchainService blockchainService;
        private readonly IWalletService walletService;

        public GasClaimCalculationService(
            IBlockchainService blockchainService,
            IWalletService walletService)
        {
            this.blockchainService = blockchainService;
            this.walletService = walletService;
        }

        public Fixed8 CalculateAvailableBonusGas()
        {
            using (var snapshot = blockchainService.GetSnapshot())
            {
                return snapshot.CalculateBonus(walletService.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference));
            }
        }

        public Fixed8 CalculateUnavailableBonusGas()
        {
            var unspent = walletService.CurrentWallet.FindUnspentCoins()
                    .Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash))
                    .Select(p => p.Reference);

            ICollection<CoinReference> references = new HashSet<CoinReference>();

            foreach (var group in unspent.GroupBy(p => p.PrevHash))
            {
                if (!blockchainService.ContainsTransaction(group.Key))
                {
                    continue; // not enough of the chain available
                }

                foreach (var reference in group)
                {
                    references.Add(reference);
                }
            }

            using (var snapshot = blockchainService.GetSnapshot())
            {
                return snapshot.CalculateBonus(references, snapshot.Height + 1);
            }
        }
    }
}
