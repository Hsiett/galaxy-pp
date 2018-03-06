using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveDeadCode : DepthFirstAdapter
    {
        public override void CaseAABlock(AABlock node)
        {
            removeDeadCode(node);
        }

        //Returns true if execution cannot continue after the block
        private bool removeDeadCode(AABlock block)
        {
            bool isDead = false;
            for (int i = 0; i < block.GetStatements().Count; i++)
            {
                PStm stm = (PStm) block.GetStatements()[i];
                if (isDead)
                {
                    block.GetStatements().RemoveAt(i);
                    i--;
                }
                else
                    isDead = removeDeadCode(stm);
            }
            return isDead;
        }

        //Returns true if execution cannot continue after the stm
        private bool removeDeadCode(PStm stm)
        {
            if (stm is ABreakStm || stm is AContinueStm || stm is AVoidReturnStm || stm is AValueReturnStm)
                return true;
            if (stm is AIfThenStm)
            {
                AIfThenStm aStm = (AIfThenStm) stm;
                bool stopped = removeDeadCode(aStm.GetBody());
                if (IsBoolConst(aStm.GetCondition(), true))
                    return stopped;
                return false;
            }
            if (stm is AIfThenElseStm)
            {
                AIfThenElseStm aStm = (AIfThenElseStm)stm;
                bool stopped1 = removeDeadCode(aStm.GetThenBody());
                if (IsBoolConst(aStm.GetCondition(), true))
                    return stopped1;
                bool stopped2 = removeDeadCode(aStm.GetElseBody());
                if (IsBoolConst(aStm.GetCondition(), false))
                    return stopped2;
                return stopped1 && stopped2;
            }
            if (stm is AWhileStm)
            {
                AWhileStm aStm = (AWhileStm)stm;
                removeDeadCode(aStm.GetBody());
                return false;
            }
            if (stm is ABlockStm)
            {
                ABlockStm aStm = (ABlockStm)stm;
                return removeDeadCode((AABlock) aStm.GetBlock());
            }
            return false;
        }

        private bool IsBoolConst(PExp exp, bool val)
        {
            return exp is ABooleanConstExp && ((((ABooleanConstExp) exp).GetBool() is ATrueBool) == val);
        }
    }
}
