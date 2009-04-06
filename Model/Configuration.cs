using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DasBackupTool.Model
{
    public class Configuration : INotifyPropertyChanged
    {
        private string accessKeyId;
        private string secretKey;
        private string bucket;
        private ICollection<BackupLocation> backupLoactions = new HashSet<BackupLocation>();

        public string AccessKeyId
        {
            get { return accessKeyId; }
            set
            {
                if (accessKeyId != value)
                {
                    accessKeyId = value;
                    NotifyPropertyChanged("AccessKeyId");
                }
            }
        }

        public string SecretKey
        {
            get { return secretKey; }
            set
            {
                if (secretKey != value)
                {
                    secretKey = value;
                    NotifyPropertyChanged("SecretKey");
                }
            }
        }

        public string Bucket
        {
            get { return bucket; }
            set
            {
                if (bucket != value)
                {
                    bucket = value;
                    NotifyPropertyChanged("Bucket");
                }
            }
        }

        public IEnumerable<BackupLocation> BackupLocations
        {
            get { return backupLoactions; }
        }

        public void AddBackupLocation(String path, bool excluded)
        {
            backupLoactions.Add(new BackupLocation(path, excluded));
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

    public class BackupLocation
    {
        private string path;
        private bool excluded;

        public BackupLocation(string path, bool excluded)
        {
            this.path = path;
            this.excluded = excluded;
        }

        public string Path
        {
            get { return path; }
        }
    }
}
