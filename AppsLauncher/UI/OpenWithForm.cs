using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class OpenWithForm : Form
    {
        protected bool IsStarted, ValidData;
        
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

        public OpenWithForm()
        {
            InitializeComponent(); 
            Icon = Properties.Resources.PortableApps_blue;
            BackColor = Color.FromArgb(255, Main.Colors.Layout.R, Main.Colors.Layout.G, Main.Colors.Layout.B);
            notifyIcon.Icon = Properties.Resources.world_16;
            startBtn.Image = new Bitmap(12, 20);
            using (Graphics g = Graphics.FromImage(startBtn.Image))
            {
                Pen pen = new Pen(Main.Colors.ButtonText, 1);
                g.DrawLine(pen, 0, 0, 0, startBtn.Image.Height - 3);
                int width = startBtn.Image.Width - 6;
                int height = startBtn.Image.Height - 12;
                g.DrawLine(pen, width, height, width + 5, height);
                g.DrawLine(pen, width + 1, height + 1, width + 1 + 3, height + 1);
                g.DrawLine(pen, width + 2, height + 2, width + 2 + 1, height + 2);
            }
            foreach (Button btn in new Button[] { startBtn, settingsBtn })
            {
                btn.ForeColor = Main.Colors.ButtonText;
                btn.BackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }
            appMenuItem2.Image = SilDev.Drawing.SystemIconAsImage(SilDev.Drawing.SystemIconKey.UserAccountControl);
            appMenuItem3.Image = SilDev.Drawing.SystemIconAsImage(SilDev.Drawing.SystemIconKey.Folder);
            appMenuItem6.Image = SilDev.Drawing.SystemIconAsImage(SilDev.Drawing.SystemIconKey.RecycleBinEmpty);
            if (!searchBox.Focused)
                searchBox.Select();
        }

        private void OpenWithForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            Text = Lang.GetText($"{Name}Title");
            if (!Directory.Exists(Main.AppsPath))
                Main.RepairAppsLauncher();
            Main.CheckCmdLineApp();
            appsBox_Update(false);
        }

        private void OpenWithForm_Shown(object sender, EventArgs e)
        {
            SilDev.Ini.Write("History", "PID", Handle);
            if (!string.IsNullOrWhiteSpace(Main.CmdLineApp))
            {
                RunCmdLine.Enabled = true;
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
            SilDev.Ini.Write("History", "PID", 0);

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

        private void appsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void appsBox_Update(bool _forceAppCheck)
        {
            if (Main.AppsDict.Count == 0 || _forceAppCheck)
                Main.CheckAvailableApps();
            string selectedItem = string.Empty;
            if (appsBox.SelectedIndex >= 0)
                selectedItem = appsBox.SelectedItem.ToString();
            appsBox.Items.Clear();
            foreach (string ent in Main.AppsList)
                appsBox.Items.Add(ent);
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
            appsCount.Text = string.Format(Lang.GetText(appsCount), appsBox.Items.Count, appsBox.Items.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"));
            int StartMenuIntegration = SilDev.Ini.ReadInteger("Settings", "StartMenuIntegration");
            if (StartMenuIntegration > 0)
            {
                List<string> list = new List<string>();
                foreach (string item in appsBox.Items)
                    list.Add(item);
                Main.StartMenuFolderUpdate(list);
            }
        }

        private void addBtn_Click(object sender, EventArgs e) => 
#if x86
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe") });
#else
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader64.exe") });
#endif

        private void addBtn_MouseEnter(object sender, EventArgs e)
        {
            toolTip.SetToolTip((Control)sender, Lang.GetText($"{((Control)sender).Name}Tip"));
            Button b = (Button)sender;
            b.Image = SilDev.Drawing.ImageGrayScaleSwitch($"{b.Name}BackgroundImage", b.Image);
        }

        private void addBtn_MouseLeave(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.Image = SilDev.Drawing.ImageGrayScaleSwitch($"{b.Name}BackgroundImage", b.Image);
        }

        private void appMenuItem_Opening(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < appMenu.Items.Count; i++)
            {
                string text = Lang.GetText(appMenu.Items[i].Name);
                appMenu.Items[i].Text = !string.IsNullOrWhiteSpace(text) ? text : appMenu.Items[i].Text;
            }
        }

        private void appMenu_Paint(object sender, PaintEventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)sender;
            using (GraphicsPath gp = new GraphicsPath())
            {
                RectangleF rect = new RectangleF(2, 2, cms.Width - 4, cms.Height - 4);
                gp.AddRectangle(rect);
                cms.Region = new Region(new RectangleF(2, 2, cms.Width - 4, cms.Height - 4));
                gp.AddRectangle(new RectangleF(2, 2, cms.Width - 5, cms.Height - 5));
                e.Graphics.FillPath(Brushes.DarkGray, gp);
                e.Graphics.DrawPath(new Pen(Main.Colors.Layout, 1), gp);
            }
        }

        private void appMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            switch (i.Name)
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
                    if (SilDev.Data.CreateShortcut(Main.GetAppPath(Main.AppsDict[appsBox.SelectedItem.ToString()]), Path.Combine("%DesktopDir%", appsBox.SelectedItem.ToString()), Main.CmdLine))
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem6":
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
            TextBox tb = (TextBox)sender;
            tb.Font = new Font("Segoe UI", 8.25F);
            tb.ForeColor = SystemColors.WindowText;
            tb.Text = string.Empty;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Font = new Font("Comic Sans MS", 8.25F);
            tb.ForeColor = SystemColors.GrayText;
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

        private void startBtn_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (PointToClient(MousePosition).X >= (b.Width - 6))
                appMenu.Show(b, new Point(0, b.Height), ToolStripDropDownDirection.BelowRight);
            else
                Main.StartApp(appsBox.SelectedItem.ToString(), true);
        }

        private void startBtn_MouseMove(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            startBtn_MouseLeave(sender, EventArgs.Empty);
            if (PointToClient(MousePosition).X >= (b.Width - 6))
            {
                if (b.BackgroundImage == null)
                {
                    b.BackgroundImage = new Bitmap(b.Width - 16, b.Height);
                    using (Graphics g = Graphics.FromImage(b.BackgroundImage))
                    {
                        using (Brush brush = new SolidBrush(Main.Colors.Button))
                            g.FillRectangle(brush, 0, 0, b.BackgroundImage.Width, b.BackgroundImage.Height);
                    }
                }
            }
            else
            {
                b.BackgroundImage = new Bitmap(b.Width, b.Height);
                using (Graphics g = Graphics.FromImage(b.BackgroundImage))
                {
                    using (Brush brush = new SolidBrush(Main.Colors.Button))
                        g.FillRectangle(brush, 0, 0, b.BackgroundImage.Width, b.BackgroundImage.Height);
                    using (Brush brush = new SolidBrush(Main.Colors.ButtonHover))
                        g.FillRectangle(brush, 0, 0, b.BackgroundImage.Width - 15, b.BackgroundImage.Height);
                }
            }
        }

        private void startBtn_MouseLeave(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.BackgroundImage != null)
                b.BackgroundImage = null;
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
                    Main.SetAppDirs();
                    appsBox_Update(true);
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
                    bool noConfirm = SilDev.Ini.ReadBoolean(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm");
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
                if (((BackgroundWorker)sender).CancellationPending)
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
    }
}
