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
            this.libraryTrackingButton = new System.Windows.Forms.Button();
            this.liveTestButton = new System.Windows.Forms.Button();
            this.showGridButton = new System.Windows.Forms.Button();
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
            // libraryTrackingButton
            // 
            this.libraryTrackingButton.Location = new System.Drawing.Point(12, 68);
            this.libraryTrackingButton.Name = "libraryTrackingButton";
            this.libraryTrackingButton.Size = new System.Drawing.Size(118, 22);
            this.libraryTrackingButton.TabIndex = 2;
            this.libraryTrackingButton.Text = "Library Tracking";
            this.libraryTrackingButton.UseVisualStyleBackColor = true;
            this.libraryTrackingButton.Click += new System.EventHandler(this.libraryTrackingButton_Click);
            // 
            // liveTestButton
            // 
            this.liveTestButton.Location = new System.Drawing.Point(12, 96);
            this.liveTestButton.Name = "liveTestButton";
            this.liveTestButton.Size = new System.Drawing.Size(118, 22);
            this.liveTestButton.TabIndex = 3;
            this.liveTestButton.Text = "Live Test";
            this.liveTestButton.UseVisualStyleBackColor = true;
            this.liveTestButton.Click += new System.EventHandler(this.liveTestButton_Click);
            // 
            // showGridButton
            // 
            this.showGridButton.Location = new System.Drawing.Point(13, 124);
            this.showGridButton.Name = "showGridButton";
            this.showGridButton.Size = new System.Drawing.Size(118, 22);
            this.showGridButton.TabIndex = 4;
            this.showGridButton.Text = "Show Grid";
            this.showGridButton.UseVisualStyleBackColor = true;
            this.showGridButton.Click += new System.EventHandler(this.showGridButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(143, 155);
            this.Controls.Add(this.showGridButton);
            this.Controls.Add(this.liveTestButton);
            this.Controls.Add(this.libraryTrackingButton);
            this.Controls.Add(this.filtersButton);
            this.Controls.Add(this.liveButton);
            this.Name = "MainForm";
            this.Text = "PW GUI Tests";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button liveButton;
        private System.Windows.Forms.Button filtersButton;
        private System.Windows.Forms.Button libraryTrackingButton;
        private System.Windows.Forms.Button liveTestButton;
        private System.Windows.Forms.Button showGridButton;
    }
}