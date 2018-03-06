using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class SetArrayIndexes : DepthFirstAdapter
    {
        private bool folding = false;
        private bool isANewExp = false;
        private int value = 0;
        private ErrorCollection errors;
        private SharedData data;

        public SetArrayIndexes(SharedData data, ErrorCollection errors)
        {
            this.data = data;
            this.errors = errors;
        }

        public override void OutAAProgram(AAProgram node)
        {
            foreach (KeyValuePair<AArrayLengthLvalue, AArrayTempType> pair in data.ArrayLengthTypes)
            {
                AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral(pair.Value.GetIntDim().Text));
                data.ExpTypes[intConst] = new ANamedType(new TIdentifier("int"), null);
                ALvalueExp exp = Util.GetAncestor<ALvalueExp>(pair.Key);
                exp.ReplaceBy(intConst);
            }

            base.OutAAProgram(node);
        }

        public override void CaseAArrayTempType(AArrayTempType node)
        {
            bool prevFolding = folding;
            int prevValue = value;
            bool wasANewExp = isANewExp;
            isANewExp = Util.HasAncestor<ANewExp>(node);
            if (!isANewExp)
            {
                folding = true;
                value = 0;
                base.CaseAArrayTempType(node);
                if (value <= 0)
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Array dimention must be greater than 0."));
                }
                node.SetIntDim(new TIntegerLiteral(value.ToString()));
            }
            folding = prevFolding;
            value = prevValue;
            isANewExp = wasANewExp;
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            if (node.GetDimention() != null)
            {
                bool prevFolding = folding;
                int prevValue = value;
                bool wasANewExp = isANewExp;
                folding = true;
                value = 0;
                node.GetDimention().Apply(this);
                node.SetIntDim(new TIntegerLiteral(value.ToString()));
                folding = prevFolding;
                value = prevValue;
                isANewExp = wasANewExp;
            }
            base.CaseAStructDecl(node);
        }

        public override void CaseAEnrichmentDecl(AEnrichmentDecl node)
        {
            if (node.GetDimention() != null)
            {
                bool prevFolding = folding;
                int prevValue = value;
                bool wasANewExp = isANewExp;
                folding = true;
                value = 0;
                node.GetDimention().Apply(this);
                node.SetIntDim(new TIntegerLiteral(value.ToString()));
                folding = prevFolding;
                value = prevValue;
                isANewExp = wasANewExp;
            }
            base.CaseAEnrichmentDecl(node);
        }

        public override void CaseAIntConstExp(AIntConstExp node)
        {
            if (folding)
                value = int.Parse(node.GetIntegerLiteral().Text);
        }

        public override void CaseABinopExp(ABinopExp node)
        {
            if (folding)
            {
                node.GetLeft().Apply(this);
                int left = value;
                node.GetRight().Apply(this);
                int right = value;
                if (node.GetBinop() is APlusBinop)
                    value = left + right;
                else if (node.GetBinop() is AMinusBinop)
                    value = left - right;
                else if (node.GetBinop() is ATimesBinop)
                    value = left * right;
                else if (node.GetBinop() is ADivideBinop)
                {
                    if (right == 0)
                    {
                        errors.Add(new ErrorCollection.Error(((ADivideBinop)node.GetBinop()).GetToken(), "Division by zero"), true);
                        throw new ParserException(null, "SetArrayIndexes.CaseABinopExp");
                    }
                    value = left / right;
                }
                else if (node.GetBinop() is AModuloBinop)
                {
                    if (right == 0)
                    {
                        errors.Add(new ErrorCollection.Error(((AModuloBinop)node.GetBinop()).GetToken(), "Division by zero"), true);
                        throw new ParserException(null, "EnviromentChecking.CaseABinopExp");
                    }
                    value = left % right;
                }
                else if (node.GetBinop() is AAndBinop)
                    value = left & right;
                else if (node.GetBinop() is AOrBinop)
                    value = left | right;
                else if (node.GetBinop() is AXorBinop)
                    value = left ^ right;
                else if (node.GetBinop() is ALBitShiftBinop)
                    value = left << right;
                else if (node.GetBinop() is ARBitShiftBinop)
                    value = left >> right;
            }
            else
                base.CaseABinopExp(node);
        }

        public override void OutAUnopExp(AUnopExp node)
        {
            if (folding)
            {
                if (node.GetUnop() is ANegateUnop)
                {
                    value = -value;
                }
               /* else
                    if (node.GetUnop() is AComplementUnop)
                        value = !value;*/
            }
            base.OutAUnopExp(node);
        }


        public override void OutAFieldLvalue(AFieldLvalue node)
        {
            if (folding)
            {
                AFieldDecl field = data.FieldLinks[node];
                //Must be int and must be const
                if (!(field.GetType() is ANamedType && ((ANamedType)field.GetType()).IsPrimitive("int")))
                {
                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must integer expressions.",
                                                  false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }

                if (field.GetConst() == null)
                {
                    if (!isANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(),
                                                      "Dimensions of array types must be constant expressions.",
                                                      false), true);
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }
                }
                if (field.GetInit() == null)//An error will be given earlier - constant fields must have an initializer
                {
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }
                field.GetInit().Apply(this);
            }
            else
                base.OutAFieldLvalue(node);
        }

        public override void OutALocalLvalue(ALocalLvalue node)
        {
            if (folding)
            {
                AALocalDecl local = data.LocalLinks[node];
                if (local.GetConst() == null)
                {
                    if (!isANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(),
                                                      "Dimensions of array types must be constant expressions.",
                                                      false), true);
                        throw new ParserException(null, null);
                    }
                }
                if (local.GetInit() == null)//An error will be given earlier
                    throw new ParserException(null, null);
                local.GetInit().Apply(this);
            }
        }

        public override void OutAStructFieldLvalue(AStructFieldLvalue node)
        {
            if (folding)
            {
                if (data.StructMethodPropertyLinks.ContainsKey(node))
                {

                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must be constant expressions.",
                                                  false,
                                                  new ErrorCollection.Error(
                                                      data.StructMethodPropertyLinks[node].GetName(),
                                                      "Matching property")), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAStructFieldLvalue");
                }
                AALocalDecl field = data.StructMethodFieldLinks[node];
                if (!(field.GetType() is ANamedType && ((ANamedType)field.GetType()).IsPrimitive("int")))
                {
                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must be integer expressions.",
                                                  false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAStructFieldLvalue");
                }

                if (field.GetConst() == null)
                {
                    if (!isANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(), 
                                                      "Dimensions of array types must be constant expressions.",
                                                      false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }
                }
                if (field.GetInit() == null)//An error will be given earlier - constant fields must have an initializer
                {
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }
                field.GetInit().Apply(this);
            }
        }

        public override void OutAIntConstExp(AIntConstExp node)
        {
            value = int.Parse(node.GetIntegerLiteral().Text);
            base.OutAIntConstExp(node);
        }

        /*public override void DefaultOut(Node node)
        {
            if ((node is PExp || node is PLvalue) && folding)
            {
                GetToken fetcher = new GetToken();
                node.Apply(fetcher);
                Token token = fetcher.Token;
                errors.Add(
                    new ErrorCollection.Error(token,
                                              "Dimensions of array types must be constant expressions."), true);
                throw new ParserException(token, "TypeLinking.OutAFieldLvalue");
            }
        }

        private class GetToken : DepthFirstAdapter
        {
            public Token Token = null;

            public override void CaseTIdentifier(TIdentifier node)
            {
                if (Token != null)
                    Token = node;
            }
        }*/

        //unop, struct field (x2), locals, fields^^
    }
}
