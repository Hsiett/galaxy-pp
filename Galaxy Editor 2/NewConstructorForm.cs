using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler.Contents;

namespace Galaxy_Editor_2
{
    internal partial class NewConstructorForm : AutoSizeForm
    {
        private List<VariableDescription> originalList;
        public List<VariableDescription> SelectedOrder = new List<VariableDescription>();

        public NewConstructorForm(List<VariableDescription> vars)
        {
            InitializeComponent();

            originalList = vars;
            int x = 3;
            int y = 3;
            const int spacing = 6;

            for (int i = 0; i < vars.Count; i++)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.AutoSize = true;
                checkBox.Text = vars[i].Type + " " + vars[i].Name;
                checkBox.Location = new Point(x, y);
                y += 17 + spacing;
                checkBox.Tag = i;
                checkBox.CheckedChanged += checkBox_CheckedChanged;
                checkBoxPanel.Controls.Add(checkBox);
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            int index = (int) ((CheckBox) sender).Tag;

            SelectedOrder.Remove(originalList[index]);
            SelectedOrder.Add(originalList[index]);
        }
    }
}
