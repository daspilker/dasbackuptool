using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DasBackupTool.Model;
using DasBackupTool.S3;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class BucketLister : IDisposable
    {
        private Configuration configuration;
        private Files files;
        private BackupProgress backupProgress;
        private BackgroundTaskQueue queue = new BackgroundTaskQueue();

        public BucketLister(Configuration configuration, Files files, BackupProgress backupProgress)
        {
            this.configuration = configuration;
            this.files = files;
            this.backupProgress = backupProgress;
        }

        public void Run()
        {
            backupProgress.AddStatus(BackupStatus.ListingBucket);
            queue.Run();
            queue.Enqueue(ListBucket, null);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        private void ListBucket(object state)
        {
            IBucket bucket = new Bucket(configuration.Bucket);
            try
            {
                foreach (IObject file in bucket.ListObjects(new Credentials(configuration.AccessKeyId, configuration.SecretKey)))
                {
                    files.AddRemoteFile(GetFileName(file.Key), file.Size, file.LastModified, file.ETag);
                }
            }
            catch (S3Exception)
            {                
                // todo
            }
            backupProgress.RemoveStatus(BackupStatus.ListingBucket);
        }

        private string GetFileName(string objectKey)
        {
            objectKey = objectKey.Replace('/', '\\');
            objectKey = objectKey.Insert(objectKey.IndexOf('\\'), ":");
            return objectKey;
        }
    }
}
