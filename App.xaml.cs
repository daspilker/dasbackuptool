using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DasBackupTool.Model;
using DasBackupTool.Ui;
using DasBackupTool.Engine;

namespace DasBackupTool
{
    public partial class App : Application
    {
        private Files files = new Files();
        private Configuration configuration = new Configuration();
        private BackupProgress backupProgress = new BackupProgress();
        private LocalLister localLister;
        private BucketLister remoteLister;
        private BackupEngine backupEngine;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            configuration.AccessKeyId = "xxxx";
            configuration.SecretKey = "xxxx";
            configuration.Bucket = "xxxx";
            configuration.AddBackupLocation(@"xxxx", false);

            remoteLister = new BucketLister(configuration, files, backupProgress);
            remoteLister.Run();
            localLister = new LocalLister(configuration, files, backupProgress);
            localLister.Run();

            backupEngine = new BackupEngine(configuration, files, backupProgress);
            
            MainWindow mainWindow = new MainWindow(files, configuration, backupProgress, backupEngine);
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
