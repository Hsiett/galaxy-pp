using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations.Tools;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class RemoveSingleUsedAssignments
    {
        public static bool Parse(ControlFlowGraph cfg, SharedData data)
        {
            Dictionary<PStm, ControlFlowGraph.Node> statementNodes = new Dictionary<PStm, ControlFlowGraph.Node>();
            //Dictionary<ControlFlowGraph.Node, List<AALocalDecl>> uses = new Dictionary<ControlFlowGraph.Node, List<AALocalDecl>>();
            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                statementNodes[node.Statement] = node;

                /*GetUses getUses = new GetUses();
                node.Statement.Apply(getUses);
                uses[node] = getUses.Uses;*/
            }


            foreach (ControlFlowGraph.Node node in cfg.Nodes)
            {
                if (node.Expression is AAssignmentExp)
                {
                    AAssignmentExp exp = (AAssignmentExp) node.Expression;



                    if (exp.GetLvalue() is ALocalLvalue)
                    {
                        AALocalDecl decl = data.LocalLinks[(ALocalLvalue) exp.GetLvalue()];
                        bool dontMove = false;

                        LocalChecker.ModifyData declModData = LocalChecker.GetLocalData(node.Expression, data);

                        /*1: Search for unique use of decl
                         *  Stop if you find an assignment to decl
                         *  Stop if decl is not live
                         *  Cancel if you find two uses of decl (also within one statement)
                         */
                        ControlFlowGraph.Node useNode = null;
                        LinkedList<ControlFlowGraph.Node> nextNodes = new LinkedList<ControlFlowGraph.Node>();
                        LinkedList<ControlFlowGraph.Node> visitedNodes = new LinkedList<ControlFlowGraph.Node>();
                        foreach (var successor in node.Successors)
                        {
                            nextNodes.AddLast(successor);
                        }
                        while (nextNodes.Count > 0)
                        {
                            ControlFlowGraph.Node successor = nextNodes.First.Value;
                            nextNodes.RemoveFirst();
                            visitedNodes.AddLast(successor);
                            LocalChecker.ModifyData succData = LocalChecker.GetLocalData(successor.Expression, data);

                            //Stop if decl is not live
                            if (!successor.LiveVariables.Contains(decl))
                                continue;
                            

                            //Check if this node uses it)
                            if (succData.Locals.ContainsKey(decl))
                            {
                                var val = succData.Locals[decl];
                                if (val.Reads)
                                {
                                    if (useNode == null)
                                    {
                                        useNode = successor;
                                    }
                                    else
                                    {
                                        //Cancel if we found two uses
                                        dontMove = true;
                                        break;
                                    }
                                }
                                //Stop if it writes to the variable
                                if (val.Writes)
                                    continue;
                            }


                            foreach (ControlFlowGraph.Node nextSucc in successor.Successors)
                            {
                                if (visitedNodes.Contains(nextSucc))
                                    continue;
                                nextNodes.AddLast(nextSucc);
                            }
                        }

                        ALocalLvalue usedLvalue;
                        if (dontMove || 
                            useNode == null ||
                            //Cancel if there are two uses of the local within same statement
                            GetUses.HasMoreThanOneUses(useNode.Statement, decl, data, out usedLvalue))
                            continue;

                        /*2: Search back from found statement, to ensure unique assignment
                         *  Stop if assignment reached
                         *  Cancel if start of method is reached
                         *  Cancel if any statement reads/writes to a variable used in the statement (where one of them writes)
                         *  Cancel if another assignment to x is found (including local decls)
                         */
                        visitedNodes.Clear();
                        nextNodes.Clear();
                        foreach (ControlFlowGraph.Node predecessor in useNode.Predecessors)
                        {
                            nextNodes.AddLast(predecessor);
                        }
                        while (nextNodes.Count > 0)
                        {
                            ControlFlowGraph.Node predecessor = nextNodes.First.Value;
                            nextNodes.RemoveFirst();
                            visitedNodes.AddLast(predecessor);
                            LocalChecker.ModifyData predData = LocalChecker.GetLocalData(predecessor.Expression, data);

                            //Stop if assignment reached
                            if (predecessor == node)
                                continue;


                            //Cancel if this statement writes to decl
                            if (predData.Locals.ContainsKey(decl) &&  
                                predData.Locals[decl].Writes ||
                                predecessor.Statement is ALocalDeclStm &&
                                ((ALocalDeclStm)predecessor.Statement).GetLocalDecl() == decl)
                            {
                                dontMove = true;
                                break;
                            }

                            //Cancel if any statement reads/writes to a variable used in the statement (where one of them writes)
                            if (declModData.Conflicts(predData))
                            {
                                dontMove = true;
                                break;
                            }

                            //Cancel if start of method is reached
                            if (predecessor.Predecessors.Count == 0)
                            {
                                dontMove = true;
                                break;
                            }

                            foreach (ControlFlowGraph.Node nextPred in predecessor.Predecessors)
                            {
                                if (visitedNodes.Contains(nextPred))
                                    continue;
                                nextNodes.AddLast(nextPred);
                            }
                        }

                        if (dontMove)
                            continue;

                        //Move it
                        cfg.Remove(node);
                        node.Statement.Parent().RemoveChild(node.Statement);
                        usedLvalue.Parent().ReplaceBy(exp.GetExp());

                        //We now need to redo liveness analysis
                        return true;
                    }
                }
            }
            return false;
        }

        private class GetUses : DepthFirstAdapter
        {
            public static bool HasMoreThanOneUses(PStm statement, AALocalDecl decl, SharedData data, out ALocalLvalue lvalue)
            {
                GetUses uses = new GetUses(decl, data);
                statement.Apply(uses);
                lvalue = uses.lvalue;
                return uses.count > 1;
            }

            private SharedData data;
            private AALocalDecl decl;
            private ALocalLvalue lvalue;
            private int count;


            private GetUses(AALocalDecl decl, SharedData data)
            {
                this.decl = decl;
                this.data = data;
            }

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                if (data.LocalLinks[node] == decl)
                {
                    lvalue = node;
                    count++;
                }
            }
        }

    }
}
