using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using ASTNode = Galaxy_Editor_2.Compiler.Generated.node.Node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class ControlFlowGraph
    {
        public class Node
        {
            public List<Node> Predecessors = new List<Node>();
            public List<Node> Successors = new List<Node>();
            public PStm Statement; 

            public PExp Expression
            {
                get
                {
                    if (Statement is AExpStm)
                        return ((AExpStm)Statement).GetExp();
                    if (Statement is ALocalDeclStm)
                        return ((AALocalDecl)((ALocalDeclStm)Statement).GetLocalDecl()).GetInit();
                    if (Statement is AWhileStm)
                        return ((AWhileStm)Statement).GetCondition();
                    if (Statement is AIfThenStm)
                        return ((AIfThenStm)Statement).GetCondition();
                    if (Statement is AIfThenElseStm)
                        return ((AIfThenElseStm)Statement).GetCondition();
                    if (Statement is AValueReturnStm)
                        return ((AValueReturnStm) Statement).GetExp();
                    return null;
                }
            }

            //Added by needed phases
            public List<AALocalDecl> LiveVariables = new List<AALocalDecl>();

            

            public Node(PStm statement)
            {
                Statement = statement;
            }

            public void AddSucc(Node node)
            {
                if (!Successors.Contains(node))
                {
                    Successors.Add(node);
                    node.AddPred(this);
                }
            }

            public void AddPred(Node node)
            {
                if (!Predecessors.Contains(node))
                {
                    Predecessors.Add(node);
                    node.AddSucc(this);
                }
            }

            public void RemoveSucc(Node node)
            {
                if (Successors.Contains(node))
                {
                    Successors.Remove(node);
                    node.RemovePred(this);
                }
            }

            public void RemovePred(Node node)
            {
                if (Predecessors.Contains(node))
                {
                    Predecessors.Remove(node);
                    node.RemoveSucc(this);
                }
            }

        }

        public static ControlFlowGraph Create(AMethodDecl method)
        {
            ControlFlowGraph graph = new ControlFlowGraph(method);
            CFGGenerator generator = new CFGGenerator();
            method.GetBlock().Apply(generator);
            graph.Nodes = new List<Node>(generator.Nodes.Count);
            graph.Nodes.AddRange(generator.Nodes);
            return graph;
        }

        public AMethodDecl Method;
        public List<Node> Nodes;

        private ControlFlowGraph(AMethodDecl method)
        {
            Method = method;
        }

        public void Remove(Node node)
        {
            Nodes.Remove(node);
            //Set succ/pred of succ/pred nodes
            foreach (Node predecessor in node.Predecessors)
            {
                foreach (Node successor in node.Successors)
                {
                    predecessor.AddSucc(successor);
                }
            }
            //Remove node from succ/pred lists
            for (int i = node.Predecessors.Count - 1; i >= 0; i--)
            {
                Node predecessor = node.Predecessors[i];
                node.RemovePred(predecessor);
            }
            for (int i = node.Successors.Count - 1; i >= 0; i--)
            {
                Node successor = node.Successors[i];
                node.RemoveSucc(successor);
            }
        }

        public Node Insert(Node node, AExpStm newStm)
        {
            //Node must be a node for an AExpStm
            Node newNode = new Node(newStm);
            foreach (Node successor in node.Successors)
            {
                newNode.AddSucc(successor);
            }
            foreach (Node successor in newNode.Successors)
            {
                node.RemoveSucc(successor);
            }
            node.AddSucc(newNode);
            Nodes.Insert(Nodes.IndexOf(node) + 1, newNode);
            return newNode;
        }

        private class CFGGenerator : DepthFirstAdapter
        {
            private Dictionary<PStm, Node> StatementNodes = new Dictionary<PStm, Node>();
            public LinkedList<Node> Nodes = new LinkedList<Node>();

            private Node GetNode(PStm node)
            {
                if (StatementNodes.ContainsKey(node))
                    return StatementNodes[node];
                Node graphNode = new Node(node);
                Nodes.AddLast(graphNode);
                StatementNodes[node] = graphNode;
                return graphNode;
            }

            private static List<PStm> GetLast(PStm stm)
            {
                List<PStm> returner = new List<PStm>();
                if (stm is ABlockStm)
                {
                    AABlock block = (AABlock) ((ABlockStm) stm).GetBlock();
                    stm = null;
                    for (int i = block.GetStatements().Count - 1; i >= 0; i--)
                    {
                        returner.AddRange(GetLast((PStm) block.GetStatements()[i]));
                        if (returner.Count > 0)
                            return returner;
                    }
                }
                else if (stm is AIfThenElseStm)
                {
                    List<PStm> stms = new List<PStm>();
                    stms.AddRange(GetLast(((AIfThenElseStm)stm).GetThenBody()));
                    bool zero = stms.Count == 0;
                    returner.AddRange(stms);
                    stms.Clear();
                    stms.AddRange(GetLast(((AIfThenElseStm)stm).GetElseBody()));
                    zero |= stms.Count == 0;
                    returner.AddRange(stms);
                    if (zero)
                        returner.Add(stm);
                }
                else
                    returner.Add(stm);
                return returner;
            }

            private static PStm GetFirst(PStm stm)
            {
                if (stm is ABlockStm)
                {
                    AABlock block = (AABlock)((ABlockStm)stm).GetBlock();
                    stm = null;
                    for (int i = 0; i < block.GetStatements().Count; i++)
                    {
                        stm = GetFirst((PStm)block.GetStatements()[i]);
                        if (stm != null)
                            return stm;
                    }
                }
                return stm;
            }

            private static PStm GetNext(PStm stm)
            {
                while (true)
                {
                    AABlock pBlock = Util.GetAncestor<AABlock>(stm);
                    if (pBlock == null)
                        return null;
                    int index = pBlock.GetStatements().IndexOf(stm);
                    while (index < pBlock.GetStatements().Count - 1)
                    {
                        stm = GetFirst((PStm)pBlock.GetStatements()[index + 1]);
                        if (stm != null)
                            return stm;
                        index++;
                    }
                    ASTNode node = pBlock;
                    stm = null;
                    while (true)
                    {
                        if (node == null)
                            return null;
                        node = Util.GetNearestAncestor(node.Parent(), typeof(AABlock), typeof(PStm));
                        if (node is PStm)
                            stm = (PStm)node;
                        else if (stm == null)
                            continue;
                        else
                            break;
                    }
                    //stm = Util.GetAncestor<PStm>(pBlock);
                    if (stm == null)
                        return null;
                }
            }

            public override void CaseAExpStm(AExpStm node)
            {
                //Create node
                GetNode(node);
            }

            public override void CaseALocalDeclStm(ALocalDeclStm node)
            {
                //Create node
                GetNode(node);
            }

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                GetNode(node);
            }

            public override void CaseAVoidReturnStm(AVoidReturnStm node)
            {
                GetNode(node);
            }

            public override void CaseAABlock(AABlock node)
            {
                //Craete nodes
                base.CaseAABlock(node);
                //Set pred/succ
                for (int i = 0; i < node.GetStatements().Count - 1; i++)
                {
                    List<PStm> stms = GetLast((PStm)node.GetStatements()[i]);
                    for (int j = i - 1; stms.Count == 0 && j >= 0; j--)
                        stms = GetLast((PStm)node.GetStatements()[j]);
                    
                    PStm stm2 = GetFirst((PStm)node.GetStatements()[i + 1]);
                    for (int j = i + 2; stm2 == null && j < node.GetStatements().Count; j++)
                        stm2 = GetFirst((PStm)node.GetStatements()[j]);
                    foreach (PStm stm1 in stms)
                    {
                        if (stm1 == null || stm2 == null)
                            continue;
                        if (stm1 is AIfThenElseStm ||
                            stm1 is AValueReturnStm ||
                            stm1 is AVoidReturnStm ||
                            stm1 is AContinueStm ||
                            stm1 is ABreakStm)
                            continue;
                        GetNode(stm1).AddSucc(GetNode(stm2));
                    }
                }
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                //Create graph node
                Node graphNode = GetNode(node);
                //Processes inner while
                node.GetBody().Apply(this);
                //Add successor and predessors
                PStm stm = GetFirst(node.GetBody());
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));
                List<PStm> stms = GetLast(node.GetBody());
                foreach (PStm pStm in stms)
                {
                    graphNode.AddPred(GetNode(pStm));
                }
            }

            public override void CaseAContinueStm(AContinueStm node)
            {
                Node graphNode = GetNode(node);
                AWhileStm whileStm = Util.GetAncestor<AWhileStm>(node);
                graphNode.AddSucc(GetNode(whileStm));
            }

            public override void CaseABreakStm(ABreakStm node)
            {
                Node graphNode = GetNode(node);
                AWhileStm whileStm = Util.GetAncestor<AWhileStm>(node);
                PStm stm = GetNext(whileStm);
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));
            }

            /*public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                GetNode(node);
                PStm stm = GetNext(whileStm);
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));
            }*/

            public override void CaseAIfThenStm(AIfThenStm node)
            {
                //Create graph node
                Node graphNode = GetNode(node);
                //Process inner if
                node.GetBody().Apply(this);
                //Add successor and predessors
                PStm stm = GetFirst(node.GetBody());
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));
                stm = GetNext(node);
                if (stm != null)
                {
                    Node nextGraphNode = GetNode(stm);
                    graphNode.AddSucc(nextGraphNode);
                    List<PStm> stms = GetLast(node.GetBody());
                    foreach (PStm pStm in stms)
                    {
                        nextGraphNode.AddPred(GetNode(pStm));
                    }
                }
            }

            private bool continues = true;
            public override void CaseAIfThenElseStm(AIfThenElseStm node)
            {
                //Create graph node
                Node graphNode = GetNode(node);
                //Process inner if
                node.GetThenBody().Apply(this);
                node.GetElseBody().Apply(this);
                //Add successor and predessors
                PStm stm = GetFirst(node.GetThenBody());
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));
                stm = GetFirst(node.GetElseBody());
                if (stm != null)
                    graphNode.AddSucc(GetNode(stm));

                stm = GetNext(node);
                if (stm != null)
                {
                    Node nextGraphNode = GetNode(stm);

                    List<PStm> stms = GetLast(node.GetThenBody());
                    if (stms.Count > 0)
                        foreach (PStm pStm in stms)
                        {
                            nextGraphNode.AddPred(GetNode(pStm));
                        }
                    else
                        graphNode.AddSucc(nextGraphNode);

                    stms = GetLast(node.GetElseBody());
                    if (stms.Count > 0)
                        foreach (PStm pStm in stms)
                        {
                            nextGraphNode.AddPred(GetNode(pStm));
                        }
                    else
                        graphNode.AddSucc(nextGraphNode);
                }
            }

            
        }
    }
}
