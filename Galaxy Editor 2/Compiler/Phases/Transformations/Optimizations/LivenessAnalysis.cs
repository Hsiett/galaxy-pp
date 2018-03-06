using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    static class LivenessAnalysis
    {
        public static void CalculateLiveVariables(ControlFlowGraph cfg, SharedData data)
        {
            //First, generate lists of what is used in each node
            Dictionary<ControlFlowGraph.Node, List<AALocalDecl>> usedVars = new Dictionary<ControlFlowGraph.Node, List<AALocalDecl>>();
            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                node.LiveVariables.Clear();

                PExp exp = node.Expression;
                if (exp == null)
                    usedVars[node] = new List<AALocalDecl>();
                else
                {
                    GetUsedVariables variableFinder = new GetUsedVariables(data);
                    exp.Apply(variableFinder);
                    usedVars[node] = variableFinder.UsedLocals;
                }
            }


            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (ControlFlowGraph.Node node in cfg.Nodes)
                {

                    int count = node.LiveVariables.Count;
                    Join(node);
                    node.LiveVariables.Subtract(GetAssignedTo(node, data));
                    node.LiveVariables.Union(usedVars[node]);
                    changed |= count != node.LiveVariables.Count;
                }
            }
        }

        private static void Join(ControlFlowGraph.Node node)
        {
            foreach (ControlFlowGraph.Node successor in node.Successors)
            {
                node.LiveVariables.Union(successor.LiveVariables);
            }
        }

        private static void Subtract(this List<AALocalDecl> list, List<AALocalDecl> toSubtract)
        {
            foreach (AALocalDecl decl in toSubtract)
            {
                if (list.Contains(decl))
                    list.Remove(decl);
            }
        }

        private static void Union(this List<AALocalDecl> list, List<AALocalDecl> toAdd)
        {
            foreach (AALocalDecl decl in toAdd)
            {
                if (!list.Contains(decl))
                    list.Add(decl);
            }
        }

        private static List<AALocalDecl> GetAssignedTo(ControlFlowGraph.Node node, SharedData data)
        {
            if (node.Statement is ALocalDeclStm)
                return new List<AALocalDecl>(){(AALocalDecl) ((ALocalDeclStm)node.Statement).GetLocalDecl()};
            PExp exp = node.Expression;
            if (exp != null && exp is AAssignmentExp)
            {
                AAssignmentExp aExp = (AAssignmentExp) exp;
                if (aExp.GetLvalue() is ALocalLvalue)
                    return new List<AALocalDecl>(){data.LocalLinks[(ALocalLvalue) aExp.GetLvalue()]};
            }
            return new List<AALocalDecl>();
        }

        private class GetUsedVariables : DepthFirstAdapter
        {
            private SharedData data;

            public GetUsedVariables(SharedData data)
            {
                this.data = data;
            }

            public List<AALocalDecl> UsedLocals = new List<AALocalDecl>();

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                //Only if it is not what it is assigning to
                AAssignmentExp exp = Util.GetAncestor<AAssignmentExp>(node);
                if (exp != null && node == exp.GetLvalue())
                    return;

                UsedLocals.Add(data.LocalLinks[node]);
            }
        }
    }
}
