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
        static readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        [STAThread]
        static void Main()
        {
            if (!File.Exists(Path.Combine(homePath, "AppsLauncher.exe")) && !File.Exists(Path.Combine(homePath, "AppsLauncher64.exe")))
                return;
            SilDev.Log.FileLocation = Path.Combine(Application.StartupPath, "Protocols");
            SilDev.Log.AllowDebug();
            SilDev.Ini.File(homePath, "Settings.ini");
            if (SilDev.Log.DebugMode == 0)
            {
                int iniDebugOption = SilDev.Ini.ReadInteger("Settings", "Debug", 0);
                if (iniDebugOption > 0)
                    SilDev.Log.ActivateDebug(iniDebugOption);
            }
#if x86
            string AppsDownloader64 = Path.Combine(Application.StartupPath, $"{Process.GetCurrentProcess().ProcessName}64.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsDownloader64))
            {
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = SilDev.Run.CommandLine(),
                    FileName = AppsDownloader64
                });
                return;
            }
#endif
            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance || Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 2 && SilDev.Run.CommandLineArgs().Count < 2 && SilDev.Run.CommandLineArgs().Count != SilDev.Ini.ReadInteger("Settings", "XInstanceArgs", SilDev.Run.CommandLineArgs().Count))
                    {
                        if (!SilDev.Elevation.WritableLocation(homePath))
                            SilDev.Elevation.RestartAsAdministrator(SilDev.Run.CommandLine());
                        SilDev.Ini.Write("Settings", "XInstanceArgs", SilDev.Run.CommandLineArgs().Count);
                        Lang.ResourcesNamespace = typeof(Program).Namespace;
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
