using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes
{
    class CloneMethod : DepthFirstAdapter
    {
        private SharedData data;
        private Dictionary<AALocalDecl, PLvalue> localMap;
        private Node currentCloneNode;

        public delegate void ExtraCheckDelegate(Node originalNode, Node cloneNode);

        private ExtraCheckDelegate extraCheck;

        public CloneMethod(SharedData data, Dictionary<AALocalDecl, PLvalue> localMap, Node currentCloneNode, ExtraCheckDelegate extraCheck = null)
        {
            this.data = data;
            this.localMap = localMap;
            this.currentCloneNode = currentCloneNode;
            this.extraCheck = extraCheck;
        }


        public override void DefaultIn(Node node)
        {
            //
            int index = 0;
            GetChildTypeIndex getChildTypeIndex = new GetChildTypeIndex() { Parent = node.Parent(), Child = node };
            node.Parent().Apply(getChildTypeIndex);
            index = getChildTypeIndex.Index;
            GetChildTypeByIndex getChildTypeByIndex = new GetChildTypeByIndex() { Child = node, Index = index, Parent = currentCloneNode };
            currentCloneNode.Apply(getChildTypeByIndex);
            currentCloneNode = getChildTypeByIndex.Child;

            //currentCloneNode should now be the clone corrosponding to node.
            /*finalTrans.data.ExpTypes
            finalTrans.data.FieldLinks
            finalTrans.data.LocalLinks
            finalTrans.data.Locals//Lets not forget about this one
            finalTrans.data.LvalueTypes
            finalTrans.data.SimpleMethodLinks
            finalTrans.data.StructFieldLinks
            finalTrans.data.StructMethodLinks
            finalTrans.data.StructTypeLinks*/
            if (node is AABlock)
                data.Locals[(AABlock) currentCloneNode] = new List<AALocalDecl>();
            if (node is PExp)
                data.ExpTypes[(PExp)currentCloneNode] = data.ExpTypes[(PExp)node];
            if (node is AFieldLvalue)
                data.FieldLinks[(AFieldLvalue)currentCloneNode] = data.FieldLinks[(AFieldLvalue)node];
            if (node is ALocalLvalue)
            {
                PLvalue replacer = Util.MakeClone(localMap[data.LocalLinks[(ALocalLvalue)node]],
                                                  data);
                currentCloneNode.ReplaceBy(replacer);
                currentCloneNode = replacer;
            }
            if (node is AALocalDecl)
            {
                ALocalLvalue replacer = new ALocalLvalue(new TIdentifier("IwillGetRenamedLater"));
                data.LvalueTypes[replacer] = ((AALocalDecl)currentCloneNode).GetType();
                data.LocalLinks[replacer] = (AALocalDecl)currentCloneNode;
                AABlock pBlock = Util.GetAncestor<AABlock>(currentCloneNode) ??
                                 (AABlock) Util.GetAncestor<AMethodDecl>(currentCloneNode).GetBlock();
                data.Locals[Util.GetAncestor<AABlock>(currentCloneNode)].Add((AALocalDecl) currentCloneNode);
                localMap.Add((AALocalDecl)node, replacer);
            }
            if (node is PLvalue)
                data.LvalueTypes[(PLvalue)currentCloneNode] = data.LvalueTypes[(PLvalue)node];
            if (node is ASimpleInvokeExp)
                data.SimpleMethodLinks[(ASimpleInvokeExp)currentCloneNode] = data.SimpleMethodLinks[(ASimpleInvokeExp)node];
            if (node is AStructLvalue)
                data.StructFieldLinks[(AStructLvalue)currentCloneNode] = data.StructFieldLinks[(AStructLvalue)node];
            if (node is ANonstaticInvokeExp)
                data.StructMethodLinks[(ANonstaticInvokeExp)currentCloneNode] = data.StructMethodLinks[(ANonstaticInvokeExp)node];
            if (node is ANamedType && data.StructTypeLinks.Keys.Contains(node))
                data.StructTypeLinks[(ANamedType)currentCloneNode] = data.StructTypeLinks[(ANamedType)node];
            if (extraCheck != null)
                extraCheck(node, currentCloneNode);
            if (node is PType && data.EnrichmentTypeLinks.ContainsKey((PType) node))
                data.EnrichmentTypeLinks[(PType)currentCloneNode] = data.EnrichmentTypeLinks[(PType)node];
            if (node is AStringConstExp)
            {
                if (data.StringsDontJoinRight.Contains(node))
                    data.StringsDontJoinRight.Add((AStringConstExp) currentCloneNode);
            }
        }

        public override void DefaultOut(Node node)
        {
            currentCloneNode = currentCloneNode.Parent();
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
                if (node.GetType() == Child.GetType())
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
                if (Child.GetType() == node.GetType())
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
    }
}
