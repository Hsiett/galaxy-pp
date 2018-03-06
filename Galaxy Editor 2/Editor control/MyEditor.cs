using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Dialog_Creator;
using Galaxy_Editor_2.Suggestion_box;

namespace Galaxy_Editor_2.Editor_control
{
    partial class MyEditor : Control
    {
        public delegate void TextEditedEventHandler(MyEditor sender);
        public delegate void ScrolledEventHandler(MyEditor sender);
        public delegate void CaretChangedEventHandler(MyEditor sender);

        public event TextEditedEventHandler OnTextEdited;
        public event ScrolledEventHandler OnScrolled;
        public event CaretChangedEventHandler OnCaretChanged;

        public class Caret
        {
            public TextPoint Position {
                get { return GetPosition(true); }
                private set
                {
                    position = value;
                    if (owner.OnCaretChanged != null)
                        owner.OnCaretChanged(owner);
                }
            }
            private TextPoint position = new TextPoint(0, 0);
            public bool Shown;

            public TextPoint GetPosition(bool absolute)
            {
                if (absolute)
                    return position;
                int line = position.Line;
                for (int i = 0; i < position.Line; i++)
                {
                    if (!owner.lines[i].LineVisible)
                        line--;
                }
                return new TextPoint(line, position.Pos);
            }

            public void SetPosition(TextPoint pos, bool absolute)
            {
                if (!absolute)
                {
                    for (int i = 0; i <= pos.Line; i++)
                    {
                        if (!owner.lines[i].LineVisible)
                            pos.Line++;
                    }
                }
                Position = pos;
            }

            private MyEditor owner;

            public Caret(MyEditor owner)
            {
                this.owner = owner;
            }
        }

