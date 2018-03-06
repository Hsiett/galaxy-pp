using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class StructInitializer : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        private SharedData data { get { return finalTrans.data; } }

        public StructInitializer(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        public override void OutAAProgram(AAProgram node)
        {
            //Remove all inits of struct fields
            foreach (AStructDecl str in data.Structs.Select(declItem => declItem.Decl))
            {
                foreach (AALocalDecl localDecl in str.GetLocals().OfType<AALocalDecl>())
                {
                    localDecl.SetInit(null);
                }
            }
            base.OutAAProgram(node);
        }


        public override void OutAALocalDecl(AALocalDecl node)
        {
            if (!Util.HasAncestor<AABlock>(node) && !Util.HasAncestor<AMethodDecl>(node))
            {
                //OutStructFieldDecl(node);
                return;
            }

            if (node.GetInit() != null)
                return;

            AABlock pBlock;
            int insertIndex;
            PLvalue lvalue;
            if (Util.HasAncestor<AABlock>(node))
            {
                //A local variable
                pBlock = Util.GetAncestor<AABlock>(node);
                insertIndex = pBlock.GetStatements().IndexOf(Util.GetAncestor<PStm>(node)) + 1;
                lvalue = new ALocalLvalue(new TIdentifier(node.GetName().Text));
                data.LocalLinks[(ALocalLvalue) lvalue] = node;
                data.LvalueTypes[lvalue] = node.GetType();
            }
            else
            {
                //Parameter

                //Parameters will be set from the caller
                return;
                pBlock = (AABlock) Util.GetAncestor<AMethodDecl>(node).GetBlock();
                insertIndex = 0;
                lvalue = new ALocalLvalue(new TIdentifier(node.GetName().Text));
                data.LocalLinks[(ALocalLvalue)lvalue] = node;
                data.LvalueTypes[lvalue] = node.GetType();
            }
            AABlock block = new AABlock(new ArrayList(), new TRBrace("}"));
            
            MakeAssignments(block, node.GetType(), lvalue, true);

            if (block.GetStatements().Count != 0)
                pBlock.GetStatements().Insert(insertIndex, new ABlockStm(new TLBrace("{"), block));
        }

        public override void OutAFieldDecl(AFieldDecl node)
        {
            if (node.GetInit() != null)
                return;

            //Field - init in main entry
            AABlock pBlock = (AABlock) finalTrans.mainEntryFieldInitBlock.GetBlock();
            int insertIndex = 0;
            AFieldLvalue lvalue = new AFieldLvalue(new TIdentifier(node.GetName().Text));
            data.FieldLinks[lvalue] = node;
            data.LvalueTypes[lvalue] = node.GetType();
            AABlock block = new AABlock(new ArrayList(), new TRBrace("}"));

            MakeAssignments(block, node.GetType(), lvalue, true);

            if (block.GetStatements().Count != 0)
                pBlock.GetStatements().Insert(insertIndex, new ABlockStm(new TLBrace("{"), block));
        }

        public override void OutAStructDecl(AStructDecl node)
        {
            

            //Insert init in each constructor
            AThisLvalue thisLvalue = new AThisLvalue(new TThis("this"));
            ALvalueExp thisExp = new ALvalueExp(thisLvalue);
            APointerLvalue pointerLvalue = new APointerLvalue(new TStar("*"), thisExp);

            ANamedType namedType = new ANamedType(new TIdentifier(node.GetName().Text), null);
            data.StructTypeLinks[namedType] = node;
            data.LvalueTypes[thisLvalue] =
                data.ExpTypes[thisExp] = new APointerType(new TStar("*"), namedType);
            data.LvalueTypes[pointerLvalue] = namedType;


            foreach (AConstructorDecl constructor in node.GetLocals().OfType<ADeclLocalDecl>().Select(decl => decl.GetDecl()).OfType<AConstructorDecl>())
            {
                AABlock block = new AABlock(new ArrayList(), new TRBrace("}"));
                MakeAssignments(block, namedType, pointerLvalue, false);

                ((AABlock)constructor.GetBlock()).GetStatements().Insert(0, new ABlockStm(new TLBrace("{"), block));
            }

            base.OutAStructDecl(node);
        }

        

        private class NewConstructorFixup : DepthFirstAdapter
        {
            private AConstructorDecl constructor;
            private AStructDecl str;
            private SharedData data;

            public NewConstructorFixup(AConstructorDecl constructor, AStructDecl str, SharedData data)
            {
                this.constructor = constructor;
                this.str = str;
                this.data = data;
            }


            public override void OutANewExp(ANewExp node)
            {
                if (node.GetType() is ANamedType && 
                    data.StructTypeLinks.ContainsKey((ANamedType) node.GetType()) &&
                    data.StructTypeLinks[(ANamedType) node.GetType()] == str &&
                    node.GetArgs().Count == 0)
                {
                    data.ConstructorLinks[node] = constructor;
                }
                base.OutANewExp(node);
            }
        }

        private void MakeAssignments(AABlock block, PType type, PLvalue leftSide, bool onEnhritedFields)
        {
            if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) type))
            {
                AStructDecl str = data.StructTypeLinks[(ANamedType) type];
                foreach (AALocalDecl field in str.GetLocals().OfType<AALocalDecl>())
                {
                    if (!onEnhritedFields && data.EnheritanceLocalMap.ContainsKey(field))
                        continue;

                    ALvalueExp lvalueExp = new ALvalueExp(Util.MakeClone(leftSide, data));
                    data.ExpTypes[lvalueExp] = data.LvalueTypes[leftSide];
                    AStructLvalue newLeftSide = new AStructLvalue(lvalueExp, new ADotDotType(new TDot(".")), new TIdentifier(field.GetName().Text));
                    data.StructFieldLinks[newLeftSide] = field;
                    data.LvalueTypes[newLeftSide] = field.GetType();

                    if (field.GetInit() != null)
                    {
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), newLeftSide,
                                                                       Util.MakeClone(field.GetInit(), data));
                        data.ExpTypes[assignment] = data.LvalueTypes[newLeftSide];

                        block.GetStatements().Add(new AExpStm(new TSemicolon(";"),
                                                              assignment));
                    }
                    else
                    {
                        MakeAssignments(block, field.GetType(), newLeftSide, onEnhritedFields);
                    }
                }
            }
            else if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType) type;
                for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                {
                    AIntConstExp index = new AIntConstExp(new TIntegerLiteral(i.ToString()));
                    data.ExpTypes[index] = new ANamedType(new TIdentifier("int"), null);

                    ALvalueExp lvalueExp = new ALvalueExp(Util.MakeClone(leftSide, data));
                    data.ExpTypes[lvalueExp] = data.LvalueTypes[leftSide];
                    AArrayLvalue newLeftSide = new AArrayLvalue(new TLBracket("["), lvalueExp, index);
                    data.LvalueTypes[newLeftSide] = aType.GetType();

                    MakeAssignments(block, aType.GetType(), newLeftSide, onEnhritedFields);
                }
            }
        }
    }
}
