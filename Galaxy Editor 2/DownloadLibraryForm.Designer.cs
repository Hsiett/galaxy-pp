namespace Galaxy_Editor_2
{
    partial class DownloadLibraryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadLibraryForm));
            this.LBLibraries = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TBSearchBox = new System.Windows.Forms.TextBox();
            this.LLibraryName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CBVersions = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.RTBDescription = new System.Windows.Forms.RichTextBox();
            this.RTBChangeLog = new System.Windows.Forms.RichTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LAuthor = new System.Windows.Forms.Label();
            this.BTNDownload = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.LBDependancies = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LBLibraries
            // 
            this.LBLibraries.FormattingEnabled = true;
            this.LBLibraries.Location = new System.Drawing.Point(12, 25);
            this.LBLibraries.Name = "LBLibraries";
            this.LBLibraries.Size = new System.Drawing.Size(228, 433);
            this.LBLibraries.TabIndex = 0;
            this.LBLibraries.SelectedIndexChanged += new System.EventHandler(this.LBLibraries_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Libraries";
            // 
            // TBSearchBox
            // 
            this.TBSearchBox.Location = new System.Drawing.Point(12, 463);
            this.TBSearchBox.Name = "TBSearchBox";
            this.TBSearchBox.Size = new System.Drawing.Size(228, 20);
            this.TBSearchBox.TabIndex = 2;
            this.TBSearchBox.TextChanged += new System.EventHandler(this.TBSearchBox_TextChanged);
            // 
            // LLibraryName
            // 
            this.LLibraryName.AutoSize = true;
            this.LLibraryName.Location = new System.Drawing.Point(246, 25);
            this.LLibraryName.Name = "LLibraryName";
            this.LLibraryName.Size = new System.Drawing.Size(35, 13);
            this.LLibraryName.TabIndex = 3;
            this.LLibraryName.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(246, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Version";
            // 
            // CBVersions
            // 
            this.CBVersions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBVersions.Enabled = false;
            this.CBVersions.FormattingEnabled = true;
            this.CBVersions.Location = new System.Drawing.Point(371, 39);
            this.CBVersions.Name = "CBVersions";
            this.CBVersions.Size = new System.Drawing.Size(144, 21);
            this.CBVersions.TabIndex = 5;
            this.CBVersions.SelectedIndexChanged += new System.EventHandler(this.CBVersions_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(246, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description";
            // 
            // RTBDescription
            // 
            this.RTBDescription.Location = new System.Drawing.Point(249, 92);
            this.RTBDescription.Name = "RTBDescription";
            this.RTBDescription.ReadOnly = true;
            this.RTBDescription.Size = new System.Drawing.Size(266, 108);
            this.RTBDescription.TabIndex = 7;
            this.RTBDescription.Text = "";
            // 
            // RTBChangeLog
            // 
            this.RTBChangeLog.Location = new System.Drawing.Point(249, 219);
            this.RTBChangeLog.Name = "RTBChangeLog";
            this.RTBChangeLog.ReadOnly = true;
            this.RTBChangeLog.Size = new System.Drawing.Size(266, 108);
            this.RTBChangeLog.TabIndex = 9;
            this.RTBChangeLog.Text = "";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(246, 203);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Change log";
            // 
            // LAuthor
            // 
            this.LAuthor.AutoSize = true;
            this.LAuthor.Location = new System.Drawing.Point(246, 63);
            this.LAuthor.Name = "LAuthor";
            this.LAuthor.Size = new System.Drawing.Size(44, 13);
            this.LAuthor.TabIndex = 10;
            this.LAuthor.Text = "Author: ";
            // 
            // BTNDownload
            // 
            this.BTNDownload.Enabled = false;
            this.BTNDownload.Location = new System.Drawing.Point(359, 460);
            this.BTNDownload.Name = "BTNDownload";
            this.BTNDownload.Size = new System.Drawing.Size(75, 23);
            this.BTNDownload.TabIndex = 11;
            this.BTNDownload.Text = "Download";
            this.BTNDownload.UseVisualStyleBackColor = true;
            this.BTNDownload.Click += new System.EventHandler(this.BTNDownload_Click);
            // 
            // BTNCancel
            // 
            this.BTNCancel.Location = new System.Drawing.Point(440, 460);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 12;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            this.BTNCancel.Click += new System.EventHandler(this.BTNCancel_Click);
            // 
            // LBDependancies
            // 
            this.LBDependancies.FormattingEnabled = true;
            this.LBDependancies.Location = new System.Drawing.Point(249, 346);
            this.LBDependancies.Name = "LBDependancies";
            this.LBDependancies.Size = new System.Drawing.Size(265, 108);
            this.LBDependancies.TabIndex = 13;
            this.LBDependancies.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LBDependancies_MouseDoubleClick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(246, 330);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Dependancies";
            // 
            // DownloadLibraryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 502);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LBDependancies);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNDownload);
            this.Controls.Add(this.LAuthor);
            this.Controls.Add(this.RTBChangeLog);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.RTBDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CBVersions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LLibraryName);
            this.Controls.Add(this.TBSearchBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LBLibraries);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DownloadLibraryForm";
            this.Text = "DownloadLibraryForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBLibraries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBSearchBox;
        private System.Windows.Forms.Label LLibraryName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CBVersions;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox RTBDescription;
        private System.Windows.Forms.RichTextBox RTBChangeLog;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label LAuthor;
        private System.Windows.Forms.Button BTNDownload;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.ListBox LBDependancies;
        private System.Windows.Forms.Label label5;
    }
}