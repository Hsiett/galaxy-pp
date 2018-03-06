using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases;
using Galaxy_Editor_2.Compiler.Phases.Transformations;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;
using Galaxy_Editor_2.Dialog_Creator;
using Galaxy_Editor_2.Editor_control;
using SharedClasses;

namespace Galaxy_Editor_2.Compiler
{
    class GalaxyCompiler
    {
        public delegate void SourceFileContentsChangedEventHandeler(SourceFileContents file);

        public event SourceFileContentsChangedEventHandeler SourceFileContentsChanged;

        public DirectoryInfo ProjectDir { private get; set; }

        private bool disposed;
        private List<KeyValuePair<SourceFileContents, MyEditor>> liteCompileQueue = new List<KeyValuePair<SourceFileContents, MyEditor>>();
        private List<Library> liteCompileLibraryQueue = new List<Library>();
        public List<SourceFileContents> ParsedSourceFiles = new List<SourceFileContents>();
        private Semaphore liteCompileSemaphore = new Semaphore(0, 1);
        public LibraryData libraryData = new LibraryData();
        public AAProgram latestQuckSnapshot = new AAProgram();
        private Form1 form;

        public GalaxyCompiler()
        {
            
        }

        public GalaxyCompiler(Form1 form)
        {
            this.form = form;
            Thread liteCompileThread = new Thread(CompileLoop);
            liteCompileThread.Start();
        }

        private void CompileLoop()
        {
            while (!disposed)
            {
                try
                {

                    while (!liteCompileSemaphore.WaitOne(5000))
                    {
                        if (Form1.Form.Disposed) return;
                    }
                    if (disposed) return;

                    while (liteCompileQueue.Count > 0 || liteCompileLibraryQueue.Count > 0)
                    {
                        List<string> sources = new List<string>();
                        List<SourceFileContents> contentList = new List<SourceFileContents>();
                        if (liteCompileQueue.Count > 0)
                        {
                            SourceFileContents contents = liteCompileQueue[0].Key;
                            if (liteCompileQueue[0].Value == null)
                            {
                                /*StreamReader reader = contents.File.File.OpenText();
                                sources.Add(reader.ReadToEnd());
                                reader.Close();*/
                                sources.Add(contents.GetSource());
                            }
                            else
                                sources.Add(liteCompileQueue[0].Value.Text);
                            if (disposed)
                                return;
                            liteCompileQueue.RemoveAt(0);
                            contentList.Add(contents);
                        }
                        else
                        {
                            Library lib = liteCompileLibraryQueue[0];
                            liteCompileLibraryQueue.RemoveAt(0);
                            AddSourceFiles(lib.Items, sources, contentList);
                            foreach (SourceFileContents contents in contentList)
                            {
                                contents.Library = lib;
                            }
                        }

                        while (sources.Count > 0)
                        {
                            string source = sources.Last();
                            sources.RemoveAt(sources.Count - 1);
                            SourceFileContents contents = contentList.Last();
                            contentList.RemoveAt(contentList.Count - 1);

                            //Parse + weeder
                            Parser parser = new Parser(new Lexer(new StringReader(source)));
                            Start ast;
                            try
                            {
                                ast = parser.Parse();
                                SimpleTransformations.Parse(ast);
                            }
                            catch (Exception err)
                            {
                                //Critical erros
                                continue;
                            }
                            bool hadChanges = contents.Parse(ast, this);
                            hadChanges |= ConstantFolder.Fold(this);
                            if (hadChanges)
                            {
                                FixStructBaseRefferences();
                                //MakeQuickSnapshot();
                                if (SourceFileContentsChanged != null)
                                    SourceFileContentsChanged(contents);
                            }
                        }
                    }

                }
                catch (Exception err)
                {
                    Program.ErrorHandeler(this, new ThreadExceptionEventArgs(err));
                }
            }
        }

        void AddSourceFiles(List<Library.Item> items, List<string> sources, List<SourceFileContents> contentList)
        {
            foreach (Library.Item item in items)
            {
                if (item is Library.File)
                {
                    sources.Add(((Library.File)item).Text);
                    SourceFileContents c = new SourceFileContents();
                    ParsedSourceFiles.Add(c);
                    contentList.Add(c);
                }
                else
                    AddSourceFiles(((Library.Folder)item).Items, sources, contentList);
            }
        }

        /*void MakeQuickSnapshot()
        {
            AAProgram localLatestQuckSnapshot = new AAProgram();
            foreach (
                    FileItem sourceFile in Form1.GetSourceFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder))
            {
                if (sourceFile.Deactivated)
                    continue;
                StreamReader reader = sourceFile.File.OpenText();
                Parser parser = new Parser(new Lexer(reader));
                string filename = sourceFile.File.FullName;
                //Remove c:/.../projectDir/src
                filename = filename.Remove(0, (ProjectDir.FullName + "/src/").Length);
                //Remove .galaxy++
                filename = filename.Remove(filename.Length - ".galaxy++".Length);
                try
                {
                    Start start = parser.Parse();
                    AASourceFile sourceNode = (AASourceFile)start.GetPSourceFile();
                    reader.Close();
                    reader = sourceFile.File.OpenText();

                    sourceNode.SetName(new TIdentifier(filename));
                    localLatestQuckSnapshot.GetSourceFiles().Add(start.GetPSourceFile());


                }
                catch (ParserException err)
                {
                }
                reader.Close();
            }
            //latestQuckSnapshot.Apply(new Weeder(new ErrorCollection(), new SharedData()));
            localLatestQuckSnapshot.Apply(new QuickSnapShotRemoveJunk());
            latestQuckSnapshot = localLatestQuckSnapshot;
        }

        private class QuickSnapShotRemoveJunk : DepthFirstAdapter
        {
            public override void InAFieldDecl(AFieldDecl node)
            {
                node.SetInit(null);
                node.SetConst(null);
            }

            public override void InATriggerDecl(ATriggerDecl node)
            {
                node.SetEvents(null);
                node.SetConditions(null);

                AABlock block = (AABlock) node.GetActions();
                if (block != null)
                {
                    block.GetStatements().Clear();
                    block.GetStatements().Add(new AValueReturnStm(new TReturn("return"),
                                                                  new ABooleanConstExp(new ATrueBool())));
                }
            }

            public override void InAConstructorDecl(AConstructorDecl node)
            {
                AABlock block = (AABlock)node.GetBlock();
                if (block != null)
                {
                    block.GetStatements().Clear();
                }
            }

            public override void InAInitializerDecl(AInitializerDecl node)
            {
                AABlock block = (AABlock) node.GetBody();
                if (block != null)
                {
                    block.GetStatements().Clear();
                }
            }

            private int paramNr;
            public override void InAMethodDecl(AMethodDecl node)
            {
                AABlock block = (AABlock) node.GetBlock();
                if (block != null)
                {
                    block.GetStatements().Clear();
                    if (!(node.GetReturnType() is AVoidType))
                    {
                        block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"),
                                                                    new AALocalDecl(new APublicVisibilityModifier(),
                                                                                    null, null, null, null,
                                                                                    (PType) node.GetReturnType().Clone(),
                                                                                    new TIdentifier("returner"), null)));
                        block.GetStatements().Add(new AValueReturnStm(new TReturn("return"),
                                                                      new ALvalueExp(
                                                                          new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier("returner"))))));
                    }
                }
                paramNr = 0;
            }

            public override void InAALocalDecl(AALocalDecl node)
            {
                node.SetInit(null);
                node.SetConst(null);
                node.SetOut(null);
                node.SetRef(null);
                if (node.Parent() is AMethodDecl)
                {
                    node.SetName(new TIdentifier("param" + paramNr++));
                }
            }

            public override void InAPropertyDecl(APropertyDecl node)
            {
                AABlock block = (AABlock) node.GetGetter();
                if (block != null)
                {
                    block.GetStatements().Clear();
                    if (!(node.GetType() is AVoidType))
                    {
                        block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"),
                                                                    new AALocalDecl(new APublicVisibilityModifier(),
                                                                                    null, null, null, null,
                                                                                    (PType) node.GetType().Clone(),
                                                                                    new TIdentifier("returner"), null)));
                        block.GetStatements().Add(new AValueReturnStm(new TReturn("return"),
                                                                      new ALvalueExp(
                                                                          new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier("returner"))))));
                    }
                }
                block = (AABlock) node.GetSetter();
                if (block != null)
                {
                    block.GetStatements().Clear();
                }
            }

        }*/

