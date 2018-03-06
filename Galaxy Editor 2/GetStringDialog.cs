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
    public partial class GetStringDialog : AutoSizeForm
    {
        public GetStringDialog(string title, string headding, string initText)
        {
            InitializeComponent();
            Text = title;
            label1.Text = headding;
            textBox1.Text = initText;
        }

        public string GetString()
        {
            return textBox1.Text;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return) button1.PerformClick();
        }
    }
}
