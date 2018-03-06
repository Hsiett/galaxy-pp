using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class Delegates : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        SharedData data
        {
            get { return finalTrans.data; }
        }

        public Delegates(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        private AMethodDecl GetMethodPartMethod;
        private AMethodDecl GetMethodMethod()
        {
            if (GetMethodPartMethod != null)
                return GetMethodPartMethod;
            /*  
                string GetMethodPart(string delegate)
                {
	                int i = StringFind(delegate, ":", false);
	                if (i == -1)
	                {
		                return delegate;
	                }
	                return StringSub(delegate, 1, i - 1);
                }
             */
            AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(finalTrans.mainEntry);

            AALocalDecl delegateFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("delegate"), null);
            ALocalLvalue delegateRef1 = new ALocalLvalue(new TIdentifier("delegate"));
            ALocalLvalue delegateRef2 = new ALocalLvalue(new TIdentifier("delegate"));
            ALocalLvalue delegateRef3 = new ALocalLvalue(new TIdentifier("delegate"));
            ALvalueExp delegateRef1Exp = new ALvalueExp(delegateRef1);
            ALvalueExp delegateRef2Exp = new ALvalueExp(delegateRef2);
            ALvalueExp delegateRef3Exp = new ALvalueExp(delegateRef3);

            AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\":\""));
            ABooleanConstExp booleanConst = new ABooleanConstExp(new AFalseBool());
            AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("-1"));
            AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("1"));
            AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("1"));

            ASimpleInvokeExp stringFindInvoke = new ASimpleInvokeExp(new TIdentifier("StringFind"),
                                                                     new ArrayList()
                                                                         {delegateRef1Exp, stringConst, booleanConst});
            AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                new TIdentifier("i"), stringFindInvoke);
            ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
            ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
            ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
            ALvalueExp iRef2Exp = new ALvalueExp(iRef2);

            ABinopExp binop1 = new ABinopExp(iRef1Exp, new AEqBinop(new TEq("==")), intConst1);
            AIfThenStm ifThen = new AIfThenStm(new TLParen("("), binop1,
                                               new ABlockStm(new TLBrace("{"),
                                                             new AABlock(
                                                                 new ArrayList()
                                                                     {
                                                                         new AValueReturnStm(new TReturn("return"),
                                                                                             delegateRef2Exp)
                                                                     },
                                                                 new TRBrace("}"))));

            ABinopExp binop2 = new ABinopExp(iRef2Exp, new AMinusBinop(new TMinus("-")), intConst3);
            ASimpleInvokeExp stringSubInvoke = new ASimpleInvokeExp(new TIdentifier("StringSub"),
                                                                    new ArrayList() {delegateRef3Exp, intConst2, binop2});

            GetMethodPartMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                  new ANamedType(new TIdentifier("string"), null),
                                                  new TIdentifier("GetMethodPart", finalTrans.data.LineCounts[sourceFile] + 1, 1), new ArrayList() { delegateFormal },
                                                  new AABlock(
                                                      new ArrayList()
                                                          {
                                                              new ALocalDeclStm(new TSemicolon(";"), iDecl),
                                                              ifThen,
                                                              new AValueReturnStm(new TReturn("return"), stringSubInvoke)
                                                          },
                                                      new TRBrace("}")));
            sourceFile.GetDecl().Add(GetMethodPartMethod);

            finalTrans.data.LocalLinks[delegateRef1] =
                finalTrans.data.LocalLinks[delegateRef2] =
                finalTrans.data.LocalLinks[delegateRef3] = delegateFormal;
            finalTrans.data.LocalLinks[iRef1] =
                finalTrans.data.LocalLinks[iRef2] = iDecl;
            finalTrans.data.LvalueTypes[delegateRef1] =
                finalTrans.data.LvalueTypes[delegateRef2] =
                finalTrans.data.LvalueTypes[delegateRef3] =
                finalTrans.data.ExpTypes[delegateRef1Exp] =
                finalTrans.data.ExpTypes[delegateRef2Exp] =
                finalTrans.data.ExpTypes[delegateRef3Exp] =
                finalTrans.data.ExpTypes[stringConst] =
                finalTrans.data.ExpTypes[stringSubInvoke] = new ANamedType(new TIdentifier("string"), null);
            finalTrans.data.LvalueTypes[iRef1] =
                finalTrans.data.LvalueTypes[iRef2] =
                finalTrans.data.ExpTypes[iRef1Exp] =
                finalTrans.data.ExpTypes[iRef2Exp] =
                finalTrans.data.ExpTypes[stringFindInvoke] =
                finalTrans.data.ExpTypes[intConst1] =
                finalTrans.data.ExpTypes[intConst2] =
                finalTrans.data.ExpTypes[intConst3] =
                finalTrans.data.ExpTypes[binop2] = new ANamedType(new TIdentifier("int"), null);
            finalTrans.data.ExpTypes[booleanConst] =
                finalTrans.data.ExpTypes[binop1] = new ANamedType(new TIdentifier("bool"), null);


            finalTrans.data.SimpleMethodLinks[stringFindInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringFindInvoke.GetName().Text);
            finalTrans.data.SimpleMethodLinks[stringSubInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringSubInvoke.GetName().Text);

            return GetMethodPartMethod;
        }

        private AMethodDecl GetStringPointerPartMethod;
        private AMethodDecl GetIntPointerPartMethod;
        private AMethodDecl GetPointerMethod(bool intPointer)
        {
            if (intPointer)
                return GetIntPointerMethod();
            return GetStringPointerMethod();
        }
        private AMethodDecl GetStringPointerMethod()
        {
            if (GetStringPointerPartMethod != null)
                return GetStringPointerPartMethod;
            /*
             *  string GetPointerPart(string delegate)
             *  {
	         *      return StringSub(delegate, StringFind(delegate, ":", false) + 1, StringLength(delegate));
             *  }
             */
            
            AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(finalTrans.mainEntry);
            AALocalDecl delegateFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("delegate"), null);
            ALocalLvalue delegateRef1 = new ALocalLvalue(new TIdentifier("delegate"));
            ALocalLvalue delegateRef2 = new ALocalLvalue(new TIdentifier("delegate"));
            ALocalLvalue delegateRef3 = new ALocalLvalue(new TIdentifier("delegate"));
            ALvalueExp delegateRef1Exp = new ALvalueExp(delegateRef1);
            ALvalueExp delegateRef2Exp = new ALvalueExp(delegateRef2);
            ALvalueExp delegateRef3Exp = new ALvalueExp(delegateRef3);

            AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\":\""));
            ABooleanConstExp booleanConst = new ABooleanConstExp(new AFalseBool());
            ASimpleInvokeExp stringFindInvoke = new ASimpleInvokeExp(new TIdentifier("StringFind"), new ArrayList(){delegateRef2Exp, stringConst, booleanConst});
            AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral("1"));
            ABinopExp binop = new ABinopExp(stringFindInvoke, new APlusBinop(new TPlus("+")), intConst);

            ASimpleInvokeExp stringLengthInvoke = new ASimpleInvokeExp(new TIdentifier("StringLength"), new ArrayList() { delegateRef3Exp });

            ASimpleInvokeExp stringSubInvoke = new ASimpleInvokeExp(new TIdentifier("StringSub"), new ArrayList(){delegateRef1Exp, binop, stringLengthInvoke});

            GetStringPointerPartMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                 new ANamedType(new TIdentifier("string"), null),
                                                 new TIdentifier("GetPointerPart", finalTrans.data.LineCounts[sourceFile] + 1, 1), new ArrayList() {delegateFormal},
                                                 new AABlock(
                                                     new ArrayList()
                                                         {new AValueReturnStm(new TReturn("return"), stringSubInvoke)},
                                                     new TRBrace("}")));
            sourceFile.GetDecl().Add(GetStringPointerPartMethod);
            data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, GetStringPointerPartMethod));

            finalTrans.data.LocalLinks[delegateRef1] =
                finalTrans.data.LocalLinks[delegateRef2] =
                finalTrans.data.LocalLinks[delegateRef3] = delegateFormal;
            finalTrans.data.LvalueTypes[delegateRef1] =
                finalTrans.data.LvalueTypes[delegateRef2] =
                finalTrans.data.LvalueTypes[delegateRef3] =
                finalTrans.data.ExpTypes[delegateRef1Exp] =
                finalTrans.data.ExpTypes[delegateRef2Exp] =
                finalTrans.data.ExpTypes[delegateRef3Exp] =
                finalTrans.data.ExpTypes[stringConst] =
                finalTrans.data.ExpTypes[stringSubInvoke] = new ANamedType(new TIdentifier("string"), null);
            finalTrans.data.ExpTypes[booleanConst] = new ANamedType(new TIdentifier("bool"), null);
            finalTrans.data.ExpTypes[intConst] =
                finalTrans.data.ExpTypes[binop] =
                finalTrans.data.ExpTypes[stringFindInvoke] =
                finalTrans.data.ExpTypes[stringLengthInvoke] = new ANamedType(new TIdentifier("int"), null);


            finalTrans.data.SimpleMethodLinks[stringFindInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringFindInvoke.GetName().Text);
            finalTrans.data.SimpleMethodLinks[stringLengthInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringLengthInvoke.GetName().Text);
            finalTrans.data.SimpleMethodLinks[stringSubInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringSubInvoke.GetName().Text);

            return GetStringPointerPartMethod;
        }

        private AMethodDecl GetIntPointerMethod()
        {
            if (GetIntPointerPartMethod != null)
                return GetIntPointerPartMethod;
            /*
             *  int GetIntPointerPart(string delegate)
             *  {
	         *      return IntToString(GetPointerPart(delegate));
             *  }
             */

            AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(finalTrans.mainEntry);
            AALocalDecl delegateFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("delegate"), null);
            ALocalLvalue delegateRef1 = new ALocalLvalue(new TIdentifier("delegate"));
            ALvalueExp delegateRef1Exp = new ALvalueExp(delegateRef1);

            ASimpleInvokeExp getPointerPartInvoke = new ASimpleInvokeExp(new TIdentifier("GetPointerPart"), new ArrayList() { delegateRef1Exp });

            ASimpleInvokeExp StringToIntInvoke = new ASimpleInvokeExp(new TIdentifier("StringToInt"), new ArrayList() { getPointerPartInvoke });

            GetIntPointerPartMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                 new ANamedType(new TIdentifier("int"), null),
                                                 new TIdentifier("GetPointerPart", finalTrans.data.LineCounts[sourceFile] + 1, 1), new ArrayList() { delegateFormal },
                                                 new AABlock(
                                                     new ArrayList() { new AValueReturnStm(new TReturn("return"), StringToIntInvoke) },
                                                     new TRBrace("}")));
            sourceFile.GetDecl().Add(GetIntPointerPartMethod);
            data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, GetIntPointerPartMethod));

            finalTrans.data.LocalLinks[delegateRef1] = delegateFormal;
            finalTrans.data.LvalueTypes[delegateRef1] =
                finalTrans.data.ExpTypes[delegateRef1Exp] =
                finalTrans.data.ExpTypes[getPointerPartInvoke] = new ANamedType(new TIdentifier("string"), null);
            finalTrans.data.ExpTypes[StringToIntInvoke] = new ANamedType(new TIdentifier("int"), null);


            finalTrans.data.SimpleMethodLinks[getPointerPartInvoke] = GetStringPointerMethod();
            finalTrans.data.SimpleMethodLinks[StringToIntInvoke] =
                finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == StringToIntInvoke.GetName().Text);

            return GetIntPointerPartMethod;
        }


        private Dictionary<AMethodDecl, string> names = new Dictionary<AMethodDecl, string>();
        private string nextName = "a";

        private string NextName()
        {
            string prevName = nextName;
            int i = nextName.Length - 1;
            while (true)
            {
                if (i == -1)
                {
                    nextName += "a";
                }
                else if (nextName[i] == 'z')
                {
                    nextName = nextName.Remove(i, 1).Insert(i, "A");
                }
                else if (nextName[i] == 'Z')
                {
                    if (i > 0)
                        nextName = nextName.Remove(i, 1).Insert(i, "0");
                    else
                    {
                        nextName = nextName.Remove(i, 1).Insert(i, "a") + "a";
                    }
                }
                else if (nextName[i] == '9')
                {
                    nextName = nextName.Remove(i, 1).Insert(i, "a");
                    i--;
                    continue;
                }
                else
                {
                    char ch = nextName[i];
                    nextName = nextName.Remove(i, 1).Insert(i, ((char)(ch + 1)).ToString());
                }
                return prevName;
            }
        }

        private string GetName(AMethodDecl method)
        {
            if (names.ContainsKey(method))
                return names[method];
            return names[method] = NextName();
        }

        public override void CaseADelegateExp(ADelegateExp node)
        {
            /* Replace delegate<Type>(method) 
             * With
             * 
             * "method"
             * 
             * or
             * 
             * "method:reciever"
             */
            AMethodDecl method = finalTrans.data.DelegateCreationMethod[node];
            string d = GetName(method);
            PExp replacer;
            APointerLvalue reciever = finalTrans.data.DelegateRecieveres[node];
            if (reciever != null)
            {
                d += ":";
                AStringConstExp leftSide = new AStringConstExp(new TStringLiteral("\"" + d + "\""));
                replacer = new ABinopExp(leftSide, new APlusBinop(new TPlus("+")), reciever.GetBase());
                finalTrans.data.ExpTypes[leftSide] =
                    finalTrans.data.ExpTypes[replacer] = new ANamedType(new TIdentifier("string"), null);

                if (Util.IsIntPointer(reciever, data.LvalueTypes[reciever], data))
                {

                    ASimpleInvokeExp intToStringInvoke = new ASimpleInvokeExp(new TIdentifier("IntToString"),
                                                                              new ArrayList()
                                                                                  {((ABinopExp) replacer).GetRight()});
                    ((ABinopExp)replacer).SetRight(intToStringInvoke);

                    finalTrans.data.SimpleMethodLinks[intToStringInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke.GetName().Text);
                    data.ExpTypes[intToStringInvoke] = new ANamedType(new TIdentifier("string"), null);

                }
            }
            else
            {
                replacer = new AStringConstExp(new TStringLiteral("\"" + d + "\""));
                finalTrans.data.ExpTypes[replacer] = new ANamedType(new TIdentifier("string"), null);
            }
            MoveMethodDeclsOut mover = new MoveMethodDeclsOut("delegateVar", finalTrans.data);
            node.Apply(mover);
            node.ReplaceBy(replacer);
            foreach (PStm stm in mover.NewStatements)
            {
                stm.Apply(this);
            }
        }

        public override void CaseADelegateInvokeExp(ADelegateInvokeExp node)
        {
            //Build a list of the possible methods
            AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
            List<AMethodDecl> methods = new List<AMethodDecl>();
            ANamedType type = (ANamedType) finalTrans.data.ExpTypes[node.GetReceiver()];
            AMethodDecl delegateMethod = finalTrans.data.DelegateTypeLinks[type];
            foreach (KeyValuePair<ADelegateExp, AMethodDecl> delegateCreationPair in finalTrans.data.DelegateCreationMethod)
            {
                if (TypeChecking.Assignable(delegateCreationPair.Key.GetType(), type))
                {
                    if (!methods.Contains(delegateCreationPair.Value))
                        methods.Add(delegateCreationPair.Value);
                }
            }
            MoveMethodDeclsOut mover;
            if (methods.Count == 0)
            {
                //Can only remove it if the return value is unused
                if (!(node.Parent() is AExpStm))
                {
                    finalTrans.errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                                    currentFile,
                                                                    "No possible methods found for delegate invoke."));
                    throw new ParserException(node.GetToken(), "Delegates.OutADelegateInvokeExp");
                }

                mover = new MoveMethodDeclsOut("delegateVar", finalTrans.data);
                foreach (Node arg in node.GetArgs())
                {
                    arg.Apply(mover);
                }
                node.Parent().Parent().RemoveChild(node.Parent());
                foreach (PStm stm in mover.NewStatements)
                {
                    stm.Apply(this);
                }
                return;
            }
            if (methods.Count == 1)
            {
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("renameMe"), new ArrayList());
                while (node.GetArgs().Count > 0)
                {
                    invoke.GetArgs().Add(node.GetArgs()[0]);
                }

                //If we have a struct method, add the pointer from the delegate
                if (finalTrans.data.StructMethods.Any(str => str.Value.Contains(methods[0])))
                {
                    AStructDecl targetStr = finalTrans.data.StructMethods.First(str => str.Value.Contains(methods[0])).Key;
                    AMethodDecl getPointerDecl = GetPointerMethod(targetStr.GetDimention() != null);
                    ASimpleInvokeExp getPointerInvoke = new ASimpleInvokeExp(new TIdentifier("renameMe"), new ArrayList(){node.GetReceiver()});
                    invoke.GetArgs().Add(getPointerInvoke);

                    finalTrans.data.SimpleMethodLinks[getPointerInvoke] = getPointerDecl;
                    finalTrans.data.ExpTypes[getPointerInvoke] = getPointerDecl.GetReturnType();
                }

                finalTrans.data.SimpleMethodLinks[invoke] = methods[0];
                finalTrans.data.ExpTypes[invoke] = methods[0].GetReturnType();
                node.ReplaceBy(invoke);
                return;
            }
            //Multiple methods. Make
            /*
             * <Methods moved out from reciever>
             * string delegate = GetMethodPart(<reciever>);
             * if (delegate == "...")
             * {
             *    Foo(...);
             * }
             * else if (delegate == "...")
             * {
             *    Bar(..., GetPointerPart(<reciever>);
             * }
             * else if(...)
             * ...
             * else
             * {
             *     UIDisplayMessage(PlayerGroupAll(), c_messageAreaDebug, StringToText("[<file>:<line>]: No methods matched delegate."));
             *     int i = 1/0;
             *     return;
             * }
             * 
             */
            AABlock block = new AABlock(new ArrayList(), new TRBrace("}"));
            mover = new MoveMethodDeclsOut("delegateVar", finalTrans.data);
            node.GetReceiver().Apply(mover);
            AMethodDecl methodPartMethod = GetMethodMethod();
            ASimpleInvokeExp methodPartInvoke = new ASimpleInvokeExp(new TIdentifier("GetMethodPart"),
                                                                     new ArrayList()
                                                                         {
                                                                             Util.MakeClone(node.GetReceiver(),
                                                                                            finalTrans.data)
                                                                         });
            finalTrans.data.SimpleMethodLinks[methodPartInvoke] = methodPartMethod;
            finalTrans.data.ExpTypes[methodPartInvoke] = methodPartMethod.GetReturnType();
            AALocalDecl methodPartDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("methodPart"), methodPartInvoke);

            block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), methodPartDecl));
            //If the invoke's return value is used, get the lvalue
            PLvalue leftSide;
            if (node.Parent() is AALocalDecl)
            {
                leftSide = new ALocalLvalue(new TIdentifier("renameMe"));
                finalTrans.data.LocalLinks[(ALocalLvalue) leftSide] = (AALocalDecl) node.Parent();
                finalTrans.data.LvalueTypes[leftSide] = new ANamedType(new TIdentifier("string"), null);
                PStm pStm = Util.GetAncestor<PStm>(node);
                AABlock pBlock = (AABlock) pStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + 1, new ABlockStm(new TLBrace("{"), block));
                node.Parent().RemoveChild(node);
            }
            else if (node.Parent() is AAssignmentExp)
            {
                AAssignmentExp assignExp = (AAssignmentExp) node.Parent();
                leftSide = assignExp.GetLvalue();
                leftSide.Apply(mover);

                PStm pStm = Util.GetAncestor<PStm>(node);
                pStm.ReplaceBy(new ABlockStm(new TLBrace("{"), block));
            }
            else if (node.Parent() is AExpStm)
            {
                //No assignments needed
                leftSide = null;
                node.Parent().ReplaceBy(new ABlockStm(new TLBrace("{"), block));
            }
            else
            {
                //Create a new local
                AALocalDecl leftSideDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                           Util.MakeClone(delegateMethod.GetReturnType(),
                                                                          finalTrans.data),
                                                           new TIdentifier("delegateVar"), null);
                ALocalLvalue leftSideLink = new ALocalLvalue(new TIdentifier("delegateVar"));
                ALvalueExp leftSideLinkExp = new ALvalueExp(leftSideLink);
                

                PStm pStm = Util.GetAncestor<PStm>(node);
                AABlock pBlock = (AABlock)pStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ABlockStm(new TLBrace("{"), block));

                node.ReplaceBy(leftSideLinkExp);

                finalTrans.data.LocalLinks[leftSideLink] = leftSideDecl;
                finalTrans.data.LvalueTypes[leftSideLink] =
                    finalTrans.data.ExpTypes[leftSideLinkExp] = leftSideDecl.GetType();

                leftSide = leftSideLink;
                block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), leftSideDecl));
            }

            ABlockStm elseBranch;
            //Make final else branch
            /* {
             *     UIDisplayMessage(PlayerGroupAll(), c_messageAreaDebug, StringToText("<file>[<line>, <pos>]: No methods matched delegate."));
             *     IntToString(1/0);
             *     return;
             * }
             */
            {
                AABlock innerBlock = new AABlock(new ArrayList(), new TRBrace("}"));
                ASimpleInvokeExp playerGroupInvoke = new ASimpleInvokeExp(new TIdentifier("PlayerGroupAll"), new ArrayList());
                AFieldLvalue messageAreaLink = new AFieldLvalue(new TIdentifier("c_messageAreaDebug"));
                ALvalueExp messageAreaLinkExp = new ALvalueExp(messageAreaLink);
                AStringConstExp stringConst =
                    new AStringConstExp(
                        new TStringLiteral("\"" + currentFile.GetName().Text.Replace('\\', '/') + "[" +
                                           node.GetToken().Line + ", " + node.GetToken().Pos +
                                           "]: Got a null delegate.\""));
                ASimpleInvokeExp stringToTextInvoke = new ASimpleInvokeExp(new TIdentifier("StringToText"),
                                                                           new ArrayList() {stringConst});
                ASimpleInvokeExp displayMessageInvoke = new ASimpleInvokeExp(new TIdentifier("UIDisplayMessage"),
                                                                             new ArrayList()
                                                                                 {
                                                                                     playerGroupInvoke,
                                                                                     messageAreaLinkExp,
                                                                                     stringToTextInvoke
                                                                                 });

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("0"));
                ABinopExp binop = new ABinopExp(intConst1, new ADivideBinop(new TDiv("/")), intConst2);
                ASimpleInvokeExp intToStringInvoke = new ASimpleInvokeExp(new TIdentifier("IntToString"),
                                                                          new ArrayList() {binop});

                innerBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), displayMessageInvoke));
                innerBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), intToStringInvoke));
                //innerBlock.GetStatements().Add(new AVoidReturnStm(new TReturn("return")));

                elseBranch = new ABlockStm(new TLBrace("{"), innerBlock);


                finalTrans.data.SimpleMethodLinks[playerGroupInvoke] =
                    finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == playerGroupInvoke.GetName().Text);
                finalTrans.data.SimpleMethodLinks[stringToTextInvoke] =
                    finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == stringToTextInvoke.GetName().Text);
                finalTrans.data.SimpleMethodLinks[displayMessageInvoke] =
                    finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == displayMessageInvoke.GetName().Text);
                finalTrans.data.SimpleMethodLinks[intToStringInvoke] =
                    finalTrans.data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke.GetName().Text);
                finalTrans.data.FieldLinks[messageAreaLink] =
                    finalTrans.data.Libraries.Fields.First(m => m.GetName().Text == messageAreaLink.GetName().Text);

                finalTrans.data.ExpTypes[playerGroupInvoke] =
                    finalTrans.data.SimpleMethodLinks[playerGroupInvoke].GetReturnType();
                finalTrans.data.LvalueTypes[messageAreaLink] =
                    finalTrans.data.ExpTypes[messageAreaLinkExp] =
                    finalTrans.data.FieldLinks[messageAreaLink].GetType();
                finalTrans.data.ExpTypes[stringToTextInvoke] =
                    finalTrans.data.SimpleMethodLinks[stringToTextInvoke].GetReturnType();
                finalTrans.data.ExpTypes[stringConst] =
                    finalTrans.data.ExpTypes[intToStringInvoke] = new ANamedType(new TIdentifier("string"), null);
                finalTrans.data.ExpTypes[displayMessageInvoke] = new AVoidType();

                finalTrans.data.ExpTypes[intConst1] =
                    finalTrans.data.ExpTypes[intConst2] =
                    finalTrans.data.ExpTypes[binop] = new ANamedType(new TIdentifier("int"), null);
            }

            foreach (AMethodDecl method in methods)
            {
             /*  * if (delegate == "...")
                 * {
                 *    Foo(...);
                 * }
                 * else if (delegate == "...")
                 * {
                 *    Bar(..., GetPointerPart(<reciever>);
                 * }
                 * else if(...)
                 * ...
                 */
                AABlock innerBlock = new AABlock(new ArrayList(), new TRBrace("}"));
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier(method.GetName().Text), new ArrayList());
                for (int i = 0; i < node.GetArgs().Count; i++)
                {
                    PExp arg = (PExp) node.GetArgs()[i];
                    invoke.GetArgs().Add(Util.MakeClone(arg, finalTrans.data));
                }
                //If we have a struct method, add the pointer from the delegate
                if (finalTrans.data.StructMethods.Any(str => str.Value.Contains(method)))
                {
                    AStructDecl targetStr = finalTrans.data.StructMethods.First(str => str.Value.Contains(method)).Key;
                    AMethodDecl getPointerDecl = GetPointerMethod(targetStr.GetDimention() != null);
                    ASimpleInvokeExp getPointerInvoke = new ASimpleInvokeExp(new TIdentifier("renameMe"), new ArrayList() { Util.MakeClone(node.GetReceiver(), data) });
                    invoke.GetArgs().Add(getPointerInvoke);

                    finalTrans.data.SimpleMethodLinks[getPointerInvoke] = getPointerDecl;
                    finalTrans.data.ExpTypes[getPointerInvoke] = getPointerDecl.GetReturnType();
                }

                finalTrans.data.SimpleMethodLinks[invoke] = method;
                finalTrans.data.ExpTypes[invoke] = method.GetReturnType();

                if (leftSide == null)
                {
                    innerBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), invoke));
                }
                else
                {
                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), Util.MakeClone(leftSide, finalTrans.data), invoke);
                    finalTrans.data.ExpTypes[assignment] = finalTrans.data.ExpTypes[invoke];
                    innerBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));
                }
                ALocalLvalue methodPartLink = new ALocalLvalue(new TIdentifier("methodPart"));
                ALvalueExp methodPartLinkExp = new ALvalueExp(methodPartLink);
                AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\"" + GetName(method) + "\""));
                finalTrans.data.LocalLinks[methodPartLink] = methodPartDecl;
                finalTrans.data.LvalueTypes[methodPartLink] =
                    finalTrans.data.ExpTypes[methodPartLinkExp] = 
                    finalTrans.data.ExpTypes[stringConst] = new ANamedType(new TIdentifier("string"), null);

                
                ABinopExp binop = new ABinopExp(methodPartLinkExp, new AEqBinop(new TEq("==")), stringConst);
                finalTrans.data.ExpTypes[binop] = new ANamedType(new TIdentifier("bool"), null);

                AIfThenElseStm ifThenElse = new AIfThenElseStm(new TLParen("("), binop, new ABlockStm(new TLBrace("{"), innerBlock), elseBranch);

                elseBranch = new ABlockStm(new TLBrace("{"), new AABlock(new ArrayList() { ifThenElse }, new TRBrace("}")));
            }

            block.GetStatements().Add(elseBranch);


        }

        public override void OutAAProgram(AAProgram node)
        {
            foreach (ANamedType type in finalTrans.data.DelegateTypeLinks.Keys)
            {
                type.SetPrimitive("string");
            }
            base.OutAAProgram(node);
        }
    }
}
