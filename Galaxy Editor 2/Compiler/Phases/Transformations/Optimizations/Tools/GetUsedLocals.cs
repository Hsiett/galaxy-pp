using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations.Tools
{
    class GetUsedLocals : DepthFirstAdapter
    {
        public static List<AALocalDecl> Parse(Node node, SharedData data)
        {
            GetUsedLocals getter = new GetUsedLocals(data);
            node.Apply(getter);
            return getter.UsedLocals;
        }

        private SharedData data;

        private GetUsedLocals(SharedData data)
        {
            this.data = data;
        }

        List<AALocalDecl> UsedLocals = new List<AALocalDecl>();

        public override void CaseALocalLvalue(ALocalLvalue node)
        {
            AALocalDecl decl = data.LocalLinks[node];
            if (!UsedLocals.Contains(decl))
                UsedLocals.Add(decl);
        }
    }
}
