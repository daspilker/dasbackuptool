using System;
using System.Collections.Generic;
using System.IO;

namespace DasBackupTool.S3
{
    public interface IBucket
    {
        String Name
        {
            get;
        }

        DateTime CreationDate
        {
            get;
        }

        IEnumerable<IObject> ListObjects(ICredentials credentials);

        void PutObject(ICredentials credentials, string name, long size, string type, Stream stream);

        void DeleteObject(ICredentials credentials, string name);
    }
}
