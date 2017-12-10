using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf
{
    public static class RootCertificate
    {
        private static readonly byte[] OnchainRootCertificate = Resources.OnchainCertificate;

        public static bool IsInstalled
        {
            get
            {
                using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                using (var cert = new X509Certificate2(OnchainRootCertificate))
                {
                    store.Open(OpenFlags.ReadOnly);

                    return store.Certificates.Contains(cert);
                }
            }
        }

        public static bool Install()
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            using (var cert = new X509Certificate2(OnchainRootCertificate))
            {
                store.Open(OpenFlags.ReadWrite);
                
                // Check if certificate is already installed
                if (store.Certificates.Contains(cert)) return true;

                // Certificate is not installed

                // Try install certificate
                try
                {
                    store.Add(cert);

                    // Installation succeeded
                    return true;
                }
                catch (CryptographicException)
                {
                    // TODO Log exception somewhere

                    // Installation failed
                    return false;
                }
            }
        }
    }
}
