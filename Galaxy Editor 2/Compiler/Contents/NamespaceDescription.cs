using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class NamespaceDescription : IDeclContainer, SuggestionBoxItem
    {
        public IDeclContainer Parent;
        private ANamespaceDecl decl;
        public List<List<string>> Usings { get; private set; }
        public List<MethodDescription> Methods { get; private set; }
        public List<VariableDescription> Fields { get; private set; }
        public List<StructDescription> Structs { get; private set; }
        public List<EnrichmentDescription> Enrichments { get; private set; }
        public List<TypedefDescription> Typedefs { get; private set; }
        public List<NamespaceDescription> Namespaces { get; private set; }
        public IDeclContainer ParentFile { get { return Parent; } }
        public TextPoint Position { get; private set; }

        public string Name { get { return decl.GetName().Text; } }

        public SourceFileContents File
        {
            get { return Parent.File; }
        }

        public List<string> NamespaceList
        {
            get {
                List<string> s = Parent.NamespaceList;
                s.Add(Name);
                return s;
            }
        }

        public string FullName
        {
            get
            {
                List<string> list = NamespaceList;
                string s = "";
                foreach (string s1 in list)
                {
                    if (s != "")
                        s += ".";
                    s += s1;
                }
                return s;
            }
        }

        public int LineFrom, LineTo;

        public NamespaceDescription(ANamespaceDecl ns)
        {
            decl = ns;
            LineFrom = decl.GetToken().Line;
            if (decl.GetEndToken() == null)
                LineTo = int.MaxValue;
            else
                LineTo = decl.GetEndToken().Line;
            SourceFileContents.Parser parser = new SourceFileContents.Parser(ns);

            //if (Structs.Count != parser.Structs.Count && Form1.Form.CurrentOpenFile != null && Form1.Form.CurrentOpenFile.OpenFile != null)
              //  Form1.Form.CurrentOpenFile.OpenFile.Editor.Restyle();

            
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
            Position = TextPoint.FromCompilerCoords(ns.GetName());
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

        public List<string> GetFullNamespace()
        {
            List<string> returner = new List<string>();
            if (Parent is NamespaceDescription)
                returner.AddRange(((NamespaceDescription)Parent).GetFullNamespace());
            returner.Add(decl.GetName().Text);
            return returner;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NamespaceDescription))
                return false;
            NamespaceDescription other = (NamespaceDescription) obj;

            if (decl.GetName().Text != other.decl.GetName().Text)
                return false;

            if (Methods.Count != other.Methods.Count ||
                Fields.Count != other.Fields.Count ||
                Structs.Count != other.Structs.Count ||
                Enrichments.Count != other.Enrichments.Count ||
                Usings.Count != other.Usings.Count ||
                Typedefs.Count != other.Typedefs.Count ||
                Namespaces.Count != other.Namespaces.Count)
                return false;

            return !(Methods.Where((t, i) => !t.Equals(other.Methods[i])).Any() ||
                        Fields.Where((t, i) => !t.Equals(other.Fields[i])).Any() ||
                        Structs.Where((t, i) => !t.Equals(other.Structs[i])).Any() ||
                        Enrichments.Where((t, i) => !t.Equals(other.Enrichments[i])).Any() ||
                        Usings.Where((t, i) => t != other.Usings[i]).Any() ||
                        Typedefs.Where((t, i) => !t.Equals(other.Typedefs[i])).Any() ||
                        Namespaces.Where((t, i) => !t.Equals(other.Namespaces[i])).Any());
        }

        public string DisplayText
        {
            get { return decl.GetName().Text; }
        }

        public string InsertText
        {
            get { return decl.GetName().Text; }
        }

        public string TooltipText
        {
            get { return "namespace"; }
        }

        public string Signature
        {
            get { return "NS:" + decl.GetName().Text; }
        }

        public string Comment
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
