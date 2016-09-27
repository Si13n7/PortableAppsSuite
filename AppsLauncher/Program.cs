using SilDev;
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
            LOG.FileDir = Path.Combine(AppsLauncher.Main.TmpDir, "logs");
            INI.File("%CurDir%\\Settings.ini");
            LOG.AllowDebug(INI.File(), "Settings");

#if x86
            string AppsLauncher64;
            if (Environment.Is64BitOperatingSystem && File.Exists(AppsLauncher64 = PATH.Combine($"%CurDir%\\{Process.GetCurrentProcess().ProcessName}64.exe")))
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = RUN.CommandLine(false),
                    FileName = AppsLauncher64
                });
                return;
            }
#endif

            if (!RequirementsAvailable())
            {
                string updPath = PATH.Combine("%CurDir%\\Binaries\\Updater.exe");
                if (File.Exists(updPath))
                    RUN.App(new ProcessStartInfo() { FileName = updPath });
                else
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (MessageBox.Show(Lang.GetText("RequirementsErrorMsg"), "Portable Apps Suite", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        Process.Start("https://github.com/Si13n7/PortableAppsSuite/releases");
                }
                return;
            }

            if (LOG.DebugMode < 2)
                AppsLauncher.Main.CheckEnvironmentVariable();

            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    Lang.ResourcesNamespace = typeof(Program).Namespace;
                    if (newInstance && string.IsNullOrWhiteSpace(AppsLauncher.Main.CmdLine) || AppsLauncher.Main.ActionGuid.IsAllowNewInstance || AppsLauncher.Main.ActionGuid.IsExtractCachedImage)
                    {
                        if (LOG.DebugMode > 0)
                            LOG.Stopwatch.Start();
                        SetInterfaceSettings();
                        Application.Run(new MenuViewForm());
                    }
                    else if (AppsLauncher.Main.CmdLineArray.Count > 0)
                    {
                        if ((newInstance || AppsLauncher.Main.ActionGuid.IsAllowNewInstance) && !AppsLauncher.Main.ActionGuid.IsDisallowInterface)
                        {
                            if (LOG.DebugMode > 0)
                                LOG.Stopwatch.Start();
                            SetInterfaceSettings();
                            Application.Run(new OpenWithForm());
                        }
                        else
                        {
                            if (AppsLauncher.Main.ActionGuid.IsRepairDirs)
                                return;

                            if (AppsLauncher.Main.CmdLineArray.Count == 2)
                            {
                                switch (AppsLauncher.Main.CmdLineArray.Skip(0).First())
                                {
                                    case AppsLauncher.Main.ActionGuid.FileTypeAssociation:
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.AssociateFileTypes(AppsLauncher.Main.CmdLineArray.Skip(1).First());
                                        return;
                                    case AppsLauncher.Main.ActionGuid.RestoreFileTypes:
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.RestoreFileTypes(AppsLauncher.Main.CmdLineArray.Skip(1).First());
                                        return;
                                    case AppsLauncher.Main.ActionGuid.SystemIntegration:
                                        SetInterfaceSettings();
                                        AppsLauncher.Main.SystemIntegration(AppsLauncher.Main.CmdLineArray.Skip(1).First().ToBoolean());
                                        return;
                                }
                            }

                            int hWnd = ActivePID();
                            if (hWnd > 0)
                                WINAPI.SendArgs((IntPtr)hWnd, AppsLauncher.Main.CmdLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        static int ActivePID(int time = 2500)
        {
            int hWnd = 0;
            try
            {
                for (int i = 0; i < time; i++)
                {
                    if ((hWnd = INI.ReadInteger("History", "PID", 0)) > 0)
                        break;
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            return hWnd;
        }

        static bool RequirementsAvailable()
        {
            if (!ELEVATION.WritableLocation())
                ELEVATION.RestartAsAdministrator(RUN.CommandLine());
            string[] sArray = new string[]
            {
                "Apps\\.free\\",
                "Apps\\.repack\\",
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
                string path = PATH.Combine($"%CurDir%\\{s}");
                if (s.EndsWith("\\"))
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            AppsLauncher.Main.RepairAppsSuiteDirs();
                        if (!Directory.Exists(path))
                            throw new DirectoryNotFoundException();
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

            Color color = WINAPI.GetSystemThemeColor();
            AppsLauncher.Main.Colors.System = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Base"), AppsLauncher.Main.Colors.System);
            AppsLauncher.Main.Colors.Base = color;

            color = Color.FromArgb(byte.MaxValue, (byte)(color.R * .5f), (byte)(color.G * .5f), (byte)(color.B * .5f));
            AppsLauncher.Main.Colors.BaseDark = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Control"), SystemColors.Window);
            AppsLauncher.Main.Colors.Control = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ControlText"), SystemColors.WindowText);
            AppsLauncher.Main.Colors.ControlText = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Button"), SystemColors.ButtonFace);
            AppsLauncher.Main.Colors.Button = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ButtonHover"), ProfessionalColors.ButtonSelectedHighlight);
            AppsLauncher.Main.Colors.ButtonHover = color;

            color = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ButtonText"), SystemColors.ControlText);
            AppsLauncher.Main.Colors.ButtonText = color;

            AppsLauncher.Main.FontFamily = INI.ReadString("Settings", "Window.FontFamily");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
