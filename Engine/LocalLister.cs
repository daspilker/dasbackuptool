using System;
using System.IO;
using DasBackupTool.Model;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class LocalLister : IDisposable
    {
        private Configuration configuration;
        private Files files;
        private BackupProgress backupProgress;
        private BackgroundTaskQueue queue = new BackgroundTaskQueue();

        public LocalLister(Configuration configuration, Files files, BackupProgress backupProgress)
        {
            this.configuration = configuration;
            this.files = files;
            this.backupProgress = backupProgress;
            queue.QueueEmpty += QueueEmpty;
        }

        public void Run()
        {
            backupProgress.AddStatus(BackupStatus.ListingLocal);
            queue.Run();
            queue.Enqueue(ListBackupLocations, null);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        private void ListBackupLocations(object state)
        {
            foreach (BackupLocation backupLocation in configuration.BackupLocations)
            {
                if (IsDirectory(backupLocation.Path))
                {
                    queue.Enqueue(ListDirectory, new DirectoryInfo(backupLocation.Path));
                }
                else
                {
                    queue.Enqueue(ListFile, new FileInfo(backupLocation.Path));
                }
            }
        }

        private void ListDirectory(object state)
        {
            DirectoryInfo directory = (DirectoryInfo)state;
            try
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    queue.Enqueue(ListDirectory, subDirectory);
                }
                foreach (FileInfo file in directory.GetFiles())
                {
                    queue.Enqueue(ListFile, file);
                }
            }
            catch (UnauthorizedAccessException)
            { 
                // todo
            }
        }

        private void ListFile(object state)
        {
            FileInfo file = (FileInfo)state;
            files.AddLocalFile(file.FullName, file.Length, file.LastWriteTimeUtc, null);
        }

        private void QueueEmpty(object sender)
        {
            backupProgress.RemoveStatus(BackupStatus.ListingLocal);
        }

        private bool IsDirectory(string path)
        {
            return (System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }
    }
}
