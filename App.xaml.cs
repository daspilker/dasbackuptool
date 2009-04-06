using System.Windows;
using DasBackupTool.Engine;
using DasBackupTool.Model;
using DasBackupTool.Ui;

namespace DasBackupTool
{
    public partial class App : Application
    {
        private Files files = new Files();
        private BackupProgress backupProgress = new BackupProgress();
        private LocalLister localLister;
        private BucketLister remoteLister;
        private BackupEngine backupEngine;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            remoteLister = new BucketLister(files, backupProgress);
            remoteLister.Run();
            localLister = new LocalLister(files, backupProgress);
            localLister.Run();

            //DasBackupTool.Properties.Settings.Default.BackupLocations = new BackupLocations();
            //DasBackupTool.Properties.Settings.Default.BackupLocations.AddBackupLocation(@"C:\Users\dspilker\Temp", false);
            //DasBackupTool.Properties.Settings.Default.Save();

            backupEngine = new BackupEngine(files, backupProgress);
            
            MainWindow mainWindow = new MainWindow(files, backupProgress, backupEngine);
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            backupEngine.Dispose();
            remoteLister.Dispose();
            localLister.Dispose();

            base.OnExit(e);
        }
    }
}
