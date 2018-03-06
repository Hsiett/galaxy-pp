using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class EnviromentBuilding : DepthFirstAdapter
    {

        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data)
        {
            ast.Apply(new EnviromentBuilding(errors, data));
        }

        private ErrorCollection errors;
        private SharedData data;
        private AASourceFile currentSourceFile;

        public EnviromentBuilding(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        

        //--------------------------

        public override void OutAEnrichmentDecl(AEnrichmentDecl node)
        {
            data.Enrichments.Add(node);
            base.OutAEnrichmentDecl(node);
        }

        public override void CaseAPreloadBankDecl(APreloadBankDecl node)
        {
            int i = 0;
            if (!(node.GetPlayer() is AIntConstExp))
            {
                errors.Add(new ErrorCollection.Error(node.GetBank(), currentSourceFile, "The player must be an integer literal."));
            }
            else
            {
                i = int.Parse(((AIntConstExp) node.GetPlayer()).GetIntegerLiteral().Text);
            }

            data.BankPreloads.Add(new KeyValuePair<string, int>(node.GetBank().Text, i));
            node.Parent().RemoveChild(node);
        }

        public override void CaseAIncludeDecl(AIncludeDecl node)
        {
            node.Parent().RemoveChild(node);
        }

        public override void CaseAASourceFile(AASourceFile node)
        {
            currentSourceFile = node;
            base.CaseAASourceFile(node);
        }

        public override void CaseATypedefDecl(ATypedefDecl node)
        {
            data.Typedefs.Add(node);
            base.CaseATypedefDecl(node);
        }

        public override void OutAFieldDecl(AFieldDecl node)
        {
            data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(currentSourceFile, node));
            data.UserFields.Add(node);
            base.OutAFieldDecl(node);
        }

        public override void InAABlock(AABlock node)
        {
            AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
            if (!data.Locals.ContainsKey(node))
                data.Locals.Add(node, new List<AALocalDecl>());
            
            base.InAABlock(node);
        }

        public override void OutATriggerDecl(ATriggerDecl node)
        {
            data.Triggers.Add(node);
        }

        public override void OutAConstructorDecl(AConstructorDecl node)
        {
            AStructDecl parentStruct = Util.GetAncestor<AStructDecl>(node);
            if (parentStruct != null)
            {
                data.StructConstructors[parentStruct].Add(node);
                if (parentStruct.GetName().Text != node.GetName().Text)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                         "Constructors must have same name as the enclosing struct."));
                }
            }
            base.OutAConstructorDecl(node);
        }

        public override void OutADeconstructorDecl(ADeconstructorDecl node)
        {
            AStructDecl parentStruct = Util.GetAncestor<AStructDecl>(node);
            if (parentStruct != null)
            {
                if (data.StructDeconstructor.ContainsKey(parentStruct) && data.StructDeconstructor[parentStruct] != node)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "You can only define one deconstructor in a " +
                                                         Util.GetTypeName(parentStruct) + ".", false,
                                                         new[]
                                                             {
                                                                 new ErrorCollection.Error(
                                                                     data.StructDeconstructor[parentStruct].GetName(),
                                                                     "Other deconstructor")
                                                             }));
                }

                data.StructDeconstructor[parentStruct] = node;
                if (parentStruct.GetName().Text != node.GetName().Text)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                         "Deconstructors must have same name as the enclosing struct."));
                }
            }
            base.OutADeconstructorDecl(node);
        }

        /*public override void InAMethodDecl(AMethodDecl node)
        {
            AABlock block = (AABlock) node.GetBlock();
            if (block != null)
            {
                if (!data.Locals.ContainsKey(block))
                    data.Locals.Add(block, new List<AALocalDecl>());
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    data.Locals[block].Add(formal);
                }
            }
        }

        public override void InAConstructorDecl(AConstructorDecl node)
        {
            AABlock block = (AABlock)node.GetBlock();
            if (block != null)
            {
                if (!data.Locals.ContainsKey(block))
                    data.Locals.Add(block, new List<AALocalDecl>());
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    data.Locals[block].Add(formal);
                }
            }
        }*/

        public override void OutAMethodDecl(AMethodDecl node)
        {
            AStructDecl parentStruct = Util.GetAncestor<AStructDecl>(node);
            AEnrichmentDecl parentEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            if (parentStruct != null)
            {
                //Struct method
                data.StructMethods[parentStruct].Add(node);
            }
            else if (parentEnrichment == null)
            {//Global method
                //Dont care about abstract methods - will add them later
                if (node.GetBlock() != null || node.GetNative() != null)
                {
                    data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(currentSourceFile, node));
                    data.UserMethods.Add(node);
                }
                else if (node.GetDelegate() != null)
                    data.Delegates.Add(new SharedData.DeclItem<AMethodDecl>(currentSourceFile, node));
                else
                {
                    node.Parent().RemoveChild(node);
                    return;
                }
            }
            base.OutAMethodDecl(node);
        }

        public override void InAStructDecl(AStructDecl node)
        {
            data.Structs.Add(new SharedData.DeclItem<AStructDecl>(currentSourceFile, node));
            data.StructMethods.Add(node, new List<AMethodDecl>());
            data.StructFields.Add(node, new List<AALocalDecl>());
            data.StructConstructors.Add(node, new List<AConstructorDecl>());
            data.StructProperties.Add(node, new List<APropertyDecl>());

            base.InAStructDecl(node);
        }

        public override void OutAPropertyDecl(APropertyDecl node)
        {
            AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);
            AEnrichmentDecl pEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
            if (pStruct != null)
                data.StructProperties[pStruct].Add(node);
            else if (pEnrichment == null)
                data.Properties.Add(new SharedData.DeclItem<APropertyDecl>(currentSourceFile, node));
            
            base.OutAPropertyDecl(node);
        }

        public override void OutAALocalDecl(AALocalDecl node)
        {
            
            //Can have a local as a struct member, a parameter or a local variable
            AABlock pBlock = Util.GetAncestor<AABlock>(node);
            AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
            AConstructorDecl pConstructor = Util.GetAncestor<AConstructorDecl>(node);
            AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);

            if (pBlock != null)
            {//We got a local variable
                data.Locals[pBlock].Add(node);
                data.UserLocals.Add(node);
            }
            else if (pMethod != null || pConstructor != null)
            {//We got a parameter
                if (pMethod != null)
                    pBlock = (AABlock) pMethod.GetBlock();
                else
                    pBlock = (AABlock) pConstructor.GetBlock();
                if (pBlock != null)
                {
                    if (!data.Locals.ContainsKey(pBlock))
                        data.Locals.Add(pBlock, new List<AALocalDecl>());
                    data.Locals[pBlock].Add(node);
                    data.UserLocals.Add(node);
                }
            }
            else
            {//We got a struct variable
                data.StructFields[pStruct].Add(node);
            }
            base.OutAALocalDecl(node);
        }


        
    }
}
