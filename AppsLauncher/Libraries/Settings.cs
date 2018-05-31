namespace AppsLauncher.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Windows;
    using SilDev;
    using SilDev.Drawing;

    internal static class Settings
    {
        internal const string EnvironmentVariable = "AppsSuiteDir";
        internal const string Section = "Launcher";
        private static string[] _appDirs;
        private static string _currentDirectory, _iconResourcePath, _language, _lastItem, _registryPath;
        private static bool? _startMenuIntegration;
        private static int? _updateChannel, _updateCheck;

        internal static string[] AppDirs
        {
            get
            {
                if (_appDirs != default(string[]))
                    return _appDirs;
                var value = Ini.Read<string>(Section, nameof(AppDirs)).DecodeStringFromBase64();
                var dirs = value.SplitNewLine();
                _appDirs = CorePaths.AppDirs;
                if (dirs.Any())
                    _appDirs = _appDirs.Concat(dirs.Select(PathEx.Combine)).Where(Directory.Exists).ToArray();
                return _appDirs;
            }
            set
            {
                _appDirs = CorePaths.AppDirs;
                var dirs = value;
                if (dirs.Any())
                    _appDirs = _appDirs.Concat(dirs.Select(PathEx.Combine)).Where(Directory.Exists).ToArray();
                var encoded = dirs.Join(Environment.NewLine).EncodeToBase64();
                WriteValue(Section, nameof(AppDirs), encoded);
            }
        }

        internal static string CurrentDirectory
        {
            get
            {
                if (_currentDirectory == default(string))
                    _currentDirectory = Ini.Read<string>(Section, nameof(CurrentDirectory));
                return _currentDirectory;
            }
            private set
            {
                _currentDirectory = value;
                WriteValueDirect(Section, nameof(CurrentDirectory), _currentDirectory);
            }
        }

        internal static bool DeveloperVersion =>
            Ini.Read(Section, nameof(DeveloperVersion), false);

        internal static string IconResourcePath
        {
            get
            {
                if (_iconResourcePath != default(string))
                    return _iconResourcePath;
                _iconResourcePath = Ini.Read<string>(Section, nameof(IconResourcePath), "%system%");
                return _iconResourcePath;
            }
            set
            {
                _iconResourcePath = value;
                WriteValue(Section, nameof(IconResourcePath), _iconResourcePath, "%system%");
            }
        }

        internal static string Language
        {
            get
            {
                if (_language == default(string))
                    _language = Ini.Read<string>(Section, nameof(Language), global::Language.SystemLang);
                return _language;
            }
            set
            {
                _language = value;
                WriteValue(Section, nameof(Language), _language, global::Language.SystemLang);
            }
        }

        internal static string LastItem
        {
            get
            {
                if (_lastItem == default(string))
                    _lastItem = Ini.Read<string>(Section, nameof(LastItem));
                return _lastItem;
            }
            set
            {
                _lastItem = value;
                WriteValueDirect(Section, nameof(LastItem), _lastItem);
            }
        }

        internal static string RegistryPath
        {
            get
            {
                if (_registryPath == default(string))
                    _registryPath = Path.Combine("HKCU", "Software", "Portable Apps Suite", PathEx.LocalPath.GetHashCode().ToString());
                return _registryPath;
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

        internal static bool StartMenuIntegration
        {
            get
            {
                if (_startMenuIntegration.HasValue)
                    return (bool)_startMenuIntegration;
                _startMenuIntegration = Ini.Read(Section, nameof(StartMenuIntegration), false);
                return (bool)_startMenuIntegration;
            }
            set
            {
                _startMenuIntegration = value;
                WriteValue(Section, nameof(StartMenuIntegration), _startMenuIntegration, false);
            }
        }

        internal static DateTime LastUpdateCheck
        {
            get => Ini.Read<DateTime>(Section, nameof(LastUpdateCheck));
            set => WriteValueDirect(Section, nameof(LastUpdateCheck), value);
        }

        internal static UpdateChannelOptions UpdateChannel
        {
            get
            {
                if (_updateChannel.HasValue)
                    return (UpdateChannelOptions)_updateChannel;
                _updateChannel = Ini.Read(Section, nameof(UpdateChannel), (int)UpdateChannelOptions.Release);
                return (UpdateChannelOptions)_updateChannel;
            }
            set
            {
                _updateChannel = (int)value;
                WriteValue(Section, nameof(UpdateChannel), (int)_updateChannel);
            }
        }

        internal static UpdateCheckOptions UpdateCheck
        {
            get
            {
                if (_updateCheck.HasValue)
                    return (UpdateCheckOptions)_updateCheck;
                _updateCheck = Ini.Read(Section, nameof(UpdateCheck), (int)UpdateCheckOptions.DailyFull);
                return (UpdateCheckOptions)_updateCheck;
            }
            set
            {
                _updateCheck = (int)value;
                WriteValue(Section, nameof(UpdateCheck), (int)_updateCheck, (int)UpdateCheckOptions.DailyFull);
            }
        }

        internal static bool SkipUpdateSearch { get; set; } = false;
        internal static bool WriteToFileInQueue { get; private set; }

        internal static void StartUpdateSearch()
        {
            if (SkipUpdateSearch)
                return;
            var i = (int)UpdateCheck;
            if (!i.IsBetween(1, 9))
                return;
            var lastCheck = LastUpdateCheck;
            if (lastCheck != default(DateTime) &&
                (i.IsBetween(1, 3) && (DateTime.Now - lastCheck).TotalHours < 1d ||
                 i.IsBetween(4, 6) && (DateTime.Now - lastCheck).TotalDays < 1d ||
                 i.IsBetween(7, 9) && (DateTime.Now - lastCheck).TotalDays < 30d))
                return;
            if (i != 2 && i != 5 && i != 8)
                ProcessEx.Start(CorePaths.AppsSuiteUpdater);
            if (i != 3 && i != 6 && i != 9)
                ProcessEx.Start(CorePaths.AppsDownloader, ActionGuid.UpdateInstance);
            LastUpdateCheck = DateTime.Now;
        }

        internal static void Initialize()
        {
            Log.FileDir = Path.Combine(CorePaths.TempDir, "Logs");

            Ini.SetFile(PathEx.LocalDir, "Settings.ini");
            Ini.SortBySections = new[]
            {
                "Downloader",
                Section
            };

            Log.AllowLogging(Ini.FilePath, "DebugMode", Ini.GetRegex(false));

#if x86
            if (Environment.Is64BitOperatingSystem)
            {
                var appsLauncher64 = Path.Combine(CorePaths.HomeDir, $"{ProcessEx.CurrentName}64.exe");
                if (File.Exists(appsLauncher64))
                {
                    ProcessEx.Start(appsLauncher64, EnvironmentEx.CommandLine(false));
                    Environment.ExitCode = 0;
                    Environment.Exit(Environment.ExitCode);
                }
            }
#endif

            CacheData.RemoveInvalidFiles();
            if (Recovery.AppsSuiteIsHealthy())
                return;
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        private static string GetConfigKey(params string[] keys)
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

        private static double ValidateValue(double value, double minValue, double maxValue)
        {
            var current = Math.Max(value, minValue);
            return Math.Min(current, maxValue);
        }

        private static void WriteValue<T>(string section, string key, T value, T defValue = default(T), bool direct = false)
        {
            if (File.Exists(CachePaths.SettingsMerges))
            {
                if (CacheData.SettingsMerges.Any())
                {
                    var path = Path.GetTempFileName();
                    if (FileEx.Copy(Ini.FilePath, path, true))
                    {
                        foreach (var curSection in CacheData.SettingsMerges)
                        {
                            Ini.RemoveSection(curSection);
                            foreach (var curKey in Ini.GetKeys(curSection, path))
                            {
                                var curValue = Ini.Read(curSection, curKey, path);
                                Ini.Write(curSection, curKey, curValue);
                            }
                        }
                        Ini.Detach(path);
                        FileEx.Delete(path);
                        WriteToFileInQueue = true;
                    }
                }
                FileEx.Delete(CachePaths.SettingsMerges);
            }
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
                if (direct)
                {
                    Ini.WriteDirect(section, key, null);
                    return;
                }
                WriteToFileInQueue = true;
                return;
            }
            Ini.Write(section, key, value);
            if (direct)
            {
                Ini.WriteDirect(section, key, value);
                return;
            }
            WriteToFileInQueue = true;
        }

        private static void WriteValueDirect<T>(string section, string key, T value, T defValue = default(T)) =>
            WriteValue(section, key, value, defValue, true);

        internal static void WriteToFile()
        {
            Ini.WriteAll();
            WriteToFileInQueue = false;
        }

        internal enum UpdateChannelOptions
        {
            Release,
            Beta
        }

        internal enum UpdateCheckOptions
        {
            Never,
            HourlyFull,
            HourlyOnlyApps,
            HourlyOnlyAppsSuite,
            DailyFull,
            DailyOnlyApps,
            DailyOnlyAppsSuite,
            MonthlyFull,
            MonthlyOnlyApps,
            MonthlyOnlyAppsSuite
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

            internal static bool IsAllowNewInstance =>
                IsActive(AllowNewInstance);

            internal static bool IsDisallowInterface =>
                IsActive(DisallowInterface);

            internal static bool IsSystemIntegration =>
                IsActive(SystemIntegration);

            internal static bool IsFileTypeAssociation =>
                IsActive(FileTypeAssociation);

            internal static bool IsRestoreFileTypes =>
                IsActive(RestoreFileTypes);

            internal static bool IsRepairAppsSuite =>
                IsActive(RepairAppsSuite);

            internal static bool IsRepairDirs =>
                IsActive(RepairDirs);

            internal static bool IsRepairVariable =>
                IsActive(RepairVariable);

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

        internal static class Arguments
        {
            private static List<string> _fileTypes, _validPaths, _savedFileTypes;

            internal static List<string> FileTypes
            {
                get
                {
                    if (_fileTypes == default(List<string>))
                        _fileTypes = new List<string>();
                    if (_fileTypes.Any() || !ValidPaths.Any())
                        return _fileTypes;
                    try
                    {
                        var comparer = new Comparison.AlphanumericComparer();
                        var types = ValidPaths.Where(x => !PathEx.IsDir(x)).Select(x => Path.GetExtension(x)?.ToLower().TrimStart('.'))
                                              .Where(Comparison.IsNotEmpty).Distinct().OrderBy(x => x, comparer);
                        _fileTypes = types.ToList();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    return _fileTypes;
                }
            }

            internal static string FoundApp { get; set; }
            internal static bool MultipleAppsFound { get; private set; }

            internal static List<string> SavedFileTypes
            {
                get
                {
                    if (_savedFileTypes == null)
                        _savedFileTypes = new List<string>();
                    if (_savedFileTypes.Count > 0)
                        return _savedFileTypes;
                    try
                    {
                        var comparer = new Comparison.AlphanumericComparer();
                        var types = ApplicationHandler.AllConfigSections.Aggregate(string.Empty, (x, y) => x + $"{Ini.Read(y, "FileTypes").RemoveChar('.')},").ToLower();
                        _savedFileTypes = types.Split(',').Where(Comparison.IsNotEmpty).Distinct().OrderBy(x => x, comparer).ToList();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    return _savedFileTypes;
                }
            }

            internal static List<string> ValidPaths
            {
                get
                {
                    if (_validPaths == default(List<string>))
                        _validPaths = new List<string>();
                    if (_validPaths.Any() || Environment.GetCommandLineArgs().Length < 2)
                        return _validPaths;
                    try
                    {
                        var comparer = new Comparison.AlphanumericComparer();
                        var args = Environment.GetCommandLineArgs().Skip(1).Where(PathEx.IsValidPath).OrderBy(x => x, comparer);
                        _validPaths = args.ToList();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    return _validPaths;
                }
            }

            internal static string ValidPathsStr
            {
                get
                {
                    var str = string.Empty;
                    if (ValidPaths.Any())
                        str = $"\"{ValidPaths.Join("\" \"")}\"";
                    return str;
                }
            }

            internal static void FindApp()
            {
                if (!ValidPaths.Any())
                    return;

                var cacheDir = Path.Combine(CorePaths.TempDir, "TypeData");
                var cacheId = ValidPathsStr?.EncryptToSha1();
                if (string.IsNullOrEmpty(cacheId))
                    return;

                var cachePath = Path.Combine(cacheDir, $"{cacheId.Substring(cacheId.Length - 8)}.ini");
                var appName = Ini.ReadDirect(cacheId, "AppName", cachePath);
                if (!string.IsNullOrEmpty(appName))
                {
                    FoundApp = appName;
                    return;
                }

                var allTypes = SavedFileTypes?.Select(x => '.' + x).ToArray();
                var typeInfo = new Dictionary<string, int>();
                var stopwatch = new Stopwatch();
                foreach (var path in ValidPaths)
                {
                    string ext;
                    try
                    {
                        if (PathEx.IsDir(path))
                        {
                            stopwatch.Start();

                            var dirInfo = new DirectoryInfo(path);
                            cacheId = dirInfo.FullName.EncryptToMd5();
                            if (cacheId.Length < 32)
                                continue;

                            cachePath = PathEx.Combine(cacheDir, $"{cacheId.Substring(cacheId.Length - 8)}.ini");
                            var dirHash = dirInfo.GetFullHashCode(false);
                            var keys = Ini.GetKeys(cacheId, cachePath);
                            if (keys.Count > 1 && Ini.ReadDirect(cacheId, "HashCode", cachePath).Equals(dirHash.ToString()))
                            {
                                foreach (var key in keys)
                                    typeInfo.Add(key, Ini.Read(cacheId, key, 0, cachePath));

                                Ini.Detach(cachePath);
                                continue;
                            }

                            FileEx.Delete(cachePath);
                            Ini.WriteDirect(cacheId, "HashCode", dirHash, cachePath);
                            foreach (var fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Take(1024))
                            {
                                if (fileInfo.MatchAttributes(FileAttributes.Hidden))
                                    continue;

                                ext = fileInfo.Extension;
                                if (typeInfo.ContainsKey(ext) || !ext.EndsWithEx(allTypes))
                                    continue;

                                var len = dirInfo.GetFiles('*' + ext, SearchOption.AllDirectories).Length;
                                if (len == 0)
                                    continue;

                                typeInfo.Add(ext, len);
                                Ini.WriteDirect(cacheId, ext, len, cachePath);
                                if (stopwatch.ElapsedMilliseconds >= 4096)
                                    break;
                            }
                            stopwatch.Reset();
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        continue;
                    }

                    if (!File.Exists(path) || FileEx.IsHidden(path) || !Path.HasExtension(path))
                        continue;

                    ext = Path.GetExtension(path);
                    if (string.IsNullOrEmpty(ext))
                        continue;

                    if (!typeInfo.ContainsKey(ext))
                        typeInfo.Add(ext, 1);
                    else
                        typeInfo[ext]++;
                }

                // Check app settings for the listed file types
                if (typeInfo?.Any() != true)
                    return;

                string typeApp = null;
                foreach (var type in typeInfo)
                    foreach (var section in ApplicationHandler.AllConfigSections)
                    {
                        var fileTypes = Ini.Read(section, "FileTypes");
                        if (string.IsNullOrWhiteSpace(fileTypes))
                            continue;
                        fileTypes = $"|.{fileTypes.RemoveChar('*', '.')?.Replace(",", "|.")}|"; // Enforce format

                        // If file type settings found for a app, select this app as default
                        if (fileTypes.ContainsEx($"|{type.Key}|"))
                        {
                            FoundApp = section;
                            if (string.IsNullOrWhiteSpace(typeApp))
                                typeApp = section;
                        }
                        if (MultipleAppsFound || string.IsNullOrWhiteSpace(FoundApp) || string.IsNullOrWhiteSpace(typeApp) || FoundApp.EqualsEx(typeApp))
                            continue;
                        MultipleAppsFound = true;
                        break;
                    }

                // If multiple file types with different app settings found, select the app with most listed file types
                if (!MultipleAppsFound)
                    return;
                var query = typeInfo?.OrderByDescending(x => x.Value);
                if (query == null)
                    return;
                var count = 0;
                string topType = null;
                foreach (var item in query)
                {
                    if (item.Value > count)
                        topType = item.Key;
                    count = item.Value;
                }
                if (string.IsNullOrEmpty(topType))
                    return;
                foreach (var section in ApplicationHandler.AllConfigSections)
                {
                    var fileTypes = Ini.Read(section, "FileTypes");
                    if (string.IsNullOrWhiteSpace(fileTypes))
                        continue;
                    fileTypes = $"|.{fileTypes.RemoveChar('*', '.').Replace(",", "|.")}|"; // Filter
                    if (!fileTypes.ContainsEx($"|{topType}|"))
                        continue;
                    FoundApp = section;
                    break;
                }
            }
        }

        internal static class CacheData
        {
            private static Dictionary<string, Image> _appImages, _currentImages;
            private static int _currentImagesCount;
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
                        _appImages = File.ReadAllBytes(CachePaths.AppImages).DeserializeObject<Dictionary<string, Image>>();
                    return _appImages;
                }
            }

            internal static Dictionary<string, Image> CurrentImages
            {
                get
                {
                    if (_currentImages == default(Dictionary<string, Image>))
                        _currentImages = new Dictionary<string, Image>();
                    if (_currentImages.Any() || !File.Exists(CachePaths.CurrentImages))
                        return _currentImages;
                    _currentImages = File.ReadAllBytes(CachePaths.CurrentImages).DeserializeObject<Dictionary<string, Image>>();
                    _currentImagesCount = _currentImages.Count;
                    return _currentImages;
                }
            }

            internal static List<string> SettingsMerges
            {
                get
                {
                    if (_settingsMerges != default(List<string>))
                        return _settingsMerges;
                    if (File.Exists(CachePaths.SettingsMerges))
                        _settingsMerges = File.ReadAllBytes(CachePaths.SettingsMerges).DeserializeObject<List<string>>();
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
                var tuple = new Tuple<ResourcesEx.IconIndex, bool, Icon>(index, large, icon);
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
                var tuple = new Tuple<ResourcesEx.IconIndex, bool, Image>(index, large, image);
                Images.Add(tuple);
                Return:
                return image;
            }

            internal static void RemoveInvalidFiles()
            {
                if (CurrentDirectory.EqualsEx(PathEx.LocalDir))
                    return;
                foreach (var type in new[] { "ini", "ixi" })
                    foreach (var file in DirectoryEx.GetFiles(CorePaths.TempDir, $"*.{type}"))
                        FileEx.Delete(file);
                CurrentDirectory = PathEx.LocalDir;
            }

            internal static void UpdateCurrentImagesFile()
            {
                if (!CurrentImages.Any() || _currentImagesCount == _currentImages.Count && File.Exists(CachePaths.CurrentImages))
                    return;
                File.WriteAllBytes(CachePaths.CurrentImages, CurrentImages.SerializeObject());
                _currentImagesCount = _currentImages.Count;
            }
        }

        internal static class CachePaths
        {
            private static string _appImages, _currentImages, _imageBg, _settingsMerges;

            internal static string AppImages
            {
                get
                {
                    if (_appImages == default(string))
                        _appImages = Path.Combine(CorePaths.TempDir, "AppImages.dat");
                    if (!File.Exists(_appImages))
                        _appImages = CorePaths.AppImages;
                    return _appImages;
                }
            }

            internal static string CurrentImages
            {
                get
                {
                    if (_currentImages == default(string))
                        _currentImages = Path.Combine(CorePaths.TempDir, "CurrentImages.dat");
                    return _currentImages;
                }
            }

            internal static string ImageBg
            {
                get
                {
                    if (_imageBg == default(string))
                        _imageBg = Path.Combine(CorePaths.TempDir, "ImageBg.dat");
                    return _imageBg;
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
        }

        internal static class CorePaths
        {
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")] private static string[] _appDirs;
            private static string _appsDir, _appImages, _appsDownloader, _appsSuiteUpdater, _archiver, _homeDir, _systemExplorer, _systemRestore, _tempDir;

            internal static string AppsDir
            {
                get
                {
                    if (_appsDir == default(string))
                        _appsDir = Path.Combine(HomeDir, "Apps");
                    return _appsDir;
                }
            }

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
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

            internal static string AppsDownloader
            {
                get
                {
                    if (_appsDownloader == default(string))
#if x86
                        _appsDownloader = Path.Combine(HomeDir, "Binaries", "AppsDownloader.exe");
#else
                        _appsDownloader = Path.Combine(HomeDir, "Binaries", "AppsDownloader64.exe");
#endif
                    return _appsDownloader;
                }
            }

            internal static string AppsSuiteUpdater
            {
                get
                {
                    if (_appsSuiteUpdater == default(string))
                        _appsSuiteUpdater = Path.Combine(HomeDir, "Binaries", "Updater.exe");
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
                    _archiver = Path.Combine(HomeDir, "Binaries", "Helper", "7z", "7zG.exe");
#else
                    _archiver = Path.Combine(HomeDir, "Binaries", "Helper", "7z", "x64", "7zG.exe");
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
                        _homeDir = PathEx.Combine(PathEx.LocalDir);
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
                        Recovery.RepairAppsSuiteDirs();
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
            private static WinApi.AnimateWindowFlags _animation;
            private static Image _backgroundImage;
            private static int? _backgroundImageLayout, _fadeInEffect;
            private static int[] _customColors;
            private static int _defaultPosition, _fadeInDuration;
            private static bool? _hideHScrollBar;
            private static double _opacity;

            internal static WinApi.AnimateWindowFlags Animation
            {
                get
                {
                    if (_animation != default(WinApi.AnimateWindowFlags))
                        return _animation;
                    if (FadeInEffect == FadeInEffectOptions.Blend)
                    {
                        _animation = WinApi.AnimateWindowFlags.Blend;
                        return _animation;
                    }
                    _animation = WinApi.AnimateWindowFlags.Slide;
                    if (DefaultPosition > 0)
                        _animation = WinApi.AnimateWindowFlags.Center;
                    else
                    {
                        var handle = Application.OpenForms.OfType<Form>().FirstOrDefault(x => x.Name?.EqualsEx(nameof(MenuViewForm)) == true)?.Handle ?? IntPtr.Zero;
                        switch (TaskBar.GetLocation(handle))
                        {
                            case TaskBar.Location.Left:
                                _animation |= WinApi.AnimateWindowFlags.HorPositive;
                                break;
                            case TaskBar.Location.Top:
                                _animation |= WinApi.AnimateWindowFlags.VerPositive;
                                break;
                            case TaskBar.Location.Right:
                                _animation |= WinApi.AnimateWindowFlags.HorNegative;
                                break;
                            case TaskBar.Location.Bottom:
                                _animation |= WinApi.AnimateWindowFlags.VerNegative;
                                break;
                            default:
                                _animation = WinApi.AnimateWindowFlags.Center;
                                break;
                        }
                    }
                    return _animation;
                }
            }

            internal static Image BackgroundImage
            {
                get
                {
                    if (_backgroundImage != default(Image))
                        return _backgroundImage;
                    if (!File.Exists(CachePaths.ImageBg))
                        return default(Image);
                    try
                    {
                        var bgBytes = File.ReadAllBytes(CachePaths.ImageBg);
                        var bgImg = bgBytes.DeserializeObject<Image>();
                        if (bgImg != default(Image))
                            _backgroundImage = bgImg;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        _backgroundImage = default(Image);
                    }
                    return _backgroundImage;
                }
                set => _backgroundImage = value;
            }

            internal static ImageLayout BackgroundImageLayout
            {
                get
                {
                    if (_backgroundImageLayout.HasValue)
                        return (ImageLayout)_backgroundImageLayout;
                    var key = GetConfigKey(nameof(Window), nameof(BackgroundImageLayout));
                    _backgroundImageLayout = Ini.Read(Section, key, 1);
                    return (ImageLayout)_backgroundImageLayout;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(BackgroundImageLayout));
                    _backgroundImageLayout = (int)value;
                    WriteValue(Section, key, _backgroundImageLayout, 1);
                }
            }

            internal static int[] CustomColors
            {
                get
                {
                    if (_customColors != default(int[]))
                        return _customColors;
                    var key = GetConfigKey(nameof(Window), nameof(CustomColors));
                    var value = FilterCostumColors(Ini.Read(Section, key, default(int[])));
                    _customColors = value;
                    return _customColors;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(CustomColors));
                    _customColors = FilterCostumColors(value);
                    var colors = _customColors?.Where(x => x != 0xffffff).ToArray();
                    if (colors?.Length == 0)
                        colors = default(int[]);
                    WriteValue(Section, key, colors);
                }
            }

            internal static int DefaultPosition
            {
                get
                {
                    if (_defaultPosition != default(int))
                        return _defaultPosition;
                    var key = GetConfigKey(nameof(Window), nameof(DefaultPosition));
                    var value = Ini.Read(Section, key, default(int));
                    _defaultPosition = ValidateValue(value, 0, 1);
                    return _defaultPosition;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(DefaultPosition));
                    _defaultPosition = ValidateValue(value, 0, 1);
                    WriteValue(Section, key, _defaultPosition);
                }
            }

            internal static int FadeInDuration
            {
                get
                {
                    if (_fadeInDuration != default(int))
                        return _fadeInDuration;
                    var key = GetConfigKey(nameof(Window), nameof(FadeInDuration));
                    var value = Ini.Read(Section, key, 100);
                    _fadeInDuration = ValidateValue(value, 25, 750);
                    return _fadeInDuration;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(FadeInDuration));
                    _fadeInDuration = ValidateValue(value, 25, 750);
                    WriteValue(Section, key, _fadeInDuration, 100);
                }
            }

            internal static FadeInEffectOptions FadeInEffect
            {
                get
                {
                    if (_fadeInEffect.HasValue)
                        return (FadeInEffectOptions)_fadeInEffect;
                    var key = GetConfigKey(nameof(Window), nameof(FadeInEffect));
                    var value = Ini.Read(Section, key, default(int));
                    _fadeInEffect = ValidateValue(value, 0, 1);
                    return (FadeInEffectOptions)_fadeInEffect;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(FadeInEffect));
                    _fadeInEffect = ValidateValue((int)value, 0, 1);
                    WriteValue(Section, key, _fadeInEffect);
                }
            }

            internal static bool HideHScrollBar
            {
                get
                {
                    if (_hideHScrollBar.HasValue)
                        return (bool)_hideHScrollBar;
                    var key = GetConfigKey(nameof(Window), nameof(HideHScrollBar));
                    _hideHScrollBar = Ini.Read(Section, key, true);
                    return (bool)_hideHScrollBar;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(HideHScrollBar));
                    _hideHScrollBar = value;
                    WriteValue(Section, key, _hideHScrollBar, true);
                }
            }

            internal static double Opacity
            {
                get
                {
                    if (_opacity > default(double))
                        return _opacity;
                    var key = GetConfigKey(nameof(Window), nameof(Opacity));
                    var value = Ini.Read(Section, key, .95d);
                    _opacity = ValidateValue(value, .2d, 1d);
                    return _opacity;
                }
                set
                {
                    var key = GetConfigKey(nameof(Window), nameof(Opacity));
                    _opacity = ValidateValue(value, .2d, 1d);
                    WriteValue(Section, key, _opacity, .95d);
                }
            }

            private static int[] FilterCostumColors(params int[] colors)
            {
                var list = (colors ?? new int[0]).ToList();
                var count = list.Count;
                if (count > 0)
                    list.Sort();
                while (list.Count < 16)
                    list.Add(0xffffff);
                if (count > 0)
                    list.Reverse();
                return list.ToArray();
            }

            internal enum FadeInEffectOptions
            {
                Blend,
                Slide
            }

            internal static class Colors
            {
                private const string DefWallpaperPath = "HKCU\\Software\\Microsoft\\Internet Explorer\\Desktop\\General";
                private const string WallpaperPath = "HKCU\\Control Panel\\Desktop";
                private static Color _system, _base, _baseText, _control, _controlText, _button, _buttonHover, _buttonText, _highlight, _highlightText;

                internal static Color System
                {
                    get
                    {
                        if (_system != default(Color))
                            return _system;
                        _system = WinApi.NativeHelper.GetSystemThemeColor();
                        if (_system != Color.Black && _system != Color.White)
                            return _system;
                        var path = Reg.Read(WallpaperPath, "WallPaper", default(string));
                        if (!File.Exists(path))
                            path = Reg.Read(DefWallpaperPath, "WallpaperSource", default(string));
                        try
                        {
                            var image = Image.FromFile(path);
                            _system = image.GetAverageColor(true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            _system = GetDefColor(nameof(Highlight));
                        }
                        return _system;
                    }
                }

                internal static Color Base
                {
                    get
                    {
                        if (_base == default(Color))
                            _base = GetColor(nameof(Base));
                        return _base;
                    }
                    set
                    {
                        _base = value;
                        WriteValue(nameof(Base), _base);
                    }
                }

                internal static Color BaseDark =>
                    ControlPaint.Dark(Base, .25f);

                internal static Color BaseLight =>
                    ControlPaint.Light(Base, .25f);

                internal static Color BaseText
                {
                    get
                    {
                        if (_baseText != default(Color))
                            return _baseText;
                        if (BackgroundImage != default(Image))
                        {
                            _baseText = BackgroundImage.GetAverageColor().InvertRgb().ToGrayScale();
                            return _baseText;
                        }
                        _baseText = BaseDark.InvertRgb().ToGrayScale();
                        return _baseText;
                    }
                }

                internal static Color Control
                {
                    get
                    {
                        if (_control == default(Color))
                            _control = GetColor(nameof(Control));
                        return _control;
                    }
                    set
                    {
                        _control = value;
                        WriteValue(nameof(Control), _control);
                    }
                }

                internal static Color ControlText
                {
                    get
                    {
                        if (_controlText == default(Color))
                            _controlText = GetColor(nameof(ControlText));
                        return _controlText;
                    }
                    set
                    {
                        _controlText = value;
                        WriteValue(nameof(ControlText), _controlText);
                    }
                }

                internal static Color Button
                {
                    get
                    {
                        if (_button == default(Color))
                            _button = GetColor(nameof(Button));
                        return _button;
                    }
                    set
                    {
                        _button = value;
                        WriteValue(nameof(Button), _button);
                    }
                }

                internal static Color ButtonHover
                {
                    get
                    {
                        if (_buttonHover == default(Color))
                            _buttonHover = GetColor(nameof(ButtonHover));
                        return _buttonHover;
                    }
                    set
                    {
                        _buttonHover = value;
                        WriteValue(nameof(ButtonHover), _buttonHover);
                    }
                }

                internal static Color ButtonText
                {
                    get
                    {
                        if (_buttonText == default(Color))
                            _buttonText = GetColor(nameof(ButtonText));
                        return _buttonText;
                    }
                    set
                    {
                        _buttonText = value;
                        WriteValue(nameof(ButtonText), _buttonText);
                    }
                }

                internal static Color Highlight
                {
                    get
                    {
                        if (_highlight == default(Color))
                            _highlight = GetColor(nameof(Highlight));
                        return _highlight;
                    }
                    set
                    {
                        _highlight = value;
                        WriteValue(nameof(Highlight), _highlight);
                    }
                }

                internal static Color HighlightText
                {
                    get
                    {
                        if (_highlightText == default(Color))
                            _highlightText = GetColor(nameof(HighlightText));
                        return _highlightText;
                    }
                    set
                    {
                        _highlightText = value;
                        WriteValue(nameof(HighlightText), _highlightText);
                    }
                }

                private static Color GetDefColor(string key)
                {
                    switch (key)
                    {
                        case nameof(Base):
                            return System;
                        case nameof(Control):
                            return SystemColors.Window;
                        case nameof(ControlText):
                            return SystemColors.WindowText;
                        case nameof(Button):
                            return SystemColors.ButtonFace;
                        case nameof(ButtonHover):
                            return ProfessionalColors.ButtonSelectedHighlight;
                        case nameof(ButtonText):
                            return SystemColors.ControlText;
                        case nameof(Highlight):
                            return SystemColors.Highlight;
                        case nameof(HighlightText):
                            return SystemColors.HighlightText;
                        default:
                            return Color.Empty;
                    }
                }

                private static Color GetColor(string key)
                {
                    var str = GetConfigKey(nameof(Window), nameof(Colors), key);
                    var html = Ini.Read(Section, str);
                    var color = html.FromHtmlToColor(GetDefColor(key), byte.MaxValue);
                    return color;
                }

                private static void WriteValue(string key, Color color)
                {
                    var str = GetConfigKey(nameof(Window), nameof(Colors), key);
                    if (color == GetDefColor(key))
                    {
                        WriteValue<string>(Section, str, null);
                        return;
                    }
                    var html = color.ToHtmlString();
                    WriteValue<string>(Section, str, html);
                }
            }

            internal static class Size
            {
                internal const int MinimumHeight = 320;
                internal const int MinimumWidth = 346;
                private static System.Drawing.Size _current, _maximum, _minimum;
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
                        var value = Ini.Read(Section, key, MinimumWidth);
                        _width = ValidateValue(value, MinimumWidth, MaximumWidth);
                        return _width;
                    }
                    set
                    {
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Width));
                        _width = ValidateValue(value, MinimumWidth, MaximumWidth);
                        _current = default(System.Drawing.Size);
                        WriteValue(Section, key, _width, MinimumWidth);
                    }
                }

                internal static int Height
                {
                    get
                    {
                        if (_height != default(int))
                            return _height;
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Height));
                        var value = Ini.Read(Section, key, MinimumHeight);
                        _height = ValidateValue(value, MinimumHeight, MaximumHeight);
                        return _width;
                    }
                    set
                    {
                        var key = GetConfigKey(nameof(Window), nameof(Size), nameof(Height));
                        _height = ValidateValue(value, MinimumHeight, MaximumHeight);
                        _current = default(System.Drawing.Size);
                        WriteValue(Section, key, _height, MinimumHeight);
                    }
                }
            }
        }
    }
}
