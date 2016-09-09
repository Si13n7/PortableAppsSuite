using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            Icon = SilDev.Resource.SystemIcon(SilDev.Resource.SystemIconKey.HELP_SHIELD, Main.SysIcoResPath);

            logoPanel.BackColor = Main.Colors.Layout;

            updateBtn.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.NETWORK, Main.SysIcoResPath);
            updateBtn.ForeColor = Main.Colors.ButtonText;
            updateBtn.BackColor = Main.Colors.Button;
            updateBtn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
            updateBtn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;

            aboutInfoLabel.ActiveLinkColor = Main.Colors.Layout;
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            Lang.SetControlLang(this);
            string title = Lang.GetText("AboutFormTitle");
            if (!string.IsNullOrWhiteSpace(title))
                Text = $"{title} Portable Apps Suite";

            copyrightLabel.Text = string.Format(copyrightLabel.Text, DateTime.Now.Year);

            appsLauncherVersion.Text = Main.CurrentFileVersion;
#if x86
            appsDownloaderVersion.Text = Main.FileVersion("%CurrentDir%\\Binaries\\AppsDownloader.exe");
#else
            appsDownloaderVersion.Text = Main.FileVersion("%CurrentDir%\\Binaries\\AppsDownloader64.exe");
#endif
            appsLauncherUpdaterVersion.Text = Main.FileVersion("%CurrentDir%\\Binaries\\Updater.exe");

            updateBtnPanel.Width = TextRenderer.MeasureText(updateBtn.Text, updateBtn.Font).Width + 32;

            aboutInfoLabel.BorderStyle = BorderStyle.None;
            aboutInfoLabel.Text = string.Format(Lang.GetText(aboutInfoLabel), "Si13n7 Developments", Lang.GetText("aboutInfoLabelLinkLabel1"), Lang.GetText("aboutInfoLabelLinkLabel2"));
            aboutInfoLabel.Links.Clear();
            SilDev.Forms.LinkLabel.LinkText(aboutInfoLabel, "Si13n7 Developments", "http://www.si13n7.com");
            SilDev.Forms.LinkLabel.LinkText(aboutInfoLabel, Lang.GetText("aboutInfoLabelLinkLabel1"), "http://paypal.si13n7.com");
            SilDev.Forms.LinkLabel.LinkText(aboutInfoLabel, Lang.GetText("aboutInfoLabelLinkLabel2"), "https://support.si13n7.com");
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
            SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\Binaries\\Updater.exe" }, 0);

        private void updateChecker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) =>
            updateBtn.Enabled = true;

        private void closeToUpdate_Tick(object sender, EventArgs e)
        {
            if (!updateChecker.IsBusy)
            {
                closeToUpdate.Enabled = false;
                SilDev.MsgBox.Show(this, Lang.GetText("NoUpdatesFoundMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void aboutInfoLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.LinkData is Uri)
                Process.Start(e.Link.LinkData.ToString());
        }
    }
}
