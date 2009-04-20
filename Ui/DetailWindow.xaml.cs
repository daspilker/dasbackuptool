using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using DasBackupTool.Model;

namespace DasBackupTool.Ui
{
    public partial class DetailWindow : Window
    {
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(Files), typeof(DetailWindow));

        public DetailWindow(Files files)
        {
            SetValue(FilesProperty, files);
            DataContext = this;
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        private void ViewBackupLocationDetailsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BackupLocationsListView.SelectedItems.Count > 0;
        }

        private void ViewBackupLocationDetailsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ShowDetailWindow();
        }

        private void BackupLocationsListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BackupLocationsListView.SelectedItems.Count > 0)
            {
                ShowDetailWindow();
            }
        }

        private void ShowDetailWindow()
        {
            BackupLocationDetailWindow backupLocationDetailWindow = new BackupLocationDetailWindow((BackupLocationStatistics)BackupLocationsListView.SelectedItem);
            backupLocationDetailWindow.Owner = this;
            backupLocationDetailWindow.ShowDialog();
        }
    }
}
