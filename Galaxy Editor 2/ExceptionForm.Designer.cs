namespace Galaxy_Editor_2
{
    partial class ExceptionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.userMessage = new System.Windows.Forms.RichTextBox();
            this.exceptionMessage = new System.Windows.Forms.RichTextBox();
            this.BTNSend = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.BTNClose = new System.Windows.Forms.Button();
            this.CBSendCode = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "A critical error has occured.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(345, 39);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please explain what you were doing prior to the crash.\r\nThe stack trace blow will" +
                " tell me what happend and where, but not why.\r\nIf you can, please add code that " +
                "will reproduce the error.";
            // 
            // userMessage
            // 
            this.userMessage.Location = new System.Drawing.Point(12, 65);
            this.userMessage.Name = "userMessage";
            this.userMessage.Size = new System.Drawing.Size(488, 156);
            this.userMessage.TabIndex = 2;
            this.userMessage.Text = "";
            // 
            // exceptionMessage
            // 
            this.exceptionMessage.Location = new System.Drawing.Point(15, 240);
            this.exceptionMessage.Name = "exceptionMessage";
            this.exceptionMessage.ReadOnly = true;
            this.exceptionMessage.Size = new System.Drawing.Size(488, 200);
            this.exceptionMessage.TabIndex = 3;
            this.exceptionMessage.Text = "";
            this.exceptionMessage.WordWrap = false;
            // 
            // BTNSend
            // 
            this.BTNSend.Location = new System.Drawing.Point(387, 448);
            this.BTNSend.Name = "BTNSend";
            this.BTNSend.Size = new System.Drawing.Size(55, 23);
            this.BTNSend.TabIndex = 4;
            this.BTNSend.Text = "Send";
            this.BTNSend.UseVisualStyleBackColor = true;
            this.BTNSend.Click += new System.EventHandler(this.BTNSend_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 224);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(206, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "The following stack trace will also be sent.";
            // 
            // BTNClose
            // 
            this.BTNClose.Location = new System.Drawing.Point(448, 448);
            this.BTNClose.Name = "BTNClose";
            this.BTNClose.Size = new System.Drawing.Size(55, 23);
            this.BTNClose.TabIndex = 6;
            this.BTNClose.Text = "Close";
            this.BTNClose.UseVisualStyleBackColor = true;
            this.BTNClose.Click += new System.EventHandler(this.BTNClose_Click);
            // 
            // CBSendCode
            // 
            this.CBSendCode.AutoSize = true;
            this.CBSendCode.Location = new System.Drawing.Point(15, 452);
            this.CBSendCode.Name = "CBSendCode";
            this.CBSendCode.Size = new System.Drawing.Size(99, 17);
            this.CBSendCode.TabIndex = 7;
            this.CBSendCode.Text = "Also send code";
            this.CBSendCode.UseVisualStyleBackColor = true;
            // 
            // ExceptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 512);
            this.Controls.Add(this.CBSendCode);
            this.Controls.Add(this.BTNClose);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BTNSend);
            this.Controls.Add(this.exceptionMessage);
            this.Controls.Add(this.userMessage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExceptionForm";
            this.Text = "Error report";
            this.Load += new System.EventHandler(this.ExceptionForm_Load);
            this.Shown += new System.EventHandler(this.ExceptionForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox userMessage;
        private System.Windows.Forms.RichTextBox exceptionMessage;
        private System.Windows.Forms.Button BTNSend;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BTNClose;
        private System.Windows.Forms.CheckBox CBSendCode;
    }
}