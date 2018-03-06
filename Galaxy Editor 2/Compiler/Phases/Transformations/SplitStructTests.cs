using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class SplitStructTests : DepthFirstAdapter
    {
        private SharedData data;

        public SplitStructTests(SharedData data)
        {
            this.data = data;
        }

        public override void OutABinopExp(ABinopExp node)
        {
            if (data.ExpTypes[node.GetLeft()] is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)data.ExpTypes[node.GetLeft()]))
            {
                AStructDecl str = data.StructTypeLinks[(ANamedType)data.ExpTypes[node.GetLeft()]];
                PExp replacementExp = new ABooleanConstExp(new ATrueBool());
                data.ExpTypes[replacementExp] = new ANamedType(new TIdentifier("bool"), null);
                foreach (AALocalDecl local in str.GetLocals().OfType<AALocalDecl>())
                {
                    AStructLvalue leftSide = new AStructLvalue(Util.MakeClone(node.GetLeft(), data),
                                                               new ADotDotType(new TDot(".")),
                                                               new TIdentifier(local.GetName().Text));
                    ALvalueExp leftSideExp = new ALvalueExp(leftSide);
                    AStructLvalue rightSide = new AStructLvalue(Util.MakeClone(node.GetRight(), data),
                                                               new ADotDotType(new TDot(".")),
                                                               new TIdentifier(local.GetName().Text));
                    ALvalueExp rightSideExp = new ALvalueExp(rightSide);


                    data.StructFieldLinks[leftSide] =
                        data.StructFieldLinks[rightSide] = local;
                    data.LvalueTypes[leftSide] =
                        data.ExpTypes[leftSideExp] =
                        data.LvalueTypes[rightSide] =
                        data.ExpTypes[rightSideExp] = local.GetType();


                    ABinopExp binop = new ABinopExp(leftSideExp, (PBinop)node.GetBinop().Clone(), rightSideExp);
                    data.ExpTypes[binop] = data.ExpTypes[node];

                    if (replacementExp is ABooleanConstExp)
                        replacementExp = binop;
                    else
                    {
                        replacementExp = new ABinopExp(replacementExp, new ALazyAndBinop(new TAndAnd("&&")), binop);
                        data.ExpTypes[replacementExp] = new ANamedType(new TIdentifier("bool"), null);
                    }
                }
                node.ReplaceBy(replacementExp);
                replacementExp.Apply(this);
            }

            base.OutABinopExp(node);
        }
    }
}
