using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Aga.Controls;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Point = Microsoft.Xna.Framework.Point;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class ListBox : DialogControl
    {
        public ListBox(GraphicsControl context, Dialog parent, DialogData data) : base(context, parent, "listBox", data)
        {
            Images[(int)Race.Terran] = new SingleTextureProperty("Assets\\Textures\\ui_glue_listboxframe_terran.dds");
            Images[(int)Race.Protoss] = new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_listboxframe.dds");
            Images[(int)Race.Zerg] = new SingleTextureProperty("Assets\\Textures\\ui_glue_listboxframe_zerg.dds");
            ImageType = ImageType.Border;
            TextStyles[(int) Race.Terran] =
                TextStyles[(int) Race.Protoss] =
                TextStyles[(int) Race.Zerg] = FontParser.Fonts["StandardListBox"];
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeListBox);");
            PrintBaseInit(builder, new ListBox(Context, null, Data) { Text = "" });

            
        }

        protected override DialogControl defaultControl
        {
            get { return new ListBox(Context, Parent, Data); }
        }

        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                List<AbstractControl> returner = new List<AbstractControl>();
                int x = Position.X + 4;
                int y = Position.Y + 2;
                if (Items != null)
                    foreach (string item in Items)
                    {
                        Label label = new Label(Context, Parent, Data);
                        label.Text = item;
                        label.TextStyle = TextStyle;
                        label.Position = new Point(x, y);
                        label.Size = new Size(Size.Width, 21);
                        y += 21;
                        if (y + 21 > Position.X + Size.Height)
                            break;
                        returner.Add(label);
                    }
                return returner;
            }
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "ListBox"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }
    }
}
