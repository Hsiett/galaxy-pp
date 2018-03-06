namespace Galaxy_Editor_2
{
    partial class DowngradeWindow
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.BTNChangeVer = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(156, 160);
            this.listBox1.TabIndex = 0;
            // 
            // BTNChangeVer
            // 
            this.BTNChangeVer.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNChangeVer.Location = new System.Drawing.Point(12, 178);
            this.BTNChangeVer.Name = "BTNChangeVer";
            this.BTNChangeVer.Size = new System.Drawing.Size(75, 23);
            this.BTNChangeVer.TabIndex = 1;
            this.BTNChangeVer.Text = "Change";
            this.BTNChangeVer.UseVisualStyleBackColor = true;
            // 
            // BTNCancel
            // 
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(93, 178);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 2;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // DowngradeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 328);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNChangeVer);
            this.Controls.Add(this.listBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DowngradeWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DowngradeWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button BTNChangeVer;
        private System.Windows.Forms.Button BTNCancel;
    }
}