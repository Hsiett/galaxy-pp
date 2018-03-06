using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class PropertyDescription
    {
        public static void CreateItems(APropertyDecl decl, List<MethodDescription> methods, out VariableDescription variable)
        {
            if (decl.GetName().Text == "")
                variable = null;
            else
                variable = new VariableDescription(decl);
            
            TextPoint getterStart, setterStart;
            getterStart = setterStart = TextPoint.FromCompilerCoords(decl.GetName());
            if (decl.GetSetter() != null && decl.GetGetter() != null)
            {
                if (Util.TokenLessThan(((AABlock)decl.GetSetter()).GetToken(), ((AABlock)decl.GetGetter()).GetToken()))
                    getterStart = TextPoint.FromCompilerCoords(((AABlock)decl.GetSetter()).GetToken());
                else
                    setterStart = TextPoint.FromCompilerCoords(((AABlock)decl.GetGetter()).GetToken());
            }
            if (decl.GetGetter() != null)
                methods.Add(new MethodDescription(getterStart, decl.GetType(), (AABlock)decl.GetGetter(), decl.GetType()));
            if (decl.GetSetter() != null)
                methods.Add(new MethodDescription(setterStart, new AVoidType(new TVoid("void")), (AABlock)decl.GetSetter(), decl.GetType()));
        }

        /*

        private APropertyDecl decl;
        public List<VariableDescription> Locals = new List<VariableDescription>();

        public bool IsStatic
        {
            get { return decl.GetStatic() != null; }
        }

        public PropertyDescription(APropertyDecl decl)
        {
            this.decl = decl;
            Parser parser = new Parser();
            decl.Apply();
        }

        


        public string DisplayText
        {
            get { return decl.GetName().Text; }
        }

        public string InsertText
        {
            get { return decl.GetName().Text; }
        }

        public string TooltipText
        {
            get { return "Property " + Util.TypeToString(decl.GetType()) + " " + decl.GetName().Text; }
        }

        public string Signature
        {
            get { return "Prop:" + decl.GetName().Text; }
        }

        private class Parser : DepthFirstAdapter
        {
            public List<VariableDescription> Locals = new List<VariableDescription>();

            public override void OutAALocalDecl(AALocalDecl node)
            {
                Locals.Add(new VariableDescription(node, VariableDescription.VariableTypes.LocalVariable));
            }
            
        }*/
    }
}
