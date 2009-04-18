using System;
using System.Windows;
using System.Windows.Input;
using DasBackupTool.Aws;

namespace DasBackupTool.Ui
{
    public partial class CreateBucketWindow : Window
    {
        public static readonly DependencyProperty BucketProperty = DependencyProperty.Register("Bucket", typeof(string), typeof(CreateBucketWindow));
        public static readonly DependencyProperty EuLocationConstraintProperty = DependencyProperty.Register("EuLocationConstraint", typeof(bool), typeof(CreateBucketWindow));

        private string accessKeyId;
        private string secretAccessKey;

        public CreateBucketWindow(string accessKeyId, string secretAccessKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretAccessKey = secretAccessKey;

            DataContext = this;
            InitializeComponent();
        }

        public string Bucket
        {
            get { return (string)GetValue(BucketProperty); }
        }

        public bool EuLocationConstraint
        {
            get { return (bool)GetValue(EuLocationConstraintProperty); }
        }

        private void DoCreateBucketCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(Bucket);
        }

        private void DoCreateBucketExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                S3 s3 = new S3(accessKeyId, secretAccessKey);
                s3.CreateBucket(Bucket, EuLocationConstraint ? S3LocationConstraint.EU : S3LocationConstraint.None);
                DialogResult = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