        private void FixStructBaseRefferences()
        {
            SourceFileContents[] srcFileArray = ParsedSourceFiles.ToArray();
            foreach (SourceFileContents file1 in srcFileArray)
            {
                foreach (IDeclContainer declContainer in file1.GetFileDecls())
                {
                    foreach (StructDescription str1 in declContainer.Structs)
                    {
                        if (str1.BaseRef != null)
                        {
                            List<string> names = ((AAName) str1.BaseRef.GetName()).ToStringList();
                            string name = names[names.Count - 1];
                            names.RemoveAt(names.Count - 1);
                            List<IDeclContainer> visibleDecls = file1.GetVisibleDecls(declContainer.NamespaceList, names);
                            foreach (IDeclContainer visibleDecl in visibleDecls)
                            {
                                foreach (StructDescription str2 in visibleDecl.Structs)
                                {
                                    if (str2.Name == name)
                                    {
                                        str1.Base = str2;
                                        break;
                                    }
                                }
                                if (str1.Base != null)
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void SignalLiteCompiler()
        {
            try
            {
                liteCompileSemaphore.Release();
            }
            catch
            {
            }
        }


        public SourceFileContents LookupFile(string name)
        {
            //First look for it in project dir
            string modifiedName = name;
            if (!modifiedName.EndsWith(".galaxy") && !modifiedName.EndsWith(".galaxy++"))
                modifiedName += ".galaxy++";
            foreach (SourceFileContents sourceFile in ParsedSourceFiles)
            {
                if (sourceFile.Item != null && new FileInfo(ProjectDir.FullName + "\\src\\" + modifiedName).FullName == sourceFile.Item.FullName)
                    return sourceFile;
            }
            //Look in starcraft library (implement it first)


            //Not found
            return null;
        }

        public void AddSourceFiles(List<FileItem> list)
        {
            foreach (FileItem fileItem in list)
            {
                AddSourceFile(fileItem);
            }
        }

        public void AddDialogItems(List<DialogItem> list)
        {
            foreach (DialogItem fileItem in list)
            {
                AddDialogFile(fileItem);
            }
        }

        public void AddLibrary(Library lib)
        {
            liteCompileLibraryQueue.Add(lib);
            SignalLiteCompiler();
        }

        public void RemoveLibrary(Library lib)
        {
            for (int i = 0; i < ParsedSourceFiles.Count; i++)
            {
                if (ParsedSourceFiles[i].Library == lib)
                {
                    ParsedSourceFiles.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < liteCompileLibraryQueue.Count; i++)
            {
                if (liteCompileLibraryQueue[i] == lib)
                {
                    liteCompileLibraryQueue.RemoveAt(i);
                    i--;
                }
            }
        }

        public void AddSourceFile(FileItem file, MyEditor text = null)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            //Check if it already exists
            foreach (SourceFileContents sourceFile in ParsedSourceFiles)
            {
                if (sourceFile.Item == file)
                    return;
            }
            SourceFileContents contents = new SourceFileContents();
            contents.Item = file;
            contents.GetSource = new ExtractSourceFileCode(file).Extract;
            ParsedSourceFiles.Add(contents);
            liteCompileQueue.Add(new KeyValuePair<SourceFileContents, MyEditor>(contents, text));
            SourceFileContentsChanged(contents);
            SignalLiteCompiler();
        }

        private class ExtractSourceFileCode
        {
            private FileItem item;

            public ExtractSourceFileCode(FileItem item)
            {
                this.item = item;
            }

            public string Extract()
            {
                StreamReader reader = item.File.OpenText();
                string ret = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                return ret;
            }
        }
        
        public void AddDialogFile(DialogItem item, MyEditor text = null)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            //Check if it already exists
            foreach (SourceFileContents sourceFile in ParsedSourceFiles)
            {
                if (sourceFile.Item == item)
                    return;
            }
            SourceFileContents contents = new SourceFileContents();
            contents.Item = item;
            contents.GetSource = new ExtractDialogItemCode(item, false).Extract;
            ParsedSourceFiles.Add(contents);
            liteCompileQueue.Add(new KeyValuePair<SourceFileContents, MyEditor>(contents, text));
            SourceFileContentsChanged(contents);

            contents = new SourceFileContents();
            contents.Item = item;
            contents.IsDialogDesigner = true;
            contents.GetSource = new ExtractDialogItemCode(item, true).Extract;
            ParsedSourceFiles.Add(contents);
            liteCompileQueue.Add(new KeyValuePair<SourceFileContents, MyEditor>(contents, text));
            SourceFileContentsChanged(contents);
            SignalLiteCompiler();
        }

        private class ExtractDialogItemCode
        {
            private DialogItem item;
            private bool ExtractDesignerCode;

            public ExtractDialogItemCode(DialogItem item, bool extractDesignerCode)
            {
                this.item = item;
                ExtractDesignerCode = extractDesignerCode;
            }

            public string Extract()
            {
                DialogData data = item.OpenFileData ?? DialogData.Load(item.FullName);
                data.DialogItem = item;
                if (ExtractDesignerCode)
                    return data.DesignerCode;
                return data.ActualCode;
            }
        }

        public void RemoveSourceFile(FileItem file)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            for (int i = 0; i < ParsedSourceFiles.Count; i++)
            {
                if (ParsedSourceFiles[i].Item == file)
                {
                    ParsedSourceFiles.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < liteCompileQueue.Count; i++)
            {
                if (liteCompileQueue[i].Key.Item == file)
                {
                    liteCompileQueue.RemoveAt(i);
                    i--;
                }
            }
        }

        public void RemoveDialogItem(DialogItem file)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            for (int i = 0; i < ParsedSourceFiles.Count; i++)
            {
                if (ParsedSourceFiles[i].Item == file)
                {
                    ParsedSourceFiles.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < liteCompileQueue.Count; i++)
            {
                if (liteCompileQueue[i].Key.Item == file)
                {
                    liteCompileQueue.RemoveAt(i);
                    i--;
                }
            }
        }

        public void RemoveAllFiles()
        {
            liteCompileQueue.Clear();
            liteCompileLibraryQueue.Clear();
            ParsedSourceFiles.Clear();
        }

        /*public void SourceFileMoved(FileInfo oldFile, FileInfo newFile)
        {
            foreach (SourceFileContents t in ParsedSourceFiles)
            {
                if (t.File == oldFile)
                {
                    t.File = newFile;
                }
            }
        }*/

        public void SourceFileChanged(FileItem file, MyEditor contents)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            //If its already in the compile queue, update the text
            for (int i = 0; i < liteCompileQueue.Count; i++)
            {
                if (liteCompileQueue[i].Key.Item == file)
                {
                    liteCompileQueue[i] = new KeyValuePair<SourceFileContents, MyEditor>(liteCompileQueue[i].Key, contents);
                    SignalLiteCompiler();
                    return;
                }
            }
            //Otherwise, add it
            foreach (SourceFileContents t in ParsedSourceFiles)
            {
                if (t.Item == file)
                {
                    liteCompileQueue.Add(new KeyValuePair<SourceFileContents, MyEditor>(t, contents));
                }
            }
            SignalLiteCompiler();
        }

        public void DialogItemChanged(DialogItem file, MyEditor contents, bool isDesigner = false)
        {
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            //If its already in the compile queue, update the text
            for (int i = 0; i < liteCompileQueue.Count; i++)
            {
                if (liteCompileQueue[i].Key.Item == file && liteCompileQueue[i].Key.IsDialogDesigner == isDesigner)
                {
                    liteCompileQueue[i] = new KeyValuePair<SourceFileContents, MyEditor>(liteCompileQueue[i].Key, contents);
                    SignalLiteCompiler();
                    return;
                }
            }
            //Otherwise, add it
            foreach (SourceFileContents t in ParsedSourceFiles)
            {
                if (t.Item == file && t.IsDialogDesigner == isDesigner)
                {
                    liteCompileQueue.Add(new KeyValuePair<SourceFileContents, MyEditor>(t, contents));
                }
            }
            SignalLiteCompiler();
        }

        public bool Compiling { get; private set; }
        private bool compilingFromCommandLine;
        public void Compile(bool fromCommandLine = false)
        {
            if (Compiling)
            {
                MessageBox.Show("The compiler is already working. Close this message box and try again.\n\n" +
                                "If this takes more than a couple of seconds, it's most likely an infinite loop.\n" +
                                "In that case, please report it as an error on sc2mapster, in the relevant thread, or in a private message to me (SBeier).\n" +
                                "Also, to make it easier for me to fix, I would like it if you could attach come code which causes the error.",
                                "Compiler already running");
                return;
            }
            compilingFromCommandLine = fromCommandLine;
            Compiling = true;
            ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.StartedCompile;
            new Thread(CompileThread).Start();
            //CompileThread();
        }

        public delegate void CompilationDoneEventHandler();

        public event CompilationDoneEventHandler CompilationSuccessfull;
        public event CompilationDoneEventHandler CompilationFailed;
        private void CompileThread()
        {
            try
            {
                if (!compilingFromCommandLine)
                    form.SetStatusText("Parsing");

                if (!compilingFromCommandLine)
                    ClearErrorWindow();
                //Build a tree with all sourcefiles
                AAProgram root = new AAProgram();
                ErrorCollection errors = new ErrorCollection();
                currentErrorCollection = errors;
                if (!compilingFromCommandLine)
                    errors.ErrorAdded += errors_ErrorAdded;
                bool addedDeobfuscator = false;
                SharedData sharedData = new SharedData();
                sharedData.AllowPrintouts = !compilingFromCommandLine;
                //Parse project files
                List<string> fileNames = new List<string>();
                List<string> sources = new List<string>();
                foreach (
                    FileItem sourceFile in Form1.GetSourceFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder))
                {
                    if (sourceFile.Deactivated)
                        continue;
                    StreamReader reader = sourceFile.File.OpenText();
                   
                    string filename = sourceFile.File.FullName;
                    //Remove c:/.../projectDir/src
                    filename = filename.Remove(0, (ProjectDir.FullName + "/src/").Length);
                    //Remove .galaxy++
                    filename = filename.Remove(filename.Length - ".galaxy++".Length);
                    fileNames.Add(filename);
                    sources.Add(reader.ReadToEnd());
                    reader.Close();
                    continue;

                    Parser parser = new Parser(new Lexer(reader));
                    try
                    {
                        Start start = parser.Parse();
                        AASourceFile sourceNode = (AASourceFile) start.GetPSourceFile();
                        reader.Close();
                        reader = sourceFile.File.OpenText();
                        int lineCount = 0;
                        while (reader.ReadLine() != null)
                        {
                            lineCount++;
                        }
                        reader.Close();
                        sharedData.LineCounts[sourceNode] = lineCount;

                        //Extract encryption function
                       /* {
                            AASourceFile file = (AASourceFile) start.GetPSourceFile();
                            if (file.GetDecl().Count > 0 && file.GetDecl()[0] is AMethodDecl)
                            {
                                AMethodDecl method = (AMethodDecl) file.GetDecl()[0];
                                if (method.GetName().Text == "Galaxy_pp_Deobfuscate")
                                {
                                    FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                    IFormatter formatter = new BinaryFormatter();
                                    Stream stream = dobfuscateFile.Open(FileMode.Create);
                                    formatter.Serialize(stream, method);
                                    stream.Close();
                                }
                            }
                        }*/

                        if (Options.Compiler.ObfuscateStrings)
                        {
                            HasStringConstExp checker = new HasStringConstExp();
                            start.Apply(checker);

                            if (!addedDeobfuscator /* && checker.HasStringConst*/)
                            {
                                FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                IFormatter formatter = new BinaryFormatter();
                                Stream stream = dobfuscateFile.Open(FileMode.Open);
                                AASourceFile file = (AASourceFile) start.GetPSourceFile();

                                AMethodDecl method = (AMethodDecl) formatter.Deserialize(stream);
                                sharedData.DeobfuscateMethod = method;
                                method.GetName().Line = 0;

                                HasStringConstExp checker2 = new HasStringConstExp();
                                method.Apply(checker2);
                                file.GetDecl().Insert(0, method);
                                stream.Close();

                                addedDeobfuscator = true;


                                foreach (AStringConstExp stringConstExp in checker2.List)
                                {
                                    int line = -sharedData.UnobfuscatedStrings.Count - 1;
                                    AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const", line, 0),
                                                                      new ANamedType(new TIdentifier("string", line, 1),
                                                                                     null),
                                                                      new TIdentifier("Galaxy_pp_stringU" +
                                                                                      sharedData.UnobfuscatedStrings.
                                                                                          Count),
                                                                      null);
                                    //If the strings are the same - point them to same field
                                    bool newField = true;
                                    foreach (AStringConstExp oldStringConstExp in sharedData.UnobfuscatedStrings.Keys)
                                    {
                                        if (stringConstExp.GetStringLiteral().Text ==
                                            oldStringConstExp.GetStringLiteral().Text)
                                        {
                                            field = sharedData.UnobfuscatedStrings[oldStringConstExp];
                                            newField = false;
                                            break;
                                        }
                                    }
                                    if (newField)
                                    {
                                        file.GetDecl().Insert(0, field);
                                        sharedData.ObfuscationFields.Add(field);
                                    }
                                    sharedData.UnobfuscatedStrings.Add(stringConstExp, field);

                                }

                            }
                            foreach (AStringConstExp stringConstExp in checker.List)
                            {
                                int line = -sharedData.ObfuscatedStrings.Count - 1;
                                AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const", line, 0),
                                                                  new ANamedType(new TIdentifier("string", line, 1),
                                                                                 null),
                                                                  new TIdentifier("Galaxy_pp_stringO" +
                                                                                  sharedData.ObfuscatedStrings.Count),
                                                                  null);
                                //If the strings are the same - point them to same field
                                bool newField = true;
                                foreach (AStringConstExp oldStringConstExp in sharedData.ObfuscatedStrings.Keys)
                                {
                                    if (stringConstExp.GetStringLiteral().Text ==
                                        oldStringConstExp.GetStringLiteral().Text)
                                    {
                                        field = sharedData.ObfuscatedStrings[oldStringConstExp];
                                        newField = false;
                                        break;
                                    }
                                }
                                if (newField)
                                {
                                    AASourceFile file = (AASourceFile) sharedData.DeobfuscateMethod.Parent();
                                    file.GetDecl().Insert(file.GetDecl().IndexOf(sharedData.DeobfuscateMethod) + 1,
                                                          field);
                                    sharedData.ObfuscationFields.Add(field);
                                }
                                sharedData.ObfuscatedStrings.Add(stringConstExp, field);
                            }
                        }

                        sourceNode.SetName(new TIdentifier(filename));
                        root.GetSourceFiles().Add(start.GetPSourceFile());


                    }
                    catch (ParserException err)
                    {
                        String errMsg = err.Message;
                        //Remove [...]
                        errMsg = errMsg.Substring(errMsg.IndexOf(']') + 1).TrimStart();
                        errors.Add(new ErrorCollection.Error(err.Token, filename, errMsg));
                    }
                    reader.Close();
                }
                //Parse project dialogs
                foreach (
                    DialogItem dialogItem in Form1.GetDialogsFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder))
                {
                    if (dialogItem.Deactivated)
                        continue;
                   // List<string> fileNames = new List<string>();
                   // List<string> sources = new List<string>();

                    DialogData data;
                    if (dialogItem.OpenFileData != null)
                    {
                        data = dialogItem.OpenFileData;
                        data.Save(dialogItem.FullName);
                    }
                    else
                    {
                        data = DialogData.Load(dialogItem.FullName);
                        data.DialogItem = dialogItem;
                    }

                    string filename = dialogItem.FullName;
                    filename = filename.Remove(0, (ProjectDir.FullName + "/src/").Length);
                    filename = filename.Remove(filename.Length - ".Dialog".Length);

                    fileNames.Add(filename);
                    sources.Add(data.Code ?? "");

                    fileNames.Add(filename + ".Designer");
                    sources.Add(data.DesignerCode ?? "");

                    continue;

                    for (int i = 0; i < fileNames.Count; i++)
                    {
                        filename = fileNames[i];
                        StringReader reader = new StringReader(sources[i] ?? "");
                        Parser parser = new Parser(new Lexer(reader));
                        try
                        {
                            Start start = parser.Parse();
                            AASourceFile sourceNode = (AASourceFile) start.GetPSourceFile();
                            reader.Close();
                            reader.Dispose();
                            reader = new StringReader(sources[i] ?? "");
                            int lineCount = 0;
                            while (reader.ReadLine() != null)
                            {
                                lineCount++;
                            }
                            reader.Close();

                            sharedData.LineCounts[sourceNode] = lineCount;

                            if (Options.Compiler.ObfuscateStrings)
                            {
                                HasStringConstExp checker = new HasStringConstExp();
                                start.Apply(checker);

                                if (!addedDeobfuscator /* && checker.HasStringConst*/)
                                {
                                    FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                    IFormatter formatter = new BinaryFormatter();
                                    Stream stream = dobfuscateFile.Open(FileMode.Open);
                                    AASourceFile file = (AASourceFile) start.GetPSourceFile();

                                    AMethodDecl method = (AMethodDecl) formatter.Deserialize(stream);
                                    sharedData.DeobfuscateMethod = method;
                                    method.GetName().Line = 0;

                                    HasStringConstExp checker2 = new HasStringConstExp();
                                    method.Apply(checker2);
                                    file.GetDecl().Insert(0, method);
                                    stream.Close();

                                    addedDeobfuscator = true;


                                    foreach (AStringConstExp stringConstExp in checker2.List)
                                    {
                                        int line = -sharedData.UnobfuscatedStrings.Count - 1;
                                        AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                          new TConst("const", line, 0),
                                                                          new ANamedType(
                                                                              new TIdentifier("string", line, 1),
                                                                              null),
                                                                          new TIdentifier("Galaxy_pp_stringU" +
                                                                                          sharedData.UnobfuscatedStrings
                                                                                              .
                                                                                              Count),
                                                                          null);
                                        //If the strings are the same - point them to same field
                                        bool newField = true;
                                        foreach (
                                            AStringConstExp oldStringConstExp in sharedData.UnobfuscatedStrings.Keys)
                                        {
                                            if (stringConstExp.GetStringLiteral().Text ==
                                                oldStringConstExp.GetStringLiteral().Text)
                                            {
                                                field = sharedData.UnobfuscatedStrings[oldStringConstExp];
                                                newField = false;
                                                break;
                                            }
                                        }
                                        if (newField)
                                        {
                                            file.GetDecl().Insert(0, field);
                                            sharedData.ObfuscationFields.Add(field);
                                        }
                                        sharedData.UnobfuscatedStrings.Add(stringConstExp, field);

                                    }

                                }
                                foreach (AStringConstExp stringConstExp in checker.List)
                                {
                                    int line = -sharedData.ObfuscatedStrings.Count - 1;
                                    AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                      new TConst("const", line, 0),
                                                                      new ANamedType(new TIdentifier("string", line, 1),
                                                                                     null),
                                                                      new TIdentifier("Galaxy_pp_stringO" +
                                                                                      sharedData.ObfuscatedStrings.Count),
                                                                      null);
                                    //If the strings are the same - point them to same field
                                    bool newField = true;
                                    foreach (AStringConstExp oldStringConstExp in sharedData.ObfuscatedStrings.Keys)
                                    {
                                        if (stringConstExp.GetStringLiteral().Text ==
                                            oldStringConstExp.GetStringLiteral().Text)
                                        {
                                            field = sharedData.ObfuscatedStrings[oldStringConstExp];
                                            newField = false;
                                            break;
                                        }
                                    }
                                    if (newField)
                                    {
                                        AASourceFile file = (AASourceFile) sharedData.DeobfuscateMethod.Parent();
                                        file.GetDecl().Insert(file.GetDecl().IndexOf(sharedData.DeobfuscateMethod) + 1,
                                                              field);
                                        sharedData.ObfuscationFields.Add(field);
                                    }
                                    sharedData.ObfuscatedStrings.Add(stringConstExp, field);
                                }
                            }

                            sourceNode.SetName(new TIdentifier(filename));
                            root.GetSourceFiles().Add(start.GetPSourceFile());


                        }
                        catch (ParserException err)
                        {
                            String errMsg = err.Message;
                            //Remove [...]
                            errMsg = errMsg.Substring(errMsg.IndexOf(']') + 1).TrimStart();
                            errors.Add(new ErrorCollection.Error(err.Token, filename, errMsg));
                        }
                        reader.Close();
                    }
                }
               // Preprocessor.Parse(sources, errors);
                for (int i = 0; i < fileNames.Count; i++)
                {
                    string filename = fileNames[i];
                    StringReader reader = new StringReader(sources[i] ?? "");
                    Parser parser = new Parser(new Lexer(reader));
                    try
                    {
                        Start start = parser.Parse();
                        AASourceFile sourceNode = (AASourceFile)start.GetPSourceFile();
                        reader.Close();
                        reader.Dispose();
                        reader = new StringReader(sources[i] ?? "");
                        int lineCount = 0;
                        while (reader.ReadLine() != null)
                        {
                            lineCount++;
                        }
                        reader.Close();

                        sharedData.LineCounts[sourceNode] = lineCount;

                        //Extract encryption function
                         /*{
                             AASourceFile file = (AASourceFile) start.GetPSourceFile();
                             if (file.GetDecl().Count > 0 && file.GetDecl()[0] is AMethodDecl)
                             {
                                 AMethodDecl method = (AMethodDecl) file.GetDecl()[0];
                                 if (method.GetName().Text == "Galaxy_pp_Deobfuscate")
                                 {
                                     FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                     IFormatter formatter = new BinaryFormatter();
                                     Stream stream = dobfuscateFile.Open(FileMode.Create);
                                     formatter.Serialize(stream, method);
                                     stream.Close();
                                 }
                             }
                         }*/

                        if (Options.Compiler.ObfuscateStrings)
                        {
                            HasStringConstExp checker = new HasStringConstExp();
                            start.Apply(checker);

                            if (!addedDeobfuscator /* && checker.HasStringConst*/)
                            {
                                FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                IFormatter formatter = new BinaryFormatter();
                                Stream stream = dobfuscateFile.Open(FileMode.Open);
                                AASourceFile file = (AASourceFile)start.GetPSourceFile();

                                AMethodDecl method = (AMethodDecl)formatter.Deserialize(stream);
                                sharedData.DeobfuscateMethod = method;
                                method.GetName().Line = 0;

                                HasStringConstExp checker2 = new HasStringConstExp();
                                method.Apply(checker2);
                                file.GetDecl().Insert(0, method);
                                stream.Close();

                                addedDeobfuscator = true;


                                foreach (AStringConstExp stringConstExp in checker2.List)
                                {
                                    int line = -sharedData.UnobfuscatedStrings.Count - 1;
                                    AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                      new TConst("const", line, 0),
                                                                      new ANamedType(
                                                                          new TIdentifier("string", line, 1),
                                                                          null),
                                                                      new TIdentifier("Galaxy_pp_stringU" +
                                                                                      sharedData.UnobfuscatedStrings
                                                                                          .
                                                                                          Count),
                                                                      null);
                                    //If the strings are the same - point them to same field
                                    bool newField = true;
                                    foreach (
                                        AStringConstExp oldStringConstExp in sharedData.UnobfuscatedStrings.Keys)
                                    {
                                        if (stringConstExp.GetStringLiteral().Text ==
                                            oldStringConstExp.GetStringLiteral().Text)
                                        {
                                            field = sharedData.UnobfuscatedStrings[oldStringConstExp];
                                            newField = false;
                                            break;
                                        }
                                    }
                                    if (newField)
                                    {
                                        file.GetDecl().Insert(0, field);
                                        sharedData.ObfuscationFields.Add(field);
                                    }
                                    sharedData.UnobfuscatedStrings.Add(stringConstExp, field);

                                }

                            }
                            foreach (AStringConstExp stringConstExp in checker.List)
                            {
                                int line = -sharedData.ObfuscatedStrings.Count - 1;
                                AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                  new TConst("const", line, 0),
                                                                  new ANamedType(new TIdentifier("string", line, 1),
                                                                                 null),
                                                                  new TIdentifier("Galaxy_pp_stringO" +
                                                                                  sharedData.ObfuscatedStrings.Count),
                                                                  null);
                                //If the strings are the same - point them to same field
                                bool newField = true;
                                foreach (AStringConstExp oldStringConstExp in sharedData.ObfuscatedStrings.Keys)
                                {
                                    if (stringConstExp.GetStringLiteral().Text ==
                                        oldStringConstExp.GetStringLiteral().Text)
                                    {
                                        field = sharedData.ObfuscatedStrings[oldStringConstExp];
                                        newField = false;
                                        break;
                                    }
                                }
                                if (newField)
                                {
                                    AASourceFile file = (AASourceFile)sharedData.DeobfuscateMethod.Parent();
                                    file.GetDecl().Insert(file.GetDecl().IndexOf(sharedData.DeobfuscateMethod) + 1,
                                                          field);
                                    sharedData.ObfuscationFields.Add(field);
                                }
                                sharedData.ObfuscatedStrings.Add(stringConstExp, field);
                            }
                        }

