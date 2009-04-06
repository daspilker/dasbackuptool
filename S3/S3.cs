using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace DasBackupTool.S3
{
    public class S3 : IS3
    {
        internal static XNamespace S3_NAMESPACE = "http://s3.amazonaws.com/doc/2006-03-01/";

        public IEnumerable<IBucket> ListBuckets(ICredentials credentials)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://s3.amazonaws.com");
            using (HttpWebResponse response = S3Helper.Send(request, credentials))
            {
                XElement xml = XElement.Load(new StreamReader(response.GetResponseStream()));

                return from e in xml.Element(S3_NAMESPACE + "Buckets").Elements(S3_NAMESPACE + "Bucket")
                       select (IBucket)new Bucket
                       {
                           Name = (string)e.Element(S3_NAMESPACE + "Name"),
                           CreationDate = (DateTime)e.Element(S3_NAMESPACE + "CreationDate")
                       };
            }
        }

        public IBucket CreateBucket(ICredentials credentials, string bucketName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://s3.amazonaws.com/" + bucketName);
            request.Method = "PUT";
            request.ContentLength = 0;
            S3Helper.SendAndForget(request, credentials);
            return new Bucket(bucketName);
        }
    }
}
