using SilDev;
using System;
using System.IO;
using System.Linq;

namespace AssemblyDateVersion
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            try
            {
                string version = DateTime.Now.ToString("yy.M.d.*");
                foreach (string f in Environment.GetCommandLineArgs().Skip(1))
                {
                    if (Path.GetFileName(f) != "AssemblyInfo.cs")
                        continue;
                    string[] lines = File.ReadAllLines(f);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (line.Contains("AssemblyVersion"))
                        {
                            line = $"[assembly: AssemblyVersion(\"{version}\")]";
                            lines[i] = line;
                        }
                    }
                    File.WriteAllLines(f, lines);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }
}
