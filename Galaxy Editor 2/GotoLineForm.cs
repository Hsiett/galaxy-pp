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
    public partial class GotoLineForm : AutoSizeForm
    {
        private Point position;
        private int lineCount;
        public int SelectedLine { get { return int.Parse(textBox1.Text); } }

        public GotoLineForm(int currentLine, int lineCount)
        {
            this.lineCount = lineCount;
            InitializeComponent();
            label1.Text = "Line number (1 - " + lineCount + ")";
            textBox1.SelectedText = currentLine.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool valid = true;
            try
            {
                int value = int.Parse(textBox1.Text);
                if (value < 1 || value > lineCount)
                    valid = false;
            }
            catch (Exception)
            {
                valid = false;
            }
            BTNOkay.Enabled = valid;
            if (valid)
                toolTip1.RemoveAll();
            else
                toolTip1.SetToolTip(textBox1, "Value must be an integer in the range [1," + lineCount + "]");
        }

        public DialogResult ShowDialog(Form parent)
        {
            position = new Point(parent.Location.X + (parent.Width - Width) / 2,
                parent.Location.Y + (parent.Height - Height) / 2);
            return base.ShowDialog(parent);
        }

        private void GotoLineForm_Load(object sender, EventArgs e)
        {
            Location = position;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return && BTNOkay.Enabled)
            {
                BTNOkay.PerformClick();
                e.Handled = true;
            }
        }
    }
}
