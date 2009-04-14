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
        private AbortableTaskExecutor executor;
        private RetryHelper retryHelper;

        public BackupEngine(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;

            executor = new AbortableTaskExecutor(Backup);
            retryHelper = new RetryHelper();
        }

        public void Run()
        {
            executor.Run();
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
                    retryHelper.Retry(PutObject, bucket, credentials, file);
                }
                foreach (string file in files.DeletedFiles)
                {
                    retryHelper.Retry(DeleteObject, bucket, credentials, file);
                }
            }
            finally
            {
                backupProgress.ExitAction(BackupAction.RunningBackup);
            }
        }

        private static string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }

        private void PutObject(params object[] args)
        {
            IBucket bucket = (IBucket)args[0];
            ICredentials credentials = (ICredentials)args[1];
            string file = (string)args[2];

            using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                bucket.PutObject(credentials, GetObjectName(file), fileStream.Length, "application/octet-stream", fileStream);
                files.FileBackedUp(file);
            }
            System.IO.File.SetAttributes(file, System.IO.File.GetAttributes(file) & ~System.IO.FileAttributes.Archive);
        }

        private void DeleteObject(params object[] args)
        {
            IBucket bucket = (IBucket)args[0];
            ICredentials credentials = (ICredentials)args[1];
            string file = (string)args[2];

            bucket.DeleteObject(credentials, GetObjectName(file));
            files.FileBackedUp(file);
        }
    }
}
