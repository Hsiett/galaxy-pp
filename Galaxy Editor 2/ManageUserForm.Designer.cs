namespace Galaxy_Editor_2
{
    partial class ManageUserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageUserForm));
            this.LUsername = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TBNewPassword = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BTNChangePassword = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BTNDeleteLibrary = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CBLibVersion = new System.Windows.Forms.ComboBox();
            this.LBLibraries = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BTNChangeEmail = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TBEmail = new System.Windows.Forms.TextBox();
            this.BTNClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // LUsername
            // 
            this.LUsername.AutoSize = true;
            this.LUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LUsername.Location = new System.Drawing.Point(12, 9);
            this.LUsername.Name = "LUsername";
            this.LUsername.Size = new System.Drawing.Size(79, 16);
            this.LUsername.TabIndex = 0;
            this.LUsername.Text = "Username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "New password";
            // 
            // TBNewPassword
            // 
            this.TBNewPassword.Location = new System.Drawing.Point(9, 32);
            this.TBNewPassword.Name = "TBNewPassword";
            this.TBNewPassword.PasswordChar = '*';
            this.TBNewPassword.Size = new System.Drawing.Size(185, 20);
            this.TBNewPassword.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BTNChangePassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TBNewPassword);
            this.groupBox1.Location = new System.Drawing.Point(12, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 90);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Change password";
            // 
            // BTNChangePassword
            // 
            this.BTNChangePassword.Location = new System.Drawing.Point(119, 58);
            this.BTNChangePassword.Name = "BTNChangePassword";
            this.BTNChangePassword.Size = new System.Drawing.Size(75, 23);
            this.BTNChangePassword.TabIndex = 4;
            this.BTNChangePassword.Text = "Change";
            this.BTNChangePassword.UseVisualStyleBackColor = true;
            this.BTNChangePassword.Click += new System.EventHandler(this.BTNChangePassword_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BTNDeleteLibrary);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.CBLibVersion);
            this.groupBox2.Controls.Add(this.LBLibraries);
            this.groupBox2.Location = new System.Drawing.Point(12, 124);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(409, 244);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Uploaded Libraries";
            // 
            // BTNDeleteLibrary
            // 
            this.BTNDeleteLibrary.Location = new System.Drawing.Point(328, 208);
            this.BTNDeleteLibrary.Name = "BTNDeleteLibrary";
            this.BTNDeleteLibrary.Size = new System.Drawing.Size(75, 23);
            this.BTNDeleteLibrary.TabIndex = 6;
            this.BTNDeleteLibrary.Text = "Delete";
            this.BTNDeleteLibrary.UseVisualStyleBackColor = true;
            this.BTNDeleteLibrary.Click += new System.EventHandler(this.BTNDeleteLibrary_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(206, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Version";
            // 
            // CBLibVersion
            // 
            this.CBLibVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBLibVersion.FormattingEnabled = true;
            this.CBLibVersion.Location = new System.Drawing.Point(209, 35);
            this.CBLibVersion.Name = "CBLibVersion";
            this.CBLibVersion.Size = new System.Drawing.Size(194, 21);
            this.CBLibVersion.TabIndex = 1;
            // 
            // LBLibraries
            // 
            this.LBLibraries.FormattingEnabled = true;
            this.LBLibraries.Location = new System.Drawing.Point(9, 19);
            this.LBLibraries.Name = "LBLibraries";
            this.LBLibraries.Size = new System.Drawing.Size(185, 212);
            this.LBLibraries.TabIndex = 0;
            this.LBLibraries.SelectedIndexChanged += new System.EventHandler(this.LBLibraries_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BTNChangeEmail);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.TBEmail);
            this.groupBox3.Location = new System.Drawing.Point(221, 28);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 90);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Change email";
            // 
            // BTNChangeEmail
            // 
            this.BTNChangeEmail.Location = new System.Drawing.Point(119, 58);
            this.BTNChangeEmail.Name = "BTNChangeEmail";
            this.BTNChangeEmail.Size = new System.Drawing.Size(75, 23);
            this.BTNChangeEmail.TabIndex = 4;
            this.BTNChangeEmail.Text = "Change";
            this.BTNChangeEmail.UseVisualStyleBackColor = true;
            this.BTNChangeEmail.Click += new System.EventHandler(this.BTNChangeEmail_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Email";
            // 
            // TBEmail
            // 
            this.TBEmail.Location = new System.Drawing.Point(9, 32);
            this.TBEmail.Name = "TBEmail";
            this.TBEmail.Size = new System.Drawing.Size(185, 20);
            this.TBEmail.TabIndex = 3;
            // 
            // BTNClose
            // 
            this.BTNClose.Location = new System.Drawing.Point(346, 374);
            this.BTNClose.Name = "BTNClose";
            this.BTNClose.Size = new System.Drawing.Size(75, 23);
            this.BTNClose.TabIndex = 7;
            this.BTNClose.Text = "Close";
            this.BTNClose.UseVisualStyleBackColor = true;
            this.BTNClose.Click += new System.EventHandler(this.BTNClose_Click);
            // 
            // ManageUserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 406);
            this.Controls.Add(this.BTNClose);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LUsername);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManageUserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage User";
            this.Load += new System.EventHandler(this.ManageUserForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LUsername;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TBNewPassword;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BTNChangePassword;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BTNDeleteLibrary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CBLibVersion;
        private System.Windows.Forms.ListBox LBLibraries;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button BTNChangeEmail;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBEmail;
        private System.Windows.Forms.Button BTNClose;
    }
}