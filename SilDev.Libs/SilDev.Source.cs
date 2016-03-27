
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Resource"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
    /// <para><see cref="SilDev.WinAPI"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Source
    {
        private static Dictionary<string, string> files = new Dictionary<string, string>();
        private static bool initialized = false;

        private readonly static string tempAssembliesDir = Run.EnvironmentVariableFilter("%TEMP%", Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        public static string TempAssembliesDir
        {
            get
            {
                if (!Directory.Exists(tempAssembliesDir))
                    Directory.CreateDirectory(tempAssembliesDir);
                return tempAssembliesDir;
            }
        }

        public static string TempAssembliesFilePath(string fileName) =>
            Path.Combine(TempAssembliesDir, fileName);

        public static void AddTempAssemblyFiles(string[] _files, string[] _hashes)
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

        public static void AddTempAssemblies(Dictionary<string, string> hashAsKeyFileNameAsValue)
        {
            if (hashAsKeyFileNameAsValue.Count > 0)
            {
                foreach (KeyValuePair<string, string> d in hashAsKeyFileNameAsValue)
                {
                    if (!string.IsNullOrWhiteSpace(d.Key) || d.Value.Length != 32)
                        continue;
                    if (files.ContainsKey(d.Key))
                    {
                        if (files[d.Key] != d.Value)
                            files[d.Key] = d.Value;
                        continue;
                    }
                    files.Add(d.Key, d.Value);
                }
            }
        }

        public static void AddTempAssembly(KeyValuePair<string, string> hashAsKeyFileNameAsValue)
        {
            if (files.ContainsKey(hashAsKeyFileNameAsValue.Key))
            {
                if (files[hashAsKeyFileNameAsValue.Key] != hashAsKeyFileNameAsValue.Value)
                    files[hashAsKeyFileNameAsValue.Key] = hashAsKeyFileNameAsValue.Value;
                return;
            }
            files.Add(hashAsKeyFileNameAsValue.Key, hashAsKeyFileNameAsValue.Value);
        }

        public static void AddTempAssembly(string hash, string fileName) =>
            AddTempAssembly(new KeyValuePair<string, string>(hash, fileName));

        private static bool TempAssembliesExists()
        {
            bool exists = true;
            if (files.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in files)
                {
                    if (Crypt.MD5.EncryptFile(TempAssembliesFilePath(entry.Value)) != entry.Key)
                    {
                        exists = false;
                        break;
                    }
                }
                if (!exists)
                {
                    foreach (KeyValuePair<string, string> file in files)
                    {
                        try
                        {
                            string filePath = TempAssembliesFilePath(file.Value);
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex);
                        }
                    }
                }
            }
            return exists;
        }

        public static void LoadTempAssemblies(byte[] resData)
        {
            if (!initialized)
            {
                initialized = true;
                AppDomain.CurrentDomain.ProcessExit += (s, e) => ClearSources();
                try
                {
                    string path = TempAssembliesFilePath(Path.GetRandomFileName());
                    if (!TempAssembliesExists())
                    {
                        Resource.ExtractConvert(resData, path);
                        using (ZipArchive zip = ZipFile.OpenRead(path))
                            zip.ExtractToDirectory(Path.GetDirectoryName(path));
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    try
                    {
                        if (File.Exists(tempAssembliesDir))
                            File.Delete(tempAssembliesDir);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug(exc);
                    }
                }
            }
        }

        public static void LoadAssembliesAsync(byte[] resData) =>
            new Thread(() => LoadTempAssemblies(resData)).Start();

        public static void IncludeTempAssemblies()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
                {
                    string filePath = TempAssembliesFilePath($"{new AssemblyName(e.Name).Name}.dll");
                    return Assembly.LoadFrom(filePath);
                };
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void ClearSources() =>
            Run.Cmd($"PING 127.0.0.1 -n 2 & RMDIR /S /Q \"{tempAssembliesDir}\"");
    }
}

#endregion
