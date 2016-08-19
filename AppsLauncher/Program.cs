using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.FileLocation = Path.Combine(Application.StartupPath, "Binaries\\Protocols");
            SilDev.Log.AllowDebug();
            SilDev.Ini.File(Application.StartupPath, "Settings.ini");
            if (SilDev.Log.DebugMode == 0)
            {
                int iniDebugOption = SilDev.Ini.ReadInteger("Settings", "Debug", 0);
                if (iniDebugOption > 0)
                    SilDev.Log.ActivateDebug(iniDebugOption);
            }
#if x86
            string AppsLauncher64 = Path.Combine(Application.StartupPath, $"{Process.GetCurrentProcess().ProcessName}64.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsLauncher64))
            {
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = SilDev.Run.CommandLine(false),
                    FileName = AppsLauncher64
                });
                return;
            }
#endif
            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    bool AllowMultipleInstances = SilDev.Ini.ReadBoolean("Settings", "AllowMultipleInstances", false);
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine) && (AllowMultipleInstances || !AllowMultipleInstances && newInstance) || AppsLauncher.Main.CmdLineArray.Contains("{0CA7046C-4776-4DB0-913B-D8F81964F8EE}") || AppsLauncher.Main.CmdLineArray.Contains("{17762FDA-39B3-4224-9525-B1A4DF75FA02}"))
                    {
                        SetInterfaceSettings();
                        Application.Run(new MenuViewForm());
                    }
                    else
                    {
                        if (newInstance)
                        {
                            SetInterfaceSettings();
                            Application.Run(new OpenWithForm());
                        }
                        else
                        {
                            if (AppsLauncher.Main.CmdLineArray.Count == 0)
                                return;
                            if (AppsLauncher.Main.CmdLineArray.Count == 2)
                            {
                                switch (AppsLauncher.Main.CmdLineArray.Skip(0).First())
                                {
                                    case "{DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC}":
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.AssociateFileTypes(AppsLauncher.Main.CmdLineArray.Skip(1).First());
                                        return;
                                    case "{A00C02E5-283A-44ED-9E4D-B82E8F87318F}":
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.UndoFileTypeAssociation(AppsLauncher.Main.CmdLineArray.Skip(1).First());
                                        return;
                                }
                            }
                            try
                            {
                                int hWnd = 0;
                                for (int i = 0; i < 2500; i++)
                                {
                                    if ((hWnd = SilDev.Ini.ReadInteger("History", "PID", 0)) > 0)
                                        break;
                                    Thread.Sleep(1);
                                }
                                if (hWnd > 0)
                                    SilDev.WinAPI.SendArgs((IntPtr)hWnd, AppsLauncher.Main.CmdLine);
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private static void SetInterfaceSettings()
        {
            AppsLauncher.Main.SetAppDirs();
            AppsLauncher.Main.Colors.System = SilDev.WinAPI.GetSystemThemeColor();
            AppsLauncher.Main.Colors.Layout = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowMainColor"), AppsLauncher.Main.Colors.System);
            AppsLauncher.Main.Colors.Control = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlColor"), SystemColors.Control);
            AppsLauncher.Main.Colors.ControlText = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlTextColor"), SystemColors.ControlText);
            AppsLauncher.Main.Colors.Button = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonColor"), SystemColors.ControlDark);
            AppsLauncher.Main.Colors.ButtonHover = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonHoverColor"), AppsLauncher.Main.Colors.System);
            AppsLauncher.Main.Colors.ButtonText = AppsLauncher.Main.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonTextColor"), SystemColors.ControlText);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
