using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Point = Microsoft.Xna.Framework.Point;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class EditBoxControl : DialogControl
    {
        public EditBoxControl(GraphicsControl context, Dialog parent, DialogData data)
            : base(context, parent, "editBox", data)
        {
            Images[(int)Race.Terran] =
                new SingleTextureProperty("Assets\\Textures\\ui_frame_big_innerline_terran.dds");
            Images[(int)Race.Protoss] =
                new SingleTextureProperty("Assets\\Textures\\ui_frame_big_innerline_protoss.dds");
            Images[(int)Race.Zerg] =
                new SingleTextureProperty("Assets\\Textures\\ui_frame_big_innerline_zerg.dds");
            ImageType = ImageType.HorizontalBorder;
            Text = "";
            TextStyles[(int)Race.Terran] =
                TextStyles[(int)Race.Protoss] =
                TextStyles[(int)Race.Zerg] = FontParser.Fonts["StandardEditBox"];
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeEditBox);");

            PrintBaseInit(builder, new EditBoxControl(Context, null, Data));
        }

        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                var returner = new List<AbstractControl>();
                ImageControl image = new ImageControl(Context, Parent, Data);
                Point p = Position;
                p.Y += 4;
                p.X += 4;
                image.Position = p;
                image.Size = new Size(Size.Width - 8, Size.Height - 8);
                image.ImageType = ImageType;
                image.Image = Image;
                image.TintColor = TintColor;
                returner.Add(image);
                Label label = new Label(Context, Parent, Data);
                p.X += 2;
                p.Y += 2;
                label.Position = p;
                label.Size = new Size(Size.Width - 12, Size.Height - 12); ;
                label.Text = EditText;
                label.TextStyle = TextStyle;
                returner.Add(label);
                return returner;
            }
        }

        protected override DialogControl defaultControl
        {
            get { return new EditBoxControl(Context, Parent, Data) { Text = "" }; }
        }

        public override bool DrawTexture
        {
            get { return false; }
        }

        protected override string TypeString
        {
            get { return "EditBox"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }

    }
}
