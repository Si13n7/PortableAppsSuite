using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if x86
            string AppsLauncher64 = string.Format("{0}64.exe", Process.GetCurrentProcess().ProcessName);
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsLauncher64))
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, AppsLauncher64), Arguments = AppsLauncher.Main.CmdLine });
                return;
            }
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                SilDev.Log.AllowDebug();
                SilDev.Initialization.File(Application.StartupPath, "AppsLauncher.ini");
                int iniDebugOption = 0;
                if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "Debug"), out iniDebugOption))
                    SilDev.Log.ActivateDebug(iniDebugOption);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (newInstance)
                {
                    SetAppDirs();
                    try
                    {
                        Application.Run(new MainForm());
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
                else
                {
                    foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                    {
                        if (p.Id != Process.GetCurrentProcess().Id)
                        {
                            SilDev.WinAPI.SetForegroundWindow(p.MainWindowHandle);
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine))
                        return;
                    if (AppsLauncher.Main.CmdLine.Contains("DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC"))
                    {
                        SetAppDirs();
                        if (Environment.GetCommandLineArgs().Length == 3)
                            AppsLauncher.Main.AssociateFileTypes(Environment.GetCommandLineArgs()[2].Replace("\"", string.Empty));
                        return;
                    }
                    try
                    {
                        int hwnd = 0;
                        for (int i = 0; i < 10000; i++)
                        {
                            int.TryParse(SilDev.Initialization.ReadValue("History", "PID"), out hwnd);
                            if (hwnd > 0)
                                break;
                            Thread.Sleep(1);
                        }
                        if (hwnd > 0)
                            SilDev.WinAPI.SendArgs((IntPtr)hwnd, AppsLauncher.Main.CmdLine);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
        }

        static void SetAppDirs()
        {
            string dirs = SilDev.Initialization.ReadValue("Settings", "AppDirs");
            if (!string.IsNullOrWhiteSpace(dirs))
            {
                dirs = SilDev.Crypt.Base64.Decrypt(dirs);
                if (!string.IsNullOrWhiteSpace(dirs))
                {
                    if (!dirs.Contains(Environment.NewLine))
                        dirs += Environment.NewLine;
                    AppsLauncher.Main.AppDirs = AppsLauncher.Main.AppDirs.Concat(dirs.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)).Where(c => Directory.Exists(SilDev.Run.EnvironmentVariableFilter(c))).ToArray();
                }
            }
        }
    }
}
