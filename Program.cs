using System;

namespace CancelLongRunningOperation
{
    class Program
    {
        static void Main(string[] args)
        {
            StartExporter();
        }

        private static void StartExporter()
        {
            Exporter exporter = new Exporter();
            exporter.Canceled += Exporter_Canceled;

            Console.WriteLine(exporter.DoExportInBackground());
        }

        private static void Exporter_Canceled(object sender, Exporter.CanceledEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine($"Export canceled from {sender}\nWhile executing {e.CalledBy}\nReason: '{e.Reason}'");
        }
    }
}
