using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Editor_control;
using Galaxy_Editor_2.Tooltip;

namespace Galaxy_Editor_2.Suggestion_box
{
    partial class SuggestionBoxForm : Form
    {
        public SuggestionBoxForm(GalaxyCompiler compiler, Form1 form1)
        {
            form1.Move += form1_Move;
            InitializeComponent();
            myListbox1.ParentForm = this;
            myListbox1.SetCompiler(compiler);
            form1.Deactivate += form1_Deactivate;
            Activated += SuggestionBoxForm_Activated;
            //UpdateScrollBar();
        }

        public MyListbox Listbox { get { return myListbox1; } }

        private bool activated;
        private void SuggestionBoxForm_Activated(object sender, EventArgs e)
        {
            activated = true;
        }


        private void form1_Deactivate(object sender, EventArgs e)
        {
            activated = false;
            new Thread(Deactivate).Start();
        }

        private delegate void NoParams();
        private void Deactivate()
        {
            try
            {
                Thread.Sleep(100);
                if (activated) return;
                if (IsHandleCreated)
                    Invoke(new NoParams(Hide));
                if (myListbox1.methodParamTooltip.IsHandleCreated)
                    myListbox1.methodParamTooltip.Invoke(new NoParams(myListbox1.methodParamTooltip.Hide));
            }
            catch (Exception err)
            {
                Program.ErrorHandeler(this, new ThreadExceptionEventArgs(err));
            }
        }

        public bool AllowHide = true;
        public new void Hide()
        {
            if (!Visible || !AllowHide)
                return;

            base.Hide();
        }

        public void SetCurrentEditor(MyEditor editor)
        {
            myListbox1.CurrentEditor = editor;
        }

        private void myListbox1_SizeChanged(object sender, EventArgs e)
        {
            //UpdateScrollBar();
        }

        public void Scroll(int ammount)
        {
            if (vScrollBar1.Visible)
            {
                int value = (int) ((500*ammount)/120 + vScrollBar1.Value);
                value = Math.Max(0, Math.Min(value, vScrollBar1.Maximum));
                vScrollBar1.Value = value;
                vScrollBar1_Scroll(null, null);
            }
        }

        private delegate void UpdateAndShowDelegate(Point position, Size size);
        public void UpdateAndShow(Point position, ref Size size)
        {

            /*if (myListbox1.tooltip.InvokeRequired)
            {
                myListbox1.tooltip.Invoke(new UpdateAndShowDelegate(UpdateAndShow), position, size);
                return;
            }*/
            size.Width += Padding.Left + Padding.Right;
            size.Height += Padding.Top + Padding.Bottom;
            UpdateScrollBar(ref size);
            if (myListbox1.tooltip.Visible)
            {
                myListbox1.tooltip.Show(new Point(position.X + size.Width, position.Y), myListbox1.tooltip.Size);
            }
            ShowInactiveTopmost(position, size);
        }

        public int VScrollValue
        {
            get { return vScrollBar1.Value/100; }
            set
            {
                value *= 100;
                if (value >= 0 && value <= vScrollBar1.Maximum)
                    SetVScroll(value);
                //Invalidate(true);
            }
        }

        private delegate void SetVScrollDelegate(int value);
        private void SetVScroll(int value)
        {
            if (vScrollBar1.InvokeRequired)
            {
                vScrollBar1.Invoke(new SetVScrollDelegate(SetVScroll), value);
                return;
            }
            vScrollBar1.Value = value;
        }


        private void UpdateScrollBar(ref Size size)
        {
            vScrollBar1.Maximum = Math.Max(0, myListbox1.LineCount - size.Height/myListbox1.Font.Height);
            vScrollBar1.Visible = vScrollBar1.Maximum > 0;
            //if (vScrollBar1.Visible)
            {
                vScrollBar1.Maximum += 10;
                size.Width += vScrollBar1.Width;
            }
            vScrollBar1.Maximum *= 100;
            vScrollBar1_Scroll(null, null);
        }

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,           // window handle
             int hWndInsertAfter,    // placement-order handle
             int X,          // horizontal position
             int Y,          // vertical position
             int cx,         // width
             int cy,         // height
             uint uFlags);       // window positioning flags

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void ShowInactiveTopmost(Point position, Size size)
        {
            Owner = Form1.Form;
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(Handle.ToInt32(), 0/*HWND_TOPMOST*/,
            position.X, position.Y, size.Width, size.Height,
            SWP_NOACTIVATE);
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (myListbox1.LineOffset != vScrollBar1.Value / 100)
            {
                myListbox1.LineOffset = vScrollBar1.Value/100;
                myListbox1.Invalidate();
            }
        }

        public bool MouseOnScrollBar;
        private void vScrollBar1_MouseLeave(object sender, EventArgs e)
        {
            myListbox1.CurrentEditor.Focus();
            MouseOnScrollBar = false;
        }

        private void vScrollBar1_MouseEnter(object sender, EventArgs e)
        {
            MouseOnScrollBar = true;
        }

        private void form1_Move(object sender, EventArgs e)
        {
            myListbox1.Reposition();
        }

        private void SuggestionBoxForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
                myListbox1.tooltip.SetVisible(false);
        }

        public new void Dispose()
        {
            myListbox1.RebuildVisibleList();
            base.Dispose();
        }
        
    }
}
