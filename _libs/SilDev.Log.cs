
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;
using System.Windows.Forms;

namespace SilDev
{
    public static class Log
    {
        private readonly static string ProcName = Process.GetCurrentProcess().ProcessName;
        private readonly static string ProcVer = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).ProductVersion.Replace(".", string.Empty);

        public static string ConsoleTitle { get; } = $"Debug Console ('{ProcName}')";

        public static int DebugMode { get; private set; }

        private static bool IsRunning = false, FirstCall = false, FirstEntry = false;
        private static IntPtr stdHandle = IntPtr.Zero;
        private static SafeFileHandle sfh = null;
        private static FileStream fs = null;
        private static StreamWriter sw = null;

        private readonly static string FileName = $"{ProcVer}-{DateTime.Now.ToString("yyMMddHHmmssfff")}.log";
        public static string FileLocation { get; set; } = Environment.GetEnvironmentVariable("TEMP");
        public static string FilePath { get; private set; } = Path.Combine(FileLocation, FileName);

        public static void ActivateDebug(int _option)
        {
            DebugMode = _option;
            if (!FirstCall)
            {
                try
                {
                    FileLocation = Path.Combine(FileLocation, ProcName);
                    if (!Directory.Exists(FileLocation))
                        Directory.CreateDirectory(FileLocation);
                    Path.GetFullPath(FileLocation);
                    if (!Elevation.WritableLocation(FileLocation))
                        throw new InvalidOperationException();
                }
                catch
                {
                    FileLocation = Path.Combine(Environment.GetEnvironmentVariable("TEMP"));
                }
                FilePath = Path.Combine(FileLocation, FileName);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
                Application.ThreadException += (s, e) => Debug(e.Exception);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => Debug(new ApplicationException());
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
                FirstCall = true;
            }
        }

        public static void ActivateDebug() =>
            ActivateDebug(2);

        public static void AllowDebug()
        {
            DebugMode = 0;
            if (new Regex("/debug [0-2]|/debug \"[0-2]\"").IsMatch(Environment.CommandLine))
            {
                int option = 0;
                if (int.TryParse(new Regex("/debug ([0-2]?)").Match(Environment.CommandLine.Replace("\"", string.Empty)).Groups[1].ToString(), out option))
                    ActivateDebug(option);
            }
        }

        public static void Debug(string _msg, string _trace)
        {
            if (DebugMode < 1)
                return;

            if (!File.Exists(FilePath) && !FirstEntry)
            {
                FirstEntry = true;
                Debug($"Create '{FilePath}'");
            }

            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz");
            string msg = string.Empty;
            msg += $"Time:  {date}{Environment.NewLine}";
            msg += $"Msg:   {_msg}{Environment.NewLine}";
            string trace = null;
            if (!string.IsNullOrWhiteSpace(_trace))
            {
                trace = _trace.Trim();
                trace = trace.Replace(Environment.NewLine, " - ");
                trace = $"{trace[0].ToString().ToUpper()}{trace.Substring(1)}";
            }
            if (!string.IsNullOrWhiteSpace(trace))
                msg += $"Trace: {trace}{Environment.NewLine}";

            string log = $"{msg}{Environment.NewLine}";
            try
            {
                if (!File.Exists(FilePath))
                    File.WriteAllText(FilePath, log);
                else
                    File.AppendAllText(FilePath, log);
            }
            catch (Exception ex)
            {
                string exFile = $"{new Random().Next(0, short.MaxValue)}-{FilePath}";
                string exMsg = $"{msg}Msg2:  {ex.Message}{Environment.NewLine}";
                if (!File.Exists(FilePath))
                    File.WriteAllText(exFile, exMsg);
                else
                    File.AppendAllText(exFile, exMsg);
            }

            if (DebugMode > 1)
            {
                try
                {
                    if (!IsRunning)
                    {
                        WinAPI.SafeNativeMethods.AllocConsole();
                        stdHandle = WinAPI.SafeNativeMethods.GetStdHandle(-11);
                        sfh = new SafeFileHandle(stdHandle, true);
                        fs = new FileStream(sfh, FileAccess.Write);
                        if (Console.Title != ConsoleTitle)
                        {
                            Console.Title = ConsoleTitle;
                            Console.BufferHeight = short.MaxValue - 1;
                            Console.BufferWidth = Console.WindowWidth;
                            Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(Crypt.Base64.Decrypt("ICAgX19fX19fX19fLl9fICBfX19fIF9fX19fX19fICAgICAgICBfX19fX19fX18gDQogIC8gICBfX19fXy98X198L18gICB8XF9fX19fICBcICAgX19fX1xfX19fX18gIFwNCiAgXF9fX19fICBcIHwgIHwgfCAgIHwgIF8oX18gIDwgIC8gICAgXCAgIC8gICAgLw0KICAvICAgICAgICBcfCAgfCB8ICAgfCAvICAgICAgIFx8ICAgfCAgXCAvICAgIC8gDQogL19fX19fX18gIC98X198IHxfX198L19fX19fXyAgL3xfX198ICAvL19fX18vICANCiAgICAgICAgIFwvICAgICAgICAgICAgICAgICAgXC8gICAgICBcLyAgICAgICAgIA=="));
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("           D E B U G    C O N S O L E");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        IsRunning = true;
                    }
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(new string('-', Console.BufferWidth - 1));
                    foreach (string line in msg.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        string[] words = line.Split(' ');
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(words[0]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($" {string.Join(" ", words.Skip(1).ToArray())}");
                    }
                    Console.ResetColor();
                    sw = new StreamWriter(fs, Encoding.ASCII) { AutoFlush = true };
                    Console.SetOut(sw);
                }
                catch (Exception ex)
                {
                    DebugMode = 1;
                    Debug(ex);
                }
            }
        }

        public static void Debug(string _msg)
        {
            if (DebugMode < 1)
                return;
            Debug(_msg, null);
        }

        public static void Debug(Exception _ex)
        {
            if (DebugMode < 1)
                return;
            string msg = null;
            string trace = null;
            try
            {
                msg = string.Join(" - ", _ex.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                msg = Regex.Replace(msg.Trim(), @"\s+", " ", RegexOptions.Singleline);
                trace = string.Join(" - ", _ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                trace = Regex.Replace(trace.Trim(), @"\s+", " ", RegexOptions.Singleline);
            }
            catch
            {
                msg = _ex.Message;
                trace = _ex.StackTrace;
            }
            Debug(msg, trace);
        }

        private static void Close()
        {
            try
            {
                if (sfh != null && !sfh.IsClosed)
                    sfh.Close();
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
            try
            {
                foreach (string file in Directory.GetFiles(FileLocation, "*.log", SearchOption.TopDirectoryOnly))
                    if ((DateTime.Now - new FileInfo(file).LastWriteTime).TotalDays >= 7d)
                        File.Delete(file);
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
        }
    }
}

#endregion
