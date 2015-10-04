using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.help_shield;
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            string title = Lang.GetText("AboutFormTitle");
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;
            Lang.SetControlLang(this);
            appsLauncherVersion.Text = Main.CurrentVersion;
            appsDownloaderVersion.Text = GetFileVersion(Path.Combine(Application.StartupPath, "Binaries\\AppsDownloader.exe"));
            appsLauncherUpdaterVersion.Text = GetFileVersion(Path.Combine(Application.StartupPath, "Binaries\\Updater.exe"));
            aboutInfoLabel_Load();
        }

        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (updateChecker.IsBusy)
                e.Cancel = true;
        }

        public static string GetFileVersion(string _path)
        {
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(_path);
                return fvi.ProductVersion;
            }
            catch
            {
                return Lang.GetText("NotFound");
            }
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            if (!updateChecker.IsBusy)
                updateChecker.RunWorkerAsync();
            closeToUpdate.Enabled = true;
        }

        private void updateChecker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SilDev.Run.App(Application.StartupPath, "Binaries\\Updater.exe", 0);
        }

        private void updateChecker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            updateBtn.Enabled = true;
        }

        private void closeToUpdate_Tick(object sender, EventArgs e)
        {
            if (updateChecker.IsBusy)
            {
                if (File.Exists(Path.Combine(Application.StartupPath, "Portable.sfx.exe")))
                    Environment.Exit(1);
            }
            else
            {
                closeToUpdate.Enabled = false;
                SilDev.MsgBox.Show(this, Lang.GetText("NoUpdatesFoundMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void aboutInfoLabel_Load()
        {
            try
            {
                aboutInfoLabel.BorderStyle = BorderStyle.None;
                string[] linkNames = new string[]
                {
                    "Si13n7 Developments",
                    Lang.GetText("aboutInfoLabelLinkLabel1"),
                    Lang.GetText("aboutInfoLabelLinkLabel2")
                };
                Uri[] linkUrls = new Uri[]
                {
                   new Uri("http://www.si13n7.com"),
                   new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K3ZJDAT3GPFYW"),
                   new Uri("http://support.si13n7.com")
                };
                aboutInfoLabel.Text = string.Format(Lang.GetText(aboutInfoLabel), linkNames[0], linkNames[1], linkNames[2]);
                aboutInfoLabel.Links.Clear();
                for (int i = 0; i < linkNames.Length; i++)
                {
                    try
                    {
                        string linkName = linkNames[i];
                        Uri linkUrl = linkUrls[i];
                        int linkStartIndex = GetLinkStartIndex(aboutInfoLabel.Text, linkName);
                        if (linkStartIndex > -1)
                            aboutInfoLabel.Links.Add(linkStartIndex, linkName.Length, linkUrl).Enabled = true;
                        else
                            throw new Exception("'linkStartIndex' is not definied.");
                    }
                    catch (Exception ex)
                    {
                        aboutInfoLabel.Text = string.Empty;
                        SilDev.Log.Debug(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                aboutInfoLabel.Text = string.Empty;
                SilDev.Log.Debug(ex);
            }
        }

        private int GetLinkStartIndex(string _linkLabelText, string _linkName)
        {
            int linkStartIndex = -1;
            try
            {
                for (int i = 0; i < _linkLabelText.Length; i++)
                {
                    if (i + _linkName.Length >= _linkLabelText.Length)
                        continue;
                    for (int j = 0; j < _linkName.Length; j++)
                    {
                        if (_linkLabelText[i + j] != _linkName[j])
                            break;
                        if (j == _linkName.Length - 1)
                            linkStartIndex = i;
                    }
                    if (linkStartIndex > -1)
                        break;
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            return linkStartIndex;
        }

        private void aboutInfoLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.LinkData is Uri)
                Process.Start(e.Link.LinkData.ToString());
        }
    }
}
