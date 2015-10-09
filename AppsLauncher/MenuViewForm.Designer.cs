namespace AppsLauncher
{
    partial class MenuViewForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.appsListView = new System.Windows.Forms.ListView();
            this.appMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.appMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.appMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.appMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.closeBtn = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.appsCount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.aboutBtn = new System.Windows.Forms.PictureBox();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.downloadBtn = new System.Windows.Forms.Button();
            this.settingsBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.appMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aboutBtn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1, 400);
            this.label1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(399, 1);
            this.label2.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Dock = System.Windows.Forms.DockStyle.Right;
            this.label3.Location = new System.Drawing.Point(399, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1, 399);
            this.label3.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.White;
            this.label4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label4.Location = new System.Drawing.Point(1, 399);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(398, 1);
            this.label4.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 148F));
            this.tableLayoutPanel1.Controls.Add(this.appsListView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(398, 398);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // appsListView
            // 
            this.appsListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.appsListView.BackColor = System.Drawing.SystemColors.Control;
            this.appsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.appsListView.ContextMenuStrip = this.appMenu;
            this.appsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsListView.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsListView.ForeColor = System.Drawing.SystemColors.ControlText;
            this.appsListView.HotTracking = true;
            this.appsListView.HoverSelection = true;
            this.appsListView.LabelWrap = false;
            this.appsListView.Location = new System.Drawing.Point(3, 3);
            this.appsListView.MultiSelect = false;
            this.appsListView.Name = "appsListView";
            this.appsListView.Size = new System.Drawing.Size(244, 352);
            this.appsListView.TabIndex = 0;
            this.appsListView.UseCompatibleStateImageBehavior = false;
            this.appsListView.View = System.Windows.Forms.View.List;
            this.appsListView.ItemMouseHover += new System.Windows.Forms.ListViewItemMouseHoverEventHandler(this.appsListView_ItemMouseHover);
            this.appsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.appsListView_MouseClick);
            // 
            // appMenu
            // 
            this.appMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.appMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.appMenuItem1,
            this.appMenuItem2,
            this.toolStripSeparator2,
            this.appMenuItem3,
            this.appMenuItem4,
            this.toolStripSeparator3,
            this.appMenuItem5});
            this.appMenu.Name = "addMenu";
            this.appMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.appMenu.Size = new System.Drawing.Size(212, 126);
            this.appMenu.Opening += new System.ComponentModel.CancelEventHandler(this.appMenu_Opening);
            // 
            // appMenuItem1
            // 
            this.appMenuItem1.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem1.Name = "appMenuItem1";
            this.appMenuItem1.Size = new System.Drawing.Size(211, 22);
            this.appMenuItem1.Text = "Run";
            this.appMenuItem1.Click += new System.EventHandler(this.appMenuItem_Click);
            // 
            // appMenuItem2
            // 
            this.appMenuItem2.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem2.Image = global::AppsLauncher.Properties.Resources.uac_16;
            this.appMenuItem2.Name = "appMenuItem2";
            this.appMenuItem2.Size = new System.Drawing.Size(211, 22);
            this.appMenuItem2.Text = "Run as administrator";
            this.appMenuItem2.Click += new System.EventHandler(this.appMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(208, 6);
            // 
            // appMenuItem3
            // 
            this.appMenuItem3.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem3.Image = global::AppsLauncher.Properties.Resources.folder_16;
            this.appMenuItem3.Name = "appMenuItem3";
            this.appMenuItem3.Size = new System.Drawing.Size(211, 22);
            this.appMenuItem3.Text = "Open app location";
            this.appMenuItem3.Click += new System.EventHandler(this.appMenuItem_Click);
            // 
            // appMenuItem4
            // 
            this.appMenuItem4.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem4.Image = global::AppsLauncher.Properties.Resources.shortcut_16;
            this.appMenuItem4.Name = "appMenuItem4";
            this.appMenuItem4.Size = new System.Drawing.Size(211, 22);
            this.appMenuItem4.Text = "Create a Desktop Shortcut";
            this.appMenuItem4.Click += new System.EventHandler(this.appMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(208, 6);
            // 
            // appMenuItem5
            // 
            this.appMenuItem5.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem5.Image = global::AppsLauncher.Properties.Resources.recycle_16;
            this.appMenuItem5.Name = "appMenuItem5";
            this.appMenuItem5.Size = new System.Drawing.Size(211, 22);
            this.appMenuItem5.Text = "Delete";
            this.appMenuItem5.Click += new System.EventHandler(this.appMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.searchBox);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 361);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(244, 34);
            this.panel1.TabIndex = 1;
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(0, 5);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(225, 21);
            this.searchBox.TabIndex = 3;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.searchBox_KeyPress);
            this.searchBox.Leave += new System.EventHandler(this.searchBox_Leave);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::AppsLauncher.Properties.Resources.search_16;
            this.pictureBox2.Location = new System.Drawing.Point(228, 10);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(13, 13);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.closeBtn);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(253, 361);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(142, 34);
            this.panel2.TabIndex = 2;
            // 
            // closeBtn
            // 
            this.closeBtn.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.closeBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkRed;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Font = new System.Drawing.Font("Comic Sans MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.closeBtn.Location = new System.Drawing.Point(116, 5);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(23, 23);
            this.closeBtn.TabIndex = 1;
            this.closeBtn.Text = "X";
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Controls.Add(this.appsCount);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.aboutBtn);
            this.panel3.Controls.Add(this.logoBox);
            this.panel3.Controls.Add(this.downloadBtn);
            this.panel3.Controls.Add(this.settingsBtn);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(253, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(142, 352);
            this.panel3.TabIndex = 3;
            // 
            // appsCount
            // 
            this.appsCount.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsCount.ForeColor = System.Drawing.Color.Silver;
            this.appsCount.Location = new System.Drawing.Point(6, 87);
            this.appsCount.Name = "appsCount";
            this.appsCount.Size = new System.Drawing.Size(107, 14);
            this.appsCount.TabIndex = 2;
            this.appsCount.Text = "0 apps found!";
            this.appsCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(3, 1);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 19);
            this.label5.TabIndex = 7;
            this.label5.Text = "Apps Launcher";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // aboutBtn
            // 
            this.aboutBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.aboutBtn.Image = global::AppsLauncher.Properties.Resources.help_gray_16;
            this.aboutBtn.Location = new System.Drawing.Point(119, 0);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(23, 23);
            this.aboutBtn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.aboutBtn.TabIndex = 6;
            this.aboutBtn.TabStop = false;
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            this.aboutBtn.MouseEnter += new System.EventHandler(this.aboutBtn_MouseEnter);
            this.aboutBtn.MouseLeave += new System.EventHandler(this.aboutBtn_MouseLeave);
            // 
            // logoBox
            // 
            this.logoBox.Location = new System.Drawing.Point(7, 23);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(106, 64);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoBox.TabIndex = 5;
            this.logoBox.TabStop = false;
            // 
            // downloadBtn
            // 
            this.downloadBtn.BackColor = System.Drawing.SystemColors.ControlDark;
            this.downloadBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.downloadBtn.FlatAppearance.BorderSize = 0;
            this.downloadBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.downloadBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.downloadBtn.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downloadBtn.Location = new System.Drawing.Point(0, 298);
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(139, 23);
            this.downloadBtn.TabIndex = 3;
            this.downloadBtn.Text = "Download";
            this.downloadBtn.UseVisualStyleBackColor = false;
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // settingsBtn
            // 
            this.settingsBtn.BackColor = System.Drawing.SystemColors.ControlDark;
            this.settingsBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.settingsBtn.FlatAppearance.BorderSize = 0;
            this.settingsBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.settingsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsBtn.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsBtn.Location = new System.Drawing.Point(0, 327);
            this.settingsBtn.Name = "settingsBtn";
            this.settingsBtn.Size = new System.Drawing.Size(139, 23);
            this.settingsBtn.TabIndex = 2;
            this.settingsBtn.Text = "Settings";
            this.settingsBtn.UseVisualStyleBackColor = false;
            this.settingsBtn.Click += new System.EventHandler(this.settingsBtn_Click);
            // 
            // MenuViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MenuViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Apps Launcher";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.MenuViewForm_Activated);
            this.Deactivate += new System.EventHandler(this.MenuViewForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuViewForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuViewForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.appMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aboutBtn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView appsListView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Button downloadBtn;
        private System.Windows.Forms.Button settingsBtn;
        private System.Windows.Forms.PictureBox logoBox;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox aboutBtn;
        private System.Windows.Forms.ContextMenuStrip appMenu;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label appsCount;
    }
}