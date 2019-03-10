using System.ComponentModel;
using Neo.Gui.Cross.Localization;
using Neo.Gui.Cross.Resources;

namespace Neo.Gui.Cross.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TransactionType
    {
        //[LocalizedDescription(nameof(Strings.Unknown), typeof(Strings))]
        Unknown,
        

        //[LocalizedDescription(nameof(Strings.MinerTransaction), typeof(Strings))]
        MinerTransaction,

        //[LocalizedDescription(nameof(Strings.IssuerTransaction), typeof(Strings))]
        IssueTransaction,

        //[LocalizedDescription(nameof(Strings.ClaimTransaction), typeof(Strings))]
        ClaimTransaction,

        //[LocalizedDescription(nameof(Strings.EnrollmentTransaction), typeof(Strings))]
        EnrollmentTransaction,

        //[LocalizedDescription(nameof(Strings.RegisterTransaction), typeof(Strings))]
        RegisterTransaction,

        //[LocalizedDescription(nameof(Strings.ContractTransaction), typeof(Strings))]
        ContractTransaction,

        //[LocalizedDescription(nameof(Strings.StateTransaction), typeof(Strings))]
        StateTransaction,

        //[LocalizedDescription(nameof(Strings.PublishTransaction), typeof(Strings))]
        PublishTransaction,

        //[LocalizedDescription(nameof(Strings.InvocationTransaction), typeof(Strings))]
        InvocationTransaction
    }
}
