using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class StaticStructMembers : DepthFirstAdapter
    {
        private SharedData data;
        private Dictionary<AALocalDecl, AFieldDecl> convertionMap = new Dictionary<AALocalDecl, AFieldDecl>();

        public StaticStructMembers(SharedData data)
        {
            this.data = data;
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            //Convert a static struct field into a global variable. All refferences to it are structFieldLvalues.
            if (node.GetStatic() == null)
                return;



            AStructDecl str = (AStructDecl) node.Parent();
            if (data.StructFields[str].Contains(node))
                data.StructFields[str].Remove(node);
            AFieldDecl replacementField;
            //Don't enhrit static fields.
            if (data.EnheritanceLocalMap.ContainsKey(node))
            {
                str.RemoveChild(node);

                AALocalDecl realVar = data.EnheritanceLocalMap[node];
                if (convertionMap.ContainsKey(realVar))
                {
                    //Already converted to a field
                    replacementField = convertionMap[realVar];
                    foreach (AStructFieldLvalue lvalue in data.StructMethodFieldLinks.Where(link => link.Value == node).Select(link => link.Key))
                    {
                        AFieldLvalue newLvalue = new AFieldLvalue(new TIdentifier(replacementField.GetName().Text));
                        data.FieldLinks[newLvalue] = replacementField;
                        data.LvalueTypes[newLvalue] = replacementField.GetType();
                        lvalue.ReplaceBy(newLvalue);
                    }
                }
                else
                {
                    List<AStructFieldLvalue> refferences = new List<AStructFieldLvalue>();
                    refferences.AddRange(data.StructMethodFieldLinks.Where(link => link.Value == node).Select(link => link.Key));
                    foreach (AStructFieldLvalue lvalue in refferences)
                    {
                        data.StructMethodFieldLinks[lvalue] = realVar;
                    }
                }
                return;
            }

            replacementField = new AFieldDecl(new APublicVisibilityModifier(), null, node.GetConst(), node.GetType(), node.GetName(), node.GetInit());

            replacementField.GetName().Text = str.GetName().Text + "_" + replacementField.GetName().Text;

            AASourceFile file = Util.GetAncestor<AASourceFile>(node);
            file.GetDecl().Insert(file.GetDecl().IndexOf(node.Parent()), replacementField);
            data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, replacementField));

            if (ContainsNewExp(replacementField.GetInit()))
                data.FieldsToInitInMapInit.Add(replacementField);

            foreach (AStructFieldLvalue lvalue in data.StructMethodFieldLinks.Where(link => link.Value == node).Select(link => link.Key))
            {
                AFieldLvalue newLvalue = new AFieldLvalue(new TIdentifier(replacementField.GetName().Text));
                data.FieldLinks[newLvalue] = replacementField;
                data.LvalueTypes[newLvalue] = replacementField.GetType();
                lvalue.ReplaceBy(newLvalue);
            }

            convertionMap.Add(node, replacementField);
            node.Parent().RemoveChild(node);
        }

        bool ContainsNewExp(PExp exp)
        {
            if (exp == null) return false;
            NewExpFinder finder = new NewExpFinder();
            exp.Apply(finder);
            return finder.HasNew;
        }

        private class NewExpFinder : DepthFirstAdapter
        {
            public bool HasNew = false;

            public override void CaseANewExp(ANewExp node)
            {
                HasNew = true;
            }

        }
    }
}
