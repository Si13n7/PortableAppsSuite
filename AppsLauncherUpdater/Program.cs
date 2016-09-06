using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        static string homePath = SilDev.Run.EnvVarFilter("%CurrentDir%\\..");

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

            if (!RequirementsExists())
            {
                Lang.ResourcesNamespace = typeof(Program).Namespace;
                if (MessageBox.Show(Lang.GetText("RequirementsErrorMsg"), "Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    Process.Start("https://github.com/Si13n7/PortableAppsSuite/releases");
                return;
            }

            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance)
                    {
                        if (!SilDev.Elevation.WritableLocation(homePath))
                            SilDev.Elevation.RestartAsAdministrator();
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
            string[] rArray = new string[]
            {
                "Helper\\7z\\7z.dll",
                "Helper\\7z\\7zG.exe",
                "Helper\\7z\\x64\\7z.dll",
                "Helper\\7z\\x64\\7zG.exe"
            };
            foreach (string s in rArray)
            {
                string path = SilDev.Run.EnvVarFilter($"%CurrentDir%\\{s}");
                if (!File.Exists(path))
                    return false;
            }
            return true;
        }
    }
}
