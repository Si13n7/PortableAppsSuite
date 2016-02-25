
#region SILENT DEVELOPMENTS generated code

using System;
using System.IO;

namespace SilDev
{
    /// <summary>
    /// To unlock shortcut functions:
    /// Define 'WindowsScriptHostObjectModel' for compiling and add the 'Windows Script Host Object Model' reference to your project.
    /// </summary>
    public static class Data
    {
        #if WindowsScriptHostObjectModel

        public enum ShortcutWndStyle : int
        {
            Normal = 1,
            Maximized = 3,
            Minimized = 7
        }

        public static bool CreateShortcut(string _target, string _path, string _args, string _icon, ShortcutWndStyle _style, bool _skipExists)
        {
            try
            {
                string shortcutPath = Run.EnvironmentVariableFilter(!_path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? $"{_path}.lnk" : _path);
                if (!Directory.Exists(Path.GetDirectoryName(shortcutPath)) || !File.Exists(_target))
                    return false;
                IWshRuntimeLibrary.WshShell wshShell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut;
                shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(shortcutPath);
                if (!string.IsNullOrWhiteSpace(_args))
                    shortcut.Arguments = _args;
                if (File.Exists(shortcutPath))
                {
                    if (_skipExists)
                        return true;
                    File.Delete(shortcutPath);
                }
                shortcut.IconLocation = _icon;
                shortcut.TargetPath = _target;
                shortcut.WorkingDirectory = Path.GetDirectoryName(shortcut.TargetPath);
                if (_style != ShortcutWndStyle.Normal)
                    shortcut.WindowStyle = (int)_style;
                shortcut.Save();
                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return false;
        }

        public static bool CreateShortcut(string _target, string _path, string _args, ShortcutWndStyle _style, bool _skipExists) => 
            CreateShortcut(_target, _path, null, _target, _style, _skipExists);

        public static bool CreateShortcut(string _target, string _path, ShortcutWndStyle _style, bool _skipExists) => 
            CreateShortcut(_target, _path, null, _target, _style, _skipExists);

        public static bool CreateShortcut(string _target, string _path, string _args, bool _skipExists) => 
            CreateShortcut(_target, _path, _args, _target, ShortcutWndStyle.Normal, _skipExists);

        public static bool CreateShortcut(string _target, string _path, string _args) => 
            CreateShortcut(_target, _path, _args, _target, ShortcutWndStyle.Normal, false);

        public static bool CreateShortcut(string _target, string _path, bool _skipExists) => 
            CreateShortcut(_target, _path, null, _target, ShortcutWndStyle.Normal, _skipExists);

        public static bool CreateShortcut(string _target, string _path) => 
            CreateShortcut(_target, _path, null, _target, ShortcutWndStyle.Normal, false);

        private static string GetShortcutTarget(string _path)
        {
            string dir = Path.GetDirectoryName(_path);
            string filename = Path.GetFileName(_path);
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(dir);
            Shell32.FolderItem item = folder.ParseName(filename);
            if (item != null)
            {
                if (item.IsLink)
                {
                    Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)item.GetLink;
                    return link.Path;
                }
                return _path;
            }
            return string.Empty;
        }

        #endif

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
