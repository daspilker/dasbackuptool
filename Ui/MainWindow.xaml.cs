using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DasBackupTool.Engine;
using DasBackupTool.Model;
using DasBackupTool.Util;

namespace DasBackupTool.Ui
{
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(Files), typeof(MainWindow));
        public static readonly DependencyProperty BackupProgressProperty = DependencyProperty.Register("BackupProgress", typeof(BackupProgress), typeof(MainWindow));

        private BackupEngine backupEngine;

        public MainWindow(Files files, BackupProgress backupProgress, BackupEngine backupEngine)
        {
            SetValue(FilesProperty, files);
            SetValue(BackupProgressProperty, backupProgress);
            this.backupEngine = backupEngine;

            InitializeComponent();
            DataContext = this;

            BackupProgress.PropertyChanged += BackupProgressChanged;
        }

        public BackupProgress BackupProgress
        {
            get { return (BackupProgress)GetValue(BackupProgressProperty); }
        }

        private void ConfigureCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BackupProgress.State != BackupState.Backup;
        }

        private void SelectBackupLocationsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BackupLocationSelectionWindow backupLocationSelectionWindow = new BackupLocationSelectionWindow();
            backupLocationSelectionWindow.Owner = this;
            backupLocationSelectionWindow.ShowDialog();
        }

        private void ConfigureBucketExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BucketConfigurationWindow bucketConfigurationWindow = new BucketConfigurationWindow();
            bucketConfigurationWindow.Owner = this;
            bucketConfigurationWindow.ShowDialog();
        }

        private void BackupCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BackupProgress.State == BackupState.Idle;
        }

        private void BackupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            backupEngine.Run();
        }

        private void BackupProgressChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(new DispatcherInvokeNoResultHandler(CommandManager.InvalidateRequerySuggested));
        }
    }
}
