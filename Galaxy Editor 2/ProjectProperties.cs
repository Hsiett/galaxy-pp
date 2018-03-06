using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Dialog_Creator;
using SharedClasses;

namespace Galaxy_Editor_2
{
    [Serializable]
    class ProjectProperties
    {

        public static DirectoryInfo SetProject(DirectoryInfo dir)
        {
            if (dir == null)
            {
                CurrentProjectPropperties = null;
                return null;
            }

            CurrentProjectPropperties = new ProjectProperties(dir);
            dir = CurrentProjectPropperties.projectDir;
            FileInfo file = new FileInfo(dir.FullName + "\\properties.dat");
            if (file.Exists)
            {
                Stream stream = file.OpenRead();
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    CurrentProjectPropperties = (ProjectProperties) formatter.Deserialize(stream);
                    if (CurrentProjectPropperties.projectDir.FullName != dir.FullName)
                    {
                        CurrentProjectPropperties.projectDir = dir;
                        //CurrentProjectPropperties.srcFolder = null;
                        //CurrentProjectPropperties.outputFolder = null;
                    }
                    if (CurrentProjectPropperties.srcFolder != null && CurrentProjectPropperties.srcFolder.Name == null)
                    {
                        CurrentProjectPropperties.srcFolder = new FolderItem(null, "src");
                        CurrentProjectPropperties.outputFolder = new FolderItem(null, "output");
                    }
                    if (CurrentProjectPropperties.Libraries == null)
                        CurrentProjectPropperties.Libraries = new List<Library>();
                }
                catch (Exception err)
                {
                }
                finally
                {
                    stream.Close();
                }
            }
            CurrentProjectPropperties.SrcFolder.FixConflicts(".galaxy++");
            CurrentProjectPropperties.OutputFolder.FixConflicts(".galaxy");
            return dir;
        }

        public static ProjectProperties CurrentProjectPropperties { get; private set; }

        //---------------------------------------------------------------------------------------

        public string lastLibName, lastLibVersion, lastLibDescription, lastLibChangeLog;
        private DirectoryInfo projectDir;
        public List<Library> Libraries = new List<Library>();

        public string ProjectDir { get { return projectDir.FullName; } }

        public ProjectProperties(DirectoryInfo projectDir)
        {
            string relocateDir = null;
            foreach (FileInfo fileInfo in projectDir.GetFiles("Relocate.txt"))
            {
                if (fileInfo.Name == "Relocate.txt")
                {
                    StreamReader reader = fileInfo.OpenText();
                    relocateDir = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                    break;
                }
            }
            if (relocateDir != null)
                projectDir = new DirectoryInfo(relocateDir);
            this.projectDir = projectDir;
            
        }

        internal void Save()
        {
            FileInfo file = new FileInfo(projectDir.FullName + "\\properties.dat");
            Stream stream = file.Open(FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
        }

        private string mapPath = "";
        public string MapPath
        {
            get { return mapPath; }
            set
            {
                if (mapPath != value)
                {
                    mapPath = value;
                    Save();
                }
            }
        }

        public bool InputMapIsMod
        {
            get { return (mapPath ?? "").ToLower().EndsWith(".sc2mod"); }
        }

        public FileSystemInfo InputMap
        {
            get
            {
                if (File.Exists(mapPath))
                    return new FileInfo(mapPath);
                if (Directory.Exists(mapPath))
                    return new DirectoryInfo(mapPath);
                return null;
            }
        }

        private string outputMapPath = "";
        public string OutputMapPath
        {
            get { return outputMapPath; }
            set
            {
                if (outputMapPath != value)
                {
                    outputMapPath = value;
                    Save();
                }
            }
        }



        public bool OutputMapIsMod
        {
            get { return (outputMapPath ?? "").ToLower().EndsWith(".sc2mod"); }
        }

        public bool IsMod
        {
            get { return InputMapIsMod || OutputMapIsMod; }
        }


        private bool isOutputDirectory = false;
        public bool IsOutputDirectory
        {
            get { return isOutputDirectory; }
            set
            {
                if (isOutputDirectory != value)
                {
                    isOutputDirectory = value;
                    Save();
                }
            }
        }

        private bool loadSaveScriptToMap = false;
        public bool LoadSaveScriptToMap
        {
            get { return loadSaveScriptToMap; }
            set
            {
                if (loadSaveScriptToMap != value)
                {
                    loadSaveScriptToMap = value;
                    Save();
                }
            }
        }

        private FolderItem srcFolder;
        public FolderItem SrcFolder
        {
            get
            {
                if (srcFolder == null)
                    srcFolder = new FolderItem(null, "src");
                return srcFolder;
            }
            set
            {
                if (srcFolder != value)
                {
                    srcFolder = value;
                    Save();
                }
            }
        }

        private FolderItem outputFolder;
        public FolderItem OutputFolder
        {
            get
            {
                if (outputFolder == null)
                    outputFolder = new FolderItem(null, "output");
                return outputFolder;
            }
            set
            {
                if (outputFolder != value)
                {
                    outputFolder = value;
                    Save();
                }
            }
        }


        public enum ECompileStatus
        {
            Changed,
            StartedCompile,
            SuccessfullyCompiled
        }


        [NonSerialized]
        public ECompileStatus CompileStatus = ECompileStatus.Changed;
        

        private string rootFileName = "";
        public string RootFileName
        {
            get
            {
                return rootFileName;
            }
            set
            {
                if (rootFileName != value)
                {
                    rootFileName = value;
                    Save();
                }
            }
        }
    }

