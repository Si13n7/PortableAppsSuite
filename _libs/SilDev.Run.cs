
#region SILENT DEVELOPMENTS generated code

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

        public static string EnvironmentVariableFilter(string _path)
        {
            string path = Path.GetInvalidPathChars().Aggregate(_path.TrimStart().TrimEnd(), (current, c) => current.Replace(c.ToString(), string.Empty));
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
                path = path.Replace(string.Format("%{0}%", variable), varDir.EndsWith("\\") ? varDir.Substring(0, varDir.Length - 1) : varDir);
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
            if (_path != path)
                Log.Debug(string.Format("Filtered path from '{0}' to '{1}'", _path, path));
            return path;
        }

        public enum WindowStyle
        {
            Hidden = ProcessWindowStyle.Hidden,
            Maximized = ProcessWindowStyle.Maximized,
            Minimized = ProcessWindowStyle.Minimized,
            Normal = ProcessWindowStyle.Normal
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
                        throw new Exception(string.Format("File '{0}' does not exists.", app.StartInfo.FileName));
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

        public static object App(ProcessStartInfo _psi, int _waitForExit)
        {
            return App(_psi, -1, _waitForExit);
        }

        public static object App(ProcessStartInfo _psi)
        {
            return App(_psi, -1, -1);
        }

        #region OLD SCRIPT COMPATIBLITY WRAPPER

        /// <summary>
        /// ALLOWED PARAMETERS: string WorkingDirectory, string FileName, string Arguments, bool VerbRunAs, ProcessWindowStyle WindowStyle, int WaitForInputIdle, int WaitForExit
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static int App(params object[] _obj)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            List<int> intValues = new List<int>();
            foreach (object obj in _obj)
            {
                if (obj is string)
                {
                    if (string.IsNullOrEmpty(psi.FileName) || string.IsNullOrEmpty(psi.WorkingDirectory))
                    {
                        string tmp = EnvironmentVariableFilter((string)obj);
                        if (string.IsNullOrEmpty(psi.WorkingDirectory) && (File.GetAttributes(tmp) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            psi.WorkingDirectory = tmp;
                            continue;
                        }
                        if (string.IsNullOrEmpty(psi.FileName))
                        {
                            psi.FileName = string.IsNullOrEmpty(psi.WorkingDirectory) ? tmp : Path.Combine(psi.WorkingDirectory, tmp);
                            if (string.IsNullOrEmpty(psi.WorkingDirectory))
                                psi.WorkingDirectory = Path.GetDirectoryName(psi.WorkingDirectory);
                            continue;
                        }
                    }
                    if (string.IsNullOrEmpty(psi.Arguments) && !string.IsNullOrEmpty(psi.FileName) && !string.IsNullOrEmpty(psi.WorkingDirectory))
                        psi.Arguments = (string)obj;
                    continue;
                }
                if (obj is bool)
                {
                    psi.Verb = (bool)obj ? "runas" : string.Empty;
                    continue;
                }
                if (obj is ProcessWindowStyle || obj is WindowStyle)
                {
                    psi.WindowStyle = (ProcessWindowStyle)obj;
                    continue;
                }
                if (obj is int)
                    intValues.Add((int)obj);
            }
            int _waitForInputIdle = -1, _waitForExit = -1;
            if (intValues.Count == 2)
            {
                _waitForInputIdle = intValues[0];
                _waitForExit = intValues[1];
            }
            else if (intValues.Count == 1)
                _waitForExit = intValues[0];
            object output = App(psi, _waitForInputIdle, _waitForExit);
            int appId = output is List<object> ? ((List<object>)output)[0] is int ? (int)((List<object>)output)[0] : -1 : output is int ? (int)output : -1;
            return appId;
        }

        #endregion

        public static void Cmd(string _command, bool _runAsAdmin, int _waitForExit)
        {
            string cmd = _command.TrimStart();
            if (cmd.Length >= 3)
            {
                cmd = cmd.StartsWith("/C", StringComparison.CurrentCultureIgnoreCase) || cmd.StartsWith("/K", StringComparison.CurrentCultureIgnoreCase) ? cmd.Substring(3) : cmd;
                cmd = string.Format("/{0} {1}{2}", Log.DebugMode < 2 ? "C" : "K", cmd, Log.DebugMode < 2 ? string.Empty : " && pause && exit /b");
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

        public static void Cmd(string _command, bool _runAsAdmin)
        {
            Cmd(_command, _runAsAdmin, 0);
        }

        public static void Cmd(string _command, int _waitForExit)
        {
            Cmd(_command, false, _waitForExit);
        }

        public static void Cmd(string _command)
        {
            Cmd(_command, false, 0);
        }
    }
}

#endregion
