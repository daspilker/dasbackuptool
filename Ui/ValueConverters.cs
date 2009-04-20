using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DasBackupTool.Util;
using Microsoft.Win32;

namespace DasBackupTool.Ui
{
    public class TimeSpanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            StringBuilder builder = new StringBuilder();
            if (timeSpan.Days > 0) builder.Append(timeSpan.Days).Append(":");
            if (timeSpan.Hours > 0 || timeSpan.Days > 0) builder.Append(timeSpan.Hours.ToString(builder.Length > 0 ? "00" : "0")).Append(":");
            builder.Append(timeSpan.Minutes.ToString(builder.Length > 0 ? "00" : "0")).Append(":");
            builder.Append(timeSpan.Seconds.ToString("00"));
            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
