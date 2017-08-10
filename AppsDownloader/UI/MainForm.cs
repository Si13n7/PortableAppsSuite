namespace AppsDownloader.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;
    using LangResources;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using Timer = System.Windows.Forms.Timer;

    public partial class MainForm : Form
    {
        private static readonly object CheckDownloadLocker = new object();
        private static readonly Stopwatch DownloadStopwatch = new Stopwatch();
        private static readonly object MultiDownloaderLocker = new object();
        private readonly ListView _appListClone = new ListView();
        private readonly NotifyBox _notifyBox = new NotifyBox { Opacity = .8d };
        private bool _iniIsDisabled, _iniIsLoaded;
        private ProgressCircle _progressCircle;
        private int _searchResultBlinkCount;

        public MainForm()
        {
            InitializeComponent();
            appsList.ListViewItemSorter = new ListViewEx.AlphanumericComparer();
            searchBox.DrawSearchSymbol(searchBox.ForeColor);
            if (!appsList.Focus())
                appsList.Select();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            Icon = Resources.PortableApps_purple_64;
            MaximumSize = Screen.FromHandle(Handle).WorkingArea.Size;
#if !x86
            Text += @" (64-bit)";
#endif
            Main.Text = Text;
            Lang.SetControlLang(this);
            for (var i = 0; i < appsList.Columns.Count; i++)
                appsList.Columns[i].Text = Lang.GetText($"columnHeader{i + 1}");
            for (var i = 0; i < appsList.Groups.Count; i++)
                appsList.Groups[i].Header = Lang.GetText(appsList.Groups[i].Name);
            for (var i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Lang.GetText(appMenu.Items[i].Name);

            showColorsCheck.Left = showGroupsCheck.Right + 4;
            highlightInstalledCheck.Left = showColorsCheck.Right + 4;

            _progressCircle = new ProgressCircle
            {
                Anchor = downloadProgress.Anchor,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(65, 85, 100),
                InnerRadius = 12,
                Location = new Point(downloadProgress.Right + 8, downloadProgress.Top - downloadProgress.Height * 2),
                OuterRadius = 15,
                RotationSpeed = 80,
                Size = new Size(downloadProgress.Height * 5, downloadProgress.Height * 5),
                Thickness = 4,
                Visible = false
            };
            downloadStateAreaPanel.Controls.Add(_progressCircle);

            appsList.SetDoubleBuffer();
            appMenu.EnableAnimation();
            appMenu.SetFixedSingle();

            if (!Main.ActionGuid.IsUpdateInstance)
                _notifyBox.Show(Lang.GetText(nameof(en_US.DatabaseAccessMsg)), Main.Text, NotifyBox.NotifyBoxStartPosition.Center);

            if (Main.AvailableProtocols.HasFlag(Main.InternetProtocols.None))
            {
                if (!Main.ActionGuid.IsUpdateInstance)
                    MessageBoxEx.Show(Lang.GetText(nameof(en_US.InternetIsNotAvailableMsg)), Main.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Main.ApplicationExit(1);
                return;
            }

            if (!Main.ActionGuid.IsUpdateInstance && Main.InternalMirrors.Count == 0)
            {
                MessageBoxEx.Show(Lang.GetText(nameof(en_US.NoServerAvailableMsg)), Main.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Main.ApplicationExit(1);
                return;
            }

            Main.ResetAppDb();
            try
            {
                Main.UpdateAppDb();
                if (!Main.ActionGuid.IsUpdateInstance)
                {
                    if (Main.AppsDbSections.Any())
                        AppsList_SetContent(Main.AppsDbSections);
                    if (appsList.Items.Count == 0)
                        throw new InvalidOperationException("No available apps found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!Main.ActionGuid.IsUpdateInstance)
                    MessageBoxEx.Show(Lang.GetText(nameof(en_US.NoServerAvailableMsg)), Main.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Main.ApplicationExit(1);
                return;
            }

            try
            {
                Main.SearchAppUpdates();
                if (!Main.AppsDbSections.Any())
                    throw new WarningException("No updates available.");
                AppsList_SetContent(Main.AppsDbSections);
                if (MessageBoxEx.Show(string.Format(Lang.GetText(nameof(en_US.UpdatesAvailableMsg)), appsList.Items.Count, appsList.Items.Count == 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), Main.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                    throw new WarningException("Update canceled.");
                foreach (ListViewItem item in appsList.Items)
                    item.Checked = true;
            }
            catch (WarningException ex)
            {
                Log.Write(ex.Message);
                Main.ApplicationExit(2);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Main.ApplicationExit(1);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            _notifyBox?.Close();
            LoadSettings();
            var timer = new Timer(components)
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    AppsList_ResizeColumns();
                    Opacity += .05d;
                    return;
                }
                timer.Dispose();
            };
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            appsList.BeginUpdate();
            appsList.Visible = false;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            appsList.EndUpdate();
            appsList.Visible = true;
            AppsList_ResizeColumns();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Main.DownloadInfo.Amount > 0 && MessageBoxEx.Show(Lang.GetText(nameof(en_US.AreYouSureMsg)), Main.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (_iniIsLoaded && !_iniIsDisabled)
            {
                if (WindowState != FormWindowState.Minimized)
                    Ini.WriteDirect("Settings", "X.Window.State", WindowState != FormWindowState.Normal ? (FormWindowState?)WindowState : null);
                if (WindowState != FormWindowState.Maximized)
                {
                    Ini.WriteDirect("Settings", "X.Window.Size.Width", Width != MinimumSize.Width ? (int?)Width : null);
                    Ini.WriteDirect("Settings", "X.Window.Size.Height", Height != MinimumSize.Height * 2 ? (int?)Height : null);
                }
                else
                {
                    Ini.WriteDirect("Settings", "X.Window.Size.Width", null);
                    Ini.WriteDirect("Settings", "X.Window.Size.Height", null);
                }
            }
            if (checkDownload.Enabled)
                checkDownload.Enabled = false;
            if (multiDownloader.Enabled)
                multiDownloader.Enabled = false;
            foreach (var transfer in Main.TransferManager.Values)
                transfer.CancelAsync();
            var appInstaller = Main.GetAllAppInstaller();
            if (appInstaller.Count > 0)
                appInstaller.ForEach(file => ProcessEx.SendHelper.WaitThenDelete(file));
        }

        private void LoadSettings()
        {
            if (Ini.Read("Settings", "X.Window.State", FormWindowState.Normal) == FormWindowState.Maximized)
                WindowState = FormWindowState.Maximized;
            if (WindowState != FormWindowState.Maximized)
            {
                var windowWidth = Ini.Read("Settings", "X.Window.Size.Width", MinimumSize.Width);
                if (windowWidth > MinimumSize.Width && windowWidth < MaximumSize.Width)
                    Width = windowWidth;
                if (windowWidth >= MaximumSize.Width)
                    Width = MaximumSize.Width;
                Left = MaximumSize.Width / 2 - Width / 2;
                switch (TaskBar.GetLocation())
                {
                    case TaskBar.Location.Left:
                        Left += TaskBar.GetSize();
                        break;
                    case TaskBar.Location.Right:
                        Left -= TaskBar.GetSize();
                        break;
                }
                var windowHeight = Ini.Read("Settings", "X.Window.Size.Height", MinimumSize.Height);
                if (windowHeight > MinimumSize.Height && windowHeight < MaximumSize.Height)
                    Height = windowHeight;
                if (windowHeight >= MaximumSize.Height)
                    Height = MaximumSize.Height;
                Top = MaximumSize.Height / 2 - Height / 2;
                switch (TaskBar.GetLocation())
                {
                    case TaskBar.Location.Top:
                        Top += TaskBar.GetSize();
                        break;
                    case TaskBar.Location.Bottom:
                        Top -= TaskBar.GetSize();
                        break;
                }
            }
            showGroupsCheck.Checked = Ini.Read("Settings", "X.ShowGroups", true);
            showColorsCheck.Checked = Ini.Read("Settings", "X.ShowGroupColors", false);
            highlightInstalledCheck.Checked = Ini.Read("Settings", "X.ShowInstalled", true);
            TopMost = false;
            Refresh();
            _iniIsLoaded = true;
        }

        private void AppsList_Enter(object sender, EventArgs e) =>
            AppsList_ShowColors(false);

        private void AppsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                if (!Main.AvailableProtocols.HasFlag(Main.InternetProtocols.Version4) && Main.AvailableProtocols.HasFlag(Main.InternetProtocols.Version6) && !appsList.Items[e.Index].Checked)
                {
                    var host = new Uri(Ini.Read(appsList.Items[e.Index].Name, "ArchivePath", Main.AppsDbPath)).Host;
                    if (host.ContainsEx(Resources.PaUrl, Resources.SfUrl))
                        MessageBox.Show(string.Format(Lang.GetText(nameof(en_US.AppInternetProtocolWarningMsg)), host), Main.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                var requiredApps = Ini.Read(appsList.Items[e.Index].Name, "Requires", Main.AppsDbPath);
                if (string.IsNullOrWhiteSpace(requiredApps))
                    return;
                if (!requiredApps.Contains(","))
                    requiredApps = $"{requiredApps},";
                foreach (var i in requiredApps.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(i))
                        continue;
                    var app = i;
                    if (i.Contains("|"))
                    {
                        var split = i.Split('|');
                        if (split.Length != 2 || !i.Contains("64"))
                            continue;
                        if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
                            continue;
                        app = Environment.Is64BitOperatingSystem ? split[0].Contains("64") ? split[0] : split[1] : !split[0].Contains("64") ? split[0] : split[1];
                    }
                    var appFlags = Main.AppFlags.Core | Main.AppFlags.Free | Main.AppFlags.Repack;
                    if (Main.SwData.IsEnabled)
                        appFlags |= Main.AppFlags.Share;
                    var installed = Main.GetInstalledApps(appFlags, true);
                    if (installed.Contains(app))
                        return;
                    foreach (ListViewItem item in appsList.Items)
                        if (item.Name.Equals(app))
                        {
                            item.Checked = e.NewValue == CheckState.Checked;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void AppsList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var transferIsBusy = Main.TransferManager.Values.Any(transfer => transfer.IsBusy);
            if (!multiDownloader.Enabled && !checkDownload.Enabled && !transferIsBusy)
                okBtn.Enabled = appsList.CheckedItems.Count > 0;
        }

        private void AppsList_ResizeColumns()
        {
            if (appsList.Columns.Count < 5)
                return;
            var staticColumnsWidth = SystemInformation.VerticalScrollBarWidth + 2;
            for (var i = 3; i < appsList.Columns.Count; i++)
                staticColumnsWidth += appsList.Columns[i].Width;
            var dynamicColumnsWidth = 0;
            while (dynamicColumnsWidth < appsList.Width - staticColumnsWidth)
                dynamicColumnsWidth++;
            for (var i = 0; i < 3; i++)
                appsList.Columns[i].Width = (int)Math.Ceiling(dynamicColumnsWidth / 100f * (i == 0 ? 35f : i == 1 ? 50f : 15f));
        }

        private void AppsList_ShowColors(bool searchResultColor = true)
        {
            if (searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = false;
            var installed = new List<string>();
            if (!highlightInstalledCheck.Checked)
                highlightInstalledCheck.Text = Lang.GetText(highlightInstalledCheck);
            else
            {
                var appFlags = Main.AppFlags.Core | Main.AppFlags.Free | Main.AppFlags.Repack;
                if (Main.SwData.IsEnabled)
                    appFlags |= Main.AppFlags.Share;
                installed = Main.GetInstalledApps(appFlags, true);
                highlightInstalledCheck.Text = $@"{Lang.GetText(highlightInstalledCheck)} ({installed.Count})";
            }
            appsList.SetDoubleBuffer(false);
            try
            {
                var darkList = appsList.BackColor.R + appsList.BackColor.G + appsList.BackColor.B < byte.MaxValue;
                foreach (ListViewItem item in appsList.Items)
                {
                    if (highlightInstalledCheck.Checked && installed.ContainsEx(item.Name))
                    {
                        item.Font = new Font(appsList.Font, FontStyle.Italic);
                        item.ForeColor = darkList ? Color.FromArgb(0xc0, 0xff, 0xc0) : Color.FromArgb(0x20, 0x40, 0x20);
                        if (searchResultColor && item.Group.Name.EqualsEx("listViewGroup0"))
                        {
                            item.BackColor = SystemColors.Highlight;
                            continue;
                        }
                        item.BackColor = darkList ? Color.FromArgb(0x20, 0x40, 0x20) : Color.FromArgb(0xc0, 0xff, 0xc0);
                        continue;
                    }
                    item.Font = appsList.Font;
                    if (searchResultColor && item.Group.Name.EqualsEx("listViewGroup0"))
                    {
                        item.ForeColor = SystemColors.HighlightText;
                        item.BackColor = SystemColors.Highlight;
                        continue;
                    }
                    item.ForeColor = appsList.ForeColor;
                    item.BackColor = appsList.BackColor;
                }
                if (!showColorsCheck.Checked)
                    return;
                foreach (ListViewItem item in appsList.Items)
                {
                    var backColor = item.BackColor;
                    switch (item.Group.Name)
                    {
                        case "listViewGroup0": // Search Result
                            continue;
                        case "listViewGroup1": // Accessibility
                            item.BackColor = Color.FromArgb(0xff, 0xff, 0x99);
                            break;
                        case "listViewGroup2": // Education
                            item.BackColor = Color.FromArgb(0xff, 0xff, 0xcc);
                            break;
                        case "listViewGroup3": // Development
                            item.BackColor = Color.FromArgb(0x77, 0x77, 0x99);
                            break;
                        case "listViewGroup4": // Office
                            item.BackColor = Color.FromArgb(0x88, 0xbb, 0xdd);
                            break;
                        case "listViewGroup5": // Internet
                            item.BackColor = Color.FromArgb(0xcc, 0x88, 0x66);
                            break;
                        case "listViewGroup6": // Graphics and Pictures	
                            item.BackColor = Color.FromArgb(0xff, 0xcc, 0xff);
                            break;
                        case "listViewGroup7": // Music and Video
                            item.BackColor = Color.FromArgb(0xcc, 0xcc, 0xff);
                            break;
                        case "listViewGroup8": // Security
                            item.BackColor = Color.FromArgb(0x66, 0xcc, 0x99);
                            break;
                        case "listViewGroup9": // Utilities
                            item.BackColor = Color.FromArgb(0x77, 0xbb, 0xbb);
                            break;
                        case "listViewGroup11": // *Advanced
                            item.BackColor = Color.FromArgb(0xff, 0x66, 0x66);
                            break;
                        case "listViewGroup12": // *Shareware
                            item.BackColor = Color.FromArgb(0xff, 0x66, 0xff);
                            break;
                    }
                    if (item.BackColor == backColor)
                        continue;
                    if (item.ForeColor != Color.Black)
                        item.ForeColor = Color.Black;

                    // Adjust bright colors when a dark Windows theme style is used
                    var lightItem = item.BackColor.R + item.BackColor.G + item.BackColor.B > byte.MaxValue * 2;
                    if (darkList && lightItem)
                        item.BackColor = Color.FromArgb((byte)(item.BackColor.R * 2), (byte)(item.BackColor.G / 2), (byte)(item.BackColor.B / 2));

                    // Highlight installed apps
                    if (highlightInstalledCheck.Checked && installed.ContainsEx(item.Name))
                        item.BackColor = Color.FromArgb(item.BackColor.R, (byte)(item.BackColor.G + 24), item.BackColor.B);
                }
            }
            finally
            {
                appsList.SetDoubleBuffer();
            }
        }

        private void AppsList_SetContent(IEnumerable<string> sections)
        {
            var index = 0;
            var dictPath = Path.Combine(Main.HomeDir, "Assets\\images.dat");
            var extractDir = Log.DebugMode > 0 ? Path.Combine(Main.HomeDir, "Assets\\extract_images") : null;
            Dictionary<string, Image> imgDict = null;
            Image defIcon = null;
            try
            {
                imgDict = File.ReadAllBytes(dictPath).DeserializeObject<Dictionary<string, Image>>();
                if (Main.SwData.IsEnabled)
                {
                    var path = PathEx.AltCombine(Main.SwData.ServerAddress, "AppImages.dat");
                    if (!NetEx.FileIsAvailable(path, Main.SwData.Username, Main.SwData.Password, 60000))
                        throw new PathNotFoundException(path);
                    var swImgDict = NetEx.Transfer.DownloadData(path, Main.SwData.Username, Main.SwData.Password).DeserializeObject<Dictionary<string, Image>>();
                    if (swImgDict != null)
                        foreach (var pair in swImgDict)
                            imgDict.Add(pair.Key, pair.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            foreach (var section in sections)
            {
                var nam = Ini.Read(section, "Name", Main.AppsDbPath);
                var des = Ini.Read(section, "Description", Main.AppsDbPath);
                var cat = Ini.Read(section, "Category", Main.AppsDbPath);
                var ver = Ini.Read(section, "Version", Main.AppsDbPath);
                var pat = Ini.Read(section, "ArchivePath", Main.AppsDbPath);
                var dls = Ini.Read(section, "DownloadSize", 1L, Main.AppsDbPath) * 1024 * 1024;
                var siz = Ini.Read(section, "InstallSize", 1L, Main.AppsDbPath) * 1024 * 1024;
                var adv = Ini.Read(section, "Advanced", false, Main.AppsDbPath);
                var src = "si13n7.com";
                if (pat.StartsWithEx("http"))
                    try
                    {
                        var host = new Uri(pat).Host;
                        while (host.Count(c => c == '.') > 1)
                            host = host.Split('.').Skip(1).Join('.');
                        src = host;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        continue;
                    }
                var item = new ListViewItem(nam) { Name = section };
                item.SubItems.Add(des);
                item.SubItems.Add(ver);
                item.SubItems.Add(dls.FormatDataSize(true, true, true));
                item.SubItems.Add(siz.FormatDataSize(true, true, true));
                item.SubItems.Add(src);
                item.ImageIndex = index;
                if (!Main.SwData.IsEnabled && section.EndsWith("###"))
                    continue;
                if (!string.IsNullOrWhiteSpace(cat))
                {
                    try
                    {
                        if (imgDict?.ContainsKey(section) == true)
                        {
                            var img = imgDict[section];
                            if (img != null)
                            {
                                imgList.Images.Add(section, img);
                                if (Log.DebugMode > 0 && Directory.Exists(extractDir))
                                    img.Save(Path.Combine(extractDir, section));
                            }
                        }
                        if (!imgList.Images.ContainsKey(section))
                            throw new PathNotFoundException(dictPath + ":\\" + section);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        if (defIcon == null)
                            defIcon = Resources.PortableAppsBox;
                        imgList.Images.Add(section, defIcon);
                    }
                    try
                    {
                        if (!section.EndsWith("###"))
                            foreach (ListViewGroup gr in appsList.Groups)
                            {
                                var enName = Lang.GetText("en-US", gr.Name);
                                if ((adv || !enName.EqualsEx(cat)) && !enName.EqualsEx("*Advanced"))
                                    continue;
                                appsList.Items.Add(item).Group = gr;
                                break;
                            }
                        else
                            appsList.Items.Add(item).Group = appsList.Groups[appsList.Groups.Count - 1];
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
                index++;
            }
            appsList.SmallImageList = imgList;
            AppsList_ShowColors();
            fileStatus.Text = string.Format(Lang.GetText(fileStatus), appsList.Items.Count, appsList.Items.Count == 1 ? Lang.GetText(nameof(en_US.App)) : Lang.GetText(nameof(en_US.Apps)));
        }

        private void AppMenu_Opening(object sender, CancelEventArgs e)
        {
            if (appsList.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            var isChecked = appsList.SelectedItems[0].Checked;
            appMenuItem1.Text = isChecked ? Lang.GetText(nameof(appMenuItem1) + "u") : Lang.GetText(nameof(appMenuItem1));
            appMenuItem2.Text = isChecked ? Lang.GetText(nameof(appMenuItem2) + "u") : Lang.GetText(nameof(appMenuItem2));
        }

        private void AppMenuItem_Click(object sender, EventArgs e)
        {
            if (appsList.SelectedItems.Count == 0)
                return;
            var owner = sender as ToolStripMenuItem;
            switch (owner?.Name)
            {
                case "appMenuItem1":
                    appsList.SelectedItems[0].Checked = !appsList.SelectedItems[0].Checked;
                    break;
                case "appMenuItem2":
                    var isChecked = !appsList.SelectedItems[0].Checked;
                    for (var i = 0; i < appsList.Items.Count; i++)
                    {
                        if (showGroupsCheck.Checked && appsList.SelectedItems[0].Group != appsList.Items[i].Group)
                            continue;
                        appsList.Items[i].Checked = isChecked;
                    }
                    break;
                case "appMenuItem3":
                    var url = Ini.Read(appsList.SelectedItems[0].Name, "Website", Main.AppsDbPath);
                    if (string.IsNullOrWhiteSpace(url))
                        url = Resources.SearchQueryUri + WebUtility.UrlEncode(appsList.SelectedItems[0].Text);
                    try
                    {
                        Process.Start(url);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    break;
            }
        }

        private void ShowGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            var owner = sender as CheckBox;
            if (owner == null)
                return;
            if (!_iniIsDisabled)
                Ini.WriteDirect("Settings", "X.ShowGroups", !owner.Checked ? (bool?)false : null);
            appsList.ShowGroups = owner.Checked;
        }

        private void ShowColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!_iniIsDisabled)
                Ini.WriteDirect("Settings", "X.ShowGroupColors", (sender as CheckBox)?.Checked == true ? (bool?)true : null);
            AppsList_ShowColors();
        }

        private void HighlightInstalledCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!_iniIsDisabled)
                Ini.WriteDirect("Settings", "X.HighlightInstalled", !(sender as CheckBox)?.Checked == true ? (bool?)false : null);
            AppsList_ShowColors();
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            var owner = sender as TextBox;
            if (string.IsNullOrWhiteSpace(owner?.Text))
                return;
            var tmp = owner.Text;
            owner.Text = string.Empty;
            owner.Text = tmp;
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                appsList.Select();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ResetSearch();
            var owner = sender as TextBox;
            if (string.IsNullOrWhiteSpace(owner?.Text))
                return;
            appsList.SetDoubleBuffer(false);
            try
            {
                foreach (ListViewItem item in appsList.Items)
                {
                    var description = item.SubItems[1];
                    if (description.Text.ContainsEx(owner.Text))
                    {
                        foreach (ListViewGroup group in appsList.Groups)
                            if (group.Name.EqualsEx("listViewGroup0"))
                            {
                                if (!group.Items.Contains(item))
                                    group.Items.Add(item);
                                if (!item.Selected)
                                    item.EnsureVisible();
                            }
                        continue;
                    }
                    if (!item.Text.ContainsEx(owner.Text))
                        continue;
                    foreach (ListViewGroup group in appsList.Groups)
                        if (group.Name.EqualsEx("listViewGroup0"))
                        {
                            if (!group.Items.Contains(item))
                                group.Items.Add(item);
                            if (!item.Selected)
                                item.EnsureVisible();
                        }
                }
            }
            finally
            {
                appsList.SetDoubleBuffer();
            }
            AppsList_ShowColors();
            if (_searchResultBlinkCount > 0)
                _searchResultBlinkCount = 0;
            if (!searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = true;
        }

        private void ResetSearch()
        {
            if (!showGroupsCheck.Checked)
                showGroupsCheck.Checked = true;
            if (_appListClone.Items.Count == 0)
            {
                foreach (ListViewGroup group in appsList.Groups)
                    _appListClone.Groups.Add(new ListViewGroup { Name = group.Name, Header = group.Header });
                foreach (ListViewItem item in appsList.Items)
                    _appListClone.Items.Add((ListViewItem)item.Clone());
            }
            for (var i = 0; i < appsList.Items.Count; i++)
            {
                var item = appsList.Items[i];
                var clone = _appListClone.Items[i];
                foreach (ListViewGroup group in appsList.Groups)
                    if (clone.Group.Name.Equals(group.Name))
                    {
                        if (!clone.Group.Name.Equals(item.Group.Name))
                            group.Items.Add(item);
                        break;
                    }
            }
            AppsList_ShowColors(false);
            appsList.Sort();
            foreach (ListViewGroup group in appsList.Groups)
            {
                if (group.Items.Count == 0)
                    continue;
                foreach (ListViewItem item in group.Items)
                {
                    item.EnsureVisible();
                    return;
                }
            }
        }

        private void SearchResultBlinker_Tick(object sender, EventArgs e)
        {
            var owner = sender as Timer;
            if (owner == null)
                return;
            if (owner.Enabled && _searchResultBlinkCount >= 5)
                owner.Enabled = false;
            appsList.SetDoubleBuffer(false);
            try
            {
                foreach (ListViewGroup group in appsList.Groups)
                {
                    if (!group.Name.EqualsEx("listViewGroup0"))
                        continue;
                    if (group.Items.Count > 0)
                        foreach (ListViewItem item in appsList.Items)
                        {
                            if (!item.Group.Name.Equals(group.Name))
                                continue;
                            if (!searchResultBlinker.Enabled || item.BackColor != SystemColors.Highlight)
                            {
                                item.BackColor = SystemColors.Highlight;
                                owner.Interval = 200;
                            }
                            else
                            {
                                item.BackColor = appsList.BackColor;
                                owner.Interval = 100;
                            }
                        }
                    else
                        owner.Enabled = false;
                }
            }
            finally
            {
                appsList.SetDoubleBuffer();
            }
            if (owner.Enabled)
                _searchResultBlinkCount++;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Button;
            if (owner == null)
                return;
            var transferIsBusy = Main.TransferManager.Values.Any(transfer => transfer.IsBusy);
            if (!owner.Enabled || appsList.Items.Count == 0 || transferIsBusy)
                return;
            owner.Enabled = false;
            searchBox.Text = string.Empty;
            foreach (ListViewItem item in appsList.Items)
            {
                if (item.Checked)
                    continue;
                item.Remove();
            }
            foreach (var filePath in Main.GetAllAppInstaller())
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            _iniIsDisabled = !owner.Enabled;
            Main.DownloadInfo.Count = 1;
            Main.DownloadInfo.Amount = appsList.CheckedItems.Count;
            appsList.HideSelection = !owner.Enabled;
            appsList.Enabled = owner.Enabled;
            appsList.Sort();
            appMenu.Enabled = owner.Enabled;
            showGroupsCheck.Checked = owner.Enabled;
            showGroupsCheck.Enabled = owner.Enabled;
            showColorsCheck.Enabled = owner.Enabled;
            highlightInstalledCheck.Enabled = owner.Enabled;
            searchBox.Enabled = owner.Enabled;
            cancelBtn.Enabled = owner.Enabled;
            downloadSpeed.Visible = !owner.Enabled;
            downloadProgress.Visible = !owner.Enabled;
            downloadReceived.Visible = !owner.Enabled;
            multiDownloader.Enabled = !owner.Enabled;
            _progressCircle.Active = !owner.Enabled;
            _progressCircle.Visible = !owner.Enabled;
        }

        private void MultiDownloader_Tick(object sender, EventArgs e)
        {
            lock (MultiDownloaderLocker)
            {
                multiDownloader.Enabled = false;
                foreach (ListViewItem item in appsList.Items)
                {
                    if (checkDownload.Enabled || !item.Checked || Main.DownloadStarter?.IsAlive == true)
                        continue;
                    if (!DownloadStopwatch.IsRunning)
                        DownloadStopwatch.Start();
                    if (!appStatusBorder.Visible)
                        appStatusBorder.Visible = true;
                    fileStatus.Text = string.Format(Lang.GetText(nameof(en_US.fileStatus1)), Main.DownloadInfo.Count, Main.DownloadInfo.Amount);
                    appStatus.Text = string.Format(Lang.GetText(nameof(en_US.appStatus)), item.Text);
                    urlStatus.Text = $@"{item.SubItems[item.SubItems.Count - 1].Text} ";
                    Text = $@"{fileStatus.Text} - {appStatus.Text}";
                    var archivePath = Ini.Read(item.Name, "ArchivePath", Main.AppsDbPath);
                    var archiveLangs = Ini.Read(item.Name, "AvailableArchiveLangs", Main.AppsDbPath);
                    if (!string.IsNullOrWhiteSpace(archiveLangs) && archiveLangs.Contains(","))
                    {
                        var defaultLang = archivePath.ContainsEx("Multilingual") ? "Multilingual" : "English";
                        archiveLangs = $"Default ({defaultLang}),{archiveLangs}";
                        var archiveLang = Ini.Read(item.Name, "ArchiveLang");
                        var archiveLangConfirmed = Ini.Read(item.Name, "ArchiveLangConfirmed", true);
                        if (!archiveLangs.ContainsEx(archiveLang) || !archiveLangConfirmed)
                            try
                            {
                                var result = DialogResult.None;
                                while (result != DialogResult.OK)
                                    using (Form dialog = new LangSelectionForm(item.Name, item.Text, archiveLangs.Split(',')))
                                    {
                                        result = dialog.ShowDialog();
                                        if (result == DialogResult.OK)
                                            break;
                                        if (MessageBoxEx.Show(Lang.GetText(nameof(en_US.AreYouSureMsg)), Main.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                                            continue;
                                        Main.DownloadInfo.Amount = 0;
                                        Main.ApplicationExit();
                                        return;
                                    }
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        archiveLang = Ini.Read(item.Name, "ArchiveLang");
                        if (archiveLang.StartsWithEx("Default"))
                            archiveLang = string.Empty;
                        if (!string.IsNullOrWhiteSpace(archiveLang))
                            archivePath = Ini.Read(item.Name, $"ArchivePath_{archiveLang}", Main.AppsDbPath);
                    }
                    string localArchivePath;
                    if (!archivePath.StartsWithEx("http"))
                        localArchivePath = Path.Combine(Main.HomeDir, item.Group.Header.EqualsEx("*Shareware") ? "Apps\\.share" : "Apps", archivePath.Replace("/", "\\"));
                    else
                    {
                        var tmp = archivePath.Split('/');
                        if (tmp.Length == 0)
                            continue;
                        localArchivePath = Path.Combine(Main.HomeDir, $"Apps\\{tmp[tmp.Length - 1]}");
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(localArchivePath)))
                    {
                        var dir = Path.GetDirectoryName(localArchivePath);
                        if (!string.IsNullOrEmpty(dir))
                            Directory.CreateDirectory(dir);
                    }
                    Main.DownloadInfo.IsFinishedTick = 0;
                    Main.LastTransferItem = item.Text;
                    if (Main.TransferManager.ContainsKey(Main.LastTransferItem))
                        Main.TransferManager[Main.LastTransferItem].CancelAsync();
                    if (!Main.TransferManager.ContainsKey(Main.LastTransferItem))
                        Main.TransferManager.Add(Main.LastTransferItem, new NetEx.AsyncTransfer());
                    Main.DownloadStarter = new Thread(() => Main.StartDownload(item.Name, archivePath, localArchivePath, item.Group.Header)) { IsBackground = true };
                    Main.DownloadStarter.Start();
                    Main.DownloadInfo.Count++;
                    checkDownload.Enabled = true;
                    item.Checked = false;
                }
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            lock (CheckDownloadLocker)
            {
                if (!fileStatusBorder.Visible)
                    fileStatusBorder.Visible = true;
                timeStatus.Text = string.Format(Lang.GetText(nameof(en_US.timeStatus)), DownloadStopwatch.Elapsed.ToString("mm\\:ss\\.fff"));
                downloadSpeed.Text = Main.TransferManager[Main.LastTransferItem].TransferSpeedAd;
                downloadReceived.Text = Main.TransferManager[Main.LastTransferItem].DataReceived;
                DownloadProgress_Update(Main.TransferManager[Main.LastTransferItem].ProgressPercentage);
                if (Main.DownloadStarter?.IsAlive == false && !Main.TransferManager[Main.LastTransferItem].IsBusy)
                    Main.DownloadInfo.IsFinishedTick++;
                if (Main.DownloadInfo.IsFinishedTick < 10)
                    return;
                checkDownload.Enabled = false;
                if (appsList.CheckedItems.Count > 0)
                {
                    multiDownloader.Enabled = true;
                    return;
                }
                Text = Main.Text;
                fileStatus.Text = string.Empty;
                fileStatusBorder.Visible = false;
                appStatus.Text = string.Empty;
                appStatusBorder.Visible = false;
                timeStatus.Text = string.Empty;
                urlStatus.Text = string.Empty;
                downloadSpeed.Visible = false;
                downloadReceived.Visible = false;
                DownloadStopwatch.Stop();
                TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
                var installations = Main.StartInstall(this);
                var downloadFails = Main.TransferManager.Where(transfer => transfer.Value.HasCanceled).ToList();
                var keysOfFailed = downloadFails.Select(transfer => transfer.Key).ToList();
                if (downloadFails.Count > 0)
                {
                    TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Error);
                    if (downloadFails.All(transfer => transfer.Value?.Address?.ToString().ContainsEx("SourceForge") == true))
                        Main.DownloadInfo.MaxTries = Main.ExternalSfMirrors.Count - 1;
                    var retryFailed = Main.DownloadInfo.Retries < Main.DownloadInfo.MaxTries;
                    var warnDialog = DialogResult.None;
                    if (!retryFailed)
                    {
                        if (WindowState == FormWindowState.Minimized)
                            WindowState = Ini.Read("Settings", "X.Window.State", FormWindowState.Normal);
                        warnDialog = MessageBoxEx.Show(string.Format(Lang.GetText(nameof(en_US.DownloadErrorMsg)), (downloadFails.Count > 1 ? nameof(en_US.Apps) : nameof(en_US.App)).ToLower(), keysOfFailed.Join(Environment.NewLine)), Main.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                    }
                    if (retryFailed || warnDialog == DialogResult.Retry)
                    {
                        Main.DownloadInfo.Retries++;
                        foreach (ListViewItem item in appsList.Items)
                            if (keysOfFailed.ContainsEx(item.Text))
                                item.Checked = true;
                        Main.DownloadInfo.Count = 0;
                        appsList.Enabled = true;
                        appsList.HideSelection = !appsList.Enabled;
                        downloadSpeed.Text = string.Empty;
                        DownloadProgress_Update(0);
                        downloadProgress.Visible = !appsList.Enabled;
                        downloadReceived.Text = string.Empty;
                        showGroupsCheck.Enabled = appsList.Enabled;
                        showColorsCheck.Enabled = appsList.Enabled;
                        highlightInstalledCheck.Enabled = appsList.Enabled;
                        searchBox.Enabled = appsList.Enabled;
                        okBtn.Enabled = appsList.Enabled;
                        cancelBtn.Enabled = appsList.Enabled;
                        _progressCircle.Active = !appsList.Enabled;
                        _progressCircle.Visible = !appsList.Enabled;
                        OkBtn_Click(okBtn, null);
                        return;
                    }
                    if (warnDialog == DialogResult.Cancel)
                    {
                        Ini.ReadAll();
                        foreach (var key in keysOfFailed)
                        {
                            var section = appsList.Items.Cast<ListViewItem>().Where(item => key.Equals(item.Text)).Select(item => item.Name).FirstOrDefault();
                            if (string.IsNullOrEmpty(section))
                                continue;
                            Ini.Write(section, "NoUpdates", true);
                            Ini.Write(section, "NoUpdatesTime", DateTime.Now);
                        }
                        Ini.WriteAll();
                    }
                }
                else
                {
                    _progressCircle.Active = false;
                    _progressCircle.Visible = false;
                    downloadSpeed.Visible = false;
                    downloadProgress.Visible = false;
                    downloadReceived.Visible = false;
                    TaskBar.Progress.SetValue(Handle, 100, 100);
                    if (WindowState == FormWindowState.Minimized)
                        WindowState = Ini.Read("Settings", "X.Window.State", FormWindowState.Normal);
                    MessageBoxEx.Show(string.Format(Lang.GetText(nameof(en_US.SuccessfullyDownloadMsg)), installations == 1 ? Lang.GetText(nameof(en_US.App)) : Lang.GetText(nameof(en_US.Apps)), Main.ActionGuid.IsUpdateInstance ? Lang.GetText(nameof(en_US.SuccessfullyDownloadMsg1)) : Lang.GetText(nameof(en_US.SuccessfullyDownloadMsg2))), Main.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Main.DownloadInfo.Amount = 0;
                Main.ApplicationExit();
            }
        }

        private void DownloadProgress_Update(int value)
        {
            if (value == 0)
            {
                if (!_progressCircle.Visible)
                    _progressCircle.Visible = true;
                TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            }
            else
            {
                if (_progressCircle.Visible)
                    _progressCircle.Visible = false;
                TaskBar.Progress.SetValue(Handle, value, 100);
            }
            var color = Color.FromArgb(byte.MaxValue - (byte)(value * (byte.MaxValue / 100f)), byte.MaxValue, value);
            using (var g = downloadProgress.CreateGraphics())
            {
                var width = value > 0 && value < 100 ? (int)Math.Round(downloadProgress.Width / 100d * value, MidpointRounding.AwayFromZero) : downloadProgress.Width;
                using (Brush b = new SolidBrush(value > 0 ? color : downloadProgress.BackColor))
                    g.FillRectangle(b, 0, 0, width, downloadProgress.Height);
            }
            fileStatus.ForeColor = color;
            appStatus.ForeColor = color;
            timeStatus.ForeColor = color;
            urlStatus.ForeColor = color;
            downloadSpeed.ForeColor = color;
            downloadReceived.ForeColor = color;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            Main.ApplicationExit();

        private void UrlStatus_Click(object sender, EventArgs e) =>
            ProcessEx.Start(PathEx.AltCombine("http", (sender as Label)?.Text));

        private void Status_TextChanged(object sender, EventArgs e)
        {
            var owner = sender as Label;
            if (owner == null)
                return;
            if (!string.IsNullOrEmpty(appStatus.Text))
            {
                if (!owner.Text.StartsWith(" "))
                {
                    owner.Text = $@" {owner.Text}";
                    return;
                }
                if (!owner.Text.EndsWith(" "))
                {
                    owner.Text = $@"{owner.Text} ";
                    return;
                }
            }
            var size = TextRenderer.MeasureText(owner.Text, owner.Font, owner.Size);
            if (owner.Width == size.Width)
                return;
            if (size.Width <= 0)
                size.Width = 1;
            owner.Width = size.Width;
        }

        protected override void WndProc(ref Message m)
        {
            var previous = (int)WindowState;
            base.WndProc(ref m);
            var current = (int)WindowState;
            if (previous == 1 || current == 1 || previous == current)
                return;
            AppsList_ResizeColumns();
        }
    }
}
