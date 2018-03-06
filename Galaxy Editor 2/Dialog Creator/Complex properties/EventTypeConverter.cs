using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Dialog_Creator.Controls;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    class EventTypeConverter : TypeConverter 
    {

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            DialogControl control = (DialogControl) context.Instance;
            

            StandardValuesCollection ret;

            /*ret =
                new StandardValuesCollection(new object[]
                                                 {
                                                     control.Context.Data.DialogIdentiferName + "_" + control.Name + "_" +
                                                     context.PropertyDescriptor.Name
                                                 });*/
            ret = new StandardValuesCollection(control.Data.GetAllTargetMethods().ToArray());

            return ret;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return context.PropertyDescriptor.PropertyType == typeof(string);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }
    }
}
