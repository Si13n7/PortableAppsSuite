using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        static string homePath = Path.GetFullPath($"{Application.StartupPath}\\..");

        [STAThread]
        static void Main()
        {
            if (!File.Exists(Path.Combine(homePath, "AppsLauncher.exe")) && !File.Exists(Path.Combine(homePath, "AppsLauncher64.exe")))
                return;
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
                            SilDev.Elevation.RestartAsAdministrator();
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
