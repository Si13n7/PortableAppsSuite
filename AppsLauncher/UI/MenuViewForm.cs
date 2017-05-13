namespace AppsLauncher.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using LangResources;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MenuViewForm : Form
    {
        private Point _appsListViewCursorLocation;
        private bool _appStartEventCalled;
        private bool _hideHScrollBar;
        private string _searchText;
        private int _windowFadeInDuration;
        private double _windowOpacity;

        public MenuViewForm()
        {
            InitializeComponent();
        }

        private void MenuViewForm_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(byte.MaxValue, Main.Colors.Base.R, Main.Colors.Base.G, Main.Colors.Base.B);
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

            layoutPanel.BackgroundImage = Main.BackgroundImage;
            layoutPanel.BackgroundImageLayout = Main.BackgroundImageLayout;
            layoutPanel.BackColor = Main.Colors.Base;
            layoutPanel.SetControlStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer);
            ControlEx.DrawSizeGrip(layoutPanel, Main.Colors.Base,
                (o, args) =>
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
                    inputMouseDown.Data.Mouse.Flags = 0x2;
                    inputMouseDown.Type = 0;
                    var inputMouseUp = new WinApi.INPUT();
                    inputMouseUp.Data.Mouse.Flags = 0x4;
                    inputMouseUp.Type = 0;
                    WinApi.INPUT[] inputs =
                    {
                        inputMouseUp,
                        inputMouseDown
                    };
                    WinApi.UnsafeNativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(WinApi.INPUT)));
                },
                (o, args) =>
                {
                    var p = o as PictureBox;
                    if (p == null)
                        return;
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
                });

            _hideHScrollBar = Ini.Read("Settings", "Window.HideHScrollBar", false);
            MenuViewForm_Resize(this, EventArgs.Empty);
            if (Main.ScreenDpi > 96)
                appsListViewPanel.Font = SystemFonts.SmallCaptionFont;
            appsListViewPanel.BackColor = Main.Colors.Control;
            appsListViewPanel.ForeColor = Main.Colors.ControlText;
            appsListView.BackColor = appsListViewPanel.BackColor;
            appsListView.ForeColor = appsListViewPanel.ForeColor;
            appsListView.SetControlStyle(ControlStyles.OptimizedDoubleBuffer);
            WinApi.UnsafeNativeMethods.SendMessage(appsListView.Handle, 0x103e, IntPtr.Zero, Cursors.Arrow.Handle);

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            searchBox.DrawSearchSymbol(Main.Colors.ControlText);
            SearchBox_Leave(searchBox, EventArgs.Empty);

            title.ForeColor = Main.BackgroundImage.GetAverageColor().InvertRgb().ToGrayScale();
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

            appMenu.CloseOnMouseLeave(32);
            appMenu.EnableAnimation();
            appMenu.SetFixedSingle();
            appMenuItem2.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem3.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem5.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Pin, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem7.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.RecycleBinEmpty, Main.SystemResourcePath)?.ToBitmap();

            var docDir = PathEx.Combine(PathEx.LocalDir, "Documents");
            if (Directory.Exists(docDir) && Data.DirIsLink(docDir) && !Data.MatchAttributes(docDir, FileAttributes.Hidden))
                Data.SetAttributes(docDir, FileAttributes.Hidden);

            _windowOpacity = Ini.Read("Settings", "Window.Opacity", 95d);
            if (_windowOpacity.IsBetween(20d, 100d))
                _windowOpacity /= 100d;
            else
                _windowOpacity = .95d;

            _windowFadeInDuration = Ini.Read("Settings", "Window.FadeInDuration", 1);
            if (_windowFadeInDuration < 1)
                _windowFadeInDuration = 1;
            var opacity = (int)(_windowOpacity * 100d);
            if (_windowFadeInDuration > opacity)
                _windowFadeInDuration = opacity;

            var windowWidth = Ini.Read("Settings", "Window.Size.Width", MinimumSize.Width);
            if (windowWidth > MinimumSize.Width && windowWidth < MaximumSize.Width)
                Width = windowWidth;
            if (windowWidth > MaximumSize.Width)
                Width = MaximumSize.Width;

            var windowHeight = Ini.Read("Settings", "Window.Size.Height", MinimumSize.Height);
            if (windowHeight > MinimumSize.Height && windowHeight < MaximumSize.Height)
                Height = windowHeight;
            if (windowHeight > MaximumSize.Height)
                Height = MaximumSize.Height;

            MenuViewForm_Update();
        }

        private void MenuViewForm_Shown(object sender, EventArgs e)
        {
            var timer = new Timer
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < _windowOpacity)
                {
                    var opacity = _windowOpacity / _windowFadeInDuration + Opacity;
                    if (opacity <= _windowOpacity)
                    {
                        Opacity = opacity;
                        return;
                    }
                }
                if (WinApi.UnsafeNativeMethods.GetForegroundWindow() != Handle)
                    WinApi.UnsafeNativeMethods.SetForegroundWindow(Handle);
                timer.Dispose();
            };
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
            var width = Ini.Read("Settings", "Window.Size.Width", Width);
            var height = Ini.Read("Settings", "Window.Size.Height", Height);
            if (Width == width && Height == height)
                return;
            Ini.Write("Settings", "Window.Size.Width", Width, false);
            Ini.Write("Settings", "Window.Size.Height", Height, false);
            Ini.WriteAll();
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
            if (!Ini.Read("Settings", "StartMenuIntegration", false))
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
                var cacheDir = Main.ImageCacheDir;
                for (var i = 0; i < Main.AppsInfo.Count; i++)
                {
                    var appInfo = Main.AppsInfo[i];
                    if (string.IsNullOrWhiteSpace(appInfo.LongName))
                        continue;
                    appsListView.Items.Add(appInfo.LongName, i);
                    var imgCachePath = Path.Combine(cacheDir, appInfo.ShortName);
                    try
                    {
                        var imgFromCache = File.Exists(imgCachePath) ? File.ReadAllBytes(imgCachePath)?.FromByteArrayToImage() : null;
                        if (imgFromCache != null)
                            imgList.Images.Add(appInfo.ShortName, imgFromCache);
                        if (imgList.Images.ContainsKey(appInfo.ShortName))
                            continue;
                        if (icoDb == null)
                            try
                            {
                                icoDb = File.ReadAllBytes(PathEx.Combine(PathEx.LocalDir, "Assets\\images.zip"));
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
                                            if (!entry.Name.EqualsEx(appInfo.ShortName))
                                                continue;
                                            var imgFromStream = Image.FromStream(entry.Open());
                                            imgList.Images.Add(appInfo.ShortName, imgFromStream);
                                            imgFromStream.Save(imgCachePath);
                                            break;
                                        }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                        if (imgList.Images.ContainsKey(appInfo.ShortName))
                            continue;
                        var appDir = Path.GetDirectoryName(appInfo.ExePath);
                        var imgPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appInfo.ExePath)}.png");
                        Image imgFromFile;
                        if (!File.Exists(imgPath))
                        {
                            if (imgList.Images.ContainsKey(appInfo.ShortName))
                                continue;
                            if (!File.Exists(imgPath))
                                imgPath = Path.Combine(appDir, "App\\AppInfo\\appicon_16.png");
                            if (File.Exists(imgPath))
                            {
                                imgFromFile = Image.FromFile(imgPath).Redraw(16, 16);
                                imgList.Images.Add(appInfo.ShortName, imgFromFile);
                                imgFromFile.Save(imgCachePath);
                            }
                            if (imgList.Images.ContainsKey(appInfo.ShortName))
                                continue;
                            using (var ico = ResourcesEx.GetIconFromFile(appInfo.ExePath))
                                if (ico != null)
                                {
                                    var imgFromIcon = ico?.ToBitmap().Redraw(16, 16);
                                    if (imgFromIcon == null)
                                        throw new ArgumentNullException();
                                    imgList.Images.Add(appInfo.ShortName, imgFromIcon);
                                    imgFromIcon.Save(imgCachePath);
                                    continue;
                                }
                            throw new ArgumentException();
                        }
                        imgFromFile = Image.FromFile(imgPath);
                        imgList.Images.Add(appInfo.ShortName, imgFromFile);
                        imgFromFile.Save(imgCachePath);
                    }
                    catch
                    {
                        if (defExeIcon == null)
                            continue;
                        imgList.Images.Add(appInfo.ShortName, defExeIcon);
                    }
                }
                appsListView.SmallImageList = imgList;
                if (setWindowLocation)
                {
                    var defaultPos = Ini.Read("Settings", "Window.DefaultPosition", 0);
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
                appsCount.Text = string.Format(Lang.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? Lang.GetText(nameof(en_US.App)) : Lang.GetText(nameof(en_US.Apps)));
                if (!appsListView.Focus())
                    appsListView.Select();
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
            if (Ini.Read("Settings", "Window.DefaultPosition", 0) == 0)
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

        private void AppsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            var owner = sender as ListView;
            if (owner == null || owner.SelectedItems.Count <= 0)
                return;
            _appStartEventCalled = true;
            if (Opacity > 0)
                Opacity = 0;
            Main.StartApp(owner.SelectedItems[0].Text, true);
        }

        private void AppsListView_MouseEnter(object sender, EventArgs e)
        {
            var owner = sender as ListView;
            if (owner != null && !owner.LabelEdit && !owner.Focus())
                owner.Select();
        }

        private void AppsListView_MouseLeave(object sender, EventArgs e)
        {
            var owner = sender as ListView;
            if (owner != null && owner.Focus())
                owner.Parent.Select();
        }

        private void AppsListView_MouseMove(object sender, MouseEventArgs e)
        {
            var owner = sender as ListView;
            if (owner != null && owner.LabelEdit)
                return;
            var ownerItem = owner.ItemFromPoint();
            if (ownerItem == null || _appsListViewCursorLocation == Cursor.Position)
                return;
            ownerItem.Selected = true;
            _appsListViewCursorLocation = Cursor.Position;
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
            var owner = sender as ListView;
            if (owner == null)
                return;
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                try
                {
                    var appInfo = Main.GetAppInfo(owner.SelectedItems[0].Text);
                    if (appInfo.LongName != owner.SelectedItems[0].Text)
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
                    Ini.WriteAll(iniPath, true, true);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                MenuViewForm_Update();
            }
            if (owner.LabelEdit)
                owner.LabelEdit = false;
        }

        private void AppMenu_Opening(object sender, CancelEventArgs e) =>
            e.Cancel = appsListView.SelectedItems.Count == 0;

        private void AppMenuItem_Click(object sender, EventArgs e)
        {
            if (appsListView.SelectedItems.Count == 0)
                return;
            var owner = sender as ToolStripMenuItem;
            switch (owner?.Name)
            {
                case "appMenuItem1":
                case "appMenuItem2":
                case "appMenuItem3":
                    if (Opacity > 0)
                        Opacity = 0;
                    switch (owner.Name)
                    {
                        case "appMenuItem1":
                        case "appMenuItem2":
                            _appStartEventCalled = true;
                            Main.StartApp(appsListView.SelectedItems[0].Text, true, owner.Name.EqualsEx("appMenuItem2"));
                            break;
                        case "appMenuItem3":
                            _appStartEventCalled = true;
                            Main.OpenAppLocation(appsListView.SelectedItems[0].Text, true);
                            break;
                    }
                    break;
                case "appMenuItem4":
                    MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    var targetPath = EnvironmentEx.GetVariablePathFull(Main.GetAppPath(appsListView.SelectedItems[0].Text), false);
                    var linkPath = Path.Combine("%Desktop%", appsListView.SelectedItems[0].Text);
                    if (Data.CreateShortcut(targetPath, linkPath))
                        MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    var appPath = Main.GetAppPath(appsListView.SelectedItems[0].Text);
                    if (Data.PinToTaskbar(appPath))
                        MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.appMenuItem4Msg0)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.appMenuItem4Msg1)), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    if (MessageBoxEx.Show(this, string.Format(Lang.GetText(nameof(en_US.appMenuItem7Msg)), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appDir = Path.GetDirectoryName(Main.GetAppPath(appsListView.SelectedItems[0].Text));
                            if (Directory.Exists(appDir))
                            {
                                ProcessEx.Terminate(Data.GetLocks(appDir));
                                try
                                {
                                    var cacheImage = Path.Combine(Main.ImageCacheDir, Path.GetFileName(appDir));
                                    if (File.Exists(cacheImage))
                                        File.Delete(cacheImage);
                                    Directory.Delete(appDir, true);
                                }
                                catch
                                {
                                    var tmpDir = Path.Combine(Path.GetTempPath(), PathEx.GetTempDirName());
                                    if (!Directory.Exists(tmpDir))
                                        Directory.CreateDirectory(tmpDir);
                                    var command = $"ROBOCOPY \"{tmpDir}\" \"{appDir}\" /MIR && RD /S /Q \"{tmpDir}\"";
                                    using (var p = ProcessEx.Send(command, false, false))
                                        if (!p?.HasExited == true)
                                            p?.WaitForExit();
                                    if (Directory.Exists(appDir))
                                        Directory.Delete(appDir, true);
                                }
                                MenuViewForm_Update(false);
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
            if (MessageBoxEx.CenterMousePointer)
                MessageBoxEx.CenterMousePointer = false;
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            _appsListViewCursorLocation = Cursor.Position;
            var owner = sender as TextBox;
            if (owner == null)
                return;
            owner.Font = new Font("Segoe UI", owner.Font.Size);
            owner.ForeColor = Main.Colors.ControlText;
            owner.Text = _searchText ?? string.Empty;
            appsListView.HideSelection = true;
        }

        private void SearchBox_Leave(object sender, EventArgs e)
        {
            var owner = sender as TextBox;
            if (owner == null)
                return;
            var c = Main.Colors.ControlText;
            owner.Font = new Font("Comic Sans MS", owner.Font.Size, FontStyle.Italic);
            owner.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = owner.Text;
            owner.Text = Lang.GetText(owner);
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
            var owner = sender as TextBox;
            if (owner == null)
                return;
            var itemList = new List<string>();
            foreach (ListViewItem item in appsListView.Items)
            {
                item.ForeColor = Main.Colors.ControlText;
                item.BackColor = Main.Colors.Control;
                itemList.Add(item.Text);
            }
            if (string.IsNullOrWhiteSpace(owner.Text) || owner.Font.Italic)
                return;
            foreach (ListViewItem item in appsListView.Items)
                if (item.Text == Main.SearchMatchItem(owner.Text, itemList))
                {
                    item.ForeColor = Main.Colors.HighlightText;
                    item.BackColor = Main.Colors.Highlight;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            _appsListViewCursorLocation = Cursor.Position;
        }

        private void ImageButton_MouseEnterLeave(object sender, EventArgs e)
        {
            var owner1 = sender as Button;
            if (owner1?.BackgroundImage != null)
                owner1.BackgroundImage = owner1.BackgroundImage.SwitchGrayScale($"{owner1.Name}BackgroundImage");
            if (owner1?.Image != null)
                owner1.Image = owner1.Image.SwitchGrayScale($"{owner1.Name}Image");
            if (owner1 != null)
                return;
            var owner2 = sender as PictureBox;
            if (owner2?.BackgroundImage != null)
                owner2.BackgroundImage = owner2.BackgroundImage.SwitchGrayScale($"{owner2.Name}BackgroundImage");
            if (owner2?.Image != null)
                owner2.Image = owner2.Image.SwitchGrayScale($"{owner2.Name}Image");
        }

        private void AboutBtn_Click(object sender, EventArgs e)
        {
            OpenForm(new AboutForm());
            if (WinApi.UnsafeNativeMethods.GetForegroundWindow() != Handle)
                WinApi.UnsafeNativeMethods.SetForegroundWindow(Handle);
        }

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            if (!OpenForm(new SettingsForm(string.Empty)))
                return;
            ProcessEx.Start(PathEx.LocalPath, Main.ActionGuid.AllowNewInstance);
            Application.Exit();
        }

        private bool OpenForm(Form form)
        {
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
                    dialog.TopMost = true;
                    dialog.Plus();
                    result = dialog.ShowDialog() == DialogResult.Yes;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            TopMost = true;
            return result;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.LWin && keyData != Keys.RWin)
                return base.ProcessCmdKey(ref msg, keyData);
            Application.Exit();
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            const uint htLeft = 10;
            const uint htRight = 11;
            const uint htBottomRight = 17;
            const uint htBottom = 15;
            const uint htBottomLeft = 16;
            const uint htTop = 12;
            const uint htTopRight = 14;
            var handled = false;
            if (m.Msg == 0x0084 || m.Msg == 0x0200)
            {
                var wndSize = Size;
                var hitBoxes = new Dictionary<uint, Rectangle>();
                switch (TaskBar.GetLocation(Handle))
                {
                    case TaskBar.Location.Left:
                    case TaskBar.Location.Top:
                        hitBoxes.Add(htRight, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htBottomRight, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htBottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TaskBar.Location.Right:
                        hitBoxes.Add(htLeft, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htBottomLeft, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htBottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        break;
                    case TaskBar.Location.Bottom:
                        hitBoxes.Add(htRight, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htTopRight, new Rectangle(wndSize.Width - 8, 0, 8, 8));
                        hitBoxes.Add(htTop, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        break;
                    default:
                        hitBoxes.Add(htLeft, new Rectangle(0, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htRight, new Rectangle(wndSize.Width - 8, 8, 8, wndSize.Height - 2 * 8));
                        hitBoxes.Add(htBottomRight, new Rectangle(wndSize.Width - 8, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htBottom, new Rectangle(8, wndSize.Height - 8, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(htBottomLeft, new Rectangle(0, wndSize.Height - 8, 8, 8));
                        hitBoxes.Add(htTop, new Rectangle(8, 0, wndSize.Width - 2 * 8, 8));
                        hitBoxes.Add(htTopRight, new Rectangle(wndSize.Width - 8, 0, 8, 8));
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
