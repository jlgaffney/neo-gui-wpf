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
        public static bool InstallCertificate()
        {
            if (!Settings.Default.InstallCertificate) return true;
            using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            using (var cert = new X509Certificate2(Resources.OnchainCertificate))
            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Contains(cert)) return true;
            }
            using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            using (var cert = new X509Certificate2(Resources.OnchainCertificate))
            {
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    return true;
                }
                catch (CryptographicException) { }
                if (MessageBox.Show(Strings.InstallCertificateText, Strings.InstallCertificateCaption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
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
                catch (Win32Exception) { }
                MessageBox.Show(Strings.InstallCertificateCancel);
                return true;
            }
        }
    }
}