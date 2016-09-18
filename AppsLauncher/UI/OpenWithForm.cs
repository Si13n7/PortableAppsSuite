using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class OpenWithForm : Form
    {
        #region WNDPROC OVERRIDE

        // Updates command line arguments
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)SilDev.WinAPI.WindowMenuFunc.WM_COPYDATA:
                    SilDev.WinAPI.COPYDATASTRUCT st = (SilDev.WinAPI.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(SilDev.WinAPI.COPYDATASTRUCT));
                    string strData = Marshal.PtrToStringUni(st.lpData);
                    if (!string.IsNullOrWhiteSpace(strData) && !Main.CmdLine.ToLower().Contains(strData.ToLower()))
                    {
                        if (SilDev.WinAPI.SafeNativeMethods.GetForegroundWindow() != Handle)
                            SilDev.WinAPI.SafeNativeMethods.SetForegroundWindow(Handle);
                        Main.CmdLineArray.Add(strData.Replace("\"", string.Empty));
                        showBalloonTip(Text, Lang.GetText("cmdLineUpdated"));
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region INITIALIZE FORM

        protected bool IsStarted, ValidData;
        string SearchText = string.Empty;

        public OpenWithForm()
        {
            InitializeComponent();

            Icon = Properties.Resources.PortableApps_blue;

            BackColor = Main.Colors.BaseDark;

            notifyIcon.Icon = SilDev.Resource.SystemIcon(SilDev.Resource.SystemIconKey.ASTERISK, true, Main.SystemResourcePath);

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            SilDev.Forms.TextBox.DrawSearchSymbol(searchBox, Main.Colors.ControlText);

            SilDev.Forms.Button.DrawSplit(startBtn, Main.Colors.ButtonText);
            foreach (Button btn in new Button[] { startBtn, settingsBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appMenuItem2.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC, Main.SystemResourcePath);
            appMenuItem3.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.DIRECTORY, Main.SystemResourcePath);
            appMenuItem7.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.RECYCLE_BIN_EMPTY, Main.SystemResourcePath);

            if (!searchBox.Focused)
                searchBox.Select();
        }

        #endregion

        #region MAIN EVENTS

        private void OpenWithForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            Text = Lang.GetText($"{Name}Title");
            if (!Directory.Exists(Main.AppsDir))
                Main.RepairAppsSuiteDirs();
            Main.CheckCmdLineApp();
            appsBox_Update(false);
        }

        private void OpenWithForm_Shown(object sender, EventArgs e)
        {
            if (SilDev.Log.DebugMode > 0)
            {
                SilDev.Log.Stopwatch.Stop();
                SilDev.Ini.Write("History", "StartTime", SilDev.Log.Stopwatch.Elapsed.TotalSeconds);
            }
            SilDev.Ini.Write("History", "PID", Handle);
            if (!string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                runCmdLine.Enabled = true;
                return;
            }
            Opacity = 1f;
        }

        private void OpenWithForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            int StartMenuIntegration = SilDev.Ini.ReadInteger("Settings", "StartMenuIntegration");
            if (StartMenuIntegration > 0)
            {
                List<string> list = new List<string>();
                foreach (string item in appsBox.Items)
                    list.Add(item);
                Main.StartMenuFolderUpdate(list);
            }
        }

        private void OpenWithForm_FormClosed(object sender, FormClosedEventArgs e) => 
            SilDev.Ini.RemoveKey("History", "PID");

        private void OpenWithForm_DragEnter(object sender, DragEventArgs e)
        {
            Array items;
            ValidData = DragFileName(out items, e);
            if (ValidData)
            {
                bool DataAdded = false;
                foreach (object item in items)
                {
                    if (item is string)
                    {
                        Main.CmdLineArray.Add(((string)item).Replace("\"", string.Empty));
                        DataAdded = true;
                    }
                }
                if (DataAdded)
                {
                    showBalloonTip(Text, Lang.GetText("cmdLineUpdated"));
                    Main.CheckCmdLineApp();
                    foreach (Main.AppInfo appInfo in Main.AppsInfo)
                    {
                        if (appInfo.ShortName == Main.CmdLineApp)
                        {
                            appsBox.SelectedItem = appInfo.LongName;
                            Main.CmdLineApp = string.Empty;
                            break;
                        }
                    }
                }
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        protected bool DragFileName(out Array files, DragEventArgs e)
        {
            bool ret = false;
            files = null;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = e.Data.GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length >= 1) && (data.GetValue(0) is string))
                    {
                        files = data;
                        ret = true;
                    }
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
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            e.Cancel = true;
        }

        bool test = false;
        private void runCmdLine_Tick(object sender, EventArgs e)
        {
            try
            {
                Process[] pArray = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (pArray.Length > 1 && pArray.Count(p => p.Handle != Process.GetCurrentProcess().Handle && p.MainWindowTitle == Lang.GetText($"{Name}Title")) > 1)
                    return;
                foreach (Main.AppInfo appInfo in Main.AppsInfo)
                {
                    if (appInfo.ShortName == Main.CmdLineApp)
                    {
                        appsBox.SelectedItem = appInfo.LongName;
                        break;
                    }
                }
                if (appsBox.SelectedIndex > 0)
                {
                    string appName = appsBox.SelectedItem.ToString();
                    Main.AppInfo appInfo = Main.GetAppInfo(appName);
                    if (appInfo.LongName == appName)
                    {
                        bool noConfirm = SilDev.Ini.ReadBoolean(appInfo.ShortName, "NoConfirm");
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
                SilDev.Log.Debug(ex);
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
            BackgroundWorker bw = (BackgroundWorker)sender;
            for (int i = 0; i < 3000; i++)
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

        private void showBalloonTip(string _title, string _tip)
        {
            if (!notifyIcon.Visible)
                notifyIcon.Visible = true;
            if (!notifyIconDisabler.IsBusy)
                notifyIconDisabler.RunWorkerAsync();
            notifyIcon.ShowBalloonTip(1800, _title, _tip, ToolTipIcon.Info);
        }

        #endregion

        #region APPS BOX

        private void appsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void appsBox_Update(bool _forceAppCheck)
        {
            if (Main.AppsInfo.Count == 0 || _forceAppCheck)
                Main.CheckAvailableApps();
            string selectedItem = string.Empty;
            if (appsBox.SelectedIndex >= 0)
                selectedItem = appsBox.SelectedItem.ToString();
            appsBox.Items.Clear();
            appsBox.Items.AddRange(Main.AppsInfo.Select(x => x.LongName).ToArray());
            if (appsBox.SelectedIndex < 0)
            {
                string lastItem = SilDev.Ini.Read("History", "LastItem");
                if (!string.IsNullOrWhiteSpace(lastItem))
                    if (appsBox.Items.Contains(lastItem))
                        appsBox.SelectedItem = lastItem;
            }
            if (!string.IsNullOrWhiteSpace(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;
            int StartMenuIntegration = SilDev.Ini.ReadInteger("Settings", "StartMenuIntegration");
            if (StartMenuIntegration > 0)
                Main.StartMenuFolderUpdate(appsBox.Items.Cast<object>().Select(item => item.ToString()).ToList());
        }

        #endregion

        #region APP MENU

        private void appMenuItem_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)sender;
            for (int i = 0; i < cms.Items.Count; i++)
            {
                string text = Lang.GetText(cms.Items[i].Name);
                cms.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : cms.Items[i].Text;
            }
        }

        private void appMenu_Paint(object sender, PaintEventArgs e) =>
            SilDev.Forms.ContextMenuStrip.SetFixedSingle((ContextMenuStrip)sender, e, Main.Colors.Base);

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
                    if (SilDev.Data.CreateShortcut(Main.GetAppPath(appsBox.SelectedItem.ToString()), Path.Combine("%DesktopDir%", appsBox.SelectedItem.ToString()), Main.CmdLine))
                        SilDev.MsgBox.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        SilDev.MsgBox.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem7":
                    if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("appMenuItem7Msg"), appsBox.SelectedItem), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            string appDir = Path.GetDirectoryName(Main.GetAppPath(appsBox.SelectedItem.ToString()));
                            if (Directory.Exists(appDir))
                            {
                                Directory.Delete(appDir, true);
                                SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            SilDev.MsgBox.Show(this, Lang.GetText("OperationFailedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            SilDev.Log.Debug(ex);
                        }
                    }
                    else
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        #endregion

        #region SEARCH BOX

        private void searchBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Font = new Font("Segoe UI", tb.Font.Size);
            tb.ForeColor = Main.Colors.ControlText;
            tb.Text = SearchText;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Color c = Main.Colors.ControlText;
            tb.Font = new Font("Comic Sans MS", tb.Font.Size, FontStyle.Italic);
            tb.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            SearchText = tb.Text;
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
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
                return;
            List<string> itemList = new List<string>();
            foreach (var item in appsBox.Items)
                itemList.Add(item.ToString());
            foreach (var item in appsBox.Items)
            {
                if (item.ToString() == Main.SearchMatchItem(tb.Text, itemList))
                {
                    appsBox.SelectedItem = item;
                    break;
                }
            }
        }

        #endregion

        #region BUTTONS

        private void addBtn_Click(object sender, EventArgs e) =>
#if x86
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe") });
#else
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader64.exe") });
#endif

        private void addBtn_MouseEnter(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.Image = SilDev.Drawing.ImageGrayScaleSwitch($"{b.Name}BackgroundImage", b.Image);
            toolTip.SetToolTip(b, Lang.GetText($"{b.Name}Tip"));
        }

        private void addBtn_MouseLeave(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.Image = SilDev.Drawing.ImageGrayScaleSwitch($"{b.Name}BackgroundImage", b.Image);
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!SilDev.Forms.Button.Split_Click((Button)sender, appMenu))
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void startBtn_MouseMove(object sender, MouseEventArgs e) =>
            SilDev.Forms.Button.Split_MouseMove((Button)sender);

        private void startBtn_MouseLeave(object sender, EventArgs e) =>
            SilDev.Forms.Button.Split_MouseLeave((Button)sender);

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
                SilDev.Log.Debug(ex);
            }
        }

        #endregion
    }
}
