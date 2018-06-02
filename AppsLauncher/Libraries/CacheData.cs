namespace AppsLauncher.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using SilDev;

    internal static class CacheData
    {
        private static Dictionary<string, Image> _appImages, _currentImages;
        private static List<AppData> _currentAppInfo;
        private static List<string> _currentAppSections;
        private static int _currentImagesCount;
        private static Dictionary<int, int> _currentTypeData;
        private static List<string> _settingsMerges;
        private static readonly List<Tuple<ResourcesEx.IconIndex, bool, Icon>> Icons = new List<Tuple<ResourcesEx.IconIndex, bool, Icon>>();
        private static readonly List<Tuple<ResourcesEx.IconIndex, bool, Image>> Images = new List<Tuple<ResourcesEx.IconIndex, bool, Image>>();

        internal static Dictionary<string, Image> AppImages
        {
            get
            {
                if (_appImages != default(Dictionary<string, Image>))
                    return _appImages;
                _appImages = FileEx.ReadAllBytes(CachePaths.AppImages)?.DeserializeObject<Dictionary<string, Image>>();
                if (_appImages?.Any() != true)
                    _appImages = FileEx.ReadAllBytes(CorePaths.AppImages)?.DeserializeObject<Dictionary<string, Image>>();
                if (_appImages == default(Dictionary<string, Image>))
                    _appImages = new Dictionary<string, Image>();
                return _appImages;
            }
        }

        internal static List<AppData> CurrentAppInfo
        {
            get
            {
                if (_currentAppInfo != default(List<AppData>))
                    return _currentAppInfo;
                UpdateAppInfo();
                return _currentAppInfo;
            }
            set => _currentAppInfo = value;
        }

        internal static List<string> CurrentAppSections
        {
            get
            {
                if (_currentAppSections != default(List<string>))
                    return _currentAppSections;
                _currentAppSections = Ini.GetSections().Where(x => CurrentAppInfo.Any(y => y.Key.EqualsEx(x))).ToList();
                return _currentAppSections;
            }
            set => _currentAppSections = value;
        }

        internal static Dictionary<string, Image> CurrentImages
        {
            get
            {
                if (_currentImages == default(Dictionary<string, Image>))
                    _currentImages = new Dictionary<string, Image>();
                if (_currentImages.Any() || !File.Exists(CachePaths.CurrentImages))
                    return _currentImages;
                _currentImages = FileEx.ReadAllBytes(CachePaths.CurrentImages)?.DeserializeObject<Dictionary<string, Image>>();
                _currentImagesCount = _currentImages?.Count ?? 0;
                return _currentImages;
            }
        }

        internal static Dictionary<int, int> CurrentTypeData
        {
            get
            {
                if (_currentTypeData != default(Dictionary<int, int>))
                    return _currentTypeData;
                if (File.Exists(CachePaths.CurrentTypeData))
                    _currentTypeData = FileEx.ReadAllBytes(CachePaths.CurrentTypeData)?.Unzip()?.DeserializeObject<Dictionary<int, int>>();
                if (_currentTypeData == default(Dictionary<int, int>))
                    _currentTypeData = new Dictionary<int, int>();
                return _currentTypeData;
            }
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

        internal static AppData FindAppData(string appKeyOrName)
        {
            if (!CurrentAppInfo.Any() || string.IsNullOrWhiteSpace(appKeyOrName))
                return default(AppData);
            return CurrentAppInfo.FirstOrDefault(x => appKeyOrName.EqualsEx(x.Key, x.Name));
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
            icon = ResourcesEx.GetSystemIcon(index, large, Settings.IconResourcePath);
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
            if (Settings.CurrentDirectory.EqualsEx(PathEx.LocalDir))
                return;
            foreach (var type in new[] { "ini", "ixi" })
                foreach (var file in DirectoryEx.GetFiles(CorePaths.TempDir, $"*.{type}"))
                    FileEx.Delete(file);
            Settings.CurrentDirectory = PathEx.LocalDir;
        }

        internal static void UpdateCurrentImagesFile()
        {
            if (!CurrentImages.Any() || _currentImagesCount == _currentImages.Count && File.Exists(CachePaths.CurrentImages))
                return;
            File.WriteAllBytes(CachePaths.CurrentImages, CurrentImages.SerializeObject());
            _currentImagesCount = _currentImages.Count;
        }

        private static void UpdateAppInfo()
        {
            _currentAppInfo = new List<AppData>();
            var cached = FileEx.ReadAllBytes(CachePaths.CurrentAppInfo)?.DeserializeObject<List<AppData>>();
            var regex = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase);
            foreach (var dir in Settings.AppDirs.SelectMany(x => DirectoryEx.EnumerateDirectories(x).Where(y => y.ContainsEx("Portable"))))
            {
                var key = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(key))
                    continue;

                var current = cached?.FirstOrDefault(x => x.Key.EqualsEx(key));
                if (current != default(AppData) && File.Exists(current.FilePath))
                {
                    _currentAppInfo.Add(current);
                    continue;
                }

                // try to get the file path
                var filePath = Path.Combine(dir, $"{key}.exe");
                var configPath = Path.Combine(dir, $"{key}.ini");
                var appInfoPath = Path.Combine(dir, "App", "AppInfo", "appinfo.ini");
                if (File.Exists(configPath) || File.Exists(appInfoPath))
                {
                    var fileName = Ini.Read("AppInfo", "File", configPath);
                    if (string.IsNullOrEmpty(fileName))
                        fileName = Ini.Read("Control", "Start", appInfoPath);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var dirPath = Ini.Read("AppInfo", "Dir", configPath);
                        if (string.IsNullOrEmpty(dirPath))
                        {
                            filePath = Path.Combine(dir, fileName);
                            if (!File.Exists(configPath))
                                configPath = filePath.Replace(".exe", ".ini");
                        }
                        else
                        {
                            var curDirEnvVars = new[]
                            {
                                "%CurrentDir%",
                                "%CurDir%"
                            };
                            foreach (var vars in curDirEnvVars)
                            {
                                if (!dirPath.StartsWithEx(vars))
                                    continue;
                                var curDir = Path.GetDirectoryName(configPath);
                                if (string.IsNullOrEmpty(curDir))
                                    continue;
                                var subDir = dirPath.Substring(vars.Length).Trim('\\');
                                dirPath = Path.Combine(curDir, subDir);
                            }
                            filePath = PathEx.Combine(dirPath, fileName);
                        }
                    }
                }
                if (!File.Exists(filePath))
                    filePath = Path.Combine(dir, $"{key}.exe");
                if (!File.Exists(filePath))
                    continue;

                // try to get the full app name
                var name = Ini.Read("AppInfo", "Name", configPath);
                if (string.IsNullOrWhiteSpace(name))
                    name = Ini.Read("Details", "Name", appInfoPath);
                if (string.IsNullOrWhiteSpace(name))
                    name = FileVersionInfo.GetVersionInfo(filePath).FileDescription;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                // apply some filters to the found app name
                var newName = regex.Replace(name, string.Empty);
                if (!Regex.Replace(newName, @"\s+", " ").EqualsEx(name))
                    name = newName;
                name = name.Trim().TrimEnd(',').TrimEnd();
                while (name.Contains("  "))
                    name = name.Replace("  ", " ");

                if (string.IsNullOrWhiteSpace(name) || !File.Exists(filePath) || _currentAppInfo?.Any(x => x.Name.EqualsEx(name)) == true)
                    continue;
                _currentAppInfo?.Add(new AppData(key, name, dir, filePath, configPath, appInfoPath));
            }

            if (_currentAppInfo?.Any() != true)
            {
                ProcessEx.Start(CorePaths.AppsDownloader);
                Environment.ExitCode = 0;
                Environment.Exit(Environment.ExitCode);
            }
            _currentAppInfo = _currentAppInfo?.OrderBy(x => x.Name, new Comparison.AlphanumericComparer()).ToList();
            if (_currentAppInfo.Count != cached?.Count)
                FileEx.WriteAllBytes(CachePaths.CurrentAppInfo, _currentAppInfo.SerializeObject());
        }
    }
}
