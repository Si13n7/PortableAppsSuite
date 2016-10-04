using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    internal static class Main
    {
        #region UPDATE SEARCH

        internal static bool SkipUpdateSearch { get; set; } = false;

        internal static void SearchForUpdates()
        {
            if (SkipUpdateSearch)
                return;
            int i = INI.ReadInteger("Settings", "UpdateCheck", 4);
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
            if (i.IsBetween(1, 9))
            {
                DateTime LastCheck = INI.ReadDateTime("History", "LastUpdateCheck");
                if (i.IsBetween(1, 3) && DateTime.Now.Hour != LastCheck.Hour ||
                    i.IsBetween(4, 6) && DateTime.Now.DayOfYear != LastCheck.DayOfYear ||
                    i.IsBetween(7, 9) && DateTime.Now.Month != LastCheck.Month)
                {
                    if (i != 2 && i != 5 && i != 8)
                        RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Binaries\\Updater.exe" });
                    if (i != 3 && i != 6 && i != 9)
                        RUN.App(new ProcessStartInfo()
                        {
                            Arguments = "{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}",
#if x86
                            FileName = "%CurDir%\\Binaries\\AppsDownloader.exe"
#else
                            FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe"
#endif
                        });
                    INI.Write("History", "LastUpdateCheck", DateTime.Now);
                }
            }
        }

        private static bool IsBetween<T>(this T item, T start, T end) where T : IComparable, IComparable<T>
        {
            try
            {
                return Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region THEME STYLE

        private static string _iconCachePath;
        internal static string IconCachePath
        {
            get
            {
                if (string.IsNullOrEmpty(_iconCachePath))
                {
                    _iconCachePath = Path.Combine(TmpDir, "IconData.ini");
                    if (!File.Exists(_iconCachePath))
                    {
                        try
                        {
                            File.Create(_iconCachePath).Close();
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                            if (!ELEVATION.IsAdministrator)
                                ELEVATION.RestartAsAdministrator(CmdLine);
                        }
                    }
                }
                return _iconCachePath;
            }
        }

        private static string _systemResourcePath;
        internal static string SystemResourcePath
        {
            get
            {
                if (string.IsNullOrEmpty(_systemResourcePath))
                {
                    string defPath = "%system%\\imageres.dll";
                    _systemResourcePath = INI.ReadString("Settings", "Window.SystemResourcePath", defPath);
                    if (!File.Exists(PATH.Combine(_systemResourcePath)))
                        _systemResourcePath = defPath;
                }
                return _systemResourcePath;
            }
        }

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
            _backgroundImage = DRAWING.DimEmpty;
            string bgDir = Path.Combine(TmpDir, "bg");
            if (Directory.Exists(bgDir))
            {
                try
                {
                    foreach (string file in Directory.GetFiles(bgDir, "image.*", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            if (_backgroundImageStream != null)
                                _backgroundImageStream.Close();
                            _backgroundImageStream = new MemoryStream(File.ReadAllBytes(file));
                            Image imgFromStream = Image.FromStream(_backgroundImageStream);
                            _backgroundImage = imgFromStream;
                            break;
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return _backgroundImage;
        }

        internal static bool ResetBackgroundImage()
        {
            _backgroundImage = null;
            if (BackgroundImage != _backgroundImage)
            {
                BackgroundImage = null;
                string bgDir = Path.Combine(TmpDir, "bg");
                if (_backgroundImageStream != null)
                    _backgroundImageStream.Close();
                try
                {
                    if (Directory.Exists(bgDir))
                        Directory.Delete(bgDir, true);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return false;
                }
                return true;
            }
            return false;
        }

        private static int? _backgroundImageLayout = null;
        internal static ImageLayout BackgroundImageLayout
        {
            get
            {
                if (_backgroundImageLayout == null)
                    _backgroundImageLayout = INI.ReadInteger("Settings", "Window.BackgroundImageLayout", 1);
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
        }

        private static string fontFamily = null;
        internal static string FontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                if (FontFamilyIsAvailable(value))
                    fontFamily = value;
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
                    float size = control.Font.Size;
                    using (Font f = new Font(FontFamily, size, control.Font.Style, control.Font.Unit))
                    {
                        Font font = f;
                        while (TextRenderer.MeasureText(control.Text, font).Width < width)
                            font = new Font(FontFamily, size += .01f, control.Font.Style, control.Font.Unit);
                        while (TextRenderer.MeasureText(control.Text, font).Width > width)
                            font = new Font(FontFamily, size -= .01f, control.Font.Style, control.Font.Unit);
                        control.Font = font;
                    }
                }
                if (full)
                {
                    foreach (Control c in control.Controls)
                        SetFont(c);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return true;
        }

        internal static string[] GetInstalledFontFamilies()
        {
            string[] names;
            using (InstalledFontCollection fonts = new InstalledFontCollection())
                names = fonts.Families.Select(x => x.Name).ToArray();
            return names;
        }

        private static bool FontFamilyIsAvailable(string fontFamily)
        {
            try
            {
                string[] names = GetInstalledFontFamilies();
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
            internal const string AllowNewInstance = "{0CA7046C-4776-4DB0-913B-D8F81964F8EE}";
            internal static bool IsAllowNewInstance { get; } = ActionGuidIsActive(AllowNewInstance);

            internal const string DisallowInterface = "{9AB50CEB-3D99-404E-BD31-4E635C09AF0F}";
            internal static bool IsDisallowInterface { get; } = ActionGuidIsActive(DisallowInterface);

            internal const string SystemIntegration = "{3A51735E-7908-4DF5-966A-9CA7626E4E3D}";
            internal static bool IsSystemIntegration { get; } = ActionGuidIsActive(SystemIntegration);

            internal const string ExtractCachedImage = "{17762FDA-39B3-4224-9525-B1A4DF75FA02}";
            internal static bool IsExtractCachedImage { get; } = ActionGuidIsActive(ExtractCachedImage);

            internal const string FileTypeAssociation = "{DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC}";
            internal static bool IsFileTypeAssociation { get; } = ActionGuidIsActive(FileTypeAssociation);

            internal const string RestoreFileTypes = "{A00C02E5-283A-44ED-9E4D-B82E8F87318F}";
            internal static bool IsRestoreFileTypes { get; } = ActionGuidIsActive(RestoreFileTypes);

            internal const string RepairDirs = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7}";
            internal static bool IsRepairDirs { get; } = ActionGuidIsActive(RepairDirs);
        }

        private static bool ActionGuidIsActive(string guid)
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 2)
                    return false;
                return args.Skip(1).Contains(guid);
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
                        int i = 0;
                        _cmdLineArray.AddRange(Environment.GetCommandLineArgs().Skip(1).Where(s => !s.ToLower().Contains("/debug") && !int.TryParse(s, out i) && !s.Contains(ActionGuid.AllowNewInstance) && !s.Contains(ActionGuid.ExtractCachedImage)));
                    }
                    _cmdLineArray = _cmdLineArray.OrderBy(x => x, new AscendentAlphanumericStringComparer()).ToList();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
                return _cmdLineArray;
            }
            set
            {
                string s = value.ToString();
                if (!_cmdLineArray.Contains(s, StringComparer.OrdinalIgnoreCase))
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

        internal static bool CmdLineMultipleApps { get; private set; } = false;

        internal static void CheckCmdLineApp()
        {
            if (string.IsNullOrWhiteSpace(CmdLine))
                return;
            try
            {
                int skip = Environment.CommandLine.Contains("/debug") ? 3 : 1;
                if (Environment.GetCommandLineArgs().Length > skip)
                {
                    List<string> types = new List<string>();
                    foreach (string arg in Environment.GetCommandLineArgs().Skip(skip))
                    {
                        if (DATA.IsDir(arg))
                        {
                            if (Directory.Exists(arg))
                            {
                                foreach (string file in Directory.GetFiles(arg, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower() != "desktop.ini" && !s.ToLower().EndsWith("tmp")))
                                {
                                    if (!DATA.MatchAttributes(file, FileAttributes.Hidden))
                                        types.Add(Path.GetExtension(file).ToLower());
                                    if (types.Count >= 768) // Maximum size to speed up this task
                                        break;
                                }
                            }
                            continue;
                        }

                        if (File.Exists(arg))
                            if (CmdLineArray.Count > 1 ? !DATA.MatchAttributes(arg, FileAttributes.Hidden) : true)
                                types.Add(Path.GetExtension(arg).ToLower());
                    }

                    // Check app settings for the listed file types
                    if (types.Count > 0)
                    {
                        string typeApp = null;
                        foreach (string t in types)
                        {
                            foreach (string app in AppConfigs)
                            {
                                string fileTypes = INI.Read(app, "FileTypes");
                                if (string.IsNullOrWhiteSpace(fileTypes))
                                    continue;
                                fileTypes = $"|.{fileTypes.RemoveChar('*', '.').Replace(",", "|.")}|"; // Sort various settings formats to a single format

                                // If file type settings found for a app, select this app as default
                                if (fileTypes.Contains($"|{t}|"))
                                {
                                    CmdLineApp = app;
                                    if (string.IsNullOrWhiteSpace(typeApp))
                                        typeApp = app;
                                }
                                if (!CmdLineMultipleApps && !string.IsNullOrWhiteSpace(CmdLineApp) && !string.IsNullOrWhiteSpace(typeApp) && CmdLineApp != typeApp)
                                {
                                    CmdLineMultipleApps = true;
                                    break;
                                }
                            }
                        }

                        // If multiple file types with different app settings found, select the app with most listed file types
                        if (CmdLineMultipleApps)
                        {
                            string a = string.Empty;
                            var q = types.GroupBy(x => x).Select(g => new { Value = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count);
                            int c = 0;
                            foreach (var x in q)
                            {
                                if (x.Count > c)
                                    a = x.Value;
                                c = x.Count;
                            }
                            if (!string.IsNullOrWhiteSpace(a))
                            {
                                foreach (string app in AppConfigs)
                                {
                                    string fileTypes = INI.Read(app, "FileTypes");
                                    if (string.IsNullOrWhiteSpace(fileTypes))
                                        continue;
                                    fileTypes = $".{fileTypes.RemoveChar('*', '.').Replace(",", "|.")}"; // Filter
                                    if (fileTypes.Contains(a))
                                    {
                                        CmdLineApp = app;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        #endregion

        #region APP FUNCTIONS

        internal static string AppsDir { get; } = PATH.Combine("%CurDir%\\Apps");

        internal static string[] AppDirs { get; set; } = new string[]
        {
            AppsDir,
            Path.Combine(AppsDir, ".free"),
            Path.Combine(AppsDir, ".repack"),
            Path.Combine(AppsDir, ".share")
        };

        internal static void SetAppDirs()
        {
            string dirs = INI.Read("Settings", "AppDirs");
            if (!string.IsNullOrWhiteSpace(dirs))
            {
                dirs = dirs.DecodeStringFromBase64();
                if (!string.IsNullOrWhiteSpace(dirs))
                {
                    if (!dirs.Contains(Environment.NewLine))
                        dirs += Environment.NewLine;
                    AppDirs = AppDirs.Concat(dirs.SplitNewLine()).Where(s => Directory.Exists(PATH.Combine(s))).ToArray();
                }
            }
        }

        internal static List<AppInfo> AppsInfo = new List<AppInfo>();

        internal static AppInfo GetAppInfo(string appName)
        {
            if (AppsInfo.Count > 0)
            {
                foreach (AppInfo appInfo in AppsInfo)
                {
                    if (appInfo.LongName == appName ||
                        appInfo.ShortName == appName)
                        return appInfo;
                }
            }
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
                if (_appConfigs.Count == 0)
                {
                    if (AppsInfo.Count == 0)
                        CheckAvailableApps();
                    _appConfigs = INI.GetSections(INI.File(), false).Where(s => s.ToLower() != "history" && s.ToLower() != "settings" && s.ToLower() != "host").ToList();
                }
                return _appConfigs;
            }
            set { _appConfigs = value; }
        }

        internal static void CheckAvailableApps(bool force = true)
        {
            if (!force && AppsInfo.Count > 0)
                return;
            AppsInfo.Clear();
            foreach (string d in AppDirs)
            {
                try
                {
                    string dir = PATH.Combine(d);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        continue;
                    }
                    foreach (string path in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).Where(s => s.Contains("Portable")))
                    {
                        string dirName = Path.GetFileName(path);

                        // If there is no exe file with the same name like the directory, search in config files for the correct start file. 
                        // This step is required for multiple exe files.
                        string exePath = Path.Combine(dir, $"{dirName}\\{dirName}.exe");
                        string iniPath = exePath.Replace(".exe", ".ini");
                        string nfoPath = Path.Combine(path, "App\\AppInfo\\appinfo.ini");
                        if (!File.Exists(exePath))
                        {
                            string appFile = INI.Read("AppInfo", "File", iniPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                appFile = INI.Read("Control", "Start", nfoPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                continue;
                            string appDir = INI.Read("AppInfo", "Dir", iniPath);
                            if (string.IsNullOrWhiteSpace(appDir))
                                exePath = exePath.Replace($"{dirName}.exe", appFile);
                            else
                            {
                                foreach (string curDirEnvVar in new string[] { "%CurrentDir%\\", "%CurDir%\\" })
                                {
                                    if (appDir.StartsWith(curDirEnvVar, StringComparison.OrdinalIgnoreCase))
                                        appDir = Path.Combine(Path.GetDirectoryName(iniPath), appDir.Substring(curDirEnvVar.Length));
                                }
                                appDir = PATH.Combine(appDir);
                                exePath = Path.Combine(appDir, appFile);
                            }
                        }
                        if (!File.Exists(exePath))
                            continue;
                        if (!File.Exists(iniPath))
                            iniPath = exePath.Replace(".exe", ".ini");

                        // Try to get the full app name
                        string appName = appName = INI.Read("AppInfo", "Name", iniPath);
                        if (string.IsNullOrWhiteSpace(appName))
                            appName = INI.Read("Details", "Name", nfoPath);
                        if (string.IsNullOrWhiteSpace(appName))
                            appName = FileVersionInfo.GetVersionInfo(exePath).FileDescription;
                        if (string.IsNullOrWhiteSpace(appName))
                            continue;

                        // Apply some filters for the found app name
                        if (!appName.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase)) // No filters needed for portable Java® runtime environment because it is not listed
                        {
                            string tmp = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(appName, string.Empty);
                            tmp = Regex.Replace(tmp, @"\s+", " ");
                            if (tmp != appName)
                                appName = tmp;
                        }
                        appName = appName.Trim().TrimEnd(',');

                        if (string.IsNullOrWhiteSpace(appName) || !File.Exists(exePath))
                            continue;
                        if (AppsInfo.Count(x => x.LongName == appName) == 0)
                        {
                            AppsInfo.Add(new AppInfo()
                            {
                                LongName = appName,
                                ShortName = dirName,
                                ExePath = exePath,
                                IniPath = iniPath,
                                NfoPath = nfoPath
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            if (AppsInfo.Count == 0)
            {
                if (force)
                    CheckAvailableApps(false);
                else
                {
                    RUN.App(new ProcessStartInfo()
                    {
#if x86
                        FileName = "%CurDir%\\Binaries\\AppsDownloader.exe"
#else
                        FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe"
#endif
                    }, 0);
                    Environment.Exit(Environment.ExitCode);
                    return;
                }
                return;
            }
            AppsInfo = AppsInfo.OrderBy(x => x.LongName, new AscendentAlphanumericStringComparer()).ToList();
        }

        internal static string GetAppPath(string appName)
        {
            AppInfo appInfo = GetAppInfo(appName);
            if (appInfo.LongName != appName &&
                appInfo.ShortName != appName)
                return null;
            return appInfo.ExePath;
        }

        internal static void OpenAppLocation(string appName, bool closeLancher = false)
        {
            try
            {
                string dir = Path.GetDirectoryName(GetAppPath(appName));
                if (!Directory.Exists(dir))
                    throw new DirectoryNotFoundException();
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = dir,
                    FileName = "%WinDir%\\explorer.exe"
                });
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            if (closeLancher)
                Application.Exit();
        }

        internal static void StartApp(string appName, bool closeLauncher = false, bool runAsAdmin = false)
        {
            try
            {
                AppInfo appInfo = GetAppInfo(appName);
                if (appInfo.LongName != appName &&
                    appInfo.ShortName != appName)
                    throw new ArgumentNullException();

                INI.Write("History", "LastItem", appInfo.LongName);
                string exeDir = Path.GetDirectoryName(appInfo.ExePath);
                string exeName = Path.GetFileName(appInfo.ExePath);
                string iniName = Path.GetFileName(appInfo.IniPath);
                if (!runAsAdmin)
                    runAsAdmin = INI.ReadBoolean(appInfo.ShortName, "RunAsAdmin", false);
                if (Directory.Exists(exeDir))
                {
                    string source = Path.Combine(exeDir, "Other\\Source\\AppNamePortable.ini");
                    if (!File.Exists(source))
                        source = Path.Combine(exeDir, $"Other\\Source\\{iniName}");
                    if (!File.Exists(appInfo.IniPath) && File.Exists(source))
                        File.Copy(source, appInfo.IniPath);
                    foreach (string file in Directory.GetFiles(exeDir, "*.ini", SearchOption.TopDirectoryOnly))
                    {
                        string content = File.ReadAllText(file);
                        if (Regex.IsMatch(content, "DisableSplashScreen.*=.*false", RegexOptions.IgnoreCase))
                        {
                            content = Regex.Replace(content, "DisableSplashScreen.*=.*false", "DisableSplashScreen=true", RegexOptions.IgnoreCase);
                            File.WriteAllText(file, content);
                        }
                    }
                    string cmdLine = INI.Read("AppInfo", "Arg", appInfo.IniPath);
                    if (string.IsNullOrWhiteSpace(cmdLine) && !string.IsNullOrWhiteSpace(CmdLine))
                    {
                        string startArgsFirst = INI.Read(appInfo.ShortName, "StartArgs.First");
                        string argDecode = startArgsFirst.DecodeStringFromBase64();
                        if (!string.IsNullOrEmpty(argDecode))
                            startArgsFirst = argDecode;
                        string startArgsLast = INI.Read(appInfo.ShortName, "StartArgs.Last");
                        argDecode = startArgsLast.DecodeStringFromBase64();
                        if (!string.IsNullOrEmpty(argDecode))
                            startArgsLast = argDecode;
                        cmdLine = $"{startArgsFirst}{CmdLine}{startArgsLast}";
                    }
                    RUN.App(new ProcessStartInfo() { Arguments = cmdLine, FileName = appInfo.ExePath, Verb = runAsAdmin ? "runas" : string.Empty });
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            if (closeLauncher)
                Application.Exit();
        }

        #endregion

        #region FILE TYPE ASSOCIATION

        internal static void AssociateFileTypes(string appName)
        {
            string types = INI.Read(appName, "FileTypes");

            if (string.IsNullOrWhiteSpace(types))
            {
                MSGBOX.Show(Lang.GetText("associateBtnMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ELEVATION.IsAdministrator)
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"{ActionGuid.FileTypeAssociation} \"{appName}\"",
                    FileName = LOG.AssemblyPath,
                    Verb = "runas"
                }, 0);
                return;
            }

            string iconData = null;
            using (Form dialog = new RESOURCE.IconBrowserDialog(SystemResourcePath, Colors.BaseDark, Colors.ControlText, Colors.Button, Colors.ButtonText, Colors.ButtonHover))
            {
                dialog.TopMost = true;
                dialog.AddLoadingTimeStopwatch();
                dialog.ShowDialog();
                if (dialog.Text.Count(c => c == ',') == 1)
                    iconData = dialog.Text;
            }
            if (string.IsNullOrWhiteSpace(iconData))
            {
                MSGBOX.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string iconPath = iconData.Split(',')[0];
            iconData = iconData.Replace(iconData, GetEnvironmentVariablePath(iconPath));

            string appPath = LOG.AssemblyPath;
            MSGBOX.ButtonText.OverrideEnabled = true;
            MSGBOX.ButtonText.Yes = "App";
            MSGBOX.ButtonText.No = "Launcher";
            MSGBOX.ButtonText.Cancel = Lang.GetText("Cancel");
            DialogResult result = MSGBOX.Show(Lang.GetText("associateAppWayQuestion"), string.Empty, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                MSGBOX.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (result == DialogResult.Yes)
                appPath = GetAppPath(appName);

            if (!File.Exists(appPath))
            {
                MSGBOX.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string restPointDir = PATH.Combine("%CurDir%\\Restoration"); ;
            try
            {
                if (!Directory.Exists(restPointDir))
                {
                    Directory.CreateDirectory(restPointDir);
                    DATA.SetAttributes(restPointDir, FileAttributes.ReadOnly | FileAttributes.Hidden);
                    string iniPath = Path.Combine(restPointDir, "desktop.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
                    DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }

            restPointDir = Path.Combine(restPointDir, Environment.MachineName, WindowsInstallDateTime.ToString().EncryptToMD5().Substring(24), appName, "FileAssociation", DateTime.Now.ToString("yy-MM-dd"));
            int backupCount = 0;
            if (Directory.Exists(restPointDir))
                backupCount = Directory.GetFiles(restPointDir, "*.ini", SearchOption.TopDirectoryOnly).Length;
            else
            {
                try
                {
                    Directory.CreateDirectory(restPointDir);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }

            string restPointCfgPath = Path.Combine(restPointDir, $"{backupCount}.ini");
            if (!File.Exists(restPointCfgPath))
                File.Create(restPointCfgPath).Close();
            restPointDir = Path.Combine(restPointDir, backupCount.ToString());
            foreach (string type in (types.Contains(",") ? types : $"{types},").Split(','))
            {
                if (string.IsNullOrWhiteSpace(type) || type.StartsWith("."))
                    continue;

                if (REG.SubKeyExist($"HKCR\\.{type}"))
                {
                    string restKeyName = $"KeyBackup_.{type}_#####.reg";
                    int count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.TopDirectoryOnly).Length;
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(restPointDir);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    string restKeyPath = Path.Combine(restPointDir, restKeyName);
                    REG.ExportFile($"HKCR\\.{type}", restKeyPath);
                    if (File.Exists(restKeyPath))
                        INI.Write(CRYPT.MD5.EncryptString(type), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    INI.Write(CRYPT.MD5.EncryptString(type), "KeyAdded", $"HKCR\\.{type}", restPointCfgPath);

                string TypeKey = $"PortableAppsSuite_{type}file";
                if (REG.SubKeyExist($"HKCR\\{TypeKey}"))
                {
                    string restKeyName = $"KeyBackup_{TypeKey}_#####.reg";
                    int count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.AllDirectories).Length;
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    string restKeyPath = Path.Combine(restPointDir, restKeyName);
                    REG.ExportFile($"HKCR\\{TypeKey}", restKeyPath.Replace("#####", count.ToString()));
                    if (File.Exists(restKeyPath))
                        INI.Write(TypeKey.EncryptToMD5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    INI.Write(TypeKey.EncryptToMD5(), "KeyAdded", $"HKCR\\{TypeKey}", restPointCfgPath);

                REG.WriteValue(REG.RegKey.ClassesRoot, $".{type}", null, TypeKey, REG.RegValueKind.ExpandString);
                string IconRegEnt = REG.ReadValue(REG.RegKey.ClassesRoot, $"{TypeKey}\\DefaultIcon", null);
                if (IconRegEnt != iconData)
                    REG.WriteValue(REG.RegKey.ClassesRoot, $"{TypeKey}\\DefaultIcon", null, iconData, REG.RegValueKind.ExpandString);
                string OpenCmdRegEnt = REG.ReadValue(REG.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", null);
                string OpenCmd = $"\"{GetEnvironmentVariablePath(appPath)}\" \"%1\"";
                if (OpenCmdRegEnt != OpenCmd)
                    REG.WriteValue(REG.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", null, OpenCmd, REG.RegValueKind.ExpandString);
                REG.RemoveValue(REG.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", "DelegateExecute");
            }

            MSGBOX.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void RestoreFileTypes(string appName)
        {
            if (string.IsNullOrEmpty(appName))
                return;

            if (!ELEVATION.IsAdministrator)
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"{ActionGuid.RestoreFileTypes} \"{appName}\"",
                    FileName = LOG.AssemblyPath,
                    Verb = "runas"
                }, 0);
                return;
            }

            string restPointDir = PATH.Combine("%CurDir%\\Restoration", Environment.MachineName, Main.WindowsInstallDateTime.ToString().EncryptToMD5().Substring(24), appName, "FileAssociation");
            string restPointPath = string.Empty;
            using (OpenFileDialog dialog = new OpenFileDialog() { Filter = "INI Files(*.ini) | *.ini", InitialDirectory = restPointDir, Multiselect = false, RestoreDirectory = false })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    MSGBOX.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                restPointPath = dialog.FileName;
            }

            if (!File.Exists(restPointPath))
                return;
            foreach (string section in INI.GetSections(restPointPath))
            {
                try
                {
                    string val = INI.Read(section, "KeyBackup", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        val = INI.Read(section, "KeyAdded", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        throw new Exception($"No value found for '{section}'.");
                    if (val.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = Path.Combine(Path.GetDirectoryName(restPointPath), "val");
                        if (File.Exists(path))
                            REG.ImportFile(path);
                    }
                    else
                        REG.RemoveExistSubKey(val);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }

            try
            {
                File.Delete(restPointPath);
                string iniDir = Path.Combine(Path.GetDirectoryName(restPointPath));
                string iniSubDir = Path.Combine(iniDir, Path.GetFileNameWithoutExtension(restPointPath));
                if (Directory.Exists(iniSubDir))
                    Directory.Delete(iniSubDir, true);
                if (Directory.GetFiles(iniDir, "*.ini", SearchOption.TopDirectoryOnly).Length == 0)
                    Directory.Delete(Path.GetDirectoryName(restPointPath), true);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }

            MSGBOX.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region SYSTEM INTEGRATION

        internal static void SystemIntegration(bool enabled, bool response = true)
        {
            if (!ELEVATION.IsAdministrator)
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"{ActionGuid.SystemIntegration} {enabled}",
                    FileName = LOG.AssemblyPath,
                    Verb = "runas"
                }, 0);
                return;
            }

            try
            {
                string varKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment";
                string varble = "AppsSuiteDir";
                string varDir = REG.ReadValue(varKey, varble);
                string curDir = PATH.GetEnvironmentVariableValue("CurDir");

                if (!enabled || varDir.ToLower() != curDir.ToLower())
                {
                    if (enabled)
                        REG.WriteValue(varKey, varble, curDir);
                    else
                        REG.RemoveValue(varKey, varble);

                    if (WINAPI.SafeNativeMethods.SendNotifyMessage((IntPtr)0xffff, (uint)WINAPI.WindowMenuFunc.WM_SETTINGCHANGE, (UIntPtr)0, "Environment"))
                    {
                        foreach (string s in new string[] { "*", "Directory" })
                        {
                            varKey = $"HKCR\\{s}\\shell\\portableapps";
                            if (enabled)
                            {
                                REG.WriteValue(varKey, null, Lang.GetText("shellText"));
                                REG.WriteValue(varKey, "Icon", $"\"{LOG.AssemblyPath}\"");
                            }
                            else
                                REG.RemoveExistSubKey(varKey);

                            varKey = $"{varKey}\\command";
                            if (enabled)
                                REG.WriteValue(varKey, null, $"\"{LOG.AssemblyPath}\" \"%1\"");
                            else
                                REG.RemoveExistSubKey(varKey);
                        }

                        if (enabled)
                        {
                            if (DATA.PinToTaskbar(LOG.AssemblyPath))
                            {
                                string pinnedDir = PATH.Combine("%AppData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
                                foreach (string file in Directory.GetFiles(pinnedDir, "*.lnk", SearchOption.TopDirectoryOnly))
                                {
                                    if (DATA.GetShortcutTarget(file).ToLower() != LOG.AssemblyPath.ToLower())
                                        continue;
                                    RUN.Cmd($"DEL /F /Q \"{file}\"", 0);
                                    Environment.SetEnvironmentVariable(varble, curDir, EnvironmentVariableTarget.Process);
                                    DATA.CreateShortcut(GetEnvironmentVariablePath(LOG.AssemblyPath), file);
                                    break;
                                }
                            }
                        }
                        else
                            DATA.UnpinFromTaskbar(LOG.AssemblyPath);

                        if (response)
                            MessageBox.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }

            if (response)
                MessageBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static bool CheckEnvironmentVariable()
        {
            string appsSuiteDir = PATH.GetEnvironmentVariableValue("AppsSuiteDir");
            if (string.IsNullOrWhiteSpace(appsSuiteDir))
                return false;
            string curDir = PATH.GetEnvironmentVariableValue("CurDir");
            if (appsSuiteDir != curDir)
                SystemIntegration(true, false);
            appsSuiteDir = PATH.GetEnvironmentVariableValue("AppsSuiteDir");
            return appsSuiteDir == curDir;
        }

        internal static string GetEnvironmentVariablePath(string path)
        {
            try
            {
                if (!CheckEnvironmentVariable())
                    throw new ArgumentException();
                string s = path.Replace(PATH.GetEnvironmentVariableValue("CurDir"), "%AppsSuiteDir%");
                return s;
            }
            catch
            {
                return path;
            }
        }

        #endregion

        #region STARTMENU INTEGRATION

        internal static void StartMenuFolderUpdate(List<string> appList)
        {
            try
            {
                string startMenuDir = PATH.Combine("%StartMenu%\\Programs");
                string shortcutPath = Path.Combine(startMenuDir, $"Apps Launcher{(Environment.Is64BitProcess ? " (64-bit)" : string.Empty)}.lnk");
                if (Directory.Exists(startMenuDir))
                {
                    string[] shortcuts = Directory.GetFiles(startMenuDir, "Apps Launcher*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (string shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(startMenuDir))
                    Directory.CreateDirectory(startMenuDir);
                DATA.CreateShortcut(GetEnvironmentVariablePath(LOG.AssemblyPath), shortcutPath);
                startMenuDir = Path.Combine(startMenuDir, "Portable Apps");
                if (Directory.Exists(startMenuDir))
                {
                    string[] shortcuts = Directory.GetFiles(startMenuDir, "*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (string shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(startMenuDir))
                    Directory.CreateDirectory(startMenuDir);
                List<Thread> ThreadList = new List<Thread>();
                foreach (string app in appList)
                {
                    if (app.ToLower().Contains("portable"))
                        continue;
                    string tmp = app;
                    Thread newThread = new Thread(() => DATA.CreateShortcut(GetEnvironmentVariablePath(GetAppPath(tmp)), Path.Combine(startMenuDir, tmp)));
                    newThread.Start();
                    ThreadList.Add(newThread);
                }
                foreach (Thread thread in ThreadList)
                    while (thread.IsAlive) ;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        #endregion

        #region REPAIRING

        internal static void RepairAppsSuiteDirs()
        {
            try
            {
                List<string> dirList = AppDirs.ToList();
                dirList.Add(PATH.Combine("%CurDir%\\Documents"));
                dirList.Add(PATH.Combine("%CurDir%\\Documents\\Documents"));
                dirList.Add(PATH.Combine("%CurDir%\\Documents\\Music"));
                dirList.Add(PATH.Combine("%CurDir%\\Documents\\Pictures"));
                dirList.Add(PATH.Combine("%CurDir%\\Documents\\Videos"));
                foreach (string dir in dirList)
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    DATA.SetAttributes(dir, FileAttributes.ReadOnly);
                }
                RepairDesktopIniFiles();
            }
            catch (Exception ex)
            {
                if (!ELEVATION.IsAdministrator)
                    ELEVATION.RestartAsAdministrator();
                LOG.Debug(ex);
            }
        }

        private static void RepairDesktopIniFiles()
        {
            string iniPath = Path.Combine(AppDirs[0], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.blue.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = Path.Combine(AppDirs[1], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Freeware", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.green.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = Path.Combine(AppDirs[2], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "\"PortableApps.com\" - Repacks", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.pink.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = Path.Combine(AppDirs[3], "desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "\"Si13n7.com\" - Shareware", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "..\\..\\Assets\\win10.folder.red.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Assets\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "IconResource", "win10.folder.gray.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Binaries\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.gray.ico,0", iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Documents\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "Profile", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            INI.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            INI.Write(".ShellClassInfo", "IconIndex", -235, iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Documents\\Documents\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21770", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,117", iniPath);
            INI.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            INI.Write(".ShellClassInfo", "IconIndex", -235, iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Documents\\Music\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21790", iniPath);
            INI.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12689", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,103", iniPath);
            INI.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            INI.Write(".ShellClassInfo", "IconIndex", -237, iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Documents\\Pictures\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21779", iniPath);
            INI.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12688", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,108", iniPath);
            INI.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            INI.Write(".ShellClassInfo", "IconIndex", -236, iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine("%CurDir%\\Documents\\Videos\\desktop.ini");
            if (!File.Exists(iniPath))
                File.Create(iniPath).Close();
            INI.Write(".ShellClassInfo", "LocalizedResourceName", "@%SystemRoot%\\system32\\shell32.dll,-21791", iniPath);
            INI.Write(".ShellClassInfo", "InfoTip", "@%SystemRoot%\\system32\\shell32.dll,-12690", iniPath);
            INI.Write(".ShellClassInfo", "IconResource", "%SystemRoot%\\system32\\imageres.dll,178", iniPath);
            INI.Write(".ShellClassInfo", "IconFile", "%SystemRoot%\\system32\\shell32.dll", iniPath);
            INI.Write(".ShellClassInfo", "IconIndex", -238, iniPath);
            DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);

            iniPath = PATH.Combine($"%CurDir%\\Help\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                DATA.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.green.ico,0", iniPath);
                DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }

            iniPath = PATH.Combine($"%CurDir%\\Langs\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                DATA.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.gray.ico,0", iniPath);
                DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }

            iniPath = PATH.Combine("%CurDir%\\Restoration\\desktop.ini");
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
            {
                DATA.SetAttributes(Path.GetDirectoryName(iniPath), FileAttributes.ReadOnly | FileAttributes.Hidden);
                if (!File.Exists(iniPath))
                    File.Create(iniPath).Close();
                INI.Write(".ShellClassInfo", "IconResource", "..\\Assets\\win10.folder.red.ico,0", iniPath);
                DATA.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
            }
        }

        #endregion

        #region MISC FUNCTIONS

        internal static readonly string TmpDir = PATH.Combine("%CurDir%\\Documents\\.cache");

        internal static int ScreenDpi
        {
            get
            {
                int dpi;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    dpi = (int)Math.Ceiling(g.DpiX);
                return dpi;
            }
        }

        internal static DateTime WindowsInstallDateTime
        {
            get
            {
                object InstallDateRegValue = REG.ReadObjValue(REG.RegKey.LocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate", REG.RegValueKind.DWord);
                object InstallTimeRegValue = REG.ReadObjValue(REG.RegKey.LocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallTime", REG.RegValueKind.DWord);
                DateTime InstallDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
                try
                {
                    InstallDateTime = InstallDateTime.AddSeconds((int)InstallDateRegValue);
                    InstallDateTime = InstallDateTime.AddSeconds((int)InstallTimeRegValue);
                }
                catch (InvalidCastException) { }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
                return InstallDateTime;
            }
        }

        internal static string FileVersion(string path)
        {
            try
            {
                path = PATH.Combine(path);
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                return fvi.ProductVersion;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        internal static string CurrentFileVersion =>
            FileVersion(LOG.AssemblyPath);

        internal static string SearchMatchItem(string search, List<string> items)
        {
            try
            {
                string[] split = null;
                if (search.Contains("*") && !search.StartsWith("*") && !search.EndsWith("*"))
                    split = search.Split('*');
                bool match = false;
                for (int i = 0; i < 2; i++)
                {
                    foreach (string item in items)
                    {
                        if (i < 1 && split != null && split.Length == 2)
                        {
                            Regex regex = new Regex($".*{split[0]}(.*){split[1]}.*", RegexOptions.IgnoreCase);
                            match = regex.IsMatch(item);
                        }
                        else
                        {
                            match = item.StartsWith(search, StringComparison.OrdinalIgnoreCase);
                            if (i > 0 && !match)
                                match = item.ToLower().Contains(search.ToLower());
                        }
                        if (match)
                            return item;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return string.Empty;
        }

        #endregion
    }
}
