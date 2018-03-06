using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using FarsiLibrary.Win;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Dialog_Creator
{
    [Serializable]
    class DialogData
    {
        [NonSerialized]
        private GraphicsControl currentControl;
        public GraphicsControl CurrentControl
        {
            get { return currentControl; }
            set
            {
                currentControl = value;
                foreach (Dialog item in Dialogs)
                {
                    item.ContextChanged(value);
                }
            }
        }

        [NonSerialized]
        public TreeNode GUINode;
        [NonSerialized]
        public FATabStripItem TabPage;
        [NonSerialized]
        public Control DialogControl;
        [NonSerialized]
        public DialogItem DialogItem;
        [NonSerialized]
        private bool changed;
        public bool Changed
        {
            get { return changed; }
            set
            {
                changed = value;
                if (value)
                {
                    ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;

                }
            }
        }

        public void UpdateDesigener()
        {
            Form1.Form.compiler.DialogItemChanged(DialogItem, null, true);
        }

        [NonSerialized]
        public TreeNode CodeGUINode;
        [NonSerialized]
        public FATabStripItem CodeTabPage;
        [NonSerialized]
        public MyEditor CodeEditor;
        [NonSerialized]
        private bool codeChanged;
        public bool CodeChanged
        {
            get { return codeChanged; }
            set
            {
                if (!codeChanged && value)
                {
                    CodeTabPage.Title += "*";
                }
                codeChanged = value;
                if (value)
                    ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
            }
        }

        [NonSerialized]
        public TreeNode DesignerGUINode;
        [NonSerialized]
        public FATabStripItem DesignerTabPage;
        [NonSerialized]
        public MyEditor DesignerEditor;



        public List<Dialog> Dialogs = new List<Dialog>();

        public string Code = "namespace Dialogs\n\n";

        internal string ActualCode
        {
            get { return CodeEditor == null ? Code : CodeEditor.Text; }
            set
            {
                if (CodeEditor == null)
                    Form1.Form.OpenFile(DialogItem, DialogItem.CodeGUINode);
                CodeEditor.Text = value;
                CodeChanged = true;
            }
        }

        public uint? MaxInstances;

        

        public void InsertEvent(string name, bool jumpToIt)
        {
            //Look for method with signature
            //void <name>(int sender, <dialogname>* dialog);
            //If found, set caret to that place, if not - append it and set carret

            //Don't assume the code can compile
            string code = ActualCode;
            int lines = 0;
            foreach (char c in code)
            {
                if (c == '\n')
                    lines++;
            }
            TextPoint focusAt = new TextPoint(-1, -1);
            using (StringReader reader = new StringReader(code))
            {
                Lexer lexer = new Lexer(reader);
                List<Token> tokens = new List<Token>();
                Token token = null;
                while ((token = lexer.Next()) != null)
                {
                    if (token is EOF)
                        break;
                    if (token is TWhiteSpace || token is TTraditionalComment || token is TEndOfLineComment)
                        continue;
                    tokens.Add(token);
                }
                for (int i = 0; i < tokens.Count - 11; i++)
                {
                    token = tokens[i];
                    if (token is TVoid)
                    {
                        token = tokens[i + 1];
                        if (token is TIdentifier && token.Text == name && tokens[i + 2] is TLParen)
                        {
                            token = tokens[i + 3];
                            if (token is TIdentifier && token.Text == "int" && 
                                tokens[i + 4] is TIdentifier && tokens[i + 5] is TComma)
                            {
                                token = tokens[i + 6];
                                if (token is TIdentifier && token.Text == DialogIdentiferName &&
                                    tokens[i + 7] is TStar && tokens[i + 8] is TIdentifier &&
                                    tokens[i + 9] is TRParen && tokens[i + 10] is TLBrace)
                                {
                                    focusAt = TextPoint.FromCompilerCoords(tokens[i + 10]);
                                }
                            }

                        }
                    }
                }
            }

            if (focusAt.Line == -1)
            {//Didn't find it. Append it
                jumpToIt = true;//This should not be auto saved. Have to open it.
                string s = "\n\nvoid " + name + "(int sender, " + DialogIdentiferName + "* dialog)\n" +
                           "{\n" +
                           "\t\n" +
                           "}\n";
                Form1.Form.OpenFile(DialogItem, DialogItem.CodeGUINode);
                CodeEditor.Text += s;
                focusAt = new TextPoint(lines + 4, 1);
            }
            else if (jumpToIt)
                Form1.Form.OpenFile(DialogItem, DialogItem.CodeGUINode);
            if (jumpToIt)
                CodeEditor.MoveCaretTo(focusAt);
        }

        public string DesignerCode
        {
            get
            {
                /*
                 * namespace Dialogs
                 * 
                 * class[42] MyDialog
                 * {
                 *     int MainDialog;
                 *     int BTNOk;
                 *     int TBOrder;
                 *     int OtherDialog;
                 *     
                 *     MyDialog()
                 *     {
                 *         MainDialog = DialogCreate(...);
                 *         DialogSetImage(MainDialog, "...");
                 *         ...
                 *         TriggerAddEventDialogItem(BTNOk, BTNOk_Pressed, c_pressed);
                 *         ...
                 *         //The ## means cast to int. #MyDialog*# would cast an int back to a pointer to this dialog.
                 *         //I added it to be able to save the pointer in the data table, but I don't think it looks nice,
                 *         //so I didn't advertise it.
                 *         DataTableSetInt(true, "Dialogs\\MyDialog\\" + MainDialog, ##this);
                 *         DataTableSetInt(true, "Dialogs\\MyDialog\\" + OtherDialog, ##this);
                 *     }
                 *     
                 *     ~MyDialog()
                 *     {
                 *          DialogDestroy(MainDialog);
                 *          DialogDestroy(OtherDialog);
                 *     }
                 *     
                 *     void SetVisible(int player, bool visible)
                 *     {
                 *         SetVisible(PlayerGroupSingle(player), visible);
                 *     }
                 *     
                 *     void SetVisible(playergroup players, bool visible)
                 *     {
                 *         ...
                 *     }
                 * }
                 * 
                 * Trigger BTNOk_Pressed
                 * {
                 *     actions
                 *     {
                 *         int dialog = DialogControlGetDialog(EventDialogControl());
                 *         MyDialog* d = #MyDialog*#DataTableGetInt(true, "Dialogs\\MyDialog\\" + dialog);
                 *         MyBTNOk_PressedFunc(EventDialogControl(), d);
                 *     }
                 * }
                 */
                StringBuilder builder = new StringBuilder();
                //Namespace decl
                builder.AppendLine("namespace Dialogs");
                builder.AppendLine();
                //Struct decl
                builder.Append("class");
                if (MaxInstances != null)
                {
                    builder.Append("[");
                    builder.Append(MaxInstances);
                    builder.Append("]");
                }
                builder.Append(" ");
                builder.AppendLine(DialogIdentiferName);
                builder.AppendLine("{");

                //add a tag for user identification
                builder.AppendLine("\tint mTag;");

                //Local decls
                foreach (Dialog dialog in Dialogs)
                {
                    builder.AppendLine(dialog.VariableDeclaration);
                    foreach (DialogControl control in dialog.ChildControls)
                    {
                        builder.AppendLine(control.VariableDeclaration);
                    }
                }
                builder.AppendLine();
                //Constructor
                builder.Append("\t");
                builder.Append(DialogIdentiferName);
                builder.AppendLine("()");
                builder.AppendLine("\t{");
                //First do dialogs
                foreach (Dialog dialog in Dialogs)
                {
                    dialog.PrintInitialization(builder);
                    //Add to data table
                    //DataTableSetInt(true, "Dialogs\\MyDialog\\" + MainDialog, ##this);
                    builder.Append("\t\tDataTableSet");
                    if (MaxInstances == null)
                        builder.Append("String");
                    else
                        builder.Append("Int");
                    builder.Append("(true, \"Dialogs\\\\");
                    builder.Append(DialogIdentiferName);
                    builder.Append("\\\\\" + ");
                    builder.Append(dialog.Name);
                    builder.AppendLine(", ##this);");

                    foreach (DialogControl control in dialog.ChildControls)
                    {
                        control.PrintInitialization(builder);
                    }
                }
                builder.AppendLine("\t}");
                builder.AppendLine("\t");
                //Deconstructor
                builder.Append("\t~");
                builder.Append(DialogIdentiferName);
                builder.AppendLine("()");
                builder.AppendLine("\t{");
                foreach (Dialog dialog in Dialogs)
                {
                    //Remove data table entry
                    builder.Append("\t\tDataTableValueRemove(true, \"Dialogs\\\\");
                    builder.Append(DialogIdentiferName);
                    builder.Append("\\\\\" + ");
                    builder.Append(dialog.Name);
                    builder.AppendLine(");");
                    //Remove all dialog controls
                    builder.Append("\t\tDialogControlDestroyAll(");
                    builder.Append(dialog.Name);
                    builder.AppendLine(");");
                    //Remove dialog
                    builder.Append("\t\tDialogDestroy(");
                    builder.Append(dialog.Name);
                    builder.AppendLine(");");
                    builder.AppendLine("\t\t");
                }
                builder.AppendLine("\t}");
                builder.AppendLine("\t");
                //Set visible player
                builder.AppendLine("\tinline void SetVisible(int player, bool visible)");
                builder.AppendLine("\t{");
                builder.AppendLine("\t\tSetVisible(PlayerGroupSingle(player), visible);");
                builder.AppendLine("\t}");
                builder.AppendLine("\t");
                //Set visible player group
                builder.AppendLine("\tvoid SetVisible(playergroup players, bool visible)");
                builder.AppendLine("\t{");
                foreach (Dialog dialog in Dialogs)
                {
                    builder.Append("\t\tDialogSetVisible(");
                    builder.Append(dialog.Name);
                    builder.AppendLine(", players, visible);");
                }
                builder.AppendLine("\t}");
                //to avoid the event being called by initialization in construction, we need to do async for trigger adding
                //InvokeAsync<TriggerAddEventDialogControl>(DBoard_onOff_OnChecked, c_playerAny, onOff, c_triggerControlEventTypeChecked);
                builder.AppendLine("\t");
                builder.AppendLine("\tprivate void AddEventDialogControl(trigger callback, int playerType,int controlId,int eventType)");
                builder.AppendLine("\t{");
                builder.AppendLine("\t\tWait(5,c_timeGame);//");
                builder.AppendLine("\t\tTriggerAddEventDialogControl(callback, playerType, controlId, eventType);");
                builder.AppendLine("\t}");
                builder.AppendLine("}");
                builder.AppendLine();
                
                 
                //Add events
                foreach (Dialog dialog in Dialogs)
                {
                    foreach (DialogControl item in dialog.ChildControls)
                    {
                        foreach (KeyValuePair<string, string> pair in item.Events)
                        {
                            builder.Append("Trigger ");
                            builder.Append(DialogIdentiferName);
                            builder.Append("_");
                            builder.Append(item.Name);
                            builder.Append("_");
                            builder.AppendLine(pair.Key);
                            builder.AppendLine("{");
                            builder.AppendLine("\tactions");
                            builder.AppendLine("\t{");
                            builder.Append("\t\t");
                            builder.Append(pair.Value);
                            builder.Append("(EventDialogControl(), #");
                            builder.Append(DialogIdentiferName);
                            builder.Append("*#DataTableGet");
                            if (MaxInstances == null)
                                builder.Append("String");
                            else
                                builder.Append("Int");
                            builder.Append("(true, \"Dialogs\\\\");
                            builder.Append(DialogIdentiferName);
                            builder.AppendLine("\\\\\" + DialogControlGetDialog(EventDialogControl())));");
                            builder.AppendLine("\t}");
                            builder.AppendLine("}");
                            builder.AppendLine();
                        }
                    }
                }
                return builder.ToString();
            }
        }

        public string DialogName
        {
            get { return DialogItem.Name.Substring(0, DialogItem.Name.LastIndexOf(".")); }
        }

        public string DialogIdentiferName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                string n = DialogName;
                foreach (char c in n)
                {
                    if (!Util.IsIdentifierLetter(c))
                        continue;
                    if (c >= '0' && c <= '9' && builder.Length == 0)
                        builder.Append("D");
                    builder.Append(c);
                }
                return builder.ToString();
            }
        }

        public List<string> GetAllTargetMethods()
        {
            List<string> returner = new List<string>();
            using (StringReader reader = new StringReader(ActualCode))
            {
                Lexer lexer = new Lexer(reader);
                List<Token> tokens = new List<Token>();
                Token token = null;
                while ((token = lexer.Next()) != null)
                {
                    if (token is EOF)
                        break;
                    if (token is TWhiteSpace || token is TTraditionalComment || token is TEndOfLineComment)
                        continue;
                    tokens.Add(token);
                }
                for (int i = 0; i < tokens.Count - 11; i++)
                {
                    token = tokens[i];
                    if (token is TVoid && tokens[i + 1] is TIdentifier && tokens[i + 2] is TLParen)
                    {
                        token = tokens[i + 3];
                        if (token is TIdentifier && token.Text == "int" &&
                            tokens[i + 4] is TIdentifier && tokens[i + 5] is TComma)
                        {
                            token = tokens[i + 6];
                            if (token is TIdentifier && token.Text == DialogIdentiferName &&
                                tokens[i + 7] is TStar && tokens[i + 8] is TIdentifier &&
                                tokens[i + 9] is TRParen && tokens[i + 10] is TLBrace)
                            {
                                returner.Add(tokens[i + 1].Text);
                            }
                        }
                    }
                }
            }
            return returner;
        }

        public delegate void SaveDelegate(string path);

        public void Save(string path)
        {
            if (Changed && TabPage != null)
            {
                if (TabPage.InvokeRequired)
                {
                    TabPage.Invoke(new SaveDelegate(Save), path);
                    return;
                }
                TabPage.Title = DialogItem.Name;
                Changed = false;
            }
            if (CodeChanged && CodeTabPage != null)
            {
                if (CodeTabPage.InvokeRequired)
                {
                    CodeTabPage.Invoke(new SaveDelegate(Save), path);
                    return;
                }
                CodeTabPage.Title = DialogItem.Name.Substring(0, DialogItem.Name.LastIndexOf(".Dialog")) + ".galaxy++";
                CodeChanged = false;
                Code = CodeEditor.Text;
            }
            Stream stream = File.Open(path, FileMode.Create);
            //XmlSerializer formatter = new XmlSerializer(typeof(DialogData));
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static DialogData Load(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                try
                {
                    //XmlSerializer formatter = new XmlSerializer(typeof(DialogData));
                    BinaryFormatter formatter = new BinaryFormatter();
                    DialogData data = (DialogData) formatter.Deserialize(stream);
                    foreach (Dialog dialog in data.Dialogs)
                    {
                        dialog.Data = data;
                        dialog.ConsistensyCheck();
                        foreach (DialogControl control in dialog.ChildControls)
                        {
                            control.Data = data;
                            control.ConsistensyCheck();
                        }
                    }
                    return data;
                }
                catch (Exception err)
                {
                }
                finally
                {
                    stream.Close();
                }
            }
            return null;
        }
    }
}
