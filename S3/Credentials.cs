using System;

namespace DasBackupTool.S3
{
    class Credentials : ICredentials
    {
        private String accessKeyId;
        private String secretAccessKey;

        public Credentials()
        {
        }

        public Credentials(String accessKeyId, String secretAccessKey)
        {
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
        }

        public String AccessKeyId
        {
            get { return accessKeyId; }
            set { accessKeyId = value; }
        }

        public String SecretAccessKey
        {
            get { return secretAccessKey; }
            set { secretAccessKey = value; }
        }
    }
}
