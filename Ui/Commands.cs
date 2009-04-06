using System.Windows;
using System.Windows.Input;

namespace DasBackupTool.Ui
{
    public static class Commands
    {
        public static RoutedUICommand ConfigureBucket;
        public static RoutedUICommand Backup;

        static Commands()
        {
            ConfigureBucket = new RoutedUICommand("Configure...", "ConfigureBucket", typeof(Commands));
            Backup = new RoutedUICommand("Start backup", "Backup", typeof(Commands));
        }
    }
}
