namespace AppsDownloader.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using LangResources;
    using SilDev;

    internal static class Settings
    {
        internal const string Section = "Downloader";
        private static string _currentDirectory, _iconResourcePath, _language, _machineId, _registryPath, _repositoryPath, _title;
        private static bool? _highlightInstalled, _showGroups, _showGroupColors;

        internal static string CurrentDirectory
        {
            get
            {
                if (_currentDirectory == default(string))
                    _currentDirectory = Ini.Read<string>("Launcher", nameof(CurrentDirectory));
                return _currentDirectory;
            }
        }

        internal static bool DeveloperVersion =>
            Ini.Read("Launcher", nameof(DeveloperVersion), false);

        internal static long FreeDiskSpace
        {
            get
            {
                try
                {
                    var appsDrive = DriveInfo.GetDrives().FirstOrDefault(x => CorePaths.AppsDir.StartsWithEx(x.Name));
                    if (appsDrive == default(DriveInfo))
                        throw new ArgumentNullException(nameof(appsDrive));
                    return appsDrive.TotalFreeSpace;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return 0L;
                }
            }
        }

        internal static bool HighlightInstalled
        {
            get
            {
                if (_highlightInstalled.HasValue)
                    return (bool)_highlightInstalled;
                _highlightInstalled = Ini.Read(Section, nameof(HighlightInstalled), true);
                return (bool)_highlightInstalled;
            }
            set
            {
                _highlightInstalled = value;
                WriteValue(Section, nameof(HighlightInstalled), (bool)_highlightInstalled, true);
            }
        }

        internal static string IconResourcePath
        {
            get
            {
                if (_iconResourcePath != default(string))
                    return _iconResourcePath;
                _iconResourcePath = Ini.Read<string>("Launcher", nameof(IconResourcePath), "%system%");
                return _iconResourcePath;
            }
            set
            {
                _iconResourcePath = value;
                WriteValue("Launcher", nameof(IconResourcePath), _iconResourcePath, "%system%");
            }
        }

        internal static string Language
        {
            get
            {
                if (_language == default(string))
                    _language = Ini.Read<string>("Launcher", nameof(Language), global::Language.SystemLang);
                return _language;
            }
        }

        internal static string MachineId
        {
            get
            {
                if (_machineId == default(string))
                    _machineId = EnvironmentEx.MachineId.ToString().EncryptToMd5().Substring(24);
                return _machineId;
            }
        }

        internal static string RegistryPath
        {
            get
            {
                if (_registryPath == default(string))
                    _registryPath = Path.Combine("HKCU", "SOFTWARE", "Portable Apps Suite", PathEx.LocalPath.GetHashCode().ToString());
                return _registryPath;
            }
        }

        internal static string RepositoryUrl
        {
            get
            {
                if (_repositoryPath == default(string))
                    _repositoryPath = "https://github.com/Si13n7/PortableAppsSuite/raw/master";
                return _repositoryPath;
            }
        }

        internal static int ScreenDpi
        {
            get
            {
                int dpi;
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var max = Math.Max(g.DpiX, g.DpiY);
                    dpi = (int)Math.Ceiling(max);
                }
                return dpi;
            }
        }

        internal static bool ShowGroups
        {
            get
            {
                if (_showGroups.HasValue)
                    return (bool)_showGroups;
                _showGroups = Ini.Read(Section, nameof(ShowGroups), true);
                return (bool)_showGroups;
            }
            set
            {
                _showGroups = value;
                WriteValue(Section, nameof(ShowGroups), (bool)_showGroups, true);
            }
        }

        internal static bool ShowGroupColors
        {
            get
            {
                if (_showGroupColors.HasValue)
                    return (bool)_showGroupColors;
                _showGroupColors = Ini.Read(Section, nameof(ShowGroupColors), false);
                return (bool)_showGroupColors;
            }
            set
            {
                _showGroupColors = value;
                WriteValue(Section, nameof(ShowGroupColors), (bool)_showGroupColors);
            }
        }

        internal static bool SkipWriteValue { get; set; }

        internal static string Title
        {
            get
            {
                if (_title == default(string))
#if x86
                    _title = "Apps Downloader";
#else
                    _title = "Apps Downloader (64-bit)";
#endif
                return _title;
            }
            set => _title = value;
        }

        internal static void Initialize()
        {
            Log.FileDir = Path.Combine(CorePaths.TempDir, "Logs");

            Ini.SetFile(PathEx.LocalDir, "..", "Settings.ini");
            Ini.SortBySections = new[]
            {
                Section,
                "Launcher"
            };

            Log.AllowLogging(Ini.FilePath, "DebugMode", Ini.GetRegex(false));

#if x86
            if (Environment.Is64BitOperatingSystem)
            {
                var appsDownloader64 = PathEx.Combine(PathEx.LocalDir, $"{ProcessEx.CurrentName}64.exe");
                if (File.Exists(appsDownloader64))
                {
                    ProcessEx.Start(appsDownloader64, EnvironmentEx.CommandLine(false));
                    Environment.ExitCode = 0;
                    Environment.Exit(Environment.ExitCode);
                }
            }
#endif

            if (Recovery.AppsSuiteIsHealthy())
                return;
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        internal static string GetConfigKey(params string[] keys)
        {
            if (keys == null || !keys.Any())
                throw new ArgumentNullException(nameof(keys));
            if (keys.Length == 1)
                return keys.First();
            var sb = new StringBuilder();
            var len = keys.Length - 1;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                sb.Append(key);
                if (i < len)
                    sb.Append('.');
            }
            return sb.ToString();
        }

        private static int ValidateValue(int value, int minValue, int maxValue)
        {
            var current = Math.Max(value, minValue);
            return Math.Min(current, maxValue);
        }

        private static void WriteValue<T>(string section, string key, T value, T defValue = default(T))
        {
            if (SkipWriteValue)
                return;
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
                Ini.RemoveKey(section, key);
                Ini.WriteDirect(section, key, null);
                CacheData.UpdateSettingsMerges(section);
                return;
            }
            Ini.Write(section, key, value);
            Ini.WriteDirect(section, key, value);
            CacheData.UpdateSettingsMerges(section);
        }

        internal struct ActionGuid
        {
            internal const string AllowNewInstance = "{0CA7046C-4776-4DB0-913B-D8F81964F8EE}";
            internal const string DisallowInterface = "{9AB50CEB-3D99-404E-BD31-4E635C09AF0F}";
            internal const string SystemIntegration = "{3A51735E-7908-4DF5-966A-9CA7626E4E3D}";
            internal const string FileTypeAssociation = "{DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC}";
            internal const string RestoreFileTypes = "{A00C02E5-283A-44ED-9E4D-B82E8F87318F}";
            internal const string RepairAppsSuite = "{FB271A84-B5A3-47DA-A873-9CE946A64531}";
            internal const string RepairDirs = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7}";
            internal const string RepairVariable = "{EA48C7DB-AD36-43D7-80A1-D6E81FB8BCAB}";
            internal const string UpdateInstance = "{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}";

            internal static bool IsUpdateInstance =>
                IsActive(UpdateInstance);

            private static bool IsActive(string guid)
            {
                try
                {
                    var args = Environment.GetCommandLineArgs().Skip(1);
                    return args.Contains(guid);
                }
                catch
                {
                    return false;
                }
            }
        }

        internal static class CacheData
        {
            private static Dictionary<string, Image> _appImages;
            private static List<AppData> _appInfo;
            private static string[] _appInfoKeys;
            private static List<string> _settingsMerges;
            private static readonly List<Tuple<ResourcesEx.IconIndex, bool, Icon>> Icons = new List<Tuple<ResourcesEx.IconIndex, bool, Icon>>();
            private static readonly List<Tuple<ResourcesEx.IconIndex, bool, Image>> Images = new List<Tuple<ResourcesEx.IconIndex, bool, Image>>();

            internal static Dictionary<string, Image> AppImages
            {
                get
                {
                    if (_appImages == default(Dictionary<string, Image>))
                        _appImages = new Dictionary<string, Image>();
                    if (!_appImages.Any() && File.Exists(CachePaths.AppImages))
                        _appImages = FileEx.ReadAllBytes(File.Exists(CachePaths.AppImages) ? CachePaths.AppImages : CorePaths.AppImages)?.DeserializeObject<Dictionary<string, Image>>();
                    return _appImages;
                }
            }

            internal static List<AppData> AppInfo
            {
                get
                {
                    if (_appInfo != null)
                        return _appInfo;
                    UpdateAppInfo();
                    return _appInfo ?? (_appInfo = new List<AppData>());
                }
                set => _appInfo = value;
            }

            internal static string[] AppInfoKeys
            {
                get
                {
                    if (_appInfoKeys?.Any() != true)
                        _appInfoKeys = AppInfo.Select(x => x.Key).ToArray();
                    return _appInfoKeys;
                }
                set => _appInfoKeys = value;
            }

            internal static List<string> SettingsMerges
            {
                get
                {
                    if (_settingsMerges != default(List<string>))
                        return _settingsMerges;
                    if (File.Exists(CachePaths.SettingsMerges))
                        _settingsMerges = FileEx.ReadAllBytes(CachePaths.SettingsMerges)?.DeserializeObject<List<string>>();
                    if (_settingsMerges == default(List<string>))
                        _settingsMerges = new List<string>();
                    return _settingsMerges;
                }
            }

            internal static Icon GetSystemIcon(ResourcesEx.IconIndex index, bool large = false)
            {
                Icon icon;
                if (Icons.Any())
                {
                    icon = Icons.FirstOrDefault(x => x.Item1.Equals(index) && x.Item2.Equals(large))?.Item3;
                    if (icon != default(Icon))
                        goto Return;
                }
                icon = ResourcesEx.GetSystemIcon(index, large, IconResourcePath);
                if (icon == default(Icon))
                    goto Return;
                var tuple = Tuple.Create(index, large, icon);
                Icons.Add(tuple);
                Return:
                return icon;
            }

            internal static Image GetSystemImage(ResourcesEx.IconIndex index, bool large = false)
            {
                Image image;
                if (Images.Any())
                {
                    image = Images.FirstOrDefault(x => x.Item1.Equals(index) && x.Item2.Equals(large))?.Item3;
                    if (image != default(Image))
                        goto Return;
                }
                image = GetSystemIcon(index, large)?.ToBitmap();
                if (image == default(Image))
                    goto Return;
                var tuple = Tuple.Create(index, large, image);
                Images.Add(tuple);
                Return:
                return image;
            }

            internal static void UpdateAppImages(bool force = false)
            {
                var fileDate = File.Exists(CachePaths.AppImages) ? File.GetLastWriteTime(CachePaths.AppImages) : DateTime.MinValue;
                foreach (var mirror in AppSupply.GetMirrors(AppSupply.Suppliers.Internal))
                {
                    var link = PathEx.AltCombine(mirror, "Downloads", "Portable%20Apps%20Suite", ".free", "AppImages.dat");
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: Looking for '{link}'.");
                    if (!NetEx.FileIsAvailable(link, 30000))
                        continue;
                    if (!force && !((NetEx.GetFileDate(link) - fileDate).TotalSeconds > 0d))
                        return;
                    NetEx.Transfer.DownloadFile(link, CachePaths.AppImages, 60000, null, false);
                    if (!File.Exists(CachePaths.AppImages))
                        continue;
                    File.SetLastWriteTime(CachePaths.AppImages, DateTime.Now);
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: '{CachePaths.AppImages}' has been updated.");
                    return;
                }
                var repoLink = PathEx.AltCombine(RepositoryUrl, "AppImages.ini");
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: Looking for '{repoLink}'.");
                if (!NetEx.FileIsAvailable(repoLink, 60000) || !force && !((NetEx.GetFileDate(repoLink) - fileDate).TotalSeconds > 0d))
                    return;
                NetEx.Transfer.DownloadFile(repoLink, CachePaths.AppImages, 60000, null, false);
                if (!File.Exists(CachePaths.AppImages))
                    return;
                File.SetLastWriteTime(CachePaths.AppImages, DateTime.Now);
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: '{CachePaths.AppImages}' has been updated.");
            }

            internal static void UpdateAppInfo(bool force = false)
            {
                ResetAppInfoIntern(force);
                if (AppInfo?.Count > 430)
                    goto Shareware;

                foreach (var mirror in AppSupply.GetMirrors(AppSupply.Suppliers.Internal))
                {
                    var link = PathEx.AltCombine(mirror, "Downloads", "Portable%20Apps%20Suite", ".free", "AppInfo.ini");
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: Looking for '{link}'.");
                    if (NetEx.FileIsAvailable(link, 30000))
                        NetEx.Transfer.DownloadFile(link, CachePaths.AppInfo, 60000, null, false);
                    if (!File.Exists(CachePaths.AppInfo))
                        continue;
                    break;
                }
                var blacklist = new string[0];
                if (!File.Exists(CachePaths.AppInfo))
                {
                    var link = PathEx.AltCombine(RepositoryUrl, "AppInfo.ini");
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: Looking for '{link}'.");
                    if (NetEx.FileIsAvailable(link, 60000))
                        NetEx.Transfer.DownloadFile(link, CachePaths.AppInfo, 60000, null, false);
                }
                if (File.Exists(CachePaths.AppInfo))
                {
                    blacklist = Ini.GetSections(CachePaths.AppInfo).Where(x => Ini.Read(x, "Disabled", false, CachePaths.AppInfo)).ToArray();
                    UpdateAppInfoIntern(CachePaths.AppInfo, blacklist);
                }

                var tmpDir = Path.Combine(CorePaths.TempDir, PathEx.GetTempDirName());
                if (!DirectoryEx.Create(tmpDir))
                    return;
                var tmpZip = Path.Combine(tmpDir, "AppInfo.7z");
                foreach (var mirror in AppSupply.GetMirrors(AppSupply.Suppliers.Internal))
                {
                    var link = PathEx.AltCombine(mirror, "Downloads", "Portable%20Apps%20Suite", ".free", "PortableAppsInfo.7z");
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: Looking for '{link}'.");
                    if (NetEx.FileIsAvailable(link, 30000))
                        NetEx.Transfer.DownloadFile(link, tmpZip, 60000, null, false);
                    if (!File.Exists(tmpZip))
                        continue;
                    break;
                }
                if (!File.Exists(tmpZip))
                {
                    var link = PathEx.AltCombine(AppSupply.GetHost(AppSupply.Suppliers.PortableApps), "updater", "update.7z");
                    if (Log.DebugMode > 0)
                        Log.Write($"Cache: Looking for '{link}'.");
                    if (NetEx.FileIsAvailable(link, 60000))
                        NetEx.Transfer.DownloadFile(link, tmpZip, 60000, null, false);
                }
                if (File.Exists(tmpZip))
                {
                    using (var p = Compaction.Zip7Helper.Unzip(tmpZip, tmpDir))
                        if (!p?.HasExited == true)
                            p.WaitForExit();
                    FileEx.TryDelete(tmpZip);
                }
                var tmpIni = DirectoryEx.GetFiles(tmpDir, "*.ini").FirstOrDefault();
                if (!File.Exists(tmpIni))
                {
                    DirectoryEx.TryDelete(tmpDir);
                    return;
                }
                UpdateAppInfoIntern(tmpIni, blacklist);

                FileEx.WriteAllBytes(CachePaths.AppInfo, AppInfo.SerializeObject().Zip());
                DirectoryEx.TryDelete(tmpDir);

                Shareware:
                if (!Shareware.Enabled)
                    return;

                foreach (var srv in Shareware.GetAddresses())
                {
                    var key = Shareware.FindAddressKey(srv);
                    var usr = Shareware.GetUser(srv);
                    var pwd = Shareware.GetPassword(srv);
                    var url = PathEx.AltCombine(srv, "AppInfo.ini");
                    if (Log.DebugMode > 0)
                        Log.Write($"Shareware: Looking for '{{{key.ToHexa()}}}/AppInfo.ini'.");
                    if (!NetEx.FileIsAvailable(url, usr, pwd, 60000))
                        continue;
                    var appInfo = NetEx.Transfer.DownloadString(url, usr, pwd);
                    if (string.IsNullOrWhiteSpace(appInfo))
                        continue;
                    UpdateAppInfoIntern(appInfo, null, key);
                }

                AppInfoKeys = null;
            }

            private static void ResetAppInfoIntern(bool force = false)
            {
                if (force || ActionGuid.IsUpdateInstance || !File.Exists(CachePaths.AppInfo))
                    goto Reset;

                try
                {
                    var appInfo = File.ReadAllBytes(CachePaths.AppInfo).Unzip().DeserializeObject<List<AppData>>();
                    if (appInfo == null)
                        throw new ArgumentNullException(nameof(appInfo));
                    if (appInfo.Count < 430)
                        throw new ArgumentOutOfRangeException(nameof(appInfo));

                    var fileInfo = new FileInfo(CachePaths.AppInfo);
                    if ((DateTime.Now - fileInfo.LastWriteTime).TotalHours >= 1d)
                        goto Reset;

                    AppInfo = appInfo;
                    return;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                Reset:
                AppInfo = new List<AppData>();
                FileEx.TryDelete(CachePaths.AppInfo);
            }

            private static void UpdateAppInfoIntern(string iniFileOrContent, string[] blacklist = null, byte[] serverKey = null)
            {
                foreach (var section in Ini.GetSections(iniFileOrContent))
                {
                    if (blacklist?.ContainsEx(section) == true || section.EqualsEx("sPortable") || section.ContainsEx(AppSupply.GetHost(AppSupply.Suppliers.PortableApps), "ByPortableApps"))
                        continue;

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                 Name                   |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var name = Ini.Read(section, "Name", iniFileOrContent);
                    if (string.IsNullOrWhiteSpace(name) || name.ContainsEx("jPortable Launcher"))
                        continue;
                    if (!name.StartsWithEx("jPortable", AppSupply.GetHost(AppSupply.Suppliers.PortableApps)))
                    {
                        var newName = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(name, string.Empty);
                        if (!string.IsNullOrWhiteSpace(newName))
                            newName = Regex.Replace(newName, @"\s+", " ").Trim().TrimEnd(',');
                        if (!string.IsNullOrWhiteSpace(newName) && !newName.Equals(name))
                            name = newName;
                    }

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |              Description               |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var description = Ini.Read(section, "Description", iniFileOrContent);
                    if (string.IsNullOrWhiteSpace(description))
                        continue;
                    switch (section)
                    {
                        case "LibreCADPortable":
                            description = description.LowerText("tool");
                            break;
                        case "Mp3spltPortable":
                            description = description.UpperText("mp3", "ogg");
                            break;
                        case "SumatraPDFPortable":
                            description = description.LowerText("comic", "book", "e-", "reader");
                            break;
                        case "WinCDEmuPortable":
                            description = description.UpperText("cd/dvd/bd");
                            break;
                        case "WinDjViewPortable":
                            description = description.UpperText("djvu");
                            break;
                    }
                    description = $"{description.Substring(0, 1).ToUpper()}{description.Substring(1)}";

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                Category                |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var category = serverKey != null ? "*Shareware" : Ini.Read(section, "Category", iniFileOrContent);
                    if (string.IsNullOrWhiteSpace(category) || AppInfo.Any(x => x.Key.EqualsEx(section) && x.Category.EqualsEx(category)))
                        continue;

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                Website                 |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var website = Ini.Read(section, "Website", iniFileOrContent);
                    if (string.IsNullOrWhiteSpace(website))
                        website = Ini.Read(section, "URL", iniFileOrContent).Replace("https", "http");
                    if (string.IsNullOrWhiteSpace(website) || website.Any(char.IsUpper))
                        website = null;

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                Version                 |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var displayVersion = Ini.Read(section, "Version", iniFileOrContent);
                    var packageVersion = default(Version);
                    var versionData = new List<Tuple<string, string>>();
                    if (!string.IsNullOrWhiteSpace(displayVersion))
                        if (!ActionGuid.IsUpdateInstance)
                            packageVersion = new Version("1.0.0.0");
                        else
                        {
                            if (char.IsDigit(displayVersion.FirstOrDefault()))
                                try
                                {
                                    var version = displayVersion;
                                    while (version.Count(x => x == '.') < 3)
                                        version += ".0";
                                    packageVersion = new Version(version);
                                }
                                catch
                                {
                                    packageVersion = new Version("1.0.0.0");
                                }
                            else
                                packageVersion = new Version("0.0.0.0");

                            var verData = Ini.Read(section, "VersionData", iniFileOrContent);
                            if (!string.IsNullOrWhiteSpace(verData))
                            {
                                var verHash = Ini.Read(section, "VersionHash", iniFileOrContent);
                                if (!string.IsNullOrWhiteSpace(verHash))
                                    if (!verData.Contains(',') || !verHash.Contains(','))
                                        versionData.Add(Tuple.Create(verData, verHash));
                                    else
                                    {
                                        var dataArray = verData.Split(',');
                                        var hashArray = verHash.Split(',');
                                        if (dataArray.Length == hashArray.Length)
                                            versionData.AddRange(dataArray.Select((data, i) => Tuple.Create(data, hashArray[i])));
                                    }
                            }
                        }
                    if (string.IsNullOrWhiteSpace(displayVersion) || packageVersion == null)
                    {
                        displayVersion = Ini.Read(section, "DisplayVersion", iniFileOrContent);
                        packageVersion = Ini.Read(section, "PackageVersion", default(Version), iniFileOrContent);
                    }

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                 Paths                  |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var path = Ini.Read(section, "ArchivePath", iniFileOrContent);
                    string hash;
                    string altPath;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        if (path.StartsWithEx(".free", ".repack"))
                            path = PathEx.AltCombine(AppSupply.GetMirrors(AppSupply.Suppliers.Internal).First(), "Downloads", "Portable%20Apps%20Suite", path);
                        hash = Ini.Read(section, "ArchiveHash", iniFileOrContent);
                        altPath = null;
                    }
                    else
                    {
                        path = Ini.Read(section, "DownloadPath", iniFileOrContent);
                        var file = Ini.Read(section, "DownloadFile", iniFileOrContent);
                        altPath = PathEx.AltCombine(default(char[]), path, file);
                        path = PathEx.AltCombine(default(char[]), GetRealUrlIntern(path, section), file);
                        if (!path.EndsWithEx(".paf.exe"))
                            continue;
                        hash = Ini.Read(section, "Hash", iniFileOrContent);
                    }
                    if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(hash))
                        continue;

                    var langPaths = new Dictionary<string, string[]>();
                    foreach (var lang in global::Language.GetText(nameof(en_US.availableLangs)).Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(lang))
                            continue;
                        var langFile = Ini.Read(section, $"DownloadFile_{lang}", iniFileOrContent);
                        if (!langFile.EndsWithEx(".paf.exe"))
                            continue;
                        var langFileHash = Ini.Read(section, $"Hash_{lang}", iniFileOrContent);
                        if (string.IsNullOrWhiteSpace(langFileHash) || langFileHash.EqualsEx(hash))
                            continue;
                        var langPath = GetRealUrlIntern(Ini.Read(section, "DownloadPath", iniFileOrContent), section);
                        var altLangPath = PathEx.AltCombine(default(char[]), langPath, langFile);
                        langPath = PathEx.AltCombine(default(char[]), langPath, langFile);
                        if (!langPath.EndsWithEx(".paf.exe"))
                            continue;
                        langPaths.Add(lang, new[]
                        {
                            langPath,
                            altLangPath,
                            langFileHash
                        });
                    }

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                 Sizes                  |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var downloadSize = Ini.Read(section, "DownloadSize", 1L, iniFileOrContent) * 1024 * 1024;
                    if (downloadSize < 1048576)
                        downloadSize = 1048576;

                    var installSize = Ini.Read(section, "InstallSizeTo", 0L, iniFileOrContent);
                    if (installSize == 0)
                        installSize = Ini.Read(section, "InstallSize", 1L, iniFileOrContent) * 1024 * 1024;
                    if (installSize < 1048576)
                        installSize = 1048576;

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                  Misc                  |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var defLanguage = default(string);
                    if (langPaths.Any())
                        defLanguage = path.ContainsEx("Multilingual") ? "Default (Multilingual)" : "Default (English)";

                    var requires = Ini.Read(section, "Requires", default(string), iniFileOrContent);
                    var requirements = new List<string>();
                    if (!string.IsNullOrEmpty(requires))
                    {
                        if (!requires.Contains(","))
                            requires += ",";
                        var values = requires.Split(',');
                        foreach (var str in values)
                        {
                            if (string.IsNullOrWhiteSpace(str))
                                continue;
                            var value = str;
                            if (!value.Contains("|"))
                                value += "|";
                            var items = value.Split('|');
                            string item;
                            if (Environment.Is64BitOperatingSystem && items.Any(x => x.EndsWith("64")))
                                item = items.FirstOrDefault(x => x.EndsWith("64"))?.Trim();
                            else
                                item = items.FirstOrDefault(x => !x.EndsWith("64"))?.Trim();
                            if (string.IsNullOrEmpty(item))
                                continue;
                            requirements.Add(item);
                        }
                    }

                    var advanced = Ini.Read(section, "Advanced", false, iniFileOrContent);
                    if (!advanced && (displayVersion.EqualsEx("Discontinued") || displayVersion.ContainsEx("Nightly", "Alpha", "Beta")))
                        advanced = true;

                    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~.
                    |                                                 |
                    |                   C R E A T E                   |
                    |                                                 |
                    '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
                    var appInfo = new AppData(section,
                                              name,
                                              description,
                                              category,
                                              website,
                                              displayVersion,
                                              packageVersion,
                                              versionData,
                                              defLanguage,
                                              null,
                                              null,
                                              downloadSize,
                                              installSize,
                                              requirements,
                                              advanced,
                                              serverKey);

                    var defLang = appInfo.Languages.First();
                    appInfo.DownloadCollection[defLang].Add(Tuple.Create(path, hash));
                    if (altPath.StartsWithEx("http") && !altPath.EqualsEx(path))
                        appInfo.DownloadCollection[defLang].Add(Tuple.Create(altPath, hash));

                    if (!langPaths.Any())
                        goto Add;

                    foreach (var item in langPaths)
                    {
                        if (appInfo.Languages.ContainsEx(item.Key) || appInfo.DownloadCollection.ContainsKey(item.Key))
                            continue;

                        appInfo.Languages.Add(item.Key);

                        var langPath = item.Value.FirstOrDefault();
                        var langHash = item.Value.LastOrDefault();
                        appInfo.DownloadCollection.Add(item.Key, new List<Tuple<string, string>>
                        {
                            Tuple.Create(langPath, langHash)
                        });

                        var langAltPath = item.Value.SecondOrDefault();
                        if (langAltPath.StartsWithEx("http") && !langAltPath.EqualsEx(langPath))
                            appInfo.DownloadCollection[item.Key].Add(Tuple.Create(langAltPath, langHash));
                    }

                    Add:
                    AppInfo.Add(appInfo);
                    if (Log.DebugMode > 1)
                        Log.Write($"AppInfo has been added:{Environment.NewLine}{appInfo.ToString(true)}");
                }

                Ini.Detach(iniFileOrContent);
            }

            private static string GetRealUrlIntern(string url, string key = null)
            {
                var realUrl = url;
                var redirect = realUrl.ContainsEx("/redirect/");
                if (string.IsNullOrWhiteSpace(realUrl) || redirect && realUrl.ContainsEx("&d=sfpa"))
                {
                    var host = AppSupply.GetHost(AppSupply.Suppliers.SourceForge);
                    realUrl = PathEx.AltCombine(default(char[]), "http:", $"downloads.{host}", "portableapps");
                    return realUrl;
                }
                if (redirect && realUrl.ContainsEx("&d=pa&f="))
                {
                    var host = AppSupply.GetHost(AppSupply.Suppliers.PortableApps);
                    realUrl = PathEx.AltCombine(default(char[]), "http:", $"downloads.{host}", "portableapps", key);
                    return realUrl;
                }
                if (redirect)
                    try
                    {
                        var filter = WebUtility.UrlDecode(realUrl)?.RemoveChar(':').Replace("https", "http").Split("http/")?.Last()?.RemoveText("/&d=pb&f=").Trim('/');
                        if (!string.IsNullOrEmpty(filter))
                            realUrl = PathEx.AltCombine(default(char[]), "http:", filter);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                if (!realUrl.ContainsEx(AppSupply.GetHost(AppSupply.Suppliers.PortableApps)))
                    return realUrl;
                var mirrors = AppSupply.GetMirrors(AppSupply.Suppliers.PortableApps);
                var first = mirrors.First();
                realUrl = mirrors.Aggregate(realUrl, (c, m) => c.Replace(m, first));
                return realUrl;
            }

            internal static void UpdateSettingsMerges(string section)
            {
                if (!ProcessEx.IsRunning(CorePaths.AppsLauncher))
                    return;
                if (!File.Exists(CachePaths.SettingsMerges))
                    SettingsMerges.Clear();
                if (!SettingsMerges.Contains(section, StringComparer.CurrentCultureIgnoreCase))
                    SettingsMerges.Add(section);
                var bytes = SettingsMerges.SerializeObject();
                if (bytes != null)
                    FileEx.WriteAllBytes(CachePaths.SettingsMerges, bytes);
            }
        }

        internal static class CachePaths
        {
            private static string _appImages, _appInfo, _settingsMerges, _swData;

            internal static string AppImages
            {
                get
                {
                    if (_appImages == default(string))
                        _appImages = Path.Combine(CorePaths.TempDir, "AppImages.dat");
                    return _appImages;
                }
            }

            internal static string AppInfo
            {
                get
                {
                    if (_appInfo == default(string))
                        _appInfo = Path.Combine(CorePaths.TempDir, $"AppInfo{Convert.ToInt32(ActionGuid.IsUpdateInstance)}.dat");
                    return _appInfo;
                }
            }

            internal static string SettingsMerges
            {
                get
                {
                    if (_settingsMerges == default(string))
                        _settingsMerges = Path.Combine(CorePaths.TempDir, "SettingsMerges.dat");
                    return _settingsMerges;
                }
            }

            internal static string SwData
            {
                get
                {
                    if (_swData == default(string))
                        _swData = Path.Combine(CorePaths.TempDir, nameof(SwData), MachineId, "SwData.dat");
                    return _swData;
                }
            }
        }

        internal static class CorePaths
        {
            private static string[] _appDirs;
            private static string _appsDir, _appImages, _appsLauncher, _appsSuiteUpdater, _archiver, _homeDir, _systemExplorer, _systemRestore, _tempDir;

            internal static string AppsDir
            {
                get
                {
                    if (_appsDir == default(string))
                        _appsDir = Path.Combine(HomeDir, "Apps");
                    return _appsDir;
                }
            }

            internal static string[] AppDirs
            {
                get
                {
                    if (_appDirs != default(string[]))
                        return _appDirs;
                    _appDirs = new[]
                    {
                        AppsDir,
                        Path.Combine(AppsDir, ".free"),
                        Path.Combine(AppsDir, ".repack"),
                        Path.Combine(AppsDir, ".share")
                    };
                    return _appDirs;
                }
            }

            internal static string AppImages
            {
                get
                {
                    if (_appImages == default(string))
                        _appImages = Path.Combine(HomeDir, "Assets", "AppImages.dat");
                    return _appImages;
                }
            }

            internal static string AppsLauncher
            {
                get
                {
                    if (_appsLauncher == default(string))
#if x86
                        _appsLauncher = Path.Combine(HomeDir, "AppsLauncher.exe");
#else
                        _appsLauncher = Path.Combine(HomeDir, "AppsLauncher64.exe");
#endif
                    return _appsLauncher;
                }
            }

            internal static string AppsSuiteUpdater
            {
                get
                {
                    if (_appsSuiteUpdater == default(string))
                        _appsSuiteUpdater = PathEx.Combine(PathEx.LocalDir, "Updater.exe");
                    return _appsSuiteUpdater;
                }
            }

            internal static string FileArchiver
            {
                get
                {
                    if (_archiver != default(string))
                        return _archiver;
#if x86
                    _archiver = PathEx.Combine(PathEx.LocalDir, "Helper", "7z", "7zG.exe");
#else
                    _archiver = PathEx.Combine(PathEx.LocalDir, "Helper", "7z", "x64", "7zG.exe");
#endif
                    Compaction.Zip7Helper.ExePath = _archiver;
                    return _archiver;
                }
            }

            internal static string HomeDir
            {
                get
                {
                    if (_homeDir == default(string))
                        _homeDir = PathEx.Combine(PathEx.LocalDir, "..");
                    return _homeDir;
                }
            }

            internal static string SystemExplorer
            {
                get
                {
                    if (_systemExplorer == default(string))
                        _systemExplorer = PathEx.Combine(Environment.SpecialFolder.Windows, "explorer.exe");
                    return _systemExplorer;
                }
            }

            internal static string SystemRestore
            {
                get
                {
                    if (_systemRestore == default(string))
                        _systemRestore = PathEx.Combine(Environment.SpecialFolder.System, "rstrui.exe");
                    return _systemRestore;
                }
            }

            internal static string TempDir
            {
                get
                {
                    if (_tempDir != default(string) && Directory.Exists(_tempDir))
                        return _tempDir;
                    _tempDir = Path.Combine(UserDirs.First(), ".cache");
                    if (Directory.Exists(_tempDir))
                        return _tempDir;
                    if (DirectoryEx.Create(_tempDir))
                    {
                        using (var p = ProcessEx.Start(AppsLauncher, ActionGuid.RepairDirs, false, false))
                            if (p?.HasExited == false)
                                p.WaitForExit();
                        return _tempDir;
                    }
                    _tempDir = EnvironmentEx.GetVariableValue("TEMP");
                    return _tempDir;
                }
            }

            internal static string[] UserDirs { get; } =
            {
                Path.Combine(HomeDir, "Documents"),
                Path.Combine(HomeDir, "Documents", "Documents"),
                Path.Combine(HomeDir, "Documents", "Music"),
                Path.Combine(HomeDir, "Documents", "Pictures"),
                Path.Combine(HomeDir, "Documents", "Videos")
            };
        }

        internal static class Window
        {
            private static FormWindowState? _state;

            internal static FormWindowState State
            {
                get
                {
                    if (!_state.HasValue)
                        _state = Ini.Read(Section, GetConfigKey(nameof(Window), nameof(State)), FormWindowState.Normal);
                    return (FormWindowState)_state;
                }
                set
                {
                    _state = value;
                    if (_state == FormWindowState.Minimized)
                        _state = FormWindowState.Normal;
                    WriteValue(Section, GetConfigKey(nameof(Window), nameof(State)), (FormWindowState)_state);
                }
            }

            internal static class Size
            {
                internal const int MinimumHeight = 125;
                internal const int MinimumWidth = 760;
                private static System.Drawing.Size _current, _default, _maximum, _minimum;
                private static int _width, _height;

                internal static System.Drawing.Size Current
                {
                    get
                    {
                        if (_current == default(System.Drawing.Size))
                            _current = new System.Drawing.Size(Width, Height);
                        return _current;
                    }
                }

                internal static System.Drawing.Size Default
                {
                    get
                    {
                        if (_default == default(System.Drawing.Size))
                            _default = new System.Drawing.Size(MinimumWidth, (int)Math.Round(MaximumHeight / 1.5d));
                        return _default;
                    }
                }

                internal static System.Drawing.Size Maximum
                {
                    get
                    {
                        if (_maximum != default(System.Drawing.Size))
                            return _maximum;
                        var curPos = WinApi.NativeHelper.GetCursorPos();
                        var screen = Screen.PrimaryScreen;
                        foreach (var scr in Screen.AllScreens)
                        {
                            if (!scr.Bounds.Contains(curPos))
                                continue;
                            screen = scr;
                            break;
                        }
                        _maximum = screen.WorkingArea.Size;
                        return _maximum;
                    }
                }

                internal static System.Drawing.Size Minimum
                {
                    get
                    {
                        if (_minimum == default(System.Drawing.Size))
                            _minimum = new System.Drawing.Size(MinimumWidth, MinimumHeight);
                        return _minimum;
                    }
                }

                internal static int DefaultWidth =>
                    Default.Width;

                internal static int DefaultHeight =>
                    Default.Height;

                internal static int MaximumWidth =>
                    Maximum.Width;

                internal static int MaximumHeight =>
                    Maximum.Height;

                internal static int Width
                {
                    get
                    {
                        if (_width != default(int))
                            return _width;
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Width));
                        var value = Ini.Read(Section, key, DefaultWidth);
                        _width = ValidateValue(value, MinimumWidth, MaximumWidth);
                        return _width;
                    }
                    set
                    {
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Width));
                        _width = State != FormWindowState.Maximized ? ValidateValue(value, MinimumWidth, MaximumWidth) : DefaultWidth;
                        _current = default(System.Drawing.Size);
                        WriteValue(Section, key, _width, DefaultWidth);
                    }
                }

                internal static int Height
                {
                    get
                    {
                        if (_height != default(int))
                            return _height;
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Height));
                        var value = Ini.Read(Section, key, DefaultHeight);
                        _height = ValidateValue(value, MinimumHeight, MaximumHeight);
                        return _width;
                    }
                    set
                    {
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Height));
                        _height = State != FormWindowState.Maximized ? ValidateValue(value, MinimumHeight, MaximumHeight) : DefaultHeight;
                        _current = default(System.Drawing.Size);
                        WriteValue(Section, key, _height, DefaultHeight);
                    }
                }
            }
        }
    }
}
