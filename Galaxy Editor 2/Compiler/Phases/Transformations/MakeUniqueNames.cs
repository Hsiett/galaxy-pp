using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class MakeUniqueNames : DepthFirstAdapter 
    {
        private FinalTransformations finalTrans;

        

        private MakeUniqueNames(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
        }

        public static void Parse(AAProgram ast, FinalTransformations finalTrans)
        {
            bool restart = true;
            while (restart)
            {
                restart = false;
                //Fix locals
                ast.Apply(new MakeUniqueNames(finalTrans));

                //Fix methods
                foreach (SharedData.DeclItem<AMethodDecl> declItem1 in finalTrans.data.Methods)
                {
                    AMethodDecl decl1 = declItem1.Decl;
                    if (decl1.GetName().Text.StartsWith("_"))
                    {
                        decl1.GetName().Text = "u" + decl1.GetName().Text;
                        restart = true;
                        break;
                    }
                    //Other methods
                    foreach (SharedData.DeclItem<AMethodDecl> declItem2 in finalTrans.data.Methods)
                    {
                        AMethodDecl decl2 = declItem2.Decl;
                        if (decl1 != decl2 && decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "1";
                            decl2.GetName().Text += "2";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;

                    //Fields
                    foreach (SharedData.DeclItem<AFieldDecl> declItem2 in finalTrans.data.Fields)
                    {
                        AFieldDecl decl2 = declItem2.Decl;
                        if (decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "M";
                            decl2.GetName().Text += "F";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;


                    //structs
                    foreach (SharedData.DeclItem<AStructDecl> declItem2 in finalTrans.data.Structs)
                    {
                        AStructDecl decl2 = declItem2.Decl;
                        if (decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "M";
                            decl2.GetName().Text += "S";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;
                }
                if (restart)
                    continue;


                //Fix fields
                foreach (SharedData.DeclItem<AFieldDecl> declItem1 in finalTrans.data.Fields)
                {
                    AFieldDecl decl1 = declItem1.Decl;
                    if (decl1.GetName().Text.StartsWith("_"))
                    {
                        decl1.GetName().Text = "u" + decl1.GetName().Text;
                        restart = true;
                        break;
                    }
                    //Other fields
                    foreach (SharedData.DeclItem<AFieldDecl> declItem2 in finalTrans.data.Fields)
                    {
                        AFieldDecl decl2 = declItem2.Decl;
                        if (decl1 != decl2 && decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "1";
                            decl2.GetName().Text += "2";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;

                    
                    //structs
                    foreach (SharedData.DeclItem<AStructDecl> declItem2 in finalTrans.data.Structs)
                    {
                        AStructDecl decl2 = declItem2.Decl;
                        if (decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "F";
                            decl2.GetName().Text += "S";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;
                }
                if (restart)
                    continue;


                //Fix structs
                foreach (SharedData.DeclItem<AStructDecl> declItem1 in finalTrans.data.Structs)
                {
                    AStructDecl decl1 = declItem1.Decl;
                    if (decl1.GetName().Text.StartsWith("_"))
                    {
                        decl1.GetName().Text = "u" + decl1.GetName().Text;
                        restart = true;
                        break;
                    }
                    //Other fields
                    foreach (SharedData.DeclItem<AStructDecl> declItem2 in finalTrans.data.Structs)
                    {
                        AStructDecl decl2 = declItem2.Decl;
                        if (decl1 != decl2 && decl1.GetName().Text == decl2.GetName().Text)
                        {
                            decl1.GetName().Text += "1";
                            decl2.GetName().Text += "2";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        break;
                }
                if (restart)
                    continue;
            
            }
        }

        private List<AALocalDecl> locals = new List<AALocalDecl>();
        public override void InAMethodDecl(AMethodDecl node)
        {
            locals.Clear();
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            locals.Add(node);
        }

        public override void OutAMethodDecl(AMethodDecl node)
        {
            //Check locals against everything else
            foreach (AALocalDecl local in locals)
            {
                if (local.GetName().Text.StartsWith("_"))
                {
                    local.GetName().Text = "u" + local.GetName().Text;
                }
                bool restart = true;
                while (restart)
                {
                    restart = false;
                    //Other locals
                    foreach (AALocalDecl local2 in locals)
                    {
                        if (local != local2 && local.GetName().Text == local2.GetName().Text)
                        {
                            local.GetName().Text += "1";
                            local.GetName().Text += "2";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        continue;


                    //Methods
                    foreach (SharedData.DeclItem<AMethodDecl> declItem in finalTrans.data.Methods)
                    {
                        AMethodDecl decl = declItem.Decl;
                        if (decl.GetName().Text == local.GetName().Text)
                        {
                            decl.GetName().Text += "M";
                            local.GetName().Text += "L";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        continue;

                    //Fields
                    foreach (SharedData.DeclItem<AFieldDecl> declItem in finalTrans.data.Fields)
                    {
                        AFieldDecl decl = declItem.Decl;
                        if (decl.GetName().Text == local.GetName().Text)
                        {
                            decl.GetName().Text += "F";
                            local.GetName().Text += "L";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        continue;

                    //Structs
                    foreach (SharedData.DeclItem<AStructDecl> declItem in finalTrans.data.Structs)
                    {
                        AStructDecl decl = declItem.Decl;
                        if (decl.GetName().Text == local.GetName().Text)
                        {
                            decl.GetName().Text += "S";
                            local.GetName().Text += "L";
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                        continue;
                }
            }
        }
    }
}
