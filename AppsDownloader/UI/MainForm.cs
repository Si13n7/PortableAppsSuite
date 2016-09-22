using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    public partial class MainForm : Form
    {
        string Title = string.Empty;

        bool SettingsLoaded = false;
        bool SettingsDisabled = false;

        static readonly bool UpdateSearch = Environment.CommandLine.Contains("{F92DAD88-DA45-405A-B0EB-10A1E9B2ADDD}");

        static readonly string HomeDir = PATH.Combine("%CurDir%\\..");

        static readonly string AppsDBDir = PATH.Combine("%CurDir%\\Helper\\TEMP");
        static readonly string AppsDBPath = Path.Combine(AppsDBDir, UpdateSearch ? "AppInfoUpd.ini" : "AppInfo.ini");
        List<string> AppsDBSections = new List<string>();

        // Initializes the notify box you see at program start
        NOTIFYBOX NotifyBox = new NOTIFYBOX()
        {
            BackColor = Color.FromArgb(64, 64, 64),
            BorderColor = Color.SteelBlue,
            CaptionColor = Color.LightSteelBlue,
            TextColor = Color.FromArgb(224, 224, 224),
            Opacity = .75d
        };

        // Helps for search function
        ListView AppListClone = new ListView();
        int SearchResultBlinkCount = 0;

        // Simple method to manage multiple downloads
        Dictionary<string, NET.ASYNCTRANSFER> TransferManager = new Dictionary<string, NET.ASYNCTRANSFER>();
        string LastTransferItem = string.Empty;
        int DownloadFinished = 0;
        int DownloadCount = 0;
        int DownloadAmount = 0;
        int DownloadRetries = 0;

        // Organizes Si13n7.com mirrors
        List<string> Si13n7Mirrors = new List<string>();

        // List of available SourceForge.net mirrors with files
        readonly string[] SourceForgeMirrors = new string[]
        {
            "downloads.sourceforge.net",
            "netcologne.dl.sourceforge.net",
            "freefr.dl.sourceforge.net",
            "heanet.dl.sourceforge.net",
            "kent.dl.sourceforge.net",
            "vorboss.dl.sourceforge.net",
            "netix.dl.sourceforge.net"
        };

        // Sorts SourgeForge.net mirrors by client connection at the first use
        List<string> SourceForgeMirrorsSorted = new List<string>();

        // Holds last used SorgeForge.net mirrors for each download to manage download fails
        Dictionary<string, List<string>> SfLastMirrors = new Dictionary<string, List<string>>();

        // Allows to use an alternate password protected server with portable apps
        string SWDataKey = Path.Combine(HomeDir, "Documents\\SWData.key");
        string SWSrv = INI.Read("Host", "Srv");
        string SWUsr = INI.Read("Host", "Usr");
        string SWPwd = INI.Read("Host", "Pwd");

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_purple_64;
            MaximumSize = Screen.FromHandle(Handle).WorkingArea.Size;
#if !x86
            Text = $"{Text} (64-bit)";
#endif
            Title = Text;
            if (!appsList.Focus())
                appsList.Select();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            for (int i = 0; i < appsList.Columns.Count; i++)
                appsList.Columns[i].Text = Lang.GetText($"columnHeader{i + 1}");
            for (int i = 0; i < appsList.Groups.Count; i++)
                appsList.Groups[i].Header = Lang.GetText(appsList.Groups[i].Name);

            bool InternetIsAvailable = NET.InternetIsAvailable();
            if (!InternetIsAvailable)
            {
                if (!UpdateSearch)
                    MSGBOX.Show(this, Lang.GetText("InternetIsNotAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            if (!UpdateSearch)
                NotifyBox.Show(Lang.GetText("DatabaseAccessMsg"), Text, NOTIFYBOX.NotifyBoxStartPosition.Center);

            // Get 'Si13n7.com' download mirrors
            Dictionary<string, Dictionary<string, string>> DnsInfo = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < 3; i++)
            {
                DnsInfo = INI.ReadAll(NET.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
                if (DnsInfo.Count == 0 && i < 2)
                    Thread.Sleep(1000);
            }
            if (DnsInfo.Count > 0)
            {
                foreach (string section in DnsInfo.Keys)
                {
                    try
                    {
                        string addr = DnsInfo[section]["addr"];
                        if (string.IsNullOrWhiteSpace(addr))
                            continue;
                        string domain = DnsInfo[section]["domain"];
                        if (string.IsNullOrWhiteSpace(domain))
                            continue;
                        bool ssl = false;
                        bool.TryParse(DnsInfo[section]["ssl"], out ssl);
                        domain = ssl ? $"https://{domain}" : $"http://{domain}";
                        if (!Si13n7Mirrors.Contains(domain))
                            Si13n7Mirrors.Add(domain);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }
            if (!UpdateSearch && Si13n7Mirrors.Count == 0)
            {
                MSGBOX.Show(Lang.GetText("NoServerAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Encrypt host access data with AES-256 (safety ofc not guaranteed)
            if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
            {
                try
                {
                    string ProductId = new ManagementObject("Win32_OperatingSystem=@")["SerialNumber"].ToString();
                    if (string.IsNullOrWhiteSpace(ProductId))
                        throw new PlatformNotSupportedException();
                    if (SWSrv.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        INI.Write("Host", "Srv", SWSrv.TextToZip().EncryptToAES256(ProductId).EncodeToBase64());
                        INI.Write("Host", "Usr", SWUsr.TextToZip().EncryptToAES256(ProductId).EncodeToBase64());
                        INI.Write("Host", "Pwd", SWPwd.TextToZip().EncryptToAES256(ProductId).EncodeToBase64());
                    }
                    else
                    {
                        SWSrv = SWSrv.DecodeByteArrayFromBase64().DecryptFromAES256(ProductId).TextFromZip();
                        SWUsr = SWUsr.DecodeByteArrayFromBase64().DecryptFromAES256(ProductId).TextFromZip();
                        SWPwd = SWPwd.DecodeByteArrayFromBase64().DecryptFromAES256(ProductId).TextFromZip();
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }

            // Enforce database reset in certain cases
            string TmpAppsDBDir = Path.Combine(AppsDBDir, PATH.GetTempDirName());
            string TmpAppsDBPath = Path.Combine(TmpAppsDBDir, "update.ini");
            DateTime AppsDBLastWriteTime = DateTime.Now.AddHours(1d);
            long AppsDBLength = 0;
            if (!File.Exists(TmpAppsDBPath) && File.Exists(AppsDBPath))
            {
                try
                {
                    FileInfo fi = new FileInfo(AppsDBPath);
                    AppsDBLastWriteTime = fi.LastWriteTime;
                    AppsDBLength = (int)Math.Round(fi.Length / 1024f);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            if (UpdateSearch || File.Exists(TmpAppsDBPath) || (DateTime.Now - AppsDBLastWriteTime).TotalHours >= 1d || AppsDBLength < 168 || (AppsDBSections = INI.GetSections(AppsDBPath)).Count < 400)
            {
                try
                {
                    if (File.Exists(AppsDBPath))
                    {
                        DATA.SetAttributes(AppsDBPath, FileAttributes.Normal);
                        File.Delete(AppsDBPath);
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }

            try
            {
                if (!File.Exists(AppsDBPath))
                {
                    if (!Directory.Exists(TmpAppsDBDir))
                        Directory.CreateDirectory(TmpAppsDBDir);

                    // Get internal app database
                    for (int i = 0; i < 3; i++)
                    {
                        NET.DownloadFile("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/AppInfo.ini", AppsDBPath);
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
                    string ExternDBPath = Path.Combine(TmpAppsDBDir, "AppInfo.7z");
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
                            foreach (string mirror in Si13n7Mirrors)
                            {
                                string tmpSrv = $"{mirror}/{srv}";
                                if (!NET.FileIsAvailable(tmpSrv))
                                    continue;
                                NET.DownloadFile(tmpSrv, ExternDBPath);
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
                            NET.DownloadFile(srv, ExternDBPath);
                            if (File.Exists(ExternDBPath))
                                length = (int)(new FileInfo(ExternDBPath).Length / 1024);
                        }
                        if (File.Exists(ExternDBPath) && length > 24)
                            break;
                    }

                    // Merge databases
                    AppsDBSections = INI.GetSections(AppsDBPath);
                    if (File.Exists(ExternDBPath))
                    {
                        PACKER.Zip7Helper.Unzip(ExternDBPath, TmpAppsDBDir);
                        File.Delete(ExternDBPath);
                        ExternDBPath = TmpAppsDBPath;
                        if (File.Exists(ExternDBPath))
                        {
                            foreach (string section in INI.GetSections(ExternDBPath))
                            {
                                if (AppsDBSections.Contains(section))
                                    continue;

                                string nam = INI.Read(section, "Name", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(nam) || nam.Contains("PortableApps.com") || nam.Contains("jPortable") && nam.Contains("Launcher"))
                                    continue;
                                if (!nam.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase))
                                {
                                    string tmp = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
                                    tmp = Regex.Replace(tmp, @"\s+", " ");
                                    if (!string.IsNullOrWhiteSpace(tmp) && tmp != nam)
                                        nam = tmp.TrimEnd();
                                }
                                string des = INI.Read(section, "Description", ExternDBPath);
                                des = des.Replace(" Editor", " editor").Replace(" Reader", " reader");
                                if (string.IsNullOrWhiteSpace(des))
                                    continue;
                                string cat = INI.Read(section, "Category", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(cat))
                                    continue;
                                string ver = INI.Read(section, "DisplayVersion", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(ver))
                                    continue;
                                string pat = INI.Read(section, "DownloadPath", ExternDBPath);
                                pat = $"{(string.IsNullOrWhiteSpace(pat) ? "http://downloads.sourceforge.net/portableapps" : pat)}/{INI.Read(section, "DownloadFile", ExternDBPath)}";
                                if (!pat.EndsWith(".paf.exe", StringComparison.OrdinalIgnoreCase))
                                    continue;
                                string has = INI.Read(section, "Hash", ExternDBPath);
                                if (string.IsNullOrWhiteSpace(has))
                                    continue;

                                Dictionary<string, List<string>> phs = new Dictionary<string, List<string>>();
                                foreach (string lang in ("Afrikaans,Albanian,Arabic,Armenian,Basque,Belarusian,Bulgarian,Catalan,Croatian,Czech,Danish,Dutch,EnglishGB,Estonian,Farsi,Filipino,Finnish,French,Galician,German,Greek,Hebrew,Hungarian,Indonesian,Japanese,Irish,Italian,Korean,Latvian,Lithuanian,Luxembourgish,Macedonian,Malay,Norwegian,Polish,Portuguese,PortugueseBR,Romanian,Russian,Serbian,SerbianLatin,SimpChinese,Slovak,Slovenian,Spanish,SpanishInternational,Sundanese,Swedish,Thai,TradChinese,Turkish,Ukrainian,Vietnamese").Split(','))
                                {
                                    string tmpFile = INI.Read(section, $"DownloadFile_{lang}", ExternDBPath);
                                    if (string.IsNullOrWhiteSpace(tmpFile))
                                        continue;
                                    string tmphash = INI.Read(section, $"Hash_{lang}", ExternDBPath);
                                    if (string.IsNullOrWhiteSpace(tmphash) || !string.IsNullOrWhiteSpace(tmphash) && tmphash == has)
                                        continue;
                                    string tmpPath = INI.Read(section, "DownloadPath", ExternDBPath);
                                    tmpFile = $"{(string.IsNullOrWhiteSpace(tmpPath) ? "http://downloads.sourceforge.net/portableapps" : tmpPath)}/{tmpFile}";
                                    phs.Add(lang, new List<string>() { tmpFile, tmphash });
                                }

                                string siz = INI.Read(section, "InstallSize", ExternDBPath);
                                string adv = INI.Read(section, "Advanced", ExternDBPath);

                                File.AppendAllText(AppsDBPath, Environment.NewLine);

                                INI.Write(section, "Name", nam, AppsDBPath);
                                INI.Write(section, "Description", des, AppsDBPath);
                                INI.Write(section, "Category", cat, AppsDBPath);
                                INI.Write(section, "Version", ver, AppsDBPath);
                                INI.Write(section, "ArchivePath", pat, AppsDBPath);
                                INI.Write(section, "ArchiveHash", has, AppsDBPath);

                                if (phs.Count > 0)
                                {
                                    INI.Write(section, "AvailableArchiveLangs", string.Join(",", phs.Keys), AppsDBPath);
                                    foreach (KeyValuePair<string, List<string>> item in phs)
                                    {
                                        INI.Write(section, $"ArchivePath_{item.Key}", item.Value[0], AppsDBPath);
                                        INI.Write(section, $"ArchiveHash_{item.Key}", item.Value[1], AppsDBPath);
                                    }
                                }

                                INI.Write(section, "InstallSize", siz, AppsDBPath);
                                if (adv.ToLower() == "true")
                                    INI.Write(section, "Advanced", true, AppsDBPath);
                            }
                            try
                            {
                                Directory.Delete(TmpAppsDBDir, true);
                            }
                            catch (Exception ex)
                            {
                                LOG.Debug(ex);
                            }
                        }

                        // Add another external app database for unpublished stuff - requires host access data
                        if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                        {
                            try
                            {
                                string ExternDB = NET.DownloadString($"{SWSrv}/AppInfo.ini", SWUsr, SWPwd);
                                if (!string.IsNullOrWhiteSpace(ExternDB))
                                    File.AppendAllText(AppsDBPath, $"{Environment.NewLine}{ExternDB}");
                            }
                            catch (Exception ex)
                            {
                                LOG.Debug(ex);
                            }
                        }

                        // Done with database
                        File.WriteAllText(AppsDBPath, File.ReadAllText(AppsDBPath).FormatNewLine());
                        DATA.SetAttributes(AppsDBPath, FileAttributes.ReadOnly);

                        // Get available apps
                        AppsDBSections = INI.GetSections(AppsDBPath);
                        if (AppsDBSections.Count == 0)
                            throw new Exception("No available apps found.");
                    }
                }

                if (!UpdateSearch)
                {
                    if (File.Exists(AppsDBPath))
                        appsList_SetContent(AppsDBSections);
                    if (appsList.Items.Count == 0)
                        throw new Exception("No available apps found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                if (!UpdateSearch)
                    MSGBOX.Show(Lang.GetText("NoServerAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
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

                    if (INI.ReadBoolean(section, "NoUpdates"))
                        continue;

                    if (dir.Contains("\\.share\\"))
                        section = $"{section}###";

                    if (!AppsDBSections.Contains(section))
                        continue;

                    Dictionary<string, string> fileData = new Dictionary<string, string>();
                    string verData = INI.Read(section, "VersionData", AppsDBPath);
                    string verHash = INI.Read(section, "VersionHash", AppsDBPath);
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
                            if (filePath.EncryptFileToSHA256() != data.Value)
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

                    Version LocalVer = INI.ReadVersion("Version", "DisplayVersion", appIniPath);
                    Version ServerVer = INI.ReadVersion(section, "Version", AppsDBPath);
                    if (LocalVer < ServerVer)
                    {
                        LOG.Debug($"Update for '{section}' found (Local: '{LocalVer}'; Server: '{ServerVer}').");
                        if (!OutdatedApps.Contains(section))
                            OutdatedApps.Add(section);
                    }
                }
                if (OutdatedApps.Count == 0)
                    throw new Exception("No updates available.");

                appsList_SetContent(OutdatedApps);
                if (MSGBOX.Show(this, string.Format(Lang.GetText("UpdatesAvailableMsg0"), appsList.Items.Count, appsList.Items.Count > 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                    throw new Exception("Update canceled.");

                foreach (ListViewItem item in appsList.Items)
                    item.Checked = true; 
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                Environment.ExitCode = 1;
                Application.Exit();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (NotifyBox != null)
                NotifyBox.Close();
            LoadSettings();
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) =>
            appsList.Visible = false;

        private void MainForm_ResizeEnd(object sender, EventArgs e) =>
            appsList.Visible = true;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DownloadAmount > 0 && MSGBOX.Show(this, Lang.GetText("AreYouSureMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (SettingsLoaded && !SettingsDisabled)
            {
                if (WindowState != FormWindowState.Minimized)
                    INI.Write("Settings", "X.Window.State", WindowState != FormWindowState.Normal ? (FormWindowState?)WindowState : null);
                if (WindowState != FormWindowState.Maximized)
                {
                    INI.Write("Settings", "X.Window.Size.Width", Width);
                    INI.Write("Settings", "X.Window.Size.Height", Height);
                }
            }
            if (checkDownload.Enabled)
                checkDownload.Enabled = false;
            if (multiDownloader.Enabled)
                multiDownloader.Enabled = false;
            foreach (NET.ASYNCTRANSFER transfer in TransferManager.Values)
                transfer.CancelAsync();
            List<string> appInstaller = GetAllAppInstaller();
            if (appInstaller.Count > 0)
                RUN.Cmd($"PING 127.0.0.1 -n 3 >NUL && DEL /F /Q \"{string.Join("\" && DEL /F /Q \"", appInstaller)}\"");
        }

        private void LoadSettings()
        {
            int WindowWidth = INI.ReadInteger("Settings", "X.Window.Size.Width", MinimumSize.Width);
            if (WindowWidth > MinimumSize.Width && WindowWidth < Screen.PrimaryScreen.WorkingArea.Width)
                Width = WindowWidth;
            if (WindowWidth >= Screen.PrimaryScreen.WorkingArea.Width)
                Width = Screen.PrimaryScreen.WorkingArea.Width;
            Left = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (Width / 2);
            switch (TASKBAR.GetLocation())
            {
                case TASKBAR.Location.LEFT:
                    Left += TASKBAR.GetSize();
                    break;
                case TASKBAR.Location.RIGHT:
                    Left -= TASKBAR.GetSize();
                    break;
            }

            int WindowHeight = INI.ReadInteger("Settings", "X.Window.Size.Height", MinimumSize.Height);
            if (WindowHeight > MinimumSize.Height && WindowHeight < Screen.PrimaryScreen.WorkingArea.Height)
                Height = WindowHeight;
            if (WindowHeight >= Screen.PrimaryScreen.WorkingArea.Height)
                Height = Screen.PrimaryScreen.WorkingArea.Height;
            Top = (Screen.PrimaryScreen.WorkingArea.Height / 2) - (Height / 2);
            switch (TASKBAR.GetLocation())
            {
                case TASKBAR.Location.TOP:
                    Top += TASKBAR.GetSize();
                    break;
                case TASKBAR.Location.BOTTOM:
                    Top -= TASKBAR.GetSize();
                    break;
            }

            if (INI.Read("Settings", "X.Window.State").StartsWith("Max", StringComparison.OrdinalIgnoreCase))
                WindowState = FormWindowState.Maximized;

            showGroupsCheck.Checked = INI.ReadBoolean("Settings", "X.ShowGroups", true);
            showColorsCheck.Checked = INI.ReadBoolean("Settings", "X.ShowGroupColors", true);

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
                LOG.Debug(ex);
            }
            return list;
        }

        private List<string> GetInstalledApps(int _index = 1)
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
                LOG.Debug(ex);
            }
            return list;
        }

        private void appsList_Enter(object sender, EventArgs e) =>
            appsList_ShowColors(false);

        private void appsList_Resize(object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.Columns.Count == 5)
            {
                int staticColumnsWidth = SystemInformation.VerticalScrollBarWidth + 2;
                for (int i = 3; i < listView.Columns.Count; i++)
                    staticColumnsWidth += listView.Columns[i].Width;
                int dynamicColumnsWidth = 0;
                while (dynamicColumnsWidth < listView.Width - staticColumnsWidth)
                    dynamicColumnsWidth++;
                for (int i = 0; i < 3; i++)
                    listView.Columns[i].Width = (int)Math.Round(dynamicColumnsWidth / 100f * (i == 0 ? 35f : i == 1 ? 50f : 15f));
            }
        }

        private void appsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                // Some apps require specific apps to work correctly
                string requiredApps = INI.Read(appsList.Items[e.Index].Name, "Requires", AppsDBPath);
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
                LOG.Debug(ex);
            }
        }

        private void appsList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            bool TransferIsBusy = false;
            foreach (NET.ASYNCTRANSFER transfer in TransferManager.Values)
            {
                if (transfer.IsBusy)
                {
                    TransferIsBusy = true;
                    break;
                }
            }
            if (!multiDownloader.Enabled && !checkDownload.Enabled && !TransferIsBusy)
                okBtn.Enabled = appsList.CheckedItems.Count > 0;
        }

        private void appsList_ShowColors(bool _searchResultColor = true)
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
                    SWIcoDb = NET.DownloadData($"{SWSrv}/AppIcon.db", SWUsr, SWPwd);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }

            foreach (string section in _list)
            {
                string nam = INI.Read(section, "Name", AppsDBPath);
                string des = INI.Read(section, "Description", AppsDBPath);
                string cat = INI.Read(section, "Category", AppsDBPath);
                string ver = INI.Read(section, "Version", AppsDBPath);
                string pat = INI.Read(section, "ArchivePath", AppsDBPath);
                string siz = $"{INI.Read(section, "InstallSize", AppsDBPath)} MB";
                string adv = INI.Read(section, "Advanced", AppsDBPath);
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
                        LOG.Debug(ex);
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
                        string nameHash = (section.EndsWith("###") ? section.Substring(0, section.Length - 3) : section).EncryptToMD5();
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
                                    LOG.Debug(ex);
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
                        LOG.Debug(ex);
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
            if (!SettingsDisabled)
                INI.Write("Settings", "X.ShowGroups", !cb.Checked ? (bool?)false : null);
            appsList.ShowGroups = cb.Checked;
        }

        private void showColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsDisabled)
                INI.Write("Settings", "X.ShowGroupColors", !((CheckBox)sender).Checked ? (bool?)false : null);
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
            if (string.IsNullOrWhiteSpace(tb.Text))
                return;
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

        private void ResetSearch()
        {
            if (!SettingsDisabled && !string.IsNullOrWhiteSpace(searchBox.Text))
            {
                SettingsDisabled = true;
                showGroupsCheck.Checked = true;
            }
            else if (SettingsDisabled)
            {
                SettingsDisabled = false;
                showGroupsCheck.Checked = INI.ReadBoolean("Settings", "X.ShowGroups", true);
            }
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
            appsList_ShowColors(false);
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
            bool TransferIsBusy = false;
            foreach (NET.ASYNCTRANSFER transfer in TransferManager.Values)
            {
                if (transfer.IsBusy)
                {
                    TransferIsBusy = true;
                    break;
                }
            }
            if (!b.Enabled || appsList.Items.Count == 0 || TransferIsBusy)
                return;
            b.Enabled = false;
            TASKBAR.PROGRESS.SetState(Handle, TASKBAR.PROGRESS.States.Indeterminate);
            searchBox.Text = string.Empty;
            foreach (ListViewItem item in appsList.Items)
            {
                if (item.Checked)
                    continue;
                item.Remove();
            }
            foreach (string filePath in GetAllAppInstaller())
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            SettingsDisabled = true;
            DownloadCount = 1;
            DownloadAmount = appsList.CheckedItems.Count;
            appsList.HideSelection = !b.Enabled;
            appsList.Enabled = b.Enabled;
            appsList.Sort();
            showGroupsCheck.Checked = b.Enabled;
            showGroupsCheck.Enabled = b.Enabled;
            showColorsCheck.Enabled = b.Enabled;
            searchBox.Enabled = b.Enabled;
            cancelBtn.Enabled = b.Enabled;
            downloadSpeed.Visible = !b.Enabled;
            downloadProgress.Visible = !b.Enabled;
            downloadReceived.Visible = !b.Enabled;
            multiDownloader.Enabled = !b.Enabled;
        }

        private void multiDownloader_Tick(object sender, EventArgs e)
        {
            multiDownloader.Enabled = false;
            foreach (ListViewItem item in appsList.Items)
            {
                if (checkDownload.Enabled || !item.Checked)
                    continue;

                Text = string.Format(Lang.GetText("StatusDownload"), DownloadCount, DownloadAmount, item.Text);
                appStatus.Text = Text;
                urlStatus.Text = item.SubItems[item.SubItems.Count - 1].Text;

                string archivePath = INI.Read(item.Name, "ArchivePath", AppsDBPath);
                string archiveLangs = INI.Read(item.Name, "AvailableArchiveLangs", AppsDBPath);
                string archiveLang = string.Empty;
                bool archiveLangConfirmed = false;
                if (!string.IsNullOrWhiteSpace(archiveLangs) && archiveLangs.Contains(","))
                {
                    string defaultLang = archivePath.ToLower().Contains("multilingual") ? "Multilingual" : "English";
                    archiveLangs = $"Default ({defaultLang}),{archiveLangs}";
                    archiveLang = INI.Read(item.Name, "ArchiveLang");
                    archiveLangConfirmed = INI.ReadBoolean(item.Name, "ArchiveLangConfirmed", false);
                    if (!archiveLangs.Contains(archiveLang) || !archiveLangConfirmed)
                    {
                        try
                        {
                            DialogResult result = DialogResult.None;
                            while (result != DialogResult.OK)
                            {
                                using (Form dialog = new LangSelectionForm(item.Name, item.Text, archiveLangs.Split(',')))
                                {
                                    result = dialog.ShowDialog();
                                    if (result != DialogResult.OK)
                                    {
                                        Close();
                                        return;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                    archiveLang = INI.Read(item.Name, "ArchiveLang");
                    if (archiveLang.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                        archiveLang = string.Empty;
                    if (!string.IsNullOrWhiteSpace(archiveLang))
                        archivePath = INI.Read(item.Name, $"ArchivePath_{archiveLang}", AppsDBPath);
                }

                string localArchivePath = string.Empty;
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Group.Header == "*Shareware")
                        localArchivePath = Path.Combine(HomeDir, "Apps\\.share", archivePath.Replace("/", "\\"));
                    else
                        localArchivePath = Path.Combine(HomeDir, "Apps", archivePath.Replace("/", "\\"));
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

                DownloadFinished = 0;
                LastTransferItem = item.Text;
                if (TransferManager.ContainsKey(LastTransferItem))
                    TransferManager[LastTransferItem].CancelAsync();
                if (!TransferManager.ContainsKey(LastTransferItem))
                    TransferManager.Add(LastTransferItem, new NET.ASYNCTRANSFER());
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Group.Header == "*Shareware")
                        TransferManager[LastTransferItem].DownloadFile($"{(SWSrv.EndsWith("/") ? SWSrv.Substring(0, SWSrv.Length - 1) : SWSrv)}/{archivePath}", localArchivePath, SWUsr, SWPwd);
                    else
                    {
                        foreach (string mirror in Si13n7Mirrors)
                        {
                            string newArchivePath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{archivePath}";
                            if (NET.FileIsAvailable(newArchivePath))
                            {
                                LOG.Debug($"{Path.GetFileName(newArchivePath)} has been found on '{mirror}'.");
                                TransferManager[LastTransferItem].DownloadFile(newArchivePath, localArchivePath);
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
                        if (SourceForgeMirrorsSorted.Count == 0)
                        {
                            Dictionary<string, long> sortHelper = new Dictionary<string, long>();
                            foreach (string mirror in SourceForgeMirrors)
                            {
                                string path = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                NET.Ping(path);
                                if (!sortHelper.Keys.Contains(mirror))
                                    sortHelper.Add(mirror, NET.LastPingReply.RoundtripTime);
                            }
                            SourceForgeMirrorsSorted = sortHelper.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
                        }
                        if (SourceForgeMirrorsSorted.Count > 0)
                        {
                            foreach (string mirror in SourceForgeMirrorsSorted)
                            {
                                if (DownloadRetries < SourceForgeMirrorsSorted.Count - 1 && SfLastMirrors.ContainsKey(item.Name) && SfLastMirrors[item.Name].Contains(mirror))
                                    continue;
                                newArchivePath = archivePath.Replace("//downloads.sourceforge.net", $"//{mirror}");
                                if (NET.FileIsAvailable(newArchivePath))
                                {
                                    if (!SfLastMirrors.ContainsKey(item.Name))
                                        SfLastMirrors.Add(item.Name, new List<string>() { mirror });
                                    else
                                        SfLastMirrors[item.Name].Add(mirror);

                                    LOG.Debug($"'{Path.GetFileName(newArchivePath)}' has been found at '{mirror}'.");
                                    break;
                                }
                            }
                        }
                        TransferManager[LastTransferItem].DownloadFile(newArchivePath, localArchivePath);
                    }
                    else
                        TransferManager[LastTransferItem].DownloadFile(archivePath, localArchivePath);
                }
                DownloadCount++;
                item.Checked = false;
                checkDownload.Enabled = true;
                break;
            }
        }

        private void checkDownload_Tick(object sender, EventArgs e)
        {
            downloadSpeed.Text = $"{(int)Math.Round(TransferManager[LastTransferItem].TransferSpeed)} kb/s";
            downloadProgress_Update(TransferManager[LastTransferItem].ProgressPercentage);
            downloadReceived.Text = TransferManager[LastTransferItem].DataReceived;
            if (!TransferManager[LastTransferItem].IsBusy)
                DownloadFinished++;
            if (DownloadFinished >= 10)
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
                TASKBAR.PROGRESS.SetState(Handle, TASKBAR.PROGRESS.States.Indeterminate);
                List<string> appInstaller = GetAllAppInstaller();
                foreach (string filePath in appInstaller)
                {
                    if (!File.Exists(filePath))
                        continue;

                    string appDir = string.Empty;
                    if (!filePath.EndsWith(".paf.exe", StringComparison.OrdinalIgnoreCase))
                        appDir = filePath.RemoveText(".7z");
                    else
                    {
                        foreach (string dir in GetInstalledApps())
                        {
                            if (Path.GetFileName(filePath).StartsWith(Path.GetFileName(dir), StringComparison.OrdinalIgnoreCase))
                            {
                                appDir = dir;
                                break;
                            }
                        }
                    }

                    // Close running apps before overwrite
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
                                    LOG.Debug(ex);
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
                                RUN.Cmd($"TASKKILL /F /IM \"{string.Join("\" && TASKKILL /F /IM \"", TaskList)}\"", true);
                            }
                            catch (Exception ex)
                            {
                                LOG.Debug(ex);
                            }
                        }
                    }

                    // Install if file hashes are valid
                    if (WindowState != FormWindowState.Minimized)
                        WindowState = FormWindowState.Minimized;
                    foreach (KeyValuePair<string, NET.ASYNCTRANSFER> Transfer in TransferManager)
                    {
                        try
                        {
                            if (Transfer.Value.FilePath != filePath)
                                continue;
                            string fileName = Path.GetFileName(filePath);
                            foreach (string section in INI.GetSections(AppsDBPath))
                            {
                                if (filePath.Contains("\\.share\\") && !section.EndsWith("###"))
                                    continue;
                                if (!INI.Read(section, "ArchivePath", AppsDBPath).EndsWith(fileName))
                                {
                                    bool found = false;
                                    string archiveLangs = INI.Read(section, "AvailableArchiveLangs", AppsDBPath);
                                    if (archiveLangs.Contains(","))
                                        foreach (string lang in archiveLangs.Split(','))
                                            if (found = INI.Read(section, $"ArchivePath_{lang}", AppsDBPath).EndsWith(fileName))
                                                break;
                                    if (!found)
                                        continue;
                                }
                                string archiveLang = INI.Read(section, "ArchiveLang");
                                if (archiveLang.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                                    archiveLang = string.Empty;
                                string archiveHash = !string.IsNullOrEmpty(archiveLang) ? INI.Read(section, $"ArchiveHash_{archiveLang}", AppsDBPath) : INI.Read(section, "ArchiveHash", AppsDBPath);
                                string localHash = filePath.EncryptFileToMD5();
                                if (localHash == archiveHash)
                                    break;
                                throw new InvalidOperationException($"Checksum is invalid. - Key: '{Transfer.Key}'; Section: '{section}'; File: '{filePath}'; Current: '{archiveHash}'; Requires: '{localHash}';");
                            }
                            if (filePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                                PACKER.Zip7Helper.Unzip(filePath, appDir, ProcessWindowStyle.Minimized);
                            else
                            {
                                int pid = RUN.App(new ProcessStartInfo()
                                {
                                    Arguments = $"/DESTINATION=\"{Path.Combine(HomeDir, "Apps")}\\\"",
                                    FileName = filePath,
                                    WorkingDirectory = Path.Combine(HomeDir, "Apps")
                                }, 0);
                                if (pid <= 0)
                                    throw new InvalidOperationException($"'{filePath}' is corrupt.");
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                            Transfer.Value.HasCanceled = true;
                        }
                        break;
                    }
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
                if (WindowState != FormWindowState.Normal)
                    WindowState = FormWindowState.Normal;
                List<string> DownloadFails = new List<string>();
                foreach (KeyValuePair<string, NET.ASYNCTRANSFER> Transfer in TransferManager)
                    if (Transfer.Value.HasCanceled)
                        DownloadFails.Add(Transfer.Key);
                if (DownloadFails.Count > 0)
                {
                    TASKBAR.PROGRESS.SetState(Handle, TASKBAR.PROGRESS.States.Error);
                    if (DownloadRetries < SourceForgeMirrorsSorted.Count - 1 || MSGBOX.Show(this, string.Format(Lang.GetText("DownloadErrorMsg"), string.Join(Environment.NewLine, DownloadFails)), Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    {
                        DownloadRetries++;
                        foreach (ListViewItem item in appsList.Items)
                            if (DownloadFails.Contains(item.Text))
                                item.Checked = true;
                        appStatus.Text = string.Empty;
                        DownloadCount = 0;
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
                    TASKBAR.PROGRESS.SetValue(Handle, 100, 100);
                    MSGBOX.Show(this, string.Format(Lang.GetText("SuccessfullyDownloadMsg0"), appInstaller.Count == 1 ? Lang.GetText("App") : Lang.GetText("Apps"), UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                DownloadAmount = 0;
                Application.Exit();
            }
        }

        private void downloadProgress_Update(int _value)
        {
            TASKBAR.PROGRESS.SetValue(Handle, _value, 100);
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
            Process.Start($"http:\\{((Label)sender).Text}");
    }
}
