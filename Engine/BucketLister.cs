using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.S3;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class BucketLister : IDisposable
    {
        private Files files;
        private BackupProgress backupProgress;
        private BackgroundTaskQueue queue = new BackgroundTaskQueue();

        public BucketLister(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;
        }

        public void Run()
        {
            queue.Run();
            queue.Enqueue(ListBucket, null);
            Settings.Default.PropertyChanged += SettingsChanged;
        }

        public void Dispose()
        {
            Settings.Default.PropertyChanged -= SettingsChanged;
            queue.Dispose();
        }

        private void ListBucket(object state)
        {
            if (state == null)
            {
                backupProgress.AddStatus(BackupStatus.ListingBucket);
                files.RemoveRemoteFiles();
            }
            string marker = null;
            if (Settings.Default.Bucket != null)
            {
                DasBackupTool.S3.Bucket bucket = new DasBackupTool.S3.Bucket(Settings.Default.Bucket.BucketName);
                try
                {
                    IDictionary<string, FileAttributes> remoteFiles = new Dictionary<string, FileAttributes>();
                    foreach (IObject file in bucket.ListObjects(new Credentials(Settings.Default.Bucket.AccessKeyId, Settings.Default.Bucket.SecretAccessKey), (string)state))
                    {
                        remoteFiles.Add(GetFileName(file.Key), new FileAttributes(file.Size, file.LastModified, file.ETag, null));
                        marker = file.Key;
                    }
                    files.AddRemoteFiles(remoteFiles);
                }
                catch (S3Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            if (marker == null)
            {
                backupProgress.RemoveStatus(BackupStatus.ListingBucket);
            }
            else
            {
                queue.Enqueue(ListBucket, marker);
            }
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Bucket")
            {
                queue.Clear();
                queue.Enqueue(ListBucket, null);
            }
        }
        
        private static string GetFileName(string objectKey)
        {
            objectKey = objectKey.Replace('/', '\\');
            objectKey = objectKey.Insert(objectKey.IndexOf('\\'), ":");
            return objectKey;
        }
    }
}
