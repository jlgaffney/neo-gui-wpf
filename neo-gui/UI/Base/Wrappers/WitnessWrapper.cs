using System.ComponentModel;
using System.Drawing.Design;
using Neo.Core;
using Neo.UI.Base.Converters;

namespace Neo.UI.Base.Wrappers
{
    internal class WitnessWrapper
    {
        [Editor(typeof(ScriptEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexConverter))]
        public byte[] InvocationScript { get; set; }

        [Editor(typeof(ScriptEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexConverter))]
        public byte[] VerificationScript { get; set; }

        public Witness Unwrap()
        {
            return new Witness
            {
                InvocationScript = InvocationScript,
                VerificationScript = VerificationScript
            };
        }
    }
}
