using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class MakeEnrichmentLinks : DepthFirstAdapter
    {
        private SharedData data;
        private ErrorCollection errors;

        public MakeEnrichmentLinks(SharedData data, ErrorCollection errors)
        {
            this.data = data;
            this.errors = errors;
        }

        public override void InAArrayTempType(AArrayTempType node)
        {
            if (node.GetIntDim() == null && !Util.HasAncestor<ANewExp>(node))
            {
                node = node;
                /*bool valid = true;
                int val = FoldInt(node.GetDimention(), ref valid);
                if (!valid)
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "The dimension must be a constant expression."));
                    throw new ParserException(null, null);
                }
                node.SetIntDim(new TIntegerLiteral(val.ToString()));*/
            }
            base.InAArrayTempType(node);
        }

        private int FoldInt(PExp exp, ref bool valid)
        {
            if (!valid) return -1;
            if (exp is AIntConstExp)
            {
                return int.Parse(((AIntConstExp) exp).GetIntegerLiteral().Text);
            }
            if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp)exp;
                int left = FoldInt(aExp.GetLeft(), ref valid);
                int right = FoldInt(aExp.GetLeft(), ref valid);
                if (!valid) return -1;
                PBinop binop = aExp.GetBinop();
                if (binop is APlusBinop)
                    return left + right;
                if (binop is AMinusBinop)
                    return left - right;
                if (binop is ATimesBinop)
                    return left * right;
                if ((binop is AModuloBinop || binop is ADivideBinop) && right == 0)
                {
                    Token token = binop is AModuloBinop
                                      ? (Token)((AModuloBinop) binop).GetToken()
                                      : ((ADivideBinop) binop).GetToken();
                    errors.Add(new ErrorCollection.Error(token, "Zero division during constant folding."));
                    throw new ParserException(null, null);
                }
                if (binop is AModuloBinop)
                    return left % right;
                if (binop is ADivideBinop)
                    return left / right;
                if (binop is AAndBinop)
                    return left & right;
                if (binop is AOrBinop)
                    return left | right;
                if (binop is AXorBinop)
                    return left ^ right;
                if (binop is ALBitShiftBinop)
                    return left << right;
                if (binop is ARBitShiftBinop)
                    return left >> right;
            }
            if (exp is ALvalueExp)
                return FoldInt(((ALvalueExp) exp).GetLvalue(), ref valid);


            valid = false;
            return -1;
        }

        private int FoldInt(PLvalue lvalue, ref bool valid)
        {
            if (!valid) return -1;

            if (lvalue is ALocalLvalue)
            {
                ALocalLvalue aLvalue = (ALocalLvalue) lvalue;
                AALocalDecl decl = data.LocalLinks[aLvalue];
                if (decl.GetConst() == null)
                {
                    valid = false;
                    return -1;
                }
                return FoldInt(decl.GetInit(), ref valid);
            }
            if (lvalue is AFieldLvalue)
            {
                AFieldLvalue aLvalue = (AFieldLvalue) lvalue;
                AFieldDecl decl = data.FieldLinks[aLvalue];
                if (decl.GetConst() == null)
                {
                    valid = false;
                    return -1;
                }
                return FoldInt(decl.GetInit(), ref valid);
            }
            if (lvalue is AStructFieldLvalue)
            {
                AStructFieldLvalue aLvalue = (AStructFieldLvalue)lvalue;
                AALocalDecl decl = data.StructMethodFieldLinks[aLvalue];
                if (decl.GetConst() == null)
                {
                    valid = false;
                    return -1;
                }
                return FoldInt(decl.GetInit(), ref valid);
            }
            if (lvalue is AStructLvalue)
            {
                AStructLvalue aLvalue = (AStructLvalue)lvalue;
                AALocalDecl decl = data.StructFieldLinks[aLvalue];
                if (decl.GetConst() == null)
                {
                    valid = false;
                    return -1;
                }
                return FoldInt(decl.GetInit(), ref valid);
            }

            valid = false;
            return -1;
        }

        public override void DefaultOut(Node node)
        {
            if (node is PType)
            {
                Link((PType) node, node);   
            }
            else if (node is PExp)
            {
                Link(data.ExpTypes[(PExp) node], node);
            } 
            else if (node is PLvalue && !(node.Parent() is ASyncInvokeExp || node.Parent() is AAsyncInvokeStm))
            {
                Link(data.LvalueTypes[(PLvalue)node], node);
            }
            base.DefaultOut(node);
        }

        public override void CaseADelegateExp(ADelegateExp node)
        {
            node.GetType().Apply(this);
            DefaultOut(node);
        }

        private void Link(PType type, Node node)
        {
            if (data.EnrichmentTypeLinks.ContainsKey(type))
                return;
            List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
            foreach (IList declList in visibleDecls)
            {
                foreach (PDecl decl in declList)
                {
                    if (decl is AEnrichmentDecl)
                    {
                        AEnrichmentDecl enrichment = (AEnrichmentDecl) decl;
                        if (!Util.TypesEqual(type, enrichment.GetType(), data))
                            continue;
                        data.EnrichmentTypeLinks[type] = enrichment;
                        break;
                    }
                }
            }

        }
    }
}
