﻿using System;
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
        private string currentFile;
        private long currentFileSize;
        private long currentFileProgress;
        private long transferedFileCount;
        private long transferedData;
        private long totalFileCount;
        private long totalData;

        public BackupEngine(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;

            executor = new AbortableTaskExecutor(Backup);
            retryHelper = new RetryHelper();
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

        public string CurrentFile
        {
            get { return currentFile; }
            set
            {
                if (currentFile != value)
                {
                    currentFile = value;
                    NotifyPropertyChanged("CurrentFile");
                }
            }
        }

        public long CurrentFileSize
        {
            get { return currentFileSize; }
            set
            {
                if (currentFileSize != value)
                {
                    currentFileSize = value;
                    NotifyPropertyChanged("CurrentFileSize");
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
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Run()
        {
            TotalFileCount = files.LocalRepositoryStatistics.NewFileCount + files.LocalRepositoryStatistics.UpdatedFileCount;
            TotalData = files.LocalRepositoryStatistics.TransferFileSize;
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

                foreach (string file in files.NewOrUpdatedFiles)
                {
                    retryHelper.Retry(PutObject, s3, file);
                }
                foreach (string file in files.DeletedFiles)
                {
                    retryHelper.Retry(DeleteObject, s3, file);
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
            string file = (string)args[1];

            using (ProgressMonitoringFileStream fileStream = new ProgressMonitoringFileStream(file))
            {
                fileStream.PositionChanged += PositionChanged;
                CurrentFile = file;
                CurrentFileSize = fileStream.Length;
                s3.PutObject(Settings.Default.Bucket.BucketName, GetObjectName(file), fileStream.Length, "application/octet-stream", fileStream);
                files.FileBackedUp(file);
                TransferedFileCount++;
                TransferedData += fileStream.Length;
            }
            System.IO.File.SetAttributes(file, System.IO.File.GetAttributes(file) & ~System.IO.FileAttributes.Archive);
            CurrentFile = null;
            CurrentFileSize = 0;
            CurrentFileProgress = 0;
        }

        private void DeleteObject(params object[] args)
        {
            S3 s3 = (S3)args[0];
            string file = (string)args[1];

            CurrentFile = file;
            s3.DeleteObject(Settings.Default.Bucket.BucketName, GetObjectName(file));
            files.FileBackedUp(file);
            CurrentFile = null;
        }

        private void PositionChanged(object sender, long position)
        {
            CurrentFileProgress = position;
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private static string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }

        private class ProgressMonitoringFileStream : FileStream
        {
            public ProgressMonitoringFileStream(string file)
                : base(file, FileMode.Open, FileAccess.Read)
            {
            }

            public event PositionChangedEventHandler PositionChanged;

            public override int Read(byte[] array, int offset, int count)
            {
                int result = base.Read(array, offset, count);
                if (PositionChanged != null)
                {
                    PositionChanged(this, Position);
                }
                return result;
            }
        }

        public delegate void PositionChangedEventHandler(object sender, long position);
    }
}
