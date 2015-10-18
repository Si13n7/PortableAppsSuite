using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Updater
{
    public partial class MainForm : Form
    {
        static string homePath = Path.GetFullPath(string.Format("{0}\\..", Application.StartupPath));
        static string DownloadServer = null;
        static string SHA256Sums = null;
        static string SfxPath = Path.Combine(homePath, "Portable.sfx.exe");
        static int DlAsyncIsBusyCounter = 0;

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_green;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            bool InternetIsAvailable = SilDev.Network.InternetIsAvailable();
            if (!InternetIsAvailable)
                Environment.Exit(Environment.ExitCode);
            DownloadServer = SilDev.Network.GetTheBestServer("raw.githubusercontent.com/Si13n7/_ServerInfos/master/Server-DNS.ini", InternetIsAvailable);
            if (string.IsNullOrWhiteSpace(DownloadServer))
                Environment.Exit(Environment.ExitCode);
            SHA256Sums = SilDev.Network.DownloadString(string.Format("{0}/Portable%20World/SHA256Sums.txt", DownloadServer));
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
                    break;
                }
            }
            if (UpdateAvailable)
            {
                if (MessageBox.Show(Lang.GetText("UpdateAvailableMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    foreach (string name in new string[] { "AppsDownloader", "PremiumAppsDownloader", "AppsLauncher", "AppsLauncher64" })
                    {
                        foreach (Process p in Process.GetProcessesByName(name))
                        {
                            if (p.MainWindowTitle.StartsWith("Portable Apps"))
                            {
                                p.CloseMainWindow();
                                p.WaitForExit(200);
                                if (!p.HasExited)
                                {
                                    p.Kill();
                                    if (!p.HasExited)
                                        SilDev.Run.App(new ProcessStartInfo() { FileName = "%WinDir%\\System32\\cmd.exe", Arguments = string.Format("/C TASKKILL /F /IM \"{0}.exe\"", p.ProcessName), Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden });
                                }
                            }
                        }
                    }
                    string logFile = SilDev.Network.DownloadString(string.Format("{0}/Portable%20World/ChangeLog.txt", DownloadServer));
                    if (!string.IsNullOrWhiteSpace(logFile))
                    {
                        changeLog.Text = logFile;
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
            SilDev.Network.DownloadFileAsync(string.Format("{0}/Portable%20World/Portable.sfx.exe", DownloadServer), SfxPath);
            CheckDownload.Enabled = true;
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = string.Format("{0} - {1}", SilDev.Network.DownloadInfo.GetTransferSpeed, SilDev.Network.DownloadInfo.GetDataReceived);
            statusBar.Value = SilDev.Network.DownloadInfo.GetProgressPercentage;
            if (!SilDev.Network.AsyncIsBusy())
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
                string helper = string.Format(SilDev.Crypt.Base64.Decrypt("QEVDSE8gT0ZGDQpUSVRMRSBVcGRhdGVIZWxwZXINCkBFQ0hPIE9GRg0KQ0QgL0QgJX5kcDANClBvcnRhYmxlLnNmeC5leGUgLWQiezB9IiAtczINClBJTkcgLW4gMSAxMjcuMC4wLjEgPm51bA0KREVMIC9GIC9TIC9RIFBvcnRhYmxlLnNmeC5leGUNCkRFTCAvRiAvUyAvUSBVcGRhdGVIZWxwZXIuYmF0DQpFWElU"), homePath);
                string helperPath = Path.Combine(homePath, "UpdateHelper.bat");
                File.WriteAllText(helperPath, helper);
                SilDev.Run.App(new ProcessStartInfo() { FileName = helperPath, Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden });
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SilDev.Network.AsyncIsBusy())
                    SilDev.Network.CancelAsyncDownload();
                if (File.Exists(SfxPath))
                    File.Delete(SfxPath);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Environment.ExitCode = 1;
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
                    Process.Start(string.Format("https://www.virustotal.com/en/file/{0}/analysis", tmp[0]));
                }
            }
        }

        private void si13n7Btn_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.si13n7.com");
        }
    }
}
