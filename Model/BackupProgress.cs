using System;
using System.ComponentModel;

namespace DasBackupTool.Model
{
    public class BackupProgress : INotifyPropertyChanged
    {
        private BackupStatus status;
        private object statusLock = new object();

        public string StatusMessage
        {
            get {
                string message = "";
                if ((status & BackupStatus.ListingBucket) == BackupStatus.ListingBucket)
                {
                    message += "listing remote files, ";
                }
                if ((status & BackupStatus.ListingLocal) == BackupStatus.ListingLocal)
                {
                    message += "listing local files, ";
                }
                if ((status & BackupStatus.RunningBackup) == BackupStatus.RunningBackup)
                {
                    message += "running backup, ";
                }
                return message.Substring(0, message.Length-2);
            }
        }

        public void AddStatus(BackupStatus status)
        {
            lock (statusLock)
            {
                this.status |= status;
            }
            NotifyPropertyChanged("StatusMessage");
        }

        public void RemoveStatus(BackupStatus status)
        {
            lock (statusLock)
            {
                this.status &= ~status;
            }
            NotifyPropertyChanged("StatusMessage");
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

    [Flags]
    public enum BackupStatus { Done, ListingBucket, ListingLocal, RunningBackup }
}
