using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;

namespace Galaxy_Editor_2.Dialog_Creator
{
    class PropertiesPropertyTab : PropertyTab
    {
        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            PropertyDescriptorCollection props;
            /*if (attributes == null)
                props = TypeDescriptor.GetProperties(component);
            else*/
                props = TypeDescriptor.GetProperties(component, attributes);

            PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];
            for (int i = 0; i < props.Count; i++)
            {
                // Create a new PropertyDescriptor from the old one, with 
                // a CategoryAttribute matching the name of the type.
                propArray[i] = TypeDescriptor.CreateProperty(props[i].ComponentType, props[i], new CategoryAttribute(props[i].PropertyType.Name));
            }
            return new PropertyDescriptorCollection(propArray);
        }

        public override string TabName
        {
            get { return "Properties2"; }
        }

        // Provides an image for the property tab.
        public override Bitmap Bitmap
        {
            get { return Properties.Resources.properties; }
        }
    }
}
