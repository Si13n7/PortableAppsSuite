using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    static class Program
    {
        static readonly string homePath = SilDev.Run.EnvVarFilter("%CurrentDir%\\..");
        static readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        [STAThread]
        static void Main()
        {
            SilDev.Log.FileDir = SilDev.Run.EnvVarFilter("%CurrentDir%\\Protocols");
            SilDev.Ini.File(homePath, "Settings.ini");
            SilDev.Log.AllowDebug(SilDev.Ini.File(), "Settings");

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

            if (!RequirementsAvailable())
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
                    if (newInstance || Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 2 && SilDev.Run.CommandLineArgs().Count < 2 && SilDev.Run.CommandLineArgs().Count != SilDev.Ini.ReadInteger("Settings", "X.InstanceArgs", SilDev.Run.CommandLineArgs().Count))
                    {
                        SilDev.Ini.Write("Settings", "X.InstanceArgs", SilDev.Run.CommandLineArgs().Count);
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

        static bool RequirementsAvailable()
        {
            if (!SilDev.Elevation.WritableLocation())
                SilDev.Elevation.RestartAsAdministrator(SilDev.Run.CommandLine());
            string[] sArray = new string[]
            {
                "..\\Apps\\.free\\",
                "..\\Apps\\.repack\\",
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
                            SilDev.Run.App(new ProcessStartInfo
                            {
                                Arguments = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7} {9AB50CEB-3D99-404E-BD31-4E635C09AF0F}",
#if x86
                                FileName = "%CurrentDir%\\..\\AppsLauncher.exe"
#else
                                FileName = "%CurrentDir%\\..\\AppsLauncher64.exe"
#endif
                            }, 0);
                        if (!Directory.Exists(path))
                            throw new DirectoryNotFoundException();
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
