
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <para><see cref="SilDev.RUN"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class DATA
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string dllName);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

            [DllImport("ntdll.dll")]
            internal static extern uint NtQueryInformationProcess([In] IntPtr ProcessHandle, [In] int ProcessInformationClass, [Out] out PROCESS_BASIC_INFORMATION ProcessInformation, [In] int ProcessInformationLength, [Out] [Optional] out int ReturnLength);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
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
                string shortcutPath = PATH.Combine(!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? $"{path}.lnk" : path);
                if (!Directory.Exists(Path.GetDirectoryName(shortcutPath)) || !File.Exists(PATH.Combine(target)))
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
                LOG.Debug(ex);
            }
            return false;
        }

        public static bool CreateShortcut(string target, string path, string args, bool skipExists) =>
            CreateShortcut(target, path, args, null, skipExists);

        public static bool CreateShortcut(string target, string path, bool skipExists) =>
            CreateShortcut(target, path, null, null, skipExists);

        public static string GetShortcutTarget(string path)
        {
            try
            {
                if (!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException();
                string targetPath;
                using (FileStream fs = File.Open(PATH.Combine(path), FileMode.Open, FileAccess.Read))
                {
                    BinaryReader br = new BinaryReader(fs);
                    fs.Seek(0x14, SeekOrigin.Begin);
                    uint flags = br.ReadUInt32();
                    if ((flags & 1) == 1)
                    {
                        fs.Seek(0x4C, SeekOrigin.Begin);
                        fs.Seek(br.ReadUInt16(), SeekOrigin.Current);
                    }
                    long start = fs.Position;
                    uint length = br.ReadUInt32();
                    fs.Seek(0xC, SeekOrigin.Current);
                    fs.Seek(start + br.ReadUInt32(), SeekOrigin.Begin);
                    targetPath = new string(br.ReadChars((int)(start + length - fs.Position - 2)));
                    int begin = targetPath.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = targetPath.IndexOf(string.Format("{0}{0}", Path.DirectorySeparatorChar), begin + 2) + 2;
                        end = targetPath.IndexOf('\0', end) + 1;
                        targetPath = Path.Combine(targetPath.Substring(0, begin), targetPath.Substring(end));
                    }
                }
                return targetPath;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string orgImagePathName = null;
        private static int unicodeSize = IntPtr.Size * 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public uint ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public int BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        private static PROCESS_BASIC_INFORMATION GetBasicInformation()
        {
            uint status;
            PROCESS_BASIC_INFORMATION pbi;
            int retLen;
            var handle = Process.GetCurrentProcess().Handle;
            if ((status = SafeNativeMethods.NtQueryInformationProcess(handle, 0, out pbi, Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION)), out retLen)) >= 0xc0000000)
                throw new Exception($"{new Exception().Message} (Status: '{status}')");
            return pbi;
        }

        private static void GetPointers(out IntPtr imageOffset, out IntPtr imageBuffer)
        {
            IntPtr pebBaseAddress = GetBasicInformation().PebBaseAddress;
            IntPtr processParameters = Marshal.ReadIntPtr(pebBaseAddress, 4 * IntPtr.Size);
            imageOffset = processParameters.Increment(4 * 4 + 5 * IntPtr.Size + unicodeSize + IntPtr.Size + unicodeSize);
            imageBuffer = Marshal.ReadIntPtr(imageOffset, IntPtr.Size);
        }

        private static IntPtr Increment(this IntPtr ptr, int value)
        {
            unchecked
            {
                if (IntPtr.Size == sizeof(int))
                    return new IntPtr(ptr.ToInt32() + value);
                return new IntPtr(ptr.ToInt64() + value);
            }
        }

        internal static void ChangeImagePathName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException();
                IntPtr imageOffset, imageBuffer;
                GetPointers(out imageOffset, out imageBuffer);
                short imageLen = Marshal.ReadInt16(imageOffset);
                if (string.IsNullOrEmpty(orgImagePathName))
                    orgImagePathName = Marshal.PtrToStringUni(imageBuffer, imageLen / 2);
                string newImagePathName = Path.Combine(Path.GetDirectoryName(orgImagePathName), name);
                if (newImagePathName.Length > orgImagePathName.Length)
                    throw new ArgumentException("New image path name cannot be longer than the original one.");
                IntPtr ptr = imageBuffer;
                foreach (char c in newImagePathName)
                {
                    Marshal.WriteInt16(ptr, c);
                    ptr = ptr.Increment(2);
                }
                Marshal.WriteInt16(ptr, 0);
                Marshal.WriteInt16(imageOffset, (short)(newImagePathName.Length * 2));
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        internal static void RestoreImagePathName()
        {
            try
            {
                if (string.IsNullOrEmpty(orgImagePathName))
                    throw new InvalidOperationException();
                IntPtr imageOffset, ptr;
                GetPointers(out imageOffset, out ptr);
                foreach (char c in orgImagePathName)
                {
                    Marshal.WriteInt16(ptr, c);
                    ptr = ptr.Increment(2);
                }
                Marshal.WriteInt16(ptr, 0);
                Marshal.WriteInt16(imageOffset, (short)(orgImagePathName.Length * 2));
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        private static bool PinUnpinTaskbar(string path, bool pin)
        {
            try
            {
                if (!File.Exists(PATH.Combine(path)))
                    throw new FileNotFoundException();
                if (Environment.OSVersion.Version.Major >= 10)
                    ChangeImagePathName("explorer.exe");
                StringBuilder sb = new StringBuilder(255);
                IntPtr hDll = SafeNativeMethods.LoadLibrary("shell32.dll");
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
                LOG.Debug(ex);
                return false;
            }
            finally
            {
                if (File.Exists(path) && Environment.OSVersion.Version.Major >= 10)
                    RestoreImagePathName();
            }
        }

        public static bool PinToTaskbar(string path) =>
            PinUnpinTaskbar(path, true);

        public static bool UnpinFromTaskbar(string path) =>
            PinUnpinTaskbar(path, false);

        public static bool MatchAttributes(string path, FileAttributes attr)
        {
            try
            {
                string src = PATH.Combine(path);
                FileAttributes fa = File.GetAttributes(src);
                return ((fa & attr) != 0);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool DirOrFileIsLink(this string path) =>
            MatchAttributes(path, FileAttributes.ReparsePoint);

        public static bool DirIsLink(string path) =>
            path.DirOrFileIsLink();

        public static bool FileIsLink(string path) =>
            path.DirOrFileIsLink();

        public static bool IsDir(string path) =>
            MatchAttributes(path, FileAttributes.Directory);

        public static void SetAttributes(string path, FileAttributes attr)
        {
            try
            {
                string src = PATH.Combine(path);
                if (IsDir(src))
                {
                    DirectoryInfo di = new DirectoryInfo(src);
                    if (di.Exists)
                    {
                        if (attr != FileAttributes.Normal)
                            di.Attributes |= attr;
                        else
                            di.Attributes = attr;
                    }
                }
                else
                {
                    FileInfo fi = new FileInfo(src);
                    if (attr != FileAttributes.Normal)
                        fi.Attributes |= attr;
                    else
                        fi.Attributes = attr;
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        private static void Linker(string linkPath, string destPath, bool destIsDir, bool backup = false, bool elevated = false, int? waitForExit = null)
        {
            string dest = PATH.Combine(destPath);
            try
            {
                if (destIsDir)
                {
                    if (!Directory.Exists(dest))
                        Directory.CreateDirectory(dest);
                }
                else
                {
                    if (!File.Exists(dest))
                        File.Create(dest).Close();
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return;
            }
            string link = PATH.Combine(linkPath);
            try
            {
                string linkDir = Path.GetDirectoryName(link);
                if (!Directory.Exists(linkDir))
                    Directory.CreateDirectory(linkDir);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return;
            }
            string cmd = string.Empty;
            if (backup)
            {
                if (link.DirOrFileExists())
                {
                    if (!DirIsLink(link))
                        cmd += $"MOVE /Y \"{link}\" \"{link}.SI13N7-BACKUP\"";
                    else
                        UnLinker(link, true, backup, elevated);
                }
            }
            if (link.DirOrFileExists())
            {
                if (!string.IsNullOrEmpty(cmd))
                    cmd += " && ";
                cmd += $"{(destIsDir ? "RD /S /Q" : "DEL / F / Q")} \"{link}\"";
            }
            if (dest.DirOrFileExists())
            {
                if (!string.IsNullOrEmpty(cmd))
                    cmd += " && ";
                cmd += $"MKLINK {(destIsDir ? "/J " : string.Empty)}\"{link}\" \"{dest}\" && ATTRIB +H \"{link}\" /L";
            }
            if (!string.IsNullOrEmpty(cmd))
                RUN.Cmd(cmd, elevated, waitForExit);
        }

        public static void DirLink(string linkPath, string destDir, bool backup = false, bool elevated = false, int? waitForExit = null) =>
            Linker(linkPath, destDir, true, backup, elevated, waitForExit);

        public static void DirLink(string linkPath, string destDir, bool backup, int? waitForExit) =>
            Linker(linkPath, destDir, true, backup, false, waitForExit);

        public static void DirLink(string linkPath, string destDir, int? waitForExit) =>
            Linker(linkPath, destDir, true, false, false, waitForExit);

        public static void FileLink(string linkPath, string destDir, bool backup = false, bool elevated = false, int? waitForExit = null) =>
            Linker(linkPath, destDir, false, backup, elevated, waitForExit);

        public static void FileLink(string linkPath, string destDir, bool backup, int? waitForExit) =>
            Linker(linkPath, destDir, false, backup, false, waitForExit);

        public static void FileLink(string linkPath, string destDir, int? waitForExit) =>
            Linker(linkPath, destDir, false, false, false, waitForExit);

        private static void UnLinker(string path, bool pathIsDir, bool backup = false, bool elevated = false, int? waitForExit = null)
        {
            string link = PATH.Combine(path);
            string cmd = string.Empty;
            if (backup)
            {
                if ($"{link}.SI13N7-BACKUP".DirOrFileExists())
                {
                    if (link.DirOrFileExists())
                        cmd += $"{(pathIsDir ? "RD /S /Q" : "DEL / F / Q")} \"{link}\"";
                    if (!string.IsNullOrEmpty(cmd))
                        cmd += " && ";
                    cmd += $"MOVE /Y \"{link}.SI13N7-BACKUP\" \"{link}\"";
                }
            }
            if (link.DirOrFileIsLink())
            {
                if (!string.IsNullOrEmpty(cmd))
                    cmd += " && ";
                cmd += $"{(pathIsDir ? "RD /S /Q" : "DEL /F /Q /A:L")} \"{link}\"";
            }
            if (!string.IsNullOrEmpty(cmd))
                RUN.Cmd(cmd, elevated, waitForExit);
        }

        public static void DirUnLink(string dir, bool backup = false, bool elevated = false, int? waitForExit = null) =>
            UnLinker(dir, true, backup, elevated, waitForExit);

        public static void DirUnLink(string dir, bool backup, int? waitForExit) =>
            UnLinker(dir, true, backup, false, waitForExit);

        public static void DirUnLink(string dir, int? waitForExit) =>
           UnLinker(dir, true, false, false, waitForExit);

        public static void FileUnLink(string dir, bool backup = false, bool elevated = false, int? waitForExit = null) =>
            UnLinker(dir, false, backup, elevated, waitForExit);

        public static void FileUnLink(string dir, bool backup, int? waitForExit) =>
            UnLinker(dir, false, backup, false, waitForExit);

        public static void FileUnLink(string dir, int? waitForExit) =>
            UnLinker(dir, false, false, false, waitForExit);

        public static bool DirCopy(string srcDir, string destDir, bool subDirs = true)
        {
            try
            {
                string src = PATH.Combine(srcDir);
                DirectoryInfo di = new DirectoryInfo(srcDir);
                if (!di.Exists)
                    throw new DirectoryNotFoundException();
                string dest = PATH.Combine(destDir);
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);
                foreach (FileInfo f in di.GetFiles())
                    f.CopyTo(Path.Combine(dest, f.Name), false);
                if (subDirs)
                {
                    foreach (DirectoryInfo d in di.GetDirectories())
                        DirCopy(d.FullName, Path.Combine(dest, d.Name), subDirs);
                }
                return true;
            }
            catch (Exception ex)
            {
                LOG.Debug($"{ex.Message} (Source: '{srcDir}'; Destination: '{destDir}')", ex.StackTrace);
                return false;
            }
        }

        public static void DirSafeMove(string srcDir, string destDir)
        {
            try
            {
                if (DirCopy(srcDir, destDir, true))
                {
                    string src = PATH.Combine(srcDir);
                    string dest = PATH.Combine(destDir);
                    if (new DirectoryInfo(src).GetFullHashCode() != new DirectoryInfo(dest).GetFullHashCode())
                        throw new AggregateException();
                    Directory.Delete(src, true);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static long GetFullHashCode(this DirectoryInfo dirInfo)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                long len = 0;
                foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    sb.Append(fi.Name);
                    len += fi.Length;
                }
                return $"{len}{sb}".GetHashCode();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return $"{new Random().Next(int.MinValue, int.MaxValue)}".GetHashCode();
            }
        }

        public static long GetSize(this DirectoryInfo dirInfo)
        {
            long size = 0;
            try
            {
                foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    size += fi.Length;
            }
            catch
            {
                size = -1;
            }
            return size;
        }

        public static long GetSize(this FileInfo fileInfo)
        {
            long size = 0;
            try
            {
                size = fileInfo.Length;
            }
            catch
            {
                size = -1;
            }
            return size;
        }
    }
}

#endregion
