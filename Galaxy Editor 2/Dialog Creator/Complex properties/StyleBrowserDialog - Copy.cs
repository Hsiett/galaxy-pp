using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Label = Galaxy_Editor_2.Dialog_Creator.Controls.Label;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    partial class StyleBrowserDialog : Form
    {
        public FontData SelectedFont { get { return (FontData) LBList.SelectedItem; } }

        private Label label;
        public StyleBrowserDialog(FontData currentFont)
        {
            InitializeComponent();

            foreach (KeyValuePair<string, FontData> pair in FontParser.Fonts)
            {
                LBList.Items.Add(pair.Value);
            }

            LBList.SelectedItem = currentFont;
            graphicsControl1.DisableMouseControl = true;
            graphicsControl1.SetDialogData(new DialogData());
        }

        private void StyleBrowserDialog_Load(object sender, EventArgs e)
        {
            Dialog mainDialog = new Dialog(graphicsControl1, new Rectangle(0, 0, 200, 200), null);
            mainDialog.Fullscreen = true;
            mainDialog.ImageType = ImageType.None;
            graphicsControl1.AddDialog(mainDialog);

            label = new Label(graphicsControl1, mainDialog, null);
            label.Text = TBTestText.Text;
            label.TextStyle = SelectedFont;
            label.Size = new Size(graphicsControl1.DrawWidth, graphicsControl1.DrawHeight);
            label.Offset = new Point(0, 0);
            label.RenderPriority = 513;
            mainDialog.AddControl(label);
            graphicsControl1.Invalidate();
        }

        private void LBList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (label == null)
                return;
            label.TextStyle = SelectedFont;
            graphicsControl1.Invalidate();
        }

        private void TBTestText_TextChanged(object sender, EventArgs e)
        {
            label.Text = TBTestText.Text;
            graphicsControl1.Invalidate();
        }

        private void graphicsControl1_SizeChanged(object sender, EventArgs e)
        {
            label.Size = new Size(graphicsControl1.DrawWidth, graphicsControl1.DrawHeight);
        }


    }
}
