using SilDev;
using SilDev.Forms;
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

            copyrightLabel.Text = string.Format(copyrightLabel.Text, DateTime.Now.Year);

            appsLauncherVersion.Text = Main.CurrentFileVersion;
#if x86
            appsDownloaderVersion.Text = Main.FileVersion("%CurDir%\\Binaries\\AppsDownloader.exe");
#else
            appsDownloaderVersion.Text = Main.FileVersion("%CurDir%\\Binaries\\AppsDownloader64.exe");
#endif
            appsLauncherUpdaterVersion.Text = Main.FileVersion("%CurDir%\\Binaries\\Updater.exe");

            updateBtnPanel.Width = TextRenderer.MeasureText(updateBtn.Text, updateBtn.Font).Width + 32;

            aboutInfoLabel.BorderStyle = BorderStyle.None;
            aboutInfoLabel.Text = string.Format(Lang.GetText(aboutInfoLabel), "Si13n7 Developments", Lang.GetText("aboutInfoLabelLinkLabel1"), Lang.GetText("aboutInfoLabelLinkLabel2"));
            aboutInfoLabel.Links.Clear();
            LINKLABEL.LinkText(aboutInfoLabel, "Si13n7 Developments", "http://www.si13n7.com");
            LINKLABEL.LinkText(aboutInfoLabel, Lang.GetText("aboutInfoLabelLinkLabel1"), "http://paypal.si13n7.com");
            LINKLABEL.LinkText(aboutInfoLabel, Lang.GetText("aboutInfoLabelLinkLabel2"), "https://support.si13n7.com");
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
