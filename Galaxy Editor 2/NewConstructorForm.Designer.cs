namespace Galaxy_Editor_2
{
    partial class NewConstructorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewConstructorForm));
            this.label1 = new System.Windows.Forms.Label();
            this.BTNOK = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.checkBoxPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(297, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the variables you want to initialize.\r\nThey will be added as arguments in " +
                "the order you check them";
            // 
            // BTNOK
            // 
            this.BTNOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOK.Location = new System.Drawing.Point(12, 325);
            this.BTNOK.Name = "BTNOK";
            this.BTNOK.Size = new System.Drawing.Size(75, 23);
            this.BTNOK.TabIndex = 1;
            this.BTNOK.Text = "OK";
            this.BTNOK.UseVisualStyleBackColor = true;
            // 
            // BTNCancel
            // 
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(93, 325);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 2;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxPanel
            // 
            this.checkBoxPanel.AutoScroll = true;
            this.checkBoxPanel.BackColor = System.Drawing.Color.White;
            this.checkBoxPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.checkBoxPanel.Location = new System.Drawing.Point(0, 38);
            this.checkBoxPanel.Name = "checkBoxPanel";
            this.checkBoxPanel.Size = new System.Drawing.Size(428, 281);
            this.checkBoxPanel.TabIndex = 3;
            // 
            // NewConstructorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 360);
            this.Controls.Add(this.checkBoxPanel);
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNOK);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewConstructorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Constructor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTNOK;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.Panel checkBoxPanel;
    }
}