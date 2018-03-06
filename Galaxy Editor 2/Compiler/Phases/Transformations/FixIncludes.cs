using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class FixIncludes
    {
        public static void Apply(AAProgram ast, FinalTransformations finalTrans)
        {
            //Build list of file dependacies
            Phase1 phase1 = new Phase1(finalTrans);
            ast.Apply(phase1);
            var dependancies = phase1.dependancies;
            if (dependancies.Keys.Count == 0) return;
            AASourceFile root = Util.GetAncestor<AASourceFile>(finalTrans.mainEntry) ??
                                dependancies.Keys.FirstOrDefault(file => !file.GetName().Text.Contains("\\")) ??
                                dependancies.Keys.First(file => true);

            //Remove files unreachable from root
            //On second thought, dont. there might be static refferences the other way which needs to be included
            /*{
                List<AASourceFile> reachable = GetReachable(root, dependancies);
                AASourceFile[] keys = new AASourceFile[dependancies.Count];
                dependancies.Keys.CopyTo(keys, 0);
                foreach (AASourceFile key in keys)
                {
                    if (!reachable.Contains(key))
                        dependancies.Remove(key);
                }
            }*/


            //Push common depancies up
            /*
             * root -> (item1 -> (item3), item2 -> (item4 -> (item3)))
             * 
             * root -> (item3, item1, item2 -> (item4))
             */
            
            //Add unreachable to the root
            while (true)
            {
                List<AASourceFile> reachable = new List<AASourceFile>();
                GetReachable(root, dependancies, ref reachable);
                if (reachable.Count == dependancies.Count + (reachable.Contains(null) ? 1 : 0)) break;
                AASourceFile[] keys = new AASourceFile[dependancies.Count];
                dependancies.Keys.CopyTo(keys, 0);
                foreach (AASourceFile key in keys)
                {
                    if (!reachable.Contains(key))
                    {
                        AASourceFile k = key;
                        //See if you can find another unreachable file which need this file
                        Dictionary<AASourceFile, int> decendantCounts = new Dictionary<AASourceFile, int>();
                        decendantCounts.Add(k, CountDecendants(k, dependancies, new List<AASourceFile>()));
                        while (true)
                        {
                            AASourceFile file = null;
                            foreach (KeyValuePair<AASourceFile, List<AASourceFile>> dependancy in dependancies)
                            {
                                if (decendantCounts.ContainsKey(dependancy.Key))
                                    continue;
                                if (!dependancy.Value.Contains(k))
                                    continue;
                                file = dependancy.Key;
                                break;
                            }

                            //AASourceFile file = dependancies.FirstOrDefault(item => item.Value.Contains(k)).Key;
                            if (file == null) break;
                            decendantCounts.Add(file, CountDecendants(file, dependancies, new List<AASourceFile>()));
                            k = file;
                        }
                        foreach (KeyValuePair<AASourceFile, int> decendantItem in decendantCounts)
                        {
                            if (decendantItem.Value > decendantCounts[k])
                                k = decendantItem.Key;
                        }

                        dependancies[root].Add(k);
                        break;
                    }
                }
            }
            //It is moved down here because cycles are not removed in unreachable
            RemoveCycles(root, dependancies, new List<AASourceFile> { root });


            //Convert to tree to make it easier
            List<Item> allItems = new List<Item>();
            IncludeItem rootIncludeItem = MakeTree(root, dependancies, allItems, null);
            bool[] removed = new bool[allItems.Count];
            for (int i = 0; i < removed.Length; i++)
                removed[i] = false;
            int removedCount = 0;

            //Ensure that each include is only included one place
            for (int i = 0; i < allItems.Count; i++)
            {
                if (removed[i])
                    continue;

                IncludeItem item1 = (IncludeItem)allItems[i];
                for (int j = i + 1; j < allItems.Count; j++)
                {
                    if (removed[j])
                        continue;
                    IncludeItem item2 = (IncludeItem)allItems[j];
                    
                    if (item1.Current == item2.Current)
                    {
                        List<Item> path1 = item1.Path;
                        List<Item> path2 = item2.Path;



                        for (int k = 0; k < Math.Min(path1.Count, path2.Count); k++)
                        {
                            if (path1[k] != path2[k])
                            {



                                int insertAt = Math.Min(path1[k - 1].Children.IndexOf(path1[k]),
                                                        path2[k - 1].Children.IndexOf(path2[k]));


                                item1.Parent.Children.Remove(item1);
                                LinkedList<IncludeItem> toRemove = new LinkedList<IncludeItem>();
                                toRemove.AddLast(item2);
                                while (toRemove.Count > 0)
                                {
                                    IncludeItem item = toRemove.First.Value;
                                    toRemove.RemoveFirst();
                                    item.Parent.Children.Remove(item);
                                    //allItems.Remove(item);
                                    removedCount++;
                                    removed[item.ListIndex] = true;
                                    foreach (IncludeItem child in item.Children)
                                    {
                                        toRemove.AddLast(child);
                                    }
                                }
                                //j--;




                                path1[k - 1].Children.Insert(insertAt, item1);
                                item1.Parent = path1[k - 1];



                                break;
                            }
                        }
                    }
                }
            }

            List<Item> newAllItems = new List<Item>(allItems.Count - removedCount);
            for (int i = 0; i < allItems.Count; i++)
                if (!removed[i])
                    newAllItems.Add(allItems[i]);
            allItems = newAllItems;

            //Move the null node to nr [0]
            foreach (IncludeItem item in allItems)
            {
                if (item.Current == null)
                {
                    int itemIndex = item.Parent.Children.IndexOf(item);
                    Item item0 = item.Parent.Children[0];
                    item.Parent.Children[0] = item;
                    item.Parent.Children[itemIndex] = item0;
                    break;
                }
            }

            //Insert method decls and move structs & fields up as needed
            ast.Apply(new Phase2(finalTrans, allItems));

            //Insert the headers in the files
            
            if (Options.Compiler.OneOutputFile)
            {
                //for (int i = 0; i < allItems.Count; i++)
                int i = 0;
                while (allItems.Count > 0)
                {
                    if (allItems[i] is IncludeItem)
                    {
                        IncludeItem includeItem = (IncludeItem) allItems[i];
                        //Dont want the standard lib
                        if (includeItem.Current == null)
                        {
                            i++;
                            continue;
                        }
                        //If it has children with children, then pick another first
                        if (includeItem.Children.Any(child => child.Children.Count > 0))
                        {
                            i++;
                            continue;
                        }
                        if (includeItem.Children.Count == 0)
                        {
                            if (includeItem.Parent == null)
                                break;
                            i++;
                            continue;
                        }
                        i = 0;
                        //Put all children into this
                        while (includeItem.Children.Count > 0)
                        {
                            int childNr = includeItem.Children.Count - 1;
                            allItems.Remove(includeItem.Children[childNr]);
                            if (includeItem.Children[childNr] is FieldItem)
                            {
                                FieldItem aItem = (FieldItem)includeItem.Children[childNr];
                                Node node = aItem.FieldDecl;
                                node.Parent().RemoveChild(node);
                                includeItem.Current.GetDecl().Insert(0, node);
                            }
                            else if (includeItem.Children[childNr] is StructItem)
                            {
                                StructItem aItem = (StructItem)includeItem.Children[childNr];
                                Node node = aItem.StructDecl;
                                node.Parent().RemoveChild(node);
                                includeItem.Current.GetDecl().Insert(0, node);
                            }
                            else if (includeItem.Children[childNr] is MethodDeclItem)
                            {
                                MethodDeclItem aItem = (MethodDeclItem)includeItem.Children[childNr];
                                AMethodDecl aNode = new AMethodDecl();
                                if (aItem.RealDecl.GetStatic() != null) aNode.SetStatic(new TStatic("static"));
                                aNode.SetReturnType(Util.MakeClone(aItem.RealDecl.GetReturnType(), finalTrans.data));
                                aNode.SetName(new TIdentifier(aItem.RealDecl.GetName().Text));
                                foreach (AALocalDecl formal in aItem.RealDecl.GetFormals())
                                {
                                    AALocalDecl clone = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(formal.GetType(), finalTrans.data), new TIdentifier(formal.GetName().Text), null);
                                    aNode.GetFormals().Add(clone);
                                }
                                includeItem.Current.GetDecl().Insert(0, aNode);
                            }
                            else if (includeItem.Children[childNr] is IncludeItem)
                            {
                                IncludeItem aChild = (IncludeItem)includeItem.Children[childNr];
                                if (aChild.Current == null)
                                {
                                    AIncludeDecl node = new AIncludeDecl(new TInclude("include"),
                                                        new TStringLiteral("\"TriggerLibs/NativeLib\""));
                                    includeItem.Current.GetDecl().Insert(0, node);
                                }
                                else
                                {
                                    PDecl[] decls = new PDecl[aChild.Current.GetDecl().Count];
                                    aChild.Current.GetDecl().CopyTo(decls, 0);
                                    for (int k = decls.Length - 1; k >= 0; k--)
                                    {
                                        includeItem.Current.GetDecl().Insert(0, decls[k]);
                                    }
                                    aChild.Current.Parent().RemoveChild(aChild.Current);
                                    //i = -1;
                                }
                            }
                            includeItem.Children.RemoveAt(childNr);
                        }
                    }
                }
            }
            else
                foreach (IncludeItem includeItem in allItems.OfType<IncludeItem>())
                {
                    for (int i = includeItem.Children.Count - 1; i >= 0; i--)
                    {
                        Node node;
                        if (includeItem.Children[i] is IncludeItem)
                        {
                            IncludeItem aItem = (IncludeItem) includeItem.Children[i];
                            node = new AIncludeDecl(new TInclude("include"),
                                                    new TStringLiteral("\"" + (aItem.Current == null ? "TriggerLibs/NativeLib" : aItem.Current.GetName().Text.Replace("\\", "/")) + "\""));
                            if (aItem.Current == null && finalTrans.mainEntry != null)
                            {
                                //Search for user defined initlib
                                bool foundInvoke = false;
                                foreach (ASimpleInvokeExp invokeExp in finalTrans.data.SimpleMethodLinks.Keys)
                                {
                                    if(invokeExp.GetName().Text == "libNtve_InitLib" && invokeExp.GetArgs().Count == 0)
                                    {
                                        /*finalTrans.errors.Add(new ErrorCollection.Error(invokeExp.GetName(),
                                                                                        Util.GetAncestor<AASourceFile>(
                                                                                            invokeExp),
                                                                                        "You are invoking libNtve_InitLib() yourself somewhere. It will not be auto inserted.",
                                                                                        true));*/
                                        foundInvoke = true;
                                        break;
                                    }
                                }

                                if (!foundInvoke)
                                {
                                    //Init the lib
                                    ASimpleInvokeExp initExp = new ASimpleInvokeExp();
                                    initExp.SetName(new TIdentifier("libNtve_InitLib"));
                                    finalTrans.data.ExpTypes[initExp] = new AVoidType(new TVoid("void"));
                                    foreach (AMethodDecl method in finalTrans.data.Libraries.Methods)
                                    {
                                        if (method.GetName().Text == "libNtve_InitLib" && method.GetFormals().Count == 0)
                                        {
                                            finalTrans.data.SimpleMethodLinks[initExp] = method;
                                        }
                                    }
                                    AABlock block = (AABlock) finalTrans.mainEntry.GetBlock();
                                    block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), initExp));
                                }
                            }
                        }
                        else if (includeItem.Children[i] is FieldItem)
                        {
                            FieldItem aItem = (FieldItem)includeItem.Children[i];
                            node = aItem.FieldDecl;
                            node.Parent().RemoveChild(node);
                        }
                        else if (includeItem.Children[i] is StructItem)
                        {
                            StructItem aItem = (StructItem)includeItem.Children[i];
                            node = aItem.StructDecl;
                            node.Parent().RemoveChild(node);
                        }
                        else if (includeItem.Children[i] is MethodDeclItem)
                        {
                            MethodDeclItem aItem = (MethodDeclItem)includeItem.Children[i];
                            AMethodDecl aNode = new AMethodDecl();
                            if (aItem.RealDecl.GetStatic() != null) aNode.SetStatic(new TStatic("static"));
                            aNode.SetReturnType(Util.MakeClone(aItem.RealDecl.GetReturnType(), finalTrans.data));
                            aNode.SetName(new TIdentifier(aItem.RealDecl.GetName().Text));
                            foreach (AALocalDecl formal in aItem.RealDecl.GetFormals())
                            {
                                AALocalDecl clone = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(formal.GetType(), finalTrans.data), new TIdentifier(formal.GetName().Text), null);
                                aNode.GetFormals().Add(clone);
                            }
                            node = aNode;
                        }
                        else
                            throw new Exception("FixIncludes.Apply: Unexpected item type");

                        includeItem.Current.GetDecl().Insert(0, node);
                    }
                }

            

            
        }

        private static int CountDecendants(AASourceFile file, Dictionary<AASourceFile, List<AASourceFile>> dependancies, List<AASourceFile> countedFiles)
        {
            if (file == null)
                return 1;

            countedFiles.Add(file);

            foreach (AASourceFile sourceFile in dependancies[file])
            {
                if (!countedFiles.Contains(sourceFile))
                    CountDecendants(sourceFile, dependancies, countedFiles);
            }
            return countedFiles.Count;
        }

        private abstract class Item
        {
            public List<Item> Children { get; set; }
            public Item Parent { get; set; }

            public Item(Item parent, List<Item> children)
            {
                Parent = parent;
                Children = children;
            }

            public List<Item> Path
            {
                get
                {
                    List<Item> list = new List<Item>();
                    if (Parent != null) list.AddRange(Parent.Path);
                    list.Add(this);
                    return list;
                }
            }
        }

        private class IncludeItem : Item
        {
            public AASourceFile Current;
            public int ListIndex;

            public IncludeItem(AASourceFile current, int listIndex, IncludeItem parent, List<Item> children) : base(parent, children)
            {
                Current = current;
                ListIndex = listIndex;
            }
        }

        private class MethodDeclItem : Item
        {
            public AMethodDecl RealDecl;

            public MethodDeclItem(AMethodDecl realDecl, Item parent, List<Item> children) : base(parent, children)
            {
                RealDecl = realDecl;
            }
        }

        private class FieldItem : Item
        {
            public AFieldDecl FieldDecl;

            public FieldItem(AFieldDecl fieldDecl, Item parent, List<Item> children)
                : base(parent, children)
            {
                FieldDecl = fieldDecl;
            }
        }

        private class StructItem : Item
        {
            public AStructDecl StructDecl;

            public StructItem(AStructDecl structDecl, Item parent, List<Item> children)
                : base(parent, children)
            {
                StructDecl = structDecl;
            }
        }

        private static IncludeItem MakeTree(AASourceFile node, Dictionary<AASourceFile, List<AASourceFile>> dependancies, List<Item> allItems, IncludeItem mainFile)
        {
            IncludeItem includeItem = new IncludeItem(node, allItems.Count, null, new List<Item>());

            //Ensure that each include is only included one place
            IncludeItem item2 = includeItem;
            bool removed = false;
            for (int i = 0; i < allItems.Count; i++)
            {

                IncludeItem item1 = (IncludeItem)allItems[i];
                if (item1.Current == item2.Current)
                {
                    List<Item> path1 = item1.Path;
                    List<Item> path2 = item2.Path;



                    for (int k = 0; k < Math.Min(path1.Count, path2.Count); k++)
                    {
                        if (path1[k] != path2[k])
                        {

                            int insertAt;
                            IncludeItem insertIn;
                            if (k != 0)
                            {
                                int index1 = path1[k - 1].Children.IndexOf(path1[k]);
                                int index2 = path2[k - 1].Children.IndexOf(path2[k]);
                                if (index2 == -1)
                                    index2 = path2[k - 1].Children.Count;
                                insertAt = Math.Min(index1, index2);
                                insertIn = (IncludeItem) path1[k - 1];
                            }
                            else
                            {
                               


                                if (path1[0] == mainFile)
                                {
                                    int index1 = mainFile.Children.IndexOf(path1[1]);
                                    if (path2[0] == mainFile)
                                    {
                                        int index2 = mainFile.Children.IndexOf(path2[1]);
                                        if (index2 == -1)
                                        {
                                            index2 = mainFile.Children.Count;
                                            if (path1.Count == 2)
                                                index2--;//Since item1 will be removed
                                        }
                                        insertAt = Math.Min(index1, index2);
                                    }
                                    else
                                        insertAt = index1;
                                }
                                else if (path2[0] == mainFile)
                                {
                                    int index2 = mainFile.Children.IndexOf(path2[1]);
                                    if (index2 == -1)
                                        index2 = mainFile.Children.Count;
                                    insertAt = index2;
                                }
                                else
                                    insertAt = mainFile.Children.Count;
                                insertIn = mainFile;
                            }

                            if (item1.Parent != insertIn || insertIn.Children.IndexOf(item1) != insertAt)
                            {
                                item1.Parent.Children.Remove(item1);
                                insertIn.Children.Insert(insertAt, item1);
                                item1.Parent = insertIn;
                            }
                            removed = true;

                            break;
                        }
                    }
                    
                }
            }
            if (removed)
                return null;

            allItems.Add(includeItem);
            if (node != null)
                foreach (AASourceFile file in dependancies[node])
                {
                    IncludeItem child = MakeTree(file, dependancies, allItems, mainFile ?? includeItem);
                    if (child == null)
                        continue;
                    child.Parent = includeItem;
                    includeItem.Children.Add(child);
                }
            return includeItem;
        }

        private static void RemoveCycles(AASourceFile file, Dictionary<AASourceFile, List<AASourceFile>> dependancies, List<AASourceFile> path)
        {
            if (file == null) return;
            for (int i = 0; i < dependancies[file].Count; i++)
            {
                AASourceFile nextFile = dependancies[file][i];
                if (path.Contains(nextFile))
                    dependancies[file].RemoveAt(i--);
                else
                {
                    path.Add(nextFile);
                    RemoveCycles(nextFile, dependancies, path);
                    path.Remove(nextFile);
                }
            }
        }

        private static void GetReachable(AASourceFile node, Dictionary<AASourceFile, List<AASourceFile>> dependancies, ref List<AASourceFile> reachable)
        {
            if (reachable.Contains(node))
                return;
            reachable.Add(node);
            if (node != null)
                foreach (AASourceFile file in dependancies[node])
                {
                    GetReachable(file, dependancies, ref reachable);
                    
                }
        }

        
        private class Phase1 : DepthFirstAdapter
        {
            public Dictionary<AASourceFile, List<AASourceFile>> dependancies = new Dictionary<AASourceFile, List<AASourceFile>>();
            private FinalTransformations finalTrans;

            public Phase1(FinalTransformations finalTrans)
            {
                this.finalTrans = finalTrans;
            }

            public override void InAASourceFile(AASourceFile node)
            {
                dependancies.Add(node, new List<AASourceFile>());
            }

            private void AddDepency(AASourceFile currentFile, AASourceFile requiredFile)
            {
                //If requiredFile is null, it is a part of the standard library
                //If it's the same file, no includes are required
                if (currentFile == requiredFile)
                    return;
                if (dependancies[currentFile].Contains(requiredFile))
                    return;
                dependancies[currentFile].Add(requiredFile);
            }

            public override void InASimpleInvokeExp(ASimpleInvokeExp node)
            {
                AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
                AddDepency(Util.GetAncestor<AASourceFile>(node), Util.GetAncestor<AASourceFile>(decl));
            }

            public override void InAFieldLvalue(AFieldLvalue node)
            {
                AFieldDecl decl = finalTrans.data.FieldLinks[node];
                AddDepency(Util.GetAncestor<AASourceFile>(node), Util.GetAncestor<AASourceFile>(decl));
            }

            public override void InANamedType(ANamedType node)
            {
                if (!node.IsPrimitive())// !GalaxyKeywords.Primitives.words.Any(word => word == node.GetName().Text))
                {
                    AStructDecl decl = finalTrans.data.StructTypeLinks[node];
                    AddDepency(Util.GetAncestor<AASourceFile>(node), Util.GetAncestor<AASourceFile>(decl));
                }
            }
        }


        //Make method decls, and move structs and fields up
        private class Phase2 : DepthFirstAdapter
        {
            private List<Item> allItems;

            private FinalTransformations finalTrans;

            public Phase2(FinalTransformations finalTrans, List<Item> allItems)
            {
                this.finalTrans = finalTrans;
                this.allItems = allItems;
            }

            private IncludeItem GetIncludeItem(Node node)
            {
                AASourceFile file = Util.GetAncestor<AASourceFile>(node);
                return allItems.OfType<IncludeItem>().FirstOrDefault(inclItem => inclItem.Current == file);
            }

            private bool IsVisible(Node currentNode, Node declNode)
            {
                IncludeItem currentFile = GetIncludeItem(currentNode);
                IncludeItem declFile = GetIncludeItem(declNode);

                List<Item> currentPath = currentFile.Path;
                List<Item> declPath = declFile.Path;
                for (int i = 0; i < Math.Min(declPath.Count, currentPath.Count); i++)
                {
                    Item currentItem = currentPath[i];
                    Item declItem = declPath[i];
                    if (declItem != currentItem)
                    {
                        int cI = currentPath[i - 1].Children.IndexOf(currentItem);
                        int dI = declPath[i - 1].Children.IndexOf(declItem);
                        return dI < cI;
                    }
                    if (i == declPath.Count - 1)
                    {
                        return false;
                    }
                    if (i == currentPath.Count - 1)
                    {
                        return true;
                    }
                }
                //Shouldn't get here
                return false;
            }

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                Item currentFile = GetIncludeItem(node);

                //If the method invocation is in a moved field, refference that field instead
                if (Util.GetAncestor<AFieldDecl>(node) != null)
                {
                    Item i = allItems.OfType<FieldItem>().FirstOrDefault(item => item.FieldDecl == Util.GetAncestor<AFieldDecl>(node));
                    if (i != null)
                        currentFile = i;
                }

                AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
                Item declItem = ((Item)allItems.OfType<MethodDeclItem>().FirstOrDefault(item => item.RealDecl == decl)) ??
                                allItems.OfType<IncludeItem>().First(
                                    item => item.Current == Util.GetAncestor<AASourceFile>(decl));

                List<Item> cPath = currentFile.Path;
                List<Item> dPath = declItem.Path;

                for (int i = 0; i < Math.Min(cPath.Count, dPath.Count); i++)
                {
                    Item cItem = cPath[i];
                    Item dItem = dPath[i];

                    if (cItem != dItem)
                    {//We have a fork in the decls. make sure that the decl is before the used
                        int cI = cPath[i - 1].Children.IndexOf(cItem);
                        int dI = dPath[i - 1].Children.IndexOf(dItem);
                        if (dI < cI)
                        {
                            break;
                        }
                        //Move the decl up before the used
                        if (!(declItem is MethodDeclItem))
                        {
                            declItem = new MethodDeclItem(decl, cPath[i - 1], new List<Item>());
                            allItems.Add(declItem);
                        }
                        else
                        {
                            declItem.Parent.Children.Remove(declItem);
                            declItem.Parent = cPath[i - 1];
                        }
                        cPath[i - 1].Children.Insert(cI, declItem);
                        break;
                    }
                    if (i == cPath.Count - 1)
                    {
                        if (i == dPath.Count - 1)
                        {
                            //The decl and use is in same file. Ensure that the decl is before
                            if (Util.TokenLessThan(decl.GetName(), node.GetName()))
                                break;
                            //Add the decl item
                            declItem = new MethodDeclItem(decl, cPath[i], new List<Item>());
                            allItems.Add(declItem);
                            cPath[i].Children.Add(declItem);
                            break;
                        }
                        else
                        {
                            //The decl is included here or somewhere deeper. But above the use
                            break;
                        }
                    }
                    else if (i == dPath.Count - 1)
                    {
                        //We have reached the file where the decl is, but the use is included deeper, so it is above. Insert decl
                        int cI = cPath[i].Children.IndexOf(cPath[i + 1]);
                        declItem = new MethodDeclItem(decl, cPath[i], new List<Item>());
                        allItems.Add(declItem);
                        cPath[i].Children.Insert(cI, declItem);
                        break;
                    }
                }

                base.CaseASimpleInvokeExp(node);
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                Item currentFile = GetIncludeItem(node);

                //If the field is in a moved field, refference that field instead
                if (Util.GetAncestor<AFieldDecl>(node) != null)
                {
                    Item i = allItems.OfType<FieldItem>().FirstOrDefault(item => item.FieldDecl == Util.GetAncestor<AFieldDecl>(node));
                    if (i != null)
                        currentFile = i;
                }

                AFieldDecl decl = finalTrans.data.FieldLinks[node];
                Item declItem = ((Item) allItems.OfType<FieldItem>().FirstOrDefault(item => item.FieldDecl == decl)) ??
                                allItems.OfType<IncludeItem>().First(
                                    item => item.Current == Util.GetAncestor<AASourceFile>(decl));
                List<Item> cPath = currentFile.Path;
                List<Item> dPath = declItem.Path;

                bool movedIt = false;
                for (int i = 0; i < Math.Min(cPath.Count, dPath.Count); i++)
                {
                    if (cPath[i] != dPath[i])
                    {
                        //We have a fork. make sure that the field is visible
                        int cI = cPath[i - 1].Children.IndexOf(cPath[i]);
                        int dI = dPath[i - 1].Children.IndexOf(dPath[i]);

                        if (dI < cI)
                        {//The decl is okay
                            break;
                        }

                        //Move the decl up
                        if (declItem is FieldItem)
                        {
                            declItem.Parent.Children.Remove(declItem);
                            declItem.Parent = cPath[i - 1];
                        }
                        else
                        {
                            declItem = new FieldItem(decl, cPath[i - 1], new List<Item>());
                            allItems.Add(declItem);
                        }
                        cPath[i - 1].Children.Insert(cI, declItem);
                        movedIt = true;
                        break;
                    }
                    if (i == cPath.Count - 1)
                    {
                        if (i == dPath.Count - 1)
                        {
                            //The decl and use is in same file. Ensure that the decl is before
                            if (Util.TokenLessThan(decl.GetName(), node.GetName()))
                                break;
                            //Add the decl item
                            declItem = new FieldItem(decl, cPath[i], new List<Item>());
                            allItems.Add(declItem);
                            cPath[i].Children.Add(declItem);
                            movedIt = true;
                            break;
                        }
                        else
                        {
                            //The decl is included here or somewhere deeper. But above the use
                            break;
                        }
                    }
                    else if (i == dPath.Count - 1)
                    {
                        //We have reached the file where the decl is, but the use is included deeper, so it is above. Insert decl
                        int cI = cPath[i].Children.IndexOf(cPath[i + 1]);
                        declItem = new FieldItem(decl, cPath[i], new List<Item>());
                        allItems.Add(declItem);
                        cPath[i].Children.Insert(cI, declItem);
                        movedIt = true;
                        break;
                    }
                }

                //In case we have a struct type field, we must make sure the struct is still on top
                if (movedIt)
                    CaseAFieldDecl(decl);
                base.CaseAFieldLvalue(node);
            }

            public override void CaseANamedType(ANamedType node)
            {
                //Remember.. if you are the child of a fieldDecl, that field may have been moved
                if (!node.IsPrimitive())//GalaxyKeywords.Primitives.words.Any(word => word == node.GetName().Text)))
                {
                    AStructDecl decl = finalTrans.data.StructTypeLinks[node];
                    Item currentItem = GetIncludeItem(node);
                    if (Util.GetAncestor<AFieldDecl>(node) != null)
                    {
                        Item i = allItems.OfType<FieldItem>().FirstOrDefault(item => item.FieldDecl == Util.GetAncestor<AFieldDecl>(node));
                        if (i != null)
                            currentItem = i;
                    }
                    if (Util.GetAncestor<AStructDecl>(node) != null)
                    {
                        Item i =
                            allItems.OfType<StructItem>().FirstOrDefault(
                                item => item.StructDecl == Util.GetAncestor<AStructDecl>(node));
                        if (i != null)
                            currentItem = i;
                    }
                    Item declItem = ((Item)allItems.OfType<StructItem>().FirstOrDefault(item => item.StructDecl == decl)) ??
                                allItems.OfType<IncludeItem>().First(
                                    item => item.Current == Util.GetAncestor<AASourceFile>(decl));

                    List<Item> cPath = currentItem.Path;
                    List<Item> dPath = declItem.Path;

                    for (int i = 0; i < Math.Min(cPath.Count, dPath.Count); i++)
                    {
                        if (cPath[i] != dPath[i])
                        {//FORK!!!!
                            //We have a fork. make sure that the field is visible
                            int cI = cPath[i - 1].Children.IndexOf(cPath[i]);
                            int dI = dPath[i - 1].Children.IndexOf(dPath[i]);

                            if (dI < cI)
                            {//The decl is okay
                                break;
                            }

                            //Move the decl up
                            if (declItem is StructItem)
                            {
                                declItem.Parent.Children.Remove(declItem);
                                declItem.Parent = cPath[i - 1];
                            }
                            else
                            {
                                declItem = new StructItem(decl, cPath[i - 1], new List<Item>());
                                allItems.Add(declItem);
                            }
                            cPath[i - 1].Children.Insert(cI, declItem);
                            break;
                        }
                        if (i == cPath.Count - 1)
                        {
                            if (i == dPath.Count - 1)
                            {
                                //The decl and use is in same file. Ensure that the decl is before
                                if (Util.TokenLessThan(decl.GetName(), node.GetToken()))
                                    break;
                                //Add the decl item
                                declItem = new StructItem(decl, cPath[i], new List<Item>());
                                allItems.Add(declItem);
                                cPath[i].Children.Add(declItem);
                                break;
                            }
                            else
                            {
                                //The decl is included here or somewhere deeper. But above the use
                                break;
                            }
                        }
                        else if (i == dPath.Count - 1)
                        {
                            //We have reached the file where the decl is, but the use is included deeper, so it is above. Insert decl
                            int cI = cPath[i].Children.IndexOf(cPath[i + 1]);
                            declItem = new StructItem(decl, cPath[i], new List<Item>());
                            allItems.Add(declItem);
                            cPath[i].Children.Insert(cI, declItem);
                            break;
                        }
                    }

                }
                base.CaseANamedType(node);
            }
        }
    }
}
