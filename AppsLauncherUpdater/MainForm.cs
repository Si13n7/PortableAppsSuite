using SilDev;
using SilDev.Forms;
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
        static readonly string HomeDir = PATH.Combine("%CurDir%\\..");

        static readonly string UpdateDir = PATH.Combine($"%TEMP%\\PortableAppsSuite-{{{Guid.NewGuid()}}}");
        readonly string UpdatePath = Path.Combine(UpdateDir, "Update.7z");

        Dictionary<string, Dictionary<string, string>> HashInfo = new Dictionary<string, Dictionary<string, string>>();

        NET.AsyncTransfer Transfer = new NET.AsyncTransfer();
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
            if (!NET.InternetIsAvailable())
            {
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Get update infos from GitHub if enabled
            if (INI.ReadInteger("Settings", "UpdateChannel", 0) > 0)
            {
                string Last = NET.DownloadString("https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/.snapshots/Last.ini");
                if (!string.IsNullOrWhiteSpace(Last))
                {
                    SnapshotLastStamp = INI.Read("Info", "LastStamp", Last);
                    if (!string.IsNullOrWhiteSpace(SnapshotLastStamp))
                        HashInfo = INI.ReadAll(NET.DownloadString($"https://raw.githubusercontent.com/Si13n7/PortableAppsSuite/master/.snapshots/{SnapshotLastStamp}.ini"));
                }
            }

            // Get update infos if not already set
            if (HashInfo.Count == 0)
            {
                // Get available download mirrors
                Dictionary<string, Dictionary<string, string>> DnsInfo = new Dictionary<string, Dictionary<string, string>>();
                for (int i = 0; i < 15; i++)
                {
                    DnsInfo = INI.ReadAll(NET.DownloadString("https://raw.githubusercontent.com/Si13n7/_ServerInfos/master/DnsInfo.ini"), false);
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
                            LOG.Debug(ex);
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
                    string Last = NET.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/Last.ini");
                    if (!string.IsNullOrWhiteSpace(Last))
                    {
                        ReleaseLastStamp = INI.Read("Info", "LastStamp", Last);
                        if (!string.IsNullOrWhiteSpace(ReleaseLastStamp))
                            HashInfo = INI.ReadAll(NET.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/{ReleaseLastStamp}.ini"));
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
                        file = PATH.Combine($"%CurDir%\\{key}.exe");
                    if (file.EncryptFileToSHA256() != HashInfo["SHA256"][key])
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
                    AppsSuiteItemList.AddRange(Directory.GetFiles(PATH.Combine("%CurDir%"), "*.exe", SearchOption.AllDirectories).Where(s => s.ToLower() != Application.ExecutablePath.ToLower()));
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
                                LOG.Debug(ex);
                            }
                            string fileName = $"{p.ProcessName}.exe";
                            if (!TaskList.Contains(fileName))
                                TaskList.Add(fileName);
                        }
                    }
                    if (TaskList.Count > 0)
                        RUN.Cmd($"TASKKILL /F /IM \"{string.Join("\" && TASKKILL /F /IM \"", TaskList)}\"", true, 0);

                    // Get and show changelog
                    if (DownloadMirrors.Count > 0)
                    {
                        string ChangeLog = null;
                        foreach (string mirror in DownloadMirrors)
                        {
                            ChangeLog = NET.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/ChangeLog.txt");
                            if (!string.IsNullOrWhiteSpace(ChangeLog))
                                break;
                        }
                        if (!string.IsNullOrWhiteSpace(ChangeLog))
                        {
                            changeLog.Font = new Font("Consolas", 8.25f);
                            changeLog.Text = ChangeLog.FormatNewLine();

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
                                    RICHTEXTBOX.MarkText(changeLog, s, color.Key);
                            }

                            RICHTEXTBOX.MarkText(changeLog, "Version History:", Color.Khaki);
                            RICHTEXTBOX.MarkText(changeLog, " * ", Color.Tomato);
                            RICHTEXTBOX.MarkText(changeLog, new string('_', 84), Color.Black);

                            foreach (string line in changeLog.Text.Split('\n'))
                            {
                                if (line.Length < 1 || !char.IsDigit(line[1]))
                                    continue;
                                RICHTEXTBOX.MarkText(changeLog, line, Color.Khaki);
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
                LOG.Debug(ex);
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
                    if (!NET.FileIsAvailable(DownloadPath))
                        throw new NotSupportedException();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
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
                        if (exist = NET.FileIsAvailable(DownloadPath))
                            break;
                    }
                    if (!exist)
                        throw new FileNotFoundException();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
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
                        string path = PATH.Combine($"%CurDir%\\Helper\\7z{(Environment.Is64BitOperatingSystem ? "\\x64" : string.Empty)}\\{file}");
                        File.Copy(path, Path.Combine(UpdateDir, file));
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex, true);
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
                LOG.Debug(ex, true);
            }
        }

        private void checkDownload_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = $"{(int)Math.Round(Transfer.TransferSpeed)} kb/s - {Transfer.DataReceived}";
            statusBar.Value = Transfer.ProgressPercentage;
            if (!Transfer.IsBusy)
                DownloadFinishedCount++;
            if (DownloadFinishedCount == 10)
                PROGRESSBAR.JumpToEnd(statusBar);
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
                    LOG.Debug(ex);
                }
                try
                {
                    string LastStamp = ReleaseLastStamp;
                    if (string.IsNullOrWhiteSpace(LastStamp))
                        LastStamp = SnapshotLastStamp;
                    if (UpdatePath.EncryptFileToMD5() != HashInfo["MD5"][LastStamp])
                        throw new NotSupportedException();
                    RUN.App(new ProcessStartInfo()
                    {
                        FileName = helperPath,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    MSGBOX.Show(this, Lang.GetText("InstallErrorMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                LOG.Debug(ex);
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
                LOG.Debug(ex);
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
                LOG.Debug(ex);
            }
            b.Enabled = true;
        }

        private void si13n7Btn_Click(object sender, EventArgs e) =>
            Process.Start("http://www.si13n7.com/");
    }
}
