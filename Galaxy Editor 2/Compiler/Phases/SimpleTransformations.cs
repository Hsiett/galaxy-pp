using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class SimpleTransformations : DepthFirstAdapter 
    {
        public static void Parse(Node ast)
        {
            ast.Apply(new SimpleTransformations());
        }

        public override void CaseAOperatorDecl(AOperatorDecl node)
        {

            AMethodDecl replacer = new AMethodDecl(node.GetVisibilityModifier(), null, node.GetStatic(), null, null,
                                                   null, node.GetReturnType(),
                                                   new TIdentifier(""), new ArrayList(),
                                                   node.GetBlock());
            while (node.GetFormals().Count > 0)
                replacer.GetFormals().Add(node.GetFormals()[0]);

            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }

        public override void OutAEnumDecl(AEnumDecl node)
        {
            AStructDecl replacer = new AStructDecl(node.GetVisibilityModifier(), null, null, null, node.GetEndToken(),
                                                   node.GetName(), new ArrayList(), null, new ArrayList());
            
            
            
            int intVal = 0;
            //int min = int.MaxValue;
            //int max = int.MinValue;
            //List<TIdentifier> types = new List<TIdentifier>();
            foreach (AAEnumLocal value in node.GetValues())
            {
                AIntConstExp intConst;
                if (value.GetValue() != null)
                {
                    intConst = (AIntConstExp) value.GetValue();
                    intVal = int.Parse(intConst.GetIntegerLiteral().Text) + 1;
                }
                else
                {
                    intConst = new AIntConstExp(new TIntegerLiteral(intVal.ToString(), value.GetName().Line, value.GetName().Pos));
                    intVal++;
                }
            //    min = Math.Min(intVal - 1, min);
            //    max = Math.Max(intVal - 1, max);
                TIdentifier typeIdentifier = new TIdentifier(replacer.GetName().Text, value.GetName().Line, value.GetName().Pos);
               // types.Add(typeIdentifier);
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(),
                                                        new TStatic("static", value.GetName().Line, value.GetName().Pos),
                                                        null, null,
                                                        new TConst("const", value.GetName().Line, value.GetName().Pos),
                                                        new ANamedType(typeIdentifier, null), value.GetName(), intConst);
                replacer.GetLocals().Add(localDecl);
            }
         /*   if (min < 0 || max > 255)
                foreach (TIdentifier identifier in types)
                {
                    identifier.Text = "int";
                }*/
            node.ReplaceBy(replacer);
            
            replacer.Apply(this);
            replacer.GetName().Text = "enum " + replacer.GetName().Text;
        }

        List<ADynamicArrayType> pointeredArrays = new List<ADynamicArrayType>();
        public override void OutADynamicArrayType(ADynamicArrayType node)
        {
            if (pointeredArrays.Contains(node))
                return;
            pointeredArrays.Add(node);
            APointerType replacer = new APointerType(new TStar("*"), null);
            node.ReplaceBy(replacer);
            replacer.SetType(node);
        }

        private AASourceFile currentSourceFile;

        public override void CaseAASourceFile(AASourceFile node)
        {
            currentSourceFile = node;
            base.CaseAASourceFile(node);
        }

        public override void CaseAThisArrayPropertyDecl(AThisArrayPropertyDecl node)
        {
            APropertyDecl replacer = new APropertyDecl(node.GetVisibilityModifier(), null, node.GetType(),
                                                       new TIdentifier("array property", node.GetToken().Line, node.GetToken().Pos),
                                                       node.GetGetter(), node.GetSetter());
            List<AALocalDecl> locals = new List<AALocalDecl>();
            if (replacer.GetGetter() != null)
            {
                AABlock block = (AABlock)replacer.GetGetter();
                AALocalDecl local = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                    (PType)node.GetArgType().Clone(),
                                                    (TIdentifier)node.GetArgName().Clone(), null);
                block.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), local));
                locals.Add(local);
            }
            if (replacer.GetSetter() != null)
            {
                AABlock block = (AABlock)replacer.GetSetter();
                AALocalDecl local = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                    (PType)node.GetArgType().Clone(),
                                                    (TIdentifier)node.GetArgName().Clone(), null);

                block.GetStatements().Insert(0, new ALocalDeclStm(new TSemicolon(";"), local));
                locals.Add(local);
            }
            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }


        public override void CaseATempNamespaceDecl(ATempNamespaceDecl node)
        {
            ANamespaceDecl replacer = null;
            Node parent = node.Parent();
            IList declList;
            if (parent is ANamespaceDecl)
                declList = ((ANamespaceDecl)parent).GetDecl();
            else
                declList = ((AASourceFile)parent).GetDecl();
            parent.RemoveChild(node);

            List<TIdentifier> identifiers = new List<TIdentifier>();
            foreach (TIdentifier identifier in node.GetName())
            {
                identifiers.Add(identifier);
            }

            foreach (TIdentifier identifier in identifiers)
            {
                TRBrace endToken = null;
                if (node.GetEndToken() != null)
                    endToken = new TRBrace("}", node.GetEndToken().Line, node.GetEndToken().Pos);
                ANamespaceDecl ns = new ANamespaceDecl(new TNamespace("namespace", node.GetToken().Line, node.GetToken().Pos), identifier, new ArrayList(), endToken);
                if (replacer == null)
                    replacer = ns;
                declList.Add(ns);
                declList = ns.GetDecl();
            }
            while (node.GetDecl().Count > 0)
            {
                declList.Add(node.GetDecl()[0]);
            }
            replacer.Apply(this);
        }


        public override void OutAAssignmentExp(AAssignmentExp node)
        {
            if (Util.GetAncestor<PStm>(node) == null)
            {
                node.ReplaceBy(node.GetExp());
                return;
            }
            base.OutAAssignmentExp(node);
        }
        

        

        public override void CaseASwitchCaseStm(ASwitchCaseStm node)
        {
            AABlock block = (AABlock) node.GetBlock();
            Token token;
            if (node.GetType() is ACaseSwitchCaseType)
                token = ((ACaseSwitchCaseType) node.GetType()).GetToken();
            else
                token = ((ADefaultSwitchCaseType)node.GetType()).GetToken();
            block.SetToken(new TRBrace("{", token.Line, token.Pos));
            base.CaseASwitchCaseStm(node);
        }

        public override void CaseAHexConstExp(AHexConstExp node)
        {
            int i = Convert.ToInt32(node.GetHexLiteral().Text.Substring(2), 16);
            AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString(), node.GetHexLiteral().Line, node.GetHexLiteral().Pos));
            node.ReplaceBy(intConst);
            intConst.Apply(this);
        }

        public override void CaseAOctalConstExp(AOctalConstExp node)
        {
            int i = Convert.ToInt32(node.GetOctalLiteral().Text.Substring(1), 8);
            AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral(i.ToString(), node.GetOctalLiteral().Line, node.GetOctalLiteral().Pos));
            node.ReplaceBy(intConst);
            intConst.Apply(this);
        }



       

        

        public override void OutASAssignmentExp(ASAssignmentExp node)
        {
            
            AAssignmentExp replacer = null;
            PAssignop assignop = node.GetAssignop();
            Token token = null;
            PBinop binop = null;
            if (assignop is AAddAssignop)
            {
                token = ((AAddAssignop)assignop).GetToken();
                binop = new APlusBinop(new TPlus("+", token.Line, token.Pos));
            }
            else if (assignop is ASubAssignop)
            {
                token = ((ASubAssignop)assignop).GetToken();
                binop = new AMinusBinop(new TMinus("-", token.Line, token.Pos));
            }
            else if (assignop is AMulAssignop)
            {
                token = ((AMulAssignop)assignop).GetToken();
                binop = new ATimesBinop(new TStar("*", token.Line, token.Pos));
            }
            else if (assignop is ADivAssignop)
            {
                token = ((ADivAssignop)assignop).GetToken();
                binop = new ADivideBinop(new TDiv("/", token.Line, token.Pos));
            }
            else if (assignop is AModAssignop)
            {
                token = ((AModAssignop)assignop).GetToken();
                binop = new AModuloBinop(new TMod("%", token.Line, token.Pos));
            }
            else// if (assignop is AAssignAssignop)
            {
                token = ((AAssignAssignop)assignop).GetToken();
            }
            PExp rightSide;
            if (binop != null)
                rightSide = new ABinopExp(new ALvalueExp((PLvalue)node.GetLvalue().Clone()),
                                                    binop,
                                                    (PExp)node.GetExp().Clone());
            else
                rightSide = (PExp)node.GetExp().Clone();
            replacer = new AAssignmentExp(new TAssign("=", token.Line, token.Pos), (PLvalue)node.GetLvalue().Clone(), rightSide);
            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }



        public override void OutATriggerDecl(ATriggerDecl node)
        {

            //If no actions, insert it
            if (node.GetActions() == null)
            {
                node.SetActions(new AABlock(new ArrayList(), new TRBrace("}", -1, -1)));
                node.SetActionsToken(new TActions("actions", -1, -1));
            }

            //If return in events is missing, insert it.
            if (node.GetEvents() != null)
            {
                AABlock block = (AABlock)node.GetEvents();
                bool insertReturn = false;
                while (true)
                {
                    if (block.GetStatements().Count == 0)
                    {
                        insertReturn = true;
                        break;
                    }
                    PStm lastStm = (PStm)block.GetStatements()[block.GetStatements().Count - 1];
                    if (lastStm is AVoidReturnStm)
                        break;
                    if (lastStm is ABlockStm)
                    {
                        block = (AABlock)((ABlockStm)block.GetStatements()[block.GetStatements().Count - 1]).GetBlock();
                        continue;
                    }
                    insertReturn = true;
                    break;
                }
                if (insertReturn)
                {
                    block.GetStatements().Add(new AVoidReturnStm(new TReturn("return", block.GetToken().Line, block.GetToken().Pos)));
                }
            }
            //Also for actions
            //if (node.GetActions() != null)
            {
                AABlock block = (AABlock)node.GetActions();
                bool insertReturn = false;
                while (true)
                {
                    if (block.GetStatements().Count == 0)
                    {
                        insertReturn = true;
                        break;
                    }
                    PStm lastStm = (PStm)block.GetStatements()[block.GetStatements().Count - 1];
                    if (lastStm is AVoidReturnStm)
                        break;
                    if (lastStm is ABlockStm)
                    {
                        block = (AABlock)((ABlockStm)block.GetStatements()[block.GetStatements().Count - 1]).GetBlock();
                        continue;
                    }
                    insertReturn = true;
                    break;
                }
                if (insertReturn)
                {
                    block.GetStatements().Add(new AVoidReturnStm(new TReturn("return", block.GetToken().Line, block.GetToken().Pos)));
                }
            }

        }

        public override void OutAConstructorDecl(AConstructorDecl node)
        {
            //If void return is missing, insert it.
            AABlock block = (AABlock)node.GetBlock();
            bool insertReturn = false;
            while (true)
            {
                if (block.GetStatements().Count == 0)
                {
                    insertReturn = true;
                    break;
                }
                PStm lastStm = (PStm)block.GetStatements()[block.GetStatements().Count - 1];
                if (lastStm is AVoidReturnStm)
                    break;
                if (lastStm is ABlockStm)
                {
                    block = (AABlock)((ABlockStm)block.GetStatements()[block.GetStatements().Count - 1]).GetBlock();
                    continue;
                }
                insertReturn = true;
                break;
            }
            if (insertReturn)
            {
                block.GetStatements().Add(new AVoidReturnStm(new TReturn("return", block.GetToken().Line, block.GetToken().Pos)));
            }


            base.OutAConstructorDecl(node);
        }


        public override void OutAMethodDecl(AMethodDecl node)
        {
            //If void return is missing, insert it.
            if (node.GetReturnType() is AVoidType && node.GetBlock() != null)
            {
                AABlock block = (AABlock)node.GetBlock();
                bool insertReturn = false;
                while (true)
                {
                    if (block.GetStatements().Count == 0)
                    {
                        insertReturn = true;
                        break;
                    }
                    PStm lastStm = (PStm)block.GetStatements()[block.GetStatements().Count - 1];
                    if (lastStm is AVoidReturnStm)
                        break;
                    if (lastStm is ABlockStm)
                    {
                        block = (AABlock)((ABlockStm)block.GetStatements()[block.GetStatements().Count - 1]).GetBlock();
                        continue;
                    }
                    insertReturn = true;
                    break;
                }
                if (insertReturn)
                {
                    block.GetStatements().Add(new AVoidReturnStm(new TReturn("return", block.GetToken().Line, block.GetToken().Pos)));
                }
            }
            base.OutAMethodDecl(node);
        }

        public override void CaseAParenExp(AParenExp node)
        {
            PExp replacer = node.GetExp();
            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }

        public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
        {
            if (node.GetDotType() is AArrowDotType)
            {
                TArrow arrow = ((AArrowDotType)node.GetDotType()).GetToken();
                node.SetReceiver(new ALvalueExp(new APointerLvalue(new TStar("*", arrow.Line, arrow.Pos), node.GetReceiver())));
                node.SetDotType(new ADotDotType(new TDot(".", arrow.Line, arrow.Pos)));
            }
            base.CaseANonstaticInvokeExp(node);
        }

        public override void OutAStructDecl(AStructDecl node)
        {
            //Insert parameterless constructor
            if (
                !node.GetLocals().OfType<ADeclLocalDecl>().Select(localDecl => localDecl.GetDecl()).OfType
                     <AConstructorDecl>().Any(constructor => constructor.GetFormals().Count == 0))
            {
                node.GetLocals().Add(
                    new ADeclLocalDecl(new AConstructorDecl(new APublicVisibilityModifier(),
                                                            new TIdentifier(node.GetName().Text), new ArrayList(),
                                                            new ArrayList(),
                                                            new AABlock(new ArrayList(), new TRBrace("}")))));
            }


        }


        public override void CaseAStructLvalue(AStructLvalue node)
        {
            //a->b => (*a).b
            if (node.GetDotType() is AArrowDotType)
            {
                TArrow arrow = ((AArrowDotType) node.GetDotType()).GetToken();
                node.SetReceiver(new ALvalueExp(new APointerLvalue(new TStar("*", arrow.Line, arrow.Pos), node.GetReceiver())));
                node.SetDotType(new ADotDotType(new TDot(".", arrow.Line, arrow.Pos)));
            }
            base.CaseAStructLvalue(node);
        }

        public override void OutAPointerMultiLvalue(APointerMultiLvalue node)
        {
            ALvalueExp lvalueExp;
            APointerLvalue pointerLvalue = new APointerLvalue((TStar) node.GetTokens()[0], node.GetBase());

            while (node.GetTokens().Count > 0)
            {
                lvalueExp = new ALvalueExp(pointerLvalue);
                pointerLvalue = new APointerLvalue((TStar) node.GetTokens()[0], lvalueExp);
            }

            node.ReplaceBy(pointerLvalue);
            pointerLvalue.Apply(this);
        }

        public override void OutAShadySAssignmentExp(AShadySAssignmentExp node)
        {
            if (node.GetLocalDeclRight().Count == 1 &&
                ((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetName() == null)
            {
                //Assignment expression

                //an assignment can't be const);
                //An assignment must have a right side
                if (((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetInit() == null ||
                    ((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetAssignop() == null)
                {
                    node.Parent().RemoveChild(node);
                    return;
                }
                ASAssignmentExp exp = new ASAssignmentExp(
                    ((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetAssignop(), node.GetLvalue(),
                    ((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetInit());
                node.ReplaceBy(exp);
                exp.Apply(this);
            }
            else
            {


                //Local decl
                AMultiLocalDecl localDecl = new AMultiLocalDecl(node.GetConst(), LvalueToType(node.GetLvalue(), node.GetPostPointers(), node.GetGenericToken(), node.GetGenericTypes()),
                                                                new ArrayList());
                while (node.GetLocalDeclRight().Count > 0)
                {
                    localDecl.GetLocalDeclRight().Add(node.GetLocalDeclRight()[0]);
                }
                                                    
                AExpStm expStm = Util.GetAncestor<AExpStm>(node);
                ALocalDeclStm localDeclStm = new ALocalDeclStm(expStm.GetToken(), localDecl);
                expStm.ReplaceBy(localDeclStm);
                localDeclStm.Apply(this);
            }
        }

        private PType LvalueToType(PLvalue lvalue, IList dynamicOpList, TLt genericToken, IList genericTypes)
        {
            PType type = LvalueToType(lvalue);
            if (genericToken != null)
            {
                type = new AGenericType(genericToken, type, new ArrayList());
                while (genericTypes.Count > 0)
                {
                    ((AGenericType) type).GetGenericTypes().Add(genericTypes[0]);
                }
            }
            foreach (PShadyDynamicOps op in dynamicOpList)
            {
                if (op is APointerShadyDynamicOps)
                {
                    APointerShadyDynamicOps aop = (APointerShadyDynamicOps) op;
                    type = new APointerType(aop.GetToken(), type);
                }
                else if (op is AArrayShadyDynamicOps)
                {
                    AArrayShadyDynamicOps aop = (AArrayShadyDynamicOps)op;
                    if (aop.GetExp() == null)
                        type = new ADynamicArrayType(aop.GetToken(), type);
                    else
                        type = new AArrayTempType(aop.GetToken(), type, aop.GetExp(), null);
                }
            }
            return type;
        }

        public override void CaseATempCastExp(ATempCastExp node)
        {
            //The cast type must be a single identifier
            if (node.GetType() is ALvalueExp)
            {
                ALvalueExp lvalueExp = (ALvalueExp)node.GetType();
                if (lvalueExp.GetLvalue() is AAmbiguousNameLvalue)
                {
                    AAmbiguousNameLvalue ambiguousLvalue = (AAmbiguousNameLvalue)lvalueExp.GetLvalue();
                    if (ambiguousLvalue.GetAmbiguous() is AAName)
                    {
                        AAName simpleName = (AAName)ambiguousLvalue.GetAmbiguous();
                        if (simpleName.GetIdentifier().Count == 1)
                        {
                            ACastExp castExp = new ACastExp(node.GetToken(), new ANamedType(simpleName), node.GetExp());
                            node.ReplaceBy(castExp);
                            castExp.Apply(this);
                            return;
                        }

                    }
                }
            }
            PExp exp = node.GetExp();
            node.ReplaceBy(exp);
            exp.Apply(this);
        }

        

        public override void CaseAMultiLocalDecl(AMultiLocalDecl node)
        {
            PStm pStm = Util.GetAncestor<PStm>(node);
            if (!(pStm.Parent() is AABlock))
            {
                return;
            }
            AABlock pBlock = (AABlock) pStm.Parent();
            List<AALocalDecl> newDecls = new List<AALocalDecl>();
            foreach (AALocalDeclRight right in node.GetLocalDeclRight())
            {
                if (right.GetName() == null)
                {
                    continue;
                }
                if ((right.GetAssignop() == null) != (right.GetInit() == null))
                {
                    continue;
                }
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null,
                                                        node.GetConst() != null
                                                            ? (TConst) node.GetConst().Clone()
                                                            : null, 
                                                        (PType) node.GetType().Clone(), 
                                                        right.GetName(),
                                                        right.GetInit());
                newDecls.Add(localDecl);
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), localDecl));
            }
            pBlock.RemoveChild(pStm);
            foreach (AALocalDecl localDecl in newDecls)
            {
                localDecl.Apply(this);
            }
        }

        public override void CaseAForStm(AForStm node)
        {
            //Replace with while
            node.GetBody().Apply(new ReworkForContinues());

            AABlock innerBlock = new AABlock();
            innerBlock.SetToken(new TRBrace("{", node.GetToken().Line, node.GetToken().Pos));
            innerBlock.GetStatements().Add(node.GetBody());
            innerBlock.GetStatements().Add(node.GetUpdate());
            ABlockStm innerBlockStm = new ABlockStm(new TLBrace(";"), innerBlock);
            AWhileStm whileStm = new AWhileStm(node.GetToken(), node.GetCond(), innerBlockStm);
            AABlock block = new AABlock();
            block.SetToken(new TRBrace("{", whileStm.GetToken().Line, whileStm.GetToken().Pos));
            block.GetStatements().Add(node.GetInit());
            block.GetStatements().Add(whileStm);
            ABlockStm blockStm = new ABlockStm(null, block);
            node.ReplaceBy(blockStm);
            blockStm.Apply(this);
        }

        private class ReworkForContinues : DepthFirstAdapter
        {
            public override void CaseAForStm(AForStm node)
            {
                //Ignore it.
            }

            public override void CaseAWhileStm(AWhileStm node)
            {
                //Ignore it.
            }

            public override void CaseAContinueStm(AContinueStm node)
            {
                AForStm forStm = Util.GetAncestor<AForStm>(node);
                AABlock pBlock; 
                if (!(node.Parent() is AABlock))
                {
                    pBlock = new AABlock(new ArrayList(), new TRBrace("}", node.GetToken().Line, node.GetToken().Pos));
                    ABlockStm blockStm = new ABlockStm(new TLBrace("{", node.GetToken().Line, node.GetToken().Pos), pBlock);
                    node.ReplaceBy(blockStm);
                    pBlock.GetStatements().Add(node);
                }
                pBlock = (AABlock) node.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), forStm.GetUpdate().Clone());
            }
        }





        private PType LvalueToType(PLvalue lvalue)
        {
            if (lvalue is AStructLvalue)
            {
                /*AStructLvalue structLvalue = (AStructLvalue) lvalue;
                TIdentifier typeName = structLvalue.GetName();
                if (structLvalue.GetReceiver() is ALvalueExp && ((ALvalueExp)structLvalue.GetReceiver()).GetLvalue() is AAmbiguousNameLvalue)
                {
                    AAmbiguousNameLvalue ambiguousNameLvalue = (AAmbiguousNameLvalue) ((ALvalueExp) structLvalue.GetReceiver()).GetLvalue();
                    return new ANamedType(typeName, ((ASimpleName)ambiguousNameLvalue.GetAmbiguous()).GetIdentifier());
                }
                else*/
                {
                    //errors.Add(new ErrorCollection.Error(((AStructLvalue)lvalue).GetName(), currentSourceFile, "Invalid type name. Must be \"<namespace>.<struct name>\"."));
                    throw new ParserException(null, null);
                }
            }
            if (lvalue is AArrayLvalue)
            {
                AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                if (!(aLvalue.GetBase() is ALvalueExp))
                {
                    //errors.Add(new ErrorCollection.Error(GetToken(lvalue), currentSourceFile, "Whatever that is, it's not allowed in a type"));
                    throw new ParserException(null, null);
                }
                PLvalue newLvalue = ((ALvalueExp)aLvalue.GetBase()).GetLvalue();
                return new AArrayTempType(aLvalue.GetToken(), LvalueToType(newLvalue), aLvalue.GetIndex(), null);
            }
            //it must be an AAmbiguousNameLvalue then
            return new ANamedType(((AAmbiguousNameLvalue)lvalue).GetAmbiguous());
        }

        private PLvalue DetypeArray(PType type)
        {
            if (type is AArrayTempType)
            {
                AArrayTempType atype = (AArrayTempType)type;
                return new AArrayLvalue(atype.GetToken(), new ALvalueExp(DetypeArray(atype.GetType())),
                                        atype.GetDimention());
            }
            if (type is ANamedType)
            {
                ANamedType atype = (ANamedType)type;
                return new AAmbiguousNameLvalue(atype.GetName());
            }
            if (type is AVoidType)
            {
                AVoidType atype = (AVoidType)type;
                throw new Exception(Util.TokenToStringPos(atype.GetToken()) + " (Weeder)Unexpected type: void. It should not be possible for this error to occur");
            }
            /*if (type is AArrayType)
            {
                AArrayType atype = (AArrayType)type;
                throw new Exception(Util.TokenToStringPos(atype.GetToken()) + " (Weeder)Unexpected type: ArrayType. It should not be possible for this error to occur");
            }*/
            throw new Exception("(Weeder)Unexpected type: none. It should not be possible for this error to occur");
        }

       
    }
}
