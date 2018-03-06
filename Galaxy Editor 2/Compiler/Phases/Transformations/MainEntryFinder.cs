using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class MainEntryFinder : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public MainEntryFinder(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }
        private List<ErrorCollection.Error> multipleEntryCandidates = new List<ErrorCollection.Error>();

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (node.GetName().Text == "InitMap" && node.GetFormals().Count == 0)
            {
                if (finalTrans.multipleMainEntries)
                {
                    multipleEntryCandidates.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node.GetName()), "Candidate"));
                }
                else if (finalTrans.mainEntry != null)
                {
                    multipleEntryCandidates.Add(new ErrorCollection.Error(finalTrans.mainEntry.GetName(), Util.GetAncestor<AASourceFile>(finalTrans.mainEntry.GetName()), "Candidate"));
                    multipleEntryCandidates.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node.GetName()), "Candidate"));
                    //finalTrans.errors.Add(new ErrorCollection.Error(node.GetName(), Util.GetAncestor<AASourceFile>(node), "Found multiple candidates for a main entry", true));
                    finalTrans.multipleMainEntries = true;
                    finalTrans.mainEntry = null;
                }
                else
                    finalTrans.mainEntry = node;
            }
            base.CaseAMethodDecl(node);
        }

        public override void OutAAProgram(AAProgram node)
        {
            if (multipleEntryCandidates.Count > 0)
            {
                finalTrans.errors.Add(new ErrorCollection.Error(multipleEntryCandidates[0], "Found multiple candidates for the main entry", multipleEntryCandidates.ToArray()));
            }
        }
    }
}
