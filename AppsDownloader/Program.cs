using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    static class Program
    {
        static string homePath = Path.GetFullPath(string.Format("{0}\\..", Application.StartupPath));

        [STAThread]
        static void Main()
        {
#if x86
            string cmdLine = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).TrimStart();
            string AppsDownloader64 = string.Format("{0}64.exe", Process.GetCurrentProcess().ProcessName);
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsDownloader64))
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, AppsDownloader64), Arguments = cmdLine });
                return;
            }
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    if (!File.Exists(Path.Combine(homePath, "AppsLauncher.exe")) && !File.Exists(Path.Combine(homePath, "AppsLauncher64.exe")))
                        return;
                    SilDev.Log.AllowDebug();
                    SilDev.Initialization.File(homePath, "AppsLauncher.ini");
                    int iniDebugOption = 0;
                    if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "Debug"), out iniDebugOption))
                        SilDev.Log.ActivateDebug(iniDebugOption);
                    if (!SilDev.Elevation.WritableLocation())
                        SilDev.Elevation.RestartAsAdministrator();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    try
                    {
                        Application.Run(new MainForm());
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
