using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    public partial class MainForm : Form
    {
        static readonly string HomeDir = Path.GetFullPath($"{Application.StartupPath}\\..");
        Dictionary<string, Dictionary<string, string>> HashInfo = new Dictionary<string, Dictionary<string, string>>();
        static readonly string UpdateDir = SilDev.Run.EnvironmentVariableFilter($"%TEMP%\\PortableAppsSuite-{{{Guid.NewGuid()}}}");
        readonly string UpdatePath = Path.Combine(UpdateDir, "Update.7z");
        static List<string> DownloadServers = new List<string>();
        string SnapshotLastStamp = null;
        int DlAsyncIsBusyCounter = 0;

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_green_64;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);

            // Checking connection to the internet
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
            {
                Environment.ExitCode = 1;
                Close();
            }

            // Get file hashes from GitHub if enabled
            if (SilDev.Ini.Read("Settings", "UpdateChannel").ToLower() == "beta")
            {
                string Last = SilDev.Network.DownloadString("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/SNAPSHOTS/Last.ini");
                if (!string.IsNullOrWhiteSpace(Last))
                {
                    SnapshotLastStamp = SilDev.Ini.Read("Info", "LastStamp", Last);
                    if (!string.IsNullOrWhiteSpace(SnapshotLastStamp))
                        HashInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString($"https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/SNAPSHOTS/{SnapshotLastStamp}.ini"));
                }
            }

            // Get file hashes if not already set
            if (HashInfo.Count == 0)
            {
                // Get download mirrors
                Dictionary<string, Dictionary<string, string>> DnsInfo = new Dictionary<string, Dictionary<string, string>>();
                for (int i = 0; i < 3; i++)
                {
                    DnsInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
                    if (DnsInfo.Count == 0 && i < 2)
                        Thread.Sleep(1000);
                }
                if (DnsInfo.Count > 0)
                {
                    foreach (string section in DnsInfo.Keys)
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
                        if (!DownloadServers.Contains(domain))
                            DownloadServers.Add(domain);
                    }
                }
                if (DownloadServers.Count == 0)
                {
                    Environment.ExitCode = 1;
                    Close();
                }
                // Get file hashes
                foreach (string mirror in DownloadServers)
                {
                    HashInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/SHA256Sums.ini"));
                    if (HashInfo.Count > 0)
                        break;
                }
            }

            if (HashInfo.Count == 0)
            {
                Environment.ExitCode = 1;
                Close();
            }

            // Compare hashes
            bool UpdateAvailable = false;
            if (HashInfo["SHA256"].Count == 5)
            {
                foreach (string key in HashInfo["SHA256"].Keys)
                {
                    string file = Path.Combine(HomeDir, $"{key}.exe");
                    if (!File.Exists(file))
                        file = Path.Combine(Application.StartupPath, $"{key}.exe");
                    if (SilDev.Crypt.SHA.EncryptFile(file, SilDev.Crypt.SHA.CryptKind.SHA256) != HashInfo["SHA256"][key])
                    {
                        UpdateAvailable = true;
                        break;
                    }
                }
            }

            // Close apps suite programs
            if (UpdateAvailable)
            {
                if (MessageBox.Show(Lang.GetText("UpdateAvailableMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    List<string> AppsSuiteItemList = new List<string>()
                    {
                        Path.Combine(HomeDir, "AppsLauncher.exe"),
                        Path.Combine(HomeDir, "AppsLauncher64.exe")
                    };
                    AppsSuiteItemList.AddRange(Directory.GetFiles(Application.StartupPath, "*.exe", SearchOption.AllDirectories).Where(s => s.ToLower() != Application.ExecutablePath.ToLower()));
                    List<string> TaskList = new List<string>();
                    foreach (string item in AppsSuiteItemList)
                    {
                        foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(item)))
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
                        SilDev.Run.Cmd($"TASKKILL /F /IM \"{string.Join("\" && TASKKILL /F /IM \"", TaskList)}\"", true, 0);
                    if (DownloadServers.Count > 0)
                    {
                        string ChangeLog = null;
                        foreach (string mirror in DownloadServers)
                        {
                            ChangeLog = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/ChangeLog.txt");
                            if (!string.IsNullOrWhiteSpace(ChangeLog))
                                break;
                        }
                        if (!string.IsNullOrWhiteSpace(ChangeLog))
                        {
                            changeLog.TextAlign = HorizontalAlignment.Left;
                            changeLog.Text = ChangeLog;
                            changeLog.Select(changeLog.Text.Length, changeLog.Text.Length);
                        }
                    }
                    ShowInTaskbar = true;
                    return;
                }
            }

            Close();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (ShowInTaskbar)
            {
                Opacity = 1d;
                Refresh();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) =>
            Environment.Exit(Environment.ExitCode);

        private void updateBtn_Click(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            string DownloadPath = null;
            if (!string.IsNullOrWhiteSpace(SnapshotLastStamp))
            {
                try
                {
                    DownloadPath = $"https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/SNAPSHOTS/{SnapshotLastStamp}.7z";
                    if (!SilDev.Network.OnlineFileExists(DownloadPath))
                        throw new NotSupportedException();
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    DownloadPath = null;
                }
            }
            if (string.IsNullOrWhiteSpace(DownloadPath))
            {
                try
                {
                    bool exist = false;
                    foreach (string mirror in DownloadServers)
                    {
                        DownloadPath = $"{mirror}/Downloads/Portable%20Apps%20Suite/PortableAppsSuite.7z";
                        if (exist = SilDev.Network.OnlineFileExists(DownloadPath))
                            break;
                    }
                    if (!exist)
                        throw new FileNotFoundException();
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    DownloadPath = null;
                }
            }
            if (!string.IsNullOrWhiteSpace(DownloadPath))
            {
                try
                {
                    if (UpdatePath.Contains(HomeDir))
                        throw new NotSupportedException();
                    foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(UpdateDir), "PortableAppsSuite-{*}", SearchOption.TopDirectoryOnly))
                        Directory.Delete(dir, true);
                    if (!Directory.Exists(UpdateDir))
                        Directory.CreateDirectory(UpdateDir);
                    foreach (string file in new string[] { "7z.dll", "7zG.exe" })
                    {
                        string path = Path.Combine(Application.StartupPath, $"Helper\\7z{(Environment.Is64BitOperatingSystem ? "\\x64" : string.Empty)}\\{file}");
                        File.Copy(path, Path.Combine(UpdateDir, file));
                    }
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    Environment.ExitCode = 1;
                    Close();
                }
            }
            try
            {
                SilDev.Network.DownloadFileAsync(DownloadPath, UpdatePath);
                CheckDownload.Enabled = true;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.ExitCode = 1;
                Close();
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = $"{SilDev.Network.LatestAsyncDownloadInfo.TransferSpeed} - {SilDev.Network.LatestAsyncDownloadInfo.DataReceived}";
            statusBar.Value = SilDev.Network.LatestAsyncDownloadInfo.ProgressPercentage;
            if (!SilDev.Network.AsyncDownloadIsBusy())
                DlAsyncIsBusyCounter++;
            if (DlAsyncIsBusyCounter == 10)
            {
                statusBar.Maximum = 1000;
                statusBar.Value = 1000;
                statusBar.Value--;
                statusBar.Maximum = 100;
                statusBar.Value = 100;
            }
            if (DlAsyncIsBusyCounter >= 100)
            {
                ((System.Windows.Forms.Timer)sender).Enabled = false;
                string helperPath = Path.Combine(Path.GetDirectoryName(UpdatePath), "UpdateHelper.bat");
                try
                {
                    string helper = string.Format(Properties.Resources.BatchDummy_7zUpdateHelper, HomeDir);
                    File.WriteAllText(helperPath, helper);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
                try
                {
                    if (string.IsNullOrWhiteSpace(SnapshotLastStamp))
                        SnapshotLastStamp = "PortableAppsSuite";
                    if (SilDev.Crypt.MD5.EncryptFile(UpdatePath) != HashInfo["MD5"][SnapshotLastStamp])
                        throw new NotSupportedException();
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        FileName = helperPath,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                    Close();
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    SilDev.MsgBox.Show(this, Lang.GetText("InstallErrorMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cancelBtn_Click(cancelBtn, EventArgs.Empty);
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SilDev.Network.AsyncDownloadIsBusy())
                    SilDev.Network.CancelAsyncDownload();
                if (Directory.Exists(UpdateDir))
                    Directory.Delete(UpdateDir, true);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Close();
        }

        private void progressLabel_TextChanged(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel2.ColumnStyles[0].Width = progressLabel.Width + 8;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void virusTotalBtn_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (string value in HashInfo["SHA256"].Values)
                {
                    Process.Start($"https://www.virustotal.com/en/file/{value}/analysis");
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void si13n7Btn_Click(object sender, EventArgs e) =>
            Process.Start("http://www.si13n7.com");
    }
}
