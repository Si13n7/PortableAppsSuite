using SilDev;
using SilDev.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class MenuViewForm : Form
    {
        #region OVERRIDES

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.LWin || keyData == Keys.RWin)
            {
                Application.Exit();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Resize function for borderless window
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
                Dictionary<uint, Rectangle> hitBoxes = new Dictionary<uint, Rectangle>();
                switch (TASKBAR.GetLocation(Handle))
                {
                    case TASKBAR.Location.LEFT:
                    case TASKBAR.Location.TOP:
                        hitBoxes.Add(HTRIGHT, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTBOTTOMRIGHT, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTBOTTOM, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TASKBAR.Location.RIGHT:
                        hitBoxes.Add(HTLEFT, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTBOTTOMLEFT, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTBOTTOM, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TASKBAR.Location.BOTTOM:
                        hitBoxes.Add(HTRIGHT, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTTOPRIGHT, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        hitBoxes.Add(HTTOP, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        break;
                    default:
                        hitBoxes.Add(HTLEFT, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTRIGHT, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(HTBOTTOMRIGHT, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTBOTTOM, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(HTBOTTOMLEFT, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(HTTOP, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(HTTOPRIGHT, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        break;
                }
                Point scrPoint = new Point(m.LParam.ToInt32());
                Point clntPoint = PointToClient(scrPoint);
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

        #endregion

        #region INITIALIZE FORM

        double WindowOpacity = .95f;
        int WindowFadeInDuration = 1;
        bool AppStartEventCalled = false;
        bool HideHScrollBar = false;
        string SearchText = string.Empty;
        Point appsListViewCursorLocation;

        public MenuViewForm()
        {
            InitializeComponent();

            BackColor = Color.FromArgb(255, Main.Colors.Base.R, Main.Colors.Base.G, Main.Colors.Base.B);
            if (Main.ScreenDpi > 96)
                Font = SystemFonts.CaptionFont;
            Icon = Properties.Resources.PortableApps_blue;

            layoutPanel.BackgroundImage = Main.BackgroundImage;
            layoutPanel.BackgroundImageLayout = Main.BackgroundImageLayout;
            layoutPanel.BackColor = Main.Colors.Base;

            if (Main.ScreenDpi > 96)
                appsListViewPanel.Font = SystemFonts.SmallCaptionFont;
            appsListViewPanel.BackColor = Main.Colors.Control;
            appsListViewPanel.ForeColor = Main.Colors.ControlText;
            appsListView.BackColor = appsListViewPanel.BackColor;
            appsListView.ForeColor = appsListViewPanel.ForeColor;
            CONTROL.DoubleBuffering(appsListView);

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            TEXTBOX.DrawSearchSymbol(searchBox, Main.Colors.ControlText);

            title.ForeColor = Main.Colors.ControlText;
            logoBox.Image = DRAWING.ImageFilter(Properties.Resources.PortableApps_Logo_gray, logoBox.Height, logoBox.Height);
            appsCount.ForeColor = title.ForeColor;

            aboutBtn.BackgroundImage = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.HELP, Main.SystemResourcePath);
            aboutBtn.BackgroundImage = DRAWING.ImageGrayScaleSwitch($"{aboutBtn.Name}BackgroundImage", aboutBtn.BackgroundImage);

            profileBtn.BackgroundImage = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.USER_DIR, true, Main.SystemResourcePath);
            downloadBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.NETWORK, Main.SystemResourcePath);
            settingsBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.SYSTEM_CONTROL, Main.SystemResourcePath);
            foreach (Button btn in new Button[] { downloadBtn, settingsBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appMenuItem2.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath);
            appMenuItem3.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.DIRECTORY, Main.SystemResourcePath);
            appMenuItem5.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.PIN, Main.SystemResourcePath);
            appMenuItem7.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.RECYCLE_BIN_EMPTY, Main.SystemResourcePath);

            CONTROL.DrawSizeGrip(sizeGrip, Main.Colors.Base);

            if (LOG.DebugMode > 0)
            {
                Shown += new EventHandler((s, e) => 
                {
                    LOG.Stopwatch.Stop();
                    INI.Write("History", "StartTime", LOG.Stopwatch.Elapsed.TotalSeconds);
                });
            }
        }

        #endregion

        #region FORM EVENTS

        private void MenuViewForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            for (int i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Lang.GetText(appMenu.Items[i].Name);

            string docDir = PATH.Combine("%CurDir%\\Documents");
            if (Directory.Exists(docDir) && DATA.DirIsLink(docDir) && !DATA.MatchAttributes(docDir, FileAttributes.Hidden))
                DATA.SetAttributes(docDir, FileAttributes.Hidden);

            WindowOpacity = INI.ReadDouble("Settings", "Window.Opacity", 95);
            if (WindowOpacity >= 20d && WindowOpacity <= 100d)
                WindowOpacity /= 100d;
            else
                WindowOpacity = .95d;

            WindowFadeInDuration = INI.ReadInteger("Settings", "Window.FadeInDuration", 1);
            if (WindowFadeInDuration < 1)
                WindowFadeInDuration = 1;
            int opacity = (int)(WindowOpacity * 100d);
            if (WindowFadeInDuration > opacity)
                WindowFadeInDuration = opacity;

            int WindowWidth = INI.ReadInteger("Settings", "Window.Size.Width", MinimumSize.Width);
            if (WindowWidth > MinimumSize.Width && WindowWidth < Screen.FromHandle(Handle).WorkingArea.Width)
                Width = WindowWidth;
            if (WindowWidth > Screen.FromHandle(Handle).WorkingArea.Width)
                Width = Screen.FromHandle(Handle).WorkingArea.Width;

            int WindowHeight = INI.ReadInteger("Settings", "Window.Size.Height", MinimumSize.Height);
            if (WindowHeight > MinimumSize.Height && WindowHeight < Screen.FromHandle(Handle).WorkingArea.Height)
                Height = WindowHeight;
            if (WindowHeight > Screen.FromHandle(Handle).WorkingArea.Height)
                Height = Screen.FromHandle(Handle).WorkingArea.Height;

            WINAPI.SafeNativeMethods.SendMessage(appsListView.Handle, 4158, IntPtr.Zero, Cursors.Arrow.Handle);
            HideHScrollBar = INI.ReadBoolean("Settings", "Window.HideHScrollBar", false);
            MenuViewForm_Resize(null, EventArgs.Empty);

            if (LOG.DebugMode > 1)
                closeBtn.Visible = true;

            MenuViewForm_Update();

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
            layoutPanel.BackgroundImage = null;
        }

        private void MenuViewForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!appsListView.Focus())
                appsListView.Select();
            layoutPanel.BackgroundImage = Main.BackgroundImage;
            INI.Write("Settings", "Window.Size.Width", Width);
            INI.Write("Settings", "Window.Size.Height", Height);
        }

        private void MenuViewForm_Resize(object sender, EventArgs e)
        {
            if (HideHScrollBar)
            {
                if (appsListView.Dock != DockStyle.None)
                    appsListView.Dock = DockStyle.None;
                int padding = (int)Math.Floor(SystemInformation.HorizontalScrollBarHeight / 3f);
                appsListView.Location = new Point(padding, padding);
                appsListView.Size = appsListViewPanel.Size;
                appsListView.Region = new Region(new RectangleF(0, 0, appsListViewPanel.Width - padding, appsListViewPanel.Height - SystemInformation.HorizontalScrollBarHeight));
            }
        }

        private void MenuViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppStartEventCalled = true;
            if (Opacity != 0)
                Opacity = 0;
            if (File.Exists(INI.File()))
            {
                string tmpIniPath = $"{INI.File()}.tmp";
                if (!File.Exists(tmpIniPath))
                {
                    try
                    {
                        File.Create(tmpIniPath).Close();
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
                if (File.Exists(tmpIniPath))
                {
                    List <string> sections = new List<string>();
                    sections.Add("History");
                    sections.Add("Host");
                    sections.Add("Settings");
                    if (Main.AppConfigs.Count > 0)
                    {
                        Main.AppConfigs.Sort();
                        sections.AddRange(Main.AppConfigs);
                    }
                    foreach (string section in sections)
                    {
                        List<string> keys = INI.GetKeys(section);
                        if (keys.Count == 0)
                            continue;
                        foreach (string key in keys)
                        {
                            string value = INI.Read(section, key);
                            if (string.IsNullOrWhiteSpace(value))
                                continue;
                            INI.Write(section, key, value, tmpIniPath);
                        }
                        File.AppendAllText(tmpIniPath, Environment.NewLine);
                    }
                    try
                    {
                        File.WriteAllText(INI.File(), File.ReadAllText(tmpIniPath));
                        File.Delete(tmpIniPath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }

            bool StartMenuIntegration = INI.ReadBoolean("Settings", "StartMenuIntegration");
            if (StartMenuIntegration)
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
                    LOG.Debug(ex);
                }
            }
        }

        private void MenuViewForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Main.SearchForUpdates();

        #endregion

        #region FUNCTIONS

        private void MenuViewForm_Update(bool setWindowLocation = true)
        {
            try
            {
                Main.CheckAvailableApps();
                appsListView.BeginUpdate();
                appsListView.Items.Clear();
                if (!appsListView.Scrollable)
                    appsListView.Scrollable = true;
                imgList.Images.Clear();
                string CacheFile = PATH.Combine("%CurDir%\\Assets\\icon.tmp");
                try
                {
                    if (!File.Exists(CacheFile))
                        File.Create(CacheFile).Close();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    if (!ELEVATION.IsAdministrator)
                        ELEVATION.RestartAsAdministrator(Main.CmdLine);
                }
                byte[] IcoDb = null;
                Image DefaultExeIcon = DRAWING.ImageFilter(RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.EXE, Main.SystemResourcePath), 16, 16);
                for (int i = 0; i < Main.AppsInfo.Count; i++)
                {
                    Main.AppInfo appInfo = Main.AppsInfo[i];
                    if (string.IsNullOrWhiteSpace(appInfo.LongName))
                        continue;
                    appsListView.Items.Add(appInfo.LongName, i);
                    string nameHash = CRYPT.MD5.EncryptString(appInfo.ShortName);
                    try
                    {
                        Image imgFromCache = INI.ReadImage("Cache", nameHash, CacheFile);
                        if (imgFromCache != null)
                        {
                            if (LOG.DebugMode > 1 && Main.CmdLineActionGuid.IsExtractCachedImage)
                            {
                                try
                                {
                                    string imgDir = PATH.Combine("%CurrntDir%\\Assets\\Images");
                                    if (!Directory.Exists(imgDir))
                                        Directory.CreateDirectory(imgDir);
                                    imgFromCache.Save(Path.Combine(imgDir, nameHash));
                                    string imgIni = Path.Combine(imgDir, "_list.ini");
                                    if (!File.Exists(imgIni))
                                        File.Create(imgIni).Close();
                                    INI.Write("list", nameHash, appInfo.ShortName, imgIni);
                                }
                                catch (Exception ex)
                                {
                                    LOG.Debug(ex);
                                }
                            }
                            imgList.Images.Add(nameHash, imgFromCache);
                        }
                        if (imgList.Images.ContainsKey(nameHash))
                            continue;

                        if (IcoDb == null)
                        {
                            try
                            {
                                IcoDb = File.ReadAllBytes(PATH.Combine("%CurDir%\\Assets\\icon.db"));
                            }
                            catch (Exception ex)
                            {
                                LOG.Debug(ex);
                            }
                        }
                        if (IcoDb != null)
                        {
                            using (MemoryStream stream = new MemoryStream(IcoDb))
                            {
                                try
                                {
                                    using (ZipArchive archive = new ZipArchive(stream))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            if (entry.Name != nameHash)
                                                continue;
                                            Image img = Image.FromStream(entry.Open());
                                            imgList.Images.Add(nameHash, img);
                                            INI.Write("Cache", nameHash, img, CacheFile);
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LOG.Debug(ex);
                                }
                            }
                        }
                        if (imgList.Images.ContainsKey(nameHash))
                            continue;

                        string appDir = Path.GetDirectoryName(appInfo.ExePath);
                        string imgPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appInfo.ExePath)}.png");
                        if (!File.Exists(imgPath))
                        {
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            if (!File.Exists(imgPath))
                                imgPath = Path.Combine(appDir, "App\\AppInfo\\appicon_16.png");
                            if (File.Exists(imgPath))
                            {
                                Image imgFromFile = DRAWING.ImageFilter(Image.FromFile(imgPath), 16, 16);
                                imgList.Images.Add(nameHash, imgFromFile);
                                INI.Write("Cache", nameHash, imgFromFile, CacheFile);
                            }
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            using (Icon ico = RESOURCE.IconFromFile(appInfo.ExePath))
                            {
                                if (ico != null)
                                {
                                    Image imgFromIcon = DRAWING.ImageFilter(ico.ToBitmap(), 16, 16);
                                    imgList.Images.Add(nameHash, imgFromIcon);
                                    INI.Write("Cache", nameHash, imgFromIcon, CacheFile);
                                    continue;
                                }
                            }
                            throw new Exception();
                        }
                        else
                        {
                            Image imgFromFile = Image.FromFile(imgPath);
                            imgList.Images.Add(nameHash, imgFromFile);
                            INI.Write("Cache", nameHash, imgFromFile, CacheFile);
                        }
                    }
                    catch
                    {
                        imgList.Images.Add(nameHash, DefaultExeIcon);
                    }
                }
                if (LOG.DebugMode > 1 && Main.CmdLineActionGuid.IsExtractCachedImage)
                    throw new Exception("Image extraction completed.");
                appsListView.SmallImageList = imgList;
                if (setWindowLocation)
                {
                    int defaultPos = INI.ReadInteger("Settings", "Window.DefaultPosition", 0);
                    TASKBAR.Location taskbarLocation = TASKBAR.GetLocation(Handle);
                    if (defaultPos == 0 && taskbarLocation != TASKBAR.Location.HIDDEN)
                    {
                        switch (taskbarLocation)
                        {
                            case TASKBAR.Location.LEFT:
                                Left = Screen.FromHandle(Handle).WorkingArea.X;
                                Top = 0;
                                break;
                            case TASKBAR.Location.TOP:
                                Left = 0;
                                Top = Screen.FromHandle(Handle).WorkingArea.Y;
                                break;
                            case TASKBAR.Location.RIGHT:
                                Left = Screen.FromHandle(Handle).WorkingArea.Width - Width;
                                Top = 0;
                                break;
                            default:
                                Left = 0;
                                Top = Screen.FromHandle(Handle).WorkingArea.Height - Height;
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
                appsCount.Text = string.Format(Lang.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"));

            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }

        private Point GetWindowStartPos(Point point)
        {
            Point pos = new Point();
            var tbLoc = TASKBAR.GetLocation(Handle);
            int tbSize = TASKBAR.GetSize(Handle);
            if (INI.ReadInteger("Settings", "Window.DefaultPosition", 0) == 0)
            {
                switch (tbLoc)
                {
                    case TASKBAR.Location.LEFT:
                        pos.X = Cursor.Position.X - (point.X / 2);
                        pos.Y = Cursor.Position.Y;
                        break;
                    case TASKBAR.Location.TOP:
                        pos.X = Cursor.Position.X - (point.X / 2);
                        pos.Y = Cursor.Position.Y;
                        break;
                    case TASKBAR.Location.RIGHT:
                        pos.X = Screen.FromHandle(Handle).WorkingArea.Width - point.X;
                        pos.Y = Cursor.Position.Y;
                        break;
                    default:
                        pos.X = Cursor.Position.X - (point.X / 2);
                        pos.Y = Cursor.Position.Y - point.Y;
                        break;
                }
                if (pos.X + point.X > Screen.FromHandle(Handle).WorkingArea.Width)
                    pos.X = Screen.FromHandle(Handle).WorkingArea.Width - point.X;
                if (pos.Y + point.Y > Screen.FromHandle(Handle).WorkingArea.Height)
                    pos.Y = Screen.FromHandle(Handle).WorkingArea.Height - point.Y;
            }
            else
            {
                Point max = new Point(Screen.FromHandle(Handle).WorkingArea.Width - point.X, Screen.FromHandle(Handle).WorkingArea.Height - point.Y);
                pos.X = Cursor.Position.X > point.X / 2 && Cursor.Position.X < max.X ? Cursor.Position.X - point.X / 2 : Cursor.Position.X > max.X ? max.X : Cursor.Position.X;
                pos.Y = Cursor.Position.Y > point.Y / 2 && Cursor.Position.Y < max.Y ? Cursor.Position.Y - point.Y / 2 : Cursor.Position.Y > max.Y ? max.Y : Cursor.Position.Y;
            }
            Point min = new Point(tbLoc == TASKBAR.Location.LEFT ? tbSize : 0, tbLoc == TASKBAR.Location.TOP ? tbSize : 0);
            if (pos.X < min.X)
                pos.X = min.X;
            if (pos.Y < min.Y)
                pos.Y = min.Y;
            return pos;
        }

        private void fadeInTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity < WindowOpacity)
                Opacity += WindowOpacity / WindowFadeInDuration;
            else
            {
                ((Timer)sender).Enabled = false;
                if (WINAPI.SafeNativeMethods.GetForegroundWindow() != Handle)
                    WINAPI.SafeNativeMethods.SetForegroundWindow(Handle);
            }
        }
        private void sizeGrip_MouseDown(object sender, MouseEventArgs e)
        {
            Point point;
            switch (TASKBAR.GetLocation(Handle))
            {
                case TASKBAR.Location.RIGHT:
                    point = new Point(1, Height - 1);
                    break;
                case TASKBAR.Location.BOTTOM:
                    point = new Point(Width - 1, 1);
                    break;
                default:
                    point = new Point(Width - 1, Height - 1);
                    break;
            }

            WINAPI.SafeNativeMethods.ClientToScreen(Handle, ref point);
            WINAPI.SafeNativeMethods.SetCursorPos((uint)point.X, (uint)point.Y);

            WINAPI.INPUT inputMouseDown = new WINAPI.INPUT();
            inputMouseDown.Data.Mouse.Flags = 0x0002;
            inputMouseDown.Type = 0;

            WINAPI.INPUT inputMouseUp = new WINAPI.INPUT();
            inputMouseUp.Data.Mouse.Flags = 0x0004;
            inputMouseUp.Type = 0;

            WINAPI.INPUT[] inputs = new WINAPI.INPUT[]
            {
                inputMouseUp,
                inputMouseDown
            };

            WINAPI.SafeNativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(WINAPI.INPUT)));
        }

        private void sizeGrip_MouseEnter(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            switch (TASKBAR.GetLocation(Handle))
            {
                case TASKBAR.Location.RIGHT:
                case TASKBAR.Location.BOTTOM:
                    p.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    p.Cursor = Cursors.SizeNWSE;
                    break;
            }
        }

        #endregion

        #region APPS LIST

        private void appsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            ListView lv = (ListView)sender;
            if (lv.SelectedItems.Count > 0)
            {
                AppStartEventCalled = true;
                if (Opacity != 0)
                    Opacity = 0;
                Main.StartApp(lv.SelectedItems[0].Text, true);
            }
        }

        private void appsListView_MouseEnter(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            if (!lv.LabelEdit && !lv.Focus())
                lv.Select();
        }

        private void appsListView_MouseLeave(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            if (lv.Focus())
                lv.Parent.Select();
        }

        private void appsListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView lv = (ListView)sender;
            if (lv.LabelEdit)
                return;
            ListViewItem lvi = LISTVIEW.ItemFromPoint(lv);
            if (lvi != null && appsListViewCursorLocation != Cursor.Position)
            {
                lvi.Selected = true;
                appsListViewCursorLocation = Cursor.Position;
            }
        }

        private void appsListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back:
                    if (searchBox.Text.Length >= 1)
                    {
                        if (!searchBox.Focus())
                            searchBox.Select();
                        searchBox.Text = searchBox.Text.Substring(0, searchBox.Text.Length - 1);
                        searchBox.SelectionStart = searchBox.TextLength;
                        searchBox.ScrollToCaret();
                        e.Handled = true;
                    }
                    break;
                case Keys.ControlKey:
                    if (!e.Shift)
                    {
                        if (!searchBox.Focus())
                            searchBox.Select();
                        e.Handled = true;
                    }
                    break;
                case Keys.Delete:
                    appMenuItem_Click(appMenuItem7, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    appMenuItem_Click(!e.Control && !e.Shift ? appMenuItem1 : appMenuItem2, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.F2:
                    appMenuItem_Click(appMenuItem6, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.Space:
                    if (!searchBox.Focus())
                        searchBox.Select();
                    searchBox.Text += " ";
                    searchBox.SelectionStart = searchBox.TextLength;
                    searchBox.ScrollToCaret();
                    e.Handled = true;
                    break;
                default:
                    if (char.IsLetterOrDigit((char)e.KeyCode))
                    {
                        if (!searchBox.Focus())
                            searchBox.Select();
                        string key = Enum.GetName(typeof(Keys), e.KeyCode).ToLower();
                        searchBox.Text += key[key.Length - 1];
                        searchBox.SelectionStart = searchBox.TextLength;
                        searchBox.ScrollToCaret();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void appsListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))
                e.Handled = true;
        }

        private void appsListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListView lv = (ListView)sender;
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                try
                {
                    Main.AppInfo appInfo = Main.GetAppInfo(lv.SelectedItems[0].Text);
                    if (appInfo.LongName != lv.SelectedItems[0].Text)
                        throw new ArgumentNullException();
                    string iniPath = Path.Combine(Path.GetDirectoryName(appInfo.ExePath), $"{Path.GetFileName(Path.GetDirectoryName(appInfo.ExePath))}.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    INI.Write("AppInfo", "Name", e.Label, iniPath);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
                MenuViewForm_Update();
            }
            if (lv.LabelEdit)
                lv.LabelEdit = false;
        }

        #endregion

        #region APP MENU

        private void appMenu_Opening(object sender, CancelEventArgs e) =>
            e.Cancel = appsListView.SelectedItems.Count == 0;

        private void appMenu_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)sender;
            cms.Left -= 48;
            cms.Top -= 10;
        }

        private void appMenu_Paint(object sender, PaintEventArgs e) =>
            CONTEXTMENUSTRIP.SetFixedSingle((ContextMenuStrip)sender, e, Main.Colors.Base);

        private void appMenuItem_Click(object sender, EventArgs e)
        {
            if (appsListView.SelectedItems.Count == 0)
                return;
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            switch (tsmi.Name)
            {
                case "appMenuItem1":
                case "appMenuItem2":
                case "appMenuItem3":
                    if (Opacity != 0)
                        Opacity = 0;
                    switch (tsmi.Name)
                    {
                        case "appMenuItem1":
                        case "appMenuItem2":
                            AppStartEventCalled = true;
                            Main.StartApp(appsListView.SelectedItems[0].Text, true, tsmi.Name == "appMenuItem2");
                            break;
                        case "appMenuItem3":
                            AppStartEventCalled = true;
                            Main.OpenAppLocation(appsListView.SelectedItems[0].Text, true);
                            break;
                    }
                    break;
                case "appMenuItem4":
                    MSGBOX.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (DATA.CreateShortcut(Main.GetAppPath(appsListView.SelectedItems[0].Text), Path.Combine("%DesktopDir%", appsListView.SelectedItems[0].Text)))
                        MSGBOX.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MSGBOX.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    MSGBOX.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                    string appPath = Main.GetAppPath(appsListView.SelectedItems[0].Text);
                    if (DATA.PinToTaskbar(appPath))
                        MSGBOX.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MSGBOX.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem6":
                    if (appsListView.SelectedItems.Count > 0)
                    {
                        if (!appsListView.LabelEdit)
                            appsListView.LabelEdit = true;
                        appsListView.SelectedItems[0].BeginEdit();
                    }
                    break;
                case "appMenuItem7":
                    MSGBOX.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (MSGBOX.Show(this, string.Format(Lang.GetText("appMenuItem7Msg"), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MSGBOX.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            string appDir = Path.GetDirectoryName(Main.GetAppPath(appsListView.SelectedItems[0].Text));
                            if (Directory.Exists(appDir))
                            {
                                Directory.Delete(appDir, true);
                                MenuViewForm_Update(false);
                                MSGBOX.Show(this, Lang.GetText("OperationCompletedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            MSGBOX.Show(this, Lang.GetText("OperationFailedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            LOG.Debug(ex);
                        }
                    }
                    else
                    {
                        MSGBOX.MoveCursorToMsgBoxAtOwner = !ClientRectangle.Contains(PointToClient(MousePosition));
                        MSGBOX.Show(this, Lang.GetText("OperationCanceledMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
            }
            if (MSGBOX.MoveCursorToMsgBoxAtOwner)
                MSGBOX.MoveCursorToMsgBoxAtOwner = false;
        }

        private void appMenu_MouseLeave(object sender, EventArgs e) =>
            appMenu.Close();

        #endregion

        #region SEARCH BOX

        private void searchBox_Enter(object sender, EventArgs e)
        {
            appsListViewCursorLocation = Cursor.Position;
            TextBox tb = (TextBox)sender;
            tb.Font = new Font("Segoe UI", tb.Font.Size);
            tb.ForeColor = Main.Colors.ControlText;
            tb.Text = SearchText;
            appsListView.HideSelection = true;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Color c = Main.Colors.ControlText;
            tb.Font = new Font("Comic Sans MS", tb.Font.Size, FontStyle.Italic);
            tb.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            SearchText = tb.Text;
            tb.Text = Lang.GetText(tb);
            appsListView.HideSelection = false;
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                    if (!appsListView.Focus())
                        appsListView.Select();
                    SendKeys.SendWait($"{{{Enum.GetName(typeof(Keys), e.KeyCode).ToUpper()}}}");
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    appMenuItem_Click(!e.Control && !e.Shift ? appMenuItem1 : appMenuItem2, EventArgs.Empty);
                    e.Handled = true;
                    break;
            }
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            List<string> itemList = new List<string>();
            foreach (ListViewItem item in appsListView.Items)
            {
                item.ForeColor = Main.Colors.ControlText;
                item.BackColor = Main.Colors.Control;
                itemList.Add(item.Text);
            }
            if (string.IsNullOrWhiteSpace(tb.Text) || tb.Font.Italic)
                return;
            foreach (ListViewItem item in appsListView.Items)
            {
                if (item.Text == Main.SearchMatchItem(tb.Text, itemList))
                {
                    item.ForeColor = SystemColors.Control;
                    item.BackColor = SystemColors.HotTrack;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            }
            appsListViewCursorLocation = Cursor.Position;
        }

        #endregion

        #region BUTTONS

        private void ImageButton_MouseEnterLeave(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                Button b = (Button)sender;
                if (b.BackgroundImage != null)
                    b.BackgroundImage = DRAWING.ImageGrayScaleSwitch($"{b.Name}BackgroundImage", b.BackgroundImage);
                if (b.Image != null)
                    b.Image = DRAWING.ImageGrayScaleSwitch($"{b.Name}Image", b.Image);
                return;
            }
            if (sender is PictureBox)
            {
                PictureBox pb = (PictureBox)sender;
                if (pb.BackgroundImage != null)
                    pb.BackgroundImage = DRAWING.ImageGrayScaleSwitch($"{pb.Name}BackgroundImage", pb.BackgroundImage);
                if (pb.Image != null)
                    pb.Image = DRAWING.ImageGrayScaleSwitch($"{pb.Name}Image", pb.Image);
            }
        }

        private void openNewFormBtn_Click(object sender, EventArgs e)
        {
            Form form;
            if (sender is Button)
                form = new SettingsForm(string.Empty);
            else if (sender is PictureBox)
                form = new AboutForm();
            else
                return;
            if (TopMost)
                TopMost = false;
            bool result = false;
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
                    result = dialog.ShowDialog() == DialogResult.Yes;
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            if (result && sender is Button)
            {
                RUN.App(new ProcessStartInfo()
                {
                    Arguments = Main.CmdLineActionGuid.AllowNewInstance,
                    FileName = LOG.AssemblyPath
                });
                Close();
            }
            else
            {
                if (!TopMost)
                    TopMost = true;
                if (WINAPI.SafeNativeMethods.GetForegroundWindow() != Handle)
                    WINAPI.SafeNativeMethods.SetForegroundWindow(Handle);
            }
        }

        private void profileBtn_Click(object sender, EventArgs e)
        {
            RUN.App(new ProcessStartInfo()
            {
                Arguments = PATH.Combine("%CurDir%\\Documents"),
                FileName = "%WinDir%\\explorer.exe"
            });
            Close();
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            Main.SkipUpdateSearch = true;
#if x86
            RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Binaries\\AppsDownloader.exe" });
#else
            RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Binaries\\AppsDownloader64.exe" });
#endif
            Close();
        }

        private void closeBtn_Click(object sender, EventArgs e) =>
            Application.Exit();

        #endregion
    }
}
