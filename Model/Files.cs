using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DasBackupTool.Model
{
    public class Files : INotifyPropertyChanged
    {
        private RepositoryStatistics localRepositoryStatistics = new RepositoryStatistics();
        private RepositoryStatistics remoteRepositoryStatistics = new RepositoryStatistics();
        private long newFileCount;
        private long updatedFileCount;
        private long deletedFileCount;
        private long transferFileSize;
        private IDictionary<String, File> files = new Dictionary<String, File>();

        public RepositoryStatistics LocalRepositoryStatistics
        {
            get { return localRepositoryStatistics; }
        }

        public RepositoryStatistics RemoteRepositoryStatistics
        {
            get { return remoteRepositoryStatistics; }
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

        public IEnumerable<string> NewOrUpdatedFiles
        {
            get { return files.Values.Where(f => f.Status == FileStatus.New || f.Status == FileStatus.Updated).Select(f => f.Path); }
        }

        public IEnumerable<string> DeletedFiles
        {
            get { return files.Values.Where(f => f.Status == FileStatus.Deleted).Select(f => f.Path); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddLocalFile(String path, long size, DateTime modificationDate, string md5)
        {
            File file = GetFile(path);
            file.LocalAttributes = new FileAttributes(size, modificationDate, md5);
            UpdateStatistics();
        }

        public void AddRemoteFile(String path, long size, DateTime modificationDate, string md5)
        {
            File file = GetFile(path);
            file.RemoteAttributes = new FileAttributes(size, modificationDate, md5);
            UpdateStatistics();
        }

        public void FileBackedUp(String path)
        {
            File file = GetFile(path);
            file.RemoteAttributes = file.LocalAttributes;
            UpdateStatistics();
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

        private void UpdateStatistics()
        {
            IEnumerable<File> localFiles = files.Values.Where(f => f.LocalAttributes != null);
            IEnumerable<File> remoteFiles = files.Values.Where(f => f.RemoteAttributes != null);
            localRepositoryStatistics.FileCount = localFiles.Count();
            localRepositoryStatistics.TotalFileSize = localFiles.Sum(f => f.LocalAttributes.Size);
            remoteRepositoryStatistics.FileCount = remoteFiles.Count();
            remoteRepositoryStatistics.TotalFileSize = remoteFiles.Sum(f => f.RemoteAttributes.Size);
            IEnumerable<File> newFiles = files.Values.Where(f => f.Status == FileStatus.New);
            IEnumerable<File> updatedFiles = files.Values.Where(f => f.Status == FileStatus.Updated);
            NewFileCount = newFiles.Count();
            UpdatedFileCount = updatedFiles.Count();
            DeletedFileCount = files.Values.Count(f => f.Status == FileStatus.Deleted);
            TransferFileSize = newFiles.Sum(f => f.LocalAttributes.Size) + updatedFiles.Sum(f => f.LocalAttributes.Size);
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class RepositoryStatistics : INotifyPropertyChanged
    {
        private long fileCount;
        private long totalFileSize;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class File
    {
        private string path;
        private FileAttributes localAttributes;
        private FileAttributes remoteAttributes;

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
                        return localAttributes.Equals(remoteAttributes) ? FileStatus.NotModified : FileStatus.Updated;
                    }
                }
            }
        }

        public FileAttributes LocalAttributes
        {
            get { return localAttributes; }
            set { localAttributes = value; }
        }

        public FileAttributes RemoteAttributes
        {
            get { return remoteAttributes; }
            set { remoteAttributes = value; }
        }
    }

    public enum FileStatus { New, Updated, Deleted, NotModified };

    public class FileAttributes
    {
        private long size;
        private DateTime modificationDate;
        private string md5;

        public FileAttributes(long size, DateTime modificationDate, string md5)
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

        public override bool Equals(object obj)
        {
            FileAttributes attributes = obj as FileAttributes;
            if (attributes == null) return false;
            return attributes.size == size && attributes.modificationDate == modificationDate;
        }
    }
}
