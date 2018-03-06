using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveEmptyStructs : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;
        private bool reqRerun;

        public RemoveEmptyStructs(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        public override void OutAAProgram(AAProgram node)
        {
            List<ALocalLvalue> deleteUs = new List<ALocalLvalue>();
            foreach (KeyValuePair<ALocalLvalue, AALocalDecl> pair in finalTrans.data.LocalLinks)
            {
                if (!Util.HasAncestor<AAProgram>(pair.Key))
                    deleteUs.Add(pair.Key);
            }
            foreach (ALocalLvalue lvalue in deleteUs)
            {
                finalTrans.data.LocalLinks.Remove(lvalue);
            }
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            if (node.GetLocals().Count == 0)
                node.Parent().RemoveChild(node);
            else
                base.CaseAStructDecl(node);
        }

        public override void DefaultIn(Node node)
        {
            if (node is PExp)
            {
                PExp exp = (PExp) node;
                if (finalTrans.data.ExpTypes[exp] is ANamedType)
                {
                    ANamedType type = (ANamedType) finalTrans.data.ExpTypes[exp];
                    if (finalTrans.data.StructTypeLinks.ContainsKey(type))
                    {
                        AStructDecl strDecl = finalTrans.data.StructTypeLinks[type];
                        if (strDecl.GetLocals().Cast<PLocalDecl>().Select(decl => decl is AALocalDecl).Count() == 0)
                        {
                            if (node.Parent() is AAssignmentExp)
                                node = node.Parent().Parent();
                            MoveMethodDeclsOut mover = new MoveMethodDeclsOut("removedStructVar", finalTrans.data);
                            node.Apply(mover);
                            foreach (PStm pStm in mover.NewStatements)
                            {
                                pStm.Apply(this);
                            }
                            node.Parent().RemoveChild(node);

                            if (node.Parent() is ABinopExp)
                            {
                                ABinopExp parent = (ABinopExp) node.Parent();
                                ABooleanConstExp replacer;
                                if (parent.GetBinop() is ANeBinop || parent.GetBinop() is AGtBinop || parent.GetBinop() is ALtBinop)
                                    replacer = new ABooleanConstExp(new AFalseBool());
                                else
                                    replacer = new ABooleanConstExp(new ATrueBool());
                                finalTrans.data.ExpTypes[replacer] = new ANamedType(new TIdentifier("bool"), null);
                                parent.ReplaceBy(replacer);
                            }
                        }
                    }
                }
            }
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (node.GetType() is ANamedType)
            {
                ANamedType type = (ANamedType)node.GetType();
                if (finalTrans.data.StructTypeLinks.ContainsKey(type))
                {
                    AStructDecl strDecl = finalTrans.data.StructTypeLinks[type];
                    if (strDecl.GetLocals().Cast<PLocalDecl>().Select(decl => decl is AALocalDecl).Count() == 0)
                    {
                        MoveMethodDeclsOut mover = new MoveMethodDeclsOut("removedStructVar", finalTrans.data);
                        node.Apply(mover);
                        foreach (PStm stm in mover.NewStatements)
                        {
                            stm.Apply(this);
                        }
                        PStm pStm = Util.GetAncestor<PStm>(node);
                        if (pStm == null)
                        {
                            strDecl = Util.GetAncestor<AStructDecl>(node);
                            

                            node.Parent().RemoveChild(node);

                            if (strDecl != null && strDecl.GetLocals().Cast<PLocalDecl>().Select(decl => decl is AALocalDecl).Count() == 0)
                                reqRerun = true;
                            
                        }
                        else
                            pStm.Parent().RemoveChild(pStm);
                        return;
                    }
                }
            }
            base.CaseAALocalDecl(node);
        }

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            if (node.GetType() is ANamedType)
            {
                ANamedType type = (ANamedType)node.GetType();
                if (finalTrans.data.StructTypeLinks.ContainsKey(type))
                {
                    AStructDecl strDecl = finalTrans.data.StructTypeLinks[type];
                    if (strDecl.GetLocals().Cast<PLocalDecl>().Select(decl => decl is AALocalDecl).Count() == 0)
                    {
                        node.Parent().RemoveChild(node);
                        return;
                    }
                }
            }
            base.CaseAFieldDecl(node);
        }

        public override void OutAASourceFile(AASourceFile node)
        {
            if (reqRerun)
            {
                reqRerun = false;
                base.CaseAASourceFile(node);
            }
        }
    }
}
