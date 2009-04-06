using System;
using System.ComponentModel;
using System.Globalization;

namespace DasBackupTool.Model
{
    [TypeConverter(typeof(BucketTypeConverter))]
    public class Bucket
    {
        private string accessKeyId;
        private string secretAccessKey;
        private string bucketName;

        public Bucket(string accessKeyId, string secretAccessKey, string bucketName)
        {
            this.accessKeyId = accessKeyId;
            this.secretAccessKey = secretAccessKey;
            this.bucketName = bucketName;
        }

        public string AccessKeyId
        {
            get { return accessKeyId; }
        }

        public string SecretAccessKey
        {
            get { return secretAccessKey; }
        }

        public string BucketName
        {
            get { return bucketName; }
        }
    }

    public class BucketTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] locations = ((string)value).Split(';');
                return new Bucket(locations[0], locations[1], locations[2]);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) ? true : base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                Bucket bucket = (Bucket) value;
                return bucket.AccessKeyId + ";" + bucket.SecretAccessKey + ";" + bucket.BucketName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
