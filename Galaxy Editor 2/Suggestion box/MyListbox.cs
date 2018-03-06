using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases;
using Galaxy_Editor_2.Compiler.Phases.Transformations;
using Galaxy_Editor_2.Dialog_Creator;
using Galaxy_Editor_2.Editor_control;
using Galaxy_Editor_2.Tooltip;

namespace Galaxy_Editor_2.Suggestion_box
{
    partial class MyListbox : UserControl
    {

        private static int LexiographicSuggestionBoxSorter(SuggestionBoxItem item1, SuggestionBoxItem item2)
        {
            return item1.DisplayText.CompareTo(item2.DisplayText);
        }

        private int RelevanceSorter(SuggestionBoxItem item1, SuggestionBoxItem item2)
        {
            //First sort what matches the prefix, then lexiographically
            if (item1.DisplayText.ToLower().StartsWith(rebuildingData.matchText))
            {
                if (!item2.DisplayText.ToLower().StartsWith(rebuildingData.matchText))
                    return -1;
            }
            else
            {
                if (item2.DisplayText.ToLower().StartsWith(rebuildingData.matchText))
                    return 1;
            }

            //Then sort on match case
            if (item1.DisplayText.StartsWith(rebuildingData.caseSensitiveMatchText))
            {
                if (!item2.DisplayText.StartsWith(rebuildingData.caseSensitiveMatchText))
                    return -1;
            }
            else
            {
                if (item2.DisplayText.StartsWith(rebuildingData.caseSensitiveMatchText))
                    return 1;
            }

            //Next, sort on shortest
            if (item1.DisplayText.Length < item2.DisplayText.Length)
                return -1;
            if (item1.DisplayText.Length > item2.DisplayText.Length)
                return 1;

            //Sort the rest alphabetically
            int cmp = item1.DisplayText.CompareTo(item2.DisplayText);
            if (cmp != 0)
                return cmp;
            //If two are the same, ensure some order is consistently chosen
            return item1.Signature.CompareTo(item2.Signature);
        }

        public new SuggestionBoxForm ParentForm;
        private GalaxyCompiler Compiler;
        internal TooltipForm tooltip;
        internal TooltipForm methodParamTooltip;
        public MyEditor CurrentEditor
        {
            get { return currentEditor; }
            set
            {
                if (value == null) return;
                if (currentEditor != null)
                {
                    currentEditor.PreviewKeyDown -= currentEditor_PreviewKeyDown;
                    currentEditor.KeyDown -= currentEditor_KeyDown;
                    currentEditor.PreviewKeyPress -= currentEditor_KeyPress;
                    currentEditor.SizeChanged -= currentEditor_SizeChanged;
                    currentEditor.OnScrolled -= currentEditor_OnScrolled;
                    currentEditor.OnCaretChanged -= currentEditor_OnCaretChanged;
                }
                currentEditor = value;
                if (currentEditor != null)
                {
                    currentEditor.PreviewKeyDown += currentEditor_PreviewKeyDown;
                    currentEditor.KeyDown += currentEditor_KeyDown;
                    currentEditor.KeyPress += currentEditor_KeyPress;
                    currentEditor.SizeChanged += currentEditor_SizeChanged;
                    currentEditor.OnScrolled += currentEditor_OnScrolled;
                    currentEditor.OnCaretChanged += currentEditor_OnCaretChanged;
                    currentEditor_OnCaretChanged(currentEditor);
                }
                Parent.Visible = false;
            }
        }

        

        private void currentEditor_OnScrolled(MyEditor sender)
        {
            //Reposition
            Reposition();
            
        }

        public ImageList ImageList { get; set; }

        


        
        

        private MyEditor currentEditor;

        //private RedBlackTree<SuggestionBoxItem> globalItems = new RedBlackTree<SuggestionBoxItem>(LexiographicSuggestionBoxSorter);
        
        public int LineOffset { get; set; }
        public int VisibleLineCount { get { return Height/Font.Height; } }
        public int LineCount { get { return currentRebuildData.displayedItems.Count; } }
        private int SelectedIndex
        {
            get { return _selectedIndex; }
            set 
            { 
                _selectedIndex = value;

                //If it is not visible, change scroll bar
                if (_selectedIndex < ParentForm.VScrollValue)
                {
                    LineOffset = ParentForm.VScrollValue = _selectedIndex;
                    Invalidate();
                }
                else if (_selectedIndex >= ParentForm.VScrollValue + VisibleLineCount)
                {
                    LineOffset = ParentForm.VScrollValue = _selectedIndex - VisibleLineCount + 1;
                    Invalidate();
                    
                }

                if (_selectedIndex < 0 || _selectedIndex >= currentRebuildData.displayedItems.Count)
                    tooltip.SetVisible(false);
                else
                {

                    tooltip.Items.Clear();
                    if (currentRebuildData.displayedItems[value].TooltipText != null)
                    {
                        //windywell add content
                        string[] lines = currentRebuildData.displayedItems[value].TooltipText.Split('\n');
                        foreach (string line in lines)
                        {
                            
                            MyToolboxControl.Item item = new MyToolboxControl.Item();
                            item.Sections.Add(new MyToolboxControl.Item.Section(line));
                            tooltip.Items.Add(item);
                        }

                        //windywell add comments
                       // string[] lines =currentRebuildData.displayedItems[value].
                    }
                    if (tooltip.Items.Count == 0 || !ParentForm.Visible)
                        tooltip.SetVisible(false);
                    else
                    {
                        Point pos = new Point(ParentForm.Right, ParentForm.Top);
                        Size size =
                            tooltip.TooltipControl.GetRequiredSize(Screen.PrimaryScreen.WorkingArea.Width - pos.X);
                        tooltip.Show(pos, new Size(size.Width, size.Height));
                        tooltip.Redraw();

                        currentEditor.ime.SetIMEWindowLocation(pos.X, pos.Y + size.Height);
                    }
                }
            }
        }

        private int _selectedIndex;
        private class RebuildData
        {
            public List<StructDescription> targetStructs;
            public List<EnrichmentDescription> targetEnrichments;
            public List<TypedefDescription> targetTypedefs; 
            public string matchText;
            public string caseSensitiveMatchText;
            public bool suggestTypes;
            public bool suggestVariables;
            public bool onlySuggestVariablesInsideMethods;
            public bool onlySuggestMethods;
            public bool onlySuggestInitKeywords;
            public bool suggestKeywords;
            public bool suggestArrayLength;
            public List<NamespaceDescription> namespacePrefixes;
            public bool isDelegateInvoke;
            public bool onlySuggestDelegates;
            public List<StructDescription> staticStructs;
            public bool isDynamicArray;

            public RedBlackTree<SuggestionBoxItem> displayedItems;
            public bool isGlobal;
        }

        RebuildData currentRebuildData = new RebuildData();
        RebuildData rebuildingData = new RebuildData();
        RebuildData nextRebuildingData = new RebuildData();


        public MyListbox()
        {
            tooltip = new TooltipForm();
            tooltip.Owner = Form1.Form;
            methodParamTooltip = new TooltipForm();
            methodParamTooltip.Owner = Form1.Form;
            currentRebuildData.displayedItems = new RedBlackTree<SuggestionBoxItem>(RelevanceSorter);
            InitializeComponent();
            tooltip.GotFocus += tooltip_GotFocus;
            tooltip.TooltipControl.GotFocus += tooltip_GotFocus;
            rebuildThread = new Thread(RebuildThread);
            rebuildThread.Start();

            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Opaque |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw, true);
        }

        

        public void SetCompiler(GalaxyCompiler compiler)
        {
            Compiler = compiler;
            compiler.SourceFileContentsChanged += compiler_SourceFileContentsChanged;
        }

        

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {

            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);
            if (LineOffset >= currentRebuildData.displayedItems.Count)
                LineOffset = currentRebuildData.displayedItems.Count - 1;
            var enumerator = currentRebuildData.displayedItems.GetEnumerator(LineOffset);
            int i = 0;
            int x = 16;

