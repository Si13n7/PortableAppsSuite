namespace AppsLauncher
{
    partial class IconBrowserForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.IconPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ResourceFileBrowserBtn = new System.Windows.Forms.Button();
            this.ResourceFilePath = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.IconPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(674, 419);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // IconPanel
            // 
            this.IconPanel.AutoScroll = true;
            this.IconPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.IconPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IconPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IconPanel.Location = new System.Drawing.Point(3, 4);
            this.IconPanel.Name = "IconPanel";
            this.IconPanel.Size = new System.Drawing.Size(668, 367);
            this.IconPanel.TabIndex = 0;
            this.IconPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.IconPanel_Scroll);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ResourceFileBrowserBtn);
            this.panel1.Controls.Add(this.ResourceFilePath);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 377);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(668, 39);
            this.panel1.TabIndex = 1;
            // 
            // ResourceFileBrowserBtn
            // 
            this.ResourceFileBrowserBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ResourceFileBrowserBtn.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResourceFileBrowserBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ResourceFileBrowserBtn.Location = new System.Drawing.Point(636, 7);
            this.ResourceFileBrowserBtn.Name = "ResourceFileBrowserBtn";
            this.ResourceFileBrowserBtn.Size = new System.Drawing.Size(24, 24);
            this.ResourceFileBrowserBtn.TabIndex = 1;
            this.ResourceFileBrowserBtn.UseVisualStyleBackColor = false;
            this.ResourceFileBrowserBtn.Click += new System.EventHandler(this.ResourceFileBrowserBtn_Click);
            // 
            // ResourceFilePath
            // 
            this.ResourceFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ResourceFilePath.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResourceFilePath.Location = new System.Drawing.Point(9, 9);
            this.ResourceFilePath.Name = "ResourceFilePath";
            this.ResourceFilePath.Size = new System.Drawing.Size(622, 20);
            this.ResourceFilePath.TabIndex = 0;
            this.ResourceFilePath.TextChanged += new System.EventHandler(this.ResourceFilePath_TextChanged);
            // 
            // IconBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BackgroundImage = global::AppsLauncher.Properties.Resources.diagonal_pattern;
            this.ClientSize = new System.Drawing.Size(674, 419);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(680, 448);
            this.MinimizeBox = false;
            this.Name = "IconBrowserForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.IconBrowserForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel IconPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ResourceFileBrowserBtn;
        private System.Windows.Forms.TextBox ResourceFilePath;
    }
}