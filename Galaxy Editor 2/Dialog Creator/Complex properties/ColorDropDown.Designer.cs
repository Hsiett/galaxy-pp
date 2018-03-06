namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    partial class ColorDropDown
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BTNColor = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.NUDAlpha = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.NUDAlpha)).BeginInit();
            this.SuspendLayout();
            // 
            // BTNColor
            // 
            this.BTNColor.BackColor = System.Drawing.Color.Black;
            this.BTNColor.Location = new System.Drawing.Point(43, 3);
            this.BTNColor.Name = "BTNColor";
            this.BTNColor.Size = new System.Drawing.Size(54, 23);
            this.BTNColor.TabIndex = 0;
            this.BTNColor.UseVisualStyleBackColor = false;
            this.BTNColor.Click += new System.EventHandler(this.BTNColor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Color:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Alpha:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "%";
            // 
            // NUDAlpha
            // 
            this.NUDAlpha.Location = new System.Drawing.Point(43, 32);
            this.NUDAlpha.Name = "NUDAlpha";
            this.NUDAlpha.Size = new System.Drawing.Size(54, 20);
            this.NUDAlpha.TabIndex = 5;
            this.NUDAlpha.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.NUDAlpha.ValueChanged += new System.EventHandler(this.NUDAlpha_ValueChanged);
            // 
            // ColorDropDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NUDAlpha);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BTNColor);
            this.Name = "ColorDropDown";
            this.Size = new System.Drawing.Size(122, 58);
            ((System.ComponentModel.ISupportInitialize)(this.NUDAlpha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTNColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown NUDAlpha;
    }
}
