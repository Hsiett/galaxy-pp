using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.NotGenerated;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    //After pointers
    class Invokes : DepthFirstAdapter 
    {
        /*
         * 1: Get a list of all invoked methods
         * 2: Get an upper bound to the number of parameters they take, and make a parm array
         * 
         * 
         * 
         * a = Invoke(args);
         * =>
         * paramAr[0] = CreatePointer...;//If returner != void
         * paramAr[1] = CreatePointer...;
         * *paramAr[1] = arg1;
         * ...
         * 
         * TriggerInvoke(false, true);
         * 
         * //If parameters are marked with ref or out and it's a sync invoke
         * a = *paramAr[0];
         * delete paramAr[0];
         * argX = *paramAr(X);
         * delete paramAr[X];
         * 
         * 
         * 
         * //Invoke method
         * bool InvokeMehod(bool ranAsync, bool runActions)
         * {
         *      string[...] localParamAr;
         *      localParamAr = paramAr;
         *      if (ranAsync)
         *      {
         *          Wait(0, gameTime);
         *      }
         *      var ret = Method(*paramAr[1], *paramAr[2],...);    
         *      delete localParamAr[x];//If not ref or out
         *      if (ranAsync)
         *      {//Nothing can be returned, just delete the pointers
         *          delete localParamAr[0];//If not void
         *          delete localParamAr[x];//If ref or out
         *      }
         *      else
         *      {
         *          *localParamAr[0] = ret;//If not void
         *      }
         * }
         */


        public static void Parse(FinalTransformations finalTrans)
        {
            SharedData data = finalTrans.data;
            int MaxParams = data.Invokes.Keys.Max(m => m.GetFormals().Count);
            //Create global param array
            AASourceFile file = (AASourceFile) finalTrans.mainEntry.Parent();
            AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral((MaxParams + 1).ToString()));
            AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral((MaxParams + 1).ToString()));
            AFieldDecl stringParamArray = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["),
                                                                     new ANamedType(new TIdentifier("string"), null),
                                                                     intConst1,
                                                                     new TIntegerLiteral((MaxParams + 1).ToString())),
                                                  new TIdentifier("stringParamArray", data.LineCounts[file] + 10, 0), null);
            file.GetDecl().Add(stringParamArray);
            AFieldDecl intParamArray = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["),
                                                                     new ANamedType(new TIdentifier("int"), null),
                                                                     intConst2,
                                                                     new TIntegerLiteral((MaxParams + 1).ToString())),
                                                  new TIdentifier("intParamArray", data.LineCounts[file] + 10, 0), null);
            file.GetDecl().Add(intParamArray);
            data.ExpTypes[intConst1] = data.ExpTypes[intConst2] = new ANamedType(new TIdentifier("int"), null);
            foreach (KeyValuePair<AMethodDecl, List<InvokeStm>> invokePair in data.Invokes)
            {
                AMethodDecl method = invokePair.Key;
                //Create a trigger method, and trigger field
                /* //Invoke method
                 * bool InvokeMehod(bool ranAsync, bool runActions)
                 * {
                 *      string[...] localParamAr; <---- correction: not array, since they are hard to optimize
                 *      localParamAr = paramAr;
                 *      if (ranAsync)
                 *      {
                 *          Wait(0, gameTime);
                 *      }
                 *      *localParamAr[0] = Method(*paramAr[1], *paramAr[2],...);//Assignment if not void    
                 *      delete localParamAr[x];//If not ref or out
                 *      if (ranAsync)
                 *      {//Nothing can be returned, just delete the pointers
                 *          delete localParamAr[0];//If not void
                 *          delete localParamAr[x];//If ref or out
                 *      }
                 *      else
                 *      {
                 *          paramAr[0] = localParamAr[0];//If not void
                 *          paramAr[x] = localParamAr[x];//If ref or out
                 *      }
                 * }
                 */
                AALocalDecl ranAsyncDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                           new ANamedType(new TIdentifier("bool"), null),
                                                           new TIdentifier("ranAsync"), null);
                AALocalDecl runActionsDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                             new ANamedType(new TIdentifier("bool"), null),
                                                             new TIdentifier("runActions"), null);
                AABlock methodBody = new AABlock(new ArrayList(), new TRBrace("}"));
                AMethodDecl invokeMethod = new AMethodDecl(new APublicVisibilityModifier(), new TTrigger("trigger"), null, null, null, null,
                                                           new ANamedType(new TIdentifier("bool"), null),
                                                           new TIdentifier("Invoke" + method.GetName().Text,
                                                                           data.LineCounts[file] + 11, 0),
                                                           new ArrayList() {ranAsyncDecl, runActionsDecl},
                                                           methodBody);

                /*AALocalDecl localParamArray = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new AArrayTempType(new TLBracket("["),
                                                                                                   new ANamedType(
                                                                                                       new TIdentifier(
                                                                                                           "string"),
                                                                                                       null),
                                                                                                   new AIntConstExp(
                                                                                                       new TIntegerLiteral
                                                                                                           ((method.
                                                                                                                 GetFormals
                                                                                                                 ().
                                                                                                                 Count +
                                                                                                             1).ToString
                                                                                                                ())),
                                                                                                   new TIntegerLiteral(
                                                                                                       (MaxParams + 1).
                                                                                                           ToString())),
                                                              new TIdentifier("localParamArray"), null);
                methodBody.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), localParamArray));*/
                AALocalDecl localReturnType = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                              new APointerType(new TStar("*"),
                                                                               Util.MakeClone(method.GetReturnType(),
                                                                                              data)),
                                                              new TIdentifier("invokeReturnVar"), null);

                if (!(method.GetReturnType() is AVoidType))
                {
                    AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                    ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                    AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                              new AIntConstExp(new TIntegerLiteral("0")));
                    ALvalueExp rightSideExp = new ALvalueExp(rightSide);
                    localReturnType.SetInit(rightSideExp);

                    PType type = localReturnType.GetType();

                    data.FieldLinks[paramArrayRef] = Util.IsIntPointer(method, method.GetReturnType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                    data.LvalueTypes[rightSide] =
                        data.ExpTypes[rightSideExp] = type;
                    AIntConstExp intDim = new AIntConstExp(new TIntegerLiteral("10"));
                    type = new AArrayTempType(new TLBracket("["), Util.MakeClone(type, data), intDim, new TIntegerLiteral("10"));
                    data.LvalueTypes[paramArrayRef] =
                        data.ExpTypes[paramArrayRefExp] = type;
                    data.ExpTypes[intDim] =
                        data.ExpTypes[rightSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                }

                methodBody.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), localReturnType));
                List<AALocalDecl> formalDecls = new List<AALocalDecl>();

                for (int i = 0; i < method.GetFormals().Count; i++)
                {
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];

                   /* ALocalLvalue localParamArrayRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                    ALvalueExp localParamArrayRefExp = new ALvalueExp(localParamArrayRef);
                    AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["), localParamArrayRefExp,
                                                             new AIntConstExp(new TIntegerLiteral(i.ToString())));*/

                    AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                    ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                    AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                              new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                    ALvalueExp rightSideExp = new ALvalueExp(rightSide);


                    AALocalDecl decl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                       new APointerType(new TStar("*"), Util.MakeClone(formal.GetType(), data)),
                                                       new TIdentifier("invokeFormal" + (i + 1)), rightSideExp);
                    formalDecls.Add(decl);
                    //AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, rightSideExp);
                    //methodBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));
                    methodBody.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), decl));

                    //data.LocalLinks[localParamArrayRef] = localParamArray;
                    data.FieldLinks[paramArrayRef] = Util.IsIntPointer(formal, formal.GetType(), data)
                                                         ? intParamArray
                                                         : stringParamArray; 
                    /*data.LvalueTypes[localParamArrayRef] =
                        data.ExpTypes[localParamArrayRefExp] =
                        data.LvalueTypes[paramArrayRef] =
                        data.ExpTypes[paramArrayRefExp] = paramArray.GetType();
                    data.LvalueTypes[leftSide] =
                        data.LvalueTypes[rightSide] =
                        data.ExpTypes[rightSideExp] =
                        data.ExpTypes[assignment] = new ANamedType(new TIdentifier("string"), null);*/
                    PType type = formal.GetType();
                    data.LvalueTypes[rightSide] =
                        data.ExpTypes[rightSideExp] = type;
                    AIntConstExp intDim = new AIntConstExp(new TIntegerLiteral("10"));
                    type = new AArrayTempType(new TLBracket("["), Util.MakeClone(type, data), intDim, new TIntegerLiteral("10"));
                    data.LvalueTypes[paramArrayRef] =
                        data.ExpTypes[paramArrayRefExp] = type;
                    data.ExpTypes[intDim] =
                        data.ExpTypes[rightSide.GetIndex()] =
                        /*data.ExpTypes[leftSide.GetIndex()] =*/ new ANamedType(new TIdentifier("int"), null);
                }
                /* if (ranAsync)
                 * {
                 *     Wait(0, c_timeGame);
                 * }
                 */

                ALocalLvalue ranAsyncLink = new ALocalLvalue(new TIdentifier("ranAsync"));
                ALvalueExp ranAsyncLinkExp = new ALvalueExp(ranAsyncLink);
                AABlock ifBody = new AABlock(new ArrayList(), new TRBrace("}"));
                AIfThenStm ifStm = new AIfThenStm(new TLParen("("), ranAsyncLinkExp,
                                                  new ABlockStm(new TLBrace("{"), ifBody));

                AFieldLvalue timeGameRef = new AFieldLvalue(new TIdentifier("c_timeGame"));
                ALvalueExp timeGameRefExp = new ALvalueExp(timeGameRef);
                AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral("0"));
                ASimpleInvokeExp waitInvoke = new ASimpleInvokeExp(new TIdentifier("Wait"),
                                                                   new ArrayList() {intConst, timeGameRefExp});
                ifBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), waitInvoke));
                methodBody.GetStatements().Add(ifStm);

                data.LocalLinks[ranAsyncLink] = ranAsyncDecl;
                data.FieldLinks[timeGameRef] =
                    data.Libraries.Fields.First(f => f.GetName().Text == timeGameRef.GetName().Text);
                data.SimpleMethodLinks[waitInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == waitInvoke.GetName().Text);
                data.LvalueTypes[ranAsyncLink] =
                    data.ExpTypes[ranAsyncLinkExp] = new ANamedType(new TIdentifier("bool"), null);
                data.ExpTypes[intConst] =
                    data.LvalueTypes[timeGameRef] =
                    data.ExpTypes[timeGameRefExp] = new ANamedType(new TIdentifier("int"), null);
                data.ExpTypes[waitInvoke] = new AVoidType(new TVoid("void"));

                //var ret = Method(*paramAr[1], *paramAr[2],...);    
                PExp[] args = new PExp[method.GetFormals().Count];
                for (int i = 0; i < args.Length; i++)
                {
                    //If the arg is bulk copy, ref or out, pass pointer, otherwise, pass value
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                    ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("invokeFormal" + (i + 1)));
                    ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                    /*AArrayLvalue arg = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                        new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                    ALvalueExp argExp = new ALvalueExp(arg);*/

                    APointerLvalue rightSideDepointered = new APointerLvalue(new TStar("*"), localParamsRefExp);
                    ALvalueExp rightSideDepointeredExp = new ALvalueExp(rightSideDepointered);

                    //data.LocalLinks[localParamsRef] = localParamArray;
                    data.LocalLinks[localParamsRef] = formalDecls[i];
                    data.LvalueTypes[localParamsRef] =
                        data.ExpTypes[localParamsRefExp] = formalDecls[i].GetType();//localParamArray.GetType();
                    data.LvalueTypes[rightSideDepointered] =
                        data.ExpTypes[rightSideDepointeredExp] = Util.MakeClone(formal.GetType(), data);
                    /*data.LvalueTypes[arg] =
                        data.ExpTypes[argExp] = new ANamedType(new TIdentifier("string"), null);
                    data.ExpTypes[arg.GetIndex()] = new ANamedType(new TIdentifier("int"), null);*/

                    /*if (!Util.IsBulkCopy(formal.GetType()) && formal.GetRef() == null && formal.GetOut() == null)
                    {
                        APointerLvalue pointer = new APointerLvalue(new TStar("*"), argExp);
                        argExp = new ALvalueExp(pointer);

                        data.LvalueTypes[pointer] =
                            data.ExpTypes[argExp] = formal.GetType();
                    }*/
                    args[i] = rightSideDepointeredExp;// argExp;
                }

                ASimpleInvokeExp methodInvoke = new ASimpleInvokeExp(new TIdentifier(method.GetName().Text),
                                                                     new ArrayList(args));
                data.BulkCopyProcessedInvokes.Add(methodInvoke);
                data.SimpleMethodLinks[methodInvoke] = method;
                data.ExpTypes[methodInvoke] = Util.MakeClone(method.GetReturnType(), data);
                if (method.GetReturnType() is AVoidType)
                {
                    data.ExpTypes[methodInvoke] = Util.MakeClone(method.GetReturnType(), data);
                    methodBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), methodInvoke));
                }
                else
                {
                    ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("invokeReturnVar"));
                    ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                    /*AArrayLvalue arg = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                        new AIntConstExp(new TIntegerLiteral("0")));
                    ALvalueExp argExp = new ALvalueExp(arg);*/


                    data.LocalLinks[localParamsRef] = localReturnType; //localParamArray;
                    data.LvalueTypes[localParamsRef] =
                        data.ExpTypes[localParamsRefExp] = localReturnType.GetType();//localParamArray.GetType();
                   /* data.LvalueTypes[arg] =
                        data.ExpTypes[argExp] = new ANamedType(new TIdentifier("string"), null);
                    data.ExpTypes[arg.GetIndex()] = new ANamedType(new TIdentifier("int"), null);*/

                    APointerLvalue pointer = new APointerLvalue(new TStar("*"), localParamsRefExp);

                    data.LvalueTypes[pointer] = Util.MakeClone(method.GetReturnType(), data);

                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="),
                                                                   /*Util.IsBulkCopy(method.GetReturnType())
                                                                       ? (PLvalue) arg
                                                                       : */pointer, methodInvoke);
                    methodBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                    data.ExpTypes[assignment] = data.LvalueTypes[assignment.GetLvalue()];
                }


                //delete localParamAr[x];//If not ref or out
                for (int i = 0; i < method.GetFormals().Count; i++)
                {
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                    if (formal.GetRef() == null && formal.GetOut() == null)
                    {
                        ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                        ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                        /*AArrayLvalue arg = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                            new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                        ALvalueExp argExp = new ALvalueExp(arg);*/

                        data.LocalLinks[localParamsRef] = formalDecls[i];//localParamArray;
                        data.LvalueTypes[localParamsRef] =
                            data.ExpTypes[localParamsRefExp] = formalDecls[i].GetType();//localParamArray.GetType();
                        /*data.LvalueTypes[arg] =
                            data.ExpTypes[argExp] = new APointerType(new TStar("*"), Util.MakeClone(data.ExpTypes[args[i]], data));
                        data.ExpTypes[arg.GetIndex()] = new ANamedType(new TIdentifier("int"), null);*/

                        methodBody.GetStatements().Add(new ADeleteStm(new TDelete("delete"), localParamsRefExp));
                    }
                }
                /*      if (ranAsync)
                 *      {//Nothing can be returned, just delete the pointers
                 *          delete localParamAr[0];//If not void
                 *          delete localParamAr[x];//If ref or out
                 *      }
                 *      else
                 *      {
                 *          paramAr[0] = localParamAr[0];//If not void
                 *          paramAr[x] = localParamAr[x];//If ref or out
                 *      }
                 */
                ranAsyncLink = new ALocalLvalue(new TIdentifier("ranAsync"));
                ranAsyncLinkExp = new ALvalueExp(ranAsyncLink);
                ifBody = new AABlock(new ArrayList(), new TRBrace("}"));
                AABlock elseBody = new AABlock(new ArrayList(), new TRBrace("}"));
                AIfThenElseStm ifElseStm = new AIfThenElseStm(new TLParen("("), ranAsyncLinkExp,
                                                              new ABlockStm(new TLBrace("{"), ifBody),
                                                              new ABlockStm(new TLBrace("{"), elseBody));

                data.LocalLinks[ranAsyncLink] = ranAsyncDecl;
                data.LvalueTypes[ranAsyncLink] =
                    data.ExpTypes[ranAsyncLinkExp] = new ANamedType(new TIdentifier("bool"), null);

                if (!(method.GetReturnType() is AVoidType))
                {
                    //Then
                    {
                        ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                        ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                        /*AArrayLvalue arg = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                            new AIntConstExp(new TIntegerLiteral("0")));
                        ALvalueExp argExp = new ALvalueExp(arg);*/

                        ifBody.GetStatements().Add(new ADeleteStm(new TDelete("delete"), localParamsRefExp));

                        data.LocalLinks[localParamsRef] = localReturnType;
                        data.LvalueTypes[localParamsRef] =
                            data.ExpTypes[localParamsRefExp] = localReturnType.GetType();
                        /*data.LvalueTypes[arg] =
                            data.ExpTypes[argExp] =
                            new APointerType(new TStar("*"), Util.MakeClone(method.GetReturnType(), data));
                        data.ExpTypes[arg.GetIndex()] = new ANamedType(new TIdentifier("int"), null);*/
                    }
                    //Else
                    {
                        ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                        ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                        /*AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                            new AIntConstExp(new TIntegerLiteral("0")));
                        ALvalueExp rightSideExp = new ALvalueExp(rightSide);*/

                        AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                        ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                        AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                                  new AIntConstExp(new TIntegerLiteral("0")));

                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, localParamsRefExp);
                        elseBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                        data.LocalLinks[localParamsRef] = localReturnType;
                        data.FieldLinks[paramArrayRef] = Util.IsIntPointer(method, method.GetReturnType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                        data.LvalueTypes[localParamsRef] =
                            data.ExpTypes[localParamsRefExp] =
                            data.LvalueTypes[leftSide] =
                            data.ExpTypes[assignment] = localReturnType.GetType();
                        //data.LvalueTypes[rightSide] =
                        //    data.ExpTypes[rightSideExp] =
                        AIntConstExp intDim = new AIntConstExp(new TIntegerLiteral("10"));
                        PType type = new AArrayTempType(new TLBracket("["), Util.MakeClone(localReturnType.GetType(), data), intDim, new TIntegerLiteral("10"));
                        data.LvalueTypes[paramArrayRef] =
                            data.ExpTypes[paramArrayRefExp] = type;
                        data.ExpTypes[leftSide.GetIndex()] =
                            data.ExpTypes[intDim] = new ANamedType(new TIdentifier("int"), null);
                    }

                }

                for (int i = 0; i < method.GetFormals().Count; i++)
                {
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                    if (formal.GetRef() != null || formal.GetOut() != null)
                    {
                        //Then
                        {
                            ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                            ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                            /*AArrayLvalue arg = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                                new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                            ALvalueExp argExp = new ALvalueExp(arg);*/

                            data.LocalLinks[localParamsRef] = formalDecls[i];
                            data.LvalueTypes[localParamsRef] =
                                data.ExpTypes[localParamsRefExp] = formalDecls[i].GetType();
                           /* data.LvalueTypes[arg] =
                                data.ExpTypes[argExp] =
                                new APointerType(new TStar("*"), Util.MakeClone(data.ExpTypes[args[i]], data));
                            data.ExpTypes[arg.GetIndex()] = new ANamedType(new TIdentifier("int"), null);*/

                            ifBody.GetStatements().Add(new ADeleteStm(new TDelete("delete"), localParamsRefExp));
                        }
                        //Else
                        {
                            ALocalLvalue localParamsRef = new ALocalLvalue(new TIdentifier("localParamArray"));
                            ALvalueExp localParamsRefExp = new ALvalueExp(localParamsRef);
                            /*AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["), localParamsRefExp,
                                                                new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                            ALvalueExp rightSideExp = new ALvalueExp(rightSide);*/

                            AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                            ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                            AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                                      new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));

                            AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, localParamsRefExp);
                            elseBody.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                            data.LocalLinks[localParamsRef] = formalDecls[i];
                            data.FieldLinks[paramArrayRef] = Util.IsIntPointer(formal, formal.GetType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                            data.LvalueTypes[localParamsRef] =
                                data.ExpTypes[localParamsRefExp] =
                                data.LvalueTypes[leftSide] =
                                data.ExpTypes[assignment] = formalDecls[i].GetType();

                            AIntConstExp intDim = new AIntConstExp(new TIntegerLiteral("10"));
                            PType type = new AArrayTempType(new TLBracket("["), Util.MakeClone(formalDecls[i].GetType(), data), intDim, new TIntegerLiteral("10"));

                            data.LvalueTypes[paramArrayRef] =
                                data.ExpTypes[paramArrayRefExp] = type;
                            data.ExpTypes[leftSide.GetIndex()] =
                                data.ExpTypes[intDim] = new ANamedType(new TIdentifier("int"), null);
                        }
                    }
                }
                if (ifBody.GetStatements().Count > 0)
                    methodBody.GetStatements().Add(ifElseStm);

                //return true;
                ABooleanConstExp boolConst = new ABooleanConstExp(new ATrueBool());
                data.ExpTypes[boolConst] = new ANamedType(new TIdentifier("bool"), null);
                methodBody.GetStatements().Add(new AValueReturnStm(new TReturn("return"), boolConst));

                file.GetDecl().Add(invokeMethod);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, invokeMethod));

                //Create trigger Invoke<Method>Trigger = TriggerCreate(...);
                AStringConstExp triggerLink = new AStringConstExp(new TStringLiteral(invokeMethod.GetName().Text));
                data.TriggerDeclarations.Add(invokeMethod, new List<TStringLiteral>{triggerLink.GetStringLiteral()});
                data.ExpTypes[triggerLink] = new ANamedType(new TIdentifier("string"), null);
                
                ASimpleInvokeExp triggerCreateInvoke = new ASimpleInvokeExp(new TIdentifier("TriggerCreate"), new ArrayList(){triggerLink});
                data.SimpleMethodLinks[triggerCreateInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == triggerCreateInvoke.GetName().Text);
                data.ExpTypes[triggerCreateInvoke] = new ANamedType(new TIdentifier("trigger"), null);

                AFieldDecl invokeField = new AFieldDecl(new APublicVisibilityModifier(), null, null, new ANamedType(new TIdentifier("trigger", data.LineCounts[file] + 11, 0), null),
                                                        new TIdentifier("Invoke" + method.GetName().Text + "Field"),
                                                        triggerCreateInvoke);
                file.GetDecl().Add(invokeField);
                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, invokeField));


                foreach (InvokeStm invokeStm in invokePair.Value)
                {
                    /* Convert to
                     * 
                     * paramArray[0] = new <return type>();//If not void
                     * paramArray[x] = new <param type();
                     * *paramArray[x] = arg<x - 1>;//If not out
                     * TriggerRun(<isAssync>, true);
                     //* Wait(0, c_timeGame);
                     * <If not async>
                     * var ret = paramArray[0];//If not void, and if the returned value is used
                     * delete paramArray[0];//If not void
                     * arg<x - 1> = paramArray[x];//If ref or out
                     * delete paramArray[x];//If ref or out
                     * </If not async>
                     * <usage>ret</usage>
                     */

                    PStm pStm = Util.GetAncestor<PStm>(invokeStm.Token);
                    AABlock pBlock = (AABlock) pStm.Parent();

                    if (!(method.GetReturnType() is AVoidType))
                    {
                        AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                        ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                        AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                                  new AIntConstExp(new TIntegerLiteral("0")));

                        ANewExp rightSide = new ANewExp(new TNew("new"), Util.MakeClone(method.GetReturnType(), data), new ArrayList());
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, rightSide);
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new AExpStm(new TSemicolon(";"), assignment));

                        data.FieldLinks[paramArrayRef] = Util.IsIntPointer(method, method.GetReturnType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                        data.ExpTypes[rightSide] =
                            data.ExpTypes[assignment] =
                            data.LvalueTypes[leftSide] = new APointerType(new TStar("*"), Util.MakeClone(method.GetReturnType(), data));
                        data.LvalueTypes[paramArrayRef] =
                            data.ExpTypes[paramArrayRefExp] =
                            new AArrayTempType(new TLBracket("["), data.LvalueTypes[leftSide],
                                               new AIntConstExp(new TIntegerLiteral((MaxParams + 1).ToString())),
                                               new TIntegerLiteral((MaxParams + 1).ToString()));
                        data.ExpTypes[leftSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                    }
                    if (invokeStm.BaseExp != null)
                    {
                        invokeStm.Args.Add(invokeStm.BaseExp);
                    }
                    for (int i = 0; i < method.GetFormals().Count; i++)
                    {
                        args[i] = (PExp) invokeStm.Args[i];
                    }
                    
                    for (int i = 0; i < method.GetFormals().Count; i++)
                    {
                        AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];

                        //paramArray[x] = new <param type>();
                        AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                        ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                        AArrayLvalue leftSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                                  new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));

                        ANewExp rightSide = new ANewExp(new TNew("new"), Util.MakeClone(formal.GetType(), data), new ArrayList());
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, rightSide);
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new AExpStm(new TSemicolon(";"), assignment));


                        data.FieldLinks[paramArrayRef] = Util.IsIntPointer(formal, formal.GetType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                        data.ExpTypes[rightSide] =
                            data.ExpTypes[assignment] =
                            data.LvalueTypes[leftSide] = new APointerType(new TStar("*"), Util.MakeClone(/*data.ExpTypes[args[i]]*/formal.GetType(), data));
                        data.LvalueTypes[paramArrayRef] =
                            data.ExpTypes[paramArrayRefExp] =
                            new AArrayTempType(new TLBracket("["), data.LvalueTypes[leftSide],
                                               new AIntConstExp(new TIntegerLiteral((MaxParams + 1).ToString())),
                                               new TIntegerLiteral((MaxParams + 1).ToString()));
                        data.ExpTypes[leftSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);

                        if (formal.GetOut() == null)
                        {
                            //paramArray[x] = arg<x - 1>;//If not out
                            paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                            paramArrayRefExp = new ALvalueExp(paramArrayRef);
                            leftSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                        new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                            ALvalueExp leftSideExp = new ALvalueExp(leftSide);
                            APointerLvalue pointer = new APointerLvalue(new TStar("*"), leftSideExp);

                            assignment = new AAssignmentExp(new TAssign("="), pointer, args[i]);
                            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new AExpStm(new TSemicolon(";"), assignment));

                            data.FieldLinks[paramArrayRef] = Util.IsIntPointer(formal, formal.GetType(), data)
                                                         ? intParamArray
                                                         : stringParamArray;
                            data.LvalueTypes[leftSide] =
                                data.ExpTypes[leftSideExp] = new APointerType(new TStar("*"), Util.MakeClone(/*data.ExpTypes[args[i]]*/formal.GetType(), data));
                            data.LvalueTypes[pointer] =
                                data.ExpTypes[assignment] = formal.GetType();//data.ExpTypes[args[i]];

                            data.LvalueTypes[paramArrayRef] =
                                data.ExpTypes[paramArrayRefExp] =
                                new AArrayTempType(new TLBracket("["), data.LvalueTypes[leftSide],
                                                   new AIntConstExp(new TIntegerLiteral((MaxParams + 1).ToString())),
                                                   new TIntegerLiteral((MaxParams + 1).ToString()));
                            data.ExpTypes[leftSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                        }
                    }
                    //TriggerExecute(triggerField, <isAssync>, true);
                    AFieldLvalue invokeFieldRef = new AFieldLvalue(new TIdentifier(invokeField.GetName().Text));
                    ALvalueExp invokeFieldRefExp = new ALvalueExp(invokeFieldRef);
                    ABooleanConstExp isAsyncArg =
                        new ABooleanConstExp(invokeStm.IsAsync ? (PBool)new ATrueBool() : new AFalseBool());
                    ABooleanConstExp waitForFinish = new ABooleanConstExp(invokeStm.IsAsync ? (PBool)new AFalseBool() : new ATrueBool());

                    ASimpleInvokeExp triggerExecuteInvoke = new ASimpleInvokeExp(new TIdentifier("TriggerExecute"),
                                                                                 new ArrayList
                                                                                     {
                                                                                         invokeFieldRefExp,
                                                                                         isAsyncArg,
                                                                                         waitForFinish
                                                                                     });
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new AExpStm(new TSemicolon(";"), triggerExecuteInvoke));

                    data.FieldLinks[invokeFieldRef] = invokeField;
                    data.LvalueTypes[invokeFieldRef] =
                            data.ExpTypes[invokeFieldRefExp] = new ANamedType(new TIdentifier("trigger"), null);
                    data.ExpTypes[isAsyncArg] = 
                        data.ExpTypes[waitForFinish] = new ANamedType(new TIdentifier("bool"), null);
                    data.ExpTypes[triggerExecuteInvoke] = new AVoidType(new TVoid("void"));
                    data.SimpleMethodLinks[triggerExecuteInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == triggerExecuteInvoke.GetName().Text);

                    if (invokeStm.IsAsync)
                    {//We're done. remove the stm
                        pBlock.RemoveChild(pStm);
                    }
                    else
                    {
                        /* <If not async>
                         * var ret = *paramArray[0];//If not void, and if the returned value is used
                         * delete paramArray[0];//If not void
                         * arg<x - 1> = *paramArray[x];//If ref or out
                         * delete paramArray[x];//If ref or out
                         * <usage>ret</usage>
                         * </If not async>
                         */
                        bool returnerIsUsed = !(invokeStm.SyncNode.Parent() is AExpStm);
                        if (!(method.GetReturnType() is AVoidType))
                        {
                            AFieldLvalue paramArrayRef;
                            ALvalueExp paramArrayRefExp;
                            AArrayLvalue rightSide;
                            ALvalueExp rightSideExp;
                            AFieldDecl paramArr;
                            if (returnerIsUsed)
                            {
                                //* var ret = *paramArray[0];//If not void, and if the returned value is used
                                paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                                paramArrayRefExp = new ALvalueExp(paramArrayRef);
                                rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                             new AIntConstExp(new TIntegerLiteral("0")));
                                rightSideExp = new ALvalueExp(rightSide);
                                APointerLvalue pointer = new APointerLvalue(new TStar("*"), rightSideExp);
                                ALvalueExp pointerExp = new ALvalueExp(pointer);

                                AALocalDecl returnerVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                          Util.MakeClone(method.GetReturnType(), data),
                                                                          new TIdentifier("invokeReturner"), pointerExp);
                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                              new ALocalDeclStm(new TSemicolon(";"), returnerVar));

                                //* <usage>ret</usage>
                                ALocalLvalue returnerVarRef = new ALocalLvalue(new TIdentifier("invokeReturner"));
                                ALvalueExp returnerVarRefExp = new ALvalueExp(returnerVarRef);

                                invokeStm.SyncNode.ReplaceBy(returnerVarRefExp);

                                paramArr = Util.IsIntPointer(method, method.GetReturnType(), data)
                                               ? intParamArray
                                               : stringParamArray;
                                data.FieldLinks[paramArrayRef] = paramArr;
                                data.LocalLinks[returnerVarRef] = returnerVar;
                                data.LvalueTypes[paramArrayRef] =
                                    data.ExpTypes[paramArrayRefExp] = paramArr.GetType();
                                data.LvalueTypes[rightSide] =
                                    data.ExpTypes[rightSideExp] =
                                    new APointerType(new TStar("*"), Util.MakeClone(method.GetReturnType(), data));
                                data.LvalueTypes[pointer] =
                                    data.ExpTypes[pointerExp] = 
                                    data.LvalueTypes[returnerVarRef] =
                                    data.ExpTypes[returnerVarRefExp] = Util.MakeClone(method.GetReturnType(), data);
                                data.ExpTypes[rightSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                            }
                            //* delete paramArray[0];//If not void
                            paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                            paramArrayRefExp = new ALvalueExp(paramArrayRef);
                            rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                         new AIntConstExp(new TIntegerLiteral("0")));
                            rightSideExp = new ALvalueExp(rightSide);
                            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                          new ADeleteStm(new TDelete("delete"), rightSideExp));

                            paramArr = Util.IsIntPointer(method, method.GetReturnType(), data)
                                           ? intParamArray
                                           : stringParamArray;
                            data.FieldLinks[paramArrayRef] = paramArr;
                            data.LvalueTypes[rightSide] =
                                data.ExpTypes[rightSideExp] =
                                new APointerType(new TStar("*"), Util.MakeClone(method.GetReturnType(), data));
                            data.LvalueTypes[paramArrayRef] =
                                data.ExpTypes[paramArrayRefExp] = paramArr.GetType();
                            data.ExpTypes[rightSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                        }
                        for (int i = 0; i < method.GetFormals().Count; i++)
                        {
                            AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                            if (formal.GetRef() != null || formal.GetOut() != null)
                            {
                                //* arg<x - 1> = *paramArray[x];//If ref or out
                                if (formal.GetRef() != null)
                                {
                                    args[i].Apply(new MoveMethodDeclsOut("invokeVar", data));
                                }
                                AFieldLvalue paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                                ALvalueExp paramArrayRefExp = new ALvalueExp(paramArrayRef);
                                AArrayLvalue rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                             new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                                ALvalueExp rightSideExp = new ALvalueExp(rightSide);
                                APointerLvalue pointer = new APointerLvalue(new TStar("*"), rightSideExp);
                                ALvalueExp pointerExp = new ALvalueExp(pointer);

                                ALvalueExp leftSideExp = (ALvalueExp) Util.MakeClone(args[i], data);
                                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSideExp.GetLvalue(), pointerExp);
                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                              new AExpStm(new TSemicolon(";"), assignment));


                                AFieldDecl paramArr = Util.IsIntPointer(formal, formal.GetType(), data)
                                               ? intParamArray
                                               : stringParamArray;

                                data.FieldLinks[paramArrayRef] = paramArr;
                                data.LvalueTypes[paramArrayRef] =
                                    data.ExpTypes[paramArrayRefExp] = paramArr.GetType();
                                data.LvalueTypes[rightSide] =
                                    data.ExpTypes[rightSideExp] =
                                    data.ExpTypes[assignment] =
                                    new APointerType(new TStar("*"), Util.MakeClone(data.ExpTypes[args[i]], data));
                                data.LvalueTypes[pointer] =
                                    data.ExpTypes[pointerExp] = data.ExpTypes[args[i]];
                                data.ExpTypes[rightSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);

                                //* delete paramArray[x];//If ref or out
                                paramArrayRef = new AFieldLvalue(new TIdentifier("paramArray"));
                                paramArrayRefExp = new ALvalueExp(paramArrayRef);
                                rightSide = new AArrayLvalue(new TLBracket("["), paramArrayRefExp,
                                                             new AIntConstExp(new TIntegerLiteral((i + 1).ToString())));
                                rightSideExp = new ALvalueExp(rightSide);
                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                              new ADeleteStm(new TDelete("delete"), rightSideExp));



                                data.FieldLinks[paramArrayRef] = paramArr;
                                data.LvalueTypes[paramArrayRef] =
                                    data.ExpTypes[paramArrayRefExp] = paramArr.GetType();
                                data.LvalueTypes[rightSide] =
                                    data.ExpTypes[rightSideExp] =
                                    new APointerType(new TStar("*"), Util.MakeClone(data.ExpTypes[args[i]], data));
                                data.ExpTypes[rightSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                            }
                        }
                        if (!returnerIsUsed)
                            pBlock.RemoveChild(pStm);
                    }
                }
            }
        }
    }
}
