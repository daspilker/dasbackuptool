using System;
using System.Threading;
using DasBackupTool.Model;

namespace DasBackupTool.Engine
{
    public class StatisticsUpdater : IDisposable
    {
        private Files files;
        private Thread thread;
        private bool running;
        private bool updateStatistics;
        private object monitor = new object();

        public StatisticsUpdater(Files files)
        {
            this.files = files;
        }

        public void Run()
        {
            running = true;
            thread = new Thread(RunBackground);
            thread.Start();
            files.StatisticStale += StatisticStale;
        }

        public void Dispose()
        {
            files.StatisticStale -= StatisticStale;
            running = false;
            lock (monitor)
            {
                updateStatistics = false;
                Monitor.PulseAll(monitor);
            }
            thread.Join();
        }

        private void RunBackground()
        {
            while (running)
            {
                lock (monitor)
                {
                    updateStatistics = false;
                }
                files.UpdateStatistics();
                lock (monitor)
                {
                    if (!updateStatistics)
                    {
                        Monitor.Wait(monitor);
                    }
                }
            }
        }

        private void StatisticStale(object sender)
        {
            lock (monitor)
            {
                updateStatistics = true;
                Monitor.PulseAll(monitor);
            }
        }
    }
}
