namespace AppsLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Text;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    internal static class Main
    {
        #region STARTMENU INTEGRATION

        internal static void StartMenuFolderUpdate(List<string> appList)
        {
            try
            {
                var startMenuDir = PathEx.Combine("%StartMenu%\\Programs");
                var shortcutPath = Path.Combine(startMenuDir, $"Apps Launcher{(Environment.Is64BitProcess ? " (64-bit)" : string.Empty)}.lnk");
                if (Directory.Exists(startMenuDir))
                {
                    var shortcuts = Directory.GetFiles(startMenuDir, "Apps Launcher*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (var shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(startMenuDir))
                    Directory.CreateDirectory(startMenuDir);
                Data.CreateShortcut(GetEnvironmentVariablePath(PathEx.LocalPath), shortcutPath);
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
                    var newThread = new Thread(() => Data.CreateShortcut(GetEnvironmentVariablePath(GetAppPath(tmp)), Path.Combine(startMenuDir, tmp)));
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

        internal static void SearchForUpdates()
        {
            if (SkipUpdateSearch)
                return;
            var i = Ini.ReadInteger("Settings", "UpdateCheck", 4);
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
            var lastCheck = Ini.ReadDateTime("History", "LastUpdateCheck");
            if (i.IsBetween(1, 3) && (DateTime.Now - lastCheck).TotalHours < 1d ||
                i.IsBetween(4, 6) && (DateTime.Now - lastCheck).TotalDays < 1d ||
                i.IsBetween(7, 9) && (DateTime.Now - lastCheck).TotalDays < 30d)
                return;
            if (i != 2 && i != 5 && i != 8)
                ProcessEx.Start("%CurDir%\\Binaries\\Updater.exe");
            if (i != 3 && i != 6 && i != 9)
                ProcessEx.Start(new ProcessStartInfo
                {
                    Arguments = "{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}",
#if x86
                    FileName = "%CurDir%\\Binaries\\AppsDownloader.exe"
#else
                    FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe"
#endif
                });
            Ini.Write("History", "LastUpdateCheck", DateTime.Now);
        }

        #endregion

        #region THEME STYLE

        private static string _iconCachePath;

        internal static string IconCachePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_iconCachePath))
                    return _iconCachePath;
                _iconCachePath = Path.Combine(TmpDir, "IconData.ini");
                if (File.Exists(_iconCachePath))
                    return _iconCachePath;
                try
                {
                    File.Create(_iconCachePath).Close();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    if (!Elevation.IsAdministrator)
                        Elevation.RestartAsAdministrator(CmdLine);
                }
                return _iconCachePath;
            }
        }

        internal static string SystemResourcePath => Ini.ReadString("Settings", "Window.SystemResourcePath", "%system%");
        private static MemoryStream _backgroundImageStream;
        private static Image _backgroundImage;

        internal static Image BackgroundImage
        {
            get
            {
                if (_backgroundImage == null)
                    ReloadBackgroundImage();
                return _backgroundImage;
            }
            set { _backgroundImage = value; }
        }

        internal static Image ReloadBackgroundImage()
        {
            _backgroundImage = Depiction.DimEmpty;
            var bgDir = Path.Combine(TmpDir, "bg");
            if (!Directory.Exists(bgDir))
                return _backgroundImage;
            try
            {
                foreach (var file in Directory.GetFiles(bgDir, "image.*", SearchOption.TopDirectoryOnly))
                    try
                    {
                        _backgroundImageStream?.Close();
                        _backgroundImageStream = new MemoryStream(File.ReadAllBytes(file));
                        var imgFromStream = Image.FromStream(_backgroundImageStream);
                        _backgroundImage = imgFromStream;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return _backgroundImage;
        }

        internal static bool ResetBackgroundImage()
        {
            _backgroundImage = null;
            if (BackgroundImage == _backgroundImage)
                return false;
            BackgroundImage = null;
            var bgDir = Path.Combine(TmpDir, "bg");
            _backgroundImageStream?.Close();
            try
            {
                if (Directory.Exists(bgDir))
                    Directory.Delete(bgDir, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }

        private static int? _backgroundImageLayout;

        internal static ImageLayout BackgroundImageLayout
        {
            get
            {
                if (_backgroundImageLayout == null)
                    _backgroundImageLayout = Ini.ReadInteger("Settings", "Window.BackgroundImageLayout", 1);
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

        private static string _fontFamily;

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

        private static bool FontFamilyIsAvailable(string fontFamily)
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

        internal struct ActionGuid
        {
            internal static string AllowNewInstance => "{0CA7046C-4776-4DB0-913B-D8F81964F8EE}";
            internal static bool IsAllowNewInstance => ActionGuidIsActive(AllowNewInstance);
            internal static string DisallowInterface => "{9AB50CEB-3D99-404E-BD31-4E635C09AF0F}";
            internal static bool IsDisallowInterface => ActionGuidIsActive(DisallowInterface);
            internal static string SystemIntegration => "{3A51735E-7908-4DF5-966A-9CA7626E4E3D}";
            internal static bool IsSystemIntegration => ActionGuidIsActive(SystemIntegration);
            internal static string ExtractCachedImage => "{17762FDA-39B3-4224-9525-B1A4DF75FA02}";
            internal static bool IsExtractCachedImage => ActionGuidIsActive(ExtractCachedImage);
            internal static string FileTypeAssociation => "{DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC}";
            internal static bool IsFileTypeAssociation => ActionGuidIsActive(FileTypeAssociation);
            internal static string RestoreFileTypes => "{A00C02E5-283A-44ED-9E4D-B82E8F87318F}";
            internal static bool IsRestoreFileTypes => ActionGuidIsActive(RestoreFileTypes);
            internal static string RepairDirs => "{48FDE635-60E6-41B5-8F9D-674E9F535AC7}";
            internal static bool IsRepairDirs => ActionGuidIsActive(RepairDirs);
        }

        private static bool ActionGuidIsActive(string guid)
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                return args.Length >= 2 && args.Skip(1).ContainsEx(guid);
            }
            catch
            {
                return false;
            }
        }

        private static List<string> _cmdLineArray = new List<string>();

        internal static List<string> CmdLineArray
        {
            get
            {
                try
                {
                    if (_cmdLineArray.Count == 0 && Environment.GetCommandLineArgs().Length > 1)
                    {
                        /*
                        var actionGuids = new ActionGuids();
                        var guids = new List<string>();
                        foreach (var fi in typeof(ActionGuids).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.GetValue(actionGuids) is string))
                            guids.Add((string)fi.GetValue(actionGuids));
                        */
                        int i;
                        _cmdLineArray.AddRange(Environment.GetCommandLineArgs()
                                                          .Skip(1)
                                                          .Where(s => !s.ContainsEx("/debug") &&
                                                                      !int.TryParse(s, out i) &&
                                                                      !s.ContainsEx(ActionGuid.AllowNewInstance) &&
                                                                      !s.ContainsEx(ActionGuid.ExtractCachedImage)));
                    }
                    _cmdLineArray = _cmdLineArray.OrderBy(x => x, new Comparison.AlphanumericComparer()).ToList();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                return _cmdLineArray;
            }
            set
            {
                var s = value.ToString();
                if (!_cmdLineArray.ContainsEx(s))
                    _cmdLineArray.Add(s);
            }
        }

        private static string _cmdLine = string.Empty;

        internal static string CmdLine
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cmdLine) && CmdLineArray.Count > 0)
                    return $"\"{CmdLineArray.Join("\" \"")}\"";
                return _cmdLine;
            }
            set { _cmdLine = value; }
        }

        internal static string CmdLineApp { get; set; }
        internal static bool CmdLineMultipleApps { get; private set; }

        internal static void CheckCmdLineApp()
        {
            if (string.IsNullOrWhiteSpace(CmdLine))
                return;
            try
            {
                var skip = Environment.CommandLine.ContainsEx("/debug") ? 3 : 1;
                if (Environment.GetCommandLineArgs().Length <= skip)
                    return;
                var typeData = Path.Combine(TmpDir, "TypeData.ini");
                var typeInfo = new Dictionary<string, int>();
                foreach (var arg in Environment.GetCommandLineArgs().Skip(skip))
                {
                    string ext;
                    if (Data.IsDir(arg))
                    {
                        var dirInfo = new DirectoryInfo(arg);
                        var section = dirInfo.FullName.EncryptToMd5();
                        var hash = dirInfo.GetFullHashCode(false);
                        var keys = Ini.GetKeys(section, typeData);
                        if (keys.Count > 1 && Ini.ReadInteger(section, "HashCode", typeData) == hash)
                            foreach (var key in keys)
                                typeInfo.Add(key, Ini.ReadInteger(section, key, typeData));
                        else
                        {
                            if (!File.Exists(typeData))
                                File.Create(typeData).Close();
                            Ini.RemoveSection(section, typeData);
                            Ini.Write(section, "HashCode", hash, typeData);
                            foreach (var fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Take(768))
                            {
                                if (fileInfo.MatchAttributes(FileAttributes.Hidden))
                                    continue;
                                ext = fileInfo.Extension;
                                if (string.IsNullOrWhiteSpace(ext) || typeInfo.ContainsKey(ext))
                                    continue;
                                var len = dirInfo.GetFiles("*" + ext, SearchOption.AllDirectories).Length;
                                if (len == 0)
                                    continue;
                                typeInfo.Add(ext, len);
                                Ini.Write(section, ext, len, typeData);
                            }
                        }
                        continue;
                    }
                    if (!File.Exists(arg))
                        continue;
                    if (CmdLineArray.Count > 1 && Data.MatchAttributes(arg, FileAttributes.Hidden))
                        continue;
                    ext = Path.GetExtension(arg);
                    if (string.IsNullOrEmpty(ext))
                        continue;
                    if (!typeInfo.ContainsKey(ext))
                        typeInfo.Add(ext, 1);
                    else
                        typeInfo[ext]++;
                }

                // Check app settings for the listed file types
                if (typeInfo.Count == 0)
                    return;
                string typeApp = null;
                foreach (var t in typeInfo)
                    foreach (var app in AppConfigs)
                    {
                        var fileTypes = Ini.Read(app, "FileTypes");
                        if (string.IsNullOrWhiteSpace(fileTypes))
                            continue;
                        fileTypes = $"|.{fileTypes.RemoveChar('*', '.').Replace(",", "|.")}|"; // Sort various settings formats to a single format

                        // If file type settings found for a app, select this app as default
                        if (fileTypes.ContainsEx($"|{t.Key}|"))
                        {
                            CmdLineApp = app;
                            if (string.IsNullOrWhiteSpace(typeApp))
                                typeApp = app;
                        }
                        if (CmdLineMultipleApps || string.IsNullOrWhiteSpace(CmdLineApp) || string.IsNullOrWhiteSpace(typeApp) || CmdLineApp == typeApp)
                            continue;
                        CmdLineMultipleApps = true;
                        break;
                    }

                // If multiple file types with different app settings found, select the app with most listed file types
                if (!CmdLineMultipleApps)
                    return;
                var a = string.Empty;
                var q = typeInfo.OrderByDescending(x => x.Value);
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
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        #endregion

        #region APP FUNCTIONS

        internal static string AppsDir => PathEx.Combine(PathEx.LocalDir, "Apps");

        internal static string[] AppDirs { get; set; } =
        {
            AppsDir,
            Path.Combine(AppsDir, ".free"),
            Path.Combine(AppsDir, ".repack"),
            Path.Combine(AppsDir, ".share")
        };

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
                if (appInfo.LongName == appName ||
                    appInfo.ShortName == appName)
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

        private static List<string> _appConfigs = new List<string>();

        internal static List<string> AppConfigs
        {
            get
            {
                if (_appConfigs.Count != 0)
                    return _appConfigs;
                if (AppsInfo.Count == 0)
                    CheckAvailableApps();
                _appConfigs = Ini.GetSections(Ini.File(), false).Where(s => s.ToLower() != "history" && s.ToLower() != "settings" && s.ToLower() != "host").ToList();
                return _appConfigs;
            }
            set { _appConfigs = value; }
        }

        internal static void CheckAvailableApps(bool force = true)
        {
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

                        // If there is no exe file with the same name like the directory, search in config files for the correct start file. 
                        // This step is required for multiple exe files.
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
                                foreach (var curDirEnvVar in new[] { "%CurrentDir%\\", "%CurDir%\\" })
                                    if (appDir.StartsWith(curDirEnvVar, StringComparison.OrdinalIgnoreCase))
                                        appDir = Path.Combine(Path.GetDirectoryName(iniPath), appDir.Substring(curDirEnvVar.Length));
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

                        // Apply some filters for the found app name
                        if (!appName.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase)) // No filters needed for portable Java® runtime environment because it is not listed
                        {
                            var tmp = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(appName, string.Empty);
                            if (Regex.Replace(tmp, @"\s+", " ") != appName)
                                appName = tmp;
                        }
                        appName = appName.Trim().TrimEnd(',');
                        if (string.IsNullOrWhiteSpace(appName) || !File.Exists(exePath))
                            continue;
                        if (AppsInfo.Count(x => x.LongName == appName) == 0)
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
                    // ReSharper disable once TailRecursiveCall
                    CheckAvailableApps(false);
                else
                {
                    ProcessEx.Start(new ProcessStartInfo
                    {
#if x86
                        FileName = "%CurDir%\\Binaries\\AppsDownloader.exe"
#else
                        FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe"
#endif
                    });
                    Environment.ExitCode = 0;
                    Environment.Exit(Environment.ExitCode);
                    return;
                }
                return;
            }
            AppsInfo = AppsInfo.OrderBy(x => x.LongName, new Comparison.AlphanumericComparer()).ToList();
        }

        internal static string GetAppPath(string appName)
        {
            var appInfo = GetAppInfo(appName);
            if (appInfo.LongName != appName &&
                appInfo.ShortName != appName)
                return null;
            return appInfo.ExePath;
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
                if (appInfo.LongName != appName &&
                    appInfo.ShortName != appName)
                    throw new ArgumentNullException(nameof(appName));
                Ini.Write("History", "LastItem", appInfo.LongName);
                var exeDir = Path.GetDirectoryName(appInfo.ExePath);
                //var exeName = Path.GetFileName(appInfo.ExePath);
                var iniName = Path.GetFileName(appInfo.IniPath);
                if (!runAsAdmin)
                    runAsAdmin = Ini.ReadBoolean(appInfo.ShortName, "RunAsAdmin");
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
                    if (string.IsNullOrWhiteSpace(cmdLine) && !string.IsNullOrWhiteSpace(CmdLine))
                    {
                        var startArgsFirst = Ini.Read(appInfo.ShortName, "StartArgs.First");
                        var argDecode = startArgsFirst.DecodeStringFromBase64();
                        if (!string.IsNullOrEmpty(argDecode))
                            startArgsFirst = argDecode;
                        var startArgsLast = Ini.Read(appInfo.ShortName, "StartArgs.Last");
                        argDecode = startArgsLast.DecodeStringFromBase64();
                        if (!string.IsNullOrEmpty(argDecode))
                            startArgsLast = argDecode;
                        cmdLine = $"{startArgsFirst}{CmdLine}{startArgsLast}";
                    }
                    ProcessEx.Start(appInfo.ExePath, cmdLine ?? string.Empty, runAsAdmin);
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

        internal static void AssociateFileTypes(string appName)
        {
            var types = Ini.Read(appName, "FileTypes");
            if (string.IsNullOrWhiteSpace(types))
            {
                MessageBoxEx.Show(Lang.GetText("associateBtnMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var cfgPath = PathEx.Combine(TmpDir, ActionGuid.FileTypeAssociation);
            if (!Elevation.IsAdministrator)
            {
                if (!File.Exists(cfgPath))
                    File.Create(cfgPath).Close();
                Ini.Write("AppInfo", "AppName", appName, cfgPath);
                Ini.Write("AppInfo", "ExePath", GetAppPath(appName), cfgPath);
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.FileTypeAssociation} \"{appName}\"", true, false))
                    if (p != null && !p.HasExited)
                        p.WaitForExit();
                return;
            }
            string iconData = null;
            using (Form dialog = new ResourcesEx.IconBrowserDialog(SystemResourcePath, Colors.BaseDark, Colors.ControlText, Colors.Button, Colors.ButtonText, Colors.ButtonHover))
            {
                dialog.TopMost = true;
                dialog.AddLoadingTimeStopwatch();
                dialog.ShowDialog();
                if (dialog.Text.Count(c => c == ',') == 1)
                    iconData = dialog.Text;
            }
            if (string.IsNullOrWhiteSpace(iconData))
            {
                MessageBoxEx.Show(Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var dataSplit = iconData.Split(',');
            var dataPath = GetEnvironmentVariablePath(dataSplit[0]);
            var dataId = dataSplit[1];
            if (File.Exists(PathEx.Combine(dataPath)) && !string.IsNullOrWhiteSpace(dataId))
                iconData = $"{dataPath},{dataSplit[1]}";
            string appPath;
            MessageBoxEx.ButtonText.OverrideEnabled = true;
            MessageBoxEx.ButtonText.Yes = "App";
            MessageBoxEx.ButtonText.No = "Launcher";
            MessageBoxEx.ButtonText.Cancel = Lang.GetText("Cancel");
            var result = MessageBoxEx.Show(Lang.GetText("associateAppWayQuestion"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    appPath = GetAppPath(appName);
                    if (string.IsNullOrWhiteSpace(appPath) && File.Exists(cfgPath))
                    {
                        if (appName == Ini.Read("AppInfo", "AppName", cfgPath))
                            appPath = Ini.Read("AppInfo", "ExePath", cfgPath);
                        try
                        {
                            File.Delete(cfgPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                    break;
                default:
                    MessageBoxEx.Show(Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            if (!File.Exists(appPath))
            {
                MessageBoxEx.Show(Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            NotifyBox.Show(Lang.GetText("AssociateMsg"), Title, NotifyBox.NotifyBoxStartPosition.Center);
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
                    Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
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
                if (Reg.SubKeyExist($"HKCR\\.{type}"))
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
                        Ini.Write(type.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    Ini.Write(type.EncryptToMd5(), "KeyAdded", $"HKCR\\.{type}", restPointCfgPath);
                string typeKey = $"PortableAppsSuite_{type}file";
                if (Reg.SubKeyExist($"HKCR\\{typeKey}"))
                {
                    string restKeyName = $"KeyBackup_{typeKey}_#####.reg";
                    var count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.AllDirectories).Length;
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    var restKeyPath = Path.Combine(restPointDir, restKeyName);
                    Reg.ExportKeys(restKeyPath.Replace("#####", count.ToString()), $"HKCR\\{typeKey}");
                    if (File.Exists(restKeyPath))
                        Ini.Write(typeKey.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    Ini.Write(typeKey.EncryptToMd5(), "KeyAdded", $"HKCR\\{typeKey}", restPointCfgPath);
                Reg.WriteValue(Reg.RegKey.ClassesRoot, $".{type}", null, typeKey, Reg.RegValueKind.ExpandString);
                var iconRegEnt = Reg.ReadStringValue(Reg.RegKey.ClassesRoot, $"{typeKey}\\DefaultIcon", null);
                if (iconRegEnt != iconData)
                    Reg.WriteValue(Reg.RegKey.ClassesRoot, $"{typeKey}\\DefaultIcon", null, iconData, Reg.RegValueKind.ExpandString);
                var openCmdRegEnt = Reg.ReadStringValue(Reg.RegKey.ClassesRoot, $"{typeKey}\\shell\\open\\command", null);
                string openCmd = $"\"{GetEnvironmentVariablePath(appPath)}\" \"%1\"";
                if (openCmdRegEnt != openCmd)
                    Reg.WriteValue(Reg.RegKey.ClassesRoot, $"{typeKey}\\shell\\open\\command", null, openCmd, Reg.RegValueKind.ExpandString);
                Reg.RemoveValue(Reg.RegKey.ClassesRoot, $"{typeKey}\\shell\\open\\command", "DelegateExecute");
            }
            NotifyBox.Close();
            MessageBoxEx.Show(Lang.GetText("OperationCompletedMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void RestoreFileTypes(string appName)
        {
            if (string.IsNullOrEmpty(appName))
                return;
            if (!Elevation.IsAdministrator)
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.RestoreFileTypes} \"{appName}\"", true, false))
                    if (p != null && !p.HasExited)
                        p.WaitForExit();
            var restPointDir = PathEx.Combine("%CurDir%\\Restoration", Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appName, "FileAssociation");
            string restPointPath;
            using (var dialog = new OpenFileDialog { Filter = @"INI Files(*.ini) | *.ini", InitialDirectory = restPointDir, Multiselect = false, RestoreDirectory = false })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    MessageBoxEx.Show(Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                restPointPath = dialog.FileName;
            }
            if (!File.Exists(restPointPath))
                return;
            foreach (var section in Ini.GetSections(restPointPath))
                try
                {
                    var val = Ini.Read(section, "KeyBackup", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        val = Ini.Read(section, "KeyAdded", restPointPath);
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
                        Reg.RemoveExistSubKey(val);
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
            MessageBoxEx.Show(Lang.GetText("OperationCompletedMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region SYSTEM INTEGRATION

        internal static void SystemIntegration(bool enabled, bool response = true)
        {
            if (!Elevation.IsAdministrator)
            {
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.SystemIntegration} {enabled}", true, false))
                    if (p != null && !p.HasExited)
                        p.WaitForExit();
                return;
            }
            try
            {
                const string varble = "AppsSuiteDir";
                var varKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment";
                var varDir = Reg.ReadStringValue(varKey, varble);
                var curDir = PathEx.LocalDir;
                if (!enabled || !string.Equals(varDir, curDir, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (enabled)
                        Reg.WriteValue(varKey, varble, curDir);
                    else
                        Reg.RemoveValue(varKey, varble);
                    if (WinApi.UnsafeNativeMethods.SendNotifyMessage((IntPtr)0xffff, (uint)WinApi.WindowMenuFunc.WM_SETTINGCHANGE, (UIntPtr)0, "Environment"))
                    {
                        foreach (var s in new[] { "*", "Directory" })
                        {
                            varKey = $"HKCR\\{s}\\shell\\portableapps";
                            if (enabled)
                            {
                                Reg.WriteValue(varKey, null, Lang.GetText("shellText"));
                                Reg.WriteValue(varKey, "Icon", $"\"{PathEx.LocalPath}\"");
                            }
                            else
                                Reg.RemoveExistSubKey(varKey);
                            varKey = $"{varKey}\\command";
                            if (enabled)
                                Reg.WriteValue(varKey, null, $"\"{PathEx.LocalPath}\" \"%1\"");
                            else
                                Reg.RemoveExistSubKey(varKey);
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
                                        if (p != null && !p.HasExited)
                                            p.WaitForExit();
                                    Environment.SetEnvironmentVariable(varble, curDir, EnvironmentVariableTarget.Process);
                                    Data.CreateShortcut(GetEnvironmentVariablePath(PathEx.LocalPath), file);
                                    break;
                                }
                            }
                        }
                        else
                            Data.UnpinFromTaskbar(PathEx.LocalPath);
                        if (response)
                            MessageBox.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (response)
                MessageBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static bool CheckEnvironmentVariable()
        {
            var appsSuiteDir = EnvironmentEx.GetVariableValue("AppsSuiteDir");
            if (string.IsNullOrWhiteSpace(appsSuiteDir))
                return false;
            var curDir = PathEx.LocalDir;
            if (appsSuiteDir != curDir && !Ini.ReadBoolean("Settings", "Develop"))
                SystemIntegration(true, false);
            appsSuiteDir = EnvironmentEx.GetVariableValue("AppsSuiteDir");
            return appsSuiteDir == curDir;
        }

        internal static string GetEnvironmentVariablePath(string path)
        {
            try
            {
                if (!CheckEnvironmentVariable())
                    throw new ArgumentException();
                var s = path.Replace(PathEx.LocalDir, "%AppsSuiteDir%");
                return s;
            }
            catch
            {
                return path;
            }
        }

        #endregion

        #region REPAIRING

        internal static void RepairAppsSuiteDirs()
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
                RepairDesktopIniFiles();
            }
            catch (Exception ex)
            {
                if (!Elevation.IsAdministrator)
                    Elevation.RestartAsAdministrator();
                Log.Write(ex);
            }
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private static void RepairDesktopIniFiles()
        {
            var iniPath = Path.Combine(AppDirs[0], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.blue.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[1], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Freeware", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.green.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[2], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "\"PortableApps.com\" - Repacks", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.pink.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = Path.Combine(AppDirs[3], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Shareware", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.red.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Assets\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "IconResource", "win10.folder.gray.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Binaries\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.gray.ico,0", iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "Profile", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            Ini.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.Write(".ShellClassInfo", "IconIndex", -235, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Documents\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21770", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            Ini.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.Write(".ShellClassInfo", "IconIndex", -235, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Music\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21790", iniPath);
            Ini.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12689", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,103", iniPath);
            Ini.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.Write(".ShellClassInfo", "IconIndex", -237, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Pictures\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21779", iniPath);
            Ini.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12688", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,108", iniPath);
            Ini.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.Write(".ShellClassInfo", "IconIndex", -236, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Documents\\Videos\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            Ini.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21791", iniPath);
            Ini.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12690", iniPath);
            Ini.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,178", iniPath);
            Ini.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            Ini.Write(".ShellClassInfo", "IconIndex", -238, iniPath);
            Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            iniPath = PathEx.Combine("%CurDir%\\Help\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                Data.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.green.ico,0", iniPath);
                Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }
            iniPath = PathEx.Combine("%CurDir%\\Langs\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                Data.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.gray.ico,0", iniPath);
                Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }
            iniPath = PathEx.Combine("%CurDir%\\Restoration\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                Data.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly | FileAttributes.Hidden);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                Ini.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
                Data.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }
        }

        #endregion

        #region MISC FUNCTIONS

        internal const string Title =
#if x86
            "Apps Launcher";
#else
            "Apps Launcher (64-bit)";
#endif

        internal static readonly string TmpDir = PathEx.Combine("%CurDir%\\Documents\\.cache");

        internal static readonly NotifyBox NotifyBox = new NotifyBox { Opacity = .8d };

        internal static int ScreenDpi
        {
            get
            {
                int dpi;
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    dpi = (int)Math.Ceiling(g.DpiX);
                return dpi;
            }
        }

        internal static string SearchMatchItem(string search, List<string> items)
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
                            match = item.StartsWith(search, StringComparison.OrdinalIgnoreCase);
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
