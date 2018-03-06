namespace Galaxy_Editor_2.Dialog_Creator
{
    partial class DialogCreatorControl
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.TBMaxInstances = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TBScreenHeight = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CBEditSelectedRaceOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CBViewZerg = new System.Windows.Forms.RadioButton();
            this.CBViewProtoss = new System.Windows.Forms.RadioButton();
            this.CBViewTerran = new System.Windows.Forms.RadioButton();
            this.TPNewControl = new System.Windows.Forms.TabPage();
            this.CBAddPullldown = new System.Windows.Forms.CheckBox();
            this.CBAddSlider = new System.Windows.Forms.CheckBox();
            this.CBAddProgressBar = new System.Windows.Forms.CheckBox();
            this.CBAddListBox = new System.Windows.Forms.CheckBox();
            this.CBAddEditBox = new System.Windows.Forms.CheckBox();
            this.CBAddCheckbox = new System.Windows.Forms.CheckBox();
            this.CBAddLabel = new System.Windows.Forms.CheckBox();
            this.CBNewImage = new System.Windows.Forms.CheckBox();
            this.CBAddButton = new System.Windows.Forms.CheckBox();
            this.CBAddDialog = new System.Windows.Forms.CheckBox();
            this.splitter = new System.Windows.Forms.SplitContainer();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.propertyGridRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.jumpToEventToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CBMainSelectedControl = new System.Windows.Forms.ComboBox();
            this.delete = new System.Windows.Forms.Button();
            this.graphicsControl1 = new Galaxy_Editor_2.Dialog_Creator.GraphicsControl();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.TPNewControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.propertyGridRightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 269);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(578, 109);
            this.panel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.TPNewControl);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(578, 109);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.TBMaxInstances);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(570, 83);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dialog Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // TBMaxInstances
            // 
            this.TBMaxInstances.Location = new System.Drawing.Point(9, 24);
            this.TBMaxInstances.Name = "TBMaxInstances";
            this.TBMaxInstances.Size = new System.Drawing.Size(146, 21);
            this.TBMaxInstances.TabIndex = 6;
            this.TBMaxInstances.TextChanged += new System.EventHandler(this.TBMaxInstances_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "Maximum number of instances";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.TBScreenHeight);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.CBEditSelectedRaceOnly);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(570, 83);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "View/Edit Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TBScreenHeight
            // 
            this.TBScreenHeight.Location = new System.Drawing.Point(87, 59);
            this.TBScreenHeight.Name = "TBScreenHeight";
            this.TBScreenHeight.Size = new System.Drawing.Size(102, 21);
            this.TBScreenHeight.TabIndex = 12;
            this.TBScreenHeight.Text = "1200";
            this.TBScreenHeight.TextChanged += new System.EventHandler(this.TBScreenHeight_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(87, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(221, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "Target screen height (affects scale)";
            // 
            // CBEditSelectedRaceOnly
            // 
            this.CBEditSelectedRaceOnly.AutoSize = true;
            this.CBEditSelectedRaceOnly.Location = new System.Drawing.Point(87, 18);
            this.CBEditSelectedRaceOnly.Name = "CBEditSelectedRaceOnly";
            this.CBEditSelectedRaceOnly.Size = new System.Drawing.Size(570, 16);
            this.CBEditSelectedRaceOnly.TabIndex = 10;
            this.CBEditSelectedRaceOnly.Text = "Edit for selected race only (when possible) - Only for dialog control image/hover" +
    " image atm";
            this.CBEditSelectedRaceOnly.UseVisualStyleBackColor = true;
            this.CBEditSelectedRaceOnly.CheckedChanged += new System.EventHandler(this.CBEditSelectedRaceOnly_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CBViewZerg);
            this.groupBox1.Controls.Add(this.CBViewProtoss);
            this.groupBox1.Controls.Add(this.CBViewTerran);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(78, 77);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "View";
            // 
            // CBViewZerg
            // 
            this.CBViewZerg.AutoSize = true;
            this.CBViewZerg.Location = new System.Drawing.Point(6, 57);
            this.CBViewZerg.Name = "CBViewZerg";
            this.CBViewZerg.Size = new System.Drawing.Size(47, 16);
            this.CBViewZerg.TabIndex = 6;
            this.CBViewZerg.Text = "Zerg";
            this.CBViewZerg.UseVisualStyleBackColor = true;
            this.CBViewZerg.CheckedChanged += new System.EventHandler(this.CBViewZerg_CheckedChanged);
            // 
            // CBViewProtoss
            // 
            this.CBViewProtoss.AutoSize = true;
            this.CBViewProtoss.Location = new System.Drawing.Point(6, 36);
            this.CBViewProtoss.Name = "CBViewProtoss";
            this.CBViewProtoss.Size = new System.Drawing.Size(65, 16);
            this.CBViewProtoss.TabIndex = 5;
            this.CBViewProtoss.Text = "Protoss";
            this.CBViewProtoss.UseVisualStyleBackColor = true;
            this.CBViewProtoss.CheckedChanged += new System.EventHandler(this.CBViewProtoss_CheckedChanged);
            // 
            // CBViewTerran
            // 
            this.CBViewTerran.AutoSize = true;
            this.CBViewTerran.Checked = true;
            this.CBViewTerran.Location = new System.Drawing.Point(6, 15);
            this.CBViewTerran.Name = "CBViewTerran";
            this.CBViewTerran.Size = new System.Drawing.Size(59, 16);
            this.CBViewTerran.TabIndex = 4;
            this.CBViewTerran.TabStop = true;
            this.CBViewTerran.Text = "Terran";
            this.CBViewTerran.UseVisualStyleBackColor = true;
            this.CBViewTerran.CheckedChanged += new System.EventHandler(this.CBViewTerran_CheckedChanged);
            // 
            // TPNewControl
            // 
            this.TPNewControl.Controls.Add(this.delete);
            this.TPNewControl.Controls.Add(this.CBAddPullldown);
            this.TPNewControl.Controls.Add(this.CBAddSlider);
            this.TPNewControl.Controls.Add(this.CBAddProgressBar);
            this.TPNewControl.Controls.Add(this.CBAddListBox);
            this.TPNewControl.Controls.Add(this.CBAddEditBox);
            this.TPNewControl.Controls.Add(this.CBAddCheckbox);
            this.TPNewControl.Controls.Add(this.CBAddLabel);
            this.TPNewControl.Controls.Add(this.CBNewImage);
            this.TPNewControl.Controls.Add(this.CBAddButton);
            this.TPNewControl.Controls.Add(this.CBAddDialog);
            this.TPNewControl.Location = new System.Drawing.Point(4, 22);
            this.TPNewControl.Name = "TPNewControl";
            this.TPNewControl.Size = new System.Drawing.Size(570, 83);
            this.TPNewControl.TabIndex = 3;
            this.TPNewControl.Text = "New Control";
            this.TPNewControl.UseVisualStyleBackColor = true;
            // 
            // CBAddPullldown
            // 
            this.CBAddPullldown.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddPullldown.AutoSize = true;
            this.CBAddPullldown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddPullldown.Location = new System.Drawing.Point(163, 3);
            this.CBAddPullldown.Name = "CBAddPullldown";
            this.CBAddPullldown.Size = new System.Drawing.Size(63, 22);
            this.CBAddPullldown.TabIndex = 14;
            this.CBAddPullldown.Text = "Pulldown";
            this.CBAddPullldown.UseVisualStyleBackColor = true;
            this.CBAddPullldown.CheckedChanged += new System.EventHandler(this.CBAddPullldown_CheckedChanged);
            // 
            // CBAddSlider
            // 
            this.CBAddSlider.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddSlider.AutoSize = true;
            this.CBAddSlider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddSlider.Location = new System.Drawing.Point(128, 30);
            this.CBAddSlider.Name = "CBAddSlider";
            this.CBAddSlider.Size = new System.Drawing.Size(51, 22);
            this.CBAddSlider.TabIndex = 13;
            this.CBAddSlider.Text = "Slider";
            this.CBAddSlider.UseVisualStyleBackColor = true;
            this.CBAddSlider.CheckedChanged += new System.EventHandler(this.CBAddSlider_CheckedChanged);
            // 
            // CBAddProgressBar
            // 
            this.CBAddProgressBar.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddProgressBar.AutoSize = true;
            this.CBAddProgressBar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddProgressBar.Location = new System.Drawing.Point(116, 56);
            this.CBAddProgressBar.Name = "CBAddProgressBar";
            this.CBAddProgressBar.Size = new System.Drawing.Size(87, 22);
            this.CBAddProgressBar.TabIndex = 12;
            this.CBAddProgressBar.Text = "Progress Bar";
            this.CBAddProgressBar.UseVisualStyleBackColor = true;
            this.CBAddProgressBar.CheckedChanged += new System.EventHandler(this.CBAddProgressBar_CheckedChanged);
            // 
            // CBAddListBox
            // 
            this.CBAddListBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddListBox.AutoSize = true;
            this.CBAddListBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddListBox.Location = new System.Drawing.Point(106, 3);
            this.CBAddListBox.Name = "CBAddListBox";
            this.CBAddListBox.Size = new System.Drawing.Size(57, 22);
            this.CBAddListBox.TabIndex = 11;
            this.CBAddListBox.Text = "ListBox";
            this.CBAddListBox.UseVisualStyleBackColor = true;
            this.CBAddListBox.CheckedChanged += new System.EventHandler(this.CBAddListBox_CheckedChanged);
            // 
            // CBAddEditBox
            // 
            this.CBAddEditBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddEditBox.AutoSize = true;
            this.CBAddEditBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddEditBox.Location = new System.Drawing.Point(57, 56);
            this.CBAddEditBox.Name = "CBAddEditBox";
            this.CBAddEditBox.Size = new System.Drawing.Size(57, 22);
            this.CBAddEditBox.TabIndex = 10;
            this.CBAddEditBox.Text = "EditBox";
            this.CBAddEditBox.UseVisualStyleBackColor = true;
            this.CBAddEditBox.CheckedChanged += new System.EventHandler(this.CBAddEditBox_CheckedChanged);
            // 
            // CBAddCheckbox
            // 
            this.CBAddCheckbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddCheckbox.AutoSize = true;
            this.CBAddCheckbox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddCheckbox.Location = new System.Drawing.Point(57, 30);
            this.CBAddCheckbox.Name = "CBAddCheckbox";
            this.CBAddCheckbox.Size = new System.Drawing.Size(63, 22);
            this.CBAddCheckbox.TabIndex = 9;
            this.CBAddCheckbox.Text = "Checkbox";
            this.CBAddCheckbox.UseVisualStyleBackColor = true;
            this.CBAddCheckbox.CheckedChanged += new System.EventHandler(this.CBAddCheckbox_CheckedChanged);
            // 
            // CBAddLabel
            // 
            this.CBAddLabel.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddLabel.AutoSize = true;
            this.CBAddLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddLabel.Location = new System.Drawing.Point(57, 3);
            this.CBAddLabel.Name = "CBAddLabel";
            this.CBAddLabel.Size = new System.Drawing.Size(45, 22);
            this.CBAddLabel.TabIndex = 8;
            this.CBAddLabel.Text = "Label";
            this.CBAddLabel.UseVisualStyleBackColor = true;
            this.CBAddLabel.CheckedChanged += new System.EventHandler(this.CBAddLabel_CheckedChanged);
            // 
            // CBNewImage
            // 
            this.CBNewImage.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBNewImage.AutoSize = true;
            this.CBNewImage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBNewImage.Location = new System.Drawing.Point(3, 56);
            this.CBNewImage.Name = "CBNewImage";
            this.CBNewImage.Size = new System.Drawing.Size(45, 22);
            this.CBNewImage.TabIndex = 7;
            this.CBNewImage.Text = "Image";
            this.CBNewImage.UseVisualStyleBackColor = true;
            this.CBNewImage.CheckedChanged += new System.EventHandler(this.CBNewImage_CheckedChanged);
            // 
            // CBAddButton
            // 
            this.CBAddButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddButton.AutoSize = true;
            this.CBAddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddButton.Location = new System.Drawing.Point(3, 30);
            this.CBAddButton.Name = "CBAddButton";
            this.CBAddButton.Size = new System.Drawing.Size(51, 22);
            this.CBAddButton.TabIndex = 6;
            this.CBAddButton.Text = "Button";
            this.CBAddButton.UseVisualStyleBackColor = true;
            this.CBAddButton.CheckedChanged += new System.EventHandler(this.CBAddButton_CheckedChanged);
            // 
            // CBAddDialog
            // 
            this.CBAddDialog.Appearance = System.Windows.Forms.Appearance.Button;
            this.CBAddDialog.AutoSize = true;
            this.CBAddDialog.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CBAddDialog.Location = new System.Drawing.Point(3, 3);
            this.CBAddDialog.Name = "CBAddDialog";
            this.CBAddDialog.Size = new System.Drawing.Size(51, 22);
            this.CBAddDialog.TabIndex = 5;
            this.CBAddDialog.Text = "Dialog";
            this.CBAddDialog.UseVisualStyleBackColor = true;
            this.CBAddDialog.CheckedChanged += new System.EventHandler(this.CBAddDialog_CheckedChanged);
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.graphicsControl1);
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.propertyGrid);
            this.splitter.Panel2.Controls.Add(this.CBMainSelectedControl);
            this.splitter.Size = new System.Drawing.Size(578, 269);
            this.splitter.SplitterDistance = 403;
            this.splitter.TabIndex = 1;
            this.splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter_SplitterMoved);
            // 
            // propertyGrid
            // 
            this.propertyGrid.ContextMenuStrip = this.propertyGridRightClickMenu;
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 20);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(171, 249);
            this.propertyGrid.TabIndex = 2;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            this.propertyGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid_SelectedGridItemChanged);
            // 
            // propertyGridRightClickMenu
            // 
            this.propertyGridRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.jumpToEventToolStripMenuItem});
            this.propertyGridRightClickMenu.Name = "propertyGridRightClickMenu";
            this.propertyGridRightClickMenu.Size = new System.Drawing.Size(159, 26);
            // 
            // jumpToEventToolStripMenuItem
            // 
            this.jumpToEventToolStripMenuItem.Enabled = false;
            this.jumpToEventToolStripMenuItem.Name = "jumpToEventToolStripMenuItem";
            this.jumpToEventToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.jumpToEventToolStripMenuItem.Text = "Jump to Event";
            this.jumpToEventToolStripMenuItem.Click += new System.EventHandler(this.jumpToEventToolStripMenuItem_Click);
            // 
            // CBMainSelectedControl
            // 
            this.CBMainSelectedControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.CBMainSelectedControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMainSelectedControl.FormattingEnabled = true;
            this.CBMainSelectedControl.Location = new System.Drawing.Point(0, 0);
            this.CBMainSelectedControl.Name = "CBMainSelectedControl";
            this.CBMainSelectedControl.Size = new System.Drawing.Size(171, 20);
            this.CBMainSelectedControl.Sorted = true;
            this.CBMainSelectedControl.TabIndex = 0;
            this.CBMainSelectedControl.SelectedIndexChanged += new System.EventHandler(this.CBMainSelectedControl_SelectedIndexChanged);
            // 
            // delete
            // 
            this.delete.Location = new System.Drawing.Point(232, 3);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(75, 23);
            this.delete.TabIndex = 15;
            this.delete.Text = "delete";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // graphicsControl1
            // 
            this.graphicsControl1.DisableMouseControl = false;
            this.graphicsControl1.DisplayRace = Galaxy_Editor_2.Dialog_Creator.Enums.Race.Terran;
            this.graphicsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphicsControl1.EditDisplayRaceOnly = false;
            this.graphicsControl1.Location = new System.Drawing.Point(0, 0);
            this.graphicsControl1.Name = "graphicsControl1";
            this.graphicsControl1.Size = new System.Drawing.Size(403, 269);
            this.graphicsControl1.TabIndex = 0;
            this.graphicsControl1.Text = "graphicsControl1";
            // 
            // DialogCreatorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.panel1);
            this.Name = "DialogCreatorControl";
            this.Size = new System.Drawing.Size(578, 378);
            this.Load += new System.EventHandler(this.DialogCreatorControl_Load);
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.TPNewControl.ResumeLayout(false);
            this.TPNewControl.PerformLayout();
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
            this.splitter.ResumeLayout(false);
            this.propertyGridRightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitter;
        private System.Windows.Forms.ComboBox CBMainSelectedControl;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private GraphicsControl graphicsControl1;
        private System.Windows.Forms.TextBox TBMaxInstances;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage TPNewControl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton CBViewZerg;
        private System.Windows.Forms.RadioButton CBViewProtoss;
        private System.Windows.Forms.RadioButton CBViewTerran;
        private System.Windows.Forms.CheckBox CBAddDialog;
        private System.Windows.Forms.CheckBox CBEditSelectedRaceOnly;
        private System.Windows.Forms.TextBox TBScreenHeight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CBAddButton;
        private System.Windows.Forms.CheckBox CBNewImage;
        private System.Windows.Forms.CheckBox CBAddLabel;
        private System.Windows.Forms.CheckBox CBAddCheckbox;
        private System.Windows.Forms.CheckBox CBAddEditBox;
        private System.Windows.Forms.ContextMenuStrip propertyGridRightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem jumpToEventToolStripMenuItem;
        private System.Windows.Forms.CheckBox CBAddListBox;
        private System.Windows.Forms.CheckBox CBAddProgressBar;
        private System.Windows.Forms.CheckBox CBAddSlider;
        private System.Windows.Forms.CheckBox CBAddPullldown;
        private System.Windows.Forms.Button delete;
    }
}
