using SilDev;
using SilDev.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            Icon = RESOURCE.SystemIcon(RESOURCE.SystemIconKey.HELP_SHIELD, Main.SystemResourcePath);

            logoPanel.BackColor = Main.Colors.Base;

            updateBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.NETWORK, Main.SystemResourcePath);
            updateBtn.ForeColor = Main.Colors.ButtonText;
            updateBtn.BackColor = Main.Colors.Button;
            updateBtn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
            updateBtn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;

            aboutInfoLabel.ActiveLinkColor = Main.Colors.Base;
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            string title = Lang.GetText("AboutFormTitle");
            if (!string.IsNullOrWhiteSpace(title))
                Text = $"{title} Portable Apps Suite";
            Main.SetFont(this);

            AddFileInfoLabels();

            updateBtnPanel.Width = TextRenderer.MeasureText(updateBtn.Text, updateBtn.Font).Width + 32;

            aboutInfoLabel.BorderStyle = BorderStyle.None;
            aboutInfoLabel.Text = string.Format(Lang.GetText(aboutInfoLabel), "Si13n7 Developments", Lang.GetText("aboutInfoLabelLinkLabel1"), Lang.GetText("aboutInfoLabelLinkLabel2"));
            aboutInfoLabel.Links.Clear();
            aboutInfoLabel.LinkText("Si13n7 Developments", "http://www.si13n7.com");
            aboutInfoLabel.LinkText(Lang.GetText("aboutInfoLabelLinkLabel1"), "http://paypal.si13n7.com");
            aboutInfoLabel.LinkText(Lang.GetText("aboutInfoLabelLinkLabel2"), "https://support.si13n7.com");

            copyrightLabel.Text = string.Format(copyrightLabel.Text, DateTime.Now.Year);
        }

        private void AddFileInfoLabels()
        {
            List<FileVersionInfo> verInfoList = new List<FileVersionInfo>();
            string[][] strArray = new string[][]
            {
                new string[]
                {
                    "AppsLauncher.exe",
                    "AppsLauncher64.exe",
                    "Binaries\\AppsDownloader.exe",
                    "Binaries\\AppsDownloader64.exe",
                    "Binaries\\Updater.exe"
                },
                new string[]
                {
                    "Binaries\\SilDev.CSharpLib.dll",
                    "Binaries\\SilDev.CSharpLib64.dll"
                },
                new string[]
                {
                    "Binaries\\Helper\\7z\\7zG.exe",
                    "Binaries\\Helper\\7z\\x64\\7zG.exe"
                }
            };
            Version[] verArray = new Version[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                foreach (string f in strArray[i])
                {
                    try
                    {
                        if (verArray[i] == null)
                            verArray[i] = Version.Parse("0.0.0.0");
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(PATH.Combine("%CurDir%", f));
                        Version ver;
                        if (Version.TryParse(fvi.ProductVersion, out ver) && verArray[i] < ver)
                            verArray[i] = ver;
                        verInfoList.Add(fvi);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }
            int bottom = 0;
            foreach (FileVersionInfo fvi in verInfoList)
            {
                try
                {
                    Label nam = new Label()
                    {
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 12.25f, FontStyle.Bold, GraphicsUnit.Point),
                        ForeColor = Color.PowderBlue,
                        Location = new Point(aboutInfoLabel.Left, bottom == 0 ? 15 : bottom + 10),
                        Text = fvi.FileDescription,
                    };
                    mainPanel.Controls.Add(nam);

                    Version reqVer;
                    switch (Path.GetFileName(fvi.FileName).ToLower())
                    {
                        case "sildev.csharplib.dll":
                        case "sildev.csharplib64.dll":
                            reqVer = verArray[1];
                            break;
                        case "7zg.exe":
                            reqVer = verArray[2];
                            break;
                        default:
                            reqVer = verArray[0];
                            break;
                    }
                    Version curVer;
                    if (!Version.TryParse(fvi.ProductVersion, out curVer))
                        curVer = Version.Parse("0.0.0.0");
                    Label ver = new Label()
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = new Font(nam.Font.FontFamily, 8.25f, FontStyle.Regular, nam.Font.Unit),
                        ForeColor = reqVer == curVer ? Color.PaleGreen : Color.Firebrick,
                        Location = new Point(nam.Left + 3, nam.Bottom),
                        Text = fvi.ProductVersion,
                    };
                    mainPanel.Controls.Add(ver);

                    Label sep = new Label()
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = ver.Font,
                        ForeColor = copyrightLabel.ForeColor,
                        Location = new Point(ver.Right, nam.Bottom),
                        Text = "|",
                    };
                    mainPanel.Controls.Add(sep);

                    Label pat = new Label()
                    {
                        AutoSize = true,
                        BackColor = nam.BackColor,
                        Font = ver.Font,
                        ForeColor = ver.ForeColor,
                        Location = new Point(sep.Right, nam.Bottom),
                        Text = fvi.FileName.Replace(PATH.GetEnvironmentVariableValue("CurDir"), string.Empty).TrimStart('\\'),
                    };
                    mainPanel.Controls.Add(pat);

                    bottom = pat.Bottom;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    MSGBOX.Show(ex.Message);
                }
            }
            Height += bottom;
        }

        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e) =>
            e.Cancel = updateChecker.IsBusy;

        private void updateBtn_Click(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            if (!updateChecker.IsBusy)
                updateChecker.RunWorkerAsync();
            closeToUpdate.Enabled = true;
        }

        private void updateChecker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) =>
            RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Binaries\\Updater.exe" }, 0);

        private void updateChecker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) =>
            updateBtn.Enabled = true;

        private void closeToUpdate_Tick(object sender, EventArgs e)
        {
            if (!updateChecker.IsBusy)
            {
                closeToUpdate.Enabled = false;
                MSGBOX.Show(this, Lang.GetText("NoUpdatesFoundMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void aboutInfoLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.LinkData is Uri)
                Process.Start(e.Link.LinkData.ToString());
        }
    }
}
