using System;
using System.ComponentModel;
using System.IO;
using DasBackupTool.Aws;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class BackupEngine : IDisposable, INotifyPropertyChanged
    {
        private Files files;
        private BackupProgress backupProgress;
        private AbortableTaskExecutor executor;
        private RetryHelper retryHelper;
        private BackupFile currentFile;
        private long currentFileProgress;
        private long transferedFileCount;
        private long transferedData;
        private long totalFileCount;
        private long totalData;
        private long failedFileCount;
        private long failedData;
        private DateTime currentFileStartTime;
        private DateTime startTime;

        public BackupEngine(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;

            executor = new AbortableTaskExecutor(Backup);
            retryHelper = new RetryHelper();
            retryHelper.ExceptionOccured += ExceptionOccured;
        }

        public long TransferedFileCount
        {
            get { return transferedFileCount; }
            set
            {
                if (transferedFileCount != value)
                {
                    transferedFileCount = value;
                    NotifyPropertyChanged("TransferedFileCount");
                }
            }
        }

        public long TransferedData
        {
            get { return transferedData; }
            set
            {
                if (transferedData != value)
                {
                    transferedData = value;
                    CurrentFileProgress = 0;
                    NotifyPropertyChanged("TransferedData");
                }
            }
        }

        public long TotalFileCount
        {
            get { return totalFileCount; }
            set
            {
                if (totalFileCount != value)
                {
                    totalFileCount = value;
                    NotifyPropertyChanged("TotalFileCount");
                }
            }
        }

        public long TotalData
        {
            get { return totalData; }
            set
            {
                if (totalData != value)
                {
                    totalData = value;
                    NotifyPropertyChanged("TotalData");
                }
            }
        }

        public long FailedFileCount
        {
            get { return failedFileCount; }
            set
            {
                if (failedFileCount != value)
                {
                    failedFileCount = value;
                    NotifyPropertyChanged("FailedFileCount");
                }
            }
        }

        public long FailedData
        {
            get { return failedData; }
            set
            {
                if (failedData != value)
                {
                    failedData = value;
                    NotifyPropertyChanged("FailedData");
                    NotifyPropertyChanged("TimeLeft");
                }
            }
        }

        public BackupFile CurrentFile
        {
            get { return currentFile; }
            set
            {
                if (currentFile != value)
                {
                    currentFile = value;
                    NotifyPropertyChanged("CurrentFile");
                    CurrentFileProgress = 0;
                    currentFileStartTime = DateTime.Now;
                }
            }
        }

        public long CurrentFileProgress
        {
            get { return currentFileProgress; }
            set
            {
                if (currentFileProgress != value)
                {
                    currentFileProgress = value;
                    NotifyPropertyChanged("CurrentFileProgress");
                    NotifyPropertyChanged("CurrentTransferedData");
                    NotifyPropertyChanged("CurrentFileTimeLeft");
                    NotifyPropertyChanged("TimeLeft");
                }
            }
        }

        public long CurrentTransferedData
        {
            get { return transferedData + currentFileProgress; }
        }

        public TimeSpan CurrentFileTimeLeft
        {
            get
            {
                return currentFile == null ? TimeSpan.Zero : EstimateTimeLeft(currentFileStartTime, currentFileProgress, currentFile.LocalAttributes.Size);
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return EstimateTimeLeft(startTime, CurrentTransferedData, totalData - failedData);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Run()
        {
            TotalFileCount = files.LocalRepositoryStatistics.NewFileCount + files.LocalRepositoryStatistics.UpdatedFileCount;
            TotalData = files.LocalRepositoryStatistics.TransferFileSize;
            startTime = DateTime.Now;
            executor.Run();
        }

        public void Cancel()
        {
            executor.Abort();

            CurrentFile = null;
            TransferedData = 0;
            transferedFileCount = 0;
        }

        public void Dispose()
        {
            executor.Dispose();
        }

        private void Backup()
        {
            backupProgress.EnterAction(BackupAction.RunningBackup);
            try
            {
                S3 s3 = new S3(Settings.Default.Bucket.AccessKeyId, Settings.Default.Bucket.SecretAccessKey);

                foreach (BackupFile file in files.NewOrUpdatedFiles)
                {
                    executor.CheckAbortion();
                    bool success = retryHelper.Retry(PutObject, s3, file);
                    if (success)
                    {
                        files.FileBackedUp(file.Path);
                        TransferedFileCount++;
                        TransferedData += file.LocalAttributes.Size;
                        File.SetAttributes(file.Path, File.GetAttributes(file.Path) & ~FileAttributes.Archive);
                    }
                    else
                    {
                        FailedFileCount++;
                    }
                }
                foreach (BackupFile file in files.DeletedFiles)
                {
                    executor.CheckAbortion();
                    bool success = retryHelper.Retry(DeleteObject, s3, file);
                    if (success)
                    {
                        files.FileBackedUp(file.Path);
                    }
                    else
                    {
                        FailedFileCount++;
                    }
                }
            }
            finally
            {
                backupProgress.ExitAction(BackupAction.RunningBackup);
            }
        }

        private void PutObject(params object[] args)
        {
            S3 s3 = (S3)args[0];
            BackupFile file = (BackupFile)args[1];

            CurrentFile = file;
            using (ProgressMonitoringFileStream fileStream = new ProgressMonitoringFileStream(file.Path))
            {
                fileStream.ProgressChanged += ProgressChanged;
                s3.PutObject(Settings.Default.Bucket.BucketName, GetObjectName(file.Path), fileStream, file.LocalAttributes.Size, file.ContentType);
            }
        }

        private void DeleteObject(params object[] args)
        {
            S3 s3 = (S3)args[0];
            BackupFile file = (BackupFile)args[1];

            CurrentFile = file;
            s3.DeleteObject(Settings.Default.Bucket.BucketName, GetObjectName(file.Path));
        }

        private void ExceptionOccured(object sender, ExceptionOccuredEventArgs e)
        {
            e.Retry = e.RetryCount < 2;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentFileProgress = (long)e.UserState;
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private TimeSpan EstimateTimeLeft(DateTime startTime, long currentData, long totalData)
        {
            if (currentData > 0)
            {
                TimeSpan totalElapsed = DateTime.Now - startTime;
                TimeSpan totalEstimated = TimeSpan.FromMilliseconds(totalElapsed.TotalMilliseconds * totalData / currentData);
                return totalEstimated - totalElapsed;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private static string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }

        private class ProgressMonitoringFileStream : FileStream
        {
            public ProgressMonitoringFileStream(string file) : base(file, FileMode.Open, FileAccess.Read) { }

            public event ProgressChangedEventHandler ProgressChanged;

            public override int Read(byte[] array, int offset, int count)
            {
                int result = base.Read(array, offset, count);
                if (ProgressChanged != null)
                {
                    ProgressChanged(this, new ProgressChangedEventArgs((int)(Position * 100 / Length), Position));
                }
                return result;
            }
        }
    }
}
