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
        private StatisticsUpdater statisticsUpdater;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            remoteLister = new BucketLister(files, backupProgress);
            remoteLister.Run();
            localLister = new LocalLister(files, backupProgress);
            localLister.Run();
            statisticsUpdater = new StatisticsUpdater(files);
            statisticsUpdater.Run();
            backupEngine = new BackupEngine(files, backupProgress);
            
            MainWindow mainWindow = new MainWindow(files, backupProgress, backupEngine);
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            backupEngine.Dispose();
            remoteLister.Dispose();
            localLister.Dispose();
            statisticsUpdater.Dispose();

            base.OnExit(e);
        }
    }
}
