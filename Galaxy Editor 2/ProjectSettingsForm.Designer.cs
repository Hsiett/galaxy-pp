namespace Galaxy_Editor_2
{
    partial class ProjectSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectSettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.BTNSelectCurrentMap = new System.Windows.Forms.Button();
            this.TBCurrentMap = new System.Windows.Forms.TextBox();
            this.BTNResetCurrentMap = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.BTNSelectOutputFolder = new System.Windows.Forms.Button();
            this.BTNResetOutput = new System.Windows.Forms.Button();
            this.TBOutputMap = new System.Windows.Forms.TextBox();
            this.BTNSelectOutputFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.CBSaveScriptToMap = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Map";
            // 
            // BTNSelectCurrentMap
            // 
            this.BTNSelectCurrentMap.Location = new System.Drawing.Point(12, 74);
            this.BTNSelectCurrentMap.Name = "BTNSelectCurrentMap";
            this.BTNSelectCurrentMap.Size = new System.Drawing.Size(75, 23);
            this.BTNSelectCurrentMap.TabIndex = 2;
            this.BTNSelectCurrentMap.Text = "Select file";
            this.BTNSelectCurrentMap.UseVisualStyleBackColor = true;
            this.BTNSelectCurrentMap.Click += new System.EventHandler(this.BTNSelectCurrentMap_Click);
            // 
            // TBCurrentMap
            // 
            this.TBCurrentMap.BackColor = System.Drawing.Color.White;
            this.TBCurrentMap.Location = new System.Drawing.Point(12, 25);
            this.TBCurrentMap.Name = "TBCurrentMap";
            this.TBCurrentMap.ReadOnly = true;
            this.TBCurrentMap.Size = new System.Drawing.Size(208, 20);
            this.TBCurrentMap.TabIndex = 3;
            this.TBCurrentMap.TextChanged += new System.EventHandler(this.TBCurrentMap_TextChanged);
            // 
            // BTNResetCurrentMap
            // 
            this.BTNResetCurrentMap.Location = new System.Drawing.Point(174, 74);
            this.BTNResetCurrentMap.Name = "BTNResetCurrentMap";
            this.BTNResetCurrentMap.Size = new System.Drawing.Size(46, 23);
            this.BTNResetCurrentMap.TabIndex = 4;
            this.BTNResetCurrentMap.Text = "Reset";
            this.BTNResetCurrentMap.UseVisualStyleBackColor = true;
            this.BTNResetCurrentMap.Click += new System.EventHandler(this.BTNResetCurrentMap_Click);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(145, 171);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Okay";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(93, 74);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFolder.TabIndex = 6;
            this.btnSelectFolder.Text = "Select folder";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // BTNSelectOutputFolder
            // 
            this.BTNSelectOutputFolder.Location = new System.Drawing.Point(93, 142);
            this.BTNSelectOutputFolder.Name = "BTNSelectOutputFolder";
            this.BTNSelectOutputFolder.Size = new System.Drawing.Size(75, 23);
            this.BTNSelectOutputFolder.TabIndex = 11;
            this.BTNSelectOutputFolder.Text = "Select folder";
            this.BTNSelectOutputFolder.UseVisualStyleBackColor = true;
            this.BTNSelectOutputFolder.Click += new System.EventHandler(this.BTNSelectOutputFolder_Click);
            // 
            // BTNResetOutput
            // 
            this.BTNResetOutput.Location = new System.Drawing.Point(174, 142);
            this.BTNResetOutput.Name = "BTNResetOutput";
            this.BTNResetOutput.Size = new System.Drawing.Size(46, 23);
            this.BTNResetOutput.TabIndex = 10;
            this.BTNResetOutput.Text = "Reset";
            this.BTNResetOutput.UseVisualStyleBackColor = true;
            this.BTNResetOutput.Click += new System.EventHandler(this.BTNResetOutput_Click);
            // 
            // TBOutputMap
            // 
            this.TBOutputMap.BackColor = System.Drawing.Color.White;
            this.TBOutputMap.Location = new System.Drawing.Point(12, 116);
            this.TBOutputMap.Name = "TBOutputMap";
            this.TBOutputMap.ReadOnly = true;
            this.TBOutputMap.Size = new System.Drawing.Size(208, 20);
            this.TBOutputMap.TabIndex = 9;
            this.TBOutputMap.TextChanged += new System.EventHandler(this.TBOutputMap_TextChanged);
            // 
            // BTNSelectOutputFile
            // 
            this.BTNSelectOutputFile.Location = new System.Drawing.Point(12, 142);
            this.BTNSelectOutputFile.Name = "BTNSelectOutputFile";
            this.BTNSelectOutputFile.Size = new System.Drawing.Size(75, 23);
            this.BTNSelectOutputFile.TabIndex = 8;
            this.BTNSelectOutputFile.Text = "Select file";
            this.BTNSelectOutputFile.UseVisualStyleBackColor = true;
            this.BTNSelectOutputFile.Click += new System.EventHandler(this.BTNSelectOutputFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Project output map";
            // 
            // CBSaveScriptToMap
            // 
            this.CBSaveScriptToMap.AutoSize = true;
            this.CBSaveScriptToMap.Location = new System.Drawing.Point(12, 51);
            this.CBSaveScriptToMap.Name = "CBSaveScriptToMap";
            this.CBSaveScriptToMap.Size = new System.Drawing.Size(186, 17);
            this.CBSaveScriptToMap.TabIndex = 12;
            this.CBSaveScriptToMap.Text = "Load/save galaxy++ script to map";
            this.CBSaveScriptToMap.UseVisualStyleBackColor = true;
            this.CBSaveScriptToMap.CheckedChanged += new System.EventHandler(this.saveScriptToMap_CheckedChanged);
            // 
            // ProjectSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 378);
            this.ControlBox = false;
            this.Controls.Add(this.CBSaveScriptToMap);
            this.Controls.Add(this.BTNSelectOutputFolder);
            this.Controls.Add(this.BTNResetOutput);
            this.Controls.Add(this.TBOutputMap);
            this.Controls.Add(this.BTNSelectOutputFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.BTNResetCurrentMap);
            this.Controls.Add(this.TBCurrentMap);
            this.Controls.Add(this.BTNSelectCurrentMap);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Project Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTNSelectCurrentMap;
        private System.Windows.Forms.TextBox TBCurrentMap;
        private System.Windows.Forms.Button BTNResetCurrentMap;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button BTNSelectOutputFolder;
        private System.Windows.Forms.Button BTNResetOutput;
        private System.Windows.Forms.TextBox TBOutputMap;
        private System.Windows.Forms.Button BTNSelectOutputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CBSaveScriptToMap;
    }
}