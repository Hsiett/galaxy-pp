using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Galaxy_Editor_2.Dialog_Creator.Controls;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class SingleTextureUITypeEditor : UITypeEditor
    {
        
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            GraphicsControl graphicsContext;
            if (context.Instance is AbstractControl)
                graphicsContext = ((AbstractControl) context.Instance).Context;
            else
                throw new Exception("Unable to find context from Type Editor");
            //Dialog.Show();
            //Dialog.InitNewSearch(value is SingleTextureProperty ? ((SingleTextureProperty)value).Path : "");
            TextureBrowserDialog dialog = new TextureBrowserDialog();
            dialog.setupNewSearch(value is SingleTextureProperty ? ((SingleTextureProperty)value).Path : "");
            //DialogResult dr=Dialog.ShowDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return new SingleTextureProperty(dialog.SelectedPath, dialog.SelectedTexture, graphicsContext);
            }
            if (((SingleTextureProperty)value).Path == "")
                return null;
            return value;
            
        }
    }
}
