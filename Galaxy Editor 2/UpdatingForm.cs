using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public partial class UpdatingForm : AutoSizeForm
    {
        private long div = 1;
        private long val;
        public UpdatingForm(long max)
        {
            InitializeComponent();

           

            if (max > int.MaxValue)
            {
                div = max/int.MaxValue + 1;
            }
            progressBar1.Maximum = (int) (max / div);
        }

        public delegate void AddValueDelegate(long v);
        public void AddValue(long v)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke(new AddValueDelegate(AddValue), v);
                return;
            }
            val += v;
            progressBar1.Value = (int)(val / div);
        }

        public delegate void CloseDelegate();
        public new void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new CloseDelegate(Close));
                return;
            }
            base.Close();
        }

    }
}
