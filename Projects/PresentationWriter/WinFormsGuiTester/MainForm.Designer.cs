namespace WinFormsGuiTester
{
    partial class MainForm
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
            this.liveButton = new System.Windows.Forms.Button();
            this.filtersButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // liveButton
            // 
            this.liveButton.Location = new System.Drawing.Point(12, 12);
            this.liveButton.Name = "liveButton";
            this.liveButton.Size = new System.Drawing.Size(118, 22);
            this.liveButton.TabIndex = 0;
            this.liveButton.Text = "Live Pen Tracking";
            this.liveButton.UseVisualStyleBackColor = true;
            this.liveButton.Click += new System.EventHandler(this.liveButton_Click);
            // 
            // filtersButton
            // 
            this.filtersButton.Location = new System.Drawing.Point(12, 40);
            this.filtersButton.Name = "filtersButton";
            this.filtersButton.Size = new System.Drawing.Size(118, 22);
            this.filtersButton.TabIndex = 1;
            this.filtersButton.Text = "Filter Calibration";
            this.filtersButton.UseVisualStyleBackColor = true;
            this.filtersButton.Click += new System.EventHandler(this.filtersButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(143, 73);
            this.Controls.Add(this.filtersButton);
            this.Controls.Add(this.liveButton);
            this.Name = "MainForm";
            this.Text = "PW GUI Tests";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button liveButton;
        private System.Windows.Forms.Button filtersButton;
    }
}