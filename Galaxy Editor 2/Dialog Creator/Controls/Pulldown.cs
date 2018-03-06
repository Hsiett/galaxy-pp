using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Aga.Controls;
using Galaxy_Editor_2.Dialog_Creator.Complex_properties;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Point = Microsoft.Xna.Framework.Point;

namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class Pulldown : DialogControl
    {
        public Pulldown(GraphicsControl context, Dialog parent, DialogData data) : base(context, parent, "pulldown", data)
        {
            Images[(int)Race.Terran] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normalpressed_terran.dds");
            Images[(int)Race.Protoss] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normalpressed_protoss.dds");
            Images[(int)Race.Zerg] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normalpressed_zerg.dds");
            HoverImages[(int)Race.Terran] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normaloverpressedover_terran.dds");
            HoverImages[(int)Race.Protoss] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normaloverpressedover_protoss.dds");
            HoverImages[(int)Race.Zerg] = new SingleTextureProperty(@"Assets\Textures\ui_glue_dropdownbutton_normaloverpressedover_zerg.dds");
            ImageType = ImageType.EndCap;
            IsHalfTexture = true;
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypePulldown);");

            PrintBaseInit(builder, new Pulldown(Context, null, Data));

            
        }

        public override List<AbstractControl> ExtraControlsToRender
        {
            get
            {
                List<AbstractControl> returner = new List<AbstractControl>();
                ImageControl image = new ImageControl(Context, Parent, Data);
                image.Size = new Size(20, 20);
                image.Position = new Point(Position.X + Size.Width - 44, Position.Y + (Size.Height - image.Size.Height)/2);
                switch (Context.DisplayRace)
                {
                    case Race.Terran:
                        image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_dropdownarrow_normalpressed_terran.dds", Context);
                        break;
                    case Race.Protoss:
                        image.Image = new SingleTextureProperty("Assets\\Textures\\ui_battlenet_glue_dropdownarrow_normalpressed.dds", Context);
                        break;
                    case Race.Zerg:
                        image.Image = new SingleTextureProperty("Assets\\Textures\\ui_glue_dropdownarrow_normalpressed_zerg.dds", Context);
                        break;
                }
                image.IsHalfTexture = true;
                returner.Add(image);
                if (SelectionIndex > 0 && Items != null && SelectionIndex <= Items.Length)
                {
                    Label label = new Label(Context, Parent, Data);
                    label.Position = new Point(Position.X + 20, Position.Y);
                    
                    label.Size = Size;
                    label.Text = Items[SelectionIndex - 1];
                    
                    switch (Context.DisplayRace)
                    {
                        case Race.Terran:
                            label.TextStyle = FontParser.Fonts["StandardPulldown_Terr"]; 
                            break;
                        case Race.Protoss:
                            label.TextStyle = FontParser.Fonts["StandardPulldown_Prot"];
                            break;
                        case Race.Zerg:
                            label.TextStyle = FontParser.Fonts["StandardPulldown_Zerg"];
                            break;
                    }
                    returner.Add(label);
                }
                return returner;
            }
        }

        protected override DialogControl defaultControl
        {
            get { return new Pulldown(Context, Parent, Data); }
        }

        public override bool DrawTexture
        {
            get { return true; }
        }

        protected override string TypeString
        {
            get { return "Pulldown"; }
        }

        public override bool DrawText
        {
            get { return false; }
        }

    }
}
