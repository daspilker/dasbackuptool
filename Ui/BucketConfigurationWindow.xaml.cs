using System.Windows;
using DasBackupTool.Model;
using DasBackupTool.Properties;

namespace DasBackupTool.Ui
{
    public partial class BucketConfigurationWindow : Window
    {
        public BucketConfigurationWindow()
        {
            InitializeComponent();
            DataContext = this;

            if (Settings.Default.Bucket != null)
            {
                AccessKeyIdTextBox.Text = Settings.Default.Bucket.AccessKeyId;
                SecretAccessKeyPasswordBox.Password = Settings.Default.Bucket.SecretAccessKey;
                BucketTextBox.Text = Settings.Default.Bucket.BucketName;
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Bucket = new Bucket(AccessKeyIdTextBox.Text, SecretAccessKeyPasswordBox.Password, BucketTextBox.Text);
            Settings.Default.Save();
            this.Close();
        }
    }
}
