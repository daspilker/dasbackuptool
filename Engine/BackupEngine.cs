using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using DasBackupTool.Util;
using DasBackupTool.Model;
using DasBackupTool.S3;

namespace DasBackupTool.Engine
{
    public class BackupEngine : IDisposable
    {
        private Configuration configuration;
        private Files files;
        private BackupProgress backupProgress;
        private BackgroundTaskQueue queue = new BackgroundTaskQueue();

        public BackupEngine(Configuration configuration, Files files, BackupProgress backupProgress)
        {
            this.configuration = configuration;
            this.files = files;
            this.backupProgress = backupProgress;
        }

        public void Run()
        {
            backupProgress.AddStatus(BackupStatus.RunningBackup);
            queue.Run();
            queue.Enqueue(Backup, null);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        private void Backup(object state)
        {
            ICredentials credentials = new Credentials(configuration.AccessKeyId, configuration.SecretKey);
            IS3 s3 = new S3.S3();
            IEnumerable<IBucket> buckets = s3.ListBuckets(credentials);
            IBucket bucket = buckets.SingleOrDefault(b => b.Name == configuration.Bucket);
            if (bucket == null)
            {
                bucket = s3.CreateBucket(credentials, configuration.Bucket);
            }

            foreach (string file in files.NewOrUpdatedFiles)
            {
                using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    bucket.PutObject(credentials, GetObjectName(file), fileStream.Length, "application/octet-stream", fileStream);
                    files.FileBackedUp(file);
                    backupProgress.FileBackedUp(fileStream.Length);
                }
            }
            foreach (string file in files.DeletedFiles)
            {
                bucket.DeleteObject(credentials, GetObjectName(file));
                files.FileBackedUp(file);
                backupProgress.FileBackedUp(0);
            }
        }

        private string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }
    }
}
