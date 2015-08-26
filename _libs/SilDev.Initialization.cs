
#region SILENT DEVELOPMENTS generated code

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SilDev
{
    public static class Initialization
    {
        private static string iniFile { get; set; }

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string _section, string _key, string _def, StringBuilder _retVal, int _size, string _file);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string _section, string _key, string _val, string _file);

        public static void File(string _path, string _name)
        {
            File(System.IO.Path.Combine(_path, _name));
        }

        public static void File(string _path)
        {
            iniFile = _path;
            try
            {
                if (!System.IO.File.Exists(iniFile))
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(iniFile)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(iniFile));
                    System.IO.File.Create(iniFile).Close();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static string File()
        {
            return (iniFile != null) ? iniFile : string.Empty;
        }

        public static void WriteValue(string _section, string _key, object _value, string _file)
        {
            try
            {
                if (System.IO.File.Exists(_file))
                    WritePrivateProfileString(_section, _key, _value.ToString(), _file);
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
                    throw new Exception("Value does not exists.");
                }
                else
                {
                    StringBuilder temp = new StringBuilder(short.MaxValue);
                    GetPrivateProfileString(_section, _key, string.Empty, temp, short.MaxValue, _fileOrContent);
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
