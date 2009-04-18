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
    public class FileIconValueConverter : IValueConverter
    {
        private static ImageSource directoryImageSource;
        private static ImageSource fileImageSource;
        private static IDictionary<String, String> fileTypes = new Dictionary<String, String>();
        private static IDictionary<String, String> libraryAndIndices = new Dictionary<String, String>();
        private static IDictionary<String, ImageSource> icons = new Dictionary<String, ImageSource>();

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
                string fileType = GetFileType(new FileInfo((string)value).Extension);
                if (fileType != null)
                {
                    string libraryAndIndex = GetLibraryAndIndex(fileType);
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
                            if (commaPos == -1)
                            {
                                library = libraryAndIndex;
                                index = 0;
                            }
                            else
                            {
                                library = libraryAndIndex.Substring(0, commaPos);
                                index = int.Parse(libraryAndIndex.Substring(commaPos + 1));
                            }
                        }
                        DateTime end = DateTime.Now;
                        result = ExtractIcon(library, index);
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

        private static string GetFileType(string extension)
        {
            string result = null;
            if (fileTypes.ContainsKey(extension))
            {
                result = fileTypes[extension];
            }
            else
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
                if (key != null)
                {
                    result = (string)key.GetValue(null);
                }
                fileTypes[extension] = result;
            }
            return result;
        }

        private static string GetLibraryAndIndex(string fileType)
        {
            string result = null;
            if (libraryAndIndices.ContainsKey(fileType))
            {
                result = libraryAndIndices[fileType];
            }
            else
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(fileType + "\\DefaultIcon");
                if (key != null)
                {
                    result = (string)key.GetValue(null);
                }
                libraryAndIndices[fileType] = result;
            }
            return result;
        }

        private static ImageSource GetIcon(string libraryAndIndex, string path)
        {
            ImageSource result = null;
            if (icons.ContainsKey(libraryAndIndex))
            {
                result = icons[libraryAndIndex];
            }
            else
            {
                string library;
                int index;
                bool cache = true;
                if (libraryAndIndex == "%1" || libraryAndIndex == "\"%1\"")
                {
                    library = path;
                    index = 0;
                    cache = false;
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
                result = ExtractIcon(library, index);
                if (cache)
                {
                    icons[libraryAndIndex] = result;
                }
            }
            return result;
        }

        [DllImport("Shell32.dll")]
        private extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

        [DllImport("User32.dll")]
        private extern static int DestroyIcon(IntPtr hIcon);

        private static BitmapSource ExtractIcon(string libraryName, int index)
        {
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
