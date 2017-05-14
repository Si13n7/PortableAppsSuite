namespace AppsDownloader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using LangResources;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using UI;

    internal static class Program
    {
        private static readonly string HomePath = PathEx.Combine(PathEx.LocalDir, "..");

        [STAThread]
        private static void Main()
        {
            Log.FileDir = PathEx.Combine(PathEx.LocalDir, "..\\Documents\\.cache\\logs");
            Ini.SetFile(HomePath, "Settings.ini");
            Log.AllowLogging(Ini.FilePath, @"(.*)\s*=\s*(.*)", "Debug");
#if x86
            string appsDownloader64;
            if (Environment.Is64BitOperatingSystem && File.Exists(appsDownloader64 = PathEx.Combine(PathEx.LocalDir, $"{Process.GetCurrentProcess().ProcessName}64.exe")))
            {
                ProcessEx.Start(appsDownloader64, EnvironmentEx.CommandLine());
                return;
            }
#endif
            if (!RequirementsAvailable())
            {
                var updPath = PathEx.Combine(PathEx.LocalDir, "Updater.exe");
                if (File.Exists(updPath))
                    ProcessEx.Start(updPath);
                else
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (MessageBox.Show(Lang.GetText(nameof(en_US.RequirementsErrorMsg)), Resources.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        Process.Start(PathEx.AltCombine(Resources.GitProfileUri, Resources.GitReleasesPath));
                }
                return;
            }
            try
            {
                var instanceKey = PathEx.LocalPath.GetHashCode().ToString();
                bool newInstance;
                using (new Mutex(true, instanceKey, out newInstance))
                {
                    var allowInstance = newInstance;
                    if (!allowInstance)
                    {
                        var instances = ProcessEx.GetInstances(PathEx.LocalPath);
                        var count = 0;
                        foreach (var instance in instances)
                        {
                            if (instance?.GetCommandLine()?.ContainsEx(AppsDownloader.Main.ActionGuid.UpdateInstance) == true)
                                count++;
                            instance?.Dispose();
                        }
                        allowInstance = count == 1;
                    }
                    if (!allowInstance)
                        return;
                    MessageBoxEx.TopMost = true;
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm().Plus());
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
            string[] sArray =
            {
                "..\\Apps\\.free\\",
                "..\\Apps\\.repack\\",
                "..\\Apps\\.share\\",
                "..\\Assets\\images.zip",
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
            foreach (var s in sArray)
            {
                var path = PathEx.Combine(PathEx.LocalDir, s);
                if (s.EndsWith("\\"))
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            ProcessEx.Start(new ProcessStartInfo
                                     {
                                         Arguments = "{48FDE635-60E6-41B5-8F9D-674E9F535AC7} {9AB50CEB-3D99-404E-BD31-4E635C09AF0F}",
#if x86
                                         FileName = "%CurDir%\\..\\AppsLauncher.exe"
#else
                                         FileName = "%CurDir%\\..\\AppsLauncher64.exe"
#endif
                                     })?.WaitForExit();
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
