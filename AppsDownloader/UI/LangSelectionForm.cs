using System;
using System.Media;
using System.Windows.Forms;

namespace AppsDownloader
{
    public partial class LangSelectionForm : Form
    {
        private string section { get; set; }

        public LangSelectionForm(string name, string text, string[] langs)
        {
            InitializeComponent();
            Lang.SetControlLang(this);
            Text = Lang.GetText($"{Name}Titel");
            section = name;
            appNameLabel.Text = text;
            langBox.Items.AddRange(langs);
            langBox.SelectedIndex = 0;
        }

        private void SetArchiveLangForm_Shown(object sender, EventArgs e) =>
            SystemSounds.Asterisk.Play();

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SilDev.Ini.Write(section, "ArchiveLang", langBox.GetItemText(langBox.SelectedItem));
            if (rememberLangCheck.Checked)
                SilDev.Ini.Write(section, "ArchiveLangConfirmed", rememberLangCheck.Checked);
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
