using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
