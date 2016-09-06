using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    static class Program
    {
        static string homePath = SilDev.Run.EnvVarFilter("%CurrentDir%\\..");
        static readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        [STAThread]
        static void Main()
        {
            SilDev.Log.FileLocation = SilDev.Run.EnvVarFilter("%CurrentDir%\\Protocols");
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

            if (!RequirementsExists())
            {
                string updPath = SilDev.Run.EnvVarFilter("%CurrentDir%\\Updater.exe");
                if (File.Exists(updPath))
                    SilDev.Run.App(new ProcessStartInfo() { FileName = updPath });
                else
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (MessageBox.Show(Lang.GetText("RequirementsErrorMsg"), "Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        Process.Start("https://github.com/Si13n7/PortableAppsSuite/releases");
                }
                return;
            }

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

        static bool RequirementsExists()
        {
            string[] sArray = new string[]
            {
                "..\\Apps\\.repack\\",
                "..\\Apps\\.free\\",
                "..\\Apps\\.share\\",
                "..\\Assets\\icon.db",
                "Updater.exe",
#if x86
                "Helper\\7z\\7z.dll",
                "Helper\\7z\\7zG.exe",
                "..\\AppsLauncher.exe",
#else
                "Helper\\7z\\x64\\7z.dll",
                "Helper\\7z\\x64\\7zG.exe",
                "..\\AppsLauncher64.exe"
#endif
            };
            foreach (string s in sArray)
            {
                string path = SilDev.Run.EnvVarFilter($"%CurrentDir%\\{s}");
                if (s.EndsWith("\\"))
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    if (!File.Exists(path))
                        return false;
                }
            }
            return true;
        }
    }
}
