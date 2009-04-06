using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DasBackupTool.Model
{
    public class BackupProgress : INotifyPropertyChanged
    {
        private BackupStatus status;
        private long transferedFileCount;
        private long transferedData;
        private object statusLock = new object();
        private object statisticsLock = new object();

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

        public long TransferedFileCount
        {
            get { return transferedFileCount; }
        }

        public long TransferedData
        {
            get { return transferedData; }
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

        public void FileBackedUp(long size)
        {
            lock (statisticsLock)
            {
                transferedFileCount++;
                transferedData += size;
            }
            NotifyPropertyChanged("TransferedFileCount");
            NotifyPropertyChanged("TransferedData");
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
