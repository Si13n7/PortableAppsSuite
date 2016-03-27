namespace AppsLauncher
{
    partial class IconResourceBox
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.iconSelectBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // iconSelectBtn
            // 
            this.iconSelectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.iconSelectBtn.FlatAppearance.BorderSize = 0;
            this.iconSelectBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.iconSelectBtn.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Highlight;
            this.iconSelectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iconSelectBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.iconSelectBtn.Location = new System.Drawing.Point(3, 3);
            this.iconSelectBtn.Name = "iconSelectBtn";
            this.iconSelectBtn.Size = new System.Drawing.Size(52, 56);
            this.iconSelectBtn.TabIndex = 2;
            this.iconSelectBtn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.iconSelectBtn.UseVisualStyleBackColor = false;
            this.iconSelectBtn.Click += new System.EventHandler(this.iconSelectBtn_Click);
            // 
            // IconResourceBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.iconSelectBtn);
            this.Name = "IconResourceBox";
            this.Size = new System.Drawing.Size(58, 62);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button iconSelectBtn;
    }
}
