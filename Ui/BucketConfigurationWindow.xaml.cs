using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DasBackupTool.Aws;
using DasBackupTool.Model;
using DasBackupTool.Properties;
using DasBackupTool.Util;

namespace DasBackupTool.Ui
{
    public partial class BucketConfigurationWindow : Window
    {
        public static readonly DependencyProperty AccessKeyIdProperty = DependencyProperty.Register("AccessKeyId", typeof(string), typeof(BucketConfigurationWindow), new PropertyMetadata(CredentialsPropertyChanged), ValidateAccessKeyId);
        public static readonly DependencyProperty SecretAccessKeyProperty = DependencyProperty.Register("SecretAccessKey", typeof(string), typeof(BucketConfigurationWindow), new PropertyMetadata(CredentialsPropertyChanged), ValidateSecretAccessKey);
        public static readonly DependencyProperty BucketProperty = DependencyProperty.Register("Bucket", typeof(string), typeof(BucketConfigurationWindow));
        public static readonly DependencyProperty BucketsProperty = DependencyProperty.Register("Buckets", typeof(IEnumerable<IS3Bucket>), typeof(BucketConfigurationWindow));

        public BucketConfigurationWindow()
        {
            if (Settings.Default.Bucket != null)
            {
                SetValue(AccessKeyIdProperty, Settings.Default.Bucket.AccessKeyId);
                SetValue(SecretAccessKeyProperty, Settings.Default.Bucket.SecretAccessKey);
                SetValue(BucketProperty, Settings.Default.Bucket.BucketName);
            }

            DataContext = this;
            InitializeComponent();
        }

        public string AccessKeyId
        {
            get { return (string)GetValue(AccessKeyIdProperty); }
        }

        public string SecretAccessKey
        {
            get { return (string)GetValue(SecretAccessKeyProperty); }
        }

        public string Bucket
        {
            get { return (string)GetValue(BucketProperty); }
        }

        private bool CredentialsValid
        {
            get { return GetValue(BucketsProperty) != null; }
        }

        private void CreateBucketCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Validation.GetHasError(AccessKeyIdTextBox) && !Validation.GetHasError(SecretAccessKeyIdTextBox) && CredentialsValid;
        }

        private void CreateBucketExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CreateBucketWindow createBucketWindow = new CreateBucketWindow(AccessKeyId, SecretAccessKey);
            createBucketWindow.Owner = this;
            bool? success = createBucketWindow.ShowDialog();
            if (success.Value == true)
            {
                ListBuckets(this);
            }
        }

        private void SaveBucketConfigurationCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Validation.GetHasError(AccessKeyIdTextBox) && !Validation.GetHasError(SecretAccessKeyIdTextBox) && BucketListBox.SelectedIndex != -1;
        }

        private void SaveBucketConfigurationExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.Default.Bucket = new Bucket(AccessKeyId, SecretAccessKey, Bucket);
            Settings.Default.Save();
            Close();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
            if (string.IsNullOrEmpty(AccessKeyId))
            {
                Validation.MarkInvalid(BindingOperations.GetBindingExpression(AccessKeyIdTextBox, TextBox.TextProperty), new ValidationError(new ExceptionValidationRule(), "mandatory"));
            }
            if (string.IsNullOrEmpty(SecretAccessKey))
            {
                Validation.MarkInvalid(BindingOperations.GetBindingExpression(SecretAccessKeyIdTextBox, TextBox.TextProperty), new ValidationError(new ExceptionValidationRule(), "mandatory"));
            }
        }

        private static bool ValidateAccessKeyId(object value)
        {
            return value == null || ((string)value).Length == 20;
        }

        private static bool ValidateSecretAccessKey(object value)
        {
            return value == null || ((string)value).Length == 40;
        }

        private static void CredentialsPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ListBuckets((BucketConfigurationWindow)sender);
        }

        private static void ListBuckets(BucketConfigurationWindow window)
        {
            window.Dispatcher.BeginInvoke((DispatcherInvokeNoResultHandler)delegate
            {
                window.SetValue(BucketsProperty, null);
                string accessKeyId = (string)window.GetValue(AccessKeyIdProperty);
                string secretAccessKey = (string)window.GetValue(SecretAccessKeyProperty);
                if (accessKeyId != null && secretAccessKey != null)
                {
                    try
                    {
                        S3 s3 = new S3(accessKeyId, secretAccessKey);
                        window.SetValue(BucketsProperty, s3.ListBuckets());
                    }
                    catch (S3Exception e)
                    {
                        MessageBox.Show(e.Message);
                        if (e.ErrorCode == "InvalidAccessKeyId")
                        {
                            Validation.MarkInvalid(BindingOperations.GetBindingExpression(window.AccessKeyIdTextBox, TextBox.TextProperty), new ValidationError(new ExceptionValidationRule(), "mandatory"));
                        }
                        else if (e.ErrorCode == "SignatureDoesNotMatch")
                        {
                            Validation.MarkInvalid(BindingOperations.GetBindingExpression(window.SecretAccessKeyIdTextBox, TextBox.TextProperty), new ValidationError(new ExceptionValidationRule(), "mandatory"));
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            });
        }
    }
}
