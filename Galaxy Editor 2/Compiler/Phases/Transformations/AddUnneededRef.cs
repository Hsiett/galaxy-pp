using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    //Inlining methods work better if parameters are marked as ref.
    //But it's disabled - if I mark it as ref, the caller must be a lvalueExp
    class AddUnneededRef : DepthFirstAdapter
    {
        private SharedData data;

        public AddUnneededRef(SharedData data)
        {
            this.data = data;
        }

        List<AALocalDecl> assignedToLocals = new List<AALocalDecl>();
        private bool isLeftsideOfAssignment;

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (node.GetInline() == null)
                return;

            //Variables marked as out can be made into ref
            foreach (AALocalDecl formal in node.GetFormals())
            {
                if (formal.GetOut() != null)
                {
                    formal.SetRef(new TRef("ref"));
                    formal.SetOut(null);
                }
            }
            assignedToLocals.Clear();
            base.CaseAMethodDecl(node);

            foreach (AALocalDecl formal in node.GetFormals())
            {
                if (!assignedToLocals.Contains(formal))
                    formal.SetRef(new TRef("ref"));
            }

        }

        public override void CaseALocalLvalue(ALocalLvalue node)
        {
            if (isLeftsideOfAssignment)
                assignedToLocals.Add(data.LocalLinks[node]);
        }

        public override void CaseAArrayLvalue(AArrayLvalue node)
        {
            node.GetBase().Apply(this);
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            isLeftsideOfAssignment = true;
            node.GetLvalue().Apply(this);
            isLeftsideOfAssignment = false;
        }

    }
}
