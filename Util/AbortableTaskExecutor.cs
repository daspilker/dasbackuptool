using System;
using System.Threading;

namespace DasBackupTool.Util
{
    public class AbortableTaskExecutor : IDisposable
    {
        private object monitor = new object();
        private Thread thread;
        private ThreadStart task;

        public AbortableTaskExecutor(ThreadStart task)
        {
            this.task = task;
        }

        public void Run()
        {
            lock (monitor)
            {
                if (thread != null)
                {
                    Abort();
                }
                thread = new Thread(task);
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
                    thread.Abort();
                    thread.Join();
                    thread = null;
                }
            }
        }
    }
}
