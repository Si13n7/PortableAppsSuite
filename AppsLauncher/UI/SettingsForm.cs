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
        bool result = false, saved = false;
        int[] customColors { get; set; }

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();

            Icon = SilDev.Resource.SystemIcon(SilDev.Resource.SystemIconKey.SYSTEM_CONTROL);

            foreach (TabPage tab in tabCtrl.TabPages)
            {
                tab.BackgroundImage = Main.BackgroundImage;
                tab.BackgroundImageLayout = Main.BackgroundImageLayout;
                tab.BackColor = Main.Colors.Layout;
            }

            locationBtn.BackgroundImage = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.DIRECTORY);

            associateBtn.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC);
            try
            {
                undoAssociationBtn.Image = new Bitmap(28, 16);
                using (Graphics g = Graphics.FromImage(undoAssociationBtn.Image))
                {
                    g.DrawImage(SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC), 0, 0);
                    g.DrawImage(SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UNDO), 12, 0);
                }
            }
            catch
            {
                undoAssociationBtn.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC);
                undoAssociationBtn.ImageAlign = ContentAlignment.MiddleLeft;
                undoAssociationBtn.Text = "<=";
                if (undoAssociationBtn.Image != null)
                    undoAssociationBtn.TextAlign = ContentAlignment.MiddleRight;
            }

            previewBg.BackgroundImage = SilDev.Drawing.ImageFilter(Main.BackgroundImage, (int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1, SmoothingMode.HighQuality);
            previewBg.BackgroundImageLayout = Main.BackgroundImageLayout;
            previewLogoBox.BackgroundImage = SilDev.Drawing.ImageFilter(Properties.Resources.PortableApps_Logo_gray, previewLogoBox.Height, previewLogoBox.Height);
            previewImgList.Images.Add(SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.EXE));
            previewImgList.Images.Add(SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.EXE));

            foreach (Button btn in new Button[] { saveBtn, exitBtn })
            {
                btn.ForeColor = Main.Colors.ButtonText;
                btn.BackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            foreach (string key in Main.AppsDict.Keys)
                appsBox.Items.Add(key);
            if (!string.IsNullOrWhiteSpace(selectedItem) && appsBox.Items.Contains(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            fileTypes.MaxLength = short.MaxValue;

            addToShellBtn.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC);
            rmFromShellBtn.Image = SilDev.Resource.SystemIconAsImage(SilDev.Resource.SystemIconKey.UAC);

            if (!saveBtn.Focused)
                saveBtn.Select();
        }

        private void SettingsForm_Load(object sender, EventArgs e) =>
            LoadSettingsForm();

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) =>
            DialogResult = result && saved ? DialogResult.Yes : DialogResult.No;

        private void LoadSettingsForm()
        {
            Lang.SetControlLang(this);
            string title = Lang.GetText("settingsBtn");
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;
            for (int i = 0; i < fileTypesMenu.Items.Count; i++)
                fileTypesMenu.Items[i].Text = Lang.GetText(fileTypesMenu.Items[i].Name);

            int value = SilDev.Ini.ReadInteger("Settings", "WindowOpacity");
            opacityNum.Value = value >= opacityNum.Minimum && value <= opacityNum.Maximum ? value : 95;

            value = SilDev.Ini.ReadInteger("Settings", "WindowFadeInDuration");
            fadeInNum.Maximum = opacityNum.Value;
            fadeInNum.Value = value >= fadeInNum.Minimum && value <= fadeInNum.Maximum ? value : 1;
            
            defBgCheck.Checked = !Directory.Exists(Path.Combine(Application.StartupPath, "Assets\\cache\\bg"));
            if (bgLayout.Items.Count > 0)
                bgLayout.Items.Clear();
            for (int i = 0; i < 5; i++)
                bgLayout.Items.Add(Lang.GetText($"bgLayoutOption{i}"));

            value = SilDev.Ini.ReadInteger("Settings", "WindowBgLayout", 1);
            bgLayout.SelectedIndex = value > 0 && value < bgLayout.Items.Count ? value : 1;

            mainColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowMainColor"), Main.Colors.System);
            controlColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlColor"), SystemColors.Control);
            controlTextColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowControlTextColor"), SystemColors.ControlText);
            btnColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonColor"), SystemColors.ControlDark);
            btnHoverColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonHoverColor"), Main.Colors.System);
            btnTextColorPanel.BackColor = SilDev.Drawing.ColorFromHtml(SilDev.Ini.Read("Settings", "WindowButtonTextColor"), SystemColors.ControlText);

            StylePreviewUpdate();

            appDirs.Text = new SilDev.Crypt.Base64().DecodeString(SilDev.Ini.Read("Settings", "AppDirs"));

            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (int i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText($"startMenuIntegrationOption{i}"));
            startMenuIntegration.SelectedIndex = SilDev.Ini.ReadBoolean("Settings", "StartMenuIntegration", false) ? 1 : 0;

            if (defaultPos.Items.Count > 0)
                defaultPos.Items.Clear();
            for (int i = 0; i < 2; i++)
                defaultPos.Items.Add(Lang.GetText($"defaultPosOption{i}"));

            value = SilDev.Ini.ReadInteger("Settings", "DefaultPosition", 0);
            defaultPos.SelectedIndex = value > 0 && value < defaultPos.Items.Count ? value : 0;

            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (int i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText($"updateCheckOption{i}"));
            value = SilDev.Ini.ReadInteger("Settings", "UpdateCheck", 4);
            if (value < 0)
                SilDev.Ini.Write("Settings", "UpdateCheck", 4);
            updateCheck.SelectedIndex = value > 0 && value < updateCheck.Items.Count ? value : 0;

            if (updateChannel.Items.Count > 0)
                updateChannel.Items.Clear();
            for (int i = 0; i < 2; i++)
                updateChannel.Items.Add(Lang.GetText($"updateChannelOption{i}"));
            value = SilDev.Ini.ReadInteger("Settings", "UpdateChannel", 0);
            updateChannel.SelectedIndex = value > 0 ? 1 : 0;

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
            string lang = SilDev.Ini.ReadString("Settings", "Lang", Lang.SystemUI);
            if (!setLang.Items.Contains(lang))
                lang = "en-US";
            setLang.SelectedItem = lang;
        }

        private void StylePreviewUpdate()
        {
            previewBg.BackgroundImageLayout = (ImageLayout)bgLayout.SelectedIndex;
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

        private void appsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string SelectedApp = ((ComboBox)sender).SelectedItem.ToString();
            fileTypes.Text = SilDev.Ini.Read(Main.AppsDict[SelectedApp], "FileTypes");
            startArg.Text = SilDev.Ini.Read(Main.AppsDict[SelectedApp], "startArg");
            endArg.Text = SilDev.Ini.Read(Main.AppsDict[SelectedApp], "endArg");
            noConfirmCheck.Checked = SilDev.Ini.ReadBoolean(Main.AppsDict[SelectedApp], "NoConfirm");
            runAsAdminCheck.Checked = SilDev.Ini.ReadBoolean(Main.AppsDict[SelectedApp], "RunAsAdmin");
            noUpdatesCheck.Checked = SilDev.Ini.ReadBoolean(Main.AppsDict[SelectedApp], "NoUpdates");
            string restPointDir = Path.Combine(Application.StartupPath, "Restoration", Environment.MachineName, SilDev.Crypt.MD5.EncryptString(Main.WindowsInstallDateTime.ToString()).Substring(24), Main.AppsDict[SelectedApp], "FileAssociation");
            undoAssociationBtn.Enabled = Directory.Exists(restPointDir) && Directory.GetFiles(restPointDir, "*.ini", SearchOption.AllDirectories).Length > 0;
            undoAssociationBtn.Visible = undoAssociationBtn.Enabled;
        }

        private void locationBtn_Click(object sender, EventArgs e) =>
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());

        private void fileTypesMenu_Paint(object sender, PaintEventArgs e) =>
            SilDev.Forms.ContextMenuStrip.SetFixedSingle((ContextMenuStrip)sender, e, Main.Colors.Layout);

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
                            string types = SilDev.Ini.Read("Associations", "FileTypes", iniPath);
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
            string section = Main.AppsDict[appsBox.SelectedItem.ToString()];
            Dictionary<string, List<string>> AlreadyDefined = new Dictionary<string, List<string>>();
            Main.AppConfigs = new List<string>();
            foreach (string app in Main.AppConfigs)
            {
                if (app == section)
                    continue;

                string types = SilDev.Ini.Read(app, "FileTypes");
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
                    msg = $"{msg}{Environment.NewLine}{Main.AppsDict.First(x => x.Value == entry.Key).Key}: {string.Join("; ", entry.Value)}{Environment.NewLine}";
                if (SilDev.MsgBox.Show(this, string.Format(Lang.GetText("associateConflictMsg"), msg), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return true;
            }

            return false;
        }

        private void associateBtn_Click(object sender, EventArgs e)
        {
            bool isNull = string.IsNullOrWhiteSpace(fileTypes.Text);
            if (!isNull)
            {
                if (fileTypes.Text.Contains(","))
                    isNull = fileTypes.Text.Split(',').Where(s => !s.StartsWith(".")).ToArray().Length == 0;
                else
                    isNull = fileTypes.Text.StartsWith(".");
            }

            if (isNull)
            {
                SilDev.MsgBox.Show(this, Lang.GetText($"{((Control)sender).Name}Msg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (fileTypesConflict())
            {
                SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string section = Main.AppsDict[appsBox.SelectedItem.ToString()];

            if (fileTypes.Text != SilDev.Ini.Read(section, "FileTypes"))
                saveBtn_Click(saveBtn, EventArgs.Empty);

            if (!SilDev.Elevation.IsAdministrator)
            {
                SilDev.Run.App(new ProcessStartInfo()
                {
                    Arguments = $"{Main.CmdLineActionGuid.FileTypeAssociation} \"{section}\"",
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                }, 0);
                appsBox_SelectedIndexChanged(appsBox, EventArgs.Empty);
            }
            else
                Main.AssociateFileTypes(section);
        }

        private void undoAssociationBtn_Click(object sender, EventArgs e)
        {
            string restPointDir = Path.Combine(Application.StartupPath, "Restoration", Environment.MachineName, SilDev.Crypt.MD5.EncryptString(Main.WindowsInstallDateTime.ToString()).Substring(24), Main.AppsDict[appsBox.SelectedItem.ToString()], "FileAssociation");
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
                    Arguments = $"{Main.CmdLineActionGuid.UndoFileTypeAssociation} \"{restPointCfgPath}\"",
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
                string path = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Assets\\bg");
                if (Directory.Exists(path))
                    dialog.InitialDirectory = path;
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
                        Image img = SilDev.Drawing.ImageFilter(Image.FromFile(dialog.FileName), SmoothingMode.HighQuality);
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
                        Image image = Image.FromStream(new MemoryStream(File.ReadAllBytes(bgPath)));
                        previewBg.BackgroundImage = SilDev.Drawing.ImageFilter(image, (int)Math.Round(image.Width * .65f) + 1, (int)Math.Round(image.Height * .65f) + 1, SmoothingMode.HighQuality);
                        if (!result)
                            result = true;
                        if (!saved)
                            saved = true;
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

        private void defBgCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            string bgDir = Path.Combine(Application.StartupPath, "Assets\\cache\\bg");
            try
            {
                Path.GetFullPath(bgDir);
                if (Directory.GetFiles(bgDir, "*", SearchOption.TopDirectoryOnly).Length == 0)
                    throw new FileNotFoundException();
                if (!cb.Checked)
                    previewBg.BackgroundImage = SilDev.Drawing.ImageFilter(Main.BackgroundImage, (int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1, SmoothingMode.HighQuality);
                else
                    previewBg.BackgroundImage = SilDev.Drawing.DimEmpty;
            }
            catch
            {
                if (!cb.Checked)
                    cb.Checked = !cb.Checked;
            }
        }

        private void bgLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!result)
                result = true;
            StylePreviewUpdate();
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
            if (!result)
                result = true;
            StylePreviewUpdate();
        }

        private void resetColorsBtn_Click(object sender, EventArgs e)
        {
            mainColorPanel.BackColor = Main.Colors.System;
            btnColorPanel.BackColor = SystemColors.ControlDark;
            controlColorPanel.BackColor = SystemColors.Control;
            controlTextColorPanel.BackColor = SystemColors.ControlText;
            btnColorPanel.BackColor = SystemColors.ControlDark;
            btnHoverColorPanel.BackColor = mainColorPanel.BackColor;
            btnTextColorPanel.BackColor = SystemColors.ControlText;
            if (!result)
                result = true;
            StylePreviewUpdate();
        }

        private void previewAppList_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            e.Graphics.TranslateTransform((int)(p.Width / (Math.PI * 2)), p.Width + 40);
            e.Graphics.RotateTransform(-70);
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            e.Graphics.DrawString("Preview", new Font("Comic Sans MS", 24f), new SolidBrush(Color.FromArgb(50, (byte)~p.BackColor.R, (byte)~p.BackColor.G, (byte)~p.BackColor.B)), 0f, 0f);
            e.Graphics.Dispose();
        }

        private void addToShellBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string[] keyContent = string.Format(Properties.Resources.RegDummy_addToShell, Application.ExecutablePath.Replace("\\", "\\\\"), Lang.GetText("shellText")).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                bool imported = SilDev.Reg.ImportFile(keyContent, true);
                if (imported)
                {
                    string ShortcutName = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileDescription.Replace("Portable", string.Empty).TrimStart();
                    SilDev.Data.CreateShortcut(Application.ExecutablePath, Path.Combine("%SendTo%", ShortcutName));
                    SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void rmFromShellBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string[] keyContent = Properties.Resources.RegDummy_rmFromShell.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                bool imported = SilDev.Reg.ImportFile(keyContent, true);
                if (imported)
                {
                    foreach (string f in Directory.GetFiles(SilDev.Run.EnvironmentVariableFilter("%SendTo%"), "*.lnk", SearchOption.TopDirectoryOnly))
                    {
                        string name = Path.GetFileName(f).ToLower();
                        if (name.Contains("apps") && name.Contains("launcher"))
                            File.Delete(f);
                    }
                    SilDev.MsgBox.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
                SilDev.MsgBox.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void shellBtns_TextChanged(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Text.Length < 22)
                b.TextAlign = ContentAlignment.MiddleCenter;
            else
                b.TextAlign = ContentAlignment.MiddleRight;
        }

        private void ToolTipAtMouseEnter(object sender, EventArgs e) =>
            toolTip.SetToolTip((Control)sender, Lang.GetText($"{((Control)sender).Name}Tip"));

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string section = Main.AppsDict[appsBox.SelectedItem.ToString()];

            string types = string.Empty;
            if (!string.IsNullOrWhiteSpace(fileTypes.Text))
            {
                if (!fileTypesConflict())
                {
                    List<string> typesList = new List<string>();
                    foreach (string item in $"{fileTypes.Text},".Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;
                        string type = new string(item.ToCharArray().Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
                        if (string.IsNullOrWhiteSpace(type) && type.Length > 1)
                            continue;
                        if (type.StartsWith("."))
                        {
                            while (type.Contains(".."))
                                type = type.Replace("..", ".");
                            if (typesList.Contains(type) || typesList.Contains(type.Substring(1)))
                                continue;
                        }
                        else
                        {
                            if (typesList.Contains(type) || typesList.Contains($".{type}"))
                                continue;
                        }
                        typesList.Add(type);
                    }
                    if (typesList.Count > 0)
                    {
                        typesList.Sort();
                        types = string.Join(",", typesList);
                        fileTypes.Text = types;
                    }
                }
                else
                    fileTypes.Text = SilDev.Ini.Read(section, "FileTypes");
            }
            SilDev.Ini.Write(section, "FileTypes", !string.IsNullOrWhiteSpace(types) ? types : null);

            SilDev.Ini.Write(section, "StartArg", !string.IsNullOrWhiteSpace(startArg.Text) ? startArg.Text : null);

            SilDev.Ini.Write(section, "EndArg", !string.IsNullOrWhiteSpace(endArg.Text) ? endArg.Text : null);

            SilDev.Ini.Write(section, "NoConfirm", noConfirmCheck.Checked ? (bool?)true : null);

            SilDev.Ini.Write(section, "RunAsAdmin", runAsAdminCheck.Checked ? (bool?)true : null);

            SilDev.Ini.Write(section, "NoUpdates", noUpdatesCheck.Checked ? (bool?)true : null);

            if (SilDev.Ini.GetKeys(section).Count == 0)
                SilDev.Ini.RemoveSection(section);

            if (defBgCheck.Checked)
            {
                try
                {
                    string bgDir = Path.Combine(Application.StartupPath, "Assets\\cache\\bg");
                    if (Directory.Exists(bgDir))
                    {
                        Directory.Delete(bgDir, true);
                        if (!result)
                            result = true;
                    }
                    Main.ResetBackgroundImage();
                    bgLayout.SelectedIndex = 1;
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }

            SilDev.Ini.Write("Settings", "WindowOpacity", opacityNum.Value != 95 ? (int?)opacityNum.Value : null);

            SilDev.Ini.Write("Settings", "WindowFadeInDuration", fadeInNum.Value != 1 ? (int?)fadeInNum.Value : null);

            SilDev.Ini.Write("Settings", "WindowBgLayout", bgLayout.SelectedIndex != 1 ? (int?)bgLayout.SelectedIndex : null);

            Color color = mainColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowMainColor", color != Main.Colors.System ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = controlColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowControlColor", color != SystemColors.Control ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = controlTextColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowControlTextColor", color != SystemColors.ControlText ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowButtonColor", color != SystemColors.ControlDark ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnHoverColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowButtonHoverColor", color != Main.Colors.System ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnTextColorPanel.BackColor;
            SilDev.Ini.Write("Settings", "WindowButtonTextColor", color != SystemColors.ControlText ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            string dirs = null;
            if (!string.IsNullOrWhiteSpace(appDirs.Text))
            {
                List<string> dirList = new List<string>();
                foreach (string item in $"{appDirs.Text}{Environment.NewLine}".Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;
                    string dir = SilDev.Run.EnvironmentVariableFilter(item);
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        dir = dir.Replace(Application.StartupPath, "%CurrentDir%\\");
                        while (dir.Contains("\\\\"))
                            dir = dir.Replace("\\\\", "\\");
                        if (!dirList.Contains(dir))
                            dirList.Add(dir);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
                if (dirList.Count > 0)
                {
                    dirList.Sort();
                    dirs = string.Join(Environment.NewLine, dirList);
                    appDirs.Text = dirs;
                }
            }
            SilDev.Ini.Write("Settings", "AppDirs", !string.IsNullOrWhiteSpace(dirs) ? new SilDev.Crypt.Base64().EncodeString(dirs) : null);

            SilDev.Ini.Write("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex != 0 ? (bool?)true : null);
            if (startMenuIntegration.SelectedIndex == 0)
            {
                try
                {
                    string StartMenuFolderPath = SilDev.Run.EnvironmentVariableFilter("%StartMenu%\\Programs");
                    string LauncherShortcutPath = Path.Combine(StartMenuFolderPath, $"Apps Launcher{(Environment.Is64BitProcess ? " (64-bit)" : string.Empty)}.lnk");
                    if (File.Exists(LauncherShortcutPath))
                        File.Delete(LauncherShortcutPath);
                    StartMenuFolderPath = Path.Combine(StartMenuFolderPath, "Portable Apps");
                    if (Directory.Exists(StartMenuFolderPath))
                        Directory.Delete(StartMenuFolderPath, true);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }

            SilDev.Ini.Write("Settings", "DefaultPosition", defaultPos.SelectedIndex != 0 ? (int?)defaultPos.SelectedIndex : null);

            SilDev.Ini.Write("Settings", "UpdateCheck", updateCheck.SelectedIndex != 4 ? (int?)updateCheck.SelectedIndex : null);

            SilDev.Ini.Write("Settings", "UpdateChannel", updateChannel.SelectedIndex != 0 ? (int?)updateChannel.SelectedIndex : null);

            string lang = SilDev.Ini.ReadString("Settings", "Lang", Lang.SystemUI);
            if (lang != setLang.SelectedItem.ToString())
            {
                SilDev.Ini.Write("Settings", "Lang", setLang.SelectedItem.ToString() != Lang.SystemUI ? setLang.SelectedItem : null);
                if (!result)
                    result = true;
                LoadSettingsForm();
            }

            if (!saved)
                saved = true;
            SilDev.MsgBox.Show(this, Lang.GetText("SavedSettings"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void exitBtn_Click(object sender, EventArgs e) =>
            Close();
    }
}