        public List<int> GetHiddenBlocks()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].BlockEndLine != null && !lines[i].BlockVisible)
                    list.Add(i);
            }
            return list;
        }

        public void SetHiddenBlocks(List<int> list)
        {
            if (list == null) return;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].BlockEndLine != null && list.Contains(i))
                    lines[i].BlockVisible = false;
            }
            UpdateBlocks();
        }

        public UndoSystem UndoSys = new UndoSystem();
        private Form1 parentForm;
        private HScrollBar horizontalScrollBar;
        private VScrollBar verticalScrollBar;
        private FontScheme fonts = new FontScheme();
        private List<Line> lines = new List<Line>();
        private TextPoint mouseDownPos;
        private bool leftMouseDown;
        public bool TextMarked { get; private set; }
        public Caret caret;
        private Timer caretTimer;
        private bool InvalidateAll = true, RestyleAll;
        public Ime ime;

        public void Restyle()
        {
            InvalidateAll = RestyleAll = true;
            Invalidate();
        }

        private Rectangle TextRegion
        {
            get
            {
                Rectangle rect = new Rectangle(11, //Line count thingy 
                                               0,
                                               //Vertical scrollbar
                                               Size.Width - (verticalScrollBar.Visible ? verticalScrollBar.Width : 0),
                                               //Horizontal scrollbar
                                               Size.Height - (horizontalScrollBar.Visible ? horizontalScrollBar.Height : 0)
                    );
                rect.X += fonts.CharWidth*lines.Count.ToString().Length + 3;

                rect.Width -= rect.X;
                rect.Height -= rect.Y;
                rect.Width = Math.Max(rect.Width, 0);
                rect.Height = Math.Max(rect.Height, 0);
                return rect;
            }
        }

        public bool IsReadonly;
        public MyEditor(Form1 parentForm, bool isReadOnly)
        {
            this.parentForm = parentForm;
            this.IsReadonly = isReadOnly;
            caret = new Caret(this);
            InitializeComponent();

            BackColor = Color.White;


            


            horizontalScrollBar = new HScrollBar();
            horizontalScrollBar.Dock = DockStyle.Bottom;
            horizontalScrollBar.Value = 0;
            horizontalScrollBar.ValueChanged += scrollBar_ValueChanged;
            horizontalScrollBar.SmallChange = 10;
            horizontalScrollBar.LargeChange = 100;
            horizontalScrollBar.KeyPress += OnKeyPress;
            horizontalScrollBar.KeyDown += OnKeyDown;
            Controls.Add(horizontalScrollBar);
            verticalScrollBar = new VScrollBar();
            verticalScrollBar.Dock = DockStyle.Right;
            verticalScrollBar.Value = 0;
            verticalScrollBar.ValueChanged += scrollBar_ValueChanged;
            verticalScrollBar.SmallChange = 1;
            verticalScrollBar.LargeChange = 10;
            verticalScrollBar.KeyPress += OnKeyPress;
            verticalScrollBar.KeyDown += OnKeyDown;
            Controls.Add(verticalScrollBar);


            caretTimer = new Timer();
            caretTimer.Interval = 500;
            caretTimer.Enabled = true;
            caretTimer.Tick += caretTimer_Tick;

            DoubleBuffered = true;
            ime = new Ime(parentForm.Handle, fonts.Base);
            //SetStyle(ControlStyles.UserPaint /*|
            //         /*ControlStyles.AllPaintingInWmPaint*/, true);

            /*SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Opaque |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw, true);*/


        }


        private delegate string GetTextHandler();

        private string GetText()
        {
            /*string text = lines.Aggregate("", (current, line) => current + (line.Text + "\n"));
            text = text.Remove(text.Length - 1);
            return text;*/

            StringBuilder builder = new StringBuilder("");
            foreach (Line line in lines)
            {
                builder.AppendLine(line.Text);
            }
            return builder.ToString();
        }

        public new string Text
        {
            get
            {
                while (true)
                {
                    try
                    {
                        string text = GetText();
                        return text;
                    }
                    catch (Exception err)
                    {
                        //Retry
                    }
                }
            }
            set
            {
                value = value ?? "";
                lines.Clear();
                string txt = value.Replace("\r", "");
                if (Options.Editor.ReplaceTabsWithSpaces)
                    txt = txt.Replace("\t", "    ");
                string[] texts = txt.Split('\n');
                foreach (string text in texts)
                {
                    lines.Add(new Line(text));
                    lines[lines.Count - 1].Restyle(fonts, lines, lines.Count - 1);
                }
                InvalidateAll = true;
                caret.SetPosition(new TextPoint(0, 0), true);
                RecalculateWidestLine();
                UpdateBlocks();
                TextEdited();
                CaretMoved();
                Invalidate();
            }
        }


        private void caretTimer_Tick(object sender, EventArgs e)
        {
            caret.Shown = !caret.Shown;
            Invalidate();
        }



        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                InvalidateAll = true;
                Invalidate();
            }
        }


        private void scrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (OnScrolled != null) OnScrolled(this);
            InvalidateAll = true;
            Invalidate();

            Point pos = GetPixelAtTextpoint(caret.GetPosition(true));
            if (parentForm.suggestionBox.Visible)
                pos.X += parentForm.suggestionBox.Width;
            ime.SetIMEWindowLocation(pos.X, pos.Y);
        }


        protected override void InitLayout()
        {
            UpdateScrollBars();
            base.InitLayout();
        }

        public void SetFontScheme(FontScheme scheme)
        {
            fonts = scheme;
            RestyleAll = true;
            InvalidateAll = true;
            Invalidate();
        }

        public FontScheme GetFontScheme()
        {
            return fonts;
        }

        public void AddFontStyle(Options.FontStyles mod, string[] words)
        {
            if (fonts.Modifications.ContainsKey(mod))
                fonts.Modifications[mod].AddRange(words);
            else
                fonts.Modifications.Add(mod, new List<string>(words));
            RestyleAll = true;
            InvalidateAll = true;
            Invalidate();
        }

        private Line widestLine = new Line("");

        private void ExtendedLines(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                if (line.GetWidth(fonts) > widestLine.GetWidth(fonts))
                    widestLine = line;
            }
        }

        private void ShrunkLine(Line line)
        {
            if (line == widestLine)
                RecalculateWidestLine();
        }

        private void RemovedLines(List<Line> lines)
        {
            if (lines.Contains(widestLine))
                RecalculateWidestLine();
        }

        private void ShowedLines(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                if (line.GetWidth(fonts) > widestLine.GetWidth(fonts))
                    widestLine = line;
            }
        }

        private void HidLines(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                if (line.GetWidth(fonts) > widestLine.GetWidth(fonts))
                    widestLine = line;
            }
        }

        private void RecalculateWidestLine()
        {
            widestLine = lines[0];
            foreach (Line line in lines)
            {
                if (line.GetWidth(fonts) > widestLine.GetWidth(fonts))
                    widestLine = line;
            }
        }

        private void UpdateScrollBars()
        {
            Rectangle textRegion = TextRegion;
            //Set max values
            //Number of lines minus the number of lines that can be displayed
            int visibleLineCount = textRegion.Height/fonts.Base.Height;
            verticalScrollBar.Maximum = Math.Max(0, lines.Count - visibleLineCount);

            //Update text region if needed
            if (verticalScrollBar.Visible && !(verticalScrollBar.Maximum > 0))
                textRegion.Width += verticalScrollBar.Width;
            if (!verticalScrollBar.Visible && (verticalScrollBar.Maximum > 0))
                textRegion.Width -= verticalScrollBar.Width;

            float widestLine = this.widestLine.GetWidth(fonts);// lines.Aggregate<Line, float>(0, (current, line) => Math.Max(current, line.GetWidth(fonts)));
            horizontalScrollBar.Maximum = Math.Max(0, (int)widestLine - textRegion.Width);
            if (horizontalScrollBar.Visible != horizontalScrollBar.Maximum > 0)
            {//Might hide the last line, which means the vertical scrollbar should be shown afterall
                verticalScrollBar.Visible = verticalScrollBar.Maximum > 0;
                textRegion = TextRegion;

                visibleLineCount = textRegion.Height / fonts.Base.Height;
                verticalScrollBar.Maximum = Math.Max(0, lines.Count - visibleLineCount);
            }
            horizontalScrollBar.Visible = horizontalScrollBar.Maximum > 0;
            verticalScrollBar.Visible = verticalScrollBar.Maximum > 0;

            if (horizontalScrollBar.Visible) horizontalScrollBar.Maximum += 50;
            if (verticalScrollBar.Visible) verticalScrollBar.Maximum += 10;
        }

        /*protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xf || m.Msg == 0x85)
            {
                
            }
            base.WndProc(ref m);
        }*/

        

        private Bitmap bg = new Bitmap(10, 10);
        private int lastLineNumerWidth;
        private Bitmap snowman = Properties.Resources.Snowman;
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
            int lineNumberWidth = lines.Count.ToString().Length;
            if (lineNumberWidth != lastLineNumerWidth)
            {
                InvalidateAll = true;
                lastLineNumerWidth = lineNumberWidth;
            }
            Graphics g = Graphics.FromImage(bg);
            //InvalidateAll = true;
            //e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            int lineNr = verticalScrollBar.Value;
            Rectangle textRegion = TextRegion;
            Rectangle bounds = new Rectangle(textRegion.X - horizontalScrollBar.Value, textRegion.Y, textRegion.Width + horizontalScrollBar.Value, fonts.Base.Height);

            Pen controlPen = new Pen(Color.FromArgb(165, 165, 165));
            int leftLineCenter = textRegion.X - 6;
            g.FillRectangle(Brushes.White, 0, 0, textRegion.X, e.ClipRectangle.Y + e.ClipRectangle.Height);
            g.DrawLine(controlPen, leftLineCenter, e.ClipRectangle.Y, leftLineCenter,
                                e.ClipRectangle.Y + e.ClipRectangle.Height);
            while (bounds.Y < textRegion.Y + textRegion.Height)
            {
                if (lineNr < lines.Count)
                {
                    if (!lines[lineNr].LineVisible)
                    {
                        lineNr++;
                        continue;
                    }


                    //Draw line nr
                    {
                        string text = (lineNr + 1).ToString();
                        while (text.Length < lines.Count.ToString().Length)
                            text = " " + text;
                        for (int i = 0; i < text.Length; i++)
                        {

                            Font font = fonts.Base;
                            g.DrawString(text[i].ToString(), font, Brushes.Gray,
                                         i*fonts.CharWidth, bounds.Y);
                        }
                    }

                    //Draw +/- to the left
                    if (lines[lineNr].BlockEndLine != null)
                    {
                        int buttonSize = 9;
                        int lineMid = bounds.Y + bounds.Height / 2;
                        g.FillRectangle(Brushes.White, leftLineCenter - buttonSize / 2 - 1,
                                                 lineMid - buttonSize / 2 - 1,
                                                 buttonSize + 2, buttonSize + 2);
                        g.DrawRectangle(controlPen, leftLineCenter - buttonSize / 2, lineMid - buttonSize / 2,
                                                 buttonSize - 1,
                                                 buttonSize - 1);
                        //Draw -
                        g.DrawLine(Pens.Black, leftLineCenter - buttonSize / 2 + 2, lineMid,
                                            leftLineCenter + buttonSize / 2 - 2, lineMid);
                        //Draw +
                        if (!lines[lineNr].BlockVisible)
                            g.DrawLine(Pens.Black, leftLineCenter, lineMid - buttonSize / 2 + 2,
                                                leftLineCenter, lineMid + buttonSize / 2 - 2);
                    }
                }
                DrawLine(lineNr, g, bounds, fonts);

                //Draw caret
                if (caret.GetPosition(true).Line == lineNr)
                {
                    Point caretpos = GetPixelAtTextpoint(caret.GetPosition(true));
                    g.DrawLine(caret.Shown && Focused ? Pens.Black : Pens.White, caretpos.X, caretpos.Y, caretpos.X,
                                        caretpos.Y + bounds.Height - 1);
                }

                lineNr++;
                bounds.Y += bounds.Height;
            }

            //g.Flush();
            InvalidateAll = false;
            e.Graphics.DrawImage(bg, 0, 0);
            //e.Graphics.DrawImage(snowman, ClientSize.Width - snowman.Width, ClientSize.Height - snowman.Height);
            //base.OnPaint(e);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message + "\n\n" + err.StackTrace);
            }
        }

        private void DrawLine(int lineNr, Graphics g, Rectangle bounds, FontScheme fonts)
        {
            Line line = lineNr < lines.Count ? lines[lineNr] : null;
            

            if (line != null)
            {
                if (!line.Invalidated && !InvalidateAll)
                    return;
                if (line.edited || RestyleAll) line.Restyle(fonts, lines, lineNr);

                DrawLineBackground(g, bounds);
                //g.DrawString(line.Text, fonts.Base, Brushes.Black, bounds.X, bounds.Y);
                DrawString(g, lineNr, line.Text, bounds);

                if (line.BlockEndLine != null && !line.BlockVisible)
                {
                    //Draw ... box
                    Rectangle boxBounds = new Rectangle(bounds.X + (line.Text.Length + 1) * fonts.CharWidth, bounds.Y, 4*fonts.CharWidth, bounds.Height - 1);
                    Pen controlPen = new Pen(Color.FromArgb(165, 165, 165));
                    g.DrawRectangle(controlPen, boxBounds);
                    for (int i = 0; i < 3; i++)
                    {
                        g.DrawString(".", fonts.Base, new SolidBrush(controlPen.Color), boxBounds.X + i * fonts.CharWidth, boxBounds.Y);
                        
                    }
                }

                line.Invalidated = false;
            }
            else
            {
                DrawLineBackground(g, bounds);
            }
        }

        private void DrawString(Graphics g, int lineNr, string text, Rectangle bounds)
        {
            
            for (int i = 0; i <= text.Length; i++)
            {
                TextPoint textPoint = new TextPoint(lineNr, i);
                if (IsTextpointMarked(textPoint))
                {
                    Point pixel = GetPixelAtTextpoint(textPoint);
                    int width = fonts.CharWidth;
                    if (i < text.Length && text[i] > 0xFF)
                        width += fonts.CharWidth;
                    else if (i < text.Length && text[i] == '\t')
                        width = 4*fonts.CharWidth;
                    g.FillRectangle(new SolidBrush(Color.FromArgb(173, 214, 255)), pixel.X,
                                    pixel.Y, width, fonts.Base.Height);
                }
            }
            Font font = new Font(fonts.Base, lines[lineNr].GetFontStyle(0).Style);
            int x = bounds.X;
            for (int i = 0; i < text.Length; i++)
            {
                FontModification modification = lines[lineNr].GetFontStyle(i);
                if (font.Style != modification.Style)
                    font = new Font(fonts.Base, modification.Style);
                g.DrawString(text[i].ToString(), font, new SolidBrush(modification.Color), x, bounds.Y);
                x += fonts.CharWidth;
                if (text[i] > 0xFF)
                    x += fonts.CharWidth;
                else if (text[i] == '\t')
                    x += 3*fonts.CharWidth;
            }
        }

        private void DrawLineBackground(Graphics g, Rectangle bounds)
        {
            //Just default it to white background.
            g.FillRectangle(IsReadonly ? new SolidBrush(Color.FromArgb(240, 240, 240)) : Brushes.White, bounds);
        }

        

        private DateTime lastMouseDown;
        private Point lastMouseDownPos;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                horizontalScrollBar.Value = Math.Max(0, Math.Min(horizontalScrollBar.Value + 50, horizontalScrollBar.Maximum));
                return;
            }
            if ( e.Button == MouseButtons.XButton2)
            {
                horizontalScrollBar.Value = Math.Max(0, Math.Min(horizontalScrollBar.Value - 50, horizontalScrollBar.Maximum));
                return;
            }

            bool shift = Control.ModifierKeys == Keys.Shift;
            Focus();

            if (Form1.Form.suggestionBox.Visible)
            {
                Form1.Form.suggestionBox.AllowHide = true;
                Form1.Form.suggestionBox.Hide();
            }

            if (TextRegion.Contains(e.Location))
            {
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    if (TextMarked && !shift)
                    {
                        InvalidateAll = true;
                        TextMarked = false;
                    }
                    if (!TextMarked && shift)
                    {
                        mouseDownPos = caret.GetPosition(true);
                        TextMarked = true;
                    }
                    if (!shift)
                        mouseDownPos = GetTextpointAtPixel(e.X, e.Y);
                    else
                        InvalidateAll = true;
                    leftMouseDown = true;
                    lines[caret.GetPosition(true).Line].Invalidated = true;
                    caret.SetPosition(GetTextpointAtPixel(e.X, e.Y), true);
                    caret.Shown = true;
                    CaretMoved(false);
                    FindAndReplaceForm.form.ResetPos();
                    Invalidate();
                }
                //If double click
                lastMouseDownPos.X -= e.X;
                lastMouseDownPos.Y -= e.Y;
                lastMouseDownPos.X *= lastMouseDownPos.X;
                lastMouseDownPos.Y *= lastMouseDownPos.Y;
                if (!shift && (DateTime.Now - lastMouseDown).TotalMilliseconds < SystemInformation.DoubleClickTime && 
                    lastMouseDownPos.X < SystemInformation.DoubleClickSize.Width && lastMouseDownPos.Y < SystemInformation.DoubleClickSize.Height)
                {
                    //Mark current word
                    TextPoint caretPos = caret.GetPosition(true);
                    int min, max;
                    min = max = caretPos.Pos;
                    min--;
                    leftMouseDown = false;
                    while (min >= 0 && Util.IsIdentifierLetter(lines[caretPos.Line].Text[min]))
                        min--;
                    while (max < lines[caretPos.Line].Text.Length &&
                            Util.IsIdentifierLetter(lines[caretPos.Line].Text[max]))
                        max++;
                    min++;
                    mouseDownPos = new TextPoint(caretPos.Line, min);
                    caret.SetPosition(new TextPoint(caretPos.Line, max), true);
                    TextMarked = true;
                    //InvalidateAll = true;
                    lines[caretPos.Line].Invalidated = true;
                    caret.Shown = true;
                    Invalidate();
                }
                lastMouseDownPos = e.Location;
                lastMouseDown = DateTime.Now;
            }
            else
            {//Might have clicked a button to hide/show block
                int line = GetTextpointAtPixel(e.X, e.Y).Line;
                if (e.X < TextRegion.X && line < lines.Count && lines[line].BlockEndLine != null)
                {
                    bool visible = lines[line].BlockVisible = !lines[line].BlockVisible;
                    Line endLine = lines[line].BlockEndLine;
                    line++;
                    List<Line> modifiedLines = new List<Line>();
                    while (lines[line] != endLine)
                    {
                        modifiedLines.Add(lines[line]);
                        lines[line].LineVisible = visible;
                        //If we wanted to show the block, and it contains hidden blocks. dont show those
                        if (lines[line].BlockEndLine != null && lines[line].BlockVisible == false && visible)
                        {
                            Line anotherEndLine = lines[line].BlockEndLine;
                            while (lines[line] != anotherEndLine)
                            {
                                line++;
                            }
                            continue;
                        }
                        line++;
                    }
                    if (visible)
                        ShowedLines(modifiedLines);
                    else
                        HidLines(modifiedLines);

                    InvalidateAll = true;
                    Invalidate();
                }
            }
        }

        

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                leftMouseDown = false;
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool inTextRegion = TextRegion.Contains(e.Location);
            Cursor = inTextRegion ? Cursors.IBeam : Cursors.Default;

            if (leftMouseDown)
            {
                TextPoint mousePos = GetTextpointAtPixel(e.X, e.Y);
                if (mousePos != caret.GetPosition(true))
                {
                    TextMarked = mousePos != mouseDownPos;

                    //Ensure visible
                    if (e.X > TextRegion.Width + TextRegion.X && horizontalScrollBar.Visible)
                    {
                        int pixelsOut = Math.Min(10, e.X - TextRegion.Width - TextRegion.X)/2;
                        horizontalScrollBar.Value = Math.Min(horizontalScrollBar.Maximum,
                                                             horizontalScrollBar.Value + pixelsOut);
                    }
                    if (e.X < TextRegion.X && horizontalScrollBar.Visible)
                    {
                        int pixelsOut = Math.Min(10, TextRegion.X - e.X);
                        horizontalScrollBar.Value = Math.Max(0,
                                                             horizontalScrollBar.Value - pixelsOut)/2;   
                    }
                    if (e.Y > TextRegion.Height + TextRegion.Y && verticalScrollBar.Visible)
                    {
                        int pixelsOut = Math.Min(2, e.Y - TextRegion.Height - TextRegion.Y) / 2;
                        verticalScrollBar.Value = Math.Min(verticalScrollBar.Maximum,
                                                             verticalScrollBar.Value + pixelsOut);
                    }
                    if (e.Y < TextRegion.Y && verticalScrollBar.Visible)
                    {
                        int pixelsOut = Math.Min(5, TextRegion.Y - e.Y) / 2;
                        verticalScrollBar.Value = Math.Max(0,
                                                             verticalScrollBar.Value - pixelsOut);
                    }

                    InvalidateAll = true;
                    caret.SetPosition(mousePos, true);
                    caret.Shown = true;
                    Invalidate();
                    
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (parentForm.suggestionBox.Visible)
            {
                Point mousePos = new Point();
                GetCursorPos(ref mousePos);
                if (parentForm.suggestionBox.RectangleToScreen(parentForm.suggestionBox.DisplayRectangle).Contains(mousePos))
                {
                    parentForm.suggestionBox.Scroll(-e.Delta);
                    return;
                }
            }
            //120 for up, -120 for down
            if (ModifierKeys == Keys.Shift)
                horizontalScrollBar.Value = Math.Max(0, Math.Min(horizontalScrollBar.Value - e.Delta / 12, horizontalScrollBar.Maximum));
            else
                verticalScrollBar.Value = Math.Max(0, Math.Min(verticalScrollBar.Value - e.Delta/30, verticalScrollBar.Maximum));
        }



        protected override void OnSizeChanged(EventArgs e)
        {
            Bitmap newBg = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(newBg);
            g.DrawImage(bg, 0, 0);
            g.FillRectangle(Brushes.White, bg.Width - snowman.Width, bg.Height - snowman.Height, snowman.Width, snowman.Height);
            //g.DrawImage(snowman, 0, 0);//ClientSize.Width - snowman.Width, ClientSize.Height - snowman.Height);
            g.Flush();
            bg = newBg;

            InvalidateAll = true;
            UpdateScrollBars();
            Invalidate();
            base.OnSizeChanged(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & Keys.Tab) != Keys.None || (keyData & Keys.Up) != Keys.None)
            {
                KeyEventArgs eventArg = new KeyEventArgs(keyData);
                OnKeyDown(eventArg);
                return eventArg.Handled;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        public event KeyEventHandler PreviewKeyDown;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (PreviewKeyDown != null) PreviewKeyDown(this, e);
            if (!e.Handled)
                e.Handled = ExecuteKey(e);
            

            base.OnKeyDown(e);
        }


        private bool ExecuteKey(KeyEventArgs e)
        {
            TextPoint caretPos = caret.GetPosition(true);
            bool shift = e.Shift;
            bool ctrl = e.Control;
            Keys keys = e.KeyData;
            if (shift) keys = e.KeyData ^ Keys.Shift;
            if (ctrl) keys = keys ^ Keys.Control;
            //Only handle special keys
            
            switch (keys)
            {
                case Keys.Tab:
                    
                    if (IsReadonly) break;
                    if (!TextMarked)
                    {
                        if (!Options.Editor.ReplaceTabsWithSpaces)
                        {
                            lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos, "\t");
                            lines[caretPos.Line].Invalidated = true;

                            UndoSys.TextAdded("\t", this, caretPos);
                            caretPos.Pos++;
                            caret.SetPosition(caretPos, true);
                            ExtendedLines(new List<Line>() { lines[caretPos.Line] });
                            Invalidate();
                            TextEdited();
                            CaretMoved(false);
                            return true;
                        }

                        int pos;
                        if (shift)
                        {//Remove spaces up to previous tab
                            int count = 0;
                            while (caretPos.Pos > 0 && lines[caretPos.Line].Text[caretPos.Pos - count - 1] == ' ')
                            {
                                if (caretPos.Pos - count == 0)
                                    break;
                                count++;
                                if ((caretPos.Pos - count) % 4 == 0)
                                {
                                    break;
                                }
                            }
                            if (count == 0)
                                return true;
                            UndoSys.TextRemoved(lines[caretPos.Line].Text.Substring(caretPos.Pos - count, count), this, new TextPoint(caretPos.Line, caretPos.Pos - count));
                            lines[caretPos.Line].Text = lines[caretPos.Line].Text.Remove(caretPos.Pos - count, count);
                            lines[caretPos.Line].Invalidated = true;
                            caretPos.Pos -= count;
                            caret.SetPosition(caretPos, true);
                            ShrunkLine(lines[caretPos.Line]);
                            Invalidate();
                            TextEdited();
                            CaretMoved(false);
                            return true;
                        }

                        pos = caretPos.Pos + 1;
                        string textInsert = " ";
                        while (pos % 4 > 0)
                        {
                            pos++;
                            textInsert += " ";
                        }
                        lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos, textInsert);
                        lines[caretPos.Line].Invalidated = true;

                        UndoSys.TextAdded(textInsert, this, caretPos);
                        caretPos.Pos += textInsert.Length;
                        caret.SetPosition(caretPos, true);
                        ExtendedLines(new List<Line>(){lines[caretPos.Line]});
                        Invalidate();
                        TextEdited();
                        CaretMoved(false);
                        return true;
                    }

                    List<Line> modifiedLines = new List<Line>();
                    int min = Math.Min(caretPos.Line, mouseDownPos.Line);
                    int max = Math.Max(caretPos.Line, mouseDownPos.Line);
                    

                    string oldText = "";
                    for (int i = min; i <= max; i++)
                    {
                        oldText += lines[i].Text;
                        if (i != max)
                            oldText += "\n";
                    }

                    string newText;
                    /*if (!Options.Editor.ReplaceTabsWithSpaces)
                    {
                        newText = "";
                        for (int i = min; i <= max; i++)
                        {
                            lines[i].Indents++;
                            lines[i].Invalidated = true;
                            newText += lines[i].Text;
                        }
                    }*/
                    int spacesCaret = 0;
                    foreach (char t in lines[caretPos.Line].Text)
                    {
                        if (t == ' ')
                            spacesCaret--;
                        else
                            break;
                    }
                    int spacesMarkStart = 0;
                    foreach (char t in lines[mouseDownPos.Line].Text)
                    {
                        if (t == ' ')
                            spacesMarkStart--;
                        else
                            break;
                    }
                    for (int i = min; i <= max; i++)
                    {
                        modifiedLines.Add(lines[i]);
                        lines[i].Invalidated = true;
                        if (e.Shift)
                            lines[i].Indents--;
                        else
                            lines[i].Indents++;
                    }
                    newText = "";
                    for (int i = min; i <= max; i++)
                    {
                        newText += lines[i].Text;
                        if (i != max)
                            newText += "\n";
                    }
                    UndoSys.TextReplaced(oldText, newText, this, new TextPoint(min, 0));

                    spacesCaret += 4 * lines[caretPos.Line].Indents;
                    if (!Options.Editor.ReplaceTabsWithSpaces && spacesCaret != 0) spacesCaret /= Math.Abs(spacesCaret);
                    if (caretPos.Pos + spacesCaret < 0)
                        spacesCaret = -caretPos.Pos;
                    caretPos = new TextPoint(caretPos.Line, caretPos.Pos + spacesCaret);
                    caret.SetPosition(caretPos, true);

                    spacesMarkStart += 4 * lines[mouseDownPos.Line].Indents;
                    if (!Options.Editor.ReplaceTabsWithSpaces && spacesMarkStart != 0)  spacesMarkStart /= Math.Abs(spacesMarkStart);
                    mouseDownPos = new TextPoint(mouseDownPos.Line, mouseDownPos.Pos + spacesMarkStart);

                    ExtendedLines(modifiedLines);
                    Invalidate();
                    TextEdited();
                    CaretMoved(false);
                    return true;
                case Keys.Delete:
                    if (IsReadonly) break;
                    if (TextMarked)
                    {
                        DeleteMarkedText();
                        caretPos = caret.GetPosition(true);
                    }
                        //Li|ne1
                    else if (lines[caretPos.Line].Text.Length > caretPos.Pos)
                    {
                        string removedPart = lines[caretPos.Line].Text.Substring(caretPos.Pos, 1);
                        UndoSys.TextRemoved(removedPart, this, new TextPoint(caretPos.Line, caretPos.Pos));
                        lines[caretPos.Line].Text = lines[caretPos.Line].Text.Remove(caretPos.Pos, 1);
                        lines[caretPos.Line].edited = true;
                        lines[caretPos.Line].Invalidated = true;
                        ShrunkLine(lines[caretPos.Line]);
                        UpdateBlocks();
                        Invalidate();
                        TextEdited();
                    }
                    //Line1|
                    //Line2
                    else if (lines.Count > caretPos.Line + 1)
                    {
                        UndoSys.TextRemoved("\n", this, new TextPoint(caretPos.Line, caretPos.Pos));
                        lines[caretPos.Line].Text += lines[caretPos.Line + 1].Text;
                        lines[caretPos.Line].edited = true;
                        lines.RemoveAt(caretPos.Line + 1);
                        //InvalidateAll = true;
                        for (int i = caretPos.Line; i < lines.Count; i++)
                        {
                            lines[i].Invalidated = true;
                        }
                        ExtendedLines(new List<Line>{lines[caretPos.Line]});
                        UpdateBlocks();
                        Invalidate();
                        TextEdited();
                    }
                    return true;
                case Keys.Back:
                    if (IsReadonly) break;
                    if (TextMarked)
                    {
                        DeleteMarkedText();
                        caretPos = caret.GetPosition(true);
                    }
                    //Li|ne1
                    else if (caretPos.Pos > 0)
                    {
                        string removedPart = lines[caretPos.Line].Text.Substring(
                            caretPos.Pos - 1, 1);
                        lines[caretPos.Line].Text = lines[caretPos.Line].Text.Remove(
                            caretPos.Pos - 1, 1);
                        lines[caretPos.Line].Invalidated = true;
                        lines[caretPos.Line].edited = true;
                        UndoSys.TextRemoved(removedPart, this, new TextPoint(caretPos.Line, caretPos.Pos - 1));
                        caret.SetPosition(new TextPoint(caretPos.Line, caretPos.Pos - 1), true);
                        ShrunkLine(lines[caretPos.Line]);
                        UpdateBlocks();
                        Invalidate();
                        TextEdited();
                    }
                    //Line1
                    //|Line2
                    else if (caretPos.Line > 0)
                    {
                        TextPoint newPos = new TextPoint(caretPos.Line - 1,
                                                         lines[caretPos.Line - 1].Text.Length);
                        lines[caretPos.Line - 1].Text += lines[caretPos.Line].Text;
                        lines.RemoveAt(caretPos.Line);
                        //InvalidateAll = true;
                        for (int l = caretPos.Line - 1; l < lines.Count; l++)
                        {
                            lines[l].Invalidated = true;
                        }
                        UndoSys.TextRemoved("\n", this, new TextPoint(newPos.Line, newPos.Pos));
                        caret.SetPosition(newPos, true);
                        lines[newPos.Line].edited = true;
                        ExtendedLines(new List<Line> { lines[newPos.Line] });
                        UpdateBlocks();
                        Invalidate();
                        TextEdited();
                        CaretMoved();
                        return true;
                    }
                    CaretMoved();
                    return true;
                case Keys.Return:
                    if (IsReadonly) break;
                    DeleteMarkedText();
                    caretPos = caret.GetPosition(true);
                    List<Line> newLines = new List<Line>();
                    Line newLine = new Line(lines[caretPos.Line].Text.Substring(caretPos.Pos));
                    newLines.Add(newLine);
                    Line extraNewLine = null;
                    string undoText = "\n";
                    //If the next char was an }, move that to its own line
                    if (newLine.Text.Length > 0 && newLine.Text[0] == '}')
                    {
                        extraNewLine = new Line(newLine.Text);
                        newLines.Add(extraNewLine);
                        lines.Insert(caretPos.Line + 1, extraNewLine);  
                        newLine.Text = "";
                    }
                    string oldNewLineText = newLine.Text;
                    lines.Insert(caretPos.Line + 1, newLine);     
                    lines[caretPos.Line].Text = lines[caretPos.Line].Text.Substring(0, caretPos.Pos);
                    lines[caretPos.Line].edited = true;
                    lines[caretPos.Line].Restyle(fonts, lines, caretPos.Line);
                    newLine.Indents = newLine.GetWantedIndents(fonts, lines, caretPos.Line + 1);
                    if (newLine.Text.Length - oldNewLineText.Length >= 0 && oldNewLineText.Length > 0)
                        undoText += newLine.Text.Remove(newLine.Text.Length - oldNewLineText.Length);
                    else
                        undoText += newLine.Text;
                    if (extraNewLine != null)
                    {
                        oldNewLineText = extraNewLine.Text;
                        extraNewLine.Indents = extraNewLine.GetWantedIndents(fonts, lines, caretPos.Line + 2);
                        if (extraNewLine.Text.Length - oldNewLineText.Length >= 0 && oldNewLineText.Length > 0)
                            undoText += "\n" + extraNewLine.Text.Remove(extraNewLine.Text.Length - oldNewLineText.Length);
                        else
                            undoText += "\n" + extraNewLine.Text;
                    }

                    caret.SetPosition(new TextPoint(caretPos.Line + 1, Options.Editor.ReplaceTabsWithSpaces ? newLine.Indents*4 : newLine.Indents), true);
                    //InvalidateAll = true;
                    for (int l = caretPos.Line; l < lines.Count; l++)
                    {
                        lines[l].Invalidated = true;
                    }
                    ShrunkLine(lines[caretPos.Line]);
                    ExtendedLines(newLines);
                    UndoSys.TextAdded(undoText, this, caretPos);
                    UpdateBlocks();
                    Invalidate();
                    TextEdited();
                    CaretMoved();
                    return true;
                case Keys.Right: 
                    if (!TextMarked)
                    {
                        TextMarked = shift;
                        mouseDownPos = caretPos;
                    }
                    //if (!shift)
                    //    TextMarked = false;
                    while (true)
                    {
                        if (lines[caretPos.Line].Text.Length > caretPos.Pos)
                        {
                            lines[caretPos.Line].Invalidated = true;

                            int pos = caretPos.Pos + 1;

                            if (ctrl)
                            {
                                List<Token> tokens = new List<Token>();
                                Lexer lexer = new Lexer(new StringReader(lines[caretPos.Line].Text));
                                {
                                    Token token;
                                    while (!((token = lexer.Next()) is EOF))
                                    {
                                        if (token is TWhiteSpace)
                                            continue;
                                        tokens.Add(token);
                                    }
                                }
                                bool moved = false;
                                foreach (Token token in tokens)
                                {
                                    if (token.Pos - 1 >= pos)
                                    {
                                        pos = token.Pos - 1;
                                        moved = true;
                                        break;
                                    }
                                }
                                if (!moved)
                                {
                                    caretPos.Pos = lines[caretPos.Line].Text.Length;
                                    continue;
                                }
                            }

                            caret.SetPosition(new TextPoint(caretPos.Line, pos), true);
                            Invalidate();
                        }
                        else if (lines.Count > caretPos.Line + 1)
                        {
                            lines[caretPos.Line].Invalidated = true;
                            //Skip over any potential hidden lines
                            caret.SetPosition(new TextPoint(caret.GetPosition(false).Line + 1, 0), false);

                            if (ctrl)
                            {
                                caretPos = caret.GetPosition(true);
                                int pos = caretPos.Pos;
                                List<Token> tokens = new List<Token>();
                                Lexer lexer = new Lexer(new StringReader(lines[caretPos.Line].Text));
                                {
                                    Token token;
                                    while (!((token = lexer.Next()) is EOF))
                                    {
                                        if (token is TWhiteSpace)
                                            continue;
                                        tokens.Add(token);
                                    }
                                }
                                if (tokens.Count > 0)
                                {
                                    Token token = tokens[0];
                                    pos = token.Pos - 1;
                                }
                                else
                                    pos = lines[caretPos.Line].Text.Length;
                                caret.SetPosition(new TextPoint(caretPos.Line, pos), true);
                            }
                            Invalidate();
                        }
                        break;
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.Left:
                    if (!TextMarked)
                    {
                        TextMarked = shift;
                        mouseDownPos = caretPos;
                    }
                    //if (!shift)
                    //    TextMarked = false;
                    while (true)
                    {
                        if (caretPos.Pos > 0)
                        {
                            lines[caretPos.Line].Invalidated = true;
                            int pos = caretPos.Pos - 1;

                            if (ctrl)
                            {
                                List<Token> tokens = new List<Token>();
                                Lexer lexer = new Lexer(new StringReader(lines[caretPos.Line].Text));
                                {
                                    Token token;
                                    while (!((token = lexer.Next()) is EOF))
                                    {
                                        if (token is TWhiteSpace)
                                            continue;
                                        tokens.Add(token);
                                    }
                                }
                                bool moved = false;
                                for (int i = tokens.Count - 1; i >= 0; i--)
                                {
                                    Token token = tokens[i];
                                    if (token.Pos - 1 <= pos)
                                    {
                                        pos = token.Pos - 1;
                                        moved = true;
                                        break;
                                    }
                                }
                                if (!moved)
                                {
                                    caretPos.Pos = 0;
                                    continue;
                                }
                            }

                            caret.SetPosition(new TextPoint(caretPos.Line, pos), true);
                            Invalidate();
                        }
                        else if (caretPos.Line > 0)
                        {
                            lines[caretPos.Line].Invalidated = true;
                            //Skip over any potential hidden lines
                            caret.SetPosition(new TextPoint(caret.GetPosition(false).Line - 1, 0), false);
                            caretPos = caret.GetPosition(true);
                            caret.SetPosition(new TextPoint(caretPos.Line, lines[caretPos.Line].Text.Length), true);

                            if (ctrl)
                            {
                                caretPos = caret.GetPosition(true);
                                int pos = caretPos.Pos;
                                List<Token> tokens = new List<Token>();
                                Lexer lexer = new Lexer(new StringReader(lines[caretPos.Line].Text));
                                {
                                    Token token;
                                    while (!((token = lexer.Next()) is EOF))
                                    {
                                        if (token is TWhiteSpace)
                                            continue;
                                        tokens.Add(token);
                                    }
                                }
                                if (tokens.Count > 0)
                                {
                                    Token token = tokens[tokens.Count - 1];
                                    pos = token.Pos - 1 + token.Text.Length;
                                }
                                caret.SetPosition(new TextPoint(caretPos.Line, pos), true);
                            }

                            Invalidate();
                        }
                        break;
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.Up:
                    if (!TextMarked)
                    {
                        TextMarked = shift;
                        mouseDownPos = caretPos;
                    }
                    //if (!shift)
                    //    TextMarked = false;
                    if (caretPos.Line > 0)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        lines[caretPos.Line - 1].Invalidated = true;

                        int oldPos = 0;
                        for (int i = 0; i < caretPos.Pos; i++)
                        {
                            oldPos++;
                            if (lines[caretPos.Line].Text[i] == '\t')
                                oldPos += 3;
                            else if (lines[caretPos.Line].Text[i] > 0xFF)
                                oldPos++;
                        }
                        caret.SetPosition(new TextPoint(caret.GetPosition(false).Line - 1, 0), false);
                        caretPos = caret.GetPosition(true);
                        int newPos = 0;
                        for (newPos = 0; newPos < lines[caretPos.Line].Text.Length; newPos++)
                        {
                            if (oldPos <= 0)
                                break;
                            if (lines[caretPos.Line].Text[newPos] == '\t')
                            {
                                oldPos -= 4;
                                if (oldPos < -2)
                                {
                                    newPos--;
                                }
                            }
                            else if (lines[caretPos.Line].Text[newPos] > 0xFF)
                                oldPos -= 2;
                            else
                                oldPos--;
                        }
                        caret.SetPosition(new TextPoint(caretPos.Line, /*Math.Min(lines[caretPos.Line].Text.Length, oldPos)*/newPos), true);
                        Invalidate();
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.Down:
                    if (!TextMarked)
                    {
                        TextMarked = shift;
                        mouseDownPos = caretPos;
                    }
                    //if (!shift)
                    //    TextMarked = false;
                    if (caretPos.Line + 1 < lines.Count)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        lines[caretPos.Line + 1].Invalidated = true;
                        int oldPos = 0;
                        for (int i = 0; i < caretPos.Pos; i++)
                        {
                            oldPos++;
                            if (lines[caretPos.Line].Text[i] == '\t')
                                oldPos += 3;
                            else if (lines[caretPos.Line].Text[i] > 0xFF)
                                oldPos++;
                        }
                        caret.SetPosition(new TextPoint(caret.GetPosition(false).Line + 1, 0), false);
                        caretPos = caret.GetPosition(true);
                        int newPos = 0;
                        for (newPos = 0; newPos < lines[caretPos.Line].Text.Length; newPos++)
                        {
                            if (oldPos <= 0)
                                break;
                            if (lines[caretPos.Line].Text[newPos] == '\t')
                            {
                                oldPos -= 4;
                                if (oldPos < -2)
                                {
                                    newPos--;
                                }
                            }
                            else if (lines[caretPos.Line].Text[newPos] > 0xFF)
                                oldPos -= 2;
                            else
                                oldPos--;
                        }
                        caret.SetPosition(new TextPoint(caretPos.Line, /*Math.Min(lines[caretPos.Line].Text.Length, oldPos)*/newPos), true);
                        Invalidate();
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.End:
                    if (!TextMarked)
                    {
                        TextMarked = true;
                        mouseDownPos = caretPos;
                    }
                    if (!shift)
                        TextMarked = false;
                    if (caretPos.Pos < lines[caretPos.Line].Text.Length)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        caret.SetPosition(new TextPoint(caretPos.Line, lines[caretPos.Line].Text.Length), true);
                        Invalidate();
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.Home:
                    if (!TextMarked)
                    {
                        TextMarked = true;
                        mouseDownPos = caretPos;
                    }
                    if (!shift)
                        TextMarked = false;
                    if (caretPos.Pos > 0)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        caret.SetPosition(new TextPoint(caretPos.Line, 0), true);
                        Invalidate();
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.PageUp:
                    if (!TextMarked)
                    {
                        TextMarked = true;
                        mouseDownPos = caretPos;
                    }
                    if (!shift)
                        TextMarked = false;
                    if (caretPos.Line > 0)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        int line = caretPos.Line;
                        int subtractedLines = 40;
                        while (line > 0 && subtractedLines > 0)
                        {
                            line--;
                            if (lines[line].LineVisible)
                                subtractedLines--;
                        }
                        int pos = caretPos.Pos;
                        if (pos > lines[line].Text.Length)
                            pos = lines[line].Text.Length;
                        caret.SetPosition(new TextPoint(line, pos), true);
                    }
                    CaretMoved(!shift);
                    return true;
                case Keys.PageDown:
                    if (!TextMarked)
                    {
                        TextMarked = true;
                        mouseDownPos = caretPos;
                    }
                    if (!shift)
                        TextMarked = false;
                    if (caretPos.Line + 1 < lines.Count)
                    {
                        lines[caretPos.Line].Invalidated = true;
                        int line = caretPos.Line;
                        int addedLines = 40;
                        while (line < lines.Count - 1 && addedLines > 0)
                        {
                            line++;
                            if (lines[line].LineVisible)
                                addedLines--;
                        }
                        int pos = caretPos.Pos;
                        if (pos > lines[line].Text.Length)
                            pos = lines[line].Text.Length;
                        caret.SetPosition(new TextPoint(line, pos), true);

                    }
                    CaretMoved(!shift);
                    return true;
            }
            //Handle ctrl commands
            if (e.Modifiers == Keys.Control)
            {
                //Copy
                if (e.KeyValue != 17)
                    e = e;
                if (e.KeyValue == 'C' || e.KeyValue == 'X')
                {
                    Copy(e.KeyValue == 'X');
                    return true;
                }
               
                //Paste
                if (e.KeyValue == 'V' && !IsReadonly)
                {
                    Paste();
                    return true;
                }

                //Goto line
                if (e.KeyValue == 'G')
                {
                    GotoLineForm dialog = new GotoLineForm(caretPos.Line + 1, lines.Count);
                    if (dialog.ShowDialog(parentForm) == DialogResult.OK)
                    {
                        caret.SetPosition(new TextPoint(dialog.SelectedLine - 1, 0), true);
                        EnsureLineVisible(dialog.SelectedLine - 1);
                        CaretMoved();
                    }
                    return true;
                }

                if (e.KeyValue == 'A')
                {
                    mouseDownPos = new TextPoint(0, 0);
                    caret.SetPosition(new TextPoint(lines.Count - 1, lines[lines.Count - 1].Text.Length), true);
                    TextMarked = true;
                    InvalidateAll = true;
                    CaretMoved(false);
                    return true;
                }

                if (e.KeyValue  == 'F' || e.KeyValue == 'R')
                {
                    //FindAndReplaceForm.form.SetStart(((Form1.OpenFileData)Tag).File, caretPos);
                    OpenFindAndReplace();
                    return true;
                }

                if (e.KeyValue == 'Z')
                {
                    UndoSys.Undo();
                    return true;
                }

                if (e.KeyValue == 'Y')
                {
                    UndoSys.Redo();
                    return true;
                }
            }
            return false;
        }

        public void Copy(bool cut)
        {
            if (TextMarked)
            {
                TextPoint caretPos = caret.GetPosition(true);
                StringBuilder text = new StringBuilder("");
                TextPoint min = TextPoint.Min(caretPos, mouseDownPos);
                TextPoint max = TextPoint.Max(caretPos, mouseDownPos);
                for (int line = min.Line; line <= max.Line; line++)
                {
                    if (line == min.Line && line == max.Line)
                        text.AppendLine(lines[line].Text.Substring(min.Pos, max.Pos - min.Pos));
                    else if (line == min.Line)
                        text.AppendLine(lines[line].Text.Substring(min.Pos));
                    else if (line == max.Line)
                        text.AppendLine(lines[line].Text.Substring(0, Math.Min(lines[line].Text.Length, max.Pos)));
                    else
                        text.AppendLine(lines[line].Text);
                }
                //We added one too many \n
                text = text.Remove(text.Length - 1, 1);
                Clipboard.Clear();
                try
                {
                    Clipboard.SetText(text.ToString());
                }
                catch (Exception err)
                {
                    return;
                }
                if (cut && !IsReadonly)
                    DeleteMarkedText();
            }

        }

        public void Paste()
        {
            DeleteMarkedText();
            TextPoint caretPos = caret.GetPosition(true);
            string text = Clipboard.GetText().Replace("\r", "");
            if (Options.Editor.ReplaceTabsWithSpaces)
                text = text.Replace("\t", "    ");
            string[] texts = text.Split('\n');
            UndoSys.TextAdded(text, this, caretPos);

            //Only currently existing line thats edited is the current line
            lines[caretPos.Line].edited = true;
            lines[caretPos.Line].Invalidated = true;

            //Insert first line to current line
            int caretFinalPos = texts[texts.Length - 1].Length;

            texts[texts.Length - 1] += lines[caretPos.Line].Text.Remove(0, caretPos.Pos);
            lines[caretPos.Line].Text = lines[caretPos.Line].Text.Substring(0, caretPos.Pos);

            if (texts.Length == 1) caretFinalPos += lines[caretPos.Line].Text.Length;
            lines[caretPos.Line].Text += texts[0];

            //Insert a line for all the other lines
            int line = caretPos.Line;
            for (int i = 1; i < texts.Length; i++)
            {
                line++;
                // if (caret.Position.Line < lines.Count)
                lines.Insert(line, new Line(texts[i]) { Invalidated = true });
                // else
                //   lines.Add(new Line(lines[i].Text));
            }

            for (int i = caretPos.Line; i < lines.Count; i++)
            {
                lines[i].Invalidated = true;
            }

            caret.SetPosition(new TextPoint(line, caretFinalPos), true);

            RecalculateWidestLine();
            //InvalidateAll = true;
            Invalidate();

            UpdateBlocks();
            TextEdited();
            CaretMoved();
        }

        public void OpenFindAndReplace()
        {
            TextPoint caretPos = caret.GetPosition(true);
            if (TextMarked)
                FindAndReplaceForm.form.InitSearch(GetTextWithin(TextPoint.Min(caretPos, mouseDownPos),
                                                                 TextPoint.Max(caretPos, mouseDownPos)));
            else
                FindAndReplaceForm.form.InitSearch();
        }

        private void DeleteMarkedText()
        {
            if (!TextMarked)
                return;
            
            TextPoint min = TextPoint.Min(caret.GetPosition(true), mouseDownPos);
            TextPoint max = TextPoint.Max(caret.GetPosition(true), mouseDownPos);
            string removedText = "";
            for (int line = min.Line; line <= max.Line; line++)
            {
                if (line == min.Line)
                {
                    if (line == max.Line)
                        removedText += lines[line].Text.Remove(0, min.Pos).Substring(0, max.Pos - min.Pos);
                    else
                        removedText += lines[line].Text.Remove(0, min.Pos) + "\n";
                }
                else 
                {
                    if (line == max.Line)
                        removedText += lines[line].Text.Substring(0, max.Pos);
                    else
                        removedText += lines[line].Text + "\n";
                }
            }
            UndoSys.TextRemoved(removedText, this, new TextPoint(min.Line, min.Pos));

            caret.SetPosition(min, true);

            lines[min.Line].Invalidated = true;
            lines[min.Line].edited = true;



            if (min.Line == max.Line)
                //Cut text between min and max
                lines[min.Line].Text = lines[min.Line].Text.Remove(min.Pos, max.Pos - min.Pos);
            else
                //Cut end of min line, and replace with end of max line
                lines[min.Line].Text = lines[min.Line].Text.Substring(0, min.Pos) + lines[max.Line].Text.Remove(0, max.Pos);
            //Cut lines between min and max
            while (max.Line > min.Line)
            {
                lines.RemoveAt(min.Line + 1);
                max.Line--;
            }

            for (int i = min.Line; i < lines.Count; i++)
            {
                lines[i].Invalidated = true;
            }

            RecalculateWidestLine();
            UpdateBlocks();
            TextEdited();
            CaretMoved();
            Invalidate();
        }

        protected void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        public event KeyPressEventHandler PreviewKeyPress;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (PreviewKeyPress != null)
                PreviewKeyPress(this, e);

            if (!e.Handled && (e.KeyChar >= 32 && e.KeyChar <= 126 || e.KeyChar > 0xFF) && !IsReadonly)
            {
                DeleteMarkedText();
                TextPoint caretPos = caret.GetPosition(true);
                lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos,
                                                                                         e.KeyChar.ToString());
                lines[caretPos.Line].Invalidated = true;
                lines[caretPos.Line].edited = true;
                string undoText = e.KeyChar.ToString();
                if (e.KeyChar == '{' && Options.Editor.InsertEndBracket)
                {
                    undoText += "}";
                    lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos + 1,
                                                                                         "}");
                    //No need to update blocks here, as theese are on same line anyway
                }
                TextPoint undoPos = caretPos;
                if (e.KeyChar == ';' || e.KeyChar == '{' || e.KeyChar == ':')
                {
                    int unindentedCaretPos = caretPos.Pos;
                    foreach (char c in lines[caretPos.Line].Text)
                    {
                        if (c == ' ' || c == '\t')
                            unindentedCaretPos--;
                        else
                            break;
                    }

                    int spaces = 0;
                    foreach (char t in lines[caretPos.Line].Text)
                    {
                        if (t == ' ')
                            spaces--;
                        else if (t == '\t')
                            spaces -= 4;
                        else
                            break;
                    }
                    lines[caretPos.Line].Indents = lines[caretPos.Line].GetWantedIndents(fonts, lines, caretPos.Line);
                    spaces += 4*lines[caretPos.Line].Indents;
                    if (spaces > 0)
                    {
                        undoText = "";
                        for (int i = 0; i < spaces; i++)
                        {
                            undoText += ' ';
                        }
                        if (!Options.Editor.ReplaceTabsWithSpaces)
                            undoText = undoText.Replace("    ", "\t");
                        UndoSys.TextAdded(undoText, this, new TextPoint(caretPos.Line, 0));
                    }
                    if (spaces < 0)
                    {
                        undoText = "";
                        for (int i = 0; i < -spaces; i++)
                        {
                            undoText += ' ';
                        }
                        if (!Options.Editor.ReplaceTabsWithSpaces)
                            undoText = undoText.Replace("    ", "\t");
                        UndoSys.TextRemoved(undoText, this, new TextPoint(caretPos.Line, 0));
                    }
                    foreach (char c in lines[caretPos.Line].Text)
                    {
                        if (c == ' ' || c == '\t')
                            unindentedCaretPos++;
                        else
                            break;
                    }
                    if (unindentedCaretPos < 0)
                        unindentedCaretPos = 0;
                    caretPos = new TextPoint(caretPos.Line, unindentedCaretPos + 1);
                    caret.SetPosition(caretPos, true);

                }
                else if (e.KeyChar == '}')
                {//Fix indents
                    //Search for partner
                    int openBraces = 0;
                    int startLine = -1;
                    TextPoint currentPos = new TextPoint(caretPos.Line, caretPos.Pos - 1);
                    while (true)
                    {
                        while (currentPos.Pos < 0)
                        {
                            currentPos.Line--;
                            if (currentPos.Line < 0)
                                break;
                            currentPos.Pos = lines[currentPos.Line].Text.Length - 1;
                        }
                        if (currentPos.Line < 0)
                            break;

                        char ch = lines[currentPos.Line].Text[currentPos.Pos];
                        if (ch == '}')
                            openBraces++;
                        else if (ch == '{')
                        {
                            if (openBraces > 0)
                                openBraces--;
                            else
                            {
                                startLine = currentPos.Line;
                                break;
                            }
                        }
                        currentPos.Pos--;
                    }
                    
                    if (startLine >= 0)
                    {
                        string oldText = "";
                        for (int i = startLine; i <= caretPos.Line; i++)
                        {
                            oldText += lines[i].Text;
                            if (i != caretPos.Line)
                                oldText += "\n";
                        }
                        string insertedOldText = "";
                        for (int i = startLine; i <= caretPos.Line; i++)
                        {
                            if (i == caretPos.Line)
                                insertedOldText += lines[i].Text.Substring(0, caretPos.Pos);
                            else
                                insertedOldText += lines[i].Text + "\n";
                                
                        }

                        int spaces = 0;
                        foreach (char t in lines[caretPos.Line].Text)
                        {
                            if (t == ' ')
                                spaces--;
                            else if (t == '\t')
                                spaces -= 4;
                            else
                                break;
                        }
                        for (int i = startLine; i <= caretPos.Line; i++)
                        {
                            lines[i].Indents = lines[i].GetWantedIndents(fonts, lines, i);
                        }

                        string newText = "";
                        for (int i = startLine; i <= caretPos.Line; i++)
                        {
                            newText += lines[i].Text;
                            if (i != caretPos.Line)
                                newText += "\n";
                        }

                        if (oldText != newText)
                        {
                            undoText = "";
                            //Remove the { from oldText


                            UndoSys.TextReplaced(insertedOldText, newText, this, new TextPoint(startLine, 0));
                        }

                        spaces += 4 * lines[caretPos.Line].Indents;
                        if (!Options.Editor.ReplaceTabsWithSpaces) spaces /= 4;
                            
                        caretPos = new TextPoint(caretPos.Line, caretPos.Pos + 1 + spaces);
                        caret.SetPosition(caretPos, true);

                    }
                    else
                    {
                        caretPos = new TextPoint(caretPos.Line, caretPos.Pos + 1);
                        caret.SetPosition(caretPos, true);
                    }

                    UpdateBlocks();
                }
                else
                {
                    caretPos = new TextPoint(caretPos.Line, caretPos.Pos + 1);
                    caret.SetPosition(caretPos, true);
                }
                if (undoText != "")
                    UndoSys.TextAdded(undoText, this, undoPos);
                e.Handled = true;
                ExtendedLines(new List<Line>() {lines[caretPos.Line]});
                TextEdited();
                CaretMoved();
                Invalidate();
            }

            base.OnKeyPress(e);
        }

        private void TextEdited()
        {
            UpdateScrollBars();
            if (OnTextEdited != null) OnTextEdited(this);
        }

        private void CaretMoved(bool removeMarked = true)
        {
            UndoSystem.AddUnsureEditorSignal(this);
            if (removeMarked && TextMarked)
            {
                InvalidateAll = true;
                TextMarked = false;
            }
            caret.Shown = true;
            caretTimer.Stop();
            caretTimer.Start();

            //First, check if the caret is outside the current bounds
            Rectangle textRegion = TextRegion;
            TextPoint caretPos = caret.GetPosition(true);
            Point caretPixel = GetPixelAtTextpoint(caret.GetPosition(false));
            if (!textRegion.Contains(caretPixel) || !textRegion.Contains(caretPixel.X, caretPixel.Y + fonts.Base.Height))
            {
                //If current line is not fully visible, scroll as little as possible
                int visibleLines = textRegion.Height/fonts.Base.Height;
                if (caretPos.Line < verticalScrollBar.Value)
                    verticalScrollBar.Value = caretPos.Line;
                if (caretPos.Line + 1 > verticalScrollBar.Value + visibleLines)
                    verticalScrollBar.Value = caretPos.Line - visibleLines + 1;

                //If current pos is not visible, scroll to fit it 10% in
                if (caretPixel.X < textRegion.X)
                    horizontalScrollBar.Value = Math.Max(0,
                                                       horizontalScrollBar.Value - textRegion.X -
                                                       (int)(0.2f * textRegion.Width) + caretPixel.X);
                if (caretPixel.X > textRegion.Right)
                    horizontalScrollBar.Value = Math.Min(horizontalScrollBar.Maximum,
                                                       horizontalScrollBar.Value - textRegion.Right +
                                                       (int)(0.2f * textRegion.Width) + caretPixel.X);
            }
            Point pos = GetPixelAtTextpoint(caret.GetPosition(true));
            if (parentForm.suggestionBox.Visible)
                pos.X += parentForm.suggestionBox.Width;
            ime.SetIMEWindowLocation(pos.X, pos.Y);
        }



        //fix bug when user has re-posed the Caret, it has been put to the pos below it.
        private TextPoint GetTextpointAtPixel(int x, int y)
        {
            Rectangle textRegion = TextRegion;
            TextPoint point = new TextPoint();
            //we need update VerticalScrollBar.Value
            int VBValue=verticalScrollBar.Value>0?verticalScrollBar.Value:0;
            point.Line = Math.Max(0, (y - textRegion.Y) / fonts.Base.Height + verticalScrollBar.Value);
            int invisibleLineMark = point.Line;
            int visibleLines = point.Line - VBValue;
            int c = 0;
            for (int i = 0; i <= point.Line && i < lines.Count; i++)
            {
                if (!lines[i].LineVisible)
                    point.Line++;
            }
            if (point.Line >= lines.Count)
                point.Line = lines.Count - 1;
            point.Pos = (x - textRegion.X + horizontalScrollBar.Value);
            //p.Y = (point.Line - verticalScrollBar.Value) * fonts.Base.Height + textRegion.Y;
            int pos = 0;
            for (pos = 0; pos < lines[point.Line].Text.Length; pos++)
            {
                if (point.Pos < 0)
                    break;
                int subtract = fonts.CharWidth;
                if (lines[point.Line].Text[pos] > 0xFF)
                    subtract += fonts.CharWidth;
                else if (lines[point.Line].Text[pos] == '\t')
                    subtract += 3 * fonts.CharWidth;
                point.Pos -= subtract;
                if (point.Pos < 0)
                {
                    if (point.Pos > -subtract / 2)
                        pos++;
                    break;
                }
            }
            point.Pos = pos;
           /* if (point.Pos % fonts.CharWidth >= fonts.CharWidth / 2)
                point.Pos += fonts.CharWidth;
            point.Pos /= fonts.CharWidth;*/
            point.Pos = Math.Max(0, Math.Min(point.Pos, lines[point.Line].Text.Length));
            
            return point;
        }

        public Point GetPixelAtTextpoint(TextPoint point, bool upper = true)
        {
            Rectangle textRegion = TextRegion;

            int hiddenLines = 0;
            for (int i = 0; i < point.Line && i < lines.Count; i++)
            {
                if (!lines[i].LineVisible)
                    hiddenLines++;
            }
            point.Line -= hiddenLines;

            Point p = new Point();
            p.Y = (point.Line - verticalScrollBar.Value)*fonts.Base.Height + textRegion.Y;
            p.X = (textRegion.X + 2 - horizontalScrollBar.Value);
            for (int i = 0; i < point.Pos; i++)
            {
                p.X += fonts.CharWidth;
                if (point.Line < lines.Count && point.Line >= 0 && i < lines[point.Line].Text.Length)
                {
                    if (lines[point.Line].Text[i] > 0xFF)
                        p.X += fonts.CharWidth;
                    else if (lines[point.Line].Text[i] == '\t')
                        p.X += 3*fonts.CharWidth;
                }
            }
            //p.X = (textRegion.X + 2 - horizontalScrollBar.Value + fonts.CharWidth * point.Pos);
            //p.X = fonts.GetWidth(lines[point.Line].Text.Substring(0, point.Pos));
            if (!upper)
                p.Y += fonts.Base.Height;
            return p;
        }

        private bool IsTextpointMarked(TextPoint point)
        {

            TextPoint min = TextPoint.Min(caret.GetPosition(true), mouseDownPos);
            TextPoint max = TextPoint.Max(caret.GetPosition(true), mouseDownPos);
            return TextMarked && point >= min && point < max;
        }

        public string GetLine(int line)
        {
            return lines[line].Text;
        }

        public string GetTextWithin(TextPoint begin, TextPoint end)
        {
            if (end.Line >= lines.Count)
                end.Line = lines.Count - 1;
            if (begin.Line == end.Line)
                return lines[begin.Line].Text.Substring(begin.Pos, end.Pos - begin.Pos);
            StringBuilder text = new StringBuilder(lines[begin.Line].Text.Substring(begin.Pos) + "\n");

            for (int i = begin.Line + 1; i < end.Line; i++)
            {
                text.Append(lines[i].Text + "\n");
            }
            if (end.Pos < lines[end.Line].Text.Length)
                text.Append(lines[end.Line].Text.Remove(end.Pos));
            else
                text.Append(lines[end.Line].Text);
            return text.ToString();
        }

        public void UndoRemove(TextPoint from, TextPoint to)
        {
            bool invalidateFollowingLines = from.Line != to.Line;
            lines[from.Line].Invalidated = true;
            lines[from.Line].edited = true;
            if (from.Line == to.Line)
                //Cut text between min and max
                lines[from.Line].Text = lines[from.Line].Text.Remove(from.Pos, to.Pos - from.Pos);
            else
                //Cut end of min line, and replace with end of max line
                lines[from.Line].Text = lines[from.Line].Text.Substring(0, from.Pos) + lines[to.Line].Text.Remove(0, to.Pos);
            //Cut lines between min and max
            while (from.Line < to.Line)
            {
                lines.RemoveAt(from.Line + 1);
                to.Line--;
            }

            if (invalidateFollowingLines)
            {//We removed a line - invalidate all following
                for (int i = from.Line; i < lines.Count; i++)
                {
                    lines[i].Invalidated = true;
                }
            }

            caret.SetPosition(new TextPoint(from.Line, from.Pos), true);
            //Dont know if it is shrunk or not, so call both :)
            ExtendedLines(new List<Line>() { lines[from.Line] });
            ShrunkLine(lines[from.Line]);
            UpdateBlocks();
            TextEdited();
            CaretMoved();
            Invalidate();
            Form1.Form.suggestionBox.Hide();
        }

        public void UndoInsert(TextPoint pos, string text)
        {
            string[] texts = text.Split('\n');
            int cLine = pos.Line;
            string endText = lines[pos.Line].Text.Substring(pos.Pos);
            if (pos.Pos < lines[pos.Line].Text.Length)
                lines[pos.Line].Text = lines[pos.Line].Text.Remove(pos.Pos);
            lines[pos.Line].Text += texts[0];
            lines[pos.Line].Invalidated = true;
            lines[pos.Line].edited = true;
            List<Line> editedLines = new List<Line>(){lines[pos.Line]};
            for (int i = 1; i < texts.Length; i++)
            {
                Line newLine = new Line(texts[i]);
                cLine++;
                lines.Insert(cLine, newLine);
                lines[cLine].Invalidated = true;
                lines[cLine].edited = true;
                editedLines.Add(newLine);
            }

            if (texts.Length > 1)
            {//We added a line - invalidate all following
                for (int i = cLine; i < lines.Count; i++)
                {
                    lines[i].Invalidated = true;
                }
            }

            caret.SetPosition(new TextPoint(cLine, lines[cLine].Text.Length), true);
            lines[cLine].Text += endText;
            ExtendedLines(editedLines);
            ShrunkLine(lines[pos.Line]);
            UpdateBlocks();
            TextEdited();
            CaretMoved();
            Invalidate();
            Form1.Form.suggestionBox.Hide();
        }



        public void InsertAndStyle(TextPoint pos, string text)
        {
            UndoInsert(pos, text);
            int lineBreaks = text.Count(c => c == '\n');
            TextPoint endPos = new TextPoint(pos.Line + lineBreaks, 0);

            int lastLineLength = text.Remove(0, text.LastIndexOf('\n') + 1).Length;

            for (int i = 0; i <= lineBreaks; i++)
            {
                int oldWhiteSpaceCount = 0;
                if (i == lineBreaks)
                {
                    oldWhiteSpaceCount = lines[pos.Line + i].Text.Count(c => c == ' ' || c == '\t');
                }

                lines[pos.Line + i].Indents = lines[pos.Line + i].GetWantedIndents(fonts, lines, pos.Line + i);

                if (i == lineBreaks)
                {
                    endPos.Pos = lastLineLength - oldWhiteSpaceCount +
                                 lines[pos.Line + i].Text.Count(c => c == ' ' || c == '\t');
                }
            }

            UndoSys.TextAdded(GetTextWithin(pos, endPos), this, pos);
        }

        public void InsertAtCaret(string text)
        {
            TextPoint caretPos = caret.GetPosition(true);
            UndoSys.TextAdded(text, this, caretPos);

            //Insert text
            lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos, text);
            lines[caretPos.Line].Invalidated = true;

            caret.SetPosition(new TextPoint(caretPos.Line, caretPos.Pos + text.Length), true);
            //Dont know if it is shrunk or not, so call both :)
            ExtendedLines(new List<Line>() { lines[caretPos.Line] });
            UpdateBlocks();
            TextEdited();
            CaretMoved();
            Invalidate();
        }

        public void ReplaceTextAt(TextPoint from, TextPoint to, string text)
        {
            string removedText = "";
            for (int line = from.Line; line <= to.Line; line++)
            {
                if (line == from.Line)
                {
                    if (line == to.Line)
                        removedText += lines[line].Text.Remove(0, from.Pos).Substring(0, to.Pos - from.Pos);
                    else
                        removedText += lines[line].Text.Remove(0, to.Pos) + "\n";
                }
                else
                {
                    if (line == to.Line)
                        removedText += lines[line].Text.Substring(0, to.Pos);
                    else
                        removedText += lines[line].Text + "\n";
                }
            }
            UndoSys.TextReplaced(removedText, text, this, new TextPoint(from.Line, from.Pos));

            lines[from.Line].Invalidated = true;
            lines[from.Line].edited = true;
            if (from.Line == to.Line)
                //Cut text between min and max
                lines[from.Line].Text = lines[from.Line].Text.Remove(from.Pos, to.Pos - from.Pos);
            else
                //Cut end of min line, and replace with end of max line
                lines[from.Line].Text = lines[from.Line].Text.Substring(0, from.Pos) + lines[to.Line].Text.Remove(0, to.Pos);
            //Cut lines between min and max
            while (from.Line < to.Line)
            {
                lines.RemoveAt(from.Line + 1);
                to.Line--;
            }

            //Insert text
            lines[from.Line].Text = lines[from.Line].Text.Insert(from.Pos, text);

            caret.SetPosition(new TextPoint(from.Line, from.Pos + text.Length), true);
            //Dont know if it is shrunk or not, so call both :)
            ExtendedLines(new List<Line>() { lines[from.Line] });
            ShrunkLine(lines[from.Line]);
            UpdateBlocks();
            TextEdited();
            CaretMoved();
            Invalidate();
        }

        public void MoveCaretTo(TextPoint pos)
        {
            if (pos.Line >= lines.Count)
                pos.Line = lines.Count - 1;
            lines[caret.GetPosition(true).Line].Invalidated = true;
            caret.SetPosition(pos, true);
            EnsureLineVisible(pos.Line);
            CaretMoved();
            Invalidate();
        }

        public void ReplaceMarkedText(string text)
        {
            DeleteMarkedText();
            TextPoint caretPos = caret.Position;
            lines[caretPos.Line].Text = lines[caretPos.Line].Text.Insert(caretPos.Pos, text);
            lines[caretPos.Line].Invalidated = true;
            ExtendedLines(new List<Line>() { lines[caretPos.Line] });
            Invalidate();
        }

        public void Mark(TextPoint from, TextPoint to)
        {
            if (TextMarked)
            {
                TextPoint min = TextPoint.Min(mouseDownPos, caret.GetPosition(true));
                TextPoint max = TextPoint.Max(mouseDownPos, caret.GetPosition(true));
                for (int i = min.Line; i <= max.Line; i++)
                {
                    lines[i].Invalidated = true;
                }
            }

            mouseDownPos = from;
            caret.SetPosition(to, true);
            for (int i = from.Line; i <= to.Line; i++)
            {
                lines[i].Invalidated = true;
            }
            TextMarked = true;
            EnsureLineVisible(to.Line);
            CaretMoved(false);
            Invalidate();
        }

        /*
         * problems:
set cursor to line (need to add/subtract invisible lines, when someone gets/sets cursor line)
what happens if the cursor is set to an invisible line?
what happens if the cursor is above an invisible line, and key down is pressed?
what happens if the cursor is at pos 0 under an invisible line, and backspace is pressed?
error messages will give a position in absolute line numbers.
when to update blocks?
    when {, } is inserted.
    when text is deleted (marked text, delete and backspace)
    when text is pasted  
*/
        private void UpdateBlocks()
        {
            List<TextPoint> openBrackets = new List<TextPoint>();
            List<Line> hiddenBlocks = new List<Line>();
            List<Line> changedToVisible = new List<Line>();
            List<Line> changedToHidden = new List<Line>();

            TextPoint point = new TextPoint(0, -1);
            int lastLine = -1;
            while (NextTextPoint(ref point))
            {
                
                for (int i = lastLine + 1; i <= point.Line; i++)
                {
                    if (!lines[i].BlockVisible)
                    {
                        hiddenBlocks.Add(lines[i]);
                        lines[i].BlockVisible = true;
                    }

                    if (!lines[i].LineVisible)
                        changedToVisible.Add(lines[i]);

                    lines[i].BlockEndLine = null;
                    lines[i].LineVisible = true;

                }
                lastLine = point.Line;

                char currentChar = lines[point.Line].Text[point.Pos];
                if (currentChar == '{')
                    openBrackets.Add(new TextPoint(point.Line, point.Pos));
                else if (currentChar == '}')
                {
                    //Find start point
                    if (openBrackets.Count > 0)
                    {
                        TextPoint openPoint = openBrackets[openBrackets.Count - 1];
                        openBrackets.RemoveAt(openBrackets.Count - 1);
                        //If there is another open bracket on the same line, this bracket can not be hidden.
                        if (openBrackets.Count == 0 || openBrackets[openBrackets.Count - 1].Line < openPoint.Line)
                        {
                            //Only add it if collapsing it causes some lines to be invis
                            if (openPoint.Line + 1< point.Line)
                            {
                                lines[openPoint.Line].BlockEndLine = lines[point.Line];
                                if (hiddenBlocks.Contains(lines[openPoint.Line]))
                                {//Hide lines between open and closed block
                                    lines[openPoint.Line].BlockVisible = false;
                                    for (int line = openPoint.Line + 1; line < point.Line; line++)
                                    {
                                        if (changedToVisible.Contains(lines[line]))
                                            changedToVisible.Remove(lines[line]);
                                        else
                                            changedToHidden.Add(lines[line]);

                                        lines[line].LineVisible = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = lastLine + 1; i < point.Line; i++)
            {
                lines[i].BlockEndLine = null;
                lines[i].LineVisible = true;
                lines[i].BlockVisible = true;
            }

            ExtendedLines(changedToVisible);
            foreach (Line line in changedToHidden)
            {
                ShrunkLine(line);
            }

            if (changedToHidden.Count > 0 || changedToVisible.Count > 0)
            {
                InvalidateAll = true;
                Invalidate();
            }
        }

        private bool NextTextPoint(ref TextPoint point)
        {
            point.Pos++;
            if (point.Pos < lines[point.Line].Text.Length)
                return true;
            point.Line++;
            point.Pos = -1;
            if (point.Line < lines.Count)
                return NextTextPoint(ref point);
            return false;
        }

       

        private void EnsureLineVisible(int line)
        {
            if (!lines[line].LineVisible)
            {
                //Go up to find a closed block
                for (int l = line - 1; l >= 0; l--)
                {
                    if (lines[l].BlockEndLine != null && lines[l].BlockVisible == false)
                    {
                        lines[l].BlockVisible = true;
                        UpdateBlocks();
                        EnsureLineVisible(line);
                        return;
                    }
                }
            }
        }

        public void GoToDeclaration()
        {
            TextPoint caretPos = caret.GetPosition(true);
            List<Token> tokens = new List<Token>();
            Lexer lexer = new Lexer(new StringReader(lines[caretPos.Line].Text));
            Token token;
            while (!((token = lexer.Next()) is EOF))
            {
                if (token is TWhiteSpace || token is TTraditionalComment || token is TEndOfLineComment || token is TDocumentationComment)
                    continue;
                    tokens.Add(token);
            }
            int nameIndex = -1;
            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];
                if (t is TIdentifier &&
                    t.Pos - 1 <= caretPos.Pos && 
                    t.Pos + t.Text.Length - 1 >= caretPos.Pos)
                {
                    token = t;
                    nameIndex = i;
                    break;
                }
            }
            if (nameIndex == -1)
            {
                MessageBox.Show(Form1.Form, "The caret must be over an identifier.", "Error");
                return;
            }
            bool isMethod = nameIndex + 1 < tokens.Count && tokens[nameIndex + 1] is TLParen;
            string name = tokens[nameIndex].Text;
            List<SuggestionBoxItem> matchingDecls = new List<SuggestionBoxItem>();
            if (nameIndex > 0 && (tokens[nameIndex - 1] is TDot || tokens[nameIndex - 1] is TArrow))
            {
                //Dotted
                StringBuilder text = new StringBuilder();
                for (int line = 0; line < caretPos.Line; line++)
                {
                    text.AppendLine(lines[line].Text);
                }
                text.Append(lines[caretPos.Line].Text.Substring(0, token.Pos - 1));
                List<StructDescription> targetStructs;
                List<EnrichmentDescription> targetEnrichments;
                List<NamespaceDescription> targetNamespaces;
                List<MethodDescription> delegateTargets;
                bool b;
                List<StructDescription> staticStructs;

                Form1.Form.suggestionBox.Listbox.ExtractTargetType(text.ToString(), out targetStructs, out targetEnrichments, out targetNamespaces,
                                    out b, out staticStructs, out b, out delegateTargets, out b);
                if (isMethod)
                {
                    foreach (StructDescription str in targetStructs)
                    {
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Name == name && !method.IsStatic && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    }
                    foreach (EnrichmentDescription enrichment in targetEnrichments)
                    {
                        foreach (MethodDescription method in enrichment.Methods)
                        {
                            if (method.Name == name && !method.IsStatic && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    }
                    foreach (NamespaceDescription ns in targetNamespaces)
                    {
                        foreach (MethodDescription method in ns.Methods)
                        {
                            if (method.Name == name && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    }
                    foreach (StructDescription str in staticStructs)
                    {
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Name == name && method.IsStatic && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    }
                }
                else
                {
                    //Not methods
                    foreach (StructDescription str in targetStructs)
                    {
                        foreach (VariableDescription var in str.Fields)
                        {
                            if (var.Name == name && !var.IsStatic)
                                matchingDecls.Add(var);
                        }
                    }
                    foreach (EnrichmentDescription str in targetEnrichments)
                    {
                        foreach (VariableDescription var in str.Fields)
                        {
                            if (var.Name == name && !var.IsStatic)
                                matchingDecls.Add(var);
                        }
                    }
                    foreach (NamespaceDescription ns in targetNamespaces)
                    {
                        foreach (VariableDescription var in ns.Fields)
                        {
                            if (var.Name == name)
                                matchingDecls.Add(var);
                        }
                        foreach (NamespaceDescription ns2 in ns.Namespaces)
                        {
                            if (ns2.Name == name)
                                matchingDecls.Add(ns2);
                        }
                        foreach (StructDescription str in ns.Structs)
                        {
                            if (str.Name == name)
                                matchingDecls.Add(str);
                        }
                        foreach (TypedefDescription typedef in ns.Typedefs)
                            if (typedef.Name == name)
                                matchingDecls.Add(typedef);
                    }
                    foreach (StructDescription str in staticStructs)
                    {
                        foreach (VariableDescription var in str.Fields)
                        {
                            if (var.Name == name && var.IsStatic)
                                matchingDecls.Add(var);
                        }
                    }
                }

            }
            else
            {
                //Simple
                object openFileData = Tag is Form1.OpenFileData
                                          ? ((Form1.OpenFileData)Tag).File
                                          : (object)((DialogData)Tag).DialogItem;
                SourceFileContents sourceFile = null;
                foreach (SourceFileContents sourceFileContents in Form1.Form.compiler.ParsedSourceFiles)
                {
                    if (openFileData == sourceFileContents.Item && !sourceFileContents.IsDialogDesigner)
                    {
                        sourceFile = sourceFileContents;
                        break;
                    }
                }
                if (sourceFile == null)
                    return;
                IDeclContainer context = sourceFile.GetDeclContainerAt(caretPos.Line);
                StructDescription currentStruct = null;
                EnrichmentDescription currentEnrichment = null;
                MethodDescription currentMethod = null;
                foreach (StructDescription str in context.Structs)
                {
                    if (str.LineFrom <= caretPos.Line && str.LineTo >= caretPos.Line)
                    {
                        currentStruct = str;
                        foreach (MethodDescription method in str.Methods)
                        {
                            if (method.Start <= caretPos && method.End >= caretPos)
                            {
                                currentMethod = method;
                                break;
                            }
                        }
                        foreach (MethodDescription method in str.Constructors)
                        {
                            if (method.Start <= caretPos && method.End >= caretPos)
                            {
                                currentMethod = method;
                                break;
                            }
                        }
                        foreach (MethodDescription method in str.Deconstructors)
                        {
                            if (method.Start <= caretPos && method.End >= caretPos)
                            {
                                currentMethod = method;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (currentStruct == null)
                    foreach (EnrichmentDescription str in context.Enrichments)
                    {
                        if (str.LineFrom <= caretPos.Line && str.LineTo >= caretPos.Line)
                        {
                            currentEnrichment = str;
                            foreach (MethodDescription method in str.Methods)
                            {
                                if (method.Start <= caretPos && method.End >= caretPos)
                                {
                                    currentMethod = method;
                                    break;
                                }
                            }
                            foreach (MethodDescription method in str.Constructors)
                            {
                                if (method.Start <= caretPos && method.End >= caretPos)
                                {
                                    currentMethod = method;
                                    break;
                                }
                            }
                            foreach (MethodDescription method in str.Deconstructors)
                            {
                                if (method.Start <= caretPos && method.End >= caretPos)
                                {
                                    currentMethod = method;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                if (currentMethod == null)
                    foreach (MethodDescription method in context.Methods)
                    {
                        if (method.Start <= caretPos && method.End >= caretPos)
                        {
                            currentMethod = method;
                            break;
                        }
                    }
                List<IDeclContainer> visibleDecls = sourceFile.GetVisibleDecls(context.NamespaceList, true);
                if (isMethod)
                {
                    if (currentStruct != null)
                        foreach (MethodDescription method in currentStruct.Methods)
                        {
                            if (method.Name == name && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    if (currentEnrichment != null)
                        foreach (MethodDescription method in currentEnrichment.Methods)
                        {
                            if (method.Name == name && !method.IsDelegate)
                                matchingDecls.Add(method);
                        }
                    if (matchingDecls.Count == 0)
                        foreach (IDeclContainer visibleDecl in visibleDecls)
                        {
                            foreach (MethodDescription method in visibleDecl.Methods)
                            {
                                if (method.Name == name && !method.IsDelegate)
                                    matchingDecls.Add(method);
                            }
                        }
                }
                else
                {
                    //Not method
                    if (currentMethod != null)
                    {
                        foreach (VariableDescription var in currentMethod.Locals)
                        {
                            if (var.Name == name)
                                matchingDecls.Add(var);
                        }
                        foreach (VariableDescription var in currentMethod.Formals)
                        {
                            if (var.Name == name)
                                matchingDecls.Add(var);
                        }
                    }
                    if (matchingDecls.Count == 0 && currentStruct != null)
                        foreach (VariableDescription var in currentStruct.Fields)
                        {
                            if (var.Name == name)
                                matchingDecls.Add(var);
                        }
                    if (matchingDecls.Count == 0 && currentEnrichment != null)
                        foreach (VariableDescription var in currentEnrichment.Fields)
                        {
                            if (var.Name == name)
                                matchingDecls.Add(var);
                        }
                    if (matchingDecls.Count == 0)
                        foreach (IDeclContainer visibleDecl in visibleDecls)
                        {
                            foreach (VariableDescription var in visibleDecl.Fields)
                            {
                                if (var.Name == name)
                                    matchingDecls.Add(var);
                            }
                            foreach (NamespaceDescription ns in visibleDecl.Namespaces)
                            {
                                if (ns.Name == name)
                                    matchingDecls.Add(ns);
                            }
                            foreach (TypedefDescription typedef in visibleDecl.Typedefs)
                                if (typedef.Name == name)
                                    matchingDecls.Add(typedef);
                        }
                }
            }

            if (matchingDecls.Count == 0)
            {
                MessageBox.Show(Form1.Form, "Found no matching declarations.", "Error");
                return;
            }

            SuggestionBoxItem item = matchingDecls[0];

            SourceFileContents file = item.ParentFile.File;

            Form1.Form.OpenFile((FileItem) file.Item);
            caretPos = item.Position;
            caretPos.Pos--;
            Form1.Form.CurrentOpenFile.OpenFile.Editor.MoveCaretTo(caretPos);




        }

    }
}
