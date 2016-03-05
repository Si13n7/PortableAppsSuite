namespace AppsDownloader
{
    partial class LangSelectionForm
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
            this.LangBox = new System.Windows.Forms.ComboBox();
            this.NoLangQuestionCheck = new System.Windows.Forms.CheckBox();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.AppNameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LangBox
            // 
            this.LangBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LangBox.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LangBox.FormattingEnabled = true;
            this.LangBox.Location = new System.Drawing.Point(13, 37);
            this.LangBox.Name = "LangBox";
            this.LangBox.Size = new System.Drawing.Size(224, 21);
            this.LangBox.TabIndex = 0;
            // 
            // NoLangQuestionCheck
            // 
            this.NoLangQuestionCheck.BackColor = System.Drawing.Color.Transparent;
            this.NoLangQuestionCheck.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoLangQuestionCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.NoLangQuestionCheck.Location = new System.Drawing.Point(15, 65);
            this.NoLangQuestionCheck.Name = "NoLangQuestionCheck";
            this.NoLangQuestionCheck.Size = new System.Drawing.Size(222, 17);
            this.NoLangQuestionCheck.TabIndex = 1;
            this.NoLangQuestionCheck.Text = "Do not ask again for this application";
            this.NoLangQuestionCheck.UseVisualStyleBackColor = false;
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(21, 90);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(99, 23);
            this.OKBtn.TabIndex = 2;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(130, 90);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(99, 23);
            this.CancelBtn.TabIndex = 3;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // AppNameLabel
            // 
            this.AppNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.AppNameLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AppNameLabel.Font = new System.Drawing.Font("Tahoma", 9.25F, System.Drawing.FontStyle.Bold);
            this.AppNameLabel.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.AppNameLabel.Location = new System.Drawing.Point(0, 0);
            this.AppNameLabel.Name = "AppNameLabel";
            this.AppNameLabel.Size = new System.Drawing.Size(249, 28);
            this.AppNameLabel.TabIndex = 4;
            this.AppNameLabel.Text = "Application Name";
            this.AppNameLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // LangSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.ClientSize = new System.Drawing.Size(249, 133);
            this.Controls.Add(this.AppNameLabel);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.NoLangQuestionCheck);
            this.Controls.Add(this.LangBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LangSelectionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Language Selection:";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.SetArchiveLangForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox LangBox;
        private System.Windows.Forms.CheckBox NoLangQuestionCheck;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label AppNameLabel;
    }
}