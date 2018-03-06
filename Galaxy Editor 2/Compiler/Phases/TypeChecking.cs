using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.NotGenerated;
using Galaxy_Editor_2.Compiler.Phases.Transformations;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class TypeChecking : DepthFirstAdapter
    {
        private Dictionary<AMethodDecl, List<AMethodDecl>> InlineMethodCalls = new Dictionary<AMethodDecl, List<AMethodDecl>>();
        private Dictionary<AStructDecl, List<AStructDecl>> StructDepandancies = new Dictionary<AStructDecl, List<AStructDecl>>();

        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data)
        {
            ast.Apply(new TypeChecking(errors, data));
        }

        private ErrorCollection errors;
        private SharedData data;
        private List<AALocalDecl> assignedToOutParams = new List<AALocalDecl>();

        public TypeChecking(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        private AASourceFile currentSourceFile;
        public override void CaseAASourceFile(AASourceFile node)
        {
            currentSourceFile = node;
            base.CaseAASourceFile(node);
        }

        public override void OutAAProgram(AAProgram node)
        {
            List<AMethodDecl> checkedMethods = new List<AMethodDecl>();
            List<AMethodDecl> path = new List<AMethodDecl>();
            //Check that we dont have an inline method cycle
            foreach (KeyValuePair<AMethodDecl, List<AMethodDecl>> inlineMethodCall in InlineMethodCalls)
            {
                CheckInlineMethod(inlineMethodCall.Key, checkedMethods, path);
            }
            //Check that we dont have a struct dependancy cycle
            List<AStructDecl> checkedStructs = new List<AStructDecl>();
            List<AStructDecl> structPath = new List<AStructDecl>();
            foreach (KeyValuePair<AStructDecl, List<AStructDecl>> structDepandancy in StructDepandancies)
            {
                CheckStructDependancies(structDepandancy.Key, checkedStructs, structPath);
            }
        }

        private void CheckInlineMethod(AMethodDecl method, List<AMethodDecl> checkedMethods, List<AMethodDecl> path)
        {
            if (path.Contains(method))
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                for (int i = path.IndexOf(method); i < path.Count; i++)
                {
                    subErrors.Add(new ErrorCollection.Error(path[i].GetName(), Util.GetAncestor<AASourceFile>(path[i]), "Method in cycle"));
                }
                subErrors.Add(new ErrorCollection.Error(method.GetName(), Util.GetAncestor<AASourceFile>(method), "Method in cycle"));
                errors.Add(new ErrorCollection.Error(method.GetName(), Util.GetAncestor<AASourceFile>(method),
                                                     "You are not allowed to make a cycle of method calls with methods marked with inline",
                                                     false, subErrors.ToArray()));
            }
            if (!checkedMethods.Contains(method))
            {
                checkedMethods.Add(method);
                path.Add(method);
                foreach (AMethodDecl nextMethod in InlineMethodCalls[method])
                {
                    CheckInlineMethod(nextMethod, checkedMethods, path);
                }
                path.Remove(method);
            }
        }

        private void CheckStructDependancies(AStructDecl currentStr, List<AStructDecl> checkedStructs, List<AStructDecl> path)
        {
            if (path.Contains(currentStr))
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                for (int i = path.IndexOf(currentStr); i < path.Count; i++)
                {
                    subErrors.Add(new ErrorCollection.Error(path[i].GetName(), Util.GetAncestor<AASourceFile>(path[i]), "Struct in cycle"));
                }
                subErrors.Add(new ErrorCollection.Error(currentStr.GetName(), Util.GetAncestor<AASourceFile>(currentStr), "Struct in cycle"));
                errors.Add(new ErrorCollection.Error(currentStr.GetName(), Util.GetAncestor<AASourceFile>(currentStr),
                                                     "You are not allowed to make a cyclic dependancy with structs.",
                                                     false, subErrors.ToArray()));
            }
            if (!checkedStructs.Contains(currentStr))
            {
                checkedStructs.Add(currentStr);
                path.Add(currentStr);
                foreach (AStructDecl nextStruct in StructDepandancies[currentStr])
                {
                    CheckStructDependancies(nextStruct, checkedStructs, path);
                }
                path.Remove(currentStr);
            }
        }
        

        public override void OutAArrayLvalue(AArrayLvalue node)
        {
            PType type = data.ExpTypes[node.GetBase()];
            PType argType = data.ExpTypes[node.GetIndex()];
            
            List<APropertyDecl> matchingArrayProperties = new List<APropertyDecl>();
            List<APropertyDecl> implicitStructArrayProperties = new List<APropertyDecl>();

            if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)type))
            {
                AStructDecl structDecl = data.StructTypeLinks[(ANamedType) type];
                foreach (APropertyDecl property in data.StructProperties[structDecl])
                {
                    if (property.GetName().Text == "")
                    {
                        PType proprtyArgType = data.ArrayPropertyLocals[property][0].GetType();
                        if (Assignable(argType, proprtyArgType))
                            matchingArrayProperties.Add(property);
                    }
                }
            }

            if (type is APointerType)
            {
                APointerType aType = (APointerType)type;
                PType baseType = aType.GetType();
                if (baseType is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)baseType))
                {
                    AStructDecl structDecl = data.StructTypeLinks[(ANamedType)baseType];
                    foreach (APropertyDecl property in data.StructProperties[structDecl])
                    {
                        if (property.GetName().Text == "")
                        {
                            PType proprtyArgType = data.ArrayPropertyLocals[property][0].GetType();
                            if (Assignable(argType, proprtyArgType))
                                implicitStructArrayProperties.Add(property);
                        }
                    }
                }
            }
            
            List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
            foreach (IList declList in visibleDecls)
            {
                foreach (PDecl decl in declList)
                {
                    if (decl is AEnrichmentDecl)
                    {
                        AEnrichmentDecl enrichment = (AEnrichmentDecl)decl;
                        if (Util.TypesEqual(type, enrichment.GetType(), data))
                        {
                            foreach (PDecl enrichmentDecl in enrichment.GetDecl())
                            {
                                if (enrichmentDecl is APropertyDecl)
                                {
                                    APropertyDecl property = (APropertyDecl) enrichmentDecl;
                                    if (property.GetName().Text == "")
                                    {
                                        PType proprtyArgType = data.ArrayPropertyLocals[property][0].GetType();
                                        if (Assignable(argType, proprtyArgType))
                                            matchingArrayProperties.Add(property);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            bool matchesNormalArray = false;
            ALvalueExp replaceBaseExp = null;
            if (Assignable(argType, new ANamedType(new TIdentifier("int"), null)))
            {
                if (type is AArrayTempType)
                {
                    AArrayTempType aType = (AArrayTempType) type;
                    type = aType.GetType();
                    matchesNormalArray = true;
                }
                else if (type is ADynamicArrayType)
                {
                    ADynamicArrayType aType = (ADynamicArrayType) type;
                    type = aType.GetType();
                    matchesNormalArray = true;
                }
                else if (type is APointerType)
                {
                    //Implicit conversion for a[] to (*a)[]
                    APointerType aType = (APointerType) type;
                    if (aType.GetType() is AArrayTempType || aType.GetType() is ADynamicArrayType)
                    {
                        APointerLvalue pointer = new APointerLvalue(new TStar("*"), node.GetBase());
                        replaceBaseExp = new ALvalueExp(pointer);
                        matchesNormalArray = true;
                    }
                }
            }
            if (matchingArrayProperties.Count == 0 && !matchesNormalArray && implicitStructArrayProperties.Count == 0)
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Found no matching properties or arrays"));

            if (matchingArrayProperties.Count + (matchesNormalArray ? 1 : 0) > 1)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (APropertyDecl property in matchingArrayProperties)
                {
                    subErrors.Add(new ErrorCollection.Error(property.GetName(), "Matching property"));
                }
                if (matchesNormalArray)
                    subErrors.Add(new ErrorCollection.Error(node.GetToken(), "Matches normal array index."));
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Found multiple matching array indexes.", false, subErrors.ToArray()));
                throw new ParserException(node.GetToken(), "TypeChecking.OutAArrayLvalue");
            }

            if (matchingArrayProperties.Count + (matchesNormalArray ? 1 : 0) == 0 &&
                implicitStructArrayProperties.Count > 1)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (APropertyDecl property in implicitStructArrayProperties)
                {
                    subErrors.Add(new ErrorCollection.Error(property.GetName(), "Matching property"));
                }
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Found multiple matching array indexes.", false, subErrors.ToArray()));
                throw new ParserException(node.GetToken(), "TypeChecking.OutAArrayLvalue");
            }



            if (matchingArrayProperties.Count == 1)
            {
                APropertyDecl property = matchingArrayProperties[0];
                type = property.GetType();
                data.ArrayPropertyLinks[node] = new Util.Pair<APropertyDecl, bool>(property, false);

                CheckPropertyAccessibility(property, node.Parent() is AAssignmentExp, node.GetToken());
            }
            else if (implicitStructArrayProperties.Count == 1)
            {
                APropertyDecl property = implicitStructArrayProperties[0];
                type = property.GetType();
                data.ArrayPropertyLinks[node] = new Util.Pair<APropertyDecl, bool>(property, true); 

                CheckPropertyAccessibility(property, node.Parent() is AAssignmentExp, node.GetToken());
            }

            if (replaceBaseExp == null)
                data.LvalueTypes[node] = type;
            else
            {
                node.SetBase(replaceBaseExp);
                //pointer.Apply(this);
                OutAPointerLvalue((APointerLvalue)replaceBaseExp.GetLvalue());
                OutALvalueExp(replaceBaseExp);
                OutAArrayLvalue(node);
            }

            //if (!Assignable(data.ExpTypes[node.GetIndex()], new ANamedType(new TIdentifier("int"), null)))
            //    errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Indexes of arrays must be of integer type."));
        }

        public override void InAStructDecl(AStructDecl node)
        {
            StructDepandancies.Add(node, new List<AStructDecl>());
            base.InAStructDecl(node);
        }

        public override void OutANamedType(ANamedType node)
        {
            if (data.StructTypeLinks.ContainsKey(node) && !Util.HasAncestor<APointerType>(node))
            {
                AStructDecl targetStr = data.StructTypeLinks[node];
                AStructDecl str = Util.GetAncestor<AStructDecl>(node);
                AMethodDecl method = Util.GetAncestor<AMethodDecl>(node);
                if (str != null && method == null)
                {
                    if (!StructDepandancies[str].Contains(targetStr))
                        StructDepandancies[str].Add(targetStr);
                }
            }
            base.OutANamedType(node);
        }

        public override void OutAPropertyLvalue(APropertyLvalue node)
        {
            data.LvalueTypes[node] = data.PropertyLinks[node].GetType();
            CheckPropertyAccessibility(data.PropertyLinks[node], node.Parent() is AAssignmentExp, node.GetName());
            base.OutAPropertyLvalue(node);
        }

        private void CheckPropertyAccessibility(APropertyDecl property, bool needSetter, Token token)
        {
            ErrorCollection.Error subError = new ErrorCollection.Error(property.GetName(), Util.GetAncestor<AASourceFile>(property), "Matching property");
            if (!needSetter && property.GetGetter() == null)
                errors.Add(new ErrorCollection.Error(token, Util.GetAncestor<AASourceFile>(token), "Property has no getter", false, subError));
            if (needSetter && property.GetSetter() == null)
                errors.Add(new ErrorCollection.Error(token, Util.GetAncestor<AASourceFile>(token), "Property has no setter", false, subError));
        }

        public override void OutAStructLvalue(AStructLvalue node)
        {
            if (node.Parent() is ADelegateExp)
                return;
            if (node.Parent() is ASyncInvokeExp && ((ASyncInvokeExp)node.Parent()).GetName() == node)
                return;
            if (node.Parent() is AAsyncInvokeStm && ((AAsyncInvokeStm)node.Parent()).GetName() == node)
                return;
            if (data.StructFieldLinks.ContainsKey(node))
            {
                data.LvalueTypes[node] = data.StructFieldLinks[node].GetType();
                return;
            }
            if (data.StructPropertyLinks.ContainsKey(node))
            {
                APropertyDecl property = data.StructPropertyLinks[node];
                data.LvalueTypes[node] = property.GetType();
                CheckPropertyAccessibility(property, node.Parent() is AAssignmentExp, node.GetName());
                return;
            }

            PExp reciever = node.GetReceiver();
            AStructDecl structDecl;
            bool linked;
            List<ErrorCollection.Error> errs;
            
                
            
            //Find the local in the struct that this struct points to
            PType type = data.ExpTypes[reciever];

            if ((type is AArrayTempType || type is ADynamicArrayType) && node.GetName().Text == "length")
            {//Array.length
                if (reciever is ALvalueExp && ((ALvalueExp)reciever).GetLvalue() is APointerLvalue)//Make new APArrayLength
                {
                    APArrayLengthLvalue replacer = new APArrayLengthLvalue(node.GetReceiver());
                    data.LvalueTypes[replacer] = new ANamedType(new TIdentifier("int"), null);
                    node.ReplaceBy(replacer);
                    return;
                }
                else
                {
                    AArrayLengthLvalue replacer = new AArrayLengthLvalue(node.GetReceiver());
                    data.LvalueTypes[replacer] = new ANamedType(new TIdentifier("int"), null);
                    data.ArrayLengthTypes[replacer] = (AArrayTempType) type;
                    node.ReplaceBy(replacer);
                    return;
                }
            }

            List<AEnrichmentDecl> enrichments = new List<AEnrichmentDecl>();
            List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
            foreach (IList declList in visibleDecls)
            {
                foreach (PDecl decl in declList)
                {
                    if (decl is AEnrichmentDecl)
                    {
                        AEnrichmentDecl enrichment = (AEnrichmentDecl) decl;
                        if (Util.TypesEqual(type, enrichment.GetType(), data))
                            enrichments.Add(enrichment);
                    }
                }
            }
            

            if (enrichments.Count == 0 && (!(type is ANamedType) || !data.StructTypeLinks.ContainsKey((ANamedType)type)))
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                     "The left side of the . must be of type struct, class, or something which is enriched"));
                throw new ParserException(null, null);
            }
            
            if (enrichments.Count > 0)
            {
                foreach (AEnrichmentDecl enrichment in enrichments)
                {

                    //Can not enrich a struct type, so it is not a struct type.
                    //Look for property
                    foreach (PDecl decl in enrichment.GetDecl())
                    {
                        if (decl is APropertyDecl)
                        {
                            APropertyDecl aDecl = (APropertyDecl) decl;
                            if (aDecl.GetName().Text == node.GetName().Text)
                            {
                                //Check visibility
                                if (!(aDecl.GetVisibilityModifier() is APublicVisibilityModifier) &&
                                    Util.GetAncestor<AEnrichmentDecl>(node) != enrichment)
                                    continue;

                                data.LvalueTypes[node] = aDecl.GetType();
                                data.StructPropertyLinks[node] = aDecl;
                                CheckPropertyAccessibility(aDecl, node.Parent() is AAssignmentExp, node.GetName());
                                return;
                            }
                        }
                    }
                }
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (var enrichment in enrichments)
                {
                    subErrors.Add(new ErrorCollection.Error(enrichment.GetToken(),
                                                                               Util.GetAncestor<AASourceFile>(enrichment),
                                                                               "Matching enrichment"));
                }
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                     "No matching property found in enrichment.", false, subErrors.ToArray()));
                throw new ParserException(null, null);
            }


            structDecl = data.StructTypeLinks[(ANamedType)type];

            //Look through structDecl to find a local matching
            errs = FindStructMember(structDecl, node, out linked);
            if (errs.Count > 0)
            {
                foreach (ErrorCollection.Error error in errs)
                {
                    errors.Add(error);
                }
                throw new ParserException(null, null);
            }
            

            base.OutAStructLvalue(node);
        }

        private List<ErrorCollection.Error> FindStructMember(AStructDecl structDecl, AStructLvalue node, out bool linked, bool onlyStatic = false)
        {
            linked = false;
            List<ErrorCollection.Error> errors = new List<ErrorCollection.Error>();
            AALocalDecl matchingLocal = data.StructFields[structDecl].FirstOrDefault(
                local =>
                local.GetName().Text == node.GetName().Text);
            APropertyDecl matchingProperty = data.StructProperties[structDecl].FirstOrDefault(
                property => property.GetName().Text == node.GetName().Text);
            if (matchingLocal == null && matchingProperty == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                     "Unable to find a field in the struct called " + node.GetName().Text));
                errors.Add(new ErrorCollection.Error(structDecl.GetName(), Util.GetAncestor<AASourceFile>(structDecl),
                                                     "Matching struct: " + structDecl.GetName().Text));
                return errors;
            }
            bool isStatic = false;
            if (matchingProperty == null)
            {
                isStatic = matchingLocal.GetStatic() != null;
                //Check visibility
                AALocalDecl originalLocal = matchingLocal;
                if (data.EnheritanceLocalMap.ContainsKey(originalLocal))
                    originalLocal = data.EnheritanceLocalMap[originalLocal];

                if (originalLocal.GetVisibilityModifier() is APrivateVisibilityModifier &&
                    Util.GetAncestor<AStructDecl>(originalLocal) != Util.GetAncestor<AStructDecl>(node))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "The struct variable is marked as private. It can only be accessed from inside the struct where it is defined.",
                                                         false,
                                                         new ErrorCollection.Error(originalLocal.GetName(),
                                                                                   "Matching local")));
                }
                else if (originalLocal.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                         (!Util.HasAncestor<AStructDecl>(node) ||
                          !Util.Extends(Util.GetAncestor<AStructDecl>(originalLocal), Util.GetAncestor<AStructDecl>(node),
                                       data)))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "The struct variable is marked as protected. It can only be accessed from inside the struct where it is defined, or a subtype thereof.",
                                                         false,
                                                         new ErrorCollection.Error(originalLocal.GetName(),
                                                                                   "Matching local")));
                }

                //Not the visiblity static thingy. The regular static
                if (matchingLocal.GetStatic() != null && !onlyStatic)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Variable is marked as static. Access it with " +
                                                         structDecl.GetName().Text + "." + node.GetName().Text, false,
                                                         new ErrorCollection.Error(matchingLocal.GetName(),
                                                                                   "Matching struct field")));
                }
                else if (matchingLocal.GetStatic() == null && onlyStatic)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Variable is not marked as static. Access it with you must access it through an instance of " +
                                                         structDecl.GetName().Text + ".", false,
                                                         new ErrorCollection.Error(matchingLocal.GetName(),
                                                                                   "Matching struct field")));
                }

                data.LvalueTypes[node] = matchingLocal.GetType();
                data.StructFieldLinks[node] = matchingLocal;
                linked = true;
            }
            else
            {
                //Check visibility
                if (matchingProperty.GetVisibilityModifier() is APrivateVisibilityModifier &&
                    Util.GetAncestor<AStructDecl>(matchingProperty) != Util.GetAncestor<AStructDecl>(node))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "The struct property is marked as private. It can only be accessed from inside the struct where it is defined.",
                                                         false,
                                                         new ErrorCollection.Error(matchingProperty.GetName(),
                                                                                   "Matching local")));
                }
                else if (matchingProperty.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                         (!Util.HasAncestor<AStructDecl>(node) ||
                          !Util.Extends(Util.GetAncestor<AStructDecl>(node), Util.GetAncestor<AStructDecl>(matchingProperty),
                                       data)))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "The struct property is marked as protected. It can only be accessed from inside the struct where it is defined, or a subtype thereof.",
                                                         false,
                                                         new ErrorCollection.Error(matchingProperty.GetName(),
                                                                                   "Matching local")));
                }

                //Not the visiblity static thingy. The regular static
                if (matchingProperty.GetStatic() != null && !onlyStatic)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Property is marked as static. Access it with " +
                                                         structDecl.GetName().Text + "." + node.GetName().Text, false,
                                                         new ErrorCollection.Error(matchingProperty.GetName(),
                                                                                   "Matching struct property")));
                }
                else if (matchingProperty.GetStatic() == null && onlyStatic)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Property is not marked as static. Access it with you must access it through an instance of " +
                                                         structDecl.GetName().Text + ".", false,
                                                         new ErrorCollection.Error(matchingProperty.GetName(),
                                                                                   "Matching struct property")));
                }

                data.LvalueTypes[node] = matchingProperty.GetType();
                data.StructPropertyLinks[node] = matchingProperty;
                CheckPropertyAccessibility(matchingProperty, node.Parent() is AAssignmentExp, node.GetName());
                linked = true;
            }

            if (!isStatic && Util.GetAncestor<AMethodDecl>(node) == null && Util.GetAncestor<AConstructorDecl>(node) == null &&
                !Util.HasAncestor<ADeconstructorDecl>(node) && !Util.HasAncestor<APropertyDecl>(node))
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                    "Unable to use struct fields outside of methods."));
            }
            return errors;
        }

        public override void OutADelegateExp(ADelegateExp node)
        {
            //Find the type of delegate
            ANamedType type = (ANamedType) node.GetType();
            if (!data.DelegateTypeLinks.ContainsKey(type))
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "The type in a delegate creation expression must be a delegate type"));
                throw new ParserException(node.GetToken(), "TypeChecking.OutADelegateExp");
            }
            AMethodDecl delegateDef = data.DelegateTypeLinks[type];
            List<PType> argTypes = new List<PType>();
            foreach (AALocalDecl formal in delegateDef.GetFormals())
            {
                argTypes.Add(formal.GetType());
            }

            APointerLvalue reciever = null;
            string methodName;
            Token token;
            Node targetReciever;
            if (node.GetLvalue() is AAmbiguousNameLvalue)
            {
                AAmbiguousNameLvalue ambigious = (AAmbiguousNameLvalue)node.GetLvalue();
                AAName aName = (AAName)ambigious.GetAmbiguous();
                methodName = aName.AsString();
                token = (TIdentifier)aName.GetIdentifier()[aName.GetIdentifier().Count - 1];
                aName.GetIdentifier().RemoveAt(aName.GetIdentifier().Count - 1);
                targetReciever = aName.GetIdentifier().Count > 0 ? aName : null;
            }
            else//node.getLvalue is AStructLvalue
            {
                AStructLvalue lvalue = (AStructLvalue) node.GetLvalue();
                token = lvalue.GetName();
                methodName = token.Text;
                targetReciever = lvalue.GetReceiver();
            }
            
            List<AMethodDecl> candidates = new List<AMethodDecl>();
            List<AMethodDecl> implicitCandidates = new List<AMethodDecl>();
            List<AMethodDecl> matchingNames = new List<AMethodDecl>();
            PExp baseExp;
            bool matchResize;
            GetTargets(token.Text, node.GetToken(), targetReciever, delegateDef.GetReturnType(), argTypes, candidates, out matchResize, implicitCandidates, matchingNames, out baseExp, null, data, errors);

            
            
            if (candidates.Count == 0 && implicitCandidates.Count > 0)
            {
                //Implicit candidates not allowed
                errors.Add(new ErrorCollection.Error(token, "Method must match exactly. Implicitly matching methods are not allowed.",
                                                     false,
                                                     new ErrorCollection.Error(implicitCandidates[0].GetName(),
                                                                               "Matching method")));
                throw new ParserException(token, "OutADelegateExp");
            }

            if (baseExp is ALvalueExp)
            {
                ALvalueExp exp = (ALvalueExp)baseExp;
                if (exp.GetLvalue() is APointerLvalue)
                {
                    reciever = (APointerLvalue) exp.GetLvalue();
                    node.SetLvalue(reciever);
                    reciever.Apply(this);
                }
                else
                {
                    errors.Add(new ErrorCollection.Error(token, "Method must be refferenced from a dynamic context.",
                                                         false,
                                                         new ErrorCollection.Error(candidates[0].GetName(),
                                                                                   "Matching method")));
                    throw new ParserException(token, "OutADelegateExp");
                }
            }
            else if (baseExp == null)
            {
                //Target is either a global method, a static struct method, or a struct method -> struct method
                //Last one is invalid.
                AStructDecl parentStruct = Util.GetAncestor<AStructDecl>(candidates[0]);
                if (parentStruct != null && candidates[0].GetStatic() == null && 
                    Util.GetAncestor<AStructDecl>(node).GetClassToken() == null && 
                    Util.GetAncestor<AConstructorDecl>(node) == null &&
                    Util.GetAncestor<ADeconstructorDecl>(node) == null)
                {
                    errors.Add(new ErrorCollection.Error(token, "Method must be refferenced from a dynamic context.",
                                                         false,
                                                         new ErrorCollection.Error(candidates[0].GetName(),
                                                                                   "Matching method")));
                    throw new ParserException(token, "OutADelegateExp");
                }
            }
            else
            {
                errors.Add(new ErrorCollection.Error(token, "Method must be a global method, or static struct/class method, or it must be refferenced from a dynamic context.",
                                                     false,
                                                     new ErrorCollection.Error(candidates[0].GetName(),
                                                                               "Matching method")));
                throw new ParserException(token, "OutADelegateExp");  
            }

            //Found exactly 1 delegate method
            data.DelegateCreationMethod[node] = candidates[0];
            data.ExpTypes[node] = node.GetType();
            data.DelegateRecieveres[node] = reciever;
        }


        public override void OutALocalLvalue(ALocalLvalue node)
        {
            AALocalDecl decl = data.LocalLinks[node];
            PType type = data.LvalueTypes[node] = decl.GetType();

            if (decl.GetOut() != null && !Util.IsBulkCopy(type) && !assignedToOutParams.Contains(decl) && !(node.Parent() is AAssignmentExp && node == ((AAssignmentExp)node.Parent()).GetLvalue()))
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Parameter " + node.GetName().Text + " marked as out is used before a value has been assigned to it."));
            }
        }


        public override void OutAFieldLvalue(AFieldLvalue node)
        {
            data.LvalueTypes[node] = data.FieldLinks[node].GetType();
        }

        public override void OutAStructFieldLvalue(AStructFieldLvalue node)
        {
            if (data.StructMethodFieldLinks.ContainsKey(node))
            {
                AALocalDecl decl = data.StructMethodFieldLinks[node];
                if (data.Enums.ContainsKey((AStructDecl) decl.Parent()))
                {
                    AStructDecl str = (AStructDecl) decl.Parent();
                    ANamedType namedType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                    data.StructTypeLinks[namedType] = str;
                    data.LvalueTypes[node] = namedType;
                }
                else
                    data.LvalueTypes[node] = decl.GetType();
                if (Util.IsStaticContext(node) && decl.GetStatic() == null)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "You can not access a non-static variable in a static context.",
                                                         false,
                                                         new ErrorCollection.Error(decl.GetName(),
                                                                                   "Matching declaration")));
                }
            }
            else
            {
                APropertyDecl decl = data.StructMethodPropertyLinks[node];
                data.LvalueTypes[node] = decl.GetType();
                CheckPropertyAccessibility(decl, node.Parent() is AAssignmentExp, node.GetName());
                if (Util.IsStaticContext(node) && decl.GetStatic() == null)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "You can not access a non-static property in a static context.",
                                                         false,
                                                         new ErrorCollection.Error(decl.GetName(),
                                                                                   "Matching property")));
                }
            }

            //If the lvalue is a static variable, it's okay
            
            if (data.StructMethodFieldLinks[node].GetStatic() == null && Util.GetAncestor<AMethodDecl>(node) == null && Util.GetAncestor<APropertyDecl>(node) == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                    "Unable to use struct fields outside of methods."));
            }
        }

        private bool IsDynamic(PType type)
        {
            return type is APointerType || type is ADynamicArrayType;
        }

        public override void OutABinopExp(ABinopExp node)
        {
            PBinop binop = node.GetBinop();
            PExp left = node.GetLeft();
            PType leftType = data.ExpTypes[left];
            string leftTypeString = Util.TypeToString(leftType);
            PExp right = node.GetRight();
            PType rightType = data.ExpTypes[right];
            string rightTypeString = Util.TypeToString(rightType);


            bool wasDefined = false;
            Token token = null;
            while (true)
            {
                if (binop is APlusBinop)
                {
                    token = ((APlusBinop) binop).GetToken();
                    //Check that types are okay for +
                    if (!new[] {"int", "fixed", "string", "text", "byte", "point"}.Any(c => c == leftTypeString))
                    {
                        errors.Add(new ErrorCollection.Error(token, currentSourceFile,
                                                             "+ is not defined for type " + leftTypeString));
                        throw new ParserException(null, null);
                    }
                    if (!new[] {"int", "fixed", "string", "text", "byte", "point"}.Any(c => c == rightTypeString))
                    {
                        errors.Add(new ErrorCollection.Error(token, currentSourceFile,
                                                             "+ is not defined for type " + rightTypeString));
                        throw new ParserException(null, null);
                    }
                    //If you are using string or text, both sides must be same type
                    if ((leftTypeString == "string" && rightTypeString != "string") ||
                        (leftTypeString == "text" && rightTypeString != "text") ||
                        (leftTypeString == "point" && rightTypeString != "point") ||
                        (rightTypeString == "string" && leftTypeString != "string") ||
                        (rightTypeString == "text" && leftTypeString != "text") ||
                        (rightTypeString == "point" && leftTypeString != "point"))
                    {
                        if (ImplicitAssignable(leftType, rightType))
                        {
                            ANamedType namedTo = (ANamedType) rightType;
                            ACastExp cast = new ACastExp(new TLParen("("),
                                                         new ANamedType(
                                                             new TIdentifier(((AAName) namedTo.GetName()).AsString()),
                                                             null), node.GetLeft());
                            node.SetLeft(cast);
                            OutACastExp(cast);
                            leftType = rightType;
                        }
                        else if (ImplicitAssignable(rightType, leftType))
                        {
                            ANamedType namedTo = (ANamedType) leftType;
                            ACastExp cast = new ACastExp(new TLParen("("),
                                                         new ANamedType(
                                                             new TIdentifier(((AAName) namedTo.GetName()).AsString()),
                                                             null), node.GetRight());
                            node.SetRight(cast);
                            OutACastExp(cast);
                            rightType = leftType;
                        }
                        else
                        {
                            //Not valid
                            break;
                        }
                    }
                    wasDefined = true;
                    PType type = leftType;
                    if (rightTypeString == "fixed")
                        type = rightType;
                    data.ExpTypes[node] = type;
                }
                else if (binop is AMinusBinop || binop is ATimesBinop || binop is ADivideBinop || binop is AModuloBinop)
                {
                    token = null;
                    if (binop is AMinusBinop) token = ((AMinusBinop) binop).GetToken();
                    else if (binop is ATimesBinop) token = ((ATimesBinop) binop).GetToken();
                    else if (binop is ADivideBinop) token = ((ADivideBinop) binop).GetToken();
                    else if (binop is AModuloBinop) token = ((AModuloBinop) binop).GetToken();

                    //Check that types are okay for whatever
                    if (!new[] {"int", "fixed", "byte", "point"}.Any(c => c == leftTypeString))
                    {
                        //Not valid
                        break;
                    }
                    if (!new[] {"int", "fixed", "byte", "point"}.Any(c => c == rightTypeString))
                    {
                        //Not valid
                        break;
                    }
                    if ((leftTypeString == "point" || rightTypeString == "point") &&
                        !(leftTypeString == "point" && rightTypeString == "point" && binop is AMinusBinop))
                    {
                        //Not valid
                        break;
                    }
                    wasDefined = true;
                    PType type = leftType;
                    if (rightTypeString == "fixed")
                        type = rightType;
                    if (rightTypeString == "int" && leftTypeString == "byte")
                        type = rightType;
                    data.ExpTypes[node] = type;
                }
                else if (binop is AEqBinop || binop is ANeBinop || binop is ALtBinop || binop is ALeBinop ||
                         binop is AGtBinop || binop is AGeBinop)
                {
                    token = null;
                    if (binop is AEqBinop) token = ((AEqBinop) binop).GetToken();
                    else if (binop is ANeBinop) token = ((ANeBinop) binop).GetToken();
                    else if (binop is ALtBinop) token = ((ALtBinop) binop).GetToken();
                    else if (binop is ALeBinop) token = ((ALeBinop) binop).GetToken();
                    else if (binop is AGtBinop) token = ((AGtBinop) binop).GetToken();
                    else if (binop is AGeBinop) token = ((AGeBinop) binop).GetToken();

                    //Unless types are int and fixed, they must be the same type, or null and a nullable type
                    if (leftTypeString == "void" || rightTypeString == "void" ||
                        !(
                             GalaxyKeywords.NullablePrimitives.words.Any(s => s == leftTypeString) &&
                             rightTypeString == "null" ||
                             leftTypeString == "null" &&
                             GalaxyKeywords.NullablePrimitives.words.Any(s => s == rightTypeString) ||
                             (leftTypeString == "int" || leftTypeString == "fixed" || leftTypeString == "byte") &&
                             (rightTypeString == "int" || rightTypeString == "fixed" || rightTypeString == "byte") ||
                             leftTypeString == rightTypeString && !(IsDynamic(leftType) || IsDynamic(rightType)) ||
                             (binop is AEqBinop || binop is ANeBinop) &&
                             (
                                 leftTypeString == rightTypeString ||
                                 leftTypeString == "null" && IsDynamic(rightType) ||
                                 IsDynamic(leftType) && rightTypeString == "null" ||
                                 Util.TypesEqual(leftType, rightType, data)
                             ) ||
                             leftType is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType) leftType) &&
                             (rightTypeString == "null" ||
                              rightType is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType) rightType)) ||
                             rightType is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType) rightType) &&
                             leftTypeString == "null"
                         )
                        )
                    {

                        //Not valid
                        break;

                    }
                    wasDefined = true;
                    data.ExpTypes[node] = new ANamedType(new TIdentifier("bool"), null);
                }
                else if (binop is AAndBinop || binop is AOrBinop || binop is AXorBinop || binop is ALBitShiftBinop ||
                         binop is ARBitShiftBinop)
                {
                    token = null;
                    if (binop is AAndBinop) token = ((AAndBinop) binop).GetToken();
                    else if (binop is AOrBinop) token = ((AOrBinop) binop).GetToken();
                    else if (binop is AXorBinop) token = ((AXorBinop) binop).GetToken();
                    else if (binop is ALBitShiftBinop) token = ((ALBitShiftBinop) binop).GetToken();
                    else if (binop is ARBitShiftBinop) token = ((ARBitShiftBinop) binop).GetToken();

                    if (
                        !((leftTypeString == "int" || leftTypeString == "byte") &&
                          (rightTypeString == "int" || rightTypeString == "byte") &&
                          (binop is ALBitShiftBinop || binop is ARBitShiftBinop ||
                           leftTypeString == rightTypeString)))
                    {
                        if (rightTypeString == "int" && leftTypeString == "byte" && left is AIntConstExp)
                        {
                            data.ExpTypes[left] =
                                leftType = new ANamedType(new TIdentifier("int"), null);
                            leftTypeString = "int";
                        }
                        else if (leftTypeString == "int" && rightTypeString == "byte" && right is AIntConstExp)
                        {
                            data.ExpTypes[right] =
                                rightType = new ANamedType(new TIdentifier("int"), null);
                            rightTypeString = "int";
                        }
                        else
                        {
                            //Not valid
                            break;
                        }
                    }
                    wasDefined = true;
                    data.ExpTypes[node] = leftType;
                    if (rightTypeString == "int")
                        data.ExpTypes[node] = rightType;
                }
                else if (binop is ALazyAndBinop || binop is ALazyOrBinop)
                {
                    token = null;
                    if (binop is ALazyAndBinop) token = ((ALazyAndBinop) binop).GetToken();
                    else if (binop is ALazyOrBinop) token = ((ALazyOrBinop) binop).GetToken();

                    if (leftTypeString != "bool" || rightTypeString != "bool")
                    {

                        errors.Add(new ErrorCollection.Error(token, currentSourceFile,
                                                             token.Text + " is only defined for (bool " +
                                                             token.Text +
                                                             " bool). Got (" +
                                                             leftTypeString + " " + token.Text + " " +
                                                             rightTypeString + ")"));
                        throw new ParserException(null, null);
                    }
                    wasDefined = true;
                    data.ExpTypes[node] = leftType;
                }
                else
                    throw new Exception("Unexpected binop (This should never happen)");
                break;
            }


            List<AMethodDecl> possibleOperators = new List<AMethodDecl>();
            List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
            List<string> currentNamespace = Util.GetFullNamespace(node);
            AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
            foreach (IList declList in visibleDecls)
            {
                bool sameNS = false;
                bool sameFile = false;
                if (declList.Count > 0)
                {
                    sameNS = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace((PDecl) declList[0]));
                    sameFile = currentFile == Util.GetAncestor<AASourceFile>((PDecl) declList[0]);
                }
                foreach (PDecl decl in declList)
                {
                    if (decl is AMethodDecl)
                    {
                        AMethodDecl method = (AMethodDecl) decl;
                        if (method.GetName().Text == token.Text)
                        {
                            if (method.GetVisibilityModifier() is APrivateVisibilityModifier && !sameNS)
                                continue;
                            if (method.GetStatic() != null && !sameFile)
                                continue;
                            //Check that parameters are assignable
                            bool add = true;
                            bool matchImplicit = false;
                            List<PType> argTypes = new List<PType>(){leftType, rightType};
                            for (int i = 0; i < argTypes.Count; i++)
                            {
                                PType argType = argTypes[i];
                                AALocalDecl formal = (AALocalDecl)method.GetFormals()[i];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    ||
                                    formal.GetRef() != null &&
                                    !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    ||
                                    formal.GetOut() == null && formal.GetRef() == null &&
                                    !Assignable(argType, formalType))
                                {
                                    add = false;
                                    if (formal.GetOut() == null && formal.GetRef() == null &&
                                        ImplicitAssignable(argType, formalType))
                                    {
                                        matchImplicit = true;
                                    }
                                    else
                                    {
                                        matchImplicit = false;
                                        break;
                                    }
                                }
                            }
                            if (!add && !matchImplicit)
                                continue;
                            if (add)
                                possibleOperators.Add(method);
                        }
                    }
                }
            }


            if (possibleOperators.Count == 0 && !wasDefined)
            {
                errors.Add(new ErrorCollection.Error(token, "Found no definitions of (" + leftTypeString + " " + token.Text + " " + rightTypeString + ")"));
                throw new ParserException(token, "TypeChecking.OutABinopExp");
            }

            if (possibleOperators.Count + (wasDefined ? 1 : 0) > 1)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (AMethodDecl method in possibleOperators)
                {
                    subErrors.Add(new ErrorCollection.Error(method.GetName(), "Matching operator"));
                }
                if (wasDefined)
                    subErrors.Add(new ErrorCollection.Error(token, "Matched default " + token.Text));
                errors.Add(new ErrorCollection.Error(token, "Multiple definitions for (" + leftTypeString + " " + token.Text + " " + rightTypeString + ") found.", false, subErrors.ToArray()));
                throw new ParserException(token, "TypeChecking.OutABinopExp");
            }

            if (wasDefined)
                return;

            AMethodDecl op = possibleOperators[0];
            ASimpleInvokeExp replacer = new ASimpleInvokeExp(new TIdentifier(op.GetName().Text), new ArrayList(){node.GetLeft(), node.GetRight()});
            node.ReplaceBy(replacer);
            data.SimpleMethodLinks[replacer] = op;
            data.ExpTypes[replacer] = op.GetReturnType();


            //base.OutABinopExp(node);
        }


        public override void OutAUnopExp(AUnopExp node)
        {
            PUnop unop = node.GetUnop();
            PExp exp = node.GetExp();
            PType expType = data.ExpTypes[exp];
            string expTypeString = Util.TypeToString(expType);

            //! okay for int and bool. - okay for int and fixed
            if (unop is ANegateUnop)
            {
                Token token = ((ANegateUnop)unop).GetToken();
                if (expTypeString != "int" && expTypeString != "fixed" && expTypeString != "byte")
                {
                    errors.Add(new ErrorCollection.Error(token, currentSourceFile,
                                                         "- is not defined for type " + expTypeString));
                    throw new ParserException(null, null);
                }
                data.ExpTypes[node] = expType;
            }
            else if (unop is AComplementUnop)
            {
                Token token = ((AComplementUnop)unop).GetToken();
                if (expTypeString != "int" && expTypeString != "byte" && expTypeString != "bool" && expTypeString != "point" && expTypeString != "order" && expTypeString != "string")
                {
                    errors.Add(new ErrorCollection.Error(token, currentSourceFile,
                                                         "! is not defined for type " + expTypeString));
                    throw new ParserException(null, null);
                }
                data.ExpTypes[node] = expType;
            }
            else
                throw new Exception("Unexpected unop (This should never happen)");

            base.OutAUnopExp(node);
        }

        public override void OutAIntConstExp(AIntConstExp node)
        {
            int i = int.Parse(node.GetIntegerLiteral().Text);
            data.ExpTypes[node] = new ANamedType(new TIdentifier(i < 256 && i >= 0 ? "byte" : "int"), null);
            base.OutAIntConstExp(node);
        }

        public override void OutAFixedConstExp(AFixedConstExp node)
        {
            data.ExpTypes[node] = new ANamedType(new TIdentifier("fixed"), null);
            base.OutAFixedConstExp(node);
        }

        public override void OutAStringConstExp(AStringConstExp node)
        {
            data.ExpTypes[node] = new ANamedType(new TIdentifier("string"), null);
            base.OutAStringConstExp(node);
        }

        public override void OutACharConstExp(ACharConstExp node)
        {
            data.ExpTypes[node] = new ANamedType(new TIdentifier("char"), null);
            base.OutACharConstExp(node);
        }

        public override void OutABooleanConstExp(ABooleanConstExp node)
        {
            data.ExpTypes[node] = new ANamedType(new TIdentifier("bool"), null);
            base.OutABooleanConstExp(node);
        }

        public override void OutANullExp(ANullExp node)
        {
            data.ExpTypes[node] = new ANamedType(new TIdentifier("null"), null);
            base.OutANullExp(node);
        }

        public override void InAMethodDecl(AMethodDecl node)
        {
            InlineMethodCalls.Add(node, new List<AMethodDecl>());
            assignedToOutParams.Clear();
        }


        public override void CaseAFieldDecl(AFieldDecl node)
        {
            //Check that it is const
            if (node.GetConst() != null && !data.ObfuscationFields.Contains(node))
            {
                if (node.GetInit() == null)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), "Constant fields must be initialized."));
                    return;
                }
                ConstChecker checker = new ConstChecker(data);
                node.GetInit().Apply(checker);
                if (!checker.IsConst)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), "Constant fields' initializer must be constant."));
                    return;
                }
            }



            base.CaseAFieldDecl(node);
        }

        private class ConstChecker : DepthFirstAdapter
        {
            private SharedData data;
            public bool IsConst = true;

            public ConstChecker(SharedData data)
            {
                this.data = data;
            }

            public override void CaseAIncDecExp(AIncDecExp node)
            {
                IsConst = false;
            }

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                IsConst = false;
            }

            public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
            {
                IsConst = false;
            }

            public override void CaseASyncInvokeExp(ASyncInvokeExp node)
            {
                IsConst = false;
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                IsConst = false;
            }

            public override void CaseACastExp(ACastExp node)
            {
                IsConst = false;
            }

            public override void CaseASharpCastExp(ASharpCastExp node)
            {
                IsConst = false;
            }

            public override void CaseANewExp(ANewExp node)
            {
                IsConst = false;
            }

            public override void CaseADelegateExp(ADelegateExp node)
            {
                IsConst = false;
            }

            public override void CaseADelegateInvokeExp(ADelegateInvokeExp node)
            {
                IsConst = false;
            }

            public override void CaseAIfExp(AIfExp node)
            {
                IsConst = false;
            }

            public override void CaseAArrayResizeExp(AArrayResizeExp node)
            {
                IsConst = false;
            }

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                if (!IsConst) return;
                AALocalDecl decl = data.LocalLinks[node];
                if (decl.GetConst() == null)
                {
                    IsConst = false;
                }
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                if (!IsConst) return;
                AFieldDecl decl = data.FieldLinks[node];
                if (decl.GetConst() == null)
                {
                    IsConst = false;
                }
            }

            public override void CaseAStructFieldLvalue(AStructFieldLvalue node)
            {
                if (!IsConst) return;
                AALocalDecl decl = data.StructMethodFieldLinks[node];
                if (decl.GetConst() == null)
                {
                    IsConst = false;
                }
            }

            public override void CaseAStructLvalue(AStructLvalue node)
            {
                if (!IsConst) return;
                AALocalDecl decl = data.StructFieldLinks[node];
                if (decl.GetConst() == null)
                {
                    IsConst = false;
                }
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                IsConst = false;
            }

            public override void CaseAPointerLvalue(APointerLvalue node)
            {
                IsConst = false;
            }

            public override void CaseAThisLvalue(AThisLvalue node)
            {
                IsConst = false;
            }

            
        }


        public override void OutAIncDecExp(AIncDecExp node)
        {
            PIncDecOp op = node.GetIncDecOp();
            if (!Util.HasAncestor<AABlock>(node))
            {
                Token token = null;
                if (op is APostDecIncDecOp)
                    token = ((APostDecIncDecOp) op).GetToken();
                else if (op is APreDecIncDecOp)
                    token = ((APreDecIncDecOp)op).GetToken();
                else if (op is APostIncIncDecOp)
                    token = ((APostIncIncDecOp)op).GetToken();
                else if (op is APreIncIncDecOp)
                    token = ((APreIncIncDecOp)op).GetToken();
                errors.Add(new ErrorCollection.Error(token, "++ and -- expressions can only reside inside methods."));
                throw new ParserException(token, "TypeChecking.OutAIncDecExp");
            }


            bool plus = op is APreIncIncDecOp || op is APostIncIncDecOp;
            if (op is APreIncIncDecOp || op is APreDecIncDecOp || node.Parent() is AExpStm)
            {//++i, --i, i++; or i--;
                //Replace with assignment
                //<exp>++ => <exp> + 1
                //(... foo = <exp> ...)++ => (... foo = <exp> ...) = (... foo ...) + 1
                //(... foo++ ...)++ => (... foo++ ...) = (... foo ...) + 1
            
                PLvalue clone = Util.MakeClone(node.GetLvalue(), data);
                clone.Apply(new AssignFixup(data));
                PBinop binop;
                if (plus)
                {
                    binop = new APlusBinop(new TPlus("+"));
                }
                else
                {
                    binop = new AMinusBinop(new TMinus("-"));
                }
                ABinopExp addExp = new ABinopExp(new ALvalueExp(clone), binop, new AIntConstExp(new TIntegerLiteral("1")));
                AAssignmentExp exp = new AAssignmentExp(new TAssign("="), node.GetLvalue(), addExp);
                node.ReplaceBy(exp);
                exp.Apply(this);
                return;
            }
            {//i++ or i--
                //Make a new local so
                //int newLocal = i;
                //++i;
                //...newLocal...;
                PLvalue lvalueClone = Util.MakeClone(node.GetLvalue(), data);
                PExp exp = new ALvalueExp(Util.MakeClone(node.GetLvalue(), data));
                data.ExpTypes[exp] = data.LvalueTypes[node.GetLvalue()];
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(data.LvalueTypes[node.GetLvalue()], data), new TIdentifier("incDecVar"), exp);
                ALocalDeclStm localDeclStm = new ALocalDeclStm(new TSemicolon(";"), localDecl);

                node.SetIncDecOp(plus
                                     ? (PIncDecOp) new APreIncIncDecOp(new TPlusPlus("++"))
                                     : new APreDecIncDecOp(new TMinusMinus("--")));

                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(localDecl.GetName().Text));
                exp = new ALvalueExp(lvalue);
                data.ExpTypes[exp] = data.LvalueTypes[lvalue] = data.LvalueTypes[node.GetLvalue()];
                data.LocalLinks[lvalue] = localDecl;

                PStm pStm = Util.GetAncestor<PStm>(node);
                node.ReplaceBy(exp);
                PStm nodeStm = new AExpStm(new TSemicolon(";"), node);

                AABlock block = (AABlock) pStm.Parent();

                block.GetStatements().Insert(block.GetStatements().IndexOf(pStm), localDeclStm);
                block.GetStatements().Insert(block.GetStatements().IndexOf(pStm), nodeStm);
                localDeclStm.Apply(this);
                nodeStm.Apply(this);
                exp.Apply(this);

                if (pStm is AWhileStm && Util.IsAncestor(exp, ((AWhileStm)pStm).GetCondition()))
                {
                    AWhileStm aStm = (AWhileStm)pStm;
                    //Insert
                    // newLocal = i
                    // ++i
                    //Before each continue in the while, and at the end.

                    //Add continue statement, if not present
                    block = (AABlock)((ABlockStm)aStm.GetBody()).GetBlock();
                    if (block.GetStatements().Count == 0 || !(block.GetStatements()[block.GetStatements().Count - 1] is AContinueStm))
                        block.GetStatements().Add(new AContinueStm(new TContinue("continue")));

                    //Get all continue statements in the while
                    ContinueFinder finder = new ContinueFinder();
                    block.Apply(finder);
                    foreach (AContinueStm continueStm in finder.Continues)
                    {
                        PLvalue nodeLvalue1 = Util.MakeClone(lvalueClone, data);
                        PExp nodeLvalue1Exp = new ALvalueExp(nodeLvalue1);
                        PLvalue nodeLvalue2 = Util.MakeClone(lvalueClone, data);
                        ALocalLvalue newLocalLvalue = new ALocalLvalue(new TIdentifier("newLocal"));
                        data.LocalLinks[newLocalLvalue] = localDecl;
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), newLocalLvalue, nodeLvalue1Exp);
                        PStm assignmentStm = new AExpStm(new TSemicolon(";"), assignment);

                        AIncDecExp newIncDecExp = new AIncDecExp(nodeLvalue2, plus
                                                                                  ? (PIncDecOp)
                                                                                    new APreIncIncDecOp(
                                                                                        new TPlusPlus("++"))
                                                                                  : new APreDecIncDecOp(
                                                                                        new TMinusMinus("--")));
                        PStm newIncDecExpStm = new AExpStm(new TSemicolon(";"), newIncDecExp);


                        block = (AABlock)continueStm.Parent();
                        block.GetStatements().Insert(block.GetStatements().IndexOf(continueStm), assignmentStm);
                        block.GetStatements().Insert(block.GetStatements().IndexOf(continueStm), newIncDecExpStm);
                        
                        assignment.Apply(this);
                        newIncDecExp.Apply(this);
                    }
                }
                return;
            }
        }

        private class ContinueFinder : DepthFirstAdapter
        {
            public List<AContinueStm> Continues = new List<AContinueStm>();

            public override void CaseAContinueStm(AContinueStm node)
            {
                Continues.Add(node);
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                //Don't enter other whiles
            }
        }

        private class AssignFixup : DepthFirstAdapter
        {
            private SharedData data;

            public AssignFixup(SharedData data)
            {
                this.data = data;
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                ALvalueExp replacer = new ALvalueExp(node.GetLvalue());
                data.ExpTypes[replacer] = data.LvalueTypes[replacer.GetLvalue()];
                node.ReplaceBy(replacer);
                replacer.Apply(this);
            }
        }

        public override void OutASharpCastExp(ASharpCastExp node)
        {
            PType fromType = data.ExpTypes[node.GetExp()];
            PType toType = node.GetType();
            //Valid from pointer to int/string and from int/string to pointer
            if (fromType is APointerType)
            {
                APointerType aFromType = (APointerType) fromType;
                
                if (toType != null && !(toType is ANamedType))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Pointers must be cast to either int or string."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                ANamedType aToType = toType == null ? null : (ANamedType) toType;
                if (toType != null && !aToType.IsPrimitive("int") && !aToType.IsPrimitive("string"))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Pointers must be cast to either int or string."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                if (aFromType.GetType() is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) aFromType.GetType()))
                {
                    AStructDecl str = data.StructTypeLinks[(ANamedType) aFromType.GetType()];
                    if (toType != null && (str.GetDimention() == null) != (aToType.IsPrimitive("string")))
                    {
                        errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                             "Expected cast to " +
                                                             (str.GetDimention() == null ? "string." : "int.")));
                        throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                    }
                    data.ExpTypes[node.GetExp()] = new ANamedType(new TIdentifier(str.GetDimention() == null ? "string" : "int"), null);
                    node.ReplaceBy(node.GetExp());
                    return;
                }
                //Look for enrichments
                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                foreach (IList declList in visibleDecls)
                {
                    foreach (PDecl decl in declList)
                    {
                        if (decl is AEnrichmentDecl)
                        {
                            AEnrichmentDecl enrichment = (AEnrichmentDecl)decl;
                            if (!Util.TypesEqual(aFromType.GetType(), enrichment.GetType(), data))
                                continue;
                            if (toType != null && (enrichment.GetDimention() == null) != (aToType.IsPrimitive("string")))
                            {
                                errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                     "Expected cast to " +
                                                                     (enrichment.GetDimention() == null
                                                                          ? "string."
                                                                          : "int.")));
                                throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                            }
                            data.ExpTypes[node.GetExp()] = new ANamedType(new TIdentifier(enrichment.GetDimention() == null ? "string" : "int"), null);
                            node.ReplaceBy(node.GetExp());
                            return;
                        }
                    }
                }
                
                if (toType != null && aToType.IsPrimitive("int"))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                         "Expected cast to string."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                    
                data.ExpTypes[node.GetExp()] = new ANamedType(new TIdentifier("string"), null);
                node.ReplaceBy(node.GetExp());
                return;
            }
            if (fromType is ANamedType)
            {
                ANamedType aFromType = (ANamedType) fromType;
                if (!(toType is APointerType))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Sharp casts can only be used from pointers to int/string and vice versa."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                APointerType aToType = (APointerType) toType;
                if (!aFromType.IsPrimitive("int") && !aFromType.IsPrimitive("string"))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Pointers must be cast from either int or string."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                if (aToType.GetType() is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)aToType.GetType()))
                {
                    AStructDecl str = data.StructTypeLinks[(ANamedType)aToType.GetType()];
                    if ((str.GetDimention() == null) != (aFromType.IsPrimitive("string")))
                    {
                        errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                             "Expected cast from " +
                                                             (str.GetDimention() == null ? "string." : "int.")));
                        throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                    }
                    data.ExpTypes[node.GetExp()] = aToType;
                    node.ReplaceBy(node.GetExp());
                    return;
                }
                //Look for enrichments
                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                foreach (IList declList in visibleDecls)
                {
                    foreach (PDecl decl in declList)
                    {
                        if (decl is AEnrichmentDecl)
                        {
                            AEnrichmentDecl enrichment = (AEnrichmentDecl)decl;
                            if (!Util.TypesEqual(aToType.GetType(), enrichment.GetType(), data))
                                continue;
                            if ((enrichment.GetDimention() == null) != (aFromType.IsPrimitive("string")))
                            {
                                errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                     "Expected cast from " +
                                                                     (enrichment.GetDimention() == null
                                                                          ? "string."
                                                                          : "int.")));
                                throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                            }
                            data.ExpTypes[node.GetExp()] = aToType;
                            node.ReplaceBy(node.GetExp());
                            return;
                        }
                    }
                }
                if (aFromType.IsPrimitive("int"))
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                         "Expected cast from string."));
                    throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                }
                data.ExpTypes[node.GetExp()] = aToType;
                node.ReplaceBy(node.GetExp());
                return;
            }
            errors.Add(new ErrorCollection.Error(node.GetToken(), "Sharp casts can only be used from pointers to int/string and vice versa."));
            throw new ParserException(node.GetToken(), "TypeChecking.OutASharpCastExp");
                

        }

        public override void OutACastExp(ACastExp node)
        {
            string toType = ((AAName)((ANamedType) node.GetType()).GetName()).AsString();
            string fromType;
            PType fromPType = data.ExpTypes[node.GetExp()];
            AStructDecl toEnum = null;
            AStructDecl fromEnum = null;

            if (data.StructTypeLinks.ContainsKey((ANamedType)node.GetType()))
            {
                AStructDecl str = data.StructTypeLinks[(ANamedType)node.GetType()];
                if (data.Enums.ContainsKey(str))
                    toEnum = str;
            }
            if (fromPType is ANamedType)
            {
                fromType = ((AAName)((ANamedType)fromPType).GetName()).AsString();
                //Namespace ignored
                if (data.StructTypeLinks.ContainsKey((ANamedType) fromPType))
                {
                    AStructDecl str = data.StructTypeLinks[(ANamedType) fromPType];
                    if (data.Enums.ContainsKey(str))
                        fromEnum = str;
                }
            }
            else
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Invalid cast"));
                throw new ParserException(node.GetToken(), "Invalid cast");
            }

            if (toEnum != null && (fromType == "int" || fromType == "byte"))
            {
                ANamedType type = new ANamedType(new TIdentifier(toEnum.GetName().Text), null);
                data.StructTypeLinks[type] = toEnum;
                data.ExpTypes[node.GetExp()] = type;
                node.ReplaceBy(node.GetExp());
                return;
            }

            if (fromEnum != null && (toType == "int" || toType == "byte"))
            {
                int enumDefinitions = 0;
                foreach (PLocalDecl local in fromEnum.GetLocals())
                {
                    if (local is AALocalDecl)
                        enumDefinitions++;
                }
                string typeName = enumDefinitions > 255 ? "int" : "byte";
                ANamedType type = new ANamedType(new TIdentifier(typeName), null);
                data.ExpTypes[node.GetExp()] = new ANamedType(new TIdentifier(typeName), null);
                node.ReplaceBy(node.GetExp());
                return;
            }

            if (fromEnum != null && toType == "string")
            {
                AMethodDecl targetMethod = data.StructMethods[fromEnum][0];
                ASimpleInvokeExp invokeExp = new ASimpleInvokeExp(new TIdentifier("toString"), new ArrayList(){node.GetExp()});
                data.SimpleMethodLinks[invokeExp] = targetMethod;
                data.ExpTypes[invokeExp] = targetMethod.GetReturnType();
                node.ReplaceBy(invokeExp);
                return;
            }

            ASimpleInvokeExp replacementMethod = null;
            switch (toType)
            {
                case "string":
                    switch (fromType)
                    {
                        case "wave":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("AIWaveToString"), new ArrayList{node.GetExp()});
                            break;
                        case "fixed"://Implicit
                            AFieldLvalue precisionArg = new AFieldLvalue(new TIdentifier("c_fixedPrecisionAny"));
                            ALvalueExp exp = new ALvalueExp(precisionArg);
                            data.FieldLinks[precisionArg] =
                                data.Libraries.Fields.First(field => field.GetName().Text == precisionArg.GetName().Text);
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("FixedToString"), new ArrayList { node.GetExp(), exp});
                            break;
                        case "int"://Implicit
                        case "byte"://Implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList { node.GetExp()});
                            break;
                        case "bool"://Implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("libNtve_gf_ConvertBooleanToString"), new ArrayList { node.GetExp() });
                            break;
                        case "color"://Implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("libNtve_gf_ConvertColorToString"), new ArrayList { node.GetExp() });
                            break;
                    }
                    break;
                case "text":
                    switch (fromType)
                    {
                        case "wave":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("AIWaveToText"), new ArrayList { node.GetExp() });
                            break;
                        case "fixed"://Implicit
                            AFieldLvalue precisionArg = new AFieldLvalue(new TIdentifier("c_fixedPrecisionAny"));
                            ALvalueExp exp = new ALvalueExp(precisionArg);
                            data.FieldLinks[precisionArg] =
                                data.Libraries.Fields.First(field => field.GetName().Text == precisionArg.GetName().Text);
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("FixedToText"), new ArrayList { node.GetExp(), exp });
                            break;
                        case "int"://Implicit
                        case "byte":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("IntToText"), new ArrayList { node.GetExp() });
                            break;
                        case "bool"://Implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("libNtve_gf_ConvertBooleanToText"), new ArrayList { node.GetExp() });
                            break;
                        case "string"://Implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("StringToText"), new ArrayList { node.GetExp() });
                            break;
                    }
                    break;
                case "int":
                    switch (fromType)
                    {
                        case "bool":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("BoolToInt"), new ArrayList {node.GetExp()});
                            break;
                        case "fixed":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("FixedToInt"), new ArrayList { node.GetExp() });
                            break;
                        case "string":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("StringToInt"), new ArrayList { node.GetExp() });
                            break;
                    }
                    break;
                case "fixed":
                    switch (fromType)
                    {
                        case "int"://Already implicit
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("IntToFixed"), new ArrayList { node.GetExp() });
                            break;
                        case "string":
                            replacementMethod = new ASimpleInvokeExp(new TIdentifier("StringToFixed"), new ArrayList { node.GetExp() });
                            break;
                    }
                    break;
                case "bool":
                    switch (fromType)
                    {
                        case "int":
                        case "byte":
                        case "fixed":
                            //Replace by
                            //exp != 0
                            AIntConstExp zero = new AIntConstExp(new TIntegerLiteral("0"));
                            ABinopExp binop = new ABinopExp(node.GetExp(), new ANeBinop(new TNeq("!=")), zero);
                            node.ReplaceBy(binop);

                            binop.Apply(this);
                            return;
                    }
                    break;
            }

            if (replacementMethod == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Unable to cast from " + fromType + " to " + toType));
                throw new ParserException(node.GetToken(), "Invalid cast");
            }


            data.SimpleMethodLinks[replacementMethod] =
                data.Libraries.Methods.First(method => method.GetName().Text == replacementMethod.GetName().Text);
            data.ExpTypes[replacementMethod] = data.SimpleMethodLinks[replacementMethod].GetReturnType();
            node.ReplaceBy(replacementMethod);
            for (int i = 1; i < replacementMethod.GetArgs().Count; i++)
            {
                ((Node)replacementMethod.GetArgs()[i]).Apply(this);
            }
        }

        private static bool ImplicitAssignable(PType from, PType to)
        {
            if (from is ANamedType && to is ANamedType)
            {
                string fromType = ((AAName)((ANamedType)from).GetName()).AsString();
                string toType = ((AAName)((ANamedType)to).GetName()).AsString();

                switch (toType)
                {
                    case "string":
                        switch (fromType)
                        {
                            case "fixed"://Implicit
                            case "int"://Implicit
                            case "byte"://Implicit
                            case "bool"://Implicit
                            case "color"://Implicit
                                return true;
                        }
                        break;
                    case "text":
                        switch (fromType)
                        {
                            case "fixed"://Implicit
                            case "int"://Implicit
                            case "byte"://Implicit
                            case "bool"://Implicit
                            case "string"://Implicit
                                return true;
                        }
                        break;
                }
            }
            return false;
        }


        public override void CaseASwitchStm(ASwitchStm node)
        {
            node.GetTest().Apply(this);

            AALocalDecl fallThroughDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                          new ANamedType(new TIdentifier("bool"), null),
                                                          new TIdentifier(MakeUniqueLocalName(node, "switchFallThrough")),
                                                          new ABooleanConstExp(new AFalseBool()));
            AALocalDecl continueDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                          new ANamedType(new TIdentifier("bool"), null),
                                                          new TIdentifier(MakeUniqueLocalName(node, "switchContinue")),
                                                          new ABooleanConstExp(new ATrueBool()));

            AALocalDecl testVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                          Util.MakeClone(data.ExpTypes[node.GetTest()], data),
                                                          new TIdentifier(MakeUniqueLocalName(node, "switchTestVar")),
                                                          node.GetTest());

            AABlock bigBlock = new AABlock();
            //AABlock previousBlock = bigBlock;
            if (node.GetCases().Count > 0)
            {
                List<SwitchCaseData> switchCaseDatas = new List<SwitchCaseData>();
                //Join cases without a body
                for (int i = node.GetCases().Count - 1; i >= 0; i--)
                {
                    ASwitchCaseStm caseStm = (ASwitchCaseStm) node.GetCases()[i];
                    SwitchCaseData caseData = new SwitchCaseData();
                    caseData.Block = (AABlock) caseStm.GetBlock();
                    if (caseStm.GetType() is ACaseSwitchCaseType)
                        caseData.Tests.Add(((ACaseSwitchCaseType)caseStm.GetType()).GetExp());
                    else
                        caseData.ContainsDefault = true;

                    caseData.IsLast = switchCaseDatas.Count == 0;

                    if (switchCaseDatas.Count == 0 || caseData.Block.GetStatements().Count > 0)
                    {
                        switchCaseDatas.Insert(0, caseData);
                        continue;
                    }
                    switchCaseDatas[0].Tests.AddRange(caseData.Tests);
                }
                for (int i = switchCaseDatas.Count - 1; i >= 0; i--)
                {
                    switchCaseDatas[i].ContainsFallthrough = CanFallthrough(switchCaseDatas[i].Block, out switchCaseDatas[i].HasBreaks, out switchCaseDatas[i].RequiresWhile);
                    if (i == switchCaseDatas.Count - 1)
                        continue;

                    switchCaseDatas[i + 1].TargetForFallThrough = switchCaseDatas[i].ContainsFallthrough;
                    switchCaseDatas[i].RequiresContinue = !switchCaseDatas[i].ContainsFallthrough &&
                                                          (switchCaseDatas[i + 1].RequiresContinue ||
                                                           switchCaseDatas[i + 1].ContainsFallthrough);
                }

                AABlock previousBlock = bigBlock;
                //Make code for specific case
                foreach (SwitchCaseData switchCase in switchCaseDatas)
                {
                    List<PExp> tests = new List<PExp>();
                    AABlock nextBlock;
                    if (switchCase.TargetForFallThrough)
                    {//Add if (continueSwitch) {}
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(continueDecl.GetName().Text));
                        ALvalueExp test = new ALvalueExp(lvalue);
                        nextBlock = new AABlock();
                        AIfThenStm ifStm = new AIfThenStm(new TLParen("("), test, new ABlockStm(new TLBrace("{"), nextBlock));
                        previousBlock.GetStatements().Add(ifStm);
                        previousBlock = nextBlock;

                        data.LocalLinks[lvalue] = continueDecl;
                        data.LvalueTypes[lvalue] = data.ExpTypes[test] = continueDecl.GetType();

                        //First test in next if: if (fallThrough || ...
                        lvalue = new ALocalLvalue(new TIdentifier(fallThroughDecl.GetName().Text));
                        test = new ALvalueExp(lvalue);
                        tests.Add(test);


                        data.LocalLinks[lvalue] = fallThroughDecl;
                        data.LvalueTypes[lvalue] = data.ExpTypes[test] = fallThroughDecl.GetType();
                    }
                    //Make code for the test in the if
                    foreach (PExp exp in switchCase.Tests)
                    {
                        ALocalLvalue leftSide = new ALocalLvalue(new TIdentifier(testVar.GetName().Text));
                        ALvalueExp lvalueExp = new ALvalueExp(leftSide);
                        ABinopExp test = new ABinopExp(lvalueExp, new AEqBinop(new TEq("==")), exp);
                        tests.Add(test);

                        data.LocalLinks[leftSide] = testVar;
                        data.LvalueTypes[leftSide] = data.ExpTypes[lvalueExp] = testVar.GetType();
                        data.ExpTypes[test] = new ANamedType(new TIdentifier("bool"), null);
                    }

                    if (switchCase.ContainsDefault)
                    {
                        ABooleanConstExp test = new ABooleanConstExp(new ATrueBool());
                        tests.Add(test);

                        data.ExpTypes[test] = new ANamedType(new TIdentifier("bool"), null);
                    }

                    PExp finalTest = tests[0];
                    tests.RemoveAt(0);
                    foreach (PExp exp in tests)
                    {
                        finalTest = new ABinopExp(finalTest, new ALazyOrBinop(new TOrOr("||")), exp);

                        data.ExpTypes[finalTest] = new ANamedType(new TIdentifier("bool"), null);
                    }

                    //Transform breaks into assignments

                    //If we can fallthrough, and there are breaks, encase in a while stm
                    AABlock testBlock = switchCase.Block;
                    if (switchCase.RequiresWhile)
                    {
                        AABlock newBlock = new AABlock();
                        PExp whileTest = new ABooleanConstExp(new ATrueBool());
                        AWhileStm whileStm = new AWhileStm(new TLParen("("), whileTest, new ABlockStm(new TLBrace("{"), switchCase.Block));
                        newBlock.GetStatements().Add(whileStm);
                        switchCase.Block = newBlock;
                    }

                    TransformBreaks(testBlock, switchCase, continueDecl, fallThroughDecl);

                    if (switchCase.ContainsFallthrough && !switchCase.TargetForFallThrough)
                    {//Add fallthrough = true;
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(fallThroughDecl.GetName().Text));
                        ABooleanConstExp rightSide = new ABooleanConstExp(new ATrueBool());
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), lvalue, rightSide);
                        testBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                        data.LocalLinks[lvalue] = fallThroughDecl;
                        data.LvalueTypes[lvalue] = data.ExpTypes[rightSide] = data.ExpTypes[assignment] = fallThroughDecl.GetType();
                    }

                    if (switchCase.RequiresWhile)
                    {//Add break at the end of the while
                        testBlock.GetStatements().Add(new ABreakStm(new TBreak("break")));
                    }

                    //Make if
                    PStm finalIfStm;
                    if (finalTest is ABooleanConstExp)
                    {//Final if is if(true). dont add it.
                        finalIfStm = new ABlockStm(new TLBrace("{"), switchCase.Block);
                        nextBlock = new AABlock();
                    }
                    else if (switchCase.IsLast || switchCase.ContainsFallthrough)
                    {//One armed if
                        finalIfStm = new AIfThenStm(new TLParen("("), finalTest,
                                                    new ABlockStm(new TLBrace("{"), switchCase.Block));
                        nextBlock = bigBlock;
                    }
                    else
                    {//Two armed if
                        nextBlock = new AABlock();
                        finalIfStm = new AIfThenElseStm(new TLParen("("), finalTest,
                                                        new ABlockStm(new TLBrace("{"), switchCase.Block),
                                                        new ABlockStm(new TLBrace("{"), nextBlock));
                    }

                    previousBlock.GetStatements().Add(finalIfStm);
                    previousBlock = nextBlock;
                }

                //If needed, add fallThroughDecl and continueDecl
                data.Locals.Add(bigBlock, new List<AALocalDecl>());
                if (data.LocalLinks.Values.Contains(fallThroughDecl))
                {
                    bigBlock.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), fallThroughDecl));
                    data.Locals[bigBlock].Add(fallThroughDecl);
                }
                if (data.LocalLinks.Values.Contains(continueDecl))
                {
                    bigBlock.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), continueDecl));
                    data.Locals[bigBlock].Add(continueDecl);
                }
                bigBlock.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), testVar));
                data.Locals[bigBlock].Add(testVar);
                
                node.ReplaceBy(new ABlockStm(new TLBrace("{"), bigBlock));
                bigBlock.Apply(this);
            }
        }

        private class SwitchCaseData
        {
            public List<PExp> Tests = new List<PExp>();
            public AABlock Block;
            public bool ContainsDefault;
            public bool TargetForFallThrough;
            public bool RequiresContinue;
            public bool ContainsFallthrough;
            public bool HasBreaks;
            public bool IsLast;

            public bool RequiresWhile;
        }

        private void TransformBreaks(AABlock block, SwitchCaseData switchCase, AALocalDecl continueDecl, AALocalDecl fallthroughDecl)
        {
            for (int i = 0; i < block.GetStatements().Count; i++)
            {
                PStm stm = (PStm) block.GetStatements()[i];
                int stmCount = block.GetStatements().Count;
                TransformBreaks(stm, switchCase, continueDecl, fallthroughDecl, ref i);
                /*if (stmCount > block.GetStatements().Count)
                    i--;*/
            }
        }

        private void TransformBreaks(PStm stm, SwitchCaseData switchCase, AALocalDecl continueDecl, AALocalDecl fallthroughDecl, ref int currI)
        {
            if (stm is ABreakStm)
            {
                AABlock pBlock = (AABlock) stm.Parent();
                if (!switchCase.IsLast && (switchCase.RequiresContinue || switchCase.ContainsFallthrough))
                {
                    //Add continue = false;
                    ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(continueDecl.GetName().Text));
                    ABooleanConstExp rightSide = new ABooleanConstExp(new AFalseBool());
                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), lvalue, rightSide);
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(stm), new AExpStm(new TSemicolon(";"), assignment));

                    data.LocalLinks[lvalue] = continueDecl;
                    data.LvalueTypes[lvalue] = data.ExpTypes[rightSide] = data.ExpTypes[assignment] = continueDecl.GetType();
                    currI++;
                }
                if (!switchCase.IsLast && switchCase.TargetForFallThrough)
                {
                    //Add fallthrough = false;
                    ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(fallthroughDecl.GetName().Text));
                    ABooleanConstExp rightSide = new ABooleanConstExp(new AFalseBool());
                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), lvalue, rightSide);
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(stm), new AExpStm(new TSemicolon(";"), assignment));

                    data.LocalLinks[lvalue] = fallthroughDecl;
                    data.LvalueTypes[lvalue] = data.ExpTypes[rightSide] = data.ExpTypes[assignment] = fallthroughDecl.GetType();
                    currI++;
                }
                if (!switchCase.RequiresWhile)
                {//Remove break
                    stm.Parent().RemoveChild(stm);
                    currI--;
                }
            }
            if (stm is ABlockStm)
                TransformBreaks((AABlock)((ABlockStm)stm).GetBlock(), switchCase, continueDecl, fallthroughDecl);
            if (stm is AIfThenStm)
                TransformBreaks(((AIfThenStm)stm).GetBody(), switchCase, continueDecl, fallthroughDecl, ref currI);
            if (stm is AIfThenElseStm)
            {
                TransformBreaks(((AIfThenElseStm)stm).GetThenBody(), switchCase, continueDecl, fallthroughDecl, ref currI);
                TransformBreaks(((AIfThenElseStm)stm).GetElseBody(), switchCase, continueDecl, fallthroughDecl, ref currI);
            }
        }

        private bool CanFallthrough(AABlock block, out bool containsBreak, out bool requiresWhile)
        {
            containsBreak = false;
            requiresWhile = false;
            foreach (PStm stm in block.GetStatements())
            {
                bool hasBreak;
                bool reqWhile;
                bool canFallThrough = CanFallthrough(stm, out hasBreak, out reqWhile);
                containsBreak |= hasBreak;
                requiresWhile |= reqWhile;
                requiresWhile |= containsBreak && block.GetStatements()[block.GetStatements().Count - 1] != stm;
                if (!canFallThrough)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanFallthrough(PStm stm, out bool containsBreak, out bool requiresWhile)
        {
            containsBreak = stm is ABreakStm;
            requiresWhile = false;
            if (stm is ABreakStm || stm is AVoidReturnStm || stm is AValueReturnStm)
                return false;
            if (stm is ABlockStm)
                return CanFallthrough((AABlock)((ABlockStm)stm).GetBlock(), out containsBreak, out requiresWhile);
            if (stm is AIfThenStm)
            {
                CanFallthrough(((AIfThenStm) stm).GetBody(), out containsBreak, out requiresWhile);
                return true;
            }
            if (stm is AIfThenElseStm)
            {
                bool b1 = CanFallthrough(((AIfThenElseStm) stm).GetThenBody(), out containsBreak, out requiresWhile);
                bool hasBreak;
                bool reqWhile;
                bool b2 = CanFallthrough(((AIfThenElseStm)stm).GetElseBody(), out hasBreak, out reqWhile);
                containsBreak |= hasBreak;
                requiresWhile |= reqWhile;
                return b1 || b2;
            }
            return true;
        }

        

        private string MakeUniqueLocalName(Node position, string name, params string[] extraNames)
        {
            return name;
            /*List<string> currentVariableNames = new List<string>(extraNames);
            {//Get other locals in the method
                AABlock parent = Util.GetAncestor<AABlock>(position);
                while (parent != null)
                {
                    currentVariableNames.AddRange(data.Locals[parent].Select(localDecl => localDecl.GetName().Text));
                    parent = Util.GetAncestor<AABlock>(parent.Parent());
                }

                //Also locals further down in the tree
                foreach (KeyValuePair<AABlock, List<AALocalDecl>> pair in data.Locals)
                {
                    if (Util.IsAncestor(pair.Key, position))
                        currentVariableNames.AddRange(pair.Value.Select(localDecl => localDecl.GetName().Text));
                }
            }
            {//Get enclosing struct variables
                AStructDecl parent = Util.GetAncestor<AStructDecl>(position);
                if (parent != null)
                {
                    currentVariableNames.AddRange(parent.GetLocals().OfType<AALocalDecl>().Select(local => local.GetName().Text));
                }
            }
            {//Get global user variables
                currentVariableNames.AddRange(data.Fields.Select(item => item.Decl).Where(decl => Util.IsVisible(position, decl)).Select(decl => decl.GetName().Text));
            }
            {//Get library fields
                currentVariableNames.AddRange(data.Libraries.Fields.Select(field => field.GetName().Text));
            }
            int i = 1;
            string returner = name;
            while (currentVariableNames.Contains(returner))
            {
                i++;
                returner = name + i;
            }
            return returner;*/
        }

        public static void GetTargets(string name,
            Token node,
            Node reciever, //Either null, AAName, or PExp
            PType returnType, //Either null, or Some type the method return type must be assignable to
            List<PType> argTypes,
            List<AMethodDecl> candidates,
            out bool matchArrayResize,
            List<AMethodDecl> implicitCandidates,
            List<AMethodDecl> matchingNames,
            out PExp baseExp,
            List<AMethodDecl> matchingDelegates,
            SharedData data,
            ErrorCollection errors
            )
        {
            baseExp = null;
            matchArrayResize = false;
            if (reciever == null)
            {//A simple invoke
                //Look in current struct
                //Look in all visible namespaces
                //Look in library methods
                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                if (currentStruct != null)
                {
                    foreach (AMethodDecl methodDecl in data.StructMethods[currentStruct])
                    {
                        if (methodDecl.GetName().Text == name &&
                            methodDecl.GetFormals().Count == argTypes.Count && 
                            methodDecl.GetDelegate() == null)
                        {
                            matchingNames.Add(methodDecl);

                            //Visibility
                            if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                Util.GetAncestor<AStructDecl>(methodDecl) != currentStruct)
                                continue;
                            if (methodDecl.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                                !Util.Extends(Util.GetAncestor<AStructDecl>(methodDecl), currentStruct, data))
                                continue;
                            if (methodDecl.GetStatic() == null &&
                                Util.IsStaticContext(node))
                                continue;

                            //Check return type
                            if (returnType != null && !(returnType is AVoidType) &&
                                !Assignable(methodDecl.GetReturnType(), returnType))
                                continue;

                            //Check that parameters are assignable
                            bool add = true;
                            bool matchImplicit = false;
                            for (int i = 0; i < argTypes.Count; i++)
                            {
                                PType argType = argTypes[i];
                                AALocalDecl formal = (AALocalDecl) methodDecl.GetFormals()[i];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    ||
                                    formal.GetRef() != null &&
                                    !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    ||
                                    formal.GetOut() == null && formal.GetRef() == null &&
                                    !Assignable(argType, formalType))
                                {
                                    add = false;
                                    if (formal.GetOut() == null && formal.GetRef() == null &&
                                        ImplicitAssignable(argType, formalType))
                                    {
                                        matchImplicit = true;
                                    }
                                    else
                                    {
                                        matchImplicit = false;
                                        break;
                                    }
                                }
                            }
                            if (!add && !matchImplicit)
                                continue;
                            if (candidates.Count == 0)
                            {//Set base exp
                                if (methodDecl.GetStatic() != null)
                                {
                                    //Calling static method
                                    baseExp = null;
                                }
                                else if (currentStruct.GetClassToken() != null || Util.HasAncestor<AConstructorDecl>(node) || Util.HasAncestor<ADeconstructorDecl>(node))
                                {//Dynamic context
                                    baseExp = new ALvalueExp(new APointerLvalue(new TStar("*"),
                                                                 new ALvalueExp(new AThisLvalue(new TThis("this")))));
                                }
                                else
                                {//Struct method to struct method
                                    baseExp = null;
                                }
                            }
                            if (add)
                                candidates.Add(methodDecl);
                            if (matchImplicit)
                                implicitCandidates.Add(methodDecl);
                        }
                    }
                }
                if (candidates.Count + implicitCandidates.Count == 0)
                {
                    //Global methods
                    List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                    AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
                    List<string> currentNamespace = Util.GetFullNamespace(node);
                    foreach (IList declList in visibleDecls)
                    {
                        bool isSameFile = false;
                        bool isSameNamespace = false;
                        if (declList.Count > 0)
                        {
                            isSameFile = currentFile == Util.GetAncestor<AASourceFile>((PDecl) declList[0]);
                            isSameNamespace = Util.NamespacesEquals(currentNamespace,
                                                                    Util.GetFullNamespace((PDecl) declList[0]));
                        }
                        foreach (PDecl decl in declList)
                        {
                            if (decl is AMethodDecl)
                            {
                                AMethodDecl methodDecl = (AMethodDecl) decl;
                                if (methodDecl.GetName().Text == name &&
                                    methodDecl.GetFormals().Count == argTypes.Count &&
                                    methodDecl.GetDelegate() == null)
                                {
                                    matchingNames.Add(methodDecl);

                                    //Visibility
                                    if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                        !isSameNamespace)
                                        continue;
                                    if (methodDecl.GetStatic() != null &&
                                        !isSameFile)
                                        continue;
                                    //Check return type
                                    if (returnType != null && !(returnType is AVoidType) &&
                                        !Assignable(methodDecl.GetReturnType(), returnType))
                                        continue;

                                    //Check that parameters are assignable
                                    bool add = true;
                                    bool matchImplicit = false;
                                    for (int i = 0; i < argTypes.Count; i++)
                                    {
                                        PType argType = argTypes[i];
                                        AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[i];
                                        PType formalType = formal.GetType();
                                        if (formal.GetOut() != null && !Assignable(formalType, argType)
                                            ||
                                            formal.GetRef() != null &&
                                            !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                            ||
                                            formal.GetOut() == null && formal.GetRef() == null &&
                                            !Assignable(argType, formalType))
                                        {
                                            add = false;
                                            if (formal.GetOut() == null && formal.GetRef() == null &&
                                                ImplicitAssignable(argType, formalType))
                                            {
                                                matchImplicit = true;
                                            }
                                            else
                                            {
                                                matchImplicit = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!add && !matchImplicit)
                                        continue;
                                    if (add)
                                        candidates.Add(methodDecl);
                                    if (matchImplicit)
                                        implicitCandidates.Add(methodDecl);
                                }
                            }
                        }
                    }
                    //Library methods
                    foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                    {
                        if (methodDecl.GetName().Text == name &&
                            methodDecl.GetFormals().Count == argTypes.Count &&
                            methodDecl.GetDelegate() == null)
                        {
                            matchingNames.Add(methodDecl);

                            //Visibility
                            //Okay, the library doesn't have any private methods. But hey.
                            if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                currentNamespace.Count > 0)
                                continue;
                            if (methodDecl.GetStatic() != null)
                                continue;
                            //Check return type
                            if (returnType != null && !(returnType is AVoidType) &&
                                !Assignable(methodDecl.GetReturnType(), returnType))
                                continue;

                            //Check that parameters are assignable
                            bool add = true;
                            bool matchImplicit = false;
                            for (int i = 0; i < argTypes.Count; i++)
                            {
                                PType argType = argTypes[i];
                                AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[i];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    ||
                                    formal.GetRef() != null &&
                                    !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    ||
                                    formal.GetOut() == null && formal.GetRef() == null &&
                                    !Assignable(argType, formalType))
                                {
                                    add = false;
                                    if (formal.GetOut() == null && formal.GetRef() == null &&
                                        ImplicitAssignable(argType, formalType))
                                    {
                                        matchImplicit = true;
                                    }
                                    else
                                    {
                                        matchImplicit = false;
                                        break;
                                    }
                                }
                            }
                            if (!add && !matchImplicit)
                                continue;
                            if (add)
                                candidates.Add(methodDecl);
                            if (matchImplicit)
                                implicitCandidates.Add(methodDecl);
                        }
                    }
                }
            }
            else if (reciever is AAName)
            {//Lookup possibilities for reciever
                List<List<Node>>[] targets;
                List<ANamespaceDecl> namespaces = new List<ANamespaceDecl>();
                bool reportedError;

                TypeLinking.GetTargets((AAName)reciever, out targets, namespaces, data, errors, out reportedError);
                
                if (reportedError)
                    throw new ParserException(node, "TypeChecking.GetTargets");

                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                int iteration;
                for (iteration = 0; iteration < targets.Length; iteration++)
                {
                    List<Node> matchingList = null;
                    foreach (List<Node> list in targets[iteration])
                    {
                        Node last = list[list.Count - 1];
                        PType type = null;
                        if (last is AALocalDecl)
                        {
                            type = ((AALocalDecl) last).GetType();
                        }
                        else if (last is APropertyDecl)
                        {
                            type = ((APropertyDecl) last).GetType();
                        }
                        else if (last is AFieldDecl)
                        {
                            type = ((AFieldDecl) last).GetType();
                        }
                        else if (last is TIdentifier)
                        {
                            type = new ANamedType(new TIdentifier("int"), null);
                        }
                        if (last is AStructDecl)
                        {
                            //Special. Static only
                            AStructDecl structDecl = ((AStructDecl)last);
                            foreach (AMethodDecl methodDecl in data.StructMethods[structDecl])
                            {
                                if (methodDecl.GetName().Text == name &&
                                    methodDecl.GetFormals().Count == argTypes.Count &&
                                    methodDecl.GetDelegate() == null)
                                {
                                    matchingNames.Add(methodDecl);

                                    //Visibility
                                    if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                        Util.GetAncestor<AStructDecl>(methodDecl) != currentStruct)
                                        continue;
                                    if (methodDecl.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                                        !Util.Extends(Util.GetAncestor<AStructDecl>(methodDecl), currentStruct, data))
                                        continue;
                                    if (methodDecl.GetStatic() == null)
                                        continue;
                                    //Check return type
                                    if (returnType != null && !(returnType is AVoidType) &&
                                        !Assignable(methodDecl.GetReturnType(), returnType))
                                        continue;
                                    //Check that parameters are assignable
                                    bool add = true;
                                    bool matchImplicit = false;
                                    for (int j = 0; j < argTypes.Count; j++)
                                    {
                                        PType argType = argTypes[j];
                                        AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[j];
                                        PType formalType = formal.GetType();
                                        if (formal.GetOut() != null && !Assignable(formalType, argType)
                                            ||
                                            formal.GetRef() != null &&
                                            !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                            ||
                                            formal.GetOut() == null && formal.GetRef() == null &&
                                            !Assignable(argType, formalType))
                                        {
                                            add = false;
                                            if (formal.GetOut() == null && formal.GetRef() == null &&
                                                ImplicitAssignable(argType, formalType))
                                            {
                                                matchImplicit = true;
                                            }
                                            else
                                            {
                                                matchImplicit = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!add && !matchImplicit)
                                        continue;
                                    if (candidates.Count == 0)
                                    {//Set base exp
                                        //Calling static method
                                        baseExp = null;
                                    }
                                    if (add)
                                        candidates.Add(methodDecl);
                                    if (matchImplicit)
                                        implicitCandidates.Add(methodDecl);

                                }
                            }
                        }
                        else
                        {
                            //Find methods based on baseType
                            if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) type) && !(data.Enums.ContainsKey(data.StructTypeLinks[(ANamedType) type])))
                            {
                                //Non static only
                                AStructDecl structDecl = data.StructTypeLinks[(ANamedType) type];
                                foreach (AMethodDecl methodDecl in data.StructMethods[structDecl])
                                {
                                    if (methodDecl.GetName().Text == name &&
                                        methodDecl.GetFormals().Count == argTypes.Count &&
                                        methodDecl.GetDelegate() == null)
                                    {
                                        matchingNames.Add(methodDecl);

                                        //Visibility
                                        if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                            Util.GetAncestor<AStructDecl>(methodDecl) != currentStruct)
                                            continue;
                                        if (methodDecl.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                                            !Util.Extends(Util.GetAncestor<AStructDecl>(methodDecl), currentStruct, data))
                                            continue;
                                        if (methodDecl.GetStatic() != null)
                                            continue;
                                        //Check return type
                                        if (returnType != null && !(returnType is AVoidType) &&
                                            !Assignable(methodDecl.GetReturnType(), returnType))
                                            continue;
                                        //Check that parameters are assignable
                                        bool add = true;
                                        bool matchImplicit = false;
                                        for (int j = 0; j < argTypes.Count; j++)
                                        {
                                            PType argType = argTypes[j];
                                            AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[j];
                                            PType formalType = formal.GetType();
                                            if (formal.GetOut() != null && !Assignable(formalType, argType)
                                                ||
                                                formal.GetRef() != null &&
                                                !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                                ||
                                                formal.GetOut() == null && formal.GetRef() == null &&
                                                !Assignable(argType, formalType))
                                            {
                                                add = false;
                                                if (formal.GetOut() == null && formal.GetRef() == null &&
                                                    ImplicitAssignable(argType, formalType))
                                                {
                                                    matchImplicit = true;
                                                }
                                                else
                                                {
                                                    matchImplicit = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!add && !matchImplicit)
                                            continue;
                                        if (candidates.Count == 0 && implicitCandidates.Count == 0)
                                        {//Set base exp
                                            baseExp = new ALvalueExp(TypeLinking.Link((AAName) reciever, node, list, data));
                                        }
                                        if (add)
                                            candidates.Add(methodDecl);
                                        if (matchImplicit)
                                            implicitCandidates.Add(methodDecl);
                                    }
                                }
                            }
                            else if (type is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType)type))
                            {
                                if (matchingDelegates != null && name == "Invoke")
                                {
                                    AMethodDecl delegateDecl = data.DelegateTypeLinks[(ANamedType) type];
                                    if (delegateDecl.GetFormals().Count == argTypes.Count)
                                    {
                                        matchingNames.Add(delegateDecl);

                                        //Check return type
                                        if (returnType != null && !(returnType is AVoidType) &&
                                            !Assignable(delegateDecl.GetReturnType(), returnType))
                                            continue;
                                        //Check that parameters are assignable
                                        bool add = true;
                                        bool matchImplicit = false;
                                        for (int j = 0; j < argTypes.Count; j++)
                                        {
                                            PType argType = argTypes[j];
                                            AALocalDecl formal = (AALocalDecl)delegateDecl.GetFormals()[j];
                                            PType formalType = formal.GetType();
                                            if (formal.GetOut() != null && !Assignable(formalType, argType)
                                                ||
                                                formal.GetRef() != null &&
                                                !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                                ||
                                                formal.GetOut() == null && formal.GetRef() == null &&
                                                !Assignable(argType, formalType))
                                            {
                                                add = false;
                                                if (formal.GetOut() == null && formal.GetRef() == null &&
                                                    ImplicitAssignable(argType, formalType))
                                                {
                                                    matchImplicit = true;
                                                }
                                                else
                                                {
                                                    matchImplicit = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!add && !matchImplicit)
                                            continue;
                                        matchingDelegates.Add(delegateDecl);
                                        if (candidates.Count == 0 && implicitCandidates.Count == 0)
                                        {//Set base exp
                                            baseExp = new ALvalueExp(TypeLinking.Link((AAName)reciever, node, list, data));
                                        }
                                        if (add)
                                            candidates.Add(delegateDecl);
                                        if (matchImplicit)
                                            implicitCandidates.Add(delegateDecl);

                                    }
                                }
                            }
                            else
                            {
                                //Look for enrichments
                                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                                AEnrichmentDecl currentEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
                                foreach (IList declList in visibleDecls)
                                {
                                    foreach (PDecl decl in declList)
                                    {
                                        if (decl is AEnrichmentDecl)
                                        {
                                            AEnrichmentDecl enrichment = (AEnrichmentDecl) decl;
                                            if (!Util.TypesEqual(type, enrichment.GetType(), data))
                                                continue;
                                            foreach (PDecl enrichmentDecl in enrichment.GetDecl())
                                            {
                                                if (enrichmentDecl is AMethodDecl)
                                                {
                                                    AMethodDecl methodDecl = (AMethodDecl) enrichmentDecl;

                                                    if (methodDecl.GetName().Text == name &&
                                                        methodDecl.GetFormals().Count == argTypes.Count &&
                                                        methodDecl.GetDelegate() == null)
                                                    {
                                                        matchingNames.Add(methodDecl);

                                                        //Visibility
                                                        if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                                            enrichment != currentEnrichment)
                                                            continue;
                                                        if (methodDecl.GetStatic() != null)
                                                            continue;
                                                        //Check return type
                                                        if (returnType != null && !(returnType is AVoidType) &&
                                                            !Assignable(methodDecl.GetReturnType(), returnType))
                                                            continue;
                                                        //Check that parameters are assignable
                                                        bool add = true;
                                                        bool matchImplicit = false;
                                                        for (int j = 0; j < argTypes.Count; j++)
                                                        {
                                                            PType argType = argTypes[j];
                                                            AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[j];
                                                            PType formalType = formal.GetType();
                                                            if (formal.GetOut() != null && !Assignable(formalType, argType)
                                                                ||
                                                                formal.GetRef() != null &&
                                                                !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                                                ||
                                                                formal.GetOut() == null && formal.GetRef() == null &&
                                                                !Assignable(argType, formalType))
                                                            {
                                                                add = false;
                                                                if (formal.GetOut() == null && formal.GetRef() == null &&
                                                                    ImplicitAssignable(argType, formalType))
                                                                {
                                                                    matchImplicit = true;
                                                                }
                                                                else
                                                                {
                                                                    matchImplicit = false;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        if (!add && !matchImplicit)
                                                            continue;
                                                        if (candidates.Count == 0 && implicitCandidates.Count == 0)
                                                        {//Set base exp
                                                            baseExp = new ALvalueExp(TypeLinking.Link((AAName)reciever, node, list, data));
                                                        }
                                                        if (add)
                                                            candidates.Add(methodDecl);
                                                        if (matchImplicit)
                                                            implicitCandidates.Add(methodDecl);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (candidates.Count + implicitCandidates.Count > 0)
                        break;
                }
                if (iteration >= 2)
                {//The search continued to global variables. Look in namespaces as well
                    AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
                    List<string> currentNamespace = Util.GetFullNamespace(node);
                    foreach (ANamespaceDecl namespaceDecl in namespaces)
                    {
                        bool isSameFile = Util.GetAncestor<AASourceFile>(namespaceDecl) == currentFile;
                        bool isSameNamespace = Util.NamespacesEquals(currentNamespace,
                                                                     Util.GetFullNamespace(namespaceDecl));
                        foreach (PDecl decl in namespaceDecl.GetDecl())
                        {
                            if (decl is AMethodDecl)
                            {
                                AMethodDecl methodDecl = (AMethodDecl) decl;
                                if (methodDecl.GetName().Text == name &&
                                    methodDecl.GetFormals().Count == argTypes.Count &&
                                    methodDecl.GetDelegate() == null)
                                {
                                    matchingNames.Add(methodDecl);

                                    //Visibility
                                    if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                        !isSameNamespace)
                                        continue;
                                    if (methodDecl.GetStatic() != null &&
                                        !isSameFile)
                                        continue;
                                    //Check return type
                                    if (returnType != null && !(returnType is AVoidType) &&
                                        !Assignable(methodDecl.GetReturnType(), returnType))
                                        continue;

                                    //Check that parameters are assignable
                                    bool add = true;
                                    bool matchImplicit = false;
                                    for (int i = 0; i < argTypes.Count; i++)
                                    {
                                        PType argType = argTypes[i];
                                        AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[i];
                                        PType formalType = formal.GetType();
                                        if (formal.GetOut() != null && !Assignable(formalType, argType)
                                            ||
                                            formal.GetRef() != null &&
                                            !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                            ||
                                            formal.GetOut() == null && formal.GetRef() == null &&
                                            !Assignable(argType, formalType))
                                        {
                                            add = false;
                                            if (formal.GetOut() == null && formal.GetRef() == null &&
                                                ImplicitAssignable(argType, formalType))
                                            {
                                                matchImplicit = true;
                                            }
                                            else
                                            {
                                                matchImplicit = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!add && !matchImplicit)
                                        continue;
                                    if (add)
                                        candidates.Add(methodDecl);
                                    if (matchImplicit)
                                        implicitCandidates.Add(methodDecl);
                                }
                            }
                        }
                    }
                }
            }
            else
            {//Get base type from exp, and find matching enrichment/struct
                PType type = data.ExpTypes[(PExp) reciever];

                if (type is ADynamicArrayType && name == "Resize" && argTypes.Count == 1 && Assignable(argTypes[0], new ANamedType(new TIdentifier("int"), null)))
                {
                    matchArrayResize = true;
                    baseExp = (PExp) reciever;
                }

                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)type))
                {
                    //Non static only
                    AStructDecl structDecl = data.StructTypeLinks[(ANamedType)type];
                    foreach (AMethodDecl methodDecl in data.StructMethods[structDecl])
                    {
                        if (methodDecl.GetName().Text == name &&
                            methodDecl.GetFormals().Count == argTypes.Count &&
                            methodDecl.GetDelegate() == null)
                        {
                            matchingNames.Add(methodDecl);

                            //Visibility
                            if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                Util.GetAncestor<AStructDecl>(methodDecl) != currentStruct)
                                continue;
                            if (methodDecl.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                                !Util.Extends(Util.GetAncestor<AStructDecl>(methodDecl), currentStruct, data))
                                continue;
                            if (methodDecl.GetStatic() != null)
                                continue;
                            //Check return type
                            if (returnType != null && !(returnType is AVoidType) &&
                                !Assignable(methodDecl.GetReturnType(), returnType))
                                continue;
                            //Check that parameters are assignable
                            bool add = true;
                            bool matchImplicit = false;
                            for (int j = 0; j < argTypes.Count; j++)
                            {
                                PType argType = argTypes[j];
                                AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[j];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    ||
                                    formal.GetRef() != null &&
                                    !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    ||
                                    formal.GetOut() == null && formal.GetRef() == null &&
                                    !Assignable(argType, formalType))
                                {
                                    add = false;
                                    if (formal.GetOut() == null && formal.GetRef() == null &&
                                        ImplicitAssignable(argType, formalType))
                                    {
                                        matchImplicit = true;
                                    }
                                    else
                                    {
                                        matchImplicit = false;
                                        break;
                                    }
                                }
                            }
                            if (!add && !matchImplicit)
                                continue;
                            if (candidates.Count == 0 && implicitCandidates.Count == 0)
                            {//Set base exp
                                baseExp = (PExp)reciever;
                            }
                            if (add)
                                candidates.Add(methodDecl);
                            if (matchImplicit)
                                implicitCandidates.Add(methodDecl);
                        }
                    }
                }

                else if (type is ANamedType && data.DelegateTypeLinks.ContainsKey((ANamedType)type))
                {
                    if (matchingDelegates != null && name == "Invoke")
                    {
                        AMethodDecl delegateDecl = data.DelegateTypeLinks[(ANamedType)type];
                        if (delegateDecl.GetFormals().Count == argTypes.Count)
                        {
                            matchingNames.Add(delegateDecl);

                            //Check return type
                            if (!(returnType != null && !(returnType is AVoidType) &&
                                !Assignable(delegateDecl.GetReturnType(), returnType)))
                            {
                                //Check that parameters are assignable
                                bool add = true;
                                bool matchImplicit = false;
                                for (int j = 0; j < argTypes.Count; j++)
                                {
                                    PType argType = argTypes[j];
                                    AALocalDecl formal = (AALocalDecl) delegateDecl.GetFormals()[j];
                                    PType formalType = formal.GetType();
                                    if (formal.GetOut() != null && !Assignable(formalType, argType)
                                        ||
                                        formal.GetRef() != null &&
                                        !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                        ||
                                        formal.GetOut() == null && formal.GetRef() == null &&
                                        !Assignable(argType, formalType))
                                    {
                                        add = false;
                                        if (formal.GetOut() == null && formal.GetRef() == null &&
                                            ImplicitAssignable(argType, formalType))
                                        {
                                            matchImplicit = true;
                                        }
                                        else
                                        {
                                            matchImplicit = false;
                                            break;
                                        }
                                    }
                                }
                                if (add || matchImplicit)
                                {
                                    matchingDelegates.Add(delegateDecl);
                                    if (candidates.Count == 0 && implicitCandidates.Count == 0)
                                    {
                                        //Set base exp
                                        baseExp = (PExp)reciever;
                                    }
                                    if (add)
                                        candidates.Add(delegateDecl);
                                    if (matchImplicit)
                                        implicitCandidates.Add(delegateDecl);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Look for enrichments
                    List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                    AEnrichmentDecl currentEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
                    foreach (IList declList in visibleDecls)
                    {
                        foreach (PDecl decl in declList)
                        {
                            if (decl is AEnrichmentDecl)
                            {
                                AEnrichmentDecl enrichment = (AEnrichmentDecl)decl;
                                if (!Util.TypesEqual(type, enrichment.GetType(), data))
                                    continue;
                                foreach (PDecl enrichmentDecl in enrichment.GetDecl())
                                {
                                    if (enrichmentDecl is AMethodDecl)
                                    {
                                        AMethodDecl methodDecl = (AMethodDecl)enrichmentDecl;

                                        if (methodDecl.GetName().Text == name &&
                                            methodDecl.GetFormals().Count == argTypes.Count &&
                                            methodDecl.GetDelegate() == null)
                                        {
                                            matchingNames.Add(methodDecl);

                                            //Visibility
                                            if (methodDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                                enrichment != currentEnrichment)
                                                continue;
                                            if (methodDecl.GetStatic() != null)
                                                continue;
                                            //Check return type
                                            if (returnType != null && !(returnType is AVoidType) &&
                                                !Assignable(methodDecl.GetReturnType(), returnType))
                                                continue;
                                            //Check that parameters are assignable
                                            bool add = true;
                                            bool matchImplicit = false;
                                            for (int j = 0; j < argTypes.Count; j++)
                                            {
                                                PType argType = argTypes[j];
                                                AALocalDecl formal = (AALocalDecl)methodDecl.GetFormals()[j];
                                                PType formalType = formal.GetType();
                                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                                    ||
                                                    formal.GetRef() != null &&
                                                    !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                                    ||
                                                    formal.GetOut() == null && formal.GetRef() == null &&
                                                    !Assignable(argType, formalType))
                                                {
                                                    add = false;
                                                    if (formal.GetOut() == null && formal.GetRef() == null &&
                                                        ImplicitAssignable(argType, formalType))
                                                    {
                                                        matchImplicit = true;
                                                    }
                                                    else
                                                    {
                                                        matchImplicit = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!add && !matchImplicit)
                                                continue;
                                            if (candidates.Count == 0 && implicitCandidates.Count == 0)
                                            {//Set base exp
                                                baseExp = (PExp)reciever;
                                            }
                                            if (add)
                                                candidates.Add(methodDecl);
                                            if (matchImplicit)
                                                implicitCandidates.Add(methodDecl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int candidateCount = candidates.Count + (matchArrayResize ? 1 : 0);
            if (candidateCount + implicitCandidates.Count == 0 && !matchArrayResize)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (AMethodDecl matchingName in matchingNames)
                {
                    subErrors.Add(new ErrorCollection.Error(matchingName.GetName(), "Method matching by name and number of parameters"));
                }
                errors.Add(new ErrorCollection.Error(node, "Unable to find a matching method.", false, subErrors.ToArray()));
                throw new ParserException(node, "TypeChecking.GetTargets");
            }
            if (candidateCount > 1)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (AMethodDecl matchingName in candidates)
                {
                    subErrors.Add(new ErrorCollection.Error(matchingName.GetName(), "Mathing method"));
                }
                if (matchArrayResize)
                    subErrors.Add(new ErrorCollection.Error(node, "Matches dynamic array resize"));
                errors.Add(new ErrorCollection.Error(node, "Found multiple matching methods.", false, subErrors.ToArray()));
                throw new ParserException(node, "TypeChecking.GetTargets");
            }
            if (candidateCount == 0 && implicitCandidates.Count > 1)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (AMethodDecl matchingName in implicitCandidates)
                {
                    subErrors.Add(new ErrorCollection.Error(matchingName.GetName(), "Mathing method"));
                }
                errors.Add(new ErrorCollection.Error(node, "Found multiple implicitly matching methods.", false, subErrors.ToArray()));
                throw new ParserException(node, "TypeChecking.GetTargets");
            }
        }

        public override void OutAAsyncInvokeStm(AAsyncInvokeStm node)
        {
            MakeInvoke(new InvokeStm(node));
            base.OutAAsyncInvokeStm(node);
        }

        public override void OutASyncInvokeExp(ASyncInvokeExp node)
        {
            MakeInvoke(new InvokeStm(node));
            base.OutASyncInvokeExp(node);
        }

        private void MakeInvoke(InvokeStm node)
        {
            //Find target method
            List<AMethodDecl> candidates = new List<AMethodDecl>();
            List<AMethodDecl> implicitCandidates = new List<AMethodDecl>();
            List<AMethodDecl> matchingNames = new List<AMethodDecl>();

            List<PType> argTypes = new List<PType>();
            foreach (PExp exp in node.Args)
            {
                argTypes.Add(data.ExpTypes[exp]);
            }
            PExp baseExp;
            bool needsVisit = node.Base == null || node.Base is AAName;
            bool matchResize;

            if (node.Base != null)
            {
                
            }

            GetTargets(node.Name.Text, node.Token, node.Base, null, argTypes, candidates, out matchResize, implicitCandidates, matchingNames, out baseExp, null, data, errors);


            if (needsVisit && baseExp != null)
            {
                //Add as an arg, visit it, then remove it
                node.Args.Add(baseExp);
                baseExp.Apply(this);
                node.Args.Remove(baseExp);
            }

            AMethodDecl candidate = candidates.Count == 1 ? candidates[0] : implicitCandidates[0];
            bool isImplicitCandidate = candidates.Count != 1;

            if (baseExp != null && baseExp is ALvalueExp && ((ALvalueExp)baseExp).GetLvalue() is APointerLvalue && Util.HasAncestor<AStructDecl>(candidate))
            {
                baseExp = ((APointerLvalue) ((ALvalueExp) baseExp).GetLvalue()).GetBase();
            }
            node.BaseExp = baseExp;

            if (isImplicitCandidate)
            {
                //Do the implicit casts
                for (int i = 0; i < node.Args.Count; i++)
                {
                    PType argType = argTypes[i];
                    AALocalDecl formal = (AALocalDecl) candidate.GetFormals()[i];
                    PType formalType = formal.GetType();
                    if (formal.GetOut() != null && !Assignable(formalType, argType)
                        ||
                        formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                        || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                    {
                        PExp exp = (PExp) node.Args[i];
                        ACastExp cast = new ACastExp(new TLParen("("), Util.MakeClone(formalType, data), null);
                        exp.ReplaceBy(cast);
                        cast.SetExp(exp);
                        OutACastExp(cast);
                    }
                }
            }

            if (!data.Invokes.ContainsKey(candidate))
                data.Invokes.Add(candidate, new List<InvokeStm>());
            data.Invokes[candidate].Add(node);
            if (!node.IsAsync)
                data.ExpTypes.Add(node.SyncNode, candidate.GetReturnType());

            //For each formal marked as ref or out, the argument must be a variable
            for (int i = 0; i < node.Args.Count; i++)
            {
                AALocalDecl formal = (AALocalDecl)candidate.GetFormals()[i];
                if (formal.GetRef() != null || formal.GetOut() != null)
                {
                    PExp exp = (PExp)node.Args[i];
                    while (true)
                    {
                        PLvalue lvalue;
                        if (exp is ALvalueExp)
                        {
                            lvalue = ((ALvalueExp)exp).GetLvalue();
                        }
                        else if (exp is AAssignmentExp)
                        {
                            lvalue = ((AAssignmentExp)exp).GetLvalue();
                        }
                        else
                        {
                            errors.Add(new ErrorCollection.Error(node.Token, currentSourceFile, "Argument " + (i + 1) + " must be a variable, as the parameter is marked as out or ref."));
                            break;
                        }
                        if (lvalue is ALocalLvalue ||
                            lvalue is AFieldLvalue ||
                            lvalue is AStructFieldLvalue)
                            break;
                        if (lvalue is AStructLvalue)
                        {
                            exp = ((AStructLvalue)lvalue).GetReceiver();
                            continue;
                        }
                        if (lvalue is AArrayLvalue)
                        {
                            exp = ((AArrayLvalue)lvalue).GetBase();
                            continue;
                        }
                        throw new Exception("Unexpected lvalue");
                    }
                }
            }
        }

        public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
        {
            if (data.SimpleMethodLinks.ContainsKey(node))
                return;




            List<AMethodDecl> candidates = new List<AMethodDecl>();
            List<AMethodDecl> implicitCandidates = new List<AMethodDecl>();
            List<AMethodDecl> matchingNames = new List<AMethodDecl>();

            List<PType> argTypes = new List<PType>();
            foreach (PExp exp in node.GetArgs())
            {
                argTypes.Add(data.ExpTypes[exp]);
            }
            PExp baseExp;
            bool matchResize;
            GetTargets(node.GetName().Text, node.GetName(), null, null, argTypes, candidates, out matchResize, implicitCandidates, matchingNames, out baseExp, null, data, errors);

            if (baseExp != null)
            {
                ANonstaticInvokeExp replacer = new ANonstaticInvokeExp(baseExp, new ADotDotType(new TDot(".")), node.GetName(), new ArrayList());
                while (node.GetArgs().Count > 0)
                {
                    replacer.GetArgs().Add(node.GetArgs()[0]);
                }
                node.ReplaceBy(replacer);
                baseExp.Apply(this);
                OutANonstaticInvokeExp(replacer);
                return;
            }

            AMethodDecl decl;
            if (candidates.Count == 0 && implicitCandidates.Count == 1)
            {
                //Do the implicit casts
                for (int i = 0; i < node.GetArgs().Count; i++)
                {
                    PType argType = data.ExpTypes[(PExp)node.GetArgs()[i]];
                    AALocalDecl formal = (AALocalDecl)implicitCandidates[0].GetFormals()[i];
                    PType formalType = formal.GetType();
                    if (formal.GetOut() != null && !Assignable(formalType, argType)
                        || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                        || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                    {
                        PExp exp = (PExp)node.GetArgs()[i];
                        ACastExp cast = new ACastExp(new TLParen("("), Util.MakeClone(formalType, data), null);
                        exp.ReplaceBy(cast);
                        cast.SetExp(exp);
                        OutACastExp(cast);
                    }
                }
                decl = implicitCandidates[0];
            }
            else
                decl = candidates[0];

            data.SimpleMethodLinks.Add(node, decl);
            data.ExpTypes.Add(node, decl.GetReturnType());
            

            

            CheckInvoke(node, decl);

            base.OutASimpleInvokeExp(node);
        }

        private void CheckInvoke(ASimpleInvokeExp node, AMethodDecl target)
        {
            if (target.GetInline() != null)
            {
                AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
                AConstructorDecl pConstructor = Util.GetAncestor<AConstructorDecl>(node);
                ADeconstructorDecl pDeconstructor = Util.GetAncestor<ADeconstructorDecl>(node);
                if (pMethod == null && !Util.HasAncestor<AConstructorDecl>(node) && 
                    !Util.HasAncestor<ADeconstructorDecl>(node) && !Util.HasAncestor<APropertyDecl>(node))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Inline methods can only be called from inside other methods."));
                }
                else if (pMethod != null && !InlineMethodCalls[pMethod].Contains(target))
                    InlineMethodCalls[pMethod].Add(target);
            }

            //For each formal marked as ref or out, the argument must be a non-const variable
            for (int i = 0; i < node.GetArgs().Count; i++)
            {
                AALocalDecl formal = (AALocalDecl)target.GetFormals()[i];
                if (formal.GetRef() != null || formal.GetOut() != null)
                {
                    PExp exp = (PExp)node.GetArgs()[i];
                    while (true)
                    {
                        PLvalue lvalue;
                        if (exp is ALvalueExp)
                        {
                            lvalue = ((ALvalueExp)exp).GetLvalue();
                        }
                        else if (exp is AAssignmentExp)
                        {
                            lvalue = ((AAssignmentExp)exp).GetLvalue();
                        }
                        else
                        {
                            errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Argument " + (i + 1) + " must be a variable, as the parameter is marked as out or ref."));
                            break;
                        }
                        if (lvalue is ALocalLvalue)
                        {
                            if (data.LocalLinks[(ALocalLvalue)lvalue].GetConst() != null)
                                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Argument " + (i + 1) + " can not be a constant variable, as the parameter is marked as out or ref."));
                            break;
                        }
                        if (lvalue is AFieldLvalue)
                        {
                            if (data.FieldLinks[(AFieldLvalue)lvalue].GetConst() != null)
                                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Argument " + (i + 1) + " can not be a constant variable, as the parameter is marked as out or ref."));
                            break;
                        }
                        if (lvalue is AStructFieldLvalue)
                        {
                            if (data.StructMethodFieldLinks[(AStructFieldLvalue)lvalue].GetConst() != null)
                                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Argument " + (i + 1) + " can not be a constant variable, as the parameter is marked as out or ref."));
                            break;
                        }
                        if (lvalue is AThisLvalue)
                            break;
                        if (lvalue is AStructLvalue)
                        {
                            exp = ((AStructLvalue)lvalue).GetReceiver();
                            continue;
                        }
                        if (lvalue is AArrayLvalue)
                        {
                            exp = ((AArrayLvalue)lvalue).GetBase();
                            continue;
                        }
                        if (lvalue is APointerLvalue)
                        {
                            exp = ((APointerLvalue)lvalue).GetBase();
                            continue;
                        }
                        throw new Exception("Unexpected lvalue");
                    }
                }
            }
        }

        public override void OutANonstaticInvokeExp(ANonstaticInvokeExp node)
        {
            List<AMethodDecl> candidates = new List<AMethodDecl>();
            List<AMethodDecl> implicitCandidates = new List<AMethodDecl>();
            List<AMethodDecl> matchingNames = new List<AMethodDecl>();
            List<AMethodDecl> delegateCandidates = new List<AMethodDecl>();

            List<PType> argTypes = new List<PType>();
            foreach (PExp exp in node.GetArgs())
            {
                argTypes.Add(data.ExpTypes[exp]);
            }
            PExp baseExp;
            bool needsVistit = false;
            Node reciever = node.GetReceiver();
            bool visitBaseExp = false;
            if (node.GetReceiver() is ALvalueExp && ((ALvalueExp)node.GetReceiver()).GetLvalue() is AAmbiguousNameLvalue)
            {
                visitBaseExp = true;
                reciever = ((AAmbiguousNameLvalue) ((ALvalueExp) node.GetReceiver()).GetLvalue()).GetAmbiguous();
            }
            bool matchResize;
            GetTargets(node.GetName().Text, node.GetName(), reciever, null, argTypes, candidates, out matchResize, implicitCandidates, matchingNames, out baseExp, delegateCandidates, data, errors);

            if (visitBaseExp && baseExp != null)
            {
                node.GetArgs().Add(baseExp);
                baseExp.Apply(this);
                node.GetArgs().Remove(baseExp);
            }

            if (matchResize)
            {
                AArrayResizeExp replacer = new AArrayResizeExp(node.GetName(), baseExp, (PExp) node.GetArgs()[0]);
                node.ReplaceBy(replacer);
                data.ExpTypes[replacer] = new ANamedType(new TIdentifier("void"), null);
                return;
            }

            if (implicitCandidates.Count > 0)
            {
                //Do the implicit casts
                for (int i = 0; i < node.GetArgs().Count; i++)
                {
                    PType argType = data.ExpTypes[(PExp)node.GetArgs()[i]];
                    AALocalDecl formal = (AALocalDecl)implicitCandidates[0].GetFormals()[i];
                    PType formalType = formal.GetType();
                    if (formal.GetOut() != null && !Assignable(formalType, argType)
                        || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                        || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                    {
                        PExp exp = (PExp)node.GetArgs()[i];
                        ACastExp cast = new ACastExp(new TLParen("("), Util.MakeClone(formalType, data), null);
                        exp.ReplaceBy(cast);
                        cast.SetExp(exp);
                        OutACastExp(cast);
                    }
                }
            }

            if (delegateCandidates.Count > 0)
            {//Target is a delegate invoke
                ADelegateInvokeExp replacer = new ADelegateInvokeExp(node.GetName(), baseExp, new ArrayList());
                while (node.GetArgs().Count > 0)
                {
                    replacer.GetArgs().Add(node.GetArgs()[0]);
                }
                data.ExpTypes[replacer] = delegateCandidates[0].GetReturnType();
                node.ReplaceBy(replacer);
                return;
            }
            AMethodDecl candidate = candidates.Count == 1 ? candidates[0] : implicitCandidates[0];

            if (baseExp == null)
            {
                //Replace with a simple invoke to it.
                ASimpleInvokeExp replacementInvoke = new ASimpleInvokeExp(node.GetName(), new ArrayList());
                while (node.GetArgs().Count > 0)
                {
                    replacementInvoke.GetArgs().Add(node.GetArgs()[0]);
                }
                data.SimpleMethodLinks[replacementInvoke] = candidate;
                data.ExpTypes[replacementInvoke] = candidate.GetReturnType();
                node.ReplaceBy(replacementInvoke);
                CheckInvoke(replacementInvoke, candidate);
                return;
            }
            node.SetReceiver(baseExp);

            data.StructMethodLinks[node] = candidate;
            data.ExpTypes[node] = candidate.GetReturnType();

            if (candidate.GetInline() != null)
            {
                AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
                AConstructorDecl pConstructor = Util.GetAncestor<AConstructorDecl>(node);
                APropertyDecl pProperty = Util.GetAncestor<APropertyDecl>(node);
                if (pMethod == null && pConstructor == null && pProperty == null && !Util.HasAncestor<ADeconstructorDecl>(node))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Inline methods can only be called from inside methods, constructors or properties."));
                }
                else if (pMethod != null && !InlineMethodCalls[pMethod].Contains(candidate))
                    InlineMethodCalls[pMethod].Add(candidate);
            }

            base.OutANonstaticInvokeExp(node);
        }

        public override void OutAIfThenElseStm(AIfThenElseStm node)
        {
            //Check that the type of the exp is not an array
            PType type = data.ExpTypes[node.GetCondition()];
            bool isValid = true;
            if (type is AArrayTempType)
                isValid = false;
            else if (type is ANamedType)
            {
                ANamedType aType = (ANamedType) type;
                if (data.StructTypeLinks.ContainsKey(aType))
                {
                    isValid = false;
                }
            }
            if (!isValid)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "The condition of the if can't be a struct or array."));
            }

            base.OutAIfThenElseStm(node);
        }

        public override void OutAIfThenStm(AIfThenStm node)
        {
            //Check that the type of the exp is not an array
            PType type = data.ExpTypes[node.GetCondition()];
            bool isValid = true;
            if (type is AArrayTempType)
                isValid = false;
            else if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                if (data.StructTypeLinks.ContainsKey(aType))
                {
                    isValid = false;
                }
            }
            if (!isValid)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), "The condition of the if can't be a struct or array."));
            }

            base.OutAIfThenStm(node);
        }

        public override void OutAIfExp(AIfExp node)
        {
            PType thenType = data.ExpTypes[node.GetThen()];
            PType elseType = data.ExpTypes[node.GetElse()];
            if (thenType is ANamedType && elseType is ANamedType)
            {
                ANamedType aThenType = (ANamedType) thenType;
                ANamedType aElseType = (ANamedType) elseType;

                if (aThenType.IsPrimitive("int") && aElseType.IsPrimitive("byte"))
                {
                    data.ExpTypes[node] = thenType;
                    return;
                }
            }
            if (Assignable(thenType, elseType))
                data.ExpTypes[node] = elseType;
            else if (Assignable(data.ExpTypes[node.GetElse()], data.ExpTypes[node.GetThen()]))
                data.ExpTypes[node] = data.ExpTypes[node.GetThen()];
            else
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                     "The then and else part of a ? expression must be assignable one way or the other."));
                //So execution can continue
                data.ExpTypes[node] = data.ExpTypes[node.GetThen()];
            }


            base.OutAIfExp(node);
        }

        public override void OutAThisLvalue(AThisLvalue node)
        {
            AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
            AConstructorDecl constructor = Util.GetAncestor<AConstructorDecl>(node);
            ADeconstructorDecl deconstructor = Util.GetAncestor<ADeconstructorDecl>(node);
            AEnrichmentDecl enrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            if (enrichment == null && (currentStruct == null || (currentStruct.GetClassToken() == null && constructor == null && deconstructor == null)))
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "The keyword this can only be used inside struct constructors, deconstructors, classes or enrichments."));
                throw new ParserException(node.GetToken(), "TypeChecking.OutAThisLvalue");
            }
            AMethodDecl method = Util.GetAncestor<AMethodDecl>(node);
            APropertyDecl property = Util.GetAncestor<APropertyDecl>(node);
            AALocalDecl field = Util.GetAncestor<AALocalDecl>(node);
            if (method != null && method.GetStatic() != null ||
                property != null && property.GetStatic() != null ||
                field != null && field.GetStatic() != null)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "The keyword this can not be used in a static context."));
                throw new ParserException(node.GetToken(), "TypeChecking.OutAThisLvalue");
            }
            
            if (enrichment != null)
            {
                if (constructor == null && deconstructor == null)
                    data.LvalueTypes[node] = enrichment.GetType();
                else
                    data.LvalueTypes[node] = new APointerType(new TStar("*"), Util.MakeClone(enrichment.GetType(), data));
            }
            else
            {
                ANamedType namedType = new ANamedType(new TIdentifier(currentStruct.GetName().Text), null);
                data.StructTypeLinks[namedType] = currentStruct;
                data.LvalueTypes[node] = new APointerType(new TStar("*"), namedType);
            }
            base.OutAThisLvalue(node);
        }

        public override void OutANewExp(ANewExp node)
        {
            /*if (node.GetType() is AArrayTempType)
                data.ExpTypes[node] = node.GetType();
            else*/
                data.ExpTypes[node] = new APointerType(new TStar("*"), Util.MakeClone(node.GetType(), data));
            
            List<AEnrichmentDecl> enrichments = new List<AEnrichmentDecl>();
            List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
            foreach (IList declList in visibleDecls)
            {
                foreach (PDecl decl in declList)
                {
                    if (decl is AEnrichmentDecl)
                    {
                        AEnrichmentDecl enrichment = (AEnrichmentDecl) decl;
                        if (!Util.TypesEqual(node.GetType(), enrichment.GetType(), data))
                            continue;
                        enrichments.Add(enrichment);
                    }
                }
            }
            if (enrichments.Count > 0 || node.GetType() is ANamedType)
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                ANamedType type = (ANamedType) node.GetType();
                if (enrichments.Count > 0 || data.StructTypeLinks.ContainsKey(type))
                {
                    //Find matching constructor
                    //Token token;
                    List<AConstructorDecl> candidates = new List<AConstructorDecl>();
                    if (enrichments.Count == 0)
                    {
                        AStructDecl str = data.StructTypeLinks[type];
                        if (data.Enums.ContainsKey(str))
                        {
                            errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                 "You can't create a new instance of an enum.", false,
                                                                 new ErrorCollection.Error(str.GetName(),
                                                                                           "Matching enum")));
                        }
                        //token = str.GetName();
                        subErrors.Add(new ErrorCollection.Error(str.GetName(), "Matching " + Util.GetTypeName(str)));
                        foreach (AConstructorDecl constructorDecl in data.StructConstructors[str])
                        {
                            //Visiblity
                            if (constructorDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                Util.GetAncestor<AStructDecl>(constructorDecl) != Util.GetAncestor<AStructDecl>(node))
                                continue;
                            if (constructorDecl.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                                (!Util.HasAncestor<AStructDecl>(node) ||
                                !Util.Extends(Util.GetAncestor<AStructDecl>(constructorDecl), Util.GetAncestor<AStructDecl>(node), data)))
                                continue;


                            if (constructorDecl.GetFormals().Count == node.GetArgs().Count)
                            {
                                //Check that parameters are assignable
                                bool add = true;
                                for (int i = 0; i < node.GetArgs().Count; i++)
                                {
                                    PType argType = data.ExpTypes[(PExp)node.GetArgs()[i]];
                                    AALocalDecl formal = (AALocalDecl)constructorDecl.GetFormals()[i];
                                    PType formalType = formal.GetType();
                                    if (formal.GetOut() != null && !Assignable(formalType, argType)
                                        || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                        || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                                if (add)
                                    candidates.Add(constructorDecl);
                            }
                        }
                    }
                    else
                    {
                        //token = null;
                        foreach (AEnrichmentDecl enrich in enrichments)
                        {
                            //token = enrich.GetToken();
                            subErrors.Add(new ErrorCollection.Error(enrich.GetToken(), "Matching enrichment"));
                            foreach (AConstructorDecl constructorDecl in enrich.GetDecl().OfType<AConstructorDecl>())
                            {
                                //Visiblity
                                if (constructorDecl.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                    Util.GetAncestor<AEnrichmentDecl>(constructorDecl) !=
                                    Util.GetAncestor<AEnrichmentDecl>(node))
                                    continue;


                                if (constructorDecl.GetFormals().Count == node.GetArgs().Count)
                                {
                                    //Check that parameters are assignable
                                    bool add = true;
                                    for (int i = 0; i < node.GetArgs().Count; i++)
                                    {
                                        PType argType = data.ExpTypes[(PExp) node.GetArgs()[i]];
                                        AALocalDecl formal = (AALocalDecl) constructorDecl.GetFormals()[i];
                                        PType formalType = formal.GetType();
                                        if (formal.GetOut() != null && !Assignable(formalType, argType)
                                            ||
                                            formal.GetRef() != null &&
                                            !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                            ||
                                            formal.GetOut() == null && formal.GetRef() == null &&
                                            !Assignable(argType, formalType))
                                        {
                                            add = false;
                                            break;
                                        }
                                    }
                                    if (add)
                                        candidates.Add(constructorDecl);
                                }
                            }
                        }
                    }


                    if (candidates.Count == 0)
                    {
                        if (node.GetArgs().Count == 0 && enrichments.Count > 0)
                            return;


                        string msg = "Could not find a constructor matching: (";
                        foreach (PExp arg in node.GetArgs())
                        {
                            if (msg.EndsWith("("))
                                msg += Util.TypeToString(data.ExpTypes[arg]);
                            else
                                msg += ", " + Util.TypeToString(data.ExpTypes[arg]);
                        }
                        msg += ")";
                        errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, msg, false, subErrors.ToArray()), true);
                        return;
                    }
                    if (candidates.Count > 1)
                    {
                        subErrors = new List<ErrorCollection.Error>();
                        int i = 0;
                        foreach (AConstructorDecl candidate in candidates)
                        {
                            i++;
                            subErrors.Add(new ErrorCollection.Error(candidate.GetName(), "Candidate " + i));
                        }
                        errors.Add(
                            new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                      "Ambigious constructor call. Found multiple matching constructor definitions.", false,
                                                      subErrors.ToArray()), true);
                        return;
                    }
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    if (pStm == null)
                    {
                        if (Util.HasAncestor<AFieldDecl>(node))
                        {
                            data.FieldsToInitInMapInit.Add(Util.GetAncestor<AFieldDecl>(node));
                        }
                        else if (Util.HasAncestor<AStructDecl>(node))
                        {
                            //Ignore - will be fixed
                        }
                        else
                            errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                                 "Instance creation expression must be in a method, since a constructor must be called."));
                    }
                    data.ConstructorLinks.Add(node, candidates[0]);

                    //For each formal marked as ref or out, the argument must be a variable
                    for (int i = 0; i < node.GetArgs().Count; i++)
                    {
                        AALocalDecl formal = (AALocalDecl)candidates[0].GetFormals()[i];
                        if (formal.GetRef() != null || formal.GetOut() != null)
                        {
                            PExp exp = (PExp)node.GetArgs()[i];
                            while (true)
                            {
                                PLvalue lvalue;
                                if (exp is ALvalueExp)
                                {
                                    lvalue = ((ALvalueExp)exp).GetLvalue();
                                }
                                else if (exp is AAssignmentExp)
                                {
                                    lvalue = ((AAssignmentExp)exp).GetLvalue();
                                }
                                else
                                {
                                    errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Argument " + (i + 1) + " must be a variable, as the parameter is marked as out or ref."));
                                    break;
                                }
                                if (lvalue is ALocalLvalue ||
                                    lvalue is AFieldLvalue ||
                                    lvalue is AStructFieldLvalue)
                                    break;
                                if (lvalue is AStructLvalue)
                                {
                                    exp = ((AStructLvalue)lvalue).GetReceiver();
                                    continue;
                                }
                                if (lvalue is AArrayLvalue)
                                {
                                    exp = ((AArrayLvalue)lvalue).GetBase();
                                    continue;
                                }
                                throw new Exception("Unexpected lvalue");
                            }
                        }
                    }
                    return;
                }
            }
            //else
            if (node.GetArgs().Count > 0)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Constructors are only defined for struct types"));
            }
        }

        public override void OutADeleteStm(ADeleteStm node)
        {
            //Child must be a pointer
            if (!(data.ExpTypes[node.GetExp()] is APointerType))
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Can only delete dynamically allocated types."));
            }

            //Deconstructor must be visible
            else
            {
                APointerType pointer = (APointerType) data.ExpTypes[node.GetExp()];
                if (pointer.GetType() is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) pointer.GetType()))
                {
                    //Struct deconstructor
                    AStructDecl currentStr = Util.GetAncestor<AStructDecl>(node);
                    AStructDecl str = data.StructTypeLinks[(ANamedType) pointer.GetType()];
                    while (str != null)
                    {
                        ADeconstructorDecl deconstructor = data.StructDeconstructor[str];
                        if (deconstructor.GetVisibilityModifier() is APrivateVisibilityModifier &&
                            currentStr != str)
                        {
                            errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                 "Unable to delete object from here, since a matching deconstructor is private.",
                                                                 false,
                                                                 new[]
                                                                     {
                                                                         new ErrorCollection.Error(
                                                                             deconstructor.GetName(), "Deconstructor")
                                                                     }));
                        }
                        else if (deconstructor.GetVisibilityModifier() is AProtectedVisibilityModifier &&
                            !Util.Extends(str, currentStr, data))
                        {
                            errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                 "Unable to delete object from here, since a matching deconstructor is protected.",
                                                                 false,
                                                                 new[]
                                                                     {
                                                                         new ErrorCollection.Error(
                                                                             deconstructor.GetName(), "Deconstructor")
                                                                     }));
                        }
                        if (str.GetBase() == null)
                            break;
                        str = data.StructTypeLinks[(ANamedType) str.GetBase()];
                    }
                }
                else
                {
                    //Enrichment deconstructor
                    AEnrichmentDecl currentEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
                    List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                    foreach (IList declList in visibleDecls)
                    {
                        foreach (PDecl decl in declList)
                        {
                            if (decl is AEnrichmentDecl)
                            {
                                AEnrichmentDecl enrichment = (AEnrichmentDecl)decl;
                                if (!Util.TypesEqual(pointer.GetType(), enrichment.GetType(), data))
                                    continue;
                                foreach (PDecl enrichmentDecl in enrichment.GetDecl())
                                {
                                    if (enrichmentDecl is ADeconstructorDecl)
                                    {
                                        ADeconstructorDecl deconstructor = (ADeconstructorDecl)enrichmentDecl;
                                        if (deconstructor.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                            currentEnrichment != enrichment)
                                        {
                                            errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                                 "Unable to delete object from here, since a matching deconstructor is private.",
                                                                                 false,
                                                                                 new[]
                                                                     {
                                                                         new ErrorCollection.Error(
                                                                             deconstructor.GetName(), "Deconstructor")
                                                                     }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            base.OutADeleteStm(node);
        }

        public override void OutAPointerLvalue(APointerLvalue node)
        {
            PType type = data.ExpTypes[node.GetBase()];
            if (!(type is APointerType))
            {
                errors.Add(new ErrorCollection.Error(node.GetTokens(), currentSourceFile, "Expected something of pointer type"));
                data.LvalueTypes[node] = type;
            }
            else
            {
                data.LvalueTypes[node] = ((APointerType) type).GetType();
            }

            base.OutAPointerLvalue(node);
        }

        public override void OutAArrayTempType(AArrayTempType node)
        {
            if (node.Parent() is APointerType && Util.GetAncestor<ANewExp>(node) == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Dynamic typed array can not put a requirement on the dimension."));
            }
            base.OutAArrayTempType(node);
        }

        public override void OutALvalueExp(ALvalueExp node)
        {
            if (!(node.GetLvalue() is AAmbiguousNameLvalue || 
                node.GetLvalue() is ANamespaceLvalue || 
                node.GetLvalue() is ATypeLvalue))
                if (node.Parent() != null) //If I havent been replaced
                    data.ExpTypes[node] = data.LvalueTypes[node.GetLvalue()];
            base.OutALvalueExp(node);
        }

        public override void OutAAssignmentExp(AAssignmentExp node)
        {
            PType from = data.ExpTypes[node.GetExp()];
            PType to = data.LvalueTypes[node.GetLvalue()];
            if (!Assignable(from, to))
            {
                if (ImplicitAssignable(from, to))
                {
                    ANamedType namedTo = (ANamedType) to;
                    ACastExp cast = new ACastExp(new TLParen("("), new ANamedType(new TIdentifier(((AAName)namedTo.GetName()).AsString()), null), node.GetExp());
                    node.SetExp(cast);
                    OutACastExp(cast);
                    //to = from;
                }
                else
                    errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                         "Unable to assign from type " + Util.TypeToString(from) +
                                                         " to type " + Util.TypeToString(to)));
            }
            data.ExpTypes[node] = to;

            if (node.GetLvalue() is ALocalLvalue)
            {
                assignedToOutParams.Add(data.LocalLinks[(ALocalLvalue) node.GetLvalue()]);
            }

            if (node.GetLvalue() is AStructLvalue && data.StructFieldLinks.ContainsKey((AStructLvalue) node.GetLvalue()))
            {
                AALocalDecl decl = data.StructFieldLinks[(AStructLvalue) node.GetLvalue()];
                if (decl.GetConst() != null)
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Unable to assign to a const field."));
            }

            if (node.GetLvalue() is AStructFieldLvalue && data.StructMethodFieldLinks.ContainsKey((AStructFieldLvalue)node.GetLvalue()))
            {
                AALocalDecl decl = data.StructMethodFieldLinks[(AStructFieldLvalue)node.GetLvalue()];
                if (decl.GetConst() != null)
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Unable to assign to a const field."));
            }

            base.OutAAssignmentExp(node);
        }




        public override void OutAALocalDecl(AALocalDecl node)
        {
            if (node.GetInit() != null)
            {
                PType from = data.ExpTypes[node.GetInit()];
                PType to = node.GetType();
                if (!Assignable(from, to))
                {
                    if (ImplicitAssignable(from, to))
                    {
                        ANamedType namedTo = (ANamedType)to;
                        ACastExp cast = new ACastExp(new TLParen("("), new ANamedType(new TIdentifier(((AAName)namedTo.GetName()).AsString()), null), node.GetInit());
                        node.SetInit(cast);
                        OutACastExp(cast);
                        to = from;
                    }
                    else
                        errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                             "Unable to assign from type " + Util.TypeToString(from) +
                                                             " to type " + Util.TypeToString(to)));
                }
            }

            //If the return type or the type of any formals is a private struct, and the method is a public context, give an error
            if (node.Parent() is AStructDecl)
            {
                AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);
                //Is public context
                if ( pStruct.GetVisibilityModifier() is APublicVisibilityModifier && !(node.GetVisibilityModifier() is APrivateVisibilityModifier))
                {
                    PType type = node.GetType();
                    int i = 0;
                    FindPrivateTypes finder = new FindPrivateTypes(data);
                    type.Apply(finder);

                    if (finder.PrivateTypes.Count > 0)
                    {
                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                        List<PDecl> usedDecls = new List<PDecl>();
                        foreach (ANamedType namedType in finder.PrivateTypes)
                        {
                            if (data.StructTypeLinks.ContainsKey(namedType))
                            {
                                AStructDecl decl = data.StructTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used struct"));
                            }
                            else if (data.DelegateTypeLinks.ContainsKey(namedType))
                            {
                                AMethodDecl decl = data.DelegateTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used delegate"));
                            }
                        }

                        errors.Add(new ErrorCollection.Error(node.GetName(), "Inconsistent accessibility. Private types used in public context.", false, subErrors.ToArray()));
                    }
                }
            }
            base.OutAALocalDecl(node);
        }


        public override void OutAParenExp(AParenExp node)
        {
            data.ExpTypes[node] = data.ExpTypes[node.GetExp()];
            base.OutAParenExp(node);
        }

        public override void OutAFieldDecl(AFieldDecl node)
        {
            if (node.GetInit() != null)
            {
                PType from = data.ExpTypes[node.GetInit()];
                PType to = node.GetType();
                if (!Assignable(from, to))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                         "Unable to assign from type " + Util.TypeToString(from) +
                                                         " to type " + Util.TypeToString(to)));
                }
            }

            //If the return type or the type of any formals is a private struct, and the method is a public context, give an error
            {
                //Is public context
                if (node.GetVisibilityModifier() is APublicVisibilityModifier)
                {
                    PType type = node.GetType();
                    FindPrivateTypes finder = new FindPrivateTypes(data);
                    
                    type.Apply(finder);


                    if (finder.PrivateTypes.Count > 0)
                    {
                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                        List<PDecl> usedDecls = new List<PDecl>();
                        foreach (ANamedType namedType in finder.PrivateTypes)
                        {
                            if (data.StructTypeLinks.ContainsKey(namedType))
                            {
                                AStructDecl decl = data.StructTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used struct"));
                            }
                            else if (data.DelegateTypeLinks.ContainsKey(namedType))
                            {
                                AMethodDecl decl = data.DelegateTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used delegate"));
                            }
                        }

                        errors.Add(new ErrorCollection.Error(node.GetName(), "Inconsistent accessibility. Private types used in public context.", false, subErrors.ToArray()));
                    }
                }
            }

            base.OutAFieldDecl(node);
        }

        public override void OutAMethodDecl(AMethodDecl node)
        {
            if (node.GetTrigger() != null)
            {
                bool validSignature = IsBoolType(node.GetReturnType());
                validSignature &= node.GetFormals().Count == 2;
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    validSignature &= IsBoolType(formal.GetType());
                    validSignature &= formal.GetRef() == null && formal.GetOut() == null;
                }
                if (!validSignature)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                         "Method marked as trigger, but does not match the signature (bool, bool):bool"));
                }
            }

            //Check that all code paths return a value
            if (!(node.GetReturnType() is AVoidType))
            {
                CheckReturns returnChecker = new CheckReturns();
                node.GetBlock().Apply(returnChecker);
                if (!returnChecker.Returned)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Not all code paths return a value"));
                }
            }

            //If the return type or the type of any formals is a private struct, and the method is a public context, give an error
            {
                AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);
                //Is public context
                if (pStruct == null && node.GetVisibilityModifier() is APublicVisibilityModifier ||
                    pStruct != null && pStruct.GetVisibilityModifier() is APublicVisibilityModifier && !(node.GetVisibilityModifier() is APrivateVisibilityModifier))
                {
                    PType type = node.GetReturnType();
                    int i = 0;
                    FindPrivateTypes finder = new FindPrivateTypes(data);
                    while (true)
                    {
                        type.Apply(finder);

                        if (i == node.GetFormals().Count)
                            break;
                        AALocalDecl formal = (AALocalDecl) node.GetFormals()[i];
                        type = formal.GetType();
                        i++;
                    }

                    if (finder.PrivateTypes.Count > 0)
                    {
                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                        List<PDecl> usedDecls = new List<PDecl>();
                        foreach (ANamedType namedType in finder.PrivateTypes)
                        {
                            if (data.StructTypeLinks.ContainsKey(namedType))
                            {
                                AStructDecl decl = data.StructTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used struct"));
                            }
                            else if (data.DelegateTypeLinks.ContainsKey(namedType))
                            {
                                AMethodDecl decl = data.DelegateTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used delegate"));
                            }
                        }

                        errors.Add(new ErrorCollection.Error(node.GetName(), "Inconsistent accessibility. Private types used in public context.", false, subErrors.ToArray()));
                    }
                }
            }

            base.OutAMethodDecl(node);
        }


        private class FindPrivateTypes : DepthFirstAdapter
        {
            public List<ANamedType> PrivateTypes = new List<ANamedType>();
            private SharedData data;

            public FindPrivateTypes(SharedData data)
            {
                this.data = data;
            }

            public override void OutANamedType(ANamedType node)
            {
                if (data.StructTypeLinks.ContainsKey(node))
                {
                    AStructDecl decl = data.StructTypeLinks[node];
                    if (decl.GetVisibilityModifier() is APrivateVisibilityModifier)
                        PrivateTypes.Add(node);
                }
                else if (data.DelegateTypeLinks.ContainsKey(node))
                {
                    AMethodDecl decl = data.DelegateTypeLinks[node];
                    if (decl.GetVisibilityModifier() is APrivateVisibilityModifier)
                        PrivateTypes.Add(node);
                }
            }
        }

        public override void OutAPropertyDecl(APropertyDecl node)
        {
            if (node.GetGetter() != null)
            {
                CheckReturns returnChecker = new CheckReturns();
                node.GetGetter().Apply(returnChecker);
                if (!returnChecker.Returned)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Not all code paths of the getter return a value"));
                }
            }

            //If the return type or the type of any formals is a private struct, and the method is a public context, give an error
            {
                //Is public context
                if (node.GetVisibilityModifier() is APublicVisibilityModifier)
                {
                    PType type = node.GetType();
                    int i = 0;
                    FindPrivateTypes finder = new FindPrivateTypes(data);
                    type.Apply(finder);

                    if (finder.PrivateTypes.Count > 0)
                    {
                        List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                        List<PDecl> usedDecls = new List<PDecl>();
                        foreach (ANamedType namedType in finder.PrivateTypes)
                        {
                            if (data.StructTypeLinks.ContainsKey(namedType))
                            {
                                AStructDecl decl = data.StructTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used struct"));
                            }
                            else if (data.DelegateTypeLinks.ContainsKey(namedType))
                            {
                                AMethodDecl decl = data.DelegateTypeLinks[namedType];
                                if (usedDecls.Contains(decl))
                                    continue;
                                usedDecls.Add(decl);
                                subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Used delegate"));
                            }
                        }

                        errors.Add(new ErrorCollection.Error(node.GetName(), "Inconsistent accessibility. Private types used in public context.", false, subErrors.ToArray()));
                    }
                }
            }

            base.OutAPropertyDecl(node);
        }

        private bool IsBoolType(PType type)
        {
            if (!(type is ANamedType))
                return false;
            return ((ANamedType) type).IsPrimitive("bool");
        }

        private class CheckReturns : DepthFirstAdapter
        {
            public bool Returned;

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                Returned = true;
            }

            public override void CaseAIfThenStm(AIfThenStm node)
            {
                //Skip the block.. we will need something below anyway
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                //Again.. skip block
            }

            public override void CaseAIfThenElseStm(AIfThenElseStm node)
            {
                if (Returned) return;
                node.GetThenBody().Apply(this);
                bool returned = Returned;
                Returned = false;
                node.GetElseBody().Apply(this);
                Returned &= returned;
            }
        }

        public override void OutAValueLvalue(AValueLvalue node)
        {
            data.LvalueTypes[node] = Util.GetAncestor<APropertyDecl>(node).GetType();
            base.OutAValueLvalue(node);
        }

        public override void OutAValueReturnStm(AValueReturnStm node)
        {
            AMethodDecl method = Util.GetAncestor<AMethodDecl>(node);
            AConstructorDecl constructor = Util.GetAncestor<AConstructorDecl>(node);
            APropertyDecl property = Util.GetAncestor<APropertyDecl>(node);
            AABlock lastBlock = Util.GetLastAncestor<AABlock>(node);

            
            //WTF is this? constructors can only return void
            if (constructor == null)
            {
                PType from = data.ExpTypes[node.GetExp()];
                PType to;

                if (property != null)
                {
                    if (lastBlock == property.GetSetter())
                    {
                        errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                             "Unable to assign from type " + Util.TypeToString(from) +
                                                             " to type void"));
                        return;
                    }
                }

                if (method != null)
                    to = method.GetReturnType();
                else
                    to = property.GetType();
                if (!Assignable(from, to))
                {
                    if (ImplicitAssignable(from, to))
                    {
                        ANamedType namedTo = (ANamedType) to;
                        ACastExp cast = new ACastExp(new TLParen("("),
                                                     new ANamedType(new TIdentifier(((AAName)namedTo.GetName()).AsString()), null),
                                                     node.GetExp());
                        node.SetExp(cast);
                        OutACastExp(cast);
                    }
                    else
                        errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                             "Unable to assign from type " + Util.TypeToString(from) +
                                                             " to type " + Util.TypeToString(to)));
                }
            }
            if (property == null)
                CheckAssignedOutParameters(
                    method != null ? method.GetFormals().Cast<AALocalDecl>() : constructor.GetFormals().Cast<AALocalDecl>(),
                    node.GetToken());
            base.OutAValueReturnStm(node);

            /*if (property != null)
            {
                AABlock lastBlock = Util.GetLastAncestor<AABlock>(node);
                if (lastBlock == property.GetGetter())
                    errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                         "The getter must return something of type " +
                                                         Util.TypeToString(property.GetType())));
            }
            else*/
        }


        public override void OutAVoidReturnStm(AVoidReturnStm node)
        {
            AMethodDecl method = Util.GetAncestor<AMethodDecl>(node);
            AConstructorDecl constructor = Util.GetAncestor<AConstructorDecl>(node);
            APropertyDecl property = Util.GetAncestor<APropertyDecl>(node);
            if (method != null && Util.TypeToString(method.GetReturnType()) != "void")
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                     "This function must return something of type " +
                                                     Util.TypeToString(method.GetReturnType())));
            }
            if (property != null)
            {
                AABlock lastBlock = Util.GetLastAncestor<AABlock>(node);
                if (lastBlock == property.GetGetter())
                    errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile,
                                                         "The getter must return something of type " +
                                                         Util.TypeToString(property.GetType())));
            }
            else if (!Util.HasAncestor<ADeconstructorDecl>(node))
                CheckAssignedOutParameters(
                    method != null ? method.GetFormals().Cast<AALocalDecl>() : constructor.GetFormals().Cast<AALocalDecl>(),
                    node.GetToken());
            base.OutAVoidReturnStm(node);
        }

        private void CheckAssignedOutParameters(IEnumerable<AALocalDecl> formals, Token token)
        {
            foreach (AALocalDecl formal in formals)
            {
                if (formal.GetOut() != null && !assignedToOutParams.Contains(formal))
                {
                    errors.Add(new ErrorCollection.Error(token ?? formal.GetName(), currentSourceFile, "Parameter marked as out is not assigned a value"));
                }
            }
        }

        public override void OutAConstructorDecl(AConstructorDecl node)
        {
            //Link to base constructor
            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            if (str != null && str.GetBase() != null)
            {
                AStructDecl baseStruct = data.StructTypeLinks[(ANamedType)str.GetBase()];
                List<AConstructorDecl> candidates = new List<AConstructorDecl>();
                foreach (AConstructorDecl baseConstructor in data.StructConstructors[baseStruct])
                {
                    //Visibility
                    if (baseConstructor.GetVisibilityModifier() is APrivateVisibilityModifier)
                        continue;

                    if (baseConstructor.GetFormals().Count == node.GetBaseArgs().Count)
                    {
                        //Check that parameters are assignable
                        bool add = true;
                        for (int i = 0; i < node.GetBaseArgs().Count; i++)
                        {
                            PType argType = data.ExpTypes[(PExp)node.GetBaseArgs()[i]];
                            AALocalDecl formal = (AALocalDecl)baseConstructor.GetFormals()[i];
                            PType formalType = formal.GetType();
                            if (formal.GetOut() != null && !Assignable(formalType, argType)
                                || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                            candidates.Add(baseConstructor);
                    }
                }
                if (candidates.Count == 0)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), "No matching base constructor found.", false,
                                                         new ErrorCollection.Error(baseStruct.GetName(), "Base struct")));
                    return;
                }
                if (candidates.Count > 1)
                {
                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                    foreach (AConstructorDecl candidate in candidates)
                    {
                        subErrors.Add(new ErrorCollection.Error(candidate.GetName(), "Candidate"));
                    }
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Base constructor call matched multiple constructors.", false,
                                                         subErrors.ToArray()));
                    return;
                }
                data.ConstructorBaseLinks[node] = candidates[0];
            }
            else if (node.GetBaseArgs().Count > 0)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), "No matching base constructor found."));
            }
            base.OutAConstructorDecl(node);
        }

       
        public static bool Assignable(PType from, PType to)
        {
            SharedData data = SharedData.LastCreated;
            string fromTypeString = Util.TypeToString(from);
            string toTypeString = Util.TypeToString(to);

            if (toTypeString == "fixed")
                return fromTypeString == "int" || fromTypeString == "byte" || fromTypeString == "fixed";
            if (toTypeString == "int")
                return fromTypeString == "int" || fromTypeString == "byte";
            if (toTypeString == "void" || fromTypeString == "void")
                return false;
            if (toTypeString == "byte")
                return fromTypeString == "byte" || fromTypeString == "int";
            //if (fromTypeString == "null" && Form1.inst.nullable.Any(s => s == toTypeString))
            //    return true;
            //Array - underlying type must be the same, and dimention must be 1
            if (to is AArrayTempType && from is AArrayTempType)
            {
                AArrayTempType aTo = (AArrayTempType)to;
                AArrayTempType aFrom = (AArrayTempType)from;
                if (!Assignable(aFrom.GetType(), aTo.GetType()))
                    return false;
                if (aTo.GetIntDim() == null || aFrom.GetIntDim() == null)
                    return false;
                return aTo.GetIntDim().Text == aFrom.GetIntDim().Text;
            }
            if ((/*to is ADynamicArrayType ||*/ to is AArrayTempType) && from is AArrayTempType)
            {
                to = to is ADynamicArrayType
                                 ? ((ADynamicArrayType)to).GetType()
                                 : ((AArrayTempType)to).GetType();
                from = ((AArrayTempType)from).GetType();
                return AssignableExact(from, to);
            }
            /*if (to is ADynamicArrayType && ((from is APointerType && ((APointerType)from).GetType() is AArrayTempType) || from is ADynamicArrayType))
            {
                from = from is ADynamicArrayType
                                 ? ((ADynamicArrayType)from).GetType()
                                 : ((AArrayTempType)((APointerType)from).GetType()).GetType();
                to = ((ADynamicArrayType)to).GetType();
                return AssignableExact(from, to);
            }*/
            //Structs - type must be the same
            if (to is ANamedType && from is ANamedType)
            {
                if (fromTypeString == "null" && GalaxyKeywords.NullablePrimitives.words.Any(p => p == toTypeString))
                    return true;
                if (GalaxyKeywords.Primitives.words.Any(p => p == toTypeString) || GalaxyKeywords.Primitives.words.Any(p => p == fromTypeString))
                {
                    /*switch (toTypeString)
                    {
                        case "int":
                            switch (fromTypeString)
                            {
                                case "byte":
                                    return true;
                            }
                            break;
                        case "fixed":
                            switch (fromTypeString)
                            {
                                case "byte":
                                case "int":
                                    return true;
                            }
                            break;
                    }*/
                    return fromTypeString == toTypeString;
                }
                ANamedType aTo = (ANamedType)to;
                ANamedType aFrom = (ANamedType)from;
                //Delegate types
                if (data.DelegateTypeLinks.ContainsKey(aTo) || data.DelegateTypeLinks.ContainsKey(aFrom))
                {
                    if (data.DelegateTypeLinks.ContainsKey(aTo) && data.DelegateTypeLinks.ContainsKey(aFrom))
                    {
                        AMethodDecl delegateFrom = data.DelegateTypeLinks[aFrom];
                        AMethodDecl delegateTo = data.DelegateTypeLinks[aTo];
                        if (delegateFrom.GetFormals().Count == delegateTo.GetFormals().Count)
                        {
                            bool assignable = Assignable(delegateFrom.GetReturnType(), delegateTo.GetReturnType()) ||
                                              delegateTo.GetReturnType() is AVoidType;
                            for (int i = 0; i < delegateTo.GetFormals().Count; i++)
                            {
                                PType argType = ((AALocalDecl)delegateFrom.GetFormals()[i]).GetType();
                                AALocalDecl formal = (AALocalDecl)delegateTo.GetFormals()[i];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                                {
                                    assignable = false;
                                    break;
                                }
                            }
                            return assignable;
                        }
                    }

                    if (fromTypeString == "null")
                        return true;
                    return false;
                }

                AStructDecl aToStruct = data.StructTypeLinks[aTo];
                AStructDecl aFromStruct = data.StructTypeLinks[aFrom];
                return Util.Extends(aToStruct, aFromStruct, data);
            }
            //Pointers
            if (to is APointerType && from is APointerType)
            {
                while (to is APointerType)
                {
                    if (!(from is APointerType))
                        return false;
                    to = ((APointerType)to).GetType();
                    from = ((APointerType)from).GetType();
                }
                /*if (to.GetType() != from.GetType())
                    return false;*/
                return AssignableExact(from, to);
            }
            if (to is APointerType && from is ANamedType && ((ANamedType)from).IsPrimitive("null"))
                return true;
            //If we have an array in comparison with something which is not of that type
            return false;
        }

        public static bool AssignableExact(PType from, PType to)
        {
            SharedData data = SharedData.LastCreated;
            string fromTypeString = Util.TypeToString(from);
            string toTypeString = Util.TypeToString(to);

            
            
            //Array - underlying type must be the same
            if (to is AArrayTempType && from is AArrayTempType)
            {
                AArrayTempType aTo = (AArrayTempType)to;
                AArrayTempType aFrom = (AArrayTempType)from;
                if (!Assignable(aFrom.GetType(), aTo.GetType()))
                    return false;
                if (aTo.GetIntDim() == null || aFrom.GetIntDim() == null)
                    return false;
                return aTo.GetIntDim().Text == aFrom.GetIntDim().Text;
            }
            if ((to is ADynamicArrayType || to is AArrayTempType) && (from is ADynamicArrayType || from is AArrayTempType))
            {
                to = to is ADynamicArrayType
                                 ? ((ADynamicArrayType)to).GetType()
                                 : ((AArrayTempType)to).GetType();

                from = from is ADynamicArrayType
                                 ? ((ADynamicArrayType)from).GetType()
                                 : ((AArrayTempType)from).GetType();
                return AssignableExact(from, to);
            }
            /*if (to is ADynamicArrayType && (from is ADynamicArrayType))
            {
                from = from is ADynamicArrayType
                                 ? ((ADynamicArrayType)from).GetType()
                                 : ((AArrayTempType)((APointerType)from).GetType()).GetType();
                to = ((ADynamicArrayType)to).GetType();
                return AssignableExact(from, to);
            }*/

            //Structs - type must be the same
            if (to is ANamedType && from is ANamedType)
            {
                if (fromTypeString == "null" && GalaxyKeywords.NullablePrimitives.words.Any(p => p == toTypeString))
                    return true;
                if (GalaxyKeywords.Primitives.words.Any(p => p == toTypeString) || GalaxyKeywords.Primitives.words.Any(p => p == fromTypeString))
                    return fromTypeString == toTypeString;
                ANamedType aTo = (ANamedType)to;
                ANamedType aFrom = (ANamedType)from;
                if (data.DelegateTypeLinks.ContainsKey(aTo) || data.DelegateTypeLinks.ContainsKey(aFrom))
                {
                    if (data.DelegateTypeLinks.ContainsKey(aTo) && data.DelegateTypeLinks.ContainsKey(aFrom))
                    {
                        AMethodDecl delegateFrom = data.DelegateTypeLinks[aFrom];
                        AMethodDecl delegateTo = data.DelegateTypeLinks[aTo];
                        if (delegateFrom.GetFormals().Count == delegateTo.GetFormals().Count)
                        {
                            bool assignable = Assignable(delegateFrom.GetReturnType(), delegateTo.GetReturnType()) ||
                                              delegateTo.GetReturnType() is AVoidType;
                            for (int i = 0; i < delegateTo.GetFormals().Count; i++)
                            {
                                PType argType = ((AALocalDecl)delegateFrom.GetFormals()[i]).GetType();
                                AALocalDecl formal = (AALocalDecl)delegateTo.GetFormals()[i];
                                PType formalType = formal.GetType();
                                if (formal.GetOut() != null && !Assignable(formalType, argType)
                                    || formal.GetRef() != null && !(Assignable(argType, formalType) && Assignable(formalType, argType))
                                    || formal.GetOut() == null && formal.GetRef() == null && !Assignable(argType, formalType))
                                {
                                    assignable = false;
                                    break;
                                }
                            }
                            return assignable;
                        }
                    }
                    if (fromTypeString == "null")
                        return true;
                    return false;
                }
                AStructDecl aToStruct = data.StructTypeLinks[aTo];
                AStructDecl aFromStruct = data.StructTypeLinks[aFrom];
                return aToStruct == aFromStruct;
            }
            //Pointers
            if (to is APointerType && from is APointerType)
            {
                while (to is APointerType)
                {
                    if (!(from is APointerType))
                        return false;
                    to = ((APointerType)to).GetType();
                    from = ((APointerType)from).GetType();
                }
                /*if (to.GetType() != from.GetType())
                    return false;*/
                return AssignableExact(from, to);
            }
            //If we have an array in comparison with something which is not of that type
            return false;
        }
    }
}
