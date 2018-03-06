using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Shell;
using FarsiLibrary.Win;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Dialog_Creator;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Editor_control;
using Galaxy_Editor_2.Suggestion_box;
using SharedClasses;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2
{
    partial class Form1 : Form, IMessageFilter
    {
        public const string ServerIP = "46.163.69.112";

        public bool Disposed;
        public static Form1 Form;

        public class OpenFileData
        {
            public bool Changed;
            public FileItem File;
            public FATabStripItem TabPage;
            public MyEditor Editor;
        }

        public class OpenLibFileData
        {
            public Library.File File;
            public FATabStripItem TabPage;
            public MyEditor Editor;
        }

        private TreeNode librariesNode;
        private CompileModWindow modWindow = new CompileModWindow();
        private DirectoryInfo projectDir = new DirectoryInfo("Projects");
        private string openProjectName = "";
        public DirectoryInfo openProjectDir
        {
            get { return _openProjectDir; }
            private set
            {
                openProjectName = "";
                if (value != null)
                {
                    Options.Editor.LastProject = value.Name;
                    Options.General.ProjectOpened(value.Name);
                    openProjectName = value.Name;
                }

                _openProjectDir = value;
                compiler.ProjectDir = value;
                value = _openProjectDir = ProjectProperties.SetProject(value);
                _openProjectDir = value;
                compiler.ProjectDir = value;
                if (value != null)
                {
                    Options.Editor.LastProject = value.Name;
                }

                if (value != null && ProjectProperties.CurrentProjectPropperties.LoadSaveScriptToMap)
                {
                    if (File.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                    {
                        string srcPath = openProjectSrcDir.Dir.FullName;
                        if (Directory.Exists(srcPath))
                            Directory.Delete(srcPath, true);
                        Directory.CreateDirectory(srcPath);
                        MpqEditor.ExtractGalaxyppScriptFiles(
                            new FileInfo(ProjectProperties.CurrentProjectPropperties.MapPath),
                            openProjectSrcDir.Dir,
                            true);
                        UploadedChangesToMap = true;
                        openProjectSrcDir.FixConflicts("*.galaxy++", ".Dialog");
                    }
                    else if (Directory.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                    {
                        string srcPath = openProjectSrcDir.Dir.FullName;
                        if (Directory.Exists(srcPath))
                            Directory.Delete(srcPath, true);
                        Directory.CreateDirectory(srcPath);


                        DirectoryInfo codeDir =
                            new DirectoryInfo(Path.Combine(ProjectProperties.CurrentProjectPropperties.MapPath,
                                                           "Galaxy++"));

                        if (!codeDir.Exists)
                        {
                            MessageBox.Show(this, "Unable to extract script. No script found in map.", "Error");
                            Form.UploadedChangesToMap = false;
                        }
                        else
                        {
                            CopyDirectories(
                                codeDir,
                                ProjectProperties.CurrentProjectPropperties.SrcFolder.Dir);
                            Form.UploadedChangesToMap = true;
                        }


                        openProjectSrcDir.FixConflicts("*.galaxy++", ".Dialog");
                    }
                }



                RefreshObjectBrowser();


            }
        }
        private DirectoryInfo _openProjectDir;
        /*public DirectoryInfo openProjectSrcDir
        {
            get
            {
                return openProjectDir == null
                           ? null
                           : openProjectDir.GetDirectories().FirstOrDefault(dir => dir.Name.ToLower() == "src") ??
                             openProjectDir.CreateSubdirectory("src");
            }
        }

        public DirectoryInfo openProjectOutputDir
        {
            get
            {
                return openProjectDir == null
                           ? null
                           : openProjectDir.GetDirectories().FirstOrDefault(dir => dir.Name.ToLower() == "output") ??
                             openProjectDir.CreateSubdirectory("output");
            }
        }*/

        public FolderItem openProjectSrcDir
        {
            get
            {
                if (openProjectDir == null)
                    return null;
                if (ProjectProperties.CurrentProjectPropperties.SrcFolder == null)
                    ProjectProperties.CurrentProjectPropperties.SrcFolder = new FolderItem(null, "src");
                return ProjectProperties.CurrentProjectPropperties.SrcFolder;
                

            }
        }

        public FolderItem openProjectOutputDir
        {
            get
            {
                if (openProjectDir == null)
                    return null;
                if (ProjectProperties.CurrentProjectPropperties.OutputFolder == null)
                    ProjectProperties.CurrentProjectPropperties.OutputFolder = new FolderItem(null, "output");
                return ProjectProperties.CurrentProjectPropperties.OutputFolder;
            }
        }

        public FileItem CurrentOpenFile
        {
            get
            {
                if (tabStrip.SelectedItem == null) return null;
                if (tabStrip.SelectedItem.Tag is OpenFileData)
                    return ((OpenFileData)tabStrip.SelectedItem.Tag).File;
                return null;
            }
        }

        public List<FileItem> ProjectSourceFiles
        {
            get { return GetSourceFiles(openProjectSrcDir); }
        }

        public static List<FileItem> GetSourceFiles(FolderItem item)
        {
            List<FileItem> list = new List<FileItem>();
            if (item == null) return list;
            foreach (DirItem dirItem in item.Children)
            {
                if (dirItem is FolderItem)
                    list.AddRange(GetSourceFiles((FolderItem) dirItem));
                else if (dirItem is FileItem && !((FileItem)dirItem).Deactivated)
                    list.Add((FileItem) dirItem);
            }
            return list;
        }

        public static List<DialogItem> GetDialogsFiles(FolderItem item)
        {
            List<DialogItem> list = new List<DialogItem>();
            if (item == null) return list;
            foreach (DirItem dirItem in item.Children)
            {
                if (dirItem is FolderItem)
                    list.AddRange(GetDialogsFiles((FolderItem)dirItem));
                else if (dirItem is DialogItem && !((DialogItem)dirItem).Deactivated)
                    list.Add((DialogItem)dirItem);
            }
            return list;
        }

        public void FocusFile(FileItem file)
        {
            if (file.OpenFile == null)
                OpenFile(file);
            else
                tabStrip.SelectedItem = file.OpenFile.TabPage;
        }

        private List<OpenFileData> openFiles = new List<OpenFileData>();
        internal GalaxyCompiler compiler;
        public SuggestionBoxForm suggestionBox;


        public Form1()
        {

            Form = this;
            compiler = new GalaxyCompiler(this);
            compiler.LoadLibraries(/*new List<DirectoryInfo>(new []{new DirectoryInfo("Standard libraries\\Standard")})*/);
            compiler.CompilationSuccessfull += compiler_CompilationSuccessfull;
            compiler.CompilationFailed += compiler_CompilationFailed;
            Application.AddMessageFilter(this);
            suggestionBox = new SuggestionBoxForm(compiler, this);
            //Create directories if missing);
            if (!projectDir.Exists) projectDir.Create();

            InitializeComponent();

            ObjectBrowserCatagory.Items.AddRange(Enum.GetNames(typeof(MapObjectsManager.ObjectType)));
            ObjectBrowserCatagory.SelectedIndex = 0;

            ObjectBrowserTooltip.SetToolTip(ObjectBrowserList, "Double click on an item to insert it in the code.");

            SetObjectBrowserVisible(Options.General.ViewObjectBrowser);
            //BTNShowErrors.BackColor = Options.General.ShowErrors ? Color.FromArgb(255, 239, 187) : panel1.BackColor;
            //BTNShowWarnings.BackColor = Options.General.ShowWarnings ? Color.FromArgb(255, 239, 187) : panel1.BackColor;
            CBShowErrors.Checked = Options.General.ShowErrors;
            CBShowWarnings.Checked = Options.General.ShowWarnings;
            
            if (Options.Editor.OpenInLastProject || Options.OverrideLoad != null)
            {
                string lastProject = Options.OverrideLoad ?? Options.Editor.LastProject;
                foreach (DirectoryInfo directory in projectDir.GetDirectories(lastProject))
                {
                    openProjectDir = directory;
                    compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
                    compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
                    foreach (Library library in ProjectProperties.CurrentProjectPropperties.Libraries)
                    {
                        compiler.AddLibrary(library);
                    }
                    break;
                }
            }

            RebuildProjectView();

            RebuildJumpList();

            UnitFilterGrid.SelectedObject = UnitFilter.Instance;
        }

        public static void RebuildJumpList()
        {
            JumpList myJumpList = new JumpList();
            foreach (string project in Options.General.RecentProjects)
            {
                JumpTask task = new JumpTask();
                task.ApplicationPath = Application.ExecutablePath;
                task.Arguments = "-open \"" + project + "\"";
                task.Title = project;
                task.CustomCategory = "Recent Projects";
                task.IconResourcePath = Path.Combine(Application.StartupPath, "icon.ico");
                task.WorkingDirectory = Application.StartupPath;
                myJumpList.JumpItems.Add(task);
            }
            System.Windows.Application app = System.Windows.Application.Current ?? new System.Windows.Application();
            JumpList.SetJumpList(app, myJumpList);
        }
        


        //New project
        private void projectToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // string projectName = GetUniqueStringDialog("New Project", "Name", "", projectDir.GetDirectories(),
            //                                            "The project can not be created because another project of same name already exists.");
            DirectoryInfo[] directories = projectDir.GetDirectories();
            string[] takenNames = new string[directories.Length];
            for (int i = 0; i < directories.Length; i++)
                takenNames[i] = directories[i].Name.ToLower();
            NewProjectForm dialog = new NewProjectForm(takenNames, projectDir.FullName);
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;

            string projectName = dialog.ProjectName;
            string folderName = dialog.Directory;

            CreateNewProject(projectName, folderName);
            //Test valid path
            //Now done in new project form
            /*{
                if (projectName.Trim() == "")
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
                DirectoryInfo t;
                try
                {
                    t = new DirectoryInfo(Path.Combine(projectDir.FullName, projectName));
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
                if (t.Parent.FullName.Trim('\\', '/') != projectDir.FullName.Trim('\\', '/'))
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
            }*/



            /*if (openProjectDir != null)
            {
                bool cancel;
                CloseProject(out cancel);
                if (cancel) return;
            }

            DirectoryInfo subDir = projectDir.CreateSubdirectory(projectName);
            if (projectDir.FullName.Trim('\\', '/') != folderName)
            {
                StreamWriter writer = File.CreateText(Path.Combine(subDir.FullName, "Relocate.txt"));
                writer.WriteLine(Path.Combine(folderName, projectName));
                writer.Flush();
                writer.Close();
                writer.Dispose();
                Directory.CreateDirectory(Path.Combine(folderName, projectName));
            }

            openProjectDir = subDir;

        
            openProjectDir.CreateSubdirectory("src");
            FileInfo defultMapScript = new FileInfo("DefaultMapScript.galaxy++");
            if (defultMapScript.Exists)
            {
                defultMapScript.CopyTo(openProjectSrcDir.Dir.FullName + "\\MapScript.galaxy++");
                openProjectSrcDir.FixConflicts(".galaxy++", ".Dialog");
                compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
                compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
                //AddSourceFilesToCompiler(openProjectSrcDir);
            }

            RebuildProjectView();*/
        }

        void CreateNewProject(string projectName, string folderName, string targetName = null)
        {
            if (targetName == null)
                targetName = Path.Combine(folderName, projectName);
            if (openProjectDir != null)
            {
                bool cancel;
                CloseProject(out cancel);
                if (cancel) return;
            }

            DirectoryInfo subDir = projectDir.CreateSubdirectory(projectName);
            if (projectDir.FullName.Trim('\\', '/') != folderName)
            {
                StreamWriter writer = File.CreateText(Path.Combine(subDir.FullName, "Relocate.txt"));
                writer.WriteLine(targetName);
                writer.Flush();
                writer.Close();
                writer.Dispose();
                Directory.CreateDirectory(targetName);
            }

            openProjectDir = subDir;


            openProjectDir.CreateSubdirectory("src");
            FileInfo defultMapScript = new FileInfo("DefaultMapScript.galaxy++");
            if (defultMapScript.Exists)
            {
                defultMapScript.CopyTo(openProjectSrcDir.Dir.FullName + "\\MapScript.galaxy++");
                openProjectSrcDir.FixConflicts(".galaxy++", ".Dialog");
                compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
                compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
                //AddSourceFilesToCompiler(openProjectSrcDir);
            }

            RebuildProjectView();
        }


        private void openExistingProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "G++ project properties file(properties.dat)|properties.dat";
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            FileInfo propertiesFile = new FileInfo(dialog.FileName);
            DirectoryInfo newProjectDir = propertiesFile.Directory;

            DirectoryInfo[] directories = projectDir.GetDirectories();
            DirectoryInfo subDir;
            for (int i = 0; i < directories.Length; i++)
                if (newProjectDir.Name.ToLower() == directories[i].Name.ToLower())
                {
                    bool isTargetDir = false;
                    if (directories[i].FullName.Trim('\\', '/') == newProjectDir.FullName.Trim('\\', '/'))
                        isTargetDir = true;
                    else 
                    {
                        foreach (FileInfo relocateFile in directories[i].GetFiles("Relocate.txt"))
                        {
                            StreamReader reader = new StreamReader(relocateFile.FullName);
                            string relocatePath = reader.ReadToEnd().Trim('\n', '\r', '\\', '/');
                            if (relocatePath == newProjectDir.FullName.Trim('\\', '/'))
                                isTargetDir = true;
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    if (isTargetDir)
                    {
                        if (openProjectDir != null)
                        {
                            bool cancel;
                            CloseProject(out cancel);
                            if (cancel) return;
                        }
                        OpenProject(directories[i]);
                        return;
                    }
                    MessageBox.Show(this,
                                    "A project with the name " + newProjectDir.Name +
                                    " already exists.\nPlease specify a new name.", "Project already exists");
                    string[] takenNames = new string[directories.Length];
                    for (int j = 0; j < directories.Length; j++)
                        takenNames[j] = directories[j].Name.ToLower();

                    NewProjectForm newNameDialog = new NewProjectForm(takenNames, projectDir.FullName, false, false);
                    if (newNameDialog.ShowDialog(this) == DialogResult.Cancel)
                        return;
                    //CreateNewProject(newNameDialog.ProjectName, newProjectDir.Parent.FullName, newProjectDir.FullName);
                    if (openProjectDir != null)
                    {
                        bool cancel;
                        CloseProject(out cancel);
                        if (cancel) return;
                    }

                    subDir = projectDir.CreateSubdirectory(newNameDialog.ProjectName);
                    //if (projectDir.FullName.Trim('\\', '/') != folderName)
                    {
                        StreamWriter writer = File.CreateText(Path.Combine(subDir.FullName, "Relocate.txt"));
                        writer.WriteLine(newProjectDir.FullName);
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }
                    OpenProject(subDir);
                    return;
                }
            if (openProjectDir != null)
            {
                bool cancel;
                CloseProject(out cancel);
                if (cancel) return;
            }

            subDir = projectDir.CreateSubdirectory(newProjectDir.Name);
            //if (projectDir.FullName.Trim('\\', '/') != folderName)
            {
                StreamWriter writer = File.CreateText(Path.Combine(subDir.FullName, "Relocate.txt"));
                writer.WriteLine(newProjectDir.FullName);
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            OpenProject(subDir);
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            DirectoryInfo[] directories = projectDir.GetDirectories();
            string[] takenNames = new string[directories.Length];
            for (int i = 0; i < directories.Length; i++)
                takenNames[i] = directories[i].Name.ToLower();
            
            NewProjectForm dialog = new NewProjectForm(takenNames, projectDir.FullName, true);
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;

            string projectName = dialog.ProjectName;
            string folderName = dialog.Directory;

            //Copy current project to a safe location
            DirectoryInfo currentProjectCopyLoc = new DirectoryInfo("temp");
            int nr = 0;
            while (currentProjectCopyLoc.Exists)
            {
                currentProjectCopyLoc = new DirectoryInfo("temp" + ++nr);
            }
            CopyDirectories(openProjectDir, currentProjectCopyLoc);
            //Save current project
            SaveAll();
            //Create relocate if neccecery
            DirectoryInfo subDir = new DirectoryInfo(Path.Combine(projectDir.FullName, projectName));
            DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(folderName, projectName));
            if (projectDir.FullName.Trim('\\', '/') != folderName)
            {
                subDir.Create();
                StreamWriter writer = File.CreateText(Path.Combine(subDir.FullName, "Relocate.txt"));

                writer.WriteLine(Path.Combine(folderName, projectName));
                writer.Flush();
                writer.Close();
                writer.Dispose();
                Directory.CreateDirectory(Path.Combine(folderName, projectName));
            }
            //Close currnet project, and then move it
            DirectoryInfo oldDir = openProjectDir;
            bool cancel;
            CloseProject(out cancel);
            string oldDirName = oldDir.FullName;
            oldDir.MoveTo(targetDir.FullName);
            //Move the original project back
            currentProjectCopyLoc.MoveTo(oldDirName);


            //Open the other project
            OpenProject(subDir);
        }

        void OpenProject(DirectoryInfo dir)
        {
            openProjectDir = dir;
            //Add all source files to the compiler
            compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
            compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
            foreach (Library library in ProjectProperties.CurrentProjectPropperties.Libraries)
            {
                compiler.AddLibrary(library);
            }
            //AddSourceFilesToCompiler(openProjectSrcDir);
            RebuildProjectView();
        }

        private void projectView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (openProjectDir == null)
            {//Open selected project
                TreeNode node = projectView.SelectedNode;
                if (node == null || node.Tag == null)
                    return;
                OpenProject((DirectoryInfo) node.Tag);
            }
            else
            {
                if (projectView.SelectedNode.Tag is FileItem)
                {
                    //Open selected file
                    OpenFile((FileItem) projectView.SelectedNode.Tag);
                } 
                else if (projectView.SelectedNode.Tag is Library.File)
                {
                    OpenFile((Library.File) projectView.SelectedNode.Tag);
                }
                else if (projectView.SelectedNode.Tag is DialogItem)
                {
                    OpenFile((DialogItem) projectView.SelectedNode.Tag, projectView.SelectedNode);
                }
            }
        }

        /*
        private void AddSourceFilesToCompiler(FileSystemInfo file)
        {
            if (file is FileInfo)
            {
                if (file.Name.EndsWith(".galaxy++"))
                    compiler.AddSourceFile((FileInfo) file);
            }
            if (file is DirectoryInfo)
            {
                DirectoryInfo dir = (DirectoryInfo) file;
                foreach (FileSystemInfo fileSystemInfo in dir.GetFileSystemInfos())
                {
                    AddSourceFilesToCompiler(fileSystemInfo);
                }
            }
        }*/
        
        internal void RebuildProjectView()
        {
            projectView.Nodes.Clear();
            if (openProjectDir == null)
            {//List of projects
                closeProjectToolStripMenuItem.Enabled =
                    saveProjectAsToolStripMenuItem.Enabled = 
                    compileAndSaveAsToolStripMenuItem.Enabled =
                    compileToolStripMenuItem.Enabled =
                    compileAndCopyToMapToolStripMenuItem.Enabled =
                    compileAndSaveToolStripMenuItem.Enabled = 
                    projectSettingsToolStripMenuItem.Enabled =
                    TBNewFile.Enabled =
                    TBNewFolder.Enabled =
                    TBSave.Enabled =
                    TBSaveAll.Enabled =
                    TBCut.Enabled =
                    TBCopy.Enabled =
                    TBPaste.Enabled =
                    TBUndo.Enabled =
                    TBRedo.Enabled =
                    TBRun.Enabled =
                    TBFind.Enabled =
                    ObjectBrowserPanel.Enabled =
                    uploadLibraryToolStripMenuItem.Enabled =
                    downloadLibraryToolStripMenuItem.Enabled = false;
                librariesNode = null;
                projectView.ContextMenuStrip = projectViewProjectMenu;
                TreeNode headding = new TreeNode("Projects   ");
                headding.NodeFont = new Font(projectView.Font, FontStyle.Bold);
                foreach (DirectoryInfo directory in projectDir.GetDirectories())
                {
                    TreeNode projectNode = new TreeNode(directory.Name);
                    projectNode.Tag = directory;
                    projectView.Nodes.Add(projectNode);
                }
                projectView.Sort();
                projectView.Sorted = false;
                projectView.Nodes.Insert(0, headding);
                headding.SelectedImageIndex = headding.ImageIndex = 1;
            }
            else
            {//List of files in project
                projectView.Sorted = false;
                closeProjectToolStripMenuItem.Enabled =
                    saveProjectAsToolStripMenuItem.Enabled = 
                    compileAndSaveAsToolStripMenuItem.Enabled =
                    compileToolStripMenuItem.Enabled =
                    compileAndSaveToolStripMenuItem.Enabled =
                    projectSettingsToolStripMenuItem.Enabled =
                    TBNewFile.Enabled =
                    TBNewFolder.Enabled =
                    TBSave.Enabled =
                    TBSaveAll.Enabled =
                    TBRun.Enabled =
                    TBFind.Enabled =
                    ObjectBrowserPanel.Enabled =
                    uploadLibraryToolStripMenuItem.Enabled =
                    downloadLibraryToolStripMenuItem.Enabled = true;
                compileAndCopyToMapToolStripMenuItem.Enabled = !ProjectProperties.CurrentProjectPropperties.IsMod;
                runToolStripMenuItem.Enabled = !ProjectProperties.CurrentProjectPropperties.IsMod;
                projectView.ContextMenuStrip = projectViewMenu;
                FolderItem dir = openProjectSrcDir;
                dir.GUINode = new TreeNode(openProjectName + "   ");
                dir.GUINode.NodeFont = new Font(projectView.Font, FontStyle.Bold);
                dir.GUINode.Tag = dir;

                if (dir.FixConflicts(".galaxy++", ".Dialog"))
                    ProjectProperties.CurrentProjectPropperties.Save();

                AddNodes(dir);
                //projectView.Sort();
                projectView.Nodes.Insert(0, dir.GUINode);
                //dir.GUINode.Expand();
                //headding.ExpandAll();

                //Libraries
                librariesNode = new TreeNode("Libraries  ");
                librariesNode.NodeFont = new Font(projectView.Font, FontStyle.Bold);
                foreach (Library library in ProjectProperties.CurrentProjectPropperties.Libraries)
                {
                    TreeNode node = new TreeNode(library.ToString());
                    AddNodes(library.Items, node);
                    librariesNode.Nodes.Add(node);
                    node.Tag = library;
                    node.SelectedImageIndex = node.ImageIndex = 4;
                }
                projectView.Nodes.Add(librariesNode);

                if (compiler.Compiling)
                    return;

                dir = openProjectOutputDir;
                if (dir.FixConflicts(".galaxy", "BankList.xml","Trigger"))
                    ProjectProperties.CurrentProjectPropperties.Save();
                if (dir.Children.Count() > 0)
                {
                    dir.GUINode = new TreeNode("Output   ");
                    dir.GUINode.NodeFont = new Font(projectView.Font, FontStyle.Bold);
                    dir.GUINode.Tag = dir;

                    AddNodes(dir);
                    //projectView.Sort();
                    projectView.Nodes.Add(dir.GUINode);
                    //dir.GUINode.Expand();
                }
            }
        }

        private void AddNodes(List<Library.Item> items, TreeNode parent)
        {
            foreach (Library.Item item in items)
            {
                if (item is Library.Folder)
                {
                    Library.Folder folder = (Library.Folder) item;
                    TreeNode node = new TreeNode(folder.Name);
                    AddNodes(folder.Items, node);
                    parent.Nodes.Add(node);
                    node.Tag = folder;
                    node.SelectedImageIndex = node.ImageIndex = 1;
                }
                else if (item is Library.File)
                {
                    Library.File file = (Library.File) item;
                    TreeNode node = new TreeNode(file.Name);
                    parent.Nodes.Add(node);
                    node.Tag = file;
                    node.SelectedImageIndex = node.ImageIndex = 2;
                }
            }
        }

        private void AddNodes(FolderItem dir)
        {
            //dir is a good enough representation of what is on hdd.. just use that tree

            foreach (DirItem dirItem in dir.Children)
            {
                dirItem.GUINode = new TreeNode(dirItem.Text);
                dirItem.GUINode.Tag = dirItem;
                dirItem.GUINode.SelectedImageIndex =
                    dirItem.GUINode.ImageIndex = dirItem is FolderItem
                                                     ? 1
                                                     : dirItem.Text.EndsWith(".xml")
                                                           ? 3
                                                           : dirItem is DialogItem ? 5 : 2;
                dirItem.GUINode.ForeColor = Color.Black;
                dir.GUINode.Nodes.Add(dirItem.GUINode);
                if (dirItem is FileItem)
                {
                    FileItem fileItem = (FileItem)dirItem;
                    if (fileItem.Deactivated)
                        dirItem.GUINode.ForeColor = Color.Gray;
                }
                if (dirItem is FolderItem)
                {
                    FolderItem folderItem = (FolderItem) dirItem;
                    AddNodes(folderItem);
                    
                    if (!folderItem.Children.Any(child => child.GUINode.ForeColor == Color.Black))
                        dirItem.GUINode.ForeColor = Color.Gray;
                }
                if (dirItem is DialogItem)
                {
                    DialogItem item = (DialogItem) dirItem;
                    item.CodeGUINode = new TreeNode(item.Name.Substring(0, item.Name.LastIndexOf(".Dialog")) + ".galaxy++");
                    item.CodeGUINode.SelectedImageIndex = item.CodeGUINode.ImageIndex = 2;
                    item.CodeGUINode.Tag = item;
                    item.GUINode.Nodes.Add(item.CodeGUINode);

                    item.DesignerGUINode = new TreeNode(item.Name.Substring(0, item.Name.LastIndexOf(".Dialog")) + ".Designer.galaxy++");
                    item.DesignerGUINode.SelectedImageIndex = item.DesignerGUINode.ImageIndex = 2;
                    item.DesignerGUINode.Tag = item;
                    item.GUINode.Nodes.Add(item.DesignerGUINode);

                }
            }
            if (dir.Expanded)
                dir.GUINode.Expand();
            else
                dir.GUINode.Collapse();
        }

        public void ProjectMapsUpdated(bool isMod)
        {
            compileAndCopyToMapToolStripMenuItem.Enabled = !isMod;
            runToolStripMenuItem.Enabled = !isMod;
        }
       

        public string GetUniqueStringDialog(string title, string headding, string reqPostfix, IEnumerable<FileSystemInfo> existing, string errorMsg)
        {
            GetStringDialog dialog = new GetStringDialog(title, headding, "");
            String filename;
            while (true)
            {
                if (dialog.ShowDialog(this) == DialogResult.Cancel)
                    return null;
                filename = dialog.GetString();
                if (!filename.EndsWith(reqPostfix)) filename += reqPostfix;
                if (filename == "")
                {
                    MessageBox.Show(this,
                                    "You cannot specify nothing",
                                    "Error");
                    continue;
                }
                //If this name is already taken
                if (existing.Any(f => f.Name.ToLower() == filename.ToLower()))
                {
                    MessageBox.Show(this,
                                    errorMsg,
                                    "Error");
                    continue;
                }
                break;
            }
            return filename;
        }

        private void newDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MakeNewDialog();
            }
            catch (FileNotFoundException err)
            {
                if (err.ToString().Contains("Microsoft.Xna.Framework"))
                {
                    MessageBox.Show(this,
                                    "An error occured creating the dialog.\nIt looks like you don't have XNA 3.1 installed.\nYou can find it at\nhttp://www.microsoft.com/download/en/details.aspx?id=15163");
                    return;
                }
                throw;
            }
        }

        private void MakeNewDialog()
        {
            if (openProjectDir == null)
            {
//Open an unsaved file

            }
            else
            {
//Make file, and open it
                //Find it's position in the gui tree
                TreeNode folderNode = projectView.SelectedNode ??
                                        ProjectProperties.CurrentProjectPropperties.SrcFolder.GUINode;


                DirItem markedChild = (DirItem) folderNode.Tag;
                FolderItem parentFolder = (FolderItem) (markedChild.Parent ?? markedChild);
                if (markedChild is FolderItem)
                    parentFolder = (FolderItem) markedChild;


                //Find a default unique name
                string name = "New dialog.Dialog";
                int i = 1;
                while (parentFolder.Children.Any(child => child.Text == name))
                {
                    i++;
                    name = "New dialog" + i + ".Dialog";
                }
                DialogItem item = new DialogItem(parentFolder);
                item.Name = name;

                //Create the file
                DialogData dialogData = new DialogData();
                Dialog mainDialog = new Dialog(null, new Rectangle(0, 0, 500, 400), dialogData);
                mainDialog.Anchor = Dialog_Creator.Enums.Anchor.TopLeft;
                dialogData.Dialogs.Add(mainDialog);
                dialogData.Save(item.FullName);

                //Todo: Add to compiler

                item.GUINode = new TreeNode(name);
                item.GUINode.SelectedImageIndex = item.GUINode.ImageIndex = 5;
                item.GUINode.Tag = item;
                //Image index

                //Insert into tree
                int insertAt = parentFolder.Children.IndexOf(markedChild) + 1;
                if (insertAt == 0 || insertAt == parentFolder.Children.Count)
                {
                    parentFolder.Children.Add(item);
                    parentFolder.GUINode.Nodes.Add(item.GUINode);
                }
                else
                {
                    parentFolder.Children.Insert(insertAt, item);
                    parentFolder.GUINode.Nodes.Insert(insertAt, item.GUINode);
                }

                item.CodeGUINode = new TreeNode(name.Substring(0, name.LastIndexOf(".Dialog")) + ".galaxy++");
                item.CodeGUINode.SelectedImageIndex = item.CodeGUINode.ImageIndex = 2;
                item.CodeGUINode.Tag = item;
                item.GUINode.Nodes.Add(item.CodeGUINode);

                item.DesignerGUINode =
                    new TreeNode(name.Substring(0, name.LastIndexOf(".Dialog")) + ".Designer.galaxy++");
                item.DesignerGUINode.SelectedImageIndex = item.DesignerGUINode.ImageIndex = 2;
                item.DesignerGUINode.Tag = item;
                item.GUINode.Nodes.Add(item.DesignerGUINode);

                compiler.AddDialogFile(item);

                ProjectProperties.CurrentProjectPropperties.Save();
                UploadedChangesToMap = false;
                UploadToMap();

                projectView.SelectedNode = item.GUINode;
                item.GUINode.EnsureVisible();
                BeginRename(item.GUINode);
            }

            
        }

        private void MakeNewFile()
        {
            if (openProjectDir == null)
            {//Open an unsaved file
                
            }
            else
            {//Make file, and open it
                //Find it's position in the gui tree
                TreeNode folderNode = projectView.SelectedNode ?? ProjectProperties.CurrentProjectPropperties.SrcFolder.GUINode;
            
            
                DirItem markedChild = (DirItem) folderNode.Tag;
                FolderItem parentFolder = (FolderItem) (markedChild.Parent ?? markedChild);
                if (markedChild is FolderItem)
                    parentFolder = (FolderItem) markedChild;
            
            
                //Find a default unique name
                string name = "New file.galaxy++";
                int i = 1;
                while (parentFolder.Children.Any(child => child.Text == name))
                {
                    i++;
                    name = "New file" + i + ".galaxy++";
                }

                //Create the file
                FileInfo fileInfo = new FileInfo(parentFolder.Dir.FullName + "\\" + name);
                fileInfo.CreateText().Close();
                FileItem newFile = new FileItem(parentFolder, name);

                compiler.AddSourceFile(newFile);
                
                newFile.GUINode = new TreeNode(name);

                newFile.GUINode.Tag = newFile;
                newFile.GUINode.ImageIndex = newFile.GUINode.SelectedImageIndex = 2;
               
                //Insert into tree
                int insertAt = parentFolder.Children.IndexOf(markedChild) + 1;
                if (insertAt == 0 || insertAt == parentFolder.Children.Count)
                {
                    parentFolder.Children.Add(newFile);
                    parentFolder.GUINode.Nodes.Add(newFile.GUINode);
                }
                else
                {
                    parentFolder.Children.Insert(insertAt, newFile);
                    parentFolder.GUINode.Nodes.Insert(insertAt, newFile.GUINode);
                }
                ProjectProperties.CurrentProjectPropperties.Save();
                UploadedChangesToMap = false;
                UploadToMap();

                projectView.SelectedNode = newFile.GUINode;
                newFile.GUINode.EnsureVisible();
                BeginRename(newFile.GUINode);



                //Find nearest folder
                /*TreeNode folderNode = projectView.SelectedNode;
                DirectoryInfo dir;
                FindNearestFolderNode(ref folderNode, out dir);
                //Get desired filename
                String filename = GetUniqueStringDialog("New File", "Name", ".galaxy++", dir.GetFiles(),
                                                        "The file can not be created because another file of same name already exists in that folder.");
                if (filename == null) return;

                FileInfo file = new FileInfo(dir.FullName + "\\" + filename);
                file.CreateText().Close();
                TreeNode node = new TreeNode(file.Name);
                node.Tag = file;
                node.ImageIndex = node.SelectedImageIndex = 2;
                (folderNode ?? projectView.Nodes[0]).Nodes.Add(node);
                node.EnsureVisible();
                projectView.SelectedNode = node;
                compiler.AddSourceFile(file);
                OpenFile(file);*/
            }
        }

        private void sourceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeNewFile();
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeNewFile();
        }

        public void OpenFile(Library.File file)
        {
            //If it is already open, focus it
            foreach (FATabStripItem item in tabStrip.Items)
            {
                if (item.Tag == file)
                {
                    tabStrip.SelectedItem = item;
                    return;
                }
            }
            //Otherwise make a new tab
            OpenLibFileData openFileData = new OpenLibFileData();
            openFileData.File = file;
            openFileData.Editor = new MyEditor(this, true);
            //If we have another editor, get font styles from that (to save memmory)
            if (openFiles.Count > 0)
                openFileData.Editor.SetFontScheme(openFiles[0].Editor.GetFontScheme());
            else
            {
                openFileData.Editor.AddFontStyle(GalaxyKeywords.InMethodKeywords.mod, GalaxyKeywords.InMethodKeywords.words);
                openFileData.Editor.AddFontStyle(GalaxyKeywords.OutMethodKeywords.mod, GalaxyKeywords.OutMethodKeywords.words);
                openFileData.Editor.AddFontStyle(GalaxyKeywords.SystemExpressions.mod, GalaxyKeywords.SystemExpressions.words);
                openFileData.Editor.AddFontStyle(GalaxyKeywords.InitializerKeywords.mod, GalaxyKeywords.InitializerKeywords.words);
                //file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.TriggerKeywords.mod, GalaxyKeywords.TriggerKeywords.words);
                openFileData.Editor.AddFontStyle(GalaxyKeywords.Primitives.mod, GalaxyKeywords.Primitives.words);
            }
            openFileData.Editor.Dock = DockStyle.Fill;


            openFileData.Editor.Tag = openFileData;

            openFileData.TabPage = new FATabStripItem(file.Name, null);
            openFileData.TabPage.Margin = new Padding(0);
            openFileData.TabPage.Padding = new Padding(0);
            openFileData.TabPage.Tag = openFileData;

            
            openFileData.Editor.Text = file.Text;


            openFileData.TabPage.Controls.Add(openFileData.Editor);
            tabStrip.AddTab(openFileData.TabPage);

            tabStrip.SelectedItem = openFileData.TabPage;
            tabStrip_TabStripItemSelectionChanged(null);
            openFileData.Editor.Focus();
        }

        public void OpenFile(DialogItem file, TreeNode nodePressed)
        {
            //If it is already open, focus it
            if (file.OpenFileData != null)
            {
                if (nodePressed == file.OpenFileData.GUINode)
                {
                    tabStrip.SelectedItem = file.OpenFileData.TabPage;
                    file.OpenFileData.DialogControl.Focus();
                    return;
                }
                if (nodePressed == file.OpenFileData.CodeGUINode)
                {
                    tabStrip.SelectedItem = file.OpenFileData.CodeTabPage;
                    file.OpenFileData.CodeEditor.Focus();
                    return;
                }
                if (nodePressed == file.OpenFileData.DesignerGUINode)
                {
                    tabStrip.SelectedItem = file.OpenFileData.DesignerTabPage;
                    file.OpenFileData.DesignerEditor.Focus();
                    return;
                }
            }
            else
            {

                file.OpenFileData = DialogData.Load(file.FullName);
                file.OpenFileData.DialogItem = file;
            }

            //Otherwise make a new tab
            if (file.GUINode == nodePressed)
            {
                //Show graphical designer

                file.OpenFileData.DialogControl = new DialogCreatorControl(file.OpenFileData);
                file.OpenFileData.DialogControl.Dock = DockStyle.Fill;


                file.OpenFileData.DialogControl.Tag = file.OpenFileData;

                file.OpenFileData.TabPage = new FATabStripItem(file.Text, null);
                file.OpenFileData.TabPage.Margin = new Padding(0);
                file.OpenFileData.TabPage.Padding = new Padding(0);
                file.OpenFileData.TabPage.Tag = file.OpenFileData;
                file.OpenFileData.GUINode = nodePressed;





                file.OpenFileData.TabPage.Controls.Add(file.OpenFileData.DialogControl);
                tabStrip.AddTab(file.OpenFileData.TabPage);
                //openFiles.Add(file.OpenFileData);

                //file.OpenFile.Editor.OnTextEdited += MyEditor_TextEdited;

                tabStrip.SelectedItem = file.OpenFileData.TabPage;
                tabStrip_TabStripItemSelectionChanged(null);
                file.OpenFileData.DialogControl.Focus();
            }
            else if (file.CodeGUINode == nodePressed)
            {
                bool editorReadOnly = Options.Editor.ReadOnlyOutput &&
                                  file.IsDecendantOf(ProjectProperties.CurrentProjectPropperties.OutputFolder);
                //Otherwise make a new tab
                file.OpenFileData.CodeEditor = new MyEditor(this, editorReadOnly);
                //If we have another editor, get font styles from that (to save memmory)
                if (openFiles.Count > 0)
                    file.OpenFileData.CodeEditor.SetFontScheme(openFiles[0].Editor.GetFontScheme());
                else
                {
                    file.OpenFileData.CodeEditor.AddFontStyle(GalaxyKeywords.InMethodKeywords.mod, GalaxyKeywords.InMethodKeywords.words);
                    file.OpenFileData.CodeEditor.AddFontStyle(GalaxyKeywords.OutMethodKeywords.mod, GalaxyKeywords.OutMethodKeywords.words);
                    file.OpenFileData.CodeEditor.AddFontStyle(GalaxyKeywords.SystemExpressions.mod, GalaxyKeywords.SystemExpressions.words);
                    file.OpenFileData.CodeEditor.AddFontStyle(GalaxyKeywords.InitializerKeywords.mod, GalaxyKeywords.InitializerKeywords.words);
                    //file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.TriggerKeywords.mod, GalaxyKeywords.TriggerKeywords.words);
                    file.OpenFileData.CodeEditor.AddFontStyle(GalaxyKeywords.Primitives.mod, GalaxyKeywords.Primitives.words);
                }
                file.OpenFileData.CodeEditor.Dock = DockStyle.Fill;


                file.OpenFileData.CodeEditor.Tag = file.OpenFileData;

                file.OpenFileData.CodeTabPage = new FATabStripItem(file.Text.Substring(0, file.Text.LastIndexOf(".Dialog")) + ".galaxy++", null);
                file.OpenFileData.CodeTabPage.Margin = new Padding(0);
                file.OpenFileData.CodeTabPage.Padding = new Padding(0);
                file.OpenFileData.CodeTabPage.Tag = file.OpenFileData;
                file.OpenFileData.CodeGUINode = nodePressed;




                file.OpenFileData.CodeEditor.Text = file.OpenFileData.Code;
                //file.OpenFile.Editor.SetHiddenBlocks(file.ClosedBlocks);


                file.OpenFileData.CodeTabPage.Controls.Add(file.OpenFileData.CodeEditor);
                tabStrip.AddTab(file.OpenFileData.CodeTabPage);
                //openFiles.Add(file.OpenFileData);

                file.OpenFileData.CodeEditor.OnTextEdited += MyEditor_TextEdited;

                tabStrip.SelectedItem = file.OpenFileData.CodeTabPage;
                tabStrip_TabStripItemSelectionChanged(null);
                file.OpenFileData.CodeEditor.Focus();
            }
            else if (file.DesignerGUINode == nodePressed)
            {
                //Otherwise make a new tab
                file.OpenFileData.DesignerEditor = new MyEditor(this, true);
                //If we have another editor, get font styles from that (to save memmory)
                if (openFiles.Count > 0)
                    file.OpenFileData.DesignerEditor.SetFontScheme(openFiles[0].Editor.GetFontScheme());
                else
                {
                    file.OpenFileData.DesignerEditor.AddFontStyle(GalaxyKeywords.InMethodKeywords.mod, GalaxyKeywords.InMethodKeywords.words);
                    file.OpenFileData.DesignerEditor.AddFontStyle(GalaxyKeywords.OutMethodKeywords.mod, GalaxyKeywords.OutMethodKeywords.words);
                    file.OpenFileData.DesignerEditor.AddFontStyle(GalaxyKeywords.SystemExpressions.mod, GalaxyKeywords.SystemExpressions.words);
                    file.OpenFileData.DesignerEditor.AddFontStyle(GalaxyKeywords.InitializerKeywords.mod, GalaxyKeywords.InitializerKeywords.words);
                    //file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.TriggerKeywords.mod, GalaxyKeywords.TriggerKeywords.words);
                    file.OpenFileData.DesignerEditor.AddFontStyle(GalaxyKeywords.Primitives.mod, GalaxyKeywords.Primitives.words);
                }
                file.OpenFileData.DesignerEditor.Dock = DockStyle.Fill;


                file.OpenFileData.DesignerEditor.Tag = file.OpenFileData;

                file.OpenFileData.DesignerTabPage = new FATabStripItem(file.Text.Substring(0, file.Text.LastIndexOf(".Dialog")) + ".Designer.galaxy++", null);
                file.OpenFileData.DesignerTabPage.Margin = new Padding(0);
                file.OpenFileData.DesignerTabPage.Padding = new Padding(0);
                file.OpenFileData.DesignerTabPage.Tag = file.OpenFileData;
                file.OpenFileData.DesignerGUINode = nodePressed;




                file.OpenFileData.DesignerEditor.Text = file.OpenFileData.DesignerCode;
                //file.OpenFile.Editor.SetHiddenBlocks(file.ClosedBlocks);


                file.OpenFileData.DesignerTabPage.Controls.Add(file.OpenFileData.DesignerEditor);
                tabStrip.AddTab(file.OpenFileData.DesignerTabPage);
                //openFiles.Add(file.OpenFileData);

                //file.OpenFileData.DesignerEditor.OnTextEdited += MyEditor_TextEdited;

                tabStrip.SelectedItem = file.OpenFileData.DesignerTabPage;
                tabStrip_TabStripItemSelectionChanged(null);
                file.OpenFileData.DesignerEditor.Focus();
            }
        }

        public void OpenFile(FileItem file)
        {
            //If it is already open, focus it
            if (file.OpenFile != null)
            {
                tabStrip.SelectedItem = file.OpenFile.TabPage;
                file.OpenFile.Editor.Focus();
                return;
            }

            bool editorReadOnly = Options.Editor.ReadOnlyOutput &&
                                  file.IsDecendantOf(ProjectProperties.CurrentProjectPropperties.OutputFolder);
            //Otherwise make a new tab
            file.OpenFile = new OpenFileData();
            file.OpenFile.File = file;
            file.OpenFile.Editor = new MyEditor(this, editorReadOnly);
            //If we have another editor, get font styles from that (to save memmory)
            if (openFiles.Count > 0)
                file.OpenFile.Editor.SetFontScheme(openFiles[0].Editor.GetFontScheme());
            else
            {
                file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.InMethodKeywords.mod, GalaxyKeywords.InMethodKeywords.words);
                file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.OutMethodKeywords.mod, GalaxyKeywords.OutMethodKeywords.words);
                file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.SystemExpressions.mod, GalaxyKeywords.SystemExpressions.words);
                file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.InitializerKeywords.mod, GalaxyKeywords.InitializerKeywords.words);
                //file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.TriggerKeywords.mod, GalaxyKeywords.TriggerKeywords.words);
                file.OpenFile.Editor.AddFontStyle(GalaxyKeywords.Primitives.mod, GalaxyKeywords.Primitives.words);
            }
            file.OpenFile.Editor.Dock = DockStyle.Fill;

            
            file.OpenFile.Editor.Tag = file.OpenFile;

            file.OpenFile.TabPage = new FATabStripItem(file == null ? "Untitled" : file.Text, null);
            file.OpenFile.TabPage.Margin = new Padding(0);
            file.OpenFile.TabPage.Padding = new Padding(0);
            file.OpenFile.TabPage.Tag = file.OpenFile;

           

            
            StreamReader reader = file.File.OpenText();
            file.OpenFile.Editor.Text = reader.ReadToEnd();
            reader.Close();
            file.OpenFile.Editor.SetHiddenBlocks(file.ClosedBlocks);
            

            file.OpenFile.TabPage.Controls.Add(file.OpenFile.Editor);
            tabStrip.AddTab(file.OpenFile.TabPage);
            openFiles.Add(file.OpenFile);

            file.OpenFile.Editor.OnTextEdited += MyEditor_TextEdited;

            tabStrip.SelectedItem = file.OpenFile.TabPage;
            tabStrip_TabStripItemSelectionChanged(null);
            file.OpenFile.Editor.Focus();
        }

        
        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Make new folder
            TreeNode folderNode = projectView.SelectedNode ?? ProjectProperties.CurrentProjectPropperties.SrcFolder.GUINode;
            
            
            DirItem markedChild = (DirItem) folderNode.Tag;
            FolderItem parentFolder = (FolderItem) (markedChild.Parent ?? markedChild);
            if (markedChild is FolderItem)
                parentFolder = (FolderItem) markedChild;
            

            string name = "New folder";
            int i = 1;
            while (parentFolder.Children.Any(child => child.Text == name))
            {
                i++;
                name = "New folder" + i;
            }

            FolderItem newFolder = new FolderItem(parentFolder, name);

            

            newFolder.GUINode = new TreeNode(name);

            newFolder.GUINode.Tag = newFolder;
            newFolder.GUINode.ImageIndex = newFolder.GUINode.SelectedImageIndex = 1;

            //Insert into tree
            int insertAt = parentFolder.Children.IndexOf(markedChild) + 1;
            if (insertAt == 0 || insertAt == parentFolder.Children.Count)
            {
                parentFolder.Children.Add(newFolder);
                parentFolder.GUINode.Nodes.Add(newFolder.GUINode);
            }
            else
            {
                parentFolder.Children.Insert(insertAt, newFolder);
                parentFolder.GUINode.Nodes.Insert(insertAt, newFolder.GUINode);
            }
            ProjectProperties.CurrentProjectPropperties.Save();
            

            projectView.SelectedNode = newFolder.GUINode;
            newFolder.GUINode.EnsureVisible();
            BeginRename(newFolder.GUINode);
        }

        private void BeginRename(TreeNode item)
        {
            projectView.SelectedNode = item;
            projectView.LabelEdit = true;
            item.BeginEdit();
        }

        private bool nextKeyHandled;
        private bool editingLabelName;
        private void projectView_KeyDown(object sender, KeyEventArgs e)
        {
            TreeNode selectedNode = projectView.SelectedNode;
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (editingLabelName) return;
                    nextKeyHandled = true;
                    RemoveTreeNode(projectView.SelectedNode);
                    break;
                case Keys.Return:
                    if (selectedNode == null || selectedNode.Tag == null) break;
                    nextKeyHandled = true;
                    if (openProjectDir == null)
                    {//Open project
                        openProjectDir = (DirectoryInfo)selectedNode.Tag;
                        compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
                        compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
                        foreach (Library library in ProjectProperties.CurrentProjectPropperties.Libraries)
                        {
                            compiler.AddLibrary(library);
                        }
                        RebuildProjectView();
                        break;
                    }
                    //A project is open
                    if (selectedNode.Tag is FileItem)
                    {//Open file
                        OpenFile((FileItem) selectedNode.Tag);
                        break;
                    }
                    if (projectView.SelectedNode.Tag is Library.File)
                    {
                        OpenFile((Library.File)projectView.SelectedNode.Tag);
                        break;
                    }
                    if (projectView.SelectedNode.Tag is DialogItem)
                    {
                        OpenFile((DialogItem) projectView.SelectedNode.Tag, projectView.SelectedNode);
                        break;
                    }
                    if (selectedNode.Tag is FolderItem)
                    {//Toggle dir
                        selectedNode.Toggle();
                        ((FolderItem) selectedNode.Tag).Expanded = ! ((FolderItem) selectedNode.Tag).Expanded;
                        break;
                    }
                    break;
                case Keys.F2:
                    if (projectView.SelectedNode != null && projectView.SelectedNode.Tag != null &&
                        !(projectView.SelectedNode.Tag is Library || projectView.SelectedNode.Tag is Library.Item) &&
                        !(projectView.SelectedNode.Tag is DialogItem && projectView.SelectedNode != ((DialogItem)projectView.SelectedNode.Tag).GUINode))
                    {
                        BeginRename(projectView.SelectedNode);
                    }
                    break;
            }
        }

        private void projectView_KeyPress(object sender, KeyPressEventArgs e)
        {
            //To get rid of the beep when a key is pressed
            e.Handled = nextKeyHandled;
            nextKeyHandled = false;
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (editingLabelName) return;
            RemoveTreeNode(projectView.SelectedNode);
        }

        private void RemoveTreeNode(TreeNode node)
        {
            if (node == null) return;
            //A headding
            if (node.Parent == null && openProjectDir != null) return;
            //A project
            if (node.Tag is DirectoryInfo)
            {
                if (openProjectDir == null)
                {
                    if (MessageBox.Show(this, 
                                        "Are you sure you wish to completely remove the project " + 
                                            node.Text + "?", 
                                        "Remove Project",
                                        MessageBoxButtons.YesNo) 
                                    == DialogResult.No)
                        return;
                    DirectoryInfo dir = (DirectoryInfo)node.Tag;
                    //dir.Delete(true);
                    DeleteDir(dir.FullName);
                    node.Remove();
                    return;
                }
                
            }
            if (node.Tag is FolderItem)
            {//Folder
                if (MessageBox.Show(this,
                                    "Are you sure you wish to completely remove the folder " + node.Text + ", and all files in it?",
                                    "Remove Folder",
                                    MessageBoxButtons.YesNo)
                                == DialogResult.No)
                    return;
                FolderItem folderItem = (FolderItem) node.Tag;
                //If the deleted file was open, close it.
                //RecursivlyCloseOpenFiles(folderItem);
                folderItem.Delete();
                //folderItem.Dir.Delete(true);
                //folderItem.Parent.Children.Remove(folderItem);
                //folderItem.Parent = null;
                //folderItem.GUINode.Remove();
                //ProjectProperties.CurrentProjectPropperties.Save();
                UploadedChangesToMap = false;
                UploadToMap();
                return;
            }
            //File
            if (node.Tag is FileItem)
            {
                FileItem file = (FileItem)node.Tag;
                if (MessageBox.Show(this,
                                        "Are you sure you wish to completely remove the file " + node.Text + "?",
                                        "Remove File",
                                        MessageBoxButtons.YesNo)
                                    == DialogResult.No)
                    return;
                //If the deleted file was open, close it.
                //RecursivlyCloseOpenFiles(file);
                //compiler.RemoveSourceFile(file);
                file.Delete();
                //file.File.Delete();
                //file.Parent.Children.Remove(file);
                //file.Parent = null;
                //ProjectProperties.CurrentProjectPropperties.Save();
                //node.Remove();
                UploadedChangesToMap = false;
                UploadToMap();
                return;
            }
            if (node.Tag is Library)
            {
                Library lib = (Library) node.Tag;
                ProjectProperties.CurrentProjectPropperties.Libraries.Remove(lib);
                node.Remove();
            }
            if (node.Tag is Library.Item)
            {
                MessageBox.Show(this, "That item is read only.\nTo remove it you must remove the entire library.",
                                "Error");
            }
            if (node.Tag is DialogItem && node == ((DialogItem)node.Tag).GUINode)
            {
                DialogItem file = (DialogItem)node.Tag;
                if (MessageBox.Show(this,
                                        "Are you sure you wish to completely remove the dialog " + node.Text + "?",
                                        "Remove File",
                                        MessageBoxButtons.YesNo)
                                    == DialogResult.No)
                    return;
                //If the deleted file was open, close it.
                //RecursivlyCloseOpenFiles(file);
                //compiler.RemoveSourceFile(file);
                if (file.OpenFileData != null)
                {
                    bool b;
                    if (file.OpenFileData.TabPage != null)
                        CloseFile(file.OpenFileData, file.OpenFileData.TabPage, out b, true);
                    if (file.OpenFileData != null && file.OpenFileData.CodeTabPage != null)
                        CloseFile(file.OpenFileData, file.OpenFileData.CodeTabPage, out b, true);
                    if (file.OpenFileData != null && file.OpenFileData.DesignerTabPage != null)
                        CloseFile(file.OpenFileData, file.OpenFileData.DesignerTabPage, out b, true);
                }
                file.Delete();
                UploadedChangesToMap = false;
                UploadToMap();
            }
        }

        internal void RecursivlyCloseOpenFiles(DirItem node)
        {
            if (node is FolderItem)
            {
                foreach (DirItem child in ((FolderItem)node).Children)
                {
                    RecursivlyCloseOpenFiles(child);
                }
                return;
            }
            if (node is FileItem)
            {
                FileItem file = (FileItem)node;
                if (file.OpenFile != null)
                {
                    file.OpenFile.Editor.Dispose();
                    tabStrip.RemoveTab(file.OpenFile.TabPage);
                    file.OpenFile.TabPage.Dispose();
                    openFiles.Remove(file.OpenFile);
                    file.OpenFile = null;
                }
                return;
            }
        }

        private void projectView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node = projectView.GetNodeAt(e.X, e.Y);
            if (node != null && projectView.SelectedNode != node)
            {
                projectView.SelectedNode = node;
            }

            if (node == null)
            {
                activateToolStripMenuItem.Enabled = false;
                activateToolStripMenuItem.Checked = true;
            }
            else if (node.Tag is DirItem)
            {
                DirItem item = (DirItem)node.Tag;

                if (item.IsDecendantOf(openProjectOutputDir))
                {
                    return;
                }

                if (item is FileItem)
                {
                    FileItem file = (FileItem)item;
                    activateToolStripMenuItem.Enabled = true;
                    activateToolStripMenuItem.Checked = !file.Deactivated;
                }
                if (item is FolderItem)
                {
                    FolderItem folder = (FolderItem)item;
                    activateToolStripMenuItem.Enabled = true;
                    activateToolStripMenuItem.Checked = GetSourceFiles(folder).Any(file => !file.Deactivated);

                }
            }
            TBDelete.Enabled =
                removeToolStripMenuItem.Enabled = node != null && (openProjectDir == null || node.Parent != null) &&
                        !(node.Tag is Library.Item);
            renameToolStripMenuItem.Enabled = node != null && (openProjectDir == null || node.Parent != null) &&
                                              !(node.Tag is Library || node.Tag is Library.Item);
            TBNewFile.Enabled = 
                TBNewFolder.Enabled =
                sourceFileToolStripMenuItem.Enabled =
                openInExploreToolStripMenuItem.Enabled =
                newFolderToolStripMenuItem.Enabled =
                newFileToolStripMenuItem.Enabled =
                newDialogToolStripMenuItem.Enabled = node != null && !(node.Tag is Library || node.Tag is Library.Item || node.Tag == null);

            activateToolStripMenuItem.Enabled &= TBNewFile.Enabled;

            if (node != null && node.Tag is DialogItem)
            {
                DialogItem item = (DialogItem)node.Tag;
                activateToolStripMenuItem.Enabled =
                    TBNewFile.Enabled =
                    TBNewFolder.Enabled =
                    sourceFileToolStripMenuItem.Enabled =
                    openInExploreToolStripMenuItem.Enabled =
                    newFolderToolStripMenuItem.Enabled =
                    newFileToolStripMenuItem.Enabled =
                    removeToolStripMenuItem.Enabled = 
                    renameToolStripMenuItem.Enabled = 
                    TBDelete.Enabled =
                    newDialogToolStripMenuItem.Enabled = node == item.GUINode;
            }
        }

        private void projectView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            activateToolStripMenuItem.Enabled &=
                TBNewFile.Enabled =
                newDialogToolStripMenuItem.Enabled =
                TBNewFolder.Enabled =
                sourceFileToolStripMenuItem.Enabled =
                openInExploreToolStripMenuItem.Enabled =
                newFolderToolStripMenuItem.Enabled =
                newFileToolStripMenuItem.Enabled = openProjectDir != null && !(e.Node.Tag is Library || e.Node.Tag is Library.Item || e.Node.Tag == null);

            if (e.Node.Tag is DialogItem)
            {
                DialogItem item = (DialogItem)e.Node.Tag;
                activateToolStripMenuItem.Enabled =
                    TBNewFile.Enabled =
                    TBNewFolder.Enabled =
                    sourceFileToolStripMenuItem.Enabled =
                    openInExploreToolStripMenuItem.Enabled =
                    newFolderToolStripMenuItem.Enabled =
                    newFileToolStripMenuItem.Enabled =
                    removeToolStripMenuItem.Enabled =
                    renameToolStripMenuItem.Enabled =
                    TBDelete.Enabled =
                    newDialogToolStripMenuItem.Enabled = e.Node == item.GUINode;
            }
        }

        private void projectView_DragStart(object sender, DragItemEventArgs e)
        {
            if (openProjectDir == null || e.Node.Tag == null /*|| ((DirItem)e.Node.Tag).IsDecendantOf(ProjectProperties.CurrentProjectPropperties.OutputFolder)*/
                 || e.Node == ProjectProperties.CurrentProjectPropperties.OutputFolder.GUINode
                 || e.Node == ProjectProperties.CurrentProjectPropperties.SrcFolder.GUINode
                || e.Node.Tag is Library || e.Node.Tag is Library.Item || 
                (e.Node.Tag is DialogItem && e.Node != ((DialogItem)e.Node.Tag).GUINode))
                projectView.AllowDrop = false;
            else
                projectView.AllowDrop = true;
        }


        private void projectView_DragComplete(object sender, DragCompleteEventArgs e)
        {
            DirItem draggedItem = (DirItem) e.RemovedNode.Tag;
            FolderItem newParentItem = (FolderItem) e.NewParent.Tag;



            draggedItem.MoveTo(newParentItem,
                e.InsertIndex == -1 ? newParentItem.Children.Count : e.InsertIndex);

            MatchClonedTags(e.CloneNode, e.RemovedNode);

            RebuildProjectView();
            //FixSystemInfoRefferences(e.NewSourceNode);
        }

        private void MatchClonedTags(TreeNode clone, TreeNode original)
        {
            DirItem item = (DirItem) (clone.Tag = original.Tag);
            item.GUINode = clone;
            for (int i = 0; i < clone.Nodes.Count; i++)
            {
                MatchClonedTags(clone.Nodes[i], original.Nodes[i]);
            }
        }

        private void projectView_DragCompleteValid(object sender, DragCompletionValidEventArgs e)
        {
            if (openProjectDir == null)
            {
                e.Cancel = true;
                return;
            }

            //Go up to nearest dir
            if (e.NewParent.Parent != null && !(e.NewParent.Tag is FolderItem && ((FolderItem)e.NewParent.Tag).Children.Count == 0))
                e.NewParent = e.NewParent.Parent;
            while (true)
            {
                if (e.NewParent == null)
                {
                    e.Cancel = true;
                    return;
                }
                if (e.NewParent.Tag is FolderItem)
                    break;
                e.NewParent = e.NewParent.Parent;
            }


            while (e.DraggedOn != null && (e.DraggedOn.Parent == null || e.DraggedOn.Parent.Tag != e.NewParent.Tag))
                e.DraggedOn = e.DraggedOn.Parent;

            e.InsertIndex = e.DraggedOn == null ? -1 : e.NewParent.Nodes.IndexOf(e.DraggedOn);

            //If we are moving it down in same folder, we must add 1 to the index
            /*if (e.InsertIndex != -1 && e.NewParent == e.OldParent && e.NewParent.Nodes.IndexOf(e.RemovedNode) < e.InsertIndex)
                e.InsertIndex++;*/

            DirItem draggedItem = (DirItem) e.RemovedNode.Tag;

            //If in output folder, only allow drag with same parrent
            if (draggedItem.IsDecendantOf(openProjectOutputDir) && e.NewParent != e.OldParent)
            {
                e.Cancel = true;
                return;
            }

            FolderItem targetFolder = (FolderItem) e.NewParent.Tag;
            if (targetFolder.Dir.GetFileSystemInfos().Any(item =>
                item.FullName.TrimEnd('\\', '/') !=  draggedItem.FullName.TrimEnd('\\', '/') && 
                item.Name == draggedItem.Text))
            {
                MessageBox.Show(this, "Unable to move item since another item of same name exists at the destination.", "Error");
                e.Cancel = true;
                return;
            }

            
        }

        /*private void FixSystemInfoRefferences(TreeNode node)
        {
            DirectoryInfo parentDir = (DirectoryInfo)node.Parent.Tag ?? openProjectSrcDir;
            if (node.Tag is FileInfo)
            {
                FileInfo newFileInfo = new FileInfo(parentDir.FullName + "\\" + node.Text);
                foreach (OpenFileData t in openFiles)
                {
                    if (t.File == node.Tag)
                    {
                        t.File = newFileInfo;
                    }
                }
            }
            if (node.Tag is DirectoryInfo)
            {
                node.Tag = new DirectoryInfo(parentDir.FullName + "\\" + node.Text);
            }
            foreach (TreeNode node1 in node.Nodes)
            {
                FixSystemInfoRefferences(node1);
            }
        }*/
        
        private void projectView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            editingLabelName = false;
            projectView.LabelEdit = false;
            string newName = e.Label;
            TreeNode node = e.Node;

            if (newName == null)
                return;

            if (openProjectDir == null)
            {
                //Project list
                DirectoryInfo target;
                try
                {
                    target = new DirectoryInfo(Path.Combine(projectDir.FullName, newName));
                }
                catch(Exception err)
                {
                    MessageBox.Show(this, "Unable to rename the project. Invalid directory name.",
                                    "Error");
                    e.CancelEdit = true;
                    return;
                }
                if (target.Parent.FullName.Trim('\\', '/') != projectDir.FullName.Trim('\\', '/'))
                {
                    MessageBox.Show(this, "The project name can not contain \\ or /.",
                                    "Error");
                    e.CancelEdit = true;
                    return;
                }
                if (target.Exists)
                {
                    MessageBox.Show(this, "Unable to rename the directory, as another the directory of same name already exists.",
                                    "Error");
                    e.CancelEdit = true;
                    return;
                }
                ((DirectoryInfo)node.Tag).MoveTo(target.FullName);
            }
            else
            {
                //File/dir list
                DirItem item = (DirItem) node.Tag;
               
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    if (newName.Contains(c))
                    {
                        string errText = "File names may not contain the characters: ";
                        bool first = true;
                        foreach (char d in Path.GetInvalidFileNameChars())
                        {
                            if (!first)
                                errText += ", ";
                            if (d == '\t')
                                errText += "\\t";
                            else if (d == '\n')
                                errText += "\\n";
                            else if (d == '\r')
                                errText += "\\r";
                            else if (d < 34)
                            {
                                first = true;
                                continue;
                            }
                            else if (d == '\0')
                                errText += "\\0";
                            else
                                errText += d;
                            first = false;
                        }
                        MessageBox.Show(this, errText,
                                        "Error");
                        e.CancelEdit = true;
                        return;
                    }
                }

                if (item is FolderItem)
                {
                    if (newName.Trim() == "")
                    {
                        MessageBox.Show(this, "Unable to rename the folder. Invalid folder name.",
                                        "Error");
                        e.CancelEdit = true;
                        return;
                    }

                    DirectoryInfo target;
                    try
                    {
                        target = new DirectoryInfo(projectDir.FullName + "\\" + newName);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(this, "Unable to rename the folder. Invalid folder name.",
                                        "Error");
                        e.CancelEdit = true;
                        return;
                    }
                    if (target.Parent.FullName.Trim('\\', '/') != projectDir.FullName.Trim('\\', '/'))
                    {
                        MessageBox.Show(this, "The folder name can not contain \\ or /.",
                                        "Error");
                        e.CancelEdit = true;
                        return;
                    }
                }
                
                if (item is FileItem && !newName.EndsWith(".galaxy++"))
                {
                    node.Text = newName = newName + ".galaxy++";
                    e.CancelEdit = true;
                }

                if (item is DialogItem && !newName.EndsWith(".Dialog"))
                {
                    node.Text = newName = newName + ".Dialog";
                    e.CancelEdit = true;
                }

                bool success = item.Rename(newName);
                if (!success)
                {
                    MessageBox.Show(this, "Unable to rename item. Another item of same name probally already exists.",
                                    "Error");
                    e.CancelEdit = true;
                    return;
                }
                if (item is FileItem)
                {
                    FileItem fileItem = (FileItem) item;
                    if (fileItem.OpenFile != null)
                        fileItem.OpenFile.TabPage.Title = newName + (fileItem.OpenFile.Changed ? "*" : "");
                }
                ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
                UploadedChangesToMap = false;
                UploadToMap();
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectView.SelectedNode != null && projectView.SelectedNode.Tag != null &&
                !(projectView.SelectedNode.Tag is Library || projectView.SelectedNode.Tag is Library.Item))
            {
                BeginRename(projectView.SelectedNode);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (openProjectDir != null)
            {
                bool cancel;
                CloseProject(out cancel);
                if (cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Disposed = true;
            suggestionBox.Dispose();
            compiler.Dispose();
        }

        private void MyEditor_TextEdited(MyEditor sender)
        {
            if (sender.Tag is OpenFileData)
            {
                OpenFileData openFile = (OpenFileData) sender.Tag;
                if (openFile.File.IsDecendantOf(openProjectSrcDir))
                {
                    compiler.SourceFileChanged(openFile.File, sender);
                    UploadedChangesToMap = false;
                }
                //Add * to indicate changes
                if (!openFile.Changed)
                {
                    openFile.Changed = true;
                    openFile.TabPage.Title += "*";
                }
                //Enable/disable undo/redo buttons
                TBUndo.Enabled = openFile.Editor.UndoSys.CanUndo;
                TBRedo.Enabled = openFile.Editor.UndoSys.CanRedo;
            }
            else if (sender.Tag is DialogData)
            {
                DialogData openFile = (DialogData)sender.Tag;
                compiler.DialogItemChanged(openFile.DialogItem, sender);
                
                //Add * to indicate changes
                if (!openFile.CodeChanged)
                {
                    openFile.CodeChanged = true;
                    //openFile.CodeTabPage.Title += "*";
                }
                //Enable/disable undo/redo buttons
                TBUndo.Enabled = openFile.CodeEditor.UndoSys.CanUndo;
                TBRedo.Enabled = openFile.CodeEditor.UndoSys.CanRedo;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabStrip.SelectedItem == null) return;
            if (openProjectDir == null) return;
            //Find the selected file
            if (tabStrip.SelectedItem.Tag is DialogData)
            {
                DialogData data = ((DialogData)tabStrip.SelectedItem.Tag);
                data.Save(data.DialogItem.FullName);
                UploadedChangesToMap = false;
                UploadToMap();
                return;
            }
            FileItem item = ((OpenFileData) tabStrip.SelectedItem.Tag).File;

            Save(item.OpenFile, false);
        }

        public void SaveAll()
        {
            saveAllToolStripMenuItem_Click(null, null);
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openProjectDir == null) return;
            foreach (OpenFileData openFileData in openFiles)
            {
                Save(openFileData, true);
            }
            foreach (DialogItem item in GetDialogsFiles(openProjectSrcDir))
            {
                if (item.OpenFileData != null)
                    item.OpenFileData.Save(item.FullName);
            }
            UploadToMap();
        }

        internal bool UploadedChangesToMap;
        private void Save(OpenFileData file, bool savingAll)
        {
            if (!file.Changed)
                return;
            StreamWriter writer = new StreamWriter(file.File.File.Open(FileMode.Create));
            writer.Write(file.Editor.Text);
            writer.Close();

            file.Changed = false;
            file.TabPage.Title = file.File.Text;

            //If all is saved, and I should upload to map - do it
            if (!savingAll)
                UploadToMap();
        }

        private void UploadToMap()
        {
            if (ProjectProperties.CurrentProjectPropperties.LoadSaveScriptToMap)
            {
                if (File.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                {
                    bool success = MpqEditor.SaveGalaxyppScriptFiles(new FileInfo(ProjectProperties.CurrentProjectPropperties.MapPath),
                                                        ProjectProperties.CurrentProjectPropperties.SrcFolder.Dir, false);
                    if (success)
                        UploadedChangesToMap = true;
                }
                else if (Directory.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                {
                    string path = Path.Combine(ProjectProperties.CurrentProjectPropperties.MapPath,
                                                           "Galaxy++");
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    CopyDirectories(openProjectSrcDir.Dir,
                        new DirectoryInfo(path));
                    UploadedChangesToMap = true;
                }
            }



        }


        public bool PreFilterMessage(ref Message m)
        {
            Control ctrl = Control.FromHandle(m.HWnd);
            if (IsControlChild(this, ctrl))
            {
                if (m.Msg == 0x201)//L button down
                {
                    suggestionBox.Hide();
                }
            }
            return false;
        }

        private bool IsControlChild(Control parent, Control child)
        {
            if (child == null) return false;
            if (child == parent) return true;
            return IsControlChild(parent, child.Parent);
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //If no change since last compile, just run compile finished
            if (ProjectProperties.CurrentProjectPropperties.CompileStatus == ProjectProperties.ECompileStatus.SuccessfullyCompiled)
            {
                compiler_CompilationSuccessfull();
                
                return;
            }


            //Save all first
            SaveAll();
           /* foreach (OpenFileData openFile in openFiles)
            {
                Save(openFile, true);
            }


            UploadToMap();*/

            //Clear output directory
            //Close open galaxy files
            for (int i = 0; i < openFiles.Count; i++)
            {
                OpenFileData openFile = openFiles[i];
                if (openFile.File.IsDecendantOf(ProjectProperties.CurrentProjectPropperties.OutputFolder))
                {
                    tabStrip.RemoveTab(openFile.TabPage);
                    openFiles.Remove(openFile);
                    openFile.File.OpenFile = null;
                    i--;
                }
            }

            if (Directory.Exists(openProjectOutputDir.Dir.FullName))
            {
                //openProjectOutputDir.Dir.Delete(true);
                DeleteDir(openProjectOutputDir.Dir.FullName);
                openProjectOutputDir.Dir.Create();
            }
            /*foreach (FileSystemInfo info in ProjectProperties.CurrentProjectPropperties.OutputFolder.Dir.GetFileSystemInfos())
            {
                info.Delete();
            }*/


            compiler.Compile();
            RebuildProjectView();
        }

        private static int deletedDirs = 0;
        internal static void DeleteDir(string path)
        {
            //Wierd errors.. Rename to DeletedDir#
            try
            {
                string targetDir = "DeletedDir" + ++deletedDirs;
                Directory.Move(path, targetDir);
                Directory.Delete(targetDir, true);
            }
            catch (Exception)
            {
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        DeleteDir(subDir);
                    }
                    foreach (string file in Directory.GetFiles(path))
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(path);

                }
                catch (Exception)
                {
                    Directory.Delete(path, true);
                }
            }

        }


        private void CloseFile(OpenFileData file, out bool cancel)
        {
            if (file.Changed)
            {
                DialogResult result = MessageBox.Show(this,
                                                      "You have unsaved changes. Do you wish to save before closing?",
                                                      "Unsaved changes",
                                                      MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    Save(file, false);
            }

            file.File.ClosedBlocks = file.Editor.GetHiddenBlocks();
            ProjectProperties.CurrentProjectPropperties.Save();
            tabStrip.RemoveTab(file.TabPage);
            openFiles.Remove(file);
            cancel = false;
            file.File.OpenFile = null;
        }

        private void CloseFile(DialogData file, FATabStripItem tabPage, out bool cancel, bool ignoreChanges = false)
        {
            if (tabPage == file.TabPage)
            {
                if (file.Changed && !ignoreChanges)
                {
                    DialogResult result = MessageBox.Show(this,
                                                          "You have unsaved changes. Do you wish to save before closing?",
                                                          "Unsaved changes",
                                                          MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Cancel)
                    {
                        cancel = true;
                        return;
                    }
                    if (result == DialogResult.Yes)
                        file.Save(file.DialogItem.FullName);
                }

                //ProjectProperties.CurrentProjectPropperties.Save();
                tabStrip.RemoveTab(tabPage);
                //openFiles.Remove(file);
                file.TabPage = null;
                file.GUINode = null;
                if (file.CodeTabPage == null && file.DesignerTabPage == null)
                    file.DialogItem.OpenFileData = null;
            }
            else if (tabPage == file.CodeTabPage)
            {
                if (file.CodeChanged && !ignoreChanges)
                {
                    DialogResult result = MessageBox.Show(this,
                                                          "You have unsaved changes. Do you wish to save before closing?",
                                                          "Unsaved changes",
                                                          MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Cancel)
                    {
                        cancel = true;
                        return;
                    }
                    if (result == DialogResult.Yes)
                        file.Save(file.DialogItem.FullName);
                }

                //ProjectProperties.CurrentProjectPropperties.Save();
                tabStrip.RemoveTab(tabPage);
                //openFiles.Remove(file);
                file.CodeTabPage = null;
                file.CodeGUINode = null;
                if (file.TabPage == null && file.DesignerTabPage == null)
                    file.DialogItem.OpenFileData = null;
            }
            else if (tabPage == file.DesignerTabPage)
            {
                //ProjectProperties.CurrentProjectPropperties.Save();
                tabStrip.RemoveTab(tabPage);
                //openFiles.Remove(file);
                file.DesignerTabPage = null;
                file.DesignerGUINode = null;
                if (file.TabPage == null && file.CodeTabPage == null)
                    file.DialogItem.OpenFileData = null;
            }
            cancel = false;
        }

        private void CloseProject(out bool cancel)
        {
            //Promt to save unsaved files
            DialogResult result = DialogResult.None;
            foreach (OpenFileData openFile in openFiles)
            {
                if (openFile.Changed)
                {
                    if (result == DialogResult.None)
                    {
                        result = MessageBox.Show(this, "You have unsaved files. Do you wish to save them?",
                                                 "Unsaved changes",
                                                 MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Cancel)
                        {
                            cancel = true;
                            return;
                        }
                    }
                    if (result == DialogResult.Yes)
                        Save(openFile, true);
                }
            }
            foreach (DialogItem item in GetDialogsFiles(openProjectSrcDir))
            {
                if (item.OpenFileData != null && (item.OpenFileData.Changed || item.OpenFileData.CodeChanged))
                {
                    if (result == DialogResult.None)
                    {
                        result = MessageBox.Show(this, "You have unsaved files. Do you wish to save them?",
                                                 "Unsaved changes",
                                                 MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Cancel)
                        {
                            cancel = true;
                            return;
                        }
                    }
                    if (result == DialogResult.Yes)
                        item.OpenFileData.Save(item.FullName);
                    if (item.OpenFileData.TabPage != null)
                        tabStrip.RemoveTab(item.OpenFileData.TabPage);
                    if (item.OpenFileData.CodeTabPage != null)
                        tabStrip.RemoveTab(item.OpenFileData.CodeTabPage);
                    if (item.OpenFileData.DesignerTabPage != null)
                        tabStrip.RemoveTab(item.OpenFileData.DesignerTabPage);
                    item.OpenFileData = null;
                }
            }
            if (result == DialogResult.Yes || result == DialogResult.None)
            {
                UploadToMap();
                if (!UploadedChangesToMap &&
                    ProjectProperties.CurrentProjectPropperties.LoadSaveScriptToMap &&
                    File.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                {
                    result = MessageBox.Show(this, "I was unable to open the map to inject the script files.\n" +
                                                    "Do you have it open in another file?\n\n" + 
                                                    "If they are not saved to the map, all changes will be lost next time you open the project.\n" +
                                                    "Are you sure you wish to close?",
                                             "Unsaved changes",
                                             MessageBoxButtons.YesNoCancel);
                    if (result != DialogResult.Yes)
                    {
                        cancel = true;
                        return;
                    }
                }
            }
            foreach (OpenFileData openFile in openFiles)
            {
                openFile.File.ClosedBlocks = openFile.Editor.GetHiddenBlocks();
                tabStrip.RemoveTab(openFile.TabPage);
            }
            tabStrip.Items.Clear();
            ProjectProperties.CurrentProjectPropperties.Save();

            ProjectProperties.CurrentProjectPropperties.SrcFolder.Close();

            openFiles.Clear();
            compiler.RemoveAllFiles();
            messageView.Nodes.Clear();
            compiler.currentErrorCollection = null;
            openProjectDir = null;
            RebuildProjectView();
            cancel = false;
        }

        private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool cancel;
            CloseProject(out cancel);
            if (!cancel)
                Options.Editor.LastProject = "";
        }

        private void messageView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            bool b = false;
            messageView_MouseDoubleClick((TreeViewDragDrop) sender, e, ref b);
        }

        private void messageView_MouseDoubleClick(TreeViewDragDrop sender, MouseEventArgs e, ref bool handled)
        {

            if (messageView.SelectedNode == null) return;
            if (messageView.SelectedNode is ErrorCollection.Error)
            {
                ErrorCollection.Error error = (ErrorCollection.Error)messageView.SelectedNode;
                FileInfo file = new FileInfo(openProjectSrcDir.Dir.FullName + "\\" + error.FileName + ".galaxy++");
                
                if (!file.Exists)
                {

                    file = new FileInfo(openProjectSrcDir.Dir.FullName + "\\" + error.FileName + ".Dialog");
                    if (!file.Exists)
                    {
                        MessageBox.Show(this, "Could not open file", "Error");
                        return;
                    }
                    if (error.pos.Line == -1)
                    {
                        MessageBox.Show(this, "Could not locate token", "Error");
                        return;
                    }
                    foreach (DialogItem fileItem in GetDialogsFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder))
                    {
                        if (fileItem.FullName == file.FullName)
                        {
                            OpenFile(fileItem, fileItem.CodeGUINode);
                            fileItem.OpenFileData.CodeEditor.MoveCaretTo(error.pos);
                            error.ExpandAll();
                            return;
                        }
                    }
                    MessageBox.Show(this, "Could not locate file in project", "Error");
                }
                if (error.pos.Line == -1)
                {
                    MessageBox.Show(this, "Could not locate token", "Error");
                    return;
                }
                foreach (FileItem fileItem in GetSourceFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder))
                {
                    if (fileItem.File.FullName == file.FullName)
                    {
                        OpenFile(fileItem);
                        fileItem.OpenFile.Editor.MoveCaretTo(error.pos);
                        error.ExpandAll();
                        return;
                    }
                }
                MessageBox.Show(this, "Could not locate file in project", "Error");
            }
        }

        private bool closedTabStrip;
        private void tabStrip_TabStripItemSelectionChanged(TabStripItemChangedEventArgs e)
        {
            TBUndo.Enabled =
                TBRedo.Enabled =
                TBCut.Enabled =
                TBCopy.Enabled =
                TBPaste.Enabled = false;
            if (tabStrip.SelectedItem == null)
            {
                return;
            }
            if (tabStrip.SelectedItem.Tag is OpenLibFileData)
            {
                OpenLibFileData openFile = (OpenLibFileData)tabStrip.SelectedItem.Tag;
                suggestionBox.SetCurrentEditor(openFile.Editor);


                //Enable/disable undo/redo buttons
                if (!closedTabStrip)
                {
                    openFile.Editor.ContextMenuStrip = editorRightClick;
                    TBUndo.Enabled = openFile.Editor.UndoSys.CanUndo;
                    TBRedo.Enabled = openFile.Editor.UndoSys.CanRedo;
                    RightClickCut.Enabled =
                        RightClickPaste.Enabled =
                        TBCut.Enabled =
                        TBPaste.Enabled = !openFile.Editor.IsReadonly;
                    TBCopy.Enabled =
                        RightClickCopy.Enabled = true;
                }
                closedTabStrip = false;
            }
            else if (tabStrip.SelectedItem.Tag is DialogData)
            {
                DialogData openFile = (DialogData)tabStrip.SelectedItem.Tag;


                if (tabStrip.SelectedItem == openFile.CodeTabPage)
                {
                    suggestionBox.SetCurrentEditor(openFile.CodeEditor);
                    //Enable/disable undo/redo buttons
                    if (!closedTabStrip)
                    {
                        openFile.CodeEditor.ContextMenuStrip = editorRightClick;
                        TBUndo.Enabled = openFile.CodeEditor.UndoSys.CanUndo;
                        TBRedo.Enabled = openFile.CodeEditor.UndoSys.CanRedo;
                        RightClickCut.Enabled =
                            RightClickPaste.Enabled =
                            TBCut.Enabled =
                            TBPaste.Enabled = !openFile.CodeEditor.IsReadonly;
                        TBCopy.Enabled =
                            RightClickCopy.Enabled = true;
                    }
                }
                else if (tabStrip.SelectedItem == openFile.DesignerTabPage)
                {
                    suggestionBox.SetCurrentEditor(openFile.DesignerEditor);
                    //Enable/disable undo/redo buttons
                    if (!closedTabStrip)
                    {
                        openFile.DesignerEditor.ContextMenuStrip = editorRightClick;
                        TBUndo.Enabled = openFile.DesignerEditor.UndoSys.CanUndo;
                        TBRedo.Enabled = openFile.DesignerEditor.UndoSys.CanRedo;
                        RightClickCut.Enabled =
                            RightClickPaste.Enabled =
                            TBCut.Enabled =
                            TBPaste.Enabled = !openFile.DesignerEditor.IsReadonly;
                        TBCopy.Enabled =
                            RightClickCopy.Enabled = true;
                    }
                }
                closedTabStrip = false;
            }
            else
            {
                OpenFileData openFile = (OpenFileData)tabStrip.SelectedItem.Tag;
                suggestionBox.SetCurrentEditor(openFile.Editor);
                openFile.Editor.Focus();

                //Enable/disable undo/redo buttons
                if (!closedTabStrip)
                {
                    openFile.Editor.ContextMenuStrip = editorRightClick;
                    TBUndo.Enabled = openFile.Editor.UndoSys.CanUndo;
                    TBRedo.Enabled = openFile.Editor.UndoSys.CanRedo;
                    RightClickCut.Enabled =
                        RightClickPaste.Enabled =
                        TBCut.Enabled =
                        TBPaste.Enabled = !openFile.Editor.IsReadonly;
                    TBCopy.Enabled =
                        RightClickCopy.Enabled = true;
                }
                closedTabStrip = false;
            }
        }

        private void tabStrip_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            closedTabStrip = true;
            if (e.Item.Tag is OpenLibFileData)
            {
                //tabStrip.RemoveTab(e.Item);
                return;
            }
            bool cancel;
            if (e.Item.Tag is DialogData)
            {
                DialogData fileData = (DialogData)e.Item.Tag;
                CloseFile(fileData, e.Item, out cancel);
            }
            else
            {
                OpenFileData fileData = (OpenFileData)e.Item.Tag;
                CloseFile(fileData, out cancel);
            }
            e.Cancel = true;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OptionsForm().ShowDialog(this);
        }


        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectToolStripMenuItem_Click(sender, e);
        }

        private void deleteProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectView.SelectedNode != null)
                RemoveTreeNode(projectView.SelectedNode);
        }

       

        private void openInExploreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectView.SelectedNode == null) return;
            if (projectView.SelectedNode.Tag is FolderItem)
            {
                System.Diagnostics.Process.Start(((FolderItem)projectView.SelectedNode.Tag).Dir.FullName);
            }
            if (projectView.SelectedNode.Tag is FileItem)
            {
                System.Diagnostics.Process.Start(((FileItem)projectView.SelectedNode.Tag).File.Directory.FullName);
            }
        }


        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new AboutForm().Show(this);
        }

        private bool isLoaded;
        private void Form1_Load(object sender, EventArgs e)
        {
            Location = Options.General.FormPos;
            Size = Options.General.FormSize;
            WindowState = Options.General.FormMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
            isLoaded = true;

            ProgramVersion lastVersion = new ProgramVersion(Options.General.LastVersion);
            if (!Options.CreatedNew && LanguageChangesForm.HasChanges(lastVersion))
                new LanguageChangesForm(lastVersion).Show(this);
            if (Options.General.LastVersion != Application.ProductVersion)
                new Change_log_form().Show(this);
            Options.General.LastVersion = Application.ProductVersion;
            //new Dialog_Creator.TestForm().Show();
        }


        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (isLoaded)
            {
                Options.General.FormSize = Size;
                Options.General.FormPos = Location;
                
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (isLoaded)
            {
                if (WindowState == FormWindowState.Maximized)
                    Options.General.FormMaximized = true;
                if (WindowState == FormWindowState.Normal)
                    Options.General.FormMaximized = false;
            }
        }


        private bool copyOutputToMap;
        private bool saveCompiledMap, saveCompiledMapAs;


        private void compiler_CompilationFailed()
        {
            copyOutputToMap = false;
            saveCompiledMap = false;
        }

        private void compiler_CompilationSuccessfull()
        {
            if (InvokeRequired)
            {
                Invoke(new GalaxyCompiler.CompilationDoneEventHandler(compiler_CompilationSuccessfull));
                return;
            }

            openProjectOutputDir.FixConflicts(".galaxy", "BankList.xml","Trigger");
            RebuildProjectView();
            if (!copyOutputToMap && !saveCompiledMap)
                return;
            
                


            FileInfo rootFile =
                new FileInfo(openProjectOutputDir.Dir.FullName + "\\" +
                             ProjectProperties.CurrentProjectPropperties.RootFileName);
            copyOutputToMap = false;
            if (!rootFile.Exists)
            {
                MessageBox.Show(this, "Unable to find the root file.\nDid you forget to define the main entry?",
                                "Unable to copy to map");
            }
            else
            {
                FileSystemInfo mapFile = null;
                bool mapIsDir = false;
                
                if (ProjectProperties.CurrentProjectPropperties.MapPath != "")
                {
                    if (File.Exists(ProjectProperties.CurrentProjectPropperties.MapPath) || 
                        Directory.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                    {
                        if ((File.GetAttributes(ProjectProperties.CurrentProjectPropperties.MapPath) &
                             FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            mapFile = new DirectoryInfo(ProjectProperties.CurrentProjectPropperties.MapPath);
                            mapIsDir = true;
                        }
                        else
                        {
                            mapFile = new FileInfo(ProjectProperties.CurrentProjectPropperties.MapPath);
                            mapIsDir = false;
                        }
                    }
                }
                if (mapFile == null)
                {
                    MessageBox.Show(this, "No map file found. Please locate it.", "Missing data");
                    new ProjectSettingsForm().ShowDialog(this);
                    if (ProjectProperties.CurrentProjectPropperties.MapPath != "")
                    {
                        if (File.Exists(ProjectProperties.CurrentProjectPropperties.MapPath) ||
                            Directory.Exists(ProjectProperties.CurrentProjectPropperties.MapPath))
                        {
                            if ((File.GetAttributes(ProjectProperties.CurrentProjectPropperties.MapPath) &
                                 FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                mapFile = new DirectoryInfo(ProjectProperties.CurrentProjectPropperties.MapPath);
                                mapIsDir = true;
                            }
                            else
                            {
                                mapFile = new FileInfo(ProjectProperties.CurrentProjectPropperties.MapPath);
                                mapIsDir = false;
                            }
                        }
                    }
                    if (mapFile == null)
                    {
                        saveCompiledMapAs = false;
                        return;
                    }
                }

                // make sure the StarCraft2.exe is known to start it
                FileInfo sc2Exe = Options.General.SC2Exe;
                if (sc2Exe == null || !StarCraftExecutableFinder.checkPathValidity(sc2Exe.FullName))
                {
                    FileInfo info = new FileInfo(StarCraftExecutableFinder.findExecutable());
                    if (info.Exists)
                        sc2Exe = Options.General.SC2Exe = info;
                }

                if (saveCompiledMap)
                {
                    saveCompiledMap = false;
                    //Check that there is an output map
                    FileSystemInfo outputMap = null;

                    if (ProjectProperties.CurrentProjectPropperties.OutputMapPath != "")
                    {
                        if (File.Exists(ProjectProperties.CurrentProjectPropperties.OutputMapPath) ||
                            Directory.Exists(ProjectProperties.CurrentProjectPropperties.OutputMapPath))
                        {
                            if ((File.GetAttributes(ProjectProperties.CurrentProjectPropperties.OutputMapPath) &
                                 FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                outputMap = new DirectoryInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                            }
                            else
                            {
                                outputMap = new FileInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                            }
                        }
                    }
                    if (saveCompiledMapAs)
                        outputMap = null;
                    saveCompiledMapAs = false;
                    if (outputMap == null)
                    {
                        MessageBox.Show(this, "Please select an output map.", "Missing data");
                        new ProjectSettingsForm().ShowDialog(this);
                        if (ProjectProperties.CurrentProjectPropperties.OutputMapPath != "")
                        {
                            /*//if (File.Exists(ProjectProperties.CurrentProjectPropperties.OutputMapPath) ||
                            //    Directory.Exists(ProjectProperties.CurrentProjectPropperties.OutputMapPath))
                            {
                                if ((File.GetAttributes(ProjectProperties.CurrentProjectPropperties.OutputMapPath) &
                                     FileAttributes.Directory) == FileAttributes.Directory)
                                {
                                    outputMap = new DirectoryInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                                    outputMapIsDir = true;
                                }
                                else
                                {
                                    outputMap = new FileInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                                    outputMapIsDir = false;
                                }
                            }*/
                            if (ProjectProperties.CurrentProjectPropperties.IsOutputDirectory)
                                outputMap = new DirectoryInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                            else
                                outputMap = new FileInfo(ProjectProperties.CurrentProjectPropperties.OutputMapPath);
                        }
                        if (outputMap == null)
                            return;
                    }

                    //Copy input to output
                    try
                    {
                        Copy(mapFile, outputMap);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(this, "Unable to copy the map. Is the output file in use?", "Error");
                        return;
                    }
                    MpqEditor.CopyOutputToMap(rootFile, outputMap, this);
                    if (!Options.Compiler.NeverAskToRunSavedMap && sc2Exe != null && sc2Exe.Exists && MessageBox.Show(this,
                                        "Don't make any changes to the gui triggers, or the script will be overwritten.\n\nDo you wish to open the map now?"
                                        , "Saved", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        FileInfo editor = sc2Exe.Directory.GetFiles("StarCraft II Editor.exe")[0];
                        ProcessStartInfo psi = new ProcessStartInfo(editor.FullName, outputMap.FullName);
                        Process.Start(psi);
                    }
                    return;
                    //Prompt for a map to save to
                    /* if (mapFile is FileInfo)
                    {
                        FileInfo aMapFile = (FileInfo) mapFile;
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "StarCraft II map (*.SC2Map)|*.SC2Map";
                        dialog.InitialDirectory = aMapFile.Directory.FullName;
                        dialog.FileName = mapFile.Name.Substring(0, mapFile.Name.LastIndexOf('.')) + " Gal++.SC2Map";
                        if (dialog.ShowDialog(this) == DialogResult.Cancel)
                            return;
                        FileInfo saveFile = new FileInfo(dialog.FileName);
                        if (saveFile.FullName != aMapFile.FullName)
                            aMapFile.CopyTo(saveFile.FullName, true);
                        MpqEditor.CopyOutputToMap(rootFile, saveFile, this);
                        if (sc2Exe != null && sc2Exe.Exists && MessageBox.Show(this,
                                        "Don't make any changes to the gui triggers, or the script will be overwritten.\n\nDo you wish to open the map now?"
                                        , "Saved", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Process.Start(saveFile.FullName);
                        }
                    }
                    else
                    {
                        DirectoryInfo aMapFile = (DirectoryInfo)mapFile;
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        dialog.SelectedPath = aMapFile.Parent.FullName;
                        dialog.ShowNewFolderButton = true;
                        //dialog.FileName = mapFile.Name.Substring(0, mapFile.Name.LastIndexOf('.')) + " Gal++.SC2Map";
                        while (true)
                        {
                            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                                return;
                            if (dialog.SelectedPath.Trim('\\', '/').ToLower().EndsWith(".sc2map"))
                                break;
                            MessageBox.Show(this, "The selected folder must end with .SC2Map");
                        }
                        DirectoryInfo saveFile = new DirectoryInfo(dialog.SelectedPath);
                        if (saveFile.FullName != aMapFile.FullName)
                            CopyDirectories(aMapFile, (DirectoryInfo)saveFile);
                        MpqEditor.CopyOutputToMap(rootFile, saveFile, this);
                        if (sc2Exe != null && sc2Exe.Exists && MessageBox.Show(this,
                                        "Don't make any changes to the gui triggers, or the script will be overwritten.\n\nDo you wish to open the map now?"
                                        , "Saved", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            FileInfo editor = sc2Exe.Directory.GetFiles("StarCraft II Editor.exe")[0];
                            ProcessStartInfo psi = new ProcessStartInfo(editor.FullName, saveFile.FullName);
                            Process.Start(psi);
                        }
                    }
                    return;*/
                }
                if (sc2Exe == null || !sc2Exe.Exists)
                    return;



                //Check that sc2 is not running
                if (Process.GetProcessesByName("SC2").Length > 0)
                {
                    MessageBox.Show(this,
                                        "Unable to copy to test file. If Starcraft II is already running, please shut it down first.", "Error");
                    return;
                }

                string runMapName = sc2Exe.Directory.FullName + "\\Maps\\Test\\Galaxy++TestMap" + (mapIsDir ? "Folder" : "File") + ".SC2Map";

                /*int i = 1;
                while (true)
                {
                    FileInfo runFile = new FileInfo(runMapName);
                    try
                    {
                        runFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite).Close();
                        break;
                    }
                    catch (Exception err)
                    {
                        i++;
                        runMapName = sc2Exe.Directory.FullName + "\\Maps\\Test\\Galaxy++TestMap" + i + ".SC2Map";
                        MessageBox.Show(this,
                                        "Unable to copy to test file. If Starcraft II is already running, please shut it down first.", "Error");
                        return;
                    }
                }*/
                if (File.Exists(runMapName) ||
                    Directory.Exists(runMapName))
                {
                    if ((File.GetAttributes(runMapName) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        DirectoryInfo dir = new DirectoryInfo(runMapName);
                        if (dir.Exists)
                            DeleteDir(dir.FullName);
                            //dir.Delete(true);
                    }
                    else
                    {
                        FileInfo file = new FileInfo(runMapName);
                        if (file.Exists)
                            file.Delete();
                    }
                }

                //if (Options.Compiler.RunCopy)
                {
                    //Ensure that all dirs are created
                    EnsurePathCreated(new FileInfo(runMapName).Directory);
                    
                    MpqEditor.CopyTo(mapFile, runMapName);
                    mapFile = mapIsDir ? (FileSystemInfo) new DirectoryInfo(runMapName) : new FileInfo(runMapName);
                }

                /*
                 * Message check
                    Message nr: 0x0287
                    wParam: 17
                    LParam: varies

                    Message nr: 0x0287
                    wParam: 18
                    LParam: same as 1

                    Message nr: 0x0287
                    wParam: 17
                    LParam: varies

                    Message nr: 0x0287
                    wParam: 18
                    LParam: same as 3

                    Message nr: 0x0287
                    wParam: 6
                    LParam: 0
                 */

                bool success;
                if (mapIsDir)
                    success = MpqEditor.CopyOutputToMap(rootFile, (DirectoryInfo)mapFile, this);
                else
                    success = MpqEditor.CopyOutputToMap(rootFile, (FileInfo)mapFile, this);
                if (success)
                {
                    /*if (!Options.Compiler.RunCopy)
                        mapFile.CopyTo(runMapName, true);*/

                    runMapName = runMapName.Substring(sc2Exe.Directory.FullName.Length + "\\Maps\\".Length);

                    /*foreach (Process process in Process.GetProcessesByName("SC2"))
                    {
                        IntPtr ptr = Marshal.StringToHGlobalAuto(runMapName);
                        SendMessage(process.Handle, 0x0287, new IntPtr(0x17), ptr);
                    }*/

                    string args = "-run " + runMapName;// +" -displaymode 0 -trigdebug -preload 1 -NoUserCheats -reloadcheck - difficulty 2 -speed 4";
                    if (Options.Run.Windowed)
                    {
                        args += " -displaymode 0";
                        if (Options.Run.ShowDebug)
                            args += " -trigdebug";
                    }
                    else
                        args += " -displaymode 2";
                    if (Options.Run.EnablePreload)
                        args += " -preload 1";
                    else
                        args += " -preload 0";
                    if (!Options.Run.AllowCheat)
                        args += " -NoUserCheats";
                    args += " -reloadcheck";
                    args += " -difficulty " + Options.Run.Difficulty;
                    args += " -speed " + Options.Run.GameSpeed;
                    if (Options.Run.UseFixedSeed)
                        args += " -fixedseed -seedvalue " + Options.Run.Seed;
                    args += " " + Options.Run.AdditionalArgs;
                    args = args.Trim();


                    //Go from Starcraft II/Starcraft 2.exe
                    //To Starcraft II/Support/SC2Switcher.exe
                    sc2Exe = sc2Exe.Directory.GetDirectories("Support")[0].GetFiles("SC2Switcher.exe")[0];
                    //sc2Exe = sc2Exe.Directory.GetFiles("SC2Switcher.exe")[0];
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = sc2Exe.FullName;
                    startInfo.Arguments = args;
                    Process.Start(sc2Exe.FullName, args);
                }
            }
            
        }

        public static void EnsurePathCreated(DirectoryInfo dir)
        {
            if (dir == null)
                return;
            if (!dir.Exists)
            {
                EnsurePathCreated(dir.Parent);
                dir.Create();
            }
        }

        public static void Copy(FileSystemInfo file1, FileSystemInfo file2)
        {
            if (file1.FullName == file2.FullName)
                return;
            if (file1 is FileInfo)
            {
                if (file2 is FileInfo)
                {
                    ((FileInfo) file1).CopyTo(file2.FullName, true);
                }
                else
                {
                    //MessageBox.Show("It is not yet possible to have intput and output maps of diffrend type.", "Error");
                    MpqEditor.Copy((FileInfo) file1, (DirectoryInfo)file2);
                }
            }
            else
            {
                if (file2 is FileInfo)
                {
                    //MessageBox.Show("It is not yet possible to have intput and output maps of diffrend type.", "Error");
                    MpqEditor.Copy((DirectoryInfo) file1, (FileInfo) file2);
                }
                else
                {
                    CopyDirectories((DirectoryInfo) file1, (DirectoryInfo) file2);
                }
            }
        }

        public static void CopyDirectories(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectories(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void compileAndCopyToMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ProjectProperties.CurrentProjectPropperties.IsMod)
                copyOutputToMap = true;
            else
            {
                if (modWindow.ShowDialog(this) == DialogResult.Cancel)
                    return;
            }
            compileToolStripMenuItem_Click(sender, e);
        }

        private void compileAndSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveCompiledMap = true;
            compileToolStripMenuItem_Click(sender, e);
        }

        private void compileAndSaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveCompiledMap = true;
            saveCompiledMapAs = true;
            compileToolStripMenuItem_Click(sender, e);
        }

        private void projectSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ProjectSettingsForm().ShowDialog(this);
        }

        private void searchDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SearcDefinitionsForm().Show();
        }

        private void projectView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is FolderItem)
            {
                FolderItem folder = (FolderItem) e.Node.Tag;
                folder.Expanded = e.Node.IsExpanded;
                ProjectProperties.CurrentProjectPropperties.Save();
            }
        }

        private void projectView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is FolderItem)
            {
                FolderItem folder = (FolderItem)e.Node.Tag;
                folder.Expanded = e.Node.IsExpanded;
                ProjectProperties.CurrentProjectPropperties.Save();
            }
        }

        private void activateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openProjectDir == null) return;
            if (projectView.SelectedNode == null) return;
            DirItem item = (DirItem)projectView.SelectedNode.Tag;
            DeActivate(item, activateToolStripMenuItem.Checked);
            ProjectProperties.CurrentProjectPropperties.Save();
            while (item.Parent != null && item.Parent.Parent != null)
            {
                item.Parent.GUINode.ForeColor = GetSourceFiles(item.Parent).Any(file => !file.Deactivated) ? Color.Black : Color.Gray;
                item = item.Parent;
            }
            activateToolStripMenuItem.Checked = !activateToolStripMenuItem.Checked;
        }

        private void DeActivate(DirItem item, bool deactivate)
        {
            if (item is FolderItem)
            {
                FolderItem folder = (FolderItem) item;
                if (folder.Parent != null)
                    folder.GUINode.ForeColor = deactivate ? Color.Gray : Color.Black;
                foreach (DirItem dirItem in folder.Children)
                {
                    DeActivate(dirItem, deactivate);
                }
            }
            if (item is FileItem)
            {
                FileItem file = (FileItem)item;
                if (file.Deactivated != deactivate)
                {
                    if (deactivate)
                        compiler.RemoveSourceFile(file);
                    else
                        compiler.AddSourceFile(file);
                }
                file.Deactivated = deactivate;
                file.GUINode.ForeColor = deactivate ? Color.Gray : Color.Black;
            }
        }

        

        private delegate void SetStatusTextDelegate(string text);
        public void SetStatusText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new SetStatusTextDelegate(SetStatusText), text);
                return;
            }
            compilerStatusText.Text = text;
        }

        private Thread updaterThread;
        private void CheckForUpdates(object _fromUser)
        {
            bool fromUser = (bool) _fromUser;
            if (!InvokeRequired)
            {
                if (updaterThread != null && updaterThread.IsAlive)
                {
                    if (fromUser)
                        MessageBox.Show(this, "An update process is already in progress.", "Unable to update.");
                    return;
                }
                updaterThread = new Thread(CheckForUpdates);
                updaterThread.Start(_fromUser);
                return;
            }

            FtpWebRequest req;
            string bestVersionName;
            WebResponse response;
            try
            {

                req = (FtpWebRequest) FtpWebRequest.Create(new Uri("ftp://46.163.69.112/Releases/"));
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.ListDirectory;
                req.Timeout = 5000;
                response = req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                int[] currentVersion = GetVersion(Application.ProductVersion);
                string[] versions = reader.ReadToEnd().Split('\n');
                response.Close();
                int[] bestVersion = currentVersion;
                bestVersionName = Application.ProductVersion;
                foreach (string version in versions)
                {
                    if (version == "")
                        continue;
                    string v = "";
                    foreach (char c in version)
                    {
                        if ((c >= '0' && c <= '9') || c == '.')
                            v += c;
                    }
                    v = v.Trim('.');
                    int[] ver = GetVersion(v);
                    if (ver[0] > bestVersion[0] ||
                        (ver[0] == bestVersion[0] && ver[1] > bestVersion[1]) ||
                        (ver[1] == bestVersion[1] && ver[2] > bestVersion[2]))
                    {
                        bestVersion = ver;
                        bestVersionName = v;
                    }
                }

            }
            catch (WebException)
            {
                if (fromUser)
                    Invoke(new MessageBoxShow1(MessageBox.Show),
                        "Update server is unreachable.\nAre you connected to the internet?\n\nIf the problem persists, contact me on mapster", "Error");
                return;
            }
            if (bestVersionName != Application.ProductVersion)
            {
                if (((DialogResult)Invoke(new MessageBoxShow2(MessageBox.Show), this, "A new version is avalible.\nDo you wish to update now?", "Version " + bestVersionName + " released", MessageBoxButtons.YesNo)) == DialogResult.No)
                    return;
                //Download zip file
                Invoke(new ChangeVersionDelegate(ChangeVersion), bestVersionName);
                //ChangeVersion(bestVersionName);
            }
            else if (fromUser)
                Invoke(new MessageBoxShow1(MessageBox.Show), this, "The program is up to date.", "Updater");
        }

        private delegate DialogResult MessageBoxShow1(IWin32Window owner, string message, string title);
        private delegate DialogResult MessageBoxShow2(IWin32Window owner, string message, string title, MessageBoxButtons buttons);

        private UpdatingForm updaterForm;
        private void ShowUpdaterForm(object max)
        {
            updaterForm = new UpdatingForm(Math.Max(0, (long)max));
            updaterForm.ShowDialog();
            updaterForm = null;
        }

        private delegate void ChangeVersionDelegate(string versionName);
        private void ChangeVersion(string versionName)
        {
            FtpWebRequest req =
                    (FtpWebRequest)
                    FtpWebRequest.Create(
                        new Uri("ftp://46.163.69.112/Releases/Version " + versionName + "/Galaxy++ editor v" +
                                versionName + ".zip"));
            req.UseBinary = true;
            req.Method = WebRequestMethods.Ftp.GetFileSize;
            
            FtpWebResponse response = (FtpWebResponse) req.GetResponse();
            Thread updaterFormThread = new Thread(new ParameterizedThreadStart(ShowUpdaterForm));
            updaterFormThread.Start(response.ContentLength);
            while (updaterForm == null || updaterForm.Visible == false)
            {
                Thread.Sleep(0);
            }

            req =
                    (FtpWebRequest)
                    FtpWebRequest.Create(
                        new Uri("ftp://46.163.69.112/Releases/Version " + versionName + "/Galaxy++ editor v" +
                                versionName + ".zip"));
            req.UseBinary = true;
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            response = (FtpWebResponse)req.GetResponse();
            Stream readStream = response.GetResponseStream();

            FileInfo newVersionFile = new FileInfo("newVersion.zip");
            Stream writeStream = newVersionFile.Open(FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[256];
            int bytesRead;
            while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                updaterForm.AddValue(bytesRead);
            }
            updaterForm.Close();
            writeStream.Close();
            response.Close();
            //Copy update files to output dir
            DirectoryInfo updateDir = new DirectoryInfo("Update");
            if (updateDir.Exists)
            {
                //updateDir.Delete(true);
                DeleteDir(updateDir.FullName);
                updateDir = new DirectoryInfo("Update");
            }

            updateDir.Create();
            FileInfo file = new FileInfo("newVersion.zip");
            file.MoveTo(Path.Combine(updateDir.FullName, file.Name));
            file = new FileInfo("Updater.exe");
            file.CopyTo(Path.Combine(updateDir.FullName, file.Name));
            file = new FileInfo("ICSharpCode.SharpZipLib.dll");
            file.CopyTo(Path.Combine(updateDir.FullName, file.Name));

            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = new DirectoryInfo(".\\Update").FullName;
            startInfo.FileName = new FileInfo(".\\Update\\Updater.exe").FullName;

            Process.Start(startInfo);
            Close();
        }

        private int[] GetVersion(string ver)
        {
            //Expect ##.###.#
            string[] strings = ver.Split('.');
            return new[] {int.Parse(strings[0]), int.Parse(strings[1]), int.Parse(strings[2])};
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdates(true);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            DirectoryInfo updateDir = new DirectoryInfo("Update");
            if (updateDir.Exists)
            {
                try
                {
                    //updateDir.Delete(true);

                    DeleteDir(updateDir.FullName);
                }
                catch (Exception)
                {
                    //Im tired of weird errors here. 
                    //Dir not empty, unauthorized, etc..
                }
            }
            CheckForUpdates(false);
        }

        private void changeToAnotherVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FtpWebRequest req;
            WebResponse response;
            try
            {

                req = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://46.163.69.112/Releases/"));
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.ListDirectory;
                response = req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                List<string> versions = reader.ReadToEnd().Split('\n').ToList();
                response.Close();
                for (int i = 0; i < versions.Count; i++)
                {
                    if (versions[i] == "")
                    {
                        versions.RemoveAt(i);
                        i--;
                        continue;
                    }
                    string v = "";
                    foreach (char c in versions[i])
                    {
                        if ((c >= '0' && c <= '9') || c == '.')
                            v += c;
                    }
                    v = v.Trim('.');
                    versions[i] = v;
                }

                DowngradeWindow dialog = new DowngradeWindow(versions);
                if (dialog.ShowDialog(this) == DialogResult.Cancel)
                    return;
                ChangeVersion(versions[dialog.SelectedIndex]);
            }
            catch (WebException)
            {
                MessageBox.Show(
                    "Update server is unreachable.\nAre you connected to the internet?\n\nIf the problem persists, contact me on mapster", "Error");
                return;
            }
        }


        public void ReloadSourceFiles()
        {
            compiler.RemoveAllFiles();
            compiler.AddSourceFiles(GetSourceFiles(openProjectSrcDir));
            compiler.AddDialogItems(GetDialogsFiles(openProjectSrcDir));
            foreach (Library library in ProjectProperties.CurrentProjectPropperties.Libraries)
            {
                compiler.AddLibrary(library);
            }
        }

        

        private void TBFind_Click(object sender, EventArgs e)
        {
            if (CurrentOpenFile != null)
            {
                CurrentOpenFile.OpenFile.Editor.OpenFindAndReplace();
            }
            else
            {
                FindAndReplaceForm.form.InitSearch();
            }
        }

        private void TBUndo_Click(object sender, EventArgs e)
        {
            CurrentOpenFile.OpenFile.Editor.UndoSys.Undo();
        }

        private void TBRedo_Click(object sender, EventArgs e)
        {
            CurrentOpenFile.OpenFile.Editor.UndoSys.Redo();
        }

        private void TBCut_Click(object sender, EventArgs e)
        {
            if (CurrentOpenFile != null)
            {
                CurrentOpenFile.OpenFile.Editor.Copy(true);
            }
            else if (tabStrip.SelectedItem.Tag is DialogData)
            {
                DialogData data = (DialogData)tabStrip.SelectedItem.Tag;
                if (tabStrip.SelectedItem == data.CodeTabPage)
                    data.CodeEditor.Copy(true);
            }
        }

        private void TBCopy_Click(object sender, EventArgs e)
        {
            if (CurrentOpenFile != null)
            {
                CurrentOpenFile.OpenFile.Editor.Copy(false);
            }
            else if (tabStrip.SelectedItem.Tag is DialogData)
            {
                DialogData data = (DialogData)tabStrip.SelectedItem.Tag;
                if (tabStrip.SelectedItem == data.DesignerTabPage)
                    data.DesignerEditor.Copy(false);
                else
                    data.CodeEditor.Copy(false);
            }
        }

        private void TBPaste_Click(object sender, EventArgs e)
        {
            if (CurrentOpenFile != null)
            {
                CurrentOpenFile.OpenFile.Editor.Paste();
            }
            else if (tabStrip.SelectedItem.Tag is DialogData)
            {
                DialogData data = (DialogData)tabStrip.SelectedItem.Tag;
                if (tabStrip.SelectedItem == data.CodeTabPage)
                    data.CodeEditor.Paste();
            }
        }

        private void TBDelete_Click(object sender, EventArgs e)
        {
            if (projectView.SelectedNode != null)
                RemoveTreeNode(projectView.SelectedNode);
        }

        private void TBRefreshObjectList_Click(object sender, EventArgs e)
        {
            RefreshObjectBrowser();
        }

        public void RefreshObjectBrowser()
        {
            FileSystemInfo map = null;
            if (ProjectProperties.CurrentProjectPropperties != null)
            {
                map = ProjectProperties.CurrentProjectPropperties.InputMap;
                if (map is FileInfo)
                {
                    File.Copy(map.FullName, ".\\ExtractorMap.SC2Map");
                    map = new FileInfo(".\\ExtractorMap.SC2Map");
                }
            }

            MapObjectsManager.ExtractData(map);

            if (File.Exists(".\\ExtractorMap.SC2Map"))
                File.Delete(".\\ExtractorMap.SC2Map");

            ObjectBrowserList.Items.Clear();
            ObjectBrowserList.Items.AddRange(MapObjectsManager.ObjectList[ObjectBrowserCatagory.SelectedIndex].ToArray());

            MapObjectsManager.ObjectType selectedItem =
                (MapObjectsManager.ObjectType)
                Enum.Parse(typeof(MapObjectsManager.ObjectType), (string)ObjectBrowserCatagory.SelectedItem);
            for (int i = 0; i < ObjectBrowserList.Columns.Count; i++)
            {
                if (ObjectBrowserList.Items.Count == 0 || selectedItem == MapObjectsManager.ObjectType.Units && i == 1)
                    ObjectBrowserList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                else
                    ObjectBrowserList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private void ObjectBrowserCatagory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectBrowserCatagory.SelectedIndex == -1)
                return;
            ObjectBrowserList.Items.Clear();
            ObjectBrowserList.Columns.Clear();
            MapObjectsManager.ObjectType selectedItem =
                (MapObjectsManager.ObjectType)
                Enum.Parse(typeof (MapObjectsManager.ObjectType), (string) ObjectBrowserCatagory.SelectedItem);
            switch (selectedItem)
            {
                case MapObjectsManager.ObjectType.Units:
                    ObjectBrowserList.Columns.Add("Type");
                    ObjectBrowserList.Columns.Add("Owner");
                    ObjectBrowserList.Columns.Add("ID");
                    ObjectBrowserList.Columns.Add("Position");
                    break;
                case MapObjectsManager.ObjectType.Regions:
                    ObjectBrowserList.Columns.Add("Name", 150);
                    ObjectBrowserList.Columns.Add("ID", 25);
                    break;
                case MapObjectsManager.ObjectType.Points:
                    ObjectBrowserList.Columns.Add("Name", 150);
                    ObjectBrowserList.Columns.Add("Type", 150);
                    ObjectBrowserList.Columns.Add("ID", 25);
                    ObjectBrowserList.Columns.Add("Position", 150);
                    break;
                case MapObjectsManager.ObjectType.Doodads:
                    ObjectBrowserList.Columns.Add("Type", 150);
                    ObjectBrowserList.Columns.Add("ID", 25);
                    ObjectBrowserList.Columns.Add("Position", 150);
                    break;
                case MapObjectsManager.ObjectType.Cameras:
                    ObjectBrowserList.Columns.Add("Name", 150);
                    ObjectBrowserList.Columns.Add("ID", 25);
                    ObjectBrowserList.Columns.Add("Position", 150);
                    break;
            }
            ObjectBrowserList.Items.AddRange(MapObjectsManager.ObjectList[ObjectBrowserCatagory.SelectedIndex].ToArray());
            for (int i = 0; i < ObjectBrowserList.Columns.Count; i++)
            {
                if (ObjectBrowserList.Items.Count == 0 || selectedItem == MapObjectsManager.ObjectType.Units && i == 1)
                    ObjectBrowserList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                else
                    ObjectBrowserList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private void ObjectBrowserList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CurrentOpenFile == null)
                return;

            foreach (MapObjectsManager.MapObject selectedItem in ObjectBrowserList.SelectedItems)
            {
                CurrentOpenFile.OpenFile.Editor.InsertAtCaret(selectedItem.InsertText);
                CurrentOpenFile.OpenFile.Editor.Focus();
                break;
            }
        }

        private void projectView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            editingLabelName = true;
        }

        private void changeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Change_log_form().Show();
        }

        private void TSInsertConstructor_Click(object sender, EventArgs e)
        {
            //Step 1: Find struct/class/enrichment at cursor
            //Step 2: Prompt the user to select the variables to initialize
            //Step 3: Generate and insert text. As close to cursor as possible.

            //Step 1
            StructDescription str = null;
            TextPoint currentPos;
            MyEditor editor = null;

            if (CurrentOpenFile == null)
            {
                foreach (DialogItem file in GetDialogsFiles(openProjectSrcDir))
                {
                    //bool isDesigner = file.OpenFileData.DesignerTabPage == tabStrip.SelectedItem;
                    if (file.OpenFileData != null &&
                        (file.OpenFileData.CodeTabPage == tabStrip.SelectedItem))
                    {
                        editor = file.OpenFileData.CodeEditor;
                        currentPos = file.OpenFileData.CodeEditor.caret.GetPosition(true);
                        foreach (SourceFileContents srcFile in compiler.ParsedSourceFiles)
                        {
                            if (srcFile.Item == file && !srcFile.IsDialogDesigner)
                            {
                                foreach (StructDescription s in srcFile.GetAllStructs())
                                {
                                    if (s.LineFrom - 1 <= currentPos.Line && s.LineTo - 1 >= currentPos.Line)
                                    {
                                        str = s;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                editor = CurrentOpenFile.OpenFile.Editor;
                currentPos = CurrentOpenFile.OpenFile.Editor.caret.GetPosition(true);
                foreach (SourceFileContents file in compiler.ParsedSourceFiles)
                {
                    if (file.Item == CurrentOpenFile)
                    {
                        foreach (StructDescription s in file.GetAllStructs())
                        {
                            if (s.LineFrom - 1 <= currentPos.Line && s.LineTo - 1 >= currentPos.Line)
                            {
                                str = s;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (str == null)
            {
                MessageBox.Show(this, "Unable to find a struct or class at the cursor.");
                return;
            }

            //Step 2
            List<VariableDescription> variables = new List<VariableDescription>();
            variables.AddRange(str.Fields);
            TextPoint lastFieldPos = new TextPoint(0, 0);
            foreach (VariableDescription variable in variables)
            {
                if (variable.Line > lastFieldPos.Line)
                    lastFieldPos.Line = variable.Line;
            }
            NewConstructorForm dialog = new NewConstructorForm(variables);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            variables = dialog.SelectedOrder;

            //Step 3
            string insertHead = "\n" + str.Name + "(";
            string insertBody = "{\n";
            bool isFirst = true;
            foreach (VariableDescription variable in variables)
            {
                if (!isFirst)
                    insertHead += ", ";
                else
                    isFirst = false;
                string paramName = variable.Name;
                paramName = char.ToLower(paramName[0]) + paramName.Remove(0, 1);

                insertHead += variable.Type + " " + paramName;
                if (paramName == variable.Name)
                    insertBody += "this->";
                insertBody += variable.Name + " = " + paramName + ";\n";
            }
            string insert = insertHead + ")\n" + insertBody + "}\n";

            //Insert after the last field

            editor.InsertAndStyle(lastFieldPos, insert);
        }

        private void mapObjectBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetObjectBrowserVisible(!Options.General.ViewObjectBrowser);
        }

        private void SetObjectBrowserVisible(bool visible)
        {
            mapObjectBrowserToolStripMenuItem.Checked =
                Options.General.ViewObjectBrowser = visible;

            if (visible)
            {
                splitContainer2.Panel1.Controls.Remove(tabStrip);
                objectBrowserSplitContainer.Panel1.Controls.Add(tabStrip);
                splitContainer2.Panel1.Controls.Add(objectBrowserSplitContainer);
            }
            else
            {
                splitContainer2.Panel1.Controls.Remove(objectBrowserSplitContainer);
                objectBrowserSplitContainer.Panel1.Controls.Remove(tabStrip);
                splitContainer2.Panel1.Controls.Add(tabStrip);
            }
        }


        private void CBShowErrors_CheckedChanged(object sender, EventArgs e)
        {
            Options.General.ShowErrors = CBShowErrors.Checked;
            ErrorsUpdated();
        }

        private void CBShowWarnings_CheckedChanged(object sender, EventArgs e)
        {
            Options.General.ShowWarnings = CBShowWarnings.Checked;
            ErrorsUpdated();
        }

        public void ErrorsUpdated()
        {
            messageView.Nodes.Clear();
            if (compiler.currentErrorCollection != null)
            {
                foreach (ErrorCollection.Error error in compiler.currentErrorCollection.Errors)
                {
                    if (error.Warning)
                    {
                        if (!Options.General.ShowWarnings) continue;
                    }
                    else
                    {
                        if (!Options.General.ShowErrors) continue;
                    }
                    messageView.Nodes.Add(error);
                }
            }
        }

        private void createAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new CreateAccountForm().ShowDialog(this);
        }

       public static bool IsFilenameValid(string s)
       {
           try
           {
               new FileInfo(s);
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       private void uploadLibraryToolStripMenuItem_Click(object sender, EventArgs e)
       {
           new UploadLibraryForm().ShowDialog(this);
       }

       private void downloadLibraryToolStripMenuItem_Click(object sender, EventArgs e)
       {
           DownloadLibraryForm form = new DownloadLibraryForm();
           form.ShowDialog(this);
           if (form.SelectedLibrary == null)
               return;

           //If the library is already there, remove the old version


           ProjectProperties.CurrentProjectPropperties.Libraries.Add(form.SelectedLibrary);
           ProjectProperties.CurrentProjectPropperties.Save();

           compiler.AddLibrary(form.SelectedLibrary);
           

           TreeNode node = new TreeNode(form.SelectedLibrary.ToString());
           AddNodes(form.SelectedLibrary.Items, node);
           librariesNode.Nodes.Add(node);
           node.Tag = form.SelectedLibrary;
           node.SelectedImageIndex = node.ImageIndex = 4;
       }

       private void DownloadDependancies(Library library)
       {
            
       }

       private void manageAccountToolStripMenuItem_Click(object sender, EventArgs e)
       {
           new ManageUserForm().ShowDialog(this);
       }

       private void reportErrorToolStripMenuItem_Click(object sender, EventArgs e)
       {
           new ExceptionForm(new Exception("Custom Error")).ShowDialog(this);
       }


       private void messageView_KeyDown(object sender, KeyEventArgs e)
       {
           if (e.KeyCode == Keys.C && e.Control && !e.Shift && !e.Alt)
           {
               ErrorCollection.Error error = (ErrorCollection.Error) messageView.SelectedNode;
               if (error == null)
                   return;
               string s = error.ToPrettyString();
               Clipboard.SetText(s);
               e.Handled = true;
               e.SuppressKeyPress = true;
           }
       }

       class UnitFilter
       {
           public static UnitFilter Instance;

           static UnitFilter()
           {
               Instance = new UnitFilter();
               Reset();
           }

           private UnitFilter() {}

           internal enum Values
           {
               Allowed,
               Required,
               Excluded
           }

           public static void Reset()
           {
               for (int i = 0; i < Instance.values.Length; i++)
               {
                   Instance.values[i] = Values.Allowed;
               }
               Instance.values[8] = Values.Excluded;
               Instance.values[16] = Values.Excluded;
               Instance.values[23] = Values.Excluded;
           }

           private Values[] values = new Values[40];

           [DisplayName("Air"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Air { get { return values[0]; } set { values[0] = value; } }
           [DisplayName("Armored"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Armored { get { return values[1]; } set { values[1] = value; } }
           [DisplayName("Benign"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Benign { get { return values[2]; } set { values[2] = value; } }
           [DisplayName("Biological"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Biological { get { return values[3]; } set { values[3] = value; } }
           [DisplayName("Buried"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Buried { get { return values[4]; } set { values[4] = value; } }
           [DisplayName("Can Have Energy"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values CanHaveEnergy { get { return values[5]; } set { values[5] = value; } }
           [DisplayName("Can Have Shields"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values CanHaveShields { get { return values[6]; } set { values[6] = value; } }
           [DisplayName("Cloaked"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Cloaked { get { return values[7]; } set { values[7] = value; } }
           [DisplayName("Dead"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Excluded")]
           public Values Dead { get { return values[8]; } set { values[8] = value; } }
           [DisplayName("Destructible"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Destructible { get { return values[9]; } set { values[9] = value; } }
           [DisplayName("Detector"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Detector { get { return values[10]; } set { values[10] = value; } }
           [DisplayName("Ground"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Ground { get { return values[11]; } set { values[11] = value; } }
           [DisplayName("Hallucination"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Hallucination { get { return values[12]; } set { values[12] = value; } }
           [DisplayName("Has Energy"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values HasEnergy { get { return values[13]; } set { values[13] = value; } }
           [DisplayName("Has Shields"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values HasShields { get { return values[14]; } set { values[14] = value; } }
           [DisplayName("Heroic"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Heroic { get { return values[15]; } set { values[15] = value; } }
           [DisplayName("Hidden"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Excluded")]
           public Values Hidden { get { return values[16]; } set { values[16] = value; } }
           [DisplayName("Hovor"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Hovor { get { return values[17]; } set { values[17] = value; } }
           [DisplayName("Invulnerable"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Invulnerable { get { return values[18]; } set { values[18] = value; } }
           [DisplayName("Item"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Item { get { return values[19]; } set { values[19] = value; } }
           [DisplayName("Light"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Light { get { return values[20]; } set { values[20] = value; } }
           [DisplayName("Massive"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Massive { get { return values[21]; } set { values[21] = value; } }
           [DisplayName("Mechanical"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Mechanical { get { return values[22]; } set { values[22] = value; } }
           [DisplayName("Missile"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Excluded")]
           public Values Missile { get { return values[23]; } set { values[23] = value; } }
           [DisplayName("Passive"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Passive { get { return values[24]; } set { values[24] = value; } }
           [DisplayName("Prevent Defeat"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values PreventDefeat { get { return values[25]; } set { values[25] = value; } }
           [DisplayName("Prevent Reveal"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values PreventReveal { get { return values[26]; } set { values[26] = value; } }
           [DisplayName("Psionic"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Psionic { get { return values[27]; } set { values[27] = value; } }
           [DisplayName("Radar"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Radar { get { return values[28]; } set { values[28] = value; } }
           [DisplayName("Resource (Harvestable)"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values ResourceHarvestable { get { return values[29]; } set { values[29] = value; } }
           [DisplayName("Resource (Raw)"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values ResourceRaw { get { return values[30]; } set { values[30] = value; } }
           [DisplayName("Revivable"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Revivable { get { return values[31]; } set { values[31] = value; } }
           [DisplayName("Robotic"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Robotic { get { return values[32]; } set { values[32] = value; } }
           [DisplayName("Self"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Self { get { return values[33]; } set { values[33] = value; } }
           [DisplayName("Statis"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Statis { get { return values[34]; } set { values[34] = value; } }
           [DisplayName("Structure"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Structure { get { return values[35]; } set { values[35] = value; } }
           [DisplayName("Uncommandable"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Uncommandable { get { return values[36]; } set { values[36] = value; } }
           [DisplayName("Under Construction"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values UnderConstruction { get { return values[37]; } set { values[37] = value; } }
           [DisplayName("Visible"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Visible { get { return values[38]; } set { values[38] = value; } }
           [DisplayName("Worker"),
            CategoryAttribute("Unit Filters"),
            DefaultValue(typeof(Values), "Allowed")]
           public Values Worker { get { return values[39]; } set { values[39] = value; } }


           public string ToString(bool compressed)
           {
               StringBuilder builder = new StringBuilder("UnitFilter(");
               /*2
                (1 << (c_targetFilterUnderConstruction - 32))
                (1 << (c_targetFilterDead - 32))
                (1 << (c_targetFilterRevivable - 32))
                (1 << (c_targetFilterHidden - 32))
                (1 << (c_targetFilterHallucination - 32))
                (1 << (c_targetFilterInvulnerable - 32))
                (1 << (c_targetFilterHasEnergy - 32))
                (1 << (c_targetFilterHasShields - 32))
                (1 << (c_targetFilterBenign - 32))
                (1 << (c_targetFilterPassive - 32))
                (1 << (c_targetFilterDetector - 32))
                (1 << (c_targetFilterRadar - 32))
                */
               int[] secondPriorityFilters = new int[]{37, 8, 31, 16, 12, 18, 13, 14, 2, 24, 10, 28};
               string[] constantNames = new string[40]
                                            {
                                                "c_targetFilterAir",
                                                "c_targetFilterArmored",
                                                "c_targetFilterBenign",
                                                "c_targetFilterBiological",
                                                "c_targetFilterBuried",
                                                "c_targetFilterCanHaveEnergy",
                                                "c_targetFilterCanHaveShields",
                                                "c_targetFilterCloaked",
                                                "c_targetFilterDead",
                                                "c_targetFilterDestructible",
                                                "c_targetFilterDetector",
                                                "c_targetFilterGround",
                                                "c_targetFilterHallucination",
                                                "c_targetFilterHasEnergy",
                                                "c_targetFilterHasShields",
                                                "c_targetFilterHeroic",
                                                "c_targetFilterHidden",
                                                "c_targetFilterHover",
                                                "c_targetFilterInvulnerable",
                                                "c_targetFilterItem",
                                                "c_targetFilterLight",
                                                "c_targetFilterMassive",
                                                "c_targetFilterMechanical",
                                                "c_targetFilterMissile",
                                                "c_targetFilterPassive",
                                                "c_targetFilterPreventDefeat",
                                                "c_targetFilterPreventReveal",
                                                "c_targetFilterPsionic",
                                                "c_targetFilterRadar",
                                                "c_targetFilterHarvestableResource",
                                                "c_targetFilterRawResource",
                                                "c_targetFilterRevivable",
                                                "c_targetFilterRobotic",
                                                "c_targetFilterSelf",
                                                "c_targetFilterStasis",
                                                "c_targetFilterStructure",
                                                "c_targetFilterUncommandable",
                                                "c_targetFilterUnderConstruction",
                                                "c_targetFilterVisible",
                                                "c_targetFilterWorker"
                                            };

               List<string> variables = new List<string>();

               for (int i = 0; i < values.Length; i++)
               {
                   if (!secondPriorityFilters.Contains(i) &&
                       values[i] == Values.Required)
                       variables.Add(constantNames[i]);
               }
               builder.Append(MakeParameter(variables, compressed, false));
               builder.Append(", ");
               variables.Clear();
               for (int i = 0; i < values.Length; i++)
               {
                   if (secondPriorityFilters.Contains(i) &&
                       values[i] == Values.Required)
                       variables.Add(constantNames[i]);
               }
               builder.Append(MakeParameter(variables, compressed, true));
               builder.Append(", ");

               variables.Clear();
               for (int i = 0; i < values.Length; i++)
               {
                   if (!secondPriorityFilters.Contains(i) &&
                       values[i] == Values.Excluded)
                       variables.Add(constantNames[i]);
               }
               builder.Append(MakeParameter(variables, compressed, false));
               builder.Append(", ");
               variables.Clear();
               for (int i = 0; i < values.Length; i++)
               {
                   if (secondPriorityFilters.Contains(i) &&
                       values[i] == Values.Excluded)
                       variables.Add(constantNames[i]);
               }
               builder.Append(MakeParameter(variables, compressed, true));
               builder.Append(")");


               return builder.ToString();
           }

           private string MakeParameter(List<string> variables, bool compressed, bool second)
           {
               if (variables.Count == 0)
                   return "0";
               int compressedInt = 0;
               string text = "";
               foreach (string variable in variables)
               {
                   if (compressed)
                   {
                       AFieldDecl field = null;
                       foreach (AFieldDecl f in Form.compiler.libraryData.Fields)
                       {
                           if (f.GetName().Text == variable)
                           {
                               field = f;
                               break;
                           }
                       }
                       if (field != null)
                       {
                           AIntConstExp exp = (AIntConstExp) field.GetInit();
                           int value = int.Parse(exp.GetIntegerLiteral().Text);
                           if (second)
                               value -= 32;
                           value = 1 << value;
                           compressedInt |= value;
                           continue;
                       }
                   }
                   if (text != "")
                       text += " | ";
                   text += "(1 << ";
                   if (second)
                       text += "(" + variable + " - 32))";
                   else
                       text += variable + ")";
               }
               if (compressed)
               {
                   if (text != "")
                       text += " | ";
                   text += compressedInt;
               }
               return text;
           }
       }

       private void BTNUnitFilterInsert_Click(object sender, EventArgs e)
       {
           if (CurrentOpenFile == null)
               return;

           CurrentOpenFile.OpenFile.Editor.InsertAtCaret(UnitFilter.Instance.ToString(CBUnitFilterCompress.Checked));
           CurrentOpenFile.OpenFile.Editor.Focus();
       }

       private void BTNUnitFilterReset_Click(object sender, EventArgs e)
       {
           UnitFilter.Reset();
           UnitFilterGrid.Refresh();
       }

       private void ObjectBrowserList_SelectedIndexChanged(object sender, EventArgs e)
       {

       }

       private void goToDeclarationToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (CurrentOpenFile != null)
           {
               CurrentOpenFile.OpenFile.Editor.GoToDeclaration();
           }
       }

        private void readTriggerFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

           // TriggerLoader.loadTriggerFile("Triggers");
        }

        private void reparseStarCraftFunctions(object sender, EventArgs e)
       {
           if (Options.General.SC2Exe == null || !StarCraftExecutableFinder.checkPathValidity(Options.General.SC2Exe.FullName))
           {
               FileInfo info = new FileInfo(StarCraftExecutableFinder.findExecutable());
               if (info.Exists)
               {
                   Options.General.SC2Exe = info;
               }
           }

           compiler.ParseAndSaveLibrary();
       }


       

      

      
    }
}
