
#region SILENT DEVELOPMENTS generated code

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace SilDev
{
    public static class Source
    {
        private static string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        private static Dictionary<string, string> files = new Dictionary<string, string>();

        public static void AddFiles(string[] _files, string[] _hashes)
        {
            if (_files.Length == _hashes.Length && files.Count <= 0)
            {
                for (int i = 0; i < _files.Length; i++)
                {
                    string _file = _files[i];
                    string _hash = _hashes[i];
                    files.Add(_file, _hash);
                }
            }
        }

        public static void AddFiles(Dictionary<string, string> _files)
        {
            if (files.Count <= 0)
                files = _files;
        }

        public static void AddFile(KeyValuePair<string, string> _file, bool _existCheck)
        {
            bool AlreadyExists = false;
            if (_existCheck)
            {
                if (files.Count > 0)
                {
                    foreach (KeyValuePair<string, string> file in files)
                    {
                        if (file.Key == _file.Key || file.Value == file.Value)
                        {
                            AlreadyExists = true;
                            break;
                        }
                    }
                }
            }
            if (!AlreadyExists)
                files.Add(_file.Key, _file.Value);
        }

        public static void AddFile(KeyValuePair<string, string> _file)
        {
            AddFile(new KeyValuePair<string, string>(_file.Key, _file.Value), false);
        }

        public static void AddFile(string _file, string _hash, bool _existCheck)
        {
            AddFile(new KeyValuePair<string, string>(_file, _hash), _existCheck);
        }

        public static void AddFile(string _file, string _hash)
        {
            AddFile(new KeyValuePair<string, string>(_file, _hash), false);
        }

        private static bool AssembliesExist()
        {
            bool AssemblyFilesExist = true;
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (!File.Exists(GetFilePath(file.Value)) || (File.Exists(GetFilePath(file.Value)) && !Crypt.MD5.Compare(Crypt.MD5.EncryptFile(GetFilePath(file.Value)), file.Key)))
                    {
                        AssemblyFilesExist = false;
                        break;
                    }
                }
                if (!AssemblyFilesExist)
                    foreach (KeyValuePair<string, string> file in files)
                        if (File.Exists(GetFilePath(file.Value)))
                            File.Delete(GetFilePath(file.Value));
            }
            return AssemblyFilesExist;
        }

        public static string GetPath()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public static string GetFilePath(string _file)
        {
            return Path.Combine(GetPath(), _file);
        }

        public static void LoadAssemblies(byte[] _sources)
        {
            string _path = GetFilePath("source.bytes");
            try
            {
                if (!AssembliesExist())
                {
                    Resource.ExtractConvert(_sources, _path);
                    using (ZipArchive zip = ZipFile.OpenRead(_path))
                    {
                        zip.ExtractToDirectory(Path.GetDirectoryName(_path));
                        zip.Dispose();
                    }
                    if (File.Exists(_path))
                        File.Delete(_path);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                if (File.Exists(_path))
                    File.Delete(_path);
            }
        }

        public static void IncludeAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var filePath = string.Format("{0}\\{1}.dll", GetPath(), new AssemblyName(e.Name).Name);
                return Assembly.LoadFrom(filePath);
            };
        }

        public static void ClearSources()
        {
            Run.App(@"%WinDir%\System32", "cmd.exe", string.Format("/C PING 127.0.0.1 -n 2 & RMDIR /S /Q \"{0}\"", path), Run.WindowStyle.Hidden);
        }
    }
}

#endregion
