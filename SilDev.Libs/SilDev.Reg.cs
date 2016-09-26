
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.CRYPT"/>.cs</para>
    /// <para><see cref="SilDev.INI"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <para><see cref="SilDev.RUN"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class REG
    {
        #region KEY

        public enum RegKey : int
        {
            Default = 0,
            ClassesRoot = 10,
            CurrentConfig = 20,
            CurrentUser = 30,
            LocalMachine = 40,
            PerformanceData = 50,
            Users = 60
        }

        private static RegistryKey AsRegistryKey(this object key)
        {
            try
            {
                if (key is RegistryKey)
                    return (RegistryKey)key;
                if (key is RegKey)
                {
                    switch ((RegKey)key)
                    {
                        case RegKey.ClassesRoot:
                            return Registry.ClassesRoot;
                        case RegKey.CurrentConfig:
                            return Registry.CurrentConfig;
                        case RegKey.LocalMachine:
                            return Registry.LocalMachine;
                        case RegKey.PerformanceData:
                            return Registry.PerformanceData;
                        case RegKey.Users:
                            return Registry.Users;
                        default:
                            return Registry.CurrentUser;
                    }
                }
                if (key is string)
                {
                    switch (((string)key).ToUpper())
                    {
                        case "HKEY_CLASSES_ROOT":
                        case "HKCR":
                            return Registry.ClassesRoot;
                        case "HKEY_CURRENT_CONFIG":
                        case "HKCC":
                            return Registry.CurrentConfig;
                        case "HKEY_LOCAL_MACHINE":
                        case "HKLM":
                            return Registry.LocalMachine;
                        case "HKEY_PERFORMANCE_DATA":
                        case "HKPD":
                            return Registry.PerformanceData;
                        case "HKEY_USERS":
                        case "HKU":
                            return Registry.Users;
                        default:
                            return Registry.CurrentUser;
                    }
                }
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return Registry.CurrentUser;
            }
        }

        private static RegistryKey GetKey(this string key)
        {
            if (key.Contains(Path.DirectorySeparatorChar))
                return key.Split(Path.DirectorySeparatorChar)[0].AsRegistryKey();
            else
                return key.AsRegistryKey();
        }

        private static string GetSubKey(this string key)
        {
            string[] keys = new string[2];
            if (key.Contains(Path.DirectorySeparatorChar))
                return key.Split(Path.DirectorySeparatorChar).Skip(1).Join(Path.DirectorySeparatorChar);
            else
                return string.Empty;
        }

        #endregion

        #region VALUE KIND

        public enum RegValueKind
        {
            None = RegistryValueKind.None,
            String = RegistryValueKind.String,
            Binary = RegistryValueKind.Binary,
            DWord = RegistryValueKind.DWord,
            QWord = RegistryValueKind.QWord,
            ExpandString = RegistryValueKind.ExpandString,
            MultiString = RegistryValueKind.MultiString,
        }

        private static RegistryValueKind AsRegistryValueKind(this object type)
        {
            try
            {
                if (type == null)
                    throw new ArgumentNullException();
                switch ((type is string ? (string)type : type.ToString()).ToUpper())
                {
                    case "STRING":
                    case "STR":
                        return RegistryValueKind.String;
                    case "BINARY":
                    case "BIN":
                        return RegistryValueKind.Binary;
                    case "DWORD":
                    case "DW":
                        return RegistryValueKind.DWord;
                    case "QWORD":
                    case "QW":
                        return RegistryValueKind.QWord;
                    case "EXPANDSTRING":
                    case "ESTR":
                        return RegistryValueKind.ExpandString;
                    case "MULTISTRING":
                    case "MSTR":
                        return RegistryValueKind.MultiString;
                    default:
                        return RegistryValueKind.None;
                }
            }
            catch (ArgumentNullException)
            {
                return RegistryValueKind.None;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return RegistryValueKind.None;
            }
        }

        #endregion

        #region KEY ORDER

        public static bool SubKeyExist(object key, string subKey)
        {
            try
            {
                RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey);
                return rKey != null;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool SubKeyExist(string keyPath) =>
            SubKeyExist(keyPath.GetKey(), keyPath.GetSubKey());

        public static bool CreateNewSubKey(object key, string subKey)
        {
            try
            {
                if (!SubKeyExist(key, subKey))
                    key.AsRegistryKey().CreateSubKey(subKey);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool CreateNewSubKey(string keyPath) =>
            CreateNewSubKey(keyPath.GetKey(), keyPath.GetSubKey());

        public static bool RemoveExistSubKey(object key, string subKey)
        {
            try
            {
                if (SubKeyExist(key, subKey))
                    key.AsRegistryKey().DeleteSubKeyTree(subKey);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool RemoveExistSubKey(string keyPath) =>
            RemoveExistSubKey(keyPath.GetKey(), keyPath.GetSubKey());

        public static List<string> GetSubKeyTree(object key, string subKey)
        {
            try
            {
                List<string> subKeys = GetSubKeys(key, subKey);
                if (subKeys.Count > 0)
                {
                    int count = subKeys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        List<string> subs = GetSubKeys(key, subKeys[i]);
                        if (subs.Count > 0)
                        {
                            subKeys.AddRange(subs);
                            count = subKeys.Count;
                        }
                    }
                }
                return subKeys.OrderBy(x => x).ToList();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static List<string> GetSubKeyTree(string keyPath) =>
            GetSubKeyTree(keyPath.GetKey(), keyPath.GetSubKey());

        public static List<string> GetSubKeys(object key, string subKey)
        {
            try
            {
                List<string> keys = new List<string>();
                if (SubKeyExist(key, subKey))
                {
                    using (RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey))
                        foreach (string e in rKey.GetSubKeyNames())
                            keys.Add($"{subKey}\\{e}");
                }
                return keys;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static List<string> GetSubKeys(string keyPath) =>
            GetSubKeys(keyPath.GetKey(), keyPath.GetSubKey());

        public static bool RenameSubKey(object key, string subKey, string newSubKeyName)
        {
            if (SubKeyExist(key, subKey) && !SubKeyExist(key, newSubKeyName))
            {
                if (CopyKey(key, subKey, newSubKeyName))
                {
                    try
                    {
                        key.AsRegistryKey().DeleteSubKeyTree(subKey);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }
            return false;
        }

        public static bool RenameSubKey(string keyPath, string newSubKeyName) =>
            RenameSubKey(keyPath.GetKey(), keyPath.GetSubKey(), newSubKeyName);

        public static bool CopyKey(object key, string subKey, string newSubKeyName)
        {
            if (SubKeyExist(key, subKey) && !SubKeyExist(key, newSubKeyName))
            {
                try
                {
                    RegistryKey destKey = key.AsRegistryKey().CreateSubKey(newSubKeyName);
                    RegistryKey srcKey = key.AsRegistryKey().OpenSubKey(subKey);
                    RecurseCopyKey(srcKey, destKey);
                    return true;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return false;
        }

        public static bool CopyKey(string keyPath, string newSubKeyName) =>
            CopyKey(keyPath.GetKey(), keyPath.GetSubKey(), newSubKeyName);

        private static void RecurseCopyKey(RegistryKey srcKey, RegistryKey destKey)
        {
            try
            {
                foreach (string valueName in srcKey.GetValueNames())
                {
                    object obj = srcKey.GetValue(valueName);
                    RegistryValueKind valKind = srcKey.GetValueKind(valueName);
                    destKey.SetValue(valueName, obj, valKind);
                }
                foreach (string sourceSubKeyName in srcKey.GetSubKeyNames())
                {
                    RegistryKey srcSubKey = srcKey.OpenSubKey(sourceSubKeyName);
                    RegistryKey destSubKey = destKey.CreateSubKey(sourceSubKeyName);
                    RecurseCopyKey(srcSubKey, destSubKey);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        #endregion

        #region READ VALUE

        public static bool ValueExist(object key, string subKey, string entry, object type = null)
        {
            try
            {
                string value = ReadValue(key, subKey, entry, type).ToLower();
                return !string.IsNullOrWhiteSpace(value) && value != "none";
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool ValueExist(string keyPath, string entry, object type = null) =>
            ValueExist(keyPath.GetKey(), keyPath.GetSubKey(), entry, type);

        public static Dictionary<string, string> GetAllTreeValues(object key, string subKey)
        {
            Dictionary<string, string> tree = new Dictionary<string, string>();
            foreach (string sKey in GetSubKeyTree(key, subKey))
            {
                if (string.IsNullOrEmpty(sKey))
                    continue;
                Dictionary<string, string> entries = GetValues(key, sKey);
                if (entries.Keys.Count > 0)
                {
                    if (!tree.ContainsKey(sKey))
                        tree.Add(sKey, sKey.EncryptToMD5());
                    foreach (KeyValuePair<string, string> entry in entries)
                    {
                        try
                        {
                            tree.Add(entry.Key, entry.Value);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                }
            }
            return tree;
        }

        public static Dictionary<string, string> GetAllTreeValues(string keyPath) =>
            GetAllTreeValues(keyPath.GetKey(), keyPath.GetSubKey());

        public static Dictionary<string, string> GetValues(object key, string subKey)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            try
            {
                if (!SubKeyExist(key, subKey))
                    throw new KeyNotFoundException();
                using (RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey))
                {
                    foreach (string ent in rKey.GetValueNames())
                    {
                        try
                        {
                            string value = ReadValue(key, subKey, ent);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                RegistryValueKind type = rKey.GetValueKind(ent);
                                values.Add(ent, $"ValueKind_{type}::{(type == RegistryValueKind.MultiString ? value.ToHexString() : value)}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return values;
        }

        public static Dictionary<string, string> GetValues(string keyPath) =>
            GetValues(keyPath.GetKey(), keyPath.GetSubKey());

        public static object ReadObjValue(object key, string subKey, string entry, object type = null)
        {
            object ent = null;
            if (SubKeyExist(key, subKey))
            {
                try
                {
                    using (RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey))
                        ent = rKey.GetValue(entry, type.AsRegistryValueKind());
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return ent;
        }

        public static object ReadObjValue(string keyPath, string entry, object type = null) =>
            ReadObjValue(keyPath.GetKey(), keyPath.GetSubKey(), entry, type);

        public static string ReadValue(object key, string subKey, string entry, object type = null)
        {
            string value = string.Empty;
            if (SubKeyExist(key, subKey))
            {
                try
                {
                    using (RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey))
                    {
                        object objValue = rKey.GetValue(entry, type.AsRegistryValueKind());
                        if (objValue != null)
                        {
                            if (objValue is string[])
                                value = (objValue as string[]).Join(Environment.NewLine);
                            else if (objValue is byte[])
                                value = BitConverter.ToString(objValue as byte[]).RemoveChar('-');
                            else
                                value = objValue.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return value;
        }

        public static string ReadValue(string keyPath, string entry, object type = null) =>
            ReadValue(keyPath.GetKey(), keyPath.GetSubKey(), entry, type);

        #endregion

        #region WRITE VALUE

        public static void WriteValue<T>(object key, string subKey, string entry, T value, object type = null)
        {
            if (!SubKeyExist(key, subKey))
                CreateNewSubKey(key, subKey);
            if (SubKeyExist(key, subKey))
            {
                using (RegistryKey rKey = key.AsRegistryKey().OpenSubKey(subKey, true))
                {
                    try
                    {
                        if (value is string)
                        {
                            if ((value as string).StartsWith("ValueKind") && type.AsRegistryValueKind() == RegistryValueKind.None)
                            {
                                string _valueKind = Regex.Match(value.ToString(), "ValueKind_(.+?)::").Groups[1].Value;
                                string _value = (value as string).RemoveText($"ValueKind_{_valueKind}::");
                                switch (_valueKind)
                                {
                                    case "String":
                                        rKey.SetValue(entry, _value, RegistryValueKind.String);
                                        return;
                                    case "Binary":
                                        rKey.SetValue(entry, _value.FromHexStringToByteArray(), RegistryValueKind.Binary);
                                        return;
                                    case "DWord":
                                        rKey.SetValue(entry, _value, RegistryValueKind.DWord);
                                        return;
                                    case "QWord":
                                        rKey.SetValue(entry, _value, RegistryValueKind.QWord);
                                        return;
                                    case "ExpandString":
                                        rKey.SetValue(entry, _value, RegistryValueKind.ExpandString);
                                        return;
                                    case "MultiString":
                                        rKey.SetValue(entry, _value.FromHexString().SplitNewLine(), RegistryValueKind.MultiString);
                                        return;
                                    default:
                                        return;
                                }
                            }
                        }
                        if (type.AsRegistryValueKind() == RegistryValueKind.None)
                        {
                            if (value is string)
                                rKey.SetValue(entry, value, RegistryValueKind.String);
                            else if (value is byte[])
                                rKey.SetValue(entry, value, RegistryValueKind.Binary);
                            else if (value is int)
                                rKey.SetValue(entry, value, RegistryValueKind.DWord);
                            else if (value is string[])
                                rKey.SetValue(entry, value, RegistryValueKind.MultiString);
                            else
                                rKey.SetValue(entry, value, RegistryValueKind.None);
                        }
                        else
                            rKey.SetValue(entry, value, type.AsRegistryValueKind());
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                        rKey.SetValue(entry, value, RegistryValueKind.String);
                    }
                }
            }
        }

        public static void WriteValue(object key, string subKey, string entry, object value, object type = null) => 
            WriteValue<object>(key, subKey, entry, value, type);

        public static void WriteValue(string keyPath, string entry, object value, object type = null) =>
            WriteValue<object>(keyPath.GetKey(), keyPath.GetSubKey(), entry, value, type);

        #endregion

        #region REMOVE VALUE

        public static void RemoveValue(object key, string subKey, string entry)
        {
            if (SubKeyExist(key, subKey))
            {
                try
                {
                    using (RegistryKey rKey = AsRegistryKey(key).OpenSubKey(subKey, true))
                        rKey.DeleteValue(entry);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
        }

        public static void RemoveValue(string keyPath, string entry) =>
             RemoveValue(keyPath.GetKey(), keyPath.GetSubKey(), entry);

        #endregion

        #region IMPORT FILE

        public static bool ImportFile(string path, bool elevated = false)
        {
            if (!File.Exists(path))
                return false;
            if (Path.GetExtension(path).ToLower() == ".ini")
            {
                string root = INI.Read("Root", "Sections", path);
                if (root.Contains(","))
                {
                    foreach (string section in root.Split(','))
                    {
                        string rootKey = INI.Read(section, $"{section}_RootKey", path);
                        string subKey = INI.Read(section, $"{section}_SubKey", path);
                        string values = INI.Read(section, $"{section}_Values", path);
                        if (string.IsNullOrWhiteSpace(rootKey) || string.IsNullOrWhiteSpace(subKey) || string.IsNullOrWhiteSpace(values))
                            continue;
                        foreach (string value in values.Split(','))
                        {
                            CreateNewSubKey(rootKey.AsRegistryKey(), subKey);
                            WriteValue(rootKey.AsRegistryKey(), subKey, value, INI.Read(section, value, path));
                        }
                    }
                    return true;
                }
            }
            else
            {
                int pid = RUN.App(new ProcessStartInfo()
                {
                    Arguments = $"IMPORT \"{path}\"",
                    FileName = "%WinDir%\\System32\\reg.exe",
                    Verb = elevated ? "runas" : string.Empty,
                    WindowStyle = ProcessWindowStyle.Hidden
                }, 1000);
                return pid != -1;
            }
            return false;
        }

        public static bool ImportFile(string path, string[] contents, bool elevated = false)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                File.WriteAllLines(path, contents);
                bool imported = ImportFile(path, elevated);
                if (File.Exists(path))
                    File.Delete(path);
                return imported;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return false;
        }

        public static bool ImportFile(string[] contents, bool elevated = false) =>
            ImportFile($"{Path.GetTempFileName()}.reg", contents, elevated);

        public static bool ImportFromIniFile(string path) =>
            ImportFile(Path.GetExtension(path).ToLower() == ".ini" ? path : null, false);

        #endregion

        #region EXPORT FILE

        public static void ExportToIniFile(object key, string subKey, string destIniPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(destIniPath))
                    destIniPath = PATH.Combine($"%CurDir%\\{Process.GetCurrentProcess().ProcessName}.ini");
                string path = Path.GetDirectoryName(destIniPath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (File.Exists(destIniPath))
                    File.Delete(destIniPath);
                File.Create(destIniPath).Close();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return;
            }
            string section = string.Empty;
            foreach (KeyValuePair<string, string> ent in GetAllTreeValues(key, subKey))
            {
                if (ent.Value == ent.Key.EncryptToMD5())
                {
                    string sections = INI.Read("Root", "Sections", destIniPath);
                    section = ent.Value;
                    INI.Write("Root", "Sections", $"{(!string.IsNullOrEmpty(sections) ? $"{sections}," : string.Empty)}{section}", destIniPath);
                    INI.Write(section, $"{section}_RootKey", key.AsRegistryKey(), destIniPath);
                    INI.Write(section, $"{section}_SubKey", ent.Key, destIniPath);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(section))
                    continue;
                string values = INI.Read(section, $"{section}_Values", destIniPath);
                INI.Write(section, $"{section}_Values", $"{(!string.IsNullOrEmpty(values) ? $"{values}," : string.Empty)}{ent.Key}", destIniPath);
                INI.Write(section, ent.Key, ent.Value, destIniPath);
            }
        }

        public static void ExportToIniFile(string keyPath) =>
            ExportToIniFile(keyPath.GetKey(), keyPath.GetSubKey());

        public static void ExportFile(string keyPath, string destFilePath, bool elevated = false)
        {
            string dir = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            RUN.App(new ProcessStartInfo()
            {
                Arguments = $"EXPORT \"{keyPath}\" \"{destFilePath}\" /y",
                FileName = "%WinDir%\\System32\\reg.exe",
                Verb = elevated ? "runas" : string.Empty,
                WindowStyle = ProcessWindowStyle.Hidden
            }, 1000);
        }

        #endregion
    }
}

#endregion
