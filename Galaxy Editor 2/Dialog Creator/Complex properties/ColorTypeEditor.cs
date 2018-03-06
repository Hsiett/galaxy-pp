using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class ColorTypeEditor: UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService service =
                (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
            ColorDropDown dropDownControl = new ColorDropDown();
            dropDownControl.Color = (Color) value;
            service.DropDownControl(dropDownControl);
            return dropDownControl.Color;
        }
    }
}
