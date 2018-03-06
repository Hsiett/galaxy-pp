using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class TransformMethodDecls : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        private SharedData data
        {
            get { return finalTrans.data; }
        }

        public TransformMethodDecls(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        public override void InAASourceFile(AASourceFile node)
        {
            namespacePrefix = "";
            /*if (node.GetNamespace() != null)
                namespacePrefix = node.GetNamespace().Text + "_";*/
        }

        private string namespacePrefix = "";

        List<AStructDecl> parsedStructs = new List<AStructDecl>();
        public override void CaseAStructDecl(AStructDecl node)
        {
            if (parsedStructs.Contains(node))
                return;
            parsedStructs.Add(node);
            if (node.GetBase() != null)
            {
                AStructDecl baseStruct = finalTrans.data.StructTypeLinks[(ANamedType) node.GetBase()];
                if (!parsedStructs.Contains(baseStruct))
                    CaseAStructDecl(baseStruct);
            }
            base.CaseAStructDecl(node);
        }

        public override void InAStructDecl(AStructDecl node)
        {
            node.GetName().Text = namespacePrefix + node.GetName().Text;
        }

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            node.GetName().Text = namespacePrefix + node.GetName().Text;
        }

        /*public override void OutAAProgram(AAProgram node)
        {
            if (multipleEntryCandidates.Count > 0)
            {
                finalTrans.errors.Add(new ErrorCollection.Error(multipleEntryCandidates[0], "Found multiple candidates for the main entry", multipleEntryCandidates.ToArray()));
            }
        }*/


        private List<AMethodDecl> structMethods = new List<AMethodDecl>();
        private Dictionary<AMethodDecl, AStructDecl> OldParentStruct = new Dictionary<AMethodDecl, AStructDecl>();
        private AALocalDecl structFormal;
        //private List<ErrorCollection.Error> multipleEntryCandidates = new List<ErrorCollection.Error>();
        public override void CaseAMethodDecl(AMethodDecl node)
        {
            //Done in a previous iteration
            /*if (node.GetName().Text == "InitMap" && node.GetFormals().Count == 0)
            {
                if (finalTrans.multipleMainEntries)
                {
                    multipleEntryCandidates.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node.GetName()), "Candidate"));
                }
                else if (finalTrans.mainEntry != null)
                {
                    multipleEntryCandidates.Add(new ErrorCollection.Error(finalTrans.mainEntry.GetName(), Util.GetAncestor<AASourceFile>(finalTrans.mainEntry.GetName()), "Candidate"));
                    multipleEntryCandidates.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node.GetName()), "Candidate"));
                    //finalTrans.errors.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node), "Found multiple candidates for a main entry", true));
                    finalTrans.multipleMainEntries = true;
                    finalTrans.mainEntry = null;
                }
                else
                    finalTrans.mainEntry = node;
            }*/

            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            if (str != null)
            {
                if (node.GetStatic() == null)
                    structMethods.Add(node);
                //Move the method outside the struct
                str.RemoveChild(node.Parent());
                AASourceFile file = (AASourceFile)str.Parent();
                int i = file.GetDecl().IndexOf(str);
                file.GetDecl().Insert(i/* + 1*/, node);
                node.GetName().Text = GetUniqueStructMethodName(str.GetName().Text + "_" + node.GetName().Text);

                if (node.GetStatic() == null)
                {
                    //Add the struct as a parameter
                    PType structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                    finalTrans.data.StructTypeLinks[(ANamedType) structType] = str;
                    if (str.GetClassToken() != null)
                    {
                        structType = new APointerType(new TStar("*"), structType);
                    }
                    structFormal = new AALocalDecl(new APublicVisibilityModifier(), null,
                                                   str.GetClassToken() == null ? new TRef("ref") : null, null, null,
                                                   structType,
                                                   new TIdentifier("currentStruct", node.GetName().Line,
                                                                   node.GetName().Pos), null);
                    node.GetFormals().Add(structFormal);
                    data.Locals[(AABlock) node.GetBlock()].Add(structFormal);
                }
                else
                    node.SetStatic(null);
                finalTrans.data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, node));
                if (node.GetStatic() == null)
                    OldParentStruct[node] = str;
                //Fix refferences to other struct stuff);
                base.CaseAMethodDecl(node);
                //Will visit later, since it's added after the struct
                //base.CaseAMethodDecl(node);
                //if (str.GetLocals().Count == 0)
                //    str.Parent().RemoveChild(str);
                return;
            }
            AEnrichmentDecl enrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            if (enrichment != null)
            {
                if (node.GetStatic() == null)
                    structMethods.Add(node);
                //Move the method outside the struct
                enrichment.RemoveChild(node);
                AASourceFile file = (AASourceFile)enrichment.Parent();
                int i = file.GetDecl().IndexOf(enrichment);
                file.GetDecl().Insert(i/* + 1*/, node);
                node.GetName().Text = GetUniqueStructMethodName(Util.TypeToIdentifierString(enrichment.GetType()) + "_" + node.GetName().Text);

                if (node.GetStatic() == null)
                {
                    //Add the struct as a parameter
                    PType structType = Util.MakeClone(enrichment.GetType(), finalTrans.data);
                    structFormal = new AALocalDecl(new APublicVisibilityModifier(), null, new TRef("ref"), null, null,
                                                   structType,
                                                   new TIdentifier("currentEnrichment", node.GetName().Line,
                                                                   node.GetName().Pos), null);
                    node.GetFormals().Add(structFormal);
                }
                finalTrans.data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, node));
                //Fix refferences to other struct stuff);
                base.CaseAMethodDecl(node);
                //Will visit later, since it's added after the struct
                //base.CaseAMethodDecl(node);
                //if (str.GetLocals().Count == 0)
                //    str.Parent().RemoveChild(str);
                return;
            }
            //Build a list of overloads
            List<AMethodDecl> overloads = new List<AMethodDecl>();
            List<string> prefixMatches = new List<string>();

            foreach (SharedData.DeclItem<AMethodDecl> declItem in finalTrans.data.Methods)
            {
                if (!Util.IsSameNamespace(declItem.Decl, node))
                    continue;
                if (declItem.Decl.GetName().Text == node.GetName().Text)
                    overloads.Add(declItem.Decl);
                if (declItem.Decl.GetName().Text.StartsWith(node.GetName().Text + "O"))
                    prefixMatches.Add(declItem.Decl.GetName().Text);
            }


            

            foreach (AMethodDecl method in finalTrans.data.Libraries.Methods)
            {
                if (method.GetBlock() != null || method.GetNative() != null)
                {
                    if (method.GetName().Text == node.GetName().Text)
                        overloads.Add(method);
                    if (method.GetName().Text.StartsWith(node.GetName().Text + "O"))
                        prefixMatches.Add(method.GetName().Text);
                }
            }

            //Add fields
            foreach (SharedData.DeclItem<AFieldDecl> declItem in finalTrans.data.Fields)
            {
                if (declItem.Decl.GetName().Text.StartsWith(node.GetName().Text + "O"))
                    prefixMatches.Add(declItem.Decl.GetName().Text);
            }

            foreach (AFieldDecl field in finalTrans.data.Libraries.Fields)
            {
                if (field.GetName().Text.StartsWith(node.GetName().Text + "O"))
                    prefixMatches.Add(field.GetName().Text);
            }

            //Dont want to hit another method by appending O#
            string postfix = "";
            while (true)
            {
                postfix += "O";
                if (prefixMatches.Any(text => text.StartsWith(node.GetName().Text + postfix)))
                {
                    continue;
                }
                break;
            }

            if (overloads.Count > 1)
            {
                int i = 0;
                foreach (AMethodDecl method in overloads)
                {
                    if (node == finalTrans.mainEntry || (node.GetTrigger() != null && finalTrans.data.HasUnknownTrigger))
                        continue;
                    i++;
                    method.GetName().Text += postfix + i;
                }
            }

            if (node != finalTrans.mainEntry && (node.GetTrigger() == null || !finalTrans.data.HasUnknownTrigger))
                node.GetName().Text = namespacePrefix + node.GetName().Text;
            

            base.CaseAMethodDecl(node);
        }

        /*private class IsThisOnLeftSide : DepthFirstAdapter
        {
            private PType type;
            private SharedData data;
            public bool IsAssignedTo;
            private List<AMethodDecl> investigatedMethods = new List<AMethodDecl>();

            public IsThisOnLeftSide(PType type, SharedData data)
            {
                this.type = type;
                this.data = data;
            }

            //Check assignments, method invocations and nonstatic method invocations.

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                investigatedMethods.Add(node);
            }

            public override void CaseAThisLvalue(AThisLvalue node)
            {
                if (IsAssignedTo)
                    return;

                Node iParent = GetClosestNodeOfType(node, typeof (AAssignmentExp),
                                                    typeof (ASimpleInvokeExp),
                                                    typeof (ANonstaticInvokeExp),
                                                    typeof (AAsyncInvokeStm),
                                                    typeof (ASyncInvokeExp),
                                                    typeof(AArrayLvalue),
                                                    typeof(APointerLvalue),
                                                    typeof(APropertyLvalue),
                                                    typeof(AStructLvalue));
                if (iParent == null)
                    return;

                if (iParent is AAssignmentExp)
                {
                    AAssignmentExp aParent = (AAssignmentExp) iParent;
                    if (Util.IsAncestor(node, aParent.GetLvalue()))
                    {
                        IsAssignedTo = true;
                    }
                    return;
                }
                if (iParent is ASimpleInvokeExp)
                {
                    ASimpleInvokeExp aParent = (ASimpleInvokeExp) iParent;
                    AMethodDecl method = data.SimpleMethodLinks[aParent];
                    if (investigatedMethods.Contains(method))
                        return;

                    if (Util.IsAncestor(node, aParent.GetLvalue()))
                    {
                        IsAssignedTo = true;
                    }
                    return;
                }
            }

            private Node GetClosestNodeOfType(Node node, params Type[] types)
            {
                if (node == null)
                    return null;
                if (types.Contains(node.GetType()))
                    return node;
                return GetClosestNodeOfType(node.Parent(), types);
            }
        }*/

        public override void CaseADeconstructorDecl(ADeconstructorDecl node)
        {
            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            AEnrichmentDecl enrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            AMethodDecl replacer = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                   node.GetName(), new ArrayList(), node.GetBlock());
            replacer.GetName().Text += "_Deconstructor";

            //Move the method outside the struct
            AASourceFile file = Util.GetAncestor<AASourceFile>(node);
            if (str != null)
                str.RemoveChild(node.Parent());
            /*else
                enrichment.RemoveChild(node);*/
            int i = file.GetDecl().IndexOf(str ?? (PDecl)enrichment);
            file.GetDecl().Insert(i/* + 1*/, replacer);
            //Add the struct as a parameter
            PType type;
            if (str != null)
            {
                ANamedType structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                finalTrans.data.StructTypeLinks[structType] = str;
                type = structType;
            }
            else
            {
                type = Util.MakeClone(enrichment.GetType(), finalTrans.data);
            }
            finalTrans.data.DeconstructorMap[node] = replacer;
            AALocalDecl structFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new APointerType(new TStar("*"), type), new TIdentifier("currentStruct", replacer.GetName().Line, replacer.GetName().Pos), null);
            replacer.GetFormals().Add(structFormal);
            finalTrans.data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, replacer));

            //Call base deconstructor before each return
            if (str != null && str.GetBase() != null)
            {
                AStructDecl baseStruct = data.StructTypeLinks[(ANamedType) str.GetBase()];
                if (data.StructDeconstructor.ContainsKey(baseStruct))
                {
                    baseStruct.Apply(this);
                    replacer.Apply(new CallDeconstructors(baseStruct, structFormal, data));
                    /*AMethodDecl baseDeconstructor = data.DeconstructorMap[data.StructDeconstructor[baseStruct]];


                    ALocalLvalue structFormalRef = new ALocalLvalue(new TIdentifier("currentStruct"));
                    ALvalueExp structFormalRefExp = new ALvalueExp(structFormalRef);
                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("baseDeconstructor"),
                                                                   new ArrayList() {structFormalRefExp});
                    AABlock block = (AABlock) replacer.GetBlock();
                    block.GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), invoke));

                    data.LocalLinks[structFormalRef] = structFormal;
                    data.SimpleMethodLinks[invoke] = baseDeconstructor;
                    data.LvalueTypes[structFormalRef] = data.ExpTypes[structFormalRefExp] = structFormal.GetType();
                    data.ExpTypes[invoke] = baseDeconstructor.GetReturnType();*/
                }
            }
            this.structFormal = structFormal;
            base.CaseAMethodDecl(replacer);
        }

        private class CallDeconstructors : DepthFirstAdapter
        {
            private AStructDecl baseStruct;
            private AALocalDecl structFormal;
            private SharedData data;

            public CallDeconstructors(AStructDecl baseStruct, AALocalDecl structFormal, SharedData data)
            {
                this.baseStruct = baseStruct;
                this.structFormal = structFormal;
                this.data = data;
            }

            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                AMethodDecl baseDeconstructor = data.DeconstructorMap[data.StructDeconstructor[baseStruct]];


                ALocalLvalue structFormalRef = new ALocalLvalue(new TIdentifier("currentStruct"));
                ALvalueExp structFormalRefExp = new ALvalueExp(structFormalRef);
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("baseDeconstructor"),
                                                               new ArrayList() { structFormalRefExp });
                AABlock block = (AABlock)node.Parent();
                block.GetStatements().Insert(block.GetStatements().IndexOf(node), new AExpStm(new TSemicolon(";"), invoke));

                data.LocalLinks[structFormalRef] = structFormal;
                data.SimpleMethodLinks[invoke] = baseDeconstructor;
                data.LvalueTypes[structFormalRef] = data.ExpTypes[structFormalRefExp] = structFormal.GetType();
                data.ExpTypes[invoke] = baseDeconstructor.GetReturnType();
            }
        }

        public override void CaseAConstructorDecl(AConstructorDecl node)
        {
            AStructDecl str = Util.GetAncestor<AStructDecl>(node);
            AEnrichmentDecl enrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            AMethodDecl replacer = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                   node.GetName(), new ArrayList(), node.GetBlock());
            replacer.GetName().Text += "_Constructor";
            while (node.GetFormals().Count > 0)
            {
                replacer.GetFormals().Add(node.GetFormals()[0]);
            }

            //Move the method outside the struct
            AASourceFile file = Util.GetAncestor<AASourceFile>(node);
            if (str != null)
                str.RemoveChild(node.Parent());
            else
                enrichment.RemoveChild(node);
            int i = file.GetDecl().IndexOf(str ?? (PDecl)enrichment);
            file.GetDecl().Insert(i/* + 1*/, replacer);
            //Add the struct as a parameter
            PType type;
            if (str != null)
            {
                ANamedType structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                finalTrans.data.StructTypeLinks[structType] = str;
                type = structType;
            }
            else
            {
                type = Util.MakeClone(enrichment.GetType(), finalTrans.data);
            }
            finalTrans.data.ConstructorMap[node] = replacer;
            structFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new APointerType(new TStar("*"), type), new TIdentifier("currentStruct", replacer.GetName().Line, replacer.GetName().Pos), null);
            replacer.GetFormals().Add(structFormal);
            finalTrans.data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, replacer));

            //Add return stm
            replacer.SetReturnType(new APointerType(new TStar("*"), Util.MakeClone(type, data)));
            replacer.Apply(new TransformConstructorReturns(structFormal, data));

            //Insert call to base constructor););
            if (finalTrans.data.ConstructorBaseLinks.ContainsKey(node))
            {
                AMethodDecl baseConstructor = finalTrans.data.ConstructorMap[finalTrans.data.ConstructorBaseLinks[node]];
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier(baseConstructor.GetName().Text), new ArrayList());
                while (node.GetBaseArgs().Count > 0)
                {
                    invoke.GetArgs().Add(node.GetBaseArgs()[0]);
                }
                AThisLvalue thisLvalue1 = new AThisLvalue(new TThis("this"));
                ALvalueExp thisExp1 = new ALvalueExp(thisLvalue1);
                invoke.GetArgs().Add(thisExp1);

                AThisLvalue thisLvalue2 = new AThisLvalue(new TThis("this"));

                AAssignmentExp assignExp = new AAssignmentExp(new TAssign("="), thisLvalue2, invoke);

                ANamedType structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                finalTrans.data.StructTypeLinks[structType] = str;

                finalTrans.data.LvalueTypes[thisLvalue1] =
                    finalTrans.data.LvalueTypes[thisLvalue2] =
                    finalTrans.data.ExpTypes[thisExp1] =
                    finalTrans.data.ExpTypes[assignExp] =
                    finalTrans.data.ExpTypes[invoke] = new APointerType(new TStar("*"), structType);

                //finalTrans.data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                finalTrans.data.SimpleMethodLinks[invoke] = baseConstructor;

                ((AABlock)replacer.GetBlock()).GetStatements().Insert(0, new AExpStm(new TSemicolon(";"), assignExp));

                //Inline if base and current are two different kinds of pointer types (int/string)
                AStructDecl baseStruct = null;
                AConstructorDecl baseC = finalTrans.data.ConstructorBaseLinks[node];
                foreach (KeyValuePair<AStructDecl, List<AConstructorDecl>> pair in finalTrans.data.StructConstructors)
                {
                    bool found = false;
                    foreach (AConstructorDecl decl in pair.Value)
                    {
                        if (baseC == decl)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        baseStruct = pair.Key;
                        break;
                    }
                }
                if ((str.GetIntDim() == null) != (baseStruct.GetIntDim() == null))
                {
                    //For the inilining, change the type to the type of the caller
                    AALocalDecl lastFormal = baseConstructor.GetFormals().OfType<AALocalDecl>().Last();
                    lastFormal.SetRef(new TRef("ref"));
                    APointerType oldType = (APointerType) lastFormal.GetType();

                    structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                    finalTrans.data.StructTypeLinks[structType] = str;

                    APointerType newType = new APointerType(new TStar("*"), structType);
                    lastFormal.SetType(newType);

                    foreach (
                        ALocalLvalue lvalue in
                            data.LocalLinks.Where(pair => pair.Value == lastFormal).Select(pair => pair.Key))
                    {
                        data.LvalueTypes[lvalue] = newType;
                        if (lvalue.Parent() is ALvalueExp)
                        {
                            data.ExpTypes[(PExp) lvalue.Parent()] = newType;
                            if (lvalue.Parent().Parent() is APointerLvalue)
                                data.LvalueTypes[(PLvalue) lvalue.Parent().Parent()] = newType.GetType();
                        }
                    }

                    FixInlineMethods.Inline(invoke, finalTrans);
                    lastFormal.SetRef(null);
                    foreach (
                        ALocalLvalue lvalue in
                            data.LocalLinks.Where(pair => pair.Value == lastFormal).Select(pair => pair.Key))
                    {
                        data.LvalueTypes[lvalue] = oldType;
                        if (lvalue.Parent() is ALvalueExp)
                        {
                            data.ExpTypes[(PExp) lvalue.Parent()] = oldType;
                            if (lvalue.Parent().Parent() is APointerLvalue)
                                data.LvalueTypes[(PLvalue) lvalue.Parent().Parent()] = oldType.GetType();
                        }
                    }

                    lastFormal.SetType(oldType);
                }

                //Inline it instead, Since the pointer implementations might not be the same (int vs string)

                /*AMethodDecl baseConstructor = finalTrans.data.ConstructorMap[finalTrans.data.ConstructorBaseLinks[node]];

                AABlock localsBlock = new AABlock(new ArrayList(), new TRBrace("}"));
                ABlockStm cloneBlock = new ABlockStm(new TLBrace("{"), (PBlock) baseConstructor.GetBlock().Clone());
                Dictionary<AALocalDecl, PLvalue> localMap = new Dictionary<AALocalDecl, PLvalue>();
                for (int argNr = 0; argNr < baseConstructor.GetFormals().Count; argNr++)
                {
                    AALocalDecl formal = (AALocalDecl) baseConstructor.GetFormals()[i];
                    PExp arg;
                    if (i < baseConstructor.GetFormals().Count - 1)
                        arg = (PExp)node.GetBaseArgs()[i];
                    else
                    {
                        AThisLvalue thisLvalue = new AThisLvalue(new TThis("this"));
                        ALvalueExp thisExp = new ALvalueExp(thisLvalue);

                        ANamedType structType = new ANamedType(new TIdentifier(str.GetName().Text), null);
                        finalTrans.data.StructTypeLinks[structType] = str;

                        finalTrans.data.LvalueTypes[thisLvalue] =
                            finalTrans.data.ExpTypes[thisExp] = new APointerType(new TStar("*"), structType);

                        arg = thisExp;
                    }

                    if (formal.GetRef() != null || formal.GetOut() != null)
                    {
                        //Use same variable
                        localMap[formal] = ((ALvalueExp) arg).GetLvalue();
                    }
                    else
                    {
                        //Make a new variable
                        AALocalDecl newLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                               Util.MakeClone(formal.GetType(), finalTrans.data),
                                                               new TIdentifier(formal.GetName().Text),
                                                               Util.MakeClone(arg, data));

                        ALocalLvalue newLocalRef = new ALocalLvalue(new TIdentifier(newLocal.GetName().Text));

                        localMap[formal] = newLocalRef;
                        data.LvalueTypes[newLocalRef] = newLocal.GetType();
                        data.LocalLinks[newLocalRef] = newLocal;

                        localsBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), newLocal));
                    }

                }

                CloneMethod cloner = new CloneMethod(finalTrans.data, localMap, cloneBlock);
                baseConstructor.GetBlock().Apply(cloner);
                
                ((AABlock)cloneBlock.GetBlock()).GetStatements().Insert(0, new ABlockStm(new TLBrace("{"), localsBlock));
                ((AABlock)node.GetBlock()).GetStatements().Insert(0, cloneBlock);*/
            }

            //Fix refferences to other struct stuff);
            base.CaseAMethodDecl(replacer);

            //Add functionality to refference the current struct in a constructor
            //Want to do it as a pointer type, since the constructer can only be called for pointer types
        }

        private class TransformConstructorReturns : DepthFirstAdapter
        {
            private AALocalDecl param;
            private SharedData data;

            public TransformConstructorReturns(AALocalDecl param, SharedData data)
            {
                this.param = param;
                this.data = data;
            }

            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                ALocalLvalue paramRef = new ALocalLvalue(new TIdentifier("paramRef"));
                ALvalueExp paramRefExp = new ALvalueExp(paramRef);
                node.ReplaceBy(new AValueReturnStm(node.GetToken(), paramRefExp));
                data.LocalLinks[paramRef] = param;
                data.LvalueTypes[paramRef] = data.ExpTypes[paramRefExp] = param.GetType();
            }
        }

        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
            if (structMethods.Contains(decl))
            {//The target is a struct method that has been moved out
                if (node.GetArgs().Count < decl.GetFormals().Count && Util.HasAncestor<AStructDecl>(node))
                {//If this is the case, we Must be inside the same struct as the target. - Not with enheritance
                    ALocalLvalue local = new ALocalLvalue(new TIdentifier("tempName"));
                    ALvalueExp exp = new ALvalueExp(local);
                    finalTrans.data.LvalueTypes[local] =
                        finalTrans.data.ExpTypes[exp] = structFormal.GetType();
                    finalTrans.data.LocalLinks[local] = structFormal;

                    //If we're calling from class to struct, we must depointer it
                    AStructDecl currentStruct =
                        finalTrans.data.StructMethods.First(
                            pair => pair.Value.Contains(Util.GetAncestor<AMethodDecl>(node))).Key;
                    AStructDecl baseStruct =
                        finalTrans.data.StructMethods.First(
                            pair => pair.Value.Contains(decl)).Key;
                    if (currentStruct.GetClassToken() != baseStruct.GetClassToken()) //It's not possible to call from struct to class
                    {
                        APointerLvalue pointerLvalue = new APointerLvalue(new TStar("*"), exp);
                        exp = new ALvalueExp(pointerLvalue);

                        finalTrans.data.LvalueTypes[pointerLvalue] =
                            finalTrans.data.ExpTypes[exp] = ((APointerType)structFormal.GetType()).GetType();
                    }

                    node.GetArgs().Add(exp);
                }
            }
            else if (Util.GetAncestor<AStructDecl>(decl) != null && OldParentStruct.ContainsKey(Util.GetAncestor<AMethodDecl>(node)))
            {//The target is a struct method that hasn't been moved out
                if (Util.GetAncestor<AStructDecl>(decl) == OldParentStruct[Util.GetAncestor<AMethodDecl>(node)] && decl.GetStatic() == null)
                {//We have an internal struct call. Expect to have one too many args
                    if (node.GetArgs().Count == decl.GetFormals().Count)
                    {
                        ALocalLvalue local = new ALocalLvalue(new TIdentifier("tempName"));
                        ALvalueExp exp = new ALvalueExp(local);
                        finalTrans.data.LvalueTypes[local] =
                            finalTrans.data.ExpTypes[exp] = structFormal.GetType();
                        finalTrans.data.LocalLinks[local] = structFormal;

                        //If we're calling from class to struct, we must depointer it
                        AStructDecl currentStruct =
                            finalTrans.data.StructMethods.First(
                                pair => pair.Value.Contains(Util.GetAncestor<AMethodDecl>(node))).Key;
                        AStructDecl baseStruct =
                            finalTrans.data.StructMethods.First(
                                pair => pair.Value.Contains(decl)).Key;
                        if (currentStruct.GetClassToken() != baseStruct.GetClassToken()) //It's not possible to call from struct to class
                        {
                            APointerLvalue pointerLvalue = new APointerLvalue(new TStar("*"), exp);
                            exp = new ALvalueExp(pointerLvalue);

                            finalTrans.data.LvalueTypes[pointerLvalue] =
                                finalTrans.data.ExpTypes[exp] = ((APointerType)structFormal.GetType()).GetType();
                        }

                        node.GetArgs().Add(exp);
                    }
                }
            }
            base.CaseASimpleInvokeExp(node);
        }

        private Dictionary<AMethodDecl, List<ANonstaticInvokeExp>> dynamicStructMethods = new Dictionary<AMethodDecl, List<ANonstaticInvokeExp>>();
        public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
        {
            PExp reciever = node.GetReceiver();
            PType type = finalTrans.data.ExpTypes[reciever];

            //If the reciever is not a var, put it in a new var.
            if (!(reciever is ALvalueExp))
            {
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(type, data), new TIdentifier("nonstaticInvokeVar"), reciever);
                ALocalLvalue localRef = new ALocalLvalue(new TIdentifier("nonstaticInvokeVar"));
                ALvalueExp localRefExp = new ALvalueExp(localRef);
                node.SetReceiver(localRefExp);
                PStm pStm = Util.GetAncestor<PStm>(node);
                AABlock pBlock = (AABlock) pStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), localDecl));
                reciever = localRefExp;

                data.LvalueTypes[localRef] =
                    data.ExpTypes[localRefExp] = type;
                data.LocalLinks[localRef] = localDecl;

                localDecl.Apply(this);
            }

            if (type is ANamedType && finalTrans.data.StructTypeLinks.ContainsKey((ANamedType)type))
            {
                //ANamedType type = (ANamedType) finalTrans.data.ExpTypes[reciever];
                if (finalTrans.data.StructTypeLinks[(ANamedType) type].GetClassToken() != null)
                {
                    //Pass the pointer
                    ALvalueExp lvalueExp = (ALvalueExp) reciever;
                    APointerLvalue pointerLvalue = (APointerLvalue) lvalueExp.GetLvalue();
                    reciever = pointerLvalue.GetBase();
                }

                AMethodDecl method = finalTrans.data.StructMethodLinks[node];
                ASimpleInvokeExp simpleInvoke = new ASimpleInvokeExp();
                simpleInvoke.SetName(node.GetName());
                PExp[] exps = new PExp[node.GetArgs().Count];
                node.GetArgs().CopyTo(exps, 0);
                foreach (PExp exp in exps)
                {
                    simpleInvoke.GetArgs().Add(exp);
                }
                simpleInvoke.GetArgs().Add(reciever);
                node.ReplaceBy(simpleInvoke);
                finalTrans.data.SimpleMethodLinks[simpleInvoke] = method;
                finalTrans.data.StructMethodLinks.Remove(node);
                finalTrans.data.ExpTypes[simpleInvoke] = method.GetReturnType();
                finalTrans.data.ExpTypes.Remove(node);
                simpleInvoke.Apply(this);
            }
            else
            {//Enrichment
                /*AEnrichmentDecl enrichment = finalTrans.data.EnrichmentTypeLinks[type];

                foreach (AEnrichmentDecl enrichmentDecl in finalTrans.data.Enrichments)
                {
                    if (Util.IsVisible(node, enrichmentDecl) &&
                        Util.IsDeclVisible(enrichmentDecl, Util.GetAncestor<AASourceFile>(node)) &&
                        Util.TypesEqual(type, enrichmentDecl.GetType(), finalTrans.data))
                    {
                        enrichment = enrichmentDecl;
                        break;
                    }
                }
                if (enrichment == null)
                {
                    finalTrans.errors.Add(new ErrorCollection.Error(node.GetName(), "TransFormMethodDecls.NonStaticInvoke: Expected enrichment - this is a bug. It should have been caught earlier"));
                    throw new ParserException(node.GetName(), "");
                }*/
                AMethodDecl method = finalTrans.data.StructMethodLinks[node];
                ASimpleInvokeExp simpleInvoke = new ASimpleInvokeExp();
                simpleInvoke.SetName(node.GetName());
                PExp[] exps = new PExp[node.GetArgs().Count];
                node.GetArgs().CopyTo(exps, 0);
                foreach (PExp exp in exps)
                {
                    simpleInvoke.GetArgs().Add(exp);
                }
                simpleInvoke.GetArgs().Add(reciever);
                node.ReplaceBy(simpleInvoke);
                finalTrans.data.SimpleMethodLinks[simpleInvoke] = method;
                finalTrans.data.StructMethodLinks.Remove(node);
                finalTrans.data.ExpTypes[simpleInvoke] = method.GetReturnType();
                finalTrans.data.ExpTypes.Remove(node);
                simpleInvoke.Apply(this);

            }
        }

        public override void CaseAThisLvalue(AThisLvalue node)
        {
            //Replace with <structFormal>
            ALocalLvalue parameterRefference = new ALocalLvalue(new TIdentifier("tempName"));
            finalTrans.data.LocalLinks[parameterRefference] = structFormal;
            finalTrans.data.LvalueTypes[parameterRefference] = structFormal.GetType();
            node.ReplaceBy(parameterRefference);
            base.CaseAThisLvalue(node);
        }

        public override void OutAStructDecl(AStructDecl node)
        {
            if (node.GetClassToken() != null && node.GetIntDim() == null)
                node.Parent().RemoveChild(node);
            base.OutAStructDecl(node);
        }

        public override void CaseAStructFieldLvalue(AStructFieldLvalue node)
        {
            //replace strField1
            //with <structFormal>.strField1
            AStructDecl str = finalTrans.data.StructTypeLinks[(ANamedType)structFormal.GetType()];
            ALocalLvalue parameterRefference = new ALocalLvalue(new TIdentifier("tempName"));
            finalTrans.data.LocalLinks[parameterRefference] = structFormal;
            finalTrans.data.LvalueTypes[parameterRefference] = structFormal.GetType();
            ALvalueExp exp = new ALvalueExp(parameterRefference);
            finalTrans.data.ExpTypes[exp] = structFormal.GetType();
            AStructLvalue replacer = new AStructLvalue(exp, new ADotDotType(new TDot(".")), node.GetName());
            foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
            {
                if (structField.GetName().Text == replacer.GetName().Text)
                {
                    finalTrans.data.StructFieldLinks[replacer] = structField;
                    finalTrans.data.LvalueTypes[replacer] = structField.GetType();
                    break;
                }
            }
            foreach (APropertyDecl property in finalTrans.data.StructProperties[str])
            {
                if (property.GetName().Text == replacer.GetName().Text)
                {
                    finalTrans.data.StructPropertyLinks[replacer] = property;
                    finalTrans.data.LvalueTypes[replacer] = property.GetType();
                    break;
                }
            }
            node.ReplaceBy(replacer);
        }


        private string GetUniqueStructMethodName(string baseName)
        {
            string name = baseName;
            List<string> prefixMatches = new List<string>();
            //Global methods (that was not struct methods)
            prefixMatches.AddRange(from method in finalTrans.data.Methods
                                   where method.Decl.GetName().Text.StartsWith(name) && !structMethods.Contains(method.Decl)
                                   select method.Decl.GetName().Text);
            //Add lib methods
            foreach (AMethodDecl method in finalTrans.data.Libraries.Methods)
            {
                if (method.GetName().Text.StartsWith(name))
                    prefixMatches.Add(method.GetName().Text);
            }

            //Add fields
            foreach (SharedData.DeclItem<AFieldDecl> declItem in finalTrans.data.Fields)
            {
                if (declItem.Decl.GetName().Text.StartsWith(name))
                    prefixMatches.Add(declItem.Decl.GetName().Text);
            }

            foreach (AFieldDecl field in finalTrans.data.Libraries.Fields)
            {
                if (field.GetName().Text.StartsWith(name))
                    prefixMatches.Add(field.GetName().Text);
            }

            int nr = 1;
            while (true)
            {
                nr++;
                if (prefixMatches.Any(text => text == name))
                {
                    name = baseName + nr;
                    continue;
                }
                break;
            }
            return name;
        }

        public override void OutAEnrichmentDecl(AEnrichmentDecl node)
        {
            node.Parent().RemoveChild(node);
        }
    }
}
