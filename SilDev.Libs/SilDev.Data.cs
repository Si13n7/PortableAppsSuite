
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Data
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string dllName);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);
        }

        private static bool PinUnpinTaskbar(string path, bool pin)
        {
            try
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                if (Environment.OSVersion.Version.Major >= 10)
                    throw new PlatformNotSupportedException();
                StringBuilder sb = new StringBuilder(255);
                IntPtr hDll = SafeNativeMethods.LoadLibrary("Shell32.dll");
                SafeNativeMethods.LoadString(hDll, (uint)(pin ? 5386 : 5387), sb, 255);
                dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                dynamic dir = shell.NameSpace(Path.GetDirectoryName(path));
                dynamic link = dir.ParseName(Path.GetFileName(path));
                string verb = sb.ToString();
                dynamic verbs = link.Verbs();
                for (int i = 0; i < verbs.Count(); i++)
                {
                    dynamic d = verbs.Item(i);
                    if (pin && d.Name.Equals(verb) || !pin && d.Name.Contains(verb))
                    {
                        d.DoIt();
                        break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool PinToTaskbar(string path) =>
            PinUnpinTaskbar(path, true);

        public static bool UnpinFromTaskbar(string path) =>
            PinUnpinTaskbar(path, false);

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)]string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)]string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)]string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)]string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)]string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)]string pszFile);
        }

        public static bool CreateShortcut(string target, string path, string args = null, string icon = null, bool skipExists = false)
        {
            try
            {
                string shortcutPath = path;
                shortcutPath = Run.EnvironmentVariableFilter(!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? $"{path}.lnk" : path);
                if (!Directory.Exists(Path.GetDirectoryName(shortcutPath)) || !File.Exists(target))
                    return false;
                if (File.Exists(shortcutPath))
                {
                    if (skipExists)
                        return true;
                    File.Delete(shortcutPath);
                }
                IShellLink shell = (IShellLink)new ShellLink();
                if (!string.IsNullOrWhiteSpace(args))
                    shell.SetArguments(args);
                shell.SetDescription(string.Empty);
                shell.SetPath(target);
                shell.SetIconLocation(icon ?? target, 0);
                shell.SetWorkingDirectory(Path.GetDirectoryName(target));
                ((IPersistFile)shell).Save(shortcutPath, false);
                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return false;
        }

        public static bool CreateShortcut(string target, string path, string args, bool skipExists) =>
            CreateShortcut(target, path, args, null, skipExists);

        public static bool CreateShortcut(string target, string path, bool skipExists) =>
            CreateShortcut(target, path, null, null, skipExists);

        public static bool MatchAttributes(string path, FileAttributes attr)
        {
            try
            {
                FileAttributes fa = File.GetAttributes(path);
                return ((fa & attr) != 0);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static void SetAttributes(string path, FileAttributes attr)
        {
            try
            {
                if (IsDir(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    if (dir.Exists)
                    {
                        if (attr != FileAttributes.Normal)
                            dir.Attributes |= attr;
                        else
                            dir.Attributes = attr;
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    if (attr != FileAttributes.Normal)
                        file.Attributes |= attr;
                    else
                        file.Attributes = attr;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static bool IsDir(string path) =>
            MatchAttributes(path, FileAttributes.Directory);

        public static bool DirIsLink(string dir) =>
            MatchAttributes(dir, FileAttributes.ReparsePoint);

        public static bool FileIsLink(string file) =>
            MatchAttributes(file, FileAttributes.ReparsePoint);

        public static void DirLink(string destDir, string srcDir, bool backup = false)
        {
            if (!Directory.Exists(srcDir))
                Directory.CreateDirectory(srcDir);
            if (!Directory.Exists(srcDir))
                return;
            if (backup)
            {
                if (Directory.Exists(destDir))
                {
                    if (!DirIsLink(destDir))
                        Run.Cmd($"MOVE /Y \"{destDir}\" \"{destDir}.SI13N7-BACKUP\"");
                    else
                        DirUnLink(destDir);
                }
            }
            if (Directory.Exists(destDir))
                Run.Cmd($"RD /S /Q \"{destDir}\"");
            if (Directory.Exists(srcDir))
                Run.Cmd($"MKLINK /J \"{destDir}\" \"{srcDir}\" && ATTRIB +H \"{destDir}\" /L");
        }

        public static void DirUnLink(string dir, bool backup = false)
        {
            if (backup)
            {
                if (Directory.Exists($"{dir}.SI13N7-BACKUP"))
                {
                    if (Directory.Exists(dir))
                        Run.Cmd($"RD /S /Q \"{dir}\"");
                    Run.Cmd($"MOVE /Y \"{dir}.SI13N7-BACKUP\" \"{dir}\"");
                }
            }
            if (DirIsLink(dir))
                Run.Cmd($"RD /S /Q \"{dir}\"");
        }

        public static bool DirCopy(string srcDir, string destDir, bool subDirs = true)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(srcDir);
                if (!di.Exists)
                    throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {di}");
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
                foreach (FileInfo f in di.GetFiles())
                    f.CopyTo(Path.Combine(destDir, f.Name), false);
                if (subDirs)
                    foreach (DirectoryInfo d in di.GetDirectories())
                        DirCopy(d.FullName, Path.Combine(destDir, d.Name), subDirs);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static void DirSafeMove(string srcDir, string destDir)
        {
            try
            {
                bool copyDone = DirCopy(srcDir, destDir, true);
                if (copyDone)
                    Directory.Delete(srcDir, true);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void FileLink(string destFile, string srcFile, bool backup = false)
        {
            if (!File.Exists(srcFile))
                File.Create(srcFile).Close();
            if (!File.Exists(srcFile))
                return;
            if (backup)
            {
                if (File.Exists(destFile))
                {
                    if (!DirIsLink(destFile))
                        Run.Cmd($"MOVE /Y \"{destFile}\" \"{destFile}.SI13N7-BACKUP\"");
                    else
                        FileUnLink(destFile);
                }
            }
            if (File.Exists(destFile))
                Run.Cmd($"DEL /F /Q \"{destFile}\"");
            if (File.Exists(srcFile))
                Run.Cmd($"MKLINK \"{destFile}\" \"{srcFile}\" && ATTRIB +H \"{destFile}\" /L");
        }

        public static void FileUnLink(string file, bool backup = false)
        {
            if (backup)
            {
                if (File.Exists($"{file}.SI13N7-BACKUP"))
                {
                    if (File.Exists(file))
                        Run.Cmd($"DEL /F /Q \"{file}\"");
                    Run.Cmd($"MOVE /Y \"{file}.SI13N7-BACKUP\" \"{file}\"");
                }
            }
            if (FileIsLink(file))
                Run.Cmd($"DEL /F /Q /A:L \"{file}\"");
        }
    }
}

#endregion
