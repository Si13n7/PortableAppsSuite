using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SilDev
{
    public static class XmlFile
    {
        private static string xmlPath;
        private static string xmlName;
        private static string xmlFile;

        public static void File(string _path, string _name)
        {
            xmlPath = _path;
            xmlName = _name;
            xmlFile = System.IO.Path.Combine(_path, _name);
        }

        public static string File()
        {
            return (xmlFile != null) ? xmlFile : string.Empty;
        }

        public static string GetXmlContent(string _xmlPath)
        {
            if (System.IO.File.Exists(_xmlPath))
            {
                string content = string.Empty;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(_xmlPath))
                {
                    content = sr.ReadToEnd();
                    sr.Dispose();
                }
                return content;
            }
            else
                return string.Empty;
        }

        public static string GetXmlContent()
        {
            return GetXmlContent(xmlFile);
        }

        public static string GetXmlValue(string _xmlContent, string _xmlKey)
        {
            return Regex.Match(_xmlContent, string.Format("<{0}>(.+?)</{0}>", _xmlKey)).Groups[1].Value;
        }

        public static string GetXmlValue(string _xmlKey)
        {
            return Regex.Match(GetXmlContent(), string.Format("<{0}>(.+?)</{0}>", _xmlKey)).Groups[1].Value;
        }

        public static void SetXmlValue(string _xmlPath, string _xmlKey, string _xmlValue)
        {
            string content = GetXmlContent(_xmlPath);
            if (!string.IsNullOrWhiteSpace(content))
            {
                string value = GetXmlValue(content, _xmlKey);
                if (System.IO.File.Exists(_xmlPath))
                    System.IO.File.Delete(_xmlPath);
                if (!System.IO.File.Exists(_xmlPath))
                    System.IO.File.WriteAllText(_xmlPath, content.Replace(value, _xmlValue));
            }
        }

        public static void SetXmlValue(string _xmlKey, string _xmlValue)
        {
            SetXmlValue(xmlFile, _xmlKey, _xmlValue);
        }
    }
}
