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

            Console.WriteLine("Export started.");
            Console.WriteLine($"Created document: {exporter.DoExportInBackground()}");

            Console.WriteLine("End with <Enter>");
            Console.ReadLine();
        }

        private static void Exporter_Canceled(object sender, Exporter.CanceledEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine($"Export canceled from {sender}.{e.CalledBy}");
            Console.WriteLine($"Reason: {e.Reason}");
        }
    }
}
