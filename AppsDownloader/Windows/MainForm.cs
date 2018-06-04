namespace AppsDownloader.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using LangResources;
    using Libraries;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public sealed partial class MainForm : Form
    {
        private static readonly object DownloadStarter = new object(),
                                       DownloadHandler = new object();

        public MainForm()
        {
            InitializeComponent();
            appsList.ListViewItemSorter = new ListViewEx.AlphanumericComparer();
            searchBox.DrawSearchSymbol(searchBox.ForeColor);
            if (!appsList.Focus())
                appsList.Select();
        }

        private ListView AppsListClone { get; } = new ListView();

        private CounterStorage Counter { get; } = new CounterStorage();

        private NotifyBox NotifyBox { get; } = new NotifyBox();

        private static Task TransferTask { get; set; }

        private Dictionary<ListViewItem, AppTransferor> TransferManager { get; } = new Dictionary<ListViewItem, AppTransferor>();

        private KeyValuePair<ListViewItem, AppTransferor> CurrentTransfer { get; set; }

        private Stopwatch TransferStopwatch { get; } = new Stopwatch();

        private List<AppData> TransferFails { get; } = new List<AppData>();

        private void MainForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            Icon = Resources.PortableApps_purple;

            MinimumSize = Settings.Window.Size.Minimum;
            MaximumSize = Settings.Window.Size.Maximum;
            if (Settings.Window.Size.Width > Settings.Window.Size.Minimum.Width)
                Width = Settings.Window.Size.Width;
            if (Settings.Window.Size.Height > Settings.Window.Size.Minimum.Height)
                Height = Settings.Window.Size.Height;
            WinApi.NativeHelper.CenterWindow(Handle);
            if (Settings.Window.State == FormWindowState.Maximized)
                WindowState = FormWindowState.Maximized;

            Text = Settings.Title;
            Language.SetControlLang(this);
            for (var i = 0; i < appsList.Columns.Count; i++)
                appsList.Columns[i].Text = Language.GetText($"columnHeader{i + 1}");
            for (var i = 0; i < appsList.Groups.Count; i++)
                appsList.Groups[i].Header = Language.GetText(appsList.Groups[i].Name);
            for (var i = 0; i < appMenu.Items.Count; i++)
                appMenu.Items[i].Text = Language.GetText(appMenu.Items[i].Name);
            var statusLabels = new[]
            {
                appStatusLabel,
                fileStatusLabel,
                urlStatusLabel,
                downloadReceivedLabel,
                downloadSpeedLabel,
                timeStatusLabel
            };
            foreach (var label in statusLabels)
                label.Text = Language.GetText(label.Name);

            showGroupsCheck.Checked = Settings.ShowGroups;
            showColorsCheck.Left = showGroupsCheck.Right + 4;
            showColorsCheck.Checked = Settings.ShowGroupColors;
            highlightInstalledCheck.Left = showColorsCheck.Right + 4;
            highlightInstalledCheck.Checked = Settings.HighlightInstalled;

            appsList.SetDoubleBuffer();
            appMenu.EnableAnimation();
            appMenu.SetFixedSingle();
            statusAreaLeftPanel.SetDoubleBuffer();
            statusAreaRightPanel.SetDoubleBuffer();

            if (!ActionGuid.IsUpdateInstance)
                NotifyBox.Show(Language.GetText(nameof(en_US.DatabaseAccessMsg)), Settings.Title, NotifyBox.NotifyBoxStartPosition.Center);

            if (!Network.InternetIsAvailable)
            {
                if (!ActionGuid.IsUpdateInstance)
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.InternetIsNotAvailableMsg)), Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ApplicationExit(1);
                return;
            }

            if (!ActionGuid.IsUpdateInstance && !AppSupply.GetMirrors(AppSupply.Suppliers.Internal).Any())
            {
                MessageBoxEx.Show(Language.GetText(nameof(en_US.NoServerAvailableMsg)), Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ApplicationExit(1);
                return;
            }

            try
            {
                if (!CacheData.AppImages.Any())
                    throw new InvalidOperationException("No app image found.");

                if (!CacheData.AppInfo.Any())
                    throw new InvalidOperationException("No app data found.");

                if (ActionGuid.IsUpdateInstance)
                {
                    var appUpdates = AppSupply.FindOutdatedApps();
                    if (!appUpdates.Any())
                        throw new WarningException("No updates available.");

                    AppsListUpdate(appUpdates);
                    if (appsList.Items.Count == 0)
                        throw new InvalidOperationException("No apps available.");

                    var asterisk = string.Format(Language.GetText(appsList.Items.Count == 1 ? nameof(en_US.AppUpdateAvailableMsg) : nameof(en_US.AppUpdatesAvailableMsg)), appsList.Items.Count);
                    if (MessageBoxEx.Show(asterisk, Settings.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                        throw new WarningException("Update canceled.");

                    foreach (var item in appsList.Items.Cast<ListViewItem>())
                        item.Checked = true;
                }
                else
                {
                    if (CacheData.AppInfo.Any())
                        AppsListUpdate();

                    if (appsList.Items.Count == 0)
                        throw new InvalidOperationException("No apps available.");
                }
            }
            catch (WarningException ex)
            {
                Log.Write(ex.Message);
                ApplicationExit(2);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!ActionGuid.IsUpdateInstance)
                    MessageBoxEx.Show(Language.GetText(nameof(en_US.NoServerAvailableMsg)), Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ApplicationExit(1);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            NotifyBox?.Close();
            TopMost = false;
            Refresh();
            var timer = new Timer(components)
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    AppsListResizeColumns();
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
            AppsListResizeColumns();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TransferManager.Any() && MessageBoxEx.Show(this, Language.GetText(nameof(en_US.AreYouSureMsg)), Settings.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            Settings.Window.State = WindowState;
            Settings.Window.Size.Width = Width;
            Settings.Window.Size.Height = Height;

            if (downloadHandler.Enabled)
                downloadHandler.Enabled = false;
            if (downloadStarter.Enabled)
                downloadStarter.Enabled = false;

            foreach (var appTransferor in TransferManager.Values)
                appTransferor.Transfer.CancelAsync();

            var appInstaller = AppSupply.FindAppInstaller();
            if (appInstaller.Any())
                appInstaller.ForEach(file => ProcessEx.SendHelper.WaitThenDelete(file));
        }

        private void AppsList_Enter(object sender, EventArgs e) =>
            AppsListShowColors(false);

        private void AppsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var appData = CacheData.AppInfo?.FirstOrDefault(x => x.Key.EqualsEx(appsList.Items[e.Index].Name));
            if (appData?.Requirements?.Any() != true)
                return;
            var installedApps = AppSupply.FindInstalledApps();
            foreach (var requirement in appData.Requirements)
            {
                if (installedApps.Any(x => x.Requirements.Contains(requirement)))
                    continue;
                foreach (var item in appsList.Items.Cast<ListViewItem>())
                {
                    if (!item.Name.Equals(requirement))
                        continue;
                    item.Checked = e.NewValue == CheckState.Checked;
                    break;
                }
            }
        }

        private void AppsList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!downloadStarter.Enabled && !downloadHandler.Enabled && !TransferManager.Values.Any(x => x.Transfer.IsBusy))
                startBtn.Enabled = appsList.CheckedItems.Count > 0;
        }

        private void AppsListReset()
        {
            if (AppsListClone.Items.Count != appsList.Items.Count)
            {
                AppsListShowColors(false);
                appsList.Sort();
                return;
            }
            for (var i = 0; i < appsList.Items.Count; i++)
            {
                var item = appsList.Items[i];
                var clone = AppsListClone.Items[i];
                foreach (var group in appsList.Groups.Cast<ListViewGroup>())
                    if (clone.Group.Name.Equals(group.Name))
                    {
                        if (!clone.Group.Name.Equals(item.Group.Name))
                            group.Items.Add(item);
                        break;
                    }
            }
            AppsListShowColors(false);
            appsList.Sort();
        }

        private void AppsListResizeColumns()
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
                appsList.Columns[i].Width = (int)Math.Ceiling(dynamicColumnsWidth / 100d * (i == 0 ? 35d : i == 1 ? 50d : 15d));
        }

        private void AppsListShowColors(bool searchResultColor = true)
        {
            if (searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = false;
            var appInfo = new List<AppData>();
            if (!highlightInstalledCheck.Checked)
                highlightInstalledCheck.Text = Language.GetText(highlightInstalledCheck);
            else
            {
                appInfo = AppSupply.FindInstalledApps();
                highlightInstalledCheck.Text = $@"{Language.GetText(highlightInstalledCheck)} ({appInfo.Count})";
            }
            appsList.SetDoubleBuffer(false);
            appsList.BeginUpdate();
            try
            {
                var darkList = appsList.BackColor.R + appsList.BackColor.G + appsList.BackColor.B < byte.MaxValue;
                foreach (var item in appsList.Items.Cast<ListViewItem>())
                {
                    if (highlightInstalledCheck.Checked && appInfo.Any(x => x.Key.EqualsEx(item.Name)))
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

                    // adjust bright colors if a dark Windows theme style is used
                    var lightItem = item.BackColor.R + item.BackColor.G + item.BackColor.B > byte.MaxValue * 2;
                    if (darkList && lightItem)
                        item.BackColor = Color.FromArgb((byte)(item.BackColor.R * 2), (byte)(item.BackColor.G / 2), (byte)(item.BackColor.B / 2));

                    // highlight installed apps
                    if (highlightInstalledCheck.Checked && appInfo.Any(x => x.Key.EqualsEx(item.Name)))
                        item.BackColor = Color.FromArgb(item.BackColor.R, (byte)(item.BackColor.G + 24), item.BackColor.B);
                }
            }
            finally
            {
                appsList.EndUpdate();
                appsList.SetDoubleBuffer();
            }
        }

        private void AppsListUpdate(List<AppData> appInfo = default(List<AppData>))
        {
            var index = 0;
            var appImages = CacheData.AppImages ?? new Dictionary<string, Image>();
            if (Shareware.Enabled)
                foreach (var srv in Shareware.GetAddresses())
                {
                    var usr = Shareware.GetUser(srv);
                    var pwd = Shareware.GetPassword(srv);
                    var url = PathEx.AltCombine(srv, "AppImages.dat");
                    if (Log.DebugMode > 0)
                        Log.Write($"Shareware: Looking for '{{{Shareware.FindAddressKey(srv).ToHexa()}}}/AppImages.dat'.");
                    if (!NetEx.FileIsAvailable(url, usr, pwd, 60000))
                        continue;
                    var swAppImages = NetEx.Transfer.DownloadData(url, usr, pwd)?.DeserializeObject<Dictionary<string, Image>>();
                    if (swAppImages == null)
                        continue;
                    foreach (var pair in swAppImages)
                    {
                        if (appImages.ContainsKey(pair.Key))
                            continue;
                        appImages.Add(pair.Key, pair.Value);
                    }
                }

            appsList.BeginUpdate();
            appsList.Items.Clear();
            foreach (var appData in appInfo == default(List<AppData>) ? CacheData.AppInfo : appInfo)
            {
                if (!Shareware.Enabled && appData.ServerKey != null)
                    continue;

                var url = appData.DownloadCollection.First().Value.First().Item1;
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                var src = AppSupply.SupplierHosts.Internal;
                if (url.StartsWithEx("http"))
                    if (url.ContainsEx(AppSupply.SupplierHosts.PortableApps) && url.ContainsEx("/redirect/"))
                        src = AppSupply.SupplierHosts.SourceForge;
                    else
                    {
                        src = url.GetShortHost();
                        if (string.IsNullOrEmpty(src))
                            continue;
                    }

                var item = new ListViewItem(appData.Name)
                {
                    Name = appData.Key
                };
                item.SubItems.Add(appData.Description);
                item.SubItems.Add(appData.DisplayVersion);
                item.SubItems.Add(appData.DownloadSize.FormatSize(Reorganize.SizeOptions.Trim));
                item.SubItems.Add(appData.InstallSize.FormatSize(Reorganize.SizeOptions.Trim));
                item.SubItems.Add(src);
                item.ImageIndex = index;

                if (appImages?.ContainsKey(appData.Key) == true)
                {
                    var img = appImages[appData.Key];
                    if (img != null)
                        imageList.Images.Add(appData.Key, img);
                }

                if (!imageList.Images.ContainsKey(appData.Key))
                {
                    if (Log.DebugMode == 0)
                        continue;
                    Log.Write($"Cache: Could not find target '{CachePaths.AppImages}:{appData.Key}'.");
                    appData.Advanced = true;
                    imageList.Images.Add(appData.Key, Resources.PortableAppsBox);
                }

                if (appData.ServerKey == null)
                    foreach (var group in appsList.Groups.Cast<ListViewGroup>())
                    {
                        var enName = Language.GetText("en-US", group.Name);
                        if ((appData.Advanced || !enName.EqualsEx(appData.Category)) && !enName.EqualsEx("*Advanced"))
                            continue;
                        appsList.Items.Add(item).Group = group;
                        break;
                    }
                else
                    appsList.Items.Add(item).Group = appsList.Groups.Cast<ListViewGroup>().Last();

                index++;
            }

            if (Log.DebugMode > 0)
                Log.Write($"Interface: {appsList.Items.Count} {(appsList.Items.Count == 1 ? "App" : "Apps")} has been added!");

            appsList.SmallImageList = imageList;
            appsList.EndUpdate();
            AppsListShowColors();

            AppsListClone.Groups.Clear();
            foreach (var group in appsList.Groups.Cast<ListViewGroup>())
                AppsListClone.Groups.Add(new ListViewGroup
                {
                    Name = group.Name,
                    Header = group.Header
                });
            AppsListClone.Items.Clear();
            foreach (var item in appsList.Items.Cast<ListViewItem>())
                AppsListClone.Items.Add((ListViewItem)item.Clone());
        }

        private void AppMenu_Opening(object sender, CancelEventArgs e)
        {
            if (appsList.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            var isChecked = appsList.SelectedItems[0].Checked;
            appMenuItem1.Text = isChecked ? Language.GetText(nameof(appMenuItem1) + "u") : Language.GetText(nameof(appMenuItem1));
            appMenuItem2.Text = isChecked ? Language.GetText(nameof(appMenuItem2) + "u") : Language.GetText(nameof(appMenuItem2));
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
                    var appData = CacheData.AppInfo.FirstOrDefault(x => x.Key.EqualsEx(appsList.SelectedItems[0].Name));
                    var website = appData?.Website;
                    if (website?.StartsWithEx("https://", "http://") != true)
                        return;
                    Process.Start(website);
                    break;
            }
        }

        private void ShowGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ShowGroups = (sender as CheckBox)?.Checked == true;
            appsList.ShowGroups = Settings.ShowGroups;
        }

        private void ShowColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ShowGroupColors = (sender as CheckBox)?.Checked == true;
            AppsListShowColors();
        }

        private void HighlightInstalledCheck_CheckedChanged(object sender, EventArgs e)
        {
            Settings.HighlightInstalled = (sender as CheckBox)?.Checked == true;
            AppsListShowColors();
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
            if (e.KeyCode != Keys.Enter)
                return;
            SearchReset();
            var owner = sender as TextBox;
            if (string.IsNullOrWhiteSpace(owner?.Text))
                return;
            appsList.SetDoubleBuffer(false);
            try
            {
                foreach (var item in appsList.Items.Cast<ListViewItem>())
                {
                    var description = item.SubItems[1];
                    if (description.Text.ContainsEx(owner.Text))
                    {
                        foreach (var group in appsList.Groups.Cast<ListViewGroup>())
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
                    foreach (var group in appsList.Groups.Cast<ListViewGroup>())
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
            AppsListShowColors();
            Counter.Reset(0);
            if (!searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = true;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var separators = Environment.NewLine.ToArray();
            if (!searchBox.Text.ContainsEx(separators))
                return;
            var text = searchBox.Text;
            searchBox.Text = text.RemoveChar(separators);
            searchBox.SelectionStart = searchBox.TextLength;
            searchBox.ScrollToCaret();
        }

        private void SearchResultBlinker_Tick(object sender, EventArgs e)
        {
            if (!(sender is Timer owner))
                return;
            if (owner.Enabled && Counter.GetValue(0) >= 5)
                owner.Enabled = false;
            appsList.SetDoubleBuffer(false);
            try
            {
                foreach (var group in appsList.Groups.Cast<ListViewGroup>())
                {
                    if (!group.Name.EqualsEx("listViewGroup0"))
                        continue;
                    if (group.Items.Count > 0)
                        foreach (var item in appsList.Items.Cast<ListViewItem>())
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
                Counter.Increase(0);
        }

        private void SearchReset()
        {
            if (!showGroupsCheck.Checked)
                showGroupsCheck.Checked = true;
            AppsListReset();
            AppsListShowColors(false);
            appsList.Sort();
            foreach (var group in appsList.Groups.Cast<ListViewGroup>())
            {
                if (group.Items.Count == 0)
                    continue;
                foreach (var item in group.Items.Cast<ListViewItem>())
                {
                    item.EnsureVisible();
                    return;
                }
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (!(sender is Button owner))
                return;

            var transferIsBusy = TransferManager.Values.Any(x => x.Transfer.IsBusy);
            if (!owner.Enabled || appsList.Items.Count == 0 || transferIsBusy)
                return;

            Settings.SkipWriteValue = true;

            owner.Enabled = false;
            searchBox.Text = string.Empty;

            appsList.BeginUpdate();
            foreach (var item in appsList.Items.Cast<ListViewItem>())
            {
                if (item.Checked)
                    continue;
                item.Remove();
            }
            appsList.EndUpdate();

            foreach (var filePath in AppSupply.FindAppInstaller())
                FileEx.TryDelete(filePath);

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

            TransferManager.Clear();
            var totalSize = 0L;
            foreach (var item in appsList.CheckedItems.Cast<ListViewItem>())
            {
                var appData = CacheData.AppInfo.FirstOrDefault(x => x.Key.EqualsEx(item.Name));
                if (appData == null)
                    continue;

                if (appData.DownloadCollection.Count > 1 && !appData.Settings.ArchiveLangConfirmed)
                    try
                    {
                        var result = DialogResult.None;
                        while (result != DialogResult.OK)
                            using (Form dialog = new LangSelectionForm(appData))
                            {
                                result = dialog.ShowDialog();
                                if (result == DialogResult.OK)
                                    break;
                                if (MessageBoxEx.Show(this, Language.GetText(nameof(en_US.AreYouSureMsg)), Settings.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                                    continue;
                                ApplicationExit();
                                return;
                            }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                TransferManager.Add(item, new AppTransferor(appData));

                unchecked
                {
                    totalSize += appData.DownloadSize;
                    totalSize += appData.InstallSize;
                }
            }

            var freeSpace = Settings.FreeDiskSpace;
            if (totalSize > freeSpace)
            {
                var warning = string.Format(Language.GetText(nameof(en_US.NotEnoughDiskSpaceMsg)), totalSize.FormatSize(), freeSpace.FormatSize());
                switch (MessageBoxEx.Show(this, warning, Settings.Title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning))
                {
                    case DialogResult.Abort:
                        TransferManager.Clear();
                        ApplicationExit();
                        break;
                    case DialogResult.Retry:
                        owner.Enabled = !owner.Enabled;
                        appsList.HideSelection = !owner.Enabled;
                        appsList.Enabled = owner.Enabled;
                        appMenu.Enabled = owner.Enabled;
                        cancelBtn.Enabled = owner.Enabled;
                        return;
                }
            }

            downloadStarter.Enabled = !owner.Enabled;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            ApplicationExit();

        private void DownloadStarter_Tick(object sender, EventArgs e)
        {
            if (!(sender is Timer owner))
                return;
            lock (DownloadStarter)
            {
                owner.Enabled = false;

                if (TransferManager.Any(x => x.Value.Transfer.IsBusy))
                    return;

                try
                {
                    CurrentTransfer = TransferManager.First(x => !TransferFails.Contains(x.Value.AppData) && (x.Key.Checked || x.Value.Transfer.HasCanceled));
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    ApplicationExit(1);
                    return;
                }

                var listViewItem = CurrentTransfer.Key;
                appStatus.Text = listViewItem.Text;
                urlStatus.Text = $@"{listViewItem.SubItems[listViewItem.SubItems.Count - 1].Text} ";
                Text = $@"{string.Format(Language.GetText(nameof(en_US.titleStatus)), TransferManager.Keys.Count(x => !x.Checked), TransferManager.Keys.Count)} - {appStatus.Text}";

                if (!TransferStopwatch.IsRunning)
                    TransferStopwatch.Start();

                var appTransferor = CurrentTransfer.Value;
                TransferTask?.Dispose();
                TransferTask = Task.Run(() => appTransferor.StartDownload());

                downloadHandler.Enabled = !owner.Enabled;
            }
        }

        private void DownloadHandler_Tick(object sender, EventArgs e)
        {
            if (!(sender is Timer owner))
                return;
            lock (DownloadHandler)
            {
                var appTransferor = CurrentTransfer.Value;
                DownloadProgressUpdate(appTransferor.Transfer.ProgressPercentage);

                if (!statusAreaBorder.Visible || !statusAreaPanel.Visible)
                {
                    SuspendLayout();
                    statusAreaBorder.Visible = true;
                    statusAreaPanel.Visible = true;
                    ResumeLayout();
                }

                statusAreaLeftPanel.SuspendLayout();
                fileStatus.Text = appTransferor.Transfer.FileName;
                if (string.IsNullOrEmpty(fileStatus.Text))
                    fileStatus.Text = Language.GetText(nameof(en_US.InitStatusText));
                fileStatus.Text = fileStatus.Text.Trim(fileStatus.Font, fileStatus.Width);
                statusAreaLeftPanel.ResumeLayout();

                statusAreaRightPanel.SuspendLayout();
                downloadReceived.Text = appTransferor.Transfer.DataReceived;
                if (downloadReceived.Text.EqualsEx("0.00 bytes / 0.00 bytes"))
                    downloadReceived.Text = Language.GetText(nameof(en_US.InitStatusText));
                downloadSpeed.Text = appTransferor.Transfer.TransferSpeedAd;
                if (downloadSpeed.Text.EqualsEx("0.00 bit/s"))
                    downloadSpeed.Text = Language.GetText(nameof(en_US.InitStatusText));
                timeStatus.Text = TransferStopwatch.Elapsed.ToString("mm\\:ss\\.fff");
                statusAreaRightPanel.ResumeLayout();

                if (TransferTask?.Status == TaskStatus.Running || appTransferor.Transfer.IsBusy)
                {
                    Counter.Reset(1);
                    return;
                }
                if (Counter.Increase(1) < (int)Math.Floor(1000d / owner.Interval))
                    return;

                owner.Enabled = false;
                if (!appTransferor.DownloadStarted)
                    TransferFails.Add(appTransferor.AppData);
                if (appsList.CheckedItems.Count > 0)
                {
                    var listViewItem = CurrentTransfer.Key;
                    listViewItem.Checked = false;
                    if (appsList.CheckedItems.Count > 0)
                    {
                        downloadStarter.Enabled = !owner.Enabled;
                        return;
                    }
                }

                var windowState = WindowState;
                WindowState = FormWindowState.Minimized;

                Text = Settings.Title;
                TransferStopwatch.Stop();
                TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);

                var installed = new List<ListViewItem>();
                installed.AddRange(TransferManager.Where(x => x.Value.StartInstall()).Select(x => x.Key));
                if (installed.Any())
                    foreach (var key in installed)
                        TransferManager.Remove(key);
                if (TransferManager.Any())
                    TransferFails.AddRange(TransferManager.Values.Select(x => x.AppData));

                TransferManager.Clear();
                CurrentTransfer = default(KeyValuePair<ListViewItem, AppTransferor>);

                WindowState = windowState;

                if (TransferFails.Any())
                {
                    TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Error);
                    var warning = string.Format(Language.GetText(TransferFails.Count == 1 ? nameof(en_US.AppDownloadErrorMsg) : nameof(en_US.AppsDownloadErrorMsg)), TransferFails.Select(x => x.Name).Join(Environment.NewLine));
                    switch (MessageBoxEx.Show(this, warning, Settings.Title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning))
                    {
                        case DialogResult.Retry:
                            foreach (var appData in TransferFails)
                            {
                                var item = appsList.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Name.EqualsEx(appData.Key));
                                if (item == default(ListViewItem))
                                    continue;
                                item.Checked = true;
                            }
                            TransferFails.Clear();

                            SuspendLayout();
                            appsList.Enabled = true;
                            appsList.HideSelection = !appsList.Enabled;

                            downloadSpeed.Text = string.Empty;
                            DownloadProgressUpdate(0);
                            downloadReceived.Text = string.Empty;

                            showGroupsCheck.Enabled = appsList.Enabled;
                            showColorsCheck.Enabled = appsList.Enabled;
                            highlightInstalledCheck.Enabled = appsList.Enabled;

                            searchBox.Enabled = appsList.Enabled;

                            startBtn.Enabled = appsList.Enabled;
                            cancelBtn.Enabled = appsList.Enabled;

                            statusAreaBorder.Visible = !appsList.Enabled;
                            statusAreaPanel.Visible = !appsList.Enabled;
                            ResumeLayout();

                            StartBtn_Click(startBtn, EventArgs.Empty);
                            return;
                        default:
                            if (ActionGuid.IsUpdateInstance)
                                foreach (var appData in TransferFails)
                                {
                                    appData.Settings.NoUpdates = true;
                                    appData.Settings.NoUpdatesTime = DateTime.Now;
                                }
                            break;
                    }
                }

                TaskBar.Progress.SetValue(Handle, 100, 100);
                var information = Language.GetText(ActionGuid.IsUpdateInstance ? installed.Count == 1 ? nameof(en_US.AppUpdatedMsg) : nameof(en_US.AppsUpdatedMsg) : installed.Count == 1 ? nameof(en_US.AppDownloadedMsg) : nameof(en_US.AppsDownloadedMsg));
                MessageBoxEx.Show(this, information, Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplicationExit();
            }
        }

        private void DownloadProgressUpdate(int value)
        {
            var color = PanelEx.FakeProgressBar.Update(downloadProgress, value);
            appStatus.ForeColor = color;
            fileStatus.ForeColor = color;
            urlStatus.ForeColor = color;
            downloadReceived.ForeColor = color;
            downloadSpeed.ForeColor = color;
            timeStatus.ForeColor = color;
        }

        private static void ApplicationExit(int exitCode = 0)
        {
            if (exitCode > 0)
            {
                Environment.ExitCode = exitCode;
                Environment.Exit(Environment.ExitCode);
            }
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            var previous = (int)WindowState;
            base.WndProc(ref m);
            var current = (int)WindowState;
            if (previous == 1 || current == 1 || previous == current)
                return;
            AppsListResizeColumns();
        }

        private class CounterStorage
        {
            private readonly Dictionary<int, int> _counter;

            public CounterStorage() =>
                _counter = new Dictionary<int, int>();

            private int Handler(int index, ActionState state)
            {
                if (!_counter.ContainsKey(index))
                    _counter.Add(index, 0);
                switch (state)
                {
                    case ActionState.Increase:
                        if (_counter[index] < int.MaxValue - 1)
                            _counter[index]++;
                        break;
                    case ActionState.Reset:
                        _counter[index] = 0;
                        break;
                }
                return _counter[index];
            }

            public int Increase(int index) =>
                Handler(index, ActionState.Increase);

            public void Reset(int index) =>
                Handler(index, ActionState.Reset);

            public int GetValue(int index) =>
                Handler(index, ActionState.Get);

            private enum ActionState
            {
                Increase,
                Reset,
                Get
            }
        }
    }
}
