
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
    /// <para><see cref="SilDev.WinAPI"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Elevation
    {
        public static bool IsAdministrator
        {
            get
            {
                try
                {
                    return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool WritableLocation(string dir)
        {
            try
            {
                File.Create(Run.EnvironmentVariableFilter(dir, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose).Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WritableLocation() => 
            WritableLocation(Application.StartupPath);

        public static void RestartAsAdministrator(string commandLineArgs = "Default")
        {
            if (!IsAdministrator)
            {
                string s = string.Empty;
                if (commandLineArgs != "Default")
                    s = commandLineArgs;
                else
                {
                    if (Log.DebugMode > 0)
                        s = $"/debug {Log.DebugMode} ";
                    s = $"{s}{Run.CommandLine(false)}";
                }
                Run.App(new ProcessStartInfo()
                {
                    Arguments = s,
                    FileName = Application.ExecutablePath,
                    WorkingDirectory = Application.StartupPath,
                    Verb = "runas"
                });
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}

#endregion
