using System.IO;

namespace DasBackupTool.Util
{
    public static class File
    {
        public static bool IsDirectory(string path)
        {
            return (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
