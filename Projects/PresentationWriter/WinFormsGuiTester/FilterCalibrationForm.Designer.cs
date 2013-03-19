namespace WinFormsGuiTester
{
    partial class FilterCalibrationForm
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
            this.pathLabel = new System.Windows.Forms.Label();
            this.inputListBox = new System.Windows.Forms.ListBox();
            this.filterPictureBox = new System.Windows.Forms.PictureBox();
            this.filterGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.minBlobHeightTextBox = new System.Windows.Forms.TextBox();
            this.minBlobWidthTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.thresholdTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.grayBTextBox = new System.Windows.Forms.TextBox();
            this.grayGTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.grayRTextBox = new System.Windows.Forms.TextBox();
            this.folderButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.foundPointXLabel = new System.Windows.Forms.Label();
            this.foundPointYLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.filterPictureBox)).BeginInit();
            this.filterGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(9, 14);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(44, 13);
            this.pathLabel.TabIndex = 1;
            this.pathLabel.Text = "c:\\temp";
            // 
            // inputListBox
            // 
            this.inputListBox.FormattingEnabled = true;
            this.inputListBox.Location = new System.Drawing.Point(12, 38);
            this.inputListBox.Name = "inputListBox";
            this.inputListBox.Size = new System.Drawing.Size(184, 251);
            this.inputListBox.TabIndex = 2;
            this.inputListBox.SelectedIndexChanged += new System.EventHandler(this.inputListBox_SelectedIndexChanged);
            // 
            // filterPictureBox
            // 
            this.filterPictureBox.Location = new System.Drawing.Point(202, 9);
            this.filterPictureBox.Name = "filterPictureBox";
            this.filterPictureBox.Size = new System.Drawing.Size(640, 480);
            this.filterPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.filterPictureBox.TabIndex = 3;
            this.filterPictureBox.TabStop = false;
            // 
            // filterGroupBox
            // 
            this.filterGroupBox.Controls.Add(this.label5);
            this.filterGroupBox.Controls.Add(this.label4);
            this.filterGroupBox.Controls.Add(this.minBlobHeightTextBox);
            this.filterGroupBox.Controls.Add(this.minBlobWidthTextBox);
            this.filterGroupBox.Controls.Add(this.label1);
            this.filterGroupBox.Controls.Add(this.thresholdTextBox);
            this.filterGroupBox.Controls.Add(this.label3);
            this.filterGroupBox.Controls.Add(this.grayBTextBox);
            this.filterGroupBox.Controls.Add(this.grayGTextBox);
            this.filterGroupBox.Controls.Add(this.label2);
            this.filterGroupBox.Controls.Add(this.grayRTextBox);
            this.filterGroupBox.Location = new System.Drawing.Point(12, 295);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Size = new System.Drawing.Size(184, 194);
            this.filterGroupBox.TabIndex = 4;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Filter Options";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(136, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "h:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "w:";
            // 
            // minBlobHeightTextBox
            // 
            this.minBlobHeightTextBox.Location = new System.Drawing.Point(152, 71);
            this.minBlobHeightTextBox.Name = "minBlobHeightTextBox";
            this.minBlobHeightTextBox.Size = new System.Drawing.Size(26, 20);
            this.minBlobHeightTextBox.TabIndex = 9;
            this.minBlobHeightTextBox.Text = "1";
            this.minBlobHeightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // minBlobWidthTextBox
            // 
            this.minBlobWidthTextBox.Location = new System.Drawing.Point(104, 71);
            this.minBlobWidthTextBox.Name = "minBlobWidthTextBox";
            this.minBlobWidthTextBox.Size = new System.Drawing.Size(26, 20);
            this.minBlobWidthTextBox.TabIndex = 7;
            this.minBlobWidthTextBox.Text = "1";
            this.minBlobWidthTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Min. Blob Size:";
            // 
            // thresholdTextBox
            // 
            this.thresholdTextBox.Location = new System.Drawing.Point(88, 45);
            this.thresholdTextBox.Name = "thresholdTextBox";
            this.thresholdTextBox.Size = new System.Drawing.Size(90, 20);
            this.thresholdTextBox.TabIndex = 5;
            this.thresholdTextBox.Text = "40";
            this.thresholdTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Theshold:";
            // 
            // grayBTextBox
            // 
            this.grayBTextBox.Location = new System.Drawing.Point(152, 19);
            this.grayBTextBox.Name = "grayBTextBox";
            this.grayBTextBox.Size = new System.Drawing.Size(26, 20);
            this.grayBTextBox.TabIndex = 3;
            this.grayBTextBox.Text = "1";
            this.grayBTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // grayGTextBox
            // 
            this.grayGTextBox.Location = new System.Drawing.Point(120, 19);
            this.grayGTextBox.Name = "grayGTextBox";
            this.grayGTextBox.Size = new System.Drawing.Size(26, 20);
            this.grayGTextBox.TabIndex = 2;
            this.grayGTextBox.Text = "1";
            this.grayGTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "GrayTransform:";
            // 
            // grayRTextBox
            // 
            this.grayRTextBox.Location = new System.Drawing.Point(88, 19);
            this.grayRTextBox.Name = "grayRTextBox";
            this.grayRTextBox.Size = new System.Drawing.Size(26, 20);
            this.grayRTextBox.TabIndex = 0;
            this.grayRTextBox.Text = "1";
            this.grayRTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // folderButton
            // 
            this.folderButton.Location = new System.Drawing.Point(156, 9);
            this.folderButton.Name = "folderButton";
            this.folderButton.Size = new System.Drawing.Size(40, 23);
            this.folderButton.TabIndex = 5;
            this.folderButton.Text = "Path";
            this.folderButton.UseVisualStyleBackColor = true;
            this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.SystemColors.WindowText;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.label6.Location = new System.Drawing.Point(215, 464);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Point (X,Y): ";
            // 
            // foundPointXLabel
            // 
            this.foundPointXLabel.AutoSize = true;
            this.foundPointXLabel.BackColor = System.Drawing.SystemColors.WindowText;
            this.foundPointXLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foundPointXLabel.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.foundPointXLabel.Location = new System.Drawing.Point(284, 464);
            this.foundPointXLabel.Name = "foundPointXLabel";
            this.foundPointXLabel.Size = new System.Drawing.Size(31, 13);
            this.foundPointXLabel.TabIndex = 7;
            this.foundPointXLabel.Text = "XXX";
            // 
            // foundPointYLabel
            // 
            this.foundPointYLabel.AutoSize = true;
            this.foundPointYLabel.BackColor = System.Drawing.SystemColors.WindowText;
            this.foundPointYLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foundPointYLabel.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.foundPointYLabel.Location = new System.Drawing.Point(321, 464);
            this.foundPointYLabel.Name = "foundPointYLabel";
            this.foundPointYLabel.Size = new System.Drawing.Size(31, 13);
            this.foundPointYLabel.TabIndex = 8;
            this.foundPointYLabel.Text = "YYY";
            // 
            // FilterCalibrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 497);
            this.Controls.Add(this.foundPointYLabel);
            this.Controls.Add(this.foundPointXLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.folderButton);
            this.Controls.Add(this.filterGroupBox);
            this.Controls.Add(this.filterPictureBox);
            this.Controls.Add(this.inputListBox);
            this.Controls.Add(this.pathLabel);
            this.Name = "FilterCalibrationForm";
            this.Text = "FilterCalibrationForm";
            this.Load += new System.EventHandler(this.FilterCalibrationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.filterPictureBox)).EndInit();
            this.filterGroupBox.ResumeLayout(false);
            this.filterGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.ListBox inputListBox;
        private System.Windows.Forms.PictureBox filterPictureBox;
        private System.Windows.Forms.GroupBox filterGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox grayRTextBox;
        private System.Windows.Forms.TextBox thresholdTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox grayBTextBox;
        private System.Windows.Forms.TextBox grayGTextBox;
        private System.Windows.Forms.Button folderButton;
        private System.Windows.Forms.TextBox minBlobHeightTextBox;
        private System.Windows.Forms.TextBox minBlobWidthTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label foundPointXLabel;
        private System.Windows.Forms.Label foundPointYLabel;
    }
}