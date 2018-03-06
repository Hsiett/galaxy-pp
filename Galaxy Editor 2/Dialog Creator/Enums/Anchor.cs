using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Dialog_Creator.Enums
{
    enum Anchor
    {
        TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight

    
    }
    static class AnchorMethods
    {
        public static bool IsTop(this Anchor a)
        {
            return a == Anchor.Top || a == Anchor.TopLeft || a == Anchor.TopRight;
        }
        public static bool IsMiddleY(this Anchor a)
        {
            return a == Anchor.Left || a == Anchor.Center || a == Anchor.Right;
        }
        public static bool IsBottom(this Anchor a)
        {
            return a == Anchor.BottomLeft || a == Anchor.Bottom || a == Anchor.BottomRight;
        }
        public static bool IsLeft(this Anchor a)
        {
            return a == Anchor.Left || a == Anchor.TopLeft || a == Anchor.BottomLeft;
        }
        public static bool IsMiddleX(this Anchor a)
        {
            return a == Anchor.Top || a == Anchor.Center || a == Anchor.Bottom;
        }
        public static bool IsRight(this Anchor a)
        {
            return a == Anchor.TopRight || a == Anchor.Right || a == Anchor.BottomRight;
        }
    }
}
