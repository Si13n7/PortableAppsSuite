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
        int DefaultWidth = 0;
        ListView AppListClone = new ListView();
        List<float> DefaultColumsWidth = new List<float>();
        int SearchResultBlinkCount = 0;

        List<string> DownloadServers = new List<string>();
        string AppsDBPath = string.Empty;
        List<string> WebInfoSections = new List<string>();

        int DlAsyncIsDoneCounter = 0, DlCount = 0, DlAmount = 0, DlFailRetry = 0;
        List<string> SfMirrors = new List<string>();
        Dictionary<string, List<string>> SfLastMirrors = new Dictionary<string, List<string>>();

        readonly string HomeDir = Path.GetFullPath($"{Application.StartupPath}\\..");

        readonly string SWSrv = SilDev.Initialization.ReadValue("Host", "Srv");
        readonly string SWUsr = SilDev.Initialization.ReadValue("Host", "Usr");
        readonly string SWPwd = SilDev.Initialization.ReadValue("Host", "Pwd");

        readonly bool UpdateSearch = Environment.CommandLine.Contains("7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e");

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_gray;
#if !x86
            Text = $"{Text} (64-bit)";
#endif
            Title = Text;
            AppList.Select();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            DefaultWidth = Width;
            for (int i = 0; i < AppList.Columns.Count; i++)
                DefaultColumsWidth.Add(AppList.Columns[i].Width);
            for (int i = 0; i < AppList.Columns.Count; i++)
            {
                AppList.Columns[i].Text = Lang.GetText($"columnHeader{i + 1}");
                DefaultColumsWidth.Add(AppList.Columns[i].Width);
            }
            for (int i = 0; i < AppList.Groups.Count; i++)
                AppList.Groups[i].Header = Lang.GetText(AppList.Groups[i].Name);
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
            {
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(this, Lang.GetText("InternetIsNotAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Environment.Exit(Environment.ExitCode);
            }
            DownloadServers = SilDev.Network.GetAvailableServers("raw.githubusercontent.com/Si13n7/_ServerInfos/master/Server-DNS.ini", InternetIsAvailable);
            if (DownloadServers.Count == 0)
            {
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(Lang.GetText("NoServerAvailableMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                Thread TipThread = null;
                if (!UpdateSearch)
                {
                    TipThread = new Thread(() => new TipForm(Text, Lang.GetText("DatabaseAccessMsg"), 0, FormStartPosition.CenterScreen).ShowDialog());
                    TipThread.Start();
                }
                AppsDBPath = Path.Combine(Application.StartupPath, "AppInfo.ini");
                SilDev.Network.DownloadFile("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/AppInfo.ini", AppsDBPath);
                if (!File.Exists(AppsDBPath))
                    throw new Exception("Server connection failed.");
                string ExternDBPath = Path.Combine(Application.StartupPath, "AppInfo.7z");
                string[] ExternDBSrvs = new string[]
                {
                    SilDev.Crypt.Base64.Decrypt("c2kxM243LmNvbS9Eb3dubG9hZHMvUG9ydGFibGUlMjBBcHBzJTIwU3VpdGUvLmZyZWUvUG9ydGFibGVBcHBzSW5mby43eg=="),
                    SilDev.Crypt.Base64.Decrypt("cG9ydGFibGVhcHBzLmNvbS91cGRhdGVyL3VwZGF0ZS43eg==")
                };
                foreach (string srv in ExternDBSrvs)
                {
                    int length = 0;
                    if (srv.StartsWith("si13n7.com"))
                    {
                        foreach (string mirror in DownloadServers)
                        {
                            string tmpSrv = srv.Replace("si13n7.com", mirror);
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
                WebInfoSections = SilDev.Initialization.GetSections(AppsDBPath);
                if (File.Exists(ExternDBPath))
                {
                    SilDev.Compress.Unzip7(ExternDBPath, Application.StartupPath);
                    File.Delete(ExternDBPath);
                    ExternDBPath = Path.Combine(Application.StartupPath, "update.ini");
                    if (File.Exists(ExternDBPath))
                    {
                        foreach (string section in SilDev.Initialization.GetSections(ExternDBPath))
                        {
                            string cat = SilDev.Initialization.ReadValue(section, "Category", ExternDBPath);
                            string nam = SilDev.Initialization.ReadValue(section, "Name", ExternDBPath);
                            if (WebInfoSections.Contains(section) || string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(nam) || nam.Contains("PortableApps.com"))
                                continue;
                            string pat = SilDev.Initialization.ReadValue(section, "DownloadPath", ExternDBPath);
                            pat = string.Format("{0}/{1}", string.IsNullOrWhiteSpace(pat) ? "http://downloads.sourceforge.net/portableapps" : pat, SilDev.Initialization.ReadValue(section, "DownloadFile", ExternDBPath));
                            if (!pat.EndsWith(".paf.exe", StringComparison.OrdinalIgnoreCase))
                                continue;
                            File.AppendAllText(AppsDBPath, Environment.NewLine);
                            if (!nam.StartsWith("jPortable", StringComparison.OrdinalIgnoreCase))
                            {
                                string tmp = new Regex(", Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
                                tmp = Regex.Replace(tmp, @"\s+", " ");
                                if (!string.IsNullOrWhiteSpace(tmp) && tmp != nam)
                                    nam = tmp;
                            }
                            string ver = SilDev.Initialization.ReadValue(section, "DisplayVersion", ExternDBPath);
                            string siz = SilDev.Initialization.ReadValue(section, "DownloadSize", ExternDBPath);
                            string des = SilDev.Initialization.ReadValue(section, "Description", ExternDBPath);
                            des = des.Replace(" Editor", " editor").Replace(" Reader", " reader");
                            string adv = SilDev.Initialization.ReadValue(section, "Advanced", ExternDBPath);
                            SilDev.Initialization.WriteValue(section, "Category", cat, AppsDBPath);
                            SilDev.Initialization.WriteValue(section, "Name", nam, AppsDBPath);
                            SilDev.Initialization.WriteValue(section, "ArchivePath", pat, AppsDBPath);
                            SilDev.Initialization.WriteValue(section, "Version", ver, AppsDBPath);
                            SilDev.Initialization.WriteValue(section, "Size", siz, AppsDBPath);
                            SilDev.Initialization.WriteValue(section, "Description", des, AppsDBPath);
                            if (adv.ToLower() == "true")
                                SilDev.Initialization.WriteValue(section, "Advanced", true, AppsDBPath);
                        }
                        File.Delete(ExternDBPath);
                    }
                    if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                    {
                        string ExternDB = SilDev.Network.DownloadString($"{SWSrv}/AppInfo.ini", SWUsr, SWPwd);
                        if (!string.IsNullOrWhiteSpace(ExternDB))
                            File.AppendAllText(AppsDBPath, $"{Environment.NewLine}{ExternDB}");
                    }
                    WebInfoSections = SilDev.Initialization.GetSections(AppsDBPath);
                }
                if (TipThread != null)
                    TipThread.Abort();
                if (TopMost)
                    TopMost = false;
                if (!UpdateSearch)
                {
                    AppList_SetContent(WebInfoSections);
                    if (AppList.Items.Count == 0)
                        throw new Exception("No available apps found.");
                    LoadSettings();
                    return;
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                List<string> UpdateInfo = new List<string>();
                foreach (string mirror in DownloadServers)
                {
                    string info = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/.free/index_virustotal.txt");
                    if (!string.IsNullOrWhiteSpace(info))
                    {
                        UpdateInfo.Add(info);
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                    UpdateInfo.Add(SilDev.Network.DownloadString(string.Format("{0}/index_virustotal.txt", SWSrv.EndsWith("/") ? SWSrv.Substring(0, SWSrv.Length - 1) : SWSrv), SWUsr, SWPwd));
                if (UpdateInfo.Count == 0 || UpdateInfo.Count > 0 && string.IsNullOrWhiteSpace(UpdateInfo[0]))
                    throw new Exception("Server connection failed.");
                Dictionary<string, string> hashList = new Dictionary<string, string>();
                foreach (string info in UpdateInfo)
                {
                    foreach (string line in info.Split(','))
                    {
                        string[] split = line.Replace(Environment.NewLine, string.Empty).Trim().Split(' ');
                        if (split.Length != 2)
                            continue;
                        if (!hashList.Keys.Contains(split[1]))
                            hashList.Add(split[1], split[0]);
                    }
                }
                if (hashList.Count == 0)
                    throw new Exception("No update data found.");
                List<string> OutdatedApps = new List<string>();
                foreach (string dir in GetInstalledApps(!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd) ? 3 : 0))
                {
                    string section = Path.GetFileName(dir);
                    if (!WebInfoSections.Contains(section))
                        continue;
                    bool NoUpdates = false;
                    bool.TryParse(SilDev.Initialization.ReadValue(section, "NoUpdates", Path.Combine(HomeDir, "AppsLauncher.ini")), out NoUpdates);
                    if (NoUpdates)
                        continue;
                    string file = SilDev.Initialization.ReadValue(section, "VerCheck", AppsDBPath);
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        string appIniPath = Path.Combine(dir, "App\\AppInfo\\appinfo.ini");
                        if (!File.Exists(appIniPath))
                            continue;
                        string localVersion = SilDev.Initialization.ReadValue("Version", "DisplayVersion", appIniPath);
                        string onlineVersion = SilDev.Initialization.ReadValue(section, "Version", AppsDBPath);
                        if (string.IsNullOrWhiteSpace(localVersion) || string.IsNullOrWhiteSpace(onlineVersion))
                            continue;
                        if (localVersion != onlineVersion)
                            OutdatedApps.Add(section);
                        continue;
                    }
                    string filePath = Path.Combine(dir, file);
                    if (!File.Exists(filePath) || string.IsNullOrWhiteSpace(hashList[file]))
                        continue;
                    if (SilDev.Crypt.SHA.EncryptFile(filePath, SilDev.Crypt.SHA.CryptKind.SHA256) != hashList[file])
                        OutdatedApps.Add(dir.Contains("\\.share\\") ? $"{section}###" : section);
                }
                if (OutdatedApps.Count == 0)
                {
                    Environment.ExitCode = 2;
                    throw new Exception("No updates available.");
                }
                AppList_SetContent(OutdatedApps);
                if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("UpdatesAvailableMsg0"), AppList.Items.Count, AppList.Items.Count > 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                    throw new Exception("Update canceled.");
                foreach (ListViewItem item in AppList.Items)
                    item.Checked = true;
                LoadSettings();
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (AppList.Columns.Count > 0)
            {
                for (int i = 0; i < AppList.Columns.Count; i++)
                {
                    float NewWidth = (DefaultColumsWidth[i] / DefaultWidth) * Width;
                    if (NewWidth > 50)
                        AppList.Columns[i].Width = (int)NewWidth;
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DlAmount > 0 && SilDev.MsgBox.Show(this, Lang.GetText("AreYouSureMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (CheckDownload.Enabled)
                CheckDownload.Enabled = false;
            if (MultiDownloader.Enabled)
                MultiDownloader.Enabled = false;
            SilDev.Network.CancelAsyncDownload();
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
            bool ShowGroupsIniValue = true;
            if (bool.TryParse(SilDev.Initialization.ReadValue("Settings", "ShowGroups"), out ShowGroupsIniValue))
                ShowGroupsCheck.Checked = ShowGroupsIniValue;
            bool ShowGroupColorsIniValue = true;
            if (bool.TryParse(SilDev.Initialization.ReadValue("Settings", "ShowGroupColors"), out ShowGroupColorsIniValue))
                ShowColorsCheck.Checked = ShowGroupColorsIniValue;
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

        private List<string> GetInstalledApps()
        {
            return GetInstalledApps(1);
        }

        private void AppList_Enter(object sender, EventArgs e)
        {
            AppList_ShowColors(false);
        }

        private void AppList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string requiredApps = SilDev.Initialization.ReadValue(AppList.Items[e.Index].Name, "Requires", AppsDBPath);
            if (!string.IsNullOrWhiteSpace(requiredApps))
            {
                if (!requiredApps.Contains(","))
                    requiredApps = $"{requiredApps},";
                foreach (string app in requiredApps.Split(','))
                {
                    foreach (ListViewItem item in AppList.Items)
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

        private void AppList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!MultiDownloader.Enabled && !CheckDownload.Enabled && !SilDev.Network.AsyncDownloadIsBusy())
                OKBtn.Enabled = AppList.CheckedItems.Count > 0;
        }

        private void AppList_ShowColors(bool _searchResultColor)
        {
            if (SearchResultBlinker.Enabled)
                SearchResultBlinker.Enabled = false;
            foreach (ListViewItem item in AppList.Items)
            {
                if (_searchResultColor && item.Group.Name == "listViewGroup0")
                {
                    item.ForeColor = SystemColors.HighlightText;
                    item.BackColor = SystemColors.Highlight;
                    continue;
                }
                item.ForeColor = AppList.ForeColor;
                item.BackColor = AppList.BackColor;
            }
            if (ShowColorsCheck.Checked)
            {
                foreach (ListViewItem item in AppList.Items)
                {
                    if (item.Group.Name != "listViewGroup0")
                        item.ForeColor = AppList.ForeColor;
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

        private void AppList_ShowColors()
        {
            AppList_ShowColors(true);
        }

        private void AppList_SetContent(List<string> _list)
        {
            Image DefaultIcon = Properties.Resources.PortableAppsBox;
            int index = 0;
            foreach (string section in _list)
            {
                string[] vars = new string[]
                {
                    SilDev.Initialization.ReadValue(section, "Category", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Name", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "ArchivePath", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Description", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Version", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Size", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Advanced", AppsDBPath),
                };
                string srcHost = "si13n7.com";
                if (vars[2].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string tmpHost = new Uri(vars[2]).Host;
                    string[] tmpSplit = tmpHost.Split('.');
                    srcHost = tmpSplit.Length >= 3 ? string.Join(".", tmpSplit[tmpSplit.Length - 2], tmpSplit[tmpSplit.Length - 1]) : tmpHost;
                }
                ListViewItem item = new ListViewItem(vars[1]);
                item.Name = section;
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[3]) ? vars[3] : string.Empty);
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[4]) ? vars[4] : "0.0.0.0");
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[5]) ? string.Format("{0} MB", vars[5]) : ">0 MB");
                item.SubItems.Add(!string.IsNullOrWhiteSpace(srcHost) ? srcHost : string.Empty);
                item.ImageIndex = index;
                if (section.EndsWith("###") && (string.IsNullOrEmpty(SWSrv) || string.IsNullOrEmpty(SWUsr) || string.IsNullOrEmpty(SWPwd)))
                    continue;
                if (!string.IsNullOrWhiteSpace(vars[0]))
                {
                    try
                    {
                        string nameHash = SilDev.Crypt.MD5.Encrypt(section);
                        string iconDbPath = Path.Combine(HomeDir, "Assets\\icon.db");
                        bool iconFound = false;
                        if (File.Exists(iconDbPath))
                        {
                            using (ZipArchive archive = ZipFile.OpenRead(iconDbPath))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.Name == nameHash)
                                    {
                                        imgList.Images.Add(Image.FromStream(entry.Open()));
                                        iconFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!iconFound)
                            throw new FileNotFoundException();
                    }
                    catch
                    {
                        imgList.Images.Add(DefaultIcon);
                    }
                    try
                    {
                        ListViewGroup group = new ListViewGroup(vars[0]);
                        if (!section.EndsWith("###"))
                        {
                            foreach (ListViewGroup gr in AppList.Groups)
                            {
                                if (string.IsNullOrWhiteSpace(vars[6]) && Lang.GetText("en-US", gr.Name) == vars[0] || Lang.GetText("en-US", gr.Name) == "*Advanced")
                                {
                                    AppList.Items.Add(item).Group = gr;
                                    break;
                                }
                            }
                        }
                        else
                            AppList.Items.Add(item).Group = AppList.Groups[AppList.Groups.Count - 1];
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
                index++;
            }
            AppList.SmallImageList = imgList;
            AppList_ShowColors();
            AppStatus.Text = string.Format(Lang.GetText(AppStatus), AppList.Items.Count, AppList.Items.Count == 1 ? "App" : "Apps");
        }

        private void ShowGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue("Settings", "ShowGroups", ShowGroupsCheck.Checked);
            AppList.ShowGroups = ShowGroupsCheck.Checked;
        }

        private void ShowColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue("Settings", "ShowGroupColors", ShowColorsCheck.Checked);
            AppList_ShowColors();
        }

        private void SearchBox_Enter(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string tmp = SearchBox.Text;
                SearchBox.Text = string.Empty;
                SearchBox.Text = tmp;
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                AppList.Select();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ResetSearch();
            TextBox tb = (TextBox)sender;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                string search = tb.Text.ToLower();
                foreach (ListViewItem item in AppList.Items)
                {
                    ListViewItem.ListViewSubItem description = item.SubItems[1];
                    if (description.Text.ToLower().Contains(search))
                    {
                        foreach (ListViewGroup group in AppList.Groups)
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
                        foreach (ListViewGroup group in AppList.Groups)
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
                if (!SearchResultBlinker.Enabled)
                    SearchResultBlinker.Enabled = true;
            }
        }

        private void ResetSearch()
        {
            if (!ShowGroupsCheck.Checked)
                ShowGroupsCheck.Checked = true;
            if (AppListClone.Items.Count == 0)
            {
                foreach (ListViewGroup group in AppList.Groups)
                    AppListClone.Groups.Add(new ListViewGroup()
                    {
                        Name = group.Name,
                        Header = group.Header,
                    });
                foreach (ListViewItem item in AppList.Items)
                    AppListClone.Items.Add((ListViewItem)item.Clone());
            }
            for (int i = 0; i < AppList.Items.Count; i++)
            {
                ListViewItem item = AppList.Items[i];
                ListViewItem clone = AppListClone.Items[i];
                foreach (ListViewGroup group in AppList.Groups)
                {
                    if (clone.Group.Name == group.Name)
                    {
                        if (clone.Group.Name != item.Group.Name)
                            group.Items.Add(item);
                        break;
                    }
                }
            }
            foreach (ListViewGroup group in AppList.Groups)
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
            AppList_ShowColors(false);
        }

        private void SearchResultBlinker_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer t = (System.Windows.Forms.Timer)sender;
            if (t.Enabled && SearchResultBlinkCount >= 5)
                t.Enabled = false;
            foreach (ListViewGroup group in AppList.Groups)
            {
                if (group.Name != "listViewGroup0")
                    continue;
                if (group.Items.Count > 0)
                {
                    foreach (ListViewItem item in AppList.Items)
                    {
                        if (item.Group.Name != group.Name)
                            continue;
                        if (!SearchResultBlinker.Enabled || item.BackColor != SystemColors.Highlight)
                        {
                            item.BackColor = SystemColors.Highlight;
                            t.Interval = 200;
                        }
                        else
                        {
                            item.BackColor = AppList.BackColor;
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

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (!b.Enabled || AppList.Items.Count == 0 || SilDev.Network.AsyncDownloadIsBusy())
                return;
            b.Enabled = false;
            foreach (ListViewItem item in AppList.Items)
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
            DlAmount = AppList.CheckedItems.Count;
            AppList.HideSelection = !b.Enabled;
            AppList.Enabled = b.Enabled;
            ShowGroupsCheck.Enabled = b.Enabled;
            ShowColorsCheck.Enabled = b.Enabled;
            SearchBox.Enabled = b.Enabled;
            CancelBtn.Enabled = b.Enabled;
            DLSpeed.Visible = !b.Enabled;
            DLPercentage.Visible = !b.Enabled;
            DLLoaded.Visible = !b.Enabled;
            MultiDownloader.Enabled = !b.Enabled;
            ResetSearch();
            SilDev.WinAPI.TaskBarProgress.SetState(Handle, SilDev.WinAPI.TaskBarProgress.States.Indeterminate);
        }

        private void MultiDownloader_Tick(object sender, EventArgs e)
        {
            MultiDownloader.Enabled = false;
            foreach (ListViewItem item in AppList.Items)
            {
                if (CheckDownload.Enabled || !item.Checked)
                    continue;
                Text = string.Format(Lang.GetText("StatusDownload"), DlCount, DlAmount, item.Text);
                AppStatus.Text = Text;
                string archivePath = SilDev.Initialization.ReadValue(item.Name, "ArchivePath", AppsDBPath);
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
                        SilDev.Network.DownloadFileAsync(item.Text, string.Format("{0}/{1}", SWSrv.EndsWith("/") ? SWSrv.Substring(0, SWSrv.Length - 1) : SWSrv, archivePath), localArchivePath, SWUsr, SWPwd);
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
                CheckDownload.Enabled = true;
                break;
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = SilDev.Network.LatestAsyncDownloadInfo.TransferSpeed;
            DLPercentage_SetProgress(SilDev.Network.LatestAsyncDownloadInfo.ProgressPercentage);
            DLLoaded.Text = SilDev.Network.LatestAsyncDownloadInfo.DataReceived;
            if (!SilDev.Network.AsyncDownloadIsBusy())
                DlAsyncIsDoneCounter++;
            if (DlAsyncIsDoneCounter >= 10)
            {
                CheckDownload.Enabled = false;
                if (AppList.CheckedItems.Count > 0)
                {
                    MultiDownloader.Enabled = true;
                    return;
                }
                Text = Title;
                AppStatus.Text = Lang.GetText("StatusExtract");
                DLSpeed.Visible = false;
                DLLoaded.Visible = false;
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
                        if (file.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                            SilDev.Compress.Unzip7(file, appDir, false);
                        else
                        {
                            if (SilDev.Run.App(new ProcessStartInfo()
                            {
                                Arguments = $"/DESTINATION=\"{Path.Combine(HomeDir, "Apps")}\\\"",
                                FileName = file,
                                WorkingDirectory = Path.Combine(HomeDir, "Apps")
                            }, 0) == null)
                            {
                                foreach (var info in SilDev.Network.AsyncDownloadInfo)
                                {
                                    if (info.Value.FilePath == file)
                                    {
                                        SilDev.Network.ASYNCDOWNLOADINFODATA state = info.Value;
                                        state.StatusCode = 3;
                                        state.StatusMessage = "Download failed!";
                                        SilDev.Network.AsyncDownloadInfo[info.Key] = state;
                                        break;
                                    }
                                }
                            }
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
                        foreach (ListViewItem item in AppList.Items)
                            if (DownloadFails.Contains(item.Text))
                                item.Checked = true;
                        AppStatus.Text = string.Empty;
                        DlCount = 0;
                        AppList.Enabled = true;
                        AppList.HideSelection = !AppList.Enabled;
                        DLSpeed.Text = string.Empty;
                        DLPercentage_SetProgress(0);
                        DLPercentage.Visible = !AppList.Enabled;
                        DLLoaded.Text = string.Empty;
                        ShowGroupsCheck.Enabled = AppList.Enabled;
                        ShowColorsCheck.Enabled = AppList.Enabled;
                        SearchBox.Enabled = AppList.Enabled;
                        OKBtn.Enabled = AppList.Enabled;
                        CancelBtn.Enabled = AppList.Enabled;
                        OKBtn_Click(OKBtn, null);
                        return;
                    }
                }
                else
                {
                    SilDev.WinAPI.TaskBarProgress.SetValue(Handle, 100, 100);
                    SilDev.MsgBox.Show(this, string.Format(Lang.GetText("SuccessfullyDownloadMsg0"), appInstaller.Count == 1 ? "App" : "Apps", UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void DLPercentage_SetProgress(int _value)
        {
            SilDev.WinAPI.TaskBarProgress.SetValue(Handle, _value, 100);
            using (Graphics g = DLPercentage.CreateGraphics())
            {
                int width = _value > 0 && _value < 100 ? DLPercentage.Width / 100 * _value : DLPercentage.Width;
                using (Brush b = new SolidBrush(_value > 0 ? Color.DodgerBlue : DLPercentage.BackColor))
                    g.FillRectangle(b, 0, 0, width, DLPercentage.Height);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void UrlStatus_Click(object sender, EventArgs e)
        {
            Process.Start("http:\\www.si13n7.com");
        }
    }
}
