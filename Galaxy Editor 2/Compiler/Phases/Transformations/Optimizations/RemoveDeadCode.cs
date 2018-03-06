using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class RemoveDeadCode
    {
        public static void Parse(ControlFlowGraph graph )
        {
            for (int i = 1; i < graph.Nodes.Count; i++)
            {
                ControlFlowGraph.Node n = graph.Nodes[i];
                if (n.Predecessors.Count == 0)
                {
                    n.Statement.Parent().RemoveChild(n.Statement);
                    graph.Remove(n);
                    i--;
                }
            }
        }
    }
}
