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
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class OpenWithForm : Form
    {
        private string _searchText = string.Empty;
        protected bool IsStarted, ValidData;

        public OpenWithForm()
        {
            InitializeComponent();

            notifyIcon.Icon = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Asterisk, true, Main.SystemResourcePath);

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

            appMenuItem2.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem3.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem7.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.RecycleBinEmpty, Main.SystemResourcePath)?.ToBitmap();

            if (!searchBox.Focused)
                searchBox.Select();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WinApi.WindowMenuFunc.WM_COPYDATA:
                    var st = (WinApi.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(WinApi.COPYDATASTRUCT));
                    var strData = Marshal.PtrToStringUni(st.lpData);
                    if (!string.IsNullOrWhiteSpace(strData) && !Main.CmdLine.ContainsEx(strData))
                    {
                        if (WinApi.UnsafeNativeMethods.GetForegroundWindow() != Handle)
                            WinApi.UnsafeNativeMethods.SetForegroundWindow(Handle);
                        Main.CmdLineArray.Add(strData.RemoveChar('\"'));
                        ShowBalloonTip(Text, Lang.GetText("cmdLineUpdated"));
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void OpenWithForm_Load(object sender, EventArgs e)
        {
            BackColor = Main.Colors.BaseDark;
            Icon = Resources.PortableApps_blue;
            Lang.SetControlLang(this);
            Text = Lang.GetText($"{Name}Title");
            Main.SetFont(this);
            Main.SetFont(appMenu);
            Main.CheckCmdLineApp();
            appsBox_Update(false);
        }

        private void OpenWithForm_Shown(object sender, EventArgs e)
        {
            Ini.Write("History", "PID", Handle);
            if (!string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                runCmdLine.Enabled = true;
                return;
            }
            Opacity = 1f;
        }

        private void OpenWithForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var startMenuIntegration = Ini.ReadInteger("Settings", "StartMenuIntegration");
            if (startMenuIntegration <= 0)
                return;
            var list = appsBox.Items.Cast<string>().ToList();
            Main.StartMenuFolderUpdate(list);
        }

        private void OpenWithForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Ini.RemoveKey("History", "PID");

        private void OpenWithForm_DragEnter(object sender, DragEventArgs e)
        {
            Array items;
            ValidData = DragFileName(out items, e);
            if (ValidData)
            {
                var dataAdded = false;
                foreach (var item in items)
                {
                    var s = item as string;
                    if (s == null)
                        continue;
                    Main.CmdLineArray.Add(s.RemoveChar('\"'));
                    dataAdded = true;
                }
                if (dataAdded)
                {
                    ShowBalloonTip(Text, Lang.GetText("cmdLineUpdated"));
                    Main.CheckCmdLineApp();
                    foreach (var appInfo in Main.AppsInfo)
                        if (appInfo.ShortName == Main.CmdLineApp)
                        {
                            appsBox.SelectedItem = appInfo.LongName;
                            Main.CmdLineApp = string.Empty;
                            break;
                        }
                }
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        protected bool DragFileName(out Array files, DragEventArgs e)
        {
            var ret = false;
            files = null;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                var data = e.Data.GetData("FileDrop") as Array;
                if (data?.Length >= 1 && data.GetValue(0) is string)
                {
                    files = data;
                    ret = true;
                }
            }
            return ret;
        }

        private void OpenWithForm_Activated(object sender, EventArgs e)
        {
            if (!IsStarted)
                IsStarted = true;
            else
                appsBox_Update(true);
        }

        private void OpenWithForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            try
            {
                using (Form dialog = new AboutForm())
                {
                    dialog.TopMost = TopMost;
                    dialog.AddLoadingTimeStopwatch();
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            e.Cancel = true;
        }

        private void runCmdLine_Tick(object sender, EventArgs e)
        {
            try
            {
                var pArray = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (pArray.Length > 1 && pArray.Count(p => p.Handle != Process.GetCurrentProcess().Handle && p.MainWindowTitle == Lang.GetText($"{Name}Title")) > 1)
                    return;
                foreach (var appInfo in Main.AppsInfo)
                    if (appInfo.ShortName == Main.CmdLineApp)
                    {
                        appsBox.SelectedItem = appInfo.LongName;
                        break;
                    }
                if (appsBox.SelectedIndex > 0)
                {
                    var appName = appsBox.SelectedItem.ToString();
                    var appInfo = Main.GetAppInfo(appName);
                    if (appInfo.LongName == appName)
                    {
                        var noConfirm = Ini.ReadBoolean(appInfo.ShortName, "NoConfirm");
                        if (!Main.CmdLineMultipleApps && noConfirm)
                        {
                            runCmdLine.Enabled = false;
                            Main.StartApp(appsBox.SelectedItem.ToString(), true);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            runCmdLine.Enabled = false;
            Opacity = 1f;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (notifyIconDisabler.IsBusy)
                notifyIconDisabler.CancelAsync();
            if (notifyIcon.Visible)
                notifyIcon.Visible = false;
        }

        private void notifyIconDisabler_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;
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

        private void notifyIconDisabler_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) =>
            notifyIcon.Visible = false;

        private void ShowBalloonTip(string title, string tip)
        {
            if (!notifyIcon.Visible)
                notifyIcon.Visible = true;
            if (!notifyIconDisabler.IsBusy)
                notifyIconDisabler.RunWorkerAsync();
            notifyIcon.ShowBalloonTip(1800, title, tip, ToolTipIcon.Info);
        }

        private void appsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void appsBox_Update(bool forceAppCheck)
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
            if (appsBox.SelectedIndex < 0)
            {
                var lastItem = Ini.Read("History", "LastItem");
                if (!string.IsNullOrWhiteSpace(lastItem))
                    if (appsBox.Items.Contains(lastItem))
                        appsBox.SelectedItem = lastItem;
            }

            if (!string.IsNullOrWhiteSpace(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            var startMenuIntegration = Ini.ReadInteger("Settings", "StartMenuIntegration");
            if (startMenuIntegration > 0)
                Main.StartMenuFolderUpdate(appsBox.Items.Cast<object>().Select(item => item.ToString()).ToList());
        }

        private void appMenuItem_Opening(object sender, CancelEventArgs e)
        {
            var cms = (ContextMenuStrip)sender;
            for (var i = 0; i < cms.Items.Count; i++)
            {
                var text = Lang.GetText(cms.Items[i].Name);
                cms.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : cms.Items[i].Text;
            }
        }

        private void appMenu_Paint(object sender, PaintEventArgs e) =>
            ((ContextMenuStrip)sender).SetFixedSingle(e, Main.Colors.Base);

        private void appMenuItem_Click(object sender, EventArgs e)
        {
            switch (((ToolStripMenuItem)sender).Name)
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
                    if (Data.CreateShortcut(Main.GetEnvironmentVariablePath(Main.GetAppPath(appsBox.SelectedItem.ToString())), Path.Combine("%Desktop%", appsBox.SelectedItem.ToString()), Main.CmdLine))
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem7":
                    if (MsgBoxEx.Show(this, string.Format(Lang.GetText("appMenuItem7Msg"), appsBox.SelectedItem), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        try
                        {
                            var appDir = Path.GetDirectoryName(Main.GetAppPath(appsBox.SelectedItem.ToString()));
                            if (Directory.Exists(appDir))
                            {
                                Directory.Delete(appDir, true);
                                MsgBoxEx.Show(this, Lang.GetText("OperationCompletedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            MsgBoxEx.Show(this, Lang.GetText("OperationFailedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Log.Write(ex);
                        }
                    else
                        MsgBoxEx.Show(this, Lang.GetText("OperationCanceledMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        private void searchBox_Enter(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            tb.Font = new Font("Segoe UI", tb.Font.Size);
            tb.ForeColor = Main.Colors.ControlText;
            tb.Text = _searchText;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            var c = Main.Colors.ControlText;
            tb.Font = new Font("Comic Sans MS", tb.Font.Size, FontStyle.Italic);
            tb.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = tb.Text;
            tb.Text = Lang.GetText(tb);
        }

        private void searchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
                return;
            }
            ((TextBox)sender).Refresh();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
                return;
            var itemList = appsBox.Items.Cast<object>().Select(item => item.ToString()).ToList();
            foreach (var item in appsBox.Items)
                if (item.ToString() == Main.SearchMatchItem(tb.Text, itemList))
                {
                    appsBox.SelectedItem = item;
                    break;
                }
        }

        private void addBtn_Click(object sender, EventArgs e) =>
#if x86
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader.exe");
#else
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader64.exe");
#endif

        private void addBtn_MouseEnter(object sender, EventArgs e)
        {
            var b = (Button)sender;
            b.Image = b.Image.SwitchGrayScale($"{b.Name}BackgroundImage");
            toolTip.SetToolTip(b, Lang.GetText($"{b.Name}Tip"));
        }

        private void addBtn_MouseLeave(object sender, EventArgs e)
        {
            var b = (Button)sender;
            b.Image = b.Image.SwitchGrayScale($"{b.Name}BackgroundImage");
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!((Button)sender).Split_ClickEvent(appMenu))
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void startBtn_MouseMove(object sender, MouseEventArgs e) =>
            ((Button)sender).Split_MouseMoveEvent();

        private void startBtn_MouseLeave(object sender, EventArgs e) =>
            ((Button)sender).Split_MouseLeaveEvent();

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (Form dialog = new SettingsForm(appsBox.SelectedItem.ToString()))
                {
                    dialog.TopMost = TopMost;
                    dialog.ShowDialog();
                    Lang.SetControlLang(this);
                    Text = Lang.GetText($"{Name}Title");
                    Main.SetAppDirs();
                    appsBox_Update(true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
