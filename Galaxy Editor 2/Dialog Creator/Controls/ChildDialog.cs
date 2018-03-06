using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class ChildDialog : Dialog
    {
        public ChildDialog(GraphicsControl sender, Rectangle rect, Dialog parent, DialogData data)
            : base(sender, rect, data)
        {
            ParentDialog = parent;
            //Parent = parent;
            ParentAttachPoint = Anchor.TopRight;
        }

        public virtual void SetParent(Dialog dialog)
        {
            ParentDialog = Parent = dialog;
        }

        public override void ParentSizeChanged(Size oldSize, Size newSize)
        {
            Point p = Position;
            if (ParentAttachPoint.IsMiddleX())
            {
                p.X += (Size.Width - oldSize.Width) / 2 - (Size.Width - newSize.Width) / 2;
            }
            else if (ParentAttachPoint.IsRight())
            {
                p.X += newSize.Width - oldSize.Width;
            }
            if (ParentAttachPoint.IsMiddleY())
            {
                p.Y += (Size.Height - oldSize.Height) / 2 - (Size.Height - newSize.Height) / 2;
            }
            else if (ParentAttachPoint.IsBottom())
            {
                p.Y += newSize.Height - oldSize.Height;
            }
            Position = p;
            LastParentSize = newSize;
        }

        public override Point Offset
        {
            get
            {
                Point p = Position;
                if (ParentAttachPoint.IsMiddleX())
                {
                    p.X -= GetParentSize().Width/2;
                }
                else if (ParentAttachPoint.IsRight())
                {
                    p.X -= GetParentSize().Width;
                }
                if (ParentAttachPoint.IsMiddleY())
                {
                    p.Y -= GetParentSize().Height / 2;
                }
                else if (ParentAttachPoint.IsBottom())
                {
                    p.Y -= GetParentSize().Height;
                }
                if (Anchor.IsMiddleX())
                {
                    p.X += Size.Width / 2;
                }
                else if (Anchor.IsRight())
                {
                    p.X += Size.Width;
                }
                if (Anchor.IsMiddleY())
                {
                    p.Y += Size.Height / 2;
                }
                else if (Anchor.IsBottom())
                {
                    p.Y += Size.Height;
                }
                return p;
            }
            set
            {
                Point p = value;
                if (ParentAttachPoint.IsMiddleX())
                {
                    p.X += GetParentSize().Width / 2;
                }
                else if (ParentAttachPoint.IsRight())
                {
                    p.X += GetParentSize().Width;
                }
                if (ParentAttachPoint.IsMiddleY())
                {
                    p.Y += GetParentSize().Height / 2;
                }
                else if (ParentAttachPoint.IsBottom())
                {
                    p.Y += GetParentSize().Height;
                }
                if (Anchor.IsMiddleX())
                {
                    p.X -= Size.Width / 2;
                }
                else if (Anchor.IsRight())
                {
                    p.X -= Size.Width;
                }
                if (Anchor.IsMiddleY())
                {
                    p.Y -= Size.Height / 2;
                }
                else if (Anchor.IsBottom())
                {
                    p.Y -= Size.Height;
                }
                Position = p;
            }
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            base.PrintInitialization(builder);
            builder.Append("\t\tDialogSetPositionRelative(");
            builder.Append(Name);
            builder.Append(", ");
            builder.Append(Anchor.ToSCIIString());
            builder.Append(", ");
            builder.Append(ParentDialog.Name);
            builder.Append(", ");
            builder.Append(ParentAttachPoint.ToSCIIString());
            builder.Append(", ");
            builder.Append(Offset.X);
            builder.Append(", ");
            builder.Append(Offset.Y);
            builder.AppendLine(");");
        }
        

        [DescriptionAttribute("When attached to another dialog, it will move around when the parent moves."),
        Category("Child Dialog Settings"),
        DefaultValue(""),
        TypeConverter(typeof(Complex_properties.ParentDialogUITypeConverter))]
        public Dialog ParentDialog { get { return Parent; } set { Parent = value; } }


        [DescriptionAttribute("The point on the parent on which to attach."), 
        Category("Child Dialog Settings"), 
        DefaultValue(Anchor.TopRight)]
        public Anchor ParentAttachPoint { get; set; }

    }
}
