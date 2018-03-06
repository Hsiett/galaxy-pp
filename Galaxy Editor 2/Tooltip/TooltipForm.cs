using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2.Tooltip
{
    public partial class TooltipForm : Form
    {
        public TooltipForm()
        {
            InitializeComponent();
        }

        public void Show(Point pos, Size size)
        {
            ShowInactiveTopmost(pos, size);
        }

        private delegate void SetVisibleDelegate(bool b);
        public void SetVisible(bool b)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetVisibleDelegate(SetVisible), b);
                return;
            }
            Visible = b;
        }

        private delegate void SetPositionDelegate(Point p);
        public void SetPosition(Point p)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetPositionDelegate(SetPosition), p);
                return;
            }
            Location = p;
        }

        public List<MyToolboxControl.Item> Items
        {
            get { return myToolboxControl1.Items; }
        }

        public void Redraw()
        {

            myToolboxControl1.Invalidate();
        }

        public MyToolboxControl TooltipControl { get { return myToolboxControl1; } }

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

        private delegate void ShowInactiveTopmostDelegate(Point position, Size size);
        private void ShowInactiveTopmost(Point position, Size size)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ShowInactiveTopmostDelegate(ShowInactiveTopmost), position, size);
                return;
            }
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            /*SetWindowPos(Handle.ToInt32(), HWND_TOPMOST,
            position.X, position.Y, size.Width, size.Height,
            SWP_NOACTIVATE);*/
            this.Location = position;
            this.Size = size;

        }

        

    }
}
