namespace AppsLauncher.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using LangResources;
    using Microsoft.Win32;
    using SilDev;
    using SilDev.Forms;

    internal static class FileTypeAssoc
    {
        internal static void Associate(AppData appData, Form owner = default(Form))
        {
            if (appData?.Settings?.FileTypes?.Any() != true)
            {
                MessageBoxEx.Show(owner, Language.GetText(nameof(en_US.associateBtnMsg)), Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!Elevation.IsAdministrator)
            {
                if (owner != null)
                {
                    owner.TopMost = !owner.TopMost;
                    owner.Enabled = !owner.Enabled;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.Indeterminate);
                }
                var bw = new BackgroundWorker();
                bw.DoWork += (sender, args) =>
                {
                    using (var process = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.FileTypeAssociation} \"{appData.Key}\"", true, false))
                        if (!process?.HasExited == true)
                            process.WaitForExit();
                };
                bw.RunWorkerCompleted += (sender, args) =>
                {
                    if (owner == default(Form))
                        return;
                    TaskBar.Progress.SetState(owner.Handle, TaskBar.Progress.Flags.NoProgress);
                    if (WinApi.NativeHelper.GetForegroundWindow() != owner.Handle)
                        WinApi.NativeHelper.SetForegroundWindow(owner.Handle);
                    owner.Enabled = !owner.Enabled;
                    owner.TopMost = !owner.TopMost;
                };
                bw.RunWorkerAsync();
                return;
            }

            Restore(appData, true);

            var assocData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "InstallId",
                    Settings.SystemInstallId
                }
            };

            using (Form dialog = new ResourcesEx.IconBrowserDialog(Settings.IconResourcePath, Settings.Window.Colors.BaseDark, Settings.Window.Colors.ControlText, Settings.Window.Colors.Button, Settings.Window.Colors.ButtonText, Settings.Window.Colors.ButtonHover))
            {
                dialog.TopMost = true;
                dialog.Plus();
                dialog.ShowDialog();
                if (dialog.Text.Count(c => c == ',') == 1)
                {
                    var iconData = dialog.Text.Split(',');
                    assocData.Add("IconPath", EnvironmentEx.GetVariablePathFull(iconData.First(), false, false));
                    assocData.Add("IconId", iconData.Last());
                }
            }

            if (!assocData.ContainsKey("IconPath") || !assocData.ContainsKey("IconId") || !FileEx.Exists(assocData["IconPath"]))
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBoxEx.ButtonText.OverrideEnabled = true;
            MessageBoxEx.ButtonText.Yes = "App";
            MessageBoxEx.ButtonText.No = "Launcher";
            MessageBoxEx.ButtonText.Cancel = Language.GetText(nameof(en_US.Cancel));
            var result = MessageBoxEx.Show(Language.GetText(nameof(en_US.associateAppWayQuestion)), Settings.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    assocData.Add("StarterPath", EnvironmentEx.GetVariablePathFull(appData.FilePath, false, false));
                    break;
                case DialogResult.No:
                    assocData.Add("StarterPath", EnvironmentEx.GetVariablePathFull(PathEx.LocalPath, false, false));
                    break;
                default:
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            if (assocData.ContainsKey("StarterPath") && FileEx.Exists(assocData["StarterPath"]))
                appData.Settings.FileTypeAssoc = assocData;
            else
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (EnvironmentEx.SystemRestore.IsEnabled)
            {
                result = MessageBoxEx.Show(Language.GetText(nameof(en_US.RestorePointMsg0)), Settings.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    EnvironmentEx.SystemRestore.Create($"{appData.Name} - File Type Assotiation", EnvironmentEx.RestoreEventType.BeginSystemChange, EnvironmentEx.RestorePointType.ModifySettings);
            }

            var restPointDir = Path.Combine(CorePaths.RestorePointDir, appData.Key);
            int restPointCount;
            if (Directory.Exists(restPointDir))
                restPointCount = Directory.GetFiles(restPointDir, "*.dat", SearchOption.TopDirectoryOnly).Length;
            else
            {
                if (!DirectoryEx.Create(restPointDir))
                {
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCanceledMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                restPointCount = 0;
            }

            var appKeyName = $"PortableAppsSuite_{appData.Key}";
            var appKeyPath = $"HKEY_CLASSES_ROOT\\{appKeyName}";

            var restPoint = new Dictionary<string, List<string>>();
            if (!Reg.SubKeyExists(appKeyPath))
                restPoint.Add(appData.Key, new List<string>
                {
                    $"[-{appKeyPath}]"
                });

            var curIconData = Reg.ReadString($"{appKeyPath}\\DefaultIcon", null);
            var newIconPath = assocData["IconPath"].Any(char.IsSeparator) ? $"\"{assocData["IconPath"]}\"" : assocData["IconPath"];
            var newIconData = $"{newIconPath},{assocData["IconId"]}";
            if (!curIconData.EqualsEx(newIconData))
                Reg.Write($"{appKeyPath}\\DefaultIcon", null, newIconData, RegistryValueKind.ExpandString);

            var curCmdData = Reg.ReadString($"{appKeyPath}\\shell\\open\\command", null);
            var newCmdPath = assocData["StarterPath"].Any(char.IsSeparator) ? $"\"{assocData["StarterPath"]}\"" : assocData["StarterPath"];
            var newCmdData = $"{newCmdPath}, \"%1\"";
            if (!curCmdData.EqualsEx(newCmdData))
                Reg.Write($"{appKeyPath}\\shell\\open\\command", null, newCmdData, RegistryValueKind.ExpandString);
            Reg.RemoveEntry($"{appKeyPath}\\shell\\open\\command", "DelegateExecute");

            foreach (var type in appData.Settings.FileTypes.Where(x => !x.StartsWith(".")))
            {
                if (string.IsNullOrWhiteSpace(type))
                    continue;

                var typeKeyPath = $"HKEY_CLASSES_ROOT\\.{type}";
                restPoint.Add(type, new List<string>());
                restPoint[type].Add($"[-{typeKeyPath}]");

                if (Reg.SubKeyExists(typeKeyPath))
                {
                    var keyPath = Path.Combine(Path.GetTempPath(), PathEx.GetTempFileName());
                    Reg.ExportKeys(keyPath, typeKeyPath);
                    if (File.Exists(keyPath))
                    {
                        var lines = FileEx.ReadAllLines(keyPath);
                        if (lines?.Length > 0)
                            lines = FileEx.ReadAllLines(keyPath)?.Skip(1).Where(Comparison.IsNotEmpty).ToArray();
                        if (lines?.Any() == true)
                            restPoint[type].AddRange(lines);
                        File.Delete(keyPath);
                    }
                }

                Reg.Write(typeKeyPath, null, appKeyName, RegistryValueKind.ExpandString);
            }

            var restPointPath = Path.Combine(restPointDir, $"{restPointCount:X4}.dat");
            FileEx.Serialize(restPointPath, restPoint, true);

            MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCompletedMsg)), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Associate(string appName, Form owner = default(Form)) =>
            Associate(CacheData.FindAppData(appName), owner);

        internal static void Restore(AppData appData, bool quite = false)
        {
            if (appData == default(AppData))
                return;

            if (!Elevation.IsAdministrator)
            {
                using (var process = ProcessEx.Start(PathEx.LocalPath, $"{ActionGuid.RestoreFileTypes} \"{appData.Key}\"", true, false))
                    if (!process?.HasExited == true)
                        process.WaitForExit();
                return;
            }

            var restPointDir = Path.Combine(CorePaths.RestorePointDir, appData.Key);
            if (Directory.Exists(restPointDir))
            {
                var files = DirectoryEx.EnumerateFiles(restPointDir, "*.dat")?.Reverse().ToArray();
                if (files?.Any() == true)
                    foreach (var file in files)
                    {
                        var restPoint = FileEx.Deserialize<Dictionary<string, List<string>>>(file, true);
                        if (restPoint?.Values.Any() == true)
                            Reg.ImportFile(restPoint.Values.SelectMany(x => x.ToArray()).ToArray());
                        FileEx.TryDelete(file);
                    }
                DirectoryEx.TryDelete(restPointDir);
            }

            if (!quite && EnvironmentEx.SystemRestore.IsEnabled)
            {
                var result = MessageBoxEx.Show(Language.GetText(nameof(en_US.RestorePointMsg1)), Settings.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ProcessEx.Start(CorePaths.SystemRestore);
                    return;
                }
            }

            if (!quite)
                MessageBoxEx.Show(Language.GetText(nameof(en_US.OperationCompletedMsg)), Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Restore(string appName, bool quite = false) =>
            Restore(CacheData.FindAppData(appName), quite);
    }
}
