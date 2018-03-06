using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases;

namespace Galaxy_Editor_2.Compiler
{
    static class Util
    {
        public static void SetPrimitive(this ANamedType node, string primitive)
        {
            AAName name = (AAName) node.GetName();
            TIdentifier lastIdentifier = (TIdentifier) name.GetIdentifier()[name.GetIdentifier().Count - 1];
            name.GetIdentifier().Clear();
            lastIdentifier.Text = primitive;
            name.GetIdentifier().Add(lastIdentifier);
        }

        public static List<string> ToStringList(this AAName name)
        {
            List<string> returner = new List<string>();
            foreach (TIdentifier identifier in name.GetIdentifier())
            {
                returner.Add(identifier.Text);
            }
            return returner;
        }

        public static bool IsStaticContext(Node node)
        {
            //Must be in a struct/enrichment, and in a static method/property/localdecl
            if (HasAncestor<AStructDecl>(node) || HasAncestor<AEnrichmentDecl>(node))
            {
                AMethodDecl pMethod = GetAncestor<AMethodDecl>(node);
                APropertyDecl pProperty = GetAncestor<APropertyDecl>(node);
                AALocalDecl pLocalDecl = GetAncestor<AALocalDecl>(node);

                return pMethod != null && pMethod.GetStatic() != null ||
                       pProperty != null && pProperty.GetStatic() != null ||
                       pLocalDecl != null && pLocalDecl.GetStatic() != null;
            }
            return false;
        }

