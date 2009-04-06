using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DasBackupTool.Model
{
    [TypeConverter(typeof(BackupLocationsTypeConverter))]
    public class BackupLocations : IEnumerable<BackupLocation>
    {
        private ICollection<BackupLocation> backupLocations = new HashSet<BackupLocation>();

        IEnumerator<BackupLocation> IEnumerable<BackupLocation>.GetEnumerator()
        {
            return backupLocations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backupLocations.GetEnumerator();
        }

        public void AddBackupLocation(string path, bool excluded)
        {
            backupLocations.Add(new BackupLocation(path, excluded));
        }
    }

    public class BackupLocation
    {
        private string path;
        private bool excluded;

        public BackupLocation(string path, bool excluded)
        {
            this.path = path;
            this.excluded = excluded;
        }

        public string Path
        {
            get { return path; }
        }

        public bool Excluded
        {
            get { return excluded; }
        }

        public override bool Equals(object obj)
        {
            return path.Equals(obj);
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
    }

    public class BackupLocationsTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                BackupLocations result = new BackupLocations();
                string[] locations = ((string)value).Split(';');
                foreach (string location in locations)
                {
                    string[] pathAndExcluded = location.Split(',');
                    result.AddBackupLocation(pathAndExcluded[0], bool.Parse(pathAndExcluded[1]));
                }
                return result;
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
                StringBuilder builder = new StringBuilder();
                foreach (BackupLocation backupLocation in (BackupLocations)value)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(';');
                    }
                    builder.Append(backupLocation.Path).Append(',').Append(backupLocation.Excluded);
                }
                return builder.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
