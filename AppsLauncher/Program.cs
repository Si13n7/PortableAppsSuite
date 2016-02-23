using System;
using System.Diagnostics;
using System.Drawing;
#if x86
using System.IO;
#endif
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
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = AppsLauncher.Main.CmdLine,
                    FileName = Path.Combine(Application.StartupPath, AppsLauncher64)
                });
                return;
            }
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                SilDev.Log.AllowDebug();
                SilDev.Initialization.File(Application.StartupPath, "Settings.ini");
                int iniDebugOption = 0;
                if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "Debug"), out iniDebugOption))
                    SilDev.Log.ActivateDebug(iniDebugOption);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (newInstance)
                {
                    AppsLauncher.Main.SetAppDirs();
                    AppsLauncher.Main.LayoutColor = AppsLauncher.Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowMainColor"), SilDev.WinAPI.GetSystemThemeColor());
                    AppsLauncher.Main.ButtonColor = AppsLauncher.Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonColor"), SystemColors.ControlDark);
                    AppsLauncher.Main.ButtonHoverColor = AppsLauncher.Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonHoverColor"), AppsLauncher.Main.LayoutColor);
                    AppsLauncher.Main.ButtonTextColor = AppsLauncher.Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonTextColor"), SystemColors.ControlText);
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine))
                        Application.Run(new MenuViewForm());
                    else
                        Application.Run(new MainForm());
                }
                else
                {
                    foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                    {
                        if (p.Id != Process.GetCurrentProcess().Id)
                        {
                            SilDev.WinAPI.SafeNativeMethods.SetForegroundWindow(p.MainWindowHandle);
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine))
                        return;
                    if (AppsLauncher.Main.CmdLine.Contains("DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC"))
                    {
                        AppsLauncher.Main.SetAppDirs();
                        if (Environment.GetCommandLineArgs().Length == 3)
                            AppsLauncher.Main.AssociateFileTypes(Environment.GetCommandLineArgs()[2].Replace("\"", string.Empty));
                        return;
                    }
                    if (AppsLauncher.Main.CmdLine.Contains("A00C02E5-283A-44ED-9E4D-B82E8F87318F"))
                    {
                        AppsLauncher.Main.SetAppDirs();
                        if (Environment.GetCommandLineArgs().Length == 3)
                            AppsLauncher.Main.UndoFileTypeAssociation(Environment.GetCommandLineArgs()[2].Replace("\"", string.Empty));
                        return;
                    }
                    try
                    {
                        int hWnd = 0;
                        for (int i = 0; i < 2500; i++)
                        {
                            int.TryParse(SilDev.Initialization.ReadValue("History", "PID"), out hWnd);
                            if (hWnd > 0)
                                break;
                            Thread.Sleep(1);
                        }
                        if (hWnd > 0)
                            SilDev.WinAPI.SendArgs((IntPtr)hWnd, AppsLauncher.Main.CmdLine);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
        }
    }
}
