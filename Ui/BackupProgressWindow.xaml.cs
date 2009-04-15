using System.ComponentModel;
using System.Windows;
using DasBackupTool.Engine;

namespace DasBackupTool.Ui
{
    public partial class BackupProgressWindow : Window
    {
        public static readonly DependencyProperty BackupEngineProperty = DependencyProperty.Register("BackupEngine", typeof(BackupEngine), typeof(BackupProgressWindow));

        public BackupProgressWindow(BackupEngine backupEngine)
        {
            SetValue(BackupEngineProperty, backupEngine);

            InitializeComponent();
            DataContext = this;
        }
        
        public BackupEngine BackupEngine
        {
            get { return (BackupEngine)GetValue(BackupEngineProperty); }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            BackupEngine.Cancel();
            base.OnClosing(e);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            MinHeight = ActualHeight;
        }
    }
}
