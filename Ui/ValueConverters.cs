using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace DasBackupTool.Ui
{
    public class FileCountValueConverter : IValueConverter
    {
        private NumberFormatInfo numberFormat;

        public FileCountValueConverter()
        {
            numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            numberFormat.NumberDecimalDigits = 0;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((long)value).ToString("n", numberFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSizeValueConverter : IValueConverter
    {
        private NumberFormatInfo numberFormat;

        public FileSizeValueConverter()
        {
            numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            numberFormat.NumberDecimalDigits = 0;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((long)value).ToString("n", numberFormat) + " bytes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
