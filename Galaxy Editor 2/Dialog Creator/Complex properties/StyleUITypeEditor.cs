using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Dialog_Creator.Fonts;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class StyleUITypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            StyleBrowserDialog dialog = new StyleBrowserDialog((FontData) value);
            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.SelectedFont;
            return value;
            
        }
    }
}
