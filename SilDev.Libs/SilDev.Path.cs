
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class PATH
    {
        public static bool DirOrFileExists(this string path) =>
            Directory.Exists(path) || File.Exists(path);

        public static bool DirsOrFilesExists(params string[] paths)
        {
            bool exists = false;
            foreach (string path in paths)
            {
                exists = path.DirOrFileExists();
                if (!exists)
                    break;
            }
            return exists;
        }

        public static string Combine(params string[] paths)
        {
            string path = string.Empty;
            try
            {
                if (paths.Length == 0 || paths.Count(s => string.IsNullOrWhiteSpace(s)) == paths.Length)
                    throw new ArgumentNullException();
                path = Path.Combine(paths);
                path = path.Trim().RemoveChar(Path.GetInvalidPathChars());
                if (path.StartsWith("%") && (path.Contains("%\\") || path.EndsWith("%")))
                {
                    string variable = Regex.Match(path, "%(.+?)%", RegexOptions.IgnoreCase).Groups[1].Value;
                    string value = GetEnvironmentVariableValue(variable);
                    path = path.Replace($"%{variable}%", value);
                }
                string seperator = Path.DirectorySeparatorChar.ToString();
                while (path.Contains(seperator + seperator))
                    path = path.Replace(seperator + seperator, seperator);
                if (path.EndsWith(seperator))
                    path = path.Substring(0, path.Length - seperator.Length);
                path = Path.GetFullPath(path);
            }
            catch (ArgumentNullException) { }
            catch (ArgumentException) { }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return path;
        }

        public static string GetEnvironmentVariableValue(string variable, bool lower = false)
        {
            string value = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(variable))
                    throw new ArgumentNullException();
                string varLower = variable.RemoveChar('%').ToLower();
                if (varLower == "currentdir" || varLower == "curdir")
                {
                    value = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Substring(8));
                    if (value.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        value = value.Substring(0, value.Length - 1);
                }
                else
                {
                    try
                    {
                        string match = Enum.GetNames(typeof(Environment.SpecialFolder)).First(s => s.ToLower() == varLower).ToString();
                        Environment.SpecialFolder specialFolder;
                        if (!Enum.TryParse(match, out specialFolder))
                            throw new ArgumentException();
                        value = Environment.GetFolderPath(specialFolder);
                    }
                    catch
                    {
                        value = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().First(x => x.Key.ToString().ToLower() == varLower).Value.ToString();
                    }
                }
                if (lower)
                    value = value.ToLower();
            }
            catch (ArgumentNullException) { }
            catch (ArgumentException) { }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return value;
        }

        public static string GetEnvironmentVariable(string path)
        {
            string variable = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException();
                string pathLower = path.ToLower();
                if (GetEnvironmentVariableValue("curdir").ToLower() == pathLower)
                    variable = "CurDir";
                else
                {
                    try
                    {
                        variable = Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>().First(s => Environment.GetFolderPath(s).ToLower() == pathLower).ToString();
                    }
                    catch
                    {
                        variable = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().First(x => x.Value.ToString().ToLower() == pathLower).Key.ToString();
                    }
                }
                if (!string.IsNullOrWhiteSpace(variable))
                    variable = $"%{variable}%";
            }
            catch (ArgumentNullException) { }
            catch (ArgumentException) { }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return variable;
        }

        public static string GetRandomDirName() =>
            Path.GetRandomFileName().RemoveChar('.');

        public static string GetTempDirName() =>
            GetTempFileName().RemoveChar('.');

        public static string GetTempFileName() =>
            Path.GetFileName(Path.GetTempFileName());

        public static bool FileIs64Bit(this string filePath)
        {
            ushort us = 0x0;
            try
            {
                using (FileStream fs = new FileStream(Combine(filePath), FileMode.Open, FileAccess.Read))
                {
                    BinaryReader br = new BinaryReader(fs);
                    fs.Seek(0x3c, SeekOrigin.Begin);
                    fs.Seek(br.ReadInt32(), SeekOrigin.Begin);
                    br.ReadUInt32();
                    us = br.ReadUInt16();
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return us == 0x8664 || us == 0x200;
        }
    }
}
