using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveNamespaces : DepthFirstAdapter 
    {
        private string currentNamespace;

        public override void CaseANamespaceDecl(ANamespaceDecl node)
        {
            string lastNamespace = currentNamespace;
            currentNamespace += node.GetName().Text + "_";
            base.CaseANamespaceDecl(node);
            currentNamespace = lastNamespace;
            while (node.GetDecl().Count > 0)
            {
                PDecl decl = (PDecl) node.GetDecl()[0];
                node.RemoveChild(decl);
                if (node.Parent() is ANamespaceDecl)
                {
                    ANamespaceDecl parent = (ANamespaceDecl) node.Parent();
                    parent.GetDecl().Insert(parent.GetDecl().IndexOf(node), decl);
                }
                else
                {
                    AASourceFile parent = (AASourceFile)node.Parent();
                    parent.GetDecl().Insert(parent.GetDecl().IndexOf(node), decl);
                }
            }
            node.Parent().RemoveChild(node);
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            node.GetName().Text = currentNamespace + node.GetName().Text;
        }

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            node.GetName().Text = currentNamespace + node.GetName().Text;
        }

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            node.GetName().Text = currentNamespace + node.GetName().Text;
        }
    }
}
