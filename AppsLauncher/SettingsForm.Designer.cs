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
            this.fileTypes = new System.Windows.Forms.RichTextBox();
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
            this.startItem = new System.Windows.Forms.ComboBox();
            this.startItemLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabCtrl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            this.tabPage1.Text = "Arguments";
            // 
            // fileTypes
            // 
            this.fileTypes.Location = new System.Drawing.Point(131, 69);
            this.fileTypes.Name = "fileTypes";
            this.fileTypes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.fileTypes.Size = new System.Drawing.Size(302, 131);
            this.fileTypes.TabIndex = 9;
            this.fileTypes.Text = "";
            this.fileTypes.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // locationBtn
            // 
            this.locationBtn.BackgroundImage = global::AppsLauncher.Properties.Resources.folder_16;
            this.locationBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.locationBtn.Location = new System.Drawing.Point(392, 34);
            this.locationBtn.Name = "locationBtn";
            this.locationBtn.Size = new System.Drawing.Size(24, 24);
            this.locationBtn.TabIndex = 8;
            this.locationBtn.UseVisualStyleBackColor = true;
            this.locationBtn.Click += new System.EventHandler(this.locationBtn_Click);
            // 
            // appsBox
            // 
            this.appsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.appsBox.FormattingEnabled = true;
            this.appsBox.Location = new System.Drawing.Point(131, 35);
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
            this.associateBtn.Location = new System.Drawing.Point(299, 213);
            this.associateBtn.Name = "associateBtn";
            this.associateBtn.Size = new System.Drawing.Size(135, 24);
            this.associateBtn.TabIndex = 3;
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
            this.fileTypesLabel.Location = new System.Drawing.Point(2, 72);
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
            this.appsBoxLabel.Location = new System.Drawing.Point(2, 38);
            this.appsBoxLabel.Name = "appsBoxLabel";
            this.appsBoxLabel.Size = new System.Drawing.Size(126, 13);
            this.appsBoxLabel.TabIndex = 1;
            this.appsBoxLabel.Text = "Current App:";
            this.appsBoxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // clLabel
            // 
            this.clLabel.AutoSize = true;
            this.clLabel.BackColor = System.Drawing.Color.Transparent;
            this.clLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.clLabel.ForeColor = System.Drawing.Color.Silver;
            this.clLabel.Location = new System.Drawing.Point(269, 253);
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
            this.endArg.Location = new System.Drawing.Point(299, 250);
            this.endArg.Name = "endArg";
            this.endArg.Size = new System.Drawing.Size(134, 21);
            this.endArg.TabIndex = 6;
            this.endArg.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // addArgsLabel
            // 
            this.addArgsLabel.BackColor = System.Drawing.Color.Transparent;
            this.addArgsLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.addArgsLabel.ForeColor = System.Drawing.Color.Silver;
            this.addArgsLabel.Location = new System.Drawing.Point(2, 253);
            this.addArgsLabel.Name = "addArgsLabel";
            this.addArgsLabel.Size = new System.Drawing.Size(126, 13);
            this.addArgsLabel.TabIndex = 4;
            this.addArgsLabel.Text = "Add:";
            this.addArgsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.addArgsLabel.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // startArg
            // 
            this.startArg.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.startArg.Location = new System.Drawing.Point(131, 250);
            this.startArg.Name = "startArg";
            this.startArg.Size = new System.Drawing.Size(134, 21);
            this.startArg.TabIndex = 4;
            this.startArg.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // noConfirmCheck
            // 
            this.noConfirmCheck.AutoSize = true;
            this.noConfirmCheck.BackColor = System.Drawing.Color.Transparent;
            this.noConfirmCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noConfirmCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.noConfirmCheck.Location = new System.Drawing.Point(142, 288);
            this.noConfirmCheck.Name = "noConfirmCheck";
            this.noConfirmCheck.Size = new System.Drawing.Size(180, 17);
            this.noConfirmCheck.TabIndex = 7;
            this.noConfirmCheck.Text = "Disable confirmation for this app";
            this.noConfirmCheck.UseVisualStyleBackColor = false;
            this.noConfirmCheck.MouseEnter += new System.EventHandler(this.ToolTipAtMouseEnter);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Highlight;
            this.tabPage2.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage2.Controls.Add(this.startMenuIntegration);
            this.tabPage2.Controls.Add(this.startMenuIntegrationLabel);
            this.tabPage2.Controls.Add(this.addToShellBtn);
            this.tabPage2.Controls.Add(this.rmFromShellBtn);
            this.tabPage2.Controls.Add(this.setLang);
            this.tabPage2.Controls.Add(this.setLangLabel);
            this.tabPage2.Controls.Add(this.appDirs);
            this.tabPage2.Controls.Add(this.updateCheck);
            this.tabPage2.Controls.Add(this.updateCheckLabel);
            this.tabPage2.Controls.Add(this.appDirsLabel);
            this.tabPage2.Controls.Add(this.startItem);
            this.tabPage2.Controls.Add(this.startItemLabel);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(484, 347);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Misc";
            // 
            // startMenuIntegration
            // 
            this.startMenuIntegration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.startMenuIntegration.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.startMenuIntegration.FormattingEnabled = true;
            this.startMenuIntegration.Location = new System.Drawing.Point(300, 201);
            this.startMenuIntegration.Name = "startMenuIntegration";
            this.startMenuIntegration.Size = new System.Drawing.Size(139, 21);
            this.startMenuIntegration.TabIndex = 9;
            // 
            // startMenuIntegrationLabel
            // 
            this.startMenuIntegrationLabel.BackColor = System.Drawing.Color.Transparent;
            this.startMenuIntegrationLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.startMenuIntegrationLabel.ForeColor = System.Drawing.Color.Silver;
            this.startMenuIntegrationLabel.Location = new System.Drawing.Point(2, 204);
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
            this.addToShellBtn.Location = new System.Drawing.Point(134, 163);
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
            this.rmFromShellBtn.Location = new System.Drawing.Point(290, 163);
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
            this.setLang.Location = new System.Drawing.Point(300, 306);
            this.setLang.Name = "setLang";
            this.setLang.Size = new System.Drawing.Size(139, 21);
            this.setLang.TabIndex = 12;
            // 
            // setLangLabel
            // 
            this.setLangLabel.BackColor = System.Drawing.Color.Transparent;
            this.setLangLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.setLangLabel.ForeColor = System.Drawing.Color.Silver;
            this.setLangLabel.Location = new System.Drawing.Point(2, 310);
            this.setLangLabel.Name = "setLangLabel";
            this.setLangLabel.Size = new System.Drawing.Size(292, 13);
            this.setLangLabel.TabIndex = 5;
            this.setLangLabel.Text = "Language:";
            this.setLangLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // appDirs
            // 
            this.appDirs.Location = new System.Drawing.Point(135, 19);
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
            this.updateCheck.Location = new System.Drawing.Point(300, 271);
            this.updateCheck.Name = "updateCheck";
            this.updateCheck.Size = new System.Drawing.Size(139, 21);
            this.updateCheck.TabIndex = 11;
            // 
            // updateCheckLabel
            // 
            this.updateCheckLabel.BackColor = System.Drawing.Color.Transparent;
            this.updateCheckLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.updateCheckLabel.ForeColor = System.Drawing.Color.Silver;
            this.updateCheckLabel.Location = new System.Drawing.Point(2, 275);
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
            this.appDirsLabel.Location = new System.Drawing.Point(2, 22);
            this.appDirsLabel.Name = "appDirsLabel";
            this.appDirsLabel.Size = new System.Drawing.Size(127, 13);
            this.appDirsLabel.TabIndex = 1;
            this.appDirsLabel.Text = "App Directories:";
            this.appDirsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // startItem
            // 
            this.startItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.startItem.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.startItem.FormattingEnabled = true;
            this.startItem.Location = new System.Drawing.Point(300, 236);
            this.startItem.Name = "startItem";
            this.startItem.Size = new System.Drawing.Size(139, 21);
            this.startItem.TabIndex = 10;
            // 
            // startItemLabel
            // 
            this.startItemLabel.BackColor = System.Drawing.Color.Transparent;
            this.startItemLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.startItemLabel.ForeColor = System.Drawing.Color.Silver;
            this.startItemLabel.Location = new System.Drawing.Point(2, 239);
            this.startItemLabel.Name = "startItemLabel";
            this.startItemLabel.Size = new System.Drawing.Size(292, 13);
            this.startItemLabel.TabIndex = 3;
            this.startItemLabel.Text = "Selected Item at startup:";
            this.startItemLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.tabPage2.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tabPage2;
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
        private System.Windows.Forms.ComboBox startItem;
        private System.Windows.Forms.Label startItemLabel;
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
    }
}