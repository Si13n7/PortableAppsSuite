using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class MainForm : Form
    {
        protected bool IsStarted, ValidData;
        
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)SilDev.WinAPI.Win32HookAction.WM_COPYDATA:
                    SilDev.WinAPI.CopyDataStruct st = (SilDev.WinAPI.CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(SilDev.WinAPI.CopyDataStruct));
                    string strData = Marshal.PtrToStringUni(st.lpData);
                    if (!string.IsNullOrWhiteSpace(strData) && !Main.CmdLine.ToLower().Contains(strData.ToLower()))
                    {
                        strData = string.Format("{0}{1}{0}", strData.Contains("\"") ? string.Empty : "\"", strData);
                        Main.CmdLine += string.Format("{0}{1}", string.IsNullOrWhiteSpace(Main.CmdLine) ? string.Empty : " ", strData);
                        showBalloonTip(Lang.GetText("notifyIconTip"), strData);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_blue;
            notifyIcon.Icon = Properties.Resources.world_16;
#if !x86
            Text = string.Format("{0} (64-bit)", Text);
#endif
            try
            {
                string WinWidth = SilDev.Initialization.ReadValue("Settings", "WinWidth");
                if (!string.IsNullOrWhiteSpace(WinWidth))
                {
                    int width = int.Parse(WinWidth);
                    if (width >= MinimumSize.Width && width <= MaximumSize.Width)
                        Size = new Size(int.Parse(WinWidth), Height);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            if (!Directory.Exists(Main.AppsPath))
                Close();
            string startItem = SilDev.Initialization.ReadValue("Settings", "StartItem");
            if (startItem == "1" && !searchBox.Focused)
                searchBox.Select();
            else if (!appsBox.Focused)
                appsBox.Select();
            Main.CheckCmdLineApp();
            appsBox_Update();
            Main.CheckPlatformIssues();
            try
            {
                int i = 0;
                int.TryParse(SilDev.Initialization.ReadValue("Settings", "UpdateCheck"), out i);
                if (Main.IsBetween(i, 1, 9))
                {
                    string LastCheck = SilDev.Initialization.ReadValue("History", "LastUpdateCheck");
                    string CheckTime = Main.IsBetween(i, 7, 9) ? DateTime.Today.Month.ToString() : DateTime.Today.Day.ToString();
                    if (LastCheck != CheckTime || Main.IsBetween(i, 1, 3))
                    {
                        if (i != 2 && i != 5 && i != 8)
                        {
                            if (Process.GetProcessesByName("Updater").Length <= 0)
                                SilDev.Run.App(Application.StartupPath, "Binaries\\Updater.exe");
                            bool isUpdating = true;
                            while (isUpdating)
                            {
                                isUpdating = Process.GetProcessesByName("Updater").Length > 0;
                                if (File.Exists(Path.Combine(Application.StartupPath, "Portable.sfx.exe")))
                                    Environment.Exit(1);
                            }
                        }
                        if (i != 3 && i != 6 && i != 9)
                        {
                            if (Process.GetProcessesByName("AppsDownloader").Length <= 0)
                            {
                                SilDev.Run.App(Application.StartupPath, "Binaries\\AppsDownloader.exe", "7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e");
                                foreach (Process p in Process.GetProcessesByName("AppsDownloader"))
                                    p.WaitForExit();
                            }
                            if (Process.GetProcessesByName("PortableAppsUpdater").Length <= 0)
                                SilDev.Run.App(Main.AppsPath, "PortableApps.com\\PortableAppsUpdater.exe", "/STARTUP=true /MODE=UPDATE /KEYBOARDFRIENDLY=false /HIDEPORTABLE=true /BETA=false /CONNECTION=Automatic", 0);
                            Thread.Sleep(1000);
                            foreach (Process p in Process.GetProcessesByName("PortableAppsUpdater"))
                                p.WaitForExit();
                            Main.CheckPlatformIssues();
                        }
                        SilDev.WinAPI.SetForegroundWindow(Handle);
                    }
                    SilDev.Initialization.WriteValue("History", "LastUpdateCheck", CheckTime);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue("History", "PID", Process.GetCurrentProcess().MainWindowHandle);
            if (!string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                RunCmdLine.Enabled = true;
                return;
            }
            Opacity = 1f;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            int StartMenuIntegration = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartMenuIntegration"), out StartMenuIntegration);
            if (StartMenuIntegration > 0)
                Main.StartMenuFolderUpdate(appsBox.Items);
            SilDev.Initialization.WriteValue("History", "PID", 0);
            SilDev.Initialization.WriteValue("Settings", "WinWidth", Size.Width.ToString());
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            Array files;
            ValidData = DragFileName(out files, e);
            if (ValidData)
            {
                string oldCmdLine = Main.CmdLine;
                bool DataAdded = false;
                foreach (object file in files)
                {
                    if (file is string)
                    {
                        if (!Main.CmdLine.Contains(file as string))
                        {
                            string strData = string.Format("{0}{1}{0}", ((string)file).Contains("\"") ? string.Empty : "\"", file);
                            Main.CmdLine += string.Format("{0}{1}", string.IsNullOrWhiteSpace(Main.CmdLine) ? string.Empty : " ", strData);
                            DataAdded = true;
                        }
                    }
                }
                if (DataAdded)
                {
                    showBalloonTip(Lang.GetText("notifyIconTip"), Main.CmdLine.Replace(oldCmdLine, string.Empty).Replace("\" \"", string.Format("\"{0}\"", Environment.NewLine)).TrimStart());
                    Main.CheckCmdLineApp();
                    foreach (var ent in Main.AppsDict)
                    {
                        if (ent.Value == Main.CmdLineApp)
                        {
                            appsBox.SelectedItem = ent.Key;
                            Main.CmdLineApp = string.Empty;
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

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (!IsStarted)
                IsStarted = true;
            else
                appsBox_Update();
        }

        private void appsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void aboutBtn_Click(object sender, EventArgs e)
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
        }

        private void aboutBtn_MouseEnter(object sender, EventArgs e)
        {
            aboutBtn.Image = Properties.Resources.help_16;
        }

        private void aboutBtn_MouseLeave(object sender, EventArgs e)
        {
            aboutBtn.Image = Properties.Resources.help_gray_16;
        }

        private void appsBox_Update()
        {
            Main.CheckAvailableApps();
            string selectedItem = string.Empty;
            if (appsBox.SelectedIndex >= 0)
                selectedItem = appsBox.SelectedItem.ToString();
            appsBox.Items.Clear();
            foreach (string ent in Main.AppsList)
                appsBox.Items.Add(ent);
            if (appsBox.SelectedIndex < 0)
            {
                string lastItem = SilDev.Initialization.ReadValue("History", "LastItem");
                if (!string.IsNullOrWhiteSpace(lastItem))
                    if (appsBox.Items.Contains(lastItem))
                        appsBox.SelectedItem = lastItem;
            }
            if (!string.IsNullOrWhiteSpace(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;
            appsCount.Text = string.Format(Lang.GetText(appsCount), appsBox.Items.Count.ToString());
            int StartMenuIntegration = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartMenuIntegration"), out StartMenuIntegration);
            if (StartMenuIntegration > 0)
                Main.StartMenuFolderUpdate(appsBox.Items);
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            addMenu.Show(MousePosition.X, MousePosition.Y);
        }

        private void addBtn_MouseEnter(object sender, EventArgs e)
        {
            toolTip.SetToolTip(sender as Control, Lang.GetText(string.Format("{0}Tip", (sender as Control).Name)));
            addBtn.Image = Properties.Resources.add_b_13;
        }

        private void addBtn_MouseLeave(object sender, EventArgs e)
        {
            addBtn.Image = Properties.Resources.add_a_13;
        }

        private void addMenu_Opening(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < addMenu.Items.Count; i++)
            {
                string text = Lang.GetText(addMenu.Items[i].Name);
                addMenu.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : addMenu.Items[i].Text;
            }
            string PAPath = Path.Combine(Main.AppsPath, "PortableApps.com\\PortableAppsUpdater.exe");
            if (File.Exists(PAPath))
            {
                for (int i = 1; i < 3; i++)
                    addMenu.Items[addMenu.Items.Count - i].Visible = true;
            }
            else
            {
                for (int i = 1; i < 3; i++)
                    addMenu.Items[addMenu.Items.Count - i].Visible = false;
            }
        }

        private void addMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            switch (i.Name)
            {
                case "addMenuItem1":
                    SilDev.Run.App(Application.StartupPath, "Binaries\\AppsDownloader.exe");
                    break;
                case "addMenuItem2":
                    SilDev.Run.App(Application.StartupPath, "Binaries\\PremiumAppsDownloader.exe");
                    break;
                case "addMenuItem4":
                    SilDev.Run.App(Main.AppsPath, "PortableApps.com\\PortableAppsUpdater.exe", "/MODE=ADD /OPENSOURCEONLY=false /KEYBOARDFRIENDLY=false /ADVANCED=true /SHOWINSTALLEDAPPS=false /HIDEPORTABLE=true /BETA=false /ORDER=category /CONNECTION=Automatic");
                    break;
            }
        }

        private void appMenuItem_Opening(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < appMenu.Items.Count; i++)
            {
                string text = Lang.GetText(appMenu.Items[i].Name);
                appMenu.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : appMenu.Items[i].Text;
            }
        }

        private void appMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            switch (i.Name)
            {
                case "appMenuItem1":
                    Main.StartApp(appsBox.SelectedItem.ToString(), !string.IsNullOrWhiteSpace(Main.CmdLineApp));
                    break;
                case "appMenuItem2":
                    Main.StartApp(appsBox.SelectedItem.ToString(), !string.IsNullOrWhiteSpace(Main.CmdLineApp), true);
                    break;
                case "appMenuItem3":
                    Main.OpenAppLocation(appsBox.SelectedItem.ToString());
                    break;
                case "appMenuItem4":
                    if (SilDev.Data.CreateShortcut(Main.GetAppPath(Main.AppsDict[appsBox.SelectedItem.ToString()]), Path.Combine("%DesktopDir%", appsBox.SelectedItem.ToString())))
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("appMenuItem5Msg"), appsBox.SelectedItem), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            string appDir = Path.GetDirectoryName(Main.GetAppPath(Main.AppsDict[appsBox.SelectedItem.ToString()]));
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

        private void searchBox_Enter(object sender, EventArgs e)
        {
            searchBox.Font = new Font("Segoe UI", 8.25F);
            searchBox.ForeColor = SystemColors.WindowText;
            searchBox.Text = string.Empty;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            searchBox.Font = new Font("Comic Sans MS", 8.25F);
            searchBox.ForeColor = SystemColors.GrayText;
            searchBox.Text = Lang.GetText(searchBox);
        }

        private void searchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
                return;
            }
            searchBox.Refresh();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string search = searchBox.Text.ToLower();
                string[] split = null;
                if (search.Contains("*") && !search.StartsWith("*") && !search.EndsWith("*"))
                    split = search.Split('*');
                bool match = false;
                for (int i = 0; i < 2; i++)
                {
                    foreach (var item in appsBox.Items)
                    {
                        if (i < 1 && split != null && split.Length == 2)
                        {
                            var regex = new Regex(string.Format(".*{0}(.*){1}.*", split[0], split[1]), RegexOptions.IgnoreCase);
                            match = regex.IsMatch(item.ToString());
                        }
                        else
                        {
                            match = item.ToString().StartsWith(search, StringComparison.OrdinalIgnoreCase);
                            if (i > 0 && !match)
                                match = item.ToString().ToLower().Contains(search);
                        }
                        if (match)
                        {
                            appsBox.SelectedItem = item;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Main.StartApp(appsBox.SelectedItem.ToString(), !string.IsNullOrWhiteSpace(Main.CmdLineApp));
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (Form dialog = new SettingsForm(appsBox.SelectedItem.ToString()))
                {
                    dialog.TopMost = TopMost;
                    dialog.ShowDialog();
                    Lang.SetControlLang(this);
                    appsBox_Update();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void RunCmdLine_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                    return;
                foreach (string app in Main.AppsList)
                    if (Main.AppsDict[app] == Main.CmdLineApp)
                        appsBox.SelectedItem = app;
                if (appsBox.SelectedIndex > 0)
                {
                    bool noConfirm = bool.Parse(SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm"));
                    if (!Main.CmdLineMultipleApps && noConfirm)
                    {
                        RunCmdLine.Enabled = false;
                        Main.StartApp(appsBox.SelectedItem.ToString(), true);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            RunCmdLine.Enabled = false;
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
            for (int i = 0; i < 3000; i++)
            {
                if (notifyIconDisabler.CancellationPending)
                { 
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(1);
            }
        }

        private void notifyIconDisabler_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (notifyIcon.Visible)
                notifyIcon.Visible = false;
        }

        private void showBalloonTip(string _title, string _tip)
        {
            if (!notifyIcon.Visible)
                notifyIcon.Visible = true;
            if (!notifyIconDisabler.IsBusy)
                notifyIconDisabler.RunWorkerAsync();
            notifyIcon.ShowBalloonTip(1800, _title, _tip, ToolTipIcon.Info);
        }
    }
}
