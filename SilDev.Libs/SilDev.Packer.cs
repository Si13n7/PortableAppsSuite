
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <para><see cref="SilDev.RUN"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class PACKER
    {
        public static void CopyTo(Stream source, Stream destination)
        {
            try
            {
                byte[] ba = new byte[4096];
                int i;
                while ((i = source.Read(ba, 0, ba.Length)) > 0)
                    destination.Write(ba, 0, i);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202")]
        public static byte[] ZipString(string text)
        {
            try
            {
                byte[] ba = Encoding.UTF8.GetBytes(text);
                using (MemoryStream msi = new MemoryStream(ba))
                {
                    using (MemoryStream mso = new MemoryStream())
                    {
                        using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress))
                            msi.CopyTo(gs);
                        ba = mso.ToArray();
                    }
                }
                return ba;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static bool UnzipFile(string srcPath, string destPath, bool deleteSource = true)
        {
            try
            {
                using (ZipArchive zip = ZipFile.OpenRead(srcPath))
                    zip.ExtractToDirectory(destPath);
                if (deleteSource)
                    File.Delete(srcPath);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202")]
        public static string UnzipString(byte[] bytes)
        {
            try
            {
                string s;
                using (MemoryStream msi = new MemoryStream(bytes))
                {
                    using (MemoryStream mso = new MemoryStream())
                    {
                        using (GZipStream gs = new GZipStream(msi, CompressionMode.Decompress))
                            gs.CopyTo(mso);
                        s = Encoding.UTF8.GetString(mso.ToArray());
                    }
                }
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        #region 7-Zip Helper

        /// <summary>Helper for 7-Zip which requires the binaries!</summary>
        public static class Zip7Helper
        {
            public static string ExePath { get; set; } =
#if x86
            PATH.Combine("%CurDir%\\Helper\\7z\\7zG.exe");
#else
            PATH.Combine("%CurDir%\\Helper\\7z\\x64\\7zG.exe");
#endif

            public static int Zip(string srcDirOrFile, string destFile, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden) =>
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"a -t7z \"\"\"{destFile}\"\"\" \"\"\"{srcDirOrFile}{(DATA.IsDir(srcDirOrFile) ? "\\*" : string.Empty)}\"\"\" -ms -mmt -mx=9",
                    FileName = ExePath,
                    WindowStyle = windowStyle
                }, 0);

            public static int Zip(string source, string destination, bool hidden) =>
                Zip(source, destination, (ProcessWindowStyle)Convert.ToInt32(hidden));

            public static int Unzip(string srcFile, string destDir, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden) =>
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"x \"\"\"{srcFile}\"\"\" -o\"\"\"{destDir}\"\"\" -y",
                    FileName = ExePath,
                    WindowStyle = windowStyle
                }, 0);

            public static int Unzip(string srcFile, string destDir, bool hideWindow) =>
                Unzip(srcFile, destDir, (ProcessWindowStyle)Convert.ToInt32(hideWindow));

        }

        #endregion
    }
}

#endregion
