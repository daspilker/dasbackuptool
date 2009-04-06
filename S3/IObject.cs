using System;

namespace DasBackupTool.S3
{
    public interface IObject
    {
        string Key
        {
            get;
        }

        DateTime LastModified
        {
            get;
        }

        string ETag
        {
            get;
        }

        long Size
        {
            get;
        }
    }
}
