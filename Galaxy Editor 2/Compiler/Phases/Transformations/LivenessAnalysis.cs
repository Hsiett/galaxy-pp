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
    class LivenessAnalysis : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public LivenessAnalysis(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        Dictionary<PStm, List<AALocalDecl>> before = new Dictionary<PStm, List<AALocalDecl>>();
        Dictionary<PStm, List<AALocalDecl>> after = new Dictionary<PStm, List<AALocalDecl>>();
        Dictionary<PStm, List<AALocalDecl>> uses = new Dictionary<PStm, List<AALocalDecl>>();
        Dictionary<PStm, AALocalDecl> assigns = new Dictionary<PStm, AALocalDecl>();
        List<AALocalDecl> definedLocals = new List<AALocalDecl>();
        /*class Span
        {
            public PStm From, To;

            public Span(PStm @from, PStm to)
            {
                From = from;
                To = to;
            }
        }
        Dictionary<AALocalDecl, List<Span>> localSpans = new Dictionary<AALocalDecl, List<Span>>();*/
        private bool changes;
        private bool setUses;
        private bool setSpans;
        private bool fixRefferences;

        class Pair
        {
            public AALocalDecl Local1, Local2;

            public Pair(AALocalDecl local1, AALocalDecl local2)
            {
                Local1 = local1;
                Local2 = local2;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Pair))
                    return false;
                Pair other = (Pair)obj;
                return (Local1 == other.Local1 && Local2 == other.Local2) ||
                       (Local1 == other.Local2 && Local2 == other.Local1);
            }
        }

        List<Pair> intersectingLocals = new List<Pair>();
        Dictionary<AALocalDecl, AALocalDecl> renamedLocals = new Dictionary<AALocalDecl, AALocalDecl>();

        private bool firstMethodCall = true;
        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (firstMethodCall)
                StatementRemover.Parse(node);
            firstMethodCall = false;

            before.Clear();
            after.Clear();
            uses.Clear();
            intersectingLocals.Clear();
            definedLocals.Clear();
            renamedLocals.Clear();
            assigns.Clear();
            changes = false;

            //Make uses
            setUses = true;
            base.CaseAMethodDecl(node);
            setUses = false;


            //Build a list of what's visible
            do
            {
                changes = false;
                base.CaseAMethodDecl(node);
            } while (changes);




            setSpans = true;
            base.CaseAMethodDecl(node);
            setSpans = false;

            //Join locals of same type, unless they are both parameters or they are listed as intersecting
            for (int i = 0; i < definedLocals.Count; i++)
            {
                for (int j = i + 1; j < definedLocals.Count; j++)
                {
                    AALocalDecl decl1 = definedLocals[i];
                    AALocalDecl decl2 = definedLocals[j];

                    if (Util.TypeToString(decl1.GetType()) == Util.TypeToString(decl2.GetType()) &&
                        !intersectingLocals.Contains(new Pair(decl1, decl2)))
                    {
                        if (Util.GetAncestor<AABlock>(decl1) == null &&
                            Util.GetAncestor<AABlock>(decl2) == null)
                            continue;

                        AALocalDecl replacement = decl1;
                        AALocalDecl replaced = decl2;

                        //Dont replace the parameter
                        if (Util.GetAncestor<AABlock>(replaced) == null)
                        {
                            replacement = decl2;
                            replaced = decl1;
                            i--;
                        }
                        j--;

                        renamedLocals.Add(replaced, replacement);
                        definedLocals.Remove(replaced);
                        foreach (Pair pair in intersectingLocals)
                        {
                            if (pair.Local1 == replaced)
                                pair.Local1 = replacement;
                            if (pair.Local2 == replaced)
                                pair.Local2 = replacement;
                        }
                        
                        //Assign defaults
                        if (replaced.GetInit() == null)
                        {
                            ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(replaced.GetName().Text));
                            finalTrans.data.LocalLinks[lvalue] = replaced;
                            finalTrans.data.LvalueTypes[lvalue] = replaced.GetType();
                            List<PStm> statements = AssignDefault(lvalue);
                            PStm pStm = Util.GetAncestor<PStm>(replaced);
                            AABlock pBlock = (AABlock) pStm.Parent();
                            foreach (PStm statement in statements)
                            {
                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), statement);
                            }
                            pBlock.RemoveChild(pStm);
                        }
                        else
                        {
                            //Make an assignment expression instead
                            ALocalDeclStm declStm = (ALocalDeclStm)replaced.Parent();
                            ALocalLvalue lvalue = new ALocalLvalue((TIdentifier)replaced.GetName().Clone());
                            finalTrans.data.LvalueTypes[lvalue] = replaced.GetType();
                            AAssignmentExp exp = new AAssignmentExp(new TAssign("=", lvalue.GetName().Line, lvalue.GetName().Pos), lvalue, replaced.GetInit());
                            AExpStm expStm = new AExpStm(declStm.GetToken(), exp);
                            declStm.ReplaceBy(expStm);
                            finalTrans.data.LvalueTypes[lvalue] = replacement.GetType();
                            finalTrans.data.ExpTypes[exp] = replacement.GetType();
                            finalTrans.data.LocalLinks[lvalue] = replacement;
                        }
                    }
                }
            }


            //Unique names
            List<string> names = new List<string>();
            //Avoid clash with methods/fields/structs
            names.AddRange(finalTrans.data.Methods.Select(declItem => declItem.Decl.GetName().Text));
            names.AddRange(finalTrans.data.Fields.Select(declItem => declItem.Decl.GetName().Text));
            names.AddRange(finalTrans.data.Structs.Select(declItem => declItem.Decl.GetName().Text));
            foreach (AALocalDecl local in definedLocals)
            {
                string name = local.GetName().Text;
                int version = 1;
                while (names.Contains(name))
                {
                    version++;
                    name = local.GetName().Text + version;
                }
                local.GetName().Text = name;
                names.Add(name);
            }

            //Move defined locals to the start of the method
            foreach (AALocalDecl formal in node.GetFormals())
            {
                definedLocals.Remove(formal);
            }
            AABlock block = (AABlock)node.GetBlock();
            for (int i = 0; i < block.GetStatements().Count; i++)
            {
                ALocalDeclStm stm;
                if (block.GetStatements()[i] is ALocalDeclStm)
                {
                    stm = (ALocalDeclStm)block.GetStatements()[i];
                    definedLocals.Remove((AALocalDecl)stm.GetLocalDecl());
                    continue;
                }
                //Add the rest at i
                if (definedLocals.Count == 0)
                    break;
                AALocalDecl decl = definedLocals[0];
                definedLocals.RemoveAt(0);
                
                if (decl.GetInit() == null)
                {
                    ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(decl.GetName().Text));
                    finalTrans.data.LocalLinks[lvalue] = decl;
                    finalTrans.data.LvalueTypes[lvalue] = decl.GetType();
                    List<PStm> statements = AssignDefault(lvalue);
                    PStm pStm = Util.GetAncestor<PStm>(decl);
                    AABlock pBlock = (AABlock) pStm.Parent();
                    foreach (PStm statement in statements)
                    {
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), statement);
                    }
                    pBlock.RemoveChild(pStm);
                }
                else
                {
                    //Make an assignment expression before moving
                    stm = (ALocalDeclStm)decl.Parent();
                    ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(decl.GetName().Text));
                    finalTrans.data.LvalueTypes[lvalue] = decl.GetType();
                    AAssignmentExp exp = new AAssignmentExp(new TAssign("="), lvalue, decl.GetInit());
                    AExpStm expStm = new AExpStm(new TSemicolon(";"), exp);
                    stm.ReplaceBy(expStm);
                    finalTrans.data.LvalueTypes[lvalue] = decl.GetType();
                    finalTrans.data.ExpTypes[exp] = decl.GetType();
                    finalTrans.data.LocalLinks[lvalue] = decl;
                }

                stm = new ALocalDeclStm(new TSemicolon(";"), decl);
                block.GetStatements().Insert(i, stm);
            }



            fixRefferences = true;
            base.CaseAMethodDecl(node);
            fixRefferences = false;


            //If we have an assignment to a local where the stored result is never used, remove the assignment

            //Since we changed some statements, rebuild stuff
            before.Clear();
            after.Clear();
            uses.Clear();
            intersectingLocals.Clear();
            definedLocals.Clear();
            renamedLocals.Clear();
            assigns.Clear();
            changes = false;

            //Make uses
            setUses = true;
            base.CaseAMethodDecl(node);
            setUses = false;


            //Build a list of what's visible
            do
            {
                changes = false;
                base.CaseAMethodDecl(node);
            } while (changes);



            PStm[] stms = new PStm[before.Keys.Count];
            before.Keys.CopyTo(stms, 0);
            foreach (PStm stm in stms)
            {
                if (assigns[stm] != null && //Assignment exp
                    !after[stm].Contains(assigns[stm]) && //Assignment unused
                    Util.GetAncestor<AMethodDecl>(assigns[stm]) != null &&
                    !(stm is ALocalDeclStm))//It is to a local
                {
                    stm.Apply(new MoveMethodDeclsOut(finalTrans.data));
                    stm.Parent().RemoveChild(stm);
                }
            }

            //Remove foo = foo;
            RemoveStupidAssignments assignmentRemover = new RemoveStupidAssignments(finalTrans.data);
            node.Apply(assignmentRemover);

            //Remove unused local variables
            foreach (AALocalDecl local in definedLocals)
            {
                if (Util.GetAncestor<AAProgram>(local) != null && Util.GetAncestor<AABlock>(local) != null &&
                    !finalTrans.data.LocalLinks.Where(p => p.Value == local).Any(p => Util.GetAncestor<AAProgram>(p.Key) != null))
                    local.Parent().Parent().RemoveChild(local.Parent());
            }

            if (assignmentRemover.RemovedOne)
            {
                CaseAMethodDecl(node);
                return;
            }


            //If an assignment to a variable is completely local, and that assignment is only used once, put the assignment where it is used
            //Since we changed some statements, rebuild stuff
            before.Clear();
            after.Clear();
            uses.Clear();
            intersectingLocals.Clear();
            definedLocals.Clear();
            renamedLocals.Clear();
            assigns.Clear();
            changes = false;

            //Make uses
            setUses = true;
            base.CaseAMethodDecl(node);
            setUses = false;


            //Build a list of what's visible
            do
            {
                changes = false;
                base.CaseAMethodDecl(node);
            } while (changes);
            foreach (KeyValuePair<PStm, AALocalDecl> pair in assigns)
            {
                PStm assignStm = pair.Key;
                AALocalDecl assignVar = pair.Value;
                bool isLocal = Util.IsLocal(assignStm, finalTrans.data);
                bool containsInvokeInPrevStm = false;
                if (assignVar != null && after[assignStm].Contains(assignVar))
                {
                    bool dontMove = false;
                    //First, if there are any conditional parrent statement, where the variable is in live before the stm, then we can't move it
                    PStm condParent = (PStm) Util.GetNearestAncestor(assignStm, typeof (AIfThenStm), typeof (AIfThenElseStm),
                                                                     typeof (AWhileStm));
                    while (condParent != null)
                    {
                        if (before[condParent].Contains(assignVar))
                        {
                            dontMove = true;
                            break;
                        }
                        condParent = (PStm)Util.GetNearestAncestor(condParent.Parent(), typeof(AIfThenStm), typeof(AIfThenElseStm),
                                                                     typeof(AWhileStm));
                    }

                    PStm useStm = null;
                    List<PStm> successors = GetSuccessor(assignStm);
                    bool containsConditionalAssignments = false;
                    while (successors.Count > 0)
                    {
                        if (successors.Count > 500)
                            useStm = useStm;
                        PStm successor = successors[0];
                        successors.RemoveAt(0);
                        if (uses[successor].Contains(assignVar))
                        {
                            if (useStm == null && !containsConditionalAssignments && !(successor is AWhileStm))
                                useStm = successor;
                            else
                            {
                                dontMove = true;
                                break;
                            }
                        }
                        if (after[successor].Contains(assignVar) && !(assigns.ContainsKey(successor) && assigns[successor] == assignVar))
                        {
                            List<PStm> newSuccessors = GetSuccessor(successor);
                            foreach (PStm stm in newSuccessors)
                            {
                                if (!successors.Contains(stm))
                                    successors.Add(stm);
                            }
                        }
                        if (assigns.ContainsKey(successor) && uses[assignStm].Contains(assigns[successor]))
                        {
                            dontMove = true;
                            break;
                        }
                        
                        if (assigns.ContainsKey(successor) && assigns[successor] == assignVar)
                        {
                            condParent = (PStm)Util.GetNearestAncestor(successor, typeof(AIfThenStm), typeof(AIfThenElseStm),
                                                                       typeof(AWhileStm));
                            while (condParent != null)
                            {
                                if (!Util.IsAncestor(assignStm, condParent))
                                {
                                    containsConditionalAssignments = true;
                                    break;
                                }
                                condParent = (PStm)Util.GetNearestAncestor(condParent.Parent(), typeof(AIfThenStm), typeof(AIfThenElseStm),
                                                                             typeof(AWhileStm));
                            }

                            //If we found a usage, and it is inside a while that the assignStm is not inside, but we are inside, don't move
                            if (useStm != null)
                            {
                                AWhileStm whileParant = Util.GetAncestor<AWhileStm>(successor);
                                while (whileParant != null)
                                {
                                    if (Util.IsAncestor(useStm, whileParant) && !Util.IsAncestor(assignStm, whileParant))
                                    {
                                        dontMove = true;
                                        break;
                                    }
                                    whileParant = Util.GetAncestor<AWhileStm>(whileParant.Parent());
                                }
                            }
                        }

                        FindInvoke finder  = new FindInvoke();
                        successor.Apply(finder);
                        if (finder.ContainsInvoke && useStm == null)
                            containsInvokeInPrevStm = true;
                    }

                    if (useStm != null && !dontMove)
                    {
                        //If assignStm is inside an if, and the use stm is not, and there is another assignment
                        //to the same variable in an else block, then don't join
                        AIfThenElseStm ifThenElse = Util.GetAncestor<AIfThenElseStm>(assignStm);
                        while (ifThenElse != null)
                        {
                            if (!Util.IsAncestor(useStm, ifThenElse))
                            {
                                ABlockStm otherBlock;
                                if (Util.IsAncestor(assignStm, ifThenElse.GetThenBody()))
                                    otherBlock = (ABlockStm) ifThenElse.GetElseBody();
                                else
                                    otherBlock = (ABlockStm)ifThenElse.GetThenBody();
                                StmEnum enumerator = new StmEnum(otherBlock);
                                while (enumerator.MoveNext())
                                {
                                    PStm stm = (PStm) enumerator.Current;
                                    if (assigns.ContainsKey(stm) && assigns[stm] == assignVar)
                                    {
                                        dontMove = true;
                                        break;
                                    }
                                }
                                if (dontMove)
                                    break;
                            }
                            ifThenElse = Util.GetAncestor<AIfThenElseStm>(ifThenElse.Parent());
                        }

                        //If the assignStm or useStm is inside a while, it could get complicated
                        //if (Util.HasAncestor<AWhileStm>(assignStm) || Util.HasAncestor<AWhileStm>(useStm))
                        //    dontMove = true;
                    }

                    if (useStm != null && dontMove == false && (isLocal || !containsInvokeInPrevStm))
                    {

                        //Ensure that it is not used twice in this stm
                        FindLvalue finder = new FindLvalue(assignVar, finalTrans.data);
                        useStm.Apply(finder);
                        if (!finder.IsUsedTwice && (isLocal || !finder.HasPrevInvoke))
                        {
                            PExp rightside;
                            if (assignStm is ALocalDeclStm)
                            {
                                rightside = ((AALocalDecl)((ALocalDeclStm)assignStm).GetLocalDecl()).GetInit();
                            }
                            else
                            {
                                rightside = ((AAssignmentExp)((AExpStm)assignStm).GetExp()).GetExp();
                                assignStm.Parent().RemoveChild(assignStm);
                            }
                            if (rightside != null)
                            {
                                finder.Lvalue.Parent().ReplaceBy(rightside);
                                CaseAMethodDecl(node);
                                return;
                            }
                        }
                    }
                }
            }

            if (StatementRemover.Parse(node))
            {
                CaseAMethodDecl(node);
                return;
            }
            firstMethodCall = true;
        }

        private class StmEnum : IEnumerator
        {
            private class NextStm : DepthFirstAdapter
            {
                private PStm oldStm;
                public PStm nextStm;

                public NextStm(PStm oldStm)
                {
                    this.oldStm = oldStm;
                }

                public override void DefaultIn(Node node)
                {
                    if (node is PStm)
                    {
                        if (node == oldStm)
                            oldStm = null;
                        else if (oldStm == null && nextStm == null)
                            nextStm = (PStm)node;
                    }
                    base.DefaultIn(node);
                }
            }

            private PStm stm;

            public StmEnum(PStm stm)
            {
                this.stm = stm;
            }

            public bool MoveNext()
            {
                NextStm next = new NextStm(stm);
                stm.Apply(next);
                stm = next.nextStm;
                return stm != null;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public object Current
            {
                get { return stm; }
            }
        }

        private class FindInvoke : DepthFirstAdapter
        {
            public bool ContainsInvoke;

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                ContainsInvoke = true;
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                if (Util.HasAncestor<AAssignmentExp>(node) && 
                    Util.IsAncestor(node, Util.GetAncestor<AAssignmentExp>(node).GetLvalue()))
                    ContainsInvoke = true;
            }
        }

        private class FindLvalue : DepthFirstAdapter
        {
            private SharedData data;
            private AALocalDecl decl;
            public ALocalLvalue Lvalue;
            public bool IsUsedTwice;
            public bool HasPrevInvoke;
            private bool isLeftAssign;

            public FindLvalue(AALocalDecl decl, SharedData data)
            {
                this.decl = decl;
                this.data = data;
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                isLeftAssign = true;
                node.GetLvalue().Apply(this);
                isLeftAssign = false;
                node.GetExp().Apply(this);
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                bool wasLeftAssign = isLeftAssign;
                node.GetBase().Apply(this);
                isLeftAssign = false;
                node.GetIndex().Apply(this);
                isLeftAssign = wasLeftAssign;
            }

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                if (!isLeftAssign && data.LocalLinks[node] == decl)
                {
                    if (Lvalue == null)
                        Lvalue = node;
                    else
                        IsUsedTwice = true;
                }
            }

            public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (Lvalue == null)
                    HasPrevInvoke = true;
            }
        }

        private class RemoveStupidAssignments : DepthFirstAdapter
        {
            private SharedData data;

            public RemoveStupidAssignments(SharedData data)
            {
                this.data = data;
            }

            public bool RemovedOne;
            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                if (node.GetExp() is ALvalueExp && Util.ReturnsTheSame(node.GetLvalue(), ((ALvalueExp)node.GetExp()).GetLvalue(), data))
                {
                    RemovedOne = true;
                    node.Parent().Parent().RemoveChild(node.Parent());
                }
                else
                    base.CaseAAssignmentExp(node);
            }
        }

        
        private List<PStm> AssignDefault(PLvalue lvalue)
        {
            List<PStm> returner = new List<PStm>();
            PType type = finalTrans.data.LvalueTypes[lvalue];
            PExp rightSide = null;
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType) type;

                if (aType.IsPrimitive(GalaxyKeywords.NullablePrimitives.words)) //GalaxyKeywords.NullablePrimitives.words.Contains(aType.GetName().Text))
                {
                    rightSide = new ANullExp();
                    finalTrans.data.ExpTypes[rightSide] = new ANamedType(new TIdentifier("null"), null);
                }
                else if (aType.IsPrimitive(new []{"int", "byte", "fixed"}))
                    /*aType.GetName().Text == "int" ||
                    aType.GetName().Text == "byte" ||
                    aType.GetName().Text == "fixed")*/
                {
                    rightSide = new AIntConstExp(new TIntegerLiteral("0"));
                    finalTrans.data.ExpTypes[rightSide] = type;
                }
                else if (aType.IsPrimitive("bool")) //aType.GetName().Text == "bool")
                {
                    rightSide = new ABooleanConstExp(new AFalseBool());
                    finalTrans.data.ExpTypes[rightSide] = type;
                }
                else if (aType.IsPrimitive("color"))//aType.GetName().Text == "color")
                {
                    PExp arg1 = new AIntConstExp(new TIntegerLiteral("0"));
                    PExp arg2 = new AIntConstExp(new TIntegerLiteral("0"));
                    PExp arg3 = new AIntConstExp(new TIntegerLiteral("0"));
                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("Color"), new ArrayList() {arg1, arg2, arg3});
                    rightSide = invoke;
                    finalTrans.data.ExpTypes[rightSide] = type;
                    finalTrans.data.ExpTypes[arg1] =
                        finalTrans.data.ExpTypes[arg2] =
                        finalTrans.data.ExpTypes[arg3] = new ANamedType(new TIdentifier("int"), null);
                    finalTrans.data.SimpleMethodLinks[invoke] =
                        finalTrans.data.Libraries.Methods.First(func => func.GetName().Text == invoke.GetName().Text);
                }
                else if (aType.IsPrimitive("char"))//aType.GetName().Text == "char")
                {
                    //Dunno?!
                    rightSide = new ACharConstExp(new TCharLiteral("'\0'"));
                    finalTrans.data.ExpTypes[rightSide] = type;
                }
                else //Struct
                {
                    AStructDecl str = finalTrans.data.StructTypeLinks[aType];
                    foreach (AALocalDecl localDecl in str.GetLocals())
                    {
                        ALvalueExp reciever = new ALvalueExp(Util.MakeClone(lvalue, finalTrans.data));
                        AStructLvalue newLvalue = new AStructLvalue(reciever, new ADotDotType(new TDot(".")), new TIdentifier(localDecl.GetName().Text));
                        finalTrans.data.StructFieldLinks[newLvalue] = localDecl;
                        finalTrans.data.ExpTypes[reciever] = type;
                        finalTrans.data.LvalueTypes[newLvalue] = localDecl.GetType();
                        returner.AddRange(AssignDefault(newLvalue));
                    }
                    return returner;
                }
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), Util.MakeClone(lvalue, finalTrans.data), rightSide);
                finalTrans.data.ExpTypes[assignment] = type;
                return new List<PStm>(){new AExpStm(new TSemicolon(";"), assignment)};
            }
            if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType) type;
                for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                {
                    ALvalueExp reciever = new ALvalueExp(Util.MakeClone(lvalue, finalTrans.data));
                    AArrayLvalue newLvalue = new AArrayLvalue(new TLBracket("["), reciever, new AIntConstExp(new TIntegerLiteral(i.ToString())));
                    finalTrans.data.ExpTypes[reciever] = type;
                    finalTrans.data.LvalueTypes[newLvalue] = aType.GetType();
                    finalTrans.data.ExpTypes[newLvalue.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                    returner.AddRange(AssignDefault(newLvalue));
                }
                return returner;
            }
            
            throw new Exception("Unexpected type. (LivenessAnalasys.AssignDefault), got " + type);
        }

        class MoveMethodDeclsOut : DepthFirstAdapter
        {
            private SharedData data;

            public MoveMethodDeclsOut(SharedData data)
            {
                this.data = data;
            }

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                PStm pStm = Util.GetAncestor<PStm>(node);
                AExpStm expStm = new AExpStm(new TSemicolon(";"), node);
                AABlock pBlock = (AABlock) pStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), expStm);


                /*PExp expNode = (PExp)node;
                PType type = data.ExpTypes[expNode];
                ALocalLvalue local = new ALocalLvalue(new TIdentifier("tempName", 0, 0));
                ALvalueExp exp = new ALvalueExp(local);
                PStm stm = Util.GetAncestor<PStm>(node);
                AABlock block = (AABlock)stm.Parent();
                node.ReplaceBy(exp);
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                        Util.MakeClone(type, data),
                                                        new TIdentifier("bulkCopyVar", 0, 0), expNode);
                block.GetStatements().Insert(block.GetStatements().IndexOf(stm), new ALocalDeclStm(new TSemicolon(";"), localDecl));

                data.LvalueTypes[local] = type;
                data.ExpTypes[exp] = type;
                data.LocalLinks[local] = localDecl;
                //localDecl.Apply(this);
                exp.Apply(this);
                return;*/
            }
        }


        public override void DefaultIn(Node node)
        {
            if (node is PStm && !(node is ABlockStm))
            {
                PStm stmNode = (PStm)node;
                if (setUses)
                {
                    uses.Add(stmNode, new List<AALocalDecl>());
                    assigns.Add(stmNode, null);
                    before.Add(stmNode, new List<AALocalDecl>());
                    after.Add(stmNode, new List<AALocalDecl>());
                    return;
                }
                if (setSpans)
                {
                    for (int i = 0; i < before[stmNode].Count; i++)
                    {
                        for (int j = i + 1; j < before[stmNode].Count; j++)
                        {
                            Pair pair = new Pair(before[stmNode][i], before[stmNode][j]);
                            if (!intersectingLocals.Contains(pair))
                                intersectingLocals.Add(pair);
                        }
                        if (assigns[stmNode] != null)
                        {
                            //It is okay to join as long as the statement below does not need it
                            List<PStm> sucessors = GetSuccessor(stmNode);
                            bool usedBelow = false;
                            foreach (PStm sucessor in sucessors)
                            {
                                if (before[sucessor].Contains(before[stmNode][i]))
                                {
                                    usedBelow = true;
                                    break;
                                }
                            }

                            if (usedBelow)
                            {
                                Pair pair = new Pair(before[stmNode][i], assigns[stmNode]);
                                if (!intersectingLocals.Contains(pair))
                                    intersectingLocals.Add(pair);
                            }
                        }
                    }
                }
                if (fixRefferences)
                    return;

                List<AALocalDecl> list = new List<AALocalDecl>();
                foreach (AALocalDecl localDecl in after[stmNode])
                {
                    if (localDecl != assigns[stmNode])
                        list.Add(localDecl);
                }
                foreach (AALocalDecl localDecl in uses[stmNode])
                {
                    if (!list.Contains(localDecl))
                        list.Add(localDecl);
                }
                if (list.Count != before[stmNode].Count) changes = true;
                before[stmNode] = list;
                List<PStm> successors = GetSuccessor(stmNode);
                list = new List<AALocalDecl>();
                foreach (PStm successor in successors)
                {
                    foreach (AALocalDecl localDecl in before[successor])
                    {
                        if (!list.Contains(localDecl))
                            list.Add(localDecl);
                    }
                }
                if (list.Count != after[stmNode].Count) changes = true;
                after[stmNode] = list;
            }
        }


        //if ur an if you got 2
        //if ur a while you got 2
        //if ur not an if else, and your the last element of a block, look up untill you encounter
        //  another block, keep searching in that
        //  a while, pick that
        //  a method decl, no successor
        private List<PStm> GetSuccessor(PStm stm)
        {
            List<PStm> list = new List<PStm>();
            AABlock block;
            ABlockStm blockStm;
            if (stm is AIfThenElseStm)
            {
                AIfThenElseStm aStm = (AIfThenElseStm)stm;
                bool fallthrough = false;
                //Then
                blockStm = (ABlockStm)aStm.GetThenBody();
                block = (AABlock)blockStm.GetBlock();
                GetNonBlockStm stmFinder = new GetNonBlockStm(true);
                block.Apply(stmFinder);
                if (stmFinder.Stm != null)
                    list.Add(stmFinder.Stm);
                else
                    fallthrough = true;
                //Else
                blockStm = (ABlockStm)aStm.GetElseBody();
                block = (AABlock)blockStm.GetBlock();
                stmFinder = new GetNonBlockStm(true);
                block.Apply(stmFinder);
                if (stmFinder.Stm != null)
                    list.Add(stmFinder.Stm);
                else
                    fallthrough = true;
                if (!fallthrough)
                    return list;
            }
            if (stm is AIfThenStm)
            {
                AIfThenStm aStm = (AIfThenStm)stm;
                //Then
                blockStm = (ABlockStm)aStm.GetBody();
                block = (AABlock)blockStm.GetBlock();
                GetNonBlockStm stmFinder = new GetNonBlockStm(true);
                block.Apply(stmFinder);
                if (stmFinder.Stm != null)
                {
                    list.Add(stmFinder.Stm);
                }
            }
            if (stm is AWhileStm)
            {
                AWhileStm aStm = (AWhileStm)stm;
                //Then
                blockStm = (ABlockStm)aStm.GetBody();
                block = (AABlock)blockStm.GetBlock();
                GetNonBlockStm stmFinder = new GetNonBlockStm(true);
                block.Apply(stmFinder);
                if (stmFinder.Stm != null)
                {
                    list.Add(stmFinder.Stm);
                }
            }
            if (stm is ABreakStm)
            {
                AWhileStm whileStm = Util.GetAncestor<AWhileStm>(stm);

                list = GetSuccessor(whileStm);
                list.RemoveAt(0);
                return list;
            }
            if (stm is AContinueStm)
            {
                AWhileStm whileStm = Util.GetAncestor<AWhileStm>(stm);
                list.Add(whileStm);
                return list;
            }

            //Get the next statement in block
            block = (AABlock)stm.Parent();
            int index = block.GetStatements().IndexOf(stm);
            if (index == block.GetStatements().Count - 1)
                list.AddRange(GetSuccessor(block));
            else
            {
                stm = (PStm)block.GetStatements()[index + 1];
                while (stm is ABlockStm)
                {
                    blockStm = (ABlockStm)stm;
                    block = (AABlock)blockStm.GetBlock();

                    if (block.GetStatements().Count == 0)
                    {
                        list.AddRange(GetSuccessor(block));
                        return list;
                    }
                    stm = (PStm)block.GetStatements()[0];
                }
                list.Add(stm);
            }
            return list;
        }

        private List<PStm> GetSuccessor(AABlock block)
        {
            List<PStm> list = new List<PStm>();
            if (block.Parent() is AMethodDecl) return list;

            PStm blockStm = (PStm)block.Parent();
            if (blockStm.Parent() is AWhileStm)
            {
                list.Add((PStm)blockStm.Parent());
                //And the statement after the while
                blockStm = (PStm)blockStm.Parent();

                //return list;
            }
            if (blockStm.Parent() is AIfThenStm || blockStm.Parent() is AIfThenElseStm)
            {
                blockStm = (PStm)blockStm.Parent();
            }

            block = (AABlock)blockStm.Parent();
            int index = block.GetStatements().IndexOf(blockStm);
            if (index == block.GetStatements().Count - 1)
                list.AddRange(GetSuccessor(block));
            else
            {
                PStm stm = (PStm)block.GetStatements()[index + 1];
                while (stm is ABlockStm)
                {
                    blockStm = stm;
                    block = (AABlock)((ABlockStm)blockStm).GetBlock();

                    if (block.GetStatements().Count == 0)
                    {
                        list.AddRange(GetSuccessor(block));
                        return list;
                    }
                    stm = (PStm)block.GetStatements()[0];
                }
                list.Add(stm);
            }
            return list;
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            /*if (setUses && node.GetLvalue() is ALocalLvalue)
            {
                PStm parentStm = Util.GetAncestor<PStm>(node);
                ALocalLvalue lvalue = (ALocalLvalue)node.GetLvalue();
                assigns[parentStm] = finalTrans.data.LocalLinks[lvalue];
                node.GetExp().Apply(this);
                return;
            }*/
            base.CaseAAssignmentExp(node);
        }




        public override void CaseALocalLvalue(ALocalLvalue node)
        {
            if (setUses)
            {
                PStm parentStm = Util.GetAncestor<PStm>(node);
                AALocalDecl decl = finalTrans.data.LocalLinks[node];
                AAssignmentExp assignment = Util.GetAncestor<AAssignmentExp>(node);
                bool isUsage = true;
                if (assignment != null)
                {
                    if (Util.IsAncestor(node, assignment.GetLvalue()))
                    {
                        AArrayLvalue arrayLvalue = Util.GetAncestor<AArrayLvalue>(node);
                        isUsage = false;
                        while (arrayLvalue != null)
                        {
                            if (Util.IsAncestor(node, arrayLvalue.GetIndex()))
                            {
                                isUsage = true;
                                break;
                            }
                            arrayLvalue = Util.GetAncestor<AArrayLvalue>(arrayLvalue.Parent());
                        }
                        if (!isUsage)
                        {
                            //If we have a bulk copy here, it is really only a partial assignment, and so we should not add it as an assignment
                            if (!Util.IsBulkCopy(decl.GetType()))
                                assigns[parentStm] = finalTrans.data.LocalLinks[node];
                        }
                    }
                }
                if (isUsage)
                {
                    if (!uses[parentStm].Contains(decl))
                        uses[parentStm].Add(decl);
                }

            }
            if (fixRefferences)
            {
                AALocalDecl decl = finalTrans.data.LocalLinks[node];
                while (renamedLocals.ContainsKey(decl))
                {
                    decl = renamedLocals[decl];
                }
                finalTrans.data.LocalLinks[node] = decl;
                node.GetName().Text = decl.GetName().Text;
            }
            base.CaseALocalLvalue(node);
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (setUses)
            {
                if (!definedLocals.Contains(node))
                    definedLocals.Add(node);

                PStm parentStm = Util.GetAncestor<PStm>(node);
                if (parentStm != null)
                    assigns[parentStm] = node;

            }
            base.CaseAALocalDecl(node);
        }

        //Remove if (false) and such
        private class StatementRemover : DepthFirstAdapter
        {
            public static bool Parse(AMethodDecl method)
            {
                StatementRemover remover = new StatementRemover();
                method.Apply(remover);
                return remover.changedSomething;
            }

            private bool changedSomething = false;

            private StatementRemover(){}

            public override void OutAIfThenStm(AIfThenStm node)
            {
                bool value;
                if (IsConst(node.GetCondition(), out value))
                {
                    if (value)
                    {
                        node.ReplaceBy(node.GetBody());
                    }
                    else
                    {
                        node.Parent().RemoveChild(node);
                    }
                    changedSomething = true;
                }
            }

            public override void OutAWhileStm(AWhileStm node)
            {
                bool value;
                if (IsConst(node.GetCondition(), out value) && !value)
                {
                    node.Parent().RemoveChild(node);
                    changedSomething = true;
                }
            }

            public override void OutAIfThenElseStm(AIfThenElseStm node)
            {
                bool value;
                if (IsConst(node.GetCondition(), out value))
                {
                    if (value)
                    {
                        node.ReplaceBy(node.GetThenBody());
                    }
                    else
                    {
                        node.ReplaceBy(node.GetElseBody());
                    }
                    changedSomething = true;
                }
            }

            static bool IsConst(PExp exp, out bool value)
            {
                if (exp is ABooleanConstExp)
                {
                    value = ((ABooleanConstExp) exp).GetBool() is ATrueBool;
                    return true;
                }
                if (exp is AIntConstExp)
                {
                    value = ((AIntConstExp) exp).GetIntegerLiteral().Text != "0";
                    return true;
                }
                if (exp is ANullExp)
                {
                    value = false;
                    return true;
                }
                value = false;
                return false;
            }
        }


        //Ran into trouble with while breaks
        /*private class RemoveUnusedAssignments : DepthFirstAdapter
        {
            private interface ILeftSideItem
            {
                bool ArrayUnsure(ILeftSideItem other);
                PLvalue Node { get; }

            }

            private class Local : ILeftSideItem
            {
                public AALocalDecl Definition;
                public PLvalue Node { get; set; }

                public Local(AALocalDecl definition, PLvalue node)
                {
                    Definition = definition;
                    Node = node;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Local))
                        return false;
                    Local other = (Local) obj;

                    return other.Definition == Definition;
                }

                public bool ArrayUnsure(ILeftSideItem other)
                {
                    return false;
                }
            }

            private class StructField : ILeftSideItem
            {
                public ILeftSideItem Base;
                public AALocalDecl Definition;
                public PLvalue Node { get; set; }

                public StructField(ILeftSideItem @base, AALocalDecl definition, PLvalue node)
                {
                    Base = @base;
                    Definition = definition;
                    Node = node;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is StructField))
                        return false;
                    StructField other = (StructField)obj;

                    return Base.Equals(other.Base) && other.Definition == Definition;
                }

                public bool ArrayUnsure(ILeftSideItem obj)
                {
                    if (!(obj is StructField))
                        return false;
                    StructField other = (StructField)obj;

                    return Base.ArrayUnsure(other.Base);
                }
            }
            
            private class Array : ILeftSideItem
            {
                public ILeftSideItem Base;
                public PExp Index;
                public PLvalue Node { get; set; }

                public Array(ILeftSideItem @base, PExp index, PLvalue node)
                {
                    Base = @base;
                    Index = index;
                    Node = node;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Array))
                        return false;
                    Array other = (Array)obj;

                    return Base.Equals(other.Base)/* && Util.ReturnsTheSame(Index, other.Index, SharedData.LastCreated);
                }

                public bool ArrayUnsure(ILeftSideItem obj)
                {
                    if (!(obj is Array))
                        return false;
                    Array other = (Array)obj;

                    return Base.ArrayUnsure(other.Base) || !Util.ReturnsTheSame(Index, other.Index, SharedData.LastCreated);
                }
            }

            private class AssignmentInfo
            {
                public AAssignmentExp Node;
                public ILeftSideItem LeftSide;
                public bool ContainsInvoke;
                public bool IsLocal;
                public List<ILeftSideItem> localsUsed = new List<ILeftSideItem>();
            }

            //This class will look for unused local assignments, and remove them.
            //It will also inline local assignments that are only used once.

            private SharedData data;

            public RemoveUnusedAssignments(SharedData data)
            {
                this.data = data;
            }

            private List<AssignmentInfo> UnusedAssignments = new List<AssignmentInfo>();
            private Dictionary<ILeftSideItem, AssignmentInfo> LastAssignments = new Dictionary<ILeftSideItem, AssignmentInfo>();
            private List<ILeftSideItem> UsedVarsInStm = new List<ILeftSideItem>();
            private Dictionary<AssignmentInfo, List<PExp>> AssignmentUses = new Dictionary<AssignmentInfo, List<PExp>>();
            private ILeftSideItem currentLeftSide;
            private bool leftSideOfAssignment;
            private bool hasInvoke;
            private bool isLocal;
            private bool onlyRecordUses;

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                UnusedAssignments.Clear();
                AssignmentUses.Clear();
                LastAssignments.Clear();
                base.CaseAMethodDecl(node);
                //Remove unusedAssignments
                //Inline single uses
            }
            

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                leftSideOfAssignment = true;
                currentLeftSide = null;
                node.GetLvalue().Apply(this);
                leftSideOfAssignment = false;
                if (currentLeftSide == null || onlyRecordUses)
                {
                    node.GetExp().Apply(this);
                    return;
                }
                List<ILeftSideItem> usedVarsLeftSide = UsedVarsInStm;
                UsedVarsInStm = new List<ILeftSideItem>();
                AssignmentInfo assignment = new AssignmentInfo();
                assignment.LeftSide = currentLeftSide;
                hasInvoke = false;
                isLocal = true;
                node.GetExp().Apply(this);
                assignment.ContainsInvoke = hasInvoke;
                assignment.IsLocal = isLocal;
                assignment.localsUsed.AddRange(UsedVarsInStm);
                UsedVarsInStm.AddRange(usedVarsLeftSide);

                OutStm();

                LastAssignments[assignment.LeftSide] = assignment;
                UnusedAssignments.Add(assignment);
                AssignmentUses.Add(assignment, new List<PExp>());
            }


            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                currentLeftSide = new Local(data.LocalLinks[node], node);
                if (!leftSideOfAssignment)
                    UsedVarsInStm.Add(currentLeftSide);
            }

            public override void OutAStructLvalue(AStructLvalue node)
            {
                if (currentLeftSide != null)
                {
                    currentLeftSide = new StructField(currentLeftSide, data.StructFieldLinks[node], node);
                    if (!leftSideOfAssignment)
                        UsedVarsInStm.Add(currentLeftSide);
                }
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                bool wasLeftSide = leftSideOfAssignment;
                leftSideOfAssignment = false;
                node.GetIndex().Apply(this);
                leftSideOfAssignment = wasLeftSide;
                currentLeftSide = null;
                node.GetBase().Apply(this);
                if (currentLeftSide != null)
                {
                    currentLeftSide = new Array(currentLeftSide, node.GetIndex(), node);
                    if (!leftSideOfAssignment)
                        UsedVarsInStm.Add(currentLeftSide);
                }
            }

            public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                hasInvoke = true;
                isLocal = false;
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                isLocal = false;
            }

            public override void DefaultOut(Node node)
            {
                if (node is PStm)
                    OutStm();
            }

            private void OutStm()
            {
                foreach (ILeftSideItem variable in UsedVarsInStm)
                {
                    if (LastAssignments.ContainsKey(variable))
                    {
                        AssignmentInfo assignment = LastAssignments[variable];
                        if (UnusedAssignments.Contains(assignment))
                            UnusedAssignments.Remove(assignment);
                        if (assignment.LeftSide.ArrayUnsure(variable))
                            continue;
                        AssignmentUses[assignment].Add((ALvalueExp) variable.Node.Parent());
                    }
                } 
                UsedVarsInStm.Clear();
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                base.CaseAWhileStm(node);
                onlyRecordUses = true;
                base.CaseAWhileStm(node);
                onlyRecordUses = false;
            }
        }*/
    }
}
