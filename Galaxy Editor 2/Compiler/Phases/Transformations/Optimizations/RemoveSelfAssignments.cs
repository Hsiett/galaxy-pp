using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class RemoveSelfAssignments : DepthFirstAdapter
    {
        public static bool Parse(AMethodDecl method, SharedData data)
        {
            RemoveSelfAssignments remover = new RemoveSelfAssignments(data);
            method.GetBlock().Apply(remover);
            return remover.changedSomething;
        }

        private SharedData data;
        private bool changedSomething = false;

        private RemoveSelfAssignments(SharedData data)
        {
            this.data = data;
        }

        public bool RemovedOne;
        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            if (node.GetExp() is ALvalueExp && Util.ReturnsTheSame(node.GetLvalue(), ((ALvalueExp)node.GetExp()).GetLvalue(), data))
            {
                RemovedOne = true;
                node.Parent().Parent().RemoveChild(node.Parent());
            }
            else
                base.CaseAAssignmentExp(node);
        }
    }
}
