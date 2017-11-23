using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Neo.Properties;

namespace Neo
{
    public static class RootCertificate
    {
        public static bool InstallRootCertificate()
        {
            if (!Settings.Default.InstallCertificate) return true;

            using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            using (var cert = new X509Certificate2(Resources.OnchainCertificate))
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

                if (MessageBox.Show(Strings.InstallCertificateText, Strings.InstallCertificateCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
                {
                    Settings.Default.InstallCertificate = false;
                    Settings.Default.Save();
                    return true;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Assembly.GetExecutingAssembly().Location,
                        UseShellExecute = true,
                        Verb = "runas",
                        WorkingDirectory = Environment.CurrentDirectory
                    });
                    return false;
                }
                catch (Win32Exception)
                {
                    // TODO Log exception somewhere
                }
                MessageBox.Show(Strings.InstallCertificateCancel);
                return true;
            }
        }
    }
}