using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class Dialog : AbstractControl
    {
        //List<KeyValuePair<Texture2D, Rectangle>> textures = new List<KeyValuePair<Texture2D, Rectangle>>();
        [NonSerialized]
        private static Texture2D[] _defaultTexture = new Texture2D[3];
        static void InitTextures(GraphicsControl sender)
        {
            if (sender == null)
                return;
            if (_defaultTexture[0] != null)
                return;
            _defaultTexture[(int)Race.Terran] = TextureLoader.Load("Assets\\Textures\\ui_frame_default_terran.dds", sender.GraphicsDevice);
            _defaultTexture[(int)Race.Protoss] = TextureLoader.Load("Assets\\Textures\\ui_frame_default_protoss.dds", sender.GraphicsDevice);
            _defaultTexture[(int)Race.Zerg] = TextureLoader.Load("Assets\\Textures\\ui_frame_default_zerg.dds", sender.GraphicsDevice);

        }

        private static int Comparerer(IRenderableItem item1, IRenderableItem item2)
        {
            return item1.RenderPriority.CompareTo(item2.RenderPriority);
        }
        public List<DialogControl> ChildControls = new List<DialogControl>();

        public void AddControl(DialogControl ctrl)
        {
            ChildControls.Add(ctrl);
            ResortChildren();
        }
        public void RemoveControl(DialogControl ctrl)
        {
            ChildControls.Remove(ctrl);
            ResortChildren();
        }

        public void ResortChildren()
        {
            ChildControls.Sort(Comparerer);
            ChildControls.Sort(Comparerer);
        }

        public override string VariableDeclaration
        {
            get { return "\tint " + Name + ";"; }
        }

        public override Size DefaultSize
        {
            get { return new Size(500, 400); }
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            //myDialog = DialogCreate(width, height, anchor, x, y, modal)
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogCreate(");
            builder.Append(Size.Width);
            builder.Append(", ");
            builder.Append(Size.Height);
            builder.Append(", ");
            builder.Append(Anchor.ToSCIIString());
            builder.Append(", ");
            builder.Append(Offset.X);
            builder.Append(", ");
            builder.Append(Offset.Y);
            builder.Append(", ");
            builder.Append(Modal.ToString().ToLower());
            builder.AppendLine(");");

            Dialog defaultDialog = new Dialog(null, new Rectangle(0, 0, 100, 100), Data);
            defaultDialog.ContextChanged(Context);

            if (Image != defaultDialog.Image && (defaultDialog.Image == null || Image.Path != defaultDialog.Image.Path))
            {
                //Set texture
                //DialogSetImage(myDialog, "my texture");
                builder.Append("\t\tDialogSetImage(");
                builder.Append(Name);
                builder.Append(", \"");
                builder.Append(_imagePath2ScriptPath(Image.Path.Replace("\\", "\\\\")));
                builder.AppendLine("\");");
            }

            if (Title != defaultDialog.Title)
            {
                //DialogSetTitle(myDialog, "my title");
                builder.Append("\t\tDialogSetTitle(");
                builder.Append(Name);
                builder.Append(", \"");
                builder.Append(Title);
                builder.AppendLine("\");");
            }

            if (Offscreen != defaultDialog.Offscreen)
            {
                //DialogSetOffscreen(myDialog, true);
                builder.Append("\t\tDialogSetOffscreen(");
                builder.Append(Name);
                builder.Append(", \"");
                builder.Append(Offscreen);
                builder.AppendLine("\");");
            }


            if (Fullscreen != defaultDialog.Fullscreen)
            {
                //DialogSetOffscreen(myDialog, true);
                builder.Append("\t\tDialogSetFullscreen(");
                builder.Append(Name);
                builder.Append(", ");
                builder.Append(Fullscreen.ToString().ToLower());
                builder.AppendLine(");");
            }

            if (BackgroundVisible != defaultDialog.BackgroundVisible)
            {
                //DialogSetOffscreen(myDialog, true);
                builder.Append("\t\tDialogSetImageVisible(");
                builder.Append(Name);
                builder.Append(", ");
                builder.Append(BackgroundVisible.ToString().ToLower());
                builder.AppendLine(");");
            }

            if (Transparency != defaultDialog.Transparency)
            {
                //DialogSetOffscreen(myDialog, true);
                builder.Append("\t\tDialogSetTransparency(");
                builder.Append(Name);
                builder.Append(", \"");
                builder.Append(Transparency);
                builder.AppendLine("\");");
            }

            if (Visible != defaultDialog.Visible)
            {
                //this->SetVisible(PlayerGroupAll(), true);
                builder.Append("\t\tthis->SetVisible(PlayerGroupAll(), ");
                builder.Append(Visible.ToString().ToLower());
                builder.AppendLine(");");
            }
        }

        public override void ContextChanged(GraphicsControl context)
        {
            base.ContextChanged(context);
            InitTextures(context);
            if (Image != null)
                Image.ContextChanged(context);
            foreach (DialogControl control in ChildControls)
            {
                control.ContextChanged(context);
            }
        }

        public Dialog(GraphicsControl sender, Rectangle rect, DialogData data) : base(sender, null, "dialog", data)
        {
            //Default values
            base.ImageType = ImageType.Border;
            base.Position = new Point(rect.X, rect.Y);
            base.Size = new Size(rect.Width, rect.Height);
            BackgroundVisible = true;
            base.RenderPriority = 512;
        }

        [BrowsableAttribute(false)]
        public FontData TitleFont
        {
            get { return FontParser.Fonts["ModCenterSize28"]; }
        }

        [BrowsableAttribute(false)]
        public Rectangle TitleFontRect
        {
            get
            {
                Rectangle drawRect = DrawRect;
                return new Rectangle(drawRect.X, drawRect.Y + 30, drawRect.Width, 50);
            }
        }

        [BrowsableAttribute(false)]
        public override ImageType ImageType
        {
            get
            {
                return base.ImageType;
            }
            set
            {
                base.ImageType = value;
            }
        }

        public override Texture2D Texture
        {
            get
            {
                if (Image != null && Image.Texture != null)
                    return Image.Texture;
                return _defaultTexture[(int) Context.DisplayRace];
            }
        }

        public override bool DrawTexture
        {
            get { return BackgroundVisible; }
        }

        public override Rectangle DrawRect
        {
            get
            {
                if (Fullscreen)
                    return new Rectangle(0, 0, (int)(Context.Width), (int) (Context.Height));
                return base.DrawRect;
            }
        }

        public override Rectangle ClipRect
        {
            get { return DrawRect; }
        }

        protected override string TypeString
        {
            get { return "Dialog"; }
        }

        //used to fix texture path selected from texture selection mod.
        private string _imagePath2ScriptPath(string imagePath)
        {
            int index = -1;
            if ((index = imagePath.LastIndexOf("Assets\\\\Textures\\\\")) != -1)
            {
                return imagePath.Substring(index);
            }
            else if ((index = imagePath.LastIndexOf("Assets/Textures/")) != -1)
            {
                return imagePath.Substring(index);
            }
            return imagePath;
        }

        public override Color Color
        {
            get
            {
                return new Color(1f, 1f, 1f, Transparency);
            }
        }

        [BrowsableAttribute(false)]
        public override int RenderPriority
        {
            get { return 512; }
        }

        [DescriptionAttribute("The background image of the dialog."),
        Category("Dialog Settings"),
        EditorAttribute(typeof(SingleTextureUITypeEditor),
        typeof(System.Drawing.Design.UITypeEditor)),
        TypeConverter(typeof(SingleTexturePropertyConverter))]
        public SingleTextureProperty Image { get; set; }

        [DescriptionAttribute("The title is displayed at the top center of the dialog."),
        Category("Dialog Settings"),
        DefaultValue("")]
        public string Title { get; set; }
        [DescriptionAttribute("An offscren dialog is not rendered normally and is instead used in conjuction with the render-to-texture feature."),
        Category("Dialog Settings"),
        DefaultValue(false)]
        public bool Offscreen { get; set; }
        [DescriptionAttribute("Setting a dialog fullscreen will make it ignore any other set position and size and always take up the full screen."),
        Category("Dialog Settings"),
        DefaultValue(false)]
        public bool Fullscreen { get; set; }
        [DescriptionAttribute("Set to false to hide the dialog's background. Dialog items will remain visible."),
        Category("Dialog Settings"),
        DefaultValue(true)]
        public bool BackgroundVisible { get; set; }
        [DescriptionAttribute("Sets the transparency of the specified dialog."),
        Category("Dialog Settings"),
        DefaultValue(1)]
        public float Transparency
        {
            get { return transparency; }
            set
            {
                if (value < 0)
                    transparency = 0;
                else if (value > 1)
                    transparency = 1;
                else
                    transparency = value;
            }
        }
        private float transparency = 1;
        [DescriptionAttribute("A modal dialog should prevent interaction with other dialogs until it is closed.\nNot currently working in SC II."),
        Category("Dialog Settings"),
        DefaultValue(false)]
        public bool Modal { get; set; }
    }


}
