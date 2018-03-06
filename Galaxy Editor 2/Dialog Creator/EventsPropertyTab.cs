using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using Galaxy_Editor_2.Dialog_Creator.Controls;

namespace Galaxy_Editor_2.Dialog_Creator
{
    class EventsPropertyTab : PropertyTab
    {
        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            //if (component is Dialog)
            //    return new PropertyDescriptorCollection(new PropertyDescriptor[0]);


            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(component);

            List<PropertyDescriptor> propList = new List<PropertyDescriptor>();
            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].Category != "Events")
                    continue;
                // Create a new PropertyDescriptor from the old one, with 
                // a CategoryAttribute matching the name of the type.
                propList.Add(TypeDescriptor.CreateProperty(props[i].ComponentType, props[i], new CategoryAttribute(props[i].Category)));
            }
            return new PropertyDescriptorCollection(propList.ToArray());
        }

        public override string TabName
        {
            get { return "Events"; }
        }

        // Provides an image for the property tab.
        public override Bitmap Bitmap
        {
            get { return Properties.Resources.events; }
        }
    }
}
