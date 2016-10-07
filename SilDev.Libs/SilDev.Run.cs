
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class RUN
    {
        private static List<string> cmdLineArgs = new List<string>();
        private static bool cmdLineArgsQuotes = true;
        public static List<string> CommandLineArgs(bool sort = true, int skip = 1, bool quotes = true)
        {
            if (cmdLineArgs.Count != Environment.GetCommandLineArgs().Length - skip || quotes != cmdLineArgsQuotes)
            {
                cmdLineArgsQuotes = quotes;
                List<string> filteredArgs = new List<string>();
                try
                {
                    if (Environment.GetCommandLineArgs().Length > skip)
                    {
                        List<string> defaultArgs = Environment.GetCommandLineArgs().Skip(skip).ToList();
                        if (sort)
                            defaultArgs = defaultArgs.OrderBy(x => x, new AscendentAlphanumericStringComparer()).ToList();
                        bool debugArg = false;
                        foreach (string arg in defaultArgs)
                        {
                            if (arg.StartsWith("/debug", StringComparison.OrdinalIgnoreCase) || debugArg)
                            {
                                debugArg = !debugArg;
                                continue;
                            }
                            filteredArgs.Add(quotes && arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg);
                        }
                        cmdLineArgs = filteredArgs;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return cmdLineArgs;
        }

        public static List<string> CommandLineArgs(bool sort, bool quotes) =>
            CommandLineArgs(sort, 1, quotes);

        public static List<string> CommandLineArgs(int skip) =>
            CommandLineArgs(true, skip);

        private static string commandLine = string.Empty;
        public static string CommandLine(bool sort = true, int skip = 1, bool quotes = true)
        {
            if (CommandLineArgs(sort).Count > 0)
                commandLine = CommandLineArgs(sort, skip, quotes).Join(" ");
            return commandLine;
        }

        public static string CommandLine(bool sort, bool quotes) =>
            CommandLine(true, 1, quotes);

        public static string CommandLine(int skip) =>
            CommandLine(true, skip);

        public static string LastStreamOutput { get; private set; }

        public static int App(ProcessStartInfo psi, int? waitForInputIdle, int? waitForExit, bool forceWorkingDir = true)
        {
            try
            {
                int pid = -1;
                using (Process p = new Process() { StartInfo = psi })
                {
                    p.StartInfo.FileName = PATH.Combine(p.StartInfo.FileName);
                    if (string.IsNullOrWhiteSpace(p.StartInfo.FileName))
                        throw new ArgumentNullException();
                    if (!File.Exists(p.StartInfo.FileName))
                        throw new FileNotFoundException($"File '{p.StartInfo.FileName}' does not exists.");
                    if (p.StartInfo.FileName.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                        p.Start();
                    else
                    {
                        if (forceWorkingDir)
                        {
                            p.StartInfo.WorkingDirectory = PATH.Combine(p.StartInfo.WorkingDirectory);
                            if (!Directory.Exists(p.StartInfo.WorkingDirectory))
                                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(p.StartInfo.FileName);
                        }
                        if (!p.StartInfo.UseShellExecute && !p.StartInfo.CreateNoWindow && p.StartInfo.WindowStyle == ProcessWindowStyle.Hidden)
                            p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        try
                        {
                            if (!p.StartInfo.UseShellExecute && p.StartInfo.RedirectStandardOutput)
                                LastStreamOutput = p.StandardOutput.ReadToEnd();
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                        try
                        {
                            if (waitForInputIdle != null && !p.HasExited)
                            {
                                if (waitForInputIdle <= 0)
                                    waitForInputIdle = -1;
                                p.WaitForInputIdle((int)waitForInputIdle);
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                        if (waitForExit != null && !p.HasExited)
                        {
                            if (waitForExit <= 0)
                                waitForExit = -1;
                            p.WaitForExit((int)waitForExit);
                        }
                        pid = p.Id;
                    }
                }
                return pid;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return -1;
        }

        public static int App(ProcessStartInfo psi, int? waitForExit = null, bool forceWorkingDir = true) => 
            App(psi, null, waitForExit);

        public static void Cmd(string command, bool runAsAdmin, int? waitForExit = null)
        {
            try
            {
                string cmd = command.Trim();
                if (cmd.StartsWith("/K", StringComparison.OrdinalIgnoreCase))
                    cmd = cmd.Substring(2).TrimStart();
                if (!cmd.StartsWith("/C", StringComparison.OrdinalIgnoreCase))
                    cmd = $"/C {cmd}";
                if (cmd.Length <= 3)
                    throw new ArgumentNullException();
                App(new ProcessStartInfo()
                {
                    Arguments = cmd,
                    FileName = "%System%\\cmd.exe",
                    UseShellExecute = runAsAdmin,
                    Verb = runAsAdmin ? "runas" : string.Empty,
                    WindowStyle = LOG.DebugMode < 2 ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                }, waitForExit);
                if (LOG.DebugMode > 0)
                    LOG.Debug($"COMMAND EXECUTED: {cmd.Substring(3)}");
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static void Cmd(string command, int? waitForExit = null) => 
            Cmd(command, false, waitForExit);
    }
}
