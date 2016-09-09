
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Resource
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("shell32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal extern static int ExtractIconEx([MarshalAs(UnmanagedType.LPStr)]string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool DestroyIcon(IntPtr hIcon);
        }

        public static Icon IconFromFile(string path, int index = 0, bool large = false)
        {
            try
            {
                IntPtr[] ptrs = new IntPtr[1];
                SafeNativeMethods.ExtractIconEx(path, index, large ? ptrs : new IntPtr[1], !large ? ptrs : new IntPtr[1], 1);
                IntPtr ptr = ptrs[0];
                if (ptr == IntPtr.Zero)
                    throw new ArgumentNullException();
                return Icon.FromHandle(ptr);
            }
            catch
            {
                return null;
            }
        }

        public static Image IconFromFileAsImage(string path, int index = 0, bool large = false)
        {
            try
            {
                Icon ico = IconFromFile(path, index, large);
                return new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
            }
            catch
            {
                return null;
            }
        }

        public enum SystemIconKey : uint
        {
            ASTERISK = 76,
            BARRIER = 81,
            BMP = 66,
            CAM = 41,
            CD = 56,
            CD_R = 57,
            CD_ROM = 58,
            CD_RW = 59,
            CHIP = 29,
            CLIPBOARD = 241,
            CLOSE = 235,
            CMD = 262,
            COMPUTER = 104,
            DEFRAG = 106,
            DESKTOP = 105,
            DIRECTORY = 3,
            DIRECTORY_SEARCH = 13,
            DISC_DRIVE = 25,
            DLL = 62,
            DVD = 51,
            DVD_DRIVE = 32,
            DVD_R = 33,
            DVD_RAM = 34,
            DVD_ROM = 35,
            DVD_RW = 36,
            EJECT = 167,
            ERROR = 93,
            EXE = 11,
            EXPLORER = 203,
            FAVORITE = 204,
            FLOPPY_DRIVE = 23,
            GAMES = 10,
            HARD_DRIVE = 30,
            HELP = 94,
            HELP_SHIELD = 99,
            INF = 64,
            INSTALL = 82,
            JPG = 67,
            KEY = 77,
            NETWORK = 170,
            ONE_DRIVE = 220,
            PLAY = 280,
            PIN = 234,
            PNG = 78,
            PRINTER = 46,
            QUESTION = 94,
            RECYCLE_BIN_EMPTY = 50,
            RECYCLE_BIN_FULL = 49,
            RETRY = 251,
            RUN = 95,
            SCREENSAVER = 96,
            SEARCH = 168,
            SECURITY = 54,
            SHARED_MARKER = 155,
            SHARING = 83,
            SHORTCUT_MARKER = 154,
            STOP = 207,
            SYSTEM_CONTROL = 22,
            SYSTEM_DRIVE = 31,
            TASK_MANAGER = 144,
            UNDO = 255,
            UNKNOWN_DRIVE = 70,
            UNPIN = 233,
            USER = 208,
            USER_DIR = 117,
            UAC = 73,
            WARNING = 79,
            ZIP = 165
        }

        public static Icon SystemIcon(SystemIconKey key, bool large = false, string path = "%system%\\imageres.dll")
        {
            try
            {
                path = Run.EnvVarFilter(path);
                if (!File.Exists(path))
                    path = Run.EnvVarFilter("%system%\\imageres.dll");
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                Icon ico = IconFromFile(path, (int)key, large);
                return ico;
            }
            catch (FileNotFoundException ex)
            {
                Log.Debug(ex);
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Icon SystemIcon(SystemIconKey key, string path) =>
            SystemIcon(key, false, path);

        public static Image SystemIconAsImage(SystemIconKey key, bool large = false, string path = "%system%\\imageres.dll")
        {
            try
            {
                Icon ico = SystemIcon(key, large, path);
                Image img = new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static Image SystemIconAsImage(SystemIconKey key, string path) =>
            SystemIconAsImage(key, false, path);

        public static void ExtractConvert(byte[] resData, string destPath, bool reverseBytes = true)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(resData))
                {
                    byte[] data = ms.ToArray();
                    if (reverseBytes)
                        data = data.Reverse().ToArray();
                    using (FileStream fs = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Extract(byte[] resData, string destPath) =>
            ExtractConvert(resData, destPath, false);

        public static void PlayWave(Stream resData)
        {
            try
            {
                using (Stream audio = resData)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(audio);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void PlayWaveAsync(Stream resData) =>
            new Thread(() => PlayWave(resData)).Start();

    }
}

#endregion
