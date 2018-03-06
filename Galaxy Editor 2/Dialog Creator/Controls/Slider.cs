using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class Slider : DialogControl
    {
        public Slider(GraphicsControl context, Dialog parent, DialogData data) : base(context, parent, "slider", data)
        {
            Images[(int)Race.Terran] = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderframe_terran.dds");
            Images[(int)Race.Protoss] = new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_sliderframe.dds");
            Images[(int)Race.Zerg] = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderframe_zerg.dds");
            ImageType = ImageType.Border;
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeSlider);");

            PrintBaseInit(builder, new Slider(Context, null, Data));
        }

        public override Rectangle DrawRect
        {
            get
            {
                Rectangle rect = base.DrawRect;
                rect.Y += (rect.Height - 25) / 2;
                rect.Height = 25;
                return rect;
            }
        }

        public override Rectangle SelectRect
        {
            get
            {
                return base.DrawRect;
            }
        }

        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                List<AbstractControl> returner = new List<AbstractControl>();
                //Fill
                float val = 0;
                if (MaxValue != MinValue)
                    val = (Value - MinValue) / (MaxValue - MinValue);
                if (val < 0)
                    val = 0;
                else if (val > 1)
                    val = 1;
                if (val > 0)
                {
                    Dialog dialog = new Dialog(Context, new Rectangle(ABSPosition.X, ABSPosition.Y + (Size.Height - 25)/2, (int)(Size.Width * val), 25), Data);
                    ImageControl image = new ImageControl(Context, dialog, Data);
                    image.Position = new Point(0, 0);
                    image.Size = new Size(Size.Width, 25);
                    switch (Context.DisplayRace)
                    {
                        case Race.Terran:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderfill_terran.dds", Context);
                            break;
                        case Race.Protoss:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_sliderfill.dds", Context);
                            break;
                        case Race.Zerg:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderfill_zerg.dds", Context);
                            break;
                    }
                    image.ImageType = ImageType.Border;
                    image.TintColor = TintColor;
                    returner.Add(image);
                }
                {//Current pos image
                    ImageControl image = new ImageControl(Context, Parent, Data);
                    image.Size = new Size(36, 48);
                    image.Position = new Point((int)(Position.X + Size.Width * val - image.Size.Width / 2), Position.Y + (Size.Height - image.Size.Height) / 2);
                    switch (Context.DisplayRace)
                    {
                        case Race.Terran:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderhandle_normalpressed_terran.dds", Context);
                            break;
                        case Race.Protoss:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_sliderhandle_normalpressed.dds", Context);
                            break;
                        case Race.Zerg:
                            image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_sliderhandle_normalpressed_zerg.dds", Context);
                            break;
                    }
                    image.ImageType = ImageType.Normal;
                    image.IsHalfTexture = true;
                    image.TintColor = TintColor;
                    returner.Add(image);
                }
                return returner;
            }
        }

        protected override DialogControl defaultControl
        {
            get { return new Slider(Context, Parent, Data); }
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "Slider"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }
    }
}
