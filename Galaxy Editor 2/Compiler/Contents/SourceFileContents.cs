using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;
using SharedClasses;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class SourceFileContents : IDeclContainer
    {
        public bool IsDialogDesigner;
        public delegate string GetSourceDelegate();
        public delegate void SourceFileChangedEventHandler(SourceFileContents sender);

        public event SourceFileChangedEventHandler SourceFileChanged;

        public GetSourceDelegate GetSource;
        public Library Library;
        public DirItem Item;
        public List<List<string>> Usings { get; private set; }
        public List<MethodDescription> Methods { get; private set; }
        public List<VariableDescription> Fields { get; private set; }
        public List<StructDescription> Structs { get; private set; }
        public List<EnrichmentDescription> Enrichments { get; private set; }
        public List<TypedefDescription> Typedefs { get; private set; }
        public List<NamespaceDescription> Namespaces { get; private set; }
        public string Namespace;

        public SourceFileContents File
        {
            get { return this; }
        }

        public List<string> NamespaceList
        {
            get { return new List<string>(); }
        }

        public string FullName
        {
            get { return Namespace; }
        }

        private GalaxyCompiler compiler;

        public SourceFileContents()
        {
            Usings = new List<List<string>>();
            Methods = new List<MethodDescription>();
            Fields = new List<VariableDescription>();
            Structs = new List<StructDescription>();
            Enrichments = new List<EnrichmentDescription>();
            Typedefs = new List<TypedefDescription>();
            Namespaces = new List<NamespaceDescription>();
        }

        public List<IDeclContainer> GetVisibleDecls(bool includeUsings)
        {
            return GetVisibleDecls(new List<string>(), includeUsings);
        }

        public IDeclContainer GetDeclContainerAt(int line)
        {
            foreach (NamespaceDescription ns in Namespaces)
            {
                if (ns.LineFrom <= line && ns.LineTo >= line)
                    return ns.GetDeclContainerAt(line);
            }
            return this;
        }

        public List<IDeclContainer> GetFileDecls()
        {
            List<IDeclContainer> returner = new List<IDeclContainer>();
            returner.Add(this);
            foreach (NamespaceDescription ns in Namespaces)
            {
                Visit(ns, returner);
            }
            return returner;
        }

        private void Visit(NamespaceDescription ns, List<IDeclContainer> returner)
        {
            returner.Add(ns);
            foreach (NamespaceDescription namespaceDescription in ns.Namespaces)
            {
                Visit(namespaceDescription, returner);
            }
        }

        public List<IDeclContainer> GetMatchingDecls(List<string> ns)
        {
            if (ns.Count == 0)
                return GetVisibleDecls(true);
            List<IDeclContainer> returner = new List<IDeclContainer>();
            foreach (SourceFileContents sourceFile in compiler.ParsedSourceFiles)
            {
                foreach (NamespaceDescription namespaceDescription in sourceFile.Namespaces)
                {
                    Visit(namespaceDescription, new List<List<string>>(){ns}, returner);
                }
            }
            return returner;
        }
        
        public List<IDeclContainer> GetVisibleDecls(List<string> currentNamespace, List<string> targetNamespace)
        {
            List<IDeclContainer> visibleDecls = GetVisibleDecls(currentNamespace, targetNamespace.Count == 0);
            List<IDeclContainer> newList;
            foreach (string n in targetNamespace)
            {
                newList = new List<IDeclContainer>();
                foreach (IDeclContainer visibleDecl in visibleDecls)
                {
                    foreach (NamespaceDescription ns in visibleDecl.Namespaces)
                    {
                        if (ns.Name == n)
                        {
                            newList.Add(ns);
                        }
                    }
                }
                visibleDecls = newList;
            }
            return visibleDecls;
        }

        public List<IDeclContainer> GetVisibleDecls(List<string> currentNamespace, bool includeUsings)
        {
            List<IDeclContainer> returner = new List<IDeclContainer>();
            List<List<string>> usings = new List<List<string>>();
            if (currentNamespace.Count > 0)
                usings.Add(currentNamespace);
            if (includeUsings)
            {
                foreach (List<string> list in Usings)
                {
                    usings.Add(list);
                }
            }
            if (compiler == null)
                compiler = Form1.Form.compiler;
            foreach (SourceFileContents sourceFile in compiler.ParsedSourceFiles)
            {
                returner.Add(sourceFile);
                foreach (NamespaceDescription ns in sourceFile.Namespaces)
                {
                    Visit(ns, usings, returner);
                }
            }
            return returner;
        }

        private void Visit(NamespaceDescription ns, List<List<string>> usings, List<IDeclContainer> returner)
        {
            bool prefix = false;
            bool match = false;
            List<string> currentNS = ns.GetFullNamespace();
            foreach (List<string> list in usings)
            {
                if (Util.NamespacePrefix(currentNS, list))
                {
                    prefix = true;
                    if (Util.NamespacesEquals(currentNS, list))
                    {
                        match = true;
                        break;
                    }
                }
            }
            if (match)
                returner.Add(ns);
            if (prefix)
            {
                foreach (NamespaceDescription namespaceDescription in ns.Namespaces)
                {
                    Visit(namespaceDescription, usings, returner);
                }
            }
        }

        public void Clear()
        {
            //Includes.Clear();
            Methods.Clear();
            Fields.Clear();
            Structs.Clear();
            Typedefs.Clear();
            Enrichments.Clear();
            Usings.Clear();
            Namespaces.Clear();
        }

        public List<StructDescription> GetAllStructs()
        {
            List<StructDescription> structs = new List<StructDescription>();
            List<IDeclContainer> list = new List<IDeclContainer>();
            list.Add(this);
            while (list.Count > 0)
            {
                List<IDeclContainer> nextList = new List<IDeclContainer>();
                foreach (IDeclContainer declContainer in list)
                {
                    structs.AddRange(declContainer.Structs);
                    nextList.AddRange(declContainer.Namespaces);
                }
                list = nextList;
            }
            return structs;
        }

        public bool Parse(Start ast, GalaxyCompiler compiler)
        {
            this.compiler = compiler;
            Parser parser = new Parser(ast);
            //Includes = parser.Includes;
            //To save time, only update if there has been changes
            //I assume that the Elements are in the same order


            //Check if recouluring is needed
            List<StructDescription> preUpdateStructs = GetAllStructs();

            List<StructDescription> postUpdateStructs = new List<StructDescription>();
            postUpdateStructs.AddRange(parser.Structs);
            List<IDeclContainer> list = new List<IDeclContainer>();
            list.AddRange(parser.Namespaces);
            while (list.Count > 0)
            {
                List<IDeclContainer> nextList = new List<IDeclContainer>();
                foreach (IDeclContainer declContainer in list)
                {
                    postUpdateStructs.AddRange(declContainer.Structs);
                    nextList.AddRange(declContainer.Namespaces);
                }
                list = nextList;
            }

            bool restyle = preUpdateStructs.Count != postUpdateStructs.Count;
            if (!restyle)
            {
                for (int i = 0; i < preUpdateStructs.Count; i++)
                {
                    if (preUpdateStructs[i].Name != postUpdateStructs[i].Name)
                    {
                        restyle = true;
                        break;
                    }
                }
            }


            if (restyle && Form1.Form.CurrentOpenFile != null && Form1.Form.CurrentOpenFile.OpenFile != null)
                Form1.Form.CurrentOpenFile.OpenFile.Editor.Restyle();
                

            bool needUpdate = Methods.Count != parser.Methods.Count ||
                              Fields.Count != parser.Fields.Count ||
                              Structs.Count != parser.Structs.Count ||
                              Enrichments.Count != parser.Enrichments.Count ||
                              Usings.Count != parser.Usings.Count ||
                              Typedefs.Count != parser.Typedefs.Count ||
                              Namespaces.Count != parser.Namespaces.Count;



            if (!needUpdate)
            {
                needUpdate = Methods.Where((t, i) => !t.Equals(parser.Methods[i])).Any() ||
                             Fields.Where((t, i) => !t.Equals(parser.Fields[i])).Any() ||
                             Structs.Where((t, i) => !t.Equals(parser.Structs[i])).Any() ||
                             Enrichments.Where((t, i) => !t.Equals(parser.Enrichments[i])).Any() ||
                             Usings.Where((t, i) => t != parser.Usings[i]).Any() ||
                             Typedefs.Where((t, i) => !t.Equals(parser.Typedefs[i])).Any() ||
                             Namespaces.Where((t, i) => !t.Equals(parser.Namespaces[i])).Any();
            }

            if (needUpdate)
            {
                Methods = parser.Methods;
                foreach (MethodDescription method in Methods)
                {
                    method.ParentFile = this;
                }
                Fields = parser.Fields;
                foreach (VariableDescription field in Fields)
                {
                    field.ParentFile = this;
                }
                Structs = parser.Structs;
                foreach (StructDescription structDescription in Structs)
                {
                    foreach (MethodDescription method in structDescription.Methods)
                    {
                        method.ParentFile = this;
                    }
                    foreach (VariableDescription field in structDescription.Fields)
                    {
                        field.ParentFile = this;
                    }
                    structDescription.ParentFile = this;
                }
                Enrichments = parser.Enrichments;
                foreach (EnrichmentDescription structDescription in Enrichments)
                {
                    structDescription.ParentFile = this;
                    foreach (MethodDescription method in structDescription.Methods)
                    {
                        method.ParentFile = this;
                    }
                    foreach (VariableDescription field in structDescription.Fields)
                    {
                        field.ParentFile = this;
                    }
                }
                Typedefs = parser.Typedefs;
                foreach (TypedefDescription typedef in Typedefs)
                {
                    typedef.ParentFile = this;
                }
                Usings = parser.Usings;
                Namespaces = parser.Namespaces;
                foreach (NamespaceDescription namespaceDescription in Namespaces)
                {
                    namespaceDescription.Parent = this;
                }
                if (SourceFileChanged != null) SourceFileChanged(this);
            }
            return needUpdate;
        }

        public class Parser : DepthFirstAdapter
        {
            //public List<SourceFileContents> Includes = new List<SourceFileContents>();
            public List<MethodDescription> Methods = new List<MethodDescription>();
            public List<VariableDescription> Fields = new List<VariableDescription>();
            public List<StructDescription> Structs = new List<StructDescription>();
            public List<EnrichmentDescription> Enrichments = new List<EnrichmentDescription>();
            public List<TypedefDescription> Typedefs = new List<TypedefDescription>();
            public List<NamespaceDescription> Namespaces = new List<NamespaceDescription>();
            public List<List<string>> Usings = new List<List<string>>();
            private bool isFirst = true;

            public override void DefaultIn(Node node)
            {
                isFirst = false;
            }

            public Parser(Node ast)
            {
                ast.Apply(this);
            }

            /*public override void OutAIncludeDecl(AIncludeDecl node)
            {
                string name = Util.GetString(node.GetName());
                SourceFileContents sourceFile = compiler.LookupFile(name);
                if (sourceFile != null)
                {
                    Includes.Add(sourceFile);
                }
            }*/

            public override void OutATriggerDecl(ATriggerDecl node)
            {
                //Add field
                Fields.Add(new VariableDescription(node));
                //Add event method
                //The methods don't start from node.GetName(), make the parser add the events, conditions and action tokens
                if (node.GetEvents() != null)
                {
                    Methods.Add(
                        new MethodDescription(TextPoint.FromCompilerCoords(node.GetEventToken().Line, node.GetEventToken().Pos),
                                              new AVoidType(new TVoid("void")), (AABlock) node.GetEvents(), null));
                }
                if (node.GetConditions() != null)
                {
                    Methods.Add(
                        new MethodDescription(TextPoint.FromCompilerCoords(node.GetConditionsToken().Line, node.GetConditionsToken().Pos),
                                              new AVoidType(new TVoid("void")), (AABlock)node.GetConditions(), null));
                }
                //Actions. this one can be called
                {
                    AMethodDecl method = new AMethodDecl((PVisibilityModifier) node.GetVisibilityModifier().Clone(), null, null, null, null, null,
                                                         new ANamedType(new TIdentifier("bool"), null),
                                                         new TIdentifier(node.GetName().Text, node.GetActionsToken().Line, node.GetActionsToken().Pos), 
                                                         new ArrayList()
                                                             {
                                                                 new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                 new ANamedType(
                                                                                     new TIdentifier("bool"), null),
                                                                                 new TIdentifier("testConds"), null),
                                                                 new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                 new ANamedType(
                                                                                     new TIdentifier("bool"), null),
                                                                                 new TIdentifier("runActions"), null)
                                                             },
                                                         node.GetActions());
                    Methods.Add(new MethodDescription(method));
                }

            }

            public override void CaseANamespaceDecl(ANamespaceDecl node)
            {
                if (isFirst)
                {
                    base.CaseANamespaceDecl(node);
                    return;
                }
                NamespaceDescription ns = new NamespaceDescription(node);
                Namespaces.Add(ns);
            }

            public override void InAUsingDecl(AUsingDecl node)
            {
                List<string> ns = new List<string>();
                foreach (TIdentifier identifier in node.GetNamespace())
                {
                    ns.Add(identifier.Text);
                }
                foreach (List<string> list in Usings)
                {
                    if (list.Count != ns.Count)
                        continue;
                    bool equal = true;
                    for (int i = 0; i < ns.Count; i++)
                    {
                        if (list[i] != ns[i])
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                        return;
                }
                Usings.Add(ns);
            }

            public override void OutAPropertyDecl(APropertyDecl node)
            {
                VariableDescription variable;
                List<MethodDescription> methods = new List<MethodDescription>();
                PropertyDescription.CreateItems(node, methods, out variable);
                foreach (MethodDescription method in methods)
                {
                    if (inEnrichment)
                    {
                        method.Name = "";
                        method.Decl = null;
                    }
                    Methods.Add(method);
                }
                if (!inEnrichment && variable != null)
                    Fields.Add(variable);
            }

            public override void OutATypedefDecl(ATypedefDecl node)
            {
                TypedefDescription typedef = new TypedefDescription(node);
                Typedefs.Add(typedef);
            }

            public override void OutAMethodDecl(AMethodDecl node)
            {
                MethodDescription method = new MethodDescription(node);
                if (inEnrichment) 
                {
                    method.Name = "";
                    method.Decl = null;
                }
                Methods.Add(method);
            }

            public override void OutAInitializerDecl(AInitializerDecl node)
            {
                MethodDescription method = new MethodDescription(node);
                Methods.Add(method);
            }

            public override void OutAFieldDecl(AFieldDecl node)
            {
                VariableDescription field = new VariableDescription(node);
                Fields.Add(field);
            }

            public override void CaseAStructDecl(AStructDecl node)
            {
                StructDescription structDescription = new StructDescription(node);
                Structs.Add(structDescription);
            }

            private bool inEnrichment;
            public override void CaseAEnrichmentDecl(AEnrichmentDecl node)
            {
                EnrichmentDescription enrichmentDescription = new EnrichmentDescription(node);
                Enrichments.Add(enrichmentDescription);
                //inEnrichment = true;
                //base.CaseAEnrichmentDecl(node);
                //inEnrichment = false;
            }
        }
    }
}
