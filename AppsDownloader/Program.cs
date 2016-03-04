using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    static class Program
    {
        static string homePath = Path.GetFullPath($"{Application.StartupPath}\\..");

        [STAThread]
        static void Main()
        {
            if (!File.Exists(Path.Combine(homePath, "AppsLauncher.exe")) && !File.Exists(Path.Combine(homePath, "AppsLauncher64.exe")))
                return;
            string cmdLine = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).TrimStart();
#if x86
            string AppsDownloader64 = $"{Process.GetCurrentProcess().ProcessName}64.exe";
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsDownloader64))
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, AppsDownloader64), Arguments = cmdLine });
                return;
            }
#endif
            SilDev.Log.AllowDebug();
            SilDev.Initialization.File(homePath, "Settings.ini");
            int iniDebugOption = 0;
            if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "Debug"), out iniDebugOption))
                SilDev.Log.ActivateDebug(iniDebugOption);
            bool newInstance = true;
            try
            {
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance)
                    {
                        if (!SilDev.Elevation.WritableLocation(homePath))
                            SilDev.Elevation.RestartAsAdministrator(cmdLine);
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm());
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }
    }
}
