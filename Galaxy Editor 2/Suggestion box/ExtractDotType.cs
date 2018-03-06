using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Suggestion_box
{
    class ExtractDotType
    {
        internal class ReturnData
        {
            public class ContextPair<T>
            {
                public T Type;
                public IDeclContainer Context;

                public ContextPair(T type, IDeclContainer context)
                {
                    Type = type;
                    Context = context;
                }
            }


            public List<ContextPair<PType>> Types = new List<ContextPair<PType>>();
            public List<ContextPair<NamespaceDescription>> Namespaces = new List<ContextPair<NamespaceDescription>>();
            public List<ContextPair<PType>> StaticTypes = new List<ContextPair<PType>>();
            public bool Error {get { return ErrorOverrite == null ? Types.Count == 0 && Namespaces.Count == 0 && StaticTypes.Count == 0 : (bool)ErrorOverrite; }}
            public bool? ErrorOverrite;

        /*    public ReturnData(PType type)
            {
                Types.Add(type);
            }*/


            public ReturnData()
            {
            }

            public void Clear()
            {
                Types.Clear();
                Namespaces.Clear();
                StaticTypes.Clear();
                ErrorOverrite = false;
            }

          /*  public override string ToString()
            {
                if (Error)
                    return "Error";
                if (Type != null)
                    return Util.TypeToString(Type);
                string ret = "";
                if (Namespace != null)
                    ret += Namespace.Signature;
                if (StaticType != null)
                    ret += "Static: " + Util.TypeToString(StaticType);
                return ret;
            }*/
        }

        private static GalaxyCompiler Compiler;
        private static MyEditor CurrentEditor;


        public static ReturnData GetType(string text, 
            GalaxyCompiler compiler, 
            IDeclContainer initialContext, 
            MyEditor currentEditor, 
            out bool IsGlobal)
        {
            Compiler = compiler;
            CurrentEditor = currentEditor;
            IsGlobal = false;



            /*{
                //Remove last . or ->
                int i = text.LastIndexOf(".");
                int j = text.LastIndexOf("->");
                if (j > i)
                    i = j;
                text = text.Remove(i, text.Length - i);
            }*/

            //Make tokens
            List<Token> tokens = new List<Token>();
            Lexer lexer = new Lexer(new StringReader(text));
            //We can be sure that the text ends on . identifier?
            Token token = lexer.Next();
            List<Token> openParams = new List<Token>();
            int declNr = 0;
            while (!(token is EOF))
            {
                //Remove comments and ws
                if (token is TEndOfLineComment || token is TTraditionalComment || token is TWhiteSpace)
                {
                    token = lexer.Next();
                    continue;
                }
                tokens.Add(token);
                if (token is TLBrace)
                    openParams.Add(token);

                if (token is TRBrace)
                {
                    if (openParams.Count > 0)
                        openParams.RemoveAt(openParams.Count - 1);
                    if (openParams.Count == 0)
                    {
                        //remove previous decls
                        tokens.Clear();
                        declNr++;
                    }
                }
                if (token is TSemicolon && openParams.Count == 0)
                {
                    declNr++;
                    tokens.Clear();
                }
                if (token is TNamespace || token is TUsing)
                    declNr++;
                token = lexer.Next();
            }


            List<Token> expTokens = new List<Token>();
            {
                
                int openBrackets = 0;
                int openParens = 0;
                int openZigs = 0;
                for (int i = tokens.Count - 1; i >= 0; i--)
                {
                    token = tokens[i];
                    if (token is TGt)
                    {
                        if (expTokens.Count > 0 && !(expTokens.Count == 1 && (expTokens[0] is TDot || expTokens[0] is TArrow)))
                            break;
                        openZigs++;
                        expTokens.Insert(0, token);
                    }
                    else if (token is TLt)
                    {
                        if (openZigs == 0)
                            break;
                        openZigs--;
                        expTokens.Insert(0, token);
                    }
                    else if (openZigs > 0)
                    {
                        expTokens.Insert(0, token);
                    }
                    else if (token is TRBracket)
                    {
                        openBrackets++;
                        if (openBrackets == 1)
                        {
                            expTokens.Insert(0, token);
                            expTokens.Insert(0, new TIntegerLiteral("0"));
                        }
                    }
                    else if (token is TLBracket)
                    {
                        if (openBrackets == 0)
                        {
                            break;
                        }
                        if (openBrackets == 1)
                            expTokens.Insert(0, token);
                        openBrackets--;
                    }
                    else if (openBrackets != 0)
                    {
                        continue;
                    }
                    else if (token is TRParen)
                    {
                        openParens++;
                        expTokens.Insert(0, token);
                    }
                    else if (token is TLParen)
                    {
                        openParens--;
                        if (openParens >= 0)
                            expTokens.Insert(0, token);
                        if (openParens <= 0)
                        {

                            if (openParens == 0)
                            {
                                if (tokens[i - 1] is TIdentifier)
                                    continue;
                                if (tokens[i - 1] is TIf || tokens[i - 1] is TWhile)
                                {
                                    //We took the param too much. Remove it and break
                                    int parens = 0;
                                    for (int j = 0; j < expTokens.Count; j++)
                                    {
                                        if (expTokens[j] is TLParen)
                                            parens++;
                                        if (expTokens[j] is TRParen)
                                        {
                                            parens--;
                                            if (parens == 0)
                                            {
                                                expTokens.RemoveRange(0, j + 1);
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    else if (openParens > 0)
                    {
                        expTokens.Insert(0, token);
                    }
                    else if (token is TIdentifier || token is TDot || token is TArrow || 
                        token is TFalse || token is TTrue ||token is TIntegerLiteral || 
                        token is TFixedLiteral || token is THexLiteral || token is TOctalLiteral ||
                        token is TValue || token is TStruct || token is TEscapeGlobal || token is TThis)
                    {
                        if (token is TIdentifier && expTokens.Count > 0 && expTokens[0] is TIdentifier)
                            break;

                        expTokens.Insert(0, token);
                    }
                    else if (token is TStar)
                    {//Add it if the next token is invalid or another TStar
                        Token nextToken = tokens[i - 1];
                        if (nextToken is TStar || nextToken is TLParen ||
                            !(nextToken is TIdentifier || nextToken is TDot || nextToken is TArrow ||
                                nextToken is TFalse || nextToken is TTrue || nextToken is TIntegerLiteral ||
                                nextToken is TFixedLiteral || nextToken is THexLiteral || nextToken is TOctalLiteral ||
                                nextToken is TValue || nextToken is TStruct || nextToken is TEscapeGlobal || token is TThis ||
                                nextToken is TRParen || nextToken is TRBracket))
                        {
                            expTokens.Insert(0, token);
                        }
                        else
                        {
                            break;
                        }
                    }
                     
                    else
                    {
                        break;
                    }
                }
                tokens.Clear();
                tokens.AddRange(expTokens);
            }

            if  (tokens.Count == 2 && tokens[1] is TDot ||
                tokens.Count == 3 && tokens[1] is TDot && tokens[2] is TIdentifier)
            {
                if (tokens[0] is TEscapeGlobal)
                {
                    IsGlobal = true;
                    return new ReturnData(){ErrorOverrite = false};
                }
                if (tokens[0] is TStruct)
                {
                    //Return the type of current struct
                    foreach (StructDescription str in initialContext.Structs)
                    {
                        if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                            str.LineTo >= CurrentEditor.caret.Position.Line)
                        {
                            ReturnData returner = new ReturnData();
                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier(str.Name), null), initialContext));
                            return returner;
                        }
                    }
                    return new ReturnData();
                }
            }

            try
            {
                //Parse as expression
                tokens.Insert(0, new TAssign("="));
                tokens.Insert(0, new TIdentifier("foo"));
                tokens.Insert(0, new TIdentifier("int"));
                if (!(tokens[tokens.Count - 1] is TIdentifier))
                    tokens.Add(new TIdentifier("identifier"));
                tokens.Add(new TSemicolon(";"));

                AFieldDecl field = (AFieldDecl)Parse(tokens);

                PExp exp = field.GetInit();

                while (exp is ALvalueExp && ((ALvalueExp)exp).GetLvalue() is APointerLvalue || exp is ACastExp)
                {
                    if (exp is ACastExp)
                        exp = ((ACastExp) exp).GetExp();
                    else
                        exp = ((APointerLvalue)((ALvalueExp)exp).GetLvalue()).GetBase();
                }

                //Remove .identifer
                PLvalue lvalue = ((ALvalueExp) exp).GetLvalue();
                if (lvalue is AStructLvalue)
                    exp = ((AStructLvalue)lvalue).GetReceiver();
                else
                {
                    AAmbiguousNameLvalue aLvalue = (AAmbiguousNameLvalue)lvalue;
                    AAName name = (AAName)aLvalue.GetAmbiguous();
                    name.GetIdentifier().RemoveAt(name.GetIdentifier().Count - 1);
                }

                TypeParser parser = new TypeParser(currentEditor, initialContext);
                exp.Apply(parser);
                return parser.Data;
               /* ReturnData data = GetType(exp);

                //Form1.Form.Text = data.ToString();
                return data;*/

            }
            catch (ParserException err)
            {
                //Try parsing as type
                tokens = expTokens;
                if (tokens[tokens.Count - 1] is TDot || tokens[tokens.Count - 1] is TArrow)
                    tokens.RemoveAt(tokens.Count - 1);
                tokens.Add(new TIdentifier("foo"));
                tokens.Add(new TSemicolon(";"));
                AFieldDecl field = (AFieldDecl)Parse(tokens);

                ReturnData retData = new ReturnData();
                retData.StaticTypes.Add(new ReturnData.ContextPair<PType>(field.GetType(), initialContext));
                return retData;
            }
        }
        
        private class TypeParser : DepthFirstAdapter
        {
            public ReturnData Data = new ReturnData();
            private IDeclContainer initialContext;
            private StructDescription currentStruct;
            private EnrichmentDescription currentEnrichment;
            private MethodDescription currentMethod;

            public TypeParser(MyEditor currentEditor, IDeclContainer initialContext)
            {
                int line = currentEditor.caret.Position.Line;
                this.initialContext = initialContext;
                
                foreach (StructDescription str in initialContext.Structs)
                {
                    if (str.LineFrom <= line && str.LineTo >= line)
                    {
                        currentStruct = str;
                        break;
                    }
                }
                foreach (EnrichmentDescription enrichment in initialContext.Enrichments)
                {
                    if (enrichment.LineFrom <= line && enrichment.LineTo >= line)
                    {
                        currentEnrichment = enrichment;
                        break;
                    }
                }
                if (currentStruct != null)
                {
                    foreach (MethodDescription method in currentStruct.Methods)
                    {
                        if (method.Start.Line <= line && method.End.Line >= line)
                        {
                            currentMethod = method;
                            break;
                        }
                    }
                }
                else if (currentEnrichment != null)
                {
                    foreach (MethodDescription method in currentEnrichment.Methods)
                    {
                        if (method.Start.Line <= line && method.End.Line >= line)
                        {
                            currentMethod = method;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (MethodDescription method in initialContext.Methods)
                    {
                        if (method.Start.Line <= line && method.End.Line >= line)
                        {
                            currentMethod = method;
                            break;
                        }
                    }
                }
            }

            public override void CaseABinopExp(ABinopExp node)
            {
                PBinop binop = node.GetBinop();
                if (binop is AEqBinop || binop is ANeBinop || binop is AGtBinop || binop is AGeBinop || binop is ALtBinop || binop is ALeBinop || binop is ALazyAndBinop
                    || binop is ALazyOrBinop)
                {
                    Data.Clear();
                    Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("bool"), null), initialContext));
                    return;
                }
                node.GetLeft().Apply(this);
                ReturnData leftData = Data;
                Data = new ReturnData();
                node.GetRight().Apply(this);
                string[] keeperTypes = new string[0];
                if (binop is APlusBinop)
                {
                    keeperTypes = new[] {"int", "fixed", "string", "text", "byte", "point"};
                }
                else if (binop is AMinusBinop || binop is ADivideBinop || binop is ATimesBinop || binop is AModuloBinop)
                {
                    keeperTypes = new[] {"int", "fixed", "byte", "point"};
                }
                else if (binop is AAndBinop || binop is AOrBinop || binop is AXorBinop || binop is ALBitShiftBinop || binop is ARBitShiftBinop)
                {
                    keeperTypes = new[] { "int", "byte"};
                }
                Data.Namespaces.Clear();
                Data.StaticTypes.Clear();
                for (int i = 0; i < Data.Types.Count; i++)
                {
                    PType type = Data.Types[i].Type;
                    bool keeper = false;
                    if (type is ANamedType && ((ANamedType)type).IsPrimitive(keeperTypes))
                    {
                        foreach (ReturnData.ContextPair<PType> contextPair in leftData.Types)
                        {
                            if (contextPair.Type is ANamedType && ((ANamedType)contextPair.Type).IsPrimitive())
                            {
                                if (((ANamedType)type).AsString() == ((ANamedType)contextPair.Type).AsString())
                                {
                                    keeper = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!keeper)
                    {
                        Data.Types.RemoveAt(i);
                        i--;
                    }
                }
            }

            public override void CaseAUnopExp(AUnopExp node)
            {
                PUnop unop = node.GetUnop();
                node.GetUnop().Apply(this);
                string[] keeperTypes = new string[0];
                if (unop is ANegateUnop)
                {
                    keeperTypes = new[] { "int", "fixed", "byte"};
                }
                else if (unop is AComplementUnop)
                {
                    keeperTypes = new[] { "int", "byte", "bool", "point", "order", "string" };
                }
                Data.Namespaces.Clear();
                Data.StaticTypes.Clear();
                for (int i = 0; i < Data.Types.Count; i++)
                {
                    PType type = Data.Types[i].Type;
                    bool keeper = false;
                    if (type is ANamedType && ((ANamedType)type).IsPrimitive(keeperTypes))
                    {
                        keeper = true;
                    }
                    if (!keeper)
                    {
                        Data.Types.RemoveAt(i);
                        i--;
                    }
                }
            }

            public override void CaseAIncDecExp(AIncDecExp node)
            {
                node.GetLvalue().Apply(this);
                string[] keeperTypes = new[] { "int", "fixed", "byte" };
                Data.Namespaces.Clear();
                Data.StaticTypes.Clear();
                for (int i = 0; i < Data.Types.Count; i++)
                {
                    PType type = Data.Types[i].Type;
                    bool keeper = false;
                    if (type is ANamedType && ((ANamedType)type).IsPrimitive(keeperTypes))
                    {
                        keeper = true;
                    }
                    if (!keeper)
                    {
                        Data.Types.RemoveAt(i);
                        i--;
                    }
                }
            }

            public override void CaseAFixedConstExp(AFixedConstExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("fixed"), null), initialContext));
            }

            public override void CaseAStringConstExp(AStringConstExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
            }

            public override void CaseACharConstExp(ACharConstExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("char"), null), initialContext));
            }

            public override void CaseABooleanConstExp(ABooleanConstExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("bool"), null), initialContext));
            }

            public override void CaseANullExp(ANullExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("null"), null), initialContext));
            }

            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                //Find method that matches name and param count.
                List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                foreach (IDeclContainer visibleDecl in visibleDecls)
                {
                    foreach (MethodDescription method in visibleDecl.Methods)
                    {
                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count)
                        {
                            Data.Types.Add(new ReturnData.ContextPair<PType>(method.realType, visibleDecl));
                        }
                    }
                }
                foreach (AMethodDecl method in Compiler.libraryData.Methods)
                {
                    if (method.GetName().Text == node.GetName().Text && method.GetFormals().Count == node.GetArgs().Count)
                    {
                        Data.Types.Add(new ReturnData.ContextPair<PType>(method.GetReturnType(), initialContext.File));
                    }
                }
            }

            public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
            {
                node.GetReceiver().Apply(this);
                ReturnData returner = new ReturnData();
                foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ANamedType)
                    {
                        List<string> list = ((AAName) ((ANamedType) type).GetName()).ToStringList();
                        string name = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);


                        List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list); 
                        
                        foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                        {
                            if (!visibleDecls.Contains(container))
                                visibleDecls.Add(container);
                        }


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            //if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (MethodDescription method in enrichment.Methods)
                                        {
                                            if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && !method.IsStatic)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (list.Count > 0)
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                        else
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (MethodDescription method in str.Methods)
                                    {
                                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && !method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }
                        
                        
                        /*List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (MethodDescription method in str.Methods)
                                    {
                                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && !method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                        }
                                    }
                                }
                            }
                            if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (MethodDescription method in enrichment.Methods)
                                        {
                                            if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && !method.IsStatic)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }*/
                    }
                    else
                    {
                        List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, context, declContainer))
                                {
                                    foreach (MethodDescription method in enrichment.Methods)
                                    {
                                        if (method.Name == node.GetName().Text &&
                                            method.Formals.Count == node.GetArgs().Count && !method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType,
                                                                                                 declContainer));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Static methods
                foreach (ReturnData.ContextPair<PType> pair in Data.StaticTypes)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ANamedType)
                    {
                        List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                        string name = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);



                        List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);

                        foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                        {
                            if (!visibleDecls.Contains(container))
                                visibleDecls.Add(container);
                        }


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            //if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (MethodDescription method in enrichment.Methods)
                                        {
                                            if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && method.IsStatic)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (list.Count > 0)
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                        else
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (MethodDescription method in str.Methods)
                                    {
                                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }
                        
                        /*List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (MethodDescription method in str.Methods)
                                    {
                                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                        }
                                    }
                                }
                            }
                            if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (MethodDescription method in enrichment.Methods)
                                        {
                                            if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count && method.IsStatic)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }*/
                    }
                    else
                    {
                        List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, context, declContainer))
                                {
                                    foreach (MethodDescription method in enrichment.Methods)
                                    {
                                        if (method.Name == node.GetName().Text &&
                                            method.Formals.Count == node.GetArgs().Count && method.IsStatic)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType,
                                                                                                 declContainer));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Global methods
                foreach (ReturnData.ContextPair<NamespaceDescription> pair in Data.Namespaces)
                {
                    NamespaceDescription ns = pair.Type;
                    IDeclContainer context = pair.Context;
                    foreach (MethodDescription method in ns.Methods)
                    {
                        if (method.Name == node.GetName().Text && method.Formals.Count == node.GetArgs().Count)
                        {
                            returner.Types.Add(new ReturnData.ContextPair<PType>(method.realType, ns));
                        }
                    }
                }
                Data = returner;
            }

            public override void CaseASyncInvokeExp(ASyncInvokeExp node)
            {
                //Replace by nonstatic or simple invoke, and let that code do it.
                TIdentifier name;
                PExp baseExp = null;
                if (node.GetName() is AAmbiguousNameLvalue)
                {
                    AAmbiguousNameLvalue lvalue = (AAmbiguousNameLvalue) node.GetName();
                    AAName aName = (AAName) lvalue.GetAmbiguous();
                    name = (TIdentifier) aName.GetIdentifier()[aName.GetIdentifier().Count - 1];
                    aName.GetIdentifier().RemoveAt(aName.GetIdentifier().Count - 1);
                    if (aName.GetIdentifier().Count > 0)
                        baseExp = new ALvalueExp(lvalue);
                }
                else //node.getName() is AStructLvalue
                {
                    AStructLvalue lvalue = (AStructLvalue) node.GetName();
                    name = lvalue.GetName();
                    baseExp = lvalue.GetReceiver();
                }
                if (baseExp == null)
                {
                    ASimpleInvokeExp replacer = new ASimpleInvokeExp(name, new ArrayList());
                    while (node.GetArgs().Count > 0)
                        replacer.GetArgs().Add(node.GetArgs()[0]);
                    CaseASimpleInvokeExp(replacer);
                }
                else
                {
                    ANonstaticInvokeExp replacer = new ANonstaticInvokeExp(baseExp, new ADotDotType(new TDot(".")), name, new ArrayList());
                    while (node.GetArgs().Count > 0)
                        replacer.GetArgs().Add(node.GetArgs()[0]);
                    CaseANonstaticInvokeExp(replacer);
                }
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                node.GetLvalue().Apply(this);
            }

            public override void CaseACastExp(ACastExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(node.GetType(), initialContext));
            }

            public override void CaseASharpCastExp(ASharpCastExp node)
            {
                if (node.GetType() != null)
                    Data.Types.Add(new ReturnData.ContextPair<PType>(node.GetType(), initialContext));
                else
                {
                    ReturnData returner = new ReturnData();
                    node.GetExp().Apply(this);
                    bool addedString = false;
                    bool addedInt = false;
                    foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                    {
                        PType type = pair.Type;
                        IDeclContainer context = pair.Context;
                        if (type is ANamedType)
                        {
                            List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                            string name = list[list.Count - 1];
                            list.RemoveAt(list.Count - 1);

                            List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                            if (list.Count > 0)
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);

                            foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                            {
                                if (!visibleDecls.Contains(container))
                                    visibleDecls.Add(container);
                            }


                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                //if (list.Count == 0)
                                {
                                    foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                    {
                                        if (TypesEqual(type, enrichment.type, context, declContainer))
                                        {
                                            if (enrichment.IsClass && !addedInt)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("int"), null), initialContext));
                                                addedInt = true;
                                            }
                                            if (!enrichment.IsClass && !addedString)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
                                                addedString = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (list.Count > 0)
                                visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                            else
                                visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription str in declContainer.Structs)
                                {
                                    if (str.Name == name)
                                    {
                                        if (str.IsClass && !addedInt)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("int"), null), initialContext));
                                            addedInt = true;
                                        }
                                        if (!str.IsClass && !addedString)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
                                            addedString = true;
                                        }
                                    }
                                }
                            }
                            
                            
                            /*List<IDeclContainer> visibleDecls;
                            if (list.Count > 0)
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                            else
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription str in declContainer.Structs)
                                {
                                    if (str.Name == name)
                                    {
                                        if (str.IsClass && !addedInt)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("int"), null), initialContext));
                                            addedInt = true;
                                        }
                                        if (!str.IsClass && !addedString)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
                                            addedString = true;
                                        }
                                    }
                                }
                                if (list.Count == 0)
                                {
                                    foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                    {
                                        if (TypesEqual(type, enrichment.type, context, declContainer))
                                        {
                                            if (enrichment.IsClass && !addedInt)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("int"), null), initialContext));
                                                addedInt = true;
                                            }
                                            if (!enrichment.IsClass && !addedString)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
                                                addedString = true;
                                            }
                                        }
                                    }
                                }
                            }*/
                        }
                        else
                        {
                            List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        if (enrichment.IsClass && !addedInt)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("int"), null), initialContext));
                                            addedInt = true;
                                        }
                                        if (!enrichment.IsClass && !addedString)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier("string"), null), initialContext));
                                            addedString = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Data = returner;
                }
            }

            public override void CaseANewExp(ANewExp node)
            {
                Data.Types.Add(new ReturnData.ContextPair<PType>(node.GetType(), initialContext));
            }

            public override void CaseADelegateExp(ADelegateExp node)
            {
                
            }

            public override void CaseADelegateInvokeExp(ADelegateInvokeExp node)
            {
                
            }

            public override void CaseAIfExp(AIfExp node)
            {
                node.GetThen().Apply(this);
                ReturnData retData = Data;
                Data = new ReturnData();
                node.GetElse().Apply(this);
                Data.Types.AddRange(retData.Types);
                Data.StaticTypes.AddRange(retData.StaticTypes);
                Data.Namespaces.AddRange(retData.Namespaces);
            }

            public override void CaseAArrayResizeExp(AArrayResizeExp node)
            {
                //Void type
            }

            public override void CaseAFieldLvalue(AFieldLvalue node)
            {
                List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                foreach (IDeclContainer visibleDecl in visibleDecls)
                {
                    foreach (VariableDescription field in visibleDecl.Fields)
                    {
                        if (field.Name == node.GetName().Text)
                        {
                            Data.Types.Add(new ReturnData.ContextPair<PType>(field.realType, visibleDecl));
                        }
                    }
                }
            }

            public override void CaseAStructFieldLvalue(AStructFieldLvalue node)
            {
                if (currentStruct != null)
                {
                    foreach (VariableDescription field in currentStruct.Fields)
                    {
                        if (field.Name == node.GetName().Text)
                        {
                            Data.Types.Add(new ReturnData.ContextPair<PType>(field.realType, initialContext));
                        }
                    }
                }
            }

            public override void CaseAStructLvalue(AStructLvalue node)
            {
                node.GetReceiver().Apply(this);
                ReturnData returner = new ReturnData();
                foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ANamedType)
                    {
                        List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                        string name = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);


                        List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);

                        foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                        {
                            if (!visibleDecls.Contains(container))
                                visibleDecls.Add(container);
                        }

                       
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            //if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, context))
                                    {
                                        foreach (VariableDescription field in enrichment.Fields)
                                        {
                                            if (field.Name == node.GetName().Text)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (list.Count > 0)
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                        else
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }



                        /*
                        List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                        //foreach (IDeclContainer declContainer in visibleDecls)
                      //  {
                            foreach (StructDescription str in context.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, context));
                                        }
                                    }
                                }
                            }
                            if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in context.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, context))
                                    {
                                        foreach (VariableDescription field in enrichment.Fields)
                                        {
                                            if (field.Name == node.GetName().Text)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, context));
                                            }
                                        }
                                    }
                     //           }
                            }
                        }*/
                    }
                    else
                    {
                        List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, context, declContainer))
                                {
                                    foreach (VariableDescription field in enrichment.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Static fields
                foreach (ReturnData.ContextPair<PType> pair in Data.StaticTypes)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ANamedType)
                    {
                        List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                        string name = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);


                        List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);

                        foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                        {
                            if (!visibleDecls.Contains(container))
                                visibleDecls.Add(container);
                        }


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            //if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, context))
                                    {
                                        foreach (VariableDescription field in enrichment.Fields)
                                        {
                                            if (field.Name == node.GetName().Text)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (list.Count > 0)
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                        else
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }
                        
                        /*List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == name)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                            if (list.Count == 0)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (VariableDescription field in enrichment.Fields)
                                        {
                                            if (field.Name == node.GetName().Text)
                                            {
                                                returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }*/
                    }
                    else
                    {
                        List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, context, declContainer))
                                {
                                    foreach (VariableDescription field in enrichment.Fields)
                                    {
                                        if (field.Name == node.GetName().Text)
                                        {
                                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Global methods
                foreach (ReturnData.ContextPair<NamespaceDescription> pair in Data.Namespaces)
                {
                    NamespaceDescription ns = pair.Type;
                    IDeclContainer context = pair.Context;
                    foreach (VariableDescription field in ns.Fields)
                    {
                        if (field.Name == node.GetName().Text)
                        {
                            returner.Types.Add(new ReturnData.ContextPair<PType>(field.realType, ns));
                        }
                    }
                    foreach (NamespaceDescription ns2 in ns.Namespaces)
                    {
                        if (ns2.Name == node.GetName().Text)
                        {
                            returner.Namespaces.Add(new ReturnData.ContextPair<NamespaceDescription>(ns2, ns2));
                        }
                    }
                }
                Data = returner;
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                node.GetBase().Apply(this);
                ReturnData returner = new ReturnData();
                foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is AArrayTempType)
                    {
                        AArrayTempType aType = (AArrayTempType) type;
                        returner.Types.Add(new ReturnData.ContextPair<PType>(aType.GetType(), context));
                    }
                    else if (type is ADynamicArrayType)
                    {
                        ADynamicArrayType aType = (ADynamicArrayType)type;
                        returner.Types.Add(new ReturnData.ContextPair<PType>(aType.GetType(), context));
                    }
                    else if (type is APointerType)
                    {
                        //Implicit conversion for a[] to (*a)[]
                        APointerType aType = (APointerType)type;
                        if (aType.GetType() is AArrayTempType)
                        {
                            AArrayTempType aaType = (AArrayTempType)aType.GetType();
                            returner.Types.Add(new ReturnData.ContextPair<PType>(aaType.GetType(), context));
                        }
                        else if (aType.GetType() is ADynamicArrayType)
                        {
                            ADynamicArrayType aaType = (ADynamicArrayType)aType.GetType();
                            returner.Types.Add(new ReturnData.ContextPair<PType>(aaType.GetType(), context));
                        }
                    }
                    List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                    foreach (IDeclContainer declContainer in visibleDecls)
                    {
                        foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                        {
                            if (TypesEqual(type, enrichment.type, context, initialContext))
                            {
                                foreach (VariableDescription prop in enrichment.Fields)
                                {
                                    if (prop.IsArrayProperty)
                                        returner.Types.Add(new ReturnData.ContextPair<PType>(prop.realType, declContainer));
                                }
                            }
                        }
                    }
                }
                Data = returner;
            }

            public override void CaseAPointerLvalue(APointerLvalue node)
            {
                node.GetBase().Apply(this);
                ReturnData returner = new ReturnData();
                foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is APointerType)
                    {
                        APointerType aType = (APointerType)type;
                        returner.Types.Add(new ReturnData.ContextPair<PType>(aType.GetType(), context));
                    }
                   /* else if (type is ADynamicArrayType)
                    {
                        //ADynamicArrayType aType = (ADynamicArrayType)type;
                       // AArrayTempType newType = new AArrayTempType(new TLBracket("["), aType.GetType(), new AIntConstExp(new TIntegerLiteral("42")), new TIntegerLiteral("42"));
                        returner.Types.Add(new ReturnData.ContextPair<PType>(type, context));
                    }*/
                }
                Data = returner;
            }

            public override void CaseAAmbiguousNameLvalue(AAmbiguousNameLvalue node)
            {
                AAName aName = (AAName) node.GetAmbiguous();
                List<string> nameList = aName.ToStringList();
                string name = nameList[0];
                nameList.RemoveAt(0);
                //if (nameList.Count == 0)
                {
                    //Look for namespaces
                    List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, false);
                    foreach (IDeclContainer visibleDecl in visibleDecls)
                    {
                        foreach (NamespaceDescription ns in visibleDecl.Namespaces)
                        {
                            if (ns.Name == name)
                                Data.Namespaces.Add(new ReturnData.ContextPair<NamespaceDescription>(ns, initialContext));
                        }
                    }
                    //Look for locals in current method)
                    if (currentMethod != null)
                    {
                        foreach (VariableDescription local in currentMethod.Locals)
                        {
                            if (local.Name == name)
                                Data.Types.Add(new ReturnData.ContextPair<PType>(local.realType, initialContext));
                        }
                        foreach (VariableDescription local in currentMethod.Formals)
                        {
                            if (local.Name == name)
                                Data.Types.Add(new ReturnData.ContextPair<PType>(local.realType, initialContext));
                        }
                    }
                    //Look for fields in current struct
                    if (currentStruct != null)
                    {
                        foreach (VariableDescription field in currentStruct.Fields)
                        {
                            if (field.Name == name)
                                Data.Types.Add(new ReturnData.ContextPair<PType>(field.realType, initialContext));
                        }
                    }
                    //Look for visible global fields and types
                    visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                    foreach (IDeclContainer visibleDecl in visibleDecls)
                    {
                        foreach (VariableDescription field in visibleDecl.Fields)
                        {
                            if (field.Name == name)
                                Data.Types.Add(new ReturnData.ContextPair<PType>(field.realType, visibleDecl));
                        }
                        foreach (StructDescription str in visibleDecl.Structs)
                        {
                            if (str.Name == name)
                            {
                                List<string> identifierList = visibleDecl.NamespaceList;
                                identifierList.Add(name);
                                AAName n = new AAName();
                                foreach (string s in identifierList)
                                {
                                    n.GetIdentifier().Add(new TIdentifier(s));
                                }
                                ANamedType namedType = new ANamedType(n);
                                Data.StaticTypes.Add(new ReturnData.ContextPair<PType>(namedType, visibleDecl));
                            }
                        }
                    }
                }
                while (nameList.Count > 0)
                {
                    //Look for namespaces
                    name = nameList[0];
                    nameList.RemoveAt(0);
                    ReturnData nextData = new ReturnData();
                    foreach (ReturnData.ContextPair<PType> pair in Data.Types)
                    {
                        PType type = pair.Type;
                        IDeclContainer context = pair.Context;
                        if (type is ANamedType)
                        {
                            //Look for a struct, and nonstatic fields in it. Otherwise enrichments
                            List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                            string strName = list[list.Count - 1];
                            list.RemoveAt(list.Count - 1);


                            List<IDeclContainer> visibleDecls = new List<IDeclContainer>();
                            if (list.Count > 0)
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);

                            foreach (IDeclContainer container in initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true))
                            {
                                if (!visibleDecls.Contains(container))
                                    visibleDecls.Add(container);
                            }


                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                //if (list.Count == 0)
                                {
                                    foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                    {
                                        if (TypesEqual(type, enrichment.type, context, declContainer))
                                        {
                                            foreach (VariableDescription field in enrichment.Fields)
                                            {
                                                if (field.Name == name && !field.IsStatic)
                                                {
                                                    nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (list.Count > 0)
                                visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                            else
                                visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription str in declContainer.Structs)
                                {
                                    if (str.Name == strName)
                                    {
                                        foreach (VariableDescription field in str.Fields)
                                        {
                                            if (field.Name == name && !field.IsStatic)
                                            {
                                                nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                            
                            /*List<IDeclContainer> visibleDecls;
                            if (list.Count > 0)
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                            else
                                visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription str in declContainer.Structs)
                                {
                                    if (str.Name == strName)
                                    {
                                        foreach (VariableDescription field in str.Fields)
                                        {
                                            if (field.Name == name && !field.IsStatic)
                                            {
                                                nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                                if (list.Count == 0)
                                {
                                    foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                    {
                                        if (TypesEqual(type, enrichment.type, context, declContainer))
                                        {
                                            foreach (VariableDescription field in enrichment.Fields)
                                            {
                                                if (field.Name == name && !field.IsStatic)
                                                {
                                                    nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                                }
                                            }
                                        }
                                    }
                                }
                            }*/
                        }
                        else
                        {
                            //Look for enrichments
                            List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichment.type, context, declContainer))
                                    {
                                        foreach (VariableDescription field in enrichment.Fields)
                                        {
                                            if (field.Name == name && !field.IsStatic)
                                            {
                                                nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (ReturnData.ContextPair<PType> pair in Data.StaticTypes)
                    {
                        ANamedType type = (ANamedType) pair.Type;
                        IDeclContainer context = pair.Context;
                        //Look for a struct, and static fields in it
                        //Look for a struct, and nonstatic fields in it. Otherwise enrichments
                        List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                        string strName = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);




                        List<IDeclContainer> visibleDecls;
                        /*if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);*/


                        if (list.Count > 0)
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, list);
                        else
                            visibleDecls = context.File.GetVisibleDecls(context.NamespaceList, true);


                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == strName)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == name && field.IsStatic)
                                        {
                                            nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }




                        /*List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);

                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription str in declContainer.Structs)
                            {
                                if (str.Name == strName)
                                {
                                    foreach (VariableDescription field in str.Fields)
                                    {
                                        if (field.Name == name && field.IsStatic)
                                        {
                                            nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, declContainer));
                                        }
                                    }
                                }
                            }
                        }*/

                    }
                    foreach (ReturnData.ContextPair<NamespaceDescription> pair in Data.Namespaces)
                    {
                        NamespaceDescription ns = pair.Type;
                        IDeclContainer context = pair.Context;
                        //Look for namespaces and static types
                        foreach (NamespaceDescription nextNS in ns.Namespaces)
                        {
                            if (nextNS.Name == name)
                                nextData.Namespaces.Add(new ReturnData.ContextPair<NamespaceDescription>(nextNS, nextNS));
                        }
                        foreach (StructDescription str in ns.Structs)
                        {
                            if (str.Name == name)
                            {
                                List<string> identifierList = ns.NamespaceList;
                                identifierList.Add(name);
                                AAName n = new AAName();
                                foreach (string s in identifierList)
                                {
                                    n.GetIdentifier().Add(new TIdentifier(s));
                                }
                                ANamedType namedType = new ANamedType(n);
                                nextData.StaticTypes.Add(new ReturnData.ContextPair<PType>(namedType, ns));
                            }
                        }
                        foreach (VariableDescription field in ns.Fields)
                        {
                            if (field.Name == name)
                            {
                                nextData.Types.Add(new ReturnData.ContextPair<PType>(field.realType, ns));
                            }
                        }
                        foreach (TypedefDescription typedef in ns.Typedefs)
                        {
                            if (typedef.Name == name)
                                nextData.StaticTypes.Add(new ReturnData.ContextPair<PType>(new ANamedType(new TIdentifier(typedef.Name), null), ns));
                        }
                    }
                    Data = nextData;
                }
            }

            public override void CaseAThisLvalue(AThisLvalue node)
            {
                if (currentStruct != null)
                {
                    APointerType type = new APointerType(new TStar("*"), new ANamedType(new TIdentifier(currentStruct.Name), null));
                    Data.Types.Add(new ReturnData.ContextPair<PType>(type, initialContext));
                }
                if (currentEnrichment != null)
                {
                    Data.Types.Add(new ReturnData.ContextPair<PType>(currentEnrichment.type, initialContext));
                }
            }

            public override void CaseAValueLvalue(AValueLvalue node)
            {
                if (currentEnrichment != null)
                {
                    Data.Types.Add(new ReturnData.ContextPair<PType>(currentEnrichment.type, initialContext));
                }
            }
        }

        /*

        private static ReturnData GetTypeNew(PExp exp)
        {
            ReturnData returner = new ReturnData();
            if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp) exp;
            }
            else if (exp is AUnopExp)
            {
                AUnopExp aExp = (AUnopExp)exp;
                
            }
            else if (exp is AIncDecExp)
            {
                AIncDecExp aExp = (AIncDecExp)exp;
                
            }
            else if (exp is AFixedConstExp)
            {
                AFixedConstExp aExp = (AFixedConstExp)exp;
                
            }
            else if (exp is AStringConstExp)
            {
                AStringConstExp aExp = (AStringConstExp)exp;

            }
            else if (exp is ACharConstExp)
            {
                ACharConstExp aExp = (ACharConstExp)exp;

            }
            else if (exp is ABooleanConstExp)
            {
                ABooleanConstExp aExp = (ABooleanConstExp)exp;

            }
            else if (exp is ANullExp)
            {
                ANullExp aExp = (ANullExp)exp;

            }
            else if (exp is ASimpleInvokeExp)
            {
                ASimpleInvokeExp aExp = (ASimpleInvokeExp)exp;

            }
            else if (exp is ANonstaticInvokeExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ASyncInvokeExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ALvalueExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is AAssignmentExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ACastExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ASharpCastExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ANewExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is ADelegateExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is AIfExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            else if (exp is AArrayResizeExp)
            {
                ABinopExp aExp = (ABinopExp)exp;

            }
            return returner;
        }


        private static ReturnData GetTypeNew(PLvalue lvalue)
        {
            ReturnData returner = new ReturnData();
            if (lvalue is AFieldLvalue)
            {
                
            }
            else if (lvalue is AStructFieldLvalue)
            {

            }
            else if (lvalue is AArrayLvalue)
            {

            }
            else if (lvalue is APointerLvalue)
            {

            }
            else if (lvalue is AAmbiguousNameLvalue)
            {

            }
            else if (lvalue is AThisLvalue)
            {

            }
            else if (lvalue is AValueLvalue)
            {

            }
            return returner;
        }








        private static ReturnData GetType(PExp exp)
        {
            if (exp is ALvalueExp)
            {
                ALvalueExp aExp = (ALvalueExp) exp;
                return GetType(aExp.GetLvalue());
            }
            if (exp is ASimpleInvokeExp)
            {
                //Just return the type of the first method that match the name and number of params
                ASimpleInvokeExp aExp = (ASimpleInvokeExp) exp;
                IDeclContainer currentDeclContainer = CurrentFile.GetDeclContainerAt(CurrentEditor.caret.Position.Line);
                foreach (StructDescription str in currentDeclContainer.Structs)
                {
                    if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                        str.LineTo >= CurrentEditor.caret.Position.Line)
                    {
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                            {
                                return new ReturnData(method.realType);
                            }
                        }
                    }
                }
                foreach (EnrichmentDescription str in currentDeclContainer.Enrichments)
                {
                    if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                        str.LineTo >= CurrentEditor.caret.Position.Line)
                    {
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                            {
                                return new ReturnData(method.realType);
                            }
                        }
                    }
                }
                List<IDeclContainer> visibleDeclContainers = CurrentFile.GetVisibleDecls(true);
                foreach (IDeclContainer file in visibleDeclContainers)
                {
                    foreach (MethodDescription method in file.Methods)
                    {
                        if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                        {
                            CurrentFile = file.File;
                            return new ReturnData(method.realType);
                        }
                    }
                }
                //Library data
                foreach (AMethodDecl method in Compiler.libraryData.Methods)
                {
                    if (method.GetName().Text == aExp.GetName().Text && method.GetFormals().Count == aExp.GetArgs().Count)
                    {
                        return new ReturnData(method.GetReturnType());
                    }
                }
                return new ReturnData();
            }
            if (exp is ASyncInvokeExp)
            {
                ASyncInvokeExp aExp = (ASyncInvokeExp)exp;
                ReturnData baseData = null;
                string methodName;
                if (aExp.GetName() is AAmbiguousNameLvalue)
                {
                    AAName aName = (AAName) ((AAmbiguousNameLvalue) aExp.GetName()).GetAmbiguous();
                    methodName = ((TIdentifier) aName.GetIdentifier()[aName.GetIdentifier().Count - 1]).Text;
                    aName.GetIdentifier().RemoveAt(aName.GetIdentifier().Count - 1);
                    if (aName.GetIdentifier().Count > 0)
                        baseData = GetType(aExp.GetName());
                }
                else
                {
                    AStructLvalue lvalue = (AStructLvalue) aExp.GetName();
                    methodName = lvalue.GetName().Text;
                    baseData = GetType(lvalue.GetReceiver());
                }
                
                ReturnData retData = new ReturnData();
                retData.Error = false;
                if (baseData != null)
                {
                    if (baseData.Error)
                        return baseData;


                    foreach (PType type in baseData.Types)
                    {
                        if (type is ANamedType)
                        {
                            ANamedType aType = (ANamedType) type;
                            AAName aName = (AAName) aType.GetName();
                            string typeName = ((TIdentifier)aName.GetIdentifier()[aName.GetIdentifier().Count - 1]).Text;
                            List<string> targetNamespace = aName.ToStringList();
                            targetNamespace.RemoveAt(targetNamespace.Count - 1);
                            List<IDeclContainer> visibleDecls = CurrentFile.GetMatchingDecls(targetNamespace);
                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription structDescription in declContainer.Structs)
                                {
                                    if (structDescription.Name == typeName)
                                    {
                                        foreach (MethodDescription method in structDescription.Methods)
                                        {
                                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count && !method.IsStatic)
                                            {
                                                retData.Types.Add(method.realType);
                                            }
                                        }
                                    }
                                }
                                foreach (NamespaceDescription ns in declContainer.Namespaces)
                                {
                                    if (ns.Name == typeName)
                                    {
                                        foreach (MethodDescription method in ns.Methods)
                                        {
                                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count)
                                            {
                                                retData.Types.Add(method.realType);
                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        {//Find enrichment
                            List<IDeclContainer> visibleDeclContainers = CurrentFile.GetVisibleDecls(true);
                            foreach (IDeclContainer declContainer in visibleDeclContainers)
                            {
                                foreach (EnrichmentDescription enrichmentDescription in declContainer.Enrichments)
                                {
                                    if (TypesEqual(type, enrichmentDescription.type, CurrentFile, enrichmentDescription.ParentFile.File))
                                    {
                                        foreach (MethodDescription method in enrichmentDescription.Methods)
                                        {
                                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count && !method.IsStatic)
                                            {
                                                retData.Types.Add(method.realType);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (PType type in baseData.StaticTypes)
                    {
                        if (type is ANamedType)
                        {
                            ANamedType aType = (ANamedType)type;
                            AAName aName = (AAName)aType.GetName();
                            string typeName = ((TIdentifier)aName.GetIdentifier()[aName.GetIdentifier().Count - 1]).Text;
                            List<string> targetNamespace = aName.ToStringList();
                            targetNamespace.RemoveAt(targetNamespace.Count - 1);
                            List<IDeclContainer> visibleDecls = CurrentFile.GetMatchingDecls(targetNamespace);
                            foreach (IDeclContainer declContainer in visibleDecls)
                            {
                                foreach (StructDescription structDescription in declContainer.Structs)
                                {
                                    if (structDescription.Name == typeName)
                                    {
                                        foreach (MethodDescription method in structDescription.Methods)
                                        {
                                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count && method.IsStatic)
                                            {
                                                retData.Types.Add(method.realType);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (NamespaceDescription ns in baseData.Namespaces)
                    {
                        foreach (MethodDescription method in ns.Methods)
                        {
                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count)
                            {
                                retData.Types.Add(method.realType);

                            }
                        }
                    }

                }
                else
                {
                    //No base data. Behave as a simpleinvoke

                    IDeclContainer currentDeclContainer = CurrentFile.GetDeclContainerAt(CurrentEditor.caret.Position.Line);
                    foreach (StructDescription str in currentDeclContainer.Structs)
                    {
                        if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                            str.LineTo >= CurrentEditor.caret.Position.Line)
                        {
                            foreach (MethodDescription method in str.Methods)
                            {
                                if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count)
                                {
                                    retData.Types.Add(method.realType);
                                }
                            }
                        }
                    }
                    foreach (EnrichmentDescription str in currentDeclContainer.Enrichments)
                    {
                        if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                            str.LineTo >= CurrentEditor.caret.Position.Line)
                        {
                            foreach (MethodDescription method in str.Methods)
                            {
                                if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count)
                                {
                                    retData.Types.Add(method.realType);
                                }
                            }
                        }
                    }
                    List<IDeclContainer> visibleDeclContainers = CurrentFile.GetVisibleDecls(true);
                    foreach (IDeclContainer file in visibleDeclContainers)
                    {
                        foreach (MethodDescription method in file.Methods)
                        {
                            if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count)
                            {
                                retData.Types.Add(method.realType);
                            }
                        }
                    }
                    //Library data
                    foreach (AMethodDecl method in Compiler.libraryData.Methods)
                    {
                        if (method.GetName().Text == methodName && method.GetFormals().Count == aExp.GetArgs().Count)
                        {
                            retData.Types.Add(method.GetReturnType());
                        }
                    }
                }

                retData.Error = retData.Types.Count + retData.StaticTypes.Count + retData.Namespaces.Count == 0;
                return retData;
            }
            if (exp is ANonstaticInvokeExp)
            {
                ANonstaticInvokeExp aExp = (ANonstaticInvokeExp)exp;
                ReturnData ret = GetType(aExp.GetReceiver());
                if (ret.Error)
                    return ret;

                ReturnData returner = new ReturnData();
                
                foreach (PType t in ret.Types)
                {
                    PType type = t;
                    AGenericType genType = null;
                    if (type is AGenericType)
                    {
                        genType = (AGenericType)type;
                        type = genType.GetBase();
                    }
                    if (type is ANamedType)
                    {
                        ANamedType aType = (ANamedType)type;
                        AAName aName = (AAName)aType.GetName();
                        string typeName = ((TIdentifier)aName.GetIdentifier()[aName.GetIdentifier().Count - 1]).Text;
                        List<string> targetNamespace = aName.ToStringList();
                        targetNamespace.RemoveAt(targetNamespace.Count - 1);
                        List<IDeclContainer> visibleDecls = CurrentFile.GetMatchingDecls(targetNamespace);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (StructDescription structDescription in declContainer.Structs)
                            {
                                if (structDescription.Name == typeName)
                                {
                                    foreach (MethodDescription method in structDescription.Methods)
                                    {
                                        if (method.Name == methodName && method.Formals.Count == aExp.GetArgs().Count && !method.IsStatic)
                                        {
                                            retData.Types.Add(method.realType);
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }


                //Join static and nonstatic in same check
                if (ret.Types.Count > 0 || ret.StaticType != null)
                {
                    PType type = ret.Type ?? ret.StaticType;
                    AGenericType genType = null;
                    if (type is AGenericType)
                    {
                        genType = (AGenericType)type;
                        type = genType.GetBase();
                    }
                    if (type is ANamedType)
                    {
                        ANamedType namedType = (ANamedType)type;
                        foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                        {
                            if ((namedType.GetNamespace() == null || namedType.GetNamespace().Text == "")
                                ? CurrentFile.CanSeeOther(file)
                                : file.Namespace == namedType.GetNamespace().Text)
                            {
                                foreach (StructDescription str in file.Structs)
                                {
                                    if (str.Name == namedType.GetName().Text)
                                    {
                                        foreach (MethodDescription method in str.Methods)
                                        {
                                            if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                                            {
                                                //If the returned type is a generic thingy, replace with the actual type.
                                                type = method.realType;
                                                if (genType != null)
                                                {
                                                    type = FixGenTypes(str, genType, type);
                                                }
                                                else
                                                    CurrentFile = file;
                                                return new ReturnData(type);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (ret.Type != null)
                        {//Look for delegates
                            foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                            {
                                if ((namedType.GetNamespace() == null || namedType.GetNamespace().Text == "")
                                    ? CurrentFile.CanSeeOther(file)
                                    : file.Namespace == namedType.GetNamespace().Text)
                                {
                                    foreach (MethodDescription method in file.Methods)
                                    {
                                        if (method.Name == namedType.GetName().Text && method.IsDelegate)
                                        {
                                            CurrentFile = file;
                                            return new ReturnData(method.realType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //No struct found. Look for enrichments
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (CurrentFile.CanSeeOther(file))
                        {
                            foreach (EnrichmentDescription enrichment in file.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, CurrentFile, file))
                                {
                                    foreach (MethodDescription method in enrichment.Methods)
                                    {
                                        if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                                        {
                                            CurrentFile = file;
                                            return new ReturnData(method.realType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //No matching enrichment method. Return error
                    if (ret.Type != null)
                        return new ReturnData();
                }
                if (ret.Namespace != null)
                {
                    //Look for methods in that namespace
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (file.Namespace == ret.Namespace)
                        {
                            foreach (MethodDescription method in file.Methods)
                            {
                                if (method.Name == aExp.GetName().Text && method.Formals.Count == aExp.GetArgs().Count)
                                {
                                    CurrentFile = file;
                                    return new ReturnData(method.realType);
                                }
                            }
                        }
                    }
                }
                return new ReturnData();
            }
            if (exp is AIntConstExp)
            {
                int value = int.Parse(((AIntConstExp) exp).GetIntegerLiteral().Text);
                if (value <= 255)
                    return new ReturnData(new ANamedType(new TIdentifier("byte"), null));
                return new ReturnData(new ANamedType(new TIdentifier("int"), null));
            }
            if (exp is AFixedConstExp)
            {
                return new ReturnData(new ANamedType(new TIdentifier("fixed"), null));
            }
            if (exp is ABooleanConstExp)
            {
                return new ReturnData(new ANamedType(new TIdentifier("bool"), null));
            }
            if (exp is AStringConstExp)
            {
                return new ReturnData(new ANamedType(new TIdentifier("string"), null));
            }
            if (exp is ACharConstExp)
            {
                return new ReturnData(new ANamedType(new TIdentifier("char"), null));
            }
            if (exp is AUnopExp)
            {
                return GetType(((AUnopExp) exp).GetExp());
            }
            if (exp is AIncDecExp)
            {
                return GetType(((AIncDecExp)exp).GetLvalue());
            }
            if (exp is ANullExp)
                return new ReturnData();
            if (exp is AAssignmentExp)
                return GetType(((AAssignmentExp)exp).GetLvalue());
            if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp) exp;
                PBinop binop = aExp.GetBinop();
                if (binop is AEqBinop || binop is ANeBinop || binop is ALtBinop || binop is ALeBinop || 
                    binop is AGtBinop || binop is AGeBinop || binop is ALazyAndBinop || binop is ALazyOrBinop)
                    return new ReturnData(new ANamedType(new TIdentifier("bool"), null));
                //AS for the rest. Get Base types. do precedence acording to text > string > fixed > int > byte > point
                ReturnData left = GetType(aExp.GetLeft());
                ReturnData right = GetType(aExp.GetRight());
                if (left.Error || right.Error)
                    return new ReturnData();
                //Must be non static named types
                if (!(left.Type != null && left.Type is ANamedType && right.Type != null && right.Type is ANamedType))
                    return new ReturnData();
                ANamedType leftType = (ANamedType) left.Type;
                ANamedType rightType = (ANamedType) right.Type;
                foreach (string type in new []{"text", "string", "fixed", "int", "int", "byte", "point"})
                {
                    if (leftType.GetName().Text == type || rightType.GetName().Text == type)
                        return new ReturnData(new ANamedType(new TIdentifier(type), null));
                }
                return new ReturnData();
            }
            if (exp is ACastExp)
            {
                PType type = ((ACastExp) exp).GetType();
                return new ReturnData(type);
            }
            if (exp is ATempCastExp)
            {
                ReturnData ret = GetType(((ATempCastExp)exp).GetType());
                if (ret.Error || ret.StaticType == null)
                    return new ReturnData();
                return new ReturnData(ret.StaticType);
            }
            if (exp is ANewExp)
            {
                return new ReturnData(new APointerType(new TStar("*"), (PType) ((ANewExp)exp).GetType().Clone()));
            }
            if (exp is ADelegateExp)
                return new ReturnData(((ADelegateExp)exp).GetType());
            throw new Exception("Unexpected exp: " + exp);
        }















        private static ReturnData GetType(PLvalue lvalue)
        {
            if (lvalue is AAmbiguousNameLvalue)
            {
                AAmbiguousNameLvalue aLvalue = (AAmbiguousNameLvalue) lvalue;
                string name = ((ASimpleName) aLvalue.GetAmbiguous()).GetIdentifier().Text;
                //Look for locals, then struct/enrichment fields/properties, then global fields/properties, then namespaces and types
                //Check if we are in a method
                MethodDescription currentMethod = null;
                foreach (MethodDescription method in CurrentFile.Methods)
                {
                    if (method.Start < CurrentEditor.caret.Position &&
                        method.End > CurrentEditor.caret.Position)
                    {
                        currentMethod = method;
                        break;
                    }
                }
                if (currentMethod == null)
                {
                    foreach (StructDescription str in CurrentFile.Structs)
                    {
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Start < CurrentEditor.caret.Position &&
                                method.End > CurrentEditor.caret.Position)
                            {
                                currentMethod = method;
                                break;
                            }
                        }
                        foreach (MethodDescription method in str.Constructors)
                        {
                            if (method.Start < CurrentEditor.caret.Position &&
                                method.End > CurrentEditor.caret.Position)
                            {
                                currentMethod = method;
                                break;
                            }
                        }
                    }
                }
                if (currentMethod != null)
                {
                    List<VariableDescription> locals = new List<VariableDescription>();
                    locals.AddRange(currentMethod.Formals);
                    locals.AddRange(currentMethod.Locals);
                    foreach (VariableDescription var in locals)
                    {
                        if (var.Name == name)
                        {
                            return new ReturnData(var.realType);
                        }
                    }
                }
                //Not a local. Look for struct variables
                foreach (StructDescription str in CurrentFile.Structs)
                {
                    if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                            str.LineTo >= CurrentEditor.caret.Position.Line)
                    {
                        foreach (VariableDescription field in str.Fields)
                        {
                            if (field.Name == name)
                            {
                                return new ReturnData(field.realType);
                            }
                        }
                    }
                }
                //Not a struct variable, look for fields
                foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                {
                    if (CurrentFile.CanSeeOther(file))
                    {
                        foreach (VariableDescription field in file.Fields)
                        {
                            if (field.Name == name)
                            {
                                CurrentFile = file;
                                return new ReturnData(field.realType);
                            }
                        }
                    }
                }
                //LibFields
                foreach (AFieldDecl field in Compiler.libraryData.Fields)
                {
                    if (field.GetName().Text == name)
                    {
                        return new ReturnData(field.GetType());
                    }
                }
                //Not a field. Look for types or namespaces
                bool matchNS = false;
                bool matchType = false;
                foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                {
                    if (file.Namespace == name)
                        matchNS = true;
                    if (CurrentFile.CanSeeOther(file))
                    {
                        foreach (StructDescription str in file.Structs)
                        {
                            if (str.Name == name)
                            {
                                matchType = true;
                            }
                        }
                    }
                }
                return new ReturnData
                           {
                               Error = !(matchNS || matchType),
                               Namespace = matchNS ? name : null,
                               StaticType = matchType ? new ANamedType(new TIdentifier(name), null) : null
                           };
            }
            if (lvalue is AStructLvalue)
            {
                //If the base has a type look for the matching struct or enrichment.
                //If the base is a namespace and/or a static type, first look for something matching the static type, and then the namespace
                AStructLvalue aLvalue = (AStructLvalue) lvalue;
                ReturnData ret = GetType(aLvalue.GetReceiver());
                if (ret.Error)
                    return ret;
                //Joined static and nonstatic vars in one. 
                if (ret.Type != null || ret.StaticType != null)
                {
                    PType type = ret.Type ?? ret.StaticType;

                    AGenericType genType = null;
                    if (type is AGenericType)
                    {
                        genType = (AGenericType)type;
                        type = genType.GetBase();
                    }
                    if (type is ANamedType)
                    {
                        ANamedType namedType = (ANamedType)type;
                        foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                        {
                            if ((namedType.GetNamespace() == null  || namedType.GetNamespace().Text == "")
                                ? CurrentFile.CanSeeOther(file) 
                                : file.Namespace == namedType.GetNamespace().Text)
                            {
                                foreach (StructDescription str in file.Structs)
                                {
                                    if (str.Name == namedType.GetName().Text)
                                    {
                                        foreach (VariableDescription field in str.Fields)
                                        {
                                            if (field.Name == aLvalue.GetName().Text)
                                            {
                                                //If the returned type is a generic thingy, replace with the actual type.
                                                type = field.realType;
                                                if (genType != null)
                                                {
                                                    type = FixGenTypes(str, genType, type);
                                                }
                                                else
                                                    CurrentFile = file;
                                                return new ReturnData(type);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //No struct found. Look for enrichments
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (CurrentFile.CanSeeOther(file))
                        {
                            foreach (EnrichmentDescription enrichment in file.Enrichments)
                            {
                                if (TypesEqual(type, enrichment.type, CurrentFile, file))
                                {
                                    foreach (VariableDescription field in enrichment.Fields)
                                    {
                                        if (field.Name == aLvalue.GetName().Text)
                                        {
                                            CurrentFile = file;
                                            return new ReturnData(field.realType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //No matching enrichment field. Return error
                    if (ret.Type != null)
                        return new ReturnData();
                }
                if (ret.Namespace != null)
                {
                    //Look for fields in that namespace
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (file.Namespace == ret.Namespace)
                        {
                            foreach (VariableDescription field in file.Fields)
                            {
                                if (field.Name == aLvalue.GetName().Text)
                                {
                                    CurrentFile = file;
                                    return new ReturnData(field.realType);
                                }
                            }
                        }
                    }
                    //Look for types in that namespace
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (file.Namespace == ret.Namespace)
                        {
                            foreach (StructDescription str in file.Structs)
                            {
                                if (str.Name == aLvalue.GetName().Text)
                                {
                                    CurrentFile = file;
                                    return new ReturnData()
                                               {
                                                   Error = false,
                                                   StaticType =
                                                       new ANamedType(new TIdentifier(str.Name),
                                                                      new TIdentifier(file.Namespace))
                                               };
                                }
                            }
                        }
                    }
                }
                return new ReturnData();
            }
            if (lvalue is APointerLvalue)
            {
                APointerLvalue aLvalue = (APointerLvalue) lvalue;
                ReturnData ret = GetType(aLvalue.GetBase());
                if (ret.Type != null && ret.Type is ADynamicArrayType)
                    return ret;
                if (ret.Error || ret.Type == null || !(ret.Type is APointerType))
                    return new ReturnData();
                return new ReturnData(((APointerType)ret.Type).GetType());
            }
            if (lvalue is AThisLvalue)
            {
                //Find parent struct or enrichment.
                //Then, if we are in a class or a constructor, make a pointer to that. Otherwise return that type
                foreach (StructDescription str in CurrentFile.Structs)
                {
                    if (str.LineFrom <= CurrentEditor.caret.Position.Line &&
                        str.LineTo >= CurrentEditor.caret.Position.Line)
                    {
                        if (str.IsClass)
                            return new ReturnData(new APointerType(new TStar("*"), new ANamedType(new TIdentifier(str.Name), null)));

                        foreach (MethodDescription constructor in str.Constructors)
                        {
                            if (constructor.Start < CurrentEditor.caret.Position &&
                                constructor.End > CurrentEditor.caret.Position)
                            {
                                return new ReturnData(new APointerType(new TStar("*"), new ANamedType(new TIdentifier(str.Name), null)));
                            }
                        }

                        //Not a dynamic context. this is not allowed
                        return new ReturnData();
                    }
                }
                foreach (EnrichmentDescription enrichment in CurrentFile.Enrichments)
                {
                    if (enrichment.LineFrom <= CurrentEditor.caret.Position.Line &&
                        enrichment.LineTo >= CurrentEditor.caret.Position.Line)
                    {
                        return new ReturnData((PType) enrichment.type.Clone());
                    }
                }
                return new ReturnData();
            }
            if (lvalue is AValueLvalue)
            {
                //We must be in a property (represented with methods).
                //Return the return type
                foreach (MethodDescription method in CurrentFile.Methods)
                {
                    if (method.Start < CurrentEditor.caret.Position &&
                        method.End > CurrentEditor.caret.Position)
                    {
                        return new ReturnData(method.propertyType);
                    }
                }
                foreach (StructDescription str in CurrentFile.Structs)
                {
                    foreach (MethodDescription method in str.Methods)
                    {
                        if (method.Start < CurrentEditor.caret.Position &&
                            method.End > CurrentEditor.caret.Position)
                        {
                            return new ReturnData(method.propertyType);
                        }
                    }
                }
                foreach (EnrichmentDescription enrichement in CurrentFile.Enrichments)
                {
                    foreach (MethodDescription method in enrichement.Methods)
                    {
                        if (method.Start < CurrentEditor.caret.Position &&
                            method.End > CurrentEditor.caret.Position)
                        {
                            return new ReturnData(method.propertyType);
                        }
                    }
                }
                return new ReturnData();
            }
            if (lvalue is AFieldLvalue)
            {
                AFieldLvalue aLvalue = (AFieldLvalue) lvalue;
                foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                {
                    if (CurrentFile.CanSeeOther(file))
                    {
                        foreach (VariableDescription field in file.Fields)
                        {
                            if (field.Name == aLvalue.GetName().Text)
                            {
                                return new ReturnData(field.realType);
                            }
                        }
                    }
                }
                foreach (AFieldDecl field in Compiler.libraryData.Fields)
                {
                    if (field.GetName().Text == aLvalue.GetName().Text)
                    {
                        return new ReturnData(field.GetType());
                    }
                }
            }
            if (lvalue is AArrayLvalue)
            {
                AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                ReturnData ret = GetType(aLvalue.GetBase());
                if (ret.Error || ret.Type == null || !(ret.Type is AArrayTempType))
                    return new ReturnData();
                return new ReturnData(((AArrayTempType)ret.Type).GetType());
            }
            throw new Exception("Unexpected lvalue: " + lvalue);
        }
        */
        public static bool TypesEqual(PType type1, PType type2, IDeclContainer context1, IDeclContainer context2)
        {
            if (type1.GetType() != type2.GetType())
                return false;
            if (type1 is AVoidType)
                return true;
            if (type1 is AArrayTempType)
            {
                AArrayTempType aType1 = (AArrayTempType)type1;
                AArrayTempType aType2 = (AArrayTempType)type2;
                bool e1, e2;
                int dim1 = FoldConstants(aType1.GetDimention(), out e1);
                int dim2 = FoldConstants(aType1.GetDimention(), out e2);
                if (e1 || e2)
                    return false;
                if (dim1 != dim2)
                    return false;
                return TypesEqual(aType1.GetType(), aType2.GetType(), context1, context2);
            }
            if (type1 is ADynamicArrayType)
            {
                ADynamicArrayType aType1 = (ADynamicArrayType)type1;
                ADynamicArrayType aType2 = (ADynamicArrayType)type2;
                return TypesEqual(aType1.GetType(), aType2.GetType(), context1, context2);
            }
            if (type1 is ANamedType)
            {
                ANamedType aType1 = (ANamedType)type1;
                ANamedType aType2 = (ANamedType)type2;
                List<string> name1 = ((AAName)aType1.GetName()).ToStringList();
                List<string> name2 = ((AAName)aType2.GetName()).ToStringList();
                bool nameMatch = (name1[name1.Count - 1] == name2[name2.Count - 1]);
                    
                if (aType1.IsPrimitive() && aType2.IsPrimitive())
                    return nameMatch;
                //What's left is only structs and classes (!or typedefs)
                string name = name1[name1.Count - 1];
                name1.RemoveAt(name1.Count - 1);
                name2.RemoveAt(name2.Count - 1);
                List<TypedefDescription>[] possibleTypedefs = new List<TypedefDescription>[2];
                possibleTypedefs[0] = new List<TypedefDescription>();
                possibleTypedefs[1] = new List<TypedefDescription>();

                List<StructDescription> possibleTargets1 = new List<StructDescription>();
                List<IDeclContainer> visibleDecls;
                if (name1.Count == 0)
                    visibleDecls = context1.File.GetVisibleDecls(context1.NamespaceList, true);
                else
                    visibleDecls = context1.File.GetVisibleDecls(context1.NamespaceList, name1);
                foreach (IDeclContainer declContainer in visibleDecls)
                {
                    foreach (StructDescription str in declContainer.Structs)
                    {
                        if (str.Name == name && nameMatch)
                            possibleTargets1.Add(str);
                    }
                    foreach (TypedefDescription typedef in declContainer.Typedefs)
                    {
                        if (typedef.Name == name)
                            possibleTypedefs[0].Add(typedef);
                    }
                }
                
                List<StructDescription> possibleTargets2 = new List<StructDescription>();
                if (name2.Count == 0)
                    visibleDecls = context2.File.GetVisibleDecls(context2.NamespaceList, true);
                else
                    visibleDecls = context2.File.GetVisibleDecls(context2.NamespaceList, name2);
                foreach (IDeclContainer declContainer in visibleDecls)
                {
                    foreach (StructDescription str in declContainer.Structs)
                    {
                        if (str.Name == name && nameMatch)
                        {
                            possibleTargets2.Add(str);
                        }
                    }
                    foreach (TypedefDescription typedef in declContainer.Typedefs)
                    {
                        if (typedef.Name == name)
                            possibleTypedefs[1].Add(typedef);
                    }
                }

                foreach (StructDescription str in possibleTargets2)
                {
                    if (possibleTargets1.Contains(str))
                        return true;
                }

                foreach (TypedefDescription typedef in possibleTypedefs[0])
                {
                    if (TypesEqual(typedef.realType, type2, context1, context2))
                        return true;
                }

                foreach (TypedefDescription typedef in possibleTypedefs[1])
                {
                    if (TypesEqual(type1, typedef.realType, context1, context2))
                        return true;
                }

                foreach (TypedefDescription typedef1 in possibleTypedefs[0])
                {
                    foreach (TypedefDescription typedef2 in possibleTypedefs[1])
                    {
                        if (TypesEqual(typedef1.realType, typedef2.realType, context1, context2))
                            return true;
                    }
                }

                return false;
            }
            if (type1 is APointerType)
            {
                APointerType aType1 = (APointerType)type1;
                APointerType aType2 = (APointerType)type2;
                return TypesEqual(aType1.GetType(), aType2.GetType(), context1, context2);
            }
            if (type1 is AGenericType)
            {
                AGenericType aType1 = (AGenericType)type1;
                AGenericType aType2 = (AGenericType)type2;
                if (!TypesEqual(aType1.GetBase(), aType2.GetBase(), context1, context2))
                    return false;
                for (int i = 0; i < aType1.GetGenericTypes().Count; i++)
                {
                    if (!TypesEqual((PType)aType1.GetGenericTypes()[i], (PType)aType2.GetGenericTypes()[i], context1, context2))
                        return false;
                }
                return true;
            }
            throw new Exception("Unexpected type: " + type1);
        }

        private static int FoldConstants(PExp exp, out bool error)
        {
            error = false;
            if (exp is AIntConstExp)
            {
                return int.Parse(((AIntConstExp) exp).GetIntegerLiteral().Text);
            }
            error = true;
            return -1;
        }

        private static PDecl Parse(List<Token> tokens)
        {
            StringBuilder builder = new StringBuilder("");
            foreach (Token token in tokens)
            {
                builder.Append(token.Text + " ");
            }
            Parser p = new Parser(new Lexer(new StringReader(builder.ToString())));
            AASourceFile file = (AASourceFile)p.Parse().GetPSourceFile();

            file.Apply(new Weeder(new ErrorCollection(), new SharedData()));

            return (PDecl)file.GetDecl()[0];
        }

        private static PType FixGenTypes(StructDescription str, AGenericType genType, PType type)
        {
            APointerType wrapper = new APointerType(new TStar("*"),  (PType) type.Clone());
            wrapper.Apply(new FixGenTypesClass(str, genType));
            return wrapper.GetType();
        }

        private class FixGenTypesClass : DepthFirstAdapter
        {
            private StructDescription str;
            private AGenericType genType;

            public FixGenTypesClass(StructDescription str, AGenericType genType)
            {
                this.str = str;
                this.genType = genType;
            }

            public override void CaseANamedType(ANamedType node)
            {
                for (int i = 0; i < str.GenericVars.Count; i++)
                {
                    string genericVar = str.GenericVars[i];
                    if (node.AsString() == genericVar)
                    {
                        node.ReplaceBy((PType)((PType)genType.GetGenericTypes()[i]).Clone());
                        return;
                    }
                }
            }
        }
    }
}
