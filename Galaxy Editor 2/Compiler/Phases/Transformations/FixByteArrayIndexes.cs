using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class FixByteArrayIndexes : DepthFirstAdapter
    {
        private SharedData data;

        public FixByteArrayIndexes(SharedData data)
        {
            this.data = data;
        }

        public override void CaseAIntConstExp(AIntConstExp node)
        {
            containsLiteral = true;
        }

        private bool containsLiteral;
        public override void CaseAArrayLvalue(AArrayLvalue node)
        {
            bool containedLiteral = containsLiteral;
            containsLiteral = false;
            node.GetIndex().Apply(this);
            bool hasLiteral = containsLiteral;
            containsLiteral = containedLiteral;
            PType type = data.ExpTypes[node.GetIndex()];
            if (!hasLiteral && type is ANamedType && ((ANamedType)type).IsPrimitive("byte"))
            {
                AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral("0"));
                ABinopExp binop = new ABinopExp(node.GetIndex(), new APlusBinop(new TPlus("+")), intConst);
                data.ExpTypes[intConst] =
                    data.ExpTypes[binop] = new ANamedType(new TIdentifier("int"), null);
                node.SetIndex(binop);
            }
            node.GetBase().Apply(this);
        }
    }
}
