using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class ProgressBar : DialogControl
    {
        public ProgressBar(GraphicsControl context, Dialog parent, DialogData data) : base(context, parent, "progressBar", data)
        {
            Images[(int) Race.Terran] =
                Images[(int) Race.Protoss] =
                Images[(int) Race.Zerg] =
                new SingleTextureProperty(@"Assets\Textures\progress-queue.dds");
            TintColors[(int)Race.Terran] = new Color(40, 125, 75);
            TintColors[(int)Race.Protoss] = new Color(35, 125, 254);
            TintColors[(int)Race.Zerg] = new Color(229, 95, 5);
            ImageType = ImageType.Normal;
            MaxValue = 1f;
            IsHalfTexture = true;
        }

        public override void ConsistensyCheck()
        {
            IsHalfTexture = true;
            base.ConsistensyCheck();
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeProgressBar);");

            PrintBaseInit(builder, new ProgressBar(Context, null, Data));
        }

        protected override DialogControl defaultControl
        {
            get { return new ProgressBar(Context, Parent, Data); }
        }


        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                List<AbstractControl> returner = new List<AbstractControl>();
                if (MaxValue != MinValue)
                {
                    float val = (Value - MinValue)/(MaxValue - MinValue);
                    if (val < 0)
                        val = 0;
                    else if (val > 1)
                        val = 1;
                    if (val > 0)
                    {
                        Dialog dialog = new Dialog(Context, new Rectangle(ABSPosition.X, ABSPosition.Y, (int)(Size.Width * val), Size.Height), Data);
                        ImageControl image = new ImageControl(Context, dialog, Data);
                        image.Position = new Point(0, 0);
                        image.Size = Size;
                        image.Image = Image;
                        image.ImageType = ImageType;
                        image.TintColor = TintColor;
                        image.IsHalfTexture = true;
                        image.IsBottomHalf = true;
                        returner.Add(image);
                    }
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
            get { return "ProgressBar"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }
    }
}
