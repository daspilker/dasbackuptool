using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using DasBackupTool.Model;
using DasBackupTool.Properties;

namespace DasBackupTool.Ui
{
    public partial class BackupLocationDetailWindow : Window
    {
        public static readonly DependencyProperty BackupLocationStatisticsProperty = DependencyProperty.Register("BackupLocationStatistics", typeof(BackupLocationStatistics), typeof(BackupLocationDetailWindow));

        public BackupLocationDetailWindow(BackupLocationStatistics backupLocationStatistics)
        {
            SetValue(BackupLocationStatisticsProperty, backupLocationStatistics);

            InitializeComponent();
            DataContext = this;

            Settings.Default.PropertyChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FileStatusFilterUpdated" || e.PropertyName == "FileStatusFilterNew" || e.PropertyName == "FileStatusFilterDeleted" || e.PropertyName == "FileStatusFilterNotModified")
            {
                ((CollectionViewSource)Resources["Files"]).View.Refresh();
            }
        }

        private void FilesFilter(object sender, FilterEventArgs e)
        {
            File file = (File)e.Item;
            e.Accepted = (file.Status == FileStatus.New && Settings.Default.FileStatusFilterNew) || (file.Status == FileStatus.Updated && Settings.Default.FileStatusFilterUpdated) || (file.Status == FileStatus.Deleted && Settings.Default.FileStatusFilterDeleted) || (file.Status == FileStatus.NotModified && Settings.Default.FileStatusFilterNotModified);
        }
    }
}