    [Serializable]
    abstract class DirItem
    {
        public abstract string Text { get; }

        public FolderItem Parent;
        [NonSerialized]
        public TreeNode GUINode;

        protected DirItem(FolderItem p)
        {
            Parent = p;
        }

        public abstract string FullName { get; }

        public delegate void Renamed(DirItem sender, string oldName, string newName);
        [field: NonSerialized]
        public event Renamed OnRenamed;
        protected void InvokeRenamed(DirItem sender, string oldName, string newName)
        {
            if (OnRenamed != null)
                OnRenamed(sender, oldName, newName);
            ProjectProperties.CurrentProjectPropperties.Save();
        }
        public abstract bool Rename(string newName);

        public delegate void Moved(DirItem mover, FolderItem oldParent, FolderItem newParent);
        [field: NonSerialized]
        public event Moved OnMoved;
        protected void InvokeMoved(DirItem mover, FolderItem oldParent, FolderItem newParent)
        {
            if (OnMoved != null)
                OnMoved(mover, oldParent, newParent);
            ProjectProperties.CurrentProjectPropperties.Save();
        }
        public abstract bool MoveTo(FolderItem target, int index);

        public abstract bool FixConflicts(params string[] reqPrefixes);

        public bool IsDecendantOf(FolderItem folder)
        {
            if (this == folder)
                return true;
            if (Parent == null)
                return false;
            return Parent.IsDecendantOf(folder);
        }

        private delegate void GUINodeRemove();
        virtual public void Delete()
        {

            if (Parent != null)
            {
                Form1.Form.RecursivlyCloseOpenFiles(this);
                Parent.Children.Remove(this);
                //Parent = null;
                Form1.Form.Invoke(new GUINodeRemove(GUINode.Remove));
                //GUINode.Remove();
                ProjectProperties.CurrentProjectPropperties.Save();
            }
        }

    }

    [Serializable]
    class FolderItem : DirItem
    {
        public DirectoryInfo Dir
        {
            get
            {
                return new DirectoryInfo(Path.Combine(Parent == null
                                                          ? ProjectProperties.CurrentProjectPropperties.ProjectDir
                                                          : Parent.Dir.FullName,
                                                      name));
            }
        }
        public List<DirItem> Children = new List<DirItem>();
        public bool Expanded = true;
        //[NonSerialized] 
        //private FileSystemWatcher watcher;
        private string name;
        public string Name { get { return name; } }
        

        public FolderItem(FolderItem p, string name) : base(p)
        {
            //Dir = dir;
            this.name = name;
            if (!Dir.Exists)
                Dir.Create();
            /*watcher = new FileSystemWatcher(dir.FullName, ".galaxy++");
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
            watcher.EnableRaisingEvents = true;*/
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Find the FileItem
                foreach (FileItem child in Children.OfType<FileItem>())
                {
                    if (child.Text == e.Name)
                    {
                        //Delete it
                        child.Delete();
                        return;
                    }
                }
            }
            catch (Exception err)
            {

            }
        }

        public override void Delete()
        {
            /*watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;*/

            foreach (DirItem child in Children.OfType<DirItem>().ToArray())
            {
                child.Delete();
            }

            //Dir.Delete(true);

            Form1.DeleteDir(Dir.FullName);

            base.Delete();
        }

        public override string Text
        {
            get { return Dir == null ? "" : Dir.Name; }
        }

