
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
        private static string iniFile { get; set; }

        public static bool File(string _path, string _name)
        {
            return File(System.IO.Path.Combine(_path, _name));
        }

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

        public static string File()
        {
            return (iniFile != null) ? iniFile : string.Empty;
        }

        public static List<string> GetSections(string _fileOrContent)
        {
            try
            {
                MatchCollection matches = new Regex(@"\[.*?\]").Matches(System.IO.File.Exists(_fileOrContent) ? System.IO.File.ReadAllText(_fileOrContent) : _fileOrContent);
                List<string> list = matches.Cast<Match>().Select(p => p.Value.Replace("[", string.Empty).Replace("]", string.Empty)).ToList();
                list.Sort();
                return list;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return new List<string>();
            }
        }

        public static void WriteValue(string _section, string _key, object _value, string _file)
        {
            try
            {
                if (System.IO.File.Exists(_file))
                    WinAPI.SafeNativeMethods.WritePrivateProfileString(_section, _key, _value.ToString(), _file);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void WriteValue(string _section, string _key, object _value)
        {
            WriteValue(_section, _key, _value, iniFile);
        }

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
                            string value = new Regex(string.Format("{0}.*=(.*)", _key)).Match(line).Groups[1].ToString();
                            if (!string.IsNullOrWhiteSpace(value))
                                return value.TrimStart().TrimEnd();
                        }
                    }
                    return string.Empty;
                }
                else
                {
                    StringBuilder temp = new StringBuilder(short.MaxValue);
                    WinAPI.SafeNativeMethods.GetPrivateProfileString(_section, _key, string.Empty, temp, short.MaxValue, _fileOrContent);
                    return temp.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string ReadValue(string _section, string _key)
        {
            return ReadValue(_section, _key, iniFile);
        }

        public static bool ValueExists(string _section, string _key, string _fileOrContent)
        {
            return !string.IsNullOrWhiteSpace(ReadValue(_section, _key, _fileOrContent));
        }

        public static bool ValueExists(string _section, string _key)
        {
            return ValueExists(_section, _key, iniFile);
        }
    }
}

#endregion
