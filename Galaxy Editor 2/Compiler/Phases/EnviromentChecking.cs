using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class EnviromentChecking : DepthFirstAdapter
    {
        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data)
        {
            ast.Apply(new EnviromentChecking(errors, data));
        }

        private ErrorCollection errors;
        private SharedData data;
        private bool doChecks;

        public EnviromentChecking(ErrorCollection errors, SharedData data, bool doChecks = true)
        {
            this.errors = errors;
            this.data = data;
            this.doChecks = doChecks;


            //Check that no name is used twice
            //Can distinguish between structs, fields and methods in the context. So check other things of same type in user data and includes
            #region Check dublicates

            if (doChecks)
            {
                //Methods
                for (int i = 0; i < data.Methods.Count; i++)
                {
                    AMethodDecl method = data.Methods[i].Decl;
                    string signature = Util.GetMethodSignature(method);
                    //User data
                    /*for (int j = i + 1; j < data.Methods.Count; j++)
                    {
                        if (signature == Util.GetMethodSignature(data.Methods[j].Decl) && 
                            (Util.IsVisible(data.Methods[i].Decl, data.Methods[j].Decl) || Util.IsVisible(data.Methods[j].Decl, data.Methods[i].Decl)))
                        {
                            errors.Add(new ErrorCollection.Error(method.GetName(), data.Methods[i].File, "Dublicate methods", false));
                            errors.Add(new ErrorCollection.Error(data.Methods[j].Decl.GetName(), data.Methods[j].File, "Dublicate methods", false));
                            data.Methods[j].Decl.Parent().RemoveChild(data.Methods[j].Decl);
                            data.Methods.RemoveAt(j);
                            j--;
                        }
                    }*/

                    //Includes
                    foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                    {
                        if (signature == Util.GetMethodSignature(methodDecl) &&
                            (methodDecl.GetBlock() != null || methodDecl.GetNative() != null))
                        {
                            errors.Add(new ErrorCollection.Error(method.GetName(), data.Methods[i].File,
                                                                 "Method has same signature as a library method.", false));
                            data.Methods[i].Decl.Parent().RemoveChild(data.Methods[i].Decl);
                            data.Methods.RemoveAt(i);
                            i--;
                            break;
                        }
                    }

                    //Triggers
                    /*if (method.GetFormals().Count == 2 && 

                        ((AALocalDecl)method.GetFormals()[0]).GetType() is ANamedType && 
                        ((ANamedType)((AALocalDecl)method.GetFormals()[0]).GetType()).GetName().Text == "bool" &&
                    
                        ((AALocalDecl)method.GetFormals()[1]).GetType() is ANamedType && 
                        ((ANamedType)((AALocalDecl)method.GetFormals()[1]).GetType()).GetName().Text == "bool")
                        foreach (ATriggerDecl trigger in data.Triggers)
                        {
                            if (trigger.GetName().Text == method.GetName().Text)
                            {
                                errors.Add(new ErrorCollection.Error(method.GetName(), data.Methods[i].File, "Method has same signature as a library method. You can uncheck the library from the include list.", false));
                                data.Methods[i].Decl.Parent().RemoveChild(data.Methods[i].Decl);
                                data.Methods.RemoveAt(i);
                                i--;
                                break;
                            }
                        }*/
                }

                //Fields
                for (int i = 0; i < data.Fields.Count; i++)
                {
                    AFieldDecl field = data.Fields[i].Decl;
                    //User data
                    /*for (int j = i + 1; j < data.Fields.Count; j++)
                    {
                        if (field.GetName().Text == data.Fields[j].Decl.GetName().Text &&
                            (Util.IsVisible(data.Fields[i].Decl, data.Fields[j].Decl) || Util.IsVisible(data.Fields[j].Decl, data.Fields[i].Decl)))
                        {
                            errors.Add(new ErrorCollection.Error(field.GetName(), data.Fields[i].File, "Dublicate fields", false));
                            errors.Add(new ErrorCollection.Error(data.Fields[j].Decl.GetName(), data.Fields[j].File, "Dublicate fields", false));
                            data.Fields[j].Decl.Parent().RemoveChild(data.Fields[j].Decl);
                            data.Fields.RemoveAt(j);
                            j--;
                        }
                    }*/

                    //Includes
                    foreach (AFieldDecl fieldDecl in data.Libraries.Fields)
                    {
                        if (field.GetName().Text == fieldDecl.GetName().Text)
                        {
                            errors.Add(new ErrorCollection.Error(field.GetName(), data.Structs[i].File,
                                                                 "Field has the same name as an included field.", false));
                            field.Parent().RemoveChild(field);
                            data.Structs.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }

                //Enrichments
                foreach (AEnrichmentDecl enrichment in data.Enrichments)
                {
                    //Constructors
                    {
                        List<AConstructorDecl> list = new List<AConstructorDecl>();
                        list.AddRange(enrichment.GetDecl().OfType<AConstructorDecl>());
                        for (int i = 0; i < list.Count; i++)
                        {
                            string sig1 = Util.GetConstructorSignature(list[i]);
                            for (int j = i + 1; j < list.Count; j++)
                            {
                                string sig2 = Util.GetConstructorSignature(list[j]);
                                if (sig1 == sig2)
                                {
                                    errors.Add(new ErrorCollection.Error(enrichment.GetToken(), currentFile,
                                                                         "Two constructors found with the signature " +
                                                                         sig1));
                                }
                            }
                        }
                    }
                    //Deconstructors
                    {
                        List<ADeconstructorDecl> list = new List<ADeconstructorDecl>();
                        list.AddRange(enrichment.GetDecl().OfType<ADeconstructorDecl>());
                        if (list.Count > 1)
                        {
                            List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                            foreach (ADeconstructorDecl deconstructor in list)
                            {
                                subErrors.Add(new ErrorCollection.Error(deconstructor.GetName(), "Deconstructor"));
                            }
                            errors.Add(new ErrorCollection.Error(enrichment.GetToken(),
                                                                 "Multiple deconstructors found in enrichment. Only one is allowed."));
                        }
                    }

                }

                //Structs
                for (int i = 0; i < data.Structs.Count; i++)
                {
                    AStructDecl str = data.Structs[i].Decl;
                    //User data
                    /*for (int j = i + 1; j < data.Structs.Count; j++)
                    {
                        if (str.GetName().Text == data.Structs[j].Decl.GetName().Text &&
                            (Util.IsVisible(data.Structs[i].Decl, data.Structs[j].Decl) || Util.IsVisible(data.Structs[j].Decl, data.Structs[i].Decl)))
                        {
                            errors.Add(new ErrorCollection.Error(str.GetName(), data.Structs[i].File, "Dublicate structs", false));
                            errors.Add(new ErrorCollection.Error(data.Structs[j].Decl.GetName(), data.Structs[j].File, "Dublicate structs", false));
                            data.Structs[j].Decl.Parent().RemoveChild(data.Structs[j].Decl);
                            data.Structs.RemoveAt(j);
                            j--;
                        }
                    }*/

                    //Includes
                    foreach (AStructDecl structDecl in data.Libraries.Structs)
                    {
                        if (str.GetName().Text == structDecl.GetName().Text)
                        {
                            errors.Add(new ErrorCollection.Error(str.GetName(), data.Structs[i].File,
                                                                 "Struct has the same name as an included struct.",
                                                                 false));
                            str.Parent().RemoveChild(str);
                            data.Structs.RemoveAt(i);
                            i--;
                            break;
                        }
                    }

                    //Constructors
                    for (int j = 0; j < data.StructConstructors[str].Count; j++)
                    {
                        string sig1 = Util.GetConstructorSignature(data.StructConstructors[str][j]);
                        for (int k = j + 1; k < data.StructConstructors[str].Count; k++)
                        {
                            string sig2 = Util.GetConstructorSignature(data.StructConstructors[str][k]);
                            if (sig1 == sig2)
                            {
                                errors.Add(new ErrorCollection.Error(str.GetName(), currentFile,
                                                                     "Two constructors found with the signature " + sig1));
                            }
                        }
                    }

                    //Struct fields

                    //if (data.StructFields[str].Count == 0)
                    //    errors.Add(new ErrorCollection.Error(str.GetName(), Util.GetAncestor<AASourceFile>(str), "A struct must have atleast one field", false));
                    for (int j = 0; j < data.StructFields[str].Count; j++)
                    {
                        AALocalDecl field = data.StructFields[str][j];
                        for (int k = j + 1; k < data.StructFields[str].Count; k++)
                        {
                            if (field.GetName().Text == data.StructFields[str][k].GetName().Text)
                            {
                                errors.Add(new ErrorCollection.Error(field.GetName(), data.Structs[i].File,
                                                                     "Dublicate struct fields", false));
                                errors.Add(new ErrorCollection.Error(data.StructFields[str][k].GetName(),
                                                                     data.Structs[i].File, "Dublicate struct fields",
                                                                     false));
                                data.StructFields[str][k].Parent().RemoveChild(data.StructFields[str][k]);
                                data.StructFields[str].RemoveAt(k);
                                j--;
                            }
                        }
                        for (int k = 0; k < data.StructProperties[str].Count; k++)
                        {
                            if (field.GetName().Text == data.StructProperties[str][k].GetName().Text)
                            {
                                errors.Add(new ErrorCollection.Error(field.GetName(), data.Structs[i].File,
                                                                     "Dublicate struct fields", false));
                                errors.Add(new ErrorCollection.Error(data.StructProperties[str][k].GetName(),
                                                                     data.Structs[i].File, "Dublicate struct fields",
                                                                     false));
                                data.StructProperties[str][k].Parent().RemoveChild(data.StructFields[str][k]);
                                data.StructProperties[str].RemoveAt(k);
                                j--;
                            }
                        }
                    }

                    //Struct methods
                    for (int j = 0; j < data.StructMethods[str].Count; j++)
                    {
                        AMethodDecl field = data.StructMethods[str][j];
                        for (int k = j + 1; k < data.StructMethods[str].Count; k++)
                        {
                            if (Util.GetMethodSignature(field) == Util.GetMethodSignature(data.StructMethods[str][k]))
                            {
                                errors.Add(new ErrorCollection.Error(field.GetName(), data.Structs[i].File,
                                                                     "Dublicate struct methods", false));
                                errors.Add(new ErrorCollection.Error(data.StructMethods[str][j].GetName(),
                                                                     data.Structs[i].File, "Dublicate struct methods",
                                                                     false));
                                data.StructMethods[str][k].Parent().RemoveChild(data.StructMethods[str][k]);
                                data.StructMethods[str].RemoveAt(k);
                                j--;
                            }
                        }
                    }
                }
                //type defs
                foreach (ATypedefDecl typedef in data.Typedefs)
                {
                    ANamedType namedType = (ANamedType) typedef.GetName();
                    AAName name = (AAName) namedType.GetName();
                    if (name.GetIdentifier().Count > 1)
                    {
                        errors.Add(new ErrorCollection.Error(typedef.GetToken(), "You can only typedef to a simple name. Use namespaces if you want to refference it with dots."));
                    }
                    else
                    {
                        if (GalaxyKeywords.Primitives.words.Contains(((TIdentifier)name.GetIdentifier()[0]).Text))
                        {
                            errors.Add(new ErrorCollection.Error(typedef.GetToken(), Util.GetAncestor<AASourceFile>(typedef),
                                                                 "You can not overwrite primitive names with typedefs."));
                        }
                    }
                }
            }

            #endregion
        }

        public override void OutAUsingDecl(AUsingDecl node)
        {
            //Check that what it points to actually exists
            AAProgram program = Util.GetAncestor<AAProgram>(node);

            bool found = false;
            
            foreach (AASourceFile sourceFile in program.GetSourceFiles())
            {
                List<TIdentifier> searchForIdentifiers = new List<TIdentifier>();
                foreach (TIdentifier identifier in node.GetNamespace())
                {
                    searchForIdentifiers.Add(identifier);
                }
                List<IList> nextDecls = new List<IList>();
                List<IList> currentDecls = new List<IList>();
                currentDecls.Add(sourceFile.GetDecl());
                while (searchForIdentifiers.Count > 0)
                {
                    string name = searchForIdentifiers[0].Text;
                    searchForIdentifiers.RemoveAt(0);
                    foreach (IList currentDeclList in currentDecls)
                    {
                        foreach (PDecl decl in currentDeclList)
                        {
                            if (decl is ANamespaceDecl)
                            {
                                ANamespaceDecl aDecl = (ANamespaceDecl) decl;
                                if (aDecl.GetName().Text == name)
                                {
                                    nextDecls.Add(aDecl.GetDecl());
                                }
                            }
                        }
                    }
                    currentDecls = nextDecls;
                    nextDecls = new List<IList>();
                }
                if (currentDecls.Count > 0)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                string identifierName = "";
                foreach (TIdentifier identifier in node.GetNamespace())
                {
                    if (identifierName != "")
                        identifierName += ".";
                    identifierName += identifier.Text;
                }
                errors.Add(new ErrorCollection.Error((Token)node.GetNamespace()[0], "No namespace found matching " + identifierName, true));
            }
        }
        

        public override void InAAProgram(AAProgram node)
        {
            return;
            //Check that there are not two diffrent definitions with same name, in same namespace

            for (int i = 0; i < node.GetSourceFiles().Count; i++)
            {
                for (int j = i; j < node.GetSourceFiles().Count; j++)
                {
                    AASourceFile file1 = (AASourceFile) node.GetSourceFiles()[i];
                    AASourceFile file2 = (AASourceFile) node.GetSourceFiles()[j];

                    Check(new List<IList>() {file1.GetDecl()}, new List<IList>() {file2.GetDecl()});
                }
            }
        }

        private void Check(List<IList> file1Decls, List<IList> file2Decls)
        {
            Dictionary<string, Util.Pair<List<IList>, List<IList>>> namespaces = new Dictionary<string, Util.Pair<List<IList>, List<IList>>>();

            foreach (IList file1DeclList in file1Decls)
            {
                foreach (PDecl file1Decl in file1DeclList)
                {
                    if (file1Decl is ANamespaceDecl)
                    {
                        ANamespaceDecl aFile1Decl = (ANamespaceDecl) file1Decl;
                        string name = aFile1Decl.GetName().Text;
                        if (!namespaces.ContainsKey(name))
                            namespaces.Add(name, new Util.Pair<List<IList>, List<IList>>(new List<IList>(), new List<IList>()));
                        namespaces[name].First.Add(aFile1Decl.GetDecl());
                        continue;
                    }
                    if (data.ObfuscationFields.Contains(file1Decl))
                        continue;
                    foreach (IList file2DeclList in file2Decls)
                    {
                        foreach (PDecl file2Decl in file2DeclList)
                        {
                            if (file2Decl is ANamespaceDecl)
                            {
                                ANamespaceDecl aFile2Decl = (ANamespaceDecl)file2Decl;
                                string name = aFile2Decl.GetName().Text;
                                if (!namespaces.ContainsKey(name))
                                    namespaces.Add(name, new Util.Pair<List<IList>, List<IList>>(new List<IList>(), new List<IList>()));
                                if (!namespaces[name].Second.Contains(aFile2Decl.GetDecl()))
                                    namespaces[name].Second.Add(aFile2Decl.GetDecl());
                                continue;
                            }
                            if (data.ObfuscationFields.Contains(file2Decl) ||
                                file1Decl == file2Decl ||
                                !Util.IsBefore(file1Decl, file2Decl))
                                continue;


                        }
                    }
                }
            }
            foreach (var v in namespaces)
            {
                Util.Pair<List<IList>, List<IList>> pair = v.Value;
                if (pair.First.Count > 0 && pair.Second.Count > 0)
                    Check(pair.First, pair.Second);
            }
        }

        

        private AASourceFile currentFile;

        private List<List<AASourceFile>> handeledNamespaces = new List<List<AASourceFile>>();
        public override void CaseAASourceFile(AASourceFile node)
        {
            currentFile = node;
            base.CaseAASourceFile(node);
           /* return;

            if (!doChecks)
            {
                base.CaseAASourceFile(node);
                return;
            }

            //Check that there are no dublicates in visible files
            List<AASourceFile> visibleFiles = new List<AASourceFile>();
            AAProgram program = (AAProgram) node.Parent();
            foreach (AASourceFile sourceFile in program.GetSourceFiles())
            {
                if (Util.IsVisible(node, sourceFile))
                    visibleFiles.Add(sourceFile);
            }

            List<AASourceFile> leftSide = new List<AASourceFile>();
            Dictionary<AASourceFile, List<AASourceFile>> rightSides = new Dictionary<AASourceFile, List<AASourceFile>>();

            for (int i = 0; i < visibleFiles.Count; i++)
            {
                AASourceFile file1 = visibleFiles[i];
                List<AASourceFile> rightSide = new List<AASourceFile>();
                if (file1 == node)
                {
                    rightSide.AddRange(visibleFiles);
                }
                else
                {
                    for (int j = i + 1; j < visibleFiles.Count; j++)
                    {
                        AASourceFile file2 = visibleFiles[j];
                        bool add = true;
                        foreach (List<AASourceFile> files in handeledNamespaces)
                        {
                            if (files.Contains(file1) && files.Contains(file2))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                        {
                            rightSide.Add(file2);
                        }
                    }
                }
                if (rightSide.Count > 0)
                {
                    leftSide.Add(file1);
                    rightSides.Add(file1, rightSide);
                }
            }
            handeledNamespaces.Add(visibleFiles);


            //for (int i = 0; i < visibleFiles.Count; i++)
            foreach (AASourceFile file1 in leftSide)
            {
                PDecl[] decls1 = file1.GetDecl().OfType<PDecl>().ToArray();
               // AASourceFile file1 = visibleFiles[i];
                //for (int j = i + 1; j < visibleFiles.Count; j++)
                foreach (AASourceFile file2 in rightSides[file1])
                {
                    PDecl[] decls2 = file2.GetDecl().OfType<PDecl>().ToArray();
                    //AASourceFile file2 = visibleFiles[j];
                    for (int decl1Index = 0; decl1Index < decls1.Length; decl1Index++)
                    {
                        PDecl decl1 = decls1[decl1Index];
                        if (!Util.IsDeclVisible(decl1, node))
                            continue;
                        for (int decl2Index = file1 == file2 ? decl1Index + 1 : 0; decl2Index < decls2.Length; decl2Index++)
                        {
                            PDecl decl2 = (PDecl)decls2[decl2Index];
                            if (data.ObfuscationFields.Contains(decl1) && data.ObfuscationFields.Contains(decl2))
                                continue;
                            if (!Util.IsDeclVisible(decl2, node))
                                continue;
                            if (decl1 == decl2)
                                continue;
                            if (file1 == file2 && file1.GetDecl().IndexOf(decl1) > file1.GetDecl().IndexOf(decl2))
                                continue;
                            if (decl1 is AEnrichmentDecl && decl2 is AEnrichmentDecl)
                            {
                                AEnrichmentDecl aDecl1 = (AEnrichmentDecl)decl1;
                                AEnrichmentDecl aDecl2 = (AEnrichmentDecl)decl2;

                                if (Util.TypesEqual(aDecl1.GetType(), aDecl2.GetType(), data))
                                {
                                    //Only an error if they define something which is the same
                                    foreach (PDecl subDecl1 in aDecl1.GetDecl())
                                    {
                                        foreach (PDecl subDecl2 in aDecl2.GetDecl())
                                        {
                                            if (subDecl1.GetType() != subDecl2.GetType())
                                                continue;
                                            if (subDecl1 is AMethodDecl)
                                            {
                                                AMethodDecl aSubDecl1 = (AMethodDecl)subDecl1;
                                                AMethodDecl aSubDecl2 = (AMethodDecl)subDecl2;

                                                if (Util.GetMethodSignature(aSubDecl1) == Util.GetMethodSignature(aSubDecl2))
                                                {
                                                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl1.GetName(), file1,
                                                                                            "Matching method declaration"));
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl2.GetName(), file2,
                                                                                            "Matching method declaration"));
                                                    errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                         "Two methods of same signature defined in the enrichment " +
                                                                                         Util.TypeToString(
                                                                                             aDecl1.GetType()) +
                                                                                         ", and is visible from " +
                                                                                         node.GetName().Text, false,
                                                                                         subErrors.ToArray()));
                                                }
                                            }
                                            else if (subDecl1 is AConstructorDecl)
                                            {
                                                AConstructorDecl aSubDecl1 = (AConstructorDecl)subDecl1;
                                                AConstructorDecl aSubDecl2 = (AConstructorDecl)subDecl2;

                                                if (Util.GetConstructorSignature(aSubDecl1) == Util.GetConstructorSignature(aSubDecl2))
                                                {
                                                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl1.GetName(), file1,
                                                                                            "Matching constructor declaration"));
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl2.GetName(), file2,
                                                                                            "Matching constructor declaration"));
                                                    errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                         "Two constructors of same signature defined in the enrichment " +
                                                                                         Util.TypeToString(
                                                                                             aDecl1.GetType()) +
                                                                                         ", and is visible from " +
                                                                                         node.GetName().Text, false,
                                                                                         subErrors.ToArray()));
                                                }
                                            }
                                            else if (subDecl1 is ADeconstructorDecl)
                                            {
                                                ADeconstructorDecl aSubDecl1 = (ADeconstructorDecl)subDecl1;
                                                ADeconstructorDecl aSubDecl2 = (ADeconstructorDecl)subDecl2;
                                                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                                subErrors.Add(new ErrorCollection.Error(aSubDecl1.GetName(), file1,
                                                                                        "Matching deconstructor declaration"));
                                                subErrors.Add(new ErrorCollection.Error(aSubDecl2.GetName(), file2,
                                                                                        "Matching deconstructor declaration"));
                                                errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                     "Two deconstructors is defined for the enrichment of " +
                                                                                     Util.TypeToString(
                                                                                         aDecl1.GetType()) +
                                                                                     ", and is visible from " +
                                                                                     node.GetName().Text, false,
                                                                                     subErrors.ToArray()));

                                            }
                                            else if (subDecl1 is APropertyDecl)
                                            {
                                                APropertyDecl aSubDecl1 = (APropertyDecl)subDecl1;
                                                APropertyDecl aSubDecl2 = (APropertyDecl)subDecl2;

                                                if (aSubDecl1.GetName().Text == aSubDecl2.GetName().Text)
                                                {
                                                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl1.GetName(), file1,
                                                                                            "Matching property declaration"));
                                                    subErrors.Add(new ErrorCollection.Error(aSubDecl2.GetName(), file2,
                                                                                            "Matching property declaration"));
                                                    errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                         "Two properties of same name defined in the enrichment " +
                                                                                         Util.TypeToString(
                                                                                             aDecl1.GetType()) +
                                                                                         ", and is visible from " +
                                                                                         node.GetName().Text, false,
                                                                                         subErrors.ToArray()));
                                                }
                                            }
                                        }
                                    }

                                    /*List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                    subErrors.Add(new ErrorCollection.Error(aDecl1.GetToken(), file1,
                                                                            "Matching enrichment declaration"));
                                    subErrors.Add(new ErrorCollection.Error(aDecl2.GetToken(), file2,
                                                                            "Matching enrichment declaration"));
                                    errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                         "Two enrichments on same type is visible from " +
                                                                         node.GetName().Text, false,
                                                                         subErrors.ToArray()));*//*
                                }

                            }
                            if (decl1 is AStructDecl)
                            {
                                AStructDecl aDecl1 = (AStructDecl) decl1;

                                if (decl2 is AStructDecl)
                                {
                                    AStructDecl aDecl2 = (AStructDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching struct declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching struct declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two types of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is AMethodDecl)
                                {
                                    AMethodDecl aDecl2 = (AMethodDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;
                                    if (aDecl2.GetDelegate() != null && aDecl2.GetName().Text == aDecl1.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching struct declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching delegate declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two types of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is ATypedefDecl)
                                {
                                    ATypedefDecl aDecl2 = (ATypedefDecl)decl2;
                                    if (aDecl1.GetName().Text == ((ANamedType)aDecl2.GetName()).GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching struct declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetToken(), file2,
                                                                                "Matching typedef"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two types of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                            }
                            if (decl1 is ATypedefDecl)
                            {
                                ATypedefDecl aDecl1 = (ATypedefDecl)decl1;
                                if (decl2 is ATypedefDecl)
                                {
                                    ATypedefDecl aDecl2 = (ATypedefDecl)decl2;
                                    if (((ANamedType)aDecl1.GetName()).GetName().Text == ((ANamedType)aDecl2.GetName()).GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetToken(), file1,
                                                                                "Matching typedef"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetToken(), file2,
                                                                                "Matching typedef"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two typedefs of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is AStructDecl)
                                {
                                    AStructDecl aDecl2 = (AStructDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;
                                    if (aDecl2.GetName().Text == ((ANamedType)aDecl1.GetName()).GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file1,
                                                                                "Matching struct declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetToken(), file2,
                                                                                "Matching typedef"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two types of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                
                            }
                            if (decl1 is AFieldDecl)
                            {
                                AFieldDecl aDecl1 = (AFieldDecl)decl1;
                                if (decl2 is AFieldDecl)
                                {
                                    AFieldDecl aDecl2 = (AFieldDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching field declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching field declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two fields of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if( decl2 is ATriggerDecl)
                                {
                                    ATriggerDecl aDecl2 = (ATriggerDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching field declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching trigger declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A field and a trigger of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is APropertyDecl)
                                {
                                    APropertyDecl aDecl2 = (APropertyDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching field declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching property declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A field and a property of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                    
                                }
                            }
                            if (decl1 is APropertyDecl)
                            {
                                APropertyDecl aDecl1 = (APropertyDecl)decl1;
                                if (decl2 is AFieldDecl)
                                {
                                    AFieldDecl aDecl2 = (AFieldDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching property declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching field declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A field and a property of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is ATriggerDecl)
                                {
                                    ATriggerDecl aDecl2 = (ATriggerDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching property declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching trigger declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A property and a trigger of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is APropertyDecl)
                                {
                                    APropertyDecl aDecl2 = (APropertyDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching property declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching property declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two properties of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }

                                }
                            }
                            if (decl1 is AMethodDecl)
                            {
                                AMethodDecl aDecl1 = (AMethodDecl)decl1;
                                

                                if (decl2 is AMethodDecl)
                                {
                                    AMethodDecl aDecl2 = (AMethodDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetDelegate() == null && aDecl2.GetDelegate() == null)
                                    {
                                        if (Util.GetMethodSignature(aDecl1) == Util.GetMethodSignature(aDecl2))
                                        {
                                            List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                            subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                    "Matching method declaration"));
                                            subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                    "Matching method declaration"));
                                            errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                 "Two methods of same signature is visible from " +
                                                                                 node.GetName().Text, false,
                                                                                 subErrors.ToArray()));
                                        }
                                    }
                                    else if (aDecl1.GetDelegate() != null && aDecl2.GetDelegate() != null)
                                    {
                                        if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                        {
                                            List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                            subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                    "Matching delegate declaration"));
                                            subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                    "Matching delegate declaration"));
                                            errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                 "Two types of same name is visible from " +
                                                                                 node.GetName().Text, false,
                                                                                 subErrors.ToArray()));
                                        }
                                    }
                                }
                                else if (decl2 is ATriggerDecl &&

                                        aDecl1.GetFormals().Count == 2 &&

                                        ((AALocalDecl)aDecl1.GetFormals()[0]).GetType() is ANamedType &&
                                        ((ANamedType)((AALocalDecl)aDecl1.GetFormals()[0]).GetType()).GetName().Text == "bool" &&

                                        ((AALocalDecl)aDecl1.GetFormals()[1]).GetType() is ANamedType &&
                                        ((ANamedType)((AALocalDecl)aDecl1.GetFormals()[1]).GetType()).GetName().Text == "bool")
                                {
                                    ATriggerDecl aDecl2 = (ATriggerDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                            "Matching method declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching trigger declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                "A method and a trigger of same signature is visible from " +
                                                                                node.GetName().Text, false,
                                                                                subErrors.ToArray()));
                                    }
                                    
                                }
                                else if (decl2 is AStructDecl)
                                {
                                    AStructDecl aDecl2 = (AStructDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;
                                    if (aDecl1.GetDelegate() != null && aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching delegate declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching struct declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two types of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                            }
                            if (decl1 is ATriggerDecl)
                            {
                                ATriggerDecl aDecl1 = (ATriggerDecl)decl1;

                                if (decl2 is ATriggerDecl)
                                {
                                    ATriggerDecl aDecl2 = (ATriggerDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching trigger declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching trigger declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "Two triggers of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is AMethodDecl)
                                {
                                    AMethodDecl aDecl2 = (AMethodDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl2.GetFormals().Count == 2 &&

                                        ((AALocalDecl)aDecl2.GetFormals()[0]).GetType() is ANamedType &&
                                        ((ANamedType)((AALocalDecl)aDecl2.GetFormals()[0]).GetType()).GetName().Text == "bool" &&

                                        ((AALocalDecl)aDecl2.GetFormals()[1]).GetType() is ANamedType &&
                                        ((ANamedType)((AALocalDecl)aDecl2.GetFormals()[1]).GetType()).GetName().Text == "bool")
                                    {

                                        if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                        {
                                            List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                            subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching trigger declaration"));
                                            subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                    "Matching method declaration"));
                                            errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                                 "A method and a trigger of same signature is visible from " +
                                                                                 node.GetName().Text, false,
                                                                                 subErrors.ToArray()));
                                        }
                                    }
                                }
                                else if (decl2 is AFieldDecl)
                                {
                                    AFieldDecl aDecl2 = (AFieldDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching trigger declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching field declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A field and a trigger of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                                else if (decl2 is APropertyDecl)
                                {
                                    APropertyDecl aDecl2 = (APropertyDecl)decl2;
                                    if (aDecl2.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(file1, file2))
                                        continue;

                                    if (aDecl1.GetName().Text == aDecl2.GetName().Text)
                                    {
                                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                        subErrors.Add(new ErrorCollection.Error(aDecl1.GetName(), file1,
                                                                                "Matching trigger declaration"));
                                        subErrors.Add(new ErrorCollection.Error(aDecl2.GetName(), file2,
                                                                                "Matching property declaration"));
                                        errors.Add(new ErrorCollection.Error(new TIdentifier("", 1, 1), node,
                                                                             "A property and a trigger of same name is visible from " +
                                                                             node.GetName().Text, false,
                                                                             subErrors.ToArray()));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //You are implementing short trigger notation. You can not change them to methods before after type checking, to avoid ambigious invokes..?
            //It's not allowed to have ambigious invokes with a trigger anyway.. so just change it in the weeder
            base.CaseAASourceFile(node);*/
        }


        public override void OutAMethodDecl(AMethodDecl node)
        {
            if (node.GetDelegate() != null)
            {
                if (GalaxyKeywords.Primitives.words.Contains(node.GetName().Text))
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentFile,
                                                         "A delegate can not be called the same name as a primitive."));
                //node.Parent().RemoveChild(node);
            }
            
            base.OutAMethodDecl(node);
        }

        public override void  OutATriggerDecl(ATriggerDecl node)
        {
            //Replace with 
            ProcessActionBlock actionProcesser = new ProcessActionBlock(errors);
            ProcessCondBlock condProcesser = new ProcessCondBlock();
            node.GetActions().Apply(actionProcesser);
            if (node.GetConditions() != null)
                node.GetConditions().Apply(condProcesser);
            AALocalDecl testCondsDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new AAName(new ArrayList() {new TIdentifier("bool")})), new TIdentifier("testConds"), null);
            AALocalDecl runActionsDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new AAName(new ArrayList() {new TIdentifier("bool")})), new TIdentifier("runActions"), null);
            AMethodDecl triggerMethod = new AMethodDecl((PVisibilityModifier) node.GetVisibilityModifier().Clone(), new TTrigger("trigger"), null, null, null, null,
                                                        new ANamedType(new AAName(new ArrayList() {new TIdentifier("bool")})),
                                                        (TIdentifier) node.GetName().Clone(),
                                                        new ArrayList() {testCondsDecl, runActionsDecl},
                                                        null);

           
            AABlock triggerMethodBlock = new AABlock();
            string name;
            int i;
            if (node.GetConditions() != null)
            {
                AABlock condBlock = new AABlock();

                data.Locals[condBlock] = new List<AALocalDecl>();
                if (condProcesser.readReturned)
                {
                    //Find a sutable name for the returned variable
                    name = "condReturned";
                    List<AALocalDecl> definedLocals = new List<AALocalDecl>();
                    foreach (AABlock block in condProcesser.blocks)
                    {
                        definedLocals.AddRange(data.Locals[block]);
                    }
                    i = 1;
                    while (definedLocals.Any(l => l.GetName().Text == name))
                    {
                        i++;
                        name = "condReturned" + i;
                    }
                    foreach (AAmbiguousNameLvalue lvalue in condProcesser.returnerRefferences)
                    {
                        lvalue.SetAmbiguous(new AAName(new ArrayList(){new TIdentifier(name)}));
                    }

                    AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                            new ANamedType(new AAName(new ArrayList() {
                                                                new TIdentifier("bool")})),
                                                            new TIdentifier(name),
                                                            new ABooleanConstExp(
                                                                new AFalseBool()));
                    condBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"),localDecl));
                    data.Locals[condBlock].Add(localDecl);
                }
                condBlock.GetStatements().Add(new AWhileStm(new TLParen("("), new ABooleanConstExp(new ATrueBool()),
                                                            new ABlockStm(new TLBrace("{"), node.GetConditions())));
                triggerMethodBlock.GetStatements().Add(new AIfThenStm(new TLParen("("),
                                                                      new ALvalueExp(
                                                                          new AAmbiguousNameLvalue(
                                                                              new AAName(new ArrayList() {
                                                                                  new TIdentifier("testConds")}))),
                                                                      new ABlockStm(new TLBrace("{"), condBlock)));
                //triggerMethodBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), condBlock));
            }

            //Add if (!runActions){return true;}
            triggerMethodBlock.GetStatements().Add(new AIfThenStm(new TLParen("("),
                                                                  new AUnopExp(
                                                                      new AComplementUnop(new TComplement("!")),
                                                                      new ALvalueExp(
                                                                          new AAmbiguousNameLvalue(
                                                                              new AAName(new ArrayList() {
                                                                                  new TIdentifier("runActions")})))),
                                                                  new ABlockStm(new TLBrace("{"),
                                                                                new AABlock(
                                                                                    new ArrayList()
                                                                                        {
                                                                                            new AValueReturnStm(
                                                                                                new TReturn("return"),
                                                                                                new ABooleanConstExp(
                                                                                                    new ATrueBool()))
                                                                                        },
                                                                                    new TRBrace("}")))));

            triggerMethodBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), node.GetActions()));

            data.Locals[triggerMethodBlock] = new List<AALocalDecl>(){testCondsDecl, runActionsDecl};

            triggerMethod.SetBlock(triggerMethodBlock);

            //Do event block
            AFieldDecl triggerVarDecl = new AFieldDecl((PVisibilityModifier)node.GetVisibilityModifier().Clone(), null, null, new ANamedType(new TIdentifier("trigger"), null), (TIdentifier)node.GetName().Clone(), null);
            

            AABlock eventBlock = new AABlock();
            AStringConstExp triggerRef = new AStringConstExp(new TStringLiteral("\"" + triggerMethod.GetName().Text + "\""));
            data.TriggerDeclarations.Add(triggerMethod, new List<TStringLiteral>(){triggerRef.GetStringLiteral()});
            eventBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"),
                                                       new AAssignmentExp(new TAssign("="),
                                                                          new AFieldLvalue(
                                                                              new TIdentifier(
                                                                                  triggerVarDecl.GetName().Text)),
                                                                          new ASimpleInvokeExp(
                                                                              new TIdentifier("TriggerCreate"),
                                                                              new ArrayList() {triggerRef}))));
            if (node.GetEvents() != null)
                eventBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), node.GetEvents()));

            name = node.GetName().Text + "Init";
            i = 1;
            while (data.Methods.Any(m => m.Decl.GetName().Text == name))
            {
                i++;
                name = node.GetName().Text + "Init" + i;
            }
            AMethodDecl eventDecl = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                    new TIdentifier(name), new ArrayList(), eventBlock);
            data.InvokeOnIniti.Add(eventDecl);

            AASourceFile srcFile = Util.GetAncestor<AASourceFile>(node);
            if (node.Parent() == srcFile)
            {
                srcFile.GetDecl().Insert(srcFile.GetDecl().IndexOf(node), triggerVarDecl);
                srcFile.GetDecl().Insert(srcFile.GetDecl().IndexOf(node), triggerMethod);
                srcFile.GetDecl().Insert(srcFile.GetDecl().IndexOf(node), eventDecl);
                srcFile.RemoveChild(node);
            }
            else
            {
                ANamespaceDecl parent = (ANamespaceDecl)node.Parent();
                parent.GetDecl().Insert(parent.GetDecl().IndexOf(node), triggerVarDecl);
                parent.GetDecl().Insert(parent.GetDecl().IndexOf(node), triggerMethod);
                parent.GetDecl().Insert(parent.GetDecl().IndexOf(node), eventDecl);
                parent.RemoveChild(node);
            }
            data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(srcFile, triggerVarDecl));
            data.UserFields.Add(triggerVarDecl);
            data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(srcFile, triggerMethod));
            data.UserMethods.Add(triggerMethod);

            data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(srcFile, eventDecl));
            data.Triggers.Remove(node);
        }

        private class ProcessActionBlock : DepthFirstAdapter
        {
            //Must return void. Replace with return true;
            ErrorCollection errors;
            List<AABlock> blocks = new List<AABlock>();

            public ProcessActionBlock(ErrorCollection errors)
            {
                this.errors = errors;
            }

            public override void OutAABlock(AABlock node)
            {
                blocks.Add(node);
            }

            public override void  CaseAVoidReturnStm(AVoidReturnStm node)
            {
 	            //Replace with return true
                node.ReplaceBy(new AValueReturnStm(node.GetToken(), new ABooleanConstExp(new ATrueBool())));
            }

            public override void  CaseAValueReturnStm(AValueReturnStm node)
            {
 	            //Return type is void.. this is silly
                errors.Add(new ErrorCollection.Error(node.GetToken(), Util.GetAncestor<AASourceFile>(node), "Only void return are allowed here."));
            }
        }

        private class ProcessCondBlock : DepthFirstAdapter
        {
            /*  return <exp>;//Do special stuff for return false and return true
             *  =>
             *  if (<exp>)
             *  {
             *      returned = true;//Only if neccerray.
             *      break;
             *  }
             *  else
             *  {
             *      return false;
             *  }
             * 
             *  while (...)
             *  {
             *      ...
             *  }
             *  =>
             *  while (...)
             *  {
             *      ...
             *  }
             *  if (returned)
             *  {
             *      break;
             *  }
             */ 

            //Phase 1: Insert everything needed above
            //Phase 2: if returned is never read, remove it

            public List<AABlock> blocks = new List<AABlock>();
            public List<AAmbiguousNameLvalue> returnerRefferences = new List<AAmbiguousNameLvalue>();
            private bool isFirstBlock = true;
            public bool readReturned;

            public override void CaseAABlock(AABlock node)
            {
                bool wasFirstBlock = isFirstBlock;
                if (isFirstBlock)
                {
                    //Insert break at the end
                    node.GetStatements().Add(new ABreakStm(new TBreak("break")));
                    isFirstBlock = false;
                }
                blocks.Add(node);
                base.CaseAABlock(node);
                if (wasFirstBlock)
                {
                    if (!readReturned)
                    {
                        foreach (AAmbiguousNameLvalue lvalue in returnerRefferences)
                        {
                            PStm pStm = Util.GetAncestor<PStm>(lvalue);
                            AABlock block = (AABlock) pStm.Parent();

                            block.RemoveChild(pStm);
                        }
                        returnerRefferences.Clear();
                    }
                }
            }

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                if (node.GetExp() is ABooleanConstExp)
                {
                    if (((ABooleanConstExp)node.GetExp()).GetBool() is ATrueBool)
                    {
                        //return true;
                        //replace with
                        //returned = true;
                        //break;
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="),
                                                                       new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier("condReturned"))),
                                                                       new ABooleanConstExp(new ATrueBool()));
                        ABreakStm breakStm = new ABreakStm(new TBreak("break"));

                        returnerRefferences.Add((AAmbiguousNameLvalue) assignment.GetLvalue());

                        AABlock block = (AABlock)node.Parent();
                        block.GetStatements().Insert(block.GetStatements().IndexOf(node), new AExpStm(new TSemicolon(";"), assignment));
                        block.GetStatements().Insert(block.GetStatements().IndexOf(node), breakStm);
                        block.RemoveChild(node);
                    }
                    else
                    {
                        //return false;
                        //Just leave it
                    }
                }
                else
                {
                    //Insert the big thing
                    AABlock thenBranch = new AABlock();
                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="),
                                                                       new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier("condReturned"))),
                                                                       new ABooleanConstExp(new ATrueBool()));
                    ABreakStm breakStm = new ABreakStm(new TBreak("break"));

                    returnerRefferences.Add((AAmbiguousNameLvalue)assignment.GetLvalue());

                    thenBranch.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));
                    thenBranch.GetStatements().Add(breakStm);

                    AABlock elseBranch = new AABlock();
                    elseBranch.GetStatements().Add(new AValueReturnStm(new TReturn("return"),
                                                                       new ABooleanConstExp(new AFalseBool())));

                    AIfThenElseStm ifThenElse = new AIfThenElseStm(new TLParen("("), node.GetExp(),
                                                                   new ABlockStm(new TLBrace("{"), thenBranch),
                                                                   new ABlockStm(new TLBrace("{"), elseBranch));
                    node.ReplaceBy(ifThenElse);
                }
            }

            public override void OutAWhileStm(AWhileStm node)
            {
                //Add the check for returned
                readReturned = true;

                AABlock innerBlock = new AABlock();
                innerBlock.GetStatements().Add(new ABreakStm(new TBreak("break")));
                AAmbiguousNameLvalue returnedRef =
                    new AAmbiguousNameLvalue(new ASimpleName(new TIdentifier("RENAMEME!")));
                returnerRefferences.Add(returnedRef);
                AIfThenStm ifThen = new AIfThenStm(new TLParen("("), new ALvalueExp(returnedRef), new ABlockStm(new TLBrace("{"), innerBlock));

                AABlock block = (AABlock) node.Parent();

                block.GetStatements().Insert(block.GetStatements().IndexOf(node) + 1, ifThen);
            }
        }

        public override void OutAAProgram(AAProgram node)
        {
            //Remove typedefs
            //foreach (ATypedefDecl typedef in data.Typedefs)
            //{
            //    typedef.Parent().RemoveChild(typedef);
            //}

            //Report string obfuscation warnings
            {
                List<ErrorCollection.Error> children = new List<ErrorCollection.Error>();
                for (int i = stringObfuscationErrors.Count - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        children.Reverse();
                        errors.Add(new ErrorCollection.Error(stringObfuscationErrors[i].GetStringLiteral(), Util.GetAncestor<AASourceFile>(stringObfuscationErrors[i]),
                                                                       "String obfuscation will not work in this context. I had some problems with triggers being initialized incorrectly.",
                                                                       true, children.ToArray()));
                    }
                    else
                    {
                        children.Add(new ErrorCollection.Error(stringObfuscationErrors[i].GetStringLiteral(), Util.GetAncestor<AASourceFile>(stringObfuscationErrors[i]),
                                                                               "Other field string", true));
                    }
                }
            }
            //Report non constant trigger create strings
            {
                List<ErrorCollection.Error> children = new List<ErrorCollection.Error>();
                for (int i = nonConstantTriggerCreates.Count - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        children.Reverse();
                        errors.Add(new ErrorCollection.Error(nonConstantTriggerCreates[i].GetName(), Util.GetAncestor<AASourceFile>(nonConstantTriggerCreates[i]),
                                                             "If you dont use an explicit string literal here, it will not be possible to rename triggers",
                                                             true, children.ToArray()));
                    }
                    else
                    {
                        children.Add(new ErrorCollection.Error(nonConstantTriggerCreates[i].GetName(), Util.GetAncestor<AASourceFile>(nonConstantTriggerCreates[i]),
                                                             "Other non literal TriggerCreate call",
                                                             true));
                    }
                }
            }


            Dictionary<AInitializerDecl, string> names = new Dictionary<AInitializerDecl, string>();
            Dictionary<AInitializerDecl, string> versions = new Dictionary<AInitializerDecl, string>();
            Dictionary<AInitializerDecl, List<string>> supportedVersions = new Dictionary<AInitializerDecl, List<string>>();
            Dictionary<AInitializerDecl, List<KeyValuePair<string, string>>> requiredLibraries = new Dictionary<AInitializerDecl, List<KeyValuePair<string, string>>>();
            //Check valid names and versions
            foreach (AInitializerDecl initializer in initializers)
            {
                string libraryName = "";
                string libraryVersion = "";
                List<string> suppVersions = new List<string>();
                List<KeyValuePair<string, string>> reqLibraries = new List<KeyValuePair<string, string>>();
                for (int i = 0; i < initializer.GetInitializerParam().Count; i++)
                {
                    PInitializerParam param = (PInitializerParam) initializer.GetInitializerParam()[i];
                
                    if (param is ALibraryNameInitializerParam)
                    {
                        ALibraryNameInitializerParam aParam = (ALibraryNameInitializerParam)param;
                        string[] strings = GetInitParamContents(aParam.GetName());
                        if (strings.Length > 1 || libraryName != "")
                        {
                            errors.Add(new ErrorCollection.Error(aParam.GetToken(), "You can only specify one library name."));
                        }
                        if (strings.Length == 0 || strings[0] == "")
                        {
                            errors.Add(new ErrorCollection.Error(aParam.GetToken(),  "Invalid library name."));
                            continue;
                        }
                        libraryName = strings[0];
                        continue;
                    }
                    if (param is ALibraryVersionInitializerParam)
                    {
                        ALibraryVersionInitializerParam aParam = (ALibraryVersionInitializerParam)param;
                        string[] strings = GetInitParamContents(aParam.GetName());
                        if (strings.Length > 1 || libraryVersion != "")
                        {
                            errors.Add(new ErrorCollection.Error(aParam.GetToken(), "You can only specify one library version."));
                        }
                        if (strings.Length == 0 || strings[0] == "")
                        {
                            errors.Add(new ErrorCollection.Error(aParam.GetToken(),"Invalid library version."));
                            continue;
                        }
                        libraryVersion = strings[0];
                        continue;
                    }
                    if (param is ASupportedVersionsInitializerParam)
                    {
                        ASupportedVersionsInitializerParam aParam = (ASupportedVersionsInitializerParam)param;
                        string[] strings = GetInitParamContents(aParam.GetName());
                        foreach (string s in strings)
                        {
                            if (s == "")
                                continue;
                            suppVersions.Add(s);
                        }
                        continue;
                    }
                    if (param is ARequiredLibrariesInitializerParam)
                    {
                        ARequiredLibrariesInitializerParam aParam = (ARequiredLibrariesInitializerParam)param;
                        string[] strings = GetInitParamContents(aParam.GetName());
                        foreach (string s in strings)
                        {
                            if (s == "")
                                continue;
                            if (!s.Contains(":"))
                            {
                                errors.Add(new ErrorCollection.Error(aParam.GetToken(), "Invalid format. It must be on the form \"(lib1:ver1),lib2:ver2,...\""));
                                continue;
                            }
                            string text = s.Replace("(", "").Replace(")", "");
                            int j = text.IndexOf(":");
                            string libName = text.Substring(0, j).Trim();
                            string version = text.Substring(j + 1).Trim();
                            if (libName == "" || version == "")
                            {
                                errors.Add(new ErrorCollection.Error(aParam.GetToken(), "Invalid format. It must be on the form \"(lib1:ver1),lib2:ver2,...\""));
                                continue;
                            }
                            reqLibraries.Add(new KeyValuePair<string, string>(libName, version));
                        }
                        continue;
                    }
                }
                names[initializer] = libraryName;
                versions[initializer] = libraryVersion;
                supportedVersions[initializer] = suppVersions;
                requiredLibraries[initializer] = reqLibraries;
            }

            //If current version is not on the list of supported versions, then add it
            foreach (KeyValuePair<AInitializerDecl, string> libVersion in versions)
            {
                if (!supportedVersions[libVersion.Key].Contains(libVersion.Value))
                    supportedVersions[libVersion.Key].Add(libVersion.Value);
            }

            //Check that all required libraries are there
            Dictionary<AInitializerDecl, List<AInitializerDecl>> dependancies = new Dictionary<AInitializerDecl, List<AInitializerDecl>>();
            foreach (AInitializerDecl initializer in initializers)
            {
                dependancies[initializer] = new List<AInitializerDecl>();
            }
            foreach (KeyValuePair<AInitializerDecl, List<KeyValuePair<string, string>>> requiredLibraryList in requiredLibraries)
            {
                for (int i = 0; i < requiredLibraryList.Value.Count; i++)
                {
                    KeyValuePair<string, string> requiredLibrary = requiredLibraryList.Value[i];
                
                    bool foundLib = false;
                    foreach (KeyValuePair<AInitializerDecl, string> library in names)
                    {
                        if (library.Value == requiredLibrary.Key)
                        {
                            if (supportedVersions[library.Key].Contains(requiredLibrary.Value))
                            {
                                /*if (!dependancies.ContainsKey(requiredLibraryList.Key))
                                    dependancies[requiredLibraryList.Key] = new List<AInitializerDecl>();*/
                                dependancies[requiredLibraryList.Key].Add(library.Key);
                                foundLib = true;
                                break;
                            }
                        }
                    }
                    if (foundLib)
                        continue;
                    //Didn't find the lib
                    errors.Add(new ErrorCollection.Error(requiredLibraryList.Key.GetToken(), "The library " + requiredLibrary.Key + ":" + requiredLibrary.Value + " is marked as required, but could not be found."));
                    requiredLibraryList.Value.RemoveAt(i);
                    i--;
                }
            }

            //Make methods
            Dictionary<AInitializerDecl, AMethodDecl> initalizerMethods = new Dictionary<AInitializerDecl, AMethodDecl>();
            foreach (AInitializerDecl initializer in initializers)
            {
                if (initializer.GetBody() == null)
                    continue;

                string methodName = (names[initializer] == "" ? "" : names[initializer] + "_") +
                                    (versions[initializer] == "" ? "" : versions[initializer] + "_") +
                                    "Init";
                //Remove every part of the name that is not a valid identifier
                //First letter may not be a number
                while ((methodName[0] >= '0' && methodName[0] <= '9') || !Util.IsIdentifierLetter(methodName[0]))
                {
                    methodName = methodName.Substring(1);
                }
                //The rest must be valid identifier letters
                for (int i = 1; i < methodName.Length; i++)
                {
                    if (!Util.IsIdentifierLetter(methodName[i]))
                    {
                        methodName = methodName.Remove(i, 1);
                        i--;
                    }
                }
                //The name may not clash with any existing methods
                int nr = 1;
                while (true)
                {
                    string name = nr == 1 ? methodName : (methodName + nr);
                    nr++;
                    if (data.Methods.Any(declItem => declItem.Decl.GetName().Text == name) ||
                        data.Libraries.Methods.Any(m => m.GetName().Text == name))
                    {
                        continue;
                    }
                    methodName = name;
                    break;
                }
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(), new TIdentifier(methodName), new ArrayList(), initializer.GetBody());
                initalizerMethods.Add(initializer, method);
            }

            //Remove cycles by removing as few dependancies as possible
            while (true)
            {
                //variable saying how many times a specific transition is part of a cycle
                Dictionary<KeyValuePair<AInitializerDecl, AInitializerDecl>, int> cycleElementCount = new Dictionary<KeyValuePair<AInitializerDecl, AInitializerDecl>, int>();
                List<AInitializerDecl> visited = new List<AInitializerDecl>();
                foreach (AInitializerDecl initializerDecl in dependancies.Select(d => d.Key))
                {
                    if (visited.Contains(initializerDecl))
                        continue;
                    LocateCycles(dependancies, cycleElementCount, visited, new List<AInitializerDecl>(), initializerDecl);
                }
                if (cycleElementCount.Count == 0)
                    break;//No cycles found
                int maxInt = 0;
                KeyValuePair<AInitializerDecl, AInitializerDecl> maxTransition = default(KeyValuePair<AInitializerDecl, AInitializerDecl>);
                foreach (KeyValuePair<KeyValuePair<AInitializerDecl, AInitializerDecl>, int> cycleCountElement in cycleElementCount)
                {
                    if (cycleCountElement.Value > maxInt)
                    {
                        maxInt = cycleCountElement.Value;
                        maxTransition = cycleCountElement.Key;
                    }
                }
                dependancies[maxTransition.Key].Remove(maxTransition.Value);
            }

            //Since there are no cycles, one of the dependancies is not needed by anyone else. Add it to the list of init functions and remove it from dependancies,
            //repeat untill no more dependancies
            while (dependancies.Count > 0)
            {
                AInitializerDecl next = null;
                foreach (AInitializerDecl initializer in dependancies.Select(item => item.Key))
                {
                    if (dependancies.Any(dependancy => dependancy.Value.Contains(initializer)))
                    {
                        continue;
                    }
                    next = initializer;
                    break;
                }
                if (next == null)
                    throw new Exception("You made an algorithmic error! See EnviromentChecking.OutAAProgram.");
                if (initalizerMethods.ContainsKey(next))
                    data.InitializerMethods.Insert(0, initalizerMethods[next]);
                dependancies.Remove(next);
            }

            foreach (AInitializerDecl initializer in initializers)
            {
                if (initalizerMethods.ContainsKey(initializer))
                {
                    AMethodDecl method = initalizerMethods[initializer];
                    data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(Util.GetAncestor<AASourceFile>(initializer), method));
                    data.UserMethods.Add(method);
                    initializer.ReplaceBy(method);
                }
                else
                    initializer.Parent().RemoveChild(initializer);
            }
            //todo: Make an Invoke to each init method in the order they are placed on the list.

            base.OutAAProgram(node);
        }

        private void LocateCycles(Dictionary<AInitializerDecl, List<AInitializerDecl>> dependancies, Dictionary<KeyValuePair<AInitializerDecl, AInitializerDecl>, int> cycleElementCount, List<AInitializerDecl> visited, List<AInitializerDecl> path, AInitializerDecl current)
        {
            if (path.Contains(current))
            {
                int i = path.IndexOf(current);

                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                for (int j = i; j < path.Count - 1; j++)
                {
                    subErrors.Add(new ErrorCollection.Error(path[j].GetToken(), "Member of cycle"));
                }
                errors.Add(new ErrorCollection.Error(path[i].GetToken(), "Cycle of libaray dependancies found. One of them will not have all it's dependancies initialized.", true, subErrors.ToArray()));

                path.Add(current);
                for (; i < path.Count - 1; i++)
                {
                    if (!cycleElementCount.Any(item => item.Key.Key == path[i] && item.Key.Value == path[i + 1]))
                        cycleElementCount.Add(new KeyValuePair<AInitializerDecl, AInitializerDecl>(path[i], path[i + 1]), 1);
                    else
                        foreach (KeyValuePair<KeyValuePair<AInitializerDecl, AInitializerDecl>, int> cycleCountElement in cycleElementCount)
                        {
                            if (cycleCountElement.Key.Key == path[i] && cycleCountElement.Key.Value == path[i + 1])
                            {
                                cycleElementCount[cycleCountElement.Key]++;
                                break;
                            }
                        }
                }
                path.RemoveAt(path.Count - 1);
            }
            if (visited.Contains(current))
                return;
            visited.Add(current);
            path.Add(current);
            foreach (AInitializerDecl dependancy in dependancies[current])
            {
                LocateCycles(dependancies, cycleElementCount, visited, path, dependancy);
            }
            path.Remove(current);
        }

        List<AInitializerDecl> initializers = new List<AInitializerDecl>();

        public override void CaseAInitializerDecl(AInitializerDecl node)
        {
            initializers.Add(node);
            base.CaseAInitializerDecl(node);
        }

        private static string[] GetInitParamContents(TStringLiteral literal)
        {
            string[] strings = literal.Text.Substring(1, literal.Text.Length - 2).Split(',');
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Trim();
            }
            return strings;
        }

        public override void OutAABlock(AABlock node)
        {
            List<AALocalDecl> reported = new List<AALocalDecl>();
            //Check that there are not 2 locals with the same name
            foreach (AALocalDecl localDecl in data.Locals[node])
            {
                AABlock block = node;
                while (block != null)
                {
                    foreach (AALocalDecl otherDecl in data.Locals[block])
                    {
                        if (reported.Contains(otherDecl))
                            continue;
                        if (otherDecl != localDecl && otherDecl.GetName().Text == localDecl.GetName().Text)
                        {
                            ErrorCollection.Error subError = new ErrorCollection.Error(otherDecl.GetName(), currentFile, "Other local variable");
                            errors.Add(new ErrorCollection.Error(localDecl.GetName(), currentFile, "You can not have two local varaiables within same scope who has the same name", false, subError));
                            reported.Add(localDecl);
                        }
                    }
                    block = Util.GetAncestor<AABlock>(block.Parent());
                }
            }
        }
        


        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (node.GetRef() != null && node.GetOut() != null)
            {//Cannot be both ref and out
                errors.Add(new ErrorCollection.Error(node.GetOut(), currentFile, "A paramter can not be marked as both ref and out."));
                node.SetOut(null);
            }
            
            base.CaseAALocalDecl(node);
        }

        public override void InAWhileStm(AWhileStm node)
        {
           
            //While statements must have blocks as bodies
            if (!(node.GetBody() is ABlockStm))
            {
                AABlock block = new AABlock();
                block.GetStatements().Add(node.GetBody());
                ABlockStm stm = new ABlockStm(null, block);
                node.SetBody(stm);
                data.Locals[block] = new List<AALocalDecl>();
            }

            base.InAWhileStm(node);
        }

        public override void CaseAIfThenStm(AIfThenStm node)
        {
            if (!(node.GetBody() is ABlockStm))
            {
                AABlock block = new AABlock();
                block.GetStatements().Add(node.GetBody());
                data.Locals.Add(block, new List<AALocalDecl>());
                ABlockStm stm = new ABlockStm(null, block);
                node.SetBody(stm);
            }
            base.CaseAIfThenStm(node);
        }

        public override void CaseAIfThenElseStm(AIfThenElseStm node)
        {
            if (!(node.GetThenBody() is ABlockStm))
            {
                AABlock block = new AABlock();
                block.GetStatements().Add(node.GetThenBody());
                data.Locals.Add(block, new List<AALocalDecl>());
                ABlockStm stm = new ABlockStm(null, block);
                node.SetThenBody(stm);
            }
            if (!(node.GetElseBody() is ABlockStm /*|| node.GetElseBody() is AIfThenStm || node.GetElseBody() is AIfThenElseStm*/))
            {
                AABlock block = new AABlock();
                block.GetStatements().Add(node.GetElseBody());
                data.Locals.Add(block, new List<AALocalDecl>());
                ABlockStm stm = new ABlockStm(null, block);
                node.SetElseBody(stm);
            }
            base.CaseAIfThenElseStm(node);
        }

        public override void CaseAFieldLvalue(AFieldLvalue node)
        {
            //If we got one of theese now, the user has typed global.identifer. It has to be in a method
            if (Util.GetAncestor<AMethodDecl>(node) == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentFile, "The keyword global can only be used inside methods.", false));
                AAmbiguousNameLvalue newLvalue = new AAmbiguousNameLvalue(new ASimpleName(node.GetName()));
                node.ReplaceBy(newLvalue);
                newLvalue.Apply(this);
                return;
            }
            base.CaseAFieldLvalue(node);
        }

        public override void OutANamedType(ANamedType node)
        {
            /*//Perhaps replace with typedef
            if (!(node.Parent() is ATypedefDecl && ((ATypedefDecl)node.Parent()).GetName() == node))
                foreach (ATypedefDecl typedef in data.Typedefs)
                {
                    if (Util.IsVisible(node, typedef) && node.GetName().Text == ((ANamedType)typedef.GetName()).GetName().Text)
                    {
                        PType clone = (PType) typedef.GetType().Clone();
                        node.ReplaceBy(clone);
                        clone.Apply(this);
                        return;
                    }
                }*/
            base.OutANamedType(node);
        }

        public override void CaseAStructFieldLvalue(AStructFieldLvalue node)
        {
            //If we got one of theese now, the user has typed struct.identifer. It has to be in a method
            if (Util.GetAncestor<AMethodDecl>(node) == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentFile, "When the keyword struct is used in this context, it has to be inside a method.", false));
                AAmbiguousNameLvalue newLvalue = new AAmbiguousNameLvalue(new ASimpleName(node.GetName()));
                node.ReplaceBy(newLvalue);
                newLvalue.Apply(this);
                return;
            }
            base.CaseAStructFieldLvalue(node);
        }

        private List<ASimpleInvokeExp> nonConstantTriggerCreates = new List<ASimpleInvokeExp>();
        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            //Mark relevant functions as triggers
            if (node.GetName().Text == "TriggerCreate" && node.GetArgs().Count == 1)
            {
                if (node.GetArgs()[0] is AStringConstExp)
                {
                    AStringConstExp arg = (AStringConstExp) node.GetArgs()[0];
                    TStringLiteral str = arg.GetStringLiteral();
                    string text = str.Text.Substring(1, str.Text.Length - 2);
                    List<string> targetNamespace = text.Split('.').ToList();
                    string name = targetNamespace[targetNamespace.Count - 1];
                    targetNamespace.RemoveAt(targetNamespace.Count - 1);

                    List<IList> visibleDecls = Util.GetVisibleDecls(node, targetNamespace.Count == 0);
                    if (targetNamespace.Count > 0)
                    {//Follow the namespaces
                        List<IList> nextVisibleDecls = new List<IList>();
                        while (targetNamespace.Count > 0)
                        {
                            string n = targetNamespace[0];
                            targetNamespace.RemoveAt(0);

                            foreach (IList declList in visibleDecls)
                            {
                                foreach (PDecl decl in declList)
                                {
                                    if (decl is ANamespaceDecl)
                                    {
                                        ANamespaceDecl aDecl = (ANamespaceDecl) decl;
                                        if (aDecl.GetName().Text == n)
                                            nextVisibleDecls.Add(aDecl.GetDecl());
                                    }
                                }
                            }
                            visibleDecls = nextVisibleDecls;
                            nextVisibleDecls = new List<IList>();
                        }
                    }

                    AASourceFile currentSourceFile = Util.GetAncestor<AASourceFile>(node);
                    List<string> currentNamespace = Util.GetFullNamespace(node);
                    List<AMethodDecl> matchingMethods = new List<AMethodDecl>();
                    foreach (IList declList in visibleDecls)
                    {
                        bool isSameFile = false;
                        if (declList.Count > 0)
                            isSameFile = currentSourceFile == Util.GetAncestor<AASourceFile>((PDecl) declList[0]);
                        foreach (PDecl decl in declList)
                        {
                            if (decl is AMethodDecl)
                            {
                                AMethodDecl aDecl = (AMethodDecl)decl;
                                if (aDecl.GetName().Text == name &&
                                    aDecl.GetFormals().Count == 2)
                                {
                                    PType[] booleanTypes = new []
                                                               {
                                                                   aDecl.GetReturnType(),
                                                                   ((AALocalDecl) aDecl.GetFormals()[0]).GetType(),
                                                                   ((AALocalDecl) aDecl.GetFormals()[1]).GetType()
                                                               };
                                    bool typesMatch = true;
                                    foreach (PType type in booleanTypes)
                                    {
                                        if (!(type is ANamedType))
                                        {
                                            typesMatch = false;
                                            break;
                                        }
                                        ANamedType aType = (ANamedType) type;
                                        AAName aName = (AAName) aType.GetName();
                                        if (aName.GetIdentifier().Count > 1 ||
                                            ((TIdentifier) aName.GetIdentifier()[0]).Text != "bool")
                                        {
                                            typesMatch = false;
                                            break;
                                        }
                                    }
                                    if (!typesMatch)
                                        continue;


                                    bool isSameNamespace = Util.NamespacesEquals(currentNamespace,
                                                                                 Util.GetFullNamespace(decl));
                                    if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                        !isSameFile && aDecl.GetStatic() == null ||
                                        aDecl.GetDelegate() != null)
                                        continue;
                                    matchingMethods.Add(aDecl);
                                }
                            }
                        }
                    }

                    if (matchingMethods.Count == 0)
                    {
                        errors.Add(new ErrorCollection.Error(node.GetName(), "Target trigger not found", true));
                    }
                    else if (matchingMethods.Count > 1)
                    {
                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                        foreach (AMethodDecl method in matchingMethods)
                        {
                            subErrors.Add(new ErrorCollection.Error(method.GetName(), "Matching method"));
                        }
                        errors.Add(new ErrorCollection.Error(node.GetName(), "Multiple target triggers found", true, subErrors.ToArray()));
                    }
                    else
                    {
                        AMethodDecl method = matchingMethods[0];
                        if (method.GetTrigger() == null)
                        {
                            method.SetTrigger(new TTrigger("trigger", method.GetName().Line, method.GetName().Pos));
                        }
                        if (!data.TriggerDeclarations.ContainsKey(method))
                            data.TriggerDeclarations[method] = new List<TStringLiteral>();
                        data.TriggerDeclarations[method].Add(str);
                    }
                }
                else
                {
                    data.HasUnknownTrigger = true;
                    if (Options.Compiler.MakeShortNames)
                        nonConstantTriggerCreates.Add(node);
                    //Mark anything that could be trigger as trigger
                    List<AMethodDecl> triggers = new List<AMethodDecl>();
                    foreach (SharedData.DeclItem<AMethodDecl> declItem in data.Methods)
                    {
                        AMethodDecl method = declItem.Decl;
                        if (method.GetTrigger() == null &&
                            method.GetFormals().Count == 2)
                        {
                            PType[] booleanTypes = new[]
                                                       {
                                                           method.GetReturnType(),
                                                           ((AALocalDecl) method.GetFormals()[0]).GetType(),
                                                           ((AALocalDecl) method.GetFormals()[1]).GetType()
                                                       };
                            bool typesMatch = true;
                            foreach (PType type in booleanTypes)
                            {
                                if (!(type is ANamedType))
                                {
                                    typesMatch = false;
                                    break;
                                }
                                ANamedType aType = (ANamedType)type;
                                AAName aName = (AAName)aType.GetName();
                                if (aName.GetIdentifier().Count > 1 ||
                                    ((TIdentifier)aName.GetIdentifier()[0]).Text != "bool")
                                {
                                    typesMatch = false;
                                    break;
                                }
                            }
                            if (typesMatch)
                                method.SetTrigger(new TTrigger("trigger", method.GetName().Line, method.GetName().Pos));
                        }
                        if (method.GetTrigger() != null)
                            triggers.Add(method);
                    }
                    //If we have two triggers with the same name, report an error
                    for (int i = 0; i < triggers.Count; i++)
                    {
                        for (int j = i + 1; j < triggers.Count; j++)
                        {
                            if (triggers[i].GetName().Text == triggers[j].GetName().Text)
                            {
                                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                                subErrors.Add(new ErrorCollection.Error(node.GetName(), currentFile, "TriggerCreate that does not have a constant literal"));
                                subErrors.Add(new ErrorCollection.Error(triggers[i].GetName(), Util.GetAncestor<AASourceFile>(triggers[i]), "Trigger" + triggers[i].GetName().Text));
                                subErrors.Add(new ErrorCollection.Error(triggers[i].GetName(), Util.GetAncestor<AASourceFile>(triggers[j]), "Trigger" + triggers[j].GetName().Text));

                                errors.Add(new ErrorCollection.Error(node.GetName(), currentFile, "You have a non constant TriggerCreate. This prevent the compiler from renaming triggers. You also have two triggers with the same name though.", false, subErrors.ToArray()), true);
                               
                            }
                        }
                    }
                }
            }
            
            
            base.CaseASimpleInvokeExp(node);
        }

        private List<AStringConstExp> stringObfuscationErrors = new List<AStringConstExp>();
        public override void OutAStringConstExp(AStringConstExp node)
        {
            /*if (Options.Compiler.ObfuscateStrings && Util.GetAncestor<PStm>(node) == null/* && Util.GetAncestor<ASimpleInvokeExp>(node) != null* /)
            {
                stringObfuscationErrors.Add(node);
                if (stringObfuscationError == null)
                {
                    stringObfuscationError = new ErrorCollection.Error(node.GetStringLiteral(), currentFile,
                                                                       "String obfuscation will only work for strings inside methods. I had some problems with fields being initialized incorrectly.",
                                                                       true);
                    errors.Add(stringObfuscationError);
                    
                }
                else
                {
                    stringObfuscationError.AddChild(new ErrorCollection.Error(node.GetStringLiteral(), currentFile,
                                                                               "Other field string"));
                }
                
            }*/
            base.OutAStringConstExp(node);
        }

        private bool dynamicType;
        public override void CaseAPointerType(APointerType node)
        {
            bool oldDynamicType = dynamicType;
            dynamicType = true;
            base.CaseAPointerType(node);
            dynamicType = oldDynamicType;
        }

        public override void CaseADynamicArrayType(ADynamicArrayType node)
        {
            //This may not be in an instance creation exp
            ANewExp newExp = Util.GetAncestor<ANewExp>(node);
            if (newExp != null)
            {
                ACastExp castExp = Util.GetAncestor<ACastExp>(node);
                if (castExp == null || !Util.IsAncestor(castExp, newExp))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), currentFile, "You must specify the dimentions of the array here"));
                }
            }
            if (!(node.Parent() is ANewExp))
            {
                //Insert pointer if missing
                if (!(node.Parent() is APointerType))
                {
                    APointerType pType = new APointerType(new TStar("*"), null);
                    node.ReplaceBy(pType);
                    pType.SetType(node);
                    pType.Apply(this);
                    return;
                }
            }

            bool oldDynamicType = dynamicType;
            dynamicType = true;
            base.CaseADynamicArrayType(node);
            dynamicType = oldDynamicType;
        }

        public override void CaseAArrayTempType(AArrayTempType node)
        {
            if (Util.GetAncestor<ANewExp>(node) != null && !(node.Parent() is ANewExp) || dynamicType)
            {
                //Insert pointer if missing
                if (!(node.Parent() is APointerType))
                {
                    APointerType pType = new APointerType(new TStar("*"), null);
                    node.ReplaceBy(pType);
                    pType.SetType(node);
                    pType.Apply(this);
                    return;
                }
            }
            base.CaseAArrayTempType(node);
        }

        /*private static Token GetToken(int list, int index, EnviromentBuilding.Data data)
        {
            switch (list)
            {
                case Decls.fields:
                    return data.fields[index].GetName();
                    break;
                case Decls.methods:
                    return data.methods[index].GetName();
                    break;
                case Decls.structs:
                    return data.structs[index].GetName();
                    break;
                case Decls.primitives:
                    return new TIdentifier(Form1.inst.primitives[index], -1, 0);
            }
            throw new Exception("(EnviromentChecking) Internal error - unexpected list index");
        }

        private static List<string> GetNames(List<AMethodDecl> methods)
        {
            return methods.Select(method => method.GetName().Text).ToList();
        }

        private static List<string> GetNames(List<AFieldDecl> methods)
        {
            return methods.Select(method => method.GetName().Text).ToList();
        }

        private static List<string> GetNames(List<AStructDecl> methods)
        {
            return methods.Select(method => method.GetName().Text).ToList();
        }

        private static List<string> GetNames(List<AALocalDecl> methods)
        {
            return methods.Select(method => method.GetName().Text).ToList();
        }*/
    }
}
