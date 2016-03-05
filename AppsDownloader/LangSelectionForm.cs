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
            Text = Lang.GetText(Name);
            section = name;
            AppNameLabel.Text = text;
            LangBox.Items.AddRange(langs);
            LangBox.SelectedIndex = 0;
        }

        private void SetArchiveLangForm_Shown(object sender, EventArgs e) =>
            SystemSounds.Asterisk.Play();

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SilDev.Initialization.WriteValue(section, "ArchiveLang", LangBox.GetItemText(LangBox.SelectedItem));
            if (NoLangQuestionCheck.Checked)
                SilDev.Initialization.WriteValue(section, "ArchiveLangConfirmed", NoLangQuestionCheck.Checked);
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
