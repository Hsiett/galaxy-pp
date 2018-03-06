namespace Galaxy_Editor_2.Suggestion_box
{
    partial class SuggestionBoxForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SuggestionBoxForm));
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.myListbox1 = new Galaxy_Editor_2.Suggestion_box.MyListbox();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.LargeChange = 101;
            this.vScrollBar1.Location = new System.Drawing.Point(265, 2);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 258);
            this.vScrollBar1.SmallChange = 100;
            this.vScrollBar1.TabIndex = 0;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            this.vScrollBar1.MouseEnter += new System.EventHandler(this.vScrollBar1_MouseEnter);
            this.vScrollBar1.MouseLeave += new System.EventHandler(this.vScrollBar1_MouseLeave);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "LocalIcon.bmp");
            this.imageList1.Images.SetKeyName(1, "Parameter.bmp");
            this.imageList1.Images.SetKeyName(2, "FieldIcon.bmp");
            this.imageList1.Images.SetKeyName(3, "ConstLocalIcon.bmp");
            this.imageList1.Images.SetKeyName(4, "ConstFieldIcon.bmp");
            this.imageList1.Images.SetKeyName(5, "MethodIcon.bmp");
            this.imageList1.Images.SetKeyName(6, "StructIcon.bmp");
            // 
            // myListbox1
            // 
            this.myListbox1.CurrentEditor = null;
            this.myListbox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myListbox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.myListbox1.ImageList = this.imageList1;
            this.myListbox1.LineOffset = 0;
            this.myListbox1.Location = new System.Drawing.Point(2, 2);
            this.myListbox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.myListbox1.Name = "myListbox1";
            this.myListbox1.Size = new System.Drawing.Size(263, 258);
            this.myListbox1.TabIndex = 1;
            this.myListbox1.SizeChanged += new System.EventHandler(this.myListbox1_SizeChanged);
            // 
            // SuggestionBoxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.myListbox1);
            this.Controls.Add(this.vScrollBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SuggestionBoxForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.ShowInTaskbar = false;
            this.Text = "SuggestionBoxForm";
            this.VisibleChanged += new System.EventHandler(this.SuggestionBoxForm_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar vScrollBar1;
        private MyListbox myListbox1;
        private System.Windows.Forms.ImageList imageList1;
    }
}