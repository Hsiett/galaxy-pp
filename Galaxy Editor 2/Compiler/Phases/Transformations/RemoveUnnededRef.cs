using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveUnnededRef : DepthFirstAdapter
    {
        private SharedData data;

        public RemoveUnnededRef(SharedData data)
        {
            this.data = data;
        }

        List<AMethodDecl> parsedMethods = new List<AMethodDecl>();
        List<AMethodDecl> methodChain = new List<AMethodDecl>();
        Dictionary<AMethodDecl, List<AALocalDecl>> NeededRefs = new Dictionary<AMethodDecl, List<AALocalDecl>>();

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (parsedMethods.Contains(node) || methodChain.Contains(node))
                return;
            methodChain.Add(node);
            NeededRefs.Add(node, new List<AALocalDecl>());
            base.CaseAMethodDecl(node);
            methodChain.Remove(node);
            parsedMethods.Add(node);
            foreach (AALocalDecl formal in node.GetFormals())
            {
                if (formal.GetRef() != null && !NeededRefs[node].Contains(formal) && !Util.IsBulkCopy(formal.GetType()))
                    formal.SetRef(null);
            }

        }

        private ALocalLvalue currentLocal;
        public override void CaseALocalLvalue(ALocalLvalue node)
        {
            currentLocal = node;
        }

        public override void CaseAArrayLvalue(AArrayLvalue node)
        {
            node.GetIndex().Apply(this);
            currentLocal = null;
            node.GetBase().Apply(this);
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            currentLocal = null;
            node.GetLvalue().Apply(this);
            if (currentLocal != null)
            {
                AALocalDecl decl = data.LocalLinks[currentLocal];
                NeededRefs[Util.GetAncestor<AMethodDecl>(node)].Add(decl);
            }
            node.GetExp().Apply(this);
        }

        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            AMethodDecl target = data.SimpleMethodLinks[node];
            for (int i = 0; i < node.GetArgs().Count; i++)
            {
                PExp arg = (PExp)node.GetArgs()[i];
                currentLocal = null;
                arg.Apply(this);
                if (currentLocal != null && target.GetFormals().Cast<AALocalDecl>().ToList()[i].GetRef() != null)
                {
                    ALocalLvalue local = currentLocal;
                    target.Apply(this);
                    if (target.GetFormals().Cast<AALocalDecl>().ToList()[i].GetRef() != null)
                    {
                        AALocalDecl decl = data.LocalLinks[local];
                        NeededRefs[Util.GetAncestor<AMethodDecl>(node)].Add(decl);
                    }
                }
            }
        }
    }
}
