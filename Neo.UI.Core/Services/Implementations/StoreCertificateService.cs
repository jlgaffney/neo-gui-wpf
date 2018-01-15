using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    public class StoreCertificateService : IStoreCertificateService
    {
        #region IStoreCertificaService Implementation 
        public IEnumerable<X509Certificate2> GetStoreCertificates()
        {
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates
                    .Cast<X509Certificate2>()
                    .ToList();
            }
        }
        #endregion
    }
}
