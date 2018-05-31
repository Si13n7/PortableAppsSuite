namespace AppsLauncher.Windows
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using LangResources;
    using Libraries;
    using Properties;
    using SilDev;
    using SilDev.Drawing;
    using SilDev.Forms;

    public partial class OpenWithForm : Form
    {
        private const string Title = "Apps Launcher";
        private static readonly object Locker = new object();
        private string _searchText;
        protected bool IsStarted, ValidData;

        public OpenWithForm()
        {
            InitializeComponent();

            notifyIcon.Icon = Settings.CacheData.GetSystemIcon(ResourcesEx.IconIndex.Asterisk, true);

            searchBox.BackColor = Settings.Window.Colors.Control;
            searchBox.ForeColor = Settings.Window.Colors.ControlText;
            searchBox.DrawSearchSymbol(Settings.Window.Colors.ControlText);

            startBtn.Split(Settings.Window.Colors.ButtonText);
            foreach (var btn in new[] { startBtn, settingsBtn })
            {
                btn.BackColor = Settings.Window.Colors.Button;
                btn.ForeColor = Settings.Window.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Settings.Window.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Settings.Window.Colors.ButtonHover;
            }

            appMenu.EnableAnimation(ContextMenuStripEx.Animations.SlideVerPositive, 100);
            appMenu.SetFixedSingle();
            appMenuItem2.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Uac);
            appMenuItem3.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Directory);
            appMenuItem7.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.RecycleBinEmpty);

            if (!searchBox.Focused)
                searchBox.Select();
        }

        private void OpenWithForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            BackColor = Settings.Window.Colors.BaseDark;

            Icon = Resources.PortableApps_blue;

            Language.SetControlLang(this);
            Text = Language.GetText(Name);

            var notifyBox = new NotifyBox { Opacity = .8d };
            notifyBox.Show(Language.GetText(nameof(en_US.FileSystemAccessMsg)), Title, NotifyBox.NotifyBoxStartPosition.Center);
            Settings.Arguments.FindApp();
            notifyBox.Close();
            if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                WinApi.NativeHelper.SetForegroundWindow(Handle);

            AppsBox_Update(false);
        }

        private void OpenWithForm_Shown(object sender, EventArgs e)
        {
            Reg.Write(Settings.RegistryPath, "Handle", Handle);
            if (!string.IsNullOrWhiteSpace(Settings.Arguments.FoundApp))
            {
                runCmdLine.Enabled = true;
                return;
            }
            Opacity = 1f;
        }

        private void OpenWithForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Reg.RemoveSubKey(Settings.RegistryPath);
            if (!Settings.StartMenuIntegration)
                return;
            var appNames = appsBox.Items.Cast<string>();
            SystemIntegration.UpdateStartMenuShortcuts(appNames);
        }

        private void OpenWithForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Settings.WriteToFileInQueue)
                Settings.WriteToFile();
        }

        private void OpenWithForm_DragEnter(object sender, DragEventArgs e)
        {
            if (!DragFileName(out var items, e))
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            var added = false;
            foreach (var item in items)
            {
                if (!(item is string str) || string.IsNullOrEmpty(str))
                    continue;
                str = str.RemoveChar('\"');
                if (Settings.Arguments.ValidPaths.Contains(str))
                    continue;
                added = true;
                Settings.Arguments.ValidPaths.Add(str);
            }
            if (added)
            {
                Settings.Arguments.FindApp();
                ShowBalloonTip(Text, Language.GetText(nameof(en_US.cmdLineUpdated)));
                foreach (var appInfo in ApplicationHandler.AllAppInfos)
                    if (appInfo.ShortName.EqualsEx(Settings.Arguments.FoundApp))
                    {
                        appsBox.SelectedItem = appInfo.LongName;
                        Settings.Arguments.FoundApp = string.Empty;
                        break;
                    }
            }
            e.Effect = DragDropEffects.Copy;
        }

        protected bool DragFileName(out Array files, DragEventArgs e)
        {
            files = null;
            if ((e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy)
                return false;
            var data = e.Data.GetData("FileDrop") as Array;
            if (!(data?.Length > 0) || !(data.GetValue(0) is string))
                return false;
            files = data;
            return true;
        }

        private void OpenWithForm_Activated(object sender, EventArgs e)
        {
            if (!IsStarted)
                IsStarted = true;
            else
                AppsBox_Update(true);
        }

        private void OpenWithForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            TopMost = false;
            try
            {
                using (Form dialog = new AboutForm())
                {
                    dialog.TopMost = true;
                    dialog.Plus();
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            TopMost = true;
            e.Cancel = true;
        }

        private void RunCmdLine_Tick(object sender, EventArgs e)
        {
            lock (Locker)
            {
                try
                {
                    var curTitle = Language.GetText($"{Name}Title");
                    var curInstance = Process.GetCurrentProcess();
                    var allInstances = ProcessEx.GetInstances(PathEx.LocalPath).ToList();
                    try
                    {
                        if (allInstances.Any(p => p.Handle != curInstance.Handle && p.MainWindowTitle.EqualsEx(curTitle)))
                            return;
                        foreach (var appInfo in ApplicationHandler.AllAppInfos)
                            if (appInfo.ShortName.EqualsEx(Settings.Arguments.FoundApp))
                            {
                                appsBox.SelectedItem = appInfo.LongName;
                                break;
                            }
                        if (appsBox.SelectedIndex > 0)
                        {
                            var appName = appsBox.SelectedItem.ToString();
                            var appInfo = ApplicationHandler.GetAppInfo(appName);
                            if (appInfo.LongName.EqualsEx(appName))
                            {
                                var noConfirm = Ini.Read(appInfo.ShortName, "NoConfirm", false);
                                if (!Settings.Arguments.MultipleAppsFound && noConfirm)
                                {
                                    runCmdLine.Enabled = false;
                                    ApplicationHandler.Start(appsBox.SelectedItem.ToString(), true);
                                    return;
                                }
                            }
                        }
                        runCmdLine.Enabled = false;
                        Opacity = 1f;
                    }
                    catch (InvalidOperationException)
                    {
                        // ignored
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    finally
                    {
                        curInstance.Dispose();
                        foreach (var p in allInstances)
                            p?.Dispose();
                    }
                }
                catch (Win32Exception)
                {
                    // ignored
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (notifyIconDisabler.IsBusy)
                notifyIconDisabler.CancelAsync();
            if (notifyIcon.Visible)
                notifyIcon.Visible = false;
        }

        private void NotifyIconDisabler_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!(sender is BackgroundWorker bw))
                return;
            for (var i = 0; i < 3000; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(1);
            }
        }

        private void NotifyIconDisabler_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) =>
            notifyIcon.Visible = false;

        private void ShowBalloonTip(string title, string tip)
        {
            if (!notifyIcon.Visible)
                notifyIcon.Visible = true;
            if (!notifyIconDisabler.IsBusy)
                notifyIconDisabler.RunWorkerAsync();
            notifyIcon.ShowBalloonTip(1800, title, tip, ToolTipIcon.Info);
        }

        private void AppsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0xd)
                ApplicationHandler.Start(appsBox.SelectedItem.ToString(), true);
        }

        private void AppsBox_Update(bool forceAppCheck)
        {
            if (!ApplicationHandler.AllAppInfos.Any() || forceAppCheck)
                ApplicationHandler.SetAppsInfo();

            var selectedItem = string.Empty;
            if (appsBox.SelectedIndex >= 0)
                selectedItem = appsBox.SelectedItem.ToString();

            appsBox.Items.Clear();
            var strAppNames = ApplicationHandler.AllAppInfos.Select(x => x.LongName).ToArray();
            var objAppNames = new object[strAppNames.Length];
            Array.Copy(strAppNames, objAppNames, objAppNames.Length);
            appsBox.Items.AddRange(objAppNames);

            if (appsBox.SelectedIndex < 0 && !string.IsNullOrWhiteSpace(Settings.Arguments.FoundApp))
            {
                var appName = ApplicationHandler.GetAppInfo(Settings.Arguments.FoundApp).ShortName;
                if (string.IsNullOrWhiteSpace(appName) || !appsBox.Items.Contains(appName))
                    appName = ApplicationHandler.GetAppInfo(Settings.Arguments.FoundApp).LongName;
                if (!string.IsNullOrWhiteSpace(appName) && appsBox.Items.Contains(appName))
                    appsBox.SelectedItem = appName;
            }

            if (appsBox.SelectedIndex < 0 && !string.IsNullOrWhiteSpace(Settings.LastItem) && appsBox.Items.Contains(Settings.LastItem))
                appsBox.SelectedItem = Settings.LastItem;

            if (!string.IsNullOrWhiteSpace(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            if (!Settings.StartMenuIntegration)
                return;
            var appNames = appsBox.Items.Cast<object>().Select(item => item.ToString());
            SystemIntegration.UpdateStartMenuShortcuts(appNames);
        }

        private void AppMenuItem_Opening(object sender, CancelEventArgs e)
        {
            var owner = sender as ContextMenuStrip;
            for (var i = 0; i < owner?.Items.Count; i++)
            {
                var text = Language.GetText(owner.Items[i].Name);
                owner.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : owner.Items[i].Text;
            }
        }

        private void AppMenuItem_Click(object sender, EventArgs e)
        {
            var owner = sender as ToolStripMenuItem;
            switch (owner?.Name)
            {
                case "appMenuItem1":
                    ApplicationHandler.Start(appsBox.SelectedItem.ToString(), true);
                    break;
                case "appMenuItem2":
                    ApplicationHandler.Start(appsBox.SelectedItem.ToString(), true, true);
                    break;
                case "appMenuItem3":
                    ApplicationHandler.OpenLocation(appsBox.SelectedItem.ToString());
                    break;
                case "appMenuItem4":
                    var targetPath = ApplicationHandler.GetPath(appsBox.SelectedItem.ToString());
                    var linkPath = Path.Combine("%Desktop%", appsBox.SelectedItem.ToString());
                    if (FileEx.CreateShortcut(targetPath, linkPath, Settings.Arguments.ValidPathsStr))
                        MessageBoxEx.Show(this, Language.GetText($"{owner.Name}Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Language.GetText($"{owner.Name}Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem7":
                    if (MessageBoxEx.Show(this, string.Format(Language.GetText($"{owner.Name}Msg"), appsBox.SelectedItem), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appInfo = ApplicationHandler.GetAppInfo(appsBox.SelectedItem.ToString());
                            var appDir = ApplicationHandler.GetLocation(appInfo.ShortName);
                            if (!Settings.AppDirs.Any(x => appDir.ContainsEx(x)))
                                throw new ArgumentOutOfRangeException(nameof(appDir));
                            if (Directory.Exists(appDir))
                            {
                                try
                                {
                                    FileEx.Delete(Settings.CachePaths.CurrentImages);
                                    Directory.Delete(appDir, true);
                                }
                                catch
                                {
                                    if (!PathEx.ForceDelete(appDir))
                                        PathEx.ForceDelete(appDir, true);
                                }
                                Ini.RemoveSection(appInfo.ShortName);
                                Ini.WriteAll();
                                appsBox.Items.RemoveAt(appsBox.SelectedIndex);
                                if (appsBox.SelectedIndex < 0)
                                    appsBox.SelectedIndex = 0;
                                MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationCompletedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationFailedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationCanceledMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
            }
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            if (!(sender is TextBox owner))
                return;
            owner.Font = new Font("Segoe UI", owner.Font.Size);
            owner.ForeColor = Settings.Window.Colors.ControlText;
            owner.Text = _searchText ?? string.Empty;
        }

        private void SearchBox_Leave(object sender, EventArgs e)
        {
            if (!(sender is TextBox owner))
                return;
            var c = Settings.Window.Colors.ControlText;
            owner.Font = new Font("Comic Sans MS", owner.Font.Size, FontStyle.Italic);
            owner.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = owner.Text;
            owner.Text = Language.GetText(owner);
        }

        private void SearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0xd)
            {
                ApplicationHandler.Start(appsBox.SelectedItem?.ToString(), true);
                return;
            }
            (sender as TextBox)?.Refresh();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var owner = sender as TextBox;
            if (string.IsNullOrWhiteSpace(owner?.Text))
                return;
            var itemList = appsBox.Items.Cast<object>().Select(item => item.ToString()).ToList();
            foreach (var item in appsBox.Items)
                if (item.ToString().EqualsEx(ApplicationHandler.SearchItem(owner.Text, itemList)))
                {
                    appsBox.SelectedItem = item;
                    break;
                }
        }

        private void AddBtn_Click(object sender, EventArgs e) =>
            ProcessEx.Start(Settings.CorePaths.AppsDownloader);

        private void AddBtn_MouseEnter(object sender, EventArgs e)
        {
            if (!(sender is Button owner))
                return;
            owner.Image = owner.Image.SwitchGrayScale($"{owner.Name}BackgroundImage");
            toolTip.SetToolTip(owner, Language.GetText($"{owner.Name}Tip"));
        }

        private void AddBtn_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button owner)
                owner.Image = owner.Image.SwitchGrayScale($"{owner.Name}BackgroundImage");
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (sender is Button owner && !owner.SplitClickHandler(appMenu))
                ApplicationHandler.Start(appsBox.SelectedItem.ToString(), true);
        }

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            TopMost = false;
            try
            {
                using (Form dialog = new SettingsForm(appsBox.SelectedItem.ToString()))
                {
                    dialog.TopMost = true;
                    dialog.Plus();
                    dialog.ShowDialog();
                    Language.SetControlLang(this);
                    Text = Language.GetText(Name);
                    AppsBox_Update(true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            TopMost = true;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WinApi.WindowMenuFlags.WmCopyData:
                    var dStruct = (WinApi.CopyData)Marshal.PtrToStructure(m.LParam, typeof(WinApi.CopyData));
                    var strData = Marshal.PtrToStringUni(dStruct.lpData);
                    if (!string.IsNullOrWhiteSpace(strData) && !Settings.Arguments.ValidPaths.ContainsEx(strData))
                    {
                        if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                            WinApi.NativeHelper.SetForegroundWindow(Handle);
                        Settings.Arguments.ValidPaths.Add(strData.Trim('"'));
                        Settings.Arguments.FindApp();
                        ShowBalloonTip(Text, Language.GetText(nameof(en_US.cmdLineUpdated)));
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
