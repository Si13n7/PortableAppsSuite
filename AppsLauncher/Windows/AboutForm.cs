namespace AppsLauncher.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using LangResources;
    using Libraries;
    using SilDev;
    using SilDev.Forms;

    public partial class AboutForm : Form
    {
        private static int? _exitCode = 0;
        private static readonly object BwLocker = new object();
        private ProgressCircle _progressCircle;

        public AboutForm() =>
            InitializeComponent();

        private void AboutForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            Icon = Settings.CacheData.GetSystemIcon(ResourcesEx.IconIndex.HelpShield);

            Language.SetControlLang(this);
            Text = Language.GetText(Name);

            AddFileInfoLabels();

            var series = spaceChart.Series.FirstOrDefault();
            if (series != null)
            {
                series.LabelForeColor = Color.Transparent;
                series.LabelBackColor = Color.Transparent;
            }
            diskChartUpdater.RunWorkerAsync();

            logoPanel.BackColor = Settings.Window.Colors.BaseDark;

            updateBtnPanel.Width = TextRenderer.MeasureText(updateBtn.Text, updateBtn.Font).Width + 32;
            updateBtn.Image = Settings.CacheData.GetSystemImage(ResourcesEx.IconIndex.Network);
            updateBtn.ForeColor = Settings.Window.Colors.ButtonText;
            updateBtn.BackColor = Settings.Window.Colors.Button;
            updateBtn.FlatAppearance.MouseDownBackColor = Settings.Window.Colors.Button;
            updateBtn.FlatAppearance.MouseOverBackColor = Settings.Window.Colors.ButtonHover;

            _progressCircle = new ProgressCircle
            {
                Anchor = updateBtnPanel.Anchor,
                BackColor = Color.Transparent,
                ForeColor = mainPanel.BackColor,
                InnerRadius = 7,
                Location = new Point(updateBtnPanel.Right + 3, updateBtnPanel.Top + 1),
                OuterRadius = 9,
                RotationSpeed = 80,
                Size = new Size(updateBtnPanel.Height, updateBtnPanel.Height),
                Thickness = 3,
                Visible = false
            };
            mainPanel.Controls.Add(_progressCircle);

            aboutInfoLabel.ActiveLinkColor = Settings.Window.Colors.Base;
            aboutInfoLabel.BorderStyle = BorderStyle.None;
            aboutInfoLabel.Text = string.Format(Language.GetText(aboutInfoLabel), "Si13n7 Developments", Language.GetText(aboutInfoLabel.Name + "LinkLabel1"), Language.GetText(aboutInfoLabel.Name + "LinkLabel2"));
            aboutInfoLabel.Links.Clear();
            aboutInfoLabel.LinkText("Si13n7 Developments", "http://www.si13n7.com");
            aboutInfoLabel.LinkText(Language.GetText(aboutInfoLabel.Name + "LinkLabel1"), "http://paypal.si13n7.com");
            aboutInfoLabel.LinkText(Language.GetText(aboutInfoLabel.Name + "LinkLabel2"), "https://support.si13n7.com");

            copyrightLabel.Text = string.Format(copyrightLabel.Text, DateTime.Now.Year);

            WinApi.NativeHelper.MoveWindowToVisibleScreenArea(Handle);
        }

        private void AboutForm_Shown(object sender, EventArgs e)
        {
            var timer = new Timer(components)
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    Opacity += .1d;
                    return;
                }
                timer.Dispose();
                if (TopMost)
                    TopMost = false;
            };
        }

        private void AddFileInfoLabels()
        {
            var verInfoList = new List<FileVersionInfo>();
            string[][] strArray =
            {
                new[]
                {
                    "AppsLauncher.exe",
                    "AppsLauncher64.exe",
                    "Binaries\\AppsDownloader.exe",
                    "Binaries\\AppsDownloader64.exe",
                    "Binaries\\Updater.exe"
                },
                new[]
                {
                    "Binaries\\SilDev.CSharpLib.dll",
                    "Binaries\\SilDev.CSharpLib64.dll"
                },
                new[]
                {
                    "Binaries\\Helper\\7z\\7zG.exe",
                    "Binaries\\Helper\\7z\\x64\\7zG.exe"
                }
            };
            var verArray = new Version[strArray.Length];
            for (var i = 0; i < strArray.Length; i++)
                foreach (var f in strArray[i])
                    try
                    {
                        var s = PathEx.Combine(PathEx.LocalDir, f);
                        var fvi = FileVersionInfo.GetVersionInfo(s);
                        verArray[i] = FileEx.GetVersion(fvi.FileName);
                        verInfoList.Add(fvi);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

            var bottom = 0;
            foreach (var fvi in verInfoList)
                try
                {
                    var des = fvi.FileDescription;
                    if (!des.Contains("(64"))
                        if (PortableExecutable.Is64Bit(fvi.FileName))
                            des += " (64-bit)";
                    var nam = new Label
                    {
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 12.25f, FontStyle.Bold, GraphicsUnit.Point),
                        ForeColor = Color.PowderBlue,
                        Location = new Point(aboutInfoLabel.Left, bottom == 0 ? 15 : bottom + 10),
                        Text = des
                    };
                    mainPanel.Controls.Add(nam);
                    Version reqVer;
                    var fna = Path.GetFileName(fvi.FileName);
                    if (fna.EqualsEx("SilDev.CSharpLib.dll", "SilDev.CSharpLib64.dll"))
                        reqVer = verArray[1];
                    else if (fna.EqualsEx("7zG.exe"))
                        reqVer = verArray[2];
                    else
                        reqVer = verArray[0];
                    var curVer = FileEx.GetVersion(fvi.FileName);
                    var strVer = curVer.ToString();
                    if (!fna.EqualsEx("7zG.exe"))
                    {
                        reqVer = Version.Parse(reqVer.ToString(3));
                        curVer = Version.Parse(curVer.ToString(3));
                    }
                    var ver = new Label
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = new Font(nam.Font.FontFamily, 8.25f, FontStyle.Regular, nam.Font.Unit),
                        ForeColor = reqVer == curVer ? Color.PaleGreen : Color.OrangeRed,
                        Location = new Point(nam.Left + 3, nam.Bottom),
                        Text = strVer
                    };
                    mainPanel.Controls.Add(ver);
                    var sep = new Label
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = ver.Font,
                        ForeColor = copyrightLabel.ForeColor,
                        Location = new Point(ver.Right, nam.Bottom),
                        Text = @"|"
                    };
                    mainPanel.Controls.Add(sep);
                    var pat = new Label
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = ver.Font,
                        ForeColor = ver.ForeColor,
                        Location = new Point(sep.Right, nam.Bottom),
                        Text = fvi.FileName.RemoveText(PathEx.LocalDir).TrimStart(Path.DirectorySeparatorChar)
                    };
                    mainPanel.Controls.Add(pat);
                    bottom = pat.Bottom;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            Height += bottom;
            if (StartPosition == FormStartPosition.CenterScreen)
                Top -= (int)Math.Floor(bottom / 2d);
        }

        private void DiskChartUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!(sender is BackgroundWorker owner))
                return;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!PathEx.LocalDir.StartsWithEx(drive.Name))
                    continue;
                var free = drive.TotalFreeSpace;
                owner.ReportProgress(0, free);
                var apps = DirectoryEx.GetSize(PathEx.LocalDir);
                owner.ReportProgress(50, apps);
                var other = drive.TotalSize - free - apps;
                owner.ReportProgress(100, other);
                break;
            }
        }

        private void DiskChartUpdater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!(e.UserState is long length))
            {
                spaceChart.Visible = false;
                return;
            }
            try
            {
                var series = spaceChart.Series.First();
                switch (e.ProgressPercentage)
                {
                    case 0:
                        series.Points.AddXY($"{length.FormatSize(Reorganize.SizeOptions.Round)} Free", length);
                        break;
                    case 50:
                        series.Points.AddXY($"{length.FormatSize(Reorganize.SizeOptions.Round)} Used (Apps)", length);
                        break;
                    case 100:
                        series.Points.AddXY($"{length.FormatSize(Reorganize.SizeOptions.Round)} Used (Other)", length);
                        break;
                }
            }
            catch (NullReferenceException ex)
            {
                if (Log.DebugMode > 1)
                    Log.Write(ex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e) =>
            e.Cancel = updateChecker.IsBusy;

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (!(sender is Button owner))
                return;
            owner.Enabled = false;
            if (!updateChecker.IsBusy)
            {
                _progressCircle.Active = true;
                _progressCircle.Visible = true;
                updateChecker.RunWorkerAsync();
            }
            closeToUpdate.Enabled = true;
        }

        private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var process = ProcessEx.Start(Settings.CorePaths.AppsSuiteUpdater, false, false))
            {
                if (process?.HasExited == false)
                    process.WaitForExit();
                lock (BwLocker)
                {
                    _exitCode = process?.ExitCode;
                }
            }
            using (var process = ProcessEx.Start(Settings.CorePaths.AppsDownloader, Settings.ActionGuid.UpdateInstance, false, false))
            {
                if (process?.HasExited == false)
                    process.WaitForExit();
                process?.WaitForExit();
                lock (BwLocker)
                {
                    _exitCode = process?.ExitCode;
                }
            }
        }

        private void UpdateChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) =>
            updateBtn.Enabled = true;

        private void CloseToUpdate_Tick(object sender, EventArgs e)
        {
            if (updateChecker.IsBusy)
                return;
            _progressCircle.Active = false;
            _progressCircle.Visible = false;
            closeToUpdate.Enabled = false;
            string message;
            switch (_exitCode)
            {
                case 0:
                    message = Language.GetText(nameof(en_US.OperationCompletedMsg));
                    break;
                case 1:
                    message = Language.GetText(nameof(en_US.OperationCanceledMsg));
                    break;
                default:
                    message = Language.GetText(nameof(en_US.NoUpdatesFoundMsg));
                    break;
            }
            MessageBoxEx.Show(this, message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AboutInfoLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e?.Link?.LinkData is Uri)
                Process.Start(e.Link.LinkData.ToString());
        }
    }
}
