namespace AppsLauncher.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using LangResources;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MenuViewForm : Form
    {
        private Point _appsListViewCursorLocation;
        private bool _appStartEventCalled, _hideHScrollBar;
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
            layoutPanel.SetControlStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer);
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
                    WinApi.NativeHelper.ClientToScreen(Handle, ref point);
                    WinApi.NativeHelper.SetCursorPos((uint)point.X, (uint)point.Y);
                    var inputMouseDown = new WinApi.DeviceInput();
                    inputMouseDown.Data.Mouse.Flags = 0x2;
                    inputMouseDown.Type = 0;
                    var inputMouseUp = new WinApi.DeviceInput();
                    inputMouseUp.Data.Mouse.Flags = 0x4;
                    inputMouseUp.Type = 0;
                    WinApi.DeviceInput[] inputs =
                    {
                        inputMouseUp,
                        inputMouseDown
                    };
                    WinApi.NativeHelper.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(WinApi.DeviceInput)));
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
            if (Main.ScreenDpi > 96)
                appsListViewPanel.Font = SystemFonts.SmallCaptionFont;
            appsListViewPanel.BackColor = Main.Colors.Control;
            appsListViewPanel.ForeColor = Main.Colors.ControlText;
            appsListView.BackColor = appsListViewPanel.BackColor;
            appsListView.ForeColor = appsListViewPanel.ForeColor;
            appsListView.SetDoubleBuffer();
            appsListView.SetMouseOverCursor();

            searchBox.BackColor = Main.Colors.Control;
            searchBox.ForeColor = Main.Colors.ControlText;
            searchBox.DrawSearchSymbol(Main.Colors.ControlText);
            SearchBox_Leave(searchBox, EventArgs.Empty);

            title.ForeColor = Main.BackgroundImage.GetAverageColor().InvertRgb().ToGrayScale();
            logoBox.Image = Resources.PortableApps_Logo_gray.Redraw(logoBox.Height, logoBox.Height);
            appsCount.ForeColor = title.ForeColor;

            aboutBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Help, Main.SystemResourcePath)?.ToBitmap();
            aboutBtn.BackgroundImage = aboutBtn.BackgroundImage.SwitchGrayScale($"{aboutBtn.Name}BackgroundImage");

            profileBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.UserDir, true, Main.SystemResourcePath)?.ToBitmap();
            downloadBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Network, Main.SystemResourcePath)?.ToBitmap();
            settingsBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.SystemControl, Main.SystemResourcePath)?.ToBitmap();
            foreach (var btn in new[] { downloadBtn, settingsBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appMenuItem2.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem3.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem5.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.Pin, Main.SystemResourcePath)?.ToBitmap();
            appMenuItem7.Image = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.RecycleBinEmpty, Main.SystemResourcePath)?.ToBitmap();
            appMenu.CloseOnMouseLeave(32);
            appMenu.EnableAnimation();
            appMenu.SetFixedSingle();

            var docDir = PathEx.Combine(PathEx.LocalDir, "Documents");
            if (Directory.Exists(docDir) && Data.DirIsLink(docDir) && !Data.MatchAttributes(docDir, FileAttributes.Hidden))
                Data.SetAttributes(docDir, FileAttributes.Hidden);

            _windowOpacity = Ini.Read("Settings", "Window.Opacity", 95d);
            if (_windowOpacity.IsBetween(20d, 100d))
                _windowOpacity /= 100d;
            else
                _windowOpacity = .95d;

            var windowFadeInEffect = Ini.Read("Settings", "Window.FadeInEffect", 0);
            _windowFadeInDuration = Ini.Read("Settings", "Window.FadeInDuration", 100);
            if (_windowFadeInDuration < 25)
                _windowFadeInDuration = 25;
            if (_windowFadeInDuration > 750)
                _windowFadeInDuration = 750;

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

            var windowAnimation = windowFadeInEffect == 0 ? WinApi.AnimateWindowFlags.Blend : WinApi.AnimateWindowFlags.Slide;
            switch (windowAnimation)
            {
                case WinApi.AnimateWindowFlags.Blend:
                    return;
                case WinApi.AnimateWindowFlags.Slide:
                    var windowPosition = Ini.Read("Settings", "Window.DefaultPosition", 0);
                    if (windowPosition == 1)
                    {
                        windowAnimation = WinApi.AnimateWindowFlags.Center;
                        break;
                    }
                    switch (TaskBar.GetLocation(Handle))
                    {
                        case TaskBar.Location.Left:
                            windowAnimation |= WinApi.AnimateWindowFlags.HorPositive;
                            break;
                        case TaskBar.Location.Top:
                            windowAnimation |= WinApi.AnimateWindowFlags.VerPositive;
                            break;
                        case TaskBar.Location.Right:
                            windowAnimation |= WinApi.AnimateWindowFlags.HorNegative;
                            break;
                        case TaskBar.Location.Bottom:
                            windowAnimation |= WinApi.AnimateWindowFlags.VerNegative;
                            break;
                        default:
                            windowAnimation = WinApi.AnimateWindowFlags.Center;
                            break;
                    }
                    break;
            }
            Opacity = _windowOpacity;
            WinApi.NativeHelper.AnimateWindow(Handle, _windowFadeInDuration, windowAnimation);
        }

        private void MenuViewForm_Shown(object sender, EventArgs e)
        {
            MenuViewForm_Resize(this, EventArgs.Empty);
            if (Opacity <= 0d)
            {
                Opacity = 0d;
                var timer = new Timer(components)
                {
                    Interval = 1,
                    Enabled = true
                };
                timer.Tick += (o, args) =>
                {
                    if (Opacity < _windowOpacity)
                    {
                        var opacity = _windowOpacity / (_windowFadeInDuration / 10d) + Opacity;
                        if (opacity <= _windowOpacity)
                        {
                            Opacity = opacity;
                            return;
                        }
                    }
                    Opacity = _windowOpacity;
                    if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                        WinApi.NativeHelper.SetForegroundWindow(Handle);
                    timer.Dispose();
                };
                return;
            }
            if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                WinApi.NativeHelper.SetForegroundWindow(Handle);
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
            Refresh();
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
                Main.StartMenuFolderUpdateHandler(list);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void MenuViewForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Main.UpdateSearchHandler();

        private void MenuViewForm_Update(bool setWindowLocation = true)
        {
            Main.CheckAvailableApps();
            appsListView.BeginUpdate();
            try
            {
                appsListView.Items.Clear();
                if (!appsListView.Scrollable)
                    appsListView.Scrollable = true;

                imgList.Images.Clear();

                var cachePath = Path.Combine(Main.TmpDir, "images.dat");
                var cacheDict = new Dictionary<string, Image>();
                var cacheSize = 0;

                string dictPath = null;
                Dictionary<string, Image> imgDict = null;
                Image defExeIcon = null;

                for (var i = 0; i < Main.AppsInfo.Count; i++)
                {
                    var appInfo = Main.AppsInfo[i];
                    var longName = appInfo.LongName;
                    if (string.IsNullOrWhiteSpace(longName))
                        continue;

                    appsListView.Items.Add(longName, i);
                    var shortName = appInfo.ShortName;
                    if (File.Exists(cachePath))
                    {
                        if (cacheDict?.Any() != true)
                        {
                            cacheDict = File.ReadAllBytes(cachePath).DeserializeObject<Dictionary<string, Image>>();
                            cacheSize = cacheDict.Count;
                        }
                        if (cacheDict?.ContainsKey(shortName) == true)
                        {
                            var img = cacheDict[shortName];
                            imgList.Images.Add(shortName, img);
                            continue;
                        }
                    }

                    if (dictPath == null)
                        dictPath = PathEx.Combine(PathEx.LocalDir, "Assets\\images.dat");
                    if (!File.Exists(dictPath))
                        goto TryHard;
                    if (imgDict == null)
                        imgDict = File.ReadAllBytes(dictPath).DeserializeObject<Dictionary<string, Image>>();
                    if (imgDict == null)
                        goto TryHard;
                    if (imgDict?.ContainsKey(shortName) == true)
                    {
                        var img = imgDict[shortName];
                        imgList.Images.Add(shortName, img);
                        cacheDict.Add(shortName, img);
                        continue;
                    }

                    TryHard:
                    try
                    {
                        var exePath = appInfo.ExePath;
                        var imgPath = Path.ChangeExtension(exePath, ".png");
                        Image img;
                        if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                        {
                            img = Image.FromFile(imgPath);
                            imgList.Images.Add(shortName, img);
                            cacheDict.Add(shortName, img);
                            continue;
                        }
                        var appDir = Path.GetDirectoryName(exePath);
                        if (!string.IsNullOrEmpty(appDir) && !File.Exists(imgPath))
                            imgPath = Path.Combine(appDir, "App\\AppInfo\\appicon_16.png");
                        if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                        {
                            img = Image.FromFile(imgPath)?.Redraw(16, 16);
                            imgList.Images.Add(shortName, img);
                            cacheDict.Add(shortName, img);
                            continue;
                        }
                        using (var ico = ResourcesEx.GetIconFromFile(exePath))
                        {
                            img = ico?.ToBitmap()?.Redraw(16, 16);
                            if (img == null)
                                goto Default;
                            imgList.Images.Add(shortName, img);
                            cacheDict.Add(shortName, img);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    Default:
                    if (defExeIcon == null)
                        defExeIcon = ResourcesEx.GetSystemIcon(ResourcesEx.IconIndex.ExeFile, Main.SystemResourcePath)?.ToBitmap().Redraw(16, 16);
                    if (defExeIcon == null)
                        continue;
                    imgList.Images.Add(shortName, defExeIcon);
                }

                appsListView.SmallImageList = imgList;
                if (cacheDict.Count > 0 && (cacheDict.Count != cacheSize || !File.Exists(cachePath)))
                    File.WriteAllBytes(cachePath, cacheDict.SerializeObject());

                if (!setWindowLocation)
                    return;
                var defaultPos = Ini.Read("Settings", "Window.DefaultPosition", 0);
                var taskbarLocation = TaskBar.GetLocation(Handle);
                if (defaultPos == 0 && taskbarLocation != TaskBar.Location.Hidden)
                {
                    var screen = Screen.PrimaryScreen.WorkingArea;
                    foreach (var scr in Screen.AllScreens)
                    {
                        if (!scr.Bounds.Contains(Cursor.Position))
                            continue;
                        screen = scr.WorkingArea;
                        break;
                    }
                    switch (taskbarLocation)
                    {
                        case TaskBar.Location.Left:
                        case TaskBar.Location.Top:
                            Left = screen.X;
                            Top = screen.Y;
                            break;
                        case TaskBar.Location.Right:
                            Left = screen.Width - Width;
                            Top = screen.Y;
                            break;
                        default:
                            Left = screen.X;
                            Top = screen.Height - Height;
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
            finally
            {
                appsListView.EndUpdate();
                appsCount.Text = string.Format(Lang.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? Lang.GetText(nameof(en_US.App)) : Lang.GetText(nameof(en_US.Apps)));
                if (!appsListView.Focus())
                    appsListView.Select();
            }
        }

        private Point GetWindowStartPos(Point point)
        {
            var pos = new Point();
            var screen = SystemInformation.VirtualScreen;
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
                        pos.X = screen.Width - point.X;
                        pos.Y = Cursor.Position.Y;
                        break;
                    default:
                        pos.X = Cursor.Position.X - point.X / 2;
                        pos.Y = Cursor.Position.Y - point.Y;
                        break;
                }
                if (pos.X + point.X > screen.Width)
                    pos.X = screen.Width - point.X;
                if (pos.Y + point.Y > screen.Height)
                    pos.Y = screen.Height - point.Y;
            }
            else
            {
                var max = new Point(screen.Width - point.X, screen.Height - point.Y);
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
            if (sender is ListView owner && !owner.LabelEdit && !owner.Focus())
                owner.Select();
        }

        private void AppsListView_MouseLeave(object sender, EventArgs e)
        {
            if (sender is ListView owner && owner.Focus())
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
                    var isEmpty = searchBox.Text.EqualsEx(Lang.GetText(searchBox));
                    if (isEmpty && _searchText.Length > 0 || !isEmpty && searchBox.Text.Length > 0)
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
                        searchBox.Text += key?.Last();
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
                    if (MessageBoxEx.Show(this, string.Format(Lang.GetText($"{owner.Name}Msg"), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appInfo = Main.GetAppInfo(appsListView.SelectedItems[0].Text);
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
                                    {
                                        _appStartEventCalled = true;
                                        Data.ForceDelete(appDir, true);
                                    }
                                }
                                Ini.RemoveSection(appInfo.ShortName);
                                Ini.WriteAll();
                                MenuViewForm_Update(false);
                                MessageBoxEx.Show(this, Lang.GetText(nameof(en_US.OperationCompletedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                _appStartEventCalled = false;
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
            appsListView.BeginUpdate();
            try
            {
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
                    if (item.Text == Main.ItemSearchHandler(owner.Text, itemList))
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
            finally
            {
                appsListView.EndUpdate();
            }
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
            if (WinApi.NativeHelper.GetForegroundWindow() != Handle)
                WinApi.NativeHelper.SetForegroundWindow(Handle);
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
            if (handled)
                return;
            /*
            if (_hideHScrollBar)
                WinApi.NativeHelper.ShowScrollBar(appsListView.Handle, WinApi.ShowScrollBarOptions.Horizontal, false);
            */
            base.WndProc(ref m);
        }
    }
}
