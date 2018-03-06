using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2
{
    public partial class SearcDefinitionsForm : AutoSizeForm
    {
        public SearcDefinitionsForm()
        {
            InitializeComponent();
            Height = maxHeight = panel1.Bottom;

            List<string> types = new List<string>();
            types.AddRange(GalaxyKeywords.Primitives.words);
            foreach (SourceFileContents file in Form1.Form.compiler.ParsedSourceFiles)
            {
                foreach (StructDescription str in file.Structs)
                {
                    types.Add(str.Name);
                }
            }

            types.Sort();
            CB_RType.Items.AddRange(types.ToArray());

            LB_Types.Items.AddRange(types.ToArray());
        }

        private readonly int maxHeight;

        private void CB_Method_CheckedChanged(object sender, EventArgs e)
        {
            GB_Params.Visible = CB_Method.Checked;
            ClientSize = new Size(ClientSize.Width, maxHeight - (GB_Params.Visible ? 0 : GB_Params.Height) - (GB_Type.Visible ? 0 : GB_Type.Height));
            Search();
        }

        private void CB_Type_CheckedChanged(object sender, EventArgs e)
        {
            GB_Type.Visible = !CB_Type.Checked;
            ClientSize = new Size(ClientSize.Width, maxHeight - (GB_Params.Visible ? 0 : GB_Params.Height) - (GB_Type.Visible ? 0 : GB_Type.Height));
            Search();
        }





        private void Search()
        {
            List<string> nameContains = new List<string>();
            string namePrefix;
            string namePostfix;
            string type;
            List<string> parameters = new List<string>();

            //Name contains
            nameContains.AddRange(TB_Contains.Text.ToLower().Split(',').Select(str => str.Trim()));
            //Prefix
            namePrefix = TB_StartsWith.Text.ToLower().Trim();
            //Postfix
            namePostfix = TB_EndsWith.Text.ToLower().Trim();
            //Type
            type = (string) (CB_RType.SelectedIndex != -1 ? CB_RType.SelectedItem : "");
            //Params
            parameters.AddRange(LB_Parameters.Items.Cast<string>());

            List<string> output = new List<string>();
            if (CB_Method.Checked)
            {
                foreach (SourceFileContents file in Form1.Form.compiler.ParsedSourceFiles)
                {
                    foreach (MethodDescription method in file.Methods)
                    {
                        //Check name
                        bool match = true;
                        foreach (string contain in nameContains)
                        {
                            if (contain == "") continue;
                            if (!method.Name.ToLower().Contains(contain))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        if (namePrefix != "" && !method.Name.ToLower().StartsWith(namePrefix))
                            continue;
                        if (namePostfix != "" && !method.Name.ToLower().EndsWith(namePostfix))
                            continue;

                        //Check return type
                        if (type != "" && type != method.ReturnType)
                            continue;

                        //Parameters
                        List<string> reqParams = new List<string>();
                        reqParams.AddRange(parameters);
                        foreach (VariableDescription formal in method.Formals)
                        {
                            for (int i = 0; i < reqParams.Count; i++)
                            {
                                if (formal.Type == reqParams[i])
                                {
                                    reqParams.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        if (reqParams.Count > 0)
                            continue;

                        string ret = method.ReturnType + " " + method.Name + "(";
                        //!FIX!if (method.ParentFile.Namespace != null)
                        //    ret = method.ParentFile.Namespace + "." + ret;
                        bool first = true;
                        foreach (VariableDescription formal in method.Formals)
                        {
                            if (!first)
                                ret += ", ";
                            ret += formal.Type;
                            first = false;
                        }
                        ret += ")";
                        output.Add(ret);
                    }
                }
                foreach (AMethodDecl method in Form1.Form.compiler.libraryData.Methods)
                {
                    bool match = true;
                    foreach (string contain in nameContains)
                    {
                        if (contain == "") continue;
                        if (!method.GetName().Text.ToLower().Contains(contain))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match) continue;
                    if (namePrefix != "" && !method.GetName().Text.ToLower().StartsWith(namePrefix))
                        continue;
                    if (namePostfix != "" && !method.GetName().Text.ToLower().EndsWith(namePostfix))
                        continue;

                    //Check return type
                    if (type != "" && type != Util.TypeToString(method.GetReturnType()))
                        continue;

                    //Parameters
                    List<string> reqParams = new List<string>();
                    reqParams.AddRange(parameters);
                    foreach (AALocalDecl formal in method.GetFormals())
                    {
                        for (int i = 0; i < reqParams.Count; i++)
                        {
                            if (Util.TypeToString(formal.GetType()) == reqParams[i])
                            {
                                reqParams.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    if (reqParams.Count > 0)
                        continue;


                    string ret = Util.TypeToString(method.GetReturnType()) + " " + method.GetName().Text + "(";

                    bool first = true;
                    foreach (AALocalDecl formal in method.GetFormals())
                    {
                        if (!first)
                            ret += ", ";
                        ret += Util.TypeToString(formal.GetType());
                        ret += " " + formal.GetName().Text;
                        first = false;
                    }
                    ret += ")";
                    output.Add(ret);
                }
            }
            else if (CB_Field.Checked)
            {
                foreach (SourceFileContents file in Form1.Form.compiler.ParsedSourceFiles)
                {
                    foreach (VariableDescription field in file.Fields)
                    {
                        if (field.VariableType != VariableDescription.VariableTypes.Field)
                            continue;

                        //Check name
                        bool match = true;
                        foreach (string contain in nameContains)
                        {
                            if (contain == "") continue;
                            if (!field.Name.ToLower().Contains(contain))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        if (namePrefix != "" && !field.Name.ToLower().StartsWith(namePrefix))
                            continue;
                        if (namePostfix != "" && !field.Name.ToLower().EndsWith(namePostfix))
                            continue;

                        //Check return type
                        if (type != "" && type != field.Type)
                            continue;
                        //!FIX!output.Add((field.ParentFile.Namespace != null ? field.ParentFile.Namespace + "." : "") + field.Type + " " + field.Name);
                    }
                }
                foreach (AFieldDecl field in Form1.Form.compiler.libraryData.Fields)
                {
                    bool match = true;
                    foreach (string contain in nameContains)
                    {
                        if (contain == "") continue;
                        if (!field.GetName().Text.ToLower().Contains(contain))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match) continue;
                    if (namePrefix != "" && !field.GetName().Text.ToLower().StartsWith(namePrefix))
                        continue;
                    if (namePostfix != "" && !field.GetName().Text.ToLower().EndsWith(namePostfix))
                        continue;

                    //Check return type
                    if (type != "" && type != Util.TypeToString(field.GetType()))
                        continue;

                    output.Add(Util.TypeToString(field.GetType()) + " " + field.GetName().Text);
                }
            }
            else//Types
            {
                foreach (SourceFileContents file in Form1.Form.compiler.ParsedSourceFiles)
                {
                    foreach (StructDescription str in file.Structs)
                    {
                        //Check name
                        bool match = true;
                        foreach (string contain in nameContains)
                        {
                            if (contain == "") continue;
                            if (!str.Name.ToLower().Contains(contain))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        if (namePrefix != "" && !str.Name.ToLower().StartsWith(namePrefix))
                            continue;
                        if (namePostfix != "" && !str.Name.ToLower().EndsWith(namePostfix))
                            continue;

                        output.Add(str.Name);
                    }
                }
                foreach (string primitive in GalaxyKeywords.Primitives.words)
                {
                    bool match = true;
                    foreach (string contain in nameContains)
                    {
                        if (contain == "") continue;
                        if (!primitive.ToLower().Contains(contain))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match) continue;
                    if (namePrefix != "" && !primitive.ToLower().StartsWith(namePrefix))
                        continue;
                    if (namePostfix != "" && !primitive.ToLower().EndsWith(namePostfix))
                        continue;


                    output.Add(primitive);
                }
            }

            output.Sort();
            TB_Output.Text = output.Aggregate("", (str, cur) => str + cur + "\n");
        }

        private void InputChanged(object sender, EventArgs e)
        {
            Search();
        }

        private void BTN_AddParameter_Click(object sender, EventArgs e)
        {
            if (LB_Types.SelectedIndex == -1) return;
            LB_Parameters.Items.Add(LB_Types.SelectedItem);
            Search();
        }

        private void BTN_RemoveParameters_Click(object sender, EventArgs e)
        {
            if (LB_Parameters.SelectedIndex == -1) return;
            LB_Parameters.Items.RemoveAt(LB_Parameters.SelectedIndex);
            Search();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}
