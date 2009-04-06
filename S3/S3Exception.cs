using System;

namespace DasBackupTool.S3
{
    public class S3Exception : Exception
    {
        public S3Exception(string message)
            : base(message)
        {
        }

        public S3Exception(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    public class BucketDoesNotExistException : S3Exception
    {
        public BucketDoesNotExistException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
