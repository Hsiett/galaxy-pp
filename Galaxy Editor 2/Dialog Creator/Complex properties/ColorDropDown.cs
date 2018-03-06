using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    public partial class ColorDropDown : UserControl
    {
        private Color color = System.Drawing.Color.Black;
        public ColorDropDown()
        {
            InitializeComponent();
        }

        public Microsoft.Xna.Framework.Color Color
        {
            get { return color.ToXNAColor(); }
            set
            {
                color = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                BTNColor.BackColor = color;
                NUDAlpha.Value = (decimal) (100f*value.A/255f);
            }
        }

        private void BTNColor_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.Color = color;
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;
            color = System.Drawing.Color.FromArgb(color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            BTNColor.BackColor = color;
        }

        private void NUDAlpha_ValueChanged(object sender, EventArgs e)
        {
            color = System.Drawing.Color.FromArgb((int)(NUDAlpha.Value * 255 / 100), color.R, color.G, color.B);
            BTNColor.BackColor = color;
        }
    }
}
