namespace AppsLauncher
{
    partial class AboutForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.updateChecker = new System.ComponentModel.BackgroundWorker();
            this.closeToUpdate = new System.Windows.Forms.Timer(this.components);
            this.logoPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.updateBtnPanel = new System.Windows.Forms.Panel();
            this.updateBtn = new System.Windows.Forms.Button();
            this.aboutInfoLabel = new System.Windows.Forms.LinkLabel();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.appsLauncherVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.appsDownloaderVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.appsLauncherUpdaterVersion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.logoPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.updateBtnPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::AppsLauncher.Properties.Resources.PortableApps_Logo_gray;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(163, 317);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // updateChecker
            // 
            this.updateChecker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.updateChecker_DoWork);
            this.updateChecker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.updateChecker_RunWorkerCompleted);
            // 
            // closeToUpdate
            // 
            this.closeToUpdate.Interval = 1;
            this.closeToUpdate.Tick += new System.EventHandler(this.closeToUpdate_Tick);
            // 
            // logoPanel
            // 
            this.logoPanel.BackColor = System.Drawing.Color.SlateGray;
            this.logoPanel.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.logoPanel.Controls.Add(this.pictureBox1);
            this.logoPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.logoPanel.Location = new System.Drawing.Point(0, 0);
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(163, 317);
            this.logoPanel.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(163, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1, 317);
            this.label1.TabIndex = 20;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.BackgroundImage = global::AppsLauncher.Properties.Resources.horizontal_pattern;
            this.panel1.Controls.Add(this.appsLauncherUpdaterVersion);
            this.panel1.Controls.Add(this.appsDownloaderVersion);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.appsLauncherVersion);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.updateBtnPanel);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.aboutInfoLabel);
            this.panel1.Controls.Add(this.copyrightLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(164, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(412, 317);
            this.panel1.TabIndex = 21;
            // 
            // updateBtnPanel
            // 
            this.updateBtnPanel.BackColor = System.Drawing.Color.Transparent;
            this.updateBtnPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.updateBtnPanel.Controls.Add(this.updateBtn);
            this.updateBtnPanel.Location = new System.Drawing.Point(20, 165);
            this.updateBtnPanel.Name = "updateBtnPanel";
            this.updateBtnPanel.Size = new System.Drawing.Size(130, 23);
            this.updateBtnPanel.TabIndex = 23;
            // 
            // updateBtn
            // 
            this.updateBtn.BackColor = System.Drawing.SystemColors.ControlDark;
            this.updateBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.updateBtn.FlatAppearance.BorderSize = 0;
            this.updateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.updateBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.updateBtn.Location = new System.Drawing.Point(0, 0);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(128, 21);
            this.updateBtn.TabIndex = 23;
            this.updateBtn.Text = "Check for updates";
            this.updateBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.updateBtn.UseVisualStyleBackColor = false;
            this.updateBtn.Click += new System.EventHandler(this.updateBtn_Click);
            // 
            // aboutInfoLabel
            // 
            this.aboutInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.aboutInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aboutInfoLabel.Font = new System.Drawing.Font("Tahoma", 7.25F);
            this.aboutInfoLabel.ForeColor = System.Drawing.Color.SlateGray;
            this.aboutInfoLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.aboutInfoLabel.LinkColor = System.Drawing.Color.PowderBlue;
            this.aboutInfoLabel.Location = new System.Drawing.Point(14, 200);
            this.aboutInfoLabel.Name = "aboutInfoLabel";
            this.aboutInfoLabel.Size = new System.Drawing.Size(384, 79);
            this.aboutInfoLabel.TabIndex = 26;
            this.aboutInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.aboutInfoLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.aboutInfoLabel_LinkClicked);
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.BackColor = System.Drawing.Color.Transparent;
            this.copyrightLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.copyrightLabel.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.copyrightLabel.Location = new System.Drawing.Point(0, 295);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(412, 22);
            this.copyrightLabel.TabIndex = 25;
            this.copyrightLabel.Text = "Copyright © Si13n7 Dev. ® {0}";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // appsLauncherVersion
            // 
            this.appsLauncherVersion.AutoSize = true;
            this.appsLauncherVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsLauncherVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsLauncherVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsLauncherVersion.Location = new System.Drawing.Point(12, 38);
            this.appsLauncherVersion.Name = "appsLauncherVersion";
            this.appsLauncherVersion.Size = new System.Drawing.Size(44, 13);
            this.appsLauncherVersion.TabIndex = 5;
            this.appsLauncherVersion.Text = "1.0.0.0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.PowderBlue;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(236, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Portable Apps Launcher";
            // 
            // appsDownloaderVersion
            // 
            this.appsDownloaderVersion.AutoSize = true;
            this.appsDownloaderVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsDownloaderVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsDownloaderVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsDownloaderVersion.Location = new System.Drawing.Point(13, 83);
            this.appsDownloaderVersion.Name = "appsDownloaderVersion";
            this.appsDownloaderVersion.Size = new System.Drawing.Size(44, 13);
            this.appsDownloaderVersion.TabIndex = 7;
            this.appsDownloaderVersion.Text = "1.0.0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.PowderBlue;
            this.label3.Location = new System.Drawing.Point(13, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(264, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Portable Apps Downloader";
            // 
            // appsLauncherUpdaterVersion
            // 
            this.appsLauncherUpdaterVersion.AutoSize = true;
            this.appsLauncherUpdaterVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsLauncherUpdaterVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsLauncherUpdaterVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsLauncherUpdaterVersion.Location = new System.Drawing.Point(12, 128);
            this.appsLauncherUpdaterVersion.Name = "appsLauncherUpdaterVersion";
            this.appsLauncherUpdaterVersion.Size = new System.Drawing.Size(44, 13);
            this.appsLauncherUpdaterVersion.TabIndex = 8;
            this.appsLauncherUpdaterVersion.Text = "1.0.0.0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.PowderBlue;
            this.label4.Location = new System.Drawing.Point(12, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(280, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "Portable Apps Suite Updater";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(576, 317);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logoPanel);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Silver;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About Portable Apps Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AboutForm_FormClosing);
            this.Load += new System.EventHandler(this.AboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.logoPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.updateBtnPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.ComponentModel.BackgroundWorker updateChecker;
        private System.Windows.Forms.Timer closeToUpdate;
        private System.Windows.Forms.Panel logoPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel aboutInfoLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Label appsLauncherVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label appsDownloaderVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label appsLauncherUpdaterVersion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button updateBtn;
        private System.Windows.Forms.Panel updateBtnPanel;
    }
}