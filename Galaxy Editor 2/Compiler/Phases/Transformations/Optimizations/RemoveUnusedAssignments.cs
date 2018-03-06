using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class RemoveUnusedAssignments
    {
        public static bool Parse(ControlFlowGraph cfg, SharedData data, out bool redoLivenessAnalysis)
        {
            bool changed = false;
            redoLivenessAnalysis = false;
            Dictionary<ControlFlowGraph.Node, List<ASimpleInvokeExp>> Modifications = new Dictionary<ControlFlowGraph.Node, List<ASimpleInvokeExp>>();
            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                if (node.Expression is AAssignmentExp)
                {
                    AAssignmentExp exp = (AAssignmentExp) node.Expression;
                    if (exp.GetLvalue() is ALocalLvalue)
                    {
                        AALocalDecl decl = data.LocalLinks[(ALocalLvalue) exp.GetLvalue()];
                        //If the variable is not live at any successors, remove this assignment
                        bool inUse = false;
                        foreach (ControlFlowGraph.Node successor in node.Successors)
                        {
                            if (successor.LiveVariables.Contains(decl))
                            {
                                inUse = true;
                                break;
                            }
                        }
                        if (!inUse)
                        {
                            //Move method invokes out
                            GetMethodInvokes getter = new GetMethodInvokes();
                            exp.GetExp().Apply(getter);
                            //Might also have to redo because we removed a reference to a variable in the right side
                            //if (getter.Invokes.Count > 0)
                                redoLivenessAnalysis = true;
                            Modifications[node] = getter.Invokes;
                            changed = true;
                        }
                    }
                }
            }
            foreach (KeyValuePair<ControlFlowGraph.Node, List<ASimpleInvokeExp>> pair in Modifications)
            {
                ControlFlowGraph.Node node = pair.Key;
                foreach (ASimpleInvokeExp invoke in pair.Value)
                {
                    AExpStm stm = new AExpStm(new TSemicolon(";"), invoke);
                    AABlock pBlock = (AABlock) node.Statement.Parent();
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node.Statement), stm);
                    cfg.Insert(node, stm);
                }
                cfg.Remove(node);
                node.Statement.Parent().RemoveChild(node.Statement);
            }
            return changed;
        }

        private class GetMethodInvokes : DepthFirstAdapter
        {
            public List<ASimpleInvokeExp> Invokes = new List<ASimpleInvokeExp>();

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                Invokes.Add(node);
            }
        }
    }
}
