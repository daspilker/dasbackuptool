using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
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
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            Settings.Default.PropertyChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FileStatusFilterUpdated" || e.PropertyName == "FileStatusFilterNew" || e.PropertyName == "FileStatusFilterDeleted" || e.PropertyName == "FileStatusFilterNotModified" || e.PropertyName == "FileStatusFilterExcluded")
            {
                ((CollectionViewSource)Resources["Files"]).View.Refresh();
            }
        }

        private void FilesFilter(object sender, FilterEventArgs e)
        {
            BackupFile file = (BackupFile)e.Item;
            e.Accepted = (file.Status == BackupFileStatus.New && Settings.Default.FileStatusFilterNew) || (file.Status == BackupFileStatus.Updated && Settings.Default.FileStatusFilterUpdated) || (file.Status == BackupFileStatus.Deleted && Settings.Default.FileStatusFilterDeleted) || (file.Status == BackupFileStatus.NotModified && Settings.Default.FileStatusFilterNotModified) || (file.Status == BackupFileStatus.Excluded && Settings.Default.FileStatusFilterExcluded);
        }
    }
}
