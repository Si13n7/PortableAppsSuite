namespace Updater
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using Timer = System.Windows.Forms.Timer;

    public partial class MainForm : Form
    {
        private const string GitUserUrl = "https://raw.githubusercontent.com/Si13n7";
        private static readonly string GitSnapsUrl = $"{GitUserUrl}/PortableAppsSuite/master/.snapshots";
        private static readonly string HomeDir = PathEx.Combine(PathEx.LocalDir, "..");
        private static readonly string UpdateDir = PathEx.Combine(Path.GetTempPath(), $"PortableAppsSuite-{{{Guid.NewGuid()}}}");
        private static readonly List<string> DownloadMirrors = new List<string>();
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private readonly string _updatePath = Path.Combine(UpdateDir, "Update.7z");
        private int _downloadFinishedCount;
        private Dictionary<string, Dictionary<string, string>> _hashInfo = new Dictionary<string, Dictionary<string, string>>();
        private bool _ipv4, _ipv6;
        private string _releaseLastStamp, _snapshotLastStamp;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.PortableApps_green_64;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);

            // Check internet connection
            if (!(_ipv4 = NetEx.InternetIsAvailable()) && !(_ipv6 = NetEx.InternetIsAvailable(true)))
            {
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Get update infos from GitHub if enabled
            if (Ini.ReadInteger("Settings", "UpdateChannel") > 0)
            {
                if (!_ipv4 && _ipv6)
                {
                    Environment.ExitCode = 1;
                    Application.Exit();
                    return;
                }
                var last = NetEx.Transfer.DownloadString($"{GitSnapsUrl}/Last.ini");
                if (!string.IsNullOrWhiteSpace(last))
                {
                    _snapshotLastStamp = Ini.Read("Info", "LastStamp", last);
                    if (!string.IsNullOrWhiteSpace(_snapshotLastStamp))
                        _hashInfo = Ini.ReadAll(NetEx.Transfer.DownloadString($"{GitSnapsUrl}/{_snapshotLastStamp}.ini"));
                }
            }

            // Get update infos if not already set
            if (_hashInfo.Count == 0)
            {
                // Get available download mirrors
                var dnsInfo = new Dictionary<string, Dictionary<string, string>>();
                for (var i = 0; i < 3; i++)
                {
                    if (!_ipv4 && _ipv6)
                    {
                        dnsInfo = Ini.ReadAll(Resources.IPv6DNS, false);
                        break;
                    }
                    dnsInfo = Ini.ReadAll(NetEx.Transfer.DownloadString($"{GitUserUrl}/_ServerInfos/master/DnsInfo.ini"), false);
                    if (dnsInfo.Count == 0 && i < 2)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    break;
                }
                if (dnsInfo.Count > 0)
                    foreach (var section in dnsInfo.Keys)
                        try
                        {
                            var addr = dnsInfo[section][_ipv4 ? "addr" : "ipv6"];
                            if (string.IsNullOrEmpty(addr))
                                continue;
                            var domain = dnsInfo[section]["domain"];
                            if (string.IsNullOrEmpty(domain))
                                continue;
                            bool ssl;
                            bool.TryParse(dnsInfo[section]["ssl"], out ssl);
                            domain = ssl ? $"https://{domain}" : $"http://{domain}";
                            if (!DownloadMirrors.ContainsEx(domain))
                                DownloadMirrors.Add(domain);
                        }
                        catch (KeyNotFoundException) { }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                if (DownloadMirrors.Count == 0)
                {
                    Environment.ExitCode = 1;
                    Application.Exit();
                    return;
                }

                // Get file hashes
                foreach (var mirror in DownloadMirrors)
                {
                    var last = NetEx.Transfer.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/Last.ini");
                    if (!string.IsNullOrWhiteSpace(last))
                    {
                        _releaseLastStamp = Ini.Read("Info", "LastStamp", last);
                        if (!string.IsNullOrWhiteSpace(_releaseLastStamp))
                            _hashInfo = Ini.ReadAll(NetEx.Transfer.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/{_releaseLastStamp}.ini"));
                    }
                    if (_hashInfo.Count > 0)
                        break;
                }
            }
            if (_hashInfo.Count == 0)
            {
                Environment.ExitCode = 1;
                Application.Exit();
                return;
            }

            // Compare hashes
            var updateAvailable = false;
            if (_hashInfo["SHA256"].Count > 0)
                foreach (var key in _hashInfo["SHA256"].Keys)
                {
                    var file = Path.Combine(HomeDir, $"{key}.exe");
                    if (!File.Exists(file))
                        file = PathEx.Combine(PathEx.LocalDir, $"{key}.exe");
                    if (Crypto.EncryptFileToSha256(file) == _hashInfo["SHA256"][key])
                        continue;
                    updateAvailable = true;
                    break;
                }

            // Install updates
            if (updateAvailable)
                if (MessageBox.Show(Lang.GetText("UpdateAvailableMsg"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    // Update changelog
                    if (DownloadMirrors.Count > 0)
                    {
                        var chLog = string.Empty;
                        foreach (var mirror in DownloadMirrors)
                        {
                            chLog = NetEx.Transfer.DownloadString($"{mirror}/Downloads/Portable%20Apps%20Suite/ChangeLog.txt");
                            if (!string.IsNullOrWhiteSpace(chLog))
                                break;
                        }
                        if (!string.IsNullOrWhiteSpace(chLog))
                        {
                            changeLog.Font = new Font("Consolas", 8.25f);
                            changeLog.Text = chLog.FormatNewLine();
                            var colorMap = new Dictionary<Color, string[]>
                            {
                                {
                                    Color.PaleGreen, new[]
                                    {
                                        " PORTABLE APPS SUITE",
                                        " UPDATED:",
                                        " CHANGES:"
                                    }
                                },
                                {
                                    Color.SkyBlue, new[]
                                    {
                                        " Global:",
                                        " Apps Launcher:",
                                        " Apps Downloader:",
                                        " Apps Suite Updater:"
                                    }
                                },
                                {
                                    Color.Khaki, new[]
                                    {
                                        "Version History:"
                                    }
                                },
                                {
                                    Color.Plum, new[]
                                    {
                                        "{", "}",
                                        "(", ")",
                                        "|",
                                        ".",
                                        "-"
                                    }
                                },
                                {
                                    Color.Tomato, new[]
                                    {
                                        " * "
                                    }
                                },
                                {
                                    Color.Black, new[]
                                    {
                                        new string('_', 84)
                                    }
                                }
                            };
                            foreach (var line in changeLog.Text.Split('\n'))
                            {
                                DateTime d;
                                if (line.Length < 1 || !DateTime.TryParseExact(line.Trim(' ', ':'), "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                                    continue;
                                changeLog.MarkText(line, Color.Khaki);
                            }
                            foreach (var color in colorMap)
                                foreach (var s in color.Value)
                                    changeLog.MarkText(s, color.Key);
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

            // Exit the application if no updates were found
            Environment.ExitCode = 2;
            Application.Exit();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!ShowInTaskbar)
                return;
            Opacity = 1d;
            Refresh();
        }

        private void ChangeLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
                WindowState = FormWindowState.Minimized;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Button;
            if (owner == null)
                return;
            owner.Enabled = false;
            string downloadPath = null;
            if (!string.IsNullOrWhiteSpace(_snapshotLastStamp))
                try
                {
                    downloadPath = $"{GitSnapsUrl}/{_snapshotLastStamp}.7z";
                    if (!NetEx.FileIsAvailable(downloadPath, 60000))
                        throw new PathNotFoundException(downloadPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    downloadPath = null;
                }
            if (string.IsNullOrWhiteSpace(downloadPath))
                try
                {
                    var exist = false;
                    foreach (var mirror in DownloadMirrors)
                    {
                        downloadPath = $"{mirror}/Downloads/Portable%20Apps%20Suite/{_releaseLastStamp}.7z";
                        exist = NetEx.FileIsAvailable(downloadPath, 60000);
                        if (exist)
                            break;
                    }
                    if (!exist)
                        throw new PathNotFoundException(downloadPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    downloadPath = null;
                }
            if (!string.IsNullOrWhiteSpace(downloadPath))
                try
                {
                    if (_updatePath.ContainsEx(HomeDir))
                        throw new NotSupportedException();
                    var updDir = Path.GetDirectoryName(UpdateDir);
                    if (!string.IsNullOrEmpty(updDir))
                        foreach (var dir in Directory.GetDirectories(updDir, "PortableAppsSuite-{*}", SearchOption.TopDirectoryOnly))
                            Directory.Delete(dir, true);
                    if (!Directory.Exists(UpdateDir))
                        Directory.CreateDirectory(UpdateDir);
                    foreach (var file in new[] { "7z.dll", "7zG.exe" })
                    {
                        var path = PathEx.Combine(PathEx.LocalDir, $"Helper\\7z{(Environment.Is64BitOperatingSystem ? "\\x64" : string.Empty)}\\{file}");
                        File.Copy(path, Path.Combine(UpdateDir, file));
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex, true);
                    return;
                }
            try
            {
                _transfer.DownloadFile(downloadPath, _updatePath);
                checkDownload.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, true);
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            var owner = sender as Timer;
            if (owner == null)
                return;
            statusLabel.Text = _transfer.TransferSpeedAd + @" - " + _transfer.DataReceived;
            statusBar.Value = _transfer.ProgressPercentage;
            if (!_transfer.IsBusy)
                _downloadFinishedCount++;
            if (_downloadFinishedCount == 10)
                statusBar.JumpToEnd();
            if (_downloadFinishedCount < 100)
                return;
            owner.Enabled = false;
            string helperPath = null;
            try
            {
                helperPath = Path.GetDirectoryName(_updatePath);
                if (string.IsNullOrEmpty(helperPath))
                    return;
                helperPath = Path.Combine(helperPath, "UpdateHelper.bat");
                var helper = string.Format(Resources.BatchDummy, HomeDir);
                File.WriteAllText(helperPath, helper);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            try
            {
                if (string.IsNullOrEmpty(helperPath))
                    throw new ArgumentNullException(nameof(helperPath));
                var lastStamp = _releaseLastStamp;
                if (string.IsNullOrWhiteSpace(lastStamp))
                    lastStamp = _snapshotLastStamp;
                if (Crypto.EncryptFileToMd5(_updatePath) != _hashInfo["MD5"][lastStamp])
                    throw new InvalidOperationException();
                CloseAppsSuite();
                ProcessEx.Start(helperPath, true, ProcessWindowStyle.Hidden);
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBoxEx.Show(this, Lang.GetText("InstallErrorMsg"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.ExitCode = 1;
                CancelBtn_Click(cancelBtn, EventArgs.Empty);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (_transfer.IsBusy)
                    _transfer.CancelAsync();
                if (Directory.Exists(UpdateDir))
                    Directory.Delete(UpdateDir, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            Application.Exit();
        }

        private void ProgressLabel_TextChanged(object sender, EventArgs e)
        {
            try
            {
                statusTableLayoutPanel.ColumnStyles[0].Width = progressLabel.Width + 8;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void VirusTotalBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Button;
            if (owner == null)
                return;
            owner.Enabled = false;
            try
            {
                foreach (var value in _hashInfo["SHA256"].Values)
                {
                    Process.Start($"https://www.virustotal.com/en/file/{value}/analysis");
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            owner.Enabled = true;
        }

        private void WebBtn_Click(object sender, EventArgs e) =>
            Process.Start("http://www.si13n7.com/");

        private static void CloseAppsSuite()
        {
            var appsSuiteItemList = new List<string>
            {
                PathEx.Combine(PathEx.LocalDir, "AppsDownloader.exe"),
                PathEx.Combine(PathEx.LocalDir, "AppsDownloader64.exe"),
                Path.Combine(HomeDir, "AppsLauncher.exe"),
                Path.Combine(HomeDir, "AppsLauncher64.exe")
            };
            appsSuiteItemList.AddRange(Directory.GetFiles(PathEx.LocalDir, "*.exe", SearchOption.AllDirectories).Where(s => !PathEx.LocalPath.EqualsEx(s)));
            var taskList = new List<string>();
            foreach (var item in appsSuiteItemList)
                foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(item)))
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
                        Log.Write(ex);
                    }
                    string fileName = $"{p.ProcessName}.exe";
                    if (!taskList.ContainsEx(fileName))
                        taskList.Add(fileName);
                }
            if (taskList.Count == 0)
                return;
            using (var p = ProcessEx.Send($"TASKKILL /F /IM \"{string.Join("\" && TASKKILL /F /IM \"", taskList)}\"", true, false))
                if (p != null && !p.HasExited)
                    p.WaitForExit();
        }
    }
}
