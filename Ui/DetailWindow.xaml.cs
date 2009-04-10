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
    public partial class DetailWindow : Window
    {
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(Files), typeof(DetailWindow));

        public DetailWindow(Files files)
        {
            SetValue(FilesProperty, files);

            InitializeComponent();
            DataContext = this;
        }
    }
}
