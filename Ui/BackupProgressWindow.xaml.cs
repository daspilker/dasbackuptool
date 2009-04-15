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

        protected override void OnClosed(EventArgs e)
        {
            BackupEngine.Cancel();

            base.OnClosed(e);
        }
    }
}
