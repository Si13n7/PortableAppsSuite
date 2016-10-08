
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

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
        public static void CopyTo(this Stream source, Stream destination)
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

        public static byte[] TextToZip(this string text)
        {
            try
            {
                byte[] ba = Encoding.UTF8.GetBytes(text);
                using (MemoryStream mso = new MemoryStream())
                {
                    MemoryStream msi = new MemoryStream(ba);
                    using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress))
                        msi.CopyTo(gs);
                    ba = mso.ToArray();
                }
                return ba;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static string TextFromZip(this byte[] bytes)
        {
            try
            {
                string s;
                using (MemoryStream mso = new MemoryStream())
                {
                    MemoryStream msi = new MemoryStream(bytes);
                    using (GZipStream gs = new GZipStream(msi, CompressionMode.Decompress))
                        gs.CopyTo(mso);
                    s = Encoding.UTF8.GetString(mso.ToArray());
                }
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static bool Unzip(string srcPath, string destPath, bool deleteSource = true)
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

            public struct CompressTemplates
            {
                public const string Default = "-t7z -mx -mmt -ms";
                public const string Ultra = "-t7z -mx -m0=lzma -md=128m -mfb=256 -ms";
            }

            public static int Zip(string srcDirOrFile, string destFile, string args = null, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden) =>
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"a {args ?? CompressTemplates.Default} \"\"\"{destFile}\"\"\" \"\"\"{srcDirOrFile}{(DATA.IsDir(srcDirOrFile) ? "\\*" : string.Empty)}\"\"\"",
                    FileName = ExePath,
                    WindowStyle = windowStyle
                }, 0);

            public static int Zip(string srcDirOrFile, string destFile, ProcessWindowStyle windowStyle) =>
                Zip(srcDirOrFile, destFile, null, windowStyle);

            public static int Zip(string srcDirOrFile, string destFile, string args, bool hidden) =>
                Zip(srcDirOrFile, destFile, args, (ProcessWindowStyle)Convert.ToInt32(hidden));

            public static int Zip(string srcDirOrFile, string destFile, bool hidden) =>
                Zip(srcDirOrFile, destFile, null, (ProcessWindowStyle)Convert.ToInt32(hidden));

            public static int Unzip(string srcFile, string destDir, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden) =>
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"x \"\"\"{srcFile}\"\"\" -o\"\"\"{destDir}\"\"\" -y",
                    FileName = ExePath,
                    WindowStyle = windowStyle
                }, 0);

            public static int Unzip(string srcFile, string destDir, bool hidden) =>
                Unzip(srcFile, destDir, (ProcessWindowStyle)Convert.ToInt32(hidden));

        }

        #endregion
    }
}
