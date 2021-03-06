﻿namespace WinFormsGuiTester
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
            this.foundPointLabel = new System.Windows.Forms.Label();
            this.cameraPictureBox = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.overlayButton = new System.Windows.Forms.Button();
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
            this.toggleParserButton.Location = new System.Drawing.Point(12, 70);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 128);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(156, 364);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Resultate:";
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
            // cameraPictureBox
            // 
            this.cameraPictureBox.Location = new System.Drawing.Point(174, 12);
            this.cameraPictureBox.Name = "cameraPictureBox";
            this.cameraPictureBox.Size = new System.Drawing.Size(640, 480);
            this.cameraPictureBox.TabIndex = 3;
            this.cameraPictureBox.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(156, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Start Config";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // overlayButton
            // 
            this.overlayButton.Location = new System.Drawing.Point(12, 99);
            this.overlayButton.Name = "overlayButton";
            this.overlayButton.Size = new System.Drawing.Size(156, 23);
            this.overlayButton.TabIndex = 5;
            this.overlayButton.Text = "Show Overlay";
            this.overlayButton.UseVisualStyleBackColor = true;
            this.overlayButton.Click += new System.EventHandler(this.overlayButton_Click);
            // 
            // LiveTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 497);
            this.Controls.Add(this.overlayButton);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button overlayButton;
    }
}