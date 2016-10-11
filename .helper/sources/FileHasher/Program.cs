using SilDev;
using System;
using System.Collections.Generic;

namespace FileHasher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            Environment.ExitCode = 1;
            List<string> args = RUN.CommandLineArgs(false);
            if (args.Count >= 3)
            {
                string key = null;
                foreach (string arg in args)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = arg.ToUpper();
                        continue;
                    }
                    string path = PATH.Combine(arg);
                    if (string.IsNullOrWhiteSpace(INI.File()))
                    {
                        INI.File(PATH.Combine(path));
                        continue;
                    }
                    string hash = null;
                    int len = 0;
                    switch (key)
                    {
                        case "MD5":
                            hash = path.EncryptFileToMD5();
                            len = CRYPT.MD5.HashLength;
                            break;
                        case "SHA1":
                            hash = path.EncryptFileToSHA1();
                            len = CRYPT.SHA1.HashLength;
                            break;
                        case "SHA256":
                            hash = path.EncryptFileToSHA256();
                            len = CRYPT.SHA256.HashLength;
                            break;
                        case "SHA384":
                            hash = path.EncryptFileToSHA384();
                            len = CRYPT.SHA384.HashLength;
                            break;
                        case "SHA512":
                            hash = path.EncryptFileToSHA512();
                            len = CRYPT.SHA512.HashLength;
                            break;
                    }
                    if (hash.Length == len)
                        if (INI.Write(key, System.IO.Path.GetFileNameWithoutExtension(path), hash))
                            Environment.ExitCode = 0;
                }
            }
            Environment.Exit(Environment.ExitCode);
        }
    }
}
