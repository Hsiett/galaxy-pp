using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
namespace Galaxy_Editor_2.Dialog_Creator.Fonts
{
    [Serializable]
    class FontData : IComparable<FontData>
    {
        public FontData()
        {
            //Default data
            Anchor = Anchor.TopLeft;
            TextColor = new Color(1f, 1f, 1f, 1f);
            Size = 14;
        }

        public string Name { get; set; }
        public string FontRef { get; set; }
        public int Size { get; set; }
        public Anchor Anchor { get; set; }
        //Font flags never used
        public StyleFlags StyleFlags { get; set; }
        public Color TextColor { get; set; }
        public Color DisabledColor { get; set; }
        public Color HighLightColor { get; set; }
        public Color HotKeyColor { get; set; }
        public Color HyperlinkColor { get; set; }
        //public Color ShadowColor { get; set; }
        public float ShadowOffset { get; set; }

        public int CompareTo(FontData other)
        {
            return Name.CompareTo(other.Name);
        }

        public FontData GetClone()
        {
            return new FontData()
                       {
                           Name = Name,
                           FontRef = FontRef,
                           Anchor = Anchor,
                           DisabledColor = DisabledColor,
                           HighLightColor = HighLightColor,
                           HotKeyColor = HotKeyColor,
                           HyperlinkColor = HyperlinkColor,
                           ShadowOffset = ShadowOffset,
                           Size = Size
                       };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
