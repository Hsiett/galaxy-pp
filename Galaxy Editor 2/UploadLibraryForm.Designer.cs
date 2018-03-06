namespace Galaxy_Editor_2
{
    partial class UploadLibraryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UploadLibraryForm));
            this.label1 = new System.Windows.Forms.Label();
            this.TBLibName = new System.Windows.Forms.TextBox();
            this.TBVersion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TBDescription = new System.Windows.Forms.RichTextBox();
            this.TBChangelog = new System.Windows.Forms.RichTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.BTNUpload = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.CBOverwrite = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Library Name";
            // 
            // TBLibName
            // 
            this.TBLibName.Location = new System.Drawing.Point(15, 25);
            this.TBLibName.Name = "TBLibName";
            this.TBLibName.Size = new System.Drawing.Size(130, 20);
            this.TBLibName.TabIndex = 1;
            // 
            // TBVersion
            // 
            this.TBVersion.Location = new System.Drawing.Point(151, 25);
            this.TBVersion.Name = "TBVersion";
            this.TBVersion.Size = new System.Drawing.Size(130, 20);
            this.TBVersion.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Version";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Description";
            // 
            // TBDescription
            // 
            this.TBDescription.Location = new System.Drawing.Point(15, 64);
            this.TBDescription.Name = "TBDescription";
            this.TBDescription.Size = new System.Drawing.Size(266, 108);
            this.TBDescription.TabIndex = 5;
            this.TBDescription.Text = "";
            // 
            // TBChangelog
            // 
            this.TBChangelog.Location = new System.Drawing.Point(15, 191);
            this.TBChangelog.Name = "TBChangelog";
            this.TBChangelog.Size = new System.Drawing.Size(266, 108);
            this.TBChangelog.TabIndex = 7;
            this.TBChangelog.Text = "";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 175);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Change Log";
            // 
            // BTNUpload
            // 
            this.BTNUpload.Location = new System.Drawing.Point(125, 328);
            this.BTNUpload.Name = "BTNUpload";
            this.BTNUpload.Size = new System.Drawing.Size(75, 23);
            this.BTNUpload.TabIndex = 8;
            this.BTNUpload.Text = "Upload";
            this.BTNUpload.UseVisualStyleBackColor = true;
            this.BTNUpload.Click += new System.EventHandler(this.BTNUpload_Click);
            // 
            // BTNCancel
            // 
            this.BTNCancel.Location = new System.Drawing.Point(206, 328);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 9;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            this.BTNCancel.Click += new System.EventHandler(this.BTNCancel_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(287, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(342, 156);
            this.label5.TabIndex = 10;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // CBOverwrite
            // 
            this.CBOverwrite.AutoSize = true;
            this.CBOverwrite.Location = new System.Drawing.Point(15, 305);
            this.CBOverwrite.Name = "CBOverwrite";
            this.CBOverwrite.Size = new System.Drawing.Size(109, 17);
            this.CBOverwrite.TabIndex = 11;
            this.CBOverwrite.Text = "Overwrite existing";
            this.CBOverwrite.UseVisualStyleBackColor = true;
            // 
            // UploadLibraryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 356);
            this.Controls.Add(this.CBOverwrite);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNUpload);
            this.Controls.Add(this.TBChangelog);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TBDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TBVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TBLibName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UploadLibraryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Upload Library";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBLibName;
        private System.Windows.Forms.TextBox TBVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox TBDescription;
        private System.Windows.Forms.RichTextBox TBChangelog;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BTNUpload;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox CBOverwrite;
    }
}