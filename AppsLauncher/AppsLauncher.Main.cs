using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    public static class Main
    {
        public static DateTime WindowsInstallDateTime
        {
            get
            {
                object InstallDateRegValue = SilDev.Reg.ReadObjValue(SilDev.Reg.RegKey.LocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate", SilDev.Reg.RegValueKind.DWord);
                object InstallTimeRegValue = SilDev.Reg.ReadObjValue(SilDev.Reg.RegKey.LocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallTime", SilDev.Reg.RegValueKind.DWord);
                DateTime InstallDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
                try
                {
                    InstallDateTime = InstallDateTime.AddSeconds((int)InstallDateRegValue);
                    InstallDateTime = InstallDateTime.AddSeconds((int)InstallTimeRegValue);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
                return InstallDateTime;
            }
        }

        private static MemoryStream _layoutBgStream;
        private static Image _layoutBackground;
        public static Image LayoutBackground
        {
            get
            {
                if (_layoutBackground == null)
                    ReloadLayoutBackground();
                return _layoutBackground;
            }
            set { _layoutBackground = value; }
        }

        public static Image ReloadLayoutBackground()
        {
            _layoutBackground = Properties.Resources.diagonal_pattern;
            string bgDir = Path.Combine(Application.StartupPath, "Assets\\cache\\bg");
            if (Directory.Exists(bgDir))
            {
                foreach (string file in Directory.GetFiles(bgDir, "image.*", SearchOption.TopDirectoryOnly))
                {
                    if (_layoutBgStream != null)
                        _layoutBgStream.Close();
                    try
                    {
                        _layoutBgStream = new MemoryStream(File.ReadAllBytes(file));
                        Image imgFromStream = Image.FromStream(_layoutBgStream);
                        _layoutBackground = imgFromStream;
                        break;
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
            return _layoutBackground;
        }

        [StructLayout(LayoutKind.Sequential)]
        public static class Colors
        {
            public static Color Layout = SystemColors.Highlight;
            public static Color Control = SystemColors.Control;
            public static Color ControlText = SystemColors.ControlText;
            public static Color Button = SystemColors.ControlDark;
            public static Color ButtonHover = SystemColors.Highlight;
            public static Color ButtonText = SystemColors.ControlText;
        }

        public static string CurrentVersion => 
            FileVersionInfo.GetVersionInfo(Application.ExecutablePath).ProductVersion;

        public static bool EnableLUA => 
            SilDev.Reg.ReadValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "EnableLUA") == "1";

        private static List<string> _cmdLineArray = new List<string>() { "92AE658C-42C4-4976-82D7-C1FD5A47B78E" };
        public static List<string> CmdLineArray
        {
            get
            {
                if (_cmdLineArray.Contains("92AE658C-42C4-4976-82D7-C1FD5A47B78E"))
                {
                    _cmdLineArray.Clear();
                    foreach (string arg in Environment.GetCommandLineArgs())
                    {
                        int i = 0;
                        if (arg.ToLower() == Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName).ToLower() || arg.ToLower().Contains("/debug") || int.TryParse(arg, out i))
                            continue;
                        _cmdLineArray.Add(arg);
                    }
                }
                _cmdLineArray.Sort();
                return _cmdLineArray;
            }
            set
            {
                if (!_cmdLineArray.Contains(value.ToString()))
                    _cmdLineArray.Add(value.ToString());
            }
        }

        private static string _cmdLine = string.Empty;
        public static string CmdLine
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cmdLine) && CmdLineArray.Count > 0)
                    return $"\"{string.Join("\" \"", CmdLineArray)}\"";
                return _cmdLine;
            }
            set { _cmdLine = value; }
        }

        public static string CmdLineApp { get; set; }

        public static bool CmdLineMultipleApps { get; private set; } = false;

        public readonly static string AppsPath = Path.Combine(Application.StartupPath, "Apps");

        public static string[] AppDirs = new string[]
        {
            AppsPath,
            Path.Combine(AppsPath, ".free"),
            Path.Combine(AppsPath, ".repack"),
            Path.Combine(AppsPath, ".share")
        };

        public static void SetAppDirs()
        {
            string dirs = SilDev.Initialization.ReadValue("Settings", "AppDirs");
            if (!string.IsNullOrWhiteSpace(dirs))
            {
                dirs = SilDev.Crypt.Base64.Decrypt(dirs);
                if (!string.IsNullOrWhiteSpace(dirs))
                {
                    if (!dirs.Contains(Environment.NewLine))
                        dirs += Environment.NewLine;
                    AppDirs = AppDirs.Concat(dirs.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)).Where(c => Directory.Exists(SilDev.Run.EnvironmentVariableFilter(c))).ToArray();
                }
            }
        }

        /// <summary>Note that AppsDict["FULL APP NAME"] outputs only the app directory name.</summary>
        public static Dictionary<string, string> AppsDict { get; private set; } = new Dictionary<string, string>();

        /// <summary>Note that the full app name is listed.</summary>
        public static List<string> AppsList { get; private set; } = new List<string>();

        private static List<string> _appConfigs = new List<string>();
        public static List<string> AppConfigs
        {
            get
            {
                if (_appConfigs.Count == 0)
                {
                    if (AppsDict.Count == 0)
                        CheckAvailableApps();
                    _appConfigs = SilDev.Initialization.GetSections(SilDev.Initialization.File(), false).Where(s => AppsDict.ContainsValue(s) && s != "History" && s != "Settings" && s != "Host").ToList();
                }
                return _appConfigs;
            }
            set { _appConfigs = value; }
        }

        public static bool IsBetween<T>(this T item, T start, T end) where T : IComparable, IComparable<T> => 
            Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;

        public static void CheckUpdates()
        {
            try
            {
                int i = 0;
                if (!int.TryParse(SilDev.Initialization.ReadValue("Settings", "UpdateCheck"), out i))
                    i = 4;
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
                    string LastCheck = SilDev.Initialization.ReadValue("History", "LastUpdateCheck");
                    string CheckTime = (IsBetween(i, 7, 9) ? DateTime.Now.Month : IsBetween(i, 4, 6) ? DateTime.Now.DayOfYear : DateTime.Now.Hour).ToString();
                    if (LastCheck != CheckTime)
                    {
                        if (i != 2 && i != 5 && i != 8)
                            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\Updater.exe") });
                        if (i != 3 && i != 6 && i != 9)
                            SilDev.Run.App(new ProcessStartInfo()
                            {
                                Arguments = "7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e",
#if x86
                                FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe")
#else
                                FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader64.exe")
#endif
                            });
                    }
                    SilDev.Initialization.WriteValue("History", "LastUpdateCheck", CheckTime);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        public static string SearchMatchItem(string search, List<string> items)
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
                SilDev.Log.Debug(ex);
            }
            return string.Empty;
        }

        public static void CheckCmdLineApp()
        {
            if (string.IsNullOrWhiteSpace(CmdLine))
                return;
            try
            {
                if (Environment.GetCommandLineArgs().Length >= 2)
                {
                    List<string> types = new List<string>();
                    foreach (string arg in Environment.GetCommandLineArgs())
                    {
                        int number;
                        if (arg.ToLower().Contains(Application.ExecutablePath.ToLower()) || arg.ToLower().Contains("/debug") || int.TryParse(arg, out number))
                            continue;

                        // List all file types from a added directory
                        if (SilDev.Data.IsDir(arg))
                        {
                            if (Directory.Exists(arg))
                            {
                                foreach (string file in Directory.GetFiles(arg, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower() != "desktop.ini"))
                                {
                                    if (!SilDev.Data.MatchAttributes(file, FileAttributes.Hidden))
                                        types.Add(Path.GetExtension(file).ToLower());
                                    if (types.Count >= 768) // Maximum size to speed up this task
                                        break;
                                }
                            }
                            continue;
                        }

                        if (File.Exists(arg))
                            if (!SilDev.Data.MatchAttributes(arg, FileAttributes.Hidden))
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
                                string fileTypes = SilDev.Initialization.ReadValue(app, "FileTypes");
                                if (string.IsNullOrWhiteSpace(fileTypes))
                                    continue;
                                fileTypes = $"|.{fileTypes.Replace("*", string.Empty).Replace(".", string.Empty).Replace(",", "|.")}|"; // Sort various settings formats to a single format

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
                                    string fileTypes = SilDev.Initialization.ReadValue(app, "FileTypes");
                                    if (string.IsNullOrWhiteSpace(fileTypes))
                                        continue;
                                    fileTypes = $".{fileTypes.Replace("*", string.Empty).Replace(".", string.Empty).Replace(",", "|.")}";
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
                SilDev.Log.Debug(ex);
            }
        }

        public static void CheckAvailableApps()
        {
            AppsDict.Clear();
            foreach (string d in AppDirs)
            {
                try
                {
                    string dir = SilDev.Run.EnvironmentVariableFilter(d);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        continue;
                    }
                    foreach (string path in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).Where(s => s.Contains("Portable")))
                    {
                        string dirName = Path.GetFileName(path);
                        string exePath = Path.Combine(dir, $"{dirName}\\{dirName}.exe");
                        string iniPath = exePath.Replace(".exe", ".ini");
                        string appName = string.Empty;
                        string infoIniPath = Path.Combine(path, "App\\AppInfo\\appinfo.ini");

                        // If there is no exe file with the same name like the directory, search in config files for the correct start file. This step is required for multiple exe files.
                        if (!File.Exists(exePath))
                        {
                            string appFile = SilDev.Initialization.ReadValue("AppInfo", "File", iniPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                appFile = SilDev.Initialization.ReadValue("Control", "Start", infoIniPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                continue;
                            string appDir = SilDev.Initialization.ReadValue("AppInfo", "Dir", iniPath);
                            if (!string.IsNullOrWhiteSpace(appDir))
                            {
                                string curDirEnvVar = "%CurrentDir%\\";
                                if (appDir.StartsWith(curDirEnvVar, StringComparison.OrdinalIgnoreCase))
                                    appDir = Path.Combine(Path.GetDirectoryName(iniPath), appDir.Substring(curDirEnvVar.Length));
                                appDir = SilDev.Run.EnvironmentVariableFilter(appDir);
                                exePath = Path.Combine(appDir, appFile);
                            }
                            else
                                exePath = exePath.Replace($"{dirName}.exe", appFile);
                        }

                        // Try to get the full app name
                        appName = SilDev.Initialization.ReadValue("AppInfo", "Name", iniPath);
                        if (string.IsNullOrWhiteSpace(appName))
                            appName = SilDev.Initialization.ReadValue("Details", "Name", infoIniPath);
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
                        appName = appName.TrimStart().TrimEnd();

                        if (!File.Exists(exePath) || string.IsNullOrWhiteSpace(appName))
                            continue;
                        if (!AppsDict.Keys.Contains(appName))
                            AppsDict.Add(appName, dirName);
                    }
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            if (AppsDict.Count <= 0)
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe") }, 0);
                if (Directory.GetDirectories(AppsPath, "*Portable", SearchOption.AllDirectories).Length > 0)
                    SilDev.Run.App(new ProcessStartInfo() { FileName = Application.ExecutablePath });
                Environment.Exit(Environment.ExitCode);
            }
            AppsList.Clear();
            AppsList = AppsDict.Keys.ToList();
            AppsList.Sort();
        }

        public static string GetAppPath(string _app)
        {
            foreach (string d in AppDirs)
            {
                try
                {
                    string dir = SilDev.Run.EnvironmentVariableFilter(d);
                    if (!Directory.Exists(dir))
                        continue;
                    string path = Path.Combine(dir, _app);
                    if (Directory.Exists(path))
                    {
                        string dirName = Path.GetFileName(path);
                        string exePath = Path.Combine(dir, $"{dirName}\\{dirName}.exe");
                        string iniPath = exePath.Replace(".exe", ".ini");
                        string infoIniPath = Path.Combine(path, "App\\AppInfo\\appinfo.ini");
                        if (!File.Exists(exePath))
                        {
                            string appFile = SilDev.Initialization.ReadValue("AppInfo", "File", iniPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                appFile = SilDev.Initialization.ReadValue("Control", "Start", infoIniPath);
                            if (string.IsNullOrWhiteSpace(appFile))
                                continue;
                            string appDir = SilDev.Initialization.ReadValue("AppInfo", "Dir", iniPath);
                            if (!string.IsNullOrWhiteSpace(appDir))
                            {
                                string curDirEnvVar = "%CurrentDir%\\";
                                if (appDir.StartsWith(curDirEnvVar, StringComparison.OrdinalIgnoreCase))
                                    appDir = Path.Combine(Path.GetDirectoryName(iniPath), appDir.Substring(curDirEnvVar.Length));
                                appDir = SilDev.Run.EnvironmentVariableFilter(appDir);
                                exePath = Path.Combine(appDir, appFile);
                            }
                            else
                                exePath = exePath.Replace($"{dirName}.exe", appFile);
                        }
                        return File.Exists(exePath) ? exePath : null;
                    }
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            return null;
        }

        public static void OpenAppLocation(string _app, bool _close)
        {
            try
            {
                SilDev.Run.App(new ProcessStartInfo() { Arguments = Path.GetDirectoryName(GetAppPath(AppsDict[_app])), FileName = "%WinDir%\\explorer.exe" });
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            if (_close)
                Application.Exit();
        }

        public static void OpenAppLocation(string _app) =>
            OpenAppLocation(_app, false);

        public static void StartApp(string _app, bool _close, bool _admin)
        {
            try
            {
                SilDev.Initialization.WriteValue("History", "LastItem", _app);
                string exePath = GetAppPath(AppsDict[_app]);
                if (string.IsNullOrWhiteSpace(exePath))
                    throw new Exception("'exePath' is not defined.");
                string exeDir = Path.GetDirectoryName(exePath);
                string exeName = Path.GetFileName(exePath);
                string iniName = $"{AppsDict[_app]}.ini";
                if (!_admin)
                    bool.TryParse(SilDev.Initialization.ReadValue(AppsDict[_app], "RunAsAdmin"), out _admin);
                if (Directory.Exists(exeDir))
                {
                    string source = Path.Combine(exeDir, "Other\\Source\\AppNamePortable.ini");
                    if (!File.Exists(source))
                        source = Path.Combine(exeDir, $"Other\\Source\\{iniName}");
                    string dest = Path.Combine(exeDir, iniName);
                    if (File.Exists(source) && !File.Exists(dest))
                        File.Copy(source, dest);
                    foreach (string file in Directory.GetFiles(exeDir, "*.ini", SearchOption.TopDirectoryOnly))
                    {
                        string content = File.ReadAllText(file);
                        if (Regex.IsMatch(content, "DisableSplashScreen.*=.*false", RegexOptions.IgnoreCase))
                        {
                            content = Regex.Replace(content, "DisableSplashScreen.*=.*false", "DisableSplashScreen=true", RegexOptions.IgnoreCase);
                            File.WriteAllText(file, content);
                        }
                    }
                    string cmdLine = SilDev.Initialization.ReadValue("AppInfo", "Arg", Path.Combine(exeDir, iniName));
                    if (string.IsNullOrWhiteSpace(cmdLine) && !string.IsNullOrWhiteSpace(CmdLine))
                        cmdLine = $"{SilDev.Initialization.ReadValue(AppsDict[_app], "StartArg")}{CmdLine}{SilDev.Initialization.ReadValue(AppsDict[_app], "EndArg")}";
                    SilDev.Run.App(new ProcessStartInfo() { Arguments = cmdLine, FileName = Path.Combine(exeDir, exeName), Verb = _admin ? "runas" : string.Empty });
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            if (_close)
                Application.Exit();
        }

        public static void StartApp(string _app, bool _close) =>
            StartApp(_app, _close, false);

        public static void StartApp(string _app) =>
            StartApp(_app, false, false);

        public static void AssociateFileTypes(string _app)
        {
            string types = SilDev.Initialization.ReadValue(_app, "FileTypes");

            if (string.IsNullOrWhiteSpace(types))
            {
                SilDev.MsgBox.Show(Lang.GetText("associateBtnMsg1"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string icon = null;
            using (Form dialog = new IconBrowserForm())
            {
                dialog.TopMost = true;
                dialog.ShowDialog();
                if (dialog.Text.Contains(","))
                    icon = dialog.Text;
            }
            if (string.IsNullOrWhiteSpace(icon))
            {
                SilDev.MsgBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string app = Application.ExecutablePath;
            
            SilDev.MsgBox.ButtonText.OverrideEnabled = true;
            SilDev.MsgBox.ButtonText.Yes = "App";
            SilDev.MsgBox.ButtonText.No = "Launcher";
            SilDev.MsgBox.ButtonText.Cancel = Lang.GetText("Cancel");
            DialogResult result = SilDev.MsgBox.Show(Lang.GetText("associateAppWayQuestion"), string.Empty, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                SilDev.MsgBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (result == DialogResult.Yes)
                app = GetAppPath(_app);

            if (!File.Exists(app))
            {
                SilDev.MsgBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string restPointDir = Path.Combine(Application.StartupPath, "Restoration");
            try
            {
                if (!Directory.Exists(restPointDir))
                {
                    Directory.CreateDirectory(restPointDir);
                    File.WriteAllText(Path.Combine(restPointDir, "desktop.ini"), $"[.ShellClassInfo]{Environment.NewLine}IconResource =..\\Assets\\win10.folder.red.ico,0");
                    SilDev.Data.SetAttributes(Path.Combine(restPointDir, "desktop.ini"), FileAttributes.Hidden);
                    SilDev.Data.SetAttributes(restPointDir, FileAttributes.Hidden | FileAttributes.ReadOnly);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }

            restPointDir = Path.Combine(restPointDir, Environment.MachineName, SilDev.Crypt.MD5.Encrypt(WindowsInstallDateTime.ToString()).Substring(24), _app, "FileAssociation", DateTime.Now.ToString("yy-MM-dd"));
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
                    SilDev.Log.Debug(ex);
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

                if (SilDev.Reg.SubKeyExist($"HKCR\\.{type}"))
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
                            SilDev.Log.Debug(ex);
                        }
                    }
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    string restKeyPath = Path.Combine(restPointDir, restKeyName);
                    SilDev.Reg.ExportFile($"HKCR\\.{type}", restKeyPath);
                    if (File.Exists(restKeyPath))
                        SilDev.Initialization.WriteValue(SilDev.Crypt.MD5.Encrypt(type), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    SilDev.Initialization.WriteValue(SilDev.Crypt.MD5.Encrypt(type), "KeyAdded", $"HKCR\\.{type}", restPointCfgPath);

                string TypeKey = $"PortableAppsSuite_{type}file";
                if (SilDev.Reg.SubKeyExist($"HKCR\\{TypeKey}"))
                {
                    string restKeyName = $"KeyBackup_{TypeKey}_#####.reg";
                    int count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.AllDirectories).Length;
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    string restKeyPath = Path.Combine(restPointDir, restKeyName);
                    SilDev.Reg.ExportFile($"HKCR\\{TypeKey}", restKeyPath.Replace("#####", count.ToString()));
                    if (File.Exists(restKeyPath))
                        SilDev.Initialization.WriteValue(SilDev.Crypt.MD5.Encrypt(TypeKey), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                else
                    SilDev.Initialization.WriteValue(SilDev.Crypt.MD5.Encrypt(TypeKey), "KeyAdded", $"HKCR\\{TypeKey}", restPointCfgPath);

                SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, $".{type}", null, TypeKey, SilDev.Reg.RegValueKind.ExpandString);
                string IconRegEnt = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.ClassesRoot, $"{TypeKey}\\DefaultIcon", null);
                if (IconRegEnt != icon)
                    SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, $"{TypeKey}\\DefaultIcon", null, icon, SilDev.Reg.RegValueKind.ExpandString);
                string OpenCmdRegEnt = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", null);
                string OpenCmd = $"\"{app}\" \"%1\"";
                if (OpenCmdRegEnt != OpenCmd)
                    SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", null, OpenCmd, SilDev.Reg.RegValueKind.ExpandString);
                SilDev.Reg.RemoveValue(SilDev.Reg.RegKey.ClassesRoot, $"{TypeKey}\\shell\\open\\command", "DelegateExecute");
            }

            SilDev.MsgBox.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void UndoFileTypeAssociation(string _iniFile)
        {
            if (!File.Exists(_iniFile))
                return;
            List<string> sections = SilDev.Initialization.GetSections(_iniFile);
            foreach (string section in sections)
            {
                try
                {
                    string val = SilDev.Initialization.ReadValue(section, "KeyBackup", _iniFile);
                    if (string.IsNullOrWhiteSpace(val))
                        val = SilDev.Initialization.ReadValue(section, "KeyAdded", _iniFile);
                    if (string.IsNullOrWhiteSpace(val))
                        throw new Exception($"No value found for '{section}'.");
                    if (val.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = Path.Combine(Path.GetDirectoryName(_iniFile), "val");
                        if (File.Exists(path))
                            SilDev.Reg.ImportFile(path);
                    }
                    else
                        SilDev.Reg.RemoveExistSubKey(val);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            try
            {
                File.Delete(_iniFile);
                string iniDir = Path.Combine(Path.GetDirectoryName(_iniFile));
                string iniSubDir = Path.Combine(iniDir, Path.GetFileNameWithoutExtension(_iniFile));
                if (Directory.Exists(iniSubDir))
                    Directory.Delete(iniSubDir, true);
                if (Directory.GetFiles(iniDir, "*.ini", SearchOption.TopDirectoryOnly).Length == 0)
                    Directory.Delete(Path.GetDirectoryName(_iniFile), true);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            SilDev.MsgBox.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void StartMenuFolderUpdate(List<string> _appList)
        {
            try
            {
                string StartMenuFolderPath = SilDev.Run.EnvironmentVariableFilter("%StartMenu%\\Programs");
                string LauncherShortcutPath = Path.Combine(StartMenuFolderPath, string.Format("Apps Launcher{0}.lnk", Environment.Is64BitProcess ? " (64-bit)" : string.Empty));
                if (Directory.Exists(StartMenuFolderPath))
                {
                    string[] shortcuts = Directory.GetFiles(StartMenuFolderPath, "Apps Launcher*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (string shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(StartMenuFolderPath))
                    Directory.CreateDirectory(StartMenuFolderPath);
                SilDev.Data.CreateShortcut(Application.ExecutablePath, LauncherShortcutPath);
                StartMenuFolderPath = Path.Combine(StartMenuFolderPath, "Portable Apps");
                if (Directory.Exists(StartMenuFolderPath))
                {
                    string[] shortcuts = Directory.GetFiles(StartMenuFolderPath, "*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach (string shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(StartMenuFolderPath))
                    Directory.CreateDirectory(StartMenuFolderPath);
                List<Thread> ThreadList = new List<Thread>();
                foreach (string app in _appList)
                {
                    if (app.ToLower().Contains("portable"))
                        continue;
                    string tmp = app;
                    Thread newThread = new Thread(() => SilDev.Data.CreateShortcut(GetAppPath(AppsDict[tmp]), Path.Combine(StartMenuFolderPath, tmp)));
                    newThread.Start();
                    ThreadList.Add(newThread);
                }
                foreach (Thread thread in ThreadList)
                    while (thread.IsAlive) ;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        public static void RepairAppsLauncher()
        {
            try
            {
                foreach (string dir in AppDirs)
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                RepairDesktopIniFiles();
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                if (SilDev.Elevation.IsAdministrator)
                {
                    Environment.ExitCode = 3;
                    Environment.Exit(Environment.ExitCode);
                }
                else
                    SilDev.Elevation.RestartAsAdministrator();
            }
        }

        private static void RepairDesktopIniFiles()
        {
            RepairDesktopIniFile(Path.Combine(AppDirs[0], "desktop.ini"), $"[.ShellClassInfo]{Environment.NewLine}IconResource =..\\Assets\\win10.folder.blue.ico,0");
            RepairDesktopIniFile(Path.Combine(AppDirs[1], "desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=\"Si13n7.com\" - Freeware{0}IconResource=..\\..\\Assets\\win10.folder.green.ico,0", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(AppDirs[2], "desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=\"PortableApps.com\" - Repacks{0}IconResource=..\\..\\Assets\\win10.folder.pink.ico,0", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(AppDirs[3], "desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=\"Si13n7.com\" - Shareware{0}IconResource=..\\..\\Assets\\win10.folder.red.ico,0", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(Application.StartupPath, "Documents\\desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=@%SystemRoot%\\system32\\shell32.dll,-21813{0}IconResource=C:\\Windows\\system32\\imageres.dll,117{0}IconFile=%SystemRoot%\\system32\\shell32.dll{0}IconIndex=-235", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(Application.StartupPath, "Documents\\Documents\\desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=@%SystemRoot%\\system32\\shell32.dll,-21770{0}IconResource=C:\\Windows\\system32\\imageres.dll,107{0}IconFile=%SystemRoot%\\system32\\shell32.dll{0}IconIndex=-235", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(Application.StartupPath, "Documents\\Music\\desktop.ini"), string.Format("[.ShellClassInfo]{0}IconResource=C:\\Windows\\system32\\imageres.dll,103{0}LocalizedResourceName=@%SystemRoot%\\system32\\shell32.dll,-21790{0}InfoTip=@%SystemRoot%\\system32\\shell32.dll,-12689{0}IconFile=%SystemRoot%\\system32\\shell32.dll{0}IconIndex=-237", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(Application.StartupPath, "Documents\\Pictures\\desktop.ini"), string.Format("[.ShellClassInfo]{0}LocalizedResourceName=@%SystemRoot%\\system32\\shell32.dll,-21779{0}InfoTip=@%SystemRoot%\\system32\\shell32.dll,-12688{0}IconResource=C:\\Windows\\system32\\imageres.dll,108{0}IconFile=%SystemRoot%\\system32\\shell32.dll{0}IconIndex=-236", Environment.NewLine));
            RepairDesktopIniFile(Path.Combine(Application.StartupPath, "Documents\\Videos\\desktop.ini"), string.Format("[.ShellClassInfo]{0}IconResource=C:\\Windows\\system32\\imageres.dll,178{0}LocalizedResourceName=@%SystemRoot%\\system32\\shell32.dll,-21791{0}InfoTip=@%SystemRoot%\\system32\\shell32.dll,-12690{0}IconFile=%SystemRoot%\\system32\\shell32.dll{0}IconIndex=-238", Environment.NewLine));
        }

        private static void RepairDesktopIniFile(string _path, string _content)
        {
            File.WriteAllText(_path, _content);
            SilDev.Run.App(new ProcessStartInfo()
            {
                Arguments = string.Format("/C ATTRIB +H \"{0}\" && ATTRIB -HR \"{1}\" && ATTRIB +R \"{1}\"", _path, Path.GetDirectoryName(_path)),
                FileName = "%WinDir%\\System32\\cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        public static Color GetHtmlColor(string _code, Color _default) =>
            _code.StartsWith("#") && _code.Length == 7 ? ColorTranslator.FromHtml(_code) : _default;

        public static Bitmap GetImageFiltered(Image image, int width, int heigth, SmoothingMode quality)
        {
            Bitmap bmp = new Bitmap(width, heigth);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.CompositingMode = CompositingMode.SourceCopy;
                switch (quality)
                {
                    case SmoothingMode.AntiAlias:
                        gr.CompositingQuality = CompositingQuality.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.SmoothingMode = SmoothingMode.AntiAlias;
                        break;
                    case SmoothingMode.HighQuality:
                        gr.CompositingQuality = CompositingQuality.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        break;
                    case SmoothingMode.HighSpeed:
                        gr.CompositingQuality = CompositingQuality.HighSpeed;
                        gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                        gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                        gr.SmoothingMode = SmoothingMode.HighSpeed;
                        break;
                }
                using (ImageAttributes imgAttrib = new ImageAttributes())
                {
                    imgAttrib.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(image, new Rectangle(0, 0, width, heigth), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttrib);
                }
            }
            return bmp;
        }

        public static Bitmap GetImageFiltered(Image image, int width, int heigth) =>
            GetImageFiltered(image, width, heigth, SmoothingMode.HighQuality);

        public static Bitmap GetImageFiltered(Image image, SmoothingMode quality)
        {
            int[] size = new int[]
            {
                image.Width,
                image.Height
            };
            for (int i = 0; i < size.Length; i++)
            {
                if (size[i] > 2048)
                {
                    int percent = Convert.ToInt32(100f - size[i] * 2048);
                    size[i] = (int)(size[i] / 100f * percent);
                    size[i == 0 ? 1 : 0] = (int)(size[i == 0 ? 1 : 0] / 100f * percent);
                }
            }
            return GetImageFiltered(image, size[0], size[1], quality);
        }

        public static Bitmap GetImageFiltered(Image image) =>
            GetImageFiltered(image, SmoothingMode.HighQuality);

        public static Icon GetSmallIcon(string _file)
        {
            try
            {
                IntPtr[] _icons = new IntPtr[1];
                SilDev.WinAPI.SafeNativeMethods.ExtractIconEx(_file, 0, new IntPtr[1], _icons, 1);
                return Icon.FromHandle(_icons[0]);
            }
            catch
            {
                return null;
            }
        }
    }
}

