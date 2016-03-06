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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("listViewGroup0", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("listViewGroup1", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("listViewGroup2", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("listViewGroup3", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("listViewGroup4", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("listViewGroup5", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("listViewGroup6", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("listViewGroup7", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup9 = new System.Windows.Forms.ListViewGroup("listViewGroup8", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup10 = new System.Windows.Forms.ListViewGroup("listViewGroup9", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup11 = new System.Windows.Forms.ListViewGroup("listViewGroup10", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup12 = new System.Windows.Forms.ListViewGroup("listViewGroup11", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup13 = new System.Windows.Forms.ListViewGroup("listViewGroup12", System.Windows.Forms.HorizontalAlignment.Left);
            this.CheckDownload = new System.Windows.Forms.Timer(this.components);
            this.MultiDownloader = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.AppStatus = new System.Windows.Forms.Label();
            this.UrlStatus = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.DLPercentage = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OKBtn = new System.Windows.Forms.Button();
            this.DLLoaded = new System.Windows.Forms.Label();
            this.DLSpeed = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.ShowColorsCheck = new System.Windows.Forms.CheckBox();
            this.ShowGroupsCheck = new System.Windows.Forms.CheckBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.SearchLabel = new System.Windows.Forms.Label();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.AppList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.SearchResultBlinker = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel7.SuspendLayout();
            this.SuspendLayout();
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
            this.panel1.BackColor = System.Drawing.Color.RoyalBlue;
            this.panel1.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.panel1.Controls.Add(this.AppStatus);
            this.panel1.Controls.Add(this.UrlStatus);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 609);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(716, 24);
            this.panel1.TabIndex = 0;
            // 
            // AppStatus
            // 
            this.AppStatus.BackColor = System.Drawing.Color.Transparent;
            this.AppStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.AppStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AppStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.AppStatus.Location = new System.Drawing.Point(0, 0);
            this.AppStatus.Name = "AppStatus";
            this.AppStatus.Size = new System.Drawing.Size(400, 24);
            this.AppStatus.TabIndex = 0;
            this.AppStatus.Text = " 0 Apps found!";
            this.AppStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UrlStatus
            // 
            this.UrlStatus.BackColor = System.Drawing.Color.Transparent;
            this.UrlStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.UrlStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UrlStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.UrlStatus.Location = new System.Drawing.Point(511, 0);
            this.UrlStatus.Name = "UrlStatus";
            this.UrlStatus.Size = new System.Drawing.Size(205, 24);
            this.UrlStatus.TabIndex = 0;
            this.UrlStatus.Text = "www.si13n7.com ";
            this.UrlStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.UrlStatus.Click += new System.EventHandler(this.UrlStatus_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 608);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(716, 1);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel3.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.panel3.Controls.Add(this.DLPercentage);
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Controls.Add(this.DLLoaded);
            this.panel3.Controls.Add(this.DLSpeed);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 540);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(716, 68);
            this.panel3.TabIndex = 2;
            // 
            // DLPercentage
            // 
            this.DLPercentage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.DLPercentage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DLPercentage.Location = new System.Drawing.Point(26, 30);
            this.DLPercentage.Name = "DLPercentage";
            this.DLPercentage.Size = new System.Drawing.Size(374, 10);
            this.DLPercentage.TabIndex = 6;
            this.DLPercentage.Visible = false;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.Transparent;
            this.panel6.Controls.Add(this.CancelBtn);
            this.panel6.Controls.Add(this.OKBtn);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel6.Location = new System.Drawing.Point(508, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(208, 68);
            this.panel6.TabIndex = 5;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(103, 24);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // OKBtn
            // 
            this.OKBtn.Enabled = false;
            this.OKBtn.Location = new System.Drawing.Point(3, 24);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // DLLoaded
            // 
            this.DLLoaded.BackColor = System.Drawing.Color.Transparent;
            this.DLLoaded.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.DLLoaded.Location = new System.Drawing.Point(26, 43);
            this.DLLoaded.Name = "DLLoaded";
            this.DLLoaded.Size = new System.Drawing.Size(374, 13);
            this.DLLoaded.TabIndex = 2;
            this.DLLoaded.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DLLoaded.Visible = false;
            // 
            // DLSpeed
            // 
            this.DLSpeed.BackColor = System.Drawing.Color.Transparent;
            this.DLSpeed.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.DLSpeed.Location = new System.Drawing.Point(26, 14);
            this.DLSpeed.Name = "DLSpeed";
            this.DLSpeed.Size = new System.Drawing.Size(374, 13);
            this.DLSpeed.TabIndex = 1;
            this.DLSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DLSpeed.Visible = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Black;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 539);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(716, 1);
            this.panel4.TabIndex = 3;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.Controls.Add(this.ShowColorsCheck);
            this.panel5.Controls.Add(this.ShowGroupsCheck);
            this.panel5.Controls.Add(this.panel7);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 493);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(716, 46);
            this.panel5.TabIndex = 4;
            // 
            // ShowColorsCheck
            // 
            this.ShowColorsCheck.AutoSize = true;
            this.ShowColorsCheck.Checked = true;
            this.ShowColorsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowColorsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowColorsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ShowColorsCheck.Location = new System.Drawing.Point(150, 13);
            this.ShowColorsCheck.Name = "ShowColorsCheck";
            this.ShowColorsCheck.Size = new System.Drawing.Size(132, 19);
            this.ShowColorsCheck.TabIndex = 3;
            this.ShowColorsCheck.Text = "Show Group Colors";
            this.ShowColorsCheck.UseVisualStyleBackColor = true;
            this.ShowColorsCheck.CheckedChanged += new System.EventHandler(this.ShowColorsCheck_CheckedChanged);
            // 
            // ShowGroupsCheck
            // 
            this.ShowGroupsCheck.AutoSize = true;
            this.ShowGroupsCheck.Checked = true;
            this.ShowGroupsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowGroupsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowGroupsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ShowGroupsCheck.Location = new System.Drawing.Point(14, 13);
            this.ShowGroupsCheck.Name = "ShowGroupsCheck";
            this.ShowGroupsCheck.Size = new System.Drawing.Size(100, 19);
            this.ShowGroupsCheck.TabIndex = 2;
            this.ShowGroupsCheck.Text = "Show Groups";
            this.ShowGroupsCheck.UseVisualStyleBackColor = true;
            this.ShowGroupsCheck.CheckedChanged += new System.EventHandler(this.ShowGroupsCheck_CheckedChanged);
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.SearchLabel);
            this.panel7.Controls.Add(this.SearchBox);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel7.Location = new System.Drawing.Point(423, 0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(293, 46);
            this.panel7.TabIndex = 1;
            // 
            // SearchLabel
            // 
            this.SearchLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.SearchLabel.Location = new System.Drawing.Point(3, 14);
            this.SearchLabel.Name = "SearchLabel";
            this.SearchLabel.Size = new System.Drawing.Size(75, 15);
            this.SearchLabel.TabIndex = 1;
            this.SearchLabel.Text = "Search:";
            this.SearchLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SearchBox
            // 
            this.SearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SearchBox.Location = new System.Drawing.Point(80, 11);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(192, 22);
            this.SearchBox.TabIndex = 0;
            this.SearchBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.SearchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            this.SearchBox.Enter += new System.EventHandler(this.SearchBox_Enter);
            this.SearchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchBox_KeyDown);
            // 
            // AppList
            // 
            this.AppList.BackColor = System.Drawing.Color.Silver;
            this.AppList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AppList.CheckBoxes = true;
            this.AppList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.AppList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AppList.ForeColor = System.Drawing.Color.Black;
            this.AppList.FullRowSelect = true;
            listViewGroup1.Header = "listViewGroup0";
            listViewGroup1.Name = "listViewGroup0";
            listViewGroup2.Header = "listViewGroup1";
            listViewGroup2.Name = "listViewGroup1";
            listViewGroup3.Header = "listViewGroup2";
            listViewGroup3.Name = "listViewGroup2";
            listViewGroup4.Header = "listViewGroup3";
            listViewGroup4.Name = "listViewGroup3";
            listViewGroup5.Header = "listViewGroup4";
            listViewGroup5.Name = "listViewGroup4";
            listViewGroup6.Header = "listViewGroup5";
            listViewGroup6.Name = "listViewGroup5";
            listViewGroup7.Header = "listViewGroup6";
            listViewGroup7.Name = "listViewGroup6";
            listViewGroup8.Header = "listViewGroup7";
            listViewGroup8.Name = "listViewGroup7";
            listViewGroup9.Header = "listViewGroup8";
            listViewGroup9.Name = "listViewGroup8";
            listViewGroup10.Header = "listViewGroup9";
            listViewGroup10.Name = "listViewGroup9";
            listViewGroup11.Header = "listViewGroup10";
            listViewGroup11.Name = "listViewGroup10";
            listViewGroup12.Header = "listViewGroup11";
            listViewGroup12.Name = "listViewGroup11";
            listViewGroup13.Header = "listViewGroup12";
            listViewGroup13.Name = "listViewGroup12";
            this.AppList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4,
            listViewGroup5,
            listViewGroup6,
            listViewGroup7,
            listViewGroup8,
            listViewGroup9,
            listViewGroup10,
            listViewGroup11,
            listViewGroup12,
            listViewGroup13});
            this.AppList.Location = new System.Drawing.Point(0, 0);
            this.AppList.MultiSelect = false;
            this.AppList.Name = "AppList";
            this.AppList.Size = new System.Drawing.Size(716, 493);
            this.AppList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.AppList.TabIndex = 5;
            this.AppList.UseCompatibleStateImageBehavior = false;
            this.AppList.View = System.Windows.Forms.View.Details;
            this.AppList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AppList_ItemCheck);
            this.AppList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.AppList_ItemChecked);
            this.AppList.Enter += new System.EventHandler(this.AppList_Enter);
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
            // SearchResultBlinker
            // 
            this.SearchResultBlinker.Interval = 300;
            this.SearchResultBlinker.Tick += new System.EventHandler(this.SearchResultBlinker_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(716, 633);
            this.Controls.Add(this.AppList);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(700, 480);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Apps Downloader";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer CheckDownload;
        private System.Windows.Forms.Timer MultiDownloader;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Label DLLoaded;
        private System.Windows.Forms.Label DLSpeed;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ListView AppList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label UrlStatus;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label AppStatus;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label SearchLabel;
        private System.Windows.Forms.CheckBox ShowGroupsCheck;
        private System.Windows.Forms.CheckBox ShowColorsCheck;
        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.Timer SearchResultBlinker;
        private System.Windows.Forms.Panel DLPercentage;
    }
}

