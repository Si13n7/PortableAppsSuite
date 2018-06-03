namespace AppsDownloader.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using SilDev;
    using GlobalSettings = Settings;

    [Serializable]
    public class AppData : ISerializable
    {
        [NonSerialized]
        private string _installDir;

        [NonSerialized]
        private AppSettings _settings;

        public AppData(string key, string name, string description, string category, string website, string displayVersion, Version packageVersion, List<Tuple<string, string>> versionData, string defaultLanguage, List<string> languages, Dictionary<string, List<Tuple<string, string>>> downloadCollection, long downloadSize, long installSize, List<string> requirements, bool advanced, byte[] serverKey = default(byte[]))
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentNullException(nameof(category));

            if (website?.StartsWithEx("http") != true)
                website = "https://duckduckgo.com/?q=" + WebUtility.UrlEncode(key.TrimEnd('#'));

            if (string.IsNullOrWhiteSpace(displayVersion))
                displayVersion = "1.0.0.0";

            if (packageVersion == default(Version))
                packageVersion = new Version("1.0.0.0");

            if (versionData == default(List<Tuple<string, string>>))
                versionData = new List<Tuple<string, string>>();

            if (string.IsNullOrWhiteSpace(defaultLanguage))
                defaultLanguage = "Default";

            if (languages == default(List<string>))
                languages = new List<string>
                {
                    defaultLanguage
                };

            if (downloadCollection == default(Dictionary<string, List<Tuple<string, string>>>))
                downloadCollection = new Dictionary<string, List<Tuple<string, string>>>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        defaultLanguage,
                        new List<Tuple<string, string>>()
                    }
                };

            if (downloadSize < 0x100000)
                downloadSize = 0x100000;

            if (installSize < 0x100000)
                installSize = 0x100000;

            if (requirements == default(List<string>))
                requirements = new List<string>();

            Key = key;
            Name = name;
            Description = description;
            Category = category;
            Website = website;

            DisplayVersion = displayVersion;
            PackageVersion = packageVersion;
            VersionData = versionData;

            DefaultLanguage = defaultLanguage;
            Languages = languages;

            DownloadCollection = downloadCollection;
            DownloadSize = downloadSize;
            InstallSize = installSize;

            Requirements = requirements;
            Advanced = advanced;

            ServerKey = serverKey;
        }

        protected AppData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            Key = info.GetString(nameof(Key));
            Name = info.GetString(nameof(Name));
            Description = info.GetString(nameof(Description));
            Category = info.GetString(nameof(Category));
            Website = info.GetString(nameof(Website));

            DisplayVersion = info.GetString(nameof(DisplayVersion));
            PackageVersion = (Version)info.GetValue(nameof(PackageVersion), typeof(Version));
            VersionData = (List<Tuple<string, string>>)info.GetValue(nameof(VersionData), typeof(List<Tuple<string, string>>));

            DefaultLanguage = info.GetString(nameof(DefaultLanguage));
            Languages = (List<string>)info.GetValue(nameof(Languages), typeof(List<string>));

            DownloadCollection = (Dictionary<string, List<Tuple<string, string>>>)info.GetValue(nameof(DownloadCollection), typeof(Dictionary<string, List<Tuple<string, string>>>));
            DownloadSize = info.GetInt64(nameof(DownloadSize));
            InstallSize = info.GetInt64(nameof(InstallSize));

            Requirements = (List<string>)info.GetValue(nameof(Requirements), typeof(List<string>));
            Advanced = info.GetBoolean(nameof(Advanced));

            ServerKey = default(byte[]);
        }

        public string Key { get; }

        public string Name { get; }

        public string Description { get; }

        public string Category { get; }

        public string Website { get; }

        public string DisplayVersion { get; }

        public Version PackageVersion { get; }

        public List<Tuple<string, string>> VersionData { get; }

        public string DefaultLanguage { get; }

        public List<string> Languages { get; }

        public Dictionary<string, List<Tuple<string, string>>> DownloadCollection { get; }

        public long DownloadSize { get; }

        public long InstallSize { get; }

        public string InstallDir
        {
            get
            {
                if (_installDir != default(string))
                    return _installDir;
                var appDir = CorePaths.AppsDir;
                switch (Key)
                {
                    case "Java":
                    case "Java64":
                        _installDir = Path.Combine(appDir, "CommonFiles", Key);
                        return _installDir;
                }
                if (ServerKey != default(byte[]))
                {
                    appDir = CorePaths.AppDirs.Last();
                    _installDir = Path.Combine(appDir, Key.TrimEnd('#'));
                    return _installDir;
                }
                if (!DownloadCollection.Any())
                    return default(string);
                var downloadUrl = DownloadCollection.First().Value.FirstOrDefault()?.Item1;
                if (downloadUrl?.Any() == true && downloadUrl.GetShortHost()?.EqualsEx(AppSupply.SupplierHosts.Internal) == true)
                    appDir = downloadUrl.ContainsEx("/.repack/") ? CorePaths.AppDirs.Third() : CorePaths.AppDirs.Second();
                _installDir = Path.Combine(appDir, Key);
                return _installDir;
            }
        }

        public List<string> Requirements { get; }

        public bool Advanced { get; set; }

        public byte[] ServerKey { get; }

        public AppSettings Settings
        {
            get
            {
                if (_settings == default(AppSettings))
                    _settings = new AppSettings(this);
                return _settings;
            }
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // used for custom servers, custom server data should never be serialized
            if (ServerKey != null)
                return;

            // cancel if required data is invalid
            if (Languages?.Any() != true ||
                DownloadCollection?.Any() != true ||
                DownloadCollection.Values.FirstOrDefault()?.Any() != true ||
                DownloadCollection.SelectMany(x => x.Value).Any(x => x?.Item1?.StartsWithEx("http") != true || x.Item2?.Length != 32))
                return;

            // finally serialize valid data
            info.AddValue(nameof(Key), Key);
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Description), Description);
            info.AddValue(nameof(Category), Category);
            info.AddValue(nameof(Website), Website);

            info.AddValue(nameof(DisplayVersion), DisplayVersion);
            info.AddValue(nameof(PackageVersion), PackageVersion);
            info.AddValue(nameof(VersionData), VersionData);

            info.AddValue(nameof(DefaultLanguage), DefaultLanguage);
            info.AddValue(nameof(Languages), Languages);

            info.AddValue(nameof(DownloadCollection), DownloadCollection);
            info.AddValue(nameof(DownloadSize), DownloadSize);
            info.AddValue(nameof(InstallSize), InstallSize);

            info.AddValue(nameof(Requirements), Requirements);
            info.AddValue(nameof(Advanced), Advanced);
        }

        public bool Equals(AppData appData) =>
            Equals(GetHashCode(), appData?.GetHashCode() ?? 0);

        public override bool Equals(object obj)
        {
            if (obj is AppData appData)
                return Equals(appData);
            return false;
        }

        public override int GetHashCode() =>
            ServerKey != default(byte[]) ? Tuple.Create(Key.ToLower(), ServerKey).GetHashCode() : Key.ToLower().GetHashCode();

        public void ToString(StringBuilder sb)
        {
            if (sb == default(StringBuilder))
                return;
            const int width = 3;
            foreach (var pi in GetType().GetProperties())
            {
                var obj = pi.GetValue(this);
                switch (obj)
                {
                    case AppSettings _:
                        continue;
                    case byte[] item:
                    {
                        sb.Append(' ', width);
                        sb.AppendFormat("{0}: '{1}'", pi.Name, item.ToHexa());
                        break;
                    }
                    case List<string> item:
                    {
                        if (!item.Any())
                            continue;

                        sb.Append(' ', width);
                        sb.AppendFormat("{0}:", pi.Name);
                        sb.AppendLine();

                        sb.Append(' ', width);
                        sb.AppendLine("{");
                        for (var i = 0; i < item.Count; i++)
                        {
                            sb.Append(' ', width * 2);
                            if (item.Count > 10 && i < 10)
                                sb.Append(" ");
                            sb.AppendFormat("{0}: '{1}'", i, item[i]);
                            if (i < item.Count - 1)
                                sb.AppendLine(",");
                        }
                        sb.AppendLine();
                        sb.Append(' ', width);
                        sb.AppendLine("},");
                        break;
                    }
                    case List<Tuple<string, string>> item:
                    {
                        if (!item.Any())
                            continue;

                        sb.Append(' ', width);
                        sb.AppendFormat("{0}:", pi.Name);
                        sb.AppendLine();

                        sb.Append(' ', width);
                        sb.AppendLine("{");

                        for (var i = 0; i < item.Count; i++)
                        {
                            var tuple = item[i];

                            sb.Append(' ', width * 2);
                            if (item.Count > 10 && i < 10)
                                sb.Append(" ");
                            sb.AppendFormat("{0}:", i);
                            sb.AppendLine();

                            sb.Append(' ', width * 2);
                            sb.AppendLine("{");

                            sb.Append(' ', width * 3);
                            sb.AppendFormat("Item1: '{0}',", tuple.Item1);
                            sb.AppendLine();

                            sb.Append(' ', width * 3);
                            sb.AppendFormat("Item2: '{0}'", tuple.Item2);
                            sb.AppendLine();

                            sb.Append(' ', width * 2);
                            sb.Append("}");
                            if (i < item.Count - 1)
                                sb.AppendLine(",");
                        }
                        sb.AppendLine();

                        sb.Append(' ', width);
                        sb.AppendLine("},");
                        break;
                    }
                    case Dictionary<string, List<Tuple<string, string>>> item:
                    {
                        if (!item.Any())
                            continue;

                        sb.Append(' ', width);
                        sb.AppendFormat("{0}:", pi.Name);
                        sb.AppendLine();

                        sb.Append(' ', width);
                        sb.AppendLine("{");

                        var keys = item.Keys;
                        for (var i = 0; i < keys.Count; i++)
                        {
                            var key = keys.Just(i);

                            sb.Append(' ', width * 2);
                            sb.AppendFormat("'{0}':", key);
                            if (item.TryGetValue(key, out var value) && value.Any())
                            {
                                sb.AppendLine();

                                sb.Append(' ', width * 2);
                                sb.AppendLine("{");

                                for (var j = 0; j < value.Count; j++)
                                {
                                    var tuple = value[j];

                                    sb.Append(' ', width * 3);
                                    sb.AppendFormat("{0}:", j);
                                    sb.AppendLine();

                                    sb.Append(' ', width * 3);
                                    sb.AppendLine("{");

                                    sb.Append(' ', width * 4);
                                    sb.AppendFormat("Item1: '{0}';", tuple.Item1);
                                    sb.AppendLine();

                                    sb.Append(' ', width * 4);
                                    sb.AppendFormat("Item2: '{0}'", tuple.Item2);
                                    sb.AppendLine();

                                    sb.Append(' ', width * 3);
                                    sb.Append("}");
                                    if (j < value.Count - 1)
                                        sb.AppendLine(",");
                                }
                                sb.AppendLine();
                            }

                            sb.Append(' ', width * 2);
                            sb.Append("}");
                            if (i < keys.Count - 1)
                                sb.AppendLine(", ");
                        }
                        sb.AppendLine();

                        sb.Append(' ', width);
                        sb.AppendLine("},");
                        break;
                    }
                    default:
                    {
                        if (obj == null)
                            continue;
                        var value = obj;
                        if (value is long num)
                            value = num.FormatSize(Reorganize.SizeOptions.Trim);
                        sb.Append(' ', width);
                        sb.AppendFormat("{0}: '{1}'", pi.Name, value);
                        if (pi.Name != nameof(Advanced) || ServerKey != default(byte[]))
                            sb.AppendLine(",");
                        break;
                    }
                }
            }
        }

        public string ToString(bool formatted)
        {
            if (!formatted)
                return Json.String(this);
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public override string ToString() =>
            ToString(false);

        public static bool operator ==(AppData left, AppData right) =>
            left is null && right is null || left?.Equals(right) == true;

        public static bool operator !=(AppData left, AppData right) =>
            !(left == right);

        public sealed class AppSettings
        {
            private readonly AppData _parent;
            private string _archiveLang;
            private bool? _archiveLangConfirmed, _noUpdates;
            private DateTime _noUpdatesTime;

            internal AppSettings(AppData parent) =>
                _parent = parent;

            public string ArchiveLang
            {
                get
                {
                    if (_archiveLang == default(string))
                        _archiveLang = ReadValue(nameof(ArchiveLang), _parent.DefaultLanguage);
                    if (_parent.DownloadCollection?.ContainsKey(_archiveLang) != true)
                        _archiveLang = _parent.DefaultLanguage;
                    return _archiveLang;
                }
                set
                {
                    _archiveLang = value;
                    WriteValue(nameof(ArchiveLang), _archiveLang, _parent.DefaultLanguage);
                }
            }

            public bool ArchiveLangConfirmed
            {
                get
                {
                    if (!_archiveLangConfirmed.HasValue)
                        _archiveLangConfirmed = ReadValue(nameof(ArchiveLangConfirmed), false);
                    return (bool)_archiveLangConfirmed;
                }
                set
                {
                    _archiveLangConfirmed = value;
                    WriteValue(nameof(ArchiveLangConfirmed), _archiveLangConfirmed, false);
                }
            }

            public bool NoUpdates
            {
                get
                {
                    if (!_noUpdates.HasValue)
                        _noUpdates = ReadValue(nameof(NoUpdates), false);
                    return (bool)_noUpdates;
                }
                set
                {
                    _noUpdates = value;
                    WriteValue(nameof(NoUpdates), _noUpdates, false);
                }
            }

            public DateTime NoUpdatesTime
            {
                get
                {
                    if (_noUpdatesTime == default(DateTime))
                        _noUpdatesTime = ReadValue(nameof(NoUpdatesTime), default(DateTime));
                    return _noUpdatesTime;
                }
                set
                {
                    _noUpdatesTime = value;
                    WriteValue(nameof(NoUpdatesTime), _noUpdatesTime);
                }
            }

            private T ReadValue<T>(string key, T defValue = default(T)) =>
                Ini.Read(_parent.Key, key, defValue);

            private void WriteValue<T>(string key, T value, T defValue = default(T)) =>
                GlobalSettings.WriteValue(_parent.Key, key, value, defValue);
        }
    }
}
