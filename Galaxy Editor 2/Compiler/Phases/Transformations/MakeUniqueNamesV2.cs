using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class MakeUniqueNamesV2 : DepthFirstAdapter
    {
        private interface IDecl
        {
            string Name { get; set; }
            void AddStandardLetter();
        }

        int Compare(IDecl decl1, IDecl decl2)
        {
            if (decl1 is LocalDecl && decl2 is LocalDecl)
            {
                LocalDecl aDecl1 = (LocalDecl)decl1;
                LocalDecl aDecl2 = (LocalDecl)decl2;
                if (aDecl1.ParentMethod != aDecl2.ParentMethod)
                    return aDecl1.ParentMethod.GetHashCode().CompareTo(aDecl2.ParentMethod.GetHashCode());//Here, I assume that the hash codes will always be diffrent.
            }
            return decl1.Name.CompareTo(decl2.Name);
        }

        private class MethodDecl : IDecl
        {
            public AMethodDecl Decl;

            public string Name
            {
                get { return Decl.GetName().Text; }
                set { Decl.GetName().Text = value; }
            }

            public MethodDecl(AMethodDecl decl)
            {
                Decl = decl;
                if (Name.StartsWith("_"))
                    Name = "u" + Name;
            }

            public void AddStandardLetter()
            {
                Name += "M";
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private class FieldDecl : IDecl
        {
            public AFieldDecl Decl;

            public string Name
            {
                get { return Decl.GetName().Text; }
                set { Decl.GetName().Text = value; }
            }

            public FieldDecl(AFieldDecl decl)
            {
                Decl = decl;
                if (Name.StartsWith("_"))
                    Name = "u" + Name;
            }

            public void AddStandardLetter()
            {
                Name += "F";
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private class LocalDecl : IDecl
        {
            public AALocalDecl Decl;
            public AMethodDecl ParentMethod;
            

            public string Name
            {
                get { return Decl.GetName().Text; }
                set { Decl.GetName().Text = value; }
            }

            public LocalDecl(AALocalDecl decl, AMethodDecl parentMethod)
            {
                Decl = decl;
                ParentMethod = parentMethod;
                if (Name.StartsWith("_"))
                    Name = "u" + Name;
            }

            public void AddStandardLetter()
            {
                Name += "L";
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private class StructDecl : IDecl
        {
            public AStructDecl Decl;

            public string Name
            {
                get { return Decl.GetName().Text; }
                set { Decl.GetName().Text = value; }
            }

            public StructDecl(AStructDecl decl)
            {
                Decl = decl;
                if (Name.StartsWith("_"))
                    Name = "u" + Name;
            }

            public void AddStandardLetter()
            {
                Name += "S";
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private LinkedList<IDecl> decls = new LinkedList<IDecl>();
        private LinkedList<IDecl> localDecls = new LinkedList<IDecl>();
        private AMethodDecl pMethod;

        public override void InAMethodDecl(AMethodDecl node)
        {
            pMethod = node;
        }

        public override void OutAMethodDecl(AMethodDecl node)
        {
            node.GetName().Text = node.GetName().Text.Replace("+", "Plus");
            node.GetName().Text = node.GetName().Text.Replace("+", "Plus");
            node.GetName().Text = node.GetName().Text.Replace("-", "Minus");
            node.GetName().Text = node.GetName().Text.Replace("*", "Mul");
            node.GetName().Text = node.GetName().Text.Replace("/", "Div");
            node.GetName().Text = node.GetName().Text.Replace("%", "Mod");
            node.GetName().Text = node.GetName().Text.Replace("==", "Equals");
            node.GetName().Text = node.GetName().Text.Replace("!=", "NotEquals");
            node.GetName().Text = node.GetName().Text.Replace("<", "LessThan");
            node.GetName().Text = node.GetName().Text.Replace("<=", "LessThanOrEqual");
            node.GetName().Text = node.GetName().Text.Replace(">", "GreaterThan");
            node.GetName().Text = node.GetName().Text.Replace(">=", "GreaterThanOrEqual");
            node.GetName().Text = node.GetName().Text.Replace("&", "And");
            node.GetName().Text = node.GetName().Text.Replace("|", "Or");
            node.GetName().Text = node.GetName().Text.Replace("^", "Xor");
            node.GetName().Text = node.GetName().Text.Replace("<<", "ShiftLeft");
            node.GetName().Text = node.GetName().Text.Replace(">>", "ShiftRight");
            decls.AddLast(new MethodDecl(node));
        }

        public override void OutAFieldDecl(AFieldDecl node)
        {
            decls.AddLast(new FieldDecl(node));
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            localDecls.AddLast(new LocalDecl(node, pMethod));
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            decls.AddLast(new StructDecl(node));
        }

        public override void OutAAProgram(AAProgram node)
        {
            List<IDecl> decls = new List<IDecl>(this.decls.Count);
            decls.AddRange(this.decls);
            bool changes;
            do
            {
                decls.Sort(Compare);
                changes = false;


                for (int i = 0; i < decls.Count; i++ )
                {
                    int j = i + 1;
                    for (; j < decls.Count; j++)
                    {
                        if (Compare(decls[i], decls[j]) != 0)
                            break;
                    }
                    if (j - i > 1)
                    {//The same from i to j-1
                        bool sameType = true;
                        for (int k = i + 1; k < j; k++)
                            if (decls[i].GetType() != decls[k].GetType())
                            {
                                //We have diffrent types. Add letter
                                sameType = false;
                                for (k = i; k < j; k++)
                                    decls[k].AddStandardLetter();
                                break;
                            }
                        //Now, add numbers if they are still the same
                        if (sameType)
                        {
                            for (int k = i; k < j; k++)
                            {
                                decls[k].Name += k - i + 1;
                            }
                        }
                        i = j - 1;
                        changes = true;
                    }
                }

            } while (changes);


            List<IDecl> localDecls = new List<IDecl>(this.localDecls.Count);
            localDecls.AddRange(this.localDecls);
            do
            {
                localDecls.Sort(Compare);
                changes = false;


                for (int i = 0; i < localDecls.Count; i++)
                {
                    int j = i + 1;
                    for (; j < localDecls.Count; j++)
                    {
                        if (Compare(localDecls[i], localDecls[j]) != 0)
                            break;
                    }
                    if (j - i > 1)
                    {//The same from i to j-1

                        bool sameType = true;
                        for (int k = i + 1; k < j; k++)
                            if (localDecls[i].GetType() != localDecls[k].GetType())
                            {
                                //We have diffrent types. Add letter
                                sameType = false;
                                for (k = i; k < j; k++)
                                    localDecls[k].AddStandardLetter();
                                break;
                            }
                        //Now, add numbers if they are still the same
                        if (sameType)
                        {
                            for (int k = i; k < j; k++)
                            {
                                localDecls[k].Name += k - i + 1;
                            }
                        }
                        i = j - 1;
                        changes = true;
                    }
                }

            } while (changes);
        }
    }
}
