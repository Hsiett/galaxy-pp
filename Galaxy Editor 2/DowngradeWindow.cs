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
    public partial class DowngradeWindow : AutoSizeForm
    {
        public DowngradeWindow(List<string> versions)
        {
            InitializeComponent();

            foreach (string version in versions)
            {
                listBox1.Items.Add("Version " + version);
            }
            listBox1.SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get { return listBox1.SelectedIndex; }
        }
    }
}
