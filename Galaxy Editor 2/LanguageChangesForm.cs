using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public partial class LanguageChangesForm : Form
    {
        public static bool HasChanges(ProgramVersion version)
        {
            return version < new ProgramVersion("2.5.5");
        }

        public LanguageChangesForm(ProgramVersion oldVersion)
        {
            InitializeComponent();

            if (oldVersion < new ProgramVersion("2.5.5"))
            {
                richTextBox1.Text += "\n\n\n";
                richTextBox1.Text += "Version 2.5.5:\n";
                richTextBox1.Text += "* Removed # infront of all the keywords, to make the code more clean.\n";
                richTextBox1.Text += "* The keyword #Invoke is now called InvokeSync\n";
                richTextBox1.Text += "* The keyword #trigger is now called Trigger (note: uppercase)\n";
                richTextBox1.Text += "\n";
                richTextBox1.Text += "Existing code can quickly be modified to fit theese changes, if you use the find and replace feature (ctrl+F)\n";
                richTextBox1.Text += "\n";
                richTextBox1.Text += "I usually try to refrain from changeing the language in a way so that current code can become invalid, ";
                richTextBox1.Text += "but in this case, I have only ever recived complaints about the #'s, and I didn't get any negative feedback when I warned about ";
                richTextBox1.Text += "the change in the project thread on mapster (see the about box). I apologize for the inconvinience this ";
                richTextBox1.Text += "leads to, and hope you agree that the change is for the better.";
            }
        }


        public void Show(Form parent)
        {
            base.Show(parent);
            Location = new Point(parent.Location.X + (parent.Width - Width) / 2,
                parent.Location.Y + (parent.Height - Height) / 2);
        }
    }
}
