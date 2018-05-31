namespace AppsDownloader.Windows
{
    using System;
    using System.Media;
    using System.Windows.Forms;
    using Libraries;
    using SilDev.Forms;

    public partial class LangSelectionForm : Form
    {
        private readonly AppData _appData;

        public LangSelectionForm(AppData appData)
        {
            InitializeComponent();

            Text = Language.GetText(Name);
            appNameLabel.Text = appData.Name;

            _appData = appData;
            langBox.Items.AddRange((object[])_appData.Languages.ToArray().Clone());
            langBox.SelectedItem = _appData.Settings.ArchiveLang;
            if (langBox.SelectedIndex < 0)
                langBox.SelectedIndex = 0;
            rememberLangCheck.Checked = _appData.Settings.ArchiveLangConfirmed;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void LangSelectionForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);
            Language.SetControlLang(this);
        }

        private void SetArchiveLangForm_Shown(object sender, EventArgs e) =>
            SystemSounds.Asterisk.Play();

        private void OKBtn_Click(object sender, EventArgs e)
        {
            _appData.Settings.ArchiveLang = langBox.GetItemText(langBox.SelectedItem);
            _appData.Settings.ArchiveLangConfirmed = rememberLangCheck.Checked;
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