                        sourceNode.SetName(new TIdentifier(filename));
                        root.GetSourceFiles().Add(start.GetPSourceFile());


                    }
                    catch (ParserException err)
                    {
                        String errMsg = err.Message;
                        //Remove [...]
                        errMsg = errMsg.Substring(errMsg.IndexOf(']') + 1).TrimStart();
                        errors.Add(new ErrorCollection.Error(err.Token, filename, errMsg));
                    }
                    reader.Close();
                }



                //Load libraries
                foreach (Library lib in ProjectProperties.CurrentProjectPropperties.Libraries)
                {
                    foreach (KeyValuePair<Library.File, string> sourceFile in lib.GetFiles())
                    {
                        StringReader sReader = new StringReader(sourceFile.Key.Text);
                        {
                            Parser parser = new Parser(new Lexer(sReader));

                            try
                            {
                                Start start = parser.Parse();
                                AASourceFile sourceNode = (AASourceFile) start.GetPSourceFile();
                                sReader.Close();
                                sReader.Dispose();
                                sReader = new StringReader(sourceFile.Key.Text);
                                int lineCount = 0;
                                while (sReader.ReadLine() != null)
                                {
                                    lineCount++;
                                }
                                sReader.Close();
                                sReader.Dispose();
                                sharedData.LineCounts[sourceNode] = lineCount;

                                

                                if (Options.Compiler.ObfuscateStrings)
                                {
                                    HasStringConstExp checker = new HasStringConstExp();
                                    start.Apply(checker);

                                    if (!addedDeobfuscator /* && checker.HasStringConst*/)
                                    {
                                        FileInfo dobfuscateFile = new FileInfo("Deobfuscator.LibraryData");
                                        IFormatter formatter = new BinaryFormatter();
                                        Stream stream = dobfuscateFile.Open(FileMode.Open);
                                        AASourceFile file = (AASourceFile) start.GetPSourceFile();

                                        AMethodDecl method = (AMethodDecl) formatter.Deserialize(stream);
                                        sharedData.DeobfuscateMethod = method;
                                        method.GetName().Line = 0;

                                        HasStringConstExp checker2 = new HasStringConstExp();
                                        method.Apply(checker2);
                                        file.GetDecl().Insert(0, method);
                                        stream.Close();

                                        addedDeobfuscator = true;


                                        foreach (AStringConstExp stringConstExp in checker2.List)
                                        {
                                            int line = -sharedData.UnobfuscatedStrings.Count - 1;
                                            AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                              new TConst("const", line, 0),
                                                                              new ANamedType(
                                                                                  new TIdentifier("string", line, 1),
                                                                                  null),
                                                                              new TIdentifier("Galaxy_pp_stringU" +
                                                                                              sharedData.
                                                                                                  UnobfuscatedStrings
                                                                                                  .
                                                                                                  Count),
                                                                              null);
                                            //If the strings are the same - point them to same field
                                            bool newField = true;
                                            foreach (
                                                AStringConstExp oldStringConstExp in sharedData.UnobfuscatedStrings.Keys
                                                )
                                            {
                                                if (stringConstExp.GetStringLiteral().Text ==
                                                    oldStringConstExp.GetStringLiteral().Text)
                                                {
                                                    field = sharedData.UnobfuscatedStrings[oldStringConstExp];
                                                    newField = false;
                                                    break;
                                                }
                                            }
                                            if (newField)
                                            {
                                                file.GetDecl().Insert(0, field);
                                                sharedData.ObfuscationFields.Add(field);
                                            }
                                            sharedData.UnobfuscatedStrings.Add(stringConstExp, field);

                                        }

                                    }
                                    foreach (AStringConstExp stringConstExp in checker.List)
                                    {
                                        int line = -sharedData.ObfuscatedStrings.Count - 1;
                                        AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null,
                                                                          new TConst("const", line, 0),
                                                                          new ANamedType(
                                                                              new TIdentifier("string", line, 1),
                                                                              null),
                                                                          new TIdentifier("Galaxy_pp_stringO" +
                                                                                          sharedData.ObfuscatedStrings.
                                                                                              Count),
                                                                          null);
                                        //If the strings are the same - point them to same field
                                        bool newField = true;
                                        foreach (AStringConstExp oldStringConstExp in sharedData.ObfuscatedStrings.Keys)
                                        {
                                            if (stringConstExp.GetStringLiteral().Text ==
                                                oldStringConstExp.GetStringLiteral().Text)
                                            {
                                                field = sharedData.ObfuscatedStrings[oldStringConstExp];
                                                newField = false;
                                                break;
                                            }
                                        }
                                        if (newField)
                                        {
                                            AASourceFile file = (AASourceFile) sharedData.DeobfuscateMethod.Parent();
                                            file.GetDecl().Insert(
                                                file.GetDecl().IndexOf(sharedData.DeobfuscateMethod) + 1,
                                                field);
                                            sharedData.ObfuscationFields.Add(field);
                                        }
                                        sharedData.ObfuscatedStrings.Add(stringConstExp, field);
                                    }
                                }

                                sourceNode.SetName(new TIdentifier(sourceFile.Value));
                                root.GetSourceFiles().Add(start.GetPSourceFile());


                            }
                            catch (ParserException err)
                            {
                                String errMsg = err.Message;
                                //Remove [...]
                                errMsg = errMsg.Substring(errMsg.IndexOf(']') + 1).TrimStart();
                                errors.Add(new ErrorCollection.Error(err.Token, sourceFile.Value, errMsg));
                            }
                        }
                    }
                }

                string rootFileName = "";
                DirectoryInfo outputDir = ProjectDir.CreateSubdirectory("output");
                try
                {
                    if (!compilingFromCommandLine)
                        form.SetStatusText("Weeding");
                    sharedData.Libraries = libraryData;
                    Weeder.Parse(root, errors, sharedData);

                    if (!compilingFromCommandLine)
                        form.SetStatusText("Building enviroment");
                    if (!errors.HasErrors) EnviromentBuilding.Parse(root, errors, sharedData);

                    if (!compilingFromCommandLine)
                        form.SetStatusText("Checking enviroment");
                    if (!errors.HasErrors) EnviromentChecking.Parse(root, errors, sharedData);

                    if (!compilingFromCommandLine)
                        form.SetStatusText("Linking types");
                    if (!errors.HasErrors) root.Apply(new LinkNamedTypes(errors, sharedData));


                    if (!compilingFromCommandLine)
                        form.SetStatusText("Fixing generics");
                    if (!errors.HasErrors) root.Apply(new FixGenerics(errors, sharedData));


                    if (!compilingFromCommandLine)
                        form.SetStatusText("Fixing enheritance");
                    if (!errors.HasErrors) root.Apply(new Enheritance(sharedData, errors));

                    if (!compilingFromCommandLine)
                        form.SetStatusText("Linking usages to declarations");
                    if (!errors.HasErrors) TypeLinking.Parse(root, errors, sharedData);

                    if (!compilingFromCommandLine)
                        form.SetStatusText("Checking types");
                    if (!errors.HasErrors) TypeChecking.Parse(root, errors, sharedData);
                    if (!errors.HasErrors) root.Apply(new MakeEnrichmentLinks(sharedData, errors));
                    if (!errors.HasErrors) root.Apply(new SetArrayIndexes(sharedData, errors));
                    if (!compilingFromCommandLine)
                        form.SetStatusText("Transforming code");
                    if (!errors.HasErrors) FinalTransformations.Parse(root, errors, sharedData, out rootFileName);
                    if (!compilingFromCommandLine)
                        form.SetStatusText("Generating code");
                    if (!errors.HasErrors) CodeGeneration.Parse(root, errors, sharedData, outputDir);
                    if (!errors.HasErrors) GenerateBankPreloadFile.Generate(sharedData, outputDir);
                    //if (!errors.HasErrors) TriggerLoader.loadTriggerFile("Triggers");
                }
                catch (ParserException err)
                {

                }

                Compiling = false;
                if (!errors.HasErrors)
                {
                    if (!compilingFromCommandLine)
                        form.SetStatusText("Compilation finished successfully");
                    ProjectProperties.CurrentProjectPropperties.RootFileName = rootFileName;
                    ProjectProperties.CurrentProjectPropperties.CompileStatus =
                        ProjectProperties.ECompileStatus.SuccessfullyCompiled;
                    if (CompilationSuccessfull != null)
                        CompilationSuccessfull();
                }
                else
                {
                    if (!compilingFromCommandLine)
                        form.SetStatusText("Compilation terminated with errors");
                    if (CompilationFailed != null)
                        CompilationFailed();
                }

            }
