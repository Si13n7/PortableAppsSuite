
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

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
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.WinAPI"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Log
    {
        private readonly static string ProcName = Process.GetCurrentProcess().ProcessName;
        private readonly static string ProcVer = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).ProductVersion;

        public static string ConsoleTitle { get; } = $"Debug Console ('{ProcName}')";

        public static int DebugMode { get; private set; } = 0;

        private static bool IsRunning = false, FirstCall = false, FirstEntry = false;
        private static IntPtr stdHandle = IntPtr.Zero;
        private static SafeFileHandle sfh = null;
        private static FileStream fs = null;
        private static StreamWriter sw = null;

        public static string FileName { get; private set; } = $"{ProcName}.log";
        public static string FileLocation { get; set; } = Environment.GetEnvironmentVariable("TEMP");
        public static string FilePath { get; private set; } = Path.Combine(FileLocation, FileName);

        public static void ActivateDebug(int mode = 1)
        {
            DebugMode = mode;
            if (!FirstCall)
            {
                FirstCall = true;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => Debug(e.Exception, true);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => Debug(new ApplicationException(), true);
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
                try
                {
                    if (!Directory.Exists(FileLocation))
                        Directory.CreateDirectory(FileLocation);
                    Path.GetFullPath(FileLocation);
                    string version = ProcVer.Replace(".", string.Empty);
                    while (version.Length < 8)
                        version += "0";
                    FileName = $"{ProcName}-{version}.log";
                    FilePath = Path.Combine(FileLocation, FileName);
                }
                catch
                {
                    FileName = $"{ProcName}.log";
                    FileLocation = Environment.GetEnvironmentVariable("TEMP");
                    FilePath = Path.Combine(FileLocation, FileName);
                }
            }
        }

        public static void ActivateDebug() =>
            ActivateDebug(2);

        public static void AllowDebug()
        {
            int mode = 0;
            if (new Regex("/debug [0-2]|/debug \"[0-2]\"").IsMatch(Environment.CommandLine))
            {
                if (!int.TryParse(new Regex("/debug ([0-2]?)").Match(Environment.CommandLine.Replace("\"", string.Empty)).Groups[1].ToString(), out mode))
                    mode = 0;
            }
            ActivateDebug(mode);
        }

        public static void Debug(string exMsg, string exTra = null)
        {
            if (!FirstCall || DebugMode < 1 || string.IsNullOrWhiteSpace(exMsg))
                return;

            if (!FirstEntry)
            {
                FirstEntry = true;
                if (!File.Exists(FilePath))
                {
                    try
                    {
                        if (!Directory.Exists(FileLocation))
                            Directory.CreateDirectory(FileLocation);
                        File.Create(FilePath).Close();
                    }
                    catch (Exception ex)
                    {
                        if (DebugMode > 1)
                            Debug(ex);
                    }
                }
                Debug("***Logging has been started***", $"'{Environment.OSVersion}' - '{ProcName}' - '{ProcVer}' - '{FilePath}'");
            }
            if (!File.Exists(FilePath) && DebugMode < 1)
                return;

            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz");
            string exmsg = string.Empty;
            exmsg += $"Time:  {date}{Environment.NewLine}";
            exmsg += $"Msg:   {exMsg}{Environment.NewLine}";
            string extra = null;
            if (!string.IsNullOrWhiteSpace(exTra))
            {
                extra = exTra.Trim();
                extra = extra.Replace(Environment.NewLine, " - ");
                extra = $"{extra[0].ToString().ToUpper()}{extra.Substring(1)}";
            }
            if (!string.IsNullOrWhiteSpace(extra))
                exmsg += $"Trace: {extra}{Environment.NewLine}";

            string log = $"{exmsg}{Environment.NewLine}";
            if (File.Exists(FilePath))
            {
                try
                {
                    File.AppendAllText(FilePath, log);
                }
                catch (Exception ex)
                {
                    try
                    {
                        string exFileName = $"{new Random().Next(0, short.MaxValue)}-{FilePath}";
                        string exFilePath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), exFileName);
                        exmsg += $"Msg2:  {ex.Message}{Environment.NewLine}";
                        File.AppendAllText(exFilePath, exmsg);
                    }
                    catch (Exception exc)
                    {
                        if (DebugMode > 1)
                        {
                            exmsg += $"Msg3:  {exc.Message}{Environment.NewLine}";
                            MessageBox.Show(exmsg.Replace(Environment.NewLine, $"{Environment.NewLine}{Environment.NewLine}"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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
                            Console.WriteLine(new Crypt.Base64().DecodeString("ICAgX19fX19fX19fLl9fICBfX19fIF9fX19fX19fICAgICAgICBfX19fX19fX18gDQogIC8gICBfX19fXy98X198L18gICB8XF9fX19fICBcICAgX19fX1xfX19fX18gIFwNCiAgXF9fX19fICBcIHwgIHwgfCAgIHwgIF8oX18gIDwgIC8gICAgXCAgIC8gICAgLw0KICAvICAgICAgICBcfCAgfCB8ICAgfCAvICAgICAgIFx8ICAgfCAgXCAvICAgIC8gDQogL19fX19fX18gIC98X198IHxfX198L19fX19fXyAgL3xfX198ICAvL19fX18vICANCiAgICAgICAgIFwvICAgICAgICAgICAgICAgICAgXC8gICAgICBcLyAgICAgICAgIA=="));
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("           D E B U G    C O N S O L E");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        IsRunning = true;
                    }
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(new string('-', Console.BufferWidth - 1));
                    foreach (string line in exmsg.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
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

        public static void Debug(Exception ex, bool forceLogging = false)
        {
            if (DebugMode < 1)
            {
                if (!forceLogging)
                    return;
                DebugMode = 1;
            }
            string exMsg = null;
            string exTra = null;
            try
            {
                exMsg = string.Join(" - ", ex.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                exMsg = Regex.Replace(exMsg.Trim(), @"\s+", " ", RegexOptions.Singleline);
                exTra = string.Join(" - ", ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                exTra = Regex.Replace(exTra.Trim(), @"\s+", " ", RegexOptions.Singleline);
            }
            catch
            {
                exMsg = ex.Message;
                exTra = ex.StackTrace;
            }
            Debug(exMsg, exTra);
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
                foreach (string file in Directory.GetFiles(FileLocation, $"{ProcName}*.log", SearchOption.TopDirectoryOnly))
                {
                    if (FilePath == file)
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

#endregion
