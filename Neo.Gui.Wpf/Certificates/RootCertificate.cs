using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.Certificates
{
    public static class RootCertificate
    {
        private static readonly byte[] OnchainRootCertificate = Resources.OnchainCertificate;

        public static bool Install()
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            using (var cert = new X509Certificate2(OnchainRootCertificate))
            {
                // Check if certificate is already installed
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Contains(cert)) return true;

                // Certificate is not installed

                // Close store so it can be re-opened in read-write mode
                store.Close();

                // Install certificate
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    return true;
                }
                catch (CryptographicException)
                {
                    // TODO Log exception somewhere
                }

                // Root certificate installation failed

                try
                {
                    // Try running application as administrator to install root certificate

                    // TODO Stop this application instance

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Assembly.GetExecutingAssembly().Location,
                        UseShellExecute = true,
                        Verb = "runas",
                        WorkingDirectory = Environment.CurrentDirectory
                    });
                }
                catch (Win32Exception)
                {
                    // TODO Log exception somewhere
                }

                return false;
            }
        }
    }
}