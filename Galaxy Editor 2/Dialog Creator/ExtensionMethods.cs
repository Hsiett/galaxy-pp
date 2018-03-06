using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
namespace Galaxy_Editor_2.Dialog_Creator
{
    static class ExtensionMethods
    {
        public static Color ToXNAColor(this System.Drawing.Color cl)
        {
            return new Color(cl.R, cl.G, cl.B, cl.A);
        }

        public static string ToSCIIString(this Anchor a)
        {
            return "c_anchor" + Enum.GetName(typeof (Anchor), a);
        }

        public static string ToSCIIString(this ImageType a)
        {
            return "c_triggerImageType" + Enum.GetName(typeof(ImageType), a);
        }

        public static string ToSCIIString(this BlendMode a)
        {
            return "c_triggerBlendMode" + Enum.GetName(typeof(BlendMode), a);
        }
    }
}
