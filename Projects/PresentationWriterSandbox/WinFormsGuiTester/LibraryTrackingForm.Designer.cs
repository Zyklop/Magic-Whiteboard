namespace WinFormsGuiTester
{
    partial class LibraryTrackingForm
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
            this.folderButton = new System.Windows.Forms.Button();
            this.framePictureBox = new System.Windows.Forms.PictureBox();
            this.inputListBox = new System.Windows.Forms.ListBox();
            this.pathLabel = new System.Windows.Forms.Label();
            this.playButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.framePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // folderButton
            // 
            this.folderButton.Location = new System.Drawing.Point(156, 8);
            this.folderButton.Name = "folderButton";
            this.folderButton.Size = new System.Drawing.Size(40, 23);
            this.folderButton.TabIndex = 9;
            this.folderButton.Text = "Path";
            this.folderButton.UseVisualStyleBackColor = true;
            this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
            // 
            // framePictureBox
            // 
            this.framePictureBox.Location = new System.Drawing.Point(202, 8);
            this.framePictureBox.Name = "framePictureBox";
            this.framePictureBox.Size = new System.Drawing.Size(640, 480);
            this.framePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.framePictureBox.TabIndex = 8;
            this.framePictureBox.TabStop = false;
            // 
            // inputListBox
            // 
            this.inputListBox.FormattingEnabled = true;
            this.inputListBox.Location = new System.Drawing.Point(12, 37);
            this.inputListBox.Name = "inputListBox";
            this.inputListBox.Size = new System.Drawing.Size(184, 420);
            this.inputListBox.TabIndex = 7;
            this.inputListBox.SelectedIndexChanged += new System.EventHandler(this.inputListBox_SelectedIndexChanged);
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(9, 13);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(44, 13);
            this.pathLabel.TabIndex = 6;
            this.pathLabel.Text = "c:\\temp";
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(156, 462);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(40, 23);
            this.playButton.TabIndex = 10;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            // 
            // processButton
            // 
            this.processButton.Location = new System.Drawing.Point(12, 462);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(78, 23);
            this.processButton.TabIndex = 11;
            this.processButton.Text = "Process All";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.processButton_Click);
            // 
            // LibraryTrackingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 497);
            this.Controls.Add(this.processButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.folderButton);
            this.Controls.Add(this.framePictureBox);
            this.Controls.Add(this.inputListBox);
            this.Controls.Add(this.pathLabel);
            this.Name = "LibraryTrackingForm";
            this.Text = "LibraryTrackingForm";
            ((System.ComponentModel.ISupportInitialize)(this.framePictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button folderButton;
        private System.Windows.Forms.PictureBox framePictureBox;
        private System.Windows.Forms.ListBox inputListBox;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button processButton;
    }
}