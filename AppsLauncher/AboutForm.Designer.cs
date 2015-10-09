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
            this.panel5 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.aboutInfoLabel = new System.Windows.Forms.LinkLabel();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.appsLauncherVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.appsDownloaderVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.appsLauncherUpdaterVersion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.updateBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::AppsLauncher.Properties.Resources.PortableApps_Logo_gray;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(163, 325);
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
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.RoyalBlue;
            this.panel5.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.panel5.Controls.Add(this.pictureBox1);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(163, 325);
            this.panel5.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(163, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1, 325);
            this.label1.TabIndex = 20;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.panel1.Controls.Add(this.aboutInfoLabel);
            this.panel1.Controls.Add(this.copyrightLabel);
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Controls.Add(this.updateBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(164, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(412, 325);
            this.panel1.TabIndex = 21;
            // 
            // aboutInfoLabel
            // 
            this.aboutInfoLabel.ActiveLinkColor = System.Drawing.Color.White;
            this.aboutInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.aboutInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aboutInfoLabel.Font = new System.Drawing.Font("Tahoma", 7.25F);
            this.aboutInfoLabel.ForeColor = System.Drawing.Color.SlateGray;
            this.aboutInfoLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.aboutInfoLabel.LinkColor = System.Drawing.Color.DodgerBlue;
            this.aboutInfoLabel.Location = new System.Drawing.Point(14, 207);
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
            this.copyrightLabel.ForeColor = System.Drawing.Color.DimGray;
            this.copyrightLabel.Location = new System.Drawing.Point(0, 303);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(412, 22);
            this.copyrightLabel.TabIndex = 25;
            this.copyrightLabel.Text = "Copyright © Si13n7 Dev. ® 2015";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(372, 152);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.appsLauncherVersion);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(366, 44);
            this.panel2.TabIndex = 0;
            // 
            // appsLauncherVersion
            // 
            this.appsLauncherVersion.AutoSize = true;
            this.appsLauncherVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsLauncherVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.appsLauncherVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsLauncherVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsLauncherVersion.Location = new System.Drawing.Point(0, 23);
            this.appsLauncherVersion.Name = "appsLauncherVersion";
            this.appsLauncherVersion.Size = new System.Drawing.Size(44, 13);
            this.appsLauncherVersion.TabIndex = 5;
            this.appsLauncherVersion.Text = "1.0.0.0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.LightBlue;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(236, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Portable Apps Launcher";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.appsDownloaderVersion);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 53);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(366, 44);
            this.panel3.TabIndex = 1;
            // 
            // appsDownloaderVersion
            // 
            this.appsDownloaderVersion.AutoSize = true;
            this.appsDownloaderVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsDownloaderVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.appsDownloaderVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsDownloaderVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsDownloaderVersion.Location = new System.Drawing.Point(0, 23);
            this.appsDownloaderVersion.Name = "appsDownloaderVersion";
            this.appsDownloaderVersion.Size = new System.Drawing.Size(44, 13);
            this.appsDownloaderVersion.TabIndex = 7;
            this.appsDownloaderVersion.Text = "1.0.0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.LightBlue;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(264, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Portable Apps Downloader";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.appsLauncherUpdaterVersion);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 103);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(366, 46);
            this.panel4.TabIndex = 2;
            // 
            // appsLauncherUpdaterVersion
            // 
            this.appsLauncherUpdaterVersion.AutoSize = true;
            this.appsLauncherUpdaterVersion.BackColor = System.Drawing.Color.Transparent;
            this.appsLauncherUpdaterVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.appsLauncherUpdaterVersion.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsLauncherUpdaterVersion.ForeColor = System.Drawing.Color.SlateGray;
            this.appsLauncherUpdaterVersion.Location = new System.Drawing.Point(0, 23);
            this.appsLauncherUpdaterVersion.Name = "appsLauncherUpdaterVersion";
            this.appsLauncherUpdaterVersion.Size = new System.Drawing.Size(44, 13);
            this.appsLauncherUpdaterVersion.TabIndex = 8;
            this.appsLauncherUpdaterVersion.Text = "1.0.0.0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.LightBlue;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(318, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "Portable Apps Launcher Updater";
            // 
            // updateBtn
            // 
            this.updateBtn.BackColor = System.Drawing.SystemColors.ControlDark;
            this.updateBtn.FlatAppearance.BorderSize = 0;
            this.updateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.updateBtn.Location = new System.Drawing.Point(14, 172);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(125, 23);
            this.updateBtn.TabIndex = 23;
            this.updateBtn.Text = "Check for updates";
            this.updateBtn.UseVisualStyleBackColor = false;
            this.updateBtn.Click += new System.EventHandler(this.updateBtn_Click);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(576, 325);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel5);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Silver;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About Portable Apps Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AboutForm_FormClosing);
            this.Load += new System.EventHandler(this.AboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.ComponentModel.BackgroundWorker updateChecker;
        private System.Windows.Forms.Timer closeToUpdate;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel aboutInfoLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label appsLauncherVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label appsDownloaderVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label appsLauncherUpdaterVersion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button updateBtn;
    }
}