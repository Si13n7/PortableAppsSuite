namespace AppsLauncher.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using LangResources;
    using Libraries;
    using Properties;
    using SilDev;
    using SilDev.Drawing;
    using SilDev.Forms;

    public sealed partial class MenuViewForm :
#if !DEBUG
        FormEx.BorderlessResizable
#else
        Form
#endif
    {
        private Point _appsListViewCursorLocation;
        private bool _preventClosure;
        private string _searchText;

        public MenuViewForm()
        {
            InitializeComponent();

            BackColor = Settings.Window.Colors.BaseDark;
            if (Settings.Window.BackgroundImage != default(Image))
            {
                BackgroundImage = Settings.Window.BackgroundImage;
                BackgroundImageLayout = Settings.Window.BackgroundImageLayout;
            }
            Icon = Resources.PortableApps_blue;
            Language.SetControlLang(this);
            ControlEx.DrawSizeGrip(this, Settings.Window.Colors.Base);
            ControlEx.DrawBorder(this, Settings.Window.Colors.Base);

            appsListViewPanel.BackColor = Settings.Window.Colors.Control;
            appsListViewPanel.ForeColor = Settings.Window.Colors.ControlText;
            appsListView.BackColor = Settings.Window.Colors.Control;
            appsListView.ForeColor = Settings.Window.Colors.ControlText;
            appsListView.SetDoubleBuffer();
            appsListView.SetMouseOverCursor();

            searchBox.BackColor = Settings.Window.Colors.Control;
            searchBox.ForeColor = Settings.Window.Colors.ControlText;
            searchBox.DrawSearchSymbol(Settings.Window.Colors.ControlText);
            SearchBox_Leave(searchBox, EventArgs.Empty);

            title.ForeColor = Settings.Window.Colors.BaseText;
            logoBox.Image = Resources.PortableApps_Logo_gray.Redraw(logoBox.Height, logoBox.Height);
            appsCount.ForeColor = Settings.Window.Colors.BaseText;

            aboutBtn.BackgroundImage = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Help);
            aboutBtn.BackgroundImage = aboutBtn.BackgroundImage.SwitchGrayScale($"{aboutBtn.Name}BackgroundImage");

            profileBtn.BackgroundImage = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.UserDir, true);

            downloadBtn.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Network);
            settingsBtn.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.SystemControl);
            foreach (var btn in new[] { downloadBtn, settingsBtn })
            {
                btn.BackColor = Settings.Window.Colors.Button;
                btn.ForeColor = Settings.Window.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Settings.Window.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Settings.Window.Colors.ButtonHover;
            }

            appMenuItem2.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Uac);
            appMenuItem3.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Directory);
            appMenuItem5.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Pin);
            appMenuItem7.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.RecycleBinEmpty);
            appMenuItem8.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.SystemControl);
            for (var i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Language.GetText(appMenu.Items[i].Name);
            appMenu.CloseOnMouseLeave(32);
            appMenu.EnableAnimation();
            appMenu.SetFixedSingle();

#if !DEBUG
            SetResizingBorders(TaskBar.GetLocation(Handle));
#endif

            appMenu.ResumeLayout(false);
            appsListViewPanel.ResumeLayout(false);
            ((ISupportInitialize)logoBox).EndInit();
            ((ISupportInitialize)aboutBtn).EndInit();
            ((ISupportInitialize)profileBtn).EndInit();
            downloadBtnPanel.ResumeLayout(false);
            settingsBtnPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private void MenuViewForm_Load(object sender, EventArgs e)
        {
            MinimumSize = Settings.Window.Size.Minimum;
            MaximumSize = Settings.Window.Size.Maximum;
            if (Settings.Window.Size.Width > Settings.Window.Size.Minimum.Width)
                Width = Settings.Window.Size.Width;
            if (Settings.Window.Size.Height > Settings.Window.Size.Minimum.Height)
                Height = Settings.Window.Size.Height;

            MenuViewForm_Update();

            var windowAnimation = Settings.Window.FadeInEffect == 0 ? WinApi.AnimateWindowFlags.Blend : WinApi.AnimateWindowFlags.Slide;
            switch (windowAnimation)
            {
                case WinApi.AnimateWindowFlags.Blend:
                    return;
                case WinApi.AnimateWindowFlags.Slide:
                    if (Settings.Window.DefaultPosition == 1)
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
            Opacity = Settings.Window.Opacity;
            WinApi.NativeHelper.AnimateWindow(Handle, Settings.Window.FadeInDuration, windowAnimation);
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
                    if (Opacity < Settings.Window.Opacity)
                    {
                        var opacity = Settings.Window.Opacity / (Settings.Window.FadeInDuration / 10d) + Opacity;
                        if (opacity <= Settings.Window.Opacity)
                        {
                            Opacity = opacity;
                            return;
                        }
                    }
                    Opacity = Settings.Window.Opacity;
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
            if (Application.OpenForms.Count > 1 || appMenu.Focus() || _preventClosure)
                return;
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
                Close();
        }

        private void MenuViewForm_ResizeBegin(object sender, EventArgs e)
        {
            if (!appsListView.Scrollable)
                appsListView.Scrollable = true;
            BackColor = Settings.Window.Colors.Base;
            if (BackgroundImage != default(Image))
                BackgroundImage = default(Image);
            this.SetChildVisibility(false, appsListViewPanel);
        }

        private void MenuViewForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!appsListView.Focus())
                appsListView.Select();
            BackColor = Settings.Window.Colors.BaseDark;
            if (Settings.Window.BackgroundImage != default(Image))
                BackgroundImage = Settings.Window.BackgroundImage;
            this.SetChildVisibility(true, appsListViewPanel);
            Settings.Window.Size.Width = Width;
            Settings.Window.Size.Height = Height;
            Settings.WriteToFile();
        }

        private void MenuViewForm_Resize(object sender, EventArgs e)
        {
            if (!Settings.Window.HideHScrollBar)
            {
                Refresh();
                return;
            }
            if (appsListView.Dock != DockStyle.None)
                appsListView.Dock = DockStyle.None;
            var padding = (int)Math.Floor(SystemInformation.HorizontalScrollBarHeight / 3d);
            appsListView.Location = new Point(padding + 3, padding + 2);
            appsListView.Size = appsListViewPanel.Size;
            appsListView.Region = new Region(new RectangleF(0, 0, appsListViewPanel.Width - padding - 3, appsListViewPanel.Height - SystemInformation.HorizontalScrollBarHeight - 2));
            Refresh();
        }

        private void MenuViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _preventClosure = true;
            if (Opacity > 0)
                Opacity = 0;
            if (!Settings.StartMenuIntegration)
                return;
            try
            {
                var appNames = appsListView.Items.OfType<ListViewItem>().Select(x => x.Text).Where(x => !x.EqualsEx("Portable"));
                SystemIntegration.UpdateStartMenuShortcuts(appNames);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void MenuViewForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Settings.StartUpdateSearch();

        private void MenuViewForm_Update(bool setWindowLocation = true)
        {
            ApplicationHandler.SetAppsInfo();
            appsListView.BeginUpdate();
            try
            {
                appsListView.Items.Clear();
                if (appsListView.SmallImageList != default(ImageList))
                    appsListView.SmallImageList.Images.Clear();
                if (!appsListView.Scrollable)
                    appsListView.Scrollable = true;

                imgList.Images.Clear();
                foreach (var appInfo in ApplicationHandler.AllAppInfos)
                {
                    var shortName = appInfo.ShortName;
                    if (string.IsNullOrWhiteSpace(shortName))
                        continue;

                    var longName = appInfo.LongName;
                    if (string.IsNullOrWhiteSpace(longName))
                        continue;

                    Image image;
                    if (Settings.CacheData.CurrentImages.ContainsKey(shortName))
                    {
                        image = Settings.CacheData.CurrentImages[shortName];
                        goto Finalize;
                    }
                    if (Settings.CacheData.AppImages.ContainsKey(shortName))
                    {
                        image = Settings.CacheData.AppImages[shortName];
                        goto UpdateCache;
                    }

                    var exePath = appInfo.ExePath;
                    if (!File.Exists(exePath))
                        continue;
                    try
                    {
                        var imgPath = Path.ChangeExtension(exePath, ".png");
                        if (!File.Exists(imgPath))
                        {
                            var appDir = Path.GetDirectoryName(exePath);
                            imgPath = Path.Combine(appDir, "App", "AppInfo", "appicon_16.png");
                        }
                        if (File.Exists(imgPath))
                        {
                            image = Image.FromFile(imgPath);
                            if (image != default(Image))
                            {
                                if (image.Width != image.Height || image.Width != 16)
                                    image = image.Redraw(16, 16);
                                goto UpdateCache;
                            }
                        }
                        using (var ico = ResourcesEx.GetIconFromFile(exePath))
                        {
                            image = ico?.ToBitmap()?.Redraw(16, 16);
                            if (image != default(Image))
                                goto UpdateCache;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.ExeFile);
                    if (image == default(Image))
                        continue;

                    UpdateCache:
                    if (!Settings.CacheData.CurrentImages.ContainsKey(shortName))
                        Settings.CacheData.CurrentImages.Add(shortName, image);

                    Finalize:
                    imgList.Images.Add(image);
                    appsListView.Items.Add(longName, imgList.Images.Count - 1);
                }

                appsListView.SmallImageList = imgList;
                Settings.CacheData.UpdateCurrentImagesFile();

                if (!setWindowLocation)
                    return;
                var taskbarLocation = TaskBar.GetLocation(Handle);
                if (Settings.Window.DefaultPosition == 0 && taskbarLocation != TaskBar.Location.Hidden)
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
                appsCount.Text = string.Format(Language.GetText(appsCount), appsListView.Items.Count, appsListView.Items.Count == 1 ? Language.GetText(nameof(en_US.App)) : Language.GetText(nameof(en_US.Apps)));
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
            if (Settings.Window.DefaultPosition == 0)
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
            if (!(sender is ListView owner) || owner.SelectedItems.Count <= 0)
                return;
            _preventClosure = true;
            if (Opacity > 0)
                Opacity = 0;
            ApplicationHandler.Start(owner.SelectedItems[0].Text, true);
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
            if (owner?.LabelEdit == true)
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
                    var isEmpty = searchBox.Text.EqualsEx(Language.GetText(searchBox));
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
            if (!(sender is ListView owner))
                return;
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                try
                {
                    var appInfo = ApplicationHandler.GetAppInfo(owner.SelectedItems[0].Text);
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
                            _preventClosure = true;
                            ApplicationHandler.Start(appsListView.SelectedItems[0].Text, true, owner.Name.EqualsEx("appMenuItem2"));
                            break;
                        case "appMenuItem3":
                            _preventClosure = true;
                            ApplicationHandler.OpenLocation(appsListView.SelectedItems[0].Text, true);
                            break;
                    }
                    break;
                case "appMenuItem4":
                    MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    var targetPath = ApplicationHandler.GetPath(appsListView.SelectedItems[0].Text);
                    var linkPath = Path.Combine("%Desktop%", appsListView.SelectedItems[0].Text);
                    if (FileEx.CreateShortcut(targetPath, linkPath))
                        MessageBoxEx.Show(this, Language.GetText($"{owner.Name}Msg0"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Language.GetText($"{owner.Name}Msg1"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "appMenuItem5":
                    MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                    var appPath = ApplicationHandler.GetPath(appsListView.SelectedItems[0].Text);
                    if (TaskBar.Pin(appPath))
                        MessageBoxEx.Show(this, Language.GetText(nameof(en_US.appMenuItem4Msg0)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBoxEx.Show(this, Language.GetText(nameof(en_US.appMenuItem4Msg1)), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (MessageBoxEx.Show(this, string.Format(Language.GetText($"{owner.Name}Msg"), appsListView.SelectedItems[0].Text), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        try
                        {
                            var appInfo = ApplicationHandler.GetAppInfo(appsListView.SelectedItems[0].Text);
                            var appDir = ApplicationHandler.GetLocation(appInfo.ShortName);
                            if (!Settings.AppDirs.Any(x => appDir.ContainsEx(x)))
                                throw new ArgumentOutOfRangeException(nameof(appDir));
                            if (Directory.Exists(appDir))
                            {
                                try
                                {
                                    FileEx.Delete(Settings.CachePaths.CurrentImages);
                                    Directory.Delete(appDir, true);
                                }
                                catch
                                {
                                    if (!PathEx.ForceDelete(appDir))
                                    {
                                        _preventClosure = true;
                                        PathEx.ForceDelete(appDir, true);
                                    }
                                }
                                Ini.RemoveSection(appInfo.ShortName);
                                Ini.WriteAll();
                                MenuViewForm_Update(false);
                                MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationCompletedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                _preventClosure = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationFailedMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBoxEx.CenterMousePointer = !ClientRectangle.Contains(PointToClient(MousePosition));
                        MessageBoxEx.Show(this, Language.GetText(nameof(en_US.OperationCanceledMsg)), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    break;
                case "appMenuItem8":
                    OpenForm(new SettingsForm(ApplicationHandler.GetAppInfo(appsListView.SelectedItems[0].Text).LongName));
                    break;
            }
            if (MessageBoxEx.CenterMousePointer)
                MessageBoxEx.CenterMousePointer = false;
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            _appsListViewCursorLocation = Cursor.Position;
            if (!(sender is TextBox owner))
                return;
            owner.Font = new Font("Segoe UI", owner.Font.Size);
            owner.ForeColor = Settings.Window.Colors.ControlText;
            owner.Text = _searchText ?? string.Empty;
            appsListView.HideSelection = true;
        }

        private void SearchBox_Leave(object sender, EventArgs e)
        {
            if (!(sender is TextBox owner))
                return;
            var c = Settings.Window.Colors.ControlText;
            owner.Font = new Font("Comic Sans MS", owner.Font.Size, FontStyle.Italic);
            owner.ForeColor = Color.FromArgb(c.A, c.R / 2, c.G / 2, c.B / 2);
            _searchText = owner.Text;
            owner.Text = Language.GetText(owner);
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
            if (!(sender is TextBox owner))
                return;
            appsListView.BeginUpdate();
            try
            {
                var itemList = new List<string>();
                foreach (ListViewItem item in appsListView.Items)
                {
                    item.ForeColor = Settings.Window.Colors.ControlText;
                    item.BackColor = Settings.Window.Colors.Control;
                    itemList.Add(item.Text);
                }
                if (string.IsNullOrWhiteSpace(owner.Text) || owner.Font.Italic)
                    return;
                foreach (ListViewItem item in appsListView.Items)
                    if (item.Text == ApplicationHandler.SearchItem(owner.Text, itemList))
                    {
                        item.ForeColor = Settings.Window.Colors.HighlightText;
                        item.BackColor = Settings.Window.Colors.Highlight;
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
            ProcessEx.Start(PathEx.LocalPath, Settings.ActionGuid.AllowNewInstance);
            Application.Exit();
        }

        private bool OpenForm(Form form)
        {
            _preventClosure = true;
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
            _preventClosure = false;
            TopMost = true;
            return result;
        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            ProcessEx.Start(Settings.CorePaths.SystemExplorer, Settings.CorePaths.UserDirs.FirstOrDefault());
            Application.Exit();
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            Settings.SkipUpdateSearch = true;
            ProcessEx.Start(Settings.CorePaths.AppsDownloader);
            Application.Exit();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.LWin && keyData != Keys.RWin)
                return base.ProcessCmdKey(ref msg, keyData);
            Application.Exit();
            return true;
        }
    }
}
