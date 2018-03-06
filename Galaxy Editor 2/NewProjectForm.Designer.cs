namespace Galaxy_Editor_2
{
    partial class NewProjectForm
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
            this.TBName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BTNResetFolder = new System.Windows.Forms.Button();
            this.BTNSelectFolder = new System.Windows.Forms.Button();
            this.TBDirectory = new System.Windows.Forms.TextBox();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.BTNOK = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TBName
            // 
            this.TBName.Location = new System.Drawing.Point(12, 25);
            this.TBName.Name = "TBName";
            this.TBName.Size = new System.Drawing.Size(490, 20);
            this.TBName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Project name";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BTNResetFolder);
            this.groupBox1.Controls.Add(this.BTNSelectFolder);
            this.groupBox1.Controls.Add(this.TBDirectory);
            this.groupBox1.Location = new System.Drawing.Point(12, 51);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(490, 76);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Directory";
            // 
            // BTNResetFolder
            // 
            this.BTNResetFolder.Location = new System.Drawing.Point(93, 45);
            this.BTNResetFolder.Name = "BTNResetFolder";
            this.BTNResetFolder.Size = new System.Drawing.Size(81, 23);
            this.BTNResetFolder.TabIndex = 2;
            this.BTNResetFolder.Text = "Reset";
            this.BTNResetFolder.UseVisualStyleBackColor = true;
            this.BTNResetFolder.Click += new System.EventHandler(this.BTNResetFolder_Click);
            // 
            // BTNSelectFolder
            // 
            this.BTNSelectFolder.Location = new System.Drawing.Point(6, 45);
            this.BTNSelectFolder.Name = "BTNSelectFolder";
            this.BTNSelectFolder.Size = new System.Drawing.Size(81, 23);
            this.BTNSelectFolder.TabIndex = 1;
            this.BTNSelectFolder.Text = "Select Folder";
            this.BTNSelectFolder.UseVisualStyleBackColor = true;
            this.BTNSelectFolder.Click += new System.EventHandler(this.BTNSelectFolder_Click);
            // 
            // TBDirectory
            // 
            this.TBDirectory.Location = new System.Drawing.Point(6, 19);
            this.TBDirectory.Name = "TBDirectory";
            this.TBDirectory.Size = new System.Drawing.Size(478, 20);
            this.TBDirectory.TabIndex = 0;
            // 
            // BTNCancel
            // 
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(421, 133);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(81, 23);
            this.BTNCancel.TabIndex = 4;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // BTNOK
            // 
            this.BTNOK.Location = new System.Drawing.Point(334, 133);
            this.BTNOK.Name = "BTNOK";
            this.BTNOK.Size = new System.Drawing.Size(81, 23);
            this.BTNOK.TabIndex = 5;
            this.BTNOK.Text = "Okay";
            this.BTNOK.UseVisualStyleBackColor = true;
            this.BTNOK.Click += new System.EventHandler(this.BTNOK_Click);
            // 
            // NewProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 170);
            this.ControlBox = false;
            this.Controls.Add(this.BTNOK);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TBName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NewProjectForm";
            this.Text = "New Project";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TBName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BTNResetFolder;
        private System.Windows.Forms.Button BTNSelectFolder;
        private System.Windows.Forms.TextBox TBDirectory;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.Button BTNOK;
    }
}