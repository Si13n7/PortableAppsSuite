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
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.updateChecker = new System.ComponentModel.BackgroundWorker();
            this.closeToUpdate = new System.Windows.Forms.Timer(this.components);
            this.logoPanel = new System.Windows.Forms.Panel();
            this.leftBorderPanel = new System.Windows.Forms.Label();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.updateBtnPanel = new System.Windows.Forms.Panel();
            this.updateBtn = new System.Windows.Forms.Button();
            this.aboutInfoLabel = new System.Windows.Forms.LinkLabel();
            this.copyrightLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            this.logoPanel.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.updateBtnPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // logoBox
            // 
            this.logoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.logoBox.BackColor = System.Drawing.Color.Transparent;
            this.logoBox.BackgroundImage = global::AppsLauncher.Properties.Resources.PortableApps_Logo_gray;
            this.logoBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.logoBox.Location = new System.Drawing.Point(0, 0);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(163, 177);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.logoBox.TabIndex = 0;
            this.logoBox.TabStop = false;
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
            this.logoPanel.Controls.Add(this.logoBox);
            this.logoPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.logoPanel.Location = new System.Drawing.Point(0, 0);
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(163, 177);
            this.logoPanel.TabIndex = 19;
            // 
            // leftBorderPanel
            // 
            this.leftBorderPanel.BackColor = System.Drawing.Color.Black;
            this.leftBorderPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftBorderPanel.Location = new System.Drawing.Point(163, 0);
            this.leftBorderPanel.Name = "leftBorderPanel";
            this.leftBorderPanel.Size = new System.Drawing.Size(1, 177);
            this.leftBorderPanel.TabIndex = 20;
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.mainPanel.BackgroundImage = global::AppsLauncher.Properties.Resources.horizontal_pattern;
            this.mainPanel.Controls.Add(this.updateBtnPanel);
            this.mainPanel.Controls.Add(this.aboutInfoLabel);
            this.mainPanel.Controls.Add(this.copyrightLabel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(164, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(412, 177);
            this.mainPanel.TabIndex = 21;
            // 
            // updateBtnPanel
            // 
            this.updateBtnPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updateBtnPanel.BackColor = System.Drawing.Color.Transparent;
            this.updateBtnPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.updateBtnPanel.Controls.Add(this.updateBtn);
            this.updateBtnPanel.Location = new System.Drawing.Point(20, 25);
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
            this.aboutInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.aboutInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.aboutInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aboutInfoLabel.Font = new System.Drawing.Font("Tahoma", 7.25F);
            this.aboutInfoLabel.ForeColor = System.Drawing.Color.SlateGray;
            this.aboutInfoLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.aboutInfoLabel.LinkColor = System.Drawing.Color.PowderBlue;
            this.aboutInfoLabel.Location = new System.Drawing.Point(14, 60);
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
            this.copyrightLabel.Location = new System.Drawing.Point(0, 155);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(412, 22);
            this.copyrightLabel.TabIndex = 25;
            this.copyrightLabel.Text = "Copyright © Si13n7 Dev. ® {0}";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(576, 177);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.leftBorderPanel);
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
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            this.logoPanel.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.updateBtnPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox logoBox;
        private System.ComponentModel.BackgroundWorker updateChecker;
        private System.Windows.Forms.Timer closeToUpdate;
        private System.Windows.Forms.Panel logoPanel;
        private System.Windows.Forms.Label leftBorderPanel;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.LinkLabel aboutInfoLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Button updateBtn;
        private System.Windows.Forms.Panel updateBtnPanel;
    }
}