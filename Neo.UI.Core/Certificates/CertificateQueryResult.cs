using System;
using System.Security.Cryptography.X509Certificates;

namespace Neo.UI.Core.Certificates
{
    internal class CertificateQueryResult : IDisposable
    {
        public CertificateQueryResultType Type { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public void Dispose()
        {
            this.Certificate?.Dispose();
        }
    }
}
