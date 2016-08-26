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
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine) && (AllowMultipleInstances || !AllowMultipleInstances && newInstance) || 
                        Environment.CommandLine.Contains(AppsLauncher.Main.CmdLineActionGuid.AllowNewInstance) ||
                        Environment.CommandLine.Contains(AppsLauncher.Main.CmdLineActionGuid.ExtractCachedImage))
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

                                    case AppsLauncher.Main.CmdLineActionGuid.FileTypeAssociation:
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.AssociateFileTypes(AppsLauncher.Main.CmdLineArray.Skip(1).First());
                                        return;
                                    case AppsLauncher.Main.CmdLineActionGuid.UndoFileTypeAssociation:
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
            AppsLauncher.Main.Colors.Layout = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowMainColor"), AppsLauncher.Main.Colors.System);
            AppsLauncher.Main.Colors.Control = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlColor"), SystemColors.Control);
            AppsLauncher.Main.Colors.ControlText = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlTextColor"), SystemColors.ControlText);
            AppsLauncher.Main.Colors.Button = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonColor"), SystemColors.ControlDark);
            AppsLauncher.Main.Colors.ButtonHover = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonHoverColor"), AppsLauncher.Main.Colors.System);
            AppsLauncher.Main.Colors.ButtonText = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonTextColor"), SystemColors.ControlText);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
