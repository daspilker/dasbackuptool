using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DasBackupTool.Aws
{
    public class S3
    {
        private static readonly XNamespace S3_NAMESPACE = "http://s3.amazonaws.com/doc/2006-03-01/";
        private static readonly string S3_BASE_URL = "https://s3.amazonaws.com/";

        private string accessKeyId;
        private string secretAccessKey;

        public S3(string accessKeyId, string secretAccessKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretAccessKey = secretAccessKey;
        }

        public IEnumerable<IS3Bucket> ListBuckets()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(S3_BASE_URL);
            using (HttpWebResponse response = Send(request, null, HttpStatusCode.OK))
            {
                XElement xml = XElement.Load(new StreamReader(response.GetResponseStream()));
                return from e in xml.Element(S3_NAMESPACE + "Buckets").Elements(S3_NAMESPACE + "Bucket") select (IS3Bucket)new S3Bucket(e);
            }
        }

        public void CreateBucket(string bucketName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(S3_BASE_URL + bucketName);
            request.Method = "PUT";
            request.ContentLength = 0;
            HttpWebResponse response = Send(request, null, HttpStatusCode.OK);
            response.Close();
        }

        public IEnumerable<IS3Object> ListObjects(string bucket)
        {
            bool truncated = true;
            string marker = null;
            while (truncated)
            {
                string query = marker == null ? "" : "?marker=" + Encode(marker);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(S3_BASE_URL + bucket + query);
                XElement xml;
                using (HttpWebResponse response = Send(request, null, HttpStatusCode.OK))
                {
                    try
                    {
                        xml = XElement.Load(new StreamReader(response.GetResponseStream()));
                    }
                    catch (IOException e)
                    {
                        throw NewS3Exception(e);
                    }
                }
                truncated = (bool)xml.Element(S3_NAMESPACE + "IsTruncated");

                foreach (S3Object o in from e in xml.Elements(S3_NAMESPACE + "Contents") select new S3Object(e))
                {
                    yield return o;
                    marker = o.Key;
                }
            }
        }

        public void PutObject(string bucket, string key, long size, string type, Stream stream)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(S3_BASE_URL + bucket + "/" + Encode(key));
            request.Method = "PUT";
            request.ContentLength = size;
            request.ContentType = type;
            Send(request, stream, HttpStatusCode.OK).Close();
        }

        public void DeleteObject(string bucket, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(S3_BASE_URL + bucket + "/" + Encode(key));
            request.Method = "DELETE";
            Send(request, null, HttpStatusCode.NoContent).Close();
        }

        private HttpWebResponse Send(HttpWebRequest request, Stream stream, HttpStatusCode expectedStatusCode)
        {
            SignRequest(request);
            request.Timeout = int.MaxValue;
            try
            {
                if (stream != null)
                {
                    Stream requestStream = request.GetRequestStream();
                    using (requestStream)
                    {
                        CopyStream(stream, requestStream);
                    }
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != expectedStatusCode)
                {
                    using (response)
                    {
                        throw NewS3Exception(response, null);
                    }
                }
                return response;
            }
            catch (IOException e)
            {
                throw NewS3Exception(e);
            }
            catch (WebException e)
            {
                throw NewS3Exception(e);
            }
        }

        private void SignRequest(HttpWebRequest request)
        {
            if (request.Headers["x-amz-date"] == null)
            {
                request.Headers["x-amz-date"] = DateTime.UtcNow.ToString("R");
            }

            StringBuilder stringToSign = new StringBuilder();
            stringToSign.Append(request.Method).Append('\n');
            stringToSign.Append(request.Headers[HttpRequestHeader.ContentMd5] ?? "").Append('\n');
            stringToSign.Append(request.ContentType ?? "").Append('\n');
            stringToSign.Append('\n');
            stringToSign.Append(CanonicalizeAmzHeaders(request));
            stringToSign.Append(CanonicalizeResource(request));

            HMACSHA1 hmac = new HMACSHA1(Encoding.ASCII.GetBytes(secretAccessKey));
            using (hmac)
            {
                string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString())));

                StringBuilder authorization = new StringBuilder();
                authorization.Append("AWS ");
                authorization.Append(accessKeyId);
                authorization.Append(':');
                authorization.Append(signature);
                request.Headers[HttpRequestHeader.Authorization] = authorization.ToString();
            }
        }

        private static string CanonicalizeAmzHeaders(HttpWebRequest request)
        {
            SortedDictionary<string, string> amzHeaders = new SortedDictionary<string, string>();
            foreach (String name in request.Headers.Keys)
            {
                string lowerCaseName = name.ToLower();
                if (lowerCaseName.StartsWith("x-amz-"))
                {
                    string[] values = request.Headers.GetValues(name);
                    StringBuilder value = new StringBuilder();
                    foreach (string v in values)
                    {
                        if (value.Length > 0)
                        {
                            value.Append(',');
                        }
                        value.Append(v);
                    }
                    amzHeaders[lowerCaseName] = value.ToString();
                }
            }
            StringBuilder canonicalizedHeaders = new StringBuilder();
            foreach (string name in amzHeaders.Keys)
            {
                canonicalizedHeaders.Append(name);
                canonicalizedHeaders.Append(':');
                canonicalizedHeaders.Append(amzHeaders[name]);
                canonicalizedHeaders.Append('\n');
            }
            return canonicalizedHeaders.ToString();
        }

        private static string CanonicalizeResource(HttpWebRequest request)
        {
            return request.RequestUri.AbsolutePath;
        }

        private static string Encode(string s)
        {
            return s.Replace("&", "%26").Replace("+", "%2B").Replace("#", "%23");
        }

        private static void CopyStream(Stream from, Stream to)
        {
            byte[] buffer = new byte[4 * 4096];
            int read;
            while ((read = from.Read(buffer, 0, buffer.Length)) > 0)
            {
                to.Write(buffer, 0, read);
            }
        }

        private static S3Exception NewS3Exception(HttpWebResponse response, Exception exception)
        {
            XElement xml = XElement.Load(new StreamReader(response.GetResponseStream()));
            string message = (string)xml.Element("Message");
            S3Exception result;
            result = new S3Exception(message, exception);
            result.Data["StatusCode"] = response.StatusCode;
            result.Data["StatusDescription"] = response.StatusDescription;
            result.Data["Code"] = (string)xml.Element("Code");
            result.Data["Resource"] = (string)xml.Element("Resource");
            result.Data["RequestId"] = (string)xml.Element("RequestId");
            return result;
        }

        private static S3Exception NewS3Exception(WebException webException)
        {
            if (webException.Response != null)
            {
                using (webException.Response)
                {
                    return NewS3Exception((HttpWebResponse)webException.Response, webException);
                }
            }
            return new S3Exception(webException.Status.ToString(), webException);
        }

        private static S3Exception NewS3Exception(IOException ioException)
        {
            return new S3Exception(ioException.ToString(), ioException);
        }

        private class S3Bucket : IS3Bucket
        {
            private String name;
            private DateTime creationDate;

            public S3Bucket(XElement element)
            {
                name = (string)element.Element(S3.S3_NAMESPACE + "Name");
                creationDate = (DateTime)element.Element(S3.S3_NAMESPACE + "CreationDate");
            }

            public String Name
            {
                get { return name; }
            }

            public DateTime CreationDate
            {
                get { return creationDate; }
            }
        }

        private class S3Object : IS3Object
        {
            private string key;
            private DateTime lastModified;
            private string eTag;
            private long size;

            public S3Object(XElement element)
            {
                key = (string)element.Element(S3_NAMESPACE + "Key");
                lastModified = (DateTime)element.Element(S3_NAMESPACE + "LastModified");
                eTag = (string)element.Element(S3_NAMESPACE + "ETag");
                size = (long)element.Element(S3_NAMESPACE + "Size");
            }

            public string Key
            {
                get { return key; }
            }

            public DateTime LastModified
            {
                get { return lastModified; }
            }

            public string ETag
            {
                get { return eTag; }
            }

            public long Size
            {
                get { return size; }
            }
        }
    }

    public interface IS3Bucket
    {
        String Name { get; }
        DateTime CreationDate { get; }
    }

    public interface IS3Object
    {
        string Key { get; }
        DateTime LastModified { get; }
        string ETag { get; }
        long Size { get; }
    }

    public class S3Exception : Exception
    {
        public S3Exception(string message, Exception exception) : base(message, exception) { }
    }
}