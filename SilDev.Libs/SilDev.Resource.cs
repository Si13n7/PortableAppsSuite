
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
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
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

        [Flags]
        public enum SystemIconKey : uint
        {
            ASTERISK = 76,
            CAM = 41,
            CD = 56,
            CD_R = 56,
            CD_ROM = 57,
            CD_RW = 58,
            CHIP = 29,
            CMD = 256,
            COMPUTER = 104,
            DEFRAG = 106,
            DESKTOP = 105,
            DIRECTORY = 3,
            DISC_DRIVE = 25,
            DVD = 51,
            DVD_R = 33,
            DVD_RAM = 34,
            DVD_ROM = 35,
            DVD_RW = 36,
            DVD_DRIVE = 32,
            DynamicLinkLibrary = 62,
            ERROR = 93,
            EXE = 11,
            EXXPLORER = 203,
            FAVORITE = 204,
            FLOPPY_DRIVE = 23,
            FOLDER = 3,
            GAMES = 10,
            HARD_DRIVE = 30,
            HELP = 94,
            HELP_SHIELD = 99,
            NETWORK = 170,
            ONE_DRIVE = 220,
            PIN = 228,
            PRINTER = 46,
            QUESTION = 94,
            RECYCLE_BIN_EMPTY = 50,
            RECYCLE_BIN_FULL = 49,
            RUN = 95,
            SCREENSAVER = 96,
            SEARCH = 13,
            SECURITY = 54,
            SHARING = 83,
            SHORTCUT = 154,
            STOP = 207,
            SYSTEM_CONTROL = 22,
            SYSTEM_DRIVE = 31,
            TASK_MANAGER = 144,
            UNDO = 249,
            UNPIN = 227,
            USER = 208,
            UAC = 73,
            WARNING = 79
        }

        public static Icon SystemIcon(SystemIconKey key, bool large = false)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imageres.dll");
                Icon ico = IconFromFile(path, (int)key);
                return ico;
            }
            catch
            {
                return null;
            }
        }

        public static Image SystemIconAsImage(SystemIconKey key, bool large = false)
        {
            try
            {
                Icon ico = SystemIcon(key, large);
                Image img = new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
                return img;
            }
            catch
            {
                return null;
            }
        }

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
