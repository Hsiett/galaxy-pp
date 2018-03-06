using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations
{
    class MoveLocalsToStart : DepthFirstAdapter
    {
        private SharedData data;

        public MoveLocalsToStart(SharedData data)
        {
            this.data = data;
        }

        public override void CaseALocalDeclStm(ALocalDeclStm node)
        {
            AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
            AALocalDecl decl = (AALocalDecl) node.GetLocalDecl();

            if (decl.GetInit() == null)
            {
                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(decl.GetName().Text));
                data.LocalLinks[lvalue] = decl;
                data.LvalueTypes[lvalue] = decl.GetType();
                List<PStm> statements = AssignDefault(lvalue);
                AABlock pBlock = (AABlock)node.Parent();
                foreach (PStm statement in statements)
                {
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), statement);
                }
                pBlock.RemoveChild(node);
            }
            else
            {
                //Make an assignment expression before moving
                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(decl.GetName().Text));
                data.LvalueTypes[lvalue] = decl.GetType();
                AAssignmentExp exp = new AAssignmentExp(new TAssign("="), lvalue, decl.GetInit());
                AExpStm expStm = new AExpStm(new TSemicolon(";"), exp);
                node.ReplaceBy(expStm);
                data.LvalueTypes[lvalue] = decl.GetType();
                data.ExpTypes[exp] = decl.GetType();
                data.LocalLinks[lvalue] = decl;
            }

            AABlock block = (AABlock) pMethod.GetBlock();
            block.GetStatements().Insert(0, node);
        }

        public override void CaseAExpStm(AExpStm node)
        {
        }

        public override void CaseAWhileStm(AWhileStm node)
        {
            node.GetBody().Apply(this);
        }

        public override void CaseAIfThenStm(AIfThenStm node)
        {
            node.GetBody().Apply(this);
        }

        public override void CaseAIfThenElseStm(AIfThenElseStm node)
        {
            node.GetThenBody().Apply(this);
            node.GetElseBody().Apply(this);
        }

        private List<PStm> AssignDefault(PLvalue lvalue)
        {
            List<PStm> returner = new List<PStm>();
            PType type = data.LvalueTypes[lvalue];
            PExp rightSide = null;
            if (type is ANamedType)
            {
                ANamedType aType = (ANamedType)type;
                if (aType.IsPrimitive("string"))//aType.GetName().Text == "string")
                {
                    rightSide = new AStringConstExp(new TStringLiteral("\"\""));
                    data.ExpTypes[rightSide] = new ANamedType(new TIdentifier("string"), null);
                }
                else if (aType.IsPrimitive(GalaxyKeywords.NullablePrimitives.words)) //GalaxyKeywords.NullablePrimitives.words.Contains(aType.GetName().Text))
                {
                    rightSide = new ANullExp();
                    data.ExpTypes[rightSide] = new ANamedType(new TIdentifier("null"), null);
                }
                else if (aType.IsPrimitive(new []{"int", "byte", "fixed"}))
                    /*aType.GetName().Text == "int" ||
                    aType.GetName().Text == "byte" ||
                    aType.GetName().Text == "fixed")*/
                {
                    rightSide = new AIntConstExp(new TIntegerLiteral("0"));
                    data.ExpTypes[rightSide] = type;
                }
                else if (aType.IsPrimitive("bool"))//aType.GetName().Text == "bool")
                {
                    rightSide = new ABooleanConstExp(new AFalseBool());
                    data.ExpTypes[rightSide] = type;
                }
                else if (aType.IsPrimitive("color"))//aType.GetName().Text == "color")
                {
                    PExp arg1 = new AIntConstExp(new TIntegerLiteral("0"));
                    PExp arg2 = new AIntConstExp(new TIntegerLiteral("0"));
                    PExp arg3 = new AIntConstExp(new TIntegerLiteral("0"));
                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("Color"), new ArrayList() { arg1, arg2, arg3 });
                    rightSide = invoke;
                    data.ExpTypes[rightSide] = type;
                    data.ExpTypes[arg1] =
                        data.ExpTypes[arg2] =
                        data.ExpTypes[arg3] = new ANamedType(new TIdentifier("int"), null);
                    data.SimpleMethodLinks[invoke] =
                        data.Libraries.Methods.First(func => func.GetName().Text == invoke.GetName().Text);
                }
                else if (aType.IsPrimitive("char"))//aType.GetName().Text == "char")
                {
                    //Dunno?!
                    rightSide = new ACharConstExp(new TCharLiteral("'\0'"));
                    data.ExpTypes[rightSide] = type;
                }
                else //Struct
                {
                    AStructDecl str = data.StructTypeLinks[aType];
                    foreach (AALocalDecl localDecl in str.GetLocals())
                    {
                        ALvalueExp reciever = new ALvalueExp(Util.MakeClone(lvalue, data));
                        AStructLvalue newLvalue = new AStructLvalue(reciever, new ADotDotType(new TDot(".")), new TIdentifier(localDecl.GetName().Text));
                        data.StructFieldLinks[newLvalue] = localDecl;
                        data.ExpTypes[reciever] = type;
                        data.LvalueTypes[newLvalue] = localDecl.GetType();
                        returner.AddRange(AssignDefault(newLvalue));
                    }
                    return returner;
                }
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), Util.MakeClone(lvalue, data), rightSide);
                data.ExpTypes[assignment] = type;
                return new List<PStm>() { new AExpStm(new TSemicolon(";"), assignment) };
            }
            if (type is AArrayTempType)
            {
                AArrayTempType aType = (AArrayTempType)type;
                for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                {
                    ALvalueExp reciever = new ALvalueExp(Util.MakeClone(lvalue, data));
                    AArrayLvalue newLvalue = new AArrayLvalue(new TLBracket("["), reciever, new AIntConstExp(new TIntegerLiteral(i.ToString())));
                    data.ExpTypes[reciever] = type;
                    data.LvalueTypes[newLvalue] = aType.GetType();
                    data.ExpTypes[newLvalue.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                    returner.AddRange(AssignDefault(newLvalue));
                }
                return returner;
            }

            throw new Exception("Unexpected type. (LivenessAnalasys.AssignDefault), got " + type);
        }
    }
}
