using System;
using System.IO;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Network;
using Neo.Properties;

namespace Neo
{
    internal static class Program
    {
        public static LocalNode LocalNode;

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Check if application needs updating
            if (VersionHelper.UpdateIsRequired)
            {
                // Prevent GUI from starting normally until it has been updated
                var app = new App(true);
                app.Run();
                return;
            }

            // Install root certificate
            if (!RootCertificate.InstallCertificate()) return;

            // Try load peer state
            PeerState.TryLoad();

            using (Blockchain.RegisterBlockchain(new LevelDBBlockchain(Settings.Default.DataDirectoryPath))) // Setup blockchain
            using (LocalNode = new LocalNode()) // Setup node
            {
                LocalNode.UpnpEnabled = true;

                ApplicationContext.Instance.LocalNode = LocalNode;

                // Start GUI normally
                var app = new App();
                app.Run();
            }

            // Save peer state
            PeerState.Save();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var fileStream = new FileStream("error.log", FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fileStream))
            {
                PrintErrorLogs(writer, (Exception)e.ExceptionObject);
            }
        }

        private static void PrintErrorLogs(StreamWriter writer, Exception ex)
        {
            writer.WriteLine(ex.GetType());
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);

            // Print inner exceptions if there are any
            if (ex is AggregateException ex2)
            {
                foreach (var innerException in ex2.InnerExceptions)
                {
                    writer.WriteLine();
                    PrintErrorLogs(writer, innerException);
                }
            }
            else if (ex.InnerException != null)
            {
                writer.WriteLine();
                PrintErrorLogs(writer, ex.InnerException);
            }
        }
    }
}