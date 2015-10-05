using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    public static class Main
    {
        public static string CurrentVersion
        {
            get
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
                return fvi.ProductVersion;
            }
        }

        public static bool EnableLUA
        {
            get
            {
                return SilDev.Reg.ReadValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA") == "1";
            }
        }

        private static string _cmdLine = Regex.Replace(Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty), "/debug [0-2]|/debug \"[0-2]\"", string.Empty).TrimStart();

        public static string CmdLine
        {
            get { return _cmdLine; }
            set { _cmdLine = value; }
        }

        private static string _cmdLineApp;

        public static string CmdLineApp
        {
            get { return _cmdLineApp; }
            set { _cmdLineApp = value; }
        }

        private static bool _cmdLineMultipleApps = false;

        public static bool CmdLineMultipleApps
        {
            get { return _cmdLineMultipleApps; }
            set { _cmdLineMultipleApps = value; }
        }

        private static string _appsPath = Path.Combine(Application.StartupPath, "Apps");

        public static string AppsPath
        {
            get { return _appsPath; }
            set { _appsPath = value; }
        }

        private static string[] _appDirs = new string[] 
        { 
            AppsPath, 
            Path.Combine(AppsPath, ".free"), 
            Path.Combine(AppsPath, ".repack"), 
            Path.Combine(AppsPath, ".share")
        };

        public static string[] AppDirs
        {
            get { return _appDirs; }
            set { _appDirs = value; }
        }

        private static Dictionary<string, string> _appsDict = new Dictionary<string,string>();

        public static Dictionary<string, string> AppsDict
        {
            get { return _appsDict; }
            set { _appsDict = value; }
        }

        private static List<string> _appsList = new List<string>();

        public static List<string> AppsList
        {
            get { return _appsList; }
            set { _appsList = value; }
        }

        public static bool IsBetween<T>(this T item, T start, T end) where T : IComparable, IComparable<T>
        {
            return Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;
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
                        if ((File.GetAttributes(arg) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (Directory.Exists(arg))
                            {
                                foreach (string file in Directory.GetFiles(arg, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower() != "desktop.ini"))
                                {
                                    if (new FileInfo(file).Attributes != FileAttributes.Hidden)
                                        types.Add(Path.GetExtension(file).ToLower());
                                    if (types.Count >= 768)
                                        break;
                                }
                            }
                            continue;
                        }
                        if (File.Exists(arg))
                            if (new FileInfo(arg).Attributes != FileAttributes.Hidden)
                                types.Add(Path.GetExtension(arg).ToLower());
                    }
                    if (types.Count > 0)
                    {
                        string FileTypeSettings = SilDev.Initialization.ReadValue("Settings", "Apps");
                        string typeApp = null;
                        foreach (string t in types)
                        {
                            foreach (string app in FileTypeSettings.Split(','))
                            {
                                string fileTypes = SilDev.Initialization.ReadValue(app, "FileTypes");
                                if (string.IsNullOrWhiteSpace(fileTypes))
                                    continue;
                                fileTypes = string.Format("|.{0}|", fileTypes.Replace("*", string.Empty).Replace(".", string.Empty).Replace(",", "|."));
                                if (fileTypes.Contains(string.Format("|{0}|", t)))
                                {
                                    CmdLineApp = app;
                                    if (string.IsNullOrWhiteSpace(typeApp))
                                        typeApp = app;
                                }
                                if (!CmdLineMultipleApps && !string.IsNullOrWhiteSpace(CmdLineApp) && !string.IsNullOrWhiteSpace(typeApp) && CmdLineApp != typeApp)
                                    CmdLineMultipleApps = true;
                            }
                        }
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
                                foreach (string app in FileTypeSettings.Split(','))
                                {
                                    string fileTypes = SilDev.Initialization.ReadValue(app, "FileTypes");
                                    if (string.IsNullOrWhiteSpace(fileTypes))
                                        continue;
                                    fileTypes = string.Format(".{0}", fileTypes.Replace("*", string.Empty).Replace(".", string.Empty).Replace(",", "|."));
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
                        string exePath = Path.Combine(dir, string.Format("{0}\\{0}.exe", dirName));
                        string iniPath = exePath.Replace(".exe", ".ini");
                        string appInfo = string.Empty;
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
                                appDir = SilDev.Run.EnvironmentVariableFilter(appDir);
                                exePath = Path.Combine(appDir, appFile);
                            }
                            else
                                exePath = exePath.Replace(string.Format("{0}.exe", dirName), appFile);
                        }
                        appInfo = SilDev.Initialization.ReadValue("AppInfo", "Name", iniPath);
                        if (string.IsNullOrWhiteSpace(appInfo))
                            appInfo = SilDev.Initialization.ReadValue("Details", "Name", infoIniPath);
                        if (string.IsNullOrWhiteSpace(appInfo))
                            appInfo = FileVersionInfo.GetVersionInfo(exePath).FileDescription;
                        if (string.IsNullOrWhiteSpace(appInfo))
                            continue;
                        if (!appInfo.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase))
                        {
                            string tmp = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(appInfo, string.Empty);
                            tmp = Regex.Replace(tmp, @"\s+", " ");
                            if (!string.IsNullOrWhiteSpace(tmp) && tmp != appInfo)
                                appInfo = tmp;
                        }
                        if (!File.Exists(exePath) || string.IsNullOrWhiteSpace(appInfo))
                            continue;
                        if (!AppsDict.Keys.Contains(appInfo))
                            AppsDict.Add(appInfo, dirName);
                    }
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            if (AppsDict.Count <= 0)
            {
                SilDev.Run.App(Application.StartupPath, "Binaries\\AppsDownloader.exe", 0);
                if (Directory.GetDirectories(AppsPath, "*Portable", SearchOption.AllDirectories).Length > 0)
                    SilDev.Run.App(Application.ExecutablePath);
                foreach (Form frm in Application.OpenForms)
                    frm.Close();
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
                        string exePath = Path.Combine(dir, string.Format("{0}\\{0}.exe", dirName));
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
                                appDir = SilDev.Run.EnvironmentVariableFilter(appDir);
                                exePath = Path.Combine(appDir, appFile);
                            }
                            else
                                exePath = exePath.Replace(string.Format("{0}.exe", dirName), appFile);
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

        public static void OpenAppLocation(string _app)
        {
            try
            {
                Process.Start(Path.GetDirectoryName(GetAppPath(AppsDict[_app])));
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

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
                string iniName = string.Format("{0}.ini", AppsDict[_app]);
                string cmdLineOverride = SilDev.Initialization.ReadValue("AppInfo", "Arg", Path.Combine(exeDir, iniName));
                if (!string.IsNullOrWhiteSpace(cmdLineOverride))
                    CmdLine = cmdLineOverride;
                else
                {
                    if (Environment.GetCommandLineArgs().Length > 1)
                        CmdLine = string.Format("{0}{1}{2}", SilDev.Initialization.ReadValue(exePath, "startArg"), CmdLine, SilDev.Initialization.ReadValue(exePath, "endArg"));
                }
                if (Directory.Exists(exeDir))
                {
                    string source = Path.Combine(exeDir, "Other\\Source\\AppNamePortable.ini");
                    if (!File.Exists(source))
                        source = Path.Combine(exeDir, string.Format("Other\\Source\\{0}", iniName));
                    string dest = Path.Combine(exeDir, iniName);
                    if (File.Exists(source) && !File.Exists(dest))
                        File.Copy(source, dest);
                    foreach (string file in Directory.GetFiles(exeDir, "*.ini", SearchOption.TopDirectoryOnly))
                    {
                        string content = File.ReadAllText(file);
                        if (Regex.IsMatch(content, "DisableSplashScreen.*=.*false", RegexOptions.IgnoreCase))
                        {
                            content = Regex.Replace(content, "DisableSplashScreen.*=.*false", "DisableSplashScreen=true", RegexOptions.IgnoreCase);
                            File.Delete(file);
                            File.WriteAllText(file, content);
                        }
                    }
                    SilDev.Run.App(exeDir, exeName, CmdLine, _admin);
                }
                if (_close)
                    Application.Exit();
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        public static void StartApp(string _app, bool _close)
        {
            StartApp(_app, _close, false);
        }

        public static void StartApp(string _app)
        {
            StartApp(_app, false, false);
        }

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
            if (SilDev.MsgBox.Show(Lang.GetText("associateAppWayQuestion"), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                app = GetAppPath(_app);
            if (!File.Exists(app))
            {
                SilDev.MsgBox.Show(Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            foreach (string type in (types.Contains(",") ? types : string.Format("{0},", types)).Split(','))
            {
                if (!string.IsNullOrWhiteSpace(type) && !type.StartsWith("."))
                {
                    string TypeKey = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.ClassesRoot, string.Format(".{0}", type), null);
                    if (string.IsNullOrWhiteSpace(TypeKey) || TypeKey == "None")
                    {
                        TypeKey = string.Format("{0}_portable_type", type);
                        SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, string.Format(".{0}", type), null, TypeKey, SilDev.Reg.RegValueKind.ExpandString);
                    }
                    string IconRegEnt = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.ClassesRoot, string.Format("{0}\\DefaultIcon", TypeKey), null);
                    if (IconRegEnt != icon)
                        SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, string.Format("{0}\\DefaultIcon", TypeKey), null, icon, SilDev.Reg.RegValueKind.ExpandString);
                    string OpenCmdRegEnt = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.ClassesRoot, string.Format("{0}\\shell\\open\\command", TypeKey), null);
                    string OpenCmd = string.Format("\"{0}\" \"%1\"", app);
                    if (OpenCmdRegEnt != OpenCmd)
                        SilDev.Reg.WriteValue(SilDev.Reg.RegKey.ClassesRoot, string.Format("{0}\\shell\\open\\command", TypeKey), null, OpenCmd, SilDev.Reg.RegValueKind.ExpandString);
                    SilDev.Reg.RemoveValue(SilDev.Reg.RegKey.ClassesRoot, string.Format("{0}\\shell\\open\\command", TypeKey), "DelegateExecute");
                }
            }
            SilDev.MsgBox.Show(Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void StartMenuFolderUpdate(ComboBox.ObjectCollection _appList)
        {
            try
            {
                string StartMenuFolderPath = SilDev.Run.EnvironmentVariableFilter("%StartMenu%\\Programs\\Portable Apps");
                if (Directory.Exists(StartMenuFolderPath))
                {
                    string[] shortcuts = Directory.GetFiles(StartMenuFolderPath, "*.lnk", SearchOption.TopDirectoryOnly);
                    if (shortcuts.Length > 0)
                        foreach(var shortcut in shortcuts)
                            File.Delete(shortcut);
                }
                if (!Directory.Exists(StartMenuFolderPath))
                    Directory.CreateDirectory(StartMenuFolderPath);
                SilDev.Data.CreateShortcut(Application.ExecutablePath, Path.Combine(StartMenuFolderPath, string.Format("-- {0} --", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileDescription)));
                foreach (string item in _appList)
                {
                    string tmp = item;
                    Thread newThread = new Thread(() => SilDev.Data.CreateShortcut(GetAppPath(AppsDict[tmp]), Path.Combine(StartMenuFolderPath, tmp)));
                    newThread.Start();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private static void RepairDesktopIniFiles()
        {
            RepairDesktopIniFile(Path.Combine(AppDirs[0], "desktop.ini"), string.Format("[.ShellClassInfo]{0}IconResource =..\\Assets\\win10.folder.blue.ico,0", Environment.NewLine));
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
            try
            {
                if (File.Exists(_path))
                    File.Delete(_path);
                File.WriteAllText(_path, _content);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            SilDev.Run.App(@"%WinDir%\System32", "cmd.exe", string.Format("/C ATTRIB +H \"{0}\" && ATTRIB -HR \"{1}\" && ATTRIB +R \"{1}\"", _path, Path.GetDirectoryName(_path)), SilDev.Run.WindowStyle.Hidden);
        }
    }
}

