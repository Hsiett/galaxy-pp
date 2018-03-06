namespace Galaxy_Editor_2
{
    partial class CompileModWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompileModWindow));
            this.CBAllowRename = new System.Windows.Forms.CheckBox();
            this.CBUpload = new System.Windows.Forms.CheckBox();
            this.GBUpload = new System.Windows.Forms.GroupBox();
            this.CBProtectMap = new System.Windows.Forms.CheckBox();
            this.GBProtect = new System.Windows.Forms.GroupBox();
            this.TBPassword = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BTNLoadMapFolder = new System.Windows.Forms.Button();
            this.BTNLoadMapFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TBMapPath = new System.Windows.Forms.TextBox();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.BTNCompile = new System.Windows.Forms.Button();
            this.GBUpload.SuspendLayout();
            this.GBProtect.SuspendLayout();
            this.SuspendLayout();
            // 
            // CBAllowRename
            // 
            this.CBAllowRename.AutoSize = true;
            this.CBAllowRename.Checked = true;
            this.CBAllowRename.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBAllowRename.Location = new System.Drawing.Point(12, 12);
            this.CBAllowRename.Name = "CBAllowRename";
            this.CBAllowRename.Size = new System.Drawing.Size(97, 17);
            this.CBAllowRename.TabIndex = 0;
            this.CBAllowRename.Text = "Allow renaming";
            this.CBAllowRename.UseVisualStyleBackColor = true;
            // 
            // CBUpload
            // 
            this.CBUpload.AutoSize = true;
            this.CBUpload.Location = new System.Drawing.Point(17, 41);
            this.CBUpload.Name = "CBUpload";
            this.CBUpload.Size = new System.Drawing.Size(164, 17);
            this.CBUpload.TabIndex = 1;
            this.CBUpload.Text = "Upload naming map to server";
            this.CBUpload.UseVisualStyleBackColor = true;
            this.CBUpload.CheckedChanged += new System.EventHandler(this.CBUpload_CheckedChanged);
            // 
            // GBUpload
            // 
            this.GBUpload.Controls.Add(this.CBProtectMap);
            this.GBUpload.Controls.Add(this.GBProtect);
            this.GBUpload.Controls.Add(this.BTNLoadMapFolder);
            this.GBUpload.Controls.Add(this.BTNLoadMapFile);
            this.GBUpload.Controls.Add(this.label1);
            this.GBUpload.Controls.Add(this.TBMapPath);
            this.GBUpload.Enabled = false;
            this.GBUpload.Location = new System.Drawing.Point(11, 42);
            this.GBUpload.Name = "GBUpload";
            this.GBUpload.Size = new System.Drawing.Size(262, 172);
            this.GBUpload.TabIndex = 2;
            this.GBUpload.TabStop = false;
            // 
            // CBProtectMap
            // 
            this.CBProtectMap.AutoSize = true;
            this.CBProtectMap.Location = new System.Drawing.Point(6, 99);
            this.CBProtectMap.Name = "CBProtectMap";
            this.CBProtectMap.Size = new System.Drawing.Size(130, 17);
            this.CBProtectMap.TabIndex = 7;
            this.CBProtectMap.Text = "Protect uploaded map";
            this.CBProtectMap.UseVisualStyleBackColor = true;
            this.CBProtectMap.CheckedChanged += new System.EventHandler(this.CBProtectMap_CheckedChanged);
            // 
            // GBProtect
            // 
            this.GBProtect.Controls.Add(this.TBPassword);
            this.GBProtect.Controls.Add(this.label2);
            this.GBProtect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.GBProtect.Enabled = false;
            this.GBProtect.Location = new System.Drawing.Point(3, 101);
            this.GBProtect.Name = "GBProtect";
            this.GBProtect.Size = new System.Drawing.Size(256, 68);
            this.GBProtect.TabIndex = 6;
            this.GBProtect.TabStop = false;
            // 
            // TBPassword
            // 
            this.TBPassword.Location = new System.Drawing.Point(6, 35);
            this.TBPassword.Name = "TBPassword";
            this.TBPassword.PasswordChar = '•';
            this.TBPassword.Size = new System.Drawing.Size(244, 20);
            this.TBPassword.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Password";
            // 
            // BTNLoadMapFolder
            // 
            this.BTNLoadMapFolder.Location = new System.Drawing.Point(87, 62);
            this.BTNLoadMapFolder.Name = "BTNLoadMapFolder";
            this.BTNLoadMapFolder.Size = new System.Drawing.Size(75, 23);
            this.BTNLoadMapFolder.TabIndex = 5;
            this.BTNLoadMapFolder.Text = "Load folder";
            this.BTNLoadMapFolder.UseVisualStyleBackColor = true;
            this.BTNLoadMapFolder.Click += new System.EventHandler(this.BTNLoadMapFolder_Click);
            // 
            // BTNLoadMapFile
            // 
            this.BTNLoadMapFile.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNLoadMapFile.Location = new System.Drawing.Point(6, 62);
            this.BTNLoadMapFile.Name = "BTNLoadMapFile";
            this.BTNLoadMapFile.Size = new System.Drawing.Size(75, 23);
            this.BTNLoadMapFile.TabIndex = 4;
            this.BTNLoadMapFile.Text = "Load file";
            this.BTNLoadMapFile.UseVisualStyleBackColor = true;
            this.BTNLoadMapFile.Click += new System.EventHandler(this.BTNLoadMapFile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Map that uses the mod";
            // 
            // TBMapPath
            // 
            this.TBMapPath.Location = new System.Drawing.Point(6, 36);
            this.TBMapPath.Name = "TBMapPath";
            this.TBMapPath.Size = new System.Drawing.Size(250, 20);
            this.TBMapPath.TabIndex = 2;
            // 
            // BTNCancel
            // 
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(199, 220);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 3;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // BTNCompile
            // 
            this.BTNCompile.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNCompile.Location = new System.Drawing.Point(118, 220);
            this.BTNCompile.Name = "BTNCompile";
            this.BTNCompile.Size = new System.Drawing.Size(75, 23);
            this.BTNCompile.TabIndex = 4;
            this.BTNCompile.Text = "Compile";
            this.BTNCompile.UseVisualStyleBackColor = true;
            this.BTNCompile.Click += new System.EventHandler(this.BTNCompile_Click);
            // 
            // CompileModWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 252);
            this.Controls.Add(this.BTNCompile);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.CBUpload);
            this.Controls.Add(this.GBUpload);
            this.Controls.Add(this.CBAllowRename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "CompileModWindow";
            this.Text = "Compile Mod";
            this.GBUpload.ResumeLayout(false);
            this.GBUpload.PerformLayout();
            this.GBProtect.ResumeLayout(false);
            this.GBProtect.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CBAllowRename;
        private System.Windows.Forms.CheckBox CBUpload;
        private System.Windows.Forms.GroupBox GBUpload;
        private System.Windows.Forms.GroupBox GBProtect;
        private System.Windows.Forms.CheckBox CBProtectMap;
        private System.Windows.Forms.Button BTNLoadMapFolder;
        private System.Windows.Forms.Button BTNLoadMapFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBMapPath;
        private System.Windows.Forms.MaskedTextBox TBPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.Button BTNCompile;
    }
}