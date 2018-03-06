using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class StatementRemover : DepthFirstAdapter
    {
        public static bool Parse(AMethodDecl method)
        {
            StatementRemover remover = new StatementRemover();
            method.Apply(remover);
            return remover.changedSomething;
        }

        private bool changedSomething = false;

        private StatementRemover() { }

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
                value = ((ABooleanConstExp)exp).GetBool() is ATrueBool;
                return true;
            }
            if (exp is AIntConstExp)
            {
                value = ((AIntConstExp)exp).GetIntegerLiteral().Text != "0";
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
}
