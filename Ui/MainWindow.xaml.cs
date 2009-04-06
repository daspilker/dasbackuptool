using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DasBackupTool.Model;
using DasBackupTool.Engine;

namespace DasBackupTool.Ui
{
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(Files), typeof(MainWindow));
        public static readonly DependencyProperty BackupProgressProperty = DependencyProperty.Register("BackupProgress", typeof(BackupProgress), typeof(MainWindow));

        private BackupEngine backupEngine;
        private Configuration configuration;

        public MainWindow(Files files, Configuration configuration, BackupProgress backupProgress, BackupEngine backupEngine)
        {
            SetValue(FilesProperty, files);
            SetValue(BackupProgressProperty, backupProgress);
            this.configuration = configuration;
            this.backupEngine = backupEngine;

            InitializeComponent();
            DataContext = this;
        }

        private void ConfigureBucketCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ConfigureBucketExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BucketConfigurationWindow bucketConfigurationWindow = new BucketConfigurationWindow(configuration);
            bucketConfigurationWindow.Owner = this;            
            bucketConfigurationWindow.ShowDialog();
        }

        private void BackupCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BackupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            backupEngine.Run();
        }
    }
}
