using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.NotGenerated;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class BulkCopyFixup : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public static void Parse(AAProgram ast, FinalTransformations finalTrans)
        {
            BulkCopyFixup bulkCopyFixup = new BulkCopyFixup(finalTrans);
            ast.Apply(new Phase0(bulkCopyFixup));
            ast.Apply(new Phase1(bulkCopyFixup));
            ast.Apply(new Phase2(bulkCopyFixup));
            ast.Apply(new Phase3(bulkCopyFixup));
            ast.Apply(new Phase4(bulkCopyFixup));
        }

        private BulkCopyFixup(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }


        /*
            0: Make a list of used parameters
               convert nonstatic method invocations to simple invocations
             
            1: fix method parameters and returns

            2: move bulk copy method invocations up

            3: fix bulk copy method calls
               fix bulk variable assignments
          
            4: Convert return types to void
        */
        private List<List<AALocalDecl>> UsedParameters = new List<List<AALocalDecl>>();
        private Dictionary<AMethodDecl, List<AALocalDecl>> Parameters = new Dictionary<AMethodDecl, List<AALocalDecl>>();
        private List<ASimpleInvokeExp> quickReturnCalls = new List<ASimpleInvokeExp>();

        private class Phase0 : DepthFirstAdapter
        {
            private BulkCopyFixup bulkCopyFixup;

            public Phase0(BulkCopyFixup bulkCopyFixup)
            {
                this.bulkCopyFixup = bulkCopyFixup;
            }

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                //Register this unless parent is an lvalueExp, and then a structlvalu
                AALocalDecl decl = bulkCopyFixup.finalTrans.data.LocalLinks[node];
                RegiseterUse(node, new List<AALocalDecl> { decl });
            }

            private void RegiseterUse(PLvalue lvalue, List<AALocalDecl> tail)
            {
                if (tail == null) tail = new List<AALocalDecl>();
                if (lvalue.Parent().Parent() is AStructLvalue)
                {
                    AStructLvalue parent = (AStructLvalue)lvalue.Parent().Parent();
                    tail.Add(bulkCopyFixup.finalTrans.data.StructFieldLinks[parent]);
                    RegiseterUse(parent, tail);
                    return;
                }
                bulkCopyFixup.UsedParameters.Add(tail);
            }

            public override void OutAAProgram(AAProgram node)
            {
                //Clean the list
                for (int i = 0; i < bulkCopyFixup.UsedParameters.Count; i++)
                {
                    for (int j = i + 1; j < bulkCopyFixup.UsedParameters.Count; j++)
                    {
                        List<AALocalDecl> list1 = bulkCopyFixup.UsedParameters[i];
                        List<AALocalDecl> list2 = bulkCopyFixup.UsedParameters[j];
                        bool distinct = false;
                        for (int k = 0; k < Math.Min(list1.Count, list2.Count); k++)
                        {
                            if (list1[k] != list2[k])
                            {
                                distinct = true;
                                break;
                            }
                        }
                        if (!distinct)
                        {
                            if (list1.Count > list2.Count)
                            {
                                bulkCopyFixup.UsedParameters.RemoveAt(i);
                                j = i;
                            }
                            else
                            {
                                bulkCopyFixup.UsedParameters.RemoveAt(j);
                                j--;
                            }
                        }
                    }
                }
            }
        }

        //Parameters and returns
        private class Phase1 : DepthFirstAdapter
        {
            private BulkCopyFixup bulkCopyFixup;

            public Phase1(BulkCopyFixup bulkCopyFixup)
            {
                this.bulkCopyFixup = bulkCopyFixup;
            }

            public override void OutAMethodDecl(AMethodDecl node)
            {
                int argNr = 0;
                AALocalDecl[] formals = new AALocalDecl[node.GetFormals().Count];
                node.GetFormals().CopyTo(formals, 0);
                bulkCopyFixup.Parameters.Add(node, new List<AALocalDecl>(formals));
                foreach (AALocalDecl formal in formals)
                {
                    PType type = formal.GetType();
                    if (Util.IsBulkCopy(type)/* type is AArrayTempType ||
                        (type is ANamedType &&
                         !GalaxyKeywords.Primitives.words.Contains(((ANamedType)type).GetName().Text))*/)
                    {
                        //Move invocation up and fetch data table stuff
                        AABlock block = new AABlock();
                        block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), formal));
                        

                        //If it's out only, there is no input
                        if (formal.GetOut() == null)
                        {
                            ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(formal.GetName().Text));
                            bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = type;
                            bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = formal;

                            bulkCopyFixup.MakeParameterGetStatements(block, lvalue, type,
                                                                     "Galaxy++/Parameter[" + argNr + "]",
                                                                     new List<AALocalDecl>() {formal});
                        }

                        ABlockStm blockStm = new ABlockStm(new TLBrace("{"), block);

                        ((AABlock)node.GetBlock()).GetStatements().Insert(0, blockStm);

                    }
                    else if (formal.GetOut() != null)
                    {
                        //Don't pass out parameters
                        AABlock block = (AABlock) node.GetBlock();
                        block.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), formal));
                    }
                    argNr++;
                }
                return;
            }

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                PType type = bulkCopyFixup.finalTrans.data.ExpTypes[node.GetExp()];
                if (Util.IsBulkCopy(type))
                {
                    //If the child is a method decl, the data is already in the data table. no need to fetch anything
                    AVoidReturnStm newReturn = new AVoidReturnStm(node.GetToken());
                    if (node.GetExp() is ASimpleInvokeExp)
                    {
                        bulkCopyFixup.quickReturnCalls.Add((ASimpleInvokeExp)node.GetExp());
                        AExpStm expStm = new AExpStm(new TSemicolon(";"), node.GetExp());
                        AABlock block = (AABlock)node.Parent();
                        block.GetStatements().Insert(block.GetStatements().IndexOf(node), expStm);
                        node.ReplaceBy(newReturn);
                        newReturn.Apply(this);
                        return;
                    }
                    else
                    {
                        //Move the expression out, and assign to data tables
                        node.GetExp().Apply(new MoveMethodDeclsOut("bulkCopyVar", bulkCopyFixup.finalTrans.data));

                        AABlock block = new AABlock();
                        bulkCopyFixup.MakeDataTableSetStatements(block, node.GetExp(), type, "Galaxy++/Returner");
                        block.GetStatements().Add(newReturn);
                        ABlockStm blockStm = new ABlockStm(new TLBrace("{"), block);
                        node.ReplaceBy(blockStm);
                        blockStm.Apply(this);
                        newReturn.Apply(this);
                        return;
                    }
                }
                ReturnParameters(node);
            }

            List<AVoidReturnStm> visitedReturns = new List<AVoidReturnStm>();
            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                if (!visitedReturns.Contains(node))
                {
                    visitedReturns.Add(node);
                    ReturnParameters(node);
                }
                base.CaseAVoidReturnStm(node);
            }

            private void ReturnParameters(PStm returnStm)
            {
                int argNr = 0;
                AMethodDecl methodDecl = Util.GetAncestor<AMethodDecl>(returnStm);
                AABlock block = new AABlock();
                foreach (AALocalDecl formal in methodDecl.GetFormals())
                {
                    PType type = formal.GetType();
                    /*if (type is AArrayTempType ||
                        (type is ANamedType &&
                         !GalaxyKeywords.Primitives.words.Contains(((ANamedType)type).GetName().Text)))*/
                    if (formal.GetRef() != null || formal.GetOut() != null)
                    {
                        //Move invocation up and fetch data table stuff
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(formal.GetName().Text));
                        bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = type;
                        bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = formal;
                        ALvalueExp exp = new ALvalueExp(lvalue);
                        bulkCopyFixup.finalTrans.data.ExpTypes[exp] = type;

                        bulkCopyFixup.MakeParameterSetStatements(block, exp, type, "Galaxy++/Parameter[" + argNr + "]", new List<AALocalDecl>() { formal });

                    }
                    argNr++;
                }
                ABlockStm blockStm = new ABlockStm(new TLBrace("{"), block);
                AABlock pBlock = (AABlock)returnStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(returnStm), blockStm);
            }



        }

        //Move bulk copy to assignments
        private class Phase2 : DepthFirstAdapter
        {
            private BulkCopyFixup bulkCopyFixup;

            public Phase2(BulkCopyFixup bulkCopyFixup)
            {
                this.bulkCopyFixup = bulkCopyFixup;
            }


            private bool Process(PExp node)
            {

                if (((node.Parent() is PExp || node.Parent() is AALocalDecl) &&
                    !(node.Parent() is AAssignmentExp) &&
                    (Util.IsBulkCopy(bulkCopyFixup.finalTrans.data.ExpTypes[node])))
                    
                    //Sync invokes must always be in its own statement
                    || (node is ASyncInvokeExp && !(node.Parent() is AExpStm || node.Parent() is AAssignmentExp)))
                {
                    PType type = bulkCopyFixup.finalTrans.data.ExpTypes[node];
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    AABlock block = (AABlock)pStm.Parent();
                    if (node.Parent() is AALocalDecl)
                    {//Move assignment under local decl
                        AALocalDecl localDecl = (AALocalDecl)node.Parent();
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(localDecl.GetName().Text));
                        AAssignmentExp assignExp = new AAssignmentExp(new TAssign("="), lvalue, node);
                        bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = type;
                        bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = localDecl;
                        bulkCopyFixup.finalTrans.data.ExpTypes[assignExp] = type;
                        AExpStm assignStm = new AExpStm(new TSemicolon(";"), assignExp);

                        block.GetStatements().Insert(block.GetStatements().IndexOf(pStm) + 1, assignStm);
                        assignStm.Apply(this);
                        return true;
                    }
                    else
                    {//Move assignment above statement
                        /*if (Util.GetAncestor<AMethodDecl>(node).GetName().Text == "Help_Init")
                            node = node;*/

                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                        ALvalueExp lvalueExp = new ALvalueExp(lvalue);
                        node.ReplaceBy(lvalueExp);
                        AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(type, bulkCopyFixup.finalTrans.data), new TIdentifier("bulkCopyVar"), null);
                        ALocalDeclStm localDeclStm = new ALocalDeclStm(new TSemicolon(";"), localDecl);
                        bulkCopyFixup.finalTrans.data.GeneratedVariables.Add(localDecl);


                        bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = type;
                        bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = localDecl;
                        bulkCopyFixup.finalTrans.data.ExpTypes[lvalueExp] = type;
                        lvalue = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                        bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = type;
                        bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = localDecl;
                        AAssignmentExp assignmentExp = new AAssignmentExp(new TAssign("="), lvalue, node);
                        bulkCopyFixup.finalTrans.data.ExpTypes[assignmentExp] = type;
                        AExpStm assignmentStm = new AExpStm(new TSemicolon(";"), assignmentExp);

                        AABlock newBlock = new AABlock();
                        newBlock.GetStatements().Add(localDeclStm);
                        newBlock.GetStatements().Add(assignmentStm);
                        ABlockStm newBlockStm = new ABlockStm(new TLBrace("{"), newBlock);


                        block.GetStatements().Insert(block.GetStatements().IndexOf(pStm), newBlockStm);
                        newBlockStm.Apply(this);
                        return true;
                    }
                }
                return false;
            }

            public override void CaseAALocalDecl(AALocalDecl node)
            {
                if (node.Parent() is ALocalDeclStm && node.GetInit() != null && !Process(node.GetInit()))
                    return;
                base.CaseAALocalDecl(node);
            }

            public override void CaseASyncInvokeExp(ASyncInvokeExp node)
            {
                Process(node);
                base.CaseASyncInvokeExp(node);
            }

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (!Process(node))
                {//Not moved out. move it out if it has bulk copy parameteres
                    if (!(node.Parent() is AAssignmentExp || node.Parent() is AExpStm))
                    {
                        bool bulkCopyParameter = false;
                        AMethodDecl decl = bulkCopyFixup.finalTrans.data.SimpleMethodLinks[node];
                        if (Util.GetAncestor<AASourceFile>(decl) != null)//If it is null, we have an included method
                        {
                            foreach (AALocalDecl formal in bulkCopyFixup.Parameters[decl])
                            {
                                //if (Util.IsBulkCopy(formal.GetType()))
                                if ((Util.IsBulkCopy(formal.GetType()) && formal.GetOut() == null) || formal.GetRef() != null)
                                {
                                    bulkCopyParameter = true;
                                    break;
                                }
                            }
                            if (bulkCopyParameter)
                            {
                                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                                ALvalueExp exp = new ALvalueExp(lvalue);
                                node.ReplaceBy(exp);
                                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                        Util.MakeClone(decl.GetReturnType(),
                                                                                       bulkCopyFixup.finalTrans.data),
                                                                        new TIdentifier("bulkCopyVar"), node);
                                ALocalDeclStm localDeclStm = new ALocalDeclStm(new TSemicolon(";"), localDecl);
                                bulkCopyFixup.finalTrans.data.ExpTypes[exp] =
                                    bulkCopyFixup.finalTrans.data.LvalueTypes[lvalue] = decl.GetReturnType();
                                bulkCopyFixup.finalTrans.data.LocalLinks[lvalue] = localDecl;
                                PStm parentStm = Util.GetAncestor<PStm>(exp);
                                AABlock pBlock = (AABlock) parentStm.Parent();
                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(parentStm), localDeclStm);
                                localDeclStm.Apply(this);
                                return;
                            }
                        }
                    }
                    base.CaseASimpleInvokeExp(node);
                }
                return;
            }

        }

        //Local bulk copy and method calls
        private class Phase3 : DepthFirstAdapter
        {
            private BulkCopyFixup bulkCopyFixup;

            private SharedData data { get { return bulkCopyFixup.finalTrans.data; } }

            public Phase3(BulkCopyFixup bulkCopyFixup)
            {
                this.bulkCopyFixup = bulkCopyFixup;
            }

            private List<PExp> preCopiedParams = new List<PExp>();

            public override void InAAProgram(AAProgram node)
            {
                //Make invocations
                foreach (KeyValuePair<AMethodDecl, List<InvokeStm>> invokePair in bulkCopyFixup.finalTrans.data.Invokes)
                {
                    AMethodDecl invokedMethod = invokePair.Key;
                    AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(invokedMethod);

                    //Make trigger variable
                    ASimpleInvokeExp triggerInitExp = new ASimpleInvokeExp();
                    triggerInitExp.SetName(new TIdentifier("TriggerCreate"));
                    AStringConstExp arg = new AStringConstExp(new TStringLiteral("InvokeTriggerMethod_" + invokedMethod.GetName().Text));
                    triggerInitExp.GetArgs().Add(arg);
                    data.SimpleMethodLinks[triggerInitExp] =
                        data.Libraries.Methods.First(method => method.GetName().Text == "TriggerCreate");
                    data.ExpTypes[triggerInitExp] = new ANamedType(new TIdentifier("trigger"), null);
                    data.ExpTypes[arg] = new ANamedType(new TIdentifier("string"), null);
                    AFieldDecl triggerDecl = new AFieldDecl(new APublicVisibilityModifier(), null, null, new ANamedType(new TIdentifier("trigger"), null),
                                                            new TIdentifier("InvokeTrigger_" +
                                                                            invokedMethod.GetName().Text),
                                                            triggerInitExp);
                    data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(sourceFile, triggerDecl));
                    //Make trigger method
                    AABlock triggerMethodBlock = new AABlock();
                    AALocalDecl ranAsyncParam = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                new ANamedType(
                                                                    new TIdentifier("bool"),
                                                                    null),
                                                                new TIdentifier("ranAsync"),
                                                                null);
                    AMethodDecl triggerMethod = new AMethodDecl(new APublicVisibilityModifier(), new TTrigger("trigger"), null, null, null, null,
                                                                new ANamedType(new TIdentifier("bool"), null),
                                                                new TIdentifier("InvokeTriggerMethod_" +
                                                                                invokedMethod.GetName().Text),
                                                                new ArrayList
                                                                    {
                                                                        ranAsyncParam,
                                                                        new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                        new ANamedType(
                                                                                            new TIdentifier("bool"),
                                                                                            null),
                                                                                        new TIdentifier("runActions"),
                                                                                        null)
                                                                    }, triggerMethodBlock);
                    data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, triggerMethod));
                    data.TriggerDeclarations[triggerMethod] = new List<TStringLiteral>{arg.GetStringLiteral()};
                    data.Locals[triggerMethodBlock] = new List<AALocalDecl>();

                    if (Options.Compiler.ObfuscateStrings)
                    {
                        int line = -data.ObfuscatedStrings.Count - 1;
                        AFieldDecl field = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const", line, 0),
                                                              new ANamedType(new TIdentifier("string", line, 1), null),
                                                              new TIdentifier("Galaxy_pp_stringO" +
                                                                              data.ObfuscatedStrings.Count), null);
                        bool newField = true;
                        foreach (AStringConstExp oldStringConstExp in data.ObfuscatedStrings.Keys)
                        {
                            if (arg.GetStringLiteral().Text == oldStringConstExp.GetStringLiteral().Text)
                            {
                                field = data.ObfuscatedStrings[oldStringConstExp];
                                newField = false;
                                break;
                            }
                        }
                        if (newField)
                        {
                            AASourceFile file = (AASourceFile) data.DeobfuscateMethod.Parent();
                            file.GetDecl().Insert(file.GetDecl().IndexOf(data.DeobfuscateMethod) + 1, field);
                            data.ObfuscationFields.Add(field);
                        }
                        data.ObfuscatedStrings.Add(arg, field);
                    }
                    sourceFile.GetDecl().Insert(sourceFile.GetDecl().IndexOf(invokedMethod), triggerMethod);
                    sourceFile.GetDecl().Insert(sourceFile.GetDecl().IndexOf(invokedMethod), triggerDecl);

                    //Fill triggerMethod
                    //Local varaiable declarations
                    List<AALocalDecl> locals = new List<AALocalDecl>();
                    List<AALocalDecl> preCompiedLocals = new List<AALocalDecl>();
                    foreach (AALocalDecl formal in bulkCopyFixup.Parameters[invokedMethod])
                    {
                        AALocalDecl local = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(formal.GetType(), data), new TIdentifier(formal.GetName().Text), null);
                        locals.Add(local);

                        //If bulk copy, no need to pull the param out and back in
                        if (Util.IsBulkCopy(local.GetType()) || formal.GetOut() != null)
                        {
                            preCompiedLocals.Add(local);
                            continue;
                        }
                        triggerMethodBlock.GetStatements().Add(new ALocalDeclStm(null, local));
                        data.Locals[triggerMethodBlock].Add(local);
                    }
                    //Returner
                    AALocalDecl returnerLocal = null;
                    if (!((invokedMethod.GetReturnType() is AVoidType) || Util.IsBulkCopy(invokedMethod.GetReturnType())))
                    {
                        returnerLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(invokedMethod.GetReturnType(), data), new TIdentifier("returner"), null);
                        triggerMethodBlock.GetStatements().Add(new ALocalDeclStm(null, returnerLocal));
                        data.Locals[triggerMethodBlock].Add(returnerLocal);
                    }
                    //Local variable initializers
                    for (int i = 0; i < locals.Count; i++)
                    {
                        AALocalDecl local = locals[i];
                        //If bulk copy, no need to pull the param out and back in
                        if (preCompiedLocals.Contains(local))
                            continue;

                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(local.GetName().Text));
                        data.LvalueTypes[lvalue] = local.GetType();
                        data.LocalLinks[lvalue] = local;
                        AABlock innerBlock = new AABlock();
                        /*bulkCopyFixup.MakeParameterGetStatements(innerBlock, lvalue, local.GetType(),
                                                                 "Galaxy++/Parameter[" + i + "]",
                                                                 new List<AALocalDecl>() { local });*/
                        bulkCopyFixup.MakeDataTableGetStatements(innerBlock, lvalue, local.GetType(), "Galaxy++/Parameter[" + i + "]");
                        triggerMethodBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), innerBlock));
                    }
                    //If it's an assync method call, call wait so that the original function can continue
                    {
                        PExp test;
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("ranAsync"));
                        test = new ALvalueExp(lvalue);
                        data.LocalLinks[lvalue] = ranAsyncParam;
                        data.ExpTypes[test] =
                            data.LvalueTypes[lvalue] = new ANamedType(new TIdentifier("bool"), null);

                        AABlock thenBranch = new AABlock();
                        AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral("0"));
                        AFieldLvalue fieldLink = new AFieldLvalue(new TIdentifier("c_timeGame"));
                        ALvalueExp fieldExp = new ALvalueExp(fieldLink);
                        ASimpleInvokeExp waitCall = new ASimpleInvokeExp(new TIdentifier("Wait"), new ArrayList{intConst, fieldExp});
                        data.FieldLinks[fieldLink] =
                            data.Libraries.Fields.First(field => field.GetName().Text == "c_timeGame");
                        data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                        data.ExpTypes[fieldExp] = data.LvalueTypes[fieldLink] = new ANamedType(new TIdentifier("int"), null);
                        data.SimpleMethodLinks[waitCall] =
                            data.Libraries.Methods.First(method => method.GetName().Text == "Wait");
                        thenBranch.GetStatements().Add(new AExpStm(new TSemicolon(";"),  waitCall));

                        AIfThenStm ifStm = new AIfThenStm(new TLParen("("), test, new ABlockStm(new TLBrace("{"), thenBranch));
                        triggerMethodBlock.GetStatements().Add(ifStm);
                    }

                    //Method call
                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier(invokedMethod.GetName().Text), new ArrayList());
                    data.ExpTypes[invoke] = invokedMethod.GetReturnType();
                    data.SimpleMethodLinks[invoke] = invokedMethod;
                    processedMethods.Add(invoke);
                    foreach (AALocalDecl local in locals)
                    {
                        //If bulk copy, no need to pull the param out and back in
                        if (preCompiedLocals.Contains(local))
                        {
                            /*ANullExp nullExp = new ANullExp();
                            invoke.GetArgs().Add(nullExp);
                            data.ExpTypes[nullExp] = new ANullType();
                            preCopiedParams.Add(nullExp);*/
                            continue;
                        }
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(local.GetName().Text));
                        ALvalueExp exp = new ALvalueExp(lvalue);
                        invoke.GetArgs().Add(exp);
                        data.LocalLinks[lvalue] = local;
                        data.LvalueTypes[lvalue] = data.ExpTypes[exp] = local.GetType();
                    }
                    if (returnerLocal == null)
                        triggerMethodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), invoke));
                    else
                    {
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(returnerLocal.GetName().Text));
                        AAssignmentExp assignExp = new AAssignmentExp(new TAssign("="), lvalue, invoke);
                        triggerMethodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignExp));
                        data.LocalLinks[lvalue] = returnerLocal;
                        data.LvalueTypes[lvalue] = returnerLocal.GetType();
                        data.ExpTypes[assignExp] = invokedMethod.GetReturnType();
                    }
                    //If this was an async call, remove everything in the datatable
                    {
                        PExp test;
                        AABlock thenBranch = new AABlock();
                        AABlock elseBranch = new AABlock();
                        //Test
                        {
                            ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("ranAsync"));
                            test = new ALvalueExp(lvalue);
                            data.LocalLinks[lvalue] = ranAsyncParam;
                            data.ExpTypes[test] =
                                data.LvalueTypes[lvalue] = new ANamedType(new TIdentifier("bool"), null);
                        }
                        //Then
                        {
                            for (int i = 0; i < bulkCopyFixup.Parameters[invokedMethod].Count; i++)
                            {
                                AALocalDecl param = bulkCopyFixup.Parameters[invokedMethod][i];
                                if (param.GetOut() != null || param.GetRef() != null)
                                {
                                    bulkCopyFixup.MakeParameterGetStatements(thenBranch, null, param.GetType(), "Galaxy++/Parameter[" + i + "]", new List<AALocalDecl>{param});
                                }
                            }
                        }
                        //Add return value if needed
                        if (returnerLocal != null)
                        {
                            ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(returnerLocal.GetName().Text));
                            ALvalueExp exp = new ALvalueExp(lvalue);
                            data.LocalLinks[lvalue] = returnerLocal;
                            data.LvalueTypes[lvalue] = returnerLocal.GetType();
                            data.ExpTypes[exp] = invokedMethod.GetReturnType();
                            bulkCopyFixup.MakeDataTableSetStatements(elseBranch, exp, invokedMethod.GetReturnType(),
                                                                     "Galaxy++/Returner");
                        }
                        //Organize the if so it doesnt have an empty block
                        if (thenBranch.GetStatements().Count > 0)
                        {
                            if (elseBranch.GetStatements().Count > 0)
                            {//if then else
                                AIfThenElseStm stm = new AIfThenElseStm(new TLParen("("), test, new ABlockStm(new TLBrace("{"), thenBranch), new ABlockStm(new TLBrace("{"), elseBranch));
                                triggerMethodBlock.GetStatements().Add(stm);
                            }
                            else
                            {//if then
                                AIfThenStm stm = new AIfThenStm(new TLParen("("), test, new ABlockStm(new TLBrace("{"), thenBranch));
                                triggerMethodBlock.GetStatements().Add(stm);
                            }
                        }
                        else
                        {
                            if (elseBranch.GetStatements().Count > 0)
                            {
                                test = new AUnopExp(new AComplementUnop(new TComplement("!")), test);
                                data.ExpTypes[test] = new ANamedType(new TIdentifier("bool"), null);
                                AIfThenStm stm = new AIfThenStm(new TLParen("("), test, new ABlockStm(new TLBrace("{"), elseBranch));
                                triggerMethodBlock.GetStatements().Add(stm);
                            }
                            //Else, add nothing
                        }
                    }
                    //Add return statement
                    {
                        ABooleanConstExp exp = new ABooleanConstExp(new ATrueBool());
                        triggerMethodBlock.GetStatements().Add(new AValueReturnStm(new TReturn("return"), exp));
                        data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
                    }




                    //Fix each method invocations
                    foreach (InvokeStm invokeStm in invokePair.Value)
                    {
                        PStm pStm = Util.GetAncestor<PStm>(invokeStm.Node);
                        AABlock bigBlock = new AABlock();
                        AABlock beforeBlock = new AABlock();
                        AABlock afterBlock = new AABlock();
                        //Get/Set parameters into datatable
                        for (int i = 0; i < invokeStm.Args.Count; i++)
                        {
                            PExp exp = (PExp)invokeStm.Args[i];
                            PType type = bulkCopyFixup.finalTrans.data.ExpTypes[exp];
                            AALocalDecl parameter = bulkCopyFixup.Parameters[invokedMethod][i];
                            //Non out bulk
                            if (Util.IsBulkCopy(type) && parameter.GetOut() == null)
                                exp.Apply(new MoveMethodDeclsOut("bulkCopyVar", bulkCopyFixup.finalTrans.data));

                            //bulk
                            if (Util.IsBulkCopy(type))
                                bulkCopyFixup.MakeParameterSetStatements(beforeBlock, exp, type, "Galaxy++/Parameter[" + i + "]", new List<AALocalDecl>() { parameter });
                            else
                                bulkCopyFixup.MakeDataTableSetStatements(beforeBlock, exp, type, "Galaxy++/Parameter[" + i + "]");

                            //ref or out
                            //Fetch parameters again);
                            if (!invokeStm.IsAsync && (parameter.GetOut() != null || parameter.GetRef() != null))
                                bulkCopyFixup.MakeParameterGetStatements(afterBlock, ((ALvalueExp)exp).GetLvalue(), type,
                                                                            "Galaxy++/Parameter[" + i + "]",
                                                                            new List<AALocalDecl>() { parameter });
                        }
                        //Get returned value
                        {
                            PType type = invokedMethod.GetReturnType();
                            if (!invokeStm.IsAsync && !(type is AVoidType))
                            {

                                if (invokeStm.Node.Parent() is AAssignmentExp)
                                {
                                    AAssignmentExp parentExp = (AAssignmentExp)invokeStm.Node.Parent();
                                    bulkCopyFixup.MakeDataTableGetStatements(afterBlock, parentExp.GetLvalue(), type,
                                                                                "Galaxy++/Returner");
                                }
                                else
                                {
                                    bulkCopyFixup.MakeDataTableGetStatements(afterBlock, null, type,
                                                                                "Galaxy++/Returner");
                                }
                            }
                        }

                        AFieldLvalue triggerVar = new AFieldLvalue(new TIdentifier(triggerDecl.GetName().Text));
                        ALvalueExp triggerVarExp = new ALvalueExp(triggerVar);
                        ABooleanConstExp runAsyncArg = new ABooleanConstExp(invokeStm.IsAsync ? (PBool)new ATrueBool() : new AFalseBool());
                        ABooleanConstExp waitForFinish = new ABooleanConstExp(invokeStm.IsAsync ? (PBool)new AFalseBool() : new ATrueBool());
                        ASimpleInvokeExp caller = new ASimpleInvokeExp(new TIdentifier("TriggerExecute"), new ArrayList {triggerVarExp, runAsyncArg, waitForFinish});
                        data.LvalueTypes[triggerVar] =
                            data.ExpTypes[triggerVarExp] = new ANamedType(new TIdentifier("trigger"), null);
                        data.FieldLinks[triggerVar] = triggerDecl;
                        data.ExpTypes[runAsyncArg] = data.ExpTypes[waitForFinish] = new ANamedType(new TIdentifier("bool"), null);
                        data.SimpleMethodLinks[caller] =
                            data.Libraries.Methods.First(method => method.GetName().Text == caller.GetName().Text);
                        processedMethods.Add(caller);

                        bigBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), beforeBlock));
                        bigBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), caller));
                        bigBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), afterBlock));

                        pStm.ReplaceBy(new ABlockStm(new TLBrace("{"), bigBlock));
                    }
                }

                base.InAAProgram(node);
            }

            private List<ASimpleInvokeExp> processedMethods = new List<ASimpleInvokeExp>();

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                if (!(node.GetExp() is ASimpleInvokeExp))
                {
                    PType type = bulkCopyFixup.finalTrans.data.ExpTypes[node];
                    PStm nodeStm = Util.GetAncestor<PStm>(node);
                    if (Util.IsBulkCopy(type))
                    {
                        //If the right side contains method calls, move them out into local variables so they are only executed once
                        node.Apply(new MoveMethodDeclsOut("bulkCopyVar", bulkCopyFixup.finalTrans.data));
                        if (type is AArrayTempType)
                        {
                            AArrayTempType aType = (AArrayTempType)type;

                            AABlock block = new AABlock();
                            for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                            {

                                ALvalueExp leftSideExp = new ALvalueExp(Util.MakeClone(node.GetLvalue(), bulkCopyFixup.finalTrans.data));
                                AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["),
                                                                         leftSideExp,
                                                                         new AIntConstExp(
                                                                             new TIntegerLiteral(i.ToString())));
                                AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["),
                                                                         Util.MakeClone(node.GetExp(), bulkCopyFixup.finalTrans.data),
                                                                         new AIntConstExp(
                                                                             new TIntegerLiteral(i.ToString())));
                                ALvalueExp rightSideExp = new ALvalueExp(rightSide);
                                AAssignmentExp exp = new AAssignmentExp(new TAssign("="), leftSide, rightSideExp);
                                AExpStm stm = new AExpStm(new TSemicolon(";"), exp);
                                block.GetStatements().Add(stm);

                                bulkCopyFixup.finalTrans.data.LvalueTypes[leftSide] = aType.GetType();
                                bulkCopyFixup.finalTrans.data.LvalueTypes[rightSide] = aType.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[leftSideExp] = aType.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[rightSideExp] = aType.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[exp] = aType.GetType();
                            }
                            ABlockStm blockStm = new ABlockStm(new TLBrace("{"), block);
                            nodeStm.ReplaceBy(blockStm);
                            blockStm.Apply(this);
                            return;
                        }
                        else//Type is ANamedType
                        {
                            ANamedType aType = (ANamedType)type;
                            AStructDecl str = bulkCopyFixup.finalTrans.data.StructTypeLinks[aType];
                            AABlock block = new AABlock();
                            foreach (AALocalDecl structField in bulkCopyFixup.finalTrans.data.StructFields[str])
                            {
                                ALvalueExp leftSideExp = new ALvalueExp(Util.MakeClone(node.GetLvalue(), bulkCopyFixup.finalTrans.data));
                                AStructLvalue leftSide = new AStructLvalue(leftSideExp, 
                                                                           new ADotDotType(new TDot(".")),
                                                                           new TIdentifier(structField.GetName().Text));
                                AStructLvalue rightSide = new AStructLvalue(Util.MakeClone(node.GetExp(), bulkCopyFixup.finalTrans.data),
                                                                           new ADotDotType(new TDot(".")),
                                                                           new TIdentifier(structField.GetName().Text));
                                ALvalueExp rightSideExp = new ALvalueExp(rightSide);
                                AAssignmentExp exp = new AAssignmentExp(new TAssign("="), leftSide, rightSideExp);
                                AExpStm stm = new AExpStm(new TSemicolon(";"), exp);
                                block.GetStatements().Add(stm);

                                bulkCopyFixup.finalTrans.data.LvalueTypes[leftSide] = structField.GetType();
                                bulkCopyFixup.finalTrans.data.LvalueTypes[rightSide] = structField.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[leftSideExp] = structField.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[rightSideExp] = structField.GetType();
                                bulkCopyFixup.finalTrans.data.ExpTypes[exp] = structField.GetType();
                                bulkCopyFixup.finalTrans.data.StructFieldLinks[leftSide] = structField;
                                bulkCopyFixup.finalTrans.data.StructFieldLinks[rightSide] = structField;
                            }
                            ABlockStm blockStm = new ABlockStm(new TLBrace("{"), block);
                            nodeStm.ReplaceBy(blockStm);
                            blockStm.Apply(this);
                            return;
                        }
                    }
                }
                base.CaseAAssignmentExp(node);
            }



            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (processedMethods.Contains(node)) return;
                processedMethods.Add(node);

                //Dont take library methods
                if (Util.GetAncestor<AASourceFile>(bulkCopyFixup.finalTrans.data.SimpleMethodLinks[node]) == null)
                    return;

                //1: Make and destroy bulk copy parameters
                //Bulk copy parameters is not method calls (but may contain method calls)
                AMethodDecl methodDecl = bulkCopyFixup.finalTrans.data.SimpleMethodLinks[node];

                int argNr = 0;
                AABlock beforeBlock = new AABlock();
                AABlock afterBlock = new AABlock();
                ABlockStm beforeBlockStm = new ABlockStm(new TLBrace("{"), beforeBlock);
                ABlockStm afterBlockStm = new ABlockStm(new TLBrace("{"), afterBlock);
                PExp[] args = new PExp[node.GetArgs().Count];
                node.GetArgs().CopyTo(args, 0);
                foreach (PExp exp in args)
                {
                    PType type = bulkCopyFixup.finalTrans.data.ExpTypes[exp];
                    //if (Util.IsBulkCopy(type))
                    {
                        AALocalDecl parameter = bulkCopyFixup.Parameters[methodDecl][argNr];
                        //Non out bulk
                        if (Util.IsBulkCopy(type) && parameter.GetOut() == null)
                            exp.Apply(new MoveMethodDeclsOut("bulkCopyVar", bulkCopyFixup.finalTrans.data));

                        //bulk
                        //Only copy what's needed
                        if (Util.IsBulkCopy(type))
                            bulkCopyFixup.MakeParameterSetStatements(beforeBlock, exp, type, "Galaxy++/Parameter[" + argNr + "]", new List<AALocalDecl>() { parameter });

                        //ref or out
                        //Fetch parameters again
                        if (parameter.GetOut() != null || parameter.GetRef() != null)
                            bulkCopyFixup.MakeParameterGetStatements(afterBlock, ((ALvalueExp)exp).GetLvalue(), type,
                                                                     "Galaxy++/Parameter[" + argNr + "]",
                                                                     new List<AALocalDecl>() { parameter });
                        argNr++;

                        if (Util.IsBulkCopy(type) || parameter.GetOut() != null)
                            node.RemoveChild(exp);
                    }
                }
                //2: Assign and destroy returned bulk copy (unless moved out before return)
                {
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    PType type = bulkCopyFixup.finalTrans.data.ExpTypes[node];
                    PStm methodCallStm;
                    if (Util.IsBulkCopy(type))
                    {
                        if (bulkCopyFixup.quickReturnCalls.Contains(node))
                        {
                            //Dont remove returns
                            methodCallStm = new AExpStm(new TSemicolon(";"), node);
                        }
                        else
                        {
                            if (node.Parent() is AAssignmentExp)
                            {
                                AAssignmentExp parentExp = (AAssignmentExp)node.Parent();
                                bulkCopyFixup.MakeDataTableGetStatements(afterBlock, parentExp.GetLvalue(), type, "Galaxy++/Returner");
                                methodCallStm = new AExpStm(new TSemicolon(";"), node);
                            }
                            else
                            {
                                bulkCopyFixup.MakeDataTableGetStatements(afterBlock, null, type, "Galaxy++/Returner");
                                methodCallStm = new AExpStm(new TSemicolon(";"), node);
                            }
                        }
                        AABlock mainBlock = new AABlock(new List<PStm>() { beforeBlockStm, methodCallStm, afterBlockStm }, new TRBrace("}"));
                        ABlockStm mainBlockStm = new ABlockStm(new TLBrace("{"), mainBlock);
                        pStm.ReplaceBy(mainBlockStm);

                        mainBlock.Apply(this);
                        return;
                    }
                    else if (beforeBlock.GetStatements().Count > 0 || afterBlock.GetStatements().Count > 0)
                    {
                        AABlock pBlock = (AABlock)pStm.Parent();
                        int index = pBlock.GetStatements().IndexOf(pStm);
                        if (beforeBlock.GetStatements().Count > 0)
                        {
                            pBlock.GetStatements().Insert(index, beforeBlockStm);
                            beforeBlock.Apply(this);
                        }
                        index = pBlock.GetStatements().IndexOf(pStm);
                        if (afterBlock.GetStatements().Count > 0)
                        {
                            pBlock.GetStatements().Insert(index + 1, afterBlockStm);
                            afterBlock.Apply(this);
                        }
                        return;
                    }
                }

            }

        }

        //Fix return types
        private class Phase4 : DepthFirstAdapter
        {
            private BulkCopyFixup bulkCopyFixup;

            public Phase4(BulkCopyFixup bulkCopyFixup)
            {
                this.bulkCopyFixup = bulkCopyFixup;
            }

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                if (Util.IsBulkCopy(node.GetReturnType()))
                {
                    PType newReturn = new AVoidType(new TVoid("void"));
                    node.SetReturnType(newReturn);
                }
            }
        }






        private void MakeParameterGetStatements(AABlock block, PLvalue leftSide, PType type, string key, List<AALocalDecl> list)
        {
            //If fetchType is a primitive, return the appropriate method
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                if (aType.IsPrimitive())
                {//Primitive
                    bool matchingUse = false;
                    foreach (List<AALocalDecl> match in UsedParameters)
                    {
                        bool isMatch = true;
                        for (int i = 0; i < Math.Min(list.Count, match.Count); i++)
                        {
                            if (list[i] != match[i])
                            {
                                isMatch = false;
                                break;
                            }
                        }
                        matchingUse |= isMatch;
                        if (matchingUse)
                            break;
                    }
                    if (matchingUse)
                    {
                        //Make data table fetch method
                        if (leftSide != null)
                        {
                            ASimpleInvokeExp invoke = MakeDataTableMethod(aType.AsString(), key);
                            AAssignmentExp assExp = new AAssignmentExp(new TAssign("="),
                                                                       Util.MakeClone(leftSide,
                                                                                      finalTrans.data),
                                                                       invoke);
                            finalTrans.data.ExpTypes[invoke] = type;
                            finalTrans.data.ExpTypes[assExp] = type;
                            //finalTrans.data.SimpleMethodLinks[invoke] = 
                            AExpStm stm = new AExpStm(new TSemicolon(";"), assExp);
                            block.GetStatements().Add(stm);
                        }
                        //Make data tbale clear method
                        {
                            ASimpleInvokeExp invoke = MakeDataTableClearMethod(key);
                            finalTrans.data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                            AExpStm stm = new AExpStm(new TSemicolon(";"), invoke);
                            block.GetStatements().Add(stm);
                        }
                    }
                    return;
                }
                else
                {//Struct
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
                    {
                        AStructLvalue newLeftSide = null;
                        if (leftSide != null)
                        {
                            PExp newLeftSideExp = new ALvalueExp(Util.MakeClone(leftSide, finalTrans.data));
                            newLeftSide = new AStructLvalue(newLeftSideExp,
                                                            new ADotDotType(new TDot(".")),
                                                            new TIdentifier(structField.GetName().Text));
                            finalTrans.data.ExpTypes[newLeftSideExp] = structField.GetType();
                            finalTrans.data.LvalueTypes[newLeftSide] = structField.GetType();
                            finalTrans.data.StructFieldLinks[newLeftSide] = structField;
                        }
                        List<AALocalDecl> newList = new List<AALocalDecl>();
                        newList.AddRange(list);
                        newList.Add(structField);
                        MakeParameterGetStatements(block, newLeftSide, structField.GetType(),
                                                   key + "." + structField.GetName().Text, newList);
                    }
                }
            }
            else
            {//Array
                AArrayTempType aType = (AArrayTempType)type;
                int dim = int.Parse(aType.GetIntDim().Text);

                for (int i = 0; i < dim; i++)
                {
                    PLvalue newLeftSide = null;
                    if (leftSide != null)
                    {
                        PExp newLeftSideExp = new ALvalueExp(Util.MakeClone(leftSide, finalTrans.data));
                        PExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                        newLeftSide = new AArrayLvalue(new TLBracket("["), newLeftSideExp, intConst);

                        finalTrans.data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                        finalTrans.data.ExpTypes[newLeftSideExp] = aType.GetType();
                        finalTrans.data.LvalueTypes[newLeftSide] = aType.GetType();
                    }
                    MakeParameterGetStatements(block, newLeftSide, aType.GetType(), key + "[" + i + "]", list);
                }
            }
        }

        private void MakeParameterSetStatements(AABlock block, PExp rightSide, PType type, string key, List<AALocalDecl> list)
        {
            if (type is ANamedType)
            {

                ANamedType aType = (ANamedType)type;
                if (aType.IsPrimitive("null"))
                    return;
                if (aType.IsPrimitive())
                {
                    //Primitive
                    bool matchingUse = false;
                    foreach (List<AALocalDecl> match in UsedParameters)
                    {
                        bool isMatch = true;
                        for (int i = 0; i < Math.Min(list.Count, match.Count); i++)
                        {
                            if (list[i] != match[i])
                            {
                                isMatch = false;
                                break;
                            }
                        }
                        matchingUse |= isMatch;
                        if (matchingUse)
                            break;
                    }
                    if (matchingUse)
                    {
                        //Make data table put method
                        ASimpleInvokeExp invoke = MakeDataTableMethod(aType.AsString(), key,
                                                                      rightSide);
                        finalTrans.data.ExpTypes[invoke] = type;
                        AExpStm stm = new AExpStm(new TSemicolon(";"), invoke);
                        block.GetStatements().Add(stm);
                        return;
                    }
                }
                else
                {
                    //Struct
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
                    {
                        AStructLvalue newRightSide = new AStructLvalue(Util.MakeClone(rightSide, finalTrans.data),
                                                                       new ADotDotType(new TDot(".")),
                                                                       new TIdentifier(
                                                                           structField.GetName().Text));
                        PExp newRightSideExp =
                            new ALvalueExp(newRightSide);
                        finalTrans.data.ExpTypes[newRightSideExp] = structField.GetType();
                        finalTrans.data.LvalueTypes[newRightSide] = structField.GetType();
                        finalTrans.data.StructFieldLinks[newRightSide] = structField;

                        List<AALocalDecl> newList = new List<AALocalDecl>();
                        newList.AddRange(list);
                        newList.Add(structField);
                        MakeParameterSetStatements(block, newRightSideExp, structField.GetType(),
                                                   key + "." + structField.GetName().Text, newList);
                    }
                }
            }
            else //type is arrayType
            {
                AArrayTempType aType = (AArrayTempType)type;
                int dim = int.Parse(aType.GetIntDim().Text);

                for (int i = 0; i < dim; i++)
                {
                    PExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                    PLvalue newRightSide = new AArrayLvalue(new TLBracket("["), Util.MakeClone(rightSide, finalTrans.data), intConst);
                    PExp newRightSideExp = new ALvalueExp(newRightSide);

                    finalTrans.data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                    finalTrans.data.ExpTypes[newRightSideExp] = aType.GetType();
                    finalTrans.data.LvalueTypes[newRightSide] = aType.GetType();

                    MakeParameterSetStatements(block, newRightSideExp, aType.GetType(), key + "[" + i + "]", list);
                }
            }
        }




        /*private void MakeParameterSetStatements(AABlock block, PLvalue rightSide, PType type, string key, List<AALocalDecl> list)
        {
            if (type is ANamedType)
            {

                ANamedType aType = (ANamedType)type;
                if (GalaxyKeywords.Primitives.words.Contains(aType.GetName().Text))
                {
                    //Primitive
                    bool matchingUse = false;
                    foreach (List<AALocalDecl> match in UsedParameters)
                    {
                        bool isMatch = true;
                        for (int i = 0; i < Math.Min(list.Count, match.Count); i++)
                        {
                            if (list[i] != match[i])
                            {
                                isMatch = false;
                                break;
                            }
                        }
                        matchingUse |= isMatch;
                        if (matchingUse)
                            break;
                    }
                    if (matchingUse)
                    {
                        //Make data table put method
                        PExp rightSideExp = new ALvalueExp(MakeClone(rightSide, finalTrans.data));
                        finalTrans.data.ExpTypes[rightSideExp] =
                            finalTrans.data.LvalueTypes[rightSide];
                        ASimpleInvokeExp invoke = MakeDataTableMethod(aType.GetName().Text, key,
                                                                      rightSideExp);
                        finalTrans.data.ExpTypes[invoke] = type;
                        AExpStm stm = new AExpStm(new TSemicolon(";"), invoke);
                        block.GetStatements().Add(stm);
                        return;
                    }
                }
                else
                {
                    //Struct
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
                    {
                        PExp newRightSideExp =
                            new ALvalueExp(MakeClone(rightSide, finalTrans.data));
                        AStructLvalue newRightSide = new AStructLvalue(newRightSideExp,
                                                                       new TIdentifier(
                                                                           structField.GetName().Text));
                        finalTrans.data.ExpTypes[newRightSideExp] =
                            finalTrans.data.LvalueTypes[rightSide];
                        finalTrans.data.LvalueTypes[newRightSide] = structField.GetType();
                        finalTrans.data.StructFieldLinks[newRightSide] = structField;

                        list.Add(structField);
                        MakeParameterSetStatements(block, newRightSide, structField.GetType(),
                                                   key + "." + structField.GetName().Text, list);
                    }
                }
            }
            else //type is arrayType
            {
                AArrayTempType aType = (AArrayTempType)type;
                int dim = int.Parse(aType.GetIntDim().Text);

                for (int i = 0; i < dim; i++)
                {
                    PExp newRightSideExp = new ALvalueExp(MakeClone(rightSide, finalTrans.data));
                    PExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                    PLvalue newRightSide = new AArrayLvalue(new TLBracket("["), newRightSideExp, intConst);

                    finalTrans.data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"));
                    finalTrans.data.ExpTypes[newRightSideExp] = aType.GetType();
                    finalTrans.data.LvalueTypes[newRightSide] = aType.GetType();

                    MakeParameterSetStatements(block, newRightSide, aType.GetType(), key + "[" + i + "]", list);
                }
            }
        }*/

        private void MakeDataTableSetStatements(AABlock block, PExp rightSide, PType type, string key)
        {
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                if (aType.IsPrimitive())
                {//Primitive
                    //Make data table put method
                    ASimpleInvokeExp invoke = MakeDataTableMethod(aType.AsString(), key, Util.MakeClone(rightSide, finalTrans.data));
                    finalTrans.data.ExpTypes[invoke] = type;
                    AExpStm stm = new AExpStm(new TSemicolon(";"), invoke);
                    block.GetStatements().Add(stm);
                    return;
                }
                else
                {//Struct
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
                    {
                        AStructLvalue newRightSide = new AStructLvalue(Util.MakeClone(rightSide, finalTrans.data),
                                                                new ADotDotType(new TDot(".")),
                                                                new TIdentifier(structField.GetName().Text));
                        PExp newRightSideExp = new ALvalueExp(newRightSide);
                        finalTrans.data.ExpTypes[newRightSideExp] = structField.GetType();
                        finalTrans.data.LvalueTypes[newRightSide] = structField.GetType();
                        finalTrans.data.StructFieldLinks[newRightSide] = structField;

                        MakeDataTableSetStatements(block, newRightSideExp, structField.GetType(),
                                                   key + "." + structField.GetName().Text);
                    }
                }
            }
            else //type is arrayType
            {
                AArrayTempType aType = (AArrayTempType)type;
                int dim = int.Parse(aType.GetIntDim().Text);

                for (int i = 0; i < dim; i++)
                {
                    PExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                    PLvalue newRightSide = new AArrayLvalue(new TLBracket("["), Util.MakeClone(rightSide, finalTrans.data), intConst);
                    PExp newRightSideExp = new ALvalueExp(newRightSide);

                    finalTrans.data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                    finalTrans.data.ExpTypes[newRightSideExp] = aType.GetType();
                    finalTrans.data.LvalueTypes[newRightSide] = aType.GetType();

                    MakeDataTableSetStatements(block, newRightSideExp, aType.GetType(), key + "[" + i + "]");
                }
            }
        }

        private void MakeDataTableGetStatements(AABlock block, PLvalue leftSide, PType type, string key)
        {
            //If fetchType is a primitive, return the appropriate method
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                if (aType.IsPrimitive())
                {//Primitive
                    ASimpleInvokeExp invoke;
                    AExpStm stm;
                    //Make data table fetch method
                    if (leftSide != null)
                    {
                        invoke = MakeDataTableMethod(aType.AsString(), key);
                        AAssignmentExp assExp = new AAssignmentExp(new TAssign("="),
                                                                   Util.MakeClone(leftSide, finalTrans.data), invoke);
                        finalTrans.data.ExpTypes[invoke] = type;
                        finalTrans.data.ExpTypes[assExp] = type;
                        stm = new AExpStm(new TSemicolon(";"), assExp);
                        block.GetStatements().Add(stm);
                    }
                    //Make data tbale clear method
                    invoke = MakeDataTableClearMethod(key);
                    finalTrans.data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                    stm = new AExpStm(new TSemicolon(";"), invoke);
                    block.GetStatements().Add(stm);
                    return;
                }
                else
                {//Struct
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl structField in finalTrans.data.StructFields[str])
                    {
                        PExp newLeftSideExp = new ALvalueExp(Util.MakeClone(leftSide, finalTrans.data));
                        AStructLvalue newLeftSide = new AStructLvalue(newLeftSideExp,
                                                                new ADotDotType(new TDot(".")),
                                                                new TIdentifier(structField.GetName().Text));
                        finalTrans.data.ExpTypes[newLeftSideExp] = structField.GetType();
                        finalTrans.data.LvalueTypes[newLeftSide] = structField.GetType();
                        finalTrans.data.StructFieldLinks[newLeftSide] = structField;

                        MakeDataTableGetStatements(block, leftSide == null ? null : newLeftSide, structField.GetType(),
                                                   key + "." + structField.GetName().Text);
                    }
                }
            }
            else
            {//Array
                AArrayTempType aType = (AArrayTempType)type;
                int dim = int.Parse(aType.GetIntDim().Text);

                for (int i = 0; i < dim; i++)
                {
                    PExp newLeftSideExp = new ALvalueExp(Util.MakeClone(leftSide, finalTrans.data));
                    PExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                    PLvalue newLeftSide = new AArrayLvalue(new TLBracket("["), newLeftSideExp, intConst);

                    finalTrans.data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                    finalTrans.data.ExpTypes[newLeftSideExp] = aType.GetType();
                    finalTrans.data.LvalueTypes[newLeftSide] = aType.GetType();

                    MakeDataTableGetStatements(block, leftSide == null ? null : newLeftSide, aType.GetType(), key + "[" + i + "]");
                }
            }
        }


        public ASimpleInvokeExp MakeDataTableMethod(string type, string key, PExp value = null)
        {
            type = type[0].ToString().ToUpper() + type.Substring(1);
            ASimpleInvokeExp method = new ASimpleInvokeExp();
            method.SetName(new TIdentifier("DataTable" + (value == null ? "Get" : "Set") +  Util.Capitalize(type)));

            PExp exp = new ABooleanConstExp(new ATrueBool());
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
            method.GetArgs().Add(exp);

            exp = new AStringConstExp(new TStringLiteral("\"" + key + "\""));
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("string"), null);
            method.GetArgs().Add(exp);

            if (value != null)
                method.GetArgs().Add(value);

            finalTrans.data.ExpTypes[method] = new ANamedType(new TIdentifier(type.ToLower()), null);
            finalTrans.data.SimpleMethodLinks[method] =
                finalTrans.data.Libraries.Methods.Find(
                    libMethod => libMethod.GetName().Text == method.GetName().Text);

            return method;
        }

        public ASimpleInvokeExp MakeDataTableClearMethod(string key)
        {
            ASimpleInvokeExp method = new ASimpleInvokeExp();
            method.SetName(new TIdentifier("DataTableValueRemove"));

            PExp exp = new ABooleanConstExp(new ATrueBool());
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
            method.GetArgs().Add(exp);

            exp = new AStringConstExp(new TStringLiteral("\"" + key + "\""));
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("string"), null);
            method.GetArgs().Add(exp);


            finalTrans.data.ExpTypes[method] = new ANamedType(new TIdentifier("void"), null);

            finalTrans.data.SimpleMethodLinks[method] =
                finalTrans.data.Libraries.Methods.Find(
                    libMethod => libMethod.GetName().Text == method.GetName().Text);


            return method;
        }
    }
}
