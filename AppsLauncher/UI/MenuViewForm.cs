namespace AppsLauncher.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MenuViewForm : Form
    {
        private Point _appsListViewCursorLocation;
        private bool _appStartEventCalled;
        private bool _hideHScrollBar;
        private string _searchText = string.Empty;
        private int _windowFadeInDuration;
        private double _windowOpacity;

        public MenuViewForm()
        {
            InitializeComponent();

            layoutPanel.BackgroundImage = Main.BackgroundImage;
            layoutPanel.BackgroundImageLayout = Main.BackgroundImageLayout;
            layoutPanel.BackColor = Main.Colors.Base;
            layoutPanel.SetControlStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint);

            if (Main.ScreenDpi > 96)
                appsListViewPanel.Font = SystemFonts.SmallCaptionFont;
            appsListViewPanel.BackColor = Main.Colors.Control;
            appsListViewPanel.ForeColor = Main.Colors.ControlText;
            appsListView.BackColor = appsListViewPanel.BackColor;
            appsListView.ForeColor = appsListViewPanel.ForeColor;
            appsListView.SetControlStyle(ControlStyles.OptimizedDoubleBuffer);

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            searchBox.DrawSearchSymbol(Main.Colors.ControlText);

            title.ForeColor = Main.Colors.Base.InvertRgb().ToGrayScale();
            logoBox.Image = Resources.PortableApps_Logo_gray.Redraw(logoBox.Height, logoBox.Height);
            appsCount.ForeColor = title.ForeColor;

            aboutBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Help, Main.SystemResourcePath)?.ToBitmap();
            aboutBtn.BackgroundImage = aboutBtn.BackgroundImage.SwitchGrayScale($"{aboutBtn.Name}BackgroundImage");

            profileBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.UserDir, true, Main.SystemResourcePath)?.ToBitmap();
            downloadBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Network, Main.SystemResourcePath)?.ToBitmap();
            settingsBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.SystemControl, Main.SystemResourcePath)?.ToBitmap();
            foreach (var btn in new[] { downloadBtn, settingsBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appMenuItem2.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem3.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem5.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Pin, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem7.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.RecycleBinEmpty, Main.SystemResourcePath)?.ToBitmap();

            ControlEx.DrawSizeGrip(sizeGrip, Main.Colors.Base);
        }

        private void MenuViewForm_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(255, Main.Colors.Base.R, Main.Colors.Base.G, Main.Colors.Base.B);

            if (Main.ScreenDpi > 96)
                Font = SystemFonts.CaptionFont;
            Icon = Resources.PortableApps_blue;

            MaximumSize = Screen.FromHandle(Handle).WorkingArea.Size;

            Lang.SetControlLang(this);
            for (var i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Lang.GetText(appMenu.Items[i].Name);
            if (Main.SetFont(this))
                layoutPanel.Size = new Size(Width - 2, Height - 2);
            Main.SetFont(appMenu);

            var docDir = PathEx.Combine(PathEx.LocalDir, "Documents");
            if (Directory.Exists(docDir) && Data.DirIsLink(docDir) && !Data.MatchAttributes(docDir, FileAttributes.Hidden))
                Data.SetAttributes(docDir, FileAttributes.Hidden);

            _windowOpacity = Ini.ReadDouble("Settings", "Window.Opacity", 95);
            if (_windowOpacity.IsBetween(20d, 100d))
                _windowOpacity /= 100d;
            else
                _windowOpacity = .95d;

            _windowFadeInDuration = Ini.ReadInteger("Settings", "Window.FadeInDuration", 1);
            if (_windowFadeInDuration < 1)
                _windowFadeInDuration = 1;
            var opacity = (int)(_windowOpacity * 100d);
            if (_windowFadeInDuration > opacity)
                _windowFadeInDuration = opacity;

            var windowWidth = Ini.ReadInteger("Settings", "Window.Size.Width", MinimumSize.Width);
            if (windowWidth > MinimumSize.Width && windowWidth < MaximumSize.Width)
                Width = windowWidth;
            if (windowWidth > MaximumSize.Width)
                Width = MaximumSize.Width;

            var windowHeight = Ini.ReadInteger("Settings", "Window.Size.Height", MinimumSize.Height);
            if (windowHeight > MinimumSize.Height && windowHeight < MaximumSize.Height)
                Height = windowHeight;
            if (windowHeight > MaximumSize.Height)
                Height = MaximumSize.Height;

            WinApi.UnsafeNativeMethods.SendMessage(appsListView.Handle, 0x103e, IntPtr.Zero, Cursors.Arrow.Handle); // !!!uncomment!!!

            _hideHScrollBar = Ini.ReadBoolean("Settings", "Window.HideHScrollBar");

            MenuViewForm_Resize(null, EventArgs.Empty);

            if (Log.DebugMode > 1)
                closeBtn.Visible = true;

            MenuViewForm_Update();

            if (!searchBox.Focus())
                searchBox.Select();

            if (!fadeInTimer.Enabled)
                fadeInTimer.Enabled = true;
        }

        private void MenuViewForm_Deactivate(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count > 1 || appMenu.Focus() || _appStartEventCalled)
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
            Ini.Write("Settings", "Window.Size.Width", Width, false);
            Ini.Write("Settings", "Window.Size.Height", Height, false);
        }

        private void MenuViewForm_Resize(object sender, EventArgs e)
        {
            if (!_hideHScrollBar)
                return;
            if (appsListView.Dock != DockStyle.None)
                appsListView.Dock = DockStyle.None;
            var padding = (int)Math.Floor(SystemInformation.HorizontalScrollBarHeight / 3f);
            appsListView.Location = new Point(padding, padding);
            appsListView.Size = appsListViewPanel.Size;
            appsListView.Region = new Region(new RectangleF(0, 0, appsListViewPanel.Width - padding, appsListViewPanel.Height - SystemInformation.HorizontalScrollBarHeight));
        }

        private void MenuViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _appStartEventCalled = true;
            if (Opacity > 0)
                Opacity = 0;
            string curIni;
            if (File.Exists(Ini.File()) && !string.IsNullOrWhiteSpace(curIni = File.ReadAllText(Ini.File())))
            {
                string tmpIni = null;
                try
                {
                    tmpIni = Path.GetTempFileName();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                if (File.Exists(tmpIni))
                {
                    try
                    {
                        var sections = new List<string>
                        {
                            "History",
                            "Host",
                            "Settings"
                        };
                        if (Main.AppConfigs.Count > 0)
                        {
                            Main.AppConfigs.Sort();
                            sections.AddRange(Main.AppConfigs);
                        }
                        foreach (var section in sections)
                        {
                            var keys = Ini.GetKeys(section);
                            if (keys.Count == 0)
                                continue;
                            foreach (var key in keys)
                            {
                                var value = Ini.Read(section, key);
                                if (string.IsNullOrWhiteSpace(value))
                                    continue;
                                Ini.Write(section, key, value, tmpIni);
                            }
                            if (!string.IsNullOrEmpty(tmpIni))
                                File.AppendAllText(tmpIni, Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(tmpIni))
                        {
                            var newIni = File.ReadAllText(tmpIni);
                            if (!string.IsNullOrWhiteSpace(newIni) && curIni != newIni)
                                File.WriteAllText(Ini.File(), newIni);
                            File.Delete(tmpIni);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
            }
            var startMenuIntegration = Ini.ReadBoolean("Settings", "StartMenuIntegration");
            if (!startMenuIntegration)
                return;
            try
            {
                var list = new List<string>();
                for (var i = 0; i < appsListView.Items.Count; i++)
                    list.Add(appsListView.Items[i].Text);
                Main.StartMenuFolderUpdate(list);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void MenuViewForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Main.SearchForUpdates();

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
                byte[] icoDb = null;
                var defExeIcon = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.ExeFile, Main.SystemResourcePath)?.ToBitmap().Redraw(16, 16);
                for (var i = 0; i < Main.AppsInfo.Count; i++)
                {
                    var appInfo = Main.AppsInfo[i];
                    if (string.IsNullOrWhiteSpace(appInfo.LongName))
                        continue;
                    appsListView.Items.Add(appInfo.LongName, i);
                    var nameHash = appInfo.ShortName.EncryptToMd5();
                    try
                    {
                        var imgFromCache = Ini.ReadImage("Cache", nameHash, Main.IconCachePath);
                        if (imgFromCache != null)
                        {
                            if (Log.DebugMode > 1 && Main.ActionGuid.IsExtractCachedImage)
                                try
                                {
                                    var imgDir = Path.Combine(Main.TmpDir, "images");
                                    if (!Directory.Exists(imgDir))
                                        Directory.CreateDirectory(imgDir);
                                    imgFromCache.Save(Path.Combine(imgDir, nameHash));
                                    var imgIni = Path.Combine(imgDir, "_images.ini");
                                    if (!File.Exists(imgIni))
                                        File.Create(imgIni).Close();
                                    Ini.Write("images", nameHash, appInfo.ShortName, imgIni);
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                            imgList.Images.Add(nameHash, imgFromCache);
                        }
                        if (imgList.Images.ContainsKey(nameHash))
                            continue;
                        if (icoDb == null)
                            try
                            {
                                icoDb = File.ReadAllBytes(PathEx.Combine(PathEx.LocalDir, "Assets\\icon.db"));
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        if (icoDb != null)
                            using (var stream = new MemoryStream(icoDb))
                                try
                                {
                                    using (var archive = new ZipArchive(stream))
                                        foreach (var entry in archive.Entries)
                                        {
                                            if (entry.Name != nameHash)
                                                continue;
                                            var img = Image.FromStream(entry.Open());
                                            imgList.Images.Add(nameHash, img);
                                            Ini.Write("Cache", nameHash, img, Main.IconCachePath);
                                            break;
                                        }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                        if (imgList.Images.ContainsKey(nameHash))
                            continue;
                        var appDir = Path.GetDirectoryName(appInfo.ExePath);
                        var imgPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appInfo.ExePath)}.png");
                        Image imgFromFile;
                        if (!File.Exists(imgPath))
                        {
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            if (!File.Exists(imgPath))
                                imgPath = Path.Combine(appDir, "App\\AppInfo\\appicon_16.png");
                            if (File.Exists(imgPath))
                            {
                                imgFromFile = Image.FromFile(imgPath).Redraw(16, 16);
                                imgList.Images.Add(nameHash, imgFromFile);
                                Ini.Write("Cache", nameHash, imgFromFile, Main.IconCachePath);
                            }
                            if (imgList.Images.ContainsKey(nameHash))
                                continue;
                            using (var ico = ResourcesEx.GetIconFromFile(appInfo.ExePath))
                                if (ico != null)
                                {
                                    var imgFromIcon = ico?.ToBitmap().Redraw(16, 16);
                                    if (imgFromIcon == null)
                                        throw new ArgumentNullException();
                                    imgList.Images.Add(nameHash, imgFromIcon);
                                    Ini.Write("Cache", nameHash, imgFromIcon, Main.IconCachePath);
                                    continue;
                                }
                            throw new Exception();
                        }
                        imgFromFile = Image.FromFile(imgPath);
                        imgList.Images.Add(nameHash, imgFromFile);
                        Ini.Write("Cache", nameHash, imgFromFile, Main.IconCachePath);
                    }
                    catch
                    {
                        if (defExeIcon == null)
                            continue;
                        imgList.Images.Add(nameHash, defExeIcon);
                    }
                }
                if (Log.DebugMode > 1 && Main.ActionGuid.IsExtractCachedImage)
                    throw new Exception("Image extraction completed.");
                appsListView.SmallImageList = imgList;
                if (setWindowLocation)
                {
                    var defaultPos = Ini.ReadInteger("Settings", "Window.DefaultPosition");
                    var taskbarLocation = TaskBar.GetLocation(Handle);
                    if (defaultPos == 0 && taskbarLocation != TaskBar.Location.Hidden)
                    {
                        switch (taskbarLocation)
                        {
                            case TaskBar.Location.Left:
                                Left = Screen.FromHandle(Handle).WorkingArea.X;
                                Top = 0;
                                break;
                            case TaskBar.Location.Top:
                                Left = 0;
                                Top = Screen.FromHandle(Handle).WorkingArea.Y;
                                break;
                            case TaskBar.Location.Right:
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
                        var newLocation = GetWindowStartPos(new Point(Width, Height));
                        Left = newLocation.X;
                        Top = newLocation.Y;
                    }
                }
                appsListView.EndUpdate();
                appsCount.Text = string.Format(Lang.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }

        private Point GetWindowStartPos(Point point)
        {
            var pos = new Point();
            var tbLoc = TaskBar.GetLocation(Handle);
            var tbSize = TaskBar.GetSize(Handle);
            if (Ini.ReadInteger("Settings", "Window.DefaultPosition") == 0)
            {
                switch (tbLoc)
                {
                    case TaskBar.Location.Left:
                        pos.X = Cursor.Position.X - point.X / 2;
                        pos.Y = Cursor.Position.Y;
                        break;
                    case TaskBar.Location.Top:
                        pos.X = Cursor.Position.X - point.X / 2;
                        pos.Y = Cursor.Position.Y;
                        break;
                    case TaskBar.Location.Right:
                        pos.X = Screen.FromHandle(Handle).WorkingArea.Width - point.X;
                        pos.Y = Cursor.Position.Y;
                        break;
                    default:
                        pos.X = Cursor.Position.X - point.X / 2;
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
                var max = new Point(Screen.FromHandle(Handle).WorkingArea.Width - point.X, Screen.FromHandle(Handle).WorkingArea.Height - point.Y);
                pos.X = Cursor.Position.X > point.X / 2 && Cursor.Position.X < max.X ? Cursor.Position.X - point.X / 2 : Cursor.Position.X > max.X ? max.X : Cursor.Position.X;
                pos.Y = Cursor.Position.Y > point.Y / 2 && Cursor.Position.Y < max.Y ? Cursor.Position.Y - point.Y / 2 : Cursor.Position.Y > max.Y ? max.Y : Cursor.Position.Y;
            }
            var min = new Point(tbLoc == TaskBar.Location.Left ? tbSize : 0, tbLoc == TaskBar.Location.Top ? tbSize : 0);
            if (pos.X < min.X)
                pos.X = min.X;
            if (pos.Y < min.Y)
                pos.Y = min.Y;
            return pos;
        }

        private void FadeInTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity < _windowOpacity)
                Opacity += _windowOpacity / _windowFadeInDuration;
            else
            {
                ((Timer)sender).Enabled = false;
                if (WinApi.UnsafeNativeMethods.GetForegroundWindow() != Handle)
                    WinApi.UnsafeNativeMethods.SetForegroundWindow(Handle);
            }
        }

        private void SizeGrip_MouseDown(object sender, MouseEventArgs e)
        {
            Point point;
            switch (TaskBar.GetLocation(Handle))
            {
                case TaskBar.Location.Right:
                    point = new Point(1, Height - 1);
                    break;
                case TaskBar.Location.Bottom:
                    point = new Point(Width - 1, 1);
                    break;
                default:
                    point = new Point(Width - 1, Height - 1);
                    break;
            }
            WinApi.UnsafeNativeMethods.ClientToScreen(Handle, ref point);
            WinApi.UnsafeNativeMethods.SetCursorPos((uint)point.X, (uint)point.Y);
            var inputMouseDown = new WinApi.INPUT();
            inputMouseDown.Data.Mouse.Flags = 0x0002;
            inputMouseDown.Type = 0;
            var inputMouseUp = new WinApi.INPUT();
            inputMouseUp.Data.Mouse.Flags = 0x0004;
            inputMouseUp.Type = 0;
            WinApi.INPUT[] inputs =
            {
                inputMouseUp,
                inputMouseDown
            };
            WinApi.UnsafeNativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(WinApi.INPUT)));
        }

        private void SizeGrip_MouseEnter(object sender, EventArgs e)
        {
            var p = (Panel)sender;
            switch (TaskBar.GetLocation(Handle))
            {
                case TaskBar.Location.Right:
                case TaskBar.Location.Bottom:
                    p.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    p.Cursor = Cursors.SizeNWSE;
                    break;
            }
        }

        private void AppsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            var lv = (ListView)sender;
            if (lv.SelectedItems.Count <= 0)
                return;
            _appStartEventCalled = true;
            if (Opacity > 0)
                Opacity = 0;
            Main.StartApp(lv.SelectedItems[0].Text, true);
        }

        private void AppsListView_MouseEnter(object sender, EventArgs e)
        {
            var lv = (ListView)sender;
            if (!lv.LabelEdit && !lv.Focus())
                lv.Select();
        }

        private void AppsListView_MouseLeave(object sender, EventArgs e)
        {
            var lv = (ListView)sender;
            if (lv.Focus())
                lv.Parent.Select();
        }

        private void AppsListView_MouseMove(object sender, MouseEventArgs e)
        {
            var lv = (ListView)sender;
            if (lv.LabelEdit)
                return;
            var lvi = lv.ItemFromPoint();
            if (lvi != null && _appsListViewCursorLocation != Cursor.Position)
            {
                lvi.Selected = true;
                _appsListViewCursorLocation = Cursor.Position;
            }
        }

        private void AppsListView_KeyDown(object sender, KeyEventArgs e)
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
                    AppMenuItem_Click(appMenuItem7, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    AppMenuItem_Click(!e.Control && !e.Shift ? appMenuItem1 : appMenuItem2, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.F2:
                    AppMenuItem_Click(appMenuItem6, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.Space:
                    if (!searchBox.Focus())
                        searchBox.Select();
                    searchBox.Text += @" ";
                    searchBox.SelectionStart = searchBox.TextLength;
                    searchBox.ScrollToCaret();
                    e.Handled = true;
                    break;
                default:
                    if (char.IsLetterOrDigit((char)e.KeyCode))
                    {
                        if (!searchBox.Focus())
                            searchBox.Select();
                        var key = Enum.GetName(typeof(Keys), e.KeyCode)?.ToLower();
                        searchBox.Text += key?[key.Length - 1];
                        searchBox.SelectionStart = searchBox.TextLength;
                        searchBox.ScrollToCaret();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void AppsListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))
                e.Handled = true;
        }

        private void AppsListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            var lv = (ListView)sender;
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                try
                {
                    var appInfo = Main.GetAppInfo(lv.SelectedItems[0].Text);
                    if (appInfo.LongName != lv.SelectedItems[0].Text)
                        throw new ArgumentNullException();
                    var exeDir = Path.GetDirectoryName(appInfo.ExePath);
                    if (string.IsNullOrEmpty(exeDir))
                        return;
                    var exeDirName = Path.GetFileName(exeDir);
                    if (string.IsNullOrEmpty(exeDirName))
                        return;
                    var iniPath = Path.Combine(exeDir, $"{exeDirName}.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    Ini.Write("AppInfo", "Name", e.Label, iniPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                MenuViewForm_Update();
            }
            if (lv.LabelEdit)
                lv.LabelEdit = false;
        }

        private void AppMenu_Opening(object sender, CancelEventArgs e) =>
            e.Cancel = appsListView.SelectedItems.Count == 0;

        private void AppMenu_Opened(object sender, EventArgs e)
        {
            var cms = (ContextMenuStrip)sender;
            cms.Left -= 48;
            cms.Top -= 10;
        }

        private void AppMenu_Paint(object sender, PaintEventArgs e) =>
            ((ContextMenuStrip)sender).SetFixedSingle(e, Main.Colors.Base);

        private void AppMenuItem_Click(object sender, EventArgs e)
        {
            if (appsListView.SelectedItems.Count == 0)
                return;
            var tsmi = (ToolStripMenuItem)sender;
            switch (tsmi.Name)
            {
                case "appMenuItem1":
                case "appMenuItem2":
                case "appMenuItem3":
                    if (Opacity > 0)
                        Opacity = 0;
                    switch (tsmi.Name)
                    {
                        case "appMenuItem1":
                        case "appMenuItem2":
                            _appStartEventCalled = true;
                            Main.StartApp(appsListView.SelectedItems[0].Text, true, tsmi.Name == "appMenuItem2");
                            break;
                        case "appMenuItem3":
                            _appStartEventCalled = true;
                            Main.OpenAppLocation(appsListView.SelectedItems[0].Text, true);
                            break;
                    }
                    break;
                case "appMenuItem4":
                    MsgBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (Data.CreateShortcut(Main.GetEnvironmentVariablePath(Main.GetAppPath(appsListView.SelectedItems[0].Text)), Path.Combine("%Desktop%", appsListView.SelectedItems[0].Text)))
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    MsgBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    var appPath = Main.GetAppPath(appsListView.SelectedItems[0].Text);
                    if (Data.PinToTaskbar(appPath))
                    {
                        if (!string.IsNullOrWhiteSpace(EnvironmentEx.GetVariableValue("AppsSuiteDir")))
                        {
                            var pinnedDir = PathEx.Combine("%AppData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
                            foreach (var file in Directory.GetFiles(pinnedDir, "*.lnk", SearchOption.TopDirectoryOnly))
                            {
                                if (appPath.EqualsEx(Data.GetShortcutTarget(file)))
                                    continue;
                                using (var p = ProcessEx.Send($"DEL /F /Q \"{file}\"", false, false))
                                    if (p != null && !p.HasExited)
                                        p.WaitForExit(1000);
                                Data.CreateShortcut(Main.GetEnvironmentVariablePath(appPath), file);
                                break;
                            }
                        }
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                        MsgBoxEx.Show(this, Lang.GetText("appMenuItem4Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MsgBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (MsgBoxEx.Show(this, string.Format(Lang.GetText("appMenuItem7Msg"), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MsgBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appDir = Path.GetDirectoryName(Main.GetAppPath(appsListView.SelectedItems[0].Text));
                            if (Directory.Exists(appDir))
                            {
                                Directory.Delete(appDir, true);
                                MenuViewForm_Update(false);
                                MsgBoxEx.Show(this, Lang.GetText("OperationCompletedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (Exception ex)
                        {
                            MsgBoxEx.Show(this, Lang.GetText("OperationFailedMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Log.Write(ex);
                        }
                    }
                    else
                    {
                        MsgBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        MsgBoxEx.Show(this, Lang.GetText("OperationCanceledMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
            }
            if (MsgBoxEx.CenterMousePointer)
                MsgBoxEx.CenterMousePointer = false;
        }

        private void AppMenu_MouseLeave(object sender, EventArgs e) =>
            appMenu.Close();

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            _appsListViewCursorLocation = Cursor.Position;
            var tb = (TextBox)sender;
            tb.Font = new Font("Segoe UI", tb.Font.Size);
            tb.ForeColor = Main.Colors.ControlText;
            tb.Text = _searchText;
            appsListView.HideSelection = true;
        }

        private void SearchBox_Leave(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            var c = Main.Colors.ControlText;
            tb.Font = new Font("Comic Sans MS", tb.Font.Size, FontStyle.Italic);
            tb.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = tb.Text;
            tb.Text = Lang.GetText(tb);
            appsListView.HideSelection = false;
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                    if (!appsListView.Focus())
                        appsListView.Select();
                    SendKeys.SendWait($"{{{Enum.GetName(typeof(Keys), e.KeyCode)?.ToUpper()}}}");
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    AppMenuItem_Click(!e.Control && !e.Shift ? appMenuItem1 : appMenuItem2, EventArgs.Empty);
                    e.Handled = true;
                    break;
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            var itemList = new List<string>();
            foreach (ListViewItem item in appsListView.Items)
            {
                item.ForeColor = Main.Colors.ControlText;
                item.BackColor = Main.Colors.Control;
                itemList.Add(item.Text);
            }
            if (string.IsNullOrWhiteSpace(tb.Text) || tb.Font.Italic)
                return;
            foreach (ListViewItem item in appsListView.Items)
                if (item.Text == Main.SearchMatchItem(tb.Text, itemList))
                {
                    item.ForeColor = SystemColors.Control;
                    item.BackColor = SystemColors.HotTrack;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            _appsListViewCursorLocation = Cursor.Position;
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        [SuppressMessage("ReSharper", "UseNullPropagationWhenPossible")]
        private void ImageButton_MouseEnterLeave(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                var b = (Button)sender;
                if (b.BackgroundImage != null)
                    b.BackgroundImage = b.BackgroundImage.SwitchGrayScale($"{b.Name}BackgroundImage");
                if (b.Image != null)
                    b.Image = b.Image.SwitchGrayScale($"{b.Name}Image");
                return;
            }
            if (!(sender is PictureBox))
                return;
            var pb = (PictureBox)sender;
            if (pb.BackgroundImage != null)
                pb.BackgroundImage = pb.BackgroundImage.SwitchGrayScale($"{pb.Name}BackgroundImage");
            if (pb.Image != null)
                pb.Image = pb.Image.SwitchGrayScale($"{pb.Name}Image");
        }

        private void OpenNewFormBtn_Click(object sender, EventArgs e)
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
            var result = false;
            try
            {
                using (var dialog = form)
                {
                    var point = GetWindowStartPos(new Point(dialog.Width, dialog.Height));
                    if (point != new Point(0, 0))
                    {
                        dialog.StartPosition = FormStartPosition.Manual;
                        dialog.Left = point.X;
                        dialog.Top = point.Y;
                    }
                    dialog.AddLoadingTimeStopwatch();
                    result = dialog.ShowDialog() == DialogResult.Yes;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (result && sender is Button)
            {
                ProcessEx.Start(PathEx.LocalPath, Main.ActionGuid.AllowNewInstance);
                Application.Exit();
            }
            else
            {
                if (!TopMost)
                    TopMost = true;
                if (WinApi.UnsafeNativeMethods.GetForegroundWindow() != Handle)
                    WinApi.UnsafeNativeMethods.SetForegroundWindow(Handle);
            }
        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            ProcessEx.Start("%WinDir%\\explorer.exe", PathEx.Combine(PathEx.LocalDir, "Documents"));
            Application.Exit();
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            Main.SkipUpdateSearch = true;
#if x86
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader.exe");
#else
            ProcessEx.Start("%CurDir%\\Binaries\\AppsDownloader64.exe");
#endif
            Application.Exit();
        }

        private void CloseBtn_Click(object sender, EventArgs e) =>
            Application.Exit();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.LWin && keyData != Keys.RWin)
                return base.ProcessCmdKey(ref msg, keyData);
            Application.Exit();
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            const uint htleft = 10;
            const uint htright = 11;
            const uint htbottomright = 17;
            const uint htbottom = 15;
            const uint htbottomleft = 16;
            const uint httop = 12;
            const uint httopright = 14;
            var handled = false;
            if (m.Msg == 0x0084 || m.Msg == 0x0200)
            {
                var wndSize = Size;
                var hitBoxes = new Dictionary<uint, Rectangle>();
                switch (TaskBar.GetLocation(Handle))
                {
                    case TaskBar.Location.Left:
                    case TaskBar.Location.Top:
                        hitBoxes.Add(htright, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htbottomright, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htbottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TaskBar.Location.Right:
                        hitBoxes.Add(htleft, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htbottomleft, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htbottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TaskBar.Location.Bottom:
                        hitBoxes.Add(htright, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(httopright, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        hitBoxes.Add(httop, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        break;
                    default:
                        hitBoxes.Add(htleft, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htright, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htbottomright, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htbottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(htbottomleft, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(httop, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(httopright, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        break;
                }
                var scrPoint = new Point(m.LParam.ToInt32());
                var clntPoint = PointToClient(scrPoint);
                foreach (var hitBox in hitBoxes)
                    if (hitBox.Value.Contains(clntPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
            }
            if (!handled)
                base.WndProc(ref m);
        }
    }
}
