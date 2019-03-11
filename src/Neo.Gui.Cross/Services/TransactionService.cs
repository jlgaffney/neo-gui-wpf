using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Neo.Cryptography.ECC;
using Neo.Gui.Cross.Exceptions;
using Neo.Gui.Cross.Models;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using AssetType = Neo.Network.P2P.Payloads.AssetType;

namespace Neo.Gui.Cross.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IBlockchainService blockchainService;
        private readonly IWalletService walletService;

        public TransactionService(
            IBlockchainService blockchainService,
            IWalletService walletService)
        {
            this.blockchainService = blockchainService;
            this.walletService = walletService;
        }

        public IEnumerable<TransactionStateDetails> GetWalletTransactions()
        {
            ThrowIfWalletNotOpen();
            
            using (var snapshot = blockchainService.GetSnapshot())
            {
                foreach (var transactionInfo in walletService.CurrentWallet.GetTransactions()
                    .Select(transactionId => snapshot.Transactions.TryGet(transactionId))
                    .Where(transactionState => transactionState.Transaction != null)
                    .Select(transactionState => new TransactionStateDetails
                    {
                        Transaction = transactionState.Transaction,
                        BlockIndex = transactionState.BlockIndex,
                        Time = snapshot.GetHeader(transactionState.BlockIndex).Timestamp.ToDateTime()
                    })
                    .OrderBy(p => p.Time))
                {
                    yield return transactionInfo;
                }
            }
        }


        public ClaimTransaction CreateClaimTransaction()
        {
            ThrowIfWalletNotOpen();

            var claims = walletService.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference).ToArray();
            if (claims.Length == 0)
            {
                // TODO Throw exception instead
                return null;
            }

            using (var snapshot = blockchainService.GetSnapshot())
            {
                return new ClaimTransaction
                {
                    Claims = claims,
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new[]
                    {
                        new TransactionOutput
                        {
                            AssetId = Blockchain.UtilityToken.Hash,
                            Value = snapshot.CalculateBonus(claims),
                            ScriptHash = walletService.CurrentWallet.GetChangeAddress()
                        }
                    }
                };
            }
        }

        public InvocationTransaction CreateContractCreationTransaction(byte[] script, byte[] parameterList, ContractParameterType returnType, ContractPropertyState properties, string name, string version, string author, string email, string description)
        {
            using (var sb = new ScriptBuilder())
            {
                sb.EmitSysCall("Neo.Contract.Create", script, parameterList, returnType, properties, name, version, author, email, description);
                return new InvocationTransaction
                {
                    Script = sb.ToArray()
                };
            }
        }

        public InvocationTransaction CreateAssetRegistrationTransaction(AssetType assetType, string name, Fixed8 amount, byte precision, ECPoint owner, UInt160 admin, UInt160 issuer)
        {
            ThrowIfWalletNotOpen();
            
            using (var sb = new ScriptBuilder())
            {
                sb.EmitSysCall("Neo.Asset.Create", assetType, name, amount, precision, owner, admin, issuer);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(owner).ToScriptHash().ToArray()
                        }
                    },
                    Script = sb.ToArray()
                };
            }
        }

        public StateTransaction CreateElectionTransaction(ECPoint publicKey)
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.MakeTransaction(new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Validator,
                        Key = publicKey.ToArray(),
                        Field = "Registered",
                        Value = BitConverter.GetBytes(true)
                    }
                }
            });
        }

        public StateTransaction CreateVotingTransaction(UInt160 accountScriptHash, IEnumerable<ECPoint> votes)
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.MakeTransaction(new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Account,
                        Key = accountScriptHash.ToArray(),
                        Field = "Votes",
                        Value = votes.ToArray().ToByteArray()
                    }
                }
            });
        }


        private void ThrowIfWalletNotOpen()
        {
            if (!walletService.WalletIsOpen)
            {
                throw new WalletNotOpenException();
            }
        }
    }
}
