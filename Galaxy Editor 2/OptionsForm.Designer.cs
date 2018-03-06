namespace Galaxy_Editor_2
{
    partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.BTNOkay = new System.Windows.Forms.Button();
            this.tabStrip = new FarsiLibrary.Win.FATabStrip();
            this.compilerTab = new FarsiLibrary.Win.FATabStripItem();
            this.CBCAutoInline = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.CBCNeverAskToOpenSavedFile = new System.Windows.Forms.CheckBox();
            this.CBCRunCopy = new System.Windows.Forms.CheckBox();
            this.TBCMapBackups = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CBCObfuscateStrings = new System.Windows.Forms.CheckBox();
            this.CBCShortNames = new System.Windows.Forms.CheckBox();
            this.CBCOneFile = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CBCRemoveMethods = new System.Windows.Forms.CheckBox();
            this.CBCRemoveStructs = new System.Windows.Forms.CheckBox();
            this.CBCRemoveFields = new System.Windows.Forms.CheckBox();
            this.editorTab = new FarsiLibrary.Win.FATabStripItem();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CBEPickFontContext = new System.Windows.Forms.ComboBox();
            this.CBEFontStrikeout = new System.Windows.Forms.CheckBox();
            this.BTNEFontColor = new System.Windows.Forms.Button();
            this.CBEFontUnderline = new System.Windows.Forms.CheckBox();
            this.CBEBoldFond = new System.Windows.Forms.CheckBox();
            this.CBEItalicsFont = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TBECharWidth = new System.Windows.Forms.TextBox();
            this.BTNEFontPick = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbEditorReadOnlyOut = new System.Windows.Forms.CheckBox();
            this.CBEOpenPreviousProjectAtLaunch = new System.Windows.Forms.CheckBox();
            this.cbEditorReplaceTabs = new System.Windows.Forms.CheckBox();
            this.CBEInsertEndBracket = new System.Windows.Forms.CheckBox();
            this.runOptionsTab = new FarsiLibrary.Win.FATabStripItem();
            this.TBROAdditionalArgs = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.CBROAllowCheat = new System.Windows.Forms.CheckBox();
            this.CBROEnablePreload = new System.Windows.Forms.CheckBox();
            this.CBROWindowed = new System.Windows.Forms.CheckBox();
            this.CBROShowTriggerDebug = new System.Windows.Forms.CheckBox();
            this.TBROSeed = new System.Windows.Forms.TextBox();
            this.LROSeed = new System.Windows.Forms.Label();
            this.CBROFixedSeed = new System.Windows.Forms.CheckBox();
            this.CBROGameSpeed = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.CBRODifficulty = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabStrip)).BeginInit();
            this.tabStrip.SuspendLayout();
            this.compilerTab.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.editorTab.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.runOptionsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BTNOkay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 377);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 30);
            this.panel1.TabIndex = 1;
            // 
            // BTNOkay
            // 
            this.BTNOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOkay.Location = new System.Drawing.Point(12, 3);
            this.BTNOkay.Name = "BTNOkay";
            this.BTNOkay.Size = new System.Drawing.Size(75, 23);
            this.BTNOkay.TabIndex = 0;
            this.BTNOkay.Text = "Okay";
            this.BTNOkay.UseVisualStyleBackColor = true;
            // 
            // tabStrip
            // 
            this.tabStrip.AlwaysShowClose = false;
            this.tabStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabStrip.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.tabStrip.Items.AddRange(new FarsiLibrary.Win.FATabStripItem[] {
            this.compilerTab,
            this.editorTab,
            this.runOptionsTab});
            this.tabStrip.Location = new System.Drawing.Point(0, 0);
            this.tabStrip.Name = "tabStrip";
            this.tabStrip.SelectedItem = this.runOptionsTab;
            this.tabStrip.Size = new System.Drawing.Size(324, 407);
            this.tabStrip.TabIndex = 0;
            this.tabStrip.Text = "faTabStrip1";
            // 
            // compilerTab
            // 
            this.compilerTab.CanClose = false;
            this.compilerTab.Controls.Add(this.CBCAutoInline);
            this.compilerTab.Controls.Add(this.groupBox8);
            this.compilerTab.Controls.Add(this.groupBox3);
            this.compilerTab.Controls.Add(this.groupBox2);
            this.compilerTab.Controls.Add(this.groupBox1);
            this.compilerTab.IsDrawn = true;
            this.compilerTab.Name = "compilerTab";
            this.compilerTab.Size = new System.Drawing.Size(322, 386);
            this.compilerTab.TabIndex = 0;
            this.compilerTab.Title = "Compiler";
            // 
            // CBCAutoInline
            // 
            this.CBCAutoInline.AutoSize = true;
            this.CBCAutoInline.Location = new System.Drawing.Point(9, 129);
            this.CBCAutoInline.Name = "CBCAutoInline";
            this.CBCAutoInline.Size = new System.Drawing.Size(189, 17);
            this.CBCAutoInline.TabIndex = 3;
            this.CBCAutoInline.Text = "Automatically inline short methods";
            this.CBCAutoInline.UseVisualStyleBackColor = true;
            this.CBCAutoInline.CheckedChanged += new System.EventHandler(this.CBCAutoInline_CheckedChanged);
            // 
            // groupBox8
            // 
            this.groupBox8.Location = new System.Drawing.Point(3, 109);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(316, 45);
            this.groupBox8.TabIndex = 7;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Optimizations";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.CBCNeverAskToOpenSavedFile);
            this.groupBox3.Controls.Add(this.CBCRunCopy);
            this.groupBox3.Controls.Add(this.TBCMapBackups);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(3, 160);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(311, 122);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "After compile options";
            // 
            // CBCNeverAskToOpenSavedFile
            // 
            this.CBCNeverAskToOpenSavedFile.AutoSize = true;
            this.CBCNeverAskToOpenSavedFile.Location = new System.Drawing.Point(6, 96);
            this.CBCNeverAskToOpenSavedFile.Name = "CBCNeverAskToOpenSavedFile";
            this.CBCNeverAskToOpenSavedFile.Size = new System.Drawing.Size(263, 17);
            this.CBCNeverAskToOpenSavedFile.TabIndex = 4;
            this.CBCNeverAskToOpenSavedFile.Text = "Never ask to open in the normal editor after save";
            this.CBCNeverAskToOpenSavedFile.UseVisualStyleBackColor = true;
            this.CBCNeverAskToOpenSavedFile.CheckedChanged += new System.EventHandler(this.CBCNeverAskToOpenSavedFile_CheckedChanged);
            // 
            // CBCRunCopy
            // 
            this.CBCRunCopy.AutoSize = true;
            this.CBCRunCopy.Location = new System.Drawing.Point(6, 73);
            this.CBCRunCopy.Name = "CBCRunCopy";
            this.CBCRunCopy.Size = new System.Drawing.Size(198, 17);
            this.CBCRunCopy.TabIndex = 3;
            this.CBCRunCopy.Text = "Copy to, and run a copy of the map";
            this.CBCRunCopy.UseVisualStyleBackColor = true;
            this.CBCRunCopy.CheckedChanged += new System.EventHandler(this.CBCRunCopy_CheckedChanged);
            // 
            // TBCMapBackups
            // 
            this.TBCMapBackups.Location = new System.Drawing.Point(6, 46);
            this.TBCMapBackups.Name = "TBCMapBackups";
            this.TBCMapBackups.Size = new System.Drawing.Size(77, 21);
            this.TBCMapBackups.TabIndex = 2;
            this.TBCMapBackups.Text = "1";
            this.TBCMapBackups.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TBCMapBackups.TextChanged += new System.EventHandler(this.TBCMapBackups_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "How many backups should be kept before overwriting?";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(293, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "The program will backup your map before copying files to it.";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CBCObfuscateStrings);
            this.groupBox2.Controls.Add(this.CBCShortNames);
            this.groupBox2.Controls.Add(this.CBCOneFile);
            this.groupBox2.Location = new System.Drawing.Point(172, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(142, 100);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output format";
            // 
            // CBCObfuscateStrings
            // 
            this.CBCObfuscateStrings.AutoSize = true;
            this.CBCObfuscateStrings.Location = new System.Drawing.Point(6, 66);
            this.CBCObfuscateStrings.Name = "CBCObfuscateStrings";
            this.CBCObfuscateStrings.Size = new System.Drawing.Size(111, 17);
            this.CBCObfuscateStrings.TabIndex = 2;
            this.CBCObfuscateStrings.Text = "Obfuscate strings";
            this.CBCObfuscateStrings.UseVisualStyleBackColor = true;
            this.CBCObfuscateStrings.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // CBCShortNames
            // 
            this.CBCShortNames.AutoSize = true;
            this.CBCShortNames.Location = new System.Drawing.Point(6, 43);
            this.CBCShortNames.Name = "CBCShortNames";
            this.CBCShortNames.Size = new System.Drawing.Size(113, 17);
            this.CBCShortNames.TabIndex = 1;
            this.CBCShortNames.Text = "Make short names";
            this.CBCShortNames.UseVisualStyleBackColor = true;
            this.CBCShortNames.CheckedChanged += new System.EventHandler(this.CBCShortNames_CheckedChanged);
            // 
            // CBCOneFile
            // 
            this.CBCOneFile.AutoSize = true;
            this.CBCOneFile.Location = new System.Drawing.Point(6, 20);
            this.CBCOneFile.Name = "CBCOneFile";
            this.CBCOneFile.Size = new System.Drawing.Size(131, 17);
            this.CBCOneFile.TabIndex = 0;
            this.CBCOneFile.Text = "Join output to one file";
            this.CBCOneFile.UseVisualStyleBackColor = true;
            this.CBCOneFile.CheckedChanged += new System.EventHandler(this.CBCOneFile_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CBCRemoveMethods);
            this.groupBox1.Controls.Add(this.CBCRemoveStructs);
            this.groupBox1.Controls.Add(this.CBCRemoveFields);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 100);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Unused declarations";
            // 
            // CBCRemoveMethods
            // 
            this.CBCRemoveMethods.AutoSize = true;
            this.CBCRemoveMethods.Location = new System.Drawing.Point(6, 20);
            this.CBCRemoveMethods.Name = "CBCRemoveMethods";
            this.CBCRemoveMethods.Size = new System.Drawing.Size(147, 17);
            this.CBCRemoveMethods.TabIndex = 1;
            this.CBCRemoveMethods.Text = "Remove unused methods";
            this.CBCRemoveMethods.UseVisualStyleBackColor = true;
            this.CBCRemoveMethods.CheckedChanged += new System.EventHandler(this.CBCRemoveDecls_CheckedChanged);
            // 
            // CBCRemoveStructs
            // 
            this.CBCRemoveStructs.AutoSize = true;
            this.CBCRemoveStructs.Location = new System.Drawing.Point(6, 66);
            this.CBCRemoveStructs.Name = "CBCRemoveStructs";
            this.CBCRemoveStructs.Size = new System.Drawing.Size(139, 17);
            this.CBCRemoveStructs.TabIndex = 3;
            this.CBCRemoveStructs.Text = "Remove unused structs\r\n";
            this.CBCRemoveStructs.UseVisualStyleBackColor = true;
            this.CBCRemoveStructs.CheckedChanged += new System.EventHandler(this.CBCRemoveDecls_CheckedChanged);
            // 
            // CBCRemoveFields
            // 
            this.CBCRemoveFields.AutoSize = true;
            this.CBCRemoveFields.Location = new System.Drawing.Point(6, 43);
            this.CBCRemoveFields.Name = "CBCRemoveFields";
            this.CBCRemoveFields.Size = new System.Drawing.Size(131, 17);
            this.CBCRemoveFields.TabIndex = 2;
            this.CBCRemoveFields.Text = "Remove unused fields";
            this.CBCRemoveFields.UseVisualStyleBackColor = true;
            this.CBCRemoveFields.CheckedChanged += new System.EventHandler(this.CBCRemoveDecls_CheckedChanged);
            // 
            // editorTab
            // 
            this.editorTab.CanClose = false;
            this.editorTab.Controls.Add(this.groupBox5);
            this.editorTab.Controls.Add(this.groupBox4);
            this.editorTab.IsDrawn = true;
            this.editorTab.Name = "editorTab";
            this.editorTab.Size = new System.Drawing.Size(322, 386);
            this.editorTab.TabIndex = 1;
            this.editorTab.Title = "Editor";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.groupBox7);
            this.groupBox5.Controls.Add(this.groupBox6);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox5.Location = new System.Drawing.Point(0, 112);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(322, 247);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Fonts";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label3);
            this.groupBox7.Controls.Add(this.CBEPickFontContext);
            this.groupBox7.Controls.Add(this.CBEFontStrikeout);
            this.groupBox7.Controls.Add(this.BTNEFontColor);
            this.groupBox7.Controls.Add(this.CBEFontUnderline);
            this.groupBox7.Controls.Add(this.CBEBoldFond);
            this.groupBox7.Controls.Add(this.CBEItalicsFont);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox7.Location = new System.Drawing.Point(3, 109);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(316, 135);
            this.groupBox7.TabIndex = 8;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Context specific modifications";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Context";
            // 
            // CBEPickFontContext
            // 
            this.CBEPickFontContext.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBEPickFontContext.FormattingEnabled = true;
            this.CBEPickFontContext.Items.AddRange(new object[] {
            "Normal text",
            "Primitive types",
            "Strings",
            "Comments",
            "Keywords",
            "Native library function calls",
            "Structs"});
            this.CBEPickFontContext.Location = new System.Drawing.Point(8, 33);
            this.CBEPickFontContext.Name = "CBEPickFontContext";
            this.CBEPickFontContext.Size = new System.Drawing.Size(183, 21);
            this.CBEPickFontContext.TabIndex = 1;
            this.CBEPickFontContext.SelectedIndexChanged += new System.EventHandler(this.CBEPickFontContext_SelectedIndexChanged);
            // 
            // CBEFontStrikeout
            // 
            this.CBEFontStrikeout.AutoSize = true;
            this.CBEFontStrikeout.Location = new System.Drawing.Point(68, 82);
            this.CBEFontStrikeout.Name = "CBEFontStrikeout";
            this.CBEFontStrikeout.Size = new System.Drawing.Size(69, 17);
            this.CBEFontStrikeout.TabIndex = 6;
            this.CBEFontStrikeout.Text = "Strikeout";
            this.CBEFontStrikeout.UseVisualStyleBackColor = true;
            this.CBEFontStrikeout.CheckedChanged += new System.EventHandler(this.FontCheckBoxes_CheckedChanged);
            // 
            // BTNEFontColor
            // 
            this.BTNEFontColor.Location = new System.Drawing.Point(8, 105);
            this.BTNEFontColor.Name = "BTNEFontColor";
            this.BTNEFontColor.Size = new System.Drawing.Size(66, 23);
            this.BTNEFontColor.TabIndex = 2;
            this.BTNEFontColor.Text = "Pick color";
            this.BTNEFontColor.UseVisualStyleBackColor = true;
            this.BTNEFontColor.Click += new System.EventHandler(this.BTNEFontColor_Click);
            // 
            // CBEFontUnderline
            // 
            this.CBEFontUnderline.AutoSize = true;
            this.CBEFontUnderline.Location = new System.Drawing.Point(68, 60);
            this.CBEFontUnderline.Name = "CBEFontUnderline";
            this.CBEFontUnderline.Size = new System.Drawing.Size(71, 17);
            this.CBEFontUnderline.TabIndex = 5;
            this.CBEFontUnderline.Text = "Underline";
            this.CBEFontUnderline.UseVisualStyleBackColor = true;
            this.CBEFontUnderline.CheckedChanged += new System.EventHandler(this.FontCheckBoxes_CheckedChanged);
            // 
            // CBEBoldFond
            // 
            this.CBEBoldFond.AutoSize = true;
            this.CBEBoldFond.Location = new System.Drawing.Point(8, 60);
            this.CBEBoldFond.Name = "CBEBoldFond";
            this.CBEBoldFond.Size = new System.Drawing.Size(46, 17);
            this.CBEBoldFond.TabIndex = 3;
            this.CBEBoldFond.Text = "Bold";
            this.CBEBoldFond.UseVisualStyleBackColor = true;
            this.CBEBoldFond.CheckedChanged += new System.EventHandler(this.FontCheckBoxes_CheckedChanged);
            // 
            // CBEItalicsFont
            // 
            this.CBEItalicsFont.AutoSize = true;
            this.CBEItalicsFont.Location = new System.Drawing.Point(8, 82);
            this.CBEItalicsFont.Name = "CBEItalicsFont";
            this.CBEItalicsFont.Size = new System.Drawing.Size(54, 17);
            this.CBEItalicsFont.TabIndex = 4;
            this.CBEItalicsFont.Text = "Italics";
            this.CBEItalicsFont.UseVisualStyleBackColor = true;
            this.CBEItalicsFont.CheckedChanged += new System.EventHandler(this.FontCheckBoxes_CheckedChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label4);
            this.groupBox6.Controls.Add(this.TBECharWidth);
            this.groupBox6.Controls.Add(this.BTNEFontPick);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox6.Location = new System.Drawing.Point(3, 17);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(316, 92);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Base font";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Char width";
            // 
            // TBECharWidth
            // 
            this.TBECharWidth.Location = new System.Drawing.Point(11, 62);
            this.TBECharWidth.Name = "TBECharWidth";
            this.TBECharWidth.Size = new System.Drawing.Size(100, 21);
            this.TBECharWidth.TabIndex = 1;
            this.TBECharWidth.TextChanged += new System.EventHandler(this.TBECharWidth_TextChanged);
            // 
            // BTNEFontPick
            // 
            this.BTNEFontPick.Location = new System.Drawing.Point(8, 20);
            this.BTNEFontPick.Name = "BTNEFontPick";
            this.BTNEFontPick.Size = new System.Drawing.Size(90, 23);
            this.BTNEFontPick.TabIndex = 0;
            this.BTNEFontPick.Text = "Pick base font";
            this.BTNEFontPick.UseVisualStyleBackColor = true;
            this.BTNEFontPick.Click += new System.EventHandler(this.BTNEFontPick_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbEditorReadOnlyOut);
            this.groupBox4.Controls.Add(this.CBEOpenPreviousProjectAtLaunch);
            this.groupBox4.Controls.Add(this.cbEditorReplaceTabs);
            this.groupBox4.Controls.Add(this.CBEInsertEndBracket);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(322, 112);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options";
            // 
            // cbEditorReadOnlyOut
            // 
            this.cbEditorReadOnlyOut.AutoSize = true;
            this.cbEditorReadOnlyOut.Location = new System.Drawing.Point(6, 20);
            this.cbEditorReadOnlyOut.Name = "cbEditorReadOnlyOut";
            this.cbEditorReadOnlyOut.Size = new System.Drawing.Size(153, 17);
            this.cbEditorReadOnlyOut.TabIndex = 0;
            this.cbEditorReadOnlyOut.Text = "Make output files readonly";
            this.cbEditorReadOnlyOut.UseVisualStyleBackColor = true;
            this.cbEditorReadOnlyOut.CheckedChanged += new System.EventHandler(this.cbEditorReadOnlyOut_CheckedChanged);
            // 
            // CBEOpenPreviousProjectAtLaunch
            // 
            this.CBEOpenPreviousProjectAtLaunch.AutoSize = true;
            this.CBEOpenPreviousProjectAtLaunch.Location = new System.Drawing.Point(6, 89);
            this.CBEOpenPreviousProjectAtLaunch.Name = "CBEOpenPreviousProjectAtLaunch";
            this.CBEOpenPreviousProjectAtLaunch.Size = new System.Drawing.Size(180, 17);
            this.CBEOpenPreviousProjectAtLaunch.TabIndex = 3;
            this.CBEOpenPreviousProjectAtLaunch.Text = "Open previous project at launch";
            this.CBEOpenPreviousProjectAtLaunch.UseVisualStyleBackColor = true;
            this.CBEOpenPreviousProjectAtLaunch.CheckedChanged += new System.EventHandler(this.CBEOpenPreviousProjectAtLaunch_CheckedChanged);
            // 
            // cbEditorReplaceTabs
            // 
            this.cbEditorReplaceTabs.AutoSize = true;
            this.cbEditorReplaceTabs.Location = new System.Drawing.Point(6, 43);
            this.cbEditorReplaceTabs.Name = "cbEditorReplaceTabs";
            this.cbEditorReplaceTabs.Size = new System.Drawing.Size(147, 17);
            this.cbEditorReplaceTabs.TabIndex = 1;
            this.cbEditorReplaceTabs.Text = "Replace tabs with spaces";
            this.cbEditorReplaceTabs.UseVisualStyleBackColor = true;
            this.cbEditorReplaceTabs.CheckedChanged += new System.EventHandler(this.cbEditorReplaceTabs_CheckedChanged);
            // 
            // CBEInsertEndBracket
            // 
            this.CBEInsertEndBracket.AutoSize = true;
            this.CBEInsertEndBracket.Location = new System.Drawing.Point(6, 66);
            this.CBEInsertEndBracket.Name = "CBEInsertEndBracket";
            this.CBEInsertEndBracket.Size = new System.Drawing.Size(98, 17);
            this.CBEInsertEndBracket.TabIndex = 2;
            this.CBEInsertEndBracket.Text = "Insert } after {";
            this.CBEInsertEndBracket.UseVisualStyleBackColor = true;
            this.CBEInsertEndBracket.CheckedChanged += new System.EventHandler(this.CBEInsertEndBracket_CheckedChanged);
            // 
            // runOptionsTab
            // 
            this.runOptionsTab.CanClose = false;
            this.runOptionsTab.Controls.Add(this.TBROAdditionalArgs);
            this.runOptionsTab.Controls.Add(this.label8);
            this.runOptionsTab.Controls.Add(this.CBROAllowCheat);
            this.runOptionsTab.Controls.Add(this.CBROEnablePreload);
            this.runOptionsTab.Controls.Add(this.CBROWindowed);
            this.runOptionsTab.Controls.Add(this.CBROShowTriggerDebug);
            this.runOptionsTab.Controls.Add(this.TBROSeed);
            this.runOptionsTab.Controls.Add(this.LROSeed);
            this.runOptionsTab.Controls.Add(this.CBROFixedSeed);
            this.runOptionsTab.Controls.Add(this.CBROGameSpeed);
            this.runOptionsTab.Controls.Add(this.label6);
            this.runOptionsTab.Controls.Add(this.CBRODifficulty);
            this.runOptionsTab.Controls.Add(this.label5);
            this.runOptionsTab.IsDrawn = true;
            this.runOptionsTab.Name = "runOptionsTab";
            this.runOptionsTab.Selected = true;
            this.runOptionsTab.Size = new System.Drawing.Size(322, 386);
            this.runOptionsTab.TabIndex = 2;
            this.runOptionsTab.Title = "Run options";
            // 
            // TBROAdditionalArgs
            // 
            this.TBROAdditionalArgs.Location = new System.Drawing.Point(11, 151);
            this.TBROAdditionalArgs.Name = "TBROAdditionalArgs";
            this.TBROAdditionalArgs.Size = new System.Drawing.Size(296, 21);
            this.TBROAdditionalArgs.TabIndex = 12;
            this.TBROAdditionalArgs.TextChanged += new System.EventHandler(this.TBROAdditionalArgs_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 135);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(108, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Additional arguments";
            // 
            // CBROAllowCheat
            // 
            this.CBROAllowCheat.AutoSize = true;
            this.CBROAllowCheat.Location = new System.Drawing.Point(162, 115);
            this.CBROAllowCheat.Name = "CBROAllowCheat";
            this.CBROAllowCheat.Size = new System.Drawing.Size(86, 17);
            this.CBROAllowCheat.TabIndex = 10;
            this.CBROAllowCheat.Text = "Allow cheats";
            this.CBROAllowCheat.UseVisualStyleBackColor = true;
            this.CBROAllowCheat.CheckedChanged += new System.EventHandler(this.CBROAllowCheat_CheckedChanged);
            // 
            // CBROEnablePreload
            // 
            this.CBROEnablePreload.AutoSize = true;
            this.CBROEnablePreload.Location = new System.Drawing.Point(11, 115);
            this.CBROEnablePreload.Name = "CBROEnablePreload";
            this.CBROEnablePreload.Size = new System.Drawing.Size(111, 17);
            this.CBROEnablePreload.TabIndex = 9;
            this.CBROEnablePreload.Text = "Enable Preloading";
            this.CBROEnablePreload.UseVisualStyleBackColor = true;
            this.CBROEnablePreload.CheckedChanged += new System.EventHandler(this.CBROEnablePreload_CheckedChanged);
            // 
            // CBROWindowed
            // 
            this.CBROWindowed.AutoSize = true;
            this.CBROWindowed.Location = new System.Drawing.Point(11, 92);
            this.CBROWindowed.Name = "CBROWindowed";
            this.CBROWindowed.Size = new System.Drawing.Size(105, 17);
            this.CBROWindowed.TabIndex = 8;
            this.CBROWindowed.Text = "Windowed mode";
            this.CBROWindowed.UseVisualStyleBackColor = true;
            this.CBROWindowed.CheckedChanged += new System.EventHandler(this.CBROWindowed_CheckedChanged);
            // 
            // CBROShowTriggerDebug
            // 
            this.CBROShowTriggerDebug.AutoSize = true;
            this.CBROShowTriggerDebug.Location = new System.Drawing.Point(162, 92);
            this.CBROShowTriggerDebug.Name = "CBROShowTriggerDebug";
            this.CBROShowTriggerDebug.Size = new System.Drawing.Size(139, 17);
            this.CBROShowTriggerDebug.TabIndex = 7;
            this.CBROShowTriggerDebug.Text = "Show Trigger Debugger";
            this.CBROShowTriggerDebug.UseVisualStyleBackColor = true;
            this.CBROShowTriggerDebug.CheckedChanged += new System.EventHandler(this.CBROShowTriggerDebug_CheckedChanged);
            // 
            // TBROSeed
            // 
            this.TBROSeed.Location = new System.Drawing.Point(162, 65);
            this.TBROSeed.Name = "TBROSeed";
            this.TBROSeed.Size = new System.Drawing.Size(145, 21);
            this.TBROSeed.TabIndex = 6;
            this.TBROSeed.TextChanged += new System.EventHandler(this.TBROSeed_TextChanged);
            // 
            // LROSeed
            // 
            this.LROSeed.AutoSize = true;
            this.LROSeed.Location = new System.Drawing.Point(159, 49);
            this.LROSeed.Name = "LROSeed";
            this.LROSeed.Size = new System.Drawing.Size(31, 13);
            this.LROSeed.TabIndex = 5;
            this.LROSeed.Text = "Seed";
            // 
            // CBROFixedSeed
            // 
            this.CBROFixedSeed.AutoSize = true;
            this.CBROFixedSeed.Location = new System.Drawing.Point(11, 67);
            this.CBROFixedSeed.Name = "CBROFixedSeed";
            this.CBROFixedSeed.Size = new System.Drawing.Size(121, 17);
            this.CBROFixedSeed.TabIndex = 4;
            this.CBROFixedSeed.Text = "Fixed Random Seed";
            this.CBROFixedSeed.UseVisualStyleBackColor = true;
            this.CBROFixedSeed.CheckedChanged += new System.EventHandler(this.CBROFixedSeed_CheckedChanged);
            // 
            // CBROGameSpeed
            // 
            this.CBROGameSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBROGameSpeed.FormattingEnabled = true;
            this.CBROGameSpeed.Items.AddRange(new object[] {
            "Slower",
            "Slow",
            "Normal",
            "Fast",
            "Faster"});
            this.CBROGameSpeed.Location = new System.Drawing.Point(162, 25);
            this.CBROGameSpeed.Name = "CBROGameSpeed";
            this.CBROGameSpeed.Size = new System.Drawing.Size(145, 21);
            this.CBROGameSpeed.TabIndex = 3;
            this.CBROGameSpeed.SelectedIndexChanged += new System.EventHandler(this.CBROGameSpeed_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(159, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Game speed";
            // 
            // CBRODifficulty
            // 
            this.CBRODifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBRODifficulty.FormattingEnabled = true;
            this.CBRODifficulty.Items.AddRange(new object[] {
            "Very Easy",
            "Easy",
            "Medium",
            "Hard",
            "Very Hard",
            "Insane"});
            this.CBRODifficulty.Location = new System.Drawing.Point(11, 25);
            this.CBRODifficulty.Name = "CBRODifficulty";
            this.CBRODifficulty.Size = new System.Drawing.Size(145, 21);
            this.CBRODifficulty.TabIndex = 1;
            this.CBRODifficulty.SelectedIndexChanged += new System.EventHandler(this.CBRODifficulty_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Difficulty";
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 407);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabStrip)).EndInit();
            this.tabStrip.ResumeLayout(false);
            this.compilerTab.ResumeLayout(false);
            this.compilerTab.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.editorTab.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.runOptionsTab.ResumeLayout(false);
            this.runOptionsTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private FarsiLibrary.Win.FATabStrip tabStrip;
        private FarsiLibrary.Win.FATabStripItem compilerTab;
        private System.Windows.Forms.CheckBox CBCRemoveStructs;
        private System.Windows.Forms.CheckBox CBCRemoveFields;
        private System.Windows.Forms.CheckBox CBCRemoveMethods;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BTNOkay;
        private FarsiLibrary.Win.FATabStripItem editorTab;
        private System.Windows.Forms.CheckBox cbEditorReadOnlyOut;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox CBCShortNames;
        private System.Windows.Forms.CheckBox CBCOneFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox TBCMapBackups;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CBCRunCopy;
        private System.Windows.Forms.CheckBox CBCObfuscateStrings;
        private System.Windows.Forms.CheckBox cbEditorReplaceTabs;
        private System.Windows.Forms.CheckBox CBCNeverAskToOpenSavedFile;
        private System.Windows.Forms.CheckBox CBEInsertEndBracket;
        private System.Windows.Forms.CheckBox CBEOpenPreviousProjectAtLaunch;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox CBEPickFontContext;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox CBEItalicsFont;
        private System.Windows.Forms.CheckBox CBEBoldFond;
        private System.Windows.Forms.Button BTNEFontColor;
        private System.Windows.Forms.CheckBox CBEFontStrikeout;
        private System.Windows.Forms.CheckBox CBEFontUnderline;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TBECharWidth;
        private System.Windows.Forms.Button BTNEFontPick;
        private System.Windows.Forms.CheckBox CBCAutoInline;
        private System.Windows.Forms.GroupBox groupBox8;
        private FarsiLibrary.Win.FATabStripItem runOptionsTab;
        private System.Windows.Forms.TextBox TBROAdditionalArgs;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox CBROAllowCheat;
        private System.Windows.Forms.CheckBox CBROEnablePreload;
        private System.Windows.Forms.CheckBox CBROWindowed;
        private System.Windows.Forms.CheckBox CBROShowTriggerDebug;
        private System.Windows.Forms.TextBox TBROSeed;
        private System.Windows.Forms.Label LROSeed;
        private System.Windows.Forms.CheckBox CBROFixedSeed;
        private System.Windows.Forms.ComboBox CBROGameSpeed;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox CBRODifficulty;
        private System.Windows.Forms.Label label5;
    }
}