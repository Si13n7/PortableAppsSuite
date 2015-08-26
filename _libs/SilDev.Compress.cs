
#region SILENT DEVELOPMENTS generated code

using System.IO;
using System.IO.Compression;
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
            byte[] bytes = Encoding.UTF8.GetBytes(_str);
            using (MemoryStream msi = new MemoryStream(bytes))
            {
                using (MemoryStream mso = new MemoryStream())
                {
                    using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress))
                        msi.CopyTo(gs);
                    return mso.ToArray();
                }
            }
        }

        public static string Unzip(byte[] _bytes)
        {
            using (MemoryStream msi = new MemoryStream(_bytes))
            {
                using (MemoryStream mso = new MemoryStream())
                {
                    using (GZipStream gs = new GZipStream(msi, CompressionMode.Decompress))
                        gs.CopyTo(mso);
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
        }
    }
}

#endregion
