using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes
{
    class GetNonBlockStm : DepthFirstAdapter
    {
        public PStm Stm = null;
        private bool first;

        public GetNonBlockStm(bool first)
        {
            this.first = first;
        }

        public override void DefaultIn(Node node)
        {
            if ((!first || Stm == null) && node is PStm && !(node is ABlockStm))
                Stm = (PStm)node;
        }
    }
}
