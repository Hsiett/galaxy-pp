using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class AssignFixup : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public AssignFixup(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        List<AAssignmentExp> internalAssignments = new List<AAssignmentExp>();
        public override void DefaultIn(Node node)
        {
            if (node is PStm)
            {
                internalAssignments.Clear();
            }
            base.DefaultIn(node);
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            if (!(node.Parent() is AExpStm))
            {
                PStm parentStm = Util.GetAncestor<PStm>(node);

                MoveMethodDeclsOut mover = new MoveMethodDeclsOut("multipleAssignmentsVar", finalTrans.data);
                node.GetLvalue().Apply(mover);
                PLvalue lvalue = Util.MakeClone(node.GetLvalue(), finalTrans.data);
                ALvalueExp exp = new ALvalueExp(lvalue);
                finalTrans.data.ExpTypes[exp] = finalTrans.data.LvalueTypes[lvalue];
                node.ReplaceBy(exp);

                AExpStm stm = new AExpStm(new TSemicolon(";"), node);



                AABlock block = (AABlock) parentStm.Parent();
                //block.GetStatements().Insert(block.GetStatements().IndexOf(parentStm), localDeclStm);
                block.GetStatements().Insert(block.GetStatements().IndexOf(parentStm), stm);

                //localDeclStm.Apply(this);
                stm.Apply(this);

                if (parentStm is AWhileStm && Util.IsAncestor(exp, ((AWhileStm)parentStm).GetCondition()))
                {
                    AWhileStm aStm = (AWhileStm)parentStm;
                    //Copy assignment before continues
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
                        stm = new AExpStm(new TSemicolon(";"), Util.MakeClone(node, finalTrans.data));
                        block.GetStatements().Insert(block.GetStatements().IndexOf(continueStm), stm);

                        stm.Apply(this);
                    }
                }
                return;
            }

            base.CaseAAssignmentExp(node);
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

        

        private static PLvalue GetSomeLvalue(PExp exp)
        {
            if (exp is AParenExp)
                return GetSomeLvalue(((AParenExp)exp).GetExp());
            if (exp is ALvalueExp)
                return ((ALvalueExp)exp).GetLvalue();
            if (exp is AAssignmentExp)
                return ((AAssignmentExp)exp).GetLvalue();
            return null;
        }
    }
}
