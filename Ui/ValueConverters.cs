using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    public class FileIconValueConverter : IValueConverter
    {
        private static ImageSource directoryImageSource;
        private static ImageSource fileImageSource;
        
        static FileIconValueConverter()
        {
            directoryImageSource = ExtractIcon("shell32.dll", 3);
            fileImageSource = ExtractIcon("shell32.dll", 1);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DasBackupTool.Util.File.IsDirectory((string)value) ? directoryImageSource : fileImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        [DllImport("Shell32.dll")]
        private extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

        [DllImport("User32.dll")]
        private extern static int DestroyIcon(IntPtr hIcon);

        private static ImageSource ExtractIcon(string libraryName, int index)
        {
            IntPtr[] smallIcon = new IntPtr[1];
            ExtractIconEx(libraryName, index, null, smallIcon, 1);
            try
            {
                Icon icon = Icon.FromHandle(smallIcon[0]);
                using (icon)
                {
                    Bitmap bitmap = icon.ToBitmap();
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
            finally
            {
                DestroyIcon(smallIcon[0]);
            }
        }
    }
}
