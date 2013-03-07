namespace WinFormsGuiTester
{
    partial class Form1
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
            this.calibrationPictureBox = new System.Windows.Forms.PictureBox();
            this.diffDebugPicture = new System.Windows.Forms.PictureBox();
            this.blobDebugPicture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.calibrationPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.diffDebugPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blobDebugPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // calibrationPictureBox
            // 
            this.calibrationPictureBox.Location = new System.Drawing.Point(12, 12);
            this.calibrationPictureBox.Name = "calibrationPictureBox";
            this.calibrationPictureBox.Size = new System.Drawing.Size(640, 480);
            this.calibrationPictureBox.TabIndex = 0;
            this.calibrationPictureBox.TabStop = false;
            // 
            // diffDebugPicture
            // 
            this.diffDebugPicture.Location = new System.Drawing.Point(658, 12);
            this.diffDebugPicture.Name = "diffDebugPicture";
            this.diffDebugPicture.Size = new System.Drawing.Size(320, 240);
            this.diffDebugPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.diffDebugPicture.TabIndex = 1;
            this.diffDebugPicture.TabStop = false;
            // 
            // blobPicture
            // 
            this.blobDebugPicture.Location = new System.Drawing.Point(658, 252);
            this.blobDebugPicture.Name = "blobPicture";
            this.blobDebugPicture.Size = new System.Drawing.Size(320, 240);
            this.blobDebugPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.blobDebugPicture.TabIndex = 2;
            this.blobDebugPicture.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 502);
            this.Controls.Add(this.blobDebugPicture);
            this.Controls.Add(this.diffDebugPicture);
            this.Controls.Add(this.calibrationPictureBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.calibrationPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.diffDebugPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blobDebugPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox calibrationPictureBox;
        private System.Windows.Forms.PictureBox diffDebugPicture;
        private System.Windows.Forms.PictureBox blobDebugPicture;
    }
}

