using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class SettingsForm : Form
    {
        bool result = false;
        int[] customColors { get; set; }

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();
            Icon = Properties.Resources.PortableApps_blue;
            foreach (TabPage tab in tabCtrl.TabPages)
            {
                tab.BackgroundImage = Main.LayoutBackground;
                tab.BackColor = Main.Colors.Layout;
            }
            previewBg.BackgroundImage = Main.LayoutBackground;
            previewLogoBox.BackgroundImage = Main.GetImageFiltered(Properties.Resources.PortableApps_Logo_gray, previewLogoBox.Height, previewLogoBox.Height);
            previewImgList.Images.Add(Properties.Resources.executable);
            previewImgList.Images.Add(Properties.Resources.executable);
            foreach (string key in Main.AppsDict.Keys)
                appsBox.Items.Add(key);
            if (!string.IsNullOrWhiteSpace(selectedItem) && appsBox.Items.Contains(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;
            fileTypes.MaxLength = short.MaxValue;
            if (!saveBtn.Focused)
                saveBtn.Select();
        }

        private void SettingsForm_Load(object sender, EventArgs e) =>
            LoadSettingsForm();

        private void LoadSettingsForm()
        {
            Lang.SetControlLang(this);
            string title = Lang.GetText("settingsBtn");
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;
            for (int i = 0; i < fileTypesMenu.Items.Count; i++)
                fileTypesMenu.Items[i].Text = Lang.GetText(fileTypesMenu.Items[i].Name);

            appDirs.Text = SilDev.Crypt.Base64.Decrypt(SilDev.Initialization.ReadValue("Settings", "AppDirs"));

            int value = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowOpacity"), out value);
            opacityNum.Value = value >= opacityNum.Minimum && value <= opacityNum.Maximum ? value : 95;

            value = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "WindowFadeInDuration"), out value);
            fadeInNum.Maximum = opacityNum.Value;
            fadeInNum.Value = value >= fadeInNum.Minimum && value <= fadeInNum.Maximum ? value : 4;

            defBgCheck.Checked = !Directory.Exists(Path.Combine(Application.StartupPath, "Assets\\cache\\bg"));

            mainColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowMainColor"), SilDev.WinAPI.GetSystemThemeColor());

            controlColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowControlColor"), SystemColors.Control);

            controlTextColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowControlTextColor"), SystemColors.ControlText);

            btnColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonColor"), SystemColors.ControlDark);

            btnHoverColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonHoverColor"), SilDev.WinAPI.GetSystemThemeColor());

            btnTextColorPanel.BackColor = Main.GetHtmlColor(SilDev.Initialization.ReadValue("Settings", "WindowButtonTextColor"), SystemColors.ControlText);

            StylePreviewUpdate();

            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (int i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText($"startMenuIntegrationOption{i}"));

            value = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "StartMenuIntegration"), out value);
            startMenuIntegration.SelectedIndex = value > 0 && value < startMenuIntegration.Items.Count ? value : 0;

            if (defaultPos.Items.Count > 0)
                defaultPos.Items.Clear();
            for (int i = 0; i < 2; i++)
                defaultPos.Items.Add(Lang.GetText($"defaultPosOption{i}"));

            value = 0;
            int.TryParse(SilDev.Initialization.ReadValue("Settings", "DefaultPosition"), out value);
            defaultPos.SelectedIndex = value > 0 && value < defaultPos.Items.Count ? value : 0;

            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (int i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText($"updateCheckOption{i}"));

            value = 0;
            if (!int.TryParse(SilDev.Initialization.ReadValue("Settings", "UpdateCheck"), out value))
                value = 4;
            if (value < 0)
                SilDev.Initialization.WriteValue("Settings", "UpdateCheck", 4);
            updateCheck.SelectedIndex = value > 0 && value < updateCheck.Items.Count ? value : 0;

            string langsDir = Path.Combine(Application.StartupPath, "Langs");
            if (Directory.Exists(langsDir))
            {
                foreach (string file in Directory.GetFiles(langsDir, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (!setLang.Items.Contains(name))
                        setLang.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            if (!setLang.Items.Contains(lang))
                lang = Lang.SystemUI;
            setLang.SelectedItem = lang;
        }

        private void StylePreviewUpdate()
        {
            previewMainColor.BackColor = mainColorPanel.BackColor;
            previewAppList.ForeColor = controlTextColorPanel.BackColor;
            previewAppList.BackColor = controlColorPanel.BackColor;
            previewAppListPanel.BackColor = controlColorPanel.BackColor;
            foreach (Button b in new Button[] { previewBtn1, previewBtn2 })
            {
                b.ForeColor = btnTextColorPanel.BackColor;
                b.BackColor = btnColorPanel.BackColor;
                b.FlatAppearance.MouseOverBackColor = btnHoverColorPanel.BackColor;
            }
        }

        private void button_TextChanged(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Text.Length < 22)
                b.TextAlign = ContentAlignment.MiddleCenter;
            else
                b.TextAlign = ContentAlignment.MiddleRight;
        }

        private void appsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            fileTypes.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes");
            startArg.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "startArg");
            endArg.Text = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "endArg");
            noConfirmCheck.Checked = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm").ToLower() == "true";
            runAsAdminCheck.Checked = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "RunAsAdmin").ToLower() == "true";
            noUpdatesCheck.Checked = SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoUpdates").ToLower() == "true";
            string restPointDir = Path.Combine(Application.StartupPath, "Restoration", Environment.MachineName, SilDev.Crypt.MD5.Encrypt(Main.WindowsInstallDateTime.ToString()).Substring(24), Main.AppsDict[appsBox.SelectedItem.ToString()], "FileAssociation");
            undoAssociationBtn.Enabled = Directory.Exists(restPointDir) && Directory.GetFiles(restPointDir, "*.ini", SearchOption.AllDirectories).Length > 0;
            undoAssociationBtn.Visible = undoAssociationBtn.Enabled;
        }

        private void locationBtn_Click(object sender, EventArgs e) =>
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());

        private void fileTypesMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            switch (i.Name)
            {
                case "fileTypesMenuItem1":
                    if (!string.IsNullOrWhiteSpace(fileTypes.Text))
                        Clipboard.SetText(fileTypes.Text);
                    break;
                case "fileTypesMenuItem2":
                    if (Clipboard.ContainsText())
                        fileTypes.Text = Clipboard.GetText();
                    break;
                case "fileTypesMenuItem3":
                    string appPath = Main.GetAppPath(Main.AppsDict[appsBox.SelectedItem.ToString()]);
                    if (File.Exists(appPath))
                    {
                        string appDir = Path.GetDirectoryName(appPath);
                        string iniPath = Path.Combine(appDir, "App\\AppInfo\\appinfo.ini");
                        if (!File.Exists(iniPath))
                            iniPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appPath)}.ini");
                        if (File.Exists(iniPath))
                        {
                            string types = SilDev.Initialization.ReadValue("Associations", "FileTypes", iniPath);
                            if (!string.IsNullOrWhiteSpace(types))
                            {
                                fileTypes.Text = types.Replace(" ", string.Empty);
                                return;
                            }
                        }
                    }
                    SilDev.MsgBox.Show(this, Lang.GetText("NoDefaultTypesFoundMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        private bool fileTypesConflict()
        {
            Dictionary<string, List<string>> AlreadyDefined = new Dictionary<string, List<string>>();
            Main.AppConfigs = new List<string>();
            foreach (string app in Main.AppConfigs)
            {
                if (app == Main.AppsDict[appsBox.SelectedItem.ToString()])
                    continue;

                string types = SilDev.Initialization.ReadValue(app, "FileTypes");
                if (string.IsNullOrWhiteSpace(types))
                    continue;

                List<string> TextBoxTypes = new List<string>();
                TextBoxTypes.AddRange(fileTypes.Text.Replace("*", string.Empty).Replace(".", string.Empty).Split(','));

                List<string> ConfigTypes = new List<string>();
                ConfigTypes.AddRange(types.Replace("*", string.Empty).Replace(".", string.Empty).Split(','));

                foreach (string type in TextBoxTypes)
                {
                    if (ConfigTypes.Contains(type))
                    {
                        if (!AlreadyDefined.ContainsKey(app))
                            AlreadyDefined.Add(app, new List<string> { type });
                        else
                        {
                            if (!AlreadyDefined[app].Contains(type))
                                AlreadyDefined[app].Add(type);
                        }
                    }
                }
            }

            if (AlreadyDefined.Count > 0)
            {
                string msg = string.Empty;
                foreach (var entry in AlreadyDefined)
                    msg = $"{msg}{Environment.NewLine}{Main.AppsDict.FirstOrDefault(x => x.Value == entry.Key).Key}: \"{string.Join(", ", entry.Value)}\"{Environment.NewLine}";
                if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("associateConflictMsg"), msg), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return true;
            }

            return false;
        }

        private void associateBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileTypes.Text))
            {
                SilDev.MsgBox.Show(this, Lang.GetText($"{((Control)sender).Name}Msg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (fileTypesConflict())
            {
                SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (fileTypes.Text != SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes"))
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes", fileTypes.Text);

            if (!SilDev.Elevation.IsAdministrator)
            {
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = $"\"DF8AB31C-1BC0-4EC1-BEC0-9A17266CAEFC\" \"{Main.AppsDict[appsBox.SelectedItem.ToString()]}\"",
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                }, 0);
                appsBox_SelectedIndexChanged(appsBox, EventArgs.Empty);
            }
            else
                Main.AssociateFileTypes(Main.AppsDict[appsBox.SelectedItem.ToString()]);
        }

        private void undoAssociationBtn_Click(object sender, EventArgs e)
        {
            string restPointDir = Path.Combine(Application.StartupPath, "Restoration", Environment.MachineName, SilDev.Crypt.MD5.Encrypt(Main.WindowsInstallDateTime.ToString()).Substring(24), Main.AppsDict[appsBox.SelectedItem.ToString()], "FileAssociation");
            string restPointCfgPath = string.Empty;
            using (OpenFileDialog dialog = new OpenFileDialog() { Filter = "INI Files(*.ini) | *.ini", InitialDirectory = restPointDir, Multiselect = false, RestoreDirectory = false })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;
                restPointCfgPath = dialog.FileName;
            }
            if (File.Exists(restPointCfgPath) && !SilDev.Elevation.IsAdministrator)
            {
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = $"\"A00C02E5-283A-44ED-9E4D-B82E8F87318F\" \"{restPointCfgPath}\"",
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                }, 0);
                appsBox_SelectedIndexChanged(appsBox, EventArgs.Empty);
            }
        }

        private void opacityNum_ValueChanged(object sender, EventArgs e) =>
            fadeInNum.Maximum = ((NumericUpDown)sender).Value;

        private void setBgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, Multiselect = false })
            {
                ImageCodecInfo[] imageCodecs = ImageCodecInfo.GetImageEncoders();
                List<string> codecExtensions = new List<string>();
                for (int i = 0; i < imageCodecs.Length; i++)
                {
                    codecExtensions.Add(imageCodecs[i].FilenameExtension.ToLower());
                    dialog.Filter = string.Format("{0}{1}{2} ({3})|{3}", dialog.Filter, i > 0 ? "|" : string.Empty, imageCodecs[i].CodecName.Substring(8).Replace("Codec", "Files").Trim(), codecExtensions[codecExtensions.Count - 1]);
                }
                dialog.Filter = string.Format("{0}|Image Files ({1})|{1}", dialog.Filter, string.Join(";", codecExtensions));
                dialog.FilterIndex = imageCodecs.Length + 1;
                dialog.ShowDialog();
                if (File.Exists(dialog.FileName))
                {
                    try
                    {
                        Image img = Main.GetImageFiltered(Image.FromFile(dialog.FileName), SmoothingMode.HighQuality);
                        string ext = Path.GetExtension(dialog.FileName).ToLower();
                        string bgPath = Path.Combine(Application.StartupPath, "Assets\\cache\\bg", $"image{ext}");
                        string bgDir = Path.GetDirectoryName(bgPath);
                        if (Directory.Exists(bgDir))
                            Directory.Delete(bgDir, true);
                        if (!Directory.Exists(bgDir))
                            Directory.CreateDirectory(bgDir);
                        switch (ext)
                        {
                            case ".bmp":
                            case ".dib":
                            case ".rle":
                                img.Save(bgPath, ImageFormat.Bmp);
                                break;
                            case ".jpg":
                            case ".jpeg":
                            case ".jpe":
                            case ".jfif":
                                img.Save(bgPath, ImageFormat.Jpeg);
                                break;
                            case ".gif":
                                img.Save(bgPath, ImageFormat.Gif);
                                break;
                            case ".tif":
                            case ".tiff":
                                img.Save(bgPath, ImageFormat.Tiff);
                                break;
                            default:
                                img.Save(bgPath, ImageFormat.Png);
                                break;
                        }
                        defBgCheck.Checked = false;
                        previewBg.BackgroundImage = Image.FromStream(new MemoryStream(File.ReadAllBytes(bgPath)));
                        if (!result)
                            result = true;
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                        SilDev.MsgBox.Show(this, Lang.GetText("OperationFailedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void colorPanel_MouseEnter(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            p.BackColor = Color.FromArgb(128, p.BackColor.R, p.BackColor.G, p.BackColor.B);
        }

        private void colorPanel_MouseLeave(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            p.BackColor = Color.FromArgb(p.BackColor.R, p.BackColor.G, p.BackColor.B);
        }

        private void colorPanel_Click(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            using (ColorDialog dialog = new ColorDialog() { AllowFullOpen = true, AnyColor = true, Color = p.BackColor, FullOpen = true })
            {
                if (customColors != null)
                    dialog.CustomColors = customColors;
                if (dialog.ShowDialog() != DialogResult.Cancel)
                {
                    if (dialog.Color != p.BackColor)
                        p.BackColor = Color.FromArgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
                    if (customColors != dialog.CustomColors)
                        customColors = dialog.CustomColors;
                }
            }
            StylePreviewUpdate();
        }

        private void resetColorsBtn_Click(object sender, EventArgs e)
        {
            mainColorPanel.BackColor = SilDev.WinAPI.GetSystemThemeColor();
            btnColorPanel.BackColor = SystemColors.ControlDark;
            controlColorPanel.BackColor = SystemColors.Control;
            controlTextColorPanel.BackColor = SystemColors.ControlText;
            btnColorPanel.BackColor = SystemColors.ControlDark;
            btnHoverColorPanel.BackColor = mainColorPanel.BackColor;
            btnTextColorPanel.BackColor = SystemColors.ControlText;
            StylePreviewUpdate();
        }

        private void previewAppList_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            e.Graphics.TranslateTransform((int)(p.Width / (Math.PI * 2)), p.Width + 40);
            e.Graphics.RotateTransform(-70);
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            e.Graphics.DrawString("Preview", new Font("Comic Sans MS", 24f), new SolidBrush(Color.FromArgb((byte)~p.BackColor.R, (byte)~p.BackColor.G, (byte)~p.BackColor.B)), 0f, 0f);
        }

        private void addToShellBtn_Click(object sender, EventArgs e)
        {
            string fileContent = string.Format("Windows Registry Editor Version 5.00{0}{0}[HKEY_CLASSES_ROOT\\*\\shell\\portableapps]{0}@=\"{2}\"{0}\"Icon\"=\"\\\"{1}\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\*\\shell\\portableapps\\command]{0}@=\"\\\"{1}\\\" \\\"%1\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\Directory\\shell\\portableapps]{0}@=\"{2}\"{0}\"Icon\"=\"\\\"{1}\\\"\"{0}{0}[HKEY_CLASSES_ROOT\\Directory\\shell\\portableapps\\command]{0}@=\"\\\"{1}\\\" \\\"%1\\\"\"", Environment.NewLine, Application.ExecutablePath.Replace("\\", "\\\\"), Lang.GetText("shellText"));
            string regFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), $"{SilDev.Crypt.MD5.Encrypt(new Random().Next(short.MinValue, short.MaxValue).ToString())}.reg");
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
            string regFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), $"{SilDev.Crypt.MD5.Encrypt(new Random().Next(short.MinValue, short.MaxValue).ToString())}.reg");
            try
            {
                File.WriteAllText(Path.Combine(Application.StartupPath, regFile), fileContent);
                bool imported = SilDev.Reg.ImportFile(regFile, true);
                if (File.Exists(regFile))
                    File.Delete(regFile);
                if (imported)
                {
                    string SendToPath = SilDev.Run.EnvironmentVariableFilter($"%SendTo%\\{FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileDescription}.lnk");
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

        private void ToolTipAtMouseEnter(object sender, EventArgs e) =>
            toolTip.SetToolTip((Control)sender, Lang.GetText($"{((Control)sender).Name}Tip"));

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(fileTypes.Text) || string.IsNullOrEmpty(fileTypes.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes").Length > 0)
            {
                if (!fileTypesConflict())
                    SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "FileTypes", fileTypes.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(startArg.Text) || string.IsNullOrEmpty(startArg.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "StartArg").Length > 0)
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "StartArg", startArg.Text);

            if (!string.IsNullOrWhiteSpace(endArg.Text) || string.IsNullOrEmpty(endArg.Text) && SilDev.Initialization.ReadValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "EndArg").Length > 0)
                SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "EndArg", endArg.Text);

            SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoConfirm", noConfirmCheck.Checked);

            SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "RunAsAdmin", runAsAdminCheck.Checked);

            SilDev.Initialization.WriteValue(Main.AppsDict[appsBox.SelectedItem.ToString()], "NoUpdates", noUpdatesCheck.Checked);

            if (defBgCheck.Checked)
            {
                try
                {
                    string bgDir = Path.Combine(Application.StartupPath, "Assets\\cache\\bg");
                    if (Directory.Exists(bgDir))
                        Directory.Delete(bgDir, true);
                    Main.LayoutBackground = Properties.Resources.diagonal_pattern;
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }

            SilDev.Initialization.WriteValue("Settings", "WindowOpacity", opacityNum.Value);

            SilDev.Initialization.WriteValue("Settings", "WindowFadeInDuration", fadeInNum.Value);

            SilDev.Initialization.WriteValue("Settings", "WindowMainColor", mainColorPanel.BackColor != SilDev.WinAPI.GetSystemThemeColor() ? $"#{mainColorPanel.BackColor.R.ToString("X2")}{mainColorPanel.BackColor.G.ToString("X2")}{mainColorPanel.BackColor.B.ToString("X2")}" : "Default");

            SilDev.Initialization.WriteValue("Settings", "WindowControlColor", controlColorPanel.BackColor != SystemColors.Control ? $"#{controlColorPanel.BackColor.R.ToString("X2")}{controlColorPanel.BackColor.G.ToString("X2")}{controlColorPanel.BackColor.B.ToString("X2")}" : "Default");

            SilDev.Initialization.WriteValue("Settings", "WindowControlTextColor", controlTextColorPanel.BackColor != SystemColors.ControlText ? $"#{controlTextColorPanel.BackColor.R.ToString("X2")}{controlTextColorPanel.BackColor.G.ToString("X2")}{controlTextColorPanel.BackColor.B.ToString("X2")}" : "Default");

            SilDev.Initialization.WriteValue("Settings", "WindowButtonColor", btnColorPanel.BackColor != SystemColors.ControlDark ? $"#{btnColorPanel.BackColor.R.ToString("X2")}{btnColorPanel.BackColor.G.ToString("X2")}{btnColorPanel.BackColor.B.ToString("X2")}" : "Default");

            SilDev.Initialization.WriteValue("Settings", "WindowButtonHoverColor", btnHoverColorPanel.BackColor != SilDev.WinAPI.GetSystemThemeColor() ? $"#{btnHoverColorPanel.BackColor.R.ToString("X2")}{btnHoverColorPanel.BackColor.G.ToString("X2")}{btnHoverColorPanel.BackColor.B.ToString("X2")}" : "Default");

            SilDev.Initialization.WriteValue("Settings", "WindowButtonTextColor", btnTextColorPanel.BackColor != SystemColors.ControlText ? $"#{btnTextColorPanel.BackColor.R.ToString("X2")}{btnTextColorPanel.BackColor.G.ToString("X2")}{btnTextColorPanel.BackColor.B.ToString("X2")}" : "Default");

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
                    saveDirs = Directory.Exists(SilDev.Run.EnvironmentVariableFilter(appDirs.Text));
                if (saveDirs)
                    SilDev.Initialization.WriteValue("Settings", "AppDirs", SilDev.Crypt.Base64.Encrypt(appDirs.Text.TrimEnd()));
            }
            else
                SilDev.Initialization.WriteValue("Settings", "AppDirs", string.Empty);

            SilDev.Initialization.WriteValue("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex);
            if (startMenuIntegration.SelectedIndex == 0)
            {
                string StartMenuFolderPath = SilDev.Run.EnvironmentVariableFilter("%StartMenu%\\Programs\\Portable Apps");
                if (Directory.Exists(StartMenuFolderPath))
                    Directory.Delete(StartMenuFolderPath, true);
            }

            SilDev.Initialization.WriteValue("Settings", "DefaultPosition", defaultPos.SelectedIndex);

            SilDev.Initialization.WriteValue("Settings", "UpdateCheck", updateCheck.SelectedIndex);

            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            SilDev.Initialization.WriteValue("Settings", "Lang", setLang.SelectedItem);
            if (lang != setLang.SelectedItem.ToString())
                LoadSettingsForm();

            if (!result)
                result = true;
            SilDev.MsgBox.Show(this, Lang.GetText("SavedSettings"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void exitBtn_Click(object sender, EventArgs e) =>
            DialogResult = result ? DialogResult.Yes : DialogResult.No;
    }
}
