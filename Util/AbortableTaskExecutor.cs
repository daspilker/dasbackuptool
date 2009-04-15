using System;
using System.Threading;

namespace DasBackupTool.Util
{
    public class AbortableTaskExecutor : IDisposable
    {
        private object monitor = new object();
        private Thread thread;
        private ThreadStart task;
        private bool abort;

        public AbortableTaskExecutor(ThreadStart task)
        {
            this.task = task;
        }

        public bool IsAborted
        {
            get { return abort; }
        }

        public void Run()
        {
            lock (monitor)
            {
                if (thread != null)
                {
                    Abort();
                }
                abort = false;
                thread = new Thread(RunTask);
                thread.Start();
            }
        }

        public void Dispose()
        {
            Abort();
        }

        public void Abort()
        {
            lock (monitor)
            {
                if (thread != null)
                {
                    abort = true;
                    if (!thread.Join(500))
                    {
                        thread.Abort();
                        thread.Join();
                    }
                    thread = null;
                }
            }
        }

        public void CheckAbortion()
        {
            if (abort)
            {
                throw new AbortException();
            }
        }

        private void RunTask()
        {
            try
            {
                task();
            }
            catch (AbortException) { }
            catch (ThreadAbortException) { }
        }
    }

    public class AbortException : Exception { }
}
