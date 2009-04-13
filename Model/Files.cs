using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace DasBackupTool.Model
{
    public class Files : INotifyPropertyChanged
    {
        private RepositoryStatistics localRepositoryStatistics = new RepositoryStatistics();
        private RepositoryStatistics remoteRepositoryStatistics = new RepositoryStatistics();
        private long transferedFileCount;
        private long transferedData;
        private IDictionary<String, File> files = new Dictionary<String, File>();
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

        public long TransferedFileCount
        {
            get { return transferedFileCount; }
            set
            {
                if (transferedFileCount != value)
                {
                    transferedFileCount = value;
                    NotifyPropertyChanged("TransferedFileCount");
                }
            }
        }

        public long TransferedData
        {
            get { return transferedData; }
            set
            {
                if (transferedData != value)
                {
                    transferedData = value;
                    NotifyPropertyChanged("TransferedData");
                }
            }
        }

        public IEnumerable<string> NewOrUpdatedFiles
        {
            get { return files.Values.Where(f => f.Status == FileStatus.New || f.Status == FileStatus.Updated).Select(f => f.Path); }
        }

        public IEnumerable<string> DeletedFiles
        {
            get { return files.Values.Where(f => f.Status == FileStatus.Deleted).Select(f => f.Path); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatisticsEventHandler StatisticStale;

        public void AddLocalFile(String path, File.Attributes attributes)
        {
            File file = GetFile(path);
            file.LocalAttributes = attributes;
            NotifyStatisticsStale();
        }

        public void AddLocalFiles(IDictionary<string, File.Attributes> files)
        {
            foreach (string path in files.Keys)
            {
                File file = GetFile(path);
                file.LocalAttributes = files[path];
            }
            if (files.Count > 0)
            {
                NotifyStatisticsStale();
            }
        }

        public void AddRemoteFiles(IDictionary<string, File.Attributes> files)
        {
            foreach (string path in files.Keys)
            {
                File file = GetFile(path);
                file.RemoteAttributes = files[path];
            }
            if (files.Count > 0)
            {
                NotifyStatisticsStale();
            }
        }

        public void FileBackedUp(String path)
        {
            File file = GetFile(path);
            file.RemoteAttributes = file.LocalAttributes;
            if (file.LocalAttributes != null)
            {
                file.LocalAttributes.Archive = false;
                TransferedFileCount++;
                TransferedData += file.LocalAttributes.Size;
            }
            NotifyStatisticsStale();
        }

        public void RemoveLocalFiles()
        {
            lock (files)
            {
                foreach (KeyValuePair<string, File> pair in files.Where(p => p.Value.RemoteAttributes == null).ToList())
                {
                    files.Remove(pair);
                }
                foreach (KeyValuePair<string, File> pair in files.Where(p => p.Value.LocalAttributes != null))
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
                foreach (KeyValuePair<string, File> pair in files.Where(p => p.Value.LocalAttributes == null).ToList())
                {
                    files.Remove(pair);
                }
                foreach (KeyValuePair<string, File> pair in files.Where(p => p.Value.RemoteAttributes != null))
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
            DateTime start = DateTime.Now;
            lock (files)
            {
                IEnumerable<File> localFiles = files.Values.Where(f => f.LocalAttributes != null);
                IEnumerable<File> newFiles = files.Values.Where(f => f.Status == FileStatus.New);
                IEnumerable<File> updatedFiles = files.Values.Where(f => f.Status == FileStatus.Updated);
                localRepositoryStatistics.FileCount = localFiles.Count();
                localRepositoryStatistics.TotalFileSize = localFiles.Sum(f => f.LocalAttributes.Size);
                localRepositoryStatistics.NewFileCount = newFiles.Count();
                localRepositoryStatistics.UpdatedFileCount = updatedFiles.Count();
                localRepositoryStatistics.DeletedFileCount = files.Values.Count(f => f.Status == FileStatus.Deleted);
                localRepositoryStatistics.TransferFileSize = newFiles.Sum(f => f.LocalAttributes.Size) + updatedFiles.Sum(f => f.LocalAttributes.Size);
                IEnumerable<File> remoteFiles = files.Values.Where(f => f.RemoteAttributes != null);
                remoteRepositoryStatistics.FileCount = remoteFiles.Count();
                remoteRepositoryStatistics.TotalFileSize = remoteFiles.Sum(f => f.RemoteAttributes.Size);
                foreach (BackupLocationStatistics statistics in backupLocationStatistics)
                {
                    statistics.Files = new HashSet<File>(files.Values.Where(f => f.IsBelow(statistics.Path)));
                    statistics.UpdateStatistics();
                }
            }
            DateTime end = DateTime.Now;
            Debug.Print("update: " + (end - start));
        }

        private File GetFile(string path)
        {
            lock (files)
            {
                File file;
                if (files.ContainsKey(path))
                {
                    file = files[path];
                }
                else
                {
                    file = new File(path);
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
        private ICollection<File> files;

        public BackupLocationStatistics(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get { return path; }
        }

        public ICollection<File> Files
        {
            get { return files; }
            set { 
                files = value;
                NotifyPropertyChanged("Files");
            }
        }

        public void UpdateStatistics()
        {
            IEnumerable<File> localFiles = files.Where(f => f.LocalAttributes != null);
            IEnumerable<File> newFiles = localFiles.Where(f => f.Status == FileStatus.New);
            IEnumerable<File> updatedFiles = localFiles.Where(f => f.Status == FileStatus.Updated);
            FileCount = localFiles.Count();
            TotalFileSize = localFiles.Sum(f => f.LocalAttributes.Size);
            NewFileCount = newFiles.Count();
            UpdatedFileCount = updatedFiles.Count();
            DeletedFileCount = files.Count(f => f.Status == FileStatus.Deleted);
            TransferFileSize = newFiles.Sum(f => f.LocalAttributes.Size) + updatedFiles.Sum(f => f.LocalAttributes.Size);
        }
    }

    public class File
    {
        private string path;
        private Attributes localAttributes;
        private Attributes remoteAttributes;

        public File(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get { return path; }
        }

        public FileStatus Status
        {
            get
            {
                if (localAttributes == null)
                {
                    return remoteAttributes == null ? FileStatus.NotModified : FileStatus.Deleted;
                }
                else
                {
                    if (remoteAttributes == null)
                    {
                        return FileStatus.New;
                    }
                    else
                    {
                        return localAttributes.Size != remoteAttributes.Size || localAttributes.ModificationDate.CompareTo(remoteAttributes.ModificationDate) > 0 ? FileStatus.Updated : FileStatus.NotModified;
                    }
                }
            }
        }

        public Attributes LocalAttributes
        {
            get { return localAttributes; }
            set { localAttributes = value; }
        }

        public Attributes RemoteAttributes
        {
            get { return remoteAttributes; }
            set { remoteAttributes = value; }
        }

        public bool IsBelow(string path)
        {
            return this.path == path || this.path.StartsWith(path + "\\");
        }

        public class Attributes
        {
            private long size;
            private DateTime modificationDate;
            private string md5;
            private bool? archive;

            public Attributes(long size, DateTime modificationDate, string md5, bool? archive)
            {
                this.size = size;
                this.modificationDate = modificationDate;
                this.md5 = md5;
                this.archive = archive;
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

            public bool? Archive
            {
                get { return archive; }
                set { archive = value; }
            }
        }
    }

    public enum FileStatus { New, Updated, Deleted, NotModified };
}
