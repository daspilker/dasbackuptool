using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.S3;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class BackupEngine : IDisposable
    {
        private Files files;
        private BackupProgress backupProgress;
        private BackgroundTaskQueue queue = new BackgroundTaskQueue();

        public BackupEngine(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;
        }

        public void Run()
        {
            queue.Run();
            queue.Enqueue(Backup, null);
            backupProgress.AddStatus(BackupStatus.RunningBackup);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        private void Backup(object state)
        {
            ICredentials credentials = new Credentials(Settings.Default.Bucket.AccessKeyId, Settings.Default.Bucket.SecretAccessKey);
            IS3 s3 = new S3.S3();
            IEnumerable<IBucket> buckets = s3.ListBuckets(credentials);
            IBucket bucket = buckets.SingleOrDefault(b => b.Name == Settings.Default.Bucket.BucketName);
            if (bucket == null)
            {
                bucket = s3.CreateBucket(credentials, Settings.Default.Bucket.BucketName);
            }

            foreach (string file in files.NewOrUpdatedFiles)
            {
                using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    bucket.PutObject(credentials, GetObjectName(file), fileStream.Length, "application/octet-stream", fileStream);
                    files.FileBackedUp(file);
                }
                System.IO.File.SetAttributes(file, System.IO.File.GetAttributes(file) & ~System.IO.FileAttributes.Archive);
            }
            foreach (string file in files.DeletedFiles)
            {
                bucket.DeleteObject(credentials, GetObjectName(file));
                files.FileBackedUp(file);
            }
        }

        private static string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }
    }
}
