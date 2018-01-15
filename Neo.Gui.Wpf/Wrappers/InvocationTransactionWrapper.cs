using System.ComponentModel;
using System.Drawing.Design;

using Neo.Core;
using Neo.UI.Core.Converters;

namespace Neo.Gui.Wpf.Wrappers
{
    internal class InvocationTransactionWrapper : TransactionWrapper
    {
        [Editor(typeof(ScriptEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexConverter))]
        public byte[] Script { get; set; }

        [TypeConverter(typeof(Fixed8Converter))]
        public Fixed8 Gas { get; set; }

        public InvocationTransactionWrapper()
        {
            this.Version = 1;
        }

        public override Transaction Unwrap()
        {
            InvocationTransaction tx = (InvocationTransaction)base.Unwrap();
            tx.Script = Script;
            tx.Gas = Gas;
            return tx;
        }
    }
}
