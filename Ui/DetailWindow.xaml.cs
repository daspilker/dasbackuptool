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

            InitializeComponent();
            DataContext = this;
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        private void ViewBackupLocationDetailsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BackupLocationsListView.SelectedItems.Count > 0;
        }

        private void ViewBackupLocationDetailsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BackupLocationDetailWindow backupLocationDetailWindow = new BackupLocationDetailWindow((BackupLocationStatistics)BackupLocationsListView.SelectedItem);
            backupLocationDetailWindow.Owner = this;
            backupLocationDetailWindow.ShowDialog();
        }

        private void BackupLocationsListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Commands.ViewBackupLocationDetails.Execute(null, null);
        }
    }
}
