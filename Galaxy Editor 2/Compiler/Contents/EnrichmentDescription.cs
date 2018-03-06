using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class EnrichmentDescription : SuggestionBoxItem
    {
        public List<VariableDescription> Fields = new List<VariableDescription>();
        public List<MethodDescription> Methods = new List<MethodDescription>();
        public List<MethodDescription> Constructors = new List<MethodDescription>();
        public List<MethodDescription> Deconstructors = new List<MethodDescription>();
        public int LineFrom, LineTo;
        public PType type;
        private IDeclContainer parentFile;
        public IDeclContainer ParentFile
        {
            get { return parentFile; }
            set
            {
                parentFile = value;
                foreach (VariableDescription field in Fields)
                {
                    field.ParentFile = value;
                }
                foreach (MethodDescription method in Methods)
                {
                    method.ParentFile = value;
                }
                foreach (MethodDescription method in Constructors)
                {
                    method.ParentFile = value;
                }
                foreach (MethodDescription method in Deconstructors)
                {
                    method.ParentFile = value;
                }
            }
        }

        public TextPoint Position { get; private set; }

        public bool IsClass;

        public EnrichmentDescription(AEnrichmentDecl structDecl)
        {
            Parser parser = new Parser(structDecl);

            Fields = parser.Fields;
            Methods = parser.Methods;
            Constructors = parser.Constructors;
            Deconstructors = parser.Deconstructors;
            LineFrom = structDecl.GetToken().Line;
            LineTo = structDecl.GetEndToken().Line;
            type = structDecl.GetType();
            type.Parent().RemoveChild(type);
            IsClass = structDecl.GetDimention() != null;
            Position = TextPoint.FromCompilerCoords(structDecl.GetToken());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StructDescription)) return false;
            StructDescription other = (StructDescription)obj;
            if (Fields.Count != other.Fields.Count ||
                Methods.Count != other.Methods.Count ||
                Constructors.Count != other.Constructors.Count ||
                Deconstructors.Count != other.Deconstructors.Count)
                return false;
            if (Fields.Where((t, i) => !t.Equals(other.Fields[i])).Any() ||
               Methods.Where((t, i) => !t.Equals(other.Methods[i])).Any() ||
               Constructors.Where((t, i) => !t.Equals(other.Constructors[i])).Any() ||
               Deconstructors.Where((t, i) => !t.Equals(other.Deconstructors[i])).Any())
                return false;
            return true;
        }

        public string DisplayText
        {
            get { return ""; }
        }

        public string InsertText
        {
            get { return ""; }
        }

        public string TooltipText
        {
            get { return ""; }
        }

        public string Signature
        {
            get { return "Enrich" + Util.TypeToString(type); }
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

        private class Parser : DepthFirstAdapter
        {

            public List<VariableDescription> Fields = new List<VariableDescription>();
            public List<MethodDescription> Methods = new List<MethodDescription>();
            public List<MethodDescription> Constructors = new List<MethodDescription>();
            public List<MethodDescription> Deconstructors = new List<MethodDescription>();
            private PType type;

            public Parser(Node structDecl)
            {
                structDecl.Apply(this);
            }

            public override void InAEnrichmentDecl(AEnrichmentDecl node)
            {
                type = node.GetType();
            }

            public override void CaseAPropertyDecl(APropertyDecl node)
            {
                VariableDescription variable;
                PropertyDescription.CreateItems(node, Methods, out variable);
                if (variable != null)
                    Fields.Add(variable);
            }


            public override void CaseAMethodDecl(AMethodDecl node)
            {
                MethodDescription method = new MethodDescription(node);
                Methods.Add(method);
            }

            public override void CaseAConstructorDecl(AConstructorDecl node)
            {
                MethodDescription method = new MethodDescription(node, Util.TypeToString(type) + "*");
                Constructors.Add(method);
            }

            public override void CaseADeconstructorDecl(ADeconstructorDecl node)
            {
                MethodDescription method = new MethodDescription(node);
                Deconstructors.Add(method);
            }
        }

        
    }
}
