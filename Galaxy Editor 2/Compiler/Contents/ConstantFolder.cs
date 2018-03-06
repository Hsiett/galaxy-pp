using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class ConstantFolder : DepthFirstAdapter
    {
        public static bool Fold(GalaxyCompiler compiler)
        {
            bool changes = false;

            for (int i = 0; i < compiler.ParsedSourceFiles.Count; i++)
            {
                SourceFileContents file = compiler.ParsedSourceFiles[i];
                foreach (VariableDescription field in file.Fields)
                {
                    if (field.Const)
                    {
                        PExp init = field.init;
                        string typeStr;
                        if (init == null)
                            typeStr = null;
                        else
                        {
                            ConstantFolder folder = new ConstantFolder();
                            field.init.Apply(folder);
                            typeStr = folder.Value;
                        }
                        if (field.initStr != typeStr)
                        {
                            changes = true;
                            field.initStr = typeStr;
                        }
                    }
                }
            }
            return changes;
        }

        public string Value = "";

        

        public override void CaseAPlusBinop(APlusBinop node)
        {
            Value += " + ";
        }

        public override void CaseAMinusBinop(AMinusBinop node)
        {
            Value += " - ";
        }

        public override void CaseATimesBinop(ATimesBinop node)
        {
            Value += "*";
        }

        public override void CaseADivideBinop(ADivideBinop node)
        {
            Value += "/";
        }

        public override void CaseAModuloBinop(AModuloBinop node)
        {
            Value += "%";
        }

        public override void CaseAEqBinop(AEqBinop node)
        {
            Value += " == ";
        }

        public override void CaseANeBinop(ANeBinop node)
        {
            Value += " != ";
        }

        public override void CaseALtBinop(ALtBinop node)
        {
            Value += " < ";
        }

        public override void CaseALeBinop(ALeBinop node)
        {
            Value += " <= ";
        }

        public override void CaseAGtBinop(AGtBinop node)
        {
            Value += " > ";
        }

        public override void CaseAGeBinop(AGeBinop node)
        {
            Value += " >= ";
        }

        public override void CaseAAndBinop(AAndBinop node)
        {
            Value += " & ";
        }

        public override void CaseAOrBinop(AOrBinop node)
        {
            Value += " | ";
        }

        public override void CaseAXorBinop(AXorBinop node)
        {
            Value += " ^ ";
        }

        public override void CaseALazyAndBinop(ALazyAndBinop node)
        {
            Value += " && ";
        }

        public override void CaseALazyOrBinop(ALazyOrBinop node)
        {
            Value += " || ";
        }

        public override void CaseALBitShiftBinop(ALBitShiftBinop node)
        {
            Value += "<<";
        }

        public override void CaseARBitShiftBinop(ARBitShiftBinop node)
        {
            Value += ">>";
        }

        public override void CaseANegateUnop(ANegateUnop node)
        {
            Value += "-";
        }

        public override void CaseAComplementUnop(AComplementUnop node)
        {
            Value += "!";
        }

        public override void CaseAParenExp(AParenExp node)
        {
            Value += "(";
            base.CaseAParenExp(node);
            Value += ")";
        }

        public override void CaseAIncDecExp(AIncDecExp node)
        {
            if (node.GetIncDecOp() is APreIncIncDecOp)
                Value += "++";
            if (node.GetIncDecOp() is APreDecIncDecOp)
                Value += "--";
            base.CaseAIncDecExp(node);
            if (node.GetIncDecOp() is APostIncIncDecOp)
                Value += "++";
            if (node.GetIncDecOp() is APostDecIncDecOp)
                Value += "--";
        }

        public override void CaseAIntConstExp(AIntConstExp node)
        {
            Value += node.GetIntegerLiteral().Text;
        }

        public override void CaseAHexConstExp(AHexConstExp node)
        {
            Value += node.GetHexLiteral().Text;
        }

        public override void CaseAOctalConstExp(AOctalConstExp node)
        {
            Value += node.GetOctalLiteral().Text;
        }

        public override void CaseAFixedConstExp(AFixedConstExp node)
        {
            Value += node.GetFixedLiteral().Text;
        }

        public override void CaseAStringConstExp(AStringConstExp node)
        {
            Value += node.GetStringLiteral().Text;
        }

        public override void CaseACharConstExp(ACharConstExp node)
        {
            Value += node.GetCharLiteral().Text;
        }

        public override void CaseATrueBool(ATrueBool node)
        {
            Value += "true";
        }

        public override void CaseAFalseBool(AFalseBool node)
        {
            Value += "false";
        }

        public override void CaseANullExp(ANullExp node)
        {
            Value += "null";
        }

        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            Value += node.GetName().Text + "(";
            bool first = true;
            foreach (PExp arg in node.GetArgs())
            {
                if (!first)
                    Value += ", ";
                else
                    first = false;
                arg.Apply(this);
            }
            Value += ")";
        }

        public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
        {
            node.GetReceiver().Apply(this);
            node.GetDotType().Apply(this);
            Value += node.GetName().Text + "(";
            bool first = true;
            foreach (PExp arg in node.GetArgs())
            {
                if (!first)
                    Value += ", ";
                else
                    first = false;
                arg.Apply(this);
            }
            Value += ")";
        }

        public override void CaseADotDotType(ADotDotType node)
        {
            Value += ".";
        }

        public override void CaseAArrowDotType(AArrowDotType node)
        {
            Value += "->";
        }

        public override void CaseAAName(AAName node)
        {
            Value += node.AsString();
        }


        

        public override void CaseAStructLvalue(AStructLvalue node)
        {
            //Only do namespace 
            node.GetReceiver().Apply(this);
            node.GetDotType().Apply(this);
            Value += node.GetName().Text;
        }

        public override void DefaultOut(Node node)
        {
            
        }
    }
}
