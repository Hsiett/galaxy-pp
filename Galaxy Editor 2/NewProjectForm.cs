using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public partial class NewProjectForm : AutoSizeForm
    {
        private string[] takenNames;
        private string defaultDir;

        public string ProjectName
        {
            get { return TBName.Text; }
        }

        public string Directory
        {
            get { return TBDirectory.Text; }
        }

        public NewProjectForm(string[] takenNames, string defaultDir, bool saveAs = false, bool showPath = true)
        {
            this.takenNames = takenNames;
            this.defaultDir = defaultDir;
            InitializeComponent();
            TBDirectory.Text = defaultDir;
            if (saveAs)
                Text = "Save Project As";
            groupBox1.Enabled = showPath;
        }

        private void BTNSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            TBDirectory.Text = dialog.SelectedPath;
        }

        private void BTNResetFolder_Click(object sender, EventArgs e)
        {
            TBDirectory.Text = defaultDir;
        }

        private void BTNOK_Click(object sender, EventArgs e)
        {
            if (takenNames.Contains(TBName.Text.ToLower()))
            {
                MessageBox.Show(this, "That name is already taken.", "Error");
                return;
            }

            //Test valid path
            {
                if (ProjectName.Trim() == "")
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
                DirectoryInfo t;
                try
                {
                    t = new DirectoryInfo(Path.Combine(Directory, ProjectName));
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
                if (t.Parent.FullName.Trim('\\', '/') != Directory.Trim('\\', '/'))
                {
                    MessageBox.Show(this, "Invalid project name.", "Error");
                    return;
                }
            }


            DialogResult = DialogResult.OK;
        }


    }
}
