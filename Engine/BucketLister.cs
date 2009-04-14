using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using DasBackupTool.Aws;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class BucketLister : IDisposable
    {
        private Files files;
        private BackupProgress backupProgress;
        private AbortableTaskExecutor executor;

        public BucketLister(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;

            executor = new AbortableTaskExecutor(ListBucket);
            Settings.Default.PropertyChanged += SettingsChanged;
        }

        public void Run()
        {
            executor.Run();
        }

        public void Dispose()
        {
            Settings.Default.PropertyChanged -= SettingsChanged;
            executor.Dispose();
        }

        private void ListBucket()
        {
            backupProgress.EnterAction(BackupAction.ListingBucket);
            try
            {
                files.RemoveRemoteFiles();
                if (Settings.Default.Bucket != null)
                {
                    try
                    {
                        S3 s3 = new S3(Settings.Default.Bucket.AccessKeyId, Settings.Default.Bucket.SecretAccessKey);
                        IDictionary<string, File.Attributes> remoteFiles = new Dictionary<string, File.Attributes>();
                        foreach (IS3Object file in s3.ListObjects(Settings.Default.Bucket.BucketName))
                        {
                            remoteFiles.Add(GetFileName(file.Key), new File.Attributes(file.Size, file.LastModified, file.ETag, null));
                            if (remoteFiles.Count == 1000)
                            {
                                files.AddRemoteFiles(remoteFiles);
                                remoteFiles.Clear();
                            }
                        }
                        files.AddRemoteFiles(remoteFiles);
                    }
                    catch (S3Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            finally
            {
                backupProgress.ExitAction(BackupAction.ListingBucket);
            }
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Bucket")
            {
                executor.Run();
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
