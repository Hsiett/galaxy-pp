using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    interface IRenderableItem
    {
        Rectangle DrawRect { get; }
        Texture2D Texture { get; }
        ImageType ImageType { get; }
        int RenderPriority { get; }
        void Move(int x, int y);
        void Resize(int w, int h, string resizeAt);
    }
}
