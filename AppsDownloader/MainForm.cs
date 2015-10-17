using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        static int DefaultWidth = 0;
        static List<float> DefaultColumsWidth = new List<float>();

        static string HomeDir = Path.GetFullPath(string.Format("{0}\\..", Application.StartupPath));

        static string DownloadServer = string.Empty;
        static string AppsDBPath = string.Empty;
        static List<string> WebInfoSections = new List<string>();

        static string IniPath = Path.Combine(Application.StartupPath, "AppsDownloader.ini");
        static string SWSrv = SilDev.Initialization.ReadValue("Host", "Srv", IniPath);
        static string SWUsr = SilDev.Initialization.ReadValue("Host", "Usr", IniPath);
        static string SWPwd = SilDev.Initialization.ReadValue("Host", "Pwd", IniPath);

        static bool UpdateSearch = Environment.CommandLine.Contains("7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e");

#if x86
        static string SevenZipPath = Path.Combine(Application.StartupPath, "Helper\\7z\\7zG.exe");
#else
        static string SevenZipPath = Path.Combine(Application.StartupPath, "Helper\\7z\\x64\\7zG.exe");
#endif

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_gray;
#if !x86
            Text = string.Format("{0} (64-bit)", Text);
#endif
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
                AppList.Columns[i].Text = Lang.GetText(string.Format("columnHeader{0}", i + 1));
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
            DownloadServer = SilDev.Network.GetTheBestServer("raw.githubusercontent.com/Si13n7/_ServerInfos/master/Server-DNS.ini", InternetIsAvailable);
            if (string.IsNullOrWhiteSpace(DownloadServer))
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
                SilDev.Network.DownloadFile("https://raw.githubusercontent.com/Si13n7/Portable-World-Project/master/AppInfo.ini", AppsDBPath);
                if (!File.Exists(AppsDBPath))
                    throw new Exception("Server connection failed.");
                string ExternDBPath = Path.Combine(Application.StartupPath, "AppInfo.7z");
                string[] ExternDBSrvs = new string[]
                {
                    SilDev.Crypt.Base64.Decrypt("aHR0cDovL3d3dy5zaTEzbjcuY29tL0Rvd25sb2Fkcy9Qb3J0YWJsZSUyMFdvcmxkLy5mcmVlL1BvcnRhYmxlQXBwc0luZm8uN3o="),
                    SilDev.Crypt.Base64.Decrypt("aHR0cDovL3MwLnNpMTNuNy5jb20vRG93bmxvYWRzL1BvcnRhYmxlJTIwV29ybGQvLmZyZWUvUG9ydGFibGVBcHBzSW5mby43eg=="),
                    SilDev.Crypt.Base64.Decrypt("aHR0cDovL3BvcnRhYmxlYXBwcy5jb20vdXBkYXRlci91cGRhdGUuN3o=")
                };
                foreach (string srv in ExternDBSrvs)
                {
                    SilDev.Network.DownloadFile(srv, ExternDBPath);
                    int length = 0;
                    if (File.Exists(ExternDBPath))
                    {
                        length = (int)(new FileInfo(ExternDBPath).Length / 1024);
                        if (File.Exists(ExternDBPath) && length > 24)
                            break;
                    }
                }
                WebInfoSections = SilDev.Initialization.GetSections(AppsDBPath);
                if (File.Exists(ExternDBPath))
                {
                    SilDev.Run.App(new ProcessStartInfo() { FileName = SevenZipPath, Arguments = string.Format("x \"\"\"{0}\"\"\" -o\"\"\"{1}\"\"\" -y", ExternDBPath, Application.StartupPath), WindowStyle = ProcessWindowStyle.Hidden }, 0);
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
                                string tmp = new Regex("(PortableApps.com Launcher)|, Portable Edition|Portable64|Portable", RegexOptions.IgnoreCase).Replace(nam, string.Empty);
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
                            SilDev.Initialization.WriteValue(section, "Website", "PortableApps.com", AppsDBPath);
                            if (adv.ToLower() == "true")
                                SilDev.Initialization.WriteValue(section, "Advanced", true, AppsDBPath);
                        }
                        File.Delete(ExternDBPath);
                    }
                    if (!string.IsNullOrEmpty(SWSrv) && !string.IsNullOrEmpty(SWUsr) && !string.IsNullOrEmpty(SWPwd))
                    {
                        string ExternDB = SilDev.Network.DownloadString(string.Format("{0}/AppInfo.ini", SWSrv), SWUsr, SWPwd);
                        if (!string.IsNullOrWhiteSpace(ExternDB))
                            File.AppendAllText(AppsDBPath, string.Format("{0}{1}", Environment.NewLine, ExternDB));
                    }
                    WebInfoSections = SilDev.Initialization.GetSections(AppsDBPath);
                }
                if (TipThread != null)
                    TipThread.Abort();
                if (!UpdateSearch)
                {
                    SetAppList(WebInfoSections);
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
                UpdateInfo.Add(SilDev.Network.DownloadString(string.Format("{0}/Portable%20World/.free/index_virustotal.txt", DownloadServer)));
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
                        OutdatedApps.Add(dir.Contains("\\.share\\") ? string.Format("{0}###", section) : section);
                }
                if (OutdatedApps.Count == 0)
                {
                    Environment.ExitCode = 2;
                    throw new Exception("No updates available.");
                }
                SetAppList(OutdatedApps);
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

        private void LoadSettings()
        {
            bool ShowGroupsIniValue = true;
            if (bool.TryParse(SilDev.Initialization.ReadValue("Settings", "ShowGroups", IniPath), out ShowGroupsIniValue))
                ShowGroupsCheck.Checked = ShowGroupsIniValue;
            bool ShowGroupColorsIniValue = true;
            if (bool.TryParse(SilDev.Initialization.ReadValue("Settings", "ShowGroupColors", IniPath), out ShowGroupColorsIniValue))
                ShowColorsCheck.Checked = ShowGroupColorsIniValue;
        }

        private void SetAppList(List<string> _list)
        {
            Image[] DefaultIcons = new Image[]
            {
                ImageHighQualityResize((Properties.Resources.PortableApps_gray).ToBitmap(), 16, 16),
                ImageHighQualityResize(Properties.Resources.PortableAppsComInstaller, 16, 16)
            };
            int index = 0;
            foreach (string section in _list)
            {
                string[] vars = new string[]
                {
                    SilDev.Initialization.ReadValue(section, "Category", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Name", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Description", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Version", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Size", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Website", AppsDBPath),
                    SilDev.Initialization.ReadValue(section, "Advanced", AppsDBPath),
                };
                ListViewItem item = new ListViewItem(vars[1]);
                item.Name = section;
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[2]) ? vars[2] : string.Empty);
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[3]) ? vars[3] : "0.0.0.0");
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[4]) ? string.Format("{0} MB", vars[4]) : ">0 MB");
                item.SubItems.Add(!string.IsNullOrWhiteSpace(vars[5]) ? vars[5] : string.Empty);
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
                                        imgList.Images.Add(ImageHighQualityResize(Image.FromStream(entry.Open()), 16, 16));
                                        iconFound = true;
                                    }
                                }
                            }
                        }
                        if (!iconFound)
                            throw new Exception();
                    }
                    catch
                    {
                        imgList.Images.Add(vars[5].StartsWith("PortableApps", StringComparison.OrdinalIgnoreCase) ? DefaultIcons[1] : DefaultIcons[0]);
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
                        index++;
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
            AppList.SmallImageList = imgList;
            ShowColors();
            AppStatus.Text = string.Format(Lang.GetText(AppStatus), AppList.Items.Count, AppList.Items.Count == 1 ? "App" : "Apps");
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

        private void MainForm_Shown(object sender, EventArgs e)
        {
            SilDev.WinAPI.SetForegroundWindow(Handle);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppList.CheckedItems.Count > 0 && SilDev.MsgBox.Show(this, Lang.GetText("AreYouSureMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        private void AppList_Enter(object sender, EventArgs e)
        {
            ShowColors();
        }

        private void AppList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string requiredApps = SilDev.Initialization.ReadValue(AppList.Items[e.Index].Name, "Requires", AppsDBPath);
            if (!string.IsNullOrWhiteSpace(requiredApps))
            {
                if (!requiredApps.Contains(","))
                    requiredApps = string.Format("{0},", requiredApps);
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
            OKBtn.Enabled = AppList.CheckedItems.Count > 0;
        }

        private void ShowGroupsCheck_CheckedChanged(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue("Settings", "ShowGroups", ShowGroupsCheck.Checked, IniPath);
            AppList.ShowGroups = ShowGroupsCheck.Checked;
        }

        private void ShowColorsCheck_CheckedChanged(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue("Settings", "ShowGroupColors", ShowColorsCheck.Checked, IniPath);
            ShowColors();
        }

        private void ShowColors()
        {
            try
            {
                foreach (ListViewItem item in AppList.Items)
                {
                    item.ForeColor = AppList.ForeColor;
                    item.BackColor = AppList.BackColor;
                }
                if (ShowColorsCheck.Checked)
                {
                    foreach (ListViewItem item in AppList.Items)
                    {
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
                                item.BackColor = ColorTranslator.FromHtml("#88BBDD");
                                break;
                            case "listViewGroup10": // Games
                                // No special color
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
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
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
            try
            {
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    string search = SearchBox.Text.ToLower();
                    string[] split = null;
                    if (search.Contains("*") && !search.StartsWith("*") && !search.EndsWith("*"))
                        split = search.Split('*');
                    bool match = false;
                    for (int i = 0; i < 2; i++)
                    {
                        foreach (ListViewItem item in AppList.Items)
                        {
                            if (i < 1 && split != null && split.Length == 2)
                            {
                                Regex regex = new Regex(string.Format(".*{0}(.*){1}.*", split[0], split[1]), RegexOptions.IgnoreCase);
                                match = regex.IsMatch(item.Name);
                            }
                            else
                            {
                                match = item.ToString().StartsWith(search, StringComparison.OrdinalIgnoreCase);
                                if (i > 0 && !match)
                                    match = item.Name.ToLower().Contains(search);
                            }
                            if (match)
                            {
                                ShowColors();
                                if (ShowColorsCheck.Checked)
                                {
                                    item.ForeColor = AppList.BackColor;
                                    item.BackColor = AppList.ForeColor;
                                }
                                else
                                {
                                    item.ForeColor = SystemColors.HighlightText;
                                    item.BackColor = SystemColors.Highlight;
                                }
                                item.Selected = true;
                                item.EnsureVisible();
                                AppList.EnsureVisible(item.Index);
                                return;
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

        private void OKBtn_Click(object sender, EventArgs e)
        {
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
            AppList.HideSelection = true;
            AppList.Enabled = false;
            ShowGroupsCheck.Enabled = false;
            ShowColorsCheck.Enabled = false;
            SearchBox.Enabled = false;
            OKBtn.Enabled = false;
            CancelBtn.Enabled = false;
            DLSpeed.Visible = true;
            DLPercentage.Visible = true;
            DLLoaded.Visible = true;
            MultiDownloader.Enabled = true;
        }

        int count = 0;
        bool LastDownload = false;
        private void MultiDownloader_Tick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in AppList.CheckedItems)
            {
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
                    localArchivePath = Path.Combine(HomeDir, string.Format("Apps\\{0}", tmp[tmp.Length - 1]));
                }
                if (File.Exists(localArchivePath) || CheckDownload.Enabled)
                    continue;
                if (!Directory.Exists(Path.GetDirectoryName(localArchivePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(localArchivePath));
                count = 0;
                if (!archivePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Group.Header == "*Shareware")
                        SilDev.Network.DownloadFileAsync(string.Format("{0}/{1}", SWSrv.EndsWith("/") ? SWSrv.Substring(0, SWSrv.Length - 1) : SWSrv, archivePath), localArchivePath, SWUsr, SWPwd);
                    else
                        SilDev.Network.DownloadFileAsync(string.Format("{0}/Portable%20World/{1}", DownloadServer, archivePath), localArchivePath);
                }
                else
                    SilDev.Network.DownloadFileAsync(archivePath, localArchivePath);
                CheckDownload.Enabled = true;
                MultiDownloader.Enabled = false;
                if (item == AppList.CheckedItems[AppList.CheckedItems.Count - 1])
                    LastDownload = true;
            }
        }
        
        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = SilDev.Network.DownloadInfo.GetTransferSpeed;
            DLPercentage.Value = SilDev.Network.DownloadInfo.GetProgressPercentage;
            DLLoaded.Text = SilDev.Network.DownloadInfo.GetDataReceived;
            if (!SilDev.Network.AsyncIsBusy())
                count++;
            if (count == 1)
            {
                DLPercentage.Maximum = 1000;
                DLPercentage.Value = 1000;
                DLPercentage.Value--;
                DLPercentage.Maximum = 100;
                DLPercentage.Value = 100;
            }
            if (count >= 10)
            {
                CheckDownload.Enabled = false;
                if (!LastDownload)
                {
                    MultiDownloader.Enabled = true;
                    return;
                }
                DLSpeed.Visible = false;
                DLLoaded.Visible = false;
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
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                            }
                            if (!p.HasExited && !TaskList.Contains(p.ProcessName))
                                TaskList.Add(p.ProcessName);
                        }
                    }
                    if (TaskList.Count > 0)
                        SilDev.Run.App(new ProcessStartInfo() { FileName = "%WinDir%\\System32\\cmd.exe", Arguments = string.Format("/C TASKKILL /F /IM \"{0}\"", string.Join("\" && TASKKILL /F /IM \"", TaskList)), Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden }, 0);
                    if (TopMost)
                        TopMost = false;
                    if (file.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                        SilDev.Run.App(new ProcessStartInfo() { FileName = SevenZipPath, Arguments = string.Format("x \"\"\"{0}\"\"\" -o\"\"\"{1}\"\"\" -y", file, appDir) }, 0);
                    else
                        SilDev.Run.App(new ProcessStartInfo() { FileName = file, WorkingDirectory = Path.Combine(HomeDir, "Apps") }, 0);
                    File.Delete(file);
                }
                if (appInstaller.Count > 0)
                    SilDev.MsgBox.Show(this, string.Format(Lang.GetText("SuccessfullyDownloadMsg0"), AppList.CheckedItems.Count == 1 ? "App" : "Apps", UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    SilDev.MsgBox.Show(this, Lang.GetText("DownloadErrorMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UrlStatus_Click(object sender, EventArgs e)
        {
            Process.Start("http:\\www.si13n7.com");
        }

        private static Bitmap ImageHighQualityResize(Image image, int width, int heigth)
        {
            Bitmap bmp = new Bitmap(width, heigth);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.CompositingMode = CompositingMode.SourceCopy;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                using (ImageAttributes imgAttrib = new ImageAttributes())
                {
                    imgAttrib.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(image, new Rectangle(0, 0, width, heigth), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttrib);
                }
            }
            return bmp;
        }
    }
}
