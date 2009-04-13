using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DasBackupTool.Model;
using DasBackupTool.Properties;

namespace DasBackupTool.Ui
{
    public partial class BackupLocationSelectionWindow : Window
    {
        public static readonly DependencyProperty BackupLocationsProperty = DependencyProperty.Register("BackupLocations", typeof(ObservableCollection<BackupLocation>), typeof(BackupLocationSelectionWindow));
        public static readonly DependencyProperty ExcludedFilesPatternProperty = DependencyProperty.Register("ExcludedFilesPattern", typeof(string), typeof(BackupLocationSelectionWindow));

        public BackupLocationSelectionWindow()
        {
            ObservableCollection<BackupLocation> backupLocations = new ObservableCollection<BackupLocation>();
            if (Settings.Default.BackupLocations != null)
            {
                foreach (BackupLocation backupLocation in Settings.Default.BackupLocations)
                {
                    backupLocations.Add((BackupLocation)backupLocation.Clone());
                }
            }
            SetValue(BackupLocationsProperty, backupLocations);
            SetValue(ExcludedFilesPatternProperty, Settings.Default.ExcludedFilesRegularExpression);

            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<BackupLocation> BackupLocations
        {
            get { return (ObservableCollection<BackupLocation>)GetValue(BackupLocationsProperty); }
        }

        private void BackupLocationsListBoxDrop(object sender, DragEventArgs e)
        {
            String[] filename = e.Data.GetData(DataFormats.FileDrop) as String[];
            foreach (String name in filename)
            {
                BackupLocations.Add(new BackupLocation(name, false));
            }
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.BackupLocations = new BackupLocations(BackupLocations);
            Settings.Default.ExcludedFilesRegularExpression = (string)GetValue(ExcludedFilesPatternProperty);
            Settings.Default.Save();
            this.Close();
        }

        private void RemoveBackupLocationCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BackupLocationsListView.SelectedItems.Count > 0;
        }

        private void RemoveBackupLocationExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (BackupLocation backupLocation in new ArrayList(BackupLocationsListView.SelectedItems))
            {
                BackupLocations.Remove(backupLocation);
            }
        }
    }
}
