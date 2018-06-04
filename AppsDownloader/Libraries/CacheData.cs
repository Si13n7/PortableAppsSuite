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
    using LangResources;
    using SilDev;

    internal static class CacheData
    {
        private static Dictionary<string, Image> _appImages;
        private static List<AppData> _appInfo;
        private static List<string> _settingsMerges;

        internal static Dictionary<string, Image> AppImages
        {
            get
            {
                if (_appImages != default(Dictionary<string, Image>))
                    return _appImages;
                UpdateAppImagesFile();
                _appImages = FileEx.Deserialize<Dictionary<string, Image>>(CachePaths.AppImages, CorePaths.AppImages);
                if (_appImages == default(Dictionary<string, Image>))
                    _appImages = new Dictionary<string, Image>();
                return _appImages;
            }
        }

        internal static List<AppData> AppInfo
        {
            get
            {
                if (_appInfo != default(List<AppData>))
                    return _appInfo;
                UpdateAppInfoFile();
                return _appInfo ?? (_appInfo = new List<AppData>());
            }
        }

        internal static List<string> SettingsMerges
        {
            get
            {
                if (_settingsMerges != default(List<string>))
                    return _settingsMerges;
                _settingsMerges = FileEx.Deserialize<List<string>>(CachePaths.SettingsMerges);
                if (_settingsMerges == default(List<string>))
                    _settingsMerges = new List<string>();
                return _settingsMerges;
            }
        }

        private static void UpdateAppImagesFile()
        {
            var fileDate = File.Exists(CachePaths.AppImages) ? File.GetLastWriteTime(CachePaths.AppImages) : DateTime.MinValue;
            foreach (var mirror in AppSupply.GetMirrors(AppSupply.Suppliers.Internal))
            {
                var link = PathEx.AltCombine(mirror, "Downloads", "Portable%20Apps%20Suite", ".free", "AppImages.dat");
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: Looking for '{link}'.");
                if (!NetEx.FileIsAvailable(link, 30000))
                    continue;
                if (!((NetEx.GetFileDate(link) - fileDate).TotalSeconds > 0d))
                    return;
                NetEx.Transfer.DownloadFile(link, CachePaths.AppImages, 60000, null, false);
                if (!File.Exists(CachePaths.AppImages))
                    continue;
                File.SetLastWriteTime(CachePaths.AppImages, DateTime.Now);
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: '{CachePaths.AppImages}' has been updated.");
                return;
            }
            var repoLink = PathEx.AltCombine(CorePaths.RepositoryUrl, "AppImages.ini");
            if (Log.DebugMode > 0)
                Log.Write($"Cache: Looking for '{repoLink}'.");
            if (!NetEx.FileIsAvailable(repoLink, 60000) || !((NetEx.GetFileDate(repoLink) - fileDate).TotalSeconds > 0d))
                return;
            NetEx.Transfer.DownloadFile(repoLink, CachePaths.AppImages, 60000, null, false);
            if (!File.Exists(CachePaths.AppImages))
                return;
            File.SetLastWriteTime(CachePaths.AppImages, DateTime.Now);
            if (Log.DebugMode > 0)
                Log.Write($"Cache: '{CachePaths.AppImages}' has been updated.");
        }

        private static void ResetAppInfoFile()
        {
            if (ActionGuid.IsUpdateInstance || !File.Exists(CachePaths.AppInfo))
                goto Reset;

            try
            {
                var appInfo = FileEx.Deserialize<List<AppData>>(CachePaths.AppInfo);
                if (appInfo == default(List<AppData>))
                    throw new ArgumentNullException(nameof(appInfo));
                if (appInfo.Count < 430)
                    throw new ArgumentOutOfRangeException(nameof(appInfo));

                var fileInfo = new FileInfo(CachePaths.AppInfo);
                if ((DateTime.Now - fileInfo.LastWriteTime).TotalHours >= 1d)
                    goto Reset;

                _appInfo = appInfo;
                return;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            Reset:
            _appInfo = new List<AppData>();
            FileEx.TryDelete(CachePaths.AppInfo);
        }

        private static void UpdateAppInfoFile()
        {
            ResetAppInfoFile();
            if (_appInfo?.Count > 430)
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
                var link = PathEx.AltCombine(CorePaths.RepositoryUrl, "AppInfo.ini");
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: Looking for '{link}'.");
                if (NetEx.FileIsAvailable(link, 60000))
                    NetEx.Transfer.DownloadFile(link, CachePaths.AppInfo, 60000, null, false);
            }
            if (File.Exists(CachePaths.AppInfo))
            {
                blacklist = Ini.GetSections(CachePaths.AppInfo).Where(x => Ini.Read(x, "Disabled", false, CachePaths.AppInfo)).ToArray();
                UpdateAppInfoData(CachePaths.AppInfo, blacklist);
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
                var link = PathEx.AltCombine(AppSupply.SupplierHosts.PortableApps, "updater", "update.7z");
                if (Log.DebugMode > 0)
                    Log.Write($"Cache: Looking for '{link}'.");
                if (NetEx.FileIsAvailable(link, 60000))
                    NetEx.Transfer.DownloadFile(link, tmpZip, 60000, null, false);
            }
            if (File.Exists(tmpZip))
            {
                using (var process = Compaction.Zip7Helper.Unzip(tmpZip, tmpDir))
                    if (process?.HasExited == false)
                        process.WaitForExit();
                FileEx.TryDelete(tmpZip);
            }
            var tmpIni = DirectoryEx.GetFiles(tmpDir, "*.ini").FirstOrDefault();
            if (!File.Exists(tmpIni))
            {
                DirectoryEx.TryDelete(tmpDir);
                return;
            }
            UpdateAppInfoData(tmpIni, blacklist);

            FileEx.Serialize(CachePaths.AppInfo, AppInfo);
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
                UpdateAppInfoData(appInfo, null, key);
            }
        }

        private static void UpdateAppInfoData(string config, string[] blacklist = null, byte[] serverKey = null)
        {
            var sectionContainsFilter = new[]
            {
                AppSupply.SupplierHosts.PortableApps,
                $"By{nameof(AppSupply.Suppliers.PortableApps)}"
            };
            foreach (var section in Ini.GetSections(config))
            {
                if (blacklist?.ContainsEx(section) == true || section.ContainsEx(sectionContainsFilter))
                    continue;

                #region Name

                var name = Ini.Read(section, "Name", config);
                if (string.IsNullOrWhiteSpace(name) || name.ContainsEx("jPortable Launcher"))
                    continue;
                if (!name.StartsWithEx("jPortable", AppSupply.SupplierHosts.PortableApps))
                {
                    var newName = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(name, string.Empty);
                    if (!string.IsNullOrWhiteSpace(newName))
                        newName = Regex.Replace(newName, @"\s+", " ").Trim().TrimEnd(',');
                    if (!string.IsNullOrWhiteSpace(newName) && !newName.Equals(name))
                        name = newName;
                }

                #endregion

                #region Description

                var description = Ini.Read(section, "Description", config);
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

                #endregion

                #region Category

                var category = serverKey != null ? "*Shareware" : Ini.Read(section, "Category", config);
                if (string.IsNullOrWhiteSpace(category) || AppInfo.Any(x => x.Key.EqualsEx(section) && x.Category.EqualsEx(category)))
                    continue;

                #endregion

                #region Website

                var website = Ini.Read(section, "Website", config);
                if (string.IsNullOrWhiteSpace(website))
                    website = Ini.Read(section, "URL", config).ToLower().Replace("https", "http");
                if (string.IsNullOrWhiteSpace(website) || website.Any(char.IsUpper))
                    website = default(string);

                #endregion

                #region Version

                var displayVersion = Ini.Read(section, "Version", config);
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

                        var verData = Ini.Read(section, "VersionData", config);
                        if (!string.IsNullOrWhiteSpace(verData))
                        {
                            var verHash = Ini.Read(section, "VersionHash", config);
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
                if (string.IsNullOrWhiteSpace(displayVersion) || packageVersion == default(Version))
                {
                    displayVersion = Ini.Read(section, "DisplayVersion", config);
                    packageVersion = Ini.Read(section, "PackageVersion", default(Version), config);
                }

                #endregion

                #region Paths

                var path1 = Ini.Read(section, "ArchivePath", config);
                var path2 = default(string);
                string hash;
                if (!string.IsNullOrWhiteSpace(path1))
                {
                    if (path1.StartsWithEx(".free", ".repack"))
                        path1 = PathEx.AltCombine(AppSupply.GetMirrors(AppSupply.Suppliers.Internal).First(), "Downloads", "Portable%20Apps%20Suite", path1);
                    hash = Ini.Read(section, "ArchiveHash", config);
                }
                else
                {
                    var path = Ini.Read(section, "DownloadPath", config);
                    var file = Ini.Read(section, "DownloadFile", config);
                    path1 = PathEx.AltCombine(default(char[]), GetAbsoluteUrl(path, section), file);
                    path2 = PathEx.AltCombine(default(char[]), path, file);
                    if (!path1.EndsWithEx(".paf.exe"))
                        continue;
                    hash = Ini.Read(section, "Hash", config);
                }
                if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(hash))
                    continue;

                var defaultLanguage = path1.ContainsEx("Multilingual") ? "Multilingual" : path1.ContainsEx("English") ? "English" : "Default";
                var downloadCollection = new Dictionary<string, List<Tuple<string, string>>>
                {
                    {
                        defaultLanguage,
                        new List<Tuple<string, string>>
                        {
                            Tuple.Create(path1, hash)
                        }
                    }
                };
                if (path2.StartsWithEx("http") && !path2.EqualsEx(path1))
                    downloadCollection[defaultLanguage].Add(Tuple.Create(path2, hash));

                foreach (var lang in Language.GetText(nameof(en_US.availableLangs)).Split(',').Where(Comparison.IsNotEmpty))
                {
                    var langFile = Ini.Read(section, $"DownloadFile_{lang}", config);
                    if (!langFile.EndsWithEx(".paf.exe"))
                        continue;

                    var langPath = Ini.Read(section, "DownloadPath", config);
                    var langPath1 = PathEx.AltCombine(default(char[]), GetAbsoluteUrl(langPath, section), langFile);
                    if (!langPath1.EndsWithEx(".paf.exe"))
                        continue;

                    var langHash = Ini.Read(section, $"Hash_{lang}", config);
                    if (string.IsNullOrWhiteSpace(langHash) || langHash.EqualsEx(hash))
                        continue;

                    downloadCollection.Add(lang, new List<Tuple<string, string>>
                    {
                        Tuple.Create(langPath1, langHash)
                    });
                    var langPath2 = PathEx.AltCombine(default(char[]), langPath, langFile);
                    if (langPath2.StartsWithEx("http") && !langPath2.EqualsEx(langPath1))
                        downloadCollection[lang].Add(Tuple.Create(langPath2, langHash));
                }

                #endregion

                #region Sizes

                var downloadSize = Ini.Read(section, "DownloadSize", 1L, config) * 1024 * 1024;
                if (downloadSize < 0x100000)
                    downloadSize = 0x100000;

                var installSize = Ini.Read(section, "InstallSizeTo", 0L, config);
                if (installSize == 0)
                    installSize = Ini.Read(section, "InstallSize", 1L, config);
                installSize = installSize * 1024 * 1024;
                if (installSize < 0x100000)
                    installSize = 0x100000;

                #endregion

                #region Misc

                var requires = Ini.Read(section, "Requires", default(string), config);
                var requirements = new List<string>();
                if (!string.IsNullOrEmpty(requires))
                {
                    if (!requires.Contains(","))
                        requires += ",";
                    foreach (var str in requires.Split(','))
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

                var advanced = Ini.Read(section, "Advanced", false, config);
                if (!advanced && (displayVersion.EqualsEx("Discontinued") || displayVersion.ContainsEx("Nightly", "Alpha", "Beta")))
                    advanced = true;

                #endregion

                var appData = new AppData(section, name, description, category, website, displayVersion, packageVersion, versionData, defaultLanguage, downloadCollection.Keys.ToList(), downloadCollection, downloadSize, installSize, requirements, advanced, serverKey);
                if (appData == default(AppData))
                    continue;
                AppInfo.Add(appData);

                if (Log.DebugMode < 2)
                    continue;
                var sb = new StringBuilder();
                sb.AppendLine("AppInfo has been added.");
                appData.ToString(sb);
                Log.Write(sb.ToString());
            }

            Ini.Detach(config);
        }

        private static string GetAbsoluteUrl(string url, string key = null)
        {
            var realUrl = url;
            var redirect = realUrl.ContainsEx("/redirect/");
            if (string.IsNullOrWhiteSpace(realUrl) || redirect && realUrl.ContainsEx("&d=sfpa"))
                realUrl = PathEx.AltCombine(default(char[]), "http:", $"downloads.{AppSupply.SupplierHosts.SourceForge}", "portableapps");
            else if (redirect && realUrl.ContainsEx("&d=pa&f="))
                realUrl = PathEx.AltCombine(default(char[]), "http:", $"downloads.{AppSupply.SupplierHosts.PortableApps}", "portableapps", key);
            if (!url.EqualsEx(realUrl))
                return realUrl;
            if (redirect)
            {
                var filter = WebUtility.UrlDecode(realUrl)?.RemoveChar(':').Replace("https", "http").Split("http/")?.Last()?.RemoveText("/&d=pb&f=").Trim('/');
                if (!string.IsNullOrEmpty(filter))
                    realUrl = PathEx.AltCombine(default(char[]), "http:", filter);
            }
            if (!realUrl.ContainsEx(AppSupply.SupplierHosts.PortableApps))
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
            FileEx.Serialize(CachePaths.SettingsMerges, SettingsMerges);
        }
    }
}
