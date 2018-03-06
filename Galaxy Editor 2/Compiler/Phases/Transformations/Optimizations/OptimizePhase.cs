using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations.Tools;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class OptimizePhase : DepthFirstAdapter
    {
        //If an assigned local is only used once, and it is safe to move, move it

        private FinalTransformations finalTrans;
        private SharedData data { get { return finalTrans.data; } }
        private int methodCount = 0;
        private int methodNr = 0;
        private string prefix;

        public OptimizePhase(FinalTransformations finalTrans, string prefix)
        {
            this.prefix = prefix;
            this.finalTrans = finalTrans;
        }

        public override void InAAProgram(AAProgram node)
        {
            LocalChecker.CalculateMethodModify(node, data, out methodCount);
        }

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (data.AllowPrintouts)
                Form1.Form.SetStatusText(prefix + " [" + ++methodNr + "/" + methodCount + " " + node.GetName().Text + "]");
            if (node.GetName().Text.Contains("t2"))
                node = node;
            //Move locals to the start
            node.Apply(new MoveLocalsToStart(finalTrans.data));
            bool changes = true;
            while (changes)
            {
                changes = false;

                //Remove foo = foo
                RemoveSelfAssignments.Parse(node, data);

                //Create cfg
                ControlFlowGraph cfg = ControlFlowGraph.Create(node);
                RemoveDeadCode.Parse(cfg);
                LivenessAnalysis.CalculateLiveVariables(cfg, data);
                bool redoLivenessAnalysis;
                changes |= RemoveUnusedAssignments.Parse(cfg, data, out redoLivenessAnalysis);
                if (redoLivenessAnalysis)
                    LivenessAnalysis.CalculateLiveVariables(cfg, data);
                changes |= VariableJoiner.Parse(cfg, data);
                //This phase doesn't use liveness analysis
                changes |= RemoveUnusedLocals.Parse(cfg, data);


                while (true)
                {
                    bool changed = RemoveSingleUsedAssignments.Parse(cfg, data);
                    if (changed)
                    {
                        changes = true;
                        LivenessAnalysis.CalculateLiveVariables(cfg, data);
                        continue;
                    }
                    break;
                }


                changes |= StatementRemover.Parse(node);
            }
        }
    }
}