        public override string FullName
        {
            get { return Dir.FullName; }
        }

        public override bool Rename(string newName)
        {
            //Header folder - dont rename
            if (Parent == null)
                return false;

            //Dont reneme if there is a clash
            if (Dir.Parent.GetDirectories().Any(dir => dir.Name == newName))
                return false;

            string oldName = name;
            if (oldName.ToLower() == newName.ToLower())
            {//Just changeing case. Rename in two goes
                int i = 1;
                string n = Path.Combine(Dir.Parent.FullName, oldName + i);
                while (Directory.Exists(n))
                {
                    i++;
                    n = Path.Combine(Dir.Parent.FullName, oldName + i);
                }
                Dir.MoveTo(n);
                Directory.Move(n, Dir.Parent.FullName + "\\" + newName);
            }
            else
                Dir.MoveTo(Dir.Parent.FullName + "\\" + newName);
            name = newName;

            InvokeRenamed(this, oldName, newName);
            return true;
        }

        public override bool MoveTo(FolderItem targetParent, int index)
        {
            //Dont move if it is the main folder
            if (Parent == null)
                return false;

            //Dont move if target is a child of current
            {
                FolderItem t = targetParent;
                while (t != null)
                {
                    if (t == this)
                        return false;
                    t = t.Parent;
                }
            }

            //Dont move if a dir of same name is there
            if (targetParent.Dir.GetDirectories().Any(dir => dir.FullName.TrimEnd('\\', '/') != Dir.FullName.TrimEnd('\\', '/') && dir.Name == Dir.Name))
                return false;

            if (targetParent != Parent)
                Dir.MoveTo(targetParent.Dir.FullName + "\\" + Dir.Name);

            FolderItem oldParent = Parent;
            Parent = targetParent;

            oldParent.Children.Remove(this);
            if (index >= Parent.Children.Count)
                Parent.Children.Add(this);
            else
                Parent.Children.Insert(index, this);

            InvokeMoved(this, oldParent, targetParent);

            return true;
        }

        public override bool FixConflicts(params string[] reqPrefixes)
        {
            /*Dir = new DirectoryInfo(Dir.FullName);
            foreach (DirItem child in Children)
            {
                if (child is FolderItem)
                {
                    FolderItem folder = (FolderItem) child;
                    folder.Dir = new DirectoryInfo(folder.Dir.FullName);
                }
                if (child is FileItem)
                {
                    FileItem folder = (FileItem)child;
                    folder.File = new FileInfo(folder.File.FullName);
                }
            }
            */
            //If this item does not exist on the hdd, remove it.
            if (!Dir.Exists)
            {
                if (Parent == null)
                    Dir.Create();
                else
                {
                    Parent.Children.Remove(this);
                    return true;
                }
            }

            /*if (watcher == null)
            {
                watcher = new FileSystemWatcher(Dir.FullName, "*.galaxy++");
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
                watcher.EnableRaisingEvents = true;
            }*/

            //If this dir contains any dirs or files that is not on the list, add them
            foreach (FileSystemInfo info in Dir.GetFileSystemInfos())
            {
                if (info is FileInfo)
                {
                    FileInfo fileInfo = (FileInfo) info;

                    if (!reqPrefixes.Any(prefix => fileInfo.Name.EndsWith(prefix)))
                        continue;



                    if (!Children.Any(child => child.FullName == fileInfo.FullName))
                    {
                        if (fileInfo.Name.EndsWith(".Dialog"))
                        {
                            DialogItem item = new DialogItem(this);
                            item.Name = fileInfo.Name;
                            Children.Add(item);
                        }
                        else
                        {
                            
                            Children.Add(new FileItem(this, fileInfo.Name));
                        }
                    }
                }
                if (info is DirectoryInfo)
                {
                    DirectoryInfo dirInfo = (DirectoryInfo)info;

                    if (!Children.Any(child => (child is FolderItem) && ((FolderItem)child).Dir.FullName.TrimEnd('\\', '/') == dirInfo.FullName.TrimEnd('\\', '/')))
                        Children.Add(new FolderItem(this, dirInfo.Name));
                }
            }

            

            bool changes = false;
            //Visit children
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                changes |= Children[i].FixConflicts(reqPrefixes);
            }

