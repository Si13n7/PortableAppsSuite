
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SilDev
{
    public static class Run
    {
        public enum MachineType : ushort
        {
            UNKNOWN = 0x0,
            AM33 = 0x1d3,
            AMD64 = 0x8664,
            ARM = 0x1c0,
            EBC = 0xebc,
            I386 = 0x14c,
            IA64 = 0x200,
            M32R = 0x9041,
            MIPS16 = 0x266,
            MIPSFPU = 0x366,
            MIPSFPU16 = 0x466,
            POWERPC = 0x1f0,
            POWERPCFP = 0x1f1,
            R4000 = 0x166,
            SH3 = 0x1a2,
            SH3DSP = 0x1a3,
            SH4 = 0x1a6,
            SH5 = 0x1a8,
            THUMB = 0x1c2,
            WCEMIPSV2 = 0x169,
        }

        public static MachineType GetPEArchitecture(string _file)
        {
            MachineType machineType;
            try
            {
                using (FileStream stream = new FileStream(_file, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    stream.Seek(0x3c, SeekOrigin.Begin);
                    stream.Seek(reader.ReadInt32(), SeekOrigin.Begin);
                    reader.ReadUInt32();
                    machineType = (MachineType)reader.ReadUInt16();
                }
            }
            catch
            {
                machineType = MachineType.UNKNOWN;
            }
            return machineType;
        }

        public static bool Is64Bit(string _file)
        {
            MachineType machineType = GetPEArchitecture(_file);
            return machineType == MachineType.AMD64 || machineType == MachineType.IA64;
        }

        private static List<string> cmdLineArgs = new List<string>();
        private static bool? cmdLineArgsSorted = null;
        public static List<string> CommandLineArgs(bool sort)
        {
            if (cmdLineArgs.Count == 0)
            {
                List<string> filteredArgs = new List<string>();
                if (Environment.GetCommandLineArgs().Length > 1 && cmdLineArgsSorted != sort)
                {
                    List<string> defaultArgs = Environment.GetCommandLineArgs().Skip(1).ToList();
                    cmdLineArgsSorted = sort;
                    if (sort)
                        defaultArgs.Sort();
                    bool debugArg = false;
                    foreach (string arg in defaultArgs)
                    {
                        if (arg.StartsWith("/debug", StringComparison.OrdinalIgnoreCase) || debugArg)
                        {
                            debugArg = !debugArg;
                            continue;
                        }
                        filteredArgs.Add(arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg);
                    }
                    cmdLineArgs = filteredArgs;
                }
            }
            return cmdLineArgs;
        }

        public static List<string> CommandLineArgs() =>
            CommandLineArgs(true);

        private static string commandLine = string.Empty;
        public static string CommandLine(bool sort)
        {
            if (CommandLineArgs(sort).Count > 0)
                commandLine = string.Join(" ", CommandLineArgs(sort));
            return commandLine;
        }

        public static string CommandLine() =>
            CommandLine(true);

        public static string EnvironmentVariableFilter(string _path)
        {
            string path = _path;
            try
            {
                path = Path.GetInvalidPathChars().Aggregate(_path.Trim(), (current, c) => current.Replace(c.ToString(), string.Empty));
                if (path.StartsWith("%") && (path.Contains("%\\") || path.EndsWith("%")))
                {
                    string variable = Regex.Match(path, "%(.+?)%", RegexOptions.IgnoreCase).Groups[1].Value;
                    string varDir = string.Empty;
                    switch (variable.ToLower())
                    {
                        case "commonstartmenu":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                            break;
                        case "commonstartup":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                            break;
                        case "currentdir":
                            varDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
                            break;
                        case "desktopdir":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            break;
                        case "mydocuments":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            break;
                        case "mymusic":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            break;
                        case "mypictures":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                            break;
                        case "myvideos":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                            break;
                        case "sendto":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
                            break;
                        case "startmenu":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                            break;
                        case "startup":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                            break;
                        default:
                            varDir = Environment.GetEnvironmentVariable(variable.ToLower());
                            break;
                    }
                    path = path.Replace($"%{variable}%", varDir);
                }
                if (path.Contains("..\\"))
                {
                    try
                    {
                        path = Path.GetFullPath(path);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
                while (path.Contains("\\\\"))
                    path = path.Replace("\\\\", "\\");
                path = path.EndsWith("\\") ? path.Substring(0, path.Length - 1) : path;
                if (_path != path)
                    Log.Debug($"Filtered path from '{_path}' to '{path}'");
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return path;
        }

        public static object App(ProcessStartInfo _psi, int _waitForInputIdle, int _waitForExit)
        {
            try
            {
                object output = null;
                using (Process app = new Process() { StartInfo = _psi })
                {
                    app.StartInfo.FileName = EnvironmentVariableFilter(app.StartInfo.FileName);
                    if (!File.Exists(app.StartInfo.FileName))
                        throw new Exception($"File '{app.StartInfo.FileName}' does not exists.");
                    app.StartInfo.WorkingDirectory = EnvironmentVariableFilter(string.IsNullOrEmpty(app.StartInfo.WorkingDirectory) || !string.IsNullOrEmpty(app.StartInfo.WorkingDirectory) && !Directory.Exists(app.StartInfo.WorkingDirectory) ? Path.GetDirectoryName(app.StartInfo.FileName) : app.StartInfo.WorkingDirectory);
                    if (!app.StartInfo.UseShellExecute && !app.StartInfo.CreateNoWindow && app.StartInfo.WindowStyle == ProcessWindowStyle.Hidden)
                        app.StartInfo.CreateNoWindow = true;
                    app.Start();
                    if (!app.StartInfo.UseShellExecute && app.StartInfo.RedirectStandardOutput)
                        output = app.StandardOutput.ReadToEnd();
                    if (_waitForInputIdle >= 0 && !app.HasExited)
                    {
                        if (_waitForInputIdle > 0)
                            app.WaitForInputIdle(_waitForInputIdle);
                        else
                            app.WaitForInputIdle();
                    }
                    if (_waitForExit >= 0 && !app.HasExited)
                    {
                        if (_waitForExit > 0)
                            app.WaitForExit(_waitForExit);
                        else
                            app.WaitForExit();
                    }
                    if (output != null)
                        output = new List<object> { app.Id, output };
                    else
                        output = app.Id;
                }
                return output;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return null;
        }

        public static object App(ProcessStartInfo _psi, int _waitForExit) => 
            App(_psi, -1, _waitForExit);

        public static object App(ProcessStartInfo _psi) => 
            App(_psi, -1, -1);

        public static void Cmd(string _command, bool _runAsAdmin, int _waitForExit)
        {
            string cmd = _command.TrimStart();
            if (cmd.Length >= 3)
            {
                cmd = cmd.StartsWith("/C", StringComparison.CurrentCultureIgnoreCase) || cmd.StartsWith("/K", StringComparison.CurrentCultureIgnoreCase) ? cmd.Substring(3) : cmd;
                cmd = $"/{(Log.DebugMode < 2 ? "C" : "K")} {cmd}{(Log.DebugMode < 2 ? string.Empty : " && pause && exit /b")}";
                App(new ProcessStartInfo()
                {
                    Arguments = cmd,
                    FileName = "%WinDir%\\System32\\cmd.exe",
                    Verb = _runAsAdmin ? "runas" : string.Empty,
                    WindowStyle = Log.DebugMode < 2 ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                }, _waitForExit);
                return;
            }
            Log.Debug("Cmd call is invalid.");
        }

        public static void Cmd(string _command, bool _runAsAdmin) => 
            Cmd(_command, _runAsAdmin, 0);

        public static void Cmd(string _command, int _waitForExit) => 
            Cmd(_command, false, _waitForExit);

        public static void Cmd(string _command) => 
            Cmd(_command, false, 0);
    }
}

#endregion
