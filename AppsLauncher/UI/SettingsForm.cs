namespace AppsLauncher.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using SilDev.QuickWmi;

    public partial class SettingsForm : Form
    {
        private readonly string _selectedItem;
        private int[] _customColors;
        private bool _result, _saved;

        public SettingsForm(string selectedItem)
        {
            InitializeComponent();
            _selectedItem = selectedItem;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (Main.ScreenDpi > 96)
                Font = SystemFonts.CaptionFont;
            Icon = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.SystemControl, Main.SystemResourcePath);

            foreach (TabPage tab in tabCtrl.TabPages)
                tab.BackColor = Main.Colors.BaseDark;

            locationBtn.BackgroundImage = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Directory, Main.SystemResourcePath)?.ToBitmap();
            fileTypesMenu.EnableAnimation();
            fileTypesMenu.SetFixedSingle();
            associateBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            try
            {
                restoreFileTypesBtn.Image = new Bitmap(28, 16);
                using (var g = Graphics.FromImage(restoreFileTypesBtn.Image))
                {
                    g.DrawImage(ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath).ToBitmap(), 0, 0);
                    g.DrawImage(ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Undo, Main.SystemResourcePath).ToBitmap(), 12, 0);
                }
            }
            catch
            {
                restoreFileTypesBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
                restoreFileTypesBtn.ImageAlign = ContentAlignment.MiddleLeft;
                restoreFileTypesBtn.Text = @"<=";
                if (restoreFileTypesBtn.Image != null)
                    restoreFileTypesBtn.TextAlign = ContentAlignment.MiddleRight;
            }

            previewBg.BackgroundImage = Main.BackgroundImage.Redraw((int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1);
            previewBg.BackgroundImageLayout = Main.BackgroundImageLayout;
            previewLogoBox.BackgroundImage = Resources.PortableApps_Logo_gray.Redraw(previewLogoBox.Height, previewLogoBox.Height);
            var exeIco = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.ExeFile, Main.SystemResourcePath);
            if (exeIco != null)
            {
                previewImgList.Images.Add(exeIco.ToBitmap());
                previewImgList.Images.Add(exeIco.ToBitmap());
            }

            foreach (var btn in new[] { saveBtn, exitBtn })
            {
                btn.BackColor = Main.Colors.Button;
                btn.ForeColor = Main.Colors.ButtonText;
                btn.FlatAppearance.MouseDownBackColor = Main.Colors.Button;
                btn.FlatAppearance.MouseOverBackColor = Main.Colors.ButtonHover;
            }
            var strAppNames = Main.AppsInfo.Select(x => x.LongName).ToArray();
            var objAppNames = new object[strAppNames.Length];
            Array.Copy(strAppNames, objAppNames, objAppNames.Length);
            appsBox.Items.AddRange(objAppNames);

            appsBox.SelectedItem = _selectedItem;
            if (appsBox.SelectedIndex < 0)
                appsBox.SelectedIndex = 0;

            fileTypes.MaxLength = short.MaxValue;
            addToShellBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();
            rmFromShellBtn.Image = ResourcesEx.GetSystemIcon(ResourcesEx.ImageresIconIndex.Uac, Main.SystemResourcePath)?.ToBitmap();

            LoadSettings();
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            var timer = new Timer
            {
                Interval = 1,
                Enabled = true
            };
            timer.Tick += (o, args) =>
            {
                if (Opacity < 1d)
                {
                    Opacity += .1d;
                    return;
                }
                timer.Dispose();
                if (TopMost)
                    TopMost = false;
            };
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) =>
            DialogResult = _result && _saved ? DialogResult.Yes : DialogResult.No;

        private void LoadSettings()
        {
            Lang.SetControlLang(this);

            var title = Lang.GetText("settingsBtn");
            if (!string.IsNullOrWhiteSpace(title))
                Text = title;

            for (var i = 0; i < fileTypesMenu.Items.Count; i++)
                fileTypesMenu.Items[i].Text = Lang.GetText(fileTypesMenu.Items[i].Name);

            Main.SetFont(this, false);
            Main.SetFont(tabPage1);
            foreach (Control c in tabPage2.Controls)
                if (c is CheckBox || c is ComboBox || c is Label)
                    Main.SetFont(c, false);
                else if (c is Panel)
                    Main.SetFont(c);
            Main.SetFont(tabPage3);

            var value = Ini.ReadInteger("Settings", "Window.Opacity");
            opacityNum.Value = value >= opacityNum.Minimum && value <= opacityNum.Maximum ? value : 95;

            value = Ini.ReadInteger("Settings", "Window.FadeInDuration");
            fadeInNum.Maximum = opacityNum.Value;
            fadeInNum.Value = value >= fadeInNum.Minimum && value <= fadeInNum.Maximum ? value : 1;

            defBgCheck.Checked = !Directory.Exists(Path.Combine(Main.TmpDir, "bg"));
            if (bgLayout.Items.Count > 0)
                bgLayout.Items.Clear();
            for (var i = 0; i < 5; i++)
                bgLayout.Items.Add(Lang.GetText($"{bgLayout.Name}Option{i}"));

            value = Ini.ReadInteger("Settings", "Window.BackgroundImageLayout", 1);
            bgLayout.SelectedIndex = value > 0 && value < bgLayout.Items.Count ? value : 1;

            _customColors = Ini.ReadStringArray("Settings", "Window.CustomColors")?.Select(int.Parse).ToArray();
            mainColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Base").FromHtmlToColor(Main.Colors.System);
            controlColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Control").FromHtmlToColor(SystemColors.Window);
            controlTextColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ControlText").FromHtmlToColor(SystemColors.WindowText);
            btnColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.Button").FromHtmlToColor(SystemColors.ButtonFace);
            btnHoverColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ButtonHover").FromHtmlToColor(ProfessionalColors.ButtonSelectedHighlight);
            btnTextColorPanel.BackColor = Ini.Read("Settings", "Window.Colors.ButtonText").FromHtmlToColor(SystemColors.ControlText);

            hScrollBarCheck.Checked = Ini.ReadBoolean("Settings", "Window.HideHScrollBar");

            StylePreviewUpdate();

            appDirs.Text = Ini.Read("Settings", "AppDirs").DecodeStringFromBase64();

            if (startMenuIntegration.Items.Count > 0)
                startMenuIntegration.Items.Clear();
            for (var i = 0; i < 2; i++)
                startMenuIntegration.Items.Add(Lang.GetText($"{startMenuIntegration.Name}Option{i}"));
            startMenuIntegration.SelectedIndex = Ini.ReadBoolean("Settings", "StartMenuIntegration") ? 1 : 0;

            if (defaultPos.Items.Count > 0)
                defaultPos.Items.Clear();
            for (var i = 0; i < 2; i++)
                defaultPos.Items.Add(Lang.GetText($"{defaultPos.Name}Option{i}"));

            value = Ini.ReadInteger("Settings", "Window.DefaultPosition");
            defaultPos.SelectedIndex = value > 0 && value < defaultPos.Items.Count ? value : 0;
            if (updateCheck.Items.Count > 0)
                updateCheck.Items.Clear();
            for (var i = 0; i < 10; i++)
                updateCheck.Items.Add(Lang.GetText($"{updateCheck.Name}Option{i}"));

            value = Ini.ReadInteger("Settings", "UpdateCheck", 4);
            if (value < 0)
                Ini.Write("Settings", "UpdateCheck", 4);
            updateCheck.SelectedIndex = value > 0 && value < updateCheck.Items.Count ? value : 0;
            if (updateChannel.Items.Count > 0)
                updateChannel.Items.Clear();
            for (var i = 0; i < 2; i++)
                updateChannel.Items.Add(Lang.GetText($"{updateChannel.Name}Option{i}"));

            value = Ini.ReadInteger("Settings", "UpdateChannel");
            updateChannel.SelectedIndex = value > 0 ? 1 : 0;

            /*
            var langsDir = PathEx.Combine("%CurDir%\\Langs");
            if (Directory.Exists(langsDir))
                foreach (var file in Directory.GetFiles(langsDir, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(name) || setLang.Items.Contains(name))
                        continue;
                    var ext = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrEmpty(ext))
                        setLang.Items.Add(ext);
                }
            */
            var lang = Ini.ReadString("Settings", "Lang", Lang.SystemUi);
            if (!setLang.Items.Contains(lang))
                lang = "en-US";
            setLang.SelectedItem = lang;

            if (!saveBtn.Focused)
                saveBtn.Select();
        }

        private void ToolTipAtMouseEnter(object sender, EventArgs e)
        {
            var owner = sender as Control;
            if (owner != null)
                toolTip.SetToolTip(owner, Lang.GetText($"{owner.Name}Tip"));
        }

        private void ExitBtn_Click(object sender, EventArgs e) =>
            Close();

        private void AppsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedApp = (sender as ComboBox)?.SelectedItem?.ToString();
            var appInfo = Main.GetAppInfo(selectedApp);
            if (appInfo.LongName != selectedApp)
                return;
            fileTypes.Text = Ini.Read(appInfo.ShortName, "FileTypes");
            var restPointDir = PathEx.Combine("%CurDir%\\Restoration", Environment.MachineName, Win32_OperatingSystem.InstallDate?.ToString("F").EncryptToMd5().Substring(24), appInfo.ShortName, "FileAssociation");
            restoreFileTypesBtn.Enabled = Directory.Exists(restPointDir) && Directory.GetFiles(restPointDir, "*.ini", SearchOption.AllDirectories).Length > 0;
            restoreFileTypesBtn.Visible = restoreFileTypesBtn.Enabled;
            startArgsFirst.Text = Ini.Read(appInfo.ShortName, "StartArgs.First");
            var argsDecode = startArgsFirst.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsFirst.Text = argsDecode;
            startArgsLast.Text = Ini.Read(appInfo.ShortName, "StartArgs.Last");
            argsDecode = startArgsLast.Text.DecodeStringFromBase64();
            if (!string.IsNullOrEmpty(argsDecode))
                startArgsLast.Text = argsDecode;
            noConfirmCheck.Checked = Ini.ReadBoolean(appInfo.ShortName, "NoConfirm");
            runAsAdminCheck.Checked = Ini.ReadBoolean(appInfo.ShortName, "RunAsAdmin");
            noUpdatesCheck.Checked = Ini.ReadBoolean(appInfo.ShortName, "NoUpdates");
        }

        private void LocationBtn_Click(object sender, EventArgs e) =>
            Main.OpenAppLocation(appsBox.SelectedItem.ToString());

        private void FileTypesMenu_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripMenuItem)?.Name)
            {
                case "fileTypesMenuItem1":
                    if (!string.IsNullOrEmpty(fileTypes.SelectedText))
                        Clipboard.SetText(fileTypes.SelectedText);
                    break;
                case "fileTypesMenuItem2":
                    if (Clipboard.ContainsText())
                        if (string.IsNullOrEmpty(fileTypes.SelectedText))
                        {
                            var start = fileTypes.SelectionStart;
                            fileTypes.Text = fileTypes.Text.Insert(start, Clipboard.GetText());
                            fileTypes.SelectionStart = start + Clipboard.GetText().Length;
                        }
                        else
                            fileTypes.SelectedText = Clipboard.GetText();
                    break;
                case "fileTypesMenuItem3":
                    var appPath = Main.GetAppPath(appsBox.SelectedItem.ToString());
                    if (File.Exists(appPath))
                    {
                        var appDir = Path.GetDirectoryName(appPath);
                        if (!string.IsNullOrEmpty(appDir))
                        {
                            var iniPath = Path.Combine(appDir, "App\\AppInfo\\appinfo.ini");
                            if (!File.Exists(iniPath))
                                iniPath = Path.Combine(appDir, $"{Path.GetFileNameWithoutExtension(appPath)}.ini");
                            if (File.Exists(iniPath))
                            {
                                var types = Ini.Read("Associations", "FileTypes", iniPath);
                                if (!string.IsNullOrWhiteSpace(types))
                                {
                                    fileTypes.Text = types.RemoveChar(' ');
                                    return;
                                }
                            }
                        }
                    }
                    MessageBoxEx.Show(this, Lang.GetText("NoDefaultTypesFoundMsg"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    break;
            }
        }

        private bool FileTypesConflict()
        {
            var appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (appInfo.LongName != appsBox.SelectedItem.ToString())
                return false;
            var alreadyDefined = new Dictionary<string, List<string>>();
            Main.AppConfigs = new List<string>();
            foreach (var section in Main.AppConfigs)
            {
                if (section == appInfo.ShortName)
                    continue;
                var types = Ini.Read(section, "FileTypes");
                if (string.IsNullOrWhiteSpace(types))
                    continue;
                var textBoxTypes = fileTypes.Text.RemoveChar('*', '.').Split(',').ToList();
                var configTypes = types.RemoveChar('*', '.').Split(',').ToList();
                foreach (var type in textBoxTypes)
                    if (configTypes.ContainsEx(type))
                        if (!alreadyDefined.ContainsKey(section))
                            alreadyDefined.Add(section, new List<string> { type });
                        else
                        {
                            if (!alreadyDefined[section].ContainsEx(type))
                                alreadyDefined[section].Add(type);
                        }
            }
            if (alreadyDefined.Count <= 0)
                return false;
            var msg = string.Empty;
            var sep = new string('-', 75);
            foreach (var entry in alreadyDefined)
            {
                string appName;
                try
                {
                    appName = Main.AppsInfo.First(x => x.ShortName == entry.Key).LongName;
                }
                catch
                {
                    Ini.RemoveSection(entry.Key);
                    continue;
                }
                var types = entry.Value.ToArray().Sort().Join("; ");
                msg = $"{msg}{sep}{Environment.NewLine}{appName}: {types}{Environment.NewLine}";
            }
            if (string.IsNullOrEmpty(msg))
                return false;
            msg += sep;
            return MessageBoxEx.Show(this, string.Format(Lang.GetText("associateConflictMsg"), msg), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
        }

        private void AssociateBtn_Click(object sender, EventArgs e)
        {
            var owner = sender as Control;
            if (owner == null)
                return;
            var isNull = string.IsNullOrWhiteSpace(fileTypes.Text);
            if (!isNull)
                if (fileTypes.Text.Contains(","))
                    isNull = fileTypes.Text.Split(',').Where(s => !s.StartsWith(".")).ToArray().Length == 0;
                else
                    isNull = fileTypes.Text.StartsWith(".");
            if (isNull)
            {
                MessageBoxEx.Show(this, Lang.GetText($"{owner.Name}Msg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var appName = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (string.IsNullOrWhiteSpace(appName) || FileTypesConflict())
            {
                MessageBoxEx.Show(this, Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (fileTypes.Text != Ini.Read(appName, "FileTypes"))
                SaveBtn_Click(saveBtn, EventArgs.Empty);
            Main.AssociateFileTypes(appName, this);
        }

        private void RestoreFileTypesBtn_Click(object sender, EventArgs e)
        {
            var appInfo = Main.GetAppInfo(appsBox.SelectedItem.ToString());
            if (string.IsNullOrWhiteSpace(appInfo.ShortName))
            {
                MessageBoxEx.Show(this, Lang.GetText("OperationCanceledMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Main.RestoreFileTypes(appInfo.ShortName);
        }

        private void OpacityNum_ValueChanged(object sender, EventArgs e)
        {
            var owner = sender as NumericUpDown;
            if (owner != null)
                fadeInNum.Maximum = owner.Value;
        }

        private void SetBgBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { CheckFileExists = true, CheckPathExists = true, Multiselect = false })
            {
                var path = PathEx.Combine("%CurDir%\\Assets\\bg");
                if (Directory.Exists(path))
                    dialog.InitialDirectory = path;
                var imgCodecs = ImageCodecInfo.GetImageEncoders();
                var codecExts = new List<string>();
                for (var i = 0; i < imgCodecs.Length; i++)
                {
                    codecExts.Add(imgCodecs[i].FilenameExtension.ToLower());
                    dialog.Filter = string.Format("{0}{1}{2} ({3})|{3}", dialog.Filter, i > 0 ? "|" : string.Empty, imgCodecs[i].CodecName.Substring(8).Replace("Codec", "Files").Trim(), codecExts[codecExts.Count - 1]);
                }
                dialog.Filter = string.Format("{0}|Image Files ({1})|{1}", dialog.Filter, codecExts.Join(";"));
                dialog.FilterIndex = imgCodecs.Length + 1;
                dialog.ShowDialog();
                if (!File.Exists(dialog.FileName))
                    return;
                try
                {
                    var img = Image.FromFile(dialog.FileName).Redraw(SmoothingMode.HighQuality, 3840);
                    var ext = Path.GetExtension(dialog.FileName).ToLower();
                    var bgDir = Path.Combine(Main.TmpDir, "bg");
                    var bgPath = PathEx.Combine(bgDir, $"image{ext}");
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
                    var image = Image.FromStream(new MemoryStream(File.ReadAllBytes(bgPath)));
                    previewBg.BackgroundImage = image.Redraw((int)Math.Round(image.Width * .65f) + 1, (int)Math.Round(image.Height * .65f) + 1);
                    if (!_result)
                        _result = true;
                    if (!_saved)
                        _saved = true;
                    MessageBoxEx.Show(this, Lang.GetText("OperationCompletedMsg"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    MessageBoxEx.Show(this, Lang.GetText("OperationFailedMsg"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DefBgCheck_CheckedChanged(object sender, EventArgs e)
        {
            var owner = sender as CheckBox;
            if (owner == null)
                return;
            var bgDir = Path.Combine(Main.TmpDir, "bg");
            try
            {
                var dir = Path.GetFullPath(bgDir);
                if (Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).Length == 0)
                    throw new PathNotFoundException(dir + "\\*");
                previewBg.BackgroundImage = !owner.Checked ? Main.BackgroundImage.Redraw((int)Math.Round(Main.BackgroundImage.Width * .65f) + 1, (int)Math.Round(Main.BackgroundImage.Height * .65f) + 1) : Depiction.DimEmpty;
            }
            catch
            {
                if (!owner.Checked)
                    owner.Checked = !owner.Checked;
            }
        }

        private void BgLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_result)
                _result = true;
            StylePreviewUpdate();
        }

        private void ColorPanel_MouseEnter(object sender, EventArgs e)
        {
            var owner = sender as Panel;
            if (owner != null)
                owner.BackColor = Color.FromArgb(128, owner.BackColor.R, owner.BackColor.G, owner.BackColor.B);
        }

        private void ColorPanel_MouseLeave(object sender, EventArgs e)
        {
            var owner = sender as Panel;
            if (owner != null)
                owner.BackColor = Color.FromArgb(owner.BackColor.R, owner.BackColor.G, owner.BackColor.B);
        }

        private void ColorPanel_Click(object sender, EventArgs e)
        {
            var owner = sender as Panel;
            if (owner == null)
                return;
            string title = null;
            try
            {
                title = Controls.Find(owner.Name + "Label", true).First().Text;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            using (var dialog = new ColorDialogEx(this, title)
            {
                AllowFullOpen = true,
                AnyColor = true,
                SolidColorOnly = true,
                Color = owner.BackColor,
                FullOpen = true
            })
            {
                if (_customColors?.Length > 0)
                    dialog.CustomColors = _customColors?.ToArray();
                if (dialog.ShowDialog() != DialogResult.Cancel)
                {
                    if (dialog.Color != owner.BackColor)
                        owner.BackColor = Color.FromArgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
                    if (_customColors?.ToArray() != dialog.CustomColors)
                        _customColors = dialog.CustomColors;
                }
            }
            if (!_result)
                _result = true;
            StylePreviewUpdate();
        }

        private void ResetColorsBtn_Click(object sender, EventArgs e)
        {
            mainColorPanel.BackColor = Main.Colors.System;
            controlColorPanel.BackColor = SystemColors.Window;
            controlTextColorPanel.BackColor = SystemColors.WindowText;
            btnColorPanel.BackColor = SystemColors.ButtonFace;
            btnHoverColorPanel.BackColor = ProfessionalColors.ButtonSelectedHighlight;
            btnTextColorPanel.BackColor = SystemColors.ControlText;
            if (!_result)
                _result = true;
            StylePreviewUpdate();
        }

        private void PreviewAppList_Paint(object sender, PaintEventArgs e)
        {
            var owner = sender as Panel;
            if (owner == null)
                return;
            using (var gr = e.Graphics)
            {
                gr.TranslateTransform((int)(owner.Width / (Math.PI * 2)), owner.Width + 40);
                gr.RotateTransform(-70);
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (Brush b = new SolidBrush(Color.FromArgb(50, (byte)~owner.BackColor.R, (byte)~owner.BackColor.G, (byte)~owner.BackColor.B)))
                    gr.DrawString("Preview", new Font("Comic Sans MS", 24f), b, 0f, 0f);
            }
        }

        private void ScrollBarCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!_result)
                _result = true;
            StylePreviewUpdate();
        }

        private void StylePreviewUpdate()
        {
            previewBg.BackgroundImageLayout = (ImageLayout)bgLayout.SelectedIndex;
            previewMainColor.BackColor = mainColorPanel.BackColor;
            previewAppList.ForeColor = controlTextColorPanel.BackColor;
            previewAppList.BackColor = controlColorPanel.BackColor;
            previewAppListPanel.BackColor = controlColorPanel.BackColor;
            foreach (var b in new[] { previewBtn1, previewBtn2 })
            {
                b.ForeColor = btnTextColorPanel.BackColor;
                b.BackColor = btnColorPanel.BackColor;
                b.FlatAppearance.MouseOverBackColor = btnHoverColorPanel.BackColor;
            }
            previewHScrollBar.Visible = !hScrollBarCheck.Checked;
        }

        private void ShellBtns_Click(object sender, EventArgs e) =>
            Main.SystemIntegration((Button)sender == addToShellBtn);

        private void ShellBtns_TextChanged(object sender, EventArgs e)
        {
            var owner = sender as Button;
            if (owner != null)
                owner.TextAlign = owner.Text.Length < 22 ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleRight;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var section = Main.GetAppInfo(appsBox.SelectedItem.ToString()).ShortName;
            if (!string.IsNullOrWhiteSpace(section))
            {
                var types = string.Empty;
                if (!string.IsNullOrWhiteSpace(fileTypes.Text))
                    if (e == EventArgs.Empty || !FileTypesConflict())
                    {
                        var typesList = new List<string>();
                        foreach (var item in $"{fileTypes.Text},".Split(','))
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            var type = new string(item.ToCharArray().Where(c => !Path.GetInvalidFileNameChars().Contains(c) && !char.IsWhiteSpace(c)).ToArray());
                            if (string.IsNullOrWhiteSpace(type) || type.Length < 1)
                                continue;
                            if (type.StartsWith("."))
                            {
                                while (type.Contains(".."))
                                    type = type.Replace("..", ".");
                                if (typesList.ContainsEx(type) || typesList.ContainsEx(type.Substring(1)))
                                    continue;
                            }
                            else
                            {
                                if (typesList.ContainsEx(type) || typesList.ContainsEx($".{type}"))
                                    continue;
                            }
                            if (type.Length == 1 && type.StartsWith("."))
                                continue;
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
                        fileTypes.Text = Ini.Read(section, "FileTypes");

                Ini.Write(section, "FileTypes", !string.IsNullOrWhiteSpace(types) ? types : null);

                Ini.Write(section, "StartArgs.First", !string.IsNullOrWhiteSpace(startArgsFirst.Text) ? startArgsFirst.Text.EncodeToBase64() : null);
                Ini.Write(section, "StartArgs.Last", !string.IsNullOrWhiteSpace(startArgsLast.Text) ? startArgsLast.Text.EncodeToBase64() : null);

                Ini.Write(section, "NoConfirm", noConfirmCheck.Checked ? (bool?)true : null);
                Ini.Write(section, "RunAsAdmin", runAsAdminCheck.Checked ? (bool?)true : null);
                Ini.Write(section, "NoUpdates", noUpdatesCheck.Checked ? (bool?)true : null);

                if (Ini.GetKeys(section).Count == 0)
                    Ini.RemoveSection(section);
            }

            if (defBgCheck.Checked)
                try
                {
                    var bgDir = Path.Combine(Main.TmpDir, "bg");
                    if (Directory.Exists(bgDir))
                    {
                        Directory.Delete(bgDir, true);
                        if (!_result)
                            _result = true;
                    }
                    Main.ResetBackgroundImage();
                    bgLayout.SelectedIndex = 1;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            Ini.Write("Settings", "Window.Opacity", opacityNum.Value != 95 ? (int?)opacityNum.Value : null);
            Ini.Write("Settings", "Window.FadeInDuration", fadeInNum.Value != 1 ? (int?)fadeInNum.Value : null);
            Ini.Write("Settings", "Window.BackgroundImageLayout", bgLayout.SelectedIndex != 1 ? (int?)bgLayout.SelectedIndex : null);

            Ini.Write("Settings", "Window.CustomColors", _customColors?.Length > 0 ? _customColors?.Select(x => x.ToString())?.ToArray() : null);
            var color = mainColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Base", color != Main.Colors.System ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = controlColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Control", color != SystemColors.Window ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = controlTextColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ControlText", color != SystemColors.WindowText ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.Button", color != SystemColors.ButtonFace ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnHoverColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ButtonHover", color != ProfessionalColors.ButtonSelectedHighlight ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);
            color = btnTextColorPanel.BackColor;
            Ini.Write("Settings", "Window.Colors.ButtonText", color != SystemColors.ControlText ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : null);

            Ini.Write("Settings", "Window.HideHScrollBar", hScrollBarCheck.Checked ? (bool?)hScrollBarCheck.Checked : null);

            string dirs = null;
            if (!string.IsNullOrWhiteSpace(appDirs.Text))
            {
                var dirList = new List<string>();
                foreach (var item in $"{appDirs.Text}\r\n".Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;
                    var dir = PathEx.Combine(item);
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        dir = dir.Replace(PathEx.LocalDir, "%CurDir%");
                        if (!dirList.ContainsEx(dir))
                            dirList.Add(dir);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
                if (dirList.Count > 0)
                {
                    dirList.Sort();
                    dirs = dirList.Join(Environment.NewLine);
                    appDirs.Text = dirs;
                }
            }

            Ini.Write("Settings", "AppDirs", !string.IsNullOrWhiteSpace(dirs) ? dirs.EncodeToBase64() : null);

            Ini.Write("Settings", "StartMenuIntegration", startMenuIntegration.SelectedIndex != 0 ? (bool?)true : null);
            if (startMenuIntegration.SelectedIndex == 0)
                try
                {
                    var startMenuFolderPath = PathEx.Combine("%StartMenu%\\Programs");
#if x86
                    var launcherShortcutPath = Path.Combine(startMenuFolderPath, $"Apps Launcher.lnk");
#else
                    var launcherShortcutPath = Path.Combine(startMenuFolderPath, "Apps Launcher (64-bit).lnk");
#endif
                    if (File.Exists(launcherShortcutPath))
                        File.Delete(launcherShortcutPath);
                    startMenuFolderPath = Path.Combine(startMenuFolderPath, "Portable Apps");
                    if (Directory.Exists(startMenuFolderPath))
                        Directory.Delete(startMenuFolderPath, true);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

            Ini.Write("Settings", "Window.DefaultPosition", defaultPos.SelectedIndex != 0 ? (int?)defaultPos.SelectedIndex : null);

            Ini.Write("Settings", "UpdateCheck", updateCheck.SelectedIndex != 4 ? (int?)updateCheck.SelectedIndex : null);
            Ini.Write("Settings", "UpdateChannel", updateChannel.SelectedIndex != 0 ? (int?)updateChannel.SelectedIndex : null);

            var lang = Ini.ReadString("Settings", "Lang", Lang.SystemUi);
            if (lang != setLang.SelectedItem.ToString())
            {
                Ini.Write("Settings", "Lang", setLang.SelectedItem.ToString() != Lang.SystemUi ? setLang.SelectedItem : null);
                if (!_result)
                    _result = true;
                LoadSettings();
            }

            if (!_saved)
                _saved = true;
            MessageBoxEx.Show(this, Lang.GetText("SavedSettings"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
