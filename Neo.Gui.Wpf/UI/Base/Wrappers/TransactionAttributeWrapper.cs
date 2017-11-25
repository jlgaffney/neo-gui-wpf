using System.ComponentModel;
using Neo.Core;
using Neo.UI.Base.Converters;

namespace Neo.UI.Base.Wrappers
{
    internal class TransactionAttributeWrapper
    {
        public TransactionAttributeUsage Usage { get; set; }

        [TypeConverter(typeof(HexConverter))]
        public byte[] Data { get; set; }

        public TransactionAttribute Unwrap()
        {
            return new TransactionAttribute
            {
                Usage = Usage,
                Data = Data
            };
        }
    }
}
