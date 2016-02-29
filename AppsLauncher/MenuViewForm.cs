using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class MenuViewForm : Form
    {
        protected override void WndProc(ref Message m)
        {
            const uint HTLEFT = 10;
            const uint HTRIGHT = 11;
            const uint HTBOTTOMRIGHT = 17;
            const uint HTBOTTOM = 15;
            const uint HTBOTTOMLEFT = 16;
            const uint HTTOP = 12;
            const uint HTTOPRIGHT = 14;
            bool handled = false;
            if (m.Msg == 0x0084 || m.Msg == 0x0200)
            {
                Size wndSize = Size;
                Point scrPoint = new Point(m.LParam.ToInt32());
                Point clntPoint = PointToClient(scrPoint);
                Dictionary<uint, Rectangle> hitBoxes = new Dictionary<uint, Rectangle>();
                switch (SilDev.WinAPI.TaskBar.GetLocation())
                {
                    case SilDev.WinAPI.TaskBar.Location.LEFT:
                    case SilDev.WinAPI.TaskBar.Location.TOP:
                        hitBoxes.Add(HTRIGHT, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTBOTTOMRIGHT, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTBOTTOM, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case SilDev.WinAPI.TaskBar.Location.RIGHT:
                        hitBoxes.Add(HTLEFT, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTBOTTOMLEFT, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTBOTTOM, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    default:
                        hitBoxes.Add(HTRIGHT, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTTOPRIGHT, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        hitBoxes.Add(HTTOP, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        break;
                }
                foreach (KeyValuePair<uint, Rectangle> hitBox in hitBoxes)
                {
                    if (hitBox.Value.Contains(clntPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }
            }
            if (!handled)
                base.WndProc(ref m);
        }

        double WindowOpacity = .95f;
        int WindowFadeInDuration = 4;
        bool AppStartEventCalled = false;

        public MenuViewForm()
        {
            InitializeComponent();
#if !x86
            Text = $"{Text} (64-bit)";
#endif
            Icon = Properties.Resources.PortableApps_blue;
            BackColor = Color.FromArgb(255, Main.LayoutColor.R, Main.LayoutColor.G, Main.LayoutColor.B);
            tableLayoutPanel1.BackgroundImage = Main.LayoutBackground;
            tableLayoutPanel1.BackColor = Main.LayoutColor;
            foreach (Button btn in new Button[] { downloadBtn, settingsBtn })
            {
                btn.ForeColor = Main.ButtonTextColor;
                btn.BackColor = Main.ButtonColor;
                btn.FlatAppearance.MouseOverBackColor = Main.ButtonHoverColor;
            }
            logoBox.Image = Main.GetImageFiltered(Properties.Resources.PortableApps_Logo_gray, logoBox.Height, logoBox.Height);
            if (!searchBox.Focus())
                searchBox.Select();
        }

        private void MenuViewForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            for (int i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Lang.GetText(appMenu.Items[i].Name);

            if (!Directory.Exists(Main.AppsPath))
                Main.RepairAppsLauncher();

            if (double.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowOpacity"), out WindowOpacity))
            {
                if (WindowOpacity >= 20 && WindowOpacity <= 100)
                    WindowOpacity /= 100d;
                else
                    WindowOpacity = .95d;
            }
            else
                WindowOpacity = .95d;

            if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowFadeInDuration"), out WindowFadeInDuration))
            {
                if (WindowFadeInDuration < 1)
                    WindowFadeInDuration = 1;
                int opacity = (int)(WindowOpacity * 100d);
                if (WindowFadeInDuration > opacity)
                    WindowFadeInDuration = opacity;
            }
            else
                WindowFadeInDuration = 4;

            int WindowWidth = MinimumSize.Width;
            if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowWidth"), out WindowWidth))
            {
                if (WindowWidth > MinimumSize.Width && WindowWidth < Screen.PrimaryScreen.WorkingArea.Width)
                    Width = WindowWidth;
                if (WindowWidth > Screen.PrimaryScreen.WorkingArea.Width)
                    Width = Screen.PrimaryScreen.WorkingArea.Width;
            }

            int WindowHeight = MinimumSize.Height;
            if (int.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowHeight"), out WindowHeight))
            {
                if (WindowHeight > MinimumSize.Height && WindowHeight < Screen.PrimaryScreen.WorkingArea.Height)
                    Height = WindowHeight;
                if (WindowHeight > Screen.PrimaryScreen.WorkingArea.Height)
                    Height = Screen.PrimaryScreen.WorkingArea.Height;
            }

            MenuViewForm_Update();

            if (SilDev.WinAPI.SafeNativeMethods.GetForegroundWindow() != Handle)
                SilDev.WinAPI.SafeNativeMethods.SetForegroundWindow(Handle);

            if (!searchBox.Focus())
                searchBox.Select();

            if (!fadeInTimer.Enabled)
                fadeInTimer.Enabled = true;
        }

        private void MenuViewForm_Deactivate(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count > 1 || appMenu.Focus() || AppStartEventCalled)
                return;
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
                Close();
        }

        private void MenuViewForm_ResizeBegin(object sender, EventArgs e)
        {
            if (!appsListView.Scrollable)
                appsListView.Scrollable = true;
        }

        private void MenuViewForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!searchBox.Focus())
                searchBox.Select();
            SilDev.Initialization.WriteValue("Settings", "WindowWidth", Width);
            SilDev.Initialization.WriteValue("Settings", "WindowHeight", Height);
        }

        private void MenuViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppStartEventCalled = true;
            if (Opacity != 0)
                Opacity = 0;
            int StartMenuIntegration = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartMenuIntegration"), out StartMenuIntegration);
            if (StartMenuIntegration > 0)
            {
                try
                {
                    List<string> list = new List<string>();
                    for (int i = 0; i < appsListView.Items.Count; i++)
                        list.Add(appsListView.Items[i].Text);
                    Main.StartMenuFolderUpdate(list);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            Main.CheckUpdates();
        }

        private void MenuViewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        private void MenuViewForm_Update(bool _setWindowLocation)
        {
            try
            {
                Main.CheckAvailableApps();
                appsListView.BeginUpdate();
                appsListView.Items.Clear();
                if (!appsListView.Scrollable)
                    appsListView.Scrollable = true;
                imgList.Images.Clear();
                string CacheDir = Path.Combine(Application.StartupPath, "Assets\\cache");
                try
                {
                    if (!Directory.Exists(CacheDir))
                        Directory.CreateDirectory(CacheDir);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    if (!SilDev.Elevation.IsAdministrator)
                        SilDev.Elevation.RestartAsAdministrator(Main.CmdLine);
                }
                Image DefaultExeIcon = Main.GetImageFiltered(Properties.Resources.executable, 16, 16);
                for (int i = 0; i < Main.AppsList.Count; i++)
                {
                    appsListView.Items.Add(Main.AppsList[i], i);
                    try
                    {
                        string appPath = Main.GetAppPath(Main.AppsDict[Main.AppsList[i]]);
                        string nameHash = SilDev.Crypt.MD5.Encrypt(Main.AppsDict[Main.AppsList[i]]);
                        string iconDbPath = Path.Combine(Application.StartupPath, "Assets\\icon.db");
                        if (File.Exists(iconDbPath))
                        {
                            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(iconDbPath)))
                            {
                                try
                                {
                                    using (ZipArchive archive = new ZipArchive(stream))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            if (entry.Name != nameHash)
                                                continue;
                                            imgList.Images.Add(nameHash, Image.FromStream(entry.Open()));
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SilDev.Log.Debug(ex);
                                }
                            }
                        }
                        if (imgList.Images.ContainsKey(nameHash))
                            continue;
                        string imgPath = Path.Combine(CacheDir, nameHash);
                        if (!File.Exists(imgPath))
                        {
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            imgPath = Path.Combine(Path.GetDirectoryName(appPath), $"{Path.GetFileNameWithoutExtension(appPath)}.png");
                            if (!File.Exists(imgPath))
                                imgPath = Path.Combine(Path.GetDirectoryName(appPath), "App\\AppInfo\\appicon_16.png");
                            if (File.Exists(imgPath))
                            {
                                Image imgFromFile = Main.GetImageFiltered(Image.FromFile(imgPath), 16, 16);
                                imgPath = Path.Combine(CacheDir, nameHash);
                                imgFromFile.Save(imgPath);
                                imgList.Images.Add(nameHash, imgFromFile);
                            }
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            imgPath = Path.Combine(CacheDir, nameHash);
                            Icon ico = Main.GetSmallIcon(appPath);
                            if (ico != null)
                            {
                                Image imgFromIcon = Main.GetImageFiltered(ico.ToBitmap(), 16, 16);
                                imgFromIcon.Save(imgPath);
                                imgList.Images.Add(nameHash, imgFromIcon);
                            }
                            else
                                throw new Exception();
                        }
                        else
                            imgList.Images.Add(nameHash, Image.FromFile(imgPath));
                    }
                    catch
                    {
                        imgList.Images.Add(DefaultExeIcon);
                    }
                }
                foreach (string file in Directory.GetFiles(CacheDir, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!imgList.Images.ContainsKey(Path.GetFileName(file)))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                }
                appsListView.SmallImageList = imgList;
                if (_setWindowLocation)
                {
                    int defaultPos = 0;
                    int.TryParse(SilDev.Initialization.ReadValue("Settings", "DefaultPosition"), out defaultPos);
                    if (defaultPos == 0)
                    {
                        switch (SilDev.WinAPI.TaskBar.GetLocation())
                        {
                            case SilDev.WinAPI.TaskBar.Location.LEFT:
                                Left = Screen.PrimaryScreen.WorkingArea.X;
                                Top = 0;
                                break;
                            case SilDev.WinAPI.TaskBar.Location.TOP:
                                Left = 0;
                                Top = Screen.PrimaryScreen.WorkingArea.Y;
                                break;
                            case SilDev.WinAPI.TaskBar.Location.RIGHT:
                                Left = Screen.PrimaryScreen.WorkingArea.Width - Width;
                                Top = 0;
                                break;
                            default:
                                Left = 0;
                                Top = Screen.PrimaryScreen.WorkingArea.Height - Height;
                                break;
                        }
                    }
                    else
                    {
                        Point newLocation = GetWindowStartPos(new Point(Width, Height));
                        Left = newLocation.X;
                        Top = newLocation.Y;
                    }
                }
                appsListView.EndUpdate();
                appsCount.Text = string.Format(Lang.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? "App" : "Apps");
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                MenuViewForm_FormClosed(null, null);
            }
        }

        private void MenuViewForm_Update() =>
            MenuViewForm_Update(true);

        private void fadeInTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity < WindowOpacity)
                Opacity += WindowOpacity / WindowFadeInDuration;
            else
                fadeInTimer.Enabled = false;
        }

        private void appsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            if (appsListView.SelectedItems.Count > 0)
            {
                AppStartEventCalled = true;
                if (Opacity != 0)
                    Opacity = 0;
                Main.StartApp(appsListView.SelectedItems[0].Text, true);
            }
        }

        private void appsListView_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (!e.Item.Selected)
                e.Item.Selected = true;
        }

        private void appMenu_Opening(object sender, CancelEventArgs e) =>
            e.Cancel = appsListView.SelectedItems.Count == 0;

        private void appMenu_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)sender;
            cms.Left -= 48;
            cms.Top -= 10;
        }

        private void appMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            switch (i.Name)
            {
                case "appMenuItem1":
                case "appMenuItem2":
                case "appMenuItem3":
                    if (Opacity != 0)
                        Opacity = 0;
                    switch (i.Name)
                    {
                        case "appMenuItem1":
                            Main.StartApp(appsListView.SelectedItems[0].Text, true);
                            break;
                        case "appMenuItem2":
                            Main.StartApp(appsListView.SelectedItems[0].Text, true, true);
                            break;
                        case "appMenuItem3":
                            Main.OpenAppLocation(appsListView.SelectedItems[0].Text, true);
                            break;
                    }
                    break;
                case "appMenuItem4":
                    SilDev.MsgBox.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (SilDev.Data.CreateShortcut(Main.GetAppPath(Main.AppsDict[appsListView.SelectedItems[0].Text]), Path.Combine("%DesktopDir%", appsListView.SelectedItems[0].Text)))
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        SilDev.MsgBox.Show(this, Lang.GetText("ShortcutCreatedMsg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    if (appsListView.SelectedItems.Count > 0)
                    {
                        if (!appsListView.LabelEdit)
                            appsListView.LabelEdit = true;
                        appsListView.SelectedItems[0].BeginEdit();
                    }
                    break;
                case "appMenuItem6":
                    SilDev.MsgBox.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("appMenuItem5Msg"), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SilDev.MsgBox.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            string appDir = Path.GetDirectoryName(Main.GetAppPath(Main.AppsDict[appsListView.SelectedItems[0].Text]));
                            if (Directory.Exists(appDir))
                            {
                                Directory.Delete(appDir, true);
                                MenuViewForm_Update(false);
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
                    {
                        SilDev.MsgBox.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
            }
            if (SilDev.MsgBox.MoveCursorToMsgBoxAtOwner)
                SilDev.MsgBox.MoveCursorToMsgBoxAtOwner = false;
        }

        private void appMenu_MouseLeave(object sender, EventArgs e) =>
            appMenu.Close();

        private void appsListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && appsListView.SelectedItems.Count > 0)
            {
                if (!appsListView.LabelEdit)
                    appsListView.LabelEdit = true;
                appsListView.SelectedItems[0].BeginEdit();
            }
        }

        private void appsListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                try
                {
                    string appPath = Main.GetAppPath(Main.AppsDict[appsListView.SelectedItems[0].Text]);
                    string appIniPath = Path.Combine(Path.GetDirectoryName(appPath), $"{Path.GetFileName(Path.GetDirectoryName(appPath))}.ini");
                    if (!File.Exists(appIniPath))
                        File.Create(appIniPath).Close();
                    SilDev.Initialization.WriteValue("AppInfo", "Name", e.Label, appIniPath);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
                MenuViewForm_Update();
            }
        }

        private void openNewFormBtn_Click(object sender, EventArgs e)
        {
            Form form;
            if (sender is Button)
                form = new SettingsForm(appsListView.SelectedItems.Count > 0 ? appsListView.SelectedItems[0].Text : string.Empty);
            else if (sender is PictureBox)
                form = new AboutForm();
            else
                return;
            if (TopMost)
                TopMost = false;
            try
            {
                using (Form dialog = form)
                {
                    Point point = GetWindowStartPos(new Point(dialog.Width, dialog.Height));
                    if (point != new Point(0, 0))
                    {
                        dialog.StartPosition = FormStartPosition.Manual;
                        dialog.Left = point.X;
                        dialog.Top = point.Y;
                    }
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            if (sender is Button)
                Close();
            else
            {
                if (!TopMost)
                    TopMost = true;
                if (SilDev.WinAPI.SafeNativeMethods.GetForegroundWindow() != Handle)
                    SilDev.WinAPI.SafeNativeMethods.SetForegroundWindow(Handle);
                if (!searchBox.Focus())
                    searchBox.Select();
            }
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
#if x86
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe") });
#else
            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader64.exe") });
#endif
            Close();
        }

        private Point GetWindowStartPos(Point _point)
        {
            Point point = new Point();
            int defaultPos = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "DefaultPosition"), out defaultPos);
            if (defaultPos == 0)
            {
                switch (SilDev.WinAPI.TaskBar.GetLocation())
                {
                    case SilDev.WinAPI.TaskBar.Location.LEFT:
                        point.X = Cursor.Position.X - (_point.X / 2);
                        point.Y = Cursor.Position.Y;
                        break;
                    case SilDev.WinAPI.TaskBar.Location.TOP:
                        point.X = Cursor.Position.X - (_point.X / 2);
                        point.Y = Cursor.Position.Y;
                        break;
                    case SilDev.WinAPI.TaskBar.Location.RIGHT:
                        point.X = Screen.PrimaryScreen.WorkingArea.Width - _point.X;
                        point.Y = Cursor.Position.Y;
                        break;
                    default:
                        point.X = Cursor.Position.X - (_point.X / 2);
                        point.Y = Cursor.Position.Y - _point.Y;
                        break;
                }
                if (point.X + _point.X > Screen.PrimaryScreen.WorkingArea.Width)
                    point.Y = Screen.PrimaryScreen.WorkingArea.Width - _point.X;
                if (point.Y + _point.Y > Screen.PrimaryScreen.WorkingArea.Height)
                    point.Y = Screen.PrimaryScreen.WorkingArea.Height - _point.Y;
            }
            else
            {
                int maxWidth = Screen.PrimaryScreen.WorkingArea.Width - _point.X;
                point.X = Cursor.Position.X > _point.X / 2 && Cursor.Position.X < maxWidth ? Cursor.Position.X - _point.X / 2 : Cursor.Position.X > maxWidth ? maxWidth : Cursor.Position.X;
                int maxHeight = Screen.PrimaryScreen.WorkingArea.Height - _point.Y;
                point.Y = Cursor.Position.Y > _point.Y / 2 && Cursor.Position.Y < maxHeight ? Cursor.Position.Y - _point.Y / 2 : Cursor.Position.Y > maxHeight ? maxHeight : Cursor.Position.Y;
            }
            return point;
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
            tb.Font = new Font("Comic Sans MS", 8.25F, FontStyle.Italic);
            tb.ForeColor = SystemColors.GrayText;
            string text = Lang.GetText(tb).Replace(" ", string.Empty).ToLower();
            tb.Text = $"{text.Substring(0, 1).ToUpper()}{text.Substring(1)}";
        }

        private void searchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (appsListView.SelectedItems.Count > 0)
                {
                    AppStartEventCalled = true;
                    Main.StartApp(appsListView.SelectedItems[0].Text, true);
                }
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
            foreach (ListViewItem item in appsListView.Items)
            {
                item.ForeColor = SystemColors.ControlText;
                item.BackColor = SystemColors.Control;
                itemList.Add(item.Text);
            }
            foreach (ListViewItem item in appsListView.Items)
            {
                if (item.Text == Main.SearchMatchItem(tb.Text, itemList))
                {
                    item.ForeColor = SystemColors.Control;
                    item.BackColor = SystemColors.HotTrack;
                    item.Selected = true;
                    break;
                }
            }
        }

        private void closeBtn_Click(object sender, EventArgs e) =>
            Application.Exit();

        private void aboutBtn_MouseEnter(object sender, EventArgs e) =>
            ((PictureBox)sender).Image = Properties.Resources.help_16;

        private void aboutBtn_MouseLeave(object sender, EventArgs e) =>
            ((PictureBox)sender).Image = Properties.Resources.help_gray_16;
    }
}
