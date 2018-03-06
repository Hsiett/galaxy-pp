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
    public partial class UploadToMapForm : AutoSizeForm
    {
        public UploadToMapForm()
        {
            InitializeComponent();
        }

        public bool Extract;
        public bool Inject;

        private void BTNExtract_Click(object sender, EventArgs e)
        {
            Extract = true;
            Close();
        }

        private void BTNInject_Click(object sender, EventArgs e)
        {
            Inject = true;
            Close();
        }

        private void BTNNeither_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
