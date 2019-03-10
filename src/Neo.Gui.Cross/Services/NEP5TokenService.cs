using Neo.Gui.Cross.Models;
using Neo.SmartContract;
using Neo.VM;

namespace Neo.Gui.Cross.Services
{
    public class NEP5TokenService : INEP5TokenService
    {
        public NEP5TokenDetails GetTokenDetails(UInt160 nep5ScriptHash)
        {
            byte[] script;
            using (var sb = new ScriptBuilder())
            {
                sb.EmitAppCall(nep5ScriptHash, "decimals");
                sb.EmitAppCall(nep5ScriptHash, "name");
                script = sb.ToArray();
            }

            var engine = ApplicationEngine.Run(script);

            if (engine.State.HasFlag(VMState.FAULT))
            {
                return null;
            }

            var name = engine.ResultStack.Pop().GetString();
            var decimals = (byte)engine.ResultStack.Pop().GetBigInteger();
            
            return new NEP5TokenDetails
            {
                ScriptHash = nep5ScriptHash,
                Name = name,
                Decimals = decimals
            };
        }
    }
}
