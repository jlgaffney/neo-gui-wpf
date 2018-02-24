using Neo.Wallets;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface ICertificateRequestService
    {
        byte[] Request(KeyPair key, string cn, string c, string s, string serialNumber);
    }
}
