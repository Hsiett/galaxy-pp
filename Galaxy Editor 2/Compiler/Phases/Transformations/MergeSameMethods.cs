using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class MergeSameMethods : DepthFirstAdapter
    {
        public static void Parse(FinalTransformations finalTrans)
        {
            SharedData data = finalTrans.data;
            for (int i = 0; i< data.Methods.Count; i++)
            {
                for (int j = i + 1; j < data.Methods.Count; j++)
                {
                    AMethodDecl m1 = data.Methods[i].Decl;
                    AMethodDecl m2 = data.Methods[j].Decl;
                    bool switched = false;
                    if (finalTrans.mainEntry == m1 || m1.GetTrigger() != null)
                    {
                        if (finalTrans.mainEntry == m2 || m2.GetTrigger() != null)
                        {
                            continue;
                        }
                    }
                    else if (finalTrans.mainEntry == m2 || m2.GetTrigger() != null)
                    {
                        AMethodDecl temp = m1;
                        m1 = m2;
                        m2 = temp;
                        switched = true;
                    }

                    MergeSameMethods merger = new MergeSameMethods(m2, data);
                    m1.Apply(merger);
                    if (merger.canMerge)
                    {
                        merger.otherNode = m1;
                        m2.Apply(merger);
                        if (merger.canMerge)
                        {
                            var arr = data.SimpleMethodLinks.ToArray();
                            foreach (KeyValuePair<ASimpleInvokeExp, AMethodDecl> link in arr)
                            {
                                if (link.Value == m2)
                                {
                                    data.SimpleMethodLinks[link.Key] = m1;
                                    link.Key.SetName(new TIdentifier(m1.GetName().Text));
                                }
                            }


                            m2.Parent().RemoveChild(m2);
                            if (switched)
                            {
                                data.Methods.RemoveAt(i);
                                i--;
                                break;
                            }
                            else
                            {
                                data.Methods.RemoveAt(j);
                                j--;
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private SharedData data;
        private bool canMerge = true;
        private Node otherNode;

        private List<AALocalDecl> locals = new List<AALocalDecl>();
        private List<AALocalDecl> otherLocals = new List<AALocalDecl>();

        private MergeSameMethods(Node other, SharedData data)
        {
            otherNode = other;
            this.data = data;
        }

        public class GetChildTypeIndex : DepthFirstAdapter
        {
            public Node Parent;
            public Node Child;
            public int Index = 0;
            private bool indexFound;

            public override void DefaultIn(Node node)
            {
                if (node == Parent)
                    return;
                if (node.Parent() != Parent)
                    return;
                if (node == Child)
                    indexFound = true;
                if (indexFound)
                    return;
                //if (node.GetType() == Child.GetType())
                    Index++;
            }
        }

        public class GetChildTypeByIndex : DepthFirstAdapter
        {
            public Node Parent;
            public Node Child;
            public int Index;
            private int index;
            private bool childFound;

            public override void DefaultIn(Node node)
            {
                if (node == Parent)
                    return;
                if (node.Parent() != Parent)
                    return;
                if (childFound)
                    return;
                //if (Child.GetType() == node.GetType())
                {
                    if (index == Index)
                    {
                        Child = node;
                        childFound = true;
                    }
                    else
                        index++;
                }
            }
        }

        public override void DefaultCase(Node node)
        {
            if (!canMerge)
                return;
            base.DefaultCase(node);
        }

        public override void DefaultIn(Node node)
        {
            if (!canMerge)
                return;
            if (node is AMethodDecl)
            {
                //First node - no need to fetch
                if (((AMethodDecl)node).GetFormals().Count != ((AMethodDecl)otherNode).GetFormals().Count)
                {
                    canMerge = false;
                }
                return;
            }
            //Fetch corrosponding other node
            int index = 0;
            GetChildTypeIndex getChildTypeIndex = new GetChildTypeIndex() { Parent = node.Parent(), Child = node };
            node.Parent().Apply(getChildTypeIndex);
            index = getChildTypeIndex.Index;
            GetChildTypeByIndex getChildTypeByIndex = new GetChildTypeByIndex() { Child = node, Index = index, Parent = otherNode };
            otherNode.Apply(getChildTypeByIndex);
            otherNode = getChildTypeByIndex.Child;

            if (otherNode.GetType() != node.GetType())
            {
                canMerge = false;
                return;
            }

            if (node is AALocalDecl)
            {
                locals.Add((AALocalDecl) node);
                otherLocals.Add((AALocalDecl) otherNode);
                return;
            }
            
            if (node is ANamedType)
            {
                ANamedType aNode = (ANamedType) node;
                ANamedType aOther = (ANamedType) otherNode;
                if (data.StructTypeLinks.ContainsKey(aNode) != data.StructTypeLinks.ContainsKey(aOther))
                {
                    canMerge = false;
                    return;
                }
                if (data.StructTypeLinks.ContainsKey(aNode) && data.StructTypeLinks[aNode] != data.StructTypeLinks[aOther])
                {
                    canMerge = false;
                }
                if (!data.StructTypeLinks.ContainsKey(aNode) && aNode.IsSame(aOther, true))//aNode.GetName().Text != aOther.GetName().Text)
                    canMerge = false;
                if (aNode.IsPrimitive() && !aOther.IsPrimitive(aNode.AsIdentifierString()))
                    canMerge = false;
                return;
            }

            if (node is AABlock)
            {
                AABlock aNode = (AABlock)node;
                AABlock aOther = (AABlock)otherNode;
                if (aNode.GetStatements().Count != aOther.GetStatements().Count)
                    canMerge = false;
                return;
            }

            if (node is AIntConstExp)
            {
                AIntConstExp aNode = (AIntConstExp)node;
                AIntConstExp aOther = (AIntConstExp)otherNode;
                if (aNode.GetIntegerLiteral().Text != aOther.GetIntegerLiteral().Text)
                    canMerge = false;
                return;
            }

            if (node is AFixedConstExp)
            {
                AFixedConstExp aNode = (AFixedConstExp)node;
                AFixedConstExp aOther = (AFixedConstExp)otherNode;
                if (aNode.GetFixedLiteral().Text != aOther.GetFixedLiteral().Text)
                    canMerge = false;
                return;
            }

            if (node is AStringConstExp)
            {
                AStringConstExp aNode = (AStringConstExp)node;
                AStringConstExp aOther = (AStringConstExp)otherNode;
                if (aNode.GetStringLiteral().Text != aOther.GetStringLiteral().Text)
                    canMerge = false;
                return;
            }

            if (node is ACharConstExp)
            {
                ACharConstExp aNode = (ACharConstExp)node;
                ACharConstExp aOther = (ACharConstExp)otherNode;
                if (aNode.GetCharLiteral().Text != aOther.GetCharLiteral().Text)
                    canMerge = false;
                return;
            }

            if (node is ASimpleInvokeExp)
            {
                ASimpleInvokeExp aNode = (ASimpleInvokeExp)node;
                ASimpleInvokeExp aOther = (ASimpleInvokeExp)otherNode;
                if (data.SimpleMethodLinks[aNode] != data.SimpleMethodLinks[aOther] &&
                    !(data.SimpleMethodLinks[aNode] == Util.GetAncestor<AMethodDecl>(aNode) &&
                        data.SimpleMethodLinks[aOther] == Util.GetAncestor<AMethodDecl>(aOther)))
                    canMerge = false;
                return;
            }

            if (node is ALocalLvalue)
            {
                ALocalLvalue aNode = (ALocalLvalue)node;
                ALocalLvalue aOther = (ALocalLvalue)otherNode;
                if (locals.IndexOf(data.LocalLinks[aNode]) != otherLocals.IndexOf(data.LocalLinks[aOther]))
                    canMerge = false;
                return;
            }

            if (node is AFieldLvalue)
            {
                AFieldLvalue aNode = (AFieldLvalue)node;
                AFieldLvalue aOther = (AFieldLvalue)otherNode;
                if (data.FieldLinks[aNode] != data.FieldLinks[aOther])
                    canMerge = false;
                return;
            }

            if (node is AStructLvalue)
            {
                AStructLvalue aNode = (AStructLvalue)node;
                AStructLvalue aOther = (AStructLvalue)otherNode;
                if (data.StructFieldLinks[aNode] != data.StructFieldLinks[aOther])
                    canMerge = false;
                return;
            }
        }

        public override void DefaultOut(Node node)
        {
            if (!canMerge)
                return;
            otherNode = otherNode.Parent();
        }
    }
}
