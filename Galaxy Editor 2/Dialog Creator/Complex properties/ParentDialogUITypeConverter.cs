using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Controls;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class ParentDialogUITypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            GraphicsControl graphicsContext;
            if (context.Instance is AbstractControl)
                graphicsContext = ((AbstractControl)context.Instance).Context;
            else
                throw new Exception("Unable to find context from Type Editor");
            
            List<Dialog> dialogs = new List<Dialog>();
            foreach (AbstractControl item in graphicsContext.Items)
            {
                if (item is Dialog && item != context.Instance)
                {
                    dialogs.Add((Dialog) item);
                }
            }

            return new StandardValuesCollection(dialogs);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                GraphicsControl graphicsContext;
                if (context.Instance is AbstractControl)
                    graphicsContext = ((AbstractControl)context.Instance).Context;
                else
                    throw new Exception("Unable to find context from Type Editor");

                foreach (AbstractControl item in graphicsContext.Items)
                {
                    if (item is Dialog && item != context.Instance && item.ToString() == (string) value)
                    {
                        return item;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
