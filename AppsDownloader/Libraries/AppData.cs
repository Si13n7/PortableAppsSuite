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

    [Serializable]
    public class AppData : ISerializable
    {
        [NonSerialized]
        private const int DefSize = 0x100000;

        [NonSerialized]
        private const string DefVer = "1.0.0.0";

        [NonSerialized]
        private const string DefWeb = "https://duckduckgo.com/?q=";

        public readonly string Key;
        public string Name;
        public string Description;
        public string Category;
        public string Website;

        public string DisplayVersion;
        public Version PackageVersion;
        public List<Tuple<string, string>> VersionData;

        public string DefaultLanguage;
        public List<string> Languages;

        public Dictionary<string, List<Tuple<string, string>>> DownloadCollection;
        public long DownloadSize;
        public long InstallSize;

        public List<string> Requirements;
        public bool Advanced;

        [NonSerialized]
        public readonly byte[] ServerKey;

        [NonSerialized]
        public readonly AppSettings Settings;

        [NonSerialized]
        private string _installDir;

        public string InstallDir
        {
            get
            {
                if (_installDir != default(string))
                    return _installDir;
                var appDir = Libraries.Settings.CorePaths.AppsDir;
                switch (Key)
                {
                    case "Java":
                    case "Java64":
                        _installDir = Path.Combine(appDir, "CommonFiles", Key);
                        return _installDir;
                }
                if (ServerKey != default(byte[]))
                {
                    appDir = Libraries.Settings.CorePaths.AppDirs.Last();
                    _installDir = Path.Combine(appDir, Key.TrimEnd('#'));
                    return _installDir;
                }
                if (!DownloadCollection.Any())
                    return default(string);
                var downloadUrl = DownloadCollection.First().Value.FirstOrDefault()?.Item1;
                if (downloadUrl?.Any() == true && downloadUrl.GetShortHost()?.EqualsEx(AppSupply.SupplierHosts.Internal) == true)
                    appDir = downloadUrl.ContainsEx("/.repack/") ? Libraries.Settings.CorePaths.AppDirs.Third() : Libraries.Settings.CorePaths.AppDirs.Second();
                _installDir = Path.Combine(appDir, Key);
                return _installDir;
            }
        }

        public AppData(string key,
                       string name,
                       string description,
                       string category,
                       string website = default(string),

                       string displayVersion = default(string),
                       Version packageVersion = default(Version),
                       List<Tuple<string, string>> versionData = default(List<Tuple<string, string>>),

                       string defaultLanguage = default(string),
                       List<string> languages = default(List<string>),

                       Dictionary<string, List<Tuple<string, string>>> downloadCollection = default(Dictionary<string, List<Tuple<string, string>>>),
                       long downloadSize = default(long),
                       long installSize = default(long),

                       List<string> requirements = default(List<string>),
                       bool advanced = default(bool),

                       byte[] serverKey = default(byte[]))
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
                website = DefWeb + WebUtility.UrlEncode(key.TrimEnd('#'));

            if (string.IsNullOrWhiteSpace(displayVersion))
                displayVersion = DefVer;

            if (packageVersion == default(Version))
                packageVersion = new Version(DefVer);

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

            if (downloadSize < DefSize)
                downloadSize = DefSize;

            if (installSize < DefSize)
                installSize = DefSize;

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
            Settings = new AppSettings(this);
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
            Settings = new AppSettings(this);
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // used for custom servers, custom server data should never be cached
            if (ServerKey != null)
                return;

            // cancel if any data is invalid
            if (string.IsNullOrWhiteSpace(Key) ||
                string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Description) ||
                string.IsNullOrWhiteSpace(Category) ||
                string.IsNullOrWhiteSpace(Website) ||

                string.IsNullOrWhiteSpace(DisplayVersion) ||
                PackageVersion == default(Version) ||

                string.IsNullOrWhiteSpace(DefaultLanguage) ||
                Languages?.Any() != true ||

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
            ServerKey != null ? Tuple.Create(Key.ToLower(), ServerKey).GetHashCode() : Key.ToLower().GetHashCode();

        public string ToString(bool formatted)
        {
            var sb = new StringBuilder();

            if (formatted)
            {
                const int width = 3;
                var spacer1 = new string(' ', width);
                var spacer2 = spacer1 + spacer1;
                var spacer3 = spacer2 + spacer1;
                var spacer4 = spacer3 + spacer1;

                sb.Append(spacer1);
                sb.Append(nameof(Key));
                sb.Append(": '");
                sb.Append(Key);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(Name));
                sb.Append(": '");
                sb.Append(Name);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(Description));
                sb.Append(": '");
                sb.Append(Description);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(Category));
                sb.Append(": '");
                sb.Append(Category);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(Website));
                sb.Append(": '");
                sb.Append(Website);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(DisplayVersion));
                sb.Append(": '");
                sb.Append(DisplayVersion);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(PackageVersion));
                sb.Append(": '");
                sb.Append(PackageVersion);
                sb.AppendLine("';");

                if (VersionData != default(List<Tuple<string, string>>) && VersionData.Any())
                {
                    sb.Append(spacer1);
                    sb.Append(nameof(VersionData));
                    sb.AppendLine(":");

                    sb.Append(spacer1);
                    sb.AppendLine("[");

                    for (var i = 0; i < VersionData.Count; i++)
                    {
                        var tuple = VersionData[i];

                        sb.Append(spacer2);
                        sb.Append("'");
                        sb.Append(i);
                        sb.AppendLine("':");

                        sb.Append(spacer2);
                        sb.AppendLine("[");

                        sb.Append(spacer3);
                        sb.Append(nameof(tuple.Item1));
                        sb.Append(": '");
                        sb.Append(tuple.Item1);
                        sb.AppendLine("', ");

                        sb.Append(spacer3);
                        sb.Append(nameof(tuple.Item2));
                        sb.Append(": '");
                        sb.Append(tuple.Item2);
                        sb.AppendLine("'");

                        sb.Append(spacer2);
                        sb.Append("]");
                        if (i < VersionData.Count - 1)
                            sb.AppendLine(",");
                    }
                    sb.AppendLine();

                    sb.Append(spacer1);
                    sb.AppendLine("];");
                }

                sb.Append(spacer1);
                sb.Append(nameof(DefaultLanguage));
                sb.Append(": '");
                sb.Append(DefaultLanguage);
                sb.AppendLine("';");

                if (Languages != default(List<string>) && Languages.Any())
                {
                    sb.Append(spacer1);
                    sb.Append(nameof(Languages));
                    sb.AppendLine(":");

                    sb.Append(spacer1);
                    sb.AppendLine("[");

                    for (var i = 0; i < Languages.Count; i++)
                    {
                        sb.Append(spacer2);
                        sb.Append("'");
                        sb.Append(i);
                        sb.Append("': '");
                        sb.Append(Languages[i]);
                        sb.Append("'");

                        if (i < Languages.Count - 1)
                            sb.AppendLine(",");
                    }
                    sb.AppendLine();

                    sb.Append(spacer1);
                    sb.AppendLine("];");
                }

                if (DownloadCollection != default(Dictionary<string, List<Tuple<string, string>>>) && DownloadCollection.Any())
                {
                    sb.Append(spacer1);
                    sb.Append(nameof(DownloadCollection));
                    sb.AppendLine(": ");

                    sb.Append(spacer1);
                    sb.AppendLine("[");

                    var keyCount = 0;
                    foreach (var pair in DownloadCollection)
                    {
                        sb.Append(spacer2);
                        sb.Append("'");
                        sb.Append(pair.Key);
                        sb.Append("'");

                        if (pair.Value == default(List<Tuple<string, string>>) || !pair.Value.Any())
                            goto Continue;

                        sb.AppendLine(":");

                        sb.Append(spacer2);
                        sb.AppendLine("[");

                        var tupleCount = 0;
                        foreach (var tuple in pair.Value)
                        {
                            sb.Append(spacer3);
                            sb.Append("'");
                            sb.Append(tupleCount);
                            sb.AppendLine("':");

                            sb.Append(spacer3);
                            sb.AppendLine("[");

                            sb.Append(spacer4);
                            sb.Append(nameof(tuple.Item1));
                            sb.Append(": '");
                            sb.Append(tuple.Item1);
                            sb.AppendLine("';");

                            sb.Append(spacer4);
                            sb.Append(nameof(tuple.Item2));
                            sb.Append(": '");
                            sb.Append(tuple.Item2);
                            sb.AppendLine("'");

                            sb.Append(spacer3);
                            sb.Append("]");
                            if (tupleCount < pair.Value.Count - 1)
                                sb.AppendLine(",");
                            tupleCount++;
                        }
                        sb.AppendLine();

                        Continue:
                        sb.Append(spacer2);
                        sb.Append("]");
                        if (keyCount < DownloadCollection.Count - 1)
                            sb.AppendLine(", ");
                        keyCount++;
                    }
                    sb.AppendLine();

                    sb.Append(spacer1);
                    sb.AppendLine("];");
                }

                sb.Append(spacer1);
                sb.Append(nameof(DownloadSize));
                sb.Append(": '");
                sb.Append(DownloadSize.FormatSize(Reorganize.SizeOptions.Trim));
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(InstallDir));
                sb.Append(": '");
                sb.Append(InstallDir);
                sb.AppendLine("';");

                sb.Append(spacer1);
                sb.Append(nameof(InstallSize));
                sb.Append(": '");
                sb.Append(InstallSize.FormatSize(Reorganize.SizeOptions.Trim));
                sb.AppendLine("';");

                if (Requirements != default(List<string>) && Requirements.Any())
                {
                    sb.Append(spacer1);
                    sb.Append(nameof(Requirements));
                    sb.AppendLine(":");

                    sb.Append(spacer1);
                    sb.AppendLine("[");

                    for (var i = 0; i < Requirements.Count; i++)
                    {
                        sb.Append(spacer2);
                        sb.Append("'");
                        sb.Append(i);
                        sb.Append("': '");
                        sb.Append(Requirements[i]);
                        sb.Append("'");

                        if (i < Requirements.Count - 1)
                            sb.AppendLine(",");
                    }
                    sb.AppendLine();

                    sb.Append(spacer1);
                    sb.AppendLine("];");
                }

                sb.Append(spacer1);
                sb.Append(nameof(Advanced));
                sb.Append(": '");
                sb.Append(Advanced);
                sb.Append("'");

                if (ServerKey == default(byte[]))
                    return sb.ToString();

                sb.AppendLine(";");

                sb.Append(spacer1);
                sb.Append(nameof(ServerKey));
                sb.Append(": '");
                sb.Append(ServerKey.ToHexa());
                sb.Append("'");

                return sb.ToString();
            }

            sb.Append("(");

            sb.Append(nameof(Key));
            sb.Append(": '");
            sb.Append(Key);
            sb.Append("'; ");

            sb.Append(nameof(Name));
            sb.Append(": '");
            sb.Append(Name);
            sb.Append("'; ");

            sb.Append(nameof(Description));
            sb.Append(": '");
            sb.Append(Description);
            sb.Append("'; ");

            sb.Append(nameof(Category));
            sb.Append(": '");
            sb.Append(Category);
            sb.Append("'; ");

            sb.Append(nameof(Website));
            sb.Append(": '");
            sb.Append(Website);
            sb.Append("'; ");

            sb.Append(nameof(DisplayVersion));
            sb.Append(": '");
            sb.Append(DisplayVersion);
            sb.Append("'; ");

            sb.Append(nameof(PackageVersion));
            sb.Append(": '");
            sb.Append(PackageVersion);
            sb.Append("'; ");

            if (VersionData != default(List<Tuple<string, string>>) && VersionData.Any())
            {
                sb.Append(nameof(VersionData));
                sb.Append(": [");

                for (var i = 0; i < VersionData.Count; i++)
                {
                    var tuple = VersionData[i];

                    sb.Append("'");
                    sb.Append(i);
                    sb.Append("': [");

                    sb.Append(nameof(tuple.Item1));
                    sb.Append(": '");
                    sb.Append(tuple.Item1);
                    sb.Append("', ");

                    sb.Append(nameof(tuple.Item2));
                    sb.Append(": '");
                    sb.Append(tuple.Item2);
                    sb.Append("']");

                    if (i < VersionData.Count - 1)
                        sb.Append(", ");
                }
                sb.Append("]; ");
            }

            sb.Append(nameof(DefaultLanguage));
            sb.Append(": '");
            sb.Append(DefaultLanguage);
            sb.Append("'; ");

            if (Languages != default(List<string>) && Languages.Any())
            {
                sb.Append(nameof(Languages));
                sb.Append(": [");

                for (var i = 0; i < Languages.Count; i++)
                {
                    sb.Append("'");
                    sb.Append(i);
                    sb.Append("': '");
                    sb.Append(Languages[i]);
                    sb.Append("'");

                    if (i < Languages.Count - 1)
                        sb.Append(", ");
                }
                sb.Append("]; ");
            }

            if (DownloadCollection != default(Dictionary<string, List<Tuple<string, string>>>) && DownloadCollection.Any())
            {
                sb.Append(nameof(DownloadCollection));
                sb.Append(": [");

                var keyCount = 0;
                foreach (var pair in DownloadCollection)
                {
                    sb.Append("'");
                    sb.Append(pair.Key);
                    sb.Append("'");

                    if (pair.Value == default(List<Tuple<string, string>>) || !pair.Value.Any())
                        goto Continue;

                    sb.Append(": [");

                    var tupleCount = 0;
                    foreach (var tuple in pair.Value)
                    {
                        sb.Append("'");
                        sb.Append(tupleCount);
                        sb.Append("': [");

                        sb.Append(nameof(tuple.Item1));
                        sb.Append(": '");
                        sb.Append(tuple.Item1);
                        sb.Append("'; ");

                        sb.Append(nameof(tuple.Item2));
                        sb.Append(": '");
                        sb.Append(tuple.Item2);
                        sb.Append("']");

                        if (tupleCount < pair.Value.Count - 1)
                            sb.Append(", ");
                        tupleCount++;
                    }

                    Continue:
                    sb.Append("]");
                    if (keyCount < DownloadCollection.Count - 1)
                        sb.Append(", ");
                    keyCount++;
                }
                sb.Append("]; ");
            }

            sb.Append(nameof(DownloadSize));
            sb.Append(": '");
            sb.Append(DownloadSize);
            sb.Append("'; ");

            sb.Append(nameof(InstallDir));
            sb.Append(": '");
            sb.Append(InstallDir);
            sb.Append("'; ");

            sb.Append(nameof(InstallSize));
            sb.Append(": '");
            sb.Append(InstallSize);
            sb.Append("'; ");

            if (Requirements != default(List<string>) && Requirements.Any())
            {
                sb.Append(nameof(Requirements));
                sb.Append(": [");

                for (var i = 0; i < Requirements.Count; i++)
                {
                    sb.Append("'");
                    sb.Append(i);
                    sb.Append("': '");
                    sb.Append(Requirements[i]);
                    sb.Append("'");

                    if (i < Requirements.Count - 1)
                        sb.Append(", ");
                }
                sb.Append("]; ");
            }

            sb.Append(nameof(Advanced));
            sb.Append(": '");
            sb.Append(Advanced);
            sb.Append("'");

            if (ServerKey != default(byte[]))
            {
                sb.Append("; ");

                sb.Append(nameof(ServerKey));
                sb.Append(": '");
                sb.Append(ServerKey.ToHexa());
                sb.Append("'");
            }

            sb.Append(")");

            return sb.ToString();
        }

        public override string ToString() =>
            ToString(false);

        public static bool operator ==(AppData left, AppData right) =>
            left is null && right is null || left?.Equals(right) == true;

        public static bool operator !=(AppData left, AppData right) =>
            !(left == right);

        public class AppSettings
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
                        _noUpdatesTime = ReadValue(nameof(NoUpdatesTime), DateTime.MinValue);
                    return _noUpdatesTime;
                }
                set
                {
                    _noUpdatesTime = value;
                    WriteValue(nameof(NoUpdatesTime), _noUpdatesTime, DateTime.MinValue);
                }
            }

            private T ReadValue<T>(string key, T defValue = default(T)) =>
                Ini.Read(_parent.Key, key, defValue);

            private void WriteValue<T>(string key, T value, T defValue = default(T))
            {
                bool equals;
                try
                {
                    equals = value.Equals(defValue);
                }
                catch (NullReferenceException)
                {
                    equals = (dynamic)value == (dynamic)defValue;
                }
                if (equals)
                {
                    Ini.RemoveKey(_parent.Key, key);
                    Ini.WriteDirect(_parent.Key, key, null);
                    Libraries.Settings.CacheData.UpdateSettingsMerges(_parent.Key);
                    return;
                }
                Ini.Write(_parent.Key, key, value);
                Ini.WriteDirect(_parent.Key, key, value);
                Libraries.Settings.CacheData.UpdateSettingsMerges(_parent.Key);
            }
        }
    }
}
