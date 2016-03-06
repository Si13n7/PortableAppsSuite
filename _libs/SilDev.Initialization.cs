
#region SILENT DEVELOPMENTS generated code

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
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(iniFile)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(iniFile));
                    System.IO.File.Create(iniFile).Close();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
                return false;
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

        public static void WriteValue(string _section, string _key, object _value, string _file)
        {
            if (System.IO.File.Exists(_file))
                WinAPI.SafeNativeMethods.WritePrivateProfileString(_section, _key, _value.ToString(), _file);
        }

        public static void WriteValue(string _section, string _key, object _value) => 
            WriteValue(_section, _key, _value, iniFile);

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
                                string section = new Regex("<(.*)>").Match(line.Replace("[", "<").Replace("]", ">")).Groups[1].ToString();
                                sectionFound = section == _section;
                                continue;
                            }
                            throw new Exception("Value does not exists.");
                        }
                        if (sectionFound && line.Contains("="))
                        {
                            string value = new Regex($"{_key}.*=(.*)").Match(line).Groups[1].ToString();
                            if (!string.IsNullOrWhiteSpace(value))
                                return value.TrimStart().TrimEnd();
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

        public static bool ValueExists(string _section, string _key, string _fileOrContent) => 
            !string.IsNullOrWhiteSpace(ReadValue(_section, _key, _fileOrContent));

        public static bool ValueExists(string _section, string _key) => 
            ValueExists(_section, _key, iniFile);
    }
}

#endregion
