namespace Galaxy_Editor_2.Tooltip
{
    partial class TooltipForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TooltipForm));
            this.myToolboxControl1 = new Galaxy_Editor_2.Tooltip.MyToolboxControl();
            this.SuspendLayout();
            // 
            // myToolboxControl1
            // 
            this.myToolboxControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myToolboxControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.myToolboxControl1.Location = new System.Drawing.Point(1, 1);
            this.myToolboxControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.myToolboxControl1.Name = "myToolboxControl1";
            this.myToolboxControl1.Size = new System.Drawing.Size(5, 0);
            this.myToolboxControl1.TabIndex = 0;
            // 
            // TooltipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(7, 2);
            this.Controls.Add(this.myToolboxControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TooltipForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowInTaskbar = false;
            this.Text = "TooltipForm";
            this.ResumeLayout(false);

        }

        #endregion

        private MyToolboxControl myToolboxControl1;
    }
}