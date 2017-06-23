namespace AppsLauncher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Text;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using LangResources;
    using Microsoft.Win32;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    internal static class Main
    {
        #region STARTMENU INTEGRATION

        internal static void StartMenuFolderUpdateHandler(List<string> appList)
        {
            try
            {
                var startMenuDir = PathEx.Combine("%StartMenu%\\Programs");
#if x86
                var shortcutPath = Path.Combine(startMenuDir, "Apps Launcher.lnk");
#else
                var shortcutPath = Path.Combine(startMenuDir, "Apps Launcher (64-bit).lnk");
#endif
                if (Directory.Exists(startMenuDir))
                {
                    var shortcuts = Directory.GetFiles(startMenuDir, "Apps Launcher*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (var shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(startMenuDir))
                    Directory.CreateDirectory(startMenuDir);
                Data.CreateShortcut(EnvironmentEx.GetVariablePathFull(PathEx.LocalPath, false), shortcutPath);
                startMenuDir = Path.Combine(startMenuDir, "Portable Apps");
                if (Directory.Exists(startMenuDir))
                {
                    var shortcuts = Directory.GetFiles(startMenuDir, "*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (var shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(startMenuDir))
                    Directory.CreateDirectory(startMenuDir);
                var threadList = new List<Thread>();
                foreach (var app in appList)
                {
                    if (app.ContainsEx("Portable"))
                        continue;
                    var tmp = app;
                    var newThread = new Thread(() => Data.CreateShortcut(EnvironmentEx.GetVariablePathFull(GetAppPath(tmp), false), Path.Combine(startMenuDir, tmp)));
                    newThread.Start();
                    threadList.Add(newThread);
                }
                foreach (var thread in threadList)
                    do
                    {
                        Thread.Sleep(100);
                    }
                    while (thread.IsAlive);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        #endregion

        #region UPDATE SEARCH

        internal static bool SkipUpdateSearch { get; set; } = false;

        internal static void UpdateSearchHandler()
        {
            if (SkipUpdateSearch)
                return;
            var cc = Thread.CurrentThread.CurrentCulture.EnglishName;
            if (!cc.EqualsEx(Ini.Read("History", "CurrentCulture")))
            {
                Ini.Write("History", "CurrentCulture", cc);
                Ini.Write("History", "LastUpdateCheck", DateTime.Now);
                foreach (var section in AppConfigs)
                {
                    if (string.IsNullOrEmpty(Ini.Read(section, "NoUpdatesTime")))
                        continue;
                    Ini.Write(section, "NoUpdatesTime", DateTime.Now);
                }
                Ini.WriteAll();
                return;
            }
            var i = Ini.Read("Settings", "UpdateCheck", 4);
            /*
                Options Index:
                    0. Never
                    1. Hourly (full)
                    2. Hourly (only apps)
                    3. Hourly (only apps suite)
                    4. Daily (full)
                    5. Daily (only apps)
                    6. Daily (only apps suite)
                    7. Monthly (full)
                    8. Monthly (only apps)
                    9. Monthly (only apps suite)
            */
            if (!i.IsBetween(1, 9))
                return;
            var lastCheck = Ini.Read<DateTime>("History", "LastUpdateCheck");
            if (lastCheck != default(DateTime) &&
                (i.IsBetween(1, 3) && (DateTime.Now - lastCheck).TotalHours < 1d ||
                 i.IsBetween(4, 6) && (DateTime.Now - lastCheck).TotalDays < 1d ||
                 i.IsBetween(7, 9) && (DateTime.Now - lastCheck).TotalDays < 30d))
                return;
            if (i != 2 && i != 5 && i != 8)
                ProcessEx.Start("%CurDir%\\Binaries\\Updater.exe");
            if (i != 3 && i != 6 && i != 9)
                ProcessEx.Start(new ProcessStartInfo
                {
                    Arguments = ActionGuid.UpdateInstance,
#if x86
                    FileName = "%CurDir%\\Binaries\\AppsDownloader.exe"
#else
                    FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe"
#endif
                });
            Ini.Write("History", "LastUpdateCheck", DateTime.Now);
            Ini.WriteAll();
        }

        #endregion

        #region THEME STYLE

        private static string _systemResourcePath, _fontFamily;
        private static Image _backgroundImage;
        private static int? _backgroundImageLayout;

        internal static string SystemResourcePath =>
            _systemResourcePath ?? (_systemResourcePath = Ini.Read<string>("Settings", "Window.SystemResourcePath", "%system%"));

        internal static Image BackgroundImage
        {
            get
            {
                if (_backgroundImage != null)
                    return _backgroundImage;
                var bgPath = PathEx.Combine(TmpDir, "ImageBg.dat");
                if (!File.Exists(bgPath))
                    _backgroundImage = Depiction.DimEmpty;
                else
                    try
                    {
                        var bgImg = File.ReadAllBytes(bgPath).DeserializeObject<Image>();
                        if (bgImg != null)
                            _backgroundImage = bgImg;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                return _backgroundImage;
            }
            set { _backgroundImage = value; }
        }

        internal static ImageLayout BackgroundImageLayout
        {
            get
            {
                if (_backgroundImageLayout == null)
                    _backgroundImageLayout = Ini.Read("Settings", "Window.BackgroundImageLayout", 1);
                return (ImageLayout)_backgroundImageLayout;
            }
        }

        internal struct Colors
        {
            internal static Color System = Color.SlateGray;
            internal static Color Base = Color.SlateGray;
            internal static Color BaseDark = SystemColors.ControlDark;
            internal static Color Control = SystemColors.Window;
            internal static Color ControlText = SystemColors.WindowText;
            internal static Color Button = SystemColors.ButtonFace;
            internal static Color ButtonHover = ProfessionalColors.ButtonSelectedHighlight;
            internal static Color ButtonText = SystemColors.ControlText;
            internal static Color Highlight = SystemColors.Highlight;
            internal static Color HighlightText = SystemColors.HighlightText;
        }

        internal static string FontFamily
        {
            get { return _fontFamily; }
            set
            {
                if (FontFamilyIsAvailable(value))
                    _fontFamily = value;
            }
        }

        internal static bool SetFont(Control control, bool full = true)
        {
            if (string.IsNullOrEmpty(FontFamily))
                return false;
            try
            {
                if (string.IsNullOrEmpty(control.Text))
                    control.Font = new Font(FontFamily, control.Font.Size, control.Font.Style, control.Font.Unit);
                else
                {
                    float width = TextRenderer.MeasureText(control.Text, control.Font).Width;
                    var size = control.Font.Size;
                    using (var f = new Font(FontFamily, size, control.Font.Style, control.Font.Unit))
                    {
                        var font = f;
                        while (TextRenderer.MeasureText(control.Text, font).Width < width)
                            font = new Font(FontFamily, size += .01f, control.Font.Style, control.Font.Unit);
                        while (TextRenderer.MeasureText(control.Text, font).Width > width)
                            font = new Font(FontFamily, size -= .01f, control.Font.Style, control.Font.Unit);
                        control.Font = font;
                    }
                }
                if (full)
                    foreach (Control c in control.Controls)
                        SetFont(c);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return true;
        }

        internal static string[] GetInstalledFontFamilies()
        {
            string[] names;
            using (var fonts = new InstalledFontCollection())
                names = fonts.Families.Select(x => x.Name).ToArray();
            return names;
        }

        internal static bool FontFamilyIsAvailable(string fontFamily)
        {
            try
            {
                var names = GetInstalledFontFamilies();
                return names.Contains(fontFamily);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region COMMAND LINE FUNCTIONS

        private static List<string> _receivedPathsArray;
        private static string _receivedPathsStr;

        internal struct ActionGuid
        {
            internal static string AllowNewInstance = "{0CA7046C-4776-4DB0-913B-D8F81964F8EE}";
            internal static bool IsAllowNewInstance => IsActive(AllowNewInstance);
            internal const string DisallowInterface = "{9AB50CEB-3D99-404E-BD31-4E635C09AF0F}";
            internal static bool IsDisallowInterface => IsActive(DisallowInterface);
            internal const string SystemIntegration = "{3A51735E-7908-4DF5-966A-9CA7626E4E3D}";
            internal static bool IsSystemIntegration => IsActive(SystemIntegration);
            internal const string FileTypeAssociation = "{DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC}";
            internal static bool IsFileTypeAssociation => IsActive(FileTypeAssociation);
            internal const string RestoreFileTypes = "{A00C02E5-283A-44ED-9E4D-B82E8F87318F}";
            internal static bool IsRestoreFileTypes => IsActive(RestoreFileTypes);
            internal const string RepairDirs = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7}";
            internal static bool IsRepairDirs => IsActive(RepairDirs);
            internal const string UpdateInstance = "{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}";

            private static bool IsActive(string guid)
            {
                try
                {
                    var args = Environment.GetCommandLineArgs();
                    return args.Contains(guid);
                }
                catch
                {
                    return false;
                }
            }
        }

        internal static List<string> ReceivedPathsArray
        {
            get
            {
                if (_receivedPathsArray == null)
                    _receivedPathsArray = new List<string>();
                if (_receivedPathsArray.Count > 0 || Environment.GetCommandLineArgs().Length < 2)
                    return _receivedPathsArray;
                try
                {
                    var args = Environment.GetCommandLineArgs()
                                          .Skip(1).Where(PathEx.IsValidPath)
                                          .OrderBy(x => x, new Comparison.AlphanumericComparer());
                    _receivedPathsArray = args.ToList();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                return _receivedPathsArray;
            }
        }

        internal static string ReceivedPathsStr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_receivedPathsStr) && ReceivedPathsArray.Count > 0)
                    return $"\"{ReceivedPathsArray.Join("\" \"")}\"";
                return _receivedPathsStr ?? string.Empty;
            }
            set { _receivedPathsStr = value; }
        }

        internal static string CmdLineApp { get; set; }
        internal static bool CmdLineMultipleApps { get; private set; }

        internal static void CheckCmdLineApp()
        {
            if (ReceivedPathsArray.Count == 0)
                return;
            var hash = ReceivedPathsStr?.EncryptToSha1();
            var cache = Path.Combine(TmpDir, "TypeData.ini");
            var appName = Ini.ReadDirect(hash, "AppName", cache);
            if (!string.IsNullOrEmpty(appName))
            {
                CmdLineApp = appName;
                return;
            }
            var allTypes = AppConfigs.Select(x => Ini.Read(x, "FileTypes"))
                                     .Select(x => x.ToCharArray())
                                     .Select(x => x.Where(y => y == ',' || char.IsLetterOrDigit(y)))
                                     .Select(x => new string(x.ToArray()))
                                     .Aggregate(string.Empty, (x, y) => x + y)
                                     .Split(',').Select(x => '.' + x).ToArray();
            var typeInfo = new Dictionary<string, int>();
            var stopwatch = new Stopwatch();
            foreach (var path in ReceivedPathsArray)
                try
                {
                    string ext;
                    if (Data.IsDir(path))
                    {
                        stopwatch.Start();
                        var dirInfo = new DirectoryInfo(path);
                        var section = dirInfo.FullName.EncryptToMd5();
                        var dirHash = dirInfo.GetFullHashCode(false);
                        var keys = Ini.GetKeys(section, cache);
                        if (keys.Count > 1 && Ini.ReadDirect(section, "HashCode", cache).Equals(dirHash.ToString()))
                        {
                            foreach (var key in keys)
                                typeInfo.Add(key, Ini.Read(section, key, 0, cache));
                            continue;
                        }
                        Ini.WriteDirect(section, null, null, cache);
                        Ini.WriteDirect(section, "HashCode", dirHash, cache);
                        foreach (var fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Take(1024))
                        {
                            if (fileInfo.MatchAttributes(FileAttributes.Hidden))
                                continue;
                            ext = fileInfo.Extension;
                            if (typeInfo.ContainsKey(ext) || !ext.EndsWithEx(allTypes))
                                continue;
                            var len = dirInfo.GetFiles("*" + ext, SearchOption.AllDirectories).Length;
                            if (len == 0)
                                continue;
                            typeInfo.Add(ext, len);
                            Ini.WriteDirect(section, ext, len, cache);
                            if (stopwatch.ElapsedMilliseconds >= 4096)
                                break;
                        }
                        stopwatch.Reset();
                        continue;
                    }
                    if (!File.Exists(path))
                        continue;
                    if (Data.MatchAttributes(path, FileAttributes.Hidden))
                        continue;
                    ext = Path.GetExtension(path);
                    if (string.IsNullOrEmpty(ext))
                        continue;
                    if (!typeInfo.ContainsKey(ext))
                        typeInfo.Add(ext, 1);
                    else
                        typeInfo[ext]++;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            Ini.WriteAll(cache);

            // Check app settings for the listed file types
            if (typeInfo?.Count == 0)
                return;
            string typeApp = null;
            foreach (var t in typeInfo)
                foreach (var app in AppConfigs)
                {
                    var fileTypes = Ini.Read(app, "FileTypes");
                    if (string.IsNullOrWhiteSpace(fileTypes))
                        continue;
                    fileTypes = $"|.{fileTypes.RemoveChar('*', '.')?.Replace(",", "|.")}|"; // Enforce format

                    // If file type settings found for a app, select this app as default
                    if (fileTypes.ContainsEx($"|{t.Key}|"))
                    {
                        CmdLineApp = app;
                        if (string.IsNullOrWhiteSpace(typeApp))
                            typeApp = app;
                    }
                    if (CmdLineMultipleApps || string.IsNullOrWhiteSpace(CmdLineApp) || string.IsNullOrWhiteSpace(typeApp) || CmdLineApp.EqualsEx(typeApp))
                        continue;
                    CmdLineMultipleApps = true;
                    break;
                }

            // If multiple file types with different app settings found, select the app with most listed file types
            if (!CmdLineMultipleApps)
                return;
            var a = string.Empty;
            var q = typeInfo?.OrderByDescending(x => x.Value);
            if (q == null)
                return;
            var c = 0;
            foreach (var x in q)
            {
                if (x.Value > c)
                    a = x.Key;
                c = x.Value;
            }
            if (string.IsNullOrWhiteSpace(a))
                return;
            foreach (var app in AppConfigs)
            {
                var fileTypes = Ini.Read(app, "FileTypes");
                if (string.IsNullOrWhiteSpace(fileTypes))
                    continue;
                fileTypes = $"|.{fileTypes.RemoveChar('*', '.').Replace(",", "|.")}|"; // Filter
                if (!fileTypes.ContainsEx($"|{a}|"))
                    continue;
                CmdLineApp = app;
                break;
            }
        }

        #endregion

        #region APP FUNCTIONS

        private static string _appsDir;
        private static string[] _appsDirs;
        private static List<string> _appConfigs;

        internal static string AppsDir =>
            _appsDir ?? (_appsDir = PathEx.Combine(PathEx.LocalDir, "Apps"));

        internal static string[] AppDirs { get; set; } =
            _appsDirs ?? (_appsDirs = new[]
            {
                AppsDir,
                Path.Combine(AppsDir, ".free"),
                Path.Combine(AppsDir, ".repack"),
                Path.Combine(AppsDir, ".share")
            });

        internal static void SetAppDirs()
        {
            var dirs = Ini.Read("Settings", "AppDirs");
            if (string.IsNullOrWhiteSpace(dirs))
                return;
            dirs = dirs.DecodeStringFromBase64();
            if (string.IsNullOrWhiteSpace(dirs))
                return;
            if (!dirs.Contains(Environment.NewLine))
                dirs += Environment.NewLine;
            AppDirs = AppDirs.Concat(dirs.SplitNewLine()).Where(s => Directory.Exists(PathEx.Combine(s))).ToArray();
        }

        internal static List<AppInfo> AppsInfo = new List<AppInfo>();

        internal static AppInfo GetAppInfo(string appName)
        {
            if (AppsInfo.Count <= 0 || string.IsNullOrWhiteSpace(appName))
                return new AppInfo();
            foreach (var appInfo in AppsInfo)
                if (appName.EqualsEx(appInfo.LongName, appInfo.ShortName))
                    return appInfo;
            return new AppInfo();
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

        internal static List<string> AppConfigs
        {
            get
            {
                if (_appConfigs == null)
                    _appConfigs = new List<string>();
                if (_appConfigs.Count > 0)
                    return _appConfigs;
                if (AppsInfo.Count == 0)
                    CheckAvailableApps();
                _appConfigs = Ini.GetSections(false).Where(s => !s.EqualsEx("History", "Host", "Settings")).ToList();
                return _appConfigs;
            }
            set { _appConfigs = value; }
        }

        internal static void CheckAvailableApps(bool force = true)
        {
            ReCheck:
            if (!force && AppsInfo.Count > 0)
                return;
            AppsInfo.Clear();
            foreach (var d in AppDirs)
                try
                {
                    var dir = PathEx.Combine(d);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        continue;
                    }
                    foreach (var path in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).Where(s => s.ContainsEx("Portable")))
                    {
                        var dirName = Path.GetFileName(path);

                        // If there is no exe file with the same name as the directory, search in config files for the correct start file
                        // This step is required for multiple exe files
                        var exePath = Path.Combine(dir, $"{dirName}\\{dirName}.exe");
                        var iniPath = exePath.Replace(".exe", ".ini");
                        var nfoPath = Path.Combine(path, "App\\AppInfo\\appinfo.ini");
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
                                    if (appDir.StartsWithEx(vars))
                                    {
                                        var curDir = Path.GetDirectoryName(iniPath);
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
                        if (AppsInfo.Count(x => x.LongName.EqualsEx(appName)) == 0)
                            AppsInfo.Add(new AppInfo
                            {
                                LongName = appName,
                                ShortName = dirName,
                                ExePath = exePath,
                                IniPath = iniPath,
                                NfoPath = nfoPath
                            });
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            if (AppsInfo.Count == 0)
            {
                if (force)
                {
                    force = false;
                    goto ReCheck;
                }
#if x86
                ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader.exe");
#else
                ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader64.exe");
#endif
                Environment.ExitCode = 0;
                Environment.Exit(Environment.ExitCode);
                return;
            }
            AppsInfo = AppsInfo.OrderBy(x => x.LongName, new Comparison.AlphanumericComparer()).ToList();
        }

        internal static string GetAppPath(string appName)
        {
            var appInfo = GetAppInfo(appName);
            return appName.EqualsEx(appInfo.LongName, appInfo.ShortName) ? appInfo.ExePath : null;
        }

        internal static void OpenAppLocation(string appName, bool closeLancher = false)
        {
            try
            {
                var dir = Path.GetDirectoryName(GetAppPath(appName));
                if (!Directory.Exists(dir))
                    throw new PathNotFoundException(dir);
                ProcessEx.Start("%WinDir%\\explorer.exe", dir);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (closeLancher)
                Application.Exit();
        }

        internal static void StartApp(string appName, bool closeLauncher = false, bool runAsAdmin = false)
        {
            try
            {
                var appInfo = GetAppInfo(appName);
                if (!appInfo.LongName.EqualsEx(appName) && !appInfo.ShortName.EqualsEx(appName))
                    throw new ArgumentNullException(nameof(appName));
                var exeDir = Path.GetDirectoryName(appInfo.ExePath);
                var iniName = Path.GetFileName(appInfo.IniPath);
                if (!runAsAdmin)
                    runAsAdmin = Ini.Read(appInfo.ShortName, "RunAsAdmin", false);
                if (Directory.Exists(exeDir))
                {
                    var source = Path.Combine(exeDir, "Other\\Source\\AppNamePortable.ini");
                    if (!File.Exists(source))
                        source = Path.Combine(exeDir, $"Other\\Source\\{iniName}");
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
                    if (ReceivedPathsArray.Count > 0)
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
                            cmdLine = $"{startArgsFirst}{ReceivedPathsStr}{startArgsLast}";
                        }
                        if (ReceivedPathsArray.Where(Data.IsDir).Any())
                        {
                            var hash = ReceivedPathsStr?.EncryptToSha1();
                            var cache = Path.Combine(TmpDir, "TypeData.ini");
                            Ini.WriteDirect(hash, "AppName", appName, cache);
                        }
                        Ini.WriteDirect("History", "LastItem", appInfo.LongName);
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

        #endregion

        #region FILE TYPE ASSOCIATION

        internal static void AssociateFileTypesHandler(string appName, Form owner = null)
        {
            var types = Ini.Read(appName, "FileTypes");
            if (string.IsNullOrWhiteSpace(types))
            {
                MessageBoxEx.Show(Lang.GetText(nameof(en_US.associateBtnMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var cfgPath = PathEx.Combine(TmpDir, ActionGuid.FileTypeAssociation);
            if (!Elevation.IsAdministrator)
            {
                if (owner != null)
                {
                    owner.Enabled = false;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.Indeterminate);
                }
                var bw = new BackgroundWorker();
                bw.DoWork += (sender, args) =>
                {
                    if (!File.Exists(cfgPath))
                        File.Create(cfgPath).Close();
                    Ini.WriteDirect("AppInfo", "AppName", appName, cfgPath);
                    Ini.WriteDirect("AppInfo", "ExePath", GetAppPath(appName), cfgPath);
                    using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.FileTypeAssociation} \"{appName}\"", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    try
                    {
                        File.Delete(cfgPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                };
                bw.RunWorkerCompleted += (sender, args) =>
                {
                    if (owner == null)
                        return;
                    owner.Enabled = true;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.NoProgress);
                    if (WinApi.NativeHelper.GetForegroundWindow() != owner.Handle)
                        WinApi.NativeHelper.SetForegroundWindow(owner.Handle);
                };
                bw.RunWorkerAsync();
                return;
            }
            string iconData = null;
            using (Form dialog = new ResourcesEx.IconBrowserDialog(SystemResourcePath, Colors.BaseDark, Colors.ControlText, Colors.Button, Colors.ButtonText, Colors.ButtonHover))
            {
                dialog.TopMost = true;
                dialog.Plus();
                dialog.ShowDialog();
                if (dialog.Text.Count(c => c == ',') == 1)
                    iconData = dialog.Text;
            }
            if (string.IsNullOrWhiteSpace(iconData))
            {
                MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var dataSplit = iconData.Split(',');
            var dataPath = EnvironmentEx.GetVariablePathFull(dataSplit[0], false);
            var dataId = dataSplit[1];
            if (File.Exists(PathEx.Combine(dataPath)) && !string.IsNullOrWhiteSpace(dataId))
                iconData = $"{dataPath},{dataSplit[1]}";
            string appPath;
            MessageBoxEx.ButtonText.OverrideEnabled = true;
            MessageBoxEx.ButtonText.Yes = "App";
            MessageBoxEx.ButtonText.No = "Launcher";
            MessageBoxEx.ButtonText.Cancel = Lang.GetText(nameof(en_US.Cancel));
            var result = MessageBoxEx.Show(Lang.GetText(nameof(en_US.associateAppWayQuestion)), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    appPath = GetAppPath(appName);
                    if (string.IsNullOrWhiteSpace(appPath) && File.Exists(cfgPath) && appName.EqualsEx(Ini.ReadDirect("AppInfo", "AppName", cfgPath)))
                        appPath = Ini.ReadDirect("AppInfo", "ExePath", cfgPath);
                    break;
                default:
                    MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            if (!File.Exists(appPath))
            {
                MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EnvironmentEx.SystemRestore.Create($"{appName} - File Type Assotiation", EnvironmentEx.SystemRestore.EventType.BeginSystemChange, EnvironmentEx.SystemRestore.PointType.ModifySettings);
            var restPointDir = PathEx.Combine("%CurDir%\\Restoration");
            try
            {
                if (!Directory.Exists(restPointDir))
                {
                    Directory.CreateDirectory(restPointDir);
                    Data.SetAttributes(restPointDir, FileAttributes.ReadOnly | FileAttributes.Hidden);
                    var iniPath = Path.Combine(restPointDir, "desktop.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
                    Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            restPointDir = Path.Combine(restPointDir, Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appName, "FileAssociation", DateTime.Now.ToString("yy-MM-dd"));
            var backupCount = 0;
            if (Directory.Exists(restPointDir))
                backupCount = Directory.GetFiles(restPointDir, "*.ini", SearchOption.TopDirectoryOnly).Length;
            else
                try
                {
                    Directory.CreateDirectory(restPointDir);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            var restPointCfgPath = Path.Combine(restPointDir, $"{backupCount}.ini");
            if (!File.Exists(restPointCfgPath))
                File.Create(restPointCfgPath).Close();
            restPointDir = Path.Combine(restPointDir, backupCount.ToString());
            foreach (var type in (types.Contains(",") ? types : $"{types},").Split(','))
            {
                if (string.IsNullOrWhiteSpace(type) || type.StartsWith("."))
                    continue;
                if (Reg.SubKeyExists($"HKCR\\.{type}"))
                {
                    string restKeyName = $"KeyBackup_.{type}_#####.reg";
                    var count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.TopDirectoryOnly).Length;
                    else
                        try
                        {
                            Directory.CreateDirectory(restPointDir);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    var restKeyPath = Path.Combine(restPointDir, restKeyName);
                    Reg.ExportKeys(restKeyPath, $"HKCR\\.{type}");
                    if (File.Exists(restKeyPath))
                        Ini.WriteDirect(type.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    Ini.WriteDirect(type.EncryptToMd5(), "KeyAdded", $"HKCR\\.{type}", restPointCfgPath);
                string typeKey = $"PortableAppsSuite_{type}file";
                if (Reg.SubKeyExists($"HKCR\\{typeKey}"))
                {
                    string restKeyName = $"KeyBackup_{typeKey}_#####.reg";
                    var count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.AllDirectories).Length;
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    var restKeyPath = Path.Combine(restPointDir, restKeyName);
                    Reg.ExportKeys(restKeyPath.Replace("#####", count.ToString()), $"HKCR\\{typeKey}");
                    if (File.Exists(restKeyPath))
                        Ini.WriteDirect(typeKey.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    Ini.WriteDirect(typeKey.EncryptToMd5(), "KeyAdded", $"HKCR\\{typeKey}", restPointCfgPath);
                Reg.Write(Registry.ClassesRoot, $".{type}", null, typeKey, RegistryValueKind.ExpandString);
                var iconRegEnt = Reg.ReadString(Registry.ClassesRoot, $"{typeKey}\\DefaultIcon", null);
                if (!iconRegEnt.EqualsEx(iconData))
                    Reg.Write(Registry.ClassesRoot, $"{typeKey}\\DefaultIcon", null, iconData, RegistryValueKind.ExpandString);
                var openCmdRegEnt = Reg.ReadString(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", null);
                string openCmd = $"\"{EnvironmentEx.GetVariablePathFull(appPath, false)}\" \"%1\"";
                if (!openCmdRegEnt.EqualsEx(openCmd))
                    Reg.Write(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", null, openCmd, RegistryValueKind.ExpandString);
                Reg.RemoveEntry(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", "DelegateExecute");
            }
            MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void RestoreFileTypesHandler(string appName)
        {
            if (string.IsNullOrEmpty(appName))
                return;
            if (!Elevation.IsAdministrator)
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.RestoreFileTypes} \"{appName}\"", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
            if (EnvironmentEx.SystemRestore.IsEnabled)
                if (MessageBox.Show(Lang.GetText(nameof(en_US.RestorePointMsg)), @"Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ProcessEx.Start("%system%\\rstrui.exe");
                    return;
                }
            var restPointDir = PathEx.Combine("%CurDir%\\Restoration", Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appName, "FileAssociation");
            string restPointPath;
            using (var dialog = new OpenFileDialog { Filter = @"INI Files(*.ini) | *.ini", InitialDirectory = restPointDir, Multiselect = false, RestoreDirectory = false })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                restPointPath = dialog.FileName;
            }
            if (!File.Exists(restPointPath))
                return;
            foreach (var section in Ini.GetSections(restPointPath))
                try
                {
                    var val = Ini.ReadDirect(section, "KeyBackup", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        val = Ini.ReadDirect(section, "KeyAdded", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        throw new InvalidOperationException($"No value found for '{section}'.");
                    if (val.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = Path.GetDirectoryName(restPointPath);
                        if (!string.IsNullOrEmpty(path))
                            path = Path.Combine(path, "val");
                        if (File.Exists(path))
                            Reg.ImportFile(path);
                    }
                    else
                        Reg.RemoveSubKey(val);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            try
            {
                File.Delete(restPointPath);
                var iniDir = Path.Combine(Path.GetDirectoryName(restPointPath));
                var iniSubDir = Path.Combine(iniDir, Path.GetFileNameWithoutExtension(restPointPath));
                if (Directory.Exists(iniSubDir))
                    Directory.Delete(iniSubDir, true);
                if (Directory.GetFiles(iniDir, "*.ini", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    var path = Path.GetDirectoryName(restPointPath);
                    if (!string.IsNullOrEmpty(path))
                        Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            MessageBoxEx.Show(Lang.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region SYSTEM INTEGRATION

        internal static bool CheckEnvironmentVariable()
        {
            var appsSuiteDir = EnvironmentEx.GetVariableValue("AppsSuiteDir");
            if (string.IsNullOrWhiteSpace(appsSuiteDir))
                return false;
            var curDir = PathEx.LocalDir;
            if (!appsSuiteDir.EqualsEx(curDir) && !Ini.Read("Settings", "Develop", false))
                SystemIntegrationHandler(true, false);
            appsSuiteDir = EnvironmentEx.GetVariableValue("AppsSuiteDir");
            return appsSuiteDir.EqualsEx(curDir);
        }

        internal static void SystemIntegrationHandler(bool enabled, bool response = true)
        {
            if (!Elevation.IsAdministrator)
            {
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.SystemIntegration} {enabled}", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                return;
            }
            const string variable = "AppsSuiteDir";
            var varKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment";
            var varDir = Reg.ReadString(varKey, variable);
            var curDir = PathEx.LocalDir;
            if (!enabled || !varDir.EqualsEx(curDir))
            {
                if (enabled)
                    Reg.Write(varKey, variable, curDir);
                else
                    Reg.RemoveEntry(varKey, variable);
                if (WinApi.NativeHelper.SendNotifyMessage((IntPtr)0xffff, (uint)WinApi.WindowMenuFlags.WmSettingChange, (UIntPtr)0, "Environment"))
                {
                    foreach (var s in new[] { "*", "Directory" })
                    {
                        varKey = $"HKCR\\{s}\\shell\\portableapps";
                        if (enabled)
                        {
                            Reg.Write(varKey, null, Lang.GetText(nameof(en_US.shellText)));
                            Reg.Write(varKey, "Icon", $"\"{PathEx.LocalPath}\"");
                        }
                        else
                            Reg.RemoveSubKey(varKey);
                        varKey = $"{varKey}\\command";
                        if (enabled)
                            Reg.Write(varKey, null, $"\"{PathEx.LocalPath}\" \"%1\"");
                        else
                            Reg.RemoveSubKey(varKey);
                    }
                    if (enabled)
                    {
                        if (Data.PinToTaskbar(PathEx.LocalPath))
                        {
                            var pinnedDir = PathEx.Combine("%AppData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
                            foreach (var file in Directory.GetFiles(pinnedDir, "*.lnk", SearchOption.TopDirectoryOnly))
                            {
                                if (!string.Equals(Data.GetShortcutTarget(file), PathEx.LocalPath, StringComparison.CurrentCultureIgnoreCase))
                                    continue;
                                using (var p = ProcessEx.Send($"DEL /F /Q \"{file}\"", false, false))
                                    if (!p?.HasExited == true)
                                        p?.WaitForExit();
                                Environment.SetEnvironmentVariable(variable, curDir, EnvironmentVariableTarget.Process);
                                Data.CreateShortcut(EnvironmentEx.GetVariablePathFull(PathEx.LocalPath, false), file);
                                break;
                            }
                        }
                    }
                    else
                        Data.UnpinFromTaskbar(PathEx.LocalPath);
                    if (response)
                        MessageBox.Show(Lang.GetText(nameof(en_US.OperationCompletedMsg)), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            if (response)
                MessageBox.Show(Lang.GetText(nameof(en_US.OperationCanceledMsg)), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region REPAIRING APPS SUITE

        internal static void RepairAppsSuiteHandler()
        {
            try
            {
                var dirList = AppDirs.ToList();
                dirList.Add(PathEx.Combine("%CurDir%\\Documents"));
                dirList.Add(PathEx.Combine("%CurDir%\\Documents\\Documents"));
                dirList.Add(PathEx.Combine("%CurDir%\\Documents\\Music"));
                dirList.Add(PathEx.Combine("%CurDir%\\Documents\\Pictures"));
                dirList.Add(PathEx.Combine("%CurDir%\\Documents\\Videos"));
                foreach (var dir in dirList)
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    Data.SetAttributes(dir, FileAttributes.ReadOnly);
                }
                RepairDesktopIniFilesHandler();
            }
            catch (Exception ex)
            {
                if (!Elevation.IsAdministrator)
                    Elevation.RestartAsAdministrator();
                Log.Write(ex);
            }
        }

        private static void RepairDesktopIniFilesHandler()
        {
            var iniPath = Path.Combine(AppDirs[0], "desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.blue.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[1], "desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Freeware", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.green.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[2], "desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "\"PortableApps.com\" - Repacks", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.pink.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[3], "desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Shareware", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.red.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Assets\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "win10.folder.gray.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Binaries\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.gray.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "Profile", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconIndex", -235, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Documents\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21770", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconIndex", -235, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Music\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21790", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12689", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,103", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconIndex", -237, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Pictures\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21779", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12688", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,108", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconIndex", -236, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Videos\\desktop.ini");
            Ini.WriteDirect(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21791", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12690", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,178", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.WriteDirect(".ShellClassInfo", "IconIndex", -238, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Help\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                Data.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly);
                Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.green.ico,0", iniPath);
                Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }
            iniPath = PathEx.Combine("%CurDir%\\Restoration\\desktop.ini");
            if (!Directory.Exists(Path.GetDirectoryName(iniPath)))
                return;
            Data.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly | FileAttributes.Hidden);
            Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
        }

        #endregion

        #region MISC FUNCTIONS

        private static string _regPath;
        private static string _tmpDir;

        internal const string Title =
#if x86
            "Apps Launcher";
#else
            "Apps Launcher (64-bit)";
#endif

        internal static string RegPath =>
            _regPath ?? (_regPath = string.Format(Resources.RegKeyLayout, PathEx.LocalPath.GetHashCode()));

        internal static string TmpDir
        {
            get
            {
                if (_tmpDir != null && Directory.Exists(_tmpDir))
                    return _tmpDir;
                try
                {
                    _tmpDir = PathEx.Combine(PathEx.LocalDir, "Documents\\.cache");
                    if (!Directory.Exists(_tmpDir))
                        Directory.CreateDirectory(_tmpDir);
                }
                catch
                {
                    _tmpDir = EnvironmentEx.GetVariableValue("TEMP");
                }
                return _tmpDir;
            }
        }

        internal static void ClearCaches()
        {
            var curDir = Ini.Read("History", "CurrentDirectory");
            if (curDir.EqualsEx(PathEx.LocalDir))
                return;
            try
            {
                foreach (var file in Directory.EnumerateFiles(TmpDir, "*.ixi", SearchOption.TopDirectoryOnly))
                    File.Delete(file);
                Ini.Write("History", "CurrentDirectory", PathEx.LocalDir);
                Ini.WriteAll();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        internal static int ScreenDpi
        {
            get
            {
                int dpi;
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    dpi = (int)Math.Ceiling(Math.Max(g.DpiX, g.DpiY));
                return dpi;
            }
        }

        internal static string ItemSearchHandler(string search, List<string> items)
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

        #endregion
    }
}
