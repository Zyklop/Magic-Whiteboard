namespace WinFormsGuiTester
{
    partial class LiveTestForm
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
            this.toggleCameraButton = new System.Windows.Forms.Button();
            this.toggleParserButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cameraPictureBox = new System.Windows.Forms.PictureBox();
            this.foundPointLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // toggleCameraButton
            // 
            this.toggleCameraButton.Location = new System.Drawing.Point(12, 12);
            this.toggleCameraButton.Name = "toggleCameraButton";
            this.toggleCameraButton.Size = new System.Drawing.Size(156, 23);
            this.toggleCameraButton.TabIndex = 0;
            this.toggleCameraButton.Text = "Start Camera";
            this.toggleCameraButton.UseVisualStyleBackColor = true;
            this.toggleCameraButton.Click += new System.EventHandler(this.toggleCameraButton_Click);
            // 
            // toggleParserButton
            // 
            this.toggleParserButton.Location = new System.Drawing.Point(12, 41);
            this.toggleParserButton.Name = "toggleParserButton";
            this.toggleParserButton.Size = new System.Drawing.Size(156, 23);
            this.toggleParserButton.TabIndex = 1;
            this.toggleParserButton.Text = "Start Parser";
            this.toggleParserButton.UseVisualStyleBackColor = true;
            this.toggleParserButton.Click += new System.EventHandler(this.toggleParserButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.foundPointLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 70);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(156, 422);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Resultate:";
            // 
            // pictureBox1
            // 
            this.cameraPictureBox.Location = new System.Drawing.Point(174, 12);
            this.cameraPictureBox.Name = "pictureBox1";
            this.cameraPictureBox.Size = new System.Drawing.Size(640, 480);
            this.cameraPictureBox.TabIndex = 3;
            this.cameraPictureBox.TabStop = false;
            // 
            // foundPointLabel
            // 
            this.foundPointLabel.AutoSize = true;
            this.foundPointLabel.Location = new System.Drawing.Point(6, 16);
            this.foundPointLabel.Name = "foundPointLabel";
            this.foundPointLabel.Size = new System.Drawing.Size(91, 13);
            this.foundPointLabel.TabIndex = 0;
            this.foundPointLabel.Text = "Fount Point: none";
            // 
            // LiveTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 497);
            this.Controls.Add(this.cameraPictureBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toggleParserButton);
            this.Controls.Add(this.toggleCameraButton);
            this.Name = "LiveTestForm";
            this.Text = "LiveTestForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button toggleCameraButton;
        private System.Windows.Forms.Button toggleParserButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label foundPointLabel;
        private System.Windows.Forms.PictureBox cameraPictureBox;
    }
}