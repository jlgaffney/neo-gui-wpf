using System;
using System.IO;

namespace Neo
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var app = new App();
            app.Run();
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