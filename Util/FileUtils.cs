using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DasBackupTool.Util
{
    public class DasFile
    {
        private static IDictionary<string, FileType> fileTypeCache = new Dictionary<string, FileType>();
        private static ReaderWriterLockSlim fileTypeCacheLock = new ReaderWriterLockSlim();
        private static IDictionary<string, BitmapImage> iconCache = new Dictionary<string, BitmapImage>();
        private static ReaderWriterLockSlim iconCacheLock = new ReaderWriterLockSlim();

        private string path;

        public DasFile(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get { return path; }
        }

        public bool IsDirectory
        {
            get { return Exists && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory; }
        }

        public bool Exists
        {
            get { return File.Exists(path) || Directory.Exists(path); }
        }

        public string ContentType
        {
            get
            {
                FileType fileType = GetFileType();
                string result = fileType == null ? null : fileType.ContentType;
                return result ?? "application/octet-stream";
            }
        }

        public BitmapImage Icon
        {
            get
            {
                if (IsDirectory)
                {
                    return GetIcon("Folder", path);
                }
                else
                {
                    FileType fileType = GetFileType();
                    BitmapImage result = fileType == null || fileType.Name == null ? null : GetIcon(fileType.Name, path);
                    return result ?? GetIcon("Unknown", path);
                }
            }
        }

        private FileType GetFileType()
        {
            FileType result;
            string extension = new FileInfo(path).Extension;
            if (extension == null) return null;
            fileTypeCacheLock.EnterReadLock();
            try
            {
                fileTypeCache.TryGetValue(extension, out result);
            }
            finally
            {
                fileTypeCacheLock.ExitReadLock();
            }
            if (result == null)
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
                if (key != null)
                {
                    result = new FileType(key);
                }
                fileTypeCacheLock.EnterWriteLock();
                try
                {
                    fileTypeCache[extension] = result;
                }
                finally
                {
                    fileTypeCacheLock.ExitWriteLock();
                }
            }
            return result;
        }

        private static BitmapImage GetIcon(string fileType, string path)
        {
            BitmapImage result;
            iconCacheLock.EnterReadLock();
            try
            {
                iconCache.TryGetValue(fileType, out result);
            }
            finally
            {
                iconCacheLock.ExitReadLock();
            }
            if (result == null)
            {
                bool cache = true;
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(fileType + "\\DefaultIcon");
                if (key != null)
                {
                    string libraryAndIndex = (string)key.GetValue(null);
                    if (libraryAndIndex != null)
                    {
                        libraryAndIndex = libraryAndIndex.Replace("\"", null);
                        string library;
                        int index;
                        if (libraryAndIndex == "%1")
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
                        IntPtr[] smallIcon = new IntPtr[1];
                        int success = ExtractIconEx(library, index, null, smallIcon, 1);
                        if (success == 0) return null;
                        try
                        {
                            using (Icon icon = System.Drawing.Icon.FromHandle(smallIcon[0]))
                            {
                                Bitmap bitmap = icon.ToBitmap();
                                MemoryStream memoryStream = new MemoryStream();
                                bitmap.Save(memoryStream, ImageFormat.Png);
                                result = new BitmapImage();
                                result.BeginInit();
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                result.StreamSource = memoryStream;
                                result.EndInit();
                            }
                        }
                        finally
                        {
                            DestroyIcon(smallIcon[0]);
                        }
                    }
                }
                if (cache)
                {
                    iconCacheLock.EnterWriteLock();
                    try
                    {
                        iconCache[fileType] = result;
                    }
                    finally
                    {
                        iconCacheLock.ExitWriteLock();
                    }
                }
            }
            return result;
        }

        [DllImport("Shell32.dll")]
        private extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

        [DllImport("User32.dll")]
        private extern static int DestroyIcon(IntPtr hIcon);

        private class FileType
        {
            string name;
            string contentType;

            public FileType(RegistryKey key)
            {
                name = (string)key.GetValue(null);
                contentType = (string)key.GetValue("Content Type");
            }

            public string Name
            {
                get { return name; }
            }

            public string ContentType
            {
                get { return contentType; }
            }
        }
    }
}
