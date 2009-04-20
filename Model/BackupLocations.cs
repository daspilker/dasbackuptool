using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using DasBackupTool.Util;

namespace DasBackupTool.Model
{
    [TypeConverter(typeof(BackupLocationsTypeConverter))]
    public class BackupLocations : IEnumerable<BackupLocation>
    {
        private IDictionary<string, BackupLocation> backupLocations = new Dictionary<string, BackupLocation>();

        public BackupLocations() { }

        public BackupLocations(IEnumerable<BackupLocation> backupLocations)
        {
            foreach (BackupLocation backupLocation in backupLocations)
            {
                this.backupLocations.Add(backupLocation.Path, backupLocation);
            }
        }

        public IEnumerable<BackupLocation> IncludedLocations
        {
            get { return backupLocations.Values.Where(b => !b.Excluded); }
        }

        IEnumerator<BackupLocation> IEnumerable<BackupLocation>.GetEnumerator()
        {
            return backupLocations.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backupLocations.Values.GetEnumerator();
        }

        public void AddBackupLocation(string path, bool excluded)
        {
            backupLocations.Add(path, new BackupLocation(path, excluded));
        }

        public bool IsExcluded(string path)
        {
            return backupLocations.ContainsKey(path) && backupLocations[path].Excluded;
        }
    }

    public class BackupLocation : ICloneable, INotifyPropertyChanged
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

        public bool Exists
        {
            get { return new DasFile(path).Exists; }
        }

        public BitmapImage Icon
        {
            get { return new DasFile(path).Icon; }
        }

        public bool IsDirectory
        {
            get { return new DasFile(path).IsDirectory; }
        }

        public bool Excluded
        {
            get { return excluded; }
            set
            {
                if (excluded != value)
                {
                    excluded = value;
                    NotifyPropertyChanged("Excluded");
                }
            }
        }

        public override bool Equals(object obj)
        {
            BackupLocation other = obj as BackupLocation;
            return other != null && path.Equals(other.path);
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
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
