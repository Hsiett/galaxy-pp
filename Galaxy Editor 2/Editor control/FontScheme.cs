using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Editor_control
{
    [Serializable]
    public struct FontModification
    {
        public FontStyle Style;
        public Color Color;

        public FontModification(FontStyle style, Color color)
        {
            Style = style;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FontModification)) return false;
            FontModification other = (FontModification)obj;
            return Style == other.Style && Color == other.Color;
        }

        /*public static FontModification Default { get { return new FontModification(0, Color.Black); } }
        public static FontModification Comment { get { return new FontModification(0, Color.FromArgb(0, 128, 0)); } }
        public static FontModification String { get { return new FontModification(0, Color.FromArgb(193, 21, 110)); } }
        public static FontModification Structs { get { return new FontModification(0, Color.FromArgb(43, 145, 175)); } }*/
    }

    class FontScheme
    {
        public Font Base {get { return Options.Editor.Font; }} //new Font(new FontFamily("Consolas"), 10);
        public int CharWidth { get { return Options.Editor.CharWidth; } }

        public Dictionary<Options.FontStyles, List<string>> Modifications = new Dictionary<Options.FontStyles, List<string>>();
    }
}
