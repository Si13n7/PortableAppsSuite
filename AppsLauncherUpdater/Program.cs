namespace Updater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;
    using SilDev.Forms;

    internal static class Program
    {
        private static readonly string HomePath = PathEx.Combine(PathEx.LocalDir, "..");

        [STAThread]
        private static void Main()
        {
            Log.FileDir = PathEx.Combine(PathEx.LocalDir, "..\\Documents\\.cache\\logs");
            Ini.File(HomePath, "Settings.ini");
            Log.AllowLogging(Ini.File(), "Settings", "Debug");
            if (!RequirementsAvailable())
            {
                Lang.ResourcesNamespace = typeof(Program).Namespace;
                if (MessageBox.Show(Lang.GetText("RequirementsErrorMsg"), @"Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    Process.Start("https://github.com/Si13n7/PortableAppsSuite/releases");
                return;
            }
            try
            {
                bool newInstance;
                using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                    if (newInstance)
                    {
                        MessageBoxEx.TopMost = true;
                        Lang.ResourcesNamespace = typeof(Program).Namespace;
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm().AddLoadingTimeStopwatch());
                    }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static bool RequirementsAvailable()
        {
            if (!Elevation.WritableLocation())
                Elevation.RestartAsAdministrator(EnvironmentEx.CommandLine());
            string[] rArray =
            {
                "..\\Assets\\icon.db",
                "Helper\\7z\\7z.dll",
                "Helper\\7z\\7zG.exe",
                "Helper\\7z\\x64\\7z.dll",
                "Helper\\7z\\x64\\7zG.exe"
            };
            return rArray.Select(s => PathEx.Combine(PathEx.LocalDir, s)).All(File.Exists);
        }
    }
}
