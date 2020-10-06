using System;
using System.Threading.Tasks;

namespace CancelLongRunningOperation
{
    class Program
    {
        private static int CancelAtPass { get; } = 2;
        private static int NumberOfLoops { get; } = 3;
        private static Exporter Exporter { get; } = new Exporter();

        static async Task Main(string[] args)
        {
            string documentname = string.Empty;

            Console.WriteLine($"Export startet with {nameof(StartAsync)}");
            documentname = await StartAsync();
            Console.Write("\n\n");

            Console.WriteLine($"Export startet with {nameof(Start)}");
            documentname = Start();
            Console.Write("\n\n");

            Console.Write("Press any key . . .");
            Console.ReadKey();
        }

        private static async Task<string> StartAsync()
        {
            string documentname = string.Empty;

            var task = Task.Run(() =>
            {
                documentname = Exporter.DoExport(numberOfLoops: NumberOfLoops, cancelAtPass: CancelAtPass);

            });

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }

            Console.WriteLine($"Created document: {documentname}");
            
            return documentname;
        }

        public static string Start()
        {
            string documentname = string.Empty;
            try
            {
                documentname = Exporter.DoExport(numberOfLoops: NumberOfLoops, cancelAtPass: CancelAtPass);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }

            Console.WriteLine($"Created document: {documentname}");

            return documentname;
        }
    }
}
