using System;

namespace DasBackupTool.S3
{
    public class Object : IObject
    {
        private string key;
        private DateTime lastModified;
        private string eTag;
        private long size;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public DateTime LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        public string ETag
        {
            get { return eTag; }
            set { eTag = value.Substring(1, value.Length - 2); }
        }

        public long Size
        {
            get { return size; }
            set { size = value; }
        }
    }
}
