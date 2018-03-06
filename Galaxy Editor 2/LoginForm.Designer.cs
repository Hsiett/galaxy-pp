namespace Galaxy_Editor_2
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.BTNLogin = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TBUsername = new System.Windows.Forms.TextBox();
            this.TBPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.BTNForgotPassword = new System.Windows.Forms.Button();
            this.PLogin = new System.Windows.Forms.Panel();
            this.PReqReset = new System.Windows.Forms.Panel();
            this.BTNNext1 = new System.Windows.Forms.Button();
            this.TBResetUsername = new System.Windows.Forms.TextBox();
            this.BTNCancel2 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.PReset = new System.Windows.Forms.Panel();
            this.BTNNext2 = new System.Windows.Forms.Button();
            this.BTNCancel3 = new System.Windows.Forms.Button();
            this.TBNewPassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TBResetCode = new System.Windows.Forms.TextBox();
            this.PLogin.SuspendLayout();
            this.PReqReset.SuspendLayout();
            this.PReset.SuspendLayout();
            this.SuspendLayout();
            // 
            // BTNLogin
            // 
            this.BTNLogin.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNLogin.Location = new System.Drawing.Point(12, 90);
            this.BTNLogin.Name = "BTNLogin";
            this.BTNLogin.Size = new System.Drawing.Size(49, 23);
            this.BTNLogin.TabIndex = 0;
            this.BTNLogin.Text = "Login";
            this.BTNLogin.UseVisualStyleBackColor = true;
            this.BTNLogin.Click += new System.EventHandler(this.BTNLogin_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Username";
            // 
            // TBUsername
            // 
            this.TBUsername.Location = new System.Drawing.Point(12, 25);
            this.TBUsername.Name = "TBUsername";
            this.TBUsername.Size = new System.Drawing.Size(211, 20);
            this.TBUsername.TabIndex = 2;
            // 
            // TBPassword
            // 
            this.TBPassword.Location = new System.Drawing.Point(12, 64);
            this.TBPassword.Name = "TBPassword";
            this.TBPassword.PasswordChar = '*';
            this.TBPassword.Size = new System.Drawing.Size(211, 20);
            this.TBPassword.TabIndex = 4;
            this.TBPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBPassword_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(168, 90);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(55, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // BTNForgotPassword
            // 
            this.BTNForgotPassword.Location = new System.Drawing.Point(67, 90);
            this.BTNForgotPassword.Name = "BTNForgotPassword";
            this.BTNForgotPassword.Size = new System.Drawing.Size(95, 23);
            this.BTNForgotPassword.TabIndex = 6;
            this.BTNForgotPassword.Text = "Forgot password";
            this.BTNForgotPassword.UseVisualStyleBackColor = true;
            this.BTNForgotPassword.Click += new System.EventHandler(this.BTNForgotPassword_Click);
            // 
            // PLogin
            // 
            this.PLogin.Controls.Add(this.label1);
            this.PLogin.Controls.Add(this.BTNForgotPassword);
            this.PLogin.Controls.Add(this.BTNLogin);
            this.PLogin.Controls.Add(this.button2);
            this.PLogin.Controls.Add(this.TBUsername);
            this.PLogin.Controls.Add(this.TBPassword);
            this.PLogin.Controls.Add(this.label2);
            this.PLogin.Location = new System.Drawing.Point(0, 0);
            this.PLogin.Name = "PLogin";
            this.PLogin.Size = new System.Drawing.Size(235, 125);
            this.PLogin.TabIndex = 7;
            // 
            // PReqReset
            // 
            this.PReqReset.Controls.Add(this.BTNNext1);
            this.PReqReset.Controls.Add(this.TBResetUsername);
            this.PReqReset.Controls.Add(this.BTNCancel2);
            this.PReqReset.Controls.Add(this.label3);
            this.PReqReset.Location = new System.Drawing.Point(241, 0);
            this.PReqReset.Name = "PReqReset";
            this.PReqReset.Size = new System.Drawing.Size(235, 125);
            this.PReqReset.TabIndex = 8;
            this.PReqReset.Visible = false;
            // 
            // BTNNext1
            // 
            this.BTNNext1.Location = new System.Drawing.Point(116, 90);
            this.BTNNext1.Name = "BTNNext1";
            this.BTNNext1.Size = new System.Drawing.Size(49, 23);
            this.BTNNext1.TabIndex = 7;
            this.BTNNext1.Text = "Next";
            this.BTNNext1.UseVisualStyleBackColor = true;
            this.BTNNext1.Click += new System.EventHandler(this.BTNNext1_Click);
            // 
            // TBResetUsername
            // 
            this.TBResetUsername.Location = new System.Drawing.Point(15, 51);
            this.TBResetUsername.Name = "TBResetUsername";
            this.TBResetUsername.Size = new System.Drawing.Size(211, 20);
            this.TBResetUsername.TabIndex = 7;
            // 
            // BTNCancel2
            // 
            this.BTNCancel2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel2.Location = new System.Drawing.Point(171, 90);
            this.BTNCancel2.Name = "BTNCancel2";
            this.BTNCancel2.Size = new System.Drawing.Size(55, 23);
            this.BTNCancel2.TabIndex = 8;
            this.BTNCancel2.Text = "Cancel";
            this.BTNCancel2.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(199, 39);
            this.label3.TabIndex = 0;
            this.label3.Text = "Enter your username.\r\nA reset code will be sent to the email you\r\nsupplied when r" +
                "egistering.";
            // 
            // PReset
            // 
            this.PReset.Controls.Add(this.BTNNext2);
            this.PReset.Controls.Add(this.BTNCancel3);
            this.PReset.Controls.Add(this.TBNewPassword);
            this.PReset.Controls.Add(this.label5);
            this.PReset.Controls.Add(this.label4);
            this.PReset.Controls.Add(this.TBResetCode);
            this.PReset.Location = new System.Drawing.Point(482, 0);
            this.PReset.Name = "PReset";
            this.PReset.Size = new System.Drawing.Size(235, 125);
            this.PReset.TabIndex = 9;
            this.PReset.Visible = false;
            // 
            // BTNNext2
            // 
            this.BTNNext2.Location = new System.Drawing.Point(113, 90);
            this.BTNNext2.Name = "BTNNext2";
            this.BTNNext2.Size = new System.Drawing.Size(49, 23);
            this.BTNNext2.TabIndex = 11;
            this.BTNNext2.Text = "Next";
            this.BTNNext2.UseVisualStyleBackColor = true;
            this.BTNNext2.Click += new System.EventHandler(this.BTNNext2_Click);
            // 
            // BTNCancel3
            // 
            this.BTNCancel3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel3.Location = new System.Drawing.Point(168, 90);
            this.BTNCancel3.Name = "BTNCancel3";
            this.BTNCancel3.Size = new System.Drawing.Size(55, 23);
            this.BTNCancel3.TabIndex = 12;
            this.BTNCancel3.Text = "Cancel";
            this.BTNCancel3.UseVisualStyleBackColor = true;
            // 
            // TBNewPassword
            // 
            this.TBNewPassword.Location = new System.Drawing.Point(12, 64);
            this.TBNewPassword.Name = "TBNewPassword";
            this.TBNewPassword.PasswordChar = '*';
            this.TBNewPassword.Size = new System.Drawing.Size(211, 20);
            this.TBNewPassword.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "New password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Reset code";
            // 
            // TBResetCode
            // 
            this.TBResetCode.Location = new System.Drawing.Point(12, 25);
            this.TBResetCode.Name = "TBResetCode";
            this.TBResetCode.Size = new System.Drawing.Size(211, 20);
            this.TBResetCode.TabIndex = 9;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 178);
            this.Controls.Add(this.PReset);
            this.Controls.Add(this.PReqReset);
            this.Controls.Add(this.PLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Login";
            this.PLogin.ResumeLayout(false);
            this.PLogin.PerformLayout();
            this.PReqReset.ResumeLayout(false);
            this.PReqReset.PerformLayout();
            this.PReset.ResumeLayout(false);
            this.PReset.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTNLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.TextBox TBUsername;
        public System.Windows.Forms.TextBox TBPassword;
        private System.Windows.Forms.Button BTNForgotPassword;
        private System.Windows.Forms.Panel PLogin;
        private System.Windows.Forms.Panel PReqReset;
        private System.Windows.Forms.Button BTNNext1;
        public System.Windows.Forms.TextBox TBResetUsername;
        private System.Windows.Forms.Button BTNCancel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel PReset;
        private System.Windows.Forms.Button BTNNext2;
        private System.Windows.Forms.Button BTNCancel3;
        public System.Windows.Forms.TextBox TBNewPassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox TBResetCode;
    }
}