using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RenameRefferences : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        private SharedData data
        {
        get { return finalTrans.data; }
        }

        public RenameRefferences(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        //Remove unnessery blocks
        public override void OutABlockStm(ABlockStm node)
        {
            if (node.Parent() is AABlock)
            {
                AABlock pBlock = (AABlock)node.Parent();
                AABlock cBlock = (AABlock)node.GetBlock();
                int index = pBlock.GetStatements().IndexOf(node);
                pBlock.RemoveChild(node);
                for (int i = cBlock.GetStatements().Count - 1; i >= 0; i--)
                {
                    pBlock.GetStatements().Insert(index, cBlock.GetStatements()[i]);
                }
            }
        }

        //Join string + string to string
        public override void CaseABinopExp(ABinopExp node)
        {
            if (node.GetBinop() is APlusBinop)
            {
                PType type = data.ExpTypes[node];
                if (type is ANamedType && ((ANamedType)type).IsPrimitive("string"))
                {
                    PExp other = null;
                    if (node.GetLeft() is ANullExp)
                        other = node.GetRight();
                    if (node.GetRight() is ANullExp)
                        other = node.GetLeft();
                    if (other != null)
                    {
                        node.ReplaceBy(other);
                        other.Apply(this);
                        return;
                    }
                }


                //Case (string + string)
                if (node.GetLeft() is AStringConstExp && node.GetRight() is AStringConstExp)
                {
                    AStringConstExp left = (AStringConstExp) node.GetLeft();
                    AStringConstExp right = (AStringConstExp) node.GetRight();

                    if (!IsJoinAllowed(left.GetStringLiteral().Text, right.GetStringLiteral().Text))
                    {
                        base.CaseABinopExp(node);
                        return;
                    }
                    left.GetStringLiteral().Text = left.GetStringLiteral().Text.Substring(0,
                                                                                          left.GetStringLiteral().Text.
                                                                                              Length - 1);
                    left.GetStringLiteral().Text += right.GetStringLiteral().Text.Substring(1);
                    node.ReplaceBy(left);
                    CaseAStringConstExp(left);
                    return;
                }
                //Case (<exp> + string) + string
                if (node.GetLeft() is ABinopExp && node.GetRight() is AStringConstExp)
                {
                    ABinopExp leftBinop = (ABinopExp) node.GetLeft();
                    if (leftBinop.GetBinop() is APlusBinop && leftBinop.GetRight() is AStringConstExp)
                    {
                        AStringConstExp left = (AStringConstExp) leftBinop.GetRight();
                        AStringConstExp right = (AStringConstExp) node.GetRight();
                        if (!IsJoinAllowed(left.GetStringLiteral().Text, right.GetStringLiteral().Text))
                        {
                            base.CaseABinopExp(node);
                            return;
                        }
                        left.GetStringLiteral().Text = left.GetStringLiteral().Text.Substring(0,
                                                                                              left.GetStringLiteral().
                                                                                                  Text.
                                                                                                  Length - 1);
                        left.GetStringLiteral().Text += right.GetStringLiteral().Text.Substring(1);
                        node.ReplaceBy(leftBinop);
                        CaseABinopExp(leftBinop);
                        return;
                    }
                }
                //Case string + (string + <exp>)
                //Case (<exp> + string) + (string + <exp>)


            }
            //Case (int + int)
            /*if (node.GetLeft() is AIntConstExp && node.GetRight() is AIntConstExp)
            {
                AIntConstExp left = (AIntConstExp) node.GetLeft();
                AIntConstExp right = (AIntConstExp) node.GetRight();

                int a = int.Parse(left.GetIntegerLiteral().Text);
                int b = int.Parse(right.GetIntegerLiteral().Text);

                if (node.GetBinop() is APlusBinop)
                {
                    a += b;
                }
                else if (node.GetBinop() is AMinusBinop)
                {
                    a -= b;
                }
                else if (node.GetBinop() is ATimesBinop)
                {
                    a *= b;
                }
                else if (node.GetBinop() is ADivideBinop)
                {
                    if (b == 0)
                    {
                        base.CaseABinopExp(node);
                        return;
                    }
                    a /= b;
                }
                else
                {
                    base.CaseABinopExp(node);
                    return;
                }

                left.GetIntegerLiteral().Text = a.ToString();
                node.ReplaceBy(left);
                left.Apply(this);
                return;
            }
            //Case (<exp> + int) + int
            if (node.GetLeft() is ABinopExp && node.GetRight() is AIntConstExp && (node.GetBinop() is APlusBinop || node.GetBinop() is AMinusBinop))
            {
                ABinopExp leftBinop = (ABinopExp) node.GetLeft();
                PType leftType = data.ExpTypes[leftBinop];
                if (leftBinop.GetRight() is AIntConstExp && leftType is ANamedType && ((ANamedType) leftType).GetName().Text == "int" &&
                     (leftBinop.GetBinop() is APlusBinop || leftBinop.GetBinop() is AMinusBinop))
                {
                    AIntConstExp left = (AIntConstExp)leftBinop.GetRight();
                    AIntConstExp right = (AIntConstExp)node.GetRight();
                    int a = int.Parse(left.GetIntegerLiteral().Text);
                    int b = int.Parse(right.GetIntegerLiteral().Text);

                    if (node.GetBinop() is APlusBinop)
                    {
                        if (leftBinop.GetBinop() is APlusBinop)
                        {
                            //(<exp> + int) + int
                            int c = a + b;
                            //Test for overflow
                            if (a > 0 && b > 0 && (c < a || c < b) ||
                                a < 0 && b < 0 && (c > a || c > b))
                            {
                                //Don't add them
                                base.CaseABinopExp(node);
                                return;
                            }
                            if (c < 0)
                            {
                                //Change binop to <exp> - c
                                if (c != int.MinValue)
                                {
                                    c = -c;
                                    leftBinop.SetBinop(new AMinusBinop(new TMinus("-")));
                                }
                            }
                            //Replace node with leftbinop
                            left.GetIntegerLiteral().Text = c.ToString();
                            node.ReplaceBy(leftBinop);
                            leftBinop.Apply(this);
                            return;
                        }
                        else
                        {
                            //(<exp> - int) + int
                            int c = b - a;
                            //Test for overflow
                            if (a < 0 && b > 0 && (c < a || c < b) ||
                                a > 0 && b < 0 && (c > a || c > b))
                            {
                                //Don't add them
                                base.CaseABinopExp(node);
                                return;
                            }
                            if (c > 0 || c == int.MinValue)
                            {
                                //Change binop to <exp> + c
                                leftBinop.SetBinop(new APlusBinop(new TPlus("+")));

                            }
                            else
                                c = -c;
                            //Replace node with leftbinop
                            left.GetIntegerLiteral().Text = c.ToString();
                            node.ReplaceBy(leftBinop);
                            leftBinop.Apply(this);
                            return;
                        }
                    }
                    else
                    {
                        if (leftBinop.GetBinop() is APlusBinop)
                        {
                            //(<exp> + int) - int
                            //ALso need to consider <exp> in the other position, and int on the other side of the binop
                            //Make a more general algorithm
                        }
                        else
                        {

                        }
                    }
                }
            }*/
            base.CaseABinopExp(node);
        }

        private bool IsJoinAllowed(string left, string right)
        {
            left = left.Substring(1, left.Length - 2);
            right = right.Substring(1, right.Length - 2);
            if (right.Length == 0 || ((right[0] < '0' || right[0] > '9') && (char.ToLower(right[0]) < 'a' || char.ToLower(right[0]) > 'f')))
                return true;
            bool hexValid = true;
            bool octValid = true;
            bool expectHexEnd = false;
            for (int i = left.Length - 1; i >= 0; i--)
            {
                if (!hexValid && !octValid)
                    break;
                int didgit = left.Length - i;//1, 2, 3...
                char c = left[i];
                switch (didgit)
                {
                    case 1:
                        if (!(c >= '0' && c <= '7'))
                            octValid = false;
                        if (!((c >= '0' && c <= '7') || (char.ToLower(c) >= 'a' && char.ToLower(c) <= 'f')))
                            hexValid = false;
                        break;
                    case 2:
                        if (octValid && c == '\\')
                            return false;
                        if (!((c >= '0' && c <= '7')))
                            octValid = false;
                        if (hexValid && c == 'x')
                            expectHexEnd = true;
                        else if (!((c >= '0' && c <= '7') || (char.ToLower(c) >= 'a' && char.ToLower(c) <= 'f')))
                            hexValid = false;
                        break;
                    case 3:
                        if ((octValid || (hexValid && expectHexEnd)) && c == '\\')
                            return false;
                        if (!((c >= '0' && c <= '3')))
                            octValid = false;
                        if (hexValid && !expectHexEnd && c == 'x')
                            expectHexEnd = true;
                        else
                            hexValid = false;
                        break;
                    case 4:
                        if ((octValid || (hexValid && expectHexEnd)) && c == '\\')
                            return false;
                        hexValid = false;
                        octValid = false;
                        break;
                }
            }
            return true;
        }

        public override void OutABinopExp(ABinopExp node)
        {
            if (node.Parent() is ABinopExp || node.Parent() is AUnopExp)
            {
                AParenExp paren = new AParenExp();
                node.ReplaceBy(paren);
                paren.SetExp(node);

                finalTrans.data.ExpTypes[paren] = finalTrans.data.ExpTypes[node];
            }
            base.OutABinopExp(node);
        }

        //Rename method invocations
        public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
        {
            AMethodDecl method = finalTrans.data.SimpleMethodLinks[node];
            if (method == null)
                method = null;
            node.GetName().Text = finalTrans.data.SimpleMethodLinks[node].GetName().Text;
        }

        public override void OutALocalLvalue(ALocalLvalue node)
        {
            node.GetName().Text = finalTrans.data.LocalLinks[node].GetName().Text;
        }

        public override void OutAFieldLvalue(AFieldLvalue node)
        {
            node.GetName().Text = finalTrans.data.FieldLinks[node].GetName().Text;
        }

        public override void OutAStructLvalue(AStructLvalue node)
        {
            node.GetName().Text = finalTrans.data.StructFieldLinks[node].GetName().Text;
        }

        public override void OutANamedType(ANamedType node)
        {
            if (!node.IsPrimitive())//!GalaxyKeywords.Primitives.words.Contains(node.GetName().Text))
                node.SetName(
                    new AAName(new List<TIdentifier>()
                                   {new TIdentifier(finalTrans.data.StructTypeLinks[node].GetName().Text)}));
        }

        //Rename trigger refferences
        public override void OutAMethodDecl(AMethodDecl node)
        {
            if (finalTrans.data.TriggerDeclarations.ContainsKey(node))
            {
                foreach (TStringLiteral stringLiteral in finalTrans.data.TriggerDeclarations[node])
                {
                    stringLiteral.Text = "\"" + node.GetName().Text + "\"";
                }
            }
        }

    }
}
