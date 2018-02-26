using System;
using System.Security.Cryptography.X509Certificates;
using Neo.UI.Core.Data.Enums;

namespace Neo.UI.Core.Data
{
    public class CertificateQueryResult : IDisposable
    {
        public CertificateQueryResultType Type { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public void Dispose()
        {
            this.Certificate?.Dispose();
        }
    }
}