#if DEBUG
            finally
            {
                
            }
#else
            catch (Exception error)
            {
                Compiling = false;
                //Program.ErrorHandeler(this, new ThreadExceptionEventArgs(error));
                new ExceptionForm(error, true).ShowDialog();
                form.SetStatusText("Critical compile error");
                if (CompilationFailed != null)
                    CompilationFailed();
            }
#endif
        }

        public ErrorCollection currentErrorCollection;

        private void errors_ErrorAdded(ErrorCollection sender, ErrorCollection.Error error)
        {
            if (form.messageView.InvokeRequired)
            {
                form.messageView.Invoke(new ErrorCollection.ErrorAddedEventHandler(errors_ErrorAdded), sender, error);
                return;
            }
            if (error.Warning)
            {
                if (!Options.General.ShowWarnings) return;
            }
            else
            {
                if (!Options.General.ShowErrors) return;
            }
            form.messageView.Nodes.Add(error);
        }

        private delegate void ClearErrorWindowDelegate();
        private void ClearErrorWindow()
        {
            if (form.messageView.InvokeRequired)
            {
                form.messageView.Invoke(new ClearErrorWindowDelegate(ClearErrorWindow));
                return;
            }
            form.messageView.Nodes.Clear();
        }


        public void CompileLibrary(DirectoryInfo libraryDir, StreamWriter writer)
        {
            AAProgram root = new AAProgram();
            ErrorCollection errors = new ErrorCollection();

            foreach (FileInfo file in GetSourceFiles(libraryDir))
            {
                //Replace keywords
                StreamReader reader = file.OpenText();
                StringBuilder text = new StringBuilder("");
                string[] keywords = new[] {"ref", "out", "InvokeSync", "InvokeAsync",
                                                  "switch", "case", "default", "new", "delete", "this","delegate", "value", "base",
                "inline", "namespace", "using",
                                                                           "Trigger", "Initializer",
                                                                           "events", "conditions", "actions", "class",
                                                                           "typedef", "get", "set", "enrich", 
                                                                           "public", "private", "protected",
                "LibraryName", "LibraryVersion", "SupportedVersions", "RequiredLibraries", "global"};

                int i;
                StringBuilder currentIdentifier = new StringBuilder("");
                while ((i = reader.Read()) != -1)
                {
                    if (Util.IsIdentifierLetter((char)i))
                    {
                        currentIdentifier.Append((char)i);
                    }
                    else
                    {
                        if (currentIdentifier.Length > 0)
                        {
                            string identifier = currentIdentifier.ToString();
                            currentIdentifier.Clear();
                            if (keywords.Contains(identifier))
                            {
                                identifier = "_" + identifier;
                            }
                            text.Append(identifier);
                        }
                        text.Append((char) i);
                    }
                }

                Parser parser = new Parser(new Lexer(new StringReader(text.ToString())));
                try
                {
                    Start start = parser.Parse();
                    AASourceFile srcFile = (AASourceFile) start.GetPSourceFile();
                    srcFile.SetName(new TIdentifier(file.FullName.Substring(libraryDir.FullName.Length + 1)));
                    root.GetSourceFiles().Add(srcFile);
                }
                catch (ParserException err)
                {
                    errors.Add(new ErrorCollection.Error(err.Token, file.Name, err.Message, false));
                }
                reader.Close();
            }

            try
            {
                Weeder.Parse(root, errors, new SharedData());
                LibraryData lib = new LibraryData(root, writer);
                FileInfo precompFile = new FileInfo(libraryDir.FullName + "\\Precompiled.LibraryData");
                IFormatter formatter = new BinaryFormatter();
                Stream stream = precompFile.Open(FileMode.Create);
                formatter.Serialize(stream, lib);
                stream.Close();
            }
            catch (Exception err)
            {

            }
            if (errors.HasErrors)
                MessageBox.Show("Errors in libray " + libraryDir.Name);

            
        }

        public void ParseAndSaveLibrary()
        {
            if (Options.General.SC2Exe == null || !StarCraftExecutableFinder.checkPathValidity(Options.General.SC2Exe.FullName))
            {
                FileInfo info = new FileInfo(StarCraftExecutableFinder.findExecutable());
                if (info.Exists)
                {
                    Options.General.SC2Exe = info;
                }
            }

            //since 3.0 mod are not placed in mod folders, I can only use the old onces to compile.
            LibraryData newLib = TriggerLoader.AddBaseLib(FunctionExtractor.LoadFunctions());// FunctionExtractor.LoadFunctions();

           
            if (newLib.Methods.Count() > 0)
            {
                // It can happen that nothing can be parsed when the editor is open since it locks the mpq files
                // If there are methods then something has been parsed
                try
                {
                    FileInfo precompFile = new FileInfo("Precompiled.LibraryData");
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = precompFile.Open(FileMode.Create);
                    formatter.Serialize(stream, newLib);
                    stream.Close();

                    libraryData = newLib;
                }
                catch (Exception err)
                {

                }
            }
            else
            {
                MessageBox.Show(form, "Could not parse StarCraft MPQs. Make sure that the editor is closed when doing this.", "Error");
            }
        }

        public void LoadLibraries()
        {
            LibraryData lib = new LibraryData();
            FileInfo precompFile = new FileInfo("Precompiled.LibraryData");
            if (!precompFile.Exists)
            {
                MessageBox.Show(form, "Unable to load library. File not found.", "Error");
                return;
            }
            IFormatter formatter = new BinaryFormatter();
            Stream stream = precompFile.OpenRead();
            try
            {
                lib.Join((LibraryData)formatter.Deserialize(stream));
                stream.Close();
            }
            catch (Exception err)
            {
                stream.Close();

                MessageBox.Show(form, "Error parsing library.", "Error");
                return;
            }
            libraryData = lib;
        }

        public void LoadLibraries(List<DirectoryInfo> libraries)
        {
            LibraryData lib = new LibraryData();

            StreamWriter writer = new StreamWriter(new FileInfo("outputList.txt").Open(FileMode.Create, FileAccess.Write));
            foreach (DirectoryInfo library in libraries)
            {
            retry:
                FileInfo precompFile = new FileInfo(library.FullName + "\\Precompiled.LibraryData");
                /*if (!precompFile.Exists)*/
                CompileLibrary(library, writer);
                IFormatter formatter = new BinaryFormatter();
                Stream stream = precompFile.OpenRead();
                try
                {
                    lib.Join((LibraryData) formatter.Deserialize(stream));
                    stream.Close();
                }
                catch (Exception err)
                {
                    stream.Close();
                    precompFile.Delete();
                    goto retry;
                }
            }
            libraryData = lib;

            {
                List<AMethodDecl> newMethods = new List<AMethodDecl>();
                List<AFieldDecl> newFields = new List<AFieldDecl>();
                XmlTextReader reader = new XmlTextReader(new FileInfo("Galaxy.xml").Open(FileMode.Open, FileAccess.Read));

                

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.Name == "KeyWord")
                    {
                        if (reader.GetAttribute("func") == null)
                        {
                            AFieldDecl fieldDecl = new AFieldDecl(new APublicVisibilityModifier(), null, null, null, new TIdentifier(reader.GetAttribute("name")), null);
                            newFields.Add(fieldDecl);
                            continue;
                        }
                        AMethodDecl methodDecl = new AMethodDecl();
                        methodDecl.SetName(new TIdentifier(reader.GetAttribute("name")));
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                            if (reader.NodeType != XmlNodeType.Element)
                                continue;
                            if (reader.Name != "Param")
                                continue;
                            string type = reader.GetAttribute("name");
                            type = type.Substring(0, type.IndexOf(" "));
                            string name = reader.GetAttribute("name");
                            name = name.Substring(name.IndexOf(" ") + 1);

                            methodDecl.GetFormals().Add(new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                        new ANamedType(new TIdentifier(type), null),
                                                                        new TIdentifier(name), null));

                        }
                        if (reader.EOF)
                            break;
                        newMethods.Add(methodDecl);
                    }
                }

                reader.Close();

                List<AMethodDecl> oldMethods = new List<AMethodDecl>();
                oldMethods.AddRange(libraryData.Methods);
                List<AFieldDecl> oldFields = new List<AFieldDecl>();
                oldFields.AddRange(libraryData.Fields);


                //Remove dublicates in old
                for (int i = 0; i < oldMethods.Count; i++)
                {
                    for (int j = i + 1; j < oldMethods.Count; j++)
                    {
                        if (oldMethods[i].GetName().Text == oldMethods[j].GetName().Text)
                        {
                            oldMethods.RemoveAt(j);
                            j--;
                        }
                    }
                }

                for (int i = 0; i < oldFields.Count; i++)
                {
                    for (int j = i + 1; j < oldFields.Count; j++)
                    {
                        if (oldFields[i].GetName().Text == oldFields[j].GetName().Text)
                        {
                            oldFields.RemoveAt(j);
                            j--;
                        }
                    }
                }

                //Remove dublicates in new
                for (int i = 0; i < newMethods.Count; i++)
                {
                    for (int j = i + 1; j < newMethods.Count; j++)
                    {
                        if (newMethods[i].GetName().Text == newMethods[j].GetName().Text)
                        {
                            newMethods.RemoveAt(j);
                            j--;
                        }
                    }
                }

                for (int i = 0; i < newFields.Count; i++)
                {
                    for (int j = i + 1; j < newFields.Count; j++)
                    {
                        if (newFields[i].GetName().Text == newFields[j].GetName().Text)
                        {
                            newFields.RemoveAt(j);
                            j--;
                        }
                    }
                }


                //Remove stuff they agree on
                for (int i = 0; i < newFields.Count; i++)
                {
                    for (int j = 0; j < oldFields.Count; j++)
                    {
                        if (newFields[i].GetName().Text == oldFields[j].GetName().Text)
                        {
                            newFields.RemoveAt(i);
                            oldFields.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                for (int j = 0; j < oldFields.Count; j++)
                {
                    if (oldFields[j].GetStatic() != null)
                    {
                        oldFields.RemoveAt(j);
                        j--;
                    }
                }
                for (int i = 0; i < newMethods.Count; i++)
                {
                    for (int j = 0; j < oldMethods.Count; j++)
                    {
                        if (newMethods[i].GetName().Text == oldMethods[j].GetName().Text)
                        {
                            newMethods.RemoveAt(i);
                            oldMethods.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                for (int j = 0; j < oldMethods.Count; j++)
                {
                    if (oldMethods[j].GetStatic() != null ||
                        (oldMethods[j].GetNative() == null && oldMethods[j].GetBlock() == null))
                    {
                        oldMethods.RemoveAt(j);
                        j--;
                    }
                }

            }

            {
                /*StreamWriter writer = new StreamWriter(new FileInfo("outputList.txt").Open(FileMode.Create, FileAccess.Write));
                foreach (AMethodDecl method in libraryData.Methods)
                {
                    string str = "native " + TypeToString(method.GetReturnType()) + " " + method.GetName().Text +
                                 "(";
                    bool first = true;
                    foreach (AALocalDecl formal in method.GetFormals())
                    {
                        if (!first)
                            str += ", ";
                        str += TypeToString(formal.GetType()) + " " + formal.GetName().Text;
                        first = false;
                    }
                    str += ");";

                    writer.WriteLine(str);
                }

                foreach (AFieldDecl field in libraryData.Fields)
                {
                    if (field.GetName().Text == "libNtve_gv__GameUIVisible")
                        writer = writer;
                    writer.WriteLine(TypeToString(field.GetType()) + " " + field.GetName().Text + ";");
                }*/
                writer.Flush();
                writer.Close();
            }
        }

        private string TypeToString(PType type)
        {
            if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType)type;
                return TypeToString(aType.GetType()) + "[" +
                        ((AIntConstExp)aType.GetDimention()).GetIntegerLiteral().Text + "]";
            }
            return Util.TypeToString(type);
        }

        private List<FileInfo> GetSourceFiles(DirectoryInfo dir)
        {
            List<FileInfo> returner = new List<FileInfo>();
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name.EndsWith(".galaxy") ||file.Name.EndsWith(".galaxy++"))
                    returner.Add(file);
            }
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                returner.AddRange(GetSourceFiles(directory));
            }
            return returner;
        }

        public void Dispose()
        {
            disposed = true;
            SignalLiteCompiler();
        }
    }
}
