
#region SILENT DEVELOPMENTS generated code

/************************************

#define WindowsScriptHostObjectModel

************************************/

using System;
using System.IO;

namespace SilDev
{
    public static class Data
    {
        #if WindowsScriptHostObjectModel

        public enum ShortcutWndStyle : int
        {
            Normal = 1,
            Maximized = 3,
            Minimized = 7
        }

        public static bool CreateShortcut(string _target, string _path, string _args, string _icon, ShortcutWndStyle _style, int _skipExists)
        {
            try
            {
                string shortcutPath = Run.EnvironmentVariableFilter(!_path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? string.Format("{0}.lnk", _path) : _path);
                IWshRuntimeLibrary.WshShell wshShell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut;
                shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(shortcutPath);
                if (!string.IsNullOrWhiteSpace(_args))
                    shortcut.Arguments = _args;
                if (File.Exists(shortcutPath))
                {
                    if (_skipExists > 0)
                        return _skipExists > 1 ? File.Exists(GetShortcutTarget(shortcutPath)) : true;
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

        public static bool CreateShortcut(string _target, string _path, string _args, ShortcutWndStyle _style, int _skipExists)
        {
            return CreateShortcut(_target, _path, null, _target, _style, _skipExists);
        }

        public static bool CreateShortcut(string _target, string _path, ShortcutWndStyle _style, int _skipExists)
        {
            return CreateShortcut(_target, _path, null, _target, _style, _skipExists);
        }

        public static bool CreateShortcut(string _target, string _path, string _args, int _skipExists)
        {
            return CreateShortcut(_target, _path, _args, _target, ShortcutWndStyle.Normal, _skipExists);
        }

        public static bool CreateShortcut(string _target, string _path, string _args)
        {
            return CreateShortcut(_target, _path, _args, _target, ShortcutWndStyle.Normal, 0);
        }

        public static bool CreateShortcut(string _target, string _path, int _skipExists)
        {
            return CreateShortcut(_target, _path, null, _target, ShortcutWndStyle.Normal, _skipExists);
        }

        public static bool CreateShortcut(string _target, string _path)
        {
            return CreateShortcut(_target, _path, null, _target, ShortcutWndStyle.Normal, 0);
        }

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

        public static bool IsDir(string _path)
        {
            try
            {
                FileAttributes attrib = File.GetAttributes(_path);
                return ((attrib & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public enum Attrib : int
        {
            Hidden = 0,
            ReadOnly = 10,
            Normal = 20,
        }

        private static FileAttributes GetAttrib(Attrib _attrib)
        {
            switch (_attrib)
            {
                case Attrib.Hidden:
                    return FileAttributes.Hidden;
                case Attrib.ReadOnly:
                    return FileAttributes.ReadOnly;
                default:
                    return FileAttributes.Normal;
            }
        }

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

        public static void SetAttributes(string _path, Attrib _attrib)
        {
            SetAttributes(_path, GetAttrib(_attrib));
        }

        public static bool DirIsLink(string _dir)
        {
            DirectoryInfo dir = new DirectoryInfo(_dir);
            return (dir.Exists && ((dir.Attributes & FileAttributes.ReparsePoint) != 0));
        }

        public static void DirLink(string _srcDir, string _destDir, bool _backup)
        {
            if (!Directory.Exists(_destDir))
                Directory.CreateDirectory(_destDir);
            if (Directory.Exists(_destDir))
            {
                if (_backup)
                {
                    if (Directory.Exists(_srcDir))
                    {

                        if (!DirIsLink(_srcDir))
                        {
                            Directory.Move(_srcDir, string.Format("{0}.SI13N7-BACKUP", _srcDir));
                            if (Directory.Exists(_srcDir))
                                Directory.Delete(_srcDir, true);
                        }
                        else
                            DirUnLink(_srcDir);
                    }
                }
                else
                {
                    if (Directory.Exists(_srcDir))
                        Directory.Delete(_srcDir, true);
                }
                Run.App(@"%WinDir%\System32", "cmd.exe", string.Format("/C MKLINK /J \"{1}\" \"{0}\" && ATTRIB +H \"{1}\" /L", _destDir, _srcDir), Run.WindowStyle.Hidden);
            }
        }

        public static void DirLink(string _srcDir, string _destDir)
        {
            DirLink(_srcDir, _destDir, false);
        }

        public static void DirUnLink(string _dir, bool _backup)
        {
            if (_backup)
            {
                if (Directory.Exists(string.Format("{0}.SI13N7-BACKUP", _dir)))
                {
                    if (Directory.Exists(_dir))
                        Directory.Delete(_dir, true);
                    Directory.Move(string.Format("{0}.SI13N7-BACKUP", _dir), _dir);
                }
            }
            if (DirIsLink(_dir))
                Directory.Delete(_dir);
        }

        public static void DirUnLink(string _dir)
        {
            DirUnLink(_dir, false);
        }

        public static bool DirCopy(string _srcDir, string _destDir, bool _subDirs)
        {
            try
            {
                DirectoryInfo srcDir = new DirectoryInfo(_srcDir);
                if (!srcDir.Exists)
                    throw new DirectoryNotFoundException(string.Format("Source directory does not exist or could not be found: {0}", _srcDir));
                if (!Directory.Exists(_destDir))
                    Directory.CreateDirectory(_destDir);
                foreach (FileInfo f in srcDir.GetFiles())
                    f.CopyTo(Path.Combine(_destDir, f.Name), false);
                if (_subDirs)
                {
                    foreach (DirectoryInfo d in srcDir.GetDirectories())
                        DirCopy(d.FullName, Path.Combine(_destDir, d.Name), _subDirs);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static void DirCopy(string _srcDir, string _destDir)
        {
            DirCopy(_srcDir, _destDir, true);
        }

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
