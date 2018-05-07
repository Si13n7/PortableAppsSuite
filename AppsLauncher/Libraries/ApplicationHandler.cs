namespace AppsLauncher.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using LangResources;
    using SilDev;
    using SilDev.Forms;

    internal static class ApplicationHandler
    {
        private static List<AppInfo> _allAppInfos;
        private static List<string> _allConfigSections;

        internal static List<AppInfo> AllAppInfos
        {
            get
            {
                if (_allAppInfos == default(List<AppInfo>))
                    _allAppInfos = new List<AppInfo>();
                return _allAppInfos;
            }
            set => _allAppInfos = value;
        }

        internal static List<string> AllConfigSections
        {
            get
            {
                if (_allConfigSections == default(List<string>))
                    _allConfigSections = new List<string>();
                if (_allConfigSections.Any())
                    return _allConfigSections;
                if (!AllAppInfos.Any())
                    SetAppsInfo();
                _allConfigSections = Ini.GetSections(false).Where(x => !x.EqualsEx("Downloader", Settings.Section)).ToList();
                return _allConfigSections;
            }
            set => _allConfigSections = value;
        }

        internal static AppInfo GetAppInfo(string appName)
        {
            if (!AllAppInfos.Any() || string.IsNullOrWhiteSpace(appName))
                return new AppInfo();
            foreach (var appInfo in AllAppInfos)
                if (appName.EqualsEx(appInfo.LongName, appInfo.ShortName))
                    return appInfo;
            return new AppInfo();
        }

        internal static void SetAppsInfo(bool force = true)
        {
            ReCheck:
            if (!force && AllAppInfos.Any())
                return;
            AllAppInfos.Clear();
            foreach (var dir in Settings.AppDirs)
            {
                if (!DirectoryEx.Create(dir))
                    continue;
                foreach (var path in DirectoryEx.GetDirectories(dir).Where(x => x.ContainsEx("Portable")))
                {
                    var dirName = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(dirName))
                        continue;

                    // If there is no exe file with the same name as the directory, search in config files for the correct start file
                    // This step is required for multiple exe files
                    var exePath = Path.Combine(dir, dirName, $"{dirName}.exe");
                    var iniPath = exePath.Replace(".exe", ".ini");
                    var nfoPath = Path.Combine(path, "App", "AppInfo", "appinfo.ini");
                    if (!File.Exists(exePath))
                    {
                        var appFile = Ini.Read("AppInfo", "File", iniPath);
                        if (string.IsNullOrWhiteSpace(appFile))
                            appFile = Ini.Read("Control", "Start", nfoPath);
                        if (string.IsNullOrWhiteSpace(appFile))
                            continue;
                        var appDir = Ini.Read("AppInfo", "Dir", iniPath);
                        if (string.IsNullOrWhiteSpace(appDir))
                            exePath = exePath.Replace($"{dirName}.exe", appFile);
                        else
                        {
                            var curDirEnvVars = new[]
                            {
                                "%CurrentDir%",
                                "%CurDir%"
                            };
                            foreach (var vars in curDirEnvVars)
                            {
                                if (!appDir.StartsWithEx(vars))
                                    continue;
                                var curDir = Path.GetDirectoryName(iniPath);
                                if (string.IsNullOrEmpty(curDir))
                                    continue;
                                var subDir = appDir.Substring(vars.Length).Trim('\\');
                                appDir = Path.Combine(curDir, subDir);
                            }
                            appDir = PathEx.Combine(appDir);
                            exePath = Path.Combine(appDir, appFile);
                        }
                    }
                    if (!File.Exists(exePath))
                        continue;
                    if (!File.Exists(iniPath))
                        iniPath = exePath.Replace(".exe", ".ini");

                    // Try to get the full app name
                    var appName = Ini.Read("AppInfo", "Name", iniPath);
                    if (string.IsNullOrWhiteSpace(appName))
                        appName = Ini.Read("Details", "Name", nfoPath);
                    if (string.IsNullOrWhiteSpace(appName))
                        appName = FileVersionInfo.GetVersionInfo(exePath).FileDescription;
                    if (string.IsNullOrWhiteSpace(appName))
                        continue;

                    // Apply some filters to the found app name
                    if (!appName.StartsWithEx("jPortable")) // No filters needed for portable JRE
                    {
                        var tmp = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(appName, string.Empty);
                        if (!Regex.Replace(tmp, @"\s+", " ").EqualsEx(appName))
                            appName = tmp;
                    }
                    appName = appName.Trim().TrimEnd(',').TrimEnd();
                    while (appName.Contains("  "))
                        appName = appName.Replace("  ", " ");

                    if (string.IsNullOrWhiteSpace(appName) || !File.Exists(exePath))
                        continue;
                    if (AllAppInfos.Count(x => x.LongName.EqualsEx(appName)) > 0)
                        continue;
                    AllAppInfos.Add(new AppInfo
                    {
                        LongName = appName,
                        ShortName = dirName,
                        ExePath = exePath,
                        IniPath = iniPath,
                        NfoPath = nfoPath
                    });
                }
            }
            if (!AllAppInfos.Any())
            {
                if (force)
                {
                    force = false;
                    goto ReCheck;
                }
                ProcessEx.Start(Settings.CorePaths.AppsDownloader);
                Environment.ExitCode = 0;
                Environment.Exit(Environment.ExitCode);
            }
            AllAppInfos = AllAppInfos.OrderBy(x => x.LongName, new Comparison.AlphanumericComparer()).ToList();
        }

        internal static string GetLocation(string appName)
        {
            var appInfo = GetAppInfo(appName);
            if (!appName.EqualsEx(appInfo.LongName, appInfo.ShortName))
                return null;
            var appDir = Path.GetDirectoryName(appInfo.ExePath);
            if (!Settings.AppDirs.Any(x => appDir.ContainsEx(x)))
                return appDir;
            var dirName = Path.GetFileName(appDir);
            while (!dirName.EqualsEx(appInfo.ShortName))
                try
                {
                    appDir = Path.GetFullPath($"{appDir}\\..");
                    if (Settings.AppDirs.ContainsEx(appDir) || appDir.Count(c => c == '\\') < 2)
                        throw new ArgumentOutOfRangeException(nameof(appDir));
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    appDir = Path.GetDirectoryName(appInfo.ExePath);
                    break;
                }
                finally
                {
                    dirName = Path.GetFileName(appDir);
                }
            return appDir;
        }

        internal static string GetPath(string appName)
        {
            var appInfo = GetAppInfo(appName);
            return appName.EqualsEx(appInfo.LongName, appInfo.ShortName) ? appInfo.ExePath : null;
        }

        internal static void OpenLocation(string appName, bool closeLancher = false)
        {
            try
            {
                var dir = GetLocation(appName);
                if (!Directory.Exists(dir))
                    throw new PathNotFoundException(dir);
                ProcessEx.Start(Settings.CorePaths.SystemExplorer, dir);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (closeLancher)
                Application.Exit();
        }

        internal static string SearchItem(string search, List<string> items)
        {
            try
            {
                string[] split = null;
                if (search.Contains("*") && !search.StartsWith("*") && !search.EndsWith("*"))
                    split = search.Split('*');
                for (var i = 0; i < 2; i++)
                    foreach (var item in items)
                    {
                        bool match;
                        if (i < 1 && split != null && split.Length == 2)
                        {
                            var regex = new Regex($".*{split[0]}(.*){split[1]}.*", RegexOptions.IgnoreCase);
                            match = regex.IsMatch(item);
                        }
                        else
                        {
                            match = item.StartsWithEx(search);
                            if (i > 0 && !match)
                                match = item.ContainsEx(search);
                        }
                        if (match)
                            return item;
                    }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return string.Empty;
        }

        internal static void Start(string appName, bool closeLauncher = false, bool runAsAdmin = false)
        {
            try
            {
                var appInfo = GetAppInfo(appName);
                if (!appInfo.LongName.EqualsEx(appName) && !appInfo.ShortName.EqualsEx(appName))
                    throw new ArgumentNullException(nameof(appName));
                var exeDir = Path.GetDirectoryName(appInfo.ExePath);
                var iniName = Path.GetFileName(appInfo.IniPath);
                if (string.IsNullOrEmpty(iniName))
                    throw new ArgumentNullException(nameof(iniName));
                if (!runAsAdmin)
                    runAsAdmin = Ini.Read(appInfo.ShortName, "RunAsAdmin", false);
                if (Directory.Exists(exeDir))
                {
                    var source = Path.Combine(exeDir, "Other", "Source", "AppNamePortable.ini");
                    if (!File.Exists(source))
                        source = Path.Combine(exeDir, "Other", "Source", iniName);
                    if (!File.Exists(appInfo.IniPath) && File.Exists(source))
                        File.Copy(source, appInfo.IniPath);
                    foreach (var file in Directory.GetFiles(exeDir, "*.ini", SearchOption.TopDirectoryOnly))
                    {
                        var content = File.ReadAllText(file);
                        if (!Regex.IsMatch(content, "DisableSplashScreen.*=.*false", RegexOptions.IgnoreCase))
                            continue;
                        content = Regex.Replace(content, "DisableSplashScreen.*=.*false", "DisableSplashScreen=true", RegexOptions.IgnoreCase);
                        File.WriteAllText(file, content);
                    }
                    var cmdLine = Ini.Read("AppInfo", "Arg", appInfo.IniPath);
                    if (Settings.Arguments.ValidPaths.Any())
                    {
                        if (string.IsNullOrWhiteSpace(cmdLine))
                        {
                            var startArgsFirst = Ini.Read(appInfo.ShortName, "StartArgs.First");
                            var argDecode = startArgsFirst.DecodeStringFromBase64();
                            if (!string.IsNullOrEmpty(argDecode))
                                startArgsFirst = argDecode;
                            var startArgsLast = Ini.Read(appInfo.ShortName, "StartArgs.Last");
                            argDecode = startArgsLast.DecodeStringFromBase64();
                            if (!string.IsNullOrEmpty(argDecode))
                                startArgsLast = argDecode;
                            cmdLine = $"{startArgsFirst}{Settings.Arguments.ValidPathsStr}{startArgsLast}";
                        }
                        var cacheId = Settings.Arguments.ValidPathsStr?.EncryptToSha1();
                        if (!string.IsNullOrEmpty(cacheId))
                        {
                            var cachePath = Path.Combine(Settings.CorePaths.TempDir, "TypeData", $"{cacheId.Substring(cacheId.Length - 8)}.ini");
                            Ini.WriteDirect(cacheId, "AppName", appName, cachePath);
                            Ini.WriteDirect("Launcher", "LastItem", appInfo.LongName);
                        }
                    }
                    if (Settings.Arguments.FileTypes.Any())
                    {
                        var types = Settings.Arguments.FileTypes.Where(x => !Settings.Arguments.SavedFileTypes.Contains(x)).ToList();
                        if (types.Any())
                        {
                            var result = MessageBoxEx.Show(types.Count == 1 ? Language.GetText(nameof(en_US.associateQuestionMsg0)) : string.Format(Language.GetText(nameof(en_US.associateQuestionMsg1)), $"{types.Join("; ")}"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result == DialogResult.Yes)
                            {
                                types = types.Select(x => $".{x}").ToList();
                                var appTypes = Ini.ReadDirect(appInfo.ShortName, "FileTypes");
                                appTypes = appTypes.Length > 0 ? $"{appTypes},{types.Join(',')}" : types.Join(',');
                                Ini.WriteDirect(appInfo.ShortName, "FileTypes", appTypes.Split(',').Sort().Join(','));
                            }
                        }
                    }
                    ProcessEx.Start(appInfo.ExePath, cmdLine, runAsAdmin);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (closeLauncher)
                Application.Exit();
        }

        [StructLayout(LayoutKind.Auto)]
        internal struct AppInfo
        {
            internal string LongName;
            internal string ShortName;
            internal string ExePath;
            internal string IniPath;
            internal string NfoPath;
        }
    }
}
