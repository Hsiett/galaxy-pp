using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
namespace Galaxy_Editor_2.Dialog_Creator.Controls
{
    [Serializable]
    class Label : DialogControl
    {
        public Label(GraphicsControl context, Dialog parent, DialogData data)
            : base(context, parent, "label", data)
        {
            Text = Name;

            TextStyles[(int)Race.Terran] =
                TextStyles[(int)Race.Protoss] =
                TextStyles[(int)Race.Zerg] = FontParser.Fonts["StandardLabel"];
        }

        protected override DialogControl defaultControl
        {
            get { return new Label(Context, Parent, Data) { Text = "" }; }
        }

        public override void PrintInitialization(StringBuilder builder)
        {
            builder.Append("\t\t");
            builder.Append(Name);
            builder.Append(" = DialogControlCreate(");
            builder.Append(Parent.Name);
            builder.AppendLine(", c_triggerControlTypeLabel);");

            PrintBaseInit(builder, new Label(Context, null, Data) {Text = ""});
        }

        public override bool DrawTexture
        {
            get { return false; }
        }

        protected override string TypeString
        {
            get { return "Label"; }
        }

        public override bool DrawText
        {
            get { return true; }
        }

        public override Color TextColor { get { return TintColor; } }
    }
}
