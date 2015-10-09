namespace AppsLauncher
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
            this.appMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.appMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.appMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.appMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.RunCmdLine = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.notifyIconDisabler = new System.ComponentModel.BackgroundWorker();
            this.aboutBtn = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.addBtn = new System.Windows.Forms.Button();
            this.appsBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.appsCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.settingsBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.appMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aboutBtn)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
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
            this.appMenu.Opening += new System.ComponentModel.CancelEventHandler(this.appMenuItem_Opening);
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
            // RunCmdLine
            // 
            this.RunCmdLine.Tick += new System.EventHandler(this.RunCmdLine_Tick);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "Portable Apps launcher";
            this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // notifyIconDisabler
            // 
            this.notifyIconDisabler.WorkerSupportsCancellation = true;
            this.notifyIconDisabler.DoWork += new System.ComponentModel.DoWorkEventHandler(this.notifyIconDisabler_DoWork);
            this.notifyIconDisabler.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.notifyIconDisabler_RunWorkerCompleted);
            // 
            // aboutBtn
            // 
            this.aboutBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.aboutBtn.Image = global::AppsLauncher.Properties.Resources.help_gray_16;
            this.aboutBtn.Location = new System.Drawing.Point(308, 3);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(18, 19);
            this.aboutBtn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.aboutBtn.TabIndex = 9;
            this.aboutBtn.TabStop = false;
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            this.aboutBtn.MouseEnter += new System.EventHandler(this.aboutBtn_MouseEnter);
            this.aboutBtn.MouseLeave += new System.EventHandler(this.aboutBtn_MouseLeave);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Controls.Add(this.searchBox, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(27, 69);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(275, 29);
            this.tableLayoutPanel6.TabIndex = 8;
            // 
            // searchBox
            // 
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchBox.Font = new System.Drawing.Font("Comic Sans MS", 8.25F);
            this.searchBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.searchBox.Location = new System.Drawing.Point(3, 3);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(269, 23);
            this.searchBox.TabIndex = 2;
            this.searchBox.Text = "S E A R C H";
            this.searchBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.searchBox_KeyPress);
            this.searchBox.Leave += new System.EventHandler(this.searchBox_Leave);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel5.Controls.Add(this.addBtn, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.appsBox, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(27, 34);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(275, 29);
            this.tableLayoutPanel5.TabIndex = 7;
            // 
            // addBtn
            // 
            this.addBtn.Image = global::AppsLauncher.Properties.Resources.add_a_13;
            this.addBtn.Location = new System.Drawing.Point(251, 3);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(21, 21);
            this.addBtn.TabIndex = 1;
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            this.addBtn.MouseEnter += new System.EventHandler(this.addBtn_MouseEnter);
            this.addBtn.MouseLeave += new System.EventHandler(this.addBtn_MouseLeave);
            // 
            // appsBox
            // 
            this.appsBox.ContextMenuStrip = this.appMenu;
            this.appsBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.appsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.appsBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsBox.FormattingEnabled = true;
            this.appsBox.Location = new System.Drawing.Point(3, 3);
            this.appsBox.Name = "appsBox";
            this.appsBox.Size = new System.Drawing.Size(242, 21);
            this.appsBox.Sorted = true;
            this.appsBox.TabIndex = 0;
            this.appsBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.appsBox_KeyPress);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.appsCount, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(27, 136);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(275, 14);
            this.tableLayoutPanel4.TabIndex = 6;
            // 
            // appsCount
            // 
            this.appsCount.BackColor = System.Drawing.Color.Transparent;
            this.appsCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsCount.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appsCount.ForeColor = System.Drawing.SystemColors.GrayText;
            this.appsCount.Location = new System.Drawing.Point(140, 0);
            this.appsCount.Name = "appsCount";
            this.appsCount.Size = new System.Drawing.Size(132, 14);
            this.appsCount.TabIndex = 6;
            this.appsCount.Text = "0 apps found!";
            this.appsCount.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 14);
            this.label1.TabIndex = 5;
            this.label1.Text = "www.si13n7.com";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.settingsBtn, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.startBtn, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(27, 104);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(275, 26);
            this.tableLayoutPanel3.TabIndex = 5;
            // 
            // settingsBtn
            // 
            this.settingsBtn.BackColor = System.Drawing.SystemColors.Control;
            this.settingsBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsBtn.FlatAppearance.BorderSize = 0;
            this.settingsBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.settingsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsBtn.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.settingsBtn.Location = new System.Drawing.Point(140, 3);
            this.settingsBtn.Name = "settingsBtn";
            this.settingsBtn.Size = new System.Drawing.Size(132, 20);
            this.settingsBtn.TabIndex = 4;
            this.settingsBtn.Text = "Settings";
            this.settingsBtn.UseVisualStyleBackColor = false;
            this.settingsBtn.Click += new System.EventHandler(this.settingsBtn_Click);
            // 
            // startBtn
            // 
            this.startBtn.BackColor = System.Drawing.SystemColors.Control;
            this.startBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.startBtn.FlatAppearance.BorderSize = 0;
            this.startBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.startBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startBtn.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.startBtn.Location = new System.Drawing.Point(3, 3);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(131, 20);
            this.startBtn.TabIndex = 3;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = false;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.aboutBtn, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65.21739F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.78261F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(329, 171);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel1.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 171);
            this.panel1.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(329, 171);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(760, 210);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(345, 210);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Portable Apps Launcher";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.appMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.aboutBtn)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer RunCmdLine;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolTip toolTip;
        private System.ComponentModel.BackgroundWorker notifyIconDisabler;
        private System.Windows.Forms.ContextMenuStrip appMenu;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem5;
        private System.Windows.Forms.PictureBox aboutBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.ComboBox appsBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label appsCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button settingsBtn;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
    }
}

