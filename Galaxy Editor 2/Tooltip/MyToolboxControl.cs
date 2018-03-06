using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2.Tooltip
{
    public partial class MyToolboxControl : UserControl
    {
        public class Item
        {
            public class Section
            {
                public string Text;
                public FontStyle Style;

                public Section(string text, FontStyle style = FontStyle.Regular)
                {
                    Text = text;
                    Style = style;
                }
            }

            public List<Section> Sections = new List<Section>();
        }

        

        public List<Item> Items = new List<Item>();

        public MyToolboxControl()
        {
            InitializeComponent();

            /*Items.Add(new Item());
            Items[0].Sections.Add(new Item.Section("regular "));
            Items[0].Sections.Add(new Item.Section("bold ", FontStyle.Bold));
            Items[0].Sections.Add(new Item.Section("regular"));

            Items.Add(new Item());
            for (int i = 0; i < "this is some crazy shit".Length; i++)
            {
                Items[1].Sections.Add(new Item.Section("this is some crazy shit".Substring(i, 1),
                                                       (i & 1) == 0 ? FontStyle.Regular : FontStyle.Bold));
            }*/

           /* SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Opaque |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw, true);*/
        }

        private const int CharWidth = 5;
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(255, 255, 224));

            int y = 0;
            int width = 0;
            int height = 0;
            /*foreach (Item item in Items)
            {
                if (y != 0)
                {
                    y += 2;
                    //Draw line
                    Point from = new Point(0, y);
                    Point to = new Point(Width, y);
                    Pen dashedPen = new Pen(Color.Black, 1);
                    dashedPen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLine(dashedPen, from, to);
                    y += 2;
                }


                int x = 0;
                foreach (Item.Section section in item.Sections)
                {
                    e.Graphics.DrawString(section.Text, new Font(Font, section.Style), Brushes.Black, x, y);

                    SizeF size = e.Graphics.MeasureString(section.Text, new Font(Font, section.Style));
                    Bitmap bitmap = new Bitmap((int)size.Width + 10, (int)size.Height + 1);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.Clear(Color.White);
                    g.DrawString(section.Text + "|", new Font(Font, section.Style), Brushes.Black, 0, 0);
                    g.Flush();
                    for (int nx = bitmap.Width - 1; nx >= 0; nx--)
                    {
                        Color cl = Color.White;
                        for (int ny = 0; ny < bitmap.Height; ny++)
                        {
                            cl = bitmap.GetPixel(nx, ny);
                            if (cl.R != 255)
                            {
                                x += nx - 4;
                                break;
                            }
                        }
                        if (cl.R != 255)
                        {
                            break;
                        }
                    }
                    width = Math.Max(width, x);
                }
                y += Font.Height;
            }*/

            foreach (List<List<Item.Section>> Item in Lines)
            {
                if (y != 0)
                {
                    y += 2;
                    //Draw line
                    Point from = new Point(0, y);
                    Point to = new Point(Width, y);
                    Pen dashedPen = new Pen(Color.Black, 1);
                    dashedPen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLine(dashedPen, from, to);
                    y += 2;
                }

                foreach (List<Item.Section> Line in Item)
                {
                    int x = 0;
                    foreach (Item.Section section in Line)
                    {
                        e.Graphics.DrawString(section.Text, new Font(Font, section.Style), Brushes.Black, x, y);

                        SizeF size = e.Graphics.MeasureString(section.Text, new Font(Font, section.Style));
                        Bitmap bitmap = new Bitmap((int)size.Width + 10, (int)size.Height + 1);
                        Graphics g = Graphics.FromImage(bitmap);
                        g.Clear(Color.White);
                        g.DrawString(section.Text + "|", new Font(Font, section.Style), Brushes.Black, 0, 0);
                        g.Flush();
                        for (int nx = bitmap.Width - 1; nx >= 0; nx--)
                        {
                            Color cl = Color.White;
                            for (int ny = 0; ny < bitmap.Height; ny++)
                            {
                                cl = bitmap.GetPixel(nx, ny);
                                if (cl.R != 255)
                                {
                                    x += nx - 4;
                                    break;
                                }
                            }
                            if (cl.R != 255)
                            {
                                break;
                            }
                        }
                        width = Math.Max(width, x);
                    }
                    y += Font.Height;
                }
            }

            height = y;
            width += 5;
            if (Parent.Width != width + 2 || Parent.Height != height + 2)
                Parent.Size = new Size(width + 2, height + 2);
            
            base.OnPaint(e);
        }

        List<List<Item.Section>>[] Lines;

        public Size GetRequiredSize(int avalibleWidth)
        {
            Lines = new List<List<Item.Section>>[Items.Count];
            int y = 0;
            int width = 0;
            int height = 0;
            int itemNr = -1;
            foreach (Item item in Items)
            {
                itemNr++;
                if (y != 0)
                {
                    y += 4;
                }


                Lines[itemNr] = new List<List<Item.Section>>();
                Lines[itemNr].Add(new List<Item.Section>());
                Lines[itemNr][0].AddRange(item.Sections);

                for (int lineNr = 0; lineNr < Lines[itemNr].Count; lineNr++)
                {
                    int x = 7;
                    for (int sectionNr = 0; sectionNr < Lines[itemNr][lineNr].Count; sectionNr++)
                    {
                        Item.Section section = Lines[itemNr][lineNr][sectionNr];

                        SizeF size = CreateGraphics().MeasureString(section.Text, new Font(Font, section.Style));
                        Bitmap bitmap = new Bitmap((int)size.Width + 10, (int)size.Height + 1);
                        Graphics g = Graphics.FromImage(bitmap);
                        g.Clear(Color.White);
                        g.DrawString(section.Text + "|", new Font(Font, section.Style), Brushes.Black, 0, 0);
                        g.Flush();
                        for (int nx = bitmap.Width - 1; nx >= 0; nx--)
                        {
                            Color cl = Color.White;
                            for (int ny = 0; ny < bitmap.Height; ny++)
                            {
                                cl = bitmap.GetPixel(nx, ny);
                                if (cl.R != 255)
                                {
                                    x += nx - 4;
                                    break;
                                }
                            }
                            if (cl.R != 255)
                            {
                                break;
                            }
                        }
                        if (x > avalibleWidth)
                        {
                            //Look for a space in current section (i,j), split that section into 2, and move the right part to next line, along with
                            //all following sections on current line. Then restart parsing this line.
                            //If no spaces could be found, look in previous section.
                            //If no spaces could be found at all, let it be wider than the screen.
                            bool restartLine = false;
                            int oldSectionNumber = sectionNr;
                            while (true)
                            {
                                int index = Lines[itemNr][lineNr][sectionNr].Text.LastIndexOf(' ');
                                if (index >= 0)
                                {
                                    Item.Section left =
                                        new Item.Section(Lines[itemNr][lineNr][sectionNr].Text.Substring(0, index),
                                                         Lines[itemNr][lineNr][sectionNr].Style);
                                    Item.Section right =
                                        new Item.Section(Lines[itemNr][lineNr][sectionNr].Text.Substring(index + 1),
                                                         Lines[itemNr][lineNr][sectionNr].Style);
                                    Lines[itemNr][lineNr][sectionNr] = left;
                                    if (Lines[itemNr].Count == lineNr + 1)
                                    {
                                        Lines[itemNr].Add(new List<Item.Section>());
                                        for (int i = Lines[itemNr][lineNr].Count - 1; i > sectionNr; i--)
                                        {
                                            Lines[itemNr][lineNr + 1].Add(Lines[itemNr][lineNr][i]);
                                            Lines[itemNr][lineNr].RemoveAt(i);
                                        }
                                        Lines[itemNr][lineNr + 1].Insert(0, right);
                                    }
                                    else
                                    {
                                        //move this section down
                                        //If the section down there is using same formatting, join em.
                                        if (Lines[itemNr][lineNr + 1][0].Style == right.Style)
                                            Lines[itemNr][lineNr + 1][0].Text = right.Text + " " + Lines[itemNr][lineNr + 1][0].Text;
                                        else
                                        {
                                            right.Text += " ";
                                            Lines[itemNr][lineNr + 1].Insert(0, right);
                                        }
                                    }

                                    //Restart on current line
                                    restartLine = true;
                                    break;
                                }
                                else
                                {
                                    //Look in previous section
                                    if (sectionNr == 0)
                                    {
                                        sectionNr = oldSectionNumber;
                                        break;
                                    }
                                    sectionNr--;
                                }
                            }
                            if (restartLine)
                            {
                                sectionNr = -1;
                                x = 7;
                                continue;
                            }
                        }
                        width = Math.Max(width, x);
                    }
                    y += Font.Height;
                    
                }
            }
            height = y;
            //width += 5;
            return new Size(width, height + 2);
        }
    }
}
