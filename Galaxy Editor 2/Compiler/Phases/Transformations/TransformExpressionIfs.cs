using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class TransformExpressionIfs : DepthFirstAdapter
    {
        private SharedData data;

        public TransformExpressionIfs(SharedData data)
        {
            this.data = data;
        }

        public override void OutAIfExp(AIfExp node)
        {
            //Transform to 
            /*
             * var expIfVar;
             * if (<cond>)
             * {
             *     expIfVar = <then>;
             * }
             * else
             * {
             *     expIfVar = <else>;
             * }
             * ... expIfVar ...
             */

            AALocalDecl expIfVarDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(data.ExpTypes[node], data), new TIdentifier("expIfVar"), null);

            ALocalLvalue thenExpIfVarLink = new ALocalLvalue(new TIdentifier("expIfVar"));
            ALocalLvalue elseExpIfVarLink = new ALocalLvalue(new TIdentifier("expIfVar"));
            ALocalLvalue usageExpIfVarLink = new ALocalLvalue(new TIdentifier("expIfVar"));

            ALvalueExp usageExpIfVarLinkExp = new ALvalueExp(usageExpIfVarLink);

            AAssignmentExp thenAssignment = new AAssignmentExp(new TAssign("="), thenExpIfVarLink, node.GetThen());
            AAssignmentExp elseAssignment = new AAssignmentExp(new TAssign("="), elseExpIfVarLink, node.GetElse());

            AIfThenElseStm ifStm = new AIfThenElseStm(new TLParen("("), node.GetCond(),
                                                      new ABlockStm(new TLBrace("{"),
                                                                    new AABlock(
                                                                        new ArrayList()
                                                                            {
                                                                                new AExpStm(new TSemicolon(";"),
                                                                                            thenAssignment)
                                                                            },
                                                                        new TRBrace("}"))),
                                                      new ABlockStm(new TLBrace("{"),
                                                                    new AABlock(
                                                                        new ArrayList()
                                                                            {
                                                                                new AExpStm(new TSemicolon(";"),
                                                                                            elseAssignment)
                                                                            },
                                                                        new TRBrace("}"))));

            data.LocalLinks[thenExpIfVarLink] =
                data.LocalLinks[elseExpIfVarLink] =
                data.LocalLinks[usageExpIfVarLink] = expIfVarDecl;

            data.LvalueTypes[thenExpIfVarLink] =
                data.LvalueTypes[elseExpIfVarLink] =
                data.LvalueTypes[usageExpIfVarLink] =
                data.ExpTypes[usageExpIfVarLinkExp] =
                data.ExpTypes[thenAssignment] =
                data.ExpTypes[elseAssignment] = expIfVarDecl.GetType();



            PStm pStm = Util.GetAncestor<PStm>(node);
            AABlock pBlock = (AABlock) pStm.Parent();

            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), expIfVarDecl));
            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), ifStm);
            node.ReplaceBy(usageExpIfVarLinkExp);
            

            base.OutAIfExp(node);
        }
    }
}
