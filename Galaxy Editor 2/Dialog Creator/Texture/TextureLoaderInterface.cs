using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Dialog_Creator.Texture
{
    public interface TextureLoaderInterface
    {
        Texture2D Load(string path, GraphicsDevice device);
        List<string> GetAllPaths();
        List<string> GetPossiblePath(string shortPath);
        void Unload(string path);
    }
}
