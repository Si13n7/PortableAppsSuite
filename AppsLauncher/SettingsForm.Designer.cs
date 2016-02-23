namespace AppsLauncher
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.saveBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.undoAssociationBtn = new System.Windows.Forms.Button();
            this.runAsAdminCheck = new System.Windows.Forms.CheckBox();
            this.noUpdatesCheck = new System.Windows.Forms.CheckBox();
            this.fileTypes = new System.Windows.Forms.RichTextBox();
            this.fileTypesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fileTypesMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTypesMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.fileTypesMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.locationBtn = new System.Windows.Forms.Button();
            this.appsBox = new System.Windows.Forms.ComboBox();
            this.associateBtn = new System.Windows.Forms.Button();
            this.fileTypesLabel = new System.Windows.Forms.Label();
            this.appsBoxLabel = new System.Windows.Forms.Label();
            this.clLabel = new System.Windows.Forms.Label();
            this.endArg = new System.Windows.Forms.TextBox();
            this.addArgsLabel = new System.Windows.Forms.Label();
            this.startArg = new System.Windows.Forms.TextBox();
            this.noConfirmCheck = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.resetColorsBtn = new System.Windows.Forms.Button();
            this.previewMainColor = new System.Windows.Forms.Panel();
            this.previewBg = new System.Windows.Forms.Panel();
            this.previewLogoBox = new System.Windows.Forms.PictureBox();
            this.previewBtn1 = new System.Windows.Forms.Button();
            this.previewBtn2 = new System.Windows.Forms.Button();
            this.previewAppList = new System.Windows.Forms.Panel();
            this.defBgCheck = new System.Windows.Forms.CheckBox();
            this.btnColorPanel = new System.Windows.Forms.Panel();
            this.setBgBtn = new System.Windows.Forms.Button();
            this.btnColorPanelLabel = new System.Windows.Forms.Label();
            this.fadeInNumLabel = new System.Windows.Forms.Label();
            this.opacityNum = new System.Windows.Forms.NumericUpDown();
            this.fadeInNum = new System.Windows.Forms.NumericUpDown();
            this.btnTextColorPanel = new System.Windows.Forms.Panel();
            this.opacityNumLabel = new System.Windows.Forms.Label();
            this.mainColorPanelLabel = new System.Windows.Forms.Label();
            this.btnHoverColorPanel = new System.Windows.Forms.Panel();
            this.btnHoverColorPanelLabel = new System.Windows.Forms.Label();
            this.mainColorPanel = new System.Windows.Forms.Panel();
            this.btnTextColorPanelLabel = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.defaultPos = new System.Windows.Forms.ComboBox();
            this.defaultPosLabel = new System.Windows.Forms.Label();
            this.startMenuIntegration = new System.Windows.Forms.ComboBox();
            this.startMenuIntegrationLabel = new System.Windows.Forms.Label();
            this.addToShellBtn = new System.Windows.Forms.Button();
            this.rmFromShellBtn = new System.Windows.Forms.Button();
            this.setLang = new System.Windows.Forms.ComboBox();
            this.setLangLabel = new System.Windows.Forms.Label();
            this.appDirs = new System.Windows.Forms.RichTextBox();
            this.updateCheck = new System.Windows.Forms.ComboBox();
            this.updateCheckLabel = new System.Windows.Forms.Label();
            this.appDirsLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabCtrl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.fileTypesMenu.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.previewMainColor.SuspendLayout();
            this.previewBg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewLogoBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeInNum)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveBtn
            // 
            this.saveBtn.BackColor = System.Drawing.SystemColors.Control;
            this.saveBtn.FlatAppearance.BorderSize = 0;
            this.saveBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.saveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveBtn.Location = new System.Drawing.Point(273, 23);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(89, 24);
            this.saveBtn.TabIndex = 100;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = false;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.BackColor = System.Drawing.SystemColors.Control;
            this.cancelBtn.FlatAppearance.BorderSize = 0;
            this.cancelBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelBtn.Location = new System.Drawing.Point(377, 23);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(89, 24);
            this.cancelBtn.TabIndex = 101;
            this.cancelBtn.Text = "Exit";
            this.cancelBtn.UseVisualStyleBackColor = false;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.cancelBtn);
            this.panel1.Controls.Add(this.saveBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 374);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(492, 68);
            this.panel1.TabIndex = 11;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::AppsLauncher.Properties.Resources.PortableApps_Logo_gray;
            this.pictureBox1.Location = new System.Drawing.Point(2, -37);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 373);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(492, 1);
            this.panel2.TabIndex = 12;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.ShowAlways = true;
            // 
            // tabCtrl
            // 
            this.tabCtrl.Controls.Add(this.tabPage1);
            this.tabCtrl.Controls.Add(this.tabPage2);
            this.tabCtrl.Controls.Add(this.tabPage3);
            this.tabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrl.Location = new System.Drawing.Point(0, 0);
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(492, 373);
            this.tabCtrl.TabIndex = 13;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Highlight;
            this.tabPage1.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage1.Controls.Add(this.undoAssociationBtn);
            this.tabPage1.Controls.Add(this.runAsAdminCheck);
            this.tabPage1.Controls.Add(this.noUpdatesCheck);
            this.tabPage1.Controls.Add(this.fileTypes);
            this.tabPage1.Controls.Add(this.locationBtn);
            this.tabPage1.Controls.Add(this.appsBox);
            this.tabPage1.Controls.Add(this.associateBtn);
            this.tabPage1.Controls.Add(this.fileTypesLabel);
            this.tabPage1.Controls.Add(this.appsBoxLabel);
            this.tabPage1.Controls.Add(this.clLabel);
            this.tabPage1.Controls.Add(this.endArg);
            this.tabPage1.Controls.Add(this.addArgsLabel);
            this.tabPage1.Controls.Add(this.startArg);
            this.tabPage1.Controls.Add(this.noConfirmCheck);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(484, 347);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "App Options";
            // 
            // undoAssociationBtn
            // 
            this.undoAssociationBtn.Enabled = false;
            this.undoAssociationBtn.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.undoAssociationBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.undoAssociationBtn.Image = global::AppsLauncher.Properties.Resources.undo_uac_16;
            this.undoAssociationBtn.Location = new System.Drawing.Point(249, 185);
            this.undoAssociationBtn.Name = "undoAssociationBtn";
            this.undoAssociationBtn.Size = new System.Drawing.Size(44, 24);
            this.undoAssociationBtn.TabIndex = 4;
            this.undoAssociationBtn.UseVisualStyleBackColor = true;
            this.undoAssociationBtn.Visible = false;
            this.undoAssociationBtn.Click += new System.EventHandler(this.undoAssociationBtn_Click);
            this.undoAssociationBtn.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // runAsAdminCheck
            // 
            this.runAsAdminCheck.AutoSize = true;
            this.runAsAdminCheck.BackColor = System.Drawing.Color.Transparent;
            this.runAsAdminCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runAsAdminCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.runAsAdminCheck.Location = new System.Drawing.Point(142, 289);
            this.runAsAdminCheck.Name = "runAsAdminCheck";
            this.runAsAdminCheck.Size = new System.Drawing.Size(259, 17);
            this.runAsAdminCheck.TabIndex = 9;
            this.runAsAdminCheck.Text = "Run this app always with administrator privileges";
            this.runAsAdminCheck.UseVisualStyleBackColor = false;
            // 
            // noUpdatesCheck
            // 
            this.noUpdatesCheck.AutoSize = true;
            this.noUpdatesCheck.BackColor = System.Drawing.Color.Transparent;
            this.noUpdatesCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noUpdatesCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.noUpdatesCheck.Location = new System.Drawing.Point(142, 312);
            this.noUpdatesCheck.Name = "noUpdatesCheck";
            this.noUpdatesCheck.Size = new System.Drawing.Size(190, 17);
            this.noUpdatesCheck.TabIndex = 10;
            this.noUpdatesCheck.Text = "Never search updates for this app";
            this.noUpdatesCheck.UseVisualStyleBackColor = false;
            // 
            // fileTypes
            // 
            this.fileTypes.ContextMenuStrip = this.fileTypesMenu;
            this.fileTypes.Location = new System.Drawing.Point(131, 51);
            this.fileTypes.Name = "fileTypes";
            this.fileTypes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.fileTypes.Size = new System.Drawing.Size(302, 128);
            this.fileTypes.TabIndex = 3;
            this.fileTypes.Text = "";
            this.fileTypes.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // fileTypesMenu
            // 
            this.fileTypesMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.fileTypesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileTypesMenuItem1,
            this.fileTypesMenuItem2,
            this.toolStripSeparator1,
            this.fileTypesMenuItem3});
            this.fileTypesMenu.Name = "fileTypesMenu";
            this.fileTypesMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.fileTypesMenu.Size = new System.Drawing.Size(142, 76);
            // 
            // fileTypesMenuItem1
            // 
            this.fileTypesMenuItem1.ForeColor = System.Drawing.Color.Silver;
            this.fileTypesMenuItem1.Name = "fileTypesMenuItem1";
            this.fileTypesMenuItem1.Size = new System.Drawing.Size(141, 22);
            this.fileTypesMenuItem1.Text = "Copy";
            this.fileTypesMenuItem1.Click += new System.EventHandler(this.fileTypesMenu_Click);
            // 
            // fileTypesMenuItem2
            // 
            this.fileTypesMenuItem2.ForeColor = System.Drawing.Color.Silver;
            this.fileTypesMenuItem2.Name = "fileTypesMenuItem2";
            this.fileTypesMenuItem2.Size = new System.Drawing.Size(141, 22);
            this.fileTypesMenuItem2.Text = "Paste";
            this.fileTypesMenuItem2.Click += new System.EventHandler(this.fileTypesMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(138, 6);
            // 
            // fileTypesMenuItem3
            // 
            this.fileTypesMenuItem3.ForeColor = System.Drawing.Color.Silver;
            this.fileTypesMenuItem3.Name = "fileTypesMenuItem3";
            this.fileTypesMenuItem3.Size = new System.Drawing.Size(141, 22);
            this.fileTypesMenuItem3.Text = "Load Default";
            this.fileTypesMenuItem3.Click += new System.EventHandler(this.fileTypesMenu_Click);
            // 
            // locationBtn
            // 
            this.locationBtn.BackgroundImage = global::AppsLauncher.Properties.Resources.folder_16;
            this.locationBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.locationBtn.Location = new System.Drawing.Point(392, 16);
            this.locationBtn.Name = "locationBtn";
            this.locationBtn.Size = new System.Drawing.Size(24, 24);
            this.locationBtn.TabIndex = 2;
            this.locationBtn.UseVisualStyleBackColor = true;
            this.locationBtn.Click += new System.EventHandler(this.locationBtn_Click);
            // 
            // appsBox
            // 
            this.appsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.appsBox.FormattingEnabled = true;
            this.appsBox.Location = new System.Drawing.Point(131, 17);
            this.appsBox.Name = "appsBox";
            this.appsBox.Size = new System.Drawing.Size(254, 21);
            this.appsBox.Sorted = true;
            this.appsBox.TabIndex = 1;
            this.appsBox.SelectedIndexChanged += new System.EventHandler(this.appsBox_SelectedIndexChanged);
            // 
            // associateBtn
            // 
            this.associateBtn.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.associateBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.associateBtn.Image = global::AppsLauncher.Properties.Resources.uac_16;
            this.associateBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.associateBtn.Location = new System.Drawing.Point(299, 185);
            this.associateBtn.Name = "associateBtn";
            this.associateBtn.Size = new System.Drawing.Size(135, 24);
            this.associateBtn.TabIndex = 5;
            this.associateBtn.Text = "Associate File Types";
            this.associateBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.associateBtn.UseVisualStyleBackColor = true;
            this.associateBtn.Click += new System.EventHandler(this.associateBtn_Click);
            // 
            // fileTypesLabel
            // 
            this.fileTypesLabel.BackColor = System.Drawing.Color.Transparent;
            this.fileTypesLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileTypesLabel.ForeColor = System.Drawing.Color.Silver;
            this.fileTypesLabel.Location = new System.Drawing.Point(2, 54);
            this.fileTypesLabel.Name = "fileTypesLabel";
            this.fileTypesLabel.Size = new System.Drawing.Size(126, 13);
            this.fileTypesLabel.TabIndex = 2;
            this.fileTypesLabel.Text = "File Types:";
            this.fileTypesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // appsBoxLabel
            // 
            this.appsBoxLabel.BackColor = System.Drawing.Color.Transparent;
            this.appsBoxLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsBoxLabel.ForeColor = System.Drawing.Color.Silver;
            this.appsBoxLabel.Location = new System.Drawing.Point(2, 20);
            this.appsBoxLabel.Name = "appsBoxLabel";
            this.appsBoxLabel.Size = new System.Drawing.Size(126, 13);
            this.appsBoxLabel.TabIndex = 1;
            this.appsBoxLabel.Text = "Application:";
            this.appsBoxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // clLabel
            // 
            this.clLabel.AutoSize = true;
            this.clLabel.BackColor = System.Drawing.Color.Transparent;
            this.clLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.clLabel.ForeColor = System.Drawing.Color.Silver;
            this.clLabel.Location = new System.Drawing.Point(269, 231);
            this.clLabel.Name = "clLabel";
            this.clLabel.Size = new System.Drawing.Size(27, 13);
            this.clLabel.TabIndex = 5;
            this.clLabel.Text = "%*";
            this.clLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.clLabel.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // endArg
            // 
            this.endArg.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.endArg.Location = new System.Drawing.Point(299, 228);
            this.endArg.Name = "endArg";
            this.endArg.Size = new System.Drawing.Size(134, 21);
            this.endArg.TabIndex = 7;
            this.endArg.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // addArgsLabel
            // 
            this.addArgsLabel.BackColor = System.Drawing.Color.Transparent;
            this.addArgsLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.addArgsLabel.ForeColor = System.Drawing.Color.Silver;
            this.addArgsLabel.Location = new System.Drawing.Point(2, 231);
            this.addArgsLabel.Name = "addArgsLabel";
            this.addArgsLabel.Size = new System.Drawing.Size(126, 13);
            this.addArgsLabel.TabIndex = 4;
            this.addArgsLabel.Text = "Add Arguments:";
            this.addArgsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.addArgsLabel.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // startArg
            // 
            this.startArg.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.startArg.Location = new System.Drawing.Point(131, 228);
            this.startArg.Name = "startArg";
            this.startArg.Size = new System.Drawing.Size(134, 21);
            this.startArg.TabIndex = 6;
            this.startArg.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // noConfirmCheck
            // 
            this.noConfirmCheck.AutoSize = true;
            this.noConfirmCheck.BackColor = System.Drawing.Color.Transparent;
            this.noConfirmCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noConfirmCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.noConfirmCheck.Location = new System.Drawing.Point(142, 266);
            this.noConfirmCheck.Name = "noConfirmCheck";
            this.noConfirmCheck.Size = new System.Drawing.Size(180, 17);
            this.noConfirmCheck.TabIndex = 8;
            this.noConfirmCheck.Text = "Disable confirmation for this app";
            this.noConfirmCheck.UseVisualStyleBackColor = false;
            this.noConfirmCheck.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Highlight;
            this.tabPage2.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage2.Controls.Add(this.resetColorsBtn);
            this.tabPage2.Controls.Add(this.previewMainColor);
            this.tabPage2.Controls.Add(this.defBgCheck);
            this.tabPage2.Controls.Add(this.btnColorPanel);
            this.tabPage2.Controls.Add(this.setBgBtn);
            this.tabPage2.Controls.Add(this.btnColorPanelLabel);
            this.tabPage2.Controls.Add(this.fadeInNumLabel);
            this.tabPage2.Controls.Add(this.opacityNum);
            this.tabPage2.Controls.Add(this.fadeInNum);
            this.tabPage2.Controls.Add(this.btnTextColorPanel);
            this.tabPage2.Controls.Add(this.opacityNumLabel);
            this.tabPage2.Controls.Add(this.mainColorPanelLabel);
            this.tabPage2.Controls.Add(this.btnHoverColorPanel);
            this.tabPage2.Controls.Add(this.btnHoverColorPanelLabel);
            this.tabPage2.Controls.Add(this.mainColorPanel);
            this.tabPage2.Controls.Add(this.btnTextColorPanelLabel);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(484, 347);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Style";
            // 
            // resetColorsBtn
            // 
            this.resetColorsBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.resetColorsBtn.Location = new System.Drawing.Point(152, 270);
            this.resetColorsBtn.Name = "resetColorsBtn";
            this.resetColorsBtn.Size = new System.Drawing.Size(63, 23);
            this.resetColorsBtn.TabIndex = 21;
            this.resetColorsBtn.Text = "Reset";
            this.resetColorsBtn.UseVisualStyleBackColor = true;
            this.resetColorsBtn.Click += new System.EventHandler(this.resetColorsBtn_Click);
            // 
            // previewMainColor
            // 
            this.previewMainColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewMainColor.Controls.Add(this.previewBg);
            this.previewMainColor.Location = new System.Drawing.Point(250, 57);
            this.previewMainColor.Name = "previewMainColor";
            this.previewMainColor.Size = new System.Drawing.Size(198, 213);
            this.previewMainColor.TabIndex = 20;
            // 
            // previewBg
            // 
            this.previewBg.BackColor = System.Drawing.Color.Transparent;
            this.previewBg.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.previewBg.Controls.Add(this.previewLogoBox);
            this.previewBg.Controls.Add(this.previewBtn1);
            this.previewBg.Controls.Add(this.previewBtn2);
            this.previewBg.Controls.Add(this.previewAppList);
            this.previewBg.Location = new System.Drawing.Point(1, 1);
            this.previewBg.Name = "previewBg";
            this.previewBg.Size = new System.Drawing.Size(194, 209);
            this.previewBg.TabIndex = 21;
            // 
            // previewLogoBox
            // 
            this.previewLogoBox.Location = new System.Drawing.Point(136, 12);
            this.previewLogoBox.Name = "previewLogoBox";
            this.previewLogoBox.Size = new System.Drawing.Size(52, 52);
            this.previewLogoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.previewLogoBox.TabIndex = 25;
            this.previewLogoBox.TabStop = false;
            // 
            // previewBtn1
            // 
            this.previewBtn1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.previewBtn1.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.previewBtn1.FlatAppearance.BorderSize = 0;
            this.previewBtn1.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.previewBtn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.previewBtn1.Font = new System.Drawing.Font("Tahoma", 5F);
            this.previewBtn1.Location = new System.Drawing.Point(135, 157);
            this.previewBtn1.Name = "previewBtn1";
            this.previewBtn1.Size = new System.Drawing.Size(54, 16);
            this.previewBtn1.TabIndex = 24;
            this.previewBtn1.TabStop = false;
            this.previewBtn1.Text = "Button 1";
            this.previewBtn1.UseVisualStyleBackColor = false;
            // 
            // previewBtn2
            // 
            this.previewBtn2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.previewBtn2.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.previewBtn2.FlatAppearance.BorderSize = 0;
            this.previewBtn2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.previewBtn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.previewBtn2.Font = new System.Drawing.Font("Tahoma", 5F);
            this.previewBtn2.Location = new System.Drawing.Point(135, 177);
            this.previewBtn2.Name = "previewBtn2";
            this.previewBtn2.Size = new System.Drawing.Size(54, 16);
            this.previewBtn2.TabIndex = 23;
            this.previewBtn2.TabStop = false;
            this.previewBtn2.Text = "Button 2";
            this.previewBtn2.UseVisualStyleBackColor = false;
            // 
            // previewAppList
            // 
            this.previewAppList.BackColor = System.Drawing.Color.White;
            this.previewAppList.Location = new System.Drawing.Point(4, 4);
            this.previewAppList.Name = "previewAppList";
            this.previewAppList.Size = new System.Drawing.Size(126, 189);
            this.previewAppList.TabIndex = 22;
            this.previewAppList.Paint += new System.Windows.Forms.PaintEventHandler(this.previewAppList_Paint);
            // 
            // defBgCheck
            // 
            this.defBgCheck.AutoSize = true;
            this.defBgCheck.BackColor = System.Drawing.Color.Transparent;
            this.defBgCheck.Checked = true;
            this.defBgCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.defBgCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.defBgCheck.ForeColor = System.Drawing.Color.Silver;
            this.defBgCheck.Location = new System.Drawing.Point(51, 134);
            this.defBgCheck.Name = "defBgCheck";
            this.defBgCheck.Size = new System.Drawing.Size(137, 17);
            this.defBgCheck.TabIndex = 19;
            this.defBgCheck.Text = "Default Background";
            this.defBgCheck.UseVisualStyleBackColor = false;
            // 
            // btnColorPanel
            // 
            this.btnColorPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.btnColorPanel.Location = new System.Drawing.Point(199, 201);
            this.btnColorPanel.Name = "btnColorPanel";
            this.btnColorPanel.Size = new System.Drawing.Size(16, 16);
            this.btnColorPanel.TabIndex = 18;
            this.btnColorPanel.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // setBgBtn
            // 
            this.setBgBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.setBgBtn.Location = new System.Drawing.Point(33, 105);
            this.setBgBtn.Name = "setBgBtn";
            this.setBgBtn.Size = new System.Drawing.Size(183, 23);
            this.setBgBtn.TabIndex = 0;
            this.setBgBtn.Text = "Change Background";
            this.setBgBtn.UseVisualStyleBackColor = true;
            this.setBgBtn.Click += new System.EventHandler(this.setBgBtn_Click);
            // 
            // btnColorPanelLabel
            // 
            this.btnColorPanelLabel.BackColor = System.Drawing.Color.Transparent;
            this.btnColorPanelLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnColorPanelLabel.ForeColor = System.Drawing.Color.Silver;
            this.btnColorPanelLabel.Location = new System.Drawing.Point(13, 201);
            this.btnColorPanelLabel.Name = "btnColorPanelLabel";
            this.btnColorPanelLabel.Size = new System.Drawing.Size(180, 13);
            this.btnColorPanelLabel.TabIndex = 17;
            this.btnColorPanelLabel.Text = "Button Color:";
            this.btnColorPanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fadeInNumLabel
            // 
            this.fadeInNumLabel.BackColor = System.Drawing.Color.Transparent;
            this.fadeInNumLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fadeInNumLabel.ForeColor = System.Drawing.Color.Silver;
            this.fadeInNumLabel.Location = new System.Drawing.Point(16, 76);
            this.fadeInNumLabel.Name = "fadeInNumLabel";
            this.fadeInNumLabel.Size = new System.Drawing.Size(130, 13);
            this.fadeInNumLabel.TabIndex = 4;
            this.fadeInNumLabel.Text = "Fade In Duration:";
            this.fadeInNumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // opacityNum
            // 
            this.opacityNum.Location = new System.Drawing.Point(152, 46);
            this.opacityNum.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.opacityNum.Name = "opacityNum";
            this.opacityNum.Size = new System.Drawing.Size(64, 21);
            this.opacityNum.TabIndex = 12;
            this.opacityNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.opacityNum.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            // 
            // fadeInNum
            // 
            this.fadeInNum.Location = new System.Drawing.Point(152, 73);
            this.fadeInNum.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.fadeInNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fadeInNum.Name = "fadeInNum";
            this.fadeInNum.Size = new System.Drawing.Size(64, 21);
            this.fadeInNum.TabIndex = 11;
            this.fadeInNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.fadeInNum.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // btnTextColorPanel
            // 
            this.btnTextColorPanel.BackColor = System.Drawing.SystemColors.WindowText;
            this.btnTextColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.btnTextColorPanel.Location = new System.Drawing.Point(199, 245);
            this.btnTextColorPanel.Name = "btnTextColorPanel";
            this.btnTextColorPanel.Size = new System.Drawing.Size(16, 16);
            this.btnTextColorPanel.TabIndex = 17;
            this.btnTextColorPanel.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // opacityNumLabel
            // 
            this.opacityNumLabel.BackColor = System.Drawing.Color.Transparent;
            this.opacityNumLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.opacityNumLabel.ForeColor = System.Drawing.Color.Silver;
            this.opacityNumLabel.Location = new System.Drawing.Point(16, 48);
            this.opacityNumLabel.Name = "opacityNumLabel";
            this.opacityNumLabel.Size = new System.Drawing.Size(130, 13);
            this.opacityNumLabel.TabIndex = 2;
            this.opacityNumLabel.Text = "Opacity:";
            this.opacityNumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mainColorPanelLabel
            // 
            this.mainColorPanelLabel.BackColor = System.Drawing.Color.Transparent;
            this.mainColorPanelLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainColorPanelLabel.ForeColor = System.Drawing.Color.Silver;
            this.mainColorPanelLabel.Location = new System.Drawing.Point(13, 179);
            this.mainColorPanelLabel.Name = "mainColorPanelLabel";
            this.mainColorPanelLabel.Size = new System.Drawing.Size(180, 13);
            this.mainColorPanelLabel.TabIndex = 2;
            this.mainColorPanelLabel.Text = "Main Color:";
            this.mainColorPanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnHoverColorPanel
            // 
            this.btnHoverColorPanel.BackColor = System.Drawing.SystemColors.Highlight;
            this.btnHoverColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.btnHoverColorPanel.Location = new System.Drawing.Point(199, 223);
            this.btnHoverColorPanel.Name = "btnHoverColorPanel";
            this.btnHoverColorPanel.Size = new System.Drawing.Size(16, 16);
            this.btnHoverColorPanel.TabIndex = 16;
            this.btnHoverColorPanel.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // btnHoverColorPanelLabel
            // 
            this.btnHoverColorPanelLabel.BackColor = System.Drawing.Color.Transparent;
            this.btnHoverColorPanelLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHoverColorPanelLabel.ForeColor = System.Drawing.Color.Silver;
            this.btnHoverColorPanelLabel.Location = new System.Drawing.Point(13, 223);
            this.btnHoverColorPanelLabel.Name = "btnHoverColorPanelLabel";
            this.btnHoverColorPanelLabel.Size = new System.Drawing.Size(180, 13);
            this.btnHoverColorPanelLabel.TabIndex = 4;
            this.btnHoverColorPanelLabel.Text = "Button Hover Color:";
            this.btnHoverColorPanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mainColorPanel
            // 
            this.mainColorPanel.BackColor = System.Drawing.SystemColors.Highlight;
            this.mainColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainColorPanel.Location = new System.Drawing.Point(199, 179);
            this.mainColorPanel.Name = "mainColorPanel";
            this.mainColorPanel.Size = new System.Drawing.Size(16, 16);
            this.mainColorPanel.TabIndex = 15;
            this.mainColorPanel.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // btnTextColorPanelLabel
            // 
            this.btnTextColorPanelLabel.BackColor = System.Drawing.Color.Transparent;
            this.btnTextColorPanelLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTextColorPanelLabel.ForeColor = System.Drawing.Color.Silver;
            this.btnTextColorPanelLabel.Location = new System.Drawing.Point(13, 245);
            this.btnTextColorPanelLabel.Name = "btnTextColorPanelLabel";
            this.btnTextColorPanelLabel.Size = new System.Drawing.Size(180, 13);
            this.btnTextColorPanelLabel.TabIndex = 13;
            this.btnTextColorPanelLabel.Text = "Button Text Color:";
            this.btnTextColorPanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Highlight;
            this.tabPage3.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage3.Controls.Add(this.defaultPos);
            this.tabPage3.Controls.Add(this.defaultPosLabel);
            this.tabPage3.Controls.Add(this.startMenuIntegration);
            this.tabPage3.Controls.Add(this.startMenuIntegrationLabel);
            this.tabPage3.Controls.Add(this.addToShellBtn);
            this.tabPage3.Controls.Add(this.rmFromShellBtn);
            this.tabPage3.Controls.Add(this.setLang);
            this.tabPage3.Controls.Add(this.setLangLabel);
            this.tabPage3.Controls.Add(this.appDirs);
            this.tabPage3.Controls.Add(this.updateCheck);
            this.tabPage3.Controls.Add(this.updateCheckLabel);
            this.tabPage3.Controls.Add(this.appDirsLabel);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(484, 347);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = "Misc";
            // 
            // defaultPos
            // 
            this.defaultPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultPos.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.defaultPos.FormattingEnabled = true;
            this.defaultPos.Location = new System.Drawing.Point(300, 238);
            this.defaultPos.Name = "defaultPos";
            this.defaultPos.Size = new System.Drawing.Size(139, 21);
            this.defaultPos.TabIndex = 10;
            // 
            // defaultPosLabel
            // 
            this.defaultPosLabel.BackColor = System.Drawing.Color.Transparent;
            this.defaultPosLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.defaultPosLabel.ForeColor = System.Drawing.Color.Silver;
            this.defaultPosLabel.Location = new System.Drawing.Point(2, 242);
            this.defaultPosLabel.Name = "defaultPosLabel";
            this.defaultPosLabel.Size = new System.Drawing.Size(292, 13);
            this.defaultPosLabel.TabIndex = 13;
            this.defaultPosLabel.Text = "Default Location:";
            this.defaultPosLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // startMenuIntegration
            // 
            this.startMenuIntegration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.startMenuIntegration.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.startMenuIntegration.FormattingEnabled = true;
            this.startMenuIntegration.Location = new System.Drawing.Point(300, 207);
            this.startMenuIntegration.Name = "startMenuIntegration";
            this.startMenuIntegration.Size = new System.Drawing.Size(139, 21);
            this.startMenuIntegration.TabIndex = 9;
            // 
            // startMenuIntegrationLabel
            // 
            this.startMenuIntegrationLabel.BackColor = System.Drawing.Color.Transparent;
            this.startMenuIntegrationLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.startMenuIntegrationLabel.ForeColor = System.Drawing.Color.Silver;
            this.startMenuIntegrationLabel.Location = new System.Drawing.Point(2, 210);
            this.startMenuIntegrationLabel.Name = "startMenuIntegrationLabel";
            this.startMenuIntegrationLabel.Size = new System.Drawing.Size(292, 13);
            this.startMenuIntegrationLabel.TabIndex = 2;
            this.startMenuIntegrationLabel.Text = "Start Menu integration";
            this.startMenuIntegrationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // addToShellBtn
            // 
            this.addToShellBtn.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.addToShellBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.addToShellBtn.Image = global::AppsLauncher.Properties.Resources.uac_16;
            this.addToShellBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.addToShellBtn.Location = new System.Drawing.Point(134, 169);
            this.addToShellBtn.Name = "addToShellBtn";
            this.addToShellBtn.Size = new System.Drawing.Size(150, 24);
            this.addToShellBtn.TabIndex = 7;
            this.addToShellBtn.Text = "Integrate to Shell";
            this.addToShellBtn.UseVisualStyleBackColor = true;
            this.addToShellBtn.TextChanged += new System.EventHandler(this.button_TextChanged);
            this.addToShellBtn.Click += new System.EventHandler(this.addToShellBtn_Click);
            // 
            // rmFromShellBtn
            // 
            this.rmFromShellBtn.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.rmFromShellBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rmFromShellBtn.Image = global::AppsLauncher.Properties.Resources.uac_16;
            this.rmFromShellBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rmFromShellBtn.Location = new System.Drawing.Point(290, 169);
            this.rmFromShellBtn.Name = "rmFromShellBtn";
            this.rmFromShellBtn.Size = new System.Drawing.Size(150, 24);
            this.rmFromShellBtn.TabIndex = 8;
            this.rmFromShellBtn.Text = "Remove from Shell";
            this.rmFromShellBtn.UseVisualStyleBackColor = true;
            this.rmFromShellBtn.TextChanged += new System.EventHandler(this.button_TextChanged);
            this.rmFromShellBtn.Click += new System.EventHandler(this.rmFromShellBtn_Click);
            // 
            // setLang
            // 
            this.setLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.setLang.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.setLang.FormattingEnabled = true;
            this.setLang.Items.AddRange(new object[] {
            "en-US",
            "de-DE"});
            this.setLang.Location = new System.Drawing.Point(300, 300);
            this.setLang.Name = "setLang";
            this.setLang.Size = new System.Drawing.Size(139, 21);
            this.setLang.TabIndex = 12;
            // 
            // setLangLabel
            // 
            this.setLangLabel.BackColor = System.Drawing.Color.Transparent;
            this.setLangLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.setLangLabel.ForeColor = System.Drawing.Color.Silver;
            this.setLangLabel.Location = new System.Drawing.Point(2, 304);
            this.setLangLabel.Name = "setLangLabel";
            this.setLangLabel.Size = new System.Drawing.Size(292, 13);
            this.setLangLabel.TabIndex = 5;
            this.setLangLabel.Text = "Language:";
            this.setLangLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // appDirs
            // 
            this.appDirs.Location = new System.Drawing.Point(135, 25);
            this.appDirs.Name = "appDirs";
            this.appDirs.Size = new System.Drawing.Size(304, 130);
            this.appDirs.TabIndex = 6;
            this.appDirs.Text = "";
            this.appDirs.WordWrap = false;
            this.appDirs.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // updateCheck
            // 
            this.updateCheck.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.updateCheck.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.updateCheck.FormattingEnabled = true;
            this.updateCheck.Location = new System.Drawing.Point(300, 269);
            this.updateCheck.Name = "updateCheck";
            this.updateCheck.Size = new System.Drawing.Size(139, 21);
            this.updateCheck.TabIndex = 11;
            // 
            // updateCheckLabel
            // 
            this.updateCheckLabel.BackColor = System.Drawing.Color.Transparent;
            this.updateCheckLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.updateCheckLabel.ForeColor = System.Drawing.Color.Silver;
            this.updateCheckLabel.Location = new System.Drawing.Point(2, 273);
            this.updateCheckLabel.Name = "updateCheckLabel";
            this.updateCheckLabel.Size = new System.Drawing.Size(292, 13);
            this.updateCheckLabel.TabIndex = 4;
            this.updateCheckLabel.Text = "Search for Updates:";
            this.updateCheckLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // appDirsLabel
            // 
            this.appDirsLabel.BackColor = System.Drawing.Color.Transparent;
            this.appDirsLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.appDirsLabel.ForeColor = System.Drawing.Color.Silver;
            this.appDirsLabel.Location = new System.Drawing.Point(2, 28);
            this.appDirsLabel.Name = "appDirsLabel";
            this.appDirsLabel.Size = new System.Drawing.Size(127, 13);
            this.appDirsLabel.TabIndex = 1;
            this.appDirsLabel.Text = "App Directories:";
            this.appDirsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(492, 442);
            this.Controls.Add(this.tabCtrl);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabCtrl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.fileTypesMenu.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.previewMainColor.ResumeLayout(false);
            this.previewBg.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.previewLogoBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeInNum)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TabControl tabCtrl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button associateBtn;
        private System.Windows.Forms.Label fileTypesLabel;
        private System.Windows.Forms.Label appsBoxLabel;
        private System.Windows.Forms.TextBox endArg;
        private System.Windows.Forms.Label addArgsLabel;
        private System.Windows.Forms.TextBox startArg;
        private System.Windows.Forms.CheckBox noConfirmCheck;
        private System.Windows.Forms.ComboBox updateCheck;
        private System.Windows.Forms.Label updateCheckLabel;
        private System.Windows.Forms.Label appDirsLabel;
        private System.Windows.Forms.ComboBox appsBox;
        private System.Windows.Forms.Label clLabel;
        private System.Windows.Forms.Button locationBtn;
        private System.Windows.Forms.RichTextBox fileTypes;
        private System.Windows.Forms.RichTextBox appDirs;
        private System.Windows.Forms.ComboBox setLang;
        private System.Windows.Forms.Label setLangLabel;
        private System.Windows.Forms.Button addToShellBtn;
        private System.Windows.Forms.Button rmFromShellBtn;
        private System.Windows.Forms.ComboBox startMenuIntegration;
        private System.Windows.Forms.Label startMenuIntegrationLabel;
        private System.Windows.Forms.ContextMenuStrip fileTypesMenu;
        private System.Windows.Forms.ToolStripMenuItem fileTypesMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fileTypesMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem fileTypesMenuItem3;
        private System.Windows.Forms.CheckBox noUpdatesCheck;
        private System.Windows.Forms.CheckBox runAsAdminCheck;
        private System.Windows.Forms.Button undoAssociationBtn;
        private System.Windows.Forms.ComboBox defaultPos;
        private System.Windows.Forms.Label defaultPosLabel;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button setBgBtn;
        private System.Windows.Forms.Panel btnTextColorPanel;
        private System.Windows.Forms.Panel btnHoverColorPanel;
        private System.Windows.Forms.Panel mainColorPanel;
        private System.Windows.Forms.Label btnTextColorPanelLabel;
        private System.Windows.Forms.Label btnHoverColorPanelLabel;
        private System.Windows.Forms.Label mainColorPanelLabel;
        private System.Windows.Forms.Label fadeInNumLabel;
        private System.Windows.Forms.NumericUpDown opacityNum;
        private System.Windows.Forms.NumericUpDown fadeInNum;
        private System.Windows.Forms.Label opacityNumLabel;
        private System.Windows.Forms.Panel btnColorPanel;
        private System.Windows.Forms.Label btnColorPanelLabel;
        private System.Windows.Forms.Panel previewMainColor;
        private System.Windows.Forms.Panel previewBg;
        private System.Windows.Forms.PictureBox previewLogoBox;
        private System.Windows.Forms.Button previewBtn1;
        private System.Windows.Forms.Button previewBtn2;
        private System.Windows.Forms.Panel previewAppList;
        private System.Windows.Forms.CheckBox defBgCheck;
        private System.Windows.Forms.Button resetColorsBtn;
    }
}