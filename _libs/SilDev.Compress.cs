
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace SilDev
{
    public static class Compress
    {
        public static void CopyTo(Stream _src, Stream _dest)
        {
            byte[] bytes = new byte[4096];
            int cnt;
            while ((cnt = _src.Read(bytes, 0, bytes.Length)) != 0)
                _dest.Write(bytes, 0, cnt);
        }

        public static byte[] Zip(string _str)
        {
            byte[] output = null;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(_str);
                using (MemoryStream msi = new MemoryStream(bytes))
                {
                    MemoryStream mso = new MemoryStream();
                    GZipStream gs = new GZipStream(mso, CompressionMode.Compress);
                    msi.CopyTo(gs);
                    output = mso.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static string Unzip(byte[] _bytes)
        {
            string output = null;
            try
            {
                using (MemoryStream msi = new MemoryStream(_bytes))
                {
                    MemoryStream mso = new MemoryStream();
                    GZipStream gs = new GZipStream(msi, CompressionMode.Decompress);
                    gs.CopyTo(mso);
                    output = Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        #region 7-Zip Helper

#if x86
        public static string SevenZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "Helper\\7z\\7zG.exe");
#else
        public static string SevenZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "Helper\\7z\\x64\\7zG.exe");
#endif

        public static int Zip7(string _src, string _dest, ProcessWindowStyle _windowStyle)
        {
            object output = Run.App(new ProcessStartInfo()
            {
                Arguments = $"a -t7z \"\"\"{_dest}\"\"\" \"\"\"{_src}{(Data.IsDir(_src) ? "\\*" : string.Empty)}\"\"\" -ms -mmt -mx=9",
                FileName = SevenZipPath,
                WindowStyle = _windowStyle
            }, 0);
            return output is int ? (int)output : -1;
        }

        public static int Zip7(string _src, string _dest, bool _hidden) =>
            Zip7(_src, _dest, _hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal);

        public static int Zip7(string _src, string _dest) =>
            Zip7(_src, _dest, ProcessWindowStyle.Hidden);

        public static int Unzip7(string _src, string _dest, ProcessWindowStyle _windowStyle)
        {
            object output = Run.App(new ProcessStartInfo()
            {
                Arguments = $"x \"\"\"{_src}\"\"\" -o\"\"\"{_dest}\"\"\" -y",
                FileName = SevenZipPath,
                WindowStyle = _windowStyle
            }, 0);
            return output is int ? (int)output : -1;
        }

        public static int Unzip7(string _src, string _dest, bool _hidden) =>
            Unzip7(_src, _dest, _hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal);

        public static int Unzip7(string _src, string _dest) =>
            Unzip7(_src, _dest, ProcessWindowStyle.Hidden);

        #endregion

    }
}

#endregion
