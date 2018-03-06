using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class ObfuscateStrings : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        private SharedData data
        {
            get { return finalTrans.data; }
        }

        public ObfuscateStrings(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        public override void CaseAAProgram(AAProgram node)
        {
            if (Options.Compiler.ObfuscateStrings)
                base.CaseAAProgram(node);
        }

        List<AStringConstExp> strings = new List<AStringConstExp>();


        public override void OutAStringConstExp(AStringConstExp node)
        {
            strings.Add(node);
        }

        private AMethodDecl CreateStringDeobfuscator()
        {
            AASourceFile file = (AASourceFile) finalTrans.mainEntry.Parent();

            //Create fields for the string constants
            AStringConstExp emptyStringConst = new AStringConstExp(new TStringLiteral("\"\""));
            AFieldDecl emptyStringField = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const"),
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("fOobar"), emptyStringConst);
            file.GetDecl().Add(emptyStringField);
            data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, emptyStringField));
            AFieldLvalue emptyStringRef1 = new AFieldLvalue(new TIdentifier(emptyStringField.GetName().Text));
            AFieldLvalue emptyStringRef2 = new AFieldLvalue(new TIdentifier(emptyStringField.GetName().Text));
            AFieldLvalue emptyStringRef3 = new AFieldLvalue(new TIdentifier(emptyStringField.GetName().Text));
            ALvalueExp emptyStringRef1Exp = new ALvalueExp(emptyStringRef1);
            ALvalueExp emptyStringRef2Exp = new ALvalueExp(emptyStringRef2);
            ALvalueExp emptyStringRef3Exp = new ALvalueExp(emptyStringRef3);


            AStringConstExp colonStringConst = new AStringConstExp(new TStringLiteral("\":\""));
            AFieldDecl colonStringField = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const"),
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("foObar"), colonStringConst);
            file.GetDecl().Add(colonStringField);
            data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, colonStringField));
            AFieldLvalue colonStringRef = new AFieldLvalue(new TIdentifier(colonStringField.GetName().Text));
            ALvalueExp colonStringRefExp = new ALvalueExp(colonStringRef);

            /*
                string output = "";
                string ch;
                int length = StringLength(s);
                int phase1 = (length - 1)%3;
                int phase2 = (length - 1)%2;
             */

            AALocalDecl stringParam = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                      new ANamedType(new TIdentifier("string"), null),
                                                      new TIdentifier("fo0bar"), emptyStringRef1Exp);
            ALocalLvalue stringParamRef1 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef2 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef3 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef4 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef5 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef6 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef7 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALocalLvalue stringParamRef8 = new ALocalLvalue(new TIdentifier(stringParam.GetName().Text));
            ALvalueExp stringParamRef1Exp = new ALvalueExp(stringParamRef1);
            ALvalueExp stringParamRef2Exp = new ALvalueExp(stringParamRef2);
            ALvalueExp stringParamRef4Exp = new ALvalueExp(stringParamRef4);
            ALvalueExp stringParamRef5Exp = new ALvalueExp(stringParamRef5);
            ALvalueExp stringParamRef7Exp = new ALvalueExp(stringParamRef7);
            ALvalueExp stringParamRef8Exp = new ALvalueExp(stringParamRef8);


            AABlock methodBlock = new AABlock(new ArrayList(), new TRBrace("}"));

            AALocalDecl outputDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                     new ANamedType(new TIdentifier("string"), null),
                                                     new TIdentifier("foobar"), emptyStringRef1Exp);
            methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), outputDecl));
            ALocalLvalue outputRef1 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef2 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef3 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef4 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef5 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef6 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef7 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef8 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef9 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef10 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef11 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef12 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALocalLvalue outputRef13 = new ALocalLvalue(new TIdentifier(outputDecl.GetName().Text));
            ALvalueExp outputRef2Exp = new ALvalueExp(outputRef2);
            ALvalueExp outputRef4Exp = new ALvalueExp(outputRef4);
            ALvalueExp outputRef5Exp = new ALvalueExp(outputRef5);
            ALvalueExp outputRef6Exp = new ALvalueExp(outputRef6);
            ALvalueExp outputRef7Exp = new ALvalueExp(outputRef7);
            ALvalueExp outputRef8Exp = new ALvalueExp(outputRef8);
            ALvalueExp outputRef10Exp = new ALvalueExp(outputRef10);
            ALvalueExp outputRef11Exp = new ALvalueExp(outputRef11);
            ALvalueExp outputRef12Exp = new ALvalueExp(outputRef12);
            ALvalueExp outputRef13Exp = new ALvalueExp(outputRef13);

            AALocalDecl chDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                     new ANamedType(new TIdentifier("string"), null),
                                                     new TIdentifier("f0obar"), null);
            methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), chDecl));
            ALocalLvalue chRef1 = new ALocalLvalue(new TIdentifier(chDecl.GetName().Text));
            ALocalLvalue chRef2 = new ALocalLvalue(new TIdentifier(chDecl.GetName().Text));
            ALocalLvalue chRef3 = new ALocalLvalue(new TIdentifier(chDecl.GetName().Text));
            ALocalLvalue chRef4 = new ALocalLvalue(new TIdentifier(chDecl.GetName().Text));
            ALocalLvalue chRef5 = new ALocalLvalue(new TIdentifier(chDecl.GetName().Text));
            ALvalueExp chRef3Exp = new ALvalueExp(chRef3);
            ALvalueExp chRef4Exp = new ALvalueExp(chRef4);
            ALvalueExp chRef5Exp = new ALvalueExp(chRef5);

            ASimpleInvokeExp stringLengthInvoke1 = new ASimpleInvokeExp(new TIdentifier("StringLength"),
                                                                        new ArrayList() { stringParamRef1Exp });
            AALocalDecl lengthDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                     new ANamedType(new TIdentifier("int"), null),
                                                     new TIdentifier("f0Obar"), stringLengthInvoke1);
            methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), lengthDecl));
            ALocalLvalue lengthRef1 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef2 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef3 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef4 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef5 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef6 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef7 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALocalLvalue lengthRef8 = new ALocalLvalue(new TIdentifier(lengthDecl.GetName().Text));
            ALvalueExp lengthRef1Exp = new ALvalueExp(lengthRef1);
            ALvalueExp lengthRef2Exp = new ALvalueExp(lengthRef2);
            ALvalueExp lengthRef3Exp = new ALvalueExp(lengthRef3);
            ALvalueExp lengthRef4Exp = new ALvalueExp(lengthRef4);
            ALvalueExp lengthRef5Exp = new ALvalueExp(lengthRef5);
            ALvalueExp lengthRef6Exp = new ALvalueExp(lengthRef6);
            ALvalueExp lengthRef7Exp = new ALvalueExp(lengthRef7);

            AIntConstExp intConstp1Init1 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConstp1Init2 = new AIntConstExp(new TIntegerLiteral("3"));
            ABinopExp binopExpP1InitMinus = new ABinopExp(lengthRef1Exp, new AMinusBinop(new TMinus("-")), intConstp1Init1);
            ABinopExp binopExpP1InitMod = new ABinopExp(binopExpP1InitMinus, new AModuloBinop(new TMod("%")), intConstp1Init2);

            AALocalDecl phase1Decl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                     new ANamedType(new TIdentifier("int"), null),
                                                     new TIdentifier("fO0bar"), binopExpP1InitMod);
            methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), phase1Decl));
            ALocalLvalue phase1Ref1 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref2 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref3 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref4 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref5 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref6 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref7 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref8 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref9 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref10 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALocalLvalue phase1Ref11 = new ALocalLvalue(new TIdentifier(phase1Decl.GetName().Text));
            ALvalueExp phase1Ref1Exp = new ALvalueExp(phase1Ref1);
            ALvalueExp phase1Ref2Exp = new ALvalueExp(phase1Ref2);
            ALvalueExp phase1Ref4Exp = new ALvalueExp(phase1Ref4);
            ALvalueExp phase1Ref5Exp = new ALvalueExp(phase1Ref5);
            ALvalueExp phase1Ref7Exp = new ALvalueExp(phase1Ref7);
            ALvalueExp phase1Ref9Exp = new ALvalueExp(phase1Ref9);
            ALvalueExp phase1Ref10Exp = new ALvalueExp(phase1Ref10);
            ALvalueExp phase1Ref11Exp = new ALvalueExp(phase1Ref11);

            AIntConstExp intConstp2Init1 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConstp2Init2 = new AIntConstExp(new TIntegerLiteral("2"));
            ABinopExp binopExpP2InitMinus = new ABinopExp(lengthRef2Exp, new AMinusBinop(new TMinus("-")), intConstp2Init1);
            ABinopExp binopExpP2InitMod = new ABinopExp(binopExpP2InitMinus, new AModuloBinop(new TMod("%")), intConstp2Init2);

            AALocalDecl phase2Decl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                     new ANamedType(new TIdentifier("int"), null),
                                                     new TIdentifier("carl"), binopExpP2InitMod);
            methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), phase2Decl));
            ALocalLvalue phase2Ref1 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref2 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref3 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref4 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref5 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref6 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref7 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref8 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALocalLvalue phase2Ref9 = new ALocalLvalue(new TIdentifier(phase2Decl.GetName().Text));
            ALvalueExp phase2Ref1Exp = new ALvalueExp(phase2Ref1);
            ALvalueExp phase2Ref2Exp = new ALvalueExp(phase2Ref2);
            ALvalueExp phase2Ref4Exp = new ALvalueExp(phase2Ref4);
            ALvalueExp phase2Ref5Exp = new ALvalueExp(phase2Ref5);
            ALvalueExp phase2Ref7Exp = new ALvalueExp(phase2Ref7);
            ALvalueExp phase2Ref9Exp = new ALvalueExp(phase2Ref9);

            /*
                while(length > 0)
                {       
                    if(phase2 == 0)
                    {
                        ch = StringSub(s, 1, 1);
                        s = StringReplace(s, "", 1, 1);
                    }
                    else
                    {
                        if(phase2 == 1)
                        {
                            ch = StringSub(s, length, length);
                            s = StringReplace(s, "", length, length);
                        }
                    }
        
                    if(phase1 == 0)
                    {
                        output = ch + output;
                    }
                    else
                    {
                        if(phase1 == 1)
                        {
                            output = StringSub(output, 1, (StringLength(output) + 1)/2) + ch + StringSub(output, (StringLength(output) + 1)/2 + 1, StringLength(output));
                        }
                        else
                        {
                            output = output + ch;
                        }
                    }
                    phase1 = phase1 - 1;
                    if(phase1 < 0)
                    {
                        phase1 = phase1 + 3;
                    }
                    phase2 = phase2 - 1;
                    if(phase2 < 0)
                    {
                        phase2 = phase2 + 2;
                    }
                    length = StringLength(s);
                }
             */ 

            AABlock whileBlock = new AABlock(new ArrayList(), new TRBrace("}"));
            AIntConstExp intConstWhileCond = new AIntConstExp(new TIntegerLiteral("0"));
            ABinopExp binopWhileCond = new ABinopExp(lengthRef3Exp, new AGtBinop(new TGt(">")), intConstWhileCond);
            methodBlock.GetStatements().Add(new AWhileStm(new TLParen("("), binopWhileCond,
                                                          new ABlockStm(new TLBrace("{"), whileBlock)));

            /*
                    if(phase2 == 0)
                    {
                        ch = StringSub(s, 1, 1);
                        s = StringReplace(s, "", 1, 1);
                    }
                    else
                    {
                        if(phase2 == 1)
                        {
                            ch = StringSub(s, length, length);
                            s = StringReplace(s, "", length, length);
                        }
                    }
             */
            AIntConstExp intConstIf1Cond = new AIntConstExp(new TIntegerLiteral("0"));
            ABinopExp binopIf1Cond = new ABinopExp(phase2Ref1Exp, new AEqBinop(new TEq("==")), intConstIf1Cond);
            AABlock thenBlock = new AABlock();
            AABlock elseBlock = new AABlock();
            whileBlock.GetStatements().Add(new AIfThenElseStm(new TLParen("("), binopIf1Cond,
                                                              new ABlockStm(new TLBrace("{"), thenBlock),
                                                              new ABlockStm(new TLBrace("{"), elseBlock)));

            //ch = StringSub(s, 1, 1);
            AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("1"));
            ASimpleInvokeExp invokeStringSub1 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() {stringParamRef2Exp, intConst1, intConst2});
            AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), chRef1, invokeStringSub1);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment1));

            //s = StringReplace(s, "", 1, 1);
            AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral("1"));
            ASimpleInvokeExp invokeStringReplace1 = new ASimpleInvokeExp(new TIdentifier("StringReplace"),
                                                           new ArrayList() { stringParamRef4Exp, emptyStringRef2Exp, intConst3, intConst4 });
            AAssignmentExp assignment2 = new AAssignmentExp(new TAssign("="), stringParamRef3, invokeStringReplace1);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment2));

            //if(phase2 == 1)
            AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop1 = new ABinopExp(phase2Ref2Exp, new AEqBinop(new TEq("==")), intConst5);
            thenBlock = new AABlock();
            elseBlock.GetStatements().Add(new AIfThenStm(new TLParen("("), binop1,
                                                         new ABlockStm(new TLBrace("{"), thenBlock)));

            //ch = StringSub(s, length, length);
            ASimpleInvokeExp invokeStringSub2 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() { stringParamRef5Exp, lengthRef3Exp, lengthRef4Exp });
            AAssignmentExp assignment3 = new AAssignmentExp(new TAssign("="), chRef2, invokeStringSub2);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment3));

            //s = StringReplace(s, "", length, length);
            ASimpleInvokeExp invokeStringReplace2 = new ASimpleInvokeExp(new TIdentifier("StringReplace"),
                                                           new ArrayList() { stringParamRef7Exp, emptyStringRef3Exp, lengthRef5Exp, lengthRef6Exp });
            AAssignmentExp assignment4 = new AAssignmentExp(new TAssign("="), stringParamRef6, invokeStringReplace2);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment4));

            //if(phase1 == 0)
            AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("0"));
            ABinopExp binop2 = new ABinopExp(phase1Ref1Exp, new AEqBinop(new TEq("==")), intConst6);
            thenBlock = new AABlock();
            elseBlock = new AABlock();
            whileBlock.GetStatements().Add(new AIfThenElseStm(new TLParen("("), binop2,
                                                              new ABlockStm(new TLBrace("{"), thenBlock),
                                                              new ABlockStm(new TLBrace("{"), elseBlock)));

            //output = ch + output;
            ABinopExp binop3 = new ABinopExp(chRef3Exp, new APlusBinop(new TPlus("+")), outputRef2Exp);
            AAssignmentExp assignment5 = new AAssignmentExp(new TAssign("="), outputRef1, binop3);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment5));

            //if(phase1 == 1)
            AABlock cBlock = elseBlock;
            AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop4 = new ABinopExp(phase1Ref2Exp, new AEqBinop(new TEq("==")), intConst7);
            thenBlock = new AABlock();
            elseBlock = new AABlock();
            cBlock.GetStatements().Add(new AIfThenElseStm(new TLParen("("), binop4,
                                                              new ABlockStm(new TLBrace("{"), thenBlock),
                                                              new ABlockStm(new TLBrace("{"), elseBlock)));

            //output = StringSub(output, 1, (StringLength(output) + 1)/2) + ch + StringSub(output, (StringLength(output) + 1)/2 + 1, StringLength(output));
            AIntConstExp intConst8 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst9 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst10 = new AIntConstExp(new TIntegerLiteral("2"));
            AIntConstExp intConst11 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst12 = new AIntConstExp(new TIntegerLiteral("2"));
            AIntConstExp intConst13 = new AIntConstExp(new TIntegerLiteral("1"));

            ASimpleInvokeExp invokeStringLength1 = new ASimpleInvokeExp(new TIdentifier("StringLength"),
                                                                        new ArrayList() {outputRef5Exp});
            ABinopExp binop5 = new ABinopExp(invokeStringLength1, new APlusBinop(new TPlus("+")), intConst9);
            ABinopExp binop6 = new ABinopExp(binop5, new ADivideBinop(new TDiv("/")), intConst10);

            ASimpleInvokeExp invokeStringSub3 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() { outputRef4Exp, intConst8, binop6});

            ABinopExp binop7 = new ABinopExp(invokeStringSub3, new APlusBinop(new TPlus("+")), chRef4Exp);


            ASimpleInvokeExp invokeStringLength2 = new ASimpleInvokeExp(new TIdentifier("StringLength"),
                                                                        new ArrayList() { outputRef7Exp });
            ABinopExp binop8 = new ABinopExp(invokeStringLength2, new APlusBinop(new TPlus("+")), intConst11);
            ABinopExp binop9 = new ABinopExp(binop8, new ADivideBinop(new TDiv("/")), intConst12);
            ABinopExp binop10 = new ABinopExp(binop9, new APlusBinop(new TPlus("+")), intConst13);


            ASimpleInvokeExp invokeStringLength3 = new ASimpleInvokeExp(new TIdentifier("StringLength"),
                                                                        new ArrayList() { outputRef8Exp });

            ASimpleInvokeExp invokeStringSub4 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() { outputRef6Exp, binop10, invokeStringLength3 });

            ABinopExp binop11 = new ABinopExp(binop7, new APlusBinop(new TPlus("+")), invokeStringSub4);

            AAssignmentExp assignment6 = new AAssignmentExp(new TAssign("="), outputRef3, binop11);

            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment6));

            //output = output + ch;
            ABinopExp binop12 = new ABinopExp(outputRef10Exp, new APlusBinop(new TPlus("+")), chRef5Exp);
            AAssignmentExp assignment7 = new AAssignmentExp(new TAssign("="), outputRef9, binop12);
            elseBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment7));

            //phase1 = phase1 - 1;
            AIntConstExp intConst14 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop13 = new ABinopExp(phase1Ref4Exp, new AMinusBinop(new TMinus("-")), intConst14);
            AAssignmentExp assignment8 = new AAssignmentExp(new TAssign("="), phase1Ref3, binop13);
            whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment8));

            //if(phase1 < 0)
            AIntConstExp intConst15 = new AIntConstExp(new TIntegerLiteral("0"));
            ABinopExp binop14 = new ABinopExp(phase1Ref5Exp, new ALtBinop(new TLt("<")), intConst15);
            thenBlock = new AABlock();
            whileBlock.GetStatements().Add(new AIfThenStm(new TLParen("("), binop14,
                                                         new ABlockStm(new TLBrace("{"), thenBlock)));

            //phase1 = phase1 + 3;
            AIntConstExp intConst16 = new AIntConstExp(new TIntegerLiteral("3"));
            ABinopExp binop15 = new ABinopExp(phase1Ref7Exp, new APlusBinop(new TPlus("+")), intConst16);
            AAssignmentExp assignment9 = new AAssignmentExp(new TAssign("="), phase1Ref6, binop15);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment9));

            //phase2 = phase2 - 1;
            AIntConstExp intConst17 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop16 = new ABinopExp(phase2Ref4Exp, new AMinusBinop(new TMinus("-")), intConst17);
            AAssignmentExp assignment10 = new AAssignmentExp(new TAssign("="), phase2Ref3, binop16);
            whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment10));

            //if(phase2 < 0)
            AIntConstExp intConst18 = new AIntConstExp(new TIntegerLiteral("0"));
            ABinopExp binop17 = new ABinopExp(phase2Ref5Exp, new ALtBinop(new TLt("<")), intConst18);
            thenBlock = new AABlock();
            whileBlock.GetStatements().Add(new AIfThenStm(new TLParen("("), binop17,
                                                         new ABlockStm(new TLBrace("{"), thenBlock)));

            //phase2 = phase2 + 2;
            AIntConstExp intConst19 = new AIntConstExp(new TIntegerLiteral("2"));
            ABinopExp binop18 = new ABinopExp(phase2Ref7Exp, new APlusBinop(new TPlus("+")), intConst19);
            AAssignmentExp assignment11 = new AAssignmentExp(new TAssign("="), phase2Ref6, binop18);
            thenBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment11));

            //length = StringLength(s);
            ASimpleInvokeExp invokeStringLength4 = new ASimpleInvokeExp(new TIdentifier("StringLength"),
                                                                        new ArrayList() { stringParamRef8Exp });
            AAssignmentExp assignment12 = new AAssignmentExp(new TAssign("="), lengthRef8, invokeStringLength4);
            whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment12));
            
            /*
                phase1 = StringFind(output, ":", false);
                phase2 = StringToInt(StringSub(output, 1, phase1 - 1));
                return StringSub(output, phase1 + 1, phase2 + phase1);
             */

            ABooleanConstExp boolConst1 = new ABooleanConstExp(new AFalseBool());
            ASimpleInvokeExp invokeStringFind = new ASimpleInvokeExp(new TIdentifier("StringFind"),
                                                                     new ArrayList()
                                                                         {outputRef11Exp, colonStringRefExp, boolConst1});
            AAssignmentExp assignment13 = new AAssignmentExp(new TAssign("="), phase1Ref8, invokeStringFind);
            methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment13));

            //phase2 = StringToInt(StringSub(output, 1, phase1 - 1));
            AIntConstExp intConst20 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst21 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop19 = new ABinopExp(phase1Ref9Exp, new AMinusBinop(new TMinus("-")), intConst21);
            ASimpleInvokeExp invokeStringSub5 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() { outputRef12Exp, intConst20, binop19});
            ASimpleInvokeExp invokeStringToInt = new ASimpleInvokeExp(new TIdentifier("StringToInt"),
                                                                      new ArrayList() { invokeStringSub5 });
            AAssignmentExp assignment14 = new AAssignmentExp(new TAssign("="), phase2Ref8, invokeStringToInt);
            methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment14));

            //return StringSub(output, phase1 + 1, phase2 + phase1);
            AIntConstExp intConst22 = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop20 = new ABinopExp(phase1Ref10Exp, new APlusBinop(new TPlus("+")), intConst22);
            ABinopExp binop21 = new ABinopExp(phase2Ref9Exp, new APlusBinop(new TPlus("+")), phase1Ref11Exp);
            ASimpleInvokeExp invokeStringSub6 = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                           new ArrayList() { outputRef12Exp, binop20, binop21 });
            methodBlock.GetStatements().Add(new AValueReturnStm(new TReturn("return"), invokeStringSub6));

            AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                 new ANamedType(new TIdentifier("string"), null),
                                                 new TIdentifier("Galaxypp_Deobfuscate"), new ArrayList() {stringParam},
                                                 methodBlock);

            //Fix data refferences.. I got tired here.

            return method;
        }

        public override void OutAAProgram(AAProgram node)
        {
            if (strings.Count == 0)
                return;


            //Obfuscate all strings
            List<string> obfuscated = new List<string>();
            foreach (AStringConstExp stringConstExp in strings)
            {
                TStringLiteral token = stringConstExp.GetStringLiteral();
                string s = token.Text.Substring(1, token.Text.Length - 2);
                obfuscated.Add(Obfuscate(s));
            }

            
            //Set invokes instead of string constants, and move varaiabes down 
            List<AFieldDecl> ignoredFields = new List<AFieldDecl>();
            List<AFieldDecl> moveFieldsIn = new List<AFieldDecl>();
            Dictionary<AFieldDecl, AMethodDecl> fieldMethods = new Dictionary<AFieldDecl, AMethodDecl>();
            for (int i = 0; i < strings.Count; i++)
            {
                AStringConstExp stringExp = strings[i];
                Token token = stringExp.GetStringLiteral();
                bool inDeobfuscator = Util.GetAncestor<AMethodDecl>(stringExp) == finalTrans.data.DeobfuscateMethod;

                if (inDeobfuscator)
                {
                    AFieldDecl field = finalTrans.data.UnobfuscatedStrings[stringExp];
                    
                    AStringConstExp newStringConst = new AStringConstExp(stringExp.GetStringLiteral());

                    field.SetInit(newStringConst);

                    AFieldLvalue fieldRef = new AFieldLvalue(new TIdentifier(field.GetName().Text, token.Line, token.Pos));
                    finalTrans.data.FieldLinks[fieldRef] = field;

                    stringExp.ReplaceBy(new ALvalueExp(fieldRef));
                }
                else
                {
                    AFieldDecl field;
                    if (!finalTrans.data.ObfuscatedStrings.ContainsKey(stringExp))
                    {
                        int line = -finalTrans.data.ObfuscatedStrings.Count - 1;
                        field = new AFieldDecl(new APublicVisibilityModifier(), null, new TConst("const", line, 0),
                                                new ANamedType(new TIdentifier("string", line, 1), null),
                                                new TIdentifier("Galaxy_pp_stringO" +
                                                                finalTrans.data.ObfuscatedStrings.Count), null);
                        //If the strings are the same - point them to same field
                        bool newField = true;
                        foreach (AStringConstExp oldStringConstExp in finalTrans.data.ObfuscatedStrings.Keys)
                        {
                            if (stringExp.GetStringLiteral().Text == oldStringConstExp.GetStringLiteral().Text)
                            {
                                field = finalTrans.data.ObfuscatedStrings[oldStringConstExp];
                                newField = false;
                                break;
                            }
                        }
                        if (newField)
                        {
                            AASourceFile file = (AASourceFile)finalTrans.data.DeobfuscateMethod.Parent();
                            file.GetDecl().Insert(file.GetDecl().IndexOf(finalTrans.data.DeobfuscateMethod) + 1, field);

                            finalTrans.data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, field));
                        }
                        finalTrans.data.ObfuscatedStrings.Add(stringExp, field);
                        
                    }
                    field = finalTrans.data.ObfuscatedStrings[stringExp];
                    string obfuscatedString = obfuscated[i];

                    ASimpleInvokeExp invoke = new ASimpleInvokeExp();
                    invoke.SetName(new TIdentifier(finalTrans.data.DeobfuscateMethod.GetName().Text,
                                                   stringExp.GetStringLiteral().Line,
                                                   stringExp.GetStringLiteral().Pos));

                    AStringConstExp newStringConst =
                        new AStringConstExp(new TStringLiteral("\"" + obfuscatedString + "\""));
                    invoke.GetArgs().Add(newStringConst);
                    finalTrans.data.SimpleMethodLinks[invoke] = finalTrans.data.DeobfuscateMethod;

                    if (Util.GetAncestor<PStm>(stringExp) == null && false)
                    {
                        ignoredFields.Add(field);
                        /*if (Util.GetAncestor<ASimpleInvokeExp>(stringExp) == null)
                            stringExp.ReplaceBy(invoke);*/
                        //Add obfuscate call to this location);
                        continue;
                        /*ASimpleInvokeExp invoke = new ASimpleInvokeExp();
                        invoke.SetName(new TIdentifier(finalTrans.data.DeobfuscateMethod.GetName().Text,
                                                       stringExp.GetStringLiteral().Line,
                                                       stringExp.GetStringLiteral().Pos));

                        AStringConstExp newStringConst =
                            new AStringConstExp(new TStringLiteral("\"" + obfuscatedString + "\""));
                        invoke.GetArgs().Add(newStringConst);
                        stringExp.ReplaceBy(invoke);

                        finalTrans.data.SimpleMethodLinks[invoke] = finalTrans.data.DeobfuscateMethod;
                        continue;*/
                    }

                    if (field.GetInit() == null)
                    {
                        /*field.SetInit(invoke);
                        field.SetConst(null);*/

                        
                        if (
                            stringExp.GetStringLiteral().Text.Remove(0, 1).Substring(0,
                                                                                     stringExp.GetStringLiteral().Text.
                                                                                         Length - 2) == "")
                        {
                            //Make method
                            /*
                                string <field>Method()
                                {
                                    return "";
                                }
                             * 
                             */
                            ANullExp nullExp = new ANullExp();

                            field.SetInit(nullExp);
                            field.SetConst(null);


                            AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\"\""));
                            AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new ANamedType(new TIdentifier("string"), null),
                                                                 new TIdentifier("Get" + field.GetName()),
                                                                 new ArrayList(),
                                                                 new AABlock(
                                                                     new ArrayList()
                                                                         {
                                                                             new AValueReturnStm(new TReturn("return"),
                                                                                                 stringConst)
                                                                         },
                                                                     new TRBrace("}")));

                            AASourceFile pFile = (AASourceFile)field.Parent();
                            pFile.GetDecl().Insert(pFile.GetDecl().IndexOf(field) + 1, method);
                            finalTrans.data.ExpTypes[stringConst] = new ANamedType(new TIdentifier("string"), null);
                            finalTrans.data.ExpTypes[nullExp] = new ANamedType(new TIdentifier("null"), null);

                            fieldMethods[field] = method;
                        }
                        else
                        {
                            //Make method
                            /*
                                string <field>Method()
                                {
                                    if (field == null)
                                    {
                                        field = Invoke;
                                    }
                                    if (field == null)
                                    {
                                        return Invoke;
                                    }
                                    return field;
                                }
                             */

                            ANullExp nullExp1 = new ANullExp();

                            field.SetInit(nullExp1);
                            field.SetConst(null);


                            ANullExp nullExp2 = new ANullExp();
                            AFieldLvalue fieldRef1 = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                            AFieldLvalue fieldRef2 = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                            AFieldLvalue fieldRef3 = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                            ALvalueExp fieldRef1Exp = new ALvalueExp(fieldRef1);
                            ALvalueExp fieldRef3Exp = new ALvalueExp(fieldRef3);
                            ABinopExp binop1 = new ABinopExp(fieldRef1Exp, new AEqBinop(new TEq("==")), nullExp2);
                            AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), fieldRef2, invoke);

                            AIfThenStm ifStm1 = new AIfThenStm(new TLParen("("), binop1,
                                                              new ABlockStm(new TLBrace("{"),
                                                                            new AABlock(
                                                                                new ArrayList()
                                                                                    {
                                                                                        new AExpStm(new TSemicolon(";"),
                                                                                                    assignment)
                                                                                    },
                                                                                new TRBrace("}"))));

                            /*ANullExp nullExp3 = new ANullExp();
                            AFieldLvalue fieldRef4 = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                            ALvalueExp fieldRef4Exp = new ALvalueExp(fieldRef4);
                            AStringConstExp invokeArgClone =
                                new AStringConstExp(new TStringLiteral("\"" + obfuscatedString + "\""));
                            ASimpleInvokeExp invokeClone = new ASimpleInvokeExp(new TIdentifier(invoke.GetName().Text),
                                                                                new ArrayList() { invokeArgClone });
                            finalTrans.data.SimpleMethodLinks[invokeClone] = finalTrans.data.DeobfuscateMethod;
                            ABinopExp binop2 = new ABinopExp(fieldRef4Exp, new AEqBinop(new TEq("==")), nullExp3);
                            
                            AIfThenStm ifStm2 = new AIfThenStm(new TLParen("("), binop2,
                                                              new ABlockStm(new TLBrace("{"),
                                                                            new AABlock(
                                                                                new ArrayList()
                                                                                    {
                                                                                        new AValueReturnStm(new TReturn("return"), invokeClone)
                                                                                    },
                                                                                new TRBrace("}"))));*/


                            AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new ANamedType(new TIdentifier("string"), null),
                                                                 new TIdentifier("Get" + field.GetName()),
                                                                 new ArrayList(),
                                                                 new AABlock(
                                                                     new ArrayList()
                                                                         {
                                                                             ifStm1,
                                                                             //ifStm2,
                                                                             new AValueReturnStm(new TReturn("return"),
                                                                                                 fieldRef3Exp)
                                                                         },
                                                                     new TRBrace("}")));
                            AASourceFile pFile = (AASourceFile) field.Parent();
                            pFile.GetDecl().Insert(pFile.GetDecl().IndexOf(field) + 1, method);

                            finalTrans.data.FieldLinks[fieldRef1] =
                                finalTrans.data.FieldLinks[fieldRef2] =
                                finalTrans.data.FieldLinks[fieldRef3] =
                                /*finalTrans.data.FieldLinks[fieldRef4] = */field;
                            finalTrans.data.LvalueTypes[fieldRef1] =
                                finalTrans.data.LvalueTypes[fieldRef2] =
                                finalTrans.data.LvalueTypes[fieldRef3] =
                                //finalTrans.data.LvalueTypes[fieldRef4] =
                                finalTrans.data.ExpTypes[fieldRef1Exp] =
                                finalTrans.data.ExpTypes[fieldRef3Exp] =
                                //finalTrans.data.ExpTypes[fieldRef4Exp] =
                                finalTrans.data.ExpTypes[assignment] = field.GetType();

                            finalTrans.data.ExpTypes[nullExp1] =
                                finalTrans.data.ExpTypes[nullExp2] =
                                /*finalTrans.data.ExpTypes[nullExp3] =*/ new ANamedType(new TIdentifier("null"), null);
                            finalTrans.data.ExpTypes[binop1] =
                                /*finalTrans.data.ExpTypes[binop2] = */new ANamedType(new TIdentifier("bool"), null);

                            fieldMethods[field] = method;
                        }
                        
                        /* AFieldLvalue fieldRef = new AFieldLvalue(new TIdentifier(field.GetName().Text, token.Line, token.Pos));
                         finalTrans.data.FieldLinks[fieldRef] = field;*/



                        //stringExp.ReplaceBy(new ALvalueExp(fieldRef));
                    }
                    ASimpleInvokeExp invoke2 =
                            new ASimpleInvokeExp(new TIdentifier(fieldMethods[field].GetName().Text), new ArrayList());
                    finalTrans.data.SimpleMethodLinks[invoke2] = fieldMethods[field];
                    stringExp.ReplaceBy(invoke2);

                    //If we are in a field, move it in
                    if (Util.GetAncestor<AFieldDecl>(invoke2) != null)
                        moveFieldsIn.Add(Util.GetAncestor<AFieldDecl>(invoke2));
                }
            }

            foreach (AFieldDecl field in finalTrans.data.ObfuscationFields)
            {
                if (field.GetInit() == null && field.Parent() != null)
                {
                    field.Parent().RemoveChild(field);
                }
            }

            //A constant field, or a field used by a constant field cannot be moved in
            List<AFieldDecl> constantFields = new List<AFieldDecl>();
            foreach (SharedData.DeclItem<AFieldDecl> field in finalTrans.data.Fields)
            {
                if (field.Decl.GetConst() != null)
                    constantFields.Add(field.Decl);
            }
            for (int i = 0; i < constantFields.Count; i++)
            {
                GetFieldLvalues lvalues = new GetFieldLvalues();
                constantFields[i].Apply(lvalues);
                foreach (AFieldLvalue lvalue in lvalues.Lvalues)
                {
                    AFieldDecl field = finalTrans.data.FieldLinks[lvalue];
                    if (!constantFields.Contains(field))
                        constantFields.Add(field);
                }
            }
            moveFieldsIn.RemoveAll(constantFields.Contains);
            Dictionary<AFieldDecl, List<AFieldDecl>> dependancies = new Dictionary<AFieldDecl, List<AFieldDecl>>();
            //Order the fields so any dependancies are instansiated first
            foreach (AFieldDecl field in moveFieldsIn)
            {
                dependancies.Add(field, new List<AFieldDecl>());
                GetFieldLvalues lvalues = new GetFieldLvalues();
                field.Apply(lvalues);
                foreach (AFieldLvalue lvalue in lvalues.Lvalues)
                {
                    AFieldDecl dependancy = finalTrans.data.FieldLinks[lvalue];
                    if (!dependancies[field].Contains(dependancy))
                        dependancies[field].Add(dependancy);
                }
            }
            List<PStm> newStatements = new List<PStm>();
            while (dependancies.Keys.Count > 0)
            {
                AFieldDecl field = dependancies.FirstOrDefault(f1 => f1.Value.Count == 0).Key ??
                                   dependancies.Keys.First(f => true);

                AFieldLvalue fieldRef = new AFieldLvalue(new TIdentifier(field.GetName().Text));
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), fieldRef, field.GetInit());
                field.SetInit(null);

                newStatements.Add(new AExpStm(new TSemicolon(";"), assignment));

                finalTrans.data.FieldLinks[fieldRef] = field;
                finalTrans.data.LvalueTypes[fieldRef] =
                    finalTrans.data.ExpTypes[assignment] = field.GetType();

                foreach (KeyValuePair<AFieldDecl, List<AFieldDecl>> dependancy in dependancies)
                {
                    if (dependancy.Value.Contains(field))
                        dependancy.Value.Remove(field);
                }

                dependancies.Remove(field);
            }
            AABlock initBody = (AABlock) finalTrans.mainEntry.GetBlock();
            for (int i = newStatements.Count - 1; i >= 0; i--)
            {
                initBody.GetStatements().Insert(0, newStatements[i]);
            }
        }

        private class GetFieldLvalues : DepthFirstAdapter
        {
            public readonly List<AFieldLvalue> Lvalues = new List<AFieldLvalue>();

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                Lvalues.Add(node);
                base.CaseAFieldLvalue(node);
            }
        }

        private string Obfuscate(string sParam)
        {
            StringWrapper s = new StringWrapper(sParam);
            Random rand = new Random();
            s = s.Length + ":" + s;
            while (s.Length % 32 > 0)
            {
                int i = rand.Next('z' - 'a' + 'Z' - 'A' + '9' - '0');
                i -= 'z' - 'a';
                if (i < 0)
                    s += (char)('a' + rand.Next('z' - 'a'));
                else
                {
                    i -= 'Z' - 'A';
                    if (i < 0)
                        s += (char) ('A' + rand.Next('Z' - 'A'));
                    else
                        s += (char) ('0' + rand.Next('9' - '0'));

                }
            }

            

            StringWrapper output = new StringWrapper("");

            int phase1 = 0;
            int phase2 = 0;
            while (s.Length > 0)
            {
                string ch = "";
                switch (phase1)
                {
                    case 0:
                        ch = s[0];
                        s = s.Remove(0, 1);
                        break;
                    case 1:
                        ch = s[s.Length / 2];
                        s = s.Remove(s.Length / 2, 1);
                        break;
                    case 2:
                        ch = s[s.Length - 1];
                        s = s.Remove(s.Length - 1, 1);
                        break;
                }
                switch (phase2)
                {
                    case 0:
                        output = ch + output;
                        break;
                    case 1:
                        output = output + ch;
                        break;
                }
                phase1 = (phase1 + 1) % 3;
                phase2 = (phase2 + 1) % 2;
            }


           /* for (int i = output.Length - 1; i >= 0; i--)
            {
                if (output[i] == "\\")
                    output = output.Insert(i, "\\");
            }*/

            return output.Str;
        }

        private class StringWrapper
        {
            public string Str { get; set; }

            public StringWrapper(string s)
            {
                Str = s;
            }

            public int Length
            {
                get
                {
                    int len = 0;
                    for (int i = 0; i < Str.Length; i++)
                    {
                        if (Str[i] == '\\')
                            i++;
                        len++;
                    }
                    return len;
                }
            }

            private int ToRealIndex(int index, out bool wChar)
            {
                int len = 0;
                for (int i = 0; i < Str.Length; i++)
                {
                    wChar = Str[i] == '\\';

                    if (index == len)
                        return i;
                    if (Str[i] == '\\')
                    {

                        i++;
                    }
                    len++;
                }
                throw new IndexOutOfRangeException();
            }

            private int FromRealIndex(int index)
            {
                for (int i = 0; i < index; i++)
                {
                    if (Str[i] == '\\')
                    {
                        i++;
                        index--;
                    }
                }
                return index;
            }

            public string this[int index]
            {
                get
                {
                    bool wChar;
                    int i = ToRealIndex(index, out wChar);
                    string str = Str[i].ToString();
                    if (wChar)
                        str += Str[i + 1];
                    return str;
                }
            }

            public StringWrapper Remove(int index, int count)
            {
                StringWrapper returner = new StringWrapper(Str);
                if (count <= 0) return returner;

                bool wChar;
                int i = returner.ToRealIndex(index, out wChar);
                returner.Str = returner.Str.Remove(i, wChar ? 2 : 1);
                return returner.Remove(index, count - (wChar ? 2 : 1));
            }

            public StringWrapper Substring(int index)
            {
                return Substring(index, Length - index);
            }

            public StringWrapper Substring(int index, int count)
            {
                if (count == 0)
                    return new StringWrapper("");

                bool wChar;
                int i = ToRealIndex(index, out wChar);

                int realCount = 0;
                for (int c = 0; c < count; c++)
                {
                    realCount++;
                    ToRealIndex(index + c, out wChar);

                    if (wChar)
                        realCount++;
                }

                return new StringWrapper(Str.Substring(i, realCount));
            }

            public int IndexOf(char c)
            {
                return FromRealIndex(Str.IndexOf(c));
            }

            public static StringWrapper operator +(StringWrapper s1, StringWrapper s2)
            {
                return new StringWrapper(s1.Str + s2.Str);
            }

            public static StringWrapper operator +(StringWrapper s1, string other)
            {
                return new StringWrapper(s1.Str + other);
            }

            public static StringWrapper operator +(string other, StringWrapper s1)
            {
                return new StringWrapper(other + s1.Str);
            }

            public static StringWrapper operator +(StringWrapper s1, char other)
            {
                return new StringWrapper(s1.Str + other);
            }

            public override string ToString()
            {
                return Str;
            }
        }
    }
}
