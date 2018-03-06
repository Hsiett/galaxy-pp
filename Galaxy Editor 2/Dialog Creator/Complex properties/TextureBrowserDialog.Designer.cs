namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    partial class TextureBrowserDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureBrowserDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.BTNOK = new System.Windows.Forms.Button();
            this.splitter = new System.Windows.Forms.SplitContainer();
            this.TVTextures = new Aga.Controls.Tree.TreeViewAdv();
            this.nodeIcon1 = new Aga.Controls.Tree.NodeControls.NodeIcon();
            this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.TBSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.graphicsControlPanel = new System.Windows.Forms.Panel();
            this.graphicsControl = new Galaxy_Editor_2.Dialog_Creator.GraphicsControl();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.panel2.SuspendLayout();
            this.graphicsControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.BTNOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 360);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(768, 41);
            this.panel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(681, 8);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 21);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // BTNOK
            // 
            this.BTNOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BTNOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOK.Enabled = false;
            this.BTNOK.Location = new System.Drawing.Point(600, 8);
            this.BTNOK.Name = "BTNOK";
            this.BTNOK.Size = new System.Drawing.Size(75, 21);
            this.BTNOK.TabIndex = 0;
            this.BTNOK.Text = "OK";
            this.BTNOK.UseVisualStyleBackColor = true;
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.TVTextures);
            this.splitter.Panel1.Controls.Add(this.panel2);
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.graphicsControlPanel);
            this.splitter.Panel2.SizeChanged += new System.EventHandler(this.splitter_Panel2_SizeChanged);
            this.splitter.Size = new System.Drawing.Size(768, 360);
            this.splitter.SplitterDistance = 412;
            this.splitter.TabIndex = 2;
            // 
            // TVTextures
            // 
            this.TVTextures.BackColor = System.Drawing.SystemColors.Window;
            this.TVTextures.DefaultToolTipProvider = null;
            this.TVTextures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TVTextures.DragDropMarkColor = System.Drawing.Color.Black;
            this.TVTextures.LineColor = System.Drawing.SystemColors.ControlDark;
            this.TVTextures.Location = new System.Drawing.Point(0, 32);
            this.TVTextures.Model = null;
            this.TVTextures.Name = "TVTextures";
            this.TVTextures.NodeControls.Add(this.nodeIcon1);
            this.TVTextures.NodeControls.Add(this.nodeTextBox1);
            this.TVTextures.SelectedNode = null;
            this.TVTextures.Size = new System.Drawing.Size(412, 328);
            this.TVTextures.TabIndex = 1;
            this.TVTextures.Text = "treeViewAdv1";
            this.TVTextures.SelectionChanged += new System.EventHandler(this.TVTextures_SelectionChanged);
            this.TVTextures.Collapsed += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.TVTextures_Collapsed);
            this.TVTextures.Expanded += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.TVTextures_Expanded);
            // 
            // nodeIcon1
            // 
            this.nodeIcon1.DataPropertyName = "Image";
            this.nodeIcon1.LeftMargin = 1;
            this.nodeIcon1.ParentColumn = null;
            this.nodeIcon1.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
            // 
            // nodeTextBox1
            // 
            this.nodeTextBox1.DataPropertyName = "Text";
            this.nodeTextBox1.IncrementalSearchEnabled = true;
            this.nodeTextBox1.LeftMargin = 3;
            this.nodeTextBox1.ParentColumn = null;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TBSearch);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(412, 32);
            this.panel2.TabIndex = 0;
            // 
            // TBSearch
            // 
            this.TBSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TBSearch.Location = new System.Drawing.Point(47, 11);
            this.TBSearch.Name = "TBSearch";
            this.TBSearch.Size = new System.Drawing.Size(365, 21);
            this.TBSearch.TabIndex = 0;
            this.TBSearch.TextChanged += new System.EventHandler(this.TBSearch_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 11);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.label1.Size = new System.Drawing.Size(47, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search:";
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(412, 11);
            this.panel3.TabIndex = 2;
            // 
            // graphicsControlPanel
            // 
            this.graphicsControlPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.graphicsControlPanel.Controls.Add(this.graphicsControl);
            this.graphicsControlPanel.Location = new System.Drawing.Point(3, 3);
            this.graphicsControlPanel.Name = "graphicsControlPanel";
            this.graphicsControlPanel.Size = new System.Drawing.Size(210, 233);
            this.graphicsControlPanel.TabIndex = 0;
            // 
            // graphicsControl
            // 
            this.graphicsControl.DisableMouseControl = false;
            this.graphicsControl.DisplayRace = Galaxy_Editor_2.Dialog_Creator.Enums.Race.Terran;
            this.graphicsControl.EditDisplayRaceOnly = false;
            this.graphicsControl.Location = new System.Drawing.Point(0, 0);
            this.graphicsControl.Name = "graphicsControl";
            this.graphicsControl.Size = new System.Drawing.Size(206, 198);
            this.graphicsControl.TabIndex = 0;
            this.graphicsControl.Text = "graphicsControl1";
            // 
            // TextureBrowserDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 401);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TextureBrowserDialog";
            this.Text = "Texture Browser";
            this.Load += new System.EventHandler(this.TextureBrowserDialog_Load);
            this.panel1.ResumeLayout(false);
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
            this.splitter.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.graphicsControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button BTNOK;
        private System.Windows.Forms.SplitContainer splitter;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox TBSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel graphicsControlPanel;
        private GraphicsControl graphicsControl;
        private Aga.Controls.Tree.TreeViewAdv TVTextures;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
        private Aga.Controls.Tree.NodeControls.NodeIcon nodeIcon1;
    }
}