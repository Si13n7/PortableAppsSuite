
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SilDev
{
    public static class Reg
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

        private static RegistryKey GetKey(object _key)
        {
            try
            {
                if (_key is RegistryKey)
                    return (RegistryKey)_key;
                if (_key is RegKey)
                {
                    switch ((RegKey)_key)
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
                else
                {
                    switch ((_key is string ? (string)_key : _key.ToString()).ToUpper())
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
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return null;
            }
        }

        private static string[] GetKeys(string _key)
        {
            string[] keys = new string[2];
            if (_key.Contains('\\'))
            {
                keys[0] = _key.Split('\\')[0];
                keys[1] = _key.Replace($"{keys[0]}\\", string.Empty);
            }
            return keys;
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

        private static RegistryValueKind GetType(object _type)
        {
            switch ((_type is string ? (string)_type : _type.ToString()).ToUpper())
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

        #endregion

        #region KEY ORDER

        public static bool SubKeyExist(object _key, string _sub)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(_sub) && GetKey(_key).OpenSubKey(_sub) != null;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool SubKeyExist(string _key)
        {
            try
            {
                string[] keys = GetKeys(_key);
                return GetKey(keys[0]).OpenSubKey(keys[1]) != null;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool CreateNewSubKey(object _key, string _sub)
        {
            try
            {
                if (!SubKeyExist(_key, _sub))
                    GetKey(_key).CreateSubKey(_sub);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool CreateNewSubKey(string _key)
        {
            string[] keys = GetKeys(_key);
            return CreateNewSubKey(GetKey(keys[0]), keys[1]);
        }

        public static bool RemoveExistSubKey(object _key, string _sub)
        {
            try
            {
                if (SubKeyExist(_key, _sub))
                    GetKey(_key).DeleteSubKeyTree(_sub);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool RemoveExistSubKey(string _key)
        {
            string[] keys = GetKeys(_key);
            return RemoveExistSubKey(GetKey(keys[0]), keys[1]);
        }

        public static List<string> GetSubKeyTree(object _key, string _sub)
        {
            try
            {
                List<string> subKeys = GetSubKeys(_key, _sub);
                if (subKeys.Count > 0)
                {
                    int count = subKeys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        List<string> subs = GetSubKeys(_key, subKeys[i]);
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
                Log.Debug(ex);
                return null;
            }
        }

        public static List<string> GetSubKeyTree(string _key)
        {
            string[] keys = GetKeys(_key);
            return GetSubKeyTree(GetKey(keys[0]), keys[1]);
        }

        public static List<string> GetSubKeys(object _key, string _sub)
        {
            try
            {
                List<string> keys = new List<string>();
                if (SubKeyExist(_key, _sub))
                {
                    using (RegistryKey key = GetKey(_key).OpenSubKey(_sub))
                        foreach (string ent in key.GetSubKeyNames())
                            keys.Add(string.Format("{0}\\{1}", _sub, ent));
                }
                return keys;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return null;
            }
        }

        public static List<string> GetSubKeys(string _key)
        {
            string[] keys = GetKeys(_key);
            return GetSubKeys(GetKey(keys[0]), keys[1]);
        }

        public static bool RenameSubKey(object _key, string _sub, string _newSubName)
        {
            if (SubKeyExist(_key, _sub) && !SubKeyExist(_key, _newSubName))
            {
                if (CopyKey(_key, _sub, _newSubName))
                {
                    try
                    {
                        GetKey(_key).DeleteSubKeyTree(_sub);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
            }
            return false;
        }

        public static bool RenameSubKey(string _key, string _newSubName)
        {
            string[] keys = GetKeys(_key);
            return RenameSubKey(GetKey(keys[0]), keys[1], _newSubName);
        }

        public static bool CopyKey(object _key, string _sub, string _newSubName)
        {
            if (SubKeyExist(_key, _sub) && !SubKeyExist(_key, _newSubName))
            {
                try
                {
                    RegistryKey destKey = GetKey(_key).CreateSubKey(_newSubName);
                    RegistryKey srcKey = GetKey(_key).OpenSubKey(_sub);
                    RecurseCopyKey(srcKey, destKey);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
            return false;
        }

        private static void RecurseCopyKey(RegistryKey _srcKey, RegistryKey _destKey)
        {
            foreach (string valueName in _srcKey.GetValueNames())
            {
                object obj = _srcKey.GetValue(valueName);
                RegistryValueKind valKind = _srcKey.GetValueKind(valueName);
                _destKey.SetValue(valueName, obj, valKind);
            }
            foreach (string sourceSubKeyName in _srcKey.GetSubKeyNames())
            {
                RegistryKey srcSubKey = _srcKey.OpenSubKey(sourceSubKeyName);
                RegistryKey destSubKey = _destKey.CreateSubKey(sourceSubKeyName);
                RecurseCopyKey(srcSubKey, destSubKey);
            }
        }

        #endregion

        #region READ VALUE

        public static Dictionary<string, string> GetAllTreeValues(object _key, string _sub)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (string sub in GetSubKeyTree(_key, _sub))
            {
                Dictionary<string, string> tmp = GetValues(_key, sub);
                if (tmp.Keys.Count > 0)
                {
                    values.Add(sub, Crypt.MD5.Encrypt(sub));
                    foreach (KeyValuePair<string, string> val in tmp)
                    {
                        try
                        {
                            values.Add(val.Key, val.Value);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex);
                        }
                    }
                }
            }
            return values;
        }

        public static Dictionary<string, string> GetAllTreeValues(string _key)
        {
            string[] keys = GetKeys(_key);
            return GetAllTreeValues(GetKey(keys[0]), keys[1]);
        }

        public static Dictionary<string, string> GetValues(object _key, string _sub)
        {
            if (!SubKeyExist(_key, _sub))
                return null;
            Dictionary<string, string> values = new Dictionary<string, string>();
            using (RegistryKey key = GetKey(_key).OpenSubKey(_sub))
            {
                foreach (string ent in key.GetValueNames())
                {
                    try
                    {
                        string value = ReadValue(_key, _sub, ent);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            RegistryValueKind type = key.GetValueKind(ent);
                            values.Add(ent, string.Format("ValueKind_{0}::{1}", type, type == RegistryValueKind.MultiString ? Crypt.Misc.ConvertToHex(value) : value));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
            }
            return values;
        }

        public static object ReadObjValue(object _key, string _sub, string _ent, object _type)
        {
            object ent = null;
            if (SubKeyExist(_key, _sub))
            {
                try
                {
                    using (RegistryKey reg = GetKey(_key).OpenSubKey(_sub))
                        ent = reg.GetValue(_ent, GetType(_type));
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
            return ent;
        }

        public static object ReadObjValue(object _key, string _sub, string _ent) => 
            ReadObjValue(_key, _sub, _ent, RegistryValueKind.None);

        public static object ReadObjValue(string _key, string _ent)
        {
            string[] keys = GetKeys(_key);
            return ReadObjValue(keys[0], keys[1], _ent, RegistryValueKind.None);
        }

        public static string ReadValue(object _key, string _sub, string _ent, object _type)
        {
            string ent = string.Empty;
            if (SubKeyExist(_key, _sub))
            {
                try
                {
                    using (RegistryKey reg = GetKey(_key).OpenSubKey(_sub))
                    {
                        object obj = reg.GetValue(_ent, GetType(_type));
                        if (obj != null)
                        {
                            if (obj is string[])
                                ent = string.Join(Environment.NewLine, obj as string[]);
                            else if (obj is byte[])
                                ent = BitConverter.ToString(obj as byte[]).Replace("-", string.Empty);
                            else
                                ent = obj.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
            return ent;
        }

        public static string ReadValue(object _key, string _sub, string _ent) => 
            ReadValue(_key, _sub, _ent, RegistryValueKind.None);

        public static string ReadValue(string _key, string _ent, object _type)
        {
            string[] keys = GetKeys(_key);
            return ReadValue(keys[0], keys[1], _ent, _type);
        }

        public static string ReadValue(string _key, string _ent) => 
            ReadValue(_key, _ent, RegistryValueKind.None);

        #endregion

        #region WRITE VALUE

        public static void WriteValue<T>(object _key, string _sub, string _ent, T _val, object _type)
        {
            if (!SubKeyExist(_key, _sub))
                CreateNewSubKey(_key, _sub);
            if (SubKeyExist(_key, _sub))
            {
                using (RegistryKey reg = GetKey(_key).OpenSubKey(_sub, true))
                {
                    try
                    {
                        if (_val is string)
                        {
                            if ((_val as string).StartsWith("ValueKind") && GetType(_type) == RegistryValueKind.None)
                            {
                                string valueKind = Regex.Match(_val.ToString(), "ValueKind_(.+?)::").Groups[1].Value;
                                string value = (_val as string).Replace(string.Format("ValueKind_{0}::", valueKind), string.Empty);
                                switch (valueKind)
                                {
                                    case "String":
                                        reg.SetValue(_ent, value, RegistryValueKind.String);
                                        return;
                                    case "Binary":
                                        reg.SetValue(_ent, Enumerable.Range(0, value.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(value.Substring(x, 2), 16)).ToArray(), RegistryValueKind.Binary);
                                        return;
                                    case "DWord":
                                        reg.SetValue(_ent, value, RegistryValueKind.DWord);
                                        return;
                                    case "QWord":
                                        reg.SetValue(_ent, value, RegistryValueKind.QWord);
                                        return;
                                    case "ExpandString":
                                        reg.SetValue(_ent, value, RegistryValueKind.ExpandString);
                                        return;
                                    case "MultiString":
                                        reg.SetValue(_ent, Crypt.Misc.ReconvertFromHex(value).Split(new string[] { Environment.NewLine }, StringSplitOptions.None), RegistryValueKind.MultiString);
                                        return;
                                    default:
                                        return;
                                }
                            }
                        }
                        if (GetType(_type) == RegistryValueKind.None)
                        {
                            if (_val is string)
                                reg.SetValue(_ent, _val, RegistryValueKind.String);
                            else if (_val is byte[])
                                reg.SetValue(_ent, _val, RegistryValueKind.Binary);
                            else if (_val is int)
                                reg.SetValue(_ent, _val, RegistryValueKind.DWord);
                            else if (_val is string[])
                                reg.SetValue(_ent, _val, RegistryValueKind.MultiString);
                            else
                                reg.SetValue(_ent, _val, RegistryValueKind.None);
                        }
                        else
                            reg.SetValue(_ent, _val, GetType(_type));
                    }
                    catch (Exception ex)
                    {
                        reg.SetValue(_ent, _val, RegistryValueKind.String);
                        Log.Debug(ex);
                    }
                }
            }
        }

        public static void WriteValue(object _key, string _sub, string _ent, object _val, object _type) => 
            WriteValue<object>(_key, _sub, _ent, _val, _type);

        public static void WriteValue(object _key, string _sub, string _ent, object _val) => 
            WriteValue<object>(_key, _sub, _ent, _val, RegistryValueKind.None);

        public static void WriteValue(string _key, string _ent, object _val)
        {
            string[] keys = GetKeys(_key);
            WriteValue<object>(GetKey(keys[0]), keys[1], _ent, _val, RegistryValueKind.None);
        }

        #endregion

        #region REMOVE VALUE

        public static void RemoveValue(object _key, string _sub, string _ent)
        {
            if (SubKeyExist(_key, _sub))
            {
                using (RegistryKey reg = GetKey(_key).OpenSubKey(_sub, true))
                {
                    try
                    {
                        reg.DeleteValue(_ent);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
            }
        }

        public static void RemoveValue(string _key, string _ent)
        {
            string[] keys = GetKeys(_key);
            RemoveValue(GetKey(keys[0]), keys[1], _ent);
        }

        #endregion

        #region IMPORT FILE

        public static bool ImportFile(string _file, bool _admin)
        {
            if (!File.Exists(_file))
                return false;
            if (_file.ToLower().EndsWith(".ini"))
            {
                string root = Initialization.ReadValue("Root", "Sections", _file);
                if (root.Contains(","))
                {
                    foreach (string section in root.Split(','))
                    {
                        string rootKey = Initialization.ReadValue(section, string.Format("{0}_RootKey", section), _file);
                        string subKey = Initialization.ReadValue(section, string.Format("{0}_SubKey", section), _file);
                        string values = Initialization.ReadValue(section, string.Format("{0}_Values", section), _file);
                        if (string.IsNullOrWhiteSpace(rootKey) || string.IsNullOrWhiteSpace(subKey) || string.IsNullOrWhiteSpace(values))
                            continue;
                        foreach (string value in values.Split(','))
                        {
                            CreateNewSubKey(GetKey(rootKey), subKey);
                            WriteValue(GetKey(rootKey), subKey, value, Initialization.ReadValue(section, value, _file));
                        }
                    }
                    return true;
                }
            }
            else
            {
                object output = Run.App(new ProcessStartInfo()
                {
                    Arguments = string.Format("IMPORT \"{0}\"", _file),
                    FileName = "%WinDir%\\System32\\reg.exe",
                    Verb = _admin ? "runas" : string.Empty,
                    WindowStyle = ProcessWindowStyle.Hidden
                }, -1, 1000);
                return output is int && (int)output > -1;
            }
            return false;
        }

        public static bool ImportFile(string _file, string[] _content, bool _admin)
        {
            try
            {
                if (File.Exists(_file))
                    File.Delete(_file);
                File.WriteAllLines(_file, _content);
                bool importSuccessful = ImportFile(_file, _admin);
                if (File.Exists(_file))
                    File.Delete(_file);
                return importSuccessful;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return false;
        }

        public static bool ImportFile(string _file, string[] _content) =>
            ImportFile(_file, _content, false);

        public static bool ImportFile(string[] _content, bool _admin)
        {
            string name = string.Format("_tmp-{0}.reg", new Random().Next(0, int.MaxValue));
            string file = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), name);
            return ImportFile(file, _content, _admin);
        }

        public static bool ImportFile(string[] _content) =>
            ImportFile(_content, false);

        public static bool ImportFile(string _file) =>
            ImportFile(_file, false);

        public static bool ImportFromIniFile() =>
            ImportFile(Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.ini", Process.GetCurrentProcess().ProcessName)), false);

        #endregion

        #region EXPORT FILE

        public static void ExportToIniFile(object _key, string _sub, string _file)
        {
            try
            {
                string path = Path.GetDirectoryName(_file);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (File.Exists(_file))
                    File.Delete(_file);
                File.Create(_file).Close();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return;
            }
            string section = string.Empty;
            foreach (KeyValuePair<string, string> ent in GetAllTreeValues(_key, _sub))
            {
                if (ent.Value == Crypt.MD5.Encrypt(ent.Key))
                {
                    section = ent.Value;
                    string sections = Initialization.ReadValue("Root", "Sections", _file);
                    Initialization.WriteValue("Root", "Sections", string.Format("{0}{1},", sections, section), _file);
                    Initialization.WriteValue(section, string.Format("{0}_RootKey", section), GetKey(_key), _file);
                    Initialization.WriteValue(section, string.Format("{0}_SubKey", section), ent.Key, _file);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(section))
                    continue;
                string values = Initialization.ReadValue(section, string.Format("{0}_Values", section), _file);
                Initialization.WriteValue(section, string.Format("{0}_Values", section), string.Format("{0}{1},", values, ent.Key), _file);
                Initialization.WriteValue(section, ent.Key, ent.Value, _file);
            }
        }

        public static void ExportToIniFile(object _key, string _sub) =>
            ExportToIniFile(_key, _sub, Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.ini", Process.GetCurrentProcess().ProcessName)));

        public static void ExportToIniFile(string _key)
        {
            string[] keys = GetKeys(_key);
            ExportToIniFile(keys[0], keys[1], Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.ini", Process.GetCurrentProcess().ProcessName)));
        }

        public static void ExportFile(string _key, string _file, bool _admin)
        {
            string dir = Path.GetDirectoryName(_file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Run.App(new ProcessStartInfo()
            {
                Arguments = string.Format("EXPORT \"{0}\" \"{1}\" /y", _key, _file),
                FileName = "%WinDir%\\System32\\reg.exe",
                Verb = _admin ? "runas" : string.Empty,
                WindowStyle = ProcessWindowStyle.Hidden
            }, -1, 1000);
        }

        public static void ExportFile(string _key, string _file) =>
            ExportFile(_key, _file, false);

        public static void ExportFile(string _key)
        {
            string name = string.Format("_tmp-{0}.reg", new Random().Next(0, int.MaxValue));
            string file = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), name);
            ExportFile(_key, file, false);
        }

        #endregion
    }
}

#endregion
