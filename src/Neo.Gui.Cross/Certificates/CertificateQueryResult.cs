using System;
using System.Security.Cryptography.X509Certificates;

namespace Neo.Gui.Cross.Certificates
{
    public class CertificateQueryResult : IDisposable
    {
        public CertificateQueryResultType Type;
        public X509Certificate2 Certificate;

        public void Dispose()
        {
            Certificate?.Dispose();
        }
    }
}
