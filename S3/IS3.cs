using System.Collections.Generic;

namespace DasBackupTool.S3
{
    interface IS3
    {
        IEnumerable<IBucket> ListBuckets(ICredentials credentials);
        IBucket CreateBucket(ICredentials credentials, string bucketName);
    }
}
