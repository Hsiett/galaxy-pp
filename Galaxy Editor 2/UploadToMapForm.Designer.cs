namespace Galaxy_Editor_2
{
    partial class UploadToMapForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.BTNExtract = new System.Windows.Forms.Button();
            this.BTNInject = new System.Windows.Forms.Button();
            this.BTNNeither = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Do you want to extract or inject the script from/to the map?";
            // 
            // BTNExtract
            // 
            this.BTNExtract.Location = new System.Drawing.Point(12, 35);
            this.BTNExtract.Name = "BTNExtract";
            this.BTNExtract.Size = new System.Drawing.Size(75, 23);
            this.BTNExtract.TabIndex = 1;
            this.BTNExtract.Text = "Extract";
            this.BTNExtract.UseVisualStyleBackColor = true;
            this.BTNExtract.Click += new System.EventHandler(this.BTNExtract_Click);
            // 
            // BTNInject
            // 
            this.BTNInject.Location = new System.Drawing.Point(93, 35);
            this.BTNInject.Name = "BTNInject";
            this.BTNInject.Size = new System.Drawing.Size(75, 23);
            this.BTNInject.TabIndex = 2;
            this.BTNInject.Text = "Inject";
            this.BTNInject.UseVisualStyleBackColor = true;
            this.BTNInject.Click += new System.EventHandler(this.BTNInject_Click);
            // 
            // BTNNeither
            // 
            this.BTNNeither.Location = new System.Drawing.Point(221, 35);
            this.BTNNeither.Name = "BTNNeither";
            this.BTNNeither.Size = new System.Drawing.Size(75, 23);
            this.BTNNeither.TabIndex = 3;
            this.BTNNeither.Text = "Neither";
            this.BTNNeither.UseVisualStyleBackColor = true;
            this.BTNNeither.Click += new System.EventHandler(this.BTNNeither_Click);
            // 
            // UploadToMapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 112);
            this.ControlBox = false;
            this.Controls.Add(this.BTNNeither);
            this.Controls.Add(this.BTNInject);
            this.Controls.Add(this.BTNExtract);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UploadToMapForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Get Script";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTNExtract;
        private System.Windows.Forms.Button BTNInject;
        private System.Windows.Forms.Button BTNNeither;
    }
}