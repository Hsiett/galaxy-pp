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
    class FixInlineMethods : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;
        private bool inlineConstructors;
        private SharedData data
        {
            get { return finalTrans.data; }
        }

        public FixInlineMethods(FinalTransformations finalTrans, bool inlineConstructors)
        {
            this.finalTrans = finalTrans;
            this.inlineConstructors = inlineConstructors;
        }

        private class FindAssignedToFormals : DepthFirstAdapter
        {
            private SharedData data;

            public FindAssignedToFormals(SharedData data)
            {
                this.data = data;
            }

            public List<AALocalDecl> AssignedFormals = new List<AALocalDecl>();


            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                if (Util.HasAncestor<AMethodDecl>(node) && //Is in a method
                    data.LocalLinks[node].Parent() == Util.GetAncestor<AMethodDecl>(node))//Is a link to a formal in that method
                {
                    if (Util.HasAncestor<AAssignmentExp>(node) &&//Is in an assignement
                       Util.IsAncestor(node, Util.GetAncestor<AAssignmentExp>(node).GetLvalue()))//Is left side of the assignment
                    {
                        AssignedFormals.Add(data.LocalLinks[node]);
                    }
                    else if (Util.HasAncestor<ASimpleInvokeExp>(node))
                    {
                        ASimpleInvokeExp invoke = Util.GetAncestor<ASimpleInvokeExp>(node);
                        AMethodDecl method = data.SimpleMethodLinks[invoke];
                        for (int i = 0; i < invoke.GetArgs().Count; i++)
                        {
                            AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                            if ((formal.GetRef() != null || formal.GetOut() != null) && Util.IsAncestor(node, (Node) invoke.GetArgs()[i]))
                            {
                                AssignedFormals.Add(data.LocalLinks[node]);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private class MarkShortMethodsAsInline : DepthFirstAdapter
        {
            private FinalTransformations finalTrans;
            private bool inlineConstructors;
            private SharedData data
            {
                get { return finalTrans.data; }
            }

            public MarkShortMethodsAsInline(FinalTransformations finalTrans, bool inlineConstructors)
            {
                this.finalTrans = finalTrans;
                this.inlineConstructors = inlineConstructors;
            }

            private class CountStatements : DepthFirstAdapter
            {
                public int Count;

                public override void DefaultIn(Node node)
                {
                    if (node is PStm && !(node is ABlockStm))
                        Count++;
                    base.DefaultIn(node);
                }
            }

            private class FindRecurssiveCall : DepthFirstAdapter
            {
                public bool InlinedCallToItself;
                private AMethodDecl initialMethod;
                private SharedData data;

                public FindRecurssiveCall(AMethodDecl initialMethod, SharedData data)
                {
                    this.initialMethod = initialMethod;
                    this.data = data;
                }

                public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
                {
                    AMethodDecl target = data.SimpleMethodLinks[node];
                    if (target == initialMethod)
                        InlinedCallToItself = true;
                    else if (target.GetInline() != null)
                        target.Apply(this);
                    base.CaseASimpleInvokeExp(node);
                }
            }

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                if (node.GetInline() == null && node.GetTrigger() == null && !(data.ConstructorMap.ContainsValue(node) && !inlineConstructors) && node != finalTrans.mainEntry)
                {
                    CountStatements counter = new CountStatements();
                    node.Apply(counter);
                    if (counter.Count <= 2)
                    {
                        //Don't inline if it has a recurssive call to itself
                        FindRecurssiveCall recurssiveCallSearcher = new FindRecurssiveCall(node, data);
                        node.Apply(recurssiveCallSearcher);
                        if (!recurssiveCallSearcher.InlinedCallToItself)
                        {
                            node.SetInline(new TInline("inline"));
                        }
                    }
                }
                base.CaseAMethodDecl(node);
            }
        }

        public override void InAAProgram(AAProgram node)
        {
            if (Options.Compiler.AutomaticallyInlineShortMethods)
                node.Apply(new MarkShortMethodsAsInline(finalTrans, inlineConstructors));
            base.InAAProgram(node);
        }

        public override void OutAAProgram(AAProgram node)
        {
            //Fix types
            foreach (var pair in data.LocalLinks.Where(pair => Util.HasAncestor<AAProgram>(pair.Key) &&
                !Util.TypesEqual(data.ExpTypes[(PExp) pair.Key.Parent()], pair.Value.GetType(), data)))
            {
                ALocalLvalue lvalue = pair.Key;
                AALocalDecl decl = pair.Value;


                data.LvalueTypes[lvalue] = decl.GetType();
                if (lvalue.Parent() is ALvalueExp)
                {
                    data.ExpTypes[(PExp) lvalue.Parent()] = decl.GetType();
                    if (lvalue.Parent().Parent() is APointerLvalue)
                        data.LvalueTypes[(PLvalue) lvalue.Parent().Parent()] = ((APointerType) decl.GetType()).GetType();
                }
            }
            
            base.OutAAProgram(node);
        }


        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (node.GetInline() != null)
            {
                bool canAlwaysInline = true;
                foreach (KeyValuePair<ASimpleInvokeExp, AMethodDecl> pair in data.SimpleMethodLinks)
                {
                    if (pair.Value == node && !Util.HasAncestor<AABlock>(pair.Key))
                    {
                        canAlwaysInline = false;
                        break;
                    }
                }
                if (canAlwaysInline)
                {
                    node.Parent().RemoveChild(node);
                    if (finalTrans.data.Methods.Any(item => item.Decl == node))
                        finalTrans.data.Methods.Remove(finalTrans.data.Methods.First(item => item.Decl == node));
                }
            }
            else
                base.CaseAMethodDecl(node);
        }

        public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
        {
            AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
            if (decl.GetInline() != null && Util.HasAncestor<AABlock>(node))
            {
                foreach (AABlock block in Inline(node, finalTrans))
                {
                    block.Apply(this);
                }
            }
            /*else
                base.CaseASimpleInvokeExp(node);*/
        }

        private class CloneBeforeContinue : DepthFirstAdapter
        {
            private ASimpleInvokeExp node;
            private AALocalDecl replaceVarDecl;
            private SharedData data;
            public List<ASimpleInvokeExp> replacementExpressions = new List<ASimpleInvokeExp>();

            public CloneBeforeContinue(ASimpleInvokeExp node, AALocalDecl replaceVarDecl, SharedData data)
            {
                this.node = node;
                this.replaceVarDecl = replaceVarDecl;
                this.data = data;
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                //Don't enter
            }

            public override void CaseAContinueStm(AContinueStm node)
            {
                AABlock pBlock = (AABlock) node.Parent();
                ALocalLvalue replaceVarRef = new ALocalLvalue(new TIdentifier("whileVar"));
                ASimpleInvokeExp clone = (ASimpleInvokeExp)Util.MakeClone(this.node, data);
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), replaceVarRef, clone);
                data.LocalLinks[replaceVarRef] = replaceVarDecl;
                data.ExpTypes[assignment] = data.LvalueTypes[replaceVarRef] = replaceVarDecl.GetType();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), new AExpStm(new TSemicolon(";"), assignment));
                replacementExpressions.Add(clone);
            }
        }

        public static List<AABlock> Inline(ASimpleInvokeExp node, FinalTransformations finalTrans)
        {
            /*if (Util.GetAncestor<AMethodDecl>(node) != null && Util.GetAncestor<AMethodDecl>(node).GetName().Text == "UIChatFrame_LeaveChannel")
                node = node;*/

            SharedData data = finalTrans.data;
            //If this node is inside the condition of a while, replace it with a new local var, 
            //make a clone before the while, one before each continue in the while, and one at the end of the while
            //(unless the end is a return or break)
            AABlock pBlock;
            if (Util.HasAncestor<AWhileStm>(node))
            {
                AWhileStm whileStm = Util.GetAncestor<AWhileStm>(node);
                if (Util.IsAncestor(node, whileStm.GetCondition()))
                {
                    List<ASimpleInvokeExp> toInline = new List<ASimpleInvokeExp>();
                    //Above while
                    AALocalDecl replaceVarDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                 Util.MakeClone(data.ExpTypes[node], data),
                                                                 new TIdentifier("whileVar"), null);
                    ALocalLvalue replaceVarRef = new ALocalLvalue(new TIdentifier("whileVar"));
                    ALvalueExp replaceVarRefExp = new ALvalueExp(replaceVarRef);
                    data.LocalLinks[replaceVarRef] = replaceVarDecl;
                    data.ExpTypes[replaceVarRefExp] = data.LvalueTypes[replaceVarRef] = replaceVarDecl.GetType();
                    node.ReplaceBy(replaceVarRefExp);
                    replaceVarDecl.SetInit(node);
                    pBlock = (AABlock) whileStm.Parent();
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(whileStm), new ALocalDeclStm(new TSemicolon(";"), replaceVarDecl));
                    toInline.Add(node);


                    //In the end of the while
                    PStm lastStm = whileStm.GetBody();
                    while (lastStm is ABlockStm)
                    {
                        AABlock block = (AABlock) ((ABlockStm) lastStm).GetBlock();
                        if (block.GetStatements().Count == 0)
                        {
                            lastStm = null;
                            break;
                        }
                        lastStm = (PStm) block.GetStatements()[block.GetStatements().Count - 1];
                    }
                    if (lastStm == null || !(lastStm is AValueReturnStm || lastStm is AVoidReturnStm || lastStm is ABreakStm))
                    {
                        lastStm = whileStm.GetBody();
                        AABlock block;
                        if (lastStm is ABlockStm)
                        {
                            block = (AABlock)((ABlockStm)lastStm).GetBlock();
                        }
                        else
                        {
                            block = new AABlock(new ArrayList(), new TRBrace("}"));
                            block.GetStatements().Add(lastStm);
                            whileStm.SetBody(new ABlockStm(new TLBrace("{"), block));
                        }

                        replaceVarRef = new ALocalLvalue(new TIdentifier("whileVar"));
                        ASimpleInvokeExp clone = (ASimpleInvokeExp)Util.MakeClone(node, data);
                        toInline.Add(clone);
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), replaceVarRef, clone);
                        data.LocalLinks[replaceVarRef] = replaceVarDecl;
                        data.ExpTypes[assignment] = data.LvalueTypes[replaceVarRef] = replaceVarDecl.GetType();
                        block.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));
                    }

                    //After each continue
                    CloneBeforeContinue cloner = new CloneBeforeContinue(node, replaceVarDecl, data);
                    whileStm.GetBody().Apply(cloner);
                    toInline.AddRange(cloner.replacementExpressions);
                    List<AABlock> visitBlocks = new List<AABlock>();
                    foreach (ASimpleInvokeExp invoke in toInline)
                    {
                        visitBlocks.AddRange(Inline(invoke, finalTrans));
                    }
                    return visitBlocks;
                }
            }




            AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
            FindAssignedToFormals assignedToFormalsFinder = new FindAssignedToFormals(finalTrans.data);
            decl.Apply(assignedToFormalsFinder);
            List<AALocalDecl> assignedToFormals = assignedToFormalsFinder.AssignedFormals;


            /*
                 * inline int foo(int a)
                 * {
                 *      int b = 2;
                 *      int c;
                 *      ...
                 *      while(...)
                 *      {
                 *          ...
                 *          break;
                 *          ...
                 *          return c;
                 *      }
                 *      ...
                 *      return 2;
                 * }
                 * 
                 * bar(foo(<arg for a>));
                 * ->
                 * 
                 * {
                 *      bool inlineMethodReturned = false;
                 *      int inlineReturner;
                 *      int a = <arg for a>;
                 *      while (!inlineMethodReturned)
                 *      {
                 *          int b = 2;
                 *          int c;
                 *          ...
                 *          while(...)
                 *          {
                 *              ... 
                 *              break
                 *              ...
                 *              inlineReturner = c;
                 *              inlineMethodReturned = true;
                 *              break;
                 *          }
                 *          if (inlineMethodReturned)
                 *          {
                 *              break;
                 *          }
                 *          ...
                 *          inlineReturner = 2;
                 *          inlineMethodReturned = true;
                 *          break;
                 *          break;
                 *      }
                 *      bar(inlineReturner);
                 * }
                 * 
                 * 
                 */


            AABlock outerBlock = new AABlock();
            PExp exp = new ABooleanConstExp(new AFalseBool());
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
            AALocalDecl hasMethodReturnedVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("bool"), null),
                                                               new TIdentifier("hasInlineReturned"), exp);
            finalTrans.data.GeneratedVariables.Add(hasMethodReturnedVar);
            PStm stm = new ALocalDeclStm(new TSemicolon(";"), hasMethodReturnedVar);
            outerBlock.GetStatements().Add(stm);

            AALocalDecl methodReturnerVar = null;
            if (!(decl.GetReturnType() is AVoidType))
            {
                methodReturnerVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(decl.GetReturnType(), finalTrans.data),
                                                       new TIdentifier("inlineReturner"), null);
                stm = new ALocalDeclStm(new TSemicolon(";"), methodReturnerVar);
                outerBlock.GetStatements().Add(stm);
            }

            AABlock afterBlock = new AABlock();

            //A dictionary from the formals of the inline method to a cloneable replacement lvalue
            Dictionary<AALocalDecl, PLvalue> Parameters = new Dictionary<AALocalDecl, PLvalue>();
            Dictionary<AALocalDecl, PExp> ParameterExps = new Dictionary<AALocalDecl, PExp>();
            for (int i = 0; i < decl.GetFormals().Count; i++)
            {
                AALocalDecl formal = (AALocalDecl)decl.GetFormals()[i];
                PExp arg = (PExp)node.GetArgs()[0];
                PLvalue lvalue;
                //if ref, dont make a new var
                if (formal.GetRef() != null && arg is ALvalueExp)
                {
                    arg.Apply(new MoveMethodDeclsOut("inlineVar", finalTrans.data));
                    arg.Parent().RemoveChild(arg);
                    lvalue = ((ALvalueExp) arg).GetLvalue();
                    
                }
                else if (!assignedToFormals.Contains(formal) && Util.IsLocal(arg, finalTrans.data))
                {
                    lvalue = new ALocalLvalue(new TIdentifier("I hope I dont make it"));
                    finalTrans.data.LvalueTypes[lvalue] = formal.GetType();
                    finalTrans.data.LocalLinks[(ALocalLvalue) lvalue] = formal;
                    ParameterExps[formal] = arg;
                    arg.Parent().RemoveChild(arg);
                }
                else 
                {
                    AAssignmentExp assExp = null;
                    if (formal.GetOut() != null)
                    {
                        //Dont initialize with arg, but assign arg after
                        arg.Apply(new MoveMethodDeclsOut("inlineVar", finalTrans.data));
                        lvalue = ((ALvalueExp)arg).GetLvalue();
                        assExp = new AAssignmentExp(new TAssign("="), lvalue, null);
                        finalTrans.data.ExpTypes[assExp] = finalTrans.data.LvalueTypes[lvalue];
                        arg.Parent().RemoveChild(arg);
                        arg = null;
                    }
                    AALocalDecl parameter = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(formal.GetType(), finalTrans.data),
                                                            new TIdentifier(formal.GetName().Text),
                                                            arg);
                    stm = new ALocalDeclStm(new TSemicolon(";"), parameter);
                    outerBlock.GetStatements().Add(stm);

                    lvalue = new ALocalLvalue(new TIdentifier(parameter.GetName().Text));
                    finalTrans.data.LvalueTypes[lvalue] = parameter.GetType();
                    finalTrans.data.LocalLinks[(ALocalLvalue)lvalue] = parameter;


                    if (formal.GetOut() != null)
                    {
                        //Dont initialize with arg, but assign arg after
                        ALvalueExp lvalueExp = new ALvalueExp(Util.MakeClone(lvalue, finalTrans.data));
                        finalTrans.data.ExpTypes[lvalueExp] = finalTrans.data.LvalueTypes[lvalue];
                        assExp.SetExp(lvalueExp);
                        afterBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assExp));
                    }
                }
                Parameters.Add(formal, lvalue);
            }

            AABlock innerBlock = (AABlock)decl.GetBlock().Clone();
            exp = new ABooleanConstExp(new ATrueBool());
            finalTrans.data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
            ABlockStm innerBlockStm = new ABlockStm(new TLBrace("{"), innerBlock);

            bool needWhile = CheckIfWhilesIsNeeded.IsWhileNeeded(decl.GetBlock());
            if (needWhile)
                stm = new AWhileStm(new TLParen("("), exp, innerBlockStm);
            else
                stm = innerBlockStm;
            outerBlock.GetStatements().Add(stm);

            outerBlock.GetStatements().Add(new ABlockStm(new TLBrace("{"), afterBlock));

            //Clone method contents to inner block.
            CloneMethod cloneFixer = new CloneMethod(finalTrans, Parameters, ParameterExps, innerBlockStm);
            decl.GetBlock().Apply(cloneFixer);
            foreach (KeyValuePair<PLvalue, PExp> pair in cloneFixer.ReplaceUsAfter)
            {
                PLvalue lvalue = pair.Key;
                PExp replacement =  Util.MakeClone(pair.Value, finalTrans.data);
                ALvalueExp lvalueParent = (ALvalueExp) lvalue.Parent();
                lvalueParent.ReplaceBy(replacement);
            }
            innerBlockStm.Apply(new FixTypes(finalTrans.data));

            innerBlock.Apply(new FixReturnsAndWhiles(hasMethodReturnedVar, methodReturnerVar, finalTrans.data, needWhile));

            GetNonBlockStm stmFinder = new GetNonBlockStm(false);
            innerBlock.Apply(stmFinder);
            if (needWhile && (stmFinder.Stm == null || !(stmFinder.Stm is ABreakStm)))
                innerBlock.GetStatements().Add(new ABreakStm(new TBreak("break")));

            //Insert before current statement
            ABlockStm outerBlockStm = new ABlockStm(new TLBrace("{"), outerBlock);

            PStm pStm = Util.GetAncestor<PStm>(node);

            pBlock = (AABlock)pStm.Parent();

            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), outerBlockStm);

            if (node.Parent() == pStm && pStm is AExpStm)
            {
                pBlock.RemoveChild(pStm);
            }
            else
            {
                PLvalue lvalue = new ALocalLvalue(new TIdentifier(methodReturnerVar.GetName().Text));
                finalTrans.data.LvalueTypes[lvalue] = methodReturnerVar.GetType();
                finalTrans.data.LocalLinks[(ALocalLvalue)lvalue] = methodReturnerVar;
                exp = new ALvalueExp(lvalue);
                finalTrans.data.ExpTypes[exp] = methodReturnerVar.GetType();

                node.ReplaceBy(exp);
            }
            return new List<AABlock>() { outerBlock };
        }

        private class FixTypes : DepthFirstAdapter
        {
            private SharedData data;

            public FixTypes(SharedData data)
            {
                this.data = data;
            }

            public override void OutALvalueExp(ALvalueExp node)
            {
                data.ExpTypes[node] = data.LvalueTypes[node.GetLvalue()];
            }

            public override void OutALocalLvalue(ALocalLvalue node)
            {
                data.LvalueTypes[node] = data.LocalLinks[node].GetType();
            }

            public override void OutAPointerLvalue(APointerLvalue node)
            {
                data.LvalueTypes[node] = ((APointerType) data.ExpTypes[node.GetBase()]).GetType();
            }
        }

        private class CheckIfWhilesIsNeeded : DepthFirstAdapter
        {
            private bool hitReturn;
            private bool NeedWhileLoop;

            public static bool IsWhileNeeded(Node root)
            {
                CheckIfWhilesIsNeeded check = new CheckIfWhilesIsNeeded();
                root.Apply(check);
                return check.NeedWhileLoop;
            }

            private CheckIfWhilesIsNeeded(){}

            public override void DefaultIn(Node node)
            {
                if (node is PStm && hitReturn)
                    NeedWhileLoop = true;
                if (node is AVoidReturnStm || node is AValueReturnStm)
                    hitReturn = true;
            }
        }

        private class FixReturnsAndWhiles : DepthFirstAdapter
        {
            private AALocalDecl hasMethodReturnedVar;
            private AALocalDecl methodReturnerVar;
            private SharedData data;
            private bool neededWhile;

            public FixReturnsAndWhiles(AALocalDecl hasMethodReturnedVar, AALocalDecl methodReturnerVar, SharedData data, bool neededWhile)
            {
                this.hasMethodReturnedVar = hasMethodReturnedVar;
                this.methodReturnerVar = methodReturnerVar;
                this.data = data;
                this.neededWhile = neededWhile;
            }

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                /*
                 * return <exp>;
                 * ->
                 * methodReturnerVar = <exp>;
                 * hasMethodReturnedVar = true;
                 * break;
                 */
                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(methodReturnerVar.GetName().Text));
                data.LvalueTypes[lvalue] = methodReturnerVar.GetType();
                data.LocalLinks[lvalue] = methodReturnerVar;
                AAssignmentExp exp = new AAssignmentExp(new TAssign("="), lvalue, node.GetExp());
                data.ExpTypes[exp] = methodReturnerVar.GetType();
                PStm stm = new AExpStm(new TSemicolon(";"), exp);
                AABlock block = new AABlock();
                block.GetStatements().Add(stm);

                block.GetStatements().Add(new AVoidReturnStm(node.GetToken()));

                node.ReplaceBy(new ABlockStm(new TLBrace("{"), block));
                block.Apply(this);
            }

            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                if (!neededWhile)
                {
                    node.Parent().RemoveChild(node);
                    return;
                }

                /*
                 * return;
                 * ->
                 * hasMethodReturnedVar = true;
                 * break;
                 */
                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(hasMethodReturnedVar.GetName().Text));
                data.LvalueTypes[lvalue] = hasMethodReturnedVar.GetType();
                data.LocalLinks[lvalue] = hasMethodReturnedVar;
                PExp exp = new ABooleanConstExp(new ATrueBool());
                data.ExpTypes[exp] = new ANamedType(new TIdentifier("bool"), null);
                exp = new AAssignmentExp(new TAssign("="), lvalue, exp);
                data.ExpTypes[exp] = hasMethodReturnedVar.GetType();
                PStm stm = new AExpStm(new TSemicolon(";"), exp);
                AABlock block = new AABlock();
                block.GetStatements().Add(stm);

                block.GetStatements().Add(new ABreakStm(new TBreak("break")));

                node.ReplaceBy(new ABlockStm(new TLBrace("{"), block));
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                /*
                 * while(...){...}
                 * ->
                 * while(...){...}
                 * if (hasMethodReturnedVar)
                 * {
                 *      break;
                 * }
                 */
                if (neededWhile)
                {
                    ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(hasMethodReturnedVar.GetName().Text));
                    data.LvalueTypes[lvalue] = hasMethodReturnedVar.GetType();
                    data.LocalLinks[lvalue] = hasMethodReturnedVar;
                    ALvalueExp exp = new ALvalueExp(lvalue);
                    data.ExpTypes[exp] = hasMethodReturnedVar.GetType();

                    AABlock ifBlock = new AABlock();
                    ifBlock.GetStatements().Add(new ABreakStm(new TBreak("break")));
                    ABlockStm ifBlockStm = new ABlockStm(new TLBrace("{"), ifBlock);

                    AIfThenStm ifStm = new AIfThenStm(new TLParen("("), exp, ifBlockStm);

                    AABlock pBlock = (AABlock) node.Parent();
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node) + 1, ifStm);
                }
                node.GetBody().Apply(this);
            }
        }

        private class CloneMethod : DepthFirstAdapter
        {
            private FinalTransformations finalTrans;
            private Dictionary<AALocalDecl, PLvalue> localMap;
            private Dictionary<AALocalDecl, PExp> localExpMap;
            public Dictionary<PLvalue, PExp> ReplaceUsAfter = new Dictionary<PLvalue, PExp>();
            private Node currentCloneNode;

            public CloneMethod(FinalTransformations finalTrans, Dictionary<AALocalDecl, PLvalue> localMap, Dictionary<AALocalDecl, PExp> localExpMap, Node currentCloneNode)
            {
                this.finalTrans = finalTrans;
                this.localMap = localMap;
                this.localExpMap = localExpMap;
                this.currentCloneNode = currentCloneNode;
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
                finalTrans.data.Locals//Lets forget about this one
                finalTrans.data.LvalueTypes
                finalTrans.data.SimpleMethodLinks
                finalTrans.data.StructFieldLinks
                finalTrans.data.StructMethodLinks
                finalTrans.data.StructTypeLinks*/
                if (node is ANewExp && finalTrans.data.ConstructorLinks.ContainsKey((ANewExp)node))
                    finalTrans.data.ConstructorLinks[(ANewExp) currentCloneNode] = finalTrans.data.ConstructorLinks[(ANewExp) node];

                if (node is PExp)
                    finalTrans.data.ExpTypes[(PExp)currentCloneNode] = finalTrans.data.ExpTypes[(PExp)node];

                if (node is AStringConstExp && finalTrans.data.TriggerDeclarations.Any(p => p.Value.Contains(((AStringConstExp)node).GetStringLiteral())))
                    finalTrans.data.TriggerDeclarations.First(
                        p => p.Value.Contains(((AStringConstExp) node).GetStringLiteral())).Value.Add(
                            ((AStringConstExp) currentCloneNode).GetStringLiteral());
                if (node is AFieldLvalue)
                    finalTrans.data.FieldLinks[(AFieldLvalue)currentCloneNode] = finalTrans.data.FieldLinks[(AFieldLvalue)node];
                if (node is ALocalLvalue)
                {
                    AALocalDecl originalFormal = finalTrans.data.LocalLinks[(ALocalLvalue) node];
                    
                    PLvalue replacer = Util.MakeClone(localMap[originalFormal],
                                                        finalTrans.data);
                    currentCloneNode.ReplaceBy(replacer);
                    currentCloneNode = replacer;

                    if (localExpMap.ContainsKey(originalFormal))
                    {
                        ReplaceUsAfter[replacer] = localExpMap[originalFormal];
                    }
                    
                }
                if (node is AALocalDecl)
                {
                    ALocalLvalue replacer = new ALocalLvalue(new TIdentifier("IwillGetRenamedLater"));
                    finalTrans.data.LvalueTypes[replacer] = ((AALocalDecl) currentCloneNode).GetType();
                    finalTrans.data.LocalLinks[replacer] = (AALocalDecl)currentCloneNode;
                    localMap.Add((AALocalDecl)node, replacer);
                }
                if (node is PLvalue)
                    finalTrans.data.LvalueTypes[(PLvalue)currentCloneNode] = finalTrans.data.LvalueTypes[(PLvalue)node];
                if (node is ASimpleInvokeExp)
                    finalTrans.data.SimpleMethodLinks[(ASimpleInvokeExp)currentCloneNode] = finalTrans.data.SimpleMethodLinks[(ASimpleInvokeExp)node];
                if (node is AStructLvalue)
                    finalTrans.data.StructFieldLinks[(AStructLvalue)currentCloneNode] = finalTrans.data.StructFieldLinks[(AStructLvalue)node];
                if (node is ANonstaticInvokeExp)
                    finalTrans.data.StructMethodLinks[(ANonstaticInvokeExp)currentCloneNode] = finalTrans.data.StructMethodLinks[(ANonstaticInvokeExp)node];
                if (node is ANamedType && finalTrans.data.StructTypeLinks.Keys.Contains(node))
                    finalTrans.data.StructTypeLinks[(ANamedType)currentCloneNode] = finalTrans.data.StructTypeLinks[(ANamedType)node];
                if (node is ANamedType && finalTrans.data.DelegateTypeLinks.Keys.Contains(node))
                    finalTrans.data.DelegateTypeLinks[(ANamedType)currentCloneNode] = finalTrans.data.DelegateTypeLinks[(ANamedType)node];
                if (node is APropertyLvalue && finalTrans.data.PropertyLinks.ContainsKey((APropertyLvalue)node))
                    finalTrans.data.PropertyLinks[(APropertyLvalue) currentCloneNode] = finalTrans.data.PropertyLinks[(APropertyLvalue) node];
            }

            public override void DefaultOut(Node node)
            {
                currentCloneNode = currentCloneNode.Parent();
            }

            private class GetChildTypeIndex : DepthFirstAdapter
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

            private class GetChildTypeByIndex : DepthFirstAdapter
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
}
