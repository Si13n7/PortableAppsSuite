
#region SILENT DEVELOPMENTS generated code

using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace SilDev
{
    public static class Log
    {
        public readonly static string ConsoleTitle = string.Format("Debug Console ('{0}')", Path.GetFileName(Application.ExecutablePath));
        public readonly static string DebugFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), string.Format("debug-{0}-{1}.log", Path.GetFileNameWithoutExtension(Application.ExecutablePath), Crypt.MD5.Encrypt(Application.ExecutablePath).Substring(24)));
        public static int DebugMode { get; private set; }
        private static bool IsRunning = false;
        private static IntPtr stdHandle = IntPtr.Zero;
        private static SafeFileHandle sfh = null;
        private static FileStream fs = null;
        private static StreamWriter sw = null;

        public static void ActivateDebug(int _option)
        {
            if (File.Exists(DebugFile))
                File.Delete(DebugFile);
            DebugMode = _option;
        }

        public static void ActivateDebug()
        {
            ActivateDebug(2);
        }

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
            string logo = @"   _________.__  ____ ________        _________ {0}" +
                          @"  /   _____/|__|/_   |\_____  \   ____\______  \{0}" +
                          @"  \_____  \ |  | |   |  _(__  <  /    \   /    /{0}" +
                          @"  /        \|  | |   | /       \|   |  \ /    / {0}" +
                          @" /_______  /|__| |___|/______  /|___|  //____/  {0}" +
                          @"         \/                  \/      \/         {0}";
            logo = string.Format(logo, Environment.NewLine);
            string date = DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("en-US"));
            string trace = null;
            if (!string.IsNullOrWhiteSpace(_trace))
            {
                trace = _trace.TrimStart().TrimEnd();
                trace = trace.Replace(Environment.NewLine, " - ");
                trace = string.Format("{0}{1}", trace[0].ToString().ToUpper(), trace.Substring(1));
            }
            if (!File.Exists(DebugFile))
                File.WriteAllText(DebugFile, string.Format("{0}{3}[Created '{1}' at {2}]{3}{3}", logo, Path.GetFileName(DebugFile), date, Environment.NewLine));

            string msg = string.Empty;
            msg += string.Format("Time:  {0}{1}", date, Environment.NewLine);
            msg += string.Format("Msg:   {0}{1}", _msg, Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(trace))
                msg += string.Format("Trace: {0}{1}", trace, Environment.NewLine);

            string log = string.Format("{0}{1}", msg, Environment.NewLine);
            try
            {
                if (!File.Exists(DebugFile))
                    File.WriteAllText(DebugFile, log);
                else
                    File.AppendAllText(DebugFile, log);
            }
            catch (Exception ex)
            {
                string exFile = string.Format("{0}-{1}", new Random().Next(0, short.MaxValue), DebugFile);
                string exMsg = string.Format("{0}Msg:  {1}{2}", msg, ex.Message, Environment.NewLine);
                if (!File.Exists(DebugFile))
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
                            Console.BufferHeight = 8000;
                            Console.BufferWidth = Console.WindowWidth;
                            Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(logo);
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("               DEBUG CONSOLE v1.5");
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.ResetColor();
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
                        Console.WriteLine(string.Format(" {0}", string.Join(" ", words.Skip(1).ToArray())));
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
            string msg = _ex.Message;
            string trace = _ex.StackTrace.TrimStart();
            Debug(msg, trace);
        }
    }
}

#endregion
