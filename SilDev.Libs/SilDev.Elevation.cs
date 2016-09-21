
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
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <para><see cref="SilDev.RUN"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class ELEVATION
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
                File.Create(PATH.Combine(dir, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose).Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WritableLocation() =>
            WritableLocation("%CurDir%");

        public static void RestartAsAdministrator(string commandLineArgs = "Default")
        {
            if (!IsAdministrator)
            {
                string args = string.Empty;
                if (commandLineArgs != "Default")
                    args = commandLineArgs;
                else
                {
                    if (LOG.DebugMode > 0)
                        args = $"/debug {LOG.DebugMode} ";
                    args = $"{args}{RUN.CommandLine(false)}";
                }
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = args,
                    FileName = LOG.AssemblyPath,
                    WorkingDirectory = PATH.Combine("%CurDir%"),
                    Verb = "runas"
                });
                Environment.ExitCode = 0;
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}

#endregion
