using System;
using System.Collections.Generic;

namespace IniWriter
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            Environment.ExitCode = 1;
            List<string> args = SilDev.Run.CommandLineArgs(false);
            if (args.Count >= 4)
            {
                SilDev.Ini.File(SilDev.Run.EnvironmentVariableFilter(args[3]));
                if (SilDev.Ini.Write(args[0], args[1], args[2]))
                    Environment.ExitCode = 0;
            }
            Environment.Exit(Environment.ExitCode);
        }
    }
}
