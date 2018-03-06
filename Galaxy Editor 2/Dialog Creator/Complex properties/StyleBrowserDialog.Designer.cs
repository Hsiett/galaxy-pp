namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    partial class StyleBrowserDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StyleBrowserDialog));
            this.LBList = new System.Windows.Forms.ListBox();
            this.TBSearch = new System.Windows.Forms.TextBox();
            this.TBTestText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.BTNOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.graphicsControl1 = new Galaxy_Editor_2.Dialog_Creator.GraphicsControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBList
            // 
            this.LBList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBList.FormattingEnabled = true;
            this.LBList.Location = new System.Drawing.Point(0, 20);
            this.LBList.Name = "LBList";
            this.LBList.Size = new System.Drawing.Size(197, 336);
            this.LBList.Sorted = true;
            this.LBList.TabIndex = 0;
            this.LBList.SelectedIndexChanged += new System.EventHandler(this.LBList_SelectedIndexChanged);
            // 
            // TBSearch
            // 
            this.TBSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.TBSearch.Location = new System.Drawing.Point(0, 0);
            this.TBSearch.Name = "TBSearch";
            this.TBSearch.Size = new System.Drawing.Size(197, 20);
            this.TBSearch.TabIndex = 1;
            this.TBSearch.Visible = false;
            // 
            // TBTestText
            // 
            this.TBTestText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TBTestText.Location = new System.Drawing.Point(0, 369);
            this.TBTestText.Name = "TBTestText";
            this.TBTestText.Size = new System.Drawing.Size(197, 20);
            this.TBTestText.TabIndex = 3;
            this.TBTestText.Text = "Hello world!";
            this.TBTestText.TextChanged += new System.EventHandler(this.TBTestText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Location = new System.Drawing.Point(0, 356);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Test text";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.LBList);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.TBSearch);
            this.splitContainer1.Panel1.Controls.Add(this.TBTestText);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.BTNCancel);
            this.splitContainer1.Panel2.Controls.Add(this.BTNOK);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.graphicsControl1);
            this.splitContainer1.Size = new System.Drawing.Size(596, 389);
            this.splitContainer1.SplitterDistance = 197;
            this.splitContainer1.TabIndex = 5;
            // 
            // BTNCancel
            // 
            this.BTNCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(308, 363);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 3;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // BTNOK
            // 
            this.BTNOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BTNOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOK.Location = new System.Drawing.Point(227, 363);
            this.BTNOK.Name = "BTNOK";
            this.BTNOK.Size = new System.Drawing.Size(75, 23);
            this.BTNOK.TabIndex = 4;
            this.BTNOK.Text = "OK";
            this.BTNOK.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(3, 372);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(209, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Text is not rendered exactly as it is ingame.";
            // 
            // graphicsControl1
            // 
            this.graphicsControl1.DisableMouseControl = false;
            this.graphicsControl1.DisplayRace = Galaxy_Editor_2.Dialog_Creator.Enums.Race.Terran;
            this.graphicsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphicsControl1.EditDisplayRaceOnly = false;
            this.graphicsControl1.Location = new System.Drawing.Point(0, 0);
            this.graphicsControl1.Name = "graphicsControl1";
            this.graphicsControl1.Size = new System.Drawing.Size(395, 389);
            this.graphicsControl1.TabIndex = 2;
            this.graphicsControl1.Text = "graphicsControl1";
            this.graphicsControl1.SizeChanged += new System.EventHandler(this.graphicsControl1_SizeChanged);
            // 
            // StyleBrowserDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 389);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StyleBrowserDialog";
            this.Text = "Style Browser";
            this.Load += new System.EventHandler(this.StyleBrowserDialog_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LBList;
        private System.Windows.Forms.TextBox TBSearch;
        private GraphicsControl graphicsControl1;
        private System.Windows.Forms.TextBox TBTestText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button BTNOK;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.Label label2;
    }
}