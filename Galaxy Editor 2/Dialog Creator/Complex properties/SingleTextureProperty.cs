using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    [Serializable]
    class SingleTextureProperty
    {
        [NonSerialized]
        private GraphicsControl context;

        public SingleTextureProperty(GraphicsControl context)
        {
            this.context = context;
        }

        public SingleTextureProperty(string path)
        {
            Path = path;
        }

        public SingleTextureProperty(string path, GraphicsControl context)
        {
            this.context = context;
            
            Texture = TextureLoader.Load(path, context.GraphicsDevice);
            Path = Texture != null ? path : "";
        }

        public SingleTextureProperty(string path, Texture2D texture, GraphicsControl context)
        {
            this.context = context;
            Path = path;
            Texture = texture;
        }

        public void ContextChanged(GraphicsControl context)
        {
            this.context = context;
            if (Path != null)
            {
                Texture = TextureLoader.Load(Path, context.GraphicsDevice);
                Path = Texture != null ? Path : "";
            }
        }

        public string Path { get; set; }
        [NonSerialized] 
        public Texture2D Texture;
    }
}
