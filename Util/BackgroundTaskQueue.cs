using System;
using System.Collections.Generic;
using System.Threading;

namespace DasBackupTool.Util
{
    public delegate void BackgroundTask(object state);

    public delegate void QueueEmptyEventHandler(object sender);

    public class BackgroundTaskQueue : IDisposable
    {
        private Queue<BackgroundTaskAndState> tasks = new Queue<BackgroundTaskAndState>();
        private Thread thread;
        private bool shutDown = false;

        public int Count
        {
            get { return tasks.Count; }
        }

        public void Run()
        {
            thread = new Thread(RunThread);
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        public void Dispose()
        {
            shutDown = true;
            lock (tasks)
            {
                Monitor.PulseAll(tasks);
            }
            if (thread != null && !thread.Join(2000))
            {
                thread.Abort();
            }

        }

        public void Enqueue(BackgroundTask task, object state)
        {
            lock (tasks)
            {
                tasks.Enqueue(new BackgroundTaskAndState(task, state));
                Monitor.PulseAll(tasks);
            }
        }

        public void Clear()
        {
            lock (tasks)
            {
                tasks.Clear();
            }
        }

        public event QueueEmptyEventHandler QueueEmpty;

        void RunThread()
        {
            while (!shutDown)
            {
                BackgroundTaskAndState taskAndState = null;
                lock (tasks)
                {
                    if (tasks.Count > 0)
                    {                        
                        taskAndState = tasks.Dequeue();
                    }
                    else
                    {
                        NotifyQueueEmpty();
                    }
                }
                if (taskAndState != null)
                {
                    taskAndState.Task(taskAndState.State);
                }
                else
                {
                    lock (tasks)
                    {
                        Monitor.Wait(tasks);
                    }
                }
            }
        }

        private void NotifyQueueEmpty()
        {
            if (QueueEmpty != null)
            {
                QueueEmpty(this);
            }
        }
    }

    public class BackgroundTaskAndState
    {
        private BackgroundTask task;
        private object state;

        public BackgroundTaskAndState(BackgroundTask task, object state)
        {
            this.task = task;
            this.state = state;
        }

        public BackgroundTask Task
        {
            get { return task; }
        }

        public object State
        {
            get { return state; }
        }
    }
}