            return changes;
        }


        internal void Close()
        {
            /*watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;*/
            foreach (DirItem child in Children)
            {
                if (child is FolderItem)
                    ((FolderItem)child).Close();
            }
        }
    }

    [Serializable]
    class FileItem : DirItem
    {

        public FileInfo File { get { return new FileInfo(Path.Combine(Parent.FullName, name)); } }
        [NonSerialized]
        public Form1.OpenFileData OpenFile;
        public bool Deactivated;
        public List<int> ClosedBlocks = new List<int>();
        private string name;
        public string Name { get { return name; } }

        public override string Text
        {
            get { return File == null ? "" : File.Name; }
        }

        public override string FullName
        {
            get { return File.FullName; }
        }


        public FileItem(FolderItem p, string name) : base(p)
        {
            //File = file;
            this.name = name;
            if (!File.Exists)
                File.Create().Close();
        }

        public override void Delete()
        {
            base.Delete();
            Form1.Form.compiler.RemoveSourceFile(this);
            if (File.Exists)
                File.Delete();
        }

        public override bool Rename(string newName)
        {
            //Add .galaxy++ if not present
            if (!newName.EndsWith(".galaxy++"))
                newName += ".galaxy++";


            //Dont reneme if there is a clash
            if (Parent.Dir.GetDirectories().Any(dir => dir.Name == newName))
                return false;

            string oldName = name;
            File.MoveTo(Parent.Dir.FullName + "\\" + newName);
            name = newName;

            InvokeRenamed(this, oldName, newName);

            return true;
        }

        public override bool MoveTo(FolderItem target, int index)
        {
            //Dont move if a file of same name is there
            if (target.Dir.GetFiles().Any(file => file.FullName != File.FullName && file.Name == File.Name))
            {
                return false;
            }

            bool doMove = target != Parent;

            FolderItem oldParent = Parent;

            oldParent.Children.Remove(this);
            if (index >= Parent.Children.Count)
                target.Children.Add(this);
            else
                target.Children.Insert(index, this);

            if (doMove)
                File.MoveTo(target.Dir.FullName + "\\" + File.Name);
            Parent = target;


            InvokeMoved(this, oldParent, target);

            return true;
        }

        public override bool FixConflicts(params string[] reqPrefixes)
        {
            //File = new FileInfo(File.FullName);
            //If this item does not exist on the hdd, remove it.
            if (!File.Exists)
            {
                Parent.Children.Remove(this);
                return true;
            }
            return false;
        }
    }

    [Serializable]
    class DialogItem : DirItem
    {
        public string Name;
        [NonSerialized]
        public DialogData OpenFileData;
        public bool Deactivated;
        public TreeNode CodeGUINode;
        public TreeNode DesignerGUINode;

        public DialogItem(FolderItem p) : base(p)
        {
        }

        public override string Text
        {
            get
            {
                return Name;
            }
        }

        public override void Delete()
        {
            if (File.Exists(FullName))
                File.Delete(FullName);
            base.Delete();
            Form1.Form.compiler.RemoveDialogItem(this);
        }

        public override string FullName
        {
            get { return Path.Combine(Parent.Dir.FullName, Name); }
        }

        public override bool Rename(string newName)
        {
            //Add .galaxy++ if not present
            if (!newName.EndsWith(".Dialog"))
                newName = newName + ".Dialog";

            //Dont reneme if there is a clash
            foreach (FileSystemInfo dir in Parent.Dir.GetFileSystemInfos())
            {
                if (dir.Name == newName + ".Dialog")
                    return false;
            }

            File.Move(Parent.Dir.FullName + "\\" + Name,
                      Parent.Dir.FullName + "\\" + newName);
            string oldName = Name;
            Name = newName;
            InvokeRenamed(this, oldName, newName);

            //Change other stuff
            string shortName = Name.Substring(0, Name.LastIndexOf(".Dialog"));
            CodeGUINode.Text = shortName + (CodeGUINode.Text.EndsWith("*") ? ".galaxy++*" : ".galaxy++");
            DesignerGUINode.Text = shortName + (DesignerGUINode.Text.EndsWith("*") ? ".Designer.galaxy++*" : ".Designer.galaxy++");
            if (OpenFileData != null)
            {
                if (OpenFileData.TabPage != null)
                    OpenFileData.TabPage.Title = Name + (OpenFileData.TabPage.Title.EndsWith("*") ? "*" : "");
                if (OpenFileData.CodeTabPage != null)
                    OpenFileData.CodeTabPage.Title = shortName + (OpenFileData.CodeTabPage.Title.EndsWith("*") ? ".galaxy++*" : ".galaxy++");
                if (OpenFileData.DesignerTabPage != null)
                    OpenFileData.DesignerTabPage.Title = shortName + ".Designer.galaxy++";
            }
            //Fix refferences to dialog
            try
            {
                DialogData data;
                if (OpenFileData != null)
                    data = OpenFileData;
                else
                {
                    data = DialogData.Load(FullName);
                    data.DialogItem = this;
                }
                string code = data.ActualCode;
                Name = oldName;
                string oldIdentifier = data.DialogIdentiferName;
                Name = newName;
                string newIdentifier = data.DialogIdentiferName;
                Parser parser = new Parser(new Lexer(new StringReader(code)));
                AASourceFile start = (AASourceFile)parser.Parse().GetPSourceFile();
                Renamer ren = new Renamer(oldIdentifier);
                start.Apply(ren);
                ren.types.Reverse();//To avoid changeing the position of stuff we must change
                string[] lines = code.Split('\n');
                foreach (TIdentifier identifier in ren.types)
                {
                    lines[identifier.Line - 1] = lines[identifier.Line - 1].Substring(0, identifier.Pos - 1) +
                                                 newIdentifier +
                                                 lines[identifier.Line - 1].Substring(identifier.Pos +
                                                                                      oldIdentifier.Length - 1);
                }
                string newCode = "";
                foreach (string line in lines)
                {
                    newCode += line + "\n";
                }
                newCode = newCode.Remove(code.Length);
                if (code != newCode)
                {
                    if (OpenFileData == null)
                        Form1.Form.OpenFile(this, CodeGUINode);
                    OpenFileData.ActualCode = newCode;
                }
            }
            catch (Exception err)
            {
                //Or not..
            }
            Form1.Form.compiler.DialogItemChanged(this, null, true);
            return true;
        }

        private class Renamer : DepthFirstAdapter
        {
            public List<TIdentifier> types = new List<TIdentifier>();
            private string oldName;

            public Renamer(string oldName)
            {
                this.oldName = oldName;
            }

            public override void CaseANamedType(ANamedType node)
            {
                AAName name = (AAName) node.GetName();
                if (name.GetIdentifier().Count > 2)
                    return;
                if (name.GetIdentifier().Count == 2 && ((TIdentifier)name.GetIdentifier()[0]).Text != "Dialogs")
                    return;
                if (name.GetIdentifier().Count == 1)
                {
                    bool foundDialogs = false;
                    AASourceFile file = Util.GetAncestor<AASourceFile>(node);
                    foreach (AUsingDecl usingDecl in file.GetUsings())
                    {
                        if (usingDecl.GetNamespace().Count == 1)
                        {
                            TIdentifier identifer = (TIdentifier) usingDecl.GetNamespace()[0];
                            if (identifer.Text == "Dialogs")
                            {
                                foundDialogs = true;
                                break;
                            }
                        }
                    }
                    if (!foundDialogs)
                    {
                        ANamespaceDecl ns = Util.GetAncestor<ANamespaceDecl>(node);
                        if (!Util.HasAncestor<ANamespaceDecl>(ns.Parent()) && ns.GetName().Text == "Dialogs")
                        {
                            foundDialogs = true;
                        }
                    }
                    if (!foundDialogs)
                        return;
                }
                if (((TIdentifier)name.GetIdentifier()[name.GetIdentifier().Count - 1]).Text == oldName)
                    types.Add((TIdentifier)name.GetIdentifier()[name.GetIdentifier().Count - 1]);
            }
        }

        public override bool MoveTo(FolderItem target, int index)
        {//Dont move if a file of same name is there
            bool doMove = target != Parent;
            if (doMove && target.Dir.GetFiles().Any(file => file.Name == Name))
            {
                return false;
            }


            FolderItem oldParent = Parent;
            Parent = target;

            oldParent.Children.Remove(this);
            if (index >= Parent.Children.Count)
                Parent.Children.Add(this);
            else
                Parent.Children.Insert(index, this);

            if (doMove)
            {
                File.Move(oldParent.Dir.FullName + "\\" + Name,
                          Parent.Dir.FullName + "\\" + Name);
            }


            InvokeMoved(this, oldParent, target);

            return true;
        }

        public override bool FixConflicts(params string[] reqPrefixes)
        {
            //If this item does not exist on the hdd, remove it.
            if (!File.Exists(FullName))
            {
                Parent.Children.Remove(this);
                return true;
            }
            return false;
        }
    }
}
