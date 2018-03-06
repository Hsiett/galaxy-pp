using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class StructDescription : SuggestionBoxItem
    {
        public string Name;
        public List<VariableDescription> Fields = new List<VariableDescription>();
        public List<MethodDescription> Methods = new List<MethodDescription>();
        public List<MethodDescription> Constructors = new List<MethodDescription>();
        public List<MethodDescription> Deconstructors = new List<MethodDescription>();
        public List<string> GenericVars = new List<string>();
        public int LineFrom, LineTo;
        public ANamedType BaseRef;
        public StructDescription Base;
        public PVisibilityModifier Visibility = new APublicVisibilityModifier();
        public bool IsClass;
        public bool IsEnum;
        public TextPoint Position { get; private set; }
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

        public StructDescription(AStructDecl structDecl)
        {
            Parser parser = new Parser(structDecl);

            Name = parser.Name;
            IsEnum = Name.StartsWith("enum ");
            if (IsEnum)
            {
                Name = Name.Substring(5);
                foreach (VariableDescription field in parser.Fields)
                {
                    field.PlacementPrefix = "Enum Field";
                }
            }
            Fields = parser.Fields;
            
            Methods = parser.Methods;
            Constructors = parser.Constructors;
            Deconstructors = parser.Deconstructors;
            LineFrom = structDecl.GetName().Line;
            LineTo = structDecl.GetEndToken().Line;
            if (structDecl.GetBase() is AGenericType)
                BaseRef = (ANamedType) ((AGenericType) structDecl.GetBase()).GetBase();
            else
                BaseRef = (ANamedType) structDecl.GetBase();
            structDecl.RemoveChild(BaseRef);
            foreach (TIdentifier identifier in structDecl.GetGenericVars())
            {
                GenericVars.Add(identifier.Text);
            }
            IsClass = structDecl.GetClassToken() != null;
            Visibility = (PVisibilityModifier)structDecl.GetVisibilityModifier().Clone();
            Position = TextPoint.FromCompilerCoords(structDecl.GetName());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StructDescription)) return false;
            StructDescription other = (StructDescription)obj;
            if (Fields.Count != other.Fields.Count ||
                Methods.Count != other.Methods.Count ||
                Constructors.Count != other.Constructors.Count ||
                Deconstructors.Count != other.Deconstructors.Count ||
                GenericVars.Count != other.GenericVars.Count ||
                IsClass != other.IsClass)
                return false;
            if ((BaseRef == null) != (other.BaseRef == null))
                return false;
            if (BaseRef != null)
            {
               /*!FIX! if (BaseRef.GetName().Text != other.BaseRef.GetName().Text)
                    return false;
                if ((BaseRef.GetNamespace() == null) != (other.BaseRef.GetNamespace() == null))
                    return false;
                if (BaseRef.GetNamespace() != null)
                {
                    if (BaseRef.GetNamespace().Text != other.BaseRef.GetNamespace().Text)
                        return false;
                }*/
            }

            if (Name != other.Name ||
               Fields.Where((t, i) => !t.Equals(other.Fields[i])).Any() ||
               Methods.Where((t, i) => !t.Equals(other.Methods[i])).Any() ||
               Constructors.Where((t, i) => !t.Equals(other.Constructors[i])).Any() ||
               Deconstructors.Where((t, i) => !t.Equals(other.Deconstructors[i])).Any() ||
               GenericVars.Where((t, i) => !t.Equals(other.GenericVars[i])).Any())
                return false;
            return true;
        }

        public string DisplayText
        {
            get { return Name; }
        }

        public string InsertText
        {
            get { return Name; }
        }

        public string TooltipText
        {
            get { return (IsClass ? "class " : IsEnum ? "enum " : "struct ") + Name; }
        }

        public string Signature
        {
            get { return "S" + Name; }
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
            public string Name;
            public List<VariableDescription> Fields = new List<VariableDescription>();
            public List<MethodDescription> Methods = new List<MethodDescription>();
            public List<MethodDescription> Constructors = new List<MethodDescription>();
            public List<MethodDescription> Deconstructors = new List<MethodDescription>();

            public Parser(AStructDecl structDecl)
            {
                structDecl.Apply(this);
            }

            public override void CaseAStructDecl(AStructDecl node)
            {
                Name = node.GetName().Text;
                
                base.CaseAStructDecl(node);
            }

            public override void CaseAALocalDecl(AALocalDecl node)
            {
                VariableDescription field = new VariableDescription(node, VariableDescription.VariableTypes.StructVariable);
                Fields.Add(field);
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
                MethodDescription method = new MethodDescription(node, Name + "*");
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
