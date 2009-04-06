using System;

namespace DasBackupTool.S3
{
    public interface ICredentials
    {
        String AccessKeyId
        {
            get;
            set;
        }

        String SecretAccessKey
        {
            get;
            set;
        }
    }
}
