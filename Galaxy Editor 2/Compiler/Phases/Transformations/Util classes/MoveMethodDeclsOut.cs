using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes
{
    class MoveMethodDeclsOut : DepthFirstAdapter
    {
        private SharedData data;
        private string varName;
        public List<PStm> NewStatements = new List<PStm>();

        public MoveMethodDeclsOut(string variableName, SharedData data)
        {
            varName = variableName;
            this.data = data;
        }

        public override void CaseADelegateInvokeExp(ADelegateInvokeExp node)
        {
            PExp expNode = (PExp)node;
            PType type = data.ExpTypes[expNode];
            if (type is APointerType) type = new ANamedType(new TIdentifier("string"), null);
            ALocalLvalue local = new ALocalLvalue(new TIdentifier("tempName", 0, 0));
            ALvalueExp exp = new ALvalueExp(local);
            PStm stm = Util.GetAncestor<PStm>(node);
            AABlock block = (AABlock)stm.Parent();
            node.ReplaceBy(exp);
            AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                    Util.MakeClone(type, data),
                                                    new TIdentifier(varName, 0, 0), expNode);
            ALocalDeclStm newStm = new ALocalDeclStm(new TSemicolon(";"), localDecl);
            block.GetStatements().Insert(block.GetStatements().IndexOf(stm), newStm);
            NewStatements.Add(newStm);

            data.LvalueTypes[local] = type;
            data.ExpTypes[exp] = type;
            data.LocalLinks[local] = localDecl;
            //localDecl.Apply(this);
            exp.Apply(this);
            return;
        }

        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            PExp expNode = (PExp)node;
            PType type = data.ExpTypes[expNode];
            if (type is APointerType) type = new ANamedType(new TIdentifier("string"), null);
            ALocalLvalue local = new ALocalLvalue(new TIdentifier("tempName", 0, 0));
            ALvalueExp exp = new ALvalueExp(local);
            PStm stm = Util.GetAncestor<PStm>(node);
            AABlock block = (AABlock)stm.Parent();
            node.ReplaceBy(exp);
            AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                    Util.MakeClone(type, data),
                                                    new TIdentifier(varName, 0, 0), expNode);
            ALocalDeclStm newStm = new ALocalDeclStm(new TSemicolon(";"), localDecl);
            block.GetStatements().Insert(block.GetStatements().IndexOf(stm), newStm);
            NewStatements.Add(newStm);

            data.LvalueTypes[local] = type;
            data.ExpTypes[exp] = type;
            data.LocalLinks[local] = localDecl;
            //localDecl.Apply(this);
            exp.Apply(this);
            return;
        }
    }
}
