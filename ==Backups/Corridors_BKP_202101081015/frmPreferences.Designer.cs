namespace Corridors
{
    partial class frmPreferences
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtGridMajorCount = new System.Windows.Forms.TextBox();
            this.btnPickMajorGridLineColor = new System.Windows.Forms.Button();
            this.btnPickGridLineColor = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPickGridBGColor = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cdpGridLine = new System.Windows.Forms.ColorDialog();
            this.cdpGridBG = new System.Windows.Forms.ColorDialog();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.cdpMajorGridLine = new System.Windows.Forms.ColorDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.txtGridGutterWidth = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtGridGutterWidth);
            this.groupBox1.Controls.Add(this.txtGridMajorCount);
            this.groupBox1.Controls.Add(this.btnPickMajorGridLineColor);
            this.groupBox1.Controls.Add(this.btnPickGridLineColor);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnPickGridBGColor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(4, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(386, 112);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Grid Settings";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(203, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Major Lines Every";
            // 
            // txtGridMajorCount
            // 
            this.txtGridMajorCount.Location = new System.Drawing.Point(305, 17);
            this.txtGridMajorCount.Name = "txtGridMajorCount";
            this.txtGridMajorCount.Size = new System.Drawing.Size(41, 20);
            this.txtGridMajorCount.TabIndex = 2;
            // 
            // btnPickMajorGridLineColor
            // 
            this.btnPickMajorGridLineColor.Location = new System.Drawing.Point(122, 73);
            this.btnPickMajorGridLineColor.Name = "btnPickMajorGridLineColor";
            this.btnPickMajorGridLineColor.Size = new System.Drawing.Size(75, 23);
            this.btnPickMajorGridLineColor.TabIndex = 1;
            this.btnPickMajorGridLineColor.UseVisualStyleBackColor = true;
            this.btnPickMajorGridLineColor.Click += new System.EventHandler(this.btnPickMajorGridLineColor_Click);
            // 
            // btnPickGridLineColor
            // 
            this.btnPickGridLineColor.Location = new System.Drawing.Point(122, 44);
            this.btnPickGridLineColor.Name = "btnPickGridLineColor";
            this.btnPickGridLineColor.Size = new System.Drawing.Size(75, 23);
            this.btnPickGridLineColor.TabIndex = 1;
            this.btnPickGridLineColor.UseVisualStyleBackColor = true;
            this.btnPickGridLineColor.Click += new System.EventHandler(this.btnPickGridLineColor_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Major Grid Lines";
            // 
            // btnPickGridBGColor
            // 
            this.btnPickGridBGColor.Location = new System.Drawing.Point(122, 15);
            this.btnPickGridBGColor.Name = "btnPickGridBGColor";
            this.btnPickGridBGColor.Size = new System.Drawing.Size(75, 23);
            this.btnPickGridBGColor.TabIndex = 1;
            this.btnPickGridBGColor.UseVisualStyleBackColor = true;
            this.btnPickGridBGColor.Click += new System.EventHandler(this.btnPickGridBGColor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Grid Lines";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Grid Background";
            // 
            // cdpGridLine
            // 
            this.cdpGridLine.Color = System.Drawing.Color.LightGray;
            // 
            // cdpGridBG
            // 
            this.cdpGridBG.Color = System.Drawing.Color.White;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(314, 126);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(233, 126);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cdpMajorGridLine
            // 
            this.cdpMajorGridLine.Color = System.Drawing.Color.LightGray;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(352, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "tiles";
            // 
            // txtGridGutterWidth
            // 
            this.txtGridGutterWidth.Location = new System.Drawing.Point(305, 46);
            this.txtGridGutterWidth.Name = "txtGridGutterWidth";
            this.txtGridGutterWidth.Size = new System.Drawing.Size(41, 20);
            this.txtGridGutterWidth.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(203, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Gutter Width";
            // 
            // frmPreferences
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(393, 153);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmPreferences";
            this.Text = "Preferences";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ColorDialog cdpGridLine;
        private System.Windows.Forms.ColorDialog cdpGridBG;
        private System.Windows.Forms.Button btnPickGridLineColor;
        private System.Windows.Forms.Button btnPickGridBGColor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnPickMajorGridLineColor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColorDialog cdpMajorGridLine;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGridMajorCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtGridGutterWidth;
    }
}