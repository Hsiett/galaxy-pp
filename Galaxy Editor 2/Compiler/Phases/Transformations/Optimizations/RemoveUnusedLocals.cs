using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations.Tools;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class RemoveUnusedLocals
    {
        public static bool Parse(ControlFlowGraph cfg, SharedData data)
        {
            List<AALocalDecl> usedLocals = GetUsedLocals.Parse(cfg.Method.GetBlock(), data);

            List<ControlFlowGraph.Node> modifications = new List<ControlFlowGraph.Node>();
            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                if (node.Statement is ALocalDeclStm)
                {
                    AALocalDecl decl = (AALocalDecl) ((ALocalDeclStm) node.Statement).GetLocalDecl();
                    if (!usedLocals.Contains(decl))
                    {
                        modifications.Add(node);
                    }
                }
                else
                {
                    break;
                }
            }
            foreach (ControlFlowGraph.Node node in modifications)
            {
                cfg.Remove(node);
                node.Statement.Parent().RemoveChild(node.Statement);
            }
            return modifications.Count > 0;
        }

        
    }
}