            while (enumerator.MoveNext())
            {
                if (i * FontHeight > e.ClipRectangle.Y + e.ClipRectangle.Height)
                    break;
                //Draw icon
                Image image = null;
                if (enumerator.Current is VariableDescription)
                {//Destinguish field, parameter, local, const/not const
                    VariableDescription variable = (VariableDescription) enumerator.Current;
                    switch (variable.VariableType)
                    {
                        case VariableDescription.VariableTypes.Field:
                        case VariableDescription.VariableTypes.StructVariable:
                            if (variable.Const)
                                image = ImageList.Images["ConstFieldIcon.bmp"];
                            else
                                image = ImageList.Images["FieldIcon.bmp"];
                            break;
                        case VariableDescription.VariableTypes.LocalVariable:
                            if (variable.Const)
                                image = ImageList.Images["ConstLocalIcon.bmp"];
                            else
                                image = ImageList.Images["LocalIcon.bmp"];
                            break;
                        case VariableDescription.VariableTypes.Parameter:
                            image = ImageList.Images["Parameter.bmp"];
                            break;
                    }
                }
                else if (enumerator.Current is MethodDescription ||
                    (enumerator.Current is CustomSuggestionBoxItem &&
                        ((CustomSuggestionBoxItem)enumerator.Current).BaseType == typeof(MethodDescription)))
                {
                    image = ImageList.Images["MethodIcon.bmp"];
                }
                else if (enumerator.Current is StructDescription)
                {
                    image = ImageList.Images["StructIcon.bmp"];
                }
                if (image != null)
                    e.Graphics.DrawImage(image, 0, i*FontHeight);

                if (i + LineOffset == SelectedIndex)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(51, 153, 255)), x + 1, i*Font.Height + 1,
                                             e.ClipRectangle.Width - 2 - x, Font.Height - 2);
                    Pen dashedPen = new Pen(Color.Black, 1);
                    dashedPen.DashStyle = DashStyle.Dot;
                   // dashedPen.DashPattern = new float[]{1};
                    e.Graphics.DrawRectangle(dashedPen, x, i*Font.Height, e.ClipRectangle.Width - 1 - x, Font.Height -1);
                   
                }

                e.Graphics.DrawString(enumerator.Current.DisplayText, Font, Brushes.Black, x, i*Font.Height);
                i++;
            }

            }
            catch (Exception err)
            {

                throw;
            }
        }

        private void currentEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Handle backspace, delete, return, key up down left right, tab, ctrl v
            if (Visible)
            {
                var displayedItems = currentRebuildData.displayedItems;
                if (displayedItems.Count > 0)
                {
                    if (e.KeyData == Keys.Up)
                    {
                        int index = SelectedIndex - 1;
                        if (index < 0)
                            index += displayedItems.Count;
                        SelectedIndex = index;
                        e.Handled = true;
                        Invalidate();
                    }
                    if (e.KeyData == Keys.Down)
                    {
                        SelectedIndex = (SelectedIndex + 1) % displayedItems.Count;
                        e.Handled = true;
                        Invalidate();
                    }
                    if (e.KeyData == Keys.PageUp)
                    {
                        int index = SelectedIndex - VisibleLineCount;
                        if (index < 0)
                        {
                            if (index == -VisibleLineCount)
                                index = displayedItems.Count - 1;
                            else
                                index = 0;
                        }
                        SelectedIndex = index;
                        e.Handled = true;
                        Invalidate();
                    }
                    if (e.KeyData == Keys.PageDown)
                    {
                        int index = (SelectedIndex + VisibleLineCount);
                        if (index >= displayedItems.Count)
                        {
                            if (index == displayedItems.Count + VisibleLineCount - 1)
                                index = 0;
                            else
                                index = Math.Max(0, displayedItems.Count - 1);
                        }
                        SelectedIndex = index;
                        e.Handled = true;
                        Invalidate();
                    }
                }
                if (e.KeyData == Keys.Return)
                {
                    if (SelectedIndex < displayedItems.Count && SelectedIndex >= 0)
                    {
                        InsertSelected();
                        ParentForm.AllowHide = true;
                        ParentForm.Hide();
                        e.Handled = true;
                    }
                }
            }
        }

        

        private void currentEditor_KeyDown(object sender, KeyEventArgs e)
        {
            //Handle backspace, delete, return, key up down left right, tab, ctrl v
            if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Back)
            {
                if (e.KeyData == Keys.Back)
                    SelectedIndex = 0;
                if (Visible)
                {
                    bool visible = ExtractMatchData();
                    if (!visible)
                    {
                        ParentForm.AllowHide = true;
                        ParentForm.Hide();

                        Point pos = currentEditor.GetPixelAtTextpoint(currentEditor.caret.GetPosition(true));
                        currentEditor.ime.SetIMEWindowLocation(pos.X, pos.Y);
                    }
                    else
                    {
                        //Point p = currentEditor.GetPixelAtTextpoint(currentEditor.caret.Position, false);
                        //p = currentEditor.PointToScreen(p);
                        RebuildVisibleList();
                        /*int desiredRows = Math.Max(1, Math.Min(10, displayedItems.Count));
                        Size size = new Size(0, desiredRows*Font.Height);
                        if (SelectedIndex < LineOffset || SelectedIndex + 10 >= LineOffset)
                            LineOffset = SelectedIndex;

                        var enumerator = displayedItems.GetEnumerator(LineOffset);
                        Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                        while (enumerator.MoveNext())
                        {
                            size.Width = Math.Max(size.Width,
                                                  (int) g.MeasureString(enumerator.Current.DisplayText, Font).Width);
                        }
                        size.Width += 16;
                        ParentForm.UpdateAndShow(p, size);
                        Invalidate();*/
                    }
                }
            }
            else if (e.KeyData == (Keys.Space | Keys.Control))
            {
                e.Handled = true;
                ShowSuggestionList();
            }

            if (Visible && (e.KeyData == Keys.Return || e.KeyData == Keys.Tab || (e.Control && e.KeyValue == 'V')))
            {
                ParentForm.AllowHide = true;
                ParentForm.Hide();


                Point pos = currentEditor.GetPixelAtTextpoint(currentEditor.caret.GetPosition(true));
                currentEditor.ime.SetIMEWindowLocation(pos.X, pos.Y);
            }
        }

        public void ShowSuggestionList()
        {
            ParentForm.AllowHide = true;
            bool visible = ExtractMatchData(true);

            if (visible)
            {
                ParentForm.AllowHide = false;
                if (!ParentForm.Visible)
                    SelectedIndex = 0;

                RebuildVisibleList();
            }
            else
                ParentForm.Hide();
        }

        private KeyPressEventArgs lastArgs = null;
        private void currentEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Hide the window if the user didn't enter an identifier letter, or a .
            if (e == lastArgs) return;
            ParentForm.AllowHide = true;
            lastArgs = e;
            bool visible;
            
            if (e.KeyChar != '.' && e.KeyChar != '>' && !Util.IsIdentifierLetter(e.KeyChar) && e.KeyChar < 0x100)
                visible = false;
            else
            {
                visible = ExtractMatchData();
                  
            }
            if (visible)
            {
                ParentForm.AllowHide = false;
                if (!ParentForm.Visible)
                    SelectedIndex = 0;

                RebuildVisibleList();
            }
            else
                ParentForm.Hide();
        }

        

        private void currentEditor_OnCaretChanged(MyEditor sender)
        {
            //Display method arg help if in a method invocation arg
            int commaCount;
            TextPoint parenPos;
            bool isAsyncInvoke;
            List<AMethodDecl> matchingMethods = GetMatchingCurrentMethod(out commaCount, out parenPos, out isAsyncInvoke);
            methodParamTooltip.Items.Clear();
            foreach (AMethodDecl method in matchingMethods)
            {
                var item = new MyToolboxControl.Item();
                string prefix = "", bold = "", postfix = "";
                prefix = "(";
                for (int i = 0; i < method.GetFormals().Count; i++)
                {
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                    string text = Util.TypeToString(formal.GetType()) + " " + formal.GetName().Text;
                    if (i < commaCount && i < method.GetFormals().Count - 1)
                        text += ", ";
                    if (i > commaCount)
                        text = ", " + text;
                    if (i < commaCount)
                        prefix += text;
                    else if (i == commaCount)
                        bold += text;
                    else
                        postfix += text;
                }
                postfix += ") : " + (isAsyncInvoke ? "void" : Util.TypeToString(method.GetReturnType()));
                if (bold == "") 
                {
                    item.Sections.Add(new MyToolboxControl.Item.Section(prefix + postfix));
                }
                else
                {
                    item.Sections.Add(new MyToolboxControl.Item.Section(prefix));
                    item.Sections.Add(new MyToolboxControl.Item.Section(bold, FontStyle.Bold));
                    item.Sections.Add(new MyToolboxControl.Item.Section(postfix));
                }
                methodParamTooltip.Items.Add(item);
            }
            if (methodParamTooltip.Items.Count == 0)
            {
                methodParamTooltip.Visible = false;
                return;
            }
            Point pos = CurrentEditor.GetPixelAtTextpoint(parenPos);
            pos = CurrentEditor.PointToScreen(pos);
            Size size = methodParamTooltip.TooltipControl.GetRequiredSize(Screen.PrimaryScreen.Bounds.Width - pos.X);
            pos.Y -= size.Height;
            methodParamTooltip.Show(pos, size);
            methodParamTooltip.Redraw();
        }

        private List<AMethodDecl> GetMatchingCurrentMethod(out int commaCount, out TextPoint parenPos, out bool isAsyncInvoke)
        {
            isAsyncInvoke = false;
            commaCount = 0;
            parenPos = new TextPoint(0, 0);
            string text = currentEditor.GetTextWithin(new TextPoint(0, 0), currentEditor.caret.Position);

            Lexer lexer = new Lexer(new StringReader(text));
            List<Token> tokens = new List<Token>();
            Token token;
            bool inString = false;
            while (true)
            {
                try
                {
                    token = lexer.Next();
                    if (token is EOF)
                        break;
                }
                catch (Exception err)
                {
                    continue;
                }
                //We can be in a string if there was an unclosed " on this line
                if (token is TUnknown && token.Text == "\"" || token.Text == "'")
                    inString = true;
                if (token is TWhiteSpace)
                {
                    if (token.Text == "\n")
                        inString = false;
                    continue;
                }
                //If this is true, we have an open block comment with no end, so we're in a comment
                if (token is TCommentBegin)
                    return new List<AMethodDecl>();
                //if (token is TArrow)
                //    token = new TDot(".");

                tokens.Add(token);
            }
            if (inString)
            {
                while (!(token is TUnknown && token.Text == "\"" || token.Text == "'"))
                {
                    token = tokens[tokens.Count - 1];
                    tokens.RemoveAt(tokens.Count - 1);
                } 
                /*token = tokens[tokens.Count - 1];
                tokens.RemoveAt(tokens.Count - 1);*/
            }
            if (tokens.Count == 0) return new List<AMethodDecl>();
            //The last entered token can either be an identifier, a dot, or a line comment
            token = tokens[tokens.Count - 1];
            tokens.RemoveAt(tokens.Count - 1);
            if (token is TEndOfLineComment)
                return new List<AMethodDecl>();
            //Remove all other comments
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is TTraditionalComment || tokens[i] is TEndOfLineComment || tokens[i] is TDocumentationComment)
                {
                    tokens.RemoveAt(i);
                    i--;
                }
            }

            //Go back untill you encounter a identifier lparen, and count all commas outside of paranthisis or brackets
            //You can stop if you encounter one of ; { } 
            commaCount = 0;
            int openParens = 0;

            while (tokens.Count > 0)
            {
                if (token is TLParen)
                {
                    if (openParens > 0)
                    {
                        openParens--;
                    }
                    else
                    {
                        parenPos = new TextPoint(token.Line - 1, token.Pos - 1);
                        token = tokens[tokens.Count - 1];
                        tokens.RemoveAt(tokens.Count - 1);
                        if (token is TPreloadBank)
                        {
                            return new List<AMethodDecl>(){new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                  new ANamedType(new TIdentifier("void"), null),
                                                                  new TIdentifier("PreloadBank"),
                                                                  new ArrayList
                                                                              {
                                                                                  new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                                  new ANamedType(
                                                                                                      new TIdentifier(
                                                                                                          "string"),
                                                                                                      null),
                                                                                                  new TIdentifier(
                                                                                                      "bankName"), null),
                                                                                  new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                                  new ANamedType(
                                                                                                      new TIdentifier(
                                                                                                          "int"), null),
                                                                                                  new TIdentifier(
                                                                                                      "player"), null)
                                                                              },
                                                                  null)};
                        }
                        if (token is TGt)
                        {//Check for invoke<...>
                            int pos = tokens.Count - 1;
                            if (tokens[pos] is TIdentifier)
                            {
                                pos--;
                                if (tokens[pos] is TDot && tokens[pos - 1] is TIdentifier)
                                {
                                    pos -= 2;
                                }
                                if (tokens[pos] is TLt)
                                {
                                    pos--;
                                    if (tokens[pos] is TSyncInvoke || tokens[pos] is TAsyncInvoke)
                                    {
                                        isAsyncInvoke = tokens[pos - 1] is TAsyncInvoke;

                                        token = tokens[tokens.Count - 1];
                                        tokens.RemoveAt(tokens.Count - 1);
                                    }
                                }
                            }
                        }
                        //Check for new exp
                        {
                            int parens = 0;
                            for (int i = tokens.Count - 1; i >= 0; i--)
                            {
                                if (tokens[i] is TRParen || tokens[i] is TRBracket)
                                    parens++;
                                else if (tokens[i] is TLParen || tokens[i] is TLBracket)
                                    parens--;
                                else if (parens > 0)
                                    continue;
                                else if (parens < 0)
                                    break;
                                else if (tokens[i] is TSemicolon || tokens[i] is TRBrace || tokens[i] is TLBrace)
                                    break;
                                else if (tokens[i] is TNew)
                                {
                                    string txt = "";
                                    for (int j = 0; j < tokens.Count; j++)
                                    {
                                        txt += tokens[j].Text + " ";
                                    }
                                    txt += token.Text + ".";
                                    List<StructDescription> targetStructs;
                                    List<EnrichmentDescription> targetEnrichments;
                                    List<NamespaceDescription> targetNamespaces;
                                    List<MethodDescription> delegateTargets;
                                    bool b;
                                    List<StructDescription> l;

                                    ExtractTargetType(txt, out targetStructs, out targetEnrichments, out targetNamespaces,
                                                        out b, out l, out b, out delegateTargets, out b);
                                    
                                    List<AMethodDecl> methods = new List<AMethodDecl>();
                                    foreach (StructDescription targetStruct in targetStructs)
                                    {
                                        foreach (MethodDescription constructor in targetStruct.Constructors)
                                        {
                                            methods.Add(constructor.Decl);
                                        }
                                    }

                                    foreach (EnrichmentDescription enrichment in targetEnrichments)
                                    {
                                        foreach (MethodDescription constructor in enrichment.Constructors)
                                        {
                                            methods.Add(constructor.Decl);
                                        }
                                    }

                                    return methods;
                                }
                            }
                        }
                        if (token is TIdentifier)
                        {
                            List<AMethodDecl> methods = new List<AMethodDecl>();
                            string methodName = token.Text;
                            if (tokens.Count == 0)
                                return new List<AMethodDecl>();
                            token = tokens[tokens.Count - 1];
                            tokens.RemoveAt(tokens.Count - 1);
                            //If we are in a method decl, we dont want to show anything
                            if (token is TIdentifier || token is TVoid)
                                return new List<AMethodDecl>();
                            bool isGlobal = false;
                            if (token is TDot || token is TArrow)
                            {//It is a struct method.. fetch the method
                                //bool b1, b2, b3;
                                //string namespacePrefix;
                                //MethodDescription delegateTarget;
                                //StructDescription target = GetTargetStruct(token, tokens, out isGlobal, out b1, out namespacePrefix, out b2, out delegateTarget, out b3);
                                
                                string strText = "";
                                {
                                    int parens = 0;
                                    for (int i = text.Length - 1; i >= 0; i--)
                                    {
                                        if (parens == -1)
                                        {
                                            if (text[i] == '.')
                                            {
                                                strText = text.Substring(0, i + 1);
                                                break;
                                            }
                                            if (text.Substring(i, 2) == "->")
                                            {
                                                strText = text.Substring(0, i + 2);
                                                break;
                                            }
                                        }
                                        if (parens < -1)
                                            return new List<AMethodDecl>();
                                        if (text[i] == ')')
                                            parens++;
                                        if (text[i] == '(')
                                            parens--;
                                    }
                                }
                                List<StructDescription> targetStructs;
                                List<EnrichmentDescription> targetEnrichments;
                                List<NamespaceDescription> targetNamespaces;
                                List<MethodDescription> delegateTargets;
                                bool b;
                                bool dynamicArray;
                                List<StructDescription> l;
                                ExtractTargetType(strText, out targetStructs, out targetEnrichments, out targetNamespaces,
                                                    out b, out l, out b, out delegateTargets, out dynamicArray);

                                if (dynamicArray && methodName == "Resize")
                                {
                                    methods.Add(new AMethodDecl(new APublicVisibilityModifier(), null, null, null,
                                                                    null, null, new AVoidType(new TVoid("void")),
                                                                    new TIdentifier("Resize"),
                                                                    new ArrayList()
                                                                        {
                                                                            new AALocalDecl(
                                                                                new APublicVisibilityModifier(), null,
                                                                                null, null, null,
                                                                                new ANamedType(
                                                                                    new TIdentifier("int"), null),
                                                                                new TIdentifier("size"), null)
                                                                        },
                                                                    new AABlock(new ArrayList(), new TRBrace("}"))));
                                }
                                foreach (StructDescription str in targetStructs)
                                {
                                    StructDescription targetStruct = str;
                                    StructDescription initTarget = str;
                                    while (targetStruct != null)
                                    {
                                        foreach (MethodDescription method in targetStruct.Methods)
                                        {
                                            if (!method.IsDelegate && method.Name == methodName)
                                                methods.Add(method.Decl);
                                        }
                                        targetStruct = targetStruct.Base;
                                        if (targetStruct == initTarget)
                                            break;
                                    }
                                }

                                foreach (EnrichmentDescription enrichment in targetEnrichments)
                                {
                                    foreach (MethodDescription method in enrichment.Methods)
                                    {
                                        if (!method.IsDelegate && method.Name == methodName)
                                            methods.Add(method.Decl);
                                    }
                                }
                                foreach (MethodDescription method in delegateTargets)
                                {
                                    methods.Add(method.Decl);
                                }

                                //Go back past all dot identifier, and see if you find a new
                                
                                if (targetNamespaces.Count > 0)
                                {
                                    bool foundNewToken = false;
                                    while (tokens.Count > 0)
                                    {
                                        token = tokens[tokens.Count - 1];
                                        tokens.RemoveAt(tokens.Count - 1);

                                        if (token is TIdentifier || token is TDot)
                                            continue;

                                        if (token is TNew)
                                            foundNewToken = true;
                                        break;
                                    }

                                    if (foundNewToken)
                                    {
                                        foreach (NamespaceDescription ns in targetNamespaces)
                                        {
                                            foreach (StructDescription str in ns.Structs)
                                            {
                                                if (str.Name == methodName)
                                                {
                                                    foreach (MethodDescription method in str.Constructors)
                                                    {
                                                        methods.Add(method.Decl);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (NamespaceDescription ns in targetNamespaces)
                                        {
                                            foreach (MethodDescription method in ns.Methods)
                                            {
                                                if (!method.IsDelegate && method.Name == methodName)
                                                    methods.Add(method.Decl);
                                            }
                                        }
                                    }
                                }
                                if (!isGlobal || methods.Count > 0)
                                    return methods;
                            }
                            if (token is TNew)
                            {//Find matching constructors
                                foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                                {
                                    foreach (StructDescription str in file.Structs)
                                    {
                                        if (str.Name == methodName)
                                        {
                                            foreach (MethodDescription constructor in str.Constructors)
                                            {
                                                methods.Add(constructor.Decl);
                                            }
                                            return methods;
                                        }
                                    }
                                }

                            }

                            object openFileData = currentEditor.Tag is Form1.OpenFileData
                                                      ? ((Form1.OpenFileData)currentEditor.Tag).File
                                                      : (object)((DialogData)currentEditor.Tag).DialogItem;
                            SourceFileContents currentSourceFile = null;
                            foreach (SourceFileContents sourceFileContents in Compiler.ParsedSourceFiles)
                            {
                                if (openFileData == sourceFileContents.Item && !sourceFileContents.IsDialogDesigner)
                                {
                                    currentSourceFile = sourceFileContents;
                                    break;
                                }
                            }
                            if (currentSourceFile == null)
                                return new List<AMethodDecl>();
                            if (!isGlobal)
                            {
                                //Current struct
                                int line = CurrentEditor.caret.Position.Line;
                                foreach (StructDescription structDescription in currentSourceFile.Structs)
                                {
                                    if (structDescription.LineFrom <= line && line <= structDescription.LineTo)
                                    {
                                        StructDescription target = structDescription;
                                        StructDescription initTarget = target;
                                        while (target != null)
                                        {
                                            foreach (MethodDescription method in structDescription.Methods)
                                            {
                                                if (!method.IsDelegate && method.Name == methodName)
                                                    methods.Add(method.Decl);
                                            }
                                            target = target.Base;
                                            if (target == initTarget)
                                                break;
                                        }
                                    }
                                }
                                //Current enrichment
                                foreach (EnrichmentDescription enrichment in currentSourceFile.Enrichments)
                                {
                                    if (enrichment.LineFrom <= line && line <= enrichment.LineTo)
                                    {
                                        foreach (MethodDescription method in enrichment.Methods)
                                        {
                                            if (!method.IsDelegate && method.Name == methodName)
                                                methods.Add(method.Decl);
                                        }
                                    }
                                }
                            }
                            if (methods.Count == 0)
                            {
                                //It is a global method
                                foreach (SourceFileContents sourceFile in Compiler.ParsedSourceFiles)
                                {
                                    /*if (!currentSourceFile.CanSeeOther(sourceFile))
                                        continue;*/

                                    foreach (MethodDescription method in sourceFile.Methods)
                                    {
                                        if (method.Visibility is APrivateVisibilityModifier &&
                                            sourceFile.Namespace != currentSourceFile.Namespace)
                                            continue;

                                        if (!method.IsDelegate && method.Name == methodName)
                                            methods.Add(method.Decl);
                                    }
                                }
                                foreach (AMethodDecl method in Compiler.libraryData.Methods)
                                {
                                    if (method.GetName().Text == methodName)
                                        methods.Add(method);
                                }
                            }
                            return methods;
                        }
                        else
                        {
                            commaCount = 0;
                            continue;
                        }
                    }
                }
                if (token is TRParen || token is TRBracket)
                    openParens++;
                if (token is TLBracket)
                {
                    if (openParens > 0)
                        openParens--;
                    else
                        commaCount = 0;
                }
                if (token is TComma && openParens == 0)
                    commaCount++;
                if (token is TSemicolon || token is TLBrace || token is TRBrace)
                    return new List<AMethodDecl>();
                token = tokens[tokens.Count - 1];
                tokens.RemoveAt(tokens.Count - 1);
            }
            return new List<AMethodDecl>();
        }

        private void compiler_SourceFileContentsChanged(SourceFileContents file)
        {
        }

        




        /*private StructDescription GetTargetStruct(Token token, List<Token> tokens, out bool isGlobal, out bool isArray, out string namespacePrefix, out bool isDelegateInvoke, out MethodDescription delegateMethod, out bool isStaticStruct)
        {
            isStaticStruct = false;
            isGlobal = false;
            isArray = false;
            isDelegateInvoke = false;
            delegateMethod = null;
            namespacePrefix = null;
            //Find reciever
            //Reciever can be a connection of the following chunks
            /*
                identifier.
                identifier[...].
                identifier(...).
                identifier(...)[...].
                (start).
             *//*
            //Plan to use this to keep a record of the chunks <type(method/variable), name> 
            //This will not work correctly if overloaded methods can return diffrent types
            List<KeyValuePair<string, string>> chunks = new List<KeyValuePair<string, string>>();

            while (true)
            {//Fetch next chunk
                if (tokens.Count == 0) return null;
                token = tokens[tokens.Count - 1];
                tokens.RemoveAt(tokens.Count - 1);
                //just remove [...]
                if (token is TRBracket)
                {
                    int openBrackets = 1;
                    while (true)
                    {
                        if (tokens.Count == 0) return null;
                        token = tokens[tokens.Count - 1];
                        tokens.RemoveAt(tokens.Count - 1);
                        if (token is TRBracket)
                            openBrackets++;
                        if (token is TLBracket)
                        {
                            openBrackets--;
                            if (openBrackets == 0)
                                break;
                        }
                    }
                    if (tokens.Count == 0) return null;
                    token = tokens[tokens.Count - 1];
                    tokens.RemoveAt(tokens.Count - 1);
                }
                //identifier.
                if (token is TIdentifier)
                {
                    chunks.Add(new KeyValuePair<string, string>("variable", token.Text));
                    //If we got a dot before this, there is more. otherwise stop
                    if (tokens.Count == 0 || !(tokens[tokens.Count - 1] is TDot))
                        break;
                    tokens.RemoveAt(tokens.Count - 1);
                    continue;
                }
                //identifier(...).
                //(start).
                if (token is TRParen)
                {
                    //Look back behind the paranthisis. If there is an identifer, we got a method invocation, otherwise a paren
                    int openParens = 1;
                    int i = tokens.Count - 1;
                    while (true)
                    {
                        if (i == -1) return null;
                        if (tokens[i] is TRParen)
                            openParens++;
                        if (tokens[i] is TLParen)
                        {
                            openParens--;
                            if (openParens == 0)
                                break;
                        }
                        i--;
                    }
                    i--;
                    if (i == -1 || !(tokens[i] is TIdentifier))
                    {//(start).
                        continue;
                    }
                    //identifier(...).
                    chunks.Add(new KeyValuePair<string, string>("method", tokens[i].Text));
                    while (i < tokens.Count)
                        tokens.RemoveAt(tokens.Count - 1);
                    if (tokens.Count == 0 || !(tokens[tokens.Count - 1] is TDot)) break;
                    tokens.RemoveAt(tokens.Count - 1);
                    continue;
                }
                if (token is TEscapeStruct || token is TThis)
                {
                    chunks.Add(new KeyValuePair<string, string>("struct", "struct"));
                    break;
                }
                if (token is TEscapeGlobal)
                {
                    chunks.Add(new KeyValuePair<string, string>("global", "global"));
                    break;
                }
                return null;
            }


            //Now, follow the chunks back to the owner.
            //If first chunk is a variable, check locals and parameters, then globals (remember struct methods are special)
            //If first chunk is a method, look for methods..
            //First, find the sourcefile that matches the open Editor
            Form1.OpenFileData openFileData = (Form1.OpenFileData)currentEditor.Tag;
            SourceFileContents sourceFile = null;
            foreach (SourceFileContents sourceFileContents in Compiler.ParsedSourceFiles)
            {
                if (openFileData.File == sourceFileContents.File)
                {
                    sourceFile = sourceFileContents;
                    break;
                }
            }
            if (sourceFile == null)
                return null;
            //Check if we are in a method
            string currentIdentifierName = chunks[chunks.Count - 1].Value;
            string currentIdentifierType = chunks[chunks.Count - 1].Key;
            chunks.RemoveAt(chunks.Count - 1);
            string nextStructType = null;
            if (currentIdentifierType == "global")
            {
                isGlobal = true;
                if (chunks.Count == 0)
                {
                    return null;
                }
            }
            if (currentIdentifierType == "struct")
            {
                int line = CurrentEditor.caret.Position.Line;
                foreach (StructDescription structDescription in sourceFile.Structs)
                {
                    if (structDescription.LineFrom <= line && line <= structDescription.LineTo)
                    {
                        nextStructType = structDescription.Name;
                        break;
                    }
                }
            }
            else if (currentIdentifierType == "variable")
            {
                //Look in global methods first
                bool insideMethod = false;
                if (!isGlobal)
                {
                    foreach (MethodDescription method in sourceFile.Methods)
                    {
                        if (method.Start < currentEditor.caret.Position &&
                            method.End > currentEditor.caret.Position)
                        {
                            insideMethod = true;
                            foreach (VariableDescription local in method.Locals)
                            {
                                if (local.Name == currentIdentifierName)
                                {
                                    isArray = local.Type.Contains("[");
                                    nextStructType = RemoveArrayType(local.Type);
                                    break;
                                }
                            }
                            if (nextStructType != null)
                                break;
                            foreach (VariableDescription formal in method.Formals)
                            {
                                if (formal.Name == currentIdentifierName)
                                {
                                    isArray = formal.Type.Contains("[");
                                    nextStructType = RemoveArrayType(formal.Type);
                                    break;
                                }
                            }
                            //Can't be inside another method aswell
                            break;
                        }
                    }
                    //Check struct methods aswell
                    if (nextStructType == null && !insideMethod)
                        foreach (StructDescription @struct in sourceFile.Structs)
                        {
                            List<MethodDescription> list = new List<MethodDescription>();
                            list.AddRange(@struct.Methods);
                            list.AddRange(@struct.Constructors);
                            foreach (MethodDescription method in list)
                            {
                                #region repeated stuff

                                if (method.Start < currentEditor.caret.Position &&
                                    method.End > currentEditor.caret.Position)
                                {
                                    insideMethod = true;
                                    foreach (VariableDescription local in method.Locals)
                                    {
                                        if (local.Name == currentIdentifierName)
                                        {
                                            isArray = local.Type.Contains("[");
                                            nextStructType = RemoveArrayType(local.Type);
                                            break;
                                        }
                                    }
                                    if (nextStructType != null)
                                        break;
                                    foreach (VariableDescription formal in method.Formals)
                                    {
                                        if (formal.Name == currentIdentifierName)
                                        {
                                            isArray = formal.Type.Contains("[");
                                            nextStructType = RemoveArrayType(formal.Type);
                                            break;
                                        }
                                    }
                                    //Can't be inside another method aswell
                                    break;
                                }

                                #endregion
                            }
                            //If we were inside this struct [insideMethod == true], but didnt find anything, we should first consider struct globals
                            if (insideMethod && nextStructType == null)
                            {
                                foreach (VariableDescription field in @struct.Fields)
                                {
                                    if (field.Name == currentIdentifierName)
                                    {
                                        isArray = field.Type.Contains("[");
                                        nextStructType = RemoveArrayType(field.Type);
                                        break;
                                    }
                                }
                            }
                            //nextStructType != null => insideMethod == true
                            if (insideMethod)
                                break;
                        }
                }
                
                if (nextStructType == null)
                {
                    //Check globals in all source files
                    foreach (SourceFileContents parsedSourceFile in Compiler.ParsedSourceFiles)
                    {
                        foreach (VariableDescription field in parsedSourceFile.Fields)
                        {
                            if (field.Name == currentIdentifierName)
                            {
                                isArray = field.Type.Contains("[");
                                nextStructType = RemoveArrayType(field.Type);
                                break;
                            }
                        }
                        if (nextStructType != null)
                            break;
                    }
                }

                //Consider Static type
                if (nextStructType == null)
                {
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        foreach (StructDescription str in file.Structs)
                        {
                            if (str.Name == currentIdentifierName)
                            {
                                isArray = false;
                                isStaticStruct = chunks.Count == 0;
                                nextStructType = str.Name;
                                break;
                            }
                        }
                        if (nextStructType != null)
                            break;
                    }
                }

                //Consider namespaces prefix
                if (nextStructType == null)
                {
                    bool foundNamespace = false;
                    string ns = currentIdentifierName;
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        if (file.Namespace == currentIdentifierName)
                        {
                            foundNamespace = true;
                            break;
                        }
                    }
                    if (foundNamespace)
                    {
                        if (chunks.Count == 0)
                        {
                            namespacePrefix = currentIdentifierName;
                            isGlobal = true;
                            return null;
                        }
                        currentIdentifierName = chunks[chunks.Count - 1].Value;
                        currentIdentifierType = chunks[chunks.Count - 1].Key;
                        chunks.RemoveAt(chunks.Count - 1);

                        foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                        {
                            if (file.Namespace == ns)
                            {
                                if (currentIdentifierType == "variable")
                                {
                                    foreach (VariableDescription field in file.Fields)
                                    {
                                        if (field.Name == currentIdentifierName)
                                        {
                                            isArray = field.Type.Contains("[");
                                            nextStructType = RemoveArrayType(field.Type);
                                            break;
                                        }
                                    }
                                }
                                else if (currentIdentifierType == "method")
                                {
                                    foreach (MethodDescription method in file.Methods)
                                    {
                                        if (method.Name == currentIdentifierName)
                                        {
                                            isArray = method.ReturnType.Contains("[");
                                            nextStructType = RemoveArrayType(method.ReturnType);
                                            break;
                                        }
                                    }
                                }
                                if (nextStructType != null)
                                    break;
                            }
                        }
                    }

                }
            }
            else //currentIdentifierType == "method"
            {
                //Check all global methods
                foreach (SourceFileContents parsedSourceFile in Compiler.ParsedSourceFiles)
                {
                    foreach (MethodDescription method in parsedSourceFile.Methods)
                    {
                        if (method.Name == currentIdentifierName)
                        {
                            isArray = method.ReturnType.Contains("[");
                            nextStructType = RemoveArrayType(method.ReturnType);
                            break;
                        }
                    }
                    if (nextStructType != null)
                        break;
                }
            }
            //Didnt find it anywhere
            if (nextStructType == null)
                return null;
            //We got the next struct type. Lets find the matching struct
            StructDescription nextStruct = null;
            foreach (SourceFileContents parsedSourceFile in Compiler.ParsedSourceFiles)
            {
                foreach (StructDescription @struct in parsedSourceFile.Structs)
                {
                    if (@struct.Name == nextStructType)
                    {
                        nextStruct = @struct;
                        break;
                    }
                }
                if (nextStruct != null)
                    break;
            }
            if (nextStruct == null)
            {
                //Might have been a delegate
                foreach (SourceFileContents parsedSourceFile in Compiler.ParsedSourceFiles)
                {
                    foreach (MethodDescription method in parsedSourceFile.Methods)
                    {
                        if (method.Name == nextStructType && method.IsDelegate)
                        {
                            isDelegateInvoke = true;
                            delegateMethod = method;
                            return null;
                        }
                    }
                }
                return null;
            }

            //As for the rest of the chunks. they are either methods or fields in "nextStruct".
            while (chunks.Count > 0)
            {
                currentIdentifierName = chunks[chunks.Count - 1].Value;
                currentIdentifierType = chunks[chunks.Count - 1].Key;
                chunks.RemoveAt(chunks.Count - 1);
                nextStructType = null;

                if (currentIdentifierType == "variable")
                {
                    foreach (VariableDescription field in nextStruct.Fields)
                    {
                        if (field.Name == currentIdentifierName)
                        {
                            isArray = field.Type.Contains("[");
                            nextStructType = RemoveArrayType(field.Type);
                            break;
                        }
                    }
                }
                else //currentIdentifierType == "method"
                {
                    foreach (MethodDescription method in nextStruct.Methods)
                    {
                        if (method.Name == currentIdentifierName)
                        {
                            isArray = method.ReturnType.Contains("[");
                            nextStructType = RemoveArrayType(method.ReturnType);
                            break;
                        }
                    }
                }
                //Didnt find it anywhere
                if (nextStructType == null)
                    return null;
                //We got the next struct type. Lets find the matching struct
                nextStruct = null;
                foreach (SourceFileContents parsedSourceFile in Compiler.ParsedSourceFiles)
                {
                    foreach (StructDescription @struct in parsedSourceFile.Structs)
                    {
                        if (@struct.Name == nextStructType)
                        {
                            nextStruct = @struct;
                            break;
                        }
                    }
                    if (nextStruct != null)
                        break;
                }
                if (nextStruct == null)
                    return null;
            }
            return nextStruct;
        }
        */

        public void ExtractTargetType(string text, out List<StructDescription> targetStructs, out List<EnrichmentDescription> targetEnrichments,
            out List<NamespaceDescription> targetNamespaces, out bool isDelegate, out List<StructDescription> staticTargetStructs, out bool isGlobal, 
            out List<MethodDescription> delegateMethods, 
            out bool isDynamicArray)
        {
            targetStructs = new List<StructDescription>();
            targetEnrichments = new List<EnrichmentDescription>();
            targetNamespaces = new List<NamespaceDescription>();
            isDelegate = false;//Missing this
            staticTargetStructs = new List<StructDescription>();
            isGlobal = false;
            delegateMethods = new List<MethodDescription>();//Missing this
            isDynamicArray = false;
            try
            {


                object openFileData = currentEditor.Tag is Form1.OpenFileData
                                          ? ((Form1.OpenFileData) currentEditor.Tag).File
                                          : (object) ((DialogData) currentEditor.Tag).DialogItem;
                SourceFileContents sourceFile = null;
                foreach (SourceFileContents sourceFileContents in Compiler.ParsedSourceFiles)
                {
                    if (openFileData == sourceFileContents.Item && !sourceFileContents.IsDialogDesigner)
                    {
                        sourceFile = sourceFileContents;
                        break;
                    }
                }
                if (sourceFile == null)
                    return;

                int line = currentEditor.caret.Position.Line;
                IDeclContainer initialContext = sourceFile;
                IDeclContainer prevFile = null;
                while (initialContext != prevFile)
                {
                    prevFile = initialContext;
                    foreach (NamespaceDescription ns in initialContext.Namespaces)
                    {
                        if (ns.LineFrom <= line && ns.LineTo >= line)
                        {
                            initialContext = ns;
                            break;
                        }
                    }
                }

                //var ret = ExtractDotType.GetType(text, Compiler, sourceFile, openFileData.Editor, out isGlobal);//Im not sure if this is correct
                var ret = ExtractDotType.GetType(text, Compiler, initialContext, currentEditor, out isGlobal);
                if (ret.Error) return;

                //Namespaces
                foreach (var pair in ret.Namespaces)
                {
                    targetNamespaces.Add(pair.Type);
                }
                //Static structs
                foreach (ExtractDotType.ReturnData.ContextPair<PType> pair in ret.StaticTypes)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ANamedType)
                    {
                        List<string> list = ((AAName)((ANamedType)type).GetName()).ToStringList();
                        string name = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);
                       /* List<IDeclContainer> visibleDecls;
                        if (list.Count > 0)
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, list);
                        else
                            visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);*/

                        //foreach (IDeclContainer declContainer in context)
                        {
                            foreach (StructDescription str in context.Structs)
                            {
                                if (str.Name == name)
                                {
                                    staticTargetStructs.Add(str);
                                }
                            }
                        }
                    }
                }
                //Structs and enrichments
                foreach (ExtractDotType.ReturnData.ContextPair<PType> pair in ret.Types)
                {
                    PType type = pair.Type;
                    IDeclContainer context = pair.Context;
                    if (type is ADynamicArrayType)
                        isDynamicArray = true;
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
                                    if (ExtractDotType.TypesEqual(type, enrichment.type, context, context))
                                    {
                                        targetEnrichments.Add(enrichment);
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
                                    targetStructs.Add(str);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Look for enrichments
                        List<IDeclContainer> visibleDecls = initialContext.File.GetVisibleDecls(initialContext.NamespaceList, true);
                        foreach (IDeclContainer declContainer in visibleDecls)
                        {
                            foreach (EnrichmentDescription enrichment in declContainer.Enrichments)
                            {
                                if (ExtractDotType.TypesEqual(type, enrichment.type, context, declContainer))
                                {
                                    targetEnrichments.Add(enrichment);
                                }
                            }
                        }
                    }
                }

            }
