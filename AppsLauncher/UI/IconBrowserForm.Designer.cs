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
            this.IconPanel = new System.Windows.Forms.Panel();
            this.ResourceFileBrowserBtn = new System.Windows.Forms.Button();
            this.ResourceFilePath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // IconPanel
            // 
            this.IconPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IconPanel.AutoScroll = true;
            this.IconPanel.BackColor = System.Drawing.SystemColors.Window;
            this.IconPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IconPanel.Location = new System.Drawing.Point(3, 3);
            this.IconPanel.Name = "IconPanel";
            this.IconPanel.Size = new System.Drawing.Size(666, 374);
            this.IconPanel.TabIndex = 0;
            this.IconPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.IconPanel_Scroll);
            // 
            // ResourceFileBrowserBtn
            // 
            this.ResourceFileBrowserBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResourceFileBrowserBtn.BackColor = System.Drawing.SystemColors.Control;
            this.ResourceFileBrowserBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ResourceFileBrowserBtn.FlatAppearance.BorderSize = 0;
            this.ResourceFileBrowserBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Highlight;
            this.ResourceFileBrowserBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResourceFileBrowserBtn.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResourceFileBrowserBtn.Location = new System.Drawing.Point(648, 381);
            this.ResourceFileBrowserBtn.Name = "ResourceFileBrowserBtn";
            this.ResourceFileBrowserBtn.Size = new System.Drawing.Size(20, 20);
            this.ResourceFileBrowserBtn.TabIndex = 2;
            this.ResourceFileBrowserBtn.UseVisualStyleBackColor = false;
            this.ResourceFileBrowserBtn.Click += new System.EventHandler(this.ResourceFileBrowserBtn_Click);
            // 
            // ResourceFilePath
            // 
            this.ResourceFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResourceFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ResourceFilePath.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResourceFilePath.Location = new System.Drawing.Point(4, 381);
            this.ResourceFilePath.Name = "ResourceFilePath";
            this.ResourceFilePath.Size = new System.Drawing.Size(640, 20);
            this.ResourceFilePath.TabIndex = 1;
            this.ResourceFilePath.TextChanged += new System.EventHandler(this.ResourceFilePath_TextChanged);
            // 
            // IconBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.ClientSize = new System.Drawing.Size(672, 409);
            this.Controls.Add(this.ResourceFileBrowserBtn);
            this.Controls.Add(this.IconPanel);
            this.Controls.Add(this.ResourceFilePath);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(680, 940);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(680, 440);
            this.Name = "IconBrowserForm";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.IconBrowserForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel IconPanel;
        private System.Windows.Forms.Button ResourceFileBrowserBtn;
        private System.Windows.Forms.TextBox ResourceFilePath;
    }
}