using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.Util;

namespace DasBackupTool.Engine
{
    public class LocalLister : IDisposable
    {
        private Files files;
        private BackupProgress backupProgress;
        private IDictionary<string, BackupFileAttributes> localFiles = new Dictionary<string, BackupFileAttributes>();
        private AbortableTaskExecutor executor;

        public LocalLister(Files files, BackupProgress backupProgress)
        {
            this.files = files;
            this.backupProgress = backupProgress;

            executor = new AbortableTaskExecutor(ListBackupLocations);
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

        private void ListBackupLocations()
        {
            backupProgress.EnterAction(BackupAction.ListingLocal);
            try
            {
                files.RemoveLocalFiles();
                localFiles.Clear();
                if (Settings.Default.BackupLocations != null)
                {
                    files.TrackStatisticsFor(Settings.Default.BackupLocations.IncludedLocations);
                    foreach (BackupLocation backupLocation in Settings.Default.BackupLocations.IncludedLocations)
                    {
                        executor.CheckAbortion();
                        try
                        {
                            if (FileUtils.IsDirectory(backupLocation.Path))
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
                backupProgress.ExitAction(BackupAction.ListingLocal);
            }
        }

        private void ListDirectory(DirectoryInfo directory)
        {
            try
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    executor.CheckAbortion();
                    if (!Settings.Default.BackupLocations.IsExcluded(subDirectory.FullName))
                    {
                        ListDirectory(subDirectory);
                    }
                }
                foreach (FileInfo file in directory.GetFiles())
                {
                    executor.CheckAbortion();
                    if (!Settings.Default.BackupLocations.IsExcluded(file.FullName))
                    {
                        ListFile(file);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ListFile(FileInfo file)
        {
            localFiles.Add(file.FullName, new BackupFileAttributes(file.Length, file.LastWriteTimeUtc, null, FileUtils.IsArchive(file.FullName)));
            if (localFiles.Count == 1000)
            {
                CommitFiles();
            }
        }

        private void CommitFiles()
        {
            files.AddLocalFiles(localFiles);
            localFiles.Clear();
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
