namespace AppsDownloader.Libraries
{
    using System.IO;
    using SilDev;

    internal static class Recovery
    {
        internal static bool AppsSuiteIsHealthy(bool repair = true)
        {
            if (!Elevation.WritableLocation())
                Elevation.RestartAsAdministrator();
            while (true)
            {
                try
                {
                    if (!File.Exists(Settings.CorePaths.AppsLauncher) ||
                        !File.Exists(Settings.CorePaths.AppsSuiteUpdater) ||
                        !File.Exists(Settings.CorePaths.FileArchiver))
                        throw new FileNotFoundException();
                }
                catch (FileNotFoundException ex)
                {
                    Log.Write(ex);
                    if (!repair)
                        return false;
                    Repair(Settings.ActionGuid.RepairAppsSuite, true);
                }

                try
                {
                    foreach (var dir in Settings.CorePaths.AppDirs)
                        if (!Directory.Exists(dir))
                            throw new PathNotFoundException(dir);
                }
                catch (PathNotFoundException ex)
                {
                    Log.Write(ex);
                    if (!repair)
                        return false;
                    Repair(Settings.ActionGuid.RepairDirs, false);
                }

                if (!repair)
                    return true;
                repair = false;
            }
        }

        private static void Repair(string guid, bool elevated)
        {
            using (var p = ProcessEx.Start(PathEx.LocalPath, guid, elevated, false))
                if (p?.HasExited == false)
                    p.WaitForExit();
        }
    }
}
