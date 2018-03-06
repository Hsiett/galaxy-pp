using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class CheckBox : DialogControl
    {
        public CheckBox(GraphicsControl context, Dialog parent, DialogData data) : base(context, parent, "checkBox", data)
        {
            Images[(int)Race.Terran] =
                new SingleTextureProperty("Assets\\Textures\\ui_glue_checkbox_normalpressed_terran.dds");
            Images[(int)Race.Protoss] =
                new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_checkbox_normalpressed.dds");
            Images[(int)Race.Zerg] =
                new SingleTextureProperty("Assets\\Textures\\ui_glue_checkbox_normalpressed_zerg.dds");
            HoverImages[(int)Race.Terran] =
                new SingleTextureProperty("Assets\\Textures\\ui_glue_checkbox_normaloverpressedover_terran.dds");
            HoverImages[(int)Race.Protoss] =
                new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_checkbox_normaloverpressedover.dds");
            HoverImages[(int)Race.Zerg] =
                new SingleTextureProperty("Assets\\Textures\\ui_glue_checkbox_normaloverpressedover_zerg.dds");
            ImageType = ImageType.Normal;
            IsHalfTexture = true;
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "CheckBox"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }

        public override Size DefaultSize
        {
            get { return new Size(44, 44); }
        }

        public override void ConsistensyCheck()
        {
            IsHalfTexture = true;
            base.ConsistensyCheck();
        }

        public override Rectangle DrawRect
        {
            get
            {
                Rectangle rect = base.DrawRect;
                rect.X += rect.Width / 2 - 44 / 2;
                rect.Y += rect.Height / 2 - 44 / 2;
                rect.Height = rect.Width = 44;
                return rect;
            }
        }

        protected override DialogControl defaultControl
        {
            get { return new CheckBox(Context, Parent, Data); }
        }

        public override Rectangle SelectRect
        {
            get { return base.DrawRect; }
        }


        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeCheckBox);");

            PrintBaseInit(builder, new CheckBox(Context, null, Data));
        }

        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                if (Checked)
                {
                    ImageControl image = new ImageControl(Context, Parent, Data);
                    Point p = Position;
                    p.Y += Size.Height / 2 - 44 / 2;
                    p.X += Size.Width / 2 - 44 / 2;
                    p.X += 10;
                    p.Y += 10;
                    image.Position = p;
                    image.Size = new Size(24, 24);
                    image.ImageType = ImageType.Normal;
                    image.Image = new SingleTextureProperty(Context.DisplayRace == Race.Terran
                                                                ? "Assets\\Textures\\ui_glue_checkboxmark_terran.dds"
                                                                : Context.DisplayRace == Race.Protoss
                                                                      ? "Assets\\Textures\\ui_battlenet_glue_checkboxmark.dds"
                                                                      : "Assets\\Textures\\ui_glue_checkboxmark_zerg.dds",
                                                            Context);
                    return new List<AbstractControl>(){image};
                }
                return new List<AbstractControl>();
            }
        }
    }
}
