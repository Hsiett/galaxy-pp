using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class FixGenerics : DepthFirstAdapter
    {
        private SharedData data;
        private ErrorCollection errors;

        Dictionary<AStructDecl, List<AGenericType>> Refferences = new Dictionary<AStructDecl, List<AGenericType>>();

        public FixGenerics(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        bool needAnotherPass = false;
        private List<AStructDecl> structsWithGenerics = new List<AStructDecl>();
        Dictionary<AStructDecl, List<List<PType>>> copies = new Dictionary<AStructDecl, List<List<PType>>>();
        Dictionary<List<PType>, AStructDecl> clones = new Dictionary<List<PType>, AStructDecl>();

        public override void OutAAProgram(AAProgram node)
        {
            foreach (var pair in Refferences)
            {
                if (structsWithGenerics.Contains(pair.Key) && pair.Value.Count > 0)
                    needAnotherPass = true;
            }
            foreach (var pair in Refferences)
            {
                AStructDecl str = pair.Key;
                if (!copies.ContainsKey(str))
                    copies[str] = new List<List<PType>>();
                IList declList;
                Node parent = str.Parent();
                if (parent is AASourceFile)
                    declList = ((AASourceFile)parent).GetDecl();
                else
                    declList = ((ANamespaceDecl)parent).GetDecl();
                //AASourceFile pFile = (AASourceFile) str.Parent();
                foreach (AGenericType refference in pair.Value)
                {

                    AStructDecl clone = null;
                    bool addList = true;
                    foreach (List<PType> list in copies[str])
                    {
                        bool listEqual = true;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (!Util.TypesEqual(list[i], (PType) refference.GetGenericTypes()[i], data))
                            {
                                listEqual = false;
                                break;
                            }
                        }
                        if (listEqual)
                        {
                            addList = false;
                            clone = clones[list];
                            break;
                        }
                    }
                    if (addList)
                    {
                        List<PType> list = new List<PType>();
                        foreach (PType type in refference.GetGenericTypes())
                        {
                            list.Add(type);
                        }
                        copies[str].Add(list);

                        clone = (AStructDecl)str.Clone();
                        declList.Insert(declList.IndexOf(str), clone);


                        clone.Apply(new EnviromentBuilding(errors, data));
                        clone.Apply(new EnviromentChecking(errors, data, false));
                        clone.Apply(new LinkNamedTypes(errors, data));
                        /*
                        data.Structs.Add(new SharedData.DeclItem<AStructDecl>(pFile, clone));
                        data.StructFields[clone] = new List<AALocalDecl>();
                        data.StructMethods[clone] = new List<AMethodDecl>();
                        data.StructProperties[clone] = new List<APropertyDecl>();
                        data.StructConstructors[clone] = new List<AConstructorDecl>();
                        foreach (PLocalDecl localDecl in clone.GetLocals())
                        {
                            if (localDecl is AALocalDecl)
                            {
                                data.StructFields[clone].Add((AALocalDecl)localDecl);
                            }
                            else
                            {
                                PDecl decl = ((ADeclLocalDecl) localDecl).GetDecl();
                                if (decl is AMethodDecl)
                                {
                                    data.StructMethods[clone].Add((AMethodDecl) decl);
                                }
                                else if (decl is APropertyDecl)
                                {
                                    data.StructProperties[clone].Add((APropertyDecl)decl);
                                }
                                else
                                {
                                    data.StructConstructors[clone].Add((AConstructorDecl) decl);   
                                }
                            }
                        }*/

                        clones[list] = clone;

                        
                        clone.setGenericVars(new ArrayList());

                        FixGenericLinks.Parse(str, clone, list, data);
                        clone.GetName().Text = Util.TypeToIdentifierString(refference);
                    }
                    //Change refference to clone
                    ANamedType baseRef = (ANamedType) refference.GetBase();
                    List<string> cloneNs = Util.GetFullNamespace(clone);
                    cloneNs.Add(clone.GetName().Text);
                    AAName aName = (AAName) baseRef.GetName();
                    aName.GetIdentifier().Clear();
                    foreach (var n in cloneNs)
                    {
                        aName.GetIdentifier().Add(new TIdentifier(n));
                    }
                    data.StructTypeLinks[baseRef] = clone;
                    refference.ReplaceBy(baseRef);
                }

                if (!needAnotherPass)
                    parent.RemoveChild(str);
            }
            if (needAnotherPass)
            {
                Refferences.Clear();
                structsWithGenerics.Clear();
                needAnotherPass = false;
                CaseAAProgram(node);
            }
        }

        private class FixGenericLinks : DepthFirstAdapter
        {
            public static void Parse(AStructDecl str, AStructDecl clone, List<PType> types, SharedData data)
            {
                FixGenericLinks fixer = new FixGenericLinks(clone, types, data)
                    {original = str, clone = clone};
                str.Apply(fixer);
            }

            private AStructDecl original, clone;

            private SharedData data;
            private List<PType> types;
            private Node currentClone;
            private bool isFirst = true;

            public FixGenericLinks(Node currentClone, List<PType> types, SharedData data)
            {
                this.currentClone = currentClone;
                this.types = types;
                this.data = data;
            }

            private PType replacer;
            public override void DefaultIn(Node node)
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    int index = 0;
                    CloneMethod.GetChildTypeIndex getChildTypeIndex = new CloneMethod.GetChildTypeIndex() { Parent = node.Parent(), Child = node };
                    node.Parent().Apply(getChildTypeIndex);
                    index = getChildTypeIndex.Index;
                    CloneMethod.GetChildTypeByIndex getChildTypeByIndex = new CloneMethod.GetChildTypeByIndex() { Child = node, Index = index, Parent = currentClone };
                    currentClone.Apply(getChildTypeByIndex);
                    currentClone = getChildTypeByIndex.Child;
                }

                if (node is ANamedType && data.GenericLinks.ContainsKey((ANamedType) node))
                {
                    TIdentifier name = data.GenericLinks[(ANamedType) node];
                    AStructDecl str = (AStructDecl) name.Parent();

                    PType type = types[str.GetGenericVars().IndexOf(name)];
                    replacer = Util.MakeClone(type, data);
                }

            }

            public override void DefaultOut(Node node)
            {
                if (node is ANamedType && replacer != null)
                {
                    currentClone.ReplaceBy(replacer);
                    currentClone = replacer.Parent();
                    replacer = null;
                }
                else
                    currentClone = currentClone.Parent();

            }
        }

        public override void OutAGenericType(AGenericType node)
        {
            if (!(node.GetBase() is ANamedType))
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Base type must be a struct or a class."));
                return;
            }
            ANamedType Base = (ANamedType) node.GetBase();
            if (!data.StructTypeLinks.ContainsKey(Base))
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Base type must be a struct or a class."));
                return;
            }
            AStructDecl str = data.StructTypeLinks[Base];
            if (str.GetGenericVars().Count != node.GetGenericTypes().Count)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "The number of generic variables does not match.",
                                                     false, new ErrorCollection.Error(str.GetName(), "Target " + Util.GetTypeName(str))));
                return;
            }
            LookForGenericVar finder = new LookForGenericVar();
            foreach (PType genericType in node.GetGenericTypes())
            {
                genericType.Apply(finder);
                if (finder.ContainsGenericVar || finder.ContainsNestedGenerics)
                {
                    //if (finder.ContainsGenericVar)
                        structsWithGenerics.Add(Util.GetAncestor<AStructDecl>(node));
                    if (finder.ContainsNestedGenerics)
                    {
                        if (!Util.HasAncestor<AStructDecl>(node) || Util.GetAncestor<AStructDecl>(node).GetGenericVars().Count == 0)
                            needAnotherPass = true;
                    }
                    return;
                }
            }
            if (!Refferences.ContainsKey(str))
                Refferences[str] = new List<AGenericType>();
            Refferences[str].Add(node);
            base.OutAGenericType(node);
        }

        class LookForGenericVar : DepthFirstAdapter
        {
            public bool ContainsGenericVar = false;
            public bool ContainsNestedGenerics = false;

            public override void CaseANamedType(ANamedType node)
            {
                if (SharedData.LastCreated.GenericLinks.ContainsKey(node))
                    ContainsGenericVar = true;
            }

            public override void CaseAGenericType(AGenericType node)
            {
                ContainsNestedGenerics = true;
                base.CaseAGenericType(node);
            }
        }


        public override void OutAStructDecl(AStructDecl node)
        {
            if (node.GetGenericVars().Count > 0 && !Refferences.ContainsKey(node))
                Refferences[node] = new List<AGenericType>();
                
            base.OutAStructDecl(node);
        }

        public override void OutANamedType(ANamedType node)
        {
            //If using a generic type, you must us it as a generic
            if (data.StructTypeLinks.ContainsKey(node))
            {
                AStructDecl str = data.StructTypeLinks[node];
                if (str.GetGenericVars().Count > 0)
                {
                    if (!(node.Parent() is AGenericType))
                    {
                        errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                             "Target struct is a generic type. You must specify types for the generics.",
                                                             false,
                                                             new ErrorCollection.Error(str.GetName(),
                                                                                       "Matching " +
                                                                                       Util.GetTypeName(str))));
                    }
                }
            }
            base.OutANamedType(node);
        }

        /*private class StructCloner : DepthFirstAdapter
        {
            public static AStructDecl Clone(AStructDecl str, SharedData data)
            {
                AStructDecl clone = (AStructDecl) str.Clone();
                StructCloner cloner = new StructCloner(new AASourceFile(new ArrayList(){clone}, null, null, new ArrayList()), data);
                str.Apply(cloner);
                return clone;
            }

            private SharedData data;
            private Node currentClone;

            private StructCloner(Node currentClone, SharedData data)
            {
                this.currentClone = currentClone;
                this.data = data;
            }

            public override void DefaultIn(Node node)
            {
                int index = 0;
                CloneMethod.GetChildTypeIndex getChildTypeIndex = new CloneMethod.GetChildTypeIndex() { Parent = node.Parent(), Child = node };
                node.Parent().Apply(getChildTypeIndex);
                index = getChildTypeIndex.Index;
                CloneMethod.GetChildTypeByIndex getChildTypeByIndex = new CloneMethod.GetChildTypeByIndex()
                                                              {Child = node, Index = index, Parent = currentClone};
                currentClone.Apply(getChildTypeByIndex);
                currentClone = getChildTypeByIndex.Child;

                if (node is AStructDecl)
                    data.Structs.Add(
                        new SharedData.DeclItem<AStructDecl>(data.Structs.First(pair => pair.Decl == node).File,
                                                             (AStructDecl) currentClone));
            }

            public override void DefaultOut(Node node)
            {
                currentClone = currentClone.Parent();
            }

        }*/
    }
}
