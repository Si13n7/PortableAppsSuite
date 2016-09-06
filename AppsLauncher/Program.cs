using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.FileLocation = SilDev.Run.EnvVarFilter("%CurrentDir%\\Binaries\\Protocols");
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

            if (!RequirementsExists())
            {
                string updPath = SilDev.Run.EnvVarFilter("%CurrentDir%\\Binaries\\Updater.exe");
                if (File.Exists(updPath))
                    SilDev.Run.App(new ProcessStartInfo() { FileName = updPath });
                else
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (MessageBox.Show(Lang.GetText("RequirementsErrorMsg"), "Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        Process.Start("https://github.com/Si13n7/PortableAppsSuite/releases");
                }
                return;
            }

            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine) && newInstance || Environment.CommandLine.Contains(AppsLauncher.Main.CmdLineActionGuid.AllowNewInstance) || Environment.CommandLine.Contains(AppsLauncher.Main.CmdLineActionGuid.ExtractCachedImage))
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

        static bool RequirementsExists()
        {
            string[] sArray = new string[]
            {
                "Apps\\.repack\\",
                "Apps\\.free\\",
                "Apps\\.share\\",
                "Assets\\icon.db",
#if x86
                "Binaries\\AppsDownloader.exe",
#else
                "Binaries\\AppsDownloader64.exe",
#endif
                "Binaries\\Updater.exe",
            };
            foreach (string s in sArray)
            {
                string path = SilDev.Run.EnvVarFilter($"%CurrentDir%\\{s}");
                if (s.EndsWith("\\"))
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    if (!File.Exists(path))
                        return false;
                }
            }
            return true;
        }

        static void SetInterfaceSettings()
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
