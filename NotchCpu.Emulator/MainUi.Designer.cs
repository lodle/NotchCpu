namespace NotchCpu.Emulator
{
    partial class MainUi
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
            this.TextGridView = new System.Windows.Forms.DataGridView();
            this.ButStartToggle = new System.Windows.Forms.Button();
            this.TBLog = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.TextGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // TextGridView
            // 
            this.TextGridView.AllowUserToAddRows = false;
            this.TextGridView.AllowUserToDeleteRows = false;
            this.TextGridView.AllowUserToResizeColumns = false;
            this.TextGridView.AllowUserToResizeRows = false;
            this.TextGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TextGridView.ColumnHeadersVisible = false;
            this.TextGridView.Location = new System.Drawing.Point(13, 13);
            this.TextGridView.MaximumSize = new System.Drawing.Size(485, 290);
            this.TextGridView.Name = "TextGridView";
            this.TextGridView.ReadOnly = true;
            this.TextGridView.RowHeadersVisible = false;
            this.TextGridView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.TextGridView.Size = new System.Drawing.Size(485, 290);
            this.TextGridView.TabIndex = 0;
            // 
            // ButStartToggle
            // 
            this.ButStartToggle.Location = new System.Drawing.Point(514, 13);
            this.ButStartToggle.Name = "ButStartToggle";
            this.ButStartToggle.Size = new System.Drawing.Size(75, 23);
            this.ButStartToggle.TabIndex = 1;
            this.ButStartToggle.Text = "Start";
            this.ButStartToggle.UseVisualStyleBackColor = true;
            this.ButStartToggle.Click += new System.EventHandler(this.ButStartToggle_Click);
            // 
            // TBLog
            // 
            this.TBLog.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TBLog.Location = new System.Drawing.Point(13, 310);
            this.TBLog.Multiline = true;
            this.TBLog.Name = "TBLog";
            this.TBLog.ReadOnly = true;
            this.TBLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TBLog.Size = new System.Drawing.Size(485, 136);
            this.TBLog.TabIndex = 2;
            // 
            // MainUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 454);
            this.Controls.Add(this.TBLog);
            this.Controls.Add(this.ButStartToggle);
            this.Controls.Add(this.TextGridView);
            this.Name = "MainUi";
            this.Text = "Notch Cpu Emulator";
            this.Load += new System.EventHandler(this.MainUi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.TextGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView TextGridView;
        private System.Windows.Forms.Button ButStartToggle;
        private System.Windows.Forms.TextBox TBLog;
    }
}