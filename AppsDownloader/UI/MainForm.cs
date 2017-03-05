namespace AppsDownloader.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;
    using Timer = System.Windows.Forms.Timer;

    public partial class MainForm : Form
    {
        private static readonly bool UpdateSearch = Environment.CommandLine.ContainsEx("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");
        private static readonly string HomeDir = PathEx.Combine(PathEx.LocalDir, "..");
        private static readonly string TmpDir = Path.Combine(HomeDir, "Documents\\.cache");
        private static readonly string AppsDbPath = Path.Combine(TmpDir, $"AppInfo{Convert.ToByte(UpdateSearch)}.ini");

        // Helps for search function
        private readonly ListView _appListClone = new ListView();

        // List of available SourceForge.net mirrors
        private readonly string[] _externalMirrors =
        {
            "downloads.sourceforge.net",
            "netcologne.dl.sourceforge.net",
            "freefr.dl.sourceforge.net",
            "heanet.dl.sourceforge.net",
            "kent.dl.sourceforge.net",
            "vorboss.dl.sourceforge.net",
            "netix.dl.sourceforge.net"
        };

        // Organizes Si13n7.com mirrors
        private readonly List<string> _internalMirrors = new List<string>();

        // Holds last used SorgeForge.net mirrors for each download to manage download fails
        private readonly Dictionary<string, List<string>> _lastExternalMirrors = new Dictionary<string, List<string>>();

        // Initializes the notify box you see at program start
        private readonly NotifyBox _notifyBox = new NotifyBox { Opacity = .8d };

        // Simplest way to manage multiple downloads
        private readonly Dictionary<string, NetEx.AsyncTransfer> _transferManager = new Dictionary<string, NetEx.AsyncTransfer>();

        // Various variables
        private List<string> _appsDbSections = new List<string>();
        private int _downloadFinished, _downloadCount, _downloadAmount, _downloadRetries, _searchResultBlinkCount;
        private string _formText, _lastTransferItem = string.Empty;
        private bool _ipv4, _ipv6, _settingsDisabled, _settingsLoaded;

        // Sorts SourgeForge.net mirrors by client connection at the first use
        private List<string> _sourceForgeMirrorsSorted = new List<string>();

        // Allows to add an alternate password protected server with portable apps
        private string _swPwd = Ini.ReadString("Host", "Pwd");
        private string _swSrv = Ini.ReadString("Host", "Srv");
        private string _swUsr = Ini.ReadString("Host", "Usr");

        public MainForm()
        {
            InitializeComponent();
            appsList.ListViewItemSorter = new ListViewEx.AlphanumericComparer();
            searchBox.DrawSearchSymbol(searchBox.ForeColor);
            if (!appsList.Focus())
                appsList.Select();
        }

        protected override void WndProc(ref Message m)
        {
            var previous = (int)WindowState;
            base.WndProc(ref m);
            var current = (int)WindowState;
            if (previous == 1 || current == 1 || previous == current)
                return;
            MainForm_ResizeBegin(this, EventArgs.Empty);
            MainForm_Resize(this, EventArgs.Empty);
            MainForm_ResizeEnd(this, EventArgs.Empty);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Icon = Resources.PortableApps_purple_64;
            MaximumSize = Screen.FromHandle(Handle).WorkingArea.Size;
#if !x86
            Text += @" (64-bit)";
#endif
            _formText = Text;
            Lang.SetControlLang(this);
            for (var i = 0; i < appsList.Columns.Count; i++)
                appsList.Columns[i].Text = Lang.GetText($"columnHeader{i + 1}");
            for (var i = 0; i < appsList.Groups.Count; i++)
                appsList.Groups[i].Header = Lang.GetText(appsList.Groups[i].Name);

            showColorsCheck.Left = showGroupsCheck.Right + 4;
            highlightInstalledCheck.Left = showColorsCheck.Right + 4;

            MessageBoxEx.TopMost = true;

            var internetIsAvailable = _ipv4 = NetEx.InternetIsAvailable();
            if (!internetIsAvailable)
            {
                internetIsAvailable = _ipv6 = NetEx.InternetIsAvailable(true);
                if (internetIsAvailable)
                    MessageBoxEx.Show(Lang.GetText("InternetProtocolWarningMsg"), _formText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (!internetIsAvailable)
            {
                if (!UpdateSearch)
                    MessageBoxEx.Show(Lang.GetText("InternetIsNotAvailableMsg"), _formText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            if (!UpdateSearch)
                _notifyBox.Show(Lang.GetText("DatabaseAccessMsg"), _formText, NotifyBox.NotifyBoxStartPosition.Center);

            // Get 'Si13n7.com' download mirrors
            var dnsInfo = new Dictionary<string, Dictionary<string, string>>();
            for (var i = 0; i < 3; i++)
            {
                if (!_ipv4 && _ipv6)
                {
                    dnsInfo = Ini.ReadAll(Resources.IPv6DNS, false);
                    break;
                }
                dnsInfo = Ini.ReadAll(NetEx.Transfer.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
                if (dnsInfo.Count == 0 && i < 2)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            if (dnsInfo.Count > 0)
                foreach (var section in dnsInfo.Keys)
                    try
                    {
                        var addr = dnsInfo[section][_ipv4 ? "addr" : "ipv6"];
                        if (string.IsNullOrEmpty(addr))
                            continue;
                        var domain = dnsInfo[section]["domain"];
                        if (string.IsNullOrEmpty(domain))
                            continue;
                        bool ssl;
                        bool.TryParse(dnsInfo[section]["ssl"], out ssl);
                        domain = ssl ? $"https://{domain}" : $"http://{domain}";
                        if (!_internalMirrors.ContainsEx(domain))
                            _internalMirrors.Add(domain);
                    }
                    catch (KeyNotFoundException) { }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

            if (!UpdateSearch && _internalMirrors.Count == 0)
            {
                MessageBoxEx.Show(Lang.GetText("NoServerAvailableMsg"), _formText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Encrypt host access data with AES-256 (safety ofc not guaranteed)
            if (!string.IsNullOrEmpty(_swSrv) && !string.IsNullOrEmpty(_swUsr) && !string.IsNullOrEmpty(_swPwd))
                try
                {
                    var winId = Win32_OperatingSystem.SerialNumber;
                    if (string.IsNullOrWhiteSpace(winId))
                        throw new PlatformNotSupportedException();
                    var aesPw = winId.EncryptToSha256();
                    if (_swSrv.StartsWithEx("http://", "https://"))
                    {
                        Ini.Write("Host", "Srv", _swSrv.EncryptToAes256(aesPw).EncodeToBase85());
                        Ini.Write("Host", "Usr", _swUsr.EncryptToAes256(aesPw).EncodeToBase85());
                        Ini.Write("Host", "Pwd", _swPwd.EncryptToAes256(aesPw).EncodeToBase85());
                    }
                    else
                    {
                        _swSrv = _swSrv.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                        _swUsr = _swUsr.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                        _swPwd = _swPwd.DecodeByteArrayFromBase85().DecryptFromAes256(aesPw).FromByteArrayToString();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            // Enforce database reset in certain cases
            var tmpAppsDbDir = Path.Combine(TmpDir, PathEx.GetTempDirName());
            var tmpAppsDbPath = Path.Combine(tmpAppsDbDir, "update.ini");
            var appsDbLastWriteTime = DateTime.Now.AddHours(1d);
            long appsDbLength = 0;
            if (!File.Exists(tmpAppsDbPath) && File.Exists(AppsDbPath))
                try
                {
                    var fi = new FileInfo(AppsDbPath);
                    appsDbLastWriteTime = fi.LastWriteTime;
                    appsDbLength = fi.Length;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            if (UpdateSearch || File.Exists(tmpAppsDbPath) || (DateTime.Now - appsDbLastWriteTime).TotalHours >= 1d || appsDbLength < 0x23000 || (_appsDbSections = Ini.GetSections(AppsDbPath)).Count < 400)
                try
                {
                    if (File.Exists(AppsDbPath))
                        File.Delete(AppsDbPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            try
            {
                if (!File.Exists(AppsDbPath))
                {
                    if (!Directory.Exists(tmpAppsDbDir))
                    {
                        Directory.CreateDirectory(tmpAppsDbDir);
                        AppDomain.CurrentDomain.ProcessExit += (s, args) =>
                        {
                            try
                            {
                                Directory.Delete(tmpAppsDbDir, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        };
                    }

                    // Get internal app database
                    for (var i = 0; i < 3; i++)
                    {
                        NetEx.Transfer.DownloadFile(_ipv4 ? "https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/AppInfo.ini" : $"{_internalMirrors[0]}/Downloads/Portable%20Apps%20Suite/.free/AppInfo.ini", AppsDbPath);
                        if (!File.Exists(AppsDbPath))
                        {
                            if (i > 1)
                                throw new InvalidOperationException("Server connection failed.");
                            Thread.Sleep(1000);
                            continue;
                        }
                        break;
                    }

                    // Get external app database
                    var externDbPath = Path.Combine(tmpAppsDbDir, "AppInfo.7z");
                    string[] externDbSrvs =
                    {
                        "Downloads/Portable%20Apps%20Suite/.free/PortableAppsInfo.7z",
                        "portableapps.com/updater/update.7z"
                    };
                    var internCheck = false;
                    foreach (var srv in externDbSrvs)
                    {
                        long length = 0;
                        if (!internCheck)
                        {
                            internCheck = true;
                            foreach (var mirror in _internalMirrors)
                            {
                                string tmpSrv = $"{mirror}/{srv}";
                                if (!NetEx.FileIsAvailable(tmpSrv, 60000))
                                    continue;
                                NetEx.Transfer.DownloadFile(tmpSrv, externDbPath);
                                if (!File.Exists(externDbPath))
                                    continue;
                                length = new FileInfo(externDbPath).Length;
                                if (File.Exists(externDbPath) && length > 0x6000)
                                    break;
                            }
                        }
                        else
                        {
                            NetEx.Transfer.DownloadFile(srv, externDbPath);
                            if (File.Exists(externDbPath))
                                length = new FileInfo(externDbPath).Length;
                        }
                        if (File.Exists(externDbPath) && length > 0x6000)
                            break;
                    }

                    // Merge databases
                    _appsDbSections = Ini.GetSections(AppsDbPath);
                    if (File.Exists(externDbPath))
                    {
                        using (var p = Compaction.Zip7Helper.Unzip(externDbPath, tmpAppsDbDir))
                            if (!p?.HasExited == true)
                                p?.WaitForExit();
                        File.Delete(externDbPath);
                        externDbPath = tmpAppsDbPath;
                        if (File.Exists(externDbPath))
                        {
                            foreach (var section in Ini.GetSections(externDbPath))
                            {
                                if (_appsDbSections.ContainsEx(section) || section.ContainsEx("PortableApps.com", "ByPortableApps"))
                                    continue;
                                var nam = Ini.Read(section, "Name", externDbPath);
                                if (string.IsNullOrWhiteSpace(nam) || nam.ContainsEx("jPortable Launcher"))
                                    continue;
                                if (!nam.StartsWithEx("jPortable", "PortableApps.com"))
                                {
                                    var tmp = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
                                    tmp = Regex.Replace(tmp, @"\s+", " ");
                                    if (!string.IsNullOrWhiteSpace(tmp) && tmp != nam)
                                        nam = tmp.Trim().TrimEnd(',');
                                }
                                var des = Ini.Read(section, "Description", externDbPath);
                                if (string.IsNullOrWhiteSpace(des))
                                    continue;
                                var cat = Ini.Read(section, "Category", externDbPath);
                                if (string.IsNullOrWhiteSpace(cat))
                                    continue;
                                var ver = Ini.Read(section, "DisplayVersion", externDbPath);
                                if (string.IsNullOrWhiteSpace(ver))
                                    continue;
                                var pat = Ini.Read(section, "DownloadPath", externDbPath);
                                pat = $"{(string.IsNullOrWhiteSpace(pat) ? "http://downloads.sourceforge.net/portableapps" : pat)}/{Ini.Read(section, "DownloadFile", externDbPath)}";
                                if (!pat.EndsWithEx(".paf.exe"))
                                    continue;
                                var has = Ini.Read(section, "Hash", externDbPath);
                                if (string.IsNullOrWhiteSpace(has))
                                    continue;
                                var phs = new Dictionary<string, List<string>>();
                                foreach (var lang in Lang.GetText("availableLangs").Split(','))
                                {
                                    if (string.IsNullOrWhiteSpace(lang))
                                        continue;
                                    var tmpFile = Ini.Read(section, $"DownloadFile_{lang}", externDbPath);
                                    if (string.IsNullOrWhiteSpace(tmpFile))
                                        continue;
                                    var tmphash = Ini.Read(section, $"Hash_{lang}", externDbPath);
                                    if (string.IsNullOrWhiteSpace(tmphash) || !string.IsNullOrWhiteSpace(tmphash) && tmphash == has)
                                        continue;
                                    var tmpPath = Ini.Read(section, "DownloadPath", externDbPath);
                                    tmpFile = $"{(string.IsNullOrWhiteSpace(tmpPath) ? "http://downloads.sourceforge.net/portableapps" : tmpPath)}/{tmpFile}";
                                    phs.Add(lang, new List<string> { tmpFile, tmphash });
                                }
                                var dis = Ini.ReadLong(section, "DownloadSize", 1, externDbPath);
                                var siz = Ini.ReadLong(section, "InstallSizeTo", externDbPath);
                                if (siz == 0)
                                    siz = Ini.ReadLong(section, "InstallSize", 1, externDbPath);
                                var adv = Ini.Read(section, "Advanced", externDbPath);
                                File.AppendAllText(AppsDbPath, Environment.NewLine);
                                Ini.Write(section, "Name", nam, AppsDbPath);
                                Ini.Write(section, "Description", des, AppsDbPath);
                                Ini.Write(section, "Category", cat, AppsDbPath);
                                Ini.Write(section, "Version", ver, AppsDbPath);
                                Ini.Write(section, "ArchivePath", pat, AppsDbPath);
                                Ini.Write(section, "ArchiveHash", has, AppsDbPath);
                                if (phs.Count > 0)
                                {
                                    Ini.Write(section, "AvailableArchiveLangs", phs.Keys.Join(","), AppsDbPath);
                                    foreach (var item in phs)
                                    {
                                        Ini.Write(section, $"ArchivePath_{item.Key}", item.Value[0], AppsDbPath);
                                        Ini.Write(section, $"ArchiveHash_{item.Key}", item.Value[1], AppsDbPath);
                                    }
                                }
                                Ini.Write(section, "DownloadSize", dis, AppsDbPath);
                                Ini.Write(section, "InstallSize", siz, AppsDbPath);
                                if (adv.EqualsEx("true"))
                                    Ini.Write(section, "Advanced", true, AppsDbPath);
                            }
                        }

                        // Add another external app database for unpublished stuff - requires host access data
                        if (!string.IsNullOrEmpty(_swSrv) && !string.IsNullOrEmpty(_swUsr) && !string.IsNullOrEmpty(_swPwd))
                            try
                            {
                                var externDb = NetEx.Transfer.DownloadString($"{_swSrv}/AppInfo.ini", _swUsr, _swPwd);
                                if (!string.IsNullOrWhiteSpace(externDb))
                                    File.AppendAllText(AppsDbPath, $@"{Environment.NewLine}{externDb}");
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }

                        // Done with database
                        File.WriteAllText(AppsDbPath, File.ReadAllText(AppsDbPath).FormatNewLine());

                        // Get available apps
                        _appsDbSections = Ini.GetSections(AppsDbPath);
                        if (_appsDbSections.Count == 0)
                            throw new InvalidOperationException("No available apps found.");
                    }
                }
                if (!UpdateSearch)
                {
                    if (File.Exists(AppsDbPath))
                        AppsList_SetContent(_appsDbSections);
                    if (appsList.Items.Count == 0)
                        throw new InvalidOperationException("No available apps found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!UpdateSearch)
                    MessageBoxEx.Show(Lang.GetText("NoServerAvailableMsg"), _formText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Search updates
            try
            {
                var outdatedApps = new List<string>();
                foreach (var dir in GetInstalledApps(!string.IsNullOrEmpty(_swSrv) && !string.IsNullOrEmpty(_swUsr) && !string.IsNullOrEmpty(_swPwd) ? 3 : 0))
                {
                    var section = Path.GetFileName(dir);
                    if (Ini.ReadBoolean(section, "NoUpdates"))
                    {
                        if ((DateTime.Now - Ini.ReadDateTime(section, "NoUpdatesTime")).TotalDays <= 7d)
                            continue;
                        Ini.RemoveKey(section, "NoUpdates");
                        Ini.RemoveKey(section, "NoUpdatesTime");
                    }
                    if (dir.ContainsEx("\\.share\\"))
                        section = $"{section}###";
                    if (!_appsDbSections.ContainsEx(section))
                        continue;
                    var fileData = new Dictionary<string, string>();
                    var verData = Ini.Read(section, "VersionData", AppsDbPath);
                    var verHash = Ini.Read(section, "VersionHash", AppsDbPath);
                    if (!string.IsNullOrWhiteSpace(verData) && !string.IsNullOrWhiteSpace(verHash))
                    {
                        if (!verData.Contains(","))
                            verData = $"{verData},";
                        var verDataSplit = verData.Split(',');
                        if (!verHash.Contains(","))
                            verHash = $"{verHash},";
                        var verHashSplit = verHash.Split(',');
                        if (verDataSplit.Length != verHashSplit.Length)
                            continue;
                        for (var i = 0; i < verDataSplit.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(verDataSplit[i]) || string.IsNullOrWhiteSpace(verHashSplit[i]))
                                continue;
                            fileData.Add(verDataSplit[i], verHashSplit[i]);
                        }
                    }
                    if (fileData.Count > 0)
                    {
                        if (fileData.Select(data => new { data, filePath = Path.Combine(dir, data.Key) })
                                    .Where(x => File.Exists(x.filePath))
                                    .Where(x => Crypto.EncryptFileToSha256(x.filePath) != x.data.Value)
                                    .Select(x => x.data).Any())
                            if (!outdatedApps.ContainsEx(section))
                                outdatedApps.Add(section);
                        continue;
                    }
                    if (dir.ContainsEx("\\.share\\"))
                        continue;
                    var appIniPath = Path.Combine(dir, "App\\AppInfo\\appinfo.ini");
                    if (!File.Exists(appIniPath))
                        continue;
                    var localVer = Ini.ReadVersion("Version", "DisplayVersion", appIniPath);
                    var serverVer = Ini.ReadVersion(section, "Version", AppsDbPath);
                    if (localVer >= serverVer)
                        continue;
                    Log.Write($"Update for '{section}' found (Local: '{localVer}'; Server: '{serverVer}').");
                    if (!outdatedApps.ContainsEx(section))
                        outdatedApps.Add(section);
                }
                if (outdatedApps.Count == 0)
                    throw new WarningException("No updates available.");
                AppsList_SetContent(outdatedApps);
                if (MessageBoxEx.Show(string.Format(Lang.GetText("UpdatesAvailableMsg"), appsList.Items.Count, appsList.Items.Count == 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), _formText, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                    throw new WarningException("Update canceled.");
                foreach (ListViewItem item in appsList.Items)
                    item.Checked = true;
            }
            catch (WarningException ex)
            {
                Log.Write(ex.Message);
                Environment.ExitCode = 0;
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Environment.ExitCode = 1;
                Application.Exit();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            _notifyBox?.Close();
            LoadSettings();
            var timer = new Timer
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    Opacity += .1d;
                    return;
                }
                timer.Dispose();
            };
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) =>
            appsList.BeginUpdate();

        private void MainForm_ResizeEnd(object sender, EventArgs e) =>
            appsList.EndUpdate();

        private void MainForm_Resize(object sender, EventArgs e)
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_downloadAmount > 0 && MessageBoxEx.Show(Lang.GetText("AreYouSureMsg"), _formText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (_settingsLoaded && !_settingsDisabled)
            {
                if (WindowState != FormWindowState.Minimized)
                    Ini.Write("Settings", "X.Window.State", WindowState != FormWindowState.Normal ? (FormWindowState?)WindowState : null);
                if (WindowState != FormWindowState.Maximized)
                {
                    Ini.Write("Settings", "X.Window.Size.Width", Width != MinimumSize.Width ? (int?)Width : null);
                    Ini.Write("Settings", "X.Window.Size.Height", Height != MinimumSize.Height * 2 ? (int?)Height : null);
                }
                else
                {
                    Ini.RemoveKey("Settings", "X.Window.Size.Width");
                    Ini.RemoveKey("Settings", "X.Window.Size.Height");
                }
            }
            if (checkDownload.Enabled)
                checkDownload.Enabled = false;
            if (multiDownloader.Enabled)
                multiDownloader.Enabled = false;
            foreach (var transfer in _transferManager.Values)
                transfer.CancelAsync();
            var appInstaller = GetAllAppInstaller();
            if (appInstaller.Count > 0)
                ProcessEx.Send($"PING 127.0.0.1 -n 3 >NUL && DEL /F /Q \"{appInstaller.Join("\" && DEL /F /Q \"")}\"");
        }

        private void LoadSettings()
        {
            if (Ini.Read("Settings", "X.Window.State") == FormWindowState.Maximized.ToString())
                WindowState = FormWindowState.Maximized;
            if (WindowState != FormWindowState.Maximized)
            {
                var windowWidth = Ini.ReadInteger("Settings", "X.Window.Size.Width", MinimumSize.Width);
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
                var windowHeight = Ini.ReadInteger("Settings", "X.Window.Size.Height", MinimumSize.Height);
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
            showGroupsCheck.Checked = Ini.ReadBoolean("Settings", "X.ShowGroups", true);
            showColorsCheck.Checked = Ini.ReadBoolean("Settings", "X.ShowGroupColors");
            highlightInstalledCheck.Checked = Ini.ReadBoolean("Settings", "X.ShowInstalled", true);
            TopMost = false;
            Refresh();
            _settingsLoaded = true;
        }

        private static List<string> GetAllAppInstaller()
        {
            var list = new List<string>();
            try
            {
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps"), "*.paf.exe", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.repack"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.free"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.share"), "*.7z", SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return list;
        }

        private static List<string> GetInstalledApps(int index = 1, bool sections = false)
        {
            var list = new List<string>();
            try
            {
                list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps"), "*", SearchOption.TopDirectoryOnly).Where(s => !s.StartsWith(".")).ToList());
                list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.free"), "*", SearchOption.TopDirectoryOnly));
                if (index > 0 && index < 3)
                    list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.repack"), "*", SearchOption.TopDirectoryOnly));
                if (index > 1)
                    list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.share"), "*", SearchOption.TopDirectoryOnly));
                try
                {
                    list = list.Where(x => Directory.GetFiles(x, "*.exe", SearchOption.TopDirectoryOnly).Length > 0 ||
                                           Directory.GetFiles(x, Path.GetFileNameWithoutExtension(x) + ".ini", SearchOption.TopDirectoryOnly).Length > 0 &&
                                           Directory.GetFiles(x, "*.exe", SearchOption.AllDirectories).Length > 0).ToList();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                if (sections)
                {
                    var swPath = Path.Combine(HomeDir, "Apps\\.share");
                    list = list.Select(x => x.StartsWithEx(swPath) ? $"{Path.GetFileName(x)}###" : Path.GetFileName(x)).ToList();
                    foreach (var s in new[] { "Java", "Java64" })
                    {
                        var jPath = Path.Combine(HomeDir, $"Apps\\CommonFiles\\{s}\\bin\\java.exe");
                        if (!list.ContainsEx(s) && File.Exists(jPath))
                            list.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return list;
        }

        private void AppsList_Enter(object sender, EventArgs e) =>
            AppsList_ShowColors(false);

        private void AppsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                if (!_ipv4 && _ipv6 && !appsList.Items[e.Index].Checked)
                {
                    var host = new Uri(Ini.Read(appsList.Items[e.Index].Name, "ArchivePath", AppsDbPath)).Host;
                    if (host.ContainsEx("portableapps.com", "sourceforge.net"))
                        MessageBox.Show(string.Format(Lang.GetText("AppInternetProtocolWarningMsg"), host), _formText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                var requiredApps = Ini.Read(appsList.Items[e.Index].Name, "Requires", AppsDbPath);
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
                    foreach (ListViewItem item in appsList.Items)
                        if (item.Name == app)
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
            var transferIsBusy = _transferManager.Values.Any(transfer => transfer.IsBusy);
            if (!multiDownloader.Enabled && !checkDownload.Enabled && !transferIsBusy)
                okBtn.Enabled = appsList.CheckedItems.Count > 0;
        }

        private void AppsList_ShowColors(bool searchResultColor = true)
        {
            if (searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = false;
            var installed = new List<string>();
            if (highlightInstalledCheck.Checked)
                installed = GetInstalledApps(!string.IsNullOrEmpty(_swSrv) && !string.IsNullOrEmpty(_swUsr) && !string.IsNullOrEmpty(_swPwd) ? 3 : 0, true);
            var darkList = appsList.BackColor.R + appsList.BackColor.G + appsList.BackColor.B < byte.MaxValue;
            foreach (ListViewItem item in appsList.Items)
            {
                if (highlightInstalledCheck.Checked && installed.ContainsEx(item.Name))
                {
                    item.Font = new Font(appsList.Font, FontStyle.Italic);
                    item.ForeColor = darkList ? Color.FromArgb(0xc0, 0xff, 0xc0) : Color.FromArgb(0x20, 0x40, 0x20);
                    if (searchResultColor && item.Group.Name == "listViewGroup0")
                    {
                        item.BackColor = SystemColors.Highlight;
                        continue;
                    }
                    item.BackColor = darkList ? Color.FromArgb(0x20, 0x40, 0x20) : Color.FromArgb(0xc0, 0xff, 0xc0);
                    continue;
                }
                item.Font = appsList.Font;
                if (searchResultColor && item.Group.Name == "listViewGroup0")
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

        private void AppsList_SetContent(IEnumerable<string> sections)
        {
            Image defIcon = Resources.PortableAppsBox;
            var index = 0;
            var icoDbPath = Path.Combine(HomeDir, "Assets\\icon.db");
            byte[] icoDb = null;
            byte[] swIcoDb = null;
            try
            {
                icoDb = File.ReadAllBytes(icoDbPath);
                if (!string.IsNullOrEmpty(_swSrv) && !string.IsNullOrEmpty(_swUsr) && !string.IsNullOrEmpty(_swPwd))
                    swIcoDb = NetEx.Transfer.DownloadData($"{_swSrv}/AppIcon.db", _swUsr, _swPwd);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            foreach (var section in sections)
            {
                var nam = Ini.Read(section, "Name", AppsDbPath);
                var des = Ini.Read(section, "Description", AppsDbPath);
                var cat = Ini.Read(section, "Category", AppsDbPath);
                var ver = Ini.Read(section, "Version", AppsDbPath);
                var pat = Ini.Read(section, "ArchivePath", AppsDbPath);
                var dls = Ini.ReadLong(section, "DownloadSize", 1, AppsDbPath) * 1024 * 1024;
                var siz = Ini.ReadLong(section, "InstallSize", 1, AppsDbPath) * 1024 * 1024;
                var adv = Ini.ReadBoolean(section, "Advanced", AppsDbPath);
                var src = "si13n7.com";
                if (pat.StartsWithEx("http"))
                    try
                    {
                        var tmpHost = new Uri(pat).Host;
                        var tmpSplit = tmpHost.Split('.');
                        src = tmpSplit.Length >= 3 ? string.Join(".", tmpSplit[tmpSplit.Length - 2], tmpSplit[tmpSplit.Length - 1]) : tmpHost;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        continue;
                    }

                // Description filter
                switch (section)
                {
                    case "LibreCADPortable":
                        des = des.LowerText("tool");
                        break;
                    case "Mp3spltPortable":
                        des = des.UpperText("mp3", "ogg");
                        break;
                    case "SumatraPDFPortable":
                        des = des.LowerText("comic", "book", "e-", "reader");
                        break;
                    case "WinCDEmuPortable":
                        des = des.UpperText("cd/dvd/bd");
                        break;
                    case "WinDjViewPortable":
                        des = des.UpperText("djvu");
                        break;
                }
                des = $"{des.Substring(0, 1).ToUpper()}{des.Substring(1)}";

                var item = new ListViewItem(nam) { Name = section };
                item.SubItems.Add(des);
                item.SubItems.Add(ver);
                item.SubItems.Add(dls.FormatDataSize(true, true, true));
                item.SubItems.Add(siz.FormatDataSize(true, true, true));
                item.SubItems.Add(src);
                item.ImageIndex = index;
                if (section.EndsWith("###") && (string.IsNullOrEmpty(_swSrv) || string.IsNullOrEmpty(_swUsr) || string.IsNullOrEmpty(_swPwd)))
                    continue;
                if (!string.IsNullOrWhiteSpace(cat))
                {
                    try
                    {
                        var nameHash = (section.EndsWith("###") ? section.Substring(0, section.Length - 3) : section).EncryptToMd5();
                        if (icoDb == null)
                            throw new ArgumentNullException(nameof(icoDb));
                        foreach (var db in new[] { icoDb, swIcoDb })
                        {
                            if (db == null)
                                continue;
                            using (var stream = new MemoryStream(db))
                                try
                                {
                                    using (var archive = new ZipArchive(stream))
                                        foreach (var entry in archive.Entries)
                                        {
                                            if (entry.Name != nameHash)
                                                continue;
                                            imgList.Images.Add(nameHash, Image.FromStream(entry.Open()));
                                            break;
                                        }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                        }
                        if (!imgList.Images.ContainsKey(nameHash))
                            throw new PathNotFoundException(icoDbPath + ":" + nameHash + ">>" + section);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        imgList.Images.Add(defIcon);
                    }
                    try
                    {
                        if (!section.EndsWith("###"))
                            foreach (ListViewGroup gr in appsList.Groups)
                            {
                                if ((adv || !Lang.GetText("en-US", gr.Name).EqualsEx(cat)) && !Lang.GetText("en-US", gr.Name).EqualsEx("*Advanced"))
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
            appStatus.Text = string.Format(Lang.GetText(appStatus), appsList.Items.Count, appsList.Items.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"));
        }

        private void ShowGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            var owner = sender as CheckBox;
            if (owner == null)
                return;
            if (!_settingsDisabled)
                Ini.Write("Settings", "X.ShowGroups", !owner.Checked ? (bool?)false : null);
            appsList.ShowGroups = owner.Checked;
        }

        private void ShowColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!_settingsDisabled)
                Ini.Write("Settings", "X.ShowGroupColors", (sender as CheckBox)?.Checked == true ? (bool?)true : null);
            AppsList_ShowColors();
        }

        private void HighlightInstalledCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!_settingsDisabled)
                Ini.Write("Settings", "X.HighlightInstalled", !(sender as CheckBox)?.Checked == true ? (bool?)false : null);
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
            foreach (ListViewItem item in appsList.Items)
            {
                var description = item.SubItems[1];
                if (description.Text.ContainsEx(owner.Text))
                {
                    foreach (ListViewGroup group in appsList.Groups)
                        if (group.Name == "listViewGroup0")
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
                    if (group.Name == "listViewGroup0")
                    {
                        if (!group.Items.Contains(item))
                            group.Items.Add(item);
                        if (!item.Selected)
                            item.EnsureVisible();
                    }
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
                    _appListClone.Groups.Add(new ListViewGroup
                    {
                        Name = group.Name,
                        Header = group.Header
                    });
                foreach (ListViewItem item in appsList.Items)
                    _appListClone.Items.Add((ListViewItem)item.Clone());
            }
            for (var i = 0; i < appsList.Items.Count; i++)
            {
                var item = appsList.Items[i];
                var clone = _appListClone.Items[i];
                foreach (ListViewGroup group in appsList.Groups)
                    if (clone.Group.Name == group.Name)
                    {
                        if (clone.Group.Name != item.Group.Name)
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
            foreach (ListViewGroup group in appsList.Groups)
            {
                if (group.Name != "listViewGroup0")
                    continue;
                if (group.Items.Count > 0)
                    foreach (ListViewItem item in appsList.Items)
                    {
                        if (item.Group.Name != group.Name)
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
            if (owner.Enabled)
                _searchResultBlinkCount++;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Button;
            if (owner == null)
                return;
            var transferIsBusy = _transferManager.Values.Any(transfer => transfer.IsBusy);
            if (!owner.Enabled || appsList.Items.Count == 0 || transferIsBusy)
                return;
            owner.Enabled = false;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            searchBox.Text = string.Empty;
            foreach (ListViewItem item in appsList.Items)
            {
                if (item.Checked)
                    continue;
                item.Remove();
            }
            foreach (var filePath in GetAllAppInstaller())
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            _settingsDisabled = true;
            _downloadCount = 1;
            _downloadAmount = appsList.CheckedItems.Count;
            appsList.HideSelection = !owner.Enabled;
            appsList.Enabled = owner.Enabled;
            appsList.Sort();
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
        }

        private void MultiDownloader_Tick(object sender, EventArgs e)
        {
            multiDownloader.Enabled = false;
            foreach (ListViewItem item in appsList.Items)
            {
                if (checkDownload.Enabled || !item.Checked)
                    continue;
                Text = string.Format(Lang.GetText("StatusDownload"), _downloadCount, _downloadAmount, item.Text);
                appStatus.Text = Text;
                urlStatus.Text = item.SubItems[item.SubItems.Count - 1].Text;
                var archivePath = Ini.Read(item.Name, "ArchivePath", AppsDbPath);
                var archiveLangs = Ini.Read(item.Name, "AvailableArchiveLangs", AppsDbPath);
                if (!string.IsNullOrWhiteSpace(archiveLangs) && archiveLangs.Contains(","))
                {
                    var defaultLang = archivePath.ContainsEx("Multilingual") ? "Multilingual" : "English";
                    archiveLangs = $"Default ({defaultLang}),{archiveLangs}";
                    var archiveLang = Ini.Read(item.Name, "ArchiveLang");
                    var archiveLangConfirmed = Ini.ReadBoolean(item.Name, "ArchiveLangConfirmed");
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
                                    if (MessageBoxEx.Show(Lang.GetText("AreYouSureMsg"), _formText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                                        continue;
                                    _downloadAmount = 0;
                                    Application.Exit();
                                    return;
                                }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    archiveLang = Ini.Read(item.Name, "ArchiveLang");
                    if (archiveLang.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                        archiveLang = string.Empty;
                    if (!string.IsNullOrWhiteSpace(archiveLang))
                        archivePath = Ini.Read(item.Name, $"ArchivePath_{archiveLang}", AppsDbPath);
                }
                string localArchivePath;
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    localArchivePath = Path.Combine(HomeDir, item.Group.Header == "*Shareware" ? "Apps\\.share" : "Apps", archivePath.Replace("/", "\\"));
                else
                {
                    var tmp = archivePath.Split('/');
                    if (tmp.Length == 0)
                        continue;
                    localArchivePath = Path.Combine(HomeDir, $"Apps\\{tmp[tmp.Length - 1]}");
                }
                if (!Directory.Exists(Path.GetDirectoryName(localArchivePath)))
                {
                    var dir = Path.GetDirectoryName(localArchivePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                }
                _downloadFinished = 0;
                _lastTransferItem = item.Text;
                if (_transferManager.ContainsKey(_lastTransferItem))
                    _transferManager[_lastTransferItem].CancelAsync();
                if (!_transferManager.ContainsKey(_lastTransferItem))
                    _transferManager.Add(_lastTransferItem, new NetEx.AsyncTransfer());
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Group.Header == "*Shareware")
                        _transferManager[_lastTransferItem].DownloadFile($"{(_swSrv.EndsWith("/") ? _swSrv.Substring(0, _swSrv.Length - 1) : _swSrv)}/{archivePath}", localArchivePath, _swUsr, _swPwd);
                    else
                        foreach (var mirror in _internalMirrors)
                        {
                            string newArchivePath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{archivePath}";
                            if (!NetEx.FileIsAvailable(newArchivePath, 60000))
                                continue;
                            Log.Write($"{Path.GetFileName(newArchivePath)} has been found on '{mirror}'.");
                            _transferManager[_lastTransferItem].DownloadFile(newArchivePath, localArchivePath);
                            break;
                        }
                }
                else
                {
                    if (archivePath.ContainsEx("sourceforge"))
                    {
                        var newArchivePath = archivePath;
                        if (_sourceForgeMirrorsSorted.Count == 0)
                        {
                            var sortHelper = new Dictionary<string, long>();
                            foreach (var mirror in _externalMirrors)
                            {
                                var path = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                NetEx.Ping(path);
                                if (!sortHelper.Keys.ContainsEx(mirror))
                                    sortHelper.Add(mirror, NetEx.LastPingReply.RoundtripTime);
                            }
                            _sourceForgeMirrorsSorted = sortHelper.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
                        }
                        if (_sourceForgeMirrorsSorted.Count > 0)
                            foreach (var mirror in _sourceForgeMirrorsSorted)
                            {
                                if (_downloadRetries < _sourceForgeMirrorsSorted.Count - 1 && _lastExternalMirrors.ContainsKey(item.Name) && _lastExternalMirrors[item.Name].ContainsEx(mirror))
                                    continue;
                                newArchivePath = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                if (!NetEx.FileIsAvailable(newArchivePath, 60000))
                                    continue;
                                if (!_lastExternalMirrors.ContainsKey(item.Name))
                                    _lastExternalMirrors.Add(item.Name, new List<string> { mirror });
                                else
                                    _lastExternalMirrors[item.Name].Add(mirror);
                                Log.Write($"'{Path.GetFileName(newArchivePath)}' has been found at '{mirror}'.");
                                break;
                            }
                        _transferManager[_lastTransferItem].DownloadFile(newArchivePath, localArchivePath);
                    }
                    else
                        _transferManager[_lastTransferItem].DownloadFile(archivePath, localArchivePath, 60000, "Mozilla/5.0");
                }
                _downloadCount++;
                item.Checked = false;
                checkDownload.Enabled = true;
                break;
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            downloadSpeed.Text = _transferManager[_lastTransferItem].TransferSpeedAd;
            downloadReceived.Text = _transferManager[_lastTransferItem].DataReceived;
            DownloadProgress_Update(_transferManager[_lastTransferItem].ProgressPercentage);
            if (!_transferManager[_lastTransferItem].IsBusy)
                _downloadFinished++;
            if (_downloadFinished < 10)
                return;
            checkDownload.Enabled = false;
            if (appsList.CheckedItems.Count > 0)
            {
                multiDownloader.Enabled = true;
                return;
            }
            Text = _formText;
            appStatus.Text = Lang.GetText("StatusExtract");
            urlStatus.Text = @"si13n7.com";
            downloadSpeed.Visible = false;
            downloadReceived.Visible = false;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            var appInstaller = GetAllAppInstaller();
            foreach (var filePath in appInstaller)
            {
                if (!File.Exists(filePath))
                    continue;
                var appDir = string.Empty;
                if (!filePath.EndsWithEx(".paf.exe"))
                {
                    var fDir = Path.GetDirectoryName(filePath);
                    var fName = Path.GetFileNameWithoutExtension(filePath);
                    if (!fName.StartsWith("_") && fName.Contains("_"))
                        fName = fName.Split('_')[0];
                    appDir = Path.Combine(fDir, fName);
                }
                else
                    foreach (var dir in GetInstalledApps())
                        if (Path.GetFileName(filePath).StartsWithEx(Path.GetFileName(dir)))
                        {
                            appDir = dir;
                            break;
                        }

                // Close running apps before overwrite
                var taskList = new List<string>();
                if (Directory.Exists(appDir))
                {
                    var locks = Data.GetLocks(appDir);
                    if (locks.Count > 0)
                    {
                        var result = MessageBoxEx.Show(string.Format(Lang.GetText("FileLocksMsg"), locks.Count == 1 ? Lang.GetText("FileLocksMsg1") : Lang.GetText("FileLocksMsg2"), $"{locks.Select(p => p.ProcessName).Join($".exe; {Environment.NewLine}")}.exe"), _formText, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        if (result != DialogResult.OK)
                            continue;
                    }
                    foreach (var p in locks)
                    {
                        try
                        {
                            if (p.MainWindowHandle != IntPtr.Zero)
                            {
                                p.CloseMainWindow();
                                p.WaitForExit(100);
                            }
                            if (!p.HasExited)
                                p.Kill();
                            if (p.HasExited)
                                continue;
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        string fileName = $"{p.ProcessName}.exe";
                        if (!taskList.ContainsEx(fileName))
                            taskList.Add(fileName);
                    }
                    if (taskList.Count > 0)
                        using (var p = ProcessEx.Send($"TASKKILL /F /IM \"{taskList.Join("\" && TASKKILL /F /IM \"")}\"", true, false))
                            if (p != null && !p.HasExited)
                                p.WaitForExit();
                    WinApi.RefreshVisibleTrayArea();
                }

                // Install if file hashes are valid
                if (WindowState != FormWindowState.Minimized)
                    WindowState = FormWindowState.Minimized;
                foreach (var transfer in _transferManager)
                {
                    Process p = null;
                    try
                    {
                        if (transfer.Value.FilePath != filePath)
                            continue;
                        var fileName = Path.GetFileName(filePath);
                        foreach (var section in Ini.GetSections(AppsDbPath))
                        {
                            if (filePath.ContainsEx("\\.share\\") && !section.EndsWith("###"))
                                continue;
                            if (!Ini.Read(section, "ArchivePath", AppsDbPath).EndsWith(fileName))
                            {
                                var found = false;
                                var archiveLangs = Ini.Read(section, "AvailableArchiveLangs", AppsDbPath);
                                if (archiveLangs.Contains(","))
                                    foreach (var lang in archiveLangs.Split(','))
                                    {
                                        found = Ini.Read(section, $"ArchivePath_{lang}", AppsDbPath).EndsWith(fileName);
                                        if (found)
                                            break;
                                    }
                                if (!found)
                                    continue;
                            }
                            var archiveLang = Ini.Read(section, "ArchiveLang");
                            if (archiveLang.StartsWithEx("Default"))
                                archiveLang = string.Empty;
                            var archiveHash = !string.IsNullOrEmpty(archiveLang) ? Ini.Read(section, $"ArchiveHash_{archiveLang}", AppsDbPath) : Ini.Read(section, "ArchiveHash", AppsDbPath);
                            var localHash = Crypto.EncryptFileToMd5(filePath);
                            if (localHash == archiveHash)
                                break;
                            throw new InvalidOperationException($"Checksum is invalid. - Key: '{transfer.Key}'; Section: '{section}'; File: '{filePath}'; Current: '{archiveHash}'; Requires: '{localHash}';");
                        }
                        if (filePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                            (p = Compaction.Zip7Helper.Unzip(filePath, appDir, ProcessWindowStyle.Minimized))?.WaitForExit();
                        else
                            (p = ProcessEx.Start(filePath, Path.Combine(HomeDir, "Apps"), $"/DESTINATION=\"{Path.Combine(HomeDir, "Apps")}\\\"", false, false))?.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        transfer.Value.HasCanceled = true;
                    }
                    finally
                    {
                        p?.Dispose();
                    }
                    break;
                }
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
            if (WindowState == FormWindowState.Minimized)
                WindowState = Ini.Read("Settings", "X.Window.State").StartsWithEx("Max") ? FormWindowState.Maximized : FormWindowState.Normal;
            var downloadFails = _transferManager.Where(transfer => transfer.Value.HasCanceled)
                                                .Select(transfer => transfer.Key)
                                                .ToList();
            if (downloadFails.Count > 0)
            {
                TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Error);
                DialogResult errDialog;
                if (_downloadRetries < _sourceForgeMirrorsSorted.Count - 1 || (errDialog = MessageBoxEx.Show(string.Format(Lang.GetText("DownloadErrorMsg"), downloadFails.Join(Environment.NewLine)), _formText, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning)) == DialogResult.Retry)
                {
                    _downloadRetries++;
                    foreach (ListViewItem item in appsList.Items)
                        if (downloadFails.ContainsEx(item.Text))
                            item.Checked = true;
                    _downloadCount = 0;
                    appStatus.Text = string.Empty;
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
                    OkBtn_Click(okBtn, null);
                    return;
                }
                if (errDialog == DialogResult.Cancel)
                    foreach (var app in downloadFails)
                    {
                        var section = appsList.Items.Cast<ListViewItem>().Where(item => app.Equals(item.Text)).Select(item => item.Name).FirstOrDefault();
                        if (string.IsNullOrEmpty(section))
                            continue;
                        Ini.Write(section, "NoUpdates", true);
                        Ini.Write(section, "NoUpdatesTime", DateTime.Now);
                    }
            }
            else
            {
                TaskBar.Progress.SetValue(Handle, 100, 100);
                MessageBoxEx.Show(string.Format(Lang.GetText("SuccessfullyDownloadMsg"), appInstaller.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"), UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), _formText, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            _downloadAmount = 0;
            Application.Exit();
        }

        private void DownloadProgress_Update(int value)
        {
            var color = Color.FromArgb(byte.MaxValue - (byte)(value * 2.55f), byte.MaxValue, value);
            TaskBar.Progress.SetValue(Handle, value, 100);
            using (var g = downloadProgress.CreateGraphics())
            {
                var width = value > 0 && value < 100 ? (int)Math.Round(downloadProgress.Width / 100d * value, MidpointRounding.AwayFromZero) : downloadProgress.Width;
                using (Brush b = new SolidBrush(value > 0 ? color : downloadProgress.BackColor))
                    g.FillRectangle(b, 0, 0, width, downloadProgress.Height);
            }
            appStatus.ForeColor = color;
            urlStatus.ForeColor = color;
            downloadSpeed.ForeColor = color;
            downloadReceived.ForeColor = color;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            Application.Exit();

        private void UrlStatus_Click(object sender, EventArgs e) =>
            Process.Start($"http:\\{(sender as Label)?.Text}");
    }
}
