
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SilDev
{
    public static class Data
    {
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        public static bool CreateShortcut(string _target, string _path, string _args, string _icon, bool _skipExists)
        {
            try
            {
                string shortcutPath = Run.EnvironmentVariableFilter(!_path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? $"{_path}.lnk" : _path);
                if (!Directory.Exists(Path.GetDirectoryName(shortcutPath)) || !File.Exists(_target))
                    return false;
                if (File.Exists(shortcutPath))
                {
                    if (_skipExists)
                        return true;
                    File.Delete(shortcutPath);
                }
                IShellLink shell = (IShellLink)new ShellLink();
                if (!string.IsNullOrWhiteSpace(_args))
                    shell.SetArguments(_args);
                shell.SetDescription(string.Empty);
                shell.SetPath(_target);
                shell.SetIconLocation(_icon, 0);
                shell.SetWorkingDirectory(Path.GetDirectoryName(_target));
                ((IPersistFile)shell).Save(shortcutPath, false);
                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return false;
        }

        public static bool CreateShortcut(string _target, string _path, string _args, bool _skipExists) =>
            CreateShortcut(_target, _path, _args, _target, _skipExists);

        public static bool CreateShortcut(string _target, string _path, string _args) =>
            CreateShortcut(_target, _path, _args, _target, false);

        public static bool CreateShortcut(string _target, string _path, bool _skipExists) =>
            CreateShortcut(_target, _path, null, _target, _skipExists);

        public static bool CreateShortcut(string _target, string _path) =>
            CreateShortcut(_target, _path, null, _target, false);

        public enum Attrib
        {
            Archive,
            Compressed,
            Device,
            Directory,
            Encrypted,
            Hidden,
            IntegrityStream,
            Normal,
            NoScrubData,
            NotContentIndexed,
            Offline,
            ReadOnly,
            ReparsePoint,
            SparseFile,
            System,
            Temporary
        }

        private static FileAttributes GetAttrib(Attrib _attrib)
        {
            switch (_attrib)
            {
                case Attrib.Archive:
                    return FileAttributes.Archive;
                case Attrib.Compressed:
                    return FileAttributes.Compressed;
                case Attrib.Device:
                    return FileAttributes.Device;
                case Attrib.Directory:
                    return FileAttributes.Directory;
                case Attrib.Encrypted:
                    return FileAttributes.Encrypted;
                case Attrib.Hidden:
                    return FileAttributes.Hidden;
                case Attrib.IntegrityStream:
                    return FileAttributes.IntegrityStream;
                case Attrib.NoScrubData:
                    return FileAttributes.NoScrubData;
                case Attrib.NotContentIndexed:
                    return FileAttributes.NotContentIndexed;
                case Attrib.Offline:
                    return FileAttributes.Offline;
                case Attrib.ReadOnly:
                    return FileAttributes.ReadOnly;
                case Attrib.ReparsePoint:
                    return FileAttributes.ReparsePoint;
                case Attrib.SparseFile:
                    return FileAttributes.SparseFile;
                case Attrib.System:
                    return FileAttributes.System;
                case Attrib.Temporary:
                    return FileAttributes.Temporary;
                default:
                    return FileAttributes.Normal;
            }
        }

        public static bool MatchAttributes(string _path, FileAttributes _attrib)
        {
            try
            {
                FileAttributes attrib = File.GetAttributes(_path);
                return ((attrib & _attrib) != 0);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool MatchAttributes(string _path, Attrib _attrib) =>
            MatchAttributes(_path, GetAttrib(_attrib));

        public static void SetAttributes(string _path, FileAttributes _attrib)
        {
            try
            {
                if (IsDir(_path))
                {
                    DirectoryInfo dir = new DirectoryInfo(_path);
                    if (dir.Exists)
                    {
                        if (_attrib != FileAttributes.Normal)
                            dir.Attributes |= _attrib;
                        else
                            dir.Attributes = _attrib;
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(_path);
                    if (_attrib != FileAttributes.Normal)
                        file.Attributes |= _attrib;
                    else
                        file.Attributes = _attrib;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void SetAttributes(string _path, Attrib _attrib) =>
            SetAttributes(_path, GetAttrib(_attrib));

        public static bool IsDir(string _path) =>
            MatchAttributes(_path, FileAttributes.Directory);

        public static bool DirIsLink(string _dir) =>
            MatchAttributes(_dir, FileAttributes.ReparsePoint);

        public static void DirLink(string _destDir, string _srcDir, bool _backup)
        {
            if (!Directory.Exists(_srcDir))
                Directory.CreateDirectory(_srcDir);
            if (!Directory.Exists(_srcDir))
                return;
            if (_backup)
            {
                if (Directory.Exists(_destDir))
                {
                    if (!DirIsLink(_destDir))
                        Run.Cmd($"MOVE /Y \"{_destDir}\" \"{_destDir}.SI13N7-BACKUP\"");
                    else
                        DirUnLink(_destDir);
                }
            }
            if (Directory.Exists(_destDir))
                Run.Cmd($"RD /S /Q \"{_destDir}\"");
            if (Directory.Exists(_srcDir))
                Run.Cmd($"MKLINK /J \"{_destDir}\" \"{_srcDir}\" && ATTRIB +H \"{_destDir}\" /L");
        }

        public static void DirLink(string _srcDir, string _destDir) =>
            DirLink(_srcDir, _destDir, false);

        public static void DirUnLink(string _dir, bool _backup)
        {
            if (_backup)
            {
                if (Directory.Exists($"{_dir}.SI13N7-BACKUP"))
                {
                    if (Directory.Exists(_dir))
                        Run.Cmd($"RD /S /Q \"{_dir}\"");
                    Run.Cmd($"MOVE /Y \"{_dir}.SI13N7-BACKUP\" \"{_dir}\"");
                }
            }
            if (DirIsLink(_dir))
                Run.Cmd($"RD /S /Q \"{_dir}\"");
        }

        public static void DirUnLink(string _dir) =>
            DirUnLink(_dir, false);

        public static bool DirCopy(string _srcDir, string _destDir, bool _subDirs)
        {
            try
            {
                DirectoryInfo srcDir = new DirectoryInfo(_srcDir);
                if (!srcDir.Exists)
                    throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {_srcDir}");
                if (!Directory.Exists(_destDir))
                    Directory.CreateDirectory(_destDir);
                foreach (FileInfo f in srcDir.GetFiles())
                    f.CopyTo(Path.Combine(_destDir, f.Name), false);
                if (_subDirs)
                    foreach (DirectoryInfo d in srcDir.GetDirectories())
                        DirCopy(d.FullName, Path.Combine(_destDir, d.Name), _subDirs);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static void DirCopy(string _srcDir, string _destDir) =>
            DirCopy(_srcDir, _destDir, true);

        public static void SafeMove(string _srcDir, string _destDir)
        {
            bool copyDone = DirCopy(_srcDir, _destDir, true);
            try
            {
                if (copyDone)
                    Directory.Delete(_srcDir, true);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }
    }
}

#endregion
