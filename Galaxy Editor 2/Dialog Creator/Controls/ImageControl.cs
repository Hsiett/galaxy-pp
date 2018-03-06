using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlendState = Galaxy_Editor_2.Dialog_Creator.Enums.BlendState;
namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class ImageControl : DialogControl
    {
        public ImageControl(GraphicsControl context, Dialog parent, DialogData data)
            : base(context, parent, "image", data)
        {
            ImageType = ImageType.Normal;
            Images[0] = Images[1] = Images[2] = new SingleTextureProperty("Assets\\Textures\\white32.dds");
        }

        protected override DialogControl defaultControl
        {
            get { return new ImageControl(Context, Parent, Data); }
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeImage);");

            PrintBaseInit(builder, new ImageControl(Context, null, Data));
        }

        public override BlendState BlendState
        {
               
            get { return BlendState.BlendStates[BlendMode]; }
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "Image"; }
        }


        public override bool DrawText
        {
            get { return false; }
        }

        
    }
}
