using System.Windows.Input;

namespace DasBackupTool.Ui
{
    public static class Commands
    {
        public static RoutedUICommand Backup;
        public static RoutedUICommand ConfigureBucket;
        public static RoutedUICommand CreateBucket;
        public static RoutedUICommand DoCreateBucket;
        public static RoutedUICommand RemoveBackupLocation;
        public static RoutedUICommand SaveBackupLocations;
        public static RoutedUICommand SaveBucketConfiguration;
        public static RoutedUICommand SelectBackupLocations;
        public static RoutedUICommand ViewDetails;
        public static RoutedUICommand ViewBackupLocationDetails;

        static Commands()
        {
            Backup = new RoutedUICommand("Start backup", "Backup", typeof(Commands));
            ConfigureBucket = new RoutedUICommand("Configure...", "ConfigureBucket", typeof(Commands));
            CreateBucket = new RoutedUICommand("Create...", "CreateBucket", typeof(Commands));
            DoCreateBucket = new RoutedUICommand("Create", "DoCreateBucket", typeof(Commands));
            RemoveBackupLocation = new RoutedUICommand("Remove", "RemoveBackupLocation", typeof(Commands));
            SaveBackupLocations = new RoutedUICommand("OK", "SaveBackupLocations", typeof(Commands));
            SaveBucketConfiguration = new RoutedUICommand("OK", "SaveBucketConfiguration", typeof(Commands));
            SelectBackupLocations = new RoutedUICommand("Select...", "SelectBackupLocations", typeof(Commands));
            ViewDetails = new RoutedUICommand("Details...", "ViewDetails", typeof(Commands));
            ViewBackupLocationDetails = new RoutedUICommand("Details...", "ViewBackupLocationDetails", typeof(Commands));
        }
    }
}
