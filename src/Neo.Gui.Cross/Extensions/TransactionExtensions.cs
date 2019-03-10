using Neo.Network.P2P.Payloads;
using LocalizableTransactionType = Neo.Gui.Cross.Models.TransactionType;

namespace Neo.Gui.Cross.Extensions
{
    public static class TransactionExtensions
    {
        public static LocalizableTransactionType GetLocalizableTransactionType(this Transaction transaction)
        {
            switch (transaction.Type)
            {
                case TransactionType.MinerTransaction:
                    return LocalizableTransactionType.MinerTransaction;
                case TransactionType.IssueTransaction:
                    return LocalizableTransactionType.IssueTransaction;
                case TransactionType.ClaimTransaction:
                    return LocalizableTransactionType.ClaimTransaction;
                case TransactionType.EnrollmentTransaction:
                    return LocalizableTransactionType.EnrollmentTransaction;
                case TransactionType.RegisterTransaction:
                    return LocalizableTransactionType.RegisterTransaction;
                case TransactionType.ContractTransaction:
                    return LocalizableTransactionType.ContractTransaction;
                case TransactionType.StateTransaction:
                    return LocalizableTransactionType.StateTransaction;
                case TransactionType.PublishTransaction:
                    return LocalizableTransactionType.PublishTransaction;
                case TransactionType.InvocationTransaction:
                    return LocalizableTransactionType.InvocationTransaction;

                default:
                    return LocalizableTransactionType.Unknown;
            }
        }
    }
}
