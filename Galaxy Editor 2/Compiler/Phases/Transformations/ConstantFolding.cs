using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class ConstantFolding : DepthFirstAdapter
    {
        private class Pair<T, V>
        {
            public T Car;
            public V Cdr;

            public Pair(T car, V cdr)
            {
                Car = car;
                Cdr = cdr;
            }
        }

        private SharedData data;

        public ConstantFolding(SharedData data)
        {
            this.data = data;
        }

        private bool isNegativeRightSide;
        List<Pair<AIntConstExp, bool>> intConsts = new List<Pair<AIntConstExp, bool>>();

        Stack<Pair<bool, List<Pair<AIntConstExp, bool>>>> stackList = new Stack<Pair<bool, List<Pair<AIntConstExp, bool>>>>();

        public override void CaseAIntConstExp(AIntConstExp node)
        {
            if (node.Parent() is ABinopExp)
                intConsts.Add(new Pair<AIntConstExp, bool>(node, isNegativeRightSide));
        }

        public override void CaseABinopExp(ABinopExp node)
        {
            bool pushed = false;
            if (!(node.Parent() is ABinopExp))
            {
                PushStack();
                pushed = true;
            }
            try
            {
                bool isIntegerType = data.ExpTypes[node] is ANamedType &&
                                     (((ANamedType) data.ExpTypes[node]).IsPrimitive("int") ||
                                      ((ANamedType) data.ExpTypes[node]).IsPrimitive("byte"));
                if (isIntegerType)
                {
                    if (node.GetBinop() is APlusBinop || node.GetBinop() is AMinusBinop)
                    {
                        node.GetLeft().Apply(this);
                        if (!Util.HasAncestor<AAProgram>(node))
                            return;
                        if (node.GetBinop() is AMinusBinop)
                            isNegativeRightSide = !isNegativeRightSide;
                        node.GetRight().Apply(this);
                        if (node.GetBinop() is AMinusBinop)
                            isNegativeRightSide = !isNegativeRightSide;
                        if (!Util.HasAncestor<AAProgram>(node))
                            return;
                        for (int i = 0; i < intConsts.Count; i++)
                        {
                            for (int j = i + 1; j < intConsts.Count; j++)
                            {
                                Pair<AIntConstExp, bool> const1 = intConsts[i];
                                Pair<AIntConstExp, bool> const2 = intConsts[j];

                                ABinopExp pBinop1 = (ABinopExp) const1.Car.Parent();
                                ABinopExp pBinop2 = (ABinopExp) const2.Car.Parent();

                                int a = int.Parse(const1.Car.GetIntegerLiteral().Text);
                                int b = int.Parse(const2.Car.GetIntegerLiteral().Text);
                                int c;

                                if (const1.Cdr != const2.Cdr)
                                {
                                    c = a - b;
                                }
                                else
                                {
                                    c = a + b;
                                }

                                //Eliminate stuff like <exp> + -1
                                if (c < 0 && pBinop1.GetRight() == const1.Car)
                                {
                                    c = -c;
                                    if (pBinop1.GetBinop() is AMinusBinop)
                                        pBinop1.SetBinop(new APlusBinop(new TPlus("+")));
                                    else
                                        pBinop1.SetBinop(new AMinusBinop(new TMinus("-")));
                                    const1.Cdr = !const1.Cdr;
                                }
                                const1.Car.GetIntegerLiteral().Text = c.ToString();

                                //Remove binop2
                                if (pBinop2.GetLeft() == const2.Car)
                                {
                                    if (pBinop2.GetBinop() is AMinusBinop)
                                    {
                                        if (pBinop2.GetRight() is AIntConstExp)
                                        {
                                            AIntConstExp const3 = (AIntConstExp) pBinop2.GetRight();
                                            const3.GetIntegerLiteral().Text =
                                                (-int.Parse(const3.GetIntegerLiteral().Text)).ToString();
                                            pBinop2.ReplaceBy(const3);
                                            intConsts.Add(new Pair<AIntConstExp, bool>(const3, isNegativeRightSide));
                                        }
                                        else
                                        {
                                            AUnopExp unop = new AUnopExp(new ANegateUnop(new TMinus("-")),
                                                                         pBinop2.GetRight());
                                            data.ExpTypes[unop] = new ANamedType(new TIdentifier("int"), null);
                                            pBinop2.ReplaceBy(unop);
                                        }
                                    }
                                    else
                                    {
                                        pBinop2.ReplaceBy(pBinop2.GetRight());
                                    }
                                }
                                else
                                {
                                    pBinop2.ReplaceBy(pBinop2.GetLeft());
                                }

                                intConsts.RemoveAt(j);
                                j--;
                            }
                        }
                        return;
                    }
                }


                {
                    PushStack();
                    node.GetLeft().Apply(this);
                    PopStack();
                    PushStack();
                    node.GetRight().Apply(this);
                    PopStack();
                }

                if (isIntegerType && (node.GetBinop() is ATimesBinop || node.GetBinop() is ADivideBinop) &&
                    node.GetLeft() is AIntConstExp && node.GetRight() is AIntConstExp)
                {
                    AIntConstExp const1 = (AIntConstExp) node.GetLeft();
                    AIntConstExp const2 = (AIntConstExp) node.GetRight();

                    int a = int.Parse(const1.GetIntegerLiteral().Text);
                    int b = int.Parse(const2.GetIntegerLiteral().Text);
                    int c;

                    if (node.GetBinop() is ATimesBinop || b != 0)
                    {
                        if (node.GetBinop() is ATimesBinop)
                            c = a*b;
                        else
                            c = a/b;
                        const1.GetIntegerLiteral().Text = c.ToString();
                        node.ReplaceBy(const1);
                        const1.Apply(this);
                        return;
                    }
                }

                if (node.GetBinop() is AEqBinop || node.GetBinop() is ANeBinop)
                {
                    if (node.GetLeft() is ABooleanConstExp && node.GetRight() is ABooleanConstExp)
                    {
                        bool b1 = ((ABooleanConstExp)node.GetLeft()).GetBool() is ATrueBool;
                        bool b2 = ((ABooleanConstExp)node.GetRight()).GetBool() is ATrueBool;
                        bool b3 = false;
                        if (node.GetBinop() is AEqBinop)
                            b3 = b1 == b2;
                        else if (node.GetBinop() is ANeBinop)
                            b3 = b1 != b2;
                        ((ABooleanConstExp)node.GetLeft()).SetBool(b3 ? (PBool)new ATrueBool() : new AFalseBool());
                        node.ReplaceBy(node.GetLeft());
                        return;
                    }
                    else if (node.GetLeft() is AIntConstExp && node.GetRight() is AIntConstExp)
                    {
                        AIntConstExp const1 = (AIntConstExp)node.GetLeft();
                        AIntConstExp const2 = (AIntConstExp)node.GetRight();

                        int a = int.Parse(const1.GetIntegerLiteral().Text);
                        int b = int.Parse(const2.GetIntegerLiteral().Text);
                        bool c = false;
                        if (node.GetBinop() is AEqBinop)
                            c = a == b;
                        else if (node.GetBinop() is ANeBinop)
                            c = a != b;
                        ABooleanConstExp booleanExp = new ABooleanConstExp(c ? (PBool)new ATrueBool() : new AFalseBool());
                        data.ExpTypes[booleanExp] = new ANamedType(new TIdentifier("bool"), null);
                        node.ReplaceBy(booleanExp);
                        return;
                    }
                    else if (node.GetLeft() is ANullExp && node.GetRight() is ANullExp)
                    {
                        ABooleanConstExp booleanExp = new ABooleanConstExp(node.GetBinop() is AEqBinop ? (PBool)new ATrueBool() : new AFalseBool());
                        data.ExpTypes[booleanExp] = new ANamedType(new TIdentifier("bool"), null);
                        node.ReplaceBy(booleanExp);
                        return;
                    }
                    else if (node.GetLeft() is AStringConstExp && node.GetRight() is AStringConstExp)
                    {
                        AStringConstExp const1 = (AStringConstExp)node.GetLeft();
                        AStringConstExp const2 = (AStringConstExp)node.GetRight();

                        string a = const1.GetStringLiteral().Text;
                        string b = const2.GetStringLiteral().Text;
                        bool c = false;
                        if (node.GetBinop() is AEqBinop)
                            c = a == b;
                        else if (node.GetBinop() is ANeBinop)
                            c = a != b;
                        ABooleanConstExp booleanExp = new ABooleanConstExp(c ? (PBool)new ATrueBool() : new AFalseBool());
                        data.ExpTypes[booleanExp] = new ANamedType(new TIdentifier("bool"), null);
                        node.ReplaceBy(booleanExp);
                        return;
                    }
                }
                if ((node.GetLeft() is ABooleanConstExp || node.GetRight() is ABooleanConstExp) && 
                    (node.GetBinop() is ALazyAndBinop ||  node.GetBinop() is ALazyOrBinop))
                {
                    ABooleanConstExp boolExp;
                    PExp other;
                    if (node.GetLeft() is ABooleanConstExp)
                    {
                        boolExp = (ABooleanConstExp)node.GetLeft();
                        other = node.GetRight();
                    }
                    else
                    {
                        boolExp = (ABooleanConstExp)node.GetRight();
                        other = node.GetLeft();
                    }
                    if (node.GetBinop() is ALazyAndBinop)
                    {
                        if (boolExp.GetBool() is ATrueBool)
                            //true && <exp>
                            node.ReplaceBy(other);
                        else
                            //false && <exp>
                            node.ReplaceBy(boolExp);
                    }
                    else
                    {
                        if (boolExp.GetBool() is ATrueBool)
                            //true || <exp>
                            node.ReplaceBy(boolExp);
                        else
                            //false || <exp>
                            node.ReplaceBy(other);
                    }
                    return;
                }


            }
            finally
            {
                if (pushed)
                    PopStack();
            }
        }

        void PushStack()
        {
            bool pIsNegativeRightSide = isNegativeRightSide;
            List<Pair<AIntConstExp, bool>> pIntConsts = intConsts;

            isNegativeRightSide = false;
            intConsts = new List<Pair<AIntConstExp, bool>>();

            stackList.Push(new Pair<bool, List<Pair<AIntConstExp, bool>>>(pIsNegativeRightSide, pIntConsts));
        }

        void PopStack()
        {
            Pair<bool, List<Pair<AIntConstExp, bool>>> elm = stackList.Pop();
            isNegativeRightSide = elm.Car;
            intConsts = elm.Cdr;
        }

        public override void DefaultIn(Node node)
        {
            PushStack();
        }

        public override void DefaultOut(Node node)
        {
            PopStack();
        }


        /*public override void OutABinopExp(ABinopExp node)
        {
            //Case (int + int)
            if (node.GetLeft() is AIntConstExp && node.GetRight() is AIntConstExp)
            {
                AIntConstExp left = (AIntConstExp)node.GetLeft();
                AIntConstExp right = (AIntConstExp)node.GetRight();

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
                        return;
                    }
                    a /= b;
                }
                else
                {
                    return;
                }

                left.GetIntegerLiteral().Text = a.ToString();
                node.ReplaceBy(left);
                left.Apply(this);
                return;
            }

            //In a tree of + and - binops



















            //Case (<exp> + int) + int
            if (node.GetLeft() is ABinopExp && node.GetRight() is AIntConstExp && (node.GetBinop() is APlusBinop || node.GetBinop() is AMinusBinop))
            {
                ABinopExp leftBinop = (ABinopExp)node.GetLeft();
                PType leftType = data.ExpTypes[leftBinop];
                if (leftBinop.GetRight() is AIntConstExp && leftType is ANamedType && ((ANamedType)leftType).GetName().Text == "int" &&
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
            }


        }*/
    }
}
