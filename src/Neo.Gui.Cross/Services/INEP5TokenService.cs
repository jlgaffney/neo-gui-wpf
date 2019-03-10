using Neo.Gui.Cross.Models;

namespace Neo.Gui.Cross.Services
{
    public interface INEP5TokenService
    {
        NEP5TokenDetails GetTokenDetails(UInt160 nep5ScriptHash);
    }
}
