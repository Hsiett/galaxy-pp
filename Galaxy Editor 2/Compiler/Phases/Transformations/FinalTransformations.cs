using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.NotGenerated;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class FinalTransformations
    {

        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data, out string rootFile)
        {
            FinalTransformations finalTrans = new FinalTransformations(errors, data);
            finalTrans.Apply(ast);
            AASourceFile rootSrcFile = Util.GetAncestor<AASourceFile>(finalTrans.mainEntry);
            if (rootSrcFile == null)
                rootFile = "";
            else
                rootFile = rootSrcFile.GetName().Text + ".galaxy";
        }

        internal ErrorCollection errors;
        internal SharedData data;
        internal AMethodDecl mainEntry;
        internal bool multipleMainEntries;
        internal ABlockStm mainEntryFieldInitBlock = new ABlockStm(new TLBrace("{"), new AABlock(new ArrayList(), new TRBrace("}")));

        public FinalTransformations(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        private void Apply(AAProgram ast)
        {
            int stage = 1;
            int totalStages = 31;
            
            List<ANamedType> deleteUs = new List<ANamedType>();
            foreach (KeyValuePair<ANamedType, AStructDecl> pair in data.StructTypeLinks)
            {
                ANamedType type = pair.Key;
                AStructDecl str = pair.Value;
                if (data.Enums.ContainsKey(str))
                {
                    type.SetName(new AAName(new ArrayList(){new TIdentifier(data.Enums[str] ? "int" : "byte")}));
                    deleteUs.Add(type);
                }
            }
            foreach (ANamedType type in deleteUs)
            {
                data.StructTypeLinks.Remove(type);
            }


            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removing namespaces");
            stage++;

            ast.Apply(new RemoveNamespaces());

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Moving static members out");
            stage++;

            ast.Apply(new StaticStructMembers(data));


            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removing constant variables");
            stage++;

            ast.Apply(new RemoveConstants(data));



            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Locating main entry");
            stage++;

            ast.Apply(new MainEntryFinder(this));

            

            if (mainEntry == null)
            {
                //errors.Add(new ErrorCollection.Error("No entry point found (void InitMap(){...})", true));

                //Generate main entry
                AASourceFile file =
                    ast.GetSourceFiles().Cast<AASourceFile>().FirstOrDefault(
                        sourceFile => !Util.GetAncestor<AASourceFile>(sourceFile).GetName().Text.Contains("\\"));
                if (file == null)
                {
                    //Make default sourcefile
                    file = new AASourceFile();
                    file.SetName(new TIdentifier("MapScript"));
                    ast.GetSourceFiles().Add(file);
                    data.LineCounts[file] = 1;
                }
                //windywell
                string entryString = "InitMap";// "GalaxyPPInitMap";//
                //
                mainEntry = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")), new TIdentifier(entryString), new ArrayList(), new AABlock());
                file.GetDecl().Add(mainEntry);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, mainEntry));
            }
            else if (Util.GetAncestor<AASourceFile>(mainEntry).GetName().Text.Contains("\\"))
                errors.Add(new ErrorCollection.Error(mainEntry.GetName(), Util.GetAncestor<AASourceFile>(mainEntry), "The source file containing the main entry function should be placed in root folder to be able to overwrite MapScript.galaxy", true));

            ((AABlock)mainEntry.GetBlock()).GetStatements().Insert(0, mainEntryFieldInitBlock);

            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Setting default values for struct variables");
            stage++;
            
            ast.Apply(new StructInitializer(this));
         
            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming expression ifs");
            stage++;

            ast.Apply(new TransformExpressionIfs(data));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Renaming unicode identifiers and strings");
            stage++;

            ast.Apply(new RenameUnicode(data));

            
            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming properties to methods (Phase 1)");
            stage++;

            TransformProperties.Phase1.Parse(this);
            
            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removing dead code");
            stage++;

            ast.Apply(new RemoveDeadCode());

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Moving methods members out of structs");
            stage++;

            ast.Apply(new TransformMethodDecls(this));
            
            
            if (errors.HasErrors)
                return;


            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Invoking initializers");
            stage++;

            

            MakeInitializerInvokes();
            
            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Moving assignments out to their own statement");
            stage++;

            ast.Apply(new AssignFixup(this));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming properties to methods (Phase 2)");
            stage++;

            TransformProperties.Phase2.Parse(this);



            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Fixing invokes (sync/assync)");
            stage++;

            if (data.Invokes.Count > 0)
                Invokes.Parse(this);


            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removing empty structs");
            stage++;

            ast.Apply(new RemoveEmptyStructs(this));
            
            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Making delegates");
            stage++;

            

            ast.Apply(new Delegates(this));
            
            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming inline methods (Run 1)");
            stage++;

            //ast.Apply(new AddUnneededRef(data));
            ast.Apply(new FixInlineMethods(this, false));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removing unneeded ref parameters");
            stage++;

            

            ast.Apply(new RemoveUnnededRef(data));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Expanding struct equality tests");
            stage++;

            ast.Apply(new SplitStructTests(data));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming pointers");
            stage++;

            new Pointers(data).Parse(ast);


            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Transforming inline methods (Run 2)");
            stage++;

            ast.Apply(new FixInlineMethods(this, true));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Splitting local structs into primitives");
            stage++;

            //Split local struct into primitives, to make optimizations easier
            ast.Apply(new StructSplitter(data));
            //ast.Apply(new BulkCopyFixup(this));
            //BulkCopyFixup.Parse(ast, this);

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Removnig redundant assignments");
            stage++;

            //Remove stupid assignments (assignments to a variable where that variable is not used before its next assignment
            ast.Apply(new RemoveUnusedVariables(this));


            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Folding constants");
            stage++;



            
            ast.Apply(new ConstantFolding(data));

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Optimizing");
            stage++;


            

            //Assign fixup was here //Dahm grafiti painters
            //ast.Apply(new LivenessAnalysis(this));
            ast.Apply(new Optimizations.OptimizePhase(this, "Transforming code (" + (stage - 1) + " / " + totalStages + "): Optimizing"));


            ast.Apply(new FixByteArrayIndexes(data));
            

            if (Options.Compiler.MakeShortNames)
            {
                if (data.AllowPrintouts)
                    Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Making short names");
                stage++;

                ast.Apply(new MakeShortNames(this));
            }
            else
            {
                if (data.AllowPrintouts)
                    Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Making unique names");
                stage++;

                //MakeUniqueNames.Parse(ast, this);
                ast.Apply(new MakeUniqueNamesV2());
            }

            

            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Renaming references to variables whose names have changed");
            stage++;
            //Remove uneeded blocks and check that names fit the decls
            ast.Apply(new RenameRefferences(this));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Obfuscating strings");
            stage++;
            //Obfuscate strings
            ast.Apply(new ObfuscateStrings(this));

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Mergeing methods if they are the same");
            stage++;
            MergeSameMethods.Parse(this);

            
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText("Transforming code (" + stage + " / " + totalStages + "): Generating includes (merging to one file if selected)");
            stage++;
            //Insert includes, and move methods, structs and fields around so they are visible
            FixIncludes.Apply(ast, this);
            if (Options.Compiler.OneOutputFile)
                ((AASourceFile)mainEntry.Parent()).SetName(new TIdentifier("MapScript"));
            
        }

        
        private void MakeInitializerInvokes()
        {
            AABlock block = (AABlock) mainEntry.GetBlock();
            AASourceFile file = Util.GetAncestor<AASourceFile>(mainEntry);
            AMethodDecl invokeMethod;
            /* Add
             * void Invoke(string methodName)
             * {
             *     trigger initTrigger = TriggerCreate(methodName);
             *     TriggerExecute(initTrigger, false, true);
             *     TriggerDestroy(initTrigger);
             * }
             */

            {
                //void Invoke(string methodName)
                AALocalDecl parameter = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("string"), null),
                                                        new TIdentifier("methodName"), null);
                AABlock methodBody = new AABlock();
                invokeMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                     new AVoidType(new TVoid("void")), new TIdentifier("Invoke"),
                                                     new ArrayList(){parameter}, methodBody);

                //trigger initTrigger = TriggerCreate(methodName);
                ALocalLvalue parameterLvalue = new ALocalLvalue(new TIdentifier("methodName"));
                data.LocalLinks[parameterLvalue] = parameter;
                ALvalueExp parameterLvalueExp = new ALvalueExp(parameterLvalue);
                data.LvalueTypes[parameterLvalue] =
                    data.ExpTypes[parameterLvalueExp] = parameter.GetType();
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("TriggerCreate"), new ArrayList(){parameterLvalueExp});
                data.ExpTypes[invoke] = new ANamedType(new TIdentifier("trigger"), null);
                foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                {
                    if (methodDecl.GetName().Text == "TriggerCreate")
                    {
                        data.SimpleMethodLinks[invoke] = methodDecl;
                        break;
                    }
                }
                AALocalDecl initTriggerDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("trigger"), null),
                                                        new TIdentifier("initTrigger"), invoke);
                methodBody.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), initTriggerDecl));

                //TriggerExecute(initTrigger, false, true);
                ALocalLvalue initTriggerLvalue = new ALocalLvalue(new TIdentifier("initTrigger"));
                data.LocalLinks[initTriggerLvalue] = initTriggerDecl;
                ALvalueExp initTriggerLvalueExp = new ALvalueExp(initTriggerLvalue);
                data.LvalueTypes[initTriggerLvalue] =
                    data.ExpTypes[initTriggerLvalueExp] = initTriggerDecl.GetType();
                ABooleanConstExp falseBool = new ABooleanConstExp(new AFalseBool());
                ABooleanConstExp trueBool = new ABooleanConstExp(new ATrueBool());
                data.ExpTypes[falseBool] =
                    data.ExpTypes[trueBool] = new ANamedType(new TIdentifier("bool"), null);
                invoke = new ASimpleInvokeExp(new TIdentifier("TriggerExecute"), new ArrayList() { initTriggerLvalueExp, falseBool, trueBool });
                data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                {
                    if (methodDecl.GetName().Text == "TriggerExecute")
                    {
                        data.SimpleMethodLinks[invoke] = methodDecl;
                        break;
                    }
                }
                methodBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), invoke));

                //TriggerDestroy(initTrigger);
                initTriggerLvalue = new ALocalLvalue(new TIdentifier("initTrigger"));
                data.LocalLinks[initTriggerLvalue] = initTriggerDecl;
                initTriggerLvalueExp = new ALvalueExp(initTriggerLvalue);
                data.LvalueTypes[initTriggerLvalue] =
                    data.ExpTypes[initTriggerLvalueExp] = initTriggerDecl.GetType();
                invoke = new ASimpleInvokeExp(new TIdentifier("TriggerDestroy"), new ArrayList() { initTriggerLvalueExp });
                data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                {
                    if (methodDecl.GetName().Text == "TriggerDestroy")
                    {
                        data.SimpleMethodLinks[invoke] = methodDecl;
                        break;
                    }
                }
                methodBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), invoke));

                file.GetDecl().Add(invokeMethod);
            }

            for (int i = data.InitializerMethods.Count - 1; i >= 0; i--)
            {
                AMethodDecl method = data.InitializerMethods[i];
                //Turn method into a trigger
                method.SetReturnType(new ANamedType(new TIdentifier("bool"), null));
                method.GetFormals().Add(new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("bool"), null),
                                                        new TIdentifier("testConds"), null));
                method.GetFormals().Add(new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("bool"), null),
                                                        new TIdentifier("runActions"), null));
                method.SetTrigger(new TTrigger("trigger"));
                ((AABlock) method.GetBlock()).GetStatements().Add(new AVoidReturnStm(new TReturn("return")));
                TriggerConvertReturn returnConverter = new TriggerConvertReturn(data);
                method.Apply(returnConverter);
                data.TriggerDeclarations[method] = new List<TStringLiteral>();


                //Add Invoke(<name>); to main entry
                TStringLiteral literal = new TStringLiteral(method.GetName().Text);
                data.TriggerDeclarations[method].Add(literal);
                AStringConstExp stringConst = new AStringConstExp(literal);
                data.ExpTypes[stringConst] = new ANamedType(new TIdentifier("string"), null);
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("Invoke"), new ArrayList(){stringConst});
                data.SimpleMethodLinks[invoke] = invokeMethod;
                data.ExpTypes[invoke] = invokeMethod.GetReturnType();
                block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), invoke));



                //ASyncInvokeExp syncInvokeExp = new ASyncInvokeExp(new TSyncInvoke("Invoke"), new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier(method.GetName().Text))), new ArrayList());
                //data.Invokes.Add(method, new List<InvokeStm>(){new InvokeStm(syncInvokeExp)});
                //data.ExpTypes[syncInvokeExp] = new AVoidType(new TVoid("void"));

                //block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), syncInvokeExp));
            }
            for (int i = data.InvokeOnIniti.Count - 1; i >= 0; i--)
            {
                AMethodDecl method = data.InvokeOnIniti[i];

                //Turn method into a trigger
                method.SetReturnType(new ANamedType(new TIdentifier("bool"), null));
                method.GetFormals().Add(new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("bool"), null),
                                                        new TIdentifier("testConds"), null));
                method.GetFormals().Add(new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        new ANamedType(new TIdentifier("bool"), null),
                                                        new TIdentifier("runActions"), null));
                method.SetTrigger(new TTrigger("trigger"));
                ((AABlock)method.GetBlock()).GetStatements().Add(new AVoidReturnStm(new TReturn("return")));
                TriggerConvertReturn returnConverter = new TriggerConvertReturn(data);
                method.Apply(returnConverter);
                data.TriggerDeclarations[method] = new List<TStringLiteral>();


                //Add Invoke(<name>); to main entry
                TStringLiteral literal = new TStringLiteral(method.GetName().Text);
                data.TriggerDeclarations[method].Add(literal);
                AStringConstExp stringConst = new AStringConstExp(literal);
                data.ExpTypes[stringConst] = new ANamedType(new TIdentifier("string"), null);
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("Invoke"), new ArrayList() { stringConst });
                data.SimpleMethodLinks[invoke] = invokeMethod;
                data.ExpTypes[invoke] = invokeMethod.GetReturnType();
                block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), invoke));


                /*
                ASyncInvokeExp syncInvokeExp = new ASyncInvokeExp(new TSyncInvoke("Invoke"),  new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier(method.GetName().Text))), new ArrayList());
                data.Invokes.Add(method, new List<InvokeStm>() { new InvokeStm(syncInvokeExp) });
                data.ExpTypes[syncInvokeExp] = new AVoidType(new TVoid("void"));

                block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), syncInvokeExp));*/

            }
            for (int i = data.FieldsToInitInMapInit.Count - 1; i >= 0; i--)
            {
                AFieldDecl field = data.FieldsToInitInMapInit[i];
                if (field.GetInit() == null)
                    continue;

                AFieldLvalue lvalue = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), lvalue, field.GetInit());

                data.ExpTypes[assignment] =
                    data.LvalueTypes[lvalue] = field.GetType();
                data.FieldLinks[lvalue] = field;

                block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), assignment));
                
            }
            block.RemoveChild(mainEntryFieldInitBlock);
            block.GetStatements().Insert(0, mainEntryFieldInitBlock);
        }

        private class TriggerConvertReturn : DepthFirstAdapter
        {
            private SharedData data;

            public TriggerConvertReturn(SharedData data)
            {
                this.data = data;
            }

            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                ABooleanConstExp trueBool = new ABooleanConstExp(new ATrueBool());
                AValueReturnStm replacer = new AValueReturnStm(node.GetToken(), trueBool);
                node.ReplaceBy(replacer);
                data.ExpTypes[trueBool] = new ANamedType(new TIdentifier("bool"), null);
            }
        }


        /*public class ErrorChecker : DepthFirstAdapter
        {
            private SharedData data;

            public ErrorChecker(SharedData data)
            {
                this.data = data;
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                if (Util.HasAncestor<AMethodDecl>(node) && Util.GetAncestor<AMethodDecl>(node).GetName().Text.Contains("WindowMouseMove"))
                {
                    if (node.GetBase() is ALvalueExp && ((ALvalueExp)node.GetBase()).GetLvalue() is AFieldLvalue)
                    {
                        AFieldLvalue lvalue = (AFieldLvalue)((ALvalueExp)node.GetBase()).GetLvalue();
                        AFieldDecl decl = data.FieldLinks[lvalue];
                        if (decl.GetName().Text.ToLower().EndsWith("windows"))
                        {
                            if (node.GetIndex() is ASimpleInvokeExp)
                            {
                                ASimpleInvokeExp invokeExp = (ASimpleInvokeExp)node.GetIndex();
                                AMethodDecl mDecl = data.SimpleMethodLinks[invokeExp];
                                if (mDecl.GetName().Text != "EventPlayer")
                                    node = node;
                            }
                            else
                            {
                                node = node;
                            }
                        }
                    }
                }
                base.CaseAArrayLvalue(node);
            }
        }*/

        public class ErrorChecker : DepthFirstAdapter
        {
            public override void InALocalDeclStm(ALocalDeclStm node)
            {
                if (node.GetLocalDecl() == null)
                    node = node;
            }
        }
    }
}
