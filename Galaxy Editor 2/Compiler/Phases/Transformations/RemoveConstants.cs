using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveConstants : DepthFirstAdapter
    {
        private SharedData data;
        private AALocalDecl initialLocalDecl;
        private AFieldDecl initialFieldDecl;

        public RemoveConstants(SharedData data)
        {
            this.data = data;
        }

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            if (node.GetConst() == null)
                return;

            initialFieldDecl = node;

            if (IsConstant(node.GetInit()))
            {
                List<AFieldLvalue> lvalues = new List<AFieldLvalue>();
                lvalues.AddRange(data.FieldLinks.Where(link => link.Value == node).Select(link => link.Key));
                foreach (AFieldLvalue lvalue in lvalues)
                {
                    PExp parent = (PExp)lvalue.Parent();
                    parent.ReplaceBy(Util.MakeClone(node.GetInit(), data));
                }
                node.Parent().RemoveChild(node);
            }


            initialFieldDecl = null;
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (node.GetConst() == null)
                return;

            initialLocalDecl = node;

            if (IsConstant(node.GetInit()))
            {
                {
                    List<ALocalLvalue> lvalues = new List<ALocalLvalue>();
                    lvalues.AddRange(data.LocalLinks.Where(link => link.Value == node).Select(link => link.Key));
                    foreach (ALocalLvalue lvalue in lvalues)
                    {
                        PExp parent = (PExp)lvalue.Parent();
                        parent.ReplaceBy(Util.MakeClone(node.GetInit(), data));
                    }
                }
                {
                    List<AStructLvalue> lvalues = new List<AStructLvalue>();
                    lvalues.AddRange(data.StructFieldLinks.Where(link => link.Value == node).Select(link => link.Key));
                    foreach (AStructLvalue lvalue in lvalues)
                    {
                        PExp parent = (PExp)lvalue.Parent();
                        parent.ReplaceBy(Util.MakeClone(node.GetInit(), data));
                    }
                }
                {
                    List<AStructFieldLvalue> lvalues = new List<AStructFieldLvalue>();
                    lvalues.AddRange(
                        data.StructMethodFieldLinks.Where(link => link.Value == node).Select(link => link.Key));
                    foreach (AStructFieldLvalue lvalue in lvalues)
                    {
                        PExp parent = (PExp) lvalue.Parent();
                        parent.ReplaceBy(Util.MakeClone(node.GetInit(), data));
                    }
                }
                if (node.Parent() is ALocalDeclStm)
                    node.Parent().Parent().RemoveChild(node.Parent());
                else
                    node.Parent().RemoveChild(node);
            }


            initialLocalDecl = null;
        }


        bool IsConstant(PExp exp)
        {
            if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp) exp;
                return IsConstant(aExp.GetLeft()) && IsConstant(aExp.GetRight());
            }
            if (exp is AUnopExp)
            {
                AUnopExp aExp = (AUnopExp)exp;
                return IsConstant(aExp.GetExp());
            }
            if (exp is AIncDecExp)
            {
                AIncDecExp aExp = (AIncDecExp)exp;
                return IsConstant(aExp.GetLvalue());
            }
            if (exp is AIntConstExp || exp is AHexConstExp || 
                exp is AOctalConstExp || exp is AFixedConstExp ||
                exp is AStringConstExp || exp is ACharConstExp ||
                exp is ABooleanConstExp || exp is ANullExp ||
                exp is AAssignmentExp || exp is ADelegateExp)
            {
                return true;
            }
            if (exp is ASimpleInvokeExp || exp is ANonstaticInvokeExp ||
                exp is ASyncInvokeExp || exp is ANewExp ||
                exp is ADelegateInvokeExp)
            {
                return false;
            }
            if (exp is ALvalueExp)
            {
                ALvalueExp aExp = (ALvalueExp)exp;
                return IsConstant(aExp.GetLvalue());
            }
            if (exp is AParenExp)
            {
                AParenExp aExp = (AParenExp)exp;
                return IsConstant(aExp.GetExp());
            }
            if (exp is ACastExp)
            {
                ACastExp aExp = (ACastExp)exp;
                return IsConstant(aExp.GetExp());
            }
            if (exp is AIfExp)
            {
                AIfExp aExp = (AIfExp)exp;
                return IsConstant(aExp.GetCond()) && IsConstant(aExp.GetThen()) && IsConstant(aExp.GetElse());
            }
            if (exp == null)
                return false;
            throw new Exception("Unexpected exp. Got " + exp);
        }

        bool IsConstant(PLvalue lvalue)
        {
            if (lvalue is ALocalLvalue)
            {
                ALocalLvalue aLvalue = (ALocalLvalue) lvalue;
                AALocalDecl decl = data.LocalLinks[aLvalue];
                if (decl == initialLocalDecl) return false;
                return decl.GetConst() != null && IsConstant(decl.GetInit());
            }
            if (lvalue is AFieldLvalue)
            {
                AFieldLvalue aLvalue = (AFieldLvalue)lvalue;
                AFieldDecl decl = data.FieldLinks[aLvalue];
                if (decl == initialFieldDecl) return false;
                return decl.GetConst() != null && IsConstant(decl.GetInit());
            }
            if (lvalue is APropertyLvalue)
            {
                return false;
            }
            if (lvalue is ANamespaceLvalue)
            {
                return true;
            }
            if (lvalue is AStructFieldLvalue)
            {
                AStructFieldLvalue aLvalue = (AStructFieldLvalue)lvalue;
                AALocalDecl decl = data.StructMethodFieldLinks[aLvalue];
                if (decl == initialLocalDecl) return false;
                return decl.GetConst() != null && IsConstant(decl.GetInit());
            }
            if (lvalue is AStructLvalue)
            {
                AStructLvalue aLvalue = (AStructLvalue)lvalue;
                AALocalDecl decl = data.StructFieldLinks[aLvalue];
                if (decl == initialLocalDecl) return false;
                return decl.GetConst() != null && IsConstant(decl.GetInit());
            }
            if (lvalue is AArrayLvalue)
            {
                AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                return IsConstant(aLvalue.GetIndex()) && IsConstant(aLvalue.GetBase());
            }
            if (lvalue is APointerLvalue)
            {
                APointerLvalue aLvalue = (APointerLvalue)lvalue;
                return IsConstant(aLvalue.GetBase());
            }
            if (lvalue is APArrayLengthLvalue)
            {
                APArrayLengthLvalue aLvalue = (APArrayLengthLvalue)lvalue;
                return data.ExpTypes[aLvalue.GetBase()] is AArrayTempType;
            }
            if (lvalue is AThisLvalue)
            {
                return false;
            }
            if (lvalue is AValueLvalue)
            {
                return false;
            }
            throw new Exception("Unexpected lvalue. Got " + lvalue);
        }
    }
}
