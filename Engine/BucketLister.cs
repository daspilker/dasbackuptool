using System;
using System.ComponentModel;
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
            backupProgress.AddStatus(BackupStatus.ListingBucket);
            files.RemoveRemoteFiles();
            if (Settings.Default.Bucket != null)
            {
                IBucket bucket = new DasBackupTool.S3.Bucket(Settings.Default.Bucket.BucketName);
                try
                {
                    foreach (IObject file in bucket.ListObjects(new Credentials(Settings.Default.Bucket.AccessKeyId, Settings.Default.Bucket.SecretAccessKey)))
                    {
                        files.AddRemoteFile(GetFileName(file.Key), file.Size, file.LastModified, file.ETag);
                    }
                }
                catch (S3Exception)
                {
                    // todo
                }
            }
            backupProgress.RemoveStatus(BackupStatus.ListingBucket);
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Bucket")
            {
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
