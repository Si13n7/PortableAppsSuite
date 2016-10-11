using SilDev;
using System;
using System.Collections.Generic;

namespace IniWriter
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            Environment.ExitCode = 1;
            List<string> args = RUN.CommandLineArgs(false);
            if (args.Count >= 4)
            {
                INI.File(PATH.Combine(args[3]));
                if (INI.Write(args[0], args[1], args[2]))
                    Environment.ExitCode = 0;
            }
            Environment.Exit(Environment.ExitCode);
        }
    }
}
