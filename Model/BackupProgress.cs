using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DasBackupTool.Model
{
    public class BackupProgress : INotifyPropertyChanged
    {
        private BackupState state = BackupState.Startup;
        private ICollection<BackupAction> actions = new LinkedList<BackupAction>();

        public BackupState State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public string StatusMessage
        {
            get
            {
                string message = "";
                foreach (BackupAction action in actions)
                {
                    switch (action)
                    {
                        case BackupAction.ListingBucket:
                            message += "listing remote files, ";
                            break;
                        case BackupAction.ListingLocal:
                            message += "listing local files, ";
                            break;
                        case BackupAction.RunningBackup:
                            message += "running backup, ";
                            break;
                    }
                }
                return message.Substring(0, message.Length - 2);
            }
        }

        public void EnterAction(BackupAction action)
        {
            lock (actions)
            {
                if (action == BackupAction.ListingLocal || action == BackupAction.ListingBucket)
                {
                    if (state == BackupState.Backup)
                    {
                        throw new InvalidOperationException();
                    }
                    State = BackupState.Listing;
                    actions.Add(action);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            NotifyPropertyChanged("StatusMessage");
        }

        public void ExitAction(BackupAction action)
        {
            lock (actions)
            {
                if (!actions.Contains(action))
                {
                    throw new InvalidOperationException();
                }
                actions.Remove(action);
                if (actions.Count == 0)
                {
                    State = BackupState.Idle;
                }
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

    public enum BackupAction { ListingBucket, ListingLocal, RunningBackup }
    public enum BackupState { Startup, Listing, Idle, Backup }
}
