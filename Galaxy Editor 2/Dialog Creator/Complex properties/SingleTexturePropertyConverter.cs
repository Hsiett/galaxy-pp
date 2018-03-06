using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class SingleTexturePropertyConverter : StringConverter   
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(SingleTextureProperty))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               Type destinationType)
        {
            if (destinationType == typeof(String) &&
                 value is SingleTextureProperty)
            {
                SingleTextureProperty textureProperty = (SingleTextureProperty)value;
                return textureProperty.Path;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
         
    }
}
