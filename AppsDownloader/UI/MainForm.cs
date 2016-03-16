using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    public partial class MainForm : Form
    {
        string Title = string.Empty;
        SilDev.NotifyBox NotifyBox;
        bool SettingsLoaded = false;

        int SearchResultBlinkCount = 0;
        ListView AppListClone = new ListView();

        List<string> DownloadServers = new List<string>();
        readonly string DnsIniPath = Path.Combine(Application.StartupPath, "Helper\\DnsInfo.ini");
        readonly string AppsDBPath = Path.Combine(Application.StartupPath, "Helper\\AppInfo.ini");
        List<string> WebInfoSections = new List<string>();

        int DlAsyncIsDoneCounter = 0, DlCount = 0, DlAmount = 0, DlFailRetry = 0;
        List<string> SfMirrors = new List<string>();
        Dictionary<string, List<string>> SfLastMirrors = new Dictionary<string, List<string>>();

        readonly string HomeDir = Path.GetFullPath($"{Application.StartupPath}\\..");

        readonly string SWSrv = SilDev.Ini.Read("Host", "Srv");
        readonly string SWUsr = SilDev.Ini.Read("Host", "Usr");
        readonly string SWPwd = SilDev.Ini.Read("Host", "Pwd");

        readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_purple_64;
#if !x86
            Text = $"{Text} (64-bit)";
#endif
            Title = Text;
            appsList.Select();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            for (int i = 0; i < appsList.Columns.Count; i++)
                appsList.Columns[i].Text = Lang.GetText($"columnHeader{i + 1}");
            for (int i = 0; i < appsList.Groups.Count; i++)
                appsList.Groups[i].Header = Lang.GetText(appsList.Groups[i].Name);

            // Checking the connection to the internet
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
            {
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(this, Lang.GetText("InternetIsNotAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            if (!UpdateSearch)
            {
                NotifyBox = new SilDev.NotifyBox()
                {
                    BackColor = Color.FromArgb(64, 64, 64),
                    BorderColor = Color.SteelBlue,
                    CaptionColor = Color.LightSteelBlue,
                    TextColor = Color.FromArgb(224, 224, 224),
                    Opacity = .75d
                };
                NotifyBox.Show(Lang.GetText("DatabaseAccessMsg"), Text, SilDev.NotifyBox.NotifyBoxStartPosition.Center);
            }

            // Get internal download mirrors
            for (int i = 0; i < 3; i++)
            {
                SilDev.Network.DownloadFile("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini", DnsIniPath);
                if (!File.Exists(DnsIniPath) && i < 2)
                    Thread.Sleep(1000);
            }
            if (File.Exists(DnsIniPath))
            {
                foreach (string section in SilDev.Ini.GetSections(DnsIniPath))
                {
                    string addr = SilDev.Ini.ReadString(section, "addr", "s0.si13n7.com", DnsIniPath);
                    if (!DownloadServers.Contains(addr) && SilDev.Network.UrlIsValid(addr))
                    {
                        bool ssl = SilDev.Ini.ReadBoolean(section, "ssl", false, DnsIniPath);
                        DownloadServers.Add(ssl ? $"https://{addr}" : $"http://{addr}");
                    }
                }
            }
            if (!UpdateSearch && DownloadServers.Count == 0)
            {
                SilDev.MsgBox.Show(Lang.GetText("NoServerAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            // Enforce database reset in certain cases
            if (UpdateSearch || (DateTime.Now - new FileInfo(AppsDBPath).LastWriteTime).TotalHours >= 1d || (WebInfoSections = SilDev.Ini.GetSections(AppsDBPath)).Count == 0)
            {
                try
                {
                    if (File.Exists(AppsDBPath))
                        File.Delete(AppsDBPath);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }

            try
            {
                if (!File.Exists(AppsDBPath))
                {
                    // Get internal app database
                    for (int i = 0; i < 3; i++)
                    {
                        SilDev.Network.DownloadFile("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/AppInfo.ini", AppsDBPath);
                        if (!File.Exists(AppsDBPath))
                        {
                            if (i < 2)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            throw new Exception("Server connection failed.");
                        }
                        break;
                    }

                    // Get external app database
                    string ExternDBPath = Path.Combine(Application.StartupPath, "Helper\\AppInfo.7z");
                    string[] ExternDBSrvs = new string[]
                    {
                        "Downloads/Portable%20Apps%20Suite/.free/PortableAppsInfo.7z",
                        "portableapps.com/updater/update.7z",
                    };
                    bool internCheck = false;
                    foreach (string srv in ExternDBSrvs)
                    {
                        int length = 0;
                        if (!internCheck)
                        {
                            internCheck = true;
                            foreach (string mirror in DownloadServers)
                            {
                                string tmpSrv = $"{mirror}/{srv}";
                                if (!SilDev.Network.OnlineFileExists(tmpSrv))
                                    continue;
                                SilDev.Network.DownloadFile(tmpSrv, ExternDBPath);
                                if (File.Exists(ExternDBPath))
                                {
                                    length = (int)(new FileInfo(ExternDBPath).Length / 1024);
                                    if (File.Exists(ExternDBPath) && length > 24)
                                        break;
                                }
                            }
                        }
                        else
                        {
                            SilDev.Network.DownloadFile(srv, ExternDBPath);
                            if (File.Exists(ExternDBPath))
                                length = (int)(new FileInfo(ExternDBPath).Length / 1024);
                        }
                        if (File.Exists(ExternDBPath) && length > 24)
                            break;
                    }

                    // Merge databases
                    WebInfoSections = SilDev.Ini.GetSections(AppsDBPath);
                    if (File.Exists(ExternDBPath))
                    {
                        SilDev.Compress.Unzip7(ExternDBPath, Path.Combine(Application.StartupPath, "Helper"));
                        File.Delete(ExternDBPath);
                        ExternDBPath = Path.Combine(Application.StartupPath, "Helper\\update.ini");
                        if (File.Exists(ExternDBPath))
                        {
                            foreach (string section in SilDev.Ini.GetSections(ExternDBPath))
                            {
                                if (WebInfoSections.Contains(section))
                                    continue;
                                string nam = SilDev.Ini.Read(section, "Name", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(nam) || nam.Contains("PortableApps.com"))
                                    continue;
                                if (!nam.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase))
                                {
                                    string tmp = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
                                    tmp = Regex.Replace(tmp, @"\s+", " ");
                                    if (!string.IsNullOrWhiteSpace(tmp) && tmp != nam)
                                        nam = tmp.TrimEnd();
                                }
                                string des = SilDev.Ini.Read(section, "Description", ExternDBPath);
                                des = des.Replace(" Editor", " editor").Replace(" Reader", " reader");
                                if (string.IsNullOrWhiteSpace(des))
                                    continue;
                                string cat = SilDev.Ini.Read(section, "Category", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(cat))
                                    continue;
                                string ver = SilDev.Ini.Read(section, "DisplayVersion", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(ver))
                                    continue;
                                string pat = SilDev.Ini.Read(section, "DownloadPath", ExternDBPath);
                                pat = $"{(string.IsNullOrWhiteSpace(pat) ? "http://downloads.sourceforge.net/portableapps" : pat)}/{SilDev.Ini.Read(section, "DownloadFile", ExternDBPath)}";
                                if (!pat.EndsWith(".paf.exe", StringComparison.OrdinalIgnoreCase))
                                    continue;
                                string has = SilDev.Ini.Read(section, "Hash", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(has))
                                    continue;

                                Dictionary<string, List<string>> phs = new Dictionary<string, List<string>>();
                                foreach (string lang in ("Afrikaans,Albanian,Arabic,Armenian,Basque,Belarusian,Bulgarian,Catalan,Croatian,Czech,Danish,Dutch,EnglishGB,Estonian,Farsi,Filipino,Finnish,French,Galician,German,Greek,Hebrew,Hungarian,Indonesian,Japanese,Irish,Italian,Korean,Latvian,Lithuanian,Luxembourgish,Macedonian,Malay,Norwegian,Polish,Portuguese,PortugueseBR,Romanian,Russian,Serbian,SerbianLatin,SimpChinese,Slovak,Slovenian,Spanish,SpanishInternational,Sundanese,Swedish,Thai,TradChinese,Turkish,Ukrainian,Vietnamese").Split(','))
                                {
                                    string tmpFile = SilDev.Ini.Read(section, $"DownloadFile_{lang}", ExternDBPath);
                                    if (string.IsNullOrWhiteSpace(tmpFile))
                                        continue;
                                    string tmphash = SilDev.Ini.Read(section, $"Hash_{lang}", ExternDBPath);
                                    if (string.IsNullOrWhiteSpace(tmphash) || !string.IsNullOrWhiteSpace(tmphash) && tmphash == has)
                                        continue;
                                    string tmpPath = SilDev.Ini.Read(section, "DownloadPath", ExternDBPath);
                                    tmpFile = $"{(string.IsNullOrWhiteSpace(tmpPath) ? "http://downloads.sourceforge.net/portableapps" : tmpPath)}/{tmpFile}";
                                    phs.Add(lang, new List<string>() { tmpFile, tmphash });
                                }

                                string siz = SilDev.Ini.Read(section, "InstallSize", ExternDBPath);
                                string adv = SilDev.Ini.Read(section, "Advanced", ExternDBPath);

                                File.AppendAllText(AppsDBPath, "\n");

                                SilDev.Ini.Write(section, "Name", nam, AppsDBPath);
                                SilDev.Ini.Write(section, "Description", des, AppsDBPath);
                                SilDev.Ini.Write(section, "Category", cat, AppsDBPath);
                                SilDev.Ini.Write(section, "Version", ver, AppsDBPath);
                                SilDev.Ini.Write(section, "ArchivePath", pat, AppsDBPath);
                                SilDev.Ini.Write(section, "ArchiveHash", has, AppsDBPath);

                                if (phs.Count > 0)
                                {
                                    SilDev.Ini.Write(section, "AvailableArchiveLangs", string.Join(",", phs.Keys), AppsDBPath);
                                    foreach (KeyValuePair<string, List<string>> item in phs)
                                    {
                                        SilDev.Ini.Write(section, $"ArchivePath_{item.Key}", item.Value[0], AppsDBPath);
                                        SilDev.Ini.Write(section, $"ArchiveHash_{item.Key}", item.Value[1], AppsDBPath);
                                    }
                                }

                                SilDev.Ini.Write(section, "InstallSize", siz, AppsDBPath);
                                if (adv.ToLower() == "true")
                                    SilDev.Ini.Write(section, "Advanced", true, AppsDBPath);
                            }
                            File.Delete(ExternDBPath);
                        }

                        // Add another external app database for unpublished stuff - requires host access data
                        if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                        {
                            string ExternDB = SilDev.Network.DownloadString($"{SWSrv}/AppInfo.ini", SWUsr, SWPwd);
                            if (!string.IsNullOrWhiteSpace(ExternDB))
                                File.AppendAllText(AppsDBPath, $"{Environment.NewLine}{ExternDB}");
                        }

                        // Fix for uniform newline format
                        File.WriteAllText(AppsDBPath, File.ReadAllText(AppsDBPath)
                            .Replace(Environment.NewLine, "\n")
                            .Replace(((char)0x2028).ToString(), "\n")
                            .Replace(((char)0x2029).ToString(), "\n")
                            .Replace("\r", "\n")
                            .Replace("\n", Environment.NewLine));

                        // Get available apps
                        WebInfoSections = SilDev.Ini.GetSections(AppsDBPath);
                        if (WebInfoSections.Count == 0)
                            throw new Exception("No available apps found.");
                    }
                }

                if (!UpdateSearch)
                {
                    if (File.Exists(AppsDBPath))
                        appsList_SetContent(WebInfoSections);
                    if (appsList.Items.Count == 0)
                        throw new Exception("No available apps found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(Lang.GetText("NoServerAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            // Search updates
            try
            {
                List<string> OutdatedApps = new List<string>();
                foreach (string dir in GetInstalledApps(!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd) ? 3 : 0))
                {
                    string section = Path.GetFileName(dir);

                    if (SilDev.Ini.ReadBoolean(section, "NoUpdates"))
                        continue;

                    if (dir.Contains("\\.share\\"))
                        section = $"{section}###";

                    if (!WebInfoSections.Contains(section))
                        continue;

                    Dictionary<string, string> fileData = new Dictionary<string, string>();
                    string verData = SilDev.Ini.Read(section, "VersionData", AppsDBPath);
                    string verHash = SilDev.Ini.Read(section, "VersionHash", AppsDBPath);
                    if (!string.IsNullOrWhiteSpace(verData) && !string.IsNullOrWhiteSpace(verHash))
                    {
                        if (!verData.Contains(","))
                            verData = $"{verData},";
                        string[] verDataSplit = verData.Split(',');
                        if (!verHash.Contains(","))
                            verHash = $"{verHash},";
                        string[] verHashSplit = verHash.Split(',');
                        if (verDataSplit.Length != verHashSplit.Length)
                            continue;
                        for (int i = 0; i < verDataSplit.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(verDataSplit[i]) || string.IsNullOrWhiteSpace(verHashSplit[i]))
                                continue;
                            fileData.Add(verDataSplit[i], verHashSplit[i]);
                        }
                    }

                    if (fileData.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> data in fileData)
                        {
                            string filePath = Path.Combine(dir, data.Key);
                            if (!File.Exists(filePath))
                                continue;
                            if (SilDev.Crypt.SHA.EncryptFile(filePath, SilDev.Crypt.SHA.CryptKind.SHA256) != data.Value)
                            {
                                if (!OutdatedApps.Contains(section))
                                    OutdatedApps.Add(section);
                                break;
                            }
                        }
                        continue;
                    }

                    if (dir.Contains("\\.share\\"))
                        continue;

                    string appIniPath = Path.Combine(dir, "App\\AppInfo\\appinfo.ini");
                    if (!File.Exists(appIniPath))
                        continue;

                    Version LocalVer = SilDev.Ini.ReadVersion("Version", "DisplayVersion", appIniPath);
                    Version ServerVer = SilDev.Ini.ReadVersion(section, "Version", AppsDBPath);
                    if (LocalVer < ServerVer)
                    {
                        SilDev.Log.Debug($"Update for '{section}' found (Local: '{LocalVer}'; Server: '{ServerVer}').");
                        if (!OutdatedApps.Contains(section))
                            OutdatedApps.Add(section);
                    }
                }
                if (OutdatedApps.Count == 0)
                    throw new Exception("No updates available.");

                appsList_SetContent(OutdatedApps);
                if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("UpdatesAvailableMsg0"), appsList.Items.Count, appsList.Items.Count > 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                    throw new Exception("Update canceled.");

                foreach (ListViewItem item in appsList.Items)
                    item.Checked = true; 
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Application.Exit();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (NotifyBox != null)
                NotifyBox.Close();
            LoadSettings();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (appsList.Columns.Count == 5)
            {
                int staticColumnsWidth = SystemInformation.VerticalScrollBarWidth + 2;
                for (int i = 3; i < appsList.Columns.Count; i++)
                    staticColumnsWidth += appsList.Columns[i].Width;
                int dynamicColumnsWidth = 0;
                while (dynamicColumnsWidth < appsList.Width - staticColumnsWidth)
                    dynamicColumnsWidth++;
                for (int i = 0; i < 3; i++)
                    appsList.Columns[i].Width = (int)Math.Round(dynamicColumnsWidth / 100f * (i == 0 ? 35f : i == 1 ? 50f : 15f));
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DlAmount > 0 && SilDev.MsgBox.Show(this, Lang.GetText("AreYouSureMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (SettingsLoaded)
            {
                if (WindowState != FormWindowState.Minimized)
                    SilDev.Ini.Write("Settings", "XWindowState", WindowState != FormWindowState.Normal ? (FormWindowState?)WindowState : null);
                if (WindowState != FormWindowState.Maximized)
                {
                    SilDev.Ini.Write("Settings", "XWindowWidth", Width);
                    SilDev.Ini.Write("Settings", "XWindowHeight", Height);
                }
            }
            if (checkDownload.Enabled)
                checkDownload.Enabled = false;
            if (multiDownloader.Enabled)
                multiDownloader.Enabled = false;
            SilDev.Network.CancelAsyncDownload();
            try
            {
                if (File.Exists(DnsIniPath))
                    File.Delete(DnsIniPath);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            List<string> appInstaller = GetAllAppInstaller();
            if (appInstaller.Count > 0)
                SilDev.Run.Cmd($"PING 127.0.0.1 -n 3 >NUL && DEL /F /Q \"{string.Join("\" && DEL /F /Q \"", appInstaller)}\"", -1);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        private void LoadSettings()
        {
            int WindowWidth = SilDev.Ini.ReadInteger("Settings", "XWindowWidth", MinimumSize.Width);
            if (WindowWidth > MinimumSize.Width && WindowWidth < Screen.PrimaryScreen.WorkingArea.Width)
                Width = WindowWidth;
            if (WindowWidth >= Screen.PrimaryScreen.WorkingArea.Width)
                Width = Screen.PrimaryScreen.WorkingArea.Width;
            Left = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (Width / 2);
            switch (SilDev.WinAPI.TaskBar.GetLocation())
            {
                case SilDev.WinAPI.TaskBar.Location.LEFT:
                    Left += SilDev.WinAPI.TaskBar.GetSize();
                    break;
                case SilDev.WinAPI.TaskBar.Location.RIGHT:
                    Left -= SilDev.WinAPI.TaskBar.GetSize();
                    break;
            }

            int WindowHeight = SilDev.Ini.ReadInteger("Settings", "XWindowHeight", MinimumSize.Height);
            if (WindowHeight > MinimumSize.Height && WindowHeight < Screen.PrimaryScreen.WorkingArea.Height)
                Height = WindowHeight;
            if (WindowHeight >= Screen.PrimaryScreen.WorkingArea.Height)
                Height = Screen.PrimaryScreen.WorkingArea.Height;
            Top = (Screen.PrimaryScreen.WorkingArea.Height / 2) - (Height / 2);
            switch (SilDev.WinAPI.TaskBar.GetLocation())
            {
                case SilDev.WinAPI.TaskBar.Location.TOP:
                    Top += SilDev.WinAPI.TaskBar.GetSize();
                    break;
                case SilDev.WinAPI.TaskBar.Location.BOTTOM:
                    Top -= SilDev.WinAPI.TaskBar.GetSize();
                    break;
            }

            if (SilDev.Ini.Read("Settings", "XWindowState").StartsWith("Max", StringComparison.OrdinalIgnoreCase))
                WindowState = FormWindowState.Maximized;

            showGroupsCheck.Checked = SilDev.Ini.ReadBoolean("Settings", "XShowGroups", true);
            showColorsCheck.Checked = SilDev.Ini.ReadBoolean("Settings", "XShowGroupColors", true);

            Opacity = 1d;
            TopMost = false;
            Refresh();
            SettingsLoaded = true;
        }

        private List<string> GetAllAppInstaller()
        {
            List<string> list = new List<string>();
            try
            {
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps"), "*.paf.exe", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.repack"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.free"), "*.7z", SearchOption.TopDirectoryOnly));
                list.AddRange(Directory.GetFiles(Path.Combine(HomeDir, "Apps\\.share"), "*.7z", SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            return list;
        }

        private List<string> GetInstalledApps(int _index)
        {
            List<string> list = new List<string>();
            try
            {
                list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps"), "*", SearchOption.TopDirectoryOnly).Where(s => !s.StartsWith(".")).ToArray());
                list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.free"), "*", SearchOption.TopDirectoryOnly));
                if (_index > 0 && _index < 3)
                    list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.repack"), "*", SearchOption.TopDirectoryOnly));
                if (_index > 1)
                    list.AddRange(Directory.GetDirectories(Path.Combine(HomeDir, "Apps\\.share"), "*", SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            return list;
        }

        private List<string> GetInstalledApps() =>
            GetInstalledApps(1);

        private void appsList_Enter(object sender, EventArgs e) =>
            appsList_ShowColors(false);

        private void appsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                string requiredApps = SilDev.Ini.Read(appsList.Items[e.Index].Name, "Requires", AppsDBPath);
                if (!string.IsNullOrWhiteSpace(requiredApps))
                {
                    if (!requiredApps.Contains(","))
                        requiredApps = $"{requiredApps},";
                    foreach (string i in requiredApps.Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(i))
                            continue;
                        string app = i;
                        if (i.Contains("|"))
                        {
                            string[] split = i.Split('|');
                            if (split.Length != 2 || !i.Contains("64"))
                                continue;
                            if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
                                continue;
                            app = Environment.Is64BitOperatingSystem ? split[0].Contains("64") ? split[0] : split[1] : !split[0].Contains("64") ? split[0] : split[1];
                        }
                        foreach (ListViewItem item in appsList.Items)
                        {
                            if (item.Name == app)
                            {
                                item.Checked = e.NewValue == CheckState.Checked;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void appsList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!multiDownloader.Enabled && !checkDownload.Enabled && !SilDev.Network.AsyncDownloadIsBusy())
                okBtn.Enabled = appsList.CheckedItems.Count > 0;
        }

        private void appsList_ShowColors(bool _searchResultColor)
        {
            if (searchResultBlinker.Enabled)
                searchResultBlinker.Enabled = false;
            foreach (ListViewItem item in appsList.Items)
            {
                if (_searchResultColor && item.Group.Name == "listViewGroup0")
                {
                    item.ForeColor = SystemColors.HighlightText;
                    item.BackColor = SystemColors.Highlight;
                    continue;
                }
                item.ForeColor = appsList.ForeColor;
                item.BackColor = appsList.BackColor;
            }
            if (showColorsCheck.Checked)
            {
                foreach (ListViewItem item in appsList.Items)
                {
                    if (item.Group.Name != "listViewGroup0")
                        item.ForeColor = appsList.ForeColor;
                    switch (item.Group.Name)
                    {
                        case "listViewGroup1":  // Accessibility
                            item.BackColor = ColorTranslator.FromHtml("#FFFF99");
                            break;
                        case "listViewGroup2":  // Education
                            item.BackColor = ColorTranslator.FromHtml("#FFFFCC");
                            break;
                        case "listViewGroup3":  // Development
                            item.BackColor = ColorTranslator.FromHtml("#777799");
                            break;
                        case "listViewGroup4":  // Office
                            item.BackColor = ColorTranslator.FromHtml("#88BBDD");
                            break;
                        case "listViewGroup5":  // Internet
                            item.BackColor = ColorTranslator.FromHtml("#CC8866");
                            break;
                        case "listViewGroup6":  // Graphics and Pictures		
                            item.BackColor = ColorTranslator.FromHtml("#FFCCFF");
                            break;
                        case "listViewGroup7":  // Music and Video	
                            item.BackColor = ColorTranslator.FromHtml("#CCCCFF");
                            break;
                        case "listViewGroup8":  // Security
                            item.BackColor = ColorTranslator.FromHtml("#66CC99");
                            break;
                        case "listViewGroup9":  // Utilities
                            item.BackColor = ColorTranslator.FromHtml("#77BBBB");
                            break;
                        case "listViewGroup11": // *Advanced
                            item.BackColor = ColorTranslator.FromHtml("#FF6666");
                            break;
                        case "listViewGroup12": // *Shareware	
                            item.BackColor = ColorTranslator.FromHtml("#FF66FF");
                            break;
                    }
                }
            }
        }

        private void appsList_ShowColors() =>
            appsList_ShowColors(true);

        private void appsList_SetContent(List<string> _list)
        {
            Image DefaultIcon = Properties.Resources.PortableAppsBox;
            int index = 0;
            byte[] IcoDb = null;
            byte[] SWIcoDb = null;
            try
            {
                IcoDb = File.ReadAllBytes(Path.Combine(HomeDir, "Assets\\icon.db"));
                if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                    SWIcoDb = SilDev.Network.DownloadData($"{SWSrv}/AppIcon.db", SWUsr, SWPwd);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }

            foreach (string section in _list)
            {
                string nam = SilDev.Ini.Read(section, "Name", AppsDBPath);
                string des = SilDev.Ini.Read(section, "Description", AppsDBPath);
                string cat = SilDev.Ini.Read(section, "Category", AppsDBPath);
                string ver = SilDev.Ini.Read(section, "Version", AppsDBPath);
                string pat = SilDev.Ini.Read(section, "ArchivePath", AppsDBPath);
                string siz = $"{SilDev.Ini.Read(section, "InstallSize", AppsDBPath)} MB";
                string adv = SilDev.Ini.Read(section, "Advanced", AppsDBPath);
                string src = "si13n7.com";
                if (pat.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string tmpHost = new Uri(pat).Host;
                        string[] tmpSplit = tmpHost.Split('.');
                        src = tmpSplit.Length >= 3 ? string.Join(".", tmpSplit[tmpSplit.Length - 2], tmpSplit[tmpSplit.Length - 1]) : tmpHost;
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                        continue;
                    }
                }

                ListViewItem item = new ListViewItem(nam);
                item.Name = section;
                item.SubItems.Add(des);
                item.SubItems.Add(ver);
                item.SubItems.Add(siz);
                item.SubItems.Add(src);
                item.ImageIndex = index;

                if (section.EndsWith("###") && (string.IsNullOrEmpty(SWSrv) || string.IsNullOrEmpty(SWUsr) || string.IsNullOrEmpty(SWPwd)))
                    continue;

                if (!string.IsNullOrWhiteSpace(cat))
                {
                    try
                    {
                        string nameHash = SilDev.Crypt.MD5.Encrypt(section.EndsWith("###") ? section.Replace("###", string.Empty) : section);
                        if (IcoDb == null)
                            throw new FileNotFoundException();
                        foreach (byte[] db in new byte[][] { IcoDb, SWIcoDb })
                        {
                            if (db == null)
                                continue;
                            using (MemoryStream stream = new MemoryStream(db))
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
                        if (!imgList.Images.ContainsKey(nameHash))
                            throw new FileNotFoundException();
                    }
                    catch
                    {
                        imgList.Images.Add(DefaultIcon);
                    }

                    try
                    {
                        ListViewGroup group = new ListViewGroup(cat);
                        if (!section.EndsWith("###"))
                        {
                            foreach (ListViewGroup gr in appsList.Groups)
                            {
                                if (string.IsNullOrWhiteSpace(adv) && Lang.GetText("en-US", gr.Name) == cat || Lang.GetText("en-US", gr.Name) == "*Advanced")
                                {
                                    appsList.Items.Add(item).Group = gr;
                                    break;
                                }
                            }
                        }
                        else
                            appsList.Items.Add(item).Group = appsList.Groups[appsList.Groups.Count - 1];
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
                index++;
            }

            appsList.SmallImageList = imgList;
            appsList_ShowColors();
            appStatus.Text = string.Format(Lang.GetText(appStatus), appsList.Items.Count, appsList.Items.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"));
        }

        private void showGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            SilDev.Ini.Write("Settings", "XShowGroups", !cb.Checked ? (bool?)false : null);
            appsList.ShowGroups = cb.Checked;
        }

        private void showColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            SilDev.Ini.Write("Settings", "XShowGroupColors", !((CheckBox)sender).Checked ? (bool?)false : null);
            appsList_ShowColors();
        }

        private void searchBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                string tmp = tb.Text;
                tb.Text = string.Empty;
                tb.Text = tmp;
            }
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                appsList.Select();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            ResetSearch();
            TextBox tb = (TextBox)sender;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                string search = tb.Text.ToLower();
                foreach (ListViewItem item in appsList.Items)
                {
                    ListViewItem.ListViewSubItem description = item.SubItems[1];
                    if (description.Text.ToLower().Contains(search))
                    {
                        foreach (ListViewGroup group in appsList.Groups)
                        {
                            if (group.Name == "listViewGroup0")
                            {
                                if (!group.Items.Contains(item))
                                    group.Items.Add(item);
                                if (!item.Selected)
                                {
                                    item.ForeColor = SystemColors.HighlightText;
                                    item.BackColor = SystemColors.Highlight;
                                    item.EnsureVisible();
                                }
                            }
                        }
                        continue;
                    }
                    if (item.Text.ToLower().Contains(search))
                    {
                        foreach (ListViewGroup group in appsList.Groups)
                        {
                            if (group.Name == "listViewGroup0")
                            {
                                if (!group.Items.Contains(item))
                                    group.Items.Add(item);
                                if (!item.Selected)
                                {
                                    item.ForeColor = SystemColors.HighlightText;
                                    item.BackColor = SystemColors.Highlight;
                                    item.EnsureVisible();
                                }
                            }
                        }
                    }
                }
                if (SearchResultBlinkCount > 0)
                    SearchResultBlinkCount = 0;
                if (!searchResultBlinker.Enabled)
                    searchResultBlinker.Enabled = true;
            }
        }

        private void ResetSearch()
        {
            if (!showGroupsCheck.Checked)
                showGroupsCheck.Checked = true;
            if (AppListClone.Items.Count == 0)
            {
                foreach (ListViewGroup group in appsList.Groups)
                    AppListClone.Groups.Add(new ListViewGroup()
                    {
                        Name = group.Name,
                        Header = group.Header,
                    });
                foreach (ListViewItem item in appsList.Items)
                    AppListClone.Items.Add((ListViewItem)item.Clone());
            }
            for (int i = 0; i < appsList.Items.Count; i++)
            {
                ListViewItem item = appsList.Items[i];
                ListViewItem clone = AppListClone.Items[i];
                foreach (ListViewGroup group in appsList.Groups)
                {
                    if (clone.Group.Name == group.Name)
                    {
                        if (clone.Group.Name != item.Group.Name)
                            group.Items.Add(item);
                        break;
                    }
                }
            }
            foreach (ListViewGroup group in appsList.Groups)
            {
                if (group.Items.Count == 0)
                    continue;
                foreach (ListViewItem item in group.Items)
                {
                    item.EnsureVisible();
                    break;
                }
                break;
            }
            appsList_ShowColors(false);
        }

        private void searchResultBlinker_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer t = (System.Windows.Forms.Timer)sender;
            if (t.Enabled && SearchResultBlinkCount >= 5)
                t.Enabled = false;
            foreach (ListViewGroup group in appsList.Groups)
            {
                if (group.Name != "listViewGroup0")
                    continue;
                if (group.Items.Count > 0)
                {
                    foreach (ListViewItem item in appsList.Items)
                    {
                        if (item.Group.Name != group.Name)
                            continue;
                        if (!searchResultBlinker.Enabled || item.BackColor != SystemColors.Highlight)
                        {
                            item.BackColor = SystemColors.Highlight;
                            t.Interval = 200;
                        }
                        else
                        {
                            item.BackColor = appsList.BackColor;
                            t.Interval = 100;
                        }
                    }
                }
                else
                    t.Enabled = false;
            }
            if (t.Enabled)
                SearchResultBlinkCount++;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (!b.Enabled || appsList.Items.Count == 0 || SilDev.Network.AsyncDownloadIsBusy())
                return;
            b.Enabled = false;
            foreach (ListViewItem item in appsList.Items)
            {
                if (item.Checked)
                    continue;
                item.Remove();
            }
            foreach (string file in GetAllAppInstaller())
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
            DlCount = 1;
            DlAmount = appsList.CheckedItems.Count;
            appsList.HideSelection = !b.Enabled;
            appsList.Enabled = b.Enabled;
            showGroupsCheck.Enabled = b.Enabled;
            showColorsCheck.Enabled = b.Enabled;
            searchBox.Enabled = b.Enabled;
            cancelBtn.Enabled = b.Enabled;
            downloadSpeed.Visible = !b.Enabled;
            downloadProgress.Visible = !b.Enabled;
            downloadReceived.Visible = !b.Enabled;
            multiDownloader.Enabled = !b.Enabled;
            ResetSearch();
            SilDev.WinAPI.TaskBarProgress.SetState(Handle, SilDev.WinAPI.TaskBarProgress.States.Indeterminate);
        }

        private void multiDownloader_Tick(object sender, EventArgs e)
        {
            multiDownloader.Enabled = false;
            foreach (ListViewItem item in appsList.Items)
            {
                if (checkDownload.Enabled || !item.Checked)
                    continue;

                Text = string.Format(Lang.GetText("StatusDownload"), DlCount, DlAmount, item.Text);
                appStatus.Text = Text;

                string archivePath = SilDev.Ini.Read(item.Name, "ArchivePath", AppsDBPath);
                string archiveLangs = SilDev.Ini.Read(item.Name, "AvailableArchiveLangs", AppsDBPath);
                string archiveLang = string.Empty;
                bool archiveLangConfirmed = false;
                if (!string.IsNullOrWhiteSpace(archiveLangs) && archiveLangs.Contains(","))
                {
                    string defaultLang = archivePath.ToLower().Contains("multilingual") ? "Multilingual" : "English";
                    archiveLangs = $"Default ({defaultLang}),{archiveLangs}";
                    archiveLang = SilDev.Ini.Read(item.Name, "ArchiveLang");
                    archiveLangConfirmed = SilDev.Ini.Read(item.Name, "ArchiveLangConfirmed").ToLower() == "true";
                    if (!archiveLangs.Contains(archiveLang) || !archiveLangConfirmed)
                    {
                        try
                        {
                            DialogResult result = DialogResult.None;
                            while (result != DialogResult.OK)
                                using (Form dialog = new LangSelectionForm(item.Name, item.Text, archiveLangs.Split(',')))
                                    if ((result = dialog.ShowDialog()) != DialogResult.OK)
                                        Close();
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                    archiveLang = SilDev.Ini.Read(item.Name, "ArchiveLang");
                    if (archiveLang.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                        archiveLang = string.Empty;
                    if (!string.IsNullOrWhiteSpace(archiveLang))
                        archivePath = SilDev.Ini.Read(item.Name, $"ArchivePath_{archiveLang}", AppsDBPath);
                }

                string localArchivePath = string.Empty;
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    localArchivePath = Path.Combine(HomeDir, "Apps");
                    if (item.Group.Header == "*Shareware")
                    {
                        localArchivePath = Path.Combine(localArchivePath, ".share");
                        localArchivePath = Path.Combine(localArchivePath, archivePath.Replace("/", "\\"));
                    }
                    else
                        localArchivePath = Path.Combine(localArchivePath, archivePath.Replace("/", "\\"));
                }
                else
                {
                    string[] tmp = archivePath.Split('/');
                    if (tmp.Length == 0)
                        continue;
                    localArchivePath = Path.Combine(HomeDir, $"Apps\\{tmp[tmp.Length - 1]}");
                }
                if (!Directory.Exists(Path.GetDirectoryName(localArchivePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(localArchivePath));

                DlAsyncIsDoneCounter = 0;
                SilDev.Network.CancelAsyncDownload();
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Group.Header == "*Shareware")
                        SilDev.Network.DownloadFileAsync(item.Text, $"{(SWSrv.EndsWith("/") ? SWSrv.Substring(0, SWSrv.Length - 1) : SWSrv)}/{archivePath}", localArchivePath, SWUsr, SWPwd);
                    else
                    {
                        foreach (string mirror in DownloadServers)
                        {
                            string newArchivePath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{archivePath}";
                            if (SilDev.Network.OnlineFileExists(newArchivePath))
                            {
                                SilDev.Log.Debug($"File has been found at '{mirror}'.");
                                SilDev.Network.DownloadFileAsync(item.Text, newArchivePath, localArchivePath);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (archivePath.ToLower().Contains("sourceforge"))
                    {
                        string newArchivePath = archivePath;
                        if (SfMirrors.Count == 0)
                        {
                            Dictionary<string, long> sortHelper = new Dictionary<string, long>();
                            string[] mirrors = new string[]
                            {
                                "downloads.sourceforge.net",
                                "netcologne.dl.sourceforge.net",
                                "freefr.dl.sourceforge.net",
                                "heanet.dl.sourceforge.net",
                                "kent.dl.sourceforge.net",
                                "vorboss.dl.sourceforge.net",
                                "netix.dl.sourceforge.net",
                                "skylink.dl.sourceforge.net"
                            };
                            foreach (string mirror in mirrors)
                            {
                                string path = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                SilDev.Network.Ping(path);
                                if (!sortHelper.Keys.Contains(mirror))
                                    sortHelper.Add(mirror, SilDev.Network.PingReply.RoundtripTime);
                            }
                            SfMirrors = sortHelper.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
                        }
                        if (SfMirrors.Count > 0)
                        {
                            foreach (string mirror in SfMirrors)
                            {
                                if (DlFailRetry < SfMirrors.Count - 1 && SfLastMirrors.ContainsKey(item.Name) && SfLastMirrors[item.Name].Contains(mirror))
                                    continue;
                                newArchivePath = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                if (SilDev.Network.OnlineFileExists(newArchivePath))
                                {
                                    if (!SfLastMirrors.ContainsKey(item.Name))
                                        SfLastMirrors.Add(item.Name, new List<string>() { mirror });
                                    else
                                        SfLastMirrors[item.Name].Add(mirror);
                                    SilDev.Log.Debug($"File has been found at '{mirror}'.");
                                    break;
                                }
                            }
                        }
                        SilDev.Network.DownloadFileAsync(item.Text, newArchivePath, localArchivePath);
                    }
                    else
                        SilDev.Network.DownloadFileAsync(item.Text, archivePath, localArchivePath);
                }
                DlCount++;
                item.Checked = false;
                checkDownload.Enabled = true;
                break;
            }
        }

        private void checkDownload_Tick(object sender, EventArgs e)
        {
            downloadSpeed.Text = SilDev.Network.LatestAsyncDownloadInfo.TransferSpeed;
            downloadProgress_Update(SilDev.Network.LatestAsyncDownloadInfo.ProgressPercentage);
            downloadReceived.Text = SilDev.Network.LatestAsyncDownloadInfo.DataReceived;
            if (!SilDev.Network.AsyncDownloadIsBusy())
                DlAsyncIsDoneCounter++;
            if (DlAsyncIsDoneCounter >= 10)
            {
                checkDownload.Enabled = false;
                if (appsList.CheckedItems.Count > 0)
                {
                    multiDownloader.Enabled = true;
                    return;
                }
                Text = Title;
                appStatus.Text = Lang.GetText("StatusExtract");
                downloadSpeed.Visible = false;
                downloadReceived.Visible = false;
                SilDev.WinAPI.TaskBarProgress.SetState(Handle, SilDev.WinAPI.TaskBarProgress.States.Indeterminate);
                List<string> appInstaller = GetAllAppInstaller();
                foreach (string file in appInstaller)
                {
                    string appDir = string.Empty;
                    if (file.EndsWith(".paf.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string dir in GetInstalledApps())
                        {
                            if (Path.GetFileName(file).StartsWith(Path.GetFileName(dir), StringComparison.OrdinalIgnoreCase))
                            {
                                appDir = dir;
                                break;
                            }
                        }
                    }
                    else
                        appDir = file.Replace(".7z", string.Empty);
                    List<string> TaskList = new List<string>();
                    if (Directory.Exists(appDir))
                    {
                        foreach (string f in Directory.GetFiles(appDir, "*.exe", SearchOption.AllDirectories))
                        {
                            foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
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
                                    SilDev.Log.Debug(ex);
                                }
                                string fileName = $"{p.ProcessName}.exe";
                                if (!TaskList.Contains(fileName))
                                    TaskList.Add(fileName);
                            }
                        }
                        if (TaskList.Count > 0)
                        {
                            try
                            {
                                SilDev.Run.Cmd($"TASKKILL /F /IM \"{string.Join("\" && TASKKILL /F /IM \"", TaskList)}\"", true);
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                            }
                        }
                    }
                    if (File.Exists(file))
                    {
                        foreach (var info in SilDev.Network.AsyncDownloadInfo)
                        {
                            try
                            {
                                if (info.Value.FilePath != file)
                                    continue;
                                string fileName = Path.GetFileName(info.Value.FilePath);
                                string AppsDB = File.ReadAllText(AppsDBPath);
                                foreach (string section in SilDev.Ini.GetSections(AppsDB))
                                {
                                    if (file.Contains("\\.share\\") && !section.EndsWith("###"))
                                        continue;
                                    if (!SilDev.Ini.Read(section, "ArchivePath", AppsDB).EndsWith(fileName))
                                    {
                                        bool found = false;
                                        string archiveLangs = SilDev.Ini.Read(section, "AvailableArchiveLangs", AppsDB);
                                        if (archiveLangs.Contains(","))
                                            foreach (string lang in archiveLangs.Split(','))
                                                if (found = SilDev.Ini.Read(section, $"ArchivePath_{lang}", AppsDB).EndsWith(fileName))
                                                    break;
                                        if (!found)
                                            continue;
                                    }
                                    string archiveLang = SilDev.Ini.Read(section, "ArchiveLang");
                                    if (archiveLang.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                                        archiveLang = string.Empty;
                                    string archiveHash = !string.IsNullOrWhiteSpace(archiveLang) ? SilDev.Ini.Read(section, $"ArchiveHash_{archiveLang}", AppsDBPath) : SilDev.Ini.Read(section, "ArchiveHash", AppsDBPath);
                                    string localHash = SilDev.Crypt.MD5.EncryptFile(file);
                                    if (localHash == archiveHash)
                                        break;
                                    throw new InvalidOperationException($"Checksum is invalid. - Key: '{info.Key}'; Section: '{section}'; File: '{file}'; Current: '{archiveHash}'; Requires: '{localHash}';");
                                }
                                if (file.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                                    SilDev.Compress.Unzip7(file, appDir, ProcessWindowStyle.Minimized);
                                else
                                {
                                    if (SilDev.Run.App(new ProcessStartInfo()
                                    {
                                        Arguments = $"/DESTINATION=\"{Path.Combine(HomeDir, "Apps")}\\\"",
                                        FileName = file,
                                        WorkingDirectory = Path.Combine(HomeDir, "Apps")
                                    }, 0) == null)
                                        throw new InvalidOperationException($"'{file}' is corrupt.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                                SilDev.Network.ASYNCDOWNLOADINFODATA state = info.Value;
                                state.StatusCode = 3;
                                state.StatusMessage = "Download failed!";
                                SilDev.Network.AsyncDownloadInfo[info.Key] = state;
                            }
                            break;
                        }
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
                List<string> DownloadFails = new List<string>();
                foreach (var info in SilDev.Network.AsyncDownloadInfo)
                    if (info.Value.StatusCode > 1)
                        DownloadFails.Add(info.Key);
                if (DownloadFails.Count > 0)
                {
                    SilDev.WinAPI.TaskBarProgress.SetState(Handle, SilDev.WinAPI.TaskBarProgress.States.Error);
                    if (DlFailRetry < SfMirrors.Count - 1 || SilDev.MsgBox.Show(this, string.Format(Lang.GetText("DownloadErrorMsg"), string.Join(Environment.NewLine, DownloadFails)), Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    {
                        DlFailRetry++;
                        foreach (ListViewItem item in appsList.Items)
                            if (DownloadFails.Contains(item.Text))
                                item.Checked = true;
                        appStatus.Text = string.Empty;
                        DlCount = 0;
                        appsList.Enabled = true;
                        appsList.HideSelection = !appsList.Enabled;
                        downloadSpeed.Text = string.Empty;
                        downloadProgress_Update(0);
                        downloadProgress.Visible = !appsList.Enabled;
                        downloadReceived.Text = string.Empty;
                        showGroupsCheck.Enabled = appsList.Enabled;
                        showColorsCheck.Enabled = appsList.Enabled;
                        searchBox.Enabled = appsList.Enabled;
                        okBtn.Enabled = appsList.Enabled;
                        cancelBtn.Enabled = appsList.Enabled;
                        okBtn_Click(okBtn, null);
                        return;
                    }
                }
                else
                {
                    SilDev.WinAPI.TaskBarProgress.SetValue(Handle, 100, 100);
                    SilDev.MsgBox.Show(this, string.Format(Lang.GetText("SuccessfullyDownloadMsg0"), appInstaller.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"), UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                DlAmount = 0;
                Application.Exit();
            }
        }

        private void downloadProgress_Update(int _value)
        {
            SilDev.WinAPI.TaskBarProgress.SetValue(Handle, _value, 100);
            using (Graphics g = downloadProgress.CreateGraphics())
            {
                int width = _value > 0 && _value < 100 ? (int)Math.Round(downloadProgress.Width / 100d * _value, MidpointRounding.AwayFromZero) : downloadProgress.Width;
                using (Brush b = new SolidBrush(_value > 0 ? Color.DodgerBlue : downloadProgress.BackColor))
                    g.FillRectangle(b, 0, 0, width, downloadProgress.Height);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e) =>
            Application.Exit();

        private void urlStatus_Click(object sender, EventArgs e) =>
            Process.Start("http:\\www.si13n7.com");
    }
}
