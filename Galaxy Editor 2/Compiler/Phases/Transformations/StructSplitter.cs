using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class StructSplitter : DepthFirstAdapter
    {
        private SharedData data;

        public StructSplitter(SharedData data)
        {
            this.data = data;
        }

        //Convert struct variables to a collection of local variables
        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (node.GetType() is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) node.GetType()) && Util.HasAncestor<PStm>(node))
            {
                //Can not have init - it would be bulk copy
                AStructDecl str = data.StructTypeLinks[(ANamedType) node.GetType()];
                Dictionary<AALocalDecl, AALocalDecl> variableMap = new Dictionary<AALocalDecl, AALocalDecl>();
                PStm pStm = (PStm) node.Parent();
                AABlock pBlock = (AABlock) pStm.Parent();
                foreach (AALocalDecl structLocal in str.GetLocals())
                {

                    AALocalDecl replacementLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                   Util.MakeClone(structLocal.GetType(), data),
                                                                   new TIdentifier(node.GetName().Text + "_" +
                                                                                   structLocal.GetName().Text), null);
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), replacementLocal));


                    AALocalDecl baseLocal = structLocal;
                    if (data.EnheritanceLocalMap.ContainsKey(baseLocal))
                        baseLocal = data.EnheritanceLocalMap[baseLocal];
                    List<AALocalDecl> localsToAdd = new List<AALocalDecl>();
                    localsToAdd.AddRange(data.EnheritanceLocalMap.Where(pair => pair.Value == baseLocal).Select(pair => pair.Key));
                    localsToAdd.Add(baseLocal);
                    foreach (AALocalDecl localDecl in localsToAdd)
                    {
                        variableMap[localDecl] = replacementLocal;
                    }
                }
                List<ALocalLvalue> uses = new List<ALocalLvalue>();
                uses.AddRange(data.LocalLinks.Where(k => k.Value == node && Util.GetAncestor<AAProgram>(k.Key) != null).Select(k => k.Key));
                foreach (ALocalLvalue lvalue in uses)
                {
                    AStructLvalue structLocalRef = (AStructLvalue) lvalue.Parent().Parent();

                    AALocalDecl replacementLocal = variableMap[data.StructFieldLinks[structLocalRef]];
                    ALocalLvalue replacementLvalue = new ALocalLvalue(new TIdentifier(replacementLocal.GetName().Text));
                    data.LocalLinks[replacementLvalue] = replacementLocal;
                    data.LvalueTypes[replacementLvalue] = replacementLocal.GetType();
                    structLocalRef.ReplaceBy(replacementLvalue);
                }
                foreach (AALocalDecl replacementLocal in variableMap.Select(k => k.Value))
                {
                    replacementLocal.Apply(this);
                }
            }
            base.CaseAALocalDecl(node);
        }
    }
}