/*#if DEBUG
            finally
#else*/
            catch (Exception err)
//#endif
            {
               
            }
        }

        


        private bool ExtractMatchData(bool spacePressed = false)
        {
            currentRebuildData.targetEnrichments = new List<EnrichmentDescription>();
            currentRebuildData.targetStructs = new List<StructDescription>();
            currentRebuildData.namespacePrefixes = new List<NamespaceDescription>();
            currentRebuildData.suggestArrayLength = false;
            currentRebuildData.onlySuggestVariablesInsideMethods = false;
            currentRebuildData.onlySuggestMethods = false;
            currentRebuildData.onlySuggestInitKeywords = false;
            currentRebuildData.onlySuggestDelegates = false;
            currentRebuildData.isDelegateInvoke = false;
            currentRebuildData.staticStructs = new List<StructDescription>();
            currentRebuildData.isDynamicArray = false;
            currentRebuildData.targetTypedefs = new List<TypedefDescription>();
            string text = currentEditor.GetTextWithin(new TextPoint(0, 0), currentEditor.caret.Position);
            //Last typed character must be an identifier letter or a .
            if (!spacePressed)
            if (text.Length == 0 || !(text[text.Length - 1] == '.' || text[text.Length - 1] == '>' || Util.IsIdentifierLetter(text[text.Length - 1]) || /*text[text.Length - 1] == '#' ||*/ text[text.Length - 1] > 0xFF))
                return false;
            Lexer lexer = new Lexer(new StringReader(text));
            List<Token> tokens = new List<Token>();
            Token token, lastToken = null;
            bool inString = false;
            bool inEnum = false;
            while (true)
            {
                try
                {
                    token = lexer.Next();
                    if (token is EOF)
                        break;
                }
                catch (Exception err)
                {
                    continue;
                }
                lastToken = token;
                if (token is TEnum)
                    inEnum = true;
                if (token is TRBrace)
                    inEnum = false;
                //We can be in a string if there was an unclosed " on this line
                if (token is TUnknown && token.Text == "\"" || token.Text == "'")
                    inString = true;
                if (token is TWhiteSpace)
                {
                    if (token.Text == "\n")
                        inString = false;
                    continue;
                }
                //If this is true, we have an open block comment with no end, so we're in a comment
                if (token is TCommentBegin)
                    return false;

                if (token is TArrow)
                    token = new TDot(".");

                tokens.Add(token);
            }
            if (inString) return false;
            if (inEnum) return false;
            if (spacePressed && (lastToken == null || !(lastToken is TDot || lastToken is TIdentifier)))
                tokens.Add(new TIdentifier(""));

            //The last entered token can either be an identifier, a dot, or a line comment
            token = tokens[tokens.Count - 1];
            tokens.RemoveAt(tokens.Count - 1);
            if (token is TEndOfLineComment)
                return false;
            //Remove all other comments
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is TTraditionalComment || tokens[i] is TEndOfLineComment || tokens[i] is TDocumentationComment)
                {
                    tokens.RemoveAt(i);
                    i--;
                }
            }



            //Set the current search text
            if (token is TDot) currentRebuildData.caseSensitiveMatchText = currentRebuildData.matchText = "";
            else
            {
                currentRebuildData.matchText = token.Text.ToLower();
                currentRebuildData.caseSensitiveMatchText = token.Text;
            }
            //If this token is a dot, or the previous is a dot, find the type of whats before the dot
            if (token is TIdentifier && tokens.Count > 0 && tokens[tokens.Count - 1] is TDot)
            {
                token = tokens[tokens.Count - 1];
                tokens.RemoveAt(tokens.Count - 1);
            }
            bool isGlobal = false;
            if (token is TDot)
            {
                List<MethodDescription> delegateTargets;
                ExtractTargetType(text, out currentRebuildData.targetStructs, out currentRebuildData.targetEnrichments,
                                    out currentRebuildData.namespacePrefixes, out currentRebuildData.isDelegateInvoke,
                                    out currentRebuildData.staticStructs, out currentRebuildData.isGlobal, out delegateTargets, out currentRebuildData.isDynamicArray);
                /*MethodDescription methodDescription;
                currentRebuildData.targetStruct = GetTargetStruct(token, tokens, out isGlobal,
                                                                  out currentRebuildData.suggestArrayLength,
                                                                  out currentRebuildData.namespacePrefix,
                                                                  out currentRebuildData.isDelegateInvoke,
                                                                  out methodDescription,
                                                                  out currentRebuildData.staticStruct);*/

                if (currentRebuildData.isDelegateInvoke)
                {
                    currentRebuildData.suggestTypes = false;
                    currentRebuildData.suggestVariables = false;
                    currentRebuildData.suggestKeywords = false;
                    return true;
                }

                if (tokens.Count > 2 && tokens[tokens.Count - 1] is TLt && (tokens[tokens.Count - 2] is TAsyncInvoke || tokens[tokens.Count - 2] is TSyncInvoke))
                {
                    currentRebuildData.onlySuggestMethods = true;
                }

                if (currentRebuildData.targetStructs.Count > 0 || currentRebuildData.staticStructs.Count > 0 || currentRebuildData.targetEnrichments.Count > 0 || currentRebuildData.suggestArrayLength)
                {
                    currentRebuildData.suggestTypes = false;
                    currentRebuildData.suggestVariables = true;
                    currentRebuildData.suggestKeywords = false;
                    return true;
                }
                if (currentRebuildData.isGlobal)
                {
                    if (currentRebuildData.namespacePrefixes.Count > 0)
                    //tokens.Clear();
                    {
                        currentRebuildData.suggestKeywords = false;
                        currentRebuildData.suggestTypes = true;
                        currentRebuildData.suggestVariables = true;
                        return true;
                    }
                    else
                    {
                        currentRebuildData.suggestKeywords = true;
                        currentRebuildData.suggestTypes = true;
                        currentRebuildData.suggestVariables = false;
                        return true;
                    }
                }
                if (currentRebuildData.namespacePrefixes.Count > 0)
                {
                    currentRebuildData.suggestKeywords = false;
                    currentRebuildData.suggestTypes = true;
                    currentRebuildData.suggestVariables = true;
                    return true;
                }
                if (currentRebuildData.isDynamicArray)
                    return true;
                return false;
            }
            if (isGlobal || token is TIdentifier)
            {
                //Ensure that we are writing a type, expression or keyword
                /*
                 * If no previous: decl
                 * If previous was a string, and the one before that was include: includeDecl -> decl
                 * If previous was a { } or a ;: stm -> stm, block->stm, fieldDecl->decl, structDecl->decl
                 * If previous was a , + - * / % ! < > = == <= >= != += -= *= /= %= ( & | ^ && || << >> ): rightHandSide, methodParams, if -> stm
                 */
                if (tokens.Count == 0)
                {
                    currentRebuildData.suggestKeywords = true;
                    currentRebuildData.suggestTypes = true;
                    currentRebuildData.suggestVariables = false;
                    return true;
                }
                token = tokens[tokens.Count - 1];
                tokens.RemoveAt(tokens.Count - 1); 
                
                if (token is TIdentifier && tokens.Count > 0 && (tokens[tokens.Count - 1] is TNamespace || tokens[tokens.Count - 1] is TUsing))
                {
                    currentRebuildData.suggestKeywords = true;
                    currentRebuildData.suggestTypes = true;
                    currentRebuildData.suggestVariables = false;
                    return true;
                }
                if (token is TStringLiteral)
                {
                    if (tokens.Count == 0)
                        return false;
                    token = tokens[tokens.Count - 1];
                    tokens.RemoveAt(tokens.Count - 1);
                    if (token is TInclude)
                    {
                        currentRebuildData.suggestKeywords = true;
                        currentRebuildData.suggestTypes = true;
                        currentRebuildData.suggestVariables = false;
                        return true;
                    }
                    return false;
                }
                if (token is TLt && tokens.Count > 0 && (tokens[tokens.Count - 1] is TSyncInvoke || tokens[tokens.Count - 1] is TSyncInvoke))
                {
                    currentRebuildData.suggestKeywords = false;
                    currentRebuildData.suggestTypes = false;
                    currentRebuildData.suggestVariables = true;
                    currentRebuildData.onlySuggestMethods = true;
                    return true;
                }
                if (token is TLt && tokens.Count > 0 && (tokens[tokens.Count - 1] is TDelegate))
                {
                    currentRebuildData.suggestKeywords = false;
                    currentRebuildData.suggestTypes = false;
                    currentRebuildData.suggestVariables = false;
                    currentRebuildData.onlySuggestMethods = false;
                    currentRebuildData.onlySuggestDelegates = true;
                    return true;
                }
                if (token is TNew)
                {
                    currentRebuildData.suggestKeywords = false;
                    currentRebuildData.suggestTypes = true;
                    currentRebuildData.suggestVariables = false;
                    return true;
                }
                {
                    //locals can be declared as const <type> name.. 
                    if (token is TConst)
                    {
                        currentRebuildData.suggestKeywords = false;
                        currentRebuildData.suggestTypes = true;
                        currentRebuildData.suggestVariables = false;
                        return true;
                    }
                    if (token is TNative || token is TStatic)
                    {
                        currentRebuildData.suggestKeywords = false;
                        currentRebuildData.suggestTypes = true;
                        currentRebuildData.suggestVariables = false;
                        return true;
                    }
                    if (token is TReturn)
                    {
                        currentRebuildData.suggestKeywords = false;
                        currentRebuildData.suggestTypes = false;
                        currentRebuildData.suggestVariables = true;
                        return true;
                    }
                    //Check last letter in previous token
                    switch (token.Text[token.Text.Length - 1])
                    {
                        case '{'://y y y
                        case '}':
                        case ';':
                        case ':':
                            currentRebuildData.onlySuggestVariablesInsideMethods = true;
                            currentRebuildData.suggestKeywords = true;
                            currentRebuildData.suggestTypes = true;
                            currentRebuildData.suggestVariables = true;
                            return true;
                        case ','://n y y
                        case '(':
                        case '[':
                        case '<':
                            currentRebuildData.suggestKeywords = false;
                            currentRebuildData.suggestTypes = true;
                            currentRebuildData.suggestVariables = true;
                            return true;
                        case '+'://n n y
                        case '-':
                        case '*':
                        case '/':
                        case '%':
                        case '!':
                        case '>':
                        case '=':
                        case '&':
                        case '|':
                        case '^':
                        case '?':
                        case ')':
                            currentRebuildData.suggestKeywords = false;
                            currentRebuildData.suggestTypes = true;
                            currentRebuildData.suggestVariables = true;
                            return true;
                    }
                    return false;
                }
            }
            else
            {
                //Token was not a dot or identifier
                //Could for example be if the current token is a number
                return false;
            }
        }

        private static string RemoveArrayType(string str)
        {//Also remove *
            int i = str.IndexOf('[');
            int j = str.IndexOf('*');
            if (i == -1 || (j != -1 && j < i))
                i = j;
            j = str.IndexOf('<');
            if (i == -1 || (j != -1 && j < i))
                i = j;
            if (i == -1) return str;
            return str.Remove(i);
        }

        

        public void RebuildVisibleList()
        {
            try
            {
                nextRebuildingData = currentRebuildData;
                rebuildSemaphore.Release();
            }
            catch
            {
            }
        }

        private Semaphore rebuildSemaphore = new Semaphore(0, 1);
        private Thread rebuildThread;
        void RebuildThread()
        {
            while (!Form1.Form.Disposed)
            {
                try
                {
                    while (!rebuildSemaphore.WaitOne(5000))
                    {
                        if (Form1.Form.Disposed) return;
                    }
                    if (Form1.Form.Disposed) return;
                    while (nextRebuildingData != null)
                    {
                        rebuildingData = nextRebuildingData;
                        nextRebuildingData = null;
                        RebuildVisibleListInternal();
                        if (Form1.Form.Disposed) return;
                    }
                    RebuildVisibleCallback();

                }
                catch (Exception err)
                {
                    Program.ErrorHandeler(this, new ThreadExceptionEventArgs(err));
                }
            }
        }

        private delegate void RebuildCallbackDelegate();
        void RebuildVisibleCallback()
        {
            if (currentEditor.InvokeRequired)
            {
                currentEditor.Invoke(new RebuildCallbackDelegate(RebuildVisibleCallback));
                return;
            }

            currentRebuildData = rebuildingData;

            Point p = currentEditor.GetPixelAtTextpoint(currentEditor.caret.Position, false);
            p = currentEditor.PointToScreen(p);
            int desiredRows = Math.Max(1, Math.Min(10, currentRebuildData.displayedItems.Count));
            Size size = new Size(0, desiredRows * Font.Height);
            if (SelectedIndex < LineOffset || SelectedIndex + 10 >= LineOffset)
                LineOffset = SelectedIndex;

            var enumerator = currentRebuildData.displayedItems.GetEnumerator(LineOffset);
            Graphics g = Graphics.FromImage(new Bitmap(1, 1));
            while (enumerator.MoveNext())
            {
                size.Width = Math.Max(size.Width,
                                        (int)g.MeasureString(enumerator.Current.DisplayText, Font).Width);
            }
            size.Width += 16;
            ParentForm.UpdateAndShow(p, ref size);
            Invalidate();
            Point pos = currentEditor.GetPixelAtTextpoint(currentEditor.caret.GetPosition(true));
            if (ParentForm.Visible)
                pos.X += size.Width;

            currentEditor.ime.SetIMEWindowLocation(pos.X, pos.Y);
            SelectedIndex = SelectedIndex;
        }

        private string oldMatchText = "";
        private void RebuildVisibleListInternal()
        {
            rebuildingData.suggestTypes &= !rebuildingData.onlySuggestMethods;
            rebuildingData.suggestKeywords &= !rebuildingData.onlySuggestMethods;
            List<string> addedNamespaces = new List<string>();

            //Get the sourcefile for the current editor
            object openFileData = currentEditor.Tag is Form1.OpenFileData ? ((Form1.OpenFileData)currentEditor.Tag).File : (object)((DialogData)currentEditor.Tag).DialogItem;
            SourceFileContents sourceFile = null;
            foreach (SourceFileContents sourceFileContents in Compiler.ParsedSourceFiles)
            {
                if (openFileData == sourceFileContents.Item && !sourceFileContents.IsDialogDesigner)
                {
                    sourceFile = sourceFileContents;
                    break;
                }
            }
            if (sourceFile == null)
                return;
            IDeclContainer currentContext = sourceFile.GetDeclContainerAt(currentEditor.caret.Position.Line);

            //Remove everything in the displayed list, and rebuild it. 
            //Remember to join overloaded methods
            string selectedSignature = null;
            if (SelectedIndex >= 0 && SelectedIndex < rebuildingData.displayedItems.Count)
                selectedSignature = rebuildingData.displayedItems[SelectedIndex].Signature;

            IEnumerator<SuggestionBoxItem> enumerator;
            if (oldMatchText != "" && rebuildingData.matchText.StartsWith(oldMatchText))
            {//Remove all those that does not match the new text
                enumerator = rebuildingData.displayedItems.GetEnumerator();
                RedBlackTree<SuggestionBoxItem> newDispalyedItems = new RedBlackTree<SuggestionBoxItem>(RelevanceSorter);
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.DisplayText.ToLower().Contains(rebuildingData.matchText))
                        newDispalyedItems.Add(enumerator.Current);
                }
                rebuildingData.displayedItems = newDispalyedItems;
            }
            else
            {

                rebuildingData.displayedItems.Clear();

                if (rebuildingData.targetEnrichments.Count == 0 && rebuildingData.isDynamicArray)
                {
                    if ("resize".Contains(rebuildingData.matchText))
                        rebuildingData.displayedItems.Add(
                            new MethodDescription(new AMethodDecl(new APublicVisibilityModifier(), null, null, null,
                                                                    null, null, new AVoidType(new TVoid("void")),
                                                                    new TIdentifier("Resize"),
                                                                    new ArrayList()
                                                                        {
                                                                            new AALocalDecl(
                                                                                new APublicVisibilityModifier(), null,
                                                                                null, null, null,
                                                                                new ANamedType(
                                                                                    new TIdentifier("int"), null),
                                                                                new TIdentifier("size"), null)
                                                                        },
                                                                    new AABlock(new ArrayList(), new TRBrace("}")))));
                    if ("length".Contains(rebuildingData.matchText))
                        rebuildingData.displayedItems.Add(new CustomSuggestionBoxItem("length", "length", "int length", null));
                    
                }
                else if (rebuildingData.onlySuggestInitKeywords)
                {
                    foreach (GalaxyKeywords.GalaxyKeyword keyword in GalaxyKeywords.InitializerKeywords.keywords)
                    {
                        if (keyword.DisplayText != "" && keyword.DisplayText.ToLower().Contains(rebuildingData.matchText))
                            rebuildingData.displayedItems.Add(keyword);
                    }
                }
                else if (rebuildingData.onlySuggestDelegates)
                {
                    foreach (SourceFileContents file in Compiler.ParsedSourceFiles)
                    {
                        foreach (MethodDescription method in file.Methods)
                        {
                            if (method.IsDelegate && method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                            {
                                rebuildingData.displayedItems.Add(method);
                            }
                        }
                    }
                }
                else if (rebuildingData.isDelegateInvoke)
                {
                    if ("invoke".Contains(rebuildingData.matchText))
                        rebuildingData.displayedItems.Add(new CustomSuggestionBoxItem("Invoke", "Invoke(", null, null));
                }
                else if (rebuildingData.isGlobal)
                {
                    //Suggest everything in visible source files
                    List<IDeclContainer> visibleDecls = sourceFile.GetVisibleDecls(currentContext.NamespaceList, true);
                    foreach (IDeclContainer file in visibleDecls)
                    {
                        //if (sourceFile.CanSeeOther(file))
                        {
                            foreach (VariableDescription field in file.Fields)
                            {
                                if (field.Visibility is APrivateVisibilityModifier &&
                                    currentContext.FullName != file.FullName)
                                    continue;

                                //Dont add static fields if we are in another file
                                if (field.IsStatic && field.ParentFile != sourceFile)
                                    continue;
                                if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(field);
                            }
                            foreach (MethodDescription method in file.Methods)
                            {
                                if (method.Visibility is APrivateVisibilityModifier &&
                                    currentContext.FullName != file.FullName)
                                    continue;

                                //Dont add static methods if we are in another file
                                if (method.IsStatic && method.ParentFile != sourceFile)
                                    continue;
                                if (method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(method);
                            }
                        
                            foreach (StructDescription structDescription in file.Structs)
                            {
                                if (structDescription.Visibility is APrivateVisibilityModifier &&
                                    currentContext.FullName != file.FullName)
                                    continue;

                                if (structDescription.DisplayText != "" && structDescription.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(structDescription);
                            }
                            foreach (TypedefDescription typedef in file.Typedefs)
                            {
                                if (typedef.DisplayText != "" && typedef.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(typedef);
                            }
                        }
                    }
                    foreach (AFieldDecl field in Compiler.libraryData.Fields)
                    {
                        if (field.GetName().Text != "" && field.GetName().Text.ToLower().Contains(rebuildingData.matchText))
                        {
                            VariableDescription desc = new VariableDescription(field);
                            if (desc.Const && desc.init != null)
                            {
                                ConstantFolder folder = new ConstantFolder();
                                desc.init.Apply(folder);
                                desc.initStr = folder.Value;
                            }
                            rebuildingData.displayedItems.Add(desc);
                        }
                    }
                    foreach (AMethodDecl method in Compiler.libraryData.Methods)
                    {
                        if (method.GetName().Text != "" && method.GetName().Text.ToLower().Contains(rebuildingData.matchText))
                            rebuildingData.displayedItems.Add(new MethodDescription(method));
                    }
                }
                else
                {
                    if (rebuildingData.isDynamicArray)
                    {
                        if ("resize".Contains(rebuildingData.matchText))
                            rebuildingData.displayedItems.Add(
                                new MethodDescription(new AMethodDecl(new APublicVisibilityModifier(), null, null, null,
                                                                      null, null, new AVoidType(new TVoid("void")),
                                                                      new TIdentifier("Resize"),
                                                                      new ArrayList()
                                                                          {
                                                                              new AALocalDecl(
                                                                                  new APublicVisibilityModifier(), null,
                                                                                  null, null, null,
                                                                                  new ANamedType(
                                                                                      new TIdentifier("int"), null),
                                                                                  new TIdentifier("size"), null)
                                                                          },
                                                                      new AABlock(new ArrayList(), new TRBrace("}")))));
                    }
                    if (rebuildingData.suggestArrayLength)
                    {
                        if ("length".Contains(rebuildingData.matchText))
                            rebuildingData.displayedItems.Add(new CustomSuggestionBoxItem("length", "length", "int length", null));
                    }
                    if (rebuildingData.suggestVariables && rebuildingData.targetStructs.Count == 0 && rebuildingData.targetEnrichments.Count == 0 && rebuildingData.namespacePrefixes.Count == 0)
                    {
                        foreach (GalaxyKeywords.GalaxyKeyword keyword in GalaxyKeywords.SystemExpressions.keywords)
                        {
                            if (keyword.DisplayText != "" && keyword.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                rebuildingData.displayedItems.Add(keyword);
                        }
                    }

                    StructDescription currentStruct = null;
                    foreach (StructDescription @struct in sourceFile.Structs)
                    {
                        if (@struct.LineFrom <= currentEditor.caret.Position.Line &&
                            @struct.LineTo >= currentEditor.caret.Position.Line)
                        {
                            currentStruct = @struct;
                            break;
                        }
                    }

                    EnrichmentDescription currentEnrichment = null;
                    foreach (EnrichmentDescription enrichment in sourceFile.Enrichments)
                    {
                        if (enrichment.LineFrom <= currentEditor.caret.Position.Line &&
                           enrichment.LineTo >= currentEditor.caret.Position.Line)
                        {
                            currentEnrichment = enrichment;
                            break;
                        }
                    }


                    //if (rebuildingData.targetStruct != null)
                    foreach (StructDescription feStr in rebuildingData.targetStructs)
                    {
                        StructDescription str = feStr;
                        StructDescription initStr = str;
                        while (str != null)
                        {
                            foreach (MethodDescription method in str.Methods)
                            {
                                //Dont add static methods if we are in another file
                                if (method.IsStatic)// != currentRebuildData.staticStruct)
                                    continue;
                                //Visibility
                                if (method.Visibility is APrivateVisibilityModifier &&
                                    str != currentStruct)
                                    continue;
                                if (method.Visibility is AProtectedVisibilityModifier)
                                {
                                    bool extends = false;
                                    StructDescription cStr = currentStruct;
                                    while (cStr != null)
                                    {
                                        if (cStr == str)
                                        {
                                            extends = true;
                                            break;
                                        }
                                        cStr = cStr.Base;
                                    }
                                    if (!extends)
                                        continue;
                                }

                                if (method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(method);
                            }

                            if (!rebuildingData.onlySuggestMethods)
                                foreach (VariableDescription field in str.Fields)
                                {
                                    if (field.IsStatic)
                                        continue;

                                    //Visibility
                                    if (field.Visibility is APrivateVisibilityModifier &&
                                        str != currentStruct)
                                        continue;
                                    if (field.Visibility is AProtectedVisibilityModifier)
                                    {
                                        bool extends = false;
                                        StructDescription cStr = currentStruct;
                                        while (cStr != null)
                                        {
                                            if (cStr == str)
                                            {
                                                extends = true;
                                                break;
                                            }
                                            cStr = cStr.Base;
                                        }
                                        if (!extends)
                                            continue;
                                    }

                                    //Dont add static fields if we are in another file
                                    //if (field.IsStatic && field.ParentFile != sourceFile)
                                    //    continue;
                                    if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                        rebuildingData.displayedItems.Add(field);
                                }
                            str = str.Base;
                            if (str == initStr)
                                break;
                        }
                    }
                    foreach (StructDescription feStr in rebuildingData.staticStructs)
                    {
                        StructDescription str = feStr;
                        StructDescription initStr = str;
                        while (str != null)
                        {
                            foreach (MethodDescription method in str.Methods)
                            {
                                //Dont add static methods if we are in another file
                                if (!method.IsStatic)// != currentRebuildData.staticStruct)
                                    continue;
                                //Visibility
                                if (method.Visibility is APrivateVisibilityModifier &&
                                    str != currentStruct)
                                    continue;
                                if (method.Visibility is AProtectedVisibilityModifier)
                                {
                                    bool extends = false;
                                    StructDescription cStr = currentStruct;
                                    while (cStr != null)
                                    {
                                        if (cStr == str)
                                        {
                                            extends = true;
                                            break;
                                        }
                                        cStr = cStr.Base;
                                    }
                                    if (!extends)
                                        continue;
                                }

                                if (method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(method);
                            }

                            if (!rebuildingData.onlySuggestMethods)
                                foreach (VariableDescription field in str.Fields)
                                {
                                    if (!field.IsStatic)
                                        continue;

                                    //Visibility
                                    if (field.Visibility is APrivateVisibilityModifier &&
                                        str != currentStruct)
                                        continue;
                                    if (field.Visibility is AProtectedVisibilityModifier)
                                    {
                                        bool extends = false;
                                        StructDescription cStr = currentStruct;
                                        while (cStr != null)
                                        {
                                            if (cStr == str)
                                            {
                                                extends = true;
                                                break;
                                            }
                                            cStr = cStr.Base;
                                        }
                                        if (!extends)
                                            continue;
                                    }

                                    //Dont add static fields if we are in another file
                                    //if (field.IsStatic && field.ParentFile != sourceFile)
                                    //    continue;
                                    if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                        rebuildingData.displayedItems.Add(field);
                                }
                            str = str.Base;
                            if (str == initStr)
                                break;
                        }
                    }
                    //if (rebuildingData.targetEnrichments.Count > 0)
                    {
                        foreach (EnrichmentDescription enrichment in rebuildingData.targetEnrichments)
                        {
                            foreach (VariableDescription field in enrichment.Fields)
                            {
                                if (field.IsStatic)
                                    continue;

                                //Visibility
                                /* !FIX! if ((field.Visibility is APrivateVisibilityModifier ||
                                       field.Visibility is AProtectedVisibilityModifier) &&
                                       (currentEnrichment == null || !ExtractDotType.TypesEqual(enrichment.type, currentEnrichment.type, 
                                          enrichment.ParentFile, currentEnrichment.ParentFile)))
                                      continue;*/


                                //Dont add static fields if we are in another file
                                //if (field.IsStatic && field.ParentFile != sourceFile)
                                //    continue;
                                if (field.DisplayText != "" &&
                                    field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(field);
                            }
                            foreach (MethodDescription method in enrichment.Methods)
                            {
                                //Dont add static methods if we are in another file
                                if (method.IsStatic)
                                    continue;
                                //Visibility
                                /*!FIX!if ((method.Visibility is APrivateVisibilityModifier ||
                                     method.Visibility is AProtectedVisibilityModifier) &&
                                    (currentEnrichment == null || !ExtractDotType.TypesEqual(enrichment.type, currentEnrichment.type,
                                        enrichment.ParentFile, currentEnrichment.ParentFile)))
                                    continue;*/

                                if (method.DisplayText != "" &&
                                    method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(method);
                            }
                        }
                    }
                    //else if (rebuildingData.namespacePrefix != null)
                    foreach (NamespaceDescription ns in rebuildingData.namespacePrefixes)
                    {
                        if (!rebuildingData.onlySuggestMethods)
                            foreach (VariableDescription field in ns.Fields)
                            {
                                //Visibility
                                if (field.Visibility is APrivateVisibilityModifier &&
                                    ns.FullName != currentContext.FullName)
                                    continue;


                                //Dont add static fields if we are in another file
                                if (field.IsStatic && field.ParentFile != sourceFile)
                                    continue;
                                if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(field);
                            }
                        foreach (MethodDescription method in ns.Methods)
                        {
                            //Visibility
                            if (method.Visibility is APrivateVisibilityModifier &&
                                ns.FullName != currentContext.FullName)
                                continue;
                            //Dont add static methods if we are in another file
                            if (method.IsStatic && method.ParentFile != sourceFile)
                                continue;
                            if (method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                rebuildingData.displayedItems.Add(method);
                        }
                        if (!rebuildingData.onlySuggestMethods)
                            foreach (StructDescription structDescription in ns.Structs)
                            {
                                if (structDescription.Visibility is APrivateVisibilityModifier &&
                                    ns.FullName != currentContext.FullName)
                                    continue;
                                if (structDescription.DisplayText != "" && structDescription.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(structDescription);
                            }
                        foreach (NamespaceDescription ns2 in ns.Namespaces)
                        {
                            if (ns2.Name.ToLower().Contains(rebuildingData.matchText) && !addedNamespaces.Contains(ns2.Name))
                            {
                                addedNamespaces.Add(ns2.Name);
                                rebuildingData.displayedItems.Add(ns2);
                            }
                        }
                        if (rebuildingData.suggestTypes)
                            foreach (TypedefDescription typedef in ns.Typedefs)
                            {
                                if (typedef.Name.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(typedef);
                            }
                    }
                    if (rebuildingData.targetStructs.Count == 0 && rebuildingData.staticStructs.Count == 0 && rebuildingData.targetEnrichments.Count == 0 && rebuildingData.namespacePrefixes.Count == 0)
                    {
                        //Check if we are inside a method
                        MethodDescription inMethod = null;
                        foreach (MethodDescription method in currentContext.Methods)
                        {
                            if (method.Start < currentEditor.caret.Position &&
                                method.End > currentEditor.caret.Position)
                            {
                                inMethod = method;
                                break;
                            }
                        }
                        //Check struct methods
                        //StructDescription inStruct = null;
                        if (inMethod == null)
                        {
                            foreach (StructDescription @struct in currentContext.Structs)
                            {
                                if (@struct.LineFrom <= currentEditor.caret.Position.Line &&
                                    @struct.LineTo >= currentEditor.caret.Position.Line)
                                {
                                    
                                    if (rebuildingData.suggestTypes)
                                        foreach (string generic in @struct.GenericVars)
                                        {
                                            if (generic != "" && generic.ToLower().Contains(rebuildingData.matchText))
                                                rebuildingData.displayedItems.Add(
                                                    new CustomSuggestionBoxItem(generic, generic,
                                                                                "Generic " + generic,
                                                                                typeof (string)));
                                        }


                                    List<MethodDescription> list = new List<MethodDescription>();
                                    list.AddRange(@struct.Methods);
                                    list.AddRange(@struct.Constructors);
                                    list.AddRange(@struct.Deconstructors);
                                    foreach (MethodDescription method in list)
                                    {
                                        if (method.Start < currentEditor.caret.Position &&
                                            method.End > currentEditor.caret.Position)
                                        {
                                            inMethod = method;


                                            StructDescription str = @struct;
                                            StructDescription initStr = str;
                                            while (str != null)
                                            {

                                                //Add variables from this struct
                                                if (rebuildingData.suggestVariables)
                                                {
                                                    if (!rebuildingData.onlySuggestMethods)
                                                        foreach (VariableDescription field in str.Fields)
                                                        {
                                                            //Visibility
                                                            if (field.Visibility is APrivateVisibilityModifier &&
                                                                str != currentStruct)
                                                                continue;
                                                            if (field.Visibility is AProtectedVisibilityModifier)
                                                            {
                                                                bool extends = false;
                                                                StructDescription cStr = currentStruct;
                                                                while (cStr != null)
                                                                {
                                                                    if (cStr == str)
                                                                    {
                                                                        extends = true;
                                                                        break;
                                                                    }
                                                                    cStr = cStr.Base;
                                                                }
                                                                if (!extends)
                                                                    continue;
                                                            }

                                                            if (!field.IsStatic && method.IsStatic)
                                                                continue;//Cant refference nonstatic fields from static

                                                            if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(
                                                                    rebuildingData.matchText))
                                                                rebuildingData.displayedItems.Add(field);
                                                        }

                                                    foreach (MethodDescription m in str.Methods)
                                                    {
                                                        //Visibility
                                                        if (m.Visibility is APrivateVisibilityModifier &&
                                                            str != currentStruct)
                                                            continue;
                                                        if (m.Visibility is AProtectedVisibilityModifier)
                                                        {
                                                            bool extends = false;
                                                            StructDescription cStr = currentStruct;
                                                            while (cStr != null)
                                                            {
                                                                if (cStr == str)
                                                                {
                                                                    extends = true;
                                                                    break;
                                                                }
                                                                cStr = cStr.Base;
                                                            }
                                                            if (!extends)
                                                                continue;
                                                        }

                                                        if (!m.IsStatic && method.IsStatic)
                                                            continue;//Cant refference nonstatic methods from static

                                                        if (m.DisplayText != "" && m.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                                            rebuildingData.displayedItems.Add(m);
                                                    }
                                                }
                                                
                                                str = str.Base;
                                                if (str == initStr)
                                                    break;
                                            }
                                            break;
                                        }
                                    }
                                    if (inMethod != null)
                                        break;
                                }
                            }
                        }

                        if (inMethod == null)
                        {//Look for enrichment
                            foreach (EnrichmentDescription enrichment in currentContext.Enrichments)
                            {
                                if (enrichment.LineFrom <= currentEditor.caret.Position.Line &&
                                    enrichment.LineTo >= currentEditor.caret.Position.Line)
                                {
                                    List<MethodDescription> list = new List<MethodDescription>();
                                    list.AddRange(enrichment.Methods);
                                    list.AddRange(enrichment.Constructors);
                                    list.AddRange(enrichment.Deconstructors);
                                    foreach (MethodDescription method in list)
                                    {
                                        if (method.Start < currentEditor.caret.Position &&
                                            method.End > currentEditor.caret.Position)
                                        {
                                            inMethod = method;
                                            //Add all fields/methods from the current enrichment
                                            if (rebuildingData.suggestVariables)
                                            {
                                                if (!rebuildingData.onlySuggestMethods)
                                                    foreach (VariableDescription field in enrichment.Fields)
                                                    {
                                                        //Visibility
                                                        if ((field.Visibility is APrivateVisibilityModifier ||
                                                             field.Visibility is AProtectedVisibilityModifier) &&
                                                            enrichment != currentEnrichment)
                                                            continue;

                                                        if (!field.IsStatic && method.IsStatic)
                                                            continue; //Cant refference nonstatic fields from static

                                                        if (field.DisplayText != "" &&
                                                            field.DisplayText.ToLower().Contains(
                                                                rebuildingData.matchText))
                                                            rebuildingData.displayedItems.Add(field);
                                                    }

                                                foreach (MethodDescription m in enrichment.Methods)
                                                {
                                                    //Visibility
                                                    if ((m.Visibility is APrivateVisibilityModifier ||
                                                            m.Visibility is AProtectedVisibilityModifier) &&
                                                        enrichment != currentEnrichment)
                                                        continue;
                                                    

                                                    if (!m.IsStatic && method.IsStatic)
                                                        continue;//Cant refference nonstatic methods from static

                                                    if (m.DisplayText != "" && m.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                                        rebuildingData.displayedItems.Add(m);
                                                }
                                            }
                                        }
                                    }
                                    if (inMethod != null)
                                        break;
                                }
                            }

                        }

                        //Add keywords depending on if we're in a method or not


                        rebuildingData.suggestVariables = rebuildingData.onlySuggestVariablesInsideMethods ? inMethod != null : rebuildingData.suggestVariables;
                        if (inMethod == null)
                        {
                            if (rebuildingData.suggestKeywords)
                                foreach (
                                    GalaxyKeywords.GalaxyKeyword keyword in GalaxyKeywords.OutMethodKeywords.keywords)
                                {
                                    if (keyword.DisplayText != "" && keyword.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                        rebuildingData.displayedItems.Add(keyword);
                                }
                            if ("preloadbank".Contains(rebuildingData.matchText))
                            {
                                rebuildingData.displayedItems.Add(
                                    new MethodDescription(new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                          new ANamedType(new TIdentifier("void"), null),
                                                                          new TIdentifier("PreloadBank"),
                                                                          new ArrayList
                                                                              {
                                                                                  new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                                  new ANamedType(
                                                                                                      new TIdentifier(
                                                                                                          "string"),
                                                                                                      null),
                                                                                                  new TIdentifier(
                                                                                                      "bankName"), null),
                                                                                  new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                                                  new ANamedType(
                                                                                                      new TIdentifier(
                                                                                                          "int"), null),
                                                                                                  new TIdentifier(
                                                                                                      "player"), null)
                                                                              },
                                                                          null)));
                            }
                        }
                        else
                        {
                            if (rebuildingData.suggestKeywords)
                                foreach (
                                    GalaxyKeywords.GalaxyKeyword keyword in GalaxyKeywords.InMethodKeywords.keywords)
                                {
                                    if (keyword.DisplayText != "" && keyword.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                        rebuildingData.displayedItems.Add(keyword);
                                }

                        }

                        //Add globals
                        List<IDeclContainer> visibleDecls = sourceFile.GetVisibleDecls(currentContext.NamespaceList, true);
                        foreach (IDeclContainer file in visibleDecls)
                        {
                            //!FIX! if (sourceFile.CanSeeOther(file))
                            {
                                if (rebuildingData.suggestVariables)
                                {
                                    if (!rebuildingData.onlySuggestMethods)
                                        foreach (VariableDescription field in file.Fields)
                                        {
                                            if (field.Visibility is APrivateVisibilityModifier &&
                                                currentContext.FullName != file.FullName)
                                                continue;

                                            //Dont add static fields if we are in another file
                                            if (field.IsStatic && field.ParentFile != sourceFile)
                                                continue;
                                            if (field.DisplayText != "" && field.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                                rebuildingData.displayedItems.Add(field);
                                        }
                                    foreach (MethodDescription method in file.Methods)
                                    {
                                        if (method.Visibility is APrivateVisibilityModifier &&
                                            currentContext.FullName != file.FullName)
                                            continue;

                                        //Dont add static methods if we are in another file
                                        if (method.IsStatic && method.ParentFile != sourceFile)
                                            continue;
                                        if (method.DisplayText != "" && method.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                            rebuildingData.displayedItems.Add(method);
                                    }

                                }
                                if (rebuildingData.suggestTypes)
                                {
                                    foreach (StructDescription structDescription in file.Structs)
                                    {
                                        if (structDescription.Visibility is APrivateVisibilityModifier &&
                                            currentContext.FullName != file.FullName)
                                            continue;
                                        if (structDescription.DisplayText != "" && structDescription.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                            rebuildingData.displayedItems.Add(structDescription);
                                    }
                                    foreach (TypedefDescription typedef in file.Typedefs)
                                    {
                                        if (typedef.DisplayText != "" && typedef.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                            rebuildingData.displayedItems.Add(typedef);
                                    }
                                }
                            }
                        }
                        if (rebuildingData.suggestTypes)
                            foreach (var primitive in GalaxyKeywords.Primitives.keywords)
                            {
                                if (primitive.DisplayText != "" && primitive.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(primitive);
                            }
                        //Add namespaces
                        if (rebuildingData.suggestVariables || rebuildingData.suggestTypes)
                        {
                            List<IDeclContainer> visibleDecls2 =
                                currentContext.File.GetVisibleDecls(currentContext.NamespaceList, false);
                            foreach (IDeclContainer visibleDecl in visibleDecls2)
                            {
                                foreach (NamespaceDescription ns in visibleDecl.Namespaces)
                                {
                                    if (ns.Name.ToLower().Contains(rebuildingData.matchText) && !addedNamespaces.Contains(ns.Name))
                                    {
                                        addedNamespaces.Add(ns.Name);
                                        rebuildingData.displayedItems.Add(ns);
                                    }
                                }
                            }
                        }
                        /*foreach (SuggestionBoxItem item in globalItems)
                        {
                            if ((item is MethodDescription && suggestVariables) ||
                                (item is StructDescription && suggestTypes) ||
                                (item is VariableDescription && suggestVariables) ||
                                (item is GalaxyKeywords.GalaxyKeyword && suggestKeywords))
                                if (item.DisplayText.ToLower().Contains(matchText))
                                    displayedItems.Add(item);
                        }*/

                        //If we are in a method, add all variables from that
                        if (inMethod != null && rebuildingData.suggestVariables && !rebuildingData.onlySuggestMethods)
                        {
                            foreach (VariableDescription formal in inMethod.Formals)
                            {
                                if (formal.DisplayText != "" && formal.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(formal);
                            }
                            foreach (VariableDescription formal in inMethod.Locals)
                            {
                                if (formal.DisplayText != "" && formal.DisplayText.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(formal);
                            }
                        }

                        //Add library data
                        if (rebuildingData.suggestVariables)
                        {
                            if (!rebuildingData.onlySuggestMethods)
                                foreach (AFieldDecl field in Compiler.libraryData.Fields)
                                {
                                    if (field.GetName().Text != "" && field.GetName().Text.ToLower().Contains(rebuildingData.matchText))
                                    {
                                        VariableDescription desc = new VariableDescription(field);
                                        if (desc.Const && desc.init != null)
                                        {
                                            ConstantFolder folder = new ConstantFolder();
                                            desc.init.Apply(folder);
                                            desc.initStr = folder.Value;
                                        }
                                        rebuildingData.displayedItems.Add(desc);
                                    }
                                }
                            foreach (AMethodDecl method in Compiler.libraryData.Methods)
                            {
                                if (method.GetName().Text != "" && method.GetName().Text.ToLower().Contains(rebuildingData.matchText))
                                    rebuildingData.displayedItems.Add(new MethodDescription(method));
                            }
                        }
                    }
                }
            }
            //Look through displayedItems, and join overloaded methods
            int firstMethodIndex = -1;
            int i = 0;
            List<MethodDescription> lastMethods = new List<MethodDescription>();
            enumerator = rebuildingData.displayedItems.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!(enumerator.Current is MethodDescription))
                    continue;
                MethodDescription method = (MethodDescription) enumerator.Current;
                if (lastMethods.Count > 0 && lastMethods[0].Name != method.Name)
                {
                    if (lastMethods.Count > 1)
                    {
                        //Join methods
                        CustomSuggestionBoxItem overloadItem =
                            new CustomSuggestionBoxItem(lastMethods[0].DisplayText,
                                                        lastMethods[0].InsertText,
                                                        lastMethods.Aggregate("",
                                                                              (
                                                                                  current,
                                                                                  methodDescription)
                                                                              =>
                                                                              current +
                                                                              (methodDescription
                                                                                   .
                                                                                   TooltipText +
                                                                               "\n")),
                                                        typeof (MethodDescription));
                        //Remove last \n
                        overloadItem.TooltipText =
                            overloadItem.TooltipText.Remove(overloadItem.TooltipText.Length - 1);
                        foreach (MethodDescription methodDescription in lastMethods)
                        {
                            rebuildingData.displayedItems.Remove(methodDescription);
                        }
                        rebuildingData.displayedItems.Add(overloadItem);
                    }
                    lastMethods.Clear();
                }
                if (lastMethods.Count == 0) firstMethodIndex = i;
                lastMethods.Add(method);
                i++;
            }
            if (lastMethods.Count > 1)
            {

                //Join methods
                CustomSuggestionBoxItem overloadItem = new CustomSuggestionBoxItem(lastMethods[0].DisplayText,
                                                                                   lastMethods[0].InsertText,
                                                                                   lastMethods.Aggregate("",
                                                                                                         (
                                                                                                             current,
                                                                                                             methodDescription)
                                                                                                         =>
                                                                                                         current +
                                                                                                         (methodDescription
                                                                                                              .
                                                                                                              TooltipText +
                                                                                                          "\n")),
                                                                                   typeof (MethodDescription));
                //Remove last \n
                overloadItem.TooltipText = overloadItem.TooltipText.Remove(overloadItem.TooltipText.Length - 1);
                foreach (MethodDescription methodDescription in lastMethods)
                {
                    rebuildingData.displayedItems.Remove(methodDescription);
                }
                rebuildingData.displayedItems.Add(overloadItem);
            }

            //Check if the old selected is still there
            enumerator = rebuildingData.displayedItems.GetEnumerator();
            int selectedIndex = 0;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Signature == selectedSignature)
                {
                    break;
                }
                selectedIndex++;
            }
            if (rebuildingData.displayedItems.Count > 0)
                SetSelectedIndex(selectedIndex % rebuildingData.displayedItems.Count);
            else
                SetSelectedIndex(-1);
            oldMatchText = rebuildingData.matchText;
            //Invalidate();
        }

        private delegate void SetSelectedIndexDelegate(int index);
        void SetSelectedIndex(int index)
        {
            if (InvokeRequired)
            {
                Invoke(new SetSelectedIndexDelegate(SetSelectedIndex), index);
                return;
            }
            SelectedIndex = index;
        }



        private void InsertSelected()
        {
            //First, find out how much should be removed
            //From the caret, go right untill the char is not an identifier
            //go left untill its not an identifier then one step back
            string text = currentEditor.GetLine(currentEditor.caret.Position.Line);
            TextPoint from, to;
            from = to = currentEditor.caret.Position;
            from.Pos--;
            while (from.Pos >= 0 && Util.IsIdentifierLetter(text[from.Pos]))
            {
                from.Pos--;
            }
            from.Pos++;
            /*while (to.Pos < text.Length && Util.IsIdentifierLetter(text[to.Pos]))
            {
                to.Pos++;
            }*/

            text = currentRebuildData.displayedItems[SelectedIndex].InsertText;
            currentEditor.ReplaceTextAt(from, to, text);
        }




        protected override void OnGotFocus(EventArgs e)
        {
            if (!ParentForm.MouseOnScrollBar)
            {
                currentEditor.Focus();
            }
            base.OnGotFocus(e);
        }

        private void tooltip_GotFocus(object sender, EventArgs e)
        {
            //currentEditor.Focus();
        }

        private DateTime lastMouseUp;
        protected override void WndProc(ref Message m)
        {
            /*if (m.Msg != 0x81 && m.Msg != 0x83 && m.Msg != 0x1 && m.Msg != 0x5 && m.Msg != 0x2210
                 && m.Msg != 0x18 && m.Msg != 0x46 && m.Msg != 0x47 && m.Msg != 0xe && m.Msg != 0xd
                 && m.Msg != 0x85 && m.Msg != 0x14 && m.Msg != 0xf && m.Msg != 0x84 && m.Msg != 0x20
                 && m.Msg != 0x1fa && m.Msg != 0x200 && m.Msg != 0x2a3 && m.Msg != 0x281 && m.Msg != 0x282
                 && m.Msg != 0x7 && m.Msg != 0x8 && m.Msg != 0xc1fa && m.Msg != 0x2a1 && m.Msg != 0x1f
                  && m.Msg != 0x215 && m.Msg != 0x3 && m.Msg != 0x27e

                 && m.Msg != 0x21 //Activate
                 && m.Msg != 0x201 //Btn down
                 && m.Msg != 0x202 //btn up
                )
                m = m;*/

            if (m.Msg == 0x202)//L button up
            {
                DateTime now = DateTime.Now;
                Point mousePos = new Point(m.LParam.ToInt32());
                int newSelected = mousePos.Y/Font.Height + LineOffset;
                if (newSelected != SelectedIndex)
                {
                    SelectedIndex = newSelected;
                    Invalidate();
                }
                else if ((now - lastMouseUp).TotalMilliseconds < 500)
                {
                    if (SelectedIndex < currentRebuildData.displayedItems.Count && SelectedIndex >= 0)
                    {
                        InsertSelected();
                        ParentForm.Visible = false;
                    }
                }
                lastMouseUp = now;
            }
            base.WndProc(ref m);
        }

        protected void MouseUp(MouseEventArgs e)
        {
            SelectedIndex = e.Y / Font.Height + LineOffset;
            Invalidate();
            currentEditor.Focus();
            base.OnMouseClick(e);
        }

        public void Reposition()
        {
            if (Visible)
            { 
                Point p = currentEditor.GetPixelAtTextpoint(currentEditor.caret.Position, false);
                p = currentEditor.PointToScreen(p);
                ParentForm.Location = p;
                tooltip.SetPosition(new Point(p.X + ParentForm.Width, p.Y));
            }
            if (currentEditor != null)
                currentEditor_OnCaretChanged(currentEditor);
        }

        private void currentEditor_SizeChanged(object sender, EventArgs e)
        {
            Reposition();
        }

    }
}
