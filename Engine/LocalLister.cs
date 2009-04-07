using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.Util;
using System.Threading;

namespace DasBackupTool.Engine
{
    public class LocalLister : IDisposable
    {
        private Files files;
        private BackupProgress backupProgress;
        private Thread thread;
        private object monitor = new object();
        IDictionary<string, DasBackupTool.Model.FileAttributes> localFiles = new Dictionary<string, DasBackupTool.Model.FileAttributes>();

        public LocalLister(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;
            Settings.Default.PropertyChanged += SettingsChanged;
        }

        public void Run()
        {
            lock (monitor)
            {
                if (thread != null)
                {
                    Abort();
                }
                thread = new Thread(ListBackupLocations);
                thread.Start();
            }
        }

        public void Dispose()
        {
            Abort();
            Settings.Default.PropertyChanged -= SettingsChanged;
        }

        private void Abort()
        {
            lock (monitor)
            {
                if (thread != null)
                {
                    thread.Abort();
                    thread.Join();
                    thread = null;
                }
            }
        }

        private void ListBackupLocations()
        {
            backupProgress.AddStatus(BackupStatus.ListingLocal);
            try
            {
                files.RemoveLocalFiles();
                localFiles.Clear();
                if (Settings.Default.BackupLocations != null)
                {
                    foreach (BackupLocation backupLocation in Settings.Default.BackupLocations)
                    {
                        try
                        {
                            if (DasBackupTool.Util.File.IsDirectory(backupLocation.Path))
                            {
                                ListDirectory(new DirectoryInfo(backupLocation.Path));
                            }
                            else
                            {
                                ListFile(new FileInfo(backupLocation.Path));
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            // todo
                        }
                    }
                    CommitFiles();
                }
            }
            finally
            {
                backupProgress.RemoveStatus(BackupStatus.ListingLocal);
            }
        }

        private void ListDirectory(DirectoryInfo directory)
        {
            try
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    ListDirectory(subDirectory);
                }
                foreach (FileInfo file in directory.GetFiles())
                {
                    ListFile(file);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // todo
            }
        }

        private void ListFile(FileInfo file)
        {
            localFiles.Add(file.FullName, GetFileAttributes(file));
            if (localFiles.Count >= 1000)
            {
                CommitFiles();
            }
        }

        private void CommitFiles()
        {
            files.AddLocalFiles(localFiles);
            localFiles.Clear();
        }

        private DasBackupTool.Model.FileAttributes GetFileAttributes(FileInfo file)
        {
            return new DasBackupTool.Model.FileAttributes(file.Length, file.LastWriteTimeUtc, null, (file.Attributes & System.IO.FileAttributes.Archive) == System.IO.FileAttributes.Archive);
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackupLocations")
            {
                Run();
            }
        }
    }
}
