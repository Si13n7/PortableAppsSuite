using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    static class Program
    {
        static readonly string homePath = PATH.Combine("%CurDir%\\..");
        static readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        [STAThread]
        static void Main()
        {
            LOG.FileDir = PATH.Combine("%CurDir%\\Protocols");
            INI.File(homePath, "Settings.ini");
            LOG.AllowDebug(INI.File(), "Settings");

#if x86
            string AppsDownloader64;
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsDownloader64 = PATH.Combine($"%CurDir%\\{Process.GetCurrentProcess().ProcessName}64.exe")))
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = RUN.CommandLine(),
                    FileName = AppsDownloader64
                });
                return;
            }
#endif

            if (!RequirementsAvailable())
            {
                string updPath = PATH.Combine("%CurDir%\\Updater.exe");
                if (File.Exists(updPath))
                    RUN.App(new ProcessStartInfo() { FileName = updPath });
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
                    if (newInstance || Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 2 && RUN.CommandLineArgs().Count < 2 && RUN.CommandLineArgs().Count != INI.ReadInteger("Settings", "X.InstanceArgs", RUN.CommandLineArgs().Count))
                    {
                        INI.Write("Settings", "X.InstanceArgs", RUN.CommandLineArgs().Count);
                        Lang.ResourcesNamespace = typeof(Program).Namespace;
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm());
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        static bool RequirementsAvailable()
        {
            if (!ELEVATION.WritableLocation())
                ELEVATION.RestartAsAdministrator(RUN.CommandLine());
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
                string path = PATH.Combine($"%CurDir%\\{s}");
                if (s.EndsWith("\\"))
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            RUN.App(new ProcessStartInfo
                            {
                                Arguments = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7} {9AB50CEB-3D99-404E-BD31-4E635C09AF0F}",
#if x86
                                FileName = "%CurDir%\\..\\AppsLauncher.exe"
#else
                                FileName = "%CurDir%\\..\\AppsLauncher64.exe"
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
