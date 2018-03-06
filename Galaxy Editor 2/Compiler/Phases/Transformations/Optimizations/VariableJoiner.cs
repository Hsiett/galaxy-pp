using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class VariableJoiner
    {
        public static bool Parse(ControlFlowGraph cfg, SharedData data)
        {
            //List of locals by type
            List<List<AALocalDecl>> locals = new List<List<AALocalDecl>>();
            //Locals -> cfg node
            Dictionary<AALocalDecl, ControlFlowGraph.Node> localNodes = new Dictionary<AALocalDecl, ControlFlowGraph.Node>();
            foreach (AALocalDecl formal in cfg.Method.GetFormals())
            {
                bool added = false;
                foreach (List<AALocalDecl> list in locals)
                {
                    if (Util.TypesEqual(list[0].GetType(), formal.GetType(), data))
                    {
                        list.Add(formal);
                        added = true;
                        break;
                    }
                }
                if (!added)
                    locals.Add(new List<AALocalDecl>(){formal});
            }
            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                //Locals have been moved to the top
                if (!(node.Statement is ALocalDeclStm))
                    break;

                AALocalDecl decl = (AALocalDecl) ((ALocalDeclStm) node.Statement).GetLocalDecl();

                localNodes[decl] = node;

                bool added = false;
                foreach (List<AALocalDecl> list in locals)
                {
                    if (Util.TypesEqual(list[0].GetType(), decl.GetType(), data))
                    {
                        list.Add(decl);
                        added = true;
                        break;
                    }
                }
                if (!added)
                    locals.Add(new List<AALocalDecl>() {decl});
            }


            bool joinedSomething = false;
            foreach (List<AALocalDecl> list in locals)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        //Can't join if both are formals
                        if (list[i].Parent() is AMethodDecl &&
                            list[j].Parent() is AMethodDecl)
                            continue;

                        //Can't join if both are live at the same time
                        bool canJoin = true;
                        foreach (ControlFlowGraph.Node cfgNode in cfg.Nodes)
                        {
                            if (cfgNode.LiveVariables.Contains(list[i]) &&
                                cfgNode.LiveVariables.Contains(list[j]))
                            {
                                canJoin = false;
                                break;
                            }
                        }
                        if (!canJoin)
                            continue;

                        joinedSomething = true;
                        Join(list[i], list[j], cfg, localNodes, data);
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
            return joinedSomething;
        }

        private static void Join(AALocalDecl decl1, AALocalDecl decl2, ControlFlowGraph cfg, Dictionary<AALocalDecl, ControlFlowGraph.Node> localNodes, SharedData data)
        {
            //Remove decl2 from cfg
            //decl2 can't be a formal, since formals was first in the list, and two formals wont be joined.
            ControlFlowGraph.Node cfgNode2 = localNodes[decl2];
            cfg.Remove(cfgNode2);
            //Remove decl2 from the ast
            cfgNode2.Statement.Parent().RemoveChild(cfgNode2.Statement);
            //Go through cfg and make live decl2 variables become live decl1 variables.));
            foreach (ControlFlowGraph.Node cfgNode in cfg.Nodes)
            {
                if (cfgNode.LiveVariables.Contains(decl2))
                {
                    cfgNode.LiveVariables.Remove(decl2);
                    cfgNode.LiveVariables.Add(decl1);
                }
            }
            //Rename all refferences to decl2 to be decl1
            LinkedList<ALocalLvalue> keys = new LinkedList<ALocalLvalue>();
            foreach (KeyValuePair<ALocalLvalue, AALocalDecl> pair in data.LocalLinks)
            {
                if (pair.Value == decl2)
                    keys.AddLast(pair.Key);
            }
            foreach (ALocalLvalue key in keys)
            {
                data.LocalLinks[key] = decl1;
            }
        }
    }
}
