namespace Galaxy_Editor_2.Dialog_Creator
{
    partial class TestForm
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
            this.dialogCreatorControl1 = new Galaxy_Editor_2.Dialog_Creator.DialogCreatorControl();
            this.SuspendLayout();
            // 
            // dialogCreatorControl1
            // 
            this.dialogCreatorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dialogCreatorControl1.Location = new System.Drawing.Point(0, 0);
            this.dialogCreatorControl1.Name = "dialogCreatorControl1";
            this.dialogCreatorControl1.Size = new System.Drawing.Size(284, 262);
            this.dialogCreatorControl1.TabIndex = 0;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 800);
            this.Controls.Add(this.dialogCreatorControl1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private DialogCreatorControl dialogCreatorControl1;
    }
}