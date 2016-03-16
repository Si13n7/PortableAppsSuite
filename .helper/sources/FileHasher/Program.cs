using System;
using System.Collections.Generic;

namespace FileHasher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            Environment.ExitCode = 1;
            List<string> args = SilDev.Run.CommandLineArgs(false);
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
                    string path = SilDev.Run.EnvironmentVariableFilter(arg);
                    if (string.IsNullOrWhiteSpace(SilDev.Ini.File()))
                    {
                        SilDev.Ini.File(SilDev.Run.EnvironmentVariableFilter(path));
                        continue;
                    }
                    string hash = null;
                    int len = 0;
                    switch (key)
                    {
                        case "MD5":
                            hash = SilDev.Crypt.MD5.EncryptFile(path);
                            len = 32;
                            break;
                        case "SHA1":
                            hash = SilDev.Crypt.SHA.EncryptFile(path, SilDev.Crypt.SHA.CryptKind.SHA1);
                            len = 40;
                            break;
                        case "SHA256":
                            hash = SilDev.Crypt.SHA.EncryptFile(path, SilDev.Crypt.SHA.CryptKind.SHA256);
                            len = 64;
                            break;
                        case "SHA384":
                            hash = SilDev.Crypt.SHA.EncryptFile(path, SilDev.Crypt.SHA.CryptKind.SHA384);
                            len = 96;
                            break;
                        case "SHA512":
                            hash = SilDev.Crypt.SHA.EncryptFile(path, SilDev.Crypt.SHA.CryptKind.SHA512);
                            len = 128;
                            break;
                    }
                    if (hash.Length == len)
                        if (SilDev.Ini.Write(key, System.IO.Path.GetFileNameWithoutExtension(path), hash))
                            Environment.ExitCode = 0;
                }
            }
            Environment.Exit(Environment.ExitCode);
        }
    }
}
