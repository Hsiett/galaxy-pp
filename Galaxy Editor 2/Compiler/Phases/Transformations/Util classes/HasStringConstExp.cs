using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes
{
    class HasStringConstExp : DepthFirstAdapter
    {
        public List<AStringConstExp> List = new List<AStringConstExp>();
        public bool HasStringConst;

        public override void CaseAStringConstExp(AStringConstExp node)
        {
            List.Add(node);
            HasStringConst = true;
        }
    }
}
