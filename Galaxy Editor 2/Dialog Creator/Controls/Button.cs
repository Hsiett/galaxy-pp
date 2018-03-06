using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Microsoft.Xna.Framework.Graphics;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class Button : DialogControl
    {
        public Button(GraphicsControl context, Dialog parent, DialogData data)
            : base(context, parent, "button", data)
        {
            ImageType = ImageType.EndCap;
            //Text = Name;
            TextStyles[(int)Race.Terran] =
                TextStyles[(int)Race.Protoss] =
                TextStyles[(int)Race.Zerg] = FontParser.Fonts["StandardButton"];
            Images[(int)Race.Terran] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normalpressed_terran.dds");
            Images[(int)Race.Protoss] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normalpressed_protoss.dds");
            Images[(int)Race.Zerg] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normalpressed_zerg.dds");
            HoverImages[(int)Race.Terran] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normaloverpressedover_terran.dds");
            HoverImages[(int)Race.Protoss] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normaloverpressedover_protoss.dds");
            HoverImages[(int)Race.Zerg] =
                new SingleTextureProperty("Assets\\Textures\\ui_button_generic_normaloverpressedover_zerg.dds");
            
            ImageType = ImageType.Border;
            Text = Name;
            IsHalfTexture = true;
        }


        

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeButton);");

            PrintBaseInit(builder, new Button(Context, null, Data){Text = ""});
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "Button"; }
        }

        public override bool DrawText
        {
            get { return true; }
        }

        protected override DialogControl defaultControl
        {
            get { return new Button(Context, Parent, Data) {Text = ""}; }
        }

        public override void ConsistensyCheck()
        {
            IsHalfTexture = true;
            base.ConsistensyCheck();
        }
    }
}
