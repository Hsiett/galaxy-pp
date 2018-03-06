using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using BlendState = Galaxy_Editor_2.Dialog_Creator.Enums.BlendState;
namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    abstract class AbstractControl : IRenderableItem
    {
        [BrowsableAttribute(false)]
        public abstract Size DefaultSize{ get;}
        [NonSerialized]
        protected internal GraphicsControl Context;
        [NonSerialized]
        protected internal DialogData Data;
        private Dialog parent;
        protected internal virtual Dialog Parent
        {
            get { return parent; }
            set
            {
                if (parent == null)
                {
                    if (Context != null)
                        Context.ParentSizeChangedEvent -= ParentSizeChanged;
                }
                else
                    parent.ParentSizeChangedEvent -= ParentSizeChanged;
                parent = value;
                if (parent == null)
                {
                    if (Context != null)
                        Context.ParentSizeChangedEvent += ParentSizeChanged;
                }
                else
                    parent.ParentSizeChangedEvent += ParentSizeChanged;
            }
        }

        public virtual void SetParent(Dialog dialog)
        {
            Parent = dialog;
        }



        protected AbstractControl(GraphicsControl context, Dialog parent, string baseName, DialogData data)
        {
            this.parent = parent;
            Data = data;
            Context = context;
            if (parent == null)
            {
                if (Context != null)
                    Context.ParentSizeChangedEvent += ParentSizeChanged;
            }
            else
                parent.ParentSizeChangedEvent += ParentSizeChanged;
            //Find unique name
            int nr = 1;
            Name = baseName + nr;

            if (Context == null)
                return;

            for (int i = 0; i < Context.Items.Count; i++)
            {
                if (context.Items[i].Name == Name)
                {
                    nr++;
                    Name = baseName + nr;
                    i = -1;
                    continue;
                }
                else
                {
                    for (int j = 0; j < Context.Items[i].ChildControls.Count; j++)
                    {
                        if (Context.Items[i].ChildControls[j].Name == Name)
                        {
                            nr++;
                            Name = baseName + nr;
                            i = -1;
                            break;
                        }
                    }
                }
            }
        }

        [BrowsableAttribute(false)]
        public abstract string VariableDeclaration { get; }
        public abstract void PrintInitialization(StringBuilder builder);

        public virtual void ContextChanged(GraphicsControl context)
        {
            Context = context;

            if (parent == null)
            {
                if (Context != null)
                    Context.ParentSizeChangedEvent += ParentSizeChanged;
            }
            else
            {
                parent.ParentSizeChangedEvent -= ParentSizeChanged;
                parent.ParentSizeChangedEvent += ParentSizeChanged;
            }

            if (LastParentSize.Width != -1)
            {
                ParentSizeChanged(LastParentSize, GetParentSize());
            }
            else
                LastParentSize = GetParentSize();
            //Reload textures
        }

        protected Size LastParentSize = new Size(-1, -1);
        protected Size GetParentSize()
        {
            if (Parent == null)
            {
                if (Context != null)
                    return new Size((int)(Context.DrawWidth), (int)(Context.DrawHeight));
                return LastParentSize;
            }
            return Parent.Size;
        }

        private Point GetParentPosition()
        {
            if (Parent == null)
                return new Point(0, 0);
            return Parent.ABSPosition;
        }
        [BrowsableAttribute(false)]
        public virtual List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                return new List<AbstractControl>();
            }
        }

        protected static Point ToAbsTopLeft(Point offset, Size size, Anchor destPoint, Anchor sourcePoint, Point parentPos, Size parentSize)
        {
            //Move the offset so sourcePoint = destPoint
            if (sourcePoint.IsLeft())
            {
                if (destPoint.IsMiddleX())
                {
                    offset.X -= parentSize.Width/2;
                }
                else if (destPoint.IsRight())
                {
                    offset.X -= parentSize.Width;
                }
            }
            else if (sourcePoint.IsMiddleX())
            {
                if (destPoint.IsLeft())
                {
                    offset.X += parentSize.Width / 2;
                }
                else if (destPoint.IsRight())
                {
                    offset.X -= parentSize.Width/2;
                }
            }
            else //source point is right
            {
                if (destPoint.IsLeft())
                {
                    offset.X += parentSize.Width;
                }
                else if (destPoint.IsMiddleX())
                {
                    offset.X += parentSize.Width / 2;
                }
            }
            //For Y
            if (sourcePoint.IsTop())
            {
                if (destPoint.IsMiddleY())
                {
                    offset.Y -= parentSize.Height/2;
                }
                else if (destPoint.IsBottom())
                {
                    offset.Y -= parentSize.Height;
                }
            }
            else if (sourcePoint.IsMiddleY())
            {
                if (destPoint.IsTop())
                {
                    offset.Y += parentSize.Height / 2;
                }
                else if (destPoint.IsBottom())
                {
                    offset.Y -= parentSize.Height / 2;
                }
            }
            else //source point is bottom
            {
                if (destPoint.IsTop())
                {
                    offset.Y += parentSize.Height;
                }
                else if (destPoint.IsMiddleY())
                {
                    offset.Y += parentSize.Height / 2;
                }
            }
           

            if (destPoint.IsRight())
            {
                offset.X = parentSize.Width + offset.X - size.Width;
            }
            else if (destPoint.IsMiddleX())
            {
                offset.X = parentSize.Width / 2 + offset.X - size.Width / 2;
            }
            if (destPoint.IsBottom())
            {
                offset.Y = parentSize.Height + offset.Y - size.Height;
            }
            else if (destPoint.IsMiddleY())
            {
                offset.Y = parentSize.Height / 2 + offset.Y - size.Height / 2;
            }
            offset.X += parentPos.X;
            offset.Y += parentPos.Y;
            return offset;
        }

        protected static Point FromAbsTopLeft(Point offset, Size size, Anchor destPoint, Anchor sourcePoint, Point parentPos, Size parentSize)
        {
            offset.X -= parentPos.X;
            offset.Y -= parentPos.Y;

            if (destPoint.IsRight())
            {
                offset.X = -parentSize.Width + offset.X + size.Width;
            }
            else if (destPoint.IsMiddleX())
            {
                offset.X = -parentSize.Width / 2 + offset.X + size.Width / 2;
            }
            if (destPoint.IsBottom())
            {
                offset.Y = -parentSize.Height + offset.Y + size.Height;
            }
            else if (destPoint.IsMiddleY())
            {
                offset.Y = -parentSize.Height / 2 + offset.Y + size.Height / 2;
            }


            //Move the offset so sourcePoint = destPoint
            if (sourcePoint.IsLeft())
            {
                if (destPoint.IsMiddleX())
                {
                    offset.X += parentSize.Width / 2;
                }
                else if (destPoint.IsRight())
                {
                    offset.X += parentSize.Width;
                }
            }
            else if (sourcePoint.IsMiddleX())
            {
                if (destPoint.IsLeft())
                {
                    offset.X -= parentSize.Width / 2;
                }
                else if (destPoint.IsRight())
                {
                    offset.X += parentSize.Width / 2;
                }
            }
            else //source point is right
            {
                if (destPoint.IsLeft())
                {
                    offset.X -= parentSize.Width;
                }
                else if (destPoint.IsMiddleX())
                {
                    offset.X -= parentSize.Width / 2;
                }
            }
            //For Y
            if (sourcePoint.IsTop())
            {
                if (destPoint.IsMiddleY())
                {
                    offset.Y += parentSize.Height / 2;
                }
                else if (destPoint.IsBottom())
                {
                    offset.Y += parentSize.Height;
                }
            }
            else if (sourcePoint.IsMiddleY())
            {
                if (destPoint.IsTop())
                {
                    offset.Y -= parentSize.Height / 2;
                }
                else if (destPoint.IsBottom())
                {
                    offset.Y += parentSize.Height / 2;
                }
            }
            else //source point is bottom
            {
                if (destPoint.IsTop())
                {
                    offset.Y -= parentSize.Height;
                }
                else if (destPoint.IsMiddleY())
                {
                    offset.Y -= parentSize.Height / 2;
                }
            }
            
            return offset;
        }

      
        protected Point GetAnchorToTopLeft()
        {
            Point p = new Point();
            if (Anchor == Anchor.TopRight || Anchor == Anchor.BottomRight || Anchor == Anchor.Right)
            {
                p.X = -Size.Width;
            }
            else if (Anchor == Anchor.Top || Anchor == Anchor.Bottom || Anchor == Anchor.Center)
            {
                p.X = -Size.Width/2;
            }
            if (Anchor == Anchor.BottomLeft || Anchor == Anchor.BottomRight || Anchor == Anchor.Bottom)
            {
                p.Y = -Size.Height;
            }
            else if (Anchor == Anchor.Left || Anchor == Anchor.Right || Anchor == Anchor.Center)
            {
                p.Y = -Size.Height/2;
            }
            return p;
        }

        [BrowsableAttribute(false)]
        public virtual Rectangle DrawRect
        {
            get
            {
                Point p = ABSPosition;
                return new Rectangle((int) (p.X), (int) (p.Y), (int) (Size.Width), (int) (Size.Height));
            }
        }

        public virtual void ConsistensyCheck(){}


        [BrowsableAttribute(false)] 
        public bool IsBottomHalf;

        [BrowsableAttribute(false)] 
        public bool IsHalfTexture;

        [BrowsableAttribute(false)]
        public virtual BlendState BlendState
        {
            get { return BlendState.BlendStates[BlendMode.Alpha]; }
        }

        [BrowsableAttribute(false)]
        public virtual Rectangle SelectRect
        {
            get { return DrawRect; }
        }

        [BrowsableAttribute(false)]
        public Point Position { get; set; }
        [BrowsableAttribute(false)]
        public virtual Point ABSPosition
        {
            get
            {
                if (Parent == null)
                    return Position;
                Point p = Parent.ABSPosition;
                p.X += Position.X;
                p.Y += Position.Y;
                return p;
            }
        }

        [BrowsableAttribute(false)]
        public virtual Color Color { get { return Color.White; } }
        [BrowsableAttribute(false)]
        public virtual bool IsTiled { get { return false; } }

        [BrowsableAttribute(false)]
        public virtual Rectangle ClipRect
        {
            get
            {
                if (Parent == null)
                    return DrawRect;
                return Parent.DrawRect;
            }
        }

        [BrowsableAttribute(false)]
        public abstract Texture2D Texture { get; }
        [BrowsableAttribute(false)]
        public abstract bool DrawTexture { get; }




        public void Move(int x, int y)
        {
            Position = new Point(Position.X + x, Position.Y + y);
        }

        public virtual void MoveTo(int x, int y, int width, int height)
        {
            Point p = GetParentPosition();
            Position = new Point(x - p.X, y - p.Y);
            Size = new Size(width, height);
        }

        

        public void Resize(int w, int h, string resizeAt)
        {
            /*TL,
            TR,
            BL,
            BR,
            T,
            B,
            L,
            R,*/

            //For X
            Point p = Position;
            Size s = Size;
            if (resizeAt == "TL" || resizeAt == "L" || resizeAt == "BL")
            {
                p.X += w;
                if (anchor.IsMiddleX())
                    s.Width -= 2 * w;
                else
                    s.Width -= w;

            }
            else if (resizeAt == "TR" || resizeAt == "R" || resizeAt == "BR")
            {
                if (anchor.IsMiddleX())
                {
                    p.X -= w;
                    s.Width += 2*w;
                }
                else
                    s.Width += w;
            }

            //For Y
            if (resizeAt == "TL" || resizeAt == "T" || resizeAt == "TR")
            {
                p.Y += h;
                if (anchor.IsMiddleY())
                    s.Height -= 2 * h;
                else
                    s.Height -= h;

            }
            else if (resizeAt == "BL" || resizeAt == "B" || resizeAt == "BR")
            {
                if (anchor.IsMiddleY())
                {
                    p.Y -= h;
                    s.Height += 2 * h;
                }
                else
                    s.Height += h;
            }

            Position = p;
            Size = s;
        }

        protected abstract string TypeString { get; }
        public override string ToString()
        {
            return Name + " (" + TypeString + ")";
        }

        public delegate void ParentSizeChangedDelegate(Size oldSize, Size newSize);
        [field:NonSerialized]
        public event ParentSizeChangedDelegate ParentSizeChangedEvent;
        public virtual void ParentSizeChanged(Size oldSize, Size newSize)
        {
            Point p = Position;
            if (Anchor.IsMiddleX())
            {
                p.X += (Size.Width - oldSize.Width) / 2 - (Size.Width - newSize.Width) / 2;
            }
            else if (Anchor.IsRight())
            {
                p.X += newSize.Width - oldSize.Width;
            }
            if (Anchor.IsMiddleY())
            {
                p.Y += (Size.Height - oldSize.Height) / 2 - (Size.Height - newSize.Height) / 2;
            }
            else if (Anchor.IsBottom())
            {
                p.Y += newSize.Height - oldSize.Height;
            }
            Position = p;
            LastParentSize = newSize;
        }



        //Exposed properties
        [DescriptionAttribute("Specifies the order controls are rendered.\nSpecify a lower value to move this control behind others."),
        CategoryAttribute("General Settings"),
        DefaultValue(512)]
        public virtual int RenderPriority { get; set; }
        [DescriptionAttribute("The offset from the anchor point.\nChange it to move the control."),
        CategoryAttribute("General Settings"),
        DefaultValue(typeof(Point), "0; 0")]
        public virtual Point Offset
        {
            get
            {
                Point p = Position;
                if (Anchor.IsMiddleX())
                {
                    p.X += (Size.Width - GetParentSize().Width) / 2;

                }
                else if (Anchor.IsRight())
                {
                    p.X += Size.Width - GetParentSize().Width;
                    p.X = -p.X;
                }
                if (Anchor.IsMiddleY())
                {
                    p.Y += (Size.Height - GetParentSize().Height) / 2;

                }
                else if (Anchor.IsBottom())
                {
                    p.Y += Size.Height - GetParentSize().Height;
                    p.Y = -p.Y;
                }
                return p;
            }
            set
            {
                Point p = value;
                if (Anchor.IsMiddleX())
                {
                    p.X -= (Size.Width - GetParentSize().Width) / 2;

                }
                else if (Anchor.IsRight())
                {
                    p.X = -p.X;
                    p.X -= Size.Width - GetParentSize().Width;
                }
                if (Anchor.IsMiddleY())
                {
                    p.Y -= (Size.Height - GetParentSize().Height) / 2;

                }
                else if (Anchor.IsBottom())
                {
                    p.Y = -p.Y;
                    p.Y -= Size.Height - GetParentSize().Height;
                }
                Position = p;
            }
        }

        private Size size;
        [DescriptionAttribute("The size of the control."),
        CategoryAttribute("General Settings")]
        public Size Size
        {
            get { return size; }
            set
            {
                if (ParentSizeChangedEvent != null)
                    ParentSizeChangedEvent(size, value);
                size = value;
            }
        }
        
        private Anchor anchor;
        [DescriptionAttribute("The anchor point of the control.\nFor example, set it to Center, and the control will be offset from the center of the parent."),
        CategoryAttribute("General Settings"),
        DefaultValue(typeof(Anchor), "TopLeft")]
        public virtual Anchor Anchor
        {
            get { return anchor; }
            set
            {
                //Calculate offset to top left
                //Offset = FromTopLeft(ToTopLeft(Offset, anchor), value);
                anchor = value;
            }
        }
        [DescriptionAttribute("Specifies what kind of texture are used. If the texture looks wierd, this is probably set wrong."),
        CategoryAttribute("Dialog Control Settings")]
        public virtual ImageType ImageType { get; set; }
        [DescriptionAttribute("The name the variable for this control will have in the code."),
        CategoryAttribute("General Settings")]
        public string Name { get; set; }
        [DescriptionAttribute("If set to false, the control will not be visible."),
        CategoryAttribute("General Settings")]
        public bool Visible { get; set; }

        //Events
        [BrowsableAttribute(false)]
        public List<KeyValuePair<string, string>> Events
        {
            get
            {
                List<KeyValuePair<string, string>> returner = new List<KeyValuePair<string, string>>();
                if (!string.IsNullOrEmpty(OnClicked))
                {
                    returner.Add(new KeyValuePair<string, string>("OnClicked", OnClicked));
                }
                if (!string.IsNullOrEmpty(OnAnyEvent))
                {
                    returner.Add(new KeyValuePair<string, string>("OnAnyEvent", OnAnyEvent));
                }
                if (!string.IsNullOrEmpty(OnChecked))
                {
                    returner.Add(new KeyValuePair<string, string>("OnChecked", OnChecked));
                }
                if (!string.IsNullOrEmpty(OnMouseEnter))
                {
                    returner.Add(new KeyValuePair<string, string>("OnMouseEnter", OnMouseEnter));
                }
                if (!string.IsNullOrEmpty(OnMouseExit))
                {
                    returner.Add(new KeyValuePair<string, string>("OnMouseExit", OnMouseExit));
                }
                if (!string.IsNullOrEmpty(OnTextChanged))
                {
                    returner.Add(new KeyValuePair<string, string>("OnTextChanged", OnTextChanged));
                }
                if (!string.IsNullOrEmpty(OnValueChanged))
                {
                    returner.Add(new KeyValuePair<string, string>("OnValueChanged", OnValueChanged));
                }
                if (!string.IsNullOrEmpty(OnSelectionChanged))
                {
                    returner.Add(new KeyValuePair<string, string>("OnSelectionChanged", OnSelectionChanged));
                }
                if (!string.IsNullOrEmpty(OnSelectionDoubleClicked))
                {
                    returner.Add(new KeyValuePair<string, string>("OnSelectionDoubleClicked", OnSelectionDoubleClicked));
                }
                return returner;
            }
        }

        [DescriptionAttribute("Occurs when the control is clicked."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnClicked { get; set; }
        [DescriptionAttribute("Occurs when any of the other events occur."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnAnyEvent { get; set; }
        [DescriptionAttribute("Occurs when the control is checked. (Only relevant for checkboxes)"),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnChecked { get; set; }
        [DescriptionAttribute("Occurs when the mouse is moved into control."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnMouseEnter { get; set; }
        [DescriptionAttribute("Occurs when the mouse is moved out of control."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnMouseExit { get; set; }
        [DescriptionAttribute("Occurs when the text of the control is changed."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnTextChanged { get; set; }
        [DescriptionAttribute("Occurs when the value of the control is changed.\nFor instance scroll bar position."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnValueChanged { get; set; }
        [//DescriptionAttribute("Occurs when the mouse is moved into control."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnSelectionChanged { get; set; }
        [//DescriptionAttribute("Occurs when the mouse is moved into control."),
        Category("Events"),
        BrowsableAttribute(false),
        TypeConverter(typeof(EventTypeConverter))]
        public string OnSelectionDoubleClicked { get; set; }
    }
}
