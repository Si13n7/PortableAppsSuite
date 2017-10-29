namespace AppsLauncher.UI
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
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class OpenWithForm : Form
    {
        private static readonly object Locker = new object();
        private string _searchText;
        protected bool IsStarted, ValidData;

        public OpenWithForm()
        {
            InitializeComponent();

            notifyIcon.Icon = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Asterisk, true, Main.SystemResourcePath);

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            searchBox.DrawSearchSymbol(Main.Colors.ControlText);

            startBtn.Split(Main.Colors.ButtonText);
            foreach (var btn in new[] { startBtn, settingsBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appMenu.EnableAnimation(ContextMenuStripEx.Animations.SlideVerPositive, 100);
            appMenu.SetFixedSingle();
            appMenuItem2.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem3.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem7.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.RecycleBinEmpty, Main.SystemResourcePath)?.ToBitmap();

            if (!searchBox.Focused)
                searchBox.Select();
        }

        private void OpenWithForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            BackColor = Main.Colors.BaseDark;

            Icon = Resources.PortableApps_blue;

            Lang.SetControlLang(this);
            Text = Lang.GetText(Name);

            Main.SetFont(this);
            Main.SetFont(appMenu);

            var notifyBox = new NotifyBox { Opacity = .8d };
            notifyBox.Show(Lang.GetText(nameof(en_US.FileSystemAccessMsg)), Main.Title, NotifyBox.NotifyBoxStartPosition.Center);
            Main.CheckCmdLineApp();
            notifyBox.Close();
            if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                WinApi.NativeHelper.SetForegroundWindow(Handle);

            AppsBox_Update(false);
        }

        private void OpenWithForm_Shown(object sender, EventArgs e)
        {
            Reg.Write(Main.RegPath, "Handle", Handle);
            if (!string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                runCmdLine.Enabled = true;
                return;
            }
            Opacity = 1f;
        }

        private void OpenWithForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Reg.RemoveSubKey(Main.RegPath);
            if (Ini.Read("Settings", "StartMenuIntegration", 0) <= 0)
                return;
            var list = appsBox.Items.Cast<string>().ToList();
            Main.StartMenuFolderUpdateHandler(list);
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
                var s = item as string;
                if (s == null)
                    continue;
                s = s.RemoveChar('\"');
                if (Main.ReceivedPathsArray.Contains(s))
                    continue;
                Main.ReceivedPathsArray.Add(s);
                added = true;
            }
            if (added)
            {
                Main.CheckCmdLineApp();
                ShowBalloonTip(Text, Lang.GetText(nameof(en_US.cmdLineUpdated)));
                foreach (var appInfo in Main.AppsInfo)
                    if (appInfo.ShortName.EqualsEx(Main.CmdLineApp))
                    {
                        appsBox.SelectedItem = appInfo.LongName;
                        Main.CmdLineApp = string.Empty;
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
            if (!(data?.Length >= 1) || !(data.GetValue(0) is string))
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
                    var curTitle = Lang.GetText($"{Name}Title");
                    var curInstance = Process.GetCurrentProcess();
                    var allInstances = ProcessEx.GetInstances(PathEx.LocalPath).ToList();
                    try
                    {
                        if (allInstances.Any(p => p.Handle != curInstance.Handle && p.MainWindowTitle.EqualsEx(curTitle)))
                            return;
                        foreach (var appInfo in Main.AppsInfo)
                            if (appInfo.ShortName.EqualsEx(Main.CmdLineApp))
                            {
                                appsBox.SelectedItem = appInfo.LongName;
                                break;
                            }
                        if (appsBox.SelectedIndex > 0)
                        {
                            var appName = appsBox.SelectedItem.ToString();
                            var appInfo = Main.GetAppInfo(appName);
                            if (appInfo.LongName.EqualsEx(appName))
                            {
                                var noConfirm = Ini.Read(appInfo.ShortName, "NoConfirm", false);
                                if (!Main.CmdLineMultipleApps && noConfirm)
                                {
                                    runCmdLine.Enabled = false;
                                    Main.StartApp(appsBox.SelectedItem.ToString(), true);
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
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void AppsBox_Update(bool forceAppCheck)
        {
            if (Main.AppsInfo.Count == 0 || forceAppCheck)
                Main.CheckAvailableApps();

            var selectedItem = string.Empty;
            if (appsBox.SelectedIndex >= 0)
                selectedItem = appsBox.SelectedItem.ToString();

            appsBox.Items.Clear();
            var strAppNames = Main.AppsInfo.Select(x => x.LongName).ToArray();
            var objAppNames = new object[strAppNames.Length];
            Array.Copy(strAppNames, objAppNames, objAppNames.Length);
            appsBox.Items.AddRange(objAppNames);

            if (appsBox.SelectedIndex < 0 && !string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                var appName = Main.GetAppInfo(Main.CmdLineApp).ShortName;
                if (string.IsNullOrWhiteSpace(appName) || !appsBox.Items.Contains(appName))
                    appName = Main.GetAppInfo(Main.CmdLineApp).LongName;
                if (!string.IsNullOrWhiteSpace(appName) && appsBox.Items.Contains(appName))
                    appsBox.SelectedItem = appName;
            }

            if (appsBox.SelectedIndex < 0)
            {
                var lastItem = Ini.Read("History", "LastItem");
                if (!string.IsNullOrWhiteSpace(lastItem) && appsBox.Items.Contains(lastItem))
                    appsBox.SelectedItem = lastItem;
            }

            if (!string.IsNullOrWhiteSpace(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            var startMenuIntegration = Ini.Read("Settings", "StartMenuIntegration", 0);
            if (startMenuIntegration > 0)
                Main.StartMenuFolderUpdateHandler(appsBox.Items.Cast<object>().Select(item => item.ToString()).ToList());
        }

        private void AppMenuItem_Opening(object sender, CancelEventArgs e)
        {
            var owner = sender as ContextMenuStrip;
            for (var i = 0; i < owner?.Items.Count; i++)
            {
                var text = Lang.GetText(owner.Items[i].Name);
                owner.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : owner.Items[i].Text;
            }
        }

        private void AppMenuItem_Click(object sender, EventArgs e)
        {
            var owner = sender as ToolStripMenuItem;
            switch (owner?.Name)
            {
                case "appMenuItem1":
                    Main.StartApp(appsBox.SelectedItem.ToString(), true);
                    break;
                case "appMenuItem2":
                    Main.StartApp(appsBox.SelectedItem.ToString(), true, true);
                    break;
                case "appMenuItem3":
                    Main.OpenAppLocation(appsBox.SelectedItem.ToString());
                    break;
                case "appMenuItem4":
                    var targetPath = EnvironmentEx.GetVariablePathFull(Main.GetAppPath(appsBox.SelectedItem.ToString()), false);
                    var linkPath = Path.Combine("%Desktop%", appsBox.SelectedItem.ToString());
                    if (Data.CreateShortcut(targetPath, linkPath, Main.ReceivedPathsStr))
                        MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem7":
                    if (MessageBoxEx.Show(this, string.Format(Lang.GetText($"{owner.Name}Msg"), appsBox.SelectedItem), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
                            var appDir = Main.GetAppLocation(appInfo.ShortName);
                            if (Directory.Exists(appDir))
                            {
                                try
                                {
                                    var imgCachePath = PathEx.Combine(Main.TmpDir, "images.dat");
                                    if (File.Exists(imgCachePath))
                                        File.Delete(imgCachePath);
                                    Directory.Delete(appDir, true);
                                }
                                catch
                                {
                                    if (!Data.ForceDelete(appDir))
                                        Data.ForceDelete(appDir, true);
                                }
                                Ini.RemoveSection(appInfo.ShortName);
                                Ini.WriteAll();
                                appsBox.Items.RemoveAt(appsBox.SelectedIndex);
                                if (appsBox.SelectedIndex < 0)
                                    appsBox.SelectedIndex = 0;
                                MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCompletedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationFailedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCanceledMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
            }
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            if (!(sender is TextBox owner))
                return;
            owner.Font = new Font("Segoe UI", owner.Font.Size);
            owner.ForeColor = Main.Colors.ControlText;
            owner.Text = _searchText ?? string.Empty;
        }

        private void SearchBox_Leave(object sender, EventArgs e)
        {
            if (!(sender is TextBox owner))
                return;
            var c = Main.Colors.ControlText;
            owner.Font = new Font("Comic Sans MS", owner.Font.Size, FontStyle.Italic);
            owner.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = owner.Text;
            owner.Text = Lang.GetText(owner);
        }

        private void SearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0xd)
            {
                Main.StartApp(appsBox.SelectedItem?.ToString(), true);
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
                if (item.ToString().EqualsEx(Main.ItemSearchHandler(owner.Text, itemList)))
                {
                    appsBox.SelectedItem = item;
                    break;
                }
        }

        private void AddBtn_Click(object sender, EventArgs e) =>
#if x86
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader.exe");
#else
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader64.exe");
#endif

        private void AddBtn_MouseEnter(object sender, EventArgs e)
        {
            if (!(sender is Button owner))
                return;
            owner.Image = owner.Image.SwitchGrayScale($"{owner.Name}BackgroundImage");
            toolTip.SetToolTip(owner, Lang.GetText($"{owner.Name}Tip"));
        }

        private void AddBtn_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button owner)
                owner.Image = owner.Image.SwitchGrayScale($"{owner.Name}BackgroundImage");
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (sender is Button owner && !owner.SplitClickHandler(appMenu))
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
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
                    Lang.SetControlLang(this);
                    Text = Lang.GetText(Name);
                    Main.SetAppDirs();
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
                    if (!string.IsNullOrWhiteSpace(strData) && !Main.ReceivedPathsArray.ContainsEx(strData))
                    {
                        if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                            WinApi.NativeHelper.SetForegroundWindow(Handle);
                        Main.ReceivedPathsArray.Add(strData.Trim('"'));
                        Main.CheckCmdLineApp();
                        ShowBalloonTip(Text, Lang.GetText(nameof(en_US.cmdLineUpdated)));
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
