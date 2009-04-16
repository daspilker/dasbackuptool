using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DasBackupTool.Util;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

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
        private bool showUnit = true;

        public FileSizeValueConverter()
        {
            numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            numberFormat.NumberDecimalDigits = 0;
        }

        public bool ShowUnit
        {
            get { return showUnit; }
            set { showUnit = value; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((long)value).ToString("n", numberFormat) + (showUnit ? " bytes" : "");
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
            fileImageSource = ExtractIcon("shell32.dll", 0);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            ImageSource result = null;
            if (FileUtils.IsDirectory((string)value))
            {
                result = directoryImageSource;
            }
            else
            {
                DateTime start = DateTime.Now;
                string extension = new FileInfo((string)value).Extension;
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
                if (key != null)
                {
                    string fileType = (string)key.GetValue(null);
                    if (fileType != null)
                    {
                        key = Registry.ClassesRoot.OpenSubKey(fileType + "\\DefaultIcon");
                        if (key != null)
                        {
                            string libraryAndIndex = (string)key.GetValue(null);
                            if (libraryAndIndex != null)
                            {
                                string library;
                                int index;
                                if (libraryAndIndex == "%1" || libraryAndIndex == "\"%1\"")
                                {
                                    library = (string)value;
                                    index = 0;
                                }
                                else if (libraryAndIndex.ToLower().EndsWith(".ico"))
                                {
                                    library = libraryAndIndex;
                                    index = 0;
                                }
                                else
                                {
                                    int commaPos = libraryAndIndex.LastIndexOf(",");
                                    library = libraryAndIndex.Substring(0, commaPos);
                                    index = int.Parse(libraryAndIndex.Substring(commaPos + 1));
                                }
                                DateTime end = DateTime.Now;
                                Debug.Print("registry lookup took " + (end - start));
                                result = ExtractIcon(library, index);
                            }
                        }
                    }
                }
            }
            if (result == null)
            {
                result = fileImageSource;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        [DllImport("Shell32.dll")]
        private extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

        [DllImport("User32.dll")]
        private extern static int DestroyIcon(IntPtr hIcon);

        private static BitmapSource ExtractIcon(string libraryName, int index)
        {
            DateTime start = DateTime.Now;
            IntPtr[] smallIcon = new IntPtr[1];
            int result = ExtractIconEx(libraryName, index, null, smallIcon, 1);
            if (result == 0) return null;
            try
            {
                using (Icon icon = Icon.FromHandle(smallIcon[0]))
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
                DateTime end = DateTime.Now;
                Debug.Print("extract icon took " + (end - start));
            }
        }
    }

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
