using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class LinkNamedTypes : DepthFirstAdapter
    {
        private SharedData data;
        private ErrorCollection errors;

        public LinkNamedTypes(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        public override void OutAAProgram(AAProgram node)
        {
            //Link stuff in delegates
           /* foreach (SharedData.DeclItem<AMethodDecl> declItem in data.Delegates)
            {
                //declItem.File.GetDecl().Add(declItem.Decl);
                declItem.Decl.Apply(this);
                //declItem.File.GetDecl().Remove(declItem.Decl);
            }*/
            //Remove typedefs
            foreach (ATypedefDecl decl in data.Typedefs)
            {
                decl.Parent().RemoveChild(decl);
            }
            data.Typedefs.Clear();
            base.OutAAProgram(node);
            
        }

        public static void GetMatchingTypes(ANamedType node, List<ATypedefDecl> typeDefs, List<AStructDecl> structs, List<AMethodDecl> delegates, List<TIdentifier> generics, out bool matchPrimitive)
        {
            List<string> names = new List<string>();
            foreach (TIdentifier identifier in ((AAName)node.GetName()).GetIdentifier())
            {
                names.Add(identifier.Text);
            }
            matchPrimitive = names.Count == 1 && GalaxyKeywords.Primitives.words.Contains(names[0]);
            GetMatchingTypes(node, names, typeDefs, structs, delegates, new List<ANamespaceDecl>(), generics);
        }

        private static void GetMatchingTypes(ANamedType node, List<string> names, List<ATypedefDecl> typeDefs, List<AStructDecl> structs, List<AMethodDecl> delegates, List<ANamespaceDecl> namespaces, List<TIdentifier> generics)
        {
            List<IList> decls = new List<IList>();
            List<string> currentNamespace = Util.GetFullNamespace(node);
            AASourceFile currentSourceFile = Util.GetAncestor<AASourceFile>(node);
            if (names.Count == 1)
            {
                string name = names[0];
                //Check generic vars
                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                if (currentStruct != null)
                {
                    foreach (TIdentifier genericVar in currentStruct.GetGenericVars())
                    {
                        if (genericVar.Text == name)
                            generics.Add(genericVar);
                    }
                }
                //Get all type decls and namespaces matching this name, visible from this location
                List<IList> visibleDecls = Util.GetVisibleDecls(node, ((AAName)node.GetName()).GetIdentifier().Count == 1);
                foreach (IList declList in visibleDecls)
                {
                    bool sameFile = false;
                    if (declList.Count > 0)
                        sameFile = currentSourceFile == Util.GetAncestor<AASourceFile>((PDecl) declList[0]);
                    foreach (PDecl decl in declList)
                    {
                        bool sameNS = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));
                        if (decl is ANamespaceDecl)
                        {
                            ANamespaceDecl aDecl = (ANamespaceDecl) decl;
                            if (aDecl.GetName().Text == name)
                                namespaces.Add(aDecl);
                            continue;
                        }
                        if (decl is ATypedefDecl)
                        {
                            if (Util.IsAncestor(node, decl))
                                continue;
                            ATypedefDecl aDecl = (ATypedefDecl)decl;
                            if (aDecl.GetStatic() != null && !sameFile ||
                                aDecl.GetVisibilityModifier() is APrivateVisibilityModifier && !sameNS)
                                continue;
                            ANamedType namedType = (ANamedType) aDecl.GetName();
                            AAName aName = (AAName) namedType.GetName();
                            string n = ((TIdentifier) aName.GetIdentifier()[0]).Text;
                            if (n == name)
                                typeDefs.Add(aDecl);
                            continue;
                        }
                        if (decl is AStructDecl)
                        {
                            AStructDecl aDecl = (AStructDecl) decl;
                            if (!sameNS && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier)
                                continue;
                            if (aDecl.GetName().Text == name)
                                structs.Add(aDecl);
                            continue;
                        }
                        if (decl is AMethodDecl)
                        {
                            AMethodDecl aDecl = (AMethodDecl)decl;
                            if (!sameNS && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !sameFile && aDecl.GetStatic() != null)
                                continue;
                            if (aDecl.GetDelegate() != null && aDecl.GetName().Text == name)
                                delegates.Add(aDecl);
                            continue;
                        }
                    }
                }
            }
            else
            {
                string name = names[names.Count - 1];
                List<ANamespaceDecl> baseNamespaces = new List<ANamespaceDecl>();
                List<string> baseNames = new List<string>();
                baseNames.AddRange(names);
                baseNames.RemoveAt(baseNames.Count - 1);
                GetMatchingTypes(node, baseNames, new List<ATypedefDecl>(), new List<AStructDecl>(), new List<AMethodDecl>(), baseNamespaces, generics);
                foreach (ANamespaceDecl ns in baseNamespaces)
                {
                    bool sameFile = currentSourceFile == Util.GetAncestor<AASourceFile>(ns);
                    foreach (PDecl decl in ns.GetDecl())
                    {
                        bool sameNS = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));
                        if (decl is ANamespaceDecl)
                        {
                            ANamespaceDecl aDecl = (ANamespaceDecl)decl;
                            if (aDecl.GetName().Text == name)
                                namespaces.Add(aDecl);
                            continue;
                        }
                        if (decl is ATypedefDecl)
                        {
                            ATypedefDecl aDecl = (ATypedefDecl)decl;
                            ANamedType namedType = (ANamedType)aDecl.GetName();
                            AAName aName = (AAName)namedType.GetName();
                            string n = ((TIdentifier)aName.GetIdentifier()[0]).Text;
                            if (n == name)
                                typeDefs.Add(aDecl);
                            continue;
                        }
                        if (decl is AStructDecl)
                        {
                            AStructDecl aDecl = (AStructDecl)decl;
                            if (!sameNS && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier)
                                continue;
                            if (aDecl.GetName().Text == name)
                                structs.Add(aDecl);
                            continue;
                        }
                        if (decl is AMethodDecl)
                        {
                            AMethodDecl aDecl = (AMethodDecl)decl;
                            if (!sameNS && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !sameFile && aDecl.GetStatic() != null)
                                continue;
                            if (aDecl.GetDelegate() != null && aDecl.GetName().Text == name)
                                delegates.Add(aDecl);
                            continue;
                        }
                    }
                }
            }
        }

        public override void OutANamedType(ANamedType node)
        {
            if (node.Parent() is ATypedefDecl && ((ATypedefDecl)node.Parent()).GetName() == node)
                return;

            //Link named type to their definition (structs)
            List<ATypedefDecl> typeDefs = new List<ATypedefDecl>();
            List<AStructDecl> structs = new List<AStructDecl>();
            List<AMethodDecl> delegates = new List<AMethodDecl>();
            List<TIdentifier> generics = new List<TIdentifier>();
            bool matchPrimitive;
            GetMatchingTypes(node, typeDefs, structs, delegates, generics, out matchPrimitive);

            int matches = typeDefs.Count + structs.Count + delegates.Count + (matchPrimitive ? 1 : 0) + generics.Count;
            if (matches == 0)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Could not find any types matching " + ((AAName)node.GetName()).AsString()), true);
            }
            else if (generics.Count != 1 && matches > 1)
            {
                List<ErrorCollection.Error> subError = new List<ErrorCollection.Error>();
                if (matchPrimitive)
                    subError.Add(new ErrorCollection.Error(node.GetToken(), "Matches primitive " + ((AAName)node.GetName()).AsString()));
                foreach (ATypedefDecl typeDef in typeDefs)
                {
                    subError.Add(new ErrorCollection.Error(typeDef.GetToken(), "Matching typedef"));
                }
                foreach (AStructDecl structDecl in structs)
                {
                    subError.Add(new ErrorCollection.Error(structDecl.GetName(), "Matching " + Util.GetTypeName(structDecl)));
                }
                foreach (AMethodDecl methodDecl in delegates)
                {
                    subError.Add(new ErrorCollection.Error(methodDecl.GetName(), "Matching delegate"));
                }
                foreach (TIdentifier identifier in generics)
                {
                    subError.Add(new ErrorCollection.Error(identifier, "Matching generic"));
                }
                errors.Add(
                    new ErrorCollection.Error(node.GetToken(),
                                              "Found multiple types matching " + ((AAName) node.GetName()).AsString(),
                                              false, subError.ToArray()), true);
            }
            else
            {
                if (generics.Count == 1)
                {
                    data.GenericLinks[node] = generics[0];
                    return;
                }
                if (typeDefs.Count == 1)
                {
                    ATypedefDecl typeDef = typeDefs[0];
                    //data.TypeDefLinks[node] = typeDef;
                    PType type = (PType) typeDef.GetType().Clone();
                    node.ReplaceBy(type);
                    type.Apply(this);
                    return;
                }
                if (structs.Count == 1)
                {
                    data.StructTypeLinks[node] = structs[0];
                }
                else if (delegates.Count == 1)
                {
                    data.DelegateTypeLinks[node] = delegates[0];
                }
                if (!matchPrimitive && !(structs.Count == 1 && data.Enums.ContainsKey(structs[0])) && node.Parent() is AEnrichmentDecl) //Not allowed to enrich a struct, class or delegate
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "You can not enrich this type."));
                }
            }
        }
    }
}
