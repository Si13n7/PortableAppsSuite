namespace AppsDownloader
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.appList = new System.Windows.Forms.CheckedListBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OKBtn = new System.Windows.Forms.Button();
            this.DLLoaded = new System.Windows.Forms.Label();
            this.DLPercentage = new System.Windows.Forms.ProgressBar();
            this.DLSpeed = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.UrlStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.AppStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.CheckDownload = new System.Windows.Forms.Timer(this.components);
            this.MultiDownloader = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 10);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(714, 310);
            this.panel2.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.appList, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 310F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(714, 310);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AppsDownloader.Properties.Resources.PortableApps_b;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(114, 290);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // appList
            // 
            this.appList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.appList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.appList.CheckOnClick = true;
            this.appList.ColumnWidth = 195;
            this.appList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.appList.FormattingEnabled = true;
            this.appList.Location = new System.Drawing.Point(123, 3);
            this.appList.MultiColumn = true;
            this.appList.Name = "appList";
            this.appList.Size = new System.Drawing.Size(580, 304);
            this.appList.TabIndex = 2;
            this.appList.ThreeDCheckBoxes = true;
            this.appList.SelectedIndexChanged += new System.EventHandler(this.appList_SelectedIndexChanged);
            // 
            // panel4
            // 
            this.panel4.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Controls.Add(this.DLLoaded);
            this.panel4.Controls.Add(this.DLPercentage);
            this.panel4.Controls.Add(this.DLSpeed);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Controls.Add(this.statusStrip1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 320);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(714, 75);
            this.panel4.TabIndex = 5;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Controls.Add(this.CancelBtn);
            this.panel3.Controls.Add(this.OKBtn);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(511, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(203, 56);
            this.panel3.TabIndex = 8;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(101, 21);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 1;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // OKBtn
            // 
            this.OKBtn.Enabled = false;
            this.OKBtn.Location = new System.Drawing.Point(9, 21);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 0;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // DLLoaded
            // 
            this.DLLoaded.BackColor = System.Drawing.Color.Transparent;
            this.DLLoaded.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLLoaded.Location = new System.Drawing.Point(34, 40);
            this.DLLoaded.Name = "DLLoaded";
            this.DLLoaded.Size = new System.Drawing.Size(455, 13);
            this.DLLoaded.TabIndex = 7;
            this.DLLoaded.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DLLoaded.Visible = false;
            // 
            // DLPercentage
            // 
            this.DLPercentage.Location = new System.Drawing.Point(34, 27);
            this.DLPercentage.Name = "DLPercentage";
            this.DLPercentage.Size = new System.Drawing.Size(455, 10);
            this.DLPercentage.TabIndex = 6;
            this.DLPercentage.Visible = false;
            // 
            // DLSpeed
            // 
            this.DLSpeed.BackColor = System.Drawing.Color.Transparent;
            this.DLSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLSpeed.Location = new System.Drawing.Point(34, 11);
            this.DLSpeed.Name = "DLSpeed";
            this.DLSpeed.Size = new System.Drawing.Size(455, 13);
            this.DLSpeed.TabIndex = 5;
            this.DLSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DLSpeed.Visible = false;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Location = new System.Drawing.Point(0, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(714, 1);
            this.label2.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.BackColor = System.Drawing.Color.RoyalBlue;
            this.statusStrip1.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.statusStrip1.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UrlStatus,
            this.AppStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 57);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(714, 18);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            // 
            // UrlStatus
            // 
            this.UrlStatus.BackColor = System.Drawing.Color.Transparent;
            this.UrlStatus.ForeColor = System.Drawing.Color.LightSlateGray;
            this.UrlStatus.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.UrlStatus.Name = "UrlStatus";
            this.UrlStatus.Size = new System.Drawing.Size(88, 13);
            this.UrlStatus.Text = "www.si13n7.com";
            this.UrlStatus.Click += new System.EventHandler(this.UrlStatus_Click);
            // 
            // AppStatus
            // 
            this.AppStatus.BackColor = System.Drawing.Color.Transparent;
            this.AppStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.AppStatus.ForeColor = System.Drawing.Color.LightSlateGray;
            this.AppStatus.Name = "AppStatus";
            this.AppStatus.Size = new System.Drawing.Size(0, 13);
            // 
            // CheckDownload
            // 
            this.CheckDownload.Interval = 10;
            this.CheckDownload.Tick += new System.EventHandler(this.CheckDownload_Tick);
            // 
            // MultiDownloader
            // 
            this.MultiDownloader.Tick += new System.EventHandler(this.MultiDownloader_Tick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(714, 10);
            this.panel1.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(714, 395);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1280, 434);
            this.MinimumSize = new System.Drawing.Size(722, 434);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Portable Apps Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel AppStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripStatusLabel UrlStatus;
        private System.Windows.Forms.Timer CheckDownload;
        private System.Windows.Forms.Label DLLoaded;
        private System.Windows.Forms.ProgressBar DLPercentage;
        private System.Windows.Forms.Label DLSpeed;
        private System.Windows.Forms.Timer MultiDownloader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckedListBox appList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
    }
}

