using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace DasBackupTool.S3
{
    public class S3Helper
    {
        public static IBucket GetBucket(IEnumerable<IBucket> buckets, string bucketName)
        {
            foreach (IBucket bucket in buckets)
            {
                if (bucket.Name.Equals(bucketName))
                {
                    return bucket;
                }
            }
            return null;
        }

        public static bool ContainsBucket(IEnumerable<IBucket> buckets, string bucketName)
        {
            return GetBucket(buckets, bucketName) != null;
        }

        internal static string Encode(string s)
        {
            return s.Replace("&", "%26").Replace("+", "%2B").Replace("#", "%23");
        }

        internal static void SendAndForget(HttpWebRequest request, ICredentials credentials)
        {
            HttpWebResponse response = Send(request, credentials, null, HttpStatusCode.OK);
            response.Close();
        }

        internal static void SendAndForget(HttpWebRequest request, ICredentials credentials, Stream stream)
        {
            HttpWebResponse response = Send(request, credentials, stream, HttpStatusCode.OK);
            response.Close();
        }

        internal static void SendAndForget(HttpWebRequest request, ICredentials credentials, HttpStatusCode expectedStatusCode)
        {
            HttpWebResponse response = Send(request, credentials, null, expectedStatusCode);
            response.Close();
        }

        internal static HttpWebResponse Send(HttpWebRequest request, ICredentials credentials)
        {
            return Send(request, credentials, null, HttpStatusCode.OK);
        }

        internal static HttpWebResponse Send(HttpWebRequest request, ICredentials credentials, HttpStatusCode expectedStatusCode)
        {
            return Send(request, credentials, null, expectedStatusCode);
        }

        internal static HttpWebResponse Send(HttpWebRequest request, ICredentials credentials, Stream stream)
        {
            return Send(request, credentials, stream, HttpStatusCode.OK);
        }

        internal static HttpWebResponse Send(HttpWebRequest request, ICredentials credentials, Stream stream, HttpStatusCode expectedStatusCode)
        {
            SignRequest(request, credentials);
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

        private static void SignRequest(HttpWebRequest request, ICredentials credentials)
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

            Console.WriteLine(stringToSign.ToString());

            HMACSHA1 hmac = new HMACSHA1(Encoding.ASCII.GetBytes(credentials.SecretAccessKey));
            using (hmac)
            {
                string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString())));

                StringBuilder authorization = new StringBuilder();
                authorization.Append("AWS ");
                authorization.Append(credentials.AccessKeyId);
                authorization.Append(':');
                authorization.Append(signature);
                request.Headers[HttpRequestHeader.Authorization] = authorization.ToString();
                Console.WriteLine(authorization.ToString());
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
                    // todo: canonicalize multi-line headers
                    // todo: handle headers with different capitalization
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
            StringBuilder canonicalizedResource = new StringBuilder();
            // todo: bucket in Host header
            canonicalizedResource.Append(request.RequestUri.AbsolutePath);
            // todo: ?acl and ?torrent sub-resources
            return canonicalizedResource.ToString();
        }

        private static S3Exception NewS3Exception(HttpWebResponse response, Exception exception)
        {
            XElement xml = XElement.Load(new StreamReader(response.GetResponseStream()));
            string message = (string)xml.Element("Message");
            S3Exception result; 
            if (message == "The specified bucket does not exist")
            {
                result = new BucketDoesNotExistException(message, exception);
            }
            else
            {
                result = new S3Exception(message, exception);
            }
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

        private static void CopyStream(Stream from, Stream to)
        {
            byte[] buffer = new byte[4* 4096];
            int read;
            int i = 0;
            while ((read = from.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (i % 10 == 0)
                {
                    Console.Out.WriteLine(i + ": " + read);
                }
                i++;
                to.Write(buffer, 0, read);
                Thread.Sleep(50);
            }
            Console.Out.WriteLine("Done");
        }
    }
}
