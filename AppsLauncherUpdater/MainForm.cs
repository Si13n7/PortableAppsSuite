using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Updater
{
    public partial class MainForm : Form
    {
        static readonly string homePath = Path.GetFullPath($"{Application.StartupPath}\\..");
        List<string> DownloadServers = new List<string>();
        string SHA256Sums = null;
        string SfxPath = Path.Combine(homePath, "Portable.sfx.exe");
        int DlAsyncIsBusyCounter = 0;

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_green_64;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
                Environment.Exit(Environment.ExitCode);
            DownloadServers = SilDev.Network.GetAvailableServers("raw.githubusercontent.com/Si13n7/_ServerInfos/master/Server-DNS.ini", InternetIsAvailable);
            if (DownloadServers.Count == 0)
                Environment.Exit(Environment.ExitCode);
            foreach (string mirror in DownloadServers)
            {
                SHA256Sums = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/SHA256Sums.txt");
                if (!string.IsNullOrWhiteSpace(SHA256Sums))
                    break;
            }
            if (string.IsNullOrWhiteSpace(SHA256Sums))
                Environment.Exit(Environment.ExitCode);
            bool UpdateAvailable = false;
            foreach (string line in SHA256Sums.Split(','))
            {
                var tmp = line.Split(' ');
                if (tmp.Length != 2)
                    continue;
                string file = Path.Combine(homePath, tmp[1]);
                if (!File.Exists(file))
                    file = Path.Combine(Application.StartupPath, tmp[1]);
                if (File.Exists(file) && !tmp[0].Contains(SilDev.Crypt.SHA.EncryptFile(file, SilDev.Crypt.SHA.CryptKind.SHA256)) || !File.Exists(file))
                {
                    UpdateAvailable = true;
                    Environment.ExitCode = 1;
                    break;
                }
            }
            if (UpdateAvailable)
            {
                if (MessageBox.Show(Lang.GetText("UpdateAvailableMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    List<string>  AppsSuiteItemList = new List<string>()
                    {
                        Path.Combine(homePath, "AppsLauncher.exe"),
                        Path.Combine(homePath, "AppsLauncher64.exe")
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
                    string ChangeLog = null;
                    foreach (string mirror in DownloadServers)
                    {
                        ChangeLog = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/ChangeLog.txt");
                        if (!string.IsNullOrWhiteSpace(ChangeLog))
                            break;
                    }
                    if (!string.IsNullOrWhiteSpace(ChangeLog))
                    {
                        changeLog.Text = ChangeLog;
                        changeLog.Select(changeLog.Text.Length, changeLog.Text.Length);
                    }
                    return;
                }
            }
            Environment.Exit(Environment.ExitCode);
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            try
            {
                if (File.Exists(SfxPath))
                    File.Delete(SfxPath);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            string DownloadPath = null;
            foreach (string mirror in DownloadServers)
            {
                DownloadPath = $"{mirror}/Downloads/Portable%20Apps%20Suite/Portable.sfx.exe";
                if (SilDev.Network.OnlineFileExists(DownloadPath))
                    break;
            }
            SilDev.Network.DownloadFileAsync(DownloadPath, SfxPath);
            CheckDownload.Enabled = true;
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
                CheckDownload.Enabled = false;
                string helper = string.Format(SilDev.Crypt.Base64.Decrypt("QGVjaG8gb2ZmDQp0aXRsZSBmYjgzZDY0ZDUzMzAwYjcwZTI0YmYxYTc3NzA1MGI5Zg0KY2QgL2QgJX5kcDANClBvcnRhYmxlLnNmeC5leGUgLWQiezB9IiAtczINCnBpbmcgLW4gMiAxMjcuMC4wLjEgPm51bA0KZGVsIC9mIC9zIC9xIFBvcnRhYmxlLnNmeC5leGUNCmlmIGV4aXN0ICIlfm4wLmJhdCIgc3RhcnQgImNtZCIgJVdpbkRpciVcU3lzdGVtMzJcY21kLmV4ZSAvYyBkZWwgL2YgL3EgIiV+ZHAwJX5uMC5iYXQiICYmIHRhc2traWxsIC9GSSAiZmI4M2Q2NGQ1MzMwMGI3MGUyNGJmMWE3NzcwNTBiOWYiIC9JTSBjbWQuZXhlIC9UDQpleGl0IC9i"), homePath);
                string helperPath = Path.Combine(homePath, "UpdateHelper.bat");
                try
                {
                    File.WriteAllText(helperPath, helper);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
                SilDev.Run.App(new ProcessStartInfo()
                {
                    FileName = helperPath,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                Environment.ExitCode = 2;
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SilDev.Network.AsyncDownloadIsBusy())
                    SilDev.Network.CancelAsyncDownload();
                if (File.Exists(SfxPath))
                    File.Delete(SfxPath);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Environment.Exit(Environment.ExitCode);
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
            if (!string.IsNullOrWhiteSpace(SHA256Sums))
            {
                foreach (string line in SHA256Sums.Split(','))
                {
                    var tmp = line.Split(' ');
                    if (tmp.Length != 2)
                        continue;
                    Process.Start($"https://www.virustotal.com/en/file/{tmp[0]}/analysis");
                }
            }
        }

        private void si13n7Btn_Click(object sender, EventArgs e) =>
            Process.Start("http://www.si13n7.com");
    }
}
