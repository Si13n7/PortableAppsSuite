using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class SettingsForm : Form
    {
        private static string _fileTypeApps;

        private static string FileTypeApps
        {
            get { return _fileTypeApps; }
            set { _fileTypeApps = value; }
        }

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_blue;
            foreach (string key in Main.AppsDict.Keys)
                appsBox.Items.Add(key);
            appsBox.SelectedItem = selectedItem;
            fileTypes.MaxLength = short.MaxValue;
            if (!saveBtn.Focused)
                saveBtn.Select();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            SetSettingsFormLang();
        }

        private void SetSettingsFormLang()
        {
            Lang.SetControlLang(this);
            string title = Lang.GetText("settingsBtn");
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;
            appDirs.Text = SilDev.Crypt.Base64.Decrypt(SilDev.Initialization.ReadValue("Settings", "AppDirs"));
            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (int i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText(string.Format("startMenuIntegrationOption{0}", i)));
            int index = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartMenuIntegration"), out index);
            startMenuIntegration.SelectedIndex = index > 0 && index < startMenuIntegration.Items.Count ? index : 0;
            if (startItem.Items.Count > 0)
                startItem.Items.Clear();
            for (int i = 0; i < 2; i++)
                startItem.Items.Add(Lang.GetText(string.Format("startItemOption{0}", i)));
            index = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartItem"), out index);
            startItem.SelectedIndex = index > 0 && index < startItem.Items.Count ? index : 0;
            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (int i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText(string.Format("updateCheckOption{0}", i)));
            index = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "UpdateCheck"), out index);
            if (index < 0)
                SilDev.Initialization.WriteValue("Settings", "UpdateCheck", 4);
            updateCheck.SelectedIndex = index > 0 && index < updateCheck.Items.Count ? index : 0;
            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            if (!setLang.Items.Contains(lang))
                lang = Lang.SystemUI;
            setLang.SelectedItem = lang;
        }

        private void button_TextChanged(object sender, EventArgs e)
        {
            if (((Button)sender).Text.Length < 22)
                ((Button)sender).TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            else
                ((Button)sender).TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        }

        private void appsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileTypeApps = SilDev.Initialization.ReadValue("Settings", "Apps");
            fileTypes.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes");
            startArg.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "startArg");
            endArg.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "endArg");
            noConfirmCheck.Checked = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm").ToLower() == "true";
        }

        private void locationBtn_Click(object sender, EventArgs e)
        {
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());
        }

        private void associateBtn_Click(object sender, EventArgs e)
        {
            if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText(string.Format("{0}Msg0", (sender as Control).Name)), Main.CurrentVersion), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
            if (fileTypes.Text != SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes"))
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes", fileTypes.Text);
            if (string.IsNullOrWhiteSpace(fileTypes.Text))
            {
                SilDev.MsgBox.Show(this, Lang.GetText(string.Format("{0}Msg1", (sender as Control).Name)), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!SilDev.Elevation.IsAdministrator)
                SilDev.Run.App(Application.StartupPath, Path.GetFileName(Application.ExecutablePath), string.Format("\"DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC\" \"{0}\"", Main.AppsDict[appsBox.SelectedItem.ToString()]), true, 0);
            else
                Main.AssociateFileTypes(Main.AppsDict[appsBox.SelectedItem.ToString()]);
        }

        private void addToShellBtn_Click(object sender, EventArgs e)
        {
            string fileContent = string.Format("Windows Registry Editor Version 5.00{0}{0}[HKEY_CLASSES_ROOT\\*\\shell\\portableapps]{0}@=\"{2}\"{0}\"Icon\"=\"\\\"{1}\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\*\\shell\\portableapps\\command]{0}@=\"\\\"{1}\\\" \\\"%1\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\Directory\\shell\\portableapps]{0}@=\"{2}\"{0}\"Icon\"=\"\\\"{1}\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\Directory\\shell\\portableapps\\command]{0}@=\"\\\"{1}\\\" \\\"%1\\\"\"", Environment.NewLine, Application.ExecutablePath.Replace("\\", "\\\\"), Lang.GetText("shellText"));
            string regFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), string.Format("{0}.reg", SilDev.Crypt.MD5.Encrypt(new Random().Next(short.MinValue, short.MaxValue).ToString())));
            try
            {
                if (File.Exists(regFile))
                    File.Delete(regFile);
                File.WriteAllText(Path.Combine(Application.StartupPath, regFile), fileContent);
                bool imported = SilDev.Reg.ImportFile(regFile, true);
                if (File.Exists(regFile))
                    File.Delete(regFile);
                if (imported)
                {
                    SilDev.Data.CreateShortcut(Application.ExecutablePath, Path.Combine("%SendTo%", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileDescription));
                    if (!Main.EnableLUA || SilDev.Elevation.IsAdministrator)
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void rmFromShellBtn_Click(object sender, EventArgs e)
        {
            string fileContent = string.Format("Windows Registry Editor Version 5.00{0}{0}[-HKEY_CLASSES_ROOT\\*\\shell\\portableapps]{0}[-HKEY_CLASSES_ROOT\\Directory\\shell\\portableapps]", Environment.NewLine);
            string regFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), string.Format("{0}.reg", SilDev.Crypt.MD5.Encrypt(new Random().Next(short.MinValue, short.MaxValue).ToString())));
            try
            {
                File.WriteAllText(Path.Combine(Application.StartupPath, regFile), fileContent);
                bool imported = SilDev.Reg.ImportFile(regFile, true);
                if (File.Exists(regFile))
                    File.Delete(regFile);
                if (imported)
                {
                    string SendToPath = SilDev.Run.EnvironmentVariableFilter(string.Format("%SendTo%\\{0}.lnk", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileDescription));
                    if (File.Exists(SendToPath))
                        File.Delete(SendToPath);
                    if (!Main.EnableLUA || SilDev.Elevation.IsAdministrator)
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        private void ToolTipAtMouseEnter(object sender, EventArgs e)
        {
            toolTip.SetToolTip(sender as Control, Lang.GetText(string.Format("{0}Tip", (sender as Control).Name)));
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (!FileTypeApps.Contains(string.Format("{0},", Main.AppsDict[appsBox.SelectedItem.ToString()])))
                SilDev.Initialization.WriteValue("Settings", "Apps", string.Format("{0}{1},", FileTypeApps, Main.AppsDict[appsBox.SelectedItem.ToString()]));
            if (!string.IsNullOrWhiteSpace(fileTypes.Text) || string.IsNullOrEmpty(fileTypes.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes").Length > 0)
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes", fileTypes.Text.Trim());
            if (!string.IsNullOrWhiteSpace(startArg.Text) || string.IsNullOrEmpty(startArg.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "StartArg").Length > 0)
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "StartArg", startArg.Text);
            if (!string.IsNullOrWhiteSpace(endArg.Text) || string.IsNullOrEmpty(endArg.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "EndArg").Length > 0)
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "EndArg", endArg.Text);
            SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm", noConfirmCheck.Checked);
            if (!string.IsNullOrWhiteSpace(appDirs.Text))
            {
                bool saveDirs = true;
                if (appDirs.Text.Contains(Environment.NewLine))
                {
                    foreach (string d in appDirs.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        if (string.IsNullOrWhiteSpace(d))
                            continue;
                        string dir = SilDev.Run.EnvironmentVariableFilter(d);
                        if (!Directory.Exists(dir))
                        {
                            try
                            {
                                Directory.CreateDirectory(dir);
                            }
                            catch (Exception ex)
                            {
                                saveDirs = false;
                                SilDev.Log.Debug(ex);
                            }
                            break;
                        }
                    }
                }
                else
                    saveDirs = Directory.Exists(appDirs.Text);
                if (saveDirs)
                    SilDev.Initialization.WriteValue("Settings", "AppDirs", SilDev.Crypt.Base64.Encrypt(appDirs.Text));
            }
            SilDev.Initialization.WriteValue("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex);
            if (startMenuIntegration.SelectedIndex == 0)
            {
                string StartMenuFolderPath = SilDev.Run.EnvironmentVariableFilter("%StartMenu%\\Programs\\Portable Apps");
                if (Directory.Exists(StartMenuFolderPath))
                    Directory.Delete(StartMenuFolderPath, true);
            }
            SilDev.Initialization.WriteValue("Settings", "StartItem", startItem.SelectedIndex);
            SilDev.Initialization.WriteValue("Settings", "UpdateCheck", updateCheck.SelectedIndex);
            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            SilDev.Initialization.WriteValue("Settings", "Lang", setLang.SelectedItem);
            if (lang != setLang.SelectedItem.ToString())
                SetSettingsFormLang();
            SilDev.MsgBox.Show(this, Lang.GetText("SavedSettings"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
