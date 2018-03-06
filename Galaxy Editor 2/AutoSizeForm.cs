using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public class AutoSizeForm : Form
    {
        protected AutoSizeForm()
        {
            Load += AutoSizeForm_Load;
        }


        protected void AutoSizeForm_Load(object sender, EventArgs e)
        {
            int minX, maxX, minY, maxY;
            minX = minY = int.MaxValue;
            maxX = maxY = 0;

            foreach (Control control in Controls)
            {
                if (!control.Visible)
                    continue;

                minX = Math.Min(minX, control.Left);
                minY = Math.Min(minY, control.Top);
                maxX = Math.Max(maxX, control.Right);
                maxY = Math.Max(maxY, control.Bottom);
            }

            ClientSize = new Size(minX + maxX, minY + maxY);// = minX + maxX;
        }
    }
}
