using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        static readonly string homePath = PATH.Combine("%CurDir%\\..");

        [STAThread]
        static void Main()
        {
            LOG.FileDir = PATH.Combine("%CurDir%\\..\\Documents\\.cache\\logs");
            INI.File(homePath, "Settings.ini");
            LOG.AllowDebug(INI.File(), "Settings");

            if (!RequirementsAvailable())
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
            string[] rArray = new string[]
            {
                "..\\Assets\\icon.db",
                "Helper\\7z\\7z.dll",
                "Helper\\7z\\7zG.exe",
                "Helper\\7z\\x64\\7z.dll",
                "Helper\\7z\\x64\\7zG.exe"
            };
            foreach (string s in rArray)
            {
                string path = PATH.Combine($"%CurDir%\\{s}");
                if (!File.Exists(path))
                    return false;
            }
            return true;
        }
    }
}
