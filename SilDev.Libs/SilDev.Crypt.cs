
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class CRYPT
    {
        #region PRIVATE

        private static string BaseEncodeFilters(string input, string prefixMark, string suffixMark, uint lineLength)
        {
            string s = input;
            if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(prefixMark))
            {
                string prefix = prefixMark;
                string suffix = suffixMark;
                if (lineLength > 0)
                {
                    prefix = $"{prefix}{Environment.NewLine}";
                    suffix = $"{Environment.NewLine}{suffix}";
                }
                s = $"{prefix}{s}{suffix}";
            }
            if (lineLength > 1 & s.Length > lineLength)
            {
                int i = 0;
                s = string.Join(Environment.NewLine, s.ToLookup(c => Math.Floor(i++ / (double)lineLength)).Select(e => new string(e.ToArray())));
            }
            return s;
        }

        private static string BaseDecodeFilters(string input, string prefixMark, string suffixMark)
        {
            string s = input;
            if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
            {
                if (s.StartsWith(prefixMark))
                    s = s.Substring(prefixMark.Length);
                if (s.EndsWith(suffixMark))
                    s = s.Substring(0, s.Length - suffixMark.Length);
            }
            if (s.Contains('\r') || s.Contains('\n'))
                s = string.Concat(s.ToCharArray().Where(c => c != '\r' && c != '\n').ToArray());
            return s;
        }

        #endregion

        #region Base64

        public class Base64
        {
            public string PrefixMark = null;
            public string SuffixMark = null;
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    LastEncodedResult = Convert.ToBase64String(bytes);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = null, string suffixMark = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    LastDecodedResult = Convert.FromBase64String(code);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    byte[] ba = text.ToByteArray();
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, uint lineLength) =>
                EncodeString(text, null, null, lineLength);

            public string DecodeString(string code, string prefixMark = null, string suffixMark = null)
            {
                try
                {
                    byte[] ba = DecodeByteArray(code, prefixMark, suffixMark);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, uint lineLength) =>
                EncodeFile(path, null, null, lineLength);

            public byte[] DecodeFile(string code, string prefixMark = null, string suffixMark = null) =>
                DecodeByteArray(code, prefixMark, suffixMark);
        }

        public static string EncodeToBase64(this byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0) =>
            new Base64().EncodeByteArray(bytes, prefixMark, suffixMark, lineLength);

        public static byte[] DecodeByteArrayFromBase64(this string code, string prefixMark = null, string suffixMark = null) =>
            new Base64().DecodeByteArray(code, prefixMark, suffixMark);

        public static string EncodeToBase64(this string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0) =>
            new Base64().EncodeString(text, prefixMark, suffixMark, lineLength);

        public static string DecodeStringFromBase64(this string code, string prefixMark = null, string suffixMark = null) =>
            new Base64().DecodeString(code, prefixMark, suffixMark);

        public static string EncodeFileToBase64(this string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0) =>
            new Base64().EncodeFile(path, prefixMark, suffixMark, lineLength);

        public static byte[] DecodeFileFromBase64(this string code, string prefixMark = null, string suffixMark = null) =>
            new Base64().DecodeFile(code, prefixMark, suffixMark);

        #endregion

        #region Base85

        public class Base85
        {
            private static uint[] p85 = 
            {
                85 * 85 * 85 * 85,
                85 * 85 * 85,
                85 * 85,
                85,
                1
            };

            private static byte[] encodeBlock = new byte[5];
            private static byte[] decodeBlock = new byte[4];

            public string PrefixMark = "<~";
            public string SuffixMark = "~>";
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    if (prefixMark != "<~" && suffixMark != "~>")
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    StringBuilder sb = new StringBuilder();
                    uint t = 0;
                    int n = 0;
                    foreach (byte b in bytes)
                    {
                        if (n + 1 < decodeBlock.Length)
                        {
                            t |= (uint)(b << (24 - (n * 8)));
                            n++;
                            continue;
                        }
                        t |= b;
                        if (t == 0)
                            sb.Append((char)122);
                        else
                        {
                            for (int i = encodeBlock.Length - 1; i >= 0; i--)
                            {
                                encodeBlock[i] = (byte)((t % 85) + 33);
                                t /= 85;
                            }
                            for (int i = 0; i < encodeBlock.Length; i++)
                                sb.Append((char)encodeBlock[i]);
                        }
                        t = 0;
                        n = 0;
                    }
                    if (n > 0)
                    {
                        for (int i = encodeBlock.Length - 1; i >= 0; i--)
                        {
                            encodeBlock[i] = (byte)((t % 85) + 33);
                            t /= 85;
                        }
                        for (int i = 0; i <= n; i++)
                            sb.Append((char)encodeBlock[i]);
                    }
                    LastEncodedResult = sb.ToString();
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = "<~", string suffixMark = "~>")
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        char[] ca = new char[] 
                        {
                            (char)0, (char)8, (char)9,
                            (char)10, (char)12, (char)13
                        };
                        int n = 0;
                        uint t = 0;
                        foreach (char c in code)
                        {
                            if (c == (char)122)
                            {
                                if (n != 0)
                                    throw new ArgumentException();
                                for (int i = 0; i < 4; i++)
                                    decodeBlock[i] = 0;
                                ms.Write(decodeBlock, 0, decodeBlock.Length);
                                continue;
                            }
                            if (ca.Contains(c))
                                continue;
                            if (c < (char)33 || c > (char)117)
                                throw new ArgumentOutOfRangeException();
                            t += (uint)((c - 33) * p85[n]);
                            n++;
                            if (n == encodeBlock.Length)
                            {
                                for (int i = 0; i < decodeBlock.Length; i++)
                                    decodeBlock[i] = (byte)(t >> 24 - (i * 8));
                                ms.Write(decodeBlock, 0, decodeBlock.Length);
                                t = 0;
                                n = 0;
                            }
                        }
                        if (n != 0)
                        {
                            if (n == 1)
                                throw new NotSupportedException();
                            n--;
                            t += p85[n];
                            for (int i = 0; i < n; i++)
                                decodeBlock[i] = (byte)(t >> 24 - (i * 8));
                            for (int i = 0; i < n; i++)
                                ms.WriteByte(decodeBlock[i]);
                        }
                        LastDecodedResult = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    byte[] ba = text.ToByteArray();
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, uint lineLength) =>
                EncodeString(text, null, null, lineLength);

            public string DecodeString(string code, string prefixMark = "<~", string suffixMark = "~>")
            {
                try
                {
                    byte[] ba = DecodeByteArray(code, prefixMark, suffixMark);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, uint lineLength) =>
                EncodeFile(path, null, null, lineLength);

            public byte[] DecodeFile(string code, string prefixMark = "<~", string suffixMark = "~>") =>
                DecodeByteArray(code, prefixMark, suffixMark);
        }

        public static string EncodeToBase85(this byte[] bytes, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0) =>
            new Base85().EncodeByteArray(bytes, prefixMark, suffixMark, lineLength);

        public static byte[] DecodeByteArrayFromBase85(this string code, string prefixMark = "<~", string suffixMark = "~>") =>
            new Base85().DecodeByteArray(code, prefixMark, suffixMark);

        public static string EncodeToBase85(this string text, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0) =>
            new Base85().EncodeString(text, prefixMark, suffixMark, lineLength);

        public static string DecodeStringFromBase85(this string code, string prefixMark = "<~", string suffixMark = "~>") =>
            new Base85().DecodeString(code, prefixMark, suffixMark);

        public static string EncodeFileToBase85(this string path, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0) =>
            new Base85().EncodeFile(path, prefixMark, suffixMark, lineLength);

        public static byte[] DecodeFileFromBase85(this string code, string prefixMark = "<~", string suffixMark = "~>") =>
            new Base85().DecodeFile(code, prefixMark, suffixMark);

        #endregion

        #region Base91

        public class Base91
        {
            private static uint[] a91 = new uint[]
            {
                 65,  66,  67,  68,  69,  70,  71,  72,  73,
                 74,  75,  76,  77,  78,  79,  80,  81,  82,
                 83,  84,  85,  86,  87,  88,  89,  90,  97,
                 98,  99, 100, 101, 102, 103, 104, 105, 106,
                107, 108, 109, 110, 111, 112, 113, 114, 115,
                116, 117, 118, 119, 120, 121, 122,  48,  49,
                 50,  51,  52,  53,  54,  55,  56,  57,  33,
                 35,  36,  37,  38,  40,  41,  42,  43,  44,
                 45,  46,  58,  59,  60,  61,  62,  63,  64,
                 91,  93,  94,  95,  96, 123, 124, 125, 126,
                 34
            };

            public static char[] defaultEncodeTable;

            public char[] DefaultEncodeTable
            {
                get
                {
                    if (defaultEncodeTable == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (uint i in a91)
                            sb.Append((char)i);
                        defaultEncodeTable = sb.ToString().ToCharArray();
                    }
                    return defaultEncodeTable;
                }
            }

            private char[] encodeTable;

            public char[] EncodeTable
            {
                get
                {
                    if (encodeTable == null)
                        encodeTable = DefaultEncodeTable;
                    return encodeTable;
                }
                set
                {
                    try
                    {
                        value = value.Distinct().ToArray();
                        if (value.Length < 91)
                            throw new ArgumentException();
                        if (value.Length > 91)
                            value = new List<char>(value).GetRange(0, 91).ToArray();
                        encodeTable = value;
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                        encodeTable = DefaultEncodeTable;
                    }
                }
            }

            private Dictionary<byte, int> decodeTable;

            private void InitializeTables()
            {
                if (encodeTable == null)
                    encodeTable = EncodeTable;
                decodeTable = new Dictionary<byte, int>();
                for (int i = 0; i < 255; i++)
                    decodeTable[(byte)i] = -1;
                for (int i = 0; i < encodeTable.Length; i++)
                    decodeTable[(byte)encodeTable[i]] = i;
            }

            public string PrefixMark = null;
            public string SuffixMark = null;
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    if (encodeTable != null)
                        EncodeTable = encodeTable;
                    InitializeTables();
                    StringBuilder sb = new StringBuilder();
                    int[] ia = new int[] { 0, 0, 0 };
                    foreach (byte b in bytes)
                    {
                        ia[0] |= b << ia[1];
                        ia[1] += 8;
                        if (ia[1] < 14)
                            continue;
                        ia[2] = ia[0] & 8191;
                        if (ia[2] > 88)
                        {
                            ia[1] -= 13;
                            ia[0] >>= 13;
                        }
                        else
                        {
                            ia[2] = ia[0] & 16383;
                            ia[1] -= 14;
                            ia[0] >>= 14;
                        }
                        sb.Append(this.encodeTable[ia[2] % 91]);
                        sb.Append(this.encodeTable[ia[2] / 91]);
                    }
                    if (ia[1] != 0)
                    {
                        sb.Append(this.encodeTable[ia[0] % 91]);
                        if (ia[1] >= 8 || ia[0] >= 91)
                            sb.Append(this.encodeTable[ia[0] / 91]);
                    }
                    LastEncodedResult = sb.ToString();
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (encodeTable != null)
                        EncodeTable = encodeTable;
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    InitializeTables();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int[] ia = new int[] { 0, -1, 0, 0 };
                        foreach (char c in code)
                        {
                            if (this.encodeTable.Count(e => e == (byte)c) == 0)
                                throw new ArgumentOutOfRangeException();
                            ia[0] = decodeTable[(byte)c];
                            if (ia[0] == -1)
                                continue;
                            if (ia[1] < 0)
                            {
                                ia[1] = ia[0];
                                continue;
                            }
                            ia[1] += ia[0] * 91;
                            ia[2] |= ia[1] << ia[3];
                            ia[3] += (ia[1] & 8191) > 88 ? 13 : 14;
                            do
                            {
                                ms.WriteByte((byte)(ia[2] & 255));
                                ia[2] >>= 8;
                                ia[3] -= 8;
                            }
                            while (ia[3] > 7);
                            ia[1] = -1;
                        }
                        if (ia[1] != -1)
                            ms.WriteByte((byte)((ia[2] | ia[1] << ia[3]) & 255));
                        LastDecodedResult = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    byte[] ba = text.ToByteArray();
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength, encodeTable) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, string prefixMark, string suffixMark, char[] encodeTable) =>
                EncodeString(text, prefixMark, suffixMark, 0, encodeTable);

            public string EncodeString(string text, uint lineLength, char[] encodeTable) =>
                EncodeString(text, null, null, lineLength, encodeTable);

            public string EncodeString(string text, char[] encodeTable) =>
                EncodeString(text, null, null, 0, encodeTable);

            public string DecodeString(string text, string prefixMark = null, string suffixMark = null, char[] encodeTable = null)
            {
                try
                {
                    byte[] ba = DecodeByteArray(text, prefixMark, suffixMark, encodeTable);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string DecodeString(string text, char[] encodeTable) =>
                DecodeString(text, null, null, encodeTable);

            public string EncodeFile(string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength, encodeTable);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark, string suffixMark, char[] encodeTable) =>
                EncodeFile(path, prefixMark, suffixMark, 0, encodeTable);

            public string EncodeFile(string path, uint lineLength, char[] encodeTable) =>
                EncodeFile(path, null, null, lineLength, encodeTable);

            public string EncodeFile(string path, char[] encodeTable) =>
                EncodeFile(path, null, null, 0, encodeTable);

            public byte[] DecodeFile(string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null) =>
                DecodeByteArray(code, prefixMark, suffixMark, encodeTable);

            public byte[] DecodeFile(string code, char[] encodeTable) =>
                DecodeFile(code, null, null, encodeTable);
        }

        public static string EncodeToBase91(this byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null) =>
            new Base91().EncodeByteArray(bytes, prefixMark, suffixMark, lineLength, encodeTable);

        public static byte[] DecodeByteArrayFromBase91(this string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null) =>
            new Base91().DecodeByteArray(code, prefixMark, suffixMark, encodeTable);

        public static string EncodeToBase91(this string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null) =>
            new Base91().EncodeString(text, prefixMark, suffixMark, lineLength, encodeTable);        

        public static string DecodeStringFromBase91(this string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null) =>
            new Base91().DecodeString(code, prefixMark, suffixMark, encodeTable);

        public static string EncodeFileToBase91(this string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null) =>
           new Base91().EncodeFile(path, prefixMark, suffixMark, lineLength, encodeTable);

        public static byte[] DecodeFileFromBase91(this string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null) =>
            new Base91().DecodeFile(code, prefixMark, suffixMark, encodeTable);

        #endregion

        #region Advanced Encryption Standard

        public static class AES
        {
            public enum KeySize : int
            {
                AES128 = 128,
                AES192 = 192,
                AES256 = 256
            }

            public static byte[] EncryptByteArray(byte[] bytes, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256)
            {
                try
                {
                    byte[] ba = null;
                    using (RijndaelManaged rm = new RijndaelManaged())
                    {
                        rm.BlockSize = 128;
                        rm.KeySize = (int)keySize;
                        using (Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(password, salt == null ? password.EncryptToSHA512().ToByteArray() : salt, 1000))
                        {
                            rm.Key = db.GetBytes(rm.KeySize / 8);
                            rm.IV = db.GetBytes(rm.BlockSize / 8);
                        }
                        rm.Mode = CipherMode.CBC;
                        MemoryStream ms = new MemoryStream();
                        using (CryptoStream cs = new CryptoStream(ms, rm.CreateEncryptor(), CryptoStreamMode.Write))
                            cs.Write(bytes, 0, bytes.Length);
                        ba = ms.ToArray();
                    }
                    return ba;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return null;
                }
            }

            public static byte[] EncryptByteArray(byte[] bytes, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                EncryptByteArray(bytes, password.ToByteArray(), salt, keySize);

            public static byte[] EncryptByteArray(byte[] bytes, string password, KeySize keySize = KeySize.AES256) =>
                EncryptByteArray(bytes, password, null, keySize);


            public static byte[] EncryptString(string text, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256) =>
                EncryptByteArray(text.ToByteArray(), password, salt, keySize);

            public static byte[] EncryptString(string text, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                EncryptString(text, password.ToByteArray(), salt, keySize);

            public static byte[] EncryptString(string text, string password, KeySize keySize = KeySize.AES256) =>
                EncryptString(text, password.ToByteArray(), null, keySize);


            public static byte[] EncryptFile(string path, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256) =>
                EncryptByteArray(File.ReadAllBytes(path), password, salt, keySize);

            public static byte[] EncryptFile(string path, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                EncryptFile(path, password.ToByteArray(), salt, keySize);

            public static byte[] EncryptFile(string path, string password, KeySize keySize = KeySize.AES256) =>
                EncryptFile(path, password.ToByteArray(), null, keySize);


            public static byte[] DecryptByteArray(byte[] code, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256)
            {
                try
                {
                    byte[] ba = null;
                    using (RijndaelManaged rm = new RijndaelManaged())
                    {
                        rm.BlockSize = 128;
                        rm.KeySize = (int)keySize;
                        using (Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(password, salt == null ? password.EncryptToSHA512().ToByteArray() : salt, 1000))
                        {
                            rm.Key = db.GetBytes(rm.KeySize / 8);
                            rm.IV = db.GetBytes(rm.BlockSize / 8);
                        }
                        rm.Mode = CipherMode.CBC;
                        MemoryStream ms = new MemoryStream();
                        using (CryptoStream cs = new CryptoStream(ms, rm.CreateDecryptor(), CryptoStreamMode.Write))
                            cs.Write(code, 0, code.Length);
                        ba = ms.ToArray();
                    }
                    return ba;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return null;
                }
            }

            public static byte[] DecryptByteArray(byte[] code, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                DecryptByteArray(code, password.ToByteArray(), salt, keySize);

            public static byte[] DecryptByteArray(byte[] code, string password, KeySize keySize = KeySize.AES256) =>
                DecryptByteArray(code, password, null, keySize);


            public static byte[] DecryptString(string code, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256) =>
                DecryptByteArray(code.FromHexStringToByteArray(), password, salt, keySize);

            public static byte[] DecryptString(string code, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                DecryptString(code, password.ToByteArray(), salt, keySize);

            public static byte[] DecryptString(string code, string password, KeySize keySize = KeySize.AES256) =>
                DecryptString(code, password.ToByteArray(), null, keySize);


            public static byte[] DecryptFile(string path, byte[] password, byte[] salt = null, KeySize keySize = KeySize.AES256) =>
                DecryptByteArray(File.ReadAllBytes(path), password, salt, keySize);

            public static byte[] DecryptFile(string path, string password, byte[] salt, KeySize keySize = KeySize.AES256) =>
                DecryptFile(path, password.ToByteArray(), salt, keySize);

            public static byte[] DecryptFile(string path, string password, KeySize keySize = KeySize.AES256) =>
                DecryptFile(path, password.ToByteArray(), null, keySize);
        }

        public static byte[] EncryptToAES128(this byte[] bytes, string password) =>
            AES.EncryptByteArray(bytes, password, AES.KeySize.AES128);

        public static byte[] EncryptToAES128(this string text, string password) =>
            AES.EncryptString(text, password, AES.KeySize.AES128);

        public static byte[] EncryptFileToAES128(this string path, string password) =>
            AES.EncryptFile(path, password, AES.KeySize.AES128);

        public static byte[] DecryptFromAES128(this byte[] bytes, string password) =>
            AES.DecryptByteArray(bytes, password, AES.KeySize.AES128);

        public static byte[] DecryptStringFromAES128(this string text, string password) =>
            AES.DecryptString(text, password, AES.KeySize.AES128);

        public static byte[] DecryptFileFromAES128(this string path, string password) =>
            AES.DecryptFile(path, password, AES.KeySize.AES128);


        public static byte[] EncryptToAES192(this byte[] bytes, string password) =>
            AES.EncryptByteArray(bytes, password, AES.KeySize.AES192);

        public static byte[] EncryptToAES192(this string text, string password) =>
            AES.EncryptString(text, password, AES.KeySize.AES192);

        public static byte[] EncryptFileToAES192(this string path, string password) =>
            AES.EncryptFile(path, password, AES.KeySize.AES192);

        public static byte[] DecryptFromAES192(this byte[] bytes, string password) =>
            AES.DecryptByteArray(bytes, password, AES.KeySize.AES192);

        public static byte[] DecryptStringFromAES192(this string text, string password) =>
            AES.DecryptString(text, password, AES.KeySize.AES192);

        public static byte[] DecryptFileFromAES192(this string path, string password) =>
            AES.DecryptFile(path, password, AES.KeySize.AES192);


        public static byte[] EncryptToAES256(this byte[] bytes, string password) =>
            AES.EncryptByteArray(bytes, password);

        public static byte[] EncryptToAES256(this string text, string password) =>
            AES.EncryptString(text, password);

        public static byte[] EncryptFileToAES256(this string path, string password) =>
            AES.EncryptFile(path, password);

        public static byte[] DecryptFromAES256(this byte[] bytes, string password) =>
            AES.DecryptByteArray(bytes, password);

        public static byte[] DecryptStringFromAES256(this string text, string password) =>
            AES.DecryptString(text, password);

        public static byte[] DecryptFileFromAES256(this string path, string password) =>
            AES.DecryptFile(path, password);

        #endregion

        #region Message-Digest 5

        public static class MD5
        {
            public static int HashLength { get; } = 32;

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.MD5.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.RemoveChar('-').ToLower();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(text.ToByteArray());

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return ba.ToHexString();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static string EncryptToMD5(this byte[] bytes) =>
            MD5.EncryptByteArray(bytes);

        public static string EncryptToMD5(this string text) =>
            MD5.EncryptString(text);

        public static string EncryptFileToMD5(this string path) =>
            MD5.EncryptFile(path);

        #endregion

        #region Secure Hash Algorithm 1

        public static class SHA1
        {
            public static int HashLength { get; } = 40;

            public static string EncryptStream(Stream stream)
            {
                try
                {
                    byte[] ba;
                    using (SHA1CryptoServiceProvider csp = new SHA1CryptoServiceProvider())
                        ba = csp.ComputeHash(stream);
                    return ba.ToHexString();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    string s;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Read(bytes, 0, bytes.Length);
                        s = EncryptStream(ms);
                    }
                    return s;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text)
            {
                try
                {
                    byte[] ba = text.ToByteArray();
                    using (var csp = System.Security.Cryptography.SHA1.Create())
                        ba = csp.ComputeHash(ba);
                    string s = BitConverter.ToString(ba);
                    return s.RemoveChar('-').ToLower();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptFile(string path)
            {
                try
                {
                    string s;
                    using (FileStream fs = File.OpenRead(path))
                        s = EncryptStream(fs);
                    return s;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static string EncryptToSHA1(this byte[] bytes) =>
            SHA1.EncryptByteArray(bytes);

        public static string EncryptToSHA1(this string text) =>
            SHA1.EncryptString(text);

        public static string EncryptFileToSHA1(this string path) =>
            SHA1.EncryptFile(path);

        #endregion

        #region Secure Hash Algorithm 2

        public static class SHA256
        {
            public static int HashLength { get; } = 64;

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA256.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.RemoveChar('-').ToLower();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(text.ToByteArray());

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return ba.ToHexString();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static string EncryptToSHA256(this byte[] bytes) =>
            SHA256.EncryptByteArray(bytes);

        public static string EncryptToSHA256(this string text) =>
            SHA256.EncryptString(text);

        public static string EncryptFileToSHA256(this string path) =>
            SHA256.EncryptFile(path);

        public static class SHA384
        {
            public static int HashLength { get; } = 96;

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA384.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.RemoveChar('-').ToLower();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(text.ToByteArray());

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (SHA384CryptoServiceProvider csp = new SHA384CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return ba.ToHexString();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static string EncryptToSHA384(this byte[] bytes) =>
            SHA384.EncryptByteArray(bytes);

        public static string EncryptToSHA384(this string text) =>
            SHA384.EncryptString(text);

        public static string EncryptFileToSHA384(this string path) =>
            SHA384.EncryptFile(path);

        public static class SHA512
        {
            public static int HashLength { get; } = 128;

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA512.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.RemoveChar('-').ToLower();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(text.ToByteArray());

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (SHA512CryptoServiceProvider csp = new SHA512CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return ba.ToHexString();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static string EncryptToSHA512(this byte[] bytes) =>
            SHA512.EncryptByteArray(bytes);

        public static string EncryptToSHA512(this string text) =>
            SHA512.EncryptString(text);

        public static string EncryptFileToSHA512(this string path) =>
            SHA512.EncryptFile(path);

        #endregion
    }
}

#endregion
