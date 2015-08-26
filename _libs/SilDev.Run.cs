
#region SILENT DEVELOPMENTS generated code

using System;
using System.Diagnostics;
using System.IO;
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
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        stream.Seek(0x3c, SeekOrigin.Begin);
                        stream.Seek(reader.ReadInt32(), SeekOrigin.Begin);
                        reader.ReadUInt32();
                        machineType = (MachineType)reader.ReadUInt16();
                    }
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
            try
            {
                string path = _path.TrimStart().TrimEnd();
                if (path.StartsWith("%") && (path.Contains("%\\") || path.EndsWith("%")))
                {
                    string variable = Regex.Match(path, "%(.+?)%", RegexOptions.IgnoreCase).Groups[1].Value;
                    switch (variable.ToLower())
                    {
                        case "commonprogramfiles":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
                        case "commonprogramfiles(x86)":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86));
                        case "commonstartmenu":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
                        case "commonstartup":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
                        case "currentdir":
                            return path.Replace(string.Format("%{0}%", variable), Environment.CurrentDirectory);
                        case "desktopdir":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                        case "mydocuments":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        case "mymusic":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                        case "mypictures":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                        case "myvideos":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
                        case "programfiles":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                        case "programfiles(x86)":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                        case "sendto":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.SendTo));
                        case "startmenu":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
                        case "startup":
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                        default:
                            return path.Replace(string.Format("%{0}%", variable), Environment.GetEnvironmentVariable(variable.ToLower()));
                    }
                }
                return path;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return _path;
            }
        }

        public enum WindowStyle
        {
            Hidden = ProcessWindowStyle.Hidden,
            Maximized = ProcessWindowStyle.Maximized,
            Minimized = ProcessWindowStyle.Minimized,
            Normal = ProcessWindowStyle.Normal
        }

        public static int App(ProcessStartInfo _psi, int _waitForInputIdle, int _waitForExit)
        {
            try
            {
                using (Process app = new Process() { StartInfo = _psi })
                {
                    app.StartInfo.FileName = EnvironmentVariableFilter(app.StartInfo.FileName);
                    if (!File.Exists(app.StartInfo.FileName))
                        throw new Exception(string.Format("File '{0}' does not exists.", app.StartInfo.FileName));
                    app.StartInfo.WorkingDirectory = EnvironmentVariableFilter(string.IsNullOrWhiteSpace(app.StartInfo.WorkingDirectory) || !Directory.Exists(app.StartInfo.WorkingDirectory) ? Path.GetDirectoryName(app.StartInfo.FileName) : app.StartInfo.WorkingDirectory);
                    app.Start();
                    if (_waitForInputIdle >= 0)
                    {
                        if (!app.HasExited)
                        {
                            if (_waitForInputIdle > 0)
                                app.WaitForInputIdle(_waitForInputIdle);
                            else
                                app.WaitForInputIdle();
                        }
                    }
                    if (_waitForExit >= 0)
                    {
                        if (!app.HasExited)
                        {
                            if (_waitForExit > 0)
                                app.WaitForExit(_waitForExit);
                            else
                                app.WaitForExit();
                        }
                    }
                    return app.Id;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return -1;
        }

        public static int App(ProcessStartInfo _psi, int _waitForExit)
        {
            return App(_psi, -1, _waitForExit);
        }

        public static int App(ProcessStartInfo _psi)
        {
            return App(_psi, -1, -1);
        }

        #region OLD SHIT

        public static int App(string _path, string _file, string _arg, bool _admin, ProcessWindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(new ProcessStartInfo() { Arguments = _arg, FileName = EnvironmentVariableFilter(Path.Combine(_path, _file)), Verb = _admin ? "runas" : string.Empty, WindowStyle = _wndStyle }, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, _arg, _admin, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, ProcessWindowStyle _wndStyle)
        {
            return App(_path, _file, _arg, _admin, _wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(_path, _file, _arg, _admin, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, _arg, _admin, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, WindowStyle _wndStyle)
        {
            return App(_path, _file, _arg, _admin, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, string _arg, bool _admin, int _waitForExit)
        {
            return App(_path, _file, _arg, _admin, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, bool _admin)
        {
            return App(_path, _file, _arg, _admin, ProcessWindowStyle.Normal, -1, -1);
        }

        public static int App(string _path, string _file, bool _admin, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, _admin, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, bool _admin, ProcessWindowStyle _wndStyle)
        {
            return App(_path, _file, string.Empty, _admin, _wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, bool _admin, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, _admin, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _path, string _file, bool _admin, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, _admin, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, bool _admin, WindowStyle _wndStyle)
        {
            return App(_path, _file, string.Empty, _admin, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, bool _admin, int _waitForExit)
        {
            return App(_path, _file, string.Empty, _admin, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _path, string _file, bool _admin)
        {
            return App(_path, _file, string.Empty, _admin, ProcessWindowStyle.Normal, -1, -1);
        }

        public static int App(string _path, string _file, string _arg, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, _arg, false, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, ProcessWindowStyle _wndStyle)
        {
            return App(_path, _file, _arg, false, _wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, string _arg, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(_path, _file, _arg, false, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, _arg, false, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg, WindowStyle _wndStyle)
        {
            return App(_path, _file, _arg, false, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, string _arg, int _waitForExit)
        {
            return App(_path, _file, _arg, false, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _path, string _file, string _arg)
        {
            return App(_path, _file, _arg, false, ProcessWindowStyle.Normal, -1, -1);
        }

        public static int App(string _path, string _file, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, false, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, ProcessWindowStyle _wndStyle)
        {
            return App(_path, _file, string.Empty, false, _wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, false, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _path, string _file, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(_path, _file, string.Empty, false, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _path, string _file, WindowStyle _wndStyle)
        {
            return App(_path, _file, string.Empty, false, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _path, string _file, int _waitForExit)
        {
            return App(_path, _file, string.Empty, false, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _path, string _file)
        {
            return App(_path, _file, string.Empty, false, ProcessWindowStyle.Normal, -1, -1);
        }

        public static int App(string _file, bool _admin, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _file, bool _admin, ProcessWindowStyle _wndStyle)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, _wndStyle, -1, -1);
        }

        public static int App(string _file, bool _admin, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _file, bool _admin, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _file, bool _admin, WindowStyle _wndStyle)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _file, bool _admin, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _file, bool _admin)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, _admin, ProcessWindowStyle.Normal, -1, -1);
        }

        public static int App(string _file, ProcessWindowStyle _wndStyle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, _wndStyle, -1, _waitForExit);
        }

        public static int App(string _file, ProcessWindowStyle _wndStyle)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, _wndStyle, -1, -1);
        }

        public static int App(string _file, WindowStyle _wndStyle, int _waitForInputIdle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, (ProcessWindowStyle)_wndStyle, _waitForInputIdle, _waitForExit);
        }

        public static int App(string _file, WindowStyle _wndStyle, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, (ProcessWindowStyle)_wndStyle, -1, _waitForExit);
        }

        public static int App(string _file, WindowStyle _wndStyle)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, (ProcessWindowStyle)_wndStyle, -1, -1);
        }

        public static int App(string _file, int _waitForExit)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, ProcessWindowStyle.Normal, -1, _waitForExit);
        }

        public static int App(string _file)
        {
            return App(Path.GetDirectoryName(_file), Path.GetFileName(_file), string.Empty, false, ProcessWindowStyle.Normal, -1, -1);
        }

        #endregion
    }
}

#endregion
