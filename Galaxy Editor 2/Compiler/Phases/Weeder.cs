using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class Weeder : DepthFirstAdapter 
    {
        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data)
        {
            ast.Apply(new Weeder(errors, data));
        }

        private ErrorCollection errors;
        private SharedData data;


        public Weeder(ErrorCollection errors, SharedData data)
        {
            this.errors = errors;
            this.data = data;
        }

        private AASourceFile currentSourceFile;

        public override void CaseAASourceFile(AASourceFile node)
        {
            currentSourceFile = node;
            base.CaseAASourceFile(node);
        }

        public override void OutAIfExp(AIfExp node)
        {
            if (!Util.HasAncestor<PStm>(node))
                errors.Add(new ErrorCollection.Error(node.GetToken(), "The ? operator can only be used in statements."));
            base.OutAIfExp(node);
        }

        public override void OutAEnumDecl(AEnumDecl node)
        {
            AStructDecl replacer = new AStructDecl(node.GetVisibilityModifier(), null, null, null, node.GetEndToken(),
                                                   node.GetName(), new ArrayList(), null, new ArrayList());
            
            TIdentifier typeIdentifier = new TIdentifier("byte");
            ASwitchStm switchStm = new ASwitchStm(new TSwitch("switch"),
                                                  new ALvalueExp(
                                                      new AAmbiguousNameLvalue(new AAName(new ArrayList() {new TIdentifier("enum")}))),
                                                  new ArrayList());
                ;
            AMethodDecl toStringMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                         new ANamedType(new TIdentifier("string"), null),
                                                         new TIdentifier("toString"),
                                                         new ArrayList()
                                                             {
                                                                 new AALocalDecl(new APublicVisibilityModifier(), null,
                                                                                 null, null, null,
                                                                                 new ANamedType(typeIdentifier, null),
                                                                                 new TIdentifier("enum"), null)
                                                             },
                                                         new AABlock(
                                                             new ArrayList()
                                                                 {
                                                                     switchStm,
                                                                     new AValueReturnStm(new TReturn("return"), new ANullExp())
                                                                 }, new TRBrace("}")));
            replacer.GetLocals().Add(new ADeclLocalDecl(toStringMethod));
            
            int intVal = 0;
            int min = int.MaxValue;
            int max = int.MinValue;
            List<TIdentifier> types = new List<TIdentifier>(){typeIdentifier};
            Dictionary<int, List<AALocalDecl>> usedValues = new Dictionary<int, List<AALocalDecl>>();
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
                min = Math.Min(intVal - 1, min);
                max = Math.Max(intVal - 1, max);
                typeIdentifier = new TIdentifier("byte", value.GetName().Line, value.GetName().Pos);
                types.Add(typeIdentifier);
                switchStm.GetCases().Add(
                    new ASwitchCaseStm(new ACaseSwitchCaseType(new TCase("case"), (PExp) intConst.Clone()),
                                       new AABlock(
                                           new ArrayList()
                                               {
                                                   new AValueReturnStm(new TReturn("return"),
                                                                       new AStringConstExp(
                                                                           new TStringLiteral("\"" +
                                                                                              value.GetName().Text +
                                                                                              "\"")))
                                               }, new TRBrace("}"))));
                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(),
                                                        new TStatic("static", value.GetName().Line, value.GetName().Pos),
                                                        null, null,
                                                        new TConst("const", value.GetName().Line, value.GetName().Pos),
                                                        new ANamedType(typeIdentifier, null), value.GetName(), intConst);
                replacer.GetLocals().Add(localDecl);
                if (!usedValues.ContainsKey(intVal - 1))
                    usedValues[intVal - 1] = new List<AALocalDecl>();
                usedValues[intVal - 1].Add(localDecl);
            }
            if (min < 0 || max > 255)
                foreach (TIdentifier identifier in types)
                {
                    identifier.Text = "int";
                }
            node.ReplaceBy(replacer);
            foreach (KeyValuePair<int, List<AALocalDecl>> pair in usedValues)
            {
                if (pair.Value.Count <= 1)
                    continue;
                int value = pair.Key;
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                foreach (AALocalDecl decl in pair.Value)
                {
                    subErrors.Add(new ErrorCollection.Error(decl.GetName(), "Enum member"));
                }
                errors.Add(new ErrorCollection.Error(replacer.GetName(), "Found multiple enum members with the value " + value + ".", false, subErrors.ToArray()));
            }
            replacer.Apply(this);
            data.Enums.Add(replacer, min < 0 || max > 255);
        }

        public override void OutATypedefDecl(ATypedefDecl node)
        {
            if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Typedefs can't be marked as protected."));
        }

        public override void CaseAOperatorDecl(AOperatorDecl node)
        {
            if (node.GetFormals().Count != 2)
                errors.Add(new ErrorCollection.Error(node.GetToken(), "Custom operators must have exactly two parameters."));
            Token token = null;
            if (node.GetOperator() is APlusBinop)
                token = ((APlusBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AMinusBinop)
                token = ((AMinusBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ADivideBinop)
                token = ((ADivideBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ATimesBinop)
                token = ((ATimesBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AModuloBinop)
                token = ((AModuloBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AEqBinop)
                token = ((AEqBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ANeBinop)
                token = ((ANeBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ALtBinop)
                token = ((ALtBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ALeBinop)
                token = ((ALeBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AGtBinop)
                token = ((AGtBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AGeBinop)
                token = ((AGeBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AGtBinop)
                token = ((AGtBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AAndBinop)
                token = ((AAndBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AOrBinop)
                token = ((AOrBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is AXorBinop)
                token = ((AXorBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ALBitShiftBinop)
                token = ((ALBitShiftBinop)node.GetOperator()).GetToken();
            else if (node.GetOperator() is ARBitShiftBinop)
                token = ((ARBitShiftBinop)node.GetOperator()).GetToken();

            AMethodDecl replacer = new AMethodDecl(node.GetVisibilityModifier(), null, node.GetStatic(), null, null,
                                                   null, node.GetReturnType(),
                                                   new TIdentifier(token.Text, token.Line, token.Pos), new ArrayList(),
                                                   node.GetBlock());
            while (node.GetFormals().Count > 0)
                replacer.GetFormals().Add(node.GetFormals()[0]);

            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }

        public override void OutAFieldDecl(AFieldDecl node)
        {
            bool isAConstStringVar = data.ObfuscationFields.Contains(node);/* data.ObfuscatedStrings.Values.Any(stringField => stringField == node) ||
                                     data.UnobfuscatedStrings.Values.Any(stringField => stringField == node);*/
            if (!isAConstStringVar && node.GetConst() != null && node.GetInit() == null)
                errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Constant fields must have an initializer", false));
            //If it's protected, it must be in a struct
            if (!Util.HasAncestor<AStructDecl>(node))
            {
                if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Only fields inside structs or classes can be marked as protected."));
            }
            base.OutAFieldDecl(node);
        }

        public override void CaseATempNamespaceDecl(ATempNamespaceDecl node)
        {
            ANamespaceDecl visitMe = null;
            Node parent = node.Parent();
            IList declList;
            if (parent is ANamespaceDecl)
                declList = ((ANamespaceDecl) parent).GetDecl();
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
                if (visitMe == null)
                    visitMe = ns;
                declList.Add(ns);
                declList = ns.GetDecl();
            }
            while (node.GetDecl().Count > 0)
            {
                declList.Add(node.GetDecl()[0]);
            }
            visitMe.Apply(this);
        }

        public override void OutAUnopExp(AUnopExp node)
        {
            if (node.GetExp() is AIntConstExp && node.GetUnop() is ANegateUnop)
            {
                AIntConstExp intConst = (AIntConstExp) node.GetExp();
                intConst.GetIntegerLiteral().Text = "-" + intConst.GetIntegerLiteral().Text;
                node.ReplaceBy(intConst);
                return;
            }
            base.OutAUnopExp(node);
        }

        public override void CaseAThisArrayPropertyDecl(AThisArrayPropertyDecl node)
        {
            if (!Util.HasAncestor<AStructDecl>(node))
            {
                /*if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                    errors.Add(new ErrorCollection.Error(node.GetToken(),
                                                         "Only fields inside structs or classes can be marked as protected."));*/
                if (!Util.HasAncestor<AEnrichmentDecl>(node))
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "This type of property declaration must be in a struct, class or enrichment."));
            }

            APropertyDecl replacer = new APropertyDecl(node.GetVisibilityModifier(), null, node.GetType(),
                                                       new TIdentifier("", node.GetToken().Line, node.GetToken().Pos),
                                                       node.GetGetter(), node.GetSetter());
            List<AALocalDecl> locals = new List<AALocalDecl>();
            if (replacer.GetGetter() != null)
            {
                AABlock block = (AABlock) replacer.GetGetter();
                AALocalDecl local = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                    (PType) node.GetArgType().Clone(),
                                                    (TIdentifier) node.GetArgName().Clone(), null);
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
            data.ArrayPropertyLocals[replacer] = locals.ToArray();
            node.ReplaceBy(replacer);
            replacer.Apply(this);
        }

        public override void OutAPropertyDecl(APropertyDecl node)
        {
            //If it's protected, it must be in a struct
            if (!Util.HasAncestor<AStructDecl>(node))
            {
                if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Only fields inside structs or classes can be marked as protected."));
            }
            base.OutAPropertyDecl(node);
        }

        public override void OutAAssignmentExp(AAssignmentExp node)
        {
            if (Util.GetAncestor<PStm>(node) == null)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Assignments have to be inside methods"));
                node.ReplaceBy(node.GetExp());
                return;
            }
            base.OutAAssignmentExp(node);
        }

        public override void CaseAValueLvalue(AValueLvalue node)
        {
            //Must be in a property setter
            AABlock lastBlockParent = Util.GetLastAncestor<AABlock>(node);
            APropertyDecl property = Util.GetAncestor<APropertyDecl>(node);
            if (property == null || property.GetSetter() != lastBlockParent)
            {
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "The keyword value can only be used in property setters."));
            }
            base.CaseAValueLvalue(node);
        }

        

        //This should be done later, after type linking since we might make new variables for i++, which shouldn't clash with user definitions
        /*public override void OutAIncDecExp(AIncDecExp node)
        {
            //Replace with assignment
            //<exp>++ => <exp> + 1
            //(... foo = <exp> ...)++ => (... foo = <exp> ...) = (... foo ...) + 1
            //(... foo++ ...)++ => (... foo++ ...) = (... foo ...) + 1
            
            PLvalue clone = (PLvalue) node.GetLvalue().Clone();
            clone.Apply(new AssignFixup());
            PBinop binop;
            Token token;
            if (node.GetIncDecOp() is APostIncIncDecOp)
            {
                APostIncIncDecOp op = (APostIncIncDecOp) node.GetIncDecOp();
                token = op.GetToken();
                binop = new APlusBinop(new TPlus("+", op.GetToken().Line, op.GetToken().Pos));
            }
            else
            {
                APostDecIncDecOp op = (APostDecIncDecOp)node.GetIncDecOp();
                token = op.GetToken();
                binop = new AMinusBinop(new TMinus("-", op.GetToken().Line, op.GetToken().Pos));
            }
            ABinopExp addExp = new ABinopExp(new ALvalueExp(clone), binop, new AIntConstExp(new TIntegerLiteral("1", token.Line, token.Pos)));
            AAssignmentExp exp = new AAssignmentExp(new TAssign("=", token.Line, token.Pos), node.GetLvalue(), addExp);
            node.ReplaceBy(exp);
            exp.Apply(this);
        }

        private class AssignFixup : DepthFirstAdapter
        {
            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                ALvalueExp replacer = new ALvalueExp(node.GetLvalue());
                node.ReplaceBy(replacer);
            }
        }*/

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

        public override void OutABreakStm(ABreakStm node)
        {
            PStm stm = Util.GetAncestor<AWhileStm>(node) ?? (PStm)Util.GetAncestor<ASwitchCaseStm>(node);
            if (stm == null)
                errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Break statements must be inside for, while or switch statements", false));
            base.OutABreakStm(node);
        }

        public override void OutAContinueStm(AContinueStm node)
        {
            AWhileStm whileStm = Util.GetAncestor<AWhileStm>(node);
            if (whileStm == null)
                errors.Add(new ErrorCollection.Error(whileStm.GetToken(), currentSourceFile, "Continue statements must be inside for or while statemetns", false));
            base.OutAContinueStm(node);
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



        /*public override void CaseAShadyLocalDecl(AShadyLocalDecl node)
        {
            bool hasIdentifier = node. node.GetName() != null;
            if (hasIdentifier)
            {//Replace with a non shady local decl
                if (node.GetInit() != null)
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Array declarations cannot be initialized", false));

                AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, node.GetConst(), node.GetType(), node.GetName(), null);
                node.ReplaceBy(localDecl);
                localDecl.Apply(this);
            }
            else
            {
                //If it is not a declaration, it is an assignment exp, so we must have a right side
                if (node.GetInit() == null)
                {
                    errors.Add(new ErrorCollection.Error(GetToken(node.GetType()), currentSourceFile, "Expected assignment", false));
                    node.Parent().RemoveChild(node);
                    return;
                }
                //If it says const first, we got an error
                if (node.GetConst() != null)
                    errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Unexpected const", false));

                ALocalDeclStm parent = (ALocalDeclStm)node.Parent();
                PLvalue lvalue = DetypeArray(node.GetType());
                ASAssignmentExp exp = new ASAssignmentExp(node.GetAssignop(), lvalue, node.GetInit());
                AExpStm stm = new AExpStm(parent.GetToken(), exp);
                parent.ReplaceBy(stm);
                stm.Apply(this);
            }
        }*/

        public override void OutAALocalDecl(AALocalDecl node)
        {
            
            if (Util.GetAncestor<AABlock>(node) != null)
            {//We are a local defined inside a method
                if (node.GetConst() != null && node.GetInit() == null)
                    errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Const variables must have an initializer", false));
            }
            else if (Util.HasAncestor<AMethodDecl>(node) || Util.HasAncestor<AConstructorDecl>(node))
            {//We are in a parameter
                if (node.GetConst() != null)
                    errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Parameters can not be const", false));
            }
            else
            {
                //Struct var
                if (node.GetConst() != null && node.GetInit() == null)
                    errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Const variables must have an initializer", false));
            }
            


            base.OutAALocalDecl(node);
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


       /* public override void CaseAArrayTempType(AArrayTempType node)
        {
            //Replace by an AArrayType
            //Have to wait untill after constant folding
            /*
            if (!(node.GetDimention() is AIntConstExp))
            {
                base.CaseAArrayTempType(node);
                return;
            }
            AIntConstExp dim = (AIntConstExp)node.GetDimention();
            AArrayType newType = new AArrayType(node.GetToken(), dim.GetIntegerLiteral(), node.GetType());
            node.ReplaceBy(newType);
            newType.Apply(this);
        }*/

        public override void OutATriggerDecl(ATriggerDecl node)
        {
            //If no actions, insert it
            if (node.GetActions() == null)
            {
                node.SetActions(new AABlock(new ArrayList(), new TRBrace("}")));
                node.SetActionsToken(new TActions("actions"));
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



            if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                errors.Add(new ErrorCollection.Error(node.GetName(), "Triggers can not be marked as protected."));

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

        public override void OutADeconstructorDecl(ADeconstructorDecl node)
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

            //Must be no parameters
            if (node.GetFormals().Count > 0)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), "A deconstructor can't take parameters"));
            }

            if (node.GetVisibilityModifier() is AProtectedVisibilityModifier && Util.HasAncestor<AEnrichmentDecl>(node))
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), "A deconstructors in enrichments can't be protected."));
            }

            base.OutADeconstructorDecl(node);
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
            //Check if delegate is valid
            if (node.GetDelegate() != null)
            {
                if (node.GetBlock() != null)
                    errors.Add(new ErrorCollection.Error(node.GetDelegate(), currentSourceFile, "Delegates can not have a body."));
                if (node.GetInline() != null)
                    errors.Add(new ErrorCollection.Error(node.GetDelegate(), currentSourceFile, "Delegates may not be marked as inline."));
                if (node.GetTrigger() != null)
                    errors.Add(new ErrorCollection.Error(node.GetDelegate(), currentSourceFile, "Delegates may not be marked as trigger."));
                if (node.GetStatic() != null)
                    errors.Add(new ErrorCollection.Error(node.GetDelegate(), currentSourceFile, "Delegates may not be marked as static."));
                if (node.GetNative() != null)
                    errors.Add(new ErrorCollection.Error(node.GetDelegate(), currentSourceFile, "Delegates may not be marked as native."));
            }
            //If it's protected, it must be in a struct
            if (!Util.HasAncestor<AStructDecl>(node))
            {
                if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                    errors.Add(new ErrorCollection.Error(node.GetName(),
                                                         "Only methods inside structs or classes can be marked as protected."));
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
                PLocalDecl decl = new ADeclLocalDecl(new AConstructorDecl(new APublicVisibilityModifier(),
                                                                          new TIdentifier(node.GetName().Text),
                                                                          new ArrayList(),
                                                                          new ArrayList(),
                                                                          new AABlock(new ArrayList(), new TRBrace("}"))));
                node.GetLocals().Add(decl);
                decl.Apply(this);
            }

            //Add deconstructor if not present
            if (node.GetLocals().OfType<ADeclLocalDecl>().Select(localDecl => localDecl.GetDecl()).OfType<ADeconstructorDecl>().ToList().Count == 0)
            {
                PLocalDecl decl =
                    new ADeclLocalDecl(new ADeconstructorDecl(new APublicVisibilityModifier(),
                                                              new TIdentifier(node.GetName().Text), new ArrayList(),
                                                              new AABlock(new ArrayList(), new TRBrace("}"))));
                node.GetLocals().Add(decl);
                decl.Apply(this);
            }

            if (node.GetVisibilityModifier() is AProtectedVisibilityModifier)
                errors.Add(new ErrorCollection.Error(node.GetName(), "Structs or classes can not be marked as protected."));
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
                //Cant have any [] or * after first lvalue
                if (node.GetPostPointers().Count > 0)
                {
                    Token token;
                    PShadyDynamicOps op = (PShadyDynamicOps)node.GetPostPointers()[0];
                    if (op is APointerShadyDynamicOps)
                        token = ((APointerShadyDynamicOps) op).GetToken();
                    else
                        token = ((AArrayShadyDynamicOps)op).GetToken();
                    errors.Add(new ErrorCollection.Error(token, currentSourceFile, "Didn't expect " + token.Text + " there. Im very confused."));
                }
                if (node.GetGenericToken() != null)
                    errors.Add(new ErrorCollection.Error(node.GetGenericToken(),
                                                         "What the hell is this? I didn't see that coming. Or maybe I did.. But I don't like it!"));

                //an assignment can't be const);
                if (node.GetConst() != null)
                    errors.Add(new ErrorCollection.Error(node.GetConst(), currentSourceFile, "Unexpected const", false));
                //An assignment must have a right side
                if (((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetInit() == null ||
                    ((AALocalDeclRight) node.GetLocalDeclRight()[0]).GetAssignop() == null)
                {
                    Token token = GetToken(node.GetLvalue());
                    errors.Add(new ErrorCollection.Error(token, currentSourceFile, "Expected assignment", false));
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
                    AAmbiguousNameLvalue ambiguousLvalue = (AAmbiguousNameLvalue) lvalueExp.GetLvalue();
                    if (ambiguousLvalue.GetAmbiguous() is AAName)
                    {
                        AAName simpleName = (AAName) ambiguousLvalue.GetAmbiguous();
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
            errors.Add(new ErrorCollection.Error(node.GetToken(), currentSourceFile, "Can only cast to string, text, int or fixed."));
        }

        

        public override void CaseAMultiLocalDecl(AMultiLocalDecl node)
        {
            PStm pStm = Util.GetAncestor<PStm>(node);
            if (!(pStm.Parent() is AABlock))
            {
                errors.Add(new ErrorCollection.Error(GetToken(node.GetType()), currentSourceFile, "A local declaration must be inside a block"));
                return;
            }
            AABlock pBlock = (AABlock) pStm.Parent();
            List<AALocalDecl> newDecls = new List<AALocalDecl>();
            foreach (AALocalDeclRight right in node.GetLocalDeclRight())
            {
                if (right.GetName() == null)
                {
                    errors.Add(new ErrorCollection.Error(GetToken(node.GetType()), currentSourceFile, "Expected a variable name"));
                    continue;
                }
                if ((right.GetAssignop() == null) != (right.GetInit() == null))
                {
                    errors.Add(new ErrorCollection.Error(GetToken(node.GetType()), currentSourceFile, "Expected an initializer"));
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

        private static Token GetToken(PLvalue lvalue)
        {
            if (lvalue is AStructLvalue)
                return ((AStructLvalue)lvalue).GetName();
            if (lvalue is AArrayLvalue)
                return ((AArrayLvalue)lvalue).GetToken();
            return (TIdentifier)((AAName)((AAmbiguousNameLvalue)lvalue).GetAmbiguous()).GetIdentifier()[0];
        }

        private static Token GetToken(PType type)
        {
            if (type is AVoidType)
                return ((AVoidType) type).GetToken();
            /*if (type is AArrayType)
                return ((AArrayType)type).GetToken();*/
            if (type is AArrayTempType)
                return ((AArrayTempType)type).GetToken();
            if (type is ANamedType)
                return (TIdentifier)((AAName)((ANamedType)type).GetName()).GetIdentifier()[0];
            throw new ParserException(null, "Weeder.GetToken(PType) - unexpected type (shouldnt happen)");
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
                    errors.Add(new ErrorCollection.Error(((AStructLvalue)lvalue).GetName(), currentSourceFile, "Invalid type name. Must be \"<namespace>.<struct name>\"."));
                    throw new ParserException(null, null);
                }
            }
            if (lvalue is AArrayLvalue)
            {
                AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                if (!(aLvalue.GetBase() is ALvalueExp))
                {
                    errors.Add(new ErrorCollection.Error(GetToken(lvalue), currentSourceFile, "Whatever that is, it's not allowed in a type"));
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
