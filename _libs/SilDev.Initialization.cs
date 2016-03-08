
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilDev
{
    public static class Initialization
    {
        private static string iniFile = null;

        public static bool File(string _path, string _name) => 
            File(System.IO.Path.Combine(_path, _name));

        public static bool File(string _path)
        {
            iniFile = _path;
            if (!System.IO.File.Exists(iniFile))
            {
                try
                {
                    string iniDir = System.IO.Path.GetDirectoryName(iniFile);
                    if (!System.IO.Directory.Exists(iniDir))
                        System.IO.Directory.CreateDirectory(iniDir);
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

        public static List<string> GetSections(string _fileOrContent, bool _sorted)
        {
            try
            {
                MatchCollection matches = new Regex(@"\[.*?\]").Matches((System.IO.File.Exists(_fileOrContent) ? System.IO.File.ReadAllText(_fileOrContent) : _fileOrContent).Replace(";[", ";"));
                List<string> list = matches.Cast<Match>().Select(p => p.Value.Replace("[", string.Empty).Replace("]", string.Empty)).ToList();
                if (_sorted)
                    list.Sort();
                return list;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return new List<string>();
            }
        }

        public static List<string> GetSections(string _fileOrContent) =>
            GetSections(_fileOrContent, true);

        public static bool ValueExists(string _section, string _key, string _fileOrContent) =>
            !string.IsNullOrWhiteSpace(ReadValue(_section, _key, _fileOrContent));

        public static bool ValueExists(string _section, string _key) =>
            ValueExists(_section, _key, iniFile);

        public static string ReadValue(string _section, string _key, string _fileOrContent)
        {
            try
            {
                if (!System.IO.File.Exists(_fileOrContent))
                {
                    bool sectionFound = false;
                    foreach (string line in _fileOrContent.Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        if (line.TrimStart().StartsWith("[") && line.TrimEnd().EndsWith("]"))
                        {
                            if (!sectionFound)
                            {
                                string section = new Regex("<(.*)>").Match(line.Replace("[", "<").Replace("]", ">")).Groups[1].ToString().Trim();
                                sectionFound = section == _section;
                                continue;
                            }
                            throw new Exception($"Value does not exists. - Section: '{_section}'; Key: '{_key}';");
                        }
                        if (sectionFound && line.Contains("="))
                        {
                            string value = new Regex($"{_key}.*=(.*)").Match(line).Groups[1].ToString();
                            if (!string.IsNullOrWhiteSpace(value))
                                return value.Trim();
                        }
                    }
                    return string.Empty;
                }
                else
                {
                    StringBuilder tmp = new StringBuilder(short.MaxValue);
                    WinAPI.SafeNativeMethods.GetPrivateProfileString(_section, _key, string.Empty, tmp, short.MaxValue, _fileOrContent);
                    return tmp.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string ReadValue(string _section, string _key) => 
            ReadValue(_section, _key, iniFile);


        public static void WriteValue(string _section, string _key, object _value, string _file, bool _forceOverwrite, bool _skipExistValue)
        {
            try
            {
                if (!System.IO.File.Exists(_file))
                    throw new System.IO.FileNotFoundException();
                if (!_forceOverwrite || _skipExistValue)
                {
                    string value = ReadValue(_section, _key, _file);
                    if (!_forceOverwrite && value == _value.ToString() || _skipExistValue && !string.IsNullOrWhiteSpace(value))
                        return;
                }
                WinAPI.SafeNativeMethods.WritePrivateProfileString(_section, _key, _value.ToString(), _file);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void WriteValue(string _section, string _key, object _value, string _file, bool _forceOverwrite) =>
            WriteValue(_section, _key, _value, _file, _forceOverwrite, false);

        public static void WriteValue(string _section, string _key, object _value, string _file) =>
            WriteValue(_section, _key, _value, _file, true, false);

        public static void WriteValue(string _section, string _key, object _value, bool _forceOverwrite, bool _skipExistValue) =>
            WriteValue(_section, _key, _value, iniFile, _forceOverwrite, _skipExistValue);

        public static void WriteValue(string _section, string _key, object _value, bool _forceOverwrite) =>
            WriteValue(_section, _key, _value, iniFile, _forceOverwrite, false);

        public static void WriteValue(string _section, string _key, object _value) =>
            WriteValue(_section, _key, _value, iniFile, true, false);
    }
}

#endregion
