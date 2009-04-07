using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace DasBackupTool.S3
{
    public class Bucket : IBucket
    {
        private String name;
        private DateTime creationDate;

        public Bucket()
        {
        }

        public Bucket(string name)
        {
            this.name = name;
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

        public IEnumerable<IObject> ListObjects(ICredentials credentials)
        {
            IList<IObject> result = new List<IObject>();
            bool truncated = true;
            string marker = null;
            while (truncated)
            {
                string query = marker == null ? "" : "?marker=" + S3Helper.Encode(marker);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://s3.amazonaws.com/" + Name + query);
                XElement xml;
                using (HttpWebResponse response = S3Helper.Send(request, credentials))
                {
                    xml = XElement.Load(new StreamReader(response.GetResponseStream()));
                }
                truncated = (bool)xml.Element(S3.S3_NAMESPACE + "IsTruncated");

                IEnumerable<IObject> objects = from e in xml.Elements(S3.S3_NAMESPACE + "Contents")
                                               select (IObject)new DasBackupTool.S3.Object()
                                               {
                                                   Key = (string)e.Element(S3.S3_NAMESPACE + "Key"),
                                                   LastModified = (DateTime)e.Element(S3.S3_NAMESPACE + "LastModified"),
                                                   ETag = (string)e.Element(S3.S3_NAMESPACE + "ETag"),
                                                   Size = (long)e.Element(S3.S3_NAMESPACE + "Size")
                                               };

                foreach (IObject o in objects)
                {
                    yield return o;
                    marker = o.Key;
                }
            }
        }

        public void PutObject(ICredentials credentials, string name, long size, string type, Stream stream)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://s3.amazonaws.com/" + Name + "/" + S3Helper.Encode(name));
            request.Method = "PUT";
            request.ContentLength = size;
            request.ContentType = type;
            S3Helper.SendAndForget(request, credentials, stream);
        }

        public void DeleteObject(ICredentials credentials, string name)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://s3.amazonaws.com/" + Name + "/" + S3Helper.Encode(name));
            request.Method = "DELETE";
            S3Helper.SendAndForget(request, credentials, HttpStatusCode.NoContent);
        }
    }
}
