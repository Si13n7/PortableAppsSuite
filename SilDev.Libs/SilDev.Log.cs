
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class LOG
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int GetPrivateProfileInt([MarshalAs(UnmanagedType.LPStr)]string lpApplicationName, [MarshalAs(UnmanagedType.LPStr)]string lpKeyName, int nDefault, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", EntryPoint = "AllocConsole", CharSet = CharSet.Unicode)]
            internal static extern int AllocConsole();

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll")]
            internal static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        }

        private static object ConLock = new object();

        internal static string AssemblyPath
        {
            get
            {
                try
                {
                    return Path.GetFullPath(Assembly.GetEntryAssembly().GetName().CodeBase.Substring(8));
                }
                catch
                {
                    return AssemblyLocation;
                }
            }
        }
        internal static readonly string AssemblyName = Assembly.GetEntryAssembly().GetName().Name;
        internal static readonly string AssemblyLocation = Assembly.GetEntryAssembly().Location;
        internal static readonly Version AssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;

        public static System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();

        public static readonly string ConsoleTitle = $"Debug Console ('{AssemblyName}')";

        public static int DebugMode { get; private set; } = 0;

        private static bool IsRunning = false, FirstCall = false, FirstEntry = false;
        private static IntPtr stdHandle = IntPtr.Zero;
        private static SafeFileHandle sfh = null;
        private static FileStream fs = null;
        private static StreamWriter sw = null;

        public static string FileName { get; private set; } = $"{AssemblyName}_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
        public static string FileDir { get; set; } = Path.GetTempPath();
        public static string FilePath { get; private set; } = Path.Combine(FileDir, FileName);

        private static string AsciiLogo
        {
            get
            {
                string s = "2020205f5f5f5f5f5f5f5f5f2e5f5f20205f5f5f5f205f5f" +
                           "5f5f5f5f5f5f20202020202020205f5f5f5f5f5f5f5f5f0a" +
                           "20202f2020205f5f5f5f5f2f7c5f5f7c2f5f2020207c5c5f" +
                           "5f5f5f5f20205c2020205f5f5f5f5c5f5f5f5f5f5f20205c" +
                           "0a20205c5f5f5f5f5f20205c207c20207c207c2020207c20" +
                           "205f285f5f20203c20202f202020205c2020202f20202020" +
                           "2f0a20202f20202020202020205c7c20207c207c2020207c" +
                           "202f202020202020205c7c2020207c20205c202f20202020" +
                           "2f0a202f5f5f5f5f5f5f5f20202f7c5f5f7c207c5f5f5f7c" +
                           "2f5f5f5f5f5f5f20202f7c5f5f5f7c20202f2f5f5f5f5f2f" +
                           "0a2020202020202020205c2f202020202020202020202020" +
                           "2020202020205c2f2020202020205c2f2020202020202020";
                return s.FromHexString();
            }
        }

        private static string ConsoleText
        {
            get
            {
                string s = "202020202020202020202044204520422055204720202020" +
                           "43204f204e2053204f204c20452020202020202020202020";
                return s.FromHexString();
            }
        }

        public static void ActivateDebug(int mode = 1)
        {
            DebugMode = mode;
            if (!FirstCall)
            {
                FirstCall = true;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => Debug(e.Exception, true, true);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => Debug(new ApplicationException(), true, true);
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
                if (DebugMode > 0)
                {
                    try
                    {
                        FileDir = Path.GetFullPath(FileDir);
                        if (!Directory.Exists(FileDir))
                            Directory.CreateDirectory(FileDir);
                    }
                    catch
                    {
                        FileDir = Path.GetTempPath();
                    }
                    finally
                    {
                        FilePath = Path.Combine(FileDir, FileName);
                    }
                }
            }
        }

        public static void AllowDebug(string iniPath = null, string section = null)
        {
            int mode = 0;
            if (!string.IsNullOrEmpty(iniPath) && !string.IsNullOrEmpty(section))
            {
                if (File.Exists(iniPath))
                    mode = SafeNativeMethods.GetPrivateProfileInt(section, "Debug", 0, iniPath);
            }
            if (new Regex("/debug [0-2]|/debug \"[0-2]\"").IsMatch(Environment.CommandLine))
            {
                int i;
                if (int.TryParse(new Regex("/debug ([0-2]?)").Match(Environment.CommandLine.RemoveChar('\"')).Groups[1].ToString(), out i) && i > mode)
                    mode = i;
            }
            ActivateDebug(mode);
        }

        public static void Debug(string exMsg, string exTra = null, bool exit = false)
        {
            if (!FirstCall || DebugMode < 1 || string.IsNullOrEmpty(exMsg))
                return;

            if (!FirstEntry)
            {
                FirstEntry = true;
                if (!File.Exists(FilePath))
                {
                    try
                    {
                        if (!Directory.Exists(FileDir))
                            Directory.CreateDirectory(FileDir);
                        File.Create(FilePath).Close();
                    }
                    catch (Exception ex)
                    {
                        if (DebugMode > 1)
                        {
                            DebugMode = 3;
                            Debug(ex);
                        }
                    }
                }
                Debug("***Logging has been started***", $"'{Environment.OSVersion}' - '{AssemblyName}' - '{AssemblyVersion}' - '{FilePath}'");
            }
            if (!File.Exists(FilePath) && DebugMode < 1)
                return;

            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz");
            string exmsg = $"Time:  {date}\r\nMsg:   {Filter(exMsg)}\r\n";
            if (!string.IsNullOrWhiteSpace(exTra))
            {
                string extra = Filter(exTra);
                extra = extra.Replace(Environment.NewLine, " - ");
                exmsg += $"Trace: {extra}\r\n";
            }

            if (DebugMode < 3 && File.Exists(FilePath))
            {
                try
                {
                    File.AppendAllText(FilePath, $"{exmsg}\r\n");
                }
                catch
                {
                    exit = true;
                }
            }

            if (DebugMode > 1)
            {
                try
                {
                    lock (ConLock)
                    {
                        if (!IsRunning)
                        {
                            SafeNativeMethods.AllocConsole();
                            SafeNativeMethods.DeleteMenu(SafeNativeMethods.GetSystemMenu(SafeNativeMethods.GetConsoleWindow(), false), 0xF060, 0x0);
                            stdHandle = SafeNativeMethods.GetStdHandle(-11);
                            sfh = new SafeFileHandle(stdHandle, true);
                            fs = new FileStream(sfh, FileAccess.Write);
                            if (Console.Title != ConsoleTitle)
                            {
                                Console.Title = ConsoleTitle;
                                Console.BufferHeight = short.MaxValue - 1;
                                Console.BufferWidth = Console.WindowWidth;
                                Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(AsciiLogo);
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(ConsoleText);
                                Console.ResetColor();
                                Console.WriteLine();
                            }
                            IsRunning = true;
                        }
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(new string('-', Console.BufferWidth - 1));
                        foreach (string line in exmsg.SplitNewLine())
                        {
                            string[] sa = line.Split(' ');
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(sa[0]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($" {sa.Skip(1).ToArray().Join(" ")}");
                        }
                        Console.ResetColor();
                        sw = new StreamWriter(fs, Encoding.ASCII) { AutoFlush = true };
                        Console.SetOut(sw);
                    }
                }
                catch (Exception ex)
                {
                    DebugMode = 1;
                    Debug(ex);
                }
            }

            if (exit)
            {
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }

        public static void Debug(Exception ex, bool forceLogging = false, bool exit = false)
        {
            if (DebugMode < 1)
            {
                if (!forceLogging)
                    return;
                DebugMode = 1;
            }
            Debug(ex.Message, ex.StackTrace, exit);
        }

        private static string Filter(string input)
        {
            try
            {
                string s = input.SplitNewLine().Join(" - ");
                s = Regex.Replace(s.Trim(), " {2,}", " ", RegexOptions.Singleline);
                s = $"{char.ToUpper(s[0])}{s.Substring(1)}";
                return s;
            }
            catch
            {
                return input;
            }
        }

        private static void Close()
        {
            if (sfh != null && !sfh.IsClosed)
                sfh.Close();
            if (Directory.Exists(FileDir))
            {
                try
                {
                    foreach (string file in Directory.GetFiles(FileDir, $"{AssemblyName}*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (FilePath.ToLower() == file.ToLower())
                            continue;
                        if ((DateTime.Now - new FileInfo(file).LastWriteTime).TotalDays >= 7d)
                            File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Debug(ex);
                }
            }
        }
    }
}

#endregion
