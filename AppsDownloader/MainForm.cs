using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AppsDownloader
{
    public partial class MainForm : Form
    {
        static string homePath = Path.GetFullPath(string.Format("{0}\\..", Application.StartupPath));

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_gray;
        }

        bool UpdateSearch = Environment.CommandLine.Contains("7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e");
        Dictionary<string, string[]> AppsList = new Dictionary<string, string[]>();
        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
            {
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(this, Lang.GetText("InternetIsNotAvailableMsg"), Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Environment.Exit(Environment.ExitCode);
            }
            string DownloadServer = SilDev.Network.GetTheBestServer("raw.githubusercontent.com/Si13n7/_ServerInfos/master/Server-DNS.ini", InternetIsAvailable);
            if (string.IsNullOrWhiteSpace(DownloadServer))
            {
                if (!UpdateSearch)
                    SilDev.MsgBox.Show(Lang.GetText("NoServerAvailableMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                string WebInfo = SilDev.Network.DownloadString(string.Format("{0}/Portable%20World/AppInfo.txt", DownloadServer));
                if (string.IsNullOrWhiteSpace(WebInfo))
                    throw new Exception("Server connection failed.");
                foreach (string line in WebInfo.Split(','))
                {
                    string[] infoList = line.Replace(Environment.NewLine, string.Empty).Split('|');
                    if (infoList.Length < 3)
                        continue;
                    if (!UpdateSearch && infoList[0].StartsWith(".share", StringComparison.OrdinalIgnoreCase) || UpdateSearch && !infoList[0].StartsWith(".free", StringComparison.OrdinalIgnoreCase))
                        break;
                    string filePath = Path.Combine(homePath, string.Format("Apps\\{0}", infoList[0].Replace("/", "\\")));
                    string appDir = filePath.Replace(".sfx.exe", string.Empty);
                    if (AppsList.ContainsKey(infoList[1]) || !UpdateSearch && (Directory.Exists(appDir) || Directory.Exists(appDir.Replace(".free", ".share"))))
                        continue;
                    AppsList.Add(infoList[1], new string[] { string.Format("{0}/Portable%20World/{1}", DownloadServer, infoList[0]), filePath, infoList[2], infoList.Length > 3 ? infoList[3] : string.Empty });
                }
                if (!UpdateSearch)
                {
                    if (AppsList.Count == 0)
                        throw new Exception("No available apps found.");
                    List<string> AppNames = AppsList.Keys.ToList();
                    AppNames.Sort();
                    foreach (string item in AppNames)
                        appList.Items.Add(item);
                }
                if (!UpdateSearch)
                    return;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.Exit(Environment.ExitCode);
            }
            try
            {
                string UpdateInfo = SilDev.Network.DownloadString(string.Format("{0}/Portable%20World/.free/index_virustotal.txt", DownloadServer));
                if (string.IsNullOrWhiteSpace(UpdateInfo))
                    throw new Exception("Server connection failed.");
                Dictionary<string, string> infoDict = new Dictionary<string, string>();
                foreach (string line in UpdateInfo.Split(','))
                {
                    string[] split = line.Replace(Environment.NewLine, string.Empty).Trim().Split(' ');
                    if (split.Length != 2)
                        continue;
                    infoDict.Add(split[1], split[0]);
                }
                if (infoDict.Count == 0)
                    throw new Exception("No update data found.");
                List<string> AppNames = new List<string>();
                foreach (KeyValuePair<string, string[]> app in AppsList)
                {
                    if (app.Value.Length < 4 || app.Value.Length >= 4 && string.IsNullOrWhiteSpace(app.Value[3]))
                        continue;
                    if (infoDict.ContainsKey(app.Value[3]))
                    {
                        string appPath = Path.Combine(app.Value[1].Replace(".sfx.exe", string.Empty), app.Value[3]);
                        if (File.Exists(appPath))
                        {
                            string currentVersion = SilDev.Crypt.SHA.EncryptFile(appPath, SilDev.Crypt.SHA.CryptKind.SHA256);
                            string onlineVersion = infoDict[app.Value[3]];
                            if (currentVersion.ToLower() != onlineVersion.ToLower())
                                AppNames.Add(app.Key);
                        }
                    }
                }
                AppNames.Sort();
                foreach (string item in AppNames)
                    appList.Items.Add(item);
                if (appList.Items.Count == 0)
                    throw new Exception("No updates available.");
                if (!SilDev.Elevation.IsAdministrator)
                {
                    if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("UpdatesAvailableMsg0"), appList.Items.Count, appList.Items.Count > 1 ? Lang.GetText("UpdatesAvailableMsg1") : Lang.GetText("UpdatesAvailableMsg2")), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        SilDev.Elevation.RestartAsAdministrator("7fc552dd-328e-4ed8-b3c3-78f4bf3f5b0e");
                        throw new Exception("Restart as administrator.");
                    }
                    throw new Exception("Update canceled.");
                }
                for (int i = 0; i < appList.Items.Count; i++)
                    appList.SetItemChecked(i, true);
                OKBtn.Enabled = appList.CheckedItems.Count > 0;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            SilDev.WinAPI.SetForegroundWindow(Handle);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (appList.CheckedItems.Count > 0 && SilDev.MsgBox.Show(this, Lang.GetText("AreYouSureMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        private void appList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OKBtn.Enabled = appList.CheckedItems.Count > 0;
            if (appList.SelectedIndex >= 0)
            {
                string item = appList.SelectedItem.ToString();
                AppStatus.Text = string.Format("|  {0}  |  Version: {1}", item, AppsList[item][2]);
                if (AppStatus.Text.EndsWith(","))
                    AppStatus.Text = AppStatus.Text.Substring(0, AppStatus.Text.Length - 1);
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var key in appList.CheckedItems)
                {
                    string filePath = AppsList[key.ToString()][1];
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }

            appList.SelectedIndex = -1;
            appList.Enabled = false;

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
            foreach (var key in appList.CheckedItems)
            {
                string filePath = AppsList[key.ToString()][1];
                if (File.Exists(filePath) || CheckDownload.Enabled)
                    continue;
                AppStatus.Text = string.Format("|  {0}  |  Version: {1}", key.ToString(), AppsList[key.ToString()][2]);
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                count = 0;
                SilDev.Network.DownloadFileAsync(AppsList[key.ToString()][0], filePath);
                CheckDownload.Enabled = true;
                MultiDownloader.Enabled = false;
                if (key == appList.CheckedItems[appList.CheckedItems.Count - 1])
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
                List<string> closedApps = new List<string>();
                foreach (object key in appList.CheckedItems)
                {
                    string filePath = AppsList[key.ToString()][1];
                    string appDir = filePath.Replace(".sfx.exe", string.Empty);
                    if (Directory.Exists(appDir))
                    {
                        try
                        {
                            foreach (string f in Directory.GetFiles(appDir, "*.exe", SearchOption.AllDirectories))
                            {
                                foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
                                {
                                    p.CloseMainWindow();
                                    p.WaitForExit(100);
                                    if (!p.HasExited)
                                        p.Kill();
                                    if (!closedApps.Contains(f) && (f.EndsWith("portable.exe", StringComparison.OrdinalIgnoreCase) || f.EndsWith("portable64.exe", StringComparison.OrdinalIgnoreCase)))
                                        closedApps.Add(f);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                    SilDev.Run.App(Path.GetDirectoryName(filePath), Path.GetFileName(filePath), "-s2", 0);
                    File.Delete(filePath);
                }
                foreach (string app in closedApps)
                    SilDev.Run.App(app);
                SilDev.MsgBox.Show(this, string.Format(Lang.GetText("SuccessfullyDownloadMsg0"), appList.CheckedItems.Count > 1 ? "Apps" : "App", UpdateSearch ? Lang.GetText("SuccessfullyDownloadMsg1") : Lang.GetText("SuccessfullyDownloadMsg2")), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                appList.Items.Clear();
                Close();
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
    }
}
