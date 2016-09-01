using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    public partial class MainForm : Form
    {
        static readonly string HomeDir = Path.GetFullPath($"{Application.StartupPath}\\..");

        static readonly string UpdateDir = SilDev.Run.EnvironmentVariableFilter($"%TEMP%\\PortableAppsSuite-{{{Guid.NewGuid()}}}");
        readonly string UpdatePath = Path.Combine(UpdateDir, "Update.7z");

        Dictionary<string, Dictionary<string, string>> HashInfo = new Dictionary<string, Dictionary<string, string>>();

        SilDev.Network.AsyncTransfer Transfer = new SilDev.Network.AsyncTransfer();
        static List<string> DownloadMirrors = new List<string>();
        int DownloadFinishedCount = 0;

        string ReleaseLastStamp = null;
        string SnapshotLastStamp = null;

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
                Application.Exit();
                return;
            }

            // Get update infos from GitHub if enabled
            if (SilDev.Ini.ReadInteger("Settings", "UpdateChannel", 0) > 0)
            {
                string Last = SilDev.Network.DownloadString("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/.snapshots/Last.ini");
                if (!string.IsNullOrWhiteSpace(Last))
                {
                    SnapshotLastStamp = SilDev.Ini.Read("Info", "LastStamp", Last);
                    if (!string.IsNullOrWhiteSpace(SnapshotLastStamp))
                        HashInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString($"https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/.snapshots/{SnapshotLastStamp}.ini"));
                }
            }

            // Get update infos if not already set
            if (HashInfo.Count == 0)
            {
                // Get available download mirrors
                Dictionary<string, Dictionary<string, string>> DnsInfo = new Dictionary<string, Dictionary<string, string>>();
                for (int i = 0; i < 15; i++)
                {
                    DnsInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
                    if (DnsInfo.Count == 0 && i < 2)
                        Thread.Sleep(200);
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
                            if (!DownloadMirrors.Contains(domain))
                                DownloadMirrors.Add(domain);
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                }
                if (DownloadMirrors.Count == 0)
                {
                    Environment.ExitCode = 1;
                    Application.Exit();
                    return;
                }
                // Get file hashes
                foreach (string mirror in DownloadMirrors)
                {
                    string Last = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/Last.ini");
                    if (!string.IsNullOrWhiteSpace(Last))
                    {
                        ReleaseLastStamp = SilDev.Ini.Read("Info", "LastStamp", Last);
                        if (!string.IsNullOrWhiteSpace(ReleaseLastStamp))
                            HashInfo = SilDev.Ini.ReadAll(SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/{ReleaseLastStamp}.ini"));
                    }
                    if (HashInfo.Count > 0)
                        break;
                }
            }

            if (HashInfo.Count == 0)
            {
                Environment.ExitCode = 1;
                Application.Exit();
                return;
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
                    if (SilDev.Crypt.SHA256.EncryptFile(file) != HashInfo["SHA256"][key])
                    {
                        UpdateAvailable = true;
                        break;
                    }
                }
            }

            if (UpdateAvailable)
            {
                if (MessageBox.Show(Lang.GetText("UpdateAvailableMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    // Close apps suite programs
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
                    // Get and show changelog
                    if (DownloadMirrors.Count > 0)
                    {
                        string ChangeLog = null;
                        foreach (string mirror in DownloadMirrors)
                        {
                            ChangeLog = SilDev.Network.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/ChangeLog.txt");
                            if (!string.IsNullOrWhiteSpace(ChangeLog))
                                break;
                        }
                        if (!string.IsNullOrWhiteSpace(ChangeLog))
                        {
                            changeLog.Font = new Font("Consolas", 8.25f);
                            changeLog.Text = SilDev.Convert.FormatNewLine(ChangeLog);

                            Dictionary<Color, string[]> colorMap = new Dictionary<Color, string[]>();
                            colorMap.Add(Color.PaleGreen, new string[]
                            {
                                " PORTABLE APPS SUITE",
                                " UPDATED:",
                                " CHANGES:"
                            });

                            colorMap.Add(Color.SkyBlue, new string[]
                            {
                                " Global:",
                                " Apps Launcher:",
                                " Apps Downloader:",
                                " Apps Suite Updater:"
                            });

                            colorMap.Add(Color.Plum, new string[] { "(", ")", "|" });

                            foreach (var color in colorMap)
                            {
                                foreach (string s in color.Value)
                                    SilDev.Forms.RichTextBox.MarkText(changeLog, s, color.Key);
                            }

                            SilDev.Forms.RichTextBox.MarkText(changeLog, "Version History:", Color.Khaki);
                            SilDev.Forms.RichTextBox.MarkText(changeLog, " * ", Color.Tomato);
                            SilDev.Forms.RichTextBox.MarkText(changeLog, new string('_', 84), Color.Black);

                            foreach (string line in changeLog.Text.Split('\n'))
                            {
                                if (line.Length < 1 || !char.IsDigit(line[1]))
                                    continue;
                                SilDev.Forms.RichTextBox.MarkText(changeLog, line, Color.Khaki);
                            }
                        }
                    }
                    else
                    {
                        changeLog.Dock = DockStyle.None;
                        changeLog.Size = new Size(changeLogPanel.Width, TextRenderer.MeasureText(changeLog.Text, changeLog.Font).Height);
                        changeLog.Location = new Point(0, changeLogPanel.Height / 2 - changeLog.Height - 16);
                        changeLog.SelectAll();
                        changeLog.SelectionAlignment = HorizontalAlignment.Center;
                        changeLog.DeselectAll();
                    }
                    ShowInTaskbar = true;
                    return;
                }
            }

            Application.Exit();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (ShowInTaskbar)
            {
                Opacity = 1d;
                Refresh();
            }
        }

        private void changeLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
                WindowState = FormWindowState.Minimized;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            string DownloadPath = null;
            if (!string.IsNullOrWhiteSpace(SnapshotLastStamp))
            {
                try
                {
                    DownloadPath = $"https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/.snapshots/{SnapshotLastStamp}.7z";
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
                    foreach (string mirror in DownloadMirrors)
                    {
                        DownloadPath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{ReleaseLastStamp}.7z";
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
                    Application.Exit();
                    return;
                }
            }
            try
            {
                Transfer.DownloadFile(DownloadPath, UpdatePath);
                checkDownload.Enabled = true;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                Environment.ExitCode = 1;
                Application.Exit();
            }
        }

        private void checkDownload_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = $"{(int)Math.Round(Transfer.TransferSpeed)} kb/s - {Transfer.DataReceived}";
            statusBar.Value = Transfer.ProgressPercentage;
            if (!Transfer.IsBusy)
                DownloadFinishedCount++;
            if (DownloadFinishedCount == 10)
                SilDev.Forms.ProgressBar.JumpToEnd(statusBar);
            if (DownloadFinishedCount >= 100)
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
                    string LastStamp = ReleaseLastStamp;
                    if (string.IsNullOrWhiteSpace(LastStamp))
                        LastStamp = SnapshotLastStamp;
                    if (SilDev.Crypt.MD5.EncryptFile(UpdatePath) != HashInfo["MD5"][LastStamp])
                        throw new NotSupportedException();
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        FileName = helperPath,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                    SilDev.MsgBox.Show(this, Lang.GetText("InstallErrorMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.ExitCode = 1;
                    cancelBtn_Click(cancelBtn, EventArgs.Empty);
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Transfer.IsBusy)
                    Transfer.CancelAsync();
                if (Directory.Exists(UpdateDir))
                    Directory.Delete(UpdateDir, true);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Application.Exit();
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
            Button b = (Button)sender;
            b.Enabled = false;
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
            b.Enabled = true;
        }

        private void si13n7Btn_Click(object sender, EventArgs e) =>
            Process.Start("http://www.si13n7.com/");
    }
}
