
#region SILENT DEVELOPMENTS generated code

using System;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace SilDev
{
    public static class Elevation
    {
        public static bool IsAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static bool WritableLocation(string _path)
        {
            string file = Path.Combine(_path, "Si13n7_TestFile");
            try
            {
                using (StreamWriter sw = new StreamWriter(file))
                    sw.WriteLine("true");
                if (File.Exists(file))
                {
                    File.Delete(file);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool WritableLocation() => 
            WritableLocation(Application.StartupPath);

        public static void RestartAsAdministrator(string _args)
        {
            if (!IsAdministrator)
            {
                Run.App(new System.Diagnostics.ProcessStartInfo()
                {
                    Arguments = _args,
                    FileName = Application.ExecutablePath
                }, true);
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }

        public static void RestartAsAdministrator() => 
            RestartAsAdministrator(string.Empty);
    }
}

#endregion
