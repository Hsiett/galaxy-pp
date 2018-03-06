using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases
{
    [Serializable]
    public class LibraryData : DepthFirstAdapter
    {
        public List<AMethodDecl> Methods = new List<AMethodDecl>();
        public List<AFieldDecl> Fields = new List<AFieldDecl>();
        public List<AStructDecl> Structs = new List<AStructDecl>();
        public Dictionary<AStructDecl, List<AMethodDecl>> StructMethods = new Dictionary<AStructDecl, List<AMethodDecl>>();
        public Dictionary<AStructDecl, List<AALocalDecl>> StructFields = new Dictionary<AStructDecl, List<AALocalDecl>>();

        [NonSerialized]
        private StreamWriter writer;

        public LibraryData(){}

        public LibraryData(AAProgram program, StreamWriter writer)
        {
            this.writer = writer;
            program.Apply(this);
        }
        //windywell
        public void JoinNew(LibraryData other)
        {
            int count = Methods.Count;
            foreach (AMethodDecl method in other.Methods)
            {
                bool add = true;
                for (int i = 0; i < count; i++)
                {
                    if (Util.GetMethodSignature(Methods[i]) == Util.GetMethodSignature(method))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    Methods.Add(method);
            }

            count = Fields.Count;
            foreach (AFieldDecl field in other.Fields)
            {
                bool add = true;
                for (int i = 0; i < count; i++)
                {
                    if (Fields[i].GetName().Text == field.GetName().Text)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    Fields.Add(field);
            }

            count = Structs.Count;
            for (int j = 0; j < other.Structs.Count; j++)
            {
                bool add = true;
                AStructDecl str = other.Structs[j];
                for (int i = 0; i < count; i++)
                {
                    if (Structs[i].GetName().Text == str.GetName().Text)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    Structs.Add(str);
                    StructMethods.Add(str, other.StructMethods[str]);
                    StructFields.Add(str, other.StructFields[str]);
                }
            }
        }
        public void Join(LibraryData other)
        {
            int count = Methods.Count;
            foreach (AMethodDecl method in other.Methods)
            {
                bool add = true;
                for (int i = 0; i < count; i++)
                {
                    if (Util.GetMethodSignature(Methods[i]) != Util.GetMethodSignature(method))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    Methods.Add(method);
            }

            count = Fields.Count;
            foreach (AFieldDecl field in other.Fields)
            {
                bool add = true;
                for (int i = 0; i < count; i++)
                {
                    if (Fields[i].GetName().Text != field.GetName().Text)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    Fields.Add(field);
            }

            count = Structs.Count;
            for (int j = 0; j < other.Structs.Count; j++)
            {
                bool add = true;
                AStructDecl str = other.Structs[j];
                for (int i = 0; i < count; i++)
                {
                    if (Structs[i].GetName().Text != str.GetName().Text)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    Structs.Add(str);
                    StructMethods.Add(str, other.StructMethods[str]);
                    StructFields.Add(str, other.StructFields[str]);
                }
            }
        }

        public override void CaseAASourceFile(AASourceFile node)
        {
            writer.WriteLine(node.GetName().Text + ":");
            base.CaseAASourceFile(node);
        }

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (node.GetNative() == null && node.GetBlock() == null)
                return;
            if (node.GetStatic() != null)
                return;

            string inputStr = "native " + TypeToString(node.GetReturnType()) + " " + node.GetName().Text +
                                 "(";
            bool first = true;
            foreach (AALocalDecl formal in node.GetFormals())
            {
                if (!first)
                    inputStr += ", ";
                inputStr += TypeToString(formal.GetType()) + " " + formal.GetName().Text;
                first = false;
            }
            inputStr += ");";

            writer.WriteLine(inputStr);

            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            List<AMethodDecl> methodList;
            if (str != null)
                methodList = StructMethods[str];
            else
                methodList = Methods;
            string sig = Util.GetMethodSignature(node);
            if (methodList.Any(otherMethod => Util.GetMethodSignature(otherMethod) == sig))
            {
                return;
            }

            methodList.Add(node);
            node.SetBlock(null);
            node.Parent().RemoveChild(node);


            
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
        

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            if (node.GetStatic() != null)
                return;
            writer.WriteLine(TypeToString(node.GetType()) + " " + node.GetName().Text + ";");
            if (Fields.Any(decl => decl.GetName().Text == node.GetName().Text))
            {
                return;
            }
            Fields.Add(node);
            //node.SetInit(null);
            node.Parent().RemoveChild(node);


        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            if (Structs.Any(structDecl => structDecl.GetName().Text == node.GetName().Text))
            {
                return;
            }
            Structs.Add(node);
            StructMethods.Add(node, new List<AMethodDecl>());
            StructFields.Add(node, new List<AALocalDecl>());
            base.CaseAStructDecl(node);
            node.Parent().RemoveChild(node);
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            //It wont enter methods
            //Repeated fields in structs are syntax errors
            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            StructFields[str].Add(node);
            node.Parent().RemoveChild(node);
        }
    }
}
