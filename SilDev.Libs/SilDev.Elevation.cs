
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
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
                File.Create(Run.EnvVarFilter(dir, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose).Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WritableLocation() =>
            WritableLocation("%CurrentDir%");

        public static void RestartAsAdministrator(string commandLineArgs = "Default")
        {
            if (!IsAdministrator)
            {
                string args = string.Empty;
                if (commandLineArgs != "Default")
                    args = commandLineArgs;
                else
                {
                    if (Log.DebugMode > 0)
                        args = $"/debug {Log.DebugMode} ";
                    args = $"{args}{Run.CommandLine(false)}";
                }
                Run.App(new ProcessStartInfo()
                {
                    Arguments = args,
                    FileName = Run.EnvVarFilter(Assembly.GetEntryAssembly().CodeBase.Substring(8)),
                    WorkingDirectory = Run.EnvVarFilter("%CurrentDir%"),
                    Verb = "runas"
                });
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}

#endregion
