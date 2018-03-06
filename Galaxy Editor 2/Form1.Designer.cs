using FarsiLibrary.Win;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openExistingProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapObjectBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileAndCopyToMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileAndSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileAndSaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readTriggerFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.reparseStarCraft2FunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.libraryServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.searchDefinitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToAnotherVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.projectViewImageList = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.objectBrowserSplitContainer = new System.Windows.Forms.SplitContainer();
            this.ObjectBrowserPanel = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.mapObjectBrowserPage = new System.Windows.Forms.TabPage();
            this.ObjectBrowserList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.ObjectBrowserCatagory = new System.Windows.Forms.ToolStripComboBox();
            this.TBRefreshObjectList = new System.Windows.Forms.ToolStripButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.UnitFilterGrid = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.CBUnitFilterCompress = new System.Windows.Forms.CheckBox();
            this.BTNUnitFilterReset = new System.Windows.Forms.Button();
            this.BTNUnitFilterInsert = new System.Windows.Forms.Button();
            this.messageViewImageList = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.CBShowWarnings = new System.Windows.Forms.CheckBox();
            this.CBShowErrors = new System.Windows.Forms.CheckBox();
            this.projectViewMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newDialogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInExploreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectViewProjectMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.compilerStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.TBNewProject = new System.Windows.Forms.ToolStripButton();
            this.TBNewFile = new System.Windows.Forms.ToolStripButton();
            this.TBNewFolder = new System.Windows.Forms.ToolStripButton();
            this.TBDelete = new System.Windows.Forms.ToolStripButton();
            this.TBSave = new System.Windows.Forms.ToolStripButton();
            this.TBSaveAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.TBCut = new System.Windows.Forms.ToolStripButton();
            this.TBCopy = new System.Windows.Forms.ToolStripButton();
            this.TBPaste = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.TBUndo = new System.Windows.Forms.ToolStripButton();
            this.TBRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TBRun = new System.Windows.Forms.ToolStripSplitButton();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.TBFind = new System.Windows.Forms.ToolStripButton();
            this.editorRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RightClickCut = new System.Windows.Forms.ToolStripMenuItem();
            this.RightClickCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.RightClickPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.rightClickFind = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.TSInsertConstructor = new System.Windows.Forms.ToolStripMenuItem();
            this.goToDeclarationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ObjectBrowserTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.projectView = new Galaxy_Editor_2.TreeViewDragDrop();
            this.tabStrip = new FarsiLibrary.Win.FATabStrip();
            this.messageView = new Galaxy_Editor_2.TreeViewDragDrop();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectBrowserSplitContainer)).BeginInit();
            this.objectBrowserSplitContainer.Panel1.SuspendLayout();
            this.objectBrowserSplitContainer.Panel2.SuspendLayout();
            this.objectBrowserSplitContainer.SuspendLayout();
            this.ObjectBrowserPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.mapObjectBrowserPage.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.projectViewMenu.SuspendLayout();
            this.projectViewProjectMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.editorRightClick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabStrip)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.buildToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.libraryServerToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(1944, 43);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAllToolStripMenuItem,
            this.openExistingProjectToolStripMenuItem,
            this.saveProjectAsToolStripMenuItem,
            this.closeProjectToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(65, 35);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.sourceFileToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.newToolStripMenuItem.Text = "New";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(233, 38);
            this.projectToolStripMenuItem.Text = "Project";
            this.projectToolStripMenuItem.Click += new System.EventHandler(this.projectToolStripMenuItem_Click);
            // 
            // sourceFileToolStripMenuItem
            // 
            this.sourceFileToolStripMenuItem.Name = "sourceFileToolStripMenuItem";
            this.sourceFileToolStripMenuItem.Size = new System.Drawing.Size(233, 38);
            this.sourceFileToolStripMenuItem.Text = "Source file";
            this.sourceFileToolStripMenuItem.Click += new System.EventHandler(this.sourceFileToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAllToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.saveAllToolStripMenuItem.Text = "Save All";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // openExistingProjectToolStripMenuItem
            // 
            this.openExistingProjectToolStripMenuItem.Name = "openExistingProjectToolStripMenuItem";
            this.openExistingProjectToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.openExistingProjectToolStripMenuItem.Text = "Open Existing Project";
            this.openExistingProjectToolStripMenuItem.Click += new System.EventHandler(this.openExistingProjectToolStripMenuItem_Click);
            // 
            // saveProjectAsToolStripMenuItem
            // 
            this.saveProjectAsToolStripMenuItem.Enabled = false;
            this.saveProjectAsToolStripMenuItem.Name = "saveProjectAsToolStripMenuItem";
            this.saveProjectAsToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.saveProjectAsToolStripMenuItem.Text = "Save project as";
            this.saveProjectAsToolStripMenuItem.Click += new System.EventHandler(this.saveProjectAsToolStripMenuItem_Click);
            // 
            // closeProjectToolStripMenuItem
            // 
            this.closeProjectToolStripMenuItem.Enabled = false;
            this.closeProjectToolStripMenuItem.Name = "closeProjectToolStripMenuItem";
            this.closeProjectToolStripMenuItem.Size = new System.Drawing.Size(359, 38);
            this.closeProjectToolStripMenuItem.Text = "Close project";
            this.closeProjectToolStripMenuItem.Click += new System.EventHandler(this.closeProjectToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mapObjectBrowserToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(81, 35);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // mapObjectBrowserToolStripMenuItem
            // 
            this.mapObjectBrowserToolStripMenuItem.Checked = true;
            this.mapObjectBrowserToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mapObjectBrowserToolStripMenuItem.Name = "mapObjectBrowserToolStripMenuItem";
            this.mapObjectBrowserToolStripMenuItem.Size = new System.Drawing.Size(247, 38);
            this.mapObjectBrowserToolStripMenuItem.Text = "Right menu";
            this.mapObjectBrowserToolStripMenuItem.Click += new System.EventHandler(this.mapObjectBrowserToolStripMenuItem_Click);
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compileAndCopyToMapToolStripMenuItem,
            this.compileAndSaveToolStripMenuItem,
            this.compileAndSaveAsToolStripMenuItem,
            this.compileToolStripMenuItem,
            this.readTriggerFilesToolStripMenuItem});
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            this.buildToolStripMenuItem.Size = new System.Drawing.Size(83, 35);
            this.buildToolStripMenuItem.Text = "Build";
            // 
            // compileAndCopyToMapToolStripMenuItem
            // 
            this.compileAndCopyToMapToolStripMenuItem.Enabled = false;
            this.compileAndCopyToMapToolStripMenuItem.Name = "compileAndCopyToMapToolStripMenuItem";
            this.compileAndCopyToMapToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F9)));
            this.compileAndCopyToMapToolStripMenuItem.Size = new System.Drawing.Size(426, 38);
            this.compileAndCopyToMapToolStripMenuItem.Text = "Compile and run";
            this.compileAndCopyToMapToolStripMenuItem.Click += new System.EventHandler(this.compileAndCopyToMapToolStripMenuItem_Click);
            // 
            // compileAndSaveToolStripMenuItem
            // 
            this.compileAndSaveToolStripMenuItem.Enabled = false;
            this.compileAndSaveToolStripMenuItem.Name = "compileAndSaveToolStripMenuItem";
            this.compileAndSaveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F9)));
            this.compileAndSaveToolStripMenuItem.Size = new System.Drawing.Size(426, 38);
            this.compileAndSaveToolStripMenuItem.Text = "Compile and save";
            this.compileAndSaveToolStripMenuItem.Click += new System.EventHandler(this.compileAndSaveToolStripMenuItem_Click);
            // 
            // compileAndSaveAsToolStripMenuItem
            // 
            this.compileAndSaveAsToolStripMenuItem.Enabled = false;
            this.compileAndSaveAsToolStripMenuItem.Name = "compileAndSaveAsToolStripMenuItem";
            this.compileAndSaveAsToolStripMenuItem.Size = new System.Drawing.Size(426, 38);
            this.compileAndSaveAsToolStripMenuItem.Text = "Compile and save as";
            this.compileAndSaveAsToolStripMenuItem.Click += new System.EventHandler(this.compileAndSaveAsToolStripMenuItem_Click);
            // 
            // compileToolStripMenuItem
            // 
            this.compileToolStripMenuItem.Enabled = false;
            this.compileToolStripMenuItem.Name = "compileToolStripMenuItem";
            this.compileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.compileToolStripMenuItem.Size = new System.Drawing.Size(426, 38);
            this.compileToolStripMenuItem.Text = "Compile";
            this.compileToolStripMenuItem.Click += new System.EventHandler(this.compileToolStripMenuItem_Click);
            // 
            // readTriggerFilesToolStripMenuItem
            // 
            this.readTriggerFilesToolStripMenuItem.Name = "readTriggerFilesToolStripMenuItem";
            this.readTriggerFilesToolStripMenuItem.Size = new System.Drawing.Size(426, 38);
            this.readTriggerFilesToolStripMenuItem.Text = "Read Trigger Files";
            this.readTriggerFilesToolStripMenuItem.Click += new System.EventHandler(this.readTriggerFilesToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectSettingsToolStripMenuItem,
            this.optionsToolStripMenuItem1,
            this.reparseStarCraft2FunctionsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(119, 35);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // projectSettingsToolStripMenuItem
            // 
            this.projectSettingsToolStripMenuItem.Enabled = false;
            this.projectSettingsToolStripMenuItem.Name = "projectSettingsToolStripMenuItem";
            this.projectSettingsToolStripMenuItem.Size = new System.Drawing.Size(318, 38);
            this.projectSettingsToolStripMenuItem.Text = "Project settings";
            this.projectSettingsToolStripMenuItem.Click += new System.EventHandler(this.projectSettingsToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem1
            // 
            this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
            this.optionsToolStripMenuItem1.Size = new System.Drawing.Size(318, 38);
            this.optionsToolStripMenuItem1.Text = "Options";
            this.optionsToolStripMenuItem1.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // reparseStarCraft2FunctionsToolStripMenuItem
            // 
            this.reparseStarCraft2FunctionsToolStripMenuItem.Name = "reparseStarCraft2FunctionsToolStripMenuItem";
            this.reparseStarCraft2FunctionsToolStripMenuItem.Size = new System.Drawing.Size(318, 38);
            this.reparseStarCraft2FunctionsToolStripMenuItem.Text = "Reparse functions";
            this.reparseStarCraft2FunctionsToolStripMenuItem.Click += new System.EventHandler(this.reparseStarCraftFunctions);
            // 
            // libraryServerToolStripMenuItem
            // 
            this.libraryServerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createAccountToolStripMenuItem,
            this.manageAccountToolStripMenuItem,
            this.uploadLibraryToolStripMenuItem,
            this.downloadLibraryToolStripMenuItem});
            this.libraryServerToolStripMenuItem.Name = "libraryServerToolStripMenuItem";
            this.libraryServerToolStripMenuItem.Size = new System.Drawing.Size(183, 35);
            this.libraryServerToolStripMenuItem.Text = "Library Server";
            // 
            // createAccountToolStripMenuItem
            // 
            this.createAccountToolStripMenuItem.Name = "createAccountToolStripMenuItem";
            this.createAccountToolStripMenuItem.Size = new System.Drawing.Size(313, 38);
            this.createAccountToolStripMenuItem.Text = "Create Account";
            this.createAccountToolStripMenuItem.Click += new System.EventHandler(this.createAccountToolStripMenuItem_Click);
            // 
            // manageAccountToolStripMenuItem
            // 
            this.manageAccountToolStripMenuItem.Name = "manageAccountToolStripMenuItem";
            this.manageAccountToolStripMenuItem.Size = new System.Drawing.Size(313, 38);
            this.manageAccountToolStripMenuItem.Text = "Manage Account";
            this.manageAccountToolStripMenuItem.Click += new System.EventHandler(this.manageAccountToolStripMenuItem_Click);
            // 
            // uploadLibraryToolStripMenuItem
            // 
            this.uploadLibraryToolStripMenuItem.Name = "uploadLibraryToolStripMenuItem";
            this.uploadLibraryToolStripMenuItem.Size = new System.Drawing.Size(313, 38);
            this.uploadLibraryToolStripMenuItem.Text = "Upload Library";
            this.uploadLibraryToolStripMenuItem.Click += new System.EventHandler(this.uploadLibraryToolStripMenuItem_Click);
            // 
            // downloadLibraryToolStripMenuItem
            // 
            this.downloadLibraryToolStripMenuItem.Name = "downloadLibraryToolStripMenuItem";
            this.downloadLibraryToolStripMenuItem.Size = new System.Drawing.Size(313, 38);
            this.downloadLibraryToolStripMenuItem.Text = "Download Library";
            this.downloadLibraryToolStripMenuItem.Click += new System.EventHandler(this.downloadLibraryToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1,
            this.searchDefinitionsToolStripMenuItem,
            this.changeLogToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.changeToAnotherVersionToolStripMenuItem,
            this.reportErrorToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(80, 35);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(419, 38);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // searchDefinitionsToolStripMenuItem
            // 
            this.searchDefinitionsToolStripMenuItem.Name = "searchDefinitionsToolStripMenuItem";
            this.searchDefinitionsToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.searchDefinitionsToolStripMenuItem.Text = "Search definitions";
            this.searchDefinitionsToolStripMenuItem.Click += new System.EventHandler(this.searchDefinitionsToolStripMenuItem_Click);
            // 
            // changeLogToolStripMenuItem
            // 
            this.changeLogToolStripMenuItem.Name = "changeLogToolStripMenuItem";
            this.changeLogToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.changeLogToolStripMenuItem.Text = "Change Log";
            this.changeLogToolStripMenuItem.Click += new System.EventHandler(this.changeLogToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // changeToAnotherVersionToolStripMenuItem
            // 
            this.changeToAnotherVersionToolStripMenuItem.Name = "changeToAnotherVersionToolStripMenuItem";
            this.changeToAnotherVersionToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.changeToAnotherVersionToolStripMenuItem.Text = "Change to another version";
            this.changeToAnotherVersionToolStripMenuItem.Click += new System.EventHandler(this.changeToAnotherVersionToolStripMenuItem_Click);
            // 
            // reportErrorToolStripMenuItem
            // 
            this.reportErrorToolStripMenuItem.Name = "reportErrorToolStripMenuItem";
            this.reportErrorToolStripMenuItem.Size = new System.Drawing.Size(419, 38);
            this.reportErrorToolStripMenuItem.Text = "Report Error";
            this.reportErrorToolStripMenuItem.Click += new System.EventHandler(this.reportErrorToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 82);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.projectView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1944, 842);
            this.splitContainer1.SplitterDistance = 198;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 1;
            // 
            // projectViewImageList
            // 
            this.projectViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("projectViewImageList.ImageStream")));
            this.projectViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.projectViewImageList.Images.SetKeyName(0, "icon.ico");
            this.projectViewImageList.Images.SetKeyName(1, "Folder.png");
            this.projectViewImageList.Images.SetKeyName(2, "GalaxyFileV3-16.png");
            this.projectViewImageList.Images.SetKeyName(3, "XMLIcon.ico");
            this.projectViewImageList.Images.SetKeyName(4, "");
            this.projectViewImageList.Images.SetKeyName(5, "DialogIcon.png");
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.objectBrowserSplitContainer);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.messageView);
            this.splitContainer2.Panel2.Controls.Add(this.panel1);
            this.splitContainer2.Size = new System.Drawing.Size(1738, 842);
            this.splitContainer2.SplitterDistance = 712;
            this.splitContainer2.SplitterWidth = 8;
            this.splitContainer2.TabIndex = 0;
            // 
            // objectBrowserSplitContainer
            // 
            this.objectBrowserSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectBrowserSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.objectBrowserSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.objectBrowserSplitContainer.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.objectBrowserSplitContainer.Name = "objectBrowserSplitContainer";
            // 
            // objectBrowserSplitContainer.Panel1
            // 
            this.objectBrowserSplitContainer.Panel1.Controls.Add(this.tabStrip);
            // 
            // objectBrowserSplitContainer.Panel2
            // 
            this.objectBrowserSplitContainer.Panel2.Controls.Add(this.ObjectBrowserPanel);
            this.objectBrowserSplitContainer.Size = new System.Drawing.Size(1738, 712);
            this.objectBrowserSplitContainer.SplitterDistance = 1494;
            this.objectBrowserSplitContainer.SplitterWidth = 8;
            this.objectBrowserSplitContainer.TabIndex = 1;
            // 
            // ObjectBrowserPanel
            // 
            this.ObjectBrowserPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ObjectBrowserPanel.Controls.Add(this.tabControl1);
            this.ObjectBrowserPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ObjectBrowserPanel.Location = new System.Drawing.Point(0, 0);
            this.ObjectBrowserPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ObjectBrowserPanel.Name = "ObjectBrowserPanel";
            this.ObjectBrowserPanel.Size = new System.Drawing.Size(236, 712);
            this.ObjectBrowserPanel.TabIndex = 3;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.mapObjectBrowserPage);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(234, 710);
            this.tabControl1.TabIndex = 4;
            // 
            // mapObjectBrowserPage
            // 
            this.mapObjectBrowserPage.BackColor = System.Drawing.SystemColors.Control;
            this.mapObjectBrowserPage.Controls.Add(this.ObjectBrowserList);
            this.mapObjectBrowserPage.Controls.Add(this.toolStrip2);
            this.mapObjectBrowserPage.Location = new System.Drawing.Point(8, 39);
            this.mapObjectBrowserPage.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.mapObjectBrowserPage.Name = "mapObjectBrowserPage";
            this.mapObjectBrowserPage.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.mapObjectBrowserPage.Size = new System.Drawing.Size(218, 663);
            this.mapObjectBrowserPage.TabIndex = 0;
            this.mapObjectBrowserPage.Text = "Map Object Browser";
            // 
            // ObjectBrowserList
            // 
            this.ObjectBrowserList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.ObjectBrowserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ObjectBrowserList.Location = new System.Drawing.Point(6, 49);
            this.ObjectBrowserList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ObjectBrowserList.MultiSelect = false;
            this.ObjectBrowserList.Name = "ObjectBrowserList";
            this.ObjectBrowserList.Size = new System.Drawing.Size(206, 608);
            this.ObjectBrowserList.TabIndex = 3;
            this.ObjectBrowserList.UseCompatibleStateImageBehavior = false;
            this.ObjectBrowserList.View = System.Windows.Forms.View.Details;
            this.ObjectBrowserList.SelectedIndexChanged += new System.EventHandler(this.ObjectBrowserList_SelectedIndexChanged);
            this.ObjectBrowserList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectBrowserList_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "ID";
            // 
            // toolStrip2
            // 
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ObjectBrowserCatagory,
            this.TBRefreshObjectList});
            this.toolStrip2.Location = new System.Drawing.Point(6, 6);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip2.Size = new System.Drawing.Size(206, 43);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // ObjectBrowserCatagory
            // 
            this.ObjectBrowserCatagory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ObjectBrowserCatagory.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.ObjectBrowserCatagory.Name = "ObjectBrowserCatagory";
            this.ObjectBrowserCatagory.Size = new System.Drawing.Size(238, 39);
            this.ObjectBrowserCatagory.SelectedIndexChanged += new System.EventHandler(this.ObjectBrowserCatagory_SelectedIndexChanged);
            // 
            // TBRefreshObjectList
            // 
            this.TBRefreshObjectList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBRefreshObjectList.Image = ((System.Drawing.Image)(resources.GetObject("TBRefreshObjectList.Image")));
            this.TBRefreshObjectList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBRefreshObjectList.Name = "TBRefreshObjectList";
            this.TBRefreshObjectList.Size = new System.Drawing.Size(36, 36);
            this.TBRefreshObjectList.Text = "toolStripButton1";
            this.TBRefreshObjectList.ToolTipText = "Refresh list";
            this.TBRefreshObjectList.Click += new System.EventHandler(this.TBRefreshObjectList_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.UnitFilterGrid);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabPage2.Size = new System.Drawing.Size(218, 663);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Unit Filter Generator";
            // 
            // UnitFilterGrid
            // 
            this.UnitFilterGrid.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.UnitFilterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UnitFilterGrid.HelpVisible = false;
            this.UnitFilterGrid.Location = new System.Drawing.Point(6, 6);
            this.UnitFilterGrid.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.UnitFilterGrid.Name = "UnitFilterGrid";
            this.UnitFilterGrid.Size = new System.Drawing.Size(206, 545);
            this.UnitFilterGrid.TabIndex = 1;
            this.UnitFilterGrid.ToolbarVisible = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.CBUnitFilterCompress);
            this.panel2.Controls.Add(this.BTNUnitFilterReset);
            this.panel2.Controls.Add(this.BTNUnitFilterInsert);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(6, 551);
            this.panel2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(206, 106);
            this.panel2.TabIndex = 0;
            // 
            // CBUnitFilterCompress
            // 
            this.CBUnitFilterCompress.AutoSize = true;
            this.CBUnitFilterCompress.Location = new System.Drawing.Point(6, 60);
            this.CBUnitFilterCompress.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CBUnitFilterCompress.Name = "CBUnitFilterCompress";
            this.CBUnitFilterCompress.Size = new System.Drawing.Size(246, 28);
            this.CBUnitFilterCompress.TabIndex = 2;
            this.CBUnitFilterCompress.Text = "Insert Compressed";
            this.CBUnitFilterCompress.UseVisualStyleBackColor = true;
            // 
            // BTNUnitFilterReset
            // 
            this.BTNUnitFilterReset.Location = new System.Drawing.Point(168, 6);
            this.BTNUnitFilterReset.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.BTNUnitFilterReset.Name = "BTNUnitFilterReset";
            this.BTNUnitFilterReset.Size = new System.Drawing.Size(150, 42);
            this.BTNUnitFilterReset.TabIndex = 1;
            this.BTNUnitFilterReset.Text = "Reset";
            this.BTNUnitFilterReset.UseVisualStyleBackColor = true;
            this.BTNUnitFilterReset.Click += new System.EventHandler(this.BTNUnitFilterReset_Click);
            // 
            // BTNUnitFilterInsert
            // 
            this.BTNUnitFilterInsert.Location = new System.Drawing.Point(6, 6);
            this.BTNUnitFilterInsert.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.BTNUnitFilterInsert.Name = "BTNUnitFilterInsert";
            this.BTNUnitFilterInsert.Size = new System.Drawing.Size(150, 42);
            this.BTNUnitFilterInsert.TabIndex = 0;
            this.BTNUnitFilterInsert.Text = "Insert";
            this.BTNUnitFilterInsert.UseVisualStyleBackColor = true;
            this.BTNUnitFilterInsert.Click += new System.EventHandler(this.BTNUnitFilterInsert_Click);
            // 
            // messageViewImageList
            // 
            this.messageViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("messageViewImageList.ImageStream")));
            this.messageViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.messageViewImageList.Images.SetKeyName(0, "Error.bmp");
            this.messageViewImageList.Images.SetKeyName(1, "Warning.bmp");
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CBShowWarnings);
            this.panel1.Controls.Add(this.CBShowErrors);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1738, 52);
            this.panel1.TabIndex = 1;
            // 
            // CBShowWarnings
            // 
            this.CBShowWarnings.AutoSize = true;
            this.CBShowWarnings.Location = new System.Drawing.Point(184, 6);
            this.CBShowWarnings.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CBShowWarnings.Name = "CBShowWarnings";
            this.CBShowWarnings.Size = new System.Drawing.Size(198, 28);
            this.CBShowWarnings.TabIndex = 3;
            this.CBShowWarnings.Text = "Show Warnings";
            this.CBShowWarnings.UseVisualStyleBackColor = true;
            this.CBShowWarnings.CheckedChanged += new System.EventHandler(this.CBShowWarnings_CheckedChanged);
            // 
            // CBShowErrors
            // 
            this.CBShowErrors.AutoSize = true;
            this.CBShowErrors.Location = new System.Drawing.Point(6, 6);
            this.CBShowErrors.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CBShowErrors.Name = "CBShowErrors";
            this.CBShowErrors.Size = new System.Drawing.Size(174, 28);
            this.CBShowErrors.TabIndex = 2;
            this.CBShowErrors.Text = "Show Errors";
            this.CBShowErrors.UseVisualStyleBackColor = true;
            this.CBShowErrors.CheckedChanged += new System.EventHandler(this.CBShowErrors_CheckedChanged);
            // 
            // projectViewMenu
            // 
            this.projectViewMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.projectViewMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFileToolStripMenuItem,
            this.newDialogToolStripMenuItem,
            this.newFolderToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.openInExploreToolStripMenuItem,
            this.activateToolStripMenuItem});
            this.projectViewMenu.Name = "projectViewMenu";
            this.projectViewMenu.Size = new System.Drawing.Size(307, 270);
            // 
            // newFileToolStripMenuItem
            // 
            this.newFileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newFileToolStripMenuItem.Image")));
            this.newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            this.newFileToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.newFileToolStripMenuItem.Text = "New File";
            this.newFileToolStripMenuItem.Click += new System.EventHandler(this.newFileToolStripMenuItem_Click);
            // 
            // newDialogToolStripMenuItem
            // 
            this.newDialogToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newDialogToolStripMenuItem.Image")));
            this.newDialogToolStripMenuItem.Name = "newDialogToolStripMenuItem";
            this.newDialogToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.newDialogToolStripMenuItem.Text = "New Dialog";
            this.newDialogToolStripMenuItem.Click += new System.EventHandler(this.newDialogToolStripMenuItem_Click);
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newFolderToolStripMenuItem.Image")));
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.newFolderToolStripMenuItem.Text = "New Folder";
            this.newFolderToolStripMenuItem.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("removeToolStripMenuItem.Image")));
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.ShortcutKeyDisplayString = "Del";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("renameToolStripMenuItem.Image")));
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.ShortcutKeyDisplayString = "F2";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // openInExploreToolStripMenuItem
            // 
            this.openInExploreToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openInExploreToolStripMenuItem.Image")));
            this.openInExploreToolStripMenuItem.Name = "openInExploreToolStripMenuItem";
            this.openInExploreToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.openInExploreToolStripMenuItem.Text = "Open in explorer";
            this.openInExploreToolStripMenuItem.Click += new System.EventHandler(this.openInExploreToolStripMenuItem_Click);
            // 
            // activateToolStripMenuItem
            // 
            this.activateToolStripMenuItem.Checked = true;
            this.activateToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.activateToolStripMenuItem.Name = "activateToolStripMenuItem";
            this.activateToolStripMenuItem.Size = new System.Drawing.Size(306, 38);
            this.activateToolStripMenuItem.Text = "Enabled";
            this.activateToolStripMenuItem.Click += new System.EventHandler(this.activateToolStripMenuItem_Click);
            // 
            // projectViewProjectMenu
            // 
            this.projectViewProjectMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.projectViewProjectMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectToolStripMenuItem,
            this.deleteProjectToolStripMenuItem});
            this.projectViewProjectMenu.Name = "projectViewProjectMenu";
            this.projectViewProjectMenu.Size = new System.Drawing.Size(328, 80);
            // 
            // newProjectToolStripMenuItem
            // 
            this.newProjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newProjectToolStripMenuItem.Image")));
            this.newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
            this.newProjectToolStripMenuItem.Size = new System.Drawing.Size(327, 38);
            this.newProjectToolStripMenuItem.Text = "New project";
            this.newProjectToolStripMenuItem.Click += new System.EventHandler(this.newProjectToolStripMenuItem_Click);
            // 
            // deleteProjectToolStripMenuItem
            // 
            this.deleteProjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteProjectToolStripMenuItem.Image")));
            this.deleteProjectToolStripMenuItem.Name = "deleteProjectToolStripMenuItem";
            this.deleteProjectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteProjectToolStripMenuItem.Size = new System.Drawing.Size(327, 38);
            this.deleteProjectToolStripMenuItem.Text = "Delete project";
            this.deleteProjectToolStripMenuItem.Click += new System.EventHandler(this.deleteProjectToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compilerStatusText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 924);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1944, 36);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // compilerStatusText
            // 
            this.compilerStatusText.Name = "compilerStatusText";
            this.compilerStatusText.Size = new System.Drawing.Size(85, 31);
            this.compilerStatusText.Text = "Ready";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TBNewProject,
            this.TBNewFile,
            this.TBNewFolder,
            this.TBDelete,
            this.TBSave,
            this.TBSaveAll,
            this.toolStripSeparator3,
            this.TBCut,
            this.TBCopy,
            this.TBPaste,
            this.toolStripSeparator2,
            this.TBUndo,
            this.TBRedo,
            this.toolStripSeparator1,
            this.TBRun,
            this.toolStripSeparator4,
            this.TBFind});
            this.toolStrip1.Location = new System.Drawing.Point(0, 43);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1944, 39);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // TBNewProject
            // 
            this.TBNewProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBNewProject.Image = ((System.Drawing.Image)(resources.GetObject("TBNewProject.Image")));
            this.TBNewProject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBNewProject.Name = "TBNewProject";
            this.TBNewProject.Size = new System.Drawing.Size(36, 36);
            this.TBNewProject.Text = "toolStripButton1";
            this.TBNewProject.ToolTipText = "New Project";
            this.TBNewProject.Click += new System.EventHandler(this.projectToolStripMenuItem_Click);
            // 
            // TBNewFile
            // 
            this.TBNewFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBNewFile.Enabled = false;
            this.TBNewFile.Image = ((System.Drawing.Image)(resources.GetObject("TBNewFile.Image")));
            this.TBNewFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBNewFile.Name = "TBNewFile";
            this.TBNewFile.Size = new System.Drawing.Size(36, 36);
            this.TBNewFile.Text = "toolStripButton3";
            this.TBNewFile.ToolTipText = "New File";
            this.TBNewFile.Click += new System.EventHandler(this.sourceFileToolStripMenuItem_Click);
            // 
            // TBNewFolder
            // 
            this.TBNewFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBNewFolder.Enabled = false;
            this.TBNewFolder.Image = ((System.Drawing.Image)(resources.GetObject("TBNewFolder.Image")));
            this.TBNewFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBNewFolder.Name = "TBNewFolder";
            this.TBNewFolder.Size = new System.Drawing.Size(36, 36);
            this.TBNewFolder.Text = "toolStripButton6";
            this.TBNewFolder.ToolTipText = "New Folder";
            this.TBNewFolder.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
            // 
            // TBDelete
            // 
            this.TBDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBDelete.Image = ((System.Drawing.Image)(resources.GetObject("TBDelete.Image")));
            this.TBDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBDelete.Name = "TBDelete";
            this.TBDelete.Size = new System.Drawing.Size(36, 36);
            this.TBDelete.Text = "Delete (Del)";
            this.TBDelete.Click += new System.EventHandler(this.TBDelete_Click);
            // 
            // TBSave
            // 
            this.TBSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBSave.Enabled = false;
            this.TBSave.Image = ((System.Drawing.Image)(resources.GetObject("TBSave.Image")));
            this.TBSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBSave.Name = "TBSave";
            this.TBSave.Size = new System.Drawing.Size(36, 36);
            this.TBSave.Text = "toolStripButton2";
            this.TBSave.ToolTipText = "Save (Ctrl+S)";
            this.TBSave.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // TBSaveAll
            // 
            this.TBSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBSaveAll.Enabled = false;
            this.TBSaveAll.Image = ((System.Drawing.Image)(resources.GetObject("TBSaveAll.Image")));
            this.TBSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBSaveAll.Name = "TBSaveAll";
            this.TBSaveAll.Size = new System.Drawing.Size(36, 36);
            this.TBSaveAll.Text = "toolStripButton1";
            this.TBSaveAll.ToolTipText = "Save All (Ctrl+Shift+S)";
            this.TBSaveAll.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // TBCut
            // 
            this.TBCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBCut.Enabled = false;
            this.TBCut.Image = ((System.Drawing.Image)(resources.GetObject("TBCut.Image")));
            this.TBCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBCut.Name = "TBCut";
            this.TBCut.Size = new System.Drawing.Size(36, 36);
            this.TBCut.Text = "toolStripButton1";
            this.TBCut.ToolTipText = "Cut (Ctrl+X)";
            this.TBCut.Click += new System.EventHandler(this.TBCut_Click);
            // 
            // TBCopy
            // 
            this.TBCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBCopy.Enabled = false;
            this.TBCopy.Image = ((System.Drawing.Image)(resources.GetObject("TBCopy.Image")));
            this.TBCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBCopy.Name = "TBCopy";
            this.TBCopy.Size = new System.Drawing.Size(36, 36);
            this.TBCopy.Text = "toolStripButton1";
            this.TBCopy.ToolTipText = "Copy (Ctrl+C)";
            this.TBCopy.Click += new System.EventHandler(this.TBCopy_Click);
            // 
            // TBPaste
            // 
            this.TBPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBPaste.Enabled = false;
            this.TBPaste.Image = ((System.Drawing.Image)(resources.GetObject("TBPaste.Image")));
            this.TBPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBPaste.Name = "TBPaste";
            this.TBPaste.Size = new System.Drawing.Size(36, 36);
            this.TBPaste.Text = "toolStripButton2";
            this.TBPaste.ToolTipText = "Paste (Ctrl+V)";
            this.TBPaste.Click += new System.EventHandler(this.TBPaste_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // TBUndo
            // 
            this.TBUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBUndo.Enabled = false;
            this.TBUndo.Image = ((System.Drawing.Image)(resources.GetObject("TBUndo.Image")));
            this.TBUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBUndo.Name = "TBUndo";
            this.TBUndo.Size = new System.Drawing.Size(36, 36);
            this.TBUndo.Text = "toolStripButton5";
            this.TBUndo.ToolTipText = "Undo (Ctrl+Z)";
            this.TBUndo.Click += new System.EventHandler(this.TBUndo_Click);
            // 
            // TBRedo
            // 
            this.TBRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBRedo.Enabled = false;
            this.TBRedo.Image = ((System.Drawing.Image)(resources.GetObject("TBRedo.Image")));
            this.TBRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBRedo.Name = "TBRedo";
            this.TBRedo.Size = new System.Drawing.Size(36, 36);
            this.TBRedo.Text = "toolStripButton4";
            this.TBRedo.ToolTipText = "Redo (Ctrl+Y)";
            this.TBRedo.Click += new System.EventHandler(this.TBRedo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // TBRun
            // 
            this.TBRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runToolStripMenuItem,
            this.buildOnlyToolStripMenuItem,
            this.saveToolStripMenuItem1,
            this.saveAsToolStripMenuItem});
            this.TBRun.Enabled = false;
            this.TBRun.Image = ((System.Drawing.Image)(resources.GetObject("TBRun.Image")));
            this.TBRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBRun.Name = "TBRun";
            this.TBRun.Size = new System.Drawing.Size(59, 36);
            this.TBRun.Text = "toolStripSplitButton1";
            this.TBRun.ToolTipText = "Run the map in Starcraft II";
            this.TBRun.ButtonClick += new System.EventHandler(this.compileAndCopyToMapToolStripMenuItem_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F9)));
            this.runToolStripMenuItem.Size = new System.Drawing.Size(278, 38);
            this.runToolStripMenuItem.Text = "Run";
            this.runToolStripMenuItem.ToolTipText = "Run the map in Starcraft II";
            this.runToolStripMenuItem.Click += new System.EventHandler(this.compileAndCopyToMapToolStripMenuItem_Click);
            // 
            // buildOnlyToolStripMenuItem
            // 
            this.buildOnlyToolStripMenuItem.Name = "buildOnlyToolStripMenuItem";
            this.buildOnlyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.buildOnlyToolStripMenuItem.Size = new System.Drawing.Size(278, 38);
            this.buildOnlyToolStripMenuItem.Text = "Build only";
            this.buildOnlyToolStripMenuItem.ToolTipText = "Only Generate Galaxy Script";
            this.buildOnlyToolStripMenuItem.Click += new System.EventHandler(this.compileToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F9)));
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(278, 38);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.ToolTipText = "Save the script to your map (for publishing)";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.compileAndSaveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(278, 38);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.ToolTipText = "Save the script to your map (for publishing)";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.compileAndSaveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 39);
            // 
            // TBFind
            // 
            this.TBFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TBFind.Enabled = false;
            this.TBFind.Image = ((System.Drawing.Image)(resources.GetObject("TBFind.Image")));
            this.TBFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TBFind.Name = "TBFind";
            this.TBFind.Size = new System.Drawing.Size(36, 36);
            this.TBFind.Text = "toolStripButton1";
            this.TBFind.ToolTipText = "Find (Ctrl+F)";
            this.TBFind.Click += new System.EventHandler(this.TBFind_Click);
            // 
            // editorRightClick
            // 
            this.editorRightClick.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.editorRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RightClickCut,
            this.RightClickCopy,
            this.RightClickPaste,
            this.toolStripSeparator5,
            this.rightClickFind,
            this.toolStripSeparator6,
            this.toolStripMenuItem1,
            this.goToDeclarationToolStripMenuItem});
            this.editorRightClick.Name = "editorRightClick";
            this.editorRightClick.Size = new System.Drawing.Size(348, 244);
            // 
            // RightClickCut
            // 
            this.RightClickCut.Image = global::Galaxy_Editor_2.Properties.Resources.CutHS;
            this.RightClickCut.Name = "RightClickCut";
            this.RightClickCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.RightClickCut.Size = new System.Drawing.Size(347, 38);
            this.RightClickCut.Text = "Cut";
            this.RightClickCut.Click += new System.EventHandler(this.TBCut_Click);
            // 
            // RightClickCopy
            // 
            this.RightClickCopy.Image = global::Galaxy_Editor_2.Properties.Resources.CopyHS;
            this.RightClickCopy.Name = "RightClickCopy";
            this.RightClickCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.RightClickCopy.Size = new System.Drawing.Size(347, 38);
            this.RightClickCopy.Text = "Copy";
            this.RightClickCopy.Click += new System.EventHandler(this.TBCopy_Click);
            // 
            // RightClickPaste
            // 
            this.RightClickPaste.Image = global::Galaxy_Editor_2.Properties.Resources.PasteHS;
            this.RightClickPaste.Name = "RightClickPaste";
            this.RightClickPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.RightClickPaste.Size = new System.Drawing.Size(347, 38);
            this.RightClickPaste.Text = "Paste";
            this.RightClickPaste.Click += new System.EventHandler(this.TBPaste_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(344, 6);
            // 
            // rightClickFind
            // 
            this.rightClickFind.Image = ((System.Drawing.Image)(resources.GetObject("rightClickFind.Image")));
            this.rightClickFind.Name = "rightClickFind";
            this.rightClickFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.rightClickFind.Size = new System.Drawing.Size(347, 38);
            this.rightClickFind.Text = "Find/Replace";
            this.rightClickFind.Click += new System.EventHandler(this.TBFind_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(344, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSInsertConstructor});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(347, 38);
            this.toolStripMenuItem1.Text = "Insert";
            // 
            // TSInsertConstructor
            // 
            this.TSInsertConstructor.Name = "TSInsertConstructor";
            this.TSInsertConstructor.Size = new System.Drawing.Size(248, 38);
            this.TSInsertConstructor.Text = "Constructor";
            this.TSInsertConstructor.Click += new System.EventHandler(this.TSInsertConstructor_Click);
            // 
            // goToDeclarationToolStripMenuItem
            // 
            this.goToDeclarationToolStripMenuItem.Name = "goToDeclarationToolStripMenuItem";
            this.goToDeclarationToolStripMenuItem.Size = new System.Drawing.Size(347, 38);
            this.goToDeclarationToolStripMenuItem.Text = "Go to Declaration";
            this.goToDeclarationToolStripMenuItem.Click += new System.EventHandler(this.goToDeclarationToolStripMenuItem_Click);
            // 
            // projectView
            // 
            this.projectView.AllowDrop = true;
            this.projectView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectView.DragCursor = null;
            this.projectView.DragCursorType = Galaxy_Editor_2.DragCursorType.None;
            this.projectView.DragImageIndex = 0;
            this.projectView.DragMode = System.Windows.Forms.DragDropEffects.Move;
            this.projectView.DragNodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.projectView.DragNodeOpacity = 0.3D;
            this.projectView.DragOverNodeBackColor = System.Drawing.SystemColors.Highlight;
            this.projectView.DragOverNodeForeColor = System.Drawing.SystemColors.HighlightText;
            this.projectView.ImageIndex = 0;
            this.projectView.ImageList = this.projectViewImageList;
            this.projectView.Location = new System.Drawing.Point(0, 0);
            this.projectView.Margin = new System.Windows.Forms.Padding(6);
            this.projectView.Name = "projectView";
            this.projectView.SelectedImageIndex = 0;
            this.projectView.Size = new System.Drawing.Size(198, 842);
            this.projectView.TabIndex = 0;
            this.projectView.DragStart += new Galaxy_Editor_2.DragItemEventHandler(this.projectView_DragStart);
            this.projectView.DragComplete += new Galaxy_Editor_2.DragCompleteEventHandler(this.projectView_DragComplete);
            this.projectView.DragCompleteValid += new Galaxy_Editor_2.DragCompletionValidEventHandler(this.projectView_DragCompleteValid);
            this.projectView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.projectView_BeforeLabelEdit);
            this.projectView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.projectView_AfterLabelEdit);
            this.projectView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.projectView_AfterCollapse);
            this.projectView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.projectView_AfterExpand);
            this.projectView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.projectView_AfterSelect);
            this.projectView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.projectView_KeyDown);
            this.projectView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.projectView_KeyPress);
            this.projectView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.projectView_MouseDoubleClick);
            this.projectView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.projectView_MouseDown);
            // 
            // tabStrip
            // 
            this.tabStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabStrip.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.tabStrip.Location = new System.Drawing.Point(0, 0);
            this.tabStrip.Name = "tabStrip";
            this.tabStrip.Size = new System.Drawing.Size(1494, 712);
            this.tabStrip.TabIndex = 0;
            this.tabStrip.TabStripItemClosing += new FarsiLibrary.Win.TabStripItemClosingHandler(this.tabStrip_TabStripItemClosing);
            this.tabStrip.TabStripItemSelectionChanged += new FarsiLibrary.Win.TabStripItemChangedHandler(this.tabStrip_TabStripItemSelectionChanged);
            // 
            // messageView
            // 
            this.messageView.AllowDrop = true;
            this.messageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageView.DragCursor = null;
            this.messageView.DragCursorType = Galaxy_Editor_2.DragCursorType.None;
            this.messageView.DragImageIndex = 0;
            this.messageView.DragMode = System.Windows.Forms.DragDropEffects.Move;
            this.messageView.DragNodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageView.DragNodeOpacity = 0.3D;
            this.messageView.DragOverNodeBackColor = System.Drawing.SystemColors.Highlight;
            this.messageView.DragOverNodeForeColor = System.Drawing.SystemColors.HighlightText;
            this.messageView.ImageIndex = 0;
            this.messageView.ImageList = this.messageViewImageList;
            this.messageView.Location = new System.Drawing.Point(0, 52);
            this.messageView.Margin = new System.Windows.Forms.Padding(6);
            this.messageView.Name = "messageView";
            this.messageView.SelectedImageIndex = 0;
            this.messageView.Size = new System.Drawing.Size(1738, 70);
            this.messageView.TabIndex = 0;
            this.messageView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.messageView_KeyDown);
            this.messageView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.messageView_MouseDoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1944, 960);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Form1";
            this.Text = "Galaxy++ editor by Beier";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.objectBrowserSplitContainer.Panel1.ResumeLayout(false);
            this.objectBrowserSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectBrowserSplitContainer)).EndInit();
            this.objectBrowserSplitContainer.ResumeLayout(false);
            this.ObjectBrowserPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.mapObjectBrowserPage.ResumeLayout(false);
            this.mapObjectBrowserPage.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.projectViewMenu.ResumeLayout(false);
            this.projectViewProjectMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.editorRightClick.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabStrip)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sourceFileToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip projectViewMenu;
        private System.Windows.Forms.ToolStripMenuItem newFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ImageList projectViewImageList;
        private TreeViewDragDrop projectView;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compileToolStripMenuItem;
        public TreeViewDragDrop messageView;
        private System.Windows.Forms.ToolStripMenuItem closeProjectToolStripMenuItem;
        private System.Windows.Forms.ImageList messageViewImageList;
        private FarsiLibrary.Win.FATabStrip tabStrip;
        private System.Windows.Forms.ContextMenuStrip projectViewProjectMenu;
        private System.Windows.Forms.ToolStripMenuItem newProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInExploreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem compileAndCopyToMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem searchDefinitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compileAndSaveToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel compilerStatusText;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeToAnotherVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compileAndSaveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton TBNewFile;
        private System.Windows.Forms.ToolStripButton TBNewFolder;
        private System.Windows.Forms.ToolStripButton TBSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton TBUndo;
        private System.Windows.Forms.ToolStripButton TBRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton TBNewProject;
        private System.Windows.Forms.ToolStripButton TBSaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton TBCut;
        private System.Windows.Forms.ToolStripButton TBCopy;
        private System.Windows.Forms.ToolStripButton TBPaste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton TBFind;
        private System.Windows.Forms.ToolStripSplitButton TBRun;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip editorRightClick;
        private System.Windows.Forms.ToolStripMenuItem RightClickCut;
        private System.Windows.Forms.ToolStripMenuItem RightClickCopy;
        private System.Windows.Forms.ToolStripMenuItem RightClickPaste;
        private System.Windows.Forms.ToolStripButton TBDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem rightClickFind;
        private System.Windows.Forms.SplitContainer objectBrowserSplitContainer;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripComboBox ObjectBrowserCatagory;
        private System.Windows.Forms.ToolStripButton TBRefreshObjectList;
        private System.Windows.Forms.Panel ObjectBrowserPanel;
        private System.Windows.Forms.ListView ObjectBrowserList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolTip ObjectBrowserTooltip;
        private System.Windows.Forms.ToolStripMenuItem changeLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem TSInsertConstructor;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapObjectBrowserToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox CBShowWarnings;
        private System.Windows.Forms.CheckBox CBShowErrors;
        private System.Windows.Forms.ToolStripMenuItem libraryServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newDialogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportErrorToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage mapObjectBrowserPage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PropertyGrid UnitFilterGrid;
        private System.Windows.Forms.Button BTNUnitFilterReset;
        private System.Windows.Forms.Button BTNUnitFilterInsert;
        private System.Windows.Forms.CheckBox CBUnitFilterCompress;
        private System.Windows.Forms.ToolStripMenuItem goToDeclarationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProjectAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openExistingProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reparseStarCraft2FunctionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readTriggerFilesToolStripMenuItem;
    }
}

