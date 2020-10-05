using System;
using System.Threading;
using System.Threading.Tasks;

namespace CancelLongRunningOperation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string documentname = string.Empty;
            Exporter exporter = new Exporter();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var task = Task.Run(() =>
            {
                // Were we already canceled?
                cancellationToken.ThrowIfCancellationRequested();

                // Execute the cancelable long running operation
                documentname = exporter.DoExport(cancellationTokenSource, cancelAtPass: 5);

                // Poll on this property if you have to do other cleanup before throwing.
                if (cancellationToken.IsCancellationRequested)
                {
                    // Clean up here, then...
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }, cancellationTokenSource.Token); // Pass same token to Task.Run.

            // Await with try-catch:
            try
            {
                Console.WriteLine("Export started.");

                await task;

                Console.WriteLine($"Created document: {documentname}");
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Console.WriteLine("End with <Enter>");
            Console.ReadLine();
        }
    }
}
