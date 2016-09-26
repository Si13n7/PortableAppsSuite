
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
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.CRYPT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <para><see cref="SilDev.RESOURCE"/>.cs</para>
    /// <para><see cref="SilDev.RUN"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class SOURCE
    {
        private static Dictionary<string, string> fileDict = new Dictionary<string, string>();
        private static bool initialized = false;

        private readonly static string tempAssembliesDir = Path.Combine(Path.GetTempPath(), PATH.GetTempDirName());
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

        public static void AddTempAssemblyFiles(string[] files, string[] hashes)
        {
            if (files.Length == hashes.Length && fileDict.Count <= 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string _file = files[i];
                    string _hash = hashes[i];
                    fileDict.Add(_file, _hash);
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
                    if (fileDict.ContainsKey(d.Key))
                    {
                        if (fileDict[d.Key] != d.Value)
                            fileDict[d.Key] = d.Value;
                        continue;
                    }
                    fileDict.Add(d.Key, d.Value);
                }
            }
        }

        public static void AddTempAssembly(KeyValuePair<string, string> hashAsKeyFileNameAsValue)
        {
            if (fileDict.ContainsKey(hashAsKeyFileNameAsValue.Key))
            {
                if (fileDict[hashAsKeyFileNameAsValue.Key] != hashAsKeyFileNameAsValue.Value)
                    fileDict[hashAsKeyFileNameAsValue.Key] = hashAsKeyFileNameAsValue.Value;
                return;
            }
            fileDict.Add(hashAsKeyFileNameAsValue.Key, hashAsKeyFileNameAsValue.Value);
        }

        public static void AddTempAssembly(string hash, string fileName) =>
            AddTempAssembly(new KeyValuePair<string, string>(hash, fileName));

        private static bool TempAssembliesExists()
        {
            bool exists = true;
            if (fileDict.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in fileDict)
                {
                    if (CRYPT.MD5.EncryptFile(TempAssembliesFilePath(entry.Value)) != entry.Key)
                    {
                        exists = false;
                        break;
                    }
                }
                if (!exists)
                {
                    foreach (KeyValuePair<string, string> file in fileDict)
                    {
                        try
                        {
                            string filePath = TempAssembliesFilePath(file.Value);
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
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
                        RESOURCE.ExtractConvert(resData, path);
                        using (ZipArchive zip = ZipFile.OpenRead(path))
                            zip.ExtractToDirectory(Path.GetDirectoryName(path));
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    try
                    {
                        if (File.Exists(tempAssembliesDir))
                            File.Delete(tempAssembliesDir);
                    }
                    catch (Exception exc)
                    {
                        LOG.Debug(exc);
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
                LOG.Debug(ex);
            }
        }

        public static void ClearSources() =>
            RUN.Cmd($"PING 127.0.0.1 -n 2 & RMDIR /S /Q \"{tempAssembliesDir}\"");
    }
}

#endregion
