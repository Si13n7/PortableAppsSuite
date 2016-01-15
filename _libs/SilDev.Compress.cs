
#region SILENT DEVELOPMENTS generated code

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
        static string SevenZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "Helper\\7z\\7zG.exe");
#else
        static string SevenZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "Helper\\7z\\x64\\7zG.exe");
#endif

        public static int Zip7(string _src, string _dest, bool _hidden)
        {
            object output = Run.App(new ProcessStartInfo() { Arguments = string.Format("a -t7z \"\"\"{0}\"\"\" \"\"\"{1}{2}\"\"\" -ms -mmt -mx=9", _dest, _src, Data.IsDir(_src) ? "\\*" : string.Empty), FileName = SevenZipPath, WindowStyle = _hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal }, 0);
            return output is int ? (int)output : -1;
        }

        public static int Zip7(string _src, string _dest)
        {
            return Zip7(_src, _dest, true);
        }

        public static int Unzip7(string _src, string _dest, bool _hidden)
        {
            object output = Run.App(new ProcessStartInfo() { Arguments = string.Format("x \"\"\"{0}\"\"\" -o\"\"\"{1}\"\"\" -y", _src, _dest), FileName = SevenZipPath, WindowStyle = _hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal }, 0);
            return output is int ? (int)output : -1;
        }

        public static int Unzip7(string _src, string _dest)
        {
            return Unzip7(_src, _dest, true);
        }

        #endregion

    }
}

#endregion
