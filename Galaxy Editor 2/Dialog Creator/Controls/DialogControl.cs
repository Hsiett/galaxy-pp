using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Aga.Controls;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    abstract class DialogControl : AbstractControl
    {
        public DialogControl(GraphicsControl context, Dialog parent, string baseName, DialogData data)
            : base(context, parent, baseName, data)
        {
            RenderPriority = 512;
            Enabled = true;
            FullDialog = false;
            Visible = true;
            Text = "";
            TintColors[0] = TintColors[1] = TintColors[2] = Color.White;
            BlendMode = BlendMode.Alpha;
        }

        public override Texture2D Texture
        {
            get { return Image == null ? null : Image.Texture; }
        }

        public override Color Color
        {
            get { return TintColor; }
        }

        public override bool IsTiled
        {
            get { return Tiled; }
        }

       

        [Browsable(false)]
        public abstract bool DrawText { get; }

        [Browsable(false)]
        public virtual Color TextColor { get { return Color.White; } }

        public override void ContextChanged(GraphicsControl context)
        {
            if (context == null)
                return;
            foreach (SingleTextureProperty image in Images)
            {
                if (image != null)
                    image.ContextChanged(context);
            }
            foreach (SingleTextureProperty image in HoverImages)
            {
                if (image != null)
                    image.ContextChanged(context);
            }
            base.ContextChanged(context);
        }

        public override string VariableDeclaration
        {
            get { return "\tint " + Name + ";"; }
        }

        public override Size DefaultSize
        {
            get { return new Size(200, 50); }
        }

        protected abstract DialogControl defaultControl { get; }

        public override void ConsistensyCheck()
        {
            if (TintColors == null)
            {
                TintColors = new Color[3];
                TintColors[0] = defaultControl.TintColors[0];
                TintColors[1] = defaultControl.TintColors[1];
                TintColors[2] = defaultControl.TintColors[2];
            }
            if (TextStyles == null)
            {
                TextStyles = new FontData[3];
                TextStyles[0] = defaultControl.TextStyles[0];
                TextStyles[1] = defaultControl.TextStyles[1];
                TextStyles[2] = defaultControl.TextStyles[2];
            }
        }

        protected void PrintBaseInit(StringBuilder builder, DialogControl defaultBase)
        {

            

            builder.Append("\t\tDialogControlSetSize(");
            builder.Append(Name);
            builder.Append(", PlayerGroupAll(), ");
            builder.Append(Size.Width);
            builder.Append(", ");
            builder.Append(Size.Height);
            builder.AppendLine(");");

            builder.Append("\t\tDialogControlSetPosition(");
            builder.Append(Name);
            builder.Append(", PlayerGroupAll(), ");
            builder.Append(Anchor.ToSCIIString());
            builder.Append(", ");
            builder.Append(Offset.X);
            builder.Append(", ");
            builder.Append(Offset.Y);
            builder.AppendLine(");");


            if (Enabled != defaultBase.Enabled)
            {
                builder.Append("\t\tDialogControlSetEnabled(");
                builder.Append(Name);
                builder.Append(", PlayerGroupAll(), ");
                builder.Append(Enabled.ToString().ToLower());
                builder.AppendLine(");");
            }
            if (Visible != defaultBase.Visible)
            {
                builder.Append("\t\tDialogControlSetVisible(");
                builder.Append(Name);
                builder.Append(", PlayerGroupAll(), ");
                builder.Append(Visible.ToString().ToLower());
                builder.AppendLine(");");
            }
            if (FullDialog != defaultBase.Visible)
            {
                builder.Append("\t\tDialogControlSetVisible(");
                builder.Append(Name);
                builder.Append(", PlayerGroupAll(), ");
                builder.Append(Visible.ToString().ToLower());
                builder.AppendLine(");");
            }
            if (Text != defaultBase.Text)
            {
                builder.Append("\t\tDialogControlSetPropertyAsText(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyText, PlayerGroupAll(), \"");
                builder.Append(Text);
                builder.AppendLine("\");");
            }
            if (EditText != defaultBase.EditText)
            {
                builder.Append("\t\tDialogControlSetPropertyAsString(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyEditText, PlayerGroupAll(), \"");
                builder.Append(EditText);
                builder.AppendLine("\");");
            }
            if (TintColors[0] != defaultBase.TintColors[0] ||
                TintColors[1] != defaultBase.TintColors[1] ||
                TintColors[2] != defaultBase.TintColors[2])
            {
                if (TintColors[0] == TintColors[1] && TintColors[1] == TintColors[2])
                {
                    //because the parameters for ColorWithAlpha is r,g,b,a in 0~100,
                    // we need to scale by 100
                    builder.Append("\t\tDialogControlSetPropertyAsColor(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyColor, PlayerGroupAll(), ColorWithAlpha(");
                    Vector4 color = TintColors[0].ToVector4() * 100;
                    builder.Append(color.X);
                    builder.Append(", ");
                    builder.Append(color.Y);
                    builder.Append(", ");
                    builder.Append(color.Z);
                    builder.Append(", ");
                    builder.Append(color.W);
                    builder.AppendLine("));");
                }
                else
                {
                    builder.AppendLine("\t\tfor (int i = 0; i < 16; i++)");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tif (PlayerStatus(i) == c_playerStatusActive &&");
                    builder.AppendLine("\t\t\t    PlayerType(i) == c_playerTypeUser)");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine("\t\t\t\tswitch (PlayerRace(i))");
                    builder.AppendLine("\t\t\t\t{");

                    builder.AppendLine("\t\t\t\t\tcase \"Terr\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsColor(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyColor, PlayerGroupSingle(i), ColorWithAlpha(");
                    Vector4 color = TintColors[0].ToVector4()*100;
                    builder.Append(color.X);
                    builder.Append(", ");
                    builder.Append(color.Y);
                    builder.Append(", ");
                    builder.Append(color.Z);
                    builder.Append(", ");
                    builder.Append(color.W);
                    builder.AppendLine("));");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Prot\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsColor(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyColor, PlayerGroupSingle(i), ColorWithAlpha(");
                    color = TintColors[1].ToVector4()*100;
                    builder.Append(color.X);
                    builder.Append(", ");
                    builder.Append(color.Y);
                    builder.Append(", ");
                    builder.Append(color.Z);
                    builder.Append(", ");
                    builder.Append(color.W);
                    builder.AppendLine("));");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Zerg\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsColor(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyColor, PlayerGroupSingle(i), ColorWithAlpha(");
                    color = TintColors[2].ToVector4()*100;
                    builder.Append(color.X);
                    builder.Append(", ");
                    builder.Append(color.Y);
                    builder.Append(", ");
                    builder.Append(color.Z);
                    builder.Append(", ");
                    builder.Append(color.W);
                    builder.AppendLine("));");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine("\t\t\t}");
                    builder.AppendLine("\t\t}");
                }
            }

            if (ImagePaths[0] != defaultBase.ImagePaths[0] ||
                ImagePaths[1] != defaultBase.ImagePaths[1] ||
                ImagePaths[2] != defaultBase.ImagePaths[2])
            {
                if (ImagePaths[0] == ImagePaths[1] && ImagePaths[1] == ImagePaths[2])
                {
                    builder.Append("\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyImage, PlayerGroupAll(), \"");
                    builder.Append(_imagePath2ScriptPath(ImagePaths[0].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                }
                else
                {
                    builder.AppendLine("\t\tfor (int i = 0; i < 16; i++)");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tif (PlayerStatus(i) == c_playerStatusActive &&");
                    builder.AppendLine("\t\t\t    PlayerType(i) == c_playerTypeUser)");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine("\t\t\t\tswitch (PlayerRace(i))");
                    builder.AppendLine("\t\t\t\t{");

                    builder.AppendLine("\t\t\t\t\tcase \"Terr\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(ImagePaths[0].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Prot\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(ImagePaths[1].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Zerg\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(ImagePaths[2].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine("\t\t\t}");
                    builder.AppendLine("\t\t}");
                }
            }

            /*if (TextStyle != null &&
                (defaultBase.TextStyle == null ||
                TextStyle.Name != defaultBase.TextStyle.Name))
            {
                builder.Append("\t\tDialogControlSetPropertyAsString(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyStyle, PlayerGroupAll(), \"");
                builder.Append(TextStyle.Name.Replace("\\", "\\\\"));
                builder.AppendLine("\");");
            }*/
            if ((TextStyles[0] != null &&
                (defaultBase.TextStyles[0] == null ||
                TextStyles[0].Name != defaultBase.TextStyles[0].Name)) ||

                (TextStyles[1] != null &&
                (defaultBase.TextStyles[1] == null ||
                TextStyles[1].Name != defaultBase.TextStyles[1].Name)) ||

                (TextStyles[1] != null &&
                (defaultBase.TextStyles[1] == null ||
                TextStyles[1].Name != defaultBase.TextStyles[1].Name)))
            {
                if (TextStyles[0] == TextStyles[1] && TextStyles[1] == TextStyles[2])
                {
                    builder.Append("\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyStyle, PlayerGroupAll(), \"");
                    builder.Append(TextStyles[0].Name.Replace("\\", "\\\\"));
                    builder.AppendLine("\");");
                }
                else
                {
                    builder.AppendLine("\t\tfor (int i = 0; i < 16; i++)");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tif (PlayerStatus(i) == c_playerStatusActive &&");
                    builder.AppendLine("\t\t\t    PlayerType(i) == c_playerTypeUser)");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine("\t\t\t\tswitch (PlayerRace(i))");
                    builder.AppendLine("\t\t\t\t{");

                    if (TextStyles[0] != null)
                    {
                        builder.AppendLine("\t\t\t\t\tcase \"Terr\":");
                        builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                        builder.Append(Name);
                        builder.Append(", c_triggerControlPropertyStyle, PlayerGroupSingle(i), \"");
                        builder.Append(TextStyles[0].Name.Replace("\\", "\\\\"));
                        builder.AppendLine("\");");
                        builder.AppendLine("\t\t\t\t\t\tbreak;");
                    }
                    if (TextStyles[1] != null)
                    {
                        builder.AppendLine("\t\t\t\t\tcase \"Prot\":");
                        builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                        builder.Append(Name);
                        builder.Append(", c_triggerControlPropertyImage, PlayerGroupSingle(i), \"");
                        builder.Append(TextStyles[1].Name.Replace("\\", "\\\\"));
                        builder.AppendLine("\");");
                        builder.AppendLine("\t\t\t\t\t\tbreak;");
                    }
                    if (TextStyles[2] != null)
                    {
                        builder.AppendLine("\t\t\t\t\tcase \"Zerg\":");
                        builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                        builder.Append(Name);
                        builder.Append(", c_triggerControlPropertyImage, PlayerGroupSingle(i), \"");
                        builder.Append(TextStyles[2].Name.Replace("\\", "\\\\"));
                        builder.AppendLine("\");");
                        builder.AppendLine("\t\t\t\t\t\tbreak;");
                    }
                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine("\t\t\t}");
                    builder.AppendLine("\t\t}");
                }
            }

            if (Tiled != defaultBase.Tiled)
            {
                builder.Append("\t\tDialogControlSetPropertyAsBool(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyTiled, PlayerGroupAll(), ");
                builder.Append(Tiled.ToString().ToLower());
                builder.AppendLine(");");
            }
            /*if (Tiled != defaultBase.Tiled)
            {
                builder.Append("\t\tDialogControlSetPropertyAsBool(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyTiled, PlayerGroupAll(), ");
                builder.Append(Tiled.ToString().ToLower());
                builder.AppendLine(");");
            }*/
            if (Checked != defaultBase.Checked)
            {
                builder.Append("\t\tDialogControlSetPropertyAsBool(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyChecked, PlayerGroupAll(), ");
                builder.Append(Checked.ToString().ToLower());
                builder.AppendLine(");");
            }
            if (ToolTip != defaultBase.ToolTip)
            {
                builder.Append("\t\tDialogControlSetPropertyAsText(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyTooltip, PlayerGroupAll(), \"");
                builder.Append(ToolTip.Replace("\\", "\\\\"));
                builder.AppendLine("\");");
            }
            if (ImageType != defaultBase.ImageType)
            {
                builder.Append("\t\tDialogControlSetPropertyAsInt(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyImageType, PlayerGroupAll(), ");
                builder.Append(ImageType.ToSCIIString());
                builder.AppendLine(");");
            }
            if (BlendMode != defaultBase.BlendMode)
            {
                builder.Append("\t\tlibNtve_gf_SetDialogItemBlendMode(");
                builder.Append(Name);
                builder.Append(", ");
                builder.Append(BlendMode.ToSCIIString());
                builder.AppendLine(", PlayerGroupAll());");
            }
            if (HoverImagePaths[0] != defaultBase.HoverImagePaths[0] ||
                HoverImagePaths[1] != defaultBase.HoverImagePaths[1] ||
                HoverImagePaths[2] != defaultBase.HoverImagePaths[2])
            {
                if (HoverImagePaths[0] == HoverImagePaths[1] && HoverImagePaths[1] == HoverImagePaths[2])
                {
                    builder.Append("\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyHoverImage, PlayerGroupAll(), \"");
                    builder.Append(_imagePath2ScriptPath(HoverImagePaths[0].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                }
                else
                {
                    builder.AppendLine("\t\tfor (int i = 0; i < 16; i++)");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tif (PlayerStatus(i) == c_playerStatusActive &&");
                    builder.AppendLine("\t\t\t    PlayerType(i) == c_playerTypeUser)");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine("\t\t\t\tswitch (PlayerRace(i))");
                    builder.AppendLine("\t\t\t\t{");

                    builder.AppendLine("\t\t\t\t\tcase \"Terr\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyHoverImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(HoverImagePaths[0].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Prot\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyHoverImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(HoverImagePaths[1].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t\tcase \"Zerg\":");
                    builder.Append("\t\t\t\t\t\tDialogControlSetPropertyAsString(");
                    builder.Append(Name);
                    builder.Append(", c_triggerControlPropertyHoverImage, PlayerGroupSingle(i), \"");
                    builder.Append(_imagePath2ScriptPath(HoverImagePaths[2].Replace("\\", "\\\\")));
                    builder.AppendLine("\");");
                    builder.AppendLine("\t\t\t\t\t\tbreak;");

                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine("\t\t\t}");
                    builder.AppendLine("\t\t}");
                }
            }
            if (RenderPriority != defaultBase.RenderPriority)
            {
                builder.Append("\t\tDialogControlSetPropertyAsInt(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyRenderPriority, PlayerGroupAll(), ");
                builder.Append(RenderPriority);
                builder.AppendLine(");");
            }
            if (MinValue != defaultBase.MinValue)
            {
                builder.Append("\t\tDialogControlSetPropertyAsFixed(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyMinValue, PlayerGroupAll(), ");
                builder.Append(MinValue.ToString(CultureInfo.InvariantCulture.NumberFormat));
                builder.AppendLine(");");
            }
            if (MaxValue != defaultBase.MaxValue)
            {
                builder.Append("\t\tDialogControlSetPropertyAsFixed(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyMaxValue, PlayerGroupAll(), ");
                builder.Append(MaxValue.ToString(CultureInfo.InvariantCulture.NumberFormat));
                builder.AppendLine(");");
            }
            if (Value != defaultBase.Value)
            {
                builder.Append("\t\tDialogControlSetPropertyAsFixed(");
                builder.Append(Name);
                builder.Append(", c_triggerControlPropertyValue, PlayerGroupAll(), ");
                builder.Append(Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
                builder.AppendLine(");");
            }
            if (Items != null)
                foreach (string item in Items)
                {
                    builder.Append("\t\tDialogControlAddItem(");
                    builder.Append(Name);
                    builder.Append(", PlayerGroupAll(), \"");
                    builder.Append(item);
                    builder.AppendLine("\");");
                }
            if (SelectionIndex != defaultBase.SelectionIndex)
            {
                builder.Append("\t\tDialogControlSelectItem(");
                builder.Append(Name);
                builder.Append(", PlayerGroupAll(), ");
                builder.Append(SelectionIndex);
                builder.AppendLine(");");
            }

            //windywell enable mouse event for all users if events count >0
            if (Events.Count > 0)
            {
                //libNtve_gf_SetDialogItemAcceptMouse(test->bounder,true,PlayerGroupAll());
                builder.Append("\t\tlibNtve_gf_SetDialogItemAcceptMouse(");
                builder.Append(Name);
                builder.AppendLine(",true,PlayerGroupAll());");

                //to avoid the calling of the following event state chane
                //builder.AppendLine("\t\tWait(0.1,c_timeReal);");
            }

            foreach (KeyValuePair<string, string> pair in Events)
            {
                //to avoid the calling of the following event state change, using InvokeAsync<>() 
                builder.Append("\t\tInvokeAsync<this->AddEventDialogControl>(");
                builder.Append(Data.DialogIdentiferName);
                builder.Append("_");
                builder.Append(Name);
                builder.Append("_");
                builder.Append(pair.Key);
                builder.Append(", c_playerAny, ");
                builder.Append(Name);
                switch (pair.Key)
                {
                    case "OnClicked":
                        builder.AppendLine(", c_triggerControlEventTypeClick);");
                        break;
                    case "OnAnyEvent":
                        builder.AppendLine(", c_triggerControlEventTypeAny);");
                        break;
                    case "OnChecked":
                        builder.AppendLine(", c_triggerControlEventTypeChecked);");
                        break;
                    case "OnMouseEnter":
                        builder.AppendLine(", c_triggerControlEventTypeMouseEnter);");
                        break;
                    case "OnMouseExit":
                        builder.AppendLine(", c_triggerControlEventTypeMouseExit);");
                        break;
                    case "OnTextChanged":
                        builder.AppendLine(", c_triggerControlEventTypeTextChanged);");
                        break;
                    case "OnValueChanged":
                        builder.AppendLine(", c_triggerControlEventTypeValueChanged);");
                        break;
                    case "OnSelectionChanged":
                        builder.AppendLine(", c_triggerControlEventTypeSelectionChanged);");
                        break;
                    case "OnSelectionDoubleClicked":
                        builder.AppendLine(", c_triggerControlEventTypeSelectionDoubleClicked);");
                        break;
                }
            }
        }

        //Missing Channel, Relative, Rotation, ItemCount, Offscreen, Achivement, ClickOnDown, Desaturated, 
        //TextWriteout, RelativeAnchor, DesaturationColor, TextWriteoutDuration
        [Description("Sets for example how full a progress bar should be."),
        Category("Dialog Control Settings")]
        public float Value { get; set; }
        [Description("Sets the minimum value for a dialog control."),
        Category("Dialog Control Settings")]
        public float MinValue { get; set; }
        [Description("Sets the maximum value for a dialog control."),
        Category("Dialog Control Settings")]
        public float MaxValue { get; set; } 
        [Description("Sets the blend mode for a dialog control."),
        Category("Dialog Control Settings")]
        public BlendMode BlendMode { get; set; } 
        [Description("Sets the text that appears within the dialog control."),
        Category("Dialog Control Settings")]
        public string Text { get; set; }
        [Description("Sets the string edit value for a dialog control."),
        Category("Dialog Control Settings")]
        public string EditText { get; set; }
        [Description("Sets the font style for the text of a dialog control."),
        Category("Dialog Control Settings"),
        EditorAttribute(typeof(StyleUITypeEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public FontData TextStyle
        {
            get { return TextStyles[(int)Context.DisplayRace]; }
            set
            {
                if (Context.EditDisplayRaceOnly)
                    TextStyles[(int)Context.DisplayRace] = value;
                else
                    TextStyles[0] =
                        TextStyles[1] =
                        TextStyles[2] = value;
            }
        }

        protected FontData[] TextStyles = new FontData[3];
        [Description("Sets the text that appears when mousing over a dialog control."),
        Category("Dialog Control Settings")]
        public string ToolTip { get; set; }
        [Description("Enable or disable a dialog item. A disabled dialog item is greyed out, and cannot be used."),
        Category("Dialog Control Settings")]
        public bool Enabled { get; set; }
        [Description("When set to true, the dialog item will ignore any other set size and position and instead always take up the full size and position of its parent."),
        Category("Dialog Control Settings")]
        public bool FullDialog { get; set; }
        [Description("If true, the image will be repeated rather than stretched."),
        Category("Dialog Control Settings")]
        public bool Tiled { get; set; }
        [Description("Use to specify if a checkbox should be in checked state."),
        Category("Dialog Control Settings")]
        public bool Checked { get; set; }
        [Description("Sets the color of a dialog item in (r,g,b,a) format."),
        Category("Dialog Control Settings"),
        EditorAttribute(typeof(ColorTypeEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        public Color TintColor
        {
            get { return TintColors[(int)Context.DisplayRace]; }
            set
            {
                if (Context.EditDisplayRaceOnly)
                    TintColors[(int)Context.DisplayRace] = value;
                else
                    TintColors[0] =
                        TintColors[1] =
                        TintColors[2] = value;
            }
        }
        protected Color[] TintColors = new Color[3];
        [Description("Sets the image to display on a dialog control. It shows the absolute path if not started with Assets"),
         Category("Dialog Control Settings"),
         EditorAttribute(typeof(SingleTextureUITypeEditor),
             typeof(System.Drawing.Design.UITypeEditor)),
         TypeConverter(typeof(SingleTexturePropertyConverter))]
        public SingleTextureProperty Image
        {
            get { return Images[(int) Context.DisplayRace]; } 
            set
            {
                if (Context.EditDisplayRaceOnly)
                    Images[(int)Context.DisplayRace] = value;
                else
                    Images[0] =
                        Images[1] =
                        Images[2] = value;
            }
        }
        protected SingleTextureProperty[] Images = new SingleTextureProperty[3];
        private string[] ImagePaths
        {
            get
            {
                return new[]
                           {
                               Images[0] == null ? "" : Images[0].Path, 
                               Images[1] == null ? "" : Images[1].Path,
                               Images[2] == null ? "" : Images[2].Path
                           };
            }
        }
        [Description("Sets the hover image to display on a dialog control."),
         Category("Dialog Control Settings"),
         EditorAttribute(typeof(SingleTextureUITypeEditor),
             typeof(System.Drawing.Design.UITypeEditor)),
         TypeConverter(typeof(SingleTexturePropertyConverter))]
        public SingleTextureProperty HoverImage
        {
            get { return HoverImages[(int)Context.DisplayRace]; }
            set
            {
                if (Context.EditDisplayRaceOnly)
                    HoverImages[(int)Context.DisplayRace] = value;
                else
                    HoverImages[0] =
                        HoverImages[1] =
                        HoverImages[2] = value;
            }
        }
        protected SingleTextureProperty[] HoverImages = new SingleTextureProperty[3];
        private string[] HoverImagePaths
        {
            get
            {
                return new[]
                           {
                               HoverImages[0] == null ? "" : HoverImages[0].Path, 
                               HoverImages[1] == null ? "" : HoverImages[1].Path,
                               HoverImages[2] == null ? "" : HoverImages[2].Path
                           };
            }
        }
        [Description("Sets that dialog this control should be attached to."),
        Category("Dialog Control Settings"),
        DefaultValue(""),
        TypeConverter(typeof(ParentDialogUITypeConverter))]
        public Dialog ParentDialog
        {
            get { return Parent; }
            set
            {
                if (Parent != null)
                    Parent.ChildControls.Remove(this);
                Parent = value;
                if (Parent != null)
                    Parent.AddControl(this);
            }
        }

        [DescriptionAttribute("Modify the items displayed in a pulldown/listbox."),
        Category("Dialog Control Settings"),
        TypeConverter(typeof(StringCollectionEditor))]
        public string[] Items { get; set; }
        [Description("Sets the selected index of a pulldown/listbox."),
        Category("Dialog Control Settings")]
        public int SelectionIndex { get; set; }

        //used to fix texture path selected from texture selection mod.
        private string _imagePath2ScriptPath(string imagePath)
        {
            int index = -1;
            if ((index=imagePath.LastIndexOf("Assets\\\\Textures\\\\")) != -1)
            {
                return imagePath.Substring(index);
            }else if ((index = imagePath.LastIndexOf("Assets/Textures/")) != -1)
            {
                return imagePath.Substring(index);
            }
            return imagePath;
        }

        private static Vector4 argb2rgba(Vector4 argb)
        {
            Vector4 rgba;
            rgba.X = argb.Y;
            rgba.Y = argb.Z;
            rgba.Z = argb.W;
            rgba.W = argb.X;
            return rgba;
        }
    }
}
