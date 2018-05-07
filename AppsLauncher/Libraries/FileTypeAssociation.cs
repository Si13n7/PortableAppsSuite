namespace AppsLauncher.Libraries
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using LangResources;
    using Microsoft.Win32;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    internal static class FileTypeAssociation
    {
        internal static void Associate(string appName, Form owner = null)
        {
            var types = Ini.Read(appName, "FileTypes");
            if (string.IsNullOrWhiteSpace(types))
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.associateBtnMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var cfgPath = PathEx.Combine(Settings.CorePaths.TempDir, Settings.ActionGuid.FileTypeAssociation);
            if (!Elevation.IsAdministrator)
            {
                if (owner != null)
                {
                    owner.Enabled = false;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.Indeterminate);
                }
                var bw = new BackgroundWorker();
                bw.DoWork += (sender, args) =>
                {
                    if (!File.Exists(cfgPath))
                        File.Create(cfgPath).Close();
                    Ini.WriteDirect("AppInfo", "AppName", appName, cfgPath);
                    Ini.WriteDirect("AppInfo", "ExePath", ApplicationHandler.GetPath(appName), cfgPath);
                    using (var p = ProcessEx.Start(PathEx.LocalPath, $"{Settings.ActionGuid.FileTypeAssociation} \"{appName}\"", true, false))
                        if (!p?.HasExited == true)
                            p.WaitForExit();
                    FileEx.TryDelete(cfgPath);
                };
                bw.RunWorkerCompleted += (sender, args) =>
                {
                    if (owner == null)
                        return;
                    owner.Enabled = true;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.NoProgress);
                    if (WinApi.NativeHelper.GetForegroundWindow() != owner.Handle)
                        WinApi.NativeHelper.SetForegroundWindow(owner.Handle);
                };
                bw.RunWorkerAsync();
                return;
            }

            string iconData = null;
            using (Form dialog = new ResourcesEx.IconBrowserDialog(Settings.IconResourcePath, Settings.Window.Colors.BaseDark, Settings.Window.Colors.ControlText, Settings.Window.Colors.Button, Settings.Window.Colors.ButtonText, Settings.Window.Colors.ButtonHover))
            {
                dialog.TopMost = true;
                dialog.Plus();
                dialog.ShowDialog();
                if (dialog.Text.Count(c => c == ',') == 1)
                    iconData = dialog.Text;
            }

            if (string.IsNullOrWhiteSpace(iconData))
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dataSplit = iconData.Split(',');
            var dataPath = EnvironmentEx.GetVariablePathFull(dataSplit[0], false, false);
            var dataId = dataSplit[1];
            if (File.Exists(PathEx.Combine(dataPath)) && !string.IsNullOrWhiteSpace(dataId))
                iconData = $"{dataPath},{dataSplit[1]}";

            string appPath;
            MessageBoxEx.ButtonText.OverrideEnabled = true;
            MessageBoxEx.ButtonText.Yes = "App";
            MessageBoxEx.ButtonText.No = "Launcher";
            MessageBoxEx.ButtonText.Cancel = Language.GetText(nameof(en_US.Cancel));
            var result = MessageBoxEx.Show(Language.GetText(nameof(en_US.associateAppWayQuestion)), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    appPath = ApplicationHandler.GetPath(appName);
                    if (string.IsNullOrWhiteSpace(appPath) && File.Exists(cfgPath) && appName.EqualsEx(Ini.ReadDirect("AppInfo", "AppName", cfgPath)))
                        appPath = Ini.ReadDirect("AppInfo", "ExePath", cfgPath);
                    break;
                default:
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            if (!File.Exists(appPath))
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (EnvironmentEx.SystemRestore.IsEnabled)
            {
                result = MessageBoxEx.Show(Language.GetText(nameof(en_US.RestorePointMsg0)), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    EnvironmentEx.SystemRestore.Create($"{appName} - File Type Assotiation", EnvironmentEx.SystemRestore.EventType.BeginSystemChange, EnvironmentEx.SystemRestore.PointType.ModifySettings);
            }

            var restPointDir = PathEx.Combine(PathEx.LocalDir, "Restoration");
            try
            {
                if (!Directory.Exists(restPointDir))
                {
                    Directory.CreateDirectory(restPointDir);
                    DirectoryEx.SetAttributes(restPointDir, FileAttributes.ReadOnly | FileAttributes.Hidden);
                    var iniPath = Path.Combine(restPointDir, "desktop.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    Ini.WriteDirect(".ShellClassInfo", "IconResource", "..\\Assets\\FolderIcons.dll,1", iniPath);
                    FileEx.SetAttributes(iniPath, FileAttributes.System | FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            restPointDir = Path.Combine(restPointDir, Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appName, "FileAssociation", DateTime.Now.ToString("yy-MM-dd"));
            var backupCount = 0;
            if (Directory.Exists(restPointDir))
                backupCount = Directory.GetFiles(restPointDir, "*.ini", SearchOption.TopDirectoryOnly).Length;
            else
                try
                {
                    Directory.CreateDirectory(restPointDir);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            var restPointCfgPath = Path.Combine(restPointDir, $"{backupCount}.ini");
            if (!File.Exists(restPointCfgPath))
                File.Create(restPointCfgPath).Close();
            restPointDir = Path.Combine(restPointDir, backupCount.ToString());
            foreach (var type in (types.Contains(",") ? types : $"{types},").Split(','))
            {
                if (string.IsNullOrWhiteSpace(type) || type.StartsWith("."))
                    continue;
                if (!Reg.SubKeyExists($"HKCR\\.{type}"))
                    Ini.WriteDirect(type.EncryptToMd5(), "KeyAdded", $"HKCR\\.{type}", restPointCfgPath);
                else
                {
                    var restKeyName = $"KeyBackup_.{type}_#####.reg";
                    var count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.TopDirectoryOnly).Length;
                    else
                        try
                        {
                            Directory.CreateDirectory(restPointDir);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    var restKeyPath = Path.Combine(restPointDir, restKeyName);
                    Reg.ExportKeys(restKeyPath, $"HKCR\\.{type}");
                    if (File.Exists(restKeyPath))
                        Ini.WriteDirect(type.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                var typeKey = $"PortableAppsSuite_{appName}";
                if (!Reg.SubKeyExists($"HKCR\\{typeKey}"))
                    Ini.WriteDirect(typeKey.EncryptToMd5(), "KeyAdded", $"HKCR\\{typeKey}", restPointCfgPath);
                else
                {
                    var restKeyName = $"KeyBackup_{typeKey}_#####.reg";
                    var count = 0;
                    if (Directory.Exists(restPointDir))
                        count = Directory.GetFiles(restPointDir, restKeyName.Replace("#####", "*"), SearchOption.AllDirectories).Length;
                    restKeyName = restKeyName.Replace("#####", count.ToString());
                    var restKeyPath = Path.Combine(restPointDir, restKeyName);
                    Reg.ExportKeys(restKeyPath.Replace("#####", count.ToString()), $"HKCR\\{typeKey}");
                    if (File.Exists(restKeyPath))
                        Ini.WriteDirect(typeKey.EncryptToMd5(), "KeyBackup", $"{backupCount}\\{restKeyName}", restPointCfgPath);
                }
                Reg.Write(Registry.ClassesRoot, $".{type}", null, typeKey, RegistryValueKind.ExpandString);
                var iconRegEnt = Reg.ReadString(Registry.ClassesRoot, $"{typeKey}\\DefaultIcon", null);
                if (!iconRegEnt.EqualsEx(iconData))
                    Reg.Write(Registry.ClassesRoot, $"{typeKey}\\DefaultIcon", null, iconData, RegistryValueKind.ExpandString);
                var openCmdRegEnt = Reg.ReadString(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", null);
                var openCmd = $"\"{EnvironmentEx.GetVariablePathFull(appPath, false, false)}\" \"%1\"";
                if (!openCmdRegEnt.EqualsEx(openCmd))
                    Reg.Write(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", null, openCmd, RegistryValueKind.ExpandString);
                Reg.RemoveEntry(Registry.ClassesRoot, $"{typeKey}\\shell\\open\\command", "DelegateExecute");
            }

            MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Restore(string appName)
        {
            if (string.IsNullOrEmpty(appName))
                return;

            if (!Elevation.IsAdministrator)
                using (var p = ProcessEx.Start(PathEx.LocalPath, $"{Settings.ActionGuid.RestoreFileTypes} \"{appName}\"", true, false))
                    if (!p?.HasExited == true)
                        p.WaitForExit();

            if (EnvironmentEx.SystemRestore.IsEnabled)
            {
                var result = MessageBoxEx.Show(Language.GetText(nameof(en_US.RestorePointMsg1)), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ProcessEx.Start(Settings.CorePaths.SystemRestore);
                    return;
                }
            }

            var restPointDir = PathEx.Combine(PathEx.LocalDir, "Restoration", Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appName, "FileAssociation");
            string restPointPath;
            using (var dialog = new OpenFileDialog { Filter = @"INI Files(*.ini) | *.ini", InitialDirectory = restPointDir, Multiselect = false, RestoreDirectory = false })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                restPointPath = dialog.FileName;
            }

            if (!File.Exists(restPointPath))
                return;
            foreach (var section in Ini.GetSections(restPointPath))
                try
                {
                    var val = Ini.ReadDirect(section, "KeyBackup", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        val = Ini.ReadDirect(section, "KeyAdded", restPointPath);
                    if (string.IsNullOrWhiteSpace(val))
                        throw new InvalidOperationException($"No value found for '{section}'.");
                    if (val.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = Path.GetDirectoryName(restPointPath);
                        if (!string.IsNullOrEmpty(path))
                            path = Path.Combine(path, "val");
                        if (File.Exists(path))
                            Reg.ImportFile(path);
                    }
                    else
                        Reg.RemoveSubKey(val);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            try
            {
                File.Delete(restPointPath);
                var iniDir = Path.Combine(Path.GetDirectoryName(restPointPath));
                var iniSubDir = Path.Combine(iniDir, Path.GetFileNameWithoutExtension(restPointPath));
                DirectoryEx.Delete(iniSubDir);
                if (Directory.EnumerateFiles(iniDir, "*.ini", SearchOption.TopDirectoryOnly).Any())
                {
                    var path = Path.GetDirectoryName(restPointPath);
                    if (!string.IsNullOrEmpty(path))
                        DirectoryEx.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
