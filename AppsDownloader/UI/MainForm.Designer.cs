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
            System.Windows.Forms.ListViewGroup listViewGroup14 = new System.Windows.Forms.ListViewGroup("listViewGroup0", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup15 = new System.Windows.Forms.ListViewGroup("listViewGroup1", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup16 = new System.Windows.Forms.ListViewGroup("listViewGroup2", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup17 = new System.Windows.Forms.ListViewGroup("listViewGroup3", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup18 = new System.Windows.Forms.ListViewGroup("listViewGroup4", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup19 = new System.Windows.Forms.ListViewGroup("listViewGroup5", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup20 = new System.Windows.Forms.ListViewGroup("listViewGroup6", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup21 = new System.Windows.Forms.ListViewGroup("listViewGroup7", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup22 = new System.Windows.Forms.ListViewGroup("listViewGroup8", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup23 = new System.Windows.Forms.ListViewGroup("listViewGroup9", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup24 = new System.Windows.Forms.ListViewGroup("listViewGroup10", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup25 = new System.Windows.Forms.ListViewGroup("listViewGroup11", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup26 = new System.Windows.Forms.ListViewGroup("listViewGroup12", System.Windows.Forms.HorizontalAlignment.Left);
            this.checkDownload = new System.Windows.Forms.Timer(this.components);
            this.multiDownloader = new System.Windows.Forms.Timer(this.components);
            this.statusBar = new System.Windows.Forms.Panel();
            this.appStatus = new System.Windows.Forms.Label();
            this.urlStatus = new System.Windows.Forms.Label();
            this.statusBarBorder = new System.Windows.Forms.Panel();
            this.downloadStateAreaPanel = new System.Windows.Forms.Panel();
            this.downloadProgress = new System.Windows.Forms.Panel();
            this.buttonArea = new System.Windows.Forms.Panel();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.downloadReceived = new System.Windows.Forms.Label();
            this.downloadSpeed = new System.Windows.Forms.Label();
            this.settingsAreaBorder = new System.Windows.Forms.Panel();
            this.settingsArea = new System.Windows.Forms.Panel();
            this.showColorsCheck = new System.Windows.Forms.CheckBox();
            this.showGroupsCheck = new System.Windows.Forms.CheckBox();
            this.searchArea = new System.Windows.Forms.Panel();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.appsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.searchResultBlinker = new System.Windows.Forms.Timer(this.components);
            this.statusBar.SuspendLayout();
            this.downloadStateAreaPanel.SuspendLayout();
            this.buttonArea.SuspendLayout();
            this.settingsArea.SuspendLayout();
            this.searchArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkDownload
            // 
            this.checkDownload.Interval = 10;
            this.checkDownload.Tick += new System.EventHandler(this.checkDownload_Tick);
            // 
            // multiDownloader
            // 
            this.multiDownloader.Tick += new System.EventHandler(this.multiDownloader_Tick);
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.Color.RoyalBlue;
            this.statusBar.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.statusBar.Controls.Add(this.appStatus);
            this.statusBar.Controls.Add(this.urlStatus);
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Location = new System.Drawing.Point(0, 609);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(716, 24);
            this.statusBar.TabIndex = 0;
            // 
            // appStatus
            // 
            this.appStatus.BackColor = System.Drawing.Color.Transparent;
            this.appStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.appStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.appStatus.Location = new System.Drawing.Point(0, 0);
            this.appStatus.Name = "appStatus";
            this.appStatus.Size = new System.Drawing.Size(400, 24);
            this.appStatus.TabIndex = 0;
            this.appStatus.Text = " 0 Apps found!";
            this.appStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // urlStatus
            // 
            this.urlStatus.BackColor = System.Drawing.Color.Transparent;
            this.urlStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.urlStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.urlStatus.Location = new System.Drawing.Point(511, 0);
            this.urlStatus.Name = "urlStatus";
            this.urlStatus.Size = new System.Drawing.Size(205, 24);
            this.urlStatus.TabIndex = 0;
            this.urlStatus.Text = "www.si13n7.com ";
            this.urlStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.urlStatus.Click += new System.EventHandler(this.urlStatus_Click);
            // 
            // statusBarBorder
            // 
            this.statusBarBorder.BackColor = System.Drawing.Color.Black;
            this.statusBarBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBarBorder.Location = new System.Drawing.Point(0, 608);
            this.statusBarBorder.Name = "statusBarBorder";
            this.statusBarBorder.Size = new System.Drawing.Size(716, 1);
            this.statusBarBorder.TabIndex = 1;
            // 
            // downloadStateAreaPanel
            // 
            this.downloadStateAreaPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.downloadStateAreaPanel.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.downloadStateAreaPanel.Controls.Add(this.downloadProgress);
            this.downloadStateAreaPanel.Controls.Add(this.buttonArea);
            this.downloadStateAreaPanel.Controls.Add(this.downloadReceived);
            this.downloadStateAreaPanel.Controls.Add(this.downloadSpeed);
            this.downloadStateAreaPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.downloadStateAreaPanel.Location = new System.Drawing.Point(0, 540);
            this.downloadStateAreaPanel.Name = "downloadStateAreaPanel";
            this.downloadStateAreaPanel.Size = new System.Drawing.Size(716, 68);
            this.downloadStateAreaPanel.TabIndex = 2;
            // 
            // downloadProgress
            // 
            this.downloadProgress.BackColor = System.Drawing.Color.WhiteSmoke;
            this.downloadProgress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.downloadProgress.Location = new System.Drawing.Point(26, 30);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(374, 10);
            this.downloadProgress.TabIndex = 6;
            this.downloadProgress.Visible = false;
            // 
            // buttonArea
            // 
            this.buttonArea.BackColor = System.Drawing.Color.Transparent;
            this.buttonArea.Controls.Add(this.cancelBtn);
            this.buttonArea.Controls.Add(this.okBtn);
            this.buttonArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonArea.Location = new System.Drawing.Point(508, 0);
            this.buttonArea.Name = "buttonArea";
            this.buttonArea.Size = new System.Drawing.Size(208, 68);
            this.buttonArea.TabIndex = 5;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(103, 24);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Enabled = false;
            this.okBtn.Location = new System.Drawing.Point(3, 24);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // downloadReceived
            // 
            this.downloadReceived.BackColor = System.Drawing.Color.Transparent;
            this.downloadReceived.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.downloadReceived.Location = new System.Drawing.Point(26, 43);
            this.downloadReceived.Name = "downloadReceived";
            this.downloadReceived.Size = new System.Drawing.Size(374, 13);
            this.downloadReceived.TabIndex = 2;
            this.downloadReceived.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadReceived.Visible = false;
            // 
            // downloadSpeed
            // 
            this.downloadSpeed.BackColor = System.Drawing.Color.Transparent;
            this.downloadSpeed.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.downloadSpeed.Location = new System.Drawing.Point(26, 14);
            this.downloadSpeed.Name = "downloadSpeed";
            this.downloadSpeed.Size = new System.Drawing.Size(374, 13);
            this.downloadSpeed.TabIndex = 1;
            this.downloadSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadSpeed.Visible = false;
            // 
            // settingsAreaBorder
            // 
            this.settingsAreaBorder.BackColor = System.Drawing.Color.Black;
            this.settingsAreaBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsAreaBorder.Location = new System.Drawing.Point(0, 539);
            this.settingsAreaBorder.Name = "settingsAreaBorder";
            this.settingsAreaBorder.Size = new System.Drawing.Size(716, 1);
            this.settingsAreaBorder.TabIndex = 3;
            // 
            // settingsArea
            // 
            this.settingsArea.BackColor = System.Drawing.Color.Transparent;
            this.settingsArea.Controls.Add(this.showColorsCheck);
            this.settingsArea.Controls.Add(this.showGroupsCheck);
            this.settingsArea.Controls.Add(this.searchArea);
            this.settingsArea.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsArea.Location = new System.Drawing.Point(0, 493);
            this.settingsArea.Name = "settingsArea";
            this.settingsArea.Size = new System.Drawing.Size(716, 46);
            this.settingsArea.TabIndex = 4;
            // 
            // showColorsCheck
            // 
            this.showColorsCheck.AutoSize = true;
            this.showColorsCheck.Checked = true;
            this.showColorsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showColorsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showColorsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.showColorsCheck.Location = new System.Drawing.Point(150, 13);
            this.showColorsCheck.Name = "showColorsCheck";
            this.showColorsCheck.Size = new System.Drawing.Size(132, 19);
            this.showColorsCheck.TabIndex = 3;
            this.showColorsCheck.Text = "Show Group Colors";
            this.showColorsCheck.UseVisualStyleBackColor = true;
            this.showColorsCheck.CheckedChanged += new System.EventHandler(this.showColorsCheck_CheckedChanged);
            // 
            // showGroupsCheck
            // 
            this.showGroupsCheck.AutoSize = true;
            this.showGroupsCheck.Checked = true;
            this.showGroupsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGroupsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showGroupsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.showGroupsCheck.Location = new System.Drawing.Point(14, 13);
            this.showGroupsCheck.Name = "showGroupsCheck";
            this.showGroupsCheck.Size = new System.Drawing.Size(100, 19);
            this.showGroupsCheck.TabIndex = 2;
            this.showGroupsCheck.Text = "Show Groups";
            this.showGroupsCheck.UseVisualStyleBackColor = true;
            this.showGroupsCheck.CheckedChanged += new System.EventHandler(this.showGroupsCheck_CheckedChanged);
            // 
            // searchArea
            // 
            this.searchArea.Controls.Add(this.searchLabel);
            this.searchArea.Controls.Add(this.searchBox);
            this.searchArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.searchArea.Location = new System.Drawing.Point(423, 0);
            this.searchArea.Name = "searchArea";
            this.searchArea.Size = new System.Drawing.Size(293, 46);
            this.searchArea.TabIndex = 1;
            // 
            // searchLabel
            // 
            this.searchLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.searchLabel.Location = new System.Drawing.Point(3, 14);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(75, 15);
            this.searchLabel.TabIndex = 1;
            this.searchLabel.Text = "Search:";
            this.searchLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // searchBox
            // 
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.Location = new System.Drawing.Point(80, 11);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(192, 22);
            this.searchBox.TabIndex = 0;
            this.searchBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // appsList
            // 
            this.appsList.BackColor = System.Drawing.Color.Silver;
            this.appsList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.appsList.CheckBoxes = true;
            this.appsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.appsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsList.ForeColor = System.Drawing.Color.Black;
            this.appsList.FullRowSelect = true;
            listViewGroup14.Header = "listViewGroup0";
            listViewGroup14.Name = "listViewGroup0";
            listViewGroup15.Header = "listViewGroup1";
            listViewGroup15.Name = "listViewGroup1";
            listViewGroup16.Header = "listViewGroup2";
            listViewGroup16.Name = "listViewGroup2";
            listViewGroup17.Header = "listViewGroup3";
            listViewGroup17.Name = "listViewGroup3";
            listViewGroup18.Header = "listViewGroup4";
            listViewGroup18.Name = "listViewGroup4";
            listViewGroup19.Header = "listViewGroup5";
            listViewGroup19.Name = "listViewGroup5";
            listViewGroup20.Header = "listViewGroup6";
            listViewGroup20.Name = "listViewGroup6";
            listViewGroup21.Header = "listViewGroup7";
            listViewGroup21.Name = "listViewGroup7";
            listViewGroup22.Header = "listViewGroup8";
            listViewGroup22.Name = "listViewGroup8";
            listViewGroup23.Header = "listViewGroup9";
            listViewGroup23.Name = "listViewGroup9";
            listViewGroup24.Header = "listViewGroup10";
            listViewGroup24.Name = "listViewGroup10";
            listViewGroup25.Header = "listViewGroup11";
            listViewGroup25.Name = "listViewGroup11";
            listViewGroup26.Header = "listViewGroup12";
            listViewGroup26.Name = "listViewGroup12";
            this.appsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup14,
            listViewGroup15,
            listViewGroup16,
            listViewGroup17,
            listViewGroup18,
            listViewGroup19,
            listViewGroup20,
            listViewGroup21,
            listViewGroup22,
            listViewGroup23,
            listViewGroup24,
            listViewGroup25,
            listViewGroup26});
            this.appsList.Location = new System.Drawing.Point(0, 0);
            this.appsList.MultiSelect = false;
            this.appsList.Name = "appsList";
            this.appsList.Size = new System.Drawing.Size(716, 493);
            this.appsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.appsList.TabIndex = 5;
            this.appsList.UseCompatibleStateImageBehavior = false;
            this.appsList.View = System.Windows.Forms.View.Details;
            this.appsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.appsList_ItemCheck);
            this.appsList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.appsList_ItemChecked);
            this.appsList.Enter += new System.EventHandler(this.appsList_Enter);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Application";
            this.columnHeader1.Width = 193;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 258;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Version";
            this.columnHeader3.Width = 72;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Size";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Source";
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader5.Width = 115;
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgList.ImageSize = new System.Drawing.Size(16, 16);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // searchResultBlinker
            // 
            this.searchResultBlinker.Interval = 300;
            this.searchResultBlinker.Tick += new System.EventHandler(this.searchResultBlinker_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(716, 633);
            this.Controls.Add(this.appsList);
            this.Controls.Add(this.settingsArea);
            this.Controls.Add(this.settingsAreaBorder);
            this.Controls.Add(this.downloadStateAreaPanel);
            this.Controls.Add(this.statusBarBorder);
            this.Controls.Add(this.statusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(700, 480);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Apps Downloader";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.statusBar.ResumeLayout(false);
            this.downloadStateAreaPanel.ResumeLayout(false);
            this.buttonArea.ResumeLayout(false);
            this.settingsArea.ResumeLayout(false);
            this.settingsArea.PerformLayout();
            this.searchArea.ResumeLayout(false);
            this.searchArea.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer checkDownload;
        private System.Windows.Forms.Timer multiDownloader;
        private System.Windows.Forms.Panel statusBar;
        private System.Windows.Forms.Panel statusBarBorder;
        private System.Windows.Forms.Panel downloadStateAreaPanel;
        private System.Windows.Forms.Panel settingsAreaBorder;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Label downloadReceived;
        private System.Windows.Forms.Label downloadSpeed;
        private System.Windows.Forms.Panel settingsArea;
        private System.Windows.Forms.ListView appsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label urlStatus;
        private System.Windows.Forms.Panel buttonArea;
        private System.Windows.Forms.Label appStatus;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Panel searchArea;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.CheckBox showGroupsCheck;
        private System.Windows.Forms.CheckBox showColorsCheck;
        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.Timer searchResultBlinker;
        private System.Windows.Forms.Panel downloadProgress;
    }
}

