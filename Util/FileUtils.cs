using System.IO;

namespace DasBackupTool.Util
{
    public static class FileUtils
    {
        public static bool IsDirectory(string path)
        {
            try
            {
                return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
        }

        public static bool IsArchive(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Archive) == FileAttributes.Archive;
        }

        public static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }
    }
}
