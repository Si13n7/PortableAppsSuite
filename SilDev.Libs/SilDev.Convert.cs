
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class CONVERT
    {
        public enum NewLineFormat
        {
            CarriageReturn = '\u000D',
            FormFeed = '\u000C',
            LineFeed = '\u000A',
            LineSeparator = '\u2028',
            NextLine = '\u0085',
            ParagraphSeparator = '\u2029',
            VerticalTab = '\u000B',
            WindowsDefault = -1
        }

        public static string FormatNewLine(this string text, NewLineFormat newLineFormat = NewLineFormat.WindowsDefault)
        {
            try
            {
                string[] sa = Enum.GetValues(typeof(NewLineFormat)).Cast<NewLineFormat>().Select(c => (int)c == -1 ? null : $"{(char)c.GetHashCode()}").ToArray();
                string f = (int)newLineFormat == -1 ? Environment.NewLine : $"{(char)newLineFormat.GetHashCode()}";
                string s = text.Replace(Environment.NewLine, $"{(char)NewLineFormat.LineFeed}");
                return s.Split(sa, StringSplitOptions.None).Join(f);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string[] ToLogStringArray(this string text)
        {
            try
            {
                int i = 0;
                double b = Math.Floor(Math.Log(text.Length));
                return text.ToLookup(c => Math.Floor(i++ / b)).Select(e => new string(e.ToArray())).ToArray();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static string Reverse(this string text)
        {
            try
            {
                string s = text;
                char[] ca = s.ToCharArray();
                Array.Reverse(ca);
                s = new string(ca);
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string Join(this IEnumerable<string> strings, string separator = null)
        {
            try
            {
                string s = string.Join(separator, strings);
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static string Join(this IEnumerable<string> strings, char separator) =>
            strings.Join(separator.ToString());

        public static string[] Sort(this string[] strings)
        {
            try
            {
                string[] sa = strings;
                Array.Sort(sa);
                return sa;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static string[] Split(this string text, string separator = "\r\n", StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            try
            {
                string[] sa = text.Split(new string[] { separator }, splitOptions);
                return sa;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return null;
            }
        }

        public static string[] SplitNewLine(this string text, params string[] strings) =>
            text.Split(Environment.NewLine);

        public static string LowerText(this string text, params string[] strings)
        {
            try
            {
                string s = text;
                foreach (string x in strings)
                    s = Regex.Replace(s, x, x.ToLower(), RegexOptions.IgnoreCase);
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string UpperText(this string text, params string[] strings)
        {
            try
            {
                string s = text;
                foreach (string x in strings)
                    s = Regex.Replace(s, x, x.ToUpper(), RegexOptions.IgnoreCase);
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string RemoveChar(this string text, params char[] chars)
        {
            try
            {
                string s = new string(text.Where(c => !chars.Contains(c)).ToArray());
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string RemoveText(this string text, params string[] strings)
        {
            try
            {
                string s = text;
                foreach (string x in strings)
                    s = s.Replace(x, string.Empty);
                return s;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return text;
            }
        }

        public static string ToBinaryString(this string text, bool separator = true)
        {
            try
            {
                byte[] ba = Encoding.UTF8.GetBytes(text);
                string s = separator ? " " : string.Empty;
                s = ba.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join(s);
                return s;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string FromBinaryString(this string bin)
        {
            try
            {
                string s = bin.RemoveChar(' ', ':', '\r', '\n');
                if (s.Count(c => !("01").Contains(c)) > 0)
                    throw new ArgumentException();
                List<byte> bl = new List<byte>();
                for (int i = 0; i < s.Length; i += 8)
                    bl.Add(Convert.ToByte(s.Substring(i, 8), 2));
                s = Encoding.UTF8.GetString(bl.ToArray());
                return s;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ToHexString(this byte[] bytes, bool separator = false, bool upper = false)
        {
            try
            {
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                string s = sb.ToString();
                if (separator)
                {
                    int i = 0;
                    s = s.ToLookup(c => Math.Floor(i++ / 2d)).Select(e => new string(e.ToArray())).Join(" ");
                }
                if (upper)
                    s = s.ToUpper();
                return s;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ToHexString(this string text, bool separator = false, bool upper = false) =>
            text.ToByteArray().ToHexString(separator, upper);

        public static byte[] FromHexStringToByteArray(this string hex)
        {
            try
            {
                string s = hex.RemoveChar(' ', ':', '\r', '\n').ToUpper();
                if (s.Count(c => !("0123456789ABCDEF").Contains(c)) > 0)
                    throw new ArgumentException();
                byte[] ba = Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
                return ba;
            }
            catch
            {
                return null;
            }
        }

        public static Image FromHexStringToImage(this string hex) =>
            FromByteArrayToImage(hex.FromHexStringToByteArray());

        public static string FromHexString(this string hex)
        {
            try
            {
                byte[] ba = hex.FromHexStringToByteArray();
                if (ba == null)
                    throw new ArgumentException();
                return Encoding.UTF8.GetString(ba);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Rectangle ToRectangle(this string value)
        {
            try
            {
                string s = value;
                if (s.StartsWith("{") && s.EndsWith("}"))
                    s = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(",", ";");
                RectangleConverter converter = new RectangleConverter();
                Rectangle rect = (Rectangle)converter.ConvertFrom(s);
                return rect;
            }
            catch
            {
                return Rectangle.Empty;
            }
        }

        public static Point ToPoint(this string value)
        {
            try
            {
                string s = value;
                if (s.StartsWith("{") && s.EndsWith("}"))
                    s = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(",", ";");
                PointConverter converter = new PointConverter();
                Point point = (Point)converter.ConvertFrom(s);
                return point;
            }
            catch
            {
                return new Point(int.MinValue, int.MinValue);
            }
        }

        public static Size ToSize(this string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                string s = value;
                if (s.StartsWith("{") && s.EndsWith("}"))
                    s = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(",", ";");
                SizeConverter converter = new SizeConverter();
                Size size = (Size)converter.ConvertFrom(s);
                return size;
            }
            catch
            {
                return Size.Empty;
            }
        }

        public static bool ToBoolean(this string value)
        {
            try
            {
                bool b;
                if (!bool.TryParse(value, out b))
                    throw new ArgumentException();
                return b;
            }
            catch
            {
                return false;
            }
        }

        public static byte[] ToByteArray(this string text)
        {
            try
            {
                byte[] ba = Encoding.UTF8.GetBytes(text);
                return ba;
            }
            catch
            {
                return null;
            }
        }

        public static string FromByteArrayToString(this byte[] bytes)
        {
            try
            {
                string s = Encoding.UTF8.GetString(bytes);
                return s;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Image FromByteArrayToImage(this byte[] bytes)
        {
            try
            {
                Image img = null;
                using (MemoryStream ms = new MemoryStream(bytes))
                    img = Image.FromStream(ms);
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ReplaceBytes(this byte[] source, byte[] oldValue, byte[] newValue)
        {
            try
            {
                byte[] ba;
                int index = -1;
                int match = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i] == oldValue[match])
                    {
                        if (match == oldValue.Length - 1)
                        {
                            index = i - match;
                            break;
                        }
                        match++;
                    }
                    else
                        match = 0;
                }
                index = match;
                if (index < 0)
                    throw new ArgumentNullException();
                ba = new byte[source.Length - oldValue.Length + newValue.Length];
                Buffer.BlockCopy(source, 0, ba, 0, index);
                Buffer.BlockCopy(newValue, 0, ba, index, newValue.Length);
                Buffer.BlockCopy(source, index + oldValue.Length, ba, index + newValue.Length, source.Length - (index + oldValue.Length));
                return ba;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return source;
            }
        }
    }

    public class AscendentAlphanumericStringComparer : IComparer<string>
    {
        public int Compare(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            int i1 = 0;
            int i2 = 0;
            while (i1 < s1.Length && i2 < s2.Length)
            {
                char c1 = s1[i1];
                char[] ca1 = new char[s1.Length];
                int l1 = 0;
                char c2 = s2[i2];
                char[] ca2 = new char[s2.Length];
                int l2 = 0;
                do
                {
                    ca1[l1++] = c1;
                    i1++;

                    if (i1 < s1.Length)
                        c1 = s1[i1];
                    else
                        break;
                }
                while (char.IsDigit(c1) == char.IsDigit(ca1[0]));

                do
                {
                    ca2[l2++] = c2;
                    i2++;
                    if (i2 < s2.Length)
                        c2 = s2[i2];
                    else
                        break;
                }
                while (char.IsDigit(c2) == char.IsDigit(ca2[0]));

                string str1 = new string(ca1);
                string str2 = new string(ca2);
                int r;
                if (char.IsDigit(ca1[0]) && char.IsDigit(ca2[0]))
                {
                    int ch1 = int.Parse(str1);
                    int ch2 = int.Parse(str2);
                    r = ch1.CompareTo(ch2);
                }
                else
                    r = str1.CompareTo(str2);

                if (r != 0)
                    return r;
            }
            return s1.Length - s2.Length;
        }
    }

    public class DescendentAlphanumericStringComparer : IComparer<string>
    {
        public int Compare(string s1, string s2) =>
            new AscendentAlphanumericStringComparer().Compare(s2, s1);
    }
}
