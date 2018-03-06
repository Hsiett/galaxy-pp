using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases
{
    class CodeGeneration : DepthFirstAdapter
    {

        public static void Parse(AAProgram ast, ErrorCollection errors, SharedData data, DirectoryInfo outputDir)
        {
            ast.Apply(new CodeGeneration(errors, data, outputDir));
        }

        private ErrorCollection errors;
        private SharedData data;
        private AASourceFile currentSourceFile;
        private DirectoryInfo outputDir;
        private StreamWriter stream;
        private int indents = 0;

        public CodeGeneration(ErrorCollection errors, SharedData data, DirectoryInfo outputDir)
        {
            this.errors = errors;
            this.data = data;
            this.outputDir = outputDir;
        }

        private string currentLine = "";
        private void Write(string text, bool newLine = false)
        {
            if (!text.Contains("\n"))
            {
                indents -= text.Count(ch => ch == '}');
                currentLine += text;
                if (newLine)
                {
                    for (int i = 0; i < indents; i++)
                    {
                        currentLine = "    " + currentLine;
                    }
                    stream.WriteLine(currentLine);
                    currentLine = "";
                }
                indents += text.Count(ch => ch == '{');

                return;
            }
            while (text != "")
            {
                int index = text.IndexOf('\n');
                if (index == -1)
                {
                    Write(text);
                    return;
                }
                string left = text.Substring(0, index);
                Write(left, true);
                text = text.Remove(0, index + 1);
            }
        }

        public override void CaseAASourceFile(AASourceFile node)
        {
            string name = outputDir.FullName + "\\";
           /* if (Options.Compiler.OneOutputFile)
                name += "MapScript";
            else*/
                name += node.GetName().Text;
            name += ".galaxy";
            FileInfo file = new FileInfo(name);
            if (!file.Directory.Exists) file.Directory.Create();
            stream = new StreamWriter(file.Open(FileMode.Create));
            foreach (PDecl decl in node.GetDecl())
            {
                decl.Apply(this);
            }
            Write("", true);
            stream.Close();
        }

        public override void CaseAFieldDecl(AFieldDecl node)
        {
            if (node.GetStatic() != null) Write("static ");
            if (node.GetConst() != null) Write("const ");
            node.GetType().Apply(this);
            Write(" " + node.GetName().Text);
            if (node.GetInit() != null)
            {
                Write(" = ");
                node.GetInit().Apply(this);
            }
            Write(";\n\n");
        }

        public override void CaseAMethodDecl(AMethodDecl node)
        {
            Write("\n");
            if (node.GetStatic() != null) Write("static ");
            if (node.GetNative() != null) Write("native ");
            node.GetReturnType().Apply(this);
            Write(" " + node.GetName().Text + "(");
            bool first = true;
            foreach (AALocalDecl formal in node.GetFormals())
            {
                if (!first) Write(", ");
                formal.Apply(this);
                first = false;
            }
            if (node.GetBlock() != null)
            {
                Write(")\n");
                node.GetBlock().Apply(this);
            }
            else
                Write(");\n\n");
        }

        public override void CaseAIncludeDecl(AIncludeDecl node)
        {
            Write("include " + node.GetName().Text + "\n");
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            Write("struct " + node.GetName().Text + "\n{\n");
            foreach (AALocalDecl local in node.GetLocals().OfType<AALocalDecl>())
            {
                local.Apply(this);
                Write(";\n");
            }
            Write("};\n");
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (node.GetConst() != null) Write("const ");
            node.GetType().Apply(this);
            Write(" " + node.GetName().Text);
            if (node.GetInit() != null)
            {
                Write(" = ");
                node.GetInit().Apply(this);
            }
        }

        public override void CaseAVoidType(AVoidType node)
        {
            Write("void");
        }

        public override void CaseAArrayTempType(AArrayTempType node)
        {
            node.GetType().Apply(this);
            Write("[" + node.GetIntDim().Text + "]");
        }

        public override void CaseANamedType(ANamedType node)
        {
            Write(node.AsString());
        }

        public override void CaseANullType(ANullType node)
        {
            Write("null");
        }

        public override void CaseAABlock(AABlock node)
        {
            Write("{\n");
            foreach (PStm stm in node.GetStatements())
            {
                stm.Apply(this);
            }
            Write("}\n");
        }

        public override void CaseAExpStm(AExpStm node)
        {
            node.GetExp().Apply(this);
            Write(";\n");
        }

        public override void CaseAIfThenStm(AIfThenStm node)
        {
            Write("if(");
            node.GetCondition().Apply(this);
            Write(")\n");
            node.GetBody().Apply(this);
        }

        public override void CaseAIfThenElseStm(AIfThenElseStm node)
        {
            Write("if(");
            node.GetCondition().Apply(this);
            Write(")\n");
            node.GetThenBody().Apply(this);
            Write("else\n");
            node.GetElseBody().Apply(this);
        }

        public override void CaseAWhileStm(AWhileStm node)
        {
            Write("while(");
            node.GetCondition().Apply(this);
            Write(")\n");
            node.GetBody().Apply(this);
        }

        public override void CaseAVoidReturnStm(AVoidReturnStm node)
        {
            Write("return;\n");
        }

        public override void CaseAValueReturnStm(AValueReturnStm node)
        {
            Write("return ");
            node.GetExp().Apply(this);
            Write(";\n");
        }

        public override void CaseALocalDeclStm(ALocalDeclStm node)
        {
            node.GetLocalDecl().Apply(this);
            Write(";\n");
        }

        public override void CaseABreakStm(ABreakStm node)
        {
            Write("break;\n");
        }

        public override void CaseAContinueStm(AContinueStm node)
        {
            Write("continue;\n");
        }

        public override void CaseAIntConstExp(AIntConstExp node)
        {
            Write(node.GetIntegerLiteral().Text);
        }

        public override void CaseAFixedConstExp(AFixedConstExp node)
        {
            Write(node.GetFixedLiteral().Text);
        }

        public override void CaseAStringConstExp(AStringConstExp node)
        {
            Write(node.GetStringLiteral().Text);
        }

        public override void CaseACharConstExp(ACharConstExp node)
        {
            Write(node.GetCharLiteral().Text);
        }

        public override void CaseABooleanConstExp(ABooleanConstExp node)
        {
            Write(node.GetBool() is ATrueBool ? "true" : "false");
        }

        public override void CaseANullExp(ANullExp node)
        {
            Write("null");
        }

        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            Write(node.GetName().Text + "(");
            bool first = true;
            foreach (PExp exp in node.GetArgs())
            {
                if (!first) Write(", ");
                exp.Apply(this);
                first = false;
            }
            Write(")");
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            node.GetLvalue().Apply(this);
            //node.GetToken().Apply(this);
            Write(" = ");
            node.GetExp().Apply(this);
        }

        public override void CaseAAssignAssignop(AAssignAssignop node)
        {
            Write(" = ");
        }

        public override void CaseAAddAssignop(AAddAssignop node)
        {
            Write(" += ");
        }

        public override void CaseASubAssignop(ASubAssignop node)
        {
            Write(" -= ");
        }

        public override void CaseAMulAssignop(AMulAssignop node)
        {
            Write(" *= ");
        }

        public override void CaseADivAssignop(ADivAssignop node)
        {
            Write(" /= ");
        }

        public override void CaseAModAssignop(AModAssignop node)
        {
            Write(" %= ");
        }

        public override void CaseAParenExp(AParenExp node)
        {
            Write("(");
            node.GetExp().Apply(this);
            Write(")");
        }

        public override void CaseTIdentifier(TIdentifier node)
        {
            Write(node.Text);
        }

        public override void CaseAStructLvalue(AStructLvalue node)
        {
            node.GetReceiver().Apply(this);
            Write("." + node.GetName().Text);
        }

        public override void CaseAArrayLvalue(AArrayLvalue node)
        {
            node.GetBase().Apply(this);
            Write("[");
            node.GetIndex().Apply(this);
            Write("]");
        }


        public override void CaseAPlusBinop(APlusBinop node)
        {
            Write(" + ");
        }

        public override void CaseAMinusBinop(AMinusBinop node)
        {
            Write(" - ");
        }

        public override void CaseATimesBinop(ATimesBinop node)
        {
            Write("*");
        }

        public override void CaseADivideBinop(ADivideBinop node)
        {
            Write("/");
        }

        public override void CaseAModuloBinop(AModuloBinop node)
        {
            Write("%");
        }

        public override void CaseAEqBinop(AEqBinop node)
        {
            Write(" == ");
        }

        public override void CaseANeBinop(ANeBinop node)
        {
            Write(" != ");
        }

        public override void CaseALtBinop(ALtBinop node)
        {
            Write(" < ");
        }

        public override void CaseALeBinop(ALeBinop node)
        {
            Write(" <= ");
        }

        public override void CaseAGtBinop(AGtBinop node)
        {
            Write(" > ");
        }

        public override void CaseAGeBinop(AGeBinop node)
        {
            Write(" >= ");
        }

        public override void CaseAAndBinop(AAndBinop node)
        {
            Write("&");
        }

        public override void CaseAOrBinop(AOrBinop node)
        {
            Write("|");
        }

        public override void CaseAXorBinop(AXorBinop node)
        {
            Write("^");
        }

        public override void CaseALazyAndBinop(ALazyAndBinop node)
        {
            Write(" && ");
        }

        public override void CaseALazyOrBinop(ALazyOrBinop node)
        {
            Write(" || ");
        }

        public override void CaseALBitShiftBinop(ALBitShiftBinop node)
        {
            Write("<<");
        }

        public override void CaseARBitShiftBinop(ARBitShiftBinop node)
        {
            Write(">>");
        }

        public override void CaseAConcatBinop(AConcatBinop node)
        {
            Write(" + ");
        }

        public override void CaseANegateUnop(ANegateUnop node)
        {
            Write("-");
        }

        public override void CaseAComplementUnop(AComplementUnop node)
        {
            Write("!");
        }

        
    }
}
