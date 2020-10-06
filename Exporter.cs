using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CancelLongRunningOperation
{
    /// <summary>
    /// This class implements a long running, cancelable operation.
    /// </summary>
    public class Exporter
    {
        private readonly object _cancelLock = new object();
        private bool _isCanceled;

        /// <summary>
        /// Signals that work with this class should be terminated. 
        /// The only way to set this signal is to call <see cref="Cancel(string, string)"/>.
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                lock (_cancelLock)
                {
                    return _isCanceled;
                }
            }
            private set
            {
                lock (_cancelLock)
                {
                    if (_isCanceled != value)
                    {
                        _isCanceled = value;
                    }
                }
            }
        }

        /// <summary>
        /// Simulates a long running operation, that uses <see cref="Cancel(string, string)"/> 
        /// after a random passes of loop.
        /// </summary>
        /// <param name="exportName">Name of the document to create, without extension.</param>
        /// <param name="numberOfLoops">Count of loops to pass.</param>
        /// <param name="cancelAtPass">Number of the pass in which the process is to be canceled. 
        /// A value of -1 or higher <paramref name="numberOfLoops"/> prevents the cancellation.</param>
        /// <returns>The full name of the created document.</returns>
        public string DoExport(string exportName = "", int numberOfLoops = 10, int cancelAtPass = -1)
        {
            bool cancel = -1 < cancelAtPass && cancelAtPass <= numberOfLoops;

            Console.WriteLine($"Export running ({numberOfLoops} passes)." + (cancel ? $"\nCancel at {cancelAtPass}" : string.Empty));
            Console.Write("Progress: ");

            for (int ii = 0; ii < numberOfLoops; ii++)
            {
                Console.Write(".");
                Thread.Sleep(1000);

                if (cancel && ii == cancelAtPass - 1) Cancel($"Cancel signaled at pass {ii + 1}");
            }

            Console.WriteLine("Done.");

            return string.IsNullOrWhiteSpace(exportName) ?
                $"{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.doc" :
                exportName.EndsWith(".doc") ? exportName : $"{exportName}.doc";
        }

        /// <summary>
        /// Sets <see cref="IsCanceled"/> to true and fires an <see cref="OperationCanceledException"/>
        /// </summary>
        /// <param name="reason">The reason for canceling.</param>
        /// <param name="calledBy">The name of the member whose calling cancel.</param>
        protected void Cancel(string reason, [CallerMemberName] string calledBy = "")
        {
            IsCanceled = true;

            throw new OperationCanceledException(
                $"\nProcess aborted by {calledBy}." +
                (string.IsNullOrWhiteSpace(reason) ? string.Empty : $" {reason}"));
        }
    }
}
