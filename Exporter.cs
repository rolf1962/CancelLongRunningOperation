using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CancelLongRunningOperation
{
    public class Exporter
    {
        private Thread _doExportThread = null;

        private readonly object _doExportLock = new object();
        private bool _isCanceled = false;

        public bool IsCanceled
        {
            get
            {
                lock (_doExportLock)
                {
                    return _isCanceled;
                }
            }
            set
            {
                lock (_doExportLock)
                {
                    _isCanceled = value;
                }
            }
        }

        public string DoExport(string exportName = "")
        {
            Console.WriteLine("Export running.");
            Console.Write("Progress: ");

            for (int ii = 0; ii < 10; ii++)
            {
                Console.Write(".");
                Thread.Sleep(1000);

                if (ii == 2) Cancel($"Test abort at ii={ii}");
            }

            Console.WriteLine("Done.");

            return string.IsNullOrWhiteSpace(exportName) ?
                DateTime.Now.ToString("yyyyMMdd-hhmmss.doc") :
                exportName.EndsWith(".doc") ? exportName : $"{exportName}.doc";
        }

        public string DoExportInBackground(string exportName = "")
        {
            string returnValue = null;
            _doExportThread = new Thread(() =>
            {
                try
                {
                    returnValue = DoExport(exportName);
                }
                catch (ThreadAbortException ex)
                {
                    string notAvailable = "n/a";

                    IsCanceled = true;

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

            _doExportThread.SetApartmentState(ApartmentState.STA);

            _doExportThread.Start();

            return returnValue;
        }

        protected void Cancel(string reason, [CallerMemberName] string calledBy = "")
        {
            if (null != _doExportThread)
            {
                _doExportThread.Abort(new CanceledEventArgs() { CalledBy = calledBy, Reason = reason });
            }
        }

        private void OnCanceled(CanceledEventArgs e)
        {
            Canceled?.Invoke(this, e);
        }

        public event EventHandler<CanceledEventArgs> Canceled;

        public class CanceledEventArgs
        {
            public string CalledBy { get; set; }
            public string Reason { get; set; }
        }
    }
}
