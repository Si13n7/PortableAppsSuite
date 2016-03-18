
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SilDev
{
    public static class Ini
    {
        #region INIT DEFAULT FILE

        private static string iniFile = null;

        public static bool File(string _path, string _name) => 
            File(Path.Combine(_path, _name));

        public static bool File(string _path)
        {
            iniFile = _path;
            if (!System.IO.File.Exists(iniFile))
            {
                try
                {
                    string iniDir = Path.GetDirectoryName(iniFile);
                    if (!Directory.Exists(iniDir))
                        Directory.CreateDirectory(iniDir);
                    System.IO.File.Create(iniFile).Close();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
                return System.IO.File.Exists(iniFile);
            }
            return true;
        }

        public static string File() => 
            iniFile != null ? iniFile : string.Empty;

        #endregion

        #region SECTION ORDER

        public static List<string> GetSections(string _fileOrContent, bool _sorted)
        {
            List<string> output = new List<string>();
            try
            {
                if (System.IO.File.Exists(_fileOrContent))
                {
                    byte[] buffer = new byte[short.MaxValue];
                    if (WinAPI.SafeNativeMethods.GetPrivateProfileSectionNames(buffer, short.MaxValue, _fileOrContent) != 0)
                        output = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0').ToList();
                }
                else
                {
                    string file = $"{Process.GetCurrentProcess().ProcessName}-{{{Guid.NewGuid()}}}.ini";
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), file);
                    System.IO.File.WriteAllText(path, _fileOrContent);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetSections(path, _sorted);
                        System.IO.File.Delete(path);
                    }
                }
                if (_sorted)
                    output.Sort();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static List<string> GetSections(string _fileOrContent) =>
            GetSections(_fileOrContent, true);

        #endregion

        #region REMOVE SECTION

        public static bool RemoveSection(string _section, string _file)
        {
            try
            {
                if (!System.IO.File.Exists(_file))
                    throw new FileNotFoundException();
                return WinAPI.SafeNativeMethods.WritePrivateProfileSection(_section, null, _file) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool RemoveSection(string _section) =>
            RemoveSection(_section, iniFile);

        #endregion

        #region KEY ORDER

        public static List<string> GetKeys(string _section, string _fileOrContent, bool _sorted)
        {
            List<string> output = new List<string>();
            try
            {
                if (System.IO.File.Exists(_fileOrContent))
                {
                    string tmp = new string(' ', short.MaxValue);
                    if (WinAPI.SafeNativeMethods.GetPrivateProfileString(_section, null, string.Empty, tmp, short.MaxValue, _fileOrContent) != 0)
                    {
                        output = new List<string>(tmp.Split('\0'));
                        output.RemoveRange(output.Count - 2, 2);
                    }
                }
                else
                {
                    string file = $"{Process.GetCurrentProcess().ProcessName}-{{{Guid.NewGuid()}}}.ini";
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), file);
                    System.IO.File.WriteAllText(path, _fileOrContent);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetKeys(_section, path, _sorted);
                        System.IO.File.Delete(path);
                    }
                }
                if (_sorted)
                    output.Sort();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static List<string> GetKeys(string _section, string _fileOrContent) =>
            GetKeys(_section, _fileOrContent, true);

        public static List<string> GetKeys(string _section, bool _sorted) =>
            GetKeys(_section, iniFile, _sorted);

        public static List<string> GetKeys(string _section) =>
            GetKeys(_section, iniFile, true);

        #endregion

        #region REMOVE KEY

        public static bool RemoveKey(string _section, string _key, string _file)
        {
            try
            {
                if (!System.IO.File.Exists(_file))
                    throw new FileNotFoundException();
                return WinAPI.SafeNativeMethods.WritePrivateProfileString(_section, _key, null, _file) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool RemoveKey(string _section, string _key) =>
            RemoveKey(_section, _key, iniFile);

        #endregion

        #region READ ALL

        public static Dictionary<string, Dictionary<string, string>> ReadAll(string _fileOrContent, bool _sorted)
        {
            Dictionary<string, Dictionary<string, string>> output = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                bool isContent = false;
                string path = _fileOrContent;
                if (!System.IO.File.Exists(path))
                {
                    isContent = true;
                    string file = $"{Process.GetCurrentProcess().ProcessName}-{{{Guid.NewGuid()}}}.ini";
                    path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), file);
                    System.IO.File.WriteAllText(path, _fileOrContent);
                }
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                List<string> sections = GetSections(path, _sorted);
                if (sections.Count == 0)
                    throw new ArgumentNullException();
                foreach (string section in sections)
                {
                    List<string> keys = GetKeys(section, path, _sorted);
                    if (keys.Count == 0)
                        continue;
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    foreach (string key in keys)
                    {
                        string value = Read(section, key, path);
                        if (string.IsNullOrWhiteSpace(value))
                            continue;
                        values.Add(key, value);
                    }
                    if (values.Count == 0)
                        continue;
                    if (!output.ContainsKey(section))
                        output.Add(section, values);
                }
                if (isContent)
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static Dictionary<string, Dictionary<string, string>> ReadAll(string _fileOrContent) =>
            ReadAll(_fileOrContent, true);

        public static Dictionary<string, Dictionary<string, string>> ReadAll(bool _sorted) =>
            ReadAll(iniFile, _sorted);

        public static Dictionary<string, Dictionary<string, string>> ReadAll() =>
            ReadAll(iniFile, true);

        #endregion

        #region READ VALUE

        public static bool ValueExists(string _section, string _key, string _fileOrContent) =>
            !string.IsNullOrWhiteSpace(Read(_section, _key, _fileOrContent));

        public static bool ValueExists(string _section, string _key) =>
            ValueExists(_section, _key, iniFile);

        public static string Read(string _section, string _key, string _fileOrContent)
        {
            string output = string.Empty;
            try
            {
                if (System.IO.File.Exists(_fileOrContent))
                {
                    StringBuilder tmp = new StringBuilder(short.MaxValue);
                    if (WinAPI.SafeNativeMethods.GetPrivateProfileString(_section, _key, string.Empty, tmp, short.MaxValue, _fileOrContent) != 0)
                        output = tmp.ToString();
                }
                else
                {
                    string file = $"{Process.GetCurrentProcess().ProcessName}-{{{Guid.NewGuid()}}}.ini";
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), file);
                    System.IO.File.WriteAllText(path, _fileOrContent);
                    if (System.IO.File.Exists(path))
                    {
                        output = Read(_section, _key, path);
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static string Read(string _section, string _key) =>
            Read(_section, _key, iniFile);

        private enum IniValueKind
        {
            Boolean,
            Byte,
            ByteArray,
            DateTime,
            Double,
            Float,
            Integer,
            Long,
            Short,
            String,
            Version
        }

        private static object ReadObject(string _section, string _key, object _defValue, IniValueKind _valkind, string _fileOrContent)
        {
            object output = null;
            string value = Read(_section, _key, _fileOrContent);
            switch (_valkind)
            {
                case IniValueKind.Boolean:
                    bool boolParser;
                    if (bool.TryParse(Read(_section, _key, _fileOrContent), out boolParser))
                        output = boolParser;
                    break;
                case IniValueKind.Byte:
                    byte byteParser;
                    if (byte.TryParse(Read(_section, _key, _fileOrContent), out byteParser))
                        output = byteParser;
                    break;
                case IniValueKind.ByteArray:
                    byte[] bytesParser = Enumerable.Range(0, value.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(value.Substring(x, 2), 16)).ToArray();
                    if (bytesParser.Length > 0)
                        output = bytesParser;
                    break;
                case IniValueKind.DateTime:
                    DateTime dateTimeParser;
                    if (DateTime.TryParse(Read(_section, _key, _fileOrContent), out dateTimeParser))
                        output = dateTimeParser;
                    break;
                case IniValueKind.Double:
                    double doubleParser;
                    if (double.TryParse(Read(_section, _key, _fileOrContent), out doubleParser))
                        output = doubleParser;
                    break;
                case IniValueKind.Float:
                    float floatParser;
                    if (float.TryParse(Read(_section, _key, _fileOrContent), out floatParser))
                        output = floatParser;
                    break;
                case IniValueKind.Integer:
                    int intParser;
                    if (int.TryParse(Read(_section, _key, _fileOrContent), out intParser))
                        output = intParser;
                    break;
                case IniValueKind.Long:
                    long longParser;
                    if (long.TryParse(Read(_section, _key, _fileOrContent), out longParser))
                        output = longParser;
                    break;
                case IniValueKind.Short:
                    short shortParser;
                    if (short.TryParse(Read(_section, _key, _fileOrContent), out shortParser))
                        output = shortParser;
                    break;
                case IniValueKind.Version:
                    Version versionParser;
                    if (Version.TryParse(Read(_section, _key, _fileOrContent), out versionParser))
                        output = versionParser;
                    break;
                default:
                    output = Read(_section, _key, _fileOrContent);
                    if (string.IsNullOrWhiteSpace(output as string))
                        output = null;
                    break;
            }

            if (output == null)
                output = _defValue;
            return output;
        }

        public static bool ReadBoolean(string _section, string _key, bool _defValue, string _fileOrContent) =>
            Convert.ToBoolean(ReadObject(_section, _key, _defValue, IniValueKind.Boolean, _fileOrContent));

        public static bool ReadBoolean(string _section, string _key, bool _defValue) =>
            ReadBoolean(_section, _key, _defValue, iniFile);

        public static bool ReadBoolean(string _section, string _key, string _fileOrContent) =>
            ReadBoolean(_section, _key, false, _fileOrContent);

        public static bool ReadBoolean(string _section, string _key) =>
            ReadBoolean(_section, _key, false, iniFile);

        public static byte ReadByte(string _section, string _key, byte _defValue, string _fileOrContent) =>
            Convert.ToByte(ReadObject(_section, _key, _defValue, IniValueKind.Byte, _fileOrContent));

        public static byte ReadByte(string _section, string _key, byte _defValue) =>
            ReadByte(_section, _key, _defValue, iniFile);

        public static byte ReadByte(string _section, string _key, string _fileOrContent) =>
            ReadByte(_section, _key, 0x0, _fileOrContent);

        public static byte ReadByte(string _section, string _key) =>
            ReadByte(_section, _key, 0x0, iniFile);

        public static byte[] ReadByteArray(string _section, string _key, byte[] _defValue, string _fileOrContent) =>
            ReadObject(_section, _key, _defValue, IniValueKind.ByteArray, _fileOrContent) as byte[];

        public static byte[] ReadByteArray(string _section, string _key, byte[] _defValue) =>
            ReadByteArray(_section, _key, _defValue, iniFile);

        public static byte[] ReadByteArray(string _section, string _key, string _fileOrContent) =>
            ReadByteArray(_section, _key, null, _fileOrContent);

        public static byte[] ReadByteArray(string _section, string _key) =>
            ReadByteArray(_section, _key, null, iniFile);

        public static DateTime ReadDateTime(string _section, string _key, DateTime _defValue, string _fileOrContent) =>
            Convert.ToDateTime(ReadObject(_section, _key, _defValue, IniValueKind.DateTime, _fileOrContent));

        public static DateTime ReadDateTime(string _section, string _key, DateTime _defValue) =>
            ReadDateTime(_section, _key, _defValue, iniFile);

        public static DateTime ReadDateTime(string _section, string _key, string _fileOrContent) =>
            ReadDateTime(_section, _key, DateTime.Now, _fileOrContent);

        public static DateTime ReadDateTime(string _section, string _key) =>
            ReadDateTime(_section, _key, DateTime.Now, iniFile);

        public static double ReadDouble(string _section, string _key, double _defValue, string _fileOrContent) =>
            Convert.ToDouble(ReadObject(_section, _key, _defValue, IniValueKind.Double, _fileOrContent));

        public static double ReadDouble(string _section, string _key, double _defValue) =>
            ReadDouble(_section, _key, _defValue, iniFile);

        public static double ReadDouble(string _section, string _key, string _fileOrContent) =>
            ReadDouble(_section, _key, 0d, _fileOrContent);

        public static double ReadDouble(string _section, string _key) =>
            ReadDouble(_section, _key, 0d, iniFile);

        public static float ReadFloat(string _section, string _key, float _defValue, string _fileOrContent) =>
            Convert.ToSingle(ReadObject(_section, _key, _defValue, IniValueKind.Float, _fileOrContent));

        public static double ReadFloat(string _section, string _key, float _defValue) =>
            ReadFloat(_section, _key, _defValue, iniFile);

        public static double ReadFloat(string _section, string _key, string _fileOrContent) =>
            ReadFloat(_section, _key, 0f, _fileOrContent);

        public static double ReadFloat(string _section, string _key) =>
            ReadFloat(_section, _key, 0f, iniFile);

        public static int ReadInteger(string _section, string _key, int _defValue, string _fileOrContent) =>
            Convert.ToInt32(ReadObject(_section, _key, _defValue, IniValueKind.Integer, _fileOrContent));

        public static int ReadInteger(string _section, string _key, int _defValue) =>
            ReadInteger(_section, _key, _defValue, iniFile);

        public static int ReadInteger(string _section, string _key, string _fileOrContent) =>
            ReadInteger(_section, _key, 0, _fileOrContent);

        public static int ReadInteger(string _section, string _key) =>
            ReadInteger(_section, _key, 0, iniFile);

        public static long ReadLong(string _section, string _key, long _defValue, string _fileOrContent) =>
            Convert.ToInt64(ReadObject(_section, _key, _defValue, IniValueKind.Long, _fileOrContent));

        public static long ReadLong(string _section, string _key, long _defValue) =>
            ReadLong(_section, _key, _defValue, iniFile);

        public static long ReadLong(string _section, string _key, string _fileOrContent) =>
            ReadLong(_section, _key, 0, _fileOrContent);

        public static long ReadLong(string _section, string _key) =>
            ReadLong(_section, _key, 0, iniFile);

        public static short ReadShort(string _section, string _key, short _defValue, string _fileOrContent) =>
            Convert.ToInt16(ReadObject(_section, _key, _defValue, IniValueKind.Short, _fileOrContent));

        public static short ReadShort(string _section, string _key, short _defValue) =>
            ReadShort(_section, _key, _defValue, iniFile);

        public static short ReadShort(string _section, string _key, string _fileOrContent) =>
            ReadShort(_section, _key, 0, _fileOrContent);

        public static short ReadShort(string _section, string _key) =>
            ReadShort(_section, _key, 0, iniFile);

        public static string ReadString(string _section, string _key, string _defValue, string _fileOrContent) =>
            Convert.ToString(ReadObject(_section, _key, _defValue, IniValueKind.String, _fileOrContent));

        public static string ReadString(string _section, string _key, string _defValue) =>
            ReadString(_section, _key, _defValue, iniFile);

        public static string ReadString(string _section, string _key) =>
            ReadString(_section, _key, string.Empty, iniFile);

        public static Version ReadVersion(string _section, string _key, Version _defValue, string _fileOrContent) =>
            Version.Parse(ReadObject(_section, _key, _defValue, IniValueKind.Version, _fileOrContent).ToString());

        public static Version ReadVersion(string _section, string _key, Version _defValue) =>
            ReadVersion(_section, _key, _defValue, iniFile);

        public static Version ReadVersion(string _section, string _key, string _fileOrContent) =>
            ReadVersion(_section, _key, Version.Parse("0.0.0.0"), _fileOrContent);

        public static Version ReadVersion(string _section, string _key) =>
            ReadVersion(_section, _key, Version.Parse("0.0.0.0"), iniFile);

        #endregion

        #region WRITE VALUE

        public static bool Write(string _section, string _key, object _value, string _file, bool _forceOverwrite, bool _skipExistValue)
        {
            try
            {
                if (!System.IO.File.Exists(_file))
                    throw new FileNotFoundException();
                if (_value == null)
                    return RemoveKey(_section, _key, _file);
                string newValue = _value.ToString();
                if (_value is byte[])
                    newValue = Crypt.Misc.ByteArrayToString((byte[])_value);
                if (!_forceOverwrite || _skipExistValue)
                {
                    string curValue = Read(_section, _key, _file);
                    if (!_forceOverwrite && curValue == newValue || _skipExistValue && !string.IsNullOrWhiteSpace(curValue))
                        return false;
                }
                return WinAPI.SafeNativeMethods.WritePrivateProfileString(_section, _key, newValue, _file) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool Write(string _section, string _key, object _value, string _file, bool _forceOverwrite) =>
            Write(_section, _key, _value, _file, _forceOverwrite, false);

        public static bool Write(string _section, string _key, object _value, string _file) =>
            Write(_section, _key, _value, _file, true, false);

        public static bool Write(string _section, string _key, object _value, bool _forceOverwrite, bool _skipExistValue) =>
            Write(_section, _key, _value, iniFile, _forceOverwrite, _skipExistValue);

        public static bool Write(string _section, string _key, object _value, bool _forceOverwrite) =>
            Write(_section, _key, _value, iniFile, _forceOverwrite, false);

        public static bool Write(string _section, string _key, object _value) =>
            Write(_section, _key, _value, iniFile, true, false);

        #endregion
    }
}

#endregion
