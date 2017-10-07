using System.ComponentModel;
using Neo.Core;

namespace Neo.UI.Base.Wrappers
{
    internal class CoinReferenceWrapper
    {
        [TypeConverter(typeof(UIntBaseConverter))]
        public UInt256 PrevHash { get; set; }
        public ushort PrevIndex { get; set; }

        public CoinReference Unwrap()
        {
            return new CoinReference
            {
                PrevHash = PrevHash,
                PrevIndex = PrevIndex
            };
        }

        public static CoinReferenceWrapper Wrap(CoinReference reference)
        {
            return new CoinReferenceWrapper
            {
                PrevHash = reference.PrevHash,
                PrevIndex = reference.PrevIndex
            };
        }
    }
}
