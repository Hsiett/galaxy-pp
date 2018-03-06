using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class MakeShortNames : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public MakeShortNames(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        private string nextName = "a";

        private void NextName()
        {
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
                    /*nextName = nextName.Remove(i, 1).Insert(i, "a");
                    i--;
                    continue;*/
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
                //Lets not hit an existing keyword or such
                if (GalaxyKeywords.InMethodKeywords.words.Contains(nextName) ||
                    GalaxyKeywords.SystemExpressions.words.Contains(nextName) ||
                    GalaxyKeywords.NullablePrimitives.words.Contains(nextName) ||
                    GalaxyKeywords.OutMethodKeywords.words.Contains(nextName) ||
                    GalaxyKeywords.Primitives.words.Contains(nextName) ||
                    finalTrans.data.Libraries.Structs.Any(item => item.GetName().Text == nextName) ||
                    finalTrans.data.Libraries.Methods.Any(item => item.GetName().Text == nextName) ||
                    finalTrans.data.Libraries.Fields.Any(item => item.GetName().Text == nextName))
                    NextName();

                return;
            }
        }

        List<Node> decls = new List<Node>();
        public override void CaseAAProgram(AAProgram node)
        {
            /*decls.AddRange(finalTrans.data.Structs.Select(str => str.Decl));
            decls.AddRange(finalTrans.data.Methods.Select(str => str.Decl));
            decls.AddRange(finalTrans.data.Fields.Select(str => str.Decl));
            if (finalTrans.data.Locals.Count > 0)
                decls.AddRange(finalTrans.data.Locals.Select(str => str.Value).Aggregate(Aggregate));
            if (finalTrans.data.Structs.Count > 0)
                decls.AddRange(finalTrans.data.StructFields.Values.Aggregate(Aggregate));*/
            base.CaseAAProgram(node);
            Random rand = new Random();
            while (decls.Count > 0)
            {
                int i = rand.Next(decls.Count);
                Node n = decls[i];
                decls.RemoveAt(i);

                if (n is AFieldDecl)
                {
                    AFieldDecl an = (AFieldDecl) n;
                    an.GetName().Text = nextName;
                }
                else if (n is AMethodDecl)
                {
                    AMethodDecl an = (AMethodDecl)n;
                    if (finalTrans.mainEntry == an || (an.GetTrigger() != null && finalTrans.data.HasUnknownTrigger))
                        continue;
                    an.GetName().Text = nextName;
                }
                else if (n is AALocalDecl)
                {
                    AALocalDecl an = (AALocalDecl)n;
                    an.GetName().Text = nextName;
                }
                else if (n is AStructDecl)
                {
                    AStructDecl an = (AStructDecl)n;
                    an.GetName().Text = nextName;
                }
                NextName();
            }
        }

        public List<AALocalDecl> Aggregate(List<AALocalDecl> init, List<AALocalDecl> str)
        {
            init.AddRange(str);
            return init;
        }

        public override void InAFieldDecl(AFieldDecl node)
        {
            decls.Add(node);
            /*node.GetName().Text = nextName;
            NextName();*/
        }

        public override void InAMethodDecl(AMethodDecl node)
        {
            decls.Add(node);
            /*if (finalTrans.mainEntry == node || (node.GetTrigger() != null && finalTrans.data.HasUnknownTrigger))
                return;
            node.GetName().Text = nextName;
            NextName();*/
        }

        public override void InAALocalDecl(AALocalDecl node)
        {
            decls.Add(node);
            /*node.GetName().Text = nextName;
            NextName();*/
        }

        public override void InAStructDecl(AStructDecl node)
        {
            decls.Add(node);
            /*node.GetName().Text = nextName;
            NextName();*/
        }
    }
}
