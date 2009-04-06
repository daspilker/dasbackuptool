﻿using System.Windows;
using System.Windows.Input;

namespace DasBackupTool.Ui
{
    public static class Commands
    {
        public static RoutedUICommand Backup;
        public static RoutedUICommand ConfigureBucket;
        public static RoutedUICommand RemoveBackupLocation;
        public static RoutedUICommand SelectBackupLocations;

        static Commands()
        {
            Backup = new RoutedUICommand("Start backup", "Backup", typeof(Commands));
            ConfigureBucket = new RoutedUICommand("Configure...", "ConfigureBucket", typeof(Commands));
            RemoveBackupLocation = new RoutedUICommand("Remove", "RemoveBackupLocation", typeof(Commands));
            SelectBackupLocations = new RoutedUICommand("Select...", "SelectBackupLocations", typeof(Commands));
        }
    }
}
