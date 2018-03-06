using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.NotGenerated;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class TypeLinking : DepthFirstAdapter
    {

        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data)
        {
            ast.Apply(new TypeLinking(errors, data));
        }

        private ErrorCollection errors;
        private SharedData data;

        public TypeLinking(ErrorCollection errors, SharedData data)
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

        public override void InAAProgram(AAProgram node)
        {
            node.Apply(new BlockFixer(data));
        }

        class BlockFixer : DepthFirstAdapter
        {
            private SharedData data;

            public BlockFixer(SharedData data)
            {
                this.data = data;
            }

            public override void InAABlock(AABlock node)
            {
                if (!data.Locals.ContainsKey(node))
                    data.Locals.Add(node, new List<AALocalDecl>());
                AABlock pBlock = Util.GetAncestor<AABlock>(node.Parent());
                if (pBlock != null)
                    data.Locals[node].AddRange(data.Locals[pBlock]);
            }
        }

        /*Moved to it's own class
        public override void OutANamedType(ANamedType node)
        {
            //Link named type to their definition (structs)
            //Lookup the named type unless it is a primitive
            if (!GalaxyKeywords.Primitives.words.Any(primitive => node.GetName().Text == primitive))
            {
                if (node.Parent() is AEnrichmentDecl) //Enriching a struct type. Not allowed
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "You can not enrich this type."));
                    return;
                }

                //Look for structs
                foreach (SharedData.DeclItem<AStructDecl> declItem in data.Structs)
                {
                    if (node.GetNamespace() != null)
                    {
                        AASourceFile targetFile = Util.GetAncestor<AASourceFile>(declItem.Decl);
                        if (targetFile == null || targetFile.GetNamespace() == null || targetFile.GetNamespace().Text != node.GetNamespace().Text)
                            continue;
                    }
                    else if (!Util.IsVisible(node, declItem.Decl))
                        continue;

                    AStructDecl decl = declItem.Decl;
                    if (decl.GetName().Text == node.GetName().Text)
                    {
                        if (decl.GetClassToken() != null && !(node.Parent() is APointerType || node.Parent() is ANewExp))
                        {
                            errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                                 "You can only make dynamic instansiations of classes."));
                        }

                        data.StructTypeLinks.Add(node, decl);
                        goto end;
                    }
                }
                //Look for delegates
                foreach (SharedData.DeclItem<AMethodDecl> declItem in data.Delegates)
                {
                    if (node.GetNamespace() != null)
                    {
                        AASourceFile targetFile = Util.GetAncestor<AASourceFile>(declItem.Decl);
                        if (targetFile == null || targetFile.GetNamespace() == null || targetFile.GetNamespace().Text != node.GetNamespace().Text)
                            continue;
                    }
                    else if (!Util.IsVisible(node, declItem.Decl))
                        continue;

                    AMethodDecl decl = declItem.Decl;
                    if (decl.GetName().Text == node.GetName().Text)
                    {
                        data.DelegateTypeLinks.Add(node, decl);
                        goto end;
                    }
                }

                errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "No type found named " + node.GetName().Text, false), true);
            }
            else if (node.GetNamespace() != null)
            {
                errors.Add(new ErrorCollection.Error(node.GetNamespace(), currentSourceFile, "You cannot put a namespace infront of a primitive type "), true);
            }
 
        end:
            base.OutANamedType(node);
        }
        */

        public override void CaseANewExp(ANewExp node)
        {
            bool wasInNew = isInANewExp;
            isInANewExp = true;
            base.CaseANewExp(node);
            isInANewExp = wasInNew;
        }


        //Check that array definitions have constant stuff
        private bool foldIntegerConstants;
        private bool isInANewExp;
        private bool foldingFailed;
        private int integerConstant;
        private Token integerConstantToken;
        public override void CaseAArrayTempType(AArrayTempType node)
        {
            /*if (node.GetDimention() is ALvalueExp && ((ALvalueExp)node.GetDimention()).GetLvalue() is AAmbiguousNameLvalue)
            {
                AAmbiguousNameLvalue lvalue = (AAmbiguousNameLvalue) ((ALvalueExp) node.GetDimention()).GetLvalue();
                ASimpleName name = (ASimpleName) lvalue.GetAmbiguous();
                if (name.GetIdentifier().Text == "PlayerData")
                    node = node;
            }*/
            if (!isInANewExp && !data.IsLiteCompile)
            {
                bool wasFolding = foldIntegerConstants;
                bool foldFailedBefore = foldingFailed;

                foldIntegerConstants = true;
                foldingFailed = false;
                integerConstant = 0;
                integerConstantToken = node.GetToken();
                CheckValidConstExp(node.GetDimention());
                base.CaseAArrayTempType(node);
                foldIntegerConstants = false;
                if (!foldingFailed)
                    node.SetIntDim(new TIntegerLiteral(integerConstant.ToString()));

                foldIntegerConstants = wasFolding;
                foldingFailed = foldFailedBefore;

            }
            else
            {
                base.CaseAArrayTempType(node);
            }
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            if (node.GetDimention() != null && !data.IsLiteCompile)
            {
                //Find int dim
                foldIntegerConstants = true;
                isInANewExp = true;//If false, we dont require a value
                node.GetDimention().Apply(this);
                isInANewExp = false;
                foldIntegerConstants = false;
                if (foldingFailed)
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile, "Struct array markers must be constant."));
                    throw new ParserException(node.GetName(), "TypeLinking.CaseAStructDecl");
                }
                node.SetIntDim(new TIntegerLiteral(integerConstant.ToString()));
            }
            base.CaseAStructDecl(node);
        }

        public override void CaseAEnrichmentDecl(AEnrichmentDecl node)
        {
            if (node.GetDimention() != null && !data.IsLiteCompile)
            {
                //Find int dim
                foldIntegerConstants = true;
                isInANewExp = true;//If false, we dont require a value
                node.GetDimention().Apply(this);
                isInANewExp = false;
                foldIntegerConstants = false;
                if (foldingFailed)
                {
                    errors.Add(new ErrorCollection.Error(node.GetToken(), "Enrichment array markers must be constant."));
                    throw new ParserException(node.GetToken(), "TypeLinking.CaseAStructDecl");
                }
                node.SetIntDim(new TIntegerLiteral(integerConstant.ToString()));
            }
            base.CaseAEnrichmentDecl(node);
        }

        private void CheckValidConstExp(PExp exp)
        {
            if (exp is ABinopExp || exp is AIntConstExp)
                return;
            if (exp is ALvalueExp)
            {
                PLvalue lvalue = ((ALvalueExp) exp).GetLvalue();
                if (lvalue is AAmbiguousNameLvalue || lvalue is AFieldLvalue || lvalue is ALocalLvalue || lvalue is AStructLvalue)
                    return;
            }
            errors.Add(new ErrorCollection.Error(integerConstantToken, currentSourceFile, "Dimensions of array types must be constant expressions.", false), true);
            throw new ParserException(integerConstantToken, "TypeLinking.CheckValidConstExp");
        }

        public override void OutAIntConstExp(AIntConstExp node)
        {
            if (foldIntegerConstants)
            {
                integerConstant = int.Parse(node.GetIntegerLiteral().Text);
            }
        }

       

        public override void CaseABinopExp(ABinopExp node)
        {
            if (!foldIntegerConstants)
            {
                base.CaseABinopExp(node);
                return;
            }

            CheckValidConstExp(node.GetLeft());
            CheckValidConstExp(node.GetRight());
            node.GetLeft().Apply(this);
            int left = integerConstant;
            node.GetBinop().Apply(this);
            node.GetRight().Apply(this);
            int right = integerConstant;

            if (node.GetBinop() is APlusBinop)
                integerConstant = left + right;
            else if (node.GetBinop() is AMinusBinop)
                integerConstant = left - right;
            else if (node.GetBinop() is ATimesBinop)
                integerConstant = left * right;
            else if (node.GetBinop() is ADivideBinop)
            {
                if (right == 0)
                {
                    errors.Add(new ErrorCollection.Error(((ADivideBinop)node.GetBinop()).GetToken(), currentSourceFile, "Division by zero", false), true);
                    throw new ParserException(null, "EnviromentChecking.CaseABinopExp");
                }
                integerConstant = left / right;
            }
            else if (node.GetBinop() is AModuloBinop)
            {
                if (right == 0)
                {
                    errors.Add(new ErrorCollection.Error(((AModuloBinop)node.GetBinop()).GetToken(), currentSourceFile, "Division by zero", false), true);
                    throw new ParserException(null, "EnviromentChecking.CaseABinopExp");
                }
                integerConstant = left % right;
            }
            else if (node.GetBinop() is AAndBinop)
                integerConstant = left & right;
            else if (node.GetBinop() is AOrBinop)
                integerConstant = left | right;
            else if (node.GetBinop() is AXorBinop)
                integerConstant = left ^ right;
            else if (node.GetBinop() is ALBitShiftBinop)
                integerConstant = left << right;
            else if (node.GetBinop() is ARBitShiftBinop)
                integerConstant = left >> right;

        }


        public override void OutALocalLvalue(ALocalLvalue node)
        {
            if (foldIntegerConstants)
            {
                AALocalDecl local = data.LocalLinks[node];
                if (local.GetConst() == null)
                {
                    foldingFailed = true;
                    if (!isInANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                      "Dimensions of array types must be constant expressions.",
                                                      false), true);
                        throw new ParserException(null, null);
                    }
                }
                if (local.GetInit() == null)//An error will be given earlier
                    throw new ParserException(null, null);
                local.GetInit().Apply(this);
            }
        }

        

        public override void OutAFieldLvalue(AFieldLvalue node)
        {
            //Since we dont apply the replacement of an OutAAmbiguousNameLvalue, this node needs a link
            //Eks: global.<field>
            //Look for fields
            AFieldDecl f = null;
            APropertyDecl p = null;
            if (data.FieldLinks.ContainsKey(node))
                f = data.FieldLinks[node];
            else //if (!data.FieldLinks.ContainsKey(node) )
            {
                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
                List<string> currentNamespace = Util.GetFullNamespace(node);
                List<PDecl> candidates = new List<PDecl>();
                foreach (IList declList in visibleDecls)
                {
                    bool isSameFile = false;
                    bool isSameNamespace = false;
                    if (declList.Count > 0)
                    {
                        isSameFile = currentFile == Util.GetAncestor<AASourceFile>((PDecl)declList[0]);
                        isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace((PDecl)declList[0]));
                    }
                    foreach (PDecl decl in declList)
                    {
                        if (decl is AFieldDecl)
                        {
                            AFieldDecl field = (AFieldDecl)decl;

                            if (field.GetName().Text != node.GetName().Text)
                                continue;

                            if (!isSameNamespace && field.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && field.GetStatic() != null)
                                continue;

                            candidates.Add(decl);
                        }
                        else if (decl is APropertyDecl)
                        {
                            APropertyDecl property = (APropertyDecl)decl;

                            if (property.GetName().Text != node.GetName().Text)
                                continue;

                            if (!isSameNamespace && property.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && property.GetStatic() != null)
                                continue;

                            candidates.Add(decl);
                        }
                    }
                }

                //Lib fields
                foreach (AFieldDecl field in data.Libraries.Fields)
                {
                    if (field.GetName().Text != node.GetName().Text)
                        continue;

                    if (field.GetStatic() != null)
                        continue;

                    candidates.Add(field);
                }

                if (candidates.Count == 0)
                {
                    errors.Add(
                        new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                  node.GetName().Text + " did not match any defined fields or properties.",
                                                  false), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }
                if (candidates.Count > 1)
                {
                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                    foreach (PDecl decl in candidates)
                    {
                        if (decl is AFieldDecl)
                        {
                            AFieldDecl field = (AFieldDecl)decl;
                            subErrors.Add(new ErrorCollection.Error(field.GetName(), "Matching field"));
                        }
                        else if (decl is APropertyDecl)
                        {
                            APropertyDecl field = (APropertyDecl)decl;
                            subErrors.Add(new ErrorCollection.Error(field.GetName(), "Matching property"));
                        }
                    }

                    errors.Add(
                        new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                  node.GetName().Text + " matched multiple defined fields or properties.",
                                                  false, subErrors.ToArray()), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }
                if (candidates[0] is AFieldDecl)
                    f = (AFieldDecl)candidates[0];
                else
                    p = (APropertyDecl)candidates[0];
            }

            if (f != null)
            {
                AFieldDecl field = f;
                data.FieldLinks[node] = field;
                if (foldIntegerConstants)
                {
                    if (!(field.GetType() is ANamedType && ((ANamedType)field.GetType()).IsPrimitive("int")))
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(),
                                                      "Dimensions of array types must integer expressions.",
                                                      false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }

                    if (field.GetConst() == null)
                    {
                        foldingFailed = true;
                        if (!isInANewExp)
                        {
                            errors.Add(
                                new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                          "Dimensions of array types must be constant expressions.",
                                                          false), true);
                            throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                        }
                    }
                    if (field.GetInit() == null)//An error will be given earlier - constant fields must have an initializer
                    {
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }
                    field.GetInit().Apply(this);
                }
            }
            else
            {
                APropertyDecl property = p;
                APropertyLvalue replacer = new APropertyLvalue(node.GetName());
                data.PropertyLinks.Add(replacer, property);
                node.ReplaceBy(replacer);
                data.PropertyLinks[replacer] = property;
                if (foldIntegerConstants)
                {
                    foldingFailed = true;
                    if (isInANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                        "Dimensions of array types must be constant expressions.",
                                                        false), true);
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }
                }
            }
        }


        public override void OutAStructLvalue(AStructLvalue node)
        {
            if (foldIntegerConstants)
            {
                //Only static fields are valid
                /*if (!(node.GetReceiver() is ALvalueExp) || !(((ALvalueExp)node.GetReceiver()).GetLvalue() is ATypeLvalue))
                {
                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must be constant expressions."), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAStructLvalue");
                }*/
                //The type has not yet been linked.
            }
        }

        public override void OutAStructFieldLvalue(AStructFieldLvalue node)
        {
            if (!data.StructMethodFieldLinks.ContainsKey(node) &&
                !data.StructMethodPropertyLinks.ContainsKey(node))
            {
                AStructDecl str = Util.GetAncestor<AStructDecl>(node);
                AConstructorDecl constructor = Util.GetAncestor<AConstructorDecl>(node);
                if (str.GetClassToken() != null || constructor != null || Util.HasAncestor<ADeconstructorDecl>(node))
                {
                    errors.Add(new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                         "Unable to use struct in classes, constructors and deconstructors. Use this instead."));
                }
                if (str != null)
                {
                    foreach (AALocalDecl localDecl in data.StructFields[str])
                    {
                        if (localDecl.GetName().Text == node.GetName().Text)
                        {
                            data.StructMethodFieldLinks.Add(node, localDecl);
                            return;
                        }
                    }
                    foreach (APropertyDecl property in data.StructProperties[str])
                    {
                        if (property.GetName().Text == node.GetName().Text)
                        {
                            data.StructMethodPropertyLinks.Add(node, property);
                            return;
                        }
                    }
                }
                errors.Add(
                    new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                              node.GetName().Text + " did not match any defined struct fields.", false),
                    true);
            }
            //We can be sure this is linked. If in an array index, it must be constant.
            if (foldIntegerConstants)
            {
                if (data.StructMethodPropertyLinks.ContainsKey(node))
                {

                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must be constant expressions.",
                                                  false,
                                                  new ErrorCollection.Error(
                                                      data.StructMethodPropertyLinks[node].GetName(),
                                                      "Matching property")), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAStructFieldLvalue");
                }
                AALocalDecl field = data.StructMethodFieldLinks[node];
                if (!(field.GetType() is ANamedType && ((ANamedType)field.GetType()).IsPrimitive("int")))
                {
                    errors.Add(
                        new ErrorCollection.Error(node.GetName(),
                                                  "Dimensions of array types must be integer expressions.",
                                                  false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                    throw new ParserException(node.GetName(), "TypeLinking.OutAStructFieldLvalue");
                }

                if (field.GetConst() == null)
                {
                    foldingFailed = true;
                    if (!isInANewExp)
                    {
                        errors.Add(
                            new ErrorCollection.Error(node.GetName(), currentSourceFile,
                                                      "Dimensions of array types must be constant expressions.",
                                                      false, new ErrorCollection.Error(field.GetName(), "Matching field")), true);
                        throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                    }
                }
                if (field.GetInit() == null)//An error will be given earlier - constant fields must have an initializer
                {
                    throw new ParserException(node.GetName(), "TypeLinking.OutAFieldLvalue");
                }
                field.GetInit().Apply(this);
            }
            //base.CaseAStructFieldLvalue(node);
        }

        public static ErrorCollection.Error GetErrorPath(List<Node> nodes)
        {
            if (nodes.Count == 1)
            {
                Token token = null;
                Node n = nodes[0];
                if (n is AALocalDecl)
                    token = ((AALocalDecl) n).GetName();
                else if (n is APropertyDecl)
                    token = ((APropertyDecl)n).GetName();
                else if (n is AFieldDecl)
                    token = ((AFieldDecl)n).GetName();
                else if (n is AStructDecl)
                    token = ((AStructDecl)n).GetName();
                return new ErrorCollection.Error(token, "Matching declaration");
            }
            else
            {
                List<ErrorCollection.Error> path = new List<ErrorCollection.Error>();
                int i = 0;
                Token token = null;
                foreach (Node n in nodes)
                {
                    i++;
                    string text = "Item " + i;
                    if (n is AALocalDecl)
                        token = ((AALocalDecl)n).GetName();
                    else if (n is APropertyDecl)
                        token = ((APropertyDecl)n).GetName();
                    else if (n is AFieldDecl)
                        token = ((AFieldDecl)n).GetName();
                    else if (n is AStructDecl)
                        token = ((AStructDecl)n).GetName();
                    else if (n is TIdentifier)
                    {
                        token = (TIdentifier)n;
                        text += ": Array length";
                    }
                    path.Add(new ErrorCollection.Error(token, text));
                }
                return new ErrorCollection.Error(token, "Matching declaration", false, path.ToArray());
            }
        }

        public override void OutAAmbiguousNameLvalue(AAmbiguousNameLvalue node)
        {
            if (node.Parent() is ADelegateExp)
                return;//Handle in delegate
            if (node.Parent().Parent() is ANonstaticInvokeExp && ((ANonstaticInvokeExp)node.Parent().Parent()).GetReceiver() == node.Parent())
                return;//Handle in nonstatic invoke
            if (node.Parent() is ASyncInvokeExp && ((ASyncInvokeExp)node.Parent()).GetName() == node)
                return;
            if (node.Parent() is AAsyncInvokeStm && ((AAsyncInvokeStm)node.Parent()).GetName() == node)
                return;
            //Transform AAmbigiousNameLvalue.

            List<List<Node>>[] targets;
            List<ANamespaceDecl> namespaces = new List<ANamespaceDecl>();
            bool reportedError;

            AAName aName = (AAName) node.GetAmbiguous();
            GetTargets(aName, out targets, namespaces, data, errors, out reportedError, false);

            if (reportedError)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].Count == 0)
                    continue;
                if (targets[i].Count > 1)
                {
                    List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                    foreach (List<Node> list in targets[i])
                    {
                        subErrors.Add(GetErrorPath(list));
                    }
                    errors.Add(new ErrorCollection.Error(aName.GetToken(), "Found multiple matching declarations", false, subErrors.ToArray()));
                    return;
                }
                PLvalue replacer = Link(aName, node, targets[i][0], data);
                node.ReplaceBy(replacer);
                replacer.Apply(this);
                return;
            }
            errors.Add(new ErrorCollection.Error(aName.GetToken(), "Found no matching declarations"));
        }

        
        /*
         * Local variable (including formals)
         * Special case: Formals from base constructor calls
         * 
         * Struct fields/properties in current struct
         *      Not enherited private fields
         *      Note: If it was a class field, it must be replaced by this->... when used
         * 
         * Struct fields/properties in target struct
         * 
         * Global field/property
         * 
         * NamedType (primitive and structs/classes)
         * 
         * enriched primitive
         * 
         * Namespace
         */
        public static void GetTargets(AAName name,
            out List<List<Node>>[] targets,
            List<ANamespaceDecl> namespaces,
            SharedData data,
            ErrorCollection errors,
            out bool reportedError, 
            bool allowStructs = true)
        {
            List<string> names = new List<string>();
            foreach (TIdentifier identifier in name.GetIdentifier())
            {
                names.Add(identifier.Text);
            }
            GetTargets(name, names, out targets, namespaces, data, errors, out reportedError, !allowStructs);
        }

        private static void GetTargets(AAName node,
            List<string> names,
            out List<List<Node>>[] targets,
            List<ANamespaceDecl> namespaces,
            SharedData data, 
            ErrorCollection errors,
            out bool reportedError, 
            bool first = false)
        {
            targets = new []
                          {
                              new List<List<Node>>(),//0: Stuff starting with a local variable
                              new List<List<Node>>(),//1: Stuff starting with a struct field/property
                              new List<List<Node>>() //2: Stuff starting with a global declaration
                          };
            reportedError = false;
            string name = names[names.Count - 1];
            if (names.Count == 1)
            {
                //Locals
                AConstructorDecl pConstructor = Util.GetAncestor<AConstructorDecl>(node);
                AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
                AABlock pBlock = Util.GetAncestor<AABlock>(node);
                if (pBlock != null)
                {
                    if (data.Locals.ContainsKey(pBlock))
                    {
                        foreach (AALocalDecl local in data.Locals[pBlock])
                        {
                            if (local.GetName().Text == name && Util.IsBefore(local, node))
                                targets[0].Add(new List<Node>(){local});
                        }
                    }
                }
                else if (pConstructor != null)
                {
                    foreach (AALocalDecl formal in pConstructor.GetFormals())
                    {
                        if (formal.GetName().Text == name)
                            targets[0].Add(new List<Node>(){formal});
                    }
                }
                //Fields/properties in current struct
                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                if (currentStruct != null)
                {
                    bool isStaticContext = false;
                    if (Util.HasAncestor<AMethodDecl>(node))
                        isStaticContext = Util.GetAncestor<AMethodDecl>(node).GetStatic() != null;
                    else if (Util.HasAncestor<APropertyDecl>(node))
                        isStaticContext = Util.GetAncestor<APropertyDecl>(node).GetStatic() != null;
                    else if (Util.HasAncestor<AALocalDecl>(node))
                        isStaticContext = Util.GetAncestor<AALocalDecl>(node).GetStatic() != null;
                    foreach (AALocalDecl local in data.StructFields[currentStruct])
                    {
                        if (local.GetName().Text != name)
                            continue;
                        //If it's an enherited private variable, you can't refer to it.
                        if (local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                            data.EnheritanceLocalMap.ContainsKey(local))
                        {
                            continue;
                        }
                        if (local.GetStatic() != null)
                        {
                            //Add it to the dotted map
                            targets[1].Add(new List<Node>(){currentStruct, local});
                            continue;
                        }
                        if (isStaticContext)
                            continue;//Can't refference non static stuff from static context
                        targets[1].Add(new List<Node>(){local});
                    }
                    foreach (APropertyDecl local in data.StructProperties[currentStruct])
                    {
                        if (local.GetName().Text != name)
                            continue;

                        //If it's an enherited private variable, you can't refer to it.
                        if (local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                            Util.GetAncestor<AStructDecl>(local) != currentStruct)
                        {
                            continue;
                        }

                        if (local.GetStatic() != null)
                        {
                            //Add it to the dotted map
                            targets[1].Add(new List<Node>() { currentStruct, local });
                            continue;
                        }
                        if (isStaticContext)
                            continue;//Can't refference non static stuff from static context
                        targets[1].Add(new List<Node>(){local});
                    }
                }
                //Global field/property
                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
                List<string> currentNamespace = Util.GetFullNamespace(node);
                foreach (IList declList in visibleDecls)
                {
                    bool isSameFile = false;
                    if (declList.Count > 0)
                        isSameFile = currentFile == Util.GetAncestor<AASourceFile>((PDecl) declList[0]);
                    foreach (PDecl decl in declList)
                    {
                        if (decl is AFieldDecl)
                        {
                            AFieldDecl aDecl = (AFieldDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && aDecl.GetStatic() != null)
                                continue;

                            targets[2].Add(new List<Node>(){decl});
                        }
                        else if (decl is APropertyDecl)
                        {
                            APropertyDecl aDecl = (APropertyDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && aDecl.GetStatic() != null)
                                continue;

                            targets[2].Add(new List<Node>() { decl });
                        }
                        else if (decl is AStructDecl && !first)
                        {
                            AStructDecl aDecl = (AStructDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier)
                                continue;

                            targets[2].Add(new List<Node>() { decl });
                        }
                    }
                }
                //Look in lib fields
                foreach (AFieldDecl field in data.Libraries.Fields)
                {
                    if (field.GetName().Text == name)
                    {
                        targets[2].Add(new List<Node>() { field });
                    }
                }
                //Namespaces
                visibleDecls = Util.GetVisibleDecls(node, false);
                foreach (IList declList in visibleDecls)
                {
                    foreach (PDecl decl in declList)
                    {
                        if (decl is ANamespaceDecl)
                        {
                            ANamespaceDecl aDecl = (ANamespaceDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            namespaces.Add(aDecl);
                        }
                    }
                }
            }
            else
            {
                /*private static void GetTargets(AAName node,
            List<string> names,
            List<AALocalDecl> locals,
            List<Node> structDecls, //<AAlocaldecl/APropertyDecl>
            List<PDecl> globalDecls,
            List<AStructDecl> structType,
            List<List<Node>> dotted, //<any of the above>.<AAlocaldecl/APropertyDecl>
            List<ANamespaceDecl> namespaces,
            SharedData data, 
            ErrorCollection errors,
            out bool reportedError)*/
                List<string> baseNames = new List<string>();
                baseNames.AddRange(names);
                baseNames.RemoveAt(baseNames.Count - 1);
                List<List<Node>>[] baseTargets;
                List<ANamespaceDecl> baseNamespaces = new List<ANamespaceDecl>();
                GetTargets(node, baseNames, out baseTargets, baseNamespaces, data, errors, out reportedError);

                AStructDecl currentStruct = Util.GetAncestor<AStructDecl>(node);
                for (int i = 0; i < baseTargets.Length; i++)
                {
                    foreach (List<Node> list in baseTargets[i])
                    {
                        Node last = list[list.Count - 1];
                        PType type = null;
                        if (last is AALocalDecl)
                        {
                            type = ((AALocalDecl) last).GetType();
                        }
                        else if (last is APropertyDecl)
                        {
                            type = ((APropertyDecl) last).GetType();
                        }
                        else if (last is AFieldDecl)
                        {
                            type = ((AFieldDecl) last).GetType();
                        }
                        else if (last is TIdentifier)
                        {
                            type = new ANamedType(new TIdentifier("int"), null);
                        }
                        if (last is AStructDecl)
                        {
                            //Special. Static only
                            AStructDecl structDecl = ((AStructDecl) last);
                            foreach (AALocalDecl local in data.StructFields[structDecl])
                            {
                                if (local.GetName().Text != name)
                                    continue;
                                //Must be public if we are outside the struct
                                //If it's an enherited private variable, you can't refer to it.
                                if (currentStruct != structDecl && !(local.GetVisibilityModifier() is APublicVisibilityModifier) ||
                                    currentStruct == structDecl &&
                                    local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                    data.EnheritanceLocalMap.ContainsKey(local))
                                {
                                    continue;
                                }
                                if (local.GetStatic() == null)
                                {
                                    //non Static types doesn't work in this context
                                    continue;
                                }
                                List<Node> nodeList = new List<Node>();
                                nodeList.Add(structDecl);
                                nodeList.Add(local);
                                targets[i].Add(nodeList);
                            }
                            foreach (APropertyDecl local in data.StructProperties[structDecl])
                            {

                                if (local.GetName().Text != name)
                                    continue;
                                //Must be public if we are outside the struct
                                //If it's an enherited private variable, you can't refer to it.
                                if (currentStruct != structDecl && !(local.GetVisibilityModifier() is APublicVisibilityModifier))
                                {
                                    continue;
                                }
                                if (local.GetStatic() == null)
                                {
                                    //non Static types doesn't work in this context
                                    continue;
                                }
                                List<Node> nodeList = new List<Node>();
                                nodeList.Add(structDecl);
                                nodeList.Add(local);
                                targets[i].Add(nodeList);
                            }
                            
                        }
                        else
                        {
                            if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType)type) && !(data.Enums.ContainsKey(data.StructTypeLinks[(ANamedType)type])))
                            {
                                AStructDecl targetStruct = data.StructTypeLinks[(ANamedType)type];
                                foreach (AALocalDecl local in data.StructFields[targetStruct])
                                {
                                    if (local.GetName().Text != name)
                                        continue;
                                    //Must be public if we are outside the struct
                                    //If it's an enherited private variable, you can't refer to it.
                                    if (currentStruct != targetStruct && !(local.GetVisibilityModifier() is APublicVisibilityModifier) ||
                                        currentStruct == targetStruct &&
                                        local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                                        data.EnheritanceLocalMap.ContainsKey(local))
                                    {
                                        continue;
                                    }
                                    if (local.GetStatic() != null)
                                    {
                                        //Static types doesn't work in this context
                                        continue;
                                    }
                                    List<Node> nodeList = new List<Node>();
                                    nodeList.AddRange(list);
                                    nodeList.Add(local);
                                    targets[i].Add(nodeList);
                                }
                                foreach (APropertyDecl local in data.StructProperties[targetStruct])
                                {

                                    if (local.GetName().Text != name)
                                        continue;
                                    //Must be public if we are outside the struct
                                    //If it's an enherited private variable, you can't refer to it.
                                    if (currentStruct != targetStruct && !(local.GetVisibilityModifier() is APublicVisibilityModifier))
                                    {
                                        continue;
                                    }
                                    if (local.GetStatic() != null)
                                    {
                                        //Static types doesn't work in this context
                                        continue;
                                    }
                                    List<Node> nodeList = new List<Node>();
                                    nodeList.AddRange(list);
                                    nodeList.Add(local);
                                    targets[i].Add(nodeList);
                                }
                            }
                            else//Find matching enrichment
                            {
                                List<IList> visibleDecls = Util.GetVisibleDecls(node, true);
                                AEnrichmentDecl currentEnrichment = Util.GetAncestor<AEnrichmentDecl>(node);
                                foreach (IList declList in visibleDecls)
                                {
                                    foreach (PDecl decl in declList)
                                    {
                                        if (decl is AEnrichmentDecl)
                                        {
                                            AEnrichmentDecl aDecl = (AEnrichmentDecl)decl;
                                            if (Util.TypesEqual(aDecl.GetType(), type, data))
                                            {
                                                foreach (PDecl enrichmentDecl in aDecl.GetDecl())
                                                {
                                                    if (enrichmentDecl is APropertyDecl)
                                                    {
                                                        APropertyDecl local = (APropertyDecl)enrichmentDecl;
                                                        if (local.GetName().Text != name)
                                                            continue;
                                                        //Must be public if we are outside the struct
                                                        if (currentEnrichment != aDecl && !(local.GetVisibilityModifier() is APublicVisibilityModifier))
                                                        {
                                                            continue;
                                                        }
                                                        if (local.GetStatic() != null)
                                                        {
                                                            //Static types doesn't work in this context
                                                            continue;
                                                        }
                                                        List<Node> nodeList = new List<Node>();
                                                        nodeList.AddRange(list);
                                                        nodeList.Add(local);
                                                        targets[i].Add(nodeList);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //Could be array.length
                                if ((type is AArrayTempType || type is ADynamicArrayType) && name == "length")
                                {
                                    List<Node> nodeList = new List<Node>();
                                    nodeList.AddRange(list);
                                    nodeList.Add((TIdentifier)node.GetIdentifier()[names.Count - 1]);
                                    targets[i].Add(nodeList);
                                }
                            }
                        }
                    }
                }

                AASourceFile currentFile = Util.GetAncestor<AASourceFile>(node);
                List<string> currentNamespace = Util.GetFullNamespace(node);
                foreach (ANamespaceDecl ns in baseNamespaces)
                {
                    bool isSameFile = currentFile == Util.GetAncestor<AASourceFile>(ns);
                    foreach (PDecl decl in ns.GetDecl())
                    {
                        if (decl is AFieldDecl)
                        {
                            AFieldDecl aDecl = (AFieldDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && aDecl.GetStatic() != null)
                                continue;

                            targets[2].Add(new List<Node>(){decl});
                        }
                        else if (decl is APropertyDecl)
                        {
                            APropertyDecl aDecl = (APropertyDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier ||
                                !isSameFile && aDecl.GetStatic() != null)
                                continue;

                            targets[2].Add(new List<Node>() { decl });
                        }
                        else if (decl is AStructDecl)
                        {
                            AStructDecl aDecl = (AStructDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            bool isSameNamespace = Util.NamespacesEquals(currentNamespace, Util.GetFullNamespace(decl));

                            if (!isSameNamespace && aDecl.GetVisibilityModifier() is APrivateVisibilityModifier)
                                continue;

                            targets[2].Add(new List<Node>() { decl });
                        }
                        else if (decl is ANamespaceDecl)
                        {
                            ANamespaceDecl aDecl = (ANamespaceDecl)decl;

                            if (aDecl.GetName().Text != name)
                                continue;

                            namespaces.Add(aDecl);
                        }
                    }
                }
            }
            
            //If we got no matches, and we are not last, report error
            if (errors != null && node.GetIdentifier().Count > names.Count &&
                targets[0].Count + targets[1].Count + targets[2].Count + namespaces.Count == 0)
            {
                string dotList = "";
                foreach (string s in names)
                {
                    if (dotList != "")
                        dotList += ".";
                    dotList += s;
                }
                errors.Add(new ErrorCollection.Error((TIdentifier)node.GetIdentifier()[names.Count - 1], dotList + " did not match any definitions."));
                reportedError = true;
            }
        }

        public static PLvalue Link(AAName name, Node refNode, List<Node> list, SharedData data)
        {
            List<TIdentifier> identifierList = new List<TIdentifier>();
            {
                int count = name.GetIdentifier().Count;
                if (count < list.Count)
                {
                    for (int i = 0; i < list.Count - count; i++)
                    {
                        if (list[i] is AStructDecl)
                            identifierList.Add(((AStructDecl)list[i]).GetName());
                    }

                    for (int i = 0; i < count; i++)
                    {
                        TIdentifier iden = (TIdentifier)name.GetIdentifier()[i];
                        identifierList.Add(iden);
                    }
                }
                else
                    for (int i = count - list.Count; i < count; i++)
                    {
                        TIdentifier iden = (TIdentifier)name.GetIdentifier()[i];
                        identifierList.Add(iden);
                    }
            }
            PLvalue baseLvalue = null;
            Node node = list[0];
            list.RemoveAt(0);
            TIdentifier identifier = identifierList[0];
            identifierList.RemoveAt(0);
            if (node is AALocalDecl)
            {
                AALocalDecl aNode = (AALocalDecl)node;
                if (node.Parent() is AStructDecl)
                {//Struct local
                    //Make it this->var or this.var
                    AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);
                    if (pStruct.GetClassToken() != null || Util.HasAncestor<AConstructorDecl>(refNode) || Util.HasAncestor<ADeconstructorDecl>(refNode))
                    {//(*this).
                        baseLvalue = new AThisLvalue(new TThis("this"));
                        baseLvalue = new APointerLvalue(new TStar("*"), new ALvalueExp(baseLvalue));
                        baseLvalue = new AStructLvalue(new ALvalueExp(baseLvalue), new ADotDotType(new TDot(".")),
                                                       identifier);
                        data.StructFieldLinks[(AStructLvalue)baseLvalue] = aNode;
                    }
                    else
                    {//struct.
                        baseLvalue = new AStructFieldLvalue(identifier);
                        data.StructMethodFieldLinks[(AStructFieldLvalue)baseLvalue] = aNode;
                    }
                }
                else
                {//Method/constructor/deconstructor local
                    ALocalLvalue replaceNode = new ALocalLvalue(identifier);
                    data.LocalLinks[replaceNode] = aNode;
                    baseLvalue = replaceNode;
                }
            }
            else if (node is APropertyDecl)
            {
                APropertyDecl aNode = (APropertyDecl)node;
                if (Util.HasAncestor<AStructDecl>(node))
                {//Property in current struct
                    AStructDecl pStruct = Util.GetAncestor<AStructDecl>(node);
                    if (pStruct.GetClassToken() != null || Util.HasAncestor<AConstructorDecl>(refNode) || Util.HasAncestor<ADeconstructorDecl>(refNode))
                    {//(*this).
                        baseLvalue = new AThisLvalue(new TThis("this"));
                        baseLvalue = new APointerLvalue(new TStar("*"), new ALvalueExp(baseLvalue));
                        baseLvalue = new AStructLvalue(new ALvalueExp(baseLvalue), new ADotDotType(new TDot(".")),
                                                       identifier);
                        data.StructPropertyLinks[(AStructLvalue)baseLvalue] = aNode;
                    }
                    else
                    {//struct.
                        baseLvalue = new AStructFieldLvalue(identifier);
                        data.StructMethodPropertyLinks[(AStructFieldLvalue)baseLvalue] = aNode;
                    }
                }
                else
                {//Global property
                    baseLvalue = new APropertyLvalue(identifier);
                    data.PropertyLinks[(APropertyLvalue)baseLvalue] = aNode;
                }
            }
            else if (node is AFieldDecl)
            {
                baseLvalue = new AFieldLvalue(identifier);
                data.FieldLinks[(AFieldLvalue)baseLvalue] = (AFieldDecl)node;
            }
            else if (node is AStructDecl)
            {
                AStructDecl targetStruct = (AStructDecl)node;
                node = list[0];
                list.RemoveAt(0);
                identifier = identifierList[0];
                identifierList.RemoveAt(0);

                AStructFieldLvalue lvalue = new AStructFieldLvalue(identifier);
                if (node is AALocalDecl)
                    data.StructMethodFieldLinks[lvalue] = (AALocalDecl)node;
                else
                    data.StructMethodPropertyLinks[lvalue] = (APropertyDecl)node;


                baseLvalue = lvalue;
            }
            while (list.Count > 0)
            {
                node = list[0];
                list.RemoveAt(0);
                identifier = identifierList[0];
                identifierList.RemoveAt(0);
                
                

                baseLvalue = new AStructLvalue(new ALvalueExp(baseLvalue), new ADotDotType(new TDot(".")),
                                                identifier);
                if (node is AALocalDecl)
                {//Struct local
                    data.StructFieldLinks[(AStructLvalue) baseLvalue] = (AALocalDecl) node;
                }
                else if (node is APropertyDecl)
                {//Struct property
                    data.StructPropertyLinks[(AStructLvalue) baseLvalue] = (APropertyDecl) node;
                }
                //Don't link array length stuff
            }
            return baseLvalue;
        }
        /*
        private PLvalue Disambiguate(PName name)
        {
            ASimpleName aName = (ASimpleName)name;
            AABlock block = Util.GetAncestor<AABlock>(aName);
                
            while (block != null || Util.HasAncestor<AConstructorDecl>(aName))
            {//We might reference a local
                if (block != null && !data.Locals.ContainsKey(block)) data.Locals.Add(block, new List<AALocalDecl>());
                List<AALocalDecl> locals = new List<AALocalDecl>();
                if (block != null)
                    locals = data.Locals[block];
                else
                {
                    foreach (AALocalDecl formal in Util.GetAncestor<AConstructorDecl>(aName).GetFormals())
                    {
                        locals.Add(formal);
                    }
                }

                foreach (AALocalDecl local in locals)
                {
                    if (aName.GetIdentifier().Text == local.GetName().Text)
                    {
                        //First, the local must not be used before it's declared
                        PStm currentStm = Util.GetAncestor<PStm>(name);
                        PStm targetStm = Util.GetAncestor<PStm>(local);
                        if (currentStm is ASwitchCaseStm)
                            currentStm = (PStm)currentStm.Parent();
                        if (targetStm is ASwitchCaseStm)
                            targetStm = (PStm)targetStm.Parent();
                        //If its a parameter, there will be no parent stm, and then there is no problem)
                        if (targetStm != null)
                        { 
                            AABlock currentBlock = (AABlock) currentStm.Parent();
                            AABlock targetBlock = (AABlock)targetStm.Parent();
                            while (targetBlock != currentBlock)
                            {
                                if (Util.IsAncestor(targetStm, currentBlock))
                                {
                                    do
                                    {
                                        targetStm = Util.GetAncestor<PStm>(targetStm.Parent());
                                    } while (!(targetStm.Parent() is AABlock));
                                    targetBlock = (AABlock) targetStm.Parent();
                                    continue;
                                }
                                if (Util.IsAncestor(currentStm, targetBlock))
                                {
                                    do
                                    {
                                        currentStm = Util.GetAncestor<PStm>(currentStm.Parent());
                                    } while (!(currentStm.Parent() is AABlock));
                                    currentBlock = (AABlock)currentStm.Parent();
                                    continue;
                                }
                            }
                            if (currentBlock.GetStatements().IndexOf(currentStm) <= targetBlock.GetStatements().IndexOf(targetStm))
                            {
                                continue;
                            }
                        }

                        //We will replace with this, so might aswell add it to local_links
                        ALocalLvalue localLvalue = new ALocalLvalue(aName.GetIdentifier());
                        data.LocalLinks.Add(localLvalue, local);
                        
                        return localLvalue;
                    }
                }
                if (block == null)
                    break;
                block = Util.GetAncestor<AABlock>(block.Parent());
            }

            //Look for struct fields if inside a struct
            bool matchedStatic = false;
            AStructDecl str = Util.GetAncestor<AStructDecl>(name);
            if (str != null)
            {
                foreach (AALocalDecl local in data.StructFields[str])
                {
                    if (aName.GetIdentifier().Text == local.GetName().Text)
                    {
                        //If it's an enherited private variable, you can't refer to it.
                        if (local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                            data.EnheritanceLocalMap.ContainsKey(local))
                        {
                            continue;
                        }
                        if (local.GetStatic() != null)
                        {
                            //matchedStatic = true;
                            //continue;
                            ATypeLvalue typeLvalue = new ATypeLvalue(new TIdentifier(str.GetName().Text));
                            AStructLvalue structLvalue = new AStructLvalue(new ALvalueExp(typeLvalue), new ADotDotType(new TDot(".")), aName.GetIdentifier());
                            return structLvalue;
                        }

                        if (str.GetClassToken() == null && !Util.HasAncestor<AConstructorDecl>(name) && !Util.HasAncestor<ADeconstructorDecl>(name))
                        {
                            //We will replace with this, so might aswell add it to local_links
                            AStructFieldLvalue structFieldLvalue = new AStructFieldLvalue(aName.GetIdentifier());
                            data.StructMethodFieldLinks.Add(structFieldLvalue, local);
                            return structFieldLvalue;
                        }
                        else
                        {
                            AStructLvalue structLvalue =
                                new AStructLvalue(new ALvalueExp(new APointerLvalue(new TStar("*"), new ALvalueExp(new AThisLvalue(new TThis("this"))))),
                                                  new ADotDotType(new TDot(".")), aName.GetIdentifier());
                            return structLvalue;
                        }
                    }
                }

                //Might be a struct property
                foreach (APropertyDecl local in data.StructProperties[str])
                {
                    if (aName.GetIdentifier().Text == local.GetName().Text)
                    {
                        
                        //If it's an enherited private variable, you can't refer to it.
                        if (local.GetVisibilityModifier() is APrivateVisibilityModifier &&
                            Util.GetAncestor<AStructDecl>(local) != str)
                        {
                            continue;
                        }

                        if (local.GetStatic() != null)
                        {
                            //matchedStatic = true;
                            //continue;
                            ATypeLvalue typeLvalue = new ATypeLvalue(new TIdentifier(str.GetName().Text));
                            AStructLvalue structLvalue = new AStructLvalue(new ALvalueExp(typeLvalue), new ADotDotType(new TDot(".")), aName.GetIdentifier());
                            return structLvalue;
                        }

                        if (str.GetClassToken() == null && !Util.HasAncestor<AConstructorDecl>(name) && !Util.HasAncestor<ADeconstructorDecl>(name))
                        {
                            //We will replace with this, so might aswell add it to local_links
                            AStructFieldLvalue structFieldLvalue = new AStructFieldLvalue(aName.GetIdentifier());
                            data.StructMethodPropertyLinks.Add(structFieldLvalue, local);
                            return structFieldLvalue;
                        }
                        else
                        {
                            AStructLvalue structLvalue =
                                new AStructLvalue(new ALvalueExp(new APointerLvalue(new TStar("*"), new ALvalueExp(new AThisLvalue(new TThis("this"))))),
                                                  new ADotDotType(new TDot(".")), aName.GetIdentifier());
                            return structLvalue;
                        }
                    }
                }
            }

            //Look for fields
            foreach (SharedData.DeclItem<AFieldDecl> declItem in data.Fields)
            {
                AFieldDecl field = declItem.Decl;
                if (!Util.IsVisible(name, field))
                    continue;
                //Static fields must be referenced from same file.
                if ((field.GetStatic() != null) &&
                    Util.GetAncestor<AASourceFile>(field) != Util.GetAncestor<AASourceFile>(aName))
                    continue;
                //Private fields must be referenced from same namespace
                if (field.GetVisibilityModifier() is APrivateVisibilityModifier && 
                    !Util.IsSameNamespace(field, name))
                    continue;
                


                if (aName.GetIdentifier().Text == field.GetName().Text)
                {
                    //We will replace with this, so might aswell add it to local_links
                    AFieldLvalue fieldLvalue = new AFieldLvalue(aName.GetIdentifier());
                    data.FieldLinks.Add(fieldLvalue, field);
                    return fieldLvalue;
                }
            }

            //Look for properties
            foreach (SharedData.DeclItem<APropertyDecl> declItem in data.Properties)
            {
                APropertyDecl property = declItem.Decl;
                if (!Util.IsVisible(name, property))
                    continue;
                //Static fields must be referenced from same file.
                if ((property.GetStatic() != null) && 
                    Util.GetAncestor<AASourceFile>(property) != Util.GetAncestor<AASourceFile>(aName))
                    continue;
                //Private fields must be referenced from same namespace
                if (property.GetVisibilityModifier() is APrivateVisibilityModifier &&
                    !Util.IsSameNamespace(property, name))
                    continue;
                if (aName.GetIdentifier().Text == property.GetName().Text)
                {
                    //We will replace with this, so might aswell add it to local_links
                    APropertyLvalue propertyLvalue = new APropertyLvalue(aName.GetIdentifier());
                    data.PropertyLinks.Add(propertyLvalue, property);
                    return propertyLvalue;
                }
            }

            //Look in lib fields
            foreach (AFieldDecl field in data.Libraries.Fields)
            {
                if (aName.GetIdentifier().Text == field.GetName().Text)
                {
                    //We will replace with this, so might aswell add it to local_links
                    AFieldLvalue fieldLvalue = new AFieldLvalue(aName.GetIdentifier());
                    data.FieldLinks.Add(fieldLvalue, field);
                    return fieldLvalue;
                }
            }

            //Look for methods
            /*if (name.Parent().Parent() is ALvalueExp && (name.Parent().Parent().Parent() is AAssignmentExp || 
                                                         name.Parent().Parent().Parent() is AALocalDecl || 
                                                         name.Parent().Parent().Parent() is AFieldDecl))//Methods can only be on the right side of an assignment
            {

                AMethodLvalue methodLvalue = new AMethodLvalue(aName.GetIdentifier());
                foreach (SharedData.DeclItem<AMethodDecl> declItem in data.Methods)
                {
                    AMethodDecl method = declItem.Decl;
                    if (!Util.IsVisible(name, method))
                        continue;
                    //Static fields must be referenced from same file.
                    if (method.GetStatic() != null && Util.GetAncestor<AASourceFile>(method) != Util.GetAncestor<AASourceFile>(aName))
                        continue;
                    if (aName.GetIdentifier().Text == method.GetName().Text)
                    {
                        if (!data.MethodLvalueLinks.ContainsKey(methodLvalue))
                            data.MethodLvalueLinks.Add(methodLvalue, new List<AMethodDecl>());
                        data.MethodLvalueLinks[methodLvalue].Add(method);
                    }
                }
                if (data.MethodLvalueLinks.ContainsKey(methodLvalue))
                    return methodLvalue;
            }*//*

            if (name.Parent().Parent().Parent() is AStructLvalue || name.Parent().Parent().Parent() is ANonstaticInvokeExp)
            {
                bool matchesNamespace = false;
                bool matchesStaticType = GalaxyKeywords.Primitives.words.Contains(aName.GetIdentifier().Text);
                AAProgram program = Util.GetAncestor<AAProgram>(name);
                foreach (AASourceFile sourceFile in program.GetSourceFiles())
                {
                    if (sourceFile.GetNamespace() != null && sourceFile.GetNamespace().Text == aName.GetIdentifier().Text)
                    {
                        matchesNamespace = true;
                    }


                    if (Util.IsVisible(name, sourceFile))
                    {
                        foreach (AStructDecl s in sourceFile.GetDecl().OfType<AStructDecl>())
                        {
                            if (s.GetVisibilityModifier() is APrivateVisibilityModifier && !Util.IsSameNamespace(name, s))
                                continue;

                            if (s.GetName().Text == aName.GetIdentifier().Text)
                                matchesStaticType = true;
                        }
                    }
                }

                if (matchesStaticType)
                {
                    if (matchesNamespace)
                    {
                        return new AAmbiguousNameLvalue(name);
                    }
                    return new ATypeLvalue(aName.GetIdentifier());
                }
                if (matchesNamespace)
                {
                    return new ANamespaceLvalue(aName.GetIdentifier());
                }
            }

            /*
            //AmbigiousLvalue, LvalueExp, structlvalue/array/nonstaticInvoke
            //Might have been a namespace prefix
            if (name.Parent().Parent().Parent() is AStructLvalue && !(name.Parent().Parent().Parent().Parent() is ADelegateExp))
            {
                AStructLvalue structLvalue = (AStructLvalue) name.Parent().Parent().Parent();
                AAProgram program = Util.GetAncestor<AAProgram>(name);
                foreach (AASourceFile sourceFile in program.GetSourceFiles())
                {
                    if (sourceFile.GetNamespace() != null &&
                        sourceFile.GetNamespace().Text == aName.GetIdentifier().Text)
                    {
                        foreach (PDecl decl in sourceFile.GetDecl())
                        {
                            if (!(decl is AFieldDecl))
                                continue;
                            AFieldDecl field = (AFieldDecl)decl;
                            if (field.GetName().Text == structLvalue.GetName().Text)
                            {
                                //Static fields must be referenced from same file.
                                if ((field.GetStatic() != null || field.GetVisibilityModifier() is APrivateVisibilityModifier) &&
                                    Util.GetAncestor<AASourceFile>(field) != Util.GetAncestor<AASourceFile>(aName))
                                    continue;

                                AFieldLvalue fieldLvalue = new AFieldLvalue(structLvalue.GetName());
                                data.FieldLinks.Add(fieldLvalue, field);
                                structLvalue.ReplaceBy(fieldLvalue);
                                //fieldLvalue.Apply(this);
                                return null;
                            }
                        }
                    }
                }
            }
            if (name.Parent().Parent().Parent() is ANonstaticInvokeExp)
            {
                ANonstaticInvokeExp invoke = (ANonstaticInvokeExp) name.Parent().Parent().Parent();
                AAProgram program = Util.GetAncestor<AAProgram>(name);
                foreach (AASourceFile sourceFile in program.GetSourceFiles())
                {
                    if (sourceFile.GetNamespace() != null &&
                        sourceFile.GetNamespace().Text == aName.GetIdentifier().Text)
                    {
                        ASimpleInvokeExp simpleInvoke = new ASimpleInvokeExp(invoke.GetName(), new List<PExp>());
                        while (invoke.GetArgs().Count > 0)
                        {
                            simpleInvoke.GetArgs().Add(invoke.GetArgs()[0]);
                        }
                        data.SimpleNamespaceInvokes.Add(simpleInvoke, aName.GetIdentifier().Text);
                        invoke.ReplaceBy(simpleInvoke);
                        simpleInvoke.Apply(this);
                        return null;
                        
                    }
                }
            }
            //Static field
            if (name.Parent().Parent() is ALvalueExp &&
                name.Parent().Parent().Parent() is AStructLvalue)
            {
                AStructLvalue strLvalue = (AStructLvalue)name.Parent().Parent().Parent();
                foreach (AStructDecl s in data.Structs.Select(declItem => declItem.Decl))
                {
                    if (s.GetName().Text == aName.GetIdentifier().Text)
                    {
                        foreach (AALocalDecl structField in s.GetLocals().OfType<AALocalDecl>())
                        {
                            if (structField.GetName().Text == strLvalue.GetName().Text && !data.EnheritanceLocalMap.ContainsKey(structField))
                            {
                                if (structField.GetStatic() == null)
                                {
                                    errors.Add(new ErrorCollection.Error(strLvalue.GetName(),
                                                                         "The struct field is not marked as static.",
                                                                         false,
                                                                         new ErrorCollection.Error(
                                                                             structField.GetName(), "Matching field")));
                                    throw new ParserException(null, "");
                                }
                                AStructFieldLvalue structFieldLvalue = new AStructFieldLvalue(new TIdentifier("renameMe"));
                                data.StructMethodFieldLinks[structFieldLvalue] = structField;
                                strLvalue.ReplaceBy(structFieldLvalue);
                                return null;
                            }
                        }
                    }
                }
            }
            //name.Parent is AAmbigiousLvalue
            //PP is lvalueExp
            //PPP is AStructLvalue
            if ( name.Parent().Parent() is ALvalueExp &&
                 name.Parent().Parent().Parent() is AStructLvalue &&
                 name.Parent().Parent().Parent().Parent() is ADelegateExp)
                return new ANamespaceLvalue(aName.GetIdentifier());
            *//*

            if (matchedStatic)
            {
                errors.Add(new ErrorCollection.Error(aName.GetIdentifier(), "To reference a static field/property, you must type " + str.GetName().Text + "." + aName.GetIdentifier().Text));
            }
            else 
                errors.Add(new ErrorCollection.Error(aName.GetIdentifier(), currentSourceFile, aName.GetIdentifier().Text + " did not match any defined methods, fields, locals or namespaces"), true);
            return null;
        }*/

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (node.GetDelegate() != null)
            {
                node.Parent().RemoveChild(node);
            }

            base.CaseAMethodDecl(node);
        }

    }
}
