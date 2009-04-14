using System;
using System.IO;
using DasBackupTool.Aws;
using DasBackupTool.Model;
using DasBackupTool.Properties;
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

        private static string GetObjectName(string fileName)
        {
            return fileName.Replace('\\', '/').Replace(":", "");
        }

        private void PutObject(params object[] args)
        {
            S3 s3 = (S3)args[0];
            string file = (string)args[1];

            using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                s3.PutObject(Settings.Default.Bucket.BucketName, GetObjectName(file), fileStream.Length, "application/octet-stream", fileStream);
                files.FileBackedUp(file);
            }
            System.IO.File.SetAttributes(file, System.IO.File.GetAttributes(file) & ~System.IO.FileAttributes.Archive);
        }

        private void DeleteObject(params object[] args)
        {
            S3 s3 = (S3)args[0];
            string file = (string)args[1];

            s3.DeleteObject(Settings.Default.Bucket.BucketName, GetObjectName(file));
            files.FileBackedUp(file);
        }
    }
}
