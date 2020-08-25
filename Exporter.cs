using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CancelLongRunningOperation
{
    /// <summary>
    /// This class implements a long running operation executed in an own, cancelabled thread.
    /// </summary>
    public class Exporter
    {
        private Thread _doExportThread = null;
        private readonly object _doExportLock = new object();
        private bool _isCanceled = false;

        /// <summary>
        /// Signals that work with this class should be terminated. 
        /// The only way to set this signal is to call <see cref="Cancel(string, string)"/>.
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                lock (_doExportLock)
                {
                    return _isCanceled;
                }
            }
            private set
            {
                lock (_doExportLock)
                {
                    _isCanceled = value;
                }
            }
        }

        /// <summary>
        /// Simulates a long running operation, that uses <see cref="Cancel(string, string)"/> 
        /// after a random passes of loop.
        /// </summary>
        /// <param name="exportName">Name of the document to create, without extension.</param>
        /// <returns>The full name of the created document.</returns>
        public string DoExport(string exportName = "", int numberOfLoops = 10)
        {
            Random random = new Random();
            int cancelAtPass = random.Next(0, numberOfLoops);

            Console.WriteLine($"Export running. Cancel at {cancelAtPass + 1}");
            Console.Write("Progress: ");

            for (int ii = 0; ii < numberOfLoops; ii++)
            {
                Console.Write(".");
                Thread.Sleep(1000);

                //if (ii == cancelAtPass) Cancel($"Test abort at ii={ii + 1}");
            }

            Console.WriteLine("Done.");

            return string.IsNullOrWhiteSpace(exportName) ?
                $"{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.doc" :
                exportName.EndsWith(".doc") ? exportName : $"{exportName}.doc";
        }

        /// <summary>
        /// Starts <see cref="DoExport(string)"/> in an own thread. 
        /// This thread can be aborted by calling <see cref="Cancel(string, string)"/>.
        /// </summary>
        /// <param name="exportName">Name of the document to create, without extension.</param>
        /// <returns>The full name of the created document.</returns>
        public string DoExportInBackground(string exportName = "", int numberOfLoops = 10)
        {
            string returnValue = null;
            _doExportThread = new Thread(() =>
            {
                try
                {
                    returnValue = DoExport(exportName, numberOfLoops);
                }
                catch (ThreadAbortException ex)
                {
                    string notAvailable = "n/a";

                    IsCanceled = true;
                    _doExportThread = null;

                    if (ex.ExceptionState is CanceledEventArgs)
                    {
                        OnCanceled((CanceledEventArgs)ex.ExceptionState);
                    }
                    else
                    {
                        OnCanceled(new CanceledEventArgs() { CalledBy = notAvailable, Reason = notAvailable });
                    }

                }
            })
            {
                Name = "DoExportThread",
            };

            // in real life it is for our purpose
            _doExportThread.SetApartmentState(ApartmentState.STA);

            _doExportThread.Start();
            _doExportThread.Join();

            return returnValue;
        }

        /// <summary>
        /// Sets <see cref="IsCanceled"/> to true and fires the <see cref="OnCanceled(CanceledEventArgs)"/>-event 
        /// or aborts <see cref="DoExport(string)"/>. In case of aborting <see cref="DoExport(string)"/>, 
        /// <see cref="OnCanceled(CanceledEventArgs)"/> is called and <see cref="IsCanceled"/> is set in 
        /// <see cref="DoExportInBackground(string)"/>.
        /// </summary>
        /// <param name="reason">The reason for canceling.</param>
        /// <param name="calledBy">The name of the member whose calling cancel.</param>
        protected void Cancel(string reason, [CallerMemberName] string calledBy = "")
        {
            if (null != _doExportThread && _doExportThread.IsAlive)
            {
                _doExportThread.Abort(new CanceledEventArgs() { CalledBy = calledBy, Reason = reason });
            }
            else
            {
                _doExportThread = null;
                IsCanceled = true;
                OnCanceled(new CanceledEventArgs() { CalledBy = calledBy, Reason = reason });
            }
        }

        /// <summary>
        /// This event is fired, when <see cref="IsCanceled"/> is set to true.
        /// </summary>
        /// <param name="e">Information about the calling member and the reason for canceling</param>
        private void OnCanceled(CanceledEventArgs e)
        {
            Canceled?.Invoke(this, e);
        }

        /// <summary>
        /// This event is fired, when <see cref="IsCanceled"/> is set to true.
        /// </summary>
        public event EventHandler<CanceledEventArgs> Canceled;

        /// <summary>
        /// Information that is passed by when canceling
        /// </summary>
        public class CanceledEventArgs
        {
            /// <summary>
            /// Returns or sets the name of the calling member. Usually you don't have to set this 
            /// member, because ist is determined by <see cref="CallerMemberNameAttribute"/>
            /// </summary>
            public string CalledBy { get; set; }
            
            /// <summary>
            /// Returns or sets the reason for canceling
            /// </summary>
            public string Reason { get; set; }
        }
    }
}
