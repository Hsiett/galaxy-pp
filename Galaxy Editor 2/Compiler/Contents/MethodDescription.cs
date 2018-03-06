using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class MethodDescription : SuggestionBoxItem
    {
        public TextPoint Start, End;
        public string ReturnType;
        public string Name;
        public List<VariableDescription> Formals = new List<VariableDescription>();
        public List<VariableDescription> Locals = new List<VariableDescription>();
        public AMethodDecl Decl;
        public bool IsStatic;
        public bool IsDelegate;
        public PVisibilityModifier Visibility = new APublicVisibilityModifier();
        public PType realType;
        public PType propertyType;
        public TextPoint Position { get; private set; }


        private IDeclContainer parentFile;
        public IDeclContainer ParentFile
        {
            get { return parentFile; }
            set
            {
                parentFile = value;
                foreach (VariableDescription var in Locals)
                {
                    var.ParentFile = value;
                }
                foreach (VariableDescription var in Formals)
                {
                    var.ParentFile = value;
                }
            }
        }

        public MethodDescription(AMethodDecl method)
        {
            Parser parser = new Parser(method);
            

            Start = parser.Start;
            End = parser.End;
            ReturnType = parser.ReturnType;
            Name = parser.Name;
            Formals = parser.Formals;
            Locals = parser.Locals;
            if (method.Parent() != null)
                method.Parent().RemoveChild(method);
            IsDelegate = method.GetDelegate() != null;
            //if (!IsDelegate)
                Decl = method;
            IsStatic = method.GetStatic() != null;
            Visibility = method.GetVisibilityModifier();
            realType = (PType)method.GetReturnType().Clone();
            Position = TextPoint.FromCompilerCoords(method.GetName());
        }

        public MethodDescription(AConstructorDecl method, string type)
        {
            Parser parser = new Parser(method);


            Start = parser.Start;
            End = parser.End;
            ReturnType = type;
            Name = parser.Name;
            Formals = parser.Formals;
            Locals = parser.Locals;
            if (method.Parent() != null)
                method.Parent().RemoveChild(method);
            Decl = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new ANamedType(new TIdentifier(type), null),
                                   new TIdentifier(""), new ArrayList(), method.GetBlock());
            while (method.GetFormals().Count > 0)
                Decl.GetFormals().Add(method.GetFormals()[0]);
            Visibility = method.GetVisibilityModifier();
            Position = TextPoint.FromCompilerCoords(method.GetName());
        }


        public MethodDescription(ADeconstructorDecl method)
        {
            Parser parser = new Parser(method);


            Start = parser.Start;
            End = parser.End;
            ReturnType = "void";
            Name = parser.Name;
            Formals = parser.Formals;
            Locals = parser.Locals;
            if (method.Parent() != null)
                method.Parent().RemoveChild(method);
            while (method.GetFormals().Count > 0)
                Decl.GetFormals().Add(method.GetFormals()[0]);
            Visibility = method.GetVisibilityModifier();
            Position = TextPoint.FromCompilerCoords(method.GetName());
        }

        public MethodDescription(AInitializerDecl initializer)
        {
            Parser parser = new Parser(initializer);

            Start = parser.Start;
            End = parser.End;
            ReturnType = parser.ReturnType;
            Name = parser.Name;
            Formals = parser.Formals;
            Locals = parser.Locals;
            if (initializer.Parent() != null)
                initializer.Parent().RemoveChild(initializer);
            //Decl = initializer;
            IsStatic = false;
            Position = TextPoint.FromCompilerCoords(initializer.GetToken());

        }

        public MethodDescription(TextPoint start, PType returnType, AABlock block, PType propertyType)
        {
            Parser parser = new Parser(block);

            Start = start;
            End = parser.End;
            ReturnType = Util.TypeToString(returnType);
            Name = "";
            Formals = parser.Formals;
            Locals = parser.Locals;
            if (block.Parent() != null)
                block.Parent().RemoveChild(block);
            //Decl = initializer;
            IsStatic = false;
            realType = (PType) returnType.Clone();
            if (propertyType != null)
                this.propertyType = (PType)propertyType.Clone();
            Position = start;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MethodDescription)) return false;
            MethodDescription other = (MethodDescription) obj;
            if (Formals.Count != other.Formals.Count ||
                Locals.Count != other.Locals.Count ||
                Visibility.GetType() != other.Visibility.GetType())
                return false;
            if (Start != other.Start ||
                End != other.End ||
                ReturnType != other.ReturnType ||
                Name != other.Name ||
                Formals.Where((t, i) => !t.Equals(other.Formals[i])).Any() ||
                Locals.Where((t, i) => !t.Equals(other.Locals[i])).Any() ||
                IsStatic != other.IsStatic)
                return false;
            return true;
        }


        public string DisplayText
        {
            get { return Name; }
        }

        public string InsertText
        {
            get { return Name + (IsDelegate ? "" : "("); }
        }

        public string TooltipText
        {
            get
            {
                string text = "(";
                if (Formals.Count == 0)
                    text += "<no parameters>";
                else
                {
                    foreach (VariableDescription formal in Formals)
                    {
                        text += formal.Type + " " + formal.Name + ", ";
                    }
                    text = text.Remove(text.Length - 2);
                }
                text += ") : " + ReturnType;
                return text;
            }
        }

        public string Signature
        {
            get { return "M" + ReturnType + ":" + Name; }
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
            public TextPoint Start, End;
            public string ReturnType;
            public string Name;
            public List<VariableDescription> Formals = new List<VariableDescription>();
            public List<VariableDescription> Locals = new List<VariableDescription>();
            
            public Parser(AMethodDecl start)
            {
                start.Apply(this);
            }

            public Parser(AInitializerDecl start)
            {
                start.Apply(this);
            }


            public Parser(Node start)
            {
                start.Apply(this);
            }

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                End = Start = TextPoint.FromCompilerCoords(node.GetName().Line, node.GetName().Pos);
                ReturnType = Util.TypeToString(node.GetReturnType());
                Name = node.GetName().Text;

                base.CaseAMethodDecl(node);
            }

            public override void CaseAConstructorDecl(AConstructorDecl node)
            {
                End = Start = TextPoint.FromCompilerCoords(node.GetName().Line, node.GetName().Pos);
                Name = node.GetName().Text;

                base.CaseAConstructorDecl(node);
            }

            public override void CaseADeconstructorDecl(ADeconstructorDecl node)
            {
                End = Start = TextPoint.FromCompilerCoords(node.GetName().Line, node.GetName().Pos);
                Name = node.GetName().Text;

                base.CaseADeconstructorDecl(node);
            }

            public override void CaseAInitializerDecl(AInitializerDecl node)
            {
                Name = "";
                ReturnType = "void";
                End = Start = TextPoint.FromCompilerCoords(node.GetToken().Line, node.GetToken().Pos);

                base.CaseAInitializerDecl(node);
            }

            public override void CaseAABlock(AABlock node)
            {
                if (node == null) return;
                //If this is the first block, it marks the end
                if (node.Parent() is AMethodDecl || node.Parent() is AInitializerDecl || node.Parent() is ATriggerDecl ||
                    node.Parent() is APropertyDecl || node.Parent() is AConstructorDecl || node.Parent() is ADeconstructorDecl)
                {
                    End = TextPoint.FromCompilerCoords(node.GetToken().Line, node.GetToken().Pos);
                }
                base.CaseAABlock(node);
            }

            public override void CaseAALocalDecl(AALocalDecl node)
            {
                //If parent is a methoddecl, we are a parameter
                if (node.Parent() is AMethodDecl)
                {
                    VariableDescription variable = new VariableDescription(node, VariableDescription.VariableTypes.Parameter);
                    Formals.Add(variable);
                }
                else
                {
                    VariableDescription variable = new VariableDescription(node, VariableDescription.VariableTypes.LocalVariable);
                    Locals.Add(variable);
                }

                base.CaseAALocalDecl(node);
            }
        }

        
    }
}
