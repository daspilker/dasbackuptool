using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using DasBackupTool.Properties;
using DasBackupTool.Util;

namespace DasBackupTool.Model
{
    public class Files : INotifyPropertyChanged
    {
        private RepositoryStatistics localRepositoryStatistics = new RepositoryStatistics();
        private RepositoryStatistics remoteRepositoryStatistics = new RepositoryStatistics();
        private IDictionary<string, BackupFile> files = new Dictionary<string, BackupFile>();
        private ICollection<BackupLocationStatistics> backupLocationStatistics = new HashSet<BackupLocationStatistics>();

        public RepositoryStatistics LocalRepositoryStatistics
        {
            get { return localRepositoryStatistics; }
        }

        public RepositoryStatistics RemoteRepositoryStatistics
        {
            get { return remoteRepositoryStatistics; }
        }

        public ICollection<BackupLocationStatistics> BackupLocationStatistics
        {
            get { return backupLocationStatistics; }
        }

        public IEnumerable<BackupFile> NewOrUpdatedFiles
        {
            get { return files.Values.Where(f => f.Status == BackupFileStatus.New || f.Status == BackupFileStatus.Updated); }
        }

        public IEnumerable<BackupFile> DeletedFiles
        {
            get { return files.Values.Where(f => f.Status == BackupFileStatus.Deleted); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatisticsEventHandler StatisticStale;

        public void AddLocalFile(string path, BackupFileAttributes attributes)
        {
            BackupFile file = GetFile(path);
            file.LocalAttributes = attributes;
            NotifyStatisticsStale();
        }

        public void AddLocalFiles(IDictionary<string, BackupFileAttributes> files)
        {
            foreach (string path in files.Keys)
            {
                BackupFile file = GetFile(path);
                file.LocalAttributes = files[path];
            }
            if (files.Count > 0)
            {
                NotifyStatisticsStale();
            }
        }

        public void AddRemoteFiles(IDictionary<string, BackupFileAttributes> files)
        {
            foreach (string path in files.Keys)
            {
                BackupFile file = GetFile(path);
                file.RemoteAttributes = files[path];
            }
            if (files.Count > 0)
            {
                NotifyStatisticsStale();
            }
        }

        public void FileBackedUp(string path)
        {
            BackupFile file = GetFile(path);
            file.RemoteAttributes = file.LocalAttributes;
            NotifyStatisticsStale();
        }

        public void RemoveLocalFiles()
        {
            lock (files)
            {
                foreach (KeyValuePair<string, BackupFile> pair in files.Where(p => p.Value.RemoteAttributes == null).ToList())
                {
                    files.Remove(pair);
                }
                foreach (KeyValuePair<string, BackupFile> pair in files.Where(p => p.Value.LocalAttributes != null))
                {
                    pair.Value.LocalAttributes = null;
                }
            }
            NotifyStatisticsStale();
        }

        public void RemoveRemoteFiles()
        {
            lock (files)
            {
                foreach (KeyValuePair<string, BackupFile> pair in files.Where(p => p.Value.LocalAttributes == null).ToList())
                {
                    files.Remove(pair);
                }
                foreach (KeyValuePair<string, BackupFile> pair in files.Where(p => p.Value.RemoteAttributes != null))
                {
                    pair.Value.RemoteAttributes = null;
                }
            }
            NotifyStatisticsStale();
        }

        public void TrackStatisticsFor(IEnumerable<BackupLocation> backupLocations)
        {
            lock (files)
            {
                backupLocationStatistics.Clear();
                foreach (BackupLocation backupLocation in backupLocations)
                {
                    backupLocationStatistics.Add(new BackupLocationStatistics(backupLocation.Path));
                }
            }
            NotifyStatisticsStale();
        }

        public void UpdateStatistics()
        {
            lock (files)
            {
                IEnumerable<BackupFile> newFiles = files.Values.Where(f => f.Status == BackupFileStatus.New);
                IEnumerable<BackupFile> updatedFiles = files.Values.Where(f => f.Status == BackupFileStatus.Updated);
                IEnumerable<BackupFile> notModifiedFiles = files.Values.Where(f => f.Status == BackupFileStatus.NotModified);
                localRepositoryStatistics.NewFileCount = newFiles.Count();
                localRepositoryStatistics.UpdatedFileCount = updatedFiles.Count();
                localRepositoryStatistics.DeletedFileCount = files.Values.Count(f => f.Status == BackupFileStatus.Deleted);
                localRepositoryStatistics.TransferFileSize = newFiles.Sum(f => f.LocalAttributes.Size) + updatedFiles.Sum(f => f.LocalAttributes.Size);
                localRepositoryStatistics.FileCount = localRepositoryStatistics.UpdatedFileCount + localRepositoryStatistics.NewFileCount + notModifiedFiles.Count();
                localRepositoryStatistics.TotalFileSize = localRepositoryStatistics.TransferFileSize + notModifiedFiles.Sum(f => f.LocalAttributes == null ? 0 : f.LocalAttributes.Size);
                IEnumerable<BackupFile> remoteFiles = files.Values.Where(f => f.RemoteAttributes != null);
                remoteRepositoryStatistics.FileCount = remoteFiles.Count();
                remoteRepositoryStatistics.TotalFileSize = remoteFiles.Sum(f => f.RemoteAttributes.Size);
                foreach (BackupLocationStatistics statistics in backupLocationStatistics)
                {
                    statistics.Files = new HashSet<BackupFile>(files.Values.Where(f => f.IsBelow(statistics.Path)));
                    statistics.UpdateStatistics();
                }
            }
        }

        private BackupFile GetFile(string path)
        {
            lock (files)
            {
                BackupFile file;
                if (files.ContainsKey(path))
                {
                    file = files[path];
                }
                else
                {
                    file = new BackupFile(path);
                    files[path] = file;
                }
                return file;
            }
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void NotifyStatisticsStale()
        {
            if (StatisticStale != null)
            {
                StatisticStale(this);
            }
        }
    }

    public delegate void StatisticsEventHandler(object sender);

    public class RepositoryStatistics : INotifyPropertyChanged
    {
        private long fileCount;
        private long totalFileSize;
        private long newFileCount;
        private long updatedFileCount;
        private long deletedFileCount;
        private long transferFileSize;

        public long FileCount
        {
            get { return fileCount; }
            set
            {
                if (fileCount != value)
                {
                    fileCount = value;
                    NotifyPropertyChanged("FileCount");
                }
            }
        }

        public long TotalFileSize
        {
            get { return totalFileSize; }
            set
            {
                if (totalFileSize != value)
                {
                    totalFileSize = value;
                    NotifyPropertyChanged("TotalFileSize");
                }
            }
        }

        public long NewFileCount
        {
            get { return newFileCount; }
            set
            {
                if (newFileCount != value)
                {
                    newFileCount = value;
                    NotifyPropertyChanged("NewFileCount");
                }
            }
        }

        public long UpdatedFileCount
        {
            get { return updatedFileCount; }
            set
            {
                if (updatedFileCount != value)
                {
                    updatedFileCount = value;
                    NotifyPropertyChanged("UpdatedFileCount");
                }
            }
        }

        public long DeletedFileCount
        {
            get { return deletedFileCount; }
            set
            {
                if (deletedFileCount != value)
                {
                    deletedFileCount = value;
                    NotifyPropertyChanged("DeletedFileCount");
                }
            }
        }

        public long TransferFileSize
        {
            get { return transferFileSize; }
            set
            {
                if (transferFileSize != value)
                {
                    transferFileSize = value;
                    NotifyPropertyChanged("TransferFileSize");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class BackupLocationStatistics : RepositoryStatistics
    {
        private string path;
        private ICollection<BackupFile> files;

        public BackupLocationStatistics(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get { return path; }
        }

        public BitmapImage Icon
        {
            get { return new DasFile(path).Icon; }
        }

        public ICollection<BackupFile> Files
        {
            get { return files; }
            set
            {
                files = value;
                NotifyPropertyChanged("Files");
            }
        }

        public void UpdateStatistics()
        {
            IEnumerable<BackupFile> localFiles = files.Where(f => f.LocalAttributes != null);
            IEnumerable<BackupFile> newFiles = localFiles.Where(f => f.Status == BackupFileStatus.New);
            IEnumerable<BackupFile> updatedFiles = localFiles.Where(f => f.Status == BackupFileStatus.Updated);
            FileCount = localFiles.Count();
            TotalFileSize = localFiles.Sum(f => f.LocalAttributes.Size);
            NewFileCount = newFiles.Count();
            UpdatedFileCount = updatedFiles.Count();
            DeletedFileCount = files.Count(f => f.Status == BackupFileStatus.Deleted);
            TransferFileSize = newFiles.Sum(f => f.LocalAttributes.Size) + updatedFiles.Sum(f => f.LocalAttributes.Size);
        }
    }

    public class BackupFile
    {
        private string path;
        private BackupFileStatus status = BackupFileStatus.NotModified;
        private BackupFileAttributes localAttributes;
        private BackupFileAttributes remoteAttributes;

        public BackupFile(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get { return path; }
        }

        public string ContentType
        {
            get { return new DasFile(path).ContentType; }
        }

        public BitmapImage Icon
        {
            get { return new DasFile(path).Icon; }
        }

        public BackupFileStatus Status
        {
            get { return status; }
        }

        public BackupFileAttributes LocalAttributes
        {
            get { return localAttributes; }
            set
            {
                localAttributes = value;
                UpdateStatus();
            }
        }

        public BackupFileAttributes RemoteAttributes
        {
            get { return remoteAttributes; }
            set
            {
                remoteAttributes = value;
                UpdateStatus();
            }
        }

        public bool IsBelow(string path)
        {
            return this.path == path || this.path.StartsWith(path + "\\");
        }

        private void UpdateStatus()
        {
            if (localAttributes == null)
            {
                status = remoteAttributes == null ? BackupFileStatus.NotModified : BackupFileStatus.Deleted;
            }
            else
            {
                if (new Regex(Settings.Default.ExcludedFilesRegularExpression).IsMatch(path))
                {
                    status = remoteAttributes != null ? BackupFileStatus.Deleted : BackupFileStatus.Excluded;
                }
                else
                {
                    if (remoteAttributes == null)
                    {
                        status = BackupFileStatus.New;
                    }
                    else
                    {
                        status = localAttributes.Size != remoteAttributes.Size || localAttributes.ModificationDate.CompareTo(remoteAttributes.ModificationDate) > 0 ? BackupFileStatus.Updated : BackupFileStatus.NotModified;
                    }
                }
            }
        }
    }

    public class BackupFileAttributes
    {
        private long size;
        private DateTime modificationDate;
        private string md5;

        public BackupFileAttributes(long size, DateTime modificationDate, string md5)
        {
            this.size = size;
            this.modificationDate = modificationDate;
            this.md5 = md5;
        }

        public long Size
        {
            get { return size; }
        }

        public DateTime ModificationDate
        {
            get { return modificationDate; }
        }

        public string MD5
        {
            get { return md5; }
        }
    }

    public enum BackupFileStatus { New, Updated, Deleted, NotModified, Excluded };
}
