namespace Galaxy_Editor_2
{
    partial class FindAndReplaceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindAndReplaceForm));
            this.label1 = new System.Windows.Forms.Label();
            this.TBFind = new System.Windows.Forms.TextBox();
            this.TBReplace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BTNFind = new System.Windows.Forms.Button();
            this.BTNReplace = new System.Windows.Forms.Button();
            this.BTNReplaceAll = new System.Windows.Forms.Button();
            this.RBCurrent = new System.Windows.Forms.RadioButton();
            this.RBProject = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.CBLookSource = new System.Windows.Forms.CheckBox();
            this.CBLookOutput = new System.Windows.Forms.CheckBox();
            this.CBMatchCase = new System.Windows.Forms.CheckBox();
            this.CBSearchUp = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find text";
            // 
            // TBFind
            // 
            this.TBFind.Location = new System.Drawing.Point(12, 25);
            this.TBFind.Name = "TBFind";
            this.TBFind.Size = new System.Drawing.Size(237, 20);
            this.TBFind.TabIndex = 1;
            this.TBFind.TextChanged += new System.EventHandler(this.TBFind_TextChanged);
            // 
            // TBReplace
            // 
            this.TBReplace.Location = new System.Drawing.Point(12, 64);
            this.TBReplace.Name = "TBReplace";
            this.TBReplace.Size = new System.Drawing.Size(237, 20);
            this.TBReplace.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Replace with";
            // 
            // BTNFind
            // 
            this.BTNFind.Location = new System.Drawing.Point(12, 214);
            this.BTNFind.Name = "BTNFind";
            this.BTNFind.Size = new System.Drawing.Size(75, 23);
            this.BTNFind.TabIndex = 4;
            this.BTNFind.Text = "Find next";
            this.BTNFind.UseVisualStyleBackColor = true;
            this.BTNFind.Click += new System.EventHandler(this.BTNFind_Click);
            // 
            // BTNReplace
            // 
            this.BTNReplace.Location = new System.Drawing.Point(93, 214);
            this.BTNReplace.Name = "BTNReplace";
            this.BTNReplace.Size = new System.Drawing.Size(75, 23);
            this.BTNReplace.TabIndex = 5;
            this.BTNReplace.Text = "Replace";
            this.BTNReplace.UseVisualStyleBackColor = true;
            this.BTNReplace.Click += new System.EventHandler(this.BTNReplace_Click);
            // 
            // BTNReplaceAll
            // 
            this.BTNReplaceAll.Location = new System.Drawing.Point(174, 214);
            this.BTNReplaceAll.Name = "BTNReplaceAll";
            this.BTNReplaceAll.Size = new System.Drawing.Size(75, 23);
            this.BTNReplaceAll.TabIndex = 6;
            this.BTNReplaceAll.Text = "Replace All";
            this.BTNReplaceAll.UseVisualStyleBackColor = true;
            this.BTNReplaceAll.Click += new System.EventHandler(this.BTNReplaceAll_Click);
            // 
            // RBCurrent
            // 
            this.RBCurrent.AutoSize = true;
            this.RBCurrent.Checked = true;
            this.RBCurrent.Location = new System.Drawing.Point(12, 103);
            this.RBCurrent.Name = "RBCurrent";
            this.RBCurrent.Size = new System.Drawing.Size(75, 17);
            this.RBCurrent.TabIndex = 7;
            this.RBCurrent.TabStop = true;
            this.RBCurrent.Text = "Current file";
            this.RBCurrent.UseVisualStyleBackColor = true;
            // 
            // RBProject
            // 
            this.RBProject.AutoSize = true;
            this.RBProject.Location = new System.Drawing.Point(93, 103);
            this.RBProject.Name = "RBProject";
            this.RBProject.Size = new System.Drawing.Size(87, 17);
            this.RBProject.TabIndex = 8;
            this.RBProject.Text = "Entire project";
            this.RBProject.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Search in";
            // 
            // CBLookSource
            // 
            this.CBLookSource.AutoSize = true;
            this.CBLookSource.Location = new System.Drawing.Point(12, 126);
            this.CBLookSource.Name = "CBLookSource";
            this.CBLookSource.Size = new System.Drawing.Size(117, 17);
            this.CBLookSource.TabIndex = 11;
            this.CBLookSource.Text = "Look in source files";
            this.CBLookSource.UseVisualStyleBackColor = true;
            this.CBLookSource.CheckedChanged += new System.EventHandler(this.CBLook_CheckedChanged);
            // 
            // CBLookOutput
            // 
            this.CBLookOutput.AutoSize = true;
            this.CBLookOutput.Location = new System.Drawing.Point(12, 145);
            this.CBLookOutput.Name = "CBLookOutput";
            this.CBLookOutput.Size = new System.Drawing.Size(115, 17);
            this.CBLookOutput.TabIndex = 12;
            this.CBLookOutput.Text = "Look in output files";
            this.CBLookOutput.UseVisualStyleBackColor = true;
            this.CBLookOutput.CheckedChanged += new System.EventHandler(this.CBLook_CheckedChanged);
            // 
            // CBMatchCase
            // 
            this.CBMatchCase.AutoSize = true;
            this.CBMatchCase.Location = new System.Drawing.Point(12, 168);
            this.CBMatchCase.Name = "CBMatchCase";
            this.CBMatchCase.Size = new System.Drawing.Size(82, 17);
            this.CBMatchCase.TabIndex = 13;
            this.CBMatchCase.Text = "Match case";
            this.CBMatchCase.UseVisualStyleBackColor = true;
            // 
            // CBSearchUp
            // 
            this.CBSearchUp.AutoSize = true;
            this.CBSearchUp.Location = new System.Drawing.Point(12, 191);
            this.CBSearchUp.Name = "CBSearchUp";
            this.CBSearchUp.Size = new System.Drawing.Size(75, 17);
            this.CBSearchUp.TabIndex = 14;
            this.CBSearchUp.Text = "Search up";
            this.CBSearchUp.UseVisualStyleBackColor = true;
            // 
            // FindAndReplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 338);
            this.Controls.Add(this.CBSearchUp);
            this.Controls.Add(this.CBMatchCase);
            this.Controls.Add(this.CBLookOutput);
            this.Controls.Add(this.CBLookSource);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.RBProject);
            this.Controls.Add(this.RBCurrent);
            this.Controls.Add(this.BTNReplaceAll);
            this.Controls.Add(this.BTNReplace);
            this.Controls.Add(this.BTNFind);
            this.Controls.Add(this.TBReplace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TBFind);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FindAndReplaceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find and Replace";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindAndReplaceForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FindAndReplaceForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBFind;
        private System.Windows.Forms.TextBox TBReplace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BTNFind;
        private System.Windows.Forms.Button BTNReplace;
        private System.Windows.Forms.Button BTNReplaceAll;
        private System.Windows.Forms.RadioButton RBCurrent;
        private System.Windows.Forms.RadioButton RBProject;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CBLookSource;
        private System.Windows.Forms.CheckBox CBLookOutput;
        private System.Windows.Forms.CheckBox CBMatchCase;
        private System.Windows.Forms.CheckBox CBSearchUp;
    }
}