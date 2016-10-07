
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.PATH"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class INI
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string nDefault, StringBuilder retVal, int nSize, string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string nDefault, string retVal, int nSize, string lpFileName);

            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int WritePrivateProfileSection([MarshalAs(UnmanagedType.LPStr)]string lpAppName, [MarshalAs(UnmanagedType.LPStr)]string lpString, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        }

        #region DEFAULT ACCESS FILE

        private static string iniFile = null;

        public static bool File(params string[] paths)
        {
            try
            {
                iniFile = PATH.Combine(paths);
                if (System.IO.File.Exists(iniFile))
                    return true;
                string iniDir = Path.GetDirectoryName(iniFile);
                if (!Directory.Exists(iniDir))
                    Directory.CreateDirectory(iniDir);
                System.IO.File.Create(iniFile).Close();
                return true;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static string File() => 
            iniFile ?? string.Empty;

        #endregion

        #region SECTION ORDER

        public static List<string> GetSections(string fileOrContent = null, bool sorted = true)
        {
            List<string> output = new List<string>();
            try
            {
                string dest = fileOrContent ?? iniFile;
                if (string.IsNullOrWhiteSpace(dest))
                    throw new ArgumentNullException();
                string path = PATH.Combine(dest);
                if (System.IO.File.Exists(path))
                {
                    byte[] buffer = new byte[short.MaxValue];
                    if (SafeNativeMethods.GetPrivateProfileSectionNames(buffer, short.MaxValue, path) != 0)
                        output = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0').ToList();
                }
                else
                {
                    path = Path.GetTempFileName();
                    System.IO.File.WriteAllText(path, dest);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetSections(path, false);
                        System.IO.File.Delete(path);
                    }
                }
                if (sorted)
                    output = output.OrderBy(x => x, new AscendentAlphanumericStringComparer()).ToList();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return output;
        }

        public static List<string> GetSections(bool sorted) =>
            GetSections(iniFile, sorted);

        #endregion

        #region REMOVE SECTION

        public static bool RemoveSection(string section, string file = null)
        {
            try
            {
                string path = !string.IsNullOrEmpty(file) ? PATH.Combine(file) : iniFile;
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                return SafeNativeMethods.WritePrivateProfileSection(section, null, path) != 0;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        #endregion

        #region KEY ORDER

        public static List<string> GetKeys(string section, string fileOrContent = null, bool sorted = true)
        {
            List<string> output = new List<string>();
            try
            {
                string dest = fileOrContent ?? iniFile;
                string path = PATH.Combine(dest);
                if (System.IO.File.Exists(dest))
                {
                    string tmp = new string(' ', short.MaxValue);
                    if (SafeNativeMethods.GetPrivateProfileString(section, null, string.Empty, tmp, short.MaxValue, path) != 0)
                    {
                        output = new List<string>(tmp.Split('\0'));
                        output.RemoveRange(output.Count - 2, 2);
                    }
                }
                else
                {
                    path = Path.GetTempFileName();
                    System.IO.File.WriteAllText(path, dest);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetKeys(section, path, false);
                        System.IO.File.Delete(path);
                    }
                }
                if (sorted)
                    output = output.OrderBy(x => x, new AscendentAlphanumericStringComparer()).ToList();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return output;
        }

        public static List<string> GetKeys(string section, bool sorted) =>
            GetKeys(section, iniFile, sorted);

        #endregion

        #region REMOVE KEY

        public static bool RemoveKey(string section, string key, string file = null)
        {
            try
            {
                string path = !string.IsNullOrEmpty(file) ? PATH.Combine(file) : iniFile;
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                return SafeNativeMethods.WritePrivateProfileString(section, key, null, path) != 0;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        #endregion

        #region READ ALL

        public static Dictionary<string, Dictionary<string, string>> ReadAll(string fileOrContent = null, bool sorted = true)
        {
            Dictionary<string, Dictionary<string, string>> output = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                bool isContent = false;
                string source = fileOrContent ?? iniFile;
                string path = PATH.Combine(source);
                if (!System.IO.File.Exists(path))
                {
                    isContent = true;
                    path = Path.GetTempFileName();
                    System.IO.File.WriteAllText(path, source);
                }
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                List<string> sections = GetSections(path, sorted);
                if (sections.Count == 0)
                    throw new ArgumentNullException();
                foreach (string section in sections)
                {
                    List<string> keys = GetKeys(section, path, sorted);
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
                LOG.Debug(ex);
            }
            return output;
        }

        public static Dictionary<string, Dictionary<string, string>> ReadAll(bool sorted) =>
            ReadAll(iniFile, sorted);

        #endregion

        #region READ VALUE

        public static bool ValueExists(string section, string key, string fileOrContent = null) =>
            !string.IsNullOrWhiteSpace(Read(section, key, fileOrContent ?? iniFile));

        public static string Read(string section, string key, string fileOrContent = null)
        {
            string output = string.Empty;
            try
            {
                string source = fileOrContent ?? iniFile;
                string path = PATH.Combine(source);
                if (System.IO.File.Exists(source))
                {
                    StringBuilder tmp = new StringBuilder(short.MaxValue);
                    if (SafeNativeMethods.GetPrivateProfileString(section, key, string.Empty, tmp, short.MaxValue, path) != 0)
                        output = tmp.ToString();
                }
                else
                {
                    path = Path.GetTempFileName();
                    System.IO.File.WriteAllText(path, source);
                    if (System.IO.File.Exists(path))
                    {
                        output = Read(section, key, path);
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return output;
        }

        private enum IniValueKind
        {
            Boolean,
            Byte,
            ByteArray,
            DateTime,
            Double,
            Float,
            Image,
            Integer,
            Long,
            Point,
            Rectangle,
            Short,
            Size,
            String,
            StringArray,
            Version
        }

        private static object ReadObject(string section, string key, object defValue, IniValueKind valkind, string fileOrContent)
        {
            object output = null;
            string value = Read(section, key, fileOrContent);
            switch (valkind)
            {
                case IniValueKind.Boolean:
                    bool boolParser;
                    if (bool.TryParse(Read(section, key, fileOrContent), out boolParser))
                        output = boolParser;
                    break;
                case IniValueKind.Byte:
                    byte byteParser;
                    if (byte.TryParse(Read(section, key, fileOrContent), out byteParser))
                        output = byteParser;
                    break;
                case IniValueKind.ByteArray:
                    byte[] bytesParser = value.FromHexStringToByteArray();
                    if (bytesParser.Length > 0)
                        output = bytesParser;
                    break;
                case IniValueKind.DateTime:
                    DateTime dateTimeParser;
                    if (DateTime.TryParse(Read(section, key, fileOrContent), out dateTimeParser))
                        output = dateTimeParser;
                    break;
                case IniValueKind.Double:
                    double doubleParser;
                    if (double.TryParse(Read(section, key, fileOrContent), out doubleParser))
                        output = doubleParser;
                    break;
                case IniValueKind.Float:
                    float floatParser;
                    if (float.TryParse(Read(section, key, fileOrContent), out floatParser))
                        output = floatParser;
                    break;
                case IniValueKind.Image:
                    Image imageParser = value.FromHexStringToImage();
                    if (imageParser != null)
                        output = imageParser;
                    break;
                case IniValueKind.Integer:
                    int intParser;
                    if (int.TryParse(Read(section, key, fileOrContent), out intParser))
                        output = intParser;
                    break;
                case IniValueKind.Long:
                    long longParser;
                    if (long.TryParse(Read(section, key, fileOrContent), out longParser))
                        output = longParser;
                    break;
                case IniValueKind.Point:
                    Point pointParser = Read(section, key, fileOrContent).ToPoint();
                    if (pointParser != new Point(int.MinValue, int.MinValue))
                        output = pointParser;
                    break;
                case IniValueKind.Rectangle:
                    Rectangle rectParser = Read(section, key, fileOrContent).ToRectangle();
                    if (rectParser != Rectangle.Empty)
                        output = rectParser;
                    break;
                case IniValueKind.Short:
                    short shortParser;
                    if (short.TryParse(Read(section, key, fileOrContent), out shortParser))
                        output = shortParser;
                    break;
                case IniValueKind.Size:
                    Size sizeParser = Read(section, key, fileOrContent).ToSize();
                    if (sizeParser != Size.Empty)
                        output = sizeParser;
                    break;
                case IniValueKind.StringArray:
                    string[] stringsParser = value.FromHexString().Split("\0\0\0");
                    if (stringsParser.Length > 0)
                        output = stringsParser;
                    break;
                case IniValueKind.Version:
                    Version versionParser;
                    if (Version.TryParse(Read(section, key, fileOrContent), out versionParser))
                        output = versionParser;
                    break;
                default:
                    output = Read(section, key, fileOrContent);
                    if (string.IsNullOrWhiteSpace(output as string))
                        output = null;
                    break;
            }
            if (output == null)
                output = defValue;
            return output;
        }


        public static bool ReadBoolean(string section, string key, bool defValue = false, string fileOrContent = null) =>
            Convert.ToBoolean(ReadObject(section, key, defValue, IniValueKind.Boolean, fileOrContent ?? iniFile));

        public static bool ReadBoolean(string section, string key, string fileOrContent) =>
            ReadBoolean(section, key, false, fileOrContent);


        public static byte ReadByte(string section, string key, byte defValue = 0x0, string fileOrContent = null) =>
            Convert.ToByte(ReadObject(section, key, defValue, IniValueKind.Byte, fileOrContent ?? iniFile));

        public static byte ReadByte(string section, string key, string fileOrContent) =>
            ReadByte(section, key, 0x0, fileOrContent);


        public static byte[] ReadByteArray(string section, string key, byte[] defValue = null, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.ByteArray, fileOrContent ?? iniFile) as byte[];

        public static byte[] ReadByteArray(string section, string key, string fileOrContent) =>
            ReadByteArray(section, key, null, fileOrContent);


        public static DateTime ReadDateTime(string section, string key, DateTime defValue, string fileOrContent = null) =>
            Convert.ToDateTime(ReadObject(section, key, defValue, IniValueKind.DateTime, fileOrContent ?? iniFile));

        public static DateTime ReadDateTime(string section, string key, string fileOrContent) =>
            ReadDateTime(section, key, DateTime.Now, fileOrContent);

        public static DateTime ReadDateTime(string section, string key) =>
            ReadDateTime(section, key, DateTime.Now, iniFile);


        public static double ReadDouble(string section, string key, double defValue = 0d, string fileOrContent = null) =>
            Convert.ToDouble(ReadObject(section, key, defValue, IniValueKind.Double, fileOrContent ?? iniFile));

        public static double ReadDouble(string section, string key, string fileOrContent) =>
            ReadDouble(section, key, 0d, fileOrContent);


        public static float ReadFloat(string section, string key, float defValue = 0f, string fileOrContent = null) =>
            Convert.ToSingle(ReadObject(section, key, defValue, IniValueKind.Float, fileOrContent ?? iniFile));

        public static double ReadFloat(string section, string key, string fileOrContent) =>
            ReadFloat(section, key, 0f, fileOrContent);


        public static Image ReadImage(string section, string key, Image defValue = null, string fileOrContent = null) =>
            ReadObject(section, key, null, IniValueKind.Image, fileOrContent ?? iniFile) as Image;

        public static Image ReadImage(string section, string key, string fileOrContent) =>
            ReadImage(section, key, null, fileOrContent);


        public static int ReadInteger(string section, string key, int defValue = 0, string fileOrContent = null) =>
            Convert.ToInt32(ReadObject(section, key, defValue, IniValueKind.Integer, fileOrContent ?? iniFile));

        public static int ReadInteger(string section, string key, string fileOrContent) =>
            ReadInteger(section, key, 0, fileOrContent);


        public static long ReadLong(string section, string key, long defValue = 0, string fileOrContent = null) =>
            Convert.ToInt64(ReadObject(section, key, defValue, IniValueKind.Long, fileOrContent ?? iniFile));

        public static long ReadLong(string section, string key, string fileOrContent) =>
            ReadLong(section, key, 0, fileOrContent);


        public static Point ReadPoint(string section, string key, Point defValue, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.Point, fileOrContent ?? iniFile).ToString().ToPoint();

        public static Point ReadPoint(string section, string key, string fileOrContent) =>
            ReadPoint(section, key, Point.Empty, fileOrContent);

        public static Point ReadPoint(string section, string key) =>
            ReadPoint(section, key, Point.Empty, iniFile);


        public static Rectangle ReadRectangle(string section, string key, Rectangle defValue, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.Rectangle, fileOrContent ?? iniFile).ToString().ToRectangle();

        public static Rectangle ReadRectangle(string section, string key, string fileOrContent) =>
            ReadRectangle(section, key, Rectangle.Empty, fileOrContent);

        public static Rectangle ReadRectangle(string section, string key) =>
            ReadRectangle(section, key, Rectangle.Empty, iniFile);


        public static short ReadShort(string section, string key, short defValue = 0, string fileOrContent = null) =>
            Convert.ToInt16(ReadObject(section, key, defValue, IniValueKind.Short, fileOrContent ?? iniFile));

        public static short ReadShort(string section, string key, string fileOrContent) =>
            ReadShort(section, key, 0, fileOrContent);


        public static Size ReadSize(string section, string key, Size defValue, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.Size, fileOrContent ?? iniFile).ToString().ToSize();

        public static Size ReadSize(string section, string key, string fileOrContent) =>
            ReadSize(section, key, Size.Empty, fileOrContent);

        public static Size ReadSize(string section, string key) =>
            ReadSize(section, key, Size.Empty, iniFile);


        public static string ReadString(string section, string key, string defValue = "", string fileOrContent = null) =>
            Convert.ToString(ReadObject(section, key, defValue, IniValueKind.String, fileOrContent));


        public static string[] ReadStringArray(string section, string key, string[] defValue = null, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.StringArray, fileOrContent ?? iniFile) as string[];

        public static string[] ReadStringArray(string section, string key, string fileOrContent) =>
            ReadStringArray(section, key, null, fileOrContent);


        public static Version ReadVersion(string section, string key, Version defValue, string fileOrContent = null) =>
            Version.Parse(ReadObject(section, key, defValue, IniValueKind.Version, fileOrContent ?? iniFile).ToString());

        public static Version ReadVersion(string section, string key, string fileOrContent) =>
            ReadVersion(section, key, Version.Parse("0.0.0.0"), fileOrContent);

        public static Version ReadVersion(string section, string key) =>
            ReadVersion(section, key, Version.Parse("0.0.0.0"), iniFile);

        #endregion

        #region WRITE VALUE

        public static bool Write(string section, string key, object value, string file = null, bool forceOverwrite = true, bool skipExistValue = false)
        {
            try
            {
                string path = !string.IsNullOrEmpty(file) ? PATH.Combine(file) : iniFile;
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                if (value == null)
                    return RemoveKey(section, key, path);
                if (value is Image)
                {
                    Image img = value as Image;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);
                        value = ms.ToArray();
                    }
                }
                string newValue = value.ToString();
                if (value is byte[])
                    newValue = ((byte[])value).ToHexString();
                if (value is string[])
                {
                    string separator = "\0\0\0";
                    newValue = ((string[])value).Join(separator);
                    if (!newValue.Contains(separator))
                        newValue += separator;
                    newValue = newValue.ToHexString();
                }
                if (!forceOverwrite || skipExistValue)
                {
                    string curValue = Read(section, key, path);
                    if (!forceOverwrite && curValue == newValue || skipExistValue && !string.IsNullOrWhiteSpace(curValue))
                        return false;
                }
                return SafeNativeMethods.WritePrivateProfileString(section, key, newValue, path) != 0;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        public static bool Write(string section, string key, object value, bool forceOverwrite, bool skipExistValue = false) =>
            Write(section, key, value, iniFile, forceOverwrite, skipExistValue);

        #endregion
    }
}
