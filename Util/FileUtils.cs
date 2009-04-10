using System.IO;

namespace DasBackupTool.Util
{
    public static class FileUtils
    {
        public static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public static bool IsArchive(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Archive) == FileAttributes.Archive;
        }
    }
}
