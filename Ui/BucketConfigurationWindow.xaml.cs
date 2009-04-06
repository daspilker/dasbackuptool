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

namespace DasBackupTool.Ui
{
    public partial class BucketConfigurationWindow : Window
    {
        private Configuration configuration;

        public BucketConfigurationWindow(Configuration configuration)
        {
            this.configuration = configuration;

            InitializeComponent();
            DataContext = this;

            AccessKeyIdTextBox.Text = configuration.AccessKeyId;
            SecretAccessKeyPasswordBox.Password = configuration.SecretKey;
            BucketTextBox.Text = configuration.Bucket;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            configuration.AccessKeyId = AccessKeyIdTextBox.Text;
            configuration.SecretKey = SecretAccessKeyPasswordBox.Password;
            configuration.Bucket = BucketTextBox.Text;
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
