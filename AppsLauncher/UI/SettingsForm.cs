using SilDev;
using SilDev.Forms;
using System;
using System.Collections.Generic;
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
        #region MAIN FUNCTIONALITY

        bool result = false, saved = false;
        int[] customColors { get; set; }

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();

            if (Main.ScreenDpi > 96)
                Font = SystemFonts.CaptionFont;
            Icon = RESOURCE.SystemIcon(RESOURCE.SystemIconKey.SYSTEM_CONTROL, Main.SystemResourcePath);

            foreach (TabPage tab in tabCtrl.TabPages)
                tab.BackColor = Main.Colors.BaseDark;

            locationBtn.BackgroundImage = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.DIRECTORY, Main.SystemResourcePath);

            associateBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath);
            try
            {
                restoreFileTypesBtn.Image = new Bitmap(28, 16);
                using (Graphics g = Graphics.FromImage(restoreFileTypesBtn.Image))
                {
                    g.DrawImage(RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath), 0, 0);
                    g.DrawImage(RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UNDO, Main.SystemResourcePath), 12, 0);
                }
            }
            catch
            {
                restoreFileTypesBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath);
                restoreFileTypesBtn.ImageAlign = ContentAlignment.MiddleLeft;
                restoreFileTypesBtn.Text = "<=";
                if (restoreFileTypesBtn.Image != null)
                    restoreFileTypesBtn.TextAlign = ContentAlignment.MiddleRight;
            }

            previewBg.BackgroundImage = DRAWING.ImageFilter(Main.BackgroundImage, (int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1, SmoothingMode.HighQuality);
            previewBg.BackgroundImageLayout = Main.BackgroundImageLayout;
            previewLogoBox.BackgroundImage = DRAWING.ImageFilter(Properties.Resources.PortableApps_Logo_gray, previewLogoBox.Height, previewLogoBox.Height);
            previewImgList.Images.Add(RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.EXE, Main.SystemResourcePath));
            previewImgList.Images.Add(RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.EXE, Main.SystemResourcePath));

            foreach (Button btn in new Button[] { saveBtn, exitBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }

            appsBox.Items.AddRange(Main.AppsInfo.Select(x => x.LongName).ToArray());
            if (!string.IsNullOrWhiteSpace(selectedItem) && appsBox.Items.Contains(selectedItem))
                appsBox.SelectedItem = selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            fileTypes.MaxLength = short.MaxValue;

            addToShellBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath);
            rmFromShellBtn.Image = RESOURCE.SystemIconAsImage(RESOURCE.SystemIconKey.UAC, Main.SystemResourcePath);

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

            int value = INI.ReadInteger("Settings", "Window.Opacity");
            opacityNum.Value = value >= opacityNum.Minimum && value <= opacityNum.Maximum ? value : 95;

            value = INI.ReadInteger("Settings", "Window.FadeInDuration");
            fadeInNum.Maximum = opacityNum.Value;
            fadeInNum.Value = value >= fadeInNum.Minimum && value <= fadeInNum.Maximum ? value : 1;
            
            defBgCheck.Checked = !Directory.Exists(PATH.Combine("%CurDir%\\Assets\\cache\\bg"));
            if (bgLayout.Items.Count > 0)
                bgLayout.Items.Clear();
            for (int i = 0; i < 5; i++)
                bgLayout.Items.Add(Lang.GetText($"bgLayoutOption{i}"));

            value = INI.ReadInteger("Settings", "Window.BackgroundImageLayout", 1);
            bgLayout.SelectedIndex = value > 0 && value < bgLayout.Items.Count ? value : 1;

            mainColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Base"), Main.Colors.System);
            controlColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Control"), SystemColors.Window);
            controlTextColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ControlText"), SystemColors.WindowText);
            btnColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.Button"), SystemColors.ButtonFace);
            btnHoverColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ButtonHover"), ProfessionalColors.ButtonSelectedHighlight);
            btnTextColorPanel.BackColor = DRAWING.ColorFromHtml(INI.Read("Settings", "Window.Colors.ButtonText"), SystemColors.ControlText);

            hScrollBarCheck.Checked = INI.ReadBoolean("Settings", "Window.HideHScrollBar", false);

            StylePreviewUpdate();

            appDirs.Text = INI.Read("Settings", "AppDirs").DecodeStringFromBase64();

            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (int i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText($"startMenuIntegrationOption{i}"));
            startMenuIntegration.SelectedIndex = INI.ReadBoolean("Settings", "StartMenuIntegration", false) ? 1 : 0;

            if (defaultPos.Items.Count > 0)
                defaultPos.Items.Clear();
            for (int i = 0; i < 2; i++)
                defaultPos.Items.Add(Lang.GetText($"defaultPosOption{i}"));

            value = INI.ReadInteger("Settings", "Window.DefaultPosition", 0);
            defaultPos.SelectedIndex = value > 0 && value < defaultPos.Items.Count ? value : 0;

            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (int i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText($"updateCheckOption{i}"));
            value = INI.ReadInteger("Settings", "UpdateCheck", 4);
            if (value < 0)
                INI.Write("Settings", "UpdateCheck", 4);
            updateCheck.SelectedIndex = value > 0 && value < updateCheck.Items.Count ? value : 0;

            if (updateChannel.Items.Count > 0)
                updateChannel.Items.Clear();
            for (int i = 0; i < 2; i++)
                updateChannel.Items.Add(Lang.GetText($"updateChannelOption{i}"));
            value = INI.ReadInteger("Settings", "UpdateChannel", 0);
            updateChannel.SelectedIndex = value > 0 ? 1 : 0;

            string langsDir = PATH.Combine("%CurDir%\\Langs");
            if (Directory.Exists(langsDir))
            {
                foreach (string file in Directory.GetFiles(langsDir, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (!setLang.Items.Contains(name))
                        setLang.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            string lang = INI.ReadString("Settings", "Lang", Lang.SystemUI);
            if (!setLang.Items.Contains(lang))
                lang = "en-US";
            setLang.SelectedItem = lang;
        }

        private void ToolTipAtMouseEnter(object sender, EventArgs e) =>
            toolTip.SetToolTip((Control)sender, Lang.GetText($"{((Control)sender).Name}Tip"));

        private void exitBtn_Click(object sender, EventArgs e) =>
            Close();

        #endregion

        #region TAB PAGE 1

        private void appsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string SelectedApp = ((ComboBox)sender).SelectedItem.ToString();
            Main.AppInfo appInfo = Main.GetAppInfo(SelectedApp);
            if (appInfo.LongName != SelectedApp)
                return;

            fileTypes.Text = INI.Read(appInfo.ShortName, "FileTypes");

            string restPointDir = PATH.Combine("%CurDir%\\Restoration", Environment.MachineName, Main.WindowsInstallDateTime.ToString().EncryptToMD5().Substring(24), appInfo.ShortName, "FileAssociation");
            restoreFileTypesBtn.Enabled = Directory.Exists(restPointDir) && Directory.GetFiles(restPointDir, "*.ini", SearchOption.AllDirectories).Length > 0;
            restoreFileTypesBtn.Visible = restoreFileTypesBtn.Enabled;

            startArgsFirst.Text = INI.Read(appInfo.ShortName, "StartArgs.First");
            string argsDecode = startArgsFirst.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsFirst.Text = argsDecode;

            startArgsLast.Text = INI.Read(appInfo.ShortName, "StartArgs.Last");
            argsDecode = startArgsLast.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsLast.Text = argsDecode;

            noConfirmCheck.Checked = INI.ReadBoolean(appInfo.ShortName, "NoConfirm");
            runAsAdminCheck.Checked = INI.ReadBoolean(appInfo.ShortName, "RunAsAdmin");
            noUpdatesCheck.Checked = INI.ReadBoolean(appInfo.ShortName, "NoUpdates");
        }

        private void locationBtn_Click(object sender, EventArgs e) =>
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());

        private void fileTypesMenu_Paint(object sender, PaintEventArgs e) =>
            CONTEXTMENUSTRIP.SetFixedSingle((ContextMenuStrip)sender, e, Main.Colors.Base);

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
                    string appPath = Main.GetAppPath(appsBox.SelectedItem.ToString());
                    if (File.Exists(appPath))
                    {
                        string appDir = Path.GetDirectoryName(appPath);
                        string iniPath = Path.Combine(appDir, "App\\AppInfo\\appinfo.ini");
                        if (!File.Exists(iniPath))
                            iniPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appPath)}.ini");
                        if (File.Exists(iniPath))
                        {
                            string types = INI.Read("Associations", "FileTypes", iniPath);
                            if (!string.IsNullOrWhiteSpace(types))
                            {
                                fileTypes.Text = types.RemoveChar(' ');
                                return;
                            }
                        }
                    }
                    MSGBOX.Show(this, Lang.GetText("NoDefaultTypesFoundMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        private bool fileTypesConflict()
        {
            Main.AppInfo appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (appInfo.LongName != appsBox.SelectedItem.ToString())
                return false;

            Dictionary<string, List<string>> AlreadyDefined = new Dictionary<string, List<string>>();
            Main.AppConfigs = new List<string>();
            foreach (string section in Main.AppConfigs)
            {
                if (section == appInfo.ShortName)
                    continue;

                string types = INI.Read(section, "FileTypes");
                if (string.IsNullOrWhiteSpace(types))
                    continue;

                List<string> TextBoxTypes = fileTypes.Text.RemoveChar('*', '.').Split(',').ToList();
                List<string> ConfigTypes = types.RemoveChar('*', '.').Split(',').ToList();
                foreach (string type in TextBoxTypes)
                {
                    if (ConfigTypes.Contains(type))
                    {
                        if (!AlreadyDefined.ContainsKey(section))
                            AlreadyDefined.Add(section, new List<string> { type });
                        else
                        {
                            if (!AlreadyDefined[section].Contains(type))
                                AlreadyDefined[section].Add(type);
                        }
                    }
                }
            }

            if (AlreadyDefined.Count > 0)
            {
                string msg = string.Empty;
                string sep = new string('-', 75);
                foreach (var entry in AlreadyDefined)
                {
                    string appName = Main.AppsInfo.First(x => x.ShortName == entry.Key).LongName;
                    string types = entry.Value.ToArray().Sort().Join("; ");
                    msg = $"{msg}{sep}{Environment.NewLine}{appName}: {types}{Environment.NewLine}";
                }
                msg += sep;
                if (MSGBOX.Show(this, string.Format(Lang.GetText("associateConflictMsg"), msg), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
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
                MSGBOX.Show(this, Lang.GetText($"{((Control)sender).Name}Msg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string appName = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (string.IsNullOrWhiteSpace(appName) || fileTypesConflict())
            {
                MSGBOX.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (fileTypes.Text != INI.Read(appName, "FileTypes"))
                saveBtn_Click(saveBtn, EventArgs.Empty);

            Main.AssociateFileTypes(appName);
        }

        private void restoreFileTypesBtn_Click(object sender, EventArgs e)
        {
            Main.AppInfo appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (string.IsNullOrWhiteSpace(appInfo.ShortName))
            {
                MSGBOX.Show(this, Lang.GetText("OperationCanceledMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Main.RestoreFileTypes(appInfo.ShortName);
        }

        #endregion

        #region TAB PAGE 2

        private void opacityNum_ValueChanged(object sender, EventArgs e) =>
            fadeInNum.Maximum = ((NumericUpDown)sender).Value;

        private void setBgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, Multiselect = false })
            {
                string path = Path.Combine(Main.TmpDir, "bg");
                if (Directory.Exists(path))
                    dialog.InitialDirectory = path;
                ImageCodecInfo[] imageCodecs = ImageCodecInfo.GetImageEncoders();
                List<string> codecExtensions = new List<string>();
                for (int i = 0; i < imageCodecs.Length; i++)
                {
                    codecExtensions.Add(imageCodecs[i].FilenameExtension.ToLower());
                    dialog.Filter = string.Format("{0}{1}{2} ({3})|{3}", dialog.Filter, i > 0 ? "|" : string.Empty, imageCodecs[i].CodecName.Substring(8).Replace("Codec", "Files").Trim(), codecExtensions[codecExtensions.Count - 1]);
                }
                dialog.Filter = string.Format("{0}|Image Files ({1})|{1}", dialog.Filter, codecExtensions.Join(";"));
                dialog.FilterIndex = imageCodecs.Length + 1;
                dialog.ShowDialog();
                if (File.Exists(dialog.FileName))
                {
                    try
                    {
                        Image img = DRAWING.ImageFilter(Image.FromFile(dialog.FileName), SmoothingMode.HighQuality);
                        string ext = Path.GetExtension(dialog.FileName).ToLower();
                        string bgPath = PATH.Combine("%CurDir%\\Assets\\cache\\bg", $"image{ext}");
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
                        previewBg.BackgroundImage = DRAWING.ImageFilter(image, (int)Math.Round(image.Width * .65f) + 1, (int)Math.Round(image.Height * .65f) + 1, SmoothingMode.HighQuality);
                        if (!result)
                            result = true;
                        if (!saved)
                            saved = true;
                        MSGBOX.Show(this, Lang.GetText("OperationCompletedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                        MSGBOX.Show(this, Lang.GetText("OperationFailedMsg"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void defBgCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            string bgDir = PATH.Combine("%CurDir%\\Assets\\cache\\bg");
            try
            {
                Path.GetFullPath(bgDir);
                if (Directory.GetFiles(bgDir, "*", SearchOption.TopDirectoryOnly).Length == 0)
                    throw new FileNotFoundException();
                if (!cb.Checked)
                    previewBg.BackgroundImage = DRAWING.ImageFilter(Main.BackgroundImage, (int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1, SmoothingMode.HighQuality);
                else
                    previewBg.BackgroundImage = DRAWING.DimEmpty;
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
            controlColorPanel.BackColor = SystemColors.Window;
            controlTextColorPanel.BackColor = SystemColors.WindowText;
            btnColorPanel.BackColor = SystemColors.ButtonFace;
            btnHoverColorPanel.BackColor = ProfessionalColors.ButtonSelectedHighlight;
            btnTextColorPanel.BackColor = SystemColors.ControlText;
            if (!result)
                result = true;
            StylePreviewUpdate();
        }

        private void previewAppList_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            using (Graphics gr = e.Graphics)
            {
                gr.TranslateTransform((int)(p.Width / (Math.PI * 2)), p.Width + 40);
                gr.RotateTransform(-70);
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (Brush b = new SolidBrush(Color.FromArgb(50, (byte)~p.BackColor.R, (byte)~p.BackColor.G, (byte)~p.BackColor.B)))
                    gr.DrawString("Preview", new Font("Comic Sans MS", 24f), b, 0f, 0f);
            }
        }

        private void hScrollBarCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!result)
                result = true;
            StylePreviewUpdate();
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
            previewHScrollBar.Visible = !hScrollBarCheck.Checked;
        }

        #endregion

        #region TAB PAGE 3

        private void shellBtns_Click(object sender, EventArgs e) =>
            Main.SystemIntegration((Button)sender == addToShellBtn);

        private void shellBtns_TextChanged(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Text.Length < 22)
                b.TextAlign = ContentAlignment.MiddleCenter;
            else
                b.TextAlign = ContentAlignment.MiddleRight;
        }

        #endregion

        #region SAVE BUTTON

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string section = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (!string.IsNullOrWhiteSpace(section))
            {
                string types = string.Empty;
                if (!string.IsNullOrWhiteSpace(fileTypes.Text))
                {
                    if (e == EventArgs.Empty || !fileTypesConflict())
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
                            types = typesList.Join(",");
                            fileTypes.Text = types;
                        }
                    }
                    else
                        fileTypes.Text = INI.Read(section, "FileTypes");
                }
                INI.Write(section, "FileTypes", !string.IsNullOrWhiteSpace(types) ? types : null);

                INI.Write(section, "StartArgs.First", !string.IsNullOrWhiteSpace(startArgsFirst.Text) ? startArgsFirst.Text.EncodeToBase64() : null);
                INI.Write(section, "StartArgs.Last", !string.IsNullOrWhiteSpace(startArgsLast.Text) ? startArgsLast.Text.EncodeToBase64() : null);

                INI.Write(section, "NoConfirm", noConfirmCheck.Checked ? (bool?)true : null);
                INI.Write(section, "RunAsAdmin", runAsAdminCheck.Checked ? (bool?)true : null);
                INI.Write(section, "NoUpdates", noUpdatesCheck.Checked ? (bool?)true : null);

                if (INI.GetKeys(section).Count == 0)
                    INI.RemoveSection(section);
            }

            if (defBgCheck.Checked)
            {
                try
                {
                    string bgDir = PATH.Combine("%CurDir%\\Assets\\cache\\bg");
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
                    LOG.Debug(ex);
                }
            }

            INI.Write("Settings", "Window.Opacity", opacityNum.Value != 95 ? (int?)opacityNum.Value : null);

            INI.Write("Settings", "Window.FadeInDuration", fadeInNum.Value != 1 ? (int?)fadeInNum.Value : null);

            INI.Write("Settings", "Window.BackgroundImageLayout", bgLayout.SelectedIndex != 1 ? (int?)bgLayout.SelectedIndex : null);

            Color color = mainColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.Base", color != Main.Colors.System ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = controlColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.Control", color != SystemColors.Window ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = controlTextColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.ControlText", color != SystemColors.WindowText ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.Button", color != SystemColors.ButtonFace ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnHoverColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.ButtonHover", color != ProfessionalColors.ButtonSelectedHighlight ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            color = btnTextColorPanel.BackColor;
            INI.Write("Settings", "Window.Colors.ButtonText", color != SystemColors.ControlText ? $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}" : null);

            INI.Write("Settings", "Window.HideHScrollBar", hScrollBarCheck.Checked ? (bool?)hScrollBarCheck.Checked : null);

            string dirs = null;
            if (!string.IsNullOrWhiteSpace(appDirs.Text))
            {
                List<string> dirList = new List<string>();
                foreach (string item in $"{appDirs.Text}\r\n".Split(new string[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;
                    string dir = PATH.Combine(item);
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        dir = dir.Replace(PATH.GetEnvironmentVariableValue("CurDir"), "%CurDir%");
                        if (!dirList.Contains(dir))
                            dirList.Add(dir);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
                if (dirList.Count > 0)
                {
                    dirList.Sort();
                    dirs = dirList.Join(Environment.NewLine);
                    appDirs.Text = dirs;
                }
            }
            INI.Write("Settings", "AppDirs", !string.IsNullOrWhiteSpace(dirs) ? dirs.EncodeToBase64() : null);

            INI.Write("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex != 0 ? (bool?)true : null);
            if (startMenuIntegration.SelectedIndex == 0)
            {
                try
                {
                    string StartMenuFolderPath = PATH.Combine("%StartMenu%\\Programs");
#if x86
                    string LauncherShortcutPath = Path.Combine(StartMenuFolderPath, $"Apps Launcher.lnk");
#else
                    string LauncherShortcutPath = Path.Combine(StartMenuFolderPath, $"Apps Launcher (64-bit).lnk");
#endif
                    if (File.Exists(LauncherShortcutPath))
                        File.Delete(LauncherShortcutPath);
                    StartMenuFolderPath = Path.Combine(StartMenuFolderPath, "Portable Apps");
                    if (Directory.Exists(StartMenuFolderPath))
                        Directory.Delete(StartMenuFolderPath, true);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }

            INI.Write("Settings", "Window.DefaultPosition", defaultPos.SelectedIndex != 0 ? (int?)defaultPos.SelectedIndex : null);

            INI.Write("Settings", "UpdateCheck", updateCheck.SelectedIndex != 4 ? (int?)updateCheck.SelectedIndex : null);

            INI.Write("Settings", "UpdateChannel", updateChannel.SelectedIndex != 0 ? (int?)updateChannel.SelectedIndex : null);

            string lang = INI.ReadString("Settings", "Lang", Lang.SystemUI);
            if (lang != setLang.SelectedItem.ToString())
            {
                INI.Write("Settings", "Lang", setLang.SelectedItem.ToString() != Lang.SystemUI ? setLang.SelectedItem : null);
                if (!result)
                    result = true;
                LoadSettingsForm();
            }

            if (!saved)
                saved = true;
            MSGBOX.Show(this, Lang.GetText("SavedSettings"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        #endregion
    }
}