        public static bool IsIntPointer(Node node, PType type, SharedData data)
        {
            //Enrichment
            if (data.EnrichmentTypeLinks.ContainsKey(type) && data.EnrichmentTypeLinks[type].GetIntDim() != null)
                return true;

            //Struct type
            return
                (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)type) &&
                   data.StructTypeLinks[(ANamedType)type].GetIntDim() != null);
        }

        public static bool IsLocal(Node node, SharedData data)
        {
            IsLocalChecker checker = new IsLocalChecker(data);
            node.Apply(checker);
            return checker.IsLocal;
        }

        private class IsLocalChecker : DepthFirstAdapter
        {
            private SharedData data;

            public IsLocalChecker(SharedData data)
            {
                this.data = data;
            }

            public bool IsLocal = true;

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                IsLocal = false;
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                if (data.FieldLinks[node].GetConst() == null)
                    IsLocal = false;
            }

            public override void CaseANewExp(ANewExp node)
            {
                IsLocal = false;
            }
        }

        public static bool Extends(AStructDecl baseType, AStructDecl cType, SharedData data)
        {
            if (cType == null)
                return false;
            if (baseType == cType)
                return true;
            if (cType.GetBase() == null)
                return false;
            return Extends(baseType, data.StructTypeLinks[(ANamedType) cType.GetBase()], data);
        }

        public static string GetTypeName(AStructDecl str)
        {
            return str.GetClassToken() == null ? "struct" : "class";
        }

        public static T GetAncestor<T>(Node node) where T : Node
        {
            if (node is T)
                return (T)node;
            if (node == null)
                return null;
            Node parent = node.Parent();
            return GetAncestor<T>(parent);
        }

        public static Node GetNearestAncestor(Node node, params Type[] types)
        {
            if (node == null)
                return null;
            foreach (Type type in types)
            {
                if (node.GetType().IsSubclassOf(type))
                {
                    return node;
                }
            }
            if (types.Contains(node.GetType()))
                return node;
            Node parent = node.Parent();
            return GetNearestAncestor(parent, types);
        }

        public static bool HasAncestor<T>(Node node) where T : Node
        {
            return GetAncestor<T>(node) != null;
        }

        public static T GetLastAncestor<T>(Node node) where T : Node
        {
            T ret = GetAncestor<T>(node);
            if (ret == null)
                return null;
            T anc = ret;
            while (true)
            {
                anc = GetAncestor<T>(anc.Parent());
                if (anc == null)
                    return ret;
                ret = anc;
            }
        }

        

        public static bool IsAncestor(Node child, Node ancestor)
        {
            if (child == null) return false;
            if (child == ancestor) return true;
            return IsAncestor(child.Parent(), ancestor);
        }

        public static bool TokenLessThan(Token t1, Token t2)
        {
            if (t1.Line < t2.Line)
                return true;
            if (t1.Line > t2.Line)
                return false;
            return t1.Pos < t2.Pos;
        }

        public static bool ReturnsTheSame(PLvalue left, PLvalue right, SharedData data)
        {
            if (left.GetType() != right.GetType())
                return false;
            if (left is ALocalLvalue)
            {
                ALocalLvalue aLeft = (ALocalLvalue)left;
                ALocalLvalue aRight = (ALocalLvalue)right;
                return data.LocalLinks[aLeft] == data.LocalLinks[aRight];
            }
            if (left is AFieldLvalue)
            {
                AFieldLvalue aLeft = (AFieldLvalue)left;
                AFieldLvalue aRight = (AFieldLvalue)right;
                return data.FieldLinks[aLeft] == data.FieldLinks[aRight];
            }
            if (left is AStructLvalue)
            {
                AStructLvalue aLeft = (AStructLvalue)left;
                AStructLvalue aRight = (AStructLvalue)right;
                if (data.StructFieldLinks[aLeft] != data.StructFieldLinks[aRight])
                    return false;
                return ReturnsTheSame(aLeft.GetReceiver(), aRight.GetReceiver(), data);
            }
            if (left is AArrayLvalue)
            {
                AArrayLvalue aLeft = (AArrayLvalue)left;
                AArrayLvalue aRight = (AArrayLvalue)right;
                return ReturnsTheSame(aLeft.GetIndex(), aRight.GetIndex(), data) &&
                       ReturnsTheSame(aLeft.GetBase(), aRight.GetBase(), data);
            }
            throw new Exception("Util.ReturnsTheSame. Unexpected type, got " + left.GetType());
        }

        public static bool TypesEqual(PType t1, PType t2, SharedData data)
        {
            if (t1.GetType() != t2.GetType())
                return false;
            if (t1 is AVoidType)
                return true;
            if (t1 is AArrayTempType)
            {
                AArrayTempType aT1 = (AArrayTempType)t1;
                AArrayTempType aT2 = (AArrayTempType)t2;
                return TypesEqual(aT1.GetType(), aT2.GetType(), data) &&
                       ReturnsTheSame(aT1.GetDimention(), aT2.GetDimention(), data);
            }
            if (t1 is ADynamicArrayType)
            {
                ADynamicArrayType aT1 = (ADynamicArrayType)t1;
                ADynamicArrayType aT2 = (ADynamicArrayType)t2;
                return TypesEqual(aT1.GetType(), aT2.GetType(), data);
            }
            if (t1 is ANamedType)
            {
                ANamedType aT1 = (ANamedType)t1;
                ANamedType aT2 = (ANamedType)t2;
                if (aT1.IsPrimitive() && aT2.IsPrimitive())
                    return ((AAName)aT1.GetName()).AsString() == ((AAName)aT2.GetName()).AsString();
                if (data.StructTypeLinks.ContainsKey(aT1) != data.StructTypeLinks.ContainsKey(aT2))
                    return false;
                if (data.StructTypeLinks.ContainsKey(aT1) && data.StructTypeLinks[aT1] != data.StructTypeLinks[aT2])
                    return false;
                if (data.DelegateTypeLinks.ContainsKey(aT1) != data.DelegateTypeLinks.ContainsKey(aT2))
                    return false;
                if (data.DelegateTypeLinks.ContainsKey(aT1) && data.DelegateTypeLinks[aT1] != data.DelegateTypeLinks[aT2])
                    return false;
                return true;
            }
            if (t1 is ANullType)
            {
                return true;
            }
            if (t1 is APointerType)
            {
                APointerType aT1 = (APointerType)t1;
                APointerType aT2 = (APointerType)t2;
                return TypesEqual(aT1.GetType(), aT2.GetType(), data);
            }
            if (t1 is AGenericType)
            {
                AGenericType aT1 = (AGenericType)t1;
                AGenericType aT2 = (AGenericType)t2;
                if (!TypesEqual(aT1.GetBase(), aT2.GetBase(), data))
                    return false;
                if (aT1.GetGenericTypes().Count != aT2.GetGenericTypes().Count)
                    return false;
                for (int i = 0; i < aT1.GetGenericTypes().Count; i++)
                {
                    if (!TypesEqual((PType)aT1.GetGenericTypes()[i], (PType)aT2.GetGenericTypes()[i], data))
                        return false;
                }
                return true;
            }
            throw new Exception("Util.TypesEqual: Unexpected type. Got + " + t1.GetType());
        }

        public static bool ReturnsTheSame(PExp left, PExp right, SharedData data)
        {
            if (left.GetType() != right.GetType())
                return false;
            if (left is ABinopExp)
            {
                ABinopExp aLeft = (ABinopExp)left;
                ABinopExp aRight = (ABinopExp)right;
                if (aLeft.GetBinop().GetType() != aRight.GetBinop().GetType())
                    return false;
                return ReturnsTheSame(aLeft.GetLeft(), aRight.GetLeft(), data) &&
                       ReturnsTheSame(aLeft.GetRight(), aRight.GetRight(), data);
            }
            if (left is AUnopExp)
            {
                AUnopExp aLeft = (AUnopExp)left;
                AUnopExp aRight = (AUnopExp)right;
                if (aLeft.GetUnop().GetType() != aRight.GetUnop().GetType())
                    return false;
                return ReturnsTheSame(aLeft.GetExp(), aRight.GetExp(), data);
            }
            if (left is AIntConstExp)
            {
                AIntConstExp aLeft = (AIntConstExp)left;
                AIntConstExp aRight = (AIntConstExp)right;
                return int.Parse(aLeft.GetIntegerLiteral().Text) == int.Parse(aRight.GetIntegerLiteral().Text);
            }
            if (left is AFixedConstExp)
            {
                AFixedConstExp aLeft = (AFixedConstExp)left;
                AFixedConstExp aRight = (AFixedConstExp)right;
                return aLeft.GetFixedLiteral().Text == aRight.GetFixedLiteral().Text;
            }
            if (left is AStringConstExp)
            {
                AStringConstExp aLeft = (AStringConstExp)left;
                AStringConstExp aRight = (AStringConstExp)right;
                return aLeft.GetStringLiteral().Text == aRight.GetStringLiteral().Text;
            }
            if (left is ACharConstExp)
            {
                ACharConstExp aLeft = (ACharConstExp)left;
                ACharConstExp aRight = (ACharConstExp)right;
                return aLeft.GetCharLiteral().Text == aRight.GetCharLiteral().Text;
            }
            if (left is ABooleanConstExp)
            {
                ABooleanConstExp aLeft = (ABooleanConstExp)left;
                ABooleanConstExp aRight = (ABooleanConstExp)right;
                return aLeft.GetBool().GetType() == aRight.GetBool().GetType();
            }
            if (left is ASimpleInvokeExp)
            {
                //A method might not return the same thing each time it is called
                return false;
            }
            if (left is ALvalueExp)
            {
                ALvalueExp aLeft = (ALvalueExp)left;
                ALvalueExp aRight = (ALvalueExp)right;
                return ReturnsTheSame(aLeft.GetLvalue(), aRight.GetLvalue(), data);
            }
            if (left is AParenExp)
            {
                AParenExp aLeft = (AParenExp)left;
                AParenExp aRight = (AParenExp)right;
                return ReturnsTheSame(aLeft.GetExp(), aRight.GetExp(), data);
            }
            throw new Exception("Util.ReturnsTheSame. Unexpected type, got " + left.GetType());
        }

        public static PExp MakeClone(PExp lvalue, SharedData data)
        {
            PExp clone = (PExp)lvalue.Clone();
            MakeCloneRefferences(clone, lvalue, data);
            return clone;
        }

        public static PLvalue MakeClone(PLvalue lvalue, SharedData data)
        {
            if (lvalue == null) return null;
            PLvalue clone = (PLvalue)lvalue.Clone();
            MakeCloneRefferences(clone, lvalue, data);
            return clone;
        }

        public static PType MakeClone(PType type, SharedData data)
        {
            PType clone = (PType)type.Clone();
            MakeCloneRefferences(clone, type, data);
            return clone;
        }

        private static void MakeCloneRefferences(PLvalue clone, PLvalue lvalue, SharedData data)
        {
            data.LvalueTypes[clone] = data.LvalueTypes[lvalue];
            if (lvalue is ALocalLvalue)
                data.LocalLinks[(ALocalLvalue)clone] = data.LocalLinks[(ALocalLvalue)lvalue];
            else if (lvalue is AFieldLvalue)
                data.FieldLinks[(AFieldLvalue)clone] = data.FieldLinks[(AFieldLvalue)lvalue];
            else if (lvalue is AStructLvalue)
            {
                AStructLvalue aLvalue = (AStructLvalue)lvalue;
                AStructLvalue aClone = (AStructLvalue)clone;
                if (data.StructFieldLinks.ContainsKey(aLvalue))
                    data.StructFieldLinks[aClone] =
                        data.StructFieldLinks[aLvalue];
                else
                {
                    data.StructPropertyLinks[aClone] =
                        data.StructPropertyLinks[aLvalue];
                }
                MakeCloneRefferences(aClone.GetReceiver(), aLvalue.GetReceiver(), data);

            }
            else if (lvalue is AStructFieldLvalue)
            {
                AStructFieldLvalue aLvalue = (AStructFieldLvalue)lvalue;
                AStructFieldLvalue aClone = (AStructFieldLvalue)clone;
                if (data.StructMethodFieldLinks.ContainsKey(aLvalue))
                    data.StructMethodFieldLinks[aClone] = data.StructMethodFieldLinks[aLvalue];
                if (data.StructMethodPropertyLinks.ContainsKey(aLvalue))
                    data.StructMethodPropertyLinks[aClone] = data.StructMethodPropertyLinks[aLvalue];
            }
            else if (lvalue is AArrayLvalue)
            {
                AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                AArrayLvalue aClone = (AArrayLvalue)clone;
                MakeCloneRefferences(aClone.GetBase(), aLvalue.GetBase(), data);
                MakeCloneRefferences(aClone.GetIndex(), aLvalue.GetIndex(), data);
            }
            else if (lvalue is APointerLvalue)
            {
                APointerLvalue aLvalue = (APointerLvalue)lvalue;
                APointerLvalue aClone = (APointerLvalue)clone;
                MakeCloneRefferences(aClone.GetBase(), aLvalue.GetBase(), data);
            }
            else if (lvalue is AThisLvalue || lvalue is AValueLvalue)
            {
                //AThisLvalue aLvalue = (AThisLvalue)lvalue;
                //Do nothing more
            }
            else if (lvalue is APropertyLvalue)
            {
                APropertyLvalue aLvalue = (APropertyLvalue)lvalue;
                APropertyLvalue aClone = (APropertyLvalue)clone;
                data.PropertyLinks[aClone] = data.PropertyLinks[aLvalue];
            }
            else
                throw new Exception("Unexpect lvalue. Got " + lvalue.GetType());
        }

        private static void MakeCloneRefferences(PExp clone, PExp exp, SharedData data)
        {
            data.ExpTypes[clone] = data.ExpTypes[exp];
            if (exp is AIntConstExp || exp is AHexConstExp || exp is AOctalConstExp ||
                exp is AFixedConstExp || exp is AStringConstExp || exp is ACharConstExp ||
                exp is ABooleanConstExp || exp is ANullExp)
            {
                //No more required   
            }
            else if(exp is AIncDecExp)
            {
                AIncDecExp aExp = (AIncDecExp)exp;
                AIncDecExp aClone = (AIncDecExp)clone;
                MakeCloneRefferences(aClone.GetLvalue(), aExp.GetLvalue(), data);
            }
            else if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp)exp;
                ABinopExp aClone = (ABinopExp)clone;
                MakeCloneRefferences(aClone.GetLeft(), aExp.GetLeft(), data);
                MakeCloneRefferences(aClone.GetRight(), aExp.GetRight(), data);
            }
            else if (exp is AUnopExp)
            {
                AUnopExp aExp = (AUnopExp)exp;
                AUnopExp aClone = (AUnopExp)clone;
                MakeCloneRefferences(aClone.GetExp(), aExp.GetExp(), data);
            }
            else if (exp is ASimpleInvokeExp)
            {
                ASimpleInvokeExp aExp = (ASimpleInvokeExp)exp;
                ASimpleInvokeExp aClone = (ASimpleInvokeExp)clone;
                data.SimpleMethodLinks[aClone] = data.SimpleMethodLinks[aExp];
                for (int i = 0; i < aExp.GetArgs().Count; i++)
                {
                    MakeCloneRefferences((PExp)aClone.GetArgs()[i], (PExp)aExp.GetArgs()[i], data);
                }
            }
            else if (exp is ANonstaticInvokeExp)
            {
                ANonstaticInvokeExp aExp = (ANonstaticInvokeExp)exp;
                ANonstaticInvokeExp aClone = (ANonstaticInvokeExp)clone;
                data.StructMethodLinks[aClone] = data.StructMethodLinks[aExp];
                for (int i = 0; i < aExp.GetArgs().Count; i++)
                {
                    MakeCloneRefferences((PExp)aClone.GetArgs()[i], (PExp)aExp.GetArgs()[i], data);
                }
                MakeCloneRefferences(aClone.GetReceiver(), aExp.GetReceiver(), data);
            }
            else if (exp is ALvalueExp)
            {
                ALvalueExp aExp = (ALvalueExp)exp;
                ALvalueExp aClone = (ALvalueExp)clone;
                MakeCloneRefferences(aClone.GetLvalue(), aExp.GetLvalue(), data);
            }
            else if (exp is AAssignmentExp)
            {
                AAssignmentExp aExp = (AAssignmentExp)exp;
                AAssignmentExp aClone = (AAssignmentExp)clone;
                MakeCloneRefferences(aClone.GetLvalue(), aExp.GetLvalue(), data);
                MakeCloneRefferences(aClone.GetExp(), aExp.GetExp(), data);
            }
            else if (exp is AParenExp)
            {
                AParenExp aExp = (AParenExp)exp;
                AParenExp aClone = (AParenExp)clone;
                MakeCloneRefferences(aClone.GetExp(), aExp.GetExp(), data);
            }
            else if (exp is AStringConstExp)
            {
                AStringConstExp aExp = (AStringConstExp)exp;
                AStringConstExp aClone = (AStringConstExp)clone;
                if (data.ObfuscatedStrings.ContainsKey(aExp))
                    data.ObfuscatedStrings[aClone] = data.ObfuscatedStrings[aExp];
                if (data.StringsDontJoinRight.Contains(aExp))
                    data.StringsDontJoinRight.Add(aClone);
            }
            else if (exp is ANewExp)
            {
                ANewExp aExp = (ANewExp)exp;
                ANewExp aClone = (ANewExp)clone;
                if (data.ConstructorLinks.ContainsKey(aExp))
                    data.ConstructorLinks[aClone] = data.ConstructorLinks[aExp];
                MakeCloneRefferences(aClone.GetType(), aExp.GetType(), data);
                for (int i = 0; i < aExp.GetArgs().Count; i++)
                {
                    MakeCloneRefferences((PExp)aClone.GetArgs()[i], (PExp)aExp.GetArgs()[i], data);
                }
            }
            else if (exp is ACastExp)
            {
                ACastExp aExp = (ACastExp)exp;
                ACastExp aClone = (ACastExp)clone;
                MakeCloneRefferences(aClone.GetType(), aExp.GetType(), data);
                MakeCloneRefferences(aClone.GetExp(), aExp.GetExp(), data);
            }
            else if (exp is ADelegateExp)
            {
                ADelegateExp aExp = (ADelegateExp)exp;
                ADelegateExp aClone = (ADelegateExp)clone;
                if (data.DelegateCreationMethod.ContainsKey(aExp))
                    data.DelegateCreationMethod[aClone] = data.DelegateCreationMethod[aExp];
                MakeCloneRefferences(aClone.GetType(), aExp.GetType(), data);
                MakeCloneRefferences(aClone.GetLvalue(), aExp.GetLvalue(), data);
            }
            else if (exp is ADelegateInvokeExp)
            {
                ADelegateInvokeExp aExp = (ADelegateInvokeExp)exp;
                ADelegateInvokeExp aClone = (ADelegateInvokeExp)clone;
                MakeCloneRefferences(aClone.GetReceiver(), aExp.GetReceiver(), data);
                for (int i = 0; i < aExp.GetArgs().Count; i++)
                {
                    MakeCloneRefferences((PExp)aClone.GetArgs()[i], (PExp)aExp.GetArgs()[i], data);
                }
            }
            else if (exp is AIfExp)
            {
                AIfExp aExp = (AIfExp)exp;
                AIfExp aClone = (AIfExp)clone;
                MakeCloneRefferences(aClone.GetCond(), aExp.GetCond(), data);
                MakeCloneRefferences(aClone.GetThen(), aExp.GetThen(), data);
                MakeCloneRefferences(aClone.GetElse(), aExp.GetElse(), data);
            }
            else
                throw new Exception("Unexpect exp. Got " + exp.GetType());
        }
        
        private static void MakeCloneRefferences(PType clone, PType type, SharedData data)
        {
            if (data.EnrichmentTypeLinks.ContainsKey(type))
                data.EnrichmentTypeLinks[clone] = data.EnrichmentTypeLinks[type];
            if (type is AArrayTempType)
            {
                MakeCloneRefferences(((AArrayTempType)clone).GetType(), ((AArrayTempType)type).GetType(), data);
                MakeCloneRefferences(((AArrayTempType)clone).GetDimention(), ((AArrayTempType)type).GetDimention(), data);
            }
            if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)type))
            {
                data.StructTypeLinks[(ANamedType)clone] = data.StructTypeLinks[(ANamedType)type];
            }
            if (type is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType)type))
            {
                data.DelegateTypeLinks[(ANamedType)clone] = data.DelegateTypeLinks[(ANamedType)type];
            }
            if (type is APointerType)
            {
                MakeCloneRefferences(((APointerType)clone).GetType(), ((APointerType)type).GetType(), data);
            }
        }

        public static bool IsBulkCopy(PType type)
        {
            return type is AArrayTempType ||
                   (type is ANamedType && ((AAName)((ANamedType)type).GetName()).AsString() != "null" &&
                    !((ANamedType)type).IsPrimitive());
        }


        /*public static bool IsDeclVisible(PDecl decl, AASourceFile viewPoint)
        {
            if (!IsVisible(viewPoint, decl))
                return false;
            if (IsStatic(decl) && GetAncestor<AASourceFile>(decl) != viewPoint)
                return false;
            return true;
        }*/

        public static bool IsStatic(PDecl decl)
        {
            if (decl is AFieldDecl)
            {
                return ((AFieldDecl) decl).GetStatic() != null;
            }
            if (decl is APropertyDecl)
            {
                return ((APropertyDecl)decl).GetStatic() != null;
            }
            if (decl is AMethodDecl)
            {
                return ((AMethodDecl)decl).GetStatic() != null;
            }
            return false;
        }



        public static bool IsBefore(Node node1, Node node2)
        {
            //Return true if node1 is before node2
            if (node1 == node2) return false;
            //Find nearest common ancestor
            List<Node> node1List = new List<Node>();
            Node n = node1;
            while (n != null)
            {
                node1List.Add(n);
                n = n.Parent();
            }
            node1List.Reverse();
            List<Node> node2List = new List<Node>();
            n = node2;
            while (n != null)
            {
                node2List.Add(n);
                n = n.Parent();
            }
            node2List.Reverse();
            for (int i = 1; i < Math.Min(node1List.Count, node2List.Count); i++)
            {
                if (node1List[i] != node2List[i])
                {
                    Node parent = node1List[i - 1];
                    Node child1 = node1List[i];
                    Node child2 = node2List[i];
                    GetFirst getFirst = new GetFirst(parent, child1, child2);
                    parent.Apply(getFirst);
                    return getFirst.FirstChild == child1;
                }
            }
            return node1List.Count < node2List.Count;
        }

        private class GetFirst : DepthFirstAdapter
        {
            private Node parent, child1, child2;
            public Node FirstChild = null;

            public GetFirst(Node parent, Node child1, Node child2)
            {
                this.parent = parent;
                this.child1 = child1;
                this.child2 = child2;
            }

            public override void DefaultIn(Node node)
            {
                if (FirstChild != null)
                    return;
               // if (node == parent)
                    //base.DefaultCase(node);
                if (node == child1)
                    FirstChild = child1;
                if (node == child2)
                    FirstChild = child2;
            }

            public override void DefaultCase(Node node)
            {
                if (FirstChild != null)
                    return;
                if (node == parent)
                    base.DefaultCase(node);
                if (node == child1)
                    FirstChild = child1;
                if (node == child2)
                    FirstChild = child2;
            }
        }

        /*public static bool IsVisible(Node currNode, Node targetNode)
        {
            AASourceFile currentFile = GetAncestor<AASourceFile>(currNode);
            AASourceFile targetFile = GetAncestor<AASourceFile>(targetNode);
            //Default namespace is always visible
            if (targetFile == null || !Util.HasAncestor<ANamespaceDecl>(targetNode))
                return true;
            //It's visible if it is in the same namespace as current file
            if (currentFile != null && currentFile.GetNamespace() != null && currentFile.GetNamespace().Text == targetFile.GetNamespace().Text)
                return true;
            //It is visible if it is set as a using file
            foreach (AUsingDecl usingDecl in currentFile.GetUsings())
            {
                if (usingDecl.GetNamespace().Text == targetFile.GetNamespace().Text)
                    return true;
            }
            return false;

        }*/

        public static bool IsSameNamespace(Node node1, Node node2)
        {
            return NamespacesEquals(GetFullNamespace(node1), GetFullNamespace(node2));
            /*

            AASourceFile file1 = GetAncestor<AASourceFile>(node1);
            AASourceFile file2 = GetAncestor<AASourceFile>(node2);

            if (file1 == null || file1.GetNamespace() == null)
                return file2 == null || file2.GetNamespace() == null;

            if (file2 == null || file2.GetNamespace() == null)
                return false;

            return file1.GetNamespace().Text == file2.GetNamespace().Text;*/
        }
        
        public static string TypeToString(PType type)
        {
            return TypeToStringBuilder(type).ToString();
        }

        public static StringBuilder TypeToStringBuilder(PType type)
        {
            if (type is AVoidType)
                return new StringBuilder("void");
            /*if (type is AArrayType)
            {
                AArrayType aType = (AArrayType)type;
                return TypeToString(aType.GetType()) + "[" + aType.GetDimention().Text + "]";
            }*/
            if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType)type;

                StringBuilder builder = new StringBuilder();
                builder.Append(TypeToStringBuilder(aType.GetType()));
                builder.Append("[");
                if (aType.GetIntDim() != null)
                    builder.Append(aType.GetIntDim().Text);
                builder.Append("]");
                return builder;
            }
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                StringBuilder builder = new StringBuilder();
                foreach (TIdentifier identifier in ((AAName)aType.GetName()).GetIdentifier())
                {
                    if (builder.Length != 0)
                        builder.Append(".");
                    builder.Append(identifier.Text);
                }
                return builder;
            }
            if (type is APointerType)
            {
                APointerType aType = (APointerType)type;
                StringBuilder builder = new StringBuilder();
                builder.Append(TypeToStringBuilder(aType.GetType()));
                builder.Append("*");
                return builder;
            }
            if (type is ADynamicArrayType)
            {
                ADynamicArrayType aType = (ADynamicArrayType)type;
                StringBuilder builder = new StringBuilder();
                builder.Append(TypeToStringBuilder(aType.GetType()));
                builder.Append("[]");
                return builder;
            }
            if (type is AGenericType)
            {
                AGenericType aType = (AGenericType)type;
                StringBuilder builder = new StringBuilder();
                builder.Append(TypeToStringBuilder(aType.GetBase()));
                builder.Append("<");
                bool first = true;
                foreach (PType t in aType.GetGenericTypes())
                {
                    if (first)
                        first = false;
                    else
                        builder.Append(", ");
                    builder.Append(TypeToStringBuilder(t));
                }
                builder.Append(">");
                return builder;
            }
            throw new Exception("Unknown type. Got " + type);
        }

        public static string TypeToIdentifierString(PType type)
        {
            if (type is AVoidType)
                return "void";
            /*if (type is AArrayType)
            {
                AArrayType aType = (AArrayType)type;
                return TypeToString(aType.GetType()) + "[" + aType.GetDimention().Text + "]";
            }*/
            if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType)type;
                return TypeToIdentifierString(aType.GetType()) + "Ar" + (aType.GetIntDim() == null ? "" : aType.GetIntDim().Text);
            }
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                return ((AAName)aType.GetName()).AsString().Replace('.', '_');
            }
            if (type is APointerType)
            {
                APointerType aType = (APointerType)type;
                return "p" + TypeToIdentifierString(aType.GetType());
            }
            if (type is ADynamicArrayType)
            {
                ADynamicArrayType aType = (ADynamicArrayType)type;
                return TypeToIdentifierString(aType.GetType()) + "DAr";
            }
            if (type is AGenericType)
            {
                AGenericType aType = (AGenericType)type;
                string ret = TypeToIdentifierString(aType.GetBase()) + "G_";
                bool first = true;
                foreach (PType t in aType.GetGenericTypes())
                {
                    if (first)
                        first = false;
                    else
                        ret += "_";
                    ret += TypeToIdentifierString(t);
                }
                return ret + "_G";
            }
            throw new Exception("Unknown type");
        }

        public static string ToString(AMethodDecl methodDecl)
        {
            string str = TypeToString(methodDecl.GetReturnType()) + " " + methodDecl.GetName().Text + "(";
            foreach (AALocalDecl formal in methodDecl.GetFormals())
            {
                if (str.EndsWith("("))
                    str += TypeToString(formal.GetType()) + " " + formal.GetName().Text;
                else
                    str += ", " + TypeToString(formal.GetType()) + " " + formal.GetName().Text;
            }
            str += ")";
            return str;
        }

        public static bool TokenPathLessThan(List<Token> p1, List<Token> p2)
        {
            return TokenPathLessThan(p1, p2, 0);
        }

        private static bool TokenPathLessThan(List<Token> p1, List<Token> p2, int index)
        {
            //If one of the lists contains no more elements, either they are both empty and they were pointing to the same token or
            //the one that is empty was pointing to an include. In both cases, consider it as equality, and return false


            if (index >= p1.Count || index >= p2.Count)
                return false;
            //Is the current p1 and p2 token the same? in that case, look further down
            if (p1[index] == p2[index])
                return TokenPathLessThan(p1, p2, index + 1);
            //Otherwise, we only need to compare theese two tokens
            return TokenLessThan(p1[index], p2[index]);
        }

       

        public static string TokenToStringPos(Token token)
        {
            return "[" + token.Line + "," + token.Pos + "]";
        }


        public static Token GetName(PDecl decl)
        {
            if (decl is AFieldDecl)
                return ((AFieldDecl)decl).GetName();
            if (decl is AMethodDecl)
                return ((AMethodDecl)decl).GetName();
            if (decl is AStructDecl)
                return ((AStructDecl)decl).GetName();
            if (decl is AIncludeDecl)
                return ((AIncludeDecl)decl).GetName();
            throw new Exception("Unknown decl");
        }

        public static string GetString(TStringLiteral str)
        {
            string s = str.Text;
            s = s.Remove(s.Length - 1);
            s = s.Substring(1);
            return s;
        }

        public static bool IsIdentifierLetter(char ch)
        {//Basically 0-9, a-z, A-Z, _, $
            return (ch >= 65 && ch <= 90) ||
                   (ch >= 97 && ch <= 122) ||
                   (ch >= 192 && ch <= 214) ||
                   (ch >= 216 && ch <= 246) ||
                   (ch >= 248 && ch <= 255) ||
                   (ch >= '0' && ch <= '9') ||
                   ch == 170 ||
                   ch == 181 ||
                   ch == 186 ||
                   ch == '$' ||
                   ch == '_' ||
                   ch == '#' ||
                   ch > 0xFF;
        }

        public static string GetMethodSignature(AMethodDecl method)
        {
            StringBuilder str = new StringBuilder(method.GetName().Text);
            str.Append("(");
            bool first = true;
            foreach (AALocalDecl localDecl in method.GetFormals())
            {
                if (!first)
                    str.Append(",");
                str.Append(Util.TypeToStringBuilder(localDecl.GetType()));
                first = false;
            }
            str.Append(")");
            return str.ToString();
        }


        public static string GetConstructorSignature(AConstructorDecl method)
        {
            string str = "(";
            bool first = true;
            foreach (AALocalDecl localDecl in method.GetFormals())
            {
                if (!first)
                    str += ",";
                str += Util.TypeToString(localDecl.GetType());
                first = false;
            }
            str += ")";
            return str;
        }

        public static string Capitalize(string s)
        {
            StringBuilder builder = new StringBuilder("");
            foreach (char c in s)
            {
                char C = c;
                if (builder.Length == 0 ||
                    builder.ToString() == "A" && C == 'i' ||
                    builder.ToString() == "Abil" ||
                    builder.ToString() == "Actor" ||
                    builder.ToString() == "AI" ||
                    builder.ToString() == "Camera" ||
                    builder.ToString() == "Player" ||
                    builder.ToString() == "Sound" ||
                    builder.ToString() == "Transmission" ||
                    builder.ToString() == "Unit" ||
                    builder.ToString() == "Wave")
                    C = char.ToUpper(c);
                builder.Append(C);
            }
            return builder.ToString();
        }

        public static Token GetToken(this AAName name)
        {
            return ((TIdentifier) name.GetIdentifier()[name.GetIdentifier().Count - 1]);
        }

        public static Token GetToken(this ANamedType node)
        {
            return GetToken((AAName) node.GetName());
        }

        public static string AsString(this ANamedType node)
        {
            return ((AAName) node.GetName()).AsString();
        }

        public static string AsString(this AAName node)
        {
            string s = "";
            foreach (TIdentifier identifier in node.GetIdentifier())
            {
                if (s != "")
                    s += ".";
                s += identifier.Text;
            }
            return s;
        }

        public static string AsIdentifierString(this ANamedType node)
        {
            return ((AAName)node.GetName()).AsIdentifierString();
        }

        public static string AsIdentifierString(this AAName node)
        {
            string s = "";
            foreach (TIdentifier identifier in node.GetIdentifier())
            {
                if (s != "")
                    s += "_";
                s += identifier.Text;
            }
            return s;
        }

        public static bool IsPrimitive(this ANamedType node, string primitive)
        {
            AAName name = (AAName) node.GetName();
            if (name.GetIdentifier().Count != 1)
                return false;
            return ((TIdentifier) name.GetIdentifier()[0]).Text == primitive;
        }

        public static bool IsSame(this ANamedType node1, ANamedType node2, bool primitiveOnly)
        {
            AAName name1 = (AAName)node1.GetName();
            AAName name2 = (AAName)node2.GetName();
            if (primitiveOnly && (name1.GetIdentifier().Count > 1 || name2.GetIdentifier().Count > 1))
                return false;
            if (name1.GetIdentifier().Count != name2.GetIdentifier().Count)
                return false;
            for (int i = 0; i < name2.GetIdentifier().Count; i++)
            {
                TIdentifier identifier1 = (TIdentifier)name1.GetIdentifier()[i];
                TIdentifier identifier2 = (TIdentifier)name2.GetIdentifier()[i];
                if (identifier1.Text != identifier2.Text)
                    return false;
            }
            return true;
        }

        public static bool IsPrimitive(this ANamedType node, string[] primitives)
        {
            AAName name = (AAName)node.GetName();
            if (name.GetIdentifier().Count != 1)
                return false;
            return primitives.Contains(((TIdentifier)name.GetIdentifier()[0]).Text);
        }

        public static bool IsPrimitive(this ANamedType node)
        {
            AAName name = (AAName)node.GetName();
            if (name.GetIdentifier().Count != 1)
                return false;
            return GalaxyKeywords.Primitives.words.Contains(((TIdentifier)name.GetIdentifier()[0]).Text);
        }

        public static List<IList> GetVisibleDecls(Node node, bool includeUsings)
        {
            List<IList> returner = new List<IList>();
            AASourceFile currentSourceFile = GetAncestor<AASourceFile>(node);
            List<List<string>> usedNamespaces = new List<List<string>>();
            if (includeUsings)
            {
                foreach (AUsingDecl usingDecl in currentSourceFile.GetUsings())
                {
                    List<string> ns = new List<string>();
                    foreach (TIdentifier identifier in usingDecl.GetNamespace())
                    {
                        ns.Add(identifier.Text);
                    }
                   // ns.Reverse();
                    usedNamespaces.Add(ns);
                }
            }
            {
                List<string> currentNS = GetFullNamespace(node);
                if (currentNS.Count > 0)
                    usedNamespaces.Add(currentNS);
            }
            List<IList> currentList = new List<IList>();
            List<IList> nextList = new List<IList>();
            AAProgram program = GetAncestor<AAProgram>(currentSourceFile);
            foreach (AASourceFile sourceFile in program.GetSourceFiles())
            {
                currentList.Add(sourceFile.GetDecl());
                returner.Add(sourceFile.GetDecl());
            }
            while (currentList.Count > 0)
            {
                foreach (IList declList in currentList)
                {
                    foreach (PDecl decl in declList)
                    {
                        if (decl is ANamespaceDecl)
                        {
                            ANamespaceDecl aDecl = (ANamespaceDecl) decl;
                            List<string> ns = GetFullNamespace(decl);
                            bool prefix = false;
                            bool match = false;
                            foreach (List<string> usedNamespace in usedNamespaces)
                            {
                                if (NamespacePrefix(ns, usedNamespace))
                                {
                                    prefix = true;
                                    if (NamespacesEquals(ns, usedNamespace))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                            }
                            if (prefix)
                                nextList.Add(aDecl.GetDecl());
                            if (match)
                                returner.Add(aDecl.GetDecl());
                        }
                    }
                }
                currentList = nextList;
                nextList = new List<IList>();
            }
            return returner;
        }

        public static List<string> GetFullNamespace(Node node)
        {
            List<string> currentNamespace = new List<string>();
            ANamespaceDecl n = GetAncestor<ANamespaceDecl>(node);
            while (n != null)
            {
                currentNamespace.Add(n.GetName().Text);
                n = GetAncestor<ANamespaceDecl>(n.Parent());
            }
            currentNamespace.Reverse();
            return currentNamespace;
        }

        public static bool NamespacesEquals(List<string> ns1, List<string> ns2)
        {
            if (ns1.Count != ns2.Count)
                return false;
            for (int i = 0; i < ns1.Count; i++)
            {
                if (ns1[i] != ns2[i])
                    return false;
            }
            return true;
        }

        public static bool NamespacePrefix(List<string> prefix, List<string> ns)
        {
            if (prefix.Count > ns.Count)
                return false;
            for (int i = 0; i < prefix.Count; i++)
            {
                if (prefix[i] != ns[i])
                    return false;
            }
            return true;
        }

        public class Pair<T, U>
        {
            public Pair()
            {
            }

            public Pair(T first, U second)
            {
                this.First = first;
                this.Second = second;
            }

            public T First { get; set; }
            public U Second { get; set; }

        }
    }
}
